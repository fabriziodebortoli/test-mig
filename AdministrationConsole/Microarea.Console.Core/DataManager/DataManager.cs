using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using Microarea.Console.Core.DataManager.Default;
using Microarea.Console.Core.DBLibrary.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using System.Collections;
using System.Globalization;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager
{
	/// <summary>
	/// ImportExportManager (entry-point alle classi che gestiscono i wizard)
	/// Deriva da BaseImportExportManager (in DatabaseLayer.DataManager) che si occupa del caricamento
	/// dei dati in modalita' silente
	/// </summary>
	//=========================================================================
	public class ImportExportManager : BaseImportExportManager
	{
		private	bool valid = false;
		
		private ExecutionForm executionForm	= null;

		public delegate bool GetFreeStatusCompanyDB();
		public event GetFreeStatusCompanyDB OnGetFreeStatusCompanyDB;

		/// <summary>
		/// questa classe viene richiamata su un nodo di tipo company nel tree della 
		/// Console, se selezionato la voce "Import/Export" nel context-menu.
		/// </summary>
		//---------------------------------------------------------------------
		public ImportExportManager(string companyIdNode, ContextInfo context, BrandLoader brand)
			: base(context, brand)
		{
			valid = MakeCompanyConnection(companyIdNode);

			executionForm = new ExecutionForm();
			executionForm.OnIsAborted += new ExecutionForm.IsAborted(AbortWizardExecution);

			// eventi da "sparare" all'ExecutionForm durante l'elaborazione
			DBDiagnostic.OnAddTextInExecutionListView	
						+= new DatabaseDiagnostic.AddTextInExecutionListView(PostTextInExecutionListView);
			DBDiagnostic.OnAddTextInLabel		
						+= new DatabaseDiagnostic.AddTextInLabel(PostTextInLabel);
			DBDiagnostic.OnSetFinishElaboration
						+= new DatabaseDiagnostic.SetFinishElaboration(FinishElaboration);
			DBDiagnostic.OnPerformProgressBarStep += new DatabaseDiagnostic.PerformProgressBarStep(PerformProgressBarStep);
		}

		/// <summary>
		/// creo una valida connessione al db aziendale 
		/// </summary>
		/// <returns>se la connessione è stata aperta con successo</returns>
		//---------------------------------------------------------------------------
		private bool MakeCompanyConnection(string companyIdNode)
		{
			if (!contextInfo.MakeCompanyConnection(companyIdNode))
			{
				DiagnosticViewer.ShowDiagnostic(contextInfo.Diagnostic);
				return false;
			}

			return true;
		}

		# region Caricamento dei vari wizard (Import, Export, Sample, Default)
		/// <summary>
		/// richiama il wizard di IMPORTAZIONE dei dati
		/// </summary>
		//---------------------------------------------------------------------------
		public void RunImportWizard()
		{
			if (!valid)
				return;

			Import.ImportWizard importWizard = new Import.ImportWizard(contextInfo, DBDiagnostic, brandLoader);
			importWizard.AddWizardPages();
			importWizard.OnFinishWizard += new Import.ImportWizard.FinishWizard(StartExecutionForm);

			importWizard.Run();
		}

		/// <summary>
		/// richiama il wizard di ESPORTAZIONE dei dati
		/// </summary>
		//---------------------------------------------------------------------------
		public void RunExportWizard()
		{
			if (!valid)
				return;

			Export.ExportWizard exportWizard = new Export.ExportWizard(contextInfo, DBDiagnostic, brandLoader);
			exportWizard.AddWizardPages();
			exportWizard.OnFinishWizard	+= new Export.ExportWizard.FinishWizard(StartExecutionForm);
			exportWizard.Run();
		}

		/// <summary>
		/// richiama il wizard per la gestione dei dati di DEFAULT
		/// </summary>
		//---------------------------------------------------------------------------
		public void RunDefaultWizard()
		{
			if (!valid)
				return;

			DefaultWizard defaultWizard = new DefaultWizard(contextInfo, DBDiagnostic, brandLoader);
			defaultWizard.AddWizardPages();
			defaultWizard.OnFinishWizard += new Default.DefaultWizard.FinishWizard(StartExecutionForm);
			// aggancio l'evento che controlla se il database non ha utenti collegati (per le operazioni di import)
			defaultWizard.OnCompanyDBIsFree += new Default.DefaultWizard.CompanyDBIsFree(GetStatusCompanyDB);
			defaultWizard.Run();
		}

		/// <summary>
		/// richiama il wizard per la gestione dei dati di ESEMPIO
		/// </summary>
		//---------------------------------------------------------------------------
		public void RunSampleWizard()
		{
			if (!valid)
				return;

			Sample.SampleWizard sampleWizard = new Sample.SampleWizard(contextInfo, DBDiagnostic, brandLoader);
			sampleWizard.AddWizardPages();
			sampleWizard.OnFinishWizard += new Sample.SampleWizard.FinishWizard(StartExecutionForm);
			// aggancio l'evento che controlla se il database non ha utenti collegati (per le operazioni di import)
			sampleWizard.OnCompanyDBIsFree += new Sample.SampleWizard.CompanyDBIsFree(GetStatusCompanyDB);
			sampleWizard.Run();
		}
		# endregion

		# region Funzioni per la gestione e visualizzazione di messaggi nell'ExecutionForm
		/// <summary>
		/// fa lo Show dell'ExecutionForm
		/// </summary>
		//---------------------------------------------------------------------------
		public void StartExecutionForm(string run, Thread t)
		{
			if (executionForm == null)
				return;
			
			executionForm.InitializeSeparateThread(t);
			executionForm.ShowDialog();
		}

		/// <summary>
		/// inserisce una riga nell'ExecutionForm con l'elenco delle operazioni effettuate
		/// </summary>
		//---------------------------------------------------------------------
		private void PostTextInExecutionListView
			(
			bool	success, 
			string	tableName, 
			string	fileName,
			string  detail,
			string	fullPath
			)
		{
			if (executionForm == null)
				return; 

			executionForm.AddTextInListView(success, tableName, fileName, detail, fullPath);
		}

		/// <summary>
		/// inserisce un testo nelle Label dell'ExecutionForm (discrimino con un booleano la label da inizializzare)
		/// </summary>
		//---------------------------------------------------------------------
		private void PostTextInLabel(string text)
		{
			if (executionForm == null)
				return; 

			executionForm.PopolateTextLabel(text);
		}

		/// <summary>
		/// al termine dell'elaborazione setta la progress bar completa e posso passare anche un messaggio da visualizzare
		/// </summary>
		//---------------------------------------------------------------------
		private void FinishElaboration(string message)
		{
			if (executionForm == null)
				return; 

			executionForm.SetFinishInProgress(message);
		}

		// l'ExecutionForm mi comunica che l'utente vuole interrompere l'elaborazione
		//---------------------------------------------------------------------
		private void AbortWizardExecution(bool abort)
		{
			// da qui risparo l'evento al dbdiagnostic che poi può comunicare al
			// manager che lo stato dell'elaborazione è Aborted
			DBDiagnostic.SetWizardAbort(abort);
		}

		//---------------------------------------------------------------------
		private void PerformProgressBarStep()
		{
			executionForm.PerformProgressBarStepParent();
		}
		# endregion

		# region Funzione per sapere se al DB aziendale sono collegati utenti lato Mago
		/// <summary>
		/// sparo un evento all'ApplicationDBAdmin che interroga a sua volta la Console
		/// per sapere se c'è qualche utente collegato al database lato TB
		/// </summary>
		//---------------------------------------------------------------------
		private bool GetStatusCompanyDB()
		{
			if (OnGetFreeStatusCompanyDB != null)
				return OnGetFreeStatusCompanyDB();

			return false;
		}
		#endregion
	}

	// per ordinare alfabeticamente i nodi di tipo tabella nel treeview
	//============================================================================
	public class DataManagerSortTreeNodeList : IComparer
	{
		//---------------------------------------------------------------------------
		int IComparer.Compare(Object node1, Object node2)
		{
			return (new CaseInsensitiveComparer(CultureInfo.InvariantCulture)).Compare
				(
				((PlugInTreeNode)node1).Text,
				((PlugInTreeNode)node2).Text
				);
		}
	}
}