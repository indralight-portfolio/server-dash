using System;
using System.Collections.Generic;
using Dash.Net.Handlers;

namespace Dash.Net
{
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    
    public class MessageChannelInitializer : ChannelInitializer<ISocketChannel>
    {
        #if Common_Unity
        private Common.Log.ILogger _logger = Common.Log.Logger.Instance;
        #else
        private readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        #endif

        private readonly Func<ChannelHandlerAdapter> _channelHandlerFactory;
        private readonly HashSet<int> _typeCodes;
        private bool _allParse = false;

        public MessageChannelInitializer(Func<ChannelHandlerAdapter> channelHandlerFactory, HashSet<int> typeCodes = null, bool allParse = false)
        {
            _channelHandlerFactory = channelHandlerFactory;
            _typeCodes = typeCodes;
            _allParse = allParse;
        }

        protected override void InitChannel(ISocketChannel channel)
        {
            // here is accepted socket.
            _logger.Info("Channel initialized : " + channel.ToString());
            IChannelPipeline pipeline = channel.Pipeline;

            // outbounds
            pipeline.AddLast(new OutBoundHandler());
            
            // inbounds
            pipeline.AddLast(new CompositeDecoder());
            pipeline.AddLast(_channelHandlerFactory());
        }
    }
}