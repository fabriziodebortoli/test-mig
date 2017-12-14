using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;


namespace Microarea.TaskBuilderNet.Core.Applications
{
	//=========================================================================
	public class UserInfo : IDisposable	
	{
		private const string			SessionKey				= "UserInfoKey";

		public bool						Valid					= true;
		public string					ErrorExplain			= "";
		public int						ErrorCode				= 0;
		public string					Company					= string.Empty;
		public int						CompanyId				= -1;
		public string					User					= string.Empty;
		public int						LoginId					= -1;
		public string					Password				= string.Empty;
		public string					CompanyDbConnection		= string.Empty;
		public string					Provider				= string.Empty;
		public bool						Admin					= false;
		public IPathFinder				PathFinder				= null;
		public Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager			LoginManager			= null;
		public Microarea.TaskBuilderNet.Core.WebServicesWrapper.TbServices				TbServices				= null;
		public bool						OverwriteLogin			= false;
		public LoginSlotType			CalType					= LoginSlotType.Invalid;
		public ActivationState			ActivationState         = ActivationState.Undefined;

		public bool						UseApproximation		= false; // enable TaskBuilder Approximation for real
		public string					ImpersonatedUser		= string.Empty;
		public CultureInfo				CompanyCulture			= CultureInfo.InvariantCulture;
		public bool						UseUnicode				= false;
		public bool						StripTrailingSpaces		= true;
		private DateTime				applicationDate			= DateTime.Today;
		private string					registrationMessage		= null;

		// servono ad evitare di riprovare a riconnettersi se c'è stato un errore
		// in fase di connessione a TbLoader.exe
		private Exception				tbLoaderInterfaceException	= null;
		private ITbLoaderClient         tbLoaderInterface		= null;
		/// <summary>
		/// Token che mantiene il lock globale finche' l'utente e' connessso, per impedire disinstallazioni
		/// </summary>
		private ApplicationLockToken	lockToken = null;
        
        //---------------------------------------------------------------------
        private BrandLoader brandLoader = null;

        public BrandLoader Brand
        {
            get
            {
                if (brandLoader == null)
                        brandLoader = new BrandLoader();
                return brandLoader;
        }}
       
		//---------------------------------------------------------------------
		public bool IsDemo
		{
			get
			{
				if (registrationMessage == null)
					LoginManager.IsRegistered(out registrationMessage, out ActivationState);
				
				return (ActivationState == ActivationState.Demo || ActivationState == ActivationState.DemoWarning);
			}
		}

		//--------------------------------------------------------------------------------
		public string RegistrationMessage 
		{
			get
			{
				if (registrationMessage == null)
					LoginManager.IsRegistered(out registrationMessage, out ActivationState);
				return registrationMessage;
			}
		}

		//---------------------------------------------------------------------
		public DateTime ApplicationDate
		{
			get { return applicationDate; }
			set { applicationDate = value; tbLoaderInterface = null; }
		}
		//---------------------------------------------------------------------
		public string ApplicationKey
		{
			get { return LoginManager.AuthenticationToken; }
		}

		//---------------------------------------------------------------------
		public bool IsTbLoaderInstantiated (string authToken)
		{
			//se l'oggetto e' bloccato, allora sto instanziando un tb
			//per avvantaggiarmene in un momento successivo, in ogni caso 
			//attualmente non ho ancora istanziato alcun tbloader
			if (!Monitor.TryEnter(this))
				return false;
			try
			{
				return TbServices.IsTbLoaderInstantiated(authToken);
			}
			finally 
			{
				Monitor.Exit(this);
			}
		}

