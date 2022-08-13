using System;

namespace Dash.Types
{
    [Flags]
    public enum AuthorityType
    {
        Undefined = 0,
        None = 0x01 << 0,
        State = 0x01 << 1,
        // 0x01 << 2 예약
        // 0x01 << 3 예약
        // 0x01 << 4 예약
        Client = None | State,
        Host = Client | (0x01 << 2),
        Server = Client | (0x01 << 3),
        Master = Host | Server,
        Editor = Master | (0x01 << 4)
    }
}