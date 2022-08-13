using Common.Utility;
using Dash.Model.Service;
using Dash.StaticData;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if Common_Server
using Dash.Server;
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    public partial class ReservedMail
    {
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public MailBodyType MailBodyType
        {
            get { return EnumInfo<MailBodyType>.TryParse(BodyType, out MailBodyType type) ? type : MailBodyType.Undefined; }
            set { BodyType = value.ToString(); }
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

        private MailConditionModel condition;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public MailConditionModel Condition
        {
            get
            {
                if (condition == null && DataCondition?.Length > 0)
                {
                    try
                    {
                        condition = JsonGzipSerializer.Deserialize<MailConditionModel>(DataCondition);
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

        private RepeatModel repeat;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public RepeatModel Repeat
        {
            get
            {
                if (repeat == null && DataRepeat?.Length > 0)
                {
                    try
                    {
                        repeat = JsonGzipSerializer.Deserialize<RepeatModel>(DataRepeat);
                    }
                    catch { return null; }
                }
                return repeat;
            }
            set
            {
                repeat = value;
                DataRepeat = JsonGzipSerializer.Serialize(repeat);
            }
        }

        public void SetData(RewardInfo reward = null, MailConditionModel condition = null, RepeatModel repeat = null)
        {
            MailBodyModel body = MailBodyType.GetMailBody();
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
            Repeat = repeat;
        }

        public bool IsValid()
        {
            if (MailBodyType == MailBodyType.Undefined)
                return false;
            if (MailData == null)
                return false;
            if (MailData is IRewardDataModel<RewardInfo> rewardableDataObject && rewardableDataObject.GetReward() == null)
                return false;
            return MailData.Body?.IsValid() ?? false;
        }

#if Common_Server
        public bool Check(ReservedMailSent sent, out Period period)
        {
            period = default;
            var serverTime = ServerTimeHelper.GetServerTime();

            period.Start = LocalStart;
            period.End = LocalEnd;
            if (Repeat != null && Repeat.RepeatType != RepeatType.None)
            {
                period.Start = serverTime.Local.Date.Add(LocalStart - LocalStart.Date);
                period.End = serverTime.Local.Date.Add(LocalEnd - LocalEnd.Date);
                if (period.End < period.Start)
                {
                    if (period.End > serverTime.Local)
                        period.Start = period.Start.AddDays(-1);
                    else
                        period.End = period.End.AddDays(1);
                }
            }

            if (Repeat?.RepeatType == RepeatType.Weekday)
            {
                if (Repeat.DayOfWeeks == null)
                    return false;

                if (Repeat.DayOfWeeks.Contains(period.Start.DayOfWeek) == false)
                    return false;
            }

            if (period.Start > serverTime.Local || period.End < serverTime.Local)
                return false;

            if (sent != null && sent.Sent.ToLocalTime(serverTime.Offset) >= period.Start)
                return false;

            return true;
        }
#endif
    }
}
