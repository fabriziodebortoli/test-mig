using System.Collections.Specialized;
using System.Text;
using Microsoft.AspNetCore.Mvc;

using Microarea.Common.Applications;
using Microarea.RSWeb.Render;
using Microarea.RSWeb.Models;

/*
localhost:5000/rs/template/erp.company.isocountrycodes/1

localhost:5000/rs/data/erp.company.isocountrycodes/1

localhost:5000/rs/xml/erp.company.isocountrycodes

localhost:5000/rs/pdf/erp.company.isocountrycodes
*/

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

            //trucco per parametri
            XmlReportEngine reportParameters = new XmlReportEngine(session);
            string parameters = reportParameters.XmlGetParameters();
            session.ReportParameters = parameters;
            reportParameters = null;
            //--------------------

            XmlReportEngine report = new XmlReportEngine(session);

            StringCollection sc = report.XmlExecuteReport();
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

            //trucco per parametri
            XmlReportEngine reportParameters = new XmlReportEngine(session);
            string parameters = reportParameters.XmlGetParameters();
            session.ReportParameters = parameters;
            reportParameters = null;
            //--------------------

            PdfReportEngine report = new PdfReportEngine(session);

            string err = string.Empty;
            byte[] pdf = report.ExecuteReport(ref err);

            return new ContentResult { Content = pdf.ToString(), ContentType = "application/pdf" };
        }

        [Route("template/{namespace}/{page}")] // /{page}
        public IActionResult GetJsonPageTemplate(string nameSpace, int page)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbReportSession session = new TbReportSession(ui, nameSpace);

            JsonReportEngine report = new JsonReportEngine(session);
            report.Execute();

            string pageLayout = report.GetJsonTemplatePage(page);

            return new ContentResult { Content = pageLayout, ContentType = "application/json" };
        }

        [Route("data/{namespace}/{page}")] // /{page}
        public IActionResult GetJsonPageData(string nameSpace, int page)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbReportSession session = new TbReportSession(ui, nameSpace);

            JsonReportEngine report = new JsonReportEngine(session);
            report.Execute();

            string pageLayout = report.GetJsonDataPage(page);

            return new ContentResult { Content = pageLayout, ContentType = "application/json" };
        }


    }
}
