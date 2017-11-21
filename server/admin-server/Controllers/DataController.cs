using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using Microarea.AdminServer.Services.PostMan;
using Microarea.AdminServer.Services.PostMan.actuators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			Task<string> responseData = SecurityManager.ValidatePermission(authHeader, this.httpHelper, this.GWAMUrl);

			if (responseData.Status == TaskStatus.Faulted)
			{
				opRes.Result = false;
				opRes.Message = "Permission token cannot be verified, operation aborted.";
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			OperationResult validateRes = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);

			if (!validateRes.Result)
			{
				opRes.Result = false;
				opRes.Message = "Invalid permission token, operation aborted.";
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// authorization ok

			string rowName;
			JToken rowValue;

			List<IModelObject> modelList = new List<IModelObject>();

			foreach (var c in ((Newtonsoft.Json.Linq.JObject)dataCluster).Children())
			{
				// ((Newtonsoft.Json.Linq.JProperty)r).Value
				rowName = ((Newtonsoft.Json.Linq.JProperty)c).Name;
				rowValue = ((Newtonsoft.Json.Linq.JProperty)c).Value;
				modelList.Add(GetItemByName(rowName, rowValue));
			}

			modelList.ForEach(modelItem =>
			{
				modelItem.Save(this.burgerData);
			});

			jsonHelper.AddJsonCouple<string>("res", "save cluster API is active");
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
        }

		//--------------------------------------------------------------------------------
		IModelObject GetItemByName(string name, JToken jToken)
		{
			switch (name)
			{
				case "accounts":
					return jToken.ToObject<Account>();

				case "instance":
					return jToken.ToObject<Instance>();

				case "subscriptionAccount":
					return jToken.ToObject<SubscriptionAccount>();

				case "subscriptionInstances":
					return jToken.ToObject<SubscriptionInstance>();

				case "subscriptions":
					return jToken.ToObject<Subscription>();

				default:
					return null;
			}
		}
    }
}
