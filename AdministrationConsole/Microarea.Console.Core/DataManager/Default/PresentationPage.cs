using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Default
{
	//=========================================================================
	public partial class PresentationPage : ExteriorWizardPage
	{
		private DefaultSelections defaultSel = null;

		//---------------------------------------------------------------------
		public PresentationPage()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		private void InitializeLabels()
		{
			ServerNameLabel.Text	= defaultSel.ContextInfo.CompanyDBServer;
			DatabaseNameLabel.Text	= defaultSel.ContextInfo.CompanyDBName;
			AziendaNameLabel.Text	= defaultSel.ContextInfo.CompanyName;
			DescriptionLabel.Text = DataManagerStrings.DefaultPresentationText;
		}

		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			defaultSel = ((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections();
			InitializeLabels();

			this.WizardForm.SetWizardButtons(WizardButton.Next);
			return true;
		}

		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			return base.OnWizardNext();
		}

		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerDefault + "PresentationPage");
			return true;
		}
	}
}
