﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class Equipment : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>($"@{nameof(Serial)}", keys[0]) };
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { Serial.ToString() };
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public Equipment() { }
        public Equipment(Equipment other)
        {
            OidAccount = other.OidAccount;
            Serial = other.Serial;
            CharacterId = other.CharacterId;
            Exp = other.Exp;
            Id = other.Id;
            Level = other.Level;
            Locked = other.Locked;
            MainStatIndex = other.MainStatIndex;
            Overcome = other.Overcome;
            Reforge = other.Reforge;
            SlotIndex = other.SlotIndex;
            SubStat1Index = other.SubStat1Index;
            SubStat2Index = other.SubStat2Index;
            SubStat3Index = other.SubStat3Index;
            SubStat4Index = other.SubStat4Index;
            SubStat5Index = other.SubStat5Index;
            SubStat6Index = other.SubStat6Index;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(1)]
        [Column]
        [KeyColumn]
        public uint Serial { get; set; }
        [MessagePack.Key(4)]
        [Column]
        public int Id { get; set; }
        [MessagePack.Key(5)]
        [Column]
        public byte Level { get; set; }
        [MessagePack.Key(3)]
        [Column]
        public uint Exp { get; set; }
        [MessagePack.Key(8)]
        [Column]
        public byte Overcome { get; set; }
        [MessagePack.Key(9)]
        [Column]
        public byte Reforge { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public int? CharacterId { get; set; }
        [MessagePack.Key(10)]
        [Column]
        public byte? SlotIndex { get; set; }
        [MessagePack.Key(7)]
        [Column]
        public int? MainStatIndex { get; set; }
        [MessagePack.Key(11)]
        [Column]
        public int? SubStat1Index { get; set; }
        [MessagePack.Key(12)]
        [Column]
        public int? SubStat2Index { get; set; }
        [MessagePack.Key(13)]
        [Column]
        public int? SubStat3Index { get; set; }
        [MessagePack.Key(14)]
        [Column]
        public int? SubStat4Index { get; set; }
        [MessagePack.Key(15)]
        [Column]
        public int? SubStat5Index { get; set; }
        [MessagePack.Key(16)]
        [Column]
        public int? SubStat6Index { get; set; }
        [MessagePack.Key(6)]
        [Column]
        public bool Locked { get; set; }
    }
}