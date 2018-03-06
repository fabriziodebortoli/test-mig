using Microarea.EasyStudio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microarea.EasyStudio.AspNetCore
{
    //=========================================================================
    public class RequestResultFilters : ActionFilterAttribute
    {
        //---------------------------------------------------------------------
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            BaseController controller = context.Controller as BaseController;
            if (controller == null)
            {
                base.OnActionExecuted(context);
                return;
            }

            if (controller.Diagnostic != null)
            {
                if (controller.Diagnostic.HasErrors)
                    context.HttpContext.Response.StatusCode = 500;
                else if (controller.Diagnostic.HasWarnings)
                    context.HttpContext.Response.StatusCode = 500;
                else
                    context.HttpContext.Response.StatusCode = 200;

                context.Result = new ContentResult
                {
                    StatusCode = context.HttpContext.Response.StatusCode,
                    Content = controller.Diagnostic.AsJson,
                    ContentType = "application/json"
                };
            }

            base.OnActionExecuted(context);
        }
    }
}
