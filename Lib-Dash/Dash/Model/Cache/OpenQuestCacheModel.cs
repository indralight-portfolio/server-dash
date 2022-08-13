using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dash.Model.Rdb;
using MessagePack;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    [MessagePackObject()]
    public class OpenQuestCacheModel : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("@MissionId", keys[0]) };
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string> { MissionId.ToString() };
        }
        public bool IsAutoIncKeysValid() => true;
        [Column]
        [KeyColumn]
        [Key(0)]
        public ulong OidAccount { get; set; }
        [Column]
        [Key(1)]
        public int MissionId { get; set; }
    }
}
