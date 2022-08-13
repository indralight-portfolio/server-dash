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
using Dash.Net.Handlers;
using Dash.Net.Sessions;
using Dash.Protocol;
using Dash.Types;
using Dash.Utility;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Internal;
using DotNetty.Common.Utilities;

namespace Dash.Net.MessageDispatcher
{
    // TODO : thread safe
    public class ConnectorMessageDispatcher
    {
        protected static readonly Type _coroutineMethodReturnType = typeof(IEnumerator);
        protected static readonly Func<MethodInfo, bool> _methodFilter = (methodInfo) => methodInfo.IsPublic == true && methodInfo.IsStatic == false;
        protected static System.Threading.ThreadLocal<HashedWheelTimer> _timer = new System.Threading.ThreadLocal<HashedWheelTimer>(() =>
        {
            HashedWheelTimer timer = new HashedWheelTimer();
            timer.Start();
            return timer;
        });

        public HashSet<int> ParseTypeCodes { get; } = new HashSet<int>();
        public IReadOnlyDictionary<(int, ServiceAreaType), IMessageHandler> MessageHandlers => _messageHandlers;

        protected readonly Dictionary<(int, ServiceAreaType), IMessageHandler> _messageHandlers = new Dictionary<(int, ServiceAreaType), IMessageHandler>();
        private Action<IProtocol> _fallbackHandler;

        public virtual void ClearAllHandlers()
        {
            _messageHandlers.Clear();
            _fallbackHandler = null;
        }

        public void SetFallbackHandler(Action<IProtocol> fallbackHandler)
        {
            _fallbackHandler = fallbackHandler;
        }

        public virtual void RegisterHandler<T>(T target)
        {
            foreach (var method in TypeInfoHolder<T>.GetMethods(_methodFilter))
            {
                var bindMessageAttribute = method.GetCustomAttributes(typeof(BindMessageAttribute), false).SingleOrDefault();
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

                bool isCoroutine = method.ReturnType == _coroutineMethodReturnType;
                if (isCoroutine == true)
                {
                    throw new Exception("Invalid coroutine method attribute!");
                }
                ServiceAreaType[] areaTypes;
                Type sessionType = null;
                Type messageType = null;
                areaTypes = (bindMessageAttribute as BindMessageAttribute).AreaTypes;
                sessionType = method.GetParameters()[0].ParameterType;
                messageType = method.GetParameters()[1].ParameterType;
                //bindMessageAttribute

                // need hashcode and parser
                var msg = Activator.CreateInstance(messageType);
                int typeCode = (msg as IProtocol).GetTypeCode();

#if UNITY_EDITOR
                Common.Log.Logger.Instance.Info($"Message : {paramType2.FullName}, TypeCode : {typeCode}{(isCoroutine == true ? " [Coroutine]" : "")}");
#endif

                if (ParseTypeCodes.Contains(typeCode) == false)
                {
                    ParseTypeCodes.Add(typeCode);
                }

                foreach (var areaType in areaTypes)
                {
                    if (_messageHandlers.TryGetValue((typeCode, areaType), out IMessageHandler messageHandler) &&
                        messageHandler.HandlerType == MessageHandlerType.CommandHandler)
                    {
                        _messageHandlers.Remove((typeCode, areaType));
                    }

                    if (_messageHandlers.ContainsKey((typeCode, areaType)) == false)
                    {
                        messageHandler =
                            (IMessageHandler)Activator.CreateInstance(
                                typeof(MessageHandler<,>).MakeGenericType(sessionType, messageType), typeCode);

                        _messageHandlers.Add((typeCode, areaType), messageHandler);
                        messageHandler.AddHandler(method, sessionType, messageType, target);
                    }
                    else
                    {
                        _messageHandlers[(typeCode, areaType)].AddHandler(method, sessionType, messageType, target);
                    }
                }
            }
        }

        public void RegisterMessageHandlerDirect(int typeCode, ServiceAreaType serviceAreaType, IMessageHandler handler)
        {
            if (_messageHandlers.ContainsKey((typeCode, serviceAreaType)))
            {
                Type type = MessageSerializer.Types[typeCode];
                throw new AlreadyMessageBindingException(typeCode);
            }
            _messageHandlers.Add((typeCode, serviceAreaType), handler);
        }

        public void UnregisterMessageHandlerDirect(int typeCode, ServiceAreaType serviceAreaType, IMessageHandler handler)
        {
            if (_messageHandlers.TryGetValue((typeCode, serviceAreaType), out var exist) == false)
            {
                throw new Exception($"MessageHandler not registered : {typeCode}/{serviceAreaType}");
            }

            if (exist != handler)
            {
                throw new Exception($"Handler mismatch : {exist} != {handler}, {typeCode}");
            }

            _messageHandlers.Remove((typeCode, serviceAreaType));
        }

        public bool TryGetHandler(int typeCode, ServiceAreaType serviceAreaType, out IMessageHandler handler)
        {
            return MessageHandlers.TryGetValue((typeCode, serviceAreaType), out handler);
        }

        public virtual void Handle(ISession session, IProtocol message)
        {
            if (message == null)
            {
                return;
            }
            int typeCode = message.GetTypeCode();
            if (message is ISequentialProtocol sequentialProtocol)
            {
                if (session.TryHandleResponse(sequentialProtocol) == true)
                {
                    return;
                }
            }

            IMessageHandler messageHandler;
            if (_messageHandlers.TryGetValue((typeCode, session.AreaType), out messageHandler) == false)
            {
                _messageHandlers.TryGetValue((typeCode, ServiceAreaType.All), out messageHandler);
            }
            if (messageHandler == null)
            {
                if (_fallbackHandler != null)
                {
                    _fallbackHandler?.Invoke(message);
                    return;
                }

                throw new NotBoundMessageException(typeCode, session.AreaType);
            }
            messageHandler.Handle(session, message);
        }

        public virtual void PollMessage(bool updateOnce)
        {
        }
    }
}
