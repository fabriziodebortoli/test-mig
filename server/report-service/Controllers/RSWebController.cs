using System;
using System.Text;
using System.IO;
using System.Collections.Specialized;

using Microsoft.AspNetCore.Mvc;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.RSWeb.Render;
using Microarea.Common.NameSolver;
using Microarea.Common.Generic;
using Microsoft.AspNetCore.Cors;
using Microarea.Common;

/*
localhost:5000/rs/template/erp.company.isocountrycodes/1

localhost:5000/rs/data/erp.company.isocountrycodes/1

localhost:5000/rs/xml/erp.company.isocountrycodes

localhost:5000/rs/pdf/erp.company.titles
*/

namespace Microarea.RSWeb.Controllers
{

    [Route("rs")]
    public class RSWebController : Controller
    {
        UserInfo GetLoginInformation()
        {
            string sAuthT = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
            if (string.IsNullOrEmpty(sAuthT))
                return null;

            Microsoft.AspNetCore.Http.ISession hsession = null;
            try
            {
                hsession = HttpContext.Session;
            }
            catch (Exception)
            {
            }

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(hsession, sAuthT);

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            return ui;
        }
        //---------------------------------------------------------------------

        [Route("xml/{namespace}")]
        public IActionResult GetXmlData(string nameSpace)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

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
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            TbReportSession session = new TbReportSession(ui, nameSpace);

            //trucco per parametri
            XmlReportEngine reportParameters = new XmlReportEngine(session);
            string parameters = reportParameters.XmlGetParameters();
            session.ReportParameters = parameters;
            reportParameters = null;
            //--------------------

            PdfReportEngine report = new PdfReportEngine(session);

            string err = string.Empty;

            PDFSharpTests.src.PdfSharp.Pdf.Samples miaClasse = new PDFSharpTests.src.PdfSharp.Pdf.Samples();
            miaClasse.HelloWorld("Arial", 12, PdfSharp.Drawing.XFontStyle.Italic);


            byte[] pdf = report.ExecuteReport(ref err);

            return new ContentResult { Content = pdf.ToString(), ContentType = "application/pdf" };
        }

        //---------------------------------------------------------------------
        [ResponseCache(Duration = 604800, Location = ResponseCacheLocation.Any)]
        [EnableCors("CorsPolicy")]
        [Route("image/{namespace}")]
        public IActionResult GetImage(string nameSpace)
        {
            if (nameSpace.IsNullOrEmpty())
                return new ContentResult { Content = "Empty file name", ContentType = "application/text" };

            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            string filename = nameSpace;
            if (!System.IO.File.Exists(filename))
            {
                PathFinder pathFinder = new PathFinder(ui.Company, ui.ImpersonatedUser);

                NameSpace ns = new NameSpace(nameSpace, NameSpaceObjectType.Image);
                if (!ns.IsValid())
                    ns = new NameSpace(nameSpace, NameSpaceObjectType.File);
                if (!ns.IsValid())
                    new ContentResult { Content = "Wrong namespace " + nameSpace, ContentType = "application/text" };

                filename = pathFinder.GetFilename(ns, string.Empty);
                if (filename == string.Empty)
                    return new ContentResult { Content = "Empty file name " + nameSpace, ContentType = "application/text" };
            }
            if (!System.IO.File.Exists(filename))
                return new ContentResult { Content = "File does not exists " + filename, ContentType = "application/text" };

            string ext = System.IO.Path.GetExtension(filename).TrimStart('.');

            try
            {
                this.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                FileStream f = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                return new FileStreamResult(f, "image/" + ext);
            }
            catch (Exception)
            {
            }
            return new ContentResult { Content = "Cannot access file " + filename, ContentType = "application/text" };
        }

        //---------------------------------------------------------------------
        [Route("excel/{filename}")]
        public IActionResult GetExcel(string filename)
        {
            string path = Path.GetTempPath();
            string pathComplete = path + filename;

            if (pathComplete.IsNullOrEmpty())
                return new ContentResult { Content = "Empty file name", ContentType = "application/text" };

            /*UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };*/

            if (!System.IO.File.Exists(pathComplete))
                return new ContentResult { Content = "File does not exists " + filename, ContentType = "application/text" };

            try
            {
                FileStream f = System.IO.File.Open(pathComplete, FileMode.Open, FileAccess.Read, FileShare.Read);

                Response.ContentType = "application/vnd.ms-excel";
                Response.Headers.Add("content-disposition", "attachment; filename="+filename);

                return new FileStreamResult(f, "application/vnd.ms-excel");

            }
            catch (Exception)
            {
            }
            return new ContentResult { Content = "Cannot access file " + filename, ContentType = "application/text" };
        }

        //---------------------------------------------------------------------
        [Route("docx/{filename}")]
        public IActionResult GetDocx(string filename)
        {
            string path = Path.GetTempPath();
            string pathComplete = path + filename;

            if (pathComplete.IsNullOrEmpty())
                return new ContentResult { Content = "Empty file name", ContentType = "application/text" };

            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            if (!System.IO.File.Exists(pathComplete))
                return new ContentResult { Content = "File does not exists " + filename, ContentType = "application/text" };

            try
            {
                FileStream f = System.IO.File.Open(pathComplete, FileMode.Open, FileAccess.Read, FileShare.Read);

                Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                Response.Headers.Add("content-disposition", "attachment; filename=" + filename);

                return new FileStreamResult(f, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

            }
            catch (Exception)
            {
            }
            return new ContentResult { Content = "Cannot access file " + filename, ContentType = "application/text" };
        }


        //---------------------------------------------------------------------
        //for DEBUG
        [Route("template/{namespace}/{page}")]
        public IActionResult GetJsonPageTemplate(string nameSpace, int page)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            TbReportSession session = new TbReportSession(ui, nameSpace);

            JsonReportEngine report = new JsonReportEngine(session);
            report.Execute();

            string pageLayout = report.GetJsonTemplatePage(ref page);

            return new ContentResult { Content = pageLayout, ContentType = "application/json" };
        }

        //---------------------------------------------------------------------
        //for DEBUG
        [Route("data/{namespace}/{page}")]
        public IActionResult GetJsonPageData(string nameSpace, int page)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            TbReportSession session = new TbReportSession(ui, nameSpace);

            JsonReportEngine report = new JsonReportEngine(session);
            report.Execute();

            string pageLayout = report.GetJsonDataPage(page);

            return new ContentResult { Content = pageLayout, ContentType = "application/json" };
        }

        //---------------------------------------------------------------------
        //for DEBUG
        [Route("dialogs/{namespace}")]
        public IActionResult GetJsonDialog(string nameSpace, string name)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            TbReportSession session = new TbReportSession(ui, nameSpace);

            JsonReportEngine report = new JsonReportEngine(session);
            report.Execute();

            string dlg = report.GetJsonAskDialogs();

            return new ContentResult { Content = dlg, ContentType = "application/json" };
        }
        //---------------------------------------------------------------------
    }
}
