#if Common_Server
using Dash.Hive;
using Dash.Server.Dao;
using Dash.Server.Dao.Cache;
using Newtonsoft.Json.Converters;
using StackExchange.Redis.Extensions.Core.Configuration;
using System.Text.Json.Serialization;

namespace Dash.Server
{
    public class ServerConfig
    {
        public string TimeZoneName { get; set; } = "Asia/Seoul";
        public string ResourcePath { get; set; }
        public string LocalePath { get; set; }
    }
    public class RdsConfig
    {
        public DBConfiguration GameDB { get; set; }
        public DBConfiguration LogDB { get; set; }
    }

    public class RedisConfig
    {
        public RedisConfiguration Game { get; set; }
        public RedisConfiguration Monitor { get; set; }
        public RedisExConfig RedisExConfig { get; set; }
    }

    public class AWSConfig
    {
        public Config Default { get; set; }
        public class Config
        {
            public string AccessKey { get; set; }
            public string SecretKey { get; set; }
            public string Region { get; set; }
        }
    }

    public class ArenaRecordConfig
    {
        public string S3Bucket { get; set; }
        public string S3Path { get; set; }
        public string DDBTableSuffix { get; set; }
    }

    public class HiveConfig
    {
        public ZoneType Zone { get; set; } = ZoneType.SANDBOX;
        public ServerZoneType ServerZone { get; set; } = ServerZoneType.TEST;
        public class LoggerConfig
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum WriterType
            {
                Null,
                Hive,
                File,
            }            
            public WriterType Type { get; set; } = WriterType.File;
        }
        public LoggerConfig Logger { get; set; } = new LoggerConfig();
    }

    public class GameLogConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum WriterType
        {
            Null,
            File,
            MySql,
        }
        public WriterType Type { get; set; } = WriterType.MySql;
    }
}
#endif