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
	}
}
