using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Common.Utility;
using MessagePack;
using Newtonsoft.Json;

namespace server_dash.Protocol
{
    // not thread safe.
    public static class JsonProtocolSerializer
    {
        private static Dictionary<int, Func<string, IJsonProtocol>> _deserializers = new Dictionary<int, Func<string, IJsonProtocol>>();
        static JsonProtocolSerializer()
        {

            var protocolTypes = DerivedTypeCache.GetDerivedTypes(typeof(IJsonProtocol));
            MethodInfo deserializeMethod = typeof(JsonProtocolSerializer).GetMethod("DeserializeRawJson");
            foreach (Type protocolType in protocolTypes)
            {
                IJsonProtocol instance = Activator.CreateInstance(protocolType) as IJsonProtocol;
                int typeHash = instance.GetTypeCode();
                _deserializers.Add(typeHash, (Func<string, IJsonProtocol>)Delegate.CreateDelegate(typeof(Func<string, IJsonProtocol>), deserializeMethod.MakeGenericMethod(protocolType)));
            }
        }

        public static IJsonProtocol DeserializeRawJson<T>(string str)
        {
            return (IJsonProtocol)JsonConvert.DeserializeObject<T>(str);
        }

        public static IJsonProtocol Deserialize(string str)
        {
            string[] splited = str.ToString().Split("|");
            int typeHash = BitConverter.ToInt32(Encoding.Unicode.GetBytes(splited[0]));
            return _deserializers[typeHash](splited[1]);
        }

        public static string Serialize<T>(T message) where T : IJsonProtocol
        {
            string typeHash = Encoding.Unicode.GetString(BitConverter.GetBytes(message.GetTypeCode()));
            return $"{typeHash}|{JsonConvert.SerializeObject(message)}";
        }
    }
}