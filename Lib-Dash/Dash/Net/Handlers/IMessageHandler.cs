using System;
using System.Reflection;
using Dash.Net.Sessions;

namespace Dash.Net.Handlers
{
    public enum MessageHandlerType
    {
        SessionHandler,
        CommandHandler,
        CoroutineHandler,
        RelayHandler,
    }
    public interface IMessageHandler
    {
        int TypeCode { get; set; }
        void AddHandler(MethodInfo method, Type t1, Type t2, Object target);
        bool Handle(ISession session, object message);
        MessageHandlerType HandlerType { get; }
    }

    public interface IMessageHandler<T>
    {
        bool Handle(ISession session, T message);
    }
    public interface IDispatchCheckHandler
    {
        int TypeCode { get; set; }
        bool CanDispatch(ISession session, object message);
    }
}