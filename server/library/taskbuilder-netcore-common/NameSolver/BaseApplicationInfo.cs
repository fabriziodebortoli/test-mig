using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;
using static Microarea.Common.Generic.InstallationInfo;

namespace Microarea.Common.NameSolver
{
    #region LibraryInfo class
    //=========================================================================
    public class LibraryInfo : ILibraryInfo
	{
		#region Data-members
		private IBaseModuleInfo	parentModuleInfo	= null;
		private IList			documents			= null;
		private string			name				= string.Empty;
		private string			aggregateName = string.Empty;
		private string			path = string.Empty;
		#endregion

		#region Properties
		//---------------------------------------------------------------------
		public string Name { get { return name; } set { name = value; } }
		public string AggregateName { get { return aggregateName; } set { aggregateName = value; } }
		public string Path { get { return path; } set { path = value; } }

		#endregion

			/// <summary>
		/// Il BaseModuleInfo del modulo di appartenenza.
		/// </summary>
		public IBaseModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }

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

		#endregion

		//---------------------------------------------------------------------
		public LibraryInfo(IBaseModuleInfo moduleInfo)
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

	# region BaseModuleInfo class
	/// <summary>
	/// Incapsula in memoria le informazioni relative ad un modulo in base al file system.
	/// </summary>
	//=========================================================================
	public class BaseModuleInfo : IBaseModuleInfo
	{
		#region Data-members
		private string name;
		private string moduleConfigFile;
        private string allCompaniesCustomPath;

		protected IBaseApplicationInfo		parentApplicationInfo;
		private IDocumentsObjectInfo		documentObjectsInfo;
		private	IModuleConfigInfo			moduleConfigInfo;
        
        protected ArrayList webMethods = new ArrayList();
        public virtual IList WebMethods { 
            get 
            {
                //if (webMethods.Count == 0)
                //{
                //    if (parentApplicationInfo != null && parentApplicationInfo.PathFinder != null)
                //        parentApplicationInfo.PathFinder.LoadFunctions();
                //}
                return webMethods; 
            }
            //set
            //{
            //    webMethods = value as ArrayList;
            //}
        }

