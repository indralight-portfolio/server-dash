using Dash.Protocol;
using MessagePack;

namespace server_dash.Protocol
{
    #region LobbyToMatch SequentialProtocol
    [MessagePackObject()]
    public class GetBattleServerRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(GetBattleServerRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
    }
    [MessagePackObject()]
    public class GetBattleServerResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(GetBattleServerResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Dash.Protocol.GetBattleServerResponse HttpResponse { get; set; }
    }

    [MessagePackObject()]
    public class EnqueueMatchRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(EnqueueMatchRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong OidAccount { get; set; }
        [Key(2)]
        public int ChapterId { get; set; }
        [Key(3)]
        public string Nickname { get; set; }
    }
    [MessagePackObject()]
    public class EnqueueMatchResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(EnqueueMatchResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Dash.Protocol.EnqueueMatchResponse HttpResponse { get; set; }
    }

    [MessagePackObject()]
    public class DequeueMatchRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(DequeueMatchRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong OidAccount { get; set; }
    }
    [MessagePackObject()]
    public class DequeueMatchResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(DequeueMatchResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Dash.Protocol.DequeueMatchResponse HttpResponse { get; set; }
    }

    [MessagePackObject()]
    public class CheckMatchRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(CheckMatchRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong OidAccount { get; set; }
    }
    [MessagePackObject()]
    public class CheckMatchResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(CheckMatchResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Dash.Protocol.CheckMatchResponse HttpResponse { get; set; }
    }

    [MessagePackObject()]
    public class CheckArenaRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(CheckArenaRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong OidAccount { get; set; }
    }
    [MessagePackObject()]
    public class CheckArenaResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(CheckArenaResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Dash.Protocol.CheckArenaResponse HttpResponse { get; set; }
    }

    [MessagePackObject()]
    public class ForgiveArenaRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(ForgiveArenaRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong OidAccount { get; set; }
    }
    [MessagePackObject()]
    public class ForgiveArenaResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(ForgiveArenaResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Dash.Protocol.ForgiveArenaResponse HttpResponse { get; set; }
    }

    [MessagePackObject()]
    public class GetSocialServerRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(GetSocialServerRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong OidAccount { get; set; }
    }
    [MessagePackObject()]
    public class GetSocialServerResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(GetSocialServerResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Dash.Protocol.GetSocialServerResponse HttpResponse { get; set; }
    }


    [MessagePackObject()]
    public class GetPartyConnectInfoRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(GetPartyConnectInfoRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong OidAccount { get; set; }
        [Key(2)]
        public string PartyCode { get; set; }
    }
    [MessagePackObject()]
    public class GetPartyConnectInfoResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(GetPartyConnectInfoResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Dash.Protocol.GetSocialServerResponse HttpResponse { get; set; }
    }
    #endregion
}