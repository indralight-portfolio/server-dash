#if Common_Unity

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Common.Utility;

namespace Common.Unity.Asset
{
    public class LocalBundleService : IAssetService
    {
        private AssetBundleManifest _bundleManifest;
        private static System.TimeSpan UnloadDelay = System.TimeSpan.FromSeconds(3);

        public virtual string BundlesFolder =>
            Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "Bundles", Utility.GetOsNameBuildTarget());

        public string LocalizableBundleType { get; protected set; } = Constant.DefaultLocalizableStr;

        private readonly HashSet<string> _preloadBundles;
        private Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        private Dictionary<string, System.DateTime> _bundlesUsedAt = new Dictionary<string, System.DateTime>();
        public Dictionary<string, AssetBundle> LoadedBundles => _loadedBundles;

        public event System.Action OnUpdateDone;
        public event System.Action<string> OnReceiveBundleMeta;

        public LocalBundleService(string[] preloadBundles)
        {
            _preloadBundles = new HashSet<string>(preloadBundles ?? new string[0]);
        }

        /// interface --------------------------------------------------------------------------------------------------
        public IEnumerator Init()
        {
            string assetBundleManifestPath = $"{Utility.GetOsNameBuildTarget()}:AssetBundleManifest";
            Debug.Log($"AssetBundleManifest path : {assetBundleManifestPath}");
            _bundleManifest = Load<AssetBundleManifest>(assetBundleManifestPath);

            foreach (var bundle in _preloadBundles.ToArray())
            {
                LoadBundle(bundle, true);
            }

            yield break;
        }

        public void Release()
        {
            foreach (AssetBundle bundle in _loadedBundles.Values)
            {
                bundle.Unload(true);
            }

            _loadedBundles.Clear();
            _bundlesUsedAt.Clear();
        }

        public T Load<T>(string assetPath) where T : Object
        {
            var bundleAndAssetName = assetPath.Split(':');

            var bundle = LoadBundle(bundleAndAssetName[0]);
            if (bundle == null)
            {
                Debug.Log($"[LocalBundleService.Load] LoadBundle failed. bundle is null. assetPath : {assetPath}");
                return null;
            }

            T asset;
            if (bundleAndAssetName.Length == 2)
            {
                var assetName = Path.GetFileNameWithoutExtension(bundleAndAssetName[1]);
                asset = bundle.LoadAsset<T>(assetName);
                if (asset == null)
                {
                    Debug.Log($"[LocalBundleService.Load] LoadAsset Name : {assetName}");
                }
            }
            else
            {
                var assetName = bundle.GetAllAssetNames()[0];
                
                asset  = bundle.LoadAsset<T>(assetName);
                if(asset == null)
                {
                    Debug.Log($"[LocalBundleService.Load] LoadAsset Name : {assetName}");
                }
            }

#if UNITY_EDITOR
            if(asset == null)
            {
                Debug.LogWarning("Asset not found : " + assetPath);
            }

            // AB로 로드한 경우 쉐이더를 현 플렛폼의 것으로 다시 연결
            var assetGo = asset as GameObject;
//            if (assetGo != null)
//            {
//                foreach (var renderer in assetGo.GetComponentsInChildren<Renderer>(true))
//                {
//                    renderer.sharedMaterial.LinkShader();
//                }
//                foreach (var renderer in assetGo.GetComponentsInChildren<TrailRenderer>(true))
//                {
//                    renderer.sharedMaterial.LinkShader();
//                }
//            }
#endif

            return asset;
        }

        public void Unload(string path)
        {
        }

        public void SetPreLoadList(string[] preload)
        {
        }

        public IEnumerator PreLoad()
        {
            yield return null;
        }

        /// ------------------------------------------------------------------------------------------------------------

        public string[] GetAssetNames(string path)
        {
            List<string> names = new List<string>();
            var bundleAndAssetName = path.Split(':');

            var bundle = LoadBundle(bundleAndAssetName[0]);
            if (bundle == null)
            {
                return names.ToArray();
            }

            string match = bundleAndAssetName[1].Remove(bundleAndAssetName[1].IndexOf('*')).ToLower();
            string[] result = bundle.GetAllAssetNames();
            string baseName = (Path.GetDirectoryName(bundleAndAssetName[1]) + '/').ToLower();
            for (int i = 0; i < result.Length; ++i)
            {
                if (result[i].Contains(match) == false)
                {
                    continue;
                }

                FileInfo file = new FileInfo(result[i]);
                names.Add(baseName + Path.GetFileNameWithoutExtension(file.Name));
            }
            return names.ToArray();
        }

