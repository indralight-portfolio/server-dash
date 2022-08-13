using System;
using System.Buffers;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Common.Utility;
using Dash.Command;
using MessagePack;

namespace Dash
{
    using Protocol;
    public static class MessageSerializer
    {
        public static IReadOnlyDictionary<int, Type> Types => _types;
        public static IReadOnlyDictionary<int, Type> CoreCommandTypes => _coreCommandTypes;
        private static Dictionary<int, Func<byte[], IProtocol>> _deserializeBytesFuncs = new Dictionary<int, Func<byte[], IProtocol>>();
        private static Dictionary<int, Func<ArraySegment<byte>, IProtocol>> _deserializeArraySegmentFuncs = new Dictionary<int, Func<ArraySegment<byte>, IProtocol>>();
        private static Dictionary<int, Func<object, byte[]>> _serializeFuncs = new Dictionary<int, Func<object, byte[]>>();
        private static Dictionary<int, Func<object, ReadOnlySequence<byte>>> _unsafeSerializeFuncs = new Dictionary<int, Func<object, ReadOnlySequence<byte>>>();
        private static Dictionary<int, Type> _types = new Dictionary<int, Type>();
        private static Dictionary<int, Type> _coreCommandTypes = new Dictionary<int, Type>();

        private static bool _init = false;
        private static bool _logging = true;
        public static void TurnLogging(bool on) => _logging = on;

        public static void Init(params Assembly[] protocolAssemblies)
        {
            if (_init == true)
            {
                return;
            }

            _init = true;
            Type currentType = null;
            try
            {
                Type iProtocolType = typeof(IProtocol);
                Type iCommandType = typeof(ICommand);
                MethodInfo deserializeBytesMethod = typeof(MessageSerializer).GetMethod("DeserializeBytesInternal");
                MethodInfo deserializeArraySegmentMethod = typeof(MessageSerializer).GetMethod("DeserializeArraySegmentInternal");
                MethodInfo makeBytesMethod = typeof(MessageSerializer).GetMethod("MakeBytesInternal");
                MethodInfo makeBytesUnsafeMethod = typeof(MessageSerializer).GetMethod("MakeBytesUnsafeInternal");


                ReadOnlyCollection<Type> candidateTypes = DerivedTypeCache.GetDerivedTypes(iProtocolType, protocolAssemblies);
                foreach (Type candidateType in candidateTypes)
                {
                    currentType = candidateType;
                    int typeCode = (Activator.CreateInstance(candidateType) as IProtocol).GetTypeCode();
                    if (_logging == true)
                        Common.Log.Logger.Info($"Protocol [{candidateType}] TypeCode : [{typeCode}]");
                    _deserializeBytesFuncs.Add(typeCode,
                        (Func<byte[], IProtocol>)Delegate.CreateDelegate(typeof(Func<byte[], IProtocol>), deserializeBytesMethod.MakeGenericMethod(candidateType)));
                    _deserializeArraySegmentFuncs.Add(typeCode,
                        (Func<ArraySegment<byte>, IProtocol>)Delegate.CreateDelegate(typeof(Func<ArraySegment<byte>, IProtocol>), deserializeArraySegmentMethod.MakeGenericMethod(candidateType)));
                    _serializeFuncs.Add(typeCode,
                        (Func<object, byte[]>)Delegate.CreateDelegate(typeof(Func<object, byte[]>), null, makeBytesMethod.MakeGenericMethod(candidateType)));
                    _unsafeSerializeFuncs.Add(typeCode,
                        (Func<object, ReadOnlySequence<byte>>)Delegate.CreateDelegate(typeof(Func<object, ReadOnlySequence<byte>>), null, makeBytesUnsafeMethod.MakeGenericMethod(candidateType)));
                    _types.Add(typeCode, candidateType);
                    if (iCommandType.IsAssignableFrom(candidateType) == true)
                    {
                        _coreCommandTypes.Add(typeCode, candidateType);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Common.Log.Logger.Fatal(e.InnerException);
                }
                Common.Log.Logger.Fatal($"Type : {currentType}");
                Common.Log.Logger.Fatal(e);
            }
        }

        public static string GetProtocolsString(bool isSimpleName = false)
        {
            var sb = new StringBuilder();
            foreach (KeyValuePair<int,Type> pair in _types)
            {
                sb.Append(pair.Key);
                sb.Append(':');
                if (isSimpleName == true)
                {
                    sb.AppendLine(pair.Value.Name);
                }
                else
                {
                    sb.AppendLine(pair.Value.FullName);
                }
            }

            return sb.ToString();
        }

        public static string ResolveTypeName(int typeCode)
        {
            if (_types.TryGetValue(typeCode, out Type type) == false)
            {
                return typeCode.ToString();
            }

            return type.ToString();
        }

        // 호출 즉시 사용해야함!!
        public static ReadOnlySequence<byte> SerializeUnsafe(IProtocol protocol)
        {
            int typeCode = protocol.GetTypeCode();
            return _unsafeSerializeFuncs[typeCode](protocol);
        }

        public static IProtocol Deserialize(int typeCode, byte[] input)
        {
            return _deserializeBytesFuncs[typeCode](input);
        }

        public static IProtocol Deserialize(int typeCode, ArraySegment<byte> input)
        {
            return _deserializeArraySegmentFuncs[typeCode](input);
        }

        public static IProtocol DeserializeBytesInternal<T>(byte[] bytes)
        {
            return (IProtocol)MessagePackSerializer.Deserialize<T>(bytes);
        }

        public static IProtocol DeserializeArraySegmentInternal<T>(ArraySegment<byte> bytes)
        {
            return (IProtocol)MessagePackSerializer.Deserialize<T>(bytes);
        }

        public static byte[] MakeBytesInternal<T>(object obj) where T : class
        {
            return MessagePackSerializer.Serialize(obj);
        }

        public static ReadOnlySequence<byte> MakeBytesUnsafeInternal<T>(object obj) where T : class
        {
            return MessagePackUtility.SerializeUnsafe(obj as T);
        }
    }
}