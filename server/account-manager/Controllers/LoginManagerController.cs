using System.IO;
using System.Text;
using Microarea.Common.WebServicesWrapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Microarea.AccountManager.Controllers
{
	[Route("account-manager")]
	public class LoginManagerController : Controller
	{

		static Microarea.Common.WebServicesWrapper.LoginManager loginManagerInstance;
		static object staticLockTicket = new object();

		//----------------------------------------------------------------------------
		/// <summary>
		/// Oggetto statico globale BasePathFinder utilizzato ovunque in Mago.Net siano necessarie
		/// informazioni non dipendenti da username e company
		/// </summary>
		public static Microarea.Common.WebServicesWrapper.LoginManager LoginManagerInstance
		{
			get
			{
				lock (staticLockTicket)
				{
					if (loginManagerInstance == null)
					{
						loginManagerInstance = new Common.WebServicesWrapper.LoginManager();
					}
					return loginManagerInstance;
				}
			}
		}

		public LoginManagerController()
		{
		}

		[Route("login-compact")]
		public IActionResult LoginCompact()
		{
			string user = HttpContext.Request.Form["user"];
			string password = HttpContext.Request.Form["password"];
			string company = HttpContext.Request.Form["company"];
			string askingProcess = HttpContext.Request.Form["askingProcess"];
			bool overwriteLogin = HttpContext.Request.Form["overwriteLogin"] == "true";

			string authenticationToken;
			int result = LoginManagerInstance.LoginCompact(user, company, password, askingProcess, overwriteLogin, out authenticationToken);
			string errorMessage = "Error message"; // TODO read error message

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.Formatting = Formatting.Indented;

			jsonWriter.WriteStartObject();

			jsonWriter.WritePropertyName("result");
			jsonWriter.WriteValue(result.ToString());

			if (result > 0)
			{
				jsonWriter.WritePropertyName("errorCode");
				jsonWriter.WriteValue(result.ToString());
				jsonWriter.WritePropertyName("errorMessage");
				jsonWriter.WriteValue(errorMessage);
			}
			else
			{
				jsonWriter.WritePropertyName("authenticationToken");
				jsonWriter.WriteValue(authenticationToken);
			}

			jsonWriter.WriteEndObject();

			string content = sb.ToString();

			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}

		[Route("logout")]
		public IActionResult Logoff()
		{
			string token = HttpContext.Request.Form["token"];
			LoginManagerInstance.LogOff(token);
			var result = new { Success = true, Message = "" };
			return new JsonResult(result);
		}


		[Route("getLoginInformation")]
		public IActionResult GetLoginInformation()
		{
			string token = HttpContext.Request.Form["authtoken"];

			LoginManagerInstance.GetLoginInformation(token);
			LoginManagerSession loginManagerSession = LoginManagerSessionManager.GetLoginManagerSession(token);
			if (loginManagerSession == null)
				return new ContentResult { StatusCode = 400, Content = "error", ContentType = "text/html" };


			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);

			jsonWriter.WriteStartObject();

			jsonWriter.WritePropertyName("userName");
			jsonWriter.WriteValue(loginManagerSession.UserName);

			jsonWriter.WritePropertyName("companyName");
			jsonWriter.WriteValue(loginManagerSession.CompanyName);

			jsonWriter.WritePropertyName("admin");
			jsonWriter.WriteValue(loginManagerSession.Admin);

			jsonWriter.WritePropertyName("connectionString");
			jsonWriter.WriteValue(loginManagerSession.ConnectionString);

			jsonWriter.WritePropertyName("providerName");
			jsonWriter.WriteValue(loginManagerSession.ProviderName);

			jsonWriter.WritePropertyName("useUnicode");
			jsonWriter.WriteValue(loginManagerSession.UseUnicode);

			jsonWriter.WritePropertyName("preferredLanguage");
			jsonWriter.WriteValue(loginManagerSession.PreferredLanguage);

			jsonWriter.WritePropertyName("applicationLanguage");
			jsonWriter.WriteValue(loginManagerSession.ApplicationLanguage);

			jsonWriter.WriteEndObject();

			string s = sb.ToString();
			return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
		}


		[Route("getCompaniesForUser")]
		public IActionResult GetCompanyForUser()
		{
			//string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
			string user = HttpContext.Request.Form["user"];

			string[] companies = LoginManagerInstance.EnumCompanies(user);

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

		[Route("isActivated")]
		public IActionResult IsActivated()
		{
			//string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
			string application = HttpContext.Request.Form["application"];
			string functionality = HttpContext.Request.Form["functionality"];

			bool result = LoginManagerInstance.IsActivated(application, functionality);

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("result");
			jsonWriter.WriteValue(result);
			jsonWriter.WriteEndObject();

			string content = sb.ToString();
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}
	}
}
