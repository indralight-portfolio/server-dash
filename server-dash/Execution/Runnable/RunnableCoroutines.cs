using System;
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
    public class CoroutineContext : IPoolable
    {
        public Exception ChildException;

        public void OnPrepare()
        {
        }

        public void OnReturn()
        {
            ChildException = null;
        }

        public void MarkExceptionChecked()
        {
            ChildException = null;
        }
        
    }

    public class RunnableCoroutine : IRunnableMessage, IPoolable
    {
        private static readonly Utility.LogicLogger _logger =
            new Utility.LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());

        private CoroutineContext _context = new CoroutineContext();
        private IEnumerator _coroutine;
        private Stack<IEnumerator> _coroutines = new Stack<IEnumerator>();

        #region IPoolable
        void IPoolable.OnPrepare()
        {
            _context.OnPrepare();
        }

        void IPoolable.OnReturn()
        {
            _context.OnReturn();
            _coroutine = null;
            _coroutines.Clear();
        }
        #endregion

        public void Init(Func<CoroutineContext, IEnumerator> func)
        {
            _coroutine = func(_context);
        }

        public bool Run()
        {
            try
            {
                if (_coroutine.MoveNext() == false)
                {
                    if (_coroutines.TryPop(out IEnumerator parent) == false)
                    {
                        return true;
                    }

                    _coroutine = parent;
                    return false;
                }

                object current = _coroutine.Current;
                if (current is IEnumerator child)
                {
                    _coroutines.Push(_coroutine);
                    _coroutine = child;
                    return false;
                }

                if (current != null)
                    throw new Exception($"Coroutine yield not appropriate object : {current}");

                return false;
            }
            catch (System.AggregateException ex)
            {
                ex.Handle(e =>
                {
                    _logger.Fatal(e.Message + e.StackTrace);
                    return true;
                });
                _context.ChildException = ex;
                // continue upper coroutine.
                if (_coroutines.TryPop(out IEnumerator parent) == false)
                {
                    return true;
                }

                _coroutine = parent;
                return false;
            }
            catch (System.Exception ex)
            {
                _logger.Fatal(ex.Message + ex.StackTrace);
                _context.ChildException = ex;
                // continue upper coroutine.
                if (_coroutines.TryPop(out IEnumerator parent) == false)
                {
                    return true;
                }

                _coroutine = parent;
                return false;
            }
        }
    }

    public class RunnableCoroutineMessage : IRunnableMessage, IPoolable
    {
        private static readonly Utility.LogicLogger _logger =
            new Utility.LogicLogger(Common.Log.NLogUtility.GetCurrentClassLogger());

        private CoroutineContext _context = new CoroutineContext();
        private ISession _session;
        private IProtocol _message;
        private IEnumerator _coroutine;
        private Stack<IEnumerator> _coroutines = new Stack<IEnumerator>();
        private MessageHandlerProvider _messageHandlerProvider;
        #region IPoolable

        void IPoolable.OnPrepare()
        {
            _context.OnPrepare();
        }

        void IPoolable.OnReturn()
        {
            _context.OnReturn();
            _session = null;
            _message = null;
            _coroutine = null;
            _coroutines.Clear();
            _messageHandlerProvider = null;
        }

        #endregion

        public void Init(ISession session, MessageHandlerProvider messageHandlerProvider, IProtocol message)
        {
            _session = session;
            _message = message;
            _messageHandlerProvider = messageHandlerProvider;
        }

        public bool Run()
        {
            try
            {
                if (_coroutine == null)
                {
                    // 한번 Coroutine이 시작되면, Session의 Alive 여부와 상관없이 완료시킨다.
                    if (_session.IsAlive == false)
                    {
                        _logger.Info($"[Dead session:{_session}] Discard message {_message}");
                        return true;
                    }

                    _coroutine = _messageHandlerProvider.GetCoroutineMessageHandler(
                            _message.GetTypeCode(),_session.AreaType)
                        .Handle(_context, _session, _message);
                }

                if (_coroutine.MoveNext() == false)
                {
                    if (_coroutines.TryPop(out IEnumerator parent) == false)
                    {
                        return true;
                    }

                    _coroutine = parent;
                    return false;
                }

                object current = _coroutine.Current;
                if (current is IEnumerator child)
                {
                    _coroutines.Push(_coroutine);
                    _coroutine = child;
                    return false;
                }

                if (current != null)
                    throw new Exception($"Coroutine yield not appropriate object : {current}");

                return false;
            }
            catch (System.AggregateException ex)
            {
                ex.Handle(e =>
                {
                    _logger.Fatal(_session, e.Message + e.StackTrace);
                    return true;
                });
                _context.ChildException = ex;
                // continue upper coroutine.
                if (_coroutines.TryPop(out IEnumerator parent) == false)
                {
                    return true;
                }

                _coroutine = parent;
                return false;
            }
            catch (System.Exception ex)
            {
                _logger.Fatal(_session, ex.Message + ex.StackTrace);
                _context.ChildException = ex;
                // continue upper coroutine.
                if (_coroutines.TryPop(out IEnumerator parent) == false)
                {
                    return true;
                }

                _coroutine = parent;
                return false;
            }
        }
    }
}