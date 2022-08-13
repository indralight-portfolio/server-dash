#if Common_Server
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Dash.Server.Dao.Cache.Transaction
{
    public abstract class AbstractCacheTask
    {
        public enum ControllerType
        {
            SINGLE, MULTIPLE
        }

        public enum ApplyCacheType
        {
            SET, DEL
        }

        public enum UpdateTargetType
        {
            ONE, LIST, ALL
        }

        public enum UpdateColumnType
        {
            ALL, PARTIAL
        }

        private readonly Type _type;

        protected Dictionary<string, object> _cacheParams = new Dictionary<string, object>();
        protected ApplyCacheType _applyCacheType;
        protected UpdateTargetType _updateTargetType;
        protected UpdateColumnType _updateColumnType;

        public abstract ControllerType GetControllerType();
        public Type Type => _type;
        public Dictionary<string, object> CacheParams => _cacheParams;

        public ApplyCacheType GetApplyCacheType() => _applyCacheType;
        public UpdateTargetType GetUpdateTargetType() => _updateTargetType;
        public UpdateColumnType GetUpdateColumnType() => _updateColumnType;

        protected AbstractCacheTask(Type type)
        {
            this._type = type;
            this._updateTargetType = UpdateTargetType.ONE;
            this._updateColumnType = UpdateColumnType.ALL;
        }
    }

    public class SingleCacheTask : AbstractCacheTask
    {
        public SingleCacheTask(Type type) : base(type)
        {
        }

        public override ControllerType GetControllerType()
        {
            return ControllerType.SINGLE;
        }

        public void AssignSet<T>(T newValue)
        {
            _applyCacheType = ApplyCacheType.SET;
            _updateColumnType = UpdateColumnType.ALL;
            _cacheParams.Add("newValue", newValue);
        }
        public void AssignSet(string mainKey, HashEntry[] changeColumns)
        {
            _applyCacheType = ApplyCacheType.SET;
            _updateColumnType = UpdateColumnType.PARTIAL;
            _cacheParams.Add("mainKey", mainKey);
            _cacheParams.Add("changeColumns", changeColumns);
        }

        public void AssignDel(string mainKey)
        {
            _applyCacheType = ApplyCacheType.DEL;
            _cacheParams.Add("mainKey", mainKey);
        }
    }

    public class MultipleCacheTask : AbstractCacheTask
    {
        public MultipleCacheTask(Type type) : base(type)
        {
        }

        public override ControllerType GetControllerType()
        {
            return ControllerType.MULTIPLE;
        }

        public void AssignSet<T>(T newValue)
        {
            _applyCacheType = ApplyCacheType.SET;
            _updateTargetType = UpdateTargetType.ONE;
            _cacheParams.Add("newValue", newValue);
        }

        public void AssignSetList<T>(List<T> newValues)
        {
            _applyCacheType = ApplyCacheType.SET;
            _updateTargetType = UpdateTargetType.LIST;
            _cacheParams.Add("newValues", newValues);
        }

        public void AssignDel(string mainKey, string subKey)
        {
            _applyCacheType = ApplyCacheType.DEL;
            _updateTargetType = UpdateTargetType.ONE;
            _cacheParams.Add("mainKey", mainKey);
            _cacheParams.Add("subKey", subKey);
        }

        public void AssignDelList(string mainKey, List<string> subKeys)
        {
            _applyCacheType = ApplyCacheType.DEL;
            _updateTargetType = UpdateTargetType.LIST;
            _cacheParams.Add("mainKey", mainKey);
            _cacheParams.Add("subKeys", subKeys);
        }

        public void AssignDelAll(string mainKey)
        {
            _applyCacheType = ApplyCacheType.DEL;
            _updateTargetType = UpdateTargetType.ALL;
            _cacheParams.Add("mainKey", mainKey);
        }
    }
}
#endif