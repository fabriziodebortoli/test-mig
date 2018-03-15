using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Microarea.Common.DiagnosticManager;
using Microarea.ProvisioningDatabase.Libraries.DatabaseManager;
using TaskBuilderNetCore.Interfaces;

using static Microarea.Common.Generic.InstallationInfo;
using static Microarea.ProvisioningDatabase.Libraries.DataManagerEngine.ImportSelections;

namespace Microarea.ProvisioningDatabase.Libraries.DataManagerEngine
{
	/// <summary>
	/// ImportMng gestore delle operazioni di import dei dati
	/// </summary>
	//=========================================================================
	public class ImportManager
	{
		// formato DateTime da utilizzare come parametro in XmlConvert.ToDateTime
		// N.B. senza indicazione dei millisecondi per problemi di conversione tra tipi di dati (introdotta con il .NET 2.0)
		private const string SQLDateTimeFormat = "yyyy-MM-ddTHH:mm:ss"; //@@TODOMICHI non piu' utilizzato, vedere se va bene

		# region Variabili
		public ImportSelections importSel;
		private ContextInfo contextInfo;
		private DatabaseDiagnostic dbDiagnostic;

		private Diagnostic checkDbDiagnostic = new Diagnostic("ImportManager");
		private CheckDBStructureInfo checkDBStructInfo;

		private bool isAppendFile = false;
		private bool toPrepareUpdate = false;
		private bool fromUpdate = false;
		private bool tbModifiedIsPresent = false; // x sapere se la colonna TBModified è presente nel file xml

		// servono per la propagazione dell'opzione overwrite sui singoli file (in caso di upgrade)
		private bool importDataForUpgrade = false;
		private UpdateExistRowType updateExistRowTypeForUpgrade = UpdateExistRowType.SKIP_ROW;
		//

		private string currentTable = string.Empty;
		private bool currentTableIsMaster = false; // la tabella e' di tipo master (gestione TBGuid introdotta con la 4.0)
		private string currentDateTimeOnServer = string.Empty;
		private string currentFileName = string.Empty;
		private string currentFullName = string.Empty;
		private bool isExportDataFileName = false;

		private StringCollection tableKeys = new StringCollection();
		private StringCollection fkOnTables = new StringCollection(); //lista delle tabelle in cui sono stati disabilitati il CHECK delle FK

		//tabelle che hanno dato errori dovuti alla loro struttura x cui non + da processare
		private StringCollection errorTables = new StringCollection();

		private XmlReader reader;
		private TBConnection connection;
		private IDbTransaction currentTransaction;
		private TBCommand insertCommand;
		private TBCommand updateCommand;
		private TBCommand updateTBGuidCommand;
		
		private CatalogTableEntry catalogEntry;

		// variabili di comodo per tenere traccia degli errori...
		private bool noExistTable = false;
		private string tagTableName = string.Empty;
		private bool showMsg = false;
		# endregion

		# region Costruttore
		//---------------------------------------------------------------------------
		public ImportManager(ImportSelections impSel, DatabaseDiagnostic diagnostic)
		{
			importSel = impSel;
			contextInfo = impSel.ContextInfo;
			dbDiagnostic = diagnostic;

			// carico le info delle tabelle
			importSel.LoadModuleTableInfo(false);
			checkDBStructInfo = new CheckDBStructureInfo(KindOfDatabase.Company, importSel.ContextInfo, importSel.AppDBStructInfo, ref checkDbDiagnostic);
			
			dbDiagnostic.SetGenericText(string.Format(DataManagerEngineStrings.MsgImportCompanyData, contextInfo.CompanyName));
		}
		# endregion

		# region Entry-point per l'importazione dati (con thread separato)
		//---------------------------------------------------------------------
		public Thread Import()
		{
			Thread myThread = new Thread(new ParameterizedThreadStart(InternalImport));
			myThread.Start(false);
			return myThread;
		}

		/// <summary>
		/// per l'esecuzione del processo di import
		/// </summary>
		//---------------------------------------------------------------------
		public void InternalImport(object importDefaultDataForUpgrade = null)
		{
			// per gestire l'opzione di overwrite dei dati sul singolo file in caso di upgrade
			importDataForUpgrade = (importDefaultDataForUpgrade != null) ? (bool)importDefaultDataForUpgrade : false;

			bool error = false;

			if (importSel.ImportList.Count == 0)
				return; //@@TODO DIAGNOSTIC ERRORE

			errorTables.Clear();

			try
			{
				//istanzio una nuova connessione di lavoro
				connection = new TBConnection(contextInfo.ConnectAzDB, contextInfo.DbType);
				connection.Open();

				if (importSel.ErrorRecovery != ImportSelections.TypeRecovery.CONTINUE && !connection.IsPostgreConnection())
					currentTransaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);

				// visualizzo il msg solo se non è elaborazione silente
				if (!importSel.IsSilent)
					dbDiagnostic.SetMessageNoAppAndModuleName
						(
						true,
						string.Empty,
						DataManagerEngineStrings.StartDataLoadingFromXml,
						DataManagerEngineStrings.InProgress,
						string.Empty
						);

				// mi tengo da parte la lista dei file di Append in modo da eseguirli alla fine
				// (per evitare sovrascritture dei dati operate dai file ancestor eseguiti successivamente
				// contestualmente al flag di elimina contenuto della tabella) // OFM + ACM
				List<FileInfo> appendFilesList = new List<FileInfo>();

				foreach (ImportItemInfo item in importSel.ImportList)
				{
					if (error)
						break;

					foreach (ImportItem ii in item.SelectedFiles)
					{
						string file = ii.File;
						updateExistRowTypeForUpgrade = (importDataForUpgrade)
							? (ii.Overwrite) ? UpdateExistRowType.UPDATE_ROW : UpdateExistRowType.SKIP_ROW
							: importSel.UpdateExistRow;

						// skippo i file con un nome che inizia con DeploymentManifest (visto che potrebbe essere
						// presente per esigenze di installazione di specifiche configurazioni dati di default/esempio 
						// (esigenza sorta principalmente con i partner polacchi - miglioria 3067)
						if (file.StartsWith("DeploymentManifest", StringComparison.OrdinalIgnoreCase))
							continue;

						if (!importSel.IsSilent &&
							Path.GetFileNameWithoutExtension(file).EndsWith(DataManagerConsts.Append, StringComparison.OrdinalIgnoreCase))
						{
							// lista dei file di Append da eseguire dopo
							appendFilesList.Add(new FileInfo(Path.Combine(item.PathName, file)));
							continue;
						}

						if (
							!ImportSingleFile(new FileInfo(Path.Combine(item.PathName, file))) &&
							importSel.ErrorRecovery == ImportSelections.TypeRecovery.STOP_LAST_FILE_ROLLBACK
							)
							error = true;

						if (dbDiagnostic.AbortWizard)
							break;
					}
				}

				// alla fine eseguo i file di Append tenuti da parte
				if (!importSel.IsSilent && appendFilesList.Count > 0)
				{
					foreach (FileInfo fi in appendFilesList)
					{
						if (!ImportSingleFile(fi) && importSel.ErrorRecovery == ImportSelections.TypeRecovery.STOP_LAST_FILE_ROLLBACK)
							error = true;
					}
				}

				if (currentTransaction != null && !connection.IsPostgreConnection())
				{
					if (error)
						currentTransaction.Rollback();
					else
						currentTransaction.Commit();
				}

				error = RestoreCheckInAllTable();
			}
			catch (Exception e)
			{
				Debug.WriteLine("class ImportManager method InternalImport: " + e.Message);
			}
			finally
			{
				connection.Close();
				connection.Dispose();

			}

			dbDiagnostic.SetFinish((dbDiagnostic.AbortWizard) ? DataManagerEngineStrings.MsgOperationInterrupted : DataManagerEngineStrings.MsgEndOperation);

