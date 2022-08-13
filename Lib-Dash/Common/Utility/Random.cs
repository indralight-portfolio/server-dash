using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Unity.Mathematics;

namespace Common.Utility
{
    public static class Random
    {
        private static System.Random _raw;

        static Random()
        {
            _raw = new System.Random(RandomProvider.GetSeed());
        }

        public static double NextDouble()
        {
            return _raw.NextDouble();
        }

        public static int Range(int start, int end)
        {
            return _raw.Next(start, end);
        }

        public static float Range(float start, float end)
        {
            return (float)NextDouble() * (end - start) + start;
        }
        //NextFloat 사용시 리턴값이 마이너스인 경우가 있어서 사용 하지 않음(추후 수정해서 사용할수 있을거 같아 주석처리)
        //public static float NextFloat()
        //{
        //    double mantissa = (_raw.NextDouble() * 2.0) - 1.0;
        //    // choose -149 instead of -126 to also generate subnormal floats (*)
        //    double exponent = Math.Pow(2.0, _raw.Next(-126, 128));
        //    return (float)(mantissa * exponent);
        //}

        public static long Range(long start, long end)
        {
            ulong uRange = (ulong)(end - start);
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                _raw.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + start;
        }

        private const string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string _alpha_numeric = "0123456789" + _alphabet;
        private const string _alpha_numeric_special = _alpha_numeric + "!,#$%&'*+-./^_`|-";
        public static string NextString(int size, bool lowerCase = false, bool specialChar = true)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            string sorce = specialChar ? _alpha_numeric_special : _alpha_numeric;
            for (int i = 0; i < size; i++)
            {
                int num = Convert.ToInt32(math.floor(sorce.Length * NextDouble()));
                builder.Append(sorce.Substring(num, 1));
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }

    public static class ThreadLocalRandom
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<System.Random> _random =
            new ThreadLocal<System.Random>(() => new System.Random(Interlocked.Increment(ref _seed)));

        public static int Rand()
        {
            return _random.Value.Next();
        }

        public static int Range(int start, int end)
        {
            return _random.Value.Next(start, end);
        }

        public static double Range(double start, double end)
        {
            return _random.Value.NextDouble() * (end - start) + start;
        }

        public static T Choose<T>(Dictionary<T, double> table)
        {
            double totalProbability = 0;
            foreach (var pair in table)
            {
                totalProbability += pair.Value;
            }

            double rand = Range(0, totalProbability);
            double currentProbability = 0;
            foreach (var entry in table)
            {
                currentProbability += entry.Value;
                if (rand < currentProbability)
                {
                    return entry.Key;
                }
            }
            throw new IndexOutOfRangeException($"[Choose] no one chosen : {table.Count}");
        }

        public static T Choose<T>(List<T> table)
        {
            double totalProbability = table.Count;
            double rand = Range(0, totalProbability);
            int index = (int)rand;
            if (index < table.Count)
            {
                return table[index];
            }
            throw new IndexOutOfRangeException($"[Choose] no one chosen : {table.Count}");
        }
    }

    public class FixedSeedRandom
    {
        private System.Random _random;

        public FixedSeedRandom(int seed)
        {
            _random = new System.Random(seed);
        }
        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public double Range(double start, double end)
        {
            return _random.NextDouble() * (end - start) + start;
        }

        public T Choose<T>(Dictionary<T, double> table)
        {
            double totalProbability = 0;
            foreach (var value in table)
            {
                totalProbability += value.Value;
            }

            double rand = Range(0, totalProbability);
            double currentProbability = 0;
            foreach (var entry in table)
            {
                currentProbability += entry.Value;
                if (rand < currentProbability)
                {
                    return entry.Key;
                }
            }
            throw new IndexOutOfRangeException($"[Choose] no one chosen : {table.Count}");
        }
    }
}