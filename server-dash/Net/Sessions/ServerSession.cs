using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dash.Model;
using Dash.Net;
using Dash.Protocol;
using Dash.Types;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace server_dash.Net.Sessions
{
    public class ServerSession : AbstractSession
    {
        public readonly string Endpoint;
        public readonly string UUID;

        private readonly ConcurrentDictionary<int, TaskCompletionSource<IProtocol>> _responseTasks = new ConcurrentDictionary<int, TaskCompletionSource<IProtocol>>();
        private readonly ConcurrentDictionary<int, ITimeout> _timeoutTasks = new ConcurrentDictionary<int, ITimeout>();

#if DEBUG
        private readonly int _responseTimeout = 1000;
#else
        private readonly int _responseTimeout = 10;
#endif

        private int _lastIssuedMessageSerial = 0;
        private int _serviceExecutorId = 0;

        private static System.Threading.ThreadLocal<HashedWheelTimer> _timer = new System.Threading.ThreadLocal<HashedWheelTimer>(() =>
        {
            HashedWheelTimer timer = new HashedWheelTimer();
            timer.Start();
            return timer;
        });

        public ServerSession(ServiceAreaType areaType, IChannel channel, string uuid, string endpoint) : base(channel)
        {
            AreaType = areaType;
            UUID = uuid;
            Endpoint = endpoint;
            Enabled = true;
        }

        public override string ToString()
        {
            if (Channel != null && Channel.Active == true)
            {
                return $"[{AreaType}] {Channel.ToString()}";
            }
            return "";
        }        

        public override bool InSafeFence => true;

        public DateTime LastMarkAliveTime { get; private set; }
        public float Density { get; private set; }
        public bool Enabled { get; private set; }
        public DateTime EnabledTime { get; private set; } // Enable 된 시점

        public void SetLastMarkAliveTime(DateTime value) => LastMarkAliveTime = value;
        public void SetDensity(float value) => Density = value; 
        public void SetEnabled(bool value) => Enabled = value;
        public void SetEnabledTime(DateTime value) => EnabledTime = value;

        public override int ServiceExecutorId 
        {
            get 
            {
                if(_serviceExecutorId == 0)
                {
                    _serviceExecutorId = Math.Abs((int)OidAccount);
                }
                return _serviceExecutorId;
            }
        }
        
        public void OnResponseReceive(ISequentialProtocol protocol)
        {
            {
                if (_responseTasks.TryRemove(protocol.Serial, out TaskCompletionSource<IProtocol> completion) == false)
                {
                    _logger.Error($"[ServerSession][ResponseMessageDispatcher][{protocol.GetTypeCode()}] Response Task not found, Serial : {protocol.Serial}");
                    return;
                }
                if (_timeoutTasks.TryRemove(protocol.Serial, out ITimeout timeout))
                {
                    timeout.Cancel();
                }
                completion.SetResult(protocol);
            }
        }
        public Task<IProtocol> GetResponse<TRequest>(TRequest request) where TRequest : ISequentialProtocol
        {
            request.Serial = ++_lastIssuedMessageSerial;
            var completion = new TaskCompletionSource<IProtocol>();
            if(_responseTasks.TryAdd(request.Serial, completion) == false)
            {
                _logger.Error($"[ServerSession][GetResponse][{request.GetTypeCode()}] Add ResponseTask Failed. Serial : {request.Serial}");
            }
            _timeoutTasks.TryAdd(request.Serial, CreateTimeout(_timer.Value, request.Serial));
            bool flush = true;
            if (flush)
            {
                Channel.WriteAndFlushAsync(request);
            }
            else
            {
                Channel.WriteAsync(request);
            }
            return completion.Task;
        }

        private ITimeout CreateTimeout(ITimer timer, int serial)
        {
            return timer.NewTimeout(new ActionTimerTask(
                _ =>
                {
                    if (_responseTasks.TryRemove(serial, out var completion))
                    {
                        _logger.Error($"[InternalClientMessageDispatcher] GetResponse Timeout. serial : {serial}");
                        completion.SetResult(null);
                    }
                    _timeoutTasks.TryRemove(serial, out var timeout);
                }), TimeSpan.FromSeconds(_responseTimeout));
        }
    }
}
