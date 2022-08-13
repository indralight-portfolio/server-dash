using Dash.Net;
using Dash.Protocol;
using DotNetty.Transport.Channels;
using server_dash.Net.Handlers;
using server_dash.Battle.Services.Entities;
using server_dash.Battle.Services.GamePlay;

namespace server_dash.Net.Sessions
{
    public interface ISession
    {
        IChannel Channel { get; }

        void Write(IProtocol message, bool flush = true);

        ulong OidAccount { get; }
        bool InSafeFence { get; }
        bool IsAlive { get; set; }
        string Version { get; }
        string Revisions { get; }
        int ServiceExecutorId { get; }
        Dash.Types.ServiceAreaType AreaType { get; } //GameSession은 Client, ServerSession은 각 서비스에 맞게. 
        System.Threading.Tasks.Task CloseAsync();

    }
}