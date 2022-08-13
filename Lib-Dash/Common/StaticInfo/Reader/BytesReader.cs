using System.Threading.Tasks;

namespace Common.StaticInfo.Reader
{
    public abstract class BytesReader<T> : IStaticInfoReader<T>
    {
        public abstract void Init(string path);
        public abstract T Read(bool isList);
        public abstract Task<T> ReadAsync(bool isList);

        public static byte[] ReadBytes(string path)
        {
#if Common_Unity
            var textAsset = Unity.Asset.AssetManager.Instance.Load<UnityEngine.TextAsset>(path);
            return textAsset.bytes;
#else
            return System.IO.File.ReadAllBytes(path);
#endif
        }
    }
}
