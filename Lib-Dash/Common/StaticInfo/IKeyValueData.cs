using MessagePack;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Common.StaticInfo
{
    public interface IKeyValueData<T>
    {
        T Key
        {
            get;
        }
    }

    [MessagePackObject]
    [Serializable]
    public class IdKeyData : IKeyValueData<int>
    {
        [ReadOnlyOnEditor]
        [Key(0)]
        public int Id;

        [JsonIgnore]
        [IgnoreMember]
        public int Key => Id;
    }

    public class IdKeyDataNoSerialize : IKeyValueData<int>
    {
        [ReadOnlyOnEditor]
        [JsonIgnore]
        //[IgnoreMember]
        public int Id;

        [JsonIgnore]
        //[IgnoreMember]
        public int Key => Id;
    }

    public abstract class GenerateIdKeyDataNoSerialize : IKeyValueData<int>
    {
        public abstract int GenerateId();
        public abstract void UpdateIdGenerator(int id);
        protected abstract void OnDeserialized();
        [ReadOnlyOnEditor, IgnoreMember]
        public int Id;

        [JsonIgnore, IgnoreMember]
        public int Key => Id;
#if UNITY_EDITOR
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            UpdateIdGenerator(Id);
            OnDeserialized();
        }
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if(Id == 0)
            {
                Id = GenerateId();
            }
        }
#endif
    }

    [Serializable]
    public class IntKeyData : IKeyValueData<int>
    {
        [ReadOnlyOnEditor]
        [JsonIgnore]
        public int Key { get; set; }
    }

    [Serializable]
    public class IdIntKeyData : IKeyValueData<KeyType<int, int>>
    {
        public int Id;
        [JsonIgnore]
        public int SubKey;

        [JsonIgnore]
        public KeyType<int, int> Key => new KeyType<int, int>(Id, SubKey);
    }

    public struct KeyType<MainKey, SubKey> : IComparable
        where MainKey : IComparable
        where SubKey : IComparable
    {
        public MainKey Main;
        public SubKey Sub;

        public override string ToString() => $"[{nameof(MainKey)} : {Main}, {nameof(SubKey)} : {Sub}]";
        public KeyType(MainKey main, SubKey sub)
        {
            Main = main;
            Sub = sub;
        }

        public int CompareTo(object obj)
        {
            if (obj is KeyType<MainKey, SubKey> other)
            {
                var result = Main.CompareTo(other);
                if (result != 0) return result;
                result = Sub.CompareTo(other);
                return result;
            }

            return -1;
        }

    }

    public struct KeyType<MainKey, SubKey1, SubKey2> : IComparable
        where MainKey : IComparable
        where SubKey1 : IComparable
        where SubKey2 : IComparable
    {
        public MainKey Main;
        public SubKey1 Sub1;
        public SubKey2 Sub2;

        public override string ToString() => $"[{nameof(MainKey)} : {Main}, {nameof(SubKey1)} : {Sub1}, {nameof(SubKey2)} : {Sub2}]";
        public KeyType(MainKey main, SubKey1 sub1, SubKey2 sub2)
        {
            Main = main;
            Sub1 = sub1;
            Sub2 = sub2;
        }

        public int CompareTo(object obj)
        {
            if (obj is KeyType<MainKey, SubKey1, SubKey2> other)
            {
                var result = Main.CompareTo(other);
                if (result != 0) return result;
                result = Sub1.CompareTo(other);
                if (result != 0) return result;
                result = Sub2.CompareTo(other);
                return result;
            }

            return -1;
        }

    }
}