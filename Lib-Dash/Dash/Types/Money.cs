using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace Dash.Types
{
    //DB에 string으로 들어가기때문에 20자를 넘어선 안된다
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MoneyType
    {
        Undefined = 0,
        FreeMoney_Start = 100,
        [Description("루비")]
        Jewel,
        [Description("골드")]
        Gold,
        [Description("스태미나")]
        Stamina,
        [Description("마일리지")]
        Mileage,
        [Description("일반소환권")]
        GachaTicket,
        [Description("한정소환권")]
        GachaLimitedTicket,
        [Description("토벌 코인")]
        ConquestPoint,
        FreeMoney_End = 199,
        PaidMoney_Start = 200,
        Jewel_Paid,
        Gold_Paid,
        PaidMoney_End = 299,
        SpecialMoney_Start = 300,
        SeasonPoint,
        DailyQuestPoint,
        WeeklyQuestPoint,
        AchievementPoint,
        SpecialMoney_End = 9999,
    }

    public static class MoneyTypeHelper
    {
        public static bool IsPaid(this MoneyType type)
        {
            switch (type)
            {
                case MoneyType.Jewel_Paid:
                case MoneyType.Gold_Paid:
                    return true;
                default:
                    return false;
            }
        }
        public static MoneyType GetFreeMoneyType(this MoneyType type)
        {
            switch (type)
            {
                case MoneyType.Jewel_Paid:
                    return MoneyType.Jewel;
                case MoneyType.Gold_Paid:
                    return MoneyType.Gold;
                default:
                    return type;
            }
        }
        public static NonItemType GetNonItemType(this MoneyType type)
        {
            switch (type)
            {
                case MoneyType.Jewel:
                case MoneyType.Jewel_Paid:
                    return NonItemType.Jewel;
                case MoneyType.Gold:
                case MoneyType.Gold_Paid:
                    return NonItemType.Gold;
                case MoneyType.Stamina:
                    return NonItemType.Stamina;
                case MoneyType.Mileage:
                    return NonItemType.Mileage;
                case MoneyType.GachaTicket:
                    return NonItemType.GachaTicket;
                case MoneyType.GachaLimitedTicket:
                    return NonItemType.GachaLimitedTicket;
                case MoneyType.SeasonPoint:
                    return NonItemType.SeasonPoint;
                case MoneyType.DailyQuestPoint:
                    return NonItemType.DailyQuestPoint;
                case MoneyType.WeeklyQuestPoint:
                    return NonItemType.WeeklyQuestPoint;
                case MoneyType.AchievementPoint:
                    return NonItemType.AchievementPoint;
                case MoneyType.ConquestPoint:
                    return NonItemType.ConquestPoint;
                default:
                    return NonItemType.Undefined;
            }
        }
        public static bool IsQuestPoint(this MoneyType type)
        {
            switch (type)
            {
                case MoneyType.DailyQuestPoint:
                case MoneyType.WeeklyQuestPoint:
                case MoneyType.AchievementPoint:
                    return true;
                default:
                    return false;
            }
        }

        public static CodeLocale GetLocaleName(this MoneyType type)
        {
            return type.GetNonItemType().GetLocaleName();
        }
    }
}
