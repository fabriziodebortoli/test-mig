using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Web.Services;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;
using Microarea.WebServices.Core.WebServicesWrapper;
using Microarea.WebServices.LoginManager.Properties;

namespace Microarea.WebServices.LoginManager
{
    //=========================================================================
    [WebService(Namespace = "http://microarea.it/LoginManager/")]
    public class MicroareaLoginManager : System.Web.Services.WebService
    {
        //---------------------------------------------------------------------
        public MicroareaLoginManager()
        {
            //CODEGEN: This call is required by the ASP.NET Web Services Designer
            InitializeComponent();
        }

        #region Component Designer generated code

        //Required by the Web Services Designer 
        private IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        [WebMethod]
        //-----------------------------------------------------------------------
        public int Init(bool reboot, string authenticationToken)
        {
            try
            {
#if !DEBUG
			if (!LoginApplication.LoginEngine.IsValidTokenForConsole(authenticationToken))
				return (int)LoginReturnCodes.AuthenticationTypeError;
#endif
                int res = LoginApplication.LoginEngine.Init(
                    reboot,
                    new InitEventArgs(
                        InitReason.WebServiceCall,
                        LoginApplication.LoginEngine.LoginManagerVersion,
                        string.Empty
                        )
                    );


                return res;
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsAlive()
        {
            return true;
        }
        

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool VerifyDBSize()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.VerifyDBSize();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

         [WebMethod]
        //----------------------------------------------------------------------
        public string GetMobileToken(string token, int loginType)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetMobileToken(token, loginType);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        //metodo chiamato solo dai terminalini ad intervalli regolari per segnalare che lo slot è ancora vivo.
        public bool RefreshWMSSlot(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.RefreshWMSSlot(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return false;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsCalAvailable(string authenticationToken, string application, string functionality)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsCalAvailableInternal(authenticationToken, application, functionality);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsValidDate(string operationDate, out string maxDate)
        {
            maxDate = null;
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsValidDate(operationDate, out maxDate);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool PingNeeded(bool force)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.PingNeeded(force);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void SetClientData(ClientData cd)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.SetClientData(cd);
               
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

        }


        [WebMethod]
        //----------------------------------------------------------------------
        public bool SetCompanyInfo(string authToken, string aName, string aValue)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.SetCompanyInfo(authToken, aName, aValue);
                //dummy random return value
                return ((DateTime.Now.Ticks % 2) == 0);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }



        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsActivated(string application, string functionality)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsActivatedInternal(application, functionality);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]// metodo obsoleto, non esiste più asincrona, lasciato per retrocompatiblità
        //----------------------------------------------------------------------
        public bool IsSynchActivation()
        {
            return true;
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection GetModules()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetActivatedListInternal();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection GetCompanyUsers(string companyName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetCompanyUsers(companyName);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection GetNonNTCompanyUsers(string companyName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetCompanyUsers(companyName, true);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection GetCompanyRoles(string companyName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetCompanyRoles(companyName);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        } 
        

        [WebMethod]
        //----------------------------------------------------------------------
        public bool HasUserEBRoles(int companyName, int userName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.HasUserEBRoles(companyName, userName);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return false;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection GetUserRoles(string companyName, string userName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetUserRoles(companyName, userName);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection EnumAllUsers()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.EnumAllUsers();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection EnumAllCompanyUsers(int companyId, bool onlyNonNTUsers)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.EnumAllCompanyUsers(companyId, onlyNonNTUsers);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection GetRoleUsers(string companyName, string roleName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetRoleUsers(companyName, roleName);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public StringCollection EnumCompanies(string userName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.EnumCompanies(userName);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new StringCollection();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsIntegratedSecurityUser(string userName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsIntegratedSecurityUser(userName);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public int GetLoggedUsersNumber()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetLoggedUsersNumber();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return -1;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public int GetCompanyLoggedUsersNumber(int companyId)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetCompanyLoggedUsersNumber(companyId);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return -1;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetLoggedUsers()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetLoggedUsers(string.Empty);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return string.Empty;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetLoggedUsersAdvanced(string token)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetLoggedUsers(token);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return string.Empty;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void GetCalNumber(out int namedCal, out int gdiConcurrent, out int unnamedCal, out int officeCal, out int tpCal)
        {
            int wmsCal = 0; 
            int manufacturingCal = 0;
            GetCalNumber2(out namedCal, out gdiConcurrent, out unnamedCal, out officeCal, out tpCal, out wmsCal, out manufacturingCal);
        }

        //devo duplicare perchè è un metodo di interfaccia non posso cambiarlo  ....
        [WebMethod]
        //----------------------------------------------------------------------
        public void GetCalNumber2(out int namedCal, out int gdiConcurrent, out int unnamedCal, out int officeCal, out int tpCal, out int wmsCal, out int manufacturingCal)
        {
            tpCal = 0;
            namedCal = 0;
            unnamedCal = 0;
            officeCal = 0;
            gdiConcurrent = 0;
            wmsCal = 0; 
            manufacturingCal = 0;
            if (LoginApplication.LoginEngine.ActivationManager == null)
                return;

            Hashtable tpProducerCal;
            Hashtable CALNumberForArticle;
            LoginApplication.LoginEngine.ActivationManager.GetCalNumber(out namedCal, out gdiConcurrent, out unnamedCal, out officeCal, out tpCal, out wmsCal, out manufacturingCal, out tpProducerCal, out CALNumberForArticle);
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public int GetTokenProcessType(string token)
        {
            return LoginApplication.LoginEngine.GetTokenProcessType(token);
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void ReloadConfiguration()
        {
            LoginApplication.LoginEngine.InitActivation();
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public int ValidateUser
            (
            string userName,
            string password,
            bool winNtAuthentication,
            out	StringCollection userCompanies,
            out int loginId,
            out bool userCannotChangePassword,
            out bool userMustChangePassword,
            out DateTime expiredDatePassword,
            out bool passwordNeverExpired,
            out bool expiredDateCannotChange
            )
        {
            userCompanies = null;
            loginId = -1;
            userCannotChangePassword = false;
            userMustChangePassword = false;
            expiredDatePassword = DateTime.MaxValue;
            passwordNeverExpired = false;
            expiredDateCannotChange = false;

            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {

                return LoginApplication.LoginEngine.ValidateUser(
                        userName,
                        password,
                        winNtAuthentication,
                        out	loginId,
                        out userCompanies,
                        out	userCannotChangePassword,
                        out	userMustChangePassword,
                        out	expiredDatePassword,
                        out	passwordNeverExpired,
                        out	expiredDateCannotChange
                    );
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        //ritorno intero corrispondente ad enumerativo mobilecal
        [WebMethod]
        //----------------------------------------------------------------------
        public int ConsumeMobileCal(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return (int)LoginApplication.LoginEngine.ConsumeMobileCal(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public int ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.ChangePassword(userName, oldPassword, newPassword);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public int LoginCompact
            (
            ref string userName,
            ref string companyName,
            string password,
            string askingProcess,
            bool overWriteLogin,
            out string authenticationToken)
        {
            authenticationToken = string.Empty;
            bool admin = false;
            int companyId = -1;
            string dbName = string.Empty;
            string dbServer = string.Empty;
            int providerId = -1;
            bool security = false;
            bool auditing = false;
            bool useKeyedUpdate = false;
            bool transactionUse = false;
            string preferredLanguage = string.Empty;
            string applicationLanguage = string.Empty;
            string providerName = string.Empty;
            string providerDescription = string.Empty;
            bool useConstParameter = false;
            bool stripTrailingSpaces = false;
            string providerCompanyConnectionString = string.Empty;
            string nonProviderCompanyConnectionString = string.Empty;
            string dbUser = string.Empty;
            string activationDB = string.Empty;

            return Login
                (
                ref	userName,
                ref	companyName,
                password,
                askingProcess,
                overWriteLogin,
                out admin,
                out authenticationToken,
                out companyId,
                out dbName,
                out dbServer,
                out providerId,
                out security,
                out auditing,
                out useKeyedUpdate,
                out transactionUse,
                out preferredLanguage,
                out applicationLanguage,
                out providerName,
                out providerDescription,
                out useConstParameter,
                out stripTrailingSpaces,
                out providerCompanyConnectionString,
                out nonProviderCompanyConnectionString,
                out dbUser,
                out activationDB
                );
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void SSOLogOff(string cryptedToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.SSOLogOff(cryptedToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public int LoginViaInfinityToken2(string cryptedToken, string username, string password, string company, out string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.LoginViaInfinityToken(cryptedToken, out authenticationToken, username, password, company);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetIToken(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetIToken(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool ExistsSSOIDUser(string cryptedToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.ExistsSSOIDUser(cryptedToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
            return false;
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public int Login
            (
            ref string userName,
            ref string companyName,
            string password,
            string askingProcess,
            bool overWriteLogin,
            out bool admin,
            out string authenticationToken,
            out int companyId,
            out string dbName,
            out string dbServer,
            out int providerId,
            out bool security,
            out bool auditing,
            out bool useKeyedUpdate,
            out bool transactionUse,
            out string preferredLanguage,
            out string applicationLanguage,
            out string providerName,
            out string providerDescription,
            out bool useConstParameter,
            out bool stripTrailingSpaces,
            out string providerCompanyConnectionString,
            out string nonProviderCompanyConnectionString,
            out string dbUser,
            out string activationDB
            )
        {
            admin = false;
            authenticationToken = string.Empty;
            companyId = -1;
            dbName = string.Empty;
            dbServer = string.Empty;
            providerId = -1;
            security = false;
            auditing = false;
            useKeyedUpdate = false;
            transactionUse = false;
            preferredLanguage = string.Empty;
            applicationLanguage = string.Empty;
            providerName = string.Empty;
            providerDescription = string.Empty;
            useConstParameter = false;
            stripTrailingSpaces = false;
            providerCompanyConnectionString = string.Empty;
            nonProviderCompanyConnectionString = string.Empty;
            dbUser = string.Empty;
            activationDB = string.Empty;

            string macIp = null;
            int index = userName.IndexOf("<*>");
            if (index > -1)
            {
                macIp = userName.Substring(index + 3);
                userName = userName.Substring(0, index);
            }

            return Login2
                (
                ref	userName,
                ref	companyName,
                password,
                askingProcess,
                macIp,
                overWriteLogin,
                out admin,
                out authenticationToken,
                out companyId,
                out dbName,
                out dbServer,
                out providerId,
                out security,
                out auditing,
                out useKeyedUpdate,
                out transactionUse,
                out preferredLanguage,
                out applicationLanguage,
                out providerName,
                out providerDescription,
                out useConstParameter,
                out stripTrailingSpaces,
                out providerCompanyConnectionString,
                out nonProviderCompanyConnectionString,
                out dbUser,
                out activationDB);

        }

        /// <summary>
        /// Metodo Login che prende anche il macAddresss piu l'ipaddress, 
        /// ger gestire login su CAL singola da stessa postazione con stesso utente
        /// </summary>
        [WebMethod]
        //----------------------------------------------------------------------
        public int Login2
            (
            ref string userName,
            ref string companyName,
            string password,
            string askingProcess,
            string macIp,
            bool overWriteLogin,
            out bool admin,
            out string authenticationToken,
            out int companyId,
            out string dbName,
            out string dbServer,
            out int providerId,
            out bool security,
            out bool auditing,
            out bool useKeyedUpdate,
            out bool transactionUse,
            out string preferredLanguage,
            out string applicationLanguage,
            out string providerName,
            out string providerDescription,
            out bool useConstParameter,
            out bool stripTrailingSpaces,
            out string providerCompanyConnectionString,
            out string nonProviderCompanyConnectionString,
            out string dbUser,
            out string activationDB
            )
        {
            admin = false;
            authenticationToken = string.Empty;
            companyId = -1;
            dbName = string.Empty;
            dbServer = string.Empty;
            providerId = -1;
            security = false;
            auditing = false;
            useKeyedUpdate = false;
            transactionUse = false;
            preferredLanguage = string.Empty;
            applicationLanguage = string.Empty;
            providerName = string.Empty;
            providerDescription = string.Empty;
            useConstParameter = false;
            stripTrailingSpaces = false;
            providerCompanyConnectionString = string.Empty;
            nonProviderCompanyConnectionString = string.Empty;
            dbUser = string.Empty;
            activationDB = string.Empty;

            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {

                return LoginApplication.LoginEngine.Login
                    (
                    ref	userName,
                    ref	companyName,
                    password,
                    askingProcess,
                    macIp,
                    overWriteLogin,
                    out admin,
                    out authenticationToken,
                    out companyId,
                    out dbName,
                    out dbServer,
                    out providerId,
                    out security,
                    out auditing,
                    out useKeyedUpdate,
                    out transactionUse,
                    out preferredLanguage,
                    out applicationLanguage,
                    out providerName,
                    out providerDescription,
                    out useConstParameter,
                    out stripTrailingSpaces,
                    out providerCompanyConnectionString,
                    out nonProviderCompanyConnectionString,
                    out dbUser,
                    out activationDB
                    );
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //----------------------------------------------------------------------
        public bool ConfirmToken(string authenticationToken, string procType)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.ConfirmToken(authenticationToken, procType);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }
        
        [WebMethod]
        //----------------------------------------------------------------------
        public bool GetLoginInformation
            (
            string authenticationToken,
            out string userName,
            out int loginId,
            out string companyName,
            out int companyId,
            out bool admin,
            out string dbName,
            out string dbServer,
            out int providerId,
            out bool security,
            out bool auditing,
            out bool useKeyedUpdate,
            out bool transactionUse,
            out bool useUnicode,
            out string preferredLanguage,
            out string applicationLanguage,
            out string providerName,
            out string providerDescription,
            out bool useConstParameter,
            out bool stripTrailingSpaces,
            out string providerCompanyConnectionString,
            out string nonProviderCompanyConnectionString,
            out string dbUser,
            out string processName,
            out string userDescription,
            out string email,
            out bool easyBuilderDeveloper,
            out bool rowSecurity,
			out bool dataSynchro
			)
        {
            userName = string.Empty;
            loginId = -1;
            companyName = string.Empty;
            companyId = -1;
            admin = false;
            dbName = string.Empty;
            dbServer = string.Empty;
            providerId = -1;
            security = false;
            auditing = false;
            useKeyedUpdate = false;
            transactionUse = false;
            useUnicode = false;
            preferredLanguage = string.Empty;
            applicationLanguage = string.Empty;
            providerName = string.Empty;
            providerDescription = string.Empty;
            useConstParameter = false;
            stripTrailingSpaces = false;
            providerCompanyConnectionString = string.Empty;
            nonProviderCompanyConnectionString = string.Empty;
            dbUser = string.Empty;
            processName = string.Empty;
            userDescription = string.Empty;
            email = string.Empty;
            easyBuilderDeveloper = false;
            rowSecurity = false;
			dataSynchro = false;

			if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetLoginInformation
                    (
                    authenticationToken,
                    out userName,
                    out loginId,
                    out companyName,
                    out companyId,
                    out admin,
                    out dbName,
                    out dbServer,
                    out providerId,
                    out security,
                    out auditing,
                    out useKeyedUpdate,
                    out transactionUse,
                    out useUnicode,
                    out preferredLanguage,
                    out applicationLanguage,
                    out providerName,
                    out providerDescription,
                    out useConstParameter,
                    out stripTrailingSpaces,
                    out providerCompanyConnectionString,
                    out nonProviderCompanyConnectionString,
                    out dbUser,
                    out processName,
                    out userDescription,
                    out email,
                    out easyBuilderDeveloper,
                    out rowSecurity,
					out dataSynchro
					);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void LogOff(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.Logout(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetUserName(int loginId)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                string descri = string.Empty, email = string.Empty;
                return LoginApplication.LoginEngine.GetLoginName(loginId, out descri, out email);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetUserDescriptionById(int loginId)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            string descri = string.Empty, email = string.Empty;
            try
            {
                LoginApplication.LoginEngine.GetLoginName(loginId, out descri, out email);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

            return descri;
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetUserDescriptionByName(string login)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            string descri = string.Empty, email = string.Empty;
            try
            {
                int loginId;
                bool winNTAuth; bool webLogin; bool gdiLogin;
                bool bOk = LoginApplication.LoginEngine.GetLoginId(login, out loginId, out winNTAuth, out webLogin, out gdiLogin);
                if (bOk)
                    LoginApplication.LoginEngine.GetLoginName(loginId, out descri, out email);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

            return descri;
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetUserEMailByName(string login)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            string descri = string.Empty, email = string.Empty;
            try
            {
                int loginId;
                bool winNTAuth; bool webLogin; bool gdiLogin;
                bool bOk = LoginApplication.LoginEngine.GetLoginId(login, out loginId, out winNTAuth, out webLogin, out gdiLogin);
                if (bOk)
                    LoginApplication.LoginEngine.GetLoginName(loginId, out descri, out email);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

            return email;
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsFloatingUser(string loginName, out bool floating)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsFloatingUser(loginName, out floating);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsWebUser(string loginName, out bool web)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsWebUser(loginName, out web);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsWinNT(int loginId)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsWinNT(loginId);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetDbOwner(int companyId)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetDbOwner(companyId);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsCompanySecured(int companyId)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsCompanySecured(companyId);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool GetAuthenticationInformations(string authenticationToken, out int loginId, out int companyId, out bool webLogin)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetAuthenticationInformations(authenticationToken, out loginId, out companyId, out webLogin);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool GetAuthenticationNames(string authenticationToken, out string userName, out string companyName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetAuthenticationNames(authenticationToken, out userName, out companyName);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool DeleteAssociation(int loginId, int companyId, string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                if (LoginApplication.LoginEngine.IsValidTokenForConsole(authenticationToken))
                {
                    LoginApplication.LoginEngine.DeleteAssociation(loginId, companyId);
                    return true;
                }

                if (!LoginApplication.LoginEngine.IsValidToken(authenticationToken))
                    return false;

                LoginApplication.LoginEngine.DeleteAssociation(loginId, companyId);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

            return true;
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool DeleteUser(int loginId, string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                if (LoginApplication.LoginEngine.IsValidTokenForConsole(authenticationToken))
                {
                    LoginApplication.LoginEngine.DeleteUser(loginId);
                    return true;
                }

                if (!LoginApplication.LoginEngine.IsValidToken(authenticationToken))
                    return false;

                LoginApplication.LoginEngine.DeleteUser(loginId);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

            return true;
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool DeleteCompany(int companyId, string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                if (LoginApplication.LoginEngine.IsValidTokenForConsole(authenticationToken))
                {
                    LoginApplication.LoginEngine.DeleteCompany(companyId);
                    return true;
                }

                if (!LoginApplication.LoginEngine.IsValidToken(authenticationToken))
                    return false;

                LoginApplication.LoginEngine.DeleteCompany(companyId);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
            return true;
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetSystemDBConnectionString(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetSystemDBConnectionString(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetDMSConnectionString(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetDMSConnectionString(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool Sql2012Allowed(string authToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.Sql2012Allowed(authToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }
             

        [WebMethod]
        //---------------------------------------------------------------------------
        public List<DmsDatabaseInfo> GetDMSDatabasesInfo(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetDMSDatabasesInfo(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

		[WebMethod]
		//---------------------------------------------------------------------------
		public List<DataSynchroDatabaseInfo> GetDataSynchroDatabasesInfo(string authenticationToken)
		{
			if (!LoginApplication.LoginEngine.TryLockResources())
				throw new Exception(LoginManagerStrings.ResourcesTimeout);

			try
			{
				return LoginApplication.LoginEngine.GetDataSynchroDatabasesInfo(authenticationToken);
			}
			catch (Exception exc)
			{
				LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				LoginApplication.LoginEngine.ReleaseResources();
			}
		}

		[WebMethod]
        //---------------------------------------------------------------------------
        public List<TbSenderDatabaseInfo> GetCompanyDatabasesInfo(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetCompanyDatabasesInfo(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetEdition()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetEdition();

            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return Edition.Undefined.ToString();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //---------------------------------------------------------------------------
        public byte[] GetConfigurationStream()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetConfigurationStream();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new byte[] { };
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetCountry()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetCountry();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return String.Empty;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetProviderNameFromCompanyId(int companyId)
        {
            string companyName = string.Empty;
            string dbName = string.Empty;
            string dbServer = string.Empty;
            int providerId = -1;
            int port = 0;

            bool useConstParameter = false;
            bool stripTrailingSpaces = false;
            string providerName = string.Empty;
            string providerDescription = string.Empty;

            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                if (!LoginApplication.LoginEngine.GetCompanyInfo(companyId, out companyName, out dbName, out dbServer, out providerId, out port))
                    return string.Empty;

                if (!LoginApplication.LoginEngine.GetProviderInfo(providerId, out providerName, out providerDescription, out useConstParameter, out stripTrailingSpaces))
                    return string.Empty;

                return providerName;
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetInstallationVersion(out string productName, out DateTime buildDate, out DateTime instDate, out int build)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                productName = LoginApplication.LoginEngine.InstallationVer.ProductName;
                buildDate = LoginApplication.LoginEngine.InstallationVer.BDate;
                instDate = LoginApplication.LoginEngine.InstallationVer.IDate;
                build = LoginApplication.LoginEngine.InstallationVer.Build;

                return LoginApplication.LoginEngine.InstallationVer.Version;
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetUserInfo()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetUserInfoString();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetUserInfoID()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetUserInfoID();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public void TraceAction
            (
            string company,
            string login,
            int type,
            string processName,
            string winUser,
            string location
            )
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.TraceAction(company, login, (TraceActionType)type, processName, winUser, location);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool HasUserAlreadyChangedPasswordToday(string user)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.HasUserAlreadyChangedPasswordToday(user);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetBrandedApplicationTitle(string application)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetBrandedApplicationTitle(application);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        /// <summary>
        /// Returns the branded name of the master product in the installation.
        /// </summary>
        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetMasterProductBrandedName()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetMasterProductBrandedName();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        /// <summary>
        /// Returns the branded name of the master solution in the installation.
        /// </summary>
        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetMasterSolutionBrandedName()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetMasterSolutionBrandedName();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetMasterSolution()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetMasterSolution();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }
        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetBrandedProducerName()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetBrandedProducerName();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetBrandedProductTitle()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetBrandedProductTitle();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }
        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetBrandedKey(string source)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetBrandedKey(source);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public DBNetworkType GetDBNetworkType()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetDBNetworkType();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetDatabaseType(string providerName)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return TBDatabaseType.GetDBMSType(providerName).ToString();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool CanUseNamespace(string nameSpace, string authenticationToken, GrantType grantType)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.CanUseNamespace(nameSpace, authenticationToken, grantType);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool CacheCounter()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsDeveloperActivationInternal();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public SerialNumberType CacheCounterGTG()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetSerialNumberType();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public ActivationState SetCurrentComponents(out int dte)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetActivationStateInternal(out dte);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool IsVirginActivation()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsVirginActivation();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public int HD()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            int dte = 0;
            try
            {
                LoginApplication.LoginEngine.GetActivationStateInternal(out dte);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

            return dte;
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void StoreMLUChoice(bool userChoseMluInChargeToMicroarea)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.StoreMLUChoice(userChoseMluInChargeToMicroarea);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool SaveLicensed(string xml, string name)
        {
            return LoginApplication.LoginEngine.SaveLicensed(xml, name);
        }

         [WebMethod]
        //----------------------------------------------------------------------
        public string ValidateIToken(string itoken, string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.ValidateIToken(itoken, authenticationToken);
            }
            catch (Exception exc)
            {
                string err = MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString();
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, err);
                    return err;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
            
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool SaveUserInfo(string xml)
        {
            return LoginApplication.LoginEngine.SaveUserInfo(xml);
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void DeleteUserInfo()
        {
            LoginApplication.LoginEngine.DeleteUserInfo();
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void DeleteLicensed(string name)
        {
            LoginApplication.LoginEngine.DeleteLicensed(name);
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public string PrePing()
        {
            return Ping(1717);
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public string Ping()
        {
            return Ping(666);
        }

        //----------------------------------------------------------------------
        private string Ping(int val)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            string res = string.Empty;

            try
            {
                res = LoginApplication.LoginEngine.PingInternal(val);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

            try
            {//non riavvio se l'attivazione ha dato errori.
                if (LoginApplication.LoginEngine.RefreshStatus)
                    LoginApplication.LoginEngine.Init(
                        false,
                        new InitEventArgs(
                            InitReason.ProductRegistration,
                            LoginApplication.LoginEngine.LoginManagerVersion,
                            string.Empty
                            )
                        );
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
            }

            return res;
        }
        

        [WebMethod]
        //----------------------------------------------------------------------
        public ModuleNameInfo[] GetArticlesWithNamedCal()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetArticlesWithNamedCal();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }
        
          [WebMethod]
        //----------------------------------------------------------------------
        public ModuleNameInfo[] GetArticlesWithFloatingCal()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetArticlesWithFloatingCal();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

          [WebMethod]
          //----------------------------------------------------------------------
          public void RefreshFloatingMark()
          {
              if (!LoginApplication.LoginEngine.TryLockResources())
                  throw new Exception(LoginManagerStrings.ResourcesTimeout);

              try
              {
                  LoginApplication.LoginEngine.RefreshFloatingMark();
              }
              catch (Exception exc)
              {
                  LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                  throw;
              }
              finally
              {
                  LoginApplication.LoginEngine.ReleaseResources();
              }
          }



        [WebMethod]
        //----------------------------------------------------------------------
        public void RefreshSecurityStatus()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.RefreshSecurityStatus();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public int GetProxySupportVersion()
        {
            return 1;
        }

        [WebMethod]
        //---------------------------------------------------------------------
        public ProxySettings GetProxySettings()
        {
            try
            {
                return LoginApplication.LoginEngine.GetProxySettings();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }


        }

        [WebMethod]
        //---------------------------------------------------------------------
        public void SetProxySettings(ProxySettings proxySettings)
        {
            try
            {
                LoginApplication.LoginEngine.SetProxySettings(proxySettings);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
        }


        [WebMethod]
        //----------------------------------------------------------------------
        public bool GetCompanyLanguage(int companyID, out string cultureUI, out string culture)
        {
            cultureUI = culture = String.Empty;

            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetCompanyLanguage(companyID, out cultureUI, out culture);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return false;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        // //DRAFT
        //Da 3.9.2 (aprile 2013) non è più possibile utilizzare Mago in data applicazione successiva alla scadenza mlu se mlu è scaduto o disdettato 
        //di conseguenza facciamo il controllo  in cambio data applicazione ma anche qui, che viene chiamata da tb ogni 5 minuti, perchè il cambio data potrebbe essere fatto prima che loginamanager reperisca 
        //il dato inviato dal ping ( il ping può essere fatto fino ad un'ora entro l'avvio!)
       //la data di mlu scaduto viene salvata criptata dal ping in un tag dentro le userinfo, invece tramite sms arriva un codice che indica l'elapsed days  a partire dal 1/1/2000 e che fa quindi riferimento alla data di scadenza.
        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsValidUpToDateToken(string authenticationToken, string mluexpired)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsValidToken(authenticationToken, mluexpired);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return false;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsValidToken(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsValidToken(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return false;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //----------------------------------------------------------------------
        public void ReloadUserArticleBindings(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                if (LoginApplication.LoginEngine.IsValidTokenForConsole(authenticationToken))
                    LoginApplication.LoginEngine.ReloadUserArticleBindings();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool FEUsed(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.FEUsed(authenticationToken);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return false;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool Sbrill(string token)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.CanTokenRunTB(token);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return false;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public LoginSlotType GetCalType(string token)//todo sarebbe da cambiare il nome....
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetSlotType(token);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return LoginSlotType.Invalid;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

		[WebMethod]
		//----------------------------------------------------------------------
		public bool IsUserLoggedByName(string userName)
		{
			if (!LoginApplication.LoginEngine.TryLockResources())
				throw new Exception(LoginManagerStrings.ResourcesTimeout);

			string descri = string.Empty, email = string.Empty;
			try
			{
				int loginId;
				bool winNTAuth; bool webLogin; bool gdiLogin;
				bool bOk = LoginApplication.LoginEngine.GetLoginId(userName, out loginId, out winNTAuth, out webLogin, out gdiLogin);

				return LoginApplication.LoginEngine.IsUserLogged(loginId);
			}
			catch (Exception exc)
			{
				LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				throw;
			}
			finally
			{
				LoginApplication.LoginEngine.ReleaseResources();
			}
		}

        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsUserLogged(int loginID)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsUserLogged(loginID);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsSecurityLightEnabled()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.SecurityLightEnabled;
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsSecurityLightAccessAllowed(string nameSpace, string authenticationToken, bool unattended)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsSecurityLightAccessAllowed(nameSpace, authenticationToken, unattended);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public int GetDBCultureLCID(int companyID)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetDBCultureLCID(companyID);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }
        [WebMethod]
        //----------------------------------------------------------------------
        public void SetMessageRead(string userName, string messageID)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.SetMessagesRead(userName, messageID);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());

            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        [WebMethod]
        //----------------------------------------------------------------------
        public Advertisement[] GetImmediateMessagesQueue(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                IList messages = LoginApplication.LoginEngine.GetImmediateMessagesQueue(authenticationToken);
                if (messages == null || messages.Count == 0)
                    return new Advertisement[] { };

                ArrayList results = new ArrayList(messages);
                return results.ToArray(typeof(Advertisement)) as Advertisement[];
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new Advertisement[] { };
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public Advertisement[] GetMessagesQueue(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                IList messages = LoginApplication.LoginEngine.GetMessagesQueue(authenticationToken);
                if (messages == null || messages.Count == 0)
                    return new Advertisement[] { };

                ArrayList results = new ArrayList(messages);
                return results.ToArray(typeof(Advertisement)) as Advertisement[];
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new Advertisement[] { };
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public Advertisement[] GetOldMessages(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                ArrayList messages = LoginApplication.LoginEngine.GetOldMessages(authenticationToken);
                if (messages == null || messages.Count == 0)
                    return new Advertisement[] { GetEmptyMessage() };

                return messages.ToArray(typeof(Advertisement)) as Advertisement[];
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                return new Advertisement[] { GetEmptyMessage() };
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void DeleteMessageFromQueue(string messageID)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.DeleteMessageFromQueue(messageID);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void PurgeMessageByTag(string tag, string user)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.PurgeMessageByTag(tag, user);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }


        //----------------------------------------------------------------------
        private Advertisement GetEmptyMessage()
        {
            return new Advertisement(LoginManagerStrings.NoMessage, "", LoginManagerStrings.NoMessage, false, DateTime.Today, MessageType.Advrtsm, 0, Guid.NewGuid().ToString());
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool SendAccessMail()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.SendAccessMail();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetAspNetUser()
        {
            return LocalMachine.GetProcessOwner(Process.GetCurrentProcess().Id);
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public string GetConfigurationHash()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetConfigurationHash();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool UserCanAccessWebSitePrivateArea(int loginId)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.UserCanAccessWebSitePrivateArea(loginId);
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool IsEasyBuilderDeveloper(string authenticationToken)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.IsEasyBuilderDeveloper(authenticationToken);
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool SendErrorFile(string LogFile, out string ErrorMessage)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var pathFinder = BasePathFinder.BasePathFinderInstance;

                    CrashInfo info = new CrashInfo();
                    info.UserCode = LoginApplication.LoginEngine.ActivationManager.User.GetUserId();

                    info.InstallationName = InstallationData.InstallationName;
                    info.Version = pathFinder.GetInstallationVersionFromInstallationVer();
                    info.LogFile = LogFile;

                    info.LogFileContent = File.ReadAllBytes(LogFile);
                    info.Unparse(ms);

                    using (CrashServer.Registration server = new CrashServer.Registration())
                    {
                        string globalFile = pathFinder.GetProxiesFilePath();
                        ProxySettings.SetRequestCredentials(server, globalFile);
                        server.StoreCrashInfo(ms.ToArray());
                    }
                    ErrorMessage = "";
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.ToString();
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ErrorMessage);
                return false;

            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public bool DownloadPdb(string PdbFile, out string ErrorMessage)
        {
            //serializzo la chiamata (non uso la TryLockResources perché non voglio bloccare la normale
            //attività di login manager (il download del pdb può impiegare qualche secondo)
            lock (typeof(CrashServer.Registration))
            {
                try
                {
                    var pathFinder = BasePathFinder.BasePathFinderInstance;

                    string version = pathFinder.GetInstallationVersionFromInstallationVer();
                    string path = pathFinder.GetCustomDebugSymbolsPath();
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string file = Path.Combine(path, PdbFile);
                    string verFile = file + ".ver";
                    if (File.Exists(file) && File.Exists(verFile))//se trovo il pdb, apro il file che ne determina la versione
                    {
                        string ver = File.ReadAllText(verFile);
                        if (ver == version)
                        {
                            ErrorMessage = "";
                            return true;
                        }
                    }
                    byte[] pdbStream = null;

                    using (CrashServer.Registration server = new CrashServer.Registration())
                    {
                        string globalFile = pathFinder.GetProxiesFilePath();
                        ProxySettings.SetRequestCredentials(server, globalFile);
                        pdbStream = server.GetPdb(PdbFile, version);
                    }

                    if (pdbStream.Length == 0)
                    {
                        ErrorMessage = "Cannot download pdb file: " + PdbFile;
                        LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ErrorMessage);
                        return false;
                    }
                    //decomprimo il pdb
                    using (GZipStream stream = new GZipStream(new MemoryStream(pdbStream), CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        using (FileStream fs = new FileStream(file, FileMode.Create))
                        {
                            int nRead = 0;
                            do
                            {
                                nRead = stream.Read(buffer, 0, buffer.Length);
                                fs.Write(buffer, 0, nRead);
                            }
                            while (nRead > 0);
                        }
                    }

                    File.WriteAllText(verFile, version);

                    ErrorMessage = "";
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.ToString();
                    LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ErrorMessage);
                    return false;

                }
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public string GetMainSerialNumber()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetMainSerialNumber();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

        }

        [WebMethod]
        //----------------------------------------------------------------------
        public string GetMLUExpiryDate()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetMLUExpiryDate();
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //----------------------------------------------------------------------
        public void SendBalloon(string authenticationToken, string bodyMessage, MessageType messageType = MessageType.Updates, List<string> recipients = null)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.SendBalloon(authenticationToken, bodyMessage, messageType, recipients);
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        ////=========================================================================
        //enum MessageType
        //    None = 0x0, Contract = 0x1, Advrtsm = 0x2, Updates = 0x4, PostaLite = 0x8
        //enum MessageSensation
        //    Information, ResultGreen, Warning, Error, AccessDenied, Help
        //----------------------------------------------------------------------
        public void AdvancedSendBalloon(string authenticationToken,
                string bodyMessage,
                DateTime expiryDate,
                int /*MessageType*/ messageType = 0,//MessageType.Updates,
                string[] recipients = null,
                int /*MessageSensation*/ sensation = 0,// MessageSensation.Information,
                bool historicize = true,
                bool immediate = false,
                int timer = 0)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.AdvancedSendBalloon(
                 authenticationToken,
                 bodyMessage,
                 expiryDate,
                 (MessageType)messageType,
                 recipients,
                 (MessageSensation)sensation,
                 historicize,
                 immediate,
                 timer);
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

    


        //----------------------------------------------------------------------
        [WebMethod]
        public void AdvancedSendTaggedBalloon(string authenticationToken,
                        string bodyMessage,
                        DateTime expiryDate,
                        int /*MessageType*/ messageType = 0,//MessageType.Updates,
                        string[] recipients = null,
                        int /*MessageSensation*/ sensation = 0,// MessageSensation.Information,
                        bool historicize = true,
                        bool immediate = false,
                        int timer = 0,
                        string tag = null)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                LoginApplication.LoginEngine.AdvancedSendBalloon(
                 authenticationToken,
                 bodyMessage,
                 expiryDate,
                 (MessageType)messageType,
                 recipients,
                 (MessageSensation)sensation,
                 historicize,
                 immediate,
                 timer, tag);
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool SetChannelFree(string authenticationToken, string[] channelCode)
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.SetChannelFree(authenticationToken, channelCode);
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }

        }

        [WebMethod]
        //---------------------------------------------------------------------------
        public bool GetChannelFree()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetChannelFree();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }

      
      
   
        [WebMethod]
        //---------------------------------------------------------------------------
        public string GetEditionType()
        {
            if (!LoginApplication.LoginEngine.TryLockResources())
                throw new Exception(LoginManagerStrings.ResourcesTimeout);

            try
            {
                return LoginApplication.LoginEngine.GetEditionType();
            }
            catch (Exception exc)
            {
                LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
                throw;
            }
            finally
            {
                LoginApplication.LoginEngine.ReleaseResources();
            }
        }
        //[WebMethod]
        ////----------------------------------------------------------------------
        //public bool SendCerberoLogFile(string LogFile, out string ErrorMessage)
        //{
        //    try
        //    {
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            CrashInfo info = new CrashInfo();
        //            info.UserCode = LoginApplication.LoginEngine.ActivationManager.User.GetUserId();

        //            info.InstallationName = InstallationData.InstallationName;
        //            info.Version = BasePathFinder.BasePathFinderInstance.GetInstallationVersionFromInstallationVer();
        //            info.LogFile = LogFile;

        //            info.LogFileContent = File.ReadAllBytes(LogFile);
        //            info.Unparse(ms);

        //            using (CrashServer.Registration server = new CrashServer.Registration())
        //            {
        //                server.StoreCerberoInfo(ms.ToArray());
        //            }
        //            ErrorMessage = "";
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage = ex.ToString();
        //        LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ErrorMessage);
        //        return false;
        //    }
        //}



    }
}


/* [WebMethod]
 //----------------------------------------------------------------------
 public void SetBalloonInfo(string authenticationToken, MessageType messageType, bool block)
 {
     if (!LoginApplication.LoginEngine.TryLockResources())
         throw new Exception(LoginManagerStrings.ResourcesTimeout);

     try
     {
         LoginApplication.LoginEngine.Set
 * Info(authenticationToken, messageType, block);
               
     }
     catch (Exception exc)
     {
         LoginApplication.LoginEngine.diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
         return ;
     }
     finally
     {
         LoginApplication.LoginEngine.ReleaseResources();
     }
 }
 */

