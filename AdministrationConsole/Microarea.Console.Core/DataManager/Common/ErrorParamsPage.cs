using System.Windows.Forms;

using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Common
{
	//=========================================================================
	public partial class ErrorParamsPage : InteriorWizardPage
	{
		protected ImportSelections importSel = null;
		private Images myImages = null;		
	
		//---------------------------------------------------------------------
		public ErrorParamsPage()
		{
			InitializeComponent();
			myImages = new Images();
		}

		# region SetImageInHeaderPicture
		//---------------------------------------------------------------------
		private void SetImageInHeaderPicture()	
		{
			bool fromDefaultOrSample = false;

			// di default metto l'image dell'import
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetImportBmpSmallIndex()];

			fromDefaultOrSample = (((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections() != null) ? true : false;
			if (fromDefaultOrSample)
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetDefaultBmpSmallIndex()];

			fromDefaultOrSample = fromDefaultOrSample || ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false);
			if (fromDefaultOrSample && ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false))
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetSampleBmpSmallIndex()];
		}
		# endregion

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			SetImageInHeaderPicture();

			importSel = ((Common.DataManagerWizard)this.WizardManager).GetImportSelections();

			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			return true;
		}

		//---------------------------------------------------------------------
        public override bool OnKillActive()
		{
			GetControlsValue();
			return base.OnKillActive();
		}
		# endregion

		# region OnWizardNext
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			if (
				(((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections() != null &&
				((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections().Mode == DefaultSelections.ModeType.IMPORT) ||
				(((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null &&
				((Common.DataManagerWizard)this.WizardManager).GetSampleSelections().Mode == SampleSelections.ModeType.IMPORT)
				)
				return "SummaryPage";

			return base.OnWizardNext();
		}
		# endregion

		# region Get e Set delle selezioni effettuate dall'utente
		/// <summary>
		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			switch (importSel.ErrorRecovery)
			{
				case ImportSelections.TypeRecovery.CONTINUE:
					ContinueRadioButton.Checked = true;
					break;

				case ImportSelections.TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK:
					ContinueLastFileRollbackRadioButton.Checked = true;
					break;

				case ImportSelections.TypeRecovery.STOP_LAST_FILE_ROLLBACK:
					StopLastFileRollbackRadioButton.Checked = true;
					StopLastFileRollbackRadioButton.Enabled = true;
					break;

				case ImportSelections.TypeRecovery.STOP_ALL_FILE_ROLLBACK:
					StopAllFileRollbackRadioButton.Checked = true;
					StopAllFileRollbackRadioButton.Enabled = true;
					break;
			}
		}

		/// <summary>
		/// per memorizzare i valori dei controls delle selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (ContinueRadioButton.Checked)
				importSel.ErrorRecovery = ImportSelections.TypeRecovery.CONTINUE;

			if (ContinueLastFileRollbackRadioButton.Checked)
				importSel.ErrorRecovery = ImportSelections.TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK;

			if (StopLastFileRollbackRadioButton.Checked)
				importSel.ErrorRecovery = ImportSelections.TypeRecovery.STOP_LAST_FILE_ROLLBACK;

			if (StopAllFileRollbackRadioButton.Checked)
				importSel.ErrorRecovery = ImportSelections.TypeRecovery.STOP_ALL_FILE_ROLLBACK;
		}
		# endregion

		# region Eventi sul click dei radio button della pagina
		//---------------------------------------------------------------------
		private void ContinueRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				StopLastFileRollbackRadioButton.Enabled = false;
				StopAllFileRollbackRadioButton.Enabled = false;
			}
		}

		//---------------------------------------------------------------------
		private void ContinueLastFileRollbackRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				StopLastFileRollbackRadioButton.Enabled = false;
				StopAllFileRollbackRadioButton.Enabled = false;
			}
		}

		//---------------------------------------------------------------------
		private void StopRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			StopLastFileRollbackRadioButton.Enabled = true;
			StopAllFileRollbackRadioButton.Enabled = true;
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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerCommon + "ErrorParamsPage");
			return true;
		}
		#endregion
	}
}
