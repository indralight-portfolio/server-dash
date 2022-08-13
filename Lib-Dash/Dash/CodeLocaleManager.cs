using Common.Locale;
using Common.Utility;
using System.Collections.Generic;

namespace Dash
{
    // LocaleKey 는 "System.{EnumValueName.Replace("_",".")}" 이다.
    // Service_InvalidVersion_Title => System.Service.InvalidVersion.Title
    // 알파벳 순으로 정렬
    public enum CodeLocale
    {
        Undefined,

        #region ResourceLocale
        BuiltIn_EntryPoint_UpdateBundle_Message,
        BuiltIn_EntryPoint_UpdateBundle_Title,
        BuiltIn_EntryPoint_UpdateVersion_Message,
        BuiltIn_EntryPoint_UpdateVersion_Title,
        BuiltIn_NativePopup_Confirm,

        BuiltIn_NativeRateUsPopup_Later,
        BuiltIn_NativeRateUsPopup_Message,
        BuiltIn_NativeRateUsPopup_No,
        BuiltIn_NativeRateUsPopup_Title,
        BuiltIn_NativeRateUsPopup_Yes,
        BuiltIn_UI_MessageBox_Title,
        BuiltIn_UI_MessageBox_Cancel,
        BuiltIn_UI_MessageBox_No,
        BuiltIn_UI_MessageBox_OK,
        BuiltIn_UI_MessageBox_Yes,
        BuiltIn_Service_InvalidVersion_Message,
        BuiltIn_Service_InvalidVersion_Title,
        BuiltIn_Service_NetworkError_Message,
        BuiltIn_Service_NetworkError_Title,
        BuiltIn_Service_NetworkError_Message_Retry,
        BuiltIn_Service_OutOfService_Message,
        BuiltIn_Service_OutOfService_Title,
        BuiltIn_Service_DuplicatedSession_Message,
        BuiltIn_Service_DuplicatedSession_Title,

        BuiltIn_Service_ServiceManager_LoginFail_Message,
        BuiltIn_Service_ServiceManager_LoginFail_Title,

        BuiltIn_Entry_UpdateBundle_Text,

        BuiltIn_Hive_GameCenterLogin_Canceled,

        BuiltIn_UI_Tip_1,
        BuiltIn_UI_Tip_2,
        BuiltIn_UI_Tip_3,
        BuiltIn_UI_Tip_4,
        BuiltIn_UI_Tip_5,
        BuiltIn_UI_Tip_6,
        BuiltIn_UI_Tip_7,
        BuiltIn_UI_Tip_8,
        BuiltIn_UI_Tip_9,
        BuiltIn_UI_Tip_10,
        #endregion

        Dash_Core_Option_AddGoldBonusOption,
        Dash_Types_NonItemType_Gold_Name,
        Dash_Types_NonItemType_Gold_Desc,
        Dash_Types_NonItemType_Jewel_Name,
        Dash_Types_NonItemType_Jewel_Desc,
        Dash_Types_NonItemType_Stamina_Name,
        Dash_Types_NonItemType_Stamina_Desc,
        Dash_Types_NonItemType_AccountExp_Name,
        Dash_Types_NonItemType_AccountExp_Desc,
        Dash_Types_NonItemType_CharacterExp_Name,
        Dash_Types_NonItemType_CharacterExp_Desc,
        Dash_Types_NonItemType_Mileage_Name,
        Dash_Types_NonItemType_Mileage_Desc,
        Dash_Types_NonItemType_GachaTicket_Name,
        Dash_Types_NonItemType_GachaTicket_Desc,
        Dash_Types_NonItemType_GachaLimitedTicket_Name,
        Dash_Types_NonItemType_GachaLimitedTicket_Desc,
        Dash_Types_NonItemType_SeasonPoint_Name,
        Dash_Types_NonItemType_SeasonPoint_Desc,
        Dash_Types_NonItemType_DailyQuestPoint_Name,
        Dash_Types_NonItemType_DailyQuestPoint_Desc,
        Dash_Types_NonItemType_WeeklyQuestPoint_Name,
        Dash_Types_NonItemType_WeeklyQuestPoint_Desc,
        Dash_Types_NonItemType_AchievementPoint_Name,
        Dash_Types_NonItemType_AchievementPoint_Desc,
        Dash_Types_NonItemType_ConquestPoint_Name,
        Dash_Types_NonItemType_ConquestPoint_Desc,
        Dash_Types_Achievement_Kill,
        Dash_Types_Achievement_Damage,
        Dash_Types_Achievement_UseUltkill,
        Dash_Types_Achievement_UseSkill,
        Dash_Types_Achievement_Attack,
        Dash_Types_Achievement_Shield,
        Dash_Types_Achievement_Barrier,
        Dash_Types_Achievement_Revive,
        Dash_Types_Achievement_Heal,
        Dash_Types_Achievement_Kill_Desc,
        Dash_Types_Achievement_Damage_Desc,
        Dash_Types_Achievement_Attack_Desc,
        Dash_Types_EtcAchievementType_MoveTile,
        Dash_Types_EtcAchievementType_MinDamage,
        Dash_Types_EtcAchievementType_UseBase,
        Dash_Types_ActionGroup_Base,
        Dash_Types_ActionGroup_Skill,
        Dash_Types_ActionGroup_UltSkill,
        Dash_Types_BuyLimitType_Daily,
        Dash_Types_BuyLimitType_Monthly,
        Dash_Types_BuyLimitType_Periodically,
        Dash_Types_BuyLimitType_Permanently,
        Dash_Types_BuyLimitType_Weekly,

