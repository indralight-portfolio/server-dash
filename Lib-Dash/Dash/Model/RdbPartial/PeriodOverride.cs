using Common.Utility;
using Dash.Model.Service;
using Dash.StaticData;
using MessagePack;
using Newtonsoft.Json;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    public partial class PeriodOverride
    {
        [IgnoreMember, JsonIgnore]
#if Common_Server
        [NotMapped]
#endif
        public PeriodType PeriodType
        {
            get
            {
                if (EnumInfo<PeriodType>.TryParse(Type, out var periodType) == false)
                    return PeriodType.Undefined;
                return periodType;
            }
            set
            {
                Type = value.ToString();
            }
        }
    }
}
