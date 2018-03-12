using Microarea.Common.Applications;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.FileSystemManager;
using Microarea.Common.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using TaskBuilderNetCore.Interfaces;
using static Microarea.Common.Generic.InstallationInfo;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using Microarea.Common.WebServicesWrapper;
using System.Text;
using Newtonsoft.Json;
using System.Xml;

namespace Microarea.Common.NameSolver
{
    public enum ImageSize { None, Size16x16, Size20x20, Size24x24, Size32x32 }
    //=========================================================================
    public class PathFinder
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
        #endregion
        
        bool easyStudioAppsInCustom = true;

        #region membri privati
        private static readonly object staticLockTicket = new object();
        private static readonly object staticLockTicketFunctions = new object();
        private readonly object instanceLockTicket = new object();
        private string installation = string.Empty;
        private string microareaServicesPath = string.Empty;
        private string standardPath = string.Empty;
        private string customPath = string.Empty;
        private string appsPath = string.Empty;
        private string publishPath = string.Empty;
        private string baseAddressUrl = string.Empty;
        private SettingsConfigInfoTable settingsTable = new SettingsConfigInfoTable();
        private string remoteWebServer = string.Empty;
        private string remoteFileServer = string.Empty;
        private bool runAtServer = false;
        private static string tbApplicationPath = null;
        private static string magonetApplicationPath = null;
        private static string microareaConsoleApplicationPath = null;
        private bool isRunningInsideInstallation = false;
        private static PathFinder pathFinderInstance = null;
        private string company = string.Empty;
        private string user = string.Empty;
        private string edition = string.Empty;
        private Microarea.Common.FileSystemManager.FileSystemManager fileSystemManager = null;

        #endregion

        #region membri protetti
        protected string build = string.Empty;
        protected Diagnostic diagnostic = new Diagnostic("NameSolver");
        protected List<ApplicationInfo> applications;
        protected CoreTypes.FunctionsList webMethods = null;
        #endregion

        #region Proprieta' 

        //-----------------------------------------------------------------------------
        public bool EasyStudioAppsInCustom { get => easyStudioAppsInCustom; set => easyStudioAppsInCustom = value; }

        public string GetStandardPath { get { return standardPath; } }
        //-----------------------------------------------------------------------------
        public string GetAppsPath { get { return appsPath; } }

        //-----------------------------------------------------------------------------
        public string GetInstallationPath { get { return Path.GetDirectoryName(standardPath); } }
     
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
        public InstallationVersion InstallationVer
        {
            get
            {
                if (installationVer == null)
                    installationVer = GetInstallationVer();
                return installationVer;
            }
        }




        //TODO butta il base nel costruttore
        //----------------------------------------------------------------------------
        /// <summary>
        /// Oggetto statico globale PathFinder utilizzato ovunque in Mago.Net siano necessarie
        /// informazioni non dipendenti da username e company
        ///// </summary>
        public static PathFinder PathFinderInstance
        {
            get
            {
                lock (staticLockTicket)
                {
                    if (pathFinderInstance == null)
                    {
                        pathFinderInstance = new PathFinder();
                        pathFinderInstance.Init();
                        pathFinderInstance.LoadPrototypes();

                    }
                    return pathFinderInstance;
                }
            }
            set
            {
                lock (staticLockTicket)
                {
                    PathFinderInstance = value;
                    PathFinderInstance.LoadPrototypes();
                }
            }
        }
      
        //----------------------------------------------------------------------------
        /// <summary>
        /// Indica se il path finder sta girando sullo stesso server dell'installazione
        /// </summary>
        private bool InitFileSystemManager(PathFinder pathfinder)
        {
            fileSystemManager = new Common.FileSystemManager.FileSystemManager(pathfinder);
            fileSystemManager.FileSystemDriver = new FileSystemDriver(); // lui c e sempre
            FileSystemManagerInfo managerInfo = new FileSystemManagerInfo();
            managerInfo.LoadFile();
            if (managerInfo.GetDriver() == DriverType.Database) 
            {
                DatabaseDriver dataBaseDriver = new DatabaseDriver(pathfinder, managerInfo.GetStandardConnectionString(), managerInfo.GetCustomConnectionString());
                fileSystemManager.AlternativeDriver = dataBaseDriver;
            }

            return true;
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Indica se il path finder sta girando ALL'INTERNO del percorso di installebazione (es. TBServices, LoginManager)
        /// </summary>
        public bool IsRunningInsideInstallation { get { return isRunningInsideInstallation; } }
        //----------------------------------------------------------------------------
        public string Installation { get { return installation; } }
        //----------------------------------------------------------------------------
        public string RemoteWebServer { get { return remoteWebServer; } }
        //----------------------------------------------------------------------------
        /// <summary>
        /// Nome del server che espone le cartelle condivise di file system
        /// </summary>
        public string RemoteFileServer { get { return remoteFileServer; } }

        //----------------------------------------------------------------------------
        public string EasyLookServiceBaseUrl
        {
            get
            {
                return string.Format("{0}/{1}/", WebFrameworkRootUrl, NameSolverStrings.EasyLook);
            }
        }
 
        /// <summary>
        /// Ritorna il path del LoginManager
        /// </summary>
        //----------------------------------------------------------------------------
        public string LoginManagerPath
        {
            get
            {
                ModuleInfo mi = GetModuleInfoByName(NameSolverStrings.WebFramework, NameSolverStrings.LoginManager);
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
  
        //tipo di build debug o release, se non settata restituisce quella in run
        //----------------------------------------------------------------------------
        public string Build
        {
            get
            {
                //se non mi viene passata la build uso quella dell'assembly corrente.
                if (build == string.Empty)
                {
                    DirectoryInfo di = new DirectoryInfo(/*AppDomain.CurrentDomain.BaseDirectory todo rsweb*/ "");
                    if (string.Compare(di.Name, NameSolverStrings.Debug, StringComparison.OrdinalIgnoreCase) == 0)
                        build = NameSolverStrings.Debug;
                    else
                        if (string.Compare(di.Name, NameSolverStrings.Release, StringComparison.OrdinalIgnoreCase) == 0)
                        build = NameSolverStrings.Release;
                    else
                        build = Functions.DebugOrRelease();
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
                //if (string.Compare(RemoteWebServer, Dns.GetHostName(), StringComparison.OrdinalIgnoreCase) == 0)
                //    webSrv = "LocalHost";

                return string.Format("http://{0}:{1}/{2}", webSrv, InstallationData.ServerConnectionInfo.WebServicesPort, installation);
            }
        }
        //----------------------------------------------------------------------------
        public string Company { get { return company; } }
        //----------------------------------------------------------------------------
        public string User { get { return user; } }
        public string Edition { get { return edition; } set { edition = value; } }
 
        //----------------------------------------------------------------------------
        public IList ApplicationInfos
        {
            get
            {
                if (applications != null)
                    return applications;

                applications = new List<ApplicationInfo>();

                if (!AddApplicationsByType(ApplicationType.All))
                    return new List<ApplicationInfo>();
                return applications;
            }
        }
        #endregion property

        #region protected function
        /// <summary>
        /// Ritorna il full filenname (per lingua) relativo al namespace desiderato
        /// </summary>
        //---------------------------------------------------------------------------------
        protected string GetStandardFilename(ModuleInfo aModuleInfo, INameSpace aNamespace, string language)
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
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning,/* System.Reflection.MethodInfo.().Name + ": " + TODO rsweb*/ exc.ToString());
                return new InstallationVersion();
            }
        }
        //---------------------------------------------------------------------
        public void CreateApplicationInfo(string applicationName, ApplicationType applicationType, string appsPath)
        {
            if (
                applicationName == null ||
                applicationName == string.Empty ||
                applicationType == ApplicationType.Undefined ||
                !IsApplicationDirectory(appsPath + NameSolverStrings.Directoryseparetor + applicationName)
                )
                return;

            //path del contenitore di applicazioni custom
            string customAppContainerPath = GetCustomApplicationContainerPath(applicationType);

            //oggetto contenente le info di un'applicazione
            ApplicationInfo applicationInfo = new ApplicationInfo
                (
                applicationName,
                appsPath,
                customAppContainerPath,
                this
                );

            //aggiungo l'applicazione all'array
            if (applicationInfo.IsKindOf(applicationType))
                ApplicationInfos.Add(applicationInfo);
        }
        //---------------------------------------------------------------------
        protected bool Init()
        {
            return Init(string.Empty, string.Empty);
        }
        //---------------------------------------------------------------------
        protected bool Init(string aRemoteServer, string aInstallation)
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
            Task<IPHostEntry> entry = Dns.GetHostEntryAsync(remoteFileServer);
            runAtServer = entry.Result.HostName == Dns.GetHostName();

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
            return true;
        }
        //------------------------------------------------------------------
        protected bool CalculatePathsInsideInstallation()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;      //  "";  todo rsweb
            if (CalculatePathsInsideInstallation(basePath))
                return true;

            basePath = Assembly.GetEntryAssembly().Location;// Assembly.GetExecutingAssembly().Location;         TODO rsweb

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
            int idx = basePath.LastIndexOf("\\" + installation + "\\", StringComparison.OrdinalIgnoreCase);
            if (idx == -1)
                return false;
            string installationPath = basePath.Substring(0, idx + 1 + installation.Length);
            standardPath = Path.Combine(installationPath, NameSolverStrings.Standard);
            customPath = Path.Combine(installationPath, NameSolverStrings.Custom);
            appsPath = Path.Combine(installationPath, NameSolverStrings.Apps);
            publishPath = Path.Combine(appsPath, NameSolverStrings.Publish);

