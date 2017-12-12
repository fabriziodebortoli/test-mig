using System;
using Microarea.TaskBuilderNet.Licence.Licence;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	//================================================================================
	public partial class PresentationPage : ExteriorWizardPage
	{
		private QuickStartSelections qsSelections = null;

		//--------------------------------------------------------------------------------
		public PresentationPage()
		{
			InitializeComponent();

		}

		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			this.m_watermarkPicture.Image = QuickStartStrings.QuickStartLarge;

			qsSelections = ((QuickStartWizard)this.WizardManager).QSSelections;

			this.WizardForm.SetWizardButtons(WizardButton.Next);
            LblPresentation.Text = String.Format(LblPresentation.Text, ((QuickStartWizard)this.WizardManager).LoginManager.GetBrandedProductTitle());
			return true;
		}

		//---------------------------------------------------------------------
		public override string OnWizardNext()
		{
            ProductActivator activator = new ProductActivator(((QuickStartWizard)this.WizardManager).LoginManager);
            bool activationOK = activator.ProductActivated();

            // se il prodotto non e' attivato propongo la finestra di attivazione
            if (activationOK)
                return "ChooseActionPage";
			return base.OnWizardNext();
		}
	}
}
