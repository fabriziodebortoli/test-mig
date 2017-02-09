using Microsoft.AspNetCore.Mvc;
using System;

using Microarea.RSWeb.Models;

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
