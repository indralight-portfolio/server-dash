using Dash.Model;
using server_dash.Internal.Services;

namespace server_dash.Match.Services
{
    public class MonitorService : AbstractMonitorService<MatchDesc>
    {
        public MonitorService(string uuid, MatchServerConfig config) : base(uuid, config.WebHostPort)
        {
        }
    }
}