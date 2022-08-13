using Dash.Server.Dao.Cache;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;

namespace server_dash
{
    public class Config
    {
        public static readonly string Server = nameof(Server);
        public static readonly string Rds = nameof(Rds);
        public static readonly string Redis = nameof(Redis);
        public static readonly string BattleServer = nameof(BattleServer);
        public static readonly string LobbyServer = nameof(LobbyServer);
        public static readonly string MatchServer = nameof(MatchServer);
        public static readonly string SocialServer = nameof(SocialServer);
        public static readonly string Check = nameof(Check);
        public static readonly string AWS = nameof(AWS);
        public static readonly string Cheat = nameof(Cheat);
    }

    public class ServerConfig : Dash.Server.ServerConfig
    {
        public bool IsPrivate { get; set; } = false;
        public string MatchServerEndpoint { get; set; }
    }

    public class RdsConfig : Dash.Server.RdsConfig
    {
    }

    public class RedisConfig : Dash.Server.RedisConfig
    {
    }

    public class BattleServerConfig
    {
        public class ArenaRecordConfig : Dash.Server.ArenaRecordConfig
        {
            public enum RecorderType
            {
                Null,
                Log,
                File,
                S3
            }
            [JsonConverter(typeof(StringEnumConverter))]
            public RecorderType Recorder { get; set; }
            public bool LoggingStatistics { get; set; }
            public bool RecordBytes { get; set; }            
        }

        public class AntiHackConfig
        {
            // 해킹 로그만 남기고 싶을 때. 서비스 처음 나갈 땐 true로 나가서 동향 파악해봐야 할 듯
            public bool AllowHack;
        }

        public bool Active { get; set; }
        public int Port { get; set; }        
        public ArenaRecordConfig ArenaRecord { get; set; }
        public AntiHackConfig AntiHack { get; set; }
        public bool CheatEnable { get; set; }
        public int CoreCount { get; set; }
    }

    public class LobbyServerConfig
    {
        public class BillingConfig
        {
            public string AppStoreVerifyReceiptUrl { get; set; }
            public string DDBTableSuffix{ get; set; }
        }
        public bool Active { get; set; }
        public int Port { get; set; }
        public bool AutoAccountCreate { get; set; } = false;
        public bool GiveAllHeroWhenCreate { get; set; } = false;
        public bool GiveTestItemWhenCreate { get; set; } = false;
        public BillingConfig Billing { get; set; }
    }

    public class MatchServerConfig
    {
        public bool Active { get; set; }
        public int WebHostPort { get; set; }
        public int Port { get; set; }
        public TimeSpan KeepAliveTimeout { get; set; } = TimeSpan.FromSeconds(1000000);
        public int CoreCount { get; set; }
    }

    public class CheckConfig
    {
        public string ServiceStateUrl { get; set; }
        public bool IgnoreVersion { get; set; } = false;
        public bool IgnoreSession { get; set; } = false;
        public List<string> WhiteClientList { get; set; } = new List<string>();
        public List<string> AdminServerList { get; set; } = new List<string>();

    }

    public class AWSConfig : Dash.Server.AWSConfig
    {
    }
    public class SocialServerConfig
    {
        public bool Active { get; set; }
        public int Port { get; set; }
        public int CoreCount { get; set; }
    }

    public class CheatConfig
    {
        public bool AutoAccountCreate { get; set; } = false;
        public bool GiveAllHeroWhenCreate { get; set; } = false;
        public bool GiveTestJewelWhenCreate { get; set; } = false;
        public bool GiveTestItemWhenCreate { get; set; } = false;
        public bool AutoOpenChapter { get; set; } = false;
    }
}