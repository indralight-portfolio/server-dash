using Common.Locale;
using Common.StaticInfo;
using Common.Utility;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Dash.Types
{
    /// <summary>
    /// Int Status 일시 StatusComponent에 추가해주어야 함.
    /// 크리쳐에게 영향을 주는 타입이 추가된 경우 CreatureStatusTypes에도 추가해줘야함
    /// </summary>
    public enum StatusType
    {
        Undefined = 0,
        [Comment("체력(툴에서 사용X)")]
        Hp,
        [Comment("최대 체력")]
        MaxHp,
        [Comment("쉴드 총량")]
        Shield,
        [Comment("공격 속도")]
        AttackSpeed,
        [Comment("이동 속도")]
        MoveSpeed,
        [Comment("공격력")]
        AttackPower,
        [Comment("흡혈 배율")]
        BloodSucking,
        [Comment("크리티컬 확률")]
        CriticalProbability,
        [Comment("크리티컬 저항 확률")]
        CriticalResistanceProbability,
        [Comment("크리티컬 대미지 배율")]
        CriticalDamageRate,
        [Comment("데미지 수치")]
        DamageValue,
        [Comment("주는 힐량 [" + nameof(HealValue) + "]")]
        HealValue,
        [Comment("주는 쉴드량")]
        ShieldValue,
        [Comment("주는 쉴드량 배율")]
        ShieldValueRate,
        [Comment("주는 쉴드 지속 턴 [" + nameof(ShieldTurnCount) + "]")]
        ShieldTurnCount,
        [Comment("회피 확률")]
        EvasionProbability,
        [Comment("헤드샷 확률")]
        HeadShotProbability,
        [Comment("주는 힐량비율 증가(기본값0)")]
        HealEffect,
        [Comment("받는 힐량 증가")]
        HpRecovery,
        [Comment("방어력")]
        Armor,
        [Comment("대미지 감소")]
        DamageResistance,
        [Comment("최종 대미지 감소 비율")]
        DamageResistanceRate,
        [Comment("힐 아이템 추가 힐량")]
        HealItemHealBonus,
        [Comment("레벨업시 추가 힐량")]
        LevelUpHealBonus,
        [Comment("그로기 체력")]
        GroggyPoint,
        [Comment("그로기 최대 체력")]
        MaxGroggyPoint,
        [Comment("그로기로 받는 데미지 비율")]
        GroggyReceiveDamageRate,
        [Comment("그로기 체력 데미지 수치")]
        GroggyPointDamageValue,
        [Comment("[CC][" + nameof(CCType.Slow) + "] 저항")]
        CC_Slow_Resistance,
        [Comment("[CC][" + nameof(CCType.Root) + "] 저항")]
        CC_Root_Resistance,
        [Comment("[CC][" + nameof(CCType.Stun) + "] 저항")]
        CC_Stun_Resistance,
        [Comment("[CC][" + nameof(CCType.Taunt) + "] 저항")]
        CC_Taunt_Resistance,

        [Comment("[Elemental][" + nameof(ElementalType.Physical) + "] 강화")]
        Elemental_Physical,
        [Comment("[Elemental][" + nameof(ElementalType.Physical) + "] 저항")]
        Elemental_Physical_Resistance,
        [Comment("[Elemental][" + nameof(ElementalType.Fire) + "] 강화")]
        Elemental_Fire,
        [Comment("[Elemental][" + nameof(ElementalType.Fire) + "] 저항")]
        Elemental_Fire_Resistance,
        [Comment("[Elemental][" + nameof(ElementalType.Air) + "] 강화")]
        Elemental_Air,
        [Comment("[Elemental][" + nameof(ElementalType.Air) + "] 저항")]
        Elemental_Air_Resistance,
        [Comment("[Elemental][" + nameof(ElementalType.Earth) + "] 강화")]
        Elemental_Earth,
        [Comment("[Elemental][" + nameof(ElementalType.Earth) + "] 저항")]
        Elemental_Earth_Resistance,
        [Comment("[Elemental][" + nameof(ElementalType.Water) + "] 강화")]
        Elemental_Water,
        [Comment("[Elemental][" + nameof(ElementalType.Water) + "] 저항")]
        Elemental_Water_Resistance,
        [Comment("[Elemental][" + nameof(ElementalType.Lightning) + "] 강화")]
        Elemental_Lightning,
        [Comment("[Elemental][" + nameof(ElementalType.Lightning) + "] 저항")]
        Elemental_Lightning_Resistance,

        [Comment("쿨타임 감소")]
        CoolDownReduction,

        [Comment("궁극기 충전량 배율")]
        UltGaugeChargingRate,

        // https://www.notion.so/dashproject/1ead08f8745849e9864f78421bd12aa0
        [Comment("ProtectionCount")]
        ProtectionCount,
        [Comment("MaxProtectionCount")]
        MaxProtectionCount,
        [Comment("레벨(계수전용)")]
        Level,
    }

    public enum StatusModifyType
    {
        Undefined = 0,
        Add,
        Minus,
        Multiply,
        Overwrite,
        PostAdd,
        PostMinus,
        PostMultiply,
    }
    public static class StatusTypeHelper
    {
        private static readonly Dictionary<StatusType, string> _statusTypeFormats = new Dictionary<StatusType, string>
        {
            { StatusType.AttackPower, "N0" },
            { StatusType.MaxHp, "N0" },
            { StatusType.Armor, "N0" },
            { StatusType.Elemental_Physical, "0.##" },
            { StatusType.Elemental_Fire, "0.##" },
            { StatusType.Elemental_Air, "0.##" },
            { StatusType.Elemental_Earth, "0.##" },
            { StatusType.Elemental_Water, "0.##" },
            { StatusType.Elemental_Lightning, "0.##" },
            { StatusType.Elemental_Physical_Resistance, "0.##" },
            { StatusType.Elemental_Fire_Resistance, "0.##" },
            { StatusType.Elemental_Air_Resistance, "0.##" },
            { StatusType.Elemental_Earth_Resistance, "0.##" },
            { StatusType.Elemental_Water_Resistance, "0.##" },
            { StatusType.Elemental_Lightning_Resistance, "0.##" },

            { StatusType.MoveSpeed, "P0" },
            { StatusType.AttackSpeed, "P0" },
            { StatusType.BloodSucking, "P0" },
            { StatusType.CriticalProbability, "P0" },
            { StatusType.CriticalResistanceProbability, "P0" },
            { StatusType.CriticalDamageRate, "P0" },
            { StatusType.DamageResistanceRate, "P0" },
            { StatusType.EvasionProbability, "P0" },
            { StatusType.HeadShotProbability, "P0" },
            { StatusType.HealEffect, "P0" },
            { StatusType.HpRecovery, "P0" },
            { StatusType.HealItemHealBonus, "P0" },
            { StatusType.LevelUpHealBonus, "P0" },
            { StatusType.CoolDownReduction, "P0" },
            { StatusType.UltGaugeChargingRate, "P0" },
        };
        public static IReadOnlyDictionary<StatusType, string> Keys => _keys;
        private static Dictionary<StatusType, string> _keys = new Dictionary<StatusType, string>();

        public static List<StatusType> CreatureStatusTypes = new List<StatusType>()
        {
            StatusType.AttackPower,
            StatusType.MoveSpeed,
            StatusType.MaxHp,
            StatusType.AttackSpeed,
            StatusType.CriticalProbability,
            StatusType.CriticalResistanceProbability,
            StatusType.CriticalDamageRate,
            StatusType.CoolDownReduction,
            StatusType.Elemental_Physical,
            StatusType.Elemental_Physical_Resistance,
            StatusType.Elemental_Fire,
            StatusType.Elemental_Fire_Resistance,
            StatusType.Elemental_Air,
            StatusType.Elemental_Air_Resistance,
            StatusType.Elemental_Earth,
            StatusType.Elemental_Earth_Resistance,
            StatusType.Elemental_Water,
            StatusType.Elemental_Water_Resistance,
            StatusType.Elemental_Lightning,
            StatusType.Elemental_Lightning_Resistance,
        };
        static StatusTypeHelper()
        {
            foreach (var type in EnumInfo<StatusType>.GetValues())
            {
                if (type == StatusType.Undefined)
                    continue;
                string key = "Code.Dash_Types_StatusType_" + type.ToString();
                _keys.Add(type, key);
            }
        }

        public static string GetKey(this StatusType type)
        {
            return _keys[type];
        }

        public static Locale ToLocale(this StatusType statusType)
        {
            return GetKey(statusType);
        }

        public static string ToPercentString(this float value, bool isMultiplyPercent = true)
        {
            value = isMultiplyPercent ? (value - 1) * 100 : value * 100;
            return $"{value:0.##}%";
        }

        public static string GetValueString(this StatusType statusType, float value, StatusModifyType modifyType = StatusModifyType.Undefined)
        {
            //특수하게 modifyType이 Multiply으로 지정될경우 value-1

            if (modifyType == StatusModifyType.Multiply)
            {
                return $"+{value.ToPercentString()}";
            }
            if (_statusTypeFormats.TryGetValue(statusType, out var format) == false)
            {
                format = "N0";
            }

            if (string.Equals(format, "N0") == true)
            {
                value = math.floor(value); // 소수점 버림을 위해서. GetMaxHp같은곳에서는 int형변환으로 버림을 한다.
                return value.ToString(format);
            }
            else if (string.Equals(format, "0.##") == true)
            {
                return value.ToString(format);
            }
            else
            {
                //기본적으로 퍼센트로 표시되는 경우
                return $"+{value.ToPercentString(false)}";
            }
        }
    }
}