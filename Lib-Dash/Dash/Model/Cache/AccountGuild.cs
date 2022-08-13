using Dash.Model.Rdb;
using System.Collections.Generic;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    [MessagePack.MessagePackObject()]
    public class AccountGuild : Common.Model.IModel
    {
        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(1)]
        [Column]
        public int OidGuild { get; set; }

        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid() => true;

        public static string Query = $"SELECT {nameof(GuildMember.OidAccount)}, {nameof(GuildMember.OidGuild)} FROM {nameof(GuildMember)} WHERE {nameof(GuildMember.OidAccount)} = @{nameof(GuildMember.OidAccount)}";
        public static List<KeyValuePair<string, object>> QueryParam(ulong oidAccount)
        {
            return new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>($"@{nameof(OidAccount)}", oidAccount) };
        }
    }
}
