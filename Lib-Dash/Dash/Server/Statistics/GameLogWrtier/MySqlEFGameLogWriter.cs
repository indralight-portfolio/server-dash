#if Common_Server && Admin_Server
using Dash.Model.Rdb;
using Dash.Server.Dao;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Dash.Server.Statistics.GameLogWriter
{
    public class MySqlGameLogWriter : IGameLogWriter, IHasDBContext
    {
        private LogDBContext _logDBContext;

        public void Init(LogDBContext logDBContext) => _logDBContext = logDBContext;

        public void Write(GameLogContext log)
        {
            _logDBContext.Add(log.ToRdbModel());
            _logDBContext.SaveChanges();
        }

        public void Write(List<GameLogContext> logs)
        {
            _logDBContext.AddRange(logs.ToRdbModel());
            _logDBContext.SaveChanges();
        }
    }

    static partial class GameLogContextHelper
    {
        public static GameLog ToRdbModel(this GameLogContext log)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            IsoDateTimeConverter dateConverter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss"
            };
            settings.Converters.Add(dateConverter);

            return new GameLog
            {
                Time = log.Time,
                OidAccount = log.OidAccount,
                Level = log.AccountLevel,
                Command = log.Command,
                Payload = JsonConvert.SerializeObject(log.PayLoad, settings),
            };
        }

        public static List<GameLog> ToRdbModel(this List<GameLogContext> logs)
        {
            return logs.Select(e => e.ToRdbModel()).ToList();
        }
    }
}
#endif