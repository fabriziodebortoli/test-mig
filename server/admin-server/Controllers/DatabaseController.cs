using Microarea.AdminServer.Controllers.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microarea.AdminServer.Libraries.DatabaseManager;
using Microsoft.AspNetCore.Hosting;

namespace Microarea.AdminServer.Controllers
{
	// Controller with APIs against database 
	// create container
	// create structure with tables

	//============================================================================
	public class DatabaseController : Controller
	{
		private IHostingEnvironment environment;
		private IJsonHelper jsonHelper;

		//---------------------------------------------------------------------
		public DatabaseController(IHostingEnvironment env, IJsonHelper jsonHelper)
		{
			this.environment = env;
			this.jsonHelper = jsonHelper;
		}

		/// <summary>
		/// Create database container
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("/api/database/create/{dbname}")]
		public IActionResult CreateDatabase(string dbname)
		{
			/*string server = "USR-DELBENEMIC";
			string database = HttpContext.Request.Form["database"];
			string user = "sa";
			string password = "14";*/

			AzureCreateDBParameters param = new AzureCreateDBParameters();
			param.DatabaseName = dbname;
			param.MaxSize = AzureMaxSize.GB1;
			DatabaseTask dTask = new DatabaseTask();
			dTask.CurrentStringConnection = "Data Source=microarea.database.windows.net;Initial Catalog='ProvisioningDB';User ID='AdminMicroarea';Password='S1cr04$34!';Connect Timeout=30;Pooling=false;";
			bool res = dTask.CreateAzureDatabase(param);

			jsonHelper.AddJsonCouple<string>("message", res ? string.Format("Database {0} successfully created", dbname) : string.Format("Database {0} creation ended with errors", dbname));
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
		}
	}
}