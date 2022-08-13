using Common.Net.WWW;

namespace Dash
{
    public static class LobbyAPIs
    {
        public static class Account
        {
            public const string Login = "/Lobby/Account/Login";
            public static readonly WWWAPI LoginAPI = new WWWAPI(Login, PathVariableType.None);

            public const string PlayerInfo = "/Lobby/Account/PlayerInfo/{0}";
            public static readonly WWWAPI PlayerInfoAPI = new WWWAPI(PlayerInfo, PathVariableType.OidAccount);

            public const string CalcRetention = "/Lobby/Account/CalcRetention/{0}";
            public static readonly WWWAPI CalcRetentionAPI = new WWWAPI(CalcRetention, PathVariableType.OidAccount);

            public const string OtherPlayerInfo = "/Lobby/Account/OtherPlayerInfo/{0}/{1}";
            public static readonly WWWAPI OtherPlayerInfoAPI = new WWWAPI(OtherPlayerInfo, PathVariableType.OidAccount);

            public const string SearchPlayerNickname = "/Lobby/Account/SearchPlayerNickname/{0}";
            public static readonly WWWAPI SearchPlayerNicknameAPI = new WWWAPI(SearchPlayerNickname, PathVariableType.OidAccount);

            public const string SetNickname = "/Lobby/Account/SetNickname/{0}";
            public static readonly WWWAPI SetNicknameAPI = new WWWAPI(SetNickname, PathVariableType.OidAccount);

            public const string Reset = "/Lobby/Account/Reset/{0}";
            public static readonly WWWAPI ResetAPI = new WWWAPI(Reset, PathVariableType.OidAccount);

            public const string ServerTime = "/Lobby/Account/ServerTime";
            public static readonly WWWAPI ServerTimeAPI = new WWWAPI(ServerTime, PathVariableType.None);

            public const string ResetEpisodeEntryCount = "/Lobby/Account/ResetEpisodeEntryCount/{0}";
            public static readonly WWWAPI ResetEpisodeEntryCountAPI = new WWWAPI(ResetEpisodeEntryCount, PathVariableType.OidAccount);

            public const string GetEpisodeEntryCount = "/Lobby/Account/GetEpisodeEntryCount/{0}";
            public static readonly WWWAPI GetEpisodeEntryCountAPI = new WWWAPI(GetEpisodeEntryCount, PathVariableType.OidAccount);

            public const string SetDisplayCharacter = "/Lobby/Account/SetDisplayCharacter/{0}";
            public static readonly WWWAPI SetDisplayCharacterAPI = new WWWAPI(SetDisplayCharacter, PathVariableType.OidAccount);

            public const string HiveDelete = "/Lobby/Account/HiveDelete/{0}";
            public static readonly WWWAPI HiveDeleteAPI = new WWWAPI(HiveDelete, PathVariableType.OidAccount);
        }

        public static class Money
        {
            public const string Exchange = "/Lobby/Money/Exchange/{0}";
            public static readonly WWWAPI ExchangeAPI = new WWWAPI(Exchange, PathVariableType.OidAccount);
        }
        public static class Mission
        {
            public const string GetCompletedMissions = "/Lobby/Mission/GetCompletedMissions/{0}";
            public static readonly WWWAPI GetCompletedMissionsAPI = new WWWAPI(GetCompletedMissions, PathVariableType.OidAccount);

            public const string CompleteMission = "/Lobby/Mission/CompleteMission/{0}";
            public static readonly WWWAPI CompleteMissionAPI = new WWWAPI(CompleteMission, PathVariableType.OidAccount, requestPolicyType: RequestPolicyType.None);

            public const string CompleteMissions = "/Lobby/Mission/CompleteMissions/{0}";
            public static readonly WWWAPI CompleteMissionsAPI = new WWWAPI(CompleteMissions, PathVariableType.OidAccount);

            public const string CompleteEpisodeGroupMission = "/Lobby/Mission/CompleteEpisodeGroupMission/{0}";
            public static readonly WWWAPI CompleteEpisodeGroupMissionAPI = new WWWAPI(CompleteEpisodeGroupMission, PathVariableType.OidAccount, requestPolicyType: RequestPolicyType.None);

