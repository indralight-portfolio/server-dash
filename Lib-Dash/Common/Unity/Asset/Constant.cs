#if Common_Unity

using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Common.Unity.Asset
{
    public static class Constant
    {
        public delegate void BundleCheckUpdateCallBack(string error, long updateSize, string other);
        public delegate void BundleUpdateCallback(BundleUpdateProgressInfo progressInfo);

        public const string DefaultLocalizableStr = "english";
        public const string BundlesPath = "Assets/_Bundles/";
    }

    public class BundleUpdateProgressInfo
    {
        public BundleUpdateProgressInfo.CountAndSize Total;
        public BundleUpdateProgressInfo.CountAndSize Completed;
        public long InProgressSize;

        public string Error;

        public class CountAndSize
        {
            public int Count;
            public long Size;
        }

        public BundleUpdateProgressInfo(Manifest updateManifest)
        {
            Total = new CountAndSize()
            {
                Count = updateManifest.Items.Count,
                Size = updateManifest.TotalSize
            };
            Completed = new CountAndSize();
        }
    }

    public class BundleEntity
    {
        private int _tryCount = 0;
#if UNITY_EDITOR
        private static readonly int MaxTryCount = 1;
#else
        private static readonly int MaxTryCount = 5;
#endif
        public int TryCount => _tryCount;

        public Manifest.Item Item;
        public IEnumerator DownloadCoroutine;
        public string Error;
        public long ReceivedSize;

        public bool ExceedMaxTry()
        {
            return MaxTryCount <= _tryCount;
        }
        public UnityWebRequest CreateWWW(string cdn, string name)
        {
            ++_tryCount;
            return UnityWebRequestAssetBundle.GetAssetBundle(System.IO.Path.Combine(cdn, name));
        }
    }
}

#endif