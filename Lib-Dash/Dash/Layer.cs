#if Common_Unity
using UnityEngine;

namespace Dash
{
    // 실제 GameObject의 Layer를 변경하기 위해 사용되는 값
    public class Layer
    {
        public static readonly int None = LayerMask.NameToLayer(nameof(None));
        public static readonly int Default = LayerMask.NameToLayer(nameof(Default));
        public static readonly int Wall = LayerMask.NameToLayer(nameof(Wall));
        public static readonly int Field = LayerMask.NameToLayer(nameof(Field));
        public static readonly int Player = LayerMask.NameToLayer(nameof(Player));
        public static readonly int PlayerIgnoreWater = LayerMask.NameToLayer(nameof(PlayerIgnoreWater));
        public static readonly int PlayerIgnoreWall = LayerMask.NameToLayer(nameof(PlayerIgnoreWall));
        public static readonly int PlayerIgnoreAll = LayerMask.NameToLayer(nameof(PlayerIgnoreAll));
        public static readonly int Monster = LayerMask.NameToLayer(nameof(Monster));
        public static readonly int MonsterIgnoreWater = LayerMask.NameToLayer(nameof(MonsterIgnoreWater));
        public static readonly int MonsterIgnoreWall = LayerMask.NameToLayer(nameof(MonsterIgnoreWall));
        public static readonly int MonsterIgnoreAll = LayerMask.NameToLayer(nameof(MonsterIgnoreAll));
        public static readonly int Barrier = LayerMask.NameToLayer(nameof(Barrier));
    }

    public static class RaycastLayer
    {
        public static readonly int Water = 1 << LayerMask.NameToLayer(nameof(Water));
        public static readonly int Wall = 1 << LayerMask.NameToLayer(nameof(Wall));
        public static readonly int Field = 1 << LayerMask.NameToLayer(nameof(Field));
        public static readonly int Player = 1 << LayerMask.NameToLayer(nameof(Player));
        public static readonly int PlayerIgnoreWater = 1 << LayerMask.NameToLayer(nameof(PlayerIgnoreWater));
        public static readonly int PlayerIgnoreWall = 1 << LayerMask.NameToLayer(nameof(PlayerIgnoreWall));
        public static readonly int PlayerIgnoreAll = 1 << LayerMask.NameToLayer(nameof(PlayerIgnoreAll));
        public static readonly int Monster = 1 << LayerMask.NameToLayer(nameof(Monster));
        public static readonly int MonsterIgnoreWater = 1 << LayerMask.NameToLayer(nameof(MonsterIgnoreWater));
        public static readonly int MonsterIgnoreWall = 1 << LayerMask.NameToLayer(nameof(MonsterIgnoreWall));
        public static readonly int MonsterIgnoreAll = 1 << LayerMask.NameToLayer(nameof(MonsterIgnoreAll));
        public static readonly int Barrier = 1 << LayerMask.NameToLayer(nameof(Barrier));
    }
}
#endif