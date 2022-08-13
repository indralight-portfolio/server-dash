
using System.Threading.Tasks;
#if Common_Unity
using Common.Unity.Asset;
#endif

namespace Common.StaticInfo.Reader
{
    public abstract class TextReader<T> : IStaticInfoReader<T>
    {
        public abstract void Init(string path);
        public abstract T Read(bool isList);
        public abstract Task<T> ReadAsync(bool isList);

        public static string ReadContents(string path)
        {
#if Common_Unity
            var textAsset = AssetManager.Instance.Load<UnityEngine.TextAsset>(path);
            return textAsset.text;
#else
            return System.IO.File.ReadAllText(path);
#endif
        }
    }
}