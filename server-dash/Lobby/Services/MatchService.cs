using Dash.Types;
using server_dash.Internal;
using server_dash.Net.Handlers;
using server_dash.Protocol;
using System.Threading.Tasks;

namespace server_dash.Lobby.Services
{
    public class MatchService : BaseService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private readonly MatchServerClient _matchServerClient;
        private Dash.Protocol.LastEnqueuePlayer _lastEnqueuePlayerInfo;
        private object _lock = new object();
        public MatchService(MatchServerClient matchServerClient)
        {
            _matchServerClient = matchServerClient;
            _matchServerClient.RegisterResponseHandler<GetBattleServerResponse>(GetBattleServerResponse.TypeCode);
            _matchServerClient.RegisterResponseHandler<EnqueueMatchResponse>(EnqueueMatchResponse.TypeCode);
            _matchServerClient.RegisterResponseHandler<DequeueMatchResponse>(DequeueMatchResponse.TypeCode);
            _matchServerClient.RegisterResponseHandler<CheckMatchResponse>(CheckMatchResponse.TypeCode);
            _matchServerClient.RegisterResponseHandler<CheckArenaResponse>(CheckArenaResponse.TypeCode);
            _matchServerClient.RegisterResponseHandler<ForgiveArenaResponse>(ForgiveArenaResponse.TypeCode);

            _matchServerClient.RegisterResponseHandler<GetSocialServerResponse>(GetSocialServerResponse.TypeCode);
            _matchServerClient.RegisterResponseHandler<GetPartyConnectInfoResponse>(GetPartyConnectInfoResponse.TypeCode);
        }

        public async Task<Dash.Protocol.GetBattleServerResponse> GetBattleServer()
        {
            var request = new GetBattleServerRequest();
            var protocol = await _matchServerClient.ResponseMessageDispatch(request);
            if (protocol == null)
            {
                var httpResponse = new Dash.Protocol.GetBattleServerResponse();
                httpResponse.SetResult(ErrorCode.InternalError);
                return httpResponse;
            }
            else
            {
                var response = protocol as GetBattleServerResponse;
                return response.HttpResponse;
            }
        }

        public async Task<Dash.Protocol.EnqueueMatchResponse> EnqueueMatch(ulong oidAccount, int chapterId, string nickname)
        {
            var request = new EnqueueMatchRequest
            {
                OidAccount = oidAccount,
                ChapterId = chapterId,
                Nickname = nickname,
            };
            var protocol = await _matchServerClient.ResponseMessageDispatch(request);
            if (protocol == null)
            {
                var httpResponse = new Dash.Protocol.EnqueueMatchResponse();
                httpResponse.SetResult(ErrorCode.InternalError);
                return httpResponse;
            }
            else
            {
                lock(_lock)
                {
                    if(_lastEnqueuePlayerInfo == null)
                    {
                        _lastEnqueuePlayerInfo = new Dash.Protocol.LastEnqueuePlayer();
                    }
                    _lastEnqueuePlayerInfo.ChapterId = chapterId;
                    _lastEnqueuePlayerInfo.Nickname = nickname;
                    _lastEnqueuePlayerInfo.UpdateTime = System.DateTime.UtcNow;
                }
                var response = protocol as EnqueueMatchResponse;
                return response.HttpResponse;
            }
        }
        public async Task<Dash.Protocol.LastEnqueuePlayerInfoResponse> GetLastEnqueuePlayerInfo()
        {
            Dash.Protocol.LastEnqueuePlayerInfoResponse response = new Dash.Protocol.LastEnqueuePlayerInfoResponse();
            lock (_lock)
            {
                if (_lastEnqueuePlayerInfo != null && (System.DateTime.UtcNow - _lastEnqueuePlayerInfo.UpdateTime).TotalSeconds > 10)
                {
                    _lastEnqueuePlayerInfo.ChapterId = 0;
                    _lastEnqueuePlayerInfo.Nickname = "";
                }
                response.Model = _lastEnqueuePlayerInfo;
            }
            return response;
        }

        public async Task<Dash.Protocol.DequeueMatchResponse> DequeueMatch(ulong oidAccount)
        {
            var request = new DequeueMatchRequest
            {
                OidAccount = oidAccount,
            };
            var protocol = await _matchServerClient.ResponseMessageDispatch(request);
            if (protocol == null)
            {
                var httpResponse = new Dash.Protocol.DequeueMatchResponse();
                httpResponse.SetResult(ErrorCode.InternalError);
                return httpResponse;
            }
            else
            {
                var response = protocol as DequeueMatchResponse;
                return response.HttpResponse;
            }
        }

        public async Task<Dash.Protocol.CheckMatchResponse> CheckMatch(ulong oidAccount)
        {
            var request = new CheckMatchRequest
            {
                OidAccount = oidAccount,
            };
            var protocol = await _matchServerClient.ResponseMessageDispatch(request);
            if (protocol == null)
            {
                var httpResponse = new Dash.Protocol.CheckMatchResponse();
                httpResponse.SetResult(ErrorCode.InternalError);
                return httpResponse;
            }
            else
            {
                var response = protocol as CheckMatchResponse;
                return response.HttpResponse;
            }
        }

        public async Task<Dash.Protocol.CheckArenaResponse> CheckArena(ulong oidAccount)
        {
            var request = new CheckArenaRequest
            {
                OidAccount = oidAccount,
            };
            var protocol = await _matchServerClient.ResponseMessageDispatch(request);
            if (protocol == null)
            {
                var httpResponse = new Dash.Protocol.CheckArenaResponse();
                httpResponse.SetResult(ErrorCode.InternalError);
                return httpResponse;
            }
            else
            {
                var response = protocol as CheckArenaResponse;
                return response.HttpResponse;
            }
        }

        public async Task<Dash.Protocol.ForgiveArenaResponse> ForgiveArena(ulong oidAccount)
        {
            var request = new ForgiveArenaRequest
            {
                OidAccount = oidAccount,
            };
            var protocol = await _matchServerClient.ResponseMessageDispatch(request);
            if (protocol == null)
            {
                var httpResponse = new Dash.Protocol.ForgiveArenaResponse();
                httpResponse.SetResult(ErrorCode.InternalError);
                return httpResponse;
            }
            else
            {
                var response = protocol as ForgiveArenaResponse;
                return response.HttpResponse;
            }
        }

        public async Task<Dash.Protocol.GetSocialServerResponse> GetSocialServer(ulong oidAccount)
        {
            var request = new GetSocialServerRequest { OidAccount = oidAccount };
            var protocol = await _matchServerClient.ResponseMessageDispatch(request);
            if (protocol == null)
            {
                var httpResponse = new Dash.Protocol.GetSocialServerResponse();
                httpResponse.SetResult(ErrorCode.InternalError);
                return httpResponse;
            }
            else
            {
                var response = protocol as GetSocialServerResponse;
                return response.HttpResponse;
            }
        }

        public async Task<Dash.Protocol.GetSocialServerResponse> GetPartyConnectInfo(ulong oidAccount, string partyCode)
        {
            var request = new GetPartyConnectInfoRequest { OidAccount = oidAccount, PartyCode = partyCode };
            var protocol = await _matchServerClient.ResponseMessageDispatch(request);
            if (protocol == null)
            {
                var httpResponse = new Dash.Protocol.GetSocialServerResponse();
                httpResponse.SetResult(ErrorCode.InternalError);
                return httpResponse;
            }
            else
            {
                var response = protocol as GetPartyConnectInfoResponse;
                return response.HttpResponse;
            }
        }
    }
}