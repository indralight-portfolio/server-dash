using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dash;
using Dash.Types;
using DotNetty.Transport.Channels;

namespace Dash.Net.Sessions
{
    public interface ISessionManager
    {
        void AddSession(ulong oidAccount, IConnectorSession session);
        void RemoveSession(IConnectorSession session);
    }

    public interface ISessionHandler : IChannelHandler
    {
        void SetAsDuplicated();
    }

    public class SessionManager : ISessionManager
    {
        private static readonly Utility.LogicLogger _logger = new Utility.LogicLogger();

        public int ConcurrentUser => (int)Interlocked.Read(ref _concurrentUserUnsafe);

        public readonly ulong MyOid;
        private readonly ConcurrentDictionary<ulong, IConnectorSession> _sessions = new ConcurrentDictionary<ulong, IConnectorSession>();
        private readonly ServiceAreaType _serviceAreaType;

        private long _concurrentUserUnsafe = 0;

        public SessionManager(ServiceAreaType serviceAreaType, ulong myOid)
        {
            _serviceAreaType = serviceAreaType;
            MyOid = myOid;
        }

        public void AddSession(ulong oidAccount, IConnectorSession session)
        {
            if (_sessions.TryGetValue(oidAccount, out IConnectorSession existSession) == true)
            {
                _logger.Error($"[{_serviceAreaType}][{oidAccount.LogOid()}][{existSession}] session exist.");
                existSession.Channel.Pipeline.Get<ISessionHandler>()?.SetAsDuplicated();
                existSession.CloseAsync();
                RemoveSession(existSession);
            }
            else
            {
                _logger.Info($"[{_serviceAreaType}]{oidAccount.LogOid()} session added.");
            }

            _sessions.AddOrUpdate(oidAccount, session, (key, value) => session);
        }

        public void RemoveSession(IConnectorSession session)
        {
            // out is an useless variable.
            if (_sessions.TryRemove(session.Oid, out _))
            {
                _logger.Info(_serviceAreaType, session, "session have removed.");
                
            }
            else
            {
                _logger.Info(_serviceAreaType, session, "session already removed maybe.");
            }
        }

        //현재는 서버랑 클라이언트랑 같이 연결되어있으면 oidAccount가 겹칠수 있다 
        public bool GetSession(ulong oidAccount, out IConnectorSession session)
        {
            return _sessions.TryGetValue(oidAccount, out session);
        }

        public List<ulong> GetAllSessionIds()
        {
            return _sessions.Keys.ToList();
        }
        public async Task Broadcast(ulong exceptOid, Protocol.IProtocol message)
        {
            var enumerator = _sessions.GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext())
            {
                if (exceptOid == enumerator.Current.Key)
                {
                    continue;
                }
                ++count;
                enumerator.Current.Value.Write(message);
                if (count % 10 == 0)
                {
                    await Task.Delay(10);
                }
            }
        }

        public int GetSessionsCount()
        {
            return _sessions.Count;
        }

        public void Clear()
        {
            _sessions.Clear();
        }

        public void PrintSessions()
        {

        }

        public int IncreaseUser()
        {
            return (int)Interlocked.Increment(ref _concurrentUserUnsafe);
        }

        public int DecreaseUser()
        {
            return (int)Interlocked.Decrement(ref _concurrentUserUnsafe);
        }
    }
}