using System.Linq;

namespace Dash.Hive
{
    public static class IAPHelper
    {
#if Common_Unity
        public static int GetProductId(string maketPid)
        {
            System.Func<StaticData.Shop.ProductInfo, bool> func;
#if UNITY_IOS
            func = (e) => e.Pid_AP == maketPid;
#else
            func = (e) => e.Pid_GO == maketPid;
#endif
            var info = StaticInfo.StaticInfo.Instance.ProductInfo.GetList().FirstOrDefault(func);
            return info?.Id ?? 0;
        }
        public static string GetMarketPid(int productId)
        {
            StaticInfo.StaticInfo.Instance.ProductInfo.TryGet(productId, out var info);
#if UNITY_IOS
            return info?.Pid_AP;
#else
            return info?.Pid_GO;
#endif
        }
#endif

#if Common_Server
        public static int GetProductId(string maketPid)
        {
            System.Func<StaticData.Shop.ProductInfo, bool> func = (e) => e.Pid_GO == maketPid || e.Pid_AP == maketPid;
            var info = StaticInfo.StaticInfo.Instance.ProductInfo.GetList().FirstOrDefault(func);
            return info?.Id ?? 0;
        }
#endif
    }
}