        Dash_Types_DeckType_Duo,
        Dash_Types_DeckType_Trio,
        Dash_Types_DeckType_Multi,
        Dash_Types_DeckType_PathOfGlory,

        Dash_Types_ElementalType_Physical,
        Dash_Types_ElementalType_Fire,
        Dash_Types_ElementalType_Air,
        Dash_Types_ElementalType_Earth,
        Dash_Types_ElementalType_Water,
        Dash_Types_ElementalType_Lightning,

        Dash_Types_EquipmentSlotType_Armor1,
        Dash_Types_EquipmentSlotType_Armor2,
        Dash_Types_EquipmentSlotType_Armor3,
        Dash_Types_EquipmentSlotType_Armor4,


        Dash_Types_EquipmentSlotType_Weapon,

        Dash_Types_WeaponType_Staff,
        Dash_Types_WeaponType_Sword,
        Dash_Types_WeaponType_Pistol,


        Dash_Types_WeaponType_Bow,
        Dash_Types_WeaponType_TwoHandSword,

        Dash_Types_ItemType_Armor,
        Dash_Types_ItemType_Character,
        Dash_Types_ItemType_CharacterSoul,
        Dash_Types_ItemType_Material,
        Dash_Types_ItemType_EventCoin,
        Dash_Types_ItemType_Weapon,
        Dash_Types_ItemType_RewardBox,
        Dash_Types_ItemType_MoneyBox,
        Dash_Types_ItemType_Ticket,

        Dash_Types_TicketType_SweepEpisode,

        Dash_Types_Rarity_Common,
        Dash_Types_Rarity_Epic,
        Dash_Types_Rarity_Legendary,
        Dash_Types_Rarity_Rare,
        Dash_Types_JobType_ADCARRY,
        Dash_Types_JobType_FIGHTER,
        Dash_Types_JobType_SUPPORTER,

        Dash_Types_SortingOption_Rarity,
        Dash_Types_SortingOption_Level,
        Dash_Types_SortingOption_ArmorSetId,

