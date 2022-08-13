using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Common.Utility
{
    public class NetUtility
    {
        public static EndPoint GetEndPoint(string endpoint)
        {
            string pattern_ip = @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d{1,5})$";
            string pattern_dns = @"^(.*):(\d{1,5})$";
            var regex_ip = new Regex(pattern_ip);
            var regex_dns = new Regex(pattern_dns);
            if (regex_ip.IsMatch(endpoint))
            {
                var match = regex_ip.Match(endpoint);
                var ip = match.Groups[1].Value;
                var port = match.Groups[2].Value;
                return new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            }
            if (regex_dns.IsMatch(endpoint))
            {
                var match = regex_dns.Match(endpoint);
                var host = match.Groups[1].Value;
                var port = match.Groups[2].Value;
                return new DnsEndPoint(host, int.Parse(port));
            }
            return null;
        }

        public static bool IsLocalIP(string ipAddress)
        {
            try
            { // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(ipAddress);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool IsPrivateIP(string ipAddress)
        {
            int[] ipParts = ipAddress.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => int.Parse(s)).ToArray();
            // in private ip range
            if (ipParts[0] == 10 ||
                (ipParts[0] == 192 && ipParts[1] == 168) ||
                (ipParts[0] == 172 && (ipParts[1] >= 16 && ipParts[1] <= 31)))
            {
                return true;
            }

            // IP Address is probably public.
            // This doesn't catch some VPN ranges like OpenVPN and Hamachi.
            return false;
        }
        public static string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "";
        }
    }
}
