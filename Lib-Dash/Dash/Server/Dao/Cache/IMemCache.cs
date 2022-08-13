#if Common_Server
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dash.Server.Dao.Cache
{
    public interface IMemCache
    {
        Task<long> ListLeftPush(string key, string value);
        Task<long> ListRightPush(string key, string value);
        Task<string> ListLeftPop(string key);
        Task<string> ListRightPop(string key);
        Task<string> HashGet(string key, string subkey);
        Task<HashEntry[]> HashGetAll(string key);
        Task<T> HashGetAll<T>(string key) where T : class, Common.Model.IModel;
        Task<string> StringGet(string key);
        Task<bool> StringSet(string key, string value);
        Task<bool> StringSet(string key, string value, long expireSeconds);
        Task<bool> Del(string key);
        Task<bool> HashSet<T>(T model) where T : class, Common.Model.IModel;
        Task<bool> HashSet(string key, HashEntry[] entries);
        Task<bool> HashSet(string key, List<KeyValuePair<string, string>> values);
        Task<bool> HashSet(string key, string field, string value);
        Task<long> Incr(string key);
        Task<bool> KeyExpire(string key, TimeSpan time);
        Task<double> Zincrby(string key, string member, int value);
        Task<bool> Zadd(string key, string member, double score);
        Task<long> Zadd(string key, SortedSetEntry[] entries);
        Task<long?> Zrank(string key, string member, Order order);
        Task<SortedSetEntry[]> Zrange(string key, long start, long stop, Order order);
        Task<bool> Zdel(string key, string member);
        Task<long> Zlength(string key);
        Task<List<string>> GetKeys(string searchKey);
    }
}
#endif