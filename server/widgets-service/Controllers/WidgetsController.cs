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

            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
            {
                errMessage = "Missing authentication";
                return false;
            }

            if (loginInfo == null)
            {
                loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT);
            }

            if (string.IsNullOrEmpty(loginInfo.userName))
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
        [Route("getActiveWidgets")]
        public IActionResult getActiveWidgets()
        {
            string errMessage;
            if (!CheckAuthentication(out errMessage))
            {
                return new ContentResult { StatusCode = 504, Content = errMessage, ContentType = "application/text" };
            }

            PathFinder pathFinder = new PathFinder(userInfo.Company, userInfo.ImpersonatedUser);

            string widgetsFilePath = Path.Combine(pathFinder.GetCustomUserApplicationDataPath(), "widgets.json");

            // no configured widgets, is not an error
            if (!System.IO.File.Exists(widgetsFilePath))
                return new ContentResult { Content = "[]", ContentType = "application/json" };

            String content;
            using (StreamReader sr = System.IO.File.OpenText(widgetsFilePath))
            {
                content = sr.ReadToEnd();
            }

            return new ContentResult { Content = content, ContentType = "application/json" };
        }
    }
}
