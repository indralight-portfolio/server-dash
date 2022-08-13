using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Common.Utility;
using Dash;
using Dash.Model.GamePlay;
using Dash.Model.Rdb;
using NLog;
using Dash.Protocol;
using server_dash.Net.Handlers;
using server_dash.Battle.Services.Entities;
using server_dash.Battle.Services.GamePlay;
using Dash.Server.Dao.Cache;
using server_dash.Execution.Runnable;
using server_dash.Internal.Services;

namespace server_dash.Battle.Services
{
    using Dash.Server.Statistics;
    using Dash.Types;
    using Net.Sessions;
    using server_dash.Protocol;
    using server_dash.Statistics;
    using Utility;

    public class SessionService : AbstractService
    {
        private readonly LogicLogger _logger = new LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());

        private readonly ArenaService _arenaService;
        private readonly ChannelManager _channelManager;
        private readonly SessionManager _sessionManager;
        private readonly ServiceExecuteMultiplexer _serviceExecuteMultiplexer;

        private CheckConfig _checkConfig;
        private readonly SessionValidator _sessionValidator;

        private ISingleDBCache<Account> _accountCache;
        private ISingleDBCache<StaticAccount> _staticAccountCache;
        private IMemCache _memCache;

        public SessionService(ArenaService arenaService, ChannelManager channelManager, SessionManager sessionManager, ServiceExecuteMultiplexer serviceExecuteMultiplexer, SessionValidator sessionValidator)
        {
            _arenaService = arenaService;
            _channelManager = channelManager;
            _sessionManager = sessionManager;
            _serviceExecuteMultiplexer = serviceExecuteMultiplexer;
            _accountCache = DaoCache.Instance.GetSingle<Account>();
            _staticAccountCache = DaoCache.Instance.GetSingle<StaticAccount>();
            _memCache = DaoCache.Instance.GetMemCache();
            _sessionValidator = sessionValidator;

            _checkConfig = ConfigManager.Get<CheckConfig>(Config.Check);
        }

        [BindMessage(ServiceAreaType.Dummy)]
        public void WhoAmI(ISession session, WhoAmI message)
        {
            AsyncTaskWrapper.Call(AuthorizeInternal(session, message));
        }


        private async Task AuthorizeInternal(ISession session, WhoAmI message)
        {
            ulong oidAccount = message.OidAccount;
            string sessionKey = message.SessionKey;
            string version = message.Version;
            string revClient = message.RevClient;
            string revDash = message.RevDash;
            string revData = message.RevData;

            // VersionValidate
            if (_checkConfig.IgnoreVersion == false)
            {
                bool valid = VersionValidator.Validate(version);
                if (valid == false)
                {
                    session.Write(new ReturnToLobby { Reason = ReturnToLobbyReasonType.InvalidVersion });
                    _logger.Warn($"{oidAccount.LogOid()} Version Validate failed, {version}/{revClient}/{revDash}/{revData}");
                    return;
                }
            }
            if (VersionValidator.ValidateRev(revDash, revData) == false)
            {
                _logger.Warn($"{oidAccount.LogOid()} Revision Validate failed, Server : {BuildVersion.RevDash}/{BuildVersion.RevData}, Client : {revDash}/{revData} ");
            }
            // SessionValidate
            if (_checkConfig.IgnoreSession == false)
            {
                bool valid = await _sessionValidator.Validate(oidAccount, sessionKey);
                if (valid == false)
                {
                    session.Write(new ReturnToLobby { Reason = ReturnToLobbyReasonType.InvalidSession });
                    _logger.Warn($"{oidAccount.LogOid()} SessionKey Validate failed, {sessionKey}");
                    return;
                }
            }
            (Account account, StaticAccount staticAccount) = await TaskUtility.WaitAll2(
                _accountCache.Get(oidAccount),
                _staticAccountCache.Get(oidAccount));
            if (account == null || staticAccount == null)
            {
                session.Write(new ReturnToLobby { Reason = ReturnToLobbyReasonType.AccountNotExists });
                _logger.Info($"{oidAccount.LogOid()} Account not exist.");
                return;
            }
            var sessionHandler = session.Channel?.Pipeline.Get<Net.Handlers.SessionHandler>();
            if (sessionHandler == null)
            {
                session.Write(new ReturnToLobby { Reason = ReturnToLobbyReasonType.InternalError });
                _logger.Error(session, $"SessionHandler not found.");
                return;
            }

            var newSession = sessionHandler.CreateGameSession(session.Channel);
            _logger.Info($"[Battle]{oidAccount.LogOid()}[nick:{staticAccount.Nickname}]{newSession.Channel.Log()} login succeed.");
            newSession.OidAccount = oidAccount;
            newSession.Player = new PlayerEntity() { OidAccount = oidAccount, AccountLevel = account.Level, Nickname = staticAccount.Nickname };
            newSession.Version = version;
            newSession.Revisions = message.Revisions;
            _sessionManager.AddSession(newSession.Player.OidAccount, newSession);

            var response = new AuthResponse
            {
                OidAccount = newSession.Player.OidAccount
            };

            newSession.Write(response);
            _channelManager.OnChannelLogin(newSession.Channel, newSession);
        }

