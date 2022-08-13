using System;
using Dash.Net.Connectors;
using Dash.Types;
using DotNetty.Transport.Channels;

namespace Dash.Net.Sessions
{
    public class ListenerSession : AbstractSession, IListenerSession
    {
        public override ulong OwnerOid => ServerConnector.MyOid;
        public IServerConnector ServerConnector { get; }

        public ListenerSession(IServerConnector connector, ServiceAreaType areaType, IChannel channel) : base(channel)
        {
            ServerConnector = connector;
            AreaType = areaType;
        }
    }
}