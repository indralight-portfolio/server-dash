﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class CouponUse : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>($"@{nameof(CouponId)}", keys[0]) };
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { CouponId.ToString() };
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public CouponUse() { }
        public CouponUse(CouponUse other)
        {
            OidAccount = other.OidAccount;
            CouponId = other.CouponId;
            UsedTime = other.UsedTime;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(1)]
        [Column]
        [KeyColumn]
        public uint CouponId { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public DateTime? UsedTime { get; set; }
    }
}
