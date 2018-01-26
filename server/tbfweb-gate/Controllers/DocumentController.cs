using Microarea.Common.NameSolver;
using Microarea.TbfWebGate.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Controllers;
using TaskBuilderNetCore.Documents.Model;

namespace Microarea.TbfWebGate.Controllers
{
    //=========================================================================
    //[ApiVersion("1.0")]
    //[Route("api/{version:apiVersion}/[controller]")]
    //[Authorize(Policy = "LoggedIn")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class DocumentController : Microsoft.AspNetCore.Mvc.Controller
    {
        IOrchestratorService orchestratorService;

        //---------------------------------------------------------------------
        public DocumentController(IOrchestratorService orchestratorService)
        {
            this.orchestratorService = orchestratorService;
        }

        //---------------------------------------------------------------------
        // GET api/document/
        [HttpGet]
        public IActionResult Get()
        {
            var userInfo = this.GetLoginInformation(null, HttpContext.Request, HttpContext.Session);
            if (userInfo == null)
            {
                return Forbid();
            }
            return Json(orchestratorService.GetAllDocuments());
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

            return Json(orchestratorService.GetDocument(context));
        }

        //---------------------------------------------------------------------
        // GET api/document/close?documentNamespace=ERP.Sales.Documents.Invoice
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

            return Json(orchestratorService.CloseDocument(context));
        }
    }
}
