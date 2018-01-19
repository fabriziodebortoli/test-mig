using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer 
{
	/// <summary>
	/// Per il collegamento con la Console e per lanciare le funzioni di 
	/// aggiornamento dei database (di sistema e/o aziendale)
	/// </summary>
	//=========================================================================
	public class DatabaseManager
	{
		# region Events e Delegates
		public delegate void ElaborationProgressMessage(object sender, bool ok, string script, string step, string detail, string fullPath, ExtendedInfo ei);
		public event ElaborationProgressMessage	OnElaborationProgressMessage;

		public delegate void ElaborationProgressBar(object sender);
		public event ElaborationProgressBar	OnElaborationProgressBar;

		public delegate void UpdateModuleCounter(object sender);
		public event UpdateModuleCounter OnUpdateModuleCounter;

		public delegate void UpdateMandatoryCols(string table);
		public event UpdateMandatoryCols OnUpdateMandatoryCols;

		public delegate void UpdateMViewCounter(string mView);
		public event UpdateMViewCounter OnUpdateMViewCounter;

		public delegate void InsertMessageInListView();
		public event InsertMessageInListView OnInsertMessageInListView;

		public delegate void AddMessageForMView(DiagnosticType type, string message);
		public event AddMessageForMView OnAddMessageForMView;

		public delegate void MViewsProcessElaborationCompleted();
		public event MViewsProcessElaborationCompleted OnMViewsProcessElaborationCompleted;

		public delegate bool CanMigrateCompanyDatabase();
		public event CanMigrateCompanyDatabase OnCanMigrateCompanyDatabase;

		#region Per la gestione decentrata del ContextInfo... rimpallo il tutto all'ApplicationDBAdmin
		// eventi e delegati che servono nel ContextInfo x richiamare le varie funzionalità offerte dalla classe PlugIns
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string server);
		public event IsUserAuthenticatedFromConsole	OnIsUserAuthenticatedFromConsole;

		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string server, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;

		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string server);
		public event GetUserAuthenticatedPwdFromConsole	OnGetUserAuthenticatedPwdFromConsole;
		# endregion

		# endregion

		# region Variabili private e pubbliche
		private CheckDBStructureInfo		checkDbStructInfo	= null; // x il check della struttura del db
		private ApplicationDBStructureInfo	appStructInfo		= null; // x fare la load dei file xml di descr. database
		private ExecuteManager				executeManager		= null; // esecuzione vera e propria degli script
		private BrandLoader					brandLoader			= null; // info del branding dalla Console
		private DmsStructureInfo			dmsStructureInfo	= null; // x il check della struttura del db documentale

		// tipo di database che sto analizzando 
		public KindOfDatabase KindOfDb = KindOfDatabase.Company;

		// così da fuori possiamo sapere se è andato tutto a buon fine
		public bool Valid = true; 
		
		public Diagnostic DBManagerDiagnostic = null;

		// per richiamare la classe delle funzioni e datamember comuni
		private ContextInfo contextInfo = null;

		// lista delle applicationi da considerare (con tutte le strutture riempite)
		private List<AddOnApplicationDBInfo> addOnApplicationList = null;
		
		// per il caricamento dei dati di default/esempio
		public BaseImportExportManager ImpExpManager;
		public bool ImportDefaultData = true;	// flag visualizzato nella DBForm
		public bool ImportSampleData = false;	// utilizzato per il caricamento dei dati di esempio contestuale alla creazione db
												// (per ora solo in creazione azienda demo)

		public bool CreateMViewForOracle = false; // flag visualizzato nella DBForm per la creazione viste materializzate (solo per Oracle)
		# endregion

		# region Properties
		//--------------------------------------------------------------------------------
		public ContextInfo ContextInfo { get { return contextInfo; } }
		//--------------------------------------------------------------------------------
		public DatabaseStatus StatusDB { get { return checkDbStructInfo.DBStatus; } }
		//--------------------------------------------------------------------------------
		public bool ErrorInRunSqlScript { get { return executeManager.ErrorInRunSqlScript; } }
		//--------------------------------------------------------------------------------
		public Diagnostic ScriptMngDiagnostic { get { return executeManager.ScriptMngDiagnostic; } }
		//--------------------------------------------------------------------------------
		public List<AddOnApplicationDBInfo> AddOnAppList { get { return addOnApplicationList; } }
		//--------------------------------------------------------------------------------
		public DmsStructureInfo DmsStructureInfo { get { return dmsStructureInfo; } }
		# endregion

		# region Costruttori
		/// <summary>
		/// costruttore per la creazione del database di sistema
		/// </summary>
		//--------------------------------------------------------------------------------
		public DatabaseManager
			(
			PathFinder	pathFinder, 
			string		sysDbConnectionString, 
			DBMSType	dbmsType,
			bool		useUnicode,
			Diagnostic	dbAdminDiagnostic,
			BrandLoader	brandLoader,
			DBNetworkType dbNetworkType,
			string		isoState
			)
		{
			// non ho bisogno di comporre la stringa, me la passa direttamente il SysAdmin
			// dopo che ha creato "fisicamente" il database
            CreateContextInfo(pathFinder, dbNetworkType, isoState, true);
			dbAdminDiagnostic.Clear();
			DBManagerDiagnostic = dbAdminDiagnostic;

			this.brandLoader = brandLoader;

			// provo ad aprire la connessione al db di sistema
			Valid = contextInfo.OpenSysDBConnectionFromString(sysDbConnectionString, dbmsType);

			if (Valid)
			{
				KindOfDb = KindOfDatabase.System;
				contextInfo.UseUnicode = useUnicode;
			}
			else
				dbAdminDiagnostic.Set(contextInfo.Diagnostic);
		}

		/// <summary>
		/// costruttore per la creazione del database aziendale
		/// </summary>
		//---------------------------------------------------------------------------
		public DatabaseManager
			(
			PathFinder	pathFinder, 
			Diagnostic	dbAdminDiagnostic, 
			BrandLoader brandLoader,
			ContextInfo.SystemDBConnectionInfo contextParameters,
			DBNetworkType dbNetworkType,
			string		isoState,
			bool		askCredential
			)
		{
            CreateContextInfo(pathFinder, dbNetworkType, isoState, askCredential);
			contextInfo.SysDBConnectionInfo = contextParameters;
			dbAdminDiagnostic.Clear();
			
			DBManagerDiagnostic = dbAdminDiagnostic;
			this.brandLoader	= brandLoader;
		}

		/// <summary>
		/// costruttore a cui viene passato un ContextInfo già esistente
		/// da utilizzare SOLO per l'aggiornamento del database aziendale dal TestManagerAdminPlugIn
		/// (sul quale è stato ripristinato un file di backup)
		/// </summary>
		//---------------------------------------------------------------------------
		public DatabaseManager
			(
			ContextInfo myContext, 
			Diagnostic dbAdminDiagnostic, 
			BrandLoader brandLoader, 
			ContextInfo.SystemDBConnectionInfo contextParameters,
			DBNetworkType dbNetworkType, 
			string isoState
			)
		{
			contextInfo	= myContext;
			contextInfo.DBNetworkType = dbNetworkType;
			contextInfo.IsoState = isoState;
			contextInfo.SysDBConnectionInfo = contextParameters;

			dbAdminDiagnostic.Clear();

			DBManagerDiagnostic = dbAdminDiagnostic;
			this.brandLoader	= brandLoader;
		}

		/// <summary>
		/// inizializzazione del ContextInfo
		/// </summary>
		//---------------------------------------------------------------------------
        private void CreateContextInfo(PathFinder pathFinder, DBNetworkType dbNetworkType, string isoState, bool askCredential)
		{
            contextInfo = new ContextInfo(pathFinder, dbNetworkType, isoState, askCredential);
			contextInfo.OnAddUserAuthenticatedFromConsole		+= new ContextInfo.AddUserAuthenticatedFromConsole(AddUserAuthenticated);
			contextInfo.OnGetUserAuthenticatedPwdFromConsole	+= new ContextInfo.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwd);
			contextInfo.OnIsUserAuthenticatedFromConsole		+= new ContextInfo.IsUserAuthenticatedFromConsole(IsUserAuthenticated);
		}
		# endregion

		# region Funzioni di caricamento informazioni (struttura applicazione e DatabaseObjects.xml)
		/// <summary>
		/// leggo tutte le AddOnApplication presenti nell'installazione, utilizzando
		/// il PathFinder passatomi dalla Console (a seconda che runni lato server o client).
		/// </summary>
		//---------------------------------------------------------------------------
		private StringCollection LoadApplicationDBStructureList()
		{		
			StringCollection applicationsList = null;

			switch (KindOfDb)
			{
				// se sto analizzando il db aziendale o documentale vado a leggere le application di
				// TaskBuilder e di TaskBuilderApplications
				case KindOfDatabase.Company:
				case KindOfDatabase.Dms:
					{
						// array di supporto per avere l'elenco totale delle AddOnApplications
						// (finchè non cambia il pathfinder e vengono unificati gli ApplicationType)
						StringCollection supportList = new StringCollection();
						applicationsList = new StringCollection();

						// prima guardo le AddOn di TaskBuilder
						contextInfo.PathFinder.GetApplicationsList(ApplicationType.TaskBuilder, out supportList);
						applicationsList = supportList;

						// poi guardo le AddOn di TaskBuilderApplications
						contextInfo.PathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);
						for (int i = 0; i < supportList.Count; i++)
							applicationsList.Add(supportList[i]);

						// infine guardo le customizzazioni realizzate con EasyStudio
						contextInfo.PathFinder.GetApplicationsList(ApplicationType.Customization, out supportList);
						for (int i = 0; i < supportList.Count; i++)
							applicationsList.Add(supportList[i]);

						break;
					}

				// in caso di db di sistema leggo quelle di tipo TaskBuilder.Net
				case KindOfDatabase.System:
					{
						applicationsList = new StringCollection();
						// guardo le AddOn di TaskBuilder.Net
						contextInfo.PathFinder.GetApplicationsList(ApplicationType.TaskBuilderNet, out applicationsList);

						break;
					}
			}
			
			return applicationsList;
		}
		# endregion	

		# region Varie MakeCompanyConnection & CloseConnection
		//---------------------------------------------------------------------------
		/// <summary>
		/// raggruppa le funzioni da effettuare per creare una valida connessione al db
		/// aziendale (dalla query al SystemDB, alla composizione della stringa di connessione
		/// al db aziendale, all'apertura della connessione stessa)
		/// </summary>
		/// <param name="companyIdNode">companyId</param>
		/// <returns>se la connessione è stata aperta con successo</returns>
		//---------------------------------------------------------------------------
		private bool MakeCompanyConnection(string companyIdNode, bool isDMSActivated, bool isRowSecurityActivated)
		{
			if (!contextInfo.MakeCompanyConnection(companyIdNode, isDMSActivated, isRowSecurityActivated))
			{
				if (contextInfo.Diagnostic.Error)
					DBManagerDiagnostic.Set(contextInfo.Diagnostic);
				return false;
			}

			DBManagerDiagnostic.Set(DiagnosticType.LogOnFile, "****************************************************************************************");
			DBManagerDiagnostic.Set(DiagnosticType.LogOnFile, "--------------------------------------------");
			DBManagerDiagnostic.Set(DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.CheckingCompany, contextInfo.CompanyName));
			DBManagerDiagnostic.Set(DiagnosticType.LogOnFile, "--------------------------------------------");

			return true;
		}

		/// <summary>
		/// richiamata dal plugIn dopo aver caricato le configurazioni dei dati di default nella DBForm
		/// (a quel livello serve una connessione aperta!)
		/// </summary>
		//---------------------------------------------------------------------------
		public void CloseConnection()
		{
			// chiudo la connessione aperta sul database 
			contextInfo.CloseConnection();
			contextInfo.UndoImpersonification();
		}
		# endregion

		# region Set dello stato del database (con o senza connessione)
		/// <summary>
		/// Effettua una connessione al SOLO db aziendale
		/// e poi richiama la CheckDBStructure, per controllare la situazione del database
		/// (utilizzata dall'Import/Export dati, RegressionTest, QuickStart)
		/// Ignora il database di EA e la struttura delle MasterTable per il RowSecurityLayer
		/// </summary>
		/// <param name="companyId">id della company da controllare</param>
		//---------------------------------------------------------------------------
		public bool ConnectAndCheckDBStructure(string companyId)
		{
			return ConnectAndCheckDBStructure(companyId, false, false);
		}

		/// <summary>
		/// Effettua una connessione al db aziendale / documentale
		/// e poi richiama la CheckDBStructure, per controllare la situazione del database
		/// </summary>
		/// <param name="companyId">id della company da controllare</param>
		/// <param name="checkSlaves">true: controlla anche il database documentale eventualmente agganciato</param>
		/// <param name="checkRSMasterTables">true: controlla la struttura delle MasterTable per il RowSecurityLayer</param>
		//---------------------------------------------------------------------------
		public bool ConnectAndCheckDBStructure(string companyId, bool isDMSActivated, bool isRowSecurityActivated)
		{
			bool result = false;
			if (MakeCompanyConnection(companyId, isDMSActivated, isRowSecurityActivated))
			{
				result = CheckDBStructure(KindOfDatabase.Company);

				if (
					((StatusDB & DatabaseStatus.PRE_40) == DatabaseStatus.PRE_40) &&
					!CheckDBStructureInfo_CanMigrateCompanyDatabase()
					)
					return result;

				// procedo con i controlli sullo slave solo se richiesto espressamente
				// (serve solo per la creazione / aggiornamento database)
				if (isDMSActivated)
				{
					if (result && contextInfo.HasSlaves)
						result = CheckDBStructure(KindOfDatabase.Dms);
				}
			}

			return result;
		}

		/// <summary>
		/// ConnectAndCheckDBStructureForTestPlan
		/// Apre subito la connessione al db aziendale (xchè il ContextInfo ha già le informazioni
		/// caricate) e poi richiama il CheckDBStructure
		/// </summary>
		//---------------------------------------------------------------------------
		public bool ConnectAndCheckDBStructureForTestPlan()
		{
			try
			{
				// apro direttamente la connessione al db aziendale
				// in questo caso il ContextInfo è già stato valorizzato e non devo fare altre operazioni
				if (this.contextInfo.Connection == null)
					this.contextInfo.Connection = new TBConnection(this.contextInfo.ConnectAzDB, DBMSType.SQLSERVER);
				else
					this.contextInfo.Connection.ConnectionString = this.contextInfo.ConnectAzDB;

				this.contextInfo.Connection.Open();
			}
			catch (TBException e)
			{
				contextInfo.CloseConnection();
				contextInfo.SqlErrorCode = e.Number;
				contextInfo.SqlMessage = e.Message;
				Debug.WriteLine(e.Message, "DatabaseManager.cs - ConnectAndCheckDBStructureForTestPlan");
				return false;
			}

			return CheckDBStructure(KindOfDatabase.Company);
		}

		/// <summary>
		/// Funzione con i vari controlli al database aziendale, per decidere
		/// poi quali operazioni devo effettuare (Create o Upgrade)
		/// </summary>
		/// <returns>il successo dell'operazione</returns>
		//---------------------------------------------------------------------------
		public bool CheckDBStructure(KindOfDatabase kindOfDatabase)
		{
			if (OnElaborationProgressBar != null)
				OnElaborationProgressBar(this);

			KindOfDb = kindOfDatabase;

			if (KindOfDb != KindOfDatabase.System && ImpExpManager == null) // anche se i dati di default NON ci sono per il db documentale
				ImpExpManager = new BaseImportExportManager(contextInfo, brandLoader);

			// leggo la directory di installazione e individuo l'elenco delle applicazioni
			// che hanno dei moduli che dichiarano oggetti di database
			StringCollection applicationsList = LoadApplicationDBStructureList();
			if (applicationsList == null || applicationsList.Count == 0)
			{
				DiagnosticViewer.ShowErrorTrace(DatabaseLayerStrings.ErrReadAppInfo, string.Empty, DatabaseLayerStrings.LblAttention);
				return false;
			}
			
			// dopo aver caricato le applicazioni e relativi moduli, procedo ad analizzare il loro contenuto
			// e stabilire in che stato si trova il database
			AnalyzeDBStructure(applicationsList);
			
			// NON POSSO CHIUDERE QUI LA CONNESSIONE, XCHE' SERVE ANCORA X LA DBFORM PER IL CARICAMENTO DELLE CONFIGURAZIONI
			// DEL DEFAULT! LA CHIUDO DOPO A LIVELLO DI PLUGIN.

			// controllo la congruenza dei parametri dell'azienda e quelli impostati effettivamente sul db
			// solo se si tratta del database aziendale ed è in uno stato diverso da empty
			if (KindOfDb == KindOfDatabase.Company && checkDbStructInfo.DBStatus != DatabaseStatus.EMPTY)
                contextInfo.CheckCompanyParameterSetting(); //@@TODO Michi: cosa fare per  Easy Attachment?

			return true;
		}

		///<summary>
		/// Metodo che si occupa di analizzare la struttura del database (sistema-aziendale-DMS)
		/// 1. Legge i file di configurazione delle applicazioni
		/// 2. Esegue la Load del Catalog del database correntemente connesso
		/// 2. Esegue un match tra la struttura fisica e la struttura "attesa" caricata in memoria e stabilisce
		/// lo stato in cui si trova il database
		///</summary>
		//---------------------------------------------------------------------------
		private void AnalyzeDBStructure(StringCollection applicationsList)
		{
			switch (KindOfDb)
			{
				case KindOfDatabase.Company:
				case KindOfDatabase.System:
					{
						/* istanzio la classe che:
						* 1. riempie le strutture con le applicazioni e i moduli che ci servono  
						* 2. fa il parsing dei relativi file DatabaseObjects.xml e AddOnDatabaseObjects.xml (caricati dal PathFinder)
						* 3. ritorna un array di applicazioni */
						appStructInfo = new ApplicationDBStructureInfo(contextInfo.PathFinder, brandLoader);
						appStructInfo.ReadDatabaseObjectsFiles(applicationsList, true); // N.B.: il 2° param è x caricare anche le AddColumns
						addOnApplicationList = appStructInfo.ApplicationDBInfoList;

						// se l'azienda usa il RowSecurity ed e' attivato carico anche le mastertable
						if (contextInfo.IsRowSecurityActivated && contextInfo.UseRowSecurity)
							appStructInfo.ReadRowSecurityObjects(applicationsList);

						// richiamo la classe che si occupa di stabilire lo stato del database
						checkDbStructInfo = new CheckDBStructureInfo(KindOfDb, contextInfo, appStructInfo, ref DBManagerDiagnostic);
						checkDbStructInfo.OnAddDefaultDataMissingTable += new CheckDBStructureInfo.AddDefaultDataMissingTable(OnAddDefaultDataMissingTable);
						checkDbStructInfo.OnAddSampleDataMissingTable += new CheckDBStructureInfo.AddSampleDataMissingTable(OnAddSampleDataMissingTable);
						checkDbStructInfo.OnCanMigrateCompanyDatabase += new CheckDBStructureInfo.CanMigrateCompanyDatabase(CheckDBStructureInfo_CanMigrateCompanyDatabase);
						checkDbStructInfo.GetDatabaseStatus();
						break;
					}

				case KindOfDatabase.Dms:
					{
						// per il database documentale utilizzo una classe ad-hoc
						// che si riempie le sue strutture dati in memoria
						dmsStructureInfo = new DmsStructureInfo(contextInfo, applicationsList, brandLoader, DBManagerDiagnostic, ImpExpManager);
						dmsStructureInfo.Load();
						break;
					}
			}
		}
		# endregion

		# region Gestione database e aggancio all'ExecuteManager
		/// <summary>
		/// Entry-point per la gestione del database
		/// (sulla base del suo stato lancio le procedure necessarie - creazione, upgrade, ripristino, etc.)
		/// </summary>
		//---------------------------------------------------------------------------
		public bool DatabaseManagement()
		{
			return DatabaseManagement(true);
		}

		/// <summary>
		/// Entry-point per la gestione del database
		/// (sulla base del suo stato lancio le procedure necessarie - creazione, upgrade, ripristino, etc.)
		/// </summary>
		//---------------------------------------------------------------------------
		public bool DatabaseManagement(bool isSilentMode)
		{
			executeManager = new ExecuteManager(checkDbStructInfo, ImpExpManager, ImportDefaultData, ImportSampleData, CreateMViewForOracle);
			executeManager.OnElaborationProgressBar		+= new ExecuteManager.ElaborationProgressBar(ExecuteManager_OnElaborationProgressBar);
			executeManager.OnElaborationProgressMessage	+= new ExecuteManager.ElaborationProgressMessage(ExecuteManager_OnElaborationProgressMessage);
			executeManager.OnUpdateModuleCounter		+= new ExecuteManager.UpdateModuleCounter(ExecuteManager_OnUpdateModuleCounter);
			executeManager.OnUpdateMandatoryCols		+= new ExecuteManager.UpdateMandatoryCols(ExecuteManager_OnUpdateMandatoryCols);
			executeManager.OnInsertMessageInListView	+= new ExecuteManager.InsertMessageInListView(ExecuteManager_OnInsertMessageInListView);
			executeManager.OnUpdateMViewCounter			+= new ExecuteManager.UpdateMViewCounter(ExecuteManager_OnUpdateMViewCounter);
			
			bool success = executeManager.Execute(DBManagerDiagnostic);

			// se l'azienda ha anche uno slave allora procedo ad aggiornare anche il database documentale
			if (contextInfo.HasSlaves)
			{
				executeManager.DbStructInfo = dmsStructureInfo.DmsCheckDbStructInfo;
				success = executeManager.Execute(DBManagerDiagnostic);
			}

			// il file di log viene creato per l'aggiornamento del database aziendale e documentale
			// se viene eseguito in maniera non silente
			if (KindOfDb != KindOfDatabase.System && !isSilentMode)
				CreateLogFile();

			return success;
		}
		# endregion

		# region Metodi richiamati esternamente per la creazione delle strutture delle viste materializzate per Oracle

		// variabile locale per la sola gestione delle viste materializzate di Oracle
		private ExecuteManager mViewExecuteManager = null;

		///<summary>
		/// Metodo per eseguire il check della situazione dei privilegi e delle viste materializzate e
		/// visualizzare i relativi messaggi all'utente
		/// Richiamato dalla form MatViews
		///</summary>
		//---------------------------------------------------------------------------
		public void CheckOracleMatViewsStatus(out bool userHasPrivileges, out List<string> mViewsList)
		{
			mViewExecuteManager = new ExecuteManager(checkDbStructInfo, ImpExpManager, false, false, true);
			mViewExecuteManager.OnAddMessageForMView += new ExecuteManager.AddMessageForMView(mViewExecuteManager_OnAddMessageForMView);
			mViewExecuteManager.OnMViewsProcessElaborationCompleted += new ExecuteManager.MViewsProcessElaborationCompleted(mViewExecuteManager_OnMViewsProcessElaborationCompleted);
			mViewExecuteManager.CheckMatViewsStatus(out userHasPrivileges, out mViewsList);
		}

		//-----------------------------------------------------------------------
		public bool ManageOracleMatViews(bool onlyDrop , bool userHasPrivileges, List<string> mViewsList)
		{
			if (mViewExecuteManager != null)
				return mViewExecuteManager.ManageOracleMatViews(onlyDrop, userHasPrivileges, mViewsList, DBManagerDiagnostic);

			return false;
		}

		///<summary>
		/// Evento richiamato per visualizzare un testo nella form di creazione delle viste materializzate
		///</summary>
		//---------------------------------------------------------------------------
		private void mViewExecuteManager_OnAddMessageForMView(DiagnosticType type, string message)
		{
			if (OnAddMessageForMView != null)
				OnAddMessageForMView(type, message);
		}

		///<summary>
		/// Evento richiamato per segnalare che la procedura di creazione delle viste materializzate e' completata
		///</summary>
		//-----------------------------------------------------------------------
		private void mViewExecuteManager_OnMViewsProcessElaborationCompleted()
		{
			if (OnMViewsProcessElaborationCompleted != null)
				OnMViewsProcessElaborationCompleted();
		}
		# endregion

		# region Evento per la gestione dell'ImportExport dal CheckDBStructureInfo (x le tabelle mancanti)
		/// <summary>
		/// evento intercettato dalla classe CheckDBStructureInfO quando vengono controllate le eventuali
		/// tabelle mancanti al database. Per ognuna di queste viene caricato anche il corrispondente file
		/// (se esiste) contenente i dati di default
		/// </summary>
		//---------------------------------------------------------------------
		private void OnAddDefaultDataMissingTable(string table, string application, string module)
		{
			if (ImpExpManager != null)
				ImpExpManager.AddDefaultDataTable(table, application, module);
		}
		/// <summary>
		/// evento intercettato dalla classe CheckDBStructureInfO quando vengono controllate le eventuali
		/// tabelle mancanti al database. Per ognuna di queste viene caricato anche il corrispondente file
		/// (se esiste) contenente i dati di esempio
		/// </summary>
		//---------------------------------------------------------------------
		private void OnAddSampleDataMissingTable(string table, string application, string module)
		{
			if (ImpExpManager != null)
				ImpExpManager.AddSampleDataTable(table, application, module);
		}
		#endregion

		//---------------------------------------------------------------------
		private bool CheckDBStructureInfo_CanMigrateCompanyDatabase()
		{
			if (OnCanMigrateCompanyDatabase != null)
				return OnCanMigrateCompanyDatabase();

			return false;
        }

		#region Eventi da rimpallare al plugin (per la visualizzazione nelle form delle operazioni)
		//---------------------------------------------------------------------
		public void ExecuteManager_OnElaborationProgressMessage
			(
			object	sender, 
			bool	ok, 
			string	script, 
			string	step, 
			string	detail,
			string	fullPathScript,
			ExtendedInfo ei
			)
		{
			if (OnElaborationProgressMessage != null)
				OnElaborationProgressMessage(sender, ok, script, step, detail, fullPathScript, ei);
		}

		//---------------------------------------------------------------------
		public void ExecuteManager_OnElaborationProgressBar(object sender)
		{
			if (OnElaborationProgressBar != null)
				OnElaborationProgressBar(sender);
		}
			
		//---------------------------------------------------------------------
		public void ExecuteManager_OnUpdateModuleCounter(object sender)
		{
			if (OnUpdateModuleCounter != null)
				OnUpdateModuleCounter(sender);
		}
		
		//---------------------------------------------------------------------
		public void ExecuteManager_OnUpdateMViewCounter(string mView)
		{
			if (OnUpdateMViewCounter != null)
				OnUpdateMViewCounter(mView);
		}

		//---------------------------------------------------------------------
		public void ExecuteManager_OnUpdateMandatoryCols(string table)
		{
			if (OnUpdateMandatoryCols != null)
				OnUpdateMandatoryCols(table);
		}
		
		//---------------------------------------------------------------------
		public void ExecuteManager_OnInsertMessageInListView()
		{
			if (OnInsertMessageInListView != null)
				OnInsertMessageInListView();
		}
		# endregion

		#region Funzioni per la verifica dell'autenticazione di un utente (viene interrogata la Console)

		#region AddUserAuthenticated - Aggiunge l'utente specificato alla lista degli utenti autenticati della Console
		/// <summary>
		/// AddUserAuthenticated
		/// Aggiunge l'utente specificato alla lista degli utenti della Console (lista che contiene gli utenti autenticati)
		/// </summary>
		//---------------------------------------------------------------------
		private void AddUserAuthenticated(string login, string password, string serverName, DBMSType dbType)
		{
			if (OnAddUserAuthenticatedFromConsole != null)
				OnAddUserAuthenticatedFromConsole(login, password, serverName, dbType);
		}
		#endregion

		#region GetUserAuthenticatedPwd - Richiede alla Console la pwd dell'utente già autenticato
		/// <summary>
		/// GetUserAuthenticatedPwd
		/// Richiede alla Console, per l'utente specificato, la sua password (già inserita precedentemente 
		/// poichè l'utente in questione risulta autenticato
		/// </summary>
		//---------------------------------------------------------------------
		private string GetUserAuthenticatedPwd(string login, string serverName)
		{
			string pwd = string.Empty;
			if (OnGetUserAuthenticatedPwdFromConsole != null)
				pwd = OnGetUserAuthenticatedPwdFromConsole(login, serverName);
			return pwd;
		}
		#endregion

		#region IsUserAuthenticated - Richiede alla Console se l'utente specificato è stato già autenticato
		/// <summary>
		/// IsUserAuthenticated
		/// Interroga la Console per verificare se l'utente specificato da login e password risulta autenticato
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsUserAuthenticated(string login, string password, string serverName)
		{
			bool result = false;
			if (OnIsUserAuthenticatedFromConsole != null)
				result = OnIsUserAuthenticatedFromConsole(login, password, serverName);
			return result;
		}
		#endregion

		#endregion

		# region Creazione File di Log da un Diagnostico
		///<summary>
		/// Al termine dell'elaborazione salvo un file di log contenente tutti i messaggi visualizzati
		/// nella form di elaborazione (esclusi le colonne obbligatorie)
		/// <nomedrive>:\<nomeistanza>\Running\Custom\Companies\<company>\Log\AllUsers
		///</summary>
		//---------------------------------------------------------------------------
		public void CreateLogFile()
		{
			//creo un file di log in xml contenente le informazioni delle aziende e degli errori
			string path = this.ContextInfo.PathFinder.GetCustomCompanyLogPath
                (
				    this.ContextInfo.CompanyName, 
				    NameSolverStrings.AllUsers
				);

			if (string.IsNullOrEmpty(path))
				path = this.ContextInfo.PathFinder.GetCustomPath();
			else
			{
				try
				{
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
					// se incontro problemi di accesso per la creazione della cartella creo il file di log
					// nella Custom, in modo da non perdere le informazioni
				}
				catch (IOException)
				{
					path = this.ContextInfo.PathFinder.GetCustomPath();
				}
				catch (UnauthorizedAccessException)
				{
					path = this.ContextInfo.PathFinder.GetCustomPath();
				}
				catch (Exception)
				{
					path = this.ContextInfo.PathFinder.GetCustomPath();
				}
			}

			string fileName = string.Format("{0}-{1}.xml", this.contextInfo.CompanyName, DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
			string filePath = Path.Combine(path, fileName);

			// estraggo al volo una serie di informazioni tecniche dal database (servizio, versione, edizione, etc)
			ExtendedInfo extInfos = new ExtendedInfo();
			extInfos.Add(DatabaseLayerStrings.DetailDatabase, contextInfo.CompanyDBName);
			if (contextInfo.DbType == DBMSType.SQLSERVER)
				extInfos.Add(DatabaseLayerStrings.DetailDBCulture, CultureHelper.GetWindowsCollation(contextInfo.DatabaseCulture, CultureHelper.SupportedDBMS.SQL2000));
			extInfos.Add(DatabaseLayerStrings.DetailUseUnicode, (contextInfo.UseUnicode) ? DatabaseLayerStrings.DetailYes : DatabaseLayerStrings.DetailNo);
			extInfos.Add(DatabaseLayerStrings.Server, contextInfo.CompanyDBServer);

			if (contextInfo.DbType == DBMSType.SQLSERVER)
			{
				string query = "SELECT SERVERPROPERTY('Edition') as Edition, SERVERPROPERTY('ProductLevel') as ProductLevel, SERVERPROPERTY('ProductVersion') as Version";
				 
				try
				{
					using (SqlConnection myConnection = new SqlConnection(contextInfo.ConnectAzDB))
					{
						myConnection.Open();

						using (SqlCommand myCommand = new SqlCommand(query, myConnection))
						{
							using (SqlDataReader myReader = myCommand.ExecuteReader())
							{
								while (myReader.Read())
								{
									extInfos.Add(DatabaseLayerStrings.DetailVersion, myReader["Version"] != DBNull.Value ? myReader["Version"].ToString() : string.Empty);
									extInfos.Add(DatabaseLayerStrings.DetailEdition, myReader["Edition"] != DBNull.Value ? myReader["Edition"].ToString() : string.Empty);
									extInfos.Add(DatabaseLayerStrings.DetailProductLevel, myReader["ProductLevel"] != DBNull.Value ? myReader["ProductLevel"].ToString() : string.Empty);
								}
							}
						}
					}
				}
				catch
				{
				}
			}

			DBManagerDiagnostic.Set(DiagnosticType.Information, string.Format(DatabaseLayerStrings.DetailCompany, contextInfo.CompanyName), extInfos);

			// se ci sono stati piu' di 5 errori negli script visualizzo un msg aggiuntivo
			if (executeManager.ErrorInRunSqlScript && this.DBManagerDiagnostic.TotalErrors > 5)
				DBManagerDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.RerunUpgrade);

			DiagnosticView diagnosticView = new DiagnosticView(this.DBManagerDiagnostic);
			diagnosticView.WriteXmlFile(filePath, LogType.Database);
			diagnosticView.Close();

			// inserisco nella listview il riferimento al file di log appena creato
			ExecuteManager_OnElaborationProgressMessage(null, true, fileName, NameSolverStrings.Log, DatabaseLayerStrings.CreateLogFile, filePath, null);
		}
		#endregion

		#region Gestione rewind database (per riportare indietro il numero di release dei moduli interessati)
		//---------------------------------------------------------------------
		public bool RewindDatabaseRelease(List<DevelopmentModuleRelease> modules, out string message)
		{
			bool result = false;

			try
			{
				if (this.contextInfo.Connection.State != System.Data.ConnectionState.Open)
				{
					message = DatabaseLayerStrings.ErrorConnectionNotValid;
					return result;
				}

				result = this.checkDbStructInfo.DBMarkInfo.UpdateDevelopmentModuleRelease(modules, out message);
			}
			catch (TBException e)
			{
				message = string.Format("Error DatabaseManager::RewindDatabaseRelease ended with errors {0}", e.Message);
				Debug.WriteLine(message);
				return false;
			}
			finally
			{
				contextInfo.CloseConnection();
			}

			return result;
		}
		# endregion

	}
}
