
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace Microarea.EasyStudio.Controllers
{
    //=========================================================================
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected IServiceManager Manager { get; set; }
        //---------------------------------------------------------------------
        protected BaseController(IServiceManager serviceManager)
        {
            Manager = serviceManager;
        }
    }
}
