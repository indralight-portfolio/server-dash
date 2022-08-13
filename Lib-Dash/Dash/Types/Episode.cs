using System.ComponentModel;

namespace Dash.Types
{
    public enum EpisodeGroupType
    {
        Undefined = 0,
        [Description("����")]
        World,
        [Description("���̵�")]
        Raid,
        [Description("����")]
        Dungeon,
        [Description("���")]
        Conquest,

        [Description("������ ��")]
        PathOfGlory,
        [Description("�÷��� ž")]
        Tower,
        [Description("���� �̼�")]
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