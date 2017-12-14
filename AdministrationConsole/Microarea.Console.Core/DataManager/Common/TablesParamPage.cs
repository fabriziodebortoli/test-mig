
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Common
{
	//=========================================================================
	public partial class TablesParamPage : InteriorWizardPage
	{
        private ExportSelections expSelections = null;
		private bool fromDefault	= false;
		private bool fromSample		= false;
		private Images myImages = null;		

		//---------------------------------------------------------------------
		public TablesParamPage()
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

			fromDefault = (((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections() != null) ? true : false;
			if (fromDefault)
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetDefaultBmpSmallIndex()];

			fromSample = (((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false;
			if (fromSample)
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetSampleBmpSmallIndex()];
		}
		# endregion

		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			SetImageInHeaderPicture();

			expSelections = ((Common.DataManagerWizard)this.WizardManager).GetExportSelections();

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
			SelColumnsCheckBox.Visible		= !fromSample;
			SelColumnsCheckBox.Checked		= expSelections.SelectColumns;
			AllTablesRadioButton.Checked	= expSelections.AllTables;
			SelTablesRadioButton.Checked	= !AllTablesRadioButton.Checked;
			WriteQueryCheckBox.Checked		= expSelections.WriteQuery;
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo all'export selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (!fromSample)
				expSelections.SelectColumns = SelColumnsCheckBox.Checked;

			expSelections.AllTables = AllTablesRadioButton.Checked;
			expSelections.WriteQuery = WriteQueryCheckBox.Checked;
		}
		# endregion

		#region OnWizardBack e OnWizardNext
		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();

			if (fromDefault || fromSample)
				return "ChooseOperationPage";

			return base.OnWizardBack();
		}

		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			GetControlsValue();
			
			if (fromSample)
			{
				if (expSelections.AllTables)
					return (expSelections.WriteQuery) ? "AddWhereClausePage" : "BaseColumnsPage";
			
				return "TablesSelectionsListPage";
			}
			else
			{
				if (expSelections.AllTables)
				{
					if (expSelections.SelectColumns)
						return "ColumnsSelectionsListPage";

					if (!expSelections.SelectColumns && expSelections.WriteQuery)
						return "AddWhereClausePage";

					if (!expSelections.SelectColumns && !expSelections.WriteQuery)
						return (fromDefault) ?  "BaseColumnsPage" : "BaseColumnsParamPage";
				}
				else
					return "TablesSelectionsListPage";
			}

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
			this.WizardManager.HelpFromWizardPage
				(
				this, 
				DataManagerConsts.NamespaceDBAdminPlugIn, 
				DataManagerConsts.NamespaceDataManagerCommon + "TablesParamPage"
				);
			return true;
		}
		#endregion

	}
}