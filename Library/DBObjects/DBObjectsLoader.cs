using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Library.TBWizardProjects;
using Microarea.Library.SqlScriptUtility;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Library.DBObjects
{
	///<summary>
	/// Dato un PathFinder o una lista di applicazioni:
	/// - carica i moduli
	/// - analizza i file .dbxml contenuti in ogni modulo che dichiara oggetti di database
	/// - per ogni oggetto genera la struttura in memoria come da classi presenti in TBWizardProjects
	/// - genera un dictionary (non ordinato) di oggetti di database (da creare / aggiornare)
	/// - genera una lista ordinata delle tabelle
	///</summary>
	//=================================================================================
	public class DBObjectsLoader
	{
		private IBasePathFinder pathFinder;
		private DBMSType dbmsType;
		private XmlDocument dbxmlDocument = new XmlDocument();
		private WizardCodeGenerator wizCodeGenerator = new WizardCodeGenerator(null);

		private IList<string> recursivedTables = new List<string>(); // lista di tabelle oggetto di ricorsione

		// classe che è in grado parsare i file dbxml e riempire le apposite strutture in memoria
		private DBObjectsProjectParser projParser = new DBObjectsProjectParser(null);

		//--------- strutture di supporto
		private IBaseModuleInfo currentModuleInfo;

		// Dictionary con elenco oggetti database suddivisi per tipo
		private Dictionary<string, ExtendedWizardTableInfo> tablesDictionary = new Dictionary<string, ExtendedWizardTableInfo>();
		private Dictionary<string, ExtendedSqlView> viewsDictionary = new Dictionary<string, ExtendedSqlView>();
		private Dictionary<string, ExtendedSqlProcedure> proceduresDictionary = new Dictionary<string, ExtendedSqlProcedure>();

		private Dictionary<string, IList<ExtendedWizardExtraAddedColumnsInfo>> addedColumnsDictionary = new Dictionary<string, IList<ExtendedWizardExtraAddedColumnsInfo>>();
		private Dictionary<string, IList<ExtendedTableUpdate>> updatesDictionary = new Dictionary<string, IList<ExtendedTableUpdate>>();

		private Dictionary<string, IList<ExtendedTBAfterScript>> tbAfterDictionary = new Dictionary<string, IList<ExtendedTBAfterScript>>();

		private IList<string> sortedTablesList = new List<string>(); // lista ordinata di tabelle

		//Lista di colonne da aggiungere alle tabelle mancanti depurati di quelle colonne che eventualmente sono nella DBRecovery
		private Dictionary<string, IList<ExtendedWizardExtraAddedColumnsInfo>> addedColumnsForMissingTable = new Dictionary<string, IList<ExtendedWizardExtraAddedColumnsInfo>>();
		
		// lista ordinata di DBObjectStatement che si stanno per eseguire
		private IList<DBObjectStatement> sortedScriptList = new List<DBObjectStatement>();

		// Properties
		//---------------------------------------------------------------------
		public Dictionary<string, ExtendedWizardTableInfo> Tables { get { return tablesDictionary; } }
		//---------------------------------------------------------------------
		public Dictionary<string, ExtendedSqlView> Views { get { return viewsDictionary; } }
		//---------------------------------------------------------------------
		public Dictionary<string, ExtendedSqlProcedure> Procedures { get { return proceduresDictionary; } }
		//---------------------------------------------------------------------
		public Dictionary<string, IList<ExtendedWizardExtraAddedColumnsInfo>> AddedColumns { get { return addedColumnsDictionary; } }
		//---------------------------------------------------------------------
		public IList<DBObjectStatement> SortedScriptList { get { return sortedScriptList; } }
		//---------------------------------------------------------------------
		public IList<string> SortedTablesList { get { return sortedTablesList; } }
		//---------------------------------------------------------------------
		public Dictionary<string, IList<ExtendedWizardExtraAddedColumnsInfo>> AddedColumnsForMissingTable 
					{ get { return addedColumnsForMissingTable; } set { addedColumnsForMissingTable = value; } }

		///<summary>
		/// Constructor
		///</summary>
		//---------------------------------------------------------------------
		public DBObjectsLoader(IBasePathFinder aPathFinder, DBMSType aDbmsType)
		{
			pathFinder = aPathFinder;
			dbmsType = aDbmsType;
		}

		///<summary>
		/// Entry-point per il riempimento delle strutture contenenti le info degli oggetti di database
		/// di un modulo applicativo passato come parametro
		///</summary>
		//---------------------------------------------------------------------
		public bool LoadModuleDbObjects(IBaseModuleInfo aBaseModuleInfo)
		{
			if (aBaseModuleInfo == null)
				return false;

			currentModuleInfo = aBaseModuleInfo;

			// se il modulo non ha signature o un numero di release valido lo skippo
			if (String.IsNullOrEmpty(aBaseModuleInfo.ModuleConfigInfo.Signature) || aBaseModuleInfo.ModuleConfigInfo.Release <= 0)
				return false;

			// x ogni app+modulo carico i file dbxml e li leggo per estrapolare le info che devo mettere nel bin
			string dbxmlPath = aBaseModuleInfo.GetDatabaseScriptPath();
			if (String.IsNullOrEmpty(dbxmlPath) || !Directory.Exists(dbxmlPath))
				return false;

			foreach (string fileName in Directory.GetFiles(dbxmlPath, "*.dbxml"))
				Parse(fileName);

			return true;
		}

		//---------------------------------------------------------------------
		public bool LoadModuleDbObjects(string applicationName, string moduleName)
		{
			if (String.IsNullOrEmpty(applicationName) || String.IsNullOrEmpty(moduleName))
				return false;

			if (pathFinder == null)
				throw new Exception("Null PathFinder.");

			IBaseApplicationInfo appInfo = pathFinder.GetApplicationInfoByName(applicationName);
			if (appInfo == null)
				return false;

			return LoadModuleDbObjects(appInfo.GetModuleInfoByName(moduleName));
		}

		//---------------------------------------------------------------------
		public bool LoadApplicationDbObjects(IBaseApplicationInfo appInfo)
		{
			if (appInfo == null || appInfo.Modules == null || appInfo.Modules.Count == 0)
				return true;

			bool rc = true;
			foreach (IBaseModuleInfo aBaseModuleInfo in appInfo.Modules)
				rc &= LoadModuleDbObjects(aBaseModuleInfo);

			return rc;
		}

		//---------------------------------------------------------------------
		public bool LoadApplicationDbObjects(string applicationName)
		{
			if (String.IsNullOrEmpty(applicationName))
				return false;

			if (pathFinder == null)
				throw new Exception("Null PathFinder.");

			return LoadApplicationDbObjects(pathFinder.GetApplicationInfoByName(applicationName));
		}

		///<summary>
		/// Entry-point per il riempimento delle strutture contenenti le info degli oggetti di database
		/// di una lista di applicazioni passate come parametro
		///</summary>
		//---------------------------------------------------------------------
		public bool LoadApplicationsDbObjects(StringCollection applicationsList)
		{
			if (pathFinder == null)
				throw new Exception("Null PathFinder.");

			if (applicationsList == null || applicationsList.Count == 0)
				return true;

			bool rc = true;
			foreach (string appName in applicationsList)
				rc &= LoadApplicationDbObjects(appName);

			return rc;
		}

		///<summary>
		/// Entry-point per il riempimento delle strutture contenenti le info degli oggetti di database
		///	questo metodo carica tutte TaskBuilder + TaskBuilderApplication
		///</summary>
		//---------------------------------------------------------------------
		public bool LoadAllDbObjects()
		{
			if (pathFinder == null)
				return false;

			// array di supporto per avere l'elenco totale delle AddOnApplications, caricando prima quelle
			// di TB e poi ERP e verticali
			StringCollection supportList = new StringCollection();
			StringCollection applicationsList = new StringCollection();

			// prima guardo le AddOn di TaskBuilder
			pathFinder.GetApplicationsList(ApplicationType.TaskBuilder, out supportList);
			applicationsList = supportList;

			// poi guardo le AddOn di TaskBuilderApplications (ERP e verticali)
			pathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);

			for (int i = 0; i < supportList.Count; i++)
				applicationsList.Add(supportList[i]);
			//

			return LoadApplicationsDbObjects(applicationsList);
		}

		///<summary>
		/// Parse di un file dbxml
		/// Ricerca nel file la sintassi degli oggetti riconosciuti e riempie dei Dictionary suddivisi x tipo
		///</summary>
		//---------------------------------------------------------------------
		private void Parse(string filePath)
		{
			if (!File.Exists(filePath))
				return;

			try
			{
				// istanzio un DOM dove carico il file
				dbxmlDocument.Load(filePath);
			}
			catch (XmlException)
			{
				return;
			}

			// cerco le tabelle 
			// P.S. il secondo parametro mi serve per stabilire se vanno create o meno 
            // le colonne obbligatorie TBCreated, TBModified, TBCreatedID e TBModifiedID (solo per le applicazioni non di tipo TaskBuilderNet)
			//---------------------------
			IList<WizardTableInfo> ti = projParser.ParseTablesInfoNode
				(
				dbxmlDocument, 
				currentModuleInfo.ParentApplicationInfo.ApplicationType != ApplicationType.TaskBuilderNet
				);
			
			if (ti != null && ti.Count > 0)
			{
				foreach (WizardTableInfo table in ti)
				{
					if (tablesDictionary.ContainsKey(table.Name))
					{
						//TODODIAGNOSTICA
						continue;
					}

					ExtendedWizardTableInfo extWizTableInfo = new ExtendedWizardTableInfo(table, currentModuleInfo, filePath);
					tablesDictionary.Add(table.Name, extWizTableInfo);
				}
			}

			// cerco le view
			//---------------------------
			SqlViewList svl = projParser.ParseViewsInfoNode(dbxmlDocument);
			if (svl != null && svl.Count > 0)
			{
				foreach (SqlView sv in svl)
				{
					if (viewsDictionary.ContainsKey(sv.Name))
					{
						//TODODIAGNOSTICA
						continue;
					}

					ExtendedSqlView extSqlView = new ExtendedSqlView(sv, currentModuleInfo, filePath);
					viewsDictionary.Add(sv.Name, extSqlView);
				}
			}

			// cerco le procedure
			//---------------------------
			SqlProcedureList spl = projParser.ParseProceduresInfoNode(dbxmlDocument);
			if (spl != null && spl.Count > 0)
			{
				foreach (SqlProcedure sp in spl)
				{
					if (proceduresDictionary.ContainsKey(sp.Name))
					{
						//TODODIAGNOSTICA
						continue;
					}

					ExtendedSqlProcedure extSqlProcedure = new ExtendedSqlProcedure(sp, currentModuleInfo, filePath);
					proceduresDictionary.Add(sp.Name, extSqlProcedure);
				}
			}

			// cerco le addoncolumns
			//---------------------------
			IList<WizardExtraAddedColumnsInfo> addCols = projParser.ParseExtraAddedColumnsInfo(dbxmlDocument);
			if (addCols != null && addCols.Count > 0)
			{
				IList<ExtendedWizardExtraAddedColumnsInfo> singleAddColumnsList = new List<ExtendedWizardExtraAddedColumnsInfo>();

				for (int i = 0; i < addCols.Count; i++)
				{
					WizardExtraAddedColumnsInfo extraAddedColumns = addCols[i] as DBObjectsExtraAddedColumnsInfo;

					if (extraAddedColumns == null)
					{
						//TODODIAGNOSTICA
						continue;
					}

					if (extraAddedColumns.ColumnsCount > 1)
					{
						foreach (WizardTableColumnInfo col in extraAddedColumns.ColumnsInfo)
						{
							WizardExtraAddedColumnsInfo extraAddedColumnInfo =
								new DBObjectsExtraAddedColumnsInfo(extraAddedColumns.TableNameSpace, extraAddedColumns.TableName);

							extraAddedColumnInfo.AddColumnInfo(col);

							ExtendedWizardExtraAddedColumnsInfo extAddColumnInfo =
								new ExtendedWizardExtraAddedColumnsInfo(extraAddedColumnInfo, currentModuleInfo, filePath);
							singleAddColumnsList.Add(extAddColumnInfo);
						}
					}
					else
					{
						ExtendedWizardExtraAddedColumnsInfo extAddColumnInfo =
							new ExtendedWizardExtraAddedColumnsInfo(extraAddedColumns, currentModuleInfo, filePath);
						singleAddColumnsList.Add(extAddColumnInfo);
					}
				}

				IList<ExtendedWizardExtraAddedColumnsInfo> colsForTableList;

				foreach (ExtendedWizardExtraAddedColumnsInfo weaci in singleAddColumnsList)
				{
					if (addedColumnsDictionary.TryGetValue(weaci.ExtraAddedColumnsInfo.TableName, out colsForTableList))
						colsForTableList.Add(weaci);
					else
					{
						colsForTableList = new List<ExtendedWizardExtraAddedColumnsInfo>();
						colsForTableList.Add(weaci);
						addedColumnsDictionary.Add(weaci.ExtraAddedColumnsInfo.TableName, colsForTableList);
					}
				}
			}

			// cerco gli update
			//---------------------------
			IList<TableUpdate> tu = projParser.ParseTableUpdate(dbxmlDocument);

			if (tu != null && tu.Count > 0)
			{
				IList<ExtendedTableUpdate> updatesForTableList;
				ExtendedTableUpdate extTableUpdate;

				foreach (TableUpdate tableUpdate in tu)
				{
					extTableUpdate = new ExtendedTableUpdate(tableUpdate, currentModuleInfo, filePath);

					if (updatesDictionary.TryGetValue(tableUpdate.TableName, out updatesForTableList))
						updatesForTableList.Add(extTableUpdate);
					else
					{
						updatesForTableList = new List<ExtendedTableUpdate>();
						updatesForTableList.Add(extTableUpdate);
						updatesDictionary.Add(tableUpdate.TableName, updatesForTableList);
					}
				}
			}

			// cerco gli script di after
			//---------------------------
			IList<TBAfterScript> tas = projParser.ParseTBAfterScript(dbxmlDocument);

			if (tas != null && tas.Count > 0)
			{
				IList<ExtendedTBAfterScript> tbScriptForTable = new List<ExtendedTBAfterScript>();
				ExtendedTBAfterScript extTBAfterScript;

				foreach (TBAfterScript tbAfter in tas)
				{
					extTBAfterScript = new ExtendedTBAfterScript(tbAfter, currentModuleInfo, filePath);
					tbScriptForTable.Add(extTBAfterScript);

				}
				tbAfterDictionary.Add(filePath, tbScriptForTable);
			}
		}

		# region Sorting DbObjects methods
		///<summary>
		/// Metodo che si occupa di scorrere tutte le tabelle e, in base ad eventuali dipendenze da altre
		/// tabelle (FK, indici), richiama l'analisi di queste in ricorsione e riempie una lista ordinata
		/// di oggetti (non usiamo un Dictionary perchè si auto-ordina)
		///</summary>
		//---------------------------------------------------------------------
		public void SortDbObjects()
		{
			try
			{
				// scorro il dictionary delle tabelle e analizzo le sue dipendenze
				foreach (KeyValuePair<string, ExtendedWizardTableInfo> kvp in tablesDictionary)
				{
					recursivedTables.Clear();
					AnalyzeSingleTable(kvp.Value);
				}
			}
			catch (StackOverflowException)
			{ }
		}

		///<summary>
		/// Metodo ricorsivo che analizza ogni singola tabella e le eventuali sue dipendenze
		/// (controllando le tabelle referenziate dalle sue FK e dagli indici)
		///</summary>
		//---------------------------------------------------------------------
		private void AnalyzeSingleTable(ExtendedWizardTableInfo extendedTable)
		{
			WizardTableInfo table = extendedTable.WizardTableInfo;
			if (table == null)
				return;

			// se la tabella che sto analizzando è già presente nella lista delle ricorsività
			// non procedo per non entrare in ricorsione e relativo overflow
			if (recursivedTables.Contains(table.Name))
				return;

			recursivedTables.Add(table.Name);

			// se la tabella ha delle FK analizzo le tabelle referenziate e controllo nella lista
			// degli oggetti ordinati se esiste già
			if (table.ForeignKeysCount > 0 && table.ForeignKeys != null)
			{
				foreach (WizardForeignKeyInfo wfki in table.ForeignKeys)
				{
					// se la FK é aggiunto a se stesso lo skippo
					if (string.Compare(wfki.ReferencedTableName, table.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;

					// se la lista contiene già la tabella referenziata dalla FK
					// aggiungo la tabella analizzata alla lista ordinata (se già non la contiene)
					// e continuo ad analizzare le eventuali altre FK
					if (!sortedTablesList.Contains(wfki.ReferencedTableName))
					{
						ExtendedWizardTableInfo fkWti;
						if (tablesDictionary.TryGetValue(wfki.ReferencedTableName, out fkWti))
							AnalyzeSingleTable(fkWti);
					}
				}
			}

			if (table.Indexes != null && table.Indexes.Count > 0)
			{
				foreach (WizardTableIndexInfo wtii in table.Indexes)
				{
					// se l'indice é aggiunto a se stesso lo skippo
					if (string.Compare(wtii.TableName, table.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;

					if (!sortedTablesList.Contains(wtii.TableName))
					{
						ExtendedWizardTableInfo fkWti;
						if (tablesDictionary.TryGetValue(wtii.TableName, out fkWti))
							AnalyzeSingleTable(fkWti);
					}
				}
			}

			AddTableInSortedList(extendedTable);
		}

		///<summary>
		/// Aggiunge il nome della tabella (se non già presente) alla lista ordinata
		///</summary>
		//---------------------------------------------------------------------
		private void AddTableInSortedList(ExtendedWizardTableInfo extendedTable)
		{
			// se il modulo corrente appartiene all'applicazione TB (Framework) lo inserisco per primo
			// perchè devo creare la tabella DBMark prima di tutti
			if (string.Compare
				(extendedTable.ExtendendInfo.ApplicationSignature,
				"TB",
				StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				if (!sortedTablesList.Contains(extendedTable.WizardTableInfo.Name))
					sortedTablesList.Insert(0, extendedTable.WizardTableInfo.Name);
			}

			if (!sortedTablesList.Contains(extendedTable.WizardTableInfo.Name))
				sortedTablesList.Add(extendedTable.WizardTableInfo.Name);
		}
		# endregion

		///<summary>
		/// Genera gli statements per tutta la struttura caricata in memoria
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateStatements()
		{
			// genero gli statements per le tabelle (ordinate)
			//---------------------------------------------------------------------------------
			foreach (string tableName in sortedTablesList)
				GenerateTableStatement(tableName);

			// genero gli statements per le ALTER TABLE e relativi UPDATE
			//---------------------------------------------------------------------------------
			foreach (IList<ExtendedWizardExtraAddedColumnsInfo> weaciList in addedColumnsDictionary.Values)
			{
				foreach (ExtendedWizardExtraAddedColumnsInfo weaci in weaciList)
					GenerateExtraAddedColumnStatement(weaci);
			}

			// genero gli statement per gli UPDATE non collegati ad alcuna ALTER TABLE di ExtraAddColumn
			//---------------------------------------------------------------------------------
			if (updatesDictionary.Count > 0)
			{
				foreach (IList<ExtendedTableUpdate> tablesUpdate in updatesDictionary.Values)
					foreach (ExtendedTableUpdate tUpdate in tablesUpdate)
						GenerateTableUpdateStatement(tUpdate);
			}

			// genero gli statements per le VIEW
			//---------------------------------------------------------------------------------
			foreach (ExtendedSqlView sv in viewsDictionary.Values)
				GenerateViewStatement(sv);

			// genero gli statements per le PROCEDURE
			//---------------------------------------------------------------------------------
			foreach (ExtendedSqlProcedure sp in proceduresDictionary.Values)
				GenerateProcedureStatement(sp);

			// leggo tutti gli statement indicati nei tag TBAfterScript e carico la sezione per il dbms che mi occorre
			ArrayList sortedTBAfterScripts = new ArrayList();
			//---------------------------------------------------------------------------------
			foreach (KeyValuePair<string, IList<ExtendedTBAfterScript>> afterScript in tbAfterDictionary)
			{
				IList<ExtendedTBAfterScript> tbScriptForTable = afterScript.Value;

				foreach (ExtendedTBAfterScript tAfter in tbScriptForTable)
					sortedTBAfterScripts.Add(tAfter);

				IComparer comp = new SortExtendedTBAfterScriptList();
				sortedTBAfterScripts.Sort(comp);

				foreach (ExtendedTBAfterScript tAfter in sortedTBAfterScripts)
					GenerateTBAfterScriptStatement(afterScript.Key, tAfter);
			}
		}

		///<summary>
		/// Per identificare l'UPDATE della colonna legata all'ALTER TABLE
		///</summary>
		//----------------------------------------------------------------------------
		private TableUpdate GetUpdate(string tableName, string addedColumnName)
		{
			IList<ExtendedTableUpdate> tablesUpdate;
			TableUpdate update = null;

			if (updatesDictionary.TryGetValue(tableName, out tablesUpdate))
			{
				foreach (ExtendedTableUpdate tu in tablesUpdate)
					if (string.Compare(tu.TableUpdate.SetColumnName, addedColumnName, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						update = tu.TableUpdate;
						break;
					}
			}

			return update;
		}

		///<summary>
		/// Dato il nome di una tabella ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateTableStatement(string tableName)
		{
			ExtendedWizardTableInfo myTableInfo;

			if (tablesDictionary.TryGetValue(tableName, out myTableInfo))
				GenerateTableStatement(myTableInfo);
		}

		///<summary>
		/// Dato il nome di una tabella ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateTableStatement(ExtendedWizardTableInfo myTableInfo)
		{
			DBObjectStatement dbStatement = null;
			string statement = string.Empty;

			if (wizCodeGenerator.GenerateTableCreationStatement(myTableInfo.WizardTableInfo, out statement, this.dbmsType))
			{
				dbStatement = new DBObjectStatement
					(
					myTableInfo.WizardTableInfo.Name,
					DBStatementType.TABLE,
					statement,
					myTableInfo.WizardTableInfo.CreationDbReleaseNumber,
					myTableInfo.ExtendendInfo
					);

				sortedScriptList.Add(dbStatement);
			}
		}

		///<summary>
		/// Dato il nome di una view ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateViewStatement(string viewName)
		{
			ExtendedSqlView sv;
			if (viewsDictionary.TryGetValue(viewName, out sv))
				GenerateViewStatement(sv);
		}

		///<summary>
		/// Dato il nome di una view ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateViewStatement(ExtendedSqlView sv)
		{
			DBObjectStatement dbStatement = null;
			string statement = string.Empty;

			if (wizCodeGenerator.GenerateViewCreationStatement(sv.SqlView, out statement, this.dbmsType))
			{
				dbStatement = new DBObjectStatement
					(
					sv.SqlView.Name,
					DBStatementType.VIEW,
					statement,
					(uint)sv.SqlView.CreationDbReleaseNumber,
					sv.ExtendendInfo
					);

				sortedScriptList.Add(dbStatement);
			}
		}

		///<summary>
		/// Dato il nome di una procedure ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateProcedureStatement(string procedureName)
		{
			ExtendedSqlProcedure sp;
			if (proceduresDictionary.TryGetValue(procedureName, out sp))
				GenerateProcedureStatement(sp);
		}

		///<summary>
		/// Dato il nome di una procedure ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateProcedureStatement(ExtendedSqlProcedure sp)
		{
			DBObjectStatement dbStatement = null;
			string statement = string.Empty;

			if (wizCodeGenerator.GenerateProcedureCreationStatement(sp.SqlProcedure, out statement, this.dbmsType))
			{
				dbStatement = new DBObjectStatement
					(
					sp.SqlProcedure.Name,
					DBStatementType.PROCEDURE,
					statement,
					(uint)sp.SqlProcedure.CreationDbReleaseNumber,
					sp.ExtendendInfo
					);

				sortedScriptList.Add(dbStatement);
			}
		}

		///<summary>
		/// Dato il nome di una tabella + colonna ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateExtraAddedColumnStatement(string tableName, string columnName)
		{
			IList<ExtendedWizardExtraAddedColumnsInfo> addColsList;

			if (addedColumnsDictionary.TryGetValue(tableName, out addColsList))
			{
				foreach (ExtendedWizardExtraAddedColumnsInfo weaci in addColsList)
				{
					if (string.Compare(weaci.ExtraAddedColumnsInfo.ColumnAtZeroIndex.Name, columnName, StringComparison.InvariantCultureIgnoreCase) != 0)
						continue;

					GenerateExtraAddedColumnStatement(weaci);
				}
			}
		}

		///<summary>
		/// Dato il nome di una tabella + colonna ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateExtraAddedColumnStatement(ExtendedWizardExtraAddedColumnsInfo weaci)
		{
			DBObjectStatement dbStatement = null;
			string statement = string.Empty;

			// cerco gli Update specifici della tabella alterata
			TableUpdate currentColUpdate =
				GetUpdate(weaci.ExtraAddedColumnsInfo.TableName, weaci.ExtraAddedColumnsInfo.ColumnAtZeroIndex.Name);

			// se ho trovato l'update lo rimuovo dalla lista complessiva xchè è collegato all'addoncol
			if (currentColUpdate != null)
				updatesDictionary.Remove(currentColUpdate.TableName);

			if (wizCodeGenerator.GenerateAlterTableStatement(weaci.ExtraAddedColumnsInfo, currentColUpdate, out statement, this.dbmsType))
			{
				dbStatement = new DBObjectStatement
					(
					weaci.ExtraAddedColumnsInfo.TableName,
					weaci.ExtraAddedColumnsInfo.ColumnAtZeroIndex.Name,
					DBStatementType.EXTRAADDEDCOLUMN,
					statement,
					weaci.ExtraAddedColumnsInfo.ColumnAtZeroIndex.CreationDbReleaseNumber,
					weaci.ExtendendInfo
					);

				sortedScriptList.Add(dbStatement);
			}
		}

		///<summary>
		/// Dato il nome di un tableupdate ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateTableUpdateStatement(string tableName, string columnName)
		{
			IList<ExtendedTableUpdate> tablesUpdate;
			if (updatesDictionary.TryGetValue(tableName, out tablesUpdate))
			{
				foreach (ExtendedTableUpdate tUpdate in tablesUpdate)
				{
					if (string.Compare(tUpdate.TableUpdate.SetColumnName, columnName, StringComparison.InvariantCultureIgnoreCase) != 0)
						continue;

					GenerateTableUpdateStatement(tUpdate);
				}
			}
		}

		///<summary>
		/// Dato il nome di un tableupdate ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateTableUpdateStatement(ExtendedTableUpdate tUpdate)
		{
			DBObjectStatement dbStatement = null;
			string statement = string.Empty;

			if (wizCodeGenerator.GenerateAlterTableStatement(null, tUpdate.TableUpdate, out statement, this.dbmsType))
			{
				dbStatement = new DBObjectStatement
					(
					tUpdate.TableUpdate.TableName,
					tUpdate.TableUpdate.SetColumnName,
					DBStatementType.TABLEUPDATE,
					statement,
					0,
					tUpdate.ExtendendInfo);

				sortedScriptList.Add(dbStatement);
			}
		}

		///<summary>
		/// Dato il nome di un tableupdate ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateTBAfterScriptStatement(string fileName, int step)
		{
			IList<ExtendedTBAfterScript> sortedTBAfterScripts;

			if (tbAfterDictionary.TryGetValue(fileName, out sortedTBAfterScripts))
			{
				foreach (ExtendedTBAfterScript tAfter in sortedTBAfterScripts)
				{
					if (tAfter.TBAfterScript.Step != step)
						continue;

					GenerateTBAfterScriptStatement(fileName, tAfter);
				}
			}
		}

		///<summary>
		/// Dato il nome di un tableupdate ritorna lo statement di creazione dell'oggetto
		///</summary>
		//---------------------------------------------------------------------
		public void GenerateTBAfterScriptStatement(string fileName, ExtendedTBAfterScript tAfter)
		{
			DBObjectStatement dbStatement = null;
			string statement = string.Empty;

			statement = tAfter.TBAfterScript.GetValueForDBMSType(this.dbmsType == DBMSType.SQLSERVER);
			// come identificare univocamente un TBAfterScript?
			// il numero di step DEVE ESSERE unico per modulo
			dbStatement = new DBObjectStatement
				(
				fileName,
				DBStatementType.TBAFTERSCRIPT,
				statement,
				0,
				tAfter.ExtendendInfo
				);

			dbStatement.Step = (uint)tAfter.TBAfterScript.Step;
			sortedScriptList.Add(dbStatement);
		}
	}
}
