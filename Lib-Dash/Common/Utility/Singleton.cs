namespace Common.Utility
{
    public class Singleton<T> where T : class, new()
    {
        public static T Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new T();
#if Common_Unity
                    UnityEngine.Debug.Log($"Singleton {typeof(T)} Created.");
#endif
                }
                return _instance;
            }
        } 
        private static T _instance;
        public static bool IsInstantiated => _instance != null;

        public void Release()
        {
            _instance = null;
        }
    }
}