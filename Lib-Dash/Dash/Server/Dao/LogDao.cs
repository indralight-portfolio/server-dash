#if Common_Server
using NLog;
using System;

namespace Dash.Server.Dao
{
    using Microsoft.EntityFrameworkCore;

    public class LogDao : AbstractDao
    {
        private readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private readonly Connector _connector;
        private readonly string _database;

        public override Connector Connector => _connector;
        public override string Database => _database;

        public LogDao(DBConfiguration configuration)
        {
            // TODO: from connection pool
            _connector = new Connector<LogDBContext>(configuration.ConnectionString);
            _database = configuration.Database;
            //_logger.Info($"[GameDao]{configuration.ConnectionString}");
        }
    }
}
#endif