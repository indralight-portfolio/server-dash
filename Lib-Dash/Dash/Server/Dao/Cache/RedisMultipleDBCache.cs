#if Common_Server
using Dash.Server.Dao.Cache.Transaction;
using Dash.Server.Dao.Model;
using Dash.Server.Dao.PreparedStatement;
using Microsoft.EntityFrameworkCore;
//using MySqlConnector;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dash.Server.Dao.Cache
{
    public class RedisMultipleDBCache<T> : IMultipleDBCache<T> where T : class, Common.Model.IModel
    {
        private readonly AbstractDao _db = null;
        private readonly IDatabase _redisClient = null;
        private readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private readonly bool _tableMapped = true;
        private readonly TimeSpan? _keyExpiry = null;

        public RedisMultipleDBCache(AbstractDao db, IDatabase cacheClient, bool tableMapped, bool isUseKeyExpire, int expireSeconds)
        {
            this._db = db;
            this._redisClient = cacheClient;
            _tableMapped = tableMapped;

            if (isUseKeyExpire == true)
            {
                _keyExpiry = new TimeSpan(TimeSpan.TicksPerSecond * expireSeconds);
            }

            if (_db != null && _tableMapped == true)
            {
                GameDao.ValidateSchema<T>(_db, _logger);
            }
        }

        private readonly string __GetAllQuery = QueryGenerator<T>.SelectQuery + QueryGenerator<T>.WhereMainKeyColumn;
        private async Task<List<T>> GetAllFromDBSetCache(string mainKey)
        {
            string mainKeyColumn = QueryGenerator<T>.MainKeyColumn;
            List<KeyValuePair<string, object>> param = new List<KeyValuePair<string, object>>();
            param.Add(new KeyValuePair<string, object>($"@{mainKeyColumn}", mainKey));

            if (_db != null && _tableMapped == true)
                return await GetListFromDBQuerySetCache(mainKey, __GetAllQuery, param);
            else
                return default;
        }
        private async Task<List<T>> GetListFromDBQuerySetCache(string mainKey, string query, List<KeyValuePair<string, object>> param)
        {
            var result = await GetListFromDBQuery(query, param);
            await SetListCache(mainKey, result, true);
            return result;
        }
        public async Task<List<T>> GetListFromDBQuery(string query, List<KeyValuePair<string, object>> param)
        {
            if (_db == null) return default;

            var result = await _db.FromSql<T>(query, param);
            return result;
        }

        public async Task<T> GetFromDBQuery(string query, List<KeyValuePair<string, object>> param)
        {
            if (_db == null) return default;

            var result = (await _db.FromSql<T>(query, param)).FirstOrDefault();
            return result;
        }

        public async Task<int> CountQuery(string query, List<KeyValuePair<string, object>> param)
        {
            if (_db != null && _tableMapped == true)
            {
                return await _db.ExecuteCount(query, param);
            }
            return 0;
        }

        public async Task<List<T>> GetAll()
        {
            var cacheResult = await GetAllCache();
            if (cacheResult != null && cacheResult.Count > 0)
            {
                return cacheResult;
            }

            var query = QueryGenerator<T>.SelectQuery;
            List<KeyValuePair<string, object>> param = new List<KeyValuePair<string, object>>();
            var result = await GetListFromDBQuery(query, param);

            await SetListCache(result);

            return result?.ToList();
        }
        public async Task<List<T>> GetAll(string mainKey)
        {
            var cacheResult = await GetAllCache(mainKey);
            if (cacheResult != null && cacheResult.Count > 0)
            {
                return cacheResult;
            }

            var result = await GetAllFromDBSetCache(mainKey);

            return result?.ToList();
        }
        public async Task<List<T>> GetAll(ulong mainKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await GetAll(mainKey.ToString());
        }

        public async Task<List<T>> GetList(string mainKey, string[] subKeys)
        {
            // subKeys 는 1depth subkey 이다.
            var cacheResult = await GetListCache(mainKey, subKeys);
            if (cacheResult != null && cacheResult.Count > 0)
            {
                return cacheResult;
            }

            // Cache 에 없다면 DB에서 GetAll 을 한 후 Filter 하여 반환한다.
            // 데이터 무결성을 위해 Cache 에서 mainKey 단위로 갱신한다.
            var result = await GetAllFromDBSetCache(mainKey);
            return result?.Where(x => subKeys.Contains(x.GetSubKeys()[0])).ToList();
        }
        public async Task<List<T>> GetList(ulong mainKey, string[] subKeys)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await GetList(mainKey.ToString(), subKeys);
        }

        public async Task<List<T>> GetListQuery(string mainKey, string query, List<KeyValuePair<string, object>> param)
        {
            var cacheResult = await GetAllCache(mainKey);
            if (cacheResult != null && cacheResult.Count > 0)
            {
                return cacheResult;
            }

            var result = await GetListFromDBQuerySetCache(mainKey, query, param);

            return result?.ToList();
        }
        public async Task<List<T>> GetListQuery(ulong mainKey, string query, List<KeyValuePair<string, object>> param)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await GetListQuery(mainKey.ToString(), query, param);
        }

        // subKey must be given in the form of key value pair. ex : {("@strMonsterId", "101"), ("?","?"), ...}
        public async Task<T> Get(string mainKey, List<KeyValuePair<string, object>> subKeyList)
        {
            string subKey = string.Join(":", subKeyList.Select(x => x.Value.ToString()));

            var cacheResult = await GetCache(mainKey, subKey);
            if (cacheResult != null)
            {
                return cacheResult;
            }

            // Cache 에 없다면 DB에서 GetAll 을 한 후 Filter 하여 반환한다.
            // 데이터 무결성을 위해 Cache 에서 mainKey 단위로 갱신한다.
            var resultAll = await GetAllFromDBSetCache(mainKey);
            if (resultAll == null)
            {
                return default;
            }
            var result = resultAll.FirstOrDefault(x => ModelConverter<T>.GetHashKeyFromSubKeys(x) == subKey);

            return result;
        }
        public async Task<T> Get(ulong mainKey, List<KeyValuePair<string, object>> subKeyList)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await Get(mainKey.ToString(), subKeyList);
        }

        public async Task<bool> SetList(List<T> values, TransactionTask transaction = null)
        {
            if (values.Count == 0)
            {
                throw new ArgumentNullException("No object : " + values.Count);
            }

            string mainKey = values[0].GetMainKey();

            if (_db != null && _tableMapped == true)
            {
                if (QueryGenerator<T>.MultipleInsertQueries.TryGetValue(values.Count, out string query) == false)
                {
                    query = QueryGenerator<T>.CreateMultipleInsertQuery(values.Count);
                }

                var param = new List<KeyValuePair<string, object>>();
                for (int i = 0; i < values.Count; ++i)
                {
                    param.AddRange(ModelConverter<T>.GetAllParams(values[i], i.ToString()));
                }

                if (transaction != null)
                {
                    var task = new MultipleCacheTask(typeof(T));
                    task.AssignSetList(values);
                    transaction.Add(query, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(query, param);
                if (result != values.Count)
                {
                    _logger.Fatal($"insert error. query: {query} | {string.Join(",", param)}");
                    return false;
                }
            }

            await SetListCache(mainKey, values);
            return true;
        }

        private readonly string __SetQuery = QueryGenerator<T>.InsertQuery;
        public async Task<bool> Set(T value, TransactionTask transaction = null)
        {
            string mainKey = value.GetMainKey();

            if (_db != null && _tableMapped == true)
            {
                var param = ModelConverter<T>.GetAllParams(value);

                if (transaction != null)
                {
                    var task = new MultipleCacheTask(typeof(T));
                    task.AssignSet(value);
                    transaction.Add(__SetQuery, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(__SetQuery, param);
                if (result != 1)
                {
                    _logger.Fatal($"insert error. query: {__SetQuery} | {string.Join(",", param)}");
                    return false;
                }
            }

            await SetCache(mainKey, value);
            return true;
        }

        private readonly string __ChangeQuery = QueryGenerator<T>.UpdateQuery + QueryGenerator<T>.WhereKeyColumns;
        public async Task<bool> Change(T value, TransactionTask transaction = null)
        {
            string mainKey = value.GetMainKey();

            if (_db != null && _tableMapped == true)
            {
                var param = ModelConverter<T>.GetNonKeyParams(value);
                param.AddRange(ModelConverter<T>.GetKeyParams(value));

                if (transaction != null)
                {
                    var task = new MultipleCacheTask(typeof(T));
                    task.AssignSet(value);
                    transaction.Add(__ChangeQuery, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(__ChangeQuery, param);
                if (result == 0)
                {
                    _logger.Fatal($"update error. query: {__ChangeQuery} | {string.Join(",", param)}");
                    return false;
                }
            }

            await SetCache(mainKey, value);
            return true;
        }

        private readonly string __SetOrChangeQuery = QueryGenerator<T>.InsertOrUpdateQuery;
        public async Task<bool> SetOrChange(T value, TransactionTask transaction = null)
        {
            string mainKey = value.GetMainKey();

            if (_db != null && _tableMapped == true)
            {
                var param = ModelConverter<T>.GetAllParams(value);

                if (transaction != null)
                {
                    var task = new MultipleCacheTask(typeof(T));
                    task.AssignSet(value);
                    transaction.Add(__SetOrChangeQuery, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(__SetOrChangeQuery, param);
                if (result < 1 || result > 2)
                {
                    _logger.Fatal($"insertOrUpdate error. query: {__SetOrChangeQuery} | {string.Join(",", param)}");
                    return false;
                }
            }

            await SetCache(mainKey, value);
            return true;
        }

        private readonly string __DelAllQuery = QueryGenerator<T>.DeleteQuery + QueryGenerator<T>.WhereMainKeyColumn;
        public async Task<bool> DelAll(string mainKey, TransactionTask transaction = null)
        {
            if (_db != null && _tableMapped == true)
            {
                var param = new List<KeyValuePair<string, object>>();
                param.Add(new KeyValuePair<string, object>($"@{QueryGenerator<T>.MainKeyColumn}", mainKey));

                if (transaction != null)
                {
                    var task = new MultipleCacheTask(typeof(T));
                    task.AssignDelAll(mainKey);
                    transaction.Add(__DelAllQuery, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(__DelAllQuery, param);
            }

            await DelAllCache(mainKey);
            return true;
        }
        public async Task<bool> DelAll(ulong mainKey, TransactionTask transaction = null)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await DelAll(mainKey.ToString(), transaction);
        }

        public async Task<bool> DelList(List<T> values, TransactionTask transaction = null)
        {
            if (values.Count <= 0)
            {
                throw new ArgumentNullException("No object : " + values.Count);
            }

            string mainKey = values[0].GetMainKey();

            var param = new List<KeyValuePair<string, object>>();
            var subKeys = new List<string>();

            param.Add(ModelConverter<T>.GetMainKeyParam(values[0]));
            for (int i = 0; i < values.Count; ++i)
            {
                var subKeyParams = ModelConverter<T>.GetSubKeyParams(values[i], i.ToString());
                param.AddRange(subKeyParams);
                subKeys.Add(ModelConverter<T>.GetHashKeyFromSubKeys(values[i]));
            }

            if (_db != null && _tableMapped == true)
            {
                if (QueryGenerator<T>.MultipleDeleteQueries.TryGetValue(values.Count, out string query) == false)
                {
                    query = QueryGenerator<T>.CreateMultipleDeleteQuery(values.Count);
                }

                if (transaction != null)
                {
                    var task = new MultipleCacheTask(typeof(T));
                    task.AssignDelList(mainKey, subKeys);
                    transaction.Add(query, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(query, param);
                if (result == 0)
                {
                    _logger.Warn($"delete error. query: {query} + {string.Join(",", param)}");
                    return false;
                }
            }

            await DelListCache(mainKey, subKeys);
            return true;
        }

        private readonly string __DelQuery = QueryGenerator<T>.DeleteQuery + QueryGenerator<T>.WhereKeyColumns;
        public async Task<bool> Del(T value, TransactionTask transaction = null)
        {
            string mainKey = value.GetMainKey();
            string subKey = ModelConverter<T>.GetHashKeyFromSubKeys(value);

            if (_db != null && _tableMapped == true)
            {
                var param = ModelConverter<T>.GetKeyParams(value);

                if (transaction != null)
                {
                    var task = new MultipleCacheTask(typeof(T));
                    task.AssignDelList(mainKey, new List<string> { subKey });
                    transaction.Add(__DelQuery, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(__DelQuery, param);
                if (result == 0)
                {
                    _logger.Fatal($"delete error. query: {__DelQuery} + {string.Join(",", param)}");
                    return false;
                }
            }

            await DelCache(mainKey, subKey);
            return true;
        }

        #region Cache
        private async Task<long> CountCache(string mainKey)
        {
            if (_redisClient == null) { return 0; }
            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                return await _redisClient.HashLengthAsync(redisKey);
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return 0;
        }
        private async Task<long> CountCache(ulong mainKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await CountCache(mainKey.ToString());
        }

        public async Task<List<T>> GetAllCache()
        {
            if (_redisClient == null) { return null; }
            try
            {
                string redisKey = typeof(T).Name;
                var hashEntries = await _redisClient.HashGetAllAsync(redisKey);
                if (hashEntries == null || hashEntries.Length == 0)
                {
                    return null;
                }
                var redisValues = hashEntries.Where(e => e.Value.HasValue == true).Select(e => e.Value).ToList();
                return redisValues.ConvertAll(v => ModelConverter<T>.FromRedisValue(v));
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return null;
        }
        public async Task<List<T>> GetAllCache(string mainKey)
        {
            if (_redisClient == null) { return null; }
            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                var hashEntries = await _redisClient.HashGetAllAsync(redisKey);
                if (hashEntries == null || hashEntries.Length == 0)
                {
                    return null;
                }
                var redisValues = hashEntries.Where(e => e.Value.HasValue == true).Select(e => e.Value).ToList();
                return redisValues.ConvertAll(v => ModelConverter<T>.FromRedisValue(v));
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return null;
        }
        public async Task<List<T>> GetAllCache(ulong mainKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await GetAllCache(mainKey.ToString());
        }

        public async Task<List<T>> GetListCache(string mainKey, string[] subKeys)
        {
            if (_redisClient == null) { return null; }

            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                var hashKeys = Array.ConvertAll(subKeys, i => (RedisValue)i);
                var redisValues_ = await _redisClient.HashGetAsync(redisKey, hashKeys);
                if (redisValues_ == null || redisValues_.Length == 0)
                {
                    return null;
                }
                var redisValues = redisValues_.Where(v => v.HasValue == true).ToList();
                return redisValues.ConvertAll(v => ModelConverter<T>.FromRedisValue(v));
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return null;
        }
        public async Task<List<T>> GetListCache(ulong mainKey, string[] subKeys)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await GetListCache(mainKey.ToString(), subKeys);
        }

        public async Task<T> GetCache(string mainKey, string subKey)
        {
            if (_redisClient == null) { return default; }
            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                var redisValue = await _redisClient.HashGetAsync(redisKey, subKey);
                if (redisValue.HasValue == true)
                {
                    return ModelConverter<T>.FromRedisValue(redisValue);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return default;
        }
        public async Task<T> GetCache(ulong mainKey, string subKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await GetCache(mainKey.ToString(), subKey);
        }

        public async Task<bool> SetListCache(List<T> values)
        {
            if (_redisClient == null) { return false; }
            if (values == null || values.Count == 0)
            {
                return false;
            }
            try
            {
                string redisKey = typeof(T).Name;
                var hashEntries = values.ConvertAll(v => new HashEntry(ModelConverter<T>.GetHashKeyFromKeys(v), ModelConverter<T>.ToRedisValue(v))).ToArray();

                await _redisClient.HashSetAsync(redisKey, hashEntries);
                await ExpireCache(redisKey);
                return true;
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }
            return false;
        }
        public async Task<bool> SetListCache(string mainKey, List<T> values, bool init = false)
        {
            if (_redisClient == null) { return false; }
            if (values == null || values.Count == 0)
            {
                return false;
            }
            // IsAutoIncKeysValid() 가 false 이면 autInc 된 키 값이 없으므로 mainKey를 지운다.
            if (values.Any(x => x.IsAutoIncKeysValid() == false)) { await DelAllCache(mainKey); return false; }
            if (init == false)
            {
                // 데이터 무결성을 위해 Cache 에서 mainKey 의 hashEntries 갯수 가 0 이면 Set하지 않는다.
                if (await CountCache(mainKey) == 0) return false;
            }

            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                var hashEntries = values.ConvertAll(v => new HashEntry(ModelConverter<T>.GetHashKeyFromSubKeys(v), ModelConverter<T>.ToRedisValue(v))).ToArray();

                await _redisClient.HashSetAsync(redisKey, hashEntries);
                await ExpireCache(redisKey);
                return true;
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }
            return false;
        }
        public async Task<bool> SetListCache(ulong mainKey, List<T> values, bool init = false)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await SetListCache(mainKey.ToString(), values, init);
        }

        public async Task<bool> SetCache(string mainKey, T value)
        {
            if (_redisClient == null || value == null) { return false; }
            // IsAutoIncKeysValid() 가 false 이면 autInc 된 키 값이 없으므로 mainKey를 지운다.
            if (value.IsAutoIncKeysValid() == false) { return await DelAllCache(mainKey); return false; }
            // 데이터 무결성을 위해 Cache 에서 mainKey 의 hashEntries 갯수 가 0 이면 Set하지 않는다.
            if (await CountCache(mainKey) == 0) return false;
            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;

                await _redisClient.HashSetAsync(redisKey, ModelConverter<T>.GetHashKeyFromSubKeys(value), ModelConverter<T>.ToRedisValue(value));
                await ExpireCache(redisKey);
                return true;
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }
            return false;
        }
        public async Task<bool> SetCache(ulong mainKey, T value)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await SetCache(mainKey.ToString(), value);
        }

        public async Task<bool> DelAllCache(string mainKey)
        {
            if (_redisClient == null) { return false; }
            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                bool cacheResult = await _redisClient.KeyDeleteAsync(redisKey);
                if (cacheResult == false)
                {
                    _logger.Warn($"cache doesn't have key: {redisKey}");
                }

                return cacheResult;
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return false;
        }
        public async Task<bool> DelAllCache(ulong mainKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await DelAllCache(mainKey.ToString());
        }

        public async Task<long> DelListCache(string mainKey, List<string> subKeys)
        {
            if (_redisClient == null) { return 0; }
            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                long cacheResult = await _redisClient.HashDeleteAsync(redisKey, subKeys.ConvertAll(v => (RedisValue)v).ToArray());
                if (cacheResult != subKeys.Count)
                {
                    _logger.Warn($"cache doesn't have subkey. requested: {subKeys.Count}. deleted: {cacheResult}");
                }

                return cacheResult;
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return 0;
        }
        public async Task<long> DelListCache(ulong mainKey, List<string> subKeys)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await DelListCache(mainKey.ToString(), subKeys);
        }

        public async Task<bool> DelCache(string mainKey, string subKey)
        {
            if (_redisClient == null) { return false; }
            if (string.IsNullOrEmpty(subKey) == true)
            {
                throw new ArgumentException("Invalid subkey count : " + mainKey);
            }

            string redisKey = typeof(T).Name + ":" + mainKey;
            try
            {
                bool cacheResult = await _redisClient.HashDeleteAsync(redisKey, subKey);
                if (cacheResult == false)
                {
                    _logger.Warn("cache doesn't have key: " + redisKey + "," + subKey);
                }

                return cacheResult;
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return false;
        }
        public async Task<bool> DelCache(ulong mainKey, string subKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid parameter key : " + mainKey);
            }

            return await DelCache(mainKey.ToString(), subKey);
        }

        public async Task<bool> ExpireCache(string mainKey, TimeSpan expiry)
        {
            string redisKey = typeof(T).Name + ":" + mainKey;
            return await _redisClient.KeyExpireAsync(redisKey, expiry);
        }
        public async Task<bool> ExpireCache(ulong mainKey, TimeSpan expiry)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await ExpireCache(mainKey.ToString(), expiry);
        }

        private async Task<bool> ExpireCache(string redisKey)
        {
            if (_keyExpiry == null)
            {
                return true;
            }

            return await _redisClient.KeyExpireAsync(redisKey, _keyExpiry, CommandFlags.FireAndForget);
        }
        #endregion
    }
}
#endif