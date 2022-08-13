using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Common.Log;
using Common.StaticInfo;
using Dash;
using Dash.Server.Dao;
using Dash.Server.Dao.Cache;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using server_dash.AWS;
using server_dash.Match;
using server_dash.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Logger = NLog.Logger;

namespace server_dash
{
    public enum WebAPI
    {
        Lobby,
        Match,
        MatchAdmin,
        Etc,
    }

    public class Program
    {
        private static Logger _logger = null;

        static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Utility.EnvUtility.SetEnv(environment);

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("Config/config.json", false, true)
                .AddJsonFile($"Config/config.{environment}.json", true, true)
                .AddCommandLine(args)
                .Build();

            if (environment == "dev-local")
            {
                config = new ConfigurationBuilder()
                    .AddConfiguration(config)
                    .AddJsonFile($"Config/config.corecount.{environment}.json", true, true)
                    .Build();
            }

            ConfigManager.Init(config);

            NLogUtility.SetUp("Config/NLog.config");

            Common.Reflection.PolyTypeDefinitionsHolder.SetInstance(new Dash.PolyTypeDefinitions());

            MessagePack.Resolvers.StaticCompositeResolver.Instance.Register(
                MessagePack.Resolvers.BuiltinResolver.Instance,
                MessagePack.Resolvers.CompositeResolver.Create(
                    MessagePackDefinition.GetCustomFormatters()  // customized
                    ),
                Dash.Resolvers.GeneratedResolver.Instance, // Dash
                server_dash.Resolvers.GeneratedResolver.Instance // server-dash
            );
            MessagePack.MessagePackSerializer.DefaultOptions =
                MessagePack.MessagePackSerializerOptions.Standard.WithResolver(
                    MessagePack.Resolvers.StaticCompositeResolver.Instance);

            try
            {
                // Override settings in nlog.config with environment specific settings
                var logFactory = LogManager.LoadConfiguration($"Config/NLog.{environment}.config");
                LogManager.Configuration = logFactory.Configuration;
            }
            catch (Exception e)
            {
                // No problems here!
                // If environment specific log config doesn't exists just continue
            }

            var serverConfig = config.GetSection(Config.Server).Get<ServerConfig>();
            var rdsConfig = config.GetSection(Config.Rds).Get<RdsConfig>();
            var redisConfig = config.GetSection(Config.Redis).Get<RedisConfig>();

            #if DEBUG
            bool needTraceOn = true;
            #else
            bool needTraceOn = serverConfig.IsPrivate;
            #endif

            if (needTraceOn == true)
            {
                LogManager.Configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Off, "console", "*");
            }

            _logger = NLogUtility.GetCurrentClassLogger();

            ThreadPool.GetMinThreads(out int workerThreads, out int iocpThreads);
            _logger.Info($"WorkerThread count : {workerThreads} CompletionPortThreads count : {iocpThreads}");

            // 생성만 해도 실행됨.
            _ = new TracingEventListener();

            Task.WaitAll(
                Task.Factory.StartNew(() =>
                {
                    var coroutine = Dash.StaticInfo.StaticInfo.Instance.Init(StaticInfoType.Common, serverConfig.ResourcePath);
                    while (coroutine.MoveNext())
                    {
                        Thread.Sleep(1);
                    }
                }),
                Task.Factory.StartNew(() =>
                {
                    MessageSerializer.Init(
                        Assembly.GetAssembly(typeof(Program)),
                        Assembly.GetAssembly(typeof(Dash.Command.ICommand))
                        );
                }),
                Task.Factory.StartNew(() =>
                    {
                        var redisConnector = new RedisConnector(redisConfig.Game);
                        bool redisConnectResult = redisConnector.ConnectAsync().Result;
                        if (redisConnectResult == false)
                        {
                            _logger.Fatal("Redis connect failed!");
                            return;
                        }

                        GameDao dao = new GameDao(rdsConfig.GameDB);
                        DaoCache.Init(redisConnector.ConnectionMultiplexer, redisConnector.Database, dao, redisConfig.RedisExConfig);
                    }
                ),
                Task.Factory.StartNew(() =>
                    {
                        var redisConnector = new RedisConnector(redisConfig.Monitor);
                        bool redisConnectResult = redisConnector.ConnectAsync().Result;
                        if (redisConnectResult == false)
                        {
                            _logger.Fatal("Redis connect failed!");
                            return;
                        }

                        MonitorCache.Init(redisConnector.ConnectionMultiplexer, redisConnector.Database);
                    }
                ));

