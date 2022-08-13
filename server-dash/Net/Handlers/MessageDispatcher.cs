using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Common.Log;
using NLog;
using DotNetty.Codecs;
using Common.Utility;
using Dash;
using Dash.Net;
using Dash.Protocol;
using DotNetty.Buffers;
using server_dash.Net.Sessions;
using server_dash.Utility;
using Dash.Types;
using server_dash.Execution.Runnable;

namespace server_dash.Net.Handlers
{
    public class MessageHandlerProvider
    {
        public IReadOnlyDictionary<(int, ServiceAreaType), IMessageHandler> MessageHandlers;
        public IReadOnlyDictionary<(int, ServiceAreaType), ICoroutineMessageHandler> CoroutineMessageHandlers;
        public RawMessageHandler RawMessageHandler;
       
        public void Init(IReadOnlyDictionary<(int, ServiceAreaType), IMessageHandler> messageHandlers, RawMessageHandler rawMessageHandler)
        {
            MessageHandlers = messageHandlers;
            RawMessageHandler = rawMessageHandler;
            CoroutineMessageHandlers = new Dictionary<(int, ServiceAreaType), ICoroutineMessageHandler>(
                messageHandlers
                .Where(p => p.Value is ICoroutineMessageHandler)
                .Select(p => new KeyValuePair<(int, ServiceAreaType), ICoroutineMessageHandler>(p.Key, p.Value as ICoroutineMessageHandler)));
        }
        public IMessageHandler GetMessageHandler(int typeCode, ServiceAreaType serviceAreaType)
        {
            ServiceAreaType[] serviceAreaTypes = { serviceAreaType, ServiceAreaType.All };
            IMessageHandler messageHandler = null;
            foreach (var areaType in serviceAreaTypes)
            {
                if (MessageHandlers.TryGetValue((typeCode, areaType), out messageHandler))
                {
                    break;
                }
            }
            if (messageHandler == null)
            {
                throw new NotBoundMessageException(typeCode, serviceAreaType);
            }
            return messageHandler;
        }
        public ICoroutineMessageHandler GetCoroutineMessageHandler(int typeCode, ServiceAreaType serviceAreaType)
        {
            ServiceAreaType[] serviceAreaTypes = { serviceAreaType, ServiceAreaType.All };
            ICoroutineMessageHandler coroutineMessage = null;
            foreach (var areaType in serviceAreaTypes)
            {
                if (CoroutineMessageHandlers.TryGetValue((typeCode, areaType), out coroutineMessage))
                {
                    break;
                }
            }
            if (coroutineMessage == null)
            {
                throw new NotBoundMessageException(typeCode, serviceAreaType);
            }
            return coroutineMessage;
        }
    }

    public class MessageDispatcher
    {
        public HashSet<int> ParseTypeCodes { get; } = new HashSet<int>();
        public IReadOnlyDictionary<(int, ServiceAreaType), IMessageHandler> MessageHandlers => _messageHandlers;

        private static readonly LogicLogger _logger = new LogicLogger(NLogUtility.GetCurrentClassLogger());
        private static readonly Type _coroutineMethodReturnType = typeof(IEnumerator);
        private static readonly Func<MethodInfo, bool> _methodFilter = (methodInfo) => methodInfo.IsPublic == true && methodInfo.IsStatic == false;

        private readonly Dictionary<(int, ServiceAreaType), IMessageHandler> _messageHandlers = new Dictionary<(int, ServiceAreaType), IMessageHandler>();


        private string _alias;
        private readonly RawMessageHandler _rawMessageHandler;
        private readonly ServiceExecuteMultiplexer _serviceExecuteMultiplexer;

        public MessageDispatcher(string alias, ServiceExecuteMultiplexer serviceExecuteMultiplexer, RawMessageHandler rawMessageHandler, CommandMessageHandler.CommandMessageHandleFunc commandMessageHandleFunc)
        {
            _alias = alias;
            _serviceExecuteMultiplexer = serviceExecuteMultiplexer;
            _rawMessageHandler = rawMessageHandler;
            if (commandMessageHandleFunc != null)
            {
                
                foreach (int typeCode in MessageSerializer.CoreCommandTypes.Keys)
                {
                    ParseTypeCodes.Add(typeCode);                    
                    _messageHandlers.Add((typeCode, ServiceAreaType.Client), new CommandMessageHandler(commandMessageHandleFunc));
                }
            }
        }

