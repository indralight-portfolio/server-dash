using Common.StaticInfo;
using Common.Utility;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dash.Core.Entities.BehaviorTree;
using Dash.StaticData.Item;

namespace Dash.StaticInfo
{
    using Common.Locale;
    using Dash.Core.Action.Property;
    using Dash.Core.Math;
    using Dash.Core.Option;
    using Dash.Model.Service;
    using Dash.StaticData.Collection;
    using Dash.StaticData.Event;
    using Dash.StaticData.MainMenu;
    using Dash.StaticData.Mission;
    using Dash.StaticData.Reward;
    using Dash.StaticData.Shop;
    using Dash.StaticData.Tower;
    using StaticData;
    using StaticData.Direction;
    using StaticData.Entity;
    using StaticData.Episode;
    using StaticData.Stage;
    using System;
    using System.Reflection;
    using Types;
    using Unity.Mathematics;
    using EventInfo = StaticData.Event.EventInfo;

    public class StaticInfo : Common.StaticInfo.AbstractStaticInfo
    {
        private static StaticInfo _instance;
        public static StaticInfo Instance => _instance ?? (_instance = new StaticInfo());

        // /        
        public readonly KeyValueInfo<int, RuleInfo> RuleInfo;
        public readonly KeyValueInfo<int, SkillInfo> SkillInfo;
        public readonly KeyValueInfo<int, AccountLevelUpInfo> AccountLevelUpInfo;
        public readonly KeyValueInfo<int, RewardChestInfo> RewardChestInfo;
        public readonly KeyValueInfo<int, CoreSimulationInfo> CoreSimulationInfo;
        public readonly KeyValueInfo<string, HelpInfo> HelpInfo;

        // /Direction
        public readonly KeyValueInfo<int, PerlinNoiseShakeInfo> PerlinNoiseShakeInfo;
        public readonly KeyValueInfo<int, CurveShakeInfo> CurveShakeInfo;
        public readonly KeyValueInfo<int, ScaleAnimationInfo> ScaleAnimationInfo;
        //

        // Episode
        public readonly KeyValueInfo<int, EpisodeGroupInfo> EpisodeGroupInfo;
        //public readonly KeyValueInfo<int, EpisodeGroupAdditionalInfo> EpisodeGroupAdditionalInfo;
        public readonly KeyValueInfo<int, EpisodeInfo> EpisodeInfo;
        public readonly KeyValueInfo<int, EpisodeAdditionalInfo> EpisodeAdditionalInfo;
        public readonly KeyValueInfo<int, EpisodeRewardInfo> EpisodeRewardInfo;
        public readonly KeyValueInfo<int, EpisodeMissionInfo> EpisodeMissionInfo;
        public readonly KeyValueInfo<int, EpisodeMonsterStatusDeltaInfo> EpisodeMonsterStatusDeltaInfo;
        // Stage
        public readonly KeyValueInfo<int, StageInfo> StageInfo;
        public readonly KeyValueInfo<int, TileSetInfo> TileSetInfo;
        //

        // /Entity
        public readonly KeyValueInfo<int, ActionContainerListInfo> ActionContainerListInfo;
        public readonly KeyValueInfo<int, ActionGroupInfo> ActionGroupInfo;
        public readonly KeyValueInfo<int, AreaInfo> AreaInfo;
        public readonly KeyValueInfo<int, BarrierInfo> BarrierInfo;
        public readonly KeyValueInfo<int, BehaviorTreeNodeInfo> BehaviorTreeNodeInfo;
        public readonly KeyValueInfo<int, BuffInfo> BuffInfo;

        public readonly KeyValueInfo<int, CharacterInfo> CharacterInfo;
        public readonly KeyValueInfo<int, CharacterStatInfo> CharacterStatInfo;
        public readonly KeyValueInfo<int, CharacterOvercomeInfo> CharacterOvercomeInfo;
        public readonly KeyValueInfo<KeyType<int, int>, CharacterOvercomeStatInfo> CharacterOvercomeStatInfo;
        public readonly KeyValueInfo<KeyType<Rarity, int>, CharacterLevelUpInfo> CharacterLevelUpInfo;
        public readonly KeyValueInfo<KeyType<Rarity, ElementalType, int>, CharacterOvercomeUpInfo> CharacterOvercomeUpInfo;
        public readonly KeyValueInfo<int, CharacterRankUpInfo> CharacterRankUpInfo;
        public readonly KeyValueInfo<int, CharacterRankAbilityGroupInfo> CharacterRankAbilityGroupInfo;
        public readonly KeyValueInfo<int, DummyInfo> DummyInfo;
        public readonly KeyValueInfo<int, EffectInfo> EffectInfo;
        public readonly KeyValueInfo<int, MonsterTableValue> MonsterTableValues;
        public readonly KeyValueInfo<int, BossMonsterTableValue> BossMonsterTableValues;
        public readonly KeyValueInfo<int, GeneratorInfo> GeneratorInfo;
        public readonly KeyValueInfo<int, MonsterInfo> MonsterInfo;
        public readonly KeyValueInfo<int, BossMonsterInfo> BossMonsterInfo;
        public readonly KeyValueInfo<int, CreatureInfo> CreatureInfo;
        public readonly KeyValueInfo<int, ActionDirectionListInfo> ActionDirectionInfo;
        public readonly KeyValueInfo<int, ProjectileInfo> ProjectileInfo;
        public readonly KeyValueInfo<int, PreviewInfo> PreviewInfo;
        public readonly KeyValueInfo<int, PropInfo> PropInfo;
        public readonly KeyValueInfo<int, IndependentDirectionInfo> IndependentDirectionInfo;
        public readonly KeyValueInfo<int, TagInfo> TagInfo;

        public readonly KeyValueInfo<int, RuneInfo> RuneInfo;
        public readonly KeyValueInfo<int, RuneGroupInfo> RuneGroupInfo;
        public readonly KeyValueInfo<int, RuneGroupUnlockInfo> RuneGroupUnlockInfo;
        //

        // Item
        public readonly KeyValueInfo<int, WeaponInfo> WeaponInfo;
        public readonly KeyValueInfo<int, WeaponOvercomeInfo> WeaponOvercomeInfo;
        public readonly KeyValueInfo<KeyType<int, int>, WeaponOvercomeStatInfo> WeaponOvercomeStatInfo;
        public readonly KeyValueInfo<KeyType<Rarity, int>, WeaponLevelUpInfo> WeaponLevelUpInfo;
        public readonly KeyValueInfo<KeyType<Rarity, WeaponType, int>, WeaponOvercomeUpInfo> WeaponOvercomeUpInfo;
        public readonly KeyValueInfo<Rarity, WeaponDecompInfo> WeaponDecompInfo;
        public readonly KeyValueInfo<int, WeaponReforgeInfo> WeaponReforgeInfo;
        public readonly KeyValueInfo<KeyType<Rarity, int>, WeaponReforgeUpInfo> WeaponReforgeUpInfo;

        public readonly KeyValueInfo<int, ArmorInfo> ArmorInfo;
        public readonly KeyValueInfo<Rarity, ArmorRarityInfo> ArmorRarityInfo;
        public readonly KeyValueInfo<KeyType<Rarity, EquipmentSlotType, int>, ArmorMainStatInfo> ArmorMainStatInfo;
        public readonly KeyValueInfo<KeyType<Rarity, int>, ArmorSubStatInfo> ArmorSubStatInfo;
        public readonly KeyValueInfo<KeyType<Rarity, int>, ArmorLevelUpInfo> ArmorLevelUpInfo;
        public readonly KeyValueInfo<Rarity, ArmorDecompInfo> ArmorDecompInfo;

        public readonly KeyValueInfo<int, ArmorSetInfo> ArmorSetInfo;

        public readonly KeyValueInfo<int, CharacterSoulInfo> CharacterSoulInfo;
        public readonly KeyValueInfo<int, TicketInfo> TicketInfo;

        public readonly KeyValueInfo<int, MaterialInfo> MaterialInfo;
        public readonly KeyValueInfo<KeyType<int, int>, MaterialExchangeInfo> MaterialExchangeInfo;


        public readonly KeyValueInfo<int, BoxInfo> BoxInfo;
        public readonly KeyValueInfo<int, MoneyBoxInfo> MoneyBoxInfo;
        public readonly KeyValueInfo<int, EventCoinInfo> EventCoinInfo;

        public readonly KeyValueInfo<KeyType<int, int>, ConsumeShortcutInfo> ConsumeShortcutInfo;

        // Reward        
        public readonly KeyValueInfo<int, FixedRewardInfo> FixedRewardInfo;
        public readonly KeyValueInfo<int, RandomRewardInfo> RandomRewardInfo;
        public readonly KeyValueInfo<KeyType<int, int>, RandomItemSetInfo> RandomItemSetInfo;

        public readonly KeyValueInfo<int, SelectableRewardInfo> SelectableRewardInfo;
        public readonly KeyValueInfo<KeyType<int, int>, SelectableItemSetInfo> SelectableItemSetInfo;




        public readonly KeyValueInfo<int, GachaInfo> GachaInfo;
        public readonly KeyValueInfo<KeyType<int, int>, GachaSetInfo> GachaSetInfo;
        public readonly KeyValueInfo<KeyType<int, int>, GachaFakeInfo> GachaFakeInfo;

        public readonly KeyValueInfo<KeyType<ItemType, Rarity>, MileageInfo> MileageInfo;

        public readonly KeyValueInfo<int, AdsInfo> AdsInfo;

        // Event
        public readonly KeyValueInfo<int, EventInfo> EventInfo;

        // Attendance
        public readonly KeyValueInfo<int, AttendanceInfo> AttendanceInfo;
        public readonly KeyValueInfo<KeyType<int, int>, AttendanceRewardSetInfo> AttendanceRewardSetInfo;