        public BaseModuleInfo GetStaticModuleinfo()
        {
            foreach (IBaseApplicationInfo ai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
            {
                foreach (IBaseModuleInfo mi in ai.Modules)
                {
                    if (this.Path.CompareNoCase(mi.Path))
                        return mi as BaseModuleInfo;
                }
            }
            return null;
        }

		private IClientDocumentsObjectInfo 	clientDocumentsObjectInfo;
		private int currentDBRelease = 0;
		private NameSpace moduleNamespace;
		//
		private IDBObjects dbObjects; // classe con la struttura degli oggetti di database apportati dal modulo
		private IDatabaseObjectsInfo databaseObjectsInfo; 
		private IAddOnDatabaseObjectsInfo addOnDatabaseObjectsInfo;
		private RowSecurityObjectsInfo rowSecurityObjectsInfo;
		private BehaviourObjectsInfo behaviourObjectsInfo;
		#endregion

		/// <summary>
		/// Costruttore
		/// </summary>
		//-------------------------------------------------------------------------------
		public BaseModuleInfo(string moduleName, IBaseApplicationInfo aParentApplicationInfo)
		{
			name					= moduleName;
			parentApplicationInfo	= aParentApplicationInfo;
			moduleNamespace = new NameSpace(aParentApplicationInfo.Name + '.' + name, NameSpaceObjectType.Module);
			moduleConfigFile = Path	+ System.IO.Path.DirectorySeparatorChar	+ NameSolverStrings.Module + NameSolverStrings.ConfigExtension;
		}

		#region Properties
		//-------------------------------------------------------------------------------
		/// <summary>
		/// L'applicationInfo della applicazione di appartenenza
		/// </summary>
		public IBaseApplicationInfo ParentApplicationInfo { get { return parentApplicationInfo; } }
		public string ParentApplicationName { get { return (parentApplicationInfo != null) ? parentApplicationInfo.Name : String.Empty; } }
		public int CurrentDBRelease { get { return currentDBRelease; } set { currentDBRelease = value; }}
		
		/// <summary>
		/// Nome del modulo
		/// </summary>
		public string Name { get { return name; } set { name = value; } }
		public string Title { get { return ModuleConfigInfo.Title; } }

		//---------------------------------------------------------------------------
		public IModuleConfigInfo ModuleConfigInfo 
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
		public IList Libraries { get {	return ModuleConfigInfo.Libraries; } }
        //---------------------------------------------------------------------
        public string GetCustomFontsFullFilename()
        {
            string moduleObjPath = GetCustomModuleObjectPath();

            if (moduleObjPath == string.Empty)
                return string.Empty;

            return moduleObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.FontsIniFile;
        }
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

		//--------------------------------------------------------------
		public  IClientDocumentsObjectInfo ClientDocumentsObjectInfo
		{
			get
			{
				if (clientDocumentsObjectInfo != null)
					return clientDocumentsObjectInfo;

				//path del file documentObjects.xml
				string clientDocumentsObjFile = GetClientDocumentObjectsPath();
				
				//se il file non esiste esco
				if (!File.Exists(clientDocumentsObjFile))
					return clientDocumentsObjectInfo;

				//Oggetto che sa parsare documentObjects.xml
				clientDocumentsObjectInfo = new ClientDocumentsObjectInfo(clientDocumentsObjFile, this);
				
				//lo parso
				clientDocumentsObjectInfo.Parse();
				
				return clientDocumentsObjectInfo;
			}
		}
	
		//------------------------------------------------------------------------------
		public IBasePathFinder PathFinder
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
				return BasePathFinder.BasePathFinderInstance.GetStandardModuleDictionaryFilePath(parentApplicationInfo.Name, Name, CultureInfo.CurrentUICulture.Name);
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

				return parentApplicationInfo.Path + System.IO.Path.DirectorySeparatorChar + name;
			}
		}

		/// <summary>
		/// Per parsare i DatabaseObjects.xml
		/// </summary>
		//------------------------------------------------------------------------------
		public IDatabaseObjectsInfo DatabaseObjectsInfo
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
					if (!File.Exists(databaseObjFile))
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

					if (string.IsNullOrEmpty(pathDbObjectsFolder) || !Directory.Exists(pathDbObjectsFolder))
						return dbObjects;

					dbObjects = new DBObjects(this);
					
					// istanzio l'oggetto che fa il parse dei file .dbxml
					DBObjectInfo dbInfo = new DBObjectInfo(this);

					foreach (string fileName in Directory.GetFiles(pathDbObjectsFolder, "*.dbxml"))
					{
						// per ogni file trovato nella directory richiamo il parse
						dbInfo.ParseObjectsFromFile(fileName);

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

		/// <summary>
		/// Per parsare i AddOnDatabaseObjects.xml
		/// </summary>
		//------------------------------------------------------------------------------
		public IAddOnDatabaseObjectsInfo AddOnDatabaseObjectsInfo
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
					if (!File.Exists(addOnDatabaseObjFile))
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
		public  IDocumentsObjectInfo DocumentObjectsInfo
		{
			get
			{
				if (documentObjectsInfo != null)
					return documentObjectsInfo;

				//Oggetto che sa parsare documentObjects.xml
				documentObjectsInfo = new DocumentsObjectInfo(this);
				
				//path del file documentObjects.xml
				string documentsObjFile = GetDocumentObjectsPath();
				
				if (File.Exists(documentsObjFile))
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
					if (!File.Exists(rowSecurityObjectFile))
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
					if (!File.Exists(behaviourObjectFile))
						return behaviourObjectsInfo;

					behaviourObjectsInfo.Parse();
				}

				return behaviourObjectsInfo;
			}
		}
		#endregion

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

