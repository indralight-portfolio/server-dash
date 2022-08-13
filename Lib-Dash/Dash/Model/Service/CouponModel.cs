using Dash.StaticData;
using MessagePack;

namespace Dash.Model.Service
{
    [MessagePackObject]
    public class CouponDataModel : IRewardDataModel<RewardInfo>
    {
        [Key(0)]
        public RewardInfo Reward { get; set; }

        public bool HasReward() => Reward != null;
        public RewardInfo GetReward() => Reward;
        public bool TryGetReward(out RewardInfo rewardInfo)
        {
            rewardInfo = GetReward();
            return HasReward();
        }
        public void SetReward(RewardInfo rewardInfo) { Reward = rewardInfo; }
    }
}