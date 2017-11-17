using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Services.BurgerData;
using Microarea.AdminServer.Services.PostMan;
using Microarea.AdminServer.Services.PostMan.actuators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Microarea.AdminServer.Controllers
{
	//================================================================================
	[Produces("application/json")]
    public class DataController : Controller
    {
		AppOptions settings;
		private IHostingEnvironment env;

		BurgerData burgerData;
		PostMan postMan;
		IPostManActuator mailActuator;

		IJsonHelper jsonHelper;
		IHttpHelper httpHelper;

		//--------------------------------------------------------------------------------
		public DataController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper, IHttpHelper httpHelper)
		{
			// configurations
			this.env = env;
			this.settings = settings.Value;

			// helpers
			this.jsonHelper = jsonHelper;
			this.httpHelper = httpHelper;

			// services
			this.burgerData = new BurgerData(this.settings.DatabaseInfo.ConnectionString);
			this.mailActuator = new MailActuator("mail.microarea.it");
			this.postMan = new PostMan(mailActuator);
		}

		//--------------------------------------------------------------------------------
		[HttpPost("/api/savecluster")]
        public IActionResult Post([FromBody]object dataCluster)
        {
			jsonHelper.AddJsonCouple<string>("res", "save cluster API is active");
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
        }
    }
}
