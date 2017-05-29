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
using Microarea.Common.WebServicesWrapper;
using LoginManagerWcf;

namespace Microarea.AccountManager.Controllers
{
    [Route("account-manager")]
    public class LoginManagerController : Controller
    {
       
        public LoginManagerController()
        {
        }

        [Route("login-compact")]
        public IActionResult LoginCompact()
        {
			Microarea.Common.WebServicesWrapper.LoginManager loginManager = new Common.WebServicesWrapper.LoginManager();

			string user = HttpContext.Request.Form["user"];
            string password = HttpContext.Request.Form["password"];
            string company = HttpContext.Request.Form["company"];
            string askingProcess = HttpContext.Request.Form["askingProcess"];
            bool overwriteLogin = HttpContext.Request.Form["overwriteLogin"] == "true";

			string authenticationToken;
			int result = loginManager.LoginCompact(user, company, password, askingProcess, overwriteLogin, out authenticationToken);
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
			Microarea.Common.WebServicesWrapper.LoginManager loginManager = new Common.WebServicesWrapper.LoginManager();

			string token = HttpContext.Request.Form["token"];
            loginManager.LogOff(token);
            var result = new { Success = true, Message = "" };
            return new JsonResult(result);
        }


        [Route("getLoginInformation")]
        public IActionResult getLoginInformation()
        {
            string token = HttpContext.Request.Form["authtoken"];

			Microarea.Common.WebServicesWrapper.LoginManager loginManager = new Common.WebServicesWrapper.LoginManager();

			loginManager.GetLoginInformation(
				token, 
				out string userName,
				out string companyName,
				out bool admin, 
				out string connectionString,
				out string providerName,
				out bool useUnicode, 
				out string preferredLanguage,
				out string applicationLanguage
				);

			StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);

            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("userName");
            jsonWriter.WriteValue(userName);

            jsonWriter.WritePropertyName("companyName");
            jsonWriter.WriteValue(companyName);

            jsonWriter.WritePropertyName("admin");
            jsonWriter.WriteValue(admin);

            jsonWriter.WritePropertyName("connectionString");
            jsonWriter.WriteValue(connectionString);

            jsonWriter.WritePropertyName("providerName");
            jsonWriter.WriteValue(providerName);

            jsonWriter.WritePropertyName("useUnicode");
            jsonWriter.WriteValue(useUnicode);

            jsonWriter.WritePropertyName("preferredLanguage");
            jsonWriter.WriteValue(preferredLanguage);

            jsonWriter.WritePropertyName("applicationLanguage");
            jsonWriter.WriteValue(applicationLanguage);

            jsonWriter.WriteEndObject();

            string s = sb.ToString();
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }


        [Route("getCompaniesForUser")]
        public IActionResult getCompanyForUser()
        {
            //string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
            string user = HttpContext.Request.Form["user"];

			Microarea.Common.WebServicesWrapper.LoginManager loginManager = new Common.WebServicesWrapper.LoginManager();
			string[] companies = loginManager.EnumCompanies(user);

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
		public IActionResult isActivated()
		{
			//string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
			string application = HttpContext.Request.Form["application"];
			string functionality = HttpContext.Request.Form["functionality"];

			Microarea.Common.WebServicesWrapper.LoginManager loginManager = new Common.WebServicesWrapper.LoginManager();
			bool result = loginManager.IsActivated(application, functionality);

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("result");
			jsonWriter.WriteValue(result);
			jsonWriter.WriteEndObject();

			string content =  sb.ToString();
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}
	}
}
