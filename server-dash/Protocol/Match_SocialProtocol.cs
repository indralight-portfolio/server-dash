using Dash.Protocol;
using Dash.Types;
using MessagePack;
using System.Collections.Generic;

namespace server_dash.Protocol
{
    #region SocialToMatch SequentialProtocol
    [MessagePackObject()]
    public class EnqueueMatchWithPartyRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(EnqueueMatchWithPartyRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public int PartySerial;
        [Key(2)]
        public List<OidAndNickname> Members;
        [Key(3)]
        public int ChapterId;
    }
    [MessagePackObject()]
    public class EnqueueMatchWithPartyResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(EnqueueMatchWithPartyResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Party.State State;
        [Key(2)]
        public ErrorCode ErrorCode;
        [Key(3)]
        public string ErrorText;
        [Key(4)]
        public Dash.Protocol.Match Match;
        public void SetResult(ErrorCode errorCode, string errorText)
        {
            ErrorCode = errorCode;
            ErrorText = errorText;
        }
    }
    [MessagePackObject()]
    public class DequeueMatchWithPartyRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(DequeueMatchWithPartyRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Party Party;
    }
    [MessagePackObject()]
    public class DequeueMatchWithPartyResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(DequeueMatchWithPartyResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public Party.State State;
        [Key(2)]
        public ErrorCode ErrorCode;
        [Key(3)]
        public string ErrorText;
        [Key(4)]
        public Dash.Protocol.Match Match;
        public void SetResult(ErrorCode errorCode, string errorText)
        {
            ErrorCode = errorCode;
            ErrorText = errorText;
        }
    }
    [MessagePackObject()]
    public class CreatePartyRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(CreatePartyRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public int ChapterId;
        [Key(2)]
        public List<MatchPlayer> NewMembers;
        [Key(3)]
        public int PartySerial;
        [Key(4)]
        public string PartyCode;
    }
    [MessagePackObject()]
    public class CreatePartyResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(CreatePartyResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public int PartySerial { get; set; }
        [Key(2)]
        public ErrorCode ErrorCode;
    }
    [MessagePackObject()]
    public class JoinPartyRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(JoinPartyRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public int PartySerial;
        [Key(2)]
        public List<MatchPlayer> NewMembers;
    }
    [MessagePackObject()]
    public class JoinPartyResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(JoinPartyResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ErrorCode ErrorCode;
    }
    [MessagePackObject()]
    public class LeavePartyRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(LeavePartyRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public int PartySerial;
    }
    [MessagePackObject()]
    public class LeavePartyResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(LeavePartyResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ErrorCode ErrorCode;
    }
    [MessagePackObject()]
    public class DequeueByLeavePartyRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(DequeueByLeavePartyRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong OidAccount;
        [Key(2)]
        public bool IsResetPartyWhenAlone;
    }
    [MessagePackObject()]
    public class DequeueByLeavePartyResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(DequeueByLeavePartyResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public ulong DequeueMember;
        [Key(2)]
        public ErrorCode ErrorCode;
    }

    //소셜서버 -> 매치 서버로 매치 엔티티에 플레이어를 넣어달라고 요청
    [MessagePackObject()]
    public class JoinMatchRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(JoinMatchRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public int PartySerial;
        [Key(2)]
        public List<ulong> Members;
        [Key(3)]
        public MatchPlayer MatchPlayer;
    }
    [MessagePackObject()]
    public class JoinMatchResponse: ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(JoinMatchResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public int PartySerial;
        [Key(2)]
        public ErrorCode ErrorCode;
    }
    [MessagePackObject()]
    public class PartySerialAndCodeRequest : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(PartySerialAndCodeRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
    }

    [MessagePackObject()]
    public class PartySerialAndCodeResponse : ISequentialProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(PartySerialAndCodeResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int Serial { get; set; }
        [Key(1)]
        public int PartySerial;
        [Key(2)]
        public string PartyCode;
    }
    #endregion

    #region SocialToMatch
    [MessagePackObject()]
    public class DequeueByBreakParty : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(DequeueByBreakParty));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public List<ulong> Members;
    }

    [MessagePackObject()]
    public class CreateMatchRequest : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(CreateMatchRequest));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int PartySerial;
        [Key(1)]
        public List<OidAndNickname> PartyMembers;
        [Key(2)]
        public int ChapterId;
        [Key(3)]
        public bool IsPublicParty;
    }
    [MessagePackObject()]
    public class ChangeAllReady : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(ChangeAllReady));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public ulong OidAccount;
        [Key(1)]
        public int PartySerial;
        [Key(2)]
        public bool IsAllReady;
        [Key(3)]
        public bool IsMaxMemberCount;
    }
    #endregion

    #region MatchToSocial
    [MessagePackObject()]
    public class PartyStateChange : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(PartyStateChange));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public int PartySerial;
        [Key(1)]
        public Party.State State;
    }
    [MessagePackObject()]
    public class PartyKickMemberFromServer : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(PartyKickMemberFromServer));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public ulong OidAccount;
        [Key(1)]
        public int PartySerial;
    }
    [MessagePackObject()]
    public class CreateMatchResponse : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(CreateMatchResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int PartySerial;
        [Key(1)]
        public Dash.Protocol.Match Match;
        [Key(2)]
        public ErrorCode ErrorCode;
    }
    [MessagePackObject()]
    public class ChangeAllReadyResponse : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode("server_dash." + nameof(ChangeAllReadyResponse));
        public int GetTypeCode() => TypeCode;
        [Key(0)]
        public int PartySerial;
    }
    #endregion
}
