using Common.Locale;
using Common.Utility;
using Dash.StaticData;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Dash.Model.Service
{
    public abstract class MailDataModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [Key(0)]
        public MailDataType Type { get; set; }
        [Key(1)]
        public MailBodyModel Body { get; set; }
        [Key(2)]
        public bool ManualRead { get; set; } = false;
    }

    [MessagePackObject]
    public class RewardMailDataModel : MailDataModel, IRewardDataModel<RewardInfo>
    {
        public RewardMailDataModel() { Type = MailDataType.Reward; }
        [Key(3)]
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

    [MessagePackObject]
    public class SimpleMailDataModel : MailDataModel
    {
        public SimpleMailDataModel() { Type = MailDataType.Simple; }
    }

    [MessagePackObject]
    public class MailBodyModel
    {
        [Key(0)]
        public LocaleWithArgs Title { get; set; }
        [Key(1)]
        public List<LocaleWithArgs> Message { get; set; }

        public bool IsValid()
        {
            return (Title?.IsNullOrEmpty() ?? true) == false || (Message?.Count ?? 0) > 0;
        }
        public bool IsEmpty()
        {
            return (Title?.IsNullOrEmpty() ?? true) == true && (Message?.Count ?? 0) == 0;
        }
        public MailBodyModel CheckEmpty()
        {
            if (IsEmpty() == false)
                return this;
            else
                return null;
        }
    }

    public class MailDataModelConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (EnumInfo<MailDataType>.TryParse(jo["Type"].Value<string>(), out MailDataType type) == false)
            {
                return null;
            }
            switch (type)
            {
                case MailDataType.Simple:
                    return jo.ToObject<SimpleMailDataModel>(serializer);
                case MailDataType.Reward:
                    return jo.ToObject<RewardMailDataModel>(serializer);
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MailDataModel);
        }
    }

    public static class MailHelper
    {
        public static Dictionary<MailType, MailDataType> MailTypeDataType = new Dictionary<MailType, MailDataType>();
        static MailHelper()
        {
            foreach (var type in EnumInfo<MailType>.GetValues())
            {
                MailTypeDataType.Add(type, type.GetDataType());
            }
        }

        public static MailDataType GetDataType(this MailType mailType)
        {
            switch (mailType)
            {
                case MailType.News:
                    return MailDataType.Simple;
                default:
                    return MailDataType.Reward;
            }
        }

        public static MailBodyModel GetDefaultMailBody(this MailType mailType)
        {
            LocaleWithArgs title = null;
            List<LocaleWithArgs> message = null;
            switch (mailType)
            {
                case MailType.Maintenance:
                    title = new LocaleWithArgs(CodeLocale.MailService_Mail_Maintenance2_Title.GetKey());
                    message = new List<LocaleWithArgs> { new LocaleWithArgs(CodeLocale.MailService_Mail_Maintenance2_Message.GetKey()) };
                    break;
                case MailType.OverLimit:
                    title = new LocaleWithArgs(CodeLocale.MailService_Mail_ExcessItem_Title.GetKey());
                    message = new List<LocaleWithArgs> { new LocaleWithArgs(CodeLocale.MailService_Mail_ExcessItem_Message.GetKey()) };
                    break;
                case MailType.DailyReward:
                    title = new LocaleWithArgs(CodeLocale.MailService_Mail_DailyReward_Title.GetKey());
                    message = new List<LocaleWithArgs> { new LocaleWithArgs(CodeLocale.MailService_Mail_DailyReward_Message.GetKey()) };
                    break;
                case MailType.ProductBuy:
                    title = new LocaleWithArgs(CodeLocale.MailService_Mail_ProductBuy_Title.GetKey());
                    message = new List<LocaleWithArgs> { new LocaleWithArgs(CodeLocale.MailService_Mail_ProductBuy_Message.GetKey()) };
                    break;
                case MailType.CS:
                default:
                    title = new LocaleWithArgs(CodeLocale.MailService_Mail_Reward_Title.GetKey());
                    message = new List<LocaleWithArgs> { new LocaleWithArgs(CodeLocale.MailService_Mail_Reward_Message.GetKey()) };
                    break;
            }
            var mailBody = new MailBodyModel
            {
                Title = title,
                Message = message,
            };

            return mailBody;
        }
        public static MailDataModel GetDefaultMailData(this MailType mailType, RewardInfo rewardInfo = null)
        {
            MailDataModel mailData;
            if (rewardInfo?.IsEmpty() == false)
            {
                mailData = new RewardMailDataModel
                {
                    Body = mailType.GetDefaultMailBody(),
                    Reward = rewardInfo,
                };
                if (mailType == MailType.ProductBuy)
                {
                    mailData.ManualRead = true;
                }
            }
            else
            {
                mailData = new SimpleMailDataModel
                {
                    Body = mailType.GetDefaultMailBody(),
                };
            }
            return mailData;
        }

        public static MailBodyModel GetMailBody(this MailBodyType mailBodyType)
        {
            LocaleWithArgs title = null;
            List<LocaleWithArgs> message = null;
            switch (mailBodyType)
            {
                case MailBodyType.Maintenance:
                    title = new LocaleWithArgs(CodeLocale.MailService_Mail_Maintenance2_Title.GetKey());
                    message = new List<LocaleWithArgs> { new LocaleWithArgs(CodeLocale.MailService_Mail_Maintenance2_Message.GetKey()) };
                    break;
                case MailBodyType.Reward:
                    title = new LocaleWithArgs(CodeLocale.MailService_Mail_Reward_Title.GetKey());
                    message = new List<LocaleWithArgs> { new LocaleWithArgs(CodeLocale.MailService_Mail_Reward_Message.GetKey()) };
                    break;
                case MailBodyType.Compensation:
                    title = new LocaleWithArgs(CodeLocale.MailService_Mail_Compensation_Title.GetKey());
                    message = new List<LocaleWithArgs> { new LocaleWithArgs(CodeLocale.MailService_Mail_Compensation_Message.GetKey()) };
                    break;
                default:
                    break;
            }
            var mailBody = new MailBodyModel
            {
                Title = title,
                Message = message,
            };

            return mailBody;
        }
    }
}