        Dash_StaticData_EpisodeMission_Clear,
        Dash_StaticData_EpisodeMission_ClearTime,
        Dash_StaticData_EpisodeMission_DeadCount,
        Dash_StaticData_EpisodeMission_PlayerOnly,
        Dash_StaticData_EpisodeMission_BeHitCount,
        Dash_StaticData_EpisodeMission_HpAbove,
        Dash_StaticData_EpisodeMission_UseSkillCountAbove,
        Dash_StaticData_EpisodeMission_UseSkillCountBelow,
        Dash_StaticData_EpisodeMission_CharacterRankAbove,
        Dash_StaticData_EpisodeMission_WithCharacterFire,
        Dash_StaticData_EpisodeMission_WithCharacterAir,
        Dash_StaticData_EpisodeMission_WithCharacterEarth,
        Dash_StaticData_EpisodeMission_WithCharacterWater,
        Dash_StaticData_EpisodeMission_WithCharacterLightning,
        Dash_StaticData_EpisodeMission_WithWeaponStaff,
        Dash_StaticData_EpisodeMission_WithWeaponSword,
        Dash_StaticData_EpisodeMission_WithWeaponPistol,
        Dash_StaticData_EpisodeMission_WithWeaponTwoHandSword,
        Dash_StaticData_EpisodeMission_WithWeaponSpear,
        Dash_StaticData_EpisodeMission_WithWeaponBow,
        Dash_StaticData_EpisodeMission_WithoutRarityCommon,
        Dash_StaticData_EpisodeMission_WithoutRarityRare,
        Dash_StaticData_EpisodeMission_WithoutRarityEpic,
        Dash_StaticData_EpisodeMission_WithoutRarityLegendary,

        Dash_StaticData_EpisodeMission_Clear_Current,
        Dash_StaticData_EpisodeMission_ClearTime_Current,
        Dash_StaticData_EpisodeMission_DeadCount_Current,
        Dash_StaticData_EpisodeMission_BeHitCount_Current,
        Dash_StaticData_EpisodeMission_HpAbove_Current,
        Dash_StaticData_EpisodeMission_UseSkillCountAbove_Current,
        Dash_StaticData_EpisodeMission_UseSkillCountBelow_Current,

        Extensions_TimeSpan_Day,
        Extensions_TimeSpan_Hour,
        Extensions_TimeSpan_Minute,
        Extensions_TimeSpan_Second,
        GamePlay_UI_ChangeDeck,
        GamePlay_UI_GameResultPopup_PlayTime,
        GamePlay_UI_GameResultPopup_NotEnoughStamina,
        GamePlay_UI_MVPPopup_Achievement_Count,
        GamePlay_UI_MissionPanel_Title,
        GamePlay_UI_PathOfGlory_MissionPanel_Title,
        GamePlay_UI_Tower_MissionPanel_Title,
        GamePlay_UI_InGame_DamageFont_Evade,
        GamePlay_UI_InGame_DamageFont_HeadShot,
        GamePlay_UI_InGame_DamageFont_HpBoost_Title,
        GamePlay_UI_InGame_DamageFont_LevelUp,
        GamePlay_UI_PausePanel_Exit_Message,
        GamePlay_UI_PausePanel_Exit_Title,
        GamePlay_UI_Revive_LackOfJewel,
        GamePlay_UI_Resume_Game_Soon_Message,
        MailService_Mail_ExcessItem_Title,
        MailService_Mail_ExcessItem_Message,
        MailService_Mail_Attendance_Title,
        MailService_Mail_AttendanceContinuous_Title,
        MailService_Mail_DailyReward_Title,
        MailService_Mail_DailyReward_Message,
        MailService_Mail_ProductBuy_Title,
        MailService_Mail_ProductBuy_Message,
        MailService_Mail_PreRegist_Title,
        MailService_Mail_PreRegist_Message,

        Service_ExpireSession_Message,

        Service_Battle_StartUndoneGame_Fail_Message,
        Service_Battle_StartUndoneGame_Fail_Title,
        Service_CountdownRevive_Cancel_Message,
        Service_Event_End,
        Service_Event_NotExist,
        Service_Event_RemainTime_Close,
        Service_GachaProbability_Title,
        Service_GachaProbability_CommonGachaDesc,
        Service_GachaProbability_Desc,
        Service_GachaProbability_LimitedGachaDesc,
        Service_Multiplay_JoinArena_ArenaEnded_Message,
        Service_Multiplay_JoinArena_Fail_Title,
        Service_PartyController_Join_Message,
        Service_PartyController_Enter_Message,
        Service_PartyController_Kick_Message,
        Service_PartyController_Kick_AliveStatus_Message,
        Service_PartyController_Leave_Message,
        Service_PartyController_MatchSuccess,
        Service_Reward_OverLimit,
        Service_Reward_SendMailExcessItem,
        Service_SeasonPass_LevelInfo,
        Service_SeasonPass_NotExist,
        Service_SeasonPass_Refresh,
        Service_Season_RemainTime_Close,
        Service_Season_RemainTime_End,
        Service_Season_Reward_RemainTime,
        Service_Season_Reward_Not_InPeriod,
        Service_RewardController_Coupon_AlreadyProcessed,
        Service_RewardController_Coupon_Expired,
        Service_RewardController_Coupon_NoData,
        Service_RewardController_Coupon_NotEnough,
        Service_StartUndoneGame_Message,
        Service_StartUndoneGame_Title,

