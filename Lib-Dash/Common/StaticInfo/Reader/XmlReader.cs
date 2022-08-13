using Common.Utility;
using System.IO;
using System.Threading.Tasks;

namespace Common.StaticInfo.Reader
{
    public class XmlReader<T> : TextReader<T>
    {
        private XmlDeserializer _deserializer = new XmlDeserializer(typeof(T));
        private string _path;

        public override void Init(string path)
        {
            _path = path;
        }

        public override T Read(bool isList)
        {
            using(StringReader reader = new StringReader(ReadContents(_path)))
            {
                return (T)_deserializer.Deserialize(reader);
            }
        }

        public override Task<T> ReadAsync(bool isList)
        {
            using(StringReader reader = new StringReader(ReadContents(_path)))
            {
                return Task.FromResult((T)_deserializer.Deserialize(reader));
            }
        }
    }
}