using System;
using System.Collections.Generic;
using System.Reflection;
using NLog;
using server_dash.Net.Handlers;

namespace server_dash.Net.Client
{
    public class ClientMessageHandler<TMsg> : IClientMessageHandler
    {
        private List<Action<TMsg>> Handler { get; set; }
        private static Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public int HashCode { get; set; }

        public ClientMessageHandler(int hashCode)
        {
            HashCode = hashCode;
            Handler = new List<Action<TMsg>>();
        }

        public void AddHandler(MethodInfo methodInfo, Type t1, Object target)
        {
            Handler.Add((Action<TMsg>)methodInfo.CreateDelegate(typeof(Action<>).MakeGenericType(t1), target));
        }
        public void AddHandler(Action<TMsg> action)
        {
            Handler.Add(action);
        }

        public bool Handle(object message)
        {
            try
            {
                Handler.ForEach(action => action((TMsg)message));
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
            }

            return true;
        }
    }
}
