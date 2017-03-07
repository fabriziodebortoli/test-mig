using Microsoft.AspNetCore.Mvc;
using Microarea.DataService.Models;
using Microarea.Common.Applications;

namespace DataService.Controllers
{
    [Route("data-service")]
    public class DSController : Controller
    {
        private LoginInfoMessage loginInfo = null;

        [Route("getdata/{namespace}")]
        public IActionResult GetData(string nameSpace)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            //DSInitMessage message = new DSInitMessage();
            //message.selection_type = 
            string s = HttpContext.Request.Query["selection_type"];
            //message.like_value = HttpContext.Request.Query["like_value"];
            //message.disabled = HttpContext.Request.Query["disabled"];
            //message.good_type = HttpContext.Request.Query["good_type"];

            if (loginInfo == null)
            {
                loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;
            }

            UserInfo ui = new UserInfo(loginInfo, sAuthT);
 
            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            if (!ds.PrepareQuery(HttpContext.Request.Query))
                return new ContentResult { Content = "It fails to load", ContentType = "application/text" };

            string records;
            if (!ds.GetCompactJson(out records))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = records, ContentType = "application/json" };
        }

        [Route("getselections/{namespace}")]
        public IActionResult GetSelectionTypes(string nameSpace)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            if (loginInfo == null)
            {
                loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;
            }

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            string list;
            if (!ds.EnumSelectionTypes(out list))
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

            if (loginInfo == null)
            {
                loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;
            }

            UserInfo ui = new UserInfo(loginInfo, sAuthT);

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            string list;
            if (!ds.EnumParameters(out list))
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

