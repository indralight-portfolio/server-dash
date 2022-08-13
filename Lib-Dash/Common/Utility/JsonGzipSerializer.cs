using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Common.Utility
{
    public static class JsonGzipSerializer
    {
        public static byte[] Serialize<T>(T value)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                });
            //Common.Log.Logger.Debug($"json1 : {json}");
            var bytes = Zip(json);
            return bytes;
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            string json = Unzip(bytes);
            T value = JsonConvert.DeserializeObject<T>(json);

            return value;
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso); ;
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}