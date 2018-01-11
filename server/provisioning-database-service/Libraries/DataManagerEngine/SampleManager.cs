using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using TaskBuilderNetCore.Interfaces;
using Microarea.ProvisioningDatabase.Libraries.DatabaseManager;
using Microarea.Common.NameSolver;
using Microarea.ProvisioningDatabase.Infrastructure;

namespace Microarea.ProvisioningDatabase.Libraries.DataManagerEngine
{
	/// <summary>
	/// DefaultManager 
	/// gestore delle operazioni dei dati di esempio (generazione o caricamento)
	/// </summary>
	//=========================================================================
	public class SampleManager
	{
		private SampleSelections	sampleSel;
		private ImportManager		importManager;
		private DatabaseDiagnostic	dbDiagnostic;
		private ContextInfo			contextInfo;
		private Thread				myThread;
		private BrandLoader			brandLoader;

		public string SelectedConfiguration = DataManagerConsts.Basic;
		// serve per il caricamento silente dei dati di esempio
		// a differenza di quelli di default, i dati di esempio esistono per l'isostato IT oppure INTL
		public string SelectedIsoState = string.Empty; 

		// array di appoggio x la gestione dell'importazione dei dati di esempio
		// in fase di creazione/aggiornamento del database aziendale
		private List<FileInfo> importFileList = new List<FileInfo>();

		// array di appoggio x tenere traccia dei file contenenti i dati di esempio da richiamare
		// per le sole tabelle mancanti
		private List<FileForMissingTable> importFileForMissingTableList = new List<FileForMissingTable>();
		private List<FileForMissingTable> importAppendFileForMissingTableList = new List<FileForMissingTable>();

		/// <summary>
		/// viene istanziato dal wizard 
		/// </summary>
		//---------------------------------------------------------------------
		public SampleManager(SampleSelections sample, DatabaseDiagnostic diagnostic)
		{
			sampleSel = sample;
			dbDiagnostic = diagnostic;
		}

		/// <summary>
		/// viene istanziato dal DataBaseManager per effettuare il caricamento dei dati
		/// di esempio in fase di creazione/aggiornamento del database aziendale
		/// </summary>
		//---------------------------------------------------------------------
		public SampleManager(ContextInfo context, DatabaseDiagnostic diagnostic, BrandLoader brand)
		{
			contextInfo = context;
			dbDiagnostic = diagnostic;
			brandLoader = brand;
		}

		///<summary>
		/// Entry-point gestione dati di Esempio (con thread separato)
		///</summary>
		//---------------------------------------------------------------------
		public Thread SampleDataManagement()
		{
			if (sampleSel.Mode == DefaultSelections.ModeType.EXPORT)
				ExportSampleData();
			else
				ImportSampleData();	
		
			return myThread;
		}

		//---------------------------------------------------------------------
		private void ExportSampleData()
		{
			myThread = new Thread(new ThreadStart(InternalExportSampleData));
			myThread.Start();
		}

		/// <summary>
		/// generazione dei dati di default. Vengono memorizzati nella directory di personalizzazione
		/// custom/allcompanies/application/module/datamanager/default dell'applicazione/modulo di 
		/// appartenenza della tabella
		/// </summary>
		//---------------------------------------------------------------------
		private void InternalExportSampleData()
		{
			dbDiagnostic.SetGenericText(string.Format(DataManagerEngineStrings.MsgExportCompanyData, sampleSel.ContextInfo.CompanyName));

			//istanzio il gestore dell'export
			ExportManager expMng = new ExportManager(sampleSel.ExportSel, dbDiagnostic);
			//DataSet dataSet = null;

			try
			{
				foreach (CatalogTableEntry entry in sampleSel.ExportSel.Catalog.TblDBList)
				{
					if (
						(sampleSel.ExportSel.AllTables || entry.Selected) &&
						!string.IsNullOrEmpty(entry.Application) && !string.IsNullOrEmpty(entry.Module)
						)					
					{
						expMng.DataMngPath = Path.Combine
							(sampleSel.ExportSel.ContextInfo.PathFinder.GetCustomDataManagerSamplePath(entry.Application, entry.Module, sampleSel.SelectedIsoState),
							sampleSel.SelectedConfiguration);

						if (!Directory.Exists(expMng.DataMngPath))
							Directory.CreateDirectory(expMng.DataMngPath);

						//@@TODOMICHI
						//expMng.ExportTable(entry, ref dataSet);

						// se l'utente vuole interrompere... non proseguo nell'elaborazione
						if (dbDiagnostic.AbortWizard)
							break;
					}
				}
			}
			catch (Exception) 
			{
				//DiagnosticViewer.ShowError(string.Empty, e.Message, string.Empty, string.Empty, DataManagerEngineStrings.DefaultText);		
			}

			dbDiagnostic.SetFinish((dbDiagnostic.AbortWizard) ? DataManagerEngineStrings.MsgOperationInterrupted : DataManagerEngineStrings.MsgEndOperation);

			// salvo il file di log
			string filePath = this.sampleSel.ExportSel.CreateLogFile(dbDiagnostic);

			// scrivo la riga con il riferimento al file di log salvato
			dbDiagnostic.SetMessageNoAppAndModuleName(true, Path.GetFileName(filePath), DatabaseManagerStrings.CreateLogFile, string.Empty, filePath);
		}

