using Microarea.Common.NameSolver;
using Microarea.TbfWebGate.Application;
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
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/[controller]")]
    public class DocumentController : Microsoft.AspNetCore.Mvc.Controller
    {
        OrchestratorService orchestratorService;

        //---------------------------------------------------------------------
        public DocumentController(IHostingEnvironment hostingEnvironment)
        {
            orchestratorService = new OrchestratorService(hostingEnvironment, null);
        }

        //---------------------------------------------------------------------
        // GET api/document/ERP.Sales.Documents.Invoice
        [HttpGet("{documentNamespace}")]
        public string Get(string documentNamespace)
        {
            var context = new CallerContext
            {
                ObjectName = documentNamespace,
                AuthToken = GetAuthToken(HttpContext.Request)
            };

            return orchestratorService.GetDocument(context);
        }

        //---------------------------------------------------------------------
        // GET api/document/ERP.Sales.Documents.Invoice
        [HttpGet("{documentNamespace}")]
        public string Close(string documentNamespace)
        {
            var context = new CallerContext
            {
                ObjectName = documentNamespace,
                AuthToken = GetAuthToken(HttpContext.Request)
            };

            return orchestratorService.CloseDocument(context);
        }

        //---------------------------------------------------------------------
        private string GetAuthToken(HttpRequest request)
        {
            JObject jObject = null;
            string authHeader = request.Headers["Authorization"];
            if (authHeader != null)
            {
                jObject = JObject.Parse(authHeader);
            }

            return authHeader;
        }

        //---------------------------------------------------------------------
        private CallerContext GetContextFromParameters(HttpRequest request)
        {
            CallerContext context = null;
            string callerContext = request.Query["context"];
            if (!string.IsNullOrEmpty(callerContext))
                context = JsonConvert.DeserializeObject<CallerContext>(callerContext);

            if (context == null)
                context = new CallerContext();

            string name = request.Query["name"];
            if (!string.IsNullOrEmpty(name))
                context.ObjectName = name;
            string company = request.Query["company"];

            if (!string.IsNullOrEmpty(company))
                context.Company = company;

            return context;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.orchestratorService != null)
            {
                this.orchestratorService.Dispose();
                this.orchestratorService = null;
            }
        }
    }
}
