

using Common.StaticInfo;

namespace Dash.Types
{
    public enum AbilityType
    {
        Undefined = 0,
        // Eir
        [Comment("에이르 - 바람의 장막")]
        KnockBack = 101,
        [Comment("에이르 - 신성한 방벽")]
        Barrier,
        [Comment("에이르 - 여신의 축복")]
        Buff,
        [Comment("에이르 - 몰아치는 빛")]
        HealOnce,
        [Comment("에이르 - 잔향")]
        SecondHeal,
        Eir_CoolTimeBuff,
        Eir_DefenseGrace,
        [Comment("에이르 - 응급 치유")]
        Eir_EmergencyHeal,
        [Comment("에이르 - 치유의 파동")]
        Eir_HealAura,
        Eir_IncreaseDamage = 150,
        Eir_IncreaseDuration,
        Eir_IncreaseRange,
        Eir_ReduceCoolTime,

        // Sieg
        Shield = 201,
        Rage,
        CounterAttack,
        DeathBombMark,
        ReduceCoolTimeMark,
        AttachBuffMark,
        ReduceCoolTimePerTaunt,
        CounterAttack_Hp,
        DelayBombMark,
        [Comment("지크 - 쾌도난마")]
        Sieg_Cutting,
        [Comment("지크 - 광전사")]
        Sieg_Berserk,
        [Comment("지크 - 역습")]
        Sieg_HitCounterAttack,
        Sieg_IncreaseDamage = 250,
        Sieg_IncreaseDuration,
        Sieg_IncreaseRange,
        Sieg_ReduceCoolTime,
        Sieg_SelfBuff,

        // Dainn
        FireBlindly = 301,
        Meteor,
        VolcanicEruption,
        Burn,
        ReduceCoolTime,
        FireballBounce,
        FireballRicochet,
        Dainn_Flame,
        Dainn_SoulExplosion,
        Dainn_PrecisionFire,
        Dainn_IncreaseDamage = 350,
        Dainn_IncreaseDuration,
        Dainn_IncreaseRange,
        Dainn_ReduceCoolTime,
        Dainn_FireballSpeed,
    }
}