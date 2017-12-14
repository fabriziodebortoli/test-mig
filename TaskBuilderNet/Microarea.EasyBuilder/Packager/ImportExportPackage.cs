using System;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	//================================================================================
	internal enum ImportExportPackageType
	{
		Export,
		Import
	}

	//================================================================================
	internal partial class ImportExportPackage : ThemedForm
	{
		private const string fileType = "{0} Files|*.{0}";

		private ImportExportPackageType packageWindowType;

		//-----------------------------------------------------------------------------------------
		/// <remarks/>
		public string FullFileName { get { return txtCustomizationPath.Text; } }

		//-----------------------------------------------------------------------------------------
		/// <remarks/>
		public ImportExportPackage(ImportExportPackageType type, string initialiFileName)
		{
			InitializeComponent();
			this.packageWindowType = type;
		
			switch (type)
			{		
				case ImportExportPackageType.Export:
					btnSavePackage.Text = Resources.ExportPackage;
					this.Text = Resources.ExportPackage;

					string proposedPath = string.Empty;
					if (!string.IsNullOrEmpty(initialiFileName))
					{
						string fileWithExtension = initialiFileName + NameSolverStrings.EasyBuilderPackageExtension;
						proposedPath = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomPath(), fileWithExtension);
					}
					sfdSavePackage.InitialDirectory = BasePathFinder.BasePathFinderInstance.GetCustomPath();
					sfdSavePackage.Filter = string.Format(fileType, NameSolverStrings.EasyBuilderPackage);
					sfdSavePackage.DefaultExt = NameSolverStrings.EasyBuilderPackage;
					sfdSavePackage.FileName = proposedPath;
					sfdSavePackage.OverwritePrompt = true;
					lblDescription.Text = Resources.ExportPackageDescription;
					txtCustomizationPath.Text = proposedPath;

					break;
				case ImportExportPackageType.Import:
					btnSavePackage.Text = Resources.ImportPackage;
					this.Text = Resources.ImportPackage;
					ofdOpenPackage.InitialDirectory = BasePathFinder.BasePathFinderInstance.GetCustomPath();
					ofdOpenPackage.Multiselect = false;
					ofdOpenPackage.Filter = string.Format(fileType, NameSolverStrings.EasyBuilderPackage);
					ofdOpenPackage.DefaultExt = NameSolverStrings.EasyBuilderPackage;
					lblDescription.Text = Resources.ImportPackageDescription;
					break;
				default:
					break;
			}
		}

		//-----------------------------------------------------------------------------------------
		private void btnBrowse_Click(object sender, EventArgs e)
		{
			DialogResult result;
			switch (packageWindowType)
			{
				case ImportExportPackageType.Export:
					result = sfdSavePackage.ShowDialog(this);
					
					if (result != System.Windows.Forms.DialogResult.OK)
						return;

					txtCustomizationPath.Text = sfdSavePackage.FileName;
					break;
				case ImportExportPackageType.Import:
					result = ofdOpenPackage.ShowDialog(this);
					if (result != System.Windows.Forms.DialogResult.OK)
						return;

					txtCustomizationPath.Text = ofdOpenPackage.FileName;
					break;
				default:
					return;
			}
		}

		//-----------------------------------------------------------------------------------------
		private void btnSavePackage_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		//-----------------------------------------------------------------------------------------
		private void OpenSavePackageWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			//se è un nome non valido impedisco l'uscita dalla form
			if (this.DialogResult != DialogResult.OK)
				return;
			
			if (
				string.IsNullOrEmpty(txtCustomizationPath.Text) ||
				(packageWindowType == ImportExportPackageType.Import && !File.Exists(txtCustomizationPath.Text))
				)
			{
				MessageBox.Show
					(
						this,
						Resources.ImportExportError,
						Resources.ImportCustomization,
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
					);
				e.Cancel = true;
			}
		}

		//-----------------------------------------------------------------------------------------
		private void ofdOpenPackage_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string file = ofdOpenPackage.FileName;
			//Verifico che il file sia buono e non vuoto
			if (file.IsNullOrEmpty() || !File.Exists(file))
			{
				MessageBox.Show
					(
						this,
						Resources.ImportExportError,
						Resources.ImportCustomization,
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
					);
				e.Cancel = true;
				return;
			}

			FileInfo fi = new FileInfo(file);
			//verifico che l'estensione mi vada bene
			if (!fi.Extension.CompareNoCase(NameSolverStrings.EasyBuilderPackageExtension))
			{
				MessageBox.Show
					(
						this,
						Resources.InvalidImportFileExtension,
						Resources.ImportCustomization,
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
					);

				e.Cancel = true;
				return;
			}
		}

		private void txtCustomizationPath_TextChanged(object sender, EventArgs e)
		{
			btnSavePackage.Enabled = !String.IsNullOrWhiteSpace(txtCustomizationPath.Text);
		}
	}
}
