#if Common_Server
using Dash.Server.Dao.Model;
using Dash.Server.Dao.Cache.Transaction;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Dash.Server.Dao.Cache
{
    public interface IMultipleDBCache<T> : IDBCacheController
    {
        Task<T> GetFromDBQuery(string query, List<KeyValuePair<string, object>> param);
        Task<List<T>> GetListFromDBQuery(string query, List<KeyValuePair<string, object>> param);
        Task<int> CountQuery(string query, List<KeyValuePair<string, object>> param);

        Task<List<T>> GetAll();
        Task<List<T>> GetAll(string mainKey);
        Task<List<T>> GetAll(ulong mainKey);
        Task<List<T>> GetList(string mainKey, string[] subKeys);
        Task<List<T>> GetList(ulong mainKey, string[] subKeys);
        Task<List<T>> GetListQuery(string mainKey, string query, List<KeyValuePair<string, object>> param);
        Task<List<T>> GetListQuery(ulong mainKey, string query, List<KeyValuePair<string, object>> param);

        Task<T> Get(string mainKey, List<KeyValuePair<string, object>> subKeyList);
        Task<T> Get(ulong mainKey, List<KeyValuePair<string, object>> subKeyList);

        Task<bool> SetList(List<T> values, TransactionTask transaction = null);
        Task<bool> Set(T value, TransactionTask transaction = null);

        Task<bool> Change(T newValue, TransactionTask transaction = null);

        Task<bool> SetOrChange(T value, TransactionTask transaction = null);

        Task<bool> DelAll(string mainKey, TransactionTask transaction = null);
        Task<bool> DelAll(ulong mainKey, TransactionTask transaction = null);
        Task<bool> DelList(List<T> values, TransactionTask transaction = null);
        Task<bool> Del(T value, TransactionTask transaction = null);

        Task<bool> SetListCache(string mainKey, List<T> values, bool init = false);
        Task<bool> SetCache(string mainKey, T value);
        Task<bool> DelAllCache(string mainKey);
        Task<long> DelListCache(string mainKey, List<string> subKeys);
        Task<bool> DelCache(string mainKey, string subKey);
        Task<bool> ExpireCache(string mainKey, TimeSpan expiry);
        Task<bool> ExpireCache(ulong mainKey, TimeSpan expiry);
    }
}
#endif