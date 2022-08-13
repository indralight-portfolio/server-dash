#if Common_Unity
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace Common.Unity.Asset
{
    /// <summary>
    /// 리소스를 해당 클래스를 통해 로드한다.
    /// </summary>
    public class AssetManager
    {
        private const string ATLAS = "atlas";
        private static IAssetService _assetService = null;

        private List<string> _prepareAtlasList;

        public static AssetManager Instance => _instance ?? (_instance = new AssetManager());
        private static AssetManager _instance;


        private AssetManager()
        {
            SpriteAtlasManager.atlasRequested += RequestAtlas;
        }

        public void Init()
        {
            // 기존에 사용중인 리소스 Release
            _assetService?.Release();
            _assetService = null;
        }

        public IEnumerator Preload()
        {
            yield return _assetService.PreLoad();
        }

        public void SetService(IAssetService assetService)
        {
            _assetService = assetService;
        }

        public void SetPreloadList(List<string> preloadBundles)
        {
            _assetService?.SetPreLoadList(preloadBundles?.ToArray());
        }

        public T Load<T>(string path) where T : Object
        {
            return _assetService?.Load<T>(path);
        }

        public void LoadScene(string path, LoadSceneMode mode)
        {
            _assetService.LoadScene(path, mode);
        }

        public AsyncOperation LoadSceneAsync(string path, LoadSceneMode mode)
        {
            return _assetService.LoadSceneAsync(path, mode);
        }

        public IEnumerator LoadBundleAsync(string scenePath)
        {
            yield return _assetService.LoadBundleAsync(scenePath);
        }

        private void RequestAtlas(string atlasTag, System.Action<SpriteAtlas> callback)
        {
            if (_assetService == null)
            {
#if UNITY_EDITOR
                return;
#else
                throw new System.Exception($"assetService is null! {ATLAS}:{atlasTag}");
#endif
            }
            var spriteAtlas = _assetService.Load<SpriteAtlas>($"{ATLAS}:{atlasTag}");
            callback(spriteAtlas);
        }

        public void UnloadContainPath(string containPath)
        {
            if (_assetService.LoadedBundles == null)
            {
                return;
            }

            List<string> unloadList = new List<string>();

            Dictionary<string, AssetBundle>.Enumerator enumerator = _assetService.LoadedBundles.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Key.Contains(containPath) == true)
                {
                    unloadList.Add(enumerator.Current.Key);
                }
            }

            for (int index = 0; index < unloadList.Count; ++index)
            {
                _assetService.Unload(unloadList[index]);
            }
        }

        public List<string> GetMatchPaths(string rootPath, string path, string[] exts)
        {
            return _assetService.GetMatchPaths(rootPath, path, exts);
        }
    }
}
#endif
