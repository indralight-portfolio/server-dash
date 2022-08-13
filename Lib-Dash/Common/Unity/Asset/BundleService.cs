#if Common_Unity
using Object = UnityEngine.Object;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common.Utility;
using UnityEngine;
using UnityEngine.Networking;
using Common.Unity.Utility;
using UnityEngine.SceneManagement;
using Logger = Common.Log.Logger;

namespace Common.Unity.Asset
{
    public class BundleService : IAssetService
    {
        private AssetBundleManifest _bundleManifest;

        public virtual string BundlesFolder => Path.Combine(Application.persistentDataPath, "Bundles");

        private HashSet<string> _preloadBundles = new HashSet<string>();
        private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        public Dictionary<string, AssetBundle> LoadedBundles => _loadedBundles;

        public event System.Action OnUpdateDone;
        public event System.Action<string> OnReceiveBundleMeta;

        private readonly IBundleUpdateController _bundleUpdateController;

        private string _cdnUrl;

        public const string ManifestFileName = "manifest.json";
        public const string MetaFileName = "meta.json";
        private const float MegaByte = 1024 * 1024f;

        private Manifest _updateManifest;
        private Manifest _newLocalManifest;
        private Manifest _deprecatedManifest;

        private string platform => Utility.GetOsName();

        public BundleService(IBundleUpdateController bundleUpdateController)
        {
            _bundleUpdateController = bundleUpdateController;
        }

        public void SetUrl(string url, string version)
        {
            _cdnUrl = url.Replace("{version}", version).Replace("{platform}", platform) + '/';
            Debug.Log("asset bundle url setting " + _cdnUrl);
        }

        /// interface --------------------------------------------------------------------------------------------------
        public IEnumerator Init()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            _loadedBundles.Clear();

            yield return CheckAndUpdate();

            string assetBundleManifestPath = $"{platform}:AssetBundleManifest";
            Debug.Log($"AssetBundleManifest path : {assetBundleManifestPath}");
            _bundleManifest = Load<AssetBundleManifest>(assetBundleManifestPath);
        }

        public void Release()
        {
            foreach (AssetBundle bundle in _loadedBundles.Values)
            {
                bundle.Unload(true);
            }

            _loadedBundles.Clear();
        }

