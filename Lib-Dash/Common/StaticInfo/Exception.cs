using System;
namespace Common.StaticInfo
{
    public class StaticInfoNotFoundException<T> : Exception
    {
        public StaticInfoNotFoundException(string key)
            : base($"Type : {typeof(T).ToString()} not found with key [{key}]")
        {
            Log.Logger.Fatal(Message);
        }
    }

    public class ResourceLoadFailedException : Exception
    {
        public ResourceLoadFailedException(string path)
            : base($"Resource load failed : {path}")
        {
        }
    }
}