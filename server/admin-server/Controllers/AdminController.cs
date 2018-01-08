using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.APIQuery;
using Microarea.AdminServer.Controllers.Helpers.Commons;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Model.Services.Queries;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using Microarea.AdminServer.Services.PostMan;
using Microarea.AdminServer.Services.PostMan.actuators;
using Microarea.AdminServer.Services.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers
{
	//=========================================================================
	public class AdminController : Controller
    {
        AppOptions _settings;
        private IHostingEnvironment _env;

		BurgerData burgerData;
		PostMan postMan;
		IPostManActuator mailActuator;

		IJsonHelper jsonHelper;
		IHttpHelper _httpHelper;

		string GWAMUrl;

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper, IHttpHelper httpHelper)
        {
			// configurations
            this._env = env;
            this._settings = settings.Value;
			this.GWAMUrl = _settings.ExternalUrls.GWAMUrl;

			// helpers
			this.jsonHelper = jsonHelper;
			this._httpHelper = httpHelper;

			// services
			this.burgerData = new BurgerData(_settings.DatabaseInfo.ConnectionString);
			this.mailActuator = new MailActuator("mail.microarea.it");
			this.postMan = new PostMan(mailActuator);
		}

		[HttpGet]
        [Route("/")]
		//-----------------------------------------------------------------------------	
		public IActionResult Index()
        {
            if (_env.WebRootPath == null)
            {
				jsonHelper.AddJsonCouple<string>("message", "wwwroot is null");
				return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
            }

            string file = Path.Combine(_env.WebRootPath, "index.html");

            if (!System.IO.File.Exists(file))
            {
				jsonHelper.AddJsonCouple<string>("message", "index.html doesn't exist");
				return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

            byte[] buff = System.IO.File.ReadAllBytes(file);
            return File(buff, "text/html");
        }

        [HttpGet]
        [Route("api")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiHome()
        {
            jsonHelper.AddJsonCouple<string>("message", "Welcome to Microarea Admin-Server API");
            return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
        }

        /// <summary>
        /// Returns all subscriptions of a specific Instance
        /// </summary>
        /// <param name="instanceKey"></param>
        /// <returns></returns>
        //-----------------------------------------------------------------------------	
        [HttpGet("/api/subscriptions/{instanceKey}")]
		[Produces("application/json")]
		public IActionResult ApiGetSubscriptionsByInstance(string instanceKey)
		{
			if (string.IsNullOrWhiteSpace(instanceKey))
			{
				jsonHelper.AddJsonCouple<bool>("result", false);
				jsonHelper.AddJsonCouple<string>("message", Strings.InstanceKeyEmpty);
				return new ContentResult { StatusCode = 501, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first			
			OperationResult opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, instanceKey, RoleLevelsStrings.Instance);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			List<ISubscription> subscriptionsList = new List<ISubscription>();

			try
			{
				subscriptionsList = this.burgerData.GetList<Subscription, ISubscription>(
					String.Format(Queries.SelectSubscriptionAccountBySubscriptionKey, instanceKey),
					ModelTables.Subscriptions);
			}
			catch (Exception exc)
			{
				jsonHelper.AddJsonCouple<bool>("result", false);
				jsonHelper.AddJsonCouple<string>("message", "010 AdminCOntroller.ApiGetSubscriptions" + exc.Message);
				return new ContentResult { StatusCode = 501, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (subscriptionsList == null)
			{
				jsonHelper.AddJsonCouple<bool>("result", false);
				jsonHelper.AddJsonCouple<string>("message", Strings.InvalidUser);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			jsonHelper.AddPlainObject<List<ISubscription>>(subscriptionsList);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

        [HttpPost("/api/query/{modelName}/{instanceKey?}")]
        [Produces("application/json")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiQuery(string modelName, string instanceKey, [FromBody] APIQueryData apiQueryData)
		{
			OperationResult opRes = new OperationResult();

			if (string.IsNullOrWhiteSpace(modelName))
			{
				opRes.Result = false;
				opRes.Message = Strings.EmptyModelName;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (apiQueryData == null ||
				(apiQueryData.MatchingFields.Count == 0 && apiQueryData.LikeFields.Count == 0))
			{
				opRes.Result = false;
				opRes.Message = Strings.MissingBody;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			ModelTables modelTable = SqlScriptManager.GetModelTable(modelName);

			if (modelTable == ModelTables.None)
			{
				opRes.Result = false;
				opRes.Message = Strings.UnknownModelName;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// check AuthorizationHeader first
			string authHeader = HttpContext.Request.Headers["Authorization"];

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, instanceKey, RoleLevelsStrings.Instance);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				opRes = APIQueryHelper.Query(modelTable, apiQueryData, this.burgerData);

				if (opRes.Result)
				{
					opRes.Message = Strings.OperationOK;
					opRes.Code = (int)AppReturnCodes.OK;
				}
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Message = "012 AdminController.ApiQuery" + e.Message;
				opRes.Code = (int)AppReturnCodes.ExceptionOccurred;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 501, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpDelete("/api/query/{modelName}/{instanceKey}")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiQueryDelete(string modelName, string instanceKey,[FromBody] APIQueryData apiQueryData)
		{
			OperationResult opRes = new OperationResult();

			if (string.IsNullOrWhiteSpace(modelName))
			{
				opRes.Result = false;
				opRes.Message = Strings.EmptyModelName;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (apiQueryData == null ||
				(apiQueryData.MatchingFields.Count == 0 && apiQueryData.LikeFields.Count == 0))
			{
				opRes.Result = false;
				opRes.Message = Strings.MissingBody;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			ModelTables modelTable = SqlScriptManager.GetModelTable(modelName);

			if (modelTable == ModelTables.None)
			{
				opRes.Result = false;
				opRes.Message = Strings.UnknownModelName;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// checking authorization

			string authHeader = HttpContext.Request.Headers["Authorization"];

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, instanceKey, RoleLevelsStrings.Instance);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			bool queryResult;

			try
			{
				DeleteScript deleteScript = new DeleteScript(SqlScriptManager.GetTableName(modelTable));
				deleteScript.LogicOperatorForAllParameters = SqlLogicOperators.AND;

				foreach (KeyValuePair<string, string> kvp in apiQueryData.MatchingFields)
				{
					deleteScript.Add(kvp.Key, kvp.Value, QueryComparingOperators.IsEqual, false);
				}

				queryResult = this.burgerData.ExecuteNoResultsQuery(deleteScript.GetParameterizedQuery(), deleteScript.SqlParameterList);
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Message = String.Format(Strings.ExceptionMessage, e.Message);
				opRes.Code = (int)AppReturnCodes.ExceptionOccurred;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (!queryResult)
			{
				opRes.Result = false;
				opRes.Message = String.Format(Strings.OperationKO, "BurgerData delete query failed.");
				opRes.Code = (int)AppReturnCodes.InternalError;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// query executed without exception

			opRes.Result = true;
			opRes.Message = Strings.OperationOK;
			opRes.Code = (int)AppReturnCodes.OK;

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpPost("/api/messages/{instanceKey}")]
		[Produces("application/json")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiMessages(string instanceKey, [FromBody] APIMessageData apiMessageData)
		{
			OperationResult opRes = new OperationResult();

			// check AuthorizationHeader first
			string authHeader = HttpContext.Request.Headers["Authorization"];

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, instanceKey, RoleLevelsStrings.Instance);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (apiMessageData == null || !apiMessageData.HasData())
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 400, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				this.postMan.Send(apiMessageData.Destination, apiMessageData.Subject, apiMessageData.Body);
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Message = String.Concat(Strings.InternalError, " (", e.Message, ")");
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			opRes.Result = true;
			opRes.Message = Strings.OK;
			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpPost("api/instances")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiInstanceRegistration([FromBody]Instance instance)
		{
			OperationResult opRes = new OperationResult();

			// checking small things before

			if (String.IsNullOrWhiteSpace(instance.InstanceKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = -1;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 400, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// now we check authorization

			string authHeader = HttpContext.Request.Headers["Authorization"];

			Task<string> responseData = SecurityManager.ValidatePermission(authHeader, this._httpHelper, this.GWAMUrl);

			if (responseData.Status == TaskStatus.Faulted)
			{
				opRes.Result = false;
				opRes.Message = Strings.InvalidCredentials;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			OperationResult validateRes = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);

			if (!validateRes.Result)
			{
				opRes.Result = false;
				opRes.Message = Strings.InvalidCredentials;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 403, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				opRes = instance.Save(this.burgerData);
			}
			catch (Exception exc)
			{
				opRes.Result = false;
				opRes.Message = "An error occurred " + exc.Message;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			int status;

			if (opRes.Result)
			{
				opRes.Result = true;
				opRes.Message = Strings.OK;
				status = 201;
			}
			else
			{
				opRes.Result = false;
				opRes.Message = Strings.OperationKO;
				status = 200;
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = status, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpGet("/api/startup")]
		[Produces("application/json")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiStartup()
		{
			OperationResult opRes = new OperationResult();
			List<IInstance> localInstances;

			try
			{
				localInstances = this.burgerData.GetList<Instance, IInstance>(Queries.SelectInstanceAll, ModelTables.Instances);
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Message = String.Concat("Startup API ended with an error ", Strings.InternalError, " (", e.Message, ")");
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			bool instancesAvailable = localInstances.Count > 0;
			opRes.Result = instancesAvailable;
			opRes.Content = localInstances.Count;

			if (instancesAvailable)
			{
				// at least one instance exists, the system is ready to work
				opRes.Message = "M4 Provisioning System is ready to go.";

			} else
			{
				// no istances exist, the system should propose to init an instance
				opRes.Message = "No application instances have been registered.";
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

	}
}
