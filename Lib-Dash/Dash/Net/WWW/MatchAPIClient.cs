#if Common_NetCore
using Common.Net.WWW;
using Dash.Model;

namespace Dash.Net.WWW
{
    public class MatchAPIClient : WebAPIClient
    {
        public static MatchAPIClient Instance { get; private set; }

        public static void Release()
        {
            Instance = null;
        }

        public static void Create(string hostUrl, string version)
        {
            Instance = new MatchAPIClient();
            Instance.Init(hostUrl, version);
        }
    }
}
#endif