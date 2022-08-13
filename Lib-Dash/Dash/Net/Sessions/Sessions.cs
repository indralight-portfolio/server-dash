using System;
using System.Threading.Tasks;
using Dash.Net.Connectors;
using Dash.Protocol;
using Dash.Types;
using DotNetty.Transport.Channels;

namespace Dash.Net.Sessions
{
    public interface ISession : ILoggable
    {
        SessionStatus Status { get; }
        ulong Oid { get; }
        ulong OwnerOid { get; }
        IChannel Channel { get; }
        bool InSafeFence { get; }
        bool IsAlive { get; set; }
        string Version { get; }
        string Revisions { get; }
        int ServiceExecutorId { get; }
        ServiceAreaType AreaType { get; }
        System.DateTime LatestReceiveAliveTime { get; set; }

        void SetOid(ulong oid);
        void Write(IProtocol message, bool flush = true);
        void Flush();
        bool TryHandleResponse(ISequentialProtocol message);
        Task<TResponse> WriteAndGetResponse<TResponse>(ISequentialProtocol message, TimeSpan timeout = default) where TResponse : IProtocol;
    }

    public interface IListenerSession : ISession
    {
        IServerConnector ServerConnector { get; }
    }

    public interface IConnectorSession : ISession
    {
        SessionManager SessionManager { get; }
        Task CloseAsync();
    }
}