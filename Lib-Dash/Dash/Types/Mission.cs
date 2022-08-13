namespace Dash.Types
{
    public enum MissionType
    {
        Undefined = 0,
        BattleTutorial,
        LobbyTutorial,
        EpisodeGroupStar,
        Quest,
        DailyQuest,
        DailyPointRewardQuest,
        WeeklyQeust,
        WeeklyPointRewardQuest,
        Achievement,
        AchievementPointRewardQuest,

        StackPointEvent,
        EventDailyQuest,
        EventAchievement,
        EventEpisodeClear,
        CharacterStory,
    }
    public enum BattleTutorialType
    {
        Undefined = 0,
        SingleTutorial,
        MultiTutorial,
        AIMultiTutorial
    }

    public enum LobbyTutorialType
    {
        Undefined = 0,
        WorldEp_Clear_1_1,
        WorldEp_Clear_1_2,
        WorldEp_Clear_1_3,
        WorldEp_Clear_1_4,
        WorldEp_Clear_1_5,
        WorldEp_Clear_1_6,
        WorldEp_Clear_1_7,
        WorldEp_Clear_1_8,
        WorldEp_Clear_1_9,
        WorldEp_Clear_1_10,
        WorldEp_Clear_2_1,
        WorldEp_Clear_2_2,
        WorldEp_Clear_2_3,
        DungeonEp_Enter,
        RaidEp_Enter,
        ConquestEp_Enter,

        DeckEquip_Duo_AI1,
        DeckEquip_Duo_Player,
        DeckEquip_Trio_Player,
        DeckEquip_Trio_AI1,
        DeckEquip_Trio_AI2,

        CharacterLevelUp,
        CharacterLevelUp_Max,
        CharacterOvercomeUp,
        CharacterRankUp,

        WeaponEquip,
        WeaponLevelUp,
        WeaponLevelUp_Max,
        WeaponOvercomeUp,

        ArmorEquip,
        ArmorLevelUp,

        InventoryArmorDecompose,

        RuneTierOpen,
        RuneEquip,

        GachaOpen_10,
        GachaOpen_Newbie,


        QuestMainComplete,

        HiveReview,
        HiveConnectGuide,
    }

    public enum AchievementCateogryType
    {
        Undefined = 0,
        Character,
        Item,
    }
}
