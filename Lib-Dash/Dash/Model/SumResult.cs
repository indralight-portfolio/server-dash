#if Common_Server
using System.Collections.Generic;
using Dash.Model.Rdb;

using Dash.Server.Dao.PreparedStatement;

namespace Dash.Model
{
    [NotTableMapped]
    public class SumResult : Common.Model.IModel
    {
        [Column]
        public ulong? Result { get; set; }

        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return null; }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return Result.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid() => true;

        public static string GetQuery(string coulmn, string table_name)
        {
            return $"SELECT SUM({WrapGrave(coulmn)}) AS Result " +
                  $"FROM {WrapGrave(table_name)}";
        }
        private static string WrapGrave(string str)
        {
            return $"`{str}`";
        }
    }
}
#endif