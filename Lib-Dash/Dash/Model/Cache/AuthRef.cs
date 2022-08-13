using Dash.Model.Rdb;
using System.Collections.Generic;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    [MessagePack.MessagePackObject()]
    public class AuthRef : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return nameof(AuthId); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return AuthId.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid() => true;

        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public string AuthId { get; set; }
        [MessagePack.Key(1)]
        [Column]
        [KeyColumn]
        public byte AuthType { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public ulong OidAccount { get; set; }

        public static string Query = $"SELECT * FROM {nameof(Auth)}" +
                $" WHERE {nameof(AuthId)}=@{nameof(AuthId)}" +
                $" AND {nameof(AuthType)}=@{nameof(AuthType)};";
        public static List<KeyValuePair<string, object>> QueryParam(string authId, byte authType)
        {
            return new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>($"@{nameof(AuthId)}", authId),
                new KeyValuePair<string, object>($"@{nameof(AuthType)}", authType),
            };
        }

        public AuthRef() { }
    }

    public static class AuthRefHelper
    {
        public static Auth ToAuth(this AuthRef authRef)
        {
            if (authRef == null) return null;
            return new Auth
            {
                OidAccount = authRef.OidAccount,
                AuthType = authRef.AuthType,
                AuthId = authRef.AuthId,
            };
        }

        public static AuthRef ToAuthRef(this Auth auth)
        {
            if (auth == null) return null;
            return new AuthRef
            {
                AuthId = auth.AuthId,
                AuthType = auth.AuthType,
                OidAccount = auth.OidAccount,
            };
        }
    }
}
