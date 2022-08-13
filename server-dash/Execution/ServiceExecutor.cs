using System.Collections.Concurrent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Pooling;
using Dash.Net;
using Dash.Protocol;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Internal;
using server_dash.Execution.Runnable;
using server_dash.Net.Handlers;
using server_dash.Net.Sessions;

#pragma warning disable 420

namespace server_dash.Execution
{
    public class ServiceExecutor
    {
        private static readonly IRunnableMessage NoOperation = new NoOperation();
        private static readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private readonly Thread _thread;
        private readonly string _name;
        private readonly ManualResetEventSlim _eventSlim = new ManualResetEventSlim(false, 1);

        private readonly List<Action<long/*TickCount*/>> _updateCallbacks = new List<Action<long>>();
        private readonly List<Action<long/*TickCount*/>> _addReservedCallbacks = new List<Action<long>>();
        private readonly List<Action<long/*TickCount*/>> _removeReservedCallbacks = new List<Action<long>>();

        private readonly Dictionary<int, IQueue<IRunnableMessage>> _queues = new Dictionary<int, IQueue<IRunnableMessage>>();
        private readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();
        private readonly MessageHandlerProvider _messageHandlerProvider;

        // private PoolManager<RunnableParsedMessage> _runnableMessagePool = new PoolManager<RunnableParsedMessage>(() => new RunnableParsedMessage());
        // private PoolManager<RunnableRawMessage> _runnableRawMessagePool = new PoolManager<RunnableRawMessage>(() => new RunnableRawMessage());
        // private PoolManager<RunnableCoroutine> _runnableCoroutinePool = new PoolManager<RunnableCoroutine>(() => new RunnableCoroutine());
        // private PoolManager<RunnableCoroutineMessage> _runnableCoroutineMessagePool = new PoolManager<RunnableCoroutineMessage>(() => new RunnableCoroutineMessage());

        private List<IQueue<IRunnableMessage>> _temp = new List<IQueue<IRunnableMessage>>();

        private const int ServiceExecutorNotStarted = 1;
        private const int ServiceExecutorStarted = 2;
        private const int ServiceExecutorShuttingDown = 3;
        private const int ServiceExecutorShutdown = 4;
        private const int ServiceExecutorTerminated = 5;

        private volatile int _executionState = ServiceExecutorNotStarted;

        public ServiceExecutor(string name, MessageHandlerProvider messageHandlerProvider)
        {
            _thread = new Thread(Loop);
            _name = name;
            _thread.Start();
            _messageHandlerProvider = messageHandlerProvider;
        }

        public int ManagedThreadId { get; private set; }

        public bool InServiceLoop => _thread == Thread.CurrentThread;

        public bool IsShutdown => _executionState >= ServiceExecutorShutdown;
        
        public bool IsShuttingDown => _executionState >= ServiceExecutorShuttingDown;

        public Task TerminationCompletion => _taskCompletionSource.Task;

