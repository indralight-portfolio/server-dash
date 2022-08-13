using Common.Utility;
using Dash.Model.Service;
using Dash.StaticData;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    public partial class Mail
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
                return mailData;
            }
            set
            {
                mailData = value;
                Data = JsonGzipSerializer.Serialize(value);
                //var data2 = MessagePackSerializer.Serialize(dataObject.GetType(), value);

                //Common.Log.Logger.Debug($"data1 : {Data.Length}");    126
                //Common.Log.Logger.Debug($"data2 : {data3.Length}");   27
            }
        }

        public void SetData(MailBodyModel body = null, RewardInfo reward = null)
        {
            if (reward == null || reward.IsEmpty())
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
        }

        public bool IsValid()
        {
            if (MailData == null)
                return false;
            if (MailData is IRewardDataModel<RewardInfo> rewardableDataObject && rewardableDataObject.GetReward() == null)
                return false;
            return MailData.Body?.IsValid() ?? false;
        }

        public Mail(ulong oidAccount, MailTemplate template)
        {
            OidAccount = oidAccount;
            Type = template.Type;
            Data = template.Data;
            Created = template.Start;
            Expire = template.Expire ?? template.Start.AddDays(template.ExpireDays ?? 7);
        }

        public Mail(ulong oidAccount, MailType mailType, MailDataModel mailData, DateTime created)
        {
            OidAccount = oidAccount;
            MailType = mailType;
            MailData = mailData;
            Created = created;
            Expire = created.AddDays(7);
        }
    }
}
