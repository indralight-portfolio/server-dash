using Dash.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Threading.Tasks;

namespace server_dash.Middleware
{
    public class DisableHttpGetMiddleware
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private RequestDelegate _next;
        public DisableHttpGetMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var route = context.GetRouteData();
            if (IgnoreValidation(context, route) == true)
            {
                await _next(context);
                return;
            }
            if (route == null || route.Values.Count == 0)
            {
                _logger.Error($"Invalid route. {context.Request.Path}");
                context.ResponseError(System.Net.HttpStatusCode.Forbidden);
                //await context.ResponseError(ErrorCode.InvalidRequest);
                return;
            }
            await _next(context);
            return;
        }

        private bool IgnoreValidation(HttpContext context, RouteData route)
        {
            var path = context.Request.Path.Value;
            if (path.StartsWith("/swagger") == true || path.Contains("favicon.ico") == true)
            {
                return true;
            }
            string[] ignoreControllers = { "Admin", "Health" };
            if (route != null && route.Values.Count > 0 && ignoreControllers.Any(route.Values["controller"].ToString().Contains))
            {
                return true;
            }
            return false;
        }
    }
}