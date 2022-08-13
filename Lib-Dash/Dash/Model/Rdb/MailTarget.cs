﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class MailTarget : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => true;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>($"@{nameof(TemplateId)}", keys[0]) };
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return new List<string>() { TemplateId.ToString() };
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public MailTarget() { }
        public MailTarget(MailTarget other)
        {
            OidAccount = other.OidAccount;
            TemplateId = other.TemplateId;
            Status = other.Status;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(1)]
        [Column]
        [KeyColumn]
        public uint TemplateId { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public sbyte Status { get; set; }
    }
}