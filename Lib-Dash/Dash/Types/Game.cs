using System;

namespace Dash.Types
{
    [Flags]
    public enum ArenaMatchType
    {
        Undefined = 0,
        Single = 0x01,
        Multi = 0x01 << 1,
    }

    [Flags]
    public enum ArenaEndType
    {
        Undefined = 0,
        Abort,
        Fail,
        Clear,
        TimeOver,
    }
}