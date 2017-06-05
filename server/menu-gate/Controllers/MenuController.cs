using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microarea.MenuGate.Models;
using Microarea.Common.MenuLoader;

namespace Microarea.Menu.Controllers
{
	//da ripristinare quando inserisce il nuovo menu nel cef
	[Route("menu-gate")]
    public class MenuController : Controller
    {
		//da modificare quando inserisce il nuovo menu nel cef
		[Route("tb/menu/getInstallationInfo/")]
		[HttpPost]
		public IActionResult GetInstallationInfo()
		{
			return new ObjectResult(new InstallationInfo { desktop = false });
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


		//[Route("getProductInfo")]
		//public IActionResult GetProductInfo()
		//{
		//	string user = HttpContext.Request.Form["user"];
		//	string company = HttpContext.Request.Form["company"];
		//	string authtoken = HttpContext.Request.Form["token"];

		//	string content = NewMenuLoader.GetJsonProductInfo(user, company, authtoken);
		//	return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		//}


		[Route("getPreferences")]
		public IActionResult GetPreferences()
		{
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			string authtoken = HttpContext.Request.Form["token"];

			string content = NewMenuLoader.GetPreferencesAsJson(user, company);
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}

		[Route("setPreferences")]
		public IActionResult SetPreferences()
		{
			string user = HttpContext.Request.Form["user"];
			string company = HttpContext.Request.Form["company"];
			string preferenceName = HttpContext.Request.Form["name"];
			string preferenceValue = HttpContext.Request.Form["value"];
			
			bool result = NewMenuSaver.SetPreference(preferenceName, preferenceValue,  user, company);
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



	}
}
