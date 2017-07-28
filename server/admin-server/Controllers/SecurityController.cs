using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.Tokens;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
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
        IJsonHelper _jsonHelper;
        IHttpHelper _httpHelper;
        string GWAMUrl;
        BurgerData burgerData;

        //-----------------------------------------------------------------------------	
        public SecurityController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper, IHttpHelper httpHelper)
        {
            _env = env;
            _settings = settings.Value;
            _jsonHelper = jsonHelper;
            SqlProviderFactory();
            this.GWAMUrl = _settings.ExternalUrls.GWAMUrl;
            _httpHelper = httpHelper;
            burgerData = new BurgerData(_settings.DatabaseInfo.ConnectionString);

        }

        //-----------------------------------------------------------------------------	
        private void SqlProviderFactory()
        {
            _accountSqlDataProvider = new AccountSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
            _instanceSqlDataProvider = new InstanceSQLDataProvider(_settings.DatabaseInfo.ConnectionString);
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
                burgerData = new BurgerData(_settings.DatabaseInfo.ConnectionString);
                IAccount account = burgerData.GetObject<Account, IAccount>(String.Empty, ModelTables.Accounts, SqlLogicOperators.AND, new WhereCondition[]
          {
                    new WhereCondition("AccountName", credentials.AccountName, QueryComparingOperators.IsEqual, false)
          });


                // L'account esiste sul db locale
                if (account!= null)
                {
                    // Chiedo al gwam se qualcosa è modificato facendo un check sui tick, se qualcosa modificato devo aggiornare.
                    Task<string> responseData = await VerifyAccountModificationGWAM(
						new AccountModification(account.AccountName, _settings.InstanceIdentity.InstanceKey, account.Ticks), GetAuthorizationInfo());

					// GWAM call could not end correctly: so we check the object
					if (responseData.Status == TaskStatus.Faulted)
					{
						return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);
					}

					OperationResult opGWAMRes = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);

					if (!opGWAMRes.Result)
					{
						return SetErrorResponse(bootstrapTokenContainer, (int)opGWAMRes.Code, Strings.GwamDislikes + opGWAMRes.Message);
					}

					bool accountIsUpdated = opGWAMRes.Code == 6;

					if (!accountIsUpdated)
					{
						AccountIdentityPack accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(opGWAMRes.Content.ToString());
						account = accountIdentityPack.Account;
						// @@TODO: it would be better to log if Save couldn't execute correctly
						((Account)account).Save(burgerData);
					}

                    // Verifica credenziali su db.
                    LoginReturnCodes res = LoginBaseClass.VerifyCredential(((Account)account), credentials.Password, burgerData);

                    if (res != LoginReturnCodes.NoError)
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)res, res.ToString());
                    }

                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(account, bootstrapToken))
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.ErrorSavingTokens, LoginReturnCodes.ErrorSavingTokens.ToString());
                    }

                    return SetSuccessResponse(bootstrapTokenContainer, bootstrapToken, Strings.OK);
                }

                // Se non esiste, richiedi a gwam.
                if (account == null)
                {
                    Task<string> responseData = await VerifyUserOnGWAM(credentials, GetAuthorizationInfo());

					// GWAM call could not end correctly: so we check the object
					if (responseData.Status == TaskStatus.Faulted)
					{
						return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);
					}

					OperationResult opGWAMRes = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);

					if (!opGWAMRes.Result)
					{
						return SetErrorResponse(bootstrapTokenContainer, (int)opGWAMRes.Code, Strings.GwamDislikes + opGWAMRes.Message);
					}

					// @@TODO: optimization?
					AccountIdentityPack accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(opGWAMRes.Content.ToString());

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
                        OperationResult result = ((Account)account).Save(burgerData);

						if (!result.Result)
						{
							return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, result.Message);
						}

						result = SaveSubscriptions(accountIdentityPack);

                        // Fallisce a salvare le subscription associate e  interrompo la login, corretto?
                        if (!result.Result)
                        {
                            return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, result.Message);
                        }
                        // Verifica credenziali.
                        LoginReturnCodes res = LoginBaseClass.VerifyCredential(((Account)account), credentials.Password, burgerData);

                        if (res != LoginReturnCodes.NoError)
                        {
                            return SetErrorResponse(bootstrapTokenContainer, (int)res, Strings.OK);
                        }
                    }
                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(account, bootstrapToken))
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

        // <summary>
        // Provides change password
        // </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/password")]
        public async Task<IActionResult> ApiPassword([FromBody] ChangePasswordInfo passwordInfo)
        {
            // Used as a response to the front-end.
            BootstrapToken bootstrapToken = new BootstrapToken();
            BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();

            if (passwordInfo == null || String.IsNullOrEmpty(passwordInfo.AccountName))
            {
                return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, Strings.AccountNameCannotBeEmpty);
            }
            try
            {
                IAccount account =  burgerData.GetObject<Account, IAccount>(
					String.Empty, 
					ModelTables.Accounts, 
					SqlLogicOperators.AND, 
					new WhereCondition[]{
						new WhereCondition("AccountName", passwordInfo.AccountName, QueryComparingOperators.IsEqual, false)
					});

                // L'account esiste sul db locale
                if (account != null)
                {
                    // Chiedo al gwam se qualcosa è modificato facendo un check sui tick, se qualcosa modificato devo aggiornare.
                    Task<string> responseData = await VerifyAccountModificationGWAM(
						new AccountModification(account.AccountName, _settings.InstanceIdentity.InstanceKey, account.Ticks), GetAuthorizationInfo());

                    // Used as a container for the GWAM response.
                    AccountIdentityPack accountIdentityPack = new AccountIdentityPack();
                    accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Result);

                    if (accountIdentityPack == null)
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, Strings.UnknownError);
                    }
                    // Se sul gwam corrisponde... 
                    if (accountIdentityPack.Result)
                    {
                        // Se non fosse ExistsOnDB vuol dire che il tick corrisponde, non suona bene ma è così, forse dovrebbe tornare un codice da valutare.
                        if (accountIdentityPack.Account != null)
                        {
                            // Salvo l'Account in locale.
                            account = accountIdentityPack.Account;
                            ((Account)account).Save(burgerData);
                        }
                    }
                    else
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, accountIdentityPack.Message);
                    }
                    LoginReturnCodes res = LoginBaseClass.ChangePassword(((Account)account), passwordInfo, burgerData);

                    if (res != LoginReturnCodes.NoError)
                    { 
                        return SetErrorResponse(bootstrapTokenContainer, (int)res, res.ToString());
                    }

                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(account, bootstrapToken))
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.ErrorSavingTokens, LoginReturnCodes.ErrorSavingTokens.ToString());
                    }
                    return SetSuccessResponse(bootstrapTokenContainer, bootstrapToken, Strings.OK);
                }
            }
            catch (Exception exc)
            {
                return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.GenericLoginFailure, "080 ApiAccounts Exception: " + exc.Message, 500);
            }
            // L'utente che sta cercando di cambiare a password non è in locale, ma secondo me è una situzione di eccezione
            return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.InvalidUserError, LoginReturnCodes.InvalidUserError.ToString());
        }

        //-----------------------------------------------------------------------------	
        private AuthorizationInfo GetAuthorizationInfo()
        { 
            // TODO SECURITYKEY CABLATO.
            return new AuthorizationInfo(AuthorizationInfo.TypeAppName, _settings.InstanceIdentity.InstanceKey, "ju23ff-KOPP-0911-ila");
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

        [HttpPost("api/recoveryCode")]
        //-----------------------------------------------------------------------------	
        public async Task<IActionResult> ApiCheckRecoveryCode(string accountName, string recoveryCode)
        {
            // Used as a response to the front-end.
            BootstrapToken bootstrapToken = new BootstrapToken();
            BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();
            try
            {
                Task<string> responseData = await CheckRecoveryCode(accountName, recoveryCode, GetAuthorizationInfo());
            }
            catch { }
            return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, LoginReturnCodes.Error.ToString());
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
            return SetResponse(bootstrapTokenContainer, 200);
        }

        //----------------------------------------------------------------------
        private IActionResult SetResponse(BootstrapTokenContainer bootstrapTokenContainer, int statuscode)
        {
            _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
            return new ContentResult { StatusCode = statuscode, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
        }

        //----------------------------------------------------------------------
        private bool ValorizeBootstrapToken(IAccount account, BootstrapToken bootstrapToken)
        {
            if (account == null)
                return false;
            ISecurityToken[] tokens = bootstrapToken.UserTokens = CreateTokens(account);

			if (tokens == null || tokens.Length == 0)
                return false;

            bootstrapToken.AccountName = account.AccountName;
            bootstrapToken.UserTokens = tokens;
            bootstrapToken.RegionalSettings = account.RegionalSettings;
            bootstrapToken.Language = account.Language;
            bootstrapToken.Instances = GetInstances(account.AccountName);
			bootstrapToken.Subscriptions = GetSubscriptions(account.AccountName); 
            bootstrapToken.Urls = GetUrlsForThisInstance();
            bootstrapToken.IsCloudAdmin = account.IsCloudAdmin(burgerData);
            bootstrapToken.Roles = account.GetRoles(burgerData);
            AuthorizationInfo ai = GetAuthorizationInfo();
			bootstrapToken.AppSecurity = new AppSecurityInfo(ai.AppId, ai.SecurityValue);

			return true;
        }

        //----------------------------------------------------------------------
        private ISubscription[] GetSubscriptions(string accountName)
        {
			List<ISubscription> listSubscriptions = this.burgerData.GetList<Subscription, ISubscription>(
				String.Format(Queries.SelectSubscriptionsByAccount, accountName),
				ModelTables.Subscriptions);

			// @@TODO: optimization: remove ToArray()
			return listSubscriptions.ToArray();
		}

		//----------------------------------------------------------------------
		private IInstance[] GetInstances(string accountName)
		{
			Instance iInstance = new Instance();
			iInstance.SetDataProvider(_instanceSqlDataProvider);
			IInstance[] instanceArray = iInstance.GetInstancesByAccount(accountName).ToArray();
			return instanceArray;
		}

		//----------------------------------------------------------------------
		private IServerURL[] GetUrlsForThisInstance()
        {
            Instance iInstance = new Instance(_settings.InstanceIdentity.InstanceKey);
            iInstance.SetDataProvider(_instanceSqlDataProvider);
            iInstance.Load();

            if (!iInstance.ExistsOnDB)
            {
                return new IServerURL[] { };
            }

			return iInstance.LoadURLs().ToArray();
        }

        //----------------------------------------------------------------------
        private OperationResult SaveSubscriptions(AccountIdentityPack accountIdentityPack)
        {
            if (accountIdentityPack == null || accountIdentityPack.Subscriptions == null)
			{
				return new OperationResult(false, Strings.EmptySubscriptions, (int)AppReturnCodes.InvalidData);
			}

			OperationResult result = new OperationResult();

			foreach (Subscription s in accountIdentityPack.Subscriptions)
            {
				result = s.Save(this.burgerData);

                if (!result.Result)
                {
                    return result;
                }
            }

			result.Result = true;
			result.Code = (int)AppReturnCodes.OK;
			result.Message = AppReturnCodes.OK.ToString();

			return result;
        }

        //----------------------------------------------------------------------
        private async Task<Task<string>> VerifyUserOnGWAM(Credentials credentials, AuthorizationInfo authInfo)
        {
			string authHeader = JsonConvert.SerializeObject(authInfo);

			string url = _settings.ExternalUrls.GWAMUrl + "accounts";

            List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
            entries.Add(new KeyValuePair<string, string>("accountName", credentials.AccountName));
            entries.Add(new KeyValuePair<string, string>("password", credentials.Password));
            entries.Add(new KeyValuePair<string, string>("instanceKey", _settings.InstanceIdentity.InstanceKey));

            OperationResult opRes = await _httpHelper.PostDataAsync(url, entries, authHeader);

			if (!opRes.Result)
			{
				return Task.FromException<string>(new Exception());
			}

			return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        private async Task<Task<string>> CheckRecoveryCode(string accountName, string recoveryCode, AuthorizationInfo authInfo)
        {
            string authHeader = JsonConvert.SerializeObject(authInfo);
            // call GWAM API
            OperationResult opRes = await _httpHelper.PostDataAsync(
                this.GWAMUrl + "recoveryCode/" + accountName + "/" + recoveryCode,
                new List<KeyValuePair<string, string>>(), 
				authHeader);

            if (!opRes.Result)
            {
                return Task.FromException<string>(new Exception());
            }

            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        private async Task<Task<string>> VerifyAccountModificationGWAM(AccountModification accMod, AuthorizationInfo authInfo)
        {
            string authHeader = JsonConvert.SerializeObject(authInfo);

			string url = String.Format(
				"{0}accounts/{1}/{2}/{3}", 
				this.GWAMUrl, accMod.AccountName, accMod.InstanceKey, accMod.Ticks);

			// call GWAM API
			OperationResult opRes = await _httpHelper.PostDataAsync(
				url, new List<KeyValuePair<string, string>>(), authHeader);

			if (!opRes.Result)
			{
				return Task.FromException<string>(new Exception());
			}

			return (Task<string>)opRes.Content;
		}

		//----------------------------------------------------------------------
		private SecurityToken[] CreateTokens(IAccount account)
		{
			List<SecurityToken> tokenList = new List<SecurityToken>();

			UserTokens tokens = new UserTokens(account.IsCloudAdmin(burgerData), account.AccountName);
			tokens.Setprovider(_tokenSQLDataProvider);

			if (tokens.Save())
			{
				return tokens.GetTokenList(account.IsCloudAdmin(burgerData), account.AccountName).ToArray();
			}

			return tokenList.ToArray();
        }
    }
}
