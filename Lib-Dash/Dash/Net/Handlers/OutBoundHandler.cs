using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Threading.Tasks;

namespace Dash.Net.Handlers
{
    public class OutBoundHandler : ChannelHandlerAdapter
    {
        public override bool IsSharable => true;

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            IByteBuffer output = (IByteBuffer) null;
            Task task;
            try
            {
                output = context.Allocator.Buffer();
                try
                {
                    MessageEncoder.Encode(context.Executor, message, output);
                }
                finally
                {
                    ReferenceCountUtil.Release(message);
                }

                if (output.IsReadable())
                {
                    task = context.WriteAsync((object) output);
                }
                else
                {
                    output.Release();
                    task = context.WriteAsync((object) Unpooled.Empty);
                }

                output = (IByteBuffer) null;
            }
            catch (EncoderException ex)
            {
                return TaskEx.FromException((Exception) ex);
            }
            catch (Exception ex)
            {
                return TaskEx.FromException((Exception) new EncoderException(ex));
            }
            finally
            {
                output?.Release();
            }

            return task;
        }

        public static void WriteDirect(IChannel channel, object message)
        {
            IByteBuffer output = null;
            try
            {
                output = channel.Allocator.Buffer();
                try
                {
                    MessageEncoder.Encode(channel.EventLoop, message, output);
                }
                finally
                {
                    ReferenceCountUtil.Release(message);
                }

                if (output.IsReadable())
                {
                    int size = output.ReadableBytes;
                    var outboundBuffer = channel.Unsafe.OutboundBuffer;
                    outboundBuffer?.AddMessage(output, size, DotNetty.Common.Concurrency.TaskCompletionSource.Void);
                }
                else
                {
                    output.Release();
                }

                output = (IByteBuffer) null;
            }
            finally
            {
                output?.Release();
            }
        }
    }
}