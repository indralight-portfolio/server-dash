using Dash.State;
using MessagePack;

namespace Dash.Model.GamePlay
{
    [MessagePackObject()]
    public class UndoneGame
    {
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode(nameof(UndoneGame));
        public int GetTypeCode() => TypeCode;

        public override string ToString()
        {
            return $"UndoneGame[{CoreState}:{Hash}:{Timestamp}:{PlayerId}:{DeckId}]";
        }

        [Key(0)] public CoreState CoreState;
        [Key(1)] public int Hash;
        [Key(2)] public long Timestamp;
        [Key(3)] public int PlayerId;
        [Key(4)] public byte DeckId;
    }
}