namespace Dash.Types
{
    public enum AchievementType
    {
        Undefined = 0,
        Kill,
        Damage,
        UseUltSkill,
        UseSkill,

        Attack,
        Shield,
        Barrier,

        Revive,
        Heal,
    }
    public enum EtcAchievementType
    {
        Undefined = 0,
        MoveTile,
        MinDamage,
        UseBase,
    }

    public static class AchievementTypeExtension
    {
        public static CodeLocale ToCodeLocaleType(this AchievementType achievementType)
        {
            switch (achievementType)
            {
                case AchievementType.Kill:
                    return CodeLocale.Dash_Types_Achievement_Kill;
                case AchievementType.Damage:
                    return CodeLocale.Dash_Types_Achievement_Damage;
                case AchievementType.UseUltSkill:
                    return CodeLocale.Dash_Types_Achievement_UseUltkill;
                case AchievementType.UseSkill:
                    return CodeLocale.Dash_Types_Achievement_UseSkill;
                case AchievementType.Attack:
                    return CodeLocale.Dash_Types_Achievement_Attack;
                case AchievementType.Shield:
                    return CodeLocale.Dash_Types_Achievement_Shield;
                case AchievementType.Barrier:
                    return CodeLocale.Dash_Types_Achievement_Barrier;
                case AchievementType.Revive:
                    return CodeLocale.Dash_Types_Achievement_Revive;
                case AchievementType.Heal:
                    return CodeLocale.Dash_Types_Achievement_Heal;
                default:
                    return CodeLocale.Undefined;
            }
        }
        public static CodeLocale ToCodeLocaleType(this EtcAchievementType secondAchievementType)
        {
            switch (secondAchievementType)
            {
                case EtcAchievementType.MoveTile:
                    return CodeLocale.Dash_Types_EtcAchievementType_MoveTile;
                case EtcAchievementType.MinDamage:
                    return CodeLocale.Dash_Types_EtcAchievementType_MinDamage;
                case EtcAchievementType.UseBase:
                    return CodeLocale.Dash_Types_EtcAchievementType_UseBase;
                default:
                    return CodeLocale.Undefined;
            }
        }
    }
}