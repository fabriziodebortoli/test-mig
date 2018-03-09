
using Microarea.EasyStudio.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace Microarea.EasyStudio.Controllers
{
    //=========================================================================
    [/* Controllo di authtoken AuthenticationFilters,*/RequestResultFilters]
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected IServiceManager Services { get; set; }
        public virtual IDiagnosticProvider Diagnostic { get; }
 
        //---------------------------------------------------------------------
        protected BaseController(IServiceManager serviceManager)
        {
            Services = serviceManager;
        }

        //---------------------------------------------------------------------
        public IActionResult ToResult(IDiagnosticProvider diagnostic)
        {
            int statusCode = 200;
            if (diagnostic.HasErrors)
                statusCode = 500;

            string result = diagnostic.IsEmpty ? string.Empty : diagnostic.AsJson;
            return ToResult(result, statusCode);
        }

        //---------------------------------------------------------------------
        public IActionResult ToResult(string text, int statusCode = 200)
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = text,
                ContentType = "application/json"
            };
        }
    }
}
