using Dash.Net;
using Dash.Protocol;
using server_dash.Net.Sessions;

namespace server_dash.Net.Handlers
{
    public static class MessageInboundListenerFactory
    {
        private static BattleServerConfig _config;

        static MessageInboundListenerFactory()
        {
            _config = ConfigManager.Get<BattleServerConfig>(Config.BattleServer);
        }

        public static IMessageInboundListener Create()
        {
            return new RecordInboundListener();
        }
    }

    public interface IMessageInboundListener
    {
        void OnInbound(ISession session, IProtocol message);
    }

    public class NullInboundListener : IMessageInboundListener
    {
        public void OnInbound(ISession session, IProtocol message)
        {
            
        }
    }

    public class RecordInboundListener : IMessageInboundListener
    {
        public void OnInbound(ISession session, IProtocol message)
        {
            if(session is PlayerSession gameSession)
            {
                gameSession.ArenaContext?.Recorder.RecordInbound(session, message);
            }
        }
    }
}