using System;
using System.Collections.Generic;
using System.Text;

namespace Dash.Types
{
    public enum PaymentType
    {
        Undefined,
        Cash,
        Jewel,
        Mileage,
        TimeReward,
        Ads,
        ConquestPoint,
        Free,
    }
    // 수정 시 Prefab 수정 필요
    public enum ShopCategoryType
    {
        Undefined,
        Money,
        Recommend,
        Package,
        SeasonPass,
        Mileage,
        Trigger,
        Conquest,//토벌은 별도의 상점팝업이 존재한다
    }
    // 수정 시 Prefab 수정 필요
    public enum SubCategoryType
    {
        Undefined,
        Jewel,
        Gold,
        Stamina,
        Ticket,
        Recommend_Limited, 
        Recommend_Growth,
        Recommend_Money,
        Recommend_DailyReward,
        Recommend_Ads,
        Package_Water,
        Package_Fire,
        Package_Air,
        Package_Earth,
        Package_Lightning,
        Package_Equipment,
        Mileage_Money,
        Mileage_Water,
        Mileage_Fire,
        Mileage_Air,
        Mileage_Earth,
        Mileage_Lightning,
        Mileage_Equipment,
        Etc,
        Conquest_CharcterSoul,
        Conquest_Growth,
        Conquest_Character_Overcome,
        Conquest_Weapon_Overcome,

        //1.18 추가 -> 중간에 넣으면 프리팹설정이 바뀜
        Recommend_StepUp,
        Recommend_Welcome,
    }
    public enum OpenBoxType
    {
        Buy,
        Key,
        TimeReward,
    }
    public enum BuyLimitType
    {
        Unlimited,
        Permanently,
        Daily,
        Weekly,
        Monthly,
        Periodically,
    }

    public static class ShopTypeHelper
    {
        public static CodeLocale ToCodeLocaleType(this BuyLimitType actionGroup)
        {
            switch (actionGroup)
            {
                case BuyLimitType.Permanently:
                    return CodeLocale.Dash_Types_BuyLimitType_Permanently;
                case BuyLimitType.Daily:
                    return CodeLocale.Dash_Types_BuyLimitType_Daily;
                case BuyLimitType.Weekly:
                    return CodeLocale.Dash_Types_BuyLimitType_Weekly;
                case BuyLimitType.Monthly:
                    return CodeLocale.Dash_Types_BuyLimitType_Monthly;
                case BuyLimitType.Periodically:
                    return CodeLocale.Dash_Types_BuyLimitType_Periodically;
                default:
                    return CodeLocale.Undefined;
            }
        }
    }
}