		///<summary>
		/// Funzione per l'importazione dei dati di esempio
		///</summary>
		//---------------------------------------------------------------------------
		public void ImportSampleData()
		{
			if (sampleSel == null)
				return;		
				
			sampleSel.ImportSel.NoOptional = true;
			importManager = new ImportManager(sampleSel.ImportSel, dbDiagnostic);
			myThread = importManager.Import();
		}

		#region Importazione dati di esempio silente (dal DatabaseManager)
		/// <summary>
		/// metodo chiamato dal DatabaseManager per effettuare il caricamento dati di esempio
		/// contestuale alla creazione/aggiornamento del database aziendale
		/// (al momento non e' mai richiamato, perche' in fase di upgrade database posso scegliere
		/// solo i dati di default, questo metodo e' stato inserito solo per simmetria)
		/// </summary>
		//---------------------------------------------------------------------------
		public bool ImportSampleDataSilentMode()
		{
			if (sampleSel == null)
				sampleSel = new SampleSelections(contextInfo, brandLoader);

			sampleSel.Mode = DefaultSelections.ModeType.IMPORT;
			sampleSel.ImportSel.ErrorRecovery = ImportSelections.TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK;

			// quando importo i dati contestualmente alla creazione/aggiornamento db aziendale
			// devo andare in UPDATE delle righe esistenti (i file di Append vengono eseguiti alla fine di tutto e devono
			// andare, appunto, in Append) // Michela: import dati OFM/ACM
			sampleSel.ImportSel.UpdateExistRow = ImportSelections.UpdateExistRowType.UPDATE_ROW;

			sampleSel.ImportSel.DeleteTableContext = false; // fix bug 14558
			sampleSel.ImportSel.DisableCheckFK = true;
			sampleSel.ImportSel.NoOptional = true;
			sampleSel.ImportSel.IsSilent = true; // imposto che si tratta di elaborazione silente

			// se ho dei file da importare per le tabelle mancanti vado ad aggiungerli nell'array principale
			if (importFileForMissingTableList.Count > 0)
				ManageImportFileForMissingTable();

			// se l'array dei file da importare è vuoto non procedo nell'elaborazione
			if (importFileList.Count == 0)
				return false;

			StringCollection appendFiles = new StringCollection();
			foreach (FileInfo file in importFileList)
			{
				// devo inserire in coda quelli con prefisso Append
				if (Path.GetFileNameWithoutExtension(file.Name).EndsWith(DataManagerConsts.Append, StringComparison.OrdinalIgnoreCase))
					appendFiles.Add(file.FullName);
				else
					sampleSel.ImportSel.AddItemInImportList(file.FullName);
			}

			//inserisco quelli con prefisso Append
			foreach (string fileName in appendFiles)
				sampleSel.ImportSel.AddAppendItemInImportList(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));

			importManager = new ImportManager(sampleSel.ImportSel, dbDiagnostic);
			// richiamo questo metodo xchè, in modo silente, non faccio il multithread
			importManager.InternalImport();

			return true;
		}