            public const string CompleteCharacterStoryMission = "/Lobby/Mission/CompleteCharacterStoryMission/{0}";
            public static readonly WWWAPI CompleteCharacterStoryMissionAPI = new WWWAPI(CompleteCharacterStoryMission, PathVariableType.OidAccount, requestPolicyType: RequestPolicyType.None);

            public const string RewardQuestPoint = "/Lobby/Mission/RewardQuestPoint/{0}";
            public static readonly WWWAPI RewardQuestPointAPI = new WWWAPI(RewardQuestPoint, PathVariableType.OidAccount, requestPolicyType: RequestPolicyType.None);

            public const string ResetQuestTimeReward = "/Lobby/Mission/ResetQuestTimeReward/{0}";
            public static readonly WWWAPI ResetQuestTimeRewardAPI = new WWWAPI(ResetQuestTimeReward, PathVariableType.OidAccount, requestPolicyType: RequestPolicyType.None);

            public const string ProgressDailyQuestAds = "/Lobby/Mission/ProgressDailyQuestAds/{0}";
            public static readonly WWWAPI ProgressDailyQuestAdsAPI = new WWWAPI(ProgressDailyQuestAds, PathVariableType.OidAccount);
        }

        public static class Character
        {
            public const string UpdateDeck = "/Lobby/Character/UpdateDeck/{0}";
            public static readonly WWWAPI UpdateDeckAPI = new WWWAPI(UpdateDeck, PathVariableType.OidAccount);

            public const string UpdateMultipleDeck = "/Lobby/Character/UpdateMultipleDeck/{0}";
            public static readonly WWWAPI UpdateMultipleDeckAPI = new WWWAPI(UpdateMultipleDeck, PathVariableType.OidAccount);

            public const string CharacterUnLock = "/Lobby/Character/CharacterUnLock/{0}";
            public static readonly WWWAPI CharacterUnLockAPI = new WWWAPI(CharacterUnLock, PathVariableType.OidAccount);

            public const string CharacterExpUp = "/Lobby/Character/CharacterExpUp/{0}";
            public static readonly WWWAPI CharacterExpUpAPI = new WWWAPI(CharacterExpUp, PathVariableType.OidAccount);

            public const string CharacterOvercomeUp = "/Lobby/Character/CharacterOvercomeUp/{0}";
            public static readonly WWWAPI CharacterOvercomeUpAPI = new WWWAPI(CharacterOvercomeUp, PathVariableType.OidAccount);

            public const string CharacterRankUp = "/Lobby/Character/CharacterRankUp/{0}";
            public static readonly WWWAPI CharacterRankUpAPI = new WWWAPI(CharacterRankUp, PathVariableType.OidAccount);

            public const string CharacterRuneUnlock = "/Lobby/Character/CharacterRuneUnlock/{0}";
            public static readonly WWWAPI CharacterRuneUnlockAPI = new WWWAPI(CharacterRuneUnlock, PathVariableType.OidAccount);
        }

        public static class Equip
        {
            public const string EquipEquipment = "/Lobby/Equip/EquipEquipment/{0}";
            public static readonly WWWAPI EquipEquipmentAPI = new WWWAPI(EquipEquipment, PathVariableType.OidAccount);

            public const string UnEquipEquipment = "/Lobby/Equip/UnEquipEquipment/{0}";
            public static readonly WWWAPI UnEquipEquipmentAPI = new WWWAPI(UnEquipEquipment, PathVariableType.OidAccount);

            public const string EquipRune = "/Lobby/Equip/EquipRune/{0}";
            public static readonly WWWAPI EquipRuneAPI = new WWWAPI(EquipRune, PathVariableType.OidAccount);
        }

        public static class Item
        {
            public const string EquipmentExpUp = "/Lobby/Item/EquipmentExpUp/{0}";
            public static readonly WWWAPI EquipmentExpUpAPI = new WWWAPI(EquipmentExpUp, PathVariableType.OidAccount);

            public const string WeaponOvercomeUp = "/Lobby/Item/WeaponOvercomeUp/{0}";
            public static readonly WWWAPI WeaponOvercomeUpAPI = new WWWAPI(WeaponOvercomeUp, PathVariableType.OidAccount);

