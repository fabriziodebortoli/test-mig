
using Microarea.EasyStudio.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace Microarea.EasyStudio.Controllers
{
    //=========================================================================
    [AuthenticationFilters, RequestResultFilters]
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        //=========================================================================
        internal class BaseStrings
        {
            // messages
            internal static readonly string ObjectSuccessfullyCreated = "Successfully Created";
            internal static readonly string ObjectSuccessfullyDeleted = "Successfully Deleted";
            internal static readonly string TagDefaultValueSuccessfullyChanged = "Tag default value Successfully Changed";
            internal static readonly string EnumsGenerateSourceCodeSuccessfullyTerminated = "Enums Generate Source Code successfully Generated";
        }

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
            int statusCode = diagnostic.HasErrors ? 500 : 200;
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
