#if Common_Server
using Dash.Server.Dao.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dash;
using Dash.Model.Rdb;

namespace Dash.Server.Dao.PreparedStatement
{
    public class QueryGenerator<T> where T : Common.Model.IModel
    {
        private static readonly Type _type = typeof(T);

        #region Pre generated queries
        public static readonly string SelectQuery;

        public static readonly string InsertQuery;
        
        public static readonly string UpdateQuery;

        public static readonly string DeleteQuery;

        public static readonly string InsertOrUpdateQuery;

        public static IReadOnlyDictionary<int, string> MultipleInsertQueries => _multipleInsertQueries;
        private static readonly Dictionary<int, string> _multipleInsertQueries = new Dictionary<int, string>();

        public static IReadOnlyDictionary<int, string> MultipleDeleteQueries => _multipleDeleteQueries;
        private static readonly Dictionary<int, string> _multipleDeleteQueries = new Dictionary<int, string>();

        public static readonly string TableName;

        public static IReadOnlyList<string> Columns => _columns;
        private static readonly List<string> _columns;
        public static IReadOnlyList<string> KeyColumns => _keyColumns;
        private static readonly List<string> _keyColumns;
        public static IReadOnlyList<string> SubKeyColumns => _subKeyColumns;
        private static readonly List<string> _subKeyColumns;
        public static IReadOnlyList<string> NonKeyColumns => _nonKeyColumns;
        private static readonly List<string> _nonKeyColumns;
        public static string MainKeyColumn;

        public static readonly List<string> SetNonKeyColumns;
        public static readonly string WhereMainKeyColumn;
        public static readonly string WhereKeyColumns;
        #endregion
        static QueryGenerator()
        {
            _columns = new List<string>();
            _keyColumns = new List<string>();
            _subKeyColumns = new List<string>();
            _nonKeyColumns = new List<string>();
            MainKeyColumn = string.Empty;
            InitColumns();

            TableName = SchemaInfoResolver.GetTableName(_type);
            SetNonKeyColumns = CreateSetNonKeyColumns();
            WhereMainKeyColumn = CreateWhereMainKeyColumn();
            WhereKeyColumns = CreateWhereKeyColumns();

            bool isSingleDbModel = DaoDefinition.Models[typeof(T)]?.IsMultipleDbModel ?? true;

            SelectQuery = CreateSelectQuery();
            InsertQuery = CreateInsertQuery();
            UpdateQuery = CreateUpdateQuery();
            DeleteQuery = CreateDeleteQuery();
            InsertOrUpdateQuery = CreateInsertOrUpdateQuery();

            if (isSingleDbModel == false)
            {
                // 10개 까지만 사전 생성한다.
                Enumerable.Range(1, 10).ToList().ForEach(i => _multipleInsertQueries.Add(i, CreateMultipleInsertQuery(i)));
                Enumerable.Range(1, 10).ToList().ForEach(i => _multipleDeleteQueries.Add(i, CreateMultipleDeleteQuery(i)));
            }
        }

        private static void InitColumns()
        {
            var columnProperties = SchemaInfoResolver.GetColumnProperties(_type);
            var columnNames = SchemaInfoResolver.GetColumnNames(_type);
            for (int i = 0; i < columnProperties.Count; ++i)
            {
                var property = columnProperties[i];
                var columnName = columnNames[i];
                var attribute = (KeyColumnAttribute)property.GetCustomAttributes(typeof(KeyColumnAttribute), false).SingleOrDefault();
                if (attribute != null)
                {
                    _keyColumns.Add(columnName);
                }
                else
                {
                    _nonKeyColumns.Add(columnName);
                }
                _columns.Add(columnName);
            }            
            for (int i = 0; i < _keyColumns.Count; ++i)
            {
                if (i == 0)
                    MainKeyColumn = _keyColumns[i];
                else
                    _subKeyColumns.Add(_keyColumns[i]);
            }
        }

        private static string CreateSelectQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");
            //sb.Append("* ");
            sb.Append(string.Join(",", WrapGrave(Columns)));
            sb.Append(" FROM ");
            sb.Append(WrapGrave(TableName));

            return sb.ToString();
        }

        private static string CreateInsertQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("INSERT INTO ");
            sb.Append(WrapGrave(TableName));
            sb.Append(" ");
            sb.Append(WrapInBracket(WrapGrave(Columns)));
            sb.Append(" VALUES ");
            sb.Append(WrapInBracket(GetColumns("@")));

            /*
            sb.Append(" ON DUPLICATE KEY UPDATE ");
            sb.Append(string.Join(", ", GetColumnsEqualWrappedValues()));
            */

