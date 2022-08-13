#pragma warning disable CS0168 // 변수가 선언되었지만 사용되지 않았습니다.
using Common.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Common.Net.WWW
{
    public class HttpClient : IDisposable
    {
        private readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();
        private volatile bool disposed = false;

        public HttpClient(TimeSpan? timeout = null)
        {
            if (timeout == null) { timeout = TimeSpan.FromSeconds(5); }
            _httpClient.Timeout = (TimeSpan)timeout;
        }

        public void SetTimeout(TimeSpan timeout)
        {
            _httpClient.Timeout = timeout;
        }

        public async Task<string> PostStringAsync(string url, object data = null, Dictionary<string, string> headers = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            headers?.ForEach(x => requestMessage.Headers.Add(x.Key, x.Value));

            HttpContent content = null;
            if (data != null)
            {
                content = new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, HeaderConstants.ContentType.Json);
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(HeaderConstants.ContentType.Json);
            }
            requestMessage.Content = content;

            return await PostStringAsync(requestMessage);
        }

        public async Task<string> PostStringAsync(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage httpResponse = await PostAsync(requestMessage);
            if (httpResponse.IsSuccessStatusCode)
            {
                return await httpResponse.Content.ReadAsStringAsync();
            }
            return null;
        }

        public async Task<HttpResponseMessage> PostAsync(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _httpClient.SendAsync(requestMessage);
            }

            catch (Exception e)
            {
                httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout);
            }
            return httpResponse;
        }

        public async Task<string> GetStringAsync(string url, Dictionary<string, string> headers = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            headers?.ForEach(x => requestMessage.Headers.Add(x.Key, x.Value));

            return await GetStringAsync(requestMessage);
        }

        public async Task<string> GetStringAsync(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage httpResponse = await GetAsync(requestMessage);
            if (httpResponse.IsSuccessStatusCode)
            {
                return await httpResponse.Content.ReadAsStringAsync();
            }
            return null;
        }

        public async Task<HttpResponseMessage> GetAsync(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _httpClient.SendAsync(requestMessage);
            }
            catch (Exception e)
            {
                httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout);
            }
            return httpResponse;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
                _httpClient.Dispose();
            }
        }
    }
}
#pragma warning restore CS0168 // 변수가 선언되었지만 사용되지 않았습니다.
