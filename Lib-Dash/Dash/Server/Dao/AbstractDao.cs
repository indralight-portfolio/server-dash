#if Common_Server
using Dash.Server.Dao.PreparedStatement;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dash.Server.Dao
{
    public class DBConfiguration
    {
        [JsonIgnore]
        public string ConnectionString
        {
            get
            {
                return $"Server={Server};Port={Port};Username={Username};Password={Password};Database={Database};maximumpoolsize=30;CharSet=utf8;UseAffectedRows=True;GuidFormat=LittleEndianBinary16;ConvertZeroDateTime=true";
            }
        }
        public string Server { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
    }
    public abstract class AbstractDao
    {
        public abstract Connector Connector { get; }
        public abstract string Database { get; }

        public async Task<List<T>> FromSql<T>(string query, List<KeyValuePair<string, object>> param) where T : class
        {
            return await new FromSqlRawStmt(Connector, query, param).Call<T>();
        }

        public async Task<int> ExecuteSql(string query, List<KeyValuePair<string, object>> param)
        {
            return await new ExecuteSqlRawStmt(Connector, query, param).Call();
        }

        public async Task<int> ExecuteCount(string query, List<KeyValuePair<string, object>> param)
        {
            return await new ExecuteCountStmt(Connector, query, param).Call();
        }

        public async Task<bool> ExecuteSqlTrans(List<string> queries, List<List<KeyValuePair<string, object>>> param)
        {
            return await new ExecuteSqlRawTransStmt(Connector, queries, param).Call();
        }
    }
}
#endif