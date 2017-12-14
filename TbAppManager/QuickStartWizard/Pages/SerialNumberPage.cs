using System;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Licence.Licence;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	///<summary>
	/// Pagina di richiesta del solo serial number del server (in caso di prima attivazione)
	///</summary>
	//================================================================================
	public partial class SerialNumberPage : InteriorWizardPage
    {
        ProductActivator activator = null;

		//--------------------------------------------------------------------------------
        public SerialNumberPage()
        {
            InitializeComponent();
        }

        //---------------------------------------------------------------------
        void activator_EnableNext(object sender, EventArgs e) 
        {
            this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
        }

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			this.m_headerPicture.Image = QuickStartStrings.QuickStartSmall;

            this.WizardForm.SetWizardButtons(WizardButton.Back);

            activator = new ProductActivator(((QuickStartWizard)this.WizardManager).LoginManager);
            activator.EnableNext += new TaskBuilderNet.Licence.Licence.Forms.ModificationManager(activator_EnableNext);
            //prima di chiudere la splash mi calcolo i due bool
            bool activationOK = activator.ProductActivated();

            // se il prodotto non e' attivato propongo la finestra di attivazione
            if (!activationOK && !activator.ShowAboutAndActivateForWizard(PnlContent))
            {
                //CloseForm();
                return true; // se l'attivazione non va a buon fine o l'utente ha cliccato su Cancel non procedo.
            }
            else 
				base.OnWizardNext();
			
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
            WizardForm.SetCurrentCursor(Cursors.WaitCursor);
            
			bool ok = activator.WizardNext();

            WizardForm.SetDefaultCursor();
			
			return (ok) ? base.OnWizardNext() : WizardForm.NoPageChange;
		}

		//---------------------------------------------------------------------
		public override string OnWizardBack()
		{
            activator.WizardBack();
			return base.OnWizardBack();
		}
		# endregion
	}
}
