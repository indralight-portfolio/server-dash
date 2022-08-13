using System.Collections.Generic;
using Dash.Protocol;
using Dash.Types;
using MessagePack;

namespace server_dash.Protocol
{
    #region BattleToMatch
    [MessagePackObject()]
    public class RemoveMatch : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(RemoveMatch));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public int MatchSerial;
    }

    [MessagePackObject()]
    public class StartMatch : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(StartMatch));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public int MatchSerial;
    }

    [MessagePackObject()]
    public class EndMatch : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(EndMatch));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public int MatchSerial;
        [Key(1)]
        public int PartySerial;
        [Key(2)]
        public EndArenaReasonType Reason;
    }

    [MessagePackObject()]
    public class DropPlayer : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(DropPlayer));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public ulong OidAccount;

        [Key(1)]
        public int PartySerial;
    }

    [MessagePackObject()]
    public class KickArena : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(KickArena));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public ulong OidAccount;
    }
    #endregion

    #region MatchToBattle
    [MessagePackObject()]
    public class AllocateMatch : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(AllocateMatch));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public Dash.Protocol.Match Match;
    }
    #endregion
}