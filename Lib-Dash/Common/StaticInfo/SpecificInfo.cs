using Common.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Logger = Common.Log.Logger;

namespace Common.StaticInfo
{
    public interface ISpecificInfo
    {
        IEnumerator Init(string rootPath, bool isToolMode);
        void PostInit();
        void Save(string rootPath);
        Type GetInfoType();
        string GetPath();
        StaticInfoType GetStaticInfoType();
        StaticInfoFormatType GetFormatType();
    }

    public class SpecificInfo<T> : ISpecificInfo where T : new()
    {
        private static readonly Type InfoType = typeof(T);
        private T _data;
        private readonly string _path;
        private readonly StaticInfoFormatType _formatType;
        private readonly StaticInfoType _staticInfoType;

        public Type GetInfoType() => InfoType;
        public string GetPath() => _path;
        public StaticInfoType GetStaticInfoType() => _staticInfoType;
        public StaticInfoFormatType GetFormatType() => _formatType;

        public SpecificInfo(string path, StaticInfoFormatType formatType, StaticInfoType staticInfoType)
        {
            _path = path;
            _formatType = formatType;
            _staticInfoType = staticInfoType;
        }

        public IEnumerator Init(string rootPath, bool isToolMode = false)
        {
            //_formatType이 MPackOrJson일 경우 toolMode에선 json을 읽고 아닌경우 MPack으로 읽어야 함
            StaticInfoFormatType formatType = _formatType.GetReadType(isToolMode);

            List<string> paths = FileUtility.GetFilePathsFromPattern(rootPath, _path, FileExtResolver.Resolve(formatType));
            bool initFailed = false;

            string currentPath = string.Empty;
            Task<T> readTask = null;
            try
            {
                foreach (string path in paths)
                {
                    currentPath = path;
                    Logger.Debug($"Init: {typeof(T).Name} {nameof(path)} : {path}");
                    Reader.IStaticInfoReader<T> reader = Reader.ReaderFactory.Create<T>(path, formatType);
                    readTask = reader.ReadAsync(false);
                }
            }
            catch (FileNotFoundException ex)
            {
                initFailed = true;
                Logger.Fatal($"Resource not found : {currentPath}, Exception Message : {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"SpecificInfo {typeof(T).Name} Init failed, path : {currentPath}");
                initFailed = true;
                Logger.Fatal(ex);
            }

            if (readTask == null || readTask.IsFaulted == true)
            {
                initFailed = true;
            }
            else
            {
                while (readTask.IsCompleted == false)
                {
                    yield return null;
                }

                _data = readTask.Result;
                initFailed |= readTask.IsFaulted;
            }

            if (initFailed == true)
            {
                throw new ResourceLoadFailedException(currentPath);
            }
        }

        public void PostInit()
        {
            if (_data is IVerifiable verifiable)
            {
                if (verifiable.Verify(out string log) == false)
                {
                    throw new Exception($"Verify failed, {this}, Log : {log}, Path : {_path}");
                }
            }

            if (_data is IPostProcessable postProcessable)
            {
                postProcessable.PostProcess();
            }
        }

        public T Get()
        {
            return _data;
        }

        public void Save(string rootPath)
        {
            StaticInfoFormatType saveFormat = _formatType;
            if (_formatType == StaticInfoFormatType.MPackOrJson)
            {
                saveFormat = StaticInfoFormatType.Json;
                Write(rootPath + _path + FileExtResolver.Resolve(saveFormat)[0]);
                saveFormat = StaticInfoFormatType.MPack;
                Write(rootPath + _path + FileExtResolver.Resolve(saveFormat)[0]);
                return;
            }

            Write(rootPath + _path + FileExtResolver.Resolve(saveFormat)[0]);
        }
        public void Write(string filePath)
        {
            Writer.IStaticInfoWriter<T> writer = Writer.WriterFactory.Create<T>(filePath, _formatType);
            writer.Init(filePath);
            writer.Write(_data);
            Logger.Info($"[{typeof(T).Name}] {filePath} saved.");
        }
    }
}