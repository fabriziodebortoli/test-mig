using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microarea.MenuGate.Models;

namespace Microarea.Menu.Controllers
{
	//da ripristinare quando inserisce il nuovo menu nel cef
	//[Route("menu-gate")]
    public class MenuController : Controller
    {
		//da modificare quando inserisce il nuovo menu nel cef
		[Route("tb/menu/getInstallationInfo/")]
		[HttpPost]
		public IActionResult GetInstallationInfo()
		{
			return new ObjectResult(new InstallationInfo { desktop = false });
		}
	}
}
