#if Common_Server
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Dash.Server.Statistics.GameLogWriter
{
    public class FileGameLogWriter : IGameLogWriter
    {
        private static NLog.Logger _logger = NLog.LogManager.GetLogger("GameLogger");
        public static NLog.Logger Logger => _logger;

        public void Write(GameLogContext log)
        {
            _logger.Info(log.ToJson());
        }

        public void Write(List<GameLogContext> logs)
        {
            foreach(var log in logs)
            {
                _logger.Info(log.ToJson());
            }
        }
    }

    static partial class GameLogContextHelper
    {
        public static string ToJson(this GameLogContext log)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            IsoDateTimeConverter dateConverter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss"
            };
            settings.Converters.Add(dateConverter);
            settings.ContractResolver = new LowercaseContractResolver();

            return JsonConvert.SerializeObject(log, settings);
        }
    }
}
namespace Newtonsoft.Json.Serialization
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }

}
#endif