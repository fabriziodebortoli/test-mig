
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Export
{
	//=========================================================================
	public partial class XmlParamsPage : InteriorWizardPage
	{
		private ExportSelections expSelections = null;
		private Common.Images myImages = null;		

		//---------------------------------------------------------------------
		public XmlParamsPage()
		{
			InitializeComponent();
			
			myImages = new Common.Images();
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Common.Images.GetExportBmpSmallIndex()];
		}

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			expSelections = ((Common.DataManagerWizard)this.WizardManager).GetExportSelections();

			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			return true;
		}

		//---------------------------------------------------------------------
        public override bool OnKillActive()
		{
			//aggiorno i criteri di selezione con il contenuto dei controls
			GetControlsValue();

			return base.OnKillActive();
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerExport + "XmlParamsPage");
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
			OneXMLForTableRadioButton.Checked = expSelections.OneFileForTable;

			AddSchemaInfoCheckBox.Checked = expSelections.SchemaInfo;
			// fix anomalia 18146: per problemi di Parse dei file con lo schema, si consente di esportare
			// le info di schema solo per SQL Server
			AddSchemaInfoCheckBox.Visible = (expSelections.ContextInfo.DbType == TaskBuilderNet.Interfaces.DBMSType.SQLSERVER);
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo all'export selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			expSelections.OneFileForTable = OneXMLForTableRadioButton.Checked;
			expSelections.SchemaInfo = AddSchemaInfoCheckBox.Checked;
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			GetControlsValue();
			return base.OnWizardNext();
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();
			return "BaseColumnsParamPage";
		}
		# endregion
	}
}
