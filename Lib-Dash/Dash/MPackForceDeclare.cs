using Dash.Core;
using Dash.Core.Action.Property;
using Dash.Core.Components;
using Dash.Core.Entities.BehaviorTree;
using Dash.Core.Systems;
using Dash.Core.Systems.Entity;
using Dash.Model;
using Dash.StaticData;
using Dash.StaticData.Entity;
using Dash.Types;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Dash
{
    // MessagePack.FormatterNotRegisteredException: Dash.Types.CollisionAttribute is not registered in this resolver. resolver:CompositeResolver
    // 위와 같은 에러가 뜰 경우, 해당 타입을 아래에 기재한 후, Message Pack Code Generation을 돌리면 해결됨.
    [MessagePackObject()]
    public class MPackForceDeclare
    {
        [Key(1)]
        public List<StatusDelta> _1;

        [Key(2)]
        public Dictionary<int, StaticData.Vector2> _2;

        [Key(3)]
        public Dictionary<int, int> _3;

        [Key(4)]
        public List<StatusType> _4;

        [Key(5)]
        public List<BuffKey> _5;

        [Key(6)]
        public Dictionary<int, float> _6;

        [Key(7)]
        public Dictionary<int, RewardInfo> _7;

        [Key(8)]
        public HashSet<int> _8;

        [Key(9)]
        public Dictionary<int, List<int>> _9;

        [Key(11)]
        public List<AreaProcedure.AreaAttribute> _11;

        [Key(12)]
        public HashSet<BlockNextStageReasonType> _12;

        [Key(13)]
        public List<Dash.StaticData.Vector2> _13;

        [Key(14)]
        public List<Dash.StaticData.Vector3> _14;

        [Key(15)]
        public List<Dash.StaticData.IntVector2> _15;

        [Key(16)]
        public List<Dash.StaticData.IntVector3> _16;

        [Key(17)]
        public Dictionary<int, PlayerResult> _17;

        [Key(18)]
        public Dictionary<int, bool> _18;

        [Key(19)]
        public Dictionary<int, Queue<int>> _19;

        [Key(20)]
        public Dictionary<int, List<StackAttributeType>> _20;

        [Key(21)]
        public List<EventInfo> _21;


        [Key(22)]
        public List<RageStatus> _22;

        [Key(23)]
        public Dictionary<int, List<RageStatus>> _23;

        [Key(24)]
        public Dictionary<int, ActorUnityState> _24;

        [Key(25)]
        public Dictionary<int, uint> _25;

        [Key(28)]
        public List<PlayerResult> _28;

        [Key(29)]
        public ProcessMonitorSnapshot[] _29;

        [Key(30)]
        public List<StaticData.Stage.StageInfo> _30;

        [Key(31)]
        public List<StaticData.Entity.BehaviorTreeNodeInfo> _31;

        [Key(32)]
        public List<ActorState> _32;

        [Key(33)]
        public List<SelectActorStateAction.CandidateCondition> _33;

        [Key(37)]
        public List<AbilityType> _37;

        [Key(38)]
        public Dictionary<int, HashSet<int>> _38;

        [Key(39)]
        public Dictionary<AbilityType, int> _39;

        [Key(40)]
        public ValueTuple<int, int>[] _40;

        [Key(41)]
        public Dash.Core.Entities.Area.Target _41;

        [Key(42)]
        public ArithmeticOperation _42;

        [Key(43)]
        public ShieldShard[] _43;

        [Key(44)]
        public Dictionary<int, long> _44;

        [Key(45)]
        public Dictionary<ActionGroupType, StaticData.Vector2> _45;

        [Key(46)]
        public Dictionary<EpisodeGroupType, int> _46;

        [Key(47)]
        public List<CompareAliveMonsterCount.CandidateCondition> _48;

        [Key(48)]
        public Dictionary<byte, sbyte> _49;

        [Key(49)]
        public List<BuffAttachConditionAndId> _50;

        [Key(50)]
        public Dictionary<BlackBoardType, int> _51;

    }
}