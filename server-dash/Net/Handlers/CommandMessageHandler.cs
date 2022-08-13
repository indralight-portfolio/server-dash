using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Utility;
using Dash.Command;
using server_dash.Net.Sessions;

namespace server_dash.Net.Handlers
{
    public class CommandMessageHandler : IMessageHandler
    {
        public delegate void CommandMessageHandleFunc(ISession session, ICommand message);
        private CommandMessageHandleFunc _handler;

        public CommandMessageHandler(CommandMessageHandleFunc callback)
        {
            _handler = callback;
        }

        bool IMessageHandler.Handle(ISession session, object message)
        {
            Handle(session, message as ICommand);
            return true;
        }

        void IMessageHandler.AddHandler(MethodInfo method, Type t1, Type t2, object target)
        {
            throw new NotSupportedException();
        }

        public void Handle(ISession session, ICommand message)
        {
            _handler(session, message);
        }
    }
}