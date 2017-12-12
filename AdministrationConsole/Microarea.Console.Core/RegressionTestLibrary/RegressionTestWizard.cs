
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Core.RegressionTestLibrary
{
	/// <summary>
	/// Summary description for RegressionTestWizard.
	/// </summary>
	//=========================================================================
	public class RegressionTestWizard : WizardManager
	{
		# region Data member

		public	RegressionTestSelections DataSelections = null;
		public	DatabaseDiagnostic	DBDiagnostic = null;
		
		# endregion

		# region Events and Delegates
		// Evento sparato sul click del pulsante Finish del wizard
		public delegate void FinishWizard();
		public event FinishWizard OnFinishWizard;
		# endregion

		# region Costruttore e add delle pagine
		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------------	
		public RegressionTestWizard(RegressionTestSelections selections, DatabaseDiagnostic diagnostic)
		{
			DataSelections = selections;
			DBDiagnostic = diagnostic;
		}

		/// <summary>
		/// Add delle pagine del wizard
		/// </summary>
		//---------------------------------------------------------------------
		public void AddWizardPages()
		{
			this.wizardTitle = "Migrazione dati";

			AddWizardPage(new WizardPages.PresentationPage());
			AddWizardPage(new WizardPages.FoldersSelectionPage());
			AddWizardPage(new WizardPages.AreasSelectionsListPage());
			AddWizardPage(new WizardPages.SummaryPage());
		}
		# endregion
		
		# region Evento sul click del pulsante Fine
		/// <summary>
		/// evento intercettato sul pulsante Finish del wizard per lanciare
		/// il thread separato per l'esecuzione delle operazioni
		/// </summary>
		//---------------------------------------------------------------------
		public void OnFinishPage()
		{
			if (OnFinishWizard != null)
				OnFinishWizard();
		}
		# endregion

	}
}
