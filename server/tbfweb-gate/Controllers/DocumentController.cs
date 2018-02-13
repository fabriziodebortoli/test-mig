using Microarea.TbfWebGate.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.Documents.Model.Interfaces;
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

        //---------------------------------------------------------------------
        // GET api/document/execute?documentNamespace=Documet.NEWERP.Orders.Documents.SaveOrders
        [Route("execute")]
        public IActionResult Execute([FromQuery]string @namespace)
        {
            var userInfo = this.GetLoginInformation(null, HttpContext.Request, HttpContext.Session);
            if (userInfo == null)
            {
                return Forbid();
            }
            var context = new CallerContext
            {
                ObjectName = @namespace,
                AuthToken = userInfo.AuthenticationToken,
                Mode = ExecutionMode.Unattended
            };

            return Json(orchestratorService.ExecuteActivity(context));
        }
    }
}
