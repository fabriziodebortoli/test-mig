using Microsoft.AspNetCore.Mvc;
using Microarea.Common.Applications;
using Microarea.RSWeb.WoormWebControl;
using System.Collections.Specialized;

namespace Microarea.RSWeb.Controllers
{
    [Route("rs")]
    public class RSWebController : Controller
    {
        [Route("xml/{namespace}")]
        public IActionResult GetXmlData(string nameSpace)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;
            
            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbReportSession session = new TbReportSession(ui, nameSpace);

            XmlReportEngine report = new XmlReportEngine(session);

            string parameters = report.XmlGetParameters();
            report = null;

            report = new XmlReportEngine(session);

            StringCollection sc = report.XmlExecuteReport(parameters);
            report = null;

            return new ContentResult { Content = sc.ToString(), ContentType = "application/xml" };
        }
   }
}
