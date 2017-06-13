﻿using System;
using System.Data.SqlClient;
using System.IO;
using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microarea.AdminServer.Services.Providers;
using Microarea.AdminServer.Library;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

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

		JsonHelper _jsonHelper;

		HttpClient client;
		//The URL of the WEB API Service
		string url = "http://gwam.azurewebsites.net/api/accounts/";
		//string url = "http://localhost:9010/api/accounts/";

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IOptions<AppOptions> settings)
        {
            _env = env;
            _settings = settings.Value;
            _jsonHelper = new JsonHelper();
            SqlProviderFactory();//gestione provider da rivedere se si porrà il caso

			client = new HttpClient();
			client.BaseAddress = new Uri(url);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		}

        private void SqlProviderFactory()
        {
            _accountSqlDataProvider = new AccountSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _companySqlDataProvider = new CompanySQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _instanceSqlDataProvider = new InstanceSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _subscriptionSQLDataProvider = new SubscriptionSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
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

        [HttpGet("/api/accounts/{username}/{field?}")]
        [Produces("application/json")]
        //-----------------------------------------------------------------------------	
        public IActionResult ApiAccountsInformations(string username, string field)
        {
            if (String.IsNullOrEmpty(username))
            {
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", "Username cannot be empty");
                return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
            }

			IAccount account = new Account(username);

			try
			{
				account.SetDataProvider(_accountSqlDataProvider);
				account.Load();
            }
            catch (Exception ex)
            {
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", ex.Message);
                return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
            }

			if (account == null)
			{
				// TODO ask to GWAM
			}

			// here account doesn't even exist in GWAM

			if (account == null)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Invalid user");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

            // user has been found

            _jsonHelper.AddJsonCouple<bool>("result", true);
            _jsonHelper.AddJsonCouple("account", account);
            return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
		}

        // <summary>
        // validate a new login
        // </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/logins/{accountname}")]
        public async Task<IActionResult> ApiAccounts(string accountname, string password)
        {
            //usato in lettura da gwam
            AccountIdentityPack accIdPack = new AccountIdentityPack();
            //usato come risposta al frontend
            BootstrapToken bootstrapToken = new BootstrapToken();
            LoginBaseClass lbc = null;

            if (String.IsNullOrEmpty(accountname))
            {
                bootstrapToken.Result = false;
                bootstrapToken.Message = "Username cannot be empty";
                _jsonHelper.AddPlainObject<BootstrapToken>(bootstrapToken);
                return new ContentResult { StatusCode = 400, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
            }

            IAccount account = new Account(accountname);
            if (account.AccountId != -1)//non esiste richiedi a gwam//todo 
                try
                {
                    account.SetDataProvider(_accountSqlDataProvider);
                    account.Load();
                    if (account.AccountId != -1)
                    {
                        //Verifica credenziali su db
                        lbc = new LoginBaseClass(account);
                        LoginReturnCodes res = lbc.VerifyCredential(password);
                        if (res != LoginReturnCodes.NoError)
                        {
                            bootstrapToken.Result = false;
                            bootstrapToken.Message = res.ToString();//TODO STRINGHE?
                            bootstrapToken.AccountName = accountname;
                            _jsonHelper.AddPlainObject<BootstrapToken>(bootstrapToken);
                            return new ContentResult { StatusCode = 400, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                        }
                        //se successo?
                        bootstrapToken.Result = true;
                        bootstrapToken.Message = res.ToString();//TODO STRINGHE?
                        bootstrapToken.AccountName = accountname;
                        _jsonHelper.AddPlainObject<BootstrapToken>(bootstrapToken);
                        return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                    }
                }
                catch (Exception ex)
                {
                    bootstrapToken.Result = false;
                    bootstrapToken.Message = ex.Message;
                    bootstrapToken.AccountName = accountname;
                    _jsonHelper.AddPlainObject<BootstrapToken>(bootstrapToken);
                    return new ContentResult { StatusCode = 501, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                }

            else //if (account.AccountId == -1)//non esiste richiedi a gwam 
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("instanceid", "1")
                });

                HttpResponseMessage responseMessage = await client.PostAsync(url + accountname, formContent);
                var responseData = responseMessage.Content.ReadAsStringAsync();

                accIdPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Result);


                if (accIdPack.Account.AccountId == -1) // it doesn't exist on GWAM
                {
                    bootstrapToken.Result = false;
                    bootstrapToken.Message = "Invalid user";//TODO STRINGHE?
                    bootstrapToken.AccountName = accountname;
                    _jsonHelper.AddPlainObject<BootstrapToken>(bootstrapToken);
                    return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                }
                else
                {
                    // user has been found
                    account = accIdPack.Account;
                    account.SetDataProvider(_accountSqlDataProvider);

                    account.Save();//in locale
                    //Verifica credenziali con salvataggio sul provider locale
                     lbc = new LoginBaseClass(account);
                    LoginReturnCodes res = lbc.VerifyCredential(password);
                }
                //login ok, creaimo token e urls per pacchetto di risposta
                bootstrapToken.Result = true;
                bootstrapToken.Message = "Login OK";//TODO STRINGHE?
                bootstrapToken.AccountName = accountname;
                bootstrapToken.ApplicationLanguage = account.ApplicationLanguage;
                bootstrapToken.PreferredLanguage = account.PreferredLanguage;
                bootstrapToken.Subscriptions = accIdPack.Subscriptions;
                bootstrapToken.UserTokens = lbc.Tokens;
                _jsonHelper.AddPlainObject<BootstrapToken>(bootstrapToken);
                return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };

            }
            bootstrapToken.Result = false;
            bootstrapToken.Message = "Invalid user";//TODO STRINGHE?
            bootstrapToken.AccountName = accountname;
            _jsonHelper.AddPlainObject<BootstrapToken>(bootstrapToken);
            return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };

        }

        /// <summary>
        /// Insert/update account
        /// </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/accounts/{accountname}")]
		public IActionResult ApiAccounts(string accountname, string password, string email)
		{
			if (String.IsNullOrEmpty(accountname))
			{
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", "Account name cannot be empty");
				return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			bool result = false;
			try
			{
                IAccount iAccount = new Account(accountname);
                iAccount.SetDataProvider(_accountSqlDataProvider);
				iAccount.Password = password;
				iAccount.Email = email;
                result = iAccount.Save();
            }
			catch (SqlException e)
			{
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", "Save account operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
			}

            _jsonHelper.AddJsonCouple<bool>("result", true);
            _jsonHelper.AddJsonCouple<string>("message", "Save account operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
		}

		/// <summary>
		/// Insert/update company
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/companies/{companyname}")]
		public IActionResult ApiCompanies(string companyname, string description, int subscriptionid)
		{
			if (String.IsNullOrEmpty(companyname))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Company name cannot be empty");
				return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			bool result = false;
			try
			{
				ICompany iCompany = new Company(companyname);
				iCompany.SetDataProvider(_companySqlDataProvider);
				iCompany.Description = description;
				iCompany.SubscriptionId = subscriptionid;
				result = iCompany.Save();
			}
			catch (SqlException e)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Save company operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
			}

			_jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple<string>("message", "Save company operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
		}

		/// <summary>
		/// Insert/update instance
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/instances/{instancename}")]
		public IActionResult ApiInstances(string instancename, string customer, bool disabled)
		{
			if (String.IsNullOrEmpty(instancename))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Instance name cannot be empty");
				return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			bool result = false;
			try
			{
				IInstance iInstance = new Instance(instancename);
				iInstance.SetDataProvider(_instanceSqlDataProvider);
				iInstance.Customer = customer;
				iInstance.Disabled = disabled;
				result = iInstance.Save();
			}
			catch (SqlException e)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Save instance operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
			}

			_jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple<string>("message", "Save instance operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
		}

		/// <summary>
		/// Insert/update Subscription
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/subscriptions/{subscriptionname}")]
		public IActionResult ApiSubscriptions(string subscriptionname, int instanceid)
		{
			if (String.IsNullOrEmpty(subscriptionname))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Subscription name cannot be empty");
				return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			bool result = false;
			try
			{
				ISubscription iSubscription = new Subscription(subscriptionname);
				iSubscription.SetDataProvider(_subscriptionSQLDataProvider);
				iSubscription.InstanceId = instanceid;
				result = iSubscription.Save();
			}
			catch (SqlException e)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Save subscription operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
			}

			_jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple<string>("message", "Save subscription operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
		}
	}
}
