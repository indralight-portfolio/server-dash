#if Common_Server
using Dash.Server.Dao.Cache.Transaction;
using Dash.Server.Dao.Model;
using Dash.Server.Dao.PreparedStatement;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dash.Server.Dao.Cache
{
    public class RedisSingleDBCache<T> : ISingleDBCache<T> where T : class, Common.Model.IModel
    {
        private readonly AbstractDao _db = null;
        private readonly IDatabase _redisClient = null;
        private readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private readonly bool _tableMapped = true;
        private readonly TimeSpan? _keyExpiry = null;

        public RedisSingleDBCache(AbstractDao db, IDatabase redisClient, bool tableMapped, bool isUseKeyExpire, int expireSeconds)
        {
            this._db = db;
            this._redisClient = redisClient;
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

        private readonly string __GetAllQuery = QueryGenerator<T>.SelectQuery;
        private async Task<List<T>> GetAllFromDBSetCache()
        {
            var param = new List<KeyValuePair<string, object>>();
            if (_db != null && _tableMapped == true)
                return await GetListFromDBQuerySetCache(__GetAllQuery, param);
            else
                return default;
        }
        private async Task<List<T>> GetListFromDBQuerySetCache(string query, List<KeyValuePair<string, object>> param)
        {
            var result = await GetListFromDBQuery(query, param);
            await SetListCache(result);
            return result;
        }
        private async Task<List<T>> GetListFromDBQuery(string query, List<KeyValuePair<string, object>> param)
        {
            if (_db == null) return default;

            var result = await _db.FromSql<T>(query, param);
            return result;
        }

        private async Task<T> GetFromDBQuery(string query, List<KeyValuePair<string, object>> param)
        {
            if (_db == null) return default;

            var result = (await _db.FromSql<T>(query, param)).FirstOrDefault();
            return result;
        }
        private readonly string __GetQuery = QueryGenerator<T>.SelectQuery + QueryGenerator<T>.WhereMainKeyColumn;
        private async Task<T> GetFromDBSetCache(string mainKey)
        {
            string mainKeyColumn = QueryGenerator<T>.MainKeyColumn;
            var param = new List<KeyValuePair<string, object>>();
            param.Add(new KeyValuePair<string, object>($"@{mainKeyColumn}", mainKey));

            T result = default;
            if (_db != null && _tableMapped == true)
            {
                result = await GetFromDBQuery(__GetQuery, param);
                await SetCache(result);
            }
            return result;
        }
        private async Task<T> GetFromDBQuerySetCache(string mainKey, string query, List<KeyValuePair<string, object>> param)
        {
            var result = await GetFromDBQuery(query, param);
            await SetCache(result);
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
            return await GetAllFromDBSetCache();
        }
        public async Task<List<T>> GetListQuery(string query, List<KeyValuePair<string, object>> param)
        {
            var cacheResult = await GetListCache();
            if (cacheResult != null && cacheResult.Count > 0)
            {
                return cacheResult;
            }

            var result = await GetListFromDBQuerySetCache(query, param);
            return result;
        }

        public async Task<T> Get(string mainKey)
        {
            var cacheResult = await GetCache(mainKey);
            if (cacheResult != null)
            {
                return cacheResult;
            }

            var result = await GetFromDBSetCache(mainKey);
            return result;
        }
        public async Task<T> Get(ulong mainKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await Get(mainKey.ToString());
        }

        public async Task<T> GetQueryNoCache(string query, List<KeyValuePair<string, object>> param)
        {
            return await GetFromDBQuery(query, param);
        }
        public async Task<T> GetQuery(string mainKey, string query, List<KeyValuePair<string, object>> param)
        {
            var cacheResult = await GetCache(mainKey);
            if (cacheResult != null)
            {
                return cacheResult;
            }
            return await GetFromDBQuerySetCache(mainKey.ToString(), query, param);
        }
        public async Task<T> GetQuery(ulong mainKey, string query, List<KeyValuePair<string, object>> param)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await GetQuery(mainKey.ToString(), query, param);
        }
        public async Task<List<T>> GetListQueryNoCache(string query, List<KeyValuePair<string, object>> param)
        {
            return await GetListFromDBQuery(query, param);
        }

        private readonly string __SetQuery = QueryGenerator<T>.InsertQuery;
        public async Task<bool> Set(T value, TransactionTask transaction = null)
        {
            if (_db != null && _tableMapped == true)
            {
                var param = ModelConverter<T>.GetAllParams(value);
                if (transaction != null)
                {
                    var task = new SingleCacheTask(typeof(T));
                    task.AssignSet(value);
                    transaction.Add(__SetQuery, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(__SetQuery, param);
                if (result == 0)
                {
                    _logger.Fatal($"insert error. query: {__SetQuery} + {string.Join(",", param)}");
                    return false;
                }
            }

            await SetCache(value);
            return true;
        }

        private readonly string __ChangeQuery = QueryGenerator<T>.UpdateQuery + QueryGenerator<T>.WhereKeyColumns;
        public async Task<bool> Change(string mainKey, List<KeyValuePair<string, object>> changeColumns, TransactionTask transaction = null)
        {
            if (changeColumns.Count <= 0)
            {
                throw new ArgumentOutOfRangeException($"Invalid param count {mainKey}");
            }

            HashEntry[] hashEntries = ModelConverter<T>.ToHashEntries(changeColumns);

            if (_db != null && _tableMapped == true)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"UPDATE {typeof(T).Name} SET ");
                sb.Append(string.Join(",", changeColumns.Select(e => $"{e.Key} = @{e.Key}1").ToList()));
                sb.Append(QueryGenerator<T>.WhereMainKeyColumn);
                string query = sb.ToString();

                var param = new List<KeyValuePair<string, object>>();
                param.AddRange(changeColumns.Select(e => new KeyValuePair<string, object>($"@{e.Key}1", e.Value)).ToList());
                param.Add(new KeyValuePair<string, object>($"@{QueryGenerator<T>.MainKeyColumn}", mainKey));

                if (transaction != null)
                {
                    var task = new SingleCacheTask(typeof(T));
                    task.AssignSet(mainKey, hashEntries);
                    transaction.Add(query, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(query, param);
                if (result == 0)
                {
                    _logger.Fatal($"update error. query: {query} | {string.Join(",", param)}");
                    return false;
                }
            }

            await SetCache(mainKey, hashEntries);
            return true;
        }
        public async Task<bool> Change(ulong mainKey, List<KeyValuePair<string, object>> changeColumns, TransactionTask transaction = null)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await Change(mainKey.ToString(), changeColumns, transaction);
        }
        public async Task<bool> Change(T value, TransactionTask transaction = null)
        {
            if (_db != null && _tableMapped == true)
            {
                var param = ModelConverter<T>.GetNonKeyParams(value);
                param.AddRange(ModelConverter<T>.GetKeyParams(value));

                if (transaction != null)
                {
                    var task = new SingleCacheTask(typeof(T));
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

            await SetCache(value);
            return true;
        }
        public async Task<bool> Change(T value, List<KeyValuePair<string, object>> changeColumns, TransactionTask transaction = null)
        {
            string mainKey = value.GetMainKey();
            return await Change(mainKey, changeColumns, transaction);
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
                    var task = new SingleCacheTask(typeof(T));
                    task.AssignSet(value);
                    transaction.Add(__SetOrChangeQuery, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(__SetOrChangeQuery, param);
                if (result == 0)
                {
                    _logger.Fatal($"insert error. query: {__SetOrChangeQuery} + {string.Join(",", param)}");
                    return false;
                }
            }

            await SetCache(value);
            return true;
        }

        private string __DelQuery = QueryGenerator<T>.DeleteQuery + QueryGenerator<T>.WhereMainKeyColumn;
        public async Task<bool> Del(string mainKey, TransactionTask transaction = null)
        {
            if (_db != null && _tableMapped == true)
            {
                var param = new List<KeyValuePair<string, object>>();
                param.Add(new KeyValuePair<string, object>($"@{QueryGenerator<T>.MainKeyColumn}", mainKey));

                if (transaction != null)
                {
                    var task = new SingleCacheTask(typeof(T));
                    task.AssignDel(mainKey);
                    transaction.Add(__DelQuery, param, task);
                    return true;
                }

                int result = await _db.ExecuteSql(__DelQuery, param);
                if (result == 0)
                {
                    _logger.Warn($"delete error. query: {__DelQuery} + {string.Join(",", param)}");
                    return false;
                }
            }

            await DelCache(mainKey);
            return true;
        }
        public async Task<bool> Del(ulong mainKey, TransactionTask transaction = null)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await Del(mainKey.ToString(), transaction);
        }
        public async Task<bool> Del(T value, TransactionTask transaction = null)
        {
            string mainKey = value.GetMainKey();
            return await Del(mainKey, transaction);
        }
        public async Task<bool> Set(string mainKey, HashEntry[] entries)
        {
            return await SetCache(mainKey, entries);
        }
        public async Task<bool> Set(ulong mainKey, HashEntry[] entries)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await Set(mainKey.ToString(), entries);
        }

        #region Redis
        public async Task<List<T>> GetListCache()
        {
            if (_redisClient == null) { return default; }
            string redisKey = typeof(T).Name + "List";
            try
            {
                var redisValues_ = await _redisClient.ListRangeAsync(redisKey);
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
            return default;
        }

        public async Task<T> GetCache(string mainKey)
        {
            if (_redisClient == null) { return default; }
            string redisKey = typeof(T).Name + ":" + mainKey;
            try
            {
                HashEntry[] result = await _redisClient.HashGetAllAsync(redisKey);
                return ModelConverter<T>.FromHashEntry(result);
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }
            return default;
        }
        public async Task<T> GetCache(ulong mainKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await GetCache(mainKey.ToString());
        }

        public async Task<bool> SetListCache(List<T> values)
        {
            if (_redisClient == null) { return false; }
            string redisKey = typeof(T).Name + "List";
            if (values == null || values.Count == 0)
            {
                return false;
            }
            // IsAutoIncrementMainKey 인 경우 cache 를 Set하지 않는다.
            if (values.Any(x => x.IsAutoIncKeysValid() == false)) { return false; }
            try
            {
                await _redisClient.KeyDeleteAsync(redisKey);
                foreach (T value in values)
                {
                    await _redisClient.ListRightPushAsync(redisKey, ModelConverter<T>.ToRedisValue(value));
                }
                await ExpireCache(redisKey);
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }
            return false;
        }

        public async Task<bool> SetCache(T value)
        {
            if (_redisClient == null || value == null) { return false; }
            // IsAutoIncrementMainKey 인 경우 cache 를 Set하지 않는다.
            if (value.IsAutoIncKeysValid() == false) { return false; }
            return await SetCache(value.GetMainKey(), ModelConverter<T>.ToHashEntries(value));
        }
        public async Task<bool> SetCache(string mainKey, HashEntry[] entries)
        {
            if (_redisClient == null) { return false; }
            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                await _redisClient.HashSetAsync(redisKey, entries);
                await ExpireCache(redisKey);
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }
            return true;
        }
        public async Task<bool> SetCache(ulong mainKey, HashEntry[] entries)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await SetCache(mainKey.ToString(), entries);
        }

        public async Task<bool> DelCache(T value)
        {
            if (_redisClient == null) { return false; }
            return await DelCache(value.GetMainKey());
        }
        public async Task<bool> DelCache(string mainKey)
        {
            if (_redisClient == null) { return false; }
            try
            {
                string redisKey = typeof(T).Name + ":" + mainKey;
                return await _redisClient.KeyDeleteAsync(redisKey);
            }
            catch (Exception e)
            {
                _logger.Fatal($"Model<{typeof(T)}> DBCache error : " + e.Message);
                _logger.Fatal(e.StackTrace);
            }

            return false;
        }
        public async Task<bool> DelCache(ulong mainKey)
        {
            if (mainKey <= 0)
            {
                throw new ArgumentOutOfRangeException("Invalid mainKey : " + mainKey);
            }

            return await DelCache(mainKey.ToString());
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