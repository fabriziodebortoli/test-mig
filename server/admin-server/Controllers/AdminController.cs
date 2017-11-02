using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.APIQuery;
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
using System;
using System.Collections.Generic;
using System.IO;

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

        [HttpPost("/api/query/{modelName}/{instanceKey}")]
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

		[HttpPost("/api/messages")]
		[Produces("application/json")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiMessages([FromBody] APIMessageData apiMessageData, string instanceKey)
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
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

	}
}
