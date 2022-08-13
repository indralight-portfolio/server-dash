using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dash.Types
{
    //DB에 string으로 들어가기때문에 20자를 넘어선 안된다
    public enum MailType
    {
        Undefined,
        News,
        CS,
        Reserved,
        Maintenance,
        OverLimit,
        Attendance,
        GuildInvite,
        GuildKick,
        DailyReward,
        ProductBuy,
        HIVE_ITEM,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MailDataType
    {
        Undefined,
        Simple,
        Reward,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MailBodyType
    {
        Undefined,
        Maintenance,
        Reward,
        Compensation,
    }


    //DB에 string으로 들어가기때문에 20자를 넘어선 안된다
    public enum MailTargetType
    {
        All,
        Group,
    }

    public enum MailTargetStatus
    {
        Invalid = -1,
        Ready = 0,
        Sent,
    }
}