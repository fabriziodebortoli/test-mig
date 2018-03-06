
using Microarea.Common;
using Microarea.Common.Applications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microarea.EasyStudio.AspNetCore
{
    //=========================================================================
    public class AuthenticationFilters : ActionFilterAttribute
    {
        const string MissingAuthentication = "Missing authentication token!";
        //---------------------------------------------------------------------
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string authToken = AutorizationHeaderManager.GetAuthorizationElement(context.HttpContext.Request, UserInfo.AuthenticationTokenKey);
            if (string.IsNullOrEmpty(authToken))
            {
                context.HttpContext.Response.StatusCode = 401;
                context.Result = new ForbidResult(/*MissingAuthentication*/);
            }

            base.OnActionExecuting(context);
        }
    }
}
