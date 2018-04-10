using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
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
                    return ;

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

        [Serializable]
        public class CachedMenuInfos
        {
            private const string StandardMenuCachingFileName = "StandardMenu.{0}.xml";
            private static XmlSerializer menuSerializer;
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
            public string GetStandardMenuCachingFullFileName(string companyName)
            {
                string path = PathFinder.PathFinderInstance.GetCustomCompanyPath(companyName);

                if (!pathFinder.ExistPath(path))
                    pathFinder.CreateFolder(path, true);

                path = Path.Combine(path, NameSolverStrings.AllUsers);

                if (!pathFinder.ExistPath(path))
                    pathFinder.CreateFolder(path, true);

                return Path.Combine(path, string.Format(StandardMenuCachingFileName, CultureInfo.CurrentUICulture.Name));
            }

            //---------------------------------------------------------------------------
            public  bool Load(DateTime date)
            {
                string file = GetStandardMenuCachingFullFileName(pathFinder.Company);
                if (!pathFinder.ExistFile(file))
                    return true;
                try
                {
                    XmlSerializer ser = GetSerializer();
                    using (Stream sw = pathFinder.GetStream(file, false))
                    {
                        CachedMenuInfos infos = ser.Deserialize(sw) as CachedMenuInfos;

                        DateTime cacheUtdDateTime = DateTime.SpecifyKind(infos.CacheDate, DateTimeKind.Utc);
                        //Se rispetto alla struttura salvata non corrispondono le informazioni
                        //richieste, non uso la cache e rigenero tutto dinamicamente						
                        if (infos.CommandsTypeToLoad == CommandsTypeToLoad &&
                            infos.ConfigurationHash == ConfigurationHash &&
                            infos.Culture == CultureInfo.CurrentUICulture.Name &&
                            DateTime.Compare(cacheUtdDateTime, date) > 0 &&
                            infos.InstallationDate == InstallationData.InstallationDate)
                            return true;

                        return false;
                    }
                }
                catch (Exception e)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(e.Message + " ---------");

                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", sb.ToString());
                    sb.Clear();

                    Debug.WriteLine(e.ToString());
                    return true;
                }
            }
            //---------------------------------------------------------------------------
            public CachedMenuInfos Load(CommandsTypeToLoad commandsTypeToLoad, string configurationHash, string company)
            {
                //Dve rimanere  cosi
                string file = GetStandardMenuCachingFullFileName(company);
                if (!pathFinder.ExistFile(file))
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
                catch (Exception e)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(e.Message + " ---------");

                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", sb.ToString());
                    sb.Clear();

                    Debug.Fail(e.ToString());
                    return null;
                }
            }

            //---------------------------------------------------------------------------
            public void Save(string company)
            {
                string file = GetStandardMenuCachingFullFileName(company);
                try
                {
                    pathFinder.SaveCachedMenuSerialization(file, this);
                }
                catch (Exception e)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(e.Message + " ---------");

                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", sb.ToString());
                    sb.Clear();
                }
            }

            //---------------------------------------------------------------------------
            public static void Delete(string company, PathFinder pathFinder, CommandsTypeToLoad commandsTypeToLoad)
            {
                try
                {
                    string configurationHash = LoginManager.LoginManagerInstance.GetConfigurationHash();

                    CachedMenuInfos cachedMenuInfos = new CachedMenuInfos(commandsTypeToLoad, configurationHash, pathFinder);

                        string file = cachedMenuInfos.GetStandardMenuCachingFullFileName(company);
                    string dirName = Path.GetDirectoryName(file);
                    if (pathFinder.ExistPath(dirName))
                        pathFinder.RemoveFolder(dirName, true, false, false);
                }
                catch (Exception e)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(e.Message + " ---------");

                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", sb.ToString());
                    sb.Clear();
                }
            }
        }
        // la classe MenuInfo costruisce un array di file di men?da caricare

        private CachedMenuInfos cachedInfos = null;

        private MenuSecurityFilter menuSecurityFilter = null;
        private string authenticationToken = null;
        private PathFinder menuPathFinder = null;

 
        #region MenuInfo constructors

        //---------------------------------------------------------------------------
        public MenuInfo()
        {

        }

        //---------------------------------------------------------------------------
        public MenuInfo(PathFinder aPathFinder, string authenticationToken, bool applySecurityFilter)
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
        }

        //---------------------------------------------------------------------------
        public MenuInfo(PathFinder aPathFinder) : this(aPathFinder, null, false)
        {
        }

        #endregion

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

            if (menuFilesToLoad == null || menuFilesToLoad.Count == 0 || filesPath == null || filesPath.Length == 0 || !CurrentPathFinder.ExistPath(filesPath))
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
                currentUserCustomReportFiles = CurrentPathFinder.GetFiles(customModuleReportPath + NameSolverStrings.Directoryseparetor + Microarea.Common.NameSolver.PathFinder.GetUserPath(userDirectoryName), "*" + NameSolverStrings.WrmExtension);
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
                            ApplicationInfo.IsValidReportFileName(currentUserCustomReportFileInfo.Name) &&
                            !ExistsReportMenuCommand(aApplication.Name, aModuleName, currentUserCustomReportFileInfo.Name) &&
                            !ExistInCustomizedMenu(this.AppsMenuXmlParser.MenuXmlDoc, reportNamespace, MenuXmlNode.XML_TAG_REPORT) &&
                            (
                            addAllCustomReports ||
                            !CurrentPathFinder.ExistFile(standardModuleReportpath + NameSolverStrings.Directoryseparetor + currentUserCustomReportFileInfo.Name)
                             )
                            )
                            userCreatedReports.Add(currentUserCustomReportFileInfo.Name);
                    }
                }
            }
            // Una volta caricati gli eventuali nuovi report per l'utente corrente posso vedere quali altri
            // report di questo tipo trovo sotto AllUsers
            if (CurrentPathFinder.ExistPath(customModuleReportPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.AllUsers))
            {
                List<TBFile> allUsersCustomReportFiles = CurrentPathFinder.GetFiles(customModuleReportPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.AllUsers, "*" + NameSolverStrings.WrmExtension);
                if (allUsersCustomReportFiles != null && allUsersCustomReportFiles.Count > 0)
                {
                    foreach (TBFile allUsersCustomReportFileInfo in allUsersCustomReportFiles)
                    {
                        string reportNamespace = string.Join(
                            ".",
                            aApplication.Name,
                            aModuleName,
                            allUsersCustomReportFileInfo.Name.Replace(NameSolverStrings.WrmExtension, string.Empty)
                            );

                        // Prima controllo che non esista un file omonimo sotto la standard o che
                        // il report non venga mai menzionato nel men?di applicazione.
                        if
                            (
                            ApplicationInfo.IsValidReportFileName(allUsersCustomReportFileInfo.Name) &&
                            !ExistsReportMenuCommand(aApplication.Name, aModuleName, allUsersCustomReportFileInfo.Name) &&
                            !ExistInCustomizedMenu(this.AppsMenuXmlParser.MenuXmlDoc, reportNamespace, MenuXmlNode.XML_TAG_REPORT) &&
                            (
                            addAllCustomReports ||
                            !CurrentPathFinder.ExistFile(standardModuleReportpath + NameSolverStrings.Directoryseparetor + allUsersCustomReportFileInfo.Name)
                            )
                            )
                        {
                            bool ignoreReport = false;
                            // Adesso vedo se ho precedentemente trovato un report con lo stesso nome relativo all'utente corrente
                            if (currentUserCustomReportFiles != null && currentUserCustomReportFiles.Count > 0)
                            {
                                foreach (TBFile currentUserCustomReportFileInfo in currentUserCustomReportFiles)
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

        //---------------------------------------------------------------------------
        public bool LoadCachedStandardMenu(CommandsTypeToLoad commandsTypeToLoad, DateTime dateTime)
        {

            string configurationHash = LoginManager.LoginManagerInstance.GetConfigurationHash();
            try
            {
                CachedMenuInfos cachedMenuInfos = new CachedMenuInfos(commandsTypeToLoad, configurationHash, menuPathFinder);
                
                return cachedMenuInfos.Load(dateTime);
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(e.Message + " ---------");

                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", sb.ToString());
                sb.Clear();

                Debug.WriteLine("Exception thrown in MenuInfo.LoadCachedStandardMenu: " + e.Message);
                return true;
            }
        }

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

            if (cachedInfos == null)
            {
                string configurationHash = LoginManager.LoginManagerInstance.GetConfigurationHash();
                cachedInfos = new CachedMenuInfos(commandsTypeToLoad, configurationHash, menuPathFinder);
            }

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
                if (appInfo.ApplicationType != ApplicationType.TaskBuilderApplication &&
                    appInfo.ApplicationType != ApplicationType.TaskBuilder &&
                    appInfo.ApplicationType != ApplicationType.Customization)
                    continue;

                ApplicationMenuInfo aApplication = new ApplicationMenuInfo(appInfo.Name, appInfo.ApplicationType);
                aApplication.ClearModulesMenuInfos();

                if (appInfo.Modules != null && aApplication != null)
                {
                    foreach (ModuleInfo moduleInfo in appInfo.Modules)
                    {
                        if (!activatedModules.Exists((s) => { return string.Compare(s, appInfo.Name + "." + moduleInfo.Name, StringComparison.OrdinalIgnoreCase) == 0; }))
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

                            if (menuFiles == null || menuFiles.Count <= 0)
                                continue;

                            foreach (TBFile aMenuFileInfo in menuFiles)
                                aModule.AddStandardMenuFile(aMenuFileInfo.Name);
                        }
                        else if (CurrentPathFinder.ExistPath(moduleMenuDirInfopath))
                        {
                            aModule.StandardMenuPath = moduleMenuDirInfopath;
                            List<TBFile> menuFiles = CurrentPathFinder.GetFiles(moduleMenuDirInfopath, "*" + NameSolverStrings.MenuExtension);
                            if (menuFiles.Count <= 0)
                                continue;

                            foreach (TBFile aMenuFileInfo in menuFiles)
                                aModule.AddStandardMenuFile(aMenuFileInfo.Name );
                        }
                        aApplication.AddModuleMenuInfos(aModule);
                    }
                }
                ApplicationsInfo.Add(aApplication);
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


            foreach(ApplicationMenuInfo aApplication in ApplicationsInfo)
            {
                if
                    (aApplication == null || aApplication.ModulesMenuInfos == null ||aApplication.ModulesMenuInfos.Count == 0)
                    continue;

                foreach (ModuleMenuInfo aModule in aApplication.ModulesMenuInfos)
                {
                    string fullCustomPath = CurrentPathFinder.GetCustomModulePath(aApplication.Name, aModule.Name);

                    if (CurrentPathFinder.ExistPath(fullCustomPath))
                    {
                        aModule.SetCustomMenuPaths(fullCustomPath, userName);

                        List<TBFile> menuFiles = null;
                        if (CurrentPathFinder.ExistPath(aModule.CustomAllUsersMenuPath))
                        {
                            menuFiles = CurrentPathFinder.GetFiles(fullCustomPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.AllUsers + NameSolverStrings.Directoryseparetor, "*" + NameSolverStrings.MenuExtension);
                            if (menuFiles.Count > 0)
                            {
                                foreach (TBFile aMenuFileInfo in menuFiles)
                                    aModule.AddCustomAllUsersMenuFile(aMenuFileInfo.Name);
                            }
                        }
                        if (userName != null && userName.Length > 0 && userName != NameSolverStrings.AllUsers)
                        {
                            if (CurrentPathFinder.ExistPath(aModule.CustomCurrentUserMenuPath))
                            {
                                menuFiles = CurrentPathFinder.GetFiles(Microarea.Common.NameSolver.PathFinder.GetUserPath(userName), "*" + NameSolverStrings.MenuExtension);
                                if (menuFiles.Count > 0)
                                {
                                    foreach (TBFile aMenuFileInfo in menuFiles)
                                        aModule.AddCustomCurrentUserMenuFile(aMenuFileInfo.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------------
        public void LoadAllMenuFiles(bool environmentStandAlone, CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks)
        {
            if (menuPathFinder == null)
                return;
            cachedInfos.AppsMenuXmlParser = new MenuXmlParser();

            if (environmentStandAlone)
               cachedInfos.EnvironmentXmlParser = new MenuXmlParser();

            foreach (ApplicationMenuInfo aApplication in ApplicationsInfo)
            {
                if (!aApplication.HasModulesMenuInfos)
                    continue;

                foreach (ModuleMenuInfo aModule in aApplication.ModulesMenuInfos)
                {
                    LoadMenuFilesFromArrayList(aApplication.Name, aModule.Name, aModule.StandardMenuPath, aModule.StandardMenuFiles, aApplication.Type, commandsTypeToLoad);
                }
            }

            cachedInfos.Save(menuPathFinder.Company);
            
            foreach (ApplicationMenuInfo aApplication in ApplicationsInfo)
            {
                if (!aApplication.HasModulesMenuInfos)
                    continue;

                foreach (ModuleMenuInfo aModule in aApplication.ModulesMenuInfos)
                {
                    if (aModule.CustomAllUsersMenuFiles != null)
                        LoadMenuFilesFromArrayList(aApplication.Name, aModule.Name, aModule.CustomAllUsersMenuPath, aModule.CustomAllUsersMenuFiles, aApplication.Type, commandsTypeToLoad);
                    if (aModule.CustomCurrentUserMenuFiles != null)
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

            if (reportFileName == null || reportFileName.Length == 0 || !aPathFinder.ExistFile(reportFileName))
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
        #endregion
    }

    #endregion


    //============================================================================
    //public delegate void MenuLoaderEventHandler(object sender, MenuInfo aMenuInfo);
    //public delegate void MenuParserEventHandler(object sender, MenuParserEventArgs e);
    //public delegate void FavoritesActionEventHandler(object sender, MenuXmlNode aNode);

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
        public MenuInfo MenuInfo { get { return menuInfo; } }
        private MenuInfo menuInfo = null;
        private bool environmentStandAlone = false;
        private PathFinder pathFinder = null;
        private string authenticationToken = string.Empty;

        //private IBrandLoader brandLoader = null;

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
        public bool IsCached(DateTime dateTime)
        {
            if (menuInfo == null)
                menuInfo = new MenuInfo(pathFinder, authenticationToken, false); //todo LARA questo false sarebbe applysecurity che ancora nn abbiamo
            
            return  menuInfo.LoadCachedStandardMenu(CommandsTypeToLoad.All,  dateTime); 
        }
        //----------------------------------------------------------------------------
        public bool LoadAllMenus(bool applySecurityFilter, CommandsTypeToLoad commandsTypeToLoad, bool ignoreAllSecurityChecks, bool clearCachedData)
        {
           
            menuInfo = new MenuInfo(pathFinder, authenticationToken, applySecurityFilter);

            if (clearCachedData)
                Microarea.Common.Generic.InstallationInfo.Functions.ClearCachedData(menuInfo.CurrentPathFinder.Company, pathFinder, commandsTypeToLoad);

            menuInfo.ScanStandardMenuComponents(environmentStandAlone, commandsTypeToLoad);
            menuInfo.ScanCustomMenuComponents();

            menuInfo.LoadAllMenuFiles(environmentStandAlone, commandsTypeToLoad, ignoreAllSecurityChecks);

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