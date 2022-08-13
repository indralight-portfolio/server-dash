using Dash.Types;
using DotNetty.Transport.Channels;
using server_dash.Battle.Services.Entities;
using server_dash.Battle.Services.GamePlay;

namespace server_dash.Net.Sessions
{
    public class PlayerSession : AbstractSession
    {
        public PlayerSession(IChannel channel) : base(channel)
        {
            ArenaContext = null;
        }
        public override string ToString()
        {
            if (Channel != null && Channel.Active == true)
            {
                return $"{Channel.ToString()}, {Player}";
            }

            return $"{Player}";
        }
        public bool InWaitingQueue { get; set; }

        public bool IsQuitArena { get; set; }

        public PlayerEntity Player { get; set; }

        public ArenaContext ArenaContext { get; set; }

        public override bool InSafeFence => ArenaContext == null;

        public override int ServiceExecutorId => ArenaContext?.Arena.Serial ?? 0;
        public override ServiceAreaType AreaType { get => ServiceAreaType.Client; }
    }
}