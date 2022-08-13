using System;
using Dash.Protocol;
using Dash.Types;
using server_dash.Net.Handlers;
using server_dash.Net.Sessions;

namespace server_dash.Battle.Services
{
    public class StatusService : AbstractService
    {
        [BindMessage(ServiceAreaType.Client, ServiceAreaType.Dummy)]
        public void Ping(ISession session, Ping message)
        {
            message.ResponseTimestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            session.Write(message);
        }
    }
}