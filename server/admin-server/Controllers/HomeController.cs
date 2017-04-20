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
using Microarea.AdminServer.Interfaces;

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
        [Route("/")]
        public IActionResult Index()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.WritePropertyName("message");
            jsonWriter.WriteValue("Welcome to Microarea Admin-Server");
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        [Route("api")]
        public IActionResult ApiHome()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.WritePropertyName("message");
            jsonWriter.WriteValue("Welcome to Microarea Admin-Server API");
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        [HttpPost("/api/login/{username}")]
        public IActionResult ApiLogin(string password, string userName)
        {
            string user = userName;
            string psw = password;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;

            if (String.IsNullOrEmpty(user))
            {
                jsonWriter.WritePropertyName("result");
                jsonWriter.WriteValue(false);
                jsonWriter.WritePropertyName("message");
                jsonWriter.WriteValue("Username can't be empty");
                return new ContentResult { StatusCode = 400, Content = sb.ToString(), ContentType = "application/json" };
            }

            IUserAccount userAccount = this.adminDataService.GetUserAccount(user, psw);

            if (userAccount == null)
            {
                jsonWriter.WritePropertyName("result");
                jsonWriter.WriteValue(false);
                jsonWriter.WritePropertyName("message");
                jsonWriter.WriteValue("Invalid Username and Password");
                return new ContentResult { StatusCode = 200, Content = sb.ToString(), ContentType = "application/json" };
            }

            // user has been found

            jsonWriter.WritePropertyName("result");
            jsonWriter.WriteValue(true);
            jsonWriter.WritePropertyName("message");
            jsonWriter.WriteValue("");
            return new ContentResult { StatusCode = 200, Content = sb.ToString(), ContentType = "application/json" };
        }
    }
}
