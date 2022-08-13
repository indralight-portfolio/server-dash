using MessagePack;
using Newtonsoft.Json;
using Dash.StaticData;
using Common.Utility;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    public partial class GachaHistory
    {
        private RewardInfo reward;
        private RewardInfo convertedReward;

#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public RewardInfo Reward
        {
            get
            {
                if (reward == null && RewardData?.Length > 0)
                {
                    try
                    {
                        reward = JsonGzipSerializer.Deserialize<RewardInfo>(RewardData);
                    }
                    catch { return new RewardInfo(); }
                }
                return reward;
            }
            set
            {
                reward = value;
                RewardData = JsonGzipSerializer.Serialize(reward);
            }
        }


#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public RewardInfo ConvertedReward
        {
            get
            {
                if (convertedReward == null && ConvertedRewardData?.Length > 0)
                {
                    try
                    {
                        convertedReward = JsonGzipSerializer.Deserialize<RewardInfo>(ConvertedRewardData);
                    }
                    catch { return new RewardInfo(); }
                }
                return convertedReward;
            }
            set
            {
                convertedReward= value;
                ConvertedRewardData = JsonGzipSerializer.Serialize(convertedReward);
            }
        }
    }
}

