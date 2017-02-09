using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Microarea.DatabaseService.Controllers
{
	[Route("database-manager")]
	//============================================================================
	public class DatabaseManagerController : Controller
	{
		[Route("create-database")]
		//---------------------------------------------------------------------
		public IActionResult CreateDatabase()
		{
			/*string server = "USR-DELBENEMIC";
			string database = HttpContext.Request.Form["database"];
			string user = "sa";
			string password = "14";*/

			return new ContentResult { Content = "", ContentType = "application/json" };
		}
	}
}