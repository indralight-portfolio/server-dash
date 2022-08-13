using System;
using Common.StaticInfo;

namespace Dash.Types
{
    // 추후 Flags 로 변경해서 관리될수 있다.
    public enum ColliderType
    {
        Undefined = 0,
        Default = 0x01 << 1,
        [Comment("플레이어 [Player]")]
        Player = 0x01 << 2,
        [Comment("플레이어 - 물 무시 [PlayerWater]")]
        PlayerWater = 0x01 << 3,
        [Comment("플레이어 - 벽 무시 [PlayerWall]")]
        PlayerWall = 0x01 << 4,
        [Comment("플레이어 - 전체 무시 [PlayerAll]")]
        PlayerAll = 0x01 << 5,
        [Comment("몬스터[Monster]")]
        Monster = 0x01 << 6,
        [Comment("몬스터 - 물 무시 [MonsterWater]")]
        MonsterWater = 0x01 << 7,
        [Comment("몬스터 - 벽 무시 [MonsterWall]")]
        MonsterWall = 0x01 << 8,
        [Comment("몬스터 - 전체 무시 [MonsterAll]")]
        MonsterAll = 0x01 << 9,
        [Comment("방벽 [Barrier]")]
        Barrier = 0x01 << 10,
        None = 0x01 << 11,
    }

    public enum ColliderOptionType
    {
        Undefined = 0,
        Water = 1 << 1,
        Wall = 1 << 2,
    }

    public enum ColliderShape
    {
        Undefined = 0,
        Circle,
        Box,
    }
}
