using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.WebServices.Core.WebServicesWrapper;
using System.IO;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{

	/// <summary>
	/// Eccezione per gli errori del server
	/// </summary>
	//=================================================================================
	public class LoginManagerException : ApplicationException 
	{
		private LoginManagerError	errorCode;
		public	LoginManagerError	ErrorCode	{	get	{ return errorCode;	}	}

        //-----------------------------------------------------------------------
        public LoginManagerException(LoginManagerError errorCode, string errorMessage)
            :
            base (errorMessage)
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

	/// <summary>
	/// Wrapper di LoginManager server
	/// </summary>
	//============================================================================
	public class LoginManager : ILoginManager
    {
		#region membri privati e proprietà
		private string[] modules = null;
		private Dictionary<string, bool> activationList = null;
		private loginMng.MicroareaLoginManager loginManager = new loginMng.MicroareaLoginManager();
		
		//---------------------------------------------------------------------------
		private LoginManagerState	loginManagerState = LoginManagerState.UnInitialized;
		
		/// <summary>
		/// Stato di Login Manager
		/// </summary>
		public LoginManagerState	LoginManagerState	{ get { return loginManagerState; }}
		
		#endregion

		#region membri privati e proprietà di get non utilizzabili in stato UnInitialized

		private string		userName = string.Empty;
		/// <summary>
		/// Nome dell'utente autenticato attualemente connesso
		/// </summary>
		public	string		UserName
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return userName; 
			} 
		}

		private string		userDescription = string.Empty;
		/// <summary>
		/// Descrizione dell'utente autenticato attualmente connesso
		/// </summary>
		public	string		UserDescription
		{ 
			get 
			{
				return userDescription; 
			} 
		}

        private string email = string.Empty;
        /// <summary>
        /// Email utente autenticato attualmente connesso
        /// </summary>
        public string Email
        {
            get
            {
                return email;
            }
        }

		private string		password = string.Empty;
		/// <summary>
		/// Password dell'utente autenticato attualemente connesso
		/// </summary>
		public	string		Password
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return password; 
			} 
			set
			{
				password = value;
			}
		}

		private string[] userCompanies = null;

		/// <summary>
		/// Aziende associate all'utente autenticato attualemente connesso
		/// </summary>
		public	string[] UserCompanies
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return userCompanies; 
			} 
		}

		private int			loginId	= -1;
		/// <summary>
		/// ID dell'utente autenticato attualemente connesso
		/// </summary>
		public	int			LoginId
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return loginId; 
			} 
		}

		private bool		winNTAuthentication					= false;
		/// <summary>
		/// Indica se l'utente autenticato attualemente connesso è in sicurezza integrata
		/// </summary>
		public	bool		WinNTAuthentication
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return winNTAuthentication; 
			} 
		}

		private bool		userCannotChangePassword	= false;
		/// <summary>
		/// Indica se l'utente autenticato attualemente connesso non può cambiare la password
		/// </summary>
		public	bool		UserCannotChangePassword
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return userCannotChangePassword; 
			} 
		}

		private bool		userMustChangePassword		= false;
		/// <summary>
		/// Indica se l'utente autenticato attualemente connesso deve cambiare la password
		/// </summary>
		public	bool		UserMustChangePassword
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return userMustChangePassword; 
			} 
		}

		private DateTime	expiredDatePassword			= DateTime.MaxValue;
		/// <summary>
		/// Indica la data di scadenza della password dell'utente autenticato attualemente connesso 
		/// </summary>
		public	DateTime	ExpiredDatePassword
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return expiredDatePassword; 
			} 
		}
		
		private bool		passwordNeverExpired		= false;
		/// <summary>
		/// Indica che la password dell'utente autenticato attualemente connesso non scade mai
		/// </summary>
		public	bool		PasswordNeverExpired
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return passwordNeverExpired; 
			} 
		}
		
		private bool	expiredDateCannotChange		= false;
		/// <summary>
		/// Indica che la data di scadenza della password dell'utente autenticato attualemente 
		/// connesso non può cambiare
		/// </summary>
		public	bool	ExpiredDateCannotChange
		{ 
			get 
			{
				if (loginManagerState == LoginManagerState.UnInitialized)
					throw new LoginManagerException(LoginManagerError.UnInitialized);

				return expiredDateCannotChange; 
			} 
		}
		
		#endregion
		
		#region membri privati e proprietà di get utilizzabili solo in stato Logged
		
		//---------------------------------------------------------------------------
		private string		companyName					= string.Empty;
		/// <summary>
		/// Nome dell'azienda dell'utente autenticato attualemente connesso
		/// </summary>
		public	string		CompanyName
		{ 
			get 
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return companyName; 
			} 
		}

		//---------------------------------------------------------------------------
		private bool		admin						= false;
		/// <summary>
		/// Indica che l'utente autenticato attualemente connesso è amministratore
		/// </summary>
		public	bool		Admin
		{ 
			get 
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return admin; 
			} 
		}

		//---------------------------------------------------------------------------
		private bool isAdminWebSitePrivateAreaInitialize = false;
		private bool adminWebSitePrivateArea = false;
		/// <summary>
		/// Indica che l'utente autenticato attualemente connesso è amministratore
		/// </summary>
		public bool AdminWebSitePrivateArea
		{
			get
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);
                if (!isAdminWebSitePrivateAreaInitialize)
                {
                    adminWebSitePrivateArea = UserCanAccessWebSitePrivateArea();
                    isAdminWebSitePrivateAreaInitialize = true;
                }
				return adminWebSitePrivateArea;
			}
		}
		//---------------------------------------------------------------------------
		private string		authenticationToken			= string.Empty;
		/// <summary>
		/// Token di autenticazione dell'utente autenticato attualemente connesso
		/// </summary>
		public	string		AuthenticationToken
		{ 
			get 
			{
				return authenticationToken; 
			} 
		}

		//---------------------------------------------------------------------------
		private int			companyId					= -1;
		/// <summary>
		/// ID dell'azienda dell'utente autenticato attualemente connesso
		/// </summary>
		public	int			CompanyId					
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return companyId; 
			}
		}

		//---------------------------------------------------------------------------
		private string		dbName						= string.Empty;
		/// <summary>
		/// Nome del database dell'azienda dell'utente autenticato attualemente connesso
		/// </summary>
		public	string		DbName					
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return dbName; 
			} 
		
		}

		//---------------------------------------------------------------------------
		private string		dbServer					= string.Empty;
		/// <summary>
		/// Nome del server del database dell'azienda dell'utente autenticato attualemente connesso
		/// </summary>
		public	string		DbServer					
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return dbServer; 
			} 
		
		}
		
		//---------------------------------------------------------------------------
		private int		providerId					= -1;
		/// <summary>
		/// ID del provider del database dell'azienda dell'utente autenticato attualemente connesso
		/// </summary>
		public	int		ProviderId					
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return providerId; 
			} 
		}
		
		//---------------------------------------------------------------------------
		private string		providerName				= string.Empty;
		/// <summary>
		/// Nome del provider del database dell'azienda dell'utente autenticato attualemente connesso
		/// </summary>
		public string		ProviderName
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return providerName	; 
			} 
		}

		//---------------------------------------------------------------------------
		private string		providerDescription			= string.Empty;
		/// <summary>
		/// Descrizione del provider del database dell'azienda dell'utente autenticato attualemente connesso
		/// </summary>
		public string		ProviderDescription
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return providerDescription; 
			} 
		}
		
		//---------------------------------------------------------------------------
		private bool		stripTrailingSpaces			= false;
		/// <summary>
		/// Indica che il provider del database dell'azienda dell'utente autenticato attualemente connesso 
		/// elimina gli spazi a fine stringa
		/// </summary>
		public bool			StripTrailingSpaces
		{ 
			get 
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return stripTrailingSpaces; 
			} 
		}

		//---------------------------------------------------------------------------
		private bool		security					= false;
		/// <summary>
		/// Indica che l'azienda dell'utente autenticato attualemente connesso è sotto controllo di security
		/// </summary>
		public bool			Security
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return security; 
			} 
		}

		//---------------------------------------------------------------------------
		private bool		auditing					= false;
		/// <summary>
		/// Indica che l'azienda dell'utente autenticato attualemente connesso è sotto controllo di auditing
		/// </summary>
		public bool			Auditing
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return auditing; 
			} 
		}

        //---------------------------------------------------------------------------
		private bool		rowSecurity					= false;
        /// <summary>
        /// Indica che l'azienda dell'utente autenticato attualemente connesso è sotto controllo del Row Security Layer
        /// </summary>
        public bool RowSecurity
        {
            get
            {
                if (loginManagerState != LoginManagerState.Logged)
                    throw new LoginManagerException(LoginManagerError.NotLogged);

                return rowSecurity;
            }
        }
		
		//---------------------------------------------------------------------------
		private bool	useKeyedUpdate				= false;
		/// <summary>
		/// Indica che il database dell'azienda dell'utente autenticato attualemente connesso utilizza l'aggiornamento keyed
		/// </summary>
		public bool		UseKeyedUpdate
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);
				
				return useKeyedUpdate; 
			} 
		}
		
		//---------------------------------------------------------------------------
		private bool	transactionUse				= false;
		private bool	useUnicode					= false;
		/// <summary>
		/// Indica che il database dell'azienda dell'utente autenticato attualemente connesso utilizza le transazioni
		/// </summary>
		public bool		TransactionUse
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);
				
				return transactionUse; 
			} 
		}
		/// <summary>
		/// Indica che il database dell'azienda dell'utente autenticato attualemente connesso utilizza unicode
		/// </summary>
		public bool UseUnicode
		{
			get
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return useUnicode;
			}
		}
	
		//---------------------------------------------------------------------------
		private bool		useConstParameter			= false;
		/// <summary>
		/// Indica che il database dell'azienda dell'utente autenticato attualemente connesso utilizza parametri costanti
		/// E' impostato a true se nelle quesry non è consentita la sintasssi ?=?
		/// Non è ammesso in sql
		/// </summary>
		public bool			UseConstParameter
		{ 
			get 
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return useConstParameter; 
			} 
		}

		//---------------------------------------------------------------------------
		private string	providerCompanyConnectionString		= string.Empty;
		public	string	ProviderCompanyConnectionString
		{ 
			get 
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return providerCompanyConnectionString; 
			} 
		}

		//---------------------------------------------------------------------------
		private string	nonProviderCompanyConnectionString	= string.Empty;
		public	string	NonProviderCompanyConnectionString
		{ 
			get 
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return nonProviderCompanyConnectionString; 
			} 
		}
		
		//---------------------------------------------------------------------------
		private string	dbUser								= string.Empty;
		public	string	DbUser
		{ 
			get 
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				return dbUser; 
			} 
		}
       
		//---------------------------------------------------------------------------
		private string		preferredLanguage			= string.Empty;
		/// <summary>
		/// Lingua preferenziale dell'utente autenticato attualemente connesso
		/// </summary>
		public	string		PreferredLanguage
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);
				
				return preferredLanguage; 
			} 
		}
		
		//---------------------------------------------------------------------------
		private string		applicationLanguage			= string.Empty;
		/// <summary>
		/// Lingua di applicazione dell'utente autenticato attualemente connesso
		/// </summary>
		public	string		ApplicationLanguage
		{ 
			get 
			{ 
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);
				
				return applicationLanguage; 
			} 
		}
		//---------------------------------------------------------------------------
		private string preferredCompanyLanguage = string.Empty;
		/// <summary>
		/// Lingua preferenziale dell'utente autenticato attualemente connesso
		/// </summary>
		public string PreferredCompanyLanguage
		{
			get
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				if (preferredCompanyLanguage.Length == 0)
				{
					applicationCompanyLanguage = string.Empty;

					try
					{
						if (loginManager.GetCompanyLanguage(this.CompanyId, out preferredCompanyLanguage, out applicationCompanyLanguage))
						{
							//throw or not throw ?
						}
					}
					catch
					{
					}
				}

				return preferredCompanyLanguage;
			}
		}

		//---------------------------------------------------------------------------
		private string applicationCompanyLanguage = string.Empty;
		/// <summary>
		/// Lingua di applicazione dell'utente autenticato attualemente connesso
		/// </summary>
		public string ApplicationCompanyLanguage
		{
			get
			{
				if (loginManagerState != LoginManagerState.Logged)
					throw new LoginManagerException(LoginManagerError.NotLogged);

				if (applicationCompanyLanguage.Length == 0)
				{
					preferredCompanyLanguage = string.Empty;

					try
					{
						if (loginManager.GetCompanyLanguage(this.CompanyId, out preferredCompanyLanguage, out applicationCompanyLanguage))
						{
							//throw or not throw ?
						}
					}
					catch
					{
					}
				}
				return applicationCompanyLanguage;
			}
		}

		#endregion
		
		#region costruttori
		/// <summary>
		/// Inizializza il login manager sull'url e sul timeout di default
		/// </summary>
		//---------------------------------------------------------------------------
		public LoginManager()
			: this(BasePathFinder.BasePathFinderInstance.LoginManagerUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut)
		{
		}

		//---------------------------------------------------------------------------
		public LoginManager(string loginManagerUrl, int timeout)
		{
			loginManager.Url = loginManagerUrl;
			loginManager.Timeout = timeout;
		}
		
		#endregion

		#region funzioni che non necessitano di dati utente o company
		
		/// <summary>
		/// Metodo utilizzato dalla console per reinizializzare loginManager
		/// Ad esempio in caso di cambio di database aziendale
		/// </summary>
		/// <param name="reinitialize"> se impostato a false reinizializza login manager 
		/// solo se è in stato di preinizializzazione</param>
		/// <returns>0 se ha successo > 0 in caso di errore</returns>
		//---------------------------------------------------------------------------
		public int Init(bool reboot, string authenticationToken)
		{
			return loginManager.Init(reboot, authenticationToken);
		}

     

		//---------------------------------------------------------------------------
		public SerialNumberType GetSerialNumberType()
		{
			return (SerialNumberType) loginManager.CacheCounterGTG();
		}
	
		//----------------------------------------------------------------------
		public bool SaveLicensed(string xml, string name)
		{
			return loginManager.SaveLicensed(xml, name);
		}
        
        //----------------------------------------------------------------------
        public bool SaveUserInfo(string xml)
		{
			return loginManager.SaveUserInfo(xml);
        }  
        
        //----------------------------------------------------------------------
        public string ValidateItoken(string itoken)
        {
            return loginManager.ValidateIToken(itoken, authenticationToken);
        }

        //----------------------------------------------------------------------
        public void DeleteLicensed(string name)
		{
			loginManager.DeleteLicensed(name);
		}

        //----------------------------------------------------------------------
        public void DeleteUserInfo()
        {
            loginManager.DeleteUserInfo();
        }

		//---------------------------------------------------------------------------
		public bool IsVirginActivation()
		{
			return loginManager.IsVirginActivation();
		}

		//---------------------------------------------------------------------------
		public ActivationState GetActivationState(out int daysToExpiration)
		{
			return (ActivationState)loginManager.SetCurrentComponents(out daysToExpiration);
		}

		//---------------------------------------------------------------------------
		public string GetAspNetUser()
		{
			return loginManager.GetAspNetUser();
		}
		//---------------------------------------------------------------------------
		public string Ping()
		{
			return loginManager.Ping();
		}

        //---------------------------------------------------------------------------
		public string PrePing()
		{
            return loginManager.PrePing();
		}

		//---------------------------------------------------------------------------
		public void StoreMLUChoice(bool userChoseMluInChargeToMicroarea)
		{
			loginManager.StoreMLUChoiceAsync(userChoseMluInChargeToMicroarea);
		}

		/// <summary>
		/// per sapere se siamo in versione Demo o DemoWarning
		/// </summary>
		/// <returns>true se siamo in demo, false se no</returns>
		//---------------------------------------------------------------------------
		public bool IsDemo()
		{
			return GetSerialNumberType() == SerialNumberType.Demo	;
		}

		/// <summary>
		/// per sapere se siamo in versione Distributor
		/// </summary>
		//---------------------------------------------------------------------------
		public bool IsDistributor()
		{
			return GetSerialNumberType() == SerialNumberType.Distributor;
		}

		/// <summary>
		/// per sapere se siamo in versione Development
		/// </summary>
		//---------------------------------------------------------------------------
		public bool IsDeveloperActivation()
		{
			return loginManager.CacheCounter();
		}
		
		/// <summary>
		/// per sapere se siamo in versione Reseller
		/// </summary>
		//---------------------------------------------------------------------------
		public bool IsReseller()
		{
			return GetSerialNumberType() == SerialNumberType.Reseller;
		}

        /// <summary>
        //per ora torno solo se è resellers poi dovrò verificare se è attivato il migrationkit, quando ci sarà
        /// </summary>
        //---------------------------------------------------------------------------
        public bool CanMigrate()
        {
            return IsActivated("MicroareaConsole", "MigrationKit");
        }

        //---------------------------------------------------------------------------
        public bool IsRegistered(out string message, out ActivationState actState)
		{
			message = "";
			int daysToExpire = 0;
			actState = GetActivationState(out daysToExpire);
			
			switch (actState)
			{
				case ActivationState.Disabled:
					message = WebServicesWrapperStrings.ActivationStateDisabled;
					return false;
				case ActivationState.NoActivated:
					message = WebServicesWrapperStrings.ActivationStateNoActivated;
					return false;
				case ActivationState.DemoWarning:
				case ActivationState.Demo:
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
				case ActivationState.Warning:
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

		private const int webExceptionErrorCode		= 9879;
		private const int soapExceptionErrorCode	= 9880;
		private const int genericExceptionErrorCode	= 9881;
		//---------------------------------------------------------------------------
		public bool IsRegisteredTrapped (out string message, out ActivationState actState)
		{
			message = string.Empty;
			actState = ActivationState.NoActivated;
			bool registered = false;
			try
			{
				registered = IsRegistered(out message, out actState);
			}
			catch (System.Net.WebException) // do not reveal exception!
			{
				// unable to contact login manager.
				// this could be a connection timeout
				message = string.Format(WebServicesWrapperStrings.WebExceptionError, webExceptionErrorCode.ToString());
				// which is something like:
				// "An unexpected error occurred. Error code is {0}. Please restart your server. If error persists, please contact customer service."
				// restaring IIS would be probably enought, but we don't want to give
				// hints to the curious guys, revealing that there's login manager behind the scenes.
			}
			catch (System.Web.Services.Protocols.SoapException) // do not reveal exception!
			{
				// console and login manager do not seem to be of the same release
				// this might be caused by an aborted update
				// better to invite fixing it.
				message = string.Format(WebServicesWrapperStrings.SoapExceptionError, soapExceptionErrorCode.ToString());
				// which is something like:
				// "An unexpected error occurred. Error code is {0}. This might be caused by a failed product update. Please try reinstalling the product."
			}
			catch
			{
				// an unhandled exception in LoginManager would raise via soap to the caller,
				// exposing implementation details we do not want to reveal.
				// better to stop it here.
				message = string.Format(WebServicesWrapperStrings.GenericExceptionError, genericExceptionErrorCode.ToString());
			}

			if (registered && message == string.Empty)
				message = WebServicesWrapperStrings.ProductIsRegistered;
			string addendum = String.Empty;
			if (IsDeveloperActivation())
				addendum = String.Format(" ({0})", GenericStrings.DvlpVersion);
			if (IsReseller() || IsDistributor())
				addendum = String.Format(" ({0})", GenericStrings.NFSVersion);
			message += addendum;
			return registered;
		}

		//---------------------------------------------------------------------
		public ProxySettings GetProxySettings()
		{
			return loginManager.GetProxySettings();

		}

		//---------------------------------------------------------------------
		public void SetProxySettings(ProxySettings proxySettings)
		{
			loginManager.SetProxySettings(proxySettings);
		}

        //----------------------------------------------------------------------
        internal string GetMobileToken(string token, int loginType)
        {
           return loginManager.GetMobileToken(token, loginType);
        }

        //----------------------------------------------------------------------
        internal string GiveMeIToken(string token)
        {
            return loginManager.GetIToken(token);
        }

        /// <summary>
        /// Questa funzione serve a verificare che esistano delle CAL siponibili per una funzionalità 
        /// appartenente ad un articolo soggetto a CAL. Chiamando questa funzione si associa un articolo ad
        /// un utente applicativo e si consuma una CAL dell'articolo relativo.
        /// Se l'articolo è già stato associato all'utente non consuma ulteriori cal.
        /// </summary>
        /// <param name="authenticationToken">Token di autenticazione dell'utente</param>
        /// <param name="application">Applicazione alla quale appartiene la funzionalità</param>
        /// <param name="functionality">Funzionalità o modulo fisico</param>
        /// <returns>true se l'articolo è stato acquistato ed esistono cal disponibili</returns>
        //----------------------------------------------------------------------
        public bool IsCalAvailable(string authenticationToken, string application, string functionality)
		{
			return loginManager.IsCalAvailable(authenticationToken, application, functionality);
		}
		//----------------------------------------------------------------------
		public string[] GetModules ()
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
				List<string> list = null;
				if (modules == null)
				{
					list = new List<string>();
					list.AddRange(loginManager.GetModules());
					modules = list.ToArray();
				}
				//le customizzazioni sono di default attivate
				list = new List<string>(modules);
				foreach (BaseApplicationInfo bai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
					if (bai.ApplicationType == ApplicationType.Customization)
					{
						foreach (BaseModuleInfo bmi in bai.Modules)
							list.Add(bai.Name + "." + bmi.Name);
					}
				return list.ToArray();
			}
		}

		//----------------------------------------------------------------------
		public bool PingNeeded(bool force)
		{
			return loginManager.PingNeeded(force);
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
			string key = string.Format("{0}.{1}", application, functionality);
			if (activationList == null)
				activationList = CreateActivationList();

			if (activationList.TryGetValue(key, out ret))
				return ret;
			return false;

		}

		//----------------------------------------------------------------------
		private Dictionary<string, bool> CreateActivationList ()
		{
			Dictionary<string, bool> list = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
			string[] modules = GetModules();
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

        // Un'espressione di attivazione (activationExpression) può essere data semplicemente 
        // dalla specifica di un'unica funzionalità oppure da un'espressione di tipo logico 
        // nella quale si possono “combinare” più funzionalità assieme in modo da effettuare un 
        // controllo di attivazione più complesso.
        // La sintassi da adottare per la formulazione corretta di questa espressione prevede 
        // di usare:
        //	- Il carattere ‘&’ per concatenare due elementi in "and". 
        //	- Il carattere ‘|’ per concatenare due elementi in "or".
        //	- Il carattere ‘!’ per negare un elemento.
        //	- Le parentesi tonde per raggruppare sotto-espressioni 
        //---------------------------------------------------------------------------
        private char[] activationExpressionOperators = new char[2] { '&', '|' };
        private char[] activationExpressionKeywords = new char[6] { '!', '&', '|', '(', ')', '?' };
        //---------------------------------------------------------------------------
        public static bool IsValidActivationExpressionSyntax(string activationExpression, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (activationExpression == null || activationExpression.Trim().Length == 0)
                return true;

            // Prima riduco l'espressione sostituendo i singoli token di attivazione con un carattere '?'  
            string activatonExprItemPattern = @"(?<ExprItem>([a-zA-Z0-9_]\.*)+)";
            string parsedString = Regex.Replace(activationExpression.Trim(), activatonExprItemPattern, "?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            parsedString = parsedString.Trim();

            // Controllo che, a parte i caratteri '*' appena messi, non ci siano rimasti caratteri diversi da quelli 
            // previsti dalle keyword
            string invalidCharPattern = @"[^\s\?\&\|\!\(\)]";
            if (Regex.IsMatch(parsedString, invalidCharPattern, RegexOptions.CultureInvariant))
            {
				errorMessage = WebServicesWrapperStrings.ActivationExpressionContainsInvalidCharErrMsg;
                return false;
            }
            string parenthesisPattern = @"(?<BeginExpr>[^()]*)(((?<Open>\()[^()]*)+((?<Close-Open>\))(?<EndExpr>[^()]*))+)*";

            Regex exp = new Regex(parenthesisPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

            try
            {
                if (!exp.IsMatch(parsedString))
                {
					errorMessage = WebServicesWrapperStrings.ActivationExpressionParenthesisMismatchErrMsg;
                    return false;
                }
                MatchCollection matches = exp.Matches(parsedString);
                foreach (Match match in matches)
                {
                    if (match != Match.Empty && String.Compare(match.Value, parsedString) == 0)
                    {
                        Group closeGroup = match.Groups["Close"];
                        if (closeGroup != null && closeGroup.Captures.Count > 0)
                        {
                            foreach (Capture aParenthesisContent in closeGroup.Captures)
                            {
                                if (!IsValidActivationExpressionSyntax(aParenthesisContent.ToString(), out errorMessage))
                                    return false;
                            }
                            Group beginGroup = match.Groups["BeginExpr"];
                            if (beginGroup != null && !String.IsNullOrEmpty(beginGroup.ToString()))
                            {
                                string beginString = beginGroup.ToString().Trim();
                                if (!String.IsNullOrEmpty(beginString) && String.Compare(beginString, "!") != 0 && !Regex.IsMatch(beginString, @"^((\s+)*!*\?)((\s+)*[\&\|](\s+)*!{0,1})$", RegexOptions.CultureInvariant))
                                {
									errorMessage = String.Format(WebServicesWrapperStrings.ActivationExpressionSyntaxErrMsg, beginString);
                                    return false;
                                }
                            }
                            Group endGroup = match.Groups["EndExpr"];
                            if (endGroup != null && !String.IsNullOrEmpty(endGroup.ToString()))
                            {
                                string endString = endGroup.ToString().Trim();
                                if (!String.IsNullOrEmpty(endString) && !Regex.IsMatch(endString, @"^((\s+)*[\&\|])((\s+)*!*\?)$", RegexOptions.CultureInvariant))
                                {
									errorMessage = String.Format(WebServicesWrapperStrings.ActivationExpressionSyntaxErrMsg, endString);
                                    return false;
                                }
                            }
                            return true;
                        }
                        // Non sono state trovate parentesi!
                        return Regex.IsMatch(parsedString, @"^((\s+)*!*\?)((((\s+)*[\&\|]{0,1}(\s+)*)!*\?)+)$", RegexOptions.CultureInvariant);
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
        }
 
        //---------------------------------------------------------------------------
        public bool CheckActivationExpression(string currentApplicationName, string activationExpression)
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
		
		//----------------------------------------------------------------------
		public bool IsSynchActivation()
		{
			return loginManager.IsSynchActivation();
		}

		/// <summary>
		/// Restituisce l'elenco degli utenti applicativi associati ad un'azienda
		/// </summary>
		/// <param name="companyName">Nome dell'azienda</param>
		/// <returns>L'elenco degli utenti</returns>
		//----------------------------------------------------------------------
		public string[] GetCompanyUsers(string companyName) 
		{
			return loginManager.GetCompanyUsers(companyName) ;
		}

		/// <summary>
		/// Restituisce l'elenco degli utenti applicativi associati ad un'azienda
		/// </summary>
		/// <param name="companyName">Nome dell'azienda</param>
		/// <returns>L'elenco degli utenti</returns>
		//----------------------------------------------------------------------
		public string GetProviderNameFromCompanyId(int companyId)
		{
			return loginManager.GetProviderNameFromCompanyId(companyId);
		}

		/// <summary>
		/// Restituisce l'elenco degli utenti applicativi associati ad un'azienda non nt
		/// </summary>
		/// <param name="companyName">Nome dell'azienda</param>
		/// <returns>L'elenco degli utenti non nt</returns>
		//----------------------------------------------------------------------
		public string[] GetNonNTCompanyUsers(string companyName) 
		{
			return loginManager.GetNonNTCompanyUsers(companyName) ;
		}

		//----------------------------------------------------------------------
		public string[] GetRoleUsers(string companyName, string roleName)
		{
			return loginManager.GetRoleUsers(companyName, roleName);
		}

		/// <summary>
		/// Restituisce l'elenco degli utenti applicativi
		/// </summary>
		/// <returns>L'elenco degli utenti</returns>
		//----------------------------------------------------------------------
		public string[] EnumAllUsers()
		{
			return loginManager.EnumAllUsers();
		}

		/// <summary>
		/// Restituisce l'elenco degli utenti applicativi
		/// </summary>
		/// <returns>L'elenco degli utenti</returns>
		//----------------------------------------------------------------------
		public string[] EnumAllCompanyUsers(int companyId, bool onlyNonNTUsers)
		{
			return loginManager.EnumAllCompanyUsers(companyId, onlyNonNTUsers);
		}

		/// <summary>
		/// Fornisce l'elenco delle aziende utilizzabili dall'utente specificato
		/// </summary>
		/// <param name="userName">Il nome dell'utente applicativo</param>
		/// <returns>L'elenco delle aziende</returns>
		//----------------------------------------------------------------------
		public string[] EnumCompanies(string userName)
		{
			return loginManager.EnumCompanies(userName);
		}

		/// <summary>
		/// Dice se l'utente è in sicurezza integrata oppure no
		/// </summary>
		/// <param name="userName">Il nome dell'utente applicativo</param>
		/// <returns>L'elenco delle aziende</returns>
		//----------------------------------------------------------------------
		public bool IsIntegrateSecurityUser(string userName)
		{
			return loginManager.IsIntegratedSecurityUser(userName);
		}

		/// <summary>
		/// Dice se l'utente è abilitato a usare calfloating o no
		/// </summary>
		/// <param name="userName">Il nome dell'utente applicativo</param>
		//----------------------------------------------------------------------
		public bool IsFloatingUser(string userName, out bool floating)
		{
			return loginManager.IsFloatingUser(userName, out floating);
		}

		/// <summary>
		/// Dice se l'utente è abilitato a usare EasyBuilder
		/// </summary>
		public bool IsEasyBuilderDeveloper(string authenticationToken)
		{
			return loginManager.IsEasyBuilderDeveloper(authenticationToken);
		}
        
        /// <summary>
        /// Dice se l'utente è abilitato a usare cal web o no
        /// </summary>
        /// <param name="userName">Il nome dell'utente applicativo</param>
        //----------------------------------------------------------------------
        public bool IsWebUser(string userName, out bool web)
        {
            return loginManager.IsWebUser(userName, out web);
        }

        /// <summary>
        /// Dice se l'utente è winAuth
        /// </summary>
        /// <param name="userName">Il nome dell'utente applicativo</param>
        //----------------------------------------------------------------------
        public bool IsWinNT(int loginId)
        {
            winNTAuthentication = loginManager.IsWinNT(loginId);
            return winNTAuthentication;
        }

        /// <summary>
        /// Restituisce il numero di utenti applicativi che si sono autenticati
        /// </summary>
        /// <returns>Numero di utenti</returns>
        //-----------------------------------------------------------------------
        public int GetLoggedUsersNumber()
		{
			return loginManager.GetLoggedUsersNumber();
		}

		/// <summary>
		/// Restituisce il numero di utenti appartenenti ad un'azienda che si sono autenticati
		/// </summary>
		/// <param name="companyId">ID dell'azienda</param>
		/// <returns>Numero di utenti</returns>
		//-----------------------------------------------------------------------
		public int GetCompanyLoggedUsersNumber(int companyId)
		{
			return loginManager.GetCompanyLoggedUsersNumber(companyId);
		}

        //----------------------------------------------------------------------
        public bool IsUserLogged()
        {
            return IsUserLogged(loginId);
        }

        //----------------------------------------------------------------------
		public bool IsUserLogged(int loginID)
		{
			return loginManager.IsUserLogged(loginID);
		}

		//---------------------------------------------------------------------
		public string GetLoggedUsers()
		{
			return loginManager.GetLoggedUsers();
		}

        //---------------------------------------------------------------------
        public string GetLoggedUsersAdvanced(string token)
        {
            return loginManager.GetLoggedUsersAdvanced(token);
        }

		//---------------------------------------------------------------------
		public int GetProxySupportVersion()
		{
			return loginManager.GetProxySupportVersion();
		}

		/// <summary>
		/// Metodo per verificare la connessione a login manager server
		/// </summary>
		/// <returns>true se ha successo</returns>
		//----------------------------------------------------------------------
		public bool IsAlive()
		{
			return loginManager.IsAlive();
		}


		/// <summary>
		/// Fornisce informazioni su un token di autenticazione
		/// </summary>
		/// <param name="authenticationToken">token di autenticazione</param>
		/// <param name="loginId">ID dell'utente applicativo</param>
		/// <param name="companyId">ID dell'azienda</param>
		/// <param name="webLogin">true se è una login eseguita dal web, false se è stata effettuata da un'applicazione smart client</param>
		/// <returns>true se le informazioni sono state reperite</returns>
		//----------------------------------------------------------------------
		public bool GetAuthenticationInformations(string authenticationToken, out int loginId, out int companyId, out bool webLogin)
		{
			loginId		= -1;
			companyId	= -1;
			webLogin	= false;

			return loginManager.GetAuthenticationInformations(authenticationToken, out loginId, out companyId, out webLogin);
		}

		//----------------------------------------------------------------------
		public bool GetAuthenticationNames(string authenticationToken, out string userName, out string companyName)
		{
			userName	= string.Empty;
			companyName	= string.Empty;

			return loginManager.GetAuthenticationNames(authenticationToken, out userName, out companyName);
		}

		/// <summary>
		/// Metodo che dice se un'azienda è posta o meno sotto sicurezza
		/// </summary>
		/// <param name="companyId">ID dell'azienda</param>
		/// <returns>true se l'azienda risulta sotto sicurezza</returns>
		//-----------------------------------------------------------------------
		public bool IsCompanySecured(int companyId)
		{
			return loginManager.IsCompanySecured(companyId);
		}

		/// <summary>
		/// Restituisce l'edizione dell'applicazione (standard, professional, enterpise)
		/// </summary>
		/// <returns>la stringa con l'edizione</returns>
		//---------------------------------------------------------------------------
		public string GetEdition()
		{
			return loginManager.GetEdition();
		}

		/// <summary>
		/// Ritorna l'ISO stato (nazionalità) dell'utente
		/// </summary>
		/// <returns>Stringa con il codice nazione es. it</returns>
		//---------------------------------------------------------------------------
		public string GetCountry()
		{
			return loginManager.GetCountry();
		}

        private string userInfoXml = string.Empty;

        private string userInfoLicensee = string.Empty;

        public string UserInfo
        {
            get
            {
                if (userInfoXml == string.Empty)
                {
                    userInfoXml = loginManager.GetUserInfo();
                }
                return userInfoXml;
            }
        }

        public string UserInfoLicensee
        {
            get
            {
                if (userInfoLicensee == string.Empty)
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.InnerXml = UserInfo;
                    System.Xml.XmlNode pUserInfoNameNode = doc.SelectSingleNode("/UserInfo/Name");
                    if (pUserInfoNameNode != null)
                        userInfoLicensee = pUserInfoNameNode.InnerText;
                }
                return userInfoLicensee;
            }
        }

        /// <summary>
        /// Legge il UserInfo restituisce il suo contenuto
        /// </summary>
        /// <returns>Stringa con l'xml di userinfo</returns>
        //---------------------------------------------------------------------------
        public string GetUserInfo()
        {
            return UserInfo;
        }

		//---------------------------------------------------------------------------
		public byte[] GetConfigurationStream()
		{
			return loginManager.GetConfigurationStream();
		}
		
		/// <summary>
		/// Restituisce l'ID dell'utente specificato nel file UserInfo
		/// </summary>
		/// <returns>ID dell'utente</returns>
		//---------------------------------------------------------------------------
		public string GetUserInfoID()
		{	
			return loginManager.GetUserInfoID();
		}

		//---------------------------------------------------------------------------
		public string GetUserDescriptionById(int loginId)
		{
			return loginManager.GetUserDescriptionById(loginId);
		}
		//---------------------------------------------------------------------------
		public string GetUserDescriptionByName(string login)
		{
			return loginManager.GetUserDescriptionByName(login);
		}

		//---------------------------------------------------------------------------
		public ModuleNameInfo[] GetArticlesWithNamedCal()
		{
			return loginManager.GetArticlesWithNamedCal();
		}

        //---------------------------------------------------------------------------
        public ModuleNameInfo[] GetArticlesWithFloatingCal()
		{
            return loginManager.GetArticlesWithFloatingCal();
		}

        //---------------------------------------------------------------------------
        public void RefreshFloatingMark()
		{
            loginManager.RefreshFloatingMark();
		}

		//----------------------------------------------------------------------
		public void RefreshSecurityStatus()
		{
			loginManager.RefreshSecurityStatus();
		}

		//---------------------------------------------------------------------------
		public string GetBrandedKey(string source)
		{
			return loginManager.GetBrandedKey(source);
		}

		//---------------------------------------------------------------------------
		public int GetDBCultureLCID()
		{
			return loginManager.GetDBCultureLCID(this.CompanyId);
		}

		//---------------------------------------------------------------------------
		public int GetDBCultureLCID(int companyID)
		{
			return loginManager.GetDBCultureLCID(companyID);
		}

		//---------------------------------------------------------------------------
        public string GetInstallationVersion(out string productName, out DateTime buildDate, out DateTime instDate, out int build)
		{
            //out string productName, out DateTime buildDate, out DateTime instDate, out int build
			return loginManager.GetInstallationVersion(out productName, out buildDate, out instDate, out build);
		}

        //---------------------------------------------------------------------------
        public string GetInstallationVersion()
        {
            string productName; DateTime buildDate; DateTime instDate; int build;
            return GetInstallationVersion(out productName, out buildDate, out instDate, out build);
        }

		#endregion
		
		#region funzioni legate ai messaggi

		/// <summary>
		/// Ritorna la lista di messaggi che sono stati ricevuti e conservati 
		/// sul server per ogni utente
		/// </summary>
		//----------------------------------------------------------------------
		public IAdvertisement[] GetMessages(string	authenticationToken)
		{
			return loginManager.GetMessagesQueue(authenticationToken);
		}
        /// <summary>
		/// Ritorna la lista di messaggi IMMEDIATI che sono stati ricevuti e conservati 
		/// sul server per ogni utente
		/// </summary>
		//----------------------------------------------------------------------
		public IAdvertisement[] GetImmediateMessages(string	authenticationToken)
		{
			return loginManager.GetImmediateMessagesQueue(authenticationToken);
		}

		/// <summary>
		/// Ritorna la lista di messaggi che sono stati già letti 
		///  per ogni utente
		/// </summary>
		//----------------------------------------------------------------------
		public IAdvertisement[] GetOldMessages (string authenticationToken)
		{
			return loginManager.GetOldMessages(authenticationToken);
		}

		/// <summary>
		/// Ritorna la lista di messaggi che sono stati ricevuti e conservati 
		/// sul server per ogni utente
		/// </summary>
		//----------------------------------------------------------------------
		public void SetMessageRead(string authenticationToken, string messageID)
		{
			loginManager.SetMessageRead(authenticationToken, messageID);
		}

        //----------------------------------------------------------------------
        public void DeleteMessageFromQueue(string messageID)
        {
            loginManager.DeleteMessageFromQueue(messageID);
        }

        //----------------------------------------------------------------------
        public void PurgeMessageByTag(string tag, string user = null)
        {
            loginManager.PurgeMessageByTag(tag, user);
        }

        #endregion

        #region funzioni legate all'utente

        //----------------------------------------------------------------------
        public void SetClientData(ClientData cd)
		{
            loginManager.SetClientData(cd);
		}

		/// <summary>
		/// Questo metodo verifica che la coppia utente/password sia valida
		/// e restituisce tutte le informazioni disponiili per quell'utente
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <param name="?"></param>
		/// <returns></returns>
		//----------------------------------------------------------------------
		public int ValidateUser(string	userName, string password, bool winNTAuthentication)
		{
			
			loginId						= -1;
			userCompanies				= null;
			userCannotChangePassword	= false;
			userMustChangePassword		= false;	
			expiredDatePassword			= DateTime.MaxValue;
			passwordNeverExpired		= false;
			expiredDateCannotChange		= false;
			
			int validateResult = loginManager.ValidateUser
						(
							userName, 
							password,
							winNTAuthentication,
							out userCompanies, 
							out loginId,
							out userCannotChangePassword,
							out userMustChangePassword,		
							out expiredDatePassword,		
							out passwordNeverExpired,		
							out expiredDateCannotChange
						);
			
			//Se la chiamata ha avuto successo seto i membri interni e lo stato
			if (validateResult == (int)LoginReturnCodes.NoError)
			{
				this.userName				= userName;
				this.password				= password;
				this.winNTAuthentication	= winNTAuthentication;

				loginManagerState = LoginManagerState.Validated;
			}
			
			return validateResult;
		}		
		
		/// <summary>
		/// Cambia la password di un utente applicativo
		/// </summary>
		/// <param name="userName">nome dell'utente applicativo</param>
		/// <param name="oldPassword">Password precedente</param>
		/// <param name="newPassword">Nuova password</param>
		/// <returns>0 se il cambio ha avuto successo, > 0 in caso di errore</returns>
		//----------------------------------------------------------------------
		public int ChangePassword(string userName, string oldPassword, string newPassword)
		{
			int result = loginManager.ChangePassword(userName, oldPassword, newPassword);
			if (result == (int)LoginReturnCodes.NoError)
			{
				this.userMustChangePassword = false;
				this.expiredDatePassword = expiredDatePassword.AddDays(30);
				this.password = newPassword;
			}
			return result;
		}

        //----------------------------------------------------------------------
        public int ChangePassword(string newPassword)
        {
            return ChangePassword(this.userName, this.password, newPassword);
        }

        /// <summary>
		/// Dice se un utente è amministatore dell'Area Riservata su sito web
		/// </summary>
		/// <returns>true se l'utente è amministratore dell'Area Riservata su sito web</returns>
		//-----------------------------------------------------------------------
		public bool UserCanAccessWebSitePrivateArea()
		{
			return loginManager.UserCanAccessWebSitePrivateArea(LoginId);
		}

		#endregion

		#region funzioni legate al token di attivazione e quindi ad una login
		
		/// <summary>
		/// Autentica un utente applicativo
		/// </summary>
		/// <param name="userName">Utente applicativo</param>
		/// <param name="password">Password dell'utente</param>
		/// <param name="winNTAuthentication">Indica se l'utente è soggetto a sicurezza integrata</param>
		/// <param name="companyName">Nome azienda</param>
		/// <param name="askingProcess">Processo richiedente un'autenticazione</param>
		/// <param name="overWriteLogin">Se lo stesso utente risulta essere autenticato 
		/// questo flag imposto a true fa si che la precedente autenticazione non sia più valida e ne genera una nuova</param>
		/// <returns>0 se l'utente si autentica.</returns>
		//----------------------------------------------------------------------
		public int Login
			(
				string	userName, 
				string	password, 
				bool	winNTAuthentication, 
				string	companyName, 
				string	askingProcess, 
				bool	overWriteLogin
			)
		{
			int validateReturn = ValidateUser(userName, password, winNTAuthentication);
			if (validateReturn != (int)LoginReturnCodes.NoError)
				return (int)LoginReturnCodes.GenericLoginFailure;
      
            return Login(companyName, askingProcess, overWriteLogin);
		}

		//----------------------------------------------------------------------
		public int SsoLogin(string ssoToken)
		{
			LoginProperties properties = new LoginProperties();
			properties.SsoToken = ssoToken;
			int loginResult = loginManager.SsoLogin(ref properties);

			if (loginResult != (int)LoginReturnCodes.NoError)
				return loginResult;

			loginManagerState = LoginManagerState.Logged;

			GetLoginInformation(properties.AuthenticationToken);

			return (int)LoginReturnCodes.NoError;
		}
		
            //----------------------------------------------------------------------
        public string SsoLoggedUser(string ssoToken)
		{
			LoginProperties properties = new LoginProperties();
			properties.SsoToken = ssoToken;
            loginManager.SsoLoggedUser(ref properties);

			GetLoginInformation(properties.AuthenticationToken);

            return userName;
		}

        public string GetIToken(string authenticationTocken)
        {
            try
            {
                //ddd int loginResult = loginManager.LoginViaInfinityToken2(cryptedToken, username, password, company, out authToken);

                return loginManager.GetIToken(authenticationTocken);
            }
            catch
            {
                return "";
            }
        }

        //----------------------------------------------------------------------
        public int LoginViaInfinityToken(string cryptedToken, string username, string password, string company)
        {
            string authToken = null;
            try
            {
                int loginResult = loginManager.LoginViaInfinityToken2(cryptedToken,username,  password,  company, out authToken);
                if (loginResult != (int)LoginReturnCodes.NoError)
                    return loginResult;

                loginManagerState = LoginManagerState.Logged;
                this.authenticationToken = authToken;
                return (int)LoginReturnCodes.NoError;

            }
            catch
            {
                return (int)LoginReturnCodes.GenericLoginFailure;
            }

        }
      
        /// <summary>
        /// Autentica un utente applicativo che è già stato riconosciuto con la ValidateUser
        /// </summary>
        /// <param name="companyName">Nome azienda</param>
        /// <param name="askingProcess">Processo richiedente un'autenticazione</param>
        /// <param name="overWriteLogin">Se lo stesso utente risulta essere autenticato 
        /// questo flag imposto a true fa si che la precedente autenticazione non sia più valida e ne genera una nuova</param>
        /// <returns>0 se l'utente si autentica.</returns>
        //----------------------------------------------------------------------
        public int Login(string companyName, string askingProcess, bool overWriteLogin, string macIp = null)
		{
			if (loginManagerState == LoginManagerState.UnInitialized)
				throw new LoginManagerException(LoginManagerError.UnInitialized);

			if (loginManagerState == LoginManagerState.Logged && !overWriteLogin)
				return (int)LoginReturnCodes.UserAlreadyLoggedError;

			this.companyName					= companyName;
		
			admin								= false;
			authenticationToken					= string.Empty;
			companyId							= -1;
			dbName								= string.Empty;
			dbServer							= string.Empty;
			providerId							= -1;
			security							= false;
			auditing							= false;
			useKeyedUpdate						= false;
			transactionUse						= false;
			preferredLanguage					= string.Empty;
			applicationLanguage					= string.Empty;
			preferredCompanyLanguage			= string.Empty;
			applicationCompanyLanguage			= string.Empty;
			providerName						= string.Empty;
			providerDescription					= string.Empty;
			useConstParameter					= false;
			stripTrailingSpaces					= false;
			providerCompanyConnectionString		= string.Empty;
			nonProviderCompanyConnectionString	= string.Empty;
			dbUser								= string.Empty;
			isAdminWebSitePrivateAreaInitialize = false;
            ////TODO ILARIA MACADDRESS
            //string macAddress = string.Empty;
            //try
            //{
            //    macAddress = LocalMachine.GetMacAddress() + "-**-" + LocalMachine.GetIPAddress();
            //}
            //catch { }
            //parametro morto
            string activationDB				    = string.Empty;

            int loginResult = loginManager.Login2
                (
                ref this.userName,
                ref this.companyName,
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

			if (loginResult != (int)LoginReturnCodes.NoError)
				return loginResult;

			loginManagerState = LoginManagerState.Logged;

			return (int)LoginReturnCodes.NoError;
		}
		
		/// <summary>
		/// Comunica al WebService che l'utente si è disconnesso.
		/// </summary>
		//----------------------------------------------------------------------
		public void LogOff()
		{
			try
			{
				this.LogOff(authenticationToken);

				loginManagerState = LoginManagerState.Validated;
			}
			catch
			{

			}
		}

		/// <summary>
		/// Comunica al WebService che l'utente si è disconnesso.
		/// </summary>
		//----------------------------------------------------------------------
		public void LogOff(string authenticationToken)
		{
			try
			{
				loginManager.LogOff(authenticationToken);
			}
			catch//(Exception exc)
			{
				//Debug.Fail(exc.Message);
			}
		}

        //----------------------------------------------------------------------
        public void SSOLogOff(string cryptedToken)
        {
            try
            {
                loginManager.SSOLogOff(cryptedToken);

                loginManagerState = LoginManagerState.UnInitialized;
            }
            catch
            {

            }
        }
        //---------------------------------------------------------------------------
        public LoginSlotType GetCalType(string authenticationToken)
		{
			return (LoginSlotType)loginManager.GetCalType(authenticationToken);
		}

		//---------------------------------------------------------------------------
		public string GetMasterProductBrandedName()
		{
			return loginManager.GetMasterProductBrandedName();
		}

		//---------------------------------------------------------------------------
		public string GetBrandedProducerName()
		{
			return loginManager.GetBrandedProducerName();
		}

		//---------------------------------------------------------------------------
		public string GetBrandedProductTitle()
		{
			return loginManager.GetBrandedProductTitle();
		}

		//---------------------------------------------------------------------------
		public string GetMasterSolutionBrandedName()
		{
			return loginManager.GetMasterSolutionBrandedName();
		}


        //---------------------------------------------------------------------------
        public string GetMasterSolution()
        {
            return loginManager.GetMasterSolution();
        }


        /// <summary>
        /// Restituisce la stringa di connessione al database di sistema
        /// </summary>
        /// <param name="authenticationToken">token di autenticazione</param>
        /// <returns>Stringa di connessione al database di sistema in chiaro.</returns>
        //---------------------------------------------------------------------------
        public string GetSystemDBConnectionString(string authenticationToken)
		{
			if (loginManagerState != LoginManagerState.Logged || String.IsNullOrWhiteSpace(authenticationToken))
				throw new LoginManagerException(LoginManagerError.NotLogged);

			return loginManager.GetSystemDBConnectionString(authenticationToken);
		}

        /// <summary>
		/// Restituisce la stringa di connessione al database del documentale
		/// </summary>
		/// <param name="authenticationToken">token di autenticazione</param>
		/// <returns>Stringa di connessione al database di sistema in chiaro.</returns>
		//---------------------------------------------------------------------------
        public string GetDMSConnectionString(string authenticationToken)
		{
			if (loginManagerState != LoginManagerState.Logged || String.IsNullOrWhiteSpace(authenticationToken))
				throw new LoginManagerException(LoginManagerError.NotLogged);

            return loginManager.GetDMSConnectionString(authenticationToken);
		}

		//---------------------------------------------------------------------------
		public List<DmsDatabaseInfo> GetDMSDatabasesInfo(string authenticationToken)
		{
			if (String.IsNullOrWhiteSpace(authenticationToken))
				throw new LoginManagerException(LoginManagerError.NotLogged);

			DmsDatabaseInfo[] dmsArray = loginManager.GetDMSDatabasesInfo(authenticationToken);
			return new List<DmsDatabaseInfo>(dmsArray);
		}

        //---------------------------------------------------------------------------
        public List<DataSynchroDatabaseInfo> GetDataSynchroDatabasesInfo(string authenticationToken)
        {
            if (String.IsNullOrWhiteSpace(authenticationToken))
                throw new LoginManagerException(LoginManagerError.NotLogged);

            DataSynchroDatabaseInfo[] dsArray = loginManager.GetDataSynchroDatabasesInfo(authenticationToken);
            return new List<DataSynchroDatabaseInfo>(dsArray);
        }

        //---------------------------------------------------------------------------
        public List<TbSenderDatabaseInfo> GetCompanyDatabasesInfo(string authenticationToken)
		{
			if (String.IsNullOrWhiteSpace(authenticationToken))
				throw new LoginManagerException(LoginManagerError.NotLogged);

			TbSenderDatabaseInfo[] dmsArray = loginManager.GetCompanyDatabasesInfo(authenticationToken);
			return new List<TbSenderDatabaseInfo>(dmsArray);
		}

        //---------------------------------------------------------------------------
        public bool ConfirmToken(string authenticationToken, string procType)
		{
            return loginManager.ConfirmToken(authenticationToken, procType);
            
        }

        
		//---------------------------------------------------------------------------
		public bool GetLoginInformation(string authenticationToken)
		{
			string processName = string.Empty;
			bool easyBuilderDeveloper = false;

			if (
				loginManager.GetLoginInformation
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
                        out rowSecurity

					)
				)
			{
				this.authenticationToken = authenticationToken;
				loginManagerState = LoginManagerState.Logged;
				return true;
			}

			return false;
		}
         //---------------------------------------------------------------------------
        public DBNetworkType GetDBNetworkType()
        {
            return (DBNetworkType)loginManager.GetDBNetworkType();
        }
        //---------------------------------------------------------------------------
        public string GetEditionType()
        {
            return loginManager.GetEditionType();
        }

        //---------------------------------------------------------------------------
        public string GetDatabaseType()
		{
			if (this.providerName == string.Empty)
				return string.Empty;

			return loginManager.GetDatabaseType(providerName);
		}

		//---------------------------------------------------------------------------
		public bool IsValidToken(string authenticationToken)
		{
			return loginManager.IsValidToken(authenticationToken);
		}

		//---------------------------------------------------------------------------
        public int GetCalNumber(out int unNamedCal, out int gdiConcurrent, out int officeCal, out int tpCal, out int wmsCal, out int manufacturingCal)
		{
            return loginManager.GetCalNumber2(out unNamedCal, out gdiConcurrent, out officeCal, out tpCal, out  wmsCal, out manufacturingCal);
		}

		//---------------------------------------------------------------------------
		public void ReloadConfiguration()
		{
			loginManager.ReloadConfiguration();
		}
		#endregion

		#region funzioni che vengono chiamate dalla console in risposta agli eventi di cancellazione utente / company
		/// <summary>
		/// Comunica al server che è stata cancellata un'associazione tra utente ed azienda
		/// </summary>
		/// <param name="loginId">ID dell'utente</param>
		/// <param name="companyId">ID dell'azienda</param>
		/// <returns>true se l'eliminazione ha avuto successo</returns>
		//---------------------------------------------------------------------------
		public bool DeleteAssociation(int loginId, int companyId, string authenticationToken)
		{
			return loginManager.DeleteAssociation(loginId, companyId, authenticationToken);
		}

		/// <summary>
		/// Comunica al server che è stato cancellato un utente
		/// </summary>
		/// <param name="loginId">ID dell'utente</param>
		/// <returns>true se l'eliminazione ha avuto successo</returns>
		//---------------------------------------------------------------------------
		public bool DeleteUser(int loginId, string authenticationToken)
		{
			return loginManager.DeleteUser(loginId, authenticationToken);
		}

		/// <summary>
		/// Comunica al server che è stata cancellata un'azienda
		/// </summary>
		/// <param name="companyId">ID dell'azienda</param>
		/// <returns>true se l'eliminazione ha avuto successo</returns>
		//---------------------------------------------------------------------------
		public bool DeleteCompany(int companyId, string authenticationToken)
		{
			return loginManager.DeleteCompany(companyId, authenticationToken);
		}

		/// <summary>
		/// ricarica la tabella LoginArticles
		/// </summary>
		/// <param name="authenticationToken"></param>
		//---------------------------------------------------------------------------
		public void ReloadUserArticleBindings(string authenticationToken)
		{
			loginManager.ReloadUserArticleBindings(authenticationToken);
		}
		#endregion

		#region funzioni per il trace

		//----------------------------------------------------------------------
		public void TraceAction
			(
			string			company, 
			string			login, 
			TraceActionType type, 
			string			processName, 
			string			winUser, 
			string			location
			)
		{
			loginManager.TraceAction
				(
				company, 
				login, 
				Convert.ToUInt16(type), 
				processName, 
				winUser, 
				location
				);
		}

		//---------------------------------------------------------------------------
		public bool HasUserAlreadyChangedPasswordToday(string user)
		{
			return loginManager.HasUserAlreadyChangedPasswordToday(user);
		}

        //---------------------------------------------------------------------------
        public bool Sql2012Allowed(string authToken)
		{
            return loginManager.Sql2012Allowed(authToken);
		}

		#endregion

		#region funzioni per la security
		//---------------------------------------------------------------------------
		public bool CanUseNamespace(string nameSpace, GrantType grantType)
		{
			return loginManager.CanUseNamespace(nameSpace, authenticationToken, grantType);
		}

		//---------------------------------------------------------------------------
		public bool IsSecurityLightEnabled()
		{
			return loginManager.IsSecurityLightEnabled();
		}

		//----------------------------------------------------------------------
        public bool IsSecurityLightAccessAllowed(string nameSpace, bool unattended)
		{
            return loginManager.IsSecurityLightAccessAllowed(nameSpace, authenticationToken, unattended);
		}

		#endregion

		//----------------------------------------------------------------------
		public string GetConfigurationHash()
		{
			return loginManager.GetConfigurationHash();
		}

		//----------------------------------------------------------------------
		public string GetMainSerialNumber()
		{
			return loginManager.GetMainSerialNumber();
		}

		//----------------------------------------------------------------------
		public string GetMLUExpiryDate()
		{
			return loginManager.GetMLUExpiryDate();
		}

        //----------------------------------------------------------------------
        public bool VerifyDBSize()
        {
            return loginManager.VerifyDBSize();
        }

        //----------------------------------------------------------------------
        public ISecurity NewSecurity(string company, string user, bool applySecurityFilter)
        {
            Security security = new Security    //superclasse di MenuSecurityFilter
                                        (
                                            InstallationData.ServerConnectionInfo.SysDBConnectionString,
                                            company,
                                            user,
                                            applySecurityFilter
                                        );
            return security;
        }

        public bool GetProxySettings(out string server, out int port)
        {
            server = string.Empty; port = 0;
            ProxySettings settings = GetProxySettings();
            if (settings == null)
                return false;
            server = settings.HttpProxy.Server;
            port = settings.HttpProxy.Port;
            return true;
        }

        #region funzioni per i Balloon

        //----------------------------------------------------------------------
        public void SendBalloon(string authenticationToken, string bodyMessage, MessageType messageType = MessageType.Updates, List<string> recipients = null)
        {
			loginManager.SendBalloon(authenticationToken, bodyMessage, messageType, (recipients != null ? recipients.ToArray() : null));
        }

        //----------------------------------------------------------------------
        public void AdvancedSendBalloon
            (
                string authenticationToken,
                string bodyMessage,
                DateTime expiryDate,
                MessageType messageType = MessageType.Updates,
                string[] recipients = null,
                MessageSensation sensation = MessageSensation.Information,
                bool historicize = true,
                bool immediate = false,
                int timer = 0,
                 string tag = null
            )
        {
            loginManager.AdvancedSendTaggedBalloon
            (
                authenticationToken,
                bodyMessage,
                expiryDate,
                (int)messageType,
                recipients,
                (int)sensation,
                historicize,
                immediate,
                timer, tag
                );
        }
        #endregion

    }
}
