using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Common.StaticInfo.Reader
{
    public class JsonReader<T> : TextReader<T>
    {
        public static JsonSerializerSettings Settings => _settings;
        private static JsonSerializerSettings _settings;

        private string _contents;
        static JsonReader()
        {
            _settings = new JsonSerializerSettings();
            JsonGlobalSettings.ApplyReaderSettings(_settings);
        }

        private string _path;

        public override void Init(string path)
        {
            _path = path;
            _contents = ReadContents(_path);
        }

        public override T Read(bool isList)
        {
            return JsonConvert.DeserializeObject<T>(_contents, _settings);
        }

        public override async Task<T> ReadAsync(bool isList)
        {
            #if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == false)
            {
                return JsonConvert.DeserializeObject<T>(_contents, _settings);
            }
            #endif

            T result = await Task.Run(() =>
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(_contents, _settings);
                }
                catch (Exception e)
                {
                    Common.Log.Logger.Error($"JsonRead<{typeof(T)}>");
                    Common.Log.Logger.Fatal(e);
                    throw;
                }
            });

            return result;
        }

        public static T ReadFrom(string contents)
        {
            return JsonConvert.DeserializeObject<T>(contents, _settings);
        }
    }
}