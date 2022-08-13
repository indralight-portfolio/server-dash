using System.Collections;
using System.Reflection;
using NLog;
using server_dash.Execution.Runnable;

namespace server_dash.Net.Handlers
{
    using System;
    using System.Collections.Generic;
    using Net.Sessions;
    using DotNetty.Common.Concurrency;

    public class MessageHandler<TSession, TMsg> : IMessageHandler where TSession : ISession
    {
        private List<Action<TSession, TMsg>> Handler { get; set; }
        private static Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        
        public int HashCode { get; set; }

        public MessageHandler(int hashCode)
        {
            HashCode = hashCode;
            Handler = new List<Action<TSession, TMsg>>();
        }

        public void AddHandler(MethodInfo methodInfo, Type t1, Type t2, Object target)
        {
            Handler.Add((Action<TSession, TMsg>) methodInfo.CreateDelegate(typeof( Action<,>).MakeGenericType(t1, t2), target));
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
                    _logger.Error($"[MessageHandler] {session} Unable to cast to {typeof(TSession)}, HasCode : {HashCode}");
                }
                
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"HashCode : {HashCode}, " + e.Message + e.StackTrace);
            }

            return true;
        }
    }

    public interface ICoroutineMessageHandler : IMessageHandler
    {
        new IEnumerator Handle(CoroutineContext context, ISession session, object message);
    }

    public class CoroutineMessageHandler<TSession, TMsg> : ICoroutineMessageHandler where TSession : ISession
    {
        private Func<CoroutineContext, TSession, TMsg, IEnumerator> _handler;
        private static Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        
        public int HashCode { get; set; }

        public CoroutineMessageHandler(int hashCode)
        {
            HashCode = hashCode;
        }

        public void AddHandler(MethodInfo methodInfo, Type t1, Type t2, Object target)
        {
            if (_handler != null)
            {
                throw new NotSupportedException($"Multiple coroutine message handler not supported! HashCode : {HashCode}");
            }

            _handler = (Func<CoroutineContext, TSession, TMsg, IEnumerator>)methodInfo.CreateDelegate(typeof(Func<,,,>).MakeGenericType(typeof(CoroutineContext), t1, t2, typeof(IEnumerator)), target);
        }

        IEnumerator ICoroutineMessageHandler.Handle(CoroutineContext context, ISession session, object message)
        {
            try
            {
                if (session is TSession tSession)
                {
                    return _handler(context, tSession, (TMsg)message);
                }

                _logger.Error($"[MessageHandler] {session} Unable to cast to {typeof(TSession)}, HasCode : {HashCode}");
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"HashCode : {HashCode}, " + e.Message + e.StackTrace);
            }

            return null;
        }

        bool IMessageHandler.Handle(ISession session, object message)
        {
            _logger.Fatal(new NotSupportedException()); // TODO: 이거 호출안되게
            return true;
        }
    }
}