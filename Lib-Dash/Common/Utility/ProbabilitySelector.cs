using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Utility
{
    public class ProbabilitySelector
    {
        public static T Select<T>(IReadOnlyList<T> list, IEnumerable<float> probabilities)
        {
            int count = 0;
            float totalProbability = 0;
            foreach (float probability in probabilities)
            {
                ++count;
                totalProbability += probability;
            }

            if (list.Count == 0 || list.Count != count)
            {
                throw new ArgumentException(list.Count.ToString());
            }

            float selected = Random.Range(0f, totalProbability);
            float current = 0f;
            count = 0;
            // 0.3, 0.5, 0.8
            // total : 1.6
            foreach (float probability in probabilities)
            {
                current += probability;
                if (selected < current)
                {
                    return list[count];
                }

                ++count;
            }

            return list[list.Count - 1];
        }

        public static int SelectIndex<T>(IReadOnlyList<T> list, IEnumerable<int> probabilities)
        {
            int count = 0;
            int totalProbability = 0;
            foreach (int probability in probabilities)
            {
                ++count;
                totalProbability += probability;
            }

            if (list.Count == 0 || list.Count != count)
            {
                throw new ArgumentException(list.Count.ToString());
            }

            int selected = Random.Range(0, totalProbability);
            int current = 0;
            count = 0;
            // 0.3, 0.5, 0.8
            // total : 1.6
            foreach (int probability in probabilities)
            {
                current += probability;
                if (selected < current)
                {
                    return count;
                }

                ++count;
            }

            return list.Count - 1;
        }
    }
}