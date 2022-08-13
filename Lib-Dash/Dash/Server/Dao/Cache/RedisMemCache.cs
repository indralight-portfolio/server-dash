#if Common_Server
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Dash.Server.Dao.Cache
{
    public class RedisMemCache : IMemCache
    {
        private readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        
        private readonly IDatabase _redisClient;
        
        public RedisMemCache(IDatabase redisClient)
        {
            _redisClient = redisClient;
        }
        
        public async Task<long> ListLeftPush(string key, string value)
        {
            long length = await _redisClient.ListLeftPushAsync(key, value);
            if (length <= 0)
            {
                _logger.Fatal($"ListLeftPush failed. Key : {key}, Value : {value}");
            }

            return length;
        }

        public async Task<long> ListRightPush(string key, string value)
        {
            long length = await _redisClient.ListRightPushAsync(key, value);
            if (length <= 0)
            {
                _logger.Fatal($"ListRightPush failed. Key : {key}, Value : {value}");
            }

            return length;
        }

        public async Task<string> ListLeftPop(string key)
        {
            RedisValue value = await _redisClient.ListLeftPopAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger.Error($"ListLeftPop failed. Key dosn't exist. Key : {key}");
            }

            return value.ToString();
        }

        public async Task<string> ListRightPop(string key)
        {
            RedisValue value = await _redisClient.ListRightPopAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger.Error($"ListRightPop failed. Key dosn't exist. Key : {key}");
            }

            return value.ToString();
        }

        public async Task<string> HashGet(string key, string subkey)
        {
            var result = await _redisClient.HashGetAsync(key, subkey);
            if (result.IsNull == true)
            {
                return null;
            }

            return result;
        }
        public async Task<HashEntry[]> HashGetAll(string key)
        {
            var result = await _redisClient.HashGetAllAsync(key);
            if (result == null)
            {
                return null;
            }

            return result;
        }
        public async Task<T> HashGetAll<T>(string key) where T : class, Common.Model.IModel
        {
            var result = await _redisClient.HashGetAllAsync(key);
            if (result == null)
            {
                return default(T);
            }
            return Model.ModelConverter<T>.FromHashEntry(result);
        }
        public async Task<string> StringGet(string key)
        {
            var result = await _redisClient.StringGetAsync(key);
            if (result.IsNull)
            {
                return null;
                
            }
            return result;
        }
        
        public async Task<bool> StringSet(string key, string value)
        {
            return await _redisClient.StringSetAsync(key, value);
        }

        public async Task<bool> StringSet(string key, string value, long expireSeconds)
        {
            return await _redisClient.StringSetAsync(key, value, TimeSpan.FromSeconds(expireSeconds));
        }

        public async Task<bool> Del(string key)
        {
            return await _redisClient.KeyDeleteAsync(key);
        }

        public async Task<bool> HashSet(string key, HashEntry[] entries)
        {
            await _redisClient.HashSetAsync(key, entries);
            return true;
        }
        public async Task<bool> HashSet(string key, List<KeyValuePair<string, string>> values)
        {
            List<HashEntry> hashEntries = new List<HashEntry>();
            for (int index = 0; index < values.Count; ++index)
            {
                hashEntries.Add(new HashEntry(values[index].Key, values[index].Value));
            }
            
            await _redisClient.HashSetAsync(key, hashEntries.ToArray());
            return true;
        }

        public async Task<bool> HashSet(string key, string field, string value)
        {
            return await _redisClient.HashSetAsync(key, field, value);
        }
        

        public async Task<long> Incr(string key)
        {
            return await _redisClient.StringIncrementAsync(key);
        }

        public async Task<bool> KeyExpire(string key, TimeSpan time)
        {
            return await _redisClient.KeyExpireAsync(key, time, CommandFlags.FireAndForget);
        }

        public async Task<double> Zincrby(string key, string member, int score)
        {
            return await _redisClient.SortedSetIncrementAsync(key, member, score);
        }
        public async Task<bool> Zadd(string key, string member, double score)
        {
            return await _redisClient.SortedSetAddAsync(key, member, score, CommandFlags.None);
        }
        public async Task<long> Zadd(string key, SortedSetEntry[] entries)
        {
            return await _redisClient.SortedSetAddAsync(key, entries, CommandFlags.None);
        }
        public async Task<long?> Zrank(string key, string member, Order order)
        {
            return await _redisClient.SortedSetRankAsync(key, member, order);
        }
        public async Task<SortedSetEntry[]> Zrange(string key, long start, long stop, Order order)
        {
            return await _redisClient.SortedSetRangeByRankWithScoresAsync(key, start, stop,  order);
        }
        public async Task<bool> Zdel(string key, string member)
        {
            return await _redisClient.SortedSetRemoveAsync(key, member);
        }
        public async Task<long> Zlength(string key)
        {
            return await _redisClient.SortedSetLengthAsync(key);
        }
        public async Task<bool> HashSet<T>(T model) where T : class, Common.Model.IModel
        {
            await _redisClient.HashSetAsync(nameof(T), Model.ModelConverter<T>.ToHashEntries(model));
            return true;
        }
        public async Task<List<string>> GetKeys(string searchKey)
        {
            int nextCursor = 0;
            List<string> keys = new List<string>();
            do
            {
                var result = await _redisClient.ExecuteAsync("scan", nextCursor.ToString(), "MATCH", $"{searchKey}:*", "COUNT", "1000");
                var innerResult = (RedisResult[])result;
                nextCursor = int.Parse((string)innerResult[0]);
                keys.AddRange(((string[])innerResult[1]).ToList());
            }
            while (nextCursor != 0);
            return keys;
        }

    }
}
#endif