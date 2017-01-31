using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Microarea.LoginManager.Controllers
{
	[Route("login-manager")]
	public class LoginManagerController : Controller
	{
		[Route("login-compact")]
		public IActionResult LoginCompact()
		{
			string user = HttpContext.Request.Form["user"];
			string password = HttpContext.Request.Form["password"];
			string company = HttpContext.Request.Form["company"];
			string askingProcess = HttpContext.Request.Form["askingProcess"];
			string overwriteLogin = HttpContext.Request.Form["overwriteLogin"];

			//string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
			string json = "{}";
			return new ContentResult { Content = json, ContentType = "application/json" };
		}

		[Route("logoff")]
		public IActionResult Logoff()
		{
			string user = HttpContext.Request.Form["token"];
			string json = "{}";
			return new ContentResult { Content = json, ContentType = "application/json" };
		}

		[Route("getCompaniesForUser")]
		public IActionResult getCompanyForUser()
		{
			

			string user = HttpContext.Request.Form["user"];
			string json = "{}";
			return new ContentResult { Content = json, ContentType = "application/json" };

		}


	}
}
