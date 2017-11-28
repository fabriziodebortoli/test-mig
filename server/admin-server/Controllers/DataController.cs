using Microarea.AdminServer.Controllers.Helpers.All;
using Microarea.AdminServer.Controllers.Helpers.DataController;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Microarea.AdminServer.Controllers
{
	//================================================================================
	[Produces("application/json")]
    public class DataController : Controller
    {
		AppOptions settings;
		private IHostingEnvironment env;

		BurgerData burgerData;
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

			OperationResult saveClusterResult = DataControllerHelper.SaveCluster(modelList, this.burgerData);

			jsonHelper.AddPlainObject<OperationResult>(saveClusterResult);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
        }
    }
}
