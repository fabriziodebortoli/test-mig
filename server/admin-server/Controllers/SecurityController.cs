using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.Tokens;
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
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers
{
	//-----------------------------------------------------------------------------	
	public class SecurityController : Controller
    {
        private IHostingEnvironment _env;
        AppOptions _settings;
        IDataProvider _accountSqlDataProvider;
        IDataProvider _tokenSQLDataProvider;
        IDataProvider _urlsSQLDataProvider;
        IDataProvider _instanceSqlDataProvider;
        IDataProvider _subscriptionSQLDataProvider;
        IJsonHelper _jsonHelper;
        IHttpHelper _httpHelper;
        string GWAMUrl;

        //-----------------------------------------------------------------------------	
        public SecurityController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper, IHttpHelper httpHelper)
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
            _instanceSqlDataProvider = new InstanceSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _subscriptionSQLDataProvider = new SubscriptionSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _tokenSQLDataProvider = new SecurityTokenSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _urlsSQLDataProvider = new ServerURLSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
        }

        // <summary>
        // Provides login token
        // </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/tokens")]
        public async Task<IActionResult> ApiTokens([FromBody] Credentials credentials)
        {
            // Used as a response to the front-end.
            BootstrapToken bootstrapToken = new BootstrapToken();
            BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();

            if (credentials == null || String.IsNullOrEmpty(credentials.AccountName) || String.IsNullOrEmpty(credentials.Password))
            {
                return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, Strings.AccountNameCannotBeEmpty);
            }

            try
            {
                IAccount account = new Account(credentials.AccountName);
                account.SetDataProvider(_accountSqlDataProvider);
                account.Load();

                // L'account esiste sul db locale
                if (account.ExistsOnDB)
                {
                    //TODO SECURITYKEY CABLATO
                    AuthorizationInfo authInfo = new AuthorizationInfo(AuthorizationInfo.TypeAppName, _settings.InstanceIdentity.InstanceKey, "ju23ff-KOPP-0911-ila");
                    // Chiedo al gwam se qualcosa è modificato facendo un check sui tick, se qualcosa modificato devo aggiornare.
                    Task<string> responseData = await VerifyAccountModificationGWAM(new AccountModification(account.AccountName, account.Ticks), authInfo);

                    // Used as a container for the GWAM response.
                    AccountIdentityPack accountIdentityPack = new AccountIdentityPack();
                    accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Result);

                    if (accountIdentityPack == null)
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error,Strings.UnknownError);
                    }
                    // Se sul gwam corrisponde... 
                    if (accountIdentityPack.Result) 
                    {
                        // Se non fosse ExistsOnDB vuol dire che il tick corrisponde, non suona bene ma è così, forse dovrebbe tornare un codice da valutare.
                        if (accountIdentityPack.Account.ExistsOnDB)
                        { 
                            // Salvo l'Account in locale.
                            account = accountIdentityPack.Account;
                            account.Save(); 
                        }
                    }
                    else
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, accountIdentityPack.Message);         
                    }
                    // Verifica credenziali su db.
                    LoginReturnCodes res = LoginBaseClass.VerifyCredential(account, credentials.Password);

                    if (res != LoginReturnCodes.NoError)
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)res, Strings.OK);
                    }
                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(accountIdentityPack, bootstrapToken))
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.ErrorSavingTokens, LoginReturnCodes.ErrorSavingTokens.ToString());
                    }

                    return SetSuccessResponse(bootstrapTokenContainer, bootstrapToken, Strings.OK);
                }
                //--------------------------------
                // Se non esiste, richiedi a gwam.
                if (!account.ExistsOnDB)
                {
                    Task<string> responseData = await VerifyUserOnGWAM(credentials);

                    // Used as a container for the GWAM response.
                    AccountIdentityPack accountIdentityPack = new AccountIdentityPack();
                    accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Result);

                    if (accountIdentityPack == null || !accountIdentityPack.Result) // it doesn't exist on GWAM
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.UnknownAccountName, Strings.GwamDislikes + accountIdentityPack.Message);
                    }
                    else
                    {
                        // User has been found.
                        // Salvataggio sul provider locale.
                        // Salvo anche l'associazione con le  subscription e  tutti gli URLs.
                        account = accountIdentityPack.Account;
                        account.SetDataProvider(_accountSqlDataProvider);
                        account.Save();

                        OperationResult subOpRes = SaveSubscriptions(accountIdentityPack);

                        // Fallisce a salvare le subscription associate e  interrompo la login, corretto?
                        if (!subOpRes.Result)
                        {
                            return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, subOpRes.Message);
                        }
                        // Verifica credenziali.
                        LoginReturnCodes res = LoginBaseClass.VerifyCredential(account, credentials.Password);

                        if (res != LoginReturnCodes.NoError)
                        {
                            return SetErrorResponse(bootstrapTokenContainer, (int)res, Strings.OK);
                        }
                    }
                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(accountIdentityPack, bootstrapToken))
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.ErrorSavingTokens, LoginReturnCodes.ErrorSavingTokens.ToString());
                    }

                    return SetSuccessResponse(bootstrapTokenContainer, bootstrapToken, Strings.LoginOK);              
                }
                return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.InvalidUserError, LoginReturnCodes.InvalidUserError.ToString());
            }
            catch (Exception exc)
            {
                return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.GenericLoginFailure, "080 ApiAccounts Exception: " + exc.Message, 500);
            }
        }

		/// <summary>
		/// Check a token
		/// </summary>
		/// <returns>
		/// OperationResult
		/// </returns>
		[HttpPost("api/token")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiCheckToken(string token)
		{
			OperationResult opRes = new OperationResult();

			try
			{
				opRes = SecurityManager.ValidateToken(token, _settings.SecretsKeys.TokenHashingKey);
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Code = (int)AppReturnCodes.ExceptionOccurred;
				opRes.Message = string.Format(Strings.ExceptionOccurred, e.Message);
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}
		}

		//----------------------------------------------------------------------
		private IActionResult SetErrorResponse(BootstrapTokenContainer bootstrapTokenContainer, int code, string message, int statuscode = 200)
        {
			bootstrapTokenContainer.SetResult(false, code, message);
            return SetResponse(bootstrapTokenContainer, statuscode);
        }

        //----------------------------------------------------------------------
        private IActionResult SetSuccessResponse(BootstrapTokenContainer bootstrapTokenContainer, BootstrapToken token, string message)
        {
            bootstrapTokenContainer.SetResult(true, (int)LoginReturnCodes.NoError, message, token, _settings.SecretsKeys.TokenHashingKey);
            return SetResponse(bootstrapTokenContainer,200);
        }

        //----------------------------------------------------------------------
        private IActionResult SetResponse(BootstrapTokenContainer bootstrapTokenContainer, int statuscode)
        {
            _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
            return new ContentResult { StatusCode = statuscode, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
        }

        //----------------------------------------------------------------------
        private bool ValorizeBootstrapToken(AccountIdentityPack accountIP, BootstrapToken bootstrapToken)
        {
			if (accountIP == null || accountIP.Account == null)
			{
				return false;
			}

            UserTokens t = CreateTokens(accountIP.Account);

			if (t == null)
			{
				return false;
			}

            bootstrapToken.AccountName = accountIP.Account.AccountName;
            bootstrapToken.UserTokens = t;
            bootstrapToken.ApplicationLanguage = accountIP.Account.ApplicationLanguage;
            bootstrapToken.PreferredLanguage = accountIP.Account.PreferredLanguage;
            bootstrapToken.Subscriptions = accountIP.Subscriptions;
            bootstrapToken.Urls = GetUrlsForThisInstance(_settings.InstanceIdentity.InstanceKey);
            return true;
        }

        //----------------------------------------------------------------------
        private List<ServerURL> GetUrlsForThisInstance(string instanceKey)
        {
            Instance iInstance = new Instance(instanceKey);
            iInstance.SetDataProvider(_instanceSqlDataProvider);
            iInstance.Load();

            if (!iInstance.ExistsOnDB)
            {
                return new List<ServerURL>();
            }

            return iInstance.LoadURLs();
        }

        //----------------------------------------------------------------------
        private OperationResult SaveSubscriptions(AccountIdentityPack accountIdentityPack)
        {
            if (accountIdentityPack == null || accountIdentityPack.Subscriptions == null) return new OperationResult(false, Strings.EmptySubscriptions);

            foreach (ISubscription s in accountIdentityPack.Subscriptions)
            {
                s.SetDataProvider(_subscriptionSQLDataProvider);
                OperationResult result = s.Save();
                if (!result.Result)
                {
                    return result;
                }
            }
            return new OperationResult(true, "ok");
        }

        //----------------------------------------------------------------------
        private async Task<Task<string>> VerifyUserOnGWAM(Credentials credentials)
        {
            string url = _settings.ExternalUrls.GWAMUrl + "accounts";

            List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
            entries.Add(new KeyValuePair<string, string>("accountName", credentials.AccountName));
            entries.Add(new KeyValuePair<string, string>("password", credentials.Password));
            entries.Add(new KeyValuePair<string, string>("instanceKey", _settings.InstanceIdentity.InstanceKey));

            OperationResult opRes = await _httpHelper.PostDataAsync(url, entries);
            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        private async Task<Task<string>> VerifyAccountModificationGWAM(AccountModification accMod, AuthorizationInfo authInfo)
        {
            string authHeader = JsonConvert.SerializeObject(authInfo);
            OperationResult opRes = await _httpHelper.PostDataAsync(
				this.GWAMUrl + "accounts/" + accMod.AccountName + "/" + accMod.Ticks, 
				new List<KeyValuePair<string, string>>(), authHeader);
            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        private UserTokens CreateTokens(IAccount account)
        {
            UserTokens tokens = new UserTokens(account.IsAdmin, account.AccountName);
            tokens.Setprovider(_tokenSQLDataProvider);
            if (tokens.Save())
                return tokens;
            return null;
        }
    }
}
