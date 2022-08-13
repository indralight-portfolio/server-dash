#if Common_Server
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dash.Server.Dao.PreparedStatement
{
    public abstract class AbstractPreparedStatement
    {
        protected static readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private readonly Connector _connector;
        private string _query;
        private List<KeyValuePair<string, object>> _param;

        private List<string> _queries;
        private List<List<KeyValuePair<string, object>>> _paramList;

        protected AbstractPreparedStatement(Connector connector, string query, List<KeyValuePair<string, object>> param)
        {
            _connector = connector;
            _query = query;
            _param = param;
        }
        protected AbstractPreparedStatement(Connector connector, List<string> queries, List<List<KeyValuePair<string, object>>> paramList)
        {
            _connector = connector;
            _queries = queries;
            _paramList = paramList;
        }

        public async Task<int> ExecuteSqlRaw()
        {
            var sqlParam = _param.Select(e => new MySqlParameter(e.Key, e.Value)).ToArray();
            using (var dbContext = _connector.GetDbContext())
            {
                try
                {
                    int rowAffected = await dbContext.Database.ExecuteSqlRawAsync(_query, sqlParam);
                    return rowAffected;
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex);
                    _logger.Fatal(PrintCurrentStatement());
                    throw ex;
                }
            }
        }

        public async Task<List<T>> FromSqlRaw<T>() where T : class
        {
            var sqlParam = _param.Select(e => new MySqlParameter(e.Key, e.Value)).ToArray();
            using (var dbContext = _connector.GetDbContext())
            {
                var dbSet = dbContext.Set<T>();
                try
                {
                    List<T> result;
                    if (sqlParam.Length > 0)
                        result = await dbSet.FromSqlRaw(_query, sqlParam).ToListAsync();
                    else
                        result = await dbSet.FromSqlRaw(_query).ToListAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex);
                    _logger.Fatal(PrintCurrentStatement());
                    throw ex;
                }
            }
        }

        public async Task<object> ExecuteScalar()
        {
            using (var dbContext = _connector.GetDbContext())
            using (var connection = dbContext.Database.GetDbConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    using var command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = _query;
                    command.SetParam(_param);
                    await command.PrepareAsync();
                    var spResult = await command.ExecuteScalarAsync();
                    return spResult;
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex);
                    _logger.Fatal(PrintCurrentStatement());
                    throw ex;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        protected async Task<bool> ExecuteSqlRawTrans()
        {
            if (_queries.Count != _paramList.Count)
            {
                throw new ArgumentException($"Unmatch params, queries count. {_queries.Count} {_paramList.Count}");
            }

            using (var dbContext = _connector.GetDbContext())
            using (var trans = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    for (int i = 0; i < _queries.Count; ++i)
                    {
                        _query = _queries[i];
                        _param = _paramList[i];

                        var sqlParam = _param.Select(e => new MySqlParameter(e.Key, e.Value)).ToArray();
                        int rowAffected;
                        if (sqlParam.Length > 0)
                            rowAffected = await dbContext.Database.ExecuteSqlRawAsync(_query, sqlParam);
                        else
                            rowAffected = await dbContext.Database.ExecuteSqlRawAsync(_query);
                        if (rowAffected <= 0)
                        {
                            _logger.Fatal(PrintCurrentStatement());
                            _logger.Fatal($"rowAffected {rowAffected}");
                            await trans.RollbackAsync();
                            return false;
                        }
                    }
                    await trans.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex);
                    _logger.Fatal(PrintCurrentStatement());
                    try
                    {
                        await trans.RollbackAsync();
                    }
                    catch (MySqlException ex1)
                    {
                        _logger.Fatal(ex1);
                    }
                }
            }
            return false;
        }

        private string PrintCurrentStatement()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"query: {_query} | params: {String.Join(",", _param.Select(kv => kv.Key + "=" + kv.Value).ToArray())}");
            return sb.ToString();
        }
    }

    public static class DbCommand_Extend
    {
        public static void SetParam(this DbCommand command, List<KeyValuePair<string, object>> param)
        {
            foreach (var kv in param)
            {
                var dbParam = command.CreateParameter();
                dbParam.ParameterName = kv.Key;
                dbParam.Value = kv.Value;

                command.Parameters.Add(dbParam);
            }
        }
    }
}
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
#endif