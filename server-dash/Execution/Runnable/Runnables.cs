using System;

namespace server_dash.Execution.Runnable
{
    public class NoOperation : IRunnableMessage
    {
        public bool Run()
        {
            return true;
        }
    }

    public class ActionMessage : IRunnableMessage
    {
        private readonly Action _action;

        public ActionMessage(Action action)
        {
            _action = action;
        }

        public bool Run()
        {
            _action?.Invoke();
            return true;
        }
    }
}