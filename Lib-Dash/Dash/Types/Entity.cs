using System;
using Common.StaticInfo;

namespace Dash.Types
{
    public enum ActorType
    {
        Undefined = 0,
        Character,
        Monster,
        Barrier,
        Creature,
    }

    public enum JobType
    {
        Undefined = 0,
        ADCARRY,
        FIGHTER,
        SUPPORTER,
    }

    public enum ActorAttribute
    {
        Undefined = 0,
        [Comment("일반")]
        Normal,
        [Comment("엘리트")]
        Elite,
        [Comment("보스")]
        Boss,
        [Comment("크리쳐")]
        Creature,
    }

    public enum ActionGroupType
    {
        Undefined,
        Base,
        Skill,
        UltSkill,
    }

    public enum AreaTriggerRule
    {
        Undefined = 0,
        Once,
        OncePerEntity,
        EverySecond,
        EveryHalfSecond,
        EveryQuarterSecond,
        OneTick,
    }

    [Flags]
    public enum AreaTriggerIgnoreType
    {
        [Comment("수정 X")]
        Undefined = 0,
        [Comment("생성자")]
        Owner = 0x01,
        [Comment("캐릭터")]
        Character = 0x01 << 1,
        [Comment("몬스터")]
        Monster = 0x01 << 2,
        [Comment("보스몬스터")]
        BossMonster = 0x01 << 3,
        [Comment("크리쳐")]
        Creature = 0x01 << 4,
    }

    public enum AreaTriggerActionType
    {
        Undefined = 0,
        Nothing,
        Damage,
        Heal,
        HealShield,
        DamageHeal,
        DamageToAll,
        GiveBuff,
    }

    public enum BuffStackType
    {
        Undefined = 0,
        Stack,
        Reset,
        ResetAndStack,
        Remove, // 빈번히 호출되는 곳에선 자제.
        StackSyncElapsedSeconds,
    }

    public enum BuffKeyType
    {
        Undefined = 0,
        Id,
        [Comment("Caster(플팀 상의 필요)")]
        Caster,
        [Comment("IdWithCaster(플팀 상의 필요)")]
        IdWithCaster,
        [Comment("IdWithCasterOwner")]
        IdWithCasterOwner,
    }

    public enum BuffActionAttribute
    {
        Undefined,
        AllCharacter,
        TargetActor,
        Self,
    }

    public enum DestroyCause
    {
        Undefined,
        None,
        System,
        Destroyer,
        Barrier,
        Wall,
        Actor,
        LifeTime,
    }

    public enum EntityType
    {
        Undefined,
        Area,
        Barrier,
        Projectile,
        Actor,
    }

    public enum MoveType
    {
        Undefined = 0,
        None,
        LinearMove,
        QuadraticBezierMove,
        SyncPosition,
        Homing,
        SineMove,
        QuadraticBezierAccelerateMove,
        PointLinearMove,
        LinearMoveDetermined,
        DeparturePointLinearMove,
        BezierHomingMove,
        DeterminedBezierMove,
    }

    public enum PreviewType
    {
        Undefined = 0,
        None,
        Line,
        LineProjectile,
        Circle,
        CirclePerCollider,
        Square,
        VisualEffect,
    }

    public enum ReviveReasonType
    {
        Undefined = 0,
        Coop,
        Ticket,
        StageClear,
        Test,
    }

    public enum BounceType
    {
        Undefined,
        Mirror,
        Return,
    }

    public enum SummonReasonType
    {
        Undefined = 0,
        Stage,
        Wave,
        EventAction,
        IAction,
        EntitySystem,
        Simulation,
    }

    public enum HealReasonType
    {
        Undefined = 0,
        Skill,
        HpBoost,
        LevelUp,
        Actor,
        Item,
        BloodSuck,
        ExtraLife,
        Buff,
    }

    public enum PushType
    {
        Undefined = 0,
        [Comment("기본 Push 규칙 적용")]
        System,
        [Comment("지정한 방향으로 밀기")]
        ManualDirection,
        [Comment("(대상.Pos - 발동자.Pos) 방향으로 밀기")]
        TargetDirection,
        [Comment("InvokerDirection (대상 -> 발동자) 방향")]
        InvokerDirection,
        [Comment("InvokerPull 대상 -> 발동자 앞으로 당겨옴")]
        InvokerDirectionPull,
    }

