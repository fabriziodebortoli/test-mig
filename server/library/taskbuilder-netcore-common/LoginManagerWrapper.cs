using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WCFLoginManager;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Newtonsoft.Json;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.WebServicesWrapper
{
    public class LoginManagerSessionManager
    {
        public static Dictionary<string, LoginManagerSession> LoginManagerSessionTable = new Dictionary<string, LoginManagerSession>();
        public static Object staticTicket = new object();
        //-----------------------------------------------------------------------
        public static LoginManagerSession GetLoginManagerSession(string authenticationToken)
        {
            lock (staticTicket)
            {
                LoginManagerSessionTable.TryGetValue(authenticationToken, out LoginManagerSession session);

                if (session == null)
                {
                    session = LoginManager.LoginManagerInstance.GetLoginInformation(authenticationToken);
                    LoginManagerSessionTable.Add(authenticationToken, session);
                }
                return session;
            }
        }

        //-----------------------------------------------------------------------
        public static void RemoveLoginManagerSession(string authenticationToken)
        {
            lock (staticTicket)
            {
                LoginManagerSessionTable.Remove(authenticationToken);
            }
        }
    }

    /// <summary>
    /// Eccezione per gli errori del server
    /// </summary>
    //=================================================================================
    public class LoginManagerException : Exception
    {
        private LoginManagerError errorCode;
        public LoginManagerError ErrorCode { get { return errorCode; } }

        //-----------------------------------------------------------------------
        public LoginManagerException(LoginManagerError errorCode, string errorMessage)
            :
            base(errorMessage)
        {
            this.errorCode = errorCode;
        }

        //-----------------------------------------------------------------------
        public LoginManagerException(LoginManagerError errorCode)
            :
            this(errorCode, GetExceptionMessageFromErrorCode(errorCode))
        {
        }

        //-----------------------------------------------------------------------
        private static string GetExceptionMessageFromErrorCode(LoginManagerError errorCode)
        {
            switch (errorCode)
            {
                case LoginManagerError.NotLogged:
                    return WebServicesWrapperStrings.NotLoggedExceptionMessage;
                case LoginManagerError.UnInitialized:
                    return WebServicesWrapperStrings.UnInitializedExceptionMessage;
                case LoginManagerError.CommunicationError:
                    return WebServicesWrapperStrings.CommunicationErrorExceptionMessage;
                case LoginManagerError.GenericError:
                    return WebServicesWrapperStrings.GenericErrorExceptionMessage;
                default:
                    break;
            }

            return String.Empty;
        }
    }

    //================================================================================================
    public class LoginManagerSession : IDisposable
    {
        public string AuthenticationToken { get; internal set; }
        public string UserName { get; internal set; }
        public bool Admin { get; internal set; }
        public string CompanyName { get; internal set; }
        public string ConnectionString { get; internal set; }
        public string DbServer { get; internal set; }
        public string DbUser { get; internal set; }
        public string DbName { get; internal set; }
        public bool Security { get; internal set; }
        public bool Auditing { get; internal set; }
        public string Password { get; internal set; }
        public bool PasswordNeverExpired { get; internal set; }
        public DateTime ExpiredDatePassword { get; internal set; }
        public bool UserCannotChangePassword { get; internal set; }
        public bool ExpiredDateCannotChange { get; internal set; }
        public bool UserMustChangePassword { get; internal set; }
        public string ApplicationLanguage { get; internal set; }
        public string PreferredLanguage { get; internal set; }
        public string ProviderName { get; internal set; }
        public string ProviderDescription { get; internal set; }

        public bool UseUnicode { get; internal set; }

        public LoginManagerState LoginManagerSessionState { get; internal set; }

        private List<string> modules;
        private Dictionary<string, bool> activationList = null;

        //----------------------------------------------------------------------------
        public LoginManagerSession(string authenticationToken)
        {
            this.AuthenticationToken = authenticationToken;
            modules = GetModules();
            activationList = CreateActivationList();
        }

        //----------------------------------------------------------------------------
        public void Init(string authenticationToken)
        {
            this.AuthenticationToken = authenticationToken;
        }

        //----------------------------------------------------------------------------
        public void Dispose()
        {
            LoginManagerSessionManager.RemoveLoginManagerSession(AuthenticationToken);
            AuthenticationToken = string.Empty;
        }

        /// <summary>
        /// Verifica che l'articolo legato alla funzionalità richiesta sia stato acquistato
        /// </summary>
        /// <param name="application">Applicazione fisica</param>
        /// <param name="functionality">Funzionalità o modulo fisico</param>
        /// <returns>true se l'articolo è attivato</returns>
        //----------------------------------------------------------------------
        public bool IsActivated(string application, string functionality)
        {
            bool ret;
            if (activationList == null)
                throw new Exception("activationList is empty");

            string key = string.Format("{0}.{1}", application, functionality);
            if (activationList.TryGetValue(key, out ret))
                return ret;
            return false;
        }

        //----------------------------------------------------------------------
        private Dictionary<string, bool> CreateActivationList()
        {
            Dictionary<string, bool> list = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
            if (modules == null)
                throw new Exception("activationList is empty");

            foreach (string module in modules)
            {
                int appIndex = module.IndexOf('.');
                if (appIndex == -1)
                    continue;
                string app = module.Substring(0, appIndex);

                appIndex++;
                int modIndex = module.IndexOf('.', appIndex);

                string mod = modIndex == -1
                    ? module.Substring(appIndex)
                    : module.Substring(appIndex, modIndex - appIndex);
                list[string.Format("{0}.{1}", app, mod)] = true;
            }
            return list;
        }

        //---------------------------------------------------------------------------
        internal List<string> GetModules()
        {
            lock (this)
            {
                //modules contiene l'elenco dei module "standard",
                //viene inizializzata la prima volta che si effettua una chiamata a GetModules e poi mai più.
                //Ad ogni chiamata, invece, dobbiamo aggiungere l'elenco dei <Applicazione,modulo> che vengono
                //creati dalle customizzazione EasyBuilder.
                //Ciò è sensato perchè:
                //1)i moduli della Standard vengono modificati solamente installando per cui è plausibile
                //elencarli la prima volta e poi mai più
                //2)i module delle customizzazioni, invece, nascono e muoiono come funghi anche durante il funzionamento
                //del gestionale per cui la loro presenza va riverificata ad ogni chiamata GetModules.
                //L'effetto collaterale di questa scelta è che se nei metodi di questa classe si accede a modules anzichè chiamare GetModules
                //si lavora su un elenco di moduli che computa solo i moduli della Standard e non anche quelli delle customizzaizoni.
                modules = LoginManager.LoginManagerInstance.GetModules();
                 //le customizzazioni sono di default attivate
                foreach (BaseApplicationInfo bai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
                {
                    if (bai.ApplicationType != ApplicationType.Customization)
                        continue;

                    foreach (BaseModuleInfo bmi in bai.Modules)
                        modules.Add(bai.Name + "." + bmi.Name);
                }
                return modules;
            }
        }

    }

    //================================================================================================
    public class LoginManager
    {
        static Microarea.Common.WebServicesWrapper.LoginManager loginManagerInstance;
        static object staticLockTicket = new object();

        //----------------------------------------------------------------------------
        /// <summary>
        /// Oggetto statico globale BasePathFinder utilizzato ovunque in Mago.Net siano necessarie
        /// informazioni non dipendenti da username e company
        /// </summary>
        public static Microarea.Common.WebServicesWrapper.LoginManager LoginManagerInstance
        {
            get
            {
                lock (staticLockTicket)
                {
                    if (loginManagerInstance == null)
                    {
                        loginManagerInstance = new Common.WebServicesWrapper.LoginManager();
                    }
                    return loginManagerInstance;
                }
            }
        }

      
        private char[] activationExpressionOperators = new char[2] { '&', '|' };
        private char[] activationExpressionKeywords = new char[6] { '!', '&', '|', '(', ')', '?' };

        WCFLoginManager.MicroareaLoginManagerSoapClient loginManagerClient = new WCFLoginManager.MicroareaLoginManagerSoapClient(WCFLoginManager.MicroareaLoginManagerSoapClient.EndpointConfiguration.MicroareaLoginManagerSoap);
        private string baseUrl = "http://localhost:5000/";
        //private string loginManagerUrl;
        private int webServicesTimeOut;
    
        //-----------------------------------------------------------------------------------------
        public LoginManager()
        {
            ConfigureWebService();
        }

        //-----------------------------------------------------------------------------------------
        private void ConfigureWebService()
        {
             loginManagerClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(PathFinder.BasePathFinderInstance.LoginManagerUrl);
        }


        //-----------------------------------------------------------------------------------------
        public LoginManager(string loginManagerUrl, int webServicesTimeOut)
        {
            this.baseUrl = loginManagerUrl;
            this.webServicesTimeOut = webServicesTimeOut;

            ConfigureWebService();
        }

        //-----------------------------------------------------------------------------------------
        public int LoginCompact(string user, string company, string password, string askingProcess, bool overwriteLogin, out string authenticationToken)
        {

            WCFLoginManager.LoginCompactRequest request = new WCFLoginManager.LoginCompactRequest(user, company, password, askingProcess, overwriteLogin);
            Task<WCFLoginManager.LoginCompactResponse> task = loginManagerClient.LoginCompactAsync(request);
            int result = task.Result.LoginCompactResult;
            authenticationToken = task.Result.authenticationToken;
            //string errorMessage = "Error message"; // TODO read error message
            if (string.IsNullOrEmpty(authenticationToken))
                return result;

            LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);

            return result;
        }

        //-----------------------------------------------------------------------------------------
        public void LogOff(string authenticationToken)
        {
            Task task = loginManagerClient.LogOffAsync(authenticationToken);
            task.Wait();

            LoginManagerSessionManager.RemoveLoginManagerSession(authenticationToken);
        }

        //-----------------------------------------------------------------------------------------
        internal LoginManagerSession GetLoginInformation(string authenticationToken)
        {
            GetLoginInformationRequest request = new GetLoginInformationRequest(authenticationToken);
            Task<GetLoginInformationResponse> task = loginManagerClient.GetLoginInformationAsync(request);
            GetLoginInformationResponse result = task.Result;

            LoginManagerSession loginManagerSession = new LoginManagerSession(authenticationToken);
            loginManagerSession.UserName = result.userName;
            loginManagerSession.CompanyName = result.companyName;
            loginManagerSession.Admin = result.admin;
            loginManagerSession.ConnectionString = result.nonProviderCompanyConnectionString;
            loginManagerSession.UseUnicode = result.useUnicode;
            loginManagerSession.PreferredLanguage = result.preferredLanguage;
            loginManagerSession.ApplicationLanguage = result.applicationLanguage;
            loginManagerSession.ProviderName = result.providerName;
            loginManagerSession.ProviderDescription = result.providerDescription;
            loginManagerSession.LoginManagerSessionState = LoginManagerState.Logged;
            loginManagerSession.Security = result.security;
            loginManagerSession.DbServer = result.dbServer;
            loginManagerSession.DbUser = result.dbUser;
            loginManagerSession.DbName = result.dbName;
            return loginManagerSession;

        }

        //-----------------------------------------------------------------------------------------
        public List<string> GetModules()
        {
            Task<string[]> task = loginManagerClient.GetModulesAsync();
            List<string> modules = new List<string>();
            modules.AddRange(task.Result);
            return modules;
        }

        //-----------------------------------------------------------------------------------------
        public bool IsActivated(string application, string functionality)
        {
            Task<bool> task = loginManagerClient.IsActivatedAsync(application, functionality);
            return task.Result;
        }

        //-----------------------------------------------------------------------------------------
        public string[] EnumCompanies(string userName)
        {
            Task<string[]> task = loginManagerClient.EnumCompaniesAsync(userName);
            return task.Result;
        }

        //-----------------------------------------------------------------------------------------
        public bool IsDeveloperActivation()
        {
            Task<bool> task = loginManagerClient.CacheCounterAsync();
            return task.Result;
        }

        //-----------------------------------------------------------------------------------------
        public string GetConfigurationHash()
        {
            Task<string> task = loginManagerClient.GetConfigurationHashAsync();
            return task.Result;
        }


        //-----------------------------------------------------------------------------------------
        public string GetUserInfo()
        {
            Task<string> task = loginManagerClient.GetUserInfoAsync();
            return task.Result;
        }

        //-----------------------------------------------------------------------------------------
        public string GetEditionType()
        {
            Task<string> task = loginManagerClient.GetEditionTypeAsync();
            return task.Result;
        }

        //-----------------------------------------------------------------------------------------
        public TaskBuilderNetCore.Interfaces.SerialNumberType GetSerialNumberType()
        {
            Task<TaskBuilderNetCore.Interfaces.SerialNumberType> task = loginManagerClient.CacheCounterGTGAsync();
            return task.Result;
        }

        //----------------------------------------------------------------------------
        public String GetActivationStateInfo()
        {
            string sMessage;
            TaskBuilderNetCore.Interfaces.ActivationState actState = GetActivationState(out int daysToExpiration);

            switch (actState)
            {
                case TaskBuilderNetCore.Interfaces.ActivationState.Activated:
                    sMessage = EnumsStateStrings.Activated;
                    break;
                case TaskBuilderNetCore.Interfaces.ActivationState.Demo:
                case TaskBuilderNetCore.Interfaces.ActivationState.DemoWarning:
                    sMessage = EnumsStateStrings.DemoVersion;
                    break;
                case TaskBuilderNetCore.Interfaces.ActivationState.SilentWarning:
                    sMessage = EnumsStateStrings.SilentWarning;
                    break;
                case TaskBuilderNetCore.Interfaces.ActivationState.Warning:
                    sMessage = EnumsStateStrings.Warning;
                    break;
                case TaskBuilderNetCore.Interfaces.ActivationState.Disabled:
                    sMessage = EnumsStateStrings.Disabled;
                    break;
                default:
                    sMessage = EnumsStateStrings.NotActivated;
                    break;
            }

            // serial number type
            switch (GetSerialNumberType())
            {
                case TaskBuilderNetCore.Interfaces.SerialNumberType.Development:
                case TaskBuilderNetCore.Interfaces.SerialNumberType.DevelopmentIU:
                case TaskBuilderNetCore.Interfaces.SerialNumberType.PersonalPlusK:
                case TaskBuilderNetCore.Interfaces.SerialNumberType.DevelopmentPlusK:
                case TaskBuilderNetCore.Interfaces.SerialNumberType.DevelopmentPlusUser:
                case TaskBuilderNetCore.Interfaces.SerialNumberType.PersonalPlusUser:
                    sMessage += " " + EnumsStateStrings.SerialNumberDevelopment;
                    break;
                case TaskBuilderNetCore.Interfaces.SerialNumberType.Reseller:
                    sMessage += " " + EnumsStateStrings.SerialNumberReseller;
                    break;
                case TaskBuilderNetCore.Interfaces.SerialNumberType.Distributor:
                    sMessage += " " + EnumsStateStrings.SerialNumberDistributor;
                    break;
                default:
                    break;
            }

            return sMessage;
        }

        //-----------------------------------------------------------------------------------------
        public string GetInstallationVersion()
        {
            return GetInstallationVersion(out string productName, out DateTime buildDate, out DateTime instDate, out int build);
        }

        //-----------------------------------------------------------------------------------------
        public string GetInstallationVersion(out string productName, out DateTime buildDate, out DateTime instDate, out int build)
        {
            GetInstallationVersionRequest request = new GetInstallationVersionRequest();

            Task<GetInstallationVersionResponse> task = loginManagerClient.GetInstallationVersionAsync(request);
            productName = task.Result.productName;
            buildDate = task.Result.buildDate;
            instDate = task.Result.instDate;
            build = task.Result.build;

            return task.Result.GetInstallationVersionResult;
        }

        //-----------------------------------------------------------------------------------------
        internal bool IsEasyBuilderDeveloper(string authenticationToken)
        {
            Task<bool> task = loginManagerClient.IsEasyBuilderDeveloperAsync(authenticationToken);
            return task.Result;
        }

        //-----------------------------------------------------------------------------------------
        public bool IsAlive()
        {
            Task<bool> task = loginManagerClient.IsAliveAsync();
            return task.Result;
        }

        //-----------------------------------------------------------------------------------------
        public bool IsValidToken(string authToken)
        {
            Task<bool> task = loginManagerClient.IsValidTokenAsync(authToken);
            return task.Result;
        }

        //-----------------------------------------------------------------------------------------
        public void WakeUp()
        {
            Task<bool> task = loginManagerClient.IsAliveAsync();
            task.Start();
        }

        //---------------------------------------------------------------------------
        public int ChangePassword(string userName, string oldPassword, string newPassword)
        {
            Task<int> task = loginManagerClient.ChangePasswordAsync(userName, oldPassword, newPassword);
            int result = task.Result;
            //if (result == (int)LoginReturnCodes.NoError)
            //{
                
            //    //TODOLUCA
            //    //loginManagerSession.UserMustChangePassword = false;
            //    //loginManagerSession.ExpiredDatePassword = loginManagerSession.ExpiredDatePassword.AddDays(30);
            //    //loginManagerSession.Password = newPassword;
            //}
            return result;
        }

        //---------------------------------------------------------------------------
        internal bool IsRegistered(out string message, out TaskBuilderNetCore.Interfaces.ActivationState dummy)
        {
            message = "";
            dummy = TaskBuilderNetCore.Interfaces.ActivationState.Undefined;
            int daysToExpire = 0;

            TaskBuilderNetCore.Interfaces.ActivationState actState = GetActivationState(out daysToExpire);

            switch (actState)
            {
                case TaskBuilderNetCore.Interfaces.ActivationState.Disabled:
                    message = WebServicesWrapperStrings.ActivationStateDisabled;
                    return false;
                case TaskBuilderNetCore.Interfaces.ActivationState.NoActivated:
                    message = WebServicesWrapperStrings.ActivationStateNoActivated;
                    return false;
                case TaskBuilderNetCore.Interfaces.ActivationState.DemoWarning:
                case TaskBuilderNetCore.Interfaces.ActivationState.Demo:
                    {
                        /*string msg = null;
						if (daysToExpire == 0)
							msg = WebServicesWrapperStrings.ToDayExpire;
						else
							msg = String.Format(WebServicesWrapperStrings.DayToExpire, daysToExpire);
						message = string.Format(, msg);*/
                        message = WebServicesWrapperStrings.ActivationStateDemoNew;
                        return true;
                    }
                case TaskBuilderNetCore.Interfaces.ActivationState.Warning:
                    {
                        string msg = null;
                        if (daysToExpire == 0)
                            msg = WebServicesWrapperStrings.ToDayExpire;
                        else
                            msg = String.Format(WebServicesWrapperStrings.DayToExpire, daysToExpire);
                        message = string.Format(WebServicesWrapperStrings.ActivationStateWarning, msg);
                        return true;
                    }
                default:
                    return true;
            }
        }

        //---------------------------------------------------------------------------
        public TaskBuilderNetCore.Interfaces.ActivationState GetActivationState(out int daysToExpiration)
        {
            SetCurrentComponentsRequest request = new SetCurrentComponentsRequest();
            Task<SetCurrentComponentsResponse> task = loginManagerClient.SetCurrentComponentsAsync(request);
            daysToExpiration = task.Result.dte;
            return (TaskBuilderNetCore.Interfaces.ActivationState)task.Result.SetCurrentComponentsResult;
        }

        //---------------------------------------------------------------------------
        public string Ping()
        {
            Task<string> task = loginManagerClient.PingAsync();
            return task.Result;
        }

        //---------------------------------------------------------------------------
        public int ValidateUser(string username, string password, bool winNT)
        {
            ValidateUserRequest request = new ValidateUserRequest(username, password, winNT);
            Task<ValidateUserResponse> task = loginManagerClient.ValidateUserAsync(request);
            return task.Result.ValidateUserResult;
        }

        //---------------------------------------------------------------------------
        internal int Login(string userName, string company, string password, string askingProcess, bool overwriteLogin, out string authenticationToken, string macIp = null)
        {
            authenticationToken = string.Empty;
            //if (loginManagerState == LoginManagerState.UnInitialized)
            //    throw new LoginManagerException(LoginManagerError.UnInitialized);

            //if (loginManagerState == LoginManagerState.Logged && !overwriteLogin)
            //    return (int)LoginReturnCodes.UserAlreadyLoggedError;


            ////TODO ILARIA MACADDRESS
            //string macAddress = string.Empty;
            //try
            //{
            //    macAddress = LocalMachine.GetMacAddress() + "-**-" + LocalMachine.GetIPAddress();
            //}
            //catch { }
            //parametro morto
            string activationDB = string.Empty;
            authenticationToken = string.Empty;
            Login2Request request = new Login2Request(userName, company, password, askingProcess, macIp, overwriteLogin);
            Task<Login2Response> task = loginManagerClient.Login2Async(request);

            if (task.Result.Login2Result != (int)LoginReturnCodes.NoError)
                return task.Result.Login2Result;

            authenticationToken = task.Result.authenticationToken;
            LoginManagerSession loginManagerSession = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);

            loginManagerSession.CompanyName = company;
            loginManagerSession.LoginManagerSessionState = LoginManagerState.Logged;

            return (int)LoginReturnCodes.NoError;
        }

        //---------------------------------------------------------------------------
        internal void SSOLogOff(string cryptedtoken)
        {
            //throw new NotImplementedException();
        }

        //---------------------------------------------------------------------------
        internal int LoginViaInfinityToken(string cryptedtoken, string username, string password, string company)
        {
            //throw new NotImplementedException();
            return -1;
        }

        //---------------------------------------------------------------------------
        internal TaskBuilderNetCore.Interfaces.DBNetworkType GetDBNetworkType()
        {
            Task<TaskBuilderNetCore.Interfaces.DBNetworkType> task = loginManagerClient.GetDBNetworkTypeAsync();
            return task.Result;
        }

        internal float GetUsagePercentageOnDBSize(string connectionString)
        {  
            return Microarea.Common.Generic.InstallationInfo.Functions.GetDBPercentageUsedSize(connectionString);
        }

        /// <summary>
        /// Restituisce la stringa di connessione al database di sistema
        /// </summary>
        /// <param name="authenticationToken">token di autenticazione</param>
        /// <returns>Stringa di connessione al database di sistema in chiaro.</returns>
        //---------------------------------------------------------------------------
        public string GetSystemDBConnectionString(string authenticationToken)
        {
            if (String.IsNullOrWhiteSpace(authenticationToken))
                throw new LoginManagerException(LoginManagerError.NotLogged);

            LoginManagerSession loginManagerSession = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            if (loginManagerSession.LoginManagerSessionState != LoginManagerState.Logged )
                throw new LoginManagerException(LoginManagerError.NotLogged);

            Task<string> tesk = loginManagerClient.GetSystemDBConnectionStringAsync(authenticationToken);
            return tesk.Result;
        }

        //---------------------------------------------------------------------------
        internal bool IsSecurityLightEnabled()
        {
            Task<bool> task = loginManagerClient.IsSecurityLightEnabledAsync();
            return task.Result;
        }

        //---------------------------------------------------------------------------
        internal void RefreshSecurityStatus()
        {
            Task task = loginManagerClient.RefreshSecurityStatusAsync();
            task.Wait();
        }

        
        //---------------------------------------------------------------------------
        internal bool CheckActivationExpression(string currentApplicationName, string activationExpression)
        {
            if (activationExpression == null || activationExpression.Trim().Length == 0)
                return true;

            char currentOperator = '\0';

            string expression = activationExpression.Trim();
            bool expressionvalue = true;

            while (expression != null && expression.Length > 0)
            {
                bool tokenValue = true;

                int firstKeyIndex = expression.IndexOfAny(activationExpressionKeywords);

                if (firstKeyIndex >= 0)
                {
                    if (firstKeyIndex == 0 && expression[0] != '!' && expression[0] != '(' && expression[0] != '?')
                    {
                        // errore di sintassi: l'espressione non può cominciare con un'operatore di tipo '&', '|', ')'
                        Debug.Fail("Activation expression syntax error encountered in LoginManager.CheckActivationExpression.");
                        // non potendo testare correttamente l'attivazione sollevo un'eccezione
                        throw new LoginManagerException(LoginManagerError.GenericError, String.Format(WebServicesWrapperStrings.CheckActivationExpressionErrFmtMsg, expression));
                    }

                    if (expression[0] == '?')
                    {
                        string instPath = PathFinder.BasePathFinderInstance.GetInstallationPath();
                        return File.Exists(Path.Combine(instPath, expression.Substring(1).Replace('|', '\\')));
                    }

                    bool negateToken = (expression[0] == '!');
                    if (negateToken)
                        expression = expression.Substring(1).TrimStart();

                    int openingParenthesisCount = 0;

                    int charIndex = 0;
                    do
                    {
                        if (expression[charIndex] == '(')
                            openingParenthesisCount++;
                        else if (expression[charIndex] == ')')
                            openingParenthesisCount--;

                        if (openingParenthesisCount == 0)
                            break;

                        charIndex++;

                    } while (charIndex < expression.Length);// esco dal while solo se l'espressione è terminata
                    // o se ho chiuso tutte eventuali le parentesi tonde
                    if (openingParenthesisCount != 0)
                    {
                        // errore di sintassi: non c'è un matching corretto di parentesi
                        Debug.Fail("Activation expression syntax error encountered in LoginManager.CheckActivationExpression.");
                        // non potendo testare correttamente l'attivazione sollevo un'eccezione
                        throw new LoginManagerException(LoginManagerError.GenericError, String.Format(WebServicesWrapperStrings.CheckActivationExpressionErrFmtMsg, expression));
                    }

                    string token = String.Empty;

                    if (charIndex > 0)// il token comincia con una parentesi e charIndex punta all'ultima parentesi chiusa
                    {
                        token = expression.Substring(1, charIndex - 1).Trim();
                        expression = expression.Substring(charIndex + 1).Trim();
                    }
                    else
                    {
                        if (negateToken)
                        {
                            int tokenLength = expression.IndexOfAny(activationExpressionOperators);
                            if (tokenLength == -1)
                            {
                                token = expression;
                                expression = String.Empty;
                            }
                            else
                            {
                                token = expression.Substring(0, tokenLength).Trim();
                                expression = expression.Substring(tokenLength).Trim();
                            }
                        }
                        else
                        {
                            token = expression.Substring(0, firstKeyIndex).Trim();
                            expression = expression.Substring(firstKeyIndex).Trim();
                        }
                    }
                    if (currentOperator != '\0' && (token == null || token.Length == 0)) // non è il primo operando !
                    {
                        // errore di sintassi: l'espressione non può terminare con un'operatore di tipo '&', '|', ')'
                        Debug.Fail("Activation expression syntax error encountered in LoginManager.CheckActivationExpression.");
                        // non potendo testare correttamente l'attivazione sollevo un'eccezione
                        throw new LoginManagerException(LoginManagerError.GenericError, String.Format(WebServicesWrapperStrings.CheckActivationExpressionErrFmtMsg, expression));
                    }

                    tokenValue = CheckActivationExpression(currentApplicationName, token);

                    if (negateToken)
                        tokenValue = !tokenValue;
                }
                else
                {
                    tokenValue = CheckSingleActivation(currentApplicationName, expression);

                    expression = String.Empty;
                }

                if (currentOperator == '&')
                    expressionvalue = expressionvalue && tokenValue;
                else if (currentOperator == '|')
                    expressionvalue = expressionvalue || tokenValue;
                else if (currentOperator == '\0') // è il primo operando !
                    expressionvalue = tokenValue;

                if (expression == null || expression.Length == 0)
                    break;

                currentOperator = expression[0];

                if (!expressionvalue && currentOperator == '&')
                    return false;

                if (expressionvalue && currentOperator == '|')
                    return true;

                expression = expression.Substring(1).TrimStart();

                if (currentOperator != '\0' && (expression == null || expression.Length == 0))
                {
                    // errore di sintassi: l'espressione non può terminare con un'operatore di tipo '&', '|', ')'
                    Debug.Fail("Activation expression syntax error encountered in LoginManager.CheckActivationExpression.");
                    // non potendo testare correttamente l'attivazione sollevo un'eccezione
                    throw new LoginManagerException(LoginManagerError.GenericError, String.Format(WebServicesWrapperStrings.CheckActivationExpressionErrFmtMsg, expression));
                }
            }
            return expressionvalue;
        }


        //---------------------------------------------------------------------------
        public string GetJsonLoginInformation(string token)
        {
            LoginManagerSession loginManagerSession = LoginManagerSessionManager.GetLoginManagerSession(token);
            if (loginManagerSession == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);

            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("userName");
            jsonWriter.WriteValue(loginManagerSession.UserName);

            jsonWriter.WritePropertyName("companyName");
            jsonWriter.WriteValue(loginManagerSession.CompanyName);

            jsonWriter.WritePropertyName("admin");
            jsonWriter.WriteValue(loginManagerSession.Admin);

            jsonWriter.WritePropertyName("connectionString");
            jsonWriter.WriteValue(loginManagerSession.ConnectionString);

            jsonWriter.WritePropertyName("providerName");
            jsonWriter.WriteValue(loginManagerSession.ProviderName);

            jsonWriter.WritePropertyName("useUnicode");
            jsonWriter.WriteValue(loginManagerSession.UseUnicode);

            jsonWriter.WritePropertyName("preferredLanguage");
            jsonWriter.WriteValue(loginManagerSession.PreferredLanguage);

            jsonWriter.WritePropertyName("applicationLanguage");
            jsonWriter.WriteValue(loginManagerSession.ApplicationLanguage);

            jsonWriter.WriteEndObject();

            return sb.ToString();
        }

        //---------------------------------------------------------------------------
        private bool CheckSingleActivation(string currentApplicationName, string singleActivation)
        {
            string activationToCheck = singleActivation.Trim();
            if (activationToCheck == null || activationToCheck.Length == 0)
                return true;

            // In nome di default dell'applicazione contenente il modulo del quale si vuole
            // testare l'attivazione viene passato come argomento alla funzione
            string activationApplicationName = currentApplicationName;

            string activationModuleName = activationToCheck;

            // Se la stringa contenente l'attivazione da testare ha dei punti al suo interno,
            // vuol dire che essa rappresenta un namespace, cioè che è nella forma 
            // <nome_applicazione>.<nome_modulo>
            if (activationToCheck.IndexOf('.') >= 0)
            {
                NameSpace namespaceActivation = new NameSpace(activationToCheck, NameSpaceObjectType.Module);
                if (namespaceActivation.Application != null && namespaceActivation.Application.Length > 0)
                    activationApplicationName = namespaceActivation.Application;
                if (namespaceActivation.Module != null && namespaceActivation.Module.Length > 0)
                    activationModuleName = namespaceActivation.Module;
            }

            return IsActivated(activationApplicationName, activationModuleName);
        }

    }
}
