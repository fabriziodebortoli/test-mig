using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.All;
using Microarea.AdminServer.Controllers.Helpers.DataController;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using Microarea.AdminServer.Services.PostMan;
using Microarea.AdminServer.Services.PostMan.actuators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		string GWAMUrl;

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
			this.GWAMUrl = this.settings.ExternalUrls.GWAMUrl;
		}

		//--------------------------------------------------------------------------------
		[HttpPost("/api/savecluster")]
        public IActionResult Post([FromBody] dynamic dataCluster)
        {
			// now we check authorization

			string authHeader = HttpContext.Request.Headers["Authorization"];
			OperationResult opRes = AuthorizationHelper.VerifyPermissionOnGWAM(authHeader, this.httpHelper, this.GWAMUrl);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// now we produce the list of items that belong to the data cluster, and save it

			List<IModelObject> modelList = DataControllerHelper.GetModelListFromCluster(dataCluster);

			OperationResult saveResult = new OperationResult();
			Dictionary<string, bool> saveLog = new Dictionary<string, bool>();

			modelList.ForEach(modelItem =>
			{
				saveResult = modelItem.Save(this.burgerData);
				saveLog.Add(modelItem.GetHashCode() + ": " + saveResult.Message, saveResult.Result);
			});

			jsonHelper.AddJsonCouple<string>("result", "API.savecluster completed");
			jsonHelper.AddJsonCouple<Dictionary<string, bool>>("operationLog", saveLog);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
        }
    }
}