    [Flags]
    public enum PushOption
    {
        Undefined = 0,
        [Comment("대상 - 발동자 최대 거리 제한")]
        CheckMaxDistance,
    }

    public enum CCType
    {
        Undefined = 0,
        Slow,
        Root,
        Stun,
        Taunt,
        Groggy,
    }

    public enum DurationType
    {
        Undefined = 0,
        Add,
        Overwrite,
        ChooseGreater,
    }

    [Flags]
    public enum GeneratorFlag : int
    {
        Undefined = 0,
        None = 0x01,
        Feature1 = 0x01 << 1,
        Feature2 = 0x01 << 2,
        Feature3 = 0x01 << 3,
        Feature4 = 0x01 << 4,
        Feature5 = 0x01 << 5,
        Feature6 = 0x01 << 6,
        Feature7 = 0x01 << 7,
        Feature8 = 0x01 << 8,
        Feature9 = 0x01 << 9,
        Feature10 = 0x01 << 10,
        Feature11 = 0x01 << 11,
        Feature12 = 0x01 << 12,
        Feature13 = 0x01 << 13,
        Feature14 = 0x01 << 14,
        Feature15 = 0x01 << 15,
        Feature16 = 0x01 << 16,
        Feature17 = 0x01 << 17,
        Feature18 = 0x01 << 18,
        Feature19 = 0x01 << 19,
        Feature20 = 0x01 << 20,
    }

    public enum ActorBattleState
    {
        Undefined = 0,
        Sleep,
        Awake,
        Default, // 소환시 사용, MonsterInfo의 초기 상태로 설정
    }

    [Flags]
    public enum AliveFlag : int
    {
        Undefined = 0,
        DelayDeath,
    }

    public enum HealFlag : int
    {
        Undefined = 0,
        None,
        ApplyOption,
    }

    public enum StatIncreaseType
    {
        Undefined,
        AddValue,
        AddBaseMultiply,
    }

    public enum ElementalType
    {
        Undefined = 0,
        Physical,
        Fire,
        Air,
        Earth,
        Water,
        Lightning,
    }

    public enum ElementalReactionType
    {
        Undefined,
        Anything,
        Physical,
        Fire,
        Air,
        Earth,
        Water,
        Lightning,
        Magma,
        Frozne,
        ElectricShock,
        Weathering,
        Crack,
        Petrification,
        IceBreaking,
        Overload,
        Diffusion,
        Repulsion,
    }

    public enum CharacterHeaderExtraUIType
    {
        Undefined,
        BuffStackCount
    }

    public static class EntityTypeHelper
    {
        public static CodeLocale ToCodeLocaleType(this JobType job)
        {
            switch (job)
            {
                case JobType.ADCARRY:
                    return CodeLocale.Dash_Types_JobType_ADCARRY;
                case JobType.FIGHTER:
                    return CodeLocale.Dash_Types_JobType_FIGHTER;
                case JobType.SUPPORTER:
                    return CodeLocale.Dash_Types_JobType_SUPPORTER;
                default:
                    return CodeLocale.Dash_Types_JobType_SUPPORTER;
            }
        }

        public static CodeLocale ToCodeLocaleType(this ActionGroupType actionGroup)
        {
            switch (actionGroup)
            {
                case ActionGroupType.Base:
                    return CodeLocale.Dash_Types_ActionGroup_Base;
                case ActionGroupType.Skill:
                    return CodeLocale.Dash_Types_ActionGroup_Skill;
                case ActionGroupType.UltSkill:
                    return CodeLocale.Dash_Types_ActionGroup_UltSkill;
                default:
                    return CodeLocale.Dash_Types_ActionGroup_Base;
            }
        }


        public static CodeLocale ToCodeLocaleType(this ElementalType elementalType)
        {
            switch (elementalType)
            {
                case ElementalType.Physical:
                    return CodeLocale.Dash_Types_ElementalType_Physical;
                case ElementalType.Fire:
                    return CodeLocale.Dash_Types_ElementalType_Fire;
                case ElementalType.Air:
                    return CodeLocale.Dash_Types_ElementalType_Air;
                case ElementalType.Earth:
                    return CodeLocale.Dash_Types_ElementalType_Earth;
                case ElementalType.Water:
                    return CodeLocale.Dash_Types_ElementalType_Water;
                case ElementalType.Lightning:
                    return CodeLocale.Dash_Types_ElementalType_Lightning;
                default:
                    return CodeLocale.Dash_Types_ElementalType_Physical;
            }
        }
    }
}