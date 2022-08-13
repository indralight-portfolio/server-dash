﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class EquipRune : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>($"@{nameof(CharacterId)}", keys[0]) };
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { CharacterId.ToString() };
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public EquipRune() { }
        public EquipRune(EquipRune other)
        {
            OidAccount = other.OidAccount;
            CharacterId = other.CharacterId;
            Data = other.Data;
            OpenTier = other.OpenTier;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(1)]
        [Column]
        [KeyColumn]
        public int CharacterId { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public byte[] Data { get; set; }
        [MessagePack.Key(3)]
        [Column]
        public byte OpenTier { get; set; }
    }
}
