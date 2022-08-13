using System.Collections;
using System.Collections.Generic;
using Common.Pooling;
using Dash.Net;
using Dash.Protocol;
using NLog;
using server_dash.Net.Handlers;
using server_dash.Net.Sessions;

namespace server_dash.Execution.Runnable
{
    public interface IRunnableMessage
    {
        bool Run();
    }

    public class RunnableParsedMessage : IRunnableMessage, IPoolable
    {
        private static readonly Utility.LogicLogger _logger = new Utility.LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());

        private ISession _session;
        private IProtocol _parsedMessage;
        private MessageHandlerProvider _messageHandlerProvider;
        #region IPoolable

        void IPoolable.OnPrepare()
        {
        }

        void IPoolable.OnReturn()
        {
            _session = null;
            _parsedMessage = null;
            _messageHandlerProvider = null;
        }

        #endregion

        public void Init(ISession session, MessageHandlerProvider messageHandlerProvider, IProtocol parsedMessage)
        {
            _session = session;
            _parsedMessage = parsedMessage;
            _messageHandlerProvider = messageHandlerProvider;
        }

        public bool Run()
        {
            try
            {
                if (_session.IsAlive == false)
                {
                    _logger.Info($"[Dead session:{_session}] Discard message {_parsedMessage}");
                    return true;
                }
                _messageHandlerProvider.GetMessageHandler(_parsedMessage.GetTypeCode(), _session.AreaType).Handle(_session, _parsedMessage);
            }
            catch (System.Exception ex)
            {
                _logger.Fatal(_session, ex.Message + ex.StackTrace);
            }

            return true;
        }
    }

    public class RunnableRawMessage : IRunnableMessage, IPoolable
    {
        private static readonly Utility.LogicLogger _logger = new Utility.LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());

        private ISession _session;
        private RawMessage _rawMessage;
        private MessageHandlerProvider _messageHandlerProvider;
        #region IPoolable

        void IPoolable.OnPrepare()
        {
        }

        void IPoolable.OnReturn()
        {
            _session = null;
            _messageHandlerProvider = null;
        }

        #endregion

        public void Init(ISession session, RawMessage rawMessage)
        {
            _session = session;
            _rawMessage = rawMessage;
        }

        public bool Run()
        {
            try
            {
                if (_session.IsAlive == false)
                {
                    _logger.Info($"[Dead session:{_session}] Discard message {_rawMessage}");
                    return true;
                }

                _messageHandlerProvider.RawMessageHandler.Handle(_session, _rawMessage);
            }
            catch (System.Exception ex)
            {
                _logger.Fatal(_session, ex.Message + ex.StackTrace);
            }

            return true;
        }
    }

    
}