        Service_UI_LackOfCostPopup_Title,
        Service_UI_LimitTime_Text,
        Service_UI_LackOfCostPopup_Message,
        Service_UI_MoneyExchange_Title,
        Service_UI_MoneyExchange_Message,

        Service_UI_ActionCooldown,
        Service_UI_ArmorGrowthLisView_Empty,
        Service_UI_Button_Gachax1,
        Service_UI_Button_Gachax10,
        Service_UI_CharacterTrial,
        Service_UI_CollectionInfo_WeaponGet,
        Service_UI_CollectionInfo_WeaponRank,
        Service_UI_CollectionInfo_CharacterGet,
        Service_UI_CollectionInfo_CharacterRank,
        Service_UI_CollectionInfo_BonusStat,
        Service_UI_ConsumeItem_Count,
        Service_UI_Common_Level,
        Service_UI_Common_Random,
        Service_UI_DailyRewardConfirmPopup_DayDesc,
        Service_UI_DecomposeConfirmPopup,
        Service_UI_Episode_World,
        Service_UI_Episode_World_Hard,
        Service_UI_Episode_Normal,
        Service_UI_Episode_Elite,
        Service_UI_Episode_Boss,
        Service_UI_Episode_LimitTime,
        Service_UI_EpisodeGroupType_World,
        Service_UI_EpisodeGroupType_Raid,
        Service_UI_EpisodeGroupType_Raid_BossLevel,
        Service_UI_EpisodeGroupDifficultyType_Hard,
        Service_UI_EpisodeGroupDifficultyType_Normal,
        Service_UI_EpisodeGroupDifficultyType_VeryHard,
        Service_UI_Episode_RecommendLevel,
        Service_UI_EpisodeGroupType_Dungeon,
        Service_UI_EpisodeGroupType_Conquest,
        Service_UI_EpisodeGroupType_PathOfGlory,
        Service_UI_EpisodeGroup_UnLockCondition,
        Service_UI_EpisodeInfo_UseStamina_Name,
        Service_UI_EpisodeInfo_UseStamina_Desc,
        Service_UI_EpisodeInfo_Title_Tutorial,
        Service_UI_EpisodeInfo_PathOfGlory_Tooltip,
        Service_UI_Episode_Dungeon_RewardType_Armor,

        Service_UI_Episode_NotInPeriod,
        Service_UI_Episode_NotInPeriod_Conquest,
        Service_UI_Episode_Result_NotUseStamina,

        Service_UI_Equipment_AttackSpeed_ATKSPD_VS,
        Service_UI_Equipment_AttackSpeed_ATKSPD_S,
        Service_UI_Equipment_AttackSpeed_ATKSPD_N,
        Service_UI_Equipment_AttackSpeed_ATKSPD_F,
        Service_UI_Equipment_AttackSpeed_ATKSPD_VF,