        public readonly KeyValueInfo<int, DailyPointEventInfo> DailyPointEventInfo;
        public readonly KeyValueInfo<KeyType<int, int>, ExchangeEventInfo> ExchangeEventInfo;
        public readonly KeyValueInfo<int, StackPointEventInfo> StackPointEventInfo;
        public readonly KeyValueInfo<KeyType<int, int>, StackPointEventInfoV2> StackPointEventInfoV2;
        public readonly KeyValueInfo<int, DailyQuestEventInfo> DailyQuestEventInfo;
        public readonly KeyValueInfo<int, AchievementEventInfo> AchievementEventInfo;
        public readonly KeyValueInfo<KeyType<int, int>, EpisodeRewardEventInfo> EpisodeRewardEventInfo;
        public readonly KeyValueInfo<int, GachaEventInfo> GachaEventInfo;
        public readonly KeyValueInfo<int, EpisodeClearEventInfo> EpisodeClearEventInfo;
        public readonly KeyValueInfo<int, LotteryBoardEventInfo> LotteryBoardEventInfo;
        public readonly KeyValueInfo<KeyType<int, int>, LotteryBoardRewardInfo> LotteryBoardRewardInfo;
        public readonly KeyValueInfo<KeyType<int, int>, LotteryBoardPresetInfo> LotteryBoardPresetInfo;
        public readonly KeyValueInfo<int, WorldMissionEventInfo> WorldMissionEventInfo;
        public readonly KeyValueInfo<KeyType<int, int>, WorldMissionTierInfo> WorldMissionTierInfo;
        public readonly KeyValueInfo<KeyType<int, int>, WorldMissionCoopRewardInfo> WorldMissionCoopRewardInfo;
        public readonly KeyValueInfo<KeyType<int, int>, WorldMissionScoreRewardInfo> WorldMissionScoreRewardInfo;


        public readonly KeyValueInfo<int, ProductInfo> ProductInfo;
        public readonly KeyValueInfo<int, TriggerPackageInfo> TriggerPackageInfo;
        public readonly KeyValueInfo<int, LobbyProductExposureInfo> LobbyProductExposureInfo;
        public readonly KeyValueInfo<Menu, LobbyProductExposureExposeInfo> LobbyProductExposureExposeInfo;

        public readonly KeyValueInfo<MenuButtonType, MenuButtonInfo> MenuButtonInfo;
        // SeasonPass
        public readonly KeyValueInfo<int, SeasonPassInfo> SeasonPassInfo;
        public readonly KeyValueInfo<KeyType<int, int>, SeasonPassRewardInfo> SeasonPassRewardInfo; // no need to access

        // 월정액
        public readonly KeyValueInfo<int, DailyRewardInfo> DailyRewardInfo;

        // Collection
        public readonly KeyValueInfo<int, CollectionInfo> CollectionInfo;
        public readonly KeyValueInfo<KeyType<int, int>, CollectionStatBonusInfo> CollectionStatBonusInfo;

        public readonly KeyValueInfo<int, BattleTutorialInfo> BattleTutorialInfo;
        public readonly KeyValueInfo<int, LobbyTutorialInfo> LobbyTutorialInfo;
        public readonly KeyValueInfo<int, EpisodeGroupMissionInfo> EpisodeGroupMissionInfo;
        public readonly KeyValueInfo<int, QuestInfo> QuestInfo;
        public readonly KeyValueInfo<int, DailyQuestInfo> DailyQuestInfo;
        public readonly KeyValueInfo<int, WeeklyQuestInfo> WeeklyQuestInfo;
        public readonly KeyValueInfo<int, DailyPointRewardQuestInfo> DailyPointRewardQuestInfo;
        public readonly KeyValueInfo<int, WeeklyPointRewardQuestInfo> WeeklyPointRewardQuestInfo;
        public readonly KeyValueInfo<int, AchievementPointRewardQuestInfo> AchievementPointRewardQuestInfo;
        public readonly KeyValueInfo<int, AchievementInfo> AchievementInfo;
        public readonly KeyValueInfo<int, CharacterStoryMissionInfo> CharacterStoryMissionInfo;
        public readonly KeyValueInfo<int, ConquestSeasonInfo> ConquestSeasonInfo;
        public readonly KeyValueInfo<KeyType<int, int>, ConquestTierInfo> ConquestTierInfo;
        public readonly KeyValueInfo<int, EpisodeRuleInfo> EpisodeRuleInfo;
        public readonly KeyValueInfo<int, ConquestScoreInfo> ConquestScoreInfo;

        public readonly KeyValueInfo<int, TowerSeasonInfo> TowerSeasonInfo;
        public readonly KeyValueInfo<KeyType<int, int>, TowerInfo> TowerInfo;

        public readonly SpecificInfo<GameLogicInfo> GameLogicInfo;
        public readonly SpecificInfo<ServiceLogicInfo> ServiceLogicInfo;
        public readonly SpecificInfo<UISoundInfo> UISoundInfo;
        public readonly SpecificInfo<MainQuestSequenceInfo> MainQuestSequenceInfo;
        public readonly SpecificInfo<TimeResourceRewardInfo> TimeResourceRewardInfo;
        public readonly SpecificInfo<TimeResourceAccountLevelInfo> TimeResourceAccountLevelInfo;
        


        public readonly GeoLite2Info GeoLite2Info = new GeoLite2Info();
        public readonly BadWordInfo BadWordInfo = new BadWordInfo();

        private HashSet<StaticInfoType> _initTypes = new HashSet<StaticInfoType>();
        public bool InitProcessing => _initProcessing;
        private bool _initProcessing = false;

        #region Post Created
        public KeyValueInfo<int, ActorInfo> ActorInfos => _actorInfos;
        private readonly KeyValueInfo<int, ActorInfo> _actorInfos = new KeyValueInfo<int, ActorInfo>(string.Empty, StaticInfoFormatType.Undefined, StaticInfoType.Common);

        public KeyValueInfo<int, ActionInfo> ActionInfos => _actionInfos;
        private readonly KeyValueInfo<int, ActionInfo> _actionInfos = new KeyValueInfo<int, ActionInfo>(string.Empty, StaticInfoFormatType.Undefined, StaticInfoType.Common);

        public KeyValueInfo<int, ActionContainerInfo> ActionContainerInfos => _actionContainers;
        private readonly KeyValueInfo<int, ActionContainerInfo> _actionContainers = new KeyValueInfo<int, ActionContainerInfo>(string.Empty, StaticInfoFormatType.Undefined, StaticInfoType.Common);

        //public KeyValueInfo<int, EventActionInfo> EventActionInfos => _eventActionInfos;
        //public readonly KeyValueInfo<int, EventActionInfo> _eventActionInfos = new KeyValueInfo<int, EventActionInfo>(string.Empty, StaticInfoFormatType.Undefined, StaticInfoType.Common);

        public IReadOnlyDictionary<int /*characterId*/, int /*soulId*/> CharacterSoulIds => _characterSoulIds;
        public Dictionary<int /*characterId*/, int /*soulId*/> _characterSoulIds;

        public IReadOnlyCollection<int /*ItemId*/> ItemKeys => _itemKeys;
        private readonly HashSet<int /*ItemId*/> _itemKeys = new HashSet<int /*ItemId*/>();

        public IReadOnlyDictionary<WeaponType, List<WeaponInfo>> WeaponInfosByType => _weaponInfosByType;
        public Dictionary<WeaponType, List<WeaponInfo>> _weaponInfosByType;

        public IReadOnlyDictionary<int, EquipmentInfo> EquipmentInfos => _equipmentInfos;
        public Dictionary<int, EquipmentInfo> _equipmentInfos;

        public IReadOnlyDictionary<int, ConsumeInfo> ConsumeInfos => _consumeInfos;
        public Dictionary<int, ConsumeInfo> _consumeInfos;

        public IReadOnlyDictionary<int/*setId*/, List<RandomItemInfo>> RandomItemSets => _randomItemSets;
        private readonly Dictionary<int/*setId*/, List<RandomItemInfo>> _randomItemSets = new Dictionary<int, List<RandomItemInfo>>();

        public IReadOnlyDictionary<int/*setId*/, List<ItemModel>> SelectableItemSets => _selectableItemSets;
        private readonly Dictionary<int/*setId*/, List<ItemModel>> _selectableItemSets = new Dictionary<int, List<ItemModel>>();

        public IReadOnlyDictionary<EpisodeGroupType, List<EpisodeGroupInfo>> SortedEpisodeGroupsByType => _sortedEpisodeGroupsByType;
        private Dictionary<EpisodeGroupType, List<EpisodeGroupInfo>> _sortedEpisodeGroupsByType;

        public IReadOnlyDictionary<int, List<EpisodeInfo>> SortedEpisodesByGroup => _sortedEpisodesByGroup;
        private Dictionary<int, List<EpisodeInfo>> _sortedEpisodesByGroup = new Dictionary<int, List<EpisodeInfo>>();

        public IReadOnlyDictionary<EpisodeGroupType, List<EpisodeInfo>> SortedEpisodesByGroupType => _sortedEpisodesByGroupType;
        private Dictionary<EpisodeGroupType, List<EpisodeInfo>> _sortedEpisodesByGroupType = new Dictionary<EpisodeGroupType, List<EpisodeInfo>>();

        public IReadOnlyDictionary<int, MissionInfo> MissionInfos => _missionInfos;
        private readonly Dictionary<int, MissionInfo> _missionInfos = new Dictionary<int, MissionInfo>();

        public IReadOnlyDictionary<int, QuestInfo> AllQuestInfos => _allQuestInfos;
        private readonly Dictionary<int, QuestInfo> _allQuestInfos = new Dictionary<int, QuestInfo>();

        public IReadOnlyDictionary<int /*episodeGroupId*/, List<MissionInfo>> EpisodeGroupMissionInfos => _episodeGroupMissionInfos;
        private readonly Dictionary<int, List<MissionInfo>> _episodeGroupMissionInfos = new Dictionary<int, List<MissionInfo>>();

        public IReadOnlyDictionary<(int buffId, int otherInfoId, int level), Core.Buff.BuffTemplate> BuffTemplates => _buffTemplates;
        private readonly Dictionary<(int buffId, int otherInfoId, int level), Core.Buff.BuffTemplate> _buffTemplates = new Dictionary<(int, int, int), Core.Buff.BuffTemplate>();

        public IReadOnlyDictionary<int /*gachaSetId*/, List<GachaSetInfo>> GachaSetInfos => _gachaSetInfos;
        private Dictionary<int, List<GachaSetInfo>> _gachaSetInfos = new Dictionary<int, List<GachaSetInfo>>();

        public IReadOnlyDictionary<int /*gachaInfoId*/, Dictionary<(int ItemId, int Count), double /*probability*/>> GachaItemProbs => _gachaItemProbs;
        private Dictionary<int, Dictionary<(int, int), double>> _gachaItemProbs = new Dictionary<int, Dictionary<(int, int), double>>();

