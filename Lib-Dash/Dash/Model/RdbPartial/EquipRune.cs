using Common.Utility;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

#nullable disable

namespace Dash.Model.Rdb
{
    public partial class EquipRune
    {
        private Dictionary<byte /*Tier*/, sbyte /*idx*/> runeIdexes;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public Dictionary<byte, sbyte> RuneIndexes
        {
            get
            {
                if (runeIdexes == null && Data?.Length > 0)
                {
                    try
                    {
                        runeIdexes = JsonGzipSerializer.Deserialize<Dictionary<byte, sbyte>>(Data);
                    }
                    catch { return new Dictionary<byte, sbyte>(); }
                }
                return runeIdexes;
            }
            set
            {
                runeIdexes = value;
                Data = JsonGzipSerializer.Serialize(runeIdexes);
            }
        }
    }
}
