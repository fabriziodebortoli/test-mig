
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Common
{
	//=========================================================================
	public partial class BaseColumnsParamPage : InteriorWizardPage
	{
		private ExportSelections expSelections = null;
		private ImportSelections importSelections = null;
		
		private bool fromImport = false;
		private Images myImages = null;		

		//---------------------------------------------------------------------
		public BaseColumnsParamPage()
		{
			InitializeComponent();

			myImages = new Images();
		}

		# region SetImageInHeaderPicture
		//---------------------------------------------------------------------
		private void SetImageInHeaderPicture()
		{
			// di default metto l'image dell'export
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetExportBmpSmallIndex()];

			if (fromImport) 
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetImportBmpSmallIndex()];

			m_subtitleLabel.Text = (fromImport)
				? DataManagerStrings.BaseColumnsPageTitleForImport
				: DataManagerStrings.BaseColumnsPageTitleForExport;
		}
		# endregion

		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			expSelections = ((Common.DataManagerWizard)this.WizardManager).GetExportSelections();
			importSelections = ((Common.DataManagerWizard)this.WizardManager).GetImportSelections();
			
			fromImport = (importSelections != null) ? true : false;

			SetImageInHeaderPicture();

			// formato utc data è disponibile solo se sto importando
			DateTimeGroupBox.Visible		= fromImport;
			UseUtcFormatCheckBox.Visible	= fromImport;

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
			TBCreatedCheckBox.Checked	= (fromImport) ? importSelections.ImportTBCreated : expSelections.ExportTBCreated;
			TBModifiedCheckBox.Checked	= (fromImport) ? importSelections.ImportTBModified : expSelections.ExportTBModified;

			ParamsGroupBox.Text = string.Format(ParamsGroupBox.Text,
				(fromImport) ? DataManagerStrings.ImportOperation : DataManagerStrings.ExportOperation);

			if (fromImport)
				UseUtcFormatCheckBox.Checked = importSelections.UseUtcDateTimeFormat;
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo all'export selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (fromImport)
			{
				importSelections.ImportTBCreated		= TBCreatedCheckBox.Checked;
				importSelections.ImportTBModified		= TBModifiedCheckBox.Checked;
				importSelections.UseUtcDateTimeFormat	= UseUtcFormatCheckBox.Checked;
			}
			else
			{
				expSelections.ExportTBCreated	= TBCreatedCheckBox.Checked;
				expSelections.ExportTBModified	= TBModifiedCheckBox.Checked;
			}
		}
		# endregion

		#region OnWizardBack e OnWizardNext
		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();

			if (!fromImport)
			{
				if (expSelections.WriteQuery)
					return "AddWhereClausePage";

				if (expSelections.SelectColumns)
					return "ColumnsSelectionsListPage";

				if (!expSelections.AllTables)
					return "TablesSelectionsListPage";

				return "TablesParamPage";
			}

			return base.OnWizardBack();
		}

		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			GetControlsValue();

			if (fromImport)
				return "ImportParamsPage";

			return base.OnWizardNext();
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerCommon + "BaseColumnsParamPage");
			return true;
		}
		#endregion
	}
}
