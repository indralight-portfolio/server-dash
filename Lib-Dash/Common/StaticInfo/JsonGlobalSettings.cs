using Newtonsoft.Json;

namespace Common.StaticInfo
{
    public static class JsonGlobalSettings
    {
        public static JsonSerializerSettings WriterSettings { get; } = new JsonSerializerSettings();
        public static JsonSerializerSettings ReaderSettings { get; } = new JsonSerializerSettings();

        public static void ApplyWriterSettings(JsonSerializerSettings settings)
        {
            foreach (JsonConverter converter in WriterSettings.Converters)
            {
                settings.Converters.Add(converter);
            }
        }

        public static void ApplyReaderSettings(JsonSerializerSettings settings)
        {
            foreach (JsonConverter converter in ReaderSettings.Converters)
            {
                settings.Converters.Add(converter);
            }
        }
    }
}