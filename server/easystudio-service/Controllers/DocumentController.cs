using Microsoft.AspNetCore.Mvc;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace Microarea.EasyStudio.Controllers
{
    //=========================================================================
    [Route("easystudio/document")]
    public class DocumentController : BaseController
    {
        //---------------------------------------------------------------------
        public DocumentController(IServiceManager serviceManager)
            : base(serviceManager)
        {
		} 
	}
}