            public const string WeaponReforgeUp = "/Lobby/Item/WeaponReforgeUp/{0}";
            public static readonly WWWAPI WeaponReforgeUpAPI = new WWWAPI(WeaponReforgeUp, PathVariableType.OidAccount);

            public const string EquipmentReset = "/Lobby/Item/EquipmentReset/{0}";
            public static readonly WWWAPI EquipmentResetAPI = new WWWAPI(EquipmentReset, PathVariableType.OidAccount);

            public const string EquipmentsDecompose = "/Lobby/Item/EquipmentsDecompose/{0}";
            public static readonly WWWAPI EquipmentsDecomposeAPI = new WWWAPI(EquipmentsDecompose, PathVariableType.OidAccount);

            public const string EquipmentToggleLock = "/Lobby/Item/EquipmentToggleLock/{0}";
            public static readonly WWWAPI EquipmentToggleLockAPI = new WWWAPI(EquipmentToggleLock, PathVariableType.OidAccount);

            public const string BoxUse = "/Lobby/Item/BoxUse/{0}";
            public static readonly WWWAPI BoxUseAPI = new WWWAPI(BoxUse, PathVariableType.OidAccount);

            public const string SelectableBoxUse = "/Lobby/Item/SelectableBoxUse/{0}";
            public static readonly WWWAPI SelectableBoxUseAPI = new WWWAPI(SelectableBoxUse, PathVariableType.OidAccount);

            public const string MoneyBoxUse = "/Lobby/Item/MoneyBoxUse/{0}";
            public static readonly WWWAPI MoneyBoxUseAPI = new WWWAPI(MoneyBoxUse, PathVariableType.OidAccount);

            public const string MaterialExchange = "/Lobby/Item/MaterialExchange/{0}";
            public static readonly WWWAPI MaterialExchangeAPI = new WWWAPI(MaterialExchange, PathVariableType.OidAccount);

            public const string EventExchange = "/Lobby/Item/EventExchange/{0}";
            public static readonly WWWAPI EventExchangeAPI = new WWWAPI(EventExchange, PathVariableType.OidAccount);
        }

        public static class Reward
        {
            public const string ReceiveTalentTimeReward = "/Lobby/Reward/ReceiveTalentTimeReward/{0}";
            public static readonly WWWAPI ReceiveTalentTimeRewardAPI = new WWWAPI(ReceiveTalentTimeReward, PathVariableType.OidAccount);

            public const string AddStaminaByTime = "/Lobby/Reward/AddStaminaByTime/{0}";
            public static readonly WWWAPI AddStaminaByTimeAPI = new WWWAPI(AddStaminaByTime, PathVariableType.OidAccount);

            public const string SetUseStamina = "/Lobby/Reward/SetUseStamina/{0}";
            public static readonly WWWAPI SetUseStaminaAPI = new WWWAPI(SetUseStamina, PathVariableType.OidAccount, requestPolicyType: RequestPolicyType.None);

            public const string ReceiveMissionsReward = "/Lobby/Reward/ReceiveMissionsReward/{0}";
            public static readonly WWWAPI ReceiveMissionsRewardAPI = new WWWAPI(ReceiveMissionsReward, PathVariableType.OidAccount);

            public const string CouponUse = "/Lobby/Reward/CouponUse/{0}";
            public static readonly WWWAPI CouponUseAPI = new WWWAPI(CouponUse, PathVariableType.OidAccount);

            public const string GachaOpen = "/Lobby/Reward/GachaOpen/{0}";
            public static readonly WWWAPI GachaOpenAPI = new WWWAPI(GachaOpen, PathVariableType.OidAccount);

            public const string GachaOpenNewbie = "/Lobby/Reward/GachaOpenNewbie/{0}";
            public static readonly WWWAPI GachaOpenNewbieAPI = new WWWAPI(GachaOpenNewbie, PathVariableType.OidAccount);
            
            public const string GachaOpenAds = "/Lobby/Reward/GachaOpenAds/{0}";
            public static readonly WWWAPI GachaOpenAdsAPI = new WWWAPI(GachaOpenAds, PathVariableType.OidAccount);

            public const string GetGachaHistory = "/Lobby/Reward/GetGachaHistory/{0}";
            public static readonly WWWAPI GetGachaHistoryAPI = new WWWAPI(GetGachaHistory, PathVariableType.OidAccount);

