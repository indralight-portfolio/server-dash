using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

#pragma warning disable SYSLIB0011 // 형식 또는 멤버는 사용되지 않습니다.
namespace Common.StaticInfo.Reader
{
    //생각보다 빠르지 않아서 일단쓰지 않음
    public class BinaryReader<T> : BytesReader<T>
    {
        private string _path;
        private byte[] _bytes;
        static BinaryReader()
        {
        }
        public override void Init(string path)
        {
            _path = path;
            _bytes = ReadBytes(_path);
        }

        public override T Read(bool isList)
        {
            using MemoryStream ms = new MemoryStream(_bytes);
            using BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = isList;
            JsonSerializer serializer = new JsonSerializer();
            foreach(var converter in JsonGlobalSettings.ReaderSettings.Converters)
            {
                serializer.Converters.Add(converter);
            }
            return serializer.Deserialize<T>(reader);
        }

        public override async Task<T> ReadAsync(bool isList)
        {
            using MemoryStream ms = new MemoryStream(_bytes);
            using BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = isList;
            JsonSerializer serializer = new JsonSerializer();
            foreach (var converter in JsonGlobalSettings.ReaderSettings.Converters)
            {
                serializer.Converters.Add(converter);
            }
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == false)
            {
                return serializer.Deserialize<T>(reader);
            }
#endif
            T result = await Task.Run(() =>
            {
                try
                {
                    return serializer.Deserialize<T>(reader);

                }
                catch (Exception e)
                {
                    Common.Log.Logger.Error($"BinaryRead<{typeof(T)}>");
                    Common.Log.Logger.Fatal(e);
                    throw;
                }
            });

            return result;
        }
    }
}
#pragma warning restore SYSLIB0011 // 형식 또는 멤버는 사용되지 않습니다.