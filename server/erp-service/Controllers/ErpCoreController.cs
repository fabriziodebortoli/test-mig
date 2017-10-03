using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace erp_service.Controllers
{
    [Route("erp-core")]
    public class ErpCoreController : Controller
    {
        //-----------------------------------------------------------------------------------------
        [Route("test")]
        public IActionResult Test()
        {
            return new JsonResult(new { Success = true, Message = "" });
        }
    }
}
