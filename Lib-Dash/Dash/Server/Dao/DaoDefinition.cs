#if Common_Server
using Common.Utility;
using Dash.Model.Cache;
using Dash.Model.Rdb;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Dash.Server.Dao.Connector;

namespace Dash.Server.Dao
{
    public enum SerializeType
    {
        Json = 0,
        MessagePack,
    }

    public static class DaoDefinition
    {
        public class DefinitionContext
        {
            public bool UseDB = true;
            public bool UseRedis = true;
            public DbSchema dbSchema = DbSchema.GameDB;
            public bool IsMultipleDbModel;
            public bool tableMapped = true;
            public bool IsKeyExpire = true;
            public int? KeyExpireSeconds;
            public SerializeType SerializeType = SerializeType.MessagePack;
        }

        public static readonly Dictionary<Type, DefinitionContext> Models = new Dictionary<Type, DefinitionContext>();
        private static ILogger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        static DaoDefinition()
        {
            // SubKey가 존재할 경우, MultipleDBCache.
            object[] tempParam = new object[] { null, null, null, null, null };
            foreach (Type derivedType in DerivedTypeCache.GetDerivedTypes(typeof(Common.Model.IModel), Assembly.GetAssembly(typeof(DaoDefinition))))
            {
                //_logger.Info($"[{derivedType}] / {derivedType.BaseType} / {derivedType.BaseType is System.Object}");                
                var attributes = Attribute.GetCustomAttributes(derivedType);
                if (attributes.Any(a => a is NonDaoModelAttribute) == true) { continue; }

                bool tableMapped = true;
                if (attributes.Any(a=> a is NotTableMappedAttribute) == true) { tableMapped = false; }

                MethodInfo methodInfo =
                    derivedType.GetMethod("MakeSubKeysWithName", BindingFlags.Public | BindingFlags.Static);
                if (methodInfo == null)
                {
                    _logger.Error($"DaoDefinition[{derivedType}] MakeSubKeysWithName method not found!");
                    continue;
                }

                bool IsMultipleDbModel;
                bool isAutoInMainKey;

                PropertyInfo p1 = derivedType.GetProperty(nameof(IsMultipleDbModel), BindingFlags.Public | BindingFlags.Static);
                if (p1 == null)
                {
                    throw new Exception($"IModel[{derivedType}] is not valid");
                }
                IsMultipleDbModel = (bool)p1.GetValue(null);

                Models.Add(derivedType, new DefinitionContext()
                {
                    IsMultipleDbModel = IsMultipleDbModel,
                    tableMapped = tableMapped,
                });
            }

            // Model별로 Redis를 사용여부률 지정할 수 있다.
            // Model별로 Cache Expire 를 다르게 지정할 수 있다.
            // Models[typeof(Auth)].UseRedis = false;
            Models[typeof(ShopReceipt)].UseRedis = false;
            Models[typeof(Coupon)].UseRedis = false;
            Models[typeof(CouponUse)].UseRedis = false;
            Models[typeof(SearchPlayerModel)].UseRedis = false;
            Models[typeof(IapReceipt)].UseRedis = false;
            Models[typeof(GachaHistory)].UseRedis = false;
            Models[typeof(ReservedMailSent)].UseRedis = false;
            Models[typeof(GameEvent)].UseRedis = false;

            Models[typeof(FriendClientCacheModel)].KeyExpireSeconds = 60;
            Models[typeof(OpenQuestCacheModel)].UseDB = false;

            // LogDB 모델
            Models[typeof(GameLog)].dbSchema = DbSchema.LogDB;
            Models[typeof(GameLog)].UseRedis = false;
        }
    }
}
#endif