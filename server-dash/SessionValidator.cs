using Dash.Server.Dao.Cache;
using System;
using System.Threading.Tasks;

namespace server_dash
{
    public struct SessionKey
    {
        public const string RedisKeyPrefix = "sk:";
        public string Key;
        public DateTime Expiry;
    }

    public class SessionValidator
    {
        private IMemCache _memCache;

        public SessionValidator()
        {
            _memCache = DaoCache.Instance.GetMemCache();
        }

        public async Task<bool> Validate(ulong oidAccount, string sessionKeyFromClient)
        {
            string sessionKeyFromRedis = await _memCache.StringGet(SessionKey.RedisKeyPrefix + oidAccount);
            if (sessionKeyFromRedis == null || sessionKeyFromRedis != sessionKeyFromClient)
            {
                return false;
            }

            return true;
        }
    }
}