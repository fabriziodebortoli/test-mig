﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microarea.DataService.Models;

namespace DataService.Controllers
{
    [Route("data-service")]
    public class DSController : Controller
    {
        [Route("GetData/{namespace}/{queryname}")]
        public IActionResult GetData()
        {
            string sAuthT = HttpContext.Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(sAuthT))
                return new ContentResult { StatusCode = 504, Content = "non sei autenticato!", ContentType = "application/text" };

            //HttpContext.Context.Request.Query

            //TODO login come RS
            //  var response = await client.PostAsync("account-manager/getLoginInformation/", content);

            Datasource ds = new Datasource(null);
            if (!ds.Load("erp.items.ds_ItemsSimple", "Code"))
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
