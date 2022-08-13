using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Utility;
using Dash.Net;
using Dash.Protocol;
using DotNetty.Common.Utilities;
using NLog;
using server_dash.Net.Handlers;
using server_dash.Utility;

namespace server_dash.Net.Client
{
    public class InternalClientMessageDispatcher
    {
        public HashSet<int> ParseTypeCodes { get; } = new HashSet<int>();
        public IReadOnlyDictionary<int, IClientMessageHandler> MessageHandlers => _messageHandlers;

        private static readonly LogicLogger _logger = new LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());
        private readonly ConcurrentDictionary<int, TaskCompletionSource<ISequentialProtocol>> _responseTasks = new ConcurrentDictionary<int, TaskCompletionSource<ISequentialProtocol>>();
        private readonly ConcurrentDictionary<int, ITimeout> _timeoutTasks = new ConcurrentDictionary<int, ITimeout>();
        private static readonly Type _coroutineMethodReturnType = typeof(IEnumerator);
        private static readonly Func<MethodInfo, bool> _methodFilter = (methodInfo) => methodInfo.IsPublic == true && methodInfo.IsStatic == false;
        private static System.Threading.ThreadLocal<HashedWheelTimer> _timer = new System.Threading.ThreadLocal<HashedWheelTimer>(() =>
        {
            HashedWheelTimer timer = new HashedWheelTimer();
            timer.Start();
            return timer;
        });

        private readonly Dictionary<int, IClientMessageHandler> _messageHandlers = new Dictionary<int, IClientMessageHandler>();
        private int _lastIssuedMessageSerial = 0;
#if DEBUG
        private int _responseTimeout = 1000;
#else
        private int _responseTimeout = 10;
#endif



        public void RegisterHandler<T>(T target)
        {
            foreach (var method in TypeInfoHolder<T>.GetMethods(_methodFilter))
            {
                var bindMessageAttribute = method.GetCustomAttributes(typeof(BindInternalMessageAttribute), false).SingleOrDefault();
                if (bindMessageAttribute == null)
                {
                    continue;
                }

                // get first parameter type
                Type paramType2 = method.GetParameters()[0].ParameterType; // TMsg
                bool isAsync = method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
                if (isAsync == true)
                {
                    throw new NotSupportedException($"Message Handler async method is not allowed. {method.Name}");
                }

                bool isCoroutine = false;// method.ReturnType == _coroutineMethodReturnType;
                if (isCoroutine == true)
                {
                    throw new Exception("Invalid coroutine method attribute!");
                }

                //bindMessageAttribute

                // need hashcode and parser
                var msg = Activator.CreateInstance(paramType2);
                int typeCode = (msg as IProtocol).GetTypeCode();

                _logger.Info($"Message : {paramType2.FullName}, TypeCode : {typeCode}{(isCoroutine == true ? " [Coroutine]" : "")}");

                if (ParseTypeCodes.Contains(typeCode) == false)
                {
                    ParseTypeCodes.Add(typeCode);
                }
                {
                    _messageHandlers.TryGetValue(typeCode, out IClientMessageHandler messageHandler);
                    if (messageHandler is CommandMessageHandler commandMessageHandler)
                    {
                        _messageHandlers.Remove(typeCode);
                    }

                    if (_messageHandlers.ContainsKey(typeCode) == false)
                    {
                        if (isCoroutine == true)
                        {
                            //messageHandler =
                            //    (IMessageHandler)Activator.CreateInstance(
                            //        typeof(CoroutineMessageHandler<,>).MakeGenericType(paramType1, paramType2), typeCode);
                        }
                        else
                        {
                            messageHandler =
                                (IClientMessageHandler)Activator.CreateInstance(
                                    typeof(ClientMessageHandler<>).MakeGenericType(paramType2), typeCode);

                        }
                        _messageHandlers.Add(typeCode, messageHandler);
                        messageHandler.AddHandler(method, paramType2, target);
                    }
                    else
                    {
                        _messageHandlers[typeCode].AddHandler(method, paramType2, target);
                    }
                }
            }
        }

        public void Handle(IProtocol protocolMessage)
        {
            IClientMessageHandler messageHandler = null;
            int typeCode = protocolMessage.GetTypeCode();
            if (_messageHandlers.TryGetValue(typeCode, out messageHandler) == false)
            {
                throw new NotBoundMessageException(typeCode);
            }
            messageHandler.Handle(protocolMessage);
        }

        private void OnResponseReceive<T>(T protocol) where T : ISequentialProtocol
        {
            if (_responseTasks.TryRemove(protocol.Serial, out TaskCompletionSource<ISequentialProtocol> completion) == false)
            {
                _logger.Error($"[InternalClientMessageDispatcher] Response Task not found, TypeCode : {protocol.GetTypeCode()}, Serial : {protocol.Serial}");
                return;
            }
            if(_timeoutTasks.TryRemove(protocol.Serial, out ITimeout timeout))
            {
                timeout.Cancel();
            }
            completion.SetResult(protocol);
        }
        public void AddMessageHandlerUnsafe<TResponse>(int responseTypeCode) where TResponse : ISequentialProtocol
        {
            if (_messageHandlers.ContainsKey(responseTypeCode) == false)
            {
                var messageHandler = new ClientMessageHandler<TResponse>(responseTypeCode);
                _messageHandlers.Add(responseTypeCode, messageHandler);
                messageHandler.AddHandler(OnResponseReceive);
            }
        }
        public Task<ISequentialProtocol> GetResponse(ISequentialProtocol request)
        {
            var completion = new TaskCompletionSource<ISequentialProtocol>();
            _responseTasks.TryAdd(request.Serial, completion);
            _timeoutTasks.TryAdd(request.Serial, CreateTimeout(_timer.Value, request.Serial));
            return completion.Task;
        }

        private ITimeout CreateTimeout(ITimer timer, int serial)
        {
            return timer.NewTimeout(new ActionTimerTask(
                _ =>
                {
                    if(_responseTasks.TryRemove(serial, out var completion))
                    {
                        _logger.Error($"[InternalClientMessageDispatcher] GetResponse Timeout. serial : {serial}");
                        completion.SetResult(null);
                    }
                    _timeoutTasks.TryRemove(serial, out var timeout);
                }), TimeSpan.FromSeconds(_responseTimeout));
        }
    }
}
