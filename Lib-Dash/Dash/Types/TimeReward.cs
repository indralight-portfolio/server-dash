using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dash.Types
{
    //DB에 string으로 들어가기때문에 50자를 넘어선 안된다
    public enum TimeRewardType
    {
        Undefined = 0,
        RestoreStamina,
        AddStaminaByAds,
        Product,
        Dungeon,
        DailyQuest,
        WeeklyQuest,
        Achievement,
        Ads,
        TriggerPackage, // See extension

        DailyQuestEvent,
        DailyEpisodeEntryLimit,//나중에 위클리가 추가될수 있을듯
        NewbieGacha,
        TimeResource,
    }

    public enum TimeRewardResetType
    {
        Undefined = 0,
        Daily,
        Weekly,
        Monthly,
        Custom = 99,
    }
}