using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SerializableTypes;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.GenericForms
{
	//<summary>
	//Controllo per la gestione della login di uno Smart Client
	// </summary>
	//=========================================================================
	public class LoginFacilities
	{
		public static LoggedUser LatestUser = LoggedUser.Load();
		public static LoginManager loginManager = new LoginManager();

		private string fullLoginName = String.Empty;
		private string company = String.Empty;
		private string password = String.Empty;
		private Diagnostic diagnostic = new Diagnostic("LoginControl");
		private bool canusewinnt;
		private bool rememberMe;
		private bool winNTSelected = false;

		public string processType = ProcessType.MenuManager;
		public bool autologinable = true;
		public string NTLoginName = String.Empty;
		public string NTcompany = String.Empty;
		public string NTpassword = String.Empty;
		public bool ClearCachedDataVisible = false;

		#region proprietà
		//-----------------------------------------------------------------------
		public bool WinNTSelected
		{
			get { return winNTSelected; }
		}

		//-----------------------------------------------------------------------
		public bool Canusewinnt
		{
			get { return canusewinnt; }
		}

		//-----------------------------------------------------------------------
		public bool RememberMe
		{
			get { return rememberMe; }
		}

		//-----------------------------------------------------------------------
		public string User
		{
			get { return fullLoginName; }
		}

		//-----------------------------------------------------------------------
		public string Password
		{
			get { return password; }

		}
		//-----------------------------------------------------------------------
		public string Company
		{
			get { return company; }
		}

		//-----------------------------------------------------------------------
		public bool Autologinable
		{
			get
			{
				return InstallationData.ServerConnectionInfo.UseAutoLogin && this.autologinable;
			}
			set
			{
				this.autologinable = value && InstallationData.ServerConnectionInfo.UseAutoLogin;
				if (!this.autologinable)
					password = String.Empty;
			}
		}

		//-----------------------------------------------------------------------
		public Diagnostic Diagnostic
		{
			get
			{
				return diagnostic;
			}
		}

		#endregion

		/// <summary>
		/// Costruttore
		/// </summary>
		public LoginFacilities()
		{
		}


		#region Funzioni varie


		/// <summary>
		/// Si connette a login manager per inizializzare i controlli.
		/// Si autoimposta sull'ultimo utente/azienda che ha effettuato una login
		/// </summary>
		/// <param name="basePathFinder">Un path finder inizializzato</param>
		/// <returns>true se ha avuto successo</returns>
		//-----------------------------------------------------------------------
		public bool Load()
		{
			if (!CheckLoginManager())
				return false;

			ManageClientDataAsynch();

			try
			{
				ClearCachedDataVisible = loginManager.IsDeveloperActivation();
			}
			catch
			{
				ClearCachedDataVisible = false;
			}
			canusewinnt = CanUseWinAuthentication();
			if (canusewinnt)//ok wint authentication
			{
				if (string.Compare(LatestUser.User, fullLoginName, true, CultureInfo.InvariantCulture) == 0)
				{
					winNTSelected = true;
					NTcompany = LatestUser.Company;
				}
				else
				{//si può usare la wintauth ma non è il last user, 
				 //quindi imposto il last user e lascio selezionabile la winath, 
				 //con i dati salvati nelle variabili apposite
					winNTSelected = false;
					fullLoginName = LatestUser.User;
					company = LatestUser.Company;
					if (Autologinable && LatestUser.Remember)
						password = LatestUser.Password;
					else password = String.Empty;
				}

			}
			else//non si può usare wintaut
			{
				//il last user era l utente nt che però non si può usare = pulisco
				if (string.Compare(LatestUser.User, fullLoginName, true, CultureInfo.InvariantCulture) == 0)
					ClearData();


				//imposto dati come il last user
				if ((LatestUser.User != null && LatestUser.User.Length > 0) && (ExistCompanies(LatestUser.User)))
				{

					fullLoginName = LatestUser.User;
					company = LatestUser.Company;
					if (Autologinable && LatestUser.Remember)
						password = LatestUser.Password;

				}
				else
					ClearData();

			}
			rememberMe =  (Autologinable && LatestUser.Remember) ;
			return true;
		}

		//-----------------------------------------------------------------------
		private void ClearData()
		{
			LatestUser.User = fullLoginName = password = company = String.Empty;
			LatestUser.Remember = false;
		}

		//---------------------------------------------------------------------
		private bool CheckLoginManager()
		{
			try
			{
				loginManager.IsAlive();
			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.ServerDown, exc.Message));
				return false;
			}

			if (!File.Exists(BasePathFinder.BasePathFinderInstance.ServerConnectionFile))
			{
				diagnostic.Set(DiagnosticType.Information, WebServicesWrapperStrings.RunConsoleFirst);
				diagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.ErrFileNotExists, BasePathFinder.BasePathFinderInstance.ServerConnectionFile));
				return false;
			}

			string systemDBConnectionString = InstallationData.ServerConnectionInfo.SysDBConnectionString;
			if (systemDBConnectionString == null || systemDBConnectionString.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.RunConsoleFirst);
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		private bool ExistCompanies(string user, string company = null)
		{
			string[] companies;
			try { companies = loginManager.EnumCompanies(user); }
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.ToString());
				return false;
			}
            if (company.IsNullOrWhiteSpace()) //se non ho specificato nessuna company torno la sola esistenza di almeno una company
			    return companies != null && companies.Length > 0;
            foreach (string s in companies)//altrimenti verifico che la company passata esista nella lista.
                if (string.Compare(company, s, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return true;
            return false;
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public static void SetRememberMe(string checkedVal)
		{
			bool rememberMe = String.Compare(checkedVal, bool.TrueString, true) == 0 ? true : false;
			LatestUser.Remember = rememberMe;

			if (loginManager.LoginManagerState == LoginManagerState.Logged)
				SaveLatestUser(loginManager.UserName, loginManager.Password, loginManager.CompanyName, rememberMe);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public static bool GetRememberMe()
		{
			return LatestUser.Remember;
		}


        //------------------------------------------------------------------------------------------------------------------------------
        public static bool IsAutoLoginable()
        {
            return InstallationData.ServerConnectionInfo.UseAutoLogin;
        }

        //---------------------------------------------------------------------
        private bool CanUseWinAuthentication()
		{
			try
			{
				//prendo i dati di user attualmente connesso a windows
				string computerName = Dns.GetHostName();

				string loginName = Environment.UserName;
				string userDomainName = Environment.UserDomainName;


				fullLoginName = userDomainName + Path.DirectorySeparatorChar + loginName;

				if (userDomainName.Length == 0)
					fullLoginName = computerName + Path.DirectorySeparatorChar + loginName;


				bool ok = ExistCompanies(fullLoginName);

				if (ok)
					NTLoginName = fullLoginName;
				return ok;
			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.ToString());
				return false;
			}

		}

		//---------------------------------------------------------------------
		public string ChangePassword(string newPassword)
		{
			try
			{
				int changePwdResult = loginManager.ChangePassword(newPassword);
				switch (changePwdResult)
				{
					case (int)LoginReturnCodes.NoError:
						break;

					case (int)LoginReturnCodes.InvalidUserError:
						//Non è riuscito a cambiare la password.
						diagnostic.SetError(WebServicesWrapperStrings.ErrChangePassword);
						break;
					case (int)LoginReturnCodes.PasswordTooShortError:
						//Non è riuscito a cambiare la password.
						diagnostic.SetError(WebServicesWrapperStrings.ErrPwdLength);
						break;
					case (int)LoginReturnCodes.CannotChangePasswordError:
					case (int)LoginReturnCodes.PasswordExpiredError:
						//Non è riuscito a cambiare la password.
						diagnostic.SetError(WebServicesWrapperStrings.ErrUserCannotChangePwdButMust);
						break;
					case (int)LoginReturnCodes.PasswordAlreadyChangedToday:
						diagnostic.SetError(WebServicesWrapperStrings.PasswordAlreadyChangedToday);
						break;
					default:
						diagnostic.SetError(WebServicesWrapperStrings.ErrChangePassword);//todo ilaria verifica
						break;
				}
			}
			catch (WebException exc)
			{
				if (exc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)exc.Response;
					if (webResponse.StatusDescription.Length > 0)
						diagnostic.SetError (string.Format(WebServicesWrapperStrings.ServerDown, webResponse.StatusDescription));
					//MessageBox.Show(string.Format(WebServicesWrapperStrings.ServerDown, webResponse.StatusDescription));
					else
						diagnostic.SetError (string.Format(WebServicesWrapperStrings.ServerDown, webResponse.StatusCode.ToString()));
					//MessageBox.Show(string.Format(WebServicesWrapperStrings.ServerDown, webResponse.StatusCode.ToString()));
					webResponse.Close();

				}
				else
					diagnostic.SetError(string.Format(WebServicesWrapperStrings.ServerDown, exc.Status.ToString()));
			}

			return diagnostic.ToJson(true);

		}

		//-----------------------------------------------------------------------
		internal void Logoff()
		{
			loginManager.LogOff();
		}

		//-----------------------------------------------------------------------
		private string GetLoginMngSession()
		{
			string pwd = string.Empty;
			string loginSessionFile = BasePathFinder.BasePathFinderInstance.GetLoginMngSessionFile();
			if (File.Exists(loginSessionFile))
			{
				try
				{
					StreamReader sr = new StreamReader(loginSessionFile);
					pwd = sr.ReadToEnd();
					sr.Close();
				}
				catch
				{
					pwd = string.Empty;
				}
			}

			return pwd;
		}

		//-----------------------------------------------------------------------
		private bool PreLoginChecks()
		{
			string message = String.Empty;
			ActivationState dummy;

			bool isRegistered = loginManager.IsRegistered(out message, out dummy);
			//se non è attivo provo forzosamente a fare un ping per vedere se si aggiusta la faccenda
			if (!isRegistered)
			{
				loginManager.Ping();
				//poi ritento e vada come vada
				isRegistered = loginManager.IsRegistered(out message, out dummy);
			}
			if (!string.IsNullOrWhiteSpace(message))
			{
				diagnostic.Set(DiagnosticType.Information, message);
			}

			return isRegistered;
		}
        
        //-----------------------------------------------------------------------
        private int ValidateSSOUser(string username, ref string password, bool winNT)
        {

            password = (winNT) ? GetLoginMngSession() : password;

            return ValidatessoUser(username, password, winNT);
        }

        //-----------------------------------------------------------------------
        private bool ValidateAppUser(string username, string password, bool winNT)
        {
            string pwd = (winNT) ? GetLoginMngSession() : password;
            return ValidateUser(username, pwd, winNT);

        }
        //-----------------------------------------------------------------------
        private int ValidatessoUser(string username, string password, bool winNT)
        {
            try
            {
                return loginManager.ValidateUser(username, password, winNT);
            }
            catch (Exception)
            {
                return (int)LoginReturnCodes.Error;
            }
        }

        //-----------------------------------------------------------------------
        private bool ValidateUser(string username, string password, bool winNT)
		{
            
            int validateResult = 0;

			try
			{
                validateResult = loginManager.ValidateUser(username, password, winNT);
			}
			catch (Exception )
            {
                diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ServerDown);
                return false;

            }

            if (validateResult == (int)LoginReturnCodes.SysDBConnectionFailure)
            {
                password = string.Empty;
                diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ServerNotConnectedToSystemDB);
                return false;
            }
            if (validateResult == (int)LoginReturnCodes.MissingConnectionString)
            {
                password = string.Empty;
                diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.MissingConnectionString);
                return false;
            }
            if (validateResult == (int)LoginReturnCodes.ActivationManagerInitializationFailure)
            {
                password = string.Empty;
                diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrActivationManager);
                return false;
            }

            if (validateResult == (int)LoginReturnCodes.LoginLocked)
            {
                password = string.Empty;
                diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.LoginLocked);
                return false;
            }

            if (validateResult == (int)LoginReturnCodes.UserAlreadyLoggedError)
            {
                diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrUserAlreadyLogged);
                return false;
            }

            if (validateResult != (int)LoginReturnCodes.NoError)
            {
                diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrLoginFailed);
                return false;
            }

            return true;
        }

		//-----------------------------------------------------------------------
		private int InternalLogin(string company, bool overwriteLogin)
		{
			//Eseguo la login sul server da cui ricevo la stringa di connessione al database
			int loginResult;
			try
			{
				loginResult = loginManager.Login(company, processType, overwriteLogin);
			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.ServerDown, exc.ToString()));
				return (int)LoginReturnCodes.Error; 
			}
           return  SetDiagnosticLoginReturnsCodeError(loginResult);
     
		}


      

        //-----------------------------------------------------------------------
        public string SSOLogin
            (string cryptedtoken, string username, string password, string company, bool rememberMe,
            bool winNT, bool overwriteLogin, bool relogin, IntPtr menuHandle,
            out bool alreadyLogged, out bool changePassword, out bool changeAutologinInfo,
            string saveAutologinInfo, out int result)
        {
            result = 0;
            alreadyLogged = false;
            changePassword = false;
            changeAutologinInfo = false;

			if (relogin)
            {
                loginManager.LogOff();
            }
            // bool dontsavelatestuser = false;

            //if (Autologinable && rememberMe)//la message box di cambio utente la mostro solo se c'è autologin, per sicurezza
            //{
            //    //se mi sta tornando indietro da una richiesta di salvataggio a cui ho risposto si salvo e continue; 
            //    if (saveAutologinInfo == "1")
            //        dontsavelatestuser = false;// = SaveLatestUser(username, password, company, rememberMe);

            //    // se l'utente (che non sia stringa vuota) o la company indicati son diversi rispetto al last user salvato allora viene dato un warning che avverte che l'utente viene sovrascritto
            //    else if (saveAutologinInfo != "-1" && !string.IsNullOrWhiteSpace(LatestUser.User) && (
            //        String.Compare(LatestUser.User, username, true) != 0 ||
            //        String.Compare(LatestUser.Company, company, true) != 0))
            //    {
            //        changeAutologinInfo = true;
            //        return string.Empty;
            //    }
            //}

            try
            {


                if (!PreLoginChecks())
                    return string.Empty;
                result = ValidateSSOUser(username, ref password, winNT);
                if (result != 0)
                {

                    SetDiagnosticLoginReturnsCodeError(result);
                    return string.Empty;
                }
                bool pwdok = winNT ? true : PostLoginPasswordChecks(username, password, out changePassword);
                if (!pwdok)
                    return string.Empty;



                result = InternalSSOLogin(cryptedtoken, username, password, company);
                if (0 != result)
                {
                    alreadyLogged = result == (int)LoginReturnCodes.UserAlreadyLoggedError;
                    return string.Empty;
                }
                else
                {      //qui controllo la login in sicurezza integrata
                    if (winNT && !ValidateDomainCredentials(username, company))
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, WebServicesWrapperStrings.ErrInvalidUser);
                        result = (int)LoginReturnCodes.InvalidUserError;
                        loginManager.LogOff(loginManager.AuthenticationToken);
                        return string.Empty;
                    }
                }
                //if (!dontsavelatestuser)
                //    SaveLatestUser(username, password, company, rememberMe);

                LockManager lockManager = new LockManager(BasePathFinder.BasePathFinderInstance.LockManagerUrl);
                lockManager.RemoveUnusedLocks();

                return loginManager.AuthenticationToken;
            }
            catch (Exception exc)
            {
                diagnostic.Set(DiagnosticType.Error, exc.ToString());
                return string.Empty;
            }

        }
        //----------------------------------------------------------------------
        internal bool ValidateDomainCredentials(string user, string company)
        {
            try
            {
                //prendo i dati di user attualmente connesso a windows
                string fullname;
				string loginName = Environment.UserName;
				string userDomainName = Environment.UserDomainName;

				if (userDomainName.Length == 0)
                    fullname = Dns.GetHostName() + Path.DirectorySeparatorChar + loginName;
                else
                    fullname = userDomainName + Path.DirectorySeparatorChar + loginName;

                if (String.Compare(fullname, user, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return ExistCompanies(fullname, company);
                return false;

            }
            catch (Exception exc)
            {
                diagnostic.Set(DiagnosticType.Error, exc.ToString());
                return false;
            }
        }


        //-----------------------------------------------------------------------
        public void SSOLogOff(string cryptedtoken)
        {
            loginManager.SSOLogOff(cryptedtoken);
        }

        //-----------------------------------------------------------------------
        private int SetDiagnosticLoginReturnsCodeError(int loginResult)
        {
            if (loginResult == 0) return 0;

                switch (loginResult)
            {
                case (int)LoginReturnCodes.NoError:
                    return loginResult;

                case (int)LoginReturnCodes.AlreadyLoggedOnDifferentCompanyError:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.AlreadyLoggedOnDifferentCompanyError);
                    return loginResult;

                case (int)LoginReturnCodes.UserAlreadyLoggedError:
                    return loginResult;
                case (int)LoginReturnCodes.WebUserAlreadyLoggedError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrWebUserAlreadyLogged);
                    return loginResult;

                case (int)LoginReturnCodes.NoCalAvailableError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.NoCalAvailableError);
                    return loginResult;

                case (int)LoginReturnCodes.NoLicenseError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrNoArticleFunctionality);
                    return loginResult;

                case (int)LoginReturnCodes.UserAssignmentToArticleFailure:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrUserAssignmentToArticle);

                    return loginResult;

                case (int)LoginReturnCodes.UserNotAllowed:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UserNotAllowed);
                    return loginResult;


                case (int)LoginReturnCodes.ProcessNotAuthenticatedError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrProcessNotAuthenticated);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidUserError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrInvalidUser);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidProcessError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrInvalidProcess);
                    return loginResult;

                case (int)LoginReturnCodes.LockedDatabaseError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrLockedDatabase);
                    return loginResult;

                case (int)LoginReturnCodes.UserMustChangePasswordError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrUserCannotChangePwdButMust);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidCompanyError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrInvalidCompany);
                    return loginResult;

                case (int)LoginReturnCodes.ProviderError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrProviderInfo);
                    return loginResult;

                case (int)LoginReturnCodes.ConnectionParamsError:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrConnectionParams);
                    return loginResult;

                case (int)LoginReturnCodes.CompanyDatabaseNotPresent:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseNotPresent);
                    return loginResult;

                case (int)LoginReturnCodes.CompanyDatabaseTablesNotPresent:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseTablesNotPresent);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidDatabaseForActivation:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseForActivation);
                    return loginResult;

                case (int)LoginReturnCodes.WebApplicationAccessDenied:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.WebApplicationAccessDenied);
                    return loginResult;

                case (int)LoginReturnCodes.GDIApplicationAccessDenied:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.GDIApplicationAccessDenied);
                    return loginResult;

                case (int)LoginReturnCodes.LoginLocked:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.LoginLocked);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidDatabaseError:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseError);
                    return loginResult;

                case (int)LoginReturnCodes.NoAdmittedCompany:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.NoAdmittedCompany);
                    return loginResult;

                case (int)LoginReturnCodes.NoOfficeLicenseError:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.NoOfficeLicenseError);
                    return loginResult;

                case (int)LoginReturnCodes.TooManyAssignedCAL:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.TooManyAssignedCAL);
                    return loginResult;

                case (int)LoginReturnCodes.NoDatabase:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseNotPresent);
                    return loginResult;

                case (int)LoginReturnCodes.NoTables:
                    diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseTablesNotPresent);
                    return loginResult;

                case (int)LoginReturnCodes.NoActivatedDatabase:
                    if (Functions.IsDebug() && loginManager.IsDeveloperActivation())
                        break;

                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseForActivation);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidModule:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseError);
                    return loginResult;

                case (int)LoginReturnCodes.DBSizeError:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.DBSizeError);
                    return loginResult;

                case (int)LoginReturnCodes.SsoTokenEmpty:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.SsoTokenEmpty);
                    return loginResult;
                case (int)LoginReturnCodes.InvalidSSOToken:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidSSOToken);
                    return loginResult;
                case (int)LoginReturnCodes.SsoTokenError:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.SsoTokenError);
                    return loginResult;
                case (int)LoginReturnCodes.MoreThanOneSSOToken:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.MoreThanOneSSOToken);
                    return loginResult;
                case (int)LoginReturnCodes.ImagoUserAlreadyAssociated:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.ImagoUserAlreadyAssociated);
                    return loginResult;
                case (int)LoginReturnCodes.ImagoCompanyNotCorresponding:
                    diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.ImagoCompanyNotCorresponding);
                    return loginResult;
             

                   
            }
           diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrLoginFailed);
                    return loginResult;

        }

        //-----------------------------------------------------------------------
        private int InternalSSOLogin(string cryptedtoken, string username, string password, string company)
        {
            //Eseguo la login sul server da cui ricevo la stringa di connessione al database
            int loginResult;
            try
            {
                loginResult = loginManager.LoginViaInfinityToken(cryptedtoken, username, password, company);
            }
            catch (Exception exc)
            {
                diagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.ServerDown, exc.ToString()));
                return (int)LoginReturnCodes.Error;
            }

            return SetDiagnosticLoginReturnsCodeError(loginResult);
       
        }

        //-----------------------------------------------------------------------
        private bool PostLoginPasswordChecks(string username, string password, out bool changePassword)
		{
			changePassword = !loginManager.PasswordNeverExpired && (loginManager.ExpiredDatePassword < DateTime.Today);
			if (changePassword)
			{
				//l'utente deve cambiare la pwd ma non può.
				if (loginManager.UserCannotChangePassword || loginManager.ExpiredDateCannotChange)
				{
					diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrUserCannotChangePwdButMust);
					changePassword = false;
				}
				return false;
			}

			//l'utente deve cambiare la pwd ma non può.
			if (loginManager.UserCannotChangePassword && loginManager.UserMustChangePassword)
			{
				diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrUserCannotChangePwdButMust);
				changePassword = false;
				return false;
			}
			//l'utente deve cambiare la pwd ma non può.
			if (loginManager.UserMustChangePassword)
			{
				diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ChangePasswordNeeded);
				changePassword = true;
				return false;
			}
			return true;

		}

        /// <summary>
        /// Chiede a Login Manager di autenticare l'utente 
        /// </summary>
        /// <param name="forceConnection">Indica se si vuole sosvrascrivere l'autenticazione di un eventuale altra istanza dello stesso utente</param>
        /// <returns>Il token di autenticazione dell'utente, String.Empty se l'autenticazione è fallita</returns>
        //-----------------------------------------------------------------------
        public string Login
            (string username, string password, string company, bool rememberMe, 
            bool winNT, bool overwriteLogin, bool relogin, IntPtr menuHandle, 
            out bool alreadyLogged, out bool changePassword, out bool changeAutologinInfo, 
            string saveAutologinInfo, out string culture, out string uiCulture)//parametro non usato per gestire in un unico punto la riabilitazione dei controlli
		{
			alreadyLogged = false;
			changePassword = false;
             changeAutologinInfo = false;
			culture = uiCulture = string.Empty;
			if (relogin)
			{
				loginManager.LogOff();
			}
			bool dontsavelatestuser = false;

			if ( Autologinable && rememberMe)//la message box di cambio utente la mostro solo se c'è autologin, per sicurezza
			{
                //se mi sta tornando indietro da una richiesta di salvataggio a cui ho risposto si salvo e continue; 
                if (saveAutologinInfo == "1")
                    dontsavelatestuser = false;// = SaveLatestUser(username, password, company, rememberMe);

                // se l'utente (che non sia stringa vuota) o la company indicati son diversi rispetto al last user salvato allora viene dato un warning che avverte che l'utente viene sovrascritto
                else if (saveAutologinInfo != "-1" && !string.IsNullOrWhiteSpace(LatestUser.User) && (
                    String.Compare(LatestUser.User, username, true) != 0 ||
                    String.Compare(LatestUser.Company, company, true) != 0))
                {
                    changeAutologinInfo = true;
                    return string.Empty;
                }
			}

			try
			{
				if (!PreLoginChecks())
					return string.Empty;

				if (!ValidateAppUser(username, password, winNT))
				{
					return string.Empty;
				}
				bool pwdok = winNT ? true : PostLoginPasswordChecks(username, password, out changePassword);
				if (!pwdok)
					return string.Empty;
				int res = InternalLogin(company, overwriteLogin);
				if (0 != res)
				{
					alreadyLogged = res == (int)LoginReturnCodes.UserAlreadyLoggedError;
					return string.Empty;
				}

				loginManager.GetLoginInformation(loginManager.AuthenticationToken);
				culture = loginManager.ApplicationLanguage;
				uiCulture = loginManager.PreferredLanguage;

                if (!dontsavelatestuser)
                    SaveLatestUser(username, password, company, rememberMe);

                LockManager lockManager = new LockManager(BasePathFinder.BasePathFinderInstance.LockManagerUrl);
				lockManager.RemoveUnusedLocks();

				return loginManager.AuthenticationToken;
			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.ToString());
				return string.Empty;
			}
		}

        //---------------------------------------------------------------------------
        private static void SaveLatestUser(string username, string password, string company, bool rememberMe)
		{
			new Task(() =>
				{
				//aggiorno il client connection con i dati dell'ultimo utente connesso
				LatestUser.Company = company;
					LatestUser.User = username;
					LatestUser.Remember = rememberMe;
					LatestUser.Password = (rememberMe) ? password : String.Empty;
					LatestUser.Save();
				}).Start();
		}

		//---------------------------------------------------------------------------
		private void ManageClientDataAsynch()
		{
			Thread thread = null;
			try
			{
				thread = new Thread(new ThreadStart(ManageClientData));
				thread.Start();
			}
			catch (Exception exception)
			{
				Debug.WriteLine("Exception raised in ManageClientDataAsynch: " + exception.Message);
				thread = null;
			}
		}


		//---------------------------------------------------------------------------
		private void ManageClientData()
		{


			//il ping colleziona per fini statistici info sistemistiche dei client
			//per cui non sono informazioni essenziali, se si schianta in qualche punto fa lo stesso
			try
			{
				ClientData cd = new ClientData(Environment.MachineName, LocalMachine.GetProcessorData(), LocalMachine.GetScreenData(), LocalMachine.GetRAMData());
				SendClientData(cd);
			}
			catch { }

		}

		//-----------------------------------------------------------------------------
		private void SendClientData(ClientData cd)
		{
			loginManager.SetClientData(cd);
		}


		#endregion



	}
}
