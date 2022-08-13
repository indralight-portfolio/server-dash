using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;

namespace Dash.Net.Handlers
{
    public sealed class FrameDecoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            #if UNITY_EDITOR
            using (new Common.Unity.EditorUtility.ThreadProfileScope("IOThread", "IOThread"))
            #endif
            {

                input.MarkReaderIndex();
                int readerIndex1 = input.ReaderIndex;
                input.MarkReaderIndex();
                ushort length = 0;
                if (input.ReadableBytes < 2)
                {
                    input.ResetReaderIndex();
                }
                else
                {
                    byte upper = input.ReadByte();
                    byte lower = input.ReadByte();
                    length = (ushort) ((upper << 8) + lower);
                }

                int readerIndex2 = input.ReaderIndex;
                if (readerIndex1 == readerIndex2)
                    return;
                if (length < 0)
                    throw new CorruptedFrameException(string.Format("Negative length: {0}", (object) length));
                if (input.ReadableBytes < length)
                {
                    input.ResetReaderIndex();
                }
                else
                {
                    IByteBuffer byteBuffer = input.ReadSlice(length);
                    output.Add((object) byteBuffer.Retain());
                }
            }
        }
    }
}