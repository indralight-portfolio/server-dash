﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class EpisodeEntryCount : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>($"@{nameof(Id)}", keys[0]) };
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { Id.ToString() };
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public EpisodeEntryCount() { }
        public EpisodeEntryCount(EpisodeEntryCount other)
        {
            OidAccount = other.OidAccount;
            Id = other.Id;
            Count = other.Count;
            UpdateTime = other.UpdateTime;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(1)]
        [Column]
        [KeyColumn]
        public int Id { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public uint Count { get; set; }
        [MessagePack.Key(3)]
        [Column]
        public DateTime UpdateTime { get; set; }
    }
}