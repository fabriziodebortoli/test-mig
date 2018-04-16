using System;
using System.Collections.Generic;
using System.Linq;
using Microarea.Common;
using Microarea.Common.Applications;
using Microarea.Common.NameSolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FakeItEasy;
using System.Security.Authentication;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;
using System.IO;

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
        [Route("GettAllApplications")] //Paolo
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
        [Route("ExistObject")]
        public IActionResult ExistObject(string objnameSpace, string user, string companyName, string culture)
        {
            try
            {
                NameSpace objNameSpace = new NameSpace(objnameSpace);

                if (!objNameSpace.IsValid())
                    return new ContentResult { StatusCode = 200, Content = string.Empty, ContentType = "application/json" };

                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);

                PathFinder pf = new PathFinder(companyName, user);
                bool exist = pf.ExistObject(objNameSpace, user, companyName, culture);

                return new ContentResult { StatusCode = 200, Content = exist.ToJson(), ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("GetDescription")]
        public IActionResult GetDescription(string objnameSpace, string user, string companyName, string culture)
        {
            try
            {
                var objNameSpace = new NameSpace(objnameSpace);

                if (!objNameSpace.IsValid())
                    return new ContentResult { StatusCode = 200, Content = string.Empty, ContentType = "application/json" };

                var authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);

                var pf = new PathFinder(companyName, user);
                var description = pf.GetDescription(objNameSpace, culture);

                return new ContentResult { StatusCode = 200, Content = description.ToJson(), ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }


        //-------------------------------------------------------------------------------
        public IActionResult GetObjsByCustomizationLevel(Enum objType, string objNamespace, string userName, string company) //paolo
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                PathFinder pf = new PathFinder(company, userName);
                string json = pf.GetJsonAllObjectsByTypeAndCustomizationLevel(authtoken, objNamespace, userName, company, objType);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("GetFileNameFromNamespace")]//mauro
        public IActionResult GetFileNameFromNamespace(string objnameSpace, string user, string company, string culture)
        {
            try
            {
                NameSpace objNameSpace = new NameSpace(objnameSpace);
                if (!objNameSpace.IsValid())
                    return new ContentResult { StatusCode = 200, Content = string.Empty, ContentType = "application/json" };

                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);

                PathFinder pf = new PathFinder(company, user);
                string json = pf.GetFileNameFromNamespace(objNameSpace, user, company, culture, false);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("GetAllModulesByApplication")] //paolo
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
        [Route("GetAllObjectsBytype")] //paolo
        public IActionResult GetAllObjectsBytype(string appName, string modulesName, ObjectType objType)
        {
            var session = GetLoginInformation();
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                var pathFinder = new PathFinder(session.Company, session.User);

                var ns = $"{appName}.{modulesName}";
                string json = pathFinder.GetObjsByCustomizationLevel(authtoken, appName, modulesName, objType, NameSolverStrings.AllUsers);
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
            Microsoft.AspNetCore.Http.ISession hsession = null;
            try
            {
                hsession = HttpContext.Session;
            }
            catch { }
            var loginInfo = LoginInfoMessage.GetLoginInformation(hsession, sAuthT);
            return new UserInfo(loginInfo, sAuthT);
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
        public IActionResult UploadObject(ICollection<IFormFile> files, ObjectType objectType, string currentNamespace, string user) //applicazione moduleo e tipo ???
        {
            var session = GetLoginInformation();
            if (session == null)
                throw new AuthenticationException();
            if (files == null || files.Count < 1)
                throw new ArgumentNullException(nameof(files));
            try
            {
                var pf = new PathFinder(session.Company, user);
                var ok = false;

                foreach (var file in files)
                {
                    var partialNamespace = $"{Enum.GetName(typeof(ObjectType), objectType)}.{currentNamespace}.{file.FileName}";
                    var fullNameSpace = new NameSpace(partialNamespace);
                    var filename = pf.GetFileNameFromNamespace(fullNameSpace, user, session.Company, session.CompanyCulture.EnglishName, true);

                    if (objectType == ObjectType.Report)
                    {
                        using (var stream = file.OpenReadStream())
                            ok = pf.SaveTextFileFromStream(filename, stream);
                    }
                    else
                    {
                        var byteArray = new byte[file.Length];
                        using (var stream = file.OpenReadStream())
                        {
                            var len = stream.Read(byteArray, 0, (int)file.Length);
                            ok = len > 0 ? pf.SaveBinaryFile(filename, byteArray, len) : false;
                        }
                    }
                    if (!ok)
                        break;
                }
                return Ok();
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }
    }
}