        Service_UI_Event_Attendance_Days,
        Service_UI_Event_LotteryBoard_Complete,
        Service_UI_Event_LotteryBoard_Reset,
        Service_UI_Event_RemainCount,
        Service_UI_ExchangeEvent_RemainCount,
        Service_UI_Event_Attendance_ContinuousDays,
        Service_UI_Event_Attendance2_ContinuousDays,
        Service_UI_Friend_Notice_AcceptFriend,
        Service_UI_Friend_Notice_MemberOnline,
        Service_UI_Friend_AlreadyJoinParty,
        Service_UI_Friend_Notice_WaitingFriendCountMax_Player, //(요청자에의해 친구 요청 실패)친구 한도에 도달하여 신청을 보낼 수 없습니다.
        Service_UI_Friend_Notice_WaitingFriendCountMax_OtherPlayer, //친구 한도에 도달하여 신청이 불가한 플레이어입니다.
        Service_UI_Friend_Notice_FriendCountMax_Player, //친구 한도에 도달하여 수락이 불가합니다.
        Service_UI_Friend_Notice_FriendCountMax_OtherPlayer,//친구 한도에 도달하여 수락이 불가한 플레이어입니다.
        Service_UI_Friend_RequestFriend,
        Service_UI_Friend_ReceiveFriend,
        Service_UI_Friend_InviteParty,
        Service_UI_Friend_Receive_InviteParty_Count,
        Service_UI_Friend_RemoveFriendPopup_Title,
        Service_UI_Friend_RemoveFriendPopup_Desc,
        Service_UI_Friend_ReceivedList_Count,
        Service_UI_Friend_MyRequestList_Count,
        Service_UI_Friend_MessageBox_Invite_PartyFull_Title,
        Service_UI_Friend_MessageBox_Invite_PartyFull_Desc,
        Service_UI_Friend_EpisodeNumber,
        Service_UI_Gacha_Date_Ago,
        Service_UI_Gacha_Date_JustNow,

        Service_UI_InviteParty_InviteMessage,
        Service_UI_Inventory_No_Items,
        Service_UI_Inventory_Category_Weapon,
        Service_UI_Inventory_Category_Armor,
        Service_UI_Inventory_Category_Consume,

        Service_UI_Mail_Date_Ago,
        Service_UI_Mail_Date_Expire,
        Service_UI_Mail_Date_Expired,
        Service_UI_Mail_Date_JustNow,
        Service_UI_Match_TimoutOut_Message,
        Service_UI_Match_TimoutOut_Title,
        Service_UI_MaterialExchangePopup_Desc,
        Service_UI_MaxLevel,

        Service_UI_NicknamePanel_Sharp,
        Service_UI_NicknamePanel_BadWord,
        Service_UI_NicknamePanel_Desc,
        Service_UI_Option_CopyClipboard,
        Service_UI_Shop_Confirm_BuyLimit,
        Service_UI_Shop_OverBuyLimit,
        Service_UI_Party_Code,
        Service_UI_Party_Chat_Preview,
        Service_UI_PartyPlayPopup_AlreadyPlaying,
        Service_UI_PartyPlayPopup_InvalidKey,
        Service_UI_PartyPlayPopup_NoSeat,
        Service_UI_PartyPlayPopup_NotFoundRoom,
        Service_UI_PartyPlayPopup_NoLicense,
        Service_UI_PartyPlayPopup_OverLimit,
        Service_UI_Party_ChangeInviteAuthority_Title,
        Service_UI_Party_ChangeInviteAuthority_Message,
        Service_UI_Party_ChangePrivate_Title,
        Service_UI_Party_ChangePrivate_Message,
        Service_UI_Party_ChangePublic_Title,
        Service_UI_Party_ChangePublic_Message,
        Service_UI_Party_ChatBlock,
        Service_UI_Party_NoInviteAuthority,
        Service_UI_Party_Recruit_FindOtherParty,
        Service_UI_Party_Recruit_JoinOtherParty,

        Service_UI_Party_Recruit_Message,
        Service_UI_Party_Recruit_SendMessage,
        Service_UI_Party_Recruit_UnLockCondition,
        Service_UI_ProductPrice_Free,
        Service_UI_RemainTime_Open,
        Service_UI_RemainTime_DailyReward,
        Service_UI_ResetEpisodeEntryLimitConfirm_Message,
        Service_UI_Shop_DailyReward_MaxDaysDesc,
        Service_UI_Shop_IsNotInitialized,
        Service_UI_Shop_StepUp_Condition,
        Service_UI_Shop_IAP_Not_Supported,
        Service_UI_RemainTime_Close,
        Service_UI_RemainTime_Close_PackageConfirm,
        Service_UI_RemainTime_Entry_Close,
        Service_UI_ResetTime,
        Service_UI_Party_ChangePublic_Failed_MaxMember,
        Service_UI_SearchPlayerResult_Sucess,
        Service_UI_SeasonPass_PointClearDesc,
        Service_UI_SeasonPass_PointDesc,
        Service_UI_ReturnToLobby_Message,
        Service_UI_ReturnToLobby_Title,

