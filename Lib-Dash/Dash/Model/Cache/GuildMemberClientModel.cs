using Dash.Model.Rdb;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    [MessagePackObject()]
    public class GuildMemberClientModel : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidGuild); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("@OidAccount", keys[0]) };
        }

        public string GetMainKey() { return OidGuild.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { OidAccount.ToString() };
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        [Key(0)]
        [Column]
        [KeyColumn]
        public int OidGuild { get; set; }
        [Key(1)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [Key(2)]
        [Column]
        public byte Grade { get; set; }
        [Key(3)]
        public string Nickname;
        [Key(4)]
        public DateTime LatestLogon;

        public static string Query = $"SELECT a.OidAccount, a.OidGuild, a.Grade, b.Nickname, b.LatestLogon" +
            " FROM GuildMember a" +
            " INNER JOIN Account b ON a.OidAccount = b.OidAccount" +
            " WHERE a.OidGuild = @OidGuild";
        public static List<KeyValuePair<string, object>> QueryParam(int oidGuild)
        {
            return new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>($"@{nameof(OidGuild)}", oidGuild) };
        }
    }
}
