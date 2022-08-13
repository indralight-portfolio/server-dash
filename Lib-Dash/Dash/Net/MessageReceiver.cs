using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dash.Net.Handlers;
using Dash.Net.MessageDispatcher;
using Dash.Net.Sessions;
using Dash.Protocol;

namespace Dash.Net
{
    public readonly struct MessageReceiver : IDisposable
    {
        private readonly ConnectorMessageDispatcher _dispatcher;
        private readonly List<IMessageHandler> _handlers;
        private readonly Dash.Types.ServiceAreaType _serviceAreaType;

        public MessageReceiver(Types.ServiceAreaType serviceAreaType, ConnectorMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _serviceAreaType = serviceAreaType;
            _handlers = new List<IMessageHandler>();
        }

        public void UnregisterHandlers()
        {
            for (int i = 0; i < _handlers.Count; ++i)
            {
                var handler = _handlers[i];
                _dispatcher.UnregisterMessageHandlerDirect(handler.TypeCode, _serviceAreaType, handler);
            }

            _handlers.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="overrideHandler">기존에 등록되어있는 Handler를 덮어쓸 것인가?</param>
        /// <returns>결과, timeout일 경우 (default)</returns>
        public Task<T> Receive<T>(TimeSpan timeout = default, bool overrideHandler = false) where T : IProtocol
        {
            if (_dispatcher == null)
            {
                throw new Exception($"No dispatcher!");
            }

            var taskSource = new TaskCompletionSource<T>();
            if (timeout != default)
            {
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(timeout);
                    // thread pool thread != unity main thread
                    taskSource.TrySetResult(default);
                });
            }
            int typeCode = Activator.CreateInstance<T>().GetTypeCode();
            MessageHandler<IListenerSession, T> handler = new MessageHandler<IListenerSession, T>(typeCode);
            handler.AddHandler(ReceiveCallback);
            void ReceiveCallback(ISession session, T message)
            {
                taskSource.TrySetResult(message);
            }
            if (overrideHandler == true && _dispatcher.TryGetHandler(typeCode, _serviceAreaType, out var exist) == true)
            {
                _handlers.Remove(exist);
                _dispatcher.UnregisterMessageHandlerDirect(typeCode, _serviceAreaType, exist);
            }

            _handlers.Add(handler);
            _dispatcher.RegisterMessageHandlerDirect(typeCode, _serviceAreaType, handler);

            return taskSource.Task;
        }

        public void Dispose()
        {
            UnregisterHandlers();
        }
    }
}