            return PathFinder.PathFinderInstance.ExistPath(standardPath);
        }
        //---------------------------------------------------------------------
        protected void CalculatePathsOutsideInstallation()
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

            if (!PathFinder.PathFinderInstance.ExistPath(standardPath))
                throw new Exception(string.Format(Messages.InvalidInstallation, installation, standardPath));
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
                    AddApplicationsByType(ApplicationType.TaskBuilderApplication) &&
                    AddApplicationsByType(ApplicationType.Customization);
            }

            List<string> tempApplications = null;
            string apps = GetStandardApplicationContainerPath(applicationType);
            if (apps == string.Empty)
            {
                diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, applicationType.ToString()));
                return false;
            }


            if (!PathFinder.PathFinderInstance.ExistPath(apps))
            {
                //tapullo per far funzionare il migratore report che legge la vecchia struttura
                if (ApplicationInfo.MatchType(applicationType, ApplicationType.TaskBuilderApplication))
                    apps = apps.Replace(NameSolverStrings.TaskBuilderApplications, "TaskBuilderApplications");

                if (!PathFinder.PathFinderInstance.ExistPath(apps))
                    return true;
            }


            tempApplications = PathFinder.PathFinderInstance.GetAllApplicationInfo(apps);
                

            // controlla le dichiarazioni di directory ed elimina quelle 
            // che non corrispondono a delle vere applicazioni da caricare.
            foreach (string applicationName in tempApplications)
            {
                CreateApplicationInfo(applicationName, applicationType, apps);
            }

            return true;
        }
        #endregion protected function

        #region static function
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
                ? GetEasyStudioHomePath()
                : standardPath;
            return Path.Combine(folder, appContainerName);
        }
 
        //---------------------------------------------------------------------
        public static string GetSolutionFileName(string productName)
        {
            return productName + NameSolverStrings.SolutionExtension;
        }
       
        //---------------------------------------------------------------------------------
        public static string GetUserPath(string userName)
        {
            if (userName == null || userName == string.Empty)
                return string.Empty;

            if (string.Compare(userName, NameSolverStrings.AllUsers, StringComparison.OrdinalIgnoreCase) == 0)
                return userName;

            string correctUserName = userName.Replace(NameSolverStrings.Directoryseparetor, '.');

            return
                NameSolverStrings.Users +
                NameSolverStrings.Directoryseparetor +
                correctUserName;
        }
        //---------------------------------------------------------------------------------
        public static string GetUserUri(string userName)
        {
            if (userName == null || userName == string.Empty || string.Compare(userName, NameSolverStrings.Standard, StringComparison.OrdinalIgnoreCase) == 0)
                return string.Empty;

            if (string.Compare(userName, NameSolverStrings.AllUsers, StringComparison.OrdinalIgnoreCase) == 0)
                return userName + "/";

            return NameSolverStrings.Users + "/" + userName + "/";
        }

        //RIVEDERE POI CON BRUNA LARA
        //--------------------------------------------------------------------------------
        public static string[] GetCSharpFilesIn(string sourcesPath)
        {
            if (!Directory.Exists(sourcesPath))
                return null;

            string searchPattern = string.Format("*{0}", NameSolverStrings.CSharpExtension);

            return Directory.GetFiles(sourcesPath, searchPattern);
        }
       
        //--------------------------------------------------------------------------------
        public static string GetDesignerSourcesPathFromDll(string dllPath)
        {
            // from:	...\ProspectiveSuppliers\sa\ProspectiveSuppliers.dll
            // to :		...\ProspectiveSuppliers\sa\ProspectiveSuppliers_src\designer
            var dllFileNameWOExtension = Path.GetFileNameWithoutExtension(dllPath);
            var srcFolderName = String.Format(System.Globalization.CultureInfo.InvariantCulture, NameSolverStrings.SourcesFolderNameMask, dllFileNameWOExtension);
            return Path.Combine(Path.GetDirectoryName(dllPath), srcFolderName, NameSolverStrings.DesignerFolderName);
        }

        #endregion static function


        #region public function

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
            if (create && !PathFinder.PathFinderInstance.ExistPath(s))
                PathFinder.PathFinderInstance.CreateFolder(s, false);
            return s;
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
                ModuleInfo bmi = GetModuleInfo(ns);
                return bmi == null ? "" : Path.Combine(bmi.Path, NameSolverStrings.JsonForms);
            }
            return "";
        }

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
            foreach (ApplicationInfo aApplicationInfo in ApplicationInfos)
            {
                if (ApplicationInfo.MatchType(aApplicationInfo.ApplicationType, applicationType))
                    applicationList.Add(aApplicationInfo.Name);
            }

            return true;
        }
        //---------------------------------------------------------------------
        public void DeleteApplicationByName(string applicationName)
        {

            for (int i = ApplicationInfos.Count - 1; i >= 0; i--)
            {
                ApplicationInfo aApplicationInfo = ApplicationInfos[i] as ApplicationInfo;
                if (string.Compare(aApplicationInfo.Name, applicationName, StringComparison.OrdinalIgnoreCase) != 0)
                    continue;

                ApplicationInfos.RemoveAt(i);
                return;
            }
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
            ModuleInfo mi = GetModuleInfoByName(applicationName, moduleName) as ModuleInfo;
            if (mi == null) return null;
            return settingsTable.GetSettingItem(sectionName, settingName, mi);
        }
        //----------------------------------------------------------------------------
        public void RefreshEasyStudioApps(ApplicationType type)
        {
            //elimino tutte le applicazioni custom e le ricarico
            for (int i = applications.Count - 1; i >= 0; i--)
                if (((ApplicationInfo)applications[i]).ApplicationType == type)
                    applications.RemoveAt(i);

            AddApplicationsByType(type);
        }

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



        //---------------------------------------------------------------------
        public string GetStandardUpgradeInfoXML(string application, string module)
        {
            return GetStandardDatabaseScriptPath(application, module) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.UpgradeScript +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.UpgradeInfoXml;
        }

        public string GetStandardDatabaseScriptPath(string application, string module)
        {
            return GetApplicationModulePath(application, module) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.DatabaseScript;
        }

        //---------------------------------------------------------------------------------
        public string GetStandardScriptPath(string path, string nameScript, string provider, bool create, int rel)
        {
            string allPath = path;

            string script =
                (create)
                ? nameScript
                : string.Concat(NameSolverDatabaseStrings.ReleaseNumDirectory, rel, NameSolverStrings.Directoryseparetor, nameScript);

            if (provider == NameSolverDatabaseStrings.SQLOLEDBProvider || provider == NameSolverDatabaseStrings.SQLODBCProvider)
                path += NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.SqlServer +
                    NameSolverStrings.Directoryseparetor +
                    script;

            if (provider == NameSolverDatabaseStrings.OraOLEDBProvider ||
                provider == NameSolverDatabaseStrings.MSDAORAProvider)
                path += NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.Oracle +
                    NameSolverStrings.Directoryseparetor +
                    script;

            if (provider == NameSolverDatabaseStrings.PostgreOdbcProvider)
                path += NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.Postgre +
                    NameSolverStrings.Directoryseparetor +
                    script;

            if (ExistFile(path))//ExistFile(path))
                return path;

            return
                (allPath += NameSolverStrings.Directoryseparetor + NameSolverStrings.All + NameSolverStrings.Directoryseparetor + script);
        }
        //---------------------------------------------------------------------------------
        public string GetDocumentPath(INameSpace aNameSpace)
        {
            if (aNameSpace == null)
                return null;

            ModuleInfo aModuleInfo = GetModuleInfo(aNameSpace);
            if (aModuleInfo == null)
                return null;

            return aModuleInfo.GetDocumentPath(aNameSpace.Document);
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
                // potrebbe arrivare un filename con sintassi Unix cioè anche con "/"
                sObjectFullPath = sObjectFullPath.Replace('/', NameSolverStrings.Directoryseparetor);

                // TODOBRUNA 
                if (sObjectFullPath == null || sObjectFullPath.Length == 0)
                    return null;

                FileInfo sObjectFullPathInfo = new FileInfo(sObjectFullPath);

                // prima controllo le cose di base
                if (sObjectFullPathInfo.Name.Length == 0)
                    return null;

                // poi la consistenza dei token di path
                string[] tokens = sObjectFullPath.Split(NameSolverStrings.Directoryseparetor);

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
                    // sono nella custom, devo scendere di un ulteriore livello
                    application = tokens[CustomAppSegmentPath];
                    module = tokens[CustomModuleSegmentPath];
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
                    dirName = (string.Compare(dirName, NameSolverStrings.AllUsers, StringComparison.OrdinalIgnoreCase) == 0)
                        ? dirInfo.Parent.Name
                        : dirInfo.Parent.Parent.Name;
                }

                // da qui controllo il tipo di oggetto sulla base del nome della directory che lo contiene
                if (string.Compare(dirName, NameSolverStrings.Report, StringComparison.OrdinalIgnoreCase) == 0) nameSpaceType = NameSolverStrings.Report;
                else if (string.Compare(dirName, NameSolverStrings.Others, StringComparison.OrdinalIgnoreCase) == 0) nameSpaceType = NameSolverStrings.File;
                else if (string.Compare(dirName, NameSolverStrings.Images, StringComparison.OrdinalIgnoreCase) == 0) nameSpaceType = NameSolverStrings.Image;
                else if (string.Compare(dirName, NameSolverStrings.Texts, StringComparison.OrdinalIgnoreCase) == 0) nameSpaceType = NameSolverStrings.Text;
                else if (string.Compare(dirName, NameSolverStrings.ReferenceObjects, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    nameSpaceType = NameSpaceSegment.Hotlink;
                    nameSpaceSuffix = nameSpaceSuffix.Substring(0, nameSpaceSuffix.LastIndexOf("."));
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
            sObjectFullPath = sObjectFullPath.Replace('/', NameSolverStrings.Directoryseparetor);

            // TODOBRUNA 
            if (sObjectFullPath == null || sObjectFullPath.Length == 0)
                return null;

            FileInfo sObjectFullPathInfo = new FileInfo(sObjectFullPath);

            // prima controllo le cose di base
            if (sObjectFullPathInfo.Name.Length == 0)
                return null;

            // poi la consistenza dei token di path
            string[] tokens = sObjectFullPath.Split(NameSolverStrings.Directoryseparetor);

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

        /// <summary>
        /// Ritorna il path nella custom della cartella Report dato un woormFilePath
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetCustomReportPathFromWoormFile(string woormFilePath, string companyName, string userName)
        {
            bool isStandard = IsStandardPath(woormFilePath);
            string[] ar = woormFilePath.Split(new char[] { '\\', '/' });
            int ub = ar.GetUpperBound(0);
            bool isAllUsers = !isStandard && string.Compare("AllUsers", ar[ub - 1], StringComparison.OrdinalIgnoreCase) == 0;
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

            if (string.Compare(userName, NameSolverStrings.Standard, StringComparison.OrdinalIgnoreCase) == 0)
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
            if (!recursive || ExistFile(reportFile))//ExistFile(reportFile))
                return reportFile;

            reportPath = GetCustomUserReportPath(companyName, NameSolverStrings.AllUsers, ns.Application, ns.Module);
            if (reportPath == string.Empty)
                return string.Empty;

            reportFile = Path.Combine(reportPath, repName);
            if (!recursive || ExistFile(reportFile))//ExistFile(reportFile))
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

        //---------------------------------------------------------------------
        public string GetStandardCreateInfoXML(string application, string module)
        {
            return GetStandardDatabaseScriptPath(application, module) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.CreateScript +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.CreateInfoXml;
        }
        //---------------------------------------------------------------------------------
        public string GetStandardModuleTextPath(INameSpace aNameSpace)
        {
            if (aNameSpace == null)
                return null;

            string fileDir = Path.Combine(GetStandardModulePath(aNameSpace), NameSolverStrings.Files);

            return Path.Combine(fileDir, NameSolverStrings.Texts);
        }
        //---------------------------------------------------------------------
        public string GetApplicationConfigFullName(string appName)
        {
            return GetStandardApplicationPath(appName) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Application +
                NameSolverStrings.ConfigExtension;
        }
        //---------------------------------------------------------------------
        public string GetModuleConfigFullName(string appName, string moduleName)
        {
            return GetApplicationModulePath(appName, moduleName) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Module +
                NameSolverStrings.ConfigExtension;
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
        //---------------------------------------------------------------------------------
        public string GetImagePath(INameSpace aNameSpace, ImageSize size = ImageSize.None)
        {
            if (aNameSpace == null)
                return null;

            string fileDir = string.Empty;
            string standardmodulepath = GetStandardModulePath(aNameSpace);
            if (standardmodulepath == null) return null;

            if (size == ImageSize.None)
                fileDir = Path.Combine(standardmodulepath, NameSolverStrings.Files, NameSolverStrings.Images);
            else
                fileDir = Path.Combine(standardmodulepath, NameSolverStrings.Files, NameSolverStrings.Images, size.ToString().ReplaceNoCase("Size", ""));

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
            var standardModulePath = GetStandardModulePath(applicationName, moduleName);
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
        //---------------------------------------------------------------------------------
        public ModuleInfo GetModuleInfo(INameSpace aNameSpace)
        {
            if (aNameSpace == null)
                return null;

            string moduleName = aNameSpace.Module;
            if (moduleName == null || moduleName.Length == 0)
                return null;

            ApplicationInfo aApplicationInfo = GetApplicationInfo(aNameSpace);

            if (aApplicationInfo == null)
                return null;

            return aApplicationInfo.GetModuleInfoByName(moduleName);
        }
        //---------------------------------------------------------------------------------
        public ApplicationInfo GetApplicationInfo(INameSpace aNameSpace)
        {
            if (aNameSpace == null)
                return null;

            string applicationName = aNameSpace.Application;

            if (applicationName == null || applicationName.Length == 0)
                return null;

            return GetApplicationInfoByName(applicationName);
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
        //---------------------------------------------------------------------------------
        public string GetStandardModuleXmlPath(INameSpace aNameSpace)
        {
            if (aNameSpace == null)
                return null;

            return Path.Combine(GetStandardModulePath(aNameSpace), xmlExt);
        }
        //---------------------------------------------------------------------------------
        public string GetStandardModuleXmlPath(INameSpace aNameSpace, string language)
        {
            if (aNameSpace == null || language == string.Empty)
                return null;

            return Path.Combine(GetStandardModuleXmlPath(aNameSpace), language);
        }
        //---------------------------------------------------------------------------------
        public string GetNumberToLiteralXmlFullName(INameSpace aNameSpace, string language)
        {
            if (aNameSpace == null || language == string.Empty)
                return null;

            return Path.Combine(GetStandardModuleXmlPath(aNameSpace, language), numberToLiteralXmlFileName);
        }
        //-----------------------------------------------------------------------------
        private string GetCustomFilename(ModuleInfo aModuleInfo, INameSpace aNamespace, string aUser)
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

                        if (aUser == null)
                            fullFileName = aModuleInfo.GetCustomReportFullFilename(reportFileName);
                        else
                            fullFileName = aModuleInfo.GetCustomReportFullFilename(reportFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.Image:
                    fullFileName = aModuleInfo.GetCustomImageFullFilename(aNamespace.Image, aUser);
                    break;

                case NameSpaceObjectType.Text:
                    fullFileName = aModuleInfo.GetCustomTextFullFilename(aNamespace.Text, aUser);
                    break;

                case NameSpaceObjectType.File:
                    fullFileName = aModuleInfo.GetCustomFileFullFilename(aNamespace.File, aUser);
                    break;

                case NameSpaceObjectType.ExcelDocument:
                    {
                        string documentFileName = aNamespace.GetExcelDocumentFileName();
                        if (documentFileName == null || documentFileName == String.Empty)
                            return String.Empty;

                        fullFileName = aModuleInfo.GetCustomExcelDocumentFullFilename(documentFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.ExcelTemplate:
                    {
                        string templateFileName = aNamespace.GetExcelTemplateFileName();
                        if (templateFileName == null || templateFileName == String.Empty)
                            return String.Empty;

                        fullFileName = aModuleInfo.GetCustomExcelTemplateFullFilename(templateFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.WordDocument:
                    {
                        string documentFileName = aNamespace.GetWordDocumentFileName();
                        if (documentFileName == null || documentFileName == String.Empty)
                            return String.Empty;

                        fullFileName = aModuleInfo.GetCustomWordDocumentFullFilename(documentFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.WordTemplate:
                    {
                        string templateFileName = aNamespace.GetWordTemplateFileName();
                        if (templateFileName == null || templateFileName == String.Empty)
                            return String.Empty;

                        fullFileName = aModuleInfo.GetCustomWordTemplateFullFilename(templateFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.ExcelDocument2007:
                    {
                        string documentFileName = aNamespace.GetExcel2007DocumentFileName();
                        if (documentFileName == null || documentFileName == String.Empty)
                            return String.Empty;

                        fullFileName = aModuleInfo.GetCustomExcelDocument2007FullFilename(documentFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.ExcelTemplate2007:
                    {
                        string templateFileName = aNamespace.GetExcel2007TemplateFileName();
                        if (templateFileName == null || templateFileName == String.Empty)
                            return String.Empty;

                        fullFileName = aModuleInfo.GetCustomExcelTemplate2007FullFilename(templateFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.WordDocument2007:
                    {
                        string documentFileName = aNamespace.GetWord2007DocumentFileName();
                        if (documentFileName == null || documentFileName == String.Empty)
                            return String.Empty;

                        fullFileName = aModuleInfo.GetCustomWordDocument2007FullFilename(documentFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.WordTemplate2007:
                    {
                        string templateFileName = aNamespace.GetWord2007TemplateFileName();
                        if (templateFileName == null || templateFileName == String.Empty)
                            return String.Empty;

                        fullFileName = aModuleInfo.GetCustomWordTemplate2007FullFilename(templateFileName, aUser);
                        break;
                    }

                case NameSpaceObjectType.DocumentSchema:
                    fullFileName = aModuleInfo.GetCustomDocumentSchemaFullFilename(aNamespace.WordDocument, aNamespace.DocumentSchema, aUser);
                    break;

                case NameSpaceObjectType.ReportSchema:
                    fullFileName = aModuleInfo.GetCustomReportSchemaFullFilename(aNamespace.ReportSchema, aUser);
                    break;
            }

            if (fullFileName != null && fullFileName != String.Empty && ExistFile(fullFileName))//ExistFile(fullFileName))
                return fullFileName;

            return String.Empty;
        }
        //-----------------------------------------------------------------------------
        private string GetCurrentUserCustomFilename(ModuleInfo aModuleInfo, INameSpace aNamespace)
        {
            if
                (
                user == null ||
                user == String.Empty ||
                String.Compare(user, NameSolverStrings.AllUsers) == 0
                )
                return String.Empty;

            return GetCustomFilename(aModuleInfo, aNamespace, user);
        }
        //-----------------------------------------------------------------------------
        private string GetAllUsersCustomFilename(ModuleInfo aModuleInfo, INameSpace aNamespace)
        {
            return GetCustomFilename(aModuleInfo, aNamespace, NameSolverStrings.AllUsers);
        }
        //-----------------------------------------------------------------------------
        public string GetFilename(INameSpace aNamespace, ref CommandOrigin aCommandOrigin, string language)
        {
            aCommandOrigin = CommandOrigin.Unknown;

            if (!aNamespace.IsValid())
                return String.Empty;

            ModuleInfo aModuleInfo = (ModuleInfo)GetModuleInfoByName(aNamespace.Application, aNamespace.Module);
            if (aModuleInfo == null)
                return String.Empty;

            // prima prova sullo user corrente
            string fullFileName = GetCurrentUserCustomFilename(aModuleInfo, aNamespace);
            if (fullFileName != null && fullFileName != String.Empty && ExistFile(fullFileName))//ExistFile(fullFileName))
            {
                aCommandOrigin = CommandOrigin.CustomCurrentUser;

                return fullFileName;
            }

            // poi su AllUsers
            fullFileName = GetAllUsersCustomFilename(aModuleInfo, aNamespace);
            if (fullFileName != null && fullFileName != String.Empty && ExistFile(fullFileName))//ExistFile(fullFileName))
            {
                aCommandOrigin = CommandOrigin.CustomAllUsers;

                return fullFileName;
            }

            // e, infine, sulla standard
            fullFileName = GetStandardFilename(aModuleInfo, aNamespace, language);
            if (fullFileName != null && fullFileName != String.Empty)
            {
                aCommandOrigin = CommandOrigin.Standard;

                return fullFileName;
            }

            return String.Empty;
        }
        //-----------------------------------------------------------------------------
        public string GetFilename(INameSpace aNamespace, string language)
        {
            CommandOrigin commandOrigin = CommandOrigin.Unknown;
            return GetFilename(aNamespace, ref commandOrigin, language);
        }
        //---------------------------------------------------------------------------------
        public string GetCustomReportPathFromNamespace(INameSpace ns)
        {
            return GetCustomReportPathFromNamespace(ns, company, user);
        }
        //---------------------------------------------------------------------------------
        public string GetAllUsersCustomReportPathFromNamespace(INameSpace ns)
        {
            return GetCustomReportPathFromNamespace(ns, company, NameSolverStrings.AllUsers);
        }
        //---------------------------------------------------------------------------------
        public string GetCustomExcelPathFromNamespace(INameSpace ns)
        {
            return GetCustomExcelPathFromNamespace(ns, company, user);
        }
        //---------------------------------------------------------------------------------
        public string GetAllUsersCustomExcelPathFromNamespace(INameSpace ns)
        {
            return GetCustomExcelPathFromNamespace(ns, company, NameSolverStrings.AllUsers);
        }
        //---------------------------------------------------------------------------------
        public string GetCustomWordPathFromNamespace(INameSpace ns)
        {
            return GetCustomWordPathFromNamespace(ns, company, user);
        }
        //---------------------------------------------------------------------------------
        public string GetAllUsersCustomWordPathFromNamespace(INameSpace ns)
        {
            return GetCustomWordPathFromNamespace(ns, company, NameSolverStrings.AllUsers);
        }
        //---------------------------------------------------------------------------------
        public string GetCustomUserApplicationDataPath()
        {
            return String.Concat(
                GetCustomCompanyPath(),
                NameSolverStrings.Directoryseparetor,
                NameSolverStrings.AppData,
                NameSolverStrings.Directoryseparetor,
                User
                );
        }
        //---------------------------------------------------------------------------------
        public string GetCustomUserApplicationStateFile()
        {
            return Path.Combine(GetCustomUserApplicationDataPath(), NameSolverStrings.CustomAppStateFile);
        }
       
        //---------------------------------------------------------------------------------
        public string GetCompanyAllUsersDefaultThemeFilePath()
        {
            return Path.Combine(
                GetCustomModulePath(NameSolverStrings.Framework, "TbGeneric"),
                NameSolverStrings.Settings,
                NameSolverStrings.AllUsers,
                defaultThemeFileName
                );
        }
        //---------------------------------------------------------------------------------
        public string GetCompanyUserDefaultThemeFilePath()
        {
            return Path.Combine(
                GetCustomModulePath(NameSolverStrings.Framework, "TbGeneric"),
                NameSolverStrings.Settings,
                User,
                defaultThemeFileName
                );
        }
        //---------------------------------------------------------------------
        public string CalculateRemoteStandardPath()
        {
            return string.Format(@"\\{0}\{1}_{2}", RemoteFileServer, installation, NameSolverStrings.Standard);
        }
        //---------------------------------------------------------------------
        public string CalculateLocalStandardPath()
        {
            return string.Format(@"\\{0}\{1}\{2}", RemoteFileServer, installation, NameSolverStrings.Standard);
        }

        //---------------------------------------------------------------------
        public string CalculateRemoteCustomPath()
        {
            return string.Format(@"\\{0}\{1}_{2}", RemoteFileServer, installation, NameSolverStrings.Custom);
        }
        //-----------------------------------------------------------------------------
        public string GetApplicationContainerName(ApplicationType applicationType)
        {
            if (ApplicationInfo.MatchType(applicationType, ApplicationType.TaskBuilder))
                return NameSolverStrings.TaskBuilder;
            if (ApplicationInfo.MatchType(applicationType, ApplicationType.TaskBuilderNet))
                return NameSolverStrings.TaskBuilder;
            if (ApplicationInfo.MatchType(applicationType, ApplicationType.TaskBuilderApplication))
                return NameSolverStrings.TaskBuilderApplications;
            if (ApplicationInfo.MatchType(applicationType, ApplicationType.Customization))
                return NameSolverStrings.TaskBuilderApplications;

            Debug.Fail("Tipo applicazione non gestito");

            return string.Empty;
        }
        //-----------------------------------------------------------------------------
        public bool IsApplicationDirectory(string appPath)
        {
            return (appPath.Length != 0) &&
               ExistFile(
                appPath +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Application +
                NameSolverStrings.ConfigExtension
                );
        }
        //-----------------------------------------------------------------------------
        public bool IsModuleDirectory(string modulePath)
        {
            return (modulePath.Length != 0) &&
                ExistFile
                (
                modulePath +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Module +
                NameSolverStrings.ConfigExtension
                );
        }
        //---------------------------------------------------------------------
        public ApplicationInfo GetApplicationInfoByName(string applicationName)
        {
            foreach (ApplicationInfo aApplicationInfo in ApplicationInfos)
            {
                if (string.Compare(aApplicationInfo.Name, applicationName, StringComparison.OrdinalIgnoreCase) == 0)
                    return aApplicationInfo;
            }

            return null;
        }
        //---------------------------------------------------------------------
        public string GetStandardApplicationPath(string applicationName)
        {
            ApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);

            if (aApplicationInfo == null)
                return string.Empty;

            return aApplicationInfo.Path;
        }
        //---------------------------------------------------------------------
        public ModuleInfo GetModuleInfoByName(string applicationName, string moduleName)
        {
            if (applicationName == null || moduleName == null ||
                applicationName == string.Empty || moduleName == string.Empty)
                return null;

            ApplicationInfo aApplicationInfo = null;
            try
            {
                aApplicationInfo = (ApplicationInfo)GetApplicationInfoByName(applicationName);
            }
            catch (Exception err)
            {
                Debug.Fail(err.Message);
            }
            if (aApplicationInfo == null)
                return null;

            return aApplicationInfo.GetModuleInfoByName(moduleName);
        }
        //---------------------------------------------------------------------
        public void RemoveModuleInfoByName(string applicationName, string moduleName)
        {
            if (applicationName == null || moduleName == null ||
                applicationName == string.Empty || moduleName == string.Empty)
                return;

            ApplicationInfo aApplicationInfo = null;
            try
            {
                aApplicationInfo = (ApplicationInfo)GetApplicationInfoByName(applicationName);
            }
            catch (Exception err)
            {
                Debug.Fail(err.Message);
            }
            if (aApplicationInfo == null)
                return;

            aApplicationInfo.RemoveModuleInfo(moduleName);
        }
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

            ApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);
            if (aApplicationInfo == null)
                return string.Empty;

            ModuleInfo aModuleInfo = aApplicationInfo.GetModuleInfoByName(moduleName);
            if (aModuleInfo == null)
                return string.Empty;

            return Path.Combine(aApplicationInfo.Path, aModuleInfo.Name);
        }
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

            ApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);
            if (aApplicationInfo == null)
                return string.Empty;

            ModuleInfo aModuleInfo = aApplicationInfo.GetModuleInfoByName(moduleName);
            if (aModuleInfo == null)
                return string.Empty;

            return Path.Combine(aApplicationInfo.Path, aModuleInfo.Name);
        }
        //---------------------------------------------------------------------
        public ICollection GetModulesList(string applicationName)
        {
            ApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);

            if (aApplicationInfo == null)
                return null;

            return aApplicationInfo.Modules;
        }
        //-----------------------------------------------------------------------------
        public bool IsStandardPath(string aPath)
        {
            string path = GetStandardPath.ToLower();
            aPath = aPath.Replace('/', Path.PathSeparator);
            aPath = aPath.ToLower();

            return aPath.StartsWith(path);
        }
        
        //-----------------------------------------------------------------------------
        public string FindSplashFile(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName)) return string.Empty;
            string filePath = Path.Combine(GetStandardPath, fileName);
            if (ExistFile(filePath))
                return filePath;

            return FindFileInSolutionFolder(fileName);
        }
        //-----------------------------------------------------------------------------
        public Dictionary<string, string> GetSynchroProfilesFilePath(string providername)
        {
            Dictionary<string, string> synchroFilesDict = GetSynchroFilesFolderPath(providername);

            Dictionary<string, string> synchroProfilesDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in synchroFilesDict)
            {
                string synchroFilesFolder = Path.Combine(kvp.Value, NameSolverStrings.SynchroProfilesXmlFile);
                if (ExistFile(synchroFilesFolder))
                    synchroProfilesDict.Add(kvp.Key, synchroFilesFolder);
            }

            return synchroProfilesDict;
        }
        //-----------------------------------------------------------------------------
        public Dictionary<string, string> GetSynchroMassiveProfilesFilePath(string providername)
        {
            Dictionary<string, string> synchroFilesDict = GetSynchroFilesFolderPath(providername);

            Dictionary<string, string> synchroProfilesDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in synchroFilesDict)
            {
                string synchroFilesFolder = Path.Combine(kvp.Value, NameSolverStrings.SynchroMassiveProfilesXmlFile);
                if (ExistFile(synchroFilesFolder))
                    synchroProfilesDict.Add(kvp.Key, synchroFilesFolder);
            }

            return synchroProfilesDict;
        }
        //-----------------------------------------------------------------------------
        public Dictionary<string, string> GetSynchroFilesActionFolderPath(string providername)
        {
            Dictionary<string, string> synchroFilesDict = GetSynchroFilesFolderPath(providername);

            Dictionary<string, string> synchroFilesActionDict = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in synchroFilesDict)
            {
                string synchroActionFolder = Path.Combine(kvp.Value, NameSolverStrings.SynchroProfilesActionsXmlFolder);
                if (PathFinder.PathFinderInstance.ExistPath(synchroActionFolder))
                    synchroFilesActionDict.Add(kvp.Key, synchroActionFolder);
            }

            return synchroFilesActionDict;
        }
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
                if (string.Compare(applicationName, "ERP", StringComparison.OrdinalIgnoreCase) != 0)
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

                if (PathFinder.PathFinderInstance.ExistPath(fullFolderPath))
                {
                    DirectoryInfo di = new DirectoryInfo(appPath);
                    string path;
                    if (!synchroFilesDictionary.TryGetValue(di.Name, out path))
                        synchroFilesDictionary.Add(di.Name, fullFolderPath);
                }
            }

            return synchroFilesDictionary;
        }
        //-----------------------------------------------------------------------------
        public TBFile[] GetBrandFiles()
        {
          //  Hashtable ht = CollectionsUtil.CreateCaseInsensitiveHashtable(); // Dictionary<string, FileInfo>
            Dictionary<string, TBFile> ht = new Dictionary<string, TBFile>();
            string stdPath = GetStandardPath;

            string appsPaths = Path.Combine(stdPath, NameSolverStrings.TaskBuilderApplications);
            if (ExistPath(appsPaths))
                foreach (TBDirectoryInfo dir in GetSubFolders(appsPaths))
                {
                    if (!IsApplicationDirectory(dir.CompleteDirectoryPath))
                        continue;
                    string solPath = Path.Combine(dir.CompleteDirectoryPath, NameSolverStrings.Solutions);
                    if (!ExistPath(solPath))
                        continue;
                    AddDirectoryBrandFiles(solPath, ht);
                }
            else
                Debug.Fail(NameSolverStrings.TaskBuilderApplications + " folder is missing.");

            List<TBFile> l = new List<TBFile>(ht.Keys.Count);
            foreach (TBFile file in ht.Values)
                l.Add(file);
            return l.ToArray();
        }


        //---------------------------------------------------------------------
        private string GetCustomSubscriptionApplicationContainerPath(ApplicationType aApplicationType, string subscription)
        {
            if (aApplicationType == ApplicationType.All ||
                aApplicationType == ApplicationType.Undefined)
            {
                diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
                return string.Empty;
            }

            string customSubscriptionPath = GetCustomSubscriptionPath();
            customSubscriptionPath = GetCustomSubscriptionPath(subscription);
            if (customSubscriptionPath == string.Empty)
                return string.Empty;

            string applicationContainerName = GetApplicationContainerName(aApplicationType);
            if (applicationContainerName == string.Empty)
            {
                diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
                return string.Empty;
            }

            return
                customSubscriptionPath +
                Path.DirectorySeparatorChar +
                applicationContainerName;
        }

        //---------------------------------------------------------------------
        private string GetCustomSubscriptionApplicationPath(string applicationName, string subscription)
        {
            ApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);
            if (aApplicationInfo == null)
                return string.Empty;

            string customApplicationContainer = GetCustomSubscriptionApplicationContainerPath(aApplicationInfo.ApplicationType, subscription);

            if (customApplicationContainer == string.Empty)
                return string.Empty;

            return
                customApplicationContainer +
                Path.DirectorySeparatorChar +
                aApplicationInfo.Name;
        }
        //---------------------------------------------------------------------
        protected string GetCustomSubscriptionModulePath(string applicationName, string moduleName, string subscription)
        {
            string customApplicationPath = GetCustomSubscriptionApplicationPath(applicationName, subscription);
            if (customApplicationPath == string.Empty)
                return string.Empty;

            return customApplicationPath + Path.DirectorySeparatorChar + moduleName;
        }
        //---------------------------------------------------------------------------------
        public string GetGroupImagePathByTheme(INameSpace aNameSpace, string themeName, string subscription)
        {
            string fileDir = Path.Combine(
              GetCustomModulePath(subscription, aNameSpace.Application, aNameSpace.Module),
              NameSolverStrings.Files,
              NameSolverStrings.Images,
              themeName
              );

            string filePath = Path.Combine(fileDir, aNameSpace.Image);

            if (ExistFile(filePath))
            {
                return filePath;
            }

            filePath = GetImagePathByTheme(aNameSpace, themeName);
            if (ExistFile(filePath))
            {
                return filePath;
            }

            return string.Empty;
        }
        //---------------------------------------------------------------------
        public string GetStandardDictionaryPath(string appName, string moduleName)
        {
            string modulePath = GetApplicationModulePath(appName, moduleName);
            if (modulePath == string.Empty)
                return string.Empty;

            return Path.Combine(modulePath, NameSolverStrings.Dictionary);

        }
        //---------------------------------------------------------------------------------
        public string GetGroupImagePath(INameSpace aNameSpace)
        {
            string fileDir = Path.Combine(
                GetCustomModulePath(aNameSpace.Application, aNameSpace.Module),
                NameSolverStrings.Files,
                NameSolverStrings.Images
                );

            string filePath = Path.Combine(fileDir, aNameSpace.Image);

            if (ExistFile(filePath))
            {
                return filePath;
            }

            return GetImagePath(aNameSpace);
        }
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
        //-----------------------------------------------------------------------------
        public void LoadPrototypes()
        {
            if (webMethods == null)
            {
                webMethods = new CoreTypes.FunctionsList();
            }
        }
        //-----------------------------------------------------------------------------
        public List<TBFile> GetSolutionFiles()
        {
            return GetSolutionFiles(false);
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
                strMainBrandFile = Path.Combine(strMainBrandFolder, NameSolverStrings.MainBrandFile);
                if (ExistFile(strMainBrandFile) && PathFinder.PathFinderInstance.ExistPath(strThemeFolder))
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
            if (!string.IsNullOrEmpty(strThemesPath) && ExistPath(strThemesPath))
            {
                //  DirectoryInfo di = new DirectoryInfo(strThemesPath);

                List<TBFile> masterThemesInfo = GetFiles(strThemesPath, "*" + NameSolverStrings.ThemeExtension);
                foreach (TBFile item in masterThemesInfo)
                {
                    allThemes.Add(item.completeFileName);
                }
            }

            ApplicationInfo appInfo = GetApplicationInfoByName(NameSolverStrings.Framework);
            string defaultTheme = Path.Combine(appInfo.Path, NameSolverStrings.Themes);
            if (!string.IsNullOrEmpty(defaultTheme) && ExistPath(defaultTheme))
            {
                DirectoryInfo di = new DirectoryInfo(defaultTheme);
                List<TBFile> masterThemesInfo = GetFiles(defaultTheme, "*.*");

                foreach (TBFile item in masterThemesInfo)
                {
                    allThemes.Add(item.completeFileName);
                }
            }
            return allThemes;
        }
        //-----------------------------------------------------------------------------
        public List<TBFile> GetSolutionFiles(bool checkApplicationConfig)
        {
            string appsPaths = Path.Combine(GetStandardPath, NameSolverStrings.TaskBuilderApplications);
            List<TBFile> list = new List<TBFile>();
            if (ExistPath(appsPaths)) // pre-TB2.0 versions named it differently
                foreach (TBDirectoryInfo dir in GetSubFolders(appsPaths))
                {
                    //Anomalia 14450: gli sviluppatori usano manipolare gli application.config 
                    //per testare le proprie applicazioni con o senza altre. 
                    //se noi carichiamo solo le solution che appartengono ad una applicazione valida 
                    //li obblighiamo ogni volta a riattivare 
                    //oltretutto questo è un effetto non voluto che prima dell a 2.7 non c'era.
                    if (checkApplicationConfig && !IsApplicationDirectory(dir.CompleteDirectoryPath))
                    {//non tradurre
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Missing application.config in: " + dir.CompleteDirectoryPath);
                        continue;
                    }

                    string solPath = Path.Combine(dir.CompleteDirectoryPath, NameSolverStrings.Solutions);
                    if (!ExistPath(solPath))
                        continue;

                    string pattern = "*" + NameSolverStrings.SolutionExtension;

                    list.AddRange(GetFiles(solPath, pattern));
                }
            return list;

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

        //---------------------------------------------------------------------
        public PathFinder()
        {
            InitFileSystemManager(this);
        }
        //---------------------------------------------------------------------
        public PathFinder(string company, string user)
        {
            this.company = company;
            this.user = user;
            InitFileSystemManager(this);

            if (!Init())
                throw new Exception(Messages.PathFinderInitFailed);
        }
        //---------------------------------------------------------------------
        public PathFinder(string server, string installation, string company, string user)
        {
            this.company = company;
            this.user = user;
            InitFileSystemManager(this);

            if (!Init(server, installation))
                throw new Exception(Messages.PathFinderInitFailed);
        }
        //-----------------------------------------------------------------------------
        public string GetUserNameFromPath(String sObjectFullPath)
        {
            if (IsStandardPath(sObjectFullPath))
                return string.Empty;

            FileInfo file = new FileInfo(sObjectFullPath);
            string sTemp = file.Directory.FullName;

            int nPos = sTemp.LastIndexOf('\\');
            string strUser = sTemp.Substring(sTemp.Length - (nPos + 1));

            if (string.IsNullOrEmpty(strUser))
                return string.Empty;

            // se non ha di estensione la GetNameWithExtension mi ritorna il . in fondo
            if (string.Compare(strUser.Substring(strUser.Length - 1), ".", true) == 0)
                strUser = strUser.Substring(0, strUser.Length - 1);


            // se sono in AllUsers ritorno vuoto, cioè non utente
            if (string.Compare(strUser, "AllUsers", true) == 0)
                return string.Empty;

            return strUser.Replace('.', '\\');
        }
        //-------------------------------------------------------------------------------------
        public void GetApplicationModuleNameFromPath(String sObjectFullPath, out String strApplication, out String strModule)
        {
            int nPathToken = 0;
            int nPos = 0;
            strApplication = string.Empty;
            strModule = string.Empty;

            while (nPos >= 0)
            {
                nPos = sObjectFullPath.IndexOf("\\", nPos + 1);
                if (nPos >= 0)
                    nPathToken++;
            }

            // il minimo che posso rappresentare è applicazione e modulo, quindi
            // con il tipo devo avere almeno tre segmenti di path per poterlo fare 
            if (nPathToken <= 3)
                return;

            bool bStandard = IsStandardPath(sObjectFullPath);

            String sStdCstPath;
            String sTemp = sObjectFullPath;
            sTemp = sTemp.Replace('\\', '/');
            sTemp = sTemp.ToLower();

            // le replace sono Case Sensitive!
            if (bStandard)
                sStdCstPath = GetStandardPath;
            else
            {
                sStdCstPath = GetCustomCompanyPath();
                // bug 13.914 point at the end of company name is not allowed
                if (sStdCstPath.Substring(sStdCstPath.Length - 1) == ".")
                    sStdCstPath = sStdCstPath.Substring(0, sStdCstPath.Length - 1);
            }

            sStdCstPath = sStdCstPath.Replace('\\', '/');
            sStdCstPath = sStdCstPath.ToLower();

            if (!sTemp.Contains(sStdCstPath))
            {
                //TODO LARA
                //  Debug("GetApplicationModuleNameFromPath: Invalid application path: %s\n%s\n",  sStdCstPath,  sObjectFullPath);
                return;
            }
            sTemp = sTemp.Substring(0, sStdCstPath.Length + 1);
            //sTemp.Replace(sStdCstPath + URL_SLASH_CHAR, _T(""));

            // la container la salto
            int nPosDir = sTemp.IndexOf('/');
            if (nPosDir > 0)
                sTemp = sTemp.Substring(0, nPosDir + 1);

            // l' application e module fanno parte del namespace
            nPosDir = sTemp.IndexOf('/');
            if (nPosDir > 0)
            {
                strApplication = sTemp.Substring(0, nPosDir);
                sTemp = sTemp.Substring(0, nPosDir + 1);
            }

            nPosDir = sTemp.IndexOf('/');
            if (nPosDir > 0)
            {
                strModule = sTemp.Substring(0, nPosDir);
                sTemp = sTemp.Substring(0, nPosDir + 1);
            }
        }
        //-----------------------------------------------------------------------------
        public string GetCustomCompanyPath()
        {
            return GetCustomCompanyPath(Company);
        }
        //-----------------------------------------------------------------------------
        public string GetCustomApplicationContainerPath(ApplicationType aApplicationType)
        {
            return GetCustomApplicationContainerPath(Company, aApplicationType);
        }
        /// <summary>
        /// Restituisce il path dell'applicazione nell'istanza custom
        /// </summary>
        /// <param name="applicationName">nome dell'applicazione</param>
        /// <returns>path dell'applicazione</returns>
        //---------------------------------------------------------------------
        public string GetCustomApplicationPath(string applicationName)
        {
            return GetCustomApplicationPath(Company, applicationName);
        }
        /// <summary>
        /// Restituisce la path custom di un modulo
        /// </summary>
        /// <param name="applicationName">nome applicazione</param>
        /// <param name="moduleName">nome modulo</param>
        /// <returns>la path del modulo</returns>
        //---------------------------------------------------------------------
        public string GetCustomModulePath(string applicationName, string moduleName)
        {
            return GetCustomModulePath(Company, applicationName, moduleName);
        }
        /// <summary>
        /// Restituisce Custom\Contenitore\Applicazione\Modulo\Dictionary
        /// </summary>
        /// <param name="appName">nome dell'applicazione</param>
        /// <param name="moduleName">nome del modulo</param>
        /// <param name="createDir">indica se creare la cartella se non presente</param>
        /// <returns>la pth richiesta</returns>
        //---------------------------------------------------------------------
        public string GetCustomModuleDictionaryPath(string appName, string moduleName, bool createDir)
        {
            return GetCustomModuleDictionaryPath(Company, appName, moduleName, createDir);
        }
        // Custom\AllCompanies\TaskBuilderApplication\Application\Module\DataManager
        //---------------------------------------------------------------------
        public string GetCustomDataManagerPath(string application, string module)
        {
            return GetCustomDataManagerPath(Company, application, module);
        }
        // Custom\AllCompanies\TaskBuilderApplication\Application\Module\DataManager\Default\<language>\<edition>		
        //---------------------------------------------------------------------
        public string GetCustomDataManagerDefaultPath(string application, string module, string language)
        {
            return GetCustomDataManagerDefaultPath(Company, application, module, language, edition);
        }
        // Custom\AllCompanies\TaskBuilderApplication\Application\Module\DataManager\Sample\<language>\<edition>		
        //---------------------------------------------------------------------
        public string GetCustomDataManagerSamplePath(string application, string module, string language)
        {
            return GetCustomDataManagerSamplePath(Company, application, module, language, edition);
        }
        // Standard\TaskBuilderApplication\Application\Module\DataManager\Default\<language>\<edition>		
        //---------------------------------------------------------------------
        public string GetStandardDataManagerDefaultPath(string application, string module, string language)
        {
            return GetStandardDataManagerDefaultPath(application, module, language, edition);
        }
        // Standard\TaskBuilderApplication\Application\Module\DataManager\Sample\<language>\<edition>	
        //---------------------------------------------------------------------
        public string GetStandardDataManagerSamplePath(string application, string module, string language)
        {
            return GetStandardDataManagerSamplePath(application, module, language, edition);
        }
        //---------------------------------------------------------------------
        public string GetCustomAppContainerPath(INameSpace aNameSpace)
        {
            return GetCustomAppContainerPath(Company, aNameSpace);
        }
        /// <summary>
        /// Ritorna la cartella di export profiles del modulo desiderato nella custom
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetCustomModuleObjectsPath(string companyName, string application, string module)
        {
            string customModulePath = GetCustomModulePath(application, module);
            if (customModulePath == string.Empty)
                return string.Empty;

            return Path.Combine(customModulePath, NameSolverStrings.ModuleObjects);
        }
        /// <summary>
        /// Ritorna la moduleobjects del modulo desiderato
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetCustomTemplatesPath(string companyName, string applicationName, string moduleName, bool createDir)
        {
            string path = Path.Combine(GetCustomModulePath(applicationName, moduleName), NameSolverStrings.Templates);
            if (!PathFinder.PathFinderInstance.ExistPath(path) && createDir)
                PathFinder.PathFinderInstance.CreateFolder(path, false);
            return path;

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

            ModuleInfo aModuleInfo = GetModuleInfo(aNameSpace);
            if (aModuleInfo == null)
                return null;

            return aModuleInfo.GetDocumentInfoByNameSpace(aNameSpace.FullNameSpace);
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

            return GetStandardModuleDictionaryFilePath(appName, moduleName, CultureInfo.CurrentUICulture.Name);
        }
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
        /// Restituisce Standard\Contenitore\Applicazione\Modulo\Dictionary utilizzando un nome di 
        /// tabella come punto di partenza
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetDictionaryFilePathFromTableName(string tableName)
        {
            foreach (ApplicationInfo ai in ApplicationInfos)
            {
                foreach (ModuleInfo mi in ai.Modules)
                {
                    if (mi.DatabaseObjectsInfo.TableInfoArray == null) continue;
                    foreach (TableInfo table in mi.DatabaseObjectsInfo.TableInfoArray)
                    {
                        if (string.Compare(table.Name, tableName, StringComparison.OrdinalIgnoreCase) == 0)
                            return mi.DictionaryFilePath;
                    }

                    if (mi.DatabaseObjectsInfo.ViewInfoArray == null) continue;
                    foreach (ViewInfo view in mi.DatabaseObjectsInfo.ViewInfoArray)
                    {
                        if (string.Compare(view.Name, tableName, StringComparison.OrdinalIgnoreCase) == 0)
                            return mi.DictionaryFilePath;
                    }

                    if (mi.DatabaseObjectsInfo.ProcedureInfoArray == null) continue;
                    foreach (ProcedureInfo proc in mi.DatabaseObjectsInfo.ProcedureInfoArray)
                    {
                        if (string.Compare(proc.Name, tableName, StringComparison.OrdinalIgnoreCase) == 0)
                            return mi.DictionaryFilePath;
                    }
                }
            }

            return string.Empty;
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

        /// <summary>
        /// Standard\TaskBuilderApplication\Application\Module\DataManager
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardDataManagerPath(string application, string module)
        {
            return GetApplicationModulePath(application, module) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.DataManager;
        }
        /// <summary>
        /// Standard\TaskBuilderApplication\Application\Module\DataManager
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardDataFilePath(string application, string module, string culture)
        {
            return GetStandardDataManagerPath(application, module) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.DataFile +
                NameSolverStrings.Directoryseparetor +
                culture;
        }
        /// <summary>
        /// Standard\TaskBuilderApplication\Application\Module\DataManager\Default\<language>\<edition>
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardDataManagerDefaultPath(string application, string module, string language, string edition)
        {
            return GetStandardDataManagerPath(application, module) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Default +
                NameSolverStrings.Directoryseparetor +
                language +
                NameSolverStrings.Directoryseparetor +
                edition;

        }
        /// <summary>
        /// Standard\TaskBuilderApplication\Application\Module\DataManager\Sample\<language>\<edition>	
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetStandardDataManagerSamplePath(string application, string module, string language, string edition)
        {
            return GetStandardDataManagerPath(application, module) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Sample +
                NameSolverStrings.Directoryseparetor +
                language +
                NameSolverStrings.Directoryseparetor +
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
            s = s.Replace(NameSolverStrings.Directoryseparetor, '/');
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
                    if (string.Compare(tokens[2], NameSolverStrings.AllUsers, StringComparison.OrdinalIgnoreCase) == 0)
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
                    if (string.Compare(tokens[3], NameSolverStrings.AllUsers, StringComparison.OrdinalIgnoreCase) == 0)
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
        /// <summary>
        /// Ritorna il percorso del file di versione dell'installazione
        /// </summary>
        /// <returns></returns>
        public string GetInstallationVersionPath()
        {
            return Path.Combine(standardPath, NameSolverStrings.InstallationVersion);
        }
    
        /// <summary>
        /// utilizzato data una path standard e una path custom effettua il  caricamento dei file xml
        /// trovati considerando prima quelli nella custom che sovvrascivono quelli della standar)
        /// </summary>
        /// <param name="standardDir"></param>
        /// <param name="customDir"></param>
        /// <param name="fileList"></param>
        //---------------------------------------------------------------------
        public void GetXMLFilesInPath(string standardDir, string customDir, ref List<TBFile> fileList)
        {
            if (fileList == null)
                return;

            //prima considero i file nella directory di custom del modulo
            //(sia tablename.xml che appendtablename.xml)
            List<TBFile> tempList = new List<TBFile>();
            if (ExistPath(customDir))
            {
                foreach (TBFile file in GetFiles(customDir, "*.*"))
                {
                    if (string.Compare(file.FileExtension, NameSolverStrings.XmlExtension, StringComparison.OrdinalIgnoreCase) == 0)
                        tempList.Add(file);
                }
            }

            if (ExistPath(standardDir))
            {
                foreach (TBFile file in GetFiles(standardDir, "*.*"))
                {
                    bool insert = true;
                    if (string.Compare(file.FileExtension, NameSolverStrings.XmlExtension, StringComparison.OrdinalIgnoreCase) != 0)
                        continue;
                    if (tempList.Count > 0)
                    {
                        foreach (TBFile fileName in tempList)
                        {
                            // é stato inserito giá quello presente nella custom
                            if (string.Compare(fileName.name, file.name, StringComparison.OrdinalIgnoreCase) == 0)
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

            foreach (TBFile file in tempList)
                fileList.Add(file);
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
        //---------------------------------------------------------------------
        public string GetAppInfoVersionFromSolutionName(string name)
        {
            string appsPaths = Path.Combine(standardPath, NameSolverStrings.TaskBuilderApplications);
            if (!ExistPath(appsPaths))
                return string.Empty;
            foreach (TBDirectoryInfo dir in GetSubFolders(appsPaths))
            {
                string solPath = Path.Combine(dir.CompleteDirectoryPath, NameSolverStrings.Solutions);
                if (!ExistPath(solPath))
                    continue;
                string filename = String.Format("{0}{1}", name, NameSolverStrings.SolutionExtension);
                if (!ExistFile(Path.Combine(solPath, filename)))
                    continue;
                ApplicationInfo app = GetApplicationInfoByName(Path.GetFileName(Path.GetDirectoryName(solPath)));
                if (app != null && app.ApplicationConfigInfo != null)
                    return app.ApplicationConfigInfo.Version;
            }
            return string.Empty;
        }
        /// <summary>
        /// True se il path è di tipo custom
        /// </summary>
        //-----------------------------------------------------------------------------
        public bool IsCustomPath(string aPath)
        {
            string path = GetCustomPath().ToLower();
            aPath = aPath.Replace('/', Path.PathSeparator);
            aPath = aPath.ToLower();

            return aPath.StartsWith(path);
        }
        /// <summary>
        /// Cerca la prima immagine nella directory di applicazione col nome dato
        /// </summary>
        /// <param name="imageFile">Nome del file da trovare</param>
        /// <returns>Percorso completo del file trovato, oppure null se il file non esiste</returns>
        //-----------------------------------------------------------------------------
        public string SearchImageInAppFolder(string imageFile)
        {
            string appsPaths = Path.Combine(GetStandardPath, NameSolverStrings.TaskBuilderApplications);
            if (ExistPath(appsPaths))
            {
                foreach (TBDirectoryInfo dir in GetSubFolders(appsPaths))
                {
                    if (!IsApplicationDirectory(dir.CompleteDirectoryPath))
                        continue;

                    string fullImageFilePath = Path.Combine(dir.CompleteDirectoryPath, imageFile);
                    if (ExistFile(fullImageFilePath))
                        return fullImageFilePath;
                }
            }

            return null;
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
                NameSolverStrings.Directoryseparetor +
                productName +
                NameSolverStrings.LicensedExtension;
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
        #endregion public fanction


        //-----------------------------------------------------------------------------
        public string GetServicePath()
        {
            return microareaServicesPath;
        }
        //-----------------------------------------------------------------------------
        public string GetSemaphoreFilePath()
        {
            return GetStandardPath +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.TaskBuilder +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Framework +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Application +
                NameSolverStrings.ConfigExtension;
        }

        //-----------------------------------------------------------------------------
        public string GetLicensesPath()
        {
            return
                GetStandardPath + NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Licenses;
        }
        //---------------------------------------------------------------------
        public string GetStandardTaskBuilderXmlPath()
        {
            return Path.Combine(GetStandardApplicationPath(NameSolverStrings.Framework), xmlExt);
        }


        #region private function
        //--------------------------------------------------------------------------------------------------------------------------------
        private string GetExecutablePath(ref string targetPath, string exe, string folder)
        {
            if (targetPath != null)
                return targetPath;

            //di default, ipotizzo si trovi nella cartella di esecuzione del programma
            targetPath = ""; //AppDomain.CurrentDomain.BaseDirectory.TrimEnd(NameSolverStrings.Directoryseparetor); TODO rsweb

            if (ExistFile(Path.Combine(targetPath, exe)))
                return targetPath;

            if (isRunningInsideInstallation)
            {
                //provo a vedere se mi trovo in \<Installazione>\Apps\<app>\<debug|release>
                //(solo in sviluppo)
                targetPath = Path.Combine(Path.Combine(GetAppsPath, folder), Build);
                //se esiste la cartella  \<Installazione>\Apps\<cartella>\<debug|release>, quello e' il path cercato
                if (ExistFile(Path.Combine(targetPath, exe)))
                    return targetPath;

                //provo a vedere se mi trovo nella cartella di pubblicazione
                targetPath = Path.Combine(GetAppsPath, "publish");
                if (ExistFile(Path.Combine(targetPath, exe)))
                    return targetPath;

            }

            targetPath = "";
            return targetPath;
        }

        //---------------------------------------------------------------------------
        private InstallationVersion installationVer;

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
        /// <summary>
        /// Converte uno share di rete in un path fisico 
        /// </summary>
        //---------------------------------------------------------------------
        private string TryConvertToPhysicalPath(string sharedPath)
        {
            string shared = Path.GetFileName(sharedPath);
            //using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(string.Format("Select * from Win32_Share where Name = '{0}'", shared)))
            //{
            //	using (ManagementObjectCollection moc = searcher.Get())
            //	{
            //		if (moc.Count != 0)
            //		{
            //			foreach (ManagementObject mo in moc)
            //			{
            //				string path = (string)mo["Path"];               TODO rsweb
            //				if (path != null)
            //					return path;
            //			}
            //		}
            //	}
            //}
            return sharedPath;
        }
        //-----------------------------------------------------------------------------
        private void AddDirectoryBrandFiles(string directory, Dictionary<string, TBFile> table)
        {
            string pattern = "*" + NameSolverStrings.BrandExtension;

            List<TBFile> bFiles = GetFiles(directory, pattern);
            foreach (TBFile bFile in bFiles)
                if (!table.Keys.Contains(bFile.completeFileName))
                    table.Add(bFile.completeFileName, bFile);

        }
        //---------------------------------------------------------------------
        private string FindFileInSolutionFolder(string fileName)
        {
            return FindFileInSolutionFolder(fileName, true);
        }
        //---------------------------------------------------------------------
        private string FindFileInSolutionFolder(string fileName, bool checkApplication)
        {
            string appsPaths = Path.Combine(standardPath, NameSolverStrings.TaskBuilderApplications);
            if (!ExistPath(appsPaths))
                return string.Empty;
            foreach (TBDirectoryInfo dir in GetSubFolders(appsPaths))
            {
                if (checkApplication && !IsApplicationDirectory(dir.CompleteDirectoryPath))
                {//non tradurre
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Missing application.config in: " + dir.CompleteDirectoryPath);
                    continue;
                }
                string solPath = Path.Combine(dir.CompleteDirectoryPath, NameSolverStrings.Solutions);
                if (!ExistPath(solPath))
                    continue;

                string fullFilePath = Path.Combine(solPath, fileName);
                if (ExistFile(fullFilePath))
                    return fullFilePath;
            }
            return string.Empty;
        }
       
        #endregion private function

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
                    NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.ServerConnection +
                    NameSolverStrings.ConfigExtension;
            }
        }

   


        //Spostata
        /// <summary>
        /// Ritorna il path della custom
        /// </summary>
        //-----------------------------------------------------------------------------
        public string GetCustomPath()
        {
            return customPath;
        }

        /// <summary>
        /// Ritorna il path della cartella Subscription
        /// </summary>
        //-----------------------------------------------------------------------------
        public string GetCustomSubscriptionPath()
        {
            return Path.Combine(GetCustomPath(), NameSolverStrings.Subscription);
        }
        
        public string GetCustomSubscriptionPath(string subscription)
        {
            return Path.Combine(GetCustomSubscriptionPath(), subscription);
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
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
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
        /// Ritorna il fullpath del LockFile
        ///</summary>
        //-----------------------------------------------------------------------------
        public string GetLockLogFile()
        {
            string customPath = GetCustomPath();

            if (customPath == string.Empty)
                return string.Empty;

            return customPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.LockLogFile;
        }

     

        /////<summary>
        ///// Ritorna il fullpath del file di Setting dei regression Test
        /////</summary>
        ////-----------------------------------------------------------------------------
        //public string GetRegressionTestSettingsPath()
        //{
        //    string modCusP = GetCustomApplicationPath(NameSolverStrings.AllCompanies, "MicroareaConsole");
        //    if (modCusP == string.Empty)
        //        return string.Empty;

        //    return System.IO.Path.Combine(modCusP, NameSolverStrings.RegressionTestSettings);
        //}

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
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.FileUserInfo;
        }



        ///<summary>
        /// Ritorna un array di fileInfo per ogni file di license trovato
        ///</summary>
        //-----------------------------------------------------------------------------
        public List<string> GetLicensedFiles()
        {
            string appDataPath = GetLogManAppDataPath();
            if (appDataPath == string.Empty)
                return null;


            if (!ExistPath(appDataPath))
                return null;

            try
            {
                List<string> files = new List<string>();
                foreach (TBFile file in GetFiles(appDataPath, NameSolverStrings.MaskFileLicensed))
                    files.Add(file.completeFileName);

                return files;
            }
            catch (System.Security.SecurityException exc)
            {
                Debug.WriteLine("PathFinder.GetLicensedFiles - Error: " + exc.Message);
                string error = String.Concat(Messages.ErrorAccessingLicensedFile, Environment.NewLine, exc.Message);
                diagnostic.Set(DiagnosticType.Error, error);
                return null;
            }
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
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Subscription +
                NameSolverStrings.Directoryseparetor +
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
            if (customCompanyPath == string.Empty)
                return string.Empty;

            string applicationContainerName = GetApplicationContainerName(aApplicationType);
            if (applicationContainerName == string.Empty)
            {
                diagnostic.Set(DiagnosticType.Error, string.Format(Messages.ApplicationContainerNonManaged, aApplicationType.ToString()));
                return string.Empty;
            }

            return
                customCompanyPath +
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
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
            if (string.Compare(userName, NameSolverStrings.AllUsers, StringComparison.OrdinalIgnoreCase) == 0)
                return
                    GetCustomCompanyPath(companyName) +
                    NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.Log +
                    NameSolverStrings.Directoryseparetor +
                    userName;

            return
                GetCustomCompanyPath(companyName) +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Log +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Users +
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
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

            ApplicationInfo aApplicationInfo = GetApplicationInfoByName(applicationName);
            if (aApplicationInfo == null)
                return string.Empty;

            string customApplicationContainer = GetCustomApplicationContainerPath(companyName, aApplicationInfo.ApplicationType);

            if (customApplicationContainer == string.Empty)
                return string.Empty;

            return
                customApplicationContainer +
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
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

            return customApplicationPath + NameSolverStrings.Directoryseparetor + moduleName;
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

            modulePath += NameSolverStrings.Directoryseparetor;

            if (!PathFinder.PathFinderInstance.ExistPath(modulePath) && createDir)
                PathFinder.PathFinderInstance.CreateFolder(modulePath, false);

            string dictionaryPath = modulePath + NameSolverStrings.Dictionary;

            if (createDir && !PathFinder.PathFinderInstance.ExistPath(dictionaryPath))
                PathFinder.PathFinderInstance.CreateFolder(dictionaryPath, false);

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
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Default +
                NameSolverStrings.Directoryseparetor +
                language +
                NameSolverStrings.Directoryseparetor +
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
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Sample +
                NameSolverStrings.Directoryseparetor +
                language +
                NameSolverStrings.Directoryseparetor +
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

        #endregion

        #region funzioni pubbliche


        /// <summary>
        /// Ritorna il path del file App_Data\Proxies.xml
        /// </summary>
        //---------------------------------------------------------------------------------
        public string GetProxiesFilePath()
        {
            return Path.Combine(GetLogManAppDataPath(), NameSolverStrings.ProxiesXml);
        }
        
        #endregion

        //---------------------------------------------------------------------------
        public string GetGenericBrandFilePath()
        {
            return Path.Combine(
                GetStandardPath,
                NameSolverStrings.GenericBrandFile
                );
        }

        //-----------------------------------------------------------------------------
        public List<ClientDocumentInfo> GetClientDocumentsFor(IDocument document)
        {
            List<ClientDocumentInfo> clientDocs = null;
            // ora vado a caricare i client doc e i relativi componenti
            foreach (ApplicationInfo appInfo in ApplicationInfos)
            {
                List<ClientDocumentInfo> appClientDocs = appInfo.GetClientDocumentsFor(document);
                if (appClientDocs == null)
                    continue;

                if (clientDocs == null)
                    clientDocs = new List<ClientDocumentInfo>();

                clientDocs.AddRange(appClientDocs);
            }

            return clientDocs;
        }

        //---------------------------------------------------------------------
        public string GetJsonAllApplications(string authenticationToken)
        {
            LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            if (session == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                JsonWriter jsonWriter = new JsonTextWriter(sw);

                try
                {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("applications");

                    jsonWriter.WriteStartArray();

                    foreach (ApplicationInfo applicationInfo in this.ApplicationInfos)
                    {
                        if (applicationInfo.ApplicationType != ApplicationType.TaskBuilderApplication &&
                            applicationInfo.ApplicationType != ApplicationType.TaskBuilder &&
                            applicationInfo.ApplicationType != ApplicationType.Customization)
                            continue;
                        jsonWriter.WriteStartObject();

                        jsonWriter.WritePropertyName("name");
                        jsonWriter.WriteValue(applicationInfo.Name);

                        jsonWriter.WritePropertyName("path");
                        jsonWriter.WriteValue(applicationInfo.Path);
                        jsonWriter.WriteEndObject();

                    }

                    jsonWriter.WriteEndArray();

                    jsonWriter.WriteEndObject();
                }
                catch (Exception)
                {
                }

                return sb.ToString();

            }
        }

        //---------------------------------------------------------------------
        public string GetJsonAllModulesByApplication(string authenticationToken, string appName)
        {
            LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            if (session == null)
                return string.Empty;

            ApplicationInfo app = GetApplicationInfoByName(appName);
            if (app == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                JsonWriter jsonWriter = new JsonTextWriter(sw);

                try
                {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("modules");

                    jsonWriter.WriteStartArray();

                    foreach (ModuleInfo moduleInfo in app.Modules)
                    {

                        jsonWriter.WriteStartObject();

                        jsonWriter.WritePropertyName("name");
                        jsonWriter.WriteValue(moduleInfo.Name);

                        jsonWriter.WritePropertyName("path");
                        jsonWriter.WriteValue(moduleInfo.Path);

                        jsonWriter.WritePropertyName("NameSpace");
                        jsonWriter.WriteValue(moduleInfo.NameSpace.GetNameSpaceWithoutType());

                        jsonWriter.WriteEndObject();

                    }

                    jsonWriter.WriteEndArray();

                    jsonWriter.WriteEndObject();
                }
                catch (Exception)
                {
                }

                return sb.ToString();

            }
        }
        #region EasyStudio functions

        //-----------------------------------------------------------------------------
        public string GetEasyStudioHomePath(bool createDir = false)
        {
            string path = string.Empty;
            if (EasyStudioAppsInCustom)
                path = Path.Combine(GetCustomPath(), NameSolverStrings.Subscription, NameSolverStrings.EasyStudioHome);
            else
                path = Path.Combine(GetStandardPath, NameSolverStrings.EasyStudioHome);

            if (createDir)
                CreateFolder(path, true);

            return path;
        }

        //-----------------------------------------------------------------------------
        public string GetEasyStudioCustomizationsPath(bool createDir = false)
        {
            string path = Path.Combine(GetEasyStudioHomePath(createDir), NameSolverStrings.Applications);

            if (createDir)
                CreateFolder(path, true);

            return path;
        }


        //-----------------------------------------------------------------------------
        public string GetEasyStudioCustomizationsListFor(string documentNamespace, string user, bool onlyDesignable = true)
        {
            IDocumentInfo info = GetDocumentInfo(new NameSpace(documentNamespace));
            if (onlyDesignable && !info.IsDesignable)
                return "";

            //TODOROBY
            //from namespace, read the fs/db and search for customization
            return "";
        }

        //-----------------------------------------------------------------------------
        public bool IsEasyStudioPath(string fileName)
        {
        	return fileName.StartsWith(GetEasyStudioHomePath());
        }

    #endregion

    #region da Filessystemmanager
    //-----------------------------------------------------------------------------
    public string GetServerConnectionConfig()
        {
            return fileSystemManager.GetServerConnectionConfig();
        }


        //-----------------------------------------------------------------------------
        public XmlDocument LoadXmlDocument(XmlDocument dom, string filename)
        {
            return fileSystemManager.LoadXmlDocument(dom, filename);
        }

        //-----------------------------------------------------------------------------
        public String GetFileTextFromFileName(string sFileName)
        {
            return fileSystemManager.GetFileTextFromFileName(sFileName);
        }

        //-----------------------------------------------------------------------------
        public Stream GetStream(string sFileName, bool readStream)
        {
            return fileSystemManager.GetStream(sFileName, readStream);
        }

        //-----------------------------------------------------------------------------
        public bool SaveTextFileFromStream(string sFileName, Stream sFileContent)
        {
            return fileSystemManager.SaveTextFileFromStream(sFileName, sFileContent);
        }

        //-----------------------------------------------------------------------------
        public bool SaveTextFileFromXml(string sFileName, XmlDocument dom)
        {
            return fileSystemManager.SaveTextFileFromXml(sFileName, dom);
        }

        //-----------------------------------------------------------------------------
        public byte[] GetBinaryFile(string sFileName, int nLen)
        {
            return fileSystemManager.GetBinaryFile(sFileName, nLen);
        }

        //-----------------------------------------------------------------------------
        public bool ExistFile(string sFileName)
        {
            return fileSystemManager.ExistFile(sFileName);
        }

        //-----------------------------------------------------------------------------
        public bool ExistPath(string sPathName)
        {
            return fileSystemManager.ExistPath(sPathName);
        }

        //-----------------------------------------------------------------------------
        public bool CreateFolder(string sPathName, bool bRecursive)
        {
            return fileSystemManager.CreateFolder(sPathName, bRecursive);
        }

        //-----------------------------------------------------------------------------
        public void RemoveFolder(string sPathName, bool bRecursive, bool bRemoveRoot, bool bAndEmptyParents)
        {
            fileSystemManager.RemoveFolder(sPathName, bRecursive, bRemoveRoot, bAndEmptyParents);
        }

        //-----------------------------------------------------------------------------
        public bool RemoveFile(string sFileName)
        {
            return fileSystemManager.RemoveFile(sFileName);
        }

        //-----------------------------------------------------------------------------
        public bool RenameFile(string sOldFileName, string sNewFileName)
        {
            return fileSystemManager.RenameFile(sOldFileName,  sNewFileName);
        }

        //-----------------------------------------------------------------------------
        public bool RenameFolder(string sOldName, string sNewName)
        {
            return fileSystemManager.RenameFolder(sOldName, sNewName);
        }

        //-----------------------------------------------------------------------------
        public bool CopyFile(string sOldFileName, string sNewFileName, bool bOverWrite)
        {
            return fileSystemManager.CopyFile(sOldFileName, sNewFileName, bOverWrite);
        }


        //-----------------------------------------------------------------------------
        public FileInfo GetFileStatus(string sFileName, FileInfo fs)
        {
            return fileSystemManager.GetFileStatus(sFileName, fs);
        }

        //-----------------------------------------------------------------------------
        public int[] GetFileAttributes(string sFileName)
        {
            return fileSystemManager.GetFileAttributes(sFileName);
        }

        //-----------------------------------------------------------------------------
        public bool CopyFolder(string sOldPathName, string sNewPathName, bool bOverwrite, bool bRecursive)
        {
            return fileSystemManager.CopyFolder(sOldPathName,  sNewPathName,  bOverwrite,  bRecursive);
        }


        //-----------------------------------------------------------------------------
        public List<TBDirectoryInfo> GetSubFolders(string sPathName)
        {
            return fileSystemManager.GetSubFolders(sPathName);
        }

        //-----------------------------------------------------------------------------
        public List<TBFile> GetFiles(string sPathName, string sFileExt)
        {
            return fileSystemManager.GetFiles(sPathName,  sFileExt);
        }

        //-----------------------------------------------------------------------------
        public bool GetPathContent(string sPathName, bool bFolders, out List<TBDirectoryInfo> pSubFolders, bool bFiles, string strFileExt, out List<TBFile> pFiles)
        {
            pSubFolders = new List<TBDirectoryInfo>();
            pFiles = new List<TBFile>();

            return fileSystemManager.GetPathContent(sPathName, bFolders, out  pSubFolders, bFiles, strFileExt,  out pFiles);
        }


        //-----------------------------------------------------------------------------
        public bool SaveBinaryFile(string sFileName, byte[] pBinaryContent, int nLen)
        {
            return fileSystemManager.SaveBinaryFile(sFileName, pBinaryContent, nLen);
        }

        // gets a binary file and it stores it into the temp directory
        //-----------------------------------------------------------------------------
        public string GetTemporaryBinaryFile(string sFileName)
        {
            return fileSystemManager.GetTemporaryBinaryFile(sFileName);
        }

        //-----------------------------------------------------------------------------
        public bool IsManagedByAlternativeDriver(string sName)
        {
            return fileSystemManager.IsManagedByAlternativeDriver(sName);
        }


        //-----------------------------------------------------------------------------
        public IFileSystemDriver GetFileSystemDriver()
        {
            return fileSystemManager.GetFileSystemDriver();
        }

        //-----------------------------------------------------------------------------
        public IFileSystemDriver GetAlternativeDriver()
        {
            return fileSystemManager.GetAlternativeDriver();
        }

        //-----------------------------------------------------------------------------
        public IFileSystemDriver GetAlternativeDriverIfManagedFile(string sName)
        {
            return fileSystemManager.GetAlternativeDriverIfManagedFile(sName);
        }

        //-----------------------------------------------------------------------------
        public bool Start(bool bLoadCaches  /*true*/)
        {
            return fileSystemManager.Start(bLoadCaches);
        }

        //-----------------------------------------------------------------------------
        public bool Stop(bool bLoadCaches  /*true*/)
        {
            return fileSystemManager.Stop(bLoadCaches);
        }

        //-----------------------------------------------------------------------------
        public List<string> GetAllApplicationInfo(string dir)
        {
            return fileSystemManager.GetAllApplicationInfo(dir);
        }

        //-----------------------------------------------------------------------------
        public List<string> GetAllModuleInfo(string strAppName)
        {
            return fileSystemManager.GetAllModuleInfo(strAppName);
        }

        #endregion
    }
}