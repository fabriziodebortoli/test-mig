
using Microarea.EasyStudio.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.EasyStudio.Services;

namespace Microarea.EasyStudio.Controllers
{
    //=========================================================================
 //   [/* Controllo di authtoken AuthenticationFilters,*/ RequestResultFilters]
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected IServiceManager Services { get; set; }

        public virtual IDiagnosticProvider Diagnostic { get; }
		private ApplicationService _service;
		//---------------------------------------------------------------------
		protected ApplicationService ApplicationServiceProp
		{
			get
			{
				if (_service == null)
					_service = Services?.GetService(typeof(ApplicationService)) as ApplicationService;

				return _service;
			}
		}
		//---------------------------------------------------------------------
		protected BaseController(IServiceManager serviceManager)
        {
            Services = serviceManager;
        }

        //---------------------------------------------------------------------
        public IActionResult ToContentResult(int statusCode, IDiagnosticProvider diagnostic)
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = diagnostic.AsJson,
                ContentType = "application/json"
            };
        }

        //---------------------------------------------------------------------
        public IActionResult ToContentResult(string text, int statusCode = 200)
        {
			return new ContentResult
			{
				StatusCode = statusCode,
				Content = text,
                ContentType = "application/json"
            };
        }
    }
}
