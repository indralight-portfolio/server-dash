using Common.Log;
using Common.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Common.StaticInfo
{
    public interface IKeyValueInfo
    {
        IEnumerator Init(string rootPath, bool truncate = true, bool isReload = false, bool isToolMode = false);
        void PostInit();
        void Save(string rootPath);
        Type GetKeyType();
        Type GetValueType();
        string GetPath();
        StaticInfoType GetStaticInfoType();
        StaticInfoFormatType GetFormatType();
        object GetRaw(object key);
        IDictionary GetRawDictionary();
    }

    public class KeyValueInfo<TKey, TValue> : IKeyValueInfo where TValue : IKeyValueData<TKey>
    {
        private static readonly Type KeyType = typeof(TKey);
        private static readonly Type ValueType = typeof(TValue);
        private readonly Dictionary<TKey, TValue> _infos;
        private readonly string _path;
        private readonly StaticInfoFormatType _formatType;
        private readonly StaticInfoType _staticInfoType;

        public Action OnSaveCallback;

        public int Count()
        {
            return _infos.Count;
        }

        public TValue this[TKey key]
        {
            get => this[key, true];
            set => _infos.Add(key, value);
        }

        public TValue this[TKey key, bool logging]
        {
            get
            {
                if (_infos.ContainsKey(key) == false)
                {
                    if (logging == true)
                        Logger.Fatal($"StaticInfo {typeof(TValue)} not found. Name : {key}");
                    return default;
                }

                return _infos[key];
            }
        }

        public bool TryGet(TKey key, out TValue data)
        {
            return _infos.TryGetValue(key, out data);
        }

        public bool Exist(TKey key)
        {
            return _infos.ContainsKey(key);
        }

        public Type GetKeyType() => KeyType;
        public Type GetValueType() => ValueType;
        public string GetPath() => _path;
        public StaticInfoType GetStaticInfoType() => _staticInfoType;
        public StaticInfoFormatType GetFormatType() => _formatType;

        object IKeyValueInfo.GetRaw(object key)
        {
            TryGet((TKey)key, out TValue info);
            return info;
        }

        IDictionary IKeyValueInfo.GetRawDictionary() => _infos;

        public Dictionary<TKey, TValue>.KeyCollection GetKeys()
        {
            return _infos.Keys;
        }

        public Dictionary<TKey, TValue>.ValueCollection GetList()
        {
            return _infos.Values;
        }

        public Dictionary<TKey, TValue> GetInfos()
        {
            return _infos;
        }

        public KeyValueInfo(string path, StaticInfoFormatType formatType, StaticInfoType staticInfoType, IEqualityComparer<TKey> comparer = null)
        {
            _path = path;
            _formatType = formatType;
            _staticInfoType = staticInfoType;
            if (comparer != null)
            {
                _infos = new Dictionary<TKey, TValue>(comparer);
            }
            else
            {
                _infos = new Dictionary<TKey, TValue>();
            }
        }

        public IEnumerator Init(string rootPath, bool truncate = true, bool isReload = false, bool isToolMode = false)
        {
            //_formatType이 MPackOrJson일 경우 toolMode에선 json을 읽고 아닌경우 MPack으로 읽어야 함
            StaticInfoFormatType formatType = _formatType.GetReadType(isToolMode);
            List<string> paths = FileUtility.GetFilePathsFromPattern(rootPath, _path, FileExtResolver.Resolve(formatType));
            bool initFailed = false;

            if (truncate == true)
            {
                _infos.Clear();
            }

            string currentPath = string.Empty;
            Task<List<TValue>> readTask = null;
            if (paths.Count > 1)
            {
                throw new Exception($"readTask 다중 지원되게 수정해줘야 함");
            }

            try
            {
                foreach (string path in paths)
                {
                    currentPath = path;
                    Logger.Debug($"Init: {typeof(TValue).Name} {nameof(path)} : {path}, {nameof(truncate)} : {truncate}, {nameof(isReload)} : {isReload}");
                    Reader.IStaticInfoReader<List<TValue>> reader = Reader.ReaderFactory.Create<List<TValue>>(path, formatType);
                    readTask = reader.ReadAsync(true);
                }
            }
            catch (FileNotFoundException ex)
            {
                initFailed = true;
                Logger.Fatal($"Resource not found : {currentPath}. Exception Message : {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"KeyValueInfo {typeof(TValue).Name} Init failed, path : {currentPath}");
                initFailed = true;
                Logger.Fatal(ex);
            }

            if (readTask == null || readTask.IsFaulted)
            {
                initFailed = true;
            }
            else
            {
                while (readTask.IsCompleted == false)
                {
                    yield return null;
                }

                foreach (TValue item in readTask.Result)
                {
                    AddInfo(item, isReload);
                }
            }

            if (initFailed == true)
            {
                throw new ResourceLoadFailedException(currentPath);
            }
        }

        public void PostInit()
        {
            foreach (TValue value in _infos.Values)
            {
                if (value is IVerifiable verifiable)
                {
                    if (verifiable.Verify(out string log) == false)
                    {
                        throw new Exception($"Verify failed, {this}, Key : {value.Key}, Log : {log}, Path : {_path}");
                    }
                }
                if (value is IPostProcessable postProcessable)
                {
                    postProcessable.PostProcess();
                }
                if (value is IHasPeriod hasPeriod)
                {
                    hasPeriod.PeriodInit();
                }
            }
        }

        protected virtual void PreAddInfo(TValue info)
        {
        }

        private void AddInfo(TValue info, bool isReload)
        {
            PreAddInfo(info);
            if (isReload == true)
            {
                if (_infos.ContainsKey(info.Key))
                {
                    ObjectExtensions.Replace(info, _infos[info.Key]);
                }
                else
                {
                    _infos.Add(info.Key, info);
                }
            }
            else
            {
                try
                {
                    _infos.Add(info.Key, info);
                }
                catch (System.ArgumentException e)
                {
                    Logger.Fatal($"[{info.GetType()}] Duplicated key : {info.Key}, {e}");
                    throw;
                }
            }
        }

        public void Save(string rootPath)
        {
            StaticInfoFormatType saveFormat = _formatType;
            if (_formatType == StaticInfoFormatType.MPackOrJson)
            {
                saveFormat = StaticInfoFormatType.Json;
                Write(rootPath + _path + FileExtResolver.Resolve(saveFormat)[0], saveFormat);
                saveFormat = StaticInfoFormatType.MPack;
                Write(rootPath + _path + FileExtResolver.Resolve(saveFormat)[0], saveFormat);
                return;
            }
            if(_formatType == StaticInfoFormatType.BinaryOrJson)
            {
                saveFormat = StaticInfoFormatType.Json;
                Write(rootPath + _path + FileExtResolver.Resolve(saveFormat)[0], saveFormat);
                saveFormat = StaticInfoFormatType.Binary;
                Write(rootPath + _path + FileExtResolver.Resolve(saveFormat)[0], saveFormat);
                return;
            }
            Write(rootPath + _path + FileExtResolver.Resolve(saveFormat)[0], saveFormat);
            OnSaveCallback?.Invoke();
        }
        public void WriteMPack(string filePath)
        {
            List<TValue> infoList = new List<TValue>();

            foreach (TValue info in _infos.Values.OrderBy(i => i.Key))
            {
                infoList.Add(info);
            }
            byte[] bytes = MessagePack.MessagePackSerializer.Serialize(infoList);
            File.WriteAllBytes(Directory.GetCurrentDirectory() + filePath, bytes);
            Logger.Info($"[{typeof(TValue).Name}] {filePath} saved.");
        }
        public void Write(string filePath, StaticInfoFormatType formatType)
        {
            List<TValue> infoList = new List<TValue>();

            foreach (TValue info in _infos.Values.OrderBy(i => i.Key))
            {
                infoList.Add(info);
            }

            Writer.IStaticInfoWriter<List<TValue>> writer = Writer.WriterFactory.Create<List<TValue>>(filePath, formatType);
            writer.Init(filePath);
            writer.Write(infoList);
            Logger.Info($"[{typeof(TValue).Name}] {filePath} saved.");
        }
    }
}