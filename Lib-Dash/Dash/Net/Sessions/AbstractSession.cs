using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Dash.Net;
using Dash.Protocol;
using Dash.Types;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace Dash.Net.Sessions
{
    public abstract class AbstractSession : ISession, IEquatable<ISession>
    {
        protected static readonly Utility.LogicLogger _logger = new Utility.LogicLogger();

        public IChannel Channel { get; }
        public SessionStatus Status { get; } = SessionStatus.Undefined; // TODO: Not implemented
        public bool IsAlive { get; set; }
        public string Version { get; set; }
        public string Revisions { get; set; }
        public ulong Oid { get; set; }
        public abstract ulong OwnerOid { get; }
        public void SetOid(ulong value) => Oid = value; 
        public virtual bool InSafeFence { get; }
        public virtual int ServiceExecutorId { get; }
        public virtual ServiceAreaType AreaType { get; protected set; }

        public DateTime LatestReceiveAliveTime { get; set; }

        private readonly SendQueue _sendQueue;
        private readonly ResponseMessageController _responseMessageController;

        protected AbstractSession(IChannel channel)
        {
            this.Channel = channel;
            this.IsAlive = true;
            _sendQueue = new SendQueue(channel);
            _responseMessageController = new ResponseMessageController(this, _sendQueue);
        }

        public string LogStr => ToString();

        public bool Equals(ISession other)
        {
            if (other == null) return false;

            return this.Channel.Id.CompareTo(other.Channel.Id) == 0
                   && this.Oid == other.Oid;
        }

        public void Write(IProtocol message, bool flush = true)
        {
            if (IsAlive == false || Channel == null)
            {
                return;
            }

            _sendQueue.Send(message, flush);
        }

        public void Flush()
        {
            _sendQueue.Flush();
        }

        public bool TryHandleResponse(ISequentialProtocol message)
        {
            if (Oid == message.Header.OidRequester)
            {
                return false;
            }

            return _responseMessageController.TryHandleResponse(message);
        }

        public async Task<TResponse> WriteAndGetResponse<TResponse>(ISequentialProtocol message, TimeSpan timeout = default) where TResponse : IProtocol
        {
            ulong myOid = OwnerOid;
            if (myOid == 0)
            {
                throw new Exception($"{this} MyOid is 0!");
            }

            var response = await _responseMessageController.GetResponse(message, myOid, timeout);
            return (TResponse) response;
        }
    }

    public class DummySession : AbstractSession, IConnectorSession
    {
        public DummySession(SessionManager sessionManager, IChannel channel) : base(channel)
        {
            SessionManager = sessionManager;
        }

        public override ulong OwnerOid => SessionManager.MyOid;
        public override bool InSafeFence => true;
        public override ServiceAreaType AreaType => ServiceAreaType.Dummy;

        public override string ToString()
        {
            if (Channel?.Active == true)
            {
                return $"{Oid.LogOid()}";
            }

            return Channel?.ToString();
        }

        #region IConnectorSession
        public SessionManager SessionManager { get; }

        public async Task CloseAsync()
        {
            await Channel.CloseAsync();
        }
        #endregion
    }
}