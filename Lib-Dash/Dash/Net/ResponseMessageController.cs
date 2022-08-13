using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Dash.Net.Sessions;
using Dash.Protocol;
using DotNetty.Common.Utilities;

namespace Dash.Net
{
    public class ResponseMessageController
    {
        protected static readonly Utility.LogicLogger _logger = new Utility.LogicLogger();

        private static System.Threading.ThreadLocal<HashedWheelTimer> _timer = new System.Threading.ThreadLocal<HashedWheelTimer>(() =>
        {
            HashedWheelTimer timer = new HashedWheelTimer();
            timer.Start();
            return timer;
        });

        private readonly ConcurrentDictionary<int, TaskCompletionSource<IProtocol>> _responseTasks =
            new ConcurrentDictionary<int, TaskCompletionSource<IProtocol>>();
        private readonly ConcurrentDictionary<int, ITimeout> _timeoutTasks = new ConcurrentDictionary<int, ITimeout>();

        private int _lastIssuedMessageSerial = 0;

        #if DEBUG
        private static readonly TimeSpan _responseTimeout = TimeSpan.FromSeconds(1000);
        #else
        private static readonly TimeSpan _responseTimeout = TimeSpan.FromSeconds(10);
        #endif

        private ISession _ownerSession;
        private readonly SendQueue _sendQueue;

        public ResponseMessageController(ISession ownerSession, SendQueue sendQueue)
        {
            _ownerSession = ownerSession;
            _sendQueue = sendQueue;
        }

        public bool TryHandleResponse(ISequentialProtocol protocol)
        {
            if (_responseTasks.TryRemove(protocol.Header.Serial, out TaskCompletionSource<IProtocol> completion) == false)
            {
                return false;
            }
            if (_timeoutTasks.TryRemove(protocol.Header.Serial, out ITimeout timeout))
            {
                if (timeout.Cancel() == false)
                {
                    return false;
                }
            }

            completion.TrySetResult(protocol);
            return true;
        }

        public Task<IProtocol> GetResponse<TRequest>(TRequest request, ulong oidRequester, TimeSpan timeout) where TRequest : ISequentialProtocol
        {
            int serial = Interlocked.Increment(ref _lastIssuedMessageSerial);
            request.Header = new SequentialProtocolHeader(serial, oidRequester);
            var completion = new TaskCompletionSource<IProtocol>();
            if(_responseTasks.TryAdd(serial, completion) == false)
            {
                _logger.Error($"{_ownerSession}[{request.GetTypeCode()}] Add ResponseTask Failed. Serial : {serial}");
            }

            TimeSpan targetTimeout = timeout == default ? _responseTimeout : timeout;
            if (_timeoutTasks.TryAdd(serial, CreateTimeout(_timer.Value, serial, targetTimeout)) == false)
            {
                _logger.Error($"{_ownerSession}[{request.GetTypeCode()}] Add TimeoutTask Failed. Serial : {serial}");
            }

            _sendQueue.Send(request, isFlush: true);

            return completion.Task;
        }

        private ITimeout CreateTimeout(ITimer timer, int serial, TimeSpan timeout)
        {
            return timer.NewTimeout(new ActionTimerTask(
                _ =>
                {
                    if (_responseTasks.TryRemove(serial, out var completion))
                    {
                        _logger.Error($"{_ownerSession} GetResponse Timeout. serial : {serial}");
                        completion.SetResult(null);
                    }
                    _timeoutTasks.TryRemove(serial, out _);
                }), timeout);
        }
    }
}