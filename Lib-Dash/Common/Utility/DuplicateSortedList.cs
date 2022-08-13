using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Common.Utility
{
    public interface IHasComapreKey<TKey> where TKey : IComparable
    {
        public TKey CompareKey { get; }
    }

    public class DuplicateSortedList<TKey, T> : List<T> where T : IHasComapreKey<TKey> where TKey : IComparable
    {
        public new void Add(T item)
        {
            base.Add(item);
            Sort_Key();
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
            Sort_Key();
        }

        public new void Clear()
        {
            base.Clear();
            Sort_Key();
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);
            Sort_Key();
        }
        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            base.InsertRange(index, collection);
            Sort_Key();
        }

        public new bool Remove(T item)
        {
            bool result = base.Remove(item);
            Sort_Key();
            return result;
        }
        public new int RemoveAll(Predicate<T> match)
        {
            int result = base.RemoveAll(match);
            Sort_Key();
            return result;
        }
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            Sort_Key();
        }
        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            Sort_Key();
        }

        void Sort_Key()
        {
            Sort((l, r) => l.CompareKey.CompareTo(r.CompareKey));
        }
    }
}