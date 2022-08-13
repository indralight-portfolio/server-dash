using Dash.Model.Rdb;
using Dash.Types;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    [MessagePackObject()]
    public class SearchPlayerModel : Common.Model.IModel
    {
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

        [Column]
        [KeyColumn]
        [Key(0)]
        public ulong OidAccount { get; set; }
        [Column]
        [Key(1)]
        public string Nickname { get; set; }
        [Column]
        [Key(2)]
        public byte Level { get; set; }
        [Column]
        [Key(3)]
        public int DisplayCharacterId { get; set; }
        [Column]
        [Key(4)]
        public DateTime LatestLogon { get; set; }

        public static string Query = $"SELECT OidAccount," +
            " Level," +
            " Nickname," +
            " LatestLogon," +
            " DisplayCharacterId" +
            " FROM Account" +
            " WHERE Nickname LIKE @Nickname" +
            " ORDER BY LatestLogon DESC" +
            " LIMIT 100";
        public static List<KeyValuePair<string, object>> QueryParam(string nickname)
        {
            return new List<KeyValuePair<string, object>> { 
                new KeyValuePair<string, object>($"@{nameof(Nickname)}", nickname),
            };
        }
    }
}
