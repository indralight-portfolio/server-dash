﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class PeriodOverride : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(Type); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>($"@{nameof(Id)}", keys[0]) };
        }

        public string GetMainKey() { return Type.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { Id.ToString() };
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public PeriodOverride() { }
        public PeriodOverride(PeriodOverride other)
        {
            Type = other.Type;
            Id = other.Id;
            End = other.End;
            Start = other.Start;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public string Type { get; set; }
        [MessagePack.Key(1)]
        [Column]
        [KeyColumn]
        public int Id { get; set; }
        [MessagePack.Key(3)]
        [Column]
        public DateTime Start { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public DateTime End { get; set; }
    }
}