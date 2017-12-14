
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
namespace Microarea.Console.Core.DataManager.Export
{
	//=========================================================================
	public partial class PresentationPage : ExteriorWizardPage
	{
		# region Variabili private		
		private ExportSelections expSelections = null;
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
		private void InitializeTextBoxes()
		{
			ServerNameLabel.Text	= expSelections.ContextInfo.CompanyDBServer;
			DatabaseNameLabel.Text	= expSelections.ContextInfo.CompanyDBName;
			AziendaNameLabel.Text	= expSelections.ContextInfo.CompanyName;
			DescriptionLabel.Text = DataManagerStrings.ExportPresentationText;
		}
		# endregion
		
		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			expSelections = ((Common.DataManagerWizard)this.WizardManager).GetExportSelections();
			InitializeTextBoxes();

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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerExport + "PresentationPage");
			return true;
		}
		#endregion

	}
}
