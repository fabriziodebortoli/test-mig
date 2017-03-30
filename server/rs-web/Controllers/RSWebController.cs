using System;
using System.Text;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Net;
using System.Collections.Specialized;

using Microsoft.AspNetCore.Mvc;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.RSWeb.Render;
using Microarea.Common.NameSolver;
using Microarea.Common.Generic;

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
        //---------------------------------------------------------------------
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

        //---------------------------------------------------------------------
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

        //---------------------------------------------------------------------
        [Route("template/{namespace}/{page}")] 
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

        //---------------------------------------------------------------------
        [Route("image/{namespace}")] 
        public IActionResult GetImage(string nameSpace)
        {
            /* string sAuthT = HttpContext.Request.Cookies["authtoken"];
             if (string.IsNullOrEmpty(sAuthT))
                 return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

             if (nameSpace.IsNullOrEmpty())
                 return new ContentResult { Content = "", ContentType = "application/text" }; 

             string filename = nameSpace;
             if (!System.IO.File.Exists(filename))
             {
                 LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;

                 UserInfo ui = new UserInfo(loginInfo, sAuthT);

                 PathFinder pathFinder = new PathFinder(ui.Company, ui.ImpersonatedUser);

                 NameSpace ns = new NameSpace(nameSpace, NameSpaceObjectType.Image);
                 filename = pathFinder.GetFilename(ns, string.Empty);
                 if (filename == string.Empty)
                     return new ContentResult { Content = "", ContentType = "application/text" };
             }
             /*if (!System.IO.File.Exists(filename))
                 return new ContentResult { Content = "", ContentType = "application/text" }; ;                          */
            string filename = "C:\\TB_4_0_ERP\\Standard\\Applications\\ERP\\Company\\Files\\Images";
            string ext = System.IO.Path.GetExtension(filename);

            using (/*StreamReader*/ FileStream f = System.IO.File.Open(filename, FileMode.Open))
            {
                //return sr.ReadByte ReadToEnd();
                long len = f.Length;
                byte[] ar = new byte[len];
                int ret = f.Read(ar, 0, (int)len);


                string s = Convert.ToBase64String(ar);

                return new ContentResult { Content = s, ContentType = "image/" + ext };
            }


            
        }

        //---------------------------------------------------------------------
        [Route("data/{namespace}/{page}")] 
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
        //---------------------------------------------------------------------
    }
}
