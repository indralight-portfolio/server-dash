using System;
using System.Collections.Generic;

namespace Dash
{
    public enum ManualTypeCodes : int
    {
        Undefined = 0,
        // Client <-> Battle
        MarkAlive = 1,
        MyTurnData = 2,
        MyTurnDataWithSyncPositions = 3,
        MyTurnDataWithSyncPositionsComplex = 4,
        SyncTurnData = 5,
        SyncTurnDataWithSyncPositions = 6,
        ReconnectSingleArena = 7,
        // Dash.Core
        Summon_Basic = 11,
        Summon_Grade,
        Summon_Level,
        Summon_Group,
        Summon_OwnerSerial_Reason,
        Summon_OwnerSerial_Creature,
        Summon_RetrieveInfo,
        SuicideActor,
        CreateProjectile_Basic,
        CreateProjectile_Destination,
        CreateProjectile_DestinationWait, // 20
        CreateProjectile_Wait,
        CreateProjectile_Homing,
        CreateProjectile_Multiple,
        CreateArea_Basic,
        CreateArea_Target,
        CreateArea_DamageHeal,
        CreateArea_DamageHealTarget,
        CreateArea_DamageHealShield,
        CreateArea_DamageHealShieldTarget,
        CreateBarrier_Basic, // 30
        RemoveActor,
        RemoveActors,
        AliveDead,
        RemoveEntities,
        RemoveProjectile,
        RemoveProjectiles,
        RemoveArea,
        RemoveAreas,
        RemoveBarrier,
        ActionComponent_ActionStart, // 40
        ActionComponent_ActionStop,
        ActionComponent_IncreaseInvokeCount,
        ActionComponent_LoopActionNextStep,
        ActionComponent_ChangeTemplateIndex,
        MoveComponent_SetMoveDirection,
        MoveComponent_SetLookDirection,
        MoveComponent_SetDirection,
        MoveComponent_SetStop,
        MoveComponent_SetLocation,
        MoveComponent_RemoveLocation,
        MoveComponent_RemoveStayTime, // 50
        MoveComponent_RemoveTileMoveCount,
        Attack,
        AttackResponse,
        ProjectileAttack,
        AreaAttack,
        Heal,
        GiveShield,
        SyncShields,
        SyncPosition,
        SyncPosition_Complex, // 60
        AreaTrigger,
        ProjectileHit,
        Triggers,
        BlackboardComponent_SetVector2,
        BlackboardComponent_SetInt,
        BlackboardComponent_RemoveInt,
        BlackboardComponent_SetTileList,
        BlackboardComponent_SetSkillDirection,
        BlackboardComponent_RemoveSkillDirection,
        BlackboardComponent_SetWatingTime,// 70
        BlackboardComponent_RemoveWatingTime, 
        BlackboardComponent_SetActorState,
        BlackboardComponent_RemoveStayTime,
        InvincibleComponent_SetInvincibleTime,
        CreateGenerator_Basic,
        CCComponent_RecoverGroggy,
        IncreaseProtectionCount,
        ToggleStatusDelta,
    }

    /* MessagePack serializer에서 integer 값이 127 이하일 때 1 byte 만을 사용.
     * 1  ~ 10 : Client <-> BattleServer Protocol
     * 11 ~ 80 : Dash.Core Command
     * 81 ~ 127 : 미정
     */
    public static class TypeCodeGenerator
    {
        // 현재 MessageSerializer에서 모든 Protocol들을 
        private static HashSet<int> _forceTypeCodes = new HashSet<int>();
        public static int Force(ManualTypeCodes type)
        {
            int value = (int) type;
            if (_forceTypeCodes.Contains(value) == true)
            {
                throw new Exception($"Already taken TypeCode : {value}");
            }

            _forceTypeCodes.Add(value);
            return value;
        }

        public static int FromName(string name)
        {
            return Common.Utility.StringHash.GetHashCode(name);
        }
    }
}