

using System;

namespace Dash.Types
{
    [Flags]
    public enum WeaponGrowthViewType
    {
        Undefined = 0,
        Empty = 0x01,
        LevelUp = 0x01 << 1,
        OvercomeUp = 0x01 << 2,
        ReforgeUp = 0x01 << 3,
        ListView = 0x01 << 4,
    }


    public enum CharacterGrowthViewType
    {
        Undefined = 0,
        Empty,
        LevelUp,
        OvercomeUp,
        RankUp,
        ListView,
    }
}