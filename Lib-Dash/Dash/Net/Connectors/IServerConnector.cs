using System.Threading.Tasks;
using Dash.Net.Sessions;
using Dash.Protocol;

namespace Dash.Net.Connectors
{
    public interface IServerConnector
    {
        bool IsConnected { get; }
        ulong MyOid { get; }
        ISession GetSession();
        Task ShutDownAsync();
    }
}