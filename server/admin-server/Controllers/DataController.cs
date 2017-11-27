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
			// checking authorization

			string authHeader = HttpContext.Request.Headers["Authorization"];
			OperationResult opRes = AuthorizationHelper.VerifyPermissionOnGWAM(authHeader, this.httpHelper, this.GWAMUrl);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// saving the cluster

			// list of model objects created from the data cluster
			List<IModelObject> modelList = DataControllerHelper.GetModelListFromCluster(dataCluster);

			// a list of all saved models. We'll use this list in case we need to rollback the savings
			Stack<IModelObject> savedModelStack = new Stack<IModelObject>();

			// log of each model saving
			Dictionary<string, bool> saveLog = new Dictionary<string, bool>();

			OperationResult saveResult = new OperationResult();
			bool saveClusterResult = true;

			foreach (IModelObject iModel in modelList)
			{
				saveResult = iModel.Save(this.burgerData);
				saveLog.Add(iModel.GetHashCode().ToString() + ": " + saveResult.Message, saveResult.Result);
				
				if (!saveResult.Result)
				{
					saveClusterResult = false;
					break;
				}

				savedModelStack.Push(iModel);
			}

			if (saveClusterResult == false)
			{
				// an error occurred while saving, so proceed to clean all items
				// in savedModelList

				while (savedModelStack.Count > 0)
				{
					savedModelStack.Pop().Delete(this.burgerData);
				}
			}

			jsonHelper.AddJsonCouple<string>("API Save Cluster", "Execution completed. See the following");
			jsonHelper.AddJsonCouple<bool>("savecluster result:", saveClusterResult);
			jsonHelper.AddJsonCouple<Dictionary<string, bool>>("operationLog", saveLog);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
        }
    }
}
