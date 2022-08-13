using Dash.Hive;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dash.Model.Service
{
    [Serializable]
    public enum Env
    {
        DEV,
        LOCAL,
        DEMO,
        TEST,
        ALPHA,
        C2S_TEST,
        C2S_QA,
        C2S_IOSQA,
        C2S_LIVE,

        // Deprecated
        QA = 999,
        LIVE,
    }

    [Serializable]
    public class HostInfo
    {
        public override string ToString()
        {
            var str = $"[HostInfo] Env : {Env} " +
                    $"AssetBundleCdnUrl : {AssetBundleCdnUrl}" +
                    $"LobbyServerUrl : {string.Join(",", LobbyServerUrls.Values.ToArray())}";
            return str;
        }

        public string Env;
        public string AssetBundleCdnUrl;
        public string ServiceStateUrl;
        public Dictionary<string, string> LobbyServerUrls;

        public string GetLobbyServerUrl(ServerZoneType serverZone)
        {
            LobbyServerUrls.TryGetValue(serverZone.toString(), out var url);
            return url;
        }
    }

    [Serializable]
    public class HostSelectInfo
    {
        [Serializable]
        public class Selector
        {
            public string Version { get; set; }
            public string Platform { get; set; } = "all";
            public string BundleVersion { get; set; }
            public string Env { get; set; }

            public bool Match(string version, string platform)
            {
                return version.Equals(Version) && (Platform.Equals("all") || Platform.Equals(platform));
            }
        }

        public Dictionary<string, HostInfo> HostInfos { get; set; }
        public List<Selector> Selectors { get; set; }
    }
}
