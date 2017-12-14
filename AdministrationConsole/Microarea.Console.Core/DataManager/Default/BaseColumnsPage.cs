using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Default
{
	//=========================================================================
	public partial class BaseColumnsPage : InteriorWizardPage
	{
		private DefaultSelections defaultSel = null;
		private bool isImportProcess = true;
		private Common.Images myImages = null;		

		//---------------------------------------------------------------------
		public BaseColumnsPage()
		{
			InitializeComponent();
			myImages = new Common.Images();
		}

		//---------------------------------------------------------------------
		private void SetImageInHeaderPicture()
		{
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Common.Images.GetDefaultBmpSmallIndex()];
			m_subtitleLabel.Text = isImportProcess ? DataManagerStrings.BaseColumnsPageTitleForImport : DataManagerStrings.BaseColumnsPageTitleForExport;
		}

		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			defaultSel = ((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections();

			isImportProcess = (defaultSel.Mode == DefaultSelections.ModeType.IMPORT);
			SetImageInHeaderPicture();

			// formato utc data è disponibile solo se sto importando
			DateTimeGroupBox.Visible	= isImportProcess;
			UseUtcFormatCheckBox.Visible= isImportProcess;

			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			return true;
		}

		# region Get e Set delle selezioni effettuate dall'utente
		/// <summary>
		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			TBCreatedCheckBox.Checked = (isImportProcess) ? defaultSel.ImportSel.ImportTBCreated : defaultSel.ExportSel.ExportTBCreated;
			TBModifiedCheckBox.Checked = (isImportProcess) ? defaultSel.ImportSel.ImportTBModified : defaultSel.ExportSel.ExportTBModified;

			ParamsGroupBox.Text = string.Format(ParamsGroupBox.Text,
				(isImportProcess) ? DataManagerStrings.ImportOperation : DataManagerStrings.ExportOperation);

			if (isImportProcess)
				UseUtcFormatCheckBox.Checked = defaultSel.ImportSel.UseUtcDateTimeFormat;
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo all'export selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (isImportProcess)
			{
				defaultSel.ImportSel.ImportTBCreated		= TBCreatedCheckBox.Checked;
				defaultSel.ImportSel.ImportTBModified		= TBModifiedCheckBox.Checked;
				defaultSel.ImportSel.UseUtcDateTimeFormat	= UseUtcFormatCheckBox.Checked;
			}
			else
			{
				defaultSel.ExportSel.ExportTBCreated	= TBCreatedCheckBox.Checked;
				defaultSel.ExportSel.ExportTBModified	= TBModifiedCheckBox.Checked;
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
				if (defaultSel.ExportSel.WriteQuery)
					return "AddWhereClausePage";

				if (defaultSel.ExportSel.SelectColumns)
					return "ColumnsSelectionsListPage";

				if (!defaultSel.ExportSel.AllTables)
					return "TablesSelectionsListPage";

				return "TablesParamPage";
			}

			return "FilesSelectionPage";
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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerDefault + "BaseColumnsPage");
			return true;
		}
		#endregion
	}
}
