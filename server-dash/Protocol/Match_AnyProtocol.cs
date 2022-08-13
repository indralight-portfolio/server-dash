using System.Collections.Generic;
using Dash.Protocol;
using Dash.Types;
using MessagePack;

namespace server_dash.Protocol
{
    #region AnyToMatch
    [MessagePackObject()]
    public class ServerAlive : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(ServerAlive));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public string UUID;
        [Key(1)]
        public string Endpoint;
        [Key(2)]
        public Dash.Types.ServiceAreaType ServiceAreaType;
    }
    #endregion

    #region MatchToAny
    [MessagePackObject()]
    public class KickAllSession : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(KickAllSession));
        public int GetTypeCode() => TypeCode;
    }
    #endregion
}