            public const string RewardAds = "/Lobby/Reward/RewardAds/{0}";
            public static readonly WWWAPI RewardAdsAPI = new WWWAPI(RewardAds, PathVariableType.OidAccount);

            public const string AccountLevelUp = "/Lobby/Reward/AccountLevelUp/{0}";
            public static readonly WWWAPI AccountLevelUpAPI = new WWWAPI(AccountLevelUp, PathVariableType.OidAccount);

            public const string ReceiveConquestSeasonReward = "/Lobby/Reward/ReceiveConquestSeasonReward/{0}";
            public static readonly WWWAPI ReceiveConquestSeasonRewardAPI = new WWWAPI(ReceiveConquestSeasonReward, PathVariableType.OidAccount);

            public const string ReceiveConquestTierReward = "/Lobby/Reward/ReceiveConquestTierReward/{0}";
            public static readonly WWWAPI ReceiveConquestTierRewardAPI = new WWWAPI(ReceiveConquestTierReward, PathVariableType.OidAccount);


            public const string ReceiveWorldMissionRankingReward = "/Lobby/Reward/ReceiveWorldMissionRankingReward/{0}";
            public static readonly WWWAPI ReceiveWorldMissionRankingRewardAPI = new WWWAPI(ReceiveWorldMissionRankingReward, PathVariableType.OidAccount);

            public const string ReceiveWorldMissionScoreReward = "/Lobby/Reward/ReceiveWorldMissionScoreReward/{0}";
            public static readonly WWWAPI ReceiveWorldMissionScoreRewardAPI = new WWWAPI(ReceiveWorldMissionScoreReward, PathVariableType.OidAccount);

            public const string ReceiveWorldMissionCoopReward = "/Lobby/Reward/ReceiveWorldMissionCoopReward/{0}";
            public static readonly WWWAPI ReceiveWorldMissionCoopRewardAPI = new WWWAPI(ReceiveWorldMissionCoopReward, PathVariableType.OidAccount);

            public const string ReceiveTimeResourceReward = "/Lobby/Reward/ReceiveTimeResourceReward/{0}";
            public static readonly WWWAPI ReceiveTimeResourceRewardAPI = new WWWAPI(ReceiveTimeResourceReward, PathVariableType.OidAccount);
        }
        public static class Shop
        {
            public const string List = "/Lobby/Shop/List/{0}";
            public static readonly WWWAPI ListAPI = new WWWAPI(List, PathVariableType.OidAccount);

            public const string CheckPurchaseCash = "/Lobby/Shop/CheckPurchaseCash/{0}";
            public static readonly WWWAPI CheckPurchaseCashAPI = new WWWAPI(CheckPurchaseCash, PathVariableType.OidAccount);

            public const string PurchaseCash = "/Lobby/Shop/PurchaseCash/{0}";
            public static readonly WWWAPI PurchaseCashAPI = new WWWAPI(PurchaseCash, PathVariableType.OidAccount);

            public const string Purchase = "/Lobby/Shop/Purchase/{0}";
            public static readonly WWWAPI PurchaseAPI = new WWWAPI(Purchase, PathVariableType.OidAccount);

            public const string PurchaseFree = "/Lobby/Shop/PurchaseFree/{0}";
            public static readonly WWWAPI PurchaseFreeAPI = new WWWAPI(PurchaseFree, PathVariableType.OidAccount);

            public const string PurchaseAds = "/Lobby/Shop/PurchaseAds/{0}";
            public static readonly WWWAPI PurchaseAdsAPI = new WWWAPI(PurchaseAds, PathVariableType.OidAccount);

            public const string TriggerPackageShow = "/Lobby/Shop/TriggerPackageShow/{0}";
            public static readonly WWWAPI TriggerPackageShowAPI = new WWWAPI(TriggerPackageShow, PathVariableType.OidAccount);

            public const string TriggerPackageClose = "/Lobby/Shop/TriggerPackageClose/{0}";
            public static readonly WWWAPI TriggerPackageCloseAPI = new WWWAPI(TriggerPackageClose, PathVariableType.OidAccount);
        }
        public static class Mail
        {
            public const string List = "/Lobby/Mail/List/{0}";
            public static readonly WWWAPI ListAPI = new WWWAPI(List, PathVariableType.OidAccount);

