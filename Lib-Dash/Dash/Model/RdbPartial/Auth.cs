using Dash.Model.Service;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dash.Model.Cache;
#if Common_Server
using Dash.Server.Dao.Cache;
#endif

namespace Dash.Model.Rdb
{
    public partial class Auth
    {
        [IgnoreMember, JsonIgnore]
        public AuthType Type => (AuthType)AuthType;

        public Auth(ulong oidAccount, AuthReq authReq)
        {
            OidAccount = oidAccount;
            AuthType = (byte)authReq.AuthType;
            AuthId = authReq.AuthId;
        }
    }

    public static class AuthCacheHelper
    {
#if Common_Server
        public static async Task<Auth> GetHiveAuth(this IMultipleDBCache<Auth> _authCache, ulong oidAccount)
        {
            return await _authCache.Get(oidAccount, Auth.MakeSubKeysWithName((byte)AuthType.Hive));
        }
#endif
    }
}