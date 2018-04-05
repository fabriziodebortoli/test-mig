using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microarea.Common;
using Microarea.Common.Applications;
using Microarea.Common.NameSolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using FakeItEasy;
using Microarea.Common.WebServicesWrapper;
using System.Security.Authentication;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;

namespace tbfs_service.Controllers
{
    public interface IFileHandler
    {
        string[] Upload(ICollection<IFormFile> filesToUpload, string nameSpace, string authenticationTokenKey);
        void Remove(string[] fileNamespaces, string authorizationToken);
    }


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
        [Route("ExistFile")]
        public IActionResult ExistFile(NameSpace objNameSpace, string user, string companyName)
        {
            try
            {

                INameSpaceType type = objNameSpace.NameSpaceType;
                bool isValid = objNameSpace.IsValid();

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(companyName))
                {
                    //Son nella custom
                    //path company 
                    string custompath = PathFinder.PathFinderInstance.GetCustomCompanyPath(companyName);


                }

                //  string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                string json = "";// PathFinder.PathFinderInstance.get(authtoken);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //-------------------------------------------------------------------------------
        public IActionResult GetObjsByCustomizationLevel(Enum objType, string objNamespace, string userName, string company)
        {
             try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                string json = PathFinder.PathFinderInstance.GetJsonAllObjectsByTypeAndCustomizationLevel(authtoken, objNamespace, userName, company, objType);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        ////---------------------------------------------------------------------
        //[Route("GetFileNameFromNamespace")]
        //public IActionResult GetFileNameFromNamespace(NameSpace objNameSpace, string user)
        //{
        //    try
        //    {
        //        objNameSpace.NameSpaceType
        //      //  string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
        //        //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
        //        string json = PathFinder.PathFinderInstance.get(authtoken);
        //        return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
        //    }
        //    catch (Exception e)
        //    {
        //        return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
        //    }
        //}

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
        public IActionResult GetAllObjectsBytype(string appName, string modulesName, ObjectType objType)
        {
            try
            {
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


        UserInfo GetLoginInformation()
        {
            var sAuthT = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
            if (string.IsNullOrEmpty(sAuthT)) return null;
            //ISession hsession = null;
            //try
            //{
            //    hsession = HttpContext.Session;
            //}
            //catch { }
          //  var loginInfo = null; // LoginInfoMessage.GetLoginInformation(hsession, sAuthT);
            return new UserInfo(new LoginInfoMessage(), sAuthT);
        }


        /// <summary>
        /// https://github.com/angular/angular/issues/18680#issuecomment-330425866        
        /// </summary>
        /// <param name="files"></param>
        /// <param name="currentNamespace"></param>
        /// <param name="authenticationTokenKey"></param>
        /// <returns></returns>
        [HttpPost("UploadObject")]
        [RequestSizeLimit(100_000_000)]
        public IActionResult UploadObject(ICollection<IFormFile> files, string currentNamespace) //applicazione moduleo e tipo ???
        {
            var session = GetLoginInformation();
            if (session == null)
                throw new AuthenticationException();
            if (files == null || files.Count < 1)
                throw new ArgumentNullException(nameof(files));
            try
            {
                var handler = A.Fake<IFileHandler>(); // TODO => implementare metodo interfaccia caricamento file
                A.CallTo(() => handler.Upload(files, currentNamespace, session.AuthenticationToken))
                    .Returns(new[] { "a" });
                var finalPaths = handler.Upload(files, currentNamespace, session.AuthenticationToken);
                return Ok(new { Content = files.Select((f, i) => new { name = f.FileName, ns = finalPaths.ElementAt(i) }) });
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }
    }
}
