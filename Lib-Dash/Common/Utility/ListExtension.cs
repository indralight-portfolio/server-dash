using System;
using System.Collections.Generic;

namespace Common.Utility
{
    public static class ListExtension
    {
        public static T Random<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("List count is zero!");
            }

            return list[Utility.Random.Range(0, list.Count)];
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                --n;
                int k = Utility.Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void Swap<T>(this IList<T> list, int index1, int index2)
        {
            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public static void AddOrUpdate<T>(this List<T> list, T value, Predicate<T> match)
        {
            int findIndex = list.FindIndex(match);

            if (findIndex < 0)
            {
                list.Add(value);
            }
            else
            {
                list[findIndex] = value;
            }
        }
    }
}