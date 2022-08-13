using Common.Net.WWW;
using Common.NetCore.Web;
using Dash.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using Dash;

namespace server_dash.Middleware
{
    public class ValidateMiddleware
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private RequestDelegate _next;
        private ServiceStateValidator _serviceStateValidator;
        private SessionValidator _sessionValidator;
        private CheckConfig _checkConfig;

        public ValidateMiddleware(RequestDelegate next, ServiceStateValidator serviceStateValidator, SessionValidator sessionValidator, CheckConfig checkConfig)
        {
            _next = next;
            _serviceStateValidator = serviceStateValidator;
            _sessionValidator = sessionValidator;
            _checkConfig = checkConfig;
        }

        public async Task Invoke(HttpContext context)
        {
            var route = context.GetRouteData();
            // InvalidRoute 체크는 DisableHttpGetMiddleware 에서 먼저 한다.
            if (route == null || route.Values.Count == 0)
            {
                await _next(context);
                return;
            }
            if (await ValidateServiceState(context, route) == false)
            {
                return;
            }
            if (await ValidateAdmin(context, route) == false)
            {
                return;
            }
            if (await ValidateVersion(context, route) == false)
            {
                return;
            }
            if (await ValidateSession(context, route) == false)
            {
                return;
            }
            await _next(context);
            return;
        }

        private async Task<bool> ValidateAdmin(HttpContext context, RouteData route)
        {
            if (route.Values["controller"].ToString() == "Admin")
            {
                // 관리자 요청이므로 체크해야 한다.
                string clientIP = HttpUtility.GetClientIP(context);

                bool check = Common.Utility.NetUtility.IsLocalIP(clientIP)
                    || Common.Utility.NetUtility.IsPrivateIP(clientIP)
                    || _checkConfig.AdminServerList.Contains("all")
                    || _checkConfig.AdminServerList.Contains(clientIP);

                if (check == false)
                {
                    context.ResponseError(System.Net.HttpStatusCode.Forbidden);
                    //await context.ResponseError(ErrorCode.AccessDenied);
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ValidateServiceState(HttpContext context, RouteData route)
        {
            if (IgnoreValidation(route))
            {
                return true;
            }

            string clientIP = HttpUtility.GetClientIP(context);
            bool valid = _serviceStateValidator.Validate(clientIP);
            if (valid == false)
            {
                await context.ResponseError(ErrorCode.OutOfService);
                return false;
            }

            return true;
        }

        const string ROUTE_KEY = "oidAccount";
        private async Task<bool> ValidateSession(HttpContext context, RouteData route)
        {
            if (_checkConfig.IgnoreSession == true)
            {
                return true;
            }
            if (IgnoreValidation(route))
            {
                return true;
            }
            if (route.Values.ContainsKey(ROUTE_KEY) == false)
            {
                return true;
            }
            ulong.TryParse(route.Values[ROUTE_KEY].ToString(), out ulong oidAccount);

            if (context.Request.Headers.TryGetValue(HeaderConstants.SessionKeyHeader, out var sessionKey) == false)
            {
                _logger.Error($"{oidAccount.LogOid()}, Invalid RequesHeader, sessionKey is null.");
                await context.ResponseError(ErrorCode.InvalidHeader);
                return false;
            }

            bool valid = await _sessionValidator.Validate(oidAccount, sessionKey);
            if (valid == false)
            {
                _logger.Info($"{oidAccount.LogOid()}, Session validation failed, from client : {sessionKey}");
                await context.ResponseError(ErrorCode.InvalidSession);
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateVersion(HttpContext context, RouteData route)
        {
            if (_checkConfig.IgnoreVersion == true)
            {
                return true;
            }
            if (IgnoreValidation(route))
            {
                return true;
            }

            if (context.Request.Headers.TryGetValue(HeaderConstants.VersionHeader, out var version) == false)
            {
                _logger.Error("Invalid RequesHeader, version is null.");
                await context.ResponseError(ErrorCode.InvalidHeader);
                return false;
            }

            bool valid = VersionValidator.Validate(version);
            if (valid == false)
            {
                _logger.Info($"Version validation failed, from client : {version}");
                await context.ResponseError(ErrorCode.InvalidVersion);
                return false;
            }

            return true;
        }

        private bool IgnoreValidation(RouteData route)
        {
            if (route.Values["controller"].ToString() == "Admin" || route.Values["controller"].ToString() == "Health")
            {
                return true;
            }
            return false;
        }
    }
}