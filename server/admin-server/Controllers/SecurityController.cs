using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.Tokens;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers
{
	//-----------------------------------------------------------------------------	
	public class SecurityController : Controller
    {
        private IHostingEnvironment _env;
        AppOptions _settings;
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
          
            this.GWAMUrl = _settings.ExternalUrls.GWAMUrl;
            _httpHelper = httpHelper;
            burgerData = new BurgerData(_settings.DatabaseInfo.ConnectionString);

        }

        // <summary>
        // Provides login token
        // </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/tokens/{instanceKey}")]
        public async Task<IActionResult> ApiTokens(string instanceKey, [FromBody] Credentials credentials)
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
                Account account = Account.GetAccountByName(burgerData, credentials.AccountName);
                // L'account esiste sul db locale
                if (account!= null)
                {
                    // Chiedo al gwam se qualcosa è modificato facendo un check sui tick, se qualcosa modificato devo aggiornare.
                    Task<string> responseData = await VerifyAccountModificationGWAM(
						new AccountModification(account.AccountName, instanceKey, account.Ticks), GetAuthorizationInfo(instanceKey));

                    //in questo putno se la connessione col gwam fallisce potrebbe esssre che in versione onpremise sia comuqnue possibile continuare
                  
                    // GWAM call could not end correctly: so we check the object
                    if (responseData.Status == TaskStatus.Faulted)
                    {
                      if(! IsOnPremisesInstance(instanceKey))
                            return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);
                        //imposto il flag pending per capire quanto tempo passa fuori copertura
                        if (!VerifyPendingFlag(instanceKey))
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
                        OperationResult result = SaveAccountIdentityPack(accountIdentityPack, account);
                        if (!result.Result)
                            return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, result.Message);
                    }

                    // Verifica credenziali su db.
                    LoginReturnCodes res = ((Account)account).VerifyCredential(credentials.Password, burgerData);

                    if (res != LoginReturnCodes.NoError)
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)res, res.ToString());
                    }

                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(account, bootstrapToken, instanceKey))
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.ErrorSavingTokens, LoginReturnCodes.ErrorSavingTokens.ToString());
                    }

                    return SetSuccessResponse(bootstrapTokenContainer, bootstrapToken, Strings.OK);
                }

                // Se non esiste, richiedi a gwam.
                if (account == null)
                {
                    Task<string> responseData = await VerifyUserOnGWAM(credentials, GetAuthorizationInfo(instanceKey), instanceKey);
                    OperationResult opGWAMRes = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);
                    // GWAM call could not end correctly, but in this case we need gwam because does not exist the account in the provisioning db

                    if (responseData.Status == TaskStatus.Faulted)
                        return SetErrorResponse(bootstrapTokenContainer, (int)opGWAMRes.Code, Strings.GWAMCommunicationError + opGWAMRes.Message);

                    if (!opGWAMRes.Result)
                        return SetErrorResponse(bootstrapTokenContainer, (int)opGWAMRes.Code, Strings.GwamDislikes + opGWAMRes.Message);

                    AccountIdentityPack accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(opGWAMRes.Content.ToString());

                    if (accountIdentityPack == null || !accountIdentityPack.Result) // it doesn't exist on GWAM
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.UnknownAccountName, Strings.GwamDislikes + accountIdentityPack.Message);

                    account = accountIdentityPack.Account;

                    OperationResult result = SaveAccountIdentityPack(accountIdentityPack, account);
                    if (!result.Result)
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, result.Message);

                    // Verifica credenziali.
                    LoginReturnCodes res = ((Account)account).VerifyCredential(credentials.Password, burgerData);

                    if (res != LoginReturnCodes.NoError)
                        return SetErrorResponse(bootstrapTokenContainer, (int)res, Strings.OK);

                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(account, bootstrapToken, instanceKey))
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.ErrorSavingTokens, LoginReturnCodes.ErrorSavingTokens.ToString());
                    return SetSuccessResponse(bootstrapTokenContainer, bootstrapToken, Strings.LoginOK);
                }

                return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.InvalidUserError, LoginReturnCodes.InvalidUserError.ToString());
            }
            catch (Exception exc)
            {
                return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.GenericLoginFailure, "080 ApiAccounts Exception: " + exc.Message, 500);
            }
        }

        //-----------------------------------------------------------------------------
        private OperationResult SaveAccountIdentityPack(AccountIdentityPack accountIdentityPack, Account account)
        {
            if (accountIdentityPack == null || account == null) return new OperationResult(true, string.Empty,0);//todo verifica

            OperationResult result = ((Account)account).Save(burgerData);
            if (result.Result && accountIdentityPack.Subscriptions != null)
                result = SaveSubscriptions(accountIdentityPack);
            if (result.Result && accountIdentityPack.Instances != null)
                result = SaveSubscriptionsInstances(accountIdentityPack);
            if (result.Result && accountIdentityPack.Subscriptions != null)
                result = SaveSubscriptionsAccounts(accountIdentityPack);
            if (result.Result && accountIdentityPack.Roles != null)
                result = SaveAccountRoles(accountIdentityPack);
           
            return result;
        }

        //-----------------------------------------------------------------------------
        private OperationResult SaveSubscriptionsInstances(AccountIdentityPack accountIdentityPack)
        {
            if (accountIdentityPack == null || accountIdentityPack.Instances == null)
            {
                return new OperationResult(false, "Empty Instances", (int)AppReturnCodes.InvalidData);
            }

            OperationResult result = new OperationResult();

            foreach (Instance i in accountIdentityPack.Instances)
            {
                result = i.Save(this.burgerData);

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

		// salvo per l'account corrente tutte le subscriptions ad esso associate
		//-----------------------------------------------------------------------------
		private OperationResult SaveSubscriptionsAccounts(AccountIdentityPack accountIdentityPack)
		{
			if (accountIdentityPack == null || accountIdentityPack.Subscriptions == null)
			{
				return new OperationResult(false, "Empty Subscriptions", (int)AppReturnCodes.InvalidData);
			}

			OperationResult result = new OperationResult();

			SubscriptionAccount subAccount;

			foreach (Subscription s in accountIdentityPack.Subscriptions)
			{
				subAccount = new SubscriptionAccount();
				subAccount.AccountName = accountIdentityPack.Account.AccountName;
				subAccount.SubscriptionKey = s.SubscriptionKey;
                subAccount.Ticks = s.Ticks;
                result = subAccount.Save(this.burgerData);

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

		//-----------------------------------------------------------------------------
		private OperationResult SaveAccountRoles(AccountIdentityPack accountIdentityPack)
		{
			if (accountIdentityPack == null || accountIdentityPack.Roles == null)
			{
				return new OperationResult(false, "Empty Roles", (int)AppReturnCodes.InvalidData);
			}

			OperationResult result = new OperationResult();

			Role role;
			AccountRoles accRole;

			foreach (AccountRoles r in accountIdentityPack.Roles)
			{
				// prima inserisco il ruolo (se non esiste)
				role = new Role();
				role.RoleName = r.RoleName;
				if (!role.Save(this.burgerData).Result)
					continue;

				// poi vado ad inserire le associazioni ruolo/account
				accRole = new AccountRoles
				{
					RoleName = r.RoleName,
					AccountName = r.AccountName,
					EntityKey = r.EntityKey,
					Level = r.Level
				};
				result = accRole.Save(this.burgerData);

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
        
        //-----------------------------------------------------------------------------	
        private bool VerifyPendingFlag(string instanceKey) 
        {
            // Verifico che la data attuale sia inferiore alla massima data prevista per la disconnessione.
            DateTime dt = GetInstancePendingDate(instanceKey);
            return DateTime.Now < dt;
        }

        // <summary>
        // Provides change password
        // </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/password/{instanceKey}")]
        public async Task<IActionResult> ApiPassword(string instanceKey,[FromBody] ChangePasswordInfo passwordInfo)
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
                Account account = Account.GetAccountByName(burgerData, passwordInfo.AccountName);

                // L'account esiste sul db locale
                if (account != null)
                {
                    // Chiedo al gwam se qualcosa è modificato facendo un check sui tick, se qualcosa modificato devo aggiornare.
                    Task<string> responseData = await VerifyAccountModificationGWAM(
						new AccountModification(account.AccountName, instanceKey, account.Ticks), GetAuthorizationInfo(instanceKey));


                    // GWAM call could not end correctly: so we check the object
                    if (responseData.Status == TaskStatus.Faulted)
                    {
                        if (!IsOnPremisesInstance(instanceKey))
                            return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);
                        //imposto il flag pending per capire quanto tempo passa fuori copertura
                        if (!VerifyPendingFlag(instanceKey))
                            return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);
                    }

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
                            (account).Save(burgerData);
                        }
                    }
                    else
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, accountIdentityPack.Message);
                    }
                    LoginReturnCodes res = (account).ChangePassword(passwordInfo, burgerData);

                    if (res != LoginReturnCodes.NoError)
                    { 
                        return SetErrorResponse(bootstrapTokenContainer, (int)res, res.ToString());
                    }

                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(account, bootstrapToken, instanceKey))
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
        private AuthorizationInfo GetAuthorizationInfo(string instanceKey)
        {
            return new AuthorizationInfo(AuthorizationInfo.TypeAppName, instanceKey, GetInstanceSecurityValue(instanceKey));
        }

        //-----------------------------------------------------------------------------	
        private string GetInstanceSecurityValue(string instancekey)
        {
            IRegisteredApp app = new RegisteredApp();
            try
            {
                app = burgerData.GetObject<RegisteredApp, IRegisteredApp>(
                    String.Empty, ModelTables.RegisteredApps, SqlLogicOperators.AND, new WhereCondition[] {
                        new WhereCondition("AppId", instancekey, QueryComparingOperators.IsEqual, false)
                    });

                if (app == null)
                    return string.Empty;
            }
            catch { }
            return app.SecurityValue; 
        }

        //-----------------------------------------------------------------------------	
        private string GetInstanceOrigin(string instancekey)
        {
            IInstance instance = GetInstance(instancekey);
            if (instance == null)
                return string.Empty;

            return instance.Origin; 
        }

        //-----------------------------------------------------------------------------	
        private DateTime GetInstancePendingDate(string instancekey)
        {
            IInstance instance = GetInstance(instancekey);
            if (instance == null)
                return DateTime.MinValue;

            return instance.PendingDate;
        }

        //-----------------------------------------------------------------------------	
        private IInstance GetInstance(string instancekey)
        {
            try
            {
                return burgerData.GetObject<Instance, IInstance>(
                    String.Empty, ModelTables.Instances, SqlLogicOperators.AND, new WhereCondition[] {
                        new WhereCondition("InstanceKey", instancekey, QueryComparingOperators.IsEqual, false) });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                //todo log
                return null;
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
		public IActionResult ApiCheckToken(string token, string roleName, string entityKey, string level)
		{
			OperationResult opRes = new OperationResult();

			try
			{
				opRes = SecurityManager.ValidateToken(token, _settings.SecretsKeys.TokenHashingKey, roleName, entityKey, level);
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
        public async Task<IActionResult> ApiCheckRecoveryCode(string accountName, string recoveryCode, string instanceKey)
        {
            // Used as a response to the front-end.
            BootstrapToken bootstrapToken = new BootstrapToken();
            BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();
            try
            {
                Task<string> responseData = await CheckRecoveryCode(accountName, recoveryCode, GetAuthorizationInfo(instanceKey));
                // GWAM call could not end correctly: so we check the object
                if (responseData.Status == TaskStatus.Faulted)
                {
                    if(! IsOnPremisesInstance(instanceKey))
                        return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);
                    
					//imposto il flag pending per capire quanto tempo passa fuori copertura
                    if (!VerifyPendingFlag(instanceKey))
                        return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);
                }
            }
            catch { }
            return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, LoginReturnCodes.Error.ToString());
        }

		/// <summary>
		/// This API returns the list of all instances belonging to an account
		/// Used primarily by the login process in order to pre-select the instance
		/// where user want to connect. We check only account name existence.
		/// </summary>
		[HttpPost("api/listInstances")]
		//-----------------------------------------------------------------------------	
		public async Task<ActionResult> ApiListInstances([FromBody]string accountName)
		{
			OperationResult opRes = new OperationResult();

			if (String.IsNullOrEmpty(accountName))
			{
				opRes.Result = false;
				opRes.Code = (int)AppReturnCodes.EmptyCredentials;
				opRes.Message = Strings.EmptyCredentials;
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

            // this is a pre-login, so only account name existence is verified
            Account account = Account.GetAccountByName(burgerData, accountName);

			if (account != null)
			{
                if (account.Disabled || account.Locked)
                {
                    //TODO far qualcosa?
                }

				// TODO: check on GWAM if tables have been updated
				IInstance[] instancesArray = this.GetInstances(accountName);
				opRes.Result = true;
				opRes.Code = (int)AppReturnCodes.OK;
				opRes.Message = Strings.OperationOK;
				opRes.Content = instancesArray;

				//@@TODO: se instancesArray.Count == 0 ritornare un msg appropriato (se in locale ho l'account ma mancano le tabelle correlate)

				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// account doesn'exist in Admin BackEnd, so we ask the instances list to GWAM

			Task<string> responseData = await GetInstancesListFromGWAM(accountName);

			// GWAM call could not end correctly: so we check the object
			if (responseData.Status == TaskStatus.Faulted)
			{
				opRes.Result = false;
				opRes.Code = (int)AppReturnCodes.GWAMCommunicationError;
				opRes.Message = Strings.GWAMCommunicationError;
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			OperationResult opGWAMRes = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);

			if (!opGWAMRes.Result)
			{
				opRes.Result = false;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				opRes.Message = Strings.InvalidAccountName;
				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			opRes.Result = true;
			opRes.Code = (int)AppReturnCodes.OK;
			opRes.Message = Strings.OperationOK;
			opRes.Content = opGWAMRes.Content;
			_jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		//----------------------------------------------------------------------
		private async Task<Task<string>> GetInstancesListFromGWAM(string accountName)
		{
			string url = String.Format("{0}listInstances/{1}", this.GWAMUrl, accountName);

			// call GWAM API 
			OperationResult opRes = await _httpHelper.PostDataAsync(
				url, new List<KeyValuePair<string, string>>(), String.Empty);

			if (!opRes.Result)
			{
				return Task.FromException<string>(new Exception());
			}

			return (Task<string>)opRes.Content;
		}

		//----------------------------------------------------------------------
		private bool IsOnPremisesInstance(string instanceKey)
        {
          return String.Compare(GetInstanceOrigin(instanceKey), Instance.OnPremisesOrigin, StringComparison.InvariantCultureIgnoreCase) == 0;
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
        private bool ValorizeBootstrapToken(IAccount account, BootstrapToken bootstrapToken, string instanceKey)
        {
            if (account == null)
                return false;

            ISecurityToken[] tokens = CreateTokens(account);

			// we have always two tokens!

			if (tokens.Length < 2)
                return false;

            bootstrapToken.AccountName = account.AccountName;
            bootstrapToken.UserTokens = tokens;
            bootstrapToken.RegionalSettings = account.RegionalSettings;
            bootstrapToken.Language = account.Language;
            bootstrapToken.Instances = GetInstances(account.AccountName);
			bootstrapToken.Subscriptions = GetSubscriptions(account.AccountName); 
            bootstrapToken.Urls = GetUrlsForThisInstance(instanceKey);
			bootstrapToken.Roles = GetRoles(account.AccountName);

            AuthorizationInfo ai = GetAuthorizationInfo(instanceKey);
			bootstrapToken.AppSecurity = new AppSecurityInfo(ai.AppId, ai.SecurityValue);

			return true;
        }

		//----------------------------------------------------------------------
		private IAccountRoles[] GetRoles(string accountName)
		{
			// only enabled roles are loaded
			return this.burgerData.GetList<AccountRoles, IAccountRoles>(
				String.Format(Queries.SelectRolesByAccountName, accountName),
				ModelTables.AccountRoles).ToArray();
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
            // getting instances for this account
            string querySelectInstancesForAccount = String.Format(Queries.SelectInstanceForAccount, accountName);
            List<IInstance> instancesList = this.burgerData.GetList<Instance, IInstance>(querySelectInstancesForAccount, ModelTables.Instances);
            return instancesList != null ? instancesList.ToArray() : new Instance[] { };
        }

        //----------------------------------------------------------------------
        private IServerURL[] GetUrlsForThisInstance(string instanceKey)
        {
            IInstance instance = GetInstance(instanceKey);

            if (instance == null)
            {
                return new IServerURL[] { };
            }
            return LoadURLs(instanceKey);
        }

		//----------------------------------------------------------------------
		private IServerURL[] LoadURLs(string instanceKey)
		{
			try
			{
				burgerData = new BurgerData(_settings.DatabaseInfo.ConnectionString);
				List<IServerURL> l = burgerData.GetList<ServerURL, IServerURL>(ModelTables.ServerURLs, SqlLogicOperators.AND, new WhereCondition[]
				{
					new WhereCondition("InstanceKey", instanceKey, QueryComparingOperators.IsEqual, false)
				});

				return l.ToArray();
			}
			catch { }
			return new IServerURL[] { };
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
        private async Task<Task<string>> VerifyUserOnGWAM(Credentials credentials, AuthorizationInfo authInfo, string instanceKey)
        {
			string authHeader = JsonConvert.SerializeObject(authInfo);

			string url = _settings.ExternalUrls.GWAMUrl + "accounts";

            List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
            entries.Add(new KeyValuePair<string, string>("accountName", credentials.AccountName));
            entries.Add(new KeyValuePair<string, string>("password", credentials.Password));
            entries.Add(new KeyValuePair<string, string>("instanceKey", instanceKey));

            OperationResult opRes = await _httpHelper.PostDataAsync(url, entries, authHeader);

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
        private async Task<Task<string>> CheckRecoveryCode(string accountName, string recoveryCode, AuthorizationInfo authInfo)
        {
            string authHeader = JsonConvert.SerializeObject(authInfo);
            // call GWAM API // todo onpremises ilaria
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

		/// <summary>
		/// We create two tokens: one for API and one for Application (M4)
		/// </summary>
		//----------------------------------------------------------------------
		private SecurityToken[] CreateTokens(IAccount account)
		{
			List<SecurityToken> tokenList = new List<SecurityToken>();

			// API token generation

			SecurityToken apiToken = SecurityToken.GetToken(TokenType.API, account.AccountName);
			OperationResult opRes = apiToken.Save(this.burgerData);
			if (!opRes.Result)
				return tokenList.ToArray();

			tokenList.Add(apiToken);

			// APP token generation

			SecurityToken appToken = SecurityToken.GetToken(TokenType.Authentication, account.AccountName);
			opRes = appToken.Save(this.burgerData);

			if (opRes.Result)
				tokenList.Add(appToken);

			return tokenList.ToArray();
        }
    }
}
