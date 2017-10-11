using Microsoft.AspNetCore.Mvc;
using Microarea.MenuGate.Models;
using Microarea.Common.MenuLoader;
using Microarea.TaskBuilderNet.Core.Generic;
using System.IO;
using Microarea.Common.NameSolver;
using System;
using Microarea.Common.Generic;
using System.Threading;
using Microarea.Common.WebServicesWrapper;

namespace Microarea.Menu.Controllers
{
    //da ripristinare quando inserisce il nuovo menu nel cef
    [Route("menu-service")]
    public class MenuController : Controller
    {
        //---------------------------------------------------------------------
        public MenuController()
        {
        }

        //---------------------------------------------------------------------
        [Route("getMenuElements")]
        public IActionResult GetMenuElements()
        {
            try
            {
                string user = HttpContext.Request.Form["user"];
                string company = HttpContext.Request.Form["company"];
                string authtoken = HttpContext.Request.Form["authtoken"];

                string clearCachedData = HttpContext.Request.Form["clearCachedData"];
                bool clearCache = bool.Parse(clearCachedData);

                string content = NewMenuLoader.LoadMenuWithFavoritesAsJson(user, company, authtoken, clearCache);
                return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getPreferences")]
        public IActionResult GetPreferences()
        {
            try
            {
                string user = HttpContext.Request.Form["user"];
                string company = HttpContext.Request.Form["company"];

                string content = NewMenuLoader.GetPreferencesAsJson(user, company);
                return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("setPreference")]
        public IActionResult SetPreference()
        {
            try
            {
                string user = HttpContext.Request.Form["user"];
                string company = HttpContext.Request.Form["company"];
                string preferenceName = HttpContext.Request.Form["name"];
                string preferenceValue = HttpContext.Request.Form["value"];

                bool result = NewMenuSaver.SetPreference(preferenceName, preferenceValue, user, company);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getThemedSettings")]
        public IActionResult GetThemedSettings()
        {
            try
            {
                string token = HttpContext.Request.Form["authtoken"];
                string content = NewMenuLoader.GetJsonMenuSettings(token);
                return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getConnectionInfo")]
        public IActionResult GetConnectionInfo()
        {
            try
            {
                string token = HttpContext.Request.Form["authtoken"];
                string content = NewMenuLoader.GetConnectionInformation(token);
                return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("clearAllMostUsed")]
        public IActionResult ClearAllMostUsed()
        {
            try
            {
                string user = HttpContext.Request.Form["user"];
                string company = HttpContext.Request.Form["company"];
                NewMenuSaver.ClearMostUsed(user, company);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getMostUsedShowNr")]
        public IActionResult GetMostUsedShowNr()
        {
            try
            {
                string user = HttpContext.Request.Form["user"];
                string company = HttpContext.Request.Form["company"];
                string content = NewMenuLoader.GetMostUsedShowNrElements(user, company);
                return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("updateAllFavoritesAndMostUsed")]
        public IActionResult UpdateAllFavoritesAndMostUsed()
        {
            try
            {
                string favorites = HttpContext.Request.Form["favorites"];
                string mostUsed = HttpContext.Request.Form["mostUsed"];
                string user = HttpContext.Request.Form["user"];
                string company = HttpContext.Request.Form["company"];
                NewMenuSaver.UpdateAllFavoritesAndMostUsed(favorites, mostUsed, user, company);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "text/plain" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("clearCachedData")]
        public IActionResult ClearCachedData()
        {
            try
            {
                string user = HttpContext.Request.Form["user"];
                Microarea.Common.Generic.InstallationInfo.Functions.ClearCachedData(user);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getLocalizedElements")]
        public IActionResult GetLocalizedElements()
        {
            try
            {
                string token = HttpContext.Request.Form["authtoken"];
                string needLoginThread = HttpContext.Request.Form["needLoginThread"];

                string json = NewMenuLoader.GetLocalizationJson(token);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }


        //---------------------------------------------------------------------
        [Route("getProductInfo")]
        public IActionResult GetProductInfo()
        {
            try
            {
                string token = HttpContext.Request.Form["authtoken"];
                string json = NewMenuLoader.GetJsonProductInfo(token);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getPingViaSMSUrl")]
        public IActionResult GetPingViaSMSUrl()
        {
            try
            {
                string url = MenuStaticFunctions.PingViaSMSUrl();
                string json = string.Format("{{ \"url\": \"{0}\" }}", url);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getProducerSite")]
        public IActionResult GetProducerSite()
        {
            try
            {
                string url = MenuStaticFunctions.ProducerSiteUrl();
                string json = string.Format("{{ \"url\": \"{0}\" }}", url);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getStaticImage/{imageFile?}")]
        public IActionResult getStaticImage(string imageFile)
        {
            string fullImagePath = Path.Combine(BasePathFinder.BasePathFinderInstance.GetStandardPath(), imageFile);
            if (!System.IO.File.Exists(fullImagePath))
                return new ContentResult { Content = "File does not exists " + fullImagePath, ContentType = "application/text" };

            string ext = System.IO.Path.GetExtension(fullImagePath);

            try
            {
                FileStream f = System.IO.File.Open(fullImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return new FileStreamResult(f, "image/" + ext);
            }
            catch (Exception)
            {
            }

            return new ContentResult { Content = "Cannot access file " + fullImagePath, ContentType = "application/text" };
        }

        //---------------------------------------------------------------------
        [Route("getOnlineHelpUrl")]
        public IActionResult GetOnlineHelpUrl()
        {
            try
            {
                string nameSpace = HttpContext.Request.Form["nameSpace"];
                string culture = HttpContext.Request.Form["culture"];

                string url = HelpManager.GetOnlineHelpUrl(nameSpace, culture);
                string json = string.Format("{{ \"url\": \"{0}\" }}", url);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }
          
    }
}


