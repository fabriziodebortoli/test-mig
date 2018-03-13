using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microarea.Common;
using Microarea.Common.Applications;
using Microarea.Common.NameSolver;
using Microsoft.AspNetCore.Mvc;

namespace tbfs_service.Controllers
{
    [Route("tbfs-service")]
    public class TBFSController : Controller
    {

        //---------------------------------------------------------------------
        public TBFSController()
        {
        }


        //---------------------------------------------------------------------
        [Route("GettAllApplications")]
        public IActionResult GettAllApplications()
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                string json = PathFinder.PathFinderInstance.GetJsonAllApplications(authtoken);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("GetAllModulesByApplication")]
        public IActionResult GetAllModulesByApplication(string appName)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                string json = PathFinder.PathFinderInstance.GetJsonAllModulesByApplication(authtoken, appName);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("GetAllObjectsBytype")]
        public IActionResult GetAllObjectsBytype(string appName, string modulesName, Enum objType)
        {
            try
            {
                appName = "ERP";
                modulesName = "ACCOUNTING";
                objType = ObjectType.Report;
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                string json = PathFinder.PathFinderInstance.GetJsonAllObjectsByType(authtoken, appName, modulesName, objType);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }
    }
}
