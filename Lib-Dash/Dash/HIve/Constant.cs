using Dash.Types;

namespace Dash.Hive
{
    public static class Constant
    {
        //public const string APP_ID = "";
        public const string APP_ID_GO = "";
        public const string APP_ID_AP = "";

        public const string CERT_KEY = "";

        public static class Timezone
        {
            public const string REAL_URL = "https://timezone.qpyou.cn";
            public const string SANDBOX_URL = "https://sandbox-timezone.qpyou.cn";
        }

#if Common_Server
        public static class Auth
        {
            public const string REAL_URL = "https://auth.qpyou.cn";
            public const string DIST_URL = "https://auth.globalwithhive.com";
            public const string SANDBOX_URL = "https://sandbox-auth.qpyou.cn";
        }

        public static class IAP
        {
            public const string REAL_URL = "https://hiveiap-verify.qpyou.cn";
            public const string SANDBOX_URL = "https://sandbox-hiveiap-verify.qpyou.cn";
        }

        public static class Analytics
        {
            public const string REAL_URL = "https://analytics-log.withhive.com/v1/server-recv";
            public const string SANDBOX_URL = "https://sandbox-analytics-log.withhive.com/v1/server-recv";
        }
#endif
    }

    public static class Helper
    {
#if Common_Unity
        public static string GetAppId()
        {
#if UNITY_IOS
            return Constant.APP_ID_AP;
#else
            return Constant.APP_ID_GO;
#endif
        }
#endif

#if Common_Server
        public static string GetAppId(MarketType marketType)
        {
            switch (marketType)
            {
                case MarketType.AppleAppStore:
                    return Constant.APP_ID_AP;
                case MarketType.GooglePlay:
                default:
                    return Constant.APP_ID_GO;
            }
        }        
        public static string GetMarketString(MarketType marketType)
        {
            switch (marketType)
            {
                case MarketType.AppleAppStore:
                    return "AP";
                case MarketType.GooglePlay:
                default:
                    return "GO";
            }
        }
#endif
    }
}