using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microarea.TbfWebGate.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.Documents.Model;

namespace Microarea.TbfWebGate.Controllers
{
    //=========================================================================
    [Authorize(Policy = "LoggedIn")]
    [Produces("application/json")]
    [Route("api/Component")]
    public class ComponentController : Controller
    {
        IOrchestratorService orchestratorService;

        //---------------------------------------------------------------------
        public ComponentController(IOrchestratorService orchestratorService)
        {
            this.orchestratorService = orchestratorService;
        }

        //---------------------------------------------------------------------
        // GET: api/Component
        [HttpGet]
        public IActionResult Get()
        {
            var userInfo = this.GetLoginInformation(null, HttpContext.Request, HttpContext.Session);
            if (userInfo == null)
            {
                return Forbid();
            }
            return Json(orchestratorService.GetAllComponents());
        }

        //---------------------------------------------------------------------
        // GET api/document/ERP.Sales.Documents.Invoice
        [HttpGet("{namespace}")]
        public IActionResult GetByNamespace(string @namespace)
        {
            var userInfo = this.GetLoginInformation(null, HttpContext.Request, HttpContext.Session);
            if (userInfo == null)
            {
                return Forbid();
            }
            var context = new CallerContext
            {
                ObjectName = @namespace,
                AuthToken = userInfo.AuthenticationToken
            };

            return Json(orchestratorService.GetComponent(context));
        }

        //---------------------------------------------------------------------
        // GET api/component/close?namespace=ERP.Sales.Documents.Invoice
        [Route("close")]
         public IActionResult Close([FromQuery]string @namespace)
        {
            var userInfo = this.GetLoginInformation(null, HttpContext.Request, HttpContext.Session);
            if (userInfo == null)
            {
                return Forbid();
            }
            var context = new CallerContext
            {
                ObjectName = @namespace,
                AuthToken = userInfo.AuthenticationToken
            };

            return Json(orchestratorService.CloseComponent(context));
        }
    }
}
