using Microsoft.AspNetCore.Mvc;
using System;

using Microarea.RSWeb.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microarea.Common.Applications;

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
