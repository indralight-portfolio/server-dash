using MessagePack;

namespace Dash
{
    [MessagePackObject()]
    public struct SuspectInfo
    {
        [Key(0)]
        public ulong OidAccount;
        [Key(1)]
        public string Description;

        public override string ToString()
        {
            return $"<<<{OidAccount.LogOid()} {Description}>>>";
        }
    }
}