        public IReadOnlyDictionary<Rarity, Dictionary<FakeType, double>> GachaFakeProbs => _gachaFakeProbs;
        private Dictionary<Rarity, Dictionary<FakeType, double>> _gachaFakeProbs = new Dictionary<Rarity, Dictionary<FakeType, double>>();

        public IReadOnlyDictionary<int, List<ConsumeShortcutInfo>> ConsumeShortcutInfos => _consumeShortcutInfos;
        private Dictionary<int, List<ConsumeShortcutInfo>> _consumeShortcutInfos = new Dictionary<int, List<ConsumeShortcutInfo>>();

        public IReadOnlyDictionary<int, List<MaterialExchangeInfo>> MaterialExchangeInfos => _materialExchangeInfos;
        private Dictionary<int, List<MaterialExchangeInfo>> _materialExchangeInfos = new Dictionary<int, List<MaterialExchangeInfo>>();

        public IReadOnlyDictionary<int, List<ExchangeEventInfo>> ExchangeEventInfos => _exchangeEventInfos;
        private Dictionary<int, List<ExchangeEventInfo>> _exchangeEventInfos = new Dictionary<int, List<ExchangeEventInfo>>();
        public IReadOnlyDictionary<int, List<StackPointEventInfo>> StackPointEventInfos => _stackPointEventInfos;
        private Dictionary<int, List<StackPointEventInfo>> _stackPointEventInfos = new Dictionary<int, List<StackPointEventInfo>>();

        public IReadOnlyDictionary<int, List<StackPointEventInfoV2>> StackPointEventInfosV2 => _stackPointEventInfosV2;
        private Dictionary<int, List<StackPointEventInfoV2>> _stackPointEventInfosV2 = new Dictionary<int, List<StackPointEventInfoV2>>();

        public IReadOnlyDictionary<int, List<EpisodeClearEventInfo>> EpisodeClearEventInfos => _episodeClearEventInfos;
        private Dictionary<int, List<EpisodeClearEventInfo>> _episodeClearEventInfos = new Dictionary<int, List<EpisodeClearEventInfo>>();

        public IReadOnlyDictionary<int, Dictionary<int, LotteryBoardRewardInfo>> LotteryBoardRewardInfos => _lotteryBoardRewardInfos;
        private Dictionary<int, Dictionary<int, LotteryBoardRewardInfo>> _lotteryBoardRewardInfos = new Dictionary<int, Dictionary<int, LotteryBoardRewardInfo>>();
        public IReadOnlyDictionary<int, List<LotteryBoardPresetInfo>> LotteryBoardPresetInfos => _lotteryBoardPresetInfos;
        private Dictionary<int, List<LotteryBoardPresetInfo>> _lotteryBoardPresetInfos = new Dictionary<int, List<LotteryBoardPresetInfo>>();
        
        public IReadOnlyDictionary<MaterialType, List<MaterialInfo>> DesSortedValueMaterials => _desSortedValueMaterials;
        private Dictionary<MaterialType, List<MaterialInfo>> _desSortedValueMaterials = new Dictionary<MaterialType, List<MaterialInfo>>();

        public IReadOnlyList<int> ConquestSeasonIds => _conquestSeasonIds;

        private List<int> _dailyEntryLimitEpisodeIds = new List<int>();
        public IReadOnlyList<int> DailyEntryLimitEpisodeIds => _dailyEntryLimitEpisodeIds;//ResetDaily할때 지워야함

        public Dictionary<int, string> EventActionInfoIdChecker = new Dictionary<int, string>();
        public List<EventActionInfo> PostGenerateEventActionId = new List<EventActionInfo>();
        public void AddEntryLimitEpisodeId(int episodeId)
        {
            _dailyEntryLimitEpisodeIds.Add(episodeId);
        }
        public int GetConquestSeaonIndex(int seasonId)
        {
            return _conquestSeasonIds.FindIndex(e => e == seasonId);
        }
        private List<int> _conquestSeasonIds = new List<int>();


        public IReadOnlyDictionary<int, List<DailyQuestEventInfo>> DailyQuestEventInfos => _dailyQuestEventInfos;
        private Dictionary<int, List<DailyQuestEventInfo>> _dailyQuestEventInfos = new Dictionary<int, List<DailyQuestEventInfo>>();
        public IReadOnlyDictionary<int, List<AchievementEventInfo>> AchievementEventInfos => _achievementEventInfos;
        private Dictionary<int, List<AchievementEventInfo>> _achievementEventInfos = new Dictionary<int, List<AchievementEventInfo>>();

        public IReadOnlyDictionary<int, QuestEventInfo> AllQuestEventInfos => _allQuestEventInfos;
        private readonly Dictionary<int, QuestEventInfo> _allQuestEventInfos = new Dictionary<int, QuestEventInfo>();

        public IReadOnlyDictionary<int, Dictionary<int, EpisodeRewardEventInfo>> EpisodeRewardEventInfos => _episodeRewardEventInfos;
        private Dictionary<int, Dictionary<int, EpisodeRewardEventInfo>> _episodeRewardEventInfos = new Dictionary<int, Dictionary<int, EpisodeRewardEventInfo>>();

        public IReadOnlyDictionary<int, Dictionary<int, ConquestTierInfo>> ConquestTierInfos => _conquestTierInfos;
        private Dictionary<int /*tierSeasonId*/, Dictionary<int /*tierId*/, ConquestTierInfo>> _conquestTierInfos = new Dictionary<int, Dictionary<int, ConquestTierInfo>>();

        public IReadOnlyDictionary<int, Dictionary<int, TowerInfo>> TowerInfos => _towerInfos;
        private Dictionary<int, Dictionary<int, TowerInfo>> _towerInfos = new Dictionary<int, Dictionary<int, TowerInfo>>();

        public IReadOnlyDictionary<int, List<WorldMissionScoreRewardInfo>> WorldMissionScoreRewardInfos => _worldMissionScoreRewardInfos;
        private Dictionary<int, List<WorldMissionScoreRewardInfo>> _worldMissionScoreRewardInfos = new Dictionary<int, List<WorldMissionScoreRewardInfo>>();

        public IReadOnlyDictionary<int, List<WorldMissionCoopRewardInfo>> WorldMissionCoopRewardInfos => _worldMissionCoopRewardInfos;
        private Dictionary<int, List<WorldMissionCoopRewardInfo>> _worldMissionCoopRewardInfos = new Dictionary<int, List<WorldMissionCoopRewardInfo>>();

        public IReadOnlyDictionary<int, List<WorldMissionTierInfo>> WorldMissionTierInfos => _worldMissionTierInfos;
        private Dictionary<int /*eventId*/, List<WorldMissionTierInfo>> _worldMissionTierInfos = new Dictionary<int, List<WorldMissionTierInfo>>();
        #endregion

