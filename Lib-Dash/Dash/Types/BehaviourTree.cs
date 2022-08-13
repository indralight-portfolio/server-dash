using System;
using Common.StaticInfo;
using Unity.Mathematics;
namespace Dash.Types
{
    public enum ActionNodeAttribute
    {
        Undefined = 0,
        [Comment("액션 컨테이너 번호 [" + nameof(Index) + "]")]
        Index,
        [Comment("블랙보드에 설정된 액션 번호")]
        BlackboardAttackActionContainerIndex,
        [Comment("블랙보드에 설정된 강제 액션 번호")]
        BlackboardForceActionContainerIndex,
    }

    [Flags]
    public enum AttackAttribute
    {
        Undefined = 0,
        Normal = 0x01,
        Critical = 0x01 << 1,
        Evade = 0x01 << 2,
        HeadShot = 0x01 << 3,
        Buff = 0x01 << 4,
        UseProtectCount = 0x01 << 5,
        IgnoreEventTrigger = 0x01 << 6,
        IgnoreShield = 0x01 << 7,
    }

    public enum ActionSelectorAttribute
    {
        Undefined = 0,
        SequentialCandidate,
        SequentialActionGroupType,
        ShuffleCandidate,
    }

    public enum ActorState
    {
        Undefined = 0,
        State01,
        State02,
        State03,
        State04,
        State05,
        State06,
        State07,
        State08,
        State09,
        State10,
        State11,
        State12,
        State13,
        State14,
        State15,
        SpecificAction01 = 100,
        Wait = 998,
        StateTransit = 999,
    }

    public enum AStarExcludeAttribute
    {
        Undefined = 0,
        None,
        Monster
    }

    public enum AStarDestinationAttribute
    {
        Undefined = 0,
        [Comment("대상 액터")]
        TargetActor,
        [Comment("맵 중앙")]
        StageCenter,
        [Comment("대상 타일")]
        TargetTile,
        [Comment("가까운 유저")]
        NearUser,
        [Comment("타일 좌표 입력")]
        MenualTileCoordinate,
        [Comment("[AIPlayer]유저를 쫒아간다")]
        FollowUser,
    }

    public enum BoolAttribute
    {
        Undefined = 0,
        [Comment("원본")]
        Normal,
        [Comment("반전")]
        Inverse,
    }

    public enum BlackBoardAttribute
    {
        Undefined = 0,
        [Comment("고정 설정")]
        Set,
        [Comment("제거")]
        Remove,
        [Comment("범위값 중 랜덤 설정")]
        RandomRangeSet,
        [Comment("감소")]
        Decrease,
        [Comment("후보들 중 랜덤 설정")]
        RandomPick,
    }

    public enum CalculateTargetTileAttribute
    {
        Undefined,
        [Comment("바라보는 방향 거리")]
        LookDistance,
        [Comment("맵 중앙")]
        StageCenter,
    }

    [Flags]
    public enum CollisionAttribute
    {
        Undefined = 0,
        Wall = 0x01,
        Monster = 0x01 << 1,
        Character = 0x01 << 2,
        Field = 0x01 << 3,
    }

    public enum CompareAttribute
    {
        Undefined = 0,
        [Comment("초과")]
        Exceed,
        [Comment("이상")]
        Above,
        [Comment("동등")]
        Equal,
        [Comment("다름")]
        NotEqual,
        [Comment("이하")]
        Below,
        [Comment("미만")]
        Under,
    }

    public enum CCStateAttribute
    {
        Undefined = 0,
        Root,
        Stun,
        Taunt,
        Groggy,
    }

    public enum CompareValueAttribute
    {
        Undefined = 0,
        [Comment("체력")]
        Hp,
        [Comment("이동속도")]
        MoveSpeed,
        [Comment("액터 상태")]
        ActorState,
        [Comment("소환된 몬스터 수")]
        ChildCount,
        [Comment("대상과의 거리")]
        TargetDistance,
        [Comment("대상과의 각도")]
        TargetAngle,
        [Comment("이동 상태로 타일에 머무른 시간")]
        MoveStayTime,
        [Comment("같은 타일에 머무른 시간")]
        TileStayTime,
        [Comment("모든 액션 회수")]
        AllActionCount,
        [Comment("타일 개수")]
        TileList,
        [Comment("이동 대기 시간")]
        WaitingTime,
        [Comment("타일 이동 개수")]
        TileMoveCount,
        [Comment("체력 비율")]
        HpRatio,
        [Comment("ProtectionCount")]
        ProtectionCount,
    }

    public enum IsActionAllDoneAttribute
    {
        Undefined,
        All,
        ActionGroup,
    }

    public enum IsExistTargetAttribute
    {
        Undefined,
        Actor,
        Tile,
    }

    public enum LocationAttribute
    {
        Undefined = 0,
        Pass,
        Remain,
    }

