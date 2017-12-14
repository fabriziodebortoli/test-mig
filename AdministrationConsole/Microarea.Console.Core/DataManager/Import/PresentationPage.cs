using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Import
{
	//=========================================================================
	public partial class PresentationPage : ExteriorWizardPage
	{
		private ImportSelections importSel = null;

		//---------------------------------------------------------------------
		public PresentationPage()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		private void InitializeTextBoxes()
		{
			ServerNameLabel.Text = importSel.ContextInfo.CompanyDBServer;
			DatabaseNameLabel.Text = importSel.ContextInfo.CompanyDBName;
			AziendaNameLabel.Text = importSel.ContextInfo.CompanyName;
			DescriptionLabel.Text = DataManagerStrings.ImportPresentationText;
		}

		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			importSel = ((Common.DataManagerWizard)this.WizardManager).GetImportSelections();
			InitializeTextBoxes();

			this.WizardForm.SetWizardButtons(WizardButton.Next);
			return true;
		}

		/// <summary>
		/// OnWizardHelp
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerImport + "PresentationPage");
			return true;
		}
    }
}