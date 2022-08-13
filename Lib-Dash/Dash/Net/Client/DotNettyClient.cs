using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dash.Net;
using Dash.Net.Connectors;
using Dash.Net.MessageDispatcher;
using Dash.Net.Sessions;
using Dash.Protocol;
using Dash.Types;
using DotNetty.Common.Internal;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
namespace Dash.Net.Client
{
    public enum ConnectResult
    {
        Undefined = 0,
        Success,
        Fail,
    }

    public class DotNettyClient : ChannelHandlerAdapter, INetSender
    {
        public override string ToString()
        {
            if (_channel != null)
            {
                return _channel.ToString();
            }

            return "Not initialized Client";
        }

        private IChannel _channel;
        private IEventLoop _eventLoop;
        private readonly ConnectorMessageDispatcher _dispatcher;
        private readonly ServiceAreaType _serviceAreaType;

        public bool IsConnected => _channel?.Active ?? false;
        public bool ManuallyDisconnected { get; private set; }
        public ServiceAreaType ServiceAreaType => _serviceAreaType;
        public IListenerSession ListenerSession { get; private set; }
        private int _createListenerSessionThreadId;

        private DotNettyClient(ConnectorMessageDispatcher dispatcher, ServiceAreaType serviceAreaType)
        {
            _dispatcher = dispatcher;
            _serviceAreaType = serviceAreaType;
        }

        public static DotNettyClient Create(ConnectorMessageDispatcher dispatcher, ServiceAreaType serviceAreaType)
        {
            return new DotNettyClient(dispatcher, serviceAreaType);
        }

        public async Task<ConnectResult> Connect(string host, int port)
        {
            try
            {
                _channel = await new Bootstrap()
                    .Group(new SingleThreadEventLoop())
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.SoRcvbuf, 1024 * 512)
                    .Option(ChannelOption.SoSndbuf, 1024 * 512)
                    .Handler(new Dash.Net.MessageChannelInitializer(() => this, null, true))
                    .ConnectAsync(Common.Utility.NetUtility.GetEndPoint($"{host}:{port}"));

                _eventLoop = _channel.EventLoop;

                return _channel.Active == true ? ConnectResult.Success : ConnectResult.Fail;
            }
            catch(Exception e)
            {
                Common.Log.Logger.Instance.Fatal(e);
                return ConnectResult.Fail;
            }
        }
        public void CreateListenerSession(IServerConnector serverConnector, ulong oid)
        {
            _createListenerSessionThreadId = Thread.CurrentThread.ManagedThreadId;

            ListenerSession = new ListenerSession(serverConnector, _serviceAreaType, _channel)
            {
                Oid = oid,
            };
        }

        public async Task Cleanup()
        {
            if (Thread.CurrentThread.ManagedThreadId != _createListenerSessionThreadId)
            {
                throw new Exception($"Cleanup at wrong thread : {Thread.CurrentThread.ManagedThreadId}");
            }
            ManuallyDisconnected = true;
            if (_channel != null)
            {
                await _channel.CloseAsync();
            }
            _channel = null;
            _eventLoop = null;
        }

        public void Send<T>(T message, bool isFlush = true) where T : Dash.Protocol.IProtocol
        {
            if (IsConnected == false || ListenerSession == null)
            {
                Common.Log.Logger.Instance.Debug($"[{_serviceAreaType}]Channel is not activated yet : {message}, IsConnected : {IsConnected}, ListenerSession is Null : {ListenerSession == null}");
                return;
            }

            ListenerSession.Write(message, isFlush);
        }

        public void Flush()
        {
            ListenerSession?.Flush();
        }

        #region ChannelHandlerAdapter

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);

            #if Common_Unity
            Common.Log.Logger.Instance.Info($"[{_serviceAreaType}] Channel active : {context.Channel}");
            #endif
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var session = ListenerSession;
            if (session == null)
            {
                Common.Log.Logger.Instance.Error($"[{_serviceAreaType}] Ignore message : {message}");
                return;
            }

            if (message is IProtocol protocol)
            {
                _dispatcher.Handle(session, protocol);
            }
            else
            {
                Common.Log.Logger.Instance.Error($"[{_serviceAreaType}]Channel Read : {message}");
            }
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Common.Log.Logger.Instance.Info($"[{_serviceAreaType}]Channel[{context.Channel}][{context.Channel.RemoteAddress}] Inactive.");
#if Common_Unity
            var sessionClosed = new SessionClosed() { Param = this };
            var session = ListenerSession;
            if (session == null)
            {
                Common.Log.Logger.Instance.Error($"[{_serviceAreaType}] Ignore message : {sessionClosed}");
                return;
            }
            _dispatcher.Handle(session, sessionClosed);
#endif
        }

        #endregion
    }
}