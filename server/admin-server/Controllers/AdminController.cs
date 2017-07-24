using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.Providers;
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

        IDataProvider _accountSqlDataProvider;
        IDataProvider _subscriptionDatabaseSqlDataProvider;
        IDataProvider _instanceSqlDataProvider;
        IDataProvider _subscriptionSQLDataProvider;
        IDataProvider _tokenSQLDataProvider;
        IDataProvider _urlsSQLDataProvider;

        IJsonHelper _jsonHelper;
		IHttpHelper _httpHelper;

		string GWAMUrl;

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper, IHttpHelper httpHelper)
        {
            _env = env;
            _settings = settings.Value;
            _jsonHelper = jsonHelper;
			SqlProviderFactory();
			this.GWAMUrl = _settings.ExternalUrls.GWAMUrl;

			_jsonHelper = jsonHelper;
			_httpHelper = httpHelper;
		}

		//-----------------------------------------------------------------------------	
		private void SqlProviderFactory()
        {
            _accountSqlDataProvider = new AccountSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _subscriptionDatabaseSqlDataProvider = new SubscriptionDatabaseSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
			_instanceSqlDataProvider = new InstanceSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _subscriptionSQLDataProvider = new SubscriptionSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _tokenSQLDataProvider =  new SecurityTokenSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _urlsSQLDataProvider = new ServerURLSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
        }

		/// <summary>
		/// From modelName returns the corresponding IDataProvider 
		/// </summary>
		/// <param name="modelName"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------	
		private IDataProvider GetProviderFromModelName(string modelName)
		{
			switch (modelName.ToLowerInvariant())
			{
				case "account":
					return _accountSqlDataProvider;

				case "subscriptiondatabase":
					return _subscriptionDatabaseSqlDataProvider;

				case "instance":
					return _instanceSqlDataProvider;

				case "subscription":
					return _subscriptionSQLDataProvider;

				default:
					break;
			}

			return null;
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

			OperationResult opRes = SecurityManager.ValidateAuthorization(authHeader, _settings.SecretsKeys.TokenHashingKey/*, isProvisioningAdmin: true*/);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				if (subDatabase != null)
				{
					subDatabase.SetDataProvider(_subscriptionDatabaseSqlDataProvider);
					opRes = subDatabase.Save();
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
			OperationResult opRes = SecurityManager.ValidateAuthorization(authHeader, _settings.SecretsKeys.TokenHashingKey, isCloudAdmin: false/*, isProvisioningAdmin: true*/);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			List<SubscriptionDatabase> databasesList = null;

			try
			{
				databasesList = ((SubscriptionDatabaseSQLDataProvider)_subscriptionDatabaseSqlDataProvider).GetDatabasesBySubscription(subscriptionKey, dbName);
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
			OperationResult opRes = SecurityManager.ValidateAuthorization(authHeader, _settings.SecretsKeys.TokenHashingKey, isCloudAdmin: true/*, isProvisioningAdmin: true*/);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			List<ISubscription> subscriptionsList = null;

			try
			{
				subscriptionsList = ((SubscriptionSQLDataProvider)_subscriptionSQLDataProvider).GetSubscriptions(instanceKey);
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
		public IActionResult ApiQuery(string modelName)
		{
			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			OperationResult opRes = SecurityManager.ValidateAuthorization(authHeader, _settings.SecretsKeys.TokenHashingKey/*, isProvisioningAdmin: true*/);

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

			try
			{
				// read Body content
				var bodyStream = new StreamReader(HttpContext.Request.Body);
				string bodyText = bodyStream.ReadToEnd();

				if (string.IsNullOrWhiteSpace(bodyText))
				{
					_jsonHelper.AddJsonCouple<bool>("result", false);
					_jsonHelper.AddJsonCouple<string>("message", Strings.MissingBody);
					return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
				}

				// load Body data in QueryInfo object
				JObject jObject = JObject.Parse(bodyText);

				QueryInfo qi = new QueryInfo();

				foreach (var item in jObject)
				{
					string k = item.Key;
					object val = ((JValue)item.Value).Value;
					qi.Fields.Add(new QueryField(k, val));
				}

				if (qi.Fields.Count == 0)
				{
					_jsonHelper.AddJsonCouple<bool>("result", true);
					_jsonHelper.AddJsonCouple<string>("message", Strings.NoQueryParams);
					return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
				}

				// I capitalize the first letter otherwise I can't obtain the type of the object
				string modelNameWithFirstLetterUpperCase = char.ToUpper(modelName[0]) + modelName.Substring(1).ToLowerInvariant();
				// to get the type I need the namespace of object!
				string fullName = "Microarea.AdminServer.Model." + modelNameWithFirstLetterUpperCase;

				Type objectType = Type.GetType(fullName);
				if (objectType == null)
				{
					_jsonHelper.AddJsonCouple<bool>("result", false);
					_jsonHelper.AddJsonCouple<string>("message", string.Format(Strings.UnknownModelName, modelNameWithFirstLetterUpperCase));
					return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
				}

				IDataProvider dataProvider = GetProviderFromModelName(modelName);
				if (dataProvider == null)
				{
					_jsonHelper.AddJsonCouple<bool>("result", false);
					_jsonHelper.AddJsonCouple<string>("message", string.Format(Strings.NoProviderAvailableForModel, modelName));
					return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
				}

				IAdminModel model = Activator.CreateInstance(objectType) as IAdminModel;
				if (model == null)
				{
					_jsonHelper.AddJsonCouple<bool>("result", false);
					_jsonHelper.AddJsonCouple<string>("message", string.Format(Strings.NoModelLoaded, modelName));
					return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
				}

				model.SetDataProvider(dataProvider);
				opRes = model.Query(qi);

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
