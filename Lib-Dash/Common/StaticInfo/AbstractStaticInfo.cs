using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.StaticInfo
{
    public enum StaticInfoType
    {
        Undefined,
        Common,
        EditorOnly,
    }

    public enum StaticInfoFormatType
    {
        Undefined = 0,
        Xml,
        Json,
        MPack,
        MPackOrJson,
        Binary,
        BinaryOrJson,
        Anything,
    }
    public static class StaticInfoFormatTypeExtention
    {
        public static StaticInfoFormatType GetReadType(this StaticInfoFormatType formatType, bool isToolMode)
        {
            if (formatType == StaticInfoFormatType.MPackOrJson)
            {
                return isToolMode ? StaticInfoFormatType.Json : StaticInfoFormatType.MPack;
            }
            if(formatType == StaticInfoFormatType.BinaryOrJson)
            {
                return isToolMode ? StaticInfoFormatType.Json : StaticInfoFormatType.Binary;
            }
            return formatType;
        }
        //public static StaticInfoFormatType[] GetSaveType(this StaticInfoFormatType formatType, bool isToolMode)
        //{

        //}
    }

    /// <summary>
    /// Reflection을 사용하지 않기 위해 아래와 같은 구조가 되었다.
    /// </summary>
    public abstract class AbstractStaticInfo
    {
        public IReadOnlyDictionary<Type, IKeyValueInfo> KeyValueInfosByType => keyValueInfosByType;
        public IReadOnlyDictionary<Type, ISpecificInfo> SpecificInfosByType => specificInfosByType;

        private readonly List<IKeyValueInfo> _keyValueInfos = new List<IKeyValueInfo>();
        private readonly List<ISpecificInfo> _specificInfos = new List<ISpecificInfo>();

        protected readonly Dictionary<Type/*KeyValueData Type*/, IKeyValueInfo> keyValueInfosByType = new Dictionary<Type, IKeyValueInfo>();
        protected readonly Dictionary<Type/*KeyValueData Type*/, ISpecificInfo> specificInfosByType = new Dictionary<Type, ISpecificInfo>();

        public abstract IEnumerator Init(StaticInfoType dataType, string path, bool reload = false, bool isToolMode = false);
        public abstract Task InitAsync(StaticInfoType dataType, string path, bool isToolMode = false);

        protected void Register<K, V>(ref KeyValueInfo<K, V> info, string path,
            StaticInfoFormatType formatType, StaticInfoType staticInfoType,
            IEqualityComparer<K> comparer = null) where V : IKeyValueData<K>
        {
            info = new KeyValueInfo<K, V>(path, formatType, staticInfoType, comparer);
            _keyValueInfos.Add(info);
            keyValueInfosByType.Add(typeof(V), info);
        }

        protected void Register<T>(ref SpecificInfo<T> info, string path, StaticInfoFormatType formatType, StaticInfoType staticInfoType) where T : new()
        {
            info = new SpecificInfo<T>(path, formatType, staticInfoType);
            _specificInfos.Add(info);
            specificInfosByType.Add(typeof(T), info);
        }

        public void Save(string path)
        {
            foreach (IKeyValueInfo keyValueInfo in _keyValueInfos)
            {
                keyValueInfo.Save(path);
            }

            foreach (ISpecificInfo specificInfo in _specificInfos)
            {
                specificInfo.Save(path);
            }
        }

        protected IEnumerable<IKeyValueInfo> GetKeyValueInfos() => _keyValueInfos;
        protected IEnumerable<ISpecificInfo> GetSpecificInfos() => _specificInfos;
    }
}