        public T Load<T>(string assetPath) where T : Object
        {
            var bundleAndAssetName = assetPath.Split(':');

            LoadBundle(bundleAndAssetName[0], out AssetBundle bundle);
            if (bundle == null)
            {
                Debug.Log($"[BundleService.Load] LoadBundle failed. bundle is null. assetPath : {assetPath}");
                return null;
            }

            T asset = null;
            string assetName = string.Empty;
            if (bundleAndAssetName.Length == 2)
            {
                assetName = Path.GetFileNameWithoutExtension(bundleAndAssetName[1]);
                asset = bundle.LoadAsset<T>(assetName);
            }
            else
            {
                assetName = bundle.GetAllAssetNames()[0];
                asset = bundle.LoadAsset<T>(assetName);
            }

            if (asset == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning(
                #else
                Debug.Log(
                #endif
                $"[BundleService.Load] LoadAsset Name : {assetName}");
            }

            return asset;
        }

        public void Unload(string bundlePath)
        {
            if (_loadedBundles.TryGetValue(bundlePath, out AssetBundle assetBundle) == true)
            {
                assetBundle.Unload(true);
                _loadedBundles.Remove(bundlePath);
            }
        }

        public void LoadScene(string path, LoadSceneMode mode)
        {
            var bundleAndSceneName = path.Split(':');
            LoadBundle(bundleAndSceneName[0], out AssetBundle assetBundle);
            SceneManager.LoadScene(bundleAndSceneName[1], mode);
        }

        public AsyncOperation LoadSceneAsync(string path, LoadSceneMode mode)
        {
            var bundleAndSceneName = path.Split(':');
            LoadBundle(bundleAndSceneName[0], out AssetBundle assetBundle);
            return SceneManager.LoadSceneAsync(bundleAndSceneName[1], mode);
        }

        public IEnumerator LoadBundleAsync(string scenePath)
        {
            var bundleAndSceneName = scenePath.Split(':');
            yield return LoadBundleAsync(bundleAndSceneName[0], null);
        }

        public void SetPreLoadList(string[] preload)
        {
            _preloadBundles = new HashSet<string>(preload ?? new string[0]);
        }

        public IEnumerator PreLoad()
        {
            foreach (var bundle in _preloadBundles.ToArray())
            {
                LoadBundle(bundle, out AssetBundle assetBundle);
                yield return null;
            }
        }

        /// ------------------------------------------------------------------------------------------------------------

        private IEnumerator CheckAndUpdate()
        {
            var bundleUpdate = _bundleUpdateController;
            if (bundleUpdate == null)
            {
                UnityEngine.Debug.Log("[BundleService.CheckAndUpdate] bundleUpdate is null");
                yield break;
            }

            string error = null;
            long size = 0;
            string other = null;

            while (true)
            {
                bundleUpdate.Show();

                yield return CheckUpdate((e, s, o) =>
                {
                    error = e;
                    size = s;
                    other = o;
                });

                if (error == null)
                {
                    break;
                }

                UnityEngine.Debug.LogError($"BundleUpdate Error : {error}, Other : {other}");
                bundleUpdate.ShowAdditionalMessage(BundleMessageType.NetworkError, 0);

                yield return bundleUpdate.WaitForConfirm(ConfirmType.Retry);
            }

            UnityEngine.Debug.Log($"Error : {error}, Size : {size}, Other : {other}");

            if (size > 0)
            {
                var sizeMB = size / MegaByte;
                bundleUpdate.ShowAdditionalMessage(BundleMessageType.AdditionalAssets, sizeMB);
                yield return bundleUpdate.WaitForConfirm(ConfirmType.Ok);

                var progressInfo = new BundleUpdateProgressInfo(_updateManifest);

                while (true)
                {
                    bundleUpdate.HideAdditionalMessage();
                    progressInfo.Error = null;

                    yield return Update(progressInfo, progress =>
                    {
                        bundleUpdate.SetProgressInfo(progress);
                        error = progress.Error;
                    });

                    if (error == null)
                    {
                        break;
                    }

                    UnityEngine.Debug.Log($"BundleUpdate Error : {error}");
                    bundleUpdate.ShowAdditionalMessage(BundleMessageType.NetworkError, sizeMB);

                    yield return bundleUpdate.WaitForConfirm(ConfirmType.Retry);
                }
            }

            OnUpdateDone?.Invoke();
            _bundleUpdateController.Hide();
        }

        public IEnumerator CheckUpdate(Constant.BundleCheckUpdateCallBack callback)
        {
            string otherDesc = string.Empty;
            string localManifestPath = Path.Combine(BundlesFolder, ManifestFileName);

            // IOS 앱 최초 실행시, 앱에 포함된 Bundle을 Bundle Folder에 복사한다.
            if (File.Exists(localManifestPath) == false && (Application.platform == RuntimePlatform.IPhonePlayer /*||
                                                            Application.platform == RuntimePlatform.OSXEditor*/))
            {
                DirectoryInfo dataDirectory = new DirectoryInfo(Application.dataPath);
                foreach (FileInfo fileInfo in dataDirectory.GetFiles())
                {
                    Debug.Log($"DataDirectory File : {fileInfo.FullName}");
                }

                DirectoryInfo bundleDirectory = null;
                foreach (DirectoryInfo directoryInfo in dataDirectory.GetDirectories())
                {
                    Debug.Log($"DataDirectory Directory : {directoryInfo.FullName}");
                    if (directoryInfo.Name == platform)
                    {
                        bundleDirectory = directoryInfo;
                        break;
                    }
                }

                otherDesc += "PackedABCopied";

                if (bundleDirectory == null)
                {
                    Debug.LogError("BundleDirectory not found");
                }
                else
                {
                    bool errorOccurs = false;
                    var errorMsg = String.Empty;

                    try
                    {
                        FileUtility.CopyFolderRecursive(bundleDirectory.FullName, BundlesFolder, true);
                    }
                    catch (Exception ex)
                    {
                        errorMsg = Utility.GetIoErrorMessage(ex);
                        errorOccurs = true;
                    }

                    if (errorOccurs == true)
                    {
                        callback(errorMsg, 0, otherDesc);
                        yield break;
                    }
                }
            }

            yield return null;

            using (var www = new UnityWebRequest(Path.Combine(_cdnUrl, MetaFileName)))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                yield return www.SendWebRequest();
                if (www.error != null)
                {
                    callback(www.error, 0, www.url);
                    yield break;
                }

                string res = www.downloadHandler.text;
                OnReceiveBundleMeta?.Invoke(res);
            }

            yield return null;

            Manifest remoteManifest;
            using (var www = new UnityWebRequest(Path.Combine(_cdnUrl, ManifestFileName)))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                yield return www.SendWebRequest();
                if (www.error != null)
                {
                    callback(www.error, 0, www.url);
                    yield break;
                }

                string res = www.downloadHandler.text;
                remoteManifest = Manifest.FromJson(res);                
            }

