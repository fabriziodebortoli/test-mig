using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

        IJsonHelper _jsonHelper;
		HttpClient client;
		string GWAMUrl;

		//-----------------------------------------------------------------------------	
		public AdminController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper)
        {
            _env = env;
            _settings = settings.Value;
            _jsonHelper = jsonHelper;
			SqlProviderFactory();
			this.GWAMUrl = _settings.ExternalUrls.GWAMUrl;

			client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		//-----------------------------------------------------------------------------	
		private void SqlProviderFactory()
        {
            _accountSqlDataProvider = new AccountSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _companySqlDataProvider = new CompanySQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _instanceSqlDataProvider = new InstanceSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _subscriptionSQLDataProvider = new SubscriptionSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _tokenSQLDataProvider =  new SecurityTokenSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
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
                return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
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
                return new ContentResult { StatusCode = 500, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
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
		// Provides login token
		// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/tokens")]
		public async Task<IActionResult> ApiAccounts([FromBody] Credentials credentials)
		{
			// used as a response to the front-end
			BootstrapToken bootstrapToken = new BootstrapToken();
			BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();

			if (credentials == null || String.IsNullOrEmpty(credentials.AccountName) || String.IsNullOrEmpty(credentials.Password))
			{
				bootstrapTokenContainer.Result = false;
                bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.Error;
                bootstrapTokenContainer.Message = "Username cannot be empty";
				_jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				LoginBaseClass lbc = null;
				IAccount account = new Account(credentials.AccountName);

				account.SetDataProvider(_accountSqlDataProvider);
				account.Load();

				if (account.ExistsOnDB)
				{
                    //chiedo al gwam se qualcosa e modificato facendo un check sui tick, se qualcosa moficiato devo aggiornare
                    Task<string> responseData = await VerifyAccountModificationGWAM(new AccountModification(account.AccountName, account.Ticks));

                    // used as a container for the GWAM response
                    AccountIdentityPack accountIdentityPack = new AccountIdentityPack();
                    accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Result);

                    if (accountIdentityPack.Result) //sul gwam non corrisponde xcui salvo questo account- todo concettualmente result true o fale?
                    {
                        if (accountIdentityPack.Account.ExistsOnDB)//se non fosse ExistsOnDB vuol dire che il tick corrisponde, non suona bene ma è così, forse dovrebbe tornare un codice da valutare.
                        {
                            account = accountIdentityPack.Account;
                            account.Save(); // in locale
                        }
                    }
                    else
                    {
                        bootstrapTokenContainer.Result = false;
                        bootstrapTokenContainer.Message = accountIdentityPack.Message;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                        return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                    }
                    // Verifica credenziali su db
                    lbc = new LoginBaseClass(account);
					LoginReturnCodes res = lbc.VerifyCredential(credentials.Password);

					if (res != LoginReturnCodes.NoError)
					{
						bootstrapTokenContainer.Result = false;
						bootstrapTokenContainer.Message = res.ToString();
                        bootstrapTokenContainer.ResultCode = (int)res;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
						return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					// se credenziali valide

					UserTokens t = CreateTokens(account);

					if (t == null)
					{
						bootstrapTokenContainer.Result = false;
						bootstrapTokenContainer.Message = LoginReturnCodes.ErrorSavingTokens.ToString();
                        bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.ErrorSavingTokens;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
						return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					// setting the token...
					bootstrapToken.AccountName = credentials.AccountName;
					bootstrapToken.UserTokens = t;

					// ...and its container.
					bootstrapTokenContainer.Result = true;
					bootstrapTokenContainer.Message = res.ToString();
                    bootstrapTokenContainer.ResultCode = (int)res;
                    bootstrapTokenContainer.PlainToken = bootstrapToken;
					bootstrapTokenContainer.ExpirationDate = DateTime.Now.AddMinutes(5);

					_jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
					return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
				}

				if (!account.ExistsOnDB) // se non esiste, richiedi a gwam
				{
					Task<string> responseData = await VerifyUserOnGWAM(credentials);

					// used as a container for the GWAM response
					AccountIdentityPack accountIdentityPack = new AccountIdentityPack();
					accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Result);

					if (accountIdentityPack == null || !accountIdentityPack.Result) // it doesn't exist on GWAM
					{
						bootstrapTokenContainer.Result = false;
						bootstrapTokenContainer.Message = "GWAM doesn't like this. Why? " + accountIdentityPack.Message;
                        bootstrapTokenContainer.ResultCode= (int)LoginReturnCodes.UnknownAccountName;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
						return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}
					else
					{
						// user has been found
						account = accountIdentityPack.Account;
						account.SetDataProvider(_accountSqlDataProvider);
						account.Save(); //salvataggio sul provider locale 
						lbc = new LoginBaseClass(account);// Verifica credenziali 
						LoginReturnCodes res = lbc.VerifyCredential(credentials.Password);
                        if (res != LoginReturnCodes.NoError)
                        {
                            bootstrapTokenContainer.Result = false;
                            bootstrapTokenContainer.Message = res.ToString();
                            bootstrapTokenContainer.ResultCode= (int)res;
                            _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                            return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                        }
					}
                    // login ok, creaimo token e urls per pacchetto di risposta
                   
                    UserTokens t = CreateTokens(account);

					if (t == null)
					{
						bootstrapTokenContainer.Result = false;
						bootstrapTokenContainer.Message = LoginReturnCodes.ErrorSavingTokens.ToString();
                        bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.ErrorSavingTokens;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
						return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					bootstrapToken.AccountName = credentials.AccountName;
					bootstrapToken.ApplicationLanguage = account.ApplicationLanguage;
					bootstrapToken.PreferredLanguage = account.PreferredLanguage;
					bootstrapToken.Subscriptions = accountIdentityPack.Subscriptions;
					bootstrapToken.Urls = new List<string>(); // todo: get server urls for this account
					bootstrapToken.UserTokens = t;

					bootstrapTokenContainer.Result = true;
					bootstrapTokenContainer.Message = "Login ok";
                    bootstrapTokenContainer.ResultCode= (int)LoginReturnCodes.NoError;

					// creating JWT Token

					JWTToken jwtToken = new JWTToken();
					JWTTokenHeader jWTTokenHeader = new JWTTokenHeader();
					jWTTokenHeader.alg = "HS256";
					jWTTokenHeader.typ = "JWT";

					jwtToken.header = jWTTokenHeader;
					jwtToken.payload = bootstrapToken;
					bootstrapTokenContainer.JwtToken = jwtToken.GetToken();

					bootstrapTokenContainer.PlainToken = bootstrapToken;
					bootstrapTokenContainer.ExpirationDate = DateTime.Now.AddMinutes(5);

					_jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
					return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
				}

				bootstrapTokenContainer.Result = false;
                bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.InvalidUserError;
                bootstrapTokenContainer.Message = LoginReturnCodes.InvalidUserError.ToString();
                _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}
			catch (Exception exc)
			{
				bootstrapTokenContainer.Result = false;
				bootstrapTokenContainer.Message = "ApiAccounts Exception: " + exc.Message;
                bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.GenericLoginFailure;
                _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}
		}

		//----------------------------------------------------------------------
		private async Task<Task<string>> VerifyUserOnGWAM(Credentials credentials)
		{
			var formContent = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("accountName", credentials.AccountName),
					new KeyValuePair<string, string>("password", credentials.Password),
					new KeyValuePair<string, string>("instanceKey", _settings.InstanceIdentity.InstanceKey)
				}
			);

			HttpResponseMessage responseMessage = await client.PostAsync(this.GWAMUrl + "accounts/", formContent);
			var responseData = responseMessage.Content.ReadAsStringAsync();
			return responseData;
		}

		//----------------------------------------------------------------------
		private async Task<Task<string>> VerifyAccountModificationGWAM(AccountModification accMod)
		{//todo modifiare
			var formContent = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("Ticks", accMod.Ticks.ToString()),
				}
			);

			HttpResponseMessage responseMessage = await client.PostAsync(this.GWAMUrl + accMod.AccountName, formContent);
			var responseData = responseMessage.Content.ReadAsStringAsync();
			return responseData;
		}

        //----------------------------------------------------------------------
        private UserTokens CreateTokens(IAccount account)
        {
            UserTokens tokens = new UserTokens(account.IsAdmin, account.AccountName);
            tokens.Setprovider(_tokenSQLDataProvider);
            if (tokens.Save()) return tokens;
            return null;
        }

        /// <summary>
        /// Insert/update account
        /// </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/accounts")]
		public IActionResult ApiAccounts()
		{
			if (HttpContext.Request == null || HttpContext.Request.Body == null)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "The Request / Body cannot be null");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			string body = string.Empty;
			using (StreamReader sr = new StreamReader(HttpContext.Request.Body))
				body = sr.ReadToEnd();

			if (string.IsNullOrWhiteSpace(body))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "The Body cannot be empty");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			bool result = false;

			try
			{
				IAccount iAccount = JsonConvert.DeserializeObject<Account>(body);
				if (iAccount != null)
				{
					OperationResult opRes = new OperationResult();
					iAccount.Email = iAccount.AccountName;
					iAccount.SetDataProvider(_accountSqlDataProvider);
					opRes = iAccount.Save();
					result = opRes.Result;
				}
            }
			catch (SqlException e)
			{
                _jsonHelper.AddJsonCouple<bool>("result", result);
                _jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (!result)
			{
                _jsonHelper.AddJsonCouple<bool>("result", result);
                _jsonHelper.AddJsonCouple<string>("message", "Save account operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<OperationResult>(new OperationResult(result, "Save account operation successfully completed"));
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Insert/update company
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/companies")]
		public IActionResult ApiCompanies(string companyname)
		{
			if (HttpContext.Request == null || HttpContext.Request.Body == null)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "The Request / Body cannot be null");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			string body = string.Empty;
			using (StreamReader sr = new StreamReader(HttpContext.Request.Body))
				body = sr.ReadToEnd();

			if (string.IsNullOrWhiteSpace(body))
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "The Body cannot be empty");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			bool result = false;
			try
			{
				ICompany iCompany = JsonConvert.DeserializeObject<Company>(body);
				if (iCompany != null)
				{
					iCompany.SetDataProvider(_companySqlDataProvider);
					result = iCompany.Save().Result;
				}
			}
			catch (SqlException e)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}
			catch (Exception ex)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", ex.Message);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (!result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Save company operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<OperationResult>(new OperationResult(result, "Save company operation successfully completed"));
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
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
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			bool result = false;
			try
			{
				IInstance iInstance = new Instance(instancename);
				iInstance.SetDataProvider(_instanceSqlDataProvider);
				iInstance.Disabled = disabled;
				result = iInstance.Save().Result;
			}
			catch (SqlException e)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (!result)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Save instance operation failed");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddJsonCouple<bool>("result", true);
			_jsonHelper.AddJsonCouple<string>("message", "Save instance operation successfully completed");
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Insert/update Subscription
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/subscriptions/{subscriptionKey?}")]
		public IActionResult ApiSubscriptions(string subscriptionKey, string instancekey)
		{
			bool result = false;
			try
			{
				ISubscription iSubscription = new Subscription(subscriptionKey);
				iSubscription.SetDataProvider(_subscriptionSQLDataProvider);
				iSubscription.InstanceKey = instancekey;
				result = iSubscription.Save().Result;
			}
			catch (SqlException e)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", e.Message);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "text/html" };
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

		[HttpGet("/api/accounts/{accountName?}")]
		[Produces("application/json")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiGetAccounts()
		{
			IAccount[] accountArray = null;

			try
			{
				accountArray = ((AccountSQLDataProvider)_accountSqlDataProvider).GetAccounts(); // gestire il parametro facoltativo per il caricamento di un solo elemento
			}
			catch (Exception ex)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", ex.Message);
				return new ContentResult { StatusCode = 501, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			if (accountArray == null)
			{
				_jsonHelper.AddJsonCouple<bool>("result", false);
				_jsonHelper.AddJsonCouple<string>("message", "Invalid user");
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
			}

			_jsonHelper.AddPlainObject<IAccount[]>(accountArray);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}
	}
}
