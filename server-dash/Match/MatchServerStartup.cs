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
using server_dash.Match.Services;
using server_dash.Middleware;
using server_dash.Swagger;
using System;
using System.IO;
using System.Reflection;

namespace server_dash.Match
{
    public class MatchServerStartup
    {
        private readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public IConfiguration Configuration { get; }

        public MatchServerStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(nameof(WebAPI.MatchAdmin), new OpenApiInfo { Title = $"Dash {nameof(WebAPI.MatchAdmin)} API", Version = "v1" });
                c.SchemaFilter<SchemaFilter>();
                c.SchemaFilter<AutoRestSchemaFilter>();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddControllers().AddMvcOptions(option =>
            {
                option.OutputFormatters.Clear();
                option.OutputFormatters.Add(new MessagePackOutputFormatter(MessagePack.MessagePackSerializer.DefaultOptions));
            });

            services.AddControllers().AddNewtonsoftJson()
                .ConfigureApiBehaviorOptions(options =>
                    options.SuppressConsumesConstraintForFormFileParameters = true
                );

            services.AddSingleton(ConfigManager.Get<MatchServerConfig>(Config.MatchServer));
            services.AddSingleton(ConfigManager.Get<CheckConfig>(Config.Check));

            services.AddSingleton(DaoCache.Instance);
            services.AddSingleton<ServiceStateValidator>();
            services.AddSingleton<IHostedService, ServiceStateResolveService>();
            services.AddSingleton<SessionValidator>();

            services.AddSingleton<IHostedService>(x => new MonitorService(new ServerUUID().Value, x.GetRequiredService<MatchServerConfig>()));

            services.AddSingleton<MatchService>();
            services.AddSingleton<ArenaService>();
            services.AddSingleton<SessionService>();
            services.AddSingleton<ServerCoordinator>();
            services.AddSingleton<TransferService>();

            services.AddSingleton<IHostedService, NetServer>();
            services.AddSingleton<IHostedService, FaultyMatchCleaner>();
            services.AddSingleton<MatchSerialProvider>();
            services.AddSingleton<PartySerialProvider>();

            services.AddSingleton<MatchEntityContainer>();
            services.AddSingleton<MatchHolder>();
            services.AddSingleton<ArenaHolder>();
            services.AddSingleton<MatchMakerProvider>();
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
                c.SwaggerEndpoint($"/swagger/{nameof(WebAPI.MatchAdmin)}/swagger.json", $"Dash {nameof(WebAPI.MatchAdmin)} API v1");
            });

            var matchConfig = app.ApplicationServices.GetService<MatchServerConfig>();

            app.UseRouting();

            app.UseMiddleware<LogExceptionMiddleware>();
            app.UseMiddleware<DisableHttpGetMiddleware>();
            app.UseMiddleware<ValidateMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // PartyService에서 참조하는데 미리 touch해서 생성해둠.
            _ = app.ApplicationServices.GetService<MatchService>();

            _logger.Info($"MatchServer started! {BuildVersion.ToString()}");
        }
    }
}