namespace Dash.Types
{
    public enum SuspectCaseType
    {
        Undefined = 0,
        AttackerNull,
        AttackOwnerNull,
        CreateProjectile_CommandKeyOwnerNull,
        InvalidAliveDeadRequest,
        ProjectileTrigger,
        AreaTrigger,
        InvalidAttackRequest,
        CreateArea_DestroyTiming,
        CreateBarrier_CommandKeyOwnerNull,
    }
    public enum ValidationSecondaryKeyType
    {
        ActionIndex,
        Serial,
    }
}