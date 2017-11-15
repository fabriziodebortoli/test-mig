using System.IO;
using System.Text;
using Microarea.Common.WebServicesWrapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microarea.Common.GenericForms;
using Microsoft.AspNetCore.Http;
using System;
using Microarea.Common;
using System.Globalization;

namespace Microarea.AccountManager.Controllers
{
    [Route("account-manager")]
    public class LoginManagerController : Controller
    {
        private void SetCulture(string authenticationToken)
        {
            LoginManagerSession loginManagerSession = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            try
            {
                CultureInfo.CurrentUICulture = new CultureInfo(loginManagerSession.PreferredLanguage);
            }
            catch
            {
                //in caso di cookie errato... non dovrebbe mai passare di qui...
            }
        }
        //-----------------------------------------------------------------------------------------
        [Route("login-compact")]
        public IActionResult LoginCompact()
        {
            try
            {
                string user = HttpContext.Request.Form["user"];
                string password = HttpContext.Request.Form["password"];
                string company = HttpContext.Request.Form["company"];
                string askingProcess = HttpContext.Request.Form["askingProcess"];
                bool overwriteLogin = HttpContext.Request.Form["overwrite"] == "true";
                int result = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.LoginCompact(user, company, password, askingProcess, overwriteLogin, out string authenticationToken);

                string errorMessage = "";
                if (result != 0)
                {
                    errorMessage = LoginFacilities.DecodeLoginReturnsCodeError(result);
                }
                else
                {
                    SetCulture(authenticationToken);
                }

                return new JsonResult(new { Success = result == 0, Message = errorMessage, ErrorCode = result, Authtoken = authenticationToken, Culture = CultureInfo.CurrentUICulture.Name });
            }
            catch(Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //-----------------------------------------------------------------------------------------
        [Route("change-password")]
        public IActionResult ChangePassword()
        {
            try
            {
                string user = HttpContext.Request.Form["user"];
                string oldPassword = HttpContext.Request.Form["oldPassword"];
                string newPassword = HttpContext.Request.Form["newPassword"];
                int result = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.ChangePassword(user, oldPassword, newPassword);
                string errorMessage = "";
                if (result != 0)
                {
                    errorMessage = LoginFacilities.DecodeLoginReturnsCodeError(result);
                }
                return new JsonResult(new { Success = result == 0, Message = errorMessage, ErrorCode = result });
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //-----------------------------------------------------------------------------------------
        [Route("logoff")]
        public IActionResult Logoff()
        {
            try
            {
                string token = HttpContext.Request.Form["authtoken"];
                Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.LogOff(token);
                var result = new { Success = true, Message = "" };
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }


        //-----------------------------------------------------------------------------------------
        [Route("getLoginInformation")]
        public IActionResult GetLoginInformation()
        {
            try
            {
                string token = HttpContext.Request.Form["authtoken"];
                string json = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.GetJsonLoginInformation(token);
                return new ContentResult { Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //-----------------------------------------------------------------------------------------
        [Route("isValidToken")]
        public IActionResult IsValidToken()
        {
            try
            {
                string token = HttpContext.Request.Form["authtoken"];
                bool valid = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.IsValidToken(token);
                if (valid)
                {
                    SetCulture(token);
                }
                var result = new { Success = valid, Culture = CultureInfo.CurrentUICulture.Name, Message = "" };
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //-----------------------------------------------------------------------------------------
        [Route("getCompaniesForUser")]
        public IActionResult GetCompanyForUser()
        {
            try
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
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //-----------------------------------------------------------------------------------------
        [Route("isServerUp")]
        public IActionResult IsServerUp()
        {
            return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
        }

        //-----------------------------------------------------------------------------------------
        [Route("isActivated")]
        public IActionResult IsActivated()
        {
            try
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
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }
    }
}