            public const string Read = "/Lobby/Mail/Read/{0}";
            public static readonly WWWAPI ReadAPI = new WWWAPI(Read, PathVariableType.OidAccount);

            public const string ReadReceive = "/Lobby/Mail/ReadReceive/{0}";
            public static readonly WWWAPI ReadReceiveAPI = new WWWAPI(ReadReceive, PathVariableType.OidAccount);

            public const string ReadReceiveAll = "/Lobby/Mail/ReadReceiveAll/{0}";
            public static readonly WWWAPI ReadReceiveAllAPI = new WWWAPI(ReadReceiveAll, PathVariableType.OidAccount);

            public const string HideAll = "/Lobby/Mail/HideAll/{0}";
            public static readonly WWWAPI HideAllAPI = new WWWAPI(HideAll, PathVariableType.OidAccount);
        }


        public static class SeasonPass
        {
            public const string Receive = "/Lobby/SeasonPass/Receive/{0}";
            public static readonly WWWAPI ReceiveAPI = new WWWAPI(Receive, PathVariableType.OidAccount);
        }

        public static class Conquest
        {
            public const string RequestConquestRankModelList = "/Lobby/Conquest/RequestConquestRankModelList/{0}";
            public static readonly WWWAPI RequestConquestRankModelListAPI = new WWWAPI(RequestConquestRankModelList, PathVariableType.OidAccount);

            public const string RequestConquestScoreList = "/Lobby/Conquest/RequestConquestScoreList/{0}";
            public static readonly WWWAPI RequestConquestScoreListAPI = new WWWAPI(RequestConquestScoreList, PathVariableType.OidAccount);

            public const string RequestConquestScoreInfo = "/Lobby/Conquest/RequestConquestScoreInfo/{0}";
            public static readonly WWWAPI RequestConquestScoreInfoAPI = new WWWAPI(RequestConquestScoreInfo, PathVariableType.OidAccount);

            public const string RequestConquestScoreRecordTop5 = "/Lobby/Conquest/RequestConquestScoreRecordTop5/{0}";
            public static readonly WWWAPI RequestConquestScoreRecordTop5API = new WWWAPI(RequestConquestScoreRecordTop5, PathVariableType.OidAccount);
        }
        public static class WorldMission
        { 
            public const string RequestWorldMissionRankModelList = "/Lobby/WorldMission/RequestWorldMissionRankModelList/{0}";
            public static readonly WWWAPI RequestWorldMissionRankModelListAPI = new WWWAPI(RequestWorldMissionRankModelList, PathVariableType.OidAccount);

            public const string RequestWorldMissionScoreList = "/Lobby/WorldMission/RequestWorldMissionScoreList/{0}";
            public static readonly WWWAPI RequestWorldMissionScoreListAPI = new WWWAPI(RequestWorldMissionScoreList, PathVariableType.OidAccount);

            public const string RequestWorldMissionScoreInfo = "/Lobby/WorldMission/RequestWorldMissionScoreInfo/{0}";
            public static readonly WWWAPI RequestWorldMissionScoreInfoAPI = new WWWAPI(RequestWorldMissionScoreInfo, PathVariableType.OidAccount);
        }
        public static class Sweep
        {
            public const string SweepEpisode = "/Lobby/Sweep/SweepEpisode/{0}";
            public static readonly WWWAPI SweepEpisodeAPI = new WWWAPI(SweepEpisode, PathVariableType.OidAccount);
        }

        public static class Event
        {
            public const string RewardStackEvent = "/Lobby/Event/RewardStackEvent/{0}";
            public static readonly WWWAPI RewardStackEventAPI = new WWWAPI(RewardStackEvent, PathVariableType.OidAccount);

            public const string EventGachaOpen = "/Lobby/Event/EventGachaOpen/{0}";
            public static readonly WWWAPI EventGachaOpenAPI = new WWWAPI(EventGachaOpen, PathVariableType.OidAccount);

            public const string RewardLotteryEvent = "/Lobby/Event/RewardLotteryEvent/{0}";
            public static readonly WWWAPI RewardLotteryEventAPI = new WWWAPI(RewardLotteryEvent, PathVariableType.OidAccount);

