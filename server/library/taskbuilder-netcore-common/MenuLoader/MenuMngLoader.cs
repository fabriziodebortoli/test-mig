using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using Microarea.Common.NameSolver;
using Microarea.Common.StringLoader;
using Microarea.Common.WebServicesWrapper;
using TaskBuilderNetCore.Interfaces;
using static Microarea.Common.MenuLoader.MenuLoader;
using Microarea.Common.SecurityLayer.SecurityLightObjects;

namespace Microarea.Common.MenuLoader
{
    #region MenuInfo class

    public class MenuInfo
    {
        #region ModuleMenuInfo class

        /// <summary>
        /// Summary description for ModuleMenuInfo.
        /// </summary>
        //============================================================================
        [Serializable]
        public class ModuleMenuInfo
        {
            private string name;
            private string title;
            private int menuViewOrder = int.MaxValue;
            private string standardMenuPath;
            private ArrayList standardMenuFiles;
            private string customAllUsersMenuPath;
            private ArrayList customAllUsersMenuFiles;
            private string customCurrentUserMenuPath;
            private ArrayList customCurrentUserMenuFiles;

            /// <summary>
            /// costruttore vuoto per il serializzatore
            /// </summary>
            public ModuleMenuInfo() { }
            //---------------------------------------------------------------------------
            public ModuleMenuInfo(string aModuleName, string aModuleTitle, int aMenuViewOrder, string aPath)
            {
                Debug.Assert(aModuleName != null && aModuleName.Length > 0, "Empty module name passed to ModuleMenuInfo constructor.");

                name = aModuleName;
                title = aModuleTitle;
                menuViewOrder = aMenuViewOrder;

                standardMenuPath = aPath;
                standardMenuFiles = null;

                customAllUsersMenuPath = String.Empty;
                customAllUsersMenuFiles = null;

                customCurrentUserMenuPath = String.Empty;
                customCurrentUserMenuFiles = null;
            }

            //---------------------------------------------------------------------------
            public ModuleMenuInfo(string aModuleName, string aModuleTitle, int aMenuViewOrder)
                : this(aModuleName, aModuleTitle, aMenuViewOrder, String.Empty)
            {
            }

            //---------------------------------------------------------------------------
            public int AddStandardMenuFile(string aFileName)
            {
                if (aFileName == null || aFileName.Length == 0)
                    return -1;

                if (standardMenuFiles == null)
                    standardMenuFiles = new ArrayList();

                return standardMenuFiles.Add(aFileName);
            }

            //---------------------------------------------------------------------------
            public int AddCustomAllUsersMenuFile(string aFileName)
            {
                if (aFileName == null || aFileName.Length == 0)
                    return -1;

                if (customAllUsersMenuFiles == null)
                    customAllUsersMenuFiles = new ArrayList();

                return customAllUsersMenuFiles.Add(aFileName);
            }

            //---------------------------------------------------------------------------
            public int AddCustomCurrentUserMenuFile(string aFileName)
            {
                if (aFileName == null || aFileName.Length == 0)
                    return -1;

                if (customCurrentUserMenuFiles == null)
                    customCurrentUserMenuFiles = new ArrayList();

                return customCurrentUserMenuFiles.Add(aFileName);
            }
            //---------------------------------------------------------------------------
            public void SetCustomMenuPaths(string aPath, string aUserName)
            {
                customAllUsersMenuPath = String.Empty;
                customCurrentUserMenuPath = String.Empty;

                if (aPath == null || aPath.Length == 0)
                    return;

                customAllUsersMenuPath = aPath + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers;
                if (aUserName != null && aUserName.Length > 0 && aUserName != NameSolverStrings.AllUsers)
                    customCurrentUserMenuPath = aPath + Path.DirectorySeparatorChar + Microarea.Common.NameSolver.PathFinder.GetUserPath(aUserName);
            }

            //---------------------------------------------------------------------------
            public string Name { get { return name; } set { name = value; } }
            //---------------------------------------------------------------------------
            public string Title { get { return (title != null && title.Length > 0) ? title : name; } set { title = value; } }
            //---------------------------------------------------------------------------
            public int MenuViewOrder { get { return menuViewOrder; } set { menuViewOrder = value; } }
            //---------------------------------------------------------------------------
            public string StandardMenuPath
            {
                get { return standardMenuPath; }
                set { standardMenuPath = (value != null && value.Length > 0 && Directory.Exists(value)) ? value : String.Empty; }
            }
            //---------------------------------------------------------------------------
            public ArrayList StandardMenuFiles { get { return standardMenuFiles; } set { standardMenuFiles = value; } }
            //---------------------------------------------------------------------------
            [XmlIgnore]
            public string CustomAllUsersMenuPath { get { return customAllUsersMenuPath; } }
            //---------------------------------------------------------------------------
            [XmlIgnore]
            public ArrayList CustomAllUsersMenuFiles { get { return customAllUsersMenuFiles; } }
            //---------------------------------------------------------------------------
            [XmlIgnore]
            public string CustomCurrentUserMenuPath { get { return customCurrentUserMenuPath; } }
            //---------------------------------------------------------------------------
            [XmlIgnore]
            public ArrayList CustomCurrentUserMenuFiles { get { return customCurrentUserMenuFiles; } }
        }


        #endregion

        #region ApplicationMenuInfo class
        /// <summary>
        /// Summary description for ApplicationMenuInfo.
        /// </summary>
        //============================================================================
        [Serializable]
        public class ApplicationMenuInfo
        {
            private string appName;
            private ApplicationType appType;
            private List<ModuleMenuInfo> modulesMenuInfos = null;

            /// <summary>
            /// costruttore vuoto per il serializzatore
            /// </summary>
            public ApplicationMenuInfo() { }
            //---------------------------------------------------------------------------
            public ApplicationMenuInfo(string aApplicationName, ApplicationType aAppType)
            {
                appName = aApplicationName;
                appType = aAppType;
            }

            //---------------------------------------------------------------------------
            public int AddModuleMenuInfos(ModuleMenuInfo aModuleMenuInfo)
            {
                if (aModuleMenuInfo == null)
                    return -1;

                if (modulesMenuInfos == null)
                    modulesMenuInfos = new List<ModuleMenuInfo>();

                if (modulesMenuInfos.Count > 0)
                {
                    int insertionIndex = -1;

                    for (int i = 0; i < modulesMenuInfos.Count; i++)
                    {
                        ModuleMenuInfo aInsertedModuleMenuInfo = (ModuleMenuInfo)modulesMenuInfos[i];
                        if (aInsertedModuleMenuInfo.MenuViewOrder >= aModuleMenuInfo.MenuViewOrder)
                        {
                            insertionIndex = i;
                            break;
                        }
                    }

                    if (insertionIndex >= 0 && insertionIndex < modulesMenuInfos.Count)
                    {
                        modulesMenuInfos.Insert(insertionIndex, aModuleMenuInfo);
                        return insertionIndex;
                    }
                }

                modulesMenuInfos.Add(aModuleMenuInfo);
                return modulesMenuInfos.Count - 1;
            }

            //---------------------------------------------------------------------------
            public void ClearModulesMenuInfos()
            {
                if (modulesMenuInfos != null)
                    modulesMenuInfos.Clear();
            }

            //---------------------------------------------------------------------------
            public string Name { get { return appName; } set { appName = value; } }

            //---------------------------------------------------------------------------
            public ApplicationType Type { get { return appType; } set { appType = value; } }

            //---------------------------------------------------------------------------
            public List<ModuleMenuInfo> ModulesMenuInfos { get { return modulesMenuInfos; } set { modulesMenuInfos = value; } }

            //---------------------------------------------------------------------------
            public bool HasModulesMenuInfos { get { return (modulesMenuInfos != null && modulesMenuInfos.Count > 0); } }
        }


        #endregion

        private const string FullMenuCachingFileName = "FullMenu.{0}.json";

        [Serializable]
        public class CachedMenuInfos
        {
            private const string StandardMenuCachingFileName = "StandardMenu.{0}.xml";
            private static XmlSerializer menuSerializer;
            /// <summary>
            /// Numero totale di moduli da analizzare
            /// </summary>
            public int TotalModules = 0;
            /// <summary>
            /// Contiene la struttura del menu delle applicazioni
            /// </summary>
            public MenuXmlParser AppsMenuXmlParser = null;
            /// <summary>
            /// Contiene la struttura del menu di TaskBuilder
            /// </summary>
            public MenuXmlParser EnvironmentXmlParser = null;
            /// <summary>
            /// Lista delle applicazioni
            /// </summary>
            public List<ApplicationMenuInfo> ApplicationsInfo = null;
            /// <summary>
            /// Tipi di comandi caricati nel menu
            /// </summary>
            public CommandsTypeToLoad CommandsTypeToLoad;
            /// <summary>
            /// Codice che identifica la configurazione di moduli attivati
            /// </summary>
            public string ConfigurationHash;
            /// <summary>
            /// Lingua dell'utente
            /// </summary>
            public string Culture;
            /// <summary>
            /// Data installazione
            /// </summary>
            public DateTime InstallationDate;
            /// <summary>
            /// Data di confronto per la validità della cache
            /// </summary>
            public DateTime CacheDate;
            /// <summary>
			/// Discrimina se la struttura e' stata letta da file
			/// (e allora non va risalvata) oppure se sata creata dinamicamente
			/// (e allora va salvata su file)
			/// </summary>
			private bool fromFile;

            private IPathFinder pathFinder;

            public bool FromFile
            {
                get { return fromFile; }
            }

            /// <summary>
            /// Costruttore senza argomenti per il serializzatore
            /// </summary>
            //---------------------------------------------------------------------------
            public CachedMenuInfos()
            {
                //se uso questo costruttore sono stato creato dal serializzatore, 
                //quindi provengo da file
                this.fromFile = true;
            }

            //---------------------------------------------------------------------------
            public CachedMenuInfos(CommandsTypeToLoad commandsTypeToLoad, string configurationHash, IPathFinder pathFinder)
            {
                //se uso questo costruttore non sono stato creato dal serializzatore, 
                //quindi non provengo da file
                this.fromFile = false;

                this.ConfigurationHash = configurationHash;
                this.CommandsTypeToLoad = commandsTypeToLoad;
                this.Culture = CultureInfo.CurrentUICulture.Name;
                this.InstallationDate = InstallationData.InstallationDate;
                this.CacheDate = InstallationData.CacheDate;
                this.pathFinder = pathFinder;
            }

            //---------------------------------------------------------------------------
            private static XmlSerializer GetSerializer()
            {
                if (menuSerializer == null)
                    menuSerializer = new XmlSerializer(typeof(CachedMenuInfos));
                return menuSerializer;
            }

            //---------------------------------------------------------------------------
            public void CalculateTotalModules()
            {
                foreach (ApplicationMenuInfo aApplication in ApplicationsInfo)
                {
                    if
                        (
                        aApplication == null ||
                        aApplication.ModulesMenuInfos == null
                        )
                        continue;
                    TotalModules += aApplication.ModulesMenuInfos.Count;
                }
            }

            //---------------------------------------------------------------------------
            public static string GetStandardMenuCachingFullFileName(string user)
            {
                string clientInstallationPath = BasePathFinder.BasePathFinderInstance.GetAppDataPath(true);
                clientInstallationPath = Path.Combine(clientInstallationPath, user);

                if (!Directory.Exists(clientInstallationPath))
                    Directory.CreateDirectory(clientInstallationPath);

                return Path.Combine(
                    clientInstallationPath,
                    string.Format(StandardMenuCachingFileName, CultureInfo.CurrentUICulture.Name) 
                    );
            }

            //---------------------------------------------------------------------------
            public static CachedMenuInfos Load(CommandsTypeToLoad commandsTypeToLoad, string configurationHash, string user)
            {
                string file = GetStandardMenuCachingFullFileName(user);
                if (!File.Exists(file))
                    return null;
                try
                {
                    XmlSerializer ser = GetSerializer();
                    FileInfo fi = new FileInfo(file);

                    using (StreamReader sw = new StreamReader(fi.OpenRead()))
                    {
                        CachedMenuInfos infos = ser.Deserialize(sw) as CachedMenuInfos;
                        //Se rispetto alla struttura salvata non corrispondono le informazioni
                        //richieste, non uso la cache e rigenero tutto dinamicamente						
                        if (infos.CommandsTypeToLoad == commandsTypeToLoad &&
                            infos.ConfigurationHash == configurationHash &&
                            infos.Culture == CultureInfo.CurrentUICulture.Name &&
                            infos.InstallationDate == InstallationData.InstallationDate &&
                            infos.CacheDate == InstallationData.CacheDate)
                            return infos;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.ToString());
                    return null;
                }
            }


            //---------------------------------------------------------------------------
            public void Save(string user)
            {
                string file = GetStandardMenuCachingFullFileName(user);
                try
                {
                    XmlSerializer ser = GetSerializer();

                    FileInfo fi = new FileInfo(file);
                    using (StreamWriter sw = fi.CreateText())
                        ser.Serialize(sw, this);
                }
                catch (Exception)
                {
                }

            //---------------------------------------------------------------------------
                }
            public static void Delete(string user)
            {
                try
                {
                    string file = GetStandardMenuCachingFullFileName(user);
                    DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(file));
                    if (di.Exists)
                        di.Delete(true);
                }
                catch (Exception)
                {
                }
            }
        }
        // la classe MenuInfo costruisce un array di file di men?da caricare

        private CachedMenuInfos cachedInfos = null;

        private MenuXmlParser favoritesXmlParser = null;
        private MenuXmlParser traceFavoritesChanges = null;
        private MenuSecurityFilter menuSecurityFilter = null;
        private string authenticationToken = null;
        private IPathFinder menuPathFinder = null;

        //---------------------------------------------------------------------------
        public event MenuParserEventHandler ScanStandardMenuComponentsStarted;
        public event MenuParserEventHandler ScanStandardMenuComponentsEnded;
        
        //----------------------------------------------------------------------------
        public event FavoritesActionEventHandler RemovingNodeFromFavorites;
        public event FavoritesActionEventHandler AddingNodeToFavorites;

        #region MenuInfo constructors

        //---------------------------------------------------------------------------
        public MenuInfo()
        {

        }

        //---------------------------------------------------------------------------
        public MenuInfo(IPathFinder aPathFinder, string authenticationToken, bool applySecurityFilter)
        {
            menuPathFinder = aPathFinder;
            this.authenticationToken = authenticationToken;
            if
                (
                applySecurityFilter &&
                menuPathFinder != null &&
                InstallationData.ServerConnectionInfo != null &&
                !string.IsNullOrEmpty(InstallationData.ServerConnectionInfo.SysDBConnectionString)
                )
            {
                menuSecurityFilter = new MenuSecurityFilter
                    (
                    InstallationData.ServerConnectionInfo.SysDBConnectionString,
                    menuPathFinder.Company,
                    menuPathFinder.User,
                    applySecurityFilter
                    );
            }

            InitSecurityLightDeniedAccesses();
        }

