using Common.Utility;
using Dash.StaticData.Reward;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

#nullable disable

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    public partial class SeasonPass
    {
        public SeasonPass(ulong oidAccount, int id)
        {
            OidAccount = oidAccount;
            Id = id;
            PassPoint = 0;
            Premium = false;
        }

        private SeasonPassInfo _info;
        [IgnoreMember, JsonIgnore]
        public SeasonPassInfo Info
        {
            get
            {
                if (_info == null)
                {
                    StaticInfo.Instance.SeasonPassInfo.TryGet(Id, out _info);
                }
                return _info;
            }
        }

        private List<int> rewardedCommon;
        private List<int> rewardedPremium;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public List<int> RewardedCommon
        {
            get
            {
                if (rewardedCommon == null && CommonRewardedData?.Length > 0)
                {
                    try
                    {
                        rewardedCommon = JsonGzipSerializer.Deserialize<List<int>>(CommonRewardedData);
                    }
                    catch { return new List<int>(); }
                }
                return rewardedCommon;
            }
            set
            {
                rewardedCommon = value;
                if (rewardedCommon != null)
                    CommonRewardedData = JsonGzipSerializer.Serialize(rewardedCommon);
                else
                    CommonRewardedData = null;
            }
        }

#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public List<int> RewardedPremium
        {
            get
            {
                if (rewardedPremium == null && PrimiumRewardedData?.Length > 0)
                {
                    try
                    {
                        rewardedPremium = JsonGzipSerializer.Deserialize<List<int>>(PrimiumRewardedData);
                    }
                    catch { return new List<int>(); }
                }
                return rewardedPremium;
            }
            set
            {
                rewardedPremium = value;
                if (rewardedPremium != null)
                    PrimiumRewardedData = JsonGzipSerializer.Serialize(rewardedPremium);
                else
                    PrimiumRewardedData = null;
            }
        }

        public int GetLevel()
        {
            var rewardInfo = Info.SeasonPassReward.Values.LastOrDefault(e => e.PassPoint <= PassPoint);
            return rewardInfo?.Level ?? Info.SeasonPassReward.Keys.Max();
        }
    }
}
