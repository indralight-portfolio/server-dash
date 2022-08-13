#if Common_Server
using Dash.Server.Dao.Cache.Transaction;
using NLog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using static Dash.Server.Dao.Connector;

namespace Dash.Server.Dao.Cache
{
    public class DaoCache : RedisCache
    {
        private readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public static DaoCache Instance { get; private set; }

        private Dictionary<Type, IDBCacheController> _controllers = new Dictionary<Type, IDBCacheController>();
        private readonly Dictionary<DbSchema, DBConfiguration> _configurations = new Dictionary<DbSchema, DBConfiguration>();
        private readonly Dictionary<DbSchema, AbstractDao> _daos = new Dictionary<DbSchema, AbstractDao>();

        public AbstractDao GameDao => _daos[DbSchema.GameDB];
        public AbstractDao LogDao => _daos[DbSchema.LogDB];

        public static void Init(RdsConfig rdsConfig, ConnectionMultiplexer redisConnectionMultiplexer, IDatabase redisDb, RedisExConfig redisExConfig)
        {
            Instance = new DaoCache(rdsConfig, redisConnectionMultiplexer, redisDb, redisExConfig);
        }

        private DaoCache(RdsConfig rdsConfig, ConnectionMultiplexer redisConnectionMultiplexer, IDatabase redisDb, RedisExConfig redisExConfig) : base(redisConnectionMultiplexer, redisDb)
        {
            _configurations.Add(DbSchema.GameDB, rdsConfig.GameDB);
            _configurations.Add(DbSchema.LogDB, rdsConfig.LogDB);

            // Dao
            _daos.Add(DbSchema.GameDB, new GameDao(_configurations[DbSchema.GameDB]));
            _daos.Add(DbSchema.LogDB, new GameDao(_configurations[DbSchema.LogDB]));            

            Type redisSingleDbCacheType = typeof(RedisSingleDBCache<>);
            Type redisMultipleDbCacheType = typeof(RedisMultipleDBCache<>);
            foreach (KeyValuePair<Type, DaoDefinition.DefinitionContext> pair in DaoDefinition.Models)
            {
                Type type = pair.Key;
                if (pair.Value.IsMultipleDbModel == true)
                {
                    _controllers.Add(type,
                        (IDBCacheController)Activator.CreateInstance(redisMultipleDbCacheType.MakeGenericType(type), new object[]
                    {
                        pair.Value.UseDB ? _daos[pair.Value.dbSchema] : null,
                        pair.Value.UseRedis ? redisDb : null,
                        pair.Value.tableMapped,
                        pair.Value.IsKeyExpire && redisExConfig.IsKeyExpire,
                        pair.Value.KeyExpireSeconds ?? redisExConfig.KeyExpireSeconds
                    }));
                }
                else
                {
                    _controllers.Add(type,
                        (IDBCacheController)Activator.CreateInstance(redisSingleDbCacheType.MakeGenericType(type), new object[]
                    {
                        pair.Value.UseDB ? _daos[pair.Value.dbSchema] : null, 
                        pair.Value.UseRedis ? redisDb : null,
                        pair.Value.tableMapped,
                        pair.Value.IsKeyExpire && redisExConfig.IsKeyExpire,
                        pair.Value.KeyExpireSeconds ?? redisExConfig.KeyExpireSeconds
                    }));
                }
            }
        }

        public ISingleDBCache<T> GetSingle<T>()
        {
            Type type = typeof(T);
            if (_controllers.ContainsKey(type) == false)
            {
                throw new InvalidOperationException($"can't have {type}");
            }

            return _controllers[type] as ISingleDBCache<T>;
        }

        public IMultipleDBCache<T> GetMultiple<T>()
        {
            Type type = typeof(T);
            if (_controllers.ContainsKey(type) == false)
            {
                throw new InvalidOperationException($"can't have {type}");
            }

            return _controllers[type] as IMultipleDBCache<T>;
        }

        public IDBCacheController Get(Type type)
        {
            if (_controllers.ContainsKey(type) == false)
            {
                throw new InvalidOperationException($"can't have {type}");
            }

            return _controllers[type];
        }

        public TransactionTask Transaction(DbSchema dbSchema = DbSchema.GameDB)
        {
            return new TransactionTask(_daos[dbSchema], this);
        }
    }

    public class RedisExConfig
    {
        public bool IsKeyExpire { get; set; }
        public int KeyExpireSeconds { get; set; }
    }
}
#endif