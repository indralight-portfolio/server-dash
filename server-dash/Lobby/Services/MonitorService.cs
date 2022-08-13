using Dash.Model;
using server_dash.Internal.Services;

namespace server_dash.Lobby.Services
{
    public class MonitorService : AbstractMonitorService<LobbyDesc>
    {
        public MonitorService(string uuid, LobbyServerConfig config) : base(uuid, config.Port)
        {
        }
    }
}