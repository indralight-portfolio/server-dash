#if Common_Unity
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Common.Unity.Asset
{
    public class Manifest
    {
        public struct Item
        {
            public string Name;
            public long Size;
            public string Hash;

            public override bool Equals(object obj)
            {
                var other = (Item)obj;
                return other.Name == Name && other.Size == Size && other.Hash == Hash;
            }

            public override int GetHashCode()
            {
                var hashCode = -1367729276;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + Size.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Hash);
                return hashCode;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public long TotalSize
        {
            get
            {
                return Items.Sum(i => i.Size);
            }
        }

        public List<Item> Items { get; private set; } = new List<Item>();

        public static Manifest FromJson(string text)
        {
            var manifest = new Manifest();
            if (text == null)
            {
                return manifest;
            }

            var jtr = new JsonTextReader(new StringReader(text));
            var js = new JsonSerializer();

            manifest.AddRange(js.Deserialize<List<Manifest.Item>>(jtr));

            return manifest;
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(Items, Formatting.Indented);
        }

        public void Add(Item item)
        {
            Items.Add(item);
        }

        public void AddRange(IEnumerable<Item> items)
        {
            Items.AddRange(items);
        }

        public void Remove(Item item)
        {
            Items.Remove(item);
        }

        public void Remove(IEnumerable<Item> removeItems)
        {
            foreach (Item item in removeItems)
            {
                Items.Remove(item);
            }
        }

        public Manifest Subtract(Manifest manifest)
        {
            var changed = new Manifest();
            changed.AddRange(Items.Except(manifest.Items));
            return changed;
        }

        public void Save(string path)
        {
            var fileInfo = new FileInfo(path);

            if(!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            File.WriteAllText(fileInfo.FullName, JsonConvert.SerializeObject(Items, Formatting.Indented));
        }
    }
}

#endif