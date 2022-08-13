using Common.StaticInfo;

namespace Dash.Types
{
    public enum ActionStateType
    {
        Undefined = 0,
        None,
        Ready,     // 사전동작
        Start,     // 선딜레이
        Active,    // 본동작
        End,       // 후딜레이
        Finish,    // 마무리 동작
        CoolTime,
        Suspend,
    }

    public enum GaugeType
    {
        Undefined = 0,
        Stack,
        Progress,
        ActionAvailable,
    }

    public enum DepartureType
    {
        Undefined = 0,
        Owner,        // 플레이어 
        TargetActor,  // 대상 액터
        TileList,     // Tile
        SkillInfo,//스킬인포에 있는 정보를 사용함
        WallLeft,
        WallRight,
        WallTop,
        WallBottom,
    }

    public enum DestinationType
    {
        Undefined = 0,
        ParameterTileIndex, // 파라미터
    }

    public enum EntityCreateType
    {
        Undefined = 0,
        AllCharacter,
        TargetActor,
        TargetAllCharacter,
    }

    public enum KidnapTarget
    {
        Undefined,
        [Comment("대상 액터")]
        TargetActor,
        [Comment("모든 캐릭터")]
        AllCharacter,
    }

    public enum MultiAreaActionType
    {
        Undefined,
        [Comment("대상을 제외한 전 캐릭터")]
        ExceptTargetCharacter,
    }
}