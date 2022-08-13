using Dash.StaticData.Reward;
using MessagePack;
using Newtonsoft.Json;

#nullable disable

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    public partial class DailyReward
    {
        private DailyRewardInfo _info;
        [IgnoreMember, JsonIgnore]
        public DailyRewardInfo Info
        {
            get
            {
                if (_info == null)
                {
                    StaticInfo.Instance.DailyRewardInfo.TryGet(Id, out _info);
                }
                return _info;
            }
        }
    }
}
