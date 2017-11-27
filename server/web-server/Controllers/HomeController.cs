using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microarea.TbLoaderGate;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServer
{
    public class HomeController : Controller
    {
		private IHostingEnvironment _env = null;

		public HomeController(IHostingEnvironment env)
		{
            this._env = env;
		}
		// GET: api/values
		[HttpGet]
        public IActionResult Index()
        {
			if (_env.WebRootPath == null)
			{
				return NotFound();
			}
			string file = Path.Combine(_env.WebRootPath, "index.html");
			byte[] buff = System.IO.File.ReadAllBytes(file);
			return File(buff, "text/html");
		}

    }
}
