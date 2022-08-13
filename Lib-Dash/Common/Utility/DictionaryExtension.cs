using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Utility
{
    public static class DictionaryExtension
    {
        public static void Put(this Dictionary<string, ulong> origin, Dictionary<string, ulong> adds)
        {
            foreach(var pair in adds)
            {
                if (origin.ContainsKey(pair.Key) == true)
                {
                    origin[pair.Key] += pair.Value;
                }
                else
                {
                    origin.Add(pair.Key, pair.Value);
                }
            }
        }
        public static void Put(this Dictionary<string, ulong> dictionary, string key, ulong value)
        {
            if (dictionary.ContainsKey(key) == true)
            {
                dictionary[key] += value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}
