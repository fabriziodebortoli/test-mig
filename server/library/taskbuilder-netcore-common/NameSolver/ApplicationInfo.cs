using System;



using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;
using System.Collections;
using static Microarea.Common.Generic.InstallationInfo;

using System.Diagnostics;
using System.Threading;
using Microarea.Common.CoreTypes;
using System.Collections.Generic;
using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace Microarea.Common.NameSolver
{
	/// <summary>
	/// Estensione di ModuleInfo con le informazioni relative alla custom
	/// </summary>
	//=========================================================================
	public class ModuleInfo 
	{

        private string name;
        private string moduleConfigFile;
        private string allCompaniesCustomPath;

        protected ApplicationInfo parentApplicationInfo;
        private IDocumentsObjectInfo documentObjectsInfo;
        private ModuleConfigInfo moduleConfigInfo;

        protected ArrayList webMethods = new ArrayList();


        private IClientDocumentsObjectInfo clientDocumentsObjectInfo;
        private int currentDBRelease = 0;
        private NameSpace moduleNamespace;
        //
        private IDBObjects dbObjects; // classe con la struttura degli oggetti di database apportati dal modulo
        private DatabaseObjectsInfo databaseObjectsInfo;
        private AddOnDatabaseObjectsInfo addOnDatabaseObjectsInfo;
        private RowSecurityObjectsInfo rowSecurityObjectsInfo;
        private BehaviourObjectsInfo behaviourObjectsInfo;

        #region Costruttori

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="aParentApplicationInfo">lìapplacazione a cui appartiene il modulo</param>
        //-------------------------------------------------------------------------------
        public ModuleInfo(string moduleName, ApplicationInfo parentApplicationInfo)
		{
            name = moduleName;
           this. parentApplicationInfo = parentApplicationInfo;
            moduleNamespace = new NameSpace(parentApplicationInfo.Name + '.' + name, NameSpaceObjectType.Module);
            moduleConfigFile = Path + NameSolverStrings.Directoryseparetor + NameSolverStrings.Module + NameSolverStrings.ConfigExtension;

        }

        #endregion

        #region proprietà
        //-------------------------------------------------------------------------------
        /// <summary>
        /// L'applicationInfo della applicazione di appartenenza
        /// </summary>
        public ApplicationInfo ParentApplicationInfo { get { return parentApplicationInfo; } }
        public string ParentApplicationName { get { return (parentApplicationInfo != null) ? parentApplicationInfo.Name : String.Empty; } }
        public int CurrentDBRelease { get { return currentDBRelease; } set { currentDBRelease = value; } }

        /// <summary>
        /// Nome del modulo
        /// </summary>
        public string Name { get { return name; } set { name = value; } }
        public string Title { get { return ModuleConfigInfo.Title; } }

        //---------------------------------------------------------------------------
        public ModuleConfigInfo ModuleConfigInfo
        {
            get
            {
                if (moduleConfigInfo != null)
                    return moduleConfigInfo;

                moduleConfigInfo = new ModuleConfigInfo(name, this, moduleConfigFile);

                moduleConfigInfo.Parse();

                return moduleConfigInfo;
            }
        }

        //---------------------------------------------------------------------------
        public INameSpace NameSpace { get { return moduleNamespace; } }
        /// <summary>
        /// array delle librerie del modulo
        /// </summary>
        //---------------------------------------------------------------------------
        public IList Libraries { get { return ModuleConfigInfo.Libraries; } }

        public ModuleInfo GetStaticModuleinfo()
        {
            foreach (ApplicationInfo ai in CurrentPathFinder.ApplicationInfos)
            {
                foreach (ModuleInfo mi in ai.Modules)
                {
                    if (this.Path.CompareNoCase(mi.Path))
                        return mi as ModuleInfo;
                }
            }
            return null;
        }
        /// <summary>
        /// Per parsare i AddOnDatabaseObjects.xml
        /// </summary>
        //------------------------------------------------------------------------------
        public AddOnDatabaseObjectsInfo AddOnDatabaseObjectsInfo
        {
            get
            {
                if (addOnDatabaseObjectsInfo == null)
                {
                    //path del file AddOnDatabaseObjects.xml
                    string addOnDatabaseObjFile = GetAddOnDatabaseObjectsPath();

                    //Oggetto che sa parsare AddOnDatabaseObjects.xml
                    addOnDatabaseObjectsInfo = new AddOnDatabaseObjectsInfo(addOnDatabaseObjFile, this);

                    //se il file non esiste esco
                    if (!PathFinder.FileSystemManager.ExistFile(addOnDatabaseObjFile))
                        return addOnDatabaseObjectsInfo;

                    addOnDatabaseObjectsInfo.Parse();
                }

                return addOnDatabaseObjectsInfo;
            }
        }

        /// <summary>
        /// Contiene le informazioni relative ai documenti del modulo
        /// </summary>
        //------------------------------------------------------------------------------
        public IDocumentsObjectInfo DocumentObjectsInfo
        {
            get
            {
                if (documentObjectsInfo != null)
                    return documentObjectsInfo;

                //Oggetto che sa parsare documentObjects.xml
                documentObjectsInfo = new DocumentsObjectInfo(this);

                //path del file documentObjects.xml
                string documentsObjFile = GetDocumentObjectsPath();

                if (PathFinder.FileSystemManager.ExistFile(documentsObjFile))
                    documentObjectsInfo.Parse(documentsObjFile);

                return documentObjectsInfo;
            }
        }

        /// <summary>
        /// Array dei documenti elencati nel moduleObj
        /// </summary>
        //------------------------------------------------------------------------------
        public IList Documents
        {
            get
            {
                if (DocumentObjectsInfo == null)
                    return null;

                return DocumentObjectsInfo.Documents;
            }
        }

        /// <summary>
        /// Parse dei file RowSecurityObjects.xml
        /// </summary>
        //------------------------------------------------------------------------------
        public RowSecurityObjectsInfo RowSecurityObjectsInfo
        {
            get
            {
                if (rowSecurityObjectsInfo == null)
                {
                    // path del file
                    string rowSecurityObjectFile = GetRowSecurityObjectsPath();

                    // Oggetto che sa parsare RowSecurityObjects.xml
                    rowSecurityObjectsInfo = new RowSecurityObjectsInfo(rowSecurityObjectFile, this);

                    //se il file non esiste esco
                    if (!PathFinder.FileSystemManager.ExistFile(rowSecurityObjectFile))
                        return rowSecurityObjectsInfo;

                    rowSecurityObjectsInfo.Parse();
                }

                return rowSecurityObjectsInfo;
            }
            set { rowSecurityObjectsInfo = value; }
        }

        /// <summary>
        /// Parse dei file BehaviourObjects.xml
        /// </summary>
        //------------------------------------------------------------------------------
        public BehaviourObjectsInfo BehaviourObjectsInfo
        {
            get
            {
                if (behaviourObjectsInfo == null)
                {
                    // path del file
                    string behaviourObjectFile = GetBehaviourObjectsPath();

                    // Oggetto che sa parsare BehaviourObjects.xml
                    behaviourObjectsInfo = new BehaviourObjectsInfo(behaviourObjectFile, this);

                    //se il file non esiste esco
                    if (!PathFinder.FileSystemManager.ExistFile(behaviourObjectFile))
                        return behaviourObjectsInfo;

                    behaviourObjectsInfo.Parse();
                }

                return behaviourObjectsInfo;
            }
        }
        //------------------------------------------------------------------------------
        public PathFinder PathFinder
		{
			get
			{ 
				if (parentApplicationInfo == null)
					return null;

				return (PathFinder)parentApplicationInfo.PathFinder;
			}
		}
        //--------------------------------------------------------------
        public IClientDocumentsObjectInfo ClientDocumentsObjectInfo
        {
            get
            {
                if (clientDocumentsObjectInfo != null)
                    return clientDocumentsObjectInfo;

                //path del file documentObjects.xml
                string clientDocumentsObjFile = GetClientDocumentObjectsPath();

                //se il file non esiste esco
                if (!PathFinder.FileSystemManager.ExistFile(clientDocumentsObjFile))
                    return clientDocumentsObjectInfo;

                //Oggetto che sa parsare documentObjects.xml
                clientDocumentsObjectInfo = new ClientDocumentsObjectInfo(clientDocumentsObjFile, this);

                //lo parso
                clientDocumentsObjectInfo.Parse();

                return clientDocumentsObjectInfo;
            }
        }

        //------------------------------------------------------------------------------
        public PathFinder CurrentPathFinder
        {
            get
            {
                if (parentApplicationInfo == null)
                    return null;

                return parentApplicationInfo.PathFinder;
            }
        }

        /// <summary>
        /// Il percorso del file di dizionario
        /// </summary>
        //------------------------------------------------------------------------------
        public string DictionaryFilePath
        {
            get
            {
                return CurrentPathFinder.GetStandardModuleDictionaryFilePath(parentApplicationInfo.Name, Name, Thread.CurrentThread.CurrentUICulture.Name);
            }
        }

        /// <summary>
        /// Path standard del modulo
        /// </summary>
        //------------------------------------------------------------------------------
        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(parentApplicationInfo.Path))
                    return string.Empty;

                return parentApplicationInfo.Path + NameSolverStrings.Directoryseparetor + name;
            }
        }

        /// <summary>
        /// Per parsare i DatabaseObjects.xml
        /// </summary>
        //------------------------------------------------------------------------------
        public DatabaseObjectsInfo DatabaseObjectsInfo
        {
            get
            {
                if (databaseObjectsInfo == null)
                {
                    //path del file DatabaseObjects.xml
                    string databaseObjFile = GetDatabaseObjectsPath();

                    //Oggetto che sa parsare DatabaseObjects.xml
                    databaseObjectsInfo = new DatabaseObjectsInfo(databaseObjFile, this);

                    //se il file non esiste esco
                    if (!PathFinder.FileSystemManager.ExistFile(databaseObjFile))
                        return databaseObjectsInfo;

                    databaseObjectsInfo.Parse();
                }

                return databaseObjectsInfo;
            }
        }

        ///<summary>
        /// Per richiamare il parser dei file .dbxml (nuova gestione struttura dati versione 3.0)
        ///</summary>
        //------------------------------------------------------------------------------
        public IDBObjects DBObjects
        {
            get
            {
                if (dbObjects == null)
                {
                    //path del folder che contiene i file xml che descrivono gli oggetti di database
                    string pathDbObjectsFolder = GetDbxmlCreatePath();

                    if (string.IsNullOrEmpty(pathDbObjectsFolder) || !PathFinder.FileSystemManager.ExistPath(pathDbObjectsFolder))
                        return dbObjects;

                    dbObjects = new DBObjects(this);

                    // istanzio l'oggetto che fa il parse dei file .dbxml
                    DBObjectInfo dbInfo = new DBObjectInfo(this);

                    foreach (TBFile fileName in PathFinder.FileSystemManager.GetFiles(pathDbObjectsFolder, "*.dbxml"))
                    {
                        // per ogni file trovato nella directory richiamo il parse
                        dbInfo.ParseObjectsFromFile(fileName.completeFileName);

                        // dopo il parse riempio le strutture che mi occorrono in questo contesto
                        dbObjects.TableInfoList.AddRange(dbInfo.TableInfoList);
                        dbObjects.ViewInfoList.AddRange(dbInfo.ViewInfoList);
                        dbObjects.ProcedureInfoList.AddRange(dbInfo.ProcedureInfoList);
                        dbObjects.ExtraAddedColsList.AddRange(dbInfo.ExtraAddedColsList);

                        // e pulisco quelle utilizzate altrimenti inserisco le info 2 volte
                        dbInfo.TableInfoList.Clear();
                        dbInfo.ViewInfoList.Clear();
                        dbInfo.ProcedureInfoList.Clear();
                        dbInfo.ExtraAddedColsList.Clear();
                    }
                }

                return dbObjects;
            }
        }

        //------------------------------------------------------------------------------
        public System.Collections.IList WebMethods
		{
			get
			{
				//se sono agganciato al singleton, allora ho i webmethods valorizzati
				if (PathFinder.PathFinderInstance == PathFinder)
					return webMethods;
				//altrimenti li recupero dal singleton
				return PathFinder.PathFinderInstance.GetModuleInfo(NameSpace).WebMethods;
			}
		}
		/// <summary>
		/// Path custom del modulo
		/// </summary>
		//------------------------------------------------------------------------------
		public string CustomPath
		{
			get
			{
				if (
					((ApplicationInfo)parentApplicationInfo).CustomPath == null ||
					((ApplicationInfo)parentApplicationInfo).CustomPath.Length == 0
					)
					return string.Empty;

				return
					((ApplicationInfo)parentApplicationInfo).CustomPath +
					NameSolverStrings.Directoryseparetor +
					Name;
			}
		}

        #endregion

        #region Funzioni pubbliche
        //---------------------------------------------------------------------
        public string GetCustomFontsFullFilename()
        {
            string moduleObjPath = GetCustomModuleObjectPath();

            if (moduleObjPath == string.Empty)
                return string.Empty;

            return moduleObjPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.FontsIniFile;
        }
        //---------------------------------------------------------------------------
        public List<TBDirectoryInfo> XTechDocuments
        {
            get
            {
                string moduleObjectsPath = GetModuleObjectPath();
                if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(moduleObjectsPath))
                    return null;

                return PathFinder.PathFinderInstance.FileSystemManager.GetSubFolders(moduleObjectsPath);
            }
        }

        #region funzioni per la custom dipendenti da company o user
        //-------------------------------------------------------------------------------
        public string GetDataMigrationLogPath()
        {
            string modCusP = GetCustomPath(NameSolverStrings.AllCompanies);
            if (string.IsNullOrEmpty(modCusP))
                return string.Empty;

            return System.IO.Path.Combine(modCusP, NameSolverStrings.MigrationLog);
        }

        //-------------------------------------------------------------------------------
        public string GetCustomPath(string companyName)
        {
            string appCustomPath = ParentApplicationInfo.GetCustomPath(companyName);
            if (string.IsNullOrEmpty(appCustomPath))
                return string.Empty;

            return System.IO.Path.Combine(appCustomPath, Name);
        }
        //-------------------------------------------------------------------------------
        public string GetCustomCompanyReportPath(string companyName)
        {
            if (string.IsNullOrEmpty(companyName))
                return string.Empty;

            string customPath = GetCustomPath(companyName);
            if (string.IsNullOrEmpty(customPath))
                return string.Empty;

            return customPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.Report;
        }

        //-------------------------------------------------------------------------------
        public string GetCustomReportPath(string companyName, string userName)
        {
            if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(userName))
                return string.Empty;

            string customReportPath = GetCustomCompanyReportPath(companyName);
            if (string.IsNullOrEmpty(customReportPath))
                return string.Empty;

            return customReportPath + NameSolverStrings.Directoryseparetor + PathFinder.GetUserPath(userName);
        }
        #endregion



        //-------------------------------------------------------------------------------
        public ArrayList GetConfigFileArray()
        {
            string path = this.GetStandardSettingsPath();
            ArrayList settingFileArray = new ArrayList();
            if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(path))
                return settingFileArray;


            string[] settingsFiles = this.GetSettingsFiles(path);
            string ext = "";

            foreach (string settingFile in settingsFiles)
            {
                ext = settingFile.Substring(settingFile.LastIndexOf("."));
                if (String.Compare(ext, ".config") != 0)
                    continue;
                settingFileArray.Add(settingFile.Substring(settingFile.LastIndexOf(NameSolverStrings.Directoryseparetor) + 1));
            }
            return settingFileArray;
        }

        #region funzioni pubbliche
        //------------------------------------------------------------------------------
        public override string ToString()
        {
            // Restituisco il titolo anzichè il nome del modulo in quanto tale stringa 
            // viene tradotta ed è quindi più significativa del nome
            return this.Title;
        }

        //------------------------------------------------------------------------------
        public string GetActionsInfoFile()
        {
            return System.IO.Path.Combine(this.Path, "Actions.Config");
        }

        /// <summary>
        /// Restituisce la path del dizionario relativo al modulo
        /// </summary>
        /// <returns>la path del dizionario</returns>
        //-------------------------------------------------------------------------------
        public string GetDictionaryPath()
        {
            if (string.IsNullOrEmpty(Path))
                return string.Empty;

            return Path + NameSolverStrings.Directoryseparetor + NameSolverStrings.Dictionary;
        }

        /// <summary>
        /// Restituisce la path della cartella module object del modulo
        /// </summary>
        /// <returns></returns>
        //-------------------------------------------------------------------------------
        public string GetModuleObjectPath()
        {
            if (string.IsNullOrEmpty(Path))
                return string.Empty;

            return Path + NameSolverStrings.Directoryseparetor + NameSolverStrings.ModuleObjects;
        }
        /// <summary>
        /// Restituisce la path della cartella module object del modulo
        /// </summary>
        /// <returns></returns>
        //-------------------------------------------------------------------------------
        public virtual string GetCustomModuleObjectPath()
        {
            if (string.IsNullOrEmpty(Path))
                return string.Empty;

            return GetCustomPath(NameSolverStrings.AllCompanies) + NameSolverStrings.Directoryseparetor + NameSolverStrings.ModuleObjects;
        }
        //-------------------------------------------------------------------------------
        public string GetStandardReportPath()
        {
            if (string.IsNullOrEmpty(Path))
                return string.Empty;

            return Path + NameSolverStrings.Directoryseparetor + NameSolverStrings.Report;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardHelpPath()
        {
            if (string.IsNullOrEmpty(Path))
                return string.Empty;

            return Path + NameSolverStrings.Directoryseparetor + NameSolverStrings.Help;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardDatabaseScriptPath()
        {
            if (string.IsNullOrEmpty(Path))
                return string.Empty;

            return Path + NameSolverStrings.Directoryseparetor + NameSolverStrings.DatabaseScript;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardReportFullFilename(string reportName)
        {
            return System.IO.Path.Combine(GetStandardReportPath(), reportName);
        }

        //---------------------------------------------------------------------
        public string GetFontsFullFilename()
        {
            string moduleObjPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(moduleObjPath))
                return string.Empty;

            return moduleObjPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.FontsIniFile;
        }

        //---------------------------------------------------------------------
        public string GetFormatsFullFilename()
        {
            string moduleObjPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(moduleObjPath))
                return string.Empty;

            return moduleObjPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.FormatsIniFile;
        }

        //---------------------------------------------------------------------
        public string GetEnumsIniPath()
        {
            string moduleObjPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(moduleObjPath))
                return string.Empty;

            return moduleObjPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.EnumsIniFile;
        }

        protected string AllCompaniesCustomPath
        {
            get
            {
                if (allCompaniesCustomPath == null)
                {
                    allCompaniesCustomPath = CurrentPathFinder.GetCustomApplicationPath(NameSolverStrings.AllCompanies, parentApplicationInfo.Name);
                    allCompaniesCustomPath = System.IO.Path.Combine(allCompaniesCustomPath, Name);
                }
                return allCompaniesCustomPath;
            }
        }

        //---------------------------------------------------------------------
        public string GetCustomFormatsFullFilename()
        {
            string moduleObjPath = GetCustomModuleObjectPath();

            if (moduleObjPath == string.Empty)
                return string.Empty;

            return moduleObjPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.FormatsIniFile;
        }

        //-------------------------------------------------------------------------------
        public string GetCustomAllCompaniesAllUsersSettingsFullFilename(string settings)
        {
            string path = GetCustomAllCompaniesAllUsersSettingsPath();
            if (path == null || path == string.Empty)
                return "";
            return System.IO.Path.Combine(path, settings);
        }

        //-------------------------------------------------------------------------------
        public string GetCustomAllCompaniesAllUsersSettingsPath()
        {
            string pathCustom = System.IO.Path.Combine(AllCompaniesCustomPath, NameSolverStrings.Settings);
            return System.IO.Path.Combine(pathCustom, NameSolverStrings.AllUsers);
        }

        //---------------------------------------------------------------------
        public string GetCustomEnumsPath()
        {
            string moduleObjPath = GetCustomModuleObjectPath();

            return GetEnumsPath(moduleObjPath);
        }

        //---------------------------------------------------------------------
        public string GetEnumsPath()
        {
            string moduleObjPath = GetModuleObjectPath();

            return GetEnumsPath(moduleObjPath);
        }

        //---------------------------------------------------------------------
        public string GetEnumsPath(string moduleObjPath)
        {
            if (string.IsNullOrEmpty(moduleObjPath))
                return string.Empty;

            return moduleObjPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.EnumsXml;
        }

        /// <summary>
        /// Restituisce la path del file DocumentObjects.xml
        /// </summary>
        /// <returns>la path richiesta</returns>
        //-------------------------------------------------------------------------------
        public string GetDocumentObjectsPath()
        {
            string docObjPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(docObjPath))
                return string.Empty;

            return docObjPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.DocumentObjectsXml;
        }

        //-------------------------------------------------------------------------------
        public string GetDBInfoPath()
        {
            string dbInfoPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(dbInfoPath))
                return string.Empty;

            return dbInfoPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.DBInfo;
        }

        //-------------------------------------------------------------------------------
        public string GetDocumentPath(string documentName)
        {
            if (string.IsNullOrEmpty(documentName))
                return String.Empty;

            string moduleObjectsPath = GetModuleObjectPath();
            if (string.IsNullOrEmpty(moduleObjectsPath))
                return String.Empty;

            return System.IO.Path.Combine(moduleObjectsPath, documentName);
        }

        ///<summary>
        /// GetDatabaseScriptPath: ritorna il path fino a Applicazione\Modulo\DatabaseScript
        ///</summary>
        //-------------------------------------------------------------------------------
        public string GetDatabaseScriptPath()
        {
            return System.IO.Path.Combine(Path, NameSolverStrings.DatabaseScript);
        }

        ///<summary>
        /// GetProviderCreateScriptPath: 
        /// ritorna il path fino a Applicazione\Modulo\DatabaseScript\Create\nome provider
        ///</summary>
        //-------------------------------------------------------------------------------
        public string GetProviderCreateScriptPath(string provider)
        {
            string dbScriptPath = GetDatabaseScriptPath();
            if (string.IsNullOrEmpty(dbScriptPath))
            {
                Debug.Assert(false);
                return string.Empty;
            }

            // se è stato specificato un provider ritorna quello, altrimenti va nella All
            if (provider == NameSolverDatabaseStrings.SQLOLEDBProvider || provider == NameSolverDatabaseStrings.SQLODBCProvider)
                return dbScriptPath +
                    NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.CreateScript +
                    NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.SqlServer +
                    NameSolverStrings.Directoryseparetor;

            if (provider == NameSolverDatabaseStrings.OraOLEDBProvider ||
                provider == NameSolverDatabaseStrings.MSDAORAProvider)
                return dbScriptPath +
                    NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.CreateScript +
                    NameSolverStrings.Directoryseparetor +
                    NameSolverStrings.Oracle +
                    NameSolverStrings.Directoryseparetor;

            return dbScriptPath +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.CreateScript +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.All +
                NameSolverStrings.Directoryseparetor;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardSettingsPath()
        {
            if (string.IsNullOrEmpty(Path))
                return "";
            return System.IO.Path.Combine(Path, NameSolverStrings.Settings);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardSettingsFullFilename(string settings)
        {
            string path = GetStandardSettingsPath();
            if (string.IsNullOrEmpty(path))
                return "";
            return System.IO.Path.Combine(path, settings);
        }

        //-------------------------------------------------------------------------------
        public string[] GetSettingsFiles(string path)
        {
            string[] files = null;
            int i = 0;
            try
            {
                foreach (TBFile file in PathFinder.FileSystemManager.GetFiles(path, NameSolverStrings.MaskFileConfig))
                {
                    files.SetValue(file.completeFileName, i);
                    i++;
                }

                return files;
            }
            catch (Exception err)
            {
                string a = err.Message;
                return files;
            }
        }

        //-------------------------------------------------------------------------------
        public string GetStandardImagePath()
        {
            //TODOBRUNA
            string fileDir = System.IO.Path.Combine(Path, NameSolverStrings.Files);

            return System.IO.Path.Combine(fileDir, NameSolverStrings.Images);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardImageFullFilename(string image)
        {
            int pos = image.LastIndexOfOccurrence(".", 2, image.Length - 1);
            if (pos >= 0)
            {
                image = image.Substring(0, pos + 1).Replace('.', '\\') + image.Substring(pos + 1);
            }
            return System.IO.Path.Combine(GetStandardImagePath(), image);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardTextPath()
        {
            string fileDir = System.IO.Path.Combine(Path, NameSolverStrings.Files);

            return System.IO.Path.Combine(fileDir, NameSolverStrings.Texts);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardFilePath()
        {
            string fileDir = System.IO.Path.Combine(Path, NameSolverStrings.Files);

            return System.IO.Path.Combine(fileDir, NameSolverStrings.Others);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardTextFullFilename(string text)
        {
            return System.IO.Path.Combine(GetStandardTextPath(), text);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardFileFullFilename(string text)
        {
            return System.IO.Path.Combine(GetStandardFilePath(), text);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardExcelFilesPath()
        {
            return System.IO.Path.Combine(Path, NameSolverStrings.Excel);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardExcelFilesPath(string language)
        {
            string officePath = System.IO.Path.Combine(Path, NameSolverStrings.Excel);
            if (string.IsNullOrEmpty(language))
                return officePath;

            return System.IO.Path.Combine(officePath, language);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardExcelDocumentFullFilename(string document, string language)
        {
            string languageFile = System.IO.Path.Combine(GetStandardExcelFilesPath(language), document);

            string extension = System.IO.Path.GetExtension(languageFile);
            if (string.IsNullOrEmpty(extension))
                languageFile += NameSolverStrings.ExcelDocumentExtension;

            if (PathFinder.PathFinderInstance.FileSystemManager.ExistFile(languageFile))//PathFinder.PathFinderInstance.FileSystemManager.ExistFile(languageFile))
                return languageFile;

            string stdFile = System.IO.Path.Combine(GetStandardExcelFilesPath(), document);

            if (string.IsNullOrEmpty(extension))
                stdFile += NameSolverStrings.ExcelDocumentExtension;

            return stdFile;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardExcelTemplateFullFilename(string template, string language)
        {
            string languageFile = System.IO.Path.Combine(GetStandardExcelFilesPath(language), template);

            string extension = System.IO.Path.GetExtension(languageFile);
            if (string.IsNullOrEmpty(extension))
                languageFile += NameSolverStrings.ExcelTemplateExtension;

            if (PathFinder.PathFinderInstance.FileSystemManager.ExistFile(languageFile))//PathFinder.PathFinderInstance.FileSystemManager.ExistFile(languageFile))
                return languageFile;

            string stdFile = System.IO.Path.Combine(GetStandardExcelFilesPath(), template);

            if (string.IsNullOrEmpty(extension))
                stdFile += NameSolverStrings.ExcelTemplateExtension;

            return stdFile;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardWordFilesPath()
        {
            return System.IO.Path.Combine(Path, NameSolverStrings.Word);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardWordFilesPath(string language)
        {
            string officePath = GetStandardWordFilesPath();
            if (string.IsNullOrEmpty(language))
                return officePath;

            return System.IO.Path.Combine(officePath, language);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardWordDocumentFullFilename(string document, string language)
        {
            string languageFile = System.IO.Path.Combine(GetStandardWordFilesPath(language), document);

            string extension = System.IO.Path.GetExtension(languageFile);
            if (string.IsNullOrEmpty(extension))
                languageFile += NameSolverStrings.WordDocumentExtension;

            if (PathFinder.PathFinderInstance.FileSystemManager.ExistFile(languageFile))//PathFinder.PathFinderInstance.FileSystemManager.ExistFile(languageFile))
                return languageFile;

            string stdFile = System.IO.Path.Combine(GetStandardWordFilesPath(), document);

            if (string.IsNullOrEmpty(extension))
                stdFile += NameSolverStrings.WordDocumentExtension;

            return stdFile;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardWordTemplateFullFilename(string template, string language)
        {
            string languageFile = System.IO.Path.Combine(GetStandardWordFilesPath(language), template);

            string extension = System.IO.Path.GetExtension(languageFile);
            if (string.IsNullOrEmpty(extension))
                languageFile += NameSolverStrings.WordTemplateExtension;

            if (PathFinder.PathFinderInstance.FileSystemManager.ExistFile(languageFile))//PathFinder.PathFinderInstance.FileSystemManager.ExistFile(languageFile))
                return languageFile;

            string stdFile = System.IO.Path.Combine(GetStandardWordFilesPath(), template);

            if (string.IsNullOrEmpty(extension))
                stdFile += NameSolverStrings.WordTemplateExtension;

            return stdFile;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardDocumentSchemaFilesPath(string documentName)
        {
            if (string.IsNullOrEmpty(documentName))
                return String.Empty;

            string documentPath = GetDocumentPath(documentName);
            if (string.IsNullOrEmpty(documentPath))
                return String.Empty;

            return System.IO.Path.Combine(documentPath, NameSolverStrings.ExportProfiles);
        }

        //-------------------------------------------------------------------------------
        public string GetStandardDocumentSchemaFullFilename(string documentName, string schemaName)
        {
            if (string.IsNullOrEmpty(documentName) || string.IsNullOrEmpty(schemaName))
                return String.Empty;

            string documentSchemaPath = GetStandardDocumentSchemaFilesPath(documentName);
            if (string.IsNullOrEmpty(documentSchemaPath))
                return String.Empty;

            string fullFilename = System.IO.Path.Combine(documentSchemaPath, schemaName);

            string extension = System.IO.Path.GetExtension(fullFilename);
            if (string.IsNullOrEmpty(extension))
                fullFilename += NameSolverStrings.SchemaExtension;

            return fullFilename;
        }

        //-------------------------------------------------------------------------------
        public string GetStandardReportSchemaFilesPath()
        {
            return GetStandardReportPath();
        }

        //-------------------------------------------------------------------------------
        public string GetStandardReportSchemaFullFilename(string schemaName)
        {
            if (string.IsNullOrEmpty(schemaName))
                return String.Empty;

            string reportSchemaPath = GetStandardReportSchemaFilesPath();
            if (string.IsNullOrEmpty(reportSchemaPath))
                return String.Empty;

            string fullFilename = System.IO.Path.Combine(reportSchemaPath, schemaName);

            string extension = System.IO.Path.GetExtension(fullFilename);
            if (string.IsNullOrEmpty(extension))
                fullFilename += NameSolverStrings.SchemaExtension;

            return fullFilename;
        }

        /// <summary>
        /// Restituisce il path del file DatabaseObjects.xml
        /// </summary>
        //-------------------------------------------------------------------------------
        public string GetDatabaseObjectsPath()
        {
            string dbPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(dbPath))
                return string.Empty;

            return System.IO.Path.Combine(dbPath, NameSolverStrings.DatabaseObjectsXml);
        }

        /// <summary>
        /// Restituisce il path del file EventHandlerObjects.xml
        /// </summary>
        //-------------------------------------------------------------------------------
        public string GetEventHandlerObjectsPath()
        {
            string ehPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(ehPath))
                return string.Empty;

            return System.IO.Path.Combine(ehPath, NameSolverStrings.EventHandlerObjectsXml);
        }

        /// <summary>
        /// Restituisce il path del file AddOnDatabaseObjects.xml
        /// </summary>
        //-------------------------------------------------------------------------------
        public string GetAddOnDatabaseObjectsPath()
        {
            string path = GetModuleObjectPath();

            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return System.IO.Path.Combine(path, NameSolverStrings.AddOnDatabaseObjectsXml);
        }

        /// <summary>
        /// Restituisce il path del file RowSecurityObjects.xml
        /// </summary>
        //-------------------------------------------------------------------------------
        public string GetRowSecurityObjectsPath()
        {
            string path = GetModuleObjectPath();

            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return System.IO.Path.Combine(path, NameSolverStrings.RowSecurityObjectsXml);
        }

        /// <summary>
        /// Restituisce il path del file BehaviourObjects.xml
        /// </summary>
        //-------------------------------------------------------------------------------
        public string GetBehaviourObjectsPath()
        {
            string path = GetModuleObjectPath();

            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return System.IO.Path.Combine(path, NameSolverStrings.BehaviourObjectsXml);
        }

        /// <summary>
        /// Restituisce il path della directory ReferenceObjects
        /// </summary>
        //-------------------------------------------------------------------------------
        public string GetReferenceObjectsPath()
        {
            string refPath = Path;

            if (string.IsNullOrEmpty(refPath))
                return string.Empty;

            return System.IO.Path.Combine(refPath, NameSolverStrings.ReferenceObjects);
        }

        // ricava il file definizione a partire dal namespace che deve essere quindi conforme
        //-----------------------------------------------------------------------------
        public string GetReferenceObjectFileName(INameSpace ns)
        {
            string fileName = ns.Hotlink + NameSolverStrings.XmlExtension;
            return System.IO.Path.Combine(GetReferenceObjectsPath(), fileName);
        }

        /// <summary>
        /// Restituisce il path del file WebMethods.xml
        /// </summary>
        //-------------------------------------------------------------------------------
        public string GetWebMethodsPath()
        {
            string webPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(webPath))
                return string.Empty;

            return System.IO.Path.Combine(webPath, NameSolverStrings.WebMethodsXml);
        }

        /// <summary>
        /// Restituisce il path del file OutDateObjects.xml
        /// </summary>
        //-------------------------------------------------------------------------------
        public string GetOutDateObjectsPath()
        {
            string outPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(outPath))
                return string.Empty;

            return System.IO.Path.Combine(outPath, NameSolverStrings.OutDateObjectsXml);
        }

        /// <summary>
        /// Rstituisce la cartella relativa al modulo
        /// Dove devono essere gli eseguibili del modulo
        /// es. Debug
        /// </summary>
        /// <param name="build">Tipo di build, es. Debug</param>
        /// <returns>build location</returns>
        //-------------------------------------------------------------------------------
        public string GetClientRelativeOutputPath(string build)
        {
            if (ModuleConfigInfo == null)
                return string.Empty;

            if (string.IsNullOrEmpty(ModuleConfigInfo.DestinationFolder))
                return string.Empty;

            return
                ModuleConfigInfo.DestinationFolder +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Bin +
                NameSolverStrings.Directoryseparetor +
                build;
        }

        //-------------------------------------------------------------------------------
        public string GetServerOutPutPath(string build)
        {
            if (ModuleConfigInfo == null)
                return string.Empty;

            string relPath = ModuleConfigInfo.GetOutPutPath(string.Compare(build, NameSolverStrings.Debug, StringComparison.OrdinalIgnoreCase) == 0);

            if (string.IsNullOrEmpty(relPath))
                return String.Empty;

            return System.IO.Path.Combine(Path, relPath);
        }

        //---------------------------------------------------------------------
        public IList GetLibraryInfosBySourceFolderName(string sourceFolderName)
        {
            ArrayList libraryInfoArray = null;

            foreach (LibraryInfo libraryInfo in this.Libraries)
            {
                if (string.Compare(libraryInfo.Path, sourceFolderName) == 0)
                {
                    if (libraryInfoArray == null)
                        libraryInfoArray = new ArrayList();

                    libraryInfoArray.Add(libraryInfo);
                }
            }

            return libraryInfoArray;
        }

        /// <summary>
        /// Restituisce il documentinfo relativo al nome passato
        /// </summary>
        /// <param name="documentName">Nome del documento da cercare</param>
        /// <returns>Il documentInfo relativo al nome</returns>
        //-------------------------------------------------------------------------------
        public LibraryInfo GetLibraryInfoByName(string libraryName)
        {
            if (libraryName == null || libraryName.Length == 0)
                return null;

            IList libraries = Libraries;

            if (libraries == null)
                return null;

            foreach (LibraryInfo aLibInfo in libraries)
            {
                if (String.Compare(aLibInfo.Name, libraryName) == 0)
                    return aLibInfo;
            }

            return null;
        }

        //-------------------------------------------------------------------------------
        public LibraryInfo GetLibraryInfoByPath(string aLibraryPath)
        {
            if (string.IsNullOrEmpty(aLibraryPath))
                return null;

            IList libraries = Libraries;

            if (libraries == null)
                return null;

            foreach (LibraryInfo aLibInfo in libraries)
            {
                if (String.Compare(aLibInfo.Path, aLibraryPath, StringComparison.OrdinalIgnoreCase) == 0)
                    return aLibInfo;
            }

            return null;
        }

        /// <summary>
        /// Restituisce il FunctionPrototype relativo al nome passato
        /// </summary>
        /// <param name="documentName">Nome del documento da cercare</param>
        /// <returns>Il documentInfo relativo al nome</returns>
        //-------------------------------------------------------------------------------
        public IFunctionPrototype GetFunctionPrototypeByNameSpace(string nameSpace)
        {
            if (string.IsNullOrEmpty(nameSpace))
                return null;

            IList wms = this.WebMethods;
            if (wms == null)
                return null;

            foreach (IFunctionPrototype f in wms)
            {
                if (nameSpace.CompareNoCase(f.FullName))
                    return f;
            }

            return null;
        }

        /// <summary>
        /// Restituisce il documentinfo relativo al nome passato
        /// 		/// </summary>
        /// <param name="documentName">Nome del documento da cercare</param>
        /// <returns>Il documentInfo relativo al nome</returns>
        //-------------------------------------------------------------------------------
        public IDocumentInfo GetDocumentInfoByNameSpace(string nameSpace)
        {
            if (string.IsNullOrEmpty(nameSpace))
                return null;

            IList docs = Documents;

            if (docs == null)
                return null;

            foreach (IDocumentInfo aDocumentInfo in docs)
            {
                if (aDocumentInfo.NameSpace.FullNameSpace.CompareNoCase(nameSpace))
                    return aDocumentInfo;
            }

            return null;
        }

        //-------------------------------------------------------------------------------
        public IDocumentInfo GetDocumentInfoByTitle(string documentTitle)
        {
            IList docs = Documents;

            if (docs == null)
                return null;

            foreach (IDocumentInfo aDocumentInfo in docs)
            {
                if (string.Compare(aDocumentInfo.NameSpace.Application, ParentApplicationName, StringComparison.OrdinalIgnoreCase) == 0 &&
                    string.Compare(aDocumentInfo.NameSpace.Module, name, StringComparison.OrdinalIgnoreCase) == 0 &&
                    string.Compare(aDocumentInfo.Title, documentTitle, StringComparison.OrdinalIgnoreCase) == 0)
                    return aDocumentInfo;
            }

            return null;
        }


        //---------------------------------------------------------------------------
        public string GetClientDocumentObjectsPath()
        {
            string clientPath = GetModuleObjectPath();

            if (string.IsNullOrEmpty(clientPath))
                return string.Empty;

            return clientPath + NameSolverStrings.Directoryseparetor + NameSolverStrings.ClientDocumentObjectsXxml;
        }
        #endregion
        //---------------------------------------------------------------------
        public IFunctionPrototype GetFunctionPrototipeByNameSpace(string nameSpace)
        {
            if (string.IsNullOrEmpty(nameSpace))
                return null;

            IList wms = this.WebMethods;
            if (wms == null || wms.Count == 0)
                return null;

            foreach (FunctionPrototype f in wms)
            {
                if (nameSpace.CompareNoCase(f.FullName))
                    return f;
            }
            return null;
        }


        ///<summary>
        /// GetDbxmlCreatePath: 
        /// ritorna il path fino a Applicazione\Modulo\DatabaseScript\Create, ove sono contenuti
        /// i file dbxml
        ///</summary>
        //-------------------------------------------------------------------------------
        public string GetDbxmlCreatePath()
        {
            return System.IO.Path.Combine(GetDatabaseScriptPath(), NameSolverStrings.CreateScript);
        }
      
		//-------------------------------------------------------------------------------
		public string GetCustomDocumentPath(string documentName)
		{
			if (documentName == null || documentName == String.Empty)
				return String.Empty;

			string moduleObjectsPath = GetCustomModuleObjectPath();
			if (moduleObjectsPath == null || moduleObjectsPath == String.Empty)
				return String.Empty;

			return  System.IO.Path.Combine(moduleObjectsPath, documentName);
		}

		#region funzioni per i report

		//-------------------------------------------------------------------------------
		public string GetCustomReportPath()
		{
			return System.IO.Path.Combine(CustomPath, NameSolverStrings.Report);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportPath(string userName)
		{
			return System.IO.Path.Combine(GetCustomReportPath(), PathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportFullFilename(string report)
		{
			return GetCustomReportFullFilename(report, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomReportFullFilename(string report, string user)
		{
			return System.IO.Path.Combine(GetCustomReportPath(user), report);
		}
		
		#endregion

		#region funzioni per i text

		//-------------------------------------------------------------------------------
		public string GetCustomTextPath()
		{
			//TODOBRUNA
			string fileDir = System.IO.Path.Combine(CustomPath, NameSolverStrings.Files);

			return System.IO.Path.Combine(fileDir, NameSolverStrings.Texts);
		}

        //-------------------------------------------------------------------------------
        public string GetCustomFilePath()
        {
            string fileDir = System.IO.Path.Combine(CustomPath, NameSolverStrings.Files);

            return System.IO.Path.Combine(fileDir, NameSolverStrings.Others);
        }

		//-------------------------------------------------------------------------------
		public string GetCustomTextPath(string userName)
		{		
			return System.IO.Path.Combine(GetCustomTextPath(), PathFinder.GetUserPath(userName));
		}

        //-------------------------------------------------------------------------------
        public string GetCustomFilePath(string userName)
        {
            return System.IO.Path.Combine(GetCustomFilePath(), PathFinder.GetUserPath(userName));
        }

		//-------------------------------------------------------------------------------
		public string GetCustomTextFullFilename(string text)
		{
			return GetCustomTextFullFilename(text, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomTextFullFilename(string text, string user)
		{
			return System.IO.Path.Combine(GetCustomTextPath(user), text);
		}

        //-------------------------------------------------------------------------------
        public string GetCustomFileFullFilename(string text)
        {
            return GetCustomFileFullFilename(text, NameSolverStrings.AllUsers);
        }

        //-------------------------------------------------------------------------------
        public string GetCustomFileFullFilename(string text, string user)
        {
            return System.IO.Path.Combine(GetCustomFilePath(user), text);
        }

		#endregion

		#region funzioni per i file di Excel

		//-------------------------------------------------------------------------------
		public string GetCustomExcelFilesPath()
		{
			return System.IO.Path.Combine(CustomPath, NameSolverStrings.Excel);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelFilesPath(string userName)
		{		
			return System.IO.Path.Combine(GetCustomExcelFilesPath(), PathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelDocumentFullFilename(string document)
		{
			return GetCustomExcelDocumentFullFilename(document, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomExcelDocumentFullFilename(string document, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomExcelFilesPath(user), document);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.ExcelDocumentExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelDocument2007FullFilename(string document, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomExcelFilesPath(user), document);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.Excel2007DocumentExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelTemplateFullFilename(string template)
		{
			return GetCustomExcelTemplateFullFilename(template, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomExcelTemplateFullFilename(string template, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomExcelFilesPath(user), template);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.ExcelTemplateExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelTemplate2007FullFilename(string template, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomExcelFilesPath(user), template);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.Excel2007TemplateExtension;

			return fullFilename;
		}
		
		#endregion
		
		#region funzioni per i file di Word

		//-------------------------------------------------------------------------------
		public string GetCustomWordFilesPath()
		{
			return System.IO.Path.Combine(CustomPath, NameSolverStrings.Word);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordFilesPath(string userName)
		{		
			return System.IO.Path.Combine(GetCustomWordFilesPath(), PathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordDocumentFullFilename(string document)
		{
			return GetCustomWordDocumentFullFilename(document, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomWordDocumentFullFilename(string document, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomWordFilesPath(user), document);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.WordDocumentExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordDocument2007FullFilename(string document, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomWordFilesPath(user), document);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.Word2007DocumentExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordTemplateFullFilename(string template)
		{
			return GetCustomWordTemplateFullFilename(template, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomWordTemplateFullFilename(string template, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomWordFilesPath(user), template);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.WordTemplateExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordTemplate2007FullFilename(string template, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomWordFilesPath(user), template);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.Word2007TemplateExtension;

			return fullFilename;
		}

		#endregion

		#region funzioni per gli schemi di documento

		//-------------------------------------------------------------------------------
		public string GetCustomDocumentSchemaFilesPath(string documentName)
		{
			if (documentName == null || documentName == String.Empty)
				return String.Empty;

			string documentPath = GetCustomDocumentPath(documentName);
			if (documentPath == null || documentPath == String.Empty)
				return String.Empty;

			return System.IO.Path.Combine(documentPath, NameSolverStrings.ExportProfiles);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomDocumentSchemaFilesPath(string documentName, string userName)
		{		
			if (documentName == null || documentName == String.Empty || userName == null || userName == String.Empty)
				return String.Empty;
			//return GetCustomDocumentSchemaFilesPath(documentName);
			return System.IO.Path.Combine(GetCustomDocumentSchemaFilesPath(documentName), PathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomDocumentSchemaFullFilename(string documentName, string schemaName)
		{
			return GetCustomDocumentSchemaFullFilename(documentName, schemaName, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomDocumentSchemaFullFilename(string documentName, string schemaName, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomDocumentSchemaFilesPath(documentName, user), schemaName);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.SchemaExtension;

			return fullFilename;
		}

		#endregion

		#region funzioni per gli schemi di report

		//-------------------------------------------------------------------------------
		public string GetCustomReportSchemaFilesPath()
		{
			return GetCustomReportPath();
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportSchemaFilesPath(string userName)
		{		
			if (userName == null || userName == String.Empty)
				return String.Empty;

			return System.IO.Path.Combine(GetCustomReportSchemaFilesPath(), PathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportSchemaFullFilename(string report)
		{
			return GetCustomReportSchemaFullFilename(report, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomReportSchemaFullFilename(string report, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomReportSchemaFilesPath(user), report);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.SchemaExtension;

			return fullFilename;
		}

        #endregion


      

        //-------------------------------------------------------------------------------
        public string GetCustomAllCompaniesUserSettingsPath()
		{
			if (PathFinder.User == null || 
				PathFinder.User == string.Empty
				)
				return "";

			string pathCustom = System.IO.Path.Combine(AllCompaniesCustomPath, NameSolverStrings.Settings);
			pathCustom = System.IO.Path.Combine(pathCustom, NameSolverStrings.Users);
			return System.IO.Path.Combine(pathCustom, PathFinder.User);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomAllCompaniesUserSettingsFullFilename(string settings)
		{
			string path = GetCustomAllCompaniesUserSettingsPath();
			if (path == null || path == string.Empty)
				return "";

			return System.IO.Path.Combine(path, settings);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomCompanyUserSettingsPath()
		{
			if (CustomPath == null || 
				CustomPath == string.Empty || 
				PathFinder.User == null || 
				PathFinder.User == string.Empty ||
				PathFinder.Company == null || 
				PathFinder.Company == string.Empty
				)
				return "";

			string pathCustom = System.IO.Path.Combine(CustomPath, NameSolverStrings.Settings);
			pathCustom = System.IO.Path.Combine(pathCustom, NameSolverStrings.Users);
			return System.IO.Path.Combine(pathCustom, PathFinder.User);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomCompanyUserSettingsPathFullFilename(string settings)
		{
			string path = GetCustomCompanyUserSettingsPath();
			if (path == null || path == string.Empty)
				return "";
			return System.IO.Path.Combine(path, settings);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomCompanyAllUserSettingsPath()
		{
			if (CustomPath == null || 
				CustomPath == string.Empty || 
				PathFinder.User == null || 
				PathFinder.User == string.Empty ||
				PathFinder.Company == null || 
				PathFinder.Company == string.Empty
				)
				return "";

			string pathCustom = System.IO.Path.Combine(CustomPath, NameSolverStrings.Settings);
			return System.IO.Path.Combine(pathCustom, NameSolverStrings.AllUsers);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomCompanyAllUserSettingsPathFullFilename(string settings)
		{
			string path = GetCustomCompanyAllUserSettingsPath();
			if (path == null || path == string.Empty)
				return "";
			return System.IO.Path.Combine(path, settings);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomImagePath()
		{
			//TODOBRUNA
			string fileDir = System.IO.Path.Combine(CustomPath, NameSolverStrings.Files);

			return System.IO.Path.Combine(fileDir, NameSolverStrings.Images);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomImagePath(string userName)
		{
			return System.IO.Path.Combine(GetCustomImagePath(), PathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomImageFullFilename(string image)
		{
			return GetCustomImageFullFilename(image, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomImageFullFilename(string image, string user)
		{
            int pos = image.LastIndexOfOccurrence(".", 2, image.Length - 1);
            if (pos >= 0)
            {
                image = image.Substring(0, pos + 1).Replace('.', '\\') + image.Substring(pos + 1);
            }
            return System.IO.Path.Combine(GetCustomImagePath(user), image);
		}
		
		//-------------------------------------------------------------------------------

		#endregion
	}
	
	//================================================================================
	public class ApplicationInfo 
	{
        private readonly object instanceLockTicket = new object();
        private string name;
        protected ApplicationType applicationType = ApplicationType.Undefined;
        protected IApplicationConfigInfo applicationConfigInfo;
        protected ArrayList modules;

        private PathFinder pathFinder;
        private string standardAppContainer;

        #region proprietà
        public PathFinder PathFinder { get { return pathFinder; } }

        public string Name { get { return name; } set { name = value; } }

        public ApplicationType ApplicationType { get { return applicationType; } }

        /// <summary>
        /// Informazioni ricavate dal file Application.config
        /// </summary>
        public IApplicationConfigInfo ApplicationConfigInfo { get { return applicationConfigInfo; } }

        /// <summary>
        /// Path standard dell'applicazione
        /// </summary>
        public string Path { get { return System.IO.Path.Combine(standardAppContainer, Name); ; } }

        /// <summary>
        /// Array dei moduli dell'applicazione
        /// </summary>
        //---------------------------------------------------------------------------
        private IList ModulesInternal
        {
            get
            {
                lock (instanceLockTicket)
                {
                    if (modules != null)
                        return modules;

                    modules = new ArrayList();

                    List<string> tempModules = null;
                    tempModules = PathFinder.PathFinderInstance.FileSystemManager.GetAllModuleInfo(Path) ;
                    //prendo tutte le sub dir dell'applicazione
                    
                    foreach (string moduleName in tempModules)
                    {
                        //verifico che siano effettivamente cartelle di moduli
                        if (IsValidModuleFolder(moduleName))
                        {
                            ModuleInfo moduleInfo = CreateModuleInfo(moduleName);
                            modules.Add(moduleInfo);
                        }
                    }

                    Sort();
                    return modules;
                }
            }
        }

        /// <summary>
        /// Array dei moduli dell'applicazione
        /// </summary>
        //---------------------------------------------------------------------------
        public ICollection Modules
        {
            get
            {
                return ModulesInternal;
            }
        }

        //---------------------------------------------------------------------------
        protected bool IsValidModuleFolder(string moduleName)
        {
            return PathFinder.IsModuleDirectory(System.IO.Path.Combine(Path, moduleName));
        }
      		
		//-------------------------------------------------------------------------------
		private		string		customPath = string.Empty;
		public		string		CustomPath { get { return customPath; } }
		#endregion
		
		#region costruttore
		//-------------------------------------------------------------------------------
		public ApplicationInfo(string aName, string standardAppContainer, string customAppContainer, PathFinder aPathFinder)
		{
			if (customAppContainer != null && customAppContainer.Length != 0)
				customPath = customAppContainer + NameSolverStrings.Directoryseparetor + aName;

            name = aName;
            pathFinder = aPathFinder;
            this.standardAppContainer = standardAppContainer;

            string applicationConfigFile = System.IO.Path.Combine(
                Path,
                NameSolverStrings.Application + NameSolverStrings.ConfigExtension
                );

            CreateApplicationConfigInfo(aName, applicationConfigFile);
        }

        //-------------------------------------------------------------------------------
        protected void CreateApplicationConfigInfo(string aName, string applicationConfigFile)
        {
            if (PathFinder.PathFinderInstance.FileSystemManager.ExistFile(applicationConfigFile))
            {
                applicationConfigInfo = new ApplicationConfigInfo(aName, applicationConfigFile);
                applicationConfigInfo.Parse();

                applicationType = GetApplicationType(applicationConfigInfo.Type);
            }

        }
        #endregion


        //-------------------------------------------------------------------------------
        protected ModuleInfo CreateModuleInfo(string moduleName)
		{
            if (string.IsNullOrEmpty(moduleName))
                return null;

            ModuleInfo moduleInfo = new ModuleInfo(moduleName, this);

            return moduleInfo;
        }
		
		/// <summary>
		/// Restituisce il modulo contenuto nell'applicazione con il nome
		/// specificato
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns>il modulo cercato o null</returns>
		//-------------------------------------------------------------------------------
		public ModuleInfo GetModuleInfoByName(string moduleName)
		{
			foreach(ModuleInfo moduleInfo in Modules)
				if (string.Compare(moduleInfo.Name, moduleName, StringComparison.OrdinalIgnoreCase) == 0)
					return moduleInfo;

			return null;
		}

		/// <summary>
		/// Restituisce il modulo contenuto nell'applicazione con il title
		/// specificato
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns>il modulo cercato o null</returns>
		//-------------------------------------------------------------------------------
		public ModuleInfo GetModuleInfoByTitle(string moduleTitle)
		{
			foreach(ModuleInfo moduleInfo in Modules)
				if (string.Compare(moduleInfo.Title, moduleTitle, StringComparison.OrdinalIgnoreCase) == 0)
					return moduleInfo;

			return null;
		}

        #region funzioni pubbliche
        //------------------------------------------------------------------------------
        public void AddDynamicModule(string moduleName)
        {
            lock (instanceLockTicket)
            {
                if (GetModuleInfoByName(moduleName) == null)
                    ModulesInternal.Add(CreateModuleInfo(moduleName));
            }
        }
        //------------------------------------------------------------------------------
        public static bool MatchType(ApplicationType aType, ApplicationType aTargetType)
        {
            if (aTargetType == ApplicationType.All)
                return true;

            return (aType & aTargetType) == aTargetType;
        }

        //------------------------------------------------------------------------------
        public static ApplicationType GetApplicationType(string typeString)
        {
            if (string.Compare(typeString, "Tb", StringComparison.OrdinalIgnoreCase) == 0)
                return ApplicationType.TaskBuilder;
            if (string.Compare(typeString, "TbOtherComponents", StringComparison.OrdinalIgnoreCase) == 0)
                return ApplicationType.TaskBuilder;
            if (string.Compare(typeString, "TbNet", StringComparison.OrdinalIgnoreCase) == 0)
                return ApplicationType.TaskBuilderNet;
            if (string.Compare(typeString, "TbApplication", StringComparison.OrdinalIgnoreCase) == 0)
                return ApplicationType.TaskBuilderApplication;
            if (string.Compare(typeString, "Customization", StringComparison.OrdinalIgnoreCase) == 0)
                return ApplicationType.Customization;

            return ApplicationType.Undefined;
        }

        //------------------------------------------------------------------------------
        public override string ToString()
        {
            return this.Name;
        }

        //---------------------------------------------------------------------------
        public int AddModule(ModuleInfo aModuleInfo)
        {
            lock (instanceLockTicket)
            {
                if (modules == null)
                    modules = new ArrayList();

                return modules.Add(aModuleInfo);
            }
        }

        //-------------------------------------------------------------------------------
        public static string GetApplicationTypeString(ApplicationType appType)
        {
            switch (appType)
            {
                case ApplicationType.TaskBuilder: return NameSolverStrings.TaskBuilder;
                case ApplicationType.TaskBuilderNet: return NameSolverStrings.TaskBuilder;
                case ApplicationType.TaskBuilderApplication: return NameSolverStrings.TaskBuilderApplications;
            }
            return string.Empty;
        }

      

        //-------------------------------------------------------------------------------
        public void RemoveModuleInfo(string moduleName)
        {
            lock (instanceLockTicket)
            {
                for (int i = ModulesInternal.Count - 1; i >= 0; i--)
                {
                    ModuleInfo moduleInfo = ModulesInternal[i] as ModuleInfo;
                    if (string.Compare(moduleInfo.Name, moduleName, StringComparison.OrdinalIgnoreCase) != 0)
                        continue;

                    ModulesInternal.RemoveAt(i);
                    return;
                }
            }
        }

       

        //-------------------------------------------------------------------------------
        public string GetCustomPath(string companyName)
        {
            if (string.IsNullOrEmpty(companyName))
                return string.Empty;

            string customPath = pathFinder.GetCustomPath();
            if (string.IsNullOrEmpty(customPath))
                return string.Empty;

            string appContName = PathFinder.GetApplicationContainerName(ApplicationType);
            if (string.IsNullOrEmpty(appContName))
                return string.Empty;

            return
                customPath +
                NameSolverStrings.Directoryseparetor +
                NameSolverStrings.Companies +
                NameSolverStrings.Directoryseparetor +
                companyName +
                NameSolverStrings.Directoryseparetor +
                appContName +
                NameSolverStrings.Directoryseparetor +
                Name;
        }

        //---------------------------------------------------------------------------
        private void Sort()
        {
            try
            {
                modules.Sort(new ModuleComparer());
            }
            catch (Exception exc)
            {
                Debug.Assert(false, exc.Message);
            }
        }

        //---------------------------------------------------------------------------
        public bool IsKindOf(ApplicationType applicationType)
        {
            return MatchType(applicationType, this.applicationType);
        }
        #endregion

        #region funzioni statiche

        //-----------------------------------------------------------------------------
        public static bool IsValidObjectFileName(string aObjectFileName, string fileExtension)
        {
            if (string.IsNullOrEmpty(aObjectFileName))
                return false;

            aObjectFileName = aObjectFileName.ToLower();

            if (fileExtension != null && fileExtension != String.Empty)
            {
                if (aObjectFileName.EndsWith(fileExtension.ToLower()))
                    aObjectFileName = aObjectFileName.Substring(0, aObjectFileName.Length - fileExtension.Length);
                if (string.IsNullOrEmpty(aObjectFileName))
                    return false;
            }

            for (int k = 0; k < aObjectFileName.Length; k++)
            {
                if (!char.IsLetterOrDigit(aObjectFileName, k) &&
                    aObjectFileName[k] != '-' &&
                    aObjectFileName[k] != '_')
                    return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidObjectFileName(string aObjectFileName)
        {
            return IsValidObjectFileName(aObjectFileName, null);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidReportFileName(string aReportFileName)
        {
            return IsValidObjectFileName(aReportFileName, NameSolverStrings.WrmExtension);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidExcelDocumentFileName(string aFileName)
        {
            return IsValidObjectFileName(aFileName, NameSolverStrings.ExcelDocumentExtension) ||
                   IsValidObjectFileName(aFileName, NameSolverStrings.Excel2007DocumentExtension);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidExcelTemplateFileName(string aFileName)
        {
            return IsValidObjectFileName(aFileName, NameSolverStrings.ExcelTemplateExtension) ||
                   IsValidObjectFileName(aFileName, NameSolverStrings.Excel2007TemplateExtension);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidExcelFileName(string aFileName)
        {
            return IsValidObjectFileName(aFileName, NameSolverStrings.ExcelDocumentExtension) ||
                    IsValidObjectFileName(aFileName, NameSolverStrings.ExcelTemplateExtension) ||
                    IsValidObjectFileName(aFileName, NameSolverStrings.Excel2007DocumentExtension) ||
                    IsValidObjectFileName(aFileName, NameSolverStrings.Excel2007TemplateExtension);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidWordDocumentFileName(string aFileName)
        {
            if (aFileName != null && aFileName != String.Empty && aFileName[0] == '~')
                return false;

            return IsValidObjectFileName(aFileName, NameSolverStrings.WordDocumentExtension) ||
                   IsValidObjectFileName(aFileName, NameSolverStrings.Word2007DocumentExtension);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidWordTemplateFileName(string aFileName)
        {
            if (aFileName != null && aFileName != String.Empty && aFileName[0] == '~')
                return false;

            return IsValidObjectFileName(aFileName, NameSolverStrings.WordTemplateExtension) ||
                   IsValidObjectFileName(aFileName, NameSolverStrings.Word2007TemplateExtension);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidWordFileName(string aFileName)
        {
            return IsValidObjectFileName(aFileName, NameSolverStrings.WordDocumentExtension) ||
                    IsValidObjectFileName(aFileName, NameSolverStrings.WordTemplateExtension) ||
                    IsValidObjectFileName(aFileName, NameSolverStrings.Word2007DocumentExtension) ||
                    IsValidObjectFileName(aFileName, NameSolverStrings.Word2007TemplateExtension);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidOfficeFileName(string aFileName)
        {
            return IsValidExcelDocumentFileName(aFileName) ||
                    IsValidExcelTemplateFileName(aFileName) ||
                    IsValidWordDocumentFileName(aFileName) ||
                    IsValidWordTemplateFileName(aFileName);
        }

        //-----------------------------------------------------------------------------
        public static bool IsValidSchemaFileName(string aFileName)
        {
            return IsValidObjectFileName(aFileName, NameSolverStrings.SchemaExtension);
        }

        //-----------------------------------------------------------------------------
        public List<ClientDocumentInfo> GetClientDocumentsFor(IDocument document)
        {
            List<ClientDocumentInfo> clientDocs = null;

            foreach (ModuleInfo moduleInfo in Modules)
            {
                if (moduleInfo.ClientDocumentsObjectInfo == null)
                    continue;

                IList list = moduleInfo.ClientDocumentsObjectInfo.GetClientDocumentsFor(document);
                if (list == null)
                    continue;

                foreach (var item in list)
                {
                    ClientDocumentInfo ci = item as ClientDocumentInfo;
                    if (ci == null)
                        continue;

                    if (clientDocs == null)
                        clientDocs = new List<ClientDocumentInfo>();

                    clientDocs.Add(ci);
                }
            }

            return clientDocs;
        }

    #endregion
}

    //=========================================================================
    public class LibraryInfo
    {
        #region Data-members
        private ModuleInfo parentModuleInfo = null;
        private IList documents = null;
        private string name = string.Empty;
        private string aggregateName = string.Empty;
        private string path = string.Empty;
        #endregion

        #region Properties
        //---------------------------------------------------------------------
        public string Name { get { return name; } set { name = value; } }
        public string AggregateName { get { return aggregateName; } set { aggregateName = value; } }
        public string Path { get { return path; } set { path = value; } }

        #endregion

        /// <summary>
        /// Il ModuleInfo del modulo di appartenenza.
        /// </summary>
        public ModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }

        public string ParentModuleName { get { return (parentModuleInfo != null) ? parentModuleInfo.Name : string.Empty; } }

        //---------------------------------------------------------------------
        public IList Documents
        {
            get
            {
                if (documents == null)
                {
                    documents = new ArrayList();

                    if (parentModuleInfo.Documents == null)
                        return null;

                    foreach (IDocumentInfo docInfo in parentModuleInfo.Documents)
                        if (string.Compare(docInfo.NameSpace.Library, Path, StringComparison.OrdinalIgnoreCase) == 0)
                            documents.Add(docInfo);
                }

                return documents;
            }
        }

        //---------------------------------------------------------------------
        public LibraryInfo(ModuleInfo moduleInfo)
        {
            parentModuleInfo = moduleInfo;
        }

        //---------------------------------------------------------------------
        public IDocumentInfo GetDocumentInfoByNameSpace(string nameSpace)
        {
            if (string.IsNullOrEmpty(nameSpace))
                return null;

            IList libraryDocuments = this.Documents;
            if (libraryDocuments == null || libraryDocuments.Count == 0)
                return null;

            foreach (IDocumentInfo aDocumentInfo in libraryDocuments)
                if (string.Compare(aDocumentInfo.NameSpace.FullNameSpace, nameSpace, StringComparison.OrdinalIgnoreCase) == 0)
                    return aDocumentInfo;

            return null;
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return this.Name;
        }
    }

    //=========================================================================
    public class ModuleComparer : IComparer
    {
        //--------------------------------------------------------------------------------
        public int Compare(object x, object y)
        {
            ModuleInfo m1 = x as ModuleInfo;
            ModuleInfo m2 = y as ModuleInfo;

            if (m1 == null || m2 == null)
                throw new NullReferenceException();

            return string.Compare(m1.Name, m2.Name);
        }
    }

}