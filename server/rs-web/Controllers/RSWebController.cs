using Microsoft.AspNetCore.Mvc;
using Microarea.Common.Applications;
using Microarea.RSWeb.WoormWebControl;
using System.Collections.Specialized;
using System.Text;

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

            StringBuilder sb = new StringBuilder(sc.Count);
            foreach (string entry in sc)
            {
                sb.Append(entry);
            }
            string xmlResult = sb.ToString();
 
            return new ContentResult { Content = xmlResult, ContentType = "application/xml" };
        }

        [Route("pdf/{namespace}")]
        public IActionResult GetPdf(string nameSpace)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbReportSession session = new TbReportSession(ui, nameSpace);

            PdfReportEngine report = new PdfReportEngine(session);

            string err = string.Empty;
            byte[] pdf = report.ExecuteReport(ref err);

            return new ContentResult { Content = pdf.ToString(), ContentType = "application/pdf" };
        }
    }
}
