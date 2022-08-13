﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class MultipleDeck : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>($"@{nameof(DeckId)}", keys[0]) };
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { DeckId.ToString() };
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public MultipleDeck() { }
        public MultipleDeck(MultipleDeck other)
        {
            OidAccount = other.OidAccount;
            DeckId = other.DeckId;
            DeckData = other.DeckData;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(1)]
        [Column]
        [KeyColumn]
        public byte DeckId { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public byte[] DeckData { get; set; }
    }
}
