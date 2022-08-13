using Microsoft.Extensions.Configuration;

namespace server_dash
{
    public static class ConfigManager
    {
        private static IConfigurationRoot _config;
        public static void Init(IConfigurationRoot config)
        {
            _config = config;
        }

        public static string Get(string key)
        {
            return _config[key];
        }

        public static string Get(string key, string defaultValue)
        {
            return _config[key] ?? defaultValue;
        }

        public static T Get<T>(string section)
        {
            return _config.GetSection(section).Get<T>();
        }
    }
}