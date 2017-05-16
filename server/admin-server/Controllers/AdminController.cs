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

namespace Microarea.AdminServer.Controllers
{
	//=========================================================================
	public class AdminController : Controller
    {
        AppOptions _settings;
        private IHostingEnvironment _env;
        AccountSQLDataProvider _accountSqlDataProvider;
		CompanySQLDataProvider _companySqlDataProvider;
		JsonHelper _jsonHelper;

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IOptions<AppOptions> settings)
        {
            _env = env;
            _settings = settings.Value;
            _jsonHelper = new JsonHelper();

            _accountSqlDataProvider = new AccountSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
			_companySqlDataProvider = new CompanySQLDataProvider(_settings.DatabaseInfo.ConnectionString);
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
            catch (NotImplementedException ex)
            {
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", ex.Message);
                return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
            }
            catch (SqlException e)
            {
                _jsonHelper.AddJsonCouple<bool>("result", false);
                _jsonHelper.AddJsonCouple<string>("message", e.Message);
                return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteAndClear(), ContentType = "text/html" };
            }

            // user has been found
            _jsonHelper.AddJsonCouple<bool>("result", true);
            _jsonHelper.AddJsonObject("account", account);
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
	}
}
