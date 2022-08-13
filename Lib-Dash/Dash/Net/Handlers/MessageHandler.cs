using System.Collections;
using System.Reflection;
using Dash.Net.Sessions;

namespace Dash.Net.Handlers
{
    using System;
    using System.Collections.Generic;

    public class MessageHandler<TSession, TMsg> : IMessageHandler where TSession : ISession
    {
        private List<Action<TSession, TMsg>> Handler { get; set; }
        
        public MessageHandlerType HandlerType { get => MessageHandlerType.SessionHandler; }
        public int TypeCode { get; set; }

        public MessageHandler(int hashCode)
        {
            TypeCode = hashCode;
            Handler = new List<Action<TSession, TMsg>>();
        }

        public void AddHandler(MethodInfo methodInfo, Type t1, Type t2, Object target)
        {
            Handler.Add((Action<TSession, TMsg>) methodInfo.CreateDelegate(typeof( Action<,>).MakeGenericType(t1, t2), target));
        }
        public void AddHandler(Action<TSession, TMsg> action)
        {
            Handler.Add(action);
        }
        public bool Handle(ISession session, object message)
        {
            try
            {
                if (session is TSession tSession)
                {
                    for (int i = 0; i < Handler.Count; ++i)
                    {
                        Handler[i](tSession, (TMsg) message);
                    }
                }
                else
                {
                    Common.Log.Logger.Instance.Error($"[MessageHandler] {session} Unable to cast to {typeof(TSession)}, HasCode : {TypeCode}");
                }
                
            }
            catch (Exception e)
            {
                Common.Log.Logger.Instance.Error($"HashCode : {TypeCode}, " + e.Message + e.StackTrace);
            }

            return true;
        }
    }

    public class CallbackHandler<TSession, TMsg> : IMessageHandler where TSession : ISession
    {
        private Action<TSession, TMsg> Handler { get; set; }
        
        public MessageHandlerType HandlerType { get => MessageHandlerType.SessionHandler; }
        public int TypeCode { get; set; }

        public CallbackHandler(int hashCode, Action<TSession, TMsg> callback)
        {
            TypeCode = hashCode;
            Handler = callback;
        }

        public void AddHandler(MethodInfo methodInfo, Type t1, Type t2, Object target)
        {
            throw new NotSupportedException();
        }
        public void AddHandler(Action<TSession, TMsg> action)
        {
            throw new NotSupportedException();
        }
        public bool Handle(ISession session, object message)
        {
            try
            {
                if (session is TSession tSession)
                {
                    Handler(tSession, (TMsg) message);
                }
                else
                {
                    Common.Log.Logger.Instance.Error($"[MessageHandler] {session} Unable to cast to {typeof(TSession)}, HasCode : {TypeCode}");
                }
                
            }
            catch (Exception e)
            {
                Common.Log.Logger.Instance.Error($"HashCode : {TypeCode}, " + e.Message + e.StackTrace);
            }

            return true;
        }
    }
}