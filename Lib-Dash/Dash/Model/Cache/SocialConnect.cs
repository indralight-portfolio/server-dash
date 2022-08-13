using Dash.Model.Rdb;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
#if Common_Server
using Dash.Server.Dao.Model;
using StackExchange.Redis;
#endif

namespace Dash.Model.Cache
{
    [NotTableMapped]
    [MessagePackObject()]
    public class SocialConnectInfo : Common.Model.IModel
    {
        public static readonly int ExpireSeconds = 60;
        public static readonly int UpdateSeconds = 30;

        public enum PlayType
        {
            Lobby,
            Party,
            Single,
            Multi,
        }

        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid() => true;

        public SocialConnectInfo() { }
        public SocialConnectInfo(ulong oidAccount, int partySerial, string partyCode)
        {
            OidAccount = oidAccount;
            PartySerial = partySerial;
            PartyCode = partyCode;
            SetUpdatedTime(DateTime.UtcNow);
            State = (int)PlayType.Lobby;
        }
        [Column]
        [KeyColumn]
        [Key(0)]
        public ulong OidAccount { get; set; }
        [Column]
        [Key(1)]
        public int PartySerial { get; set; }
        [Column]
        [Key(2)]
        public string PartyCode { get; set; }
        [Column]
        [Key(3)]
        public DateTime UpdatedTime { get; set; }
        [Column]
        [Key(4)]
        public int State { get; set; }
        [Column]
        [Key(5)]
        public bool IsOnline { get; set; }
        [Column]
        [Key(6)]
        public string EndPoint { get; set; }
        [Column]
        [Key(7)]
        public string ServerUUID { get; set; }

        public void SetUpdatedTime(DateTime time) { UpdatedTime = new SqlDateTime(time).Value; } // must use Utc
    }
    public class SocialConnectOnlineUpdater : Common.Model.IModel
    {
        public SocialConnectOnlineUpdater()
        {
            SetUpdatedTime(DateTime.UtcNow);
        }
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [Column]
        public bool IsOnline { get; set; }
        [Column]
        public string EndPoint { get; set; }
        [Column]
        public string ServerUUID { get; set; }
        [Column]
        public DateTime UpdatedTime { get; set; }

        public string GetMainKey() => OidAccount.ToString();

        public List<string> GetSubKeys() => null;

        public bool IsAutoIncKeysValid() => true;


        public void SetUpdatedTime(DateTime time) { UpdatedTime = new SqlDateTime(time).Value; } // must use Utc        

#if Common_Server
        public HashEntry[] ToHashEntries()
        {
            return ModelConverter<SocialConnectOnlineUpdater>.ToHashEntries(this);
        }
#endif
    }
    public class SocialConnectPartyUpdater : Common.Model.IModel
    {
        public SocialConnectPartyUpdater() { SetUpdatedTime(DateTime.UtcNow); }
        public SocialConnectPartyUpdater(ulong oidAccount)
        {
            OidAccount = oidAccount;
            PartySerial = 0;
            PartyCode = "";
            State = (int)SocialConnectInfo.PlayType.Lobby;
            SetUpdatedTime(DateTime.UtcNow);
        }
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [Column]
        public int PartySerial { get; set; }
        [Column]
        public string PartyCode { get; set; }
        [Column]
        public int State { get; set; }
        [Column]
        public DateTime UpdatedTime { get; set; }

        public string GetMainKey() => OidAccount.ToString();

        public List<string> GetSubKeys() => null;

        public bool IsAutoIncKeysValid() => true;

        public void SetUpdatedTime(DateTime time) { UpdatedTime = new SqlDateTime(time).Value; } // must use Utc

#if Common_Server
        public HashEntry[] ToHashEntries()
        {
            return ModelConverter<SocialConnectPartyUpdater>.ToHashEntries(this);
        }
#endif
    }
}