            public const string GetGameEvent = "/Lobby/Event/GetGameEvent/{0}";
            public static readonly WWWAPI GetGameEventAPI = new WWWAPI(GetGameEvent, PathVariableType.OidAccount);

            public const string ResetGameEvent = "/Lobby/Event/ResetGameEvent/{0}";
            public static readonly WWWAPI ResetGameEventAPI = new WWWAPI(ResetGameEvent, PathVariableType.OidAccount);
        }
    }

    public static class MatchAPIs
    {
        public static class Multiplay
        {
            public const string EnqueueMatch = "/Match/Multiplay/EnqueueMatch/{0}";
            public static readonly WWWAPI EnqueueMatchAPI = new WWWAPI(EnqueueMatch, PathVariableType.OidAccount);

            public const string DequeueMatch = "/Match/Multiplay/DequeueMatch/{0}";
            public static readonly WWWAPI DequeueMatchAPI = new WWWAPI(DequeueMatch, PathVariableType.OidAccount);

            public const string CheckMatch = "/Match/Multiplay/CheckMatch/{0}";
            public static readonly WWWAPI CheckMatchAPI = new WWWAPI(CheckMatch, PathVariableType.OidAccount);

            public const string CheckArena = "/Match/Multiplay/CheckArena/{0}";
            public static readonly WWWAPI CheckArenaAPI = new WWWAPI(CheckArena, PathVariableType.OidAccount);

            public const string ForgiveArena = "/Match/Multiplay/ForgiveArena/{0}";
            public static readonly WWWAPI ForgiveArenaAPI = new WWWAPI(ForgiveArena, PathVariableType.OidAccount);

            public const string CheckMatchAndGetSocialServer = "/Match/Multiplay/CheckMatchAndGetSocialServer/{0}";
            public static readonly WWWAPI CheckMatchAndGetSocialServerAPI = new WWWAPI(CheckMatchAndGetSocialServer, PathVariableType.OidAccount);
        }

        public static class Singleplay
        {
            public const string GetBattleServer = "/Match/Singleplay/GetBattleServer/{0}";
            public static readonly WWWAPI GetBattleServerAPI = new WWWAPI(GetBattleServer, PathVariableType.OidAccount);
        }

        public static class Party
        {
            public const string GetSocialServer = "/Match/Party/GetSocialServer/{0}";
            public static readonly WWWAPI GetSocialServerAPI =
                new WWWAPI(GetSocialServer, PathVariableType.OidAccount, requestPolicyType: RequestPolicyType.None);

            public const string GetPartyConnectInfo = "/Match/Party/GetPartyConnectInfo/{0}";
            public static readonly WWWAPI GetPartyConnectInfoAPI = new WWWAPI(GetPartyConnectInfo, PathVariableType.OidAccount);
        }
    }
    public static class MatchAdminAPIs
    {
        public const string GetDisabledServers = "/MatchAdmin/GetDisabledServers";
        public static readonly WWWAPI GetDisabledServersAPI = new WWWAPI(GetDisabledServers, PathVariableType.None);

        public const string SetDisabledServer = "/MatchAdmin/SetDisabledServer";
        public static readonly WWWAPI SetDisabledServerAPI = new WWWAPI(SetDisabledServer, PathVariableType.None);

        public const string BroadCastKickAll = "/MatchAdmin/BroadCastKickAll";
        public static readonly WWWAPI BroadCastKickAllAPI = new WWWAPI(BroadCastKickAll, PathVariableType.None);

        public const string KickAll = "/MatchAdmin/KickAll";
        public static readonly WWWAPI KickAllAPI = new WWWAPI(KickAll, PathVariableType.None);

        public const string MakeConquestRank = "/MatchAdmin/MakeConquestRank";
        public static readonly WWWAPI MakeConquestRankAPI = new WWWAPI(MakeConquestRank, PathVariableType.None);

        public const string MakeWorldMissionRank = "/MatchAdmin/MakeWorldMissionRank";
        public static readonly WWWAPI MakeWorldMissionRankAPI = new WWWAPI(MakeWorldMissionRank, PathVariableType.None);
    }
}