		// Gestisce il caricamento TaskBuilder completamente autenticato, dal pool di istanze.
		// Si aggancia al TB running in modalità unattended se esiste 
		// e se è nella stessa applicationDate, altrimenti ne istanzia uno nuovo e ne setta la
		// data alla corrente applicationDate
		//
		//---------------------------------------------------------------------
		public ITbLoaderClient GetTbLoaderInterface()
		{
			lock (this)
			{
				// Il TbLocalizer non esegue login e pertanto non può istanziare
				// interfacce a TbLoader ne eseguire chiamate a WebService esposti da lui
				if (LoginManager == null)
					throw new ApplicationException(ApplicationsStrings.LoginNotYetPerformed);

				if (tbLoaderInterfaceException != null)
					throw tbLoaderInterfaceException;

				if (tbLoaderInterface != null)
				{
					if (tbLoaderInterface.Available)
						return tbLoaderInterface;
					tbLoaderInterface = null;
				}

				try
				{
					tbLoaderInterface = TbServices.CreateTB(PathFinder, LoginManager.AuthenticationToken, LoginManager.CompanyName, tbLoaderInterface, ApplicationDate, true);
				}
				catch (TbServicesException e)
				{
					tbLoaderInterface = null;
					tbLoaderInterfaceException = e;
					throw (e);
				}

				try
				{
					tbLoaderInterface.UseRemoteInterface(true);
				}
				catch (Exception e)
				{
					tbLoaderInterface = null;
					tbLoaderInterfaceException = e;
					throw (e);
				}
				return tbLoaderInterface;
			}
		}

		//---------------------------------------------------------------------
		public UserInfo ()
		{
		}
		
		//---------------------------------------------------------------------
		public static UserInfo FromSession()
		{
			UserInfo ui = TBWebContext.Current.FromSession(UserInfo.SessionKey) as UserInfo;

			if (ui != null && !ui.Valid)
				return null;
			
			return ui;
		}

		//---------------------------------------------------------------------
		public static void ToSession(UserInfo ui)
		{
			TBWebContext.Current.ToSession(UserInfo.SessionKey, ui);
		}

		//---------------------------------------------------------------------
		public string SsoLogin(string ssoToken)
		{
			//Istanzio un wrapper locale con cui fare login.
			LoginManager LoginManager = new LoginManager();

			ErrorCode = LoginManager.SsoLogin(ssoToken);

			if (ErrorCode != 0)
				return String.Empty;

			//Ottenuto il token mi valido proprio come farebbe il consueto metodo Login.
			if (Login(LoginManager.AuthenticationToken))
				return LoginManager.UserName;

			return String.Empty;
		}
        //---------------------------------------------------------------------
        public string SsoLoggedUser(string ssoToken)
		{
			//Istanzio un wrapper locale con cui fare login.
			LoginManager LoginManager = new LoginManager();

            string username = LoginManager.SsoLoggedUser(ssoToken);
            
			if (ErrorCode != 0)
				return String.Empty;

			//Ottenuto il token mi valido proprio come farebbe il consueto metodo Login.
			if (Login(LoginManager.AuthenticationToken))
				return LoginManager.UserName;

			return String.Empty;
		}
		
		//---------------------------------------------------------------------
		public bool Login(string authenticationToken)
		{
			LoginManager = new LoginManager();
			TbServices = new TbServices();
			
			if (!LoginManager.GetLoginInformation(authenticationToken))
			{
				ErrorExplain = string.Format(ApplicationsStrings.PasswordError, "");
				return false;
			}

			User				= LoginManager.UserName;
			Company				= LoginManager.CompanyName;
			Admin				= LoginManager.Admin;
			LoginId				= LoginManager.LoginId;
			CompanyId			= LoginManager.CompanyId;
            CompanyDbConnection = AdjustConnectionString(LoginManager.NonProviderCompanyConnectionString);
            CompanyDbConnection = CompanyDbConnection.Replace("false", "true");

            Provider			= LoginManager.ProviderName;
			UseUnicode			= LoginManager.UseUnicode;
			CalType				= LoginManager.GetCalType(authenticationToken);
		
			PathFinder = new PathFinder(Company, User);

			//acquisisco il lock globale per evitare disinstallazioni mentre uso il programma
			lockToken = ApplicationSemaphore.Lock(BasePathFinder.BasePathFinderInstance.GetSemaphoreFilePath());

			return true;
		}

        //---------------------------------------------------------------------
		//modifica la stringa di connessione in modo da farle usare il connection pool
        private string AdjustConnectionString(string connectionString)
        {
            string pattern = @"Pooling\s*=\s*false";
            return Regex.Replace(connectionString, pattern, "Pooling=true", RegexOptions.IgnoreCase);
        }