            return sb.ToString();
        }

        private static string CreateUpdateQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("UPDATE ");
            sb.Append(WrapGrave(TableName));
            sb.Append(" SET ");
            sb.Append(string.Join(",", SetNonKeyColumns));

            return sb.ToString();
        }

        // DELETE FROM Account WHERE primaryKey1 = @primaryKey1 AND primaryKey2 = @primaryKey2 ...
        private static string CreateDeleteQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("DELETE FROM ");
            sb.Append(WrapGrave(TableName));

            return sb.ToString();
        }

        private static string CreateInsertOrUpdateQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("INSERT INTO ");
            sb.Append(WrapGrave(TableName));
            sb.Append(" ");
            sb.Append(WrapInBracket(WrapGrave(Columns)));
            sb.Append(" VALUES ");
            sb.Append(WrapInBracket(GetColumns("@")));
            sb.Append(" ON DUPLICATE KEY UPDATE ");            
            sb.Append(string.Join(",", SetNonKeyColumns));

            return sb.ToString();
        }

        /*
            INSERT INTO $Table
            ($Columns)
            VALUES
            ($Values), ($Values)            
         */
        public static string CreateMultipleInsertQuery(int count)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("INSERT INTO ");
            sb.Append(WrapGrave(TableName));
            sb.Append(WrapInBracket(WrapGrave(Columns)));
            sb.Append(" VALUES ");

            List<string> valuesQuery = new List<string>();
            for (int i = 0; i < count; ++i)
            {
                var paramColumns = GetColumns("@", i.ToString());
                valuesQuery.Add(WrapInBracket(paramColumns));
            }
            sb.Append(string.Join(",", valuesQuery));

            /*
            sb.Append(" ON DUPLICATE KEY UPDATE ");
            sb.Append(string.Join(", ", GetColumnsEqualWrappedValues()));
            */

            return sb.ToString();
        }

        /*
            DELETE FROM $Table
            WHERE $MainKey = $MainKey
            AND $SubKeys IN (($SubKey1,$SubKey2),($SubKey1,$SubKey2),...)
        */
        public static string CreateMultipleDeleteQuery(int count)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("DELETE FROM ");
            sb.Append(WrapGrave(TableName));
            sb.Append(WhereMainKeyColumn);
            sb.Append(" AND ");
            sb.Append(WrapInBracket(WrapGrave(SubKeyColumns)));
            sb.Append(" IN ");

            List<string> inQuery = new List<string>();
            for (int i = 0; i < count; ++i)
            {
                var paramColumns = GetSubKeyColumns("@", i.ToString());
                inQuery.Add(WrapInBracket(paramColumns));
            }
            sb.Append(WrapInBracket(inQuery));

            return sb.ToString();
        }

        // @oidAccount = VALUES(oidAccount), @strVirtualId = VALUES(strVirtualId)
        private static List<string> CreateSetNonKeyColumns()
        {
            return NonKeyColumns.Select(v => $"{WrapGrave(v)}=@{v}").ToList();
        }

        private static string WrapGrave(string str)
        {
            return $"`{str}`";
        }
        private static List<string> WrapGrave(IReadOnlyList<string> strs)
        {
            return strs.Select(x => $"`{x}`").ToList();            
        }
        // ["1", "2", "3"] -> string( "(1,2,3)" )
        private static string WrapInBracket(IReadOnlyList<string> strs)
        {
            if (strs.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            sb.Append(string.Join(",", strs));
            sb.Append(")");
            return sb.ToString();
        }

        public static List<string> GetColumns(string prefix = "", string postfix = "")
        {
            return Columns.Select(v => prefix + v + postfix).ToList();
        }

        public static List<string> GetSubKeyColumns(string prefix = "", string postfix = "")
        {
            return SubKeyColumns.Select(v => prefix + v + postfix).ToList();
        }

        public static List<string> GetKeyColumns(string prefix = "", string postfix = "")
        {
            return KeyColumns.Select(v => prefix + v + postfix).ToList();
        }
        public static List<string> GetNonKeyColumns(string prefix = "", string postfix = "")
        {
            return Columns.Except(KeyColumns).Select(v => prefix + v + postfix).ToList();
        }

        private static string CreateWhereMainKeyColumn(string postfix = "")
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" WHERE ");
            sb.Append($"{WrapGrave(MainKeyColumn)} = @{MainKeyColumn}{postfix}");

            return sb.ToString();
        }

        private static string CreateWhereKeyColumns(string postfix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" WHERE ");

            var columns = KeyColumns.Select(v => $"{WrapGrave(v)} = @{v}{postfix}").ToList();
            sb.Append(string.Join(" AND ", columns));

            return sb.ToString();
        }

        public static string CreateTruncateQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("TRUNCATE TABLE ");
            sb.Append(WrapGrave(TableName));

            return sb.ToString();
        }

        public static string CreateWhereColumn(string column)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" WHERE ");
            sb.Append($"{WrapGrave(column)} = @{column}");

            return sb.ToString();
        }
    }
}
#endif