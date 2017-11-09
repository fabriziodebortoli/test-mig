using Microsoft.AspNetCore.Mvc;
using Microarea.Common.MenuLoader;
using Microarea.TaskBuilderNet.Core.Generic;
using System.IO;
using Microarea.Common.NameSolver;
using System;
using Microarea.Common.Generic;
using System.Collections;

namespace Microarea.Menu.Controllers
{
	//da ripristinare quando inserisce il nuovo menu nel cef
	[Route("localization-service")]
	public class LocalizationController : Controller
	{

		//---------------------------------------------------------------------
		[Route("getTranslations")]
		public IActionResult GetTranslations()
		{
			ArrayList list = new ArrayList();
			list.Add(new { Base = "a", Target = "b" });
			return new JsonResult(list);
		}
	}
}


