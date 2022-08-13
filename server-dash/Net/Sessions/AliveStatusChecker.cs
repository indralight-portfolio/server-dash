using System.Collections.Generic;
using System.Linq;
using server_dash.Net.Sessions;

namespace server_dash.Net.Sessions
{
    public interface IAliveStatusListener<TSession, TContext>
    where TSession : class, ISession
    where TContext : ServiceExecutionContext<TSession>
    {
        bool KeepListening { get; }

        void OnSessionBecomeAlive(TSession session);
        void OnSessionNotAlive(ulong oidAccount, int notAliveCount, TContext context);
        void OnSessionNeedDrop(ulong oidAccount, TContext context);
    }

    public readonly struct AliveStatusCheckConfig
    {
        public AliveStatusCheckConfig(long checkIntervalMs, long notAliveThresholdMs, long dropPlayerThresholdMs)
        {
            CheckIntervalMs = checkIntervalMs;
            NotAliveThresholdMs = notAliveThresholdMs;
            DropPlayerThresholdMs = dropPlayerThresholdMs;
        }

        public readonly long CheckIntervalMs;
        public readonly long NotAliveThresholdMs;
        public readonly long DropPlayerThresholdMs;
    }

    /// <summary>
    /// Session의 상태를 Listener에게 보고.
    /// Session 객체를 Key로 잡지 않는다.
    /// 재연결등의 사유로 Session 객체가 바뀌었을 수 있다.
    /// </summary>
    public class AliveStatusChecker<TSession, TContext>
    where TSession : class, ISession
    where TContext : ServiceExecutionContext<TSession>
    {
        private readonly TContext _executionContext;
        private readonly IAliveStatusListener<TSession, TContext> _listener;
        private readonly AliveStatusCheckConfig _config;

        private List<ulong> _targetOids = new List<ulong>();
        private Dictionary<ulong, long> _aliveTimes = new Dictionary<ulong, long>();
        private Dictionary<ulong, int> _becomeNotAliveCount = new Dictionary<ulong, int>();
        private HashSet<ulong> _notAliveSessions = new HashSet<ulong>();
        private long _lastCheckTimeMs;

        public AliveStatusChecker(TContext executionContext, IAliveStatusListener<TSession, TContext> listener, AliveStatusCheckConfig config)
        {
            _executionContext = executionContext;
            _listener = listener;
            _config = config;

            RefreshList();
        }

        public void OnMarkAlive(TSession session)
        {
            ulong oidAccount = session.OidAccount;
            if (_aliveTimes.ContainsKey(oidAccount) == false)
            {
                _aliveTimes.Add(oidAccount, _executionContext.ElapsedMillisecondsUnsafe);
            }
            else
            {
                _aliveTimes[oidAccount] = _executionContext.ElapsedMillisecondsUnsafe;
            }


            if (_notAliveSessions.Contains(oidAccount) == true)
            {
                _notAliveSessions.Remove(oidAccount);
                _listener.OnSessionBecomeAlive(session);
            }
        }
        public void OnStartCheckAlive(ulong oidAccount)
        {
            if (_aliveTimes.ContainsKey(oidAccount) == false)
            {
                _aliveTimes.Add(oidAccount, _executionContext.ElapsedMillisecondsUnsafe);
            }
            else
            {
                _aliveTimes[oidAccount] = _executionContext.ElapsedMillisecondsUnsafe;
            }


            if (_notAliveSessions.Contains(oidAccount) == true)
            {
                _notAliveSessions.Remove(oidAccount);
                OnSessionBecomeAlive(oidAccount);
            }
        }

        public void Update(long elapsedMilliseconds)
        {
            if (elapsedMilliseconds - _lastCheckTimeMs < _config.CheckIntervalMs)
            {
                return;
            }

            if (_listener.KeepListening == false)
            {
                return;
            }

            _lastCheckTimeMs = elapsedMilliseconds;
            foreach (ulong oidAccount in _targetOids)
            {
                if (_aliveTimes.TryGetValue(oidAccount, out long sessionMarkTimeMs) == false)
                {
                    continue;
                }

                long elapsedSinceAliveMs = elapsedMilliseconds - sessionMarkTimeMs;

                // not alive
                if (_notAliveSessions.Contains(oidAccount) == false &&
                    elapsedSinceAliveMs > _config.NotAliveThresholdMs)
                {
                    ++_becomeNotAliveCount[oidAccount];
                    _notAliveSessions.Add(oidAccount);
                    _listener.OnSessionNotAlive(oidAccount, _becomeNotAliveCount[oidAccount], _executionContext);
                }
                // drop player
                else if (elapsedSinceAliveMs > _config.DropPlayerThresholdMs)
                {
                    _aliveTimes.Remove(oidAccount);
                    _listener.OnSessionNeedDrop(oidAccount, _executionContext);
                }
            }

            // Session 삭제 / 추가 대응
            RefreshList();
        }

        private void RefreshList()
        {
            if (_targetOids != null && _targetOids.Count == _executionContext.GetOidAccounts.Count)
            {
                return;
            }

            var prevList = _targetOids;
            var newList = _executionContext.GetOidAccounts;

            if (prevList != null)
            {
                foreach (ulong oidAccount in prevList)
                {
                    if (newList.Contains(oidAccount) == false)
                    {
                        // 삭제된 경우
                        _aliveTimes.Remove(oidAccount);
                        _notAliveSessions.Remove(oidAccount);
                        _becomeNotAliveCount.Remove(oidAccount);
                    }
                }
            }

            long currentTime = _executionContext.ElapsedMillisecondsUnsafe;

            foreach (ulong oidAccount in newList)
            {
                if (prevList == null || prevList.Contains(oidAccount) == false)
                {
                    // 추가된 경우
                    _aliveTimes[oidAccount] = currentTime;
                    _becomeNotAliveCount[oidAccount] = 0;
                }
            }
            _targetOids.Clear();
            _targetOids.AddRange(newList);
        }
        private void OnSessionBecomeAlive(ulong oidAccount)
        {

        }
    }
}