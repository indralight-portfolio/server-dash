#if Common_Server
using System;
using Dash.Server.Dao.Cache.Transaction;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dash.Server.Dao.Cache
{
    public interface ISingleDBCache<T> : IDBCacheController
    {
        Task<List<T>> GetAll();
        Task<List<T>> GetListQuery(string query, List<KeyValuePair<string, object>> param);

        Task<T> Get(string mainKey);
        Task<T> Get(ulong mainKey);

        Task<T> GetQueryNoCache(string query, List<KeyValuePair<string, object>> param);
        Task<T> GetQuery(string mainKey, string query, List<KeyValuePair<string, object>> param);
        Task<T> GetQuery(ulong mainKey, string query, List<KeyValuePair<string, object>> param);
        Task<List<T>> GetListQueryNoCache(string query, List<KeyValuePair<string, object>> param);

        Task<bool> Set(T value, TransactionTask transaction = null);

        Task<bool> Change(string mainKey, List<KeyValuePair<string, object>> changeColumns, TransactionTask transaction = null);
        Task<bool> Change(ulong mainKey, List<KeyValuePair<string, object>> changeColumns, TransactionTask transaction = null);
        Task<bool> Change(T value, TransactionTask transaction = null);
        Task<bool> Change(T value, List<KeyValuePair<string, object>> changeColumns, TransactionTask transaction = null);

        Task<bool> SetOrChange(T value, TransactionTask transaction = null);

        Task<bool> Del(string mainKey, TransactionTask transaction = null);
        Task<bool> Del(ulong mainKey, TransactionTask transaction = null);
        Task<bool> Del(T value, TransactionTask transaction = null);

        Task<bool> Set(string mainKey, HashEntry[] entries);
        Task<bool> Set(ulong mainKey, HashEntry[] entries);

        Task<bool> SetCache(T value);
        Task<bool> SetCache(string mainKey, HashEntry[] entries);
        Task<bool> SetCache(ulong mainKey, HashEntry[] entries);
        Task<bool> DelCache(string mainKey);
        Task<bool> ExpireCache(string mainKey, TimeSpan expiry);
        Task<bool> ExpireCache(ulong mainKey, TimeSpan expiry);
        Task<int> CountQuery(string query, List<KeyValuePair<string, object>> param);
    }
}
#endif