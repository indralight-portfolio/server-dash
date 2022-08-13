using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Handlers;
using NLog;
using DotNetty.Handlers.Logging;
using server_dash.Net.Handlers;
using server_dash.Battle.Services;
using server_dash.Battle.Services.GamePlay;
using server_dash.Battle.Services.Match;
using Dash.Server.Dao.Cache;
using server_dash.Internal.Services;
using Dash.Model;
using Dash.Types;
using server_dash.Battle.Services.Validation;
using server_dash.Execution.Runnable;
using TaskScheduler = server_dash.Execution.TaskScheduler;

namespace server_dash.Battle
{
    public class BattleServer
    {
        public readonly string UUID;
        private BattleServerConfig _config;
        private readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private readonly ChannelManager _channelManager;
        private readonly Net.Sessions.SessionManager _sessionManager;

        private readonly MatchMakingProxyService _matchMakingProxyService;
        private readonly MultiplayService _multiplayService;
        private readonly SingleplayService _singleplayService;
        private readonly GamePlayService _gamePlayService;
        private readonly ArenaService _arenaService;
        private readonly ManageService _manageService;
        private readonly SessionService _sessionService;
        private readonly StatusService _statusService;

        private readonly RawMessageHandler _rawMessageHandler;
        private readonly CommandMessageHandler _commandMessageHandler;
        private readonly MessageDispatcher _dispatcher;
        private readonly TaskScheduler _taskScheduler;
        private Net.InternalServerClient _matchServerClient;
        private MonitorService _monitorService;

        private SessionValidator _sessionValidator;

        private IChannel _bootstrapChannel;
        private IEventLoopGroup _boss;
        private IEventLoopGroup _worker;

        private bool _isCleanedUp = false;

        public static MessageHandlerProvider MessageHandlerProvider { get; private set; }

        public static ServiceExecuteMultiplexer ServiceExecuteMultiplexerInstance { get; private set; }
        public BattleServer(BattleServerConfig config)
        {
            _config = config;
            UUID = new ServerUUID().Value;
            string endPoint = ServerIPManager.Instance.Endpoint + ":" + config.Port;
            var serverConfig = ConfigManager.Get<ServerConfig>(Config.Server);
            string matchServerEndpoint = serverConfig.MatchServerEndpoint;

            CoreStateValidator.Init(_config.AntiHack);

            SerialIssuer serialIssuer = new SerialIssuer();
            _channelManager = new ChannelManager(Dash.Types.ServiceAreaType.Battle);
            _sessionManager = new Net.Sessions.SessionManager(Dash.Types.ServiceAreaType.Battle);
            _monitorService = new MonitorService(UUID, _channelManager, _config);
            _matchServerClient = new Net.InternalServerClient(UUID, endPoint, Dash.Types.ServiceAreaType.Battle, matchServerEndpoint);

            _rawMessageHandler = new RawMessageHandler();
            MessageHandlerProvider = new MessageHandlerProvider();
            ServiceExecuteMultiplexerInstance = new ServiceExecuteMultiplexer(ServiceAreaType.Battle, MessageHandlerProvider, _config.CoreCount);

            _sessionValidator = new SessionValidator();

            MailService mailService = new MailService(DaoCache.Instance);
            RewardService rewardService = new RewardService(DaoCache.Instance, mailService);
            _matchMakingProxyService = new MatchMakingProxyService(_matchServerClient);
            _arenaService = new ArenaService(rewardService);
            _sessionService = new SessionService(_arenaService, _channelManager, _sessionManager, ServiceExecuteMultiplexerInstance, _sessionValidator);
            _statusService = new StatusService();
            _multiplayService = new MultiplayService(_arenaService, serialIssuer, _matchMakingProxyService);
            _singleplayService = new SingleplayService(_arenaService, serialIssuer);
            _gamePlayService = new GamePlayService(_arenaService);
            _manageService = new ManageService(config);

            _dispatcher = new MessageDispatcher(Dash.Types.ServiceAreaType.Battle.ToString(), ServiceExecuteMultiplexerInstance, _rawMessageHandler, _gamePlayService.OnCommandMessage);
            _taskScheduler = new TaskScheduler(_dispatcher, ServiceExecuteMultiplexerInstance);
        }

        public async Task Run()
        {
            _matchServerClient.RegisterHandler(_matchMakingProxyService);
            _matchServerClient.RegisterHandler(_sessionService);
            _matchServerClient.RegisterHandler(_multiplayService);
            await Task.WhenAll(
                _monitorService.StartAsync(CancellationToken.None),
                _matchServerClient.StartAsync(CancellationToken.None)
            );

            _dispatcher.RegisterHandler(_sessionService);
            _dispatcher.RegisterHandler(_multiplayService);
            _dispatcher.RegisterHandler(_statusService);
            _dispatcher.RegisterHandler(_singleplayService);
            _dispatcher.RegisterHandler(_gamePlayService);
            _dispatcher.RegisterHandler(_manageService);
            _dispatcher.RegisterHandler(_arenaService);

            _rawMessageHandler.Register(_gamePlayService);
            _rawMessageHandler.SetFallbackHandler(_gamePlayService.OnRawMessage);
            MessageHandlerProvider.Init(_dispatcher.MessageHandlers, _rawMessageHandler);

            var logger = new LoggingHandler("Logger");

            _boss = new MultithreadEventLoopGroup(2);
            if (_config.CoreCount != 0)
                _worker = new MultithreadEventLoopGroup(_config.CoreCount * 2);
            else
                _worker = new MultithreadEventLoopGroup();

            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(_boss, _worker)
                .Channel<TcpServerSocketChannel>()
                .LocalAddress(_config.Port)
                .Handler(logger)
                .ChildHandler(new MessageChannelInitializer(Dash.Types.ServiceAreaType.Battle, _dispatcher, _channelManager, _sessionManager, MessageInboundListenerFactory.Create()));

            bootstrap.Option(ChannelOption.SoReuseaddr, true);
            bootstrap.Option(ChannelOption.SoBacklog, 1024);
            bootstrap.ChildOption(ChannelOption.TcpNodelay, true);
            bootstrap.ChildOption(ChannelOption.WriteBufferHighWaterMark, 768 * 1024);
            bootstrap.ChildOption(ChannelOption.SoRcvbuf, 1024 * 512);
            bootstrap.ChildOption(ChannelOption.SoSndbuf, 1024 * 512);

            _bootstrapChannel = await bootstrap.BindAsync(_config.Port);

            MatchAspectFactory.Init(_matchMakingProxyService);
            _logger.Info($"BattleServer started! {BuildVersion.ToString()}");

            while (_bootstrapChannel.Active)
            {
                Thread.Sleep(10);
            }
        }

        public async Task Clean()
        {
            await _bootstrapChannel.CloseAsync();

            Task.WaitAll(
                _boss.ShutdownGracefullyAsync(),
                _worker.ShutdownGracefullyAsync(),
                ServiceExecuteMultiplexerInstance.ShutdownGracefullyAsync(),
                _taskScheduler.ShutdownGracefullyAsync(),
                _channelManager.ShutdownGracefullyAsync(),
                _matchServerClient.ShutDownAsync(),
                _monitorService.StopAsync(CancellationToken.None));

            _isCleanedUp = true;
            _logger.Info("Cleanup successfully.");
        }

        public void OnProcessExit(object sender, EventArgs e)
        {
            if (_isCleanedUp == false)
            {
                // SIGTERM을 받으면 NLog가 동작하지 않아서 이걸로 남김.
                Console.WriteLine("OnProcessExit: Starting Clean");
                Clean().Wait();
            }
        }
    }
}