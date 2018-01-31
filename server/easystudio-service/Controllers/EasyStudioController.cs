using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microarea.EasyStudio.Controllers
{
    //=========================================================================
 
    //[Produces("application/json")]
    [Route("easystudio")]
    public class EasyStudioController : Microsoft.AspNetCore.Mvc.Controller
    {
        //---------------------------------------------------------------------
        public EasyStudioController()
        {
        }

        //---------------------------------------------------------------------
        [Route("getpippo")]
        public IActionResult GetPippo()
        {
            try
            {
/*                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string content = NewMenuLoader.GetJsonMenuSettings(authtoken);*/
                return new ContentResult { StatusCode = 200, Content = "aaa {}", ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }
    }
}
