using System.Collections;
using System.Collections.Generic;
using Dash.Protocol;
using Dash.Types;
using DotNetty.Transport.Channels;
using server_dash.Net.Handlers;
using server_dash.Net.Sessions;
using server_dash.Protocol;

namespace server_dash.Match.Services
{
    public class SessionService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private ServerCoordinator _serverCoordinator;
        //private PartySerialProvider _serialProvider;
        private PartySerialProvider _partySerialProvider;

        public SessionService(ServerCoordinator battleServerCoordinator, PartySerialProvider serialProvider)
        {
            _serverCoordinator = battleServerCoordinator;
            _partySerialProvider = serialProvider;
        }
        [BindMessage(ServiceAreaType.Dummy)]
        public void ChannelWhoAmI(ISession session, ChannelWhoAmI message)
        {
            _logger.Info($"[Match][SessionService] ChannelWhoAmI ServiceAreaType : " +
                $"{message.ServiceAreaType}, UUID : {message.UUID}, Endpoint : {message.Endpoint}");
            var sessionHandler = session.Channel.Pipeline.Get<SessionHandler>();
            var serverSession = sessionHandler.CreateServerSession(message.ServiceAreaType, session.Channel, message.UUID, message.Endpoint);
            serverSession.OidAccount = (ulong)Common.Utility.StringHash.GetHashCode(message.Endpoint);
            NetServer.ChannelManager.OnChannelLogin(serverSession.Channel, serverSession);
            NetServer.SessionManager.AddSession(serverSession.OidAccount, serverSession);

            //레디스에 시리얼 셋팅하고 소셜 서버에 알려준다.
            //if(message.ServiceAreaType == ServiceAreaType.Social)
            //{
            //    var range = _serialProvider.IssueRange();
            //    session.Write(new PartySerialRange() { Begin = range.Item1, End = range.Item2 });
            //}
        }
        [BindMessage(ServiceAreaType.All)]
        public void SessionClosed(ISession session, SessionClosed message)
        {
            _logger.Info($"[Match][SessionService] : SessionClosed [{session}]");
            session.IsAlive = false;
            NetServer.ServiceExecuteMultiplexerInstance.GetExecutor(session.ServiceExecutorId).RemoveMessageQueue(session.ServiceExecutorId);
        }
        [BindMessage(ServiceAreaType.All)]
        public void ChannelActive(ISession session, ChannelActive message)
        {
        }
        [BindMessage(ServiceAreaType.All)]
        public void ChannelInactive(ISession session, ChannelInactive message)
        {
            _logger.Info($"[Match][ObserveBattleServerService] ChannelInactive : {session.Channel}");
            ServerSession serverSession = session as ServerSession;
            if (serverSession == null)
            {
                _logger.Error($"[{session}]Session is not ServerSession!");
                return;
            }

            _serverCoordinator.Remove(serverSession);
        }

        [BindMessage(ServiceAreaType.Battle, ServiceAreaType.Social, ServiceAreaType.Lobby)]
        public void ServerAlive(ISession session, ServerAlive message)
        {
            ServerSession serverSession = session as ServerSession;
            if (serverSession == null)
            {
                _logger.Error($"[{session}]Session is not ServerSession!");
                return;
            }

            _serverCoordinator.OnServerAlive(serverSession, message);
        }
        //[BindMessage(ServiceAreaType.Social)]
        //public void IssueSerialRange(ServerSession session, PartySerialRange message)
        //{
        //    var range = _serialProvider.IssueRange();
        //    session.Write(new PartySerialRange() { Begin = range.Item1, End = range.Item2 });
        //}
        [BindMessage(ServiceAreaType.Social)]
        public void MakePartySerialAndCode(ServerSession session, PartySerialAndCodeRequest message)
        {
            PartySerialAndCodeResponse response = new PartySerialAndCodeResponse()
            {
                Serial = message.Serial,
                
            };
            response.PartySerial = _partySerialProvider.Issue();
            response.PartyCode = Utility.Party.MakePartyCode(response.PartySerial);
            session.Write(response);
        }

        public void BroadCastKickAll()
        {
            foreach(var session in _serverCoordinator.ServerSessions.Values)
            {
                session.Write(new KickAllSession());
            }
        }
    }
}