            Internal.Services.GameService.Init(DaoCache.Instance);
            Internal.Services.PlayerService.Init(DaoCache.Instance);
            Internal.Services.InventoryService.Init(DaoCache.Instance);
            Internal.Services.TalentService.Init(DaoCache.Instance);
            Internal.Services.ProgressService.Init(DaoCache.Instance);
            Internal.Services.StaminaService.Init(DaoCache.Instance);

            ServerIPManager.Instance.Init("Endpoint.json");

            var awsConfig = ConfigManager.Get<AWSConfig>(Config.AWS);
            var awsOptions = new AWSOptions
            {
                Credentials = new BasicAWSCredentials(awsConfig.Default.AccessKey, awsConfig.Default.SecretKey),
                Region = RegionEndpoint.GetBySystemName(awsConfig.Default.Region),
            };
            S3ClientFactory.Init().AddClient("Default", awsOptions);
            DynamoDBClientFactory.Init().AddClient("Default", awsOptions);
            CloudWatchClientFactory.Init().AddClient("Default", awsOptions);

            List<Task> serverTasks = new List<Task>();
            try
            {
                var matchServerConfig = config.GetSection(Config.MatchServer).Get<MatchServerConfig>();
                if (matchServerConfig.Active == true)
                {
                    serverTasks.Add(
                        WebHost.CreateDefaultBuilder(args)
                            .SuppressStatusMessages(true) //disable the status messages
                            .UseUrls($"http://*:{matchServerConfig.WebHostPort}")
                            .UseConfiguration(config)
                            .UseStartup<MatchServerStartup>()
                            .ConfigureLogging(builder =>
                            {
                                builder.ClearProviders();
                                builder.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
                            })
                            .UseNLog()
                            .Build().RunAsync()
                    );
                }

                var lobbyServerConfig = config.GetSection(Config.LobbyServer).Get<LobbyServerConfig>();
                if (lobbyServerConfig.Active == true)
                {
                    serverTasks.Add(
                        WebHost.CreateDefaultBuilder(args)
                            .SuppressStatusMessages(true) //disable the status messages
                            .UseUrls($"http://*:{lobbyServerConfig.Port}")
                            .UseConfiguration(config)
                            .UseStartup<LobbyServerStartup>()
                            .ConfigureLogging(builder =>
                            {
                                builder.ClearProviders();
                                builder.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
                            })
                            .UseNLog()
                            .Build().RunAsync()
                    );
                }

                var battleServerConfig = config.GetSection(Config.BattleServer).Get<BattleServerConfig>();
                if (battleServerConfig.Active == true)
                {
                    var server = new Battle.BattleServer(battleServerConfig);

                    AppDomain.CurrentDomain.ProcessExit += server.OnProcessExit;
                    AppDomain.CurrentDomain.UnhandledException += server.OnProcessExit;
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                    serverTasks.Add(server.Run());
                }
                var socialServerConfig = config.GetSection(Config.SocialServer).Get<SocialServerConfig>();
                if (socialServerConfig.Active == true)
                {
                    var server = new Social.SocialServer(socialServerConfig);
                    AppDomain.CurrentDomain.ProcessExit += server.OnProcessExit;
                    AppDomain.CurrentDomain.UnhandledException += server.OnProcessExit;
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                    serverTasks.Add(server.Run());
                }
                while (true)
                {
                    bool unhandledException = false;
                    for (int i = 0; i < serverTasks.Count; ++i)
                    {
                        Task task = serverTasks[i];
                        if (task.IsFaulted == true)
                        {
                            _logger.Fatal(task.Exception);
                            unhandledException = true;
                            break;
                        }

                        if (task.IsCompleted == true)
                        {
                            serverTasks.Remove(task);
                            break;
                        }
                    }

                    if (unhandledException == true || serverTasks.Count == 0)
                    {
                        break;
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal("UnhandledException", e);
        }
    }
}