using System;
using System.Reflection;
using server_dash.Net.Sessions;
using DotNetty.Common.Concurrency;

namespace server_dash.Net.Handlers
{
    public interface IMessageHandler
    {
        void AddHandler(MethodInfo method, Type t1, Type t2, Object target);
        bool Handle(ISession session, object message);
    }

    public interface IMessageHandler<T>
    {
        bool Handle(ISession session, T message);
    }
    public interface IClientMessageHandler
    {
        void AddHandler(MethodInfo method, Type t1, Object target);
        bool Handle(object message);
    }
}