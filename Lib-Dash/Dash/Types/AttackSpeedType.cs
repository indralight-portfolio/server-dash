namespace Dash.Types
{
    public enum AttackSpeedType
    {
        Undefined = 0,
        ATKSPD_VS,
        ATKSPD_S,
        ATKSPD_N,
        ATKSPD_F,
        ATKSPD_VF,
    }


    public static class AttackSpeedTypeExtension
    {
        public static CodeLocale ToCodeLocaleType(this AttackSpeedType attackSpeedType)
        {
            switch (attackSpeedType)
            {
                case AttackSpeedType.ATKSPD_VS:
                    return CodeLocale.Service_UI_Equipment_AttackSpeed_ATKSPD_VS;
                case AttackSpeedType.ATKSPD_S:
                    return CodeLocale.Service_UI_Equipment_AttackSpeed_ATKSPD_S;
                case AttackSpeedType.ATKSPD_N:
                    return CodeLocale.Service_UI_Equipment_AttackSpeed_ATKSPD_N;
                case AttackSpeedType.ATKSPD_F:
                    return CodeLocale.Service_UI_Equipment_AttackSpeed_ATKSPD_F;
                case AttackSpeedType.ATKSPD_VF:
                    return CodeLocale.Service_UI_Equipment_AttackSpeed_ATKSPD_VF;
            }
            return CodeLocale.Service_UI_Equipment_AttackSpeed_ATKSPD_VS;
        }
    }
}