        Service_UI_RuneOpen_Enable,
        Service_UI_RuneOpen_Disable,
        Service_UI_Setting_Coupon_InputMessage,
        Service_UI_Setting_Hive_Connect_Failed,
        Service_UI_Setting_Hive_Disconnect_Failed,
        Service_UI_Setting_Hive_Disconnect_Last_Idp,
        Service_UI_Setting_Hive_Disconnect_Last_Idp_Result,
        Service_UI_Setting_Hive_SignOut_Confirm,
        Service_UI_Setting_Hive_SignOut_Failed,
        Service_UI_Setting_Hive_SignOut_Complete,
        Service_UI_Setting_Hive_Notice_Agree,
        Service_UI_Setting_Hive_Notice_Disagree,
        Service_UI_Setting_Hive_Night_Agree,
        Service_UI_Setting_Hive_Night_Disagree,
        Service_UI_Setting_Reset_Confirm1,
        Service_UI_Setting_Reset_Confirm2,
        Service_UI_Setting_Reset_Complete,
        Service_UI_Setting_Hive_Delete_Confirm,
        Service_UI_Setting_Hive_Delete_Confirm2,
        Service_UI_Setting_Hive_Delete_Complete,
        Service_UI_Setting_Hive_Delete_Failed,

        Service_UI_SweepEpisode_NotClearEpisode,
        Service_UI_SweepEpisode_NotEnoughStar,
        Service_UI_SweepEpisode_ConfirmStart,
        Service_UI_SweepEpisode_RewardTitle,
        Service_UI_Tower_Floor,
        Service_UI_TimeResourceReward_GoldGainMinute,
        Service_UI_TimeResourceRewardQuick_Desc,
        Service_UI_Reward_Name_Count,
        Service_UI_Shop_OnPurchase,
        Service_UI_Shop_OnPurchase_SendMail,
        Service_UI_Shop_Purchasing,
        Service_UI_Shop_Recommend_ProductNotExist,
        Service_UI_WeaponGrowthLisView_Empty,
        Service_UI_WeaponGrowthReforgeLisView_Empty,
        Service_UI_WeaponReforgeValue,
        Service_UI_WeaponOvercomeUp_Tab,
        Service_UI_OvercomeUp_LackOfLevel,
        Service_UI_WeaponLevelUp_Tab,
        Service_UI_WeaponReforgeUp_Tab,
        Service_UI_WeaponGrowth_MaxLevel_MaxOvercome,
        Service_UI_WeaponGrowth_NotExist_ReforgeInfo,
        Service_UI_Weapon_Equipped,
        Service_UI_WeaponLevelUp_OverExp,
        Service_UI_Growth_LackOfMaterial,
        Service_UI_WeaponRankUp_MaxRank,

        Service_UI_WeaponUnlock_Title,
        Service_UI_WeaponUnlock_Desc,

        Service_UnknownError_Message,
        Service_UnknownError_Title,

        Service_UI_LockScreen_Ads,
        Service_UI_Ads_FailedToLoad,
        Service_UI_Ads_FailedToShow,
        Service_UI_ShowAds,
        Service_UI_ShowDailyQuestAds,
        Service_UI_Shortcut,

        Service_LocalPush_NotConnect_Title,
        Service_LocalPush_NotConnect_Message,
        Service_LocalPush_StaminaFull_Title,
        Service_LocalPush_StaminaFull_Message,
        Service_LocalPush_RaidOpen_Title,
        Service_LocalPush_RaidOpen_Message,

        UI_ConnectingServer,
        UI_InfoPopup_SelectableRewardBox_ItemList,
        UI_ApplicationQuit_Title,
        UI_ApplicationQuit_Message,
        UI_RemainTimer_Remain,
        UI_RemainTimer_End,

        UI_MessageBox_TowerEpInfoPopup_EmptySlot,
        UI_MessageBox_EpisodeInfoPopup_EmptySlot,
        UI_MessageBox_CheckBot_Fail,
        UI_MessageBox_SaveAndClose,