            yield return null;

            Debug.Log("Check Manifest path : " + localManifestPath);
            Manifest localManifest = new Manifest();
            var localManifestExistence = File.Exists(localManifestPath);
            if (localManifestExistence == true)
            {
                var jsonString = File.ReadAllText(localManifestPath);
                yield return null;

                localManifest = Manifest.FromJson(jsonString);
            }

            yield return null;

            // Remote와 Local의 차이점을 구한다.
            _updateManifest = remoteManifest.Subtract(localManifest);

            foreach (var item in _updateManifest.Items)
            {
                var localItem = localManifest.Items.FirstOrDefault(e => e.Name == item.Name);
                Debug.Log($"update manifest item : {item}{Environment.NewLine}{localItem.Hash}/{localItem.Size} : {item.Hash}/{item.Size}");
            }

            // 기존 로컬에 있던 Item + Update할 Item -> 최신 local manifest.
            _newLocalManifest = new Manifest();
            _newLocalManifest.AddRange(_updateManifest.Items);
            foreach (Manifest.Item localItem in localManifest.Items)
            {
                if (_updateManifest.Items.Exists((i) => i.Name == localItem.Name) == false)
                {
                    _newLocalManifest.Add(localItem);
                }
            }

            // 기존 로컬 중에 리모트에 없는 것 -> 만료된 번들
            _deprecatedManifest = new Manifest();
            for (int i = 0; i < localManifest.Items.Count; ++i)
            {
                Manifest.Item localItem = localManifest.Items[i];
                bool existence = false;
                for (int j = 0; j < remoteManifest.Items.Count; ++j)
                {
                    Manifest.Item remoteItem = remoteManifest.Items[j];
                    if (remoteItem.Name == localItem.Name)
                    {
                        existence = true;
                        break;
                    }
                }
                if (existence == false)
                {
                    _newLocalManifest.Remove(localItem);
                    _deprecatedManifest.Add(localItem);
                }
            }

            RemoveDeprecatedBundles(_deprecatedManifest);
            _deprecatedManifest = null;

