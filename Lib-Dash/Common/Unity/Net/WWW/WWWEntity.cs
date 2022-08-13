#if Common_Unity
using Common.Net.WWW;
using System.Collections.Generic;

namespace Common.Unity.Net.WWW
{
    public class WWWEntity<T> : Common.Net.WWW.WWWEntity<T>
    {
        public WebClient<T>.OnError OnError { get; }
        public IHttpMessageHandler OverrideHandler { get; }
#if UNITY_EDITOR
        private static int MaxTryCount = 1;
#else
        private static int MaxTryCount = 3;
#endif

        public WWWEntity(string url, WWWAPI api, object data, Dictionary<string, string> header, 
            WebClient<T>.OnError onError, IHttpMessageHandler overrideHandler) : base(url, api, data, header)
        {
            OnError = onError;
            OverrideHandler = overrideHandler;
        }

        public override bool ExceedMaxTry() => MaxTryCount <= _tryCount;
    }
}
#endif