        /// <summary>
        ///  Admin 에서 사용
        /// </summary>
        MailService_Mail_HiveItem_Title,
        MailService_Mail_HiveItem_Message,
        MailService_Mail_Maintenance_Message,
        MailService_Mail_Maintenance_Title,
        MailService_Mail_Maintenance2_Message,
        MailService_Mail_Maintenance2_Title,
        MailService_Mail_Reward_Message,
        MailService_Mail_Reward_Title,
        MailService_Mail_Compensation_Message,
        MailService_Mail_Compensation_Title,

        #region Not Use
        /// <summary>
        /// 자체 서비스 시에 사용
        /// </summary>
        //NativePopup_No,//
        //NativePopup_Yes,//
        //Service_ServiceManager_Social_LoadExistsPopup_Message,//
        //Service_ServiceManager_Social_LoadExistsPopup_Title,//
        //Service_ServiceManager_Social_OverwriteExistsPopup_Message,//
        //Service_ServiceManager_Social_OverwriteExistsPopup_Title,//

        /// 미사용
        //Dash_Types_ActionGroup_Passive,//
        //Dash_Types_EquipmentSlotType_Armor5,//
        //Dash_Types_WeaponType_None,//        

        //Service_UI_AddStamina_Ads_NotReady,//
        //Service_UI_ArmorLevelUp_UnlockStatAdd,//

        //Service_UI_Episode_Dungeon_RemainCount,//
        //Service_UI_Episode_DungeonElement_RemainCount,//

        //Service_UI_Event_RemainTime,//

        //Service_UI_Gacha_Normal, //
        //Service_UI_Gacha_Limited,//

        //Service_UI_Reward_Count,//


        //Service_UI_RuneState_UnEquippedText,//
        //Service_UI_RuneState_Locked,//

        //Service_UI_Shop_BoxHeader,//
        //Service_UI_Shop_BoxOpen_OpenMore,//
        //Service_UI_Shop_BoxOpen_SaleRemainTime,//
        //Service_UI_Shop_BoxProbability_Header_NoneItem,//
        //Service_UI_Shop_BoxProbability_Header_Grade,//
        //Service_UI_Shop_BoxProbability_Header_Item,//
        //Service_UI_Shop_GoldHeader,//
        //Service_UI_Shop_JewelHeader,//

        //Service_UI_WeaponReforgeChangeValue,//
        //Service_UI_WeaponReforgeUp_Step,//

        //Service_UI_WeaponLevelUp_MaxLevel,//

        //Service_UI_WeaponRankUp_ActionGroupCanUse,//
        //Service_UI_WeaponRankUp_ActionGroupAdded,//
        //Service_UI_WeaponRankUp_LackOfGold,//
        //Service_UI_WeaponRankUp_LackOfLevel,//
        //Service_UI_WeaponRankUp_CanUseRune,//

        //Service_UI_WeaponReset_ConfirmTitle,//
        //Service_UI_WeaponReset_ConfirmMessage,//
        //Service_UI_WeaponReset_LackOfJewel,//
        //Service_UI_WeaponReset_MinRank,//
        //Stat_CalcType_Duration,//
        //Stat_CalcType_Tick,//
        //Stat_CalcType_Cooldown_AttackCount,//
        #endregion

    }
    public static class CodeLocaleManager
    {
        public static IReadOnlyDictionary<CodeLocale, string> Keys => _keys;

        static CodeLocaleManager()
        {
            foreach (var type in EnumInfo<CodeLocale>.GetValues())
            {
                string key = "Code." + type.ToString();
                _keys.Add(type, key);
            }
        }

        private static Dictionary<CodeLocale, string> _keys = new Dictionary<CodeLocale, string>();

        public static string GetKey(this CodeLocale type)
        {
            return _keys[type];
        }
        public static Locale GetLocale(this CodeLocale type)
        {
            return _keys[type];
        }

        public static string GetValue(this CodeLocale type, params object[] args)
        {
            string key = _keys[type];
            string value = LocaleManager.Instance.Get(key, true, args);
            return value;
        }
    }

    public interface IHasTypeLocale
    {
        // Parameter가 필요한 로케일 처리가 까다로워서 Locale 말고 string으로 return.
        string GetTypeLocale();
    }
}