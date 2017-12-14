
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Core.RegressionTestLibrary.WizardPages
{
	/// <summary>
	/// Summary description for PresentationPage.
	/// </summary>
	//=========================================================================
	public partial class PresentationPage : ExteriorWizardPage
	{
		# region Variabili private
		private RegressionTestSelections dataSel = null;
		# endregion

		# region Costruttore
		//---------------------------------------------------------------------
		public PresentationPage()
		{
			InitializeComponent();
		}
		# endregion
		
		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			dataSel = ((RegressionTestWizard)this.WizardManager).DataSelections;

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
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage
				(this, 
				RegressionTestLibraryConsts.NamespacePlugIn, 
				RegressionTestLibraryConsts.SearchParameter + "PresentationPage"
				);

			return true;
		}
		#endregion

	}
}
