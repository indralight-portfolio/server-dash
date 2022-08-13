using Dash.Model.Rdb;
using Dash.Types;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    [MessagePackObject()]
    public class FriendClientCacheModel : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("@OidFriend", keys[0]) };
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { OidFriend.ToString() };
        }
        public bool IsAutoIncKeysValid() => true;

        [Column]
        [KeyColumn]
        [Key(0)]
        public ulong OidAccount { get; set; }
        [Column]
        [KeyColumn]
        [Key(1)]
        public ulong OidFriend { get; set; }
        [Column]
        [Key(2)]
        public string Nickname { get; set; }
        [Column]
        [Key(3)]
        public string Country { get; set; }
        [Column]
        [Key(4)]
        public int Level { get; set; }
        [Column]
        [Key(5)]
        public int DisplayCharacterId { get; set; }
        [Column]
        [Key(6)]
        public byte InviteState { get; set; }
        [Column]
        [Key(7)]
        public DateTime LatestLogon { get; set; }
        [Column]
        [Key(8)]
        public DateTime CreateTime { get; set; }

        public static string Query = $"SELECT a.OidAccount, a.OidFriend, a.CreateTime, b.Nickname, b.Country, b.Level, b.DisplayCharacterId, a.InviteState, b.LatestLogon" +
            " FROM Friend a " +
            " INNER JOIN Account b ON a.OidFriend = b.OidAccount" +
            " WHERE a.OidAccount = @OidAccount";
        public static List<KeyValuePair<string, object>> QueryParam(ulong oidAccount)
        {
            return new List<KeyValuePair<string, object>> { 
                new KeyValuePair<string, object>($"@{nameof(OidAccount)}", oidAccount),         
            };
        }
    }
}
