﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class AccountExtra : Common.Model.IModel
    {
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
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public AccountExtra() { }
        public AccountExtra(AccountExtra other)
        {
            OidAccount = other.OidAccount;
            AppVersion = other.AppVersion;
            ClientIp = other.ClientIp;
            Device = other.Device;
            DeviceId = other.DeviceId;
            Language = other.Language;
            Market = other.Market;
            OsVersion = other.OsVersion;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(6)]
        [Column]
        public string Market { get; set; }
        [MessagePack.Key(3)]
        [Column]
        public string Device { get; set; }
        [MessagePack.Key(7)]
        [Column]
        public string OsVersion { get; set; }
        [MessagePack.Key(1)]
        [Column]
        public string AppVersion { get; set; }
        [MessagePack.Key(4)]
        [Column]
        public string DeviceId { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public string ClientIp { get; set; }
        [MessagePack.Key(5)]
        [Column]
        public string Language { get; set; }
    }
}
