using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microarea.AdminServer.Controllers.Helpers;
using Microsoft.AspNetCore.Hosting;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Microarea.AdminServer.Controllers
{
    public class TBFSController : Controller
    {
		private IHostingEnvironment _env;
		IJsonHelper jsonHelper;

		//-----------------------------------------------------------------------------	
		public TBFSController(IHostingEnvironment env, IJsonHelper jsonHelper)
		{
			_env = env;
			this.jsonHelper = jsonHelper;
		}

		[HttpPost("api/tbfs/init")]
		//-----------------------------------------------------------------------------	
		public async Task<IActionResult> ApiTBFSInit(string instanceKey, string path)
		{
			jsonHelper.AddJsonCouple<string>("message", "Welcome to Microarea Admin-Server API");
			return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
		}
	}
}