		/// <summary>
		/// metodo chiamato in fase di importazione dei dati di esempio 
		/// dal componente di <app-database-import> di SubscriptionDatabase
		/// </summary>
		//---------------------------------------------------------------------------
		public bool ImportSampleDataForSubscription(ImportDataParameters parameters)
		{
			if (sampleSel == null)
				sampleSel = new SampleSelections(contextInfo, brandLoader);

			sampleSel.Mode = DefaultSelections.ModeType.IMPORT;
			sampleSel.ImportSel.ErrorRecovery = ImportSelections.TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK;

			sampleSel.ImportSel.UpdateExistRow = parameters.OverwriteRecord ? ImportSelections.UpdateExistRowType.UPDATE_ROW : ImportSelections.UpdateExistRowType.SKIP_ROW;

			sampleSel.ImportSel.DeleteTableContext = parameters.DeleteTableContext;
			sampleSel.ImportSel.DisableCheckFK = true;
			sampleSel.ImportSel.NoOptional = parameters.NoOptional;
			sampleSel.ImportSel.IsSilent = true; // imposto che si tratta di elaborazione silente

			// in questo caso pre-carico tutti i file a monte (perche' non arrivo da un wizard con le selezioni dei file)
			sampleSel.LoadSampleData();

			importManager = new ImportManager(sampleSel.ImportSel, dbDiagnostic);
			// richiamo questo metodo xchè, in modo silente, non faccio il multithread
			importManager.InternalImport();

			return true;
		}

		//---------------------------------------------------------------------------
		public bool ImportAppendSampleData(List<string> missingTablesListForAppend)
		{
			// pulisco l'array dei file da importare
			importFileList.Clear();
			importFileForMissingTableList.Clear();

			if (sampleSel != null)
				sampleSel.ImportSel.ImportList.Clear();

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

			return ImportSampleDataSilentMode();
		}

		# endregion

		# region Metodi richiamati dal DatabaseManager per gestione dati di esempio delle tabelle da creare
		/// <summary>
		/// Chiamata dal Database Manager per aggiungere gli eventuali dati di esempio di Append
		/// della singola tabella appartenente al moduleName dell'applicazione appName. Avviene
		/// in fase di creazione della singola tabella (non viene chiamata se la tabella viene creata
		/// in fase di creazione di tutte le tabelle appartenti al modulo/applicazione)	
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddAppendSampleDataTable(string appName, string moduleName)
		{
			string stdPath = Path.Combine
				(contextInfo.PathFinder.GetStandardDataManagerSamplePath(appName, moduleName, SelectedIsoState),
				SelectedConfiguration);

			string customPath = Path.Combine
				(contextInfo.PathFinder.GetCustomDataManagerSamplePath(appName, moduleName, SelectedIsoState),
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
		/// Chiamata dal Database Manager per aggiungere gli eventuali dati di esempio delle tabelle appartenenti 
		/// al moduleName dell'applicazione appName. Avviene in fase di creazione delle tabelle del modulo	
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddSampleDataTable(string appName, string moduleName)
		{
			DirectoryInfo standardDir = new DirectoryInfo
				(
				Path.Combine
				(contextInfo.PathFinder.GetStandardDataManagerSamplePath(appName, moduleName, SelectedIsoState),
				SelectedConfiguration)
				);

			DirectoryInfo customDir = new DirectoryInfo
				(
				Path.Combine
				(contextInfo.PathFinder.GetCustomDataManagerSamplePath(appName, moduleName, SelectedIsoState),
				SelectedConfiguration)
				);

			contextInfo.PathFinder.GetXMLFilesInPath(standardDir, customDir, ref importFileList);
		}

		/// <summary>
		/// Chiamata dal Database Manager per aggiungere gli eventuali dati di esempio 
		/// della singola tabella appartenente al moduleName dell'applicazione appName. Avviene
		/// in fase di creazione della singola tabella (non viene chiamata se la tabella viene creata
		/// in fase di creazione di tutte le tabelle appartenti al modulo/applicazione)	
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddSampleDataTable(string tableName, string appName, string moduleName)
		{
			importFileForMissingTableList.Add
			(
				new FileForMissingTable
				(
				contextInfo.PathFinder.GetStandardDataManagerDefaultPath(appName, moduleName, SelectedIsoState),
				contextInfo.PathFinder.GetCustomDataManagerDefaultPath(appName, moduleName, SelectedIsoState),
				tableName
				)
			);
		}
		# endregion
	}
}