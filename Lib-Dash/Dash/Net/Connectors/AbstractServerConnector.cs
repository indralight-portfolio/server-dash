using System;
using System.Threading;
using System.Threading.Tasks;
using Dash.Net.Client;
using Dash.Net.MessageDispatcher;
using Dash.Net.Sessions;
using Dash.Protocol;
using Dash.Types;

namespace Dash.Net.Connectors
{
    public abstract class AbstractServerConnector : IServerConnector
    {
        protected static readonly Utility.LogicLogger logger = new Utility.LogicLogger();

        public readonly ServiceAreaType ServiceAreaType;
        public readonly ConnectorMessageDispatcher Dispatcher;
        protected DotNettyClient dotNettyClient { get; private set; }

        public virtual bool IsConnected => dotNettyClient?.IsConnected ?? false;
        public ulong MyOid { get; private set; }
        public bool CheckNetworkStatus { get; protected set; } = true;
        public bool CheckPing { get; protected set; } = false;

        public ISession GetSession() => dotNettyClient?.ListenerSession;
        private CancellationTokenSource _shutdownCts;
        private Task _markAliveTask;

        protected AbstractServerConnector(ServiceAreaType serviceAreaType, ConnectorMessageDispatcher dispatcher)
        {
            ServiceAreaType = serviceAreaType;
            Dispatcher = dispatcher;
        }

        public virtual async Task<bool> ConnectServer(string endpoint, Action onConnectionError, bool checkNetworkStatus = true, bool checkPing = true)
        {
            bool connected = await ConnectServerNoAuth(endpoint);
            if (connected == false)
            {
                logger.Error($"[ConnectServer][{ServiceAreaType}][{endpoint}] ConnectFailed.");
                return false;
            }

            CheckNetworkStatus = checkNetworkStatus;
            CheckPing = checkPing;

            bool authResult = await DoAuth();
            if (authResult == false)
            {
                return false;
            }
            logger.Debug($"[ConnectServer][{ ServiceAreaType}][{ endpoint}]");
            if (_markAliveTask != null && _shutdownCts != null)
            {
                _shutdownCts?.Cancel();
                await _markAliveTask;
            }
            _shutdownCts = new CancellationTokenSource();

            if (checkNetworkStatus)
            {
                logger.Debug($"[ConnectServer][{ ServiceAreaType}][{ endpoint}] RunMarkAliveTask");
                _markAliveTask = RunMarkAliveTask(_shutdownCts.Token);
            }

            return true;
        }

        public virtual async Task<bool> ConnectServerNoAuth(string endpoint)
        {
            if (dotNettyClient != null)
            {
                throw new Exception($"[ServerConnector][{ServiceAreaType}] NetClient exist, Connected : {dotNettyClient.IsConnected}");
            }

            dotNettyClient = await Connect(endpoint, Dispatcher);
            if (dotNettyClient == null)
            {
                return false;
            }

            return true;
        }

        protected abstract Task<bool> DoAuth();
        protected abstract void SendAlive();
        protected abstract TimeSpan GetSendAliveInterval();

        protected void SetMyOid(ulong oid)
        {
            MyOid = oid;
        }

        public virtual Task RunMarkAliveTask(CancellationToken token)
        {
            return Task.Factory.StartNew(async () =>
            {
                TimeSpan interval = GetSendAliveInterval();
                while (token.IsCancellationRequested == false)
                {
                    if (dotNettyClient == null)
                    {
                        logger.Error($"[{ServiceAreaType}][ServerConnector] client is null!");
                        break;
                    }

                    if (dotNettyClient.IsConnected == false)
                    {
                        logger.Error($"[{ServiceAreaType}][ServerConnector] Server disconnected!");
                        break;
                    }

                    SendAlive();
                    await Task.Delay(interval, token);
                }
            }, TaskCreationOptions.LongRunning);
        }


        private async Task<DotNettyClient> Connect(string endpoint, ConnectorMessageDispatcher dispatcher)
        {
            var netClient = DotNettyClient.Create(dispatcher, ServiceAreaType);
            logger.Info($"Try connect to {ServiceAreaType} server [{endpoint}]");
            string[] splitted = endpoint.Split(':');
            string ip = splitted[0];
            int port = int.Parse(splitted[1]);

            ConnectResult connectResult = await netClient.Connect(ip, port);
            logger.Info($"[{ServiceAreaType}] ConnectResult : " + connectResult);
            if (connectResult != ConnectResult.Success)
            {
                return null;
            }

            return netClient;
        }


        public virtual async Task CleanConnectionAsync()
        {
            _shutdownCts?.Cancel();
            if (_markAliveTask != null)
                await _markAliveTask;
            dotNettyClient?.Cleanup();
            dotNettyClient = null;
        }

        public virtual async Task ShutDownAsync()
        {
            await CleanConnectionAsync();
        }

        // 아직 서버에서 쓰기엔 쓰레딩 문제가 있음.
        public MessageReceiver CreateReceiver()
        {
            #if Common_Server
            throw new NotSupportedException();
            #endif
            return new MessageReceiver(ServiceAreaType, Dispatcher);
        }

        public Task<T> GetResponseFor<T>(ISequentialProtocol request, TimeSpan timeout = default) where T : ISequentialProtocol
        {
            var session = GetSession();
            if (session == null)
            {
                return Task.FromResult<T>(default(T));
            }

            return session.WriteAndGetResponse<T>(request, timeout);
        }
    }
}