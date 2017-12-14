using System.Windows.Forms;

using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Common
{
	//=========================================================================
	public partial class ImportParamsPage : InteriorWizardPage
	{
		protected ImportSelections importSel = null;
		private bool fromDefaultOrSample = false;
		private Images myImages = null;		

		//---------------------------------------------------------------------
		public ImportParamsPage()
		{
			InitializeComponent();

			myImages = new Images();
		}

		# region SetImageInHeaderPicture
		//---------------------------------------------------------------------
		private void SetImageInHeaderPicture()
		{
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

			fromDefaultOrSample = ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false);

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

		# region OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			if (
				(((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null &&
				((Common.DataManagerWizard)this.WizardManager).GetSampleSelections().Mode == SampleSelections.ModeType.IMPORT) ||
				(((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections() != null && 
				((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections().Mode == DefaultSelections.ModeType.IMPORT)
				)
				return "BaseColumnsPage";

			return base.OnWizardBack();
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerCommon + "ImportParamsPage");
			return true;
		}
		#endregion

		# region Get e Set delle selezioni effettuate dall'utente
		/// <summary>
		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			switch (importSel.UpdateExistRow)
			{
				case ImportSelections.UpdateExistRowType.SKIP_ROW:
				{
					SkipRowRadioButton.Checked	 = true;
					UpdateRowRadioButton.Checked = false;
					ErrorRowRadioButton.Checked	 = false;
					break;
				}
				case ImportSelections.UpdateExistRowType.UPDATE_ROW:
				{
					SkipRowRadioButton.Checked	 = false;
					UpdateRowRadioButton.Checked = true;
					ErrorRowRadioButton.Checked	 = false;
					break;
				}
				case ImportSelections.UpdateExistRowType.SKIP_ROW_ERROR:
				{
					SkipRowRadioButton.Checked	 = false;
					UpdateRowRadioButton.Checked = false;
					ErrorRowRadioButton.Checked	 = true;
					break;
				}
			}

			InsertExtraFieldsCheckBox.Checked	= importSel.InsertExtraFieldsRow;
			DeleteTableContextCheckBox.Checked	= importSel.DeleteTableContext;
			DisableCheckFKCheckBox.Checked		= importSel.DisableCheckFK;
		}

		/// <summary>
		/// per memorizzare i valori dei controls delle selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			importSel.InsertExtraFieldsRow	= InsertExtraFieldsCheckBox.Checked;
			importSel.DeleteTableContext	= DeleteTableContextCheckBox.Checked;
			importSel.DisableCheckFK		= DisableCheckFKCheckBox.Checked;

			if (SkipRowRadioButton.Checked)
				importSel.UpdateExistRow = ImportSelections.UpdateExistRowType.SKIP_ROW;

			if (UpdateRowRadioButton.Checked)
				importSel.UpdateExistRow = ImportSelections.UpdateExistRowType.UPDATE_ROW;

			if (ErrorRowRadioButton.Checked)
				importSel.UpdateExistRow = ImportSelections.UpdateExistRowType.SKIP_ROW_ERROR;
		}
		# endregion

		//---------------------------------------------------------------------
		private void DeleteTableContextCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			bool notCheck = !(((CheckBox)sender).Checked);
			SkipRowRadioButton.Enabled	= notCheck;
			UpdateRowRadioButton.Enabled= notCheck;
			ErrorRowRadioButton.Enabled = notCheck;			
		}
	}
}
