using System;
using System.Threading.Tasks;
using Dash.Net;
using Dash.Protocol;
using Dash.Types;
using DotNetty.Transport.Channels;

namespace server_dash.Net.Sessions
{
    public abstract class AbstractSession : ISession, IEquatable<ISession>
    {
        protected readonly Utility.LogicLogger _logger = new Utility.LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());

        public IChannel Channel { get; }
        private readonly SendQueue _sendQueue;

        protected AbstractSession(IChannel channel)
        {
            this.Channel = channel;
            this.IsAlive = true;
            _sendQueue = new SendQueue(channel);
        }

        public bool IsAlive { get; set; }

        public string Version { get; set; }
        public string Revisions { get; set; }
        public ulong OidAccount { get; set; }

        public virtual bool InSafeFence { get; }

        public virtual int ServiceExecutorId { get; }
        public virtual ServiceAreaType AreaType { get; protected set; }

        public bool Equals(ISession other)
        {
            if (other == null) return false;

            return this.Channel.Id.CompareTo(other.Channel.Id) == 0
                   && this.OidAccount == other.OidAccount;
        }

        public void Write(IProtocol message, bool flush = true)
        {
            if (IsAlive == false || Channel == null)
            {
                return;
            }

            _sendQueue.Send(message, flush);
        }

        public async System.Threading.Tasks.Task CloseAsync()
        {
            await Channel?.CloseAsync();
        }
        
    }
    public class DummySession : AbstractSession
    {
        public DummySession(IChannel channel) : base(channel)
        {
        }
        public override bool InSafeFence { get { return true; } }
        public override ServiceAreaType AreaType { get => ServiceAreaType.Dummy; }
    }
}