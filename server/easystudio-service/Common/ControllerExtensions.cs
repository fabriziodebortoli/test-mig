
using Microsoft.AspNetCore.Mvc;

namespace Microarea.EasyStudio.Common
{
    //=========================================================================
    public static class ControllerExtensions
    {
        //---------------------------------------------------------------------
        public static IActionResult ToContentResult(this Controller controller, int statusCode, MsgType type, string text)
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = ControllerDiagnostic.ToJson(type, text),
                ContentType = "application/json"
            };
        }
    }
}
