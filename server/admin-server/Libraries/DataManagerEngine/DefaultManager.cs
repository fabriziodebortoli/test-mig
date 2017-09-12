using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.NameSolver;
using Microarea.AdminServer.Libraries.DatabaseManager;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.AdminServer.Libraries.DataManagerEngine
{
	/// <summary>
	/// DefaultManager 
	/// gestore delle operazioni dei dati di default (generazione o caricamento)
	/// </summary>
	//=========================================================================
	public class DefaultManager
	{
		# region Variabili private
		private ContextInfo contextInfo;
		private DatabaseDiagnostic dbDiagnostic;
		private DefaultSelections defaultSel;
		private BrandLoader brandLoader;
		private ImportManager importManager;

		public string SelectedConfiguration = DataManagerConsts.Basic;

		// array di appoggio x la gestione dell'importazione dei dati di default
		// in fase di creazione/aggiornamento del database aziendale
		private ArrayList importFileList = new ArrayList();

		private Thread myThread;

		// array di appoggio x tenere traccia dei file contenenti i dati di default da richiamare
		// per le sole tabelle mancanti
		private ArrayList importFileForMissingTableList = new ArrayList();
		private ArrayList importAppendFileForMissingTableList = new ArrayList();

		private Dictionary<string, ArrayList> filesForAppAndModule = new Dictionary<string, ArrayList>();
		# endregion

		# region Costruttori
		/// <summary>
		/// viene istanziato dal wizard 
		/// </summary>
		//---------------------------------------------------------------------
		public DefaultManager(DefaultSelections defSel, DatabaseDiagnostic diagnostic, BrandLoader brand)
		{
			defaultSel = defSel;
			dbDiagnostic = diagnostic;
			brandLoader = brand;
		}

		/// <summary>
		/// viene istanziato dal DataBaseManager per effettuare il caricamento dei dati
		/// di default in fase di creazione/aggiornamento del database aziendale
		/// </summary>
		//---------------------------------------------------------------------
		public DefaultManager(ContextInfo context, DatabaseDiagnostic diagnostic, BrandLoader brand)
		{
			contextInfo = context;
			dbDiagnostic = diagnostic;
			brandLoader = brand;
		}
		# endregion

		///<summary>
		///Entry-point Gestione Dati di Default
		///</summary>
		//---------------------------------------------------------------------
		public Thread DefaultDataManagement()
		{
			if (defaultSel.Mode == DefaultSelections.ModeType.EXPORT)
				ExportDefaultData();
			else
				ImportDefaultData();

			return myThread;
		}

		# region Esportazione dati di default (con thread separato)
		//---------------------------------------------------------------------
		private void ExportDefaultData()
		{
			myThread = new Thread(new ThreadStart(InternalExportDefaultData));
			myThread.Start();
		}

		/// <summary>
		/// generazione dei dati di default. Vengono memorizzati nella directory di personalizzazione
		/// custom/allcompanies/application/module/datamanager/default dell'applicazione/modulo di 
		/// appartenenza della tabella
		/// </summary>
		//---------------------------------------------------------------------
		private void InternalExportDefaultData()
		{
			dbDiagnostic.SetGenericText(string.Format(DataManagerEngineStrings.MsgExportCompanyData, defaultSel.ContextInfo.CompanyName));

			//istanzio il gestore dell'export
			ExportManager expMng = new ExportManager(defaultSel.ExportSel, dbDiagnostic);
			//DataSet dataSet = null;

			// se l'utente ha scelto di eseguire il testo dello script lo eseguo subito sul db aziendale
			if (defaultSel.ExportSel.ExecuteScriptTextBeforeExport)
			{
				TBCommand myCommand =
					new TBCommand(defaultSel.ExportSel.ScriptTextBeforeExport, defaultSel.ExportSel.ContextInfo.Connection);

				try
				{
					myCommand.ExecuteNonQuery();
					dbDiagnostic.SetMessageNoAppAndModuleName
						(
						true,
						string.Empty,
						DataManagerEngineStrings.RunScriptBeforeExportProcess,
						DataManagerEngineStrings.EndedWithSuccess,
						DataManagerEngineStrings.ExecutionScript
						);
				}
				catch (TBException e)
				{
					dbDiagnostic.SetMessageNoAppAndModuleName
						(
						false,
						string.Empty,
						DataManagerEngineStrings.RunScriptBeforeExportProcess,
						string.Format(DataManagerEngineStrings.EndedWithError, e.Message),
						DataManagerEngineStrings.ExecutionScript
						);

					// SE FALLISCE PROCEDO CMQ NELL'ELABORAZIONE (parlato con A.R. - giusto fare cos�)
				}
			}

			try
			{
				foreach (CatalogTableEntry entry in defaultSel.ExportSel.Catalog.TblDBList)
				{
					if (
						(defaultSel.ExportSel.AllTables || entry.Selected) &&
						!string.IsNullOrEmpty(entry.Application) && !string.IsNullOrEmpty(entry.Module)
						)
					{
						expMng.DataMngPath = Path.Combine
							(defaultSel.ExportSel.ContextInfo.PathFinder.GetCustomDataManagerDefaultPath(entry.Application, entry.Module, defaultSel.SelectedIsoState),
							defaultSel.SelectedConfiguration);

						if (!Directory.Exists(expMng.DataMngPath))
							Directory.CreateDirectory(expMng.DataMngPath);

						//@@TODOMICHI
						//expMng.ExportTable(entry, ref dataSet);

						// se l'utente vuole interrompere... non proseguo nell'elaborazione
						if (dbDiagnostic.AbortWizard)
							break;
					}
				}

				// se l'utente ha scelto di salvare le selezioni in un file di configurazione
				// istanzio la classe apposita ed effettuo l'unparse
				if (defaultSel.ExportSel.SaveInConfigurationFile)
				{
					if (defaultSel.ExportSel.ConfigInfo == null)
						defaultSel.ExportSel.ConfigInfo = new ConfigurationInfo();

					FileInfo fi = new FileInfo(defaultSel.ExportSel.ConfigurationFilePathToSave);
					// UNPARSE
					if (defaultSel.ExportSel.ConfigInfo.Unparse(defaultSel.ExportSel.ConfigurationFilePathToSave, defaultSel))
						dbDiagnostic.SetMessageNoAppAndModuleName(true, fi.Name, DataManagerEngineStrings.SavingConfigurationFile, DataManagerEngineStrings.EndedWithSuccess, defaultSel.ExportSel.ConfigurationFilePathToSave);
					else
					{
						IDiagnosticItems diagnItems = defaultSel.ExportSel.ConfigInfo.ConfigurationInfoDiagnostic.AllMessages();
						if (diagnItems != null)
						{
							foreach (DiagnosticItem item in diagnItems)
								dbDiagnostic.SetMessageNoAppAndModuleName
								(
								false,
								fi.Name,
								DataManagerEngineStrings.SavingConfigurationFile,
								string.Format(DataManagerEngineStrings.EndedWithError, item.FullExplain),
								defaultSel.ExportSel.ConfigurationFilePathToSave
								);
						}
					}
				}
			}
			catch (Exception)
			{
				//DiagnosticViewer.ShowError(string.Empty, e.Message, string.Empty, string.Empty, DataManagerEngineStrings.DefaultText);
			}

			dbDiagnostic.SetFinish((dbDiagnostic.AbortWizard) ? DataManagerEngineStrings.MsgOperationInterrupted : DataManagerEngineStrings.MsgEndOperation);

			// salvo il file di log
			string filePath = this.defaultSel.ExportSel.CreateLogFile(dbDiagnostic);

			// scrivo la riga con il riferimento al file di log salvato
			dbDiagnostic.SetMessageNoAppAndModuleName(true, Path.GetFileName(filePath), DatabaseLayerStrings.CreateLogFile, string.Empty, filePath);
		}
		# endregion

		# region Importazione dati di default
		//---------------------------------------------------------------------------
		public void ImportDefaultData()
		{
			defaultSel.ImportSel.NoOptional = false; 
			importManager = new ImportManager(defaultSel.ImportSel, dbDiagnostic);
			myThread = importManager.Import();
		}
		# endregion

		# region Importazione dati di default silente (dal DatabaseManager)
		/// <summary>
		/// metodo chiamato dal DatabaseManager per effettuare il caricamento dati di default
		/// contestuale alla creazione/aggiornamento del database aziendale
		/// </summary>
		//---------------------------------------------------------------------------
		public bool ImportDefaultDataSilentMode()
		{
			// se ho dei file da importare per le tabelle mancanti vado ad aggiungerli nell'array principale
			if (importFileForMissingTableList.Count > 0)
				ManageImportFileForMissingTable();

			// se l'array dei file da importare � vuoto non procedo nell'elaborazione
			if (importFileList.Count == 0)
				return false;

			if (defaultSel == null)
				defaultSel = new DefaultSelections(contextInfo, brandLoader);

			defaultSel.Mode = DefaultSelections.ModeType.IMPORT;
			defaultSel.ImportSel.ErrorRecovery = ImportSelections.TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK;

			// quando importo i dati contestualmente alla creazione/aggiornamento db aziendale
			// devo andare in UPDATE delle righe esistenti (i file di Append vengono eseguiti alla fine di tutto e devono
			// andare, appunto, in Append) // Michela: import dati OFM/ACM
			defaultSel.ImportSel.UpdateExistRow = ImportSelections.UpdateExistRowType.UPDATE_ROW; 

			defaultSel.ImportSel.DeleteTableContext = false; // fix bug 14558
			defaultSel.ImportSel.DisableCheckFK = true;
			defaultSel.ImportSel.NoOptional = true;
			defaultSel.ImportSel.IsSilent = true; // imposto che si tratta di elaborazione silente

			StringCollection appendFiles = new StringCollection();
			foreach (FileInfo file in importFileList)
			{
				// devo inserire in coda quelli con prefisso Append
				if (Path.GetFileNameWithoutExtension(file.Name).EndsWith(DataManagerConsts.Append, StringComparison.OrdinalIgnoreCase))
					appendFiles.Add(file.FullName);
				else
					defaultSel.ImportSel.AddItemInImportList(file.FullName);
			}

			//inserisco quelli con prefisso Append
			foreach (string fileName in appendFiles)
				defaultSel.ImportSel.AddAppendItemInImportList(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));

			importManager = new ImportManager(defaultSel.ImportSel, dbDiagnostic);
			// richiamo questo metodo xch�, in modo silente, non faccio il multithread
			importManager.InternalImport();

			return true;
		}

		//---------------------------------------------------------------------------
		public bool ImportAppendDefaultData(List<string> missingTablesListForAppend)
		{
			// pulisco l'array dei file da importare
			importFileList.Clear();
			importFileForMissingTableList.Clear();

			if (defaultSel != null)
				defaultSel.ImportSel.ImportList.Clear();

			string fileName = string.Empty, table = string.Empty;
			DirectoryInfo di = null;
			int idx = 0;

			foreach (FileForMissingTable file in importAppendFileForMissingTableList)
			{
				di = new DirectoryInfo(file.StandardPath);
				if (di.Exists)
				{
					foreach (FileInfo fi in di.GetFiles("*Append.xml"))
					{
						idx = fi.Name.IndexOf("Append");
						table = fi.Name.Substring(0, idx);
						if (missingTablesListForAppend.Contains(table))
							importFileList.Add(fi);
					}
				}

				di = new DirectoryInfo(file.CustomPath);
				if (di.Exists)
				{
					foreach (FileInfo fi in di.GetFiles("*Append.xml"))
					{
						idx = fi.Name.IndexOf("Append");
						table = fi.Name.Substring(0, idx);
						if (missingTablesListForAppend.Contains(table))
							importFileList.Add(fi);
					}
				}
			}

			return ImportDefaultDataSilentMode();
		}
		# endregion

		# region Metodi richiamati dal DatabaseManager per gestione dati default delle tabelle da creare
		/// <summary>
		/// Chiamata dal Database Manager per aggiungere gli eventuali dati di default di Append
		/// della singola tabella appartenente al moduleName dell'applicazione appName. Avviene
		/// in fase di creazione della singola tabella (non viene chiamata se la tabella viene creata
		/// in fase di creazione di tutte le tabelle appartenti al modulo/applicazione)	
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddAppendDefaultDataTable(string appName, string moduleName)
		{
			string stdPath = Path.Combine
				(contextInfo.PathFinder.GetStandardDataManagerDefaultPath(appName, moduleName, contextInfo.IsoState),
				SelectedConfiguration);

			string customPath = Path.Combine
				(contextInfo.PathFinder.GetCustomDataManagerDefaultPath(appName, moduleName, contextInfo.IsoState),
				SelectedConfiguration);

			importAppendFileForMissingTableList.Add(new FileForMissingTable(stdPath, customPath, string.Empty));
		}

		/// <summary>
		/// dato l'array contenente i dati di default da caricare per le tabelle mancanti, compongo i vari
		/// path (custom e standard) con la configurazione selezionata dall'utente
		/// </summary>
		//---------------------------------------------------------------------------
		private void ManageImportFileForMissingTable()
		{
			string custPath, stdPath, fileName = string.Empty;

			foreach (FileForMissingTable file in importFileForMissingTableList)
			{
				custPath = Path.Combine(file.CustomPath, SelectedConfiguration) + Path.DirectorySeparatorChar;
				stdPath = Path.Combine(file.StandardPath, SelectedConfiguration) + Path.DirectorySeparatorChar;
				fileName = custPath + file.Table + NameSolverStrings.XmlExtension;

				// controllo se alla tabella sono associati dei dati di default
				// prima nella custom del modulo (sia tablename.xml che appendtablename.xml)
				// poi nella standard del modulo per il solo file tablename.xml
				if (File.Exists(fileName))
					importFileList.Add(new FileInfo(fileName));
				else
				{
					fileName = stdPath + file.Table + NameSolverStrings.XmlExtension;
					if (File.Exists(fileName))
						importFileList.Add(new FileInfo(fileName));
				}

				// controllo se esiste nella custom il file tablenameappend.xml che permette di caricare altri dati di default
				fileName = custPath + file.Table + DataManagerConsts.Append + NameSolverStrings.XmlExtension;
				if (File.Exists(fileName))
					importFileList.Add(new FileInfo(fileName));
			}
		}

		/// <summary>
		/// Chiamata dal Database Manager per aggiungere gli eventuali dati di default 
		/// delle tabelle appartenenti al moduleName dell'applicazione appName. Avviene
		/// in fase di creazione delle tabelle del modulo	
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddDefaultDataTable(string appName, string moduleName)
		{
			DirectoryInfo standardDir = new DirectoryInfo
				(
				Path.Combine
				(contextInfo.PathFinder.GetStandardDataManagerDefaultPath(appName, moduleName, contextInfo.IsoState),
				SelectedConfiguration)
				);

			DirectoryInfo customDir = new DirectoryInfo
				(
				Path.Combine
				(contextInfo.PathFinder.GetCustomDataManagerDefaultPath(appName, moduleName, contextInfo.IsoState),
				SelectedConfiguration)
				);

			contextInfo.PathFinder.GetXMLFilesInPath(standardDir, customDir, ref importFileList);
		}

		/// <summary>
		/// Chiamata dal Database Manager per aggiungere gli eventuali dati di default 
		/// della singola tabella appartenente al moduleName dell'applicazione appName. Avviene
		/// in fase di creazione della singola tabella (non viene chiamata se la tabella viene creata
		/// in fase di creazione di tutte le tabelle appartenti al modulo/applicazione)	
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddDefaultDataTable(string tableName, string appName, string moduleName)
		{
			importFileForMissingTableList.Add
			(
				new FileForMissingTable
				(
				contextInfo.PathFinder.GetStandardDataManagerDefaultPath(appName, moduleName, contextInfo.IsoState),
				contextInfo.PathFinder.GetCustomDataManagerDefaultPath(appName, moduleName, contextInfo.IsoState),
				tableName
				)
			);
		}
		# endregion
	}

	# region class FileForMissingTable (per i default delle tabelle mancanti)
	/// <summary>
	/// classe FileForMissingTable
	/// � di appoggio per tenere traccia dei possibili path di ogni file (contenente i dati da importare) 
	/// per ogni tabella che risulta mancante (la configurazione prescelta non deve essere cablata in Base!) 
	/// </summary>
	//=========================================================================
	public class FileForMissingTable
	{
		public string StandardPath = string.Empty;
		public string CustomPath = string.Empty;
		public string Table = string.Empty;

		//---------------------------------------------------------------------------
		public FileForMissingTable(string standardPath, string customPath, string table)
		{
			this.StandardPath = standardPath;
			this.CustomPath = customPath;
			this.Table = table;
		}
	}
	# endregion
}
