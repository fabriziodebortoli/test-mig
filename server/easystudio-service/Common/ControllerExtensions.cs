
using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace Microarea.EasyStudio.Common
{
    //=========================================================================
    public static class ControllerExtensions
    {
        //---------------------------------------------------------------------
        public static IActionResult ToContentResult(this Controller controller, int statusCode, IDiagnosticProvider diagnostic)
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = diagnostic.AsJson,
                ContentType = "application/json"
            };
        }

        //---------------------------------------------------------------------
        public static IActionResult ToContentResult(this Controller controller, int statusCode, string text)
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
