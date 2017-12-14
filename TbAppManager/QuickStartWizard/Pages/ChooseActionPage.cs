
using Microarea.TaskBuilderNet.Licence.Licence;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	//================================================================================
	public partial class ChooseActionPage : InteriorWizardPage
	{
		private QuickStartSelections qsSelections = null;

		//--------------------------------------------------------------------------------
		public ChooseActionPage()
		{
			InitializeComponent();
		}

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			this.m_headerPicture.Image = QuickStartStrings.QuickStartSmall;

			qsSelections = ((QuickStartWizard)this.WizardManager).QSSelections;

			// inizializzo il control
			RBBaseConfiguration.Checked = !qsSelections.SkipBaseConfiguration;

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			return true;
		}

		//---------------------------------------------------------------------
		public override bool OnKillActive()
		{
			return base.OnKillActive();
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
		public override string OnWizardNext()
		{
			qsSelections.SkipBaseConfiguration = RBSkipConfiguration.Checked;
			return (qsSelections.SkipBaseConfiguration) ? "SummaryPage" : base.OnWizardBack();
		}

		//---------------------------------------------------------------------
		public override string OnWizardBack()
		{
			qsSelections.SkipBaseConfiguration = RBSkipConfiguration.Checked;
            ProductActivator activator = new ProductActivator(((QuickStartWizard)this.WizardManager).LoginManager);
            bool activationOK = activator.ProductActivated();

            // se il prodotto non e' attivato propongo la finestra di attivazione
            if (activationOK)
                return "PresentationPage";

			return base.OnWizardNext(); 
		}
		# endregion
	}
}
