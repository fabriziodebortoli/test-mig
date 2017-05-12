using System;
using System.Data.SqlClient;
using System.IO;
using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microarea.AdminServer.Model;
using Microsoft.Extensions.Options;

namespace Microarea.AdminServer.Controllers
{
	//=========================================================================
	public class AdminController : Controller
    {
        AppOptions _settings;
        private IHostingEnvironment _env;
        private IAdminDataServiceProvider _adminDataService;

        JsonHelper jsonHelper;

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IAdminDataServiceProvider adminDataService, IOptions<AppOptions> settings)
        {
            _env = env;
            _adminDataService = adminDataService;
            _settings = settings.Value;
            this.jsonHelper = new JsonHelper();
        }

        [HttpGet]
        [Route("/")]
		//-----------------------------------------------------------------------------	
		public IActionResult Index()
        {
            if (_env.WebRootPath == null)
            {
                return NotFound();
            }
            string file = Path.Combine(_env.WebRootPath, "index.html");
            byte[] buff = System.IO.File.ReadAllBytes(file);
            return File(buff, "text/html");
        }

        [HttpGet]
        [Route("api")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiHome()
        {
            jsonHelper.AddJsonCouple<string>("message", "Welcome to Microarea Admin-Server API");
            return new ContentResult { Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
        }

        [HttpGet("/api/accounts/{username}/{field}")]  
        [Produces("application/json")]
        //-----------------------------------------------------------------------------	
        public IActionResult ApiAccountsInformations(string username)
        {
            string user = username;

            if (String.IsNullOrEmpty(user))
            {
                jsonHelper.AddJsonCouple<bool>("result", false);
                jsonHelper.AddJsonCouple<string>("message", "Username cannot be empty");
                return new ContentResult { StatusCode = 400, Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
            }

			IAccount account;
			try
			{
				account = _adminDataService.ReadLogin(user, String.Empty);
			}
			catch (NotImplementedException ex)
			{
				jsonHelper.AddJsonCouple<bool>("result", false);
				jsonHelper.AddJsonCouple<string>("message", ex.Message);
				return new ContentResult { StatusCode = 501, Content = jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}
			catch (SqlException e)
			{
				jsonHelper.AddJsonCouple<bool>("result", false);
				jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 501, Content = jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

            if (account == null)
            {
                jsonHelper.AddJsonCouple<bool>("result", false);
                jsonHelper.AddJsonCouple<string>("message", "Invalid Username and Password");
                return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteAndClear(), ContentType = "text/html" };
            }

            // user has been found
            jsonHelper.AddJsonCouple<bool>("result", true);
            jsonHelper.AddJsonObject("account", account);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
        }

		[HttpPost("/api/accounts/{accountname}")] // post
		//-----------------------------------------------------------------------------	
		public IActionResult ApiAddAccount(string accountname, string password, string email)
		{
			string user = accountname;
			string psw = password;
            string _email = email;

			if (String.IsNullOrEmpty(user))
			{
				jsonHelper.AddJsonCouple<bool>("result", false);
				jsonHelper.AddJsonCouple<string>("message", "Account name cannot be empty");
				return new ContentResult { StatusCode = 400, Content = jsonHelper.WriteAndClear(), ContentType = "application/json" };
			}

			bool result = false;
			try
			{
                IAccount iAccount = new Account();
                iAccount.Email = _email;
                result = iAccount.Save(_settings.DatabaseInfo.ConnectionString);
            }
			catch (SqlException e)
			{
				jsonHelper.AddJsonCouple<bool>("result", false);
				jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 501, Content = jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
				jsonHelper.AddJsonCouple<bool>("result", false);
				jsonHelper.AddJsonCouple<string>("message", "Adding account operation failed");
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			jsonHelper.AddJsonCouple<bool>("result", true);
			jsonHelper.AddJsonCouple<string>("message", "Adding account operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteAndClear(), ContentType = "text/html" };
		}
	}
}
