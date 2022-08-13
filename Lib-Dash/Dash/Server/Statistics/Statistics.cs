#if Common_Server
using Common.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Dash.Server.Statistics
{
    using Common.StaticInfo;
    using Dash.StaticData.Episode;
    using Dash.StaticData.Mission;
    using Dash.StaticData.Reward;
    using Dash.StaticData.Shop;
    using Dash.StaticInfo;
    using Dash.Types;

    public enum ReasonType
    {
        Undefined = 0,

        FOR_HIVE_START,

        /// 재화 획득/소모
        [Description("계정 생성")]
        Account_Create = 1000,
        [Description("계정 레벨업")]
        Account_LevelUp, // 계정 레벨업
        [Description("스태미나 복구")]
        Stamina_Restore = 1050,

        [Description("상품 구매")]
        Product = 1100,
        [Description("보석 구매")]
        Product_Jewel,
        [Description("골드 구매")]
        Product_Gold,
        [Description("스태미나 구매")]
        Product_Stamina,
        [Description("패키지 구매")]
        Product_Package,
        [Description("마일리지상품 구매")]
        Product_Mileage,

        Episode = 1200,
        [Description("에피소드 클리어")]
        Episode_Clear,   // 에피소드 클리어
        [Description("에피소드 별 달성")]
        Episode_Star,    // 에피소드 별보상
        [Description("에피소드 비용")]
        Episode_Cost,

        [Description("미션 보상")]
        Mission = 1300,
        [Description("튜토리얼 보상")]
        Mission_Tutorial,
        [Description("별 수집 보상")]
        Mission_EpGroupStar, // 별 수집 미션
        [Description("퀘스트 보상")]
        Mission_Quest,   // 메인 퀘스트
        [Description("일간퀘 보상")]
        Mission_DailyQuest,  // 일일 퀘스트
        [Description("업적 보상")]
        Mission_Achievement, // 업적 보상
        [Description("일간퀘 포인트 보상")]
        Mission_DailyQuestPoint,  // 일일 퀘스트
        [Description("주간퀘 포인트 보상")]
        Mission_WeeklyQuestPoint, // 주간 퀘스트
        [Description("업적 포인트 보상")]
        Mission_AchievementPoint, // 주간 퀘스트

        [Description("우편")]
        Mail = 1400,               // 우편
        [Description("우편 월정액")]
        Mail_DailyReward,    // 우편 월정액
        [Description("우편 CS")]
        Mail_HiveItem = 1499,

        [Description("시즌패스 보상")]
        SeasonPass = 1500,         // 시즌 패스

        /// 재화 소모
        Character = 1600,
        [Description("캐릭터 강화")]
        Character_ExpUp,
        [Description("캐릭터 승급")]
        Character_OvercomeUp,
        [Description("캐릭터 돌파")]
        Character_RankUp,

        Weapon = 1700,
        [Description("무기 강화")]
        Weapon_ExpUp,
        [Description("무기 돌파")]
        Weapon_OvercomeUp,
        [Description("무기 재련")]
        Weapon_ReforgeUp,

        Armor = 1800,
        [Description("장신구 강화")]
        Armor_ExpUp,

        Gacha = 1900,
        [Description("소환 뽑기")]
        Gacha_Open,


        Exchange = 2000,
        [Description("재화 교환")]
        Money_Exchange,
        [Description("재료 교환")]
        Material_Exchange,

        Box = 2100,
        [Description("상자 사용")]
        Box_Use,
        [Description("충전형 아이템 사용")]
        MoneyBox_Use,

        ConquestReward = 2200,
        [Description("토벌 시즌보상")]
        ConquestReward_Season,
        [Description("토벌 티어보상")]
        ConquestReward_Tier,

        Sweep = 2300,
        [Description("소탕권 사용")]
        Sweep_Episode,

        EventQuest = 2400,
        [Description("이벤트 교환")]
        Event_Exchange,
        [Description("이벤트 누적 보상")]
        Event_Reward,
        [Description("이벤트 일일퀘 보상")]
        Mission_EventDailyQuest,
        [Description("이벤트 업적 보상")]
        Mission_EventAchievement,
        [Description("이벤트 뽑기")]
        Event_GachaPlay,
        [Description("이벤트 뽑기판")]
        Event_Lottery,

        [Description("광고 보상")]
        AdsReward = 2500,

        WorldMission = 2600,
        WorldMission_RankingReward,
        WorldMission_TierReward,
        WorldMission_CoopReward,
        [Description("시간자원 보상")]
        TimeResourceReward = 2700,

        [Description("에피소드입장횟수 초기화")]
        ResetEpisodeEntryCount = 2800,

        [Description("CS")]
        HiveItem = 9999,

        FOR_HIVE_END,

        /// 재화 무관
        Account_ExpUp,
        Character_UnLock,
        Rune_Unlock,
        Equipment_Decomp,
        Equipment_ToggleLock,
        /// not use
        Coupon,
    }

    public enum DetailReasonType
    {
        Undefined = 0,

        Account = 10,
        [Description("계정 레벨업 {0}")]
        Account_Level,
        Product = 20,
        Episode = 30,

        Mission = 40,
        [Description("튜토 {0}")]
        Tutorial,
        [Description("{0}/{1}별")]
        EpGroup_Star,
        [Description("퀘스트 {0}")]
        Quest,
        [Description("업적 {0}x{1}")]
        Achievement,
        [Description("일간퀘 {0}점")]
        DailyQuest_Point,
        [Description("주간퀘 {0}점")]
        WeeklyQuest_Point,
        [Description("업적 {0}점")]
        Achievement_Point,

        Gacha = 60,
    }

    public struct Reason
    {
        public ReasonType Type;
        public int Id;
        public string Name;

        public Reason(ReasonType type)
        {
            Type = type;
            Id = (int)type;
            Name = type.GetReasonName();
        }
    }

    public struct DetailReason
    {
        public long Id;
        public string Name;
        public long Id2;
        public string Name2;

        public void Format()
        {
            if (Id == 0) Name = "0";
            if (Id2 == 0) Name2 = "0";
        }
    }

    public class ReasonContext
    {
        public ReasonType Type => Reason.Type;
        public Reason Reason { get; set; }
        public DetailReason DetailReason { get; set; }

        public ReasonContext(ReasonType reasonType, DetailReasonType detailReasonType = DetailReasonType.Undefined, params object[] args)
        {
            Reason = new Reason(reasonType);
            DetailReason = Helper.MakeDetailReason(detailReasonType, args);
        }
        public ReasonContext(ReasonType reasonType, IdKeyData idKeyData, params object[] args)
        {
            Reason = new Reason(reasonType);
            DetailReason = Helper.MakeDetailReason(idKeyData, args);
        }
    }

    public static class Helper
    {
        static Dictionary<MoneyType, string> _moneyNames = new Dictionary<MoneyType, string>();
        static Dictionary<ReasonType, string> _reasonNames = new Dictionary<ReasonType, string>();
        static Dictionary<DetailReasonType, string> _detailNames = new Dictionary<DetailReasonType, string>();
        static Dictionary<EpisodeGroupType, string> _epGroupNames = new Dictionary<EpisodeGroupType, string>();

        public static void Init()
        {
            foreach (var val in EnumInfo<MoneyType>.GetValues())
            {
                FieldInfo fi = val.GetType().GetField(val.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if ((attributes?.Length ?? 0) == 0)
                    continue;

                _moneyNames.Add(val, attributes[0].Description);
            }
            foreach (var val in EnumInfo<ReasonType>.GetValues())
            {
                FieldInfo fi = val.GetType().GetField(val.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if ((attributes?.Length ?? 0) == 0)
                    continue;

                _reasonNames.Add(val, attributes[0].Description);
            }
            foreach (var val in EnumInfo<DetailReasonType>.GetValues())
            {
                FieldInfo fi = val.GetType().GetField(val.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if ((attributes?.Length ?? 0) == 0)
                    continue;

                _detailNames.Add(val, attributes[0].Description);
            }
            foreach (var val in EnumInfo<EpisodeGroupType>.GetValues())
            {
                FieldInfo fi = val.GetType().GetField(val.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if ((attributes?.Length ?? 0) == 0)
                    continue;

                _epGroupNames.Add(val, attributes[0].Description);
            }
#if DEBUG && false
            foreach (var kv in _moneyNames)
            {
                var assetId = GetAssetId(kv.Key);
                Common.Log.Logger.Info($"{assetId}:{kv.Value}");
            }
            foreach (var kv in _reasonNames)
            {
                Common.Log.Logger.Info($"{(int)kv.Key}:{kv.Value}");
            }

            var detailReasons = new List<DetailReason>();
            foreach (var info in StaticInfo.Instance.AccountLevelUpInfo.GetList())
            {
                detailReasons.Add(MakeDetailReason(DetailReasonType.Account_Level, info.Level));
            }
            foreach (var info in StaticInfo.Instance.ProductInfo.GetList())
            {
                detailReasons.Add(MakeDetailReason(info));
            }
            foreach (var info in StaticInfo.Instance.GachaInfo.GetList())
            {
                detailReasons.Add(MakeDetailReason(info));
            }
            foreach (var info in StaticInfo.Instance.EpisodeInfo.GetList())
            {
                if (info.Type == EpisodeType.Editor || info.Type == EpisodeType.Tutorial) continue;
                detailReasons.Add(MakeDetailReason(info));
            }
            foreach (var info in StaticInfo.Instance.MissionInfos.Values)
            {
                if (info.Type == MissionType.BattleTutorial) continue;
                detailReasons.Add(MakeDetailReason(info));
            }

            foreach (var v in detailReasons)
            {
                Common.Log.Logger.Info($"{v.Id}:{v.Name}");
            }
#endif
        }

        public static string GetReasonName(this ReasonType reason)
        {
            _reasonNames.TryGetValue(reason, out var name);
            return name;
        }
        public static int GetAssetId(MoneyType moneyType)
        {
            //return (delta < 0 ? 2000 : 1000) + (int)moneyType;
            return (int)moneyType;
        }
        public static string GetAssetName(this MoneyType moneyType)
        {
            _moneyNames.TryGetValue(moneyType, out var name);
            return name;
        }
        public static string GetEpGroupName(this EpisodeGroupType groupType)
        {
            _epGroupNames.TryGetValue(groupType, out var name);
            return name;
        }

        public static DetailReason MakeDetailReason(DetailReasonType detailReason, params object[] args)
        {
            _detailNames.TryGetValue(detailReason, out var name);
            switch (detailReason)
            {
                case DetailReasonType.Account_Level:
                    if (args.Length > 0 && args[0] is int)
                    {
                        var level = (int)args[0];
                        long id = (long)((long)detailReason * Math.Pow(10, 8) + level);
                        return new DetailReason { Id = id, Name = string.Format(name, level) };
                    }
                    break;
            }
            return default;
        }
        public static DetailReason MakeDetailReason(IdKeyData idKeyData, params object[] args)
        {
            switch (idKeyData)
            {
                case ProductInfo productInfo:
                    return MakeDetailReason(productInfo);
                case GachaInfo gachaInfo:
                    return MakeDetailReason(gachaInfo);
                case EpisodeInfo episodeInfo:
                    return MakeDetailReason(episodeInfo);
                case MissionInfo missionInfo:
                    return MakeDetailReason(missionInfo);
                default:
                    return default;
            }
        }
        public static DetailReason MakeDetailReason(ProductInfo info)
        {
            var detailReason = DetailReasonType.Product;
            var id = (long)((long)detailReason * Math.Pow(10, 8) + info.Id);
            return new DetailReason { Id = id, Name = info.Name.GetValue() };
        }
        public static DetailReason MakeDetailReason(GachaInfo info)
        {
            var detailReason = DetailReasonType.Gacha;
            var id = (long)((long)detailReason * Math.Pow(10, 8) + info.Id);
            return new DetailReason { Id = id, Name = info.Name.GetValue() };
        }
        public static DetailReason MakeDetailReason(EpisodeInfo info)
        {
            var detailReason = DetailReasonType.Episode;
            var id = (long)((long)detailReason * Math.Pow(10, 8) + info.EpisodeGroupId * Math.Pow(10, 3));
            var id2 = (long)((long)detailReason * Math.Pow(10, 8) + info.Id);
            return new DetailReason { Id = id, Name = info.GroupInfo.GetFullName(), Id2 = id2, Name2 = info.GetFullName() };
        }
        public static DetailReason MakeDetailReason(MissionInfo info)
        {
            switch (info)
            {
                //case LobbyTutorialInfo lobbyTutorialInfo:
                //    return MakeDetailReason(lobbyTutorialInfo);
                case EpisodeGroupMissionInfo episodeGroupMissionInfo:
                    return MakeDetailReason(episodeGroupMissionInfo);
                    //case QuestInfo questInfo:
                    //    return MakeDetailReason(questInfo);
                    //case AchievementInfo achievementInfo:
                    //    return MakeDetailReason(achievementInfo);
                    //case PointRewardQuestInfo pointRewardQuestInfo:
                    //    return MakeDetailReason(pointRewardQuestInfo);
            }
            return default;
        }
        private static DetailReason MakeDetailReason(LobbyTutorialInfo info)
        {
            var detailReason = DetailReasonType.Tutorial;
            var id = (long)((long)detailReason * Math.Pow(10, 8) + info.Id);
            _detailNames.TryGetValue(detailReason, out var name);
            return new DetailReason { Id = id, Name = string.Format(name, info.Comment) };
        }
        private static DetailReason MakeDetailReason(EpisodeGroupMissionInfo info)
        {
            var detailReason = DetailReasonType.EpGroup_Star;
            var id = (long)((long)detailReason * Math.Pow(10, 8) + info.EpisodeGroupId * Math.Pow(10, 2) + info.StarCount);
            _detailNames.TryGetValue(detailReason, out var name);
            StaticInfo.Instance.EpisodeGroupInfo.TryGet(info.EpisodeGroupId, out var groupInfo);
            return new DetailReason { Id = id, Name = string.Format(name, groupInfo?.GetFullName(), info.StarCount) };
        }
        private static DetailReason MakeDetailReason(QuestInfo info)
        {
            var detailReason = DetailReasonType.Quest;
            var id = (long)((long)detailReason * Math.Pow(10, 8) + info.Id);
            _detailNames.TryGetValue(detailReason, out var name);
            return new DetailReason { Id = id, Name = string.Format(name, info.Name.GetValue()) };
        }
        private static DetailReason MakeDetailReason(AchievementInfo info)
        {
            var detailReason = DetailReasonType.Achievement;
            var id = (long)((long)detailReason * Math.Pow(10, 8) + info.Id);
            _detailNames.TryGetValue(detailReason, out var name);
            return new DetailReason { Id = id, Name = string.Format(name, info.Name.GetValue(), info.Count) };
        }
        private static DetailReason MakeDetailReason(PointRewardQuestInfo info)
        {
            DetailReasonType detailReason = DetailReasonType.Quest;
            switch (info)
            {
                case DailyPointRewardQuestInfo:
                    detailReason = DetailReasonType.DailyQuest_Point;
                    break;
                case WeeklyPointRewardQuestInfo:
                    detailReason = DetailReasonType.WeeklyQuest_Point;
                    break;
                case AchievementPointRewardQuestInfo:
                    detailReason = DetailReasonType.Achievement_Point;
                    break;
            }
            var id = (long)((long)detailReason * Math.Pow(10, 8) + info.Id);
            _detailNames.TryGetValue(detailReason, out var name);
            return new DetailReason { Id = id, Name = string.Format(name, info.NeedPoint) };
        }

        public static ReasonType GetReasonType(this Model.Rdb.Mail mail)
        {
            switch (mail.MailType)
            {
                case MailType.HIVE_ITEM:
                    return ReasonType.Mail_HiveItem;
                case MailType.DailyReward:
                    return ReasonType.Mail_DailyReward;
                default:
                    return ReasonType.Mail;
            }
        }

        public static ReasonType GetReasonType(this ProductInfo info)
        {
            switch (info.ShopCategory)
            {
                case ShopCategoryType.Money:
                    switch (info.SubCategory)
                    {
                        case SubCategoryType.Jewel:
                            return ReasonType.Product_Jewel;
                        case SubCategoryType.Gold:
                            return ReasonType.Product_Gold;
                        case SubCategoryType.Stamina:
                            return ReasonType.Product_Stamina;
                        default:
                            return ReasonType.Product;
                    }
                case ShopCategoryType.Mileage:
                    return ReasonType.Product_Mileage;
                case ShopCategoryType.Package:
                case ShopCategoryType.Recommend:
                    return ReasonType.Product_Package;
                default:
                    return ReasonType.Product;
            }
        }

        public static ReasonType GetReasonType(this MissionInfo info)
        {
            switch (info.Type)
            {
                case MissionType.LobbyTutorial:
                    return ReasonType.Mission_Tutorial;
                case MissionType.EpisodeGroupStar:
                    return ReasonType.Mission_EpGroupStar;
                case MissionType.Quest:
                    return ReasonType.Mission_Quest;
                case MissionType.DailyQuest:
                    return ReasonType.Mission_DailyQuest;
                case MissionType.Achievement:
                    return ReasonType.Mission_Achievement;
                case MissionType.DailyPointRewardQuest:
                    return ReasonType.Mission_DailyQuestPoint;
                case MissionType.WeeklyPointRewardQuest:
                    return ReasonType.Mission_WeeklyQuestPoint;
                case MissionType.AchievementPointRewardQuest:
                    return ReasonType.Mission_AchievementPoint;

                case MissionType.EventDailyQuest:
                    return ReasonType.Mission_EventDailyQuest;
                case MissionType.EventAchievement:
                    return ReasonType.Mission_EventAchievement;
                default:
                    return ReasonType.Mission;
            }
        }


        public static string GetFullName(this EpisodeGroupInfo info)
        {
            string fullName = string.Empty;
            switch (info.Type)
            {
                case EpisodeGroupType.World:
                    string difficutly;
                    switch (info.Difficulty)
                    {
                        case EpisodeDifficulty.Hard:
                            difficutly = "어려움";
                            break;
                        default:
                            difficutly = "보통";
                            break;
                    }
                    fullName = $"월드 {difficutly} {info.Number}";
                    break;
                default:
                    fullName = info.Name.GetValue().RemoveTag();
                    break;
            }
            return fullName;
        }

        public static string GetFullName(this EpisodeInfo info)
        {
            string fullName = string.Empty;
            switch (info.GroupType)
            {
                case EpisodeGroupType.World:
                case EpisodeGroupType.Dungeon:
                case EpisodeGroupType.PathOfGlory:
                    fullName = $"{info.GroupInfo.GetFullName()}-{info.Number}";
                    break;
                default:
                    fullName = info.Name.GetValue().RemoveTag();
                    break;
            }
            return fullName;
        }
    }
}
#endif