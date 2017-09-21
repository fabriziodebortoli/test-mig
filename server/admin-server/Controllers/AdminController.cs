using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
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

        IJsonHelper jsonHelper;
		IHttpHelper _httpHelper;

		string GWAMUrl;

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper, IHttpHelper httpHelper)
        {
            _env = env;
            _settings = settings.Value;
            this.jsonHelper = jsonHelper;
			_httpHelper = httpHelper;

			this.GWAMUrl = _settings.ExternalUrls.GWAMUrl;

			this.burgerData = new BurgerData(_settings.DatabaseInfo.ConnectionString);
		}

		[HttpGet]
        [Route("/")]
		//-----------------------------------------------------------------------------	
		public IActionResult Index()
        {
            if (_env.WebRootPath == null)
            {
                return NotFound();
            }

            string file = Path.Combine(_env.WebRootPath, "index.html");

            if (!System.IO.File.Exists(file))
            {
                return NotFound();
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
		/// Insert/update subscription database
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/databases")]
		public IActionResult ApiDatabases([FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (string.IsNullOrWhiteSpace(subDatabase.InstanceKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.InstanceKeyEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (String.IsNullOrEmpty(subDatabase.SubscriptionKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.SubscriptionKeyEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (String.IsNullOrEmpty(subDatabase.Name))
			{
				opRes.Result = false;
				opRes.Message = Strings.DatabaseNameEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subDatabase.SubscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				if (subDatabase != null)
				{
					opRes = subDatabase.Save(burgerData);
					opRes.Message = Strings.OperationOK;
				}
				else
				{
					opRes.Result = false;
					opRes.Message = Strings.NoValidInput;
					opRes.Code = (int)AppReturnCodes.InvalidData;
				}
			}
			catch (Exception exc)
			{
				opRes.Result = false;
				opRes.Message = "040 AdminController.ApiDatabases" + exc.Message;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Returns the databases list of a instanceKey+subscriptionKey
		/// You can also specify a database name (it is the third pk segment)
		/// </summary>
		/// <param name="instanceKey"></param>
		/// <param name="subscriptionKey"></param>
		/// <param name="dbName"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------	
		[HttpGet("/api/databases/{instanceKey}/{subscriptionKey}/{dbName?}")]
		[Produces("application/json")]
		public IActionResult ApiGetDatabases(string instanceKey, string subscriptionKey, string dbName)
		{
			OperationResult opRes = new OperationResult();

			if (string.IsNullOrWhiteSpace(instanceKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.InstanceKeyEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (string.IsNullOrWhiteSpace(subscriptionKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.SubscriptionKeyEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];
			
			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			List<ISubscriptionDatabase> databasesList = new List<ISubscriptionDatabase>();

			try
			{
				databasesList = string.IsNullOrWhiteSpace(dbName) ?
					this.burgerData.GetList<SubscriptionDatabase, ISubscriptionDatabase>(string.Format(Queries.SelectDatabases, instanceKey, subscriptionKey), ModelTables.SubscriptionDatabases) :
					this.burgerData.GetList<SubscriptionDatabase, ISubscriptionDatabase>(string.Format(Queries.SelectDatabaseByName, instanceKey, subscriptionKey, dbName), ModelTables.SubscriptionDatabases);
			}
			catch (Exception exc)
			{
				opRes.Result = false;
				opRes.Message = "010 AdminController.ApiGetDatabasesBySubscription" + exc.Message;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 501, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (databasesList.Count == 0)
			{
				opRes.Code = (int)AppReturnCodes.NoSubscriptionDatabasesAvailable;
				opRes.Message = Strings.NoSubscriptionDatabasesAvailable;
			}

			opRes.Result = true;
			opRes.Content = databasesList;
			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
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
				opRes = Query(modelTable, apiQueryData);

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

		//-----------------------------------------------------------------------------	
		private OperationResult Query(ModelTables modelTable, APIQueryData apiQueryData)
		{
			// load Body data in QueryInfo object

			SelectScript selectScript = new SelectScript(SqlScriptManager.GetTableName(modelTable));

			foreach (KeyValuePair<string, string> kvp in apiQueryData.MatchingFields)
			{
				selectScript.AddWhereParameter(kvp.Key, kvp.Value, QueryComparingOperators.IsEqual, false);
			}

			foreach (KeyValuePair<string, string> kvp in apiQueryData.LikeFields)
			{
				selectScript.AddWhereParameter(kvp.Key, kvp.Value, QueryComparingOperators.Like, false);
			}

			OperationResult opRes = new OperationResult();
			opRes.Result = true;

			switch (modelTable)
			{
				case ModelTables.Accounts:
					opRes.Content = this.burgerData.GetList<Account, IAccount>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.Subscriptions:
					opRes.Content = this.burgerData.GetList<Subscription, ISubscription>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.Roles:
					opRes.Content = this.burgerData.GetList<Role, IRole>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.AccountRoles:
					opRes.Content = this.burgerData.GetList<AccountRoles, IAccountRoles>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.Instances:
					opRes.Content = this.burgerData.GetList<Instance, IInstance>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.SubscriptionAccounts:
					opRes.Content = this.burgerData.GetList<SubscriptionAccount, ISubscriptionAccount>(selectScript.GetParameterizedQuery(), modelTable, selectScript.SqlParameterList);
					break;
				case ModelTables.None:
				default:
					opRes.Result = false;
					opRes.Code = (int)AppReturnCodes.UnknownModelName;
					opRes.Message = Strings.UnknownModelName;
					break;
			}

			return opRes;
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

				queryResult = this.burgerData.ExecuteNoResultsQuery(deleteScript.ToString());
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

	}
}
