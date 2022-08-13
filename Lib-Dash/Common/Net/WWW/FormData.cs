using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Common.Net.WWW
{
    public class FormData
    {
        private Dictionary<object, object> _dic = new Dictionary<object, object>();
        public Dictionary<string, string> Data => _dic.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());

        public string JsonString => JsonConvert.SerializeObject(_dic);

        private FormData()
        {
        }

        public static FormData Make(params object[] args)
        {
            var messageData = new FormData { _dic = ToDictionary(args) };
            return messageData;
        }

        public object Get(object key)
        {
            if (_dic.ContainsKey(key))
            {
                return _dic[key];
            }
            return null;
        }

        public void Add(string key, object value)
        {
            _dic.Add(key, value);
        }

        public void Set(string key, object value)
        {
            _dic[key] = value;
        }

        private static Dictionary<object, object> ToDictionary(params object[] args)
        {
            if (args == null || args.Length % 2 != 0)
            {
                return new Dictionary<object, object>();
            }
            else
            {
                var dic = new Dictionary<object, object>();
                for (int index = 0; index < args.Length - 1; index += 2)
                {
                    dic.Add(args[index], args[index + 1]);
                }
                return dic;
            }
        }
    }
}