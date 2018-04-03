using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

using TaskBuilderNetCore.Interfaces;
using Microarea.ProvisioningDatabase.Libraries.DataManagerEngine;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
{
	//=====================================================================
	public class ExecuteManager
	{
		#region Events and Delegates
		public delegate void ElaborationProgressMessage(object sender, bool ok, string script, string step, string detail, string fullPath, ExtendedInfo ei);
		public event ElaborationProgressMessage OnElaborationProgressMessage;

		public delegate void ElaborationProgressBar(object sender);
		public event ElaborationProgressBar OnElaborationProgressBar;

		public delegate void UpdateModuleCounter(object sender);
		public event UpdateModuleCounter OnUpdateModuleCounter;

		public delegate void UpdateMandatoryCols(string table);
		public event UpdateMandatoryCols OnUpdateMandatoryCols;

		public delegate void InsertMessageInListView();
		public event InsertMessageInListView OnInsertMessageInListView;
		#endregion

		#region Variables and Properties
		private ScriptManager scriptMng = null; // x accedere alle funzioni degli script
		private CheckDBStructureInfo dbStructInfo = null;
		private ContextInfo contextInfo = null;
		private Diagnostic diagnostic = null;

		private List<string> tbLevel1List = null;
		private List<string> tbLevel2List = null;
		private List<string> tbLevel3List = null;

		// array di appoggio (contenente un elenco di script) valorizzato a seconda del livello
		private List<string> scriptList = null;

		private BaseImportExportManager impExpManager = null;
		private bool importDefaultData = true;
		private bool importSampleData = false;

		private int numLevel = 0;

		private bool forMissingTables = false;
		private bool isRecoveryAction = false;

		// per l'aggiornamento della struttura delle tabelle dell'Auditing
		private List<string> auditTables = null; // array di tabelle AU_
		private List<AuditTableStructureInfo> auditTablesInfo = null; // array di appoggio di oggetti di tipo AuditTableStructureInfo

		// per la gestione dei triggers
		private List<TriggerInfo> triggersList = null; // lista di nomi triggers

		public bool ErrorInRunSqlScript = false;

		// per tenere traccia della diagnostica dello ScriptManager
		public Diagnostic ScriptMngDiagnostic = null;

		//---------------------------------------------------------------------------
		public CheckDBStructureInfo DbStructInfo { get { return dbStructInfo; } set { dbStructInfo = value; } }
		#endregion

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------------
		public ExecuteManager
			(
			CheckDBStructureInfo dbStructInfo,
			BaseImportExportManager impExpMng,
			bool importDefault, // import dati di default
			bool importSample   // import dati di esempio
			)
		{
			this.dbStructInfo = dbStructInfo;
			this.contextInfo = dbStructInfo.ContextInfo;
			this.impExpManager = impExpMng;
			this.importDefaultData = importDefault;
			this.importSampleData = importSample;
		}

		#region Execute (entry-point per la gestione database)
		/// <summary>
		/// Entry-point per la gestione database (dopo che l'utente ha dato ok alle operazioni)
		/// In base allo stato del db richiamo le apposite funzioni e i metodi esposti dalla classe CheckDBStructureInfo.
		/// </summary>
		//---------------------------------------------------------------------------
		public bool Execute(Diagnostic dbManagerDiagnostic)
		{
			// DB COMPLETO----------------------------------------------------------------
			if (dbStructInfo.DBStatus == DatabaseStatus.NOT_EMPTY)
			{
				if (dbStructInfo.KindOfDb == KindOfDatabase.Company)
					contextInfo.SetUpdatingFlag(false);
				return true;
			}

			// GESTIONE DATABASE UNRECOVERABLE----------------------------------------------------------------
			if (dbStructInfo.DBStatus == DatabaseStatus.UNRECOVERABLE)
			{
				ErrorInRunSqlScript = true;
				return false;
			}

			if (contextInfo.Connection != null && contextInfo.Connection.State == ConnectionState.Closed)
			{
				switch (dbStructInfo.KindOfDb)
				{
					case KindOfDatabase.Company:
						{
							contextInfo.Connection.ConnectionString = contextInfo.ConnectAzDB;
							contextInfo.Connection.Open();
							break;
						}
					case KindOfDatabase.Dms:
						{
							contextInfo.Connection.ConnectionString = contextInfo.ConnectAzDB;
							contextInfo.DmsConnection.ConnectionString = contextInfo.ConnectDmsDB;
							contextInfo.DmsConnection.Open();
							contextInfo.Connection = contextInfo.DmsConnection; // inguardabile, ma devo fare cosi' per tirare su il puntatore giusto
							break;
						}
					case KindOfDatabase.System:
						{
							contextInfo.Connection.ConnectionString = contextInfo.ConnectSysDB;
							contextInfo.Connection.Open();
							break;
						}
				}
			}

			this.diagnostic = dbManagerDiagnostic;

			bool success = false;

			// istanzio l'oggetto ScriptManager che mi consente di eseguire gli statement contenuti nello script
			scriptMng = new ScriptManager
				(
				contextInfo.UseUnicode,
				dbStructInfo.KindOfDb,
				contextInfo.DatabaseCulture,
				dbStructInfo.DBStatus == DatabaseStatus.EMPTY,
				this.diagnostic
				);
			scriptMng.Connection = contextInfo.Connection;

			// prima di avviare qualunque operazione sul database aziendale, imposto il flag Updating a true.
			if (dbStructInfo.KindOfDb == KindOfDatabase.Company)
			{
				contextInfo.SetUpdatingFlag(true);
				// imposto sul database il parametro ArithAbort a true (per creare le view con indici)
				contextInfo.SetArithAbortDbOption(true);

				// carico le info dei triggers (solo una volta perche' mi servono anche dopo)
				LoadTriggersInfo();
				// e poi disabilito i triggers (se trovati)
				ManageTriggers(false);
			}

			bool isDatabaseCreated = false;
			bool isDatabaseUpgraded = false;

			try
			{
				DateTime start = DateTime.Now; 
				Debug.WriteLine(string.Format("MP-LOG - Start database {0} structure creation: ", contextInfo.Connection.Database) + start.ToString("hh:mm:ss.fff"));

				// GESTIONE DATABASE EMPTY----------------------------------------------------------------------------
				if (dbStructInfo.DBStatus == DatabaseStatus.EMPTY)
				{
					success = CreateDatabase(false);
					if (success)
						isDatabaseCreated = true;
				}

				// PRE-AGGIORNAMENTO RIGHE DBMARK PER GESTIONE PREVIOUS SIGNATURE (SOLO DB AZIENDALE) ------------
				if (dbStructInfo.KindOfDb == KindOfDatabase.Company)
				{
					string msgDetail = string.Empty;
					if ((dbStructInfo.DBStatus & DatabaseStatus.NEED_UPDATE_DBMARK_INFO) == DatabaseStatus.NEED_UPDATE_DBMARK_INFO)
					{
						success = dbStructInfo.DBMarkInfo.DBMarkTable.UpdatePreviousSignature(dbStructInfo.PreviousUpdateList, out msgDetail);

						string msg = string.Format((success ? DatabaseManagerStrings.PreUpdateDBMarkWithSuccess : DatabaseManagerStrings.PreUpdateDBMarkWithErrors), dbStructInfo.DBMarkInfo.DBMarkTable.TableName);
						OnElaborationProgressMessage?.Invoke(null, success, string.Format(DatabaseManagerStrings.PreUpdateDBMark, dbStructInfo.DBMarkInfo.DBMarkTable.TableName), "1", msg, msgDetail, null);
						diagnostic.Set(DiagnosticType.LogOnFile, msg + msgDetail);

						if (!success)
							return success;
					}
				}

				// DB CHE DEVE FARE RECOVERY E UPGRADE----------------------------------------------------------------
				if (((dbStructInfo.DBStatus & DatabaseStatus.NEED_RECOVERY) == DatabaseStatus.NEED_RECOVERY) &&
				((dbStructInfo.DBStatus & DatabaseStatus.NEED_UPGRADE) == DatabaseStatus.NEED_UPGRADE))
				{
					// prima eseguo il recovery dei dati
					success = RecoveryData();

					// se è andato a buon fine 
					if (success)
					{
						success = ManageDatabaseUpgrade();
						if (!success)
							return success;
						else
							isDatabaseUpgraded = true;
					}
				}

				// DB CHE DEVE FARE SOLO RECOVERY----------------------------------------------------------------
				if (((dbStructInfo.DBStatus & DatabaseStatus.NEED_RECOVERY) == DatabaseStatus.NEED_RECOVERY) &&
				((dbStructInfo.DBStatus & DatabaseStatus.NEED_UPGRADE) != DatabaseStatus.NEED_UPGRADE))
				{
					success = RecoveryData();
				}

				// DB CHE DEVE FARE SOLO UPGRADE----------------------------------------------------------------
				if (((dbStructInfo.DBStatus & DatabaseStatus.NEED_UPGRADE) == DatabaseStatus.NEED_UPGRADE) &&
				((dbStructInfo.DBStatus & DatabaseStatus.NEED_RECOVERY) != DatabaseStatus.NEED_RECOVERY))
				{
					success = ManageDatabaseUpgrade();
					if (!success)
						return success;
					else
						isDatabaseUpgraded = true;
				}

				DateTime end = DateTime.Now;
				Debug.WriteLine("MP-LOG - End database structure creation: " + end.ToString("hh:mm:ss.fff"));
				TimeSpan ts1 = end - start;
				Debug.WriteLine("MP-LOG - Database structure creation - total seconds: " + ts1.TotalSeconds.ToString());

				if (dbStructInfo.KindOfDb == KindOfDatabase.Company)
				{
					start = DateTime.Now;
					Debug.WriteLine("MP-LOG - Start mandatory columns creation: " + start.ToString("hh:mm:ss.fff"));
					// check delle colonne TBCreated e TBModified
					// @@TODO: TOGLIERE IL COMMENTO!!!
					CheckMandatoryColumns();
					end = DateTime.Now;
					Debug.WriteLine("MP-LOG - End mandatory columns creation: " + end.ToString("hh:mm:ss.fff"));
					TimeSpan ts = end - start;
					Debug.WriteLine("MP-LOG - Mandatory columns creation - total seconds: " + ts.TotalSeconds.ToString());

					start = DateTime.Now;
					Debug.WriteLine("MP-LOG - Start TBGuid column creation: " + start.ToString("hh:mm:ss.fff"));
					// check colonna TBGuid per le master tables
					// @@TODO: TOGLIERE IL COMMENTO!!!
					CheckTBGuidColumn();
					end = DateTime.Now;
					Debug.WriteLine("MP-LOG - End TBGuid column creation: " + end.ToString("hh:mm:ss.fff"));
					ts = end - start;
					Debug.WriteLine("MP-LOG - TBGuid column creation - total seconds: " + ts.TotalSeconds.ToString());

					// check delle colonne obbligatorie per le master tables del RowLevelSecurity
					CheckRSMasterTablesColumns();

					//*****************************************************************************************
					// anche se l'azienda non è sottoposta a tracciatura devo aggiornare le tabelle di Auditing
					// per contemplare il caso in cui venga attachato un db esistente dimenticandoci di mettere
					// il flag usa auditing

					// in questo metodo gestiamo gli scatti di release del modulo TBAuditing:
					// a) fix anomalia 13679 (su Mago.Net 2.8): lo scatto di release 2 prevede l'aggiornamento manuale della PK di tutte le 
					// tabelle sottoposte a tracciatura, aggiungendo un segmento di tipo contatore alla chiave primaria
					// b) fix anomalia 16538 (su Mago.Net 3.0): lo scatto di release 3 prevede l'aggiornamento manuale della PK di tutte le 
					// tabelle sottoposte a tracciatura, aggiungendo un segmento di tipo long alla chiave primaria
					// al posto della colonna di tipo Identity
					UpgradeAuditingTables();

					// questo metodo deve sempre essere eseguito se l'azienda è sottoposta a tracciatura
					// serve per riallineare la struttura delle tabelle sottoposte a tracciatura dopo gli aggiornamenti
					// del database di ERP (eliminazione colonne, cambio dimensione o tipo e modifica segmenti PK)
					// (fix anomalia 15535 su Mago.Net 2.10)
					AdjustAuditingTablesStructure();
					//*****************************************************************************************

					// richiamo il metodo di refresh delle view, utilizzando la sp di sistema sp_refreshview
					RefreshViews();

					// gestione caricamento dati di default/esempio contestuale alla creazione/aggiornamento del database
					if (isDatabaseCreated)
					{
						// importazione dati di default
						if (importDefaultData)
						{
							start = DateTime.Now;
							Debug.WriteLine("MP-LOG - Start import default data: " + start.ToString("hh:mm:ss.fff"));

							if (!impExpManager.ImportDefaultDataSilentMode() && OnInsertMessageInListView != null)
								OnInsertMessageInListView();

							end = DateTime.Now;
							Debug.WriteLine("MP-LOG - End import default data: " + end.ToString("hh:mm:ss.fff"));
							ts = end - start;
							Debug.WriteLine("MP-LOG - Import default data - total seconds: " + ts.TotalSeconds.ToString());
						}
						// importazione dati di esempio
						if (importSampleData)
							impExpManager.ImportSampleDataSilentMode();
					}

					if (isDatabaseUpgraded)
					{
						// gestione dei dati di default in fase di upgrade
						impExpManager.ImportDefaultDataForUpgrade();

						// gestione standard dei dati di default/esempio
						bool existDataToImport = false;
						if (importDefaultData)
							existDataToImport = impExpManager.ImportDefaultDataSilentMode();
						if (importSampleData)
							existDataToImport = impExpManager.ImportSampleDataSilentMode();

						// ulteriore gestione dei file di Append, che altrimenti non vengono caricati 
						if (dbStructInfo.MissingTablesListForAppend.Count > 0)
						{
							// qui devo fare un giro sulle applicazioni e cercare il file di Append
							foreach (AddOnApplicationDBInfo addOn in dbStructInfo.AddOnAppList)
								foreach (ModuleDBInfo module in addOn.ModuleList)
								{
									if (importDefaultData)
										impExpManager.AddAppendDefaultDataTable(addOn.ApplicationName, module.ModuleName);
									if (importSampleData)
										impExpManager.AddAppendSampleDataTable(addOn.ApplicationName, module.ModuleName);
								}

							if (importDefaultData)
								existDataToImport = impExpManager.ImportAppendDefaultData(dbStructInfo.MissingTablesListForAppend);
							if (importSampleData)
								existDataToImport = impExpManager.ImportAppendSampleData(dbStructInfo.MissingTablesListForAppend);
						}

						// se nessuna importazione dati è stata effettuata allora visualizzo la stringa "No default data exist..."
						if (importDefaultData && !existDataToImport && OnInsertMessageInListView != null)
							OnInsertMessageInListView();
					}
					//
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("TaskBuilderNet.Data.DatabaseLayer.ExecuteManager.Execute() " + e.Message);
			}
			finally
			{
				// terminate le operazioni sul database aziendale, devo rimettere il flag Updating a false.
				if (dbStructInfo.KindOfDb == KindOfDatabase.Company)
				{
					contextInfo.SetUpdatingFlag(false);
					// imposto sul database il parametro ArithAbort a false
					contextInfo.SetArithAbortDbOption(false);
					// ri-abilito i triggers
					ManageTriggers(true);
				}

				// chiudo la connessione aperta sul database dallo ScriptManager
				if (scriptMng.Connection.State == ConnectionState.Open || scriptMng.Connection.State == ConnectionState.Broken)
				{
					scriptMng.Connection.Close();
					scriptMng.Connection.Dispose();
				}

				// chiudo la connessione aperta sul database 
				if (contextInfo.DbType != DBMSType.POSTGRE)
					contextInfo.CloseConnection();
			}

			// assegno la diagnostica dello ScriptManager
			ScriptMngDiagnostic = scriptMng.Diagnostic;

			return success;
		}
		#endregion

		#region Creazione delle tabelle del database (per tutti i moduli)
		/// <summary>
		/// Creazione del database
		/// </summary>
		/// <param name="forMissingModules">true: si considera l'array dei soli moduli mancanti - false: array di tutti i moduli da creare</param>
		/// <returns>successo dell'operazione</returns>
		/// <remarks>questo metodo viene chiamato sempre con false, quindi solo in creazione del database da zero</remarks>
		//---------------------------------------------------------------------------
		private bool CreateDatabase(bool forMissingModules)
		{
			dbStructInfo.GraphLevel1 = new DirectGraph();
			dbStructInfo.GraphLevel2 = new DirectGraph();
			dbStructInfo.GraphLevel3 = new DirectGraph();

			string error = string.Empty;
			bool ok = true;

			// per ogni modulo inserito nella lista da updatare faccio il parse del relativo file 
			// CreateInfo.xml (se esiste) e inserisco i vari nodi nel grafo (Level1 e Level2)
			foreach (ModuleDBInfo moduleDBInfo in (forMissingModules) ? dbStructInfo.MissingModulesList : dbStructInfo.ModuleToCreateList)
				if (moduleDBInfo.StatusOk)
				{
					moduleDBInfo.UpdateInfo.Parse(dbStructInfo, moduleDBInfo.XmlPath, true, out error, false);

					if (!string.IsNullOrWhiteSpace(error))
					{
						OnElaborationProgressMessage?.Invoke
								(
								moduleDBInfo,
								false,
								moduleDBInfo.DirectoryScript,
								string.Empty,
								error,
								moduleDBInfo.DirectoryScript,
								null
								);
						moduleDBInfo.StatusOk = false;
						continue;
					}

					if (dbStructInfo.KindOfDb == KindOfDatabase.Company)
					{
						if (impExpManager != null && importDefaultData)
							impExpManager.AddDefaultDataTable(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);

						if (impExpManager != null && importSampleData)
							impExpManager.AddSampleDataTable(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);
					}
				}

			// richiamo il Topological Sorting per entrambi i grafi
			dbStructInfo.GraphLevel1.Topo();
			dbStructInfo.GraphLevel2.Topo();
			dbStructInfo.GraphLevel3.Topo();

			tbLevel1List = new List<string>();
			tbLevel2List = new List<string>();
			tbLevel3List = new List<string>();

			if (dbStructInfo.KindOfDb != KindOfDatabase.System)
			{
				// riordinamento degli array x caricare prima i moduli di TB
				SortArrays(dbStructInfo.GraphLevel1, tbLevel1List);
				SortArrays(dbStructInfo.GraphLevel2, tbLevel2List);
				SortArrays(dbStructInfo.GraphLevel3, tbLevel3List);
			}

			ErrorInRunSqlScript = false;
			// ricerca ed esecuzione degli script SQL 
			if (!ManageSQLScript(tbLevel1List, dbStructInfo.GraphLevel1, (forMissingModules) ? dbStructInfo.MissingModulesList : dbStructInfo.ModuleToCreateList, 1, false) ||
				!ManageSQLScript(tbLevel2List, dbStructInfo.GraphLevel2, (forMissingModules) ? dbStructInfo.MissingModulesList : dbStructInfo.ModuleToCreateList, 2, false) ||
				!ManageSQLScript(tbLevel3List, dbStructInfo.GraphLevel3, (forMissingModules) ? dbStructInfo.MissingModulesList : dbStructInfo.ModuleToCreateList, 3, false))
				ok = false;

			string msgError = string.Empty;
			// solo se esiste sul database la tabella DBMark procedo a fare le insert
			if (dbStructInfo.DBMarkInfo.DBMarkTable.Exists())
				dbStructInfo.DBMarkInfo.InsertInDBMark
					(
					(forMissingModules) ? dbStructInfo.MissingModulesList : dbStructInfo.ModuleToCreateList,
					out msgError,
					false
					); // inserisco le righe nella tabella TB_DBMark

			if (!string.IsNullOrWhiteSpace(msgError))
			{
				OnElaborationProgressMessage?.Invoke
					(
					null,
					false,
					string.Format(DatabaseManagerStrings.ErrInsertingTableDBMark, dbStructInfo.DBMarkInfo.DBMarkTable.TableName),
					string.Empty,
					msgError,
					string.Empty,
					null
					);
				ErrorInRunSqlScript = true;
			}
			return ok;
		}
		#endregion

		#region SortArrays
		/// <summary>
		/// funzione (richiamata sia per il level1 che per il level2) 
		/// per re-sortare gli array generati dal topological sorting dell'algoritmo del grafo. 
		/// Nel sorted array individuo gli elementi di TB, li aggiungo in un arraylist 
		/// di supporto e li elimino da quello originario.
		/// Questo per avere i moduli di TB per primi.
		/// </summary>
		/// <param name="graph">oggetto di tipo graph</param>
		/// <param name="tbLevList">arraylist di supporto per i moduli di TB</param>
		//---------------------------------------------------------------------------
		private void SortArrays(DirectGraph graph, List<string> tbLevList)
		{
			for (int i = graph.SortedArray.Count - 1; i >= 0; i--)
			{
				string[] p = ((string)graph.SortedArray[i]).Split(new Char[] { '.' });
				string appName = p[0].ToString();

				if (string.Compare(appName, DatabaseLayerConsts.TBModuleName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					tbLevList.Add(graph.SortedArray[i] as string);
					graph.SortedArray.RemoveAt(i);
				}
			}
		}
		#endregion

		#region Analisi delle strutture dati contenenti gli script ed lancio dell'esecuzione degli stessi
		/// <summary>
		/// </summary>
		/// <param name="tbLevList">array dei soli moduli di TB</param>
		/// <param name="graph">oggetto di tipo graph</param>
		/// <param name="moduleToUpdateList">array del totale dei moduli da updatare</param>
		/// <param name="forLevel1">true: analizzo il level1; false: analizzo il level2</param>
		/// <param name="missingTables">true: creo le tabelle mancanti; false: creo tutte le tabelle</param>
		//---------------------------------------------------------------------------
		private bool ManageSQLScript
			(
			List<string> tbLevList,
			DirectGraph graph,
			ModuleDBInfoList moduleToUpdateList,
			int level,
			bool missingTables
			)
		{
			numLevel = level;
			forMissingTables = missingTables;
			bool ok = false;

			try
			{
				// prima analizzo l'array che contiene i moduli di TB e li eseguo
				// se l'esecuzione è andata a buon fine e la tabella DBMark é stata creata allora
				// passo all'analisi (ed alla creazione) dei moduli delle altre applicazioni
				if (dbStructInfo.KindOfDb != KindOfDatabase.System)
				{
					if (AnalyzeSQLScript(tbLevList, graph, moduleToUpdateList) && dbStructInfo.DBMarkInfo.DBMarkTable.Exists())
						ok = AnalyzeSQLScript(graph.SortedArray.Cast<string>().ToList(), graph, moduleToUpdateList);
				}
				else
					ok = AnalyzeSQLScript(graph.SortedArray.Cast<string>().ToList(), graph, moduleToUpdateList);
			}
			catch (Exception)
			{
				ok = false;
				//DiagnosticViewer.ShowErrorTrace(e.Message, e.Source, string.Empty);
			}

			return ok;
		}

		/// <summary>
		/// questa funzione viene richiamata prima con l'array dei moduli di TB e poi il 
		/// sorted array del grafo (che contiene l'elenco delle altre AddOnApp). 
		/// </summary>
		/// <param name="listModule">array di moduli da creare</param>
		/// <param name="graph">oggetto di tipo Graph</param>
		/// <param name="moduleToUpdateList">array di oggetti di tipo ModuleDBInfoList</param>
		/// <returns>successo dell'operazione</returns>
		//---------------------------------------------------------------------------
		private bool AnalyzeSQLScript(List<string> listModule, DirectGraph graph, ModuleDBInfoList moduleToUpdateList)
		{
			bool ok = true;
			ModuleDBInfo dbInfo, dbInfoChild = null;
			DependencyInfo depInfo = null;

			for (int i = 0; i < listModule.Count; i++)
			{
				// ritorna il corrispondente oggetto di tipo ModuleDBInfo relativo al modulo analizzato
				dbInfo = moduleToUpdateList.GetItem(listModule[i].ToString(), false);

				// se il dbInfo è null significa che non ho trovato nell'array dei moduledbinfo 
				// l'entry che mi interessa, devo quindi controllare nella tabella DBMark 
				// se esiste un entry con quel numero di release specificato.
				bool exist = false;
				bool statusInDBMark = false;
				string application, module = string.Empty;
				int release = 0;

				if (dbInfo == null)
				{
					exist = dbStructInfo.DBMarkInfo.DBMarkTable.CheckEntryInDBMark
						(
						listModule[i].ToString(),
						out application,
						out module,
						out release,
						out statusInDBMark
						);

					dbInfo = new ModuleDBInfo(listModule[i].ToString());

					// se l'entry esiste nella tabella, allora lo inserisco nell'array
					// moduli da updatare e valorizzo il suo status esistente
					if (exist)
					{
						dbInfo.StatusOk = statusInDBMark;
						dbInfo.EntryOnlyInDBMark = true;
						dbInfo.ApplicationSign = application;
						dbInfo.ModuleSign = module;
						dbInfo.DBRelease = release;

						dbInfo.CreateUpdateInfoInstance();
						dbInfo.UpdateInfo.CurrRel = release;

						moduleToUpdateList.Add(dbInfo);
					}
					else // e se non lo trovo? devo cmq tenerne traccia! e impostare status = false!
					{
						dbInfo.StatusOk = false;
						dbInfo.EntryOnlyInDBMark = true;
						dbInfo.ApplicationSign = application;
						dbInfo.ModuleSign = module;
						dbInfo.DBRelease = release;

						dbInfo.CreateUpdateInfoInstance();
						dbInfo.UpdateInfo.CurrRel = release;

						moduleToUpdateList.Add(dbInfo);
					}
				}
				else
				{
					OnUpdateModuleCounter?.Invoke(dbInfo);
					ok = dbInfo.StatusOk;
				}

				// ritorna il corrispondente oggetto di tipo DependencyInfo relativo al modulo analizzato
				depInfo = graph.DependencyList.GetItem(listModule[i].ToString());

				if (depInfo == null)
					continue;

				// analizzo lo stato di tutti i moduli da cui dipende quello che sta analizzando
				for (int k = 0; k < depInfo.ChildList.Count; k++)
				{
					dbInfoChild = moduleToUpdateList.GetItem(depInfo.ChildList[k].ToString(), true);
					if (dbInfoChild != null)
						ok = ok && dbInfoChild.StatusOk;
				}

				// se l'entry nella DBMark non esiste non vado avanti nell'esecuzione degli altri passaggi
				if (!exist)
				{
					// se il modulo non dipende da nessuno runno subito gli script
					if (depInfo.ChildList.Count == 0 && !dbInfo.EntryOnlyInDBMark)
						dbInfo.StatusOk = ok = ok && RunSQLScript(dbInfo);
					else
					{
						// se ok è a false significa che qualcuno dei moduli da cui dipende
						// quello analizzato ha fallito l'elaborazione
						if (ok && !dbInfo.EntryOnlyInDBMark)
							dbInfo.StatusOk = ok = ok && RunSQLScript(dbInfo);
						else
						{
							dbInfo.StatusOk = ok = false;
							// se il NrRelease attuale è uguale a zero significa che non ho provato neppure
							// ad eseguire gli script di quel modulo (xchè dipendeva da moduli già falliti)
							// xciò lo inizializzo con l'attuale release della DBMark (se è diversa da 0)
							if (dbInfo.NrRelease == 0)
								dbInfo.NrRelease = (dbInfo.UpdateInfo.DbMarkRel == 0) ? 1 : dbInfo.UpdateInfo.DbMarkRel;

							OnElaborationProgressMessage?.Invoke
									(
									dbInfo,
									false,
									dbInfo.DirectoryScript,
									dbInfo.NrStep.ToString(),
									string.Format(DatabaseManagerStrings.ErrCreatingModule, dbInfo.Title),
									dbInfo.DirectoryScript,
									null
									);
							// bisognerebbe indicare che l'errore è dovuto alla dipendenza tra i moduli!!!

							diagnostic.Set(DiagnosticType.Error | DiagnosticType.LogOnFile, string.Concat(string.Format(DatabaseManagerStrings.ErrCreatingModule, dbInfo.Title), DatabaseManagerStrings.ErrCreatingDependencyModule));
						}
					}

					OnElaborationProgressBar?.Invoke(dbInfo);
				}
			}

			return ok;
		}
		#endregion

		#region RunSQLScript (esecuzione degli script attraverso lo ScriptManager)
		/// <summary>
		/// per ogni modulo vado ad eseguire gli script di creazione delle tabelle,
		/// componendo il loro path
		/// </summary>
		/// <param name="moduleDBInfo">oggetto di tipo moduleDBInfo</param>
		/// <returns>successo della funzione</returns>
		//---------------------------------------------------------------------------
		private bool RunSQLScript(ModuleDBInfo moduleDBInfo)
		{
			scriptList = new List<string>();
			string scriptLev, error = string.Empty;

			foreach (SingleUpdateInfo singleInfo in
					(isRecoveryAction) ? moduleDBInfo.RecoveryInfo.UpdateInfoList : moduleDBInfo.UpdateInfo.UpdateInfoList)
			{
				// nel caso di creazione di tabella mancante con rel. di creazione > 1 e annessa procedura
				// di ripristino esegue gli script 2 volte... 
				if (!forMissingTables)
					if (singleInfo.DBRel != moduleDBInfo.UpdateInfo.CurrRel)
						if (!moduleDBInfo.IsNew) // se un modulo viene creato da zero non devo fare lo skip!
							continue;

				// se sto creando una tabella mancante devo eseguire contestualmente anche tutti gli 
				// script degli eventuali addonfields, indipendentemente dal numero di livello
				if (forMissingTables)
				{
					scriptList = new List<string>();
					scriptList.AddRange(singleInfo.ScriptLevel1List);
					scriptList.AddRange(singleInfo.ScriptLevel2List);
					scriptList.AddRange(singleInfo.ScriptLevel3List);
				}
				else
				{
					// carico la lista di script a seconda del livello da considerare
					switch (numLevel)
					{
						case 1:
							scriptList = singleInfo.ScriptLevel1List;
							break;
						case 2:
							scriptList = singleInfo.ScriptLevel2List;
							break;
						case 3:
							scriptList = singleInfo.ScriptLevel3List;
							break;
						default:
							break;
					}
				}

				// scorro la lista degli script
				foreach (string script in scriptList)
				{
					// estrapolo prima il nome dello script e poi il numero dello step
					// metto come carattere separatore (-) per non confonderlo con il separatore del namespace
					string[] str = script.Split(new Char[] { ';' });
					string sql, step = string.Empty;
					sql = str[0].ToString();
					step = str[1].ToString();

					// se sto creando le tabelle mancanti non è necessario "comporre" a mano
					// il path completo degli script sql, in quanto è già presente nell'array di script
					if (forMissingTables)
					{
						scriptLev = sql;
						// estrapolo dall'intera stringa il solo nome dello script, x visualizzarlo
						string[] s = scriptLev.Split(new Char[] { Path.DirectorySeparatorChar });
						sql = s[s.Length - 1].ToString();
					}
					else
					{
						scriptLev = contextInfo.PathFinder.GetStandardScriptPath
							(
							moduleDBInfo.DirectoryScript,
							sql,
							(dbStructInfo.KindOfDb == KindOfDatabase.Dms) ? NameSolverDatabaseStrings.SQLOLEDBProvider : contextInfo.Provider,
							(isRecoveryAction) ? moduleDBInfo.RecoveryInfo.ParseForCreate : moduleDBInfo.UpdateInfo.ParseForCreate,
							singleInfo.DBRel
							);

						// MICHI: non modifico il codice del PathFinder perchè usato da altri
						// questo è un errore che capita in un caso estremo (ovvero recovery + oggetto mancante), 
						// pertanto può andare bene metterlo a posto in questo punto.
						// L'errore è nella composizione del path dello script: se il path contiene sia il
						// il token di Create e quello di Release_ allora sostituisco il Create con l'Upgrade
						if (scriptLev.IndexOf("\\Create\\") >= 0 && scriptLev.IndexOf("\\Release_") >= 0)
							scriptLev = scriptLev.Replace("\\Create\\", "\\Upgrade\\");
					}

					bool result = false;
					// se sto analizzando gli script di livello 3 significa che deve essere una library
					//@@TODOMICHI
					/*if (numLevel == 3)
						result = scriptMng.ExecuteLibrary(scriptLev, out error);
					else*/
					result = scriptMng.ExecuteFileSql(scriptLev, out error, 1947483647); // altrimenti eseguo lo script SQL

					if (!result)
					{
						ExtendedInfo ei = scriptMng.Diagnostic.AllItems[scriptMng.Diagnostic.AllItems.Length - 1].ExtendedInfo as ExtendedInfo;
						OnElaborationProgressMessage?.Invoke(moduleDBInfo, false, sql, step, error, scriptLev, ei);

						// se fallisce l'esecuzione dello script imposto lo status a false e
						// memorizzo il numero di livello, di step e di release in cui si è verificato 
						// l'errore e non proseguo con l'elaborazione.
						moduleDBInfo.StatusOk = false;
						moduleDBInfo.NrLevel = numLevel;
						moduleDBInfo.NrStep = Convert.ToInt32(step);
						moduleDBInfo.NrRelease = singleInfo.DBRel;
						singleInfo.Status = false;
						ErrorInRunSqlScript = true;
						break;
					}
					else
						OnElaborationProgressMessage?.Invoke(moduleDBInfo, true, sql, step, DatabaseLayerConsts.OK, scriptLev, null);

					if (!forMissingTables)
					{
						moduleDBInfo.NrLevel = numLevel;
						moduleDBInfo.NrStep = Convert.ToInt32(step);
					}
				}

				// scorro la lista degli eventuali step per i dati di default e me li tengo da parte 
				foreach (DefaultDataStep defaultStep in singleInfo.DefaultDataStepList)
				{
					if (
						string.IsNullOrWhiteSpace(defaultStep.Country) ||
						string.Compare(defaultStep.Country, this.contextInfo.IsoState, StringComparison.InvariantCultureIgnoreCase) == 0
						)
					{
						impExpManager.AddDefaultDataStepTable(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName, defaultStep);
					}
				}
			}

			// se le operazioni sono andate a buon fine imposto i valori del livello e dello step a zero
			if (moduleDBInfo.StatusOk)
			{
				moduleDBInfo.NrLevel = 0;
				moduleDBInfo.NrStep = 0;

				// se sto facendo un recovery, e se si tratta di un recovery per errori riscontrati in creazione del modulo,
				// metto come numero di release il numero effettivo di modulo (tag <Release> del DatabaseObjects.xml)
				// se la release di recovery è > 1, allora assegno quella che ho appena eseguito
				// se non sto facendo un recovery allora assegno il numero effettivo di modulo (tag <Release>)
				moduleDBInfo.NrRelease =
					(isRecoveryAction)
					? ((moduleDBInfo.RecoveryInfo.NrRelease == 1) ? moduleDBInfo.DBRelease : moduleDBInfo.RecoveryInfo.NrRelease)
					: moduleDBInfo.DBRelease;
			}

			return moduleDBInfo.StatusOk;
		}
		#endregion

		#region GESTIONE UPGRADE DATABASE
		/// <summary>
		/// entry-point per la gestione dell'upgrade del database (in caso che il database risulti incompleto)
		/// </summary>
		//---------------------------------------------------------------------------
		private bool ManageDatabaseUpgrade()
		{
			bool ok = true;

			//**** GESTIONE TABELLE MANCANTI ****//
			// se nell'array delle tabelle mancanti ho almeno un elemento allora lancio la procedura di creazione
			if (dbStructInfo.ModuleWithMissingTblList.Count > 0)
				ok = UpgradeDatabase(true);

			//**** GESTIONE UPGRADE DATABASE ****//
			// include anche la gestione dei moduli mancanti (da creare da zero se non presenti nella DBMark)
			string error = string.Empty;
			if (dbStructInfo.ModuleToUpgradeList.Count > 0)
			{
				// ora vado a parsare altri file, xciò devo costruire dei nuovi oggetti di tipo Graph
				dbStructInfo.GraphLevel1 = new DirectGraph();
				dbStructInfo.GraphLevel2 = new DirectGraph();
				dbStructInfo.GraphLevel3 = new DirectGraph();

				// per ogni modulo inserito nella lista da upgradare 
				// cerco il corrispondente file UpgradeInfo.xml (se esiste) e ne faccio il parse
				foreach (ModuleDBInfo moduleDBInfo in dbStructInfo.ModuleToUpgradeList)
				{
					// se il modulo e' mancante allora devo caricare le info dal folder Create
					if (moduleDBInfo.IsNew)
					{
						LoadInfoForMissingModule(moduleDBInfo);
					}
					else
					{
						moduleDBInfo.XmlPath = contextInfo.PathFinder.GetStandardUpgradeInfoXML(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);

						if (string.IsNullOrWhiteSpace(moduleDBInfo.XmlPath))
							continue;

						FileInfo fi = new FileInfo(moduleDBInfo.XmlPath);
						if (!contextInfo.PathFinder.ExistFile(moduleDBInfo.XmlPath))
						{
							OnElaborationProgressMessage?.Invoke(moduleDBInfo, false, moduleDBInfo.XmlPath, string.Empty, DatabaseManagerStrings.MsgFileNotExist, moduleDBInfo.XmlPath, null);
							moduleDBInfo.ErrorInFileXML = true;
							continue;
						}
						moduleDBInfo.DirectoryScript = fi.Directory.FullName;

						if (moduleDBInfo.StatusOk)
							moduleDBInfo.UpdateInfo.Parse(dbStructInfo, moduleDBInfo.XmlPath, false, out error, false);

						if (!string.IsNullOrWhiteSpace(error))
						{
							OnElaborationProgressMessage?.Invoke(moduleDBInfo, false, moduleDBInfo.XmlPath, string.Empty, error, moduleDBInfo.DirectoryScript, null);
							moduleDBInfo.ErrorInFileXML = true;
							continue;
						}
					}
				}

				// effettuo il vero e proprio upgrade del database, combinando tutte le info raccolte
				ok = UpgradeDatabase(false);

				if (!ok) // se entro in questo if significa che si e' verificato un errore di ricorsione, quindi ritorno subito
					return ok;
			}
			//

			return ok;
		}

		//---------------------------------------------------------------------------
		private void LoadInfoForMissingModule(ModuleDBInfo moduleDBInfo)
		{
			string error = string.Empty;

			if (moduleDBInfo.StatusOk)
			{
				moduleDBInfo.UpdateInfo.Parse(dbStructInfo, moduleDBInfo.XmlPath, true, out error, true);

				if (!string.IsNullOrWhiteSpace(error))
				{
					OnElaborationProgressMessage?.Invoke
							(
							moduleDBInfo,
							false,
							moduleDBInfo.DirectoryScript,
							string.Empty,
							error,
							moduleDBInfo.DirectoryScript,
							null
							);
					moduleDBInfo.StatusOk = false;
					return;
				}

				if (dbStructInfo.KindOfDb == KindOfDatabase.Company)
				{
					if (impExpManager != null && importDefaultData)
						impExpManager.AddDefaultDataTable(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);

					if (impExpManager != null && importSampleData)
						impExpManager.AddSampleDataTable(moduleDBInfo.ApplicationMember, moduleDBInfo.ModuleName);
				}
			}
		}

		#region Algoritmo Upgrade Database
		/// <summary>
		/// per ogni modulo da upgradare vado a costruire il grafo in base alle dipendenze e poi 
		/// eseguo in successione gli script sql.
		/// </summary>
		//---------------------------------------------------------------------------
		private bool UpgradeDatabase(bool forMissingTables)
		{
			bool success = true;

			tbLevel1List = new List<string>();
			tbLevel2List = new List<string>();
			tbLevel3List = new List<string>();

			// se sono in fase di creazione di tabelle mancanti richiamo il Topological Sorting,
			// se invece sto facendo un vero e proprio upgrade richiamo il Weighted Topological Sorting
			if (forMissingTables)
			{
				dbStructInfo.GraphLevel1.Topo();
				dbStructInfo.GraphLevel2.Topo();
				dbStructInfo.GraphLevel3.Topo();
			}
			else
			{
				try
				{
					dbStructInfo.GraphLevel1.WeightedTopo();
					dbStructInfo.GraphLevel2.WeightedTopo();
					dbStructInfo.GraphLevel3.WeightedTopo();
				}
				catch (Exception ae)
				{
					// eccezione generata dalla ricorsione (non procedo)
					//DiagnosticViewer.ShowCustomizeIconMessage(string.Format(DatabaseManagerStrings.RecursionDetected, ae.Message), DatabaseManagerStrings.LblError);
					OnElaborationProgressMessage?.Invoke
					(
					null,
					false,
					string.Format(DatabaseManagerStrings.RecursionDetected, ae.Message),
					string.Empty,
					string.Empty,
					string.Empty,
					null
					);
					ErrorInRunSqlScript = true;
					return false;
				}
			}

			// riordinamento degli array x caricare prima i moduli di TB
			SortArrays(dbStructInfo.GraphLevel1, tbLevel1List);
			SortArrays(dbStructInfo.GraphLevel2, tbLevel2List);
			SortArrays(dbStructInfo.GraphLevel3, tbLevel3List);

			// se non si è già verificato un errore in fase di recovery inizializzo la varibile a false
			if (!ErrorInRunSqlScript)
				ErrorInRunSqlScript = false;

			// ricerca ed esecuzione degli script SQL 
			if (
				!ManageSQLScript(tbLevel1List, dbStructInfo.GraphLevel1, (forMissingTables) ? dbStructInfo.ModuleWithMissingTblList : dbStructInfo.ModuleToUpgradeList, 1, forMissingTables) ||
				!ManageSQLScript(tbLevel2List, dbStructInfo.GraphLevel2, (forMissingTables) ? dbStructInfo.ModuleWithMissingTblList : dbStructInfo.ModuleToUpgradeList, 2, forMissingTables) ||
				!ManageSQLScript(tbLevel3List, dbStructInfo.GraphLevel3, (forMissingTables) ? dbStructInfo.ModuleWithMissingTblList : dbStructInfo.ModuleToUpgradeList, 3, forMissingTables)
				)
				success = false;

			string msgError = string.Empty;

			// procedo a fare le INSERT, ma solo per i moduli mancanti (che sono insieme alla lista degli aggiornamenti)
			if (!forMissingTables)
				dbStructInfo.DBMarkInfo.InsertInDBMark(dbStructInfo.ModuleToUpgradeList, out msgError, true);

			if (!string.IsNullOrWhiteSpace(msgError))
			{
				OnElaborationProgressMessage?.Invoke
					(
					null,
					false,
					string.Format(DatabaseManagerStrings.ErrInsertingTableDBMark, dbStructInfo.DBMarkInfo.DBMarkTable.TableName),
					string.Empty,
					msgError,
					string.Empty,
					null
					);
				ErrorInRunSqlScript = true;
			}

			msgError = string.Empty;
			// vado ad aggiornare la tabella DBMark
			dbStructInfo.DBMarkInfo.Update_DBMark
				(
				(forMissingTables) ? dbStructInfo.ModuleWithMissingTblList : dbStructInfo.ModuleToUpgradeList,
				out msgError
				);

			if (!string.IsNullOrWhiteSpace(msgError))
			{
				OnElaborationProgressMessage?.Invoke
					(
					null,
					false,
					string.Format(DatabaseManagerStrings.ErrUpdatingTableDBMark, dbStructInfo.DBMarkInfo.DBMarkTable.TableName),
					string.Empty,
					msgError,
					string.Empty,
					null
					);
				ErrorInRunSqlScript = true;
			}

			return success;
		}
		#endregion

		#endregion

		#region Gestione Recovery Data
		//---------------------------------------------------------------------------
		private bool RecoveryData()
		{
			bool success = true;
			isRecoveryAction = true;

			if (dbStructInfo.ModuleToCreateMandColsList.Count > 0)
				CheckMandatoryColumns();

			if (dbStructInfo.ModuleToRecoveryList.Count > 0)
			{
				tbLevel1List = new List<string>();
				tbLevel2List = new List<string>();
				tbLevel3List = new List<string>();

				dbStructInfo.RecoveryGraphLevel1.Topo();
				dbStructInfo.RecoveryGraphLevel2.Topo();
				dbStructInfo.RecoveryGraphLevel3.Topo();

				// riordinamento degli array x caricare prima i moduli di TB
				SortArrays(dbStructInfo.RecoveryGraphLevel1, tbLevel1List);
				SortArrays(dbStructInfo.RecoveryGraphLevel2, tbLevel2List);
				SortArrays(dbStructInfo.RecoveryGraphLevel3, tbLevel3List);

				ErrorInRunSqlScript = false;

				// ricerca ed esecuzione degli script SQL 
				if (
					!ManageSQLScript(tbLevel1List, dbStructInfo.RecoveryGraphLevel1, dbStructInfo.ModuleToRecoveryList, 1, false) ||
					!ManageSQLScript(tbLevel2List, dbStructInfo.RecoveryGraphLevel2, dbStructInfo.ModuleToRecoveryList, 2, false) ||
					!ManageSQLScript(tbLevel3List, dbStructInfo.RecoveryGraphLevel3, dbStructInfo.ModuleToRecoveryList, 3, false)
					)
					success = false;
			}

			string msgError = string.Empty;
			dbStructInfo.DBMarkInfo.Update_DBMark(dbStructInfo.ModuleToRecoveryList, out msgError);

			if (!string.IsNullOrWhiteSpace(msgError))
			{
				OnElaborationProgressMessage?.Invoke
					(
					null,
					false,
					string.Format(DatabaseManagerStrings.ErrUpdatingTableDBMark, dbStructInfo.DBMarkInfo.DBMarkTable.TableName),
					string.Empty,
					msgError,
					string.Empty,
					null
					);
				ErrorInRunSqlScript = true;
			}

			isRecoveryAction = false;
			return success;
		}
		#endregion

		#region Create mandatory columns: TBCreated, TBModified, TBCreatedID, TBModifiedID
		///<summary>
		/// Crea le colonne obbligatorie su una tabella
		/// <param name="tableName">nome tabella da modificare</param>
		/// <param name="columnName">nome colonna da creare</param>
		/// <param name="withID">true: crea la colonna TBCreatedID, false: crea la colonna TBCreated</param>
		///</summary>
		//---------------------------------------------------------------------------
		private bool CreateSingleMandatoryColumn(string tableName, string columnName, bool withID, out string error)
		{
			string commandText = string.Empty;

			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
					commandText = string.Format
						(
						withID ? DatabaseLayerConsts.SQLAddWorkersMandatoryColums : DatabaseLayerConsts.SQLAddMandatoryColums,
						tableName,
						columnName
						);
					break;

				case DBMSType.POSTGRE:
					commandText = string.Format
						(
						withID ? DatabaseLayerConsts.PostgreAddWorkersMandatoryColums : DatabaseLayerConsts.PostgreAddMandatoryColums,
						tableName,
						columnName
						);
					break;
			}

			// eseguo lo script (imposto un timeout alto visto che l'update dei valori delle colonne esistenti potrebbe non essere immediato)
			return scriptMng.ExecuteSql(commandText, out error, false, 1947483647);
		}

		//---------------------------------------------------------------------------
		private bool CreateMandatoryColumns(string tableName, ref ModuleDBInfoList moduleToSetBadStatus)
		{
			ModuleDBInfo moduleDBInfo = null;

			//check if the table is a table of an application
			if (!dbStructInfo.GetApplicationModuleInfo(tableName, out moduleDBInfo))
				return true;

			string error = string.Empty;
			bool ret = false;

			// Add and set TBCreated and TBModified columns
			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
					ret = CreateSingleMandatoryColumn(tableName, DatabaseLayerConsts.TBCreatedColNameForSql, false, out error) && // TBCreated
							CreateSingleMandatoryColumn(tableName, DatabaseLayerConsts.TBModifiedColNameForSql, false, out error) && // TBModified
							CreateSingleMandatoryColumn(tableName, DatabaseLayerConsts.TBCreatedIDColNameForSql, true, out error) && // TBCreatedID
							CreateSingleMandatoryColumn(tableName, DatabaseLayerConsts.TBModifiedIDColNameForSql, true, out error); // TBModifiedID
					break;

				case DBMSType.POSTGRE:
					ret = CreateSingleMandatoryColumn(tableName, DatabaseLayerConsts.TBCreatedColNameForPostgre, false, out error) && // TBCreated
							CreateSingleMandatoryColumn(tableName, DatabaseLayerConsts.TBModifiedColNameForPostgre, false, out error) && // TBModified 
							CreateSingleMandatoryColumn(tableName, DatabaseLayerConsts.TBCreatedIDColNameForPostgre, true, out error) && // TBCreatedID
							CreateSingleMandatoryColumn(tableName, DatabaseLayerConsts.TBModifiedIDColNameForPostgre, true, out error); // TBModifiedID
					break;

			}

			if (ret)
				OnElaborationProgressMessage?.Invoke(moduleDBInfo, true, tableName, "1", DatabaseManagerStrings.CreatedMandatoryColumns, string.Empty, null);
			else
			{
				OnElaborationProgressMessage?.Invoke(moduleDBInfo, false, tableName, "1", error, string.Empty, null);
				diagnostic.Set(DiagnosticType.Error | DiagnosticType.LogOnFile, string.Format(DatabaseManagerStrings.ErrTableCreatingMandatoryColumns, tableName, moduleDBInfo.ApplicationBrand, moduleDBInfo.Title));
				moduleDBInfo.StatusOk = false;
				moduleDBInfo.NrStep = CheckDBStructureInfo.mandatoryColsRelNumb;
				moduleToSetBadStatus.Add(moduleDBInfo);
			}

			OnUpdateMandatoryCols?.Invoke(tableName);

			return ret;
		}

		/// <summary>
		/// Check for the mandatory columns. Each table of the application should have 
		/// </summary>
		//---------------------------------------------------------------------------
		private void CheckMandatoryColumns()
		{
			// non eseguo il controllo se si tratta del database di sistema
			if (dbStructInfo.KindOfDb == KindOfDatabase.System)
				return;

			// mi faccio ritornare la lista delle tabelle che non hanno tutte le colonne obbligatorie
			List<string> tablesWithNoMandatoryColumns = dbStructInfo.GetTablesListWithNoMandatoryColumns();

			if (tablesWithNoMandatoryColumns.Count == 0)
				return;

			Debug.WriteLine("MP-LOG - Nr. tables without mandatory columns: " + tablesWithNoMandatoryColumns.Count.ToString());

			bool result = true;
			ModuleDBInfoList moduleToSetBadStatus = new ModuleDBInfoList();

			try
			{
				if (scriptMng.Connection == null)
					scriptMng.Connection = contextInfo.Connection;

				foreach (string table in tablesWithNoMandatoryColumns)
					result = CreateMandatoryColumns(table, ref moduleToSetBadStatus) && result;
			}
			catch (TBException)
			{
				//DiagnosticViewer.ShowError(DatabaseManagerStrings.ErrCreatingMandatoryColumns, e.Message, e.Procedure, e.Number.ToString(), DatabaseManagerStrings.LblError);
			}

			string msgError = string.Empty;

			if (isRecoveryAction)
			{
				foreach (ModuleDBInfo moduleDBInfo in dbStructInfo.ModuleToCreateMandColsList)
				{
					moduleDBInfo.StatusOk = true;
					moduleDBInfo.NrStep = 0;
				}

				dbStructInfo.DBMarkInfo.Update_DBMark(dbStructInfo.ModuleToCreateMandColsList, out msgError);
			}
			else
				if (moduleToSetBadStatus.Count > 0)
				dbStructInfo.DBMarkInfo.Update_DBMark(moduleToSetBadStatus, out msgError);

			if (!string.IsNullOrWhiteSpace(msgError))
			{
				OnElaborationProgressMessage?.Invoke
					(
					null,
					false,
					string.Format(DatabaseManagerStrings.ErrUpdatingTableDBMark, dbStructInfo.DBMarkInfo.DBMarkTable.TableName),
					string.Empty,
					msgError,
					string.Empty,
					null
					);
				ErrorInRunSqlScript = true;
			}

			if (!result)
				ErrorInRunSqlScript = ErrorInRunSqlScript && !result;
		}
		#endregion

		#region TBGuid: colonna obbligatoria per tabelle master
		///<summary>
		/// Crea sulla tabella la colonna obbligatoria prevista per il TBGuid
		///</summary>
		//---------------------------------------------------------------------------
		private bool CreateSingleTBGuidColumn(TbGuidTable guidTable, out string error)
		{
			error = string.Empty;

			ModuleDBInfo modDBInfo = null;
			if (!dbStructInfo.GetApplicationModuleInfo(guidTable.TableName, out modDBInfo))
				return true;

			string commandText1 = string.Empty;
			bool ret = false;

			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
					commandText1 = (guidTable.IsOnlyToUpdate)
						? string.Format(DatabaseLayerConsts.SQLUpdateGuidMandatoryColumn, guidTable.TableName, DatabaseLayerConsts.TBGuidColNameForSql)
						: string.Format(DatabaseLayerConsts.SQLAddGuidMandatoryColumn, guidTable.TableName, DatabaseLayerConsts.TBGuidColNameForSql);
					break;
			}

			// eseguo lo script (imposto un timeout alto visto che l'update dei valori delle colonne esistenti potrebbe non essere immediato)
			ret = scriptMng.ExecuteSql(commandText1, out error, false, 1947483647);

			if (ret)
				OnElaborationProgressMessage?.Invoke(modDBInfo, true, guidTable.TableName, "1", string.Format((guidTable.IsOnlyToUpdate) ? DatabaseManagerStrings.UpdatedTBGuidColValue : DatabaseManagerStrings.CreatedMandatoryTBGuidCol, guidTable.TableName), string.Empty, null);
			else
			{
				OnElaborationProgressMessage?.Invoke(modDBInfo, false, guidTable.TableName, "1", error, string.Empty, null);
				diagnostic.Set(DiagnosticType.Error | DiagnosticType.LogOnFile, string.Format((guidTable.IsOnlyToUpdate) ? DatabaseManagerStrings.ErrUpdatingMandatoryTBGuidCol : DatabaseManagerStrings.ErrTableCreatingMandatoryTBGuidCol, guidTable.TableName, modDBInfo.ApplicationBrand, modDBInfo.Title));
				modDBInfo.StatusOk = false;
				modDBInfo.NrStep = 8888;
				//moduleToSetBadStatus.Add(moduleDBInfo);
			}

			OnUpdateMandatoryCols?.Invoke(guidTable.TableName);
			return ret;
		}
		/// <summary>
		/// Controlla le presenza delle colonne obbligatorie previste per le mastertable 
		/// del RowSecurityLayer ed eventualmente le crea
		/// </summary>
		//---------------------------------------------------------------------------
		private void CheckTBGuidColumn()
		{
			// vado ad analizzare la struttura delle tabelle identificate come mastertable
			dbStructInfo.CheckMissingTBGuidColumn();

			if (dbStructInfo.TablesWithMissingTBGuidCol.Count == 0)
				return;

			Debug.WriteLine("MP-LOG - Nr. tables without TBGuid column: " + dbStructInfo.TablesWithMissingTBGuidCol.Count.ToString());

			bool result = false;
			string error;

			try
			{
				if (scriptMng.Connection == null)
					scriptMng.Connection = contextInfo.Connection;

				if (contextInfo.DbType == DBMSType.ORACLE)
				{
					result = scriptMng.ExecuteSql(DatabaseLayerConsts.OracleCreateGetGuidFunction, out error);
					if (!result)
						return;
				}

				foreach (TbGuidTable table in dbStructInfo.TablesWithMissingTBGuidCol)
					result = CreateSingleTBGuidColumn(table, out error) && result;

				if (contextInfo.DbType == DBMSType.ORACLE)
					scriptMng.ExecuteSql(DatabaseLayerConsts.OracleDropGetGuidFunction, out error);
			}
			catch (TBException)
			{
				//DiagnosticViewer.ShowError(DatabaseManagerStrings.ErrCreatingMandatoryTBGuidCol, e.Message, e.Procedure, e.Number.ToString(), DatabaseManagerStrings.LblError);
			}
		}

		#endregion

		#region Colonne obbligatorie del RowSecurityLayer
		/// <summary>
		/// Controlla le presenza delle colonne obbligatorie previste per le mastertable 
		/// del RowSecurityLayer ed eventualmente le crea
		/// </summary>
		//---------------------------------------------------------------------------
		private void CheckRSMasterTablesColumns()
		{
			// se il modulo non e' attivato oppure l'azienda corrente non e' sottoposta alla RowSecurity non procedo
			if (!contextInfo.IsRowSecurityActivated || !contextInfo.UseRowSecurity)
				return;

			// vado ad analizzare la struttura delle tabelle identificate come mastertable
			dbStructInfo.CheckMissingRSColumns();

			if (dbStructInfo.TablesWithMissingRSCols.Count == 0)
				return;

			bool result = false;
			string error;

			try
			{
				if (scriptMng.Connection == null)
					scriptMng.Connection = contextInfo.Connection;

				foreach (string table in dbStructInfo.TablesWithMissingRSCols)
					result = CreateSingleRowSecurityColumn(table, out error) && result;
			}
			catch (TBException)
			{
				//DiagnosticViewer.ShowError(DatabaseManagerStrings.ErrCreatingMandatoryColumnsForRS, e.Message, e.Procedure, e.Number.ToString(), DatabaseManagerStrings.LblError);
			}
		}

		///<summary>
		/// Crea sulla tabella la colonna obbligatoria prevista per il RowSecurityLayer
		///</summary>
		//---------------------------------------------------------------------------
		private bool CreateSingleRowSecurityColumn(string tableName, out string error)
		{
			error = string.Empty;

			ModuleDBInfo modDBInfo = null;
			if (!dbStructInfo.GetApplicationModuleInfo(tableName, out modDBInfo))
				return true;

			string commandText1 = string.Empty;
			string commandText2 = string.Empty;
			bool ret = false;

			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
					commandText1 = string.Format(DatabaseLayerConsts.SQLAddRowSecurityIDColumn, tableName, DatabaseLayerConsts.RowSecurityIDForSQL);
					commandText2 = string.Format(DatabaseLayerConsts.SQLAddIsProtectedColumn, tableName, DatabaseLayerConsts.IsProtectedForSQL);
					break;
			}

			// eseguo lo script (imposto un timeout alto visto che l'update dei valori delle colonne esistenti potrebbe non essere immediato)
			ret = scriptMng.ExecuteSql(commandText1, out error, false, 1947483647) && scriptMng.ExecuteSql(commandText2, out error, false, 1947483647);

			if (ret)
				OnElaborationProgressMessage?.Invoke(modDBInfo, true, tableName, "1", DatabaseManagerStrings.CreatedMandatoryColumnsForRS, string.Empty, null);
			else
			{
				OnElaborationProgressMessage?.Invoke(modDBInfo, false, tableName, "1", error, string.Empty, null);
				diagnostic.Set(DiagnosticType.Error | DiagnosticType.LogOnFile, string.Format(DatabaseManagerStrings.ErrTableCreatingMandatoryColumnsForRS, tableName, modDBInfo.ApplicationBrand, modDBInfo.Title));
				modDBInfo.StatusOk = false;
				modDBInfo.NrStep = 8888;
				//moduleToSetBadStatus.Add(moduleDBInfo);
			}

			OnUpdateMandatoryCols?.Invoke(tableName);
			return ret;
		}

		#endregion

		#region RefreshViews: aggiorna le view al termine dell'elaborazione per allineare le loro strutture
		/// <summary>
		/// RefreshViews
		/// Al termine dell'aggiornamento del database, per tutte le view presenti nel database 
		/// viene eseguito un comando di refresh (con l'apposita procedura di sistema sp_refreshview)
		/// in modo da fare in modo di aggiornare la struttura delle view qualora fosse cambiata la 
		/// struttura delle colonne sulle quali si appoggiano le viste
		/// Correzione anomalia 16061
		/// </summary>
		//---------------------------------------------------------------------------
		private void RefreshViews()
		{
			// se il database aziendale non è di SQL non procedo
			if (!contextInfo.Connection.IsSqlConnection())
				return;

			List<string> viewsList = new List<string>();

			try
			{
				if (contextInfo.Connection.State != ConnectionState.Open)
					contextInfo.Connection.Open();

				using (TBCommand myCommand = new TBCommand(contextInfo.Connection))
				{
					myCommand.CommandText = "select TABLE_NAME from INFORMATION_SCHEMA.VIEWS";

					using (IDataReader myReader = myCommand.ExecuteReader())
					{
						while (myReader.Read())
						{
							string viewName = myReader["TABLE_NAME"].ToString();

							// skippo le view di sistema di SQL
							if ((string.Compare(viewName, "sysconstraints", StringComparison.OrdinalIgnoreCase) != 0) &&
								(string.Compare(viewName, "syssegments", StringComparison.OrdinalIgnoreCase) != 0))
								viewsList.Add(viewName);
						}
					}
				}
			}
			catch (TBException e)
			{
				Debug.WriteLine("Error loading views name from database (method RefreshViews): " + e.Message);
				return;
			}

			if (viewsList.Count <= 0)
				return;

			// per ogni view eseguo il comando di refresh
			using (TBCommand myCommand = new TBCommand(contextInfo.Connection))
			{
				foreach (string view in viewsList)
				{
					myCommand.CommandText = string.Format("sp_refreshview '{0}'", view);
					try
					{
						myCommand.ExecuteNonQuery();
					}
					catch (TBException e)
					{
						Debug.WriteLine("Error executing sp_refreshview (method RefreshViews): " + e.Message);
					}
				}
			}
		}
		# endregion

		# region Gestione aggiornamento tabelle sottoposte a tracciatura
		/// <summary>
		/// UpgradeAuditingTables
		/// Gestione aggiornamento tabelle sottoposte a tracciatura per l'aggiunta della colonna AU_ID
		/// (tramite script Transact-SQL non è possibile perchè la sintassi di ALTER TABLE non accetta 
		/// il passaggio di parametri)
		/// </summary>
		//---------------------------------------------------------------------------
		private void UpgradeAuditingTables()
		{
			if (this.dbStructInfo.ModuleToUpgradeList == null)
				return;

			ModuleDBInfo dbinfo = this.dbStructInfo.ModuleToUpgradeList.GetItem("TBExtensions.TBAuditing", true);

			if (dbinfo != null && dbinfo.UpdateInfo != null && dbinfo.UpdateInfo.UpdateInfoList != null)
			{
				foreach (SingleUpdateInfo sui in dbinfo.UpdateInfo.UpdateInfoList)
				{
					// gestisco l'upgrade da 1 a 2
					if (sui.DBRel == 2)
					{
						// carico l'elenco delle tabelle che iniziano con AU_ e il nome della PK
						LoadAuditingTables();

						if (auditTables == null)
							return;

						string[] str = null;
						string tbl, pk = string.Empty;

						// per ogni tabella individuata estrapolo il suo nome e il nome della PK (divise da -)
						foreach (string audTable in auditTables)
						{
							str = audTable.Split(new Char[] { '-' });
							tbl = str[0].ToString();
							pk = str[1].ToString();

							// a seconda del tipo di database eseguo l'UPDATE delle tabelle
							if (contextInfo.DbType == DBMSType.SQLSERVER)
								UpdateAuditTableForSql_Rel2(tbl, pk);
						}
					}

					// gestisco l'upgrade da 2 a 3 solo per SQL Server (le colonne IDENTITY sono solo in SQL)
					if (sui.DBRel == 3 && contextInfo.DbType == DBMSType.SQLSERVER)
					{
						// carico l'elenco delle tabelle che iniziano con AU_
						LoadAuditingTables();

						if (auditTables == null)
							return;

						string[] str = null;
						string tbl, pk = string.Empty;

						// per ogni tabella individuata estrapolo il suo nome e il nome della PK (divise da -)
						foreach (string audTable in auditTables)
						{
							str = audTable.Split(new Char[] { '-' });
							tbl = str[0].ToString();
							pk = str[1].ToString();

							// eseguo l'aggiornamento per ogni tabella sottoposta a tracciatura
							UpdateAuditTableForSql_Rel3(tbl, pk);
						}
					}
				}
			}
		}

		# region Update tabelle di Auditing (scatto di release 2) per SqlServer
		/// <summary>
		/// Aggiorno ogni tabella sottoposta a tracciatura (a parte AUDIT_Tables e AUDIT_Namespaces) come segue:
		/// 1. compongo lo script di creazione della nuova tabella con i segmenti di chiave primaria di tipo corretto
		/// piu' le colonne di Mago sottoposte a tracciatura presenti nella vecchia tabella
		/// 2. compongo l'elenco dei segmenti che andranno a comporre la PK
		/// 3. rinomino la vecchia tabella AU_x in TEMP_AU_x
		/// 4. droppo sulla vecchia tabella (diventata TEMP_AU_x) il constraint di PK
		/// 5. sposto i dati dalla vecchia tabella alla nuova, componendo lo script di INSERT INTO
		/// 6. creo il nuovo constraint di PK con lo stesso ordinal_position [necessario per il funzionamentod di TB quando traccia]
		/// 7. droppo la vecchia tabella di appoggio
		/// </summary>
		/// <param name="auditTable">nome tabella</param>
		/// <param name="pkConstraint">nome constraint PK</param>
		//---------------------------------------------------------------------------
		private void UpdateAuditTableForSql_Rel2(string auditTable, string pkConstraint)
		{
			//--- Compongo lo script di creazione della nuova tabella
			string createTableScript = (contextInfo.UseUnicode)
				? string.Format(DatabaseLayerConsts.SqlCreateTableTextUnicode, auditTable)
				: string.Format(DatabaseLayerConsts.SqlCreateTableText, auditTable);

			List<CatalogColumn> cols = dbStructInfo.CatalogInfo.GetColumnsInfo(auditTable, contextInfo.Connection);

			foreach (CatalogColumn catCol in cols)
			{
				// skippo le colonne che iniziano con AU_
				if (catCol.Name.StartsWith("AU_"))
					continue;

				createTableScript += catCol.CreateSqlColumnScript(false); //non devo considerare la proprietà di identity
				createTableScript += ", "; // metto la virgola tra una colonna e l'altra
			}
			createTableScript = createTableScript.Substring(0, createTableScript.Length - 2); // levo l'ultima virgola
			createTableScript += " )"; // chiudo la parentesi

			//--- compongo l'elenco dei segmenti che andranno a comporre la pk (per primo AU_ID)
			StringCollection pkCols = new StringCollection();
			dbStructInfo.CatalogInfo.GetPrimaryKeys(auditTable, ref pkCols);
			string pkScript = "AU_ID, ";
			foreach (string pk in pkCols)
			{
				pkScript += pk;
				pkScript += ", "; // metto la virgola tra una colonna e l'altra
			}
			pkScript = pkScript.Substring(0, pkScript.Length - 2); // levo l'ultima virgola

			// nome tabella temporanea
			string tempAuditTable = string.Concat("TEMP_", auditTable);

			TBCommand myCommand = new TBCommand(contextInfo.Connection);

			try
			{
				// rinomino la vecchia tabella
				myCommand.CommandText = string.Format("EXEC sp_rename '{0}' , '{1}'", auditTable, tempAuditTable);
				myCommand.ExecuteNonQuery();
				// droppo sulla vecchia tabella il constraint di pk
				myCommand.CommandText = string.Format
					(
					@"IF EXISTS (SELECT dbo.sysobjects.name FROM dbo.sysobjects 
					WHERE dbo.sysobjects.name = '{1}')
					ALTER TABLE {0} DROP CONSTRAINT {1}",
					tempAuditTable,
					pkConstraint);
				myCommand.ExecuteNonQuery();
				// creo la nuova tabella con la colonna AU_ID
				myCommand.CommandText = createTableScript;
				myCommand.ExecuteNonQuery();
				// sposto i dati dalla vecchia tabella alla nuova, componendo lo script di INSERT INTO
				myCommand.CommandText = GetInsertIntoScriptForSql_Rel2(tempAuditTable, auditTable, cols);
				myCommand.ExecuteNonQuery();
				// creo il nuovo constraint di pk (compresa la colonna AU_ID)
				myCommand.CommandText = string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY NONCLUSTERED ({2})", auditTable, pkConstraint, pkScript);
				myCommand.ExecuteNonQuery();
				// droppo la vecchia tabella di appoggio
				myCommand.CommandText = string.Format("DROP TABLE {0}", tempAuditTable);
				myCommand.ExecuteNonQuery();
			}
			catch (TBException e)
			{
				Debug.WriteLine("Error in method UpdateAuditTableForSql_Rel2\r\n");
				Debug.WriteLine("Operation failed: " + e.Message + " " + e.Number);
			}
		}

		/// <summary>
		/// Compongo lo script di INSERT INTO per spostare i dati dalla tabella di appoggio alla nuova tabella
		/// </summary>
		/// <param name="sourceTable">nome tabella di origine</param>
		/// <param name="destinationTable">nome tabella di destinazione</param>
		/// <param name="columnsInfo">array di colonne</param>
		/// <returns>script di INSERT</returns>
		//----------------------------------------------------------------------
		private string GetInsertIntoScriptForSql_Rel2(string sourceTable, string destinationTable, List<CatalogColumn> columnsInfo)
		{
			// non indicando la colonna di tipo IDENTITY nella INSERT non serve impostare la proprietà SET IDENTITY_INSERT
			string script = string.Format("INSERT INTO [{0}]\r\n(", destinationTable);

			string columnsScript = string.Empty;
			foreach (CatalogColumn columnInfo in columnsInfo)
			{
				// skippo la colonna AU_ID
				if (string.Compare(columnInfo.Name, "AU_ID", StringComparison.OrdinalIgnoreCase) == 0)
					continue;

				columnsScript += string.Format("[{0}], ", columnInfo.Name);
			}

			if (columnsScript.Length == 0)
				return string.Empty;

			columnsScript = columnsScript.Substring(0, columnsScript.Length - 2);

			script += columnsScript;
			script += ")\r\nSELECT ";
			script += columnsScript;
			script += string.Format("\r\nFROM [{0}]\r\n", sourceTable);

			Debug.WriteLine("GetInsertIntoScriptForSql_Rel2:\r\n" + script);

			return script;
		}
		# endregion

		# region Update tabelle di Auditing (scatto di release 3) SOLO per SqlServer
		/// <summary>
		/// Aggiorno ogni tabella sottoposta a tracciatura (a parte AUDIT_Tables e AUDIT_Namespaces) come segue:
		/// 1. compongo lo script di creazione della nuova tabella con i segmenti di chiave primaria di tipo corretto
		/// piu' le colonne di Mago sottoposte a tracciatura presenti nella vecchia tabella
		/// 2. compongo l'elenco dei segmenti che andranno a comporre la PK
		/// 3. rinomino la vecchia tabella AU_x in TEMP_AU_x
		/// 4. droppo sulla vecchia tabella (diventata TEMP_AU_x) il constraint di PK
		/// 5. sposto i dati dalla vecchia tabella alla nuova, componendo lo script di INSERT INTO
		/// 6. creo il nuovo constraint di PK con lo stesso ordinal_position [necessario per il funzionamentod di TB quando traccia]
		/// 7. droppo la vecchia tabella di appoggio
		/// </summary>
		/// <param name="auditTable">nome tabella</param>
		/// <param name="pkConstraint">nome constraint PK</param>
		//-----------------------------------------------------------------------------
		private void UpdateAuditTableForSql_Rel3(string auditTable, string pkConstraint)
		{
			//--- Compongo lo script di creazione della nuova tabella
			string createTableScript = (contextInfo.UseUnicode)
				? string.Format(DatabaseLayerConsts.SqlCreateTableAuditRel3Unicode, auditTable)
				: string.Format(DatabaseLayerConsts.SqlCreateTableAuditRel3, auditTable);

			List<CatalogColumn> cols = dbStructInfo.CatalogInfo.GetColumnsInfo(auditTable, contextInfo.Connection);

			foreach (CatalogColumn catCol in cols)
			{
				// skippo le colonne che iniziano con AU_
				if (catCol.Name.StartsWith("AU_"))
					continue;

				createTableScript += catCol.CreateSqlColumnScript(false); //non devo conderare la proprietà di identity
				createTableScript += ", "; // metto la virgola tra una colonna e l'altra
			}
			createTableScript = createTableScript.Substring(0, createTableScript.Length - 2); // levo l'ultima virgola
			createTableScript += " )"; // chiudo la parentesi

			//--- compongo l'elenco dei segmenti che andranno a comporre la pk
			StringCollection pkCols = new StringCollection();
			dbStructInfo.CatalogInfo.GetPrimaryKeys(auditTable, ref pkCols);

			string pkScript = string.Empty;
			foreach (string pk in pkCols)
			{
				pkScript += pk;
				pkScript += ", "; // metto la virgola tra una colonna e l'altra
			}
			pkScript = pkScript.Substring(0, pkScript.Length - 2); // levo l'ultima virgola

			// nome tabella temporanea
			string tempAuditTable = string.Concat("TEMP_", auditTable);

			TBCommand myCommand = new TBCommand(contextInfo.Connection);

			try
			{
				// rinomino la vecchia tabella
				myCommand.CommandText = string.Format("EXEC sp_rename '{0}' , '{1}'", auditTable, tempAuditTable);
				myCommand.ExecuteNonQuery();
				// droppo sulla vecchia tabella il constraint di PK
				myCommand.CommandText = string.Format
					(
					@"IF EXISTS (SELECT dbo.sysobjects.name FROM dbo.sysobjects 
					WHERE dbo.sysobjects.name = '{1}')
					ALTER TABLE {0} DROP CONSTRAINT {1}",
					tempAuditTable,
					pkConstraint
					);
				myCommand.ExecuteNonQuery();
				// creo la nuova tabella con la colonna AU_ID di tipo int senza IDENTITY
				myCommand.CommandText = createTableScript;
				myCommand.ExecuteNonQuery();
				// sposto i dati dalla vecchia tabella alla nuova, componendo lo script di INSERT INTO
				myCommand.CommandText = GetInsertIntoScriptForSql_Rel3(tempAuditTable, auditTable, cols);
				myCommand.ExecuteNonQuery();
				// creo il nuovo constraint di PK
				myCommand.CommandText = string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY NONCLUSTERED ({2})", auditTable, pkConstraint, pkScript);
				myCommand.ExecuteNonQuery();
				// droppo la vecchia tabella di appoggio
				myCommand.CommandText = string.Format("DROP TABLE {0}", tempAuditTable);
				myCommand.ExecuteNonQuery();
			}
			catch (TBException e)
			{
				Debug.WriteLine("Error in method UpdateAuditTableForSql_Rel2\r\n");
				Debug.WriteLine("Operation failed: " + e.Message + " " + e.Number);
			}
		}

		/// <summary>
		/// Compongo lo script di INSERT INTO per spostare i dati dalla tabella di appoggio alla nuova tabella
		/// </summary>
		/// <param name="sourceTable">nome tabella di origine</param>
		/// <param name="destinationTable">nome tabella di destinazione</param>
		/// <param name="columnsInfo">array di colonne</param>
		/// <returns>script di INSERT</returns>
		//----------------------------------------------------------------------
		private string GetInsertIntoScriptForSql_Rel3(string sourceTable, string destinationTable, List<CatalogColumn> columnsInfo)
		{
			string script = string.Format("INSERT INTO [{0}]\r\n(", destinationTable);

			string columnsScript = string.Empty;
			foreach (CatalogColumn columnInfo in columnsInfo)
				columnsScript += string.Format("[{0}], ", columnInfo.Name);

			if (string.IsNullOrEmpty(columnsScript))
				return string.Empty;

			columnsScript = columnsScript.Substring(0, columnsScript.Length - 2);

			script += columnsScript;
			script += ")\r\nSELECT ";
			script += columnsScript;
			script += string.Format("\r\nFROM [{0}]\r\n", sourceTable);

			Debug.WriteLine("GetInsertIntoScriptForSql_Rel3:\r\n" + script);

			return script;
		}
		# endregion

		# endregion

		# region LoadAuditingTables (caricamento tabelle sottoposte a tracciatura)
		/// <summary>
		/// LoadAuditingTables
		/// Funzione centralizzata per il caricamento delle tabelle sottoposte a tracciatura (ovvero che iniziano con AU_)
		/// Tramite le view di sistema estraggo l'elenco delle tabelle che iniziano con 'AU%', con il nome del
		/// segmento di PK e l'elenco delle colonne segmenti di chiave.
		/// Scorro poi su questo elenco e vado ad aggiornare ogni singola tabella.
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadAuditingTables()
		{
			string findAuditTablesForSql = @"SELECT TABLE_NAME AS MYTABLE, CONSTRAINT_NAME AS MYCONSTRAINT 
											FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME LIKE 'AU%'";

			string findAuditTablesForPostgre = @"SELECT TABLE_NAME AS MYTABLE, CONSTRAINT_NAME AS MYCONSTRAINT 
											FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME LIKE 'AU%'";

			try
			{
				using (TBCommand myCommand = new TBCommand(contextInfo.Connection))
				{
					if (contextInfo.DbType == DBMSType.SQLSERVER)
						myCommand.CommandText = findAuditTablesForSql;
					else if (contextInfo.DbType == DBMSType.POSTGRE)
						myCommand.CommandText = findAuditTablesForPostgre;

					using (IDataReader myReader = myCommand.ExecuteReader())
					{
						auditTables = new List<string>();

						while (myReader.Read())
						{
							// skippo le tabelle AUDIT_Tables e AUDIT_Namespaces
							if (string.Compare(myReader["MYTABLE"].ToString(), "AUDIT_Tables", StringComparison.OrdinalIgnoreCase) == 0 ||
								string.Compare(myReader["MYTABLE"].ToString(), "AUDIT_Namespaces", StringComparison.OrdinalIgnoreCase) == 0)
								continue;

							auditTables.Add(string.Concat(myReader["MYTABLE"].ToString(), "-", myReader["MYCONSTRAINT"].ToString()));
						}
					}
				}
			}
			catch (TBException e)
			{
				Debug.WriteLine("Error in LoadAuditingTables: " + e.Message);
				return;
			}
		}
		# endregion

		# region AdjustAuditingTablesStructure - Allineamento struttura tabelle Auditing con la struttura di ERP
		/// <summary>
		/// AdjustAuditingTablesStructure
		/// Gestione allineamento tabelle sottoposte a tracciatura con le tabelle di ERP
		/// Operazione effettuata al termine di ogni aggiornamento database se l'azienda è sottoposta a tracciatura
		/// Nuova gestione inserita con la rel. 2.10 (prevista per maggio 2008).
		/// </summary>
		//---------------------------------------------------------------------------
		private void AdjustAuditingTablesStructure()
		{
			// carico l'elenco delle tabelle che iniziano con AU_
			LoadAuditingTables();

			if (auditTables == null)
				return;

			auditTablesInfo = new List<AuditTableStructureInfo>();

			string[] str = null;
			string auditTbl, pk, erpTbl = string.Empty;

			// per ogni tabella sottoposta a tracciatura memorizzo le informazioni in una classe di appoggio:
			// 1. memorizzo il nome ed il nome del constraint di PK
			// 2. carico le informazioni delle colonne e dei segmenti di chiave primaria
			// 3. estrapolo il nome della corrispondente tabella in ERP
			// 4. carico le informazioni delle colonne e dei segmenti di chiave primaria della tabella di ERP
			// 5. riempio un array di oggetti di tipo AuditTableStructureInfo
			foreach (string table in auditTables)
			{
				str = table.Split(new Char[] { '-' });

				auditTbl = str[0].ToString();
				pk = str[1].ToString();
				erpTbl = auditTbl.Substring(3);

				AuditTableStructureInfo myAuditTable = new AuditTableStructureInfo(auditTbl, pk);
				// riempio le strutture con le informazioni di catalog sia per la tabella di Auditing che per quella di ERP
				myAuditTable.ColumnsInfo = dbStructInfo.CatalogInfo.GetColumnsInfo(auditTbl, contextInfo.Connection);
				dbStructInfo.CatalogInfo.GetPrimaryKeys(auditTbl, ref myAuditTable.PKColumnsInfo);

				myAuditTable.ErpName = erpTbl;
				myAuditTable.ErpColumnsInfo = dbStructInfo.CatalogInfo.GetColumnsInfo(erpTbl, contextInfo.Connection);
				dbStructInfo.CatalogInfo.GetPrimaryKeys(erpTbl, ref myAuditTable.ErpPKColumnsInfo);

				auditTablesInfo.Add(myAuditTable);
			}

			if (contextInfo.DbType == DBMSType.SQLSERVER)
				AdjustAuditingTablesStructureForSql();
			else if (contextInfo.DbType == DBMSType.ORACLE)
				AdjustAuditingTablesStructureForOracle();
		}

		/// <summary>
		/// AdjustAuditingTablesStructureForSql
		/// Gestione allineamento tabelle sottoposte a tracciatura con le tabelle di ERP per SQL Server
		/// 1. per ogni colonna della AU_ cerco la corrispondente in quella di ERP e:
		///		a. se la colonna esiste confronto il tipo e la dimensione e la aggiorno
		///		b. devo controllare i segmenti di PK se sono uguali
		///		c. se sono diversi rinomino la tabella e la ricreo con il nuovo constraint di PK
		///		d. altrimenti allargo le dimensioni della sola colonna
		/// </summary>
		//---------------------------------------------------------------------------
		private void AdjustAuditingTablesStructureForSql()
		{
			// collezione di statements da eseguire al termine del caricamento delle informazioni delle colonne/tabelle
			List<string> alterSizeStatements = null;
			List<string> alterPKStatements = null;

			CatalogColumn erpCol = null;
			bool keySegmentFound = false;

			// per ogni tabella sottoposta a tracciatura controllo le sue colonne e le confronto con la Size
			// della corrispondente in ERP. 
			// Se in ERP e' piu' grande (magari modificata a mano) devo modificare quella nell'auditing
			foreach (AuditTableStructureInfo auditTable in auditTablesInfo)
			{
				alterSizeStatements = new List<string>();
				keySegmentFound = false;

				foreach (CatalogColumn auditCol in auditTable.ColumnsInfo)
				{
					// skippo le colonne che iniziano con AU_ perchè non hanno una corrispondenza in ERP
					if (auditCol.Name.StartsWith("AU_"))
						continue;

					// mi faccio ritornare le informazioni della colonna nella corrispondente tabella di ERP
					erpCol = auditTable.GetColumnInfoFromErpTable(auditCol.Name);

					// se la colonna in ERP esiste procedo (se non esiste piu' la skippo [prima la cancellavamo ma meglio lasciare il cadavere])
					if (erpCol != null)
					{
						// se la size della tabella di audit è inferiore a quella di ERP devo allargarla alla nuova dimensione
						if (auditCol.ColumnSize < erpCol.ColumnSize)
						{
							// mi compongo lo script e lo metto da parte
							alterSizeStatements.Add(string.Format("ALTER TABLE [{0}] ALTER COLUMN {1}", auditTable.Name, erpCol.ToSql(false))); //non devo considerare la proprietà di identity

							if (auditCol.IsKey)
								keySegmentFound = true;
						}
					}
				}

				// per ogni segmento di PK della tabella di ERP controllo che esista il corrispettivo 
				// nella tabella di auditing e li conto
				int elemFoundCount = 0;
				foreach (string erpPk in auditTable.ErpPKColumnsInfo)
				{
					alterPKStatements = new List<string>();

					// se la trovo tra i segmenti allora la conto
					if (auditTable.PKColumnsInfo.ContainsNoCase(erpPk))
						elemFoundCount++;
				}

				// fix anomalia 19819
				if (auditTable.ErpPKColumnsInfo.Count != elemFoundCount)
				{
					// in caso di discrepanza tra le pk della tabella dell'auditing e quella nuova di ERP vado a:
					// 1. rename vecchio constraint PK
					// 2. rename vecchia tabella auditing
					// 3. elimino dalla tabella AUDIT_Tables il riferimento alla tabella appena rinominata
					// 4. visualizzo msg all'utente
					string oldAuditTblName = "OLD_" + auditTable.Name;
					alterPKStatements.Add(string.Format("sp_rename N'{0}.{1}', N'{2}', N'INDEX'", auditTable.Name, auditTable.PKName, "OLD_" + auditTable.PKName));
					alterPKStatements.Add(string.Format("sp_rename N'{0}', N'{1}'", auditTable.Name, oldAuditTblName));
					alterPKStatements.Add(string.Format("DELETE FROM [AUDIT_Tables] WHERE TableName = N'{0}'", auditTable.ErpName));

					if (OnElaborationProgressMessage != null)
					{
						ExtendedInfo ei = new ExtendedInfo();
						ei.Add(DatabaseManagerStrings.Detail, DatabaseManagerStrings.AuditMsg1);
						ei.Add(DatabaseManagerStrings.Description, string.Format(DatabaseManagerStrings.AuditMsg2, auditTable.ErpName, auditTable.Name, oldAuditTblName));

						OnElaborationProgressMessage
							(
							"Extensions#TBAuditing",
							false,
							auditTable.Name,
							string.Empty,
							string.Format(DatabaseManagerStrings.AuditMsg3, auditTable.ErpName),
							string.Empty,
							ei
							);
					}
				}
				else
				{
					// se ho trovato una variazione di lunghezza per una colonna che fa parte della PK devo droppare e ri-creare il constraint
					if (keySegmentFound)
					{
						// devo inserire prima di tutti gli statement la drop del constraint e poi aggiungere in fondo la creazione dello stesso
						alterSizeStatements.Insert(0, string.Format("ALTER TABLE [{0}] DROP CONSTRAINT [{1}]", auditTable.Name, auditTable.PKName));

						// compongo lo script di add constraint
						string pkScript = string.Empty;
						// considero solo le colonne obbligatorie che iniziano con AU_ dalla tabella audit
						foreach (string pk in auditTable.PKColumnsInfo)
						{
							if (pk.StartsWith("AU_"))
							{
								pkScript += pk;
								pkScript += ", "; // metto la virgola tra una colonna e l'altra
							}
						}
						// in coda aggiungo i segmenti di PK aggiornati della tabella di ERP
						foreach (string pk in auditTable.ErpPKColumnsInfo)
						{
							pkScript += pk;
							pkScript += ", "; // metto la virgola tra una colonna e l'altra
						}
						pkScript = pkScript.Substring(0, pkScript.Length - 2); // levo l'ultima virgola

						// in coda ri-creo il constraint
						alterSizeStatements.Add(string.Format("ALTER TABLE [{0}] ADD CONSTRAINT [{1}] PRIMARY KEY NONCLUSTERED ({2})",
										auditTable.Name, auditTable.PKName, pkScript));
					}
				}

				// se ho degli statement da eseguire relativi a cambiamenti strutturali dei segmenti di PK
				// ri-creo la tabella ex-novo, quindi non ho bisogno di eseguire quelli relativi alla Size
				if (alterPKStatements.Count > 0)
				{
					diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.StartAdjustAuditingTables);
					ExecuteAuditStatementList(alterPKStatements);
					diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.EndAdjustAuditingTables);
				}
				else
				{
					if (alterSizeStatements.Count > 0)
					{
						diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.StartAdjustAuditingTables);
						ExecuteAuditStatementList(alterSizeStatements);
						diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.EndAdjustAuditingTables);
					}
				}
			}
		}

		/// <summary>
		/// AdjustAuditingTablesStructureForOracle
		/// Gestione allineamento tabelle sottoposte a tracciatura con le tabelle di ERP per Oracle
		/// Operazione effettuata al termine di ogni aggiornamento database se l'azienda è sottoposta a tracciatura.
		/// 1. per ogni colonna della AU_ cerco la corrispondente in quella di ERP e:
		///		a. se la colonna esiste confronto il tipo e la dimensione e la aggiorno
		///		b. devo controllare i segmenti di PK se sono uguali
		///		c. se sono diversi rinomino la tabella e la ricreo con il nuovo constraint di PK
		///		d. altrimenti allargo le dimensioni della sola colonna
		/// </summary>
		//---------------------------------------------------------------------------
		private void AdjustAuditingTablesStructureForOracle()
		{
			// collezione di statements da eseguire al termine del caricamento delle informazioni delle colonne/tabelle
			List<string> alterSizeStatements = null;
			List<string> alterPKStatements = null;
			
			CatalogColumn erpCol = null;
			bool keySegmentFound = false;

			// per ogni tabella sottoposta a tracciatura controllo le sue colonne e le confronto con la Size
			// della corrispondente in ERP. 
			// Se in ERP e' piu' grande (magari modificata a mano) devo modificare quella nell'auditing
			foreach (AuditTableStructureInfo auditTable in auditTablesInfo)
			{
				alterSizeStatements = new List<string>();
				keySegmentFound = false;

				// per ogni colonna della tabella sottoposta a tracciatura
				foreach (CatalogColumn auditCol in auditTable.ColumnsInfo)
				{
					// skippo le colonne che iniziano con AU_ perchè non hanno una corrispondenza in ERP
					if (auditCol.Name.StartsWith("AU_"))
						continue;

					// mi faccio ritornare le informazioni della colonna nella corrispondente tabella di ERP
					erpCol = auditTable.GetColumnInfoFromErpTable(auditCol.Name);

					// se la colonna in ERP esiste procedo (se non esiste piu' la skippo [prima la cancellavamo ma meglio lasciare il cadavere])
					if (erpCol != null)
					{
						// se la size della tabella di audit è inferiore a quella di Erp devo allargarla alla nuova dimensione
						if (auditCol.ColumnSize < erpCol.ColumnSize)
						{
							//non devo considerare la proprietà di identity
							string str = string.Format("ALTER TABLE \"{0}\" MODIFY {1}", auditTable.Name, erpCol.ToSql(false)); 
							// lo script di modifica della lunghezza di una colonna non deve contenere 
							// la parola NULL (o NOT NULL) in fondo pertanto devo comporre lo script a pezzi
							if (str.EndsWith("NOT NULL", StringComparison.OrdinalIgnoreCase))
								str = str.Substring(0, str.Length - 8);
							else if (str.EndsWith("NULL", StringComparison.OrdinalIgnoreCase))
								str = str.Substring(0, str.Length - 4);

							alterSizeStatements.Add(str);

							if (auditCol.IsKey)
								keySegmentFound = true;
						}
					}
				}

				// per ogni segmento di PK della tabella di ERP controllo che esista il corrispettivo 
				// nella tabella di auditing e li conto
				int elemFoundCount = 0;
				foreach (string erpPk in auditTable.ErpPKColumnsInfo)
				{
					alterPKStatements = new List<string>();

					// se la trovo tra i segmenti allora la conto
					if (auditTable.PKColumnsInfo.ContainsNoCase(erpPk))
						elemFoundCount++;
				}

				// fix anomalia 19819
				if (auditTable.ErpPKColumnsInfo.Count != elemFoundCount)
				{
					// in caso di discrepanza tra le pk della tabella dell'auditing e quella nuova di ERP vado a:
					// 1. rename vecchio constraint PK e indice
					// 2. rename vecchia tabella auditing
					// 3. elimino dalla tabella AUDIT_Tables il riferimento alla tabella appena rinominata
					// 4. visualizzo msg all'utente
					string oldAuditTblName = "OLD_" + auditTable.Name;
					if (oldAuditTblName.Length > 30) // se superiore ai 30 chr tronco la stringa
						oldAuditTblName = oldAuditTblName.Substring(0, 29);
					string oldAuditPKName = "OLD_" + auditTable.PKName;
					if (oldAuditPKName.Length > 30) // se superiore ai 30 chr tronco la stringa
						oldAuditPKName = oldAuditPKName.Substring(0, 29);

					alterPKStatements.Add(string.Format("ALTER TABLE \"{0}\" RENAME CONSTRAINT \"{1}\" TO \"{2}\"", auditTable.Name, auditTable.PKName, oldAuditPKName));
					alterPKStatements.Add(string.Format("ALTER INDEX \"{0}\" RENAME TO \"{1}\"", auditTable.PKName, oldAuditPKName));
					alterPKStatements.Add(string.Format("ALTER TABLE \"{0}\" RENAME TO \"{1}\"", auditTable.Name, oldAuditTblName));
					alterPKStatements.Add(string.Format("DELETE FROM \"AUDIT_TABLES\" WHERE TABLENAME = '{0}'", auditTable.ErpName));

					if (OnElaborationProgressMessage != null)
					{
						ExtendedInfo ei = new ExtendedInfo();
						ei.Add(DatabaseManagerStrings.Detail, DatabaseManagerStrings.AuditMsg1);
						ei.Add(DatabaseManagerStrings.Description, string.Format(DatabaseManagerStrings.AuditMsg2, auditTable.ErpName, auditTable.Name, oldAuditTblName));

						OnElaborationProgressMessage
							(
							"Extensions#TBAuditing",
							false,
							auditTable.Name,
							string.Empty,
							string.Format(DatabaseManagerStrings.AuditMsg3, auditTable.ErpName),
							string.Empty,
							ei
							);
					}
				}
				else
				{
					// se ho trovato una variazione di lunghezza per una colonna che fa parte della PK devo droppare e ri-creare il constraint
					if (keySegmentFound)
					{
						// devo inserire prima di tutti gli statement la drop del constraint e poi aggiungere in fondo la creazione dello stesso
						alterSizeStatements.Insert(0, string.Format("ALTER TABLE \"{0}\" DROP CONSTRAINT \"{1}\"", auditTable.Name, auditTable.PKName));

						// compongo lo script di add constraint
						string pkScript = string.Empty;
						// considero solo le colonne obbligatorie che iniziano con AU_ dalla tabella audit
						foreach (string pk in auditTable.PKColumnsInfo)
						{
							if (pk.StartsWith("AU_"))
							{
								pkScript += pk;
								pkScript += ", "; // metto la virgola tra una colonna e l'altra
							}
						}
						// in coda aggiungo i segmenti di PK aggiornati della tabella di ERP
						foreach (string pk in auditTable.ErpPKColumnsInfo)
						{
							pkScript += pk;
							pkScript += ", "; // metto la virgola tra una colonna e l'altra
						}
						pkScript = pkScript.Substring(0, pkScript.Length - 2); // levo l'ultima virgola

						// in coda ri-creo il constraint
						alterSizeStatements.Add(string.Format("ALTER TABLE \"{0}\" ADD CONSTRAINT \"{1}\" PRIMARY KEY ({2})",
							auditTable.Name, auditTable.PKName, pkScript));
					}
				}

				// se ho degli statement da eseguire relativi a cambiamenti strutturali dei segmenti di PK
				// ri-creo la tabella ex-novo, quindi non ho bisogno di eseguire quelli relativi alla Size
				if (alterPKStatements.Count > 0)
				{
					diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.StartAdjustAuditingTables); 
					ExecuteAuditStatementList(alterPKStatements);
					diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.EndAdjustAuditingTables);
				}
				else
				{
					if (alterSizeStatements.Count > 0)
					{
						diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.StartAdjustAuditingTables);
						ExecuteAuditStatementList(alterSizeStatements);
						diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.EndAdjustAuditingTables);
					}
				}
			}
		}

		///<summary>
		/// Esecuzione lista statement per mettere a posto la struttura delle tabelle dell'Auditing
		///</summary>
		//---------------------------------------------------------------------------
		private void ExecuteAuditStatementList(List<string> statementList)
		{
			TBCommand command = null;
			bool success = false;
			string exceptionMsg = string.Empty;

			try
			{
				if (contextInfo.Connection.State != ConnectionState.Open)
					contextInfo.Connection.Open();

				command = new TBCommand(contextInfo.Connection);

				// eseguo gli statement di aggiornamento uno dopo l'altro
				foreach (string execStatement in statementList)
				{
					success = false;

					command.CommandText = execStatement;
					command.CommandTimeout = 120;

					try
					{
						command.ExecuteNonQuery();
						success = true;
					}
					catch (TBException e)
					{
						Debug.WriteLine(string.Format("Error in method ExecuteStatementList, executing statement: {0}\r\n", execStatement));
						Debug.WriteLine("Operation failed: " + e.Message + " " + e.Number);
						exceptionMsg = e.Message;
					}

					diagnostic.Set(DiagnosticType.LogOnFile, success ? string.Format(DatabaseManagerStrings.ExecStatementWithSuccess, execStatement) : string.Format(DatabaseManagerStrings.ExecStatementWithErrors, execStatement, exceptionMsg));
				}
			}
			catch (TBException e)
			{
				Debug.WriteLine("Operation failed: " + e.Message + " " + e.Number);
			}
		}
		# endregion

		# region Gestione triggers
		///<summary>
		/// LoadTriggersInfo
		/// Carico le informazioni per identificare i triggers presenti nel database aziendale
		/// Per SQL memorizzo l'id della tabella
		///</summary>
		//-----------------------------------------------------------------------
		private void LoadTriggersInfo()
		{
			try
			{
				using (TBCommand myCommand = new TBCommand(contextInfo.Connection))
				{
					if (contextInfo.DbType == DBMSType.SQLSERVER)
						myCommand.CommandText = DatabaseLayerConsts.SQLFindTriggersName;
					else if (contextInfo.DbType == DBMSType.POSTGRE)
						myCommand.CommandText = DatabaseLayerConsts.PostgreFindTriggersName;

					using (IDataReader myReader = myCommand.ExecuteReader())
					{
						triggersList = new List<TriggerInfo>();

						while (myReader.Read())
						{
							// per SQL devo tenere da parte sia il nome della tabella che il trigger
							if (contextInfo.DbType == DBMSType.SQLSERVER)
								triggersList.Add(new TriggerInfo(myReader["TRNAME"].ToString(), myReader["TBLNAME"].ToString()));
							else if (contextInfo.DbType == DBMSType.POSTGRE)
								triggersList.Add(new TriggerInfo(myReader["trname"].ToString(), myReader["tblname"].ToString()));
						}
					}
				}

				if (triggersList.Count == 0)
					diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.TriggersNotFound);
				else
				{
					string msg = DatabaseManagerStrings.TriggersList + "\r\n";
					foreach (TriggerInfo ti in triggersList)
						msg += string.Format("- {0} {1}", ti.TriggerName, (contextInfo.DbType == DBMSType.SQLSERVER) ? string.Format(DatabaseManagerStrings.TriggerTable, ti.TableName) : string.Empty) + "\r\n";
					diagnostic.Set(DiagnosticType.LogOnFile, msg);
				}
			}
			catch (TBException e)
			{
				diagnostic.Set(DiagnosticType.LogOnFile, "Error in LoadTriggersInfo: " + e.Message);
				Debug.WriteLine("Error in LoadTriggersInfo: " + e.Message);
			}
		}

		///<summary>
		/// ManageTriggers
		/// Onde evitare errori in fase di aggiornamento database provocati da eventuali triggers presenti
		/// nel database di Mago, si effettua un controllo preventivo sulle tabelle di sistema del database
		/// e si disabilitano temporaneamente per tutta la durata dell'aggiornamento della struttura del db.
		/// Successivamente vengono ri-abilitati.
		///</summary>
		//-----------------------------------------------------------------------
		private void ManageTriggers(bool enable)
		{
			if (triggersList == null || triggersList.Count == 0)
				return;

			string commandTxt = string.Empty;
			TBCommand myCommand = null;

			try
			{
				if (contextInfo.Connection.State != ConnectionState.Open)
					contextInfo.Connection.Open();

				myCommand = new TBCommand(contextInfo.Connection);

				foreach (TriggerInfo ti in triggersList)
				{
					if (contextInfo.DbType == DBMSType.SQLSERVER)
					{
						commandTxt = (enable)
							? string.Format(DatabaseLayerConsts.SQLEnableTriggerOfTable, ti.TableName, ti.TriggerName)
							: string.Format(DatabaseLayerConsts.SQLDisableTriggerOfTable, ti.TableName, ti.TriggerName);
					}
					else
						if (contextInfo.DbType == DBMSType.ORACLE)
						{
							// escludo gli oggetti eliminati
							if (ti.TriggerName.StartsWith("BIN$", StringComparison.OrdinalIgnoreCase))
								continue;

							commandTxt = (enable)
								? string.Format(DatabaseLayerConsts.OracleEnableTrigger, ti.TriggerName)
								: string.Format(DatabaseLayerConsts.OracleDisableTrigger, ti.TriggerName);
						}

					bool success = false;
					string exceptionMsg = string.Empty;
					try
					{
						myCommand.CommandText = commandTxt;
						myCommand.ExecuteNonQuery();
						success = true;
					}
					catch (Exception e)
					{
						exceptionMsg = e.Message;
						success = false;
					}

					string msg = string.Empty;
					if (enable)
					{
						if (success)
							msg = string.Format(DatabaseManagerStrings.TriggerEnableWithSuccess, ti.TriggerName,
									(contextInfo.DbType == DBMSType.SQLSERVER) ? string.Format(DatabaseManagerStrings.TriggerTable, ti.TableName) : string.Empty);
						else
						{
							msg = string.Format(DatabaseManagerStrings.TriggerEnableWithErrors, ti.TriggerName,
								(contextInfo.DbType == DBMSType.SQLSERVER) ? string.Format(DatabaseManagerStrings.TriggerTable, ti.TableName) : string.Empty, commandTxt, exceptionMsg);

							if (OnElaborationProgressMessage != null)
								OnElaborationProgressMessage(null, false, msg, "1", exceptionMsg, string.Empty, null);
						}
					}
					else
						msg = (success)
							? string.Format(DatabaseManagerStrings.TriggerDisableWithSuccess, ti.TriggerName,
							(contextInfo.DbType == DBMSType.SQLSERVER) ? string.Format(DatabaseManagerStrings.TriggerTable, ti.TableName) : string.Empty)
							: string.Format(DatabaseManagerStrings.TriggerDisableWithErrors, ti.TriggerName,
							(contextInfo.DbType == DBMSType.SQLSERVER) ? string.Format(DatabaseManagerStrings.TriggerTable, ti.TableName) : string.Empty, commandTxt, exceptionMsg);

					diagnostic.Set(DiagnosticType.LogOnFile, msg);
				}
			}
			catch (TBException e)
			{
				diagnostic.Set(DiagnosticType.LogOnFile, "Error in ManageTriggers: " + e.Message);
				Debug.WriteLine("Error in ManageTriggers: " + e.Message);
			}
			catch (Exception ex)
			{
				diagnostic.Set(DiagnosticType.LogOnFile, "Error in ManageTriggers: " + ex.Message);
				Debug.WriteLine("Error in ManageTriggers: " + ex.Message);
			}
			finally
			{
				myCommand.Dispose();
			}
		}
		# endregion
	}

	# region Class AuditTableStructureInfo (per l'aggiornamento della struttura dati tra ERP e Auditing)
	/// <summary>
	/// classe AuditTableStructureInfo, utilizzata in appoggio per le operazioni di riallineamento della
	/// struttura delle tabelle sottoposte a tracciatura (AU_) e quelle di ERP
	/// </summary>
	//===========================================================================
	public class AuditTableStructureInfo
	{
		public string Name = string.Empty; // nome tabella (starts with AU_)
		public string PKName = string.Empty; // nome pk 
		public string ErpName = string.Empty; // nome tabella corrispondente in ERP

		public List<CatalogColumn> ColumnsInfo = new List<CatalogColumn>();	// array di colonne (oggetti di tipo CatalogColumn)
		public StringCollection PKColumnsInfo = new StringCollection(); // collection di nomi di segmenti di chiave primaria

		public List<CatalogColumn> ErpColumnsInfo = new List<CatalogColumn>();	// array di colonne (oggetti di tipo CatalogColumn)
		public StringCollection ErpPKColumnsInfo = new StringCollection(); // collection di nomi di segmenti di chiave primaria

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="tblName">nome tabella AU_</param>
		/// <param name="pkName">nome constraint PK della tabella AU_</param>
		//-----------------------------------------------------------------------
		public AuditTableStructureInfo(string tblName, string pkName)
		{
			Name = tblName;
			PKName = pkName;
		}

		/// <summary>
		/// GetColumnInfoFromErpTable
		/// Dato il nome di una colonna della tabella AU_ cerca la corrispondente nelle info della tabella di Erp
		/// </summary>
		/// <param name="auditCol">nome della colonna da ricercare</param>
		/// <returns>informazione di tipo CatalogColumn</returns>
		//-----------------------------------------------------------------------
		public CatalogColumn GetColumnInfoFromErpTable(string auditCol)
		{
			if (this.ErpColumnsInfo == null) // nel caso di tabella AU_ legata ad una tabella di ERP eliminata
				return null;

			foreach (CatalogColumn erpCol in this.ErpColumnsInfo)
				if (string.Compare(auditCol, erpCol.Name, StringComparison.OrdinalIgnoreCase) == 0)
					return erpCol;

			return null;
		}
	}
	# endregion

	# region Class TriggerInfo (per la gestione dei triggers)
	/// <summary>
	/// Classe TriggerInfo, utilizzata in appoggio per abilitare/disabilitare i triggers in fase
	/// di aggiornamento del database aziendale
	/// Per SQL devo tenermi da parte anche il nome della tabella a cui si appoggia il trigger
	/// </summary>
	//===========================================================================
	public class TriggerInfo
	{
		public string TriggerName = string.Empty;
		public string TableName = string.Empty;

		//-----------------------------------------------------------------------
		public TriggerInfo(string triggerName, string tblName = "")
		{
			TriggerName = triggerName;
			TableName = tblName;
		}
	}
	# endregion
}