			// se si tratta di importazione silente (ovvero contestuale alla creazione del db) non procedo con il salvataggio del log
			if (importSel.IsSilent)
				return;

			// salvo il file di log
			string filePath = this.importSel.CreateLogFile(dbDiagnostic);

			// scrivo la riga con il riferimento al file di log salvato
			dbDiagnostic.SetMessageNoAppAndModuleName(true, Path.GetFileName(filePath), DatabaseManagerStrings.CreateLogFile, string.Empty, filePath);
		}
		# endregion

		# region Funzioni dell'algoritmo di importazione dati
		//---------------------------------------------------------------------------
		private bool ImportSingleFile(FileInfo importFile)
		{
			tbModifiedIsPresent = false;

			if (dbDiagnostic.AbortWizard)
				return false;

			if (!importFile.Exists)
			{
				dbDiagnostic.SetMessageNoAppAndModuleName
					(
					false,
					importFile.Name,
					string.Empty,
					DataManagerEngineStrings.ErrFileNotExist,
					importFile.FullName
					);
				return true; //@@ERRORE
			}

			isExportDataFileName =
				string.Compare(importFile.Name, DataManagerConsts.ExportDataFileName, StringComparison.OrdinalIgnoreCase) == 0;

			if (isExportDataFileName)
				dbDiagnostic.SetMessageNoAppAndModuleName
					(
					true,
					importFile.Name,
					DataManagerEngineStrings.NotAvailable,
					DataManagerEngineStrings.LoadDataFromFile,
					importFile.FullName
					);

			showMsg = false;

			string currentApplication = string.Empty;
			string currentModule = string.Empty;

			if (!importSel.LoadXmlToFileSystem)
			{
				// dal path completo del file da importare ricavo i nomi dell'applicazione e del modulo
				currentApplication = Functions.GetDirectoryAncestor(importFile.DirectoryName, 6);
				currentModule = Functions.GetDirectoryAncestor(importFile.DirectoryName, 5);

				int lastLocation = currentApplication.LastIndexOf(Path.DirectorySeparatorChar);
				if (lastLocation >= 0)
					currentApplication = currentApplication.Substring(lastLocation + 1);
				lastLocation = currentModule.LastIndexOf(Path.DirectorySeparatorChar);
				if (lastLocation >= 0)
					currentModule = currentModule.Substring(lastLocation + 1);
				//
			}

			// controllo nel file se sono presenti gli attributi di reference e carico il file linkato (se esiste)
			bool importResult = LoadReferenceInformation(ref importFile, currentApplication, currentModule);

			currentFileName = importFile.Name;
			currentFullName = importFile.FullName;

			isAppendFile = Path.GetFileNameWithoutExtension(importFile.Name).EndsWith(DataManagerConsts.Append, StringComparison.OrdinalIgnoreCase);

			SynchronizeDBServerDateTime();
			
			try
			{
				//using (StreamReader sr = File.OpenText(importFile.FullName))
				using (Stream sr = this.contextInfo.PathFinder.GetStream(importFile.FullName, false))
				{
					reader = XmlReader.Create(sr, new XmlReaderSettings() { IgnoreWhitespace = true, CloseInput = true });

					while (reader.Read() && importResult)
					{
						if (reader.NodeType == XmlNodeType.Element)
						{
							if (reader.Name.CompareTo(DataManagerConsts.DataTables) == 0)
							{
								// se l'elemento DataTables è vuoto e non sono presenti gli attributi di reference
								// dò un messaggio di errore e non procedo
								if (reader.IsEmptyElement)
								{
									dbDiagnostic.SetMessage
										(
										false,
										currentApplication,
										currentModule,
										importFile.Name,
										tagTableName,
										DataManagerEngineStrings.ErrNoDataInFile,
										importFile.FullName
										);
									return true;
								}

								bool isOptional = (reader.MoveToAttribute(DataManagerConsts.Optional) &&
									string.Compare(reader.Value, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0);

								// se sono in fase di caricamento dati di default e l'utente
								// non vuole caricare quelli opzionali allora skippo il file
								if (importSel.NoOptional)
								{
									if (isOptional)
										return true;
								}
								else
									if (!isOptional)
										return true;

								continue;
							}

							if (reader.Name.CompareTo(DataManagerConsts.Schema) == 0)
								reader.Skip(); //skip the schema

							// leggo i dati presenti nel file xml
							importResult = ReadData();
						}
					}

					// se era un caso in cui la tabella non esisteva ho già dato in precedenza un messaggio di errore.
					if (!noExistTable)
					{
						// se il nome del file analizzato è ExportData.xml, 
						// allora non rendo disponibile il nome della tabella
						if (isExportDataFileName)
							dbDiagnostic.SetMessage
								(
								importResult,
								currentApplication,
								currentModule,
								importFile.Name,
								DataManagerEngineStrings.NotAvailable,
								(importResult) ? DataManagerEngineStrings.OK : DataManagerEngineStrings.SyntaxError,
								importFile.FullName
								);
						else
						{
							if (!showMsg) // se ho già mostrato un msg prima non faccio vedere questa riga
							{
								dbDiagnostic.SetMessage
									(
									importResult,
									currentApplication,
									currentModule,
									importFile.Name,
									Path.GetFileNameWithoutExtension(importFile.FullName),
									(importResult) ? DataManagerEngineStrings.OK : DataManagerEngineStrings.SyntaxError,
									importFile.FullName
									);
							}
						}
					}
					else
					{
						if (reader.Name.CompareTo(DataManagerConsts.DataTables) == 0)
						{
							string tName = Path.GetFileNameWithoutExtension(importFile.FullName);
							dbDiagnostic.SetMessageNoAppAndModuleName
								(
								true,
								importFile.Name,
								tName,
								string.Format(DataManagerEngineStrings.MsgNotExistTable, tName),
								importFile.FullName
								);
						}
						else
							dbDiagnostic.SetMessageNoAppAndModuleName
								(
								true,
								importFile.Name,
								tagTableName,
								DataManagerEngineStrings.UnknownDBObject,
								importFile.FullName
								);
					}

					dbDiagnostic.SetImportFileCounter(importFile.Name, currentApplication, currentModule);
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("class ImportManager method ImportSingleFile:" + e.Message);
				dbDiagnostic.SetMessage(false, currentApplication, currentModule, importFile.Name, currentTable, e.Message, importFile.FullName);
				return false;
			}
			finally
			{
				if (reader != null)
					reader.Dispose();
				reader = null;
			}

			if (importResult)
			{
				// faccio la commit se lo spazio transazionale é per file e faccio
				// ripartire un'altra transazione per il file successivo
				if (
					importSel.ErrorRecovery == ImportSelections.TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK ||
					importSel.ErrorRecovery == ImportSelections.TypeRecovery.STOP_LAST_FILE_ROLLBACK
					)
				{
                    if (currentTransaction != null && !connection.IsPostgreConnection())
						currentTransaction.Commit();
					importResult = RestoreCheckInAllTable();
                    if (!connection.IsPostgreConnection())
					    currentTransaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
				}
			}
			else // se l'importazione non é andata a buon fine
			{
				// faccio il roolback dell'ultimo file e continuo
				if (importSel.ErrorRecovery == ImportSelections.TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK)
				{
                    if (currentTransaction != null && !connection.IsPostgreConnection())
						currentTransaction.Rollback();
					importResult = RestoreCheckInAllTable();
                    if (!connection.IsPostgreConnection())
					    currentTransaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
				}
			}

			return importResult;
		}

		///<summary>
		/// Dato un file da importare, devo controllare al suo interno se sono presenti gli attributi
		/// per la gestione dei reference tra file xml.
		/// Prima controllo l'attributo hasreference=true per procedere
		/// Poi leggo refconfiguration. se non esiste o contiene stringa vuota non procedo.
		/// Leggo refedition. se non esiste o contiene stringa vuota considero l'edizione corrente con cui sta
		/// girando il programma
		/// A questo punto ri-compongo il path con il contenuto dei nuovi attributi e, solo se esiste su file system,
		/// sovrascrivo quello che mi e' stato passato come parametro
		///</summary>
		//---------------------------------------------------------------------------
		private bool LoadReferenceInformation(ref FileInfo importFile, string currentApplication, string currentModule)
		{
			FileInfo fiOriginal = importFile;

			string refEdition = string.Empty;
			string refConfiguration = string.Empty;
			string refCountry = string.Empty;

			XmlReader xtr = null;

			try
			{
				//using (StreamReader sr = File.OpenText(importFile.FullName))
				using (Stream sr = this.contextInfo.PathFinder.GetStream(importFile.FullName, false))
				{
					xtr = XmlReader.Create(sr, new XmlReaderSettings() { IgnoreWhitespace = true });

					while (xtr.Read())
					{
						if (xtr.NodeType == XmlNodeType.Element)
						{
							// cerco il nodo DataTables
							if (xtr.Name.CompareTo(DataManagerConsts.DataTables) == 0)
							{
								// se l'attributo hasreference non esiste ritorno subito
								if (!xtr.MoveToAttribute(DataManagerConsts.HasReference))
									return true;
								// se l'attributo hasreference e' impostato a false ritorno subito
								if (xtr.Value == bool.FalseString)
									return true;

								// se l'attributo refconfiguration e' vuoto segnalo errore
								if (xtr.MoveToAttribute(DataManagerConsts.RefConfiguration))
								{
									if (string.IsNullOrEmpty(xtr.Value))
										return false;
									else
										refConfiguration = xtr.Value;
								}
								else
									return false;

								// se l'attributo refedition e' vuoto considero l'edizione attuale
								if (xtr.MoveToAttribute(DataManagerConsts.RefEdition))
									refEdition = string.IsNullOrEmpty(xtr.Value) ? importSel.ContextInfo.PathFinder.Edition : xtr.Value;
								else
									refEdition = importSel.ContextInfo.PathFinder.Edition;

								// se l'attributo refcountry e' vuoto o non esiste lascio stare cosi (altrimenti saltano i dati INTL - fix bug 26436)
								if (xtr.MoveToAttribute(DataManagerConsts.RefCountry))
									refCountry = string.IsNullOrEmpty(xtr.Value) ? string.Empty : xtr.Value;

								// devo ricalcolare il file da tornare in modo da pilotare l'importazione
								// C:\<nome-istanza>\Standard\Applications\ERP\Accounting\DataManager\Default\ ed eventualmente la country (se presente)
								string absoluteDirName = Functions.GetDirectoryAncestor(importFile.DirectoryName, string.IsNullOrWhiteSpace(refCountry) ? 2 : 3);

								string newImportFilePath = string.Empty;

								if (!string.IsNullOrWhiteSpace(refCountry))
									newImportFilePath = refCountry;

								newImportFilePath = Path.Combine(newImportFilePath, refEdition);
								newImportFilePath = Path.Combine(newImportFilePath, refConfiguration);
								newImportFilePath = Path.Combine(newImportFilePath, Path.GetFileName(importFile.FullName));

								// alla fine compongo tutto il path
								newImportFilePath = Path.Combine(absoluteDirName, newImportFilePath);

								// se il file con il nuovo path ricalcolato esiste ri-assegno il valore....
								// okkio che non va in ricorsione, pertanto se nel nuovo file e' specificato un altro reference il file verra' poi scartato!
								if (File.Exists(newImportFilePath))
									importFile = new FileInfo(newImportFilePath);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("class ImportManager method LoadReferenceInformation:" + e.Message);
				dbDiagnostic.SetMessage(false, currentApplication, currentModule, fiOriginal.Name, currentTable, e.Message, fiOriginal.FullName);
				return false;
			}
			finally
			{
				if (xtr != null)
					xtr.Dispose();
				xtr = null;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private bool ReadData()
		{
			bool read = true;
			while (read)
			{
				if (dbDiagnostic.AbortWizard)
					break;

				if (!InsertRow() && importSel.ErrorRecovery != ImportSelections.TypeRecovery.CONTINUE)
					return false;
				else
					read = reader.Read() &&
							reader.NodeType == XmlNodeType.Element &&
							reader.Name.CompareTo(DataManagerConsts.DataTables) != 0;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private string GetParamName(string param)
		{
			return "@" + param.Replace(' ', '_');
		}

		/// <summary>
		/// per la gestione delle colonne di tipo DateTime e l'inizializzazione della colonna TBModified
		/// </summary>
		//---------------------------------------------------------------------------
		private DateTime GetDateTimeValue(TBParameter tbParam)
		{
            try
			{
				// caso particolare per la gestione della colonna base TBModified solo in caso di UpdateRow
				// Se sul file xml esiste ma l'utente ha specificato che non la vuole importare
				// devo cmq valorizzarla con la data odierna.
                string colTbModified = null;
                if (contextInfo.DbType == DBMSType.SQLSERVER)
                    colTbModified = DatabaseLayerConsts.TBModifiedColNameForSql;
                else if (contextInfo.DbType == DBMSType.POSTGRE)
                    colTbModified = DatabaseLayerConsts.TBModifiedColNameForPostgre;

                if (tbParam.ParameterName.IndexOf(colTbModified) > 0 && fromUpdate)
				{
					if (importSel.ImportTBModified)
					{
						// se l'utente ha scelto di importare la colonna TBModified ma sul file non esiste,
						// devo inizializzarla con la data corrente senza considerare il value del reader
						if (!tbModifiedIsPresent)
							return Convert.ToDateTime(currentDateTimeOnServer);

						if (!string.IsNullOrWhiteSpace(reader.Value))
						{
							if (importSel.UseUtcDateTimeFormat)
								return XmlConvert.ToDateTime(reader.Value, XmlDateTimeSerializationMode.Utc);
							else
								return ConvertDateTimeValue(reader.Value);
						}
						else
							return Convert.ToDateTime(currentDateTimeOnServer);
					}
					else
						return Convert.ToDateTime(currentDateTimeOnServer);
				}

                // se la data e' vuota ritorno subito
                if (string.IsNullOrWhiteSpace(reader.Value))
                    return Common.CoreTypes.ObjectHelper.NullTbDateTime;

				// se la data è vuota (ovvero 1799-12-31T00:00:00.00000000 per TaskBuilder),
				// non devo considerare il formato Utc, anche se l'utente ha scelto quest'opzione.
				if (reader.Value.IndexOf("1799-12-31") > 0)
					return ConvertDateTimeValue(reader.Value, true);

				if (importSel.UseUtcDateTimeFormat)
					return XmlConvert.ToDateTime(reader.Value, XmlDateTimeSerializationMode.Utc);
				else
					return ConvertDateTimeValue(reader.Value);
			}
            catch (Exception)
            { }

			// nel caso si verifica qualche eccezione ritorno la classica data vuota
			return Common.CoreTypes.ObjectHelper.NullTbDateTime;
		}

		//---------------------------------------------------------------------------
		private DateTime ConvertDateTimeValue(string value, bool emptyDate)
		{
			// formato di esempio data esportando dalla Console: 1799-12-31T00:00:00.0000000+01:00
			// formato di esempio data esportando dal C++: 1799-12-31T00:00:00
			// devo cercare i valori + e - SOLO DOPO la T
			int tIdx = value.IndexOf("T");

			if (value.IndexOf("+", tIdx) > 0) // cerco il + dopo la T
			{
				if (value.IndexOf(".") > 0)
					return XmlConvert.ToDateTime(value.Substring(0, value.IndexOf(".")), XmlDateTimeSerializationMode.Utc);// SQLDateTimeFormat);

				return XmlConvert.ToDateTime(value.Substring(0, value.IndexOf("+")), XmlDateTimeSerializationMode.Utc);// SQLDateTimeFormat);
			}

			if (value.IndexOf("-", tIdx) > 0) // cerco il - dopo la T
			{
				if (value.IndexOf(".") > 0)
					return XmlConvert.ToDateTime(value.Substring(0, value.IndexOf(".")), XmlDateTimeSerializationMode.Utc);// SQLDateTimeFormat);

				return XmlConvert.ToDateTime(value.Substring(0, value.LastIndexOf("-")), XmlDateTimeSerializationMode.Utc);// SQLDateTimeFormat);
			}

			if (emptyDate)
				return XmlConvert.ToDateTime(reader.Value, XmlDateTimeSerializationMode.Utc);// SQLDateTimeFormat);

			return Convert.ToDateTime(currentDateTimeOnServer);
		}

		//---------------------------------------------------------------------------
		private DateTime ConvertDateTimeValue(string value)
		{
			return ConvertDateTimeValue(value, false);
		}

		//---------------------------------------------------------------------------
		private bool SetParameters(TBCommand tbCommand)
		{
			bool exist = reader.MoveToFirstAttribute();

			TBParameter tbParam = null;
			string colName = null;
			foreach (IDataParameter param in tbCommand.Parameters)
			{
				tbParam = tbCommand.GetTBParameter(param);

				//tolgo dal nome del parametro @ e ottengo il nome della colonna
				colName = tbParam.ParameterName.Substring(1);

				if (!reader.MoveToAttribute(colName))
				{
					// se si tratta della colonna TBModified, anche se non presente nel file procedo cmq
					// xchè imposto la data d'ufficio...
                    string tbModified = null;
					if (contextInfo.DbType == DBMSType.SQLSERVER)
						tbModified = DatabaseLayerConsts.TBModifiedColNameForSql;
					else if (contextInfo.DbType == DBMSType.POSTGRE)
                        tbModified = DatabaseLayerConsts.TBModifiedColNameForPostgre;

                    if (string.Compare(colName, tbModified, StringComparison.OrdinalIgnoreCase) == 0)
					{
						tbParam.Value = GetDateTimeValue(tbParam);
						continue;
					}

					// se si tratta di una tabella master e nel file non e' presente la colonna TBGuid 
					// la aggiungo d'ufficio e continuo
					if (currentTableIsMaster)
					{
						tbParam.SetParameterValue(Guid.NewGuid().ToString().ToUpperInvariant());
						continue;
					}

					//ERRORE nel logging
					dbDiagnostic.SetMessageNoAppAndModuleName
						(
						false,
						currentFileName,
						currentTable,
						string.Format(DataManagerEngineStrings.MsgNotPresentColumn, "reader.LineNumber", colName), //@@TODOMICHI non ho piu' il nr linea
						currentFullName
						);

					showMsg = true;
					return false;
				}

				// per le colonne di tipo DateTime faccio dei controlli ulteriori relativamente alla sintassi 
				// presenti nell'xml (+2.00, etc) e se necessario utilizzo la XmlConvert.ToDateTime
				// passando come parametro XmlDateTimeSerializationMode.Utc, in modo da non perdere l'info del fuso orario
				if (tbParam.DbType == DbType.Date || tbParam.DbType == DbType.Time || tbParam.DbType == DbType.DateTime)
				{
					tbParam.Value = GetDateTimeValue(tbParam);
					continue;
				}

				string columnValue = reader.Value;

				// gestione colonna TBGuid per ovviare al problema delle colonne null: se nell'attributo TBGuid e' specificato un valore non valido ne assegno uno d'ufficio
				if (string.Compare(colName, DatabaseLayerConsts.TBGuidColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (
						string.Compare(columnValue, "00000000-0000-0000-0000-000000000000", StringComparison.OrdinalIgnoreCase) == 0 ||
						string.Compare(columnValue, "00000000-0000-0000-0008-000000000000", StringComparison.OrdinalIgnoreCase) == 0
						)
						columnValue = Guid.NewGuid().ToString().ToUpperInvariant();
				}

				tbParam.SetParameterValue(columnValue);
			}

			return true;
		}

		/// <summary>
		/// Permette di disabilitare/abilitare il check dei constraints di FK 
		/// - propri della tabella (quando vengono inseriti nuovi dati)
		/// - che riferisco alla tabella di cui si vuole cancellare il contenuto prima di importare i dati
		/// </summary>
		/// <param name="enable"> se abilitare o meno il check dei constraints</param>
		/// <param name="refTable">true : se si devono considerare i constraints riferiti
		///		false : se si devono considerare i constraints propri</param>
		//---------------------------------------------------------------------------
		private void NoCheckFKConstraints(bool refTable)
		{
			if (string.IsNullOrWhiteSpace(currentTable) || catalogEntry == null)
				return;

			if (catalogEntry.FKConstraintsInfo == null || catalogEntry.RefFKConstraints == null)
				return;

			CatalogTableEntry refCatEntry = null;
			try
			{

                if (contextInfo.Connection.IsPostgreConnection())
                {
                    catalogEntry.DisableCheckOnForeignKey(connection);
                    return;
                }

				//a seconda del flag devo disabilitare gli eventuali constraint che referenziano alla tabella
				// o quelli definiti sulla tabella
				if (refTable)
					foreach (RefFKConstraint refFKConst in catalogEntry.RefFKConstraints)
					{
						refCatEntry = importSel.Catalog.GetTableEntry(refFKConst.FKTableName);
						if (refCatEntry != null && !fkOnTables.Contains(refFKConst.FKTableName))
						{
							fkOnTables.Add(refFKConst.FKTableName);
							refCatEntry.DisableCheckOnForeignKey(contextInfo.Connection);
						}
					}
				else
				{
					if (!fkOnTables.Contains(catalogEntry.TableName))
					{
						fkOnTables.Add(catalogEntry.TableName);
						catalogEntry.DisableCheckOnForeignKey(contextInfo.Connection);
					}
				}
			}
			catch (TBException)
			{
				throw;
			}
		}

		///<summary>
		/// RestoreCheckInAllTable
		/// Ri-abilita i constraint di FK
		///</summary>
		//---------------------------------------------------------------------
		private bool RestoreCheckInAllTable()
		{
			bool errorFound = false;

            if (contextInfo.Connection.IsPostgreConnection())
            {
                try
                {
                    catalogEntry.EnableCheckOnForeignKey(connection, false, false);
                    return true;
                }
                catch (TBException e)
                {
                    Debug.WriteLine("class ImportManager method RestoreCheckInAllTable: " + e.Message);
                    dbDiagnostic.SetMessageNoAppAndModuleName(false, currentFileName, currentTable, e.Message, currentFullName);
                    errorFound = true;
                    return false;
                }
            }

			foreach (string tableFK in fkOnTables)
			{
				try
				{
					CatalogTableEntry catEntry = importSel.Catalog.GetTableEntry(tableFK);
					if (catEntry != null)
						catEntry.EnableCheckOnForeignKey(contextInfo.Connection, false, false);
				}
				catch (TBException e)
				{
					Debug.WriteLine("class ImportManager method RestoreCheckInAllTable: " + e.Message);
					dbDiagnostic.SetMessageNoAppAndModuleName(false, currentFileName, currentTable, e.Message, currentFullName);
					errorFound = true;
					continue;
				}
			}

			fkOnTables.Clear();
			return !errorFound;
		}

		//---------------------------------------------------------------------------
		private bool ChangeTable()
		{
			try
			{
				tableKeys.Clear();
				catalogEntry = importSel.Catalog.GetTableEntry(tagTableName);
				noExistTable = false;

				if (catalogEntry == null)
				{
					noExistTable = true;
					return false;
				}

				if (isExportDataFileName)
					dbDiagnostic.SetMessageNoAppAndModuleName
						(
						true,
						currentFileName,
						tagTableName,
						DataManagerEngineStrings.OK,
						currentFullName
						);

				/* deve lavorare sulla connessione del contesto mentre la procedura lavora su una propria connessione
				   vengono caricate le informazioni relative alle:
					- colonne
					- chiave primarie
					- foreign key definite sulla tabella */

				currentTable = tagTableName;
				currentTableIsMaster = false;

				// controllo che lo stato della connessione sia buono... se non è così la riapro
				if (contextInfo.Connection.State == ConnectionState.Closed)
				{
					contextInfo.Connection = new TBConnection(contextInfo.ConnectAzDB, contextInfo.DbType);
					contextInfo.Connection.Open();
				}

				// gestione TBGuid (M. 5936) devo verificare se la tabella che sto processando e' una mastertable
				// in questo caso devo aggiungere d'ufficio la colonna TBGuid (se non specificata nel file xml) oppure,
				// se e' presente, mettere un guid valido in caso di valore NULL o empty
				ModuleDBInfo moduleDBInfo = null;
                if (checkDBStructInfo.GetApplicationModuleInfo(currentTable, out moduleDBInfo))
                {
                    EntryDBInfo entryDBInfo = moduleDBInfo.TablesList.Find(a => string.Compare(a.Name, currentTable, StringComparison.OrdinalIgnoreCase) == 0);
					if (entryDBInfo != null && entryDBInfo.MasterTable)
						currentTableIsMaster = true;
				}

				// carico le colonne
				catalogEntry.LoadColumnsInfo(contextInfo.Connection, true);
				// leggo solo i nomi delle FK eventualmente presenti sulla tabella
				catalogEntry.LoadFKConstraintsName(contextInfo.Connection);
				// leggo le FK che eventualmente referenziano la tabella corrente
				catalogEntry.LoadRefFKConstraints(contextInfo.Connection);
				// leggo le PK
				catalogEntry.GetPrimaryKeys(ref tableKeys);

				string keyName = string.Empty;
				foreach (string key in tableKeys)
				{
					// non posso usare if (reader.GetAttribute(key) == null) poichè Oracle restituisce i nomi delle
					// colonne maiuscole mentre il metodo GetAttribute è case sensitive
					bool next = reader.MoveToFirstAttribute();

					bool exist = false;
					string missingKeys = string.Empty;
					while (next && !exist)
					{
						keyName = XmlConvert.DecodeName(reader.Name);
						exist = string.Compare(keyName, key, StringComparison.OrdinalIgnoreCase) == 0;
						next = reader.MoveToNextAttribute();
						if (!exist)
							missingKeys += (keyName + " "); 
					}

					// non ho trovato un segmento di chiave dentro il file non posso continuare
					if (!exist)
					{
						//ERRORE nel logging
						dbDiagnostic.SetMessageNoAppAndModuleName
							(
							false,
							missingKeys,
							currentTable,
							string.Format(DataManagerEngineStrings.MsgMissingKeyFields, currentTable),
							currentFullName
							);
						showMsg = true;
						return false;
					}
				}

				//controllo ed eventualmente disabilito i constraints definiti sulla tabella
				if (catalogEntry.FKConstraintsInfo != null && catalogEntry.FKConstraintsInfo.Count > 0)
				{
					if (!importSel.DisableCheckFK)
					{
						//errore nel logging
						dbDiagnostic.SetMessageNoAppAndModuleName
							(
							false,
							currentFileName,
							currentTable,
							string.Format(DataManagerEngineStrings.MsgPresentFK, currentTable),
							currentFullName
							);
						showMsg = true;
						return false;
					}

					//devo disabilitare gli eventuali constraint presenti nella tabella
					NoCheckFKConstraints(false);
				}

				// cancello il contenuto della tabella se voluto dall'utente (se non sto importando un file append)
				if (importSel.DeleteTableContext && !isAppendFile)
				{
					if (!importSel.DisableCheckFK && catalogEntry.RefFKConstraints.Count > 0)
					{
						// errore nel logging
						dbDiagnostic.SetMessageNoAppAndModuleName
							(
							false,
							currentFileName,
							currentTable,
							string.Format(DataManagerEngineStrings.MsgReferencedFKTable1, currentTable),
							currentFullName
							);
						showMsg = true;
						return false;
					}

					// devo disabilitare gli eventuali constraint che riferiscono alla tabella
					NoCheckFKConstraints(true);

					string deleteCommand = string.Format(contextInfo.DbType == DBMSType.SQLSERVER
											? "DELETE from [{0}]" : "DELETE from {0}", currentTable);

					TBCommand deleteTbCommand = new TBCommand(deleteCommand, connection);

                    if (currentTransaction != null && !connection.IsPostgreConnection())
                        deleteTbCommand.Transaction = currentTransaction;

					deleteTbCommand.ExecuteNonQuery();
					dbDiagnostic.ProgressBarStep();
				}
			}
			catch (TBException e)
			{
				//da inserire nel log dei messaggi
				dbDiagnostic.SetMessageNoAppAndModuleName(false, currentFileName, currentTable, e.Message, currentFullName);
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private bool CheckRow()
		{
			if (reader.HasAttributes == false)
				return false;

			if (errorTables.Contains(XmlConvert.DecodeName(reader.Name)))
				return false;

			/* se ho cambiato tabella allora devo: 
				- effettuare i controlli sulal struttura della riga che sia compatibile
				- con la struttura della tabella
				- riprepararmi la query
				- eventualmente chiudere la transazione corrente e ripartirne una nuova
				- disabilitare i check sui constraints di FK */

			tagTableName = XmlConvert.DecodeName(reader.Name);

			// effettuo la preparazione della query se:
			// è cambiata la tabella
			// oppure il numero di attibuti della riga corrente è differente rispetto ai parametri (ad esempio
			// in Oracle i valori nulli non sono esportati)

			if (string.IsNullOrWhiteSpace(currentTable) || tagTableName.CompareTo(currentTable) != 0)
			{
				if (!(ChangeTable() && PrepareInsertQuery()))
				{
					errorTables.Add(tagTableName);
					currentTable = string.Empty;
					return false;
				};
			}
			else
			{
				if (insertCommand != null)
				{
					// ulteriore controllo per risolvere l'anomalia di Fabio -- loop sui nomi dei parametri
					if (reader.AttributeCount == insertCommand.Parameters.Count)
					{
						TBParameter tbParam = null;
						string colName = null;

						if (PrepareInsertQuery())
						{
							foreach (IDataParameter param in insertCommand.Parameters)
							{
								tbParam = insertCommand.GetTBParameter(param);
								//tolgo dal nome del parametro @ e ottengo il nome della colonna
								colName = tbParam.ParameterName.Substring(1);

								// skippo il check della colonna TBGuid perche' potrebbe essere presente nei parametri del command
								// ma non comparire nell'xml
								if (currentTableIsMaster && string.Compare(colName, DatabaseLayerConsts.TBGuidColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
									continue;

								if (!reader.MoveToAttribute(colName))
								{
									//ERRORE nel logging
									dbDiagnostic.SetMessageNoAppAndModuleName
										(
										false,
										currentFileName,
										currentTable,
										string.Format(DataManagerEngineStrings.MsgNotPresentColumn, "reader.LineNumber", colName), //@@TODOMICHI non ho piu' il nr linea
										currentFullName
										);
									return false;
								}
							}
						}
					}

					if (reader.AttributeCount != insertCommand.Parameters.Count && !PrepareInsertQuery())
						return false;

					// puoi capitare che la stessa tabella sia presente come prima nel file successivo
					// in questo caso la query è possibile che non venga ripreparata 
					// É necessario eventualmente modificare la transazione			
                    if (insertCommand.Transaction != currentTransaction && !connection.IsPostgreConnection())
                        insertCommand.Transaction = currentTransaction;
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private bool IsExistRowError(TBException e)
		{
			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
					{
						if (e.Errors == null)
							return false;

						for (int j = 0; j < e.Errors.Count; j++)
						{
							// il numero dell'errore che corrisponde a chiave duplicata é 2627
							if (e.Errors[j].Number == 2627)
								return true;
						}
						break;
					}

                case DBMSType.POSTGRE:
                    return e.Message.Contains("23505");
			}

			return false;
		}

		//---------------------------------------------------------------------------
		private bool InsertRow()
		{
			if (!(CheckRow() && SetParameters(insertCommand)))
				return false; //ERRORE

			try
			{
				insertCommand.ExecuteNonQuery();
				dbDiagnostic.ProgressBarStep();
			}
			catch (TBException e)
			{
				//Debug.WriteLine(string.Format("Exception handled in ImportManager::InsertRow ({0})", e.Message));
				StringBuilder errorBuilder = new StringBuilder();
				errorBuilder.AppendFormat(DataManagerEngineStrings.MsgImportInsertRow, "reader.LineNumber.ToString()", currentFullName); //@@TODOMICHI non ho piu' il nr linea

				if (IsExistRowError(e))
				{
					// se l'utente mi ha detto di effettuare l'update di un record giá esistente
					// allora non visualizzo errore ma effettuo il comando di update
					switch ((importDataForUpgrade) ? updateExistRowTypeForUpgrade : importSel.UpdateExistRow)
					{
						case ImportSelections.UpdateExistRowType.UPDATE_ROW:
							{
								bool updated = UpdateRow();
								if (updated)
									UpdateRowOnlyForTBGuid();
								return updated;
							}
						// se ho scelto di skippare la riga cmq vado ad aggiornare i TBGuid nulli per le tabelle master
						case ImportSelections.UpdateExistRowType.SKIP_ROW:
							{
								UpdateRowOnlyForTBGuid();
								return true;
							}
						case ImportSelections.UpdateExistRowType.SKIP_ROW_ERROR:
							{
								UpdateRowOnlyForTBGuid();
								break;
							}
					}
				}
				errorBuilder.AppendFormat(DataManagerEngineStrings.MsgErrorNumberMessage, e.Message, e.Number);

				if (errorBuilder.ToString().Length > 0)
				{
					dbDiagnostic.SetError(errorBuilder.ToString());
					if (e.Number == 2627) // Cannot insert duplicate key in object 
						dbDiagnostic.SetMessageNoAppAndModuleName
							(
							false,
							currentFileName,
							currentTable,
							string.Format(DataManagerEngineStrings.MsgDuplicateKey, "reader.LineNumber"), //@@TODOMICHI non ho piu' il nr linea
							currentFullName
							);
					else
						dbDiagnostic.SetMessageNoAppAndModuleName
							(
							false,
							currentFileName,
							currentTable,
							string.Format(DataManagerEngineStrings.MsgErrorAtLineNumber, e.Message, "reader.LineNumber"), //@@TODOMICHI non ho piu' il nr linea
							currentFullName
							);

					showMsg = true;
				}

				// metto a null il Command, perche' potrebbe essere stato invalidato
				insertCommand = null;

				return false;
			}

			return true;
		}

		/// <summary>
		/// costruisce la query preparata di INSERT INTO tableName (field1, field2, field3) values (?, ?, ?)
		/// ed effettua il binding dei parametri
		/// </summary>
		//---------------------------------------------------------------------------
		private bool PrepareInsertQuery()
		{
			StringBuilder sqlInsert = new StringBuilder(); // INSERT INTO tablename 
			StringBuilder sqlColumns = new StringBuilder(); //(col1, col2.., coln)
			StringBuilder sqlValues = new StringBuilder(); // VALUES (?, ?, ?...)

			if (insertCommand == null)
				insertCommand = new TBCommand(connection);
			else
				insertCommand.Parameters.Clear();

            if (currentTransaction != null && !connection.IsPostgreConnection())
                insertCommand.Transaction = currentTransaction;

			sqlInsert.Append(string.Format
				(
				(contextInfo.DbType == DBMSType.SQLSERVER) ? "INSERT INTO [{0}] ( " : "INSERT INTO {0} ( ", currentTable)
				);

			sqlValues.Append(" ) VALUES ( ");

			bool exist = reader.MoveToFirstAttribute();
			bool identityColFound = false;
			bool tbGuidColFoundInXml = false;
			int i = 0;

			CatalogColumn colInfo = null;
			string param = null;
			string colName = string.Empty;

			while (exist)
			{
				colName = XmlConvert.DecodeName(reader.Name);
				colInfo = catalogEntry.GetColumnInfo(colName);

				// se la colonna non esiste e l'utente mi ha detto di non considerare le
				// colonne non esistenti allora continuo, altrimenti errore
				if (colInfo == null)
				{
					if (importSel.InsertExtraFieldsRow)
					{
						exist = reader.MoveToNextAttribute();
						continue;
					}
					else
						return false; //@@ERRORE
				}

				// tengo traccia se leggo nel file il nome di una colonna che è di tipo Identity
				// ma devo farlo solo una volta!!! (anomalia 18150)
				if (colInfo.IsAutoIncrement && !identityColFound)
					identityColFound = colInfo.IsAutoIncrement;

				if (string.Compare(colName, DatabaseLayerConsts.TBGuidColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
					tbGuidColFoundInXml = true;

				if (i > 0)
				{
					sqlColumns.Append(',');
					sqlValues.Append(',');
				}

				// colonna TBCreated
                string tbCreated = null;
				if (contextInfo.DbType == DBMSType.POSTGRE)
                    tbCreated = DatabaseLayerConsts.TBCreatedColNameForPostgre;
                else
                    tbCreated = DatabaseLayerConsts.TBCreatedColNameForSql;

                if (string.Compare(colName, tbCreated, StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (!importSel.ImportTBCreated)
					{
						exist = reader.MoveToNextAttribute();
						sqlColumns.Remove(sqlColumns.Length - 1, 1);
						sqlValues.Remove(sqlValues.Length - 1, 1);
						continue;
					}
				}
				
				// colonna TBModified
                string tbModified;          
                if (contextInfo.DbType == DBMSType.POSTGRE)
                    tbModified = DatabaseLayerConsts.TBModifiedColNameForPostgre;
                else
                    tbModified = DatabaseLayerConsts.TBModifiedColNameForSql;
				if (string.Compare(colName, tbModified, StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (!importSel.ImportTBModified)
					{
						exist = reader.MoveToNextAttribute();
						sqlColumns.Remove(sqlColumns.Length - 1, 1);
						sqlValues.Remove(sqlValues.Length - 1, 1);
						continue;
					}
				}

                if (colName.Equals("Offset") && contextInfo.DbType == DBMSType.POSTGRE)
                    sqlColumns.Append("\"offset\"");
                else if (colName.Equals("Action") && contextInfo.DbType == DBMSType.POSTGRE)
                    sqlColumns.Append("\"action\"");
                else if (colName.Equals("Limit") && contextInfo.DbType == DBMSType.POSTGRE)
                    sqlColumns.Append("\"limit\"");
                else
                    sqlColumns.Append(string.Format((contextInfo.DbType == DBMSType.SQLSERVER) ? "[{0}]" : "{0}", colName));
                
                param = GetParamName(reader.Name);
				insertCommand.Parameters.Add(param, colInfo.ProviderType, (int)colInfo.ColumnSize);
				sqlValues.Append(param);
				i++;
				exist = reader.MoveToNextAttribute();
			}

			// se si tratta di una tabella master e non ho trovato nel file la colonna TBGuid la metto d'ufficio
			if (currentTableIsMaster && !tbGuidColFoundInXml)
			{
				string tbGuidColName = (contextInfo.DbType == DBMSType.SQLSERVER) ? DatabaseLayerConsts.TBGuidColNameForSql : DatabaseLayerConsts.TBGuidColNameForOracle;
				colInfo = catalogEntry.GetColumnInfo(tbGuidColName);
				if (colInfo != null)
				{
					sqlColumns.Append(',');
					sqlColumns.Append(string.Format((contextInfo.DbType == DBMSType.SQLSERVER) ? "[{0}]" : "{0}", tbGuidColName));
					param = GetParamName(tbGuidColName);
					insertCommand.Parameters.Add(param, colInfo.ProviderType, (int)colInfo.ColumnSize);
					sqlValues.Append(',');
					sqlValues.Append(param);
				}
			}

			sqlInsert.Append(sqlColumns.ToString());
			sqlInsert.Append(sqlValues.ToString());
			sqlInsert.Append(')');

			// solo in SQL, se la tabella contiene una colonna di tipo IDENTITY aggiungo allo script di INSERT
			// anche la proprietà IDENTITY_INSERT prima ad ON e poi OFF
			if (contextInfo.DbType == DBMSType.SQLSERVER && identityColFound)
			{
				sqlInsert.Insert(0, string.Format(DatabaseLayerConsts.SetIdentityInsertON, catalogEntry.TableName));
				sqlInsert.Append(string.Format(DatabaseLayerConsts.SetIdentityInsertOFF, catalogEntry.TableName));
			}

			insertCommand.CommandText = sqlInsert.ToString();

			try
			{
				insertCommand.Prepare();  // Calling Prepare after having set the Commandtext and parameters.
			}
			catch (TBException e)
			{
				dbDiagnostic.SetMessageNoAppAndModuleName(false, currentFileName, currentTable, e.Message, currentFullName);
				return false;
			}

			toPrepareUpdate = (importDataForUpgrade)
							? (updateExistRowTypeForUpgrade == ImportSelections.UpdateExistRowType.UPDATE_ROW)
							: (importSel.UpdateExistRow == ImportSelections.UpdateExistRowType.UPDATE_ROW);
			return true;
		}

		//---------------------------------------------------------------------------
		private bool UpdateRow()
		{
			fromUpdate = true;

			if (toPrepareUpdate && !PrepareUpdateQuery())
			{
				fromUpdate = false;
				return false; //@@ERRORE
			}

			// se sono nel caso di file con solo colonne segmenti di chiave non effettuo l'update e non segnalo errore		
			if (string.IsNullOrWhiteSpace(updateCommand.CommandText))
			{
				fromUpdate = false;
				return true;
			}

			if (!SetParameters(updateCommand))
			{
				fromUpdate = false;
				return false; //@@ERRORE
			}

			try
			{
				updateCommand.ExecuteNonQuery();
				dbDiagnostic.ProgressBarStep();
				fromUpdate = false;
			}
			catch (TBException e)
			{
				fromUpdate = false;
				dbDiagnostic.SetMessageNoAppAndModuleName(false, currentFileName, currentTable, e.Message, currentFullName);
				return false;
			}

			return true;
		}

		/// <summary>
		/// costruisce la query preparata di 
		/// UPDATE tableName SET field1 = ?, field2 = ?, field3 = ? WHERE key1 = ?, key2 = ?
		/// ed effettua il binding dei parametri viene chiamata quando fallisce l'inserimento della row 
		/// per chiave duplicata ovvero la row esiste giá
		/// </summary>
		//---------------------------------------------------------------------------
		private bool PrepareUpdateQuery()
		{
			StringBuilder sqlUpdate = new StringBuilder();	// UPDATE tablename 
			StringBuilder sqlSet = new StringBuilder();		// field1 = ?...
			StringBuilder sqlWhere = new StringBuilder();	// key1 = ?...
            string updateColNameTb = string.Empty;

			string tbGuidValueInXml = Guid.NewGuid().ToString().ToUpperInvariant(); // assegno gia' d'ufficio un guid valido

			if (updateCommand == null)
				updateCommand = new TBCommand(connection);
			else
				updateCommand.Parameters.Clear();

            if (currentTransaction != null && !connection.IsPostgreConnection())
                updateCommand.Transaction = currentTransaction;

			sqlUpdate.Append(string.Format((contextInfo.DbType == DBMSType.SQLSERVER) ? "UPDATE [{0}] SET " : "UPDATE {0} SET ", currentTable));

			bool exist = reader.MoveToFirstAttribute();
			int i = 0;

			CatalogColumn colInfo = null;
			string param = string.Empty;
			string colName = string.Empty;

			while (exist)
			{
                colName = contextInfo.DbType == DBMSType.POSTGRE ? XmlConvert.DecodeName(reader.Name).ToLower() : XmlConvert.DecodeName(reader.Name);

                colInfo = catalogEntry.GetColumnInfo(colName);

                if (colName.Equals("Offset") && contextInfo.DbType == DBMSType.POSTGRE)
                    colName = "\"offset\"";
                else if (colName.Equals("Action") && contextInfo.DbType == DBMSType.POSTGRE)
                    colName = "\"action\"";
                else if (colName.Equals("Limit") && contextInfo.DbType == DBMSType.POSTGRE)
                    colName = "\"limit\"";

				// se la colonna non esiste e l'utente mi ha detto di non considerare le
				// colonne non esistenti allora continuo, altrimenti errore
				if (colInfo == null)
				{
					if (importSel.InsertExtraFieldsRow)
					{
						exist = reader.MoveToNextAttribute();
						continue;
					}
					else
						return false;
				}

				// se sono in UPDATE e si tratta di una tabella master skippo il TBGuid specificato nel file
				// perche' sulla tabella potrebbe gia' esserci un valore valido e NON devo sovrascriverlo
				// la gestione dell'update della sola colonna TBGuid viene gestita in un metodo a parte
				if (currentTableIsMaster && string.Compare(colName, DatabaseLayerConsts.TBGuidColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
				{
					exist = reader.MoveToNextAttribute();
					continue;
				}

				param = GetParamName(reader.Name);       
                if (contextInfo.DbType == DBMSType.SQLSERVER)
                    updateColNameTb = DatabaseLayerConsts.TBCreatedColNameForSql;
                else if (contextInfo.DbType == DBMSType.POSTGRE)
                    updateColNameTb = DatabaseLayerConsts.TBCreatedColNameForPostgre;
				// colonna TBCreated
				if (string.Compare(colName, updateColNameTb, StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (!importSel.ImportTBCreated)
					{
						exist = reader.MoveToNextAttribute();
						continue;
					}
				}

                if (contextInfo.DbType == DBMSType.SQLSERVER)
                    updateColNameTb = DatabaseLayerConsts.TBModifiedColNameForSql;
                else if (contextInfo.DbType == DBMSType.POSTGRE)
                    updateColNameTb = DatabaseLayerConsts.TBModifiedColNameForPostgre;
                // colonna TBModified
				if (string.Compare(colName, updateColNameTb, StringComparison.OrdinalIgnoreCase) == 0)
					tbModifiedIsPresent = true; // significa che l'ho trovata nel file

				// é un segmento di chiave allora lo devo mettere nella WHERE clause
				if (colInfo.IsKey)
				{
					if (sqlWhere.Length > 0)
						sqlWhere.Append(" AND ");

					sqlWhere.Append(string.Format((contextInfo.DbType == DBMSType.SQLSERVER) ? "[{0}]" : "{0}", colName));
					sqlWhere.Append(" = ");
					sqlWhere.Append(param);
				}
				else
				{
					if (sqlSet.Length > 0)
						sqlSet.Append(", ");
					sqlSet.Append(string.Format((contextInfo.DbType == DBMSType.SQLSERVER) ? "[{0}]" : "{0}", colName));
					sqlSet.Append(" = ");
					sqlSet.Append(param);
				}

				updateCommand.Parameters.Add(param, colInfo.ProviderType, colInfo.ColumnSize);
				i++;
				exist = reader.MoveToNextAttribute();
			}

			// se la colonna TBModified non è stata trovata nel file devo aggiungerla a mano, xchè cmq deve
			// essere valorizzata
			if (!tbModifiedIsPresent)
			{
                if (contextInfo.DbType == DBMSType.SQLSERVER)
                    updateColNameTb = DatabaseLayerConsts.TBModifiedColNameForSql;
                else if (contextInfo.DbType == DBMSType.POSTGRE)
                    updateColNameTb = DatabaseLayerConsts.TBModifiedColNameForPostgre;

                colInfo = catalogEntry.GetColumnInfo(updateColNameTb);
				if (colInfo != null)
				{
                    param = GetParamName(updateColNameTb);

					if (sqlSet.Length > 0)
						sqlSet.Append(", ");

                    sqlSet.Append
                        (string.Format(contextInfo.DbType == DBMSType.SQLSERVER ? "[{0}]" : "{0}", updateColNameTb));

					sqlSet.Append(" = ");
					sqlSet.Append(param);
					updateCommand.Parameters.Add(param, colInfo.ProviderType, colInfo.ColumnSize);
				}
			}

			// se il file ha solo colonne segmenti di chiave allora non effettuo l'update (non avrebbe senso effettuare
			// UPDATE tableName SET key1 = value1, key2 = value2 WHERE key1 = value1 AND key2 = value2)
			if (sqlSet.ToString().Length == 0)
			{
				updateCommand.CommandText = string.Empty;
				toPrepareUpdate = false;
				return true;
			}

			sqlUpdate.Append(sqlSet.ToString());
			sqlUpdate.Append(" WHERE ");
			sqlUpdate.Append(sqlWhere.ToString());

			updateCommand.CommandText = sqlUpdate.ToString();
			updateCommand.Prepare();  // Calling Prepare after having set the Commandtext and parameters.
			toPrepareUpdate = false;

			return true;
		}

		///<summary>
		/// DA ESEGUIRE SOLO PER LE MASTER TABLE
		/// Utilizzata per eseguire l'update della sola colonna TBGuid nel caso in cui
		/// sia impostata con un valore non valido
		///</summary>
		//---------------------------------------------------------------------------
		private bool UpdateRowOnlyForTBGuid()
		{
			if (!currentTableIsMaster)
				return true;

			// preparo la query con la SET della sola colonna TBGuid e nella WHERE i soli campi chiave
			if (!PrepareUpdateTBGuidQuery())
				return true; 

			if (!string.IsNullOrWhiteSpace(updateTBGuidCommand.CommandText))
			{
				if (!SetParameters(updateTBGuidCommand))
					return true;

				try
				{
					updateTBGuidCommand.ExecuteNonQuery();
					dbDiagnostic.ProgressBarStep();
				}
				catch (TBException e)
				{
					dbDiagnostic.SetMessageNoAppAndModuleName(false, currentFileName, currentTable, e.Message, currentFullName);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// costruisce la query preparata di 
		/// UPDATE [tableName] SET [TBGuid] = '012FC3EC-19E8-4CAC-AB0D-33CE5FABAC4D' 
		//  WHERE key1 = ?, key2 = ? 
		//  AND (TBGuid IS NULL OR TBGuid = 0x00 OR TBGuid = '00000000-0000-0000-0008-000000000000')
		/// </summary>
		//---------------------------------------------------------------------------
		private bool PrepareUpdateTBGuidQuery()
		{
			StringBuilder sqlUpdate = new StringBuilder();	// UPDATE tablename SET TBGuid = xxx
			StringBuilder sqlWhere = new StringBuilder();	// key1 = ?...

			string tbGuidValueInXml = Guid.NewGuid().ToString().ToUpperInvariant(); // assegno gia' d'ufficio un guid valido

			if (updateTBGuidCommand == null)
				updateTBGuidCommand = new TBCommand(connection);
			else
				updateTBGuidCommand.Parameters.Clear();

			if (currentTransaction != null && !connection.IsPostgreConnection())
				updateTBGuidCommand.Transaction = currentTransaction;

			if (contextInfo.DbType == DBMSType.SQLSERVER)
				sqlUpdate.Append(string.Format("UPDATE [{0}] SET [{1}] = '{2}' ", currentTable, DatabaseLayerConsts.TBGuidColNameForSql, tbGuidValueInXml));
			else
				sqlUpdate.Append(string.Format("UPDATE {0} SET {1} = '{2}' ", currentTable, DatabaseLayerConsts.TBGuidColNameForOracle, tbGuidValueInXml));

			bool exist = reader.MoveToFirstAttribute();
	
			while (exist)
			{
				string colName = XmlConvert.DecodeName(reader.Name);
				CatalogColumn colInfo = catalogEntry.GetColumnInfo(colName);

				// se la colonna non esiste e l'utente mi ha detto di non considerare le
				// colonne non esistenti allora continuo, altrimenti errore
				if (colInfo == null)
				{
					if (importSel.InsertExtraFieldsRow)
					{
						exist = reader.MoveToNextAttribute();
						continue;
					}
					else
						return false;
				}

				// se sono in UPDATE leggo il TBGuid specificato nel file ma lo assegno solo se e' necessario
				// perche' sulla tabella potrebbe gia' esserci un valore valido e NON devo sovrascriverlo
				if (string.Compare(colName, DatabaseLayerConsts.TBGuidColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
				{
					tbGuidValueInXml = reader.Value;
					// se il valore nel file e' nullo ne assegno uno nuovo in automatico
					if (
						string.Compare(tbGuidValueInXml, "00000000-0000-0000-0000-000000000000", StringComparison.OrdinalIgnoreCase) == 0 ||
						string.Compare(tbGuidValueInXml, "00000000-0000-0000-0008-000000000000", StringComparison.OrdinalIgnoreCase) == 0
						)
						tbGuidValueInXml = Guid.NewGuid().ToString().ToUpperInvariant();
				}

				// considero solo i segmenti di chiave da mettere nella WHERE clause
				if (colInfo.IsKey)
				{
					string param = GetParamName(reader.Name);

					if (sqlWhere.Length > 0)
						sqlWhere.Append(" AND ");

					sqlWhere.Append(string.Format((contextInfo.DbType == DBMSType.SQLSERVER) ? "[{0}]" : "{0}", colName));
					sqlWhere.Append(" = ");
					sqlWhere.Append(param);

					// aggiungo i parametri delle chiavi per la query di update del tbguid
					updateTBGuidCommand.Parameters.Add(param, colInfo.ProviderType, colInfo.ColumnSize);
				}

				exist = reader.MoveToNextAttribute();
			}

			sqlUpdate.Append(" WHERE ");
			sqlUpdate.Append(sqlWhere.ToString());

			// poi aggiungo la clausola sul TBGuid
			if (contextInfo.DbType == DBMSType.SQLSERVER)
				sqlUpdate.Append(string.Format(" AND ([{0}] IS NULL OR [{0}] = 0x00 OR [{0}] = '00000000-0000-0000-0008-000000000000')", DatabaseLayerConsts.TBGuidColNameForSql));
			else
				sqlUpdate.Append(string.Format(" AND ({0} IS NULL OR {0} = '0x00' OR {0} = '00000000-0000-0000-0008-000000000000')", DatabaseLayerConsts.TBGuidColNameForOracle));

			updateTBGuidCommand.CommandText = sqlUpdate.ToString();
			updateTBGuidCommand.Prepare();

			return true;
		}

		/// <summary>
		/// metodo che mi serve per fare una query al server che mi ritorni la CurrenDate...
		/// da utilizzare per aggiornare correttamente il TBModified.
		/// si usa GetDate() per SQLServer
		/// </summary>
		//---------------------------------------------------------------------------
		private void SynchronizeDBServerDateTime()
		{
			string query = string.Empty;

			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
					query = "SELECT getdate()";
					break;
                case DBMSType.POSTGRE:
                    query = "SELECT now()";
                    break;
			}

			try
			{
				using (TBCommand comm = new TBCommand(query, connection))
				{
					if (currentTransaction != null && !connection.IsPostgreConnection())
						comm.Transaction = currentTransaction;

					using (IDataReader reader = comm.ExecuteReader())
						while (reader.Read())
							currentDateTimeOnServer = reader[0].ToString();
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message, "SynchronizeDBServerDateTime()");
			}
		}
		# endregion
	}
}
