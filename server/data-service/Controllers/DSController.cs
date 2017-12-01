using System;
using System.Globalization;
using Microarea.Common;
using Microarea.Common.Applications;
using Microarea.Common.Hotlink;
using Microarea.DataService.Managers.Interfaces;
using Microarea.DataService.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DataService.Controllers
{
    [Route("data-service")]
    public class DSController : Controller
    {
        private readonly IParameterManager _parameterManager;

        public DSController(IParameterManager parameterManager)
        {
            _parameterManager = parameterManager;
        }

        [Route("getinstalleddictionaries")]
        public IActionResult GetInstalledDictionaries()
        {
            CultureInfo[] cultures = Microarea.Common.Generic.InstallationData.GetInstalledDictionaries();
            Dictionaries dic = new Dictionaries();
            foreach (var ci in cultures)
                dic.dictionaries.Add(new Dictionary(ci.Name, ci.NativeName));
            return new JsonResult(dic);
        }

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
        [Route("getselections/{namespace}")]
        public IActionResult GetSelectionTypes(string nameSpace)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            string list;
            if (!ds.GetSelectionTypes(out list))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            return new ContentResult { Content = list, ContentType = "application/json" };
        }

        //---------------------------------------------------------------------
        [Route("getdata/{namespace}/{selectiontype}")]
        public IActionResult GetData(string nameSpace, string selectionType)
        {
            string authHeader = HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };
            }
            UserInfo ui = GetLoginInformation();
            if (ui == null)
            {
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };
            }

            TbSession session = new TbSession(ui, nameSpace);

            JObject jObject = JObject.Parse(authHeader);
            string instanceID = jObject.GetValue("tbLoaderName")?.ToString();  //TODO RSWEB  togliere stringa cablata e usare il tostring del datamember del messaggio
            if (!string.IsNullOrWhiteSpace(instanceID))
            {
                session.TbInstanceID = instanceID;
                session.LoggedToTb = true;
            }

            Datasource ds = new Datasource(session);
            if (!ds.PrepareQueryAsync(HttpContext.Request.Query, selectionType).Result)
                return new ContentResult { Content = "It fails to load", ContentType = "application/text" };

            string records;
            if (!ds.GetRowsJson(out records))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = records, ContentType = "application/json" };
        }

        /*
        [Route("getcolumns/{namespace}/{selectiontype}")]
        public IActionResult GetColumns(string nameSpace, string selectionType)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            if (!ds.PrepareQuery(HttpContext.Request.Query, selectionType))
                return new ContentResult { Content = "It fails to load", ContentType = "application/text" };

            string columns;
            if (!ds.GetColumns(out columns))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            return new ContentResult { Content = columns, ContentType = "application/json" };
        }*/

        [Route("getparameters/{namespace}")]
        public IActionResult GetParameters(string nameSpace)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "no auth", ContentType = "application/text" };

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            string list;
            if (!ds.GetParameters(out list))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            return new ContentResult { Content = list, ContentType = "application/json" };
        }


        //---------------------------------------------------------------------
        [Route("radar")]
        public IActionResult GetRadar()
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new ContentResult { StatusCode = 401, Content = "non sei autenticato!", ContentType = "application/text" };

            Datasource ds = new Datasource(ui);

            if (!ds.PrepareRadar(HttpContext.Request))
                return new ContentResult { Content = "It fails to load", ContentType = "application/text" };

            string records;
            if (!ds.GetRowsJson(out records))
                return new ContentResult { Content = "It fails to execute", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = records, ContentType = "application/json" };
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

