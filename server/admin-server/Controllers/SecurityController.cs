using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Library;
using Microarea.AdminServer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microarea.AdminServer.Services.Providers;
using Microarea.AdminServer.Model.Interfaces;
using Newtonsoft.Json;
using Microarea.AdminServer.Model;
using System.IO;
using Microarea.AdminServer.Properties;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
                bootstrapTokenContainer.Result = false;
                bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.Error;
                bootstrapTokenContainer.Message = Strings.AccountNameCannotBeEmpty;
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
                    // Chiedo al gwam se qualcosa e modificato facendo un check sui tick, se qualcosa modificato devo aggiornare.
                    Task<string> responseData = await VerifyAccountModificationGWAM(new AccountModification(account.AccountName, account.Ticks));

                    // Used as a container for the GWAM response.
                    AccountIdentityPack accountIdentityPack = new AccountIdentityPack();
                    accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Result);

                    if (accountIdentityPack == null)
                    {
                        bootstrapTokenContainer.Result = false;
                        bootstrapTokenContainer.Message = Strings.UnknownError;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                        return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                    }
                    // Se sul gwam non corrisponde... todo concettualmente result true o fale?
                    if (accountIdentityPack.Result) 
                    {
                        // Se non fosse ExistsOnDB vuol dire che il tick corrisponde, non suona bene ma è così, forse dovrebbe tornare un codice da valutare.
                        if (accountIdentityPack.Account.ExistsOnDB)
                        {
                            account = accountIdentityPack.Account;
                            // Salvo l Account in locale.
                            account.Save(); 
                        }
                    }
                    else
                    {
                        bootstrapTokenContainer.Result = false;
                        bootstrapTokenContainer.Message = accountIdentityPack.Message;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                        return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                    }
                    // Verifica credenziali su db.
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

                    // Se credenziali valide.
                    UserTokens t = CreateTokens(account);

                    if (t == null)
                    {
                        bootstrapTokenContainer.Result = false;
                        bootstrapTokenContainer.Message = LoginReturnCodes.ErrorSavingTokens.ToString();
                        bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.ErrorSavingTokens;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                        return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                    }

                    // Setting the token...
                    bootstrapToken.AccountName = credentials.AccountName;
                    bootstrapToken.UserTokens = t;
                    bootstrapToken.ApplicationLanguage = account.ApplicationLanguage;
                    bootstrapToken.PreferredLanguage = account.PreferredLanguage;
                    bootstrapToken.Subscriptions = accountIdentityPack.Subscriptions;
                    bootstrapToken.Urls = GetUrlsForThisInstance(_settings.InstanceIdentity.InstanceKey);

                    // ...and its container.
                    bootstrapTokenContainer.Result = true;
                    bootstrapTokenContainer.Message = res.ToString();
                    bootstrapTokenContainer.ResultCode = (int)res;
                    bootstrapTokenContainer.ExpirationDate = DateTime.Now.AddMinutes(5);

                    // Creating JWT Token.
                    bootstrapTokenContainer.JwtToken = GenerateJWTToken(bootstrapToken);

                    // For now, we maintain also the plain token.
                    bootstrapTokenContainer.ExpirationDate = DateTime.Now.AddMinutes(5);

                    _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                    return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                }
                // Se non esiste, richiedi a gwam.
                if (!account.ExistsOnDB) 
                {
                    Task<string> responseData = await VerifyUserOnGWAM(credentials);

                    // Used as a container for the GWAM response.
                    AccountIdentityPack accountIdentityPack = new AccountIdentityPack();
                    accountIdentityPack = JsonConvert.DeserializeObject<AccountIdentityPack>(responseData.Result);

                    if (accountIdentityPack == null || !accountIdentityPack.Result) // it doesn't exist on GWAM
                    {
                        bootstrapTokenContainer.Result = false;
                        bootstrapTokenContainer.Message = Strings.GwamDislikes + accountIdentityPack.Message;
                        bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.UnknownAccountName;
                        _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                        return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                    }
                    else
                    {
                        // User has been found.
                        account = accountIdentityPack.Account;
                        account.SetDataProvider(_accountSqlDataProvider);

                        // Salvataggio sul provider locale.
                        // Salvo anche l associazione con le  subscription e  tutti gli URLs.
                        account.Save();

                        OperationResult subOpRes = SaveSubscriptions(accountIdentityPack);

                        // Fallisce a salvare le subscription associate e  interrompo la login, corretto?
                        if (!subOpRes.Result)
                        {
                            bootstrapTokenContainer.Result = false;
                            bootstrapTokenContainer.Message = subOpRes.Message;
                            bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.Error;
                            _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                            return new ContentResult { StatusCode = 200, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
                        }
                        // Verifica credenziali.
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
                    }

                    // Login ok, creaimo token e urls per pacchetto di risposta.
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
                    bootstrapToken.Urls = GetUrlsForThisInstance(_settings.InstanceIdentity.InstanceKey);
                    bootstrapToken.UserTokens = t;

                    bootstrapTokenContainer.Result = true;
                    bootstrapTokenContainer.Message = Strings.LoginOK;
                    bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.NoError;

                    // Creating JWT Token.
                    bootstrapTokenContainer.JwtToken = GenerateJWTToken(bootstrapToken);

                    // For now, we maintain also the plain token.
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
                bootstrapTokenContainer.Message = "080 ApiAccounts Exception: " + exc.Message;
                bootstrapTokenContainer.ResultCode = (int)LoginReturnCodes.GenericLoginFailure;
                _jsonHelper.AddPlainObject<BootstrapTokenContainer>(bootstrapTokenContainer);
                return new ContentResult { StatusCode = 500, Content = _jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
            }
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
        private string GenerateJWTToken(BootstrapToken bootstrapToken)
        {
            JWTToken jwtToken = new JWTToken();
            JWTTokenHeader jWTTokenHeader = new JWTTokenHeader();
            jWTTokenHeader.alg = "HS256";
            jWTTokenHeader.typ = "JWT";
            jwtToken.header = jWTTokenHeader;
            jwtToken.payload = bootstrapToken;
            return jwtToken.GetToken();
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
        private async Task<Task<string>> VerifyAccountModificationGWAM(AccountModification accMod)
        {
            OperationResult opRes = await _httpHelper.PostDataAsync(this.GWAMUrl + "accounts/" + accMod.AccountName + "/" + accMod.Ticks, new List<KeyValuePair<string, string>>());
            return (Task<string>)opRes.Content;
        }

        //----------------------------------------------------------------------
        private UserTokens CreateTokens(IAccount account)
        {
            UserTokens tokens = new UserTokens(account.IsAdmin, account.AccountName);
            tokens.Setprovider(_tokenSQLDataProvider);
            if (tokens.Save())
            {
                return tokens;
            }
            // We should never return null.
            return new UserTokens();
        }
    }
}
