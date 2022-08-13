#if Common_NetCore
using Common.Utility;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;

namespace Common.NetCore.Web
{
    public class HttpUtility
    {
        public static string GetRequestHeader(HttpRequest request, string key)
        {
            return TypeUtility.Convert<string>(request.Headers[key].ToString());
        }

        public static string GetClientIP(HttpContext context)
        {
            string ip = GetRequestHeader(context.Request, "X-Forwarded-For");
            if (string.IsNullOrEmpty(ip) || string.Equals(ip, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                ip = GetRequestHeader(context.Request, "Proxy-Client-IP");
            }
            if (string.IsNullOrEmpty(ip) || string.Equals(ip, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                ip = GetRequestHeader(context.Request, "WL-Proxy-Client-IP");
            }
            if (string.IsNullOrEmpty(ip) || string.Equals(ip, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                ip = GetRequestHeader(context.Request, "HTTP_CLIENT_IP");
            }
            if (string.IsNullOrEmpty(ip) || string.Equals(ip, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                ip = GetRequestHeader(context.Request, "HTTP_X_FORWARDED_FOR");
            }
            if (string.IsNullOrEmpty(ip) || string.Equals(ip, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                IPAddress ipAddress = context.Connection.RemoteIpAddress;
                if (IPAddress.IsLoopback(ipAddress))
                    ipAddress = IPAddress.Loopback;

                ip = ipAddress.MapToIPv4().ToString();
            }

            return ip;
        }
    }
}
#endif