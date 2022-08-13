#if Common_Server
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;

namespace Dash.Server.Dao
{
    public abstract class Connector
    {
        public enum DbSchema
        {
            GameDB,
            LogDB,
        }

        public abstract MySqlConnection GetConnection();
        public abstract DbContext GetDbContext();
    }
    public class Connector<TContext> : Connector where TContext : DbContext
    {
        private readonly string _connectionString;
        private DbContextOptionsBuilder<TContext> _dbContextOptionsBuilder;

        public Connector(string connectionString)
        {
            _connectionString = connectionString;
            var serverVersion = new MySqlServerVersion(new Version(5, 7, 34));
            _dbContextOptionsBuilder = new DbContextOptionsBuilder<TContext>();
            _dbContextOptionsBuilder.UseMySql(_connectionString, serverVersion);
        }

        public override MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public override DbContext GetDbContext()
        {
            if (typeof(TContext) == typeof(GameDBContext))
                return new GameDBContext(_dbContextOptionsBuilder.Options);
            else if (typeof(TContext) == typeof(LogDBContext))
                return new LogDBContext(_dbContextOptionsBuilder.Options);
            else
                return null;
        }
    }
}
#endif