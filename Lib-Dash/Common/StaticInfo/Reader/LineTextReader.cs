using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Common.StaticInfo.Reader
{
    public class LineTextReader : TextReader<List<string>>
    {
        private string _path;

        public override void Init(string path)
        {
            _path = path;
        }

        public override List<string> Read(bool isList)
        {
            string contents = ReadContents(_path);
            using (StringReader stringReader = new StringReader(contents))
            {
                List<string> result = new List<string>();
                string str = null;
                while ((str = stringReader.ReadLine()) != null)
                {
                    result.Add(str);
                }

                return result;
            }
        }

        public override Task<List<string>> ReadAsync(bool isList)
        {
            string contents = ReadContents(_path);
            using (StringReader stringReader = new StringReader(contents))
            {
                List<string> result = new List<string>();
                string str = null;
                while ((str = stringReader.ReadLine()) != null)
                {
                    result.Add(str);
                }

                return Task.FromResult(result);
            }
        }

        public HashSet<string> ReadAsHashSet()
        {
            string contents = ReadContents(_path);
            using (StringReader stringReader = new StringReader(contents))
            {
                HashSet<string> result = new HashSet<string>();
                string str = null;
                while ((str = stringReader.ReadLine()) != null)
                {
                    result.Add(str);
                }

                return result;
            }
        }
    }
}