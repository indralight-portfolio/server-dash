using System;
using System.Buffers;
using MessagePack;

namespace Dash
{
    public class MessagePackUtility
    {
        public static int GetInt32BufferSize(int value)
        {
            if (value >= 0)
            {
                // positive int(use uint)
                if (value <= MessagePackRange.MaxFixPositiveInt)
                {
                    return 1;
                }
                else if (value <= byte.MaxValue)
                {
                    return 2;
                }
                else if (value <= ushort.MaxValue)
                {
                    return 3;
                }
                else
                {
                    return 5;
                }
            }
            else
            {
                // negative int(use int)
                if (MessagePackRange.MinFixNegativeInt <= value)
                {
                    return 1;
                }
                else if (sbyte.MinValue <= value)
                {
                    return 2;
                }
                else if (short.MinValue <= value)
                {
                    return 3;
                }
                else
                {
                    return 5;
                }
            }
        }

        public static int WriteTypeCodeNoEnsureCapacity(byte[] bytes, int offset, int value)
        {
            if (value >= 0)
            {
                // positive int(use uint)
                if (value <= MessagePackRange.MaxFixPositiveInt)
                {
                    bytes[offset] = unchecked((byte)value);
                    return 1;
                }
                else if (value <= byte.MaxValue)
                {
                    bytes[offset] = MessagePackCode.UInt8;
                    bytes[offset + 1] = unchecked((byte)value);
                    return 2;
                }
                else if (value <= ushort.MaxValue)
                {
                    bytes[offset] = MessagePackCode.UInt16;
                    bytes[offset + 1] = unchecked((byte)(value >> 8));
                    bytes[offset + 2] = unchecked((byte)value);
                    return 3;
                }
                else
                {
                    bytes[offset] = MessagePackCode.UInt32;
                    bytes[offset + 1] = unchecked((byte)(value >> 24));
                    bytes[offset + 2] = unchecked((byte)(value >> 16));
                    bytes[offset + 3] = unchecked((byte)(value >> 8));
                    bytes[offset + 4] = unchecked((byte)value);
                    return 5;
                }
            }
            else
            {
                // negative int(use int)
                if (MessagePackRange.MinFixNegativeInt <= value)
                {
                    bytes[offset] = unchecked((byte)value);
                    return 1;
                }
                else if (sbyte.MinValue <= value)
                {
                    bytes[offset] = MessagePackCode.Int8;
                    bytes[offset + 1] = unchecked((byte)value);
                    return 2;
                }
                else if (short.MinValue <= value)
                {
                    bytes[offset] = MessagePackCode.Int16;
                    bytes[offset + 1] = unchecked((byte)(value >> 8));
                    bytes[offset + 2] = unchecked((byte)value);
                    return 3;
                }
                else
                {
                    bytes[offset] = MessagePackCode.Int32;
                    bytes[offset + 1] = unchecked((byte)(value >> 24));
                    bytes[offset + 2] = unchecked((byte)(value >> 16));
                    bytes[offset + 3] = unchecked((byte)(value >> 8));
                    bytes[offset + 4] = unchecked((byte)value);
                    return 5;
                }
            }
        }

        public static int TryReadTypeCode(byte[] bytes, int offset, int readableCount, out int typeCode)
        {
            byte header = bytes[offset];
            var reader = new MessagePackReader(new ReadOnlySequence<byte>(bytes, offset, readableCount));
            if (header <= MessagePackCode.MaxFixInt && readableCount >= 1)
            {
                typeCode = reader.ReadInt32();
                return (int)reader.Consumed;
            }
            if (MessagePackCode.MinNegativeFixInt <= header && readableCount >= 1)
            {
                typeCode = reader.ReadInt32();
                return (int)reader.Consumed;
            }
            if (header == MessagePackCode.UInt8)
            {
                typeCode = reader.ReadInt32();
                return (int)reader.Consumed;
            }
            if (header == MessagePackCode.UInt16)
            {
                typeCode = reader.ReadInt32();
                return (int)reader.Consumed;
            }
            if (header == MessagePackCode.UInt32)
            {
                typeCode = reader.ReadInt32();
                return (int)reader.Consumed;
            }
            if (header == MessagePackCode.Int8)
            {
                typeCode = reader.ReadInt32();
                return (int)reader.Consumed;
            }
            if (header == MessagePackCode.Int16)
            {
                typeCode = reader.ReadInt32();
                return (int)reader.Consumed;
            }
            if (header == MessagePackCode.Int32)
            {
                typeCode = reader.ReadInt32();
                return (int)reader.Consumed;
            }

            typeCode = 0;
            return 0;
        }

        [ThreadStatic]
        private static Nerdbank.Streams.Sequence<byte> _messageBytes; 
        public static ReadOnlySequence<byte> SerializeUnsafe<T>(T obj)
        {
            if (_messageBytes == null)
            {
                _messageBytes = new Nerdbank.Streams.Sequence<byte>();
            }

            _messageBytes.Reset();
            MessagePackSerializer.Serialize(_messageBytes, obj);

            return _messageBytes.AsReadOnlySequence;
        }
    }
}