        private static bool _jsonSettingDone = false;
        public static void DoJsonSetting()
        {
            if (_jsonSettingDone == true)
            {
                return;
            }

            _jsonSettingDone = true;
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<ActionInfo>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicListTypeConverter<ActionInfo>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<ActorInfo>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<EventTriggerInfo>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<EventActionInfo>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<BuffInfo>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<GeneratorInfo>());

            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<Node>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<Dash.Core.Option.IOption>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<Dash.StaticData.Entity.AbstractDirection>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicTypeReadConverter<SimulationScript>());
            JsonGlobalSettings.ReaderSettings.Converters.Add(new Common.Json.DynamicListTypeConverter<ProjectileAttribute>());
            JsonGlobalSettings.ReaderSettings.NullValueHandling = NullValueHandling.Ignore;

            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<ActionInfo>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicListTypeConverter<ActionInfo>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<ActorInfo>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<EventTriggerInfo>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<EventActionInfo>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<BuffInfo>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<GeneratorInfo>());

            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<Node>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<Dash.Core.Option.IOption>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<Dash.StaticData.Entity.AbstractDirection>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicTypeWriteConverter<SimulationScript>());
            JsonGlobalSettings.WriterSettings.Converters.Add(new Common.Json.DynamicListTypeConverter<ProjectileAttribute>());

            JsonGlobalSettings.WriterSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            JsonGlobalSettings.WriterSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        public StaticInfo()
        {
            //////////////////////////// Settings ////////////////////////////

            DoJsonSetting();

            ////////////////////////// KeyValueInfo //////////////////////////

            Register(ref AccountLevelUpInfo, "/AccountLevelUpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CoreSimulationInfo, "/CoreSimulationInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref HelpInfo, "/HelpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref EffectInfo, "/EffectInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref SkillInfo, "/SkillInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref IndependentDirectionInfo, "/IndependentDirectionInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ActionDirectionInfo, "/ActionDirectionInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref PreviewInfo, "/PreviewInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref RuleInfo, "/RuleInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref RewardChestInfo, "/RewardChestInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            // /Direction
            Register(ref PerlinNoiseShakeInfo, "/Direction/PerlinNoiseShakeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CurveShakeInfo, "/Direction/CurveShakeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ScaleAnimationInfo, "/Direction/ScaleAnimationInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            //

            // Episode
            Register(ref EpisodeGroupInfo, "/Episode/EpisodeGroupInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            //Register(ref EpisodeGroupAdditionalInfo, "/Episode/EpisodeGroupAdditionalInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeInfo, "/Episode/EpisodeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeAdditionalInfo, "/Episode/EpisodeAdditionalInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeRewardInfo, "/Episode/EpisodeRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeMissionInfo, "/Episode/EpisodeMissionInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeMonsterStatusDeltaInfo, "/Episode/EpisodeMonsterStatusDeltaInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            //

            // Stage
            Register(ref StageInfo, "/Stage/StageInfo", StaticInfoFormatType.MPackOrJson, StaticInfoType.Common);
            Register(ref TileSetInfo, "/Stage/TileSetInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            //

            // Entity
            Register(ref ActionContainerListInfo, "/Entity/ActionContainerListInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ActionGroupInfo, "/Entity/ActionGroupInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref AreaInfo, "/Entity/AreaInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref BarrierInfo, "/Entity/BarrierInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref BuffInfo, "/Entity/BuffInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref BehaviorTreeNodeInfo, "/Entity/BehaviorTreeNodeInfo", StaticInfoFormatType.MPackOrJson, StaticInfoType.Common);
            Register(ref CharacterInfo, "/Entity/CharacterInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CharacterStatInfo, "/Entity/CharacterStatInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CharacterOvercomeInfo, "/Entity/CharacterOvercomeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CharacterOvercomeStatInfo, "/Entity/CharacterOvercomeStatInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CharacterLevelUpInfo, "/Entity/CharacterLevelUpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CharacterOvercomeUpInfo, "/Entity/CharacterOvercomeUpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CharacterRankUpInfo, "/Entity/CharacterRankUpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CharacterRankAbilityGroupInfo, "/Entity/CharacterRankAbilityGroupInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref DummyInfo, "/Entity/DummyInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref GeneratorInfo, "/Entity/GeneratorInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref MonsterInfo, "/Entity/MonsterInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref BossMonsterInfo, "/Entity/BossMonsterInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CreatureInfo, "/Entity/CreatureInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref MonsterTableValues, "/Entity/MonsterTableValue", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref BossMonsterTableValues, "/Entity/BossMonsterTableValue", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ProjectileInfo, "/Entity/ProjectileInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref PropInfo, "/Entity/PropInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref TagInfo, "/Entity/TagInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref RuneInfo, "/Entity/RuneInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref RuneGroupInfo, "/Entity/RuneGroupInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref RuneGroupUnlockInfo, "/Entity/RuneGroupUnlockInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            // /Entity

            // Item
            Register(ref WeaponInfo, "/Item/WeaponInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeaponOvercomeInfo, "/Item/WeaponOvercomeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeaponOvercomeStatInfo, "/Item/WeaponOvercomeStatInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeaponLevelUpInfo, "/Item/WeaponLevelUpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeaponOvercomeUpInfo, "/Item/WeaponOvercomeUpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeaponDecompInfo, "/Item/WeaponDecompInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeaponReforgeInfo, "/Item/WeaponReforgeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeaponReforgeUpInfo, "/Item/WeaponReforgeUpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ArmorInfo, "/Item/ArmorInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ArmorRarityInfo, "/Item/ArmorRarityInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ArmorMainStatInfo, "/Item/ArmorMainStatInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ArmorSubStatInfo, "/Item/ArmorSubStatInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ArmorLevelUpInfo, "/Item/ArmorLevelUpInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ArmorDecompInfo, "/Item/ArmorDecompInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref ArmorSetInfo, "/Item/ArmorSetInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref CharacterSoulInfo, "/Item/CharacterSoulInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref TicketInfo, "/Item/TicketInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref MaterialInfo, "/Item/MaterialInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref MaterialExchangeInfo, "/Item/MaterialExchangeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref BoxInfo, "/Item/BoxInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref MoneyBoxInfo, "/Item/MoneyBoxInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EventCoinInfo, "/Item/EventCoinInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref ConsumeShortcutInfo, "/Item/ConsumeShortcutInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            // Reward            
            Register(ref FixedRewardInfo, "/Reward/FixedRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref RandomRewardInfo, "/Reward/RandomRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref RandomItemSetInfo, "/Reward/RandomItemSetInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref SelectableRewardInfo, "/Reward/SelectableRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref SelectableItemSetInfo, "/Reward/SelectableItemSetInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref GachaInfo, "/Reward/GachaInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref GachaSetInfo, "/Reward/GachaSetInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref GachaFakeInfo, "/Reward/GachaFakeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref MileageInfo, "/Reward/MileageInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref AdsInfo, "/Reward/AdsInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            // Event
            Register(ref EventInfo, "/Event/EventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            // Attendance
            Register(ref AttendanceInfo, "/Event/AttendanceInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref AttendanceRewardSetInfo, "/Event/AttendanceRewardSetInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref DailyPointEventInfo, "/Event/DailyPointEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ExchangeEventInfo, "/Event/ExchangeEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref StackPointEventInfo, "/Event/StackPointEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref StackPointEventInfoV2, "/Event/StackPointEventInfoV2", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref DailyQuestEventInfo, "/Event/DailyQuestEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref AchievementEventInfo, "/Event/AchievementEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeRewardEventInfo, "/Event/EpisodeRewardEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeClearEventInfo, "/Event/EpisodeClearEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref GachaEventInfo, "/Event/GachaEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref LotteryBoardEventInfo, "/Event/LotteryBoardEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref LotteryBoardRewardInfo, "/Event/LotteryBoardRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref LotteryBoardPresetInfo, "/Event/LotteryBoardPresetInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WorldMissionEventInfo, "/Event/WorldMissionEventInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WorldMissionTierInfo, "/Event/WorldMissionTierInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WorldMissionScoreRewardInfo, "/Event/WorldMissionScoreRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WorldMissionCoopRewardInfo, "/Event/WorldMissionCoopRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            // Shop
            Register(ref ProductInfo, "/Shop/ProductInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref TriggerPackageInfo, "/Shop/TriggerPackageInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref LobbyProductExposureInfo, "/Shop/LobbyProductExposureInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref LobbyProductExposureExposeInfo, "/Shop/LobbyProductExposureExposeInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            // MenuButton
            Register(ref MenuButtonInfo, "/Menu/MenuButtonInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            // SeasonPass
            Register(ref SeasonPassInfo, "/Reward/SeasonPassInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref SeasonPassRewardInfo, "/Reward/SeasonPassRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            // DailyReward
            Register(ref DailyRewardInfo, "/Reward/DailyRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            // Collection
            Register(ref CollectionInfo, "/Collection/CollectionInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CollectionStatBonusInfo, "/Collection/CollectionStatBonusInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            // Mission
            Register(ref BattleTutorialInfo, "/Tutorial/BattleTutorialInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref LobbyTutorialInfo, "/Tutorial/LobbyTutorialInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeGroupMissionInfo, "/Episode/EpisodeGroupMissionInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref QuestInfo, "/Quest/QuestInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref DailyQuestInfo, "/Quest/DailyQuestInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeeklyQuestInfo, "/Quest/WeeklyQuestInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref DailyPointRewardQuestInfo, "/Quest/DailyPointRewardQuestInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref WeeklyPointRewardQuestInfo, "/Quest/WeeklyPointRewardQuestInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref AchievementPointRewardQuestInfo, "/Quest/AchievementPointRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref AchievementInfo, "/Quest/AchievementInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref CharacterStoryMissionInfo, "/Quest/CharacterStoryMissionInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref ConquestSeasonInfo, "/Conquest/ConquestSeasonInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ConquestTierInfo, "/Conquest/ConquestTierInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ConquestScoreInfo, "/Conquest/ConquestScoreInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            Register(ref TowerSeasonInfo, "/Tower/TowerSeasonInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref TowerInfo, "/Tower/TowerInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref EpisodeRuleInfo, "/EpisodeRuleInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            ////////////////////////// SpecificInfo //////////////////////////

            Register(ref GameLogicInfo, "/GameLogicInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref ServiceLogicInfo, "/ServiceLogicInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref UISoundInfo, "/UISoundInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref MainQuestSequenceInfo, "/Quest/MainQuestSequenceInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref TimeResourceRewardInfo, "/Reward/TimeResourceRewardInfo", StaticInfoFormatType.Json, StaticInfoType.Common);
            Register(ref TimeResourceAccountLevelInfo, "/Reward/TimeResourceAccountLevelInfo", StaticInfoFormatType.Json, StaticInfoType.Common);

            //////////////////////////////////////////////////////////////////
            keyValueInfosByType.Add(typeof(ActorInfo), _actorInfos);

            CharacterInfo.OnSaveCallback = ActorPostInit;
            MonsterInfo.OnSaveCallback = ActorPostInit;
            BossMonsterInfo.OnSaveCallback = ActorPostInit;
            CreatureInfo.OnSaveCallback = ActorPostInit;

            ActionContainerListInfo.OnSaveCallback = ActionPostInit;
            ActionGroupInfo.OnSaveCallback = ActionPostInit;
            RuneInfo.OnSaveCallback = ActionPostInit;
        }

        public void ActorPostInit()
        {
            _actorInfos.GetInfos().Clear();
            foreach (CharacterInfo characterInfo in CharacterInfo.GetList().OrderBy(i => i.Id))
            {
                _actorInfos.GetInfos().Add(characterInfo.Id, characterInfo);
                if (WeaponInfo.TryGet(characterInfo.DefaultWeaponId, out var weaponInfo) == false)
                {
                    characterInfo.DefaultWeaponId = WeaponInfo.GetList().FirstOrDefault(e => e.WeaponType == characterInfo.WeaponType)?.Id ?? 0;
                }
            }

            foreach (MonsterTableValue tableValue in MonsterTableValues.GetList().OrderBy(i => i.Id))
            {
                if (MonsterInfo.Exist(tableValue.Id) == false)
                    Common.Log.Logger.Instance.Error($"{nameof(MonsterInfo)} TableValue doesn't exist! {tableValue.Id}");
                else
                    MonsterInfo[tableValue.Id].Apply(tableValue);
            }

            foreach (MonsterInfo monsterInfo in MonsterInfo.GetList().OrderBy(i => i.Id))
            {
                _actorInfos.GetInfos().Add(monsterInfo.Id, monsterInfo);
            }

            foreach (MonsterTableValue tableValue in BossMonsterTableValues.GetList().OrderBy(i => i.Id))
            {
                if (BossMonsterInfo.Exist(tableValue.Id) == true)
                    BossMonsterInfo[tableValue.Id].Apply(tableValue);
                else
                    Common.Log.Logger.Instance.Error($"{nameof(BossMonsterInfo)} TableValue doesn't exist! {tableValue.Id}");
            }

            foreach (BossMonsterInfo bossMonsterInfo in BossMonsterInfo.GetList().OrderBy(i => i.Id))
            {
                _actorInfos.GetInfos().Add(bossMonsterInfo.Id, bossMonsterInfo);
            }

            foreach (CreatureInfo creatureInfo in CreatureInfo.GetList().OrderBy(i => i.Id))
            {
                _actorInfos.GetInfos().Add(creatureInfo.Id, creatureInfo);
            }
        }
        public void ActionPostInit()
        {
            _actionInfos.GetInfos().Clear();
            _actionContainers.GetInfos().Clear();
            int actionInfoId = 0;
            int actionContainerId = 0;
            foreach (ActionContainerListInfo actionConainterListInfo in ActionContainerListInfo.GetList())
            {
                foreach (ActionContainerInfo actionContainerInfo in actionConainterListInfo.ActionContainers)
                {
                    actionContainerInfo.Id = ++actionContainerId;
                    _actionContainers.GetInfos().Add(actionContainerInfo.Id, actionContainerInfo);
                    foreach (var actionInfo in actionContainerInfo.GetActionInfos)
                    {
                        actionInfo.Id = ++actionInfoId;
                        _actionInfos.GetInfos().Add(actionInfo.Id, actionInfo);
                    }
                }
            }

            foreach (ActionGroupInfo actionGroupInfo in ActionGroupInfo.GetList().OrderBy(i => i.Id))
            {
                if (actionGroupInfo.Action == null)
                {
                    continue;
                }
                actionGroupInfo.Action.Id = ++actionContainerId;
                _actionContainers.GetInfos().Add(actionGroupInfo.Action.Id, actionGroupInfo.Action);
                var actionInfos = actionGroupInfo.Action.GetActionInfos ?? null;
                if (actionInfos != null)
                {
                    foreach (ActionInfo actionInfo in actionInfos)
                    {
                        actionInfo.Id = ++actionInfoId;
                        _actionInfos.GetInfos().Add(actionInfo.Id, actionInfo);
                        if (actionGroupInfo.Type == ActionGroupType.Base && actionInfo is IVerifiable verifiable)
                        {
                            string actionVerifyLog;
                            if (verifiable.Verify(out actionVerifyLog) == false)
                            {
                                Common.Log.Logger.Instance.Error(actionVerifyLog);
                            }
                        }
                    }
                }
            }
            foreach (RuneInfo runeInfo in RuneInfo.GetList().OrderBy(i => i.Id))
            {
                var actionInfos = runeInfo.ReplaceAction.GetActionInfos ?? null;
                if (actionInfos != null && actionInfos.Count > 0)
                {
                    runeInfo.ReplaceAction.Id = ++actionContainerId;
                    _actionContainers.GetInfos().Add(runeInfo.ReplaceAction.Id, runeInfo.ReplaceAction);
                    foreach (ActionInfo actionInfo in actionInfos)
                    {
                        actionInfo.Id = ++actionInfoId;
                        _actionInfos.GetInfos().Add(actionInfo.Id, actionInfo);
                        if (runeInfo.ActionGroupType == ActionGroupType.Base && actionInfo is IVerifiable verifiable)
                        {
                            string actionVerifyLog;
                            if (verifiable.Verify(out actionVerifyLog) == false)
                            {
                                Common.Log.Logger.Instance.Error($"[RuneInfo][{runeInfo.Id}] " + actionVerifyLog);
                            }
                        }
                    }
                }
            }
        }

        public static void UnInit()
        {
            _instance = new StaticInfo();
        }

        public bool IsInitialized(StaticInfoType type)
        {
            return _initTypes.Contains(type);
        }

        public override IEnumerator Init(StaticInfoType staticInfoType, string path, bool forceInit = false, bool isToolMode = false)
        {
            if (_initProcessing == true)
            {
                Common.Log.Logger.Error($"StaticInfo[{staticInfoType}] initializing!");
                yield break;
            }
            if (_initTypes.Contains(staticInfoType) == true && forceInit == false)
            {
                Common.Log.Logger.Error($"StaticInfo[{staticInfoType}] already initialized!");
                yield break;
            }
            _initProcessing = true;
            Stopwatch sw = Stopwatch.StartNew();
            Common.Log.Logger.Info($"StaticInfo[{staticInfoType}] Init start, Path : {path}");
            List<IEnumerator> initCoroutines = new List<IEnumerator>();
            foreach (IKeyValueInfo info in GetKeyValueInfos())
            {
                if (staticInfoType == info.GetStaticInfoType())
                {
                    initCoroutines.Add(info.Init(path, true, false, isToolMode));
                }
            }

            foreach (ISpecificInfo info in GetSpecificInfos())
            {
                if (staticInfoType == info.GetStaticInfoType())
                {
                    initCoroutines.Add(info.Init(path, isToolMode));
                }
            }

            bool allDone = false;
            while (allDone == false)
            {
                allDone = true;
                foreach (IEnumerator coroutine in initCoroutines)
                {
                    if (coroutine.MoveNext() == true)
                    {
                        allDone = false;
                    }
                }

                yield return null;
            }

            GeoLite2Info.Init(path);
            BadWordInfo.Init(path + "/BadWords.txt");
            PostInit(staticInfoType);
            DataValidation();

            _initProcessing = false;
            _initTypes.Add(staticInfoType);
            Common.Log.Logger.Info($"StaticInfo {staticInfoType} init time : {sw.ElapsedMilliseconds}ms");
        }

        public override async Task InitAsync(StaticInfoType staticInfoType, string path, bool isToolMode = false)
        {
            if (_initTypes.Contains(staticInfoType) == true)
            {
                Common.Log.Logger.Error($"StaticInfo [{staticInfoType}] already initialized!");
                return;
            }

            _initTypes.Add(staticInfoType);
            Common.Log.Logger.Info($"StaticInfo [{staticInfoType}] Init start, Path : {path}");
            List<Task> tasks = new List<Task>();
            tasks.AddRange(GetKeyValueInfos()
                .Where(i => i.GetStaticInfoType() == staticInfoType)
                .Select(i => Task.Factory.StartNew(() => i.Init(path, true, false, isToolMode))));

            tasks.AddRange(GetSpecificInfos()
                .Where(i => i.GetStaticInfoType() == staticInfoType)
                .Select(i => Task.Factory.StartNew(() => i.Init(path, isToolMode))));

            await Task.WhenAll(tasks);
            GeoLite2Info.Init(path);
            PostInit(staticInfoType);
        }

        public void AddBuffTemplate((int buffId, int otherInfoId, int level) key, Core.Buff.BuffTemplate buffTemplate)
        {
            if (_buffTemplates.ContainsKey(key) == false)
            {
                _buffTemplates.Add(key, buffTemplate);
            }
            else
            {
                _buffTemplates[key] = buffTemplate;
            }
        }
        private void PostInit(StaticInfoType staticInfoType)
        {
            if (staticInfoType == StaticInfoType.Common)
            {
                _characterSoulIds = CharacterSoulInfo.GetList().ToDictionary(e => e.CharacterId, e => e.Id);
                ActorPostInit();
                ActionPostInit();

                foreach (CharacterInfo characterInfo in CharacterInfo.GetList())
                {
                    List<ActionDirectionInfo> infos = new List<ActionDirectionInfo>();
                    List<int> actionGroupIds = new List<int>();
                    characterInfo.ActionGroup.GetIds(in actionGroupIds);
                    if (actionGroupIds != null && actionGroupIds.Count > 0)
                    {
                        foreach (var actionGroupId in actionGroupIds)
                        {
                            if (StaticInfo.Instance.ActionGroupInfo.TryGet(actionGroupId, out var actionGroupInfo) == false ||
                                actionGroupInfo.GetActionDirectionListInfo() == null)
                            {
                                continue;
                            }
                            infos.AddRange(actionGroupInfo.GetActionDirectionListInfo().DirectionInfos);
                        }
                    }
                    for (int i = 0; i < infos?.Count; ++i)
                    {
                        infos[i].Elements.Sort((x, y) => x.T.CompareTo(y.T));
                    }
                }

                foreach (MonsterInfo monsterInfo in MonsterInfo.GetList())
                {
                    List<ActionDirectionInfo> infos = new List<ActionDirectionInfo>();

                    if (monsterInfo.GetDirectionListInfo() != null)
                    {
                        infos = monsterInfo.GetDirectionListInfo().DirectionInfos;
                    }
                    for (int i = 0; i < infos?.Count; ++i)
                    {
                        infos[i].Elements.Sort((x, y) => x.T.CompareTo(y.T));
                    }
                }

                foreach (CreatureInfo creatureInfo in CreatureInfo.GetList())
                {
                    List<ActionDirectionInfo> infos = new List<ActionDirectionInfo>();

                    if (creatureInfo.GetDirectionListInfo() != null)
                    {
                        infos = creatureInfo.GetDirectionListInfo().DirectionInfos;
                    }
                    for (int i = 0; i < infos?.Count; ++i)
                    {
                        infos[i].Elements.Sort((x, y) => x.T.CompareTo(y.T));
                    }
                }
                foreach (var tierInfo in WorldMissionTierInfo.GetList())
                {
                    if (_worldMissionTierInfos.ContainsKey(tierInfo.EventId) == false)
                    {
                        _worldMissionTierInfos.Add(tierInfo.EventId, new List<WorldMissionTierInfo>());
                    }
                    _worldMissionTierInfos[tierInfo.EventId].Add(tierInfo);
                }
                foreach (var coopRewardInfo in WorldMissionCoopRewardInfo.GetList())
                {
                    if (_worldMissionCoopRewardInfos.ContainsKey(coopRewardInfo.EventId) == false)
                    {
                        _worldMissionCoopRewardInfos.Add(coopRewardInfo.EventId, new List<WorldMissionCoopRewardInfo>());
                    }
                    _worldMissionCoopRewardInfos[coopRewardInfo.EventId].Add(coopRewardInfo);
                }
                foreach (var rewardInfo in WorldMissionScoreRewardInfo.GetList())
                {
                    if (_worldMissionScoreRewardInfos.ContainsKey(rewardInfo.EventId) == false)
                    {
                        _worldMissionScoreRewardInfos.Add(rewardInfo.EventId, new List<WorldMissionScoreRewardInfo>());
                    }
                    _worldMissionScoreRewardInfos[rewardInfo.EventId].Add(rewardInfo);
                }
                _itemKeys.Clear();
                _itemKeys.UnionWith(CharacterInfo.GetKeys());
                _itemKeys.UnionWith(CharacterSoulInfo.GetKeys());
                _itemKeys.UnionWith(WeaponInfo.GetKeys());
                _itemKeys.UnionWith(ArmorInfo.GetKeys());
                _itemKeys.UnionWith(MaterialInfo.GetKeys());
                _itemKeys.UnionWith(BoxInfo.GetKeys());
                _itemKeys.UnionWith(MoneyBoxInfo.GetKeys());
                _itemKeys.UnionWith(EventCoinInfo.GetKeys());
                _itemKeys.UnionWith(TicketInfo.GetKeys());

                _weaponInfosByType = WeaponInfo.GetList().GroupBy(e => e.WeaponType).ToDictionary(g => g.Key, g => g.ToList());

                var equipmentInfos = new List<EquipmentInfo>();
                equipmentInfos.AddRange(WeaponInfo.GetList());
                equipmentInfos.AddRange(ArmorInfo.GetList());
                _equipmentInfos = equipmentInfos.ToDictionary(e => e.Key, e => e);

                var consumeInfos = new List<ConsumeInfo>();
                consumeInfos.AddRange(CharacterSoulInfo.GetList());
                consumeInfos.AddRange(MaterialInfo.GetList());
                consumeInfos.AddRange(BoxInfo.GetList());
                consumeInfos.AddRange(MoneyBoxInfo.GetList());
                consumeInfos.AddRange(EventCoinInfo.GetList());
                consumeInfos.AddRange(TicketInfo.GetList());
                _consumeInfos = consumeInfos.ToDictionary(e => e.Key, e => e);
            }

            // ConsumeShortcut
            _consumeShortcutInfos = ConsumeShortcutInfo.GetList().GroupBy(e => e.Id).ToDictionary(g => g.Key, g => g.OrderBy(x => x.SubId).ToList());
            // must be called after all manual post init process done.
            PostInitInfos(staticInfoType);

            // Verify, PostProcess 후
            if (staticInfoType == StaticInfoType.Common)
            {
                // RandomItemSet
                MakeRandomItemSet();
                // SelectableItemSet
                MakeSelectableItemSet();

                // MaterialExchange
                _materialExchangeInfos = MaterialExchangeInfo.GetList().GroupBy(e => e.Id).ToDictionary(g => g.Key, g => g.OrderBy(x => x.SubId).ToList());
                // Gacha
                _gachaSetInfos = GachaSetInfo.GetList().GroupBy(e => e.Id).ToDictionary(g => g.Key, g => g.OrderBy(x => (ushort)x.Rarity).ToList());
                // GachaFake
                _gachaFakeProbs = GachaFakeInfo.GetList().GroupBy(e => e.Rarity).ToDictionary(g => g.Key, g => g.ToDictionary(x => x.FakeType, x => x.Probability));
                MakeGachaItemProbs();
                // EpisodeGroupInfo 그룹 및 정렬
                _sortedEpisodeGroupsByType = EpisodeGroupInfo.GetList().GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Number).ToList());

                // EpisodeInfo 그룹 및 정렬
                _sortedEpisodesByGroup = EpisodeInfo.GetList().Where(e => e.Enabled == true).GroupBy(e => e.EpisodeGroupId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Number).ToList());

                // EpisodeInfo 그룹 및 정렬
                _sortedEpisodesByGroupType = EpisodeInfo.GetList().Where(e => e.Enabled == true).GroupBy(e => e.GroupType).ToDictionary(g => g.Key, g => g.OrderBy(x => x.No).ToList());

                // FirstEpisode Id
                StaticData.Episode.EpisodeGroupInfo.SetStartEpisodeGroup();
                StaticData.Episode.EpisodeInfo.SetStartEpisode();

                // NextEpisodeId
                EpisodeInfo.GetList().ForEach(e => e.SetNextEpisodeId());

                _missionInfos.Clear();
                _allQuestInfos.Clear();
                foreach (MissionInfo missionInfo in BattleTutorialInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                foreach (MissionInfo missionInfo in LobbyTutorialInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                foreach (EpisodeGroupMissionInfo missionInfo in EpisodeGroupMissionInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                    if (_episodeGroupMissionInfos.ContainsKey(missionInfo.EpisodeGroupId) == false)
                    {
                        _episodeGroupMissionInfos.Add(missionInfo.EpisodeGroupId, new List<MissionInfo>());
                    }
                    _episodeGroupMissionInfos[missionInfo.EpisodeGroupId].Add(missionInfo);
                }
                foreach (CharacterStoryMissionInfo missionInfo in CharacterStoryMissionInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                foreach (QuestInfo questInfo in QuestInfo.GetList())
                {
                    _missionInfos.Add(questInfo.Id, questInfo);
                    _allQuestInfos.Add(questInfo.Id, questInfo);
                }
                foreach (QuestInfo questInfo in DailyQuestInfo.GetList())
                {
                    _missionInfos.Add(questInfo.Id, questInfo);
                    _allQuestInfos.Add(questInfo.Id, questInfo);
                }
                foreach (QuestInfo questInfo in WeeklyQuestInfo.GetList())
                {
                    _missionInfos.Add(questInfo.Id, questInfo);
                    _allQuestInfos.Add(questInfo.Id, questInfo);
                }
                foreach (MissionInfo missionInfo in DailyPointRewardQuestInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                foreach (MissionInfo missionInfo in WeeklyPointRewardQuestInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                foreach (MissionInfo missionInfo in AchievementInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                foreach (MissionInfo missionInfo in AchievementPointRewardQuestInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                foreach (MissionInfo missionInfo in StackPointEventInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                UISoundInfo.Get().MakeUISoundList();
                StaticData.AccountLevelUpInfo.SetMaxLevel(AccountLevelUpInfo.GetKeys().OrderByDescending(i => i).First());

                StaticData.Event.AttendanceInfo.ComputeRewards(AttendanceInfo, AttendanceRewardSetInfo);

                _exchangeEventInfos = ExchangeEventInfo.GetList().GroupBy(e => e.EventId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.SubKey).ToList());
                _stackPointEventInfos = StackPointEventInfo.GetList().GroupBy(e => e.EventId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Id).ToList());
                _stackPointEventInfosV2 = StackPointEventInfoV2.GetList().GroupBy(e => e.EventId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.SubId).ToList());
                foreach (var info in ConquestSeasonInfo.GetList())
                {
                    _conquestSeasonIds.Add(info.Id);
                }

                _dailyQuestEventInfos = DailyQuestEventInfo.GetList().GroupBy(e => e.EventId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.MissionId).ToList());
                _achievementEventInfos = AchievementEventInfo.GetList().GroupBy(e => e.EventId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.MissionId).ToList());
                _exchangeEventInfos = ExchangeEventInfo.GetList().GroupBy(e => e.EventId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.ExchangeId).ToList());

                _allQuestEventInfos.Clear();
                foreach (QuestEventInfo questInfo in DailyQuestEventInfo.GetList())
                {
                    _missionInfos.Add(questInfo.Id, questInfo);
                    _allQuestEventInfos.Add(questInfo.Id, questInfo);
                }
                foreach (QuestEventInfo questInfo in AchievementEventInfo.GetList())
                {
                    _missionInfos.Add(questInfo.Id, questInfo);
                    _allQuestEventInfos.Add(questInfo.Id, questInfo);
                }

                _episodeRewardEventInfos = EpisodeRewardEventInfo.GetList().GroupBy(e => e.EventId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.EpisodeId).ToDictionary(g => g.EpisodeId));
                _episodeClearEventInfos = EpisodeClearEventInfo.GetList().GroupBy(e => e.EventId).ToDictionary(e => e.Key, g => g.OrderBy(x => x.Id).ToList());
                foreach (MissionInfo missionInfo in EpisodeClearEventInfo.GetList())
                {
                    _missionInfos.Add(missionInfo.Id, missionInfo);
                }
                _lotteryBoardRewardInfos = LotteryBoardRewardInfo.GetList().GroupBy(e => e.EventId).ToDictionary(g => g.Key, g => g.OrderBy(e => e.Id).ToDictionary(g => g.SubId));
                _lotteryBoardPresetInfos = LotteryBoardPresetInfo.GetList().GroupBy(e => e.PresetId).ToDictionary(e => e.Key, g => g.ToList());

                _conquestTierInfos = ConquestTierInfo.GetList().GroupBy(e => e.TierSeasonId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.TierId).ToDictionary(g => g.TierId));
                _towerInfos = TowerInfo.GetList().GroupBy(e => e.SeasonId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Floor).ToDictionary(g => g.EpisodeId));

                _desSortedValueMaterials = MaterialInfo.GetList().Where(c => c.Value != 0).GroupBy(e => e.MaterialType).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Value).ToList());
                foreach(var info in PostGenerateEventActionId)
                {
                    info.Id = info.GenerateId();
                    EventActionInfoIdChecker.Add(info.Id, nameof(EventActionInfo));
                }
            }
        }
        private void PostInitInfos(StaticInfoType staticInfoType)
        {
            foreach (IKeyValueInfo info in GetKeyValueInfos())
            {
                if (staticInfoType == info.GetStaticInfoType())
                {
                    info.PostInit();
                }
            }

            foreach (ISpecificInfo info in GetSpecificInfos())
            {
                if (staticInfoType == info.GetStaticInfoType())
                {
                    info.PostInit();
                }
            }
        }

        private void DataValidation()
        {
            ConquestSeasonInfo prevSeasonInfo = null;
            foreach (var seasonInfo in ConquestSeasonInfo.GetList())
            {
                if(prevSeasonInfo == null)
                {
                    prevSeasonInfo = seasonInfo;
                    continue;
                }
                var intervalSeconds = (seasonInfo.TotalPeriod.Start - prevSeasonInfo.TotalPeriod.End).TotalSeconds;
                if (intervalSeconds < Dash.StaticData.ConquestSeasonInfo.MinSeasonIntervalSeconds)
                {
                    throw new Exception($"ConquestSeasonInfo OpenPeriod Verify failed, " +
                        $"PrevSeason : [{prevSeasonInfo.Id}][{prevSeasonInfo.TotalPeriod.Start}~{prevSeasonInfo.TotalPeriod.End}] " +
                        $"Season : [{seasonInfo.Id}][{seasonInfo.TotalPeriod.Start}~{seasonInfo.TotalPeriod.End}]");
                }
                prevSeasonInfo = seasonInfo;
            }
            foreach (var characterInfo in CharacterInfo.GetList())
            {
                if (characterInfo.ActionGroup.Base != 0 && ActionGroupInfo.Exist(characterInfo.ActionGroup.Base) == false)
                {
                    Common.Log.Logger.Instance.Error($"Wrong ActionGroup CharacterId:{characterInfo.Id} {nameof(characterInfo.ActionGroup.Base)}:{characterInfo.ActionGroup.Base}");
                }

                if (characterInfo.ActionGroup.Skill != 0 && ActionGroupInfo.Exist(characterInfo.ActionGroup.Skill) == false)
                {
                    Common.Log.Logger.Instance.Error($"Wrong ActionGroup CharacterId:{characterInfo.Id} {nameof(characterInfo.ActionGroup.Skill)}:{characterInfo.ActionGroup.Skill}");
                }

                if (characterInfo.ActionGroup.UltSkill != 0 && ActionGroupInfo.Exist(characterInfo.ActionGroup.UltSkill) == false)
                {
                    Common.Log.Logger.Instance.Error($"Wrong ActionGroup CharacterId:{characterInfo.Id} {nameof(characterInfo.ActionGroup.UltSkill)}:{characterInfo.ActionGroup.UltSkill}");
                }
            }

            {
                var duplicated = LobbyTutorialInfo.GetList().GroupBy(e => e.TutorialType).FirstOrDefault(g => g.Count() > 1);
                if (duplicated != null)
                {
                    Common.Log.Logger.Instance.Error($"Duplicated TutorialType :{duplicated.Key}");
                }
            }

            {
                var duplicated = BattleTutorialInfo.GetList().GroupBy(e => e.TutorialType).FirstOrDefault(g => g.Count() > 1);
                if (duplicated != null)
                {
                    Common.Log.Logger.Instance.Error($"Duplicated TutorialType :{duplicated.Key}");
                }
            }
            {
                HashSet<(int, int)> keys = new HashSet<(int, int)>();
                foreach (EpisodeGroupMissionInfo missionInfo in EpisodeGroupMissionInfo.GetList())
                {
                    if (keys.Contains((missionInfo.EpisodeGroupId, missionInfo.StarCount)))
                    {
                        Common.Log.Logger.Instance.Error($"Duplicated EpisodeGroupMission. EpisodeGroupId : {missionInfo.EpisodeGroupId}, StarCount : {missionInfo.StarCount}");
                    }
                    else
                    {
                        keys.Add((missionInfo.EpisodeGroupId, missionInfo.StarCount));
                    }
                }
            }
            {
                foreach (var template in BuffTemplates)
                {
                    if (BuffInfo.TryGet(template.Value.BuffId, out var buffInfo) == false)
                    {
                        Common.Log.Logger.Instance.Error($"[DataValidation][{template.Value.BuffId}] Invalid BuffTemplate.BuffId");
                    }
                    if (template.Value.StatusTypes.Count != template.Value.Coefficients.Count)
                    {
                        Common.Log.Logger.Instance.Error($"[DataValidation][{template.Value.BuffId}][StatusTypes.Count {template.Value.StatusTypes.Count}] [Coefficients.Count {template.Value.Coefficients.Count}] StatusTypes.Count and Coefficients.Count should be same.");
                    }
                    if ((buffInfo is IHasStatusCoefficient) == false)
                    {
                        foreach (var coefficient in template.Value.Coefficients)
                        {
                            if (coefficient.IsZero() == false)
                            {
                                Common.Log.Logger.Instance.Error($"[DataValidation][{template.Value.BuffId}] Invalid BuffTemplate.Coefficient");
                            }
                        }
                    }
                    if (template.Value.StatusDeltaValue.IsZero() == false && (buffInfo is StatusBuffInfo) == false)
                    {
                        Common.Log.Logger.Instance.Error($"[DataValidation][{template.Value.BuffId}] Invalid BuffTemplate.StatusDeltaValue");
                    }
                }
            }
            {
#if UNITY_EDITOR
                // ActionInfo, DirectionInfo 갯수 다른 곳 찾아주는 로그
                foreach (var monsterInfo in MonsterInfo.GetList())
                {
                    int directionInfoCount = monsterInfo.GetDirectionListInfo()?.DirectionInfos?.Count ?? 0;
                    if (ActionContainerListInfo.TryGet(monsterInfo.ActionContainerListId, out var actionContainerListInfo) == false)
                    {
                        Common.Log.Logger.Instance.Error($"[DataValidation][{monsterInfo.Comment}] {nameof(MonsterInfo)} Invalid ActionContainerListId");
                        continue;
                    }

                    int actionInfoCount = 0;
                    foreach (var actionContainer in actionContainerListInfo.ActionContainers)
                    {
                        actionInfoCount += actionContainer.GetActionInfos.Count;
                    }

                    if (directionInfoCount != actionInfoCount)
                    {
                        Common.Log.Logger.Instance.Error($"[DataValidation][{monsterInfo.Comment}] {nameof(MonsterInfo)} Invalid directionInfoCount and actionInfoCount");
                    }
                }

                foreach (var monsterInfo in BossMonsterInfo.GetList())
                {
                    int directionInfoCount = monsterInfo.GetDirectionListInfo()?.DirectionInfos?.Count ?? 0;
                    if (ActionContainerListInfo.TryGet(monsterInfo.ActionContainerListId, out var actionContainerListInfo) == false)
                    {
                        Common.Log.Logger.Instance.Error($"[DataValidation][{monsterInfo.Comment}] {nameof(BossMonsterInfo)} Invalid ActionContainerListId");
                        continue;
                    }
                    int actionInfoCount = 0;
                    foreach (var actionContainer in actionContainerListInfo.ActionContainers)
                    {
                        actionInfoCount += actionContainer.GetActionInfos.Count;
                    }
                    if (directionInfoCount != actionInfoCount)
                    {
                        Common.Log.Logger.Instance.Error($"[DataValidation][{monsterInfo.Comment}] {nameof(BossMonsterInfo)} Invalid directionInfoCount and actionInfoCount");
                    }
                }

                foreach (var actionGroupInfo in ActionGroupInfo.GetList())
                {
                    int directionInfoCount = actionGroupInfo.GetActionDirectionListInfo()?.DirectionInfos?.Count ?? 0;
                    int actionInfoCount = actionGroupInfo.Action.GetActionInfos.Count;
                    if (directionInfoCount != actionInfoCount)
                    {
                        Common.Log.Logger.Instance.Error($"[DataValidation][{actionGroupInfo.Comment}] {nameof(ActionGroupInfo)} Invalid directionInfoCount and actionInfoCount");
                    }
                }
#endif// UNITY_EDITOR
            }
            {
#if UNITY_EDITOR
                var actionInfos = new List<ActionInfo>();
                ActionGroupInfo.GetList().Select(e => e.Action.GetActionInfos).ForEach(e => actionInfos.AddRange(e));
                ActionContainerInfos.GetList().Select(e => e.GetActionInfos).ForEach(e => actionInfos.AddRange(e));

                foreach (var actionInfo in actionInfos)
                {
                    if (actionInfo is CreateGeneratorActionInfo createGeneratorActionInfo)
                    {
                        if (createGeneratorActionInfo.ShowPreview == true)
                        {
                            if (GeneratorInfo.TryGet(createGeneratorActionInfo.GeneratorId, out var generatorInfo) == true)
                            {
                                if (generatorInfo is IPreviewGeneratorInfo == false)
                                {
                                    Common.Log.Logger.Instance.Error($"[DataValidation][{actionInfo.Comment}] {generatorInfo.comment} does not inherit from IPreviewGeneratorInfo.");
                                }
                            }
                        }
                    }
                }
#endif// UNITY_EDITOR
            }
        }

        #region Utility Methods
        /*
        public int CalcJewelForBuyGold(int amount)
        {
            if (amount <= 0) return 0;
            // 판매 상품 중 가성비 가장 안좋은 비율로 처리 상품이 없다면 기본값 0.006
            return (int)math.ceil(amount * StaticData.Shop.ProductInfo.JewelGoldRate);
        }
        public int CalcJewelForBuyStamina(int amount)
        {
            if (amount <= 0) return 0;
            // 판매 상품 중 가성비 가장 안좋은 비율로 처리 상품이 없다면 기본값 0.006
            return (int)math.ceil(amount * StaticData.Shop.ProductInfo.JewelStaminaRate);
        }
        */
        public List<EpisodeInfo> GetEpisodeInfos(EpisodeGroupType episodeGroupType)
        {
            return EpisodeInfo.GetList().Where(e => e.GroupType == episodeGroupType).OrderBy(e => e.EpisodeNo).ToList();
        }

        public EpisodeInfo GetFirstEpisodeInfo(EpisodeGroupType episodeGroupType)
        {
            if (SortedEpisodeGroupsByType.TryGetValue(episodeGroupType, out var episodeGroups) == false)
                return null;

            var firstGroup = episodeGroups.FirstOrDefault();
            if (firstGroup == null)
                return null;

            if (SortedEpisodesByGroup.TryGetValue(firstGroup.Id, out var episodes) == false)
                return null;

            return episodes.FirstOrDefault();
        }

        private void MakeSelectableItemSet()
        {
            _selectableItemSets.Clear();
            var selectableItemGroup = SelectableItemSetInfo.GetList().GroupBy(e => e.Id);
            foreach (var group in selectableItemGroup)
            {
                List<ItemModel> randomItems = new List<ItemModel>();
                foreach (var setInfo in group)
                {
                    randomItems.Add(setInfo.Item);
                }
                if (_selectableItemSets.ContainsKey(group.Key) == false)
                    _selectableItemSets.Add(group.Key, new List<ItemModel>());
                _selectableItemSets[group.Key].AddRange(randomItems);
            }
        }

        private void MakeRandomItemSet()
        {
            {
                var totalProbabilities = RandomItemSetInfo.GetList().GroupBy(e => e.Id).ToDictionary(g => g.Key, g => g.Sum(e => e.Probability));
                foreach (var info in RandomItemSetInfo.GetList())
                {
                    info.Probability = info.Probability / totalProbabilities[info.Id];
                }
            }
            _randomItemSets.Clear();
            foreach (var info in RandomItemSetInfo.GetList())
            {
                List<RandomItemInfo> randomItems = new List<RandomItemInfo>();
                List<IdKeyData> list = null;

                switch (info.ItemType)
                {
                    case ItemType.Character:
                        list = CharacterInfo.GetList().Where(e => e.Filter(info.Filter)).Select(e => (IdKeyData)e).ToList();
                        break;
                    case ItemType.Weapon:
                        list = WeaponInfo.GetList().Where(e => e.Filter(info.Filter)).Select(e => (IdKeyData)e).ToList();
                        break;
                    case ItemType.Armor:
                        list = ArmorInfo.GetList().Where(e => e.Filter(info.Filter)).Select(e => (IdKeyData)e).ToList();
                        break;
                    case ItemType.CharacterSoul:
                        list = CharacterSoulInfo.GetList().Where(e => e.Filter(info.Filter)).Select(e => (IdKeyData)e).ToList();
                        break;
                    case ItemType.Material:
                        list = MaterialInfo.GetList().Where(e => e.Filter(info.Filter)).Select(e => (IdKeyData)e).ToList();
                        break;
                    case ItemType.RewardBox:
                        list = BoxInfo.GetList().Where(e => e.Filter(info.Filter)).Select(e => (IdKeyData)e).ToList();
                        break;
                    case ItemType.MoneyBox:
                        list = MoneyBoxInfo.GetList().Where(e => e.Filter(info.Filter)).Select(e => (IdKeyData)e).ToList();
                        break;
                    case ItemType.Ticket:
                        list = TicketInfo.GetList().Where(e => e.Filter(info.Filter)).Select(e => (IdKeyData)e).ToList();
                        break;
                }

                var count = list?.Count ?? 0;
                foreach (var item in list)
                {
                    var itemProbability = info.Probability / count;
                    randomItems.Add(new RandomItemInfo { Id = item.Id, Probability = itemProbability, Count = info.Count });
                }

                if (_randomItemSets.ContainsKey(info.Id) == false)
                    _randomItemSets.Add(info.Id, new List<RandomItemInfo>());

                _randomItemSets[info.Id].AddRange(randomItems);
            }

#if UNITY_EDITOR && false
            Common.Log.Logger.Info($"-- RandomItemSets Begin ---");
            foreach (var p in _randomItemSets)
            {
                foreach (var i in p.Value)
                {
                    Common.Log.Logger.Info($"{p.Key}/{i.Id}/{i.Probability}/{i.ItemType}/{i.Rarity}");
                }
                Common.Log.Logger.Info($"{p.Key} : {p.Value.Sum(e => e.Probability)}");
            }
            Common.Log.Logger.Info($"-- RandomItemSets End ---");
#endif
        }

        public void MakeGachaItemProbs()
        {
            _gachaItemProbs.Clear();
            foreach (var info in GachaInfo.GetList())
            {
                var itemProbs = new Dictionary<(int, int), double>();
                double pityProbability = 0;
                if (_gachaSetInfos.TryGetValue(info.Pity.GachaSetId, out var pitySetInfos))
                {
                    pityProbability = info.Pity.Probabilities?.FirstOrDefault()?.Probability ?? 0;
                    foreach (var setInfo in pitySetInfos)
                    {
                        double probability = setInfo.Probability * pityProbability / setInfo.FilteredItems.Count;
                        foreach (var itemId in setInfo.FilteredItems)
                        {
                            probability = probability / (setInfo.Count.Max - setInfo.Count.Min + 1);
                            for (int count = setInfo.Count.Min; count <= setInfo.Count.Max; ++count)
                            {
                                var key = (itemId, count);
                                if (itemProbs.ContainsKey(key) == true)
                                    itemProbs[key] += probability;
                                else
                                    itemProbs[key] = probability;
                            }
                        }
                    }
                }
                if (_gachaSetInfos.TryGetValue(info.GachaSetId, out var setInfos))
                {
                    double normalProbability = 1 - pityProbability;
                    foreach (var setInfo in setInfos)
                    {
                        double probability = setInfo.Probability * normalProbability / setInfo.FilteredItems.Count;
                        foreach (var itemId in setInfo.FilteredItems)
                        {
                            probability = probability / (setInfo.Count.Max - setInfo.Count.Min + 1);
                            for (int count = setInfo.Count.Min; count <= setInfo.Count.Max; ++count)
                            {
                                var key = (itemId, count);
                                if (itemProbs.ContainsKey(key) == true)
                                    itemProbs[key] += probability;
                                else
                                    itemProbs[key] = probability;
                            }
                        }
                    }
                }
                _gachaItemProbs.Add(info.Id, itemProbs);
            }

#if UNITY_EDITOR && false
            Common.Log.Logger.Info($"-- GachaItemProbs Begin ---");
            foreach (var p in _gachaItemProbs)
            {
                foreach (var i in p.Value)
                {
                    var itemType = ItemTypeHelper.GetItemType(i.Key);
                    var rarity = ItemTypeHelper.GetRarity(i.Key);
                    Common.Log.Logger.Info($"{p.Key}/{i.Key}/{i.Value}/{itemType}/{rarity}");
                }
                Common.Log.Logger.Info($"{p.Key} : {p.Value.Sum(e => e.Value)}");
            }
            Common.Log.Logger.Info($"-- GachaItemProbs End ---");
#endif
        }
        #endregion


        #region Locale
        public void GetAllLocaleKeys(SortedDictionary<string, HashSet<string>> keyPaths)
        {
            // Init 된 후에 사용해야 한다.
            foreach (IKeyValueInfo keyValueInfo in GetKeyValueInfos())
            {
                if (keyValueInfo.GetStaticInfoType() != StaticInfoType.Common) continue;

                Type infoType = keyValueInfo.GetValueType();
                MethodInfo methodInfo = keyValueInfo.GetType().GetMethod("GetInfos");
                if (methodInfo != null)
                {
                    IDictionary infos = (IDictionary)methodInfo.Invoke(keyValueInfo, null);
                    foreach (DictionaryEntry info in infos)
                    {
                        if (info.Value is IPublicable publicable && publicable.IsPublic == false) continue;
                        TypeContext typeContext = new TypeContext(infoType);
                        string path = $"{typeContext.Type.FullName}[{info.Key}]";
                        AddLocaleKeyRecursively(typeContext, info.Value, keyPaths, path);
                    }
                }
            }

            foreach (ISpecificInfo specificInfo in GetSpecificInfos())
            {
                if (specificInfo.GetStaticInfoType() != StaticInfoType.Common) continue;

                Type infoType = specificInfo.GetInfoType();
                MethodInfo methodInfo = specificInfo.GetType().GetMethod("Get");
                if (methodInfo != null)
                {
                    object info = methodInfo.Invoke(specificInfo, null);
                    TypeContext typeContext = new TypeContext(infoType);
                    string path = $"{typeContext.Type.FullName}";
                    AddLocaleKeyRecursively(typeContext, info, keyPaths, path);
                }
            }
        }

        private void AddLocaleKeyRecursively(TypeContext context, object value, SortedDictionary<string, HashSet<string>> keyPaths, string path = "")
        {
            if (value == null) return;
            if (value is IPublicable publicable && publicable.IsPublic == false) return;

            string localeKey;
            path = string.IsNullOrEmpty(path) ? context.Name : path;
            //Common.Log.Logger.Info(path);
            if (context.Type == typeof(Locale) || context.Type == typeof(LocaleWithArgs))
            {
                localeKey = (string)context.Type.GetProperty("Key").GetValue(value);
                if (string.IsNullOrEmpty(localeKey) == true) return;
            }
            else if (context.Type == typeof(TextAssetLocale))
            {
                localeKey = (string)context.Type.GetProperty("Key").GetValue(value);
                if (string.IsNullOrEmpty(localeKey) == true) return;
            }
            else if (value is IDictionary dictionary)
            {
                //Common.Log.Logger.Info($"{path} is IDictionary");
                foreach (var key in dictionary.Keys)
                {
                    var childValue = dictionary[key];
                    TypeContext childContext = new TypeContext(childValue.GetType());
                    //Common.Log.Logger.Info($"{path}.{childContext.Name} / {childContext.Type} / {context.Type} / {item == null}");
                    string childPath = path + "[" + key + "]";
                    AddLocaleKeyRecursively(childContext, childValue, keyPaths, childPath);
                }
                return;
            }
            else if (value is IList list)
            {
                //Common.Log.Logger.Info($"{path} is IList");
                int i = 0;
                foreach (var childValue in list)
                {
                    TypeContext childContext = new TypeContext(childValue.GetType());
                    //Common.Log.Logger.Info($"{path}.{childContext.Name} / {childContext.Type} / {context.Type} / {item == null}");
                    string childPath = path + "[" + i + "]";
                    AddLocaleKeyRecursively(childContext, childValue, keyPaths, childPath);
                    ++i;
                }
                return;
            }
            else
            {
                foreach (TypeContext childContext in context.Children)
                {
                    //Common.Log.Logger.Info($"{path}.{childContext.Name} / {childContext.Type} / {context.Type} / {value == null}");

                    var field = context.Type.GetField(childContext.Name);
                    var property = context.Type.GetProperty(childContext.Name);
                    object childValue = null;
                    if (field != null)
                        childValue = context.Type.GetField(childContext.Name).GetValue(value);
                    else if (property != null)
                        childValue = context.Type.GetProperty(childContext.Name).GetValue(value);

                    if (childValue == null)
                        continue;
                    if (field?.GetCustomAttribute<NoLocaleAttribute>() != null || property?.GetCustomAttribute<NoLocaleAttribute>() != null)
                        continue;

                    string childPath = path + "." + childContext.Name;
                    AddLocaleKeyRecursively(childContext, childValue, keyPaths, childPath);
                }
                return;
            }
            //Common.Log.Logger.Info($"[{localeKey}] / {path}");
            path = $"Data.{path}";
            if (keyPaths.ContainsKey(localeKey))
            {
                keyPaths[localeKey].Add(path);
            }
            else
            {
                keyPaths.Add(localeKey, new HashSet<string> { path });
            }
        }

        public string GetPathByType(Type type)
        {
            object info = null;
            if (KeyValueInfosByType.TryGetValue(type, out IKeyValueInfo keyValueInfo))
            {
                return $"{keyValueInfo.GetPath()}.{keyValueInfo.GetFormatType()}";
            }
            else if (SpecificInfosByType.TryGetValue(type, out ISpecificInfo specificInfo))
            {
                return $"{specificInfo.GetPath()}.{specificInfo.GetFormatType()}";
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
