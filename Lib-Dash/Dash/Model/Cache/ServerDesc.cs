using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Dash.Model.Rdb;
using Dash.Types;

namespace Dash.Model.Cache
{
    [NotTableMapped]
    public abstract class ServerDesc : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return nameof(UUID); }
        public string GetRedisKey() { return GetType().Name + ":" + GetMainKey(); }

        public string GetMainKey() { return UUID.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid() => true;

        public abstract ServiceAreaType GetServiceAreaType();

        public ServerDesc()
        {
            SetStarted(DateTime.UtcNow);
        }

        [Column]
        [KeyColumn]
        public string UUID { get; set; }
        [Column]
        public string HostName { get; set; }
        [Column]
        public string IP { get; set; }
        [Column]
        public string Endpoint { get; set; }
        [Column]
        public DateTime Started { get; set; }
        [Column]
        public DateTime Updated { get; set; }
        [Column]
        public string Version { get; set; }
        [Column]
        public string RevServer { get; set; }
        [Column]
        public string RevDash { get; set; }
        [Column]
        public string RevData { get; set; }
        [Column]
        public byte[] ProcessSnapshotsRaw { get; set; }
        [Column]
        public float Density { get; set; }

        public void SetStarted(DateTime time) { Started = new SqlDateTime(time).Value; } // must use Utc
        public void SetUpdated(DateTime time) { Updated = new SqlDateTime(time).Value; } // must use Utc

        public ServerDesc(ServerDesc other)
        {
            UUID = other.UUID;
            HostName = other.HostName;
            IP = other.IP;
            Endpoint = other.Endpoint;
            Started = other.Started;
            Updated = other.Updated;
            Version = other.Version;
            RevServer = other.RevServer;
            RevDash = other.RevDash;
            RevData = other.RevData;
            ProcessSnapshotsRaw = other.ProcessSnapshotsRaw;
        }
    }

    public class MatchDesc : ServerDesc
    {
        public static bool IsMultipleDbModel => false;

        public override ServiceAreaType GetServiceAreaType() => ServiceAreaType.Match;
        public static void MakeSubKeysWithName(params object[] keys) { }
    }
    public class RelayDesc : ServerDesc
    {
        public static bool IsMultipleDbModel => false;

        public override ServiceAreaType GetServiceAreaType() => ServiceAreaType.Relay;
        public static void MakeSubKeysWithName(params object[] keys) { }
    }
    public class BattleDesc : ServerDesc
    {
        public static bool IsMultipleDbModel => false;

        public override ServiceAreaType GetServiceAreaType() => ServiceAreaType.Battle;
        public static void MakeSubKeysWithName(params object[] keys) { }
        [Column]
        public int CCU { get; set; } = 0;
    }
    public class LobbyDesc : ServerDesc
    {
        public static bool IsMultipleDbModel => false;

        public override ServiceAreaType GetServiceAreaType() => ServiceAreaType.Lobby;
        public static void MakeSubKeysWithName(params object[] keys) { }
    }
    public class SocialDesc : ServerDesc
    {
        public static bool IsMultipleDbModel => false;

        public override ServiceAreaType GetServiceAreaType() => ServiceAreaType.Social;
        public static void MakeSubKeysWithName(params object[] keys) { }
        [Column]
        public int CCU { get; set; } = 0;
    }
}
