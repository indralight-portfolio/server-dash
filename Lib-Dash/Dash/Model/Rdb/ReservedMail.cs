﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class ReservedMail : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return nameof(Id); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return Id.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid()
        {
            return Id > 0;
        }

        public ReservedMail() { }
        public ReservedMail(ReservedMail other)
        {
            Id = other.Id;
            AdminId = other.AdminId;
            BodyType = other.BodyType;
            Comment = other.Comment;
            Data = other.Data;
            DataCondition = other.DataCondition;
            DataRepeat = other.DataRepeat;
            LocalEnd = other.LocalEnd;
            LocalStart = other.LocalStart;
            Updated = other.Updated;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public uint Id { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public string BodyType { get; set; }
        [MessagePack.Key(4)]
        [Column]
        public byte[] Data { get; set; }
        [MessagePack.Key(5)]
        [Column]
        public byte[] DataCondition { get; set; }
        [MessagePack.Key(8)]
        [Column]
        public DateTime LocalStart { get; set; }
        [MessagePack.Key(7)]
        [Column]
        public DateTime LocalEnd { get; set; }
        [MessagePack.Key(6)]
        [Column]
        public byte[] DataRepeat { get; set; }
        [MessagePack.Key(9)]
        [Column]
        public DateTime Updated { get; set; }
        [MessagePack.Key(3)]
        [Column]
        public string Comment { get; set; }
        [MessagePack.Key(1)]
        [Column]
        public string AdminId { get; set; }
    }
}
