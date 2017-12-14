
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.MenuManager.QuickStartWizard
{
	///<summary>
	/// UserControl contenitore del pagine del wizard QuickStart
	///</summary>
	//================================================================================
	public class QuickStartWizard : WizardManager
	{
		private QuickStartSelections qsSelections = null;
		private Diagnostic qsDiagnostic = null;
		private LoginManager loginManager = null;

		//--------------------------------------------------------------------------------
		public QuickStartSelections QSSelections { get { return qsSelections; } }
		public Diagnostic QSDiagnostic { get { return qsDiagnostic; } }
		public LoginManager LoginManager { get { return loginManager; } }

		// NOTA: a queste properties si puo' accedere nelle singole pagine con questa sintassi:
		// ((QuickStartWizard)this.WizardManager).QSSelections
		// ((QuickStartWizard)this.WizardManager).QSDiagnostic 
		// ((QuickStartWizard)this.WizardManager).LoginManager

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public QuickStartWizard(LoginManager lm)
		{
			this.wizardFormIcon = MenuManagerStrings.MenuMngForm;
			

			loginManager = lm;

            this.wizardTitle = string.Format("{0} Quick Start Wizard", lm.GetBrandedProductTitle());

			// istanzio la classe con le selezioni
            qsSelections = new QuickStartSelections(lm.GetBrandedKey("DBPrefix"));

			qsDiagnostic = new Diagnostic("QuickStartWizard");
		}

		///<summary>
		/// Add delle pagine del wizard
		///</summary>
		//---------------------------------------------------------------------
		public virtual void AddWizardPages()
		{
			AddWizardPage(new Pages.PresentationPage());
			AddWizardPage(new Pages.SerialNumberPage());
			AddWizardPage(new Pages.ChooseActionPage());
			AddWizardPage(new Pages.CompanyInfoPage());
			AddWizardPage(new Pages.SummaryPage());
		}
	}
}
