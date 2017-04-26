using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Microarea.AdminServer.Interfaces;
using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Services.Interfaces;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Microarea.AdminServer.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _env;
        private IAdminDataServiceProvider _adminDataService;

        JsonHelper jsonHelper;

        public HomeController(IHostingEnvironment env, IAdminDataServiceProvider adminDataService)
        {
            _env = env;
            _adminDataService = adminDataService;
            this.jsonHelper = new JsonHelper();
        }

        [HttpGet]
        [Route("/")]
        public IActionResult Index()
        {
            jsonHelper.AddJsonCouple<string>("message", "Welcome to Microarea Admin-Server");
            return new ContentResult { Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
        }

        [HttpGet]
        [Route("api")]
        public IActionResult ApiHome()
        {
            jsonHelper.AddJsonCouple<string>("message", "Welcome to Microarea Admin-Server API");
            return new ContentResult { Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
        }

        [HttpPost("/api/login/{username}")]
        public IActionResult ApiLogin(string password, string userName)
        {
            string user = userName;
            string psw = password;

            if (String.IsNullOrEmpty(user))
            {
                jsonHelper.AddJsonCouple<bool>("result", false);
                jsonHelper.AddJsonCouple<string>("message", "Username cannot be empty");
                return new ContentResult { StatusCode = 400, Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
            }

            IUserAccount userAccount = _adminDataService.ReadLogin(user, psw);

            if (userAccount == null)
            {
                jsonHelper.AddJsonCouple<bool>("result", false);
                jsonHelper.AddJsonCouple<string>("message", "Invalid Username and Password");
                return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
            }

            // user has been found

            jsonHelper.AddJsonCouple<bool>("result", true);
            return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
        }
    }
}
