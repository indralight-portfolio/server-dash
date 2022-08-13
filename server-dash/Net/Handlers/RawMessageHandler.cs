using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Utility;
using Dash.Net;
using DotNetty.Common.Concurrency;
using server_dash.Net.Sessions;

namespace server_dash.Net.Handlers
{
    public class RawMessageHandler
    {
        public delegate void RawMessageHandleFunc(ISession session, RawMessage message);
        private RawMessageHandleFunc _fallbackHandler;
        private Dictionary<int, RawMessageHandleFunc> _callbacks = new Dictionary<int, RawMessageHandleFunc>();

        private static readonly Func<MethodInfo, bool> _methodFilter = (methodInfo) => methodInfo.IsPublic == true && methodInfo.IsStatic == false;


        public void Register<T>(T target)
        {
            foreach (var method in TypeInfoHolder<T>.GetMethods(_methodFilter))
            {
                var attribute = (BindRawMessageAttribute)method.GetCustomAttributes(typeof(BindRawMessageAttribute), false)
                    .SingleOrDefault();
                if (attribute == null)
                {
                    continue;
                }

                int hashCode = attribute.HashCode;
                RawMessageHandleFunc func = (RawMessageHandleFunc)method.CreateDelegate(typeof(RawMessageHandleFunc));
                Register(hashCode, func);
            }
        }

        public void Register(int hashCode, RawMessageHandleFunc callback)
        {
            if (_callbacks.ContainsKey(hashCode) == true)
            {
                throw new AlreadyMessageBindingException(hashCode);
            }

            _callbacks.Add(hashCode, callback);
        }

        public void Handle(ISession session, RawMessage message)
        {
            if (_callbacks.TryGetValue(message.TypeCode, out RawMessageHandleFunc callback) == true)
            {
                callback(session, message);
            }
            _fallbackHandler?.Invoke(session, message);
        }

        public void SetFallbackHandler(RawMessageHandleFunc callback)
        {
            _fallbackHandler = callback;
        }
    }
}