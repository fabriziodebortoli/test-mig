﻿using System;
using System.Globalization;
using Microarea.Common;
using Microarea.Common.Applications;
using Microarea.Common.Hotlink;
using Microarea.DataService.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;

namespace DataService.Controllers
{
    [Route("data-service")]
    public class DSController : Controller
    {
        private string tbBaseAddress = "http://localhost:5000/";
        //---------------------------------------------------------------------
        public DSController(IOptions<TbLoaderGateConfigParameters> parameters, IHostingEnvironment hostingEnvironment)
        {
            if (!string.IsNullOrWhiteSpace(parameters.Value.TbLoaderGateFullUrl))
            {
                tbBaseAddress = parameters.Value.TbLoaderGateFullUrl;
            }
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
                return GetAuthenticationErrorResponse();

            TbSession session = new TbSession(ui, nameSpace);
            session.TbBaseAddress = tbBaseAddress;

            Datasource ds = new Datasource(session);

            string list;
            if (!ds.GetSelectionTypes(out list))
                return new ErrorResult("It fails to execute");

            return new SuccessResult(list);
        }

        //---------------------------------------------------------------------
        [HttpGet]
        [Route("getdata/{namespace}/{selectiontype}")]
        public IActionResult GetData(string nameSpace, string selectionType)
        {
            string authHeader = HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return GetAuthenticationErrorResponse();
            }
            UserInfo ui = GetLoginInformation();
            if (ui == null)
            {
                return GetAuthenticationErrorResponse();
            }

            TbSession session = new TbSession(ui, nameSpace);
            session.TbBaseAddress = tbBaseAddress;

            JObject jObject = JObject.Parse(authHeader);
            string instanceID = jObject == null ? null : jObject.GetValue("tbLoaderName")?.ToString();  //TODO RSWEB  togliere stringa cablata e usare il tostring del datamember del messaggio
            if (!string.IsNullOrWhiteSpace(instanceID))
            {
                session.TbInstanceID = instanceID;
                session.LoggedToTb = true;
            }

            Datasource ds = new Datasource(session);
            if (!ds.PrepareQueryAsync(HttpContext.Request.Query, selectionType).Result)
            {
                return new ErrorResult("It fails to load");
            }
            string records;
            if (!ds.GetRowsJson(out records))
            {
                return new ErrorResult("It fails to execute");
            }
            //---------------------
            return new SuccessResult(records);
        }

        /*
        [Route("getcolumns/{namespace}/{selectiontype}")]
        public IActionResult GetColumns(string nameSpace, string selectionType)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new NoAuthResult("Not authenticated");

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            if (!ds.PrepareQuery(HttpContext.Request.Query, selectionType))
                return new ErrorResult("It fails to load");

            string columns;
            if (!ds.GetColumns(out columns))
                return new ErrorResult("It fails to execute");

            return new SuccessResult(columns);
        }

        [Route("getparameters/{namespace}")]
        public IActionResult GetParameters(string nameSpace)
        {
            UserInfo ui = GetLoginInformation();
            if (ui == null)
                return new NoAuthResult("Not authenticated");

            TbSession session = new TbSession(ui, nameSpace);

            Datasource ds = new Datasource(session);

            string list;
            if (!ds.GetParameters(out list))
                return new ErrorResult("It fails to execute");

            return new SuccessResult(list);
        }*/


        //---------------------------------------------------------------------
        // radar/nsdoc/finder, radar/nsdoc/browser,  radar/nsdoc/query-name
        [Route("radar")]
        public IActionResult GetRadar()
        {
            string authHeader = HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return GetAuthenticationErrorResponse();
            }
            UserInfo ui = GetLoginInformation();
            if (ui == null)
            {
                return GetAuthenticationErrorResponse();
            }

            TbSession session = new TbSession(ui, null);
            session.TbBaseAddress = tbBaseAddress;

            JObject jObject = JObject.Parse(authHeader);
            string instanceID = jObject.GetValue("tbLoaderName")?.ToString();  //TODO RSWEB  togliere stringa cablata e usare il tostring del datamember del messaggio
            if (!string.IsNullOrWhiteSpace(instanceID))
            {
                session.TbInstanceID = instanceID;
                session.LoggedToTb = true;
            }

            Datasource ds = new Datasource(session);

            ResponseRadarInfo responseRadarInfo = ds.PrepareRadar(HttpContext.Request.Query/*nsDoc, , name*/).Result;
            if (responseRadarInfo == null)
                return new ErrorResult("It fails to load");

            string records;
            if (!ds.GetRowsJson(out records, responseRadarInfo.radarInfo.recordKeys))
                return new ErrorResult("It fails to execute");

            //---------------------
            return new SuccessResult(records);
        }


        //---------------------------------------------------------------------
        private IActionResult GetAuthenticationErrorResponse()
        {
            return new NoAuthResult("Not authenticated");
        }
    }
}

