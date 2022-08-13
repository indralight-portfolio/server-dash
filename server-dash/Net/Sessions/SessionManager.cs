using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dash;
using Dash.Types;

namespace server_dash.Net.Sessions
{
    public interface ISessionManager
    {
        void AddSession(ulong oidAccount, ISession session);
        void RemoveSession(ISession session);
    }
    public class SessionManager
    {
        private ConcurrentDictionary<ulong, ISession> _sessions = new ConcurrentDictionary<ulong, ISession>();
        private readonly Utility.LogicLogger _logger = new Utility.LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());
        private ServiceAreaType _serviceAreaType;
        public int ConcurrentUser = 0;
        public SessionManager(ServiceAreaType serviceAreaType)
        {
            _serviceAreaType = serviceAreaType;
        }

        public void AddSession(ulong oidAccount, ISession session)
        {
            if (_sessions.TryGetValue(oidAccount, out ISession existSession) == true)
            {
                _logger.Error($"[{_serviceAreaType}]{oidAccount.LogOid()} session exist.");
                existSession.CloseAsync();
            }
            else
            {
                _logger.Info($"[{_serviceAreaType}]{oidAccount.LogOid()} session added.");
            }

            _sessions.AddOrUpdate(oidAccount, session, (key, value) => session);
        }

        public void RemoveSession(ISession session)
        {
            // out is an useless variable.
            if (_sessions.TryRemove(session.OidAccount, out _))
            {
                _logger.Info(_serviceAreaType, session, "session have removed.");
                
            }
            else
            {
                _logger.Info(_serviceAreaType, session, "session already removed maybe.");
            }
        }

        //현재는 서버랑 클라이언트랑 같이 연결되어있으면 oidAccount가 겹칠수 있다 
        public bool GetSession(ulong oidAccount, out ISession session)
        {
            return _sessions.TryGetValue(oidAccount, out session);
        }

        public List<ulong> GetAllSessionIds()
        {
            return _sessions.Keys.ToList();
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
    }
}