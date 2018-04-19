﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

using Microarea.Common;
using Microarea.Common.Applications;
using Microarea.Common.Generic;
using Microarea.Common.MenuLoader;
using Microarea.Common.NameSolver;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Collections.Generic;

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
                long.TryParse(storageMenuDate, out long nStorageMenudate);
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(nStorageMenudate);
                DateTime dateTime = dateTimeOffset.UtcDateTime;
                bool isTooOld = NewMenuLoader.IsOldMenuFile(user, company, dateTime, authtoken);

                return new SuccessResult(isTooOld.ToJson());
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
            }
        }

        //---------------------------------------------------------------------
        [Route("getThemes")]
        public IActionResult GetThemes([FromBody] JObject value)
        {
            try
            {
                string json =  DefaultTheme.GetAllThemesJson();
                return new SuccessResult(json);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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

                string content = NewMenuLoader.LoadMenuWithFavoritesAsJson(user, company, authtoken, true);
               
                return new SuccessResult (content);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                return new SuccessResult(content);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                string content = NewMenuLoader.GetJsonMenuSettings(authtoken);
                return new SuccessResult(content);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                string content = NewMenuLoader.GetConnectionInformation(authtoken);
                return new SuccessResult(content);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                NewMenuSaver.ClearMostUsed(user, company);

                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                return new SuccessResult(content);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string favorites = value["favorites"]?.Value<string>();
                NewMenuSaver.UpdateFavorites(favorites, user, company);
                
                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string mostUsed = value["mostUsed"]?.Value<string>();
                NewMenuSaver.UpdateMostUsed(mostUsed, user, company);
                
                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
            }
        }

        //---------------------------------------------------------------------
        [Route("getProductInfo")]
        public IActionResult GetProductInfo()
        {
            try
            {
                string authtoken = AutorizationHeaderManager.GetAuthorizationElement(HttpContext.Request, UserInfo.AuthenticationTokenKey);
                if (string.IsNullOrEmpty(authtoken))
                    authtoken = HttpContext.Request.Form[UserInfo.AuthenticationTokenKey];

                //potrebbe arrivarmi vuoto, se non sono ancora connesso, allora ritorno solo informazioni parziali
                string json = NewMenuLoader.GetJsonProductInfo(authtoken);

                return new SuccessResult(json);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                
                return new SuccessResult(json);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                
                return new SuccessResult(json);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
            }
        }

        //---------------------------------------------------------------------
        [Route("getStaticImage/{imageFile?}")]
        public IActionResult getStaticImage(string imageFile)
        {
            string fullImagePath = Path.Combine(PathFinder.PathFinderInstance.GetStandardPath, imageFile);

            if (!PathFinder.PathFinderInstance.ExistFile(fullImagePath))
                return new FileNotFoundResult("File does not exists: " + fullImagePath);

            string ext = System.IO.Path.GetExtension(fullImagePath);

            try
            {
                Stream f = PathFinder.PathFinderInstance.GetStream(fullImagePath, false);

                return new FileStreamResult(f, "image/" + ext);
            }
            catch (Exception)
            {
            }

            return new ForbiddenResult("Cannot access file: " + fullImagePath);
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

                return new SuccessResult(json);
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string appName = value["application"]?.Value<string>();
                string groupName = value["group"]?.Value<string>();
                string menuName = value["menu"]?.Value<string>();
                string tileName = value["tile"]?.Value<string>();
                NewMenuSaver.AddToHiddenTiles(user, company, appName, groupName, menuName, tileName);
                
                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                string appName = value["application"]?.Value<string>();
                string groupName = value["group"]?.Value<string>();
                string menuName = value["menu"]?.Value<string>();
                string tileName = value["tile"]?.Value<string>();
                NewMenuSaver.RemoveFromHiddenTiles(user, company, appName, groupName, menuName, tileName);
                
                return new SuccessResult();

            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                string user = value["user"]?.Value<string>();
                string company = value["company"]?.Value<string>();
                NewMenuSaver.RemoveAllHiddenTiles(user, company);
                
                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
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
                    return new NoAuthResult("missing authentication token");

                //BasePathFinder.BasePathFinderInstance.ResetApplicationsInfo();
                PathFinder.PathFinderInstance.RefreshEasyStudioApps(TaskBuilderNetCore.Interfaces.ApplicationType.Customization);
                PathFinder.PathFinderInstance.InstallationVer.UpdateCachedDateAndSave();

                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new ErrorResult(e.Message);
            }
        }
    }
}