            callback(null, _updateManifest.TotalSize, otherDesc);
        }

        private IEnumerator Update(BundleUpdateProgressInfo progressInfo, Constant.BundleUpdateCallback callback)
        {
            yield return null;
            if (_updateManifest == null)
            {
                yield break;
            }

            List<BundleEntity> downloadEntities = new List<BundleEntity>();

            callback(progressInfo);
            foreach (var item in _updateManifest.Items)
            {
                var bundleEntity = new BundleEntity()
                {
                    Item = item,
                };

                bundleEntity.DownloadCoroutine = DownloadBundle(bundleEntity);
                downloadEntities.Add(bundleEntity);
            }

            while (downloadEntities.Count > 0)
            {
                yield return null;

                progressInfo.InProgressSize = 0;
                for (int i = 0; i < downloadEntities.Count;)
                {
                    BundleEntity bundleEntity = downloadEntities[i];
                    if (bundleEntity.DownloadCoroutine.Current is CustomYieldInstruction cy)
                    {
                        if (cy.keepWaiting == true)
                        {
                            ++i;
                            continue;
                        }
                        else
                        {
                            bundleEntity.DownloadCoroutine = DownloadBundle(bundleEntity); // 첨부터 다시 시도
                            ++i;
                            continue;
                        }
                    }

                    if (bundleEntity.DownloadCoroutine.MoveNext() == true)
                    {
                        // 실제로 다운로드 받는 사이즈가 Manifest Item의 크기보다 큰 경우가 있어서 이렇게 처리.
                        long uiSize = Math.Min(bundleEntity.ReceivedSize, bundleEntity.Item.Size);
                        progressInfo.InProgressSize += uiSize;
                        ++i;
                        continue;
                    }

                    // 완료, 혹은 실패의 경우.
                    downloadEntities.RemoveAt(i);

                    // 한개라도 에러나면 처음부터 다시 처리되게끔.
                    if (bundleEntity.Error != null)
                    {
                        Logger.Instance.Error($"{bundleEntity.Item} download error!");
                        progressInfo.Error = bundleEntity.Error;
                        callback(progressInfo);
                        yield break;
                    }

                    ++progressInfo.Completed.Count;
                    progressInfo.Completed.Size += bundleEntity.Item.Size;
                }

                callback?.Invoke(progressInfo);
            }

            yield return null;

            var manifestFi = new FileInfo(Path.Combine(BundlesFolder, ManifestFileName));
            if (!Directory.Exists(manifestFi.DirectoryName))
            {
                Directory.CreateDirectory(manifestFi.DirectoryName);
            }

            bool ioException = false;
            string errMsg = string.Empty;
            try
            {
                File.WriteAllText(manifestFi.FullName, _newLocalManifest.ToJsonString());
                UnityEngine.Debug.Log($"Save Manifest File : {manifestFi.FullName}");
#if UNITY_IOS
                UnityEngine.iOS.Device.SetNoBackupFlag(manifestFi.FullName);
#endif
            }
            catch (Exception ex)
            {
                errMsg = Utility.GetIoErrorMessage(ex);
                ioException = true;
            }

            if (ioException == true)
            {
                progressInfo.Error = errMsg;
                callback(progressInfo);
                yield break;
            }

            yield return null;
        }

        private IEnumerator DownloadBundle(BundleEntity bundleEntity)
        {
            bundleEntity.Error = null;
            bundleEntity.ReceivedSize = 0;
            Manifest.Item item = bundleEntity.Item;
            using (var www = bundleEntity.CreateWWW(_cdnUrl, item.Name))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SendWebRequest();
                UnityEngine.Debug.Log($"Download Bundle {Path.Combine(_cdnUrl, item.Name)}");

                while (!www.isDone)
                {
                    yield return null;
                    //UnityEngine.Debug.Log($"item {item.Name} / isDone : {www.isDone} / downloadProgress : {www.downloadProgress}");
                    if (www.error != null)
                    {
                        Debug.Log($"[DownloadBundle] Error : {www.error}.");
                        if (bundleEntity.ExceedMaxTry())
                        {
                            Debug.LogError($"[DownloadBundle] Item {item}");
                            Debug.Log($"[DownloadBundle] ExceedMaxTry.");
                            bundleEntity.Error = www.error;
                            yield break;
                        }
                        else
                        {
                            Debug.Log($"[DownloadBundle] retry download. {bundleEntity.TryCount} ");
                            yield return new WaitForSecondsRealtime(1.0f);

                            // 이렇게 호출하면 받는 쪽에서는 coroutine.Current is IEnumerator일경우 current를 moveNext해줘야함.
                            // 이게 총 다섯 depth니까 대략 coroutine.Current.Current.Current.Current.MoveNext 요런식으로
                            // 접근해야 해서 재귀함수 써야 하는데 위에서 걍 깔끔하게 함.
                            //yield return DownloadBundle(bundleEntity);
                            yield break;
                        }
                    }

                    bundleEntity.ReceivedSize = (long)www.downloadedBytes;
                }

                var errorOccurs = false;
                var errorMsg = string.Empty;

                try
                {
                    var itemFi = new FileInfo(Path.Combine(BundlesFolder, item.Name));
                    if (!Directory.Exists(itemFi.DirectoryName))
                    {
                        Directory.CreateDirectory(itemFi.DirectoryName);
                    }

                    File.WriteAllBytes(itemFi.FullName, www.downloadHandler.data);
#if UNITY_IOS
                    UnityEngine.iOS.Device.SetNoBackupFlag(itemFi.FullName);
#endif
                }
                catch (Exception ex)
                {
                    // ref : https://stackoverflow.com/questions/9293227/how-to-check-if-ioexception-is-not-enough-disk-space-exception-type
                    // catch (IOException ex) when ((ex.HResult & 0xFFFF) == 0x27 || (ex.HResult & 0xFFFF) == 0x70)
                    // 와 같이 쓸 수도 있지만 다른 io exception이 발생할 수도 있기 때문에 아래 처럼 처리.
                    errorMsg = Utility.GetIoErrorMessage(ex);
                    errorOccurs = true;
                    throw;
                }

                if (errorOccurs == true)
                {
                    bundleEntity.Error = errorMsg;
                    yield break;
                }

                _updateManifest.Remove(item);
            }
        }

        private void LoadBundle(string bundlePath, out AssetBundle assetBundle, HashSet<string> visitedBundles = null)
        {
            if (!_loadedBundles.TryGetValue(bundlePath, out assetBundle))
            {
                var path = Path.Combine(BundlesFolder, bundlePath);
                if (!File.Exists(path))
                {
                    UnityEngine.Debug.LogWarning("Bundle not found : " + path);
                    return;
                }

                var dependentBundles = _bundleManifest?.GetAllDependencies(bundlePath) ?? null;
                if (dependentBundles != null)
                {
                    if (visitedBundles == null)
                    {
                        visitedBundles = new HashSet<string>(_loadedBundles.Keys);
                    }
                    visitedBundles.Add(bundlePath);

                    foreach (var depBundle in dependentBundles)
                    {
                        if (visitedBundles.Contains(depBundle))
                        {
                            continue;
                        }

                        LoadBundle(depBundle, out AssetBundle dependency, visitedBundles);
                    }
                }

                assetBundle = AssetBundle.LoadFromFile(path);
                if (assetBundle == null)
                {
                    UnityEngine.Debug.LogWarning("Failed to load bundle file : " + path);
                    return;
                }

                _loadedBundles.Add(bundlePath, assetBundle);
            }
        }

        private IEnumerator LoadBundleAsync(string bundlePath, HashSet<string> visitedBundles)
        {
            if (!_loadedBundles.TryGetValue(bundlePath, out var assetBundle))
            {
                var path = Path.Combine(BundlesFolder, bundlePath);
                if (!File.Exists(path))
                {
                    Debug.LogWarning("Bundle not found : " + path);
                    yield break;
                }

                var dependentBundles = _bundleManifest?.GetAllDependencies(bundlePath) ?? null;
                if (dependentBundles != null)
                {
                    if (visitedBundles == null)
                    {
                        visitedBundles = new HashSet<string>();
                    }
                    visitedBundles.Add(bundlePath);

                    foreach (var depBundle in dependentBundles)
                    {
                        if (visitedBundles.Contains(depBundle))
                        {
                            continue;
                        }

                        yield return LoadBundleAsync(depBundle, visitedBundles);
                    }
                }

                var t = AssetBundle.LoadFromFileAsync(path);
                yield return new WaitUntil(() => t.isDone == true);
                if (t.assetBundle == null)
                {
                    UnityEngine.Debug.LogWarning("Failed to load bundle file : " + path);
                    yield break;
                }

                _loadedBundles.Add(bundlePath, t.assetBundle);
            }
        }

        private void RemoveBundle(Manifest.Item item)
        {
            if (string.IsNullOrEmpty(item.Name))
            {
                return;
            }

            var itemFi = new FileInfo(Path.Combine(BundlesFolder, item.Name));

            var directoryExistence = Directory.Exists(itemFi.DirectoryName);
            if (directoryExistence == false)
            {
                return;
            }

            var fileExistence = File.Exists(itemFi.FullName);
            if (fileExistence == false)
            {
                return;
            }

            File.Delete(itemFi.FullName);
        }

        private static void RemoveEmptyDirectory(string startPath)
        {
            var directories = Directory.GetDirectories(startPath);
            foreach (var directory in directories)
            {
                RemoveEmptyDirectory(directory);
                var childDirectories = Directory.GetDirectories(directory);
                var files = Directory.GetFiles(directory);

                if (files.Length < 1 && childDirectories.Length < 1)
                {
                    Directory.Delete(directory);
                    continue;
                }
            }
        }

        private void RemoveDeprecatedBundles(Manifest deprecatedManifest)
        {
            if (deprecatedManifest == null)
            {
                return;
            }

            if (deprecatedManifest.Items.Count < 1)
            {
                return;
            }

            foreach (var item in deprecatedManifest.Items)
            {
                RemoveBundle(item);
            }

            RemoveEmptyDirectory(BundlesFolder);
        }

        public void RemoveBundleFolder()
        {
            DirectoryInfo di = new DirectoryInfo(BundlesFolder);
            if (di.Exists == false)
            {
                return;
            }

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            Debug.Log($"<color=blue>RemoveBundleFolder succeeded.</color>");
        }

        public List<string> GetMatchPaths(string rootPath, string path, string[] exts)
        {
            var bundleName = rootPath.Split(':')[0];
            LoadBundle(bundleName, out AssetBundle bundle);
            if (bundle == null)
            {
                Debug.Log($"[BundleService.Load] LoadBundle failed. bundle is null. assetPath : {rootPath}");
                return null;
            }

            string pattern = exts.Length == 1 ? path + exts[0] : path + ".*";
            string bundlePrefix = Constant.BundlesPath.ToLower() + bundleName;
            var names = bundle.GetAllAssetNames();            

            var list = names.Where(name => FileUtility.FitsMask(name, pattern))
                .Where(name=> FileUtility.FilterExtensions(name, exts))
                .Select(name => name.Replace(bundlePrefix, rootPath))
                .ToList();

            return list;
        }
    }
}
#endif