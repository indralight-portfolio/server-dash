using Common.Utility;
using Dash.Model.Service;
using Dash.StaticData;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    public partial class MailTemplate
    {
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public MailType MailType
        {
            get { return EnumInfo<MailType>.TryParse(Type, out MailType type) ? type : MailType.News; }
            set { Type = value.ToString(); }
        }

        private MailDataModel mailData;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public MailDataModel MailData
        {
            get
            {
                if (mailData == null && Data?.Length > 0)
                {
                    try
                    {
                        var json = JsonGzipSerializer.Unzip(Data);
                        var jObject = JObject.Parse(json);
                        if (jObject.TryGetValue(nameof(MailDataModel.Type), out var type) == false)
                            return null;

                        EnumInfo<MailDataType>.TryParse(type.Value<string>(), out var mailDataType);
                        switch (mailDataType)
                        {
                            case MailDataType.Simple:
                                mailData = JsonConvert.DeserializeObject<SimpleMailDataModel>(json);
                                break;
                            case MailDataType.Reward:
                                mailData = JsonConvert.DeserializeObject<RewardMailDataModel>(json);
                                break;
                        }
                    }
                    catch { return null; }
                }
                return mailData;
            }
            set
            {
                mailData = value;
                Data = JsonGzipSerializer.Serialize(value);
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
                    try
                    {
                        condition = JsonGzipSerializer.Deserialize<ConditionModel>(DataCondition);
                    }
                    catch { return null; }
                }
                return condition;
            }
            set
            {
                condition = value;
                DataCondition = JsonGzipSerializer.Serialize(condition);
            }
        }

        public void SetData(MailBodyModel body, RewardInfo reward = null, ConditionModel condition = null)
        {
            if (reward == null || reward.IsEmpty() == true)
            {
                MailData = new SimpleMailDataModel
                {
                    Body = body.CheckEmpty(),
                };
            }
            else
            {
                MailData = new RewardMailDataModel
                {
                    Body = body.CheckEmpty(),
                    Reward = reward.CheckEmpty(),
                };
            }
            Condition = condition?.CheckEmpty();
        }

        public bool IsValid()
        {
            if (MailData == null)
                return false;
            if (MailData is IRewardDataModel<RewardInfo> rewardableDataObject && rewardableDataObject.GetReward() == null)
                return false;
            return MailData.Body?.IsValid() ?? false;
        }
    }
}
