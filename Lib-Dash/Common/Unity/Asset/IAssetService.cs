#if Common_Unity
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Common.Unity.Asset
{
    public interface IAssetService
    {
        Dictionary<string, AssetBundle> LoadedBundles { get; }

        IEnumerator Init();
        void Release();
        T Load<T>(string path) where T : Object;
        void Unload(string path);
        void LoadScene(string path, LoadSceneMode mode);
        AsyncOperation LoadSceneAsync(string path, LoadSceneMode mode);
        IEnumerator LoadBundleAsync(string scenePath);
        void SetPreLoadList(string[] preload);
        IEnumerator PreLoad();

        List<string> GetMatchPaths(string rootPath, string path, string[] exts);

        event System.Action OnUpdateDone;
        event System.Action<string> OnReceiveBundleMeta;
    }
}

#endif