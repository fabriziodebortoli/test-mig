using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Microarea.LoginManager.Controllers
{
	[Route("login-manager")]
	public class LoginManagerController : Controller
    {
		[Route("index")]
		public IActionResult Index()
		{
			return new ObjectResult("TBLoader Gate default page");
		}

       
    }
}
