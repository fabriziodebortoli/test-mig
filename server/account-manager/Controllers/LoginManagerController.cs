using System.IO;
using System.Text;
using Microarea.Common.WebServicesWrapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microarea.Common.GenericForms;

namespace Microarea.AccountManager.Controllers
{
	[Route("account-manager")]
	public class LoginManagerController : Controller
	{
		//-----------------------------------------------------------------------------------------
		[Route("login-compact")]
		public IActionResult LoginCompact()
		{
			string user = HttpContext.Request.Form["user"];
			string password = HttpContext.Request.Form["password"];
			string company = HttpContext.Request.Form["company"];
			string askingProcess = HttpContext.Request.Form["askingProcess"];
			bool overwriteLogin = HttpContext.Request.Form["overwrite"] == "true";
			int result = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.LoginCompact(user, company, password, askingProcess, overwriteLogin, out string authenticationToken);

            string errorMessage = "";
            if (result != 0)
                errorMessage = LoginFacilities.DecodeLoginReturnsCodeError(result);

            return new JsonResult(new { Success = result == 0, Message = errorMessage, ErrorCode = result, Authtoken = authenticationToken });
		}

		//-----------------------------------------------------------------------------------------
		[Route("logoff")]
		public IActionResult Logoff()
		{
			string token = HttpContext.Request.Form["authtoken"];
			Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.LogOff(token);
			var result = new { Success = true, Message = "" };
			return new JsonResult(result);
		}


		//-----------------------------------------------------------------------------------------
		[Route("getLoginInformation")]
		public IActionResult GetLoginInformation()
		{
			string token = HttpContext.Request.Form["authtoken"];
			string json = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.GetJsonLoginInformation(token);
			return new ContentResult { Content = json, ContentType = "application/json" };
		}

		//-----------------------------------------------------------------------------------------
		[Route("isValidToken")]
		public IActionResult IsValidToken()
		{
			string token = HttpContext.Request.Form["authtoken"];
			bool valid =  Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.IsValidToken(token);
			var result = new { Success = valid, Message = "" };
			return new JsonResult(result);
		}

		//-----------------------------------------------------------------------------------------
		[Route("getCompaniesForUser")]
		public IActionResult GetCompanyForUser()
		{
			//string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
			string user = HttpContext.Request.Form["user"];

			string[] companies = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.EnumCompanies(user);

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

		//-----------------------------------------------------------------------------------------
		[Route("isActivated")]
		public IActionResult IsActivated()
		{
			//string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
			string application = HttpContext.Request.Form["application"];
			string functionality = HttpContext.Request.Form["functionality"];

			bool result = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.IsActivated(application, functionality);

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
