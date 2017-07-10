using System;
using System.Collections.Generic;
//using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microarea.Common.Applications;
using Microarea.Common.NameSolver;
using Microarea.Common.Generic;
using System.IO;
using System.Reflection;
using Microsoft.DotNet.PlatformAbstractions;
using TaskBuilderNetCore.Interfaces;

namespace widgets_service.Controllers
{
	[Route("widgets-service")]
	public class WidgetsController : Controller
	{
		private LoginInfoMessage loginInfo = null;
		private UserInfo userInfo = null;

		private bool CheckAuthentication(out string errMessage)
		{
			errMessage = string.Empty;

			string sAuthT = HttpContext.Request.Cookies[UserInfo.AuthenticationTokenKey];
			if (string.IsNullOrEmpty(sAuthT))
			{
				errMessage = "Missing authentication";
				return false;
			}

			if (loginInfo == null)
			{
				//string baseAddress = "http://" + HttpContext.Request.Host + HttpContext.Request.PathBase + "/";
				loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT /*, baseAddress*/);
			}

			if (loginInfo == null || string.IsNullOrEmpty(loginInfo.userName))
			{
				errMessage = "Invalid authentication token";
				return false;
			}

			if (userInfo == null)
			{
				userInfo = new UserInfo(loginInfo, sAuthT);
			}

			return true;
		}

		// GET api/values
		[HttpPost]
		public IEnumerable<string> Get()
		{
			string param1 = HttpContext.Request.Form["param"];
			string param2 = HttpContext.Request.Form["param2"];
			string param3 = HttpContext.Request.Form["param3"];
			return new string[] { "value1", "value2" };
		}

        // GET widgets-service/getActiveWidgets
        [Route("getWidget/{namespace}")]
        public IActionResult GetWidget(string nameSpace)
        {
            if (nameSpace.IsNullOrEmpty())
                return new ContentResult { StatusCode = 500, Content = "Empty file name", ContentType = "application/text" };

            string errMessage = "";
            String content = "";
            try
            {
                if (!CheckAuthentication(out errMessage))
                {
                    return new ContentResult { StatusCode = 504, Content = errMessage, ContentType = "application/text" };
                }
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "application/text" };
            }

            PathFinder pathFinder = new PathFinder(userInfo.Company, userInfo.ImpersonatedUser);
            NameSpace ns = new NameSpace(nameSpace, NameSpaceObjectType.File);
            string widgetFilename = pathFinder.GetFilename(ns, string.Empty) + ".widget.json";

            if (!System.IO.File.Exists(widgetFilename))
                return new ContentResult { StatusCode = 500, Content = "file non trovato", ContentType = "application/text" };

            try
            {
                using (StreamReader sr = System.IO.File.OpenText(widgetFilename))
                {
                    content = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 500, Content = e.Message, ContentType = "application/text" };
            }
            return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
        }

        // GET widgets-service/getActiveWidgets
        [Route("getActiveWidgets")]
		public IActionResult GetActiveWidgets()
		{

			string errMessage = "";
			String content = "";
			string widgetFileFullName = "";
            int statusCode = 404;
			try
			{
				if (!CheckAuthentication(out errMessage))
				{
					return new ContentResult { StatusCode = 504, Content = errMessage, ContentType = "application/text" };
				}
			}
			catch (Exception e)
			{
				return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "application/text" };
			}
			try
			{
				string code = Assembly.GetEntryAssembly().Location;
				DirectoryInfo di = new DirectoryInfo(code);
                PathFinder pathFinder = new PathFinder(userInfo.Company, userInfo.ImpersonatedUser);
                widgetFileFullName = Path.Combine(pathFinder.GetCustomUserApplicationDataPath(), "widgets.json");
                if (!System.IO.File.Exists(widgetFileFullName))
                {
                    // user configured widgets are missing, create a default one
                    string defaultWidgetFileFullName = di.FullName.ToLower().Replace("web-server.dll", "widgets.json");
                    System.IO.File.Copy(defaultWidgetFileFullName, widgetFileFullName);
                    statusCode = 203; // success with info
                }
                else
                    statusCode = 200; // all was fine
            }
			catch (Exception e)
			{

				return new ContentResult { StatusCode = 501, Content = e.Message, ContentType = "application/text" };
			}

			// no configured widgets, is not an error
			if (!System.IO.File.Exists(widgetFileFullName))
				return new ContentResult { StatusCode = 500, Content = "file non trovato", ContentType = "application/text" };


			try
			{
				using (StreamReader sr = System.IO.File.OpenText(widgetFileFullName))
				{
					content = sr.ReadToEnd();
				}
			}
			catch (Exception e)
			{

				return new ContentResult { StatusCode = 500, Content = e.Message, ContentType = "application/text" };
			}
			return new ContentResult { StatusCode = statusCode, Content = content, ContentType = "application/json" };
		}
	}
}
