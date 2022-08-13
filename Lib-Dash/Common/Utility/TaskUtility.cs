using System.Collections;
using System.Threading.Tasks;

namespace Common.Utility
{
    public static class TaskUtility
    {
        public static IEnumerator MakeEnumerator(Task task)
        {
            while (task.IsCompleted == false)
            {
                yield return null;
            }
        }

        public static IEnumerator WaitAnyEnumerator(params Task[] tasks)
        {
            if (tasks == null || tasks.Length == 0)
            {
                yield break;
            }

            while (true)
            {
                foreach (Task task in tasks)
                {
                    if (task.IsCompleted == true)
                    {
                        yield break;
                    }
                }

                yield return null;
            }
        }

        public static async Task<(T1, T2)> WaitAll2<T1, T2>(Task<T1> task1, Task<T2> task2)
        {
            await Task.WhenAll(task1, task2);
            return (task1.Result, task2.Result);
        }

        public static async Task<(T1, T2, T3)> WaitAll3<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
        {
            await Task.WhenAll(task1, task2, task3);
            return (task1.Result, task2.Result, task3.Result);
        }

        public static async Task<(T1, T2, T3, T4)> WaitAll4<T1, T2, T3, T4>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
        {
            await Task.WhenAll(task1, task2, task3, task4);
            return (task1.Result, task2.Result, task3.Result, task4.Result);
        }

        public static async Task<(T1, T2, T3, T4, T5)> WaitAll5<T1, T2, T3, T4, T5>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
        {
            await Task.WhenAll(task1, task2, task3, task4, task5);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
        }

        public static async Task<(T1, T2, T3, T4, T5, T6)> WaitAll6<T1, T2, T3, T4, T5, T6>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
        {
            await Task.WhenAll(task1, task2, task3, task4, task5, task6);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
        }

        public static async Task<(T1, T2, T3, T4, T5, T6, T7)> WaitAll7<T1, T2, T3, T4, T5, T6, T7>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7)
        {
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result);
        }

        public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WaitAll8<T1, T2, T3, T4, T5, T6, T7, T8>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8)
        {
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result);
        }

        public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WaitAll9<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9)
        {
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result);
        }

        public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WaitAll10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10)
        {
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result, task10.Result);
        }

        public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WaitAll11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11)
        {
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result, task10.Result, task11.Result);
        }

        public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WaitAll12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12)
        {
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result, task10.Result, task11.Result, task12.Result);
        }
    }
}