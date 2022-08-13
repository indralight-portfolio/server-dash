#if Common_Unity
using Common.Model;
using Common.Net.WWW;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

namespace Common.Unity.Net.WWW
{
    public class WWWConnector<T>
    {
        enum State
        {
            Connect,
            TimeOut,
        };
#if UNITY_EDITOR
        private const float TimeOut = 90.0f;           // 디버깅 할 경우에 대비해 1분 30로 변경
#else
        private const float TimeOut = 10.0f;
#endif

        private readonly Common.Net.WWW.HttpClient _httpClient = new Common.Net.WWW.HttpClient(TimeSpan.FromSeconds(TimeOut));
        private readonly string _hostUrl;
        private readonly Dictionary<string, string> _headers;
        private readonly HttpMessageDispatcher<T> _dispatcher;

        private readonly HashSet<string> _ignoreErrorApis = new HashSet<string>();
        private readonly HashSet<WWWAPI> _responseWaitingApis = new HashSet<WWWAPI>();

        private readonly Dictionary<string, string> _defaultHeader = new Dictionary<string, string>();

        public event EventHandler<EventArgs> OnSessionExpire;

        private SessionKey _sessionKey;

        public WWWConnector(string hostUrl, Dictionary<string, string> headers, HttpMessageDispatcher<T> dispatcher)
        {
            _dispatcher = dispatcher;

            _hostUrl = hostUrl;
            _headers = headers;
        }

        public override string ToString()
        {
            return $"[Connector:{_hostUrl}]";
        }

        public void SetSessionKey(SessionKey key)
        {
            _sessionKey = key;
        }

        public IEnumerator Request(WWWAPI api, object data,
            WebClient<T>.OnError onError, IHttpMessageHandler overrideHandler, params object[] pathVariables)
        {
            // check session key expired
            // 클라이언트의 DateTime.UtcNow 를 신뢰할 수 없다.
            // 일단 보내고 서버쪽 검증에 따르도록 한다.
            //if (_sessionKey != null && _sessionKey.Expiry < DateTime.UtcNow)
            //{
            //    Debug.Log($"{this} session key is expired. {_sessionKey.Key}:{_sessionKey.Expiry} Now:{DateTime.UtcNow}");
            //    OnSessionExpire?.Invoke(this, null);
            //    yield break;
            //}

            if (api.RequestPolicyType == RequestPolicyType.AfterResponse)
            {
                if (_responseWaitingApis.Contains(api) == true)
                {
                    Debug.LogWarning($"{this}[{api}] Response waiting, can't request again.");
                    yield break;
                }
            }

            _responseWaitingApis.Add(api);

            string path = WWWUtility.ReplacePathVariable(api.Path, pathVariables);

            var header = new Dictionary<string, string>(_defaultHeader);
            if (api.ResponseContentType == ResponseContentType.MessagePack)
            {
                header.Add(HeaderConstants.AcceptHeader, HeaderConstants.ContentType.MessagePack);
            }
            else
            {
                header.Add(HeaderConstants.AcceptHeader, HeaderConstants.ContentType.Json);
            }
            foreach (var kv in _headers)
            {
                header.Add(kv.Key, kv.Value);
            }
            if (string.IsNullOrEmpty(_sessionKey?.Key) == false)
            {
                header.Add(HeaderConstants.SessionKeyHeader, _sessionKey.Key);
            }

            var url = $"{_hostUrl}{path}";
            WWWEntity<T> wwwEntity = new WWWEntity<T>(url, api, data, header, onError, overrideHandler);

            yield return RequestByEntity(wwwEntity, api);
        }

