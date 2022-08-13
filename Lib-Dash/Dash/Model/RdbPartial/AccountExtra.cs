using Common.Utility;
using Dash.Protocol;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
#if Common_Server
#endif

namespace Dash.Model.Rdb
{
    public partial class AccountExtra
    {
        public AccountExtra(ulong oidAccount, ExtraLoginInfo extraLoginInfo, string clientIp)
        {
            OidAccount = oidAccount;
            Market = extraLoginInfo.Market.ToString();
            Device = extraLoginInfo.Device;
            OsVersion = extraLoginInfo.OsVersion;
            AppVersion = extraLoginInfo.AppVersion;
            DeviceId = extraLoginInfo.DeviceId;
            Language = extraLoginInfo.Language;
            ClientIp = clientIp;
        }

        public AccountExtra(ulong oidAccount, AccountExtra other) : this(other)
        {
            OidAccount = oidAccount;
        }

        [IgnoreMember, JsonIgnore]
        public MarketType MarketType
        {
            get
            {
                EnumInfo<MarketType>.TryParse(Market, out var marketType);
                return marketType;
            }
        }
    }
}
