using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Dash.Model.GamePlay;
using MessagePack;

namespace Dash
{
    [MessagePackObject()]
    public struct MessageInfo
    {
        [Key(0)]
        public uint Timestamp;
        [Key(1)]
        public int TypeCode;
        [Key(2)]
        public byte[] Bytes;
    }

    [MessagePackObject()]
    public class PlayerInfo
    {
        [Key(0)]
        public string Version;
        [Key(1)]
        public ulong OidAccount;
        [Key(2)]
        public string Nickname;
        [Key(3)]
        public int PlayerId;
        [Key(4)]
        public int CoreId;
    }

    [MessagePackObject()]
    public class RecordMetadata : ILoggable
    {
        [Key(0)]
        public string Version;
        [Key(1)]
        public DateTime Date;
        [Key(2)]
        public List<PlayerInfo> Players;
        [Key(3)]
        public int TotalMessageBytes;
        ////////////Optional/////////////
        [Key(4)]
        public UndoneGame UndoneGame;

        [IgnoreMember]
        public string LogStr => Prefix(this);
        public static string Prefix(RecordMetadata metadata)
        {
            var sb = new StringBuilder();
            if (metadata.Players != null)
            {
                sb.Append("[oid:");
                for (int i = 0; i < metadata.Players.Count; ++i)
                {
                    sb.Append('@');
                    sb.Append(metadata.Players[i]?.OidAccount ?? 0);
                    if (i != metadata.Players.Count - 1)
                    {
                        sb.Append('|');
                    }
                }

                sb.Append("]");
            }
            return sb.ToString();
        }
    }

    [MessagePackObject, MessagePackFormatter(typeof(MessageStreamCustomFormatter))]
    public class MessageStream
    {
        ~MessageStream()
        {
            if (_buffer != null)
            {
                _buffer = null;
                throw new Exception("Buffer not released!");
            }
        }

        [Key(0)]
        public int Count;

        [IgnoreMember]
        public List<MessageInfo> Deserialized = null;
        [IgnoreMember]
        public int StartWriterIndex;

        [IgnoreMember]
        public Dictionary<int, (int Count, int PayloadSize)> Statistics;

        public int GetTotalSize()
        {
            if (_buffer == null)
            {
                throw new Exception("No Buffer!");
            }

            return _buffer.WriterIndex - StartWriterIndex;
        }

        public DotNetty.Buffers.IByteBuffer GetBuffer() => _buffer;
        [IgnoreMember]
        private DotNetty.Buffers.IByteBuffer _buffer;
        public void SetBuffer(DotNetty.Buffers.IByteBuffer buffer)
        {
            _buffer = buffer;
            if (_buffer != null)
            {
                StartWriterIndex = _buffer.WriterIndex;
            }
        }


        public void Write(long timestamp, int typeCode, ReadOnlySequence<byte> payload, bool addStatistics)
        {
            if(_buffer == null)
            {
                return;
            }
            try
            {
                ++Count;
                _buffer.WriteLong(timestamp);
                _buffer.WriteInt(typeCode);
                _buffer.WriteUnsignedShort((ushort)payload.Length);
                if (payload.Length > 0)
                {
                    _buffer.EnsureWritable((int) payload.Length);
                    payload.CopyTo(new Span<byte>(_buffer.Array, _buffer.ArrayOffset + _buffer.WriterIndex,
                        (int) payload.Length));
                    _buffer.SetWriterIndex(_buffer.WriterIndex + (int) payload.Length);
                }
                if (addStatistics == true)
                {
                    AddStatistics(typeCode, (int)payload.Length);
                }

            }
            catch (Exception e)
            {
                Common.Log.Logger.Fatal(e);
            }
        }

        private void AddStatistics(int typeCode, int payloadSize)
        {
            if (Statistics == null)
            {
                Statistics = new Dictionary<int, (int Count, int PayloadSize)>();
            }

            if (Statistics.TryGetValue(typeCode, out (int Count, int PayloadSize) p) == true)
            {
                Statistics[typeCode] = (p.Count + 1, p.PayloadSize + payloadSize);
            }
            else
            {
                Statistics[typeCode] = (1, payloadSize);
            }
        }
    }

    public class MessageStreamCustomFormatter : MessagePack.Formatters.IMessagePackFormatter<MessageStream>
    {
        public void Serialize(ref MessagePackWriter writer, MessageStream value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }
            
            writer.WriteInt32(value.Count);
            // write buffer
            var buffer = value.GetBuffer();
            buffer.SetReaderIndex(value.StartWriterIndex);
            for (int i = 0; i < value.Count; ++i)
            {
                uint timestamp = (uint)buffer.ReadLong();
                writer.WriteUInt32(timestamp);
                int typeCode = buffer.ReadInt();
                writer.WriteInt32(typeCode);
                ushort payloadSize = buffer.ReadUnsignedShort();
                if (payloadSize <= 0)
                {
                    writer.WriteNil();
                }
                if (payloadSize > 0)
                {
                    writer.WriteRaw(new ReadOnlySpan<byte>(buffer.Array, buffer.ArrayOffset + buffer.ReaderIndex, payloadSize));
                    buffer.SetReaderIndex(buffer.ReaderIndex + payloadSize);
                }
            }
        }

        public MessageStream Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }
            
            var result = new MessageStream();

            int count = reader.ReadInt32();

            if (count > 0)
            {
                result.Deserialized = new List<MessageInfo>();
                for (int i = 0; i < count; ++i)
                {
                    uint timestamp = reader.ReadUInt32();
                    int typeCode = reader.ReadInt32();
                    byte[] payload = null;
                    if (reader.IsNil == false)
                    {
                        payload = reader.ReadRaw().ToArray();
                    }
                    else
                    {
                        reader.ReadNil();
                    }
            
                    result.Deserialized.Add(new MessageInfo()
                    {
                        Timestamp = timestamp,
                        TypeCode = typeCode,
                        Bytes = payload
                    });
                }
            }

            return result;
        }
    }

    [MessagePackObject()]
    public class MessageRecords
    {
        [Key(0)]
        public RecordMetadata Metadata;
        [Key(1)]
        public Dictionary<int, MessageStream> ServerInbound;
        [Key(2)]
        public Dictionary<int, MessageStream> ServerOutbound;
        [Key(3)]
        public MessageStream ServerBroadcast;
        [IgnoreMember]
        public Dictionary<int /*TypeCode*/, (int Count, int Size)> ServerBeginMessages;
    }
}