        //---------------------------------------------------------------------------
        public MenuInfo(PathFinder aPathFinder) : this(aPathFinder, null, false)
        {
        }

        #endregion

        #region MenuInfo private methods

        //---------------------------------------------------------------------------
        private void LoadModuleStandardMenuFiles
            (
            string aApplicationName,
            ApplicationType aApplicationType,
            ModuleMenuInfo aModule,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            if
                (
                menuPathFinder == null ||
                aModule == null ||
                aApplicationName == null ||
                aApplicationName.Length == 0 ||
                commandsTypeToLoad == CommandsTypeToLoad.Undefined
                )
                return;

            LoadMenuFilesFromArrayList(aApplicationName, aModule.Name, aModule.StandardMenuPath, aModule.StandardMenuFiles, aApplicationType, commandsTypeToLoad);
        }

        //---------------------------------------------------------------------------
        private void LoadModuleCustomMenuFiles
            (
            string aApplicationName,
            ApplicationType aApplicationType,
            ModuleMenuInfo aModule,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            if
                (
                menuPathFinder == null ||
                aModule == null ||
                aApplicationName == null ||
                aApplicationName.Length == 0 ||
                commandsTypeToLoad == CommandsTypeToLoad.Undefined
                )
                return;

            LoadMenuFilesFromArrayList(aApplicationName, aModule.Name, aModule.CustomAllUsersMenuPath, aModule.CustomAllUsersMenuFiles, aApplicationType, commandsTypeToLoad);
            LoadMenuFilesFromArrayList(aApplicationName, aModule.Name, aModule.CustomCurrentUserMenuPath, aModule.CustomCurrentUserMenuFiles, aApplicationType, commandsTypeToLoad);
        }

        //---------------------------------------------------------------------------
        private void LoadMenuFilesFromArrayList(
            string aApplicationName,
            string aModuleName,
            string filesPath,
            ArrayList menuFilesToLoad,
            ApplicationType aApplicationType,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            if
                (
                menuPathFinder == null ||
                aApplicationName == null ||
                aApplicationName.Length == 0 ||
                aModuleName == null ||
                aModuleName.Length == 0 ||
                commandsTypeToLoad == CommandsTypeToLoad.Undefined
                )
                return;

            if (menuFilesToLoad == null || menuFilesToLoad.Count == 0 || filesPath == null || filesPath.Length == 0 || !Directory.Exists(filesPath))
                return;

            if (aApplicationType == ApplicationType.TaskBuilder && EnvironmentXmlParser != null)
                EnvironmentXmlParser.LoadMenuFilesFromArrayList(aApplicationName, aModuleName, menuPathFinder, filesPath, menuFilesToLoad, commandsTypeToLoad);
            else
                AppsMenuXmlParser.LoadMenuFilesFromArrayList(aApplicationName, aModuleName, menuPathFinder, filesPath, menuFilesToLoad, commandsTypeToLoad);
        }

        //---------------------------------------------------------------------------
        private bool AddUserCreatedReportsGroup(string aApplicationName, BaseModuleInfo aModule, ArrayList userCreatedReports)
        {
            if (userCreatedReports == null || userCreatedReports.Count <= 0)
                return false;

            if (aApplicationName.IsNullOrEmpty() || aModule == null || aModule.Name.IsNullOrEmpty())
                return false;

            MenuXmlNode applicationNode = AppsMenuXmlParser.GetApplicationNodeByName(aApplicationName);
            if (applicationNode == null)
                applicationNode = AppsMenuXmlParser.CreateApplicationNode(aApplicationName, aApplicationName);
            if (applicationNode == null)
                return false;

            string groupName = GetUserReportsGroupName(aApplicationName);
            MenuXmlNode groupNode = applicationNode.GetGroupNodeByName(groupName);
            string groupTitle = MenuManagerLoaderStrings.UserReportsGroupTitle;
            if (groupNode == null)
                groupNode = AppsMenuXmlParser.CreateGroupNodeAfterAll(applicationNode, groupName, groupTitle, null);
            else
                groupNode.Title = groupTitle;
            if (groupNode == null)
                return false;

            groupNode.UserReportsGroup = true;

            MenuXmlNode menuNode = groupNode.GetMenuNodeByTitle(aModule.Title);
            if (menuNode == null)
                menuNode = AppsMenuXmlParser.CreateMenuNode(groupNode, aModule.Name, aModule.Title);
            if (menuNode == null)
                return false;

            foreach (string userReport in userCreatedReports)
            {
                string reportName = userReport;
                int extensionIndex = reportName.IndexOf(NameSolverStrings.WrmExtension);
                if (extensionIndex > 0)
                    reportName = reportName.Substring(0, extensionIndex);
                AppsMenuXmlParser.CreateReportCommandNode
                    (
                    menuNode,
                    reportName,
                    null,
                    "",
                    aApplicationName + NameSpace.TokenSeparator + aModule.Name + NameSpace.TokenSeparator + reportName,
                    ""
                    );
            }

            return true;
        }

        //---------------------------------------------------------------------------
        private static string GetUserReportsGroupName(string aApplicationName)
        {
            return aApplicationName + MenuUserGroups.DotUserReportsGroup;
        }

