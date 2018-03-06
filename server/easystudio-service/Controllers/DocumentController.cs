using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json.Linq;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using Microarea.EasyStudio.Common;

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
