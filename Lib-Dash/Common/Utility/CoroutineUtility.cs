using System.Collections;

namespace Common.Utility
{
    public static class CoroutineUtility
    {
        public static void Wait(IEnumerator coroutine)
        {
            while (coroutine.MoveNext())
            {
                
            }
        }
    }
}