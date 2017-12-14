
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Sample
{
	//=========================================================================
	public partial class PresentationPage : ExteriorWizardPage
	{
		# region Variabili private		
		private SampleSelections sampleSel = null;
		# endregion

		# region Costruttore
		//---------------------------------------------------------------------
		public PresentationPage()
		{
			InitializeComponent();
		}
		# endregion

		# region Inizializzazione label varie
		//---------------------------------------------------------------------
		private void InitializeLabels()
		{
			ServerNameLabel.Text	= sampleSel.ContextInfo.CompanyDBServer;
			DatabaseNameLabel.Text	= sampleSel.ContextInfo.CompanyDBName;
			AziendaNameLabel.Text	= sampleSel.ContextInfo.CompanyName;
			DescriptionLabel.Text = DataManagerStrings.SamplePresentationText;
		}
		# endregion

		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			sampleSel = ((Common.DataManagerWizard)this.WizardManager).GetSampleSelections();
			InitializeLabels();

			this.WizardForm.SetWizardButtons(WizardButton.Next);
			return true;
		}
		# endregion

		# region OnWizardNext
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			return base.OnWizardNext();
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerSample + "PresentationPage");
			return true;
		}
		#endregion
	}
}
