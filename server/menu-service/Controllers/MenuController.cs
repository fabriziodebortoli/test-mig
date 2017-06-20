using Microsoft.AspNetCore.Mvc;
using Microarea.MenuGate.Models;
using Microarea.Common.MenuLoader;
using Microarea.TaskBuilderNet.Core.Generic;
using System.IO;
using Microarea.Common.NameSolver;
using System;

namespace Microarea.Menu.Controllers
{
	//da ripristinare quando inserisce il nuovo menu nel cef
	[Route("menu-service")]
	public class MenuController : Controller
	{
		public MenuController()
		{
		}

		[Route("getMenuElements")]
		public IActionResult GetMenuElements()
		{
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			string authtoken = HttpContext.Request.Form["token"];

			string content = NewMenuLoader.LoadMenuWithFavoritesAsJson(user, company, authtoken);
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}

		[Route("getPreferences")]
		public IActionResult GetPreferences()
		{
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			string authtoken = HttpContext.Request.Form["token"];

			string content = NewMenuLoader.GetPreferencesAsJson(user, company);
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}

		[Route("setPreference")]
		public IActionResult SetPreference()
		{
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			string preferenceName = HttpContext.Request.Form["name"];
			string preferenceValue = HttpContext.Request.Form["value"];

			bool result = NewMenuSaver.SetPreference(preferenceName, preferenceValue, user, company);
			return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
		}

		[Route("getThemedSettings")]
		public IActionResult GetThemedSettings()
		{
			string token = HttpContext.Request.Form["token"];
			string content = NewMenuLoader.GetJsonMenuSettings(token);
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}

		[Route("getConnectionInfo")]
		public IActionResult GetConnectionInfo()
		{
			string token = HttpContext.Request.Form["token"];
			string content = NewMenuLoader.GetConnectionInformation(token);
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}

		[Route("favoriteObject")]
		public IActionResult FavoriteObject()
		{
			string target = HttpContext.Request.Form["target"];
			string objectType = HttpContext.Request.Form["objectType"];
			string objectName = HttpContext.Request.Form["objectName"];
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			NewMenuSaver.AddToFavorites(target, objectType, objectName, user, company);
			return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
		}

		[Route("unFavoriteObject")]
		public IActionResult UnFavoriteObject()
		{
			string target = HttpContext.Request.Form["target"];
			string objectType = HttpContext.Request.Form["objectType"];
			string objectName = HttpContext.Request.Form["objectName"];
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			NewMenuSaver.RemoveFromFavorites(target, objectType, objectName, user, company);
			return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
		}

		[Route("clearAllMostUsed")]
		public IActionResult ClearAllMostUsed()
		{
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			NewMenuSaver.ClearMostUsed(user, company);
			return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
		}

		[Route("getMostUsedShowNr")]
		public IActionResult GetMostUsedShowNr()
		{
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			string content = NewMenuLoader.GetMostUsedShowNrElements(user, company);
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}

		[Route("addToMostUsed")]
		public IActionResult AddToMostUsed()
		{
			string target = HttpContext.Request.Form["target"];
			string objectType = HttpContext.Request.Form["objectType"];
			string objectName = HttpContext.Request.Form["objectName"];
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			NewMenuSaver.AddToMostUsed(target, objectType, objectName, user, company);
			return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
		}


		[Route("removeFromMostUsed")]
		public IActionResult RemoveFromMostUsed()
		{
			string target = HttpContext.Request.Form["target"];
			string objectType = HttpContext.Request.Form["objectType"];
			string objectName = HttpContext.Request.Form["objectName"];
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			NewMenuSaver.RemoveFromMostUsed(target, objectType, objectName, user, company);
			return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
		}

		[Route("clearCachedData")]
		public IActionResult ClearCachedData()
		{
			string user = HttpContext.Request.Form["user"];
			Microarea.Common.Generic.InstallationInfo.Functions.ClearCachedData(user);
			return new ContentResult { StatusCode = 200, Content = "", ContentType = "application/json" };
		}

		[Route("getLocalizedElements")]
		public IActionResult GetLocalizedElements()
		{
			string token = HttpContext.Request.Form["token"];
			string needLoginThread = HttpContext.Request.Form["needLoginThread"];

			string json = NewMenuLoader.GetLocalizationJson(token);
			return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
		}


		[Route("getProductInfo")]
		public IActionResult GetProductInfo()
		{
			string token = HttpContext.Request.Form["token"];
			string json = NewMenuLoader.GetJsonProductInfo(token);
			return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
		}

		[Route("getPingViaSMSUrl")]
		public IActionResult GetPingViaSMSUrl()
		{
			string url = MenuStaticFunctions.PingViaSMSUrl();
			string json = string.Format("{{ \"url\": \"{0}\" }}", url);
			return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
		}

		[Route("getProducerSite")]
		public IActionResult GetProducerSite()
		{
			string url = MenuStaticFunctions.ProducerSiteUrl();
			string json = string.Format("{{ \"url\": \"{0}\" }}", url);
			return new ContentResult { StatusCode = 200, Content = json, ContentType = "application/json" };
		}

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
	}
}


