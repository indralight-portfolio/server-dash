using Common.StaticInfo;

namespace Dash.Types
{
    public enum BlackBoardType
    {
        Undefined = 0,
        None,
        [Comment("대상 액터")]
        TargetActor,
        [Comment("대상 타일")]
        TargetTile,
        [Comment("어그로 액터")]
        AggroActor,
        [Comment("마지막 액션 번호")]
        LastActionContainerIndex,
        [Comment("마지막 발동된 액션 번호")]
        LastInvokeAttackIndex,
        [Comment("마지막 발동된 액션 컨테이너 번호")]
        LastInvokeAttackContainerIndex, //액션컨테이너 마지막 인덱스 실행시 저장
        [Comment("다음 액션 번호")]
        NextAttackActionContainerIndex,
        [Comment("강제 액션 번호")]
        ForceActionContainerIndex,
        [Comment("남은 액션 회수")]
        RemainActionInvokeCount,
        SkillInvokeScore,
        PlayerAIState,
        RepeatCount,
        [Comment("도착지 액터")]
        DestinationActor,
        [Comment("자동 공격")]
        AutoAttack,
        [Comment("적중 횟수")]
        HitCount,
        [Comment("피격 횟수")]
        TakeDamageCount,
        [Comment("나를 죽인 액터")]
        KillerActor,
        [Comment("가까운 유저")]
        NearUser,
        [Comment("AI공격중 여부")]
        AIAttackState,
        ActorPhase,
        ActionGroupActiveTime,
    }
}