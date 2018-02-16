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
using Newtonsoft.Json.Linq;
using Microarea.Common.Applications;
using Microarea.Common.Generic;

namespace Microarea.AccountManager.Controllers
{
    [Route("account-manager")]
    public class LoginManagerController : Controller
    {
        private void SetCulture(string authenticationToken)
        {
            LoginManagerSession loginManagerSession = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            if (loginManagerSession == null)
                return;
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
        public IActionResult LoginCompact([FromBody] JObject value)
        {
            try
            {
                string user = value["user"]?.Value<string>();
                string password = value["password"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string askingProcess = value["askingProcess"]?.Value<string>();
                string overwriteLoginString = value["overwrite"]?.Value<string>();
                bool.TryParse(overwriteLoginString, out bool overwriteLogin);
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
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //-----------------------------------------------------------------------------------------
        [Route("change-password")]
        public IActionResult ChangePassword([FromBody] JObject value)
        {
            try
            {
                string user = value["user"]?.Value<string>();
                string oldPassword = value["oldPassword"]?.Value<string>();
                string newPassword = value["newPassword"]?.Value<string>();
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
        public IActionResult Logoff([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.LogOff(authtoken);
                
                var result = new { Success = true, Culture = InstallationData.ServerConnectionInfo.PreferredLanguage, Message = "" };
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
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string json = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.GetJsonLoginInformation(authtoken);
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
                bool valid = false;
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (!string.IsNullOrEmpty(authtoken))
                {
                    valid = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.IsValidToken(authtoken);
                    if (valid)
                    {
                        SetCulture(authtoken);
                    }
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
        public IActionResult GetCompanyForUser([FromBody] JObject value)
        {
            try
            {
                //string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
                string user = value["user"]?.Value<string>();
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
        public IActionResult IsActivated([FromBody] JObject value)
        {
            try
            {
                //string json = "{\"Companies\": { \"Company\": [{ \"name\": \"Development\" },{\"name\": \"Development2\" }] }}";
                string application = value["application"]?.Value<string>();
                string functionality = value["functionality"]?.Value<string>();

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
