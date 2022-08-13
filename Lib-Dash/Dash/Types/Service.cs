using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Dash.Types
{
    public enum PlatformType
    {
        Undefined = 0,
        Editor,
        Android,
        iOS,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceState
    {
        Close,
        TesterOnly,
        Open,
    }

    public enum AuthType : byte
    {
        UnityEditor,
        Android,
        IOS,
        PlayGames,
        GameCenter,
        UnitySocial,
        Hive,
        Dummy = 99,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarketType : byte
    {
        Undefined,
        GooglePlay,
        AppleAppStore,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StoreType : byte
    {
        Fake,
        GooglePlay,
        AppleAppStore,
        HiveIAP,
    }

    public enum EndArenaReasonType
    {
        Undefined = 0,
        GameEnd,
        PlayerExit,
        ArenaReadyTimeOver,
        LackOfCost,
    }

    [Flags]
    public enum TopInfoType
    {
        Undefined = 0,
        Account = 0x01 << 1,
        //HardJewel = 0x01 << 2,
        Jewel = 0x01 << 3,
        Gold = 0x01 << 4,
        Stamina = 0x01 << 5,
        Mileage = 0x01 << 6,
        GachaTicket = 0x01 << 7,
        GachaLimitedTicket = 0x01 << 8,
        ConqusetPoint = 0x01 << 9,
    }
    public enum MenuLockReasonType
    {
        Undefined = 0,
        Static,
        Tutorial,
        PartyMain,
    }

    public enum MenuButtonType
    {
        Undefined = 0,
        SeasonPass,
        Recommend,
        Ads,
    }
    public enum AfterEnterLobbyType
    {
        None,
        EpisodeGroupInfo,
        EpisodeInfo,
        Deck,
        Gacha,
        CharacterGrowth,
        WeaponGrowth,
        Shop,
        WorldMissionEvent,
    }

    public enum UIServiceType
    {
        Player,
        Battle,
        Mission,
    }
    public enum ServiceAreaType
    {
        Undefined,
        Client,
        Lobby,
        Match,
        Battle,
        Social,
        Relay,
        Dummy,
        All,
    }

    public static class TopInfoHelper
    {
        public static MoneyType ToMoneyType(this TopInfoType topInfoType)
        {
            switch (topInfoType)
            {
                case TopInfoType.Jewel:
                    return MoneyType.Jewel;
                case TopInfoType.Gold:
                    return MoneyType.Gold;
                case TopInfoType.Stamina:
                    return MoneyType.Stamina;
                case TopInfoType.Mileage:
                    return MoneyType.Mileage;
                case TopInfoType.GachaTicket:
                    return MoneyType.GachaTicket;
                case TopInfoType.GachaLimitedTicket:
                    return MoneyType.GachaLimitedTicket;
                case TopInfoType.ConqusetPoint:
                    return MoneyType.ConquestPoint;
                default:
                    return MoneyType.Undefined;
            }
        }

        public static TopInfoType ToTopInfoType(this MoneyType moneyType)
        {
            switch (moneyType)
            {
                case MoneyType.Jewel:
                    return TopInfoType.Jewel;
                case MoneyType.Gold:
                    return TopInfoType.Gold;
                case MoneyType.Stamina:
                    return TopInfoType.Stamina;
                case MoneyType.Mileage:
                    return TopInfoType.Mileage;
                case MoneyType.GachaTicket:
                    return TopInfoType.GachaTicket;
                case MoneyType.GachaLimitedTicket:
                    return TopInfoType.GachaLimitedTicket;
                case MoneyType.ConquestPoint:
                    return TopInfoType.ConqusetPoint;
                default:
                    return TopInfoType.Undefined;
            }
        }
    }
}