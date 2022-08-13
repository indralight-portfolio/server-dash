using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dash;
using Dash.Net;
using Dash.Types;
using DotNetty.Transport.Channels;
using server_dash.Social.Party;

namespace server_dash.Net.Sessions
{
    public class SocialSession : AbstractSession
    {
        public SocialSession(IChannel channel) : base(channel)
        {
            this.PartyContext = null;
        }
        public PartyContext PartyContext { get; set; }
        public override string ToString()
        {
            if (Channel != null && Channel.Active == true)
            {
                return $"{OidAccount.LogOid()}{Channel.Log()}";
            }

            return OidAccount.LogOid();
        }

        public override bool InSafeFence => PartyContext == null;
        
        public override int ServiceExecutorId => PartyContext?.Party.Serial ?? 0;
        public override ServiceAreaType AreaType { get => ServiceAreaType.Client; }
    }
}
