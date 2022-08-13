#if Common_Server

namespace Dash.Server.Statistics
{
    public interface IPayload
    {
    }

    public struct OldNewDelta
    {
        public long Old;
        public long New;
        public long Delta;

        public OldNewDelta(long old, long @new)
        {
            Old = old;
            New = @new;
            Delta = @new - old;
        }
    }

    public class CreatePayload : IPayload
    {
        public string Country;
    }
    public class LogInPayload : IPayload
    {
        public string Country;
        public bool IsNew;
        public bool IsReturn;
    }
    public class ResetPayload : IPayload
    {
        public ulong NewOidAccount;
        public string AuthId;
    }
    public class HiveDeletePayload : IPayload
    {
        public string AuthId;
    }

    public class MoneyChangePayload : IPayload
    {
        public string Reason;
        public string Type;
        public OldNewDelta Value;
        public OldNewDelta Free;
        public OldNewDelta Paid;
    }
    public class ChangeAccountPayload : IPayload
    {
        public string Reason;
        public OldNewDelta Exp;
        public OldNewDelta Level;
    }
    public class ConsumeChangePayload : IPayload
    {
        public string Reason;
        public int ItemId;
        public OldNewDelta Count;
    }
    public class EquipmentChangePayload : IPayload
    {
        public string Reason;
        public uint Serial;
        public int ItemId;
        public string Rarity;
        public string SlotType;
        public OldNewDelta Exp;
        public OldNewDelta Level;
        public OldNewDelta Overcome;
        public OldNewDelta Reforge;
        public bool IsGain;
        public bool IsLose;
    }
    public class CharacterChangePayload : IPayload
    {
        public string Reason;
        public int CharacterId;
        public string Rarity;
        public OldNewDelta Exp;
        public OldNewDelta Level;
        public OldNewDelta Overcome;
        public OldNewDelta Rank;
        public bool IsGain;
    }
    public class SeasonPassChangePayload : IPayload
    {
        public string Reason;
        public int Id;
        public OldNewDelta Value;
    }
    public class ShopBuyPayload : IPayload
    {
        public int ProductId;
        public string PaymentType;
        public int Price;
    }
    public class ConquestScorePayload : IPayload
    {
        public int SeasonId;
        public uint Score;
        public int Tier;
        public string Deck;
    }
    public class WorldMissionScorePayload : IPayload
    {
        public int EventId;
        public uint Score;
        public uint CoopScore;
        public string Deck;
    }
}
#endif