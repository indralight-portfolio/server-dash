using System;
using System.Threading;
using Dash.Net.Handlers;
using Dash.Protocol;
using DotNetty.Common.Internal;
using DotNetty.Transport.Channels;

namespace Dash.Net
{
    public class SendQueue
    {
        private readonly IChannel _channel;
        private readonly IEventLoop _eventLoop;

        private readonly IQueue<IProtocol> _sendMessageQueue = new CompatibleConcurrentQueue<IProtocol>();

        // Lambda 대신 Delegate를 사용하여 GC Alloc 회피.
        private readonly Action _eventLoopAction = null;

        private readonly ManualResetEventSlim _sendSlim = new ManualResetEventSlim();
        private readonly ManualResetEventSlim _flushSlim = new ManualResetEventSlim();

        public SendQueue(IChannel channel)
        {
            _channel = channel;
            _eventLoop = channel.EventLoop;

            _eventLoopAction = OnEventLoop;
        }

        public void Send<T>(T message, bool isFlush = true) where T : Dash.Protocol.IProtocol
        {
            if (_eventLoop.InEventLoop)
            {
                // Send()가 호출된 순서대로 실제로 Message가 Outbound Buffer에 작성되지는 않는다.
                // 굳이 EventLoop와 다른 쓰레드간의 전송 순서를 보장해야 할 필요성이 없어서.
                // 나중에 필요해지면 이 코드 지우면 될 듯.

                OutBoundHandler.WriteDirect(_channel, message);
                if (isFlush)
                    _channel.Flush();

                return;
            }

            _sendMessageQueue.TryEnqueue(message);
            _sendSlim.Set();
            if (isFlush)
            {
                _flushSlim.Set();
            }
            _eventLoop.Execute(_eventLoopAction);
        }

        public void Flush()
        {
            if (_eventLoop.InEventLoop)
            {
                _channel.Flush();
            }
            else
            {
                _flushSlim.Set();
                _eventLoop.Execute(_eventLoopAction);
            }
        }

        private void OnEventLoop()
        {
            if (_sendSlim.IsSet)
            {
                _sendSlim.Reset();
                while (_sendMessageQueue.TryDequeue(out IProtocol message))
                {
                    //_channel.WriteAsync(message);
                    OutBoundHandler.WriteDirect(_channel, message);
                }
            }

            if (_flushSlim.IsSet)
            {
                _flushSlim.Reset();
                _channel.Flush();
            }
        }
    }
}