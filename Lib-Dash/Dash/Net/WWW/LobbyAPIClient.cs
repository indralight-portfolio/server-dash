#if Common_NetCore
using Common.Net.WWW;
using Dash.Model;

namespace Dash.Net.WWW
{
    public class LobbyAPIClient : WebAPIClient
    {
        public static LobbyAPIClient Instance { get; private set; }

        public static void Release()
        {
            Instance = null;
        }

        public static void Create(string hostUrl, string version)
        {
            Instance = new LobbyAPIClient();
            Instance.Init(hostUrl, version);
        }
    }
}
#endif