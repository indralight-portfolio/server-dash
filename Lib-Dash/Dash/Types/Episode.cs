using System.ComponentModel;

namespace Dash.Types
{
    public enum EpisodeGroupType
    {
        Undefined = 0,
        [Description("월드")]
        World,
        [Description("레이드")]
        Raid,
        [Description("던전")]
        Dungeon,
        [Description("토벌")]
        Conquest,

        [Description("영광의 길")]
        PathOfGlory,
        [Description("시련의 탑")]
        Tower,
        [Description("월드 미션")]
        WorldMission,
        Tutorial,
    }

    public enum EpisodeDifficulty
    {
        Undefined = 0,
        Normal,
        Hard,
        VeryHard,
    }
    public enum EpisodeType
    {
        Undefined = 0,
        Tutorial,
        Normal,
        Elite,
        Boss,
        Editor,
        CharacterTrial,
    }

    public static class EpisodeTypeHelper
    {
        public static CodeLocale GetLocale(this EpisodeGroupType groupType)
        {
            switch (groupType)
            {
                case EpisodeGroupType.World:
                    return CodeLocale.Service_UI_EpisodeGroupType_World;
                case EpisodeGroupType.Raid:
                    return CodeLocale.Service_UI_EpisodeGroupType_Raid;
                case EpisodeGroupType.Dungeon:
                    return CodeLocale.Service_UI_EpisodeGroupType_Dungeon;
                case EpisodeGroupType.Conquest:
                    return CodeLocale.Service_UI_EpisodeGroupType_Conquest;
                case EpisodeGroupType.PathOfGlory:
                    return CodeLocale.Service_UI_EpisodeGroupType_PathOfGlory;
                default:
                    return CodeLocale.Undefined;
            }
        }

        public static CodeLocale GetLocale(this EpisodeDifficulty difficulty)
        {
            switch (difficulty)
            {
                case EpisodeDifficulty.Hard:
                    return CodeLocale.Service_UI_EpisodeGroupDifficultyType_Hard;
                case EpisodeDifficulty.VeryHard:
                    return CodeLocale.Service_UI_EpisodeGroupDifficultyType_VeryHard;
                case EpisodeDifficulty.Normal:
                default:
                    return CodeLocale.Service_UI_EpisodeGroupDifficultyType_Normal;
            }
        }
    }
}