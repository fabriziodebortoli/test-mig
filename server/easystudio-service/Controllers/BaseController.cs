
using Microarea.EasyStudio.AspNetCore;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace Microarea.EasyStudio.Controllers
{
    //=========================================================================
    [RequestResultFilters]
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected IServiceManager Services { get; set; }

        public virtual IDiagnosticProvider Diagnostic { get; }

        //---------------------------------------------------------------------
        protected BaseController(IServiceManager serviceManager)
        {
            Services = serviceManager;
        }
    }
}
