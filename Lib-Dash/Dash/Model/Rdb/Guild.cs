﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class Guild : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return nameof(OidGuild); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return OidGuild.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid()
        {
            return OidGuild > 0;
        }

        public Guild() { }
        public Guild(Guild other)
        {
            OidGuild = other.OidGuild;
            Descr = other.Descr;
            MasterOid = other.MasterOid;
            Name = other.Name;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public int OidGuild { get; set; }
        [MessagePack.Key(3)]
        [Column]
        public string Name { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public ulong MasterOid { get; set; }
        [MessagePack.Key(1)]
        [Column]
        public string Descr { get; set; }
    }
}
