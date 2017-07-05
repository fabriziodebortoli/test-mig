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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace Microarea.AdminServer.Controllers
{
	//=========================================================================
	public class AdminController : Controller
    {
        AppOptions _settings;
        private IHostingEnvironment _env;

        IDataProvider _accountSqlDataProvider;
        IDataProvider _companySqlDataProvider;
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
            _companySqlDataProvider = new CompanySQLDataProvider(_settings.DatabaseInfo.ConnectionString);
			_instanceSqlDataProvider = new InstanceSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _subscriptionSQLDataProvider = new SubscriptionSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _tokenSQLDataProvider =  new SecurityTokenSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _urlsSQLDataProvider = new ServerURLSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
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
		/// Insert/update company
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/companies")]
		public IActionResult ApiCompanies([FromBody] Company company)
		{
			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			OperationResult opRes = SecurityManager.ValidateAuthorization(authHeader, _settings.SecretsKeys.TokenHashingKey, isProvisioningAdmin: true);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				if (company != null)
				{
					company.SetDataProvider(_companySqlDataProvider);
					opRes = company.Save();
					opRes.Message = Strings.SaveCompanyOK;
				}
			}
			catch (Exception exc)
			{
				_jsonHelper.AddJsonCouple<bool>("result", opRes.Result);
				_jsonHelper.AddJsonCouple<string>("message", "040 AdminController.ApiCompanies" + exc.Message);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (!opRes.Result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", opRes.Result);
				_jsonHelper.AddJsonCouple<string>("message", Strings.SaveCompanyKO);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpGet("/api/companies/{accountName}/{subscriptionKey?}")]
		[Produces("application/json")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiGetCompaniesByAccount(string accountName, string subscriptionKey)
		{
			if (string.IsNullOrWhiteSpace(accountName))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", Strings.AccountNameCannotBeEmpty);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];
			
			// CloudAdmin role required if SubscriptionKey is empty
			bool cloudAdminRequired = string.IsNullOrWhiteSpace(subscriptionKey);

			// check AuthorizationHeader first
			OperationResult opRes = SecurityManager.ValidateAuthorization(authHeader, _settings.SecretsKeys.TokenHashingKey, isCloudAdmin: cloudAdminRequired, isProvisioningAdmin: true);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			List<Company> companiesList = null;

			try
			{
				companiesList = ((CompanySQLDataProvider)_companySqlDataProvider).GetCompanies(accountName, subscriptionKey);
			}
			catch (Exception exc)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "010 AdminController.ApiGetCompaniesByAccount" + exc.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (companiesList == null)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", Strings.InvalidAccountName);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<List<Company>>(companiesList);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpGet("/api/subscriptions/{instanceKey?}")]
		[Produces("application/json")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiGetSubscriptions(string instanceKey)
		{
			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first
			OperationResult opRes = SecurityManager.ValidateAuthorization(authHeader, _settings.SecretsKeys.TokenHashingKey, isCloudAdmin: true, isProvisioningAdmin: true);

			if (!opRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			List<Subscription> subscriptionsList = null;

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

			_jsonHelper.AddPlainObject<List<Subscription>>(subscriptionsList);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}
	}
}
