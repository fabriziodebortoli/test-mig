using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microarea.DataService.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Microarea.Common.Applications;

namespace DataService.Controllers
{
    [Route("data-service")]
    public class DSController : Controller
    {
        private LoginInfoMessage loginInfo;

        [Route("getdata/{namespace}")]
        public IActionResult GetData(string nameSpace)
        {
            string sAuthT = HttpContext.Request.Cookies["authtoken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            var jsonObj = JsonConvert.SerializeObject(HttpContext.Request.Query);
            DSInitMessage message = JsonConvert.DeserializeObject<DSInitMessage>(jsonObj);

            if (loginInfo == null)
            {
                loginInfo = LoginInfoMessage.GetLoginInformation(sAuthT).Result;
            }
           
            //TODO login come RS
            //  var response = await client.PostAsync("account-manager/getLoginInformation/", content);

            Datasource ds = new Datasource(null);
            if (!ds.Load(nameSpace, message.selection_type))
                return new ContentResult { Content = "It fails to load", ContentType = "application/text" };

            //---------------------
            return new ContentResult { Content = "e mo ci vogliono i dati", ContentType = "application/json" };
        }

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
