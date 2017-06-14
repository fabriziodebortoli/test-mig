using System;
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

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IOptions<AppOptions> settings)
        {
            _env = env;
            _settings = settings.Value;
            _jsonHelper = new JsonHelper();
            SqlProviderFactory(); //gestione provider da rivedere se si porrà il caso
		}

		//-----------------------------------------------------------------------------	
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
            return new ContentResult { Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
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
                return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
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
                return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
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
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
			}

            // user has been found

            _jsonHelper.AddJsonCouple<bool>("result", true);
            _jsonHelper.AddJsonCouple("account", account);
            return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
		}

        // <summary>
        // validate a new login
        // </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/logins/{accountname}")]
        public IActionResult ApiAccounts(string accountname, string password)
        {
            if (String.IsNullOrEmpty(accountname))
            {
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", "Username cannot be empty");
                return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
            }

            IAccount account = new Account(accountname);

            try
            {
                account.SetDataProvider(_accountSqlDataProvider);
                account.Load();
                if (account != null)
                {
                    //Verifica credenziali su db
                    LoginBaseClass lbc = new LoginBaseClass(account);
                    LoginReturnCodes res = lbc.VerifyCredential(password);
                    if (res != LoginReturnCodes.NoError)
                    {
                        _jsonHelper.AddJsonCouple<bool>("result", false);
                        _jsonHelper.AddJsonCouple("message", res.ToString());//TODO STRINGHE?
                        return new ContentResult { StatusCode = 401, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
                    }
                    //se successo?
                    _jsonHelper.AddJsonCouple<bool>("result", true);
                    _jsonHelper.AddJsonCouple("message", res.ToString());//TODO STRINGHE?
                    return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
                }
            }
            catch (Exception ex)
            {
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", ex.Message);
                return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
            }

			if (account == null)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple("message", "Invalid user");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
			}

            // here account doesn't even exist in GWAM

			_jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple("account", account);

            if (account == null)//non esiste su gwam
            {
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple("message", "Invalid user");
                return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
            }
            else
            {
                // user has been found
                //account.Save();//in locale
                //Verifica credenziali su GWAM con salvataggio sul provider locale
                //LoginBaseClass lbc = new LoginBaseClass(account);
                //LoginReturnCodes res = lbc.VerifyCredential(password);
            }
            _jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple("account", account);

			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };

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
				return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
			}

			Stream body = HttpContext.Request.Body;
			StreamReader sr = new StreamReader(body);
			string bodyString = sr.ReadToEnd();

			_jsonHelper.Read(bodyString);

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
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", "Save account operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

            _jsonHelper.AddJsonCouple<bool>("result", true);
            _jsonHelper.AddJsonCouple<string>("message", "Save account operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
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
				return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
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
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Save company operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			_jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple<string>("message", "Save company operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
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
				return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
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
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Save instance operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			_jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple<string>("message", "Save instance operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
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
				return new ContentResult { StatusCode = 400, Content = _jsonHelper.WriteAndClear(), ContentType = "application/json" };
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
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			if (!result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Save subscription operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
			}

			_jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple<string>("message", "Save subscription operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
		}
	}
}
