using System;
using System.Collections.Generic;
using Dash.Protocol;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace Dash.Net.Handlers
{
    public sealed class CompositeDecoder : ByteToMessageDecoder
    {
        #if Common_Unity
        private Common.Log.ILogger _logger = Common.Log.Logger.Instance;
        #else
        private readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        #endif

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            #if UNITY_EDITOR
            using (new Common.Unity.EditorUtility.ThreadProfileScope("IOThread", "IOThread"))
                #endif
            {
                int typeCode = 0;
                try
                {

                    int readableBytes = input.ReadableBytes;
                    int startReaderIndex = input.ReaderIndex;
                    if (readableBytes < 2)
                    {
                        return;
                    }

                    var rawArray = input.Array;
                    int arrayOffset = input.ArrayOffset + startReaderIndex;

                    byte upper = rawArray[arrayOffset];
                    byte lower = rawArray[arrayOffset + 1];
                    int length = (ushort) ((upper << 8) + lower);

                    if (length <= 0)
                    {
                        throw new CodecException($"Invalid Length : {length}");
                    }

                    if (readableBytes < 2 + length)
                    {
                        // need to receive more bytes
                        return;
                    }

                    int startArrayOffset = arrayOffset;

                    arrayOffset += 2;
                    readableBytes -= 2;

                    int typeCodeSize = MessagePackUtility.TryReadTypeCode(rawArray, arrayOffset, readableBytes, out typeCode);
                    arrayOffset += typeCodeSize;
                    readableBytes -= typeCodeSize;
                    int messageBodySize = length - typeCodeSize;

                    IProtocol deserialized = MessageSerializer.Deserialize(typeCode, new ArraySegment<byte>(rawArray, arrayOffset, messageBodySize));
                    output.Add(deserialized);

                    arrayOffset += messageBodySize;
                    input.SetReaderIndex(startReaderIndex + (arrayOffset - startArrayOffset));
                }
                catch (Exception e)
                {
                    _logger.Fatal($"[Codec][{typeCode}] Channel : {context.Channel}, Message : {e.Message}");
                                    context.Channel.WriteAndFlushAsync(new InvalidCodec() {TargetTypeCode = typeCode});
                                    context.Channel.CloseAsync();
                }
            }
        }
    }
}