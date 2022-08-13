#if Common_Server
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Utility;
using Dash.Model.Rdb;

namespace Dash.Server.Dao
{
    public static class SchemaInfoResolver
    {
        public class SchemaInfoContext
        {
            public string TableName;
            public PropertyInfo[] ColumnProperties;
            public string[] ColumnNames;
        }

        private static Dictionary<Type, SchemaInfoContext> _schemaNameContexts = new Dictionary<Type, SchemaInfoContext>();
        static SchemaInfoResolver()
        {
            foreach (Type derivedType in DerivedTypeCache.GetDerivedTypes(typeof(Common.Model.IModel), Assembly.GetAssembly(typeof(SchemaInfoResolver))))
            {
                SchemaInfoContext context = new SchemaInfoContext();
                context.TableName = ResolveName(derivedType.Name);
                context.ColumnProperties = derivedType.GetProperties().Where(p => p.GetCustomAttribute(typeof(ColumnAttribute), false) != null).ToArray();
                context.ColumnNames = context.ColumnProperties.Select(p => ResolveName(p.Name)).ToArray();

                _schemaNameContexts.Add(derivedType, context);
            }
        }

        // as lowercase
        // 실제로 훨씬 많으나 게임에서 사용할법한 것만 등록해둠.
        // https://mariadb.com/kb/en/library/reserved-words/
        private static HashSet<string> _reservedWords = new HashSet<string>()
        {
            "character",
            "key",
            "create",
            "check",
            "change",
            "index",
        };

        public static IReadOnlyList<PropertyInfo> GetColumnProperties(Type type)
        {
            return _schemaNameContexts[type].ColumnProperties;
        }

        public static IReadOnlyList<string> GetColumnNames(Type type)
        {
            return _schemaNameContexts[type].ColumnNames;
        }

        public static string GetTableName(Type type)
        {
            return _schemaNameContexts[type].TableName;
        }

        public static string ResolveName(string raw)
        {
            string lower = raw.ToLower();
            //if (_reservedWords.Contains(lower) == true)
            //{
            //    return $"`{raw}`";
            //}

            return raw;
        }
    }
}
#endif