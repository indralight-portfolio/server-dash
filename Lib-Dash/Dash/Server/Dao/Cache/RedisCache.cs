#if Common_Server
using System;
using System.Collections.Generic;
using System.Net;
using Dash.Model.Rdb;
using StackExchange.Redis;
using NLog;
using Dash.Server.Dao.Cache.Transaction;
using Dash.Server.Dao.Model;

namespace Dash.Server.Dao.Cache
{
    public abstract class RedisCache
    {
        private readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public ConnectionMultiplexer RedisClient => _redisClient;        
        protected readonly IMemCache _memCache;

        protected ConnectionMultiplexer _redisClient;

        protected RedisCache(ConnectionMultiplexer redisConnectionMultiplexer, IDatabase redisDb)
        {
            _redisClient = redisConnectionMultiplexer;            
            _memCache = new RedisMemCache(redisDb);
        }

        public IMemCache GetMemCache()
        {            
            return _memCache;
        }
    }

    public class MonitorCache : RedisCache
    {
        private readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public static RedisCache Instance { get; private set; }
        public static void Init(ConnectionMultiplexer redisConnectionMultiplexer, IDatabase redisDb)
        {
            Instance = new MonitorCache(redisConnectionMultiplexer, redisDb);
        }

        protected MonitorCache(ConnectionMultiplexer redisConnectionMultiplexer, IDatabase redisDb) : base(redisConnectionMultiplexer, redisDb) { }
    }
}
#endif