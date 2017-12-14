using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Sample
{
	//=========================================================================
	public partial class BaseColumnsPage : InteriorWizardPage
	{
		# region Variabili private
		private SampleSelections	sampleSel	= null;
		private bool	isImportProcess = true;
		private Common.Images myImages = null;		
		# endregion

		# region Costruttore
		//---------------------------------------------------------------------
		public BaseColumnsPage()
		{
			InitializeComponent();

			myImages = new Common.Images();
		}
		# endregion

		# region SetImageInHeaderPicture
		//---------------------------------------------------------------------
		private void SetImageInHeaderPicture()
		{
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Common.Images.GetSampleBmpSmallIndex()];
			m_subtitleLabel.Text = isImportProcess ? DataManagerStrings.BaseColumnsPageTitleForImport : DataManagerStrings.BaseColumnsPageTitleForExport;
		}
		# endregion

		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			sampleSel = ((Common.DataManagerWizard)this.WizardManager).GetSampleSelections();

			isImportProcess = (sampleSel.Mode == SampleSelections.ModeType.IMPORT);
			SetImageInHeaderPicture();

			// formato utc data è disponibile solo se sto importando
			DateTimeGroupBox.Visible	= isImportProcess;
			UseUtcFormatCheckBox.Visible= isImportProcess;

			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			return true;
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
			TBCreatedCheckBox.Checked = (isImportProcess) ? sampleSel.ImportSel.ImportTBCreated : sampleSel.ExportSel.ExportTBCreated;
			TBModifiedCheckBox.Checked = (isImportProcess) ? sampleSel.ImportSel.ImportTBModified : sampleSel.ExportSel.ExportTBModified;

			ParamsGroupBox.Text = string.Format(ParamsGroupBox.Text,
				(isImportProcess) ? DataManagerStrings.ImportOperation : DataManagerStrings.ExportOperation);

			if (isImportProcess)
				UseUtcFormatCheckBox.Checked = sampleSel.ImportSel.UseUtcDateTimeFormat;
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo all'export selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (isImportProcess)
			{
				sampleSel.ImportSel.ImportTBCreated			= TBCreatedCheckBox.Checked;
				sampleSel.ImportSel.ImportTBModified		= TBModifiedCheckBox.Checked;
				sampleSel.ImportSel.UseUtcDateTimeFormat	= UseUtcFormatCheckBox.Checked;
			}
			else
			{
				sampleSel.ExportSel.ExportTBCreated		= TBCreatedCheckBox.Checked;
				sampleSel.ExportSel.ExportTBModified	= TBModifiedCheckBox.Checked;
			}
		}
		# endregion

		#region OnWizardBack e OnWizardNext
		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();

			if (!isImportProcess)
			{
				if (sampleSel.ExportSel.WriteQuery)
					return "AddWhereClausePage";

				if (sampleSel.ExportSel.SelectColumns)
					return "ColumnsSelectionsListPage";

				if (!sampleSel.ExportSel.AllTables)
					return "TablesSelectionsListPage";

				return "TablesParamPage";
			}

			return "ChooseOperationPage";
		}

		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			GetControlsValue();

			if (isImportProcess)
				return "ImportParamsPage";

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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerSample + "BaseColumnsPage");
			return true;
		}
		#endregion
	}
}
