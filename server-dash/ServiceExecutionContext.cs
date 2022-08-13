using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Dash.Protocol;
using server_dash.Net.Sessions;

namespace server_dash
{
    public interface IServiceExecutionContext
    {
        int ServiceExecutorId { get; }
        int ManagedThreadId { get; }
    }

    public abstract class ServiceExecutionContext<TSession> : IServiceExecutionContext
        where TSession : class, ISession
    {
        public int ServiceExecutorId { get; }
        public int ManagedThreadId { get; }
        public ReadOnlyCollection<TSession> GetSessions => Sessions.AsReadOnly();
        public ReadOnlyCollection<ulong> GetOidAccounts => OidAccounts.AsReadOnly();
        protected readonly List<TSession> Sessions = new List<TSession>();
        protected readonly List<ulong> OidAccounts = new List<ulong>();

        // 모든 쓰레드에서 사용 가능.
        public long ElapsedMillisecondsSafe => Interlocked.Read(ref _elapsedMillisecondsSafe);
        private long _elapsedMillisecondsSafe;

        // ServiceExecute Thread에서만 사용해야 함.
        public long ElapsedMillisecondsUnsafe => _elapsedMilliseconds;
        private long _elapsedMilliseconds;

        private readonly Stopwatch _startTimer = new Stopwatch();

        protected abstract long UpdateIntervalMs { get; }
        private long _lastUpdateTickCount;


        protected ServiceExecutionContext(int serviceExecutorId, int managedThreadId)
        {
            ServiceExecutorId = serviceExecutorId;
            ManagedThreadId = managedThreadId;
        }

        public void AssertThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != ManagedThreadId)
            {
                throw new Exception($"[{this}] Validate Thread failed, Expected : {ManagedThreadId}, Actual : {Thread.CurrentThread.ManagedThreadId}");
            }
        }
        public void AddSession(TSession session)
        {
            Sessions.Add(session);
            AddOidAccount(session.OidAccount);
        }
        public void AddOidAccount(ulong oidAccount)
        {
            if (OidAccounts.Contains(oidAccount) == false)
            {
                OidAccounts.Add(oidAccount);
            }
        }
        public void RemoveSession(TSession session)
        {
            Sessions.Remove(session);
            RemoveOidAccount(session.OidAccount);
        }
        public void RemoveOidAccount(ulong oidAccount)
        {
            OidAccounts.Remove(oidAccount);
        }

        public void ClearSession()
        {
            Sessions.Clear();
            OidAccounts.Clear();
        }
        
        public TSession FindSessionByOidAccount(ulong oidAccount)
        {
            return Sessions.Find(s => s.OidAccount == oidAccount);
        }
        public TSession GetSession(Predicate<TSession> c)
        {
            return Sessions.Find(c);
        }
        public bool SessionsTrueForAll(Predicate<TSession> c)
        {
            return Sessions.TrueForAll(c);
        }
        protected void StartElapsedTimer()
        {
            _startTimer.Start();
        }

        public void OnUpdate(long tickCount)
        {
            if (tickCount - _lastUpdateTickCount < UpdateIntervalMs)
            {
                return;
            }

            _lastUpdateTickCount = tickCount;

            _elapsedMilliseconds = _startTimer.ElapsedMilliseconds;
            Interlocked.Exchange(ref _elapsedMillisecondsSafe, _elapsedMilliseconds);

            AssertThread();
            DoUpdate();
        }

        protected abstract void DoUpdate();


        public virtual void Broadcast(IProtocol message, TSession exceptSession = null, bool flush = true)
        {
            for (int i = 0; i < Sessions.Count; ++i)
            {
                if (Sessions[i] == exceptSession)
                {
                    continue;
                }

                Sessions[i].Write(message, flush);
            }
        }
    }
}