    public enum MoveDecisionAttribute
    {
        Undefined = 0,
        [Comment("조작시 이동, 아닐경우 정지")]
        Normal,
        [Comment("이동")]
        True,
        [Comment("정지")]
        False,
    }

    public enum SkillChargeType
    {
        Undefined = 0,
        BasicAttackCount,
        NoHitTime,
        ActionCoolTime,
        ActionGroupCoolTime,
        SequentialAction,
        UltActionGroupCoolTime,
        ActionCoolTimeStackable,
    }

    public enum SkillDurationType
    {
        None = 0,
        BuffDuration,
    }

    public enum SkillTriggerType
    {
        Undefined = 0,
        None,
        Touch,
        TapPan,
    }

    public enum SkillPanType
    {
        Undefined = 0,
        OwnerPosition,
        PanPosition,
        PanDirection,
        ClampOwnerToPanDirection,
    }

    public enum SearchAttribute
    {
        Undefined = 0,
        World,
        Range,
    }

    public enum SearchTileAttribute
    {
        Undefined = 0,
        Random,    // 조건 검사 없이 랜덤
        OnlyField, // Field Type만
        All,       // 전부
        Edge,      // 맵의 테두리 부분만
        TileCoordinate,
        Wall,
        EdgeWall,
    }

    public enum SetActionAllStopAttribute
    {
        Undefined,
        All,
        ActionGroup,
        ExceptMovable,
    }

    public enum SetLookDirectionAttribute
    {
        Undefined,
        TargetTile,
        MoveDirection,
        MainSkill,
        TargetActor,
    }

    public enum SetMoveDirectionAttribute
    {
        Undefined,
        Value,
        TargetActor,
    }

    public enum SetDirectionAttribute
    {
        Undefined,
        [Comment("설정된 값")]
        Value,
        [Comment("범위내 랜덤 값")]
        RandomRange,
        [Comment("대상 액터")]
        TargetActor,
        [Comment("메인스킬 방향")]
        MainSkill,
    }

    public enum SetTargetActorAttribute
    {
        Undefined = 0,
        [Comment("도발 시전자")]
        Taunt,
    }

    public enum TargetAttribute
    {
        Undefined = 0,
        [Comment("가까운 캐릭터(AI제외)")]
        NearCharacter,
        [Comment("가까운 캐릭터(AI포함)")]
        NearCharacter_WidthAI,
        [Comment("가까운 몬스터")]
        NearMonster,
        [Comment("자신 기준 가까운 랜덤 타일")]
        NearRandomTile,
        [Comment("먼 캐릭터(AI제외)")]
        FarCharacter,
        [Comment("먼 캐릭터(AI포함)")]
        FarCharacter_WidthAI,
        [Comment("먼 몬스터")]
        FarMonster,
        [Comment("대상 액터 근처 랜덤 타일")]
        TargetActorRandomTile,
        [Comment("최소 체력 캐릭터(AI제외)")]
        LowestHpCharacter,
        [Comment("최소 체력 캐릭터(AI포함)")]
        LowestHpCharacter_WidthAI,
        [Comment("최소 체력 몬스터")]
        LowestHpMonster,
        [Comment("자신")]
        Self,
        [Comment("랜덤 캐릭터(AI제외)")]
        RandomCharacter,
        [Comment("랜덤 캐릭터(AI포함)")]
        RandomCharacter_WidthAI,
        [Comment("가까운 두번째 캐릭터(AI제외)")]
        NearSecondCharacter,
        [Comment("가까운 두번째 캐릭터(AI포함)")]
        NearSecondCharacter_WidthAI,
    }
    public enum AggroTargetAttribute
    {
        Undefined = 0,
        [Comment("대상 액터")]
        TargetActor,
        [Comment("어그로가 가장 높은 캐릭터")]
        HighestAggroCharacter,
        [Comment("어그로가 가장 낮은 캐릭터")]
        LowestAggroCharacter,
        [Comment("랜덤 캐릭터")]
        RandomCharacter,
    }

    public enum AggroValueAttribute
    {
        Set,
        Add,
    }

    public enum ActionGroupConditionType//액션 그룹 동작 타입
    {
        Undefined = -1,
        None,
        Sequential,
        SequentialChangableByBuff,
        Chain,
        ChainChangableByBuff
    }
    public static class CompareValueHelper
    {
        public static bool Compare(this CompareAttribute attribute, float value, float condition)
        {
            switch (attribute)
            {
                case CompareAttribute.Exceed:
                    return value > condition;
                case CompareAttribute.Above:
                    return value >= condition || math.abs(value - condition) <= float.Epsilon;
                case CompareAttribute.Equal:
                    return math.abs(value - condition) <= float.Epsilon;
                case CompareAttribute.NotEqual:
                    return math.abs(value - condition) > float.Epsilon;
                case CompareAttribute.Below:
                    return value <= condition || math.abs(value - condition) <= float.Epsilon;
                case CompareAttribute.Under:
                    return value < condition;
                default:
                    return false;
            }
        }
    }
}
    