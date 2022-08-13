using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace server_dash.Middleware
{
    public class LogExceptionMiddleware
    {
        private static readonly NLog.ILogger _logger = Common.Log.NLogUtility.GetCurrentClassLogger(); 
        private RequestDelegate _next;

        public LogExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                context.ResponseError(System.Net.HttpStatusCode.InternalServerError);
                _logger.Fatal(e);
            }
        }
    }
}