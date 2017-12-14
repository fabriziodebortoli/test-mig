using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.DataManager.Default;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Common
{
	//=========================================================================
	public partial class ConfigurationFileParamPage : InteriorWizardPage
	{
		private DefaultSelections defaultSel = null;
		private bool fromDefaultOrSample = false;

		private Images myImages = null;		

		//---------------------------------------------------------------------
		public ConfigurationFileParamPage()
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

			fromDefaultOrSample = (((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections() != null) ? true : false;
			if (fromDefaultOrSample)
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetDefaultBmpSmallIndex()];

			fromDefaultOrSample = fromDefaultOrSample || ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false);
			if (fromDefaultOrSample && ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false))
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetSampleBmpSmallIndex()];
		
			this.m_headerPicture.Refresh();
		}
		# endregion

		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			defaultSel = ((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections();

			if (defaultSel == null)
				return false;

			SetImageInHeaderPicture();

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
			SaveFileCheckBox.Checked	= defaultSel.ExportSel.SaveInConfigurationFile;
			PathTextBox.Text			= defaultSel.ExportSel.ConfigurationFilePathToSave;
			PathTextBox.Enabled			= defaultSel.ExportSel.SaveInConfigurationFile;
			BrowseButton.Enabled		= defaultSel.ExportSel.SaveInConfigurationFile;
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo alle default selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			defaultSel.ExportSel.ConfigurationFilePathToSave	= PathTextBox.Text;
			defaultSel.ExportSel.SaveInConfigurationFile		= SaveFileCheckBox.Checked;
		}
		# endregion

		#region OnWizardBack e OnWizardNext
		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();
			return base.OnWizardBack();
		}

		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			GetControlsValue();

			// se si è scelto di salvare il file di configurazione e non si è specificato 
			// un path non faccio procedere
			if (defaultSel.ExportSel.SaveInConfigurationFile && 
				defaultSel.ExportSel.ConfigurationFilePathToSave.Length == 0)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.MustSpecifyFileName, DataManagerStrings.LblAttention, MessageBoxIcon.Error);
				return WizardForm.NoPageChange;
			}
	
			if (defaultSel.ExportSel.SaveInConfigurationFile && 
				defaultSel.ExportSel.ConfigurationFilePathToSave.Length > 0)
			{
				FileInfo fi = new FileInfo(defaultSel.ExportSel.ConfigurationFilePathToSave);
				// se il file non ha estensione xml non faccio procedere
				if (fi.Extension != NameSolverStrings.XmlExtension)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.SpecifyAnXMLFile, DataManagerStrings.LblAttention, MessageBoxIcon.Error);
					return WizardForm.NoPageChange;
				}

				// se il file esiste già visualizzo un msg di avvertimento
				if (File.Exists(defaultSel.ExportSel.ConfigurationFilePathToSave))
					DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.OverwriteExistingFile, DataManagerStrings.LblAttention, MessageBoxIcon.Warning);
			}

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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerCommon + "ConfigurationFileParamPage");
			return true;
		}
		#endregion

		# region Eventi sui controls
		//---------------------------------------------------------------------
		private void SaveFileCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			PathTextBox.Enabled		= ((CheckBox)sender).Checked;
			BrowseButton.Enabled	= ((CheckBox)sender).Checked;
		}

		//---------------------------------------------------------------------
		private void BrowseButton_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog fileDlg = new OpenFileDialog();
			if (PathTextBox.Text.Length > 0)
			{
				fileDlg.InitialDirectory = PathTextBox.Text;
				fileDlg.FileName = Path.GetFileName(PathTextBox.Text);
			}

			fileDlg.DefaultExt = "*.xml";
			fileDlg.CheckPathExists = true;
			fileDlg.CheckFileExists = false;
			fileDlg.Filter = "XML files (*.xml)|*.xml";

			DialogResult fileDlgResult = fileDlg.ShowDialog();
			if (fileDlgResult == DialogResult.OK)
				PathTextBox.Text = fileDlg.FileName;
		}
		# endregion
	}
}