using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dash.Types;
using Newtonsoft.Json;

namespace server_dash.Statistics
{
    public interface IPayload
    {
    }

    public interface IDeltaPayload : IPayload
    {
        int GetDelta();
    }

    #region Service
    public class CreatePayload : IPayload
    {
        public string Country;
    }
    public class LogInPayload : IPayload
    {
        public string Country;
        public DateTime Created;
        public bool IsNew => Created > DateTime.UtcNow.Date;
    }
    public class ChangeValuePayload : IDeltaPayload
    {
        public int GetDelta() => Delta;

        public string Reason;
        public int Delta;
        [JsonProperty("OldVal")]
        public int OldValue;
        [JsonProperty("NewVal")]
        public int NewValue;
    }
    public class ChangeExpPayload : IPayload
    {
        public int GetDelta() => DeltaExp;

        public string Reason;
        public int DeltaExp;
        public int OldExp;
        public int NewExp;
        public int OldLevel;
        public int NewLevel;
    }
    public class ChangeConsumePayload : IPayload
    {
        public int GetDelta() => DeltaCount;

        public string Reason;
        public int ItemId;
        public int DeltaCount;
        public int OldCount;
        public int NewCount;
    }
    public class ChangeBoxKeyPayload : IPayload
    {
        public int GetDelta() => DeltaCount;

        public string Reason;
        public int BoxId;
        public int DeltaCount;
        public int OldCount;
        public int NewCount;
    }
    public class EquipmentItemGainPayload : IPayload
    {
        public string Reason;
        public uint Serial;
        public int ItemId;
        public string ItemGrade;
        public int Level;
    }
    public class ShopBuyPayload : IPayload
    {
        public int ProductId;
        public string PaymentType;
        public int Price;
    }

    public class BoxOpenPayLoad : IPayload
    {
        public int BoxId;
        public Dash.StaticData.RewardInfo RewardInfo;
    }
    #endregion

    #region Battle
    
    #endregion
}
