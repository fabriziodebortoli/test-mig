﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.AccountManager.Controllers
{
    [Route("account-manager")]
    //---------------------------------------------------------------------
    public class AccountManagerService : Controller
    {

        [Route("prelogin")]
        //---------------------------------------------------------------------
        public IActionResult PreLogin()
        {
            string user = HttpContext.Request.Form["user"];
            string password = HttpContext.Request.Form["password"];
            //int PRELOGIN(user, password);       
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.WritePropertyName("result");
            jsonWriter.WriteValue("0");//0 is ok in LoginReturnCodes

            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        [Route("login")]
        //---------------------------------------------------------------------
        public IActionResult Login()
        {
            string tokendep = HttpContext.Request.Form["tokendep"];
            string company = HttpContext.Request.Form["company"];
            string askingProcess = HttpContext.Request.Form["askingProcess"];
            bool overwriteLogin = HttpContext.Request.Form["overwriteLogin"] == "true";
            //int LOGIN(tokendep,company,askingprocess, overwritelogin, out token?);         
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.WritePropertyName("result");
            jsonWriter.WriteValue("0");//0 is ok in LoginReturnCodes

            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }


        [Route("logoff")]
        //---------------------------------------------------------------------
        public IActionResult Logoff()
        {
            string token = HttpContext.Request.Form["token"];
            //bool LOGOFF(TOKEN);           
            var result = new { Success = "True", Message = "" };
            return new JsonResult(result);
        }
    }
}
