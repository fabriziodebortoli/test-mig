using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	# region Enumerativo DatabaseStatus
	// enumerativo per gestire lo stato del database oggetto di analisi
	//============================================================================
	[Flags]
	public enum DatabaseStatus
	{ 
		EMPTY						= 0,	// DB vuoto
		NOT_EMPTY					= 1,	// DB non completamente vuoto (stato intermedio prima di ulteriori controlli)
		NEED_RECOVERY				= 2,	// necessita di recovery (almeno uno Status = false)
		NEED_UPGRADE				= 4,	// necessita di upgrade (ovvero: tabelle o moduli mancanti, scatti di release)
		NEED_MANDATORY_COLUMNS		= 8,	// è un database completo ma necessita della creazione delle colonne obbligatorie TBCreated a TBModified
		NEED_ROWSECURITY_COLUMNS	= 16,	// è un database completo ma necessita della creazione delle colonne per il RowSecurityLayer
        NEED_TBGUID_COLUMN          = 32,   // è un database completo ma necessita della creazione della colonna TBGuid per le master table
		UNRECOVERABLE				= 64,	// DB privo della DBMark e xciò non utilizzabile
		PRE_40						= 128,  // DB PRE 4.0
		NEED_UPDATE_DBMARK_INFO		= 256   // il DB necessita di gestione pre-upgrade per aggiornare gli addOnModule che hanno cambiato AddOnApp
	}
	#endregion

	//============================================================================
	public class TbGuidTable
    {
        public string TableName = string.Empty;
        public bool IsOnlyToUpdate = false;

        public TbGuidTable(string tableName, bool isOnly)
        {
            TableName = tableName;
            IsOnlyToUpdate =isOnly;
        }
    }
	/// <summary>
	/// Classe che si occupa di ritornare lo stato del database analizzato,
	/// sulla base di un controllo effettuato sulle informazioni fornite dal CatalogInfo
	/// e dalle informazioni memorizzate dalla classe ApplicationDBStructureInfo
	/// </summary>
	//============================================================================
	public class CheckDBStructureInfo
	{
		# region Data members
		public const int mandatoryColsRelNumb = 99999;

		private	DatabaseStatus dbStatus = DatabaseStatus.EMPTY;
		private KindOfDatabase kindOfDb = KindOfDatabase.Company;

		private CatalogInfo catalog		= null;
		private ContextInfo context		= null;
		private Diagnostic	diagnostic	= null;
		private DBMarkInfo	dbMarkInfo	= null;
		private TBConnection currentConnection = null;
		private ApplicationDBStructureInfo appStructInfo = null;

		// lista di appoggio per il controllo delle colonne obbligatorie
		private List<string> registeredTables = new List<string>();

        // lista di appoggio per il controllo della colonna TBGuid obbligatoria nelle mastertable
        private List<TbGuidTable> tablesWithMissingTBGuidCol = new List<TbGuidTable>();

		// lista di appoggio per il controllo delle colonne della rowsecurity obbligatorie nelle mastertable elencati profili di sicurezza
		private List<string> tablesWithMissingRSCols = new List<string>();

		// booleani di appoggio per stabilire la presenza (o l'assenza) delle varie entry sul db
		private bool present = true;
		private	bool absent	= true;

		// array di appoggio per tenere traccia delle tabelle mancanti e per recuperare i file xml di default
		// di Append, che non si trovano nella directory del modulo che dichiara la tabella e che altrimenti
		// non verrebbero caricati in fase di creazione della tabella.
		public List<string> MissingTablesListForAppend = null;

		// lista dei moduli da aggiornare
		public ModuleDBInfoList ModuleToUpgradeList			= null; // devo effettuare un upgrade
		public ModuleDBInfoList ModuleWithMissingTblList	= null; // hanno delle tabelle mancanti
		public ModuleDBInfoList MissingModulesList			= null; // devono essere creati da zero (nuovi)
		// lista dei moduli con tabelle mancanti da ri-creare
		public ModuleDBInfoList ModuleToCreateList			= null; // elenco moduli che devono essere creati
		// lista dei moduli che devono effettuare un recovery dei dati
		public ModuleDBInfoList ModuleToRecoveryList		= null; 
		//list of the module those have to recovery only the mandatory columns TBCreated and TBModified
		public ModuleDBInfoList ModuleToCreateMandColsList	= null;
		// lista dei moduli che dichiarano un nodo PreviousSignature per gestire i cambi di signature
		public List<PreviousSignatureInfo> PreviousUpdateList = null;

		// istanze di DirectGraph per i vari livelli
		public DirectGraph GraphLevel1 = null;
		public DirectGraph GraphLevel2 = null;
		public DirectGraph GraphLevel3 = null;

		public DirectGraph RecoveryGraphLevel1 = null;
		public DirectGraph RecoveryGraphLevel2 = null;
		public DirectGraph RecoveryGraphLevel3 = null;
		# endregion

		# region Properties and events
		//---------------------------------------------------------------------
		public DatabaseStatus DBStatus { get { return dbStatus ; } }
		public ContextInfo ContextInfo { get { return context ; } }
		public CatalogInfo CatalogInfo { get { return catalog ; } }
		public DBMarkInfo DBMarkInfo { get { return dbMarkInfo; } }
		public List<AddOnApplicationDBInfo> AddOnAppList { get { return appStructInfo.ApplicationDBInfoList; } }
		public KindOfDatabase KindOfDb { get { return kindOfDb; } }

		public List<string> TablesWithMissingRSCols { get { return tablesWithMissingRSCols; } }

        public List<TbGuidTable> TablesWithMissingTBGuidCol { get { return tablesWithMissingTBGuidCol; } }

		// spara un evento al DatabaseManager, per legare i dati di default o di esempio ad una tabella mancante
		//---------------------------------------------------------------------
		public delegate void AddDefaultDataMissingTable(string table, string application, string module);
		public event AddDefaultDataMissingTable OnAddDefaultDataMissingTable;

		public delegate void AddSampleDataMissingTable(string table, string application, string module);
		public event AddSampleDataMissingTable OnAddSampleDataMissingTable;

		public delegate bool CanMigrateCompanyDatabase();
		public event CanMigrateCompanyDatabase OnCanMigrateCompanyDatabase;
		#endregion

		#region Costruttore
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="kindOfDb">enum che identifica il tipo di database</param>
		/// <param name="context">istanza di ContextInfo</param>
		/// <param name="diagnostic">oggetto Diagnostic per la messaggistica</param>
		//---------------------------------------------------------------------
		public CheckDBStructureInfo
			(
			KindOfDatabase kindOfDb, 
			ContextInfo context, 
			ApplicationDBStructureInfo appStructInfo,
			ref Diagnostic diagnostic
			)
		{
			this.kindOfDb		= kindOfDb;
			this.context		= context;
			this.appStructInfo	= appStructInfo;
			this.diagnostic		= diagnostic;

			// a seconda del tipo di database valorizzo la connessione giusta
			this.currentConnection = (kindOfDb == KindOfDatabase.Dms) ? context.DmsConnection : context.Connection;

			// istanzio un oggetto di tipo CatalogInfo
			catalog = new CatalogInfo();
			// carico il catalog del database connesso
			catalog.Load(this.currentConnection, false);
		}
		# endregion

		# region Check if the specific table is an application table 
		//---------------------------------------------------------------------
		public bool GetApplicationModuleInfo(string tableName, out ModuleDBInfo modDBInfo)
		{
			modDBInfo  = null;
			foreach (AddOnApplicationDBInfo addOnAppDBInfo in appStructInfo.ApplicationDBInfoList)
			{
				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{					
					foreach (EntryDBInfo entryDBInfo in moduleDBInfo.TablesList)
					{
						if (string.Compare(entryDBInfo.Name, tableName, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							modDBInfo = moduleDBInfo;
							return true;
						}
					}
				}
			}
			return false;
		}
		#endregion

		# region GetDatabaseStatus
		/// <summary>
		/// Match della struttura del Catalog con quella generata dalla ApplicationDBStructureInfo
		/// e verifica dello stato del db
		/// </summary>
		//---------------------------------------------------------------------
		public void GetDatabaseStatus()
		{
			CheckExistTables();

			// istanzio la classe che si occupa di leggere le info dalla tabella DBMark
			dbMarkInfo = new DBMarkInfo(this.currentConnection, kindOfDb);

			// se il database risulta vuoto, carico le informazioni e lo segnalo
			if (dbStatus == DatabaseStatus.EMPTY)
			{
				LoadCreateInformations();
				if (kindOfDb == KindOfDatabase.Company)
					diagnostic.Set(DiagnosticType.Information | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.MsgEmptyDB, DatabaseLayerConsts.ERPSignature));
				if (kindOfDb == KindOfDatabase.Dms)
					diagnostic.Set(DiagnosticType.Information | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.MsgEmptyDB, DatabaseLayerConsts.DMSSignature));
			}

			if (dbStatus == DatabaseStatus.NOT_EMPTY)
			{
				// se non esiste la tabella DBMark non procedo ed imposto lo stato UNRECOVERABLE
				if (!catalog.GetExistingTableInfo(dbMarkInfo.DBMarkTableName, DBObjectTypes.TABLE))
				{
					diagnostic.Set(DiagnosticType.Error | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.ErrDBMarkNotExist, dbMarkInfo.DBMarkTableName));
					dbStatus = DatabaseStatus.UNRECOVERABLE;
					return;
				}	

				// se è un database di sistema e la MSD_DBMark esiste devo controllare di che tipo è la colonna Application 
				// per il db aziendale non è necessario, xchè vado a leggere direttamente il flag UseUnicode sulla MSD_Companies
				if (kindOfDb == KindOfDatabase.System)
					context.UseUnicode = catalog.IsUnicodeValueInColumn(this.currentConnection, DatabaseLayerConsts.MSD_DBMark, "Application");

				// carico in un DataTable le informazioni della DBMark
				dbMarkInfo.LoadInfosFromDBMark();

				if (kindOfDb == KindOfDatabase.Company)
				{
					// controllo per capire se il db di partenza e' di una 3.x
					int dbRelCore = dbMarkInfo.GetDBReleaseFromDBMark(DatabaseLayerConsts.ERPSignature, DatabaseLayerConsts.ERPCoreModuleName, true);
					if (dbRelCore < 400)
					{
						// Michela: questo stato "pilota" anche la gestione degli scatti di release paralleli tra 3.x e 4.x, pertanto attenzione a non spostare questa istruzione!
						dbStatus = (dbStatus | DatabaseStatus.PRE_40);

						// chiedo a LM se l'attivazione corrente consente di aggiornare il database alla 4.0
						if (OnCanMigrateCompanyDatabase == null || !OnCanMigrateCompanyDatabase())
						{
                            // se non e' possibile visualizzo msg e ritorno subito
                            diagnostic.Set(DiagnosticType.Error | DiagnosticType.LogOnFile, DatabaseLayerStrings.ErrCantMigrateDBPre40);
							return;
						}
					}
				}

				// eseguo il check delle applicazioni con previous signature solo per il database aziendale
				if (kindOfDb == KindOfDatabase.Company)
					if (CheckModulesWithPreviousSignature())
					{
						dbStatus = (dbStatus | DatabaseStatus.NEED_UPDATE_DBMARK_INFO);
						diagnostic.Set(DiagnosticType.Information | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.MsgNeedUpdateDBMarkInfo, dbMarkInfo.DBMarkTableName), new ExtendedInfo());
					}

				// per ogni applicazione + modulo cerco le righe con Status = 0 e riempio le relative strutture
				if (CheckDBRecoveryStatus())
					dbStatus = (dbStatus | DatabaseStatus.NEED_RECOVERY);

				// vado a controllare i moduli che devono eseguire l'upgrade
				if (CheckDBUpgradeStatus())
					dbStatus = (dbStatus | DatabaseStatus.NEED_UPGRADE);
			}

			// se si tratta di un db aziendale e lo stato è SOLO NOT_EMPTY significa che è completo
			// ma devo cmq controllare le colonne obbligatorie, comprese quelle per il RowSecurityLayer
			if (kindOfDb == KindOfDatabase.Company && dbStatus == DatabaseStatus.NOT_EMPTY)
			{
				// effettuo un ulteriore controllo sulla TB_DBMark per verificare se le colonne obbligatorie TBCreated e TBModified esistono
				if (GetTablesListWithNoMandatoryColumns().Count > 0)
				{
					dbStatus = (dbStatus | DatabaseStatus.NEED_MANDATORY_COLUMNS);
					diagnostic.Set(DiagnosticType.Information | DiagnosticType.LogOnFile, DatabaseLayerStrings.MsgNeedMandatoryColumns, new ExtendedInfo());
				}

				//verifico che le tabelle master abbiamo tutte il campo TBGuid se così non fosse creo il campo
				CheckMissingTBGuidColumn();
                if (tablesWithMissingTBGuidCol.Count > 0)
                {
                    dbStatus = (dbStatus | DatabaseStatus.NEED_TBGUID_COLUMN);
					diagnostic.Set(DiagnosticType.Information | DiagnosticType.LogOnFile, DatabaseLayerStrings.MsgNeedMandatoryTBGuidCol, new ExtendedInfo());
				}

				// se il plugin e' attivato e la company corrente usa il RowSecurity procedo con i controlli
				if (context.IsRowSecurityActivated && context.UseRowSecurity)
				{
					CheckMissingRSColumns();
					if (tablesWithMissingRSCols.Count > 0)
					{
						dbStatus = (dbStatus | DatabaseStatus.NEED_ROWSECURITY_COLUMNS);
						ExtendedInfo ei = new ExtendedInfo();
						diagnostic.Set(DiagnosticType.Information | DiagnosticType.LogOnFile, DatabaseLayerStrings.MsgNeedMandatoryColumnsForRS, ei);
					}
				}
			}

			if (kindOfDb == KindOfDatabase.Company && dbStatus == DatabaseStatus.NOT_EMPTY)
				diagnostic.Set(DiagnosticType.Information | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.MsgFullDB, DatabaseLayerConsts.ERPSignature));

			// se si tratta di un db documentale e lo stato è solo NOT_EMPTY significa che è completo
			if (kindOfDb == KindOfDatabase.Dms && dbStatus == DatabaseStatus.NOT_EMPTY)
				diagnostic.Set(DiagnosticType.Information | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.MsgFullDB, DatabaseLayerConsts.DMSSignature));
		}
		# endregion	

		# region Gestione colonne obbligatorie
		///<summary>
		/// Ritorna l'elenco delle tabelle presenti nel database che non possiedono tutte
		/// le colonne obbligatorie (TBCreated, TBModified, TBCreatedID, TBModifiedID)
		/// (solo per il database aziendale)
		///</summary>
		//---------------------------------------------------------------------------
		public List<string> GetTablesListWithNoMandatoryColumns()
		{
			List<string> tablesWithNoMandatoryColumns = new List<string>();

			string textCmd = DatabaseLayerConsts.SQLTablesWithNoMandatoryFields;

			if (this.currentConnection.IsOracleConnection())
				textCmd = DatabaseLayerConsts.ORACLETablesWithNoMandatoryFields;
            if (this.currentConnection.IsPostgreConnection())
                textCmd = DatabaseLayerConsts.PostgreTablesWithNoMandatoryFields;

			IDataReader reader = null;
			TBCommand command = null;

			try
			{
				command = new TBCommand(textCmd, this.currentConnection);
				reader = command.ExecuteReader();

				// se il reader e' null oppure non ha estratto righe 
				// significa che tutte le tabelle hanno le colonne obbligatorie
				if (reader == null || !this.currentConnection.DataReaderHasRows(reader))
					return tablesWithNoMandatoryColumns;

				while (reader.Read())
				{
					string tableName = reader["TABLE_NAME"].ToString();

					// la tabella e' tra quelle dichiarate dall'applicazione e
					// non e' gia' presente nella lista la devo considerare, quindi l'aggiungo
					if (
						!string.IsNullOrWhiteSpace(registeredTables.Find((s) => s.CompareNoCase(tableName))) &&
						string.IsNullOrWhiteSpace(tablesWithNoMandatoryColumns.Find((s) => s.CompareNoCase(tableName)))
						)
						tablesWithNoMandatoryColumns.Add(tableName);
				}
			}
			catch (TBException e)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("GetTablesListWithNoMandatoryColumns. TBException {0}", e.Message));
				return tablesWithNoMandatoryColumns;
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
					reader.Dispose();
				}
				command.Dispose();
			}

			return tablesWithNoMandatoryColumns;
		}
		# endregion
        
		# region Gestione colonna obbligatorie TBGuid sulle masterTable dell'applicativo
        //---------------------------------------------------------------------------
        public bool IsTableWithEmptyTBGuid(string tableName)
        {
			string textCmd = (context.DbType == DBMSType.SQLSERVER) ? DatabaseLayerConsts.SQLSelectEmptyGuidColumn : DatabaseLayerConsts.OracleSelectEmptyGuidColumn;
			string tbGuid = (context.DbType == DBMSType.SQLSERVER) ? DatabaseLayerConsts.TBGuidColNameForSql : DatabaseLayerConsts.TBGuidColNameForOracle;

            int count = 0;

            try
            {
				string query = string.Format(textCmd, (context.DbType == DBMSType.SQLSERVER) ? tableName : tableName.ToUpperInvariant(), tbGuid);
				using (TBCommand command = new TBCommand(query, this.currentConnection))
				{
					count = command.ExecuteTBScalar();
					return (count > 0);
				}
            }
            catch (TBException e)
            {
				System.Diagnostics.Debug.WriteLine(string.Format("IsTableWithEmptyTBGuid. TBException {0}", e.Message));
                return false;
            }
        }

        ///<summary>
        /// Scorro la lista delle tabelle registrate, per ognuna che sia una masterTable (vedi attributo nel file DatabaseObjects.xml)
        /// verifico che esista sul database e/o che contenga la colonna obbligatoria TBGuid
        /// Se cosi' non fosse la memorizzo una lista a parte che mi servira' poi per creare la colonna TBGuid
        ///</summary>
        //---------------------------------------------------------------------------
        public void CheckMissingTBGuidColumn()
        {
            // devo forzare il ri-caricamento del catalog perche' sicuramente e' cambiato
            // essendo stato effettuato un aggiornamento del database
            catalog.Load(context.Connection, false);
            tablesWithMissingTBGuidCol.Clear();

            ModuleDBInfo moduleDBInfo = null;

            foreach (string tblName in registeredTables)
            {
                // per ogni tabella registrata dall'applicazione,
                // devo controllare se e' dichiarata come mastertable e se contiene la colonna obbligatoria RowSecurityID

                //check if the table is a table of an application
                if (GetApplicationModuleInfo(tblName, out moduleDBInfo))
                {
                    EntryDBInfo entryDBInfo = moduleDBInfo.TablesList.Find(a => string.Compare(a.Name, tblName, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if (entryDBInfo != null && entryDBInfo.MasterTable)
                    {
                        // prima carico le columnInfo (di default non sono caricate)
                        CatalogTableEntry cte = catalog.GetTableEntry(tblName);
                        if (cte == null)
                            continue;
                        cte.LoadColumnsInfo(context.Connection, false);

                        CatalogColumn cc = cte.GetColumnInfo((context.DbType == DBMSType.SQLSERVER) ? DatabaseLayerConsts.TBGuidColNameForSql : DatabaseLayerConsts.TBGuidColNameForOracle);
                        // il metodo GetColumnInfo ritorna null sia che la tabella o la colonna non esistano
                        // quindi in ogni caso mi tengo da parte il nome della tabella
                        if (cc == null)
                            tablesWithMissingTBGuidCol.Add(new TbGuidTable(tblName, false));
                        else
                            //se esiste la colonna verifico se è necessario effettuare un update dei valori di TBGuid = NULL oppure = 0x00
                            if (IsTableWithEmptyTBGuid(tblName))
                                tablesWithMissingTBGuidCol.Add(new TbGuidTable(tblName, true));                       
                    }
                }
            }
        }
        # endregion

		# region Gestione colonne obbligatorie sulle masterTable del RowSecurityLayer
		///<summary>
		/// Scorro la lista delle tabelle registrate, per ognuna che sia contenuta nella lista
		/// delle masterTable dichiarate verifico che esista sul database e/o che contenga
		/// la colonna obbligatoria RowSecurityID
		/// Se cosi' non fosse la memorizzo una lista a parte che mi servira' poi per creare la colonna
		///</summary>
		//---------------------------------------------------------------------------
		public void CheckMissingRSColumns()
		{
			// devo forzare il ri-caricamento del catalog perche' sicuramente e' cambiato
			// essendo stato effettuato un aggiornamento del database
			catalog.Load(context.Connection, false);
			tablesWithMissingRSCols.Clear();

			foreach (string tblName in registeredTables)
			{
				// per ogni tabella registrata dall'applicazione,
				// devo controllare se e' dichiarata come mastertable e se contiene la colonna obbligatoria RowSecurityID
				if (appStructInfo.RSMasterTables.Contains(tblName))
				{
					// prima carico le columnInfo (di default non sono caricate)
					CatalogTableEntry cte = catalog.GetTableEntry(tblName);
					if (cte == null)
						continue;
					cte.LoadColumnsInfo(context.Connection, false);

					CatalogColumn cc = cte.GetColumnInfo((context.DbType == DBMSType.SQLSERVER) ? DatabaseLayerConsts.RowSecurityIDForSQL : DatabaseLayerConsts.RowSecurityIDForOracle);
					// il metodo GetColumnInfo ritorna null sia che la tabella o la colonna non esistano
					// quindi in ogni caso mi tengo da parte il nome della tabella
					if (cc == null)
						tablesWithMissingRSCols.Add(tblName);
				}
			}
		}
		# endregion

		# region Check esistenza tabella e primo stato di database (EMPTY OR NOT_EMPTY)
		/// <summary>
		/// per ogni tabella (di ogni modulo di ogni applicazione) viene controllata la sua esistenza
		/// e viene conteggiata
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckExistTables()
		{
			if (appStructInfo.ApplicationDBInfoList.Count <= 0)
				return;

			foreach (AddOnApplicationDBInfo addOnAppDBInfo in appStructInfo.ApplicationDBInfoList)
			{
				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{	
					int numtab = 0;

					// controllo e conto le tabelle
					foreach (EntryDBInfo entryDBInfo in moduleDBInfo.TablesList)
					{
						registeredTables.Add(entryDBInfo.Name); // tengo da parte la lista delle tabelle dichiarate

						if (catalog.GetExistingTableInfo(entryDBInfo.Name, DBObjectTypes.TABLE))
						{
							entryDBInfo.Exist = true;
							numtab++;
						}
					}

					// controllo e conto le view
					foreach (EntryDBInfo entryDBInfo in moduleDBInfo.ViewsList)
					{
						if (catalog.GetExistingTableInfo(entryDBInfo.Name, DBObjectTypes.VIEW))
						{
							entryDBInfo.Exist = true;
							numtab++;
						}
					}

					// controllo e conto le stored procedure
					foreach (EntryDBInfo entryDBInfo in moduleDBInfo.ProceduresList)
					{
						if (catalog.GetExistingTableInfo(entryDBInfo.Name, DBObjectTypes.ROUTINE))
						{
							entryDBInfo.Exist = true;
							numtab++;
						}
					}

					moduleDBInfo.NumExistTables = numtab;
				}
			}

			SetBooleanInAddOnAppDBInfo();

			dbStatus = (!present && absent) ? DatabaseStatus.EMPTY : DatabaseStatus.NOT_EMPTY; 
		}

		/// <summary>
		/// per aggiornare i flag allTablesPresent e allTablesAbsent, in base all'analisi
		/// delle due strutture dati
		/// </summary>
		//---------------------------------------------------------------------------
		private void SetBooleanInAddOnAppDBInfo()
		{
			foreach (AddOnApplicationDBInfo addOnAppDBInfo in appStructInfo.ApplicationDBInfoList)
			{
				int totEntryDB = 0;

				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{
					totEntryDB = moduleDBInfo.TablesList.Count + moduleDBInfo.ViewsList.Count + moduleDBInfo.ProceduresList.Count;
					
					// non esiste alcun entry nel modulo
					if (moduleDBInfo.NumExistTables == 0)
					{
						addOnAppDBInfo.AllTablesPresent = addOnAppDBInfo.AllTablesPresent && false;
						if (!addOnAppDBInfo.AllTablesAbsent)
							addOnAppDBInfo.AllTablesAbsent = addOnAppDBInfo.AllTablesAbsent && true;
						continue;
					}
					
					// il modulo ha meno tabelle di quelle previste
					if (moduleDBInfo.NumExistTables < totEntryDB)
					{
						addOnAppDBInfo.AllTablesPresent = addOnAppDBInfo.AllTablesPresent && false;
						addOnAppDBInfo.AllTablesAbsent  = addOnAppDBInfo.AllTablesAbsent && false;
						continue;
					}

					// il modulo ha tutte le tabelle
					if (moduleDBInfo.NumExistTables == totEntryDB)
					{
						if (!addOnAppDBInfo.AllTablesPresent)
							addOnAppDBInfo.AllTablesPresent = addOnAppDBInfo.AllTablesPresent && false;
						addOnAppDBInfo.AllTablesAbsent = addOnAppDBInfo.AllTablesAbsent && false;
						continue;
					}
				}

				present = present && addOnAppDBInfo.AllTablesPresent;
				absent	= absent && addOnAppDBInfo.AllTablesAbsent;
			}
		}
		#endregion

		#region Check moduli con il nodo PreviousSignature
		///<summary>
		/// Per tutti i moduli che hanno dichiarato il nodo <PreviousSignature> nel DatabaseObjects.xml
		/// vado a cercare i dati dell'addonmodule che vanno a sostituire nella struttura della DBMark in memoria 
		/// e me le tengo da parte in una lista. 
		///</summary>
		//---------------------------------------------------------------------------
		private bool CheckModulesWithPreviousSignature()
		{
			PreviousUpdateList = new List<PreviousSignatureInfo>();

			bool atLeastOneNeedsUpdate = false;
			foreach (AddOnApplicationDBInfo app in appStructInfo.ApplicationDBInfoList)
			{
				foreach (ModuleDBInfo module in app.ModuleList)
				{
					if (string.IsNullOrWhiteSpace(module.PreviousApplication) || string.IsNullOrWhiteSpace(module.PreviousModule))
						continue;

					PreviousSignatureInfo psi = dbMarkInfo.CreatePreviousSignatureInfo(module);
					if (psi != null)
					{
						PreviousUpdateList.Add(psi);
						atLeastOneNeedsUpdate = true;
					}
				}
			}

			return atLeastOneNeedsUpdate;
		}
		#endregion

		#region Check del database per il recovery
		/// <summary>
		/// funzione che controlla le righe della DBMark ed individua i moduli con Status a false
		/// che necessitano quindi della procedura di ripristino.
		/// Riempimento dell'apposita struttura RecoveryInfo (a livello di ModuleDBInfo) 
		/// </summary>
		//---------------------------------------------------------------------------
		private bool CheckDBRecoveryStatus()
		{
			// considero i soli moduli che hanno lo status a false
			DataTable dtRecoveryModules = dbMarkInfo.GetDBRecoveryStatus();

			for (int i = dtRecoveryModules.Rows.Count - 1; i >= 0; i--)
			{
				DataRow row = dtRecoveryModules.Rows[i];

				string application = row["Application"].ToString();
				string module = row["AddOnModule"].ToString();

				ModuleDBInfo info = GetModuleItemFromSignature(application, module);
				if (info != null)
				{
					info.RecoveryInfo = new RecoveryInfo
						(
						application,
						module,
						Convert.ToInt32(row["DBRelease"]),
						Convert.ToInt32(row["UpgradeLevel"]),
						Convert.ToInt32(row["Step"])
						);
					diagnostic.Set(DiagnosticType.Warning | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.ErrModuleNeedRecovery, info.Title, info.ApplicationBrand));
				}
				else
				{
					// caso in cui nella TB_DBMark ho un modulo con Status = false ma non esiste nel filesystem 
					// visualizzo un avvertimento e elimino il modulo dalla lista di quelli da considerare
					diagnostic.Set(DiagnosticType.Warning | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.ErrModuleIgnored, module, application));
					dtRecoveryModules.Rows[i].Delete();
					dtRecoveryModules.AcceptChanges();
				}
			}

			if (dtRecoveryModules.Rows.Count > 0)
				FindRecoveryInformations();

			return dtRecoveryModules.Rows.Count > 0;
		}

		/// <summary>
		/// per ogni modulo che necessita di recovery vado a cercare le informazioni, parsando i file xml di
		/// configurazione e cercando gli script da eseguire per ripristinare lo stato coerente del db.
		/// </summary>
		//---------------------------------------------------------------------------
		private void FindRecoveryInformations()
		{
			ModuleToRecoveryList = new ModuleDBInfoList();

			//for the mandatory columns TBCreated and TBModified
			ModuleToCreateMandColsList = new ModuleDBInfoList();

			RecoveryGraphLevel1 = new DirectGraph();
			RecoveryGraphLevel2 = new DirectGraph();
			RecoveryGraphLevel3 = new DirectGraph();

			foreach (AddOnApplicationDBInfo addOnAppDBInfo in appStructInfo.ApplicationDBInfoList)
			{
				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{	
					// se è null significa che non devo fare operazioni di recovery (x pararci!)
					if (moduleDBInfo.RecoveryInfo == null)
						continue;

					//I need the recovery only for creating the mandatory columns TBCreated and TBModified
					if (moduleDBInfo.RecoveryInfo.NrStep == mandatoryColsRelNumb)
					{
						ModuleToCreateMandColsList.Add(moduleDBInfo);
						continue;
					}

					// numero di release <= 1 bisogna parsare il CreateInfo.xml altrimenti l'UpgradeInfo.xml
					moduleDBInfo.XmlPath =
						(moduleDBInfo.RecoveryInfo.NrRelease == 1) //(moduleDBInfo.NrRelease <= 1)
						? context.PathFinder.GetStandardCreateInfoXML(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName)
						: context.PathFinder.GetStandardUpgradeInfoXML(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);

					// estrapolo il path della directory a cui agganciare successivamente il path degli script sql
					FileInfo fi = new FileInfo(moduleDBInfo.XmlPath);
					if (!fi.Exists)
					{
						diagnostic.Set
							(
							DiagnosticType.Error | DiagnosticType.LogOnFile, 
							string.Format
							(
							DatabaseLayerStrings.ErrNoRecoveryFileExist, 
							(moduleDBInfo.NrRelease <= 1) ? DatabaseLayerConsts.CreateInfoFile : DatabaseLayerConsts.UpgradeInfoFile,
							moduleDBInfo.ModuleName,
							moduleDBInfo.ApplicationBrand
							));

						continue;
					}
					moduleDBInfo.DirectoryScript = fi.Directory.FullName;

					// richiamo la funzione che mi consente di fare un parsing "mirato" del livello e step che mi serve
					moduleDBInfo.RecoveryInfo.FindRecoveryData(moduleDBInfo.XmlPath, this);

					ModuleToRecoveryList.Add(moduleDBInfo);
				}
			}
		}
		# endregion

		# region Gestione informazioni per la creazione di un db nuovo
		/// <summary>
		/// richiamata solo se il database risulta vuoto
		/// per ogni applicazione+modulo faccio il parse del file CreateInfo.xml e viene riempito un array list
		/// con l'elenco di tutte le operazioni da effettuare.
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadCreateInformations()
		{
			ModuleToCreateList = new ModuleDBInfoList();

			// per ogni AddOn + Module cerco il path del file CreateInfo.xml (se esiste)
			foreach (AddOnApplicationDBInfo addOnAppDBInfo in appStructInfo.ApplicationDBInfoList)
			{
				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{
					moduleDBInfo.XmlPath = 
						context.PathFinder.GetStandardCreateInfoXML(addOnAppDBInfo.ApplicationName, moduleDBInfo.ModuleName);

					if (moduleDBInfo.XmlPath.Length == 0)
						continue;

					FileInfo fi = new FileInfo(moduleDBInfo.XmlPath);

					if (!fi.Exists)
					{
						diagnostic.Set 
							(
							DiagnosticType.Error | DiagnosticType.LogOnFile, 
							string.Format(DatabaseLayerStrings.ErrCreateInfoFileNotExist, moduleDBInfo.ModuleName, moduleDBInfo.ApplicationBrand)
							);

						continue;
					}
					moduleDBInfo.DirectoryScript = fi.Directory.FullName;

					ModuleToCreateList.Add(moduleDBInfo);
				}
			}
		}
		# endregion

		# region Check del database per l'upgrade
		/// <summary>
		/// esecuzione dei vari controlli sulle versioni del database oggetto di analisi:
		/// 1. tabelle mancanti in moduli pre-esistenti
		/// 2. moduli mancanti (nuovi)
		/// 3. passaggi di release da effettuare
		/// </summary>
		//---------------------------------------------------------------------------
		private bool CheckDBUpgradeStatus()
		{
			// inizializzo l'array di appoggio per cercare i file xml con i dati di default di Append
			MissingTablesListForAppend = new List<string>();

			ModuleToUpgradeList		= new ModuleDBInfoList();
			ModuleWithMissingTblList= new ModuleDBInfoList();
			MissingModulesList		= new ModuleDBInfoList();

			GraphLevel1	= new DirectGraph();
			GraphLevel2	= new DirectGraph();
			GraphLevel3 = new DirectGraph();

			int numRelMark;
			// eseguo prima i vari controlli:
			// 1. se devo fare un passaggio di release (confronto la dbrel sulla TB_DBMark con quella del DatabaseObjects.xml)
			// 2. se ci sono delle tabelle mancanti... ossia eliminate a mano dall'utente
			// 3. se ci sono dei moduli mancanti... ossia che non compaiono nella TB_DBMark
			foreach (AddOnApplicationDBInfo addOnAppDBInfo in appStructInfo.ApplicationDBInfoList)
			{
				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{
					// leggo il record della DBMark, in corrispondenza di Applicazione + Modulo
					numRelMark = dbMarkInfo.GetDBReleaseFromDBMark(moduleDBInfo.ApplicationSign, moduleDBInfo.ModuleSign);

					// per evitare di fare gli upgrade nel caso in cui il recovery debba essere effettuato
					// per la release 1, ossia nel caso in cui si sia verificato in precedenza un errore in 
					// fase di creazione delle tabelle
					if (numRelMark == 1 && moduleDBInfo.RecoveryInfo != null)
					{
						if (moduleDBInfo.RecoveryInfo.NrRelease == 1)
							continue;
					}

					// se il numero di release indicato nella TB_DBMark è >= 0 allora procedo nell'analisi
					// se è inferiore a zero significa che manca proprio la riga nella tabella e 
					// significa che devo creare il modulo da zero.
					if (numRelMark >= 0)
					{
						moduleDBInfo.UpdateInfo.DbMarkSignature	= moduleDBInfo.ApplicationSign;
						moduleDBInfo.UpdateInfo.DbMarkModule	= moduleDBInfo.ModuleSign;
						moduleDBInfo.UpdateInfo.DbMarkRel		= numRelMark;

						// solo se il nr. di rel indicato sul DatabaseObjects.xml è maggiore di quello presente
						// sulla TB_DBMark, inserisco quel modulo sulla lista dei moduli da upgradare
						if (moduleDBInfo.DBRelease > moduleDBInfo.UpdateInfo.DbMarkRel)
						{
							ModuleToUpgradeList.Add(moduleDBInfo);

							diagnostic.Set
								(DiagnosticType.Warning | DiagnosticType.LogOnFile, 
								string.Format
									(
									DatabaseLayerStrings.MsgModuleNeedUpgrade,
									moduleDBInfo.UpdateInfo.DbMarkRel, 
									moduleDBInfo.DBRelease, 
									moduleDBInfo.Title,
									moduleDBInfo.ApplicationBrand
									));
						}
						else
						{
							// se il nr. di rel indicato sul DatabaseObjects.xml è minore stretto di quello presente
							// sulla TB_DBMark, significa che ho una versione di db più aggiornata rispetto a quella 
							// dell'installazione, pertanto devo visualizzare un messaggio di incongruenza
							if (moduleDBInfo.DBRelease < moduleDBInfo.UpdateInfo.DbMarkRel)
								diagnostic.Set
									(DiagnosticType.Error | DiagnosticType.LogOnFile, 
									string.Format
									(
									DatabaseLayerStrings.MsgModuleContradictoryRelease,
									moduleDBInfo.UpdateInfo.DbMarkRel, 
									moduleDBInfo.Title,
									moduleDBInfo.ApplicationBrand,
									moduleDBInfo.DBRelease
									));
						}
					}
					else
					{
						// questo lo marchio come un modulo mancante
						moduleDBInfo.IsNew = true;

						// devo creare il modulo da zero (quindi leggo il folder Create)
						moduleDBInfo.XmlPath = 
							context.PathFinder.GetStandardCreateInfoXML(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);

						if (string.IsNullOrWhiteSpace(moduleDBInfo.XmlPath))
							continue;

						FileInfo fi = new FileInfo(moduleDBInfo.XmlPath);
						if (!fi.Exists)
						{
							diagnostic.Set
								(DiagnosticType.Warning | DiagnosticType.LogOnFile, 
								string.Format(DatabaseLayerStrings.ErrCreateInfoFileNotExist, moduleDBInfo.Title, moduleDBInfo.ApplicationBrand));
							continue;
						}
						moduleDBInfo.DirectoryScript = fi.Directory.FullName;

						// lo aggiungo cmq alla lista dei moduli da aggiornare
						ModuleToUpgradeList.Add(moduleDBInfo);

						// ne tengo traccia anche qui altrimenti potrei fare casino ad identificare gli oggetti mancanti
						MissingModulesList.Add(moduleDBInfo);

						diagnostic.Set
							(DiagnosticType.Warning | DiagnosticType.LogOnFile, 
							string.Format(DatabaseLayerStrings.MsgNewModuleToCreate, moduleDBInfo.Title, moduleDBInfo.ApplicationBrand));
					}

					//**** GESTIONE TABELLE MANCANTI ****//
					// se ho trovato nel modulo almeno una tabella mancante procedo nella gestione
					// e inserisco una voce nella lista dei moduli con tabelle mancanti
					if (FindModuleMissingObjects(moduleDBInfo))
					{
						MissingTablesManagement(moduleDBInfo);
						ModuleWithMissingTblList.Add(moduleDBInfo);
					}
				}
			}

			// se non ho neppure un elemento in tutti gli ArrayList significa che non 
			// devo effettuare alcun upgrade e ritorno false
			if (ModuleToUpgradeList.Count		== 0 && 
				ModuleWithMissingTblList.Count	== 0 && 
				MissingModulesList.Count		== 0)
				return false;

			return true;
		}
		# endregion

		# region Gestione Oggetti Mancanti 
		/// <summary>
		/// controllo relativo agli oggetti mancanti per modulo. Memorizzo nell'apposito array
		/// le informazioni di ogni oggetto che risulta mancante sul database.
		/// </summary>
		//---------------------------------------------------------------------------
		private bool FindModuleMissingObjects(ModuleDBInfo moduleDBInfo)
		{
			// prima di scendere al dettaglio degli oggetti mancanti controllo se il 
			// modulo è già stato inserito nell'array di quei moduli che devono essere 
			// creati da zero (xchè non esiste l'entry nella TB_DBMark)
			foreach (ModuleDBInfo info in MissingModulesList)
			{
				if (string.Compare(moduleDBInfo.ModuleName, info.ModuleName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return false;
			}

			bool found = false;

			/* N.B.: un'entry di database si può considerare mancante se:
			 * 1. il modulo ha lo status a false e
			 *    il numero di release di creazione dell'entry è uguale a quello in cui si è verificato l'errore e
			 *	  il numero di step è maggiore o uguale a quello in cui si è verificato l'errore
			 * oppure
			 * 2. il numero di release di creazione dell'entry è minore uguale a quello indicato nella DBMark */
			foreach (EntryDBInfo entry in moduleDBInfo.TablesList)
			{
				if (!entry.Exist)
				{
					// in questo caso la tabella è mancante
					if (entry.Rel <= moduleDBInfo.UpdateInfo.DbMarkRel)
					{
						if (moduleDBInfo.RecoveryInfo != null					&&
							entry.Rel == moduleDBInfo.RecoveryInfo.NrRelease	&&
							entry.Step >=  moduleDBInfo.RecoveryInfo.NrStep)
							continue;
						else
						{
							moduleDBInfo.MissingEntryList.Add(entry);
							MissingTablesListForAppend.Add(entry.Name);
						}
					}
					else
					{
						// se il numero di release di creazione della tabella è maggiore del numero di release 
						// del modulo (indicato nel tag <Release> del DatabaseObjects.xml) segnalo un 
						// messaggio di errore di incongruenza 
						if (entry.Rel > moduleDBInfo.DBRelease)
							diagnostic.Set
								(DiagnosticType.Error | DiagnosticType.LogOnFile, 
								string.Format
								(
								DatabaseLayerStrings.ErrCheckMissingObject,
								entry.Name,
								moduleDBInfo.Title,
								moduleDBInfo.ApplicationBrand,
								entry.Rel,
								moduleDBInfo.UpdateInfo.DbMarkRel							
								));
						else
						{
							// se l'entry è mancante perchè verrà creata in uno scatto di release corrente,
							// devo cmq tenere traccia dell'eventuale file di default/esempio associato alla tabella
							if (OnAddDefaultDataMissingTable != null)
								OnAddDefaultDataMissingTable(entry.Name, moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);
							if (OnAddSampleDataMissingTable != null)
								OnAddSampleDataMissingTable(entry.Name, moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);

							// ed eventualmente anche i file di Append
							MissingTablesListForAppend.Add(entry.Name);
							continue;
						}
					}

					// per la gestione dei dati di default/esempio 
					//(sparo un evento al DatabaseManager xchè qui non ho la visibilità del puntatore al BaseImportExportManager)
					if (OnAddDefaultDataMissingTable != null)
						OnAddDefaultDataMissingTable(entry.Name, moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);
					if (OnAddSampleDataMissingTable != null)
						OnAddSampleDataMissingTable(entry.Name, moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);

					found = true;
				}
			}

			foreach (EntryDBInfo entry in moduleDBInfo.ViewsList)
			{
				if (!entry.Exist)
				{
					// in questo caso la view è mancante
					if (entry.Rel <= moduleDBInfo.UpdateInfo.DbMarkRel)
					{
						if (moduleDBInfo.RecoveryInfo != null					&&
							entry.Rel == moduleDBInfo.RecoveryInfo.NrRelease	&&
							entry.Step >=  moduleDBInfo.RecoveryInfo.NrStep)
							continue;
						else
							moduleDBInfo.MissingEntryList.Add(entry);
					}
					else
					{
						// se il numero di release di creazione della view è maggiore del numero di release 
						// del modulo (indicato nel tag <Release> del DatabaseObjects.xml) segnalo un 
						// messaggio di errore di incongruenza 
						if (entry.Rel > moduleDBInfo.DBRelease)
							diagnostic.Set
								(DiagnosticType.Error | DiagnosticType.LogOnFile, 
								string.Format
								(
								DatabaseLayerStrings.ErrCheckMissingObject,
								entry.Name,
								moduleDBInfo.Title,
								moduleDBInfo.ApplicationBrand,
								entry.Rel,
								moduleDBInfo.UpdateInfo.DbMarkRel							
								));
						continue;
					}
					
					found = true;
				}
			}

			foreach (EntryDBInfo entry in moduleDBInfo.ProceduresList)
			{
				if (!entry.Exist)
				{
					// in questo caso la stored-procedure è mancante
					if (entry.Rel <= moduleDBInfo.UpdateInfo.DbMarkRel)
					{
						if (moduleDBInfo.RecoveryInfo != null					&&
							entry.Rel == moduleDBInfo.RecoveryInfo.NrRelease	&&
							entry.Step >=  moduleDBInfo.RecoveryInfo.NrStep)
							continue;
						else
							moduleDBInfo.MissingEntryList.Add(entry);
					}
					else
					{
						// se il numero di release di creazione della procedure è maggiore del numero di release 
						// del modulo (indicato nel tag <Release> del DatabaseObjects.xml) segnalo un 
						// messaggio di errore di incongruenza 
						if (entry.Rel > moduleDBInfo.DBRelease)
							diagnostic.Set
								(DiagnosticType.Error | DiagnosticType.LogOnFile, 
								string.Format
								(
								DatabaseLayerStrings.ErrCheckMissingObject,
								entry.Name,
								moduleDBInfo.Title,
								moduleDBInfo.ApplicationBrand,
								entry.Rel,
								moduleDBInfo.UpdateInfo.DbMarkRel							
								));
						continue;
					}

					found = true;
				}
			}

			return found;
		}

		/// <summary>
		/// l'array delle tabelle mancanti viene riordinato per numero di step crescente e poi 
		/// richiamo le funzioni che vanno a scorrere i singoli nodi, per cercare gli script
		/// </summary>
		//---------------------------------------------------------------------------
		private void MissingTablesManagement(ModuleDBInfo moduleDBInfo)
		{
			// faccio il sort (per numero di step crescente) solo se la lista contiene almeno 2 elementi
			if (moduleDBInfo.MissingEntryList.Count > 1)
			{
				IComparer<EntryDBInfo> entryComparer = new CustomSortMissingEntryList();
				moduleDBInfo.MissingEntryList.Sort(entryComparer);
			}

			FindScriptForMissingTables(moduleDBInfo);
		}

		/// <summary>
		/// per ogni entry presente nell'array list delle tabelle mancanti vado a cercare con
		/// il path finder il path del file CreateInfo.xml dove sono presenti le informazioni.
		/// </summary>
		//---------------------------------------------------------------------------
		private void FindScriptForMissingTables(ModuleDBInfo moduleDBInfo)
		{
			int breakRel = 0;

			moduleDBInfo.UpdateInfo.ModGraphLevel1 = GraphLevel1;
			moduleDBInfo.UpdateInfo.ModGraphLevel2 = GraphLevel2;
			moduleDBInfo.UpdateInfo.ModGraphLevel3 = GraphLevel3;
			moduleDBInfo.UpdateInfo.UpdateInfoList = new List<SingleUpdateInfo>();
			
			foreach (EntryDBInfo entry in moduleDBInfo.MissingEntryList)
			{
				diagnostic.Set
					(DiagnosticType.Warning | DiagnosticType.LogOnFile, 
					string.Format
					(
					DatabaseLayerStrings.MsgMissingObjectToCreate, 
					entry.Name, 
					moduleDBInfo.Title,
					moduleDBInfo.ApplicationBrand)
					);

				moduleDBInfo.XmlPath = context.PathFinder.GetStandardCreateInfoXML(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);

				if (string.IsNullOrWhiteSpace(moduleDBInfo.XmlPath))
					return;

				// se il CurrSingleUpdate è uguale a null, oppure manca un elemento associato al nr. 
				// di release che stiamo esaminando, creo un nuovo elemento di tipo SingleUpdateInfo.
				if (	
					moduleDBInfo.UpdateInfo.CurrSingleUpdate == null || 
					moduleDBInfo.UpdateInfo.CurrSingleUpdate.DBRel != entry.Rel
					)
					moduleDBInfo.UpdateInfo.CurrSingleUpdate = new SingleUpdateInfo();

				FileInfo fi = new FileInfo(moduleDBInfo.XmlPath);
				if (!fi.Exists)
				{
					diagnostic.Set
						(DiagnosticType.Warning | DiagnosticType.LogOnFile, 
						string.Format(DatabaseLayerStrings.ErrCreateInfoFileForObjNotExist, entry.Name, moduleDBInfo.Title, moduleDBInfo.ApplicationBrand));
					continue;
				}
				moduleDBInfo.DirectoryScript = fi.Directory.FullName;
				
				string error = string.Empty;
				moduleDBInfo.UpdateInfo.LoadSingleXML(this, moduleDBInfo.XmlPath, out error);
				if (!string.IsNullOrWhiteSpace(error))
				{
					diagnostic.Set(DiagnosticType.Warning | DiagnosticType.LogOnFile, error);
					continue;
				}
				
				// richiamo il parsing per quel specifico file xml contenente le informazioni per 
				// rintracciare lo specifico script che mi serve per ricreare la tabella mancante (o la colonna mancante)
				moduleDBInfo.UpdateInfo.ParseSingleXML
					(	
					entry, 
					null,
					true, 
					moduleDBInfo.DirectoryScript, 
					context.PathFinder,
					(kindOfDb == KindOfDatabase.Dms) ? NameSolverDatabaseStrings.SQLOLEDBProvider : context.Provider,
					appStructInfo.ApplicationDBInfoList,
					out error
					);

				if (!string.IsNullOrWhiteSpace(error))
				{
					diagnostic.Set(DiagnosticType.Warning | DiagnosticType.LogOnFile, error);
					continue;
				}

				if (moduleDBInfo.UpdateInfo.CurrSingleUpdate.DBRel != breakRel)
				{
					if (
						moduleDBInfo.UpdateInfo.CurrSingleUpdate.ScriptLevel1List.Count > 0 ||
						moduleDBInfo.UpdateInfo.CurrSingleUpdate.ScriptLevel2List.Count > 0 ||
						moduleDBInfo.UpdateInfo.CurrSingleUpdate.ScriptLevel3List.Count > 0 
						)
						moduleDBInfo.UpdateInfo.UpdateInfoList.Add(moduleDBInfo.UpdateInfo.CurrSingleUpdate);
					else
						moduleDBInfo.UpdateInfo.UpdateInfoList.Remove(moduleDBInfo.UpdateInfo.CurrSingleUpdate);

					breakRel = moduleDBInfo.UpdateInfo.CurrSingleUpdate.DBRel;
				}

				//***** GESTIONE ADDONCOLUMNS *****\\
				bool createCol = false;

				foreach (AdditionalColumnsInfo col in entry.AddColumnsList)
				{
					diagnostic.Set(DiagnosticType.Warning | DiagnosticType.LogOnFile, string.Format(DatabaseLayerStrings.ErrAdditionalColumnIsMissing, col.TableName, moduleDBInfo.Title));

					// qui non mi piace il fatto che cambio il path del modulo globale, anche se sto valutando gli addoncols che si trovano
					// in altro modulo!
					moduleDBInfo.XmlPath = context.PathFinder.GetStandardCreateInfoXML(col.AppName, col.ModName);
					createCol = true;

					/*if (col.NumRelease == 1)
					{
						moduleDBInfo.XmlPath = context.PathFinder.GetStandardCreateInfoXML(col.AppName, col.ModName);
						createCol = true;
					}

					if (col.NumRelease > 1)
					{
						moduleDBInfo.XmlPath = context.PathFinder.GetStandardUpgradeInfoXML(col.AppName, col.ModName);
						createCol = false;
					}*/
				
					if (string.IsNullOrWhiteSpace(moduleDBInfo.XmlPath))
						return;

					bool isNewSingleUpdateInfo = false;
					// se il CurrSingleUpdate è uguale a null, oppure manca un elemento associato al nr. 
					// di release che stiamo esaminando, creo un nuovo elemento di tipo SingleUpdateInfo.
					if
						(
						moduleDBInfo.UpdateInfo.CurrSingleUpdate == null ||
						moduleDBInfo.UpdateInfo.CurrSingleUpdate.DBRel != col.NumRelease
						)
					{
						isNewSingleUpdateInfo = true;
						moduleDBInfo.UpdateInfo.CurrSingleUpdate = new SingleUpdateInfo();
					}

					FileInfo fifo = new FileInfo(moduleDBInfo.XmlPath);
					if (!fifo.Exists)
						continue;
					moduleDBInfo.DirectoryScript = fifo.Directory.FullName;
				
					moduleDBInfo.UpdateInfo.LoadSingleXML(this, moduleDBInfo.XmlPath, out error);
					if (!string.IsNullOrWhiteSpace(error))
					{
						diagnostic.Set(DiagnosticType.Warning | DiagnosticType.LogOnFile, error);
						continue;
					}
				
					// richiamo il parsing per quel specifico file xml contenente le informazioni per 
					// rintracciare lo specifico script che mi serve per ricreare la colonna mancante.
					moduleDBInfo.UpdateInfo.ParseSingleXML
						(	
						null,
						col, 
						createCol, 
						moduleDBInfo.DirectoryScript, 
						context.PathFinder,
						(kindOfDb == KindOfDatabase.Dms) ? NameSolverDatabaseStrings.SQLOLEDBProvider : context.Provider,
						appStructInfo.ApplicationDBInfoList,
						out error
						);

					if (!string.IsNullOrWhiteSpace(error))
					{
						diagnostic.Set(DiagnosticType.Error | DiagnosticType.LogOnFile, error);
						continue;
					}

					// aggiungo alla lista da updatare solo se ho appena creato il SingleUpdate (altrimenti mi appoggio sull'esistente)
					if (isNewSingleUpdateInfo)
					{
						if (
							moduleDBInfo.UpdateInfo.CurrSingleUpdate.ScriptLevel1List.Count > 0 ||
							moduleDBInfo.UpdateInfo.CurrSingleUpdate.ScriptLevel2List.Count > 0 ||
							moduleDBInfo.UpdateInfo.CurrSingleUpdate.ScriptLevel3List.Count > 0
							)
							moduleDBInfo.UpdateInfo.UpdateInfoList.Add(moduleDBInfo.UpdateInfo.CurrSingleUpdate);
					}
				}
			}
		}
		# endregion

		/// <summary>
		/// Date la signature di applicazione e modulo, ritorna il relativo oggetto ModuleDBInfo
		/// </summary>
		//---------------------------------------------------------------------------
		private ModuleDBInfo GetModuleItemFromSignature(string appName, string moduleName)
		{
			foreach (AddOnApplicationDBInfo addOnAppDBInfo in appStructInfo.ApplicationDBInfoList)
			{
				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{
					if (string.Compare(moduleDBInfo.ApplicationSign, appName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
						string.Compare(moduleDBInfo.ModuleSign, moduleName, StringComparison.InvariantCultureIgnoreCase) == 0)
						return moduleDBInfo;
				}
			}
			return null;
		}
	}

	# region Re-sort array oggetti mancanti (per nr. di step crescenti)
	/// <summary>
	/// Questa classe si occupa di re-sortare una lista.
	/// Nello specifico caso mi serve per fare il sort dell'array delle tabelle mancanti
	/// (MissingTablesList), ordinandolo per numero di step) crescente.
	/// </summary>
	//============================================================================
	public class CustomSortMissingEntryList : IComparer<EntryDBInfo>
	{
		//---------------------------------------------------------------------------
		public int Compare(EntryDBInfo x, EntryDBInfo y)
		{
			CaseInsensitiveComparer comparer = new CaseInsensitiveComparer(System.Globalization.CultureInfo.InvariantCulture);
			
			// ordino per numero di step crescente
			int i = comparer.Compare(x.Step, y.Step);

			// a parità di numero di step, ordino per numero di release (sempre crescente)
			if (i == 0)
				i = comparer.Compare(x.Rel, y.Rel);

			return i;
		}
	}
	# endregion
}