        private void Loop()
        {
            ManagedThreadId = Thread.CurrentThread.ManagedThreadId;
            Thread.CurrentThread.Name = _name;

            Interlocked.CompareExchange(ref _executionState, ServiceExecutorStarted, ServiceExecutorNotStarted);

            try
            {
                while (!ConfirmShutdown())
                {
                    RunUpdateCallbacks();
                    RunAllTasks2();
                    Thread.Sleep(5);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
            }

            this.CleanupAndTerminate();
        }

        private void CleanupAndTerminate()
        {
            SynchronizationContext ctx = SynchronizationContext.Current;
            try
            {
                // double check
                int state;
                do
                {
                    state = _executionState;
                } while (state < ServiceExecutorShutdown &&
                         Interlocked.CompareExchange(ref _executionState, ServiceExecutorShuttingDown, state) != state);

                while (true)
                {
                    if (this.ConfirmShutdown())
                    {
                        break;
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _executionState, ServiceExecutorTerminated);
                _taskCompletionSource.SetResult(0);
            }
        }

        private bool ConfirmShutdown()
        {
            if (!IsShuttingDown)
            {
                return false;
            }

            return RunAllTasks2();
        }

        protected void RunUpdateCallbacks()
        {
            try
            {
                long tickCount = Environment.TickCount64;
                foreach (var callback in _addReservedCallbacks)
                {
                    _updateCallbacks.Add(callback);
                }
                _addReservedCallbacks.Clear();

                foreach (var callback in _removeReservedCallbacks)
                {
                    _updateCallbacks.Remove(callback);
                }
                _removeReservedCallbacks.Clear();

                foreach (Action<long> action in _updateCallbacks)
                {
                    action(tickCount);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e.Message);
                _logger.Fatal(e.StackTrace);
            }
        }

        protected bool RunAllTasks2()
        {
            bool allProcessed = true;
            try
            {
                _temp.Clear();
                lock (_queues)
                {
                    foreach (var queue in _queues.Values)
                    {
                        _temp.Add(queue);
                    }
                }
                foreach (IQueue<IRunnableMessage> queue in _temp)
                {
                    while (true)
                    {
                        if (queue.TryPeek(out IRunnableMessage message) == true)
                        {
                            if (message.Run() == true)
                            {
                                if (queue.TryDequeue(out message) == true)
                                {
                                    //ReturnToPool(message);
                                }
                            }
                            else
                            {
                                allProcessed = false;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Fatal(ex.Message);
                _logger.Fatal(ex.StackTrace);
            }

            return allProcessed;
        }

        private void ReturnToPool(IRunnableMessage message, bool checkThread = true)
        {
            // // 다른 쓰레드에서 호출되는 상황 예상 X
            // if (checkThread == true && InServiceLoop == false)
            // {
            //     throw new InvalidOperationException();
            // }
            //
            // if (message is RunnableParsedMessage runnableMessage)
            // {
            //     lock (_runnableMessagePool)
            //     {
            //         _runnableMessagePool.Return(runnableMessage);
            //     }
            // }
            // else if (message is RunnableRawMessage runnableRawMessage)
            // {
            //     lock (_runnableRawMessagePool)
            //     {
            //         _runnableRawMessagePool.Return(runnableRawMessage);
            //     }
            // }
            // else if (message is RunnableCoroutine runnableCoroutine)
            // {
            //     lock (_runnableCoroutinePool)
            //     {
            //         _runnableCoroutinePool.Return(runnableCoroutine);
            //     }
            // }
            // else if (message is RunnableCoroutineMessage runnableCoroutineMessage)
            // {
            //     lock (_runnableCoroutineMessagePool)
            //     {
            //         _runnableCoroutineMessagePool.Return(runnableCoroutineMessage);
            //     }
            // }
        }

        public void AddUpdateCallback(Action<long> updateCallback)
        {
            if (InServiceLoop == false)
            {
                throw new NotSupportedException();
            }

            _addReservedCallbacks.Add(updateCallback);
        }

        public void RemoveUpdateCallback(Action<long> updateCallback)
        {
            if (InServiceLoop == false)
            {
                throw new NotSupportedException();
            }

            _removeReservedCallbacks.Add(updateCallback);
        }

        public void RemoveMessageQueue(int id)
        {
            IQueue<IRunnableMessage> queue = null;
            lock (_queues)
            {
                _queues.Remove(id, out queue);
            }

            // if (queue != null)
            // {
            //     IRunnableMessage message = null;
            //     while (queue.TryDequeue(out message) == true)
            //     {
            //         ReturnToPool(message, false);
            //     }
            // }
        }

        public void ExecuteParsedMessage(int id, ISession session, IProtocol parsedMessage)
        {
            RunnableParsedMessage runnableParsedMessage = new RunnableParsedMessage();
            runnableParsedMessage.Init(session, _messageHandlerProvider, parsedMessage);
            // lock (_runnableMessagePool)
            // {
            //     runnableParsedMessage = _runnableMessagePool.Get();
            //     runnableParsedMessage.Init(session, _messageHandlerProvider, message);
            // }
            
            Execute(id, runnableParsedMessage);
        }

        public void ExecuteCoroutineMessage(int id, ISession session, IProtocol message)
        {
            RunnableCoroutineMessage runnableCoroutineMessage = new RunnableCoroutineMessage();
            runnableCoroutineMessage.Init(session, _messageHandlerProvider, message);
            // lock (_runnableCoroutineMessagePool)
            // {
            //     runnableCoroutineMessage = _runnableCoroutineMessagePool.Get();
            //     runnableCoroutineMessage.Init(session, _messageHandlerProvider, message);
            // }
            Execute(id, runnableCoroutineMessage);
        }

        public void ExecuteRawMessage(int id, ISession session, RawMessage message)
        {
            RunnableRawMessage runnableRawMessage = new RunnableRawMessage();
            runnableRawMessage.Init(session, message);
            // lock (_runnableRawMessagePool)
            // {
            //     runnableRawMessage = _runnableRawMessagePool.Get();
            //     runnableRawMessage.Init(session, message);
            // }
            Execute(id, runnableRawMessage);
        }

        public void ExecuteCoroutine(IServiceExecutionContext context, Func<CoroutineContext, IEnumerator> func)
        {
            RunnableCoroutine runnableCoroutine = new RunnableCoroutine();
            runnableCoroutine.Init(func);
            // lock (_runnableCoroutinePool)
            // {
            //     runnableCoroutine = _runnableCoroutinePool.Get();
            //     runnableCoroutine.Init(func);
            // }
            Execute(context.ServiceExecutorId, runnableCoroutine);
        }
        public void ExecuteCoroutine(int id, Func<CoroutineContext, IEnumerator> func)
        {
            RunnableCoroutine runnableCoroutine = new RunnableCoroutine();
            runnableCoroutine.Init(func);
            // lock (_runnableCoroutinePool)
            // {
            //     runnableCoroutine = _runnableCoroutinePool.Get();
            //     runnableCoroutine.Init(func);
            // }
            Execute(id, runnableCoroutine);
        }

        public void ExecuteAction(IServiceExecutionContext context, Action action)
        {
            Execute(context.ServiceExecutorId, new ActionMessage(action));
        }

        public void ExecuteAction(int id, Action action)
        {
            Execute(id, new ActionMessage(action));
        }

        public void Execute(int id, IRunnableMessage runnable)
        {
            lock (_queues)
            {
                if (_queues.TryGetValue(id, out var queue) == true)
                {
                    queue.TryEnqueue(runnable);
                }
                else
                {
                    queue = new CompatibleConcurrentQueue<IRunnableMessage>();
                    queue.TryEnqueue(runnable);
                    _queues.Add(id, queue);
                }
            }
            if (InServiceLoop)
            {
                return;
            }

            _eventSlim.Set();
        }


        public void Wakeup()
        {
            if (InServiceLoop && !IsShuttingDown)
            {
                return;
            }

            Execute(0, NoOperation);
        }

        public Task ShutdownGracefully()
        {
            if (IsShuttingDown)
            {
                return _taskCompletionSource.Task;
            }
            
            while (!IsShuttingDown)
            {
                bool wakeup = true;
                int oldState = _executionState;
                int newState;

                if (InServiceLoop)
                {
                    newState = ServiceExecutorShuttingDown;
                }
                else
                {
                    switch (oldState)
                    {
                        case ServiceExecutorNotStarted:
                        case ServiceExecutorStarted:
                            newState = ServiceExecutorShuttingDown;
                            break;
                        default:
                            newState = oldState;
                            wakeup = false;
                            break; 
                    }
                }

                if (Interlocked.CompareExchange(ref _executionState, newState, oldState) == oldState)
                {
                    if (wakeup)
                    {
                        Wakeup();
                    }

                    return _taskCompletionSource.Task;
                }
            }

            return _taskCompletionSource.Task;
        }
    }
}

namespace server_dash.Net.Handlers
{
    
}