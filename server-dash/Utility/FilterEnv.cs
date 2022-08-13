using Dash.Model.Service;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Net;

namespace server_dash.Utility
{
    public class FilterEnvAttribute : ActionFilterAttribute, IActionFilter
    {
        Env env = EnvUtility.Env;

        public Env[] Include;
        public Env[] Exclude;

        public FilterEnvAttribute() { }
        public FilterEnvAttribute(params Env[] Include) { this.Include = Include; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (IsValid())
            {
                base.OnActionExecuting(context);
                return;
            }

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.HttpContext.ResponseError(Dash.Types.ErrorCode.AccessDenied);
            return;
        }

        private bool IsValid()
        {
            bool valid = true;

            if (Include != null)
            {
                valid &= Include.Contains(env);
            }
            if (Exclude != null)
            {
                valid &= !(Exclude.Contains(env));
            }
            return valid;
        }
    }
}