using Microarea.EasyStudio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microarea.EasyStudio.AspNetCore
{
    //=========================================================================
    public class RequestResultFilters : ActionFilterAttribute
    {
        //---------------------------------------------------------------------
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            BaseController controller = context.Controller as BaseController;
            if (controller != null && controller.Diagnostic != null)
                controller.Diagnostic.Clear();

            base.OnActionExecuting(context);
        }

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
				string content = controller.Diagnostic.AsJson;
				if (controller.Diagnostic.HasErrors)
					context.HttpContext.Response.StatusCode = 500;
				else if (controller.Diagnostic.HasWarnings)
					context.HttpContext.Response.StatusCode = 500;
				else
				{
					context.HttpContext.Response.StatusCode = 200;
					content = ((ContentResult)context?.Result)?.Content ?? content;
				}

                context.Result = new ContentResult
                {
                    StatusCode = context.HttpContext.Response.StatusCode,
                    Content = content,
                    ContentType = "application/json"
                };
            }

            base.OnActionExecuted(context);
        }
    }
}