        [Coroutine(ServiceAreaType.All)]
        public IEnumerator SessionClosed(CoroutineContext coroutineContext, ISession session, SessionClosed message)
        {

            session.IsAlive = false;
            PlayerSession playerSession = session as PlayerSession;
            if (playerSession == null)
            {
                _logger.Info($"SessionService : SessionClosed But GameSession is null. [{session}]");
                yield break;
            }
            _logger.Info(playerSession, $"[Battle][SessionService] : SessionClosed");
            ArenaContext arenaContext = playerSession.ArenaContext;
            if (arenaContext == null)
            {
                yield break;
            }

            // 아직 끝나지 않은 경우.
            if (arenaContext.Arena.Result == null && arenaContext.CapturedState != null)
            {
                _memCache.StringSet($"undone:{playerSession.Player.OidAccount}", arenaContext.CapturedState.Hash.ToString());
            }

            if (arenaContext.SessionsTrueForAll(s => s.IsAlive == false) == false)
            {
                // 아직 다른 정상 세션이 있을 때.
                int playerId = arenaContext.FindPlayerByOidAccount(playerSession.Player.OidAccount)?.Id ?? 0;
                arenaContext.Broadcast(new OtherSessionClosed() { OidAccount = playerSession.Player.OidAccount }, session as PlayerSession);

                if (arenaContext.Arena.IsStarted == true && arenaContext.Arena.Result == null)
                {
                    // 게임 진행도중 튕겼을 때.
                    arenaContext.Arena.OnSessionNeedDrop(playerSession.OidAccount, arenaContext);
                    yield break;
                }
                else // 게임 시작도 안됐는데 플레이어중 하나가 끊긴 경우
                {
                    // 다른 정상 Session이 있다면 여기까지.
                    if (arenaContext.GetSessions.FirstOrDefault(s => s.IsAlive == true) != null)
                    {

                        yield break;
                    }
                }
            }

            if (arenaContext.EndProcessed == false)
            {
                arenaContext.EndProcessed = true;
                yield return _arenaService.GameEndCoroutine(coroutineContext, playerSession.ArenaContext, EndArenaReasonType.PlayerExit);
            }

            BattleServer.ServiceExecuteMultiplexerInstance.GetExecutor(arenaContext.Arena.Serial).RemoveMessageQueue(arenaContext.Arena.Serial);
            BattleServer.ServiceExecuteMultiplexerInstance.GetExecutor(arenaContext.Arena.Serial).RemoveUpdateCallback(arenaContext.OnUpdate);
            _logger.Info(arenaContext, "######### [SessionService] MessageQueue, UpdateCallback removed #########");

            arenaContext.ReleaseResources();
        }

        [BindMessage(ServiceAreaType.All)]
        public void ChannelActive(ISession session, ChannelActive message)
        {
        }

        [BindMessage(ServiceAreaType.All)]
        public void ChannelInactive(ISession session, ChannelInactive message)
        {
        }

        // From Match Server
        [BindInternalMessage]
        public void KickAll(KickAllSession message)
        {
            foreach (var id in _sessionManager.GetAllSessionIds())
            {
                if (_sessionManager.GetSession(id, out var session) == true)
                {
                    _logger.Info($"[SessionService] Kick, {id.LogOid()}");
                    session.CloseAsync();
                }
            }
        }
    }
}