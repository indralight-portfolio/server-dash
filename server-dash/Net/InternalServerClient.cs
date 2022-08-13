using System;
using System.Threading;
using System.Threading.Tasks;
using Dash.Protocol;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Hosting;
using NLog;
using server_dash.Net.Client;
using server_dash.Protocol;

namespace server_dash.Net
{
    public class InternalServerClient : BackgroundService
    {
        private static readonly ILogger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private readonly ManualResetEventSlim _shotDownEventSlim = new ManualResetEventSlim();

        private ServerAlive _serverAlive;
        private Task _markAliveTask;
        private ServerTcpClient _tcpClient;
        private InternalClientMessageDispatcher _messageDispatcher;
        private int _lastIssuedMessageSerial = 0;

        private readonly string _uuid;
        private readonly string _myEndpoint;
        private readonly string _endpoint;
        private readonly Dash.Types.ServiceAreaType _serviceAreaType;


        public InternalServerClient(string uuid, string myEndpoint, Dash.Types.ServiceAreaType serviceAreaType, string endpoint)
        {
            _uuid = uuid;
            _myEndpoint = myEndpoint;
            _serviceAreaType = serviceAreaType;
            _endpoint = endpoint;
            _serverAlive = new ServerAlive()
            {
                UUID = _uuid,
                Endpoint = _myEndpoint,
                ServiceAreaType = serviceAreaType,
            };
            _messageDispatcher = new InternalClientMessageDispatcher();
        }

        public void RegisterHandler<T>(T target)
        {
            _messageDispatcher.RegisterHandler(target);
        }

        public void RegisterResponseHandler<TResponse>(int typeCode) where TResponse : ISequentialProtocol
        {
            _messageDispatcher.AddMessageHandlerUnsafe<TResponse>(typeCode);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoConnect(_endpoint, 0);
        }

        private async Task<bool> DoConnect(string endpoint, int tryCount)
        {
            _logger.Info($"[{_serviceAreaType}] Trying to {(tryCount > 0 ? "re" : string.Empty)}connect to Server ({endpoint})");
            try
            {
                _tcpClient = new ServerTcpClient(_messageDispatcher);

                bool result = await _tcpClient.ConnectAsync(Common.Utility.NetUtility.GetEndPoint(endpoint));
                if (result == false || _tcpClient.Channel == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Min(30, tryCount)));
                    DoConnect(endpoint, tryCount + 1);
                    return false;
                }

                _logger.Info($"[{_serviceAreaType}][InternalServerClient] Server connected, Endpoint : {_tcpClient.Channel.RemoteAddress}");
                await _tcpClient.SendAsync(new Dash.Protocol.ChannelWhoAmI()
                {
                    ServiceAreaType = _serviceAreaType,
                    UUID = _uuid,
                    Endpoint = _myEndpoint
                });
                await Task.Delay(TimeSpan.FromSeconds(1));

                _markAliveTask = Task.Factory.StartNew(async () =>
                {
                    while (_shotDownEventSlim.IsSet == false)
                    {
                        if (_tcpClient.Channel.Active == false)
                        {
                            _logger.Error($"[{_serviceAreaType}][InternalServerClient] Server disconnected!");
                            await Task.Delay(TimeSpan.FromSeconds(Math.Min(30, tryCount)));
                            DoConnect(endpoint, tryCount + 1);
                            break;
                        }

                        _tcpClient.SendAsync(_serverAlive);
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }, TaskCreationOptions.LongRunning);
                return true;
            }
            catch (ConnectException connectException)
            {
                _logger.Fatal(connectException.Message);
                if (tryCount == 0)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
                if (tryCount == 0)
                {
                    throw;
                }
            }
            finally
            {
                if (_tcpClient.Channel == null || _tcpClient.Channel.Active == false)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Min(30, tryCount)));
                    await DoConnect(endpoint, tryCount + 1);
                }
            }

            return true;
        }

        public async Task ShutDownAsync()
        {
            _shotDownEventSlim.Set();
            await Task.WhenAll(_tcpClient.CloseAsync(), _markAliveTask);
        }
        public async Task<ISequentialProtocol> ResponseMessageDispatch<TRequest>(TRequest request) where TRequest : ISequentialProtocol
        {
            request.Serial = Interlocked.Increment(ref _lastIssuedMessageSerial);
            var responseTask = _messageDispatcher.GetResponse(request);
            _tcpClient.SendAsync(request);
            return await responseTask;
        }
        public void Send<T>(T protocol) where T : IProtocol
        {
            _tcpClient.SendAsync(protocol);
        }
    }
}