        private IEnumerator RequestByEntity(WWWEntity<T> entity, WWWAPI api)
        {
#if UNITY_EDITOR
            Debug.Log($"{this}[{entity.Api}] Request Body : {entity.MakeRequestBody()}");
#endif

            var taskPost = _httpClient.PostAsync(entity.RequestMessage);
            yield return Common.Utility.TaskUtility.MakeEnumerator(taskPost);
            HttpResponseMessage httpResponse = taskPost.Result;

            if (httpResponse.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            {
                // time out
                if (entity.ExceedMaxTry() == false)
                {
                    Debug.LogError($"{this}[{entity.Api}]Timeout occured. retry!");
                    entity.AddTryCount();
                    yield return RequestByEntity(entity, api);
                }
                else
                {
                    if (api != null)
                        _responseWaitingApis.Remove(api);

                    if (_ignoreErrorApis.Contains(entity.Api) == false)
                    {
                        string errorMsg = httpResponse.ReasonPhrase ?? "Looks like the server is taking to long to respond, please try again in sometime.";
                        Debug.LogError($"{this}[{entity.Api}]{errorMsg}");
                        entity.OnError?.Invoke(errorMsg, entity.Api);
                    }
                }

                yield break;
            }

            if (api != null)
                _responseWaitingApis.Remove(api);

            if (httpResponse.IsSuccessStatusCode == false && _ignoreErrorApis.Contains(entity.Api) == false)
            {

                Debug.LogError($"{this}[{entity.Api}]Error occur : httpCode : {httpResponse.StatusCode}, {httpResponse.ReasonPhrase}");
                entity.OnError?.Invoke(httpResponse.ReasonPhrase, entity.Api);
                yield break;
            }

            var elapsedMs = (int)(DateTime.UtcNow - entity.RequestStart).TotalMilliseconds;
            var mediaType = httpResponse.Content.Headers.ContentType?.MediaType;
            if (mediaType == HeaderConstants.ContentType.MessagePack)
            {
                yield return HandleAsByteArray(entity, httpResponse.Content, elapsedMs);
            }
            else if (mediaType == HeaderConstants.ContentType.Json)
            {
                yield return HandleAsString(entity, httpResponse.Content, elapsedMs);
            }
            else
            {
                Debug.LogError($"{this}[{entity.Api}] Invalid ResponseBody : {httpResponse}");
                entity.OnError?.Invoke("responseBody is invalid", entity.Api);
            }
        }

        private IEnumerator HandleAsString(WWWEntity<T> entity, HttpContent httpContent, int elapsedMs)
        {
            var taskReadContent = httpContent.ReadAsStringAsync();
            yield return Common.Utility.TaskUtility.MakeEnumerator(taskReadContent);
            try
            {
                var result = taskReadContent.Result;
                if (result.Length == 0)
                {
                    Debug.LogError($"{this}[{entity.Api}][{elapsedMs}ms] No ResponseBody");
                    entity.OnError?.Invoke("responseBody is null", entity.Api);
                    yield break;
                }

                Debug.Log($"{this}[{entity.Api}][{elapsedMs}ms] {result}");
                if (entity.OverrideHandler == null)
                {
                    _dispatcher.Dispatch(entity.Api, result, elapsedMs);
                }
                else
                {
                    entity.OverrideHandler.Handle(result, elapsedMs);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{this}[{entity.Api}][{elapsedMs}ms] Exception occured,");
                Debug.LogException(e);
                entity.OnError?.Invoke(e.Message, entity.Api);
            }
        }

        private IEnumerator HandleAsByteArray(WWWEntity<T> entity, HttpContent httpContent, int elapsedMs)
        {

            var taskReadContent = httpContent.ReadAsByteArrayAsync();
            yield return Common.Utility.TaskUtility.MakeEnumerator(taskReadContent);
            try
            {
                var result = taskReadContent.Result;
                if (result.Length == 0)
                {
                    Debug.LogError($"{this}[{entity.Api}][{elapsedMs}ms] No ResponseBody [{entity.Api}]");
                    yield break;
                }
                if (entity.OverrideHandler == null)
                {
                    _dispatcher.Dispatch(entity.Api, result, elapsedMs);
                }
                else
                {
                    entity.OverrideHandler.Handle(result, elapsedMs);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{this}[{entity.Api}][{elapsedMs}ms] Exception occured,");
                Debug.LogException(e);
                entity.OnError?.Invoke(e.Message, entity.Api);
            }
        }
    }
}
#endif