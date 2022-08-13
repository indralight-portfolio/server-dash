#if Common_Server
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dash.Server.Dao.PreparedStatement
{
    public class FromSqlRawStmt : AbstractPreparedStatement
    {
        public FromSqlRawStmt(Connector connector, string query, List<KeyValuePair<string, object>> param) :
            base(connector, query, param)
        {
        }

        public async Task<List<T>> Call<T>() where T : class
        {
            return await FromSqlRaw<T>();
        }
    }

    public class ExecuteSqlRawStmt : AbstractPreparedStatement
    {
        public ExecuteSqlRawStmt(Connector connector, string query, List<KeyValuePair<string, object>> param) :
            base(connector, query, param)
        {
        }

        public async Task<int> Call()
        {
            return await ExecuteSqlRaw();
        }
    }

    public class ExecuteCountStmt : AbstractPreparedStatement
    {
        public ExecuteCountStmt(Connector connector, string query, List<KeyValuePair<string, object>> param) :
           base(connector, query, param)
        {
        }

        public async Task<int> Call()
        {
            return Convert.ToInt32(await ExecuteScalar() ?? 0);
        }
    }

    public class ExecuteSqlRawTransStmt : AbstractPreparedStatement
    {
        public ExecuteSqlRawTransStmt(Connector connector, List<string> queries, List<List<KeyValuePair<string, object>>> paramList) :
           base(connector, queries, paramList)
        {
        }

        public async Task<bool> Call()
        {
            return await ExecuteSqlRawTrans();
        }
    }
}
#endif