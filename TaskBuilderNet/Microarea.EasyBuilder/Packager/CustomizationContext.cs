using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	
	//=========================================================================
	/// <summary>
	/// The customization context is the virtual space where all changes to the standard
	/// application are saved.
	/// </summary>
	//-----------------------------------------------------------------------------
	public partial class CustomizationContext : BaseCustomizationContext, ICustomizationContext
	{
		internal event EventHandler<CustomListItemAddedEventArgs> AddedItem;

		private IDiagnostic diagnostic;

		private LoginManager loginManager = new LoginManager();
		private List<string> activeDocuments;

		/// <summary>
		/// Internal Use
		/// </summary>
		public event EventHandler<EventArgs> CurrentEasyBuilderAppChanged;
		
		private IList<IEasyBuilderApp> easyBuilderApplications;
		private IEasyBuilderApp currentEasyBuilderApp;

		/// <summary>
		/// Internal Use
		/// </summary>
		private string currentApplication = null;

		/// <summary>
		/// Internal Use
		/// </summary>
		private string currentModule = null;

		/// <summary>
		/// L'applicazione corrente della customizzazione
		/// </summary>
		//-----------------------------------------------------------------------------
		public string CurrentApplication { get	{ return currentApplication; } set { currentApplication = value; } }

		/// <summary>
		/// Il modulo corrente della customizzazione
		/// </summary>
		//-----------------------------------------------------------------------------
		public string CurrentModule { get { return currentModule; } set { currentModule = value; } }


		//-----------------------------------------------------------------------------
		/// <summary>
		/// Internal use
		/// </summary>
		/// <returns></returns>
		public bool ShouldStandardizationsBeAvailable()
        {
            try
            {
                PathFinder pf = new PathFinder("", "");

                SettingItem i = pf.GetSettingItem("Extensions", "EasyStudio", "EasyStudio", "Standardizations");

                if (i == null)
                    return false;

                bool b = false;
                try { b = (string)i.Values[0] != "0"; }
                catch { }

                return b;
            }
            catch
            {
                return false;
            }
        }

		//-----------------------------------------------------------------------------
        static CustomizationContext()
        {
            try
            {
                //Logica per l'aggiornamento del file di proprietà.
                //Come scritto nei remarks di http://msdn.microsoft.com/en-us/library/system.configuration.localfilesettingsprovider.upgrade.aspx
                //Standard Windows Forms and console applications must manually call Upgrade,
                //because there is not a general, automatic way to determine when such an application
                //is first run. The two common ways to do this are either from the installation program
                //or using from the application itself, using a persisted property, often named something
                //like IsFirstRun.
                //Questo vale solo quando l'applicazione gira solo come installazione server perchè,
                //quando invece gira come installazione client, è click once che pensa all'aggiornamento
                //del file di proprietà:
                //http://msdn.microsoft.com/en-us/library/ms228995.aspx come scritto al titolo "Version Upgrades"
                if (Settings.Default.IsFirstRun)
                {
                    try { Settings.Default.Upgrade(); }
                    catch { }
                    Settings.Default.IsFirstRun = false;
				
					BaseCustomizationContext.CustomizationContextInstance.SaveSettings();

		
				}
            }
            catch { }

            try
            {
                //Imposta la currentEasyBuilderApp leggendola dal file Settings
                //currentEasyBuilderApp = SetCurrentEasyBuilderApp();
            }
            catch { }
        }

		

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public void SaveSettings()
        {
            try
            {
                Settings.Default.Save();
            }
            catch (Exception exc)
            {
				Debug.WriteLine(String.Format("Unable to save settings: ", exc.ToString()));
			}
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Internal use - Gets a Diagnostic to log errors.
        /// </summary>
        public IDiagnostic Diagnostic
        {
            get
            {
                lock (lockObject)
                {
                    if (diagnostic == null)
                        diagnostic = new Diagnostic("CustomizationManager");

                    return diagnostic;
                }
            }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Ritorna una lista delle customizzazioni attive
        /// </summary>
        public List<string> ActiveDocuments
        {
            get
            {
                lock (lockObject)
                {
                    if (activeDocuments != null)
                        return activeDocuments;

                    activeDocuments = new List<string>();
                    foreach (IEasyBuilderApp cust in EasyBuilderApplications)
                    {
                        foreach (CustomListItem item in cust.EasyBuilderAppFileListManager.CustomList)
                        {
                            if (!item.IsActiveDocument)
                                continue;

                            string custNs = GetPseudoNamespaceFromFullPath(item.FilePath, item.PublishedUser);
                            if (custNs.IsNullOrEmpty())
                                continue;

                            if (!activeDocuments.ContainsNoCase(custNs))
                                activeDocuments.Add(custNs);
                        }
                    }

                    return activeDocuments;
                }
            }
        }

		/// <summary>
		/// Internal Use
		/// </summary>
		//-----------------------------------------------------------------------------
		public void UpdateActiveDocuments(bool isActiveDocument, string filePath, string publishedUser)
        {
            string pseudoNs = GetPseudoNamespaceFromFullPath(filePath, publishedUser);
            if (!isActiveDocument && ActiveDocuments != null)
            {
                for (int i = 0; i < ActiveDocuments.Count; i++)
                {
                    if (String.Compare(ActiveDocuments[i], pseudoNs, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ActiveDocuments.RemoveAt(i);
                        return;
                    }
                }
            }

            if (isActiveDocument)
            {
                if (!ActiveDocuments.ContainsNoCase(pseudoNs))
                    ActiveDocuments.Add(pseudoNs);
            }
        }

        /// <summary>
        /// Ritorna una lista delle customizzazioni attive
        /// </summary>
        //-----------------------------------------------------------------------------
        public IList<IEasyBuilderApp> EasyBuilderApplications
        {
            get
            {
                lock (lockObject)
                {
                    if (easyBuilderApplications != null)
                        return easyBuilderApplications;

                    easyBuilderApplications = new List<IEasyBuilderApp>();

                    foreach (EasyBuilderAppDetails current in GetAllEasyBuilderAppsFileListPath())
                        easyBuilderApplications.Add(WrapExisting(current.Application, current.Module, current.ApplicationType));

                    return easyBuilderApplications;
                }
            }
        }

		/// <summary>
		/// Internal use
		/// </summary>
		//--------------------------------------------------------------------------------
		public string DynamicLibraryName { get { return NameSolverStrings.DynamicLibraryName; } }

        /// <summary>
        /// Gets the current customization.
        /// </summary>
        /// <seealso cref="Microarea.EasyBuilder.Packager.Customization"/>
        //-----------------------------------------------------------------------------
        public IEasyBuilderApp CurrentEasyBuilderApp
        {
            get
            {
                lock (lockObject)
                {
                    if (currentEasyBuilderApp != null)
                        return currentEasyBuilderApp;

                    return FindEasyBuilderApp(currentApplication, currentModule);
                }
            }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating if the current EasyBuilder application is a Standardization.
        /// </summary>
        public bool IsCurrentEasyBuilderAppAStandardization
        {
            get
            {
                return currentEasyBuilderApp == null ?
                    false
                    :
                    currentEasyBuilderApp.ApplicationType == ApplicationType.Standardization;
            }
        }

        /// <summary>
        /// Return true if exists a current easybuilder application
        /// </summary>
        //-----------------------------------------------------------------------------
        public bool ExistsCurrentEasyBuilderApp
        {
            get {
				if (	currentApplication == null 
					||	currentModule == null
					||	FindEasyBuilderApp(currentApplication, currentModule)==null)
				{
					ChooseCustomizationContext.Choose();
					return false;
				}					
				return true;
			}
        }

		/// <summary>
		/// Internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IsActiveApplication(string application)
        {
            return application.CompareNoCase(currentApplication);
        }

        /// <summary>
        /// Imposta il context su una diversa customizzazione, impostando applicazione e modulo.
        /// Se il modulo è nullo, viene impostato quello di default
        /// Se l'applicazione è nulla, ArgumentException
        /// </summary>
        //-----------------------------------------------------------------------------
        public void ChangeEasyBuilderApp(string appName, string modName = "")
        {
            //se non sto effettivamente cambiando la customizzazione (è la stessa di prima) non faccio niente
            lock (lockObject)
            {
                IEasyBuilderApp cust = null;
                if (!appName.IsNullOrEmpty())
                {
                    if (!modName.IsNullOrEmpty())
                        cust = FindEasyBuilderApp(appName, modName);
                    else
                    {
                        IList<IEasyBuilderApp> custs = FindEasyBuilderAppsByApplicationName(appName);
                        if (custs.Count > 0)
                        {
                            cust = custs[0];
                            modName = cust.ModuleName;
                        }
                    }
                }
                currentEasyBuilderApp = cust;

				Settings.Default.LastApplication = currentApplication = currentEasyBuilderApp != null ? currentEasyBuilderApp.ApplicationName : string.Empty;
				Settings.Default.LastModule = currentModule = currentEasyBuilderApp != null ?  currentEasyBuilderApp.ModuleName : string.Empty;
				BaseCustomizationContext.CustomizationContextInstance.SaveSettings();

				if (CurrentEasyBuilderAppChanged != null)
                    CurrentEasyBuilderAppChanged(null, EventArgs.Empty);
            }
        }

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public void GetApplicationAndModuleFromCustomFile(
            string customListFile,
            out string application,
            out string module
            )
        {
            string tempPath = customListFile
                .ReplaceNoCase(NameSolverStrings.CustomListFileExtension, string.Empty)
                .ReplaceNoCase(NameSolverStrings.StandardListFileExtension, String.Empty);

            string[] tokens = tempPath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);

            application = tokens[tokens.Length - 2];
            module = tokens[tokens.Length - 1];
        }

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IEasyBuilderApp FindEasyBuilderApp(string customListFile)
        {
            foreach (IEasyBuilderApp item in EasyBuilderApplications)
            {
                if (item.EasyBuilderAppFileListManager.CustomListFullPath.CompareNoCase(customListFile))
                    return item;
            }
            return null;
        }

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IEasyBuilderApp FindEasyBuilderApp(string appName, string modName)
        {
            foreach (IEasyBuilderApp item in EasyBuilderApplications)
            {
                if (modName.CompareNoCase(item.ModuleName) && appName.CompareNoCase(item.ApplicationName))
                    return item;
            }
            return null;
        }

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IList<IEasyBuilderApp> FindEasyBuilderAppsByApplicationName(string appName)
        {
            List<IEasyBuilderApp> custs = new List<IEasyBuilderApp>();
            foreach (IEasyBuilderApp item in EasyBuilderApplications)
            {
                if (appName.CompareNoCase(item.ApplicationName))
                    custs.Add(item);
            }
            return custs;
        }

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IsApplicationAlreadyExisting(string applicationName)
        {
            foreach (EasyBuilderAppDetails item in GetAllEasyBuilderAppsFileListPath())
            {
                if (item.Application.CompareNoCase(applicationName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns full file paths for all the custom list.
        /// </summary>
        /// <returns>Full file paths for all the custom list,
        /// either in the 'Custom' folder or in the 'Standard' folder</returns>
        //-----------------------------------------------------------------------------
        public IList<EasyBuilderAppDetails> GetAllEasyBuilderAppsFileListPath()
        {
            List<EasyBuilderAppDetails> easyBuilderAppDetailsList = new List<EasyBuilderAppDetails>();

            try
            {
                //directory AllCompanies\Applications
                string appsPath = BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath();

                //Cerco tutti le sottodirectory
                if (!Directory.Exists(appsPath))
                    Directory.CreateDirectory(appsPath);

                string[] dirs = Directory.GetDirectories(appsPath);
                easyBuilderAppDetailsList.AddRange(GetAllEasyBuilderAppDetailsFromFileSystem(dirs, ApplicationType.Customization));

                //Cerco le standardizzazioni
                appsPath = BasePathFinder.BasePathFinderInstance.GetStandardApplicationContainerPath(ApplicationType.Standardization);

                dirs = Directory.GetDirectories(appsPath);
                easyBuilderAppDetailsList.AddRange(GetAllEasyBuilderAppDetailsFromFileSystem(dirs, ApplicationType.Standardization));

                return easyBuilderAppDetailsList;
            }
            catch
            {
                return easyBuilderAppDetailsList;
            }
        }

        //-----------------------------------------------------------------------------
        private IList<EasyBuilderAppDetails> GetAllEasyBuilderAppDetailsFromFileSystem(
            string[] dirs,
            ApplicationType appType
            )
        {
            string searchCriteria =
                appType == ApplicationType.Customization
                ?
                NameSolverStrings.CustomListFileSearchCriteria
                :
                NameSolverStrings.StandardListFileSearchCriteria;

            string searchExtension =
                appType == ApplicationType.Customization
                ?
                NameSolverStrings.CustomListFileExtension
                :
                NameSolverStrings.StandardListFileExtension;

            List<EasyBuilderAppDetails> customizationDetailsList = new List<EasyBuilderAppDetails>();
            foreach (string dir in dirs)
            {
                //per ogni sottodirectory cerco i file filtrandoli per "*.customList.xml" o "*.standardList.xml"
                if (!Directory.Exists(dir))
                    continue;

                DirectoryInfo aDirInfo = new DirectoryInfo(dir);
                string application, module = null;
                FileInfo[] fileInfos = aDirInfo.GetFiles(searchCriteria, SearchOption.TopDirectoryOnly);
                if (fileInfos != null && fileInfos.Length > 0)
                {
                    foreach (FileInfo aFileInfo in fileInfos)
                    {
                        application = aFileInfo.Directory.Name;
                        module = aFileInfo.Name.ReplaceNoCase(searchExtension, string.Empty);

                        customizationDetailsList.Add(new EasyBuilderAppDetails(application, module, appType));
                    }
                }
            }

            return customizationDetailsList;
        }

        /// <summary>
        /// Returns all EasyBuilderApps given the application name.
        /// </summary>
        /// <param name="application">The application name to be searhed.</param>
        /// <param name="applicationType">Type of application</param>
        //-----------------------------------------------------------------------------
        public IList<IEasyBuilderApp> GetEasyBuilderApps(string application, ApplicationType applicationType)
        {
            IList<IEasyBuilderApp> easyBuilderApps = new List<IEasyBuilderApp>();

            if (String.IsNullOrWhiteSpace(application) || EasyBuilderApplications.Count == 0)
                return easyBuilderApps;

            if (applicationType != ApplicationType.Customization && applicationType != ApplicationType.Standardization)
                return easyBuilderApps;

            foreach (var easyBuilderApp in EasyBuilderApplications)
                if (easyBuilderApp.ApplicationName.CompareNoCase(application) && easyBuilderApp.ApplicationType == applicationType)
                    easyBuilderApps.Add(easyBuilderApp);

            return easyBuilderApps;
        }

        /// <summary>
        /// Returns full file paths for all the custom list.
        /// </summary>
        /// <param name="application">The application name to be searhed.</param>
        /// <returns>Full file paths for all the custom list.</returns>
        //-----------------------------------------------------------------------------
        public List<string> GetAllEasyBuilderAppsFileListPath(string application)
        {
            List<string> customListFilesPath = new List<string>();

            try
            {
                string appsPath = null;
                string path = null;
                string[] files = null;
                IBaseApplicationInfo appInfo = BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(application);
                switch (appInfo.ApplicationType)
                {
                    case ApplicationType.Customization:
                        {
                            //directory AllCompanies\Applications
                            appsPath = BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath();

                            path = Path.Combine(appsPath, application);
                            if (Directory.Exists(path))
                            {
                                files = Directory.GetFiles(path, NameSolverStrings.CustomListFileSearchCriteria);
                                if (files.Length > 0)
                                {
                                    customListFilesPath.AddRange(files);
                                    return customListFilesPath;
                                }
                            }
                            break;
                        }
                        case ApplicationType.Standardization:
                        {
                            appsPath = BasePathFinder
                                .BasePathFinderInstance
                                .GetStandardApplicationContainerPath(ApplicationType.Standardization);

                            path = Path.Combine(appsPath, application);
                            if (!Directory.Exists(path))
                                return customListFilesPath;

                            files = Directory.GetFiles(path, NameSolverStrings.StandardListFileSearchCriteria);
                            if (files.Length > 0)
                                customListFilesPath.AddRange(files);

                            break;
                        }
                    default:
                        break;
                }

                return customListFilesPath;
            }
            catch (Exception exc)
            {
                BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(exc.ToString());
                return customListFilesPath;
            }
        }

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name) || char.IsDigit(name[0]))
                return false;

            foreach (char ch in name)
            {
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_')
                    continue;
                return false;
            }

            return true;
        }


		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IsFileToExport(string filefullPath)
        {
            if (String.IsNullOrWhiteSpace(filefullPath))
                return false;

            //controllo se e' un  file .xml del sales module
            FileInfo fi = new FileInfo(filefullPath);
            bool isXmlSourceSalesModule = String.Compare(fi.Directory.Name, NameSolverStrings.Modules, StringComparison.OrdinalIgnoreCase) == 0
                                            &&
                                            String.Compare(fi.Extension, NameSolverStrings.XmlExtension, StringComparison.OrdinalIgnoreCase) == 0;

            //Se il file esiste, non e' il file .xml del sales module ed ha un'estensione 
            //da non escludere lo aggiungo allo zip
            return !isXmlSourceSalesModule && File.Exists(filefullPath);
        }

		/// <summary>
		/// Adds an item to the current customization.
		/// </summary>
		/// <param name="fileFullPath">The path of the item to be added.</param>
		/// <param name="isActiveDocument">A value indicating whether the current document is the active document</param>
		/// <param name="publishedUser">The name of the user to publish for</param>
		/// <param name="save">A value indicating if changes are to be saved.</param>
		/// <param name="documentNamespace">The original document namespace, if existing.</param>
		//-----------------------------------------------------------------------------
		public void AddToCurrentCustomizationList
            (
            string fileFullPath,
            bool save = true,
            bool isActiveDocument = false,
            string publishedUser = "",
			string documentNamespace = ""
			)
        {
            if (fileFullPath.IsNullOrWhiteSpace())
                throw new ArgumentException("Invalid empty path");

            IEasyBuilderApp cust = null;
            lock (lockObject)
            {
				//Cerco la current customization, se non c'è non faccio niente
				cust = FindEasyBuilderApp(currentApplication, currentModule);
				if (cust == null)
					return;

				AddToEasyBuilderAppCustomizationList(cust, fileFullPath, save, isActiveDocument, publishedUser, documentNamespace);
            }
        }


		/// <summary>
		/// Adds an item to the specified EasyBuilder application.
		/// </summary>
		/// <param name="app">The EasyBuilder application to be added.</param>
		/// <param name="fileFullPath">The path of the item to be added.</param>
		/// <param name="isActiveDocument">A value indicating whether the current document is the active document</param>
		/// <param name="publishedUser">The name of the user to publish for</param>
		/// <param name="save">A value indicating if changes are to be saved.</param>
		/// <param name="documentNamespace">The original document namespace, if existing.</param>
		//-----------------------------------------------------------------------------
		public void AddToEasyBuilderAppCustomizationList
            (
            IEasyBuilderApp app,
            string fileFullPath,
            bool save = true,
            bool isActiveDocument = false,
            string publishedUser = "", 
			string documentNamespace = ""
            )
        {
            bool added = false;

            lock (lockObject)
            {

                //se non è attivato non faccio niente
                if (!EBLicenseManager.CanDesign)
                    return;

                if (ExcludeFileForUser(fileFullPath))
                    return;

                try
                {
                    added = app.EasyBuilderAppFileListManager.AddToCustomList(fileFullPath, save, isActiveDocument, publishedUser, documentNamespace);
                }
                catch (Exception exc)
                {
                    Diagnostic.SetError(String.Format("Error adding {0} to current customization: {1}", fileFullPath, exc.ToString()));
                    return;
                }
            }

            if (added && AddedItem != null)
                AddedItem(null, new CustomListItemAddedEventArgs(fileFullPath, app, publishedUser, isActiveDocument));
        }
        /// <summary>
        /// Ritorna true se il file appartiene all'utente specifico corrente e non è nè 
        /// una personalizzazione nè un report
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        //--------------------------------------------------------------------------------
        private bool ExcludeFileForUser(string fileFullPath)
        {
            try
            {
                if (!File.Exists(fileFullPath))
                    return true;

                //Se non c'è nessun utente allora non è un file da escludere
                string user = CUtility.GetUser().Replace("\\", ".");
                if (user.IsNullOrEmpty())
                    return false;

                FileInfo fi = new FileInfo(fileFullPath);
                //I file che non sono dll o report e appartengono all'utente specifico, vengono esclusi
                if (
                    fileFullPath.ContainsNoCase(string.Format("\\{0}\\{1}\\", NameSolverStrings.Users, user)) &&
                    !fi.Extension.CompareNoCase(NameSolverStrings.DllExtension) &&
                    !fi.Extension.CompareNoCase(NameSolverStrings.WrmExtension)
                    )
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ritorna uno pseudo namespace della personalizzazione composto da app, mod, doc, e customizzazione
        /// dato: C:\\Development\\Custom\\Companies\\AllCompanies\\Applications\\ERP\\Company\\ModuleObjects\\Titles\\Titles1.dll
        /// torna: ERP.Company.Documents.Titles.Titles1
        /// </summary>
        //-----------------------------------------------------------------------------
        public string GetPseudoNamespaceFromFullPath(string fullPath, string publishedUser)
        {
            if (!publishedUser.IsNullOrEmpty())
                publishedUser = publishedUser.Replace("\\", ".");

            //C:\\Development\\Custom\\Companies\\AllCompanies\\Applications\\ERP\\Company\\ModuleObjects\\Titles\\Titles1.dll
            if (fullPath.IsNullOrEmpty())
                return string.Empty;

            fullPath = fullPath.ReplaceNoCase(NameSolverStrings.DllExtension, string.Empty);

            string[] tokens = fullPath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);

            int tokenFolder = FindTokenIndex(tokens, NameSolverStrings.ModuleObjects);
            if (tokenFolder < 0)
                return string.Empty;

			try
			{
				//A questo punto, una volta individuata la cartella che mi interessa, (Report o module objects)
				//il namespace è composto da i due token precedenti e da quello successivo
				//Questa gestione è un azzardo, rischia di avere i tokens del nome documento e della 
				//customizzazione sbagliati se ci sono in mezzo token inprevisti
				return (publishedUser.IsNullOrEmpty())
						? string.Format
								(
								"{0}.{1}.{2}.{3}",
								tokens[tokenFolder - 2],
								tokens[tokenFolder - 1],
								tokens[tokenFolder + 1],
								tokens[tokens.Length - 1]
								)
						: string.Format
								(
								"{0}.{1}.{2}.{3}.{4}",
								tokens[tokenFolder - 2],
								tokens[tokenFolder - 1],
								tokens[tokenFolder + 1],
								publishedUser,
								tokens[tokens.Length - 1]);

            }
            catch
            {
                return string.Empty;
            }
        }

        /// <remarks />
        //-----------------------------------------------------------------------------
        public INameSpace CurrentModuleNamespace
        {
            get { return new NameSpace("Standardization.App1.Module1"); }

        }

	
		//-----------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating if the document controller identified by the
		/// given namespace is the active document for the customization context.
		/// </summary>
		/// <param name="documentNamespace"></param>
		/// <returns></returns>
		public bool IsActiveDocument(INameSpace documentNamespace)
        {
            if (ActiveDocuments.Count <= 0)
                return false;

            string user = CUtility.GetUser().Replace("\\", ".");
            foreach (string current in ActiveDocuments)
            {
                string custNs = string.Format(
                    "{0}.{1}.{2}.{3}",
                    documentNamespace.Application,
                    documentNamespace.Module,
                    documentNamespace.Document,
                    documentNamespace.Leaf
                    );

                string custUsernNs = string.Format(
                    "{0}.{1}.{2}.{3}.{4}",
                    documentNamespace.Application,
                    documentNamespace.Module,
                    documentNamespace.Document,
                    user,
                    documentNamespace.Leaf
                    );

                //Se la customizzazione che sto caricando è di default, la imposto per il menù a tendina
                if (current.CompareNoCase(custNs) || current.CompareNoCase(custUsernNs))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Returns a pseudo-namespace of the document (also for reports) made up of
        /// application name, module name and document name.
        /// torna: ERP.Company.Titles
        /// </summary>
        /// <example>
        /// Given the path C:\\Development\\Custom\\Companies\\AllCompanies\\Applications\\ERP\\Company\\ModuleObjects\\Titles\\Titles1.dll
        /// the pseudo-namespace will be ERP.Company.Titles.
        /// </example>
        /// <param name="fullPath">The path to calculate the pseudo-namespace</param>
        /// <returns>The pseudo-namespace</returns>
        //-----------------------------------------------------------------------------
        public string GetParentPseudoNamespaceFromFullPath(string fullPath)
        {
            try
            {
                //"C:\\Development\\Custom\\Companies\\Dev\\Applications\\ERP\\Accounting\\Report\\Users\\MICROAREA.bruni\\Woorm.wrm"
                //es: C:\\Development\\Custom\\Companies\\AllCompanies\\Applications\\ERP\\Shippings\\ModuleObjects\\Ports\\Ports1.dll
                if (fullPath.IsNullOrEmpty())
                    return string.Empty;

                fullPath = fullPath.ReplaceNoCase(NameSolverStrings.DllExtension, string.Empty);

                string[] tokens = fullPath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);

                //Se è un domento ritorno app.mod.doc
                int tokenFolder = FindTokenIndex(tokens, NameSolverStrings.ModuleObjects);
                if (tokenFolder > 0)
                {
                    return string.Format(
                                "{0}.{1}.{2}",
                                tokens[tokenFolder - 2],
                                tokens[tokenFolder - 1],
                                tokens[tokenFolder + 1]
                                );
                }

                //Se è un report...
                tokenFolder = FindTokenIndex(tokens, NameSolverStrings.Report);
                if (tokenFolder > 0)
                {
                    //...per User ritorno app.mod.users.user
                    string reportForUsers = string.Format("\\{0}\\{1}\\", NameSolverStrings.Report, NameSolverStrings.Users);
                    if (fullPath.ContainsNoCase(reportForUsers))
                    {
                        return string.Format(
                                "{0}.{1}.{2}.{3}",
                                tokens[tokenFolder - 2],
                                tokens[tokenFolder - 1],
                                tokens[tokenFolder + 1],
                                tokens[tokenFolder + 2]
                                );
                    }

                    //...per AllUser ritorno app.mod.AllUsers
                    string reportForAllUsers = string.Format("\\{0}\\{1}\\", NameSolverStrings.Report, NameSolverStrings.AllUsers);
                    if (fullPath.ContainsNoCase(reportForAllUsers))
                    {
                        return string.Format(
                            "{0}.{1}.{2}",
                            tokens[tokenFolder - 2],
                            tokens[tokenFolder - 1],
                            tokens[tokenFolder + 1]
                            );
                    }
                    else
                    {
                        //...per Standard ritorno app.mod.AllUsers
                        return string.Format(
                                "{0}.{1}.{2}",
                                tokens[tokenFolder - 2],
                                tokens[tokenFolder - 1],
                                tokens[tokenFolder]
                                );
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }

        //-----------------------------------------------------------------------------
        private int FindTokenIndex(string[] tokens, string tokenToFind)
        {
            int tokenFolder = -1;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].CompareNoCase(tokenToFind))
                    tokenFolder = i;
            }

            return tokenFolder;
        }

		/// <summary>
		/// Internal Use
		/// </summary>
		//--------------------------------------------------------------------------------
		public bool IsSubjectedToPublication(string filePath)
        {
            //Per ora le lascio così ma se le estensioni da pubblicare diventassero di più
            //allora converrebbe metterle in un array di classe.
            return (
                filePath.IndexOfNoCase(NameSolverStrings.DllExtension) > 0 ||
                filePath.IndexOfNoCase(NameSolverStrings.MenuExtension) > 0
                );
        }

        /// <summary>
        /// Internal Use
        /// </summary>
        //--------------------------------------------------------------------------------
        public bool HasSourceCode (string filePath)
        {
            return filePath.IndexOfNoCase(NameSolverStrings.DllExtension) > 0;
        }
        /// <summary>
        /// Internal Use
        /// </summary>
        //--------------------------------------------------------------------------------
        public void RemoveFileReadonly(string file)
        {
            FileInfo fi = new FileInfo(file);
            if (fi.Exists && fi.IsReadOnly)
                fi.IsReadOnly = false;
        }

        //--------------------------------------------------------------------------------
        private void CreateFileBackup(string fileToBackup)
        {
            try
            {
                string backupFile = fileToBackup + NameSolverStrings.BackupExtension;
                RemoveFileReadonly(backupFile);

                File.Copy(fileToBackup, backupFile, true);
            }
            catch (Exception e)
            {
                Diagnostic.SetError(string.Format(Resources.UnableToBackupFile, fileToBackup, e.Message));
            }
        }

        //--------------------------------------------------------------------------------
        private void DeleteFilesForPublish(string documentFolder, string documentFileNameWithoutExtension)
        {
            try
            {
                string[] files = Directory.GetFiles(documentFolder, documentFileNameWithoutExtension + ".*");
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.Extension.CompareNoCase(NameSolverStrings.BackupExtension))
                        continue;

                    RemoveFileReadonly(file);
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Diagnostic.SetError(string.Format(Resources.ErrorDeletingCustomizationFile, string.Empty, e.Message));
                return;
            }
        }

 

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Loads all DLLs containing customizations for the document identified by the
        /// given document namespace.
        /// </summary>
        /// <param name="documentNamespace">The namespace of the document to customize.</param>
        /// <seealso cref="Microarea.TaskBuilderNet.Interfaces.INameSpace"/>
        /// <remarks>
        /// At first all DLLs from the common folder are loaded.
        /// </remarks>
        public IList<IEBLink> GetEasyBuilderAppStandardizationLinks(INameSpace documentNamespace)
        {
            string standardAllUsersPath = BasePathFinder.BasePathFinderInstance.GetApplicationPath(documentNamespace);

            if (Directory.Exists(standardAllUsersPath))
                return EBLink.ParseFolder(standardAllUsersPath);

            return new EBLink[0];
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// Returns the full name of the assembly containing a customization for the given
        /// customization namespace and for the given user.
        /// </summary>
        public string GetEasyBuilderAppAssemblyFullName(
            INameSpace customizationNameSpace,
            string user, 
			IEasyBuilderApp app
            )
        {
            NameSpace nameSpace = new NameSpace(customizationNameSpace.FullNameSpace);
            nameSpace.Application = app.ApplicationName;
            nameSpace.Module = app.ModuleName;
            return PathFinderWrapper.GetEasyStudioAssemblyFullName(nameSpace.FullNameSpace, user);
        }


		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IBaseModuleInfo GetModuleInfo(
            string applicationName,
            string moduleName,
            ApplicationType applicationType
            )
        {
            IBaseModuleInfo moduleInfo = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(applicationName, moduleName);
            if (moduleInfo == null)
            {
                CUtility.ReloadApplication(applicationName);
                //forzo la lettura dell'applicazione
                BasePathFinder.BasePathFinderInstance.RefreshEasyBuilderApps(applicationType);

                moduleInfo = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(applicationName, moduleName);
            }

            return moduleInfo;
        }

		///<remarks></remarks>
		//-----------------------------------------------------------------------------
		public bool ContainsControllerDll(INameSpace controllerNamespace)
		{
			return currentEasyBuilderApp.EasyBuilderAppFileListManager.ContainsControllerDll(controllerNamespace);
		}

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return true if there are no more login sessions than maxLogins and
        /// no more open documents than maxDocuments.
        /// </summary>
        /// <param name="caption">The caption of the message box showed to warn the user</param>
        /// <param name="maxLogins">Maximum number of login to be checked</param>
        /// <param name="maxDocuments">Maximum number of open document to be checked</param>
        /// <param name="owner">The owner of the message box showed to warn the user.</param>
        /// <returns></returns>
        public bool NotAlone(string caption, int maxLogins, int maxDocuments, IWin32Window owner = null)
        {
            if (CUtility.GetLoginSessionCount() > maxLogins)
            {
                MessageBox.Show(
                    owner,
                    Resources.OtherSessions,
                    caption,
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                return true;
            }
            if (CUtility.GetAllOpenDocumentNumber() > maxDocuments)
            {
                MessageBox.Show(
                        owner,
                        Resources.OtherDocuments,
                        caption,
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                        );
                return true;
            }
            return false;
        }

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public INameSpace FormatDynamicNamespaceDocument(string applicationName, string moduleName, string documentName)
        {
            return new NameSpace(string.Format("{0}.{1}.{2}.{3}", applicationName, moduleName, BaseCustomizationContext.CustomizationContextInstance.DynamicLibraryName, documentName), NameSpaceObjectType.Document);
        }

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public void RemoveFromCustomListAndFromFileSystem(string path)
		{
			currentEasyBuilderApp.EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(path);
		}
	  
		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public void RemoveFromCustomListAndFromFileSystem(IEasyBuilderApp app, string path)
        {
            app.EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(path);
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// returns ApplicatioName.ModuleName of the current CustomizationContext
        /// </summary>
        /// <returns></returns>
        public string GetCurrentModuleNamespace()
        {
            return String.Concat(CurrentEasyBuilderApp.ApplicationName, ".", CurrentEasyBuilderApp.ModuleName);
        }
    }

   
}

