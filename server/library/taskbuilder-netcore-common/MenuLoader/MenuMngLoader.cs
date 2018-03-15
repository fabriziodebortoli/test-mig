using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
            private List<string> standardMenuFiles;
            private string customAllUsersMenuPath;
            private List<string> customAllUsersMenuFiles;
            private string customCurrentUserMenuPath;
            private List<string> customCurrentUserMenuFiles;


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
            public void AddStandardMenuFile(string aFileName)
            {
                if (aFileName == null || aFileName.Length == 0)
                    return;

                if (standardMenuFiles == null)
                    standardMenuFiles = new List<string>();

                standardMenuFiles.Add(aFileName);
            }

            //---------------------------------------------------------------------------
            public void AddCustomAllUsersMenuFile(string aFileName)
            {
                if (aFileName == null || aFileName.Length == 0)
                    return;

                if (customAllUsersMenuFiles == null)
                    customAllUsersMenuFiles = new List<string>();

                customAllUsersMenuFiles.Add(aFileName);
            }

            //---------------------------------------------------------------------------
            public void AddCustomCurrentUserMenuFile(string aFileName)
            {
                if (aFileName == null || aFileName.Length == 0)
                    return;

                if (customCurrentUserMenuFiles == null)
                    customCurrentUserMenuFiles = new List<string>();

                customCurrentUserMenuFiles.Add(aFileName);
            }
            //---------------------------------------------------------------------------
            public void SetCustomMenuPaths(string aPath, string aUserName)
            {
                customAllUsersMenuPath = String.Empty;
                customCurrentUserMenuPath = String.Empty;

                if (aPath == null || aPath.Length == 0)
                    return;

                customAllUsersMenuPath = aPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.AllUsers;
                if (aUserName != null && aUserName.Length > 0 && aUserName != NameSolverStrings.AllUsers)
                    customCurrentUserMenuPath = aPath + NameSolverStrings.Directoryseparetor + PathFinder.GetUserPath(aUserName);
            }

            //---------------------------------------------------------------------------
            public string Name { get { return name; } set { name = value; } }
            //---------------------------------------------------------------------------
            public int MenuViewOrder { get { return menuViewOrder; } set { menuViewOrder = value; } }
            //---------------------------------------------------------------------------
            public string StandardMenuPath
            {
                get { return standardMenuPath; }
                set { standardMenuPath = (value != null && value.Length > 0 && PathFinder.PathFinderInstance.ExistPath(value)) ? value : String.Empty; }
            }
            //---------------------------------------------------------------------------
            public List<string> StandardMenuFiles { get { return standardMenuFiles; } set { standardMenuFiles = value; } }
            //---------------------------------------------------------------------------
            [XmlIgnore]
            public string CustomAllUsersMenuPath { get { return customAllUsersMenuPath; } }
            //---------------------------------------------------------------------------
            [XmlIgnore]
            public List<string> CustomAllUsersMenuFiles { get { return customAllUsersMenuFiles; } }
            //---------------------------------------------------------------------------
            [XmlIgnore]
            public string CustomCurrentUserMenuPath { get { return customCurrentUserMenuPath; } }
            //---------------------------------------------------------------------------
            [XmlIgnore]
            public List<string> CustomCurrentUserMenuFiles { get { return customCurrentUserMenuFiles; } }
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
            /// Data di confronto per la validit� della cache
            /// </summary>
            public DateTime CacheDate;
            /// <summary>
			/// Discrimina se la struttura e' stata letta da file
			/// (e allora non va risalvata) oppure se sata creata dinamicamente
			/// (e allora va salvata su file)
			/// </summary>
			private bool fromFile;

            private PathFinder pathFinder;

            public bool FromFile
            {
                get { return fromFile; }
            }

            //---------------------------------------------------------------------------
            public CachedMenuInfos(CommandsTypeToLoad commandsTypeToLoad, string configurationHash, PathFinder pathFinder)
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
            public static string GetStandardMenuCachingFullFileName(string user)
            {
                string clientInstallationPath = PathFinder.PathFinderInstance.GetAppDataPath(true);
                clientInstallationPath = Path.Combine(clientInstallationPath, user);

                if (!PathFinder.PathFinderInstance.ExistPath(clientInstallationPath))
                    PathFinder.PathFinderInstance.CreateFolder(clientInstallationPath, false);

                return Path.Combine(
                    clientInstallationPath,
                    string.Format(StandardMenuCachingFileName, CultureInfo.CurrentUICulture.Name)
                    );
            }

            //---------------------------------------------------------------------------
            public static CachedMenuInfos Load(CommandsTypeToLoad commandsTypeToLoad, string configurationHash, string user)
            {
               //Dve rimanere  cosi
                string file = GetStandardMenuCachingFullFileName(user); 
                if (!PathFinder.PathFinderInstance.ExistFile(file))//PathFinder.PathFinderInstance.ExistFile(file))
                    return null;
                try
                {
                    XmlSerializer ser = GetSerializer();
                    FileInfo fi = new FileInfo(file);  //ok

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
                //Deve rimanere cosi
                string file = GetStandardMenuCachingFullFileName(user);
                try
                {
                    XmlSerializer ser = GetSerializer();
                    FileInfo fi = new FileInfo(file); //OK
                    using (StreamWriter sw = fi.CreateText())
                        ser.Serialize(sw, this);
                }
                catch (Exception)
                {
                }

                
            }

            //---------------------------------------------------------------------------
            public static void Delete(string user)
            {
                try
                {
                    string file = GetStandardMenuCachingFullFileName(user); 

                    if (PathFinder.PathFinderInstance.ExistPath(Path.GetDirectoryName(file)))
                        PathFinder.PathFinderInstance.RemoveFolder(Path.GetDirectoryName(file), true, true, true);
                }
                catch (Exception)
                {
                }
            }
        }
        // la classe MenuInfo costruisce un array di file di men?da caricare

        private CachedMenuInfos cachedInfos = null;


        private string authenticationToken = null;
        private PathFinder menuPathFinder = null;


      
        //---------------------------------------------------------------------------
        public MenuInfo(PathFinder aPathFinder, string authenticationToken, bool applySecurityFilter)
        {
            menuPathFinder = aPathFinder;
            this.authenticationToken = authenticationToken;
            if
                (
                menuPathFinder != null &&
                InstallationData.ServerConnectionInfo != null &&
                !string.IsNullOrEmpty(InstallationData.ServerConnectionInfo.SysDBConnectionString)
                )
            {
                //menuSecurityFilter = new MenuSecurityFilter
                //    (
                //    InstallationData.ServerConnectionInfo.SysDBConnectionString,
                //    menuPathFinder.Company,
                //    menuPathFinder.User,
                //    applySecurityFilter
                //    );
            }
        }



        #region MenuInfo private methods

  

        //---------------------------------------------------------------------------
        private void LoadMenuFilesFromArrayList(
            string aApplicationName,
            string aModuleName,
            string filesPath,
            List<string> menuFilesToLoad,
            ApplicationType aApplicationType,
            CommandsTypeToLoad commandsTypeToLoad
            )
        {
            if(menuPathFinder == null ||aApplicationName.IsNullOrEmpty() ||aModuleName.IsNullOrEmpty() ||commandsTypeToLoad == CommandsTypeToLoad.Undefined)
                return;

            if (menuFilesToLoad == null || filesPath.IsNullOrEmpty() || !PathFinder.PathFinderInstance.ExistPath(filesPath))
                return;

            if (aApplicationType == ApplicationType.TaskBuilder && EnvironmentXmlParser != null)
                EnvironmentXmlParser.LoadMenuFilesFromArrayList(aApplicationName, aModuleName, menuPathFinder, filesPath, menuFilesToLoad, commandsTypeToLoad);
            else
                AppsMenuXmlParser.LoadMenuFilesFromArrayList(aApplicationName, aModuleName, menuPathFinder, filesPath, menuFilesToLoad, commandsTypeToLoad);
        }

        //---------------------------------------------------------------------------
        private bool AddUserCreatedReportsGroup(string aApplicationName, ModuleInfo aModule, List<string> userCreatedReports)
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
        private List<string> SearchUserCreatedReportFiles(ApplicationInfo aApplication, string aModuleName)
        {
            if (aApplication == null || aModuleName.IsNullOrEmpty())
                return null;

            string fullCustomPath = CurrentPathFinder.GetCustomModulePath(aApplication.Name, aModuleName);
            if (!CurrentPathFinder.ExistPath(fullCustomPath))
                return null;

            string customModuleReportPath = fullCustomPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.Report;


            if (!CurrentPathFinder.ExistPath(customModuleReportPath))
                return null;

            string userDirectoryName = GetValidUserDirectoryOrFileName();

            if (userDirectoryName == null || userDirectoryName.Length == 0)
                return null;

            string standardModuleReportpath = CurrentPathFinder.GetApplicationModulePath(aApplication.Name, aModuleName) + NameSolverStrings.Directoryseparetor + NameSolverStrings.Report;


            bool addAllCustomReports = !CurrentPathFinder.ExistPath(standardModuleReportpath);

            List<string> userCreatedReports = new List<string>();

            // Prima carico i report relativi all'utente corrente. Infatti, se esiste per tale utente
            // un report con un dato nome, poi non devo considerare file di report "omonimi" relativi 
            // a tutti gli utenti (AllUsers)
            List<TBFile> currentUserCustomReportFiles = null;
            if
                (
                userDirectoryName != null &&
                userDirectoryName.Length > 0 &&
                userDirectoryName != NameSolverStrings.AllUsers &&
                CurrentPathFinder.ExistPath(customModuleReportPath + NameSolverStrings.Directoryseparetor + Microarea.Common.NameSolver.PathFinder.GetUserPath(userDirectoryName))
                )
            {
                currentUserCustomReportFiles = CurrentPathFinder.GetFiles(customModuleReportPath + NameSolverStrings.Directoryseparetor +  Microarea.Common.NameSolver.PathFinder.GetUserPath(userDirectoryName) , "*" + NameSolverStrings.WrmExtension);
                if (currentUserCustomReportFiles != null && currentUserCustomReportFiles.Count > 0)
                {
                    foreach (TBFile currentUserCustomReportFileInfo in currentUserCustomReportFiles)
                    {

                        string reportNamespace = string.Join(
                            ".",
                            aApplication.Name,
                            aModuleName,
                            currentUserCustomReportFileInfo.completeFileName.Replace(NameSolverStrings.WrmExtension, string.Empty)
                            );

                        // Controllo che non esista un file omonimo sotto la standard perch?in tal caso
                        // si tratterebbe di una personalizzazione di un report "ufficiale" e non di un
                        // report creato ex-novo dall'utente
                        // Inoltre, devo anche controllare se il file non ?mai utilizzato nel men? in tal caso
                        // va comunque aggiunto nei report dell'utente (anche se c'?un file omonimo nella standsard)
                        if
                            (
                            ApplicationInfo.IsValidReportFileName(currentUserCustomReportFileInfo.name) &&
                            !ExistsReportMenuCommand(aApplication.Name, aModuleName, currentUserCustomReportFileInfo.name) &&
                            !ExistInCustomizedMenu(this.AppsMenuXmlParser.MenuXmlDoc, reportNamespace, MenuXmlNode.XML_TAG_REPORT) &&
                            (
                            addAllCustomReports ||
                            !CurrentPathFinder.ExistFile(standardModuleReportpath + NameSolverStrings.Directoryseparetor + currentUserCustomReportFileInfo.name)
                             )
                            )
                            userCreatedReports.Add(currentUserCustomReportFileInfo.name);
                    }
                }
            }
            // Una volta caricati gli eventuali nuovi report per l'utente corrente posso vedere quali altri
            // report di questo tipo trovo sotto AllUsers
            if (CurrentPathFinder.ExistPath(customModuleReportPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.AllUsers))
            {
                List<TBFile> allUsersCustomReportFiles = CurrentPathFinder.GetFiles(customModuleReportPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.AllUsers , "*" + NameSolverStrings.WrmExtension);
                if (allUsersCustomReportFiles != null && allUsersCustomReportFiles.Count > 0)
                {
                    foreach (TBFile allUsersCustomReportFileInfo in allUsersCustomReportFiles)
                    {
                        string reportNamespace = string.Join(
                            ".",
                            aApplication.Name,
                            aModuleName,
                            allUsersCustomReportFileInfo.name.Replace(NameSolverStrings.WrmExtension, string.Empty)
                            );

                        // Prima controllo che non esista un file omonimo sotto la standard o che
                        // il report non venga mai menzionato nel men?di applicazione.
                        if
                            (
                            ApplicationInfo.IsValidReportFileName(allUsersCustomReportFileInfo.name) &&
                            !ExistsReportMenuCommand(aApplication.Name, aModuleName, allUsersCustomReportFileInfo.name) &&
                            !ExistInCustomizedMenu(this.AppsMenuXmlParser.MenuXmlDoc, reportNamespace, MenuXmlNode.XML_TAG_REPORT) &&
                            (
                            addAllCustomReports ||
                            !CurrentPathFinder.ExistFile(standardModuleReportpath + NameSolverStrings.Directoryseparetor + allUsersCustomReportFileInfo.name)
                            )
                            )
                        {
                            bool ignoreReport = false;
                            // Adesso vedo se ho precedentemente trovato un report con lo stesso nome relativo all'utente corrente
                            if (currentUserCustomReportFiles != null && currentUserCustomReportFiles.Count > 0)
                            {
                                foreach (TBFile currentUserCustomReportFileInfo in currentUserCustomReportFiles)
                                {
                                    if (String.Compare(currentUserCustomReportFileInfo.name, allUsersCustomReportFileInfo.name, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        ignoreReport = true;
                                        break;
                                    }
                                }
                            }
                            if (!ignoreReport)
                                userCreatedReports.Add(allUsersCustomReportFileInfo.name);
                        }
                    }
                }
            }

            return (userCreatedReports.Count > 0) ? userCreatedReports : null;
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

        #endregion // MenuInfo standard menu caching

        #endregion // MenuInfo private methods

        #region MenuInfo private static methods

        //---------------------------------------------------------------------------
        private static string GetExternalAppCmdDescription(PathFinder aPathFinder, MenuXmlNode aCommandNode)
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
        public MenuXmlParser AppsMenuXmlParser { get { return cachedInfos.AppsMenuXmlParser; } }
        //---------------------------------------------------------------------------
        public MenuXmlParser EnvironmentXmlParser { get { return cachedInfos.EnvironmentXmlParser; } }
        //---------------------------------------------------------------------------
        public PathFinder CurrentPathFinder { get { return menuPathFinder; } }
        //---------------------------------------------------------------------------
        public List<ApplicationMenuInfo> ApplicationsInfo { get { return cachedInfos.ApplicationsInfo; } }

        #endregion

        #region MenuInfo public methods
        /// <summary>
        /// Searches all application standard menu files
        /// </summary>
        //---------------------------------------------------------------------------
        public void ScanStandardMenuComponents(bool environmentStandAlone, CommandsTypeToLoad commandsTypeToLoad)
        {
            if (menuPathFinder == null)
                return;

            bool isCachedStandardMenuLoaded = LoadCachedStandardMenu(commandsTypeToLoad);

            if (!isCachedStandardMenuLoaded)
            {

                LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
                if (session == null)
                    return;
                List<string> activatedModules = session.GetModules();
                if (activatedModules == null || activatedModules.Count <= 0)
                {
                    Debug.Fail("No activated modules found");
                    return;
                }

                cachedInfos.ApplicationsInfo = new List<ApplicationMenuInfo>();

                //poi inizio a leggere le informazioni
                foreach (ApplicationInfo appInfo in CurrentPathFinder.ApplicationInfos)
                {
                    if (
                        appInfo.ApplicationType != ApplicationType.TaskBuilderApplication &&
                        appInfo.ApplicationType != ApplicationType.TaskBuilder &&
                        appInfo.ApplicationType != ApplicationType.Customization
                        )
                        continue;

                    ApplicationMenuInfo aApplication = new ApplicationMenuInfo(appInfo.Name, appInfo.ApplicationType);
                    aApplication.ClearModulesMenuInfos();

                    if (appInfo.Modules != null && aApplication != null)
                    {
                        foreach (ModuleInfo moduleInfo in appInfo.Modules)
                        {
                            bool moduleFound = activatedModules.Exists((s) => { return string.Compare(s, appInfo.Name + "." + moduleInfo.Name, StringComparison.OrdinalIgnoreCase) == 0; });
                            if (!moduleFound)
                                continue;

                            ModuleMenuInfo aModule = new ModuleMenuInfo(moduleInfo.Name, moduleInfo.ModuleConfigInfo.Title, moduleInfo.ModuleConfigInfo.MenuViewOrder);

                            string fullStandardPath = CurrentPathFinder.GetApplicationModulePath(aApplication.Name, aModule.Name);

                            string moduleMenuDirInfopath = fullStandardPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.Menu;

                            if (appInfo.ApplicationType == ApplicationType.Customization)
                            {
                                aModule.StandardMenuPath = moduleMenuDirInfopath;
                                //Do per scontato che menuPathFinder.User abbia un valore sensato perch� se invece � vuoto allora c'e un problema a monte.
                                string workingMenuPath = Path.Combine(moduleMenuDirInfopath, menuPathFinder.User);

                                List<TBFile> menuFiles = null;

                                if (CurrentPathFinder.ExistPath(workingMenuPath))
                                {
                                    menuFiles = CurrentPathFinder.GetFiles(workingMenuPath, "*" + NameSolverStrings.MenuExtension);
                                    aModule.StandardMenuPath = workingMenuPath;
                                }
                                if ((menuFiles == null || menuFiles.Count == 0) && CurrentPathFinder.ExistPath(moduleMenuDirInfopath))
                                {
                                    menuFiles = CurrentPathFinder.GetFiles(moduleMenuDirInfopath, "*" + NameSolverStrings.MenuExtension);
                                    aModule.StandardMenuPath = moduleMenuDirInfopath;
                                }

                                if (menuFiles == null)
                                    continue;

                                foreach (TBFile aMenuFileInfo in menuFiles)
                                    aModule.AddStandardMenuFile(aMenuFileInfo.name);

                                menuFiles = null;
                            }
                            else if (CurrentPathFinder.ExistPath(moduleMenuDirInfopath))
                            {
                                aModule.StandardMenuPath = moduleMenuDirInfopath;

                                List<TBFile> menuFiles = CurrentPathFinder.GetFiles(moduleMenuDirInfopath, "*" + NameSolverStrings.MenuExtension);

                                if (menuFiles == null)
                                    continue;

                                foreach (TBFile aMenuFileInfo in menuFiles)
                                    aModule.AddStandardMenuFile(aMenuFileInfo.name);

                                menuFiles = null;
                            }

                            aApplication.AddModuleMenuInfos(aModule);
                        }
                    }

                    ApplicationsInfo.Add(aApplication);
                }
            }
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
                    string fullCustomPath = CurrentPathFinder.GetCustomModulePath(aApplication.Name, aModule.Name);
                    
                    if (CurrentPathFinder.ExistPath(fullCustomPath))
                    {
                        aModule.SetCustomMenuPaths(fullCustomPath, userName);

                        List<TBFile> menuFiles = null;
                        if (PathFinder.PathFinderInstance.ExistPath(aModule.CustomAllUsersMenuPath))
                        {
                            menuFiles = CurrentPathFinder.GetFiles(fullCustomPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.AllUsers + NameSolverStrings.Directoryseparetor , "*" + NameSolverStrings.MenuExtension);
                            if (menuFiles.Count  > 0)
                            {
                                foreach (TBFile aMenuFileInfo in menuFiles)
                                    aModule.AddCustomAllUsersMenuFile(aMenuFileInfo.name);
                            }
                        }
                        if (userName != null && userName.Length > 0 && userName != NameSolverStrings.AllUsers)
                        {
                            if (PathFinder.PathFinderInstance.ExistPath(aModule.CustomCurrentUserMenuPath))
                            {
                                menuFiles = PathFinder.PathFinderInstance.GetFiles(Microarea.Common.NameSolver.PathFinder.GetUserPath(userName) , "*" + NameSolverStrings.MenuExtension);
                                if (menuFiles.Count  > 0)
                                {
                                    foreach (TBFile aMenuFileInfo in menuFiles)
                                        aModule.AddCustomCurrentUserMenuFile(aMenuFileInfo.name);
                                }
                            }
                        }
                    }
                }
            }
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
                        LoadMenuFilesFromArrayList(aApplication.Name, aModule.Name, aModule.StandardMenuPath, aModule.StandardMenuFiles, aApplication.Type, commandsTypeToLoad);
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
                    if (aModule.CustomAllUsersMenuFiles != null )
                        LoadMenuFilesFromArrayList(aApplication.Name, aModule.Name, aModule.CustomAllUsersMenuPath, aModule.CustomAllUsersMenuFiles, aApplication.Type, commandsTypeToLoad);
                    if(aModule.CustomCurrentUserMenuFiles != null)
                        LoadMenuFilesFromArrayList(aApplication.Name, aModule.Name, aModule.CustomCurrentUserMenuPath, aModule.CustomCurrentUserMenuFiles, aApplication.Type, commandsTypeToLoad);
                }
            }

            // Adesso che ho caricato i menu di TUTTE le applicazioni ed il mio menu completo, devo
            // andare a vedere se ci sono dei nuovi report 
            // Lo faccio DOPO aver caricato i men� di tutti i moduli di ciascuna applicazione di modo che il
            // gruppo dei report dell'utente compaia sempre per ultimo!!!
            foreach (ApplicationInfo aApplication in CurrentPathFinder.ApplicationInfos)
            {
                //report dell'utente
                if ((commandsTypeToLoad & CommandsTypeToLoad.Report) == CommandsTypeToLoad.Report)
                    SearchAndAddUserCreatedReportsGroup(aApplication);
            }

            //TODO x ora nn esiste security
            //if (!ignoreAllSecurityChecks)
            //{
            //    if (menuSecurityFilter != null)
            //    {
            //        menuSecurityFilter.Filter(cachedInfos.AppsMenuXmlParser);
            //        if (environmentStandAlone)
            //            menuSecurityFilter.Filter(cachedInfos.EnvironmentXmlParser);
            //    }
            //}
        }


        //---------------------------------------------------------------------------
        public static string GetFullMenuCachingFullFileName(string user)
        {
            string clientInstallationPath = PathFinder.PathFinderInstance.GetAppDataPath(true);
            clientInstallationPath = Path.Combine(clientInstallationPath, user);

            if (!PathFinder.PathFinderInstance.ExistPath(clientInstallationPath))
                PathFinder.PathFinderInstance.CreateFolder(clientInstallationPath, false);

            return Path.Combine(
                clientInstallationPath,
                string.Format(FullMenuCachingFileName, CultureInfo.CurrentUICulture.Name)
                );
        }

        /// <summary>
        /// Se trovo report costruiti dall'utente aggiungo un gruppo denominato "Report dell'utente"
        /// nel men�
        /// Tale gruppo � suddiviso per moduli (cio� per ciascuna directory di modulo che contiene
        /// nuovi report creo una relativa voce di men� avente come titolo il nome del modulo stesso). 
        /// All'interno di ciascuna di tali voci si hanno come comandi i report suddetti.
        /// I comandi hanno come titolo il nome file del report.
        /// </summary>
        //---------------------------------------------------------------------------
        private void SearchAndAddUserCreatedReportsGroup(ApplicationInfo aApplication)
        {
            foreach (ModuleInfo aModule in aApplication.Modules)
            {
                List<string> userCreatedReports = SearchUserCreatedReportFiles(aApplication, aModule.Name);
                AddUserCreatedReportsGroup(aApplication.Name, aModule, userCreatedReports);
            }
        }

        //---------------------------------------------------------------------------
        private static List<DocumentInfo> GetCustomDocuments(
            MenuXmlParser xmlParser,
            CommandsTypeToLoad commandsTypeToLoad,
            PathFinder pathFinder
            )
        {
            List<DocumentInfo> dynamicDocuments = new List<DocumentInfo>();

            if (
                (commandsTypeToLoad & CommandsTypeToLoad.Form) != CommandsTypeToLoad.Form &&
                (commandsTypeToLoad & CommandsTypeToLoad.Batch) != CommandsTypeToLoad.Batch
                )
                return dynamicDocuments;

            string nodeTypeName = null;
            foreach (ApplicationInfo bai in pathFinder.ApplicationInfos)
            {
                foreach (ModuleInfo bmi in bai.Modules)
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

            //Abbiamo usato la IndexOf perch� la Regex in questo caso � 5 volte pi� lenta da test fatti.
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

                    //per ogni nodo verifico se il parent node � dello stesso tipo
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
            ModuleInfo bmi = CurrentPathFinder.GetModuleInfoByName(aApplicationName, moduleName);
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
        public string GetValidUserDirectoryOrFileName()
        {
            return GetValidUserDirectoryOrFileName(menuPathFinder);
        }

        #endregion

        #region MenuInfo public static methods

        //---------------------------------------------------------------------------
        public static string GetValidUserDirectoryOrFileName(PathFinder aPathFinder)
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
                                    NameSolverStrings.Directoryseparetor,
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
        public static string GetExternalDocumentDescription(PathFinder aPathFinder, MenuXmlNode aDocumentNode)
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
        public static string SetExternalDocumentDescription(PathFinder aPathFinder, MenuXmlNode aDocumentNode)
        {
            if (aDocumentNode == null)
                return String.Empty;

            string externalDocumentDescription = GetExternalDocumentDescription(aPathFinder, aDocumentNode);

            if (externalDocumentDescription != null && aDocumentNode.ExternalDescription == null)
                aDocumentNode.ExternalDescription = externalDocumentDescription;

            return externalDocumentDescription;
        }

        //---------------------------------------------------------------------------
        public static string GetExternalBatchDescription(PathFinder aPathFinder, MenuXmlNode aBatchNode)
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
        public static string SetExternalBatchDescription(PathFinder aPathFinder, MenuXmlNode aBatchNode)
        {
            if (aBatchNode == null)
                return String.Empty;

            string externalBatchDescription = GetExternalBatchDescription(aPathFinder, aBatchNode);

            if (externalBatchDescription != null && aBatchNode.ExternalDescription == null)
                aBatchNode.ExternalDescription = externalBatchDescription;

            return externalBatchDescription;
        }

        //---------------------------------------------------------------------------
        public static string GetExternalFunctionDescription(PathFinder aPathFinder, MenuXmlNode aFunctionNode)
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
        public static string SetExternalFunctionDescription(PathFinder aPathFinder, MenuXmlNode aFunctionNode)
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
        public static string GetExternalReportDescription(PathFinder aPathFinder, MenuXmlNode aReportNode)
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
        public static string GetReportDescriptionFromNameSpace(PathFinder aPathFinder, INameSpace reportNameSpace)
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

            //TODO il terzo parametro � la lingua dell'utente corrente serve per poter trovare alcuni file nella 
            //standard divisi per lingua. Con string.empty cerca nella cartella di default 19/6/2006
            string reportFileName = aPathFinder.GetFilename(reportNameSpace, string.Empty);

            if (reportFileName == null || reportFileName.Length == 0 || !PathFinder.PathFinderInstance.ExistFile(reportFileName))
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
        public static string SetExternalReportDescription(PathFinder aPathFinder, MenuXmlNode aReportNode)
        {
            if (aReportNode == null)
                return String.Empty;

            string externalReportDescription = GetExternalReportDescription(aPathFinder, aReportNode);

            if (externalReportDescription != null && aReportNode.ExternalDescription == null)
                aReportNode.ExternalDescription = externalReportDescription;

            return externalReportDescription;
        }

  

        //---------------------------------------------------------------------------
        public static string SetExternalDescription(PathFinder aPathFinder, MenuXmlNode aCommandNode)
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

        ////---------------------------------------------------------------------------
        //public static bool GetReportFileDateTimes(PathFinder aPathFinder, MenuXmlNode aReportNode, out DateTime creationTime, out DateTime lastWriteTime)
        //{
        //    creationTime = DateTime.MinValue;
        //    lastWriteTime = DateTime.MinValue;

        //    if
        //        (
        //        aPathFinder == null ||
        //        aReportNode == null ||
        //        !(aReportNode.IsRunReport || aReportNode.IsReportShortcut) ||
        //        aReportNode.ItemObject == null ||
        //        aReportNode.ItemObject.Length == 0
        //        )
        //        return false;

        //    if (aReportNode.ReportFileCreationTime != DateTime.MinValue)
        //    {
        //        creationTime = aReportNode.ReportFileCreationTime;
        //        if (aReportNode.ReportFileLastWriteTime != DateTime.MinValue)
        //        {
        //            lastWriteTime = aReportNode.ReportFileLastWriteTime;
        //            return true;
        //        }
        //    }

        //    NameSpace reportNameSpace = new NameSpace(aReportNode.ItemObject, NameSpaceObjectType.Report);

        //    //TODO il terzo parametro � la lingua dell'utente corrente serve per poter trovare alcuni file nella 
        //    //standard divisi per lingua. Con string.empty cerca nella cartella di default 19/6/2006
        //    string fullReportFileName = aPathFinder.GetFilename(reportNameSpace, string.Empty);

        //    if (fullReportFileName == null || fullReportFileName.Length == 0 || !aPathFinder.ExistFile(fullReportFileName))
        //        return false;

        //    try
        //    {
        //        TBFile reportFileInfo = new TBFile(fullReportFileName, aPathFinder.GetAlternativeDriverIfManagedFile(fullReportFileName));

        //        creationTime = reportFileInfo.CreationTime;
        //        lastWriteTime = reportFileInfo.LastWriteTime;
        //        reportFileInfo.Dispose();
        //        return true;
        //    }
        //    catch (IOException)
        //    {
        //        creationTime = DateTime.MinValue;
        //        lastWriteTime = DateTime.MinValue;

        //        return false;
        //    }
        //}


        #endregion
    }

    #endregion


    //============================================================================
  //  public delegate void MenuLoaderEventHandler(object sender, MenuInfo aMenuInfo);
    //public delegate void MenuParserEventHandler(object sender, MenuParserEventArgs e);
  //  public delegate void FavoritesActionEventHandler(object sender, MenuXmlNode aNode);

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
        private PathFinder pathFinder = null;
        private string authenticationToken = string.Empty;


        #region MenuLoader constructors

        //----------------------------------------------------------------------------
        public MenuLoader(PathFinder aPathFinder, string authenticationToken, bool aEnvironmentStandAloneFlag)
        {
            pathFinder = aPathFinder;
            this.authenticationToken = authenticationToken;

            environmentStandAlone = aEnvironmentStandAloneFlag;

        }


        #endregion

        //----------------------------------------------------------------------------
        public bool LoadAllMenus(bool ignoreAllSecurityChecks, bool clearCachedData)
        {
            return LoadAllMenus(CommandsTypeToLoad.All, ignoreAllSecurityChecks, clearCachedData);
        }

        //----------------------------------------------------------------------------
        public bool LoadAllMenus(CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks, bool clearCachedData)
        {
            // Per decidere se si deve applicare o meno il filtro sul men� oltre a 
            // verificare l'attivazione del modulo della sicurezza, si deve anche 
            // controllare che l'azienda alla quale ci si ?connessi sia effettivamente
            // posta sotto sicurezza
            // Inoltre, � importante notare che il controllo dell'azienda sotto sicurezza va
            // fatto SOLO SE si � loggati !!! 
            bool applySecurityFilter = false;

            LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);

            if (session == null)
                return false;

            //if (!ignoreAllSecurityChecks)
            //    applySecurityFilter = session.IsActivated("MicroareaConsole", "SecurityAdmin") &&
            //        (
            //        session.LoginManagerSessionState == LoginManagerState.Logged && session.Security
            //        );

            return LoadAllMenus(applySecurityFilter, commandsTypeToLoad, ignoreAllSecurityChecks, clearCachedData);
        }

        //----------------------------------------------------------------------------
        public bool LoadAllMenus(bool applySecurityFilter, CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks, bool clearCachedData)
        {
            menuInfo = new MenuInfo(pathFinder, authenticationToken, applySecurityFilter);

            if (clearCachedData)
                Microarea.Common.Generic.InstallationInfo.Functions.ClearCachedData(menuInfo.CurrentPathFinder.User);

            menuInfo.ScanStandardMenuComponents(environmentStandAlone, commandsTypeToLoad);
            menuInfo.ScanCustomMenuComponents();

            string file = MenuInfo.CachedMenuInfos.GetStandardMenuCachingFullFileName(pathFinder.User);
            menuInfo.LoadAllMenuFiles(environmentStandAlone, commandsTypeToLoad, ignoreAllSecurityChecks, clearCachedData || !PathFinder.PathFinderInstance.ExistFile(file));

            return true;
        }

        #region MenuLoader public properties
        //---------------------------------------------------------------------------
        public MenuXmlParser AppsMenuXmlParser { get { return (menuInfo != null) ? menuInfo.AppsMenuXmlParser : null; } }
        //---------------------------------------------------------------------------
        public MenuXmlParser EnvironmentXmlParser { get { return (menuInfo != null) ? menuInfo.EnvironmentXmlParser : null; } }
        #endregion

        #region MenuLoader static methods

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

   
    }

    #endregion
}