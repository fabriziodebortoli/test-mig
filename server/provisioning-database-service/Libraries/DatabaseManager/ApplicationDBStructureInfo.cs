using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
{
	// delegate per il caricamento delle informazioni delle applicazioni e moduli 
	public delegate void LoadDatabaseInfoEventHandler(object sender, int nCount);

	/// <summary>
	/// Classe che gestisce:
	/// 1. caricamento applicazioni e moduli
	/// 2. relativo caricamento on demand delle tabelle e delle AdditionalColumns
	/// 3. richiamo del parsing del DatabaseObjects.xml e AddOnDatabaseObjects.xml
	/// 4. riempimento delle strutture e creazione di un apposito array
	/// 5. parsing dei file RowSecurityObjects.xml e riempimento lista con le mastertables dichiarate
	/// </summary>
	//============================================================================
	public class ApplicationDBStructureInfo
	{
		# region Variabili private
		private List<AddOnApplicationDBInfo> applicationDBInfoList;
		private PathFinder pathFinder = null;
		private BrandLoader brandLoader = null;

		private KindOfDatabase kindOfDb = KindOfDatabase.Company;

		// per il caricamento anche delle colonne aggiuntive (usato dal DatabaseManager)
		private bool loadAddCol = false;

		// array globale x memorizzare le strutture delle AdditionalColumns
		private List<AdditionalColumnsInfo> addColumnsList = new List<AdditionalColumnsInfo>();
		private AdditionalColumnsInfo addColInfo = null;

		// lista con i nomi delle mastertables dichiarate nei RowSecurityObjects.xml
		private List<string> rsMasterTables = new List<string>();

		// properties
		//---------------------------------------------------------------------
		public List<AddOnApplicationDBInfo> ApplicationDBInfoList { get { return applicationDBInfoList; } }
		public List<string> RSMasterTables { get { return rsMasterTables; } }

		# endregion

		# region Eventi per la gestione della progressbar in fase di caricamento della struttura di database x modulo
		// usato nell'AuditingAdminPlugIn
		public event LoadDatabaseInfoEventHandler LoadDatabaseInfoStarted;
		public event LoadDatabaseInfoEventHandler LoadDatabaseInfoModChanged;
		public event LoadDatabaseInfoEventHandler LoadDatabaseInfoEnded;
		# endregion

		# region Costruttori
		///<summary>
		/// Costruttore utilizzato per i database di sistema e aziendale
		///</summary>
		//---------------------------------------------------------------------
		public ApplicationDBStructureInfo(PathFinder aPathFinder, BrandLoader aBrandLoader)
		{
			pathFinder = aPathFinder;
			brandLoader = aBrandLoader;
		}

		///<summary>
		/// Costruttore dove viene specificato il tipo di database (al momento e' utilizzato per il database
		/// documentale, che ha una sua gestione a parte)
		///</summary>
		//---------------------------------------------------------------------
		public ApplicationDBStructureInfo(PathFinder aPathFinder, BrandLoader aBrandLoader, KindOfDatabase kindOfDb)
			: this(aPathFinder, aBrandLoader)
		{
			this.kindOfDb = kindOfDb;
		}
		# endregion

		# region ReadDatabaseObjectsFiles e overload
		/// <summary>
		/// per leggere i DatabaseObjects.xml e, su richiesta, anche le AdditionalColumns
		/// </summary>
		/// <param name="appList">lista di applicazioni</param>
		/// <param name="loadAddCol">load anche delle AdditionalColumn</param>
		//---------------------------------------------------------------------
		public void ReadDatabaseObjectsFiles(StringCollection appList, bool loadAddCol)
		{
			this.loadAddCol = loadAddCol;
			ReadDatabaseObjectsFiles(appList);
		}

		/// <summary>
		/// legge tutta la struttura applicazione\modulo\tabelle a partire da una lista di applicazioni
		/// </summary>
		/// <param name="appList">lista applicazioni</param>
		//---------------------------------------------------------------------
		private void ReadDatabaseObjectsFiles(StringCollection appList)
		{
			InitApplicationList(appList);

			int nModCount = 1;
			int modulesTotalCount = 0;

			if (LoadDatabaseInfoStarted != null)
			{
				foreach (AddOnApplicationDBInfo appDBInfo in applicationDBInfoList)
				{
					if (appDBInfo.ModuleList != null)
						modulesTotalCount += appDBInfo.ModuleList.Count;
				}

				LoadDatabaseInfoStarted(this, modulesTotalCount);
			}

			foreach (AddOnApplicationDBInfo appDBInfo in applicationDBInfoList)
			{
				foreach (ModuleDBInfo moduleDBInfo in appDBInfo.ModuleList)
					LoadTablesInfo(appDBInfo.ApplicationName, moduleDBInfo);

				if (LoadDatabaseInfoModChanged != null)
					LoadDatabaseInfoModChanged(this, nModCount++);
			}

			if (LoadDatabaseInfoEnded != null)
				LoadDatabaseInfoEnded(this, 0);

			if (loadAddCol)
				AssignAddColumnsInfo();
		}
		# endregion

		# region InitApplicationList (load applicazioni + moduli) e LoadTablesInfo (tabelle + colonne on demand)
		/// <summary>
		/// Inizializzo la lista dell'applicationInfo a partire dalla lista di applicazioni passata come parametro
		/// </summary>
		/// <param name="appList">lista applicazioni</param> 
		//---------------------------------------------------------------------
		public void InitApplicationList(StringCollection appList)
		{
			applicationDBInfoList = new List<AddOnApplicationDBInfo>();
			ApplicationInfo appInfo = null;

			int modulesTotalCount = 0;
			if (LoadDatabaseInfoStarted != null)
			{
				foreach (string appName in appList)
				{
					appInfo = pathFinder.GetApplicationInfoByName(appName);
					if (appInfo.Modules != null)
						modulesTotalCount += appInfo.Modules.Count;
				}

				LoadDatabaseInfoStarted(this, modulesTotalCount);
			}

			AddOnApplicationDBInfo addOnAppDBInfo = null;
			int nModCount = 0;

			foreach (string appName in appList)
			{
                string appMenuTitle = brandLoader.GetApplicationBrandMenuTitle(appName);
                addOnAppDBInfo = new AddOnApplicationDBInfo(appName, string.IsNullOrWhiteSpace(appMenuTitle) ? appName : appMenuTitle);
				appInfo = pathFinder.GetApplicationInfoByName(appName);

				if (appInfo.Modules == null)
					continue;

				foreach (ModuleInfo modInfo in appInfo.Modules)
				{
					ModuleDBInfo modDBInfo = null;

					if (!pathFinder.FileSystemManager.ExistFile(modInfo.GetDatabaseObjectsPath()))
						continue;

					LoadDatabaseInfoModChanged?.Invoke(this, nModCount++);

					switch (kindOfDb)
					{
						case KindOfDatabase.Company:
						case KindOfDatabase.System:
							{
								if (!modInfo.DatabaseObjectsInfo.Dms)
								{
									modDBInfo = new ModuleDBInfo(modInfo.Name);
									modDBInfo.Title = modInfo.Title;
									modDBInfo.ApplicationMember = appName;
									modDBInfo.ApplicationBrand = addOnAppDBInfo.BrandTitle;
								}
								break;
							}
						case KindOfDatabase.Dms:
							{
								// per il database documentale aggiungo i moduli con l'attributo dms="true"
								// e il SOLO modulo TbOledb (sotto TaskBuilder\Framework potrebbero esserci degli altri moduli da skippare)
								if (modInfo.DatabaseObjectsInfo.Dms ||
									string.Compare(modInfo.DatabaseObjectsInfo.Signature, DatabaseLayerConsts.TbOleDbModuleName, StringComparison.OrdinalIgnoreCase) == 0)
								{
									modDBInfo = new ModuleDBInfo(modInfo.Name);
									modDBInfo.Title = modInfo.Title;
									modDBInfo.ApplicationMember = appName;
									modDBInfo.ApplicationBrand = addOnAppDBInfo.BrandTitle;
								}
								break;
							}
					}

					if (modDBInfo != null)
						addOnAppDBInfo.ModuleList.Add(modDBInfo);
				}

				// se l'array dei moduli contiene almeno un elemento allora memorizzo la
				// relativa AddOnApplication
				if (addOnAppDBInfo.ModuleList.Count > 0)
					applicationDBInfoList.Add(addOnAppDBInfo);
			}

			LoadDatabaseInfoEnded?.Invoke(this, 0);
		}

		/// <summary>
		/// Carica le informazioni del modulo passato come argomento. Utile in fase di richiesta di informazioni
		/// ondemand (ad esempio nella costruzione dell'albero nell'auditing)
		/// </summary>
		/// <param name="appName">nome applicazione</param>
		/// <param name="modDBInfo">struttura modulo oggetto di analisi</param>
		//---------------------------------------------------------------------
		public void LoadTablesInfo(string appName, ModuleDBInfo modDBInfo)
		{
			ApplicationInfo appInfo = pathFinder.GetApplicationInfoByName(appName);

			if (appInfo == null || modDBInfo == null)
				return;

			ModuleInfo modInfo = pathFinder.GetModuleInfoByName(appName, modDBInfo.ModuleName);

			// se il file non esiste skippo il modulo (significa che non apporta oggetti di database all'applicazione)
			if (!pathFinder.FileSystemManager.ExistFile(modInfo.GetDatabaseObjectsPath()))
				return;

			DatabaseObjectsInfo databaseObjInfo = modInfo.DatabaseObjectsInfo;
			AddOnDatabaseObjectsInfo addOnDatabaseObjInfo = modInfo.AddOnDatabaseObjectsInfo;

			modDBInfo.PathErrorFile = databaseObjInfo.FilePath;
			modDBInfo.ErrorDescription = databaseObjInfo.ParsingError;
			modDBInfo.Valid = databaseObjInfo.Valid;

			if (databaseObjInfo.Valid)
			{
				// valorizzo i campi relativi alla signature dell'applicazione, modulo e release
				modDBInfo.ApplicationSign	= appInfo.ApplicationConfigInfo.DbSignature;
				modDBInfo.ModuleSign		= databaseObjInfo.Signature;
				modDBInfo.DBRelease			= databaseObjInfo.Release;

				// aggiungo anche i campi per la gestione della PreviousSignature
				modDBInfo.PreviousApplication	= databaseObjInfo.PreviousApplication;
				modDBInfo.PreviousModule		= databaseObjInfo.PreviousModule;

				modDBInfo.CreateUpdateInfoInstance();

				// leggo le info nel file DatabaseObjects.xml controllando la sua validità
				modDBInfo.LoadDatabaseObjectsInfo(databaseObjInfo);

				// se il file AddOnDatabaseObjects.xml non ha errori di sintassi (e il relativo DatabaseObjects.xml
				// risulta valido ovvero esiste e contiene i tag obbligatori) procedo a caricare le info
				if (databaseObjInfo.Valid && addOnDatabaseObjInfo.AdditionalColumns != null)
				{
					addOnDatabaseObjInfo.AppName = appInfo.Name;
					addOnDatabaseObjInfo.ModName = modDBInfo.ModuleName;

					if (loadAddCol)
						LoadAddOnDatabaseObjectsInfo(addOnDatabaseObjInfo, modDBInfo.ApplicationSign, modDBInfo.ModuleSign);
				}
			}
		}
		# endregion

		# region LoadAddOnDatabaseObjectsInfo (parsing) e riempimento struttura
		/// <summary>
		/// funzione che analizza tutte le informazioni di tipo AddOnDatabaseObjectsInfo,
		/// dopo aver parsato il file AddOnDatabaseObjects.xml
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadAddOnDatabaseObjectsInfo
			(
			AddOnDatabaseObjectsInfo addOnDatabaseObjInfo,
			string appSignature,
			string modSignature
			)
		{
			if (addOnDatabaseObjInfo == null)
				return;

			if (addOnDatabaseObjInfo.AdditionalColumns != null)
			{
				foreach (AdditionalColumnTblInfo addCol in addOnDatabaseObjInfo.AdditionalColumns)
				{
					if (addCol.AddColTableInfoArray != null)
					{
						foreach (AlterTableInfo alterTable in addCol.AddColTableInfoArray)
						{
							addColInfo = new AdditionalColumnsInfo();

							// il nome dell'applicazione + modulo mi servono per cercare nel
							// filesystem i file xml
							addColInfo.AppName = addOnDatabaseObjInfo.AppName;
							addColInfo.ModName = addOnDatabaseObjInfo.ModName;

							// le signature servono per gestire le info necessarie al grafo
							addColInfo.AppSignature = appSignature;
							addColInfo.ModSignature = modSignature;

							// dati relativi alla colonna mancante
							addColInfo.TableName = addCol.Name;
							addColInfo.NumRelease = alterTable.Release;
							addColInfo.NumStep = alterTable.Createstep;

							addColumnsList.Add(addColInfo);
						}
					}
				}
			}
		}

		/// <summary>
		/// Funzione per assegnare i valori caricati dal parser dei file AddColumnInfo.xml
		/// alle strutture EntryDBInfo della tabella corrispondente
		/// </summary>
		//---------------------------------------------------------------------
		private void AssignAddColumnsInfo()
		{
			// per ogni AdditionalColumn memorizzata cerco la corrispondente EntryDBInfo e 
			// aggiungo anche i dati delle colonne aggiuntive
			foreach (AdditionalColumnsInfo addCol in addColumnsList)
				foreach (AddOnApplicationDBInfo appDBInfo in applicationDBInfoList)
					foreach (ModuleDBInfo modDBInfo in appDBInfo.ModuleList)
						foreach (EntryDBInfo entryInfo in modDBInfo.TablesList)
						{
							if (string.Compare(entryInfo.Name, addCol.TableName, StringComparison.OrdinalIgnoreCase) == 0)
							{
								entryInfo.AddColumnsList.Add(addCol);
								break;
							}
						}
		}
		# endregion

		# region AddModuleInfoToCatalog (per ogni CatalogEntry le info dell'applicazione/modulo di appartenenza)
		/// <summary>
		/// permette di inserire per ogni catalog entry l'informazione dell'applicazione/modulo di appartenenza			
		/// </summary>
		/// <param name="appList">lista applicazioni</param>
		/// <param name="catalog">CatalogInfo</param>
		//---------------------------------------------------------------------
		public void AddModuleInfoToCatalog(StringCollection appList, ref CatalogInfo catalog)
		{
			ReadDatabaseObjectsFiles(appList);

			foreach (AddOnApplicationDBInfo appDBInfo in applicationDBInfoList)
			{
				foreach (ModuleDBInfo modInfo in appDBInfo.ModuleList)
				{
					// se la tabella non esiste nel database allora non la considero
					foreach (EntryDBInfo table in modInfo.TablesList)
					{
						CatalogEntry entry = catalog.GetTableEntry(table.Name);
						table.Exist = (entry != null);
						if (entry != null)
						{
							entry.Application = appDBInfo.ApplicationName;
							entry.Module = modInfo.ModuleName;
						}
					}
				}
			}
		}
		# endregion

		# region LoadRowSecurityObjects
		///<summary>
		/// Leggo i file di configurazione del RowSecurityLayer e riempio una lista
		/// con i nomi delle mastertable dichiarate in essi
		///</summary>
		//--------------------------------------------------------------------------------
		public void ReadRowSecurityObjects(StringCollection appList)
		{
			foreach (string appName in appList)
			{
				ApplicationInfo appInfo = pathFinder.GetApplicationInfoByName(appName);
				if (appInfo.Modules == null)
					continue;

				foreach (ModuleInfo modInfo in appInfo.Modules)
				{
					if (!pathFinder.FileSystemManager.ExistFile(modInfo.GetRowSecurityObjectsPath()))
						continue;

					foreach (RSEntity entity in modInfo.RowSecurityObjectsInfo.RSEntities)
					{
						if (string.IsNullOrWhiteSpace(entity.MasterTableName) || rsMasterTables.Contains(entity.MasterTableName))
							continue;

						rsMasterTables.Add(entity.MasterTableName);
					}
				}
			}
		}
		# endregion
	}
}