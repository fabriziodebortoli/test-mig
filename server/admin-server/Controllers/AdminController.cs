using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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

        IJsonHelper _jsonHelper;
		IHttpHelper _httpHelper;

		string GWAMUrl;

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper, IHttpHelper httpHelper)
        {
            _env = env;
            _settings = settings.Value;
            _jsonHelper = jsonHelper;
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
            _jsonHelper.AddJsonCouple<string>("message", "Welcome to Microarea Admin-Server API");
            return new ContentResult { Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
        }

		/// <summary>
		/// Insert/update subscription database
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/databases")]
		public IActionResult ApiDatabases([FromBody] SubscriptionDatabase subDatabase)
		{
			if (String.IsNullOrEmpty(subDatabase.SubscriptionKey))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", Strings.SubscriptionKeyEmpty);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (String.IsNullOrEmpty(subDatabase.Name))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Database name empty");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			OperationResult opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subDatabase.SubscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
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
				_jsonHelper.AddJsonCouple<bool>("result", opRes.Result);
				_jsonHelper.AddJsonCouple<string>("message", "040 AdminController.ApiDatabases" + exc.Message);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (!opRes.Result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", opRes.Result);
				_jsonHelper.AddJsonCouple<string>("message", Strings.OperationKO);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Returns the databases list of a subscriptionKey
		/// You can also specify a database name
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="dbName"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------	
		[HttpGet("/api/databases/{subscriptionKey}/{dbName?}")]
		[Produces("application/json")]
		public IActionResult ApiGetDatabasesBySubscription(string subscriptionKey, string dbName)
		{
			if (string.IsNullOrWhiteSpace(subscriptionKey))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", Strings.SubscriptionKeyEmpty);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];
			
			// check AuthorizationHeader first

			OperationResult opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			List<SubscriptionDatabase> databasesList = null;

			try
			{
				databasesList = GetDatabasesBySubscription(subscriptionKey, dbName);
			}
			catch (Exception exc)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "010 AdminController.ApiGetDatabasesBySubscription" + exc.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (databasesList == null)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", Strings.InvalidAccountName);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<List<SubscriptionDatabase>>(databasesList);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

        //-----------------------------------------------------------------------------	
        private List<SubscriptionDatabase> GetDatabasesBySubscription(string subscriptionKey, string dbName)
        {
            throw new NotImplementedException();
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
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", Strings.InstanceKeyEmpty);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first			
			OperationResult opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, instanceKey, RoleLevelsStrings.Instance);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
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
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "010 AdminCOntroller.ApiGetSubscriptions" + exc.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (subscriptionsList == null)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", Strings.InvalidUser);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<List<ISubscription>>(subscriptionsList);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}
        [HttpPost("/api/query/{modelName}")]
        [Produces("application/json")]
       
        //-----------------------------------------------------------------------------	
        private OperationResult Query(ModelTables modelTable, string bodyText)
        {
            // load Body data in QueryInfo object
            JObject jObject = JObject.Parse(bodyText);
            SelectScript selectScript = new SelectScript(SqlScriptManager.GetTableName(modelTable));

            foreach (var item in jObject)
            {
                selectScript.AddWhereParameter(item.Key, item.Value, QueryComparingOperators.IsEqual, false);
            }

            OperationResult opRes = new OperationResult();
            switch (modelTable)
            {
                case ModelTables.Accounts:
                    opRes.Result = true;
                    opRes.Content = this.burgerData.GetList<Account, IAccount>(selectScript.ToString(), modelTable);
                    break;
                case ModelTables.Subscriptions:
                    break;
                case ModelTables.Roles:
                    break;
                case ModelTables.AccountRoles:
                    break;
                case ModelTables.RegisteredApps:
                    break;
                case ModelTables.Instances:
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



        [HttpPost("/api/query/{modelName}")]
		[Produces("application/json")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiQuery(string modelName)
		{
			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			// for now, we set the highest rights for this API

			OperationResult opRes = SecurityManager.ValidateAuthorization(
				authHeader, _settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, RolesStrings.All, RoleLevelsStrings.Instance);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (string.IsNullOrWhiteSpace(modelName))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", Strings.EmptyModelName);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}
            ModelTables modelTable = SqlScriptManager.GetModelTable(modelName);

            if (modelTable == ModelTables.None)
            {
                opRes.Result = false;
                opRes.Message = Strings.UnknownModelName;
                _jsonHelper.AddPlainObject<OperationResult>(opRes);
                return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
            }
            try
            {
                // read Body content
                var bodyStream = new StreamReader(HttpContext.Request.Body);
                string bodyText = bodyStream.ReadToEnd();

                if (string.IsNullOrWhiteSpace(bodyText))
                {
                    opRes.Result = false;
                    opRes.Message = Strings.MissingBody;
                    _jsonHelper.AddPlainObject<OperationResult>(opRes);
                    return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                }

                Query(modelTable, bodyText);

                if (opRes.Result)
                {
                    opRes.Message = Strings.OperationOK;
                    opRes.Code = (int)AppReturnCodes.OK;
                }
            }
          
            catch (Exception e)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "010 AdminController.ApiQuery " + e.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}
	}
}
