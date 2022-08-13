using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility
{
    public static class LinqExtentions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

        public static int CountTrue(this BitArray bitArray)
        {
            int count = 0;
            foreach (bool bit in bitArray)
            {
                if (bit)
                {
                    count++;
                }
            }
            return count;
        }

    }
}
