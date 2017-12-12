
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	/// <summary>
	/// DatabaseDiagnostic: classe per la diagnostica a livello delle varie gestioni di database
	/// </summary>
	//=========================================================================
	public class DatabaseDiagnostic
	{
		private Diagnostic diagnostic = new Diagnostic("DatabaseDiagnostic");
		public bool AbortWizard = false; // per segnalare l'Abort ai vari Manager

		// eventi e delegate agganciati dal DatabaseManager (gestione silente)
		public delegate void ElaborationMessage(bool success, string appName, string modName, string fileName, string detail, string fullPath);
		public event ElaborationMessage	OnElaborationMessage;

		public delegate void UpdateImportFileCounter(string fileName, string appName, string moduleName);
		public event UpdateImportFileCounter OnUpdateImportFileCounter;

		// eventi e delegate da "rimbalzare" nell'execution form
		public delegate void AddTextInExecutionListView(bool success, string tableName, string fileName, string detail, string fullPath);
		public event AddTextInExecutionListView	OnAddTextInExecutionListView;
	
		public delegate void AddTextInLabel(string operation);
		public event AddTextInLabel OnAddTextInLabel;

		public delegate void SetFinishElaboration(string message);
		public event SetFinishElaboration OnSetFinishElaboration;

		// per far avanzare la barra della progressbar
		public delegate void PerformProgressBarStep();
		public event PerformProgressBarStep OnPerformProgressBarStep;
		
		// per aggiungere una riga di testo alla processing form, derivata dall'execution form
		public delegate void AddTextInProcessingForm(bool success, string operation, string detail, object obj, bool updateItem);
		public event AddTextInProcessingForm OnAddTextInProcessingForm;

		// gestione dei tempi e relativa inizializzazione
		public delegate void InsertSingleTimeDbInLabel(int singleDbMin);
		public event InsertSingleTimeDbInLabel OnInsertSingleTimeDbInLabel;

		public delegate void InsertTotalTimeDbInLabel(int totalDbMin);
		public event InsertTotalTimeDbInLabel OnInsertTotalTimeDbInLabel;

		//---------------------------------------------------------------------
		public Diagnostic Diagnostic { get { return diagnostic; } }

		//---------------------------------------------------------------------
		public DatabaseDiagnostic()
		{
		}

		// eredita dal tipo Diagnostic passato
		//-----------------------------------------------------------------------------
		public void SetError(Diagnostic diagnostic)
		{ 
			diagnostic.Set(DiagnosticType.Error, diagnostic); 
		}

		//-----------------------------------------------------------------------------
		public void SetError(string explain)
		{
			diagnostic.Set(DiagnosticType.Error, explain);
		}

		// eredita dal tipo Diagnostic passato
		//-----------------------------------------------------------------------------
		public void SetWarning(Diagnostic diagnostic)
		{ 
			diagnostic.Set(DiagnosticType.Warning, diagnostic); 
		}

		//-----------------------------------------------------------------------------
		public void SetWarning(string explain)
		{
			diagnostic.Set(DiagnosticType.Warning, explain);
		}

		// eredita dal tipo Diagnostic passato
		//-----------------------------------------------------------------------------
		public void SetLogInfo(Diagnostic diagnostic)
		{ 
			diagnostic.Set(DiagnosticType.LogInfo, diagnostic); 
		}

		//-----------------------------------------------------------------------------
		public void SetLogInfo(string explain)
		{
			diagnostic.Set(DiagnosticType.LogInfo, explain);
		}

		// eredita dal tipo Diagnostic passato
		//-----------------------------------------------------------------------------
		public void Set(DiagnosticType type, string message)
		{ 
			diagnostic.Set(type, message); 
		}

		// eredita dal tipo Diagnostic passato
		//-----------------------------------------------------------------------------
		public void Set(Diagnostic diagnostic)
		{ 
			diagnostic.Set(diagnostic); 
		}

		// eredita dal tipo Diagnostic passato
		//-----------------------------------------------------------------------------
		public void Clear()
		{ 
			diagnostic.Clear();
		}

		/// <summary>
		/// generico metodo SetMessage (richiamabile ovunque utilizzando un'istanza dell'oggetto
		/// DatabaseDiagnostic. In base al booleano si stabilisce se si tratta di Errore o Warning)
		/// </summary>
		//-----------------------------------------------------------------------------
		public void SetMessage
			(
			bool	success, 
			string	appName, 
			string	modName, 
			string	fileName, 
			string	tableName, 
			string	detail,
			string	fullPath
			)
		{
			// "fire" evento alla form di elaborazione del DatabaseManager
			if (OnElaborationMessage != null)
				OnElaborationMessage(success, appName, modName, fileName, detail, fullPath);

			// "fire" evento alla form di esecuzione del DataManager
			if (OnAddTextInExecutionListView != null)
				OnAddTextInExecutionListView(success, tableName, fileName, detail, fullPath);

			ExtendedInfo ei = new ExtendedInfo();
			ei.Add(DatabaseLayerStrings.Detail, string.IsNullOrWhiteSpace(detail) ? DatabaseLayerStrings.ElaborationSuccessfullyCompleted : detail);
			ei.Add(DatabaseLayerStrings.FileName2, fullPath);
			diagnostic.Set
				(
				success ? DiagnosticType.Information : DiagnosticType.Error,
				string.Format(DatabaseLayerStrings.ProcessedTable, tableName),
				ei
				);

/*			if (success)
				SetWarning(success, appName, modName, tableName, fileName, detail, fullPath);
			else
				SetError(success, appName, modName, tableName, fileName, detail, fullPath);*/
		}

		//-----------------------------------------------------------------------------
		public void SetMessageNoAppAndModuleName
			(
			bool	success, 
			string	fileName, 
			string	tableName, 
			string	detail,
			string	fullPath
			)
		{
			// "fire" evento alla form di elaborazione del DatabaseManager
			if (OnElaborationMessage != null)
				OnElaborationMessage(success, string.Empty, string.Empty, fileName, detail, fullPath);

			// "fire" evento alla form di esecuzione del DataManager
			if (OnAddTextInExecutionListView != null)
				OnAddTextInExecutionListView(success, tableName, fileName, detail, fullPath);

			ExtendedInfo ei = new ExtendedInfo();
			ei.Add(DatabaseLayerStrings.Detail, string.IsNullOrWhiteSpace(detail) ? DatabaseLayerStrings.ElaborationSuccessfullyCompleted : detail);
			ei.Add(DatabaseLayerStrings.FileName2, fullPath);
			diagnostic.Set
				(
				success ? DiagnosticType.Information : DiagnosticType.Error, 
				string.Format(DatabaseLayerStrings.ProcessedTable, tableName), 
				ei
				);

			/*if (success)
				SetWarning(success, string.Empty, string.Empty, tableName, fileName, detail, fullPath);
			else
				SetError(success, string.Empty, string.Empty, tableName, fileName, detail, fullPath);*/
		}

		//-----------------------------------------------------------------------------
		private void SetError
			(
			bool	success, 
			string	appName, 
			string	modName, 
			string	tableName, 
			string	fileName,
			string	detail,
			string	fullPath
			)
		{
			// "fire" evento alla form di elaborazione del DatabaseManager
			if (OnElaborationMessage != null)
				OnElaborationMessage(success, appName, modName, fileName, detail, fullPath);
		
			// "fire" evento alla form di esecuzione del DataManager
			if (OnAddTextInExecutionListView != null)
				OnAddTextInExecutionListView(success, tableName, fileName, detail, fullPath);

			diagnostic.Set(DiagnosticType.Error, fileName);
		}

		//-----------------------------------------------------------------------------
		private void SetWarning
			(
			bool	success, 
			string	appName, 
			string	modName, 
			string	tableName, 
			string	fileName,
			string	detail,
			string	fullPath
			)
		{
			// "fire" evento alla form di elaborazione del DatabaseManager
			if (OnElaborationMessage != null)
				OnElaborationMessage(success, appName, modName, fileName, detail, fullPath);
		
			// "fire" evento alla form di esecuzione del DataManager
			if (OnAddTextInExecutionListView != null)
				OnAddTextInExecutionListView(success, tableName, fileName, detail, fullPath);

			diagnostic.Set(DiagnosticType.Warning, fileName);
		}

		// metodo usato per aggiornare le label nella ExecutionForm
		//-----------------------------------------------------------------------------
		public void SetGenericText(string text)
		{
			if (OnAddTextInLabel != null)
				OnAddTextInLabel(text);
		}
		
		// setta la fine dell'elaborazione
		//-----------------------------------------------------------------------------
		public void SetFinish(string text)
		{
			if (OnSetFinishElaboration != null)
				OnSetFinishElaboration(text);
		}

		// solo per l'import dei dati di default silente
		// aggiorna il testo con l'indicazione del file e del modulo in elaborazione
		//-----------------------------------------------------------------------------
		public void SetImportFileCounter(string fileName, string appName, string moduleName)
		{
			if (OnUpdateImportFileCounter != null)
				OnUpdateImportFileCounter(fileName, appName, moduleName);
		}

		// l'utente ha cliccato sul pulsante Interrompi nell'ExecutionForm.
		// bisogna impostare lo stato di Abort e passare la gestione ai singoli Manager
		//-----------------------------------------------------------------------------
		public void SetWizardAbort(bool abort)
		{
			AbortWizard = abort;
		}

		/// <summary>
		/// funzione richiamata in fase di Processing Form nel DataMigrationKit
		/// </summary>
		//-----------------------------------------------------------------------------
		public void ModifyTextInProcessingForm(bool success, string operation, string detail, object obj)
		{
			if (OnAddTextInProcessingForm != null)
				OnAddTextInProcessingForm(success, operation, detail, obj, true);

			if (obj == null)
				diagnostic.Set((success) ? DiagnosticType.Information : DiagnosticType.Error, operation, detail);
			else 
				diagnostic.Set((Diagnostic)obj);
		}

		//-----------------------------------------------------------------------------
		public void ProgressBarStep()
		{
			if (OnPerformProgressBarStep != null)
				OnPerformProgressBarStep();
		}

		//-----------------------------------------------------------------------------
		public void SetEstimatedSingleTimeDb(int singleDbMin)
		{
			if (OnInsertSingleTimeDbInLabel != null)
				OnInsertSingleTimeDbInLabel(singleDbMin);
		}

		//-----------------------------------------------------------------------------
		public void SetEstimatedTotalTimeDb(int totalDbMin)
		{
			if (OnInsertTotalTimeDbInLabel != null)
				OnInsertTotalTimeDbInLabel(totalDbMin);
		}
	}
}
