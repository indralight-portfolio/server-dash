using Dash.Model.Rdb;
using System.Collections.Generic;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    public class TableSchema : Common.Model.IModel
    {
        [Column]        
        public string COLUMN_NAME { get; set; }

        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return null; }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return COLUMN_NAME.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid() => true;

        public static string Query = "SELECT COLUMN_NAME " +
                  "FROM INFORMATION_SCHEMA.COLUMNS " +
                  "WHERE TABLE_SCHEMA=@table_schema " +
                  "AND TABLE_NAME=@table_name";
        public static List<KeyValuePair<string, object>> QueryParam(string table_schema, string table_name)
        {
            return new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>($"table_schema", table_schema),
                new KeyValuePair<string, object>($"table_name", table_name),
            };
        }
    }
}
