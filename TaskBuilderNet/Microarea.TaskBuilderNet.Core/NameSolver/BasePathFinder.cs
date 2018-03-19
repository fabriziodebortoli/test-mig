using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Threading;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Interfaces;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	public enum ImageSize { None, Size16x16, Size20x20, Size24x24, Size32x32 }
	/// <summary>
	/// Gestore dei path di tutta l'applicazione dipendenti da drive, installazione e/o server 
	/// </summary>
	//=========================================================================
	public class BasePathFinder : IBasePathFinder
	{
		#region costanti
		private const string logManagerFile = "XMLLogManager.xsl";
		private const string xmlExt = "Xml";
		private const string description = "Description";
		private const string views = "Views";
		private const string shared = "Shared";
		private const string schema = "Schema";
		private const string uiControllers = "UIControllers";
		private const string MicroareaRegKey = "Software\\Microarea\\";
		private const string numberToLiteralXmlFileName = "NumberToLiteral.xml";
        protected const string defaultThemeFileName = "DefaultTheme.config";

		public const int StandardAppSegmentPath = 4;
		public const int StandardModuleSegmentPath = 5;
		public const int CustomAppSegmentPath = 6;
		public const int CustomModuleSegmentPath = 7;

		public const char DomainUserSeparator = '.'; // deve essere simmetrico al carattere usato in C++

		private static readonly object staticLockTicket = new object();
		private static readonly object staticLockTicketFunctions = new object();
		private readonly object instanceLockTicket = new object();
		#endregion

		#region membri privati
		private static BasePathFinder basePathFinderInstance;

		private string installation = string.Empty;
		private string masterSolution = string.Empty;
		private string microareaServicesPath = string.Empty;
		private string standardPath = string.Empty;
		private string customPath = string.Empty;
		private string appsPath = string.Empty;
		private string publishPath = string.Empty;

		private SettingsConfigInfoTable settingsTable = new SettingsConfigInfoTable();

		//indica se chi sta utilizzando pathfinder si trova nello stesso server della
		private string remoteWebServer = string.Empty;
		private string remoteFileServer = string.Empty;
		private bool runAtServer = false;
		private static string tbApplicationPath = null;
		private static string magonetApplicationPath = null;
		private static string microareaConsoleApplicationPath = null;

		/// <summary>
		/// Indica se il programma sta girando all'interno del percorso di installazione (Apps o Standard ad es. per i web services)
		/// </summary>
		private bool isRunningInsideInstallation = false;
		#endregion

		#region membri protetti
		protected string build = string.Empty;
		protected Diagnostic diagnostic = new Diagnostic("NameSolver");
		protected ArrayList applications;

		protected CoreTypes.FunctionsList webMethods = null;
        EasyStudioConfiguration easyStudioConfiguration = null;
        public string EasyStudioHome { get => easyStudioConfiguration.Settings.HomeName; }

        public CoreTypes.FunctionsList WebMethods
		{
			get
			{
				lock (staticLockTicketFunctions)
				{
					LoadPrototypes();
					return webMethods;
				}
			}
		}


		//-----------------------------------------------------------------------------
		public void LoadPrototypes()
		{
			if (webMethods == null)
			{
				webMethods = new CoreTypes.FunctionsList();
			}
		} 
		
	
		public IFunctions WebFunctions
		{
			get
			{
				return WebMethods;
			}
		}

		#endregion

		#region proprietà

		//----------------------------------------------------------------------------
		/// <summary>
		/// Oggetto statico globale BasePathFinder utilizzato ovunque in Mago.Net siano necessarie
		/// informazioni non dipendenti da username e company
		/// </summary>
		public static BasePathFinder BasePathFinderInstance
		{
			get
			{
				lock (staticLockTicket)
				{
					if (basePathFinderInstance == null)
					{
						basePathFinderInstance = new BasePathFinder();
						basePathFinderInstance.Init();
						basePathFinderInstance.LoadPrototypes();

					}
					return basePathFinderInstance;
				}
			}
			set
			{
				lock (staticLockTicket)
				{
					basePathFinderInstance = value;
					basePathFinderInstance.LoadPrototypes();
				}
			}
		}

        //----------------------------------------------------------------------------
        /// <summary>
        /// Indica se il path finder sta girando sullo stesso server dell'installazione
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return basePathFinderInstance != null;
            }
        }

		//----------------------------------------------------------------------------
		/// <summary>
		/// Indica se il path finder sta girando sullo stesso server dell'installazione
		/// </summary>
		public bool RunAtServer { get { return runAtServer; } }
		//----------------------------------------------------------------------------
		/// <summary>
		/// Indica se il path finder sta girando ALL'INTERNO del percorso di installazione (es. TBServices, LoginManager)
		/// </summary>
		public bool IsRunningInsideInstallation { get { return isRunningInsideInstallation; } }

		
		/// <summary>
		/// Ritorna le informazioni di ogni applicazione trovata (virtual in quanto la classe PathFinder
		/// ne reimplementa una variante che include anche la configurazione)
		/// </summary>
		//----------------------------------------------------------------------------
		public virtual IList ApplicationInfos
		{
			get
			{
				lock (instanceLockTicket)
				{
					if (applications != null)
						return applications;

					applications = new ArrayList();

					if (!AddApplicationsByType(ApplicationType.All))
						return new ArrayList();

					return applications;
				}
			}
		}

		//----------------------------------------------------------------------------
		public void RefreshEasyBuilderApps(ApplicationType type)
		{
			//elimino tutte le applicazioni custom e le ricarico
			for (int i = applications.Count - 1; i >= 0; i--)
				if (((BaseApplicationInfo)applications[i]).ApplicationType == type)
					applications.RemoveAt(i);

			AddApplicationsByType(type);
		}

		//----------------------------------------------------------------------------
		public string MasterSolution
        {
            get
            {
                if (masterSolution == null)
                    masterSolution = InstallationData.ServerConnectionInfo.MasterSolutionName;
                return masterSolution;
            }
        }
		
		//----------------------------------------------------------------------------
		public string Installation { get { return installation; } }

		//----------------------------------------------------------------------------
		public int TbLoaderSOAPPort { get { return GetSettingValue("TBLoaderDefaultSOAPPort"); } }

		//----------------------------------------------------------------------------
		public int TbLoaderTCPPort { get { return GetSettingValue("TBLoaderDefaultTCPPort"); } }

		//----------------------------------------------------------------------------
		/// <summary>
		/// Nome del server che espone le cartelle virtuali di IIS
		/// </summary>
		public string RemoteWebServer { get { return remoteWebServer; } }
		//----------------------------------------------------------------------------
		/// <summary>
		/// Nome del server che espone le cartelle condivise di file system
		/// </summary>
		public string RemoteFileServer { get { return remoteFileServer; } }

		//----------------------------------------------------------------------------
		public Diagnostic Diagnostic { get { return diagnostic; } }

		//tipo di build debug o release, se non settata restituisce quella in run
		//----------------------------------------------------------------------------
		public string Build
		{
			get
			{
				//se non mi viene passata la build uso quella dell'assembly corrente.
				if (build == string.Empty)
				{
					DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
					if (string.Compare(di.Name, NameSolverStrings.Debug, true, CultureInfo.InvariantCulture) == 0)
						build = NameSolverStrings.Debug;
					else
						if (string.Compare(di.Name, NameSolverStrings.Release, true, CultureInfo.InvariantCulture) == 0)
							build = NameSolverStrings.Release;
						else
							build = Generic.Functions.DebugOrRelease();
				}
				return build;
			}
			set
			{
				build = value;
			}
		}

		/// <summary>
		/// Ritorna il path nel formato http://localhost:80/Installation
		/// </summary>
		//----------------------------------------------------------------------------
		public string WebFrameworkRootUrl
		{
			get
			{
				if (installation == string.Empty)
					return string.Empty;

				string webSrv = RemoteWebServer;
				if (string.Compare(RemoteWebServer, Dns.GetHostName(), true, CultureInfo.InvariantCulture) == 0)
					webSrv = "LocalHost";

				return string.Format("http://{0}:{1}/{2}", webSrv, InstallationData.ServerConnectionInfo.WebServicesPort, installation);
			}
		}

		/// <summary>
		/// Ritorna il path nel formato http://localhost:80/Installation/WebService/Webservice.asmx
		/// </summary>
		//----------------------------------------------------------------------------
		public string WebFrameworkMethodsUrl(string name, string webMethods)
		{
			if (installation == string.Empty)
				return string.Empty;

			return string.Format("{0}/{1}/{2}.asmx", WebFrameworkRootUrl, name, webMethods);
		}

		/// <summary>
		/// Ritorna il path nel formato http://localhost:80/Installation/WebService/Webservice.svc
		/// </summary>
		//----------------------------------------------------------------------------
		public string WcfWebFrameworkMethodsUrl(string name, string webMethods)
		{
			if (installation == string.Empty)
				return string.Empty;

			return string.Format("{0}/{1}/{2}.svc", WebFrameworkRootUrl, name, webMethods);
		}

		/// <summary>
		/// Ritorna il path del LoginManager
		/// </summary>
		//----------------------------------------------------------------------------
		public string LoginManagerPath
		{
			get
			{
				IBaseModuleInfo mi = GetModuleInfoByName(NameSolverStrings.WebFramework, NameSolverStrings.LoginManager);
				if (mi == null || mi.Path == null || mi.Path == string.Empty)
					return string.Empty;

				return mi.Path;
			}
		}


		//----------------------------------------------------------------------------
		public string PingViaSMSPage
		{
			get
			{
				return string.Format("{0}PingViaSMS.aspx", LoginManagerBaseUrl);
			}
		}
		//----------------------------------------------------------------------------
		public string LoginManagerBaseUrl
		{
			get
			{
				return string.Format("{0}/{1}/", WebFrameworkRootUrl, NameSolverStrings.LoginManager);
			}
		}
		/// <summary>
		/// Ritorna il path del LoginManager nel formato http://localhost:80/Installation/LoginManager
		/// </summary>
		//----------------------------------------------------------------------------
		public string LoginManagerUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.LoginManager, NameSolverStrings.LoginManager);
			}
		}

		/// <summary>
		/// Ritorna il path del RestGate
		/// 
		/// </summary>
		//----------------------------------------------------------------------------
		public string RESTGateUrl
		{
			get
			{
				return string.Concat(WebFrameworkRootUrl, '/', NameSolverStrings.RESTGate, '/');
			}
		}

		/// <summary>
		/// Ritorna il path del LockManager nel formato http://localhost:80/Installation/LockManager
		/// </summary>
		//----------------------------------------------------------------------------
		public string LockManagerUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.LockManager, NameSolverStrings.LockManager);
			}
		}

		/// <summary>
		/// Ritorna il path del TbSender nel formato http://localhost:80/Installation/TbSender
		/// </summary>
		//----------------------------------------------------------------------------
		public string TbSenderUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.TbSender, NameSolverStrings.PLProxy);
			}
		}

		/// <summary>
		/// Ritorna il path del TbServices nel formato http://localhost:80/Installation/TbServices
		/// </summary>
		//----------------------------------------------------------------------------
		public string TbServicesUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.TbServices, NameSolverStrings.TbServices);
			}
		}

		/// <summary>
		/// Ritorna il path del TbLoaderLauncher nel formato http://localhost:80/Installation/TbLoaderLauncher
		/// </summary>
		//----------------------------------------------------------------------------
		public string TbLoaderLauncherUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.TbLoaderLauncher, NameSolverStrings.TbLoaderLauncher);
			}
		}

		//----------------------------------------------------------------------------
		public string EasyLookServiceBaseUrl
		{
			get
			{
				return string.Format("{0}/{1}/", WebFrameworkRootUrl, NameSolverStrings.EasyLook);
			}
		}

		/// <summary>
		/// Ritorna il path di EasyLookService nel formato http://localhost:80/Installation/EasyLookService
		/// </summary>
		//----------------------------------------------------------------------------
		public string EasyLookServiceUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.EasyLook, NameSolverStrings.EasyLookService);
			}
		}

		/// <summary>
		/// Ritorna il path del EasyAttachmentSync nel formato http://localhost:80/Installation/EasyAttachmentSync
		/// </summary>
		//----------------------------------------------------------------------------
		public string EasyAttachmentSyncUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.EasyAttachmentSync, NameSolverStrings.EasyAttachmentSync);
			}
		}

		/// <summary>
		/// Ritorna il path del DataSynchronizer nel formato http://localhost:80/Installation/DataSynchronizer
		/// </summary>
		//----------------------------------------------------------------------------
		public string DataSynchronizerUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.DataSynchronizer, NameSolverStrings.DataSynchronizer);
			}
		}

		/// <summary>
		/// Ritorna il path del SOSProxy nel formato http://localhost:80/Installation/TBSender/SOSProxy.asmx
		/// </summary>
		//----------------------------------------------------------------------------
		public string SOSProxyUrl
		{
			get
			{
				return WebFrameworkMethodsUrl(NameSolverStrings.TbSender, NameSolverStrings.SOSProxy);
			}
		}

		/// <summary>
		/// Ritorna il path del NotificationService nel formato http://localhost/Installation/NotificationService/NotificationService.svc
		/// </summary>
		//----------------------------------------------------------------------------
		public string NotificationServiceUrl
		{
			get
			{
				return WcfWebFrameworkMethodsUrl(NameSolverStrings.NotificationService, NameSolverStrings.NotificationService);
			}
		}
		#endregion

		#region costruttori e init

		/// <summary>
		/// Costruttore protetto concepito per evitare che vengano istanziate nuovi oggetti di tipo 
		/// BasePathFinder (usare l'oggetto statico BasePathFinderInstance)
		/// </summary>
		//---------------------------------------------------------------------
		protected BasePathFinder()
		{
			//if (!Init())
			//    throw new Exception(Messages.PathFinderInitFailed);
		}

		/// <summary>
		/// Annulla l'oggetto basePathFinderInstance, in modo che la prima volta che viene riutilizzato
		/// viene automaticamente reinizializzato (usato ad esempio al cambio della stringa di connessione)
		/// </summary>
		//---------------------------------------------------------------------
		public static void ClearBasePathFinderInstance()
		{
			basePathFinderInstance = null;
		}

		//---------------------------------------------------------------------
		protected virtual bool Init()
		{
			return Init(string.Empty, string.Empty);
		}

		/// <summary>
		/// Questa init è da usare in un applicazione che gira in un server !=
		/// dal server di installazione che non risiede nella microarea client
		/// es un woorm su un server che chiede servizi ad un login manager su un
		/// altro server
		/// </summary>
		/// <param name="aRemoteServer"></param>
		/// <param name="aInstallation"></param>
		//---------------------------------------------------------------------
		protected virtual bool Init(string aRemoteServer, string aInstallation)
		{
			applications = null;
			installation = aInstallation;
			remoteWebServer = aRemoteServer;
			remoteFileServer = aRemoteServer;

			//il client connection mi da informazioni su dove si trova il server; il file e' nella cartella di applicazione
			//se non ho il server, lo pesco dall'url di update
			if (string.IsNullOrEmpty(remoteFileServer))
				remoteFileServer = InstallationData.FileSystemServerName;

			//se non ho il server, lo pesco dall'url di update
			if (string.IsNullOrEmpty(remoteWebServer))
				remoteWebServer = InstallationData.WebServerName;

			//se non ho l'installazione, lo pesco dall'url di update
			if (string.IsNullOrEmpty(installation))
				installation = InstallationData.InstallationName;

			if (string.IsNullOrEmpty(installation))
				throw new Exception(Messages.EmptyInstallation);

			if (string.IsNullOrEmpty(remoteFileServer) || string.IsNullOrEmpty(remoteWebServer))
				throw new Exception(Messages.EmptyServer);

			//se il server e il client coincidono, sono in modalita' run at server (sto girando sulla stessa macchina dove e' installato il server)
			runAtServer = Dns.GetHostEntry(remoteFileServer).HostName == Dns.GetHostEntry(Dns.GetHostName()).HostName;

			if (!InstallationData.IsClickOnceInstallation) //sto runnando dalla standard (Web application) oppure Apps, non uso share di rete
			{
				if (CalculatePathsInsideInstallation())
					isRunningInsideInstallation = true;
				else
					CalculatePathsOutsideInstallation();
			}
			else //sto runnando da una posizione diversa dalla standard, l'unico modo per trovare la standard e custom
			//e' passare per gli share di rete
			{
				CalculatePathsOutsideInstallation();
			}

            easyStudioConfiguration = new EasyStudioConfiguration(this);
			

			return true;
		}

        //---------------------------------------------------------------------
        protected virtual bool CalculatePathsInsideInstallation()
		{
			string basePath = AppDomain.CurrentDomain.BaseDirectory;
			if (CalculatePathsInsideInstallation(basePath))
				return true;

			basePath = Assembly.GetExecutingAssembly().Location;
			return CalculatePathsInsideInstallation(basePath);
		}

		/// <summary>
		/// Inizializza i path standardPath, customPath e appsPath se determina che il basepath 
		/// è all'interno dell'installazione corrente
		/// </summary>
		/// <param name="basePath"></param>
		//---------------------------------------------------------------------
		protected bool CalculatePathsInsideInstallation(string basePath)
		{
			int idx = basePath.LastIndexOf("\\" + installation + "\\", StringComparison.InvariantCultureIgnoreCase);
			if (idx == -1)
				return false;
			string installationPath = basePath.Substring(0, idx + 1 + installation.Length);
			standardPath = Path.Combine(installationPath, NameSolverStrings.Standard);
			customPath = Path.Combine(installationPath, NameSolverStrings.Custom);
			appsPath = Path.Combine(installationPath, NameSolverStrings.Apps);
			publishPath = Path.Combine(appsPath, NameSolverStrings.Publish);

			return Directory.Exists(standardPath);
		}

		//---------------------------------------------------------------------
		public string CalculateRemoteStandardPath()
		{
			return string.Format(@"\\{0}\{1}_{2}", RemoteFileServer, installation, NameSolverStrings.Standard);
		}
		//---------------------------------------------------------------------
		public string CalculateRemoteCustomPath()
		{
			return string.Format(@"\\{0}\{1}_{2}", RemoteFileServer, installation, NameSolverStrings.Custom);
		}
		//---------------------------------------------------------------------
		protected virtual void CalculatePathsOutsideInstallation()
		{
			standardPath = CalculateRemoteStandardPath();
			customPath = CalculateRemoteCustomPath();
			appsPath = ""; //non ho share di rete per accedere alla apps
			publishPath = "";//non ho share di rete per accedere alla publish

			//se sono sulla stessa macchina dell'installazione, provo a convertire gli share di rete 
			//in path fisici per migliorare le prestazioni (potrei non avere l'accesso a questa informazione
			//se sono un utente 'debole', in tal caso devo accontentarmi degli share
			if (runAtServer)
			{
				standardPath = TryConvertToPhysicalPath(standardPath);
				customPath = TryConvertToPhysicalPath(customPath);
			}

			if (!Directory.Exists(standardPath))
				throw new ApplicationException(string.Format(Messages.InvalidInstallation, installation, standardPath));
		}

		

		/// <summary>
		/// Converte uno share di rete in un path fisico 
		/// </summary>
		//---------------------------------------------------------------------
		private string TryConvertToPhysicalPath(string sharedPath)
		{
			string shared = Path.GetFileName(sharedPath);
			using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(string.Format("Select * from Win32_Share where Name = '{0}'", shared)))
			{
				using (ManagementObjectCollection moc = searcher.Get())
				{
					if (moc.Count != 0)
					{
						foreach (ManagementObject mo in moc)
						{
							string path = (string)mo["Path"];
							if (path != null)
								return path;
						}
					}
				}
			}
			return sharedPath;
		}

		/// <summary>
		/// Aggiunge all'array delle applicazioni di path finder quelle del un tipo 
		/// specificate cercando nel file system
		/// </summary>
		/// <param name="applicationType">il tipo delle applicazioni da aggiungere all indica tutte</param>
		/// <param name="aEnumImages">il tipo di immagine da scandagliare</param>
		/// <returns>true se non ci sono stati errori</returns>
		//---------------------------------------------------------------------
		protected bool AddApplicationsByType(ApplicationType applicationType)
		{
			if (applicationType == ApplicationType.All)
			{
				return
					AddApplicationsByType(ApplicationType.TaskBuilder | ApplicationType.TaskBuilderNet) &&
					AddApplicationsByType(ApplicationType.TaskBuilderApplication | ApplicationType.Standardization) &&
					AddApplicationsByType(ApplicationType.Customization);
			}

			ArrayList tempApplications = null;
			string apps = GetStandardApplicationContainerPath(applicationType);
			if (apps == string.Empty)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, applicationType.ToString()));
				return false;
			}

			if (!Directory.Exists(apps))
			{
				//tapullo per far funzionare il migratore report che legge la vecchia struttura
				if (BaseApplicationInfo.MatchType(applicationType, ApplicationType.TaskBuilderApplication))
					apps = apps.Replace(NameSolverStrings.TaskBuilderApplications, "TaskBuilderApplications");

				if (!Directory.Exists(apps))
					return true;
			}

			//prendo tutte le applicazioni di tb tb.net tbapps tools apps.net
			Generic.Functions.ReadSubDirectoryList(apps, out tempApplications);

			// controlla le dichiarazioni di directory ed elimina quelle 
			// che non corrispondono a delle vere applicazioni da caricare.
			foreach (string applicationName in tempApplications)
			{
				CreateApplicationInfo(applicationName, applicationType, apps);
			}

			return true;
		}

		#endregion

		#region statiche

		/// <summary>
		/// Ritorna il "nome" dell'applicazion container dato l'ApplicationType 
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetApplicationContainerName(ApplicationType applicationType)
		{
			if (BaseApplicationInfo.MatchType(applicationType, ApplicationType.TaskBuilder))
				return NameSolverStrings.TaskBuilder;
			if (BaseApplicationInfo.MatchType(applicationType, ApplicationType.TaskBuilderNet))
				return NameSolverStrings.TaskBuilder;
			if (BaseApplicationInfo.MatchType(applicationType, ApplicationType.TaskBuilderApplication))
				return NameSolverStrings.TaskBuilderApplications;
			if (BaseApplicationInfo.MatchType(applicationType, ApplicationType.Customization))
				return NameSolverStrings.TaskBuilderApplications;
			if (BaseApplicationInfo.MatchType(applicationType, ApplicationType.Standardization))
				return NameSolverStrings.TaskBuilderApplications;

			Debug.Fail("Tipo applicazione non gestito");

			return string.Empty;
		}

		/// <summary>
		/// True se la cartella contiene un file "Application.config"
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IsApplicationDirectory(string appPath)
		{
			return (appPath.Length != 0) &&
                File.Exists
				(
				appPath +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Application +
                NameSolverStrings.ConfigExtension
				);
		}

		/// <summary>
		/// True se la cartella contiene un file "Module.config"
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IsModuleDirectory(string modulePath)
		{
			return (modulePath.Length != 0) &&
                File.Exists
				(
				modulePath +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Module +
                NameSolverStrings.ConfigExtension
				);
		}

		#endregion

		#region funzioni pubbliche
		/// <summary>
		/// Restituisce l'elenco dei nomi delle applicazioni in base al tipo di applicazione specificata
		/// </summary>
		/// <param name="applicationType">tipo dell'applicazione</param>
		/// <param name="applicationList">lista delle applicazioni</param>
		/// <returns>il successo della funzione</returns>
		//---------------------------------------------------------------------
		public bool GetApplicationsList(ApplicationType applicationType, out StringCollection applicationList)
		{
			applicationList = new StringCollection();
			foreach (BaseApplicationInfo aApplicationInfo in ApplicationInfos)
			{
				if (BaseApplicationInfo.MatchType(aApplicationInfo.ApplicationType, applicationType) && 
					Directory.Exists(aApplicationInfo.Path))
						applicationList.Add(aApplicationInfo.Name);
			}

			return true;
		}

		/// <summary>
		/// Restituisce un'application info in base al nome dell'applicazione
		/// </summary>
		/// <param name="applicationName">nome dell'applicazione</param>
		//---------------------------------------------------------------------
		public virtual IBaseApplicationInfo GetApplicationInfoByName(string applicationName)
		{
			foreach (IBaseApplicationInfo aApplicationInfo in ApplicationInfos)
			{
				if (string.Compare(aApplicationInfo.Name, applicationName, true, CultureInfo.InvariantCulture) == 0)
					return Directory.Exists(aApplicationInfo.Path) ? aApplicationInfo : null;
			}

			return null;
		}

		//---------------------------------------------------------------------
		public void DeleteApplicationByName(string applicationName)
		{

			for (int i = ApplicationInfos.Count - 1; i >= 0; i--)
			{
				IBaseApplicationInfo aApplicationInfo = ApplicationInfos[i] as IBaseApplicationInfo;
				if (string.Compare(aApplicationInfo.Name, applicationName, true, CultureInfo.InvariantCulture) != 0)
					continue;

				ApplicationInfos.RemoveAt(i);
				return;
			}
		}

		/// <summary>
		/// Restituisce la path di un'applicazione
		/// </summary>
		/// <param name="applicationName">il nome dell'applicazione</param>
		//---------------------------------------------------------------------
		public string GetStandardApplicationPath(string applicationName)
		{
			IBaseApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);

			if (aApplicationInfo == null)
				return string.Empty;

			return aApplicationInfo.Path;
		}

		/// <summary>
		/// Ritorna l'oggetto SettingItem relativo ad applicazione e modulo desiderato
		/// filtrato per sectionName e settingName
		/// </summary>
		/// <param name="applicationName">Applicazione in cui cercare il setting</param>
		/// <param name="moduleName">Modulo in cui cercare il setting</param>
		/// <param name="sectionName">Sezione desiderata</param>
		/// <param name="settingName">Setting desiderato</param>
		//---------------------------------------------------------------------
		public SettingItem GetSettingItem(string applicationName, string moduleName, string sectionName, string settingName)
		{
			BaseModuleInfo mi = GetModuleInfoByName(applicationName, moduleName) as BaseModuleInfo;
			if (mi == null) return null;
			return settingsTable.GetSettingItem(sectionName, settingName, mi);
		}

		/// <summary>
		/// Restituisce un BaseModuleInfo in base all'applicazione e modulo
		/// </summary>
		/// <param name="moduleName">nome del modulo</param>
		/// <returns>il module info o null se non trovato</returns>
		//---------------------------------------------------------------------
		public virtual IBaseModuleInfo GetModuleInfoByName(string applicationName, string moduleName)
		{
			if (applicationName == null || moduleName == null ||
                applicationName == string.Empty || moduleName == string.Empty)
				return null;

			BaseApplicationInfo aApplicationInfo = null;
			try
			{
				aApplicationInfo = (BaseApplicationInfo)GetApplicationInfoByName(applicationName);
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
			}
			if (aApplicationInfo == null)
				return null;

			return aApplicationInfo.GetModuleInfoByName(moduleName);
		}

		/// <summary>
		/// Restituisce un BaseModuleInfo in base all'applicazione e modulo
		/// </summary>
		/// <param name="moduleName">nome del modulo</param>
		/// <returns>il module info o null se non trovato</returns>
		//---------------------------------------------------------------------
		public void RemoveModuleInfoByName(string applicationName, string moduleName)
		{
			if (applicationName == null || moduleName == null ||
				applicationName == string.Empty || moduleName == string.Empty)
				return;

			BaseApplicationInfo aApplicationInfo = null;
			try
			{
				aApplicationInfo = (BaseApplicationInfo)GetApplicationInfoByName(applicationName);
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
			}
			if (aApplicationInfo == null)
				return;

			aApplicationInfo.RemoveModuleInfo(moduleName);
		}

		/// <summary>
		/// Restituisce la path di un modulo in base all'applicazione ed il modulo
		/// </summary>
		//---------------------------------------------------------------------
		public string GetApplicationModulePath(string applicationName, string moduleName)
		{
			if (
				applicationName == null ||
                moduleName == null ||
                applicationName == string.Empty ||
                moduleName == string.Empty
				)
				return null;

			IBaseApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);
			if (aApplicationInfo == null)
				return string.Empty;

			IBaseModuleInfo aModuleInfo = aApplicationInfo.GetModuleInfoByName(moduleName);
			if (aModuleInfo == null)
				return string.Empty;

			return Path.Combine(aApplicationInfo.Path, aModuleInfo.Name);
		}

        /// <summary>
        /// Restituisce la path di un modulo in base all'applicazione ed il modulo
        /// </summary>
        //---------------------------------------------------------------------
        public string GetStandardModulePath(string applicationName, string moduleName)
        {
            if (
                applicationName == null ||
                moduleName == null ||
                applicationName == string.Empty ||
                moduleName == string.Empty
                )
                return null;

            IBaseApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);
            if (aApplicationInfo == null)
                return string.Empty;

            IBaseModuleInfo aModuleInfo = aApplicationInfo.GetModuleInfoByName(moduleName);
            if (aModuleInfo == null)
                return string.Empty;
            
            return Path.Combine(aApplicationInfo.Path, aModuleInfo.Name);
        }

        /// <summary>
        /// Restituisce l'array dei nomi dei moduli in base al nome applicazione
        /// </summary>
        /// <param name="applicationName">nome applicazione</param>
        /// <param name="moduleList">ritorna la lista dei moduli</param>
        /// <returns>il successo della funzione</returns>
        //---------------------------------------------------------------------
        public ICollection GetModulesList(string applicationName)
		{
			IBaseApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);

			if (aApplicationInfo == null)
				return null;

			return aApplicationInfo.Modules;
		}

		/// <summary>
		/// Analizza la path passata per vedere se è standard
		/// </summary>
		/// <param name="aPath"></param>
		/// <returns>true se è standard</returns>
		//-----------------------------------------------------------------------------
		public bool IsStandardPath(string aPath)
		{
			string path = GetStandardPath().ToLower(CultureInfo.InvariantCulture);
			aPath = aPath.Replace('/', Path.PathSeparator);
			aPath = aPath.ToLower(CultureInfo.InvariantCulture);

			return aPath.StartsWith(path);
		}

		/// <summary>
		/// Path dei servizi
		/// </summary>
		/// <returns>La parh dei servizi</returns>
		//-----------------------------------------------------------------------------
		public string GetServicePath()
		{
			return microareaServicesPath;
		}

		/// <summary>
		/// GetStandardPath della running
		/// </summary>
		/// <returns>Standard installation path</returns>
		//-----------------------------------------------------------------------------
		public virtual string GetStandardPath()
		{
			return standardPath;
		}

		/// <summary>
		/// Ritorna il path della cartella Apps oppure stringa vuota se non sto girando lato server
		/// </summary>
		//-----------------------------------------------------------------------------
		public virtual string GetAppsPath()
		{
			return appsPath;
		}

		/// <summary>
		/// Ritorna il path della cartella Apps oppure stringa vuota se non sto girando lato server
		/// </summary>
		//-----------------------------------------------------------------------------
		public virtual string GetPublishPath()
		{
			return publishPath;
		}

		/// <summary>
		/// Ritorna il path di installazione
		/// </summary>
		//-----------------------------------------------------------------------------
		public virtual string GetInstallationPath()
		{
			return Path.GetDirectoryName(standardPath);
		}

		/// <summary>
		/// Ritorna il path del file utilizzato come semaforo per evitare che venga utilizzato mago
		/// durante una disinstallazione o che venga fatto partire un processo di aggiornamento mentre
		/// ci sono utenti attivi su Mago
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetSemaphoreFilePath()
		{
			return GetStandardPath() +
                Path.DirectorySeparatorChar +
                NameSolverStrings.TaskBuilder +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Framework +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Application +
                NameSolverStrings.ConfigExtension;
		}

		/// <summary>
		/// Ritorna il path (standard) dato l'applicationContainer
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetStandardApplicationContainerPath(ApplicationType aApplicationType)
		{
			if (aApplicationType == ApplicationType.All || aApplicationType == ApplicationType.Undefined)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
				return string.Empty;
			}

			string appContainerName = GetApplicationContainerName(aApplicationType);
			if (appContainerName == string.Empty)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
				return string.Empty;
			}

			string folder = (aApplicationType == ApplicationType.Customization)
				? GetEasyStudioCustomizationsPath()
                : standardPath;
			return Path.Combine(folder, appContainerName);
		}

		/// <summary>
		/// Ritorna il fullpath delle immagini relative alla splash (cerca nella cartella solutions per gli 
		/// Embedded)
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		public string FindSplashFile(string fileName)
		{
			if (String.IsNullOrWhiteSpace(fileName)) return string.Empty;
			string filePath = Path.Combine(GetStandardPath(), fileName);
			if (File.Exists(filePath))
				return filePath;

			return FindFileInSolutionFolder(fileName);
		}

        /// <summary>
        /// Ritorna un dictionary (nome applicazione - path file SynchroProfiles.xml)
        /// con tutte le applicazioni che hanno quel file nel folder relativo al provider indicato
        /// </summary>
        /// <param name="providername">nome del provider</param>
        //-----------------------------------------------------------------------------
        public Dictionary<string, string> GetSynchroProfilesFilePath(string providername)
        {
            Dictionary<string, string> synchroFilesDict = GetSynchroFilesFolderPath(providername);

            Dictionary<string, string> synchroProfilesDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in synchroFilesDict)
            {
                string synchroFilesFolder = Path.Combine(kvp.Value, NameSolverStrings.SynchroProfilesXmlFile);
                if (File.Exists(synchroFilesFolder))
                    synchroProfilesDict.Add(kvp.Key, synchroFilesFolder);
            }

            return synchroProfilesDict;
        }

        /// <summary>
        /// Ritorna un dictionary (nome applicazione - path file SynchroMassiveProfiles.xml)
        /// con tutte le applicazioni che hanno quel file nel folder relativo al provider indicato
        /// </summary>
        /// <param name="providername">nome del provider</param>
        //-----------------------------------------------------------------------------
        public Dictionary<string, string> GetSynchroMassiveProfilesFilePath(string providername)
        {
            Dictionary<string, string> synchroFilesDict = GetSynchroFilesFolderPath(providername);

            Dictionary<string, string> synchroProfilesDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in synchroFilesDict)
            {
                string synchroFilesFolder = Path.Combine(kvp.Value, NameSolverStrings.SynchroMassiveProfilesXmlFile);
                if (File.Exists(synchroFilesFolder))
                    synchroProfilesDict.Add(kvp.Key, synchroFilesFolder);
            }

            return synchroProfilesDict;
        }

        /// <summary>
        /// Ritorna un dictionary (nome applicazione -folder Actions)
        /// con tutte le applicazioni che hanno quel folder nel folder relativo al provider indicato
        /// </summary>
        /// <param name="providername">nome del provider</param>
        //-----------------------------------------------------------------------------
        public Dictionary<string, string> GetSynchroFilesActionFolderPath(string providername)
        {
            Dictionary<string, string> synchroFilesDict = GetSynchroFilesFolderPath(providername);

            Dictionary<string, string> synchroFilesActionDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in synchroFilesDict)
            {
                string synchroActionFolder = Path.Combine(kvp.Value, NameSolverStrings.SynchroProfilesActionsXmlFolder);
                if (Directory.Exists(synchroActionFolder))
                    synchroFilesActionDict.Add(kvp.Key, synchroActionFolder);
            }

            return synchroFilesActionDict;
        }

        /// <summary>
        /// Ritorna un dictionary (nome applicazione - path folder SynchroConnector)
        /// con tutti i path del folder SynchroFiles di tutte le applicazioni che hanno 
        /// dichiarato il modulo SynchroConnector relativo al provider indicato
        /// </summary>
        /// <param name="providername">nome del provider</param>
        //-----------------------------------------------------------------------------
        public Dictionary<string, string> GetSynchroFilesFolderPath(string providername)
        {
            Dictionary<string, string> synchroFilesDictionary = new Dictionary<string, string>();

            if (String.IsNullOrWhiteSpace(providername))
                return synchroFilesDictionary;

            // carico in una lista di appoggio tutte le applicazione dichiarati nell'installazione
            // sia nella Standard che nella Custom (ad es. EasyStudio)
            StringCollection supportList = new StringCollection();
            StringCollection applicationsList = new StringCollection();

            // carico le applications dentro la Standard
            GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);
            for (int i = 0; i < supportList.Count; i++)
                applicationsList.Add(supportList[i]);

            // infine guardo le customizzazioni realizzate con EasyStudio
            GetApplicationsList(ApplicationType.Customization, out supportList);
            for (int i = 0; i < supportList.Count; i++)
                applicationsList.Add(supportList[i]);

            // calcolo il path completo di tutte le applicazione
            List<string> applicationsPathList = new List<string>();

            // PRIMA DI TUTTO aggiungo ERP
            if (applicationsList.ContainsNoCase("ERP"))
                applicationsPathList.Add(GetStandardApplicationPath("ERP"));

            foreach (string applicationName in applicationsList)
            {
                if (string.Compare(applicationName, "ERP", StringComparison.InvariantCultureIgnoreCase) != 0)
                    applicationsPathList.Add(GetStandardApplicationPath(applicationName));
            }

            // per tutte le applicazioni memorizzo solo quelle che hanno il modulo SynchroConnector
            // e il provider passato come parametro
            foreach (string appPath in applicationsPathList)
            {
                if (!IsApplicationDirectory(appPath))
                    continue;

                // esempio di path: ERP\SynchroConnector\SynchroProviders\<nome provider>
                string fullFolderPath = Path.Combine(appPath, NameSolverStrings.SynchroConnectorModule, NameSolverStrings.SynchroProvidersXmlFolder, providername);

                if (Directory.Exists(fullFolderPath))
                {
                    DirectoryInfo di = new DirectoryInfo(appPath);
                    string path;
                    if (!synchroFilesDictionary.TryGetValue(di.Name, out path))
                        synchroFilesDictionary.Add(di.Name, fullFolderPath);
                }
            }

            return synchroFilesDictionary;
        }

        /// <summary>
        /// Ritorna un array di fileInfo per ogni file di brand trovato
        /// </summary>
        //-----------------------------------------------------------------------------
        public FileInfo[] GetBrandFiles()
		{
			Hashtable ht = CollectionsUtil.CreateCaseInsensitiveHashtable(); // Dictionary<string, FileInfo>

			string stdPath = GetStandardPath();

			string appsPaths = Path.Combine(stdPath, NameSolverStrings.TaskBuilderApplications);
			if (Directory.Exists(appsPaths))
				foreach (string appPath in Directory.GetDirectories(appsPaths))
				{
					if (!IsApplicationDirectory(appPath))
						continue;
					string solPath = Path.Combine(appPath, NameSolverStrings.Solutions);
					if (!Directory.Exists(solPath))
						continue;
					AddDirectoryBrandFiles(solPath, ht);
				}
			else
				Debug.Fail(NameSolverStrings.TaskBuilderApplications + " folder is missing.");

			ArrayList l = new ArrayList(ht.Keys.Count);
			l.AddRange(ht.Values);
			return (FileInfo[])l.ToArray(typeof(FileInfo));
		}

		/// <summary>
		/// Aggiunge tutti i file di brand contenuti nella directory desiderata alla lista
		/// in memoria dei file di brand 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void AddDirectoryBrandFiles(string directory, Hashtable table)
		{
			string pattern = "*" + NameSolverStrings.BrandExtension;
			FileInfo[] bFiles = new DirectoryInfo(directory).GetFiles(pattern);
			foreach (FileInfo bFile in bFiles)
				if (!table.Contains(bFile.Name))
					table[bFile.Name] = bFile;
		}

		/// <summary>
		/// Cerca il file desiderato in tutte le cartelle [NomeApplicazione]\Solutions
		/// </summary>
		//---------------------------------------------------------------------
		private string FindFileInSolutionFolder(string fileName)
		{
			return FindFileInSolutionFolder(fileName, true);
		}

		/// <summary>
		/// Ritorna il full path della cartella Licences
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetLicensesPath()
		{
			return
				GetStandardPath() + Path.DirectorySeparatorChar +
                NameSolverStrings.Licenses;
		}

		#region funzioni per attivazione e licenze

		/// <summary>
		/// Dato il nome di un prodotto, restituisce il percorso completo del
		/// file di Solution, ad esempio
		/// \Standard\Solutions\MagoNet-Pro.Solution.xml.
		/// </summary>
		/// <param name="productName">Il nome del prodotto.</param>
		/// <returns>Il percorso completo del file di Solution.</returns>
		//-----------------------------------------------------------------------------
		public string GetSolutionFile(string productName)
		{
			string solutionFileName = GetSolutionFileName(productName);
			return FindFileInSolutionFolder(solutionFileName);
		}

		//----------------------------------------------------------------------------------------------
		public String GetMasterApplicationSolutionsThemeFolder()
		{
			StringCollection collection = new StringCollection();
			GetApplicationsList(ApplicationType.TaskBuilderApplication, out collection);

			String strMainBrandFolder;
			String strThemeFolder;
			String strMainBrandFile;
			foreach (string applicationName in collection)
			{	
				strMainBrandFolder = Path.Combine(GetStandardApplicationPath(applicationName), NameSolverStrings.Solutions);
				strThemeFolder = Path.Combine(GetStandardApplicationPath(applicationName), NameSolverStrings.Themes);
				strMainBrandFile = Path.Combine(strMainBrandFolder, MasterSolution + NameSolverStrings.BrandExtension);
				if (File.Exists(strMainBrandFile) && Directory.Exists(strThemeFolder))
				{
					return strThemeFolder;
				}
			}
			return string.Empty;
		}

		//----------------------------------------------------------------------------------------------
		public List<string> GetAvailableThemesFullNames()
		{
			List<string> allThemes = new List<string>();
			string strThemesPath = GetMasterApplicationSolutionsThemeFolder();
			if (!string.IsNullOrEmpty(strThemesPath) && Directory.Exists(strThemesPath))
			{
				DirectoryInfo di = new DirectoryInfo(strThemesPath);

				FileInfo[] masterThemesInfo = di.GetFiles("*" + NameSolverStrings.ThemeExtension);
				foreach (FileInfo item in masterThemesInfo)
				{
					allThemes.Add(item.FullName);
				}
			}

			IBaseApplicationInfo appInfo = GetApplicationInfoByName(NameSolverStrings.Framework);
			string defaultTheme = Path.Combine(appInfo.Path, NameSolverStrings.Themes);
			if (!string.IsNullOrEmpty(defaultTheme) && Directory.Exists(defaultTheme))
			{
				DirectoryInfo di = new DirectoryInfo(defaultTheme);
				FileInfo[] masterThemesInfo = di.GetFiles("*");
				foreach (FileInfo item in masterThemesInfo)
				{
					allThemes.Add(item.FullName);
				}
			}
			return allThemes;
		}


		/// <summary>
		/// Cerca il file specificato dal filename in tutte le cartelle \[NomeApplicazione]\Solutions, poi  ne prendo l'application.config e ne leggo la version
		/// </summary>
		//---------------------------------------------------------------------
		public string GetAppInfoVersionFromSolutionName(string name)
		{
			string appsPaths = Path.Combine(standardPath, NameSolverStrings.TaskBuilderApplications);
			if (!Directory.Exists(appsPaths))
				return string.Empty;
			foreach (string appPath in Directory.GetDirectories(appsPaths))
			{
				string solPath = Path.Combine(appPath, NameSolverStrings.Solutions);
				if (!Directory.Exists(solPath))
					continue;
				string filename = String.Format("{0}{1}", name, NameSolverStrings.SolutionExtension);
				if (!File.Exists(Path.Combine(solPath, filename)))
					continue;
				IBaseApplicationInfo app = GetApplicationInfoByName(Path.GetFileName(Path.GetDirectoryName(solPath)));
				if (app != null && app.ApplicationConfigInfo != null)
					return app.ApplicationConfigInfo.Version;
			}
			return string.Empty;
		}

		/// <summary>
		/// Cerca il file specificato dal filename in tutte le cartelle \[NomeApplicazione]\Solutions
		/// </summary>
		//---------------------------------------------------------------------
		private string FindFileInSolutionFolder(string fileName, bool checkApplication)
		{
			string appsPaths = Path.Combine(standardPath, NameSolverStrings.TaskBuilderApplications);
			if (!Directory.Exists(appsPaths))
				return string.Empty;
			foreach (string appPath in Directory.GetDirectories(appsPaths))
			{
                if (checkApplication && !IsApplicationDirectory(appPath))
                {//non tradurre
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Missing application.config in: " + appPath);
                    continue;
                }
				string solPath = Path.Combine(appPath, NameSolverStrings.Solutions);
				if (!Directory.Exists(solPath))
					continue;

				string fullFilePath = Path.Combine(solPath, fileName);
				if (File.Exists(fullFilePath))
					return fullFilePath;
			}
			return string.Empty;
		}

		/// <summary>
		/// Dato il nome di un prodotto restituisce il nome del file di Solution,
		/// ad esempio MagoNet-Std.Solution.xml.
		/// </summary>
		/// <param name="productName">Il nome del prodotto.</param>
		/// <returns>Il nome del file di Solution.</returns>
		//---------------------------------------------------------------------
		public static string GetSolutionFileName(string productName)
		{
			return productName + NameSolverStrings.SolutionExtension;
		}

		/// <summary>
		/// Dato il nome di un prodotto restituisce il nome del file di Solution
		/// "brandizzato", ad esempio MagoNet-Pro.Solution.Brand.xml.
		/// </summary>
		/// <param name="productName">Il nome del prodotto.</param>
		/// <returns>Il nome del file di Solution "brandizzato".</returns>
		//---------------------------------------------------------------------
		public static string GetBrandSolutionFileName(string productName)
		{
			return productName + NameSolverStrings.BrandSolutionExtension;
		}

		/// <summary>
		/// Ritorna il nome del file di brand nella forma [brandfile].brand.xml
		/// </summary>
		//---------------------------------------------------------------------
		public static string GetBrandProductFileName(string productName)
		{
			return productName + NameSolverStrings.BrandExtension;
		}

		/// <summary>
		/// Restituisce un arrayList di FileInfo relativi a tutti i file di solution
		/// </summary>
		//-----------------------------------------------------------------------------
		public FileInfo[] GetSolutionFiles(bool checkApplicationConfig)
		{
			string appsPaths = Path.Combine(GetStandardPath(), NameSolverStrings.TaskBuilderApplications);
			ArrayList list = new ArrayList();
			if (Directory.Exists(appsPaths)) // pre-TB2.0 versions named it differently
				foreach (string appPath in Directory.GetDirectories(appsPaths))
				{
					//Anomalia 14450: gli sviluppatori usano manipolare gli application.config 
					//per testare le proprie applicazioni con o senza altre. 
					//se noi carichiamo solo le solution che appartengono ad una applicazione valida 
					//li obblighiamo ogni volta a riattivare 
					//oltretutto questo è un effetto non voluto che prima dell a 2.7 non c'era.
                    if (checkApplicationConfig && !IsApplicationDirectory(appPath))
                    {//non tradurre
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Missing application.config in: " + appPath);
                        continue;
                    }

					string solPath = Path.Combine(appPath, NameSolverStrings.Solutions);
					if (!Directory.Exists(solPath))
						continue;

					string pattern = "*" + NameSolverStrings.SolutionExtension;

					list.AddRange(new DirectoryInfo(solPath).GetFiles(pattern));
				}
			return (FileInfo[])list.ToArray(typeof(FileInfo));
		}

		/// <summary>
		/// Restituisce un arrayList di FileInfo relativi a tutti i file di solution
		/// </summary>
		//-----------------------------------------------------------------------------
		public FileInfo[] GetSolutionFiles()
		{
			return GetSolutionFiles(false);
		}

		/// <summary>
		/// Restituisce un arrayList di FileInfo relativi ai file di solution 
		/// relativi al productName specificato
		/// </summary>
		//-----------------------------------------------------------------------------
		public FileInfo GetSolutionFileInfo(string productName)
		{
			string solutionFile = GetSolutionFile(productName);
			if (solutionFile == string.Empty)
				return null;

			return new FileInfo(solutionFile);
		}

		#endregion

		/// <summary>
		/// ritorna per esteso il path di un module.config
		/// </summary>
		/// <param name="appName">nome applicazione</param>
		/// <param name="moduleName">nome modulo</param>
		/// <returns>path</returns>
		//---------------------------------------------------------------------
		public string GetModuleConfigFullName(string appName, string moduleName)
		{
			return GetApplicationModulePath(appName, moduleName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Module +
                NameSolverStrings.ConfigExtension;
		}

		/// <summary>
		/// ritorna per esteso il path di un application.config
		/// </summary>
		/// <param name="appName">nome applicazione</param>
		/// <returns>path</returns>
		//---------------------------------------------------------------------
		public string GetApplicationConfigFullName(string appName)
		{
			return GetStandardApplicationPath(appName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Application +
                NameSolverStrings.ConfigExtension;
		}

		/// <summary>
		/// Retituisce Framwork\Xml
		/// </summary>
		/// <returns>path</returns>
		//---------------------------------------------------------------------
		public string GetStandardTaskBuilderXmlPath()
		{
			return Path.Combine(GetStandardApplicationPath(NameSolverStrings.Framework), xmlExt);
		}

		/// <summary>
		/// Restituisce Standard\Contenitore\Applicazione\Modulo\Dictionary
		/// </summary>
		/// <param name="appName">nome dell'applicazione</param>
		/// <param name="moduleName">nome del modulo</param>
		/// <returns>il percorso richiesto</returns>
		//---------------------------------------------------------------------
		public string GetStandardModuleDictionaryFilePath(string appName, string moduleName, string culture)
		{
			string modulePath = GetApplicationModulePath(appName, moduleName);
			if (modulePath == string.Empty)
				return string.Empty;

			string path = Path.Combine(modulePath, NameSolverStrings.Dictionary);
			path = Path.Combine(path, culture);
			return Path.Combine(path, NameSolverStrings.StandardDictionaryFile);
		}

		/// <summary>
		/// ritorna il path fino al livello della directory DatabaseScript
		/// </summary>
		/// <param name="application">nome dell'applicazione</param>
		/// <param name="module">nome del modulo</param>
		/// <returns>Path</returns>
		//---------------------------------------------------------------------
		public string GetStandardDatabaseScriptPath(string application, string module)
		{
			return GetApplicationModulePath(application, module) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.DatabaseScript;
		}

		/// <summary>
		/// per recuperare il file UpgradeInfo.xml per l'upgrade del database
		/// </summary>
		/// <param name="application">nome applicazione</param>
		/// <param name="module">nome modulo</param>
		/// <returns>path</returns>
		//---------------------------------------------------------------------
		public string GetStandardUpgradeInfoXML(string application, string module)
		{
			return GetStandardDatabaseScriptPath(application, module) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.UpgradeScript +
                Path.DirectorySeparatorChar +
                NameSolverStrings.UpgradeInfoXml;
		}

		/// <summary>
		/// per recuperare il file CreateInfo.xml per la creazione del database
		/// </summary>
		/// <param name="application">nome applicazione</param>
		/// <param name="module">nome modulo</param>
		/// <returns>path</returns>
		//---------------------------------------------------------------------
		public string GetStandardCreateInfoXML(string application, string module)
		{
			return GetStandardDatabaseScriptPath(application, module) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.CreateScript +
                Path.DirectorySeparatorChar +
                NameSolverStrings.CreateInfoXml;
		}

		/// <summary>
		/// per rintracciare i file contenenti gli script delle tabelle del database:
		/// - per la creazione non si scende nel dettaglio del numero di release
		/// - per l'upgrade occorre comporre il path con un livello in più (per es: "Release_2")
		/// In base al provider vengono cercate in directory specifiche, altrimenti si cerca nella "all"
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardScriptPath(string path, string nameScript, string provider, bool create, int rel)
		{
			string allPath = path;

			string script =
                (create)
                ? nameScript
                : string.Concat(NameSolverDatabaseStrings.ReleaseNumDirectory, rel, Path.DirectorySeparatorChar, nameScript);

            if (provider == NameSolverDatabaseStrings.SQLOLEDBProvider || provider == NameSolverDatabaseStrings.SQLODBCProvider)
				path += Path.DirectorySeparatorChar +
                    NameSolverStrings.SqlServer +
                    Path.DirectorySeparatorChar +
                    script;

			if (provider == NameSolverDatabaseStrings.OraOLEDBProvider ||
                provider == NameSolverDatabaseStrings.MSDAORAProvider)
				path += Path.DirectorySeparatorChar +
                    NameSolverStrings.Oracle +
                    Path.DirectorySeparatorChar +
                    script;

            if (provider == NameSolverDatabaseStrings.PostgreOdbcProvider)
                path += Path.DirectorySeparatorChar +
                    NameSolverStrings.Postgre +
                    Path.DirectorySeparatorChar +
                    script;

			if (File.Exists(path))
				return path;

			return
				(allPath += Path.DirectorySeparatorChar + NameSolverStrings.All + Path.DirectorySeparatorChar + script);
		}

		//Funzioni che utilizzano il namespace
		/// <summary>
		/// Restituisce l'applicationInfo realitivo all'applicazione del 
		/// namespace passato
		/// </summary>
		/// <param name="aNameSpace">Il namespace dell'applicazione</param>
		/// <returns>Un application info o null se non trovato</returns>
		//---------------------------------------------------------------------------------
		public virtual IBaseApplicationInfo GetApplicationInfo(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			string applicationName = aNameSpace.Application;

			if (applicationName == null || applicationName.Length == 0)
				return null;

			return GetApplicationInfoByName(applicationName);
		}

		/// <summary>
		/// Restituisce il moduleInfo relativo al namespace passato
		/// </summary>
		/// <param name="aNameSpace">Il namespace del modulo</param>
		//---------------------------------------------------------------------------------
		public virtual IBaseModuleInfo GetModuleInfo(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			string moduleName = aNameSpace.Module;
			if (moduleName == null || moduleName.Length == 0)
				return null;

			IBaseApplicationInfo aApplicationInfo = GetApplicationInfo(aNameSpace);

			if (aApplicationInfo == null)
				return null;

			return aApplicationInfo.GetModuleInfoByName(moduleName);
		}

		/// <summary>
		/// Ritorna il path completo del modulo specificato dal namespace
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardModulePath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			return GetApplicationModulePath(aNameSpace.Application, aNameSpace.Module);
		}

		/// <summary>
		/// 
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardModuleXmlPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			return Path.Combine(GetStandardModulePath(aNameSpace), xmlExt);
		}

		/// <summary>
		/// Ritorna la cartella XML del modulo desiderato: ad esempio \Framework\TbGenlib\Xml
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardModuleXmlPath(INameSpace aNameSpace, string language)
		{
			if (aNameSpace == null || language == string.Empty)
				return null;

			return Path.Combine(GetStandardModuleXmlPath(aNameSpace), language);
		}

		/// <summary>
		/// Ritorna la cartella di lingua XML del modulo desiderato: 
		/// ad esempio \Framework\TbGenlib\Xml\it-it
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetNumberToLiteralXmlFullName(INameSpace aNameSpace, string language)
		{
			if (aNameSpace == null || language == string.Empty)
				return null;

			return Path.Combine(GetStandardModuleXmlPath(aNameSpace, language), numberToLiteralXmlFileName);
		}

		/// <summary>
		///Ritorna la cartella Text del modulo desiderato: 
		/// ad esempio \Framework\TbGenlib\Text/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardModuleTextPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			string fileDir = Path.Combine(GetStandardModulePath(aNameSpace), NameSolverStrings.Files);

			return Path.Combine(fileDir, NameSolverStrings.Texts);
		}

		/// <summary>
		/// Ritorna il path dell'immagine di un gruppo di menu relativa al namespace in base al tema
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetGroupImagePathByTheme(INameSpace aNameSpace, string themeName)
		{
			string fileDir = Path.Combine(
			  GetCustomAllCompaniesModulePath(aNameSpace.Application, aNameSpace.Module),
			  NameSolverStrings.Files,
			  NameSolverStrings.Images, 
			  themeName
			  );

			string filePath = Path.Combine(fileDir, aNameSpace.Image);

			if (File.Exists(filePath))
			{
				return filePath;
			}

			filePath = GetImagePathByTheme(aNameSpace, themeName);
			if (File.Exists(filePath))
			{
				return filePath;
			}

			return string.Empty;
		}
			

		/// <summary>
		/// Ritorna il path dell'immagine di un gruppo di menu relativa al namespace.
		/// Cerca nella Custom all Companies e poi nella standard
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetGroupImagePath(INameSpace aNameSpace)
        {
            string fileDir = Path.Combine(
                GetCustomAllCompaniesModulePath(aNameSpace.Application, aNameSpace.Module),
                NameSolverStrings.Files,
                NameSolverStrings.Images
                );

            string filePath = Path.Combine(fileDir, aNameSpace.Image);

            if (File.Exists(filePath))
            {
                return filePath;
            }

            return GetImagePath(aNameSpace);
        }

		/// <summary>
		///Ritorna il path dell'immagine relativa al namespace
		//---------------------------------------------------------------------------------
		public string GetImagePathByTheme(INameSpace aNameSpace, string themeName, ImageSize size = ImageSize.None)
		{
			if (aNameSpace == null || themeName.IsNullOrEmpty())
				return null;

			string fileDir = string.Empty;
			string standardmodulepath = GetStandardModulePath(aNameSpace);
			if (standardmodulepath == null)
				return null;

			fileDir = Path.Combine(standardmodulepath, NameSolverStrings.Files, NameSolverStrings.Images, themeName);

			if (size != ImageSize.None)
				fileDir = Path.Combine(fileDir, size.ToString().ReplaceNoCase("Size", ""));

			return Path.Combine(fileDir, aNameSpace.Image);
		}

		/// <summary>
		///Ritorna il path dell'immagine relativa al namespace
		//---------------------------------------------------------------------------------
		public string GetImagePath(INameSpace aNameSpace, ImageSize size = ImageSize.None)
		{
			if(aNameSpace == null)
				return null;

			string fileDir = string.Empty;
            string standardmodulepath = GetStandardModulePath(aNameSpace);
            if (standardmodulepath == null) return null;
              
                if (size == ImageSize.None)
				fileDir = Path.Combine(standardmodulepath, NameSolverStrings.Files, NameSolverStrings.Images);
			else
				fileDir = Path.Combine(standardmodulepath, NameSolverStrings.Files, NameSolverStrings.Images, size.ToString().ReplaceNoCase("Size" , ""));

			return Path.Combine(fileDir, aNameSpace.Image);
		}

		/// <summary>
		/// Ritorna la moduleobjects del modulo desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardModuleObjectsPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			return GetApplicationModuleObjectsPath(aNameSpace.Application, aNameSpace.Module);
		}

        /// <summary>
        /// Ritorna la moduleobjects del modulo desiderato
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetApplicationModuleObjectsPath(string applicationName, string moduleName)
        {
            return Path.Combine(GetApplicationModulePath(applicationName, moduleName), NameSolverStrings.ModuleObjects);
        }
        /// <summary>
        /// Ritorna la moduleobjects del modulo desiderato
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardModuleObjectsPath(string applicationName, string moduleName)
        {
            var standardModulePath= GetStandardModulePath(applicationName, moduleName);
            return string.IsNullOrEmpty(standardModulePath) 
				? string.Empty
				: Path.Combine(standardModulePath, NameSolverStrings.ModuleObjects);
        }

        /// <summary>
        /// Ritorna la referenceobjects del modulo desiderato
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardReferenceObjectsPath(string applicationName, string moduleName)
		{
			return Path.Combine(GetApplicationModulePath(applicationName, moduleName), NameSolverStrings.ReferenceObjects);
		}

        /// <summary>
        /// Ritorna la UIController del modulo desiderato
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardUIControllerFilePath(string applicationName, string module, string migrationFileName)
		{
			return Path.Combine(GetStandardUIControllerPath(applicationName, module), migrationFileName);
		}

		/// <summary>
		/// Ritorna il file di migration desiderato dentro la cartella UIController del modulo desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardUIControllerFilePath(INameSpace aNameSpace, string migrationFileName)
		{
			return Path.Combine(GetStandardUIControllerPath(aNameSpace), migrationFileName);
		}

		/// <summary>
		/// Ritorna la cartella UIController del modulo desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardUIControllerPath(string applicationName, string module)
		{
			return Path.Combine(GetApplicationModulePath(applicationName, module), uiControllers);
		}

		/// <summary>
		/// Ritorna la cartella UIController del modulo desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardUIControllerPath(INameSpace aNameSpace)
		{
			return Path.Combine(GetStandardModulePath(aNameSpace), uiControllers);
		}

		/// <summary>
		///Ritorna la cartella Document del modulo desiderato 
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardDocumentPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			return Path.Combine(GetStandardModuleObjectsPath(aNameSpace), aNameSpace.Document);
		}

		/// <summary>
		///Ritorna la cartella Document del modulo desiderato 
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetApplicationDocumentPath(string applicationName, string moduleName, string documentName)
		{
			return Path.Combine(GetApplicationModuleObjectsPath(applicationName, moduleName), documentName);
		}
        /// <summary>
        ///Ritorna la cartella Document del modulo desiderato 
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardDocumentPath(string applicationName, string moduleName, string documentName)
        {
            var standardModuleObjectsPath = GetStandardModuleObjectsPath(applicationName, moduleName);
            return string.IsNullOrEmpty(standardModuleObjectsPath) 
				? string.Empty
				: Path.Combine(standardModuleObjectsPath, documentName);
		}

        /// <summary>
        ///Ritorna la cartella Document\Description del modulo desiderato 
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardDocumentDescriptionPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			return Path.Combine(GetStandardDocumentPath(aNameSpace), description);
		}

		/// <summary>
		/// Ritorna la cartella di export profiles del modulo desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardDocumentExportprofilesPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			return Path.Combine(GetStandardDocumentPath(aNameSpace), NameSolverStrings.ExportProfiles);
		}

        //---------------------------------------------------------------------
        protected string GetCustomAllCompaniesModulePath(string applicationName, string moduleName)
        {
            string customApplicationPath = GetCustomAllCompaniesApplicationPath(applicationName);
            if (customApplicationPath == string.Empty)
                return string.Empty;

            return customApplicationPath + Path.DirectorySeparatorChar + moduleName;
        }

        //---------------------------------------------------------------------
        private string GetCustomAllCompaniesApplicationPath(string applicationName)
        {
            IBaseApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);
            if (aApplicationInfo == null)
                return string.Empty;

            string customApplicationContainer = GetCustomAllCompaniesApplicationContainerPath(aApplicationInfo.ApplicationType);

            if (customApplicationContainer == string.Empty)
                return string.Empty;

            return
                customApplicationContainer +
                Path.DirectorySeparatorChar +
                aApplicationInfo.Name;
        }

		//---------------------------------------------------------------------
		private string GetCustomAllCompaniesApplicationContainerPath(ApplicationType aApplicationType)
        {
            if (aApplicationType == ApplicationType.All ||
                aApplicationType == ApplicationType.Undefined)
            {
                diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
                return string.Empty;
            }

            string customAllCompaniesPath = GetCustomAllCompaniesPath();
            if (customAllCompaniesPath == string.Empty)
                return string.Empty;

            string applicationContainerName = GetApplicationContainerName(aApplicationType);
            if (applicationContainerName == string.Empty)
            {
                diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
                return string.Empty;
            }

            return
                customAllCompaniesPath +
                Path.DirectorySeparatorChar +
                applicationContainerName;
        }

		/// <summary>
		/// Ritorna la cartella di export profiles del modulo desiderato nella custom
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomModuleObjectsPath(string companyName, string application, string module)
		{
			string customModulePath = GetCustomModulePath(companyName, application, module);
			if (customModulePath == string.Empty)
				return string.Empty;

			return Path.Combine(customModulePath, NameSolverStrings.ModuleObjects);
		}

        /// <summary>
        /// Ritorna la cartella di document del modulo desiderato nella custom
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetCustomDocumentPath(string companyName, string application, string module, string document)
		{
			string customModuleObjectsPath = GetCustomModuleObjectsPath(companyName, application, module);
			if (customModuleObjectsPath == string.Empty)
				return string.Empty;

			return Path.Combine(customModuleObjectsPath, document);
		}

		/// <summary>
		/// Restituisce il documentInfo relativo al namespace passato
		/// </summary>
		/// <param name="aNameSpace">Il namespace del documento</param>
		/// <returns>Il documentInfo o null se il documento non esiste</returns>
		//---------------------------------------------------------------------------------
		public IDocumentInfo GetDocumentInfo(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			IBaseModuleInfo aModuleInfo = GetModuleInfo(aNameSpace);
			if (aModuleInfo == null)
				return null;

			return aModuleInfo.GetDocumentInfoByNameSpace(aNameSpace.FullNameSpace);
		}

		/// <summary>
		/// Ritorna la cartella di document del modulo desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetDocumentPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			IBaseModuleInfo aModuleInfo = GetModuleInfo(aNameSpace);
			if (aModuleInfo == null)
				return null;

			return aModuleInfo.GetDocumentPath(aNameSpace.Document);
		}

		/// <summary>
		/// Restituisce Standard\Contenitore\Applicazione\Modulo\Dictionary utilizzando il path
		/// di un filewoorm come punto di partenza
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetDictionaryFilePathFromWoormFile(string woormFilePath)
		{
			bool isStandard = IsStandardPath(woormFilePath);

			int steps = isStandard ? 2 : 4;

			for (int i = 0; i < steps; i++)
			{
				woormFilePath = Path.GetDirectoryName(woormFilePath);

				if (!isStandard && i == 0 && Path.GetFileName(woormFilePath).ToLowerInvariant() == "allusers")
					steps--;
			}

			string moduleName = Path.GetFileName(woormFilePath);
			woormFilePath = Path.GetDirectoryName(woormFilePath);
			string appName = Path.GetFileName(woormFilePath);

			return GetStandardModuleDictionaryFilePath(appName, moduleName, Thread.CurrentThread.CurrentUICulture.Name);
		}

		/// <summary>
		/// Restituisce Standard\Contenitore\Applicazione\Modulo\Dictionary utilizzando un nome di 
		/// tabella come punto di partenza
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetDictionaryFilePathFromTableName(string tableName)
		{
			foreach (BaseApplicationInfo ai in ApplicationInfos)
			{
				foreach (BaseModuleInfo mi in ai.Modules)
				{
					if (mi.DatabaseObjectsInfo.TableInfoArray == null) continue;
					foreach (TableInfo table in mi.DatabaseObjectsInfo.TableInfoArray)
					{
						if (string.Compare(table.Name, tableName, true, CultureInfo.InvariantCulture) == 0)
							return mi.DictionaryFilePath;
					}

					if (mi.DatabaseObjectsInfo.ViewInfoArray == null) continue;
					foreach (ViewInfo view in mi.DatabaseObjectsInfo.ViewInfoArray)
					{
						if (string.Compare(view.Name, tableName, true, CultureInfo.InvariantCulture) == 0)
							return mi.DictionaryFilePath;
					}

					if (mi.DatabaseObjectsInfo.ProcedureInfoArray == null) continue;
					foreach (ProcedureInfo proc in mi.DatabaseObjectsInfo.ProcedureInfoArray)
					{
						if (string.Compare(proc.Name, tableName, true, CultureInfo.InvariantCulture) == 0)
							return mi.DictionaryFilePath;
					}
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// Standard\TaskBuilderApplication\Application\Module\DataManager
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardDataManagerPath(string application, string module)
		{
			return GetApplicationModulePath(application, module) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.DataManager;
		}

		/// <summary>
		/// Standard\TaskBuilderApplication\Application\Module\DataManager\Default\<language>\<edition>
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardDataManagerDefaultPath(string application, string module, string language, string edition)
		{
			return GetStandardDataManagerPath(application, module) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Default +
                Path.DirectorySeparatorChar +
                language +
                Path.DirectorySeparatorChar +
                edition;

		}

		/// <summary>
		/// Standard\TaskBuilderApplication\Application\Module\DataManager\Sample\<language>\<edition>	
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardDataManagerSamplePath(string application, string module, string language, string edition)
		{
			return GetStandardDataManagerPath(application, module) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Sample +
                Path.DirectorySeparatorChar +
                language +
                Path.DirectorySeparatorChar +
                edition;
		}

		/// <summary>
		/// Ritorna il fullpath del file Reports.xml dato il namespace desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardReportFile(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			string descriptionPath = GetStandardModuleObjectsPath(aNameSpace);
			descriptionPath = Path.Combine(descriptionPath, aNameSpace.Document);
			descriptionPath = Path.Combine(descriptionPath, description);
			return Path.Combine(descriptionPath, NameSolverStrings.ReportXml);

		}

		/// <summary>
		/// Ritorna il fullpath della cartella Reports dato il namespace desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardReportPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			string descriptionPath = GetStandardModulePath(aNameSpace);
			return Path.Combine(descriptionPath, NameSolverStrings.Report);
		}

		/// <summary>
		/// Ritorna il fullpath della cartella Dbts dato il namespace desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetDbtsPath(INameSpace aNameSpace)
		{
			if (aNameSpace == null)
				return null;

			string descriptionPath = GetStandardModuleObjectsPath(aNameSpace);
			descriptionPath = Path.Combine(descriptionPath, aNameSpace.Document);
			descriptionPath = Path.Combine(descriptionPath, description);
			return Path.Combine(descriptionPath, NameSolverStrings.DbtsXml);
		}

		/// <summary>
		/// Ritorna il fullpath della cartella Description dato il namespace desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetDescriptionFolder(string applicationPath, INameSpace aNameSpace)
		{
			if (applicationPath == null || applicationPath == string.Empty)
				return string.Empty;

			string path = Path.Combine(applicationPath, aNameSpace.Module);
			path = Path.Combine(path, aNameSpace.Document);
			path = Path.Combine(path, NameSolverStrings.ModuleObjects);
			return Path.Combine(path, description);
		}

		/// <summary>
		/// True se il namespace fornito è nella forma http://www.microarea.it/Schema/2004/Smart/... 
		/// </summary>
		//---------------------------------------------------------------------------------
		public bool IsMicroareaSchema(string namespaceURI)
		{
			return namespaceURI.StartsWith("http://www.microarea.it/Schema/2004/Smart/");
		}

		#region Possibili formati di NsUri
		/*
		report
		.APP/mod/aaa.xsd
		.APP/mod/AllUsers/	aaa.xsd
		.APP/mod/users/		sa/			aaa.xsd
		
		document
		.APP/mod/Doc/		aaa.xsd
		.APP/mod/Doc/		AllUsers/	aaa.xsd
		.APP/mod/Doc/		users/		sa/			aaa.xsd
		*/
		#endregion

		/// <summary>
		/// Ritorna il namespace dato un Uri e ritorna inoltre lo user dello schema
		/// </summary>
		/// <param name="namespaceURI">Uri nel formato utilizzato da Magic Document</param>
		/// <param name="schemaUser">variabile di out contenente lo user</param>
		//---------------------------------------------------------------------------------
		public INameSpace GetNSAndUserFromNSURI(string namespaceURI, out string schemaUser)
		{
			schemaUser = NameSolverStrings.AllUsers;
			string schemaName = string.Empty;
			string nsString = string.Empty;

			string s = namespaceURI.Replace("http://www.microarea.it/Schema/2004/Smart/", "");
			s = s.Replace(Path.DirectorySeparatorChar, '/');
			string[] tokens = s.Split('/');

			switch (tokens.Length)
			{
				case 3:
					//report std
					schemaName = tokens[2].Replace(".xsd", "");
					nsString = string.Format("ReportSchema.{0}.{1}.{2}", tokens[0], tokens[1], schemaName);
					schemaUser = NameSolverStrings.Standard;
					break;
				case 4:
					if (string.Compare(tokens[2], NameSolverStrings.AllUsers, true, CultureInfo.InvariantCulture) == 0)
					{
						//report allusers
						schemaName = tokens[3].Replace(".xsd", "");
						nsString = string.Format("ReportSchema.{0}.{1}.{2}", tokens[0], tokens[1], schemaName);
						schemaUser = NameSolverStrings.AllUsers;
					}
					else
					{
						//document std
						schemaName = tokens[3].Replace(".xsd", "");
						nsString = string.Format("DocumentSchema.{0}.{1}.{2}.{3}", tokens[0], tokens[1], tokens[2], schemaName);
						schemaUser = NameSolverStrings.Standard;
					}
					break;
				case 5:
					if (string.Compare(tokens[3], NameSolverStrings.AllUsers, true, CultureInfo.InvariantCulture) == 0)
					{
						//document allusers
						schemaName = tokens[4].Replace(".xsd", "");
						nsString = string.Format("DocumentSchema.{0}.{1}.{2}.{3}", tokens[0], tokens[1], tokens[2], schemaName);
						schemaUser = NameSolverStrings.AllUsers;
					}
					else
					{
						//report user
						schemaName = tokens[4].Replace(".xsd", "");
						nsString = string.Format("ReportSchema.{0}.{1}.{2}", tokens[0], tokens[1], schemaName);
						schemaUser = tokens[3];
					}
					break;
				case 6:
					//document user
					schemaName = tokens[5].Replace(".xsd", "");
					nsString = string.Format("DocumentSchema.{0}.{1}.{2}.{3}", tokens[0], tokens[1], tokens[2], schemaName);
					schemaUser = tokens[4];
					break;
				default:
					Debug.Assert(false);
					return null;
			}

			NameSpace ns = new NameSpace(nsString);
			if (!ns.IsValid())
				return null;

			return ns;
		}

		/// <summary>
		/// Ritorna il full filenname (per lingua) relativo al namespace desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		protected string GetStandardFilename(BaseModuleInfo aModuleInfo, INameSpace aNamespace, string language)
		{
			if (aModuleInfo == null || !aNamespace.IsValid())
				return String.Empty;

			string fullFileName = String.Empty;

			switch (aNamespace.NameSpaceType.Type)
			{
				case NameSpaceObjectType.Report:
					{
						string reportFileName = aNamespace.GetReportFileName();
						if (reportFileName == null || reportFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardReportFullFilename(reportFileName);
						break;
					}

				case NameSpaceObjectType.Image:
					fullFileName = aModuleInfo.GetStandardImageFullFilename(aNamespace.Image);
					break;

				case NameSpaceObjectType.Text:
					fullFileName = aModuleInfo.GetStandardTextFullFilename(aNamespace.Text);
					break;

				case NameSpaceObjectType.File:
					fullFileName = aModuleInfo.GetStandardFileFullFilename(aNamespace.File);
					break;

				case NameSpaceObjectType.ExcelDocument:
					{
						string documentFileName = aNamespace.GetExcelDocumentFileName();
						if (documentFileName == null || documentFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardExcelDocumentFullFilename(documentFileName, language);
						break;
					}

				case NameSpaceObjectType.ExcelTemplate:
					{
						string templateFileName = aNamespace.GetExcelTemplateFileName();
						if (templateFileName == null || templateFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardExcelTemplateFullFilename(templateFileName, language);
						break;
					}

				case NameSpaceObjectType.WordDocument:
					{
						string documentFileName = aNamespace.GetWordDocumentFileName();
						if (documentFileName == null || documentFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardWordDocumentFullFilename(documentFileName, language);
						break;
					}

				case NameSpaceObjectType.WordTemplate:
					{
						string templateFileName = aNamespace.GetWordTemplateFileName();
						if (templateFileName == null || templateFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardWordTemplateFullFilename(templateFileName, language);
						break;
					}

				case NameSpaceObjectType.ExcelDocument2007:
					{
						string documentFileName = aNamespace.GetExcel2007DocumentFileName();
						if (documentFileName == null || documentFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardExcelDocumentFullFilename(documentFileName, language);
						break;
					}

				case NameSpaceObjectType.ExcelTemplate2007:
					{
						string templateFileName = aNamespace.GetExcel2007TemplateFileName();
						if (templateFileName == null || templateFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardExcelTemplateFullFilename(templateFileName, language);
						break;
					}

				case NameSpaceObjectType.WordDocument2007:
					{
						string documentFileName = aNamespace.GetWord2007DocumentFileName();
						if (documentFileName == null || documentFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardWordDocumentFullFilename(documentFileName, language);
						break;
					}

				case NameSpaceObjectType.WordTemplate2007:
					{
						string templateFileName = aNamespace.GetWord2007TemplateFileName();
						if (templateFileName == null || templateFileName == String.Empty)
							return String.Empty;

						fullFileName = aModuleInfo.GetStandardWordTemplateFullFilename(templateFileName, language);
						break;
					}

				case NameSpaceObjectType.DocumentSchema:
					fullFileName = aModuleInfo.GetStandardDocumentSchemaFullFilename(aNamespace.Library, aNamespace.DocumentSchema);
					break;

				case NameSpaceObjectType.ReportSchema:
					fullFileName = aModuleInfo.GetStandardReportSchemaFullFilename(aNamespace.ReportSchema);
					break;
			}

			return fullFileName;
		}

		//---------------------------------------------------------------------------------
		public string GetCustomAllCompaniesOfficeItem(
			string itemObject,
			MenuXmlNode.MenuXmlNodeCommandSubType commandSubType,
			MenuXmlNode.OfficeItemApplication application
			)
		{

			NameSpace officeFileNameSpace = GetOfficeItemNamespace(itemObject, commandSubType, application);

			IBaseModuleInfo aModuleInfo = GetModuleInfoByName(officeFileNameSpace.Application, officeFileNameSpace.Module);
			if (aModuleInfo == null)
				return string.Empty;

			string fileName = string.Empty;

			switch (officeFileNameSpace.NameSpaceType.Type)
			{
				case NameSpaceObjectType.ExcelDocument:
					fileName = officeFileNameSpace.GetExcelDocumentFileName();
					break;
				case NameSpaceObjectType.ExcelTemplate:
					fileName = officeFileNameSpace.GetExcelTemplateFileName();
					break;
				case NameSpaceObjectType.WordDocument:
					fileName = officeFileNameSpace.GetWordDocumentFileName();
					break;
				case NameSpaceObjectType.WordTemplate:
					fileName = officeFileNameSpace.GetWordTemplateFileName();
					break;
				case NameSpaceObjectType.ExcelDocument2007:
					fileName = officeFileNameSpace.GetExcel2007DocumentFileName();
					break;
				case NameSpaceObjectType.ExcelTemplate2007:
					fileName = officeFileNameSpace.GetExcel2007TemplateFileName();
					break;
				case NameSpaceObjectType.WordDocument2007:
					fileName = officeFileNameSpace.GetWord2007DocumentFileName();
					break;
				case NameSpaceObjectType.WordTemplate2007:
					fileName = officeFileNameSpace.GetWord2007TemplateFileName();
					break;
				default:
					fileName = string.Empty;
					break;
			}

			if (fileName.IsNullOrEmpty())
				return string.Empty;

			return Path.Combine
				(
				GetCustomAllCompaniesOfficeItemsFolder(aModuleInfo, application),
				fileName
				);
		}

		//-------------------------------------------------------------------------------
		internal NameSpace GetOfficeItemNamespace(string itemObject, MenuXmlNode.MenuXmlNodeCommandSubType commandSubType, MenuXmlNode.OfficeItemApplication application)
		{
			NameSpace officeFileNameSpace = null;
			switch (application)
			{
				case MenuXmlNode.OfficeItemApplication.Excel:
					if (commandSubType.IsOfficeDocument)
						officeFileNameSpace = new NameSpace(itemObject, NameSpaceObjectType.ExcelDocument);
					else if (commandSubType.IsOfficeDocument2007)
						officeFileNameSpace = new NameSpace(itemObject, NameSpaceObjectType.ExcelDocument2007);
					else if (commandSubType.IsOfficeTemplate)
						officeFileNameSpace = new NameSpace(itemObject, NameSpaceObjectType.ExcelTemplate);
					else if (commandSubType.IsOfficeTemplate2007)
						officeFileNameSpace = new NameSpace(itemObject, NameSpaceObjectType.ExcelTemplate2007);
					break;
				case MenuXmlNode.OfficeItemApplication.Word:
					if (commandSubType.IsOfficeDocument)
						officeFileNameSpace = new NameSpace(itemObject, NameSpaceObjectType.WordDocument);
					else if (commandSubType.IsOfficeDocument2007)
						officeFileNameSpace = new NameSpace(itemObject, NameSpaceObjectType.WordDocument2007);
					else if (commandSubType.IsOfficeTemplate)
						officeFileNameSpace = new NameSpace(itemObject, NameSpaceObjectType.WordTemplate);
					else if (commandSubType.IsOfficeTemplate2007)
						officeFileNameSpace = new NameSpace(itemObject, NameSpaceObjectType.WordTemplate2007);
					break;
				default:
					break;
			}
			return officeFileNameSpace;
		}

		//-------------------------------------------------------------------------------
		private string GetCustomAllCompaniesOfficeItemsFolder(IBaseModuleInfo moduleInfo, MenuXmlNode.OfficeItemApplication application)
		{
			return System.IO.Path.Combine
				(
				BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath(),
				moduleInfo.ParentApplicationInfo.Name,
				moduleInfo.ModuleConfigInfo.ModuleName,
				application.ToString()
				);
		}

		/// <summary>
		/// Restituisce la cartella in cui si trova TBLoader.EXE
		/// (path finder potrebbe non avere informazioni sufficienti, in tal caso viene restituito null)
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetTBLoaderPath()
		{
			return GetExecutablePath(ref tbApplicationPath, "TBLoader.exe", NameSolverStrings.TbApps);
		}

		/// <summary>
        /// Restituisce il percorso completo dell'eseguibili "TbAppManager.exe"
		/// (path finder potrebbe non avere informazioni sufficienti, in tal caso viene restituito null)
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetMagoNetApplicationPath() //NON RINOMINARE IN GetTbAppManagerApplicationPath altrimenti non funziona più il test manager
		{
            return Path.Combine(GetExecutablePath(ref magonetApplicationPath, "TbAppManager.exe", "TbAppManager"), "TbAppManager.exe");
		}

		/// <summary>
		/// Restituisce il percorso completo dell'eseguibile "MicroareaConsole.exe"
		/// (path finder potrebbe non avere informazioni sufficienti, in tal caso viene restituito null)
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetMicroareaConsoleApplicationPath()
		{
			return Path.Combine(GetExecutablePath(ref microareaConsoleApplicationPath, "AdministrationConsole.exe", "AdministrationConsole"), "AdministrationConsole.exe");
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private string GetExecutablePath(ref string targetPath, string exe, string folder)
		{
			if (targetPath != null)
				return targetPath;

			//di default, ipotizzo si trovi nella cartella di esecuzione del programma
			targetPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

			if (File.Exists(Path.Combine(targetPath, exe)))
				return targetPath;

			if (isRunningInsideInstallation)
			{
				//provo a vedere se mi trovo in \<Installazione>\Apps\<app>\<debug|release>
				//(solo in sviluppo)
				targetPath = Path.Combine(Path.Combine(GetAppsPath(), folder), Build);
				//se esiste la cartella  \<Installazione>\Apps\<cartella>\<debug|release>, quello e' il path cercato
				if (File.Exists(Path.Combine(targetPath, exe)))
					return targetPath;

				//provo a vedere se mi trovo nella cartella di pubblicazione
				targetPath = Path.Combine(GetAppsPath(), "publish");
				if (File.Exists(Path.Combine(targetPath, exe)))
					return targetPath;

			}

			targetPath = "";
			return targetPath;
		}


		//--------------------------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Ritorna il percorso del file di versione dell'installazione
		/// </summary>
		/// <returns></returns>
		public string GetInstallationVersionPath()
		{
			return Path.Combine(standardPath, NameSolverStrings.InstallationVersion);
		}

        //---------------------------------------------------------------------------
        public string GetGenericBrandFilePath()
        {
            return Path.Combine(
                GetStandardPath(),
                NameSolverStrings.GenericBrandFile
                );
        }

        //---------------------------------------------------------------------------
        public string GetInstallationVersionFromInstallationVer()
		{
			return GetInstallationVersionFromInstallationVer(null);
		}

		//---------------------------------------------------------------------------
		public string GetInstallationVersionFromInstallationVer(string product)
		{
			if (!String.IsNullOrWhiteSpace(product) && !product.ToLower().StartsWith("magonet-"))
				//lo so fa schifo, ma altrimenti non so come riconoscere i casi in cui mi stanno chiedendo la versione di un verticale o embedded, 
				//che non hanno il file in questione.
				return GetAppInfoVersionFromSolutionName(product);

			InstallationVersion info = GetInstallationVer();
			return info.Version;
		}

		//---------------------------------------------------------------------------
		private InstallationVersion installationVer;

		public InstallationVersion InstallationVer
		{
			get
			{
				if (installationVer == null)
					installationVer = GetInstallationVer();
				return installationVer;
			}
		}

		public string ProductVersion { get { return InstallationVer.Version; } }
		public DateTime ProductDate { get { return InstallationVer.BDate; } }

		protected InstallationVersion GetInstallationVer()
		{
			string path = GetInstallationVersionPath();

			try
			{
				//se non trova il file (generato dal setup, quindi in debug e' normale) restituisce la versione della
				//dll corrente
				return InstallationVersion.LoadFromOrCreate(path);
			}
			catch (Exception exc)
			{
				diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, System.Reflection.MethodInfo.GetCurrentMethod().Name + ": " + exc.ToString());
				return new InstallationVersion();
			}
		}

		#endregion

		#region funzioni protette e private

		/// <summary>
		/// Ritorna il valore del Setting specficato dal name (Cerca in Framework, TbGenLib, preference)
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		//----------------------------------------------------------------------------
		private int GetSettingValue(string name)
		{
			SettingItem i = GetSettingItem("Framework", "TBGenlib", "Preference", name);
			if (i == null) return 0;
			int x = 0;
			int.TryParse((string)i.Values[0], out x);
			return x;
		}

		//---------------------------------------------------------------------
		public void CreateDynamicApplicationInfo
			(
			string applicationName,
			string appsPath
			)
		{
			CreateApplicationInfo(applicationName, ApplicationType.Customization, appsPath);
		}

		/// <summary>
		/// Aggiunge all'array degli applicationinfo l'applicazione desiderata
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="applicationType"></param>
		/// <param name="appsPath"></param>
		//---------------------------------------------------------------------
		public virtual void CreateApplicationInfo
			(
			string applicationName,
			ApplicationType applicationType,
			string appsPath
			)
		{
			if (
				string.IsNullOrEmpty(applicationName) ||
                applicationType == ApplicationType.Undefined ||
                !IsApplicationDirectory(appsPath + Path.DirectorySeparatorChar + applicationName)
				)
				return;

			//oggetto contenente le info di un'applicazione
			BaseApplicationInfo applicationInfo = new BaseApplicationInfo
				(
				applicationName,
				appsPath,
				this
				);

			//aggiungo l'applicazione all'array
			if (applicationInfo.IsKindOf(applicationType))
				applications.Add(applicationInfo);
		}
		#endregion

			#region funzioni Custom senza user o company
			/// <summary>
			/// Ritorna il Full file path del ServerConnection.config
			/// </summary>
			//-----------------------------------------------------------------------------
		public string ServerConnectionFile
		{
			get
			{
				if (customPath == string.Empty)
					return string.Empty;

				return
					customPath +
                    Path.DirectorySeparatorChar +
                    NameSolverStrings.ServerConnection +
                    NameSolverStrings.ConfigExtension;
			}
		}

		/// <summary>
		/// True se il path è di tipo custom
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IsCustomPath(string aPath)
		{
			string path = GetCustomPath().ToLower(CultureInfo.InvariantCulture);
			aPath = aPath.Replace('/', Path.PathSeparator);
			aPath = aPath.ToLower(CultureInfo.InvariantCulture);

			return aPath.StartsWith(path);
		}

		/// <summary>
		/// Ritorna il path della custom
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetCustomPath()
		{
			return customPath;
		}

		/// <summary>
		/// Ritorna il path della cartella Companies
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetCustomCompaniesPath()
		{
			return Path.Combine(GetCustomPath(), NameSolverStrings.Subscription);
		}

		/// <summary>
		/// Ritorna il path della cartella AllCompanies
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetCustomAllCompaniesPath()
		{
			return Path.Combine(GetCustomCompaniesPath(), NameSolverStrings.AllCompanies);
		}

        /// <summary>
        /// Ritorna il path della cartella AllCompanies
        /// </summary>
        //-----------------------------------------------------------------------------
        public string GetEasyStudioCustomizationsPath()
        {
            return
                easyStudioConfiguration.Settings.CustomizationsInCustom ?
                Path.Combine(Path.Combine(GetCustomPath(), NameSolverStrings.Subscription), easyStudioConfiguration.Settings.HomeName) :
                GetStandardPath();
        }


        ///<summary>
        /// Ritorna il path del file BIN per la nuova gestione 3.0
        ///</summary>
        //-----------------------------------------------------------------------------
        public string GetDatabaseObjectsBinPath()
		{
			return Path.Combine(GetCustomPath(), NameSolverStrings.DatabaseObjectsBinFile);
		}

		///<summary>
		/// Ritorna il path della cartella Temp nella custom
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetTempPath()
		{
			return Path.Combine(GetCustomPath(), "Temp");
		}

		///<summary>
		/// Ritorna il path della cartella Temp\WebProxyImages nella custom
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetWebProxyImagesPath()
		{
			return Path.Combine(GetTempPath(), "WebProxyImages");
		}

		///<summary>
		/// Ritorna il path della cartella Temp\WebProxyFiles nella custom
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetWebProxyFilesPath()
		{
			return Path.Combine(GetTempPath(), "WebProxyFiles");
		}

		///<summary>
		/// Ritorna il fullpath della cartella Custom\Configuration
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetCustomConfigurationPath()
		{
			string customPath = GetCustomPath();
			if (customPath == string.Empty)
				return string.Empty;

			return customPath +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Configuration;
		}
		
		///<summary>
		/// Ritorna il fullpath del file App_Data\MessagesQueue.bin"
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetMessagesQueuePath()
		{
			string path = GetLogManAppDataPath();
			if (path == string.Empty)
				return string.Empty;

			return path +
                Path.DirectorySeparatorChar +
                NameSolverStrings.MessagesQueueFile;
		}


		///<summary>
		/// Ritorna il fullpath del file Custom\MessagesQueue.bin"
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetShsFilePath()
		{
			string path = GetCustomConfigurationPath();
			return path +
                Path.DirectorySeparatorChar +
                NameSolverStrings.ShsFile;
		}

		///<summary>
		/// Ritorna il fullpath del file App_Data
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetLogManAppDataPath()
		{
			return Path.Combine(LoginManagerPath, NameSolverStrings.AppData);
		}


        ///<summary>
        /// Ritorna il fullpath del file App_Data
        ///</summary>
        //-----------------------------------------------------------------------------
        public string GetM4ClientPath()
        {
            BaseApplicationInfo aApplicationInfo = null;
            try
            {
                aApplicationInfo = (BaseApplicationInfo)GetApplicationInfoByName(NameSolverStrings.WebFramework);
                if (aApplicationInfo != null)
                    return Path.Combine(aApplicationInfo.Path, NameSolverStrings.M4Client);
            }
            catch { }
            return string.Empty;
        }

        ///<summary>
        /// Ritorna il fullpath del file App_Data
        ///</summary>
        //-----------------------------------------------------------------------------
        public string GetM4ServerPath()
        {
            BaseApplicationInfo aApplicationInfo = null;
            try
            {
                aApplicationInfo = (BaseApplicationInfo)GetApplicationInfoByName(NameSolverStrings.WebFramework);
                if (aApplicationInfo != null)
                    return Path.Combine(aApplicationInfo.Path, NameSolverStrings.M4Server);
            }
            catch { }
            return string.Empty;
        }

        ///<summary>
        /// Ritorna il fullpath del LockFile
        ///</summary>
        //-----------------------------------------------------------------------------
        public string GetLockLogFile()
		{
			string customPath = GetCustomPath();

			if (customPath == string.Empty)
				return string.Empty;

			return customPath + Path.DirectorySeparatorChar + NameSolverStrings.LockLogFile;
		}

		///<summary>
		/// Ritorna il fullpath del DataMigration Log
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetDataMigrationLogPath()
		{
			IBaseModuleInfo mi = GetModuleInfoByName("MicroareaConsole", "MigrationKitAdmin");
			if (mi == null)
				return string.Empty;

			return mi.GetDataMigrationLogPath();
		}

		///<summary>
		/// Ritorna il fullpath del file di Setting dei regression Test
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetRegressionTestSettingsPath()
		{
			string modCusP = GetCustomApplicationPath(NameSolverStrings.AllCompanies, "MicroareaConsole");
			if (modCusP == string.Empty)
				return string.Empty;

			return System.IO.Path.Combine(modCusP, NameSolverStrings.RegressionTestSettings);
		}

		///<summary>
		/// Ritorna il fullpath del file di User.info
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetUserInfoFile()
		{
			string appDataPath = GetLogManAppDataPath();
			if (appDataPath == string.Empty)
				return string.Empty;

			return
				appDataPath +
                Path.DirectorySeparatorChar +
                NameSolverStrings.FileUserInfo;
		}

		///<summary>
		/// Ritorna il fullpath del file di license dato il productName
		///</summary>
		//-----------------------------------------------------------------------------
		public string GetLicensedFile(string productName)
		{
			string appDataPath = GetLogManAppDataPath();
			if (appDataPath == string.Empty)
				return string.Empty;

			return appDataPath +
                Path.DirectorySeparatorChar +
                productName +
                NameSolverStrings.LicensedExtension;
		}

		///<summary>
		/// Ritorna un array di fileInfo per ogni file di license trovato
		///</summary>
		//-----------------------------------------------------------------------------
		public FileInfo[] GetLicensedFiles()
		{
			string appDataPath = GetLogManAppDataPath();
			if (appDataPath == string.Empty)
				return null;

			DirectoryInfo di = new DirectoryInfo(appDataPath);
			if (!di.Exists)
				return null;

			try
			{
				return di.GetFiles(NameSolverStrings.MaskFileLicensed);
			}
			catch (System.Security.SecurityException exc)
			{
				Debug.WriteLine("BasePathFinder.GetLicensedFiles - Error: " + exc.Message);
				string error = String.Concat(Messages.ErrorAccessingLicensedFile, Environment.NewLine, exc.Message);
				diagnostic.Set(DiagnosticType.Error, error);
				return new FileInfo[] { };
			}
		}

		/// <summary>
		/// utilizzato data una path standard e una path custom effettua il  caricamento dei file xml
		/// trovati considerando prima quelli nella custom che sovvrascivono quelli della standar)
		/// </summary>
		/// <param name="standardDir"></param>
		/// <param name="customDir"></param>
		/// <param name="fileList"></param>
		//---------------------------------------------------------------------
		public void GetXMLFilesInPath(DirectoryInfo standardDir, DirectoryInfo customDir, ref ArrayList fileList)
		{
			if (fileList == null)
				return;

			//prima considero i file nella directory di custom del modulo
			//(sia tablename.xml che appendtablename.xml)
			ArrayList tempList = new ArrayList();
			if (customDir.Exists)
			{
				foreach (FileInfo file in customDir.GetFiles())
				{
					if (string.Compare(file.Extension, NameSolverStrings.XmlExtension, true, CultureInfo.InvariantCulture) == 0)
						tempList.Add(file);
				}
			}

			if (standardDir.Exists)
			{
				foreach (FileInfo file in standardDir.GetFiles())
				{
					bool insert = true;
					if (string.Compare(file.Extension, NameSolverStrings.XmlExtension, true, CultureInfo.InvariantCulture) != 0)
						continue;
					if (tempList.Count > 0)
					{
						foreach (FileInfo fileName in tempList)
						{
							// é stato inserito giá quello presente nella custom
							if (string.Compare(fileName.Name, file.Name, true, CultureInfo.InvariantCulture) == 0)
							{
								insert = false;
								break;
							}
						}
					}
					if (insert)
						tempList.Add(file);
				}
			}

			foreach (FileInfo file in tempList)
				fileList.Add(file);
		}

		/// <summary>
		/// Ritorna il path del file Custom\LoginMngSessionFile
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetLoginMngSessionFile()
		{
			string customPath = GetCustomPath();
			if (customPath == string.Empty)
				return string.Empty;

			return Path.Combine(customPath, NameSolverStrings.LoginMngSessionFile);
		}

		/// <summary>
		/// Cerca la prima immagine nella directory di applicazione col nome dato
		/// </summary>
		/// <param name="imageFile">Nome del file da trovare</param>
		/// <returns>Percorso completo del file trovato, oppure null se il file non esiste</returns>
		//-----------------------------------------------------------------------------
		public string SearchImageInAppFolder(string imageFile)
		{
			string appsPaths = Path.Combine(GetStandardPath(), NameSolverStrings.TaskBuilderApplications);
			if (Directory.Exists(appsPaths))
			{
				foreach (string appPath in Directory.GetDirectories(appsPaths))
				{
					if (!IsApplicationDirectory(appPath))
						continue;

					string fullImageFilePath = Path.Combine(appPath, imageFile);
					if (File.Exists(fullImageFilePath))
						return fullImageFilePath;
				}
			}

			return null;
		}

		#endregion

		#region funzioni custom dipendenti da company

		/// <summary>
		/// Ritorna il path nella Custom della company desiderata
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetCustomCompanyPath(string companyName)
		{
			if (string.IsNullOrEmpty(companyName))
				return string.Empty;

			return
				GetCustomPath() +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Subscription +
                Path.DirectorySeparatorChar +
                companyName;
		}

		/// <summary>
		/// Ritorna il path nella Custom dell'application desiderata
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetCustomApplicationContainerPath(string companyName, ApplicationType aApplicationType)
		{
			if (aApplicationType == ApplicationType.All ||
                aApplicationType == ApplicationType.Undefined)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
				return string.Empty;
			}

            string customCompanyPath = GetCustomCompanyPath(companyName);

            if (aApplicationType == ApplicationType.Customization)
                customCompanyPath = GetEasyStudioCustomizationsPath();

            string applicationContainerName = GetApplicationContainerName(aApplicationType);
			if (applicationContainerName == string.Empty)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
				return string.Empty;
			}

			return
				customCompanyPath +
                Path.DirectorySeparatorChar +
                applicationContainerName;
		}

		///<summary>
		/// Ritorna il path \Custom\Companies\"companyName"\DataTransfer
		/// il parametro "companyName" può ricevere un nome azienda oppure AllCompanies
		///</summary>
		//---------------------------------------------------------------------
		public string GetCustomCompanyDataTransferPath(string companyName)
		{
			return
				GetCustomCompanyPath(companyName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.DataTransfer;
		}

		///<summary>
		/// Ritorna il path \Custom\Companies\"companyName"\Log
		/// il parametro "companyName" può ricevere un nome azienda oppure AllCompanies
		///</summary>
		//---------------------------------------------------------------------
		public string GetCustomCompanyLogPath(string companyName)
		{
			return
				GetCustomCompanyPath(companyName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Log;
		}

		///<summary>
		/// Ritorna il path \Custom\DebugSymbols
		///</summary>
		//---------------------------------------------------------------------
		public string GetCustomDebugSymbolsPath()
		{
			return Path.Combine(GetCustomPath(), NameSolverStrings.DebugSymbols);
		}
		///<summary>
		/// Ritorna il path \Custom\Companies\"companyName"\Log\"userName"
		/// il parametro "companyName" può ricevere un nome azienda oppure AllCompanies
		/// il parametro "userName" può ricevere un nome utente oppure AllUsers
		///</summary>
		//---------------------------------------------------------------------
		public string GetCustomCompanyLogPath(string companyName, string userName)
		{
			if (string.Compare(userName, NameSolverStrings.AllUsers, true, CultureInfo.InvariantCulture) == 0)
				return
					GetCustomCompanyPath(companyName) +
                    Path.DirectorySeparatorChar +
                    NameSolverStrings.Log +
                    Path.DirectorySeparatorChar +
                    userName;

			return
				GetCustomCompanyPath(companyName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Log +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Users +
                Path.DirectorySeparatorChar +
                userName;
		}

		/// <summary>
		/// data una company restituisce il path custom\companies\company\DataTransfer\Backup dell'image attuale 
		/// </summary>
		//---------------------------------------------------------------------
		public string GetCustomCompanyDataTransferBackupPath(string companyName)
		{
			return
				GetCustomCompanyDataTransferPath(companyName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Backup;
		}

		/// <summary>
		/// data una company restituisce il patch custom\companies\company\DataTransfer\DataManager dell'image attuale
		/// </summary>
		//---------------------------------------------------------------------
		public string GetCustomCompanyDataManagerPath(string companyName)
		{
			return
				GetCustomCompanyDataTransferPath(companyName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.DataManager;
		}

		/// <summary>
		/// Restituisce il path dell'applicazione nell'istanza custom
		/// </summary>
		/// <param name="applicationName">nome dell'applicazione</param>
		/// <returns>path dell'applicazione</returns>
		//---------------------------------------------------------------------
		public string GetCustomApplicationPath(string companyName, string applicationName)
		{
			if (companyName == null || companyName == string.Empty || applicationName == null || applicationName == string.Empty)
				return string.Empty;

			IBaseApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);
			if (aApplicationInfo == null)
				return string.Empty;

			string customApplicationContainer = GetCustomApplicationContainerPath(companyName, aApplicationInfo.ApplicationType);

			if (customApplicationContainer == string.Empty)
				return string.Empty;

			return
				customApplicationContainer +
                Path.DirectorySeparatorChar +
                aApplicationInfo.Name;
		}

		/// <summary>
		/// Restituisce il path dell'applicazione nell'istanza custom nonostante non esista una corrispondente in Standard 
		/// </summary>
		//---------------------------------------------------------------------
		public string GetAnyCustomApplicationPath(string companyName, string applicationName)
		{
			if (companyName == null || companyName == string.Empty || applicationName == null || applicationName == string.Empty)
				return string.Empty;

			string customApplicationContainer = GetCustomApplicationContainerPath(companyName, ApplicationType.TaskBuilderApplication);

			if (customApplicationContainer == string.Empty)
				return string.Empty;

			return
				customApplicationContainer +
                Path.DirectorySeparatorChar +
                applicationName;
		}

		/// <summary>
		/// Restituisce la path custom di un modulo
		/// </summary>
		/// <param name="applicationName">nome applicazione</param>
		/// <param name="moduleName">nome modulo</param>
		/// <returns>la path del modulo</returns>
		//---------------------------------------------------------------------
		public string GetCustomModulePath(string companyName, string applicationName, string moduleName)
		{
			if (
				companyName == null || companyName == string.Empty ||
                applicationName == null || applicationName == string.Empty ||
                moduleName == null || moduleName == string.Empty
				)
				return string.Empty;

			string customApplicationPath = GetCustomApplicationPath(companyName, applicationName);
			if (customApplicationPath == string.Empty)
				return string.Empty;

			return customApplicationPath + Path.DirectorySeparatorChar + moduleName;
		}

		/// <summary>
		/// Ritorna la cartella Report nella custom data application, module e company
		/// </summary>
		//---------------------------------------------------------------------
		public string GetCustomReportPath(string companyName, string applicationName, string moduleName)
		{
			if (
				companyName == null || companyName == string.Empty ||
                applicationName == null || applicationName == string.Empty ||
                moduleName == null || moduleName == string.Empty
				)
				return string.Empty;

			string customModulePath = GetCustomModulePath(companyName, applicationName, moduleName);
			if (customModulePath == string.Empty)
				return string.Empty;

			return Path.Combine(customModulePath, NameSolverStrings.Report);
		}

		/// <summary>
		/// Restituisce Custom\Contenitore\Applicazione\Modulo\Dictionary
		/// </summary>
		/// <param name="appName">nome dell'applicazione</param>
		/// <param name="moduleName">nome del modulo</param>
		/// <param name="createDir">indica se creare la cartella se non presente</param>
		/// <returns>la pth richiesta</returns>
		//---------------------------------------------------------------------
		public string GetCustomModuleDictionaryPath(string companyName, string applicationName, string moduleName, bool createDir)
		{
			if (
				companyName == null || companyName == string.Empty ||
                applicationName == null || applicationName == string.Empty ||
                moduleName == null || moduleName == string.Empty
				)
				return string.Empty;

			string modulePath = GetCustomModulePath(companyName, applicationName, moduleName);
			if (modulePath == string.Empty)
				return string.Empty;

			modulePath += Path.DirectorySeparatorChar;

			if (!Directory.Exists(modulePath) && createDir)
				Directory.CreateDirectory(modulePath);

			string dictionaryPath = modulePath + NameSolverStrings.Dictionary;

			if (createDir && !Directory.Exists(dictionaryPath))
				Directory.CreateDirectory(dictionaryPath);

			return dictionaryPath;
		}

		/// <summary>
		/// Ritorna la cartella DataManager nella custom data la company desiderata
		/// </summary>
		//---------------------------------------------------------------------
		public string GetCustomDataManagerPath(string companyName, string applicationName, string moduleName)
		{
			if (
				companyName == null || companyName == string.Empty ||
                applicationName == null || applicationName == string.Empty ||
                moduleName == null || moduleName == string.Empty
				)
				return string.Empty;

			return
				GetCustomModulePath(companyName, applicationName, moduleName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.DataManager;
		}

		/// <summary>
		/// Custom\AllCompanies\TaskBuilderApplication\Application\Module\DataManager\Default\[language]\[Edition]
		/// </summary>
		//---------------------------------------------------------------------
		public string GetCustomDataManagerDefaultPath(string companyName, string applicationName, string moduleName, string language, string edition)
		{
			if (
				companyName == null || companyName == string.Empty ||
                applicationName == null || applicationName == string.Empty ||
                moduleName == null || moduleName == string.Empty
				)
				return string.Empty;

			return
				GetCustomDataManagerPath(companyName, applicationName, moduleName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Default +
                Path.DirectorySeparatorChar +
                language +
                Path.DirectorySeparatorChar +
                edition;

		}

		/// <summary>
		/// Custom\AllCompanies\TaskBuilderApplication\Application\Module\DataManager\Sample\[language]\[Edition]
		/// </summary>
		//---------------------------------------------------------------------
		public string GetCustomDataManagerSamplePath(string companyName, string applicationName, string moduleName, string language, string edition)
		{
			if (
				companyName == null || companyName == string.Empty ||
                applicationName == null || applicationName == string.Empty ||
                moduleName == null || moduleName == string.Empty
				)
				return string.Empty;

			return
				GetCustomDataManagerPath(companyName, applicationName, moduleName) +
                Path.DirectorySeparatorChar +
                NameSolverStrings.Sample +
                Path.DirectorySeparatorChar +
                language +
                Path.DirectorySeparatorChar +
                edition;
		}

		//TODO deve diventare GetCustomApplicationPath
		//---------------------------------------------------------------------
		public string GetCustomAppContainerPath(string companyName, INameSpace aNameSpace)
		{
			if (
				companyName == null ||
                companyName == string.Empty ||
                aNameSpace.Application == null ||
                aNameSpace.Application == string.Empty
				)
				return null;

			return GetCustomApplicationPath(companyName, aNameSpace.Application);
		}

		#endregion

		#region funzioni custom dipendenti da company e user

		/// <summary>
		/// Ritorna il path nella custom della cartella Report dato un woormFilePath
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomReportPathFromWoormFile(string woormFilePath, string companyName, string userName)
		{
			bool isStandard = IsStandardPath(woormFilePath);
			string[] ar = woormFilePath.Split(new char[] { '\\', '/' });
			int ub = ar.GetUpperBound(0);
			bool isAllUsers = !isStandard && string.Compare("AllUsers", ar[ub - 1], true, CultureInfo.InvariantCulture) == 0;
			string moduleName, appName;

			if (isStandard)
			{
				moduleName = ar[ub - 2];
				appName = ar[ub - 3];
			}
			else if (isAllUsers)
			{
				moduleName = ar[ub - 3];
				appName = ar[ub - 4];
			}
			else
			{
				moduleName = ar[ub - 4];
				appName = ar[ub - 5];
			}

			return Path.Combine(GetCustomReportPath(companyName, appName, moduleName), GetUserPath(userName));
		}

		/// <summary>
		/// Ritorna il path  dello user desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public static string GetUserPath(string userName)
		{
			if (userName == null || userName == string.Empty)
				return string.Empty;

			if (string.Compare(userName, NameSolverStrings.AllUsers, true, CultureInfo.InvariantCulture) == 0)
				return userName;

			string correctUserName = userName.Replace(Path.DirectorySeparatorChar, '.');

			return
				NameSolverStrings.Users +
                System.IO.Path.DirectorySeparatorChar +
                correctUserName;
		}

		/// <summary>
		/// Dato lo username ritorna \Users\[userName]
		/// </summary>
		//---------------------------------------------------------------------------------
		public static string GetUserUri(string userName)
		{
			if (userName == null || userName == string.Empty || string.Compare(userName, NameSolverStrings.Standard, true, CultureInfo.InvariantCulture) == 0)
				return string.Empty;

			if (string.Compare(userName, NameSolverStrings.AllUsers, true, CultureInfo.InvariantCulture) == 0)
				return userName + "/";

			return NameSolverStrings.Users + "/" + userName + "/";
		}

		/// <summary>
		/// Ritorna il path nella custom della cartella Report dato il namespace
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomReportPathFromNamespace(INameSpace ns, string companyName, string userName)
		{
			if (ns.NameSpaceType.Type != NameSpaceObjectType.Report)
				return String.Empty;

			string p = GetCustomReportPath(companyName, ns.Application, ns.Module);

			return Path.Combine(p, GetUserPath(userName));
		}

		/// <summary>
		/// Ritorna il fullname del Report nella custom dato il namespace
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomReportFullNameFromNamespace(INameSpace ns, string companyName, string userName)
		{
			if (ns.NameSpaceType.Type != NameSpaceObjectType.Report)
				return String.Empty;

			string p = GetCustomReportPath(companyName, ns.Application, ns.Module);

			return Path.Combine(p, GetUserPath(userName), ns.Report);
		}

		/// <summary>
		/// Ritorna il fullname del Report nella standard dato il namespace
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetStandardReportFullNameFromNamespace(INameSpace ns)
		{
			if (ns.NameSpaceType.Type != NameSpaceObjectType.Report)
				return String.Empty;

			string p = GetStandardReportPath(ns);

			return Path.Combine(p, ns.Report);
		}

		/// <summary>
		/// Ritorna il path nella custom della cartella report per user
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomUserReportPath(string companyName, string userName, string applicationName, string moduleName)
		{
			if (
				companyName == null || companyName == string.Empty ||
                userName == null || userName == string.Empty ||
                applicationName == null || applicationName == string.Empty ||
                moduleName == null || moduleName == string.Empty
				)
				return string.Empty;

			string reportPath = GetCustomReportPath(companyName, applicationName, moduleName);
			if (reportPath == string.Empty)
				return string.Empty;

			return Path.Combine(reportPath, GetUserPath(userName));
		}

		/// <summary>
		/// Ritorna il path nella custom della cartella report per user dato il namespace del report desiderato
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomUserReportFile(string companyName, string userName, INameSpace ns, bool recursive)
		{
			if (
				companyName == null || companyName == string.Empty ||
                userName == null || userName == string.Empty ||
                !ns.IsValid()
				)
				return string.Empty;

			string repName = ns.Report + NameSolverStrings.WrmExtension;
			string reportPath = string.Empty;
			string reportFile = string.Empty;

			if (string.Compare(userName, NameSolverStrings.Standard, true, CultureInfo.InvariantCulture) == 0)
			{
				reportPath = GetStandardReportPath(ns);

				if (reportPath == string.Empty)
					return string.Empty;

				reportFile = Path.Combine(reportPath, repName);
				return reportFile;
			}

			reportPath = GetCustomUserReportPath(companyName, userName, ns.Application, ns.Module);
			if (reportPath == string.Empty)
				return string.Empty;

			reportFile = Path.Combine(reportPath, repName);
			if (!recursive || File.Exists(reportFile))
				return reportFile;

			reportPath = GetCustomUserReportPath(companyName, NameSolverStrings.AllUsers, ns.Application, ns.Module);
			if (reportPath == string.Empty)
				return string.Empty;

			reportFile = Path.Combine(reportPath, repName);
			if (!recursive || File.Exists(reportFile))
				return reportFile;

			reportPath = GetStandardReportPath(ns);
			if (reportPath == string.Empty)
				return string.Empty;

			reportFile = Path.Combine(reportPath, repName);
			return reportFile;
		}


		/// <summary>
		/// TODO_MARCO da rifare 
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetUserCustomReports(string applicationPath, INameSpace aNameSpace, string userName)
		{
			if (applicationPath == null || applicationPath == string.Empty || userName == null || userName == string.Empty)
				return string.Empty;

			string path = Path.Combine(GetDescriptionFolder(applicationPath, aNameSpace), GetUserPath(userName));
			return Path.Combine(path, NameSolverStrings.ReportXml);
		}

        /// <summary>
        /// Ritorna il path completo del file DefaultTheme.config che gestisce le
        /// impostazioni di tema per l'applicazione dalla cartella Standard
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetDefaultThemeFilePath()
        {
            //C:\Development_MZP\Standard\TaskBuilder\Framework\TbGeneric\Settings\DefaultTheme.config
            return Path.Combine(
                GetApplicationModulePath(NameSolverStrings.Framework, "TbGeneric"),
                NameSolverStrings.Settings,
                defaultThemeFileName
                );
        }

        /// <summary>
        /// Ritorna il path completo del file DefaultTheme.config che gestisce le
        /// impostazioni di tema per l'applicazione per AllCompanies e AllUsers
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetAllCompaniesAllUsersDefaultThemeFilePath()
        {
            //C:\MBOOK\Custom\Companies\AllCompanies\TaskBuilder\Framework\TbGeneric\Settings\AllUsers
            return Path.Combine(
                GetCustomAllCompaniesModulePath(NameSolverStrings.Framework, "TbGeneric"),
                NameSolverStrings.Settings,
                NameSolverStrings.AllUsers,
                defaultThemeFileName
                );
        }

		/// <summary>
		/// TODO_MARCO da rifare 
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetAllUserCustomReports(string applicationPath, INameSpace aNameSpace)
		{
			return GetUserCustomReports(applicationPath, aNameSpace, NameSolverStrings.AllUsers);
		}

		/// <summary>
		/// Ritorna il namespace dato il fullfile path di un oggetto
		/// </summary>
		//---------------------------------------------------------------------------------
		public INameSpace GetNamespaceFromPath(string sObjectFullPath)
		{
			try
			{
				// TODOBRUNA 
				if (sObjectFullPath == null || sObjectFullPath.Length == 0)
					return null;				
				
				// potrebbe arrivare un filename con sintassi Unix cioè anche con "/"
				sObjectFullPath = sObjectFullPath.Replace('/', Path.DirectorySeparatorChar);

				FileInfo sObjectFullPathInfo = new FileInfo(sObjectFullPath);

				// prima controllo le cose di base
				if (sObjectFullPathInfo.Name.Length == 0)
					return null;

				// poi la consistenza dei token di path
				string[] tokens = sObjectFullPath.Split(System.IO.Path.DirectorySeparatorChar);

				int nPathToken = tokens.Length;
				// il minimo che posso rappresentare è applicazione e modulo, quindi
				// con il tipo devo avere almeno tre segmenti di path per poterlo fare 
				if (nPathToken < 8)
					return null;

				string module;
				string application;

				bool isStandard = IsStandardPath(sObjectFullPath);
                if (isStandard)
                {
                    // di default parto con il namespace di modulo
                    application = tokens[StandardAppSegmentPath];
                    module = tokens[StandardModuleSegmentPath];
                }
                else
                {
                    int nPosCompaniesPos = -1;
                    for (int i = 0; i <= tokens.Length; i++)
                    {

                        if (string.Compare(NameSolverStrings.Subscription, tokens[i], true) == 0)
                        {
                            nPosCompaniesPos = i;
                            break;
                        }
                    }

                    if (nPosCompaniesPos > 0)
                    {
                        // sono nella custom, devo scendere di un ulteriore livello
                        application = tokens[nPosCompaniesPos + 3];
                        module = tokens[nPosCompaniesPos + 4];
                    }
                    else
                    { 
                        // sono nella custom, devo scendere di un ulteriore livello
                        application = tokens[CustomAppSegmentPath];
                        module = tokens[CustomModuleSegmentPath];
                    }
				}

				string nameSpaceType = string.Empty;
				string nameSpaceSuffix = "." + application + "." + module + "." + sObjectFullPathInfo.Name;

				DirectoryInfo dirInfo = new DirectoryInfo(sObjectFullPathInfo.DirectoryName);
				string dirName = dirInfo.Name;

				// devo aggiungere il tipo di oggetto al namespace.
				// nella custom ho un livello in più e posso avere:
				//		AllUsers/....
				// oppure: 
				//		User/[UserName]/.....
				//
				if (!isStandard)
				{
					dirName = (string.Compare(dirName, NameSolverStrings.AllUsers, true, CultureInfo.InvariantCulture) == 0)
                        ? dirInfo.Parent.Name
                        : dirInfo.Parent.Parent.Name;
				}

				// da qui controllo il tipo di oggetto sulla base del nome della directory che lo contiene
				if (string.Compare(dirName, NameSolverStrings.Report, true, CultureInfo.InvariantCulture) == 0) nameSpaceType = NameSolverStrings.Report;
				else if (string.Compare(dirName, NameSolverStrings.Others, true, CultureInfo.InvariantCulture) == 0) nameSpaceType = NameSolverStrings.File;
				else if (string.Compare(dirName, NameSolverStrings.Images, true, CultureInfo.InvariantCulture) == 0) nameSpaceType = NameSolverStrings.Image;
				else if (string.Compare(dirName, NameSolverStrings.Texts, true, CultureInfo.InvariantCulture) == 0) nameSpaceType = NameSolverStrings.Text;
				else if (string.Compare(dirName, NameSolverStrings.ReferenceObjects, true, CultureInfo.InvariantCulture) == 0)
				{
					nameSpaceType = NameSpaceSegment.Hotlink;
					nameSpaceSuffix = nameSpaceSuffix.Substring(0, nameSpaceSuffix.LastIndexOf("."));
				}
                else if 
                    (
                        !isStandard &&
                        Path.GetExtension(sObjectFullPathInfo.Name).CompareNoCase(NameSolverStrings.DllExtension) &&
                        sObjectFullPath.IndexOf(NameSolverStrings.AllCompanies) >= 0 &&
                        sObjectFullPath.IndexOf(NameSolverStrings.ModuleObjects) >= 0
                    )
                {                            
                        // customizzazione di EasyStudio
                        nameSpaceType = NameSpaceSegment.Customization;
                }
                else
					if (nameSpaceType == string.Empty)
						return null;

				return new NameSpace(nameSpaceType + nameSpaceSuffix);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Ritorna un namespace (composto da applicazione + modulo) dato il path di un file
		/// </summary>
		/// <param name="sObjectFullPath">path del file</param>
		/// <returns>Namespace</returns>
		//-------------------------------------------------------------------------------------
		public INameSpace GetAppModNSFromFilePath(string sObjectFullPath)
		{
			// potrebbe arrivare un filename con sintassi Unix cioè anche con "/"
			sObjectFullPath = sObjectFullPath.Replace('/', Path.DirectorySeparatorChar);

			// TODOBRUNA 
			if (sObjectFullPath == null || sObjectFullPath.Length == 0)
				return null;

			FileInfo sObjectFullPathInfo = new FileInfo(sObjectFullPath);

			// prima controllo le cose di base
			if (sObjectFullPathInfo.Name.Length == 0)
				return null;

			// poi la consistenza dei token di path
			string[] tokens = sObjectFullPath.Split(System.IO.Path.DirectorySeparatorChar);

			int nPathToken = tokens.Length;
			// il minimo che posso rappresentare è applicazione e modulo, quindi
			// con il tipo devo avere almeno tre segmenti di path per poterlo fare 
			if (nPathToken <= 3)
				return null;

			string application = IsStandardPath(sObjectFullPath) ? tokens[StandardAppSegmentPath] : tokens[CustomAppSegmentPath];
			string module = IsStandardPath(sObjectFullPath) ? tokens[StandardModuleSegmentPath] : tokens[CustomModuleSegmentPath];

			return new NameSpace(NameSolverStrings.Module + "." + application + "." + module);
		}

		/// <summary>
		/// Ritorna il fullpath della cartella Excel nella custom
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomExcelPath(string companyName, string applicationName, string moduleName)
		{
			if (
				companyName == null ||
                companyName == String.Empty ||
                applicationName == null ||
                applicationName == String.Empty ||
                moduleName == null ||
                moduleName == String.Empty
				)
				return String.Empty;

			string customModulePath = GetCustomModulePath(companyName, applicationName, moduleName);
			if (customModulePath == null || customModulePath == String.Empty)
				return String.Empty;

			return Path.Combine(customModulePath, NameSolverStrings.Excel);
		}

		/// <summary>
		/// Ritorna il path della cartella Excel nella custom dato un namespace
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomExcelPathFromNamespace(INameSpace ns, string companyName, string userName)
		{
			if
				(
				ns.NameSpaceType.Type != NameSpaceObjectType.ExcelDocument &&
                ns.NameSpaceType.Type != NameSpaceObjectType.ExcelTemplate
				)
				return String.Empty;

			string customExcelPath = GetCustomExcelPath(companyName, ns.Application, ns.Module);
			if (customExcelPath == null || customExcelPath == String.Empty)
				return String.Empty;

			return Path.Combine(customExcelPath, GetUserPath(userName));
		}

		/// <summary>
		/// Ritorna il path della cartella Word 
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomWordPath(string companyName, string applicationName, string moduleName)
		{
			if (
				companyName == null ||
                companyName == String.Empty ||
                applicationName == null ||
                applicationName == String.Empty ||
                moduleName == null ||
                moduleName == String.Empty
				)
				return String.Empty;

			string customModulePath = GetCustomModulePath(companyName, applicationName, moduleName);
			if (customModulePath == null || customModulePath == String.Empty)
				return String.Empty;

			return Path.Combine(customModulePath, NameSolverStrings.Word);
		}

		/// <summary>
		/// Ritorna il path della cartella Word nella custom dato un namespace
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomWordPathFromNamespace(INameSpace ns, string companyName, string userName)
		{
			if
				(
				ns.NameSpaceType.Type != NameSpaceObjectType.WordDocument &&
                ns.NameSpaceType.Type != NameSpaceObjectType.WordTemplate
				)
				return String.Empty;

			string customWordPath = GetCustomWordPath(companyName, ns.Application, ns.Module);
			if (customWordPath == null || customWordPath == String.Empty)
				return String.Empty;

			return Path.Combine(customWordPath, GetUserPath(userName));
		}

		/// <summary>
		/// Ritorna il path della cartella Text nella custom
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetCustomModuleTextPath(string companyName, string userName, string appName, string modName)
		{
			string fileDir = Path.Combine(GetCustomModulePath(companyName, appName, modName), NameSolverStrings.Files);

			return Path.Combine(Path.Combine(fileDir, NameSolverStrings.Texts), userName);
		}

		#endregion

		#region funzioni pubbliche
		/// <summary>
		/// Ritorna il path della cartella Solutions\modules dato il nome del prodotto
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetSolutionsModulesPath(string product)
		{
			string solutionsPath;
			string solFile = GetSolutionFile(product);
			if (solFile == null || solFile.Length == 0)
				return string.Empty;
			solutionsPath = Path.GetDirectoryName(solFile);
			return Path.Combine(solutionsPath, NameSolverStrings.Modules);
		}

		/// <summary>
		/// Ritorna il path del file App_Data\Proxies.xml
		/// </summary>
		//---------------------------------------------------------------------------------
		public string GetProxiesFilePath()
		{
			return Path.Combine(GetLogManAppDataPath(), NameSolverStrings.ProxiesXml);
		}

		//---------------------------------------------------------------------------------
		public string GetAppDataPath(bool create)
		{
			//su alcuni server (2008, 2003) potrebbero non esserci i diritti di scrittura sulle cartelle usate nel commento
			//prima provo nella local App Data, se girassi in IIS allora questa potrebbe essere vuota,
			//allora uso la Common App Data
			/*string s = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (string.IsNullOrEmpty(s))
				s = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);*/
			string s = Path.GetTempPath();
			s = Path.Combine(s,
				string.Format("TBTemp\\{0}\\{1}\\{2}",
				RemoteFileServer,
				Installation,
				Process.GetCurrentProcess().ProcessName));
			if (create && !Directory.Exists(s))
				Directory.CreateDirectory(s);
			return s;
		}


		//--------------------------------------------------------------------------------
		public string GetMenuThumbnailsFolderPath(bool create)
		{ 
			string s = Path.Combine(
				GetAppDataPath(false),
				NameSolverStrings.Thumbnails
				);
			
			if (create && !Directory.Exists(s))
				Directory.CreateDirectory(s);

			return s;
		}

		//--------------------------------------------------------------------------------
		public string GetCustomApplicationsPath()
		{
			return Path.Combine(GetEasyStudioCustomizationsPath(), NameSolverStrings.Applications);
		}

		/// <summary>
		/// Dato un namespace di una customizzazione torna la folder in cui è contenuta
		/// es: Customization.ERP.Shippings.Documents.Ports.Prova 
		/// torna
		/// C:\Development\Custom\Companies\AllCompanies\Applications\ERP\Shippings\ModuleObjects\Ports\
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetCustomizationPath(INameSpace documentNamespace, string user, IEasyBuilderApp easybuilderApp)
		{
			if (!user.IsNullOrEmpty())
				user = user.Replace("\\", ".");

			string path = GetCustomDocumentPath(easyStudioConfiguration.Settings.HomeName, easybuilderApp.ApplicationName, easybuilderApp.ModuleName, documentNamespace.Document);
			return string.IsNullOrEmpty(user)
				? path
				: Path.Combine(path, user);
		}

        //--------------------------------------------------------------------------------
        public string GetApplicationPath(INameSpace documentNamespace)
        {
            return GetApplicationDocumentPath(documentNamespace.Application, documentNamespace.Module, documentNamespace.Document);
        }
        /// <summary>
        /// Dato un namespace di una standardizzazione torna la folder in cui è contenuta se è nella cartella: Standard altrimenti stringa vuota
        /// </summary>
        /// <returns></returns>
        //--------------------------------------------------------------------------------
        public string GetStandardizationPath(INameSpace documentNamespace)
        {
            return GetStandardDocumentPath(documentNamespace.Application, documentNamespace.Module, documentNamespace.Document);
        }

		/// <summary>
		/// Loads all DLLs containing customizations for the document identified by the
		/// given document namespace.
		/// </summary>
		/// <param name="documentNamespace">The namespace of the document to customize.</param>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.INameSpace"/>
		/// <remarks>
		/// At first all DLLs from the common folder are loaded.
		/// On the second hand they are overloaded by the DLLs present for any specific user.
		/// </remarks>
		//--------------------------------------------------------------------------------
		public void GetEasyBuilderAppAssembliesPaths(Dictionary<string, string> fileMap, INameSpace documentNamespace, string user, IEasyBuilderApp app)
		{
			if (app == null)
				return;

            string standardAllUsersPath = BasePathFinder.BasePathFinderInstance.GetStandardizationPath(documentNamespace);
			string customAllUsersPath = BasePathFinder.BasePathFinderInstance.GetCustomizationPath(documentNamespace, null, app);
			string customUsersPath = BasePathFinder.BasePathFinderInstance.GetCustomizationPath(documentNamespace, user, app);

			if (String.Compare(standardAllUsersPath, customAllUsersPath, StringComparison.OrdinalIgnoreCase) == 0)
			{
				standardAllUsersPath = String.Empty;
			}
         
            if (Directory.Exists(customAllUsersPath))
			{
                foreach (string file in Directory.GetFiles(customAllUsersPath, "*.dll"))
                {
                    if (IsSameDocument(file, documentNamespace, app))
                        fileMap[String.Concat(NameSolverStrings.Custom, file)] = file;
                }
			}
			//poi sovrascrivo quelle dell'utente con lo stesso nome
			if (Directory.Exists(customUsersPath))
			{
                foreach (string file in Directory.GetFiles(customUsersPath, "*.dll"))
                {
                    if (!IsSameDocument(file, documentNamespace, app))
                        continue;
                    string allCompaniesPath = String.Concat(NameSolverStrings.Custom, Path.Combine(customAllUsersPath, Path.GetFileName(file)));
                    if (fileMap.ContainsKey(allCompaniesPath))
                        fileMap.Remove(allCompaniesPath);

                    fileMap[String.Concat(NameSolverStrings.Custom, file)] = file;
                }
			}
		}

        //--------------------------------------------------------------------------------
        public bool IsSameDocument(string fileName, INameSpace documentNamespace, IEasyBuilderApp app)
        {
            ICustomListItem item = app.EasyBuilderAppFileListManager.CustomList.FindItem(fileName);
            return item == null || string.IsNullOrEmpty(item.DocumentNamespace) || string.Compare(item.DocumentNamespace, documentNamespace.FullNameSpace, true) == 0;
        }

        /// <summary>
        /// Loads all DLLs containing customizations for the document identified by the
        /// given document namespace.
        /// </summary>
        /// <param name="documentNamespace">The namespace of the document to customize.</param>
        /// <seealso cref="Microarea.TaskBuilderNet.Interfaces.INameSpace"/>
        /// <remarks>
        /// At first all DLLs from the common folder are loaded.
        /// On the second hand they are overloaded by the DLLs present for any specific user.
        /// </remarks>
            //--------------------------------------------------------------------------------
        public List<string> GetEasyBuilderAppAssembliesPaths(INameSpace documentNamespace, string user, IList<IEasyBuilderApp> easybuilderApps)
		{
			if (easybuilderApps == null)
				return null;

            Dictionary<string, string> fileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (IEasyBuilderApp app in easybuilderApps)
			{
                GetEasyBuilderAppAssembliesPaths(fileMap, documentNamespace, user, app);
			}
			return fileMap.Values.ToList();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Calculate the path of the EasyBuilder module dll
		/// </summary>
		/// <returns></returns>
		public string GetEBModuleDllPath(string applicationName, string moduleName)
		{
			string path = GetApplicationModuleObjectsPath(applicationName, moduleName);
			//C:\Development\Standard\Applications\WIZARDDEMO\NuovoModulo1\ModuleObjects
			//C:\Development\Custom\Companies\AllCompanies\Applications\WIZARDDEMO\NuovoModulo1\ModuleObjects
			return Path.Combine(path, String.Concat(applicationName, '.', moduleName, ".dll"));
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Dato il namespace di un oggetto json, ritorna il percorso dove verrà salvato il file json
		/// </summary>
		/// <param name="nameSpace"></param>
		/// <returns></returns>
		public string GetJsonFormPath(INameSpace ns)
		{
			if (ns == null || !ns.IsValid())
				return "";
			if (ns.NameSpaceType.Type == NameSpaceObjectType.Document)
				return Path.Combine(GetDocumentPath(ns), NameSolverStrings.JsonForms);
			if (ns.NameSpaceType.Type == NameSpaceObjectType.Module)
			{
				IBaseModuleInfo bmi = GetModuleInfo(ns);
				return bmi == null ? "" : Path.Combine(bmi.Path, NameSolverStrings.JsonForms);
			}
			return "";
		}
	
		//--------------------------------------------------------------------------------
		public static string[] GetCSharpFilesIn(string sourcesPath)
		{
			if (!Directory.Exists(sourcesPath))
				return null;

			string searchPattern = string.Format("*{0}", NameSolverStrings.CSharpExtension);

			return Directory.GetFiles(sourcesPath, searchPattern);
		}

		//--------------------------------------------------------------------------------
		public static string ChangeExtension(string fileName, string extension)
		{
			return string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(fileName), extension);
		}

        public static string GetSourcesFolderNameFromDll(string dllPath)
        {
            // from:	...\ProspectiveSuppliers\sa\ProspectiveSuppliers.dll
            // to :		...\ProspectiveSuppliers\sa\ProspectiveSuppliers_src\designer
            var dllFileNameWOExtension = Path.GetFileNameWithoutExtension(dllPath);
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, NameSolverStrings.SourcesFolderNameMask, dllFileNameWOExtension);
        }

        //--------------------------------------------------------------------------------
        public static string GetDesignerSourcesPathFromDll(string dllPath)
		{
            // from:	...\ProspectiveSuppliers\sa\ProspectiveSuppliers.dll
            // to :		...\ProspectiveSuppliers\sa\ProspectiveSuppliers_src\designer

            return Path.Combine(Path.GetDirectoryName(dllPath), GetSourcesFolderNameFromDll(dllPath), NameSolverStrings.DesignerFolderName);
		}

        //--------------------------------------------------------------------------------
        public static string GetResSourcesPathFromDll(string dllPath)
        {
            // from:	...\ProspectiveSuppliers\sa\ProspectiveSuppliers.dll
            // to :		...\ProspectiveSuppliers\sa\ProspectiveSuppliers_src\res

            return Path.Combine(Path.GetDirectoryName(dllPath), GetSourcesFolderNameFromDll(dllPath));
        }

        //--------------------------------------------------------------------------------
        public static string GetSourcesFolderPathFromDll(string dllPath)
        {
            return Path.Combine(Path.GetDirectoryName(dllPath), GetSourcesFolderNameFromDll(dllPath));
        }

        //--------------------------------------------------------------------------------
        private Enums enums;
		/// <summary>
		/// Enumerativi caricati da un' applicazione TaskBuilder.net
		/// </summary>
		public Enums Enums
		{
			get
			{
				if (this.enums == null)
				{
					this.enums = new Enums();
					this.enums.LoadXml();
				}

				return this.enums;
			}
		}

        #endregion


    }
}

