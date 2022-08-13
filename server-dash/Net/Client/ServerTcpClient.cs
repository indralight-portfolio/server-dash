using System;
using System.Net;
using System.Threading.Tasks;
using Dash.Net;
using Dash.Protocol;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NLog;

namespace server_dash.Net.Client
{
    public class ServerTcpClient : ChannelHandlerAdapter
    {
        public IChannel Channel => _channel;
        private IChannel _channel;
        private InternalClientMessageDispatcher _dispatcher;

        private static readonly ILogger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public ServerTcpClient(InternalClientMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task<bool> ConnectAsync(EndPoint endpoint)
        {
            _channel = await new Bootstrap()
                .Group(new SingleThreadEventLoop())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.SoRcvbuf, 512 * 512)
                .Option(ChannelOption.SoSndbuf, 512 * 512)
                .Handler(new Dash.Net.MessageChannelInitializer(this, _dispatcher.ParseTypeCodes, true))
                .ConnectAsync(endpoint);
            if (_channel != null && _channel.Active == true)
            {
                return true;
            }

            return false;
        }

        public Task CloseAsync()
        {
            return _channel.CloseAsync();
        }

        public Task SendAsync(IProtocol protocol)
        {
            return _channel.WriteAndFlushAsync(protocol);
        }

        public Task SendAsync(RawMessage rawMessage)
        {
            return _channel.WriteAndFlushAsync(rawMessage);
        }

        
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IProtocol protocolMessage)
                {
                    _dispatcher.Handle(protocolMessage);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
            }
        }
    }
}