        public void RegisterHandler<T>(T target)
        {
            var logBuilder = new StringBuilder($"[{_alias}]<{NLogUtility.MakeSimplifiedName(target.GetType().FullName)}> ");
            int registerCount = 0;
            foreach (var method in TypeInfoHolder<T>.GetMethods(_methodFilter))
            {
                var bindMessageAttribute = method.GetCustomAttributes(typeof(BindMessageAttribute), false).SingleOrDefault();
                var coroutineAttribute = method.GetCustomAttributes(typeof(CoroutineAttribute)).SingleOrDefault();
                if (bindMessageAttribute == null && coroutineAttribute == null)
                {
                    continue;
                }


                bool isAsync = method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
                if (isAsync == true)
                {
                    throw new NotSupportedException($"Message Handler async method is not allowed. {method.Name}");
                }

                bool isCoroutine = method.ReturnType == _coroutineMethodReturnType;
                if (isCoroutine == true && coroutineAttribute == null)
                {
                    throw new Exception("Invalid coroutine method attribute!");
                }
                ServiceAreaType[] areaTypes;

                Type sessionType = null;
                Type messageType = null;
                if(isCoroutine)
                {
                    areaTypes = (coroutineAttribute as CoroutineAttribute).AreaTypes;
                    sessionType = method.GetParameters()[1].ParameterType;
                    messageType = method.GetParameters()[2].ParameterType;
                }
                else
                {
                    areaTypes = (bindMessageAttribute as BindMessageAttribute).AreaTypes;
                    sessionType = method.GetParameters()[0].ParameterType;
                    messageType = method.GetParameters()[1].ParameterType;
                }

                
                
                //bindMessageAttribute

                // need hashcode and parser
                var msg = Activator.CreateInstance(messageType);
                int typeCode = (msg as IProtocol).GetTypeCode();

                ++registerCount;
                logBuilder.Append('[');
                if (isCoroutine == true)
                {
                    logBuilder.Append("(Coroutine)");
                }
                logBuilder.Append(NLogUtility.MakeSimplifiedName(messageType.FullName));
                logBuilder.Append(':');
                logBuilder.Append(typeCode);
                logBuilder.Append(']');
                if (logBuilder.Length >= 100)
                {
                    _logger.Info(logBuilder.ToString());
                    logBuilder.Clear();
                }

                if (ParseTypeCodes.Contains(typeCode) == false)
                {
                    ParseTypeCodes.Add(typeCode);
                }
                foreach(var areaType in areaTypes)
                {
                    _messageHandlers.TryGetValue((typeCode, areaType), out IMessageHandler messageHandler);
                    if (messageHandler is CommandMessageHandler commandMessageHandler)
                    {
                        _messageHandlers.Remove((typeCode, areaType));
                    }

                    if (_messageHandlers.ContainsKey((typeCode, areaType)) == false)
                    {
                        if (isCoroutine == true)
                        {
                            messageHandler =
                                (IMessageHandler)Activator.CreateInstance(
                                    typeof(CoroutineMessageHandler<,>).MakeGenericType(sessionType, messageType), typeCode);
                        }
                        else
                        {
                            messageHandler =
                                (IMessageHandler)Activator.CreateInstance(
                                    typeof(MessageHandler<,>).MakeGenericType(sessionType, messageType), typeCode);

                        }
                        _messageHandlers.Add((typeCode, areaType), messageHandler);
                        messageHandler.AddHandler(method, sessionType, messageType, target);
                    }
                    else
                    {
                        _messageHandlers[(typeCode, areaType)].AddHandler(method, sessionType, messageType, target);
                    }
                }
            }

            if (logBuilder.Length > 0)
            {
                _logger.Info(logBuilder.ToString());
            }
        }

        public void Handle(ISession session, IProtocol message)
        {
            if (message == null)
            {
                return;
            }

            int typeCode = message.GetTypeCode();
            int id = session.ServiceExecutorId;
            IMessageHandler messageHandler = null;
            if (_messageHandlers.TryGetValue((typeCode, session.AreaType), out messageHandler) == false)
            {
                _messageHandlers.TryGetValue((typeCode, ServiceAreaType.All), out messageHandler);
            }

            if (messageHandler == null)
            {
                throw new NotBoundMessageException(typeCode, session.AreaType);
            }

            if (session.InSafeFence == true)
            {
                if (messageHandler is ICoroutineMessageHandler)
                {
                    _serviceExecuteMultiplexer.PrimaryExecutor.ExecuteCoroutineMessage(id, session, message);
                }
                else
                {
                    messageHandler.Handle(session, message);
                }
            }
            else
            {
                if (messageHandler is ICoroutineMessageHandler)
                {
                    _serviceExecuteMultiplexer.GetExecutor(id).ExecuteCoroutineMessage(id, session, message);
                }
                else
                {
                    _serviceExecuteMultiplexer.GetExecutor(id).ExecuteParsedMessage(id, session, message);
                }
            }
        }

        public void Handle(ISession session, RawMessage rawMessage)
        {
            if (session.InSafeFence == true)
            {
                _rawMessageHandler.Handle(session, rawMessage);
            }
            else
            {
                int id = session.ServiceExecutorId;
                UnsafeBufferPool unsafeBufferPool = _serviceExecuteMultiplexer.GetUnsafeBufferPool(id);
                rawMessage.Bytes = unsafeBufferPool.MakeManagedBuffer(rawMessage.Bytes);
                rawMessage.Pool = unsafeBufferPool;
                _serviceExecuteMultiplexer.GetExecutor(id).ExecuteRawMessage(id, session, rawMessage);
            }
        }
    }

    internal class NotBoundMessageException : Exception
    {
        public NotBoundMessageException(int hashCode, ServiceAreaType type) :
            base($"No boundMessage[{hashCode}][{type}]")
        {
        }
        public NotBoundMessageException(int hashCode) :
            base($"No boundMessage[{hashCode}]")
        {
        }
    }

    internal class AlreadyMessageBindingException : Exception
    {
        public AlreadyMessageBindingException(int handlerHashCode) :
            base ($"AlreadyMessageBind[{handlerHashCode}]")
        {
        }
    }
}