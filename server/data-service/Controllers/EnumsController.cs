using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microarea.Common.Applications;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DataService.Controllers
{
	[Route("enums-service")]
	public class EnumsController : Controller
	{
		//---------------------------------------------------------------------
		[Route("getEnumsTable")]
		public IActionResult GetEnumsTable(string nameSpace, string selectionType)
		{
			string content = Enums.GetJsonEnumsTable();
			return new ContentResult { StatusCode = 200, Content = content, ContentType = "application/json" };
		}
	}
}
