using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Microarea.LoginManager.Controllers
{
	[Route("login-manager")]
	public class LoginManagerController : Controller
	{
		LoginManagerWcf.MicroareaLoginManagerSoapClient loginManagerClient = new LoginManagerWcf.MicroareaLoginManagerSoapClient(LoginManagerWcf.MicroareaLoginManagerSoapClient.EndpointConfiguration.MicroareaLoginManagerSoap);

		public LoginManagerController()
		{
			ConfigureWebService();
		}

		private void ConfigureWebService()
		{
			string path = Assembly.GetEntryAssembly().Location;
			int index = path.IndexOf("\\Standard\\Web\\", StringComparison.CurrentCultureIgnoreCase);
			if (index < 0)
			{
				Debug.Assert(false, "Invalid Path");
				return;
			}

			path = path.Substring(0, index);

			int startIndex = path.LastIndexOf('\\');
			string installation = path.Substring(startIndex + 1, path.Length - startIndex - 1);


			string uri = string.Format("http://localhost/{0}/LoginManager/LoginManager.asmx", installation);
			loginManagerClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(uri);
		}

		[Route("login-compact")]
		public IActionResult LoginCompact()
		{
			string user = HttpContext.Request.Form["user"];
			string password = HttpContext.Request.Form["password"];
			string company = HttpContext.Request.Form["company"];
			string askingProcess = HttpContext.Request.Form["askingProcess"];
			bool overwriteLogin = HttpContext.Request.Form["overwriteLogin"] == "true";


			LoginManagerWcf.LoginCompactRequest request = new LoginManagerWcf.LoginCompactRequest(user, company, password, askingProcess, overwriteLogin);
			Task<LoginManagerWcf.LoginCompactResponse> task = loginManagerClient.LoginCompactAsync(request);
			int result = task.Result.LoginCompactResult;

			//awaiter.OnCompleted(() => { result = task.Result.LoginCompactResult; });

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.Formatting = Formatting.Indented;
			jsonWriter.WritePropertyName("result");
			jsonWriter.WriteValue(result.ToString());
			

			return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
		}

		[Route("logoff")]
		public IActionResult Logoff()
		{
			string token = HttpContext.Request.Form["token"];

			Task task = loginManagerClient.LogOffAsync(token);
			task.Wait();
			var result = new { Success = "True", Message = "" };
			return new JsonResult(result);
		}

		[Route("getCompaniesForUser")]
		public IActionResult getCompanyForUser()
		{
			//string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
			string user = HttpContext.Request.Form["user"];

			Task<string[]> task = loginManagerClient.EnumCompaniesAsync(user);
			string[] companies = task.Result;

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.Formatting = Formatting.Indented;

			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("Companies");

			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("Company");

			jsonWriter.WriteStartArray();

			foreach (string item in companies)
			{
				jsonWriter.WriteStartObject();
				jsonWriter.WritePropertyName("name");
				jsonWriter.WriteValue(item);
				jsonWriter.WriteEndObject();
			}
			jsonWriter.WriteEndArray();

			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();

			string s = sb.ToString();
			return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };

		}


	}
}
