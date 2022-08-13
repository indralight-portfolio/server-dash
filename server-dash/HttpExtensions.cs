using Common.Net.WWW;
using Dash.Model;
using Dash.Types;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;

namespace server_dash
{
    public static class HttpRequestExtensions
    {
        public static void ResponseError(this HttpContext context, HttpStatusCode statusCode)
        {
            context.Response.StatusCode = (int)statusCode;
        }

        public static async Task ResponseError(this HttpContext context, ErrorCode errorCode)
        {
            var model = new HttpResponseModel(errorCode);
            var request = context.Request;
            if (request.Headers.TryGetValue(HeaderConstants.AcceptHeader, out var accepts) == true && accepts.Contains(HeaderConstants.ContentType.MessagePack) == true)
            {
                var bytes = MessagePackSerializer.Serialize(model);
                context.Response.ContentType = HeaderConstants.ContentType.MessagePack;
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                return;
            }

            context.Response.ContentType = HeaderConstants.ContentType.Json;
            string response = JsonConvert.SerializeObject(model);
            await context.Response.WriteAsync(response);
        }
    }
}