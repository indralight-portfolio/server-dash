#if Common_Unity
using Common.Model;
using UnityEngine;
using Common.Net.WWW;
using System.Collections.Generic;

namespace Common.Unity.Net.WWW
{
    public class WebClient<T>
    {
        public WWWConnector<T> NetworkConnector => _networkConnector;
        public HttpMessageDispatcher<T> Dispatcher => _dispatcher;

        private MonoBehaviour _coroutineDelegate;
        private WWWConnector<T> _networkConnector;
        private HttpMessageDispatcher<T> _dispatcher;

        public delegate void OnError(string errorMessage, string restfulApi);

        public OnError DefaultError = null;

        public void RegisterHandler(object target)
        {
            _dispatcher?.RegisterHandler(target);
        }

        public void UnregisterHandler(object target)
        {
            _dispatcher?.UnregisterHandler(target);
        }

        protected void Init(MonoBehaviour coroutineDelegate, string hostUrl, Dictionary<string, string> headers)
        {
            _coroutineDelegate = coroutineDelegate;
            _dispatcher = new HttpMessageDispatcher<T>();
            DefaultError = NullOnError;
            _dispatcher?.Init();
            _networkConnector = new WWWConnector<T>(hostUrl, headers, _dispatcher);

            Debug.Log($"Selected Host : {hostUrl}");
        }

        protected Coroutine Request(WWWAPI api, object data, OnError onError, IHttpMessageHandler overrideHandler, params object[] pathVariables)
        {
            return _coroutineDelegate.StartCoroutine(
                _networkConnector.Request(
                    api,
                    data,
                    onError ?? DefaultError,
                    overrideHandler,
                    pathVariables
                )
            );
        }

        public void SetSessionKey(SessionKey key)
        {
            _networkConnector.SetSessionKey(key);
        }

        private void NullOnError(string errorMessage, string restfulApi)
        {
        }
    }
}
#endif