using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Services.Interfaces;

namespace Microarea.AdminServer.Controllers
{
    public class AdminController : Controller
    {
        private IHostingEnvironment _env;
        private IAdminDataServiceProvider _adminDataService;

        JsonHelper jsonHelper;

        public AdminController(IHostingEnvironment env, IAdminDataServiceProvider adminDataService)
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
        public IActionResult ApiLogin(string passworde, string username)
        {
            string user = username;
            string psw = passworde;

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