        //---------------------------------------------------------------------------
        // Se un report che sta sotto la Custom corrisponde ad una personalizzazione
        // di un report standard fornito da Microarea, il suo nome file coincide con
        // quello originale.
        // Se, invece, si tratta di un report ex-novo (creato dall'utente) il suo nome 
        // differisce comunque da quelli preesistenti. 
        // Quindi, per ciascun file nella Custom vado a vedere se esiste o meno un file
        // con lo stesso nome nella corrispondente sottodirectory della Standard.
        //---------------------------------------------------------------------------
        private ArrayList SearchUserCreatedReportFiles(BaseApplicationInfo aApplication, string aModuleName)
        {
            if (aApplication == null || aModuleName.IsNullOrEmpty())
                return null;

            string fullCustomPath = PathFinder.GetCustomModulePath(aApplication.Name, aModuleName);
            if (!Directory.Exists(fullCustomPath))
                return null;

            string customModuleReportPath = fullCustomPath + Path.DirectorySeparatorChar + NameSolverStrings.Report;

            DirectoryInfo customModuleReportDirInfo = new DirectoryInfo(customModuleReportPath);

            if (customModuleReportDirInfo == null || !customModuleReportDirInfo.Exists)
                return null;

            string userDirectoryName = GetValidUserDirectoryOrFileName();

            if (userDirectoryName == null || userDirectoryName.Length == 0)
                return null;

            string standardModuleReportpath = PathFinder.GetApplicationModulePath(aApplication.Name, aModuleName) + Path.DirectorySeparatorChar + NameSolverStrings.Report;
            DirectoryInfo standardModuleReportDirInfo = new DirectoryInfo(standardModuleReportpath);

            bool addAllCustomReports = (standardModuleReportDirInfo == null || !standardModuleReportDirInfo.Exists);

            ArrayList userCreatedReports = new ArrayList();

            // Prima carico i report relativi all'utente corrente. Infatti, se esiste per tale utente
            // un report con un dato nome, poi non devo considerare file di report "omonimi" relativi 
            // a tutti gli utenti (AllUsers)
            FileInfo[] currentUserCustomReportFiles = null;
            if
                (
                userDirectoryName != null &&
                userDirectoryName.Length > 0 &&
                userDirectoryName != NameSolverStrings.AllUsers &&
                Directory.Exists(customModuleReportDirInfo.FullName + Path.DirectorySeparatorChar + Microarea.Common.NameSolver.PathFinder.GetUserPath(userDirectoryName))
                )
            {
                currentUserCustomReportFiles = customModuleReportDirInfo.GetFiles(Microarea.Common.NameSolver.PathFinder.GetUserPath(userDirectoryName) + Path.DirectorySeparatorChar + "*" + NameSolverStrings.WrmExtension);
                if (currentUserCustomReportFiles != null && currentUserCustomReportFiles.Length > 0)
                {
                    foreach (FileInfo currentUserCustomReportFileInfo in currentUserCustomReportFiles)
                    {
                        string reportNamespace = string.Join(
                            ".",
                            aApplication.Name,
                            aModuleName,
                            currentUserCustomReportFileInfo.Name.Replace(currentUserCustomReportFileInfo.Extension, string.Empty)
                            );

                        // Controllo che non esista un file omonimo sotto la standard perch?in tal caso
                        // si tratterebbe di una personalizzazione di un report "ufficiale" e non di un
                        // report creato ex-novo dall'utente
                        // Inoltre, devo anche controllare se il file non ?mai utilizzato nel men? in tal caso
                        // va comunque aggiunto nei report dell'utente (anche se c'?un file omonimo nella standsard)
                        if
                            (
                            BaseApplicationInfo.IsValidReportFileName(currentUserCustomReportFileInfo.Name) &&
                            !ExistsReportMenuCommand(aApplication.Name, aModuleName, currentUserCustomReportFileInfo.Name) &&
                            !ExistInCustomizedMenu(this.AppsMenuXmlParser.MenuXmlDoc, reportNamespace, MenuXmlNode.XML_TAG_REPORT) &&
                            (
                            addAllCustomReports ||
                            !File.Exists(standardModuleReportDirInfo.FullName + Path.DirectorySeparatorChar + currentUserCustomReportFileInfo.Name)
                            )
                            )
                            userCreatedReports.Add(currentUserCustomReportFileInfo.Name);
                    }
                }
            }
            // Una volta caricati gli eventuali nuovi report per l'utente corrente posso vedere quali altri
            // report di questo tipo trovo sotto AllUsers
            if (Directory.Exists(customModuleReportDirInfo.FullName + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers))
            {
                FileInfo[] allUsersCustomReportFiles = customModuleReportDirInfo.GetFiles(NameSolverStrings.AllUsers + Path.DirectorySeparatorChar + "*" + NameSolverStrings.WrmExtension);
                if (allUsersCustomReportFiles != null && allUsersCustomReportFiles.Length > 0)
                {
                    foreach (FileInfo allUsersCustomReportFileInfo in allUsersCustomReportFiles)
                    {
                        string reportNamespace = string.Join(
                            ".",
                            aApplication.Name,
                            aModuleName,
                            allUsersCustomReportFileInfo.Name.Replace(allUsersCustomReportFileInfo.Extension, string.Empty)
                            );

                        // Prima controllo che non esista un file omonimo sotto la standard o che
                        // il report non venga mai menzionato nel men?di applicazione.
                        if
                            (
                            BaseApplicationInfo.IsValidReportFileName(allUsersCustomReportFileInfo.Name) &&
                            !ExistsReportMenuCommand(aApplication.Name, aModuleName, allUsersCustomReportFileInfo.Name) &&
                            !ExistInCustomizedMenu(this.AppsMenuXmlParser.MenuXmlDoc, reportNamespace, MenuXmlNode.XML_TAG_REPORT) &&
                            (
                            addAllCustomReports ||
                            !File.Exists(standardModuleReportDirInfo.FullName + Path.DirectorySeparatorChar + allUsersCustomReportFileInfo.Name)
                            )
                            )
                        {
                            bool ignoreReport = false;
                            // Adesso vedo se ho precedentemente trovato un report con lo stesso nome relativo all'utente corrente
                            if (currentUserCustomReportFiles != null && currentUserCustomReportFiles.Length > 0)
                            {
                                foreach (FileInfo currentUserCustomReportFileInfo in currentUserCustomReportFiles)
                                {
                                    if (String.Compare(currentUserCustomReportFileInfo.Name, allUsersCustomReportFileInfo.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        ignoreReport = true;
                                        break;
                                    }
                                }
                            }
                            if (!ignoreReport)
                                userCreatedReports.Add(allUsersCustomReportFileInfo.Name);
                        }
                    }
                }
            }

            return (userCreatedReports.Count > 0) ? userCreatedReports : null;
        }

        //---------------------------------------------------------------------------
        private bool AddUserCreatedOfficeFilesGroup(string aApplicationName, BaseModuleInfo aModule, ArrayList userCreatedOfficeFiles)
        {
            if (userCreatedOfficeFiles == null || userCreatedOfficeFiles.Count <= 0)
                return false;

            if (aApplicationName.IsNullOrEmpty() || aModule == null || aModule.Name.IsNullOrEmpty())
                return false;

            MenuXmlNode applicationNode = AppsMenuXmlParser.GetApplicationNodeByName(aApplicationName);
            if (applicationNode == null)
                applicationNode = AppsMenuXmlParser.CreateApplicationNode(aApplicationName, aApplicationName);
            if (applicationNode == null)
                return false;

            string groupName = aApplicationName + MenuUserGroups.DotUserOfficeFilesGroup;
            MenuXmlNode groupNode = applicationNode.GetGroupNodeByName(groupName);
            if (groupNode == null)
            {
                string groupTitle = MenuManagerLoaderStrings.OfficeFilesGroupTitle;
                groupNode = AppsMenuXmlParser.CreateGroupNodeAfterAll(applicationNode, groupName, groupTitle, null);
            }
            if (groupNode == null)
                return false;

            groupNode.UserOfficeFilesGroup = true;

            MenuXmlNode menuNode = groupNode.GetMenuNodeByTitle(aModule.Title);
            if (menuNode == null)
                menuNode = AppsMenuXmlParser.CreateMenuNode(groupNode, aModule.Name, aModule.Title);
            if (menuNode == null)
                return false;

            foreach (string userOfficeFullFilename in userCreatedOfficeFiles)
            {
                AppsMenuXmlParser.CreateOfficeFileCommandNode
                    (
                    menuNode,
                    aModule.Name,
                    userOfficeFullFilename
                    );
            }

            return true;
        }

        //---------------------------------------------------------------------------
        // Se un file di Office (Excel o Word) che sta sotto la Custom corrisponde ad
        // una personalizzazione di un file di Office standard fornito da Microarea, 
        // il suo nome file coincide con quello originale.
        // Se, invece, si tratta di un file ex-novo (creato dall'utente) il suo nome 
        // differisce comunque da quelli preesistenti. 
        // Quindi, per ciascun file nella Custom vado a vedere se esiste o meno un file
        // con lo stesso nome nella corrispondente sottodirectory della Standard.
        //---------------------------------------------------------------------------
        private ArrayList SearchUserCreatedOfficeFiles
            (
            BaseApplicationInfo aApplication,
            string aModuleName,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            if (aApplication == null || aModuleName.IsNullOrEmpty())
                return null;

            string customModuleOfficeFilesPath = PathFinder.GetCustomModulePath(aApplication.Name, aModuleName);
            if (!Directory.Exists(customModuleOfficeFilesPath))
                return null;

            DirectoryInfo customModuleOfficeFilesDirInfo = new DirectoryInfo(customModuleOfficeFilesPath);

            if (customModuleOfficeFilesDirInfo == null || !customModuleOfficeFilesDirInfo.Exists)
                return null;

            string userDirectoryName = GetValidUserDirectoryOrFileName();

            if (userDirectoryName == null || userDirectoryName.Length == 0)
                return null;

            string standardModuleOfficeFilespath = PathFinder.GetApplicationModulePath(aApplication.Name, aModuleName);
            DirectoryInfo standardModuleOfficeFilesDirInfo = new DirectoryInfo(standardModuleOfficeFilespath);

            bool addAllCustomOfficeFiles = (standardModuleOfficeFilesDirInfo == null || !standardModuleOfficeFilesDirInfo.Exists);

            ArrayList userCreatedOfficeFiles = new ArrayList();

            // Prima carico i file relativi all'utente corrente. Infatti, se esiste per tale utente
            // un file di Office con un dato nome, poi non devo considerare file "omonimi" relativi 
            // a tutti gli utenti (AllUsers)
            ArrayList currentUserCustomOfficeFiles = new ArrayList();

            string userPath = Microarea.Common.NameSolver.PathFinder.GetUserPath(userDirectoryName);
            if
                (
                userPath != null &&
                userPath.Length > 0 &&
                userPath != NameSolverStrings.AllUsers
                )
            {
                if
                    (
                    ((commandsTypeToLoad & CommandsTypeToLoad.ExcelItem) == CommandsTypeToLoad.ExcelItem) &&
                    Directory.Exists(customModuleOfficeFilesDirInfo.FullName + Path.DirectorySeparatorChar + NameSolverStrings.Excel + Path.DirectorySeparatorChar + userPath)
                    )
                {
                    FileInfo[] excelDocuments = customModuleOfficeFilesDirInfo.GetFiles(NameSolverStrings.Excel + Path.DirectorySeparatorChar + userPath + Path.DirectorySeparatorChar + "*" + NameSolverStrings.ExcelDocumentExtension);
                    if (excelDocuments != null && excelDocuments.Length > 0)
                        currentUserCustomOfficeFiles.AddRange(excelDocuments);
                    FileInfo[] excelTemplates = customModuleOfficeFilesDirInfo.GetFiles(NameSolverStrings.Excel + Path.DirectorySeparatorChar + userPath + Path.DirectorySeparatorChar + "*" + NameSolverStrings.ExcelTemplateExtension);
                    if (excelTemplates != null && excelTemplates.Length > 0)
                        currentUserCustomOfficeFiles.AddRange(excelTemplates);
                }

                if
                    (
                    ((commandsTypeToLoad & CommandsTypeToLoad.WordItem) == CommandsTypeToLoad.WordItem) &&
                    Directory.Exists(customModuleOfficeFilesDirInfo.FullName + Path.DirectorySeparatorChar + NameSolverStrings.Word + Path.DirectorySeparatorChar + userPath)
                    )
                {
                    FileInfo[] wordDocuments = customModuleOfficeFilesDirInfo.GetFiles(NameSolverStrings.Word + Path.DirectorySeparatorChar + userPath + Path.DirectorySeparatorChar + "*" + NameSolverStrings.WordDocumentExtension);
                    if (wordDocuments != null && wordDocuments.Length > 0)
                        currentUserCustomOfficeFiles.AddRange(wordDocuments);
                    FileInfo[] wordTemplates = customModuleOfficeFilesDirInfo.GetFiles(NameSolverStrings.Word + Path.DirectorySeparatorChar + userPath + Path.DirectorySeparatorChar + "*" + NameSolverStrings.WordTemplateExtension);
                    if (wordTemplates != null && wordTemplates.Length > 0)
                        currentUserCustomOfficeFiles.AddRange(wordTemplates);
                }

                if (currentUserCustomOfficeFiles.Count > 0)
                {
                    foreach (FileInfo currentUserCustomOfficeFileInfo in currentUserCustomOfficeFiles)
                    {
                        string officeNamespace = string.Join(
                            ".",
                            aApplication.Name,
                            aModuleName,
                            currentUserCustomOfficeFileInfo.Name.Replace(currentUserCustomOfficeFileInfo.Extension, string.Empty)
                            );

                        // Controllo che non esista un file omonimo sotto la standard perché in tal caso
                        // si tratterebbe di una personalizzazione di un file di Office "ufficiale" e non di un
                        // file creato ex-novo dall'utente
                        // Inoltre, devo anche controllare se il file non sia mai utilizzato nel menù. In tal caso
                        // va comunque aggiunto nei file di Office dell'utente (anche se c'è un file omonimo nella
                        // standard)
                        if (
                            BaseApplicationInfo.IsValidOfficeFileName(currentUserCustomOfficeFileInfo.Name) &&
                            !ExistsOfficeMenuCommand(aApplication.Name, aModuleName, currentUserCustomOfficeFileInfo.Name) &&
                            !ExistInCustomizedMenu(this.AppsMenuXmlParser.MenuXmlDoc, officeNamespace, MenuXmlNode.XML_TAG_OFFICE_ITEM) &&
                            (
                            addAllCustomOfficeFiles ||
                            !File.Exists(standardModuleOfficeFilesDirInfo.FullName + Path.DirectorySeparatorChar + currentUserCustomOfficeFileInfo.Name)
                            )
                            )
                            userCreatedOfficeFiles.Add(currentUserCustomOfficeFileInfo.Name);
                    }
                }
            }
            // Una volta caricati gli eventuali nuovi file di Office per l'utente corrente posso vedere quali altri
            // file di Office trovo sotto AllUsers
            ArrayList allUsersCustomOfficeFiles = new ArrayList();

            if
                (
                ((commandsTypeToLoad & CommandsTypeToLoad.ExcelItem) == CommandsTypeToLoad.ExcelItem) &&
                Directory.Exists(customModuleOfficeFilesDirInfo.FullName + Path.DirectorySeparatorChar + NameSolverStrings.Excel + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers)
                )
            {
                FileInfo[] excelDocuments = customModuleOfficeFilesDirInfo.GetFiles(NameSolverStrings.Excel + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers + Path.DirectorySeparatorChar + "*" + NameSolverStrings.ExcelDocumentExtension);
                if (excelDocuments != null && excelDocuments.Length > 0)
                    allUsersCustomOfficeFiles.AddRange(excelDocuments);

                FileInfo[] excelTemplates = customModuleOfficeFilesDirInfo.GetFiles(NameSolverStrings.Excel + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers + Path.DirectorySeparatorChar + "*" + NameSolverStrings.ExcelTemplateExtension);
                if (excelTemplates != null && excelTemplates.Length > 0)
                    allUsersCustomOfficeFiles.AddRange(excelTemplates);
            }

            if
                (
                ((commandsTypeToLoad & CommandsTypeToLoad.WordItem) == CommandsTypeToLoad.WordItem) &&
                Directory.Exists(customModuleOfficeFilesDirInfo.FullName + Path.DirectorySeparatorChar + NameSolverStrings.Word + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers)
                )
            {
                FileInfo[] wordDocuments = customModuleOfficeFilesDirInfo.GetFiles(NameSolverStrings.Word + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers + Path.DirectorySeparatorChar + "*" + NameSolverStrings.WordDocumentExtension);
                if (wordDocuments != null && wordDocuments.Length > 0)
                    allUsersCustomOfficeFiles.AddRange(wordDocuments);
                FileInfo[] wordTemplates = customModuleOfficeFilesDirInfo.GetFiles(NameSolverStrings.Word + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers + Path.DirectorySeparatorChar + "*" + NameSolverStrings.WordTemplateExtension);
                if (wordTemplates != null && wordTemplates.Length > 0)
                    allUsersCustomOfficeFiles.AddRange(wordTemplates);
            }

            if (allUsersCustomOfficeFiles.Count > 0)
            {
                foreach (FileInfo allUsersCustomOfficeFileInfo in allUsersCustomOfficeFiles)
                {
                    string officeNamespace = string.Join(
                            ".",
                            aApplication.Name,
                            aModuleName,
                            allUsersCustomOfficeFileInfo.Name.Replace(allUsersCustomOfficeFileInfo.Extension, string.Empty)
                            );
                    // Prima controllo che non esista un file omonimo sotto la standard o che
                    // il file di Office non venga mai menzionato nel men?di applicazione.
                    if
                        (
                        BaseApplicationInfo.IsValidOfficeFileName(allUsersCustomOfficeFileInfo.Name) &&
                        !ExistsOfficeMenuCommand(aApplication.Name, aModuleName, allUsersCustomOfficeFileInfo.Name) &&
                        !ExistInCustomizedMenu(this.AppsMenuXmlParser.MenuXmlDoc, officeNamespace, MenuXmlNode.XML_TAG_OFFICE_ITEM) &&
                        (
                        addAllCustomOfficeFiles ||
                        !File.Exists(standardModuleOfficeFilesDirInfo.FullName + Path.DirectorySeparatorChar + allUsersCustomOfficeFileInfo.Name)
                        )
                        )
                    {
                        bool ignoreOfficeFile = false;
                        // Adesso vedo se ho precedentemente trovato un report con lo stesso nome relativo all'utente corrente
                        if (currentUserCustomOfficeFiles.Count > 0)
                        {
                            foreach (FileInfo currentUserCustomOfficeFileInfo in currentUserCustomOfficeFiles)
                            {
                                if (String.Compare(currentUserCustomOfficeFileInfo.Name, allUsersCustomOfficeFileInfo.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    ignoreOfficeFile = true;
                                    break;
                                }
                            }
                        }
                        if (!ignoreOfficeFile)
                            userCreatedOfficeFiles.Add(allUsersCustomOfficeFileInfo.Name);
                    }
                }
            }

            // Riordino l'array in ordine alfabetico
            userCreatedOfficeFiles.Sort();

            return (userCreatedOfficeFiles.Count > 0) ? userCreatedOfficeFiles : null;
        }

        //---------------------------------------------------------------------------
        private bool ExistsReportMenuCommand
            (
            string aApplicationName,
            string aModuleName,
            string aReportFileName
            )
        {
            if
                (
                aApplicationName == null ||
                aApplicationName.Length == 0 ||
                aModuleName == null ||
                aModuleName.Length == 0 ||
                aReportFileName == null ||
                aReportFileName.Length == 0
                )
                return false;

            MenuXmlNode appNode = AppsMenuXmlParser.GetApplicationNodeByName(aApplicationName);
            if (appNode == null || appNode.Node == null)
                return false;

            return appNode.ExistsReportMenuCommand(aModuleName, aReportFileName);
        }

        //---------------------------------------------------------------------------
        private bool ExistsOfficeMenuCommand
            (
            string aApplicationName,
            string aModuleName,
            string aOfficeFileName
            )
        {
            if (aApplicationName.IsNullOrEmpty() || aModuleName.IsNullOrEmpty() || aOfficeFileName.IsNullOrEmpty())
                return false;

            MenuXmlNode appNode = AppsMenuXmlParser.GetApplicationNodeByName(aApplicationName);
            if (appNode == null || appNode.Node == null)
                return false;

            return appNode.ExistsOfficeMenuCommand(aModuleName, aOfficeFileName);
        }

        //---------------------------------------------------------------------------
        private void InitSecurityLightDeniedAccesses()
        {
            LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            if (session.LoginManagerSessionState != LoginManagerState.Logged)
                return;

            LoginManager.LoginManagerInstance.RefreshSecurityStatus();
            if (!LoginManager.LoginManagerInstance.IsSecurityLightEnabled())
                return;
        }

        //---------------------------------------------------------------------------
        private bool CleanSecurityLightDeniedAccesses(MenuXmlParser aParser, CommandsTypeToLoad commandTypesToClean)
        {
            LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            if (
                aParser == null ||
                session.LoginManagerSessionState != LoginManagerState.Logged ||
                !LoginManager.LoginManagerInstance.IsSecurityLightEnabled()
                )
                return false;

            SecurityLightManager.CleanDeniedAccesses(aParser, commandTypesToClean, menuPathFinder, LoginManager.LoginManagerInstance.GetSystemDBConnectionString(session.AuthenticationToken));
            return true;
        }

        //---------------------------------------------------------------------------
        private bool CleanSecurityLightDeniedAccesses(MenuXmlParser aParser)
        {
            return CleanSecurityLightDeniedAccesses(aParser, CommandsTypeToLoad.All);
        }

        #region MenuInfo standard menu caching

        //---------------------------------------------------------------------------
        private bool LoadCachedStandardMenu(CommandsTypeToLoad commandsTypeToLoad)
        {

            string configurationHash = LoginManager.LoginManagerInstance.GetConfigurationHash();
            try
            {
                cachedInfos = CachedMenuInfos.Load(commandsTypeToLoad, configurationHash, menuPathFinder.User);
                return cachedInfos != null;
            }
            catch (Exception exception)
            {
                Debug.Fail("Exception thrown in MenuInfo.LoadCachedStandardMenu: " + exception.Message);

                return false;
            }
            finally
            {
                if (cachedInfos == null)
                    cachedInfos = new CachedMenuInfos(commandsTypeToLoad, configurationHash, menuPathFinder);
            }
        }

        //---------------------------------------------------------------------------
        public void DeleteCachedStandardMenu()
        {
            CachedMenuInfos.Delete(menuPathFinder.User);
        }

        #endregion // MenuInfo standard menu caching

        #endregion // MenuInfo private methods

        #region MenuInfo private static methods

        //---------------------------------------------------------------------------
        private static string GetExternalAppCmdDescription(IPathFinder aPathFinder, MenuXmlNode aCommandNode)
        {
            if
                (
                aPathFinder == null ||
                aCommandNode == null ||
                !(
                aCommandNode.IsCommand ||
                aCommandNode.IsShortcut ||
                aCommandNode.IsRunBatch ||
                aCommandNode.IsBatchShortcut ||
                aCommandNode.IsRunFunction ||
                aCommandNode.IsFunctionShortcut
                ) ||
                aCommandNode.ItemObject == null ||
                aCommandNode.ItemObject.Length == 0
                )
                return null;

            // Se la descrizione non ?mai stata desunta dalle DocumentInfo, la propriet?ExternalDescription
            // vale null e non stringa vuota: se la descrizione ?gi?stata letta ed ?vuota la suddetta propriet?
            // restituisce comunque String.Empty
            if (aCommandNode.ExternalDescription != null)
                return aCommandNode.ExternalDescription;

            NameSpace commandNameSpace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.Document);

            IDocumentInfo documentInfo = aPathFinder.GetDocumentInfo(commandNameSpace);
            if (documentInfo == null)
                return String.Empty;

            return (string.IsNullOrEmpty(documentInfo.Description))
                ? ((documentInfo.Title != null)
                    ? documentInfo.Title
                    : String.Empty)
                : documentInfo.Description;
        }

        #endregion

        #region MenuInfo public properties

        //---------------------------------------------------------------------------
        public CachedMenuInfos GetCachedMenuInfos() { return cachedInfos; }
        //---------------------------------------------------------------------------
        public MenuXmlParser AppsMenuXmlParser { get { return cachedInfos.AppsMenuXmlParser; } }
        //---------------------------------------------------------------------------
        public MenuXmlParser EnvironmentXmlParser { get { return cachedInfos.EnvironmentXmlParser; } }
        //---------------------------------------------------------------------------
        public IPathFinder PathFinder { get { return menuPathFinder; } }
        //---------------------------------------------------------------------------
        public List<ApplicationMenuInfo> ApplicationsInfo { get { return cachedInfos.ApplicationsInfo; } }
        //---------------------------------------------------------------------------
        public ArrayList AppsMenuLoadErrorMessages { get { return AppsMenuXmlParser.LoadErrorMessages; } }
        //---------------------------------------------------------------------------
        public ArrayList FavoritesMenuLoadErrorMessages { get { return (favoritesXmlParser != null) ? favoritesXmlParser.LoadErrorMessages : null; } }
        //---------------------------------------------------------------------------
        public ArrayList EnvironmentMenuLoadErrorMessages { get { return (EnvironmentXmlParser != null) ? EnvironmentXmlParser.LoadErrorMessages : null; } }

        #endregion

        #region MenuInfo public methods
        /// <summary>
        /// Searches all application standard menu files
        /// </summary>
        //---------------------------------------------------------------------------
        public void ScanStandardMenuComponents(bool environmentStandAlone, CommandsTypeToLoad commandsTypeToLoad)
        {
            // The complete standard menu structure is stored in a specific file which 
            // holds this information from the last time the program was run.
            // The program checks whether the content of the cached file is still valid.
            // If the menu is not valid anymore (i.e. a different culture must be applied  
            // or the setup timestamp has been changed in the meanwhile), the program 
            // needs to reload the whole standard menu structure.

            if (menuPathFinder == null)
                return;

            bool isCachedStandardMenuLoaded = LoadCachedStandardMenu(commandsTypeToLoad);

            if (isCachedStandardMenuLoaded)
            {
                //necessario per sbloccare l'evento in attesa
                if (ScanStandardMenuComponentsStarted != null)
                    ScanStandardMenuComponentsStarted(this, new MenuParserEventArgs(0));
            }
            else
            {
               
				LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
				List <string> activatedModules = session.GetModules();
                if (activatedModules == null || activatedModules.Count <= 0)
                {
                    Debug.Fail("No activated modules found");
                    return;
                }

                if (ScanStandardMenuComponentsStarted != null)
                {
                    int modulesTotalCount = 0;
                    //prima li conto tutti per sapere il totale a beneficio della progress bar
                    foreach (BaseApplicationInfo appInfo in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
                    {
                        if (appInfo.ApplicationType != ApplicationType.TaskBuilderApplication && appInfo.ApplicationType != ApplicationType.TaskBuilder)
                            continue;

                        foreach (BaseModuleInfo moduleInfo in appInfo.Modules)
                        {
                            bool moduleFound = activatedModules.Exists((s) => { return string.Compare(s, appInfo.Name + "." + moduleInfo.Name, StringComparison.OrdinalIgnoreCase) == 0; });
                            if (moduleFound)
                                modulesTotalCount++;
                        }
                    }
                    ScanStandardMenuComponentsStarted(this, new MenuParserEventArgs(modulesTotalCount));
                }

                cachedInfos.ApplicationsInfo = new List<ApplicationMenuInfo>();

                //poi inizio a leggere le informazioni
                foreach (BaseApplicationInfo appInfo in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
                {
                    if (
                        appInfo.ApplicationType != ApplicationType.TaskBuilderApplication &&
                        appInfo.ApplicationType != ApplicationType.TaskBuilder &&
                        appInfo.ApplicationType != ApplicationType.Customization &&
                        appInfo.ApplicationType != ApplicationType.Standardization &&
                        appInfo.ApplicationType != ApplicationType.StandardModuleWrapper
                        )
                        continue;

                    ApplicationMenuInfo aApplication = new ApplicationMenuInfo(appInfo.Name, appInfo.ApplicationType);
                    aApplication.ClearModulesMenuInfos();

                    if (appInfo.Modules != null && aApplication != null)
                    {
                        foreach (BaseModuleInfo moduleInfo in appInfo.Modules)
                        {
                            bool moduleFound = activatedModules.Exists((s) => { return string.Compare(s, appInfo.Name + "." + moduleInfo.Name, StringComparison.OrdinalIgnoreCase) == 0; });
                            if (!moduleFound)
                                continue;

                            ModuleMenuInfo aModule = new ModuleMenuInfo(moduleInfo.Name, moduleInfo.ModuleConfigInfo.Title, moduleInfo.ModuleConfigInfo.MenuViewOrder);

                            string fullStandardPath = BasePathFinder.BasePathFinderInstance.GetApplicationModulePath(aApplication.Name, aModule.Name);

                            DirectoryInfo moduleMenuDirInfo = new DirectoryInfo(fullStandardPath + Path.DirectorySeparatorChar + NameSolverStrings.Menu);

                            if (
                                appInfo.ApplicationType == ApplicationType.Customization ||
                                appInfo.ApplicationType == ApplicationType.Standardization ||
                                appInfo.ApplicationType == ApplicationType.StandardModuleWrapper
                                )
                            {
                                aModule.StandardMenuPath = moduleMenuDirInfo.FullName;
                                //Do per scontato che menuPathFinder.User abbia un valore sensato perchè se invece è vuoto allora c'e un problema a monte.
                                string workingMenuPath = Path.Combine(moduleMenuDirInfo.FullName, menuPathFinder.User);

                                DirectoryInfo workingDirInfo = new DirectoryInfo(workingMenuPath);
                                FileInfo[] menuFiles = null;

                                if (workingDirInfo.Exists)
                                {
                                    menuFiles = workingDirInfo.GetFiles("*" + NameSolverStrings.MenuExtension);
                                    aModule.StandardMenuPath = workingDirInfo.FullName;
                                }
                                if ((menuFiles == null || menuFiles.Length <= 0) && moduleMenuDirInfo.Exists)
                                {
                                    menuFiles = moduleMenuDirInfo.GetFiles("*" + NameSolverStrings.MenuExtension);
                                    aModule.StandardMenuPath = moduleMenuDirInfo.FullName;
                                }

                                if (menuFiles == null || menuFiles.Length <= 0)
                                    continue;

                                foreach (FileInfo aMenuFileInfo in menuFiles)
                                    aModule.AddStandardMenuFile(aMenuFileInfo.Name);
                            }
                            else if (moduleMenuDirInfo != null && moduleMenuDirInfo.Exists)
                            {
                                aModule.StandardMenuPath = moduleMenuDirInfo.FullName;
                                // Se c'?la directory Menu e contiene dei file .menu inserisco
                                // il modulo nella lista
                                FileInfo[] menuFiles = moduleMenuDirInfo.GetFiles("*" + NameSolverStrings.MenuExtension);
                                if (menuFiles.Length <= 0)
                                    continue;

                                foreach (FileInfo aMenuFileInfo in menuFiles)
                                    aModule.AddStandardMenuFile(aMenuFileInfo.Name);
                            }

                            aApplication.AddModuleMenuInfos(aModule);
                        }
                    }

                    ApplicationsInfo.Add(aApplication);
                }

                cachedInfos.CalculateTotalModules();
            }

            ScanStandardMenuComponentsEnded?.Invoke(this, null);
        }

        //---------------------------------------------------------------------------
        public void ScanStandardMenuComponents(CommandsTypeToLoad commandsTypeToLoad)
        {
            ScanStandardMenuComponents(false, commandsTypeToLoad);
        }

        //---------------------------------------------------------------------------
        public void ScanStandardMenuComponents()
        {
            ScanStandardMenuComponents(CommandsTypeToLoad.All);
        }

        /// <summary>
        /// Searches all application custom menu files
        /// </summary>
        //---------------------------------------------------------------------------
        public void ScanCustomMenuComponents()
        {
            if (menuPathFinder == null)
                return;

            string userName = (menuPathFinder.User != null && menuPathFinder.User.Length > 0) ? menuPathFinder.User : NameSolverStrings.AllUsers;

           
            foreach (ApplicationMenuInfo aApplication in ApplicationsInfo)
            {
                if
                    (
                        aApplication == null ||
                        aApplication.ModulesMenuInfos == null ||
                        aApplication.ModulesMenuInfos.Count == 0
                    )
                    continue;

                foreach (ModuleMenuInfo aModule in aApplication.ModulesMenuInfos)
                {
                    string fullCustomPath = PathFinder.GetCustomModulePath(aApplication.Name, aModule.Name);
                    DirectoryInfo moduleCustomMenuDirInfo = new DirectoryInfo(fullCustomPath + Path.DirectorySeparatorChar + NameSolverStrings.Menu);
                    if (moduleCustomMenuDirInfo != null && moduleCustomMenuDirInfo.Exists)
                    {
                        aModule.SetCustomMenuPaths(moduleCustomMenuDirInfo.FullName, userName);

                        FileInfo[] menuFiles = null;
                        if (Directory.Exists(aModule.CustomAllUsersMenuPath))
                        {
                            menuFiles = moduleCustomMenuDirInfo.GetFiles(NameSolverStrings.AllUsers + Path.DirectorySeparatorChar + "*" + NameSolverStrings.MenuExtension);
                            if (menuFiles.Length > 0)
                            {
                                foreach (FileInfo aMenuFileInfo in menuFiles)
                                    aModule.AddCustomAllUsersMenuFile(aMenuFileInfo.Name);
                            }
                        }
                        if (userName != null && userName.Length > 0 && userName != NameSolverStrings.AllUsers)
                        {
                            if (Directory.Exists(aModule.CustomCurrentUserMenuPath))
                            {
                                menuFiles = moduleCustomMenuDirInfo.GetFiles(Microarea.Common.NameSolver.PathFinder.GetUserPath(userName) + Path.DirectorySeparatorChar + "*" + NameSolverStrings.MenuExtension);
                                if (menuFiles.Length > 0)
                                {
                                    foreach (FileInfo aMenuFileInfo in menuFiles)
                                        aModule.AddCustomCurrentUserMenuFile(aMenuFileInfo.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------------
        public MenuXmlParser LoadAllMenuFiles()
        {
            return LoadAllMenuFiles(false);
        }

        //---------------------------------------------------------------------------
        public MenuXmlParser LoadAllMenuFiles(bool ignoreAllSecurityChecks, bool cacheStandardMenu)
        {
            return LoadAllMenuFiles(CommandsTypeToLoad.All, ignoreAllSecurityChecks, cacheStandardMenu);
        }

        //---------------------------------------------------------------------------
        public MenuXmlParser LoadAllMenuFiles(bool ignoreAllSecurityChecks)
        {
            return LoadAllMenuFiles(CommandsTypeToLoad.All, ignoreAllSecurityChecks, true);
        }

        //---------------------------------------------------------------------------
        public MenuXmlParser LoadAllMenuFiles(CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks, bool cacheStandardMenu)
        {
            LoadAllMenuFiles(false, commandsTypeToLoad, ignoreAllSecurityChecks, cacheStandardMenu);

            return (AppsMenuXmlParser.MenuXmlDoc != null) ? AppsMenuXmlParser : null;
        }

        //---------------------------------------------------------------------------
        public MenuXmlParser LoadAllMenuFiles(CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks)
        {
            return LoadAllMenuFiles(commandsTypeToLoad, ignoreAllSecurityChecks, true);
        }

        //---------------------------------------------------------------------------
        public MenuXmlParser LoadAllMenuFiles(CommandsTypeToLoad commandsTypeToLoad)
        {
            return LoadAllMenuFiles(commandsTypeToLoad, false);
        }

        //---------------------------------------------------------------------------
        public void LoadAllMenuFiles(bool environmentStandAlone, CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks)
        {
            LoadAllMenuFiles(environmentStandAlone, commandsTypeToLoad, ignoreAllSecurityChecks, true);
        }

        //---------------------------------------------------------------------------
        public void LoadAllMenuFiles(bool environmentStandAlone, CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks, bool cacheStandardMenu)
        {
            if (menuPathFinder == null)
                return;

           
            //se sono stato letto da cache non ricalcolo le informazioni
            //tanto nulla e' cambiato
            if (!cachedInfos.FromFile)
            {
                cachedInfos.AppsMenuXmlParser = new MenuXmlParser();

                if (environmentStandAlone)
                    cachedInfos.EnvironmentXmlParser = new MenuXmlParser();

                foreach (ApplicationMenuInfo aApplication in ApplicationsInfo)
                {
                    if (!aApplication.HasModulesMenuInfos)
                        continue;

                    foreach (ModuleMenuInfo aModule in aApplication.ModulesMenuInfos)
                    {
                        LoadModuleStandardMenuFiles(aApplication.Name, aApplication.Type, aModule, commandsTypeToLoad);
                    }
                }

                if (cacheStandardMenu)
                    cachedInfos.Save(menuPathFinder.User);
            }
            foreach (ApplicationMenuInfo aApplication in ApplicationsInfo)
            {
                if (!aApplication.HasModulesMenuInfos)
                    continue;

                foreach (ModuleMenuInfo aModule in aApplication.ModulesMenuInfos)
                {
                    if (
                        (aModule.CustomAllUsersMenuFiles == null || aModule.CustomAllUsersMenuFiles.Count == 0) &&
                        (aModule.CustomCurrentUserMenuFiles == null || aModule.CustomCurrentUserMenuFiles.Count == 0)
                        )
                        continue;

                    LoadModuleCustomMenuFiles(aApplication.Name, aApplication.Type, aModule, commandsTypeToLoad);
                }
            }

            //leggo i documentobjects in un task a parte
            Task<List<DocumentInfo>> docInfoTask = Task.Factory.StartNew<List<DocumentInfo>>(() => { return GetCustomDocuments(this.AppsMenuXmlParser, commandsTypeToLoad); });

            // Adesso che ho caricato i menu di TUTTE le applicazioni ed il mio menu completo, devo
            // andare a vedere se ci sono dei nuovi report o dei file di Office costruiti ex-novo dall'utente.
            // Lo faccio DOPO aver caricato i menù di tutti i moduli di ciascuna applicazione di modo che il
            // gruppo dei report dell'utente compaia sempre per ultimo!!!
            foreach (BaseApplicationInfo aApplication in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
            {
                //report dell'utente
                if ((commandsTypeToLoad & CommandsTypeToLoad.Report) == CommandsTypeToLoad.Report)
                    SearchAndAddUserCreatedReportsGroup(aApplication);

                //file di office dell'utente
                if ((commandsTypeToLoad & CommandsTypeToLoad.OfficeItem) != CommandsTypeToLoad.Undefined)
                    SearchAndAddUserCreatedOfficeFilesGroup(aApplication, commandsTypeToLoad);
            }

            foreach (DocumentInfo di in docInfoTask.Result)
                AddUserCreatedDocumentsGroup(di);

            if (!ignoreAllSecurityChecks)
            {
                CleanSecurityLightDeniedAccesses(AppsMenuXmlParser, commandsTypeToLoad);
                if (environmentStandAlone)
                    CleanSecurityLightDeniedAccesses(EnvironmentXmlParser, commandsTypeToLoad);

                if (menuSecurityFilter != null)
                {
                    menuSecurityFilter.Filter(cachedInfos.AppsMenuXmlParser);
                    if (environmentStandAlone)
                        menuSecurityFilter.Filter(cachedInfos.EnvironmentXmlParser);
                }
            }
        }


        //---------------------------------------------------------------------------
        public static string GetFullMenuCachingFullFileName(string user)
        {
            string clientInstallationPath = BasePathFinder.BasePathFinderInstance.GetAppDataPath(true);
            clientInstallationPath = Path.Combine(clientInstallationPath, user);

            if (!Directory.Exists(clientInstallationPath))
                Directory.CreateDirectory(clientInstallationPath);

            return Path.Combine(
                clientInstallationPath,
                string.Format(FullMenuCachingFileName, CultureInfo.CurrentUICulture.Name)
                );
        }

        /// <summary>
        /// Se trovo report costruiti dall'utente aggiungo un gruppo denominato "Report dell'utente"
        /// nel menù
        /// Tale gruppo è suddiviso per moduli (cioè per ciascuna directory di modulo che contiene
        /// nuovi report creo una relativa voce di menù avente come titolo il nome del modulo stesso). 
        /// All'interno di ciascuna di tali voci si hanno come comandi i report suddetti.
        /// I comandi hanno come titolo il nome file del report.
        /// </summary>
        //---------------------------------------------------------------------------
        private void SearchAndAddUserCreatedReportsGroup(BaseApplicationInfo aApplication)
        {
            foreach (BaseModuleInfo aModule in aApplication.Modules)
            {
                ArrayList userCreatedReports = SearchUserCreatedReportFiles(aApplication, aModule.Name);
                AddUserCreatedReportsGroup(aApplication.Name, aModule, userCreatedReports);
            }
        }

        /// <summary>
        /// // Se trovo file di Office creati dall'utente aggiungo un gruppo denominato "File di Office
        /// dell'utente" nel menù
        /// </summary>
        //---------------------------------------------------------------------------
        private void SearchAndAddUserCreatedOfficeFilesGroup(BaseApplicationInfo aApplication, CommandsTypeToLoad commandsTypeToLoad)
        {
            foreach (BaseModuleInfo aModule in aApplication.Modules)
            {
                ArrayList userCreatedOfficeFiles = SearchUserCreatedOfficeFiles(aApplication, aModule.Name, commandsTypeToLoad);
                AddUserCreatedOfficeFilesGroup(aApplication.Name, aModule, userCreatedOfficeFiles);
            }
        }


        //---------------------------------------------------------------------------
        public void RefreshUserDocumentsGroup()
        {
            CommandsTypeToLoad commandsTypeToLoad = CommandsTypeToLoad.Form;
            //leggo i documentobjects in un task a parte
            Task<List<DocumentInfo>> docInfoTask = Task.Factory.StartNew<List<DocumentInfo>>(() => { return GetCustomDocuments(cachedInfos.AppsMenuXmlParser, commandsTypeToLoad); });

            //Ripulisco tutti i nodi che fanno parte della UserDocumentsGroups
            ClearUserDocumentsGroups();

            foreach (DocumentInfo di in docInfoTask.Result)
                AddUserCreatedDocumentsGroup(di);
        }

        /// <summary>
        /// Questo metodo cancella tutti i nodi 
        /// </summary>
        //---------------------------------------------------------------------------
        public void ClearUserDocumentsGroups()
        {
            MenuXmlNode root = AppsMenuXmlParser.Root as MenuXmlNode;
            if (root == null)
                return;

            string xPath = string.Format("//{0}/{1}/{2}",
                MenuXmlNode.XML_TAG_MENU_ROOT,
                MenuXmlNode.XML_TAG_APPLICATION,
                MenuXmlNode.XML_TAG_GROUP);

            MenuXmlNodeCollection nodeList = root.SelectMenuNodes(xPath);
            foreach (MenuXmlNode item in nodeList)
            {
                if (item.Node.Attributes[MenuXmlNode.XML_ATTRIBUTE_NAME] == null)
                    continue;

                string s = item.Node.Attributes[MenuXmlNode.XML_ATTRIBUTE_NAME].Value.ToString();
                if (s.ContainsNoCase(MenuUserGroups.DotUserDocumentsGroup))
                    item.ClearChilds();
            }
        }

        //---------------------------------------------------------------------------
        public void RefreshUserReportsGroup()
        {
            foreach (BaseApplicationInfo bai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
                ClearUserGroup(bai.Name, GetUserReportsGroupName(bai.Name));

            foreach (BaseApplicationInfo aApplication in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
                SearchAndAddUserCreatedReportsGroup(aApplication);
        }

        //---------------------------------------------------------------------------
        private static List<DocumentInfo> GetCustomDocuments(
            MenuXmlParser xmlParser,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            List<DocumentInfo> dynamicDocuments = new List<DocumentInfo>();

            if (
                (commandsTypeToLoad & CommandsTypeToLoad.Form) != CommandsTypeToLoad.Form &&
                (commandsTypeToLoad & CommandsTypeToLoad.Batch) != CommandsTypeToLoad.Batch
                )
                return dynamicDocuments;

            string nodeTypeName = null;
            foreach (BaseApplicationInfo bai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
            {
                //La standardizzazione fatta con EasyBuilder viene qui saltata perchè i suoi documenti sono messi
                //a menù dai relativi file di menù.
                //Serve anche per le logiche di attivazione, altirmenti, nel caso in cui un eventuale modulo non fosse attivato,
                //il relativo documento verrebbe caricato nel gruppo "Custom Documents".
                if (bai.ApplicationType == ApplicationType.Standardization || bai.ApplicationType == ApplicationType.StandardModuleWrapper)
                    continue;

                foreach (BaseModuleInfo bmi in bai.Modules)
                {
                    if (bmi.Documents == null || bmi.Documents.Count == 0)
                        continue;

                    foreach (DocumentInfo di in bmi.Documents)
                    {
                        if (!di.IsDynamic && di.TemplateNamespace == null)
                            continue;

                        if (di.NameSpace == null)
                            continue;

                        if (di.IsDataEntry)
                            nodeTypeName = MenuXmlNode.XML_TAG_DOCUMENT;
                        else if (di.IsBatch)
                            nodeTypeName = MenuXmlNode.XML_TAG_BATCH;
                        else
                        {
                            nodeTypeName = null;
                            continue;
                        }
                        if (!ExistInCustomizedMenu(xmlParser.MenuXmlDoc, di.NameSpace.GetNameSpaceWithoutType(), nodeTypeName))
                            dynamicDocuments.Add(di);
                    }
                }
            }
            return dynamicDocuments;
        }

        //---------------------------------------------------------------------------
        private static bool ExistInCustomizedMenu(XmlDocument menuXml, string nameSpace, string nodeTypeName)
        {
            if (
                menuXml == null ||
                String.IsNullOrWhiteSpace((nameSpace))
                )
                return false;

            //Abbiamo usato la IndexOf perchè la Regex in questo caso è 5 volte più lenta da test fatti.
            //se non ne trovo, allora torno false
            bool found = menuXml.OuterXml.IndexOf(nameSpace) > -1;
            if (!found)
                return false;

            //se ne trovo, devo verificare che sia dello stesso tipo (per evitare che namespace omonimi
            //di report o officeitem si escludano a vicenda
            try
            {
                //cerco tutti i nodi Object che matchano quel namespace
                XmlNodeList nodeList =
                    menuXml.SelectNodes(string.Format("//{0}[text() = '{1}']", MenuXmlNode.XML_TAG_OBJECT, nameSpace));

                foreach (XmlNode node in nodeList)
                {
                    if (node.ParentNode == null)
                        continue;

                    //per ogni nodo verifico se il parent node è dello stesso tipo
                    if (node.ParentNode.Name.CompareNoCase(nodeTypeName))
                        return true;
                }
            }
            catch
            {
            }
            return false;
        }

        //---------------------------------------------------------------------------
        private void ClearUserGroup(string aApplicationName, string groupName)
        {
            MenuXmlNode applicationNode = AppsMenuXmlParser.GetApplicationNodeByName(aApplicationName);
            if (applicationNode == null)
                return;
            MenuXmlNode groupNode = applicationNode.GetGroupNodeByName(groupName);
            if (groupNode == null)
                return;

            groupNode.ClearChilds();
        }
        //---------------------------------------------------------------------------
        private void AddUserCreatedDocumentsGroup(DocumentInfo di)
        {
            string aApplicationName = di.NameSpace.Application; //di.OwnerModule.ParentApplicationName;
            MenuXmlNode applicationNode = AppsMenuXmlParser.GetApplicationNodeByName(aApplicationName);
            if (applicationNode == null)
                applicationNode = AppsMenuXmlParser.CreateApplicationNode(aApplicationName, aApplicationName);
            if (applicationNode == null)
                return;

            string groupName = GetUserDocumentGroupName(aApplicationName);
            MenuXmlNode groupNode = applicationNode.GetGroupNodeByName(groupName);
            string groupTitle = MenuManagerLoaderStrings.DocumentsGroupTitle;
            if (groupNode == null)
                groupNode = AppsMenuXmlParser.CreateGroupNodeAfterAll(applicationNode, groupName, groupTitle, null);
            else
                groupNode.Title = groupTitle;

            if (groupNode == null)
                return;

            string moduleName = di.NameSpace.Module/*di.OwnerModule.Name*/;
            IBaseModuleInfo bmi = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(aApplicationName, moduleName);
            if (bmi == null)
                return;
            MenuXmlNode menuNode = groupNode.GetMenuNodeByTitle(bmi.Title);
            if (menuNode == null)
                menuNode = AppsMenuXmlParser.CreateMenuNode(groupNode, moduleName, bmi.Title);
            if (menuNode == null)
                return;

            AppsMenuXmlParser.CreateDocumentCommandNode
                (
                menuNode,
                di.Title,
                di.Description,
                di.NameSpace.GetNameSpaceWithoutType(),
                ""
                );

        }

        //---------------------------------------------------------------------------
        private static string GetUserDocumentGroupName(string aApplicationName)
        {
            return aApplicationName + MenuUserGroups.DotUserDocumentsGroup;
        }

        //---------------------------------------------------------------------------
        public bool LoadFavoritesMenu(CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks)
        {
            favoritesXmlParser = null;

            string allUsersPrefFileName = GetAllUsersFavoritesMenuFileName();
            string userPrefFileName = GetUserFavoritesMenuFileName();
            if (String.Compare(userPrefFileName, allUsersPrefFileName) == 0)
                userPrefFileName = String.Empty;

            if ((allUsersPrefFileName == null || allUsersPrefFileName.Length == 0) && (userPrefFileName == null || userPrefFileName.Length == 0))
                return false;

            MenuXmlParser tmpfavoritesXmlParser = new MenuXmlParser();

            if (!File.Exists(allUsersPrefFileName))// non esiste AllUsers.menu
            {
                if (userPrefFileName == null || userPrefFileName.Length == 0 || !File.Exists(userPrefFileName))// non esistono men?preferiti 
                    return false;

                if (!tmpfavoritesXmlParser.LoadFavoritesMenuFile(this, userPrefFileName, commandsTypeToLoad, false))
                    return false;

                favoritesXmlParser = tmpfavoritesXmlParser;

                if (!ignoreAllSecurityChecks)
                {
                    CleanSecurityLightDeniedAccesses(favoritesXmlParser, commandsTypeToLoad);

                    if (menuSecurityFilter != null)
                        menuSecurityFilter.Filter(favoritesXmlParser);
                }

                return true;
            }

            // carico AllUsers.menu
            bool menuLoaded = tmpfavoritesXmlParser.LoadFavoritesMenuFile(this, allUsersPrefFileName, commandsTypeToLoad, false);

            // carico <user_name>.menu
            if (userPrefFileName != null && userPrefFileName.Length > 0 && File.Exists(userPrefFileName))
                menuLoaded = tmpfavoritesXmlParser.LoadFavoritesMenuFile(this, userPrefFileName, commandsTypeToLoad, false) || menuLoaded;

            favoritesXmlParser = tmpfavoritesXmlParser;

            if (!ignoreAllSecurityChecks)
            {
                CleanSecurityLightDeniedAccesses(favoritesXmlParser, commandsTypeToLoad);

                if (menuSecurityFilter != null)
                    menuSecurityFilter.Filter(favoritesXmlParser);
            }

            return menuLoaded;
        }

        //---------------------------------------------------------------------------
        public bool SaveUserFavoritesMenu()
        {
            //if (favoritesXmlParser == null || favoritesXmlParser.XmlDocument == null)
            //    return false;

            if (traceFavoritesChanges == null || traceFavoritesChanges.MenuActionsNode == null)
                return true; // non sono state apportate modifiche al men?

            string userPrefFileName = GetUserFavoritesMenuFileName();
            if (userPrefFileName == null || userPrefFileName.Length == 0)
                return false;

            try
            {
                // In favoritesXmlParser ci sono i men?caricati da AllUsers.menu e da <user_name>.menu,
                // nonch?le modifiche apportate loro in modo interattivo: se chiamassi il metodo Save
                // su favoritesXmlParser farei confluire il men?contenuto in AllUsers.menu dentro a 
                // <user_name>.menu.... devo, quindi, partire dal men?originale, aggiungere i nodi che
                // sono stati aggiunti interattivamente e anche quelli cancellati.
                // Queste informazioni sono state tracciate in traceFavoritesChanges.

                MenuXmlParser tmpfavoritesXmlParser = new MenuXmlParser();

                // carico il file <user_name>.menu preesistente
                if (!File.Exists(userPrefFileName))
                {
                    // Se il file non esiste devo controllare che almeno esista la sua
                    // directory, altrimenti il salvataggio solleva un'eccezione
                    string userPrefFilePath = GetFavoritesMenuPath();
                    if (!Directory.Exists(userPrefFilePath))
                        Directory.CreateDirectory(userPrefFilePath);
                }
                else
                    tmpfavoritesXmlParser.LoadFavoritesMenuFile(this, userPrefFileName, CommandsTypeToLoad.All, true);

                tmpfavoritesXmlParser.ApplyMenuChanges(traceFavoritesChanges.MenuActionsNode, CommandsTypeToLoad.All, true);


                FileInfo file = new FileInfo(userPrefFileName);
                using (StreamWriter sw = file.CreateText())
                {
                    tmpfavoritesXmlParser.MenuXmlDoc.Save(sw);
                }
                return true;
            }
            catch (Exception exception)
            {
                Debug.Fail("Exception raised setting the MenuInfo.SaveUserFavoritesMenu property: " + exception.Message);
                return false;
            }
        }

        //---------------------------------------------------------------------------
        public bool RemoveNodeFromFavorites(MenuXmlNode aMenuNodeToRemove)
        {
            if (favoritesXmlParser == null || aMenuNodeToRemove == null)
                return false;

            if (RemovingNodeFromFavorites != null)
                RemovingNodeFromFavorites(this, aMenuNodeToRemove);

            // devo fare il trace prima di cancellare il nodo altrimenti non riesco pi?
            // a ricavarne alcuna informazione...
            TraceFavoritesMenuOperation(aMenuNodeToRemove, MenuXmlNode.MenuActionType.Remove, false);

            return favoritesXmlParser.RemoveNode(aMenuNodeToRemove);
        }

        //---------------------------------------------------------------------------
        public bool AddMenuNodeToFavorites(MenuXmlNode aMenuNodeToAdd, MenuXmlParser originalMenuParser)
        {
            return AddMenuNodeToFavorites(aMenuNodeToAdd, originalMenuParser, CommandsTypeToLoad.All);
        }

        //---------------------------------------------------------------------------
        public bool AddMenuNodeToFavorites
            (
            MenuXmlNode aMenuNodeToAdd,
            MenuXmlParser originalMenuParser,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            if (aMenuNodeToAdd == null || !aMenuNodeToAdd.IsMenu)
                return false;

            if (AddingNodeToFavorites != null)
                AddingNodeToFavorites(this, aMenuNodeToAdd);

            MenuXmlParser tmpfavoritesXmlParser = new MenuXmlParser(favoritesXmlParser);

            if (tmpfavoritesXmlParser.AddMenuNode(aMenuNodeToAdd, true, commandsTypeToLoad) == null)
                return false;

            tmpfavoritesXmlParser.CopyNodeImageInfos(aMenuNodeToAdd, originalMenuParser, this.PathFinder);

            favoritesXmlParser = tmpfavoritesXmlParser;

            TraceFavoritesMenuOperation(aMenuNodeToAdd, MenuXmlNode.MenuActionType.Add, true);

            return true;
        }

        //---------------------------------------------------------------------------
        public bool AddCommandNodeToFavorites
            (
            MenuXmlNode aCmdNodeToAdd,
            MenuXmlParser originalMenuParser
            )
        {
            return AddCommandNodeToFavorites(aCmdNodeToAdd, originalMenuParser, CommandsTypeToLoad.All);
        }

        //---------------------------------------------------------------------------
        public bool AddCommandNodeToFavorites
            (
            MenuXmlNode aCmdNodeToAdd,
            MenuXmlParser originalMenuParser,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            if (aCmdNodeToAdd == null || !aCmdNodeToAdd.IsCommand)
                return false;

            if (AddingNodeToFavorites != null)
                AddingNodeToFavorites(this, aCmdNodeToAdd);

            MenuXmlParser tmpfavoritesXmlParser = new MenuXmlParser(favoritesXmlParser);

            if (tmpfavoritesXmlParser.AddCommandNode(aCmdNodeToAdd, true, commandsTypeToLoad) == null)
                return false;

            tmpfavoritesXmlParser.CopyNodeImageInfos(aCmdNodeToAdd, originalMenuParser, this.PathFinder);

            favoritesXmlParser = tmpfavoritesXmlParser;

            TraceFavoritesMenuOperation(aCmdNodeToAdd, MenuXmlNode.MenuActionType.Add, true);

            return true;
        }

        //---------------------------------------------------------------------------
        public MenuXmlNode AddShortcutToFavorites
            (
            string shortcutName,
            MenuXmlNode commandNode,
            string shortcutDescription,
            string shortcutImageLink,
            bool startup
            )
        {
            return AddShortcutToFavorites
                (
                        shortcutName,
                        commandNode.Type,
                        commandNode.CommandSubType,
                        commandNode.ItemObject,
                        shortcutDescription,
                        shortcutImageLink,
                        commandNode.ArgumentsOuterXml,
                        commandNode.DifferentCommandImage,
                        commandNode.GetOfficeApplication(),
                        commandNode.GetActivationAttribute(),
                        commandNode.GetNoWebAttribute(),
                        commandNode.GetRunNativeAttribute(),
                        startup
                );
        }

        //---------------------------------------------------------------------------
        public MenuXmlNode AddShortcutToFavorites
            (
            string shortcutName,
            MenuXmlNode.MenuXmlNodeType shortcutType,
            MenuXmlNode.MenuXmlNodeCommandSubType shortcutSubType,
            string shortcutCommand,
            string shortcutDescription,
            string shortcutImageLink,
            string shortcutArguments,
            string differentCommandImage,
            MenuXmlNode.OfficeItemApplication officeApplication,
            string activation,
            bool noweb,
            bool runNativeReport,
            bool startup
            )
        {
            return AddShortcutToFavorites
                (
                    shortcutName,
                    shortcutType,
                    shortcutSubType,
                    shortcutCommand,
                    shortcutDescription,
                    shortcutImageLink,
                    shortcutArguments,
                    differentCommandImage,
                    activation,
                    officeApplication,
                    CommandsTypeToLoad.All,
                    noweb,
                    runNativeReport,
                    startup
                );
        }

        //---------------------------------------------------------------------------
        public MenuXmlNode AddShortcutToFavorites
            (
            string shortcutName,
            MenuXmlNode.MenuXmlNodeType shortcutType,
            MenuXmlNode.MenuXmlNodeCommandSubType shortcutSubType,
            string shortcutCommand,
            string shortcutDescription,
            string shortcutImageLink,
            string shortcutArguments,
            string differentCommandImage,
            string activation,
            MenuXmlNode.OfficeItemApplication officeApplication,
            CommandsTypeToLoad commandsTypeToLoad,
            bool noweb,
            bool runNativeReport,
            bool startup
            )
        {
            MenuXmlParser tmpfavoritesXmlParser = new MenuXmlParser(favoritesXmlParser);

            MenuXmlNode shortcutNode = tmpfavoritesXmlParser.AddShortcutNode
                (
                    shortcutName,
                    shortcutType,
                    shortcutSubType,
                    shortcutCommand,
                    shortcutDescription,
                    shortcutImageLink,
                    shortcutArguments,
                    differentCommandImage,
                    activation,
                    commandsTypeToLoad,
                    officeApplication,
                    noweb,
                    runNativeReport,
                    startup
                );

            if (shortcutNode == null)
                return null;

            favoritesXmlParser = tmpfavoritesXmlParser;

            TraceFavoritesMenuOperation(shortcutNode, MenuXmlNode.MenuActionType.Add, false);

            return shortcutNode;
        }

        //---------------------------------------------------------------------------
        public bool RemoveShortcutFromFavorites(MenuXmlNode shortcutToRemove)
        {
            if (shortcutToRemove == null || !shortcutToRemove.IsShortcut)
                return false;

            TraceFavoritesMenuOperation(shortcutToRemove, MenuXmlNode.MenuActionType.Remove, false);

            return
                (favoritesXmlParser != null ? favoritesXmlParser.RemoveShortcutNode(shortcutToRemove) : false)
                ||
                AppsMenuXmlParser.RemoveShortcutNode(shortcutToRemove)
                ;
        }

        //---------------------------------------------------------------------------
        public void TraceFavoritesMenuOperation(MenuXmlNode nodeToTrace, MenuXmlNode.MenuActionType actionType, bool deep)
        {
            if (nodeToTrace == null || nodeToTrace.IsAction)
                return;

            if (traceFavoritesChanges == null)
                traceFavoritesChanges = new MenuXmlParser();

            MenuXmlNode traceNode = traceFavoritesChanges.TraceMenuAction(nodeToTrace, actionType, deep);
        }

        //---------------------------------------------------------------------------
        public string GetFavoritesMenuPath()
        {
            return GetFavoritesMenuPath(menuPathFinder);
        }

        //---------------------------------------------------------------------------
        public string GetAllUsersFavoritesMenuFileName()
        {
            return GetAllUsersFavoritesMenuFileName(menuPathFinder);
        }

        //---------------------------------------------------------------------------
        public string GetUserFavoritesMenuFileName()
        {
            return GetUserFavoritesMenuFileName(menuPathFinder);
        }

        //---------------------------------------------------------------------------
        public string GetValidUserDirectoryOrFileName()
        {
            return GetValidUserDirectoryOrFileName(menuPathFinder);
        }

        //TODOLUCA
        ////---------------------------------------------------------------------------
        //public static string GetOfficeItemFileName(string itemObject, MenuXmlNode.MenuXmlNodeCommandSubType commandSubType, MenuXmlNode.OfficeItemApplication application, IPathFinder pathFinder, string languageName)
        //{
        //	if
        //		(
        //			pathFinder == null ||
        //			itemObject == null ||
        //			itemObject.Length == 0 ||
        //			commandSubType.IsUndefined
        //		)
        //		return String.Empty;

        //	NameSpace officeFileNameSpace = BasePathFinder.BasePathFinderInstance.GetOfficeItemNamespace(
        //		itemObject,
        //		commandSubType,
        //		application
        //		);

        //	return pathFinder.GetFilename(officeFileNameSpace, languageName);
        //}

        ////---------------------------------------------------------------------------
        //public string GetOfficeItemFileName(MenuXmlNode aOfficeItemNode, IPathFinder pathFinder, string languageName)
        //{
        //	if (aOfficeItemNode == null || !aOfficeItemNode.IsOfficeItem)
        //		return String.Empty;

        //	return GetOfficeItemFileName(aOfficeItemNode.ItemObject, aOfficeItemNode.CommandSubType, aOfficeItemNode.GetOfficeApplication(), pathFinder, languageName );
        //}

        //---------------------------------------------------------------------------
        public static string GetLocalizedFileName(string oldFileName, string languageName)
        {
            if (oldFileName == null || oldFileName.Length == 0)
                return String.Empty;

            oldFileName = oldFileName.Trim();

            int index = oldFileName.LastIndexOf('.');
            if (index != -1)
            {
                string newFileName = oldFileName.Insert(index, languageName);
                if (newFileName != null && newFileName.Length > 0 && File.Exists(newFileName))
                    return newFileName;
            }

            return oldFileName;
        }

        //TODOLUCA
        ////---------------------------------------------------------------------------
        //public static string GetMenuEditorOfficeItemFileName(string itemObject, MenuXmlNode.MenuXmlNodeCommandSubType commandSubType, MenuXmlNode.OfficeItemApplication application)
        //{
        //	if
        //		(
        //		itemObject.IsNullOrEmpty() || 
        //		application == MenuXmlNode.OfficeItemApplication.Undefined ||
        //		commandSubType.IsUndefined
        //		)
        //		return String.Empty;

        //	return BasePathFinder.BasePathFinderInstance.GetCustomAllCompaniesOfficeItem
        //		(
        //		itemObject,
        //		commandSubType, 
        //		application
        //		);
        //}

        //---------------------------------------------------------------------------
        public CommandOrigin GetMenuCommandOrigin(MenuXmlNode aCommandNode, string language)
        {
            return GetMenuCommandOrigin(menuPathFinder, aCommandNode, language);
        }

        //---------------------------------------------------------------------------
        public static CommandOrigin GetMenuCommandOrigin(IPathFinder aPathFinder, MenuXmlNode aCommandNode, string language)
        {
            if
                (
                aPathFinder == null ||
                aCommandNode == null ||
                !(aCommandNode.IsCommand || aCommandNode.IsShortcut) ||
                aCommandNode.ItemObject == null ||
                aCommandNode.ItemObject.Length == 0
                )
                return CommandOrigin.Unknown;

            if (aCommandNode.CommandOrigin != CommandOrigin.Unknown)
                return aCommandNode.CommandOrigin;

            CommandOrigin commandOrigin = CommandOrigin.Unknown;

            NameSpace commandItemNamespace = null;
            if (aCommandNode.IsRunReport || aCommandNode.IsReportShortcut)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.Report);

            else if (aCommandNode.IsExcelDocument || aCommandNode.IsExcelDocumentShortcut)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.ExcelDocument);
            else if (aCommandNode.IsExcelTemplate || aCommandNode.IsExcelTemplateShortcut)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.ExcelTemplate);
            else if (aCommandNode.IsWordDocument || aCommandNode.IsWordDocumentShortcut)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.WordDocument);
            else if (aCommandNode.IsWordTemplate || aCommandNode.IsWordTemplateShortcut)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.WordTemplate);

            else if (aCommandNode.IsExcelDocument2007 || aCommandNode.IsExcelDocumentShortcut2007)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.ExcelDocument2007);
            else if (aCommandNode.IsExcelTemplate2007 || aCommandNode.IsExcelTemplateShortcut2007)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.ExcelTemplate2007);
            else if (aCommandNode.IsWordDocument2007 || aCommandNode.IsWordDocumentShortcut2007)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.WordDocument2007);
            else if (aCommandNode.IsWordTemplate2007 || aCommandNode.IsWordTemplateShortcut2007)
                commandItemNamespace = new NameSpace(aCommandNode.ItemObject, NameSpaceObjectType.WordTemplate2007);

            if (commandItemNamespace == null)
                return CommandOrigin.Standard;

            string commandFileFullFileName = aPathFinder.GetFilename(commandItemNamespace, ref commandOrigin, language);

            return commandOrigin;
        }

        //---------------------------------------------------------------------------
        public string GetExternalDescription(MenuXmlNode aDocumentNode)
        {
            return GetExternalDescription(menuPathFinder, aDocumentNode);
        }

        //---------------------------------------------------------------------------
        public bool GetReportFileDateTimes(MenuXmlNode aReportNode, out DateTime creationTime, out DateTime lastWriteTime)
        {
            return GetReportFileDateTimes(menuPathFinder, aReportNode, out creationTime, out lastWriteTime);
        }

        #endregion

        #region MenuInfo public static methods

        //---------------------------------------------------------------------------
        public static CommandOrigin SetMenuCommandOrigin(IPathFinder aPathFinder, MenuXmlNode aCommandNode, string language)
        {
            CommandOrigin commandOrigin = GetMenuCommandOrigin(aPathFinder, aCommandNode, language);

            if (aCommandNode != null && commandOrigin != CommandOrigin.Unknown)
                aCommandNode.CommandOrigin = commandOrigin;

            return commandOrigin;
        }

        //---------------------------------------------------------------------------
        public static string GetUserFavoritesMenuFileName(IPathFinder aPathFinder)
        {
            string userDirectoryName = GetValidUserDirectoryOrFileName(aPathFinder);

            if (userDirectoryName == null || userDirectoryName.Length == 0)
                return String.Empty;

            string filePath = GetFavoritesMenuPath(aPathFinder);
            if (filePath == null || filePath.Length == 0)
                return String.Empty;

            return filePath + Path.DirectorySeparatorChar + userDirectoryName + NameSolverStrings.MenuExtension;
        }

        //---------------------------------------------------------------------------
        public static string GetValidUserDirectoryOrFileName(IPathFinder aPathFinder)
        {
            if
                (
                aPathFinder == null ||
                aPathFinder.User == null ||
                aPathFinder.User.Length == 0
                )
                return String.Empty;

            string userDirectoryName = aPathFinder.User;
            userDirectoryName.Trim();
            char[] invalidFilenameChars = new char[]
                                {
                                    Path.AltDirectorySeparatorChar ,
                                    Path.DirectorySeparatorChar,
                                    Path.PathSeparator,
                                    Path.VolumeSeparatorChar,
                                    '\"',
                                    '<',
                                    '>',
                                    '!',
                                    '?',
                                    '*'
                                };

            if (userDirectoryName.IndexOfAny(invalidFilenameChars) >= 0)
            {
                //Ci sono dei caratteri non validi nel nome file: li sostituisco con '.'
                string[] validSubStrings = userDirectoryName.Split(invalidFilenameChars);
                userDirectoryName = String.Empty;
                foreach (string subString in validSubStrings)
                {
                    if (userDirectoryName.Length > 0)
                        userDirectoryName += '.';
                    if (subString != null && subString.Length > 0)
                        userDirectoryName += subString;
                }
            }

            return userDirectoryName;
        }

        //---------------------------------------------------------------------------
        public static string GetAllUsersFavoritesMenuFileName(IPathFinder aPathFinder)
        {
            string filePath = GetFavoritesMenuPath(aPathFinder);
            if (filePath == null || filePath.Length == 0)
                return String.Empty;

            return filePath + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers + NameSolverStrings.MenuExtension;
        }

        //---------------------------------------------------------------------------
        public static string GetFavoritesMenuPath(IPathFinder aPathFinder)
        {
            if (aPathFinder == null || aPathFinder.Company == null || aPathFinder.Company.Length == 0)
                return String.Empty;

            string companyName = aPathFinder.Company;
            companyName.Trim();
            if (companyName == null || companyName.Length == 0)
                return String.Empty;

            string customCompanyPath = (aPathFinder != null) ? aPathFinder.GetCustomCompanyPath() : String.Empty;
            customCompanyPath.Trim();

            if (customCompanyPath == null || customCompanyPath.Length == 0)
                return String.Empty;

            return customCompanyPath + Path.DirectorySeparatorChar + NameSolverStrings.Favorites + Path.DirectorySeparatorChar + NameSolverStrings.Menu;
        }

        //---------------------------------------------------------------------------
        public static string GetExternalDocumentDescription(IPathFinder aPathFinder, MenuXmlNode aDocumentNode)
        {
            if
                (
                aPathFinder == null ||
                aDocumentNode == null ||
                !(aDocumentNode.IsRunDocument || aDocumentNode.IsDocumentShortcut) ||
                aDocumentNode.ItemObject == null ||
                aDocumentNode.ItemObject.Length == 0
                )
                return null;

            return GetExternalAppCmdDescription(aPathFinder, aDocumentNode);
        }

        //---------------------------------------------------------------------------
        public static string SetExternalDocumentDescription(IPathFinder aPathFinder, MenuXmlNode aDocumentNode)
        {
            if (aDocumentNode == null)
                return String.Empty;

            string externalDocumentDescription = GetExternalDocumentDescription(aPathFinder, aDocumentNode);

            if (externalDocumentDescription != null && aDocumentNode.ExternalDescription == null)
                aDocumentNode.ExternalDescription = externalDocumentDescription;

            return externalDocumentDescription;
        }

        //---------------------------------------------------------------------------
        public static string GetExternalBatchDescription(IPathFinder aPathFinder, MenuXmlNode aBatchNode)
        {
            if
                (
                aPathFinder == null ||
                aBatchNode == null ||
                !(aBatchNode.IsRunBatch || aBatchNode.IsBatchShortcut) ||
                aBatchNode.ItemObject == null ||
                aBatchNode.ItemObject.Length == 0
                )
                return null;

            return GetExternalAppCmdDescription(aPathFinder, aBatchNode);
        }

        //---------------------------------------------------------------------------
        public static string SetExternalBatchDescription(IPathFinder aPathFinder, MenuXmlNode aBatchNode)
        {
            if (aBatchNode == null)
                return String.Empty;

            string externalBatchDescription = GetExternalBatchDescription(aPathFinder, aBatchNode);

            if (externalBatchDescription != null && aBatchNode.ExternalDescription == null)
                aBatchNode.ExternalDescription = externalBatchDescription;

            return externalBatchDescription;
        }

        //---------------------------------------------------------------------------
        public static string GetExternalFunctionDescription(IPathFinder aPathFinder, MenuXmlNode aFunctionNode)
        {
            if
                (
                aPathFinder == null ||
                aFunctionNode == null ||
                !(aFunctionNode.IsRunFunction || aFunctionNode.IsFunctionShortcut) ||
                aFunctionNode.ItemObject == null ||
                aFunctionNode.ItemObject.Length == 0
                )
                return null;

            return GetExternalAppCmdDescription(aPathFinder, aFunctionNode);
        }

        //---------------------------------------------------------------------------
        public static string SetExternalFunctionDescription(IPathFinder aPathFinder, MenuXmlNode aFunctionNode)
        {
            if (aFunctionNode == null)
                return String.Empty;

            string externalFunctionDescription = GetExternalFunctionDescription(aPathFinder, aFunctionNode);

            if (externalFunctionDescription != null && aFunctionNode.ExternalDescription == null)
                aFunctionNode.ExternalDescription = externalFunctionDescription;

            return externalFunctionDescription;
        }

        /// <summary>
        /// ///////
        /// </summary>
        /// <param name="aPathFinder"></param>
        /// <returns></returns>

        //---------------------------------------------------------------------------
        public static string GetExternalReportDescription(IPathFinder aPathFinder, MenuXmlNode aReportNode)
        {
            if
                (
                aPathFinder == null ||
                aReportNode == null ||
                !(aReportNode.IsRunReport || aReportNode.IsReportShortcut) ||
                aReportNode.ItemObject == null ||
                aReportNode.ItemObject.Length == 0
                )
                return null;

            // Se la descrizione non ?mai stata desunta dal file .wrm, la propriet?ExternalDescription
            // vale null e non stringa vuota: se la descrizione ?gi?stata letta ed ?vuota la suddetta propriet?
            // restituisce comunque String.Empty
            if (aReportNode.ExternalDescription != null)
                return aReportNode.ExternalDescription;

            NameSpace reportNameSpace = new NameSpace(aReportNode.ItemObject, NameSpaceObjectType.Report);

            return GetReportDescriptionFromNameSpace(aPathFinder, reportNameSpace);
        }

        //---------------------------------------------------------------------------
        public static string GetReportDescriptionFromNameSpace(IPathFinder aPathFinder, INameSpace reportNameSpace)
        {
            if
                (
                    aPathFinder == null ||
                    reportNameSpace == null ||
                    reportNameSpace.NameSpaceType.Type != NameSpaceObjectType.Report ||
                    reportNameSpace.Report == null ||
                    reportNameSpace.Report.Length == 0
                )
                return String.Empty;

            //TODO il terzo parametro è la lingua dell'utente corrente serve per poter trovare alcuni file nella 
            //standard divisi per lingua. Con string.empty cerca nella cartella di default 19/6/2006
            string reportFileName = aPathFinder.GetFilename(reportNameSpace, string.Empty);

            if (reportFileName == null || reportFileName.Length == 0 || !File.Exists(reportFileName))
                return String.Empty;

            Parser reportParser = new Parser(Parser.SourceType.FromFile);

            if (!reportParser.Open(reportFileName))
                return String.Empty;

            string reportDescription = String.Empty;

            if (reportParser.SkipToToken(Token.SUBJECT, true, false))
                reportParser.ParseCEdit(out reportDescription);

            reportParser.Close();

            if (reportDescription == null || reportDescription.Length == 0)
                return String.Empty;

            WoormLocalizer reportLocalizer = new WoormLocalizer(reportFileName, aPathFinder);

            reportDescription = reportLocalizer.Translate(reportDescription);

            return reportDescription;
        }

        //---------------------------------------------------------------------------
        public static string SetExternalReportDescription(IPathFinder aPathFinder, MenuXmlNode aReportNode)
        {
            if (aReportNode == null)
                return String.Empty;

            string externalReportDescription = GetExternalReportDescription(aPathFinder, aReportNode);

            if (externalReportDescription != null && aReportNode.ExternalDescription == null)
                aReportNode.ExternalDescription = externalReportDescription;

            return externalReportDescription;
        }

        //---------------------------------------------------------------------------
        public static string GetExternalDescription(IPathFinder aPathFinder, MenuXmlNode aCommandNode)
        {
            if (aCommandNode == null || !(aCommandNode.IsCommand || aCommandNode.IsShortcut))
                return String.Empty;

            if (aCommandNode.IsRunDocument || aCommandNode.IsDocumentShortcut)
                return GetExternalDocumentDescription(aPathFinder, aCommandNode);

            if (aCommandNode.IsRunReport || aCommandNode.IsReportShortcut)
                return GetExternalReportDescription(aPathFinder, aCommandNode);

            if (aCommandNode.IsRunBatch || aCommandNode.IsBatchShortcut)
                return GetExternalBatchDescription(aPathFinder, aCommandNode);

            if (aCommandNode.IsRunFunction || aCommandNode.IsFunctionShortcut)
                return GetExternalFunctionDescription(aPathFinder, aCommandNode);

            return String.Empty;
        }

        //---------------------------------------------------------------------------
        public static string SetExternalDescription(IPathFinder aPathFinder, MenuXmlNode aCommandNode)
        {
            if (aCommandNode == null || !(aCommandNode.IsCommand || aCommandNode.IsShortcut))
                return String.Empty;

            if (aCommandNode.IsRunDocument || aCommandNode.IsDocumentShortcut)
                return SetExternalDocumentDescription(aPathFinder, aCommandNode);

            if (aCommandNode.IsRunReport || aCommandNode.IsReportShortcut)
                return SetExternalReportDescription(aPathFinder, aCommandNode);

            if (aCommandNode.IsRunBatch || aCommandNode.IsBatchShortcut)
                return SetExternalBatchDescription(aPathFinder, aCommandNode);

            if (aCommandNode.IsRunFunction || aCommandNode.IsFunctionShortcut)
                return SetExternalFunctionDescription(aPathFinder, aCommandNode);

            return String.Empty;
        }

        //---------------------------------------------------------------------------
        public static bool GetReportFileDateTimes(IPathFinder aPathFinder, MenuXmlNode aReportNode, out DateTime creationTime, out DateTime lastWriteTime)
        {
            creationTime = DateTime.MinValue;
            lastWriteTime = DateTime.MinValue;

            if
                (
                aPathFinder == null ||
                aReportNode == null ||
                !(aReportNode.IsRunReport || aReportNode.IsReportShortcut) ||
                aReportNode.ItemObject == null ||
                aReportNode.ItemObject.Length == 0
                )
                return false;

            if (aReportNode.ReportFileCreationTime != DateTime.MinValue)
            {
                creationTime = aReportNode.ReportFileCreationTime;
                if (aReportNode.ReportFileLastWriteTime != DateTime.MinValue)
                {
                    lastWriteTime = aReportNode.ReportFileLastWriteTime;
                    return true;
                }
            }

            NameSpace reportNameSpace = new NameSpace(aReportNode.ItemObject, NameSpaceObjectType.Report);

            //TODO il terzo parametro è la lingua dell'utente corrente serve per poter trovare alcuni file nella 
            //standard divisi per lingua. Con string.empty cerca nella cartella di default 19/6/2006
            string fullReportFileName = aPathFinder.GetFilename(reportNameSpace, string.Empty);

            if (fullReportFileName == null || fullReportFileName.Length == 0 || !File.Exists(fullReportFileName))
                return false;

            try
            {
                FileInfo reportFileInfo = new FileInfo(fullReportFileName);

                creationTime = reportFileInfo.CreationTime;
                lastWriteTime = reportFileInfo.LastWriteTime;

                return true;
            }
            catch (IOException)
            {
                creationTime = DateTime.MinValue;
                lastWriteTime = DateTime.MinValue;

                return false;
            }
        }

        //---------------------------------------------------------------------------	
        public static bool SetReportFileDateTimes(IPathFinder aPathFinder, MenuXmlNode aReportNode)
        {
            if (aReportNode == null)
                return false;

            DateTime creationTime;
            DateTime lastWriteTime;

            if (!GetReportFileDateTimes(aPathFinder, aReportNode, out creationTime, out lastWriteTime))
                return false;

            if (creationTime != DateTime.MinValue && aReportNode.ReportFileCreationTime == DateTime.MinValue)
                aReportNode.ReportFileCreationTime = creationTime;
            if (lastWriteTime != DateTime.MinValue && aReportNode.ReportFileLastWriteTime == DateTime.MinValue)
                aReportNode.ReportFileLastWriteTime = lastWriteTime;

            return true;
        }

        #endregion
    }

    #endregion

    //============================================================================
    public class MenuParserEventArgs : EventArgs
    {
        private MenuXmlParser parser = null;
        private int counter = 0;
        private string moduleName = String.Empty;
        private string moduleTitle = String.Empty;
        //----------------------------------------------------------------------------
        public MenuXmlParser Parser { get { return parser; } }
        public int Counter { get { return counter; } }
        public string ModuleName { get { return moduleName; } }
        public string ModuleTitle { get { return moduleTitle; } }


        //----------------------------------------------------------------------------
        public MenuParserEventArgs(MenuXmlParser aParser)
        {
            parser = aParser;
        }

        //----------------------------------------------------------------------------
        public MenuParserEventArgs(MenuXmlParser aParser, int aNumber)
        {
            parser = aParser;
            counter = aNumber;
        }

        //----------------------------------------------------------------------------
        public MenuParserEventArgs(int aNumber)
        {
            counter = aNumber;
        }

        //----------------------------------------------------------------------------
        public MenuParserEventArgs(int aNumber, string aModuleName, string aModuleTitle)
        {
            counter = aNumber;
            moduleName = aModuleName;
            moduleTitle = aModuleTitle;
        }

        //----------------------------------------------------------------------------
        public MenuParserEventArgs(int aNumber, BaseModuleInfo aModuleInfo)
        {
            counter = aNumber;

            if (aModuleInfo != null)
            {
                moduleName = aModuleInfo.Name;
                moduleTitle = aModuleInfo.Title;
            }
        }
    }

    //============================================================================
    public delegate void MenuLoaderEventHandler(object sender, MenuInfo aMenuInfo);
    public delegate void MenuParserEventHandler(object sender, MenuParserEventArgs e);
    public delegate void FavoritesActionEventHandler(object sender, MenuXmlNode aNode);

    #region MenuLoader class

    /// <summary>
    /// Summary description for MenuLoader.
    /// </summary>
    //============================================================================
    public class MenuLoader
    {
        //============================================================================
        [Flags]
        public enum CommandsTypeToLoad
        {
            Undefined = 0x00000000,
            Form = 0x00000001,
            Report = 0x00000002,
            Batch = 0x00000004,
            Function = 0x00000008,
            Text = 0x00000010,
            Exe = 0x00000020,
            ExternalItem = 0x00000040,
            ExcelItem = 0x00000080,
            WordItem = 0x00000100,
            OfficeItem = ExcelItem | WordItem,
            All = Form | Report | Batch | Function | Text | Exe | ExternalItem | OfficeItem
        };

        private MenuInfo menuInfo = null;
        private bool environmentStandAlone = false;
        private IPathFinder pathFinder = null;
        private string authenticationToken = string.Empty;

        private IBrandLoader brandLoader = null;

        #region MenuLoader constructors

        //----------------------------------------------------------------------------
        public MenuLoader(IPathFinder aPathFinder, string authenticationToken, bool aEnvironmentStandAloneFlag)
        {
            pathFinder = aPathFinder;
            this.authenticationToken = authenticationToken;

            environmentStandAlone = aEnvironmentStandAloneFlag;

        }

        //----------------------------------------------------------------------------
        public MenuLoader(IPathFinder aPathFinder, bool aEnvironmentStandAloneFlag) : this(aPathFinder, null, aEnvironmentStandAloneFlag)
        {
        }

        //----------------------------------------------------------------------------
        public MenuLoader(IPathFinder aPathFinder) : this(aPathFinder, null, false)
        {
        }

        #endregion

        //----------------------------------------------------------------------------
        public bool LoadAllMenus(bool ignoreAllSecurityChecks, bool clearCachedData)
        {
            return LoadAllMenus(CommandsTypeToLoad.All, ignoreAllSecurityChecks, clearCachedData);
        }

        //----------------------------------------------------------------------------
        public bool LoadAllMenus(CommandsTypeToLoad commandsTypeToLoad)
        {
            return LoadAllMenus(commandsTypeToLoad, false, false);
        }

        //----------------------------------------------------------------------------
        public bool LoadAllMenus(CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks, bool clearCachedData)
        {
            // Per decidere se si deve applicare o meno il filtro sul menù oltre a 
            // verificare l'attivazione del modulo della sicurezza, si deve anche 
            // controllare che l'azienda alla quale ci si ?connessi sia effettivamente
            // posta sotto sicurezza
            // Inoltre, è importante notare che il controllo dell'azienda sotto sicurezza va
            // fatto SOLO SE si è loggati !!! 
            bool applySecurityFilter = false;

            LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            if (!ignoreAllSecurityChecks)
                applySecurityFilter = session.IsActivated("MicroareaConsole", "SecurityAdmin") &&
                    (
                    session.LoginManagerSessionState == LoginManagerState.Logged && session.Security
                    );

            return LoadAllMenus(applySecurityFilter, commandsTypeToLoad, ignoreAllSecurityChecks, clearCachedData);
        }

        //----------------------------------------------------------------------------
        public bool LoadAllMenus
            (
            bool applySecurityFilter,
            CommandsTypeToLoad commandsTypeToLoad,
            bool ignoreAllSecurityChecks,
            bool clearCachedData
            )
        {
            if (menuInfo != null)
            {
                menuInfo.ScanStandardMenuComponentsStarted -= new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsStarted);
                menuInfo.ScanStandardMenuComponentsEnded -= new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsEnded);
            }

            menuInfo = new MenuInfo(pathFinder, authenticationToken, applySecurityFilter);

            menuInfo.ScanStandardMenuComponentsStarted += new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsStarted);
            menuInfo.ScanStandardMenuComponentsEnded += new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsEnded);

            if (clearCachedData)
                Microarea.Common.Generic.InstallationInfo.Functions.ClearCachedData(menuInfo.PathFinder.User);

            menuInfo.ScanStandardMenuComponents(environmentStandAlone, commandsTypeToLoad);
            menuInfo.ScanCustomMenuComponents();

            string file = MenuInfo.CachedMenuInfos.GetStandardMenuCachingFullFileName(pathFinder.User);
            menuInfo.LoadAllMenuFiles(environmentStandAlone, commandsTypeToLoad, ignoreAllSecurityChecks, clearCachedData || !File.Exists(file));

            return true;
        }

        //---------------------------------------------------------------------------
        public string GetUserFavoritesMenuFileName()
        {
            return GetUserFavoritesMenuFileName(pathFinder);
        }

        //---------------------------------------------------------------------------
        public string GetAllUsersFavoritesMenuFileName()
        {
            return GetAllUsersFavoritesMenuFileName(pathFinder);
        }

        //---------------------------------------------------------------------------
        public string GetFavoritesMenuPath()
        {
            return GetFavoritesMenuPath(pathFinder);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public List<MenuXmlNode> GetShortcuts(bool bStartup)
        {
            List<MenuXmlNode> shortcuts = new List<MenuXmlNode>();

            if (AppsMenuXmlParser != null)
            {
                ArrayList shortcuts1 = AppsMenuXmlParser.ShortcutsItems;
                if (shortcuts1 != null)
                {
                    for (int i = 0; i < shortcuts1.Count; i++)
                    {
                        MenuXmlNode n = shortcuts1[i] as MenuXmlNode;
                        if
                            (
                            bStartup
                            ? n.IsStartupShortcut
                            : n.IsShortcut && !n.IsStartupShortcut
                            )
                            shortcuts.Add(n);
                    }
                }
            }

            return shortcuts;
        }


        #region MenuLoader public properties

        //---------------------------------------------------------------------------
        public MenuInfo MenuInfo { get { return menuInfo; } }
        //---------------------------------------------------------------------------
        public MenuXmlParser AppsMenuXmlParser { get { return (menuInfo != null) ? menuInfo.AppsMenuXmlParser : null; } }
        //---------------------------------------------------------------------------
        public MenuXmlParser EnvironmentXmlParser { get { return (menuInfo != null) ? menuInfo.EnvironmentXmlParser : null; } }
        //---------------------------------------------------------------------------
        public bool IsEnvironmentStandAlone { get { return environmentStandAlone; } }
        //---------------------------------------------------------------------------
        public IPathFinder PathFinder { get { return pathFinder; } }
        //---------------------------------------------------------------------------
        public IBrandLoader BrandLoader { get { return brandLoader; } }

        #endregion

        #region MenuLoader static methods

        //---------------------------------------------------------------------------
        public static string GetUserFavoritesMenuFileName(IPathFinder aPathFinder)
        {
            return MenuInfo.GetUserFavoritesMenuFileName(aPathFinder);
        }

        //---------------------------------------------------------------------------
        public static string GetAllUsersFavoritesMenuFileName(IPathFinder aPathFinder)
        {
            return MenuInfo.GetAllUsersFavoritesMenuFileName(aPathFinder);
        }

        //---------------------------------------------------------------------------
        public static string GetFavoritesMenuPath(IPathFinder aPathFinder)
        {
            return MenuInfo.GetFavoritesMenuPath(aPathFinder);
        }

        //---------------------------------------------------------------------------
        public static bool IsNodeTypeToLoad(MenuXmlNode.MenuXmlNodeType aNodeType, MenuXmlNode.OfficeItemApplication aOfficeApplication, CommandsTypeToLoad commandsTypeToLoad)
        {
            if (!aNodeType.IsCommand || commandsTypeToLoad == CommandsTypeToLoad.All)
                return true;

            if (commandsTypeToLoad == CommandsTypeToLoad.Undefined)
                return false;

            if (aNodeType.IsRunDocument)
                return (commandsTypeToLoad & CommandsTypeToLoad.Form) == CommandsTypeToLoad.Form;

            if (aNodeType.IsRunReport)
                return (commandsTypeToLoad & CommandsTypeToLoad.Report) == CommandsTypeToLoad.Report;

            if (aNodeType.IsRunBatch)
                return (commandsTypeToLoad & CommandsTypeToLoad.Batch) == CommandsTypeToLoad.Batch;

            if (aNodeType.IsRunFunction)
                return (commandsTypeToLoad & CommandsTypeToLoad.Function) == CommandsTypeToLoad.Function;

            if (aNodeType.IsRunExecutable)
                return (commandsTypeToLoad & CommandsTypeToLoad.Exe) == CommandsTypeToLoad.Exe;

            if (aNodeType.IsRunText)
                return (commandsTypeToLoad & CommandsTypeToLoad.Text) == CommandsTypeToLoad.Text;

            if (aNodeType.IsExternalItem)
                return (commandsTypeToLoad & CommandsTypeToLoad.ExternalItem) == CommandsTypeToLoad.ExternalItem;

            if (aNodeType.IsOfficeItem)
            {
                if (aOfficeApplication == MenuXmlNode.OfficeItemApplication.Excel)
                    return (commandsTypeToLoad & CommandsTypeToLoad.ExcelItem) == CommandsTypeToLoad.ExcelItem;

                if (aOfficeApplication == MenuXmlNode.OfficeItemApplication.Word)
                    return (commandsTypeToLoad & CommandsTypeToLoad.WordItem) == CommandsTypeToLoad.WordItem;

                return (commandsTypeToLoad & CommandsTypeToLoad.OfficeItem) != CommandsTypeToLoad.Undefined;
            }
            return true;
        }

        //---------------------------------------------------------------------------
        public static bool IsNodeToLoad(MenuXmlNode aNode, CommandsTypeToLoad commandsTypeToLoad)
        {
            if (!(aNode.IsCommand || aNode.IsShortcut) || commandsTypeToLoad == CommandsTypeToLoad.All)
                return true;

            if (commandsTypeToLoad == CommandsTypeToLoad.Undefined)
                return false;

            if (aNode.IsRunDocument || aNode.IsDocumentShortcut)
                return (commandsTypeToLoad & CommandsTypeToLoad.Form) == CommandsTypeToLoad.Form;

            if (aNode.IsRunReport || aNode.IsReportShortcut)
                return (commandsTypeToLoad & CommandsTypeToLoad.Report) == CommandsTypeToLoad.Report;

            if (aNode.IsRunBatch || aNode.IsBatchShortcut)
                return (commandsTypeToLoad & CommandsTypeToLoad.Batch) == CommandsTypeToLoad.Batch;

            if (aNode.IsRunFunction || aNode.IsFunctionShortcut)
                return (commandsTypeToLoad & CommandsTypeToLoad.Function) == CommandsTypeToLoad.Function;

            if (aNode.IsRunExecutable || aNode.IsExeShortcut)
                return (commandsTypeToLoad & CommandsTypeToLoad.Exe) == CommandsTypeToLoad.Exe;

            if (aNode.IsRunText || aNode.IsTextShortcut)
                return (commandsTypeToLoad & CommandsTypeToLoad.Text) == CommandsTypeToLoad.Text;

            if (aNode.IsExternalItem || aNode.IsExternalItemShortcut)
                return (commandsTypeToLoad & CommandsTypeToLoad.ExternalItem) == CommandsTypeToLoad.ExternalItem;

            if (aNode.IsOfficeItem || aNode.IsOfficeItemShortcut)
            {
                if (aNode.IsExcelItem)
                    return (commandsTypeToLoad & CommandsTypeToLoad.ExcelItem) == CommandsTypeToLoad.ExcelItem;

                if (aNode.IsWordItem)
                    return (commandsTypeToLoad & CommandsTypeToLoad.WordItem) == CommandsTypeToLoad.WordItem;

                return (commandsTypeToLoad & CommandsTypeToLoad.OfficeItem) != CommandsTypeToLoad.Undefined;
            }
            return true;
        }


        #endregion

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler ScanStandardMenuComponentsStarted;
        public void MenuInfo_ScanStandardMenuComponentsStarted(object sender, MenuParserEventArgs e)
        {
            if (ScanStandardMenuComponentsStarted != null)
                ScanStandardMenuComponentsStarted(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler ScanStandardMenuComponentsModuleIndexChanged;
        public void MenuInfo_ScanStandardMenuComponentsModuleIndexChanged(object sender, MenuParserEventArgs e)
        {
            if (ScanStandardMenuComponentsModuleIndexChanged != null)
                ScanStandardMenuComponentsModuleIndexChanged(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler ScanStandardMenuComponentsEnded;
        public void MenuInfo_ScanStandardMenuComponentsEnded(object sender, MenuParserEventArgs e)
        {
            if (ScanStandardMenuComponentsEnded != null)
                ScanStandardMenuComponentsEnded(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler ScanCustomMenuComponentsStarted;
        public void MenuInfo_ScanCustomMenuComponentsStarted(object sender, MenuParserEventArgs e)
        {
            if (ScanCustomMenuComponentsStarted != null)
                ScanCustomMenuComponentsStarted(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler ScanCustomMenuComponentsModuleIndexChanged;
        public void MenuInfo_ScanCustomMenuComponentsModuleIndexChanged(object sender, MenuParserEventArgs e)
        {
            if (ScanCustomMenuComponentsModuleIndexChanged != null)
                ScanCustomMenuComponentsModuleIndexChanged(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler ScanCustomMenuComponentsEnded;
        public void MenuInfo_ScanCustomMenuComponentsEnded(object sender, MenuParserEventArgs e)
        {
            if (ScanCustomMenuComponentsEnded != null)
                ScanCustomMenuComponentsEnded(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler LoadAllMenuFilesStarted;
        public void MenuInfo_LoadAllMenuFilesStarted(object sender, MenuParserEventArgs e)
        {
            if (LoadAllMenuFilesStarted != null)
                LoadAllMenuFilesStarted(this, e);
        }
        //----------------------------------------------------------------------------
        public event MenuParserEventHandler LoadAllMenuFilesModuleIndexChanged;
        public void MenuInfo_LoadAllMenuFilesModuleIndexChanged(object sender, MenuParserEventArgs e)
        {
            if (LoadAllMenuFilesModuleIndexChanged != null)
                LoadAllMenuFilesModuleIndexChanged(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler LoadAllMenuFilesEnded;
        public void MenuInfo_LoadAllMenuFilesEnded(object sender, MenuParserEventArgs e)
        {
            if (LoadAllMenuFilesEnded != null)
                LoadAllMenuFilesEnded(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler LoadFavoritesMenuStarted;
        public void MenuInfo_LoadFavoritesMenuStarted(object sender, MenuParserEventArgs e)
        {
            if (LoadFavoritesMenuStarted != null)
                LoadFavoritesMenuStarted(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler LoadFavoritesMenuAppIndexChanged;
        public void MenuInfo_LoadFavoritesMenuAppIndexChanged(object sender, MenuParserEventArgs e)
        {
            if (LoadFavoritesMenuAppIndexChanged != null)
                LoadFavoritesMenuAppIndexChanged(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler LoadFavoritesMenuEnded;
        public void MenuInfo_LoadFavoritesMenuEnded(object sender, MenuParserEventArgs e)
        {
            if (LoadFavoritesMenuEnded != null)
                LoadFavoritesMenuEnded(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler LoadCachedStandardMenuStarted;
        public void MenuInfo_LoadCachedStandardMenuStarted(object sender, MenuParserEventArgs e)
        {
            if (LoadCachedStandardMenuStarted != null)
                LoadCachedStandardMenuStarted(this, e);
        }

        //----------------------------------------------------------------------------
        public event MenuParserEventHandler LoadCachedStandardMenuEnded;
        public void MenuInfo_LoadCachedStandardMenuEnded(object sender, MenuParserEventArgs e)
        {
            if (LoadCachedStandardMenuEnded != null)
                LoadCachedStandardMenuEnded(this, e);
        }
    }

    #endregion
}