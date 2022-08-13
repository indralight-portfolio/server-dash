#pragma warning disable CS0168 // 변수가 선언되었지만 사용되지 않았습니다.
using Common.Log;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Net.WWW
{
    public abstract class WebAPIClient
    {
        private const float TimeOut = 5.0f;
        private readonly Common.Net.WWW.HttpClient httpClient = new Common.Net.WWW.HttpClient(TimeSpan.FromSeconds(TimeOut));

        private string _hostUrl;
        private string _version;

        public delegate void OnError(string errorMessage, string restfulApi);

        private Dictionary<PathVariableType, object> _pathVariableValues = new Dictionary<PathVariableType, object>();

        public void Init(string hostUrl, string version)
        {
            _hostUrl = hostUrl;
            _version = version;
        }

        public void SetTimeout(TimeSpan timeout)
        {
            httpClient.SetTimeout(timeout);
        }

        public async Task<T> Request<T>(WWWAPI api, OnError onError = null)
        {
            return await Request<T>(api, null, null, null, onError);
        }
        public async Task<T> Request<T>(WWWAPI api, object data, OnError onError = null)
        {
            return await Request<T>(api, data, null, null, onError);
        }
        public async Task<T> Request<T>(WWWAPI api, object data, Dictionary<string, string> header, object[] pathVariables)
        {
            return await Request<T>(api, data, header, pathVariables, DefaultOnError);
        }
        public async Task<T> Request<T>(WWWAPI api, object data, Dictionary<string, string> header, object[] pathVariables, OnError onError)
        {
            string path = WWWUtility.ReplacePathVariable(api.Path, pathVariables);

            var _header = header == null ? new Dictionary<string, string>() : new Dictionary<string, string>(header);
            if (api.ResponseContentType == ResponseContentType.MessagePack)
            {
                _header.Add(HeaderConstants.AcceptHeader, HeaderConstants.ContentType.MessagePack);
            }
            else
            {
                _header.Add(HeaderConstants.AcceptHeader, HeaderConstants.ContentType.Json);
            }
            _header.Add(HeaderConstants.VersionHeader, _version);

            var url = $"{_hostUrl}{path}";
            WWWEntity<T> entity = new WWWEntity<T>(url, api, data, _header);

            try
            {
                var httpResponse = await httpClient.PostAsync(entity.RequestMessage);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var mediaType = httpResponse.Content.Headers.ContentType?.MediaType;
                    if (mediaType == HeaderConstants.ContentType.MessagePack)
                    {
                        var rawResponse = await httpResponse.Content.ReadAsByteArrayAsync();
                        T response = MessagePackSerializer.Deserialize<T>(rawResponse);
                        return response;
                    }
                    else if (mediaType == HeaderConstants.ContentType.Json)
                    {
                        var rawResponse = await httpResponse.Content.ReadAsStringAsync();
                        T response = JsonConvert.DeserializeObject<T>(rawResponse);
                        return response;
                    }
                    else
                    {
                        Logger.Error($"[WWW][{entity.Api}] Invalid ResponseBody : {httpResponse}");
                        onError?.Invoke("responseBody is invalid", entity.Api);
                    }
                }
                else
                {
                    Logger.Error($"[WWW][{entity.Api}] Error occur : httpCode : {httpResponse.StatusCode}, {httpResponse.ReasonPhrase}");
                    onError?.Invoke(httpResponse.ReasonPhrase, entity.Api);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"[WWW][{entity.Api}] Exception : {e.Message}");
                onError?.Invoke(e.Message, entity.Api);
            }
            return default;
        }

        protected void DefaultOnError(string errorMessage, string restfulApi)
        {
            Logger.Error("DefaultOnError");
        }
    }
}
#pragma warning restore CS0168 // 변수가 선언되었지만 사용되지 않았습니다.
