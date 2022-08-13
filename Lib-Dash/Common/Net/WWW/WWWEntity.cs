using Common.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Common.Net.WWW
{
    public class WWWEntity<T>
    {
        public string Url { get; }
        public WWWAPI Api { get; }
        protected object Data { get; }
        protected readonly Dictionary<string, string> RequestHeaders;

        public readonly HttpRequestMessage RequestMessage;
        public DateTime RequestStart { get; }

        public string MakeRequestBody()
        {
            string content;
            if (Data is FormData formData)
                content = formData.JsonString;
            else if (Data is MessagePackData messagePackData)
                content = messagePackData.JsonString;
            else
                content = JsonConvert.SerializeObject(Data);
            return content;
        }

        protected int _tryCount = 0;
        private static int MaxTryCount = 1;

        public WWWEntity(string url, WWWAPI api, object data = null, Dictionary<string, string> header = null)
        {
            Url = url;
            Api = api;
            Data = data;
            RequestHeaders = header;
            RequestStart = DateTime.UtcNow;

            RequestMessage = CreateRequestMessage();
        }

        protected HttpRequestMessage CreateRequestMessage()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Url);
            RequestHeaders?.ForEach(x => requestMessage.Headers.Add(x.Key, x.Value));

            HttpContent content = null;
            if (Data is FormData formData)
            {
                content = new FormUrlEncodedContent(formData.Data);
            }
            else if (Data is MessagePackData messagePackData)
            {
                content = new ByteArrayContent(messagePackData.Data);
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(HeaderConstants.ContentType.MessagePack);
            }
            else if (Data != null)
            {
                content = new StringContent(JsonConvert.SerializeObject(Data));
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(HeaderConstants.ContentType.Json);
            }
            requestMessage.Content = content;

            return requestMessage;
        }

        public virtual bool ExceedMaxTry() => MaxTryCount <= _tryCount;
        public void AddTryCount() => ++_tryCount;
    }
}