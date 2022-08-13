using System.Buffers;
using Dash;
using Dash.Net;
using Dash.Protocol;
using DotNetty.Common.Concurrency;
using MessagePack;
#if Common_Unity
using UnityEngine.Profiling;

#endif

namespace Dash.Net.Handlers
{
    using System;
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Codecs;
    using DotNetty.Transport.Channels;
    using Common.Utility;

    public static class MessageEncoder
    {
        #if Common_Unity
        private static Common.Log.ILogger _logger = Common.Log.Logger.Instance;
        #else
        private static readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        #endif

        public static void Encode(IEventExecutor executor, object message, IByteBuffer outputBuffer)
        {
            #if UNITY_EDITOR
            using (new Common.Unity.EditorUtility.ThreadProfileScope("IOThread", "IOThread"))
            #endif
            {
                try
                {
                    if (executor.InEventLoop == false)
                    {
                        throw new Exception("MessageEncoder Encode not called in event loop!");
                    }

                    if (message is IProtocol protocolMessage)
                    {
                        ReadOnlySequence<byte> messageBytes = MessageSerializer.SerializeUnsafe(protocolMessage);
                        int typeCode = protocolMessage.GetTypeCode();
                        int typeCodeSize = MessagePackUtility.GetInt32BufferSize(typeCode);
                        int payloadSize = (int) messageBytes.Length + typeCodeSize;

                        outputBuffer.EnsureWritable(2 + payloadSize);

                        var rawArray = outputBuffer.Array;
                        int arrayOffset = outputBuffer.ArrayOffset + outputBuffer.WriterIndex;
                        int startArrayOffset = arrayOffset;

                        // 1. Write size
                        rawArray[arrayOffset] = (byte)(payloadSize >> 8); 
                        rawArray[arrayOffset + 1] = (byte)payloadSize;

                        arrayOffset += 2;

                        // 2. Write TypeCode
                        arrayOffset += MessagePackUtility.WriteTypeCodeNoEnsureCapacity(outputBuffer.Array,
                            arrayOffset, typeCode);


                        // 3. Write Message Body
                        messageBytes.CopyTo(new Span<byte>(rawArray,
                            arrayOffset, (int) messageBytes.Length));

                        arrayOffset += (int) messageBytes.Length;

                        outputBuffer.SetWriterIndex(outputBuffer.WriterIndex + arrayOffset - startArrayOffset);
                    }
                }
                catch (MessagePackSerializationException ex)
                {
                    _logger.Fatal($"[{message}] {ex.Message} {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        _logger.Fatal(ex.InnerException);
                    }
                    throw new CodecException(ex);
                }
                catch (Exception ex)
                {
                    _logger.Fatal($"[{message}] {ex.Message} {ex.StackTrace}");
                    throw new CodecException(ex);
                }
                finally
                {
                }
            }
        }
    }
}