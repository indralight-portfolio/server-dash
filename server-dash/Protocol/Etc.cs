using Dash.Protocol;
using MessagePack;

namespace server_dash.Protocol
{
    [MessagePackObject()]
    public class ReviveConfirm : IProtocol
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode(nameof(ReviveConfirm));
        public int GetTypeCode() => TypeCode;

        [Key(0)]
        public bool IsConfirm;
        [Key(1)]
        public int PlayerId;
    }
}