			return Path + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.Dictionary;
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

			return Path + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.ModuleObjects;
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

			return GetCustomPath(NameSolverStrings.AllCompanies) + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.ModuleObjects;
		}
		//-------------------------------------------------------------------------------
		public string GetStandardReportPath()
		{
			if (string.IsNullOrEmpty(Path))
				return string.Empty;

			return Path + System.IO.Path.DirectorySeparatorChar	+ NameSolverStrings.Report;
		}

		//-------------------------------------------------------------------------------
		public string GetStandardHelpPath()
		{
			if (string.IsNullOrEmpty(Path))
				return string.Empty;

			return Path + System.IO.Path.DirectorySeparatorChar +  NameSolverStrings.Help;
		}

		//-------------------------------------------------------------------------------
		public string GetStandardDatabaseScriptPath()
		{
			if (string.IsNullOrEmpty(Path))
				return string.Empty;

			return Path + System.IO.Path.DirectorySeparatorChar	+ NameSolverStrings.DatabaseScript;
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

			return moduleObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.FontsIniFile;
		}
		
		//---------------------------------------------------------------------
		public string GetFormatsFullFilename()
		{
			string moduleObjPath = GetModuleObjectPath();

			if (string.IsNullOrEmpty(moduleObjPath))
				return string.Empty;

			return moduleObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.FormatsIniFile;
		}
		
		//---------------------------------------------------------------------
		public string GetEnumsIniPath()
		{
			string moduleObjPath = GetModuleObjectPath();

			if (string.IsNullOrEmpty(moduleObjPath))
				return string.Empty;

			return moduleObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.EnumsIniFile;
		}

        protected string AllCompaniesCustomPath
        {
            get
            {
                if (allCompaniesCustomPath == null)
                {
                    allCompaniesCustomPath = PathFinder.GetCustomApplicationPath(NameSolverStrings.AllCompanies, parentApplicationInfo.Name);
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

            return moduleObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.FormatsIniFile;
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

			return moduleObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.EnumsXml;
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

			return docObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.DocumentObjectsXml;
		}
		
		//-------------------------------------------------------------------------------
		public string GetDBInfoPath()
		{
			string dbInfoPath = GetModuleObjectPath();

			if (string.IsNullOrEmpty(dbInfoPath))
				return string.Empty;

			return dbInfoPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.DBInfo;
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
				return dbScriptPath							+ 
					System.IO.Path.DirectorySeparatorChar	+ 
					NameSolverStrings.CreateScript			+
					System.IO.Path.DirectorySeparatorChar	+ 
					NameSolverStrings.SqlServer				+
					System.IO.Path.DirectorySeparatorChar;

			if (provider == NameSolverDatabaseStrings.OraOLEDBProvider || 
				provider == NameSolverDatabaseStrings.MSDAORAProvider)
				return dbScriptPath							+ 
					System.IO.Path.DirectorySeparatorChar	+ 
					NameSolverStrings.CreateScript			+
					System.IO.Path.DirectorySeparatorChar	+ 
					NameSolverStrings.Oracle				+
					System.IO.Path.DirectorySeparatorChar;

			return dbScriptPath							+ 
				System.IO.Path.DirectorySeparatorChar	+ 
				NameSolverStrings.CreateScript			+
				System.IO.Path.DirectorySeparatorChar	+ 
				NameSolverStrings.All					+
				System.IO.Path.DirectorySeparatorChar;
		}

		//-------------------------------------------------------------------------------
		public string GetStandardSettingsPath()
		{
			if (string.IsNullOrEmpty(Path))
				return "";
			return  System.IO.Path.Combine(Path, NameSolverStrings.Settings);
		}

		//-------------------------------------------------------------------------------
		public string GetStandardSettingsFullFilename(string settings)
		{
			string path = GetStandardSettingsPath();
			if (string.IsNullOrEmpty(path))
				return "";
			return  System.IO.Path.Combine(path, settings);
		}
		
		//-------------------------------------------------------------------------------
		public string[] GetSettingsFiles(string path)
		{
			string [] files = null; 
			try
			{
				return files = Directory.GetFiles(path, NameSolverStrings.MaskFileConfig);
			}
			catch( Exception err)
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

			if (File.Exists(languageFile))
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

			if (File.Exists(languageFile))
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

			if (File.Exists(languageFile))
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

			if (File.Exists(languageFile))
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
				ModuleConfigInfo.DestinationFolder		+ 
				System.IO.Path.DirectorySeparatorChar	+ 
				NameSolverStrings.Bin					+
				System.IO.Path.DirectorySeparatorChar	+ 
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

			foreach(ILibraryInfo libraryInfo in this.Libraries)
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
		public ILibraryInfo GetLibraryInfoByName(string libraryName)
		{
			if (libraryName == null || libraryName.Length == 0)
				return null;

			IList libraries = Libraries;

			if (libraries == null)
				return null;

			foreach (LibraryInfo aLibInfo in libraries)
			{
				if (String.Compare(aLibInfo.Name, libraryName)== 0)
					return aLibInfo;
			}

			return null;
		}

		//-------------------------------------------------------------------------------
		public ILibraryInfo GetLibraryInfoByPath(string aLibraryPath)
		{
			if (string.IsNullOrEmpty(aLibraryPath))
				return null;

			IList libraries = Libraries;

			if (libraries == null)
				return null;

			foreach (ILibraryInfo aLibInfo in libraries)
			{
				if (String.Compare(aLibInfo.Path, aLibraryPath, StringComparison.OrdinalIgnoreCase)== 0)
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

			return clientPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.ClientDocumentObjectsXxml;
		}
		#endregion

		//---------------------------------------------------------------------------
		public string[] XTechDocuments
		{
			get
			{
				string moduleObjectsPath = GetModuleObjectPath();
				if (!Directory.Exists(moduleObjectsPath))
					return new string[0];
				
				return Directory.GetDirectories(moduleObjectsPath);
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

			return customPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.Report;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportPath(string companyName, string userName)
		{
			if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(userName))
				return string.Empty;

			string customReportPath = GetCustomCompanyReportPath(companyName);
			if (string.IsNullOrEmpty(customReportPath))
				return string.Empty;

			return customReportPath + System.IO.Path.DirectorySeparatorChar	+ BasePathFinder.GetUserPath(userName);
		}
		#endregion

		#region IBaseModuleInfo Members


		#endregion

		//-------------------------------------------------------------------------------
		public ArrayList GetConfigFileArray()
		{
			string path = this.GetStandardSettingsPath();
			ArrayList settingFileArray = new ArrayList();
			if (!Directory.Exists(path))
				return settingFileArray;


			string[] settingsFiles = this.GetSettingsFiles(path);
			string ext = "";

			foreach (string settingFile in settingsFiles)
			{
				ext = settingFile.Substring(settingFile.LastIndexOf("."));
				if (String.Compare(ext, ".config") != 0)
					continue;
				settingFileArray.Add(settingFile.Substring(settingFile.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1));
			}
			return settingFileArray;
		}

	}
    
    #endregion

    #region BaseApplicationInfo class
    /// <summary>
    /// Incapsula in memoria le informazioni relative ad un'applicazione.
    /// </summary>
    //=========================================================================
    public class BaseApplicationInfo : IBaseApplicationInfo
	{
		#region Data-Member
		private readonly object instanceLockTicket = new object();
		private	string					name;
		protected ApplicationType		applicationType = ApplicationType.Undefined;
		protected IApplicationConfigInfo applicationConfigInfo;
		protected ArrayList				modules;

		protected IBasePathFinder		pathFinder;
		private string standardAppContainer;
		#endregion
		
		#region proprietà
		public	IBasePathFinder	PathFinder { get { return pathFinder; } }

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

					ArrayList tempModules = null;

					//prendo tutte le sub dir dell'applicazione
					Functions.ReadSubDirectoryList(Path, out tempModules);
					foreach (string moduleName in tempModules)
					{
						//verifico che siano effettivamente cartelle di moduli
						if (IsValidModuleFolder(moduleName))
						{
							IBaseModuleInfo moduleInfo = CreateModuleInfo(moduleName);
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
			return BasePathFinder.BasePathFinderInstance.IsModuleDirectory(System.IO.Path.Combine(Path, moduleName));
		}


		#endregion

		#region costruttori
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aName">Nome dell'applicazione</param>
		/// <param name="aApplicationType">Tipo dell'applicazione</param>
		/// <param name="standardAppContainer">Path del contenitore dell'applicazione standard</param>
		/// <param name="customAppContainer">Path del contenitore dell'applicazione custom</param>
		//-------------------------------------------------------------------------------
		public BaseApplicationInfo(string aName, string standardAppContainer, IBasePathFinder aPathFinder)
		{
			name = aName;
			pathFinder = aPathFinder;
			this.standardAppContainer = standardAppContainer;

			string applicationConfigFile =	System.IO.Path.Combine(
				Path,
				NameSolverStrings.Application + NameSolverStrings.ConfigExtension
				);

			CreateApplicationConfigInfo(aName, applicationConfigFile);
		}

		//-------------------------------------------------------------------------------
		protected void CreateApplicationConfigInfo(string aName, string applicationConfigFile)
		{
			if (File.Exists(applicationConfigFile))
			{
				applicationConfigInfo = new ApplicationConfigInfo(aName, applicationConfigFile);
				applicationConfigInfo.Parse();

				applicationType = GetApplicationType(applicationConfigInfo.Type);
			}
			
		}
		#endregion

		#region funzioni private
		//-------------------------------------------------------------------------------
		protected virtual IBaseModuleInfo CreateModuleInfo(string moduleName)
		{
			if (string.IsNullOrEmpty(moduleName))
				return null;

			IBaseModuleInfo moduleInfo = new BaseModuleInfo(moduleName, this);

			return moduleInfo;
		}
		#endregion 

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
        public static bool IsApplicationOfTypes(BaseApplicationInfo info, ApplicationType types)
        {
            return (info.ApplicationType & types) == info.ApplicationType;
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
			if (string.Compare(typeString, "EasyBuilderApplication", StringComparison.OrdinalIgnoreCase) == 0)
				return ApplicationType.Standardization;
			if (string.Compare(typeString, "StandardModuleWrapper", StringComparison.OrdinalIgnoreCase) == 0)
				return ApplicationType.StandardModuleWrapper;

			return ApplicationType.Undefined;
		}

		//------------------------------------------------------------------------------
		public override string ToString()
		{
			return this.Name;
		}
		
		//---------------------------------------------------------------------------
		public int AddModule(IBaseModuleInfo aModuleInfo)
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
				case ApplicationType.TaskBuilder:				return NameSolverStrings.TaskBuilder;
				case ApplicationType.TaskBuilderNet:			return NameSolverStrings.TaskBuilder;
				case ApplicationType.TaskBuilderApplication:	return NameSolverStrings.TaskBuilderApplications;
			}
			return string.Empty;
		}

		/// <summary>
		/// Restituisce il modulo contenuto nell'applicazione con il nome
		/// specificato
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns>il modulo cercato o null</returns>
		//-------------------------------------------------------------------------------
		public virtual IBaseModuleInfo GetModuleInfoByName(string moduleName)
		{
			foreach(IBaseModuleInfo moduleInfo in Modules)
				if (string.Compare(moduleInfo.Name, moduleName, StringComparison.OrdinalIgnoreCase) == 0)
					return moduleInfo;

			return null;
		}

		//-------------------------------------------------------------------------------
		public void RemoveModuleInfo(string moduleName)
		{
			lock (instanceLockTicket)
			{
				for (int i = ModulesInternal.Count - 1; i >= 0; i--)
				{
					IBaseModuleInfo moduleInfo = ModulesInternal[i] as IBaseModuleInfo;
					if (string.Compare(moduleInfo.Name, moduleName, StringComparison.OrdinalIgnoreCase) != 0)
						continue;

					ModulesInternal.RemoveAt(i);
					return;
				}
			}
		}

		/// <summary>
		/// Restituisce il modulo contenuto nell'applicazione con il title
		/// specificato
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns>il modulo cercato o null</returns>
		//-------------------------------------------------------------------------------
		public virtual IBaseModuleInfo GetModuleInfoByTitle(string moduleTitle)
		{
			foreach (IBaseModuleInfo moduleInfo in Modules)
				if (string.Compare(moduleInfo.Title, moduleTitle, StringComparison.OrdinalIgnoreCase) == 0)
					return moduleInfo;

			return null;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomPath(string companyName)
		{
			if (string.IsNullOrEmpty(companyName))
				return string.Empty;

			string customPath = pathFinder.GetCustomPath();
			if (string.IsNullOrEmpty(customPath))
				return string.Empty;

			string appContName = BasePathFinder.BasePathFinderInstance.GetApplicationContainerName(ApplicationType);
			if (string.IsNullOrEmpty(appContName))
				return string.Empty;

			return 
				customPath								+
				System.IO.Path.DirectorySeparatorChar	+
				NameSolverStrings.Companies				+
				System.IO.Path.DirectorySeparatorChar	+
				companyName								+
				System.IO.Path.DirectorySeparatorChar	+
				appContName								+
				System.IO.Path.DirectorySeparatorChar	+
				Name;
		}

		//---------------------------------------------------------------------------
		private void Sort()
		{
			try
			{
				modules.Sort(new ModuleComparer());
			}
			catch(Exception exc)
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
			return IsValidObjectFileName(aFileName, NameSolverStrings.ExcelDocumentExtension)||
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
			return	IsValidObjectFileName(aFileName, NameSolverStrings.ExcelDocumentExtension) ||
					IsValidObjectFileName(aFileName, NameSolverStrings.ExcelTemplateExtension) ||
					IsValidObjectFileName(aFileName, NameSolverStrings.Excel2007DocumentExtension)||
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
			return	IsValidObjectFileName(aFileName, NameSolverStrings.WordDocumentExtension) ||
					IsValidObjectFileName(aFileName, NameSolverStrings.WordTemplateExtension) ||
					IsValidObjectFileName(aFileName, NameSolverStrings.Word2007DocumentExtension) ||
					IsValidObjectFileName(aFileName, NameSolverStrings.Word2007TemplateExtension);	
		}
		
		//-----------------------------------------------------------------------------
		public static bool IsValidOfficeFileName(string aFileName)
		{
			return 	IsValidExcelDocumentFileName(aFileName) ||
					IsValidExcelTemplateFileName(aFileName)	||
					IsValidWordDocumentFileName(aFileName)	||
					IsValidWordTemplateFileName(aFileName);
		}

		//-----------------------------------------------------------------------------
		public static bool IsValidSchemaFileName(string aFileName)
		{
			return IsValidObjectFileName(aFileName, NameSolverStrings.SchemaExtension);	
		}
		#endregion
	}
	# endregion
	
	//=========================================================================
	public class ModuleComparer : IComparer
	{
		//--------------------------------------------------------------------------------
		public int Compare(object x, object y)
		{
			IBaseModuleInfo m1 = x as IBaseModuleInfo;
			IBaseModuleInfo m2 = y as IBaseModuleInfo;

			if (m1 == null || m2 == null)
				throw new NullReferenceException();

			return string.Compare(m1.Name, m2.Name);
		}
	}
}