		//---------------------------------------------------------------------
		public bool Login(string user, string password, string company, bool useNTAuth)
		{
			this.User = user;
			this.Password = password;
			this.Company = company;
			PathFinder = new PathFinder(company, user);

			// crea ed esegue la login attraverso il WebService
			LoginManager = new LoginManager();
			TbServices = new TbServices();

			ErrorCode = LoginManager.ValidateUser(user, password, useNTAuth);
			if (ErrorCode != 0)
			{
				ErrorExplain = string.Format(ApplicationsStrings.PasswordError, Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManagerWrapperStrings.GetString(ErrorCode));
				return false;
			}

			ErrorCode = LoginManager.Login(company, ProcessType.EasyLook, OverwriteLogin);
			if (ErrorCode != 0)
			{
				ErrorExplain = string.Format(ApplicationsStrings.LoginError, Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManagerWrapperStrings.GetString(ErrorCode));
				return false;
			}

			this.CalType = LoginManager.GetCalType(LoginManager.AuthenticationToken);

			// recupera l'informazione relativa allo stato di amministratore dell'utente
			this.Admin = LoginManager.Admin;
			this.LoginId = LoginManager.LoginId;
			this.CompanyId = LoginManager.CompanyId;

			// recupera via Soap con query al SystemDatabase la stringa di connessione per la Company specificata
			this.CompanyDbConnection	= AdjustConnectionString(LoginManager.NonProviderCompanyConnectionString);
			this.Provider				= LoginManager.ProviderName;

			LoginManager.GetLoginInformation(LoginManager.AuthenticationToken);
			this.UseUnicode = LoginManager.UseUnicode;

			//leggo parametro che dice se devo strippare gli spazi dei campi letti da Db
			this.StripTrailingSpaces = LoginManager.StripTrailingSpaces;
			//acquisisco il lock globale per evitare disinstallazioni mentre uso il programma
			lockToken = ApplicationSemaphore.Lock(BasePathFinder.BasePathFinderInstance.GetSemaphoreFilePath());

			return true;
		}

		//---------------------------------------------------------------------
		public void SetCulture()
		{
		
			// se non ho fatto ancora una login prendo le informazioni di lingua dal
			// ServerConnection.config settato dalla console
			if (LoginManager == null)
			{
				DictionaryFunctions.SetCultureInfo
					(
					 InstallationData.ServerConnectionInfo.PreferredLanguage,
					 InstallationData.ServerConnectionInfo.ApplicationLanguage
					);
				return;
			}

			//Setta le informazioni per gestire le lingue sulla base dell'Utente collegato
			// ma potrei aver fatto una Logoff
			if (LoginManager.LoginManagerState == LoginManagerState.Logged)
				DictionaryFunctions.SetCultureInfo
					(
					LoginManager.PreferredLanguage,
					LoginManager.ApplicationLanguage
					);
		}

		//---------------------------------------------------------------------
		public bool LogOff()
		{
			if (LoginManager == null) return false;
			LoginManager.LogOff();

			try
			{
				if (tbLoaderInterface != null)
					tbLoaderInterface.CloseLogin();
			}
			catch 
			{
			}

			if (lockToken != null)
			{
				lockToken.Dispose();
				lockToken = null;
			}

			return true;
		}
		
		//---------------------------------------------------------------------
		public static string[] EnumCompanies(string user)
		{
			try
			{

				LoginManager lm = new LoginManager();
				return lm.EnumCompanies(user);
			}
			catch 
			{
			}
			return null;
		}

		//---------------------------------------------------------------------
		public int[] EnumActiveDocuments()
		{
			try
			{
				//se non c'e' un tbloader istanziato, significa che non ci sono documenti aperti
				if (!TbServices.IsTbLoaderInstantiated(ApplicationKey))
					return  new int[0];

				ITbLoaderClient i = GetTbLoaderInterface();
				if (i == null)
					return new int[0];
				return i.GetDocumentThreads();
			}
			catch 
			{
				return new int[0];
			}
		}

		//---------------------------------------------------------------------
		public static bool IsIntegratedSecurityUser(string user)
		{
			try
			{
				LoginManager lm = new LoginManager();
				return lm.IsIntegrateSecurityUser(user);
				
			}
			catch(Exception e)
			{
				string a = e.Message;
			}
			return false;
		}



		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose ()
		{
			if (lockToken != null)
			{
				lockToken.Dispose();
				lockToken = null;
			}
		}

		#endregion

	}
}
