using Microsoft.AspNetCore.Mvc;
using Microarea.DataService.Models;
using Microarea.Common.Applications;

namespace DataService.Controllers
{
    [Route("data-service")]
    public class DSController : Controller
    {
        [Route("getdata/{namespace}/{selectiontype}")]
        public IActionResult GetData(string nameSpace, string selectionType)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            //Microsoft.AspNetCore.Session["logininfo"];
            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;

            UserInfo ui = new UserInfo(loginInfo, sAuthT);
 
            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            if (!ds.PrepareQuery(HttpContext.Request.Query, selectionType))
                return new ContentResult { Content = "It fails to load", ContentType = "application/text" };

            string records;
            if (!ds.GetCompactJson(out records))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = records, ContentType = "application/json" };
        }

        [Route("getcolumns/{namespace}/{selectiontype}")]
        public IActionResult GetColumns(string nameSpace, string selectionType)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            if (!ds.PrepareQuery(HttpContext.Request.Query, selectionType))
                return new ContentResult { Content = "It fails to load", ContentType = "application/text" };

            string columns;
            if (!ds.GetColumns(out columns))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = columns, ContentType = "application/json" };
        }

        [Route("getselections/{namespace}")]
        public IActionResult GetSelectionTypes(string nameSpace)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            string list;
            if (!ds.GetSelectionTypes(out list))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = list, ContentType = "application/json" };
        }

        [Route("getparameters/{namespace}")]
        public IActionResult GetParameters(string nameSpace)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            string list;
            if (!ds.GetParameters(out list))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = list, ContentType = "application/json" };
        }

        //---------------------------------------------------------------------
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

