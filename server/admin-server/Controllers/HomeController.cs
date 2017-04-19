using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Microarea.AdminServer.Services.AdminDataService;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Microarea.AdminServer.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _env;

        AdminDataService adminDataService;

        public HomeController(IHostingEnvironment env, AdminDataService adminDataService)
        {
            _env = env;
            this.adminDataService = adminDataService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.WritePropertyName("message");
            jsonWriter.WriteValue("Welcome to Microarea Admin-Server");
            jsonWriter.WritePropertyName("data service");
            jsonWriter.WriteValue(this.adminDataService.DataName);
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }
    }
}