        public T LoadLocalizable<T>(string path) where T : Object
        {
            // TODO: Localize Load가 필요한가 ?
//            var bundleAndAssetName = path.Split(':');
//            if (bundleAndAssetName.Length != 2)
//            {
//                Debug.LogError($"Localizable Bundle path not usable, {path}");
//                return null;
//            }
//
//            string bundleName = $"{BundleConstants.LocalizableBundlePath}{LocalizableBundleType}";
//
//            var bundle = LoadBundle(bundleName);
//            if (bundle == null)
//            {
//                return null;
//            }
//
//            var assetName = Path.GetFileNameWithoutExtension(bundleAndAssetName[1]);
//            return bundle.LoadAsset<T>(assetName);

            return null;
        }

        public Sprite[] LoadAllSprites(string bundlePath)
        {
            var bundle = LoadBundle(bundlePath);
            if (bundle == null)
            {
                return null;
            }

            return bundle.LoadAllAssets<Sprite>();
        }

        public void LoadScene(string path, LoadSceneMode mode)
        {
            var bundleAndSceneName = path.Split(':');

            LoadBundle(bundleAndSceneName[0]);

            SceneManager.LoadScene(bundleAndSceneName[1], mode);
        }

        public AsyncOperation LoadSceneAsync(string path, LoadSceneMode mode)
        {
            var bundleAndSceneName = path.Split(':');

            LoadBundle(bundleAndSceneName[0]);

            return SceneManager.LoadSceneAsync(bundleAndSceneName[1], mode);
        }

        public IEnumerator LoadBundleAsync(string scenePath)
        {
            yield return null;
        }

        private AssetBundle LoadBundle(string bundlePath, bool preload = false, HashSet<string> visitedBundles = null)
        {
            AssetBundle bundle;

            if (preload)
            {
                _preloadBundles.Add(bundlePath);
            }

            if (!_loadedBundles.TryGetValue(bundlePath, out bundle))
            {
                var path = Path.Combine(BundlesFolder, bundlePath);
                if (!File.Exists(path))
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Bundle not found : " + path);
#endif
                    return null;
                }

                var depBundles = _bundleManifest?.GetAllDependencies(bundlePath) ?? null;
                if (depBundles != null)
                {
                    if (visitedBundles == null)
                    {
                        visitedBundles = new HashSet<string>();
                    }
                    visitedBundles.Add(bundlePath);

                    foreach (var depBundle in depBundles)
                    {
                        if (visitedBundles.Contains(depBundle))
                        {
                            continue;
                        }

                        LoadBundle(depBundle, preload, visitedBundles);
                    }
                }

                bundle = AssetBundle.LoadFromFile(path);
                if (bundle == null)
                {
                    Debug.LogWarning("Failed to load bundle file : " + path);
                    return null;
                }

                _loadedBundles.Add(bundlePath, bundle);
            }

            // preload 번들은 자동 해제하지 않음
            if (!_preloadBundles.Contains(bundlePath))
            {
                _bundlesUsedAt[bundlePath] = System.DateTime.UtcNow + UnloadDelay;
            }

            return bundle;
        }

        public IEnumerator CheckUpdate(Constant.BundleCheckUpdateCallBack callback)
        {
            callback(null, 0, string.Empty);
            yield return null;
        }

        public virtual IEnumerator Update(Constant.BundleUpdateCallback callback)
        {
            yield return null;
        }

        public void Tick()
        {
            return;
#if UNLOAD_BUNDLE
            var now = System.DateTime.UtcNow;

            var expiredBundles = _bundlesUsedAt
                .Where(_ => _.Value < now)
                .Select(_ => _.Key)
                .ToArray();

            foreach (var bundle in expiredBundles)
            {
                UnloadBundle(bundle);
            }
#endif
        }

        private void UnloadBundle(string bundleName)
        {
            AssetBundle bundle;
            if (_loadedBundles.TryGetValue(bundleName, out bundle))
            {
                bundle.Unload(false);
            }

            _loadedBundles.Remove(bundleName);
            _bundlesUsedAt.Remove(bundleName);
        }

        public List<string> GetMatchPaths(string rootPath, string path, string[] exts)
        {

            var bundleName = rootPath.Split(':')[0];
            var bundle = LoadBundle(bundleName);
            if (bundle == null)
            {
                Debug.Log($"[LocalBundleService.Load] LoadBundle failed. bundle is null. assetPath : {rootPath}");
                return null;
            }

            string pattern = exts.Length == 1 ? path + exts[0] : path + ".*";
            string bundlePrefix = Constant.BundlesPath.ToLower() + bundleName;
            var names = bundle.GetAllAssetNames();

            var list = names.Where(name => FileUtility.FitsMask(name, pattern))
                .Select(name => name.Replace(bundlePrefix, rootPath))
                .ToList();

            return list;
        }
    }
}
#endif