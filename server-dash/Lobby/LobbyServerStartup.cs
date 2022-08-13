using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Dash.Server.Dao.Cache;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog;
using server_dash.Internal;
using server_dash.Lobby.Services;
using server_dash.Middleware;
using server_dash.Swagger;
using System;
using System.IO;
using System.Reflection;

namespace server_dash.Match
{
    public class LobbyServerStartup
    {
        public readonly string UUID;
        private readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public IConfiguration Configuration { get; }

        public LobbyServerStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string UUID = new ServerUUID().Value;
            var serverConfig = ConfigManager.Get<ServerConfig>(Config.Server);
            string matchServerEndpoint = serverConfig.MatchServerEndpoint;

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(nameof(WebAPI.Lobby), new OpenApiInfo { Title = $"Dash {nameof(WebAPI.Lobby)} API", Version = "v1" });
                c.SwaggerDoc(nameof(WebAPI.Match), new OpenApiInfo { Title = $"Dash {nameof(WebAPI.Match)} API", Version = "v1" });
                c.SwaggerDoc(nameof(WebAPI.Etc), new OpenApiInfo { Title = $"Dash {nameof(WebAPI.Etc)} API", Version = "v1" });
                c.DocumentFilter<DocumentFilter>();
                c.SchemaFilter<SchemaFilter>();
                c.SchemaFilter<AutoRestSchemaFilter>();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            }).AddSwaggerGenNewtonsoftSupport();

            services.AddControllers().AddMvcOptions(option =>
            {
                option.OutputFormatters.Clear();
                option.OutputFormatters.Add(new MessagePackOutputFormatter(MessagePack.MessagePackSerializer.DefaultOptions));
            });

            services.AddControllers().AddNewtonsoftJson()
                .ConfigureApiBehaviorOptions(options =>
                    options.SuppressConsumesConstraintForFormFileParameters = true
                );

            var awsConfig = Configuration.GetSection(Config.AWS).Get<AWSConfig>();
            var awsDefaultOptions = new AWSOptions
            {
                Credentials = new BasicAWSCredentials(awsConfig.Default.AccessKey, awsConfig.Default.SecretKey),
                Region = RegionEndpoint.GetBySystemName(awsConfig.Default.Region),
            };

            services.AddDefaultAWSOptions(awsDefaultOptions);
            services.AddAWSService<IAmazonDynamoDB>();

            var lobbyConfig = ConfigManager.Get<LobbyServerConfig>(Config.LobbyServer); 
            services.AddSingleton(lobbyConfig);
            services.AddSingleton(ConfigManager.Get<CheckConfig>(Config.Check));

            services.AddSingleton(DaoCache.Instance);
            services.AddSingleton<ServiceStateValidator>();
            services.AddSingleton<SessionValidator>();

            services.AddHostedService<ServiceStateResolveService>();
            services.AddHostedService(x => new MonitorService(UUID, x.GetRequiredService<LobbyServerConfig>()));
            var matchServerClient = new MatchServerClient(UUID, ServerIPManager.Instance.Endpoint + ":" + lobbyConfig.Port, Dash.Types.ServiceAreaType.Lobby, matchServerEndpoint);
            services.AddHostedService(x => matchServerClient);
            services.AddSingleton<MatchServerClient>(x => matchServerClient);

            services.AddSingleton<Internal.Services.MailService>();

            services.AddSingleton<AccountService>();
            services.AddSingleton<InventoryService>();
            services.AddSingleton<MailService>();
            services.AddSingleton<RewardService>();
            services.AddSingleton<UpgradeService>();
            services.AddSingleton<BillingService>();
            services.AddSingleton<ShopService>();            
            services.AddSingleton<MatchService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{nameof(WebAPI.Lobby)}/swagger.json", $"Dash {nameof(WebAPI.Lobby)} API v1");
                c.SwaggerEndpoint($"/swagger/{nameof(WebAPI.Match)}/swagger.json", $"Dash {nameof(WebAPI.Match)} API v1");
                c.SwaggerEndpoint($"/swagger/{nameof(WebAPI.Etc)}/swagger.json", $"Dash {nameof(WebAPI.Etc)} API v1");
            });

            var lobbyConfig = app.ApplicationServices.GetService<LobbyServerConfig>();
            var checkConfig = app.ApplicationServices.GetService<CheckConfig>();

            app.UseRouting();

            app.UseMiddleware<LogExceptionMiddleware>();
            app.UseMiddleware<DisableHttpGetMiddleware>();
            app.UseMiddleware<ValidateMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            _logger.Info($"LobbyServer started! {BuildVersion.ToString()}");
        }
    }
}