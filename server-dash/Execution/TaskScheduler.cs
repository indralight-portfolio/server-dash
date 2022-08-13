using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Dash.Net;
using Dash.Protocol;
using DotNetty.Common.Concurrency;
using NLog;
using server_dash.Execution.Runnable;
using server_dash.Net.Handlers;
using server_dash.Net.Sessions;

namespace server_dash.Execution
{
    public class TaskScheduler
    {
        public delegate void ScheduleAction(Action<ISession, IProtocol> messageDispatcher, Action<int, IRunnableMessage> executor);
        class ScheduledTask
        {
            public int EndTickCount = 0;
            public ScheduleAction Action = null;
        }

        // message를 handler마다 dispatch해야할 필요가 없는 단일 작업들은 아래 ExecuteTask를 생성하여 executor에 Execute시키면 된다.
        public class ExecuteTask : IRunnableMessage
        {
            public Action Task = null;
            public bool Run()
            {
                Task();
                return true;
            }
        }

        private readonly SerialIssuer _serialIssuer = new SerialIssuer();
        private readonly MessageDispatcher _dispatcher;
        private readonly ServiceExecuteMultiplexer _executor;
        private readonly ConcurrentDictionary<int, ScheduledTask> _tasks = new ConcurrentDictionary<int, ScheduledTask>();
        private readonly ManualResetEventSlim _eventSlim = new ManualResetEventSlim(false);
        private readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

        private Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private const int NotStarted = 1;
        private const int Started = 2;
        private const int ShuttingDown = 3;
        private const int Shutdown = 4;
        private const int Terminated = 5;

        private volatile int _executionState = NotStarted;
        public bool IsShuttingdown => _executionState >= ShuttingDown;
        public bool IsShutdown => _executionState >= Shutdown;
        public bool IsTerminated => _executionState == Terminated;

        public TaskScheduler(MessageDispatcher dispatcher, ServiceExecuteMultiplexer executor)
        {
            _dispatcher = dispatcher;
            _executor = executor;

            Task.Factory.StartNew(async () =>
            {
                Interlocked.CompareExchange(ref _executionState, Started, NotStarted);

                while (IsShuttingdown == false)
                {
                    RunElapsedTasks();
                    await Task.Delay(10);
                }

                CleanupAndTerminate();
            }, TaskCreationOptions.LongRunning);
        }

        private void CleanupAndTerminate()
        {
            try
            {
                // discard all schedules.
                _tasks.Clear();

                // double check
                int state;
                do
                {
                    state = _executionState;
                } while (state < Shutdown &&
                         Interlocked.CompareExchange(ref _executionState, Shutdown, state) != state);
            }
            finally
            {
                Console.WriteLine("TaskScheduler Terminated.");
                Interlocked.Exchange(ref _executionState, Terminated);
                _taskCompletionSource.SetResult(0);
            }
        }

        private void RunElapsedTasks()
        {
            if (_tasks.IsEmpty)
            {
                _eventSlim.Wait();
            }
            _eventSlim.Reset();

            int currentTickCount = Environment.TickCount & Int32.MaxValue;
            var elapsedList = _tasks.Where(task => task.Value.EndTickCount <= currentTickCount).ToList();
            if (elapsedList.Count == 0)
            {
                return;
            }

            // 먼저 삭제하고 dispatch 진행
            foreach (var scheduledMessage in elapsedList)
            {
                ScheduledTask dummy;
                if (_tasks.TryRemove(scheduledMessage.Key, out dummy) == false)
                {
                    _logger.Info($"Remove failed. {scheduledMessage.Key}\r\n");
                }
            }

            foreach (var scheduledTask in elapsedList)
            {
                scheduledTask.Value.Action(_dispatcher.Handle, _executor.Execute);
            }
        }

        public int? Add(int waitSecond, ScheduleAction action)
        {
            int taskId = _serialIssuer.Issue();

            ScheduledTask task = new ScheduledTask
            {
                EndTickCount = (Environment.TickCount & Int32.MaxValue) + waitSecond * 1000,
                Action = action
            };

            if (_tasks.TryAdd(taskId, task) == false)
            {
                _logger.Info($"Add failed. {taskId}\r\n");
                return null;
            }

            _eventSlim.Set();
            return taskId;
        }

        public void InternalDispatch(ScheduleAction action)
        {
            action(_dispatcher.Handle, _executor.Execute);
        }

        public void InternalDispatch(int id, IRunnableMessage runnable)
        {
            _executor.Execute(id, runnable);
        }

        // checkalive 남용 금지. 어쩔수 없을때만 확인후 사용.
        public void InternalDispatch(ISession session, IProtocol message)
        {
            _dispatcher.Handle(session, message);
        }

        public bool Remove(int id)
        {
            ScheduledTask dummy;
            bool result = _tasks.TryRemove(id, out dummy);
            if (result == false)
            {
                _logger.Info($"Remove  failed. {id}\r\n");
            }

            return result;
        }

        public Task ShutdownGracefullyAsync()
        {
            if (IsShutdown)
            {
                return _taskCompletionSource.Task;
            }

            int currentState = _executionState;
            if (IsShuttingdown == false)
            {
                Interlocked.CompareExchange(ref _executionState, ShuttingDown, currentState);
            }

            _eventSlim.Set();
            return _taskCompletionSource.Task;
        }
    }
}
