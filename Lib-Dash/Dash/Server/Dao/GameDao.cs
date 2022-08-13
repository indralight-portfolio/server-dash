#if Common_Server
using NLog;
using System;

namespace Dash.Server.Dao
{
    public class GameDao : AbstractDao
    {
        private readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private readonly Connector _connector;
        private readonly string _database;

        public override Connector Connector => _connector;
        public override string Database => _database;

        public GameDao(DBConfiguration configuration)
        {
            // TODO: from connection pool
            _connector = new Connector<GameDBContext>(configuration.ConnectionString);
            _database = configuration.Database;
            //_logger.Info($"[GameDao]{configuration.ConnectionString}");
        }

        public static void ValidateSchema<T>(AbstractDao db, NLog.Logger logger)
        {
            try
            {
                if (db == null)
                {
                    return;
                }

                //var attributes = Attribute.GetCustomAttributes(typeof(T));
                //if (attributes.Any(a => a is NotMappedTableAttribute) == true) { return; }

                var properties = SchemaInfoResolver.GetColumnProperties(typeof(T));
                int fieldCount = properties.Count;

                var query = Dash.Model.Cache.TableSchema.Query;
                var param = Dash.Model.Cache.TableSchema.QueryParam(db.Database, typeof(T).Name);
                var task = db.FromSql<Dash.Model.Cache.TableSchema>(query, param);
                task.Wait();
                var list = task.Result;
                var rowCount = list.Count;
                if (rowCount != fieldCount)
                {
                    logger.Fatal($"unmatch columns count. {typeof(T).Name}, db: {rowCount}, code:{fieldCount}");
                    throw new Exception($"unmatch columns count. {typeof(T).Name}, db: {rowCount}, code:{fieldCount}");
                }

                for (int i = 0; i < rowCount; ++i)
                {
                    string fromCode = properties[i].Name;
                    string fromDb = list[i].COLUMN_NAME;
                    if (fromCode.Equals(fromDb) == false)
                    {
                        logger.Fatal($"unmatch column name: {typeof(T).Name}, {fromCode}, {fromDb}");
                        throw new Exception($"unmatch column name: {typeof(T).Name}, {fromCode}, {fromDb}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal($"exception ocurred in {typeof(T).Name} {ex.Message} {ex.StackTrace}");
                throw ex;
            }
        }
    }
}
#endif