using System;
using System.IO;
using System.Threading.Tasks;
#if Common_Unity
using Common.Unity.Asset;
#endif

namespace Common.StaticInfo.Reader
{
    public class MPackReader<T> : BytesReader<T>
    {
        private string _path;
        private byte[] _bytes;

        static MPackReader()
        {
        }

        public override void Init(string path)
        {
            _path = path;
            _bytes = ReadBytes(path);
        }

        public override T Read(bool isList)
        {
            return MessagePack.MessagePackSerializer.Deserialize<T>(_bytes);
        }

        public async override Task<T> ReadAsync(bool isList)
        {
            #if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == false)
            {
                return MessagePack.MessagePackSerializer.Deserialize<T>(_bytes);
            }
            #endif

            T result = await Task.Run(() =>
            {
                try
                {
                    T deserialized = MessagePack.MessagePackSerializer.Deserialize<T>(_bytes);
                    return deserialized;

                }
                catch (Exception e)
                {
                    Common.Log.Logger.Error($"MPackRead<{typeof(T)}>");
                    Common.Log.Logger.Fatal(e);
                    throw;
                }
            });

            return result;
        }
    }
}