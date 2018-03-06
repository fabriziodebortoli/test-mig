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
using Newtonsoft.Json.Linq;
using Microarea.Common;

namespace widgets_service.Controllers
{
	[Route("widgets-service")]
	public class WidgetsController : Controller
	{
		private LoginInfoMessage loginInfo = null;
		private UserInfo userInfo = null;

		private bool CheckAuthentication(string sAuthT, out string errMessage)
		{
			errMessage = string.Empty;
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

        // GET widgets-service/getActiveWidgets
        [Route("getWidget/{namespace}")]
        public IActionResult GetWidget(string nameSpace)
        {
            if (nameSpace.IsNullOrEmpty())
                return new ContentResult { StatusCode = 500, Content = "Empty file name", ContentType = "text/plan" };

            string sAuthT =  AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);

            string errMessage = "";
            String content = "";
            try
            {
                if (!CheckAuthentication(sAuthT, out errMessage))
                {
                    return new ContentResult { StatusCode = 401, Content = errMessage, ContentType = "text/plan" };
                }
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plan" };
            }

            PathFinder pathFinder = new PathFinder(userInfo.Company, userInfo.ImpersonatedUser);
            NameSpace ns = new NameSpace(nameSpace, NameSpaceObjectType.File);
            string widgetFilename = pathFinder.GetFilename(ns, string.Empty) + ".widget.json";

            if (!PathFinder.PathFinderInstance.ExistFile(widgetFilename))
                return new ContentResult { StatusCode = 500, Content = "file non trovato", ContentType = "text/plan" };

            try
            {
                content =  PathFinder.PathFinderInstance.GetFileTextFromFileName(widgetFilename);
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 500, Content = e.Message, ContentType = "text/plan" };
            }
            return new ContentResult { StatusCode = 200, Content = content, ContentType = "text/plan" };
        }

        // GET widgets-service/getActiveWidgets
        [Route("getActiveWidgets")]
		public IActionResult GetActiveWidgets()
		{

			string errMessage = "";
			String content = "";
			string widgetFileFullName = "";
            int statusCode = 404;

            string sAuthT = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);

            try
			{
				if (!CheckAuthentication(sAuthT, out errMessage))
				{
					return new ContentResult { StatusCode = 401, Content = errMessage, ContentType = "application/text" };
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
                if (!PathFinder.PathFinderInstance.ExistFile(widgetFileFullName))
                {
                    // user configured widgets are missing, create a default one
                    string defaultWidgetFileFullName = di.FullName.ToLower().Replace("web-server.dll", "widgets.json");
                    PathFinder.PathFinderInstance.CopyFile(defaultWidgetFileFullName, widgetFileFullName, false);
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
			if (!PathFinder.PathFinderInstance.ExistFile(widgetFileFullName))
				return new ContentResult { StatusCode = 500, Content = "file non trovato", ContentType = "application/text" };


			try
			{
                content = PathFinder.PathFinderInstance.GetFileTextFromFileName(widgetFileFullName);
            }
            catch (Exception e)
			{

				return new ContentResult { StatusCode = 500, Content = e.Message, ContentType = "application/text" };
			}
			return new ContentResult { StatusCode = statusCode, Content = content, ContentType = "application/json" };
		}
	}
}
