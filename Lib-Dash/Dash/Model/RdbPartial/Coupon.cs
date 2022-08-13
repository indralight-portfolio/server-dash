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
    public partial class Coupon
    {
        private CouponDataModel dataObject;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public CouponDataModel DataObject
        {
            get
            {
                if (dataObject == null && Data?.Length > 0)
                {
                    try
                    {
                        dataObject = JsonGzipSerializer.Deserialize<CouponDataModel>(Data);
                    }
                    catch { return null; }
                }
                return dataObject;
            }
            set
            {
                dataObject = value;
                Data = JsonGzipSerializer.Serialize(dataObject);
            }
        }

        private ConditionModel condition;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public ConditionModel Condition
        {
            get
            {
                if (condition == null && DataCondition?.Length > 0)
                {
                    condition = JsonGzipSerializer.Deserialize<ConditionModel>(DataCondition);
                }
                return condition;
            }
            set
            {
                condition = value;
                DataCondition = JsonGzipSerializer.Serialize(condition);
            }
        }

        public void SetData(RewardInfo reward = null, ConditionModel condition = null)
        {
            DataObject = new CouponDataModel { Reward = reward.CheckEmpty() };
            Condition = condition.CheckEmpty();
        }

        public bool IsValid()
        {
            if (DataObject == null)
                return false;
            return DataObject.HasReward();
        }

        public void SetCodeLower() { Code = Code?.ToLower(); }

        public static string GenerateCode()
        {
            return Random.NextString(10, lowerCase: true, specialChar: false);
        }
    }
}
