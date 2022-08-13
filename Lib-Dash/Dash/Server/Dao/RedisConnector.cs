#if Common_Server
using System;
using System.Threading.Tasks;
using Common.Log;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using ILogger = NLog.ILogger;

namespace Dash.Server.Dao
{
    public class RedisConnector
    {
        private Lazy<ConnectionMultiplexer> lazyConnection;
        public ConnectionMultiplexer ConnectionMultiplexer { get { return lazyConnection.Value; } }
        public IDatabase Database { get { return ConnectionMultiplexer.GetDatabase(_config.Database); } }
        private RedisConfiguration _config;
        private static readonly ILogger _logger = NLogUtility.GetCurrentClassLogger();
        public RedisConnector(RedisConfiguration config)
        {
            _config = config;
            _config.ConfigurationOptions.CertificateValidation += Common.Utility.SslHelper.CheckServerCertificate;
        }

        public async Task<bool> ConnectAsync()
        {
            ConfigurationOptions redisOptions = null;
            try
            {
                redisOptions = _config.ConfigurationOptions;
                _logger.Info($"connecting to Redis {string.Join(",", redisOptions.EndPoints)} (UseSsl : {redisOptions.Ssl}) (Database : {redisOptions.DefaultDatabase})");

                lazyConnection = new Lazy<ConnectionMultiplexer>(
                    () => { return ConnectionMultiplexer.Connect(redisOptions); });

                //ConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(redisOptions);
                //Database = ConnectionMultiplexer.GetDatabase(_config.Database);
                _logger.Info("Redis connected.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
                throw e;
            }
        }
    }
}
#endif