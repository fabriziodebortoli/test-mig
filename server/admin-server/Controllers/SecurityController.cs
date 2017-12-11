using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.All;
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
    //================================================================================
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

            IInstance instance= this.GetInstance(instanceKey);

            try
            {
                burgerData = new BurgerData(_settings.DatabaseInfo.ConnectionString);                
                Account account = Account.GetAccountByName(burgerData, credentials.AccountName);

                // L'account esiste sul db locale
                if (account != null)
                {
                    GwamCaller gc = new GwamCaller(_httpHelper, this.GWAMUrl, instance);

                    // Chiedo al gwam se qualcosa è modificato facendo un check sui tick, se qualcosa modificato devo aggiornare.
                    OperationResult result = gc.VerifyAccountModificationGWAM(new AccountModification(account.AccountName, instanceKey, account.Ticks));

                    if (!result.Result) //Errore, mi devo fermare
                        return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);

                    //l'oggetto potrebbe essere da aggiornare o  no

                    if (result.Code == (int)GwamMessageStrings.GWAMCodes.DataToUpdate)
                    {
                        AccountIdentityPack accountIdentityPack =  JsonConvert.DeserializeObject<AccountIdentityPack>(result.Content.ToString());
                        account = accountIdentityPack.Account;
                        result = SaveAccountIdentityPack(accountIdentityPack, account);
                        if (!result.Result)
                            return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, result.Message);
                    }

                    // Verifica credenziali su db.
                    LoginReturnCodes res = account.VerifyCredential(credentials.Password, burgerData);

                    if (res != LoginReturnCodes.NoError)
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)res, res.ToString());
                    }

                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(account, bootstrapToken, instance))
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.ErrorSavingTokens, LoginReturnCodes.ErrorSavingTokens.ToString());
                    }

                    return SetSuccessResponse(bootstrapTokenContainer, bootstrapToken, Strings.OK);
                }

                // Se non esiste, richiedi a gwam, in questo caso evidentemente se il gwam non risponde non possiamo proseguire offline
                if (account == null)
                {
                    GwamCaller gc = new GwamCaller(_httpHelper, this.GWAMUrl, instance);
                    Task <string> responseData = await gc.VerifyUserOnGWAM(credentials, instanceKey);
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
                    LoginReturnCodes res = account.VerifyCredential(credentials.Password, burgerData);

                    if (res != LoginReturnCodes.NoError)
                        return SetErrorResponse(bootstrapTokenContainer, (int)res, Strings.OK);

                    // Valorizzo il bootstraptoken per la risposta
                    if (!ValorizeBootstrapToken(account, bootstrapToken, instance))
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

        // <summary>
        // Provides change password
        // </summary>
        //-----------------------------------------------------------------------------	
        [HttpPost("/api/password/{instanceKey}")]
        public IActionResult ApiChangePassword(string instanceKey, [FromBody] ChangePasswordInfo passwordInfo)
        {
            // Used as a response to the front-end.
            BootstrapToken bootstrapToken = new BootstrapToken();
            BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();

            if (passwordInfo == null || String.IsNullOrEmpty(passwordInfo.AccountName))
            {
                return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, Strings.AccountNameCannotBeEmpty);
            }
            IInstance instance = this.GetInstance(instanceKey);
            GwamCaller gc = new GwamCaller(_httpHelper, this.GWAMUrl, instance);
            try
            {
                Account account = Account.GetAccountByName(burgerData, passwordInfo.AccountName);

                // L'account deve esistere sul db locale perchè il cambio pwd è possibile solo dopo la login
                if (account != null)
                {
                    // Chiedo al gwam se qualcosa è modificato facendo un check sui tick, se qualcosa modificato devo aggiornare.
                    OperationResult responseData = gc.VerifyAccountModificationGWAM(
                        new AccountModification(account.AccountName, instanceKey, account.Ticks));

                    //TODO verifica valori ritorno
                    // se il GWAM non risponde non si deve poter andare avanti, il  cambio pwd si può fare solo se connessi, perlomeno per adesso.
                    if (!responseData.Result)
                        return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);

                    // Used as a container for the GWAM response.
                    AccountIdentityPack accountIdentityPack = new AccountIdentityPack();
                    accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Content as string);

                    if (accountIdentityPack == null)
                    {
                        return SetErrorResponse(bootstrapTokenContainer, (int)LoginReturnCodes.Error, Strings.UnknownError);
                    }
                    // Se sul gwam corrisponde... 
                    if (accountIdentityPack.Result)
                    {

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
                    if (!ValorizeBootstrapToken(account, bootstrapToken, instance))
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

        [HttpPost("api/forgotPassword/{accountName}/{instanceKey}")]
        //-----------------------------------------------------------------------------	
        public IActionResult ApiForgotPassword(string accountName, string instanceKey)
        {
            // Used as a response to the front-end.
            OperationResult opRes = new OperationResult();
            IInstance instance = this.GetInstance(instanceKey);
            GwamCaller gc = new GwamCaller(_httpHelper, this.GWAMUrl, instance);
            try
            {
               gc.CreateRecoveryCode(accountName);
            }
            catch (Exception e)
            {
                opRes.Result = false;
                opRes.Code = (int)AppReturnCodes.ExceptionOccurred;
                opRes.Message = string.Format(Strings.ExceptionOccurred, e.Message);
                _jsonHelper.AddPlainObject<OperationResult>(opRes);
                return new ContentResult { StatusCode = 500, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
            }
            return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };

        }

        [HttpPost("api/recoveryCode")]
        //-----------------------------------------------------------------------------	
        public async Task<IActionResult> ApiCheckRecoveryCode(string accountName, string recoveryCode, string instanceKey)
        {
            // Used as a response to the front-end.
            BootstrapToken bootstrapToken = new BootstrapToken();
            BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();
            IInstance instance = this.GetInstance(instanceKey);
            GwamCaller gc = new GwamCaller(_httpHelper, this.GWAMUrl, instance);
            try
            {
                Task<string> responseData = await gc.CheckRecoveryCode(accountName, recoveryCode);
                if (responseData.Status == TaskStatus.Faulted)
                    return SetErrorResponse(bootstrapTokenContainer, (int)AppReturnCodes.GWAMCommunicationError, Strings.GWAMCommunicationError);

                OperationResult opres = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);
                if (!opres.Result)
                    return SetErrorResponse(bootstrapTokenContainer, (int)opres.Code, Strings.GwamDislikes + opres.Message);

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

            GwamCaller gc = null;
            Account account = Account.GetAccountByName(burgerData, accountName);

            if (account != null)
            {
                LoginReturnCodes code = account.IsValidUser();
                if (code != LoginReturnCodes.NoError)
                {
					opRes.Result = false;
					opRes.Code = (int)code;
                    opRes.Message = Strings.InvalidUser  +": " + code.ToString();
                    _jsonHelper.AddPlainObject<OperationResult>(opRes);//todo valutare se  avere metodo che traduce codice in stringa( codice esistente altrove)
                    return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
				}

				// TODO optimization

				// getting roles

				List<IAccountRoles> accountRoles = this.burgerData.GetList<AccountRoles, IAccountRoles>(
					String.Format(Queries.SelectAccountRoles, accountName), ModelTables.AccountRoles);

				bool isAdmin = accountRoles.Find(k => k.RoleName == "Admin" && k.Level == "Instance") != null;

				// if the account is an administrator, we look through the InstanceAccounts to find his Instances

                IInstance[] instancesArray = this.GetInstances(accountName, isAdmin);

                opRes = UpdateInstances(instancesArray);

                if (!opRes.Result)
                {
                    _jsonHelper.AddPlainObject<OperationResult>(opRes);
                    return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                }
                if (instancesArray.Length == 0)
                {
                    opRes.Result = false;
                    opRes.Code = (int)AppReturnCodes.NoInstancesAvailable;
                    opRes.Message = Strings.NoInstancesAvailable;
                    opRes.Content = instancesArray;
                    _jsonHelper.AddPlainObject<OperationResult>(opRes);
                    return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                }
                opRes.Result = true;
				opRes.Code = (int)AppReturnCodes.OK;
				opRes.Message = Strings.OperationOK;
				opRes.Content = instancesArray;

				//@@TODO: se instancesArray.Count == 0 ritornare un msg appropriato (se in locale ho l'account ma mancano le tabelle correlate)

				_jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}
           
            // account doesn'exist in Admin BackEnd, so we ask the instances list to GWAM
            gc = new GwamCaller(_httpHelper, GWAMUrl, null);
            Task<string> responseData = await gc.GetInstancesListFromGWAM(accountName);
            OperationResult opGWAMRes = JsonConvert.DeserializeObject<OperationResult>(responseData.Result);

            if (responseData.Status == TaskStatus.Faulted || !opGWAMRes.Result)
			{
				_jsonHelper.AddPlainObject<OperationResult>(opGWAMRes);
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
        #region Metodi privati
        //----------------------------------------------------------------------
        private OperationResult UpdateInstances(IInstance[] instancesArray)
        {
            OperationResult opRes = new OperationResult();
            ////qui per ogni istanza verifico se ci sono aggiornamenti sul gwam e nel caso me li salvo in locale
            for (int x = 0; x < instancesArray.Length; x++)
            {
                IInstance i = instancesArray[x];
                GwamCaller gc = new GwamCaller(_httpHelper, GWAMUrl, i);
                opRes = gc.GetInstance();
                if (opRes.Result && opRes.Code == (int)GwamMessageStrings.GWAMCodes.DataToUpdate)
                {
                    IInstance instanceUpdated = JsonConvert.DeserializeObject<Instance>(opRes.Content.ToString());
                    if (instanceUpdated != null)
                    {
                        i = instanceUpdated;
                        ((Instance)i).Save(burgerData);
                    }
                }
            }
            opRes.Result = true;
            return opRes;
        }

        //----------------------------------------------------------------------
        private IActionResult SetErrorResponse(BootstrapTokenContainer bootstrapTokenContainer, int code, string message, int statuscode = 200)
        {
            bootstrapTokenContainer.SetToken(false, code, message);
            return SetResponse(bootstrapTokenContainer, statuscode);
        }

        //----------------------------------------------------------------------
        private IActionResult SetSuccessResponse(BootstrapTokenContainer bootstrapTokenContainer, BootstrapToken token, string message)
        {
            bootstrapTokenContainer.SetToken(true, (int)LoginReturnCodes.NoError, message, token, _settings.SecretsKeys.TokenHashingKey);
            return SetResponse(bootstrapTokenContainer, 200);
        }

        //----------------------------------------------------------------------
        private IActionResult SetResponse(BootstrapTokenContainer bootstrapTokenContainer, int statuscode)
        {
            _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
            return new ContentResult { StatusCode = statuscode, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
        }

        //----------------------------------------------------------------------
        private bool ValorizeBootstrapToken(IAccount account, BootstrapToken bootstrapToken, IInstance instance)
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
            bootstrapToken.Instances = GetInstances(account.AccountName, false);
			bootstrapToken.Subscriptions = GetSubscriptions(account.AccountName); 
            bootstrapToken.Urls = GetUrlsForThisInstance(instance.InstanceKey);
			bootstrapToken.Roles = GetRoles(account.AccountName);
            AuthorizationInfo ai = instance.GetAuthorizationInfo();
            bootstrapToken.AppSecurity = new AppSecurityInfo(ai?.AppId, ai?.SecurityValue);

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
        private IInstance[] GetInstances(string accountName, bool includeAdminInstances)
        {
			// getting instances for this account

			// if includeAdminInstances is true, this method returns all instances linked to the
			// account via InstanceAccounts,
			// otherwise, it returns all the instances that are linked to the account via SubscriptionAccounts

			string querySelectInstancesForAccount = includeAdminInstances ?
				String.Format(Queries.SelectInstanceForAdminAccount, accountName) :
				String.Format(Queries.SelectInstanceForAccount, accountName);

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

        //-----------------------------------------------------------------------------
        private OperationResult SaveAccountIdentityPack(AccountIdentityPack accountIdentityPack, Account account)
        {
            if (accountIdentityPack == null || account == null) return new OperationResult(true, string.Empty, 0);//todo verifica

            OperationResult result = account.Save(burgerData);
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
        private string GetInstanceSecurityValue(string instancekey)
        {
            IInstance iInstance = this.burgerData.GetObject<Instance, IInstance>(String.Empty, ModelTables.Instances, SqlLogicOperators.AND,
                new WhereCondition[]
                {
                    new WhereCondition("InstanceKey", instancekey, QueryComparingOperators.IsEqual, false)
                });

            return iInstance.SecurityValue;
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
            Instance instance = (Instance)GetInstance(instancekey);
            if (instance == null)
                return DateTime.MinValue;

            if (!instance.VerifyPendingDate())
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
#endregion
    }
}
