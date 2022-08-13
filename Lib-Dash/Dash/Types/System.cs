using Common.StaticInfo;

namespace Dash.Types
{
    public enum CorePauseReasonType
    {
        Undefined = 0,
        HostNetworkStatus,
        SingleplayUserWanted,
        GameEnd,
    }

    public enum PlayerType
    {
        Undefined = 0,
        User,
        AI,
    }

    public enum PlayerStatusType
    {
        Undefined = 0,
        Fine,
        NetworkNotAlive,
    }

    public enum StackAttributeType
    {
        Undefined = 0,
        [Comment("강제 크리티컬")]
        ForceCritical,
        [Comment("100% 회피")]
        ForceEvade,
        [Comment("강제 헤드샷")]
        ForceHeadShot,
        [Comment("툴 사용X(거인화)")]
        ActorGiant,
        [Comment("툴 사용X(소인화")]
        ActorDwarf,
        [Comment("추가 목숨")]
        ExtraLife,
    }

    public enum PlayerAIState
    {
        Undefined = 0,
        None,
        NonBattle,
        EnterBattleStage,
        TryUseSkill,
        TryAttack,
        TryRunAway,
        TryDodgeProjectile,
        TryMoveUserPosition,
        MoveUserPosition,
    }

    public enum AIAttackState
    {
        Undefined = 0,
        None,
        AttackMove,
        Attacking,
    }
}