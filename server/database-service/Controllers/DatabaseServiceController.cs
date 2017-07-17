using Microsoft.AspNetCore.Mvc;
using Microarea.DatabaseService.DatabaseManager;
using Microsoft.AspNetCore.Hosting;

namespace Microarea.DatabaseService.Controllers
{
	//[Route("database-service")]
	[Produces("application/json")]
	//============================================================================
	public class DatabaseServiceController : Controller
	{
		private IHostingEnvironment environment;
		private IJsonHelper jsonHelper;

		//---------------------------------------------------------------------
		public DatabaseServiceController(IHostingEnvironment env, IJsonHelper jsonHelper)
		{
			this.environment = env;
			this.jsonHelper = jsonHelper;
		}

		[HttpGet]
		[Route("/")]
		//-----------------------------------------------------------------------------	
		public IActionResult Index()
		{
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
		}

		[HttpGet]
		[Route("api")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiHome()
		{
			jsonHelper.AddJsonCouple<string>("message", "Welcome to Microarea DatabaseService API");
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
		}

		[Route("create-database")]
		//---------------------------------------------------------------------
		public IActionResult CreateDatabase()
		{
			/*string server = "USR-DELBENEMIC";
			string database = HttpContext.Request.Form["database"];
			string user = "sa";
			string password = "14";*/

			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[Route("test-database")]
		//---------------------------------------------------------------------
		public IActionResult TestDatabase()
		{
			/*string server = "USR-DELBENEMIC";
			string database = HttpContext.Request.Form["database"];
			string user = "sa";
			string password = "14";*/

			DbTester myTester = new DbTester();
			myTester.Run();

			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

	}
}