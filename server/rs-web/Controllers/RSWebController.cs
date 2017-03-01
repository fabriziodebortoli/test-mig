using Microsoft.AspNetCore.Mvc;

namespace Microarea.RSWeb.Controllers
{
    [Route("rs")]
    public class RSWebController : Controller
    {
        [Route("report")]
        public IActionResult GetData()
        {
            return new ContentResult { Content = "Ecco il report ToDo", ContentType = "application/json" };
        }
   }
}
