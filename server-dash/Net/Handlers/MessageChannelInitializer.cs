using System;
using Dash.Net.Handlers;

namespace server_dash.Net.Handlers
{
    using DotNetty.Codecs.Protobuf;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    
    class MessageChannelInitializer : ChannelInitializer<ISocketChannel>
    {
        private readonly MessageDispatcher _dispatcher;
        private readonly ChannelManager _channelManager;
        private readonly Sessions.SessionManager _sessionManager;
        private readonly IMessageInboundListener _messageInboundListener;
        private readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private Dash.Types.ServiceAreaType _myServiceType;
        public MessageChannelInitializer(Dash.Types.ServiceAreaType myServiceType, MessageDispatcher dispatcher, ChannelManager channelManager, Sessions.SessionManager sessionManager, IMessageInboundListener messageInboundListener)
        {
            this._dispatcher = dispatcher;
            this._channelManager = channelManager;
            this._sessionManager = sessionManager;
            this._messageInboundListener = messageInboundListener;
            this._myServiceType = myServiceType;
        }

        protected override void InitChannel(ISocketChannel channel)
        {
            // here is accepted socket.
            _logger.Info($"[{_myServiceType}] Channel initialized : " + channel.ToString());
            IChannelPipeline pipeline = channel.Pipeline;

            // outbounds
            pipeline.AddLast(new OutBoundHandler());
            
            // inbounds
            pipeline.AddLast(new CompositeDecoder());
            //pipeline.AddLast(new FrameDecoder());
            //pipeline.AddLast(new MessageDecoder(_dispatcher.ParseTypeCodes));
            pipeline.AddLast(new SessionHandler(_dispatcher, _channelManager, _sessionManager, _messageInboundListener, _myServiceType));
        }
    }
}