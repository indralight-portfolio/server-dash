using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dash.Types;
using DotNetty.Transport.Channels;

namespace server_dash.Net.Handlers
{
    public class ChannelManager
    {
        private class ChannelContext
        {
            public ulong OidAccount;
            public IChannel Channel;
            public System.DateTime RegisteredTime;
        }

        private static readonly Utility.LogicLogger _logger = new Utility.LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());
        private static readonly System.TimeSpan BecomeTrashChannelThreshold = System.TimeSpan.FromSeconds(60);
        private static readonly System.TimeSpan CloseTrashChannelsInterval = System.TimeSpan.FromSeconds(30);

        private const int Started = 1;
        private const int ShuttingDown = 2;
        private const int ShutDown = 3;

        private Dictionary<ulong, ChannelContext> _loggedInChannels = new Dictionary<ulong, ChannelContext>();
        private Dictionary<IChannelId, ChannelContext> _notLoggedInChannels = new Dictionary<IChannelId, ChannelContext>();
        private DotNetty.Common.Concurrency.XThread _thread;
        private volatile int _threadState;
        private TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();
        private ServiceAreaType _serviceAreaType;
        public ChannelManager(ServiceAreaType serviceAreaType)
        {
            _thread = new DotNetty.Common.Concurrency.XThread(Loop);
            _thread.Start();
            _threadState = Started;
            _serviceAreaType = serviceAreaType;
        }

        public Task ShutdownGracefullyAsync()
        {
            if (_threadState > Started)
            {
                return Task.CompletedTask;
            }

            int currentState = _threadState;
            if (_threadState == Started)
            {
                Interlocked.CompareExchange(ref _threadState, ShuttingDown, currentState);
            }

            return _taskCompletionSource.Task;
        }

        private void Loop()
        {
            System.TimeSpan checkInterval = System.TimeSpan.FromMilliseconds(1000);
            System.TimeSpan elapsed = new System.TimeSpan();
            while (_threadState == Started)
            {
                Thread.Sleep(checkInterval);
                elapsed = elapsed + checkInterval;
                if (elapsed > CloseTrashChannelsInterval)
                {
                    elapsed = new System.TimeSpan();
                    CloseTrashChannels();
                }
            }

           System.Console.WriteLine("ChannelManager Terminated.");
           Interlocked.Exchange(ref _threadState, ShutDown);
            _taskCompletionSource.SetResult(0);
        }

        public void AddChannel(IChannel channel)
        {
            lock (_notLoggedInChannels)
            {
                _notLoggedInChannels.Add(channel.Id, new ChannelContext()
                {
                    OidAccount = 0,
                    Channel = channel,
                    RegisteredTime = System.DateTime.UtcNow
                });
            }
        }

        public void RemoveChannel(IChannel channel, Sessions.ISession session)
        {
            // Session != null 인데, Player == null일 수 있음. 클라가 WhoAmI보내야 Player 생김.
            if (session == null)
            {
                lock (_notLoggedInChannels)
                {
                    bool removed = _notLoggedInChannels.Remove(channel.Id);
                }
            }
            else
            {
                ulong oidAccount = session.OidAccount;
                if (oidAccount != 0)
                {
                    lock (_loggedInChannels)
                    {
                        _loggedInChannels.Remove(oidAccount);
                    }
                }
            }
        }

        public void OnChannelLogin(IChannel channel, Sessions.ISession session)
        {
            ChannelContext context = null;
            lock (_notLoggedInChannels)
            {
                _notLoggedInChannels.TryGetValue(channel.Id, out context);
                if (context != null)
                {
                    _notLoggedInChannels.Remove(context.Channel.Id);
                }
            }

            if (context == null)
            {
                _logger.Error(_serviceAreaType, session, $"[ChannelManager] Not logined channel not found, Id : {channel.Id}");
                return;
            }

            lock (_loggedInChannels)
            {
                context.OidAccount = session.OidAccount;
                if (_loggedInChannels.ContainsKey(context.OidAccount) == true)
                {
                    _logger.Error(_serviceAreaType, session, $"[ChannelManager] already channel exist!, Id : {channel.Id}");
                    _loggedInChannels.Remove(context.OidAccount);
                }

                _loggedInChannels.Add(context.OidAccount, context);
            }
        }

        private void CloseTrashChannels()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int closedCount = 0;
            lock (_notLoggedInChannels)
            {
                System.DateTime now = System.DateTime.UtcNow;
                List<IChannelId> removed = new List<IChannelId>();
                foreach (KeyValuePair<IChannelId, ChannelContext> pair in _notLoggedInChannels)
                {
                    ChannelContext channel = pair.Value;
                    if (channel.RegisteredTime + BecomeTrashChannelThreshold < now)
                    {
                        _logger.Info($"[{_serviceAreaType}][ChannelManager] TrashChannel found. Id : {channel.Channel.Id}");
                        channel.Channel.CloseAsync();
                        removed.Add(pair.Key);
                    }
                }

                closedCount = removed.Count;
                for (int i = 0; i < removed.Count; ++i)
                {
                    _notLoggedInChannels.Remove(removed[i]);
                }
            }

            if (closedCount > 0)
            {
                _logger.Info($"[{_serviceAreaType}][ChannelManager] Close TrashChannels ended. Closed Count : {closedCount}, Elapsed : {sw.ElapsedMilliseconds} ms.");
            }
        }

        public int GetLoggedInChannelsCount()
        {
            lock (_loggedInChannels)
            {
                return _loggedInChannels.Count;
            }
        }

        public int GetNotLoggedInChannelsCount()
        {
            lock (_notLoggedInChannels)
            {
                return _notLoggedInChannels.Count;
            }
        }
    }
}