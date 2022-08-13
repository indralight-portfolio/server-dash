using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Dash.Model.Rdb;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    public class PartyConnect : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return nameof(Code); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return Code.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid() => true;

        public PartyConnect() { }
        public PartyConnect(string code, int serial, string endpoint, string serverUUID)
        {
            Code = code;
            PartySerial = serial;
            Endpoint = endpoint;
            ServerUUID = serverUUID;
            SetUpdatedTime(DateTime.UtcNow);
        }
        [Column]
        [KeyColumn]
        public string Code{ get; set; }
        [Column]
        public int PartySerial { get; set; }
        [Column]
        public string Endpoint { get; set; }
        [Column]
        public string ServerUUID { get; set; }
        [Column]
        public DateTime UpdatedTime { get; set; }

        public void SetUpdatedTime(DateTime time) { UpdatedTime = new SqlDateTime(time).Value; } // must use Utc
    }
}
