using System;
using System.Net.Sockets;
using System.Threading;
using Common.Utility;
using Dash.Net;
using Dash.Protocol;

namespace server_dash.Net.Handlers
{
    using Net.Sessions;
    using DotNetty.Transport.Channels;
    using Utility;
    using Dash.Types;

    public class SessionHandler : ChannelHandlerAdapter
    {
        private readonly MessageDispatcher _dispatcher;
        private readonly ChannelManager _channelManager;
        private readonly SessionManager _sessionManager;
        private readonly IMessageInboundListener _messageInboundListener;
        private ISession _session;
        private readonly LogicLogger _logger = new LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());
        //private static int _concurrentUser = 0;
        private ServiceAreaType _myServiceType;

        public SessionHandler(
            MessageDispatcher dispatcher, 
            ChannelManager channelManager, 
            SessionManager sessionManager, 
            IMessageInboundListener messageInboundListener,
            Dash.Types.ServiceAreaType myServiceType)
        {
            this._dispatcher = dispatcher;
            this._channelManager = channelManager;
            this._sessionManager = sessionManager;
            this._messageInboundListener = messageInboundListener;
            this._myServiceType = myServiceType;
        }
        
        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            if ( _session.Channel.Id.Equals(context.Channel.Id) == false )
            {
                _logger.Info(_session, $"[{_myServiceType}] Channel Unregistered! but already replaced");
                return;
            }

            _logger.Info(_session, $"[{_myServiceType}] Channel Unregistered!");
            _sessionManager.RemoveSession(_session);

            _session = null;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (_session == null)
            {
                _logger.Error($"[{_myServiceType}] Session is not valid yet.");
                return;
            }

            try
            {
                if (message is IProtocol protocolMessage)
                {
                    _dispatcher.Handle(_session, protocolMessage);
                    _messageInboundListener.OnInbound(_session, protocolMessage);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal(_session, $"[{_myServiceType}] Exception caught, {message}");
                _logger.Fatal(ex);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (exception is SocketException socketException)
            {
                if (socketException.SocketErrorCode == SocketError.ConnectionReset)
                {
                    _logger.Info(_session, $"[{_myServiceType}] reset by peer.");
                    return;
                }
            }

            _logger.Fatal(_session, exception);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            if (context.Channel.Active == false)
            {
                _logger.Info(_myServiceType, _session, $"[{_myServiceType}]{context.Channel.Log()} Flawed Channel Activated, close channel.");
                context.Channel.CloseAsync();
                return;
            }
            
            _logger.Info($"[{_myServiceType}]{context.Channel.Log()} Channel Activated.");
            _session = new DummySession(context.Channel);

            var channelActive = new ChannelActive();
            _dispatcher.Handle(_session, channelActive);

            _channelManager.AddChannel(context.Channel);
            IncUser();
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _channelManager.RemoveChannel(context.Channel, _session);
            if (_session == null)
            {
                _logger.Info(_myServiceType, _session, $"[{_myServiceType}]{context.Channel.Log()} Flawed Channel Inactivated!");
                return;
            }

            _logger.Info(_myServiceType, _session, $"[{_myServiceType}]{context.Channel.Log()} Channel Inactivated!");

            var channelInactive = new ChannelInactive();
            _dispatcher.Handle(_session, channelInactive);

            if (_session.Channel.Id.Equals(context.Channel.Id)) // 현재의 채널과 동일할때만 SessionClosed를 보냄.
            {
                var channelClosed = new SessionClosed
                {
                    OidAccount = _session.OidAccount
                };

                _logger.Info(_myServiceType, _session, $"[{_myServiceType}]{context.Channel.Log()} Dispatch SessionClosed");
                _dispatcher.Handle(_session, channelClosed);
            }

            DecUser();
        }

        public ServerSession CreateServerSession(ServiceAreaType serviceAreaType, IChannel channel, string uuid, string endpoint)
        {
            var session = new ServerSession(serviceAreaType, channel, uuid, endpoint);
            
            _session = session;
            return session;
        }
        public PlayerSession CreateGameSession(IChannel channel)
        {
            var session = new PlayerSession(channel);
            _session = session;
            return session;
        }
        public SocialSession CreateSocialSession(IChannel channel)
        {
            var session = new SocialSession(channel);
            _session = session;
            return session;
        }

        private void IncUser()
        {
            Interlocked.Increment(ref _sessionManager.ConcurrentUser);
            _logger.Info($"[{_myServiceType}] Concurrent Users : {_sessionManager.ConcurrentUser}");
        }

        private void DecUser()
        {
            int after = Interlocked.Decrement(ref _sessionManager.ConcurrentUser);
            _logger.Info($"[{_myServiceType}] Concurrent Users : {_sessionManager.ConcurrentUser}");

            if (_sessionManager.ConcurrentUser == 0 && after == 0)
            {
                _logger.Info($"[{_myServiceType}] Starting garbage collection. {UnitUtility.SizeSuffix(GC.GetTotalMemory(false))}");
                int tick = Environment.TickCount;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                _logger.Info($"[{_myServiceType}] End garbage collection. {UnitUtility.SizeSuffix(GC.GetTotalMemory(true))} {Environment.TickCount-tick}ms");

                _logger.Info($"[{_myServiceType}] Channel Count : {_channelManager.GetLoggedInChannelsCount()}," +
                             $"{_channelManager.GetNotLoggedInChannelsCount()}");
            }
        }
    }
}