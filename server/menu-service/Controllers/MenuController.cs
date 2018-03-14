using Microsoft.AspNetCore.Mvc;
using Microarea.Common.MenuLoader;
using Microarea.TaskBuilderNet.Core.Generic;
using System.IO;
using Microarea.Common.NameSolver;
using System;
using Microarea.Common.Generic;
using Newtonsoft.Json.Linq;
using Microarea.Common;
using Microarea.Common.Applications;
using System.Text;
using Newtonsoft.Json;

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
        [Route("isCachedMenuTooOld")]
        public IActionResult IsCachedMenuTooOld([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string storageMenuDate = value["storageMenuDate"]?.Value<string>();
                long nStorageMenudate = 0;
                long.TryParse(storageMenuDate, out nStorageMenudate);
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(nStorageMenudate);
                DateTime dateTime = dateTimeOffset.UtcDateTime;
                bool isTooOld = true;

                return new ContentResult { StatusCode = 200, Content = isTooOld.ToJson(), ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("getMenuElements")]
        public IActionResult GetMenuElements([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string clearCachedData = value["clearCachedData"]?.Value<string>();
                string storageMenuDate = value["storageMenuDate"]?.Value<string>();
                long nStorageMenudate = 0;
                long.TryParse(storageMenuDate, out nStorageMenudate);

                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(nStorageMenudate);
                DateTime dateTime = dateTimeOffset.UtcDateTime;
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
        public IActionResult GetPreferences([FromBody] JObject value)
        {
            try
            {
                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();

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
        public IActionResult SetPreference([FromBody] JObject value)
        {
            try
            {
                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string preferenceName = value["name"]?.Value<string>();
                string preferenceValue = value["value"]?.Value<string>();

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
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string content = NewMenuLoader.GetJsonMenuSettings(authtoken);
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
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string content = NewMenuLoader.GetConnectionInformation(authtoken);
                return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("clearAllMostUsed")]
        public IActionResult ClearAllMostUsed([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
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
        public IActionResult GetMostUsedShowNr([FromBody] JObject value)
        {
            try
            {
                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string content = NewMenuLoader.GetMostUsedShowNrElements(user, company);
                return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("updateFavorites")]
        public IActionResult UpdateFavorites([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string favorites = value["favorites"]?.Value<string>();
                NewMenuSaver.UpdateFavorites(favorites, user, company);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "text/plain" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("updateMostUsed")]
        public IActionResult UpdateMostUsed([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string mostUsed = value["mostUsed"]?.Value<string>();
                NewMenuSaver.UpdateMostUsed(mostUsed, user, company);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "text/plain" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }


        //---------------------------------------------------------------------
        [Route("clearCachedData")]
        public IActionResult ClearCachedData([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string user = value["user"]?.Value<string>();
                Microarea.Common.Generic.InstallationInfo.Functions.ClearCachedData(user);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
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
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                string json = NewMenuLoader.GetJsonProductInfo(authtoken);
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
            string fullImagePath = Path.Combine(PathFinder.PathFinderInstance.GetStandardPath, imageFile);

            if (!PathFinder.PathFinderInstance.ExistFile(fullImagePath))
                return new ContentResult { Content = "File does not exists " + fullImagePath, ContentType = "text/plain" };

            string ext = System.IO.Path.GetExtension(fullImagePath);

            try
            {
                Stream f = PathFinder.PathFinderInstance.GetStream(fullImagePath, false);

                return new FileStreamResult(f, "image/" + ext);
            }
            catch (Exception)
            {
            }

            return new ContentResult { Content = "Cannot access file " + fullImagePath, ContentType = "text/plan" };
        }

        //---------------------------------------------------------------------
        [Route("getOnlineHelpUrl")]
        public IActionResult GetOnlineHelpUrl([FromBody] JObject value)
        {
            try
            {
                string nameSpace = value["nameSpace"]?.Value<string>();
                string culture = value["culture"]?.Value<string>();
                string url = HelpManager.GetOnlineHelpUrl(nameSpace, culture);
                string json = string.Format("{{ \"url\": \"{0}\" }}", url);
                return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("addToHiddenTiles")]
        public IActionResult AddToHiddenTiles([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string appName = value["application"]?.Value<string>();
                string groupName = value["group"]?.Value<string>();
                string menuName = value["menu"]?.Value<string>();
                string tileName = value["tile"]?.Value<string>();
                NewMenuSaver.AddToHiddenTiles(user, company, appName, groupName, menuName, tileName);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("removeFromHiddenTiles")]
        public IActionResult RemoveFromHiddenTiles([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string appName = value["application"]?.Value<string>();
                string groupName = value["group"]?.Value<string>();
                string menuName = value["menu"]?.Value<string>();
                string tileName = value["tile"]?.Value<string>();
                NewMenuSaver.RemoveFromHiddenTiles(user, company, appName, groupName, menuName, tileName);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };

            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }
        //---------------------------------------------------------------------
        [Route("removeAllHiddenTiles")]
        public IActionResult RemoveAllHiddenTiles([FromBody] JObject value)
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                NewMenuSaver.RemoveAllHiddenTiles(user, company);
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };

            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }

        //---------------------------------------------------------------------
        [Route("updateCachedDateAndSave")]
        public IActionResult UpdateCachedDateAndSave()
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    return new ContentResult { StatusCode = 401, Content = "missing authentication token", ContentType = "text/plain" };

                //BasePathFinder.BasePathFinderInstance.ResetApplicationsInfo();
                PathFinder.PathFinderInstance.RefreshEasyStudioApps(TaskBuilderNetCore.Interfaces.ApplicationType.Customization);
                PathFinder.PathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
                return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
            }
            catch (Exception e)
            {
                return new ContentResult { StatusCode = 502, Content = e.Message, ContentType = "text/plain" };
            }
        }
    }
}


