using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.StaticInfo.Writer
{
    public class JsonWriter<T> : LocalFileWriter<T>
    {
        public static JsonSerializerSettings Settings => _settings;
        private static JsonSerializerSettings _settings;

        static JsonWriter()
        {
            _settings = new JsonSerializerSettings();
            JsonGlobalSettings.ApplyWriterSettings(_settings);
        }

        public override void Write(T data)
        {
            var stream = base.CreateFileStream();

            stream.Write(MakeJson(data));

            stream.Close();
        }

        public static string MakeJson<T>(T data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented, _settings);
        }
    }
}