using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Import
{
	//=========================================================================
	public partial class FilesParamPage : InteriorWizardPage
	{
		private ImportSelections importSel = null;
		private Common.Images myImages = null;
		string customDir = string.Empty;

		//---------------------------------------------------------------------
		public FilesParamPage()
		{
			InitializeComponent();
			myImages = new Common.Images();
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Common.Images.GetImportBmpSmallIndex()];
		}

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			importSel = ((Common.DataManagerWizard)this.WizardManager).GetImportSelections();

			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);

			return true;
		}

		//---------------------------------------------------------------------
        public override bool OnKillActive()
		{
			return base.OnKillActive();
		}

		//---------------------------------------------------------------------
		public override string OnWizardNext()
		{
			GetControlsValue();

			if (importSel.LoadXmlToFileSystem)
			{
				if (importSel.PathFolderXml.Length == 0 || !Directory.Exists(importSel.PathFolderXml))
				{
					MessageBox.Show(DataManagerStrings.SpecifyValidPath);
					return WizardForm.NoPageChange;
				}
			}
			else
			{ 
				if (CompaniesComboBox.SelectedItem.ToString().Length == 0)
					return WizardForm.NoPageChange;
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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerImport + "FilesParamPage");
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
			SelectCompanyRadioButton.Checked	= !importSel.LoadXmlToFileSystem;
			CompaniesComboBox.Enabled			= SelectCompanyRadioButton.Checked;
			
			if (CompaniesComboBox.Enabled && importSel.Company.Length > 0)
				CompaniesComboBox.SelectedIndex = 
					(CompaniesComboBox.FindStringExact(importSel.Company) != -1)
					? CompaniesComboBox.FindStringExact(importSel.Company)
					: 0;

			BrowseRadioButton.Checked	= importSel.LoadXmlToFileSystem;
			PathTextBox.Enabled			= BrowseRadioButton.Checked;
			BrowseButton.Enabled		= BrowseRadioButton.Checked;
			PathTextBox.Text			= (PathTextBox.Enabled) ? importSel.PathFolderXml : string.Empty;
		}

		/// <summary>
		/// per memorizzare i valori dei controls delle selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (string.Compare(PathTextBox.Text, importSel.PathFolderXml, true, CultureInfo.InvariantCulture) != 0)
				importSel.Clear();

			if (CompaniesComboBox.SelectedIndex != -1)
				importSel.Company = CompaniesComboBox.SelectedItem.ToString();

			importSel.LoadXmlToFileSystem	= BrowseRadioButton.Checked;
			importSel.PathFolderXml			= PathTextBox.Text;
		}
		# endregion

		# region Caricamento delle companies presenti sul database di sistema
		/// <summary>
		/// mi connetto al db di sistema e lo interrogo per avere l'elenco delle
		/// company (che inserisco nella combo)
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadCompanies()
		{
			CompaniesComboBox.Items.Clear();

			TBConnection connection		= null; 
			IDataReader reader			= null;
			string query, companyName	= string.Empty;
	
			try
			{
                connection = new TBConnection(importSel.ContextInfo.ConnectSysDB, DBMSType.SQLSERVER);
				connection.Open();
				query = "SELECT Company	FROM MSD_Companies"; 
				TBCommand command = new TBCommand(query, connection);
				reader = command.ExecuteReader();
			}
			catch (TBException exc)
			{
				Debug.Fail(exc.Message);
				if (reader != null)
					reader.Close();
				connection.Close();
				connection.Dispose();

			}

			// se il datareader è null o non contiene almeno una riga non proseguo
			// nell'analisi dei dati
			if (reader == null || !connection.DataReaderHasRows(reader))
			{
				connection.Close();
				connection.Dispose();
				return;
			}

			while (reader.Read())
			{
				companyName	= reader["Company"].ToString();
				CompaniesComboBox.Items.Add(companyName);
			}

			reader.Close();
			connection.Close();
			connection.Dispose();

			// devo caricare tutte le company presenti nella Custom/Companies (oltre a quelle amministrate in questo system DB)
			customDir = Path.Combine(importSel.ContextInfo.PathFinder.GetCustomPath(), NameSolverStrings.Subscription); 

			DirectoryInfo dir = new DirectoryInfo(customDir);
			// se e solo se la directory esiste... allora cerco le companies al suo interno
			if (dir.Exists)
			{
				foreach (DirectoryInfo info in dir.GetDirectories())
				{
					// scarto la AllCompanies
					if (string.Compare(info.Name, NameSolverStrings.AllCompanies, true, CultureInfo.InvariantCulture) == 0)
						continue;

					// ogni cartella contenente il folder DataManager viene inserita nella combo-box
					if (Directory.Exists(importSel.ContextInfo.PathFinder.GetCustomCompanyDataManagerPath(info.Name)))
					{
						// non reinserisco una company già presente
						if (!CompaniesComboBox.Items.Contains(info.Name))
							CompaniesComboBox.Items.Add(info.Name);
					}
				}
			}
		}
		# endregion

		# region Eventi sui vari controls della pagina
		//---------------------------------------------------------------------
		private void FilesParamPage_Load(object sender, System.EventArgs e)
		{
			LoadCompanies();

			if (CompaniesComboBox.Items.Count > 0)
			{
				// inizializzo la combobox con il nome dell'azienda da cui sono partito, se esiste
				CompaniesComboBox.SelectedIndex = 
					(CompaniesComboBox.FindStringExact(importSel.ContextInfo.CompanyName) != -1)
					? CompaniesComboBox.FindStringExact(importSel.ContextInfo.CompanyName)
					: 0;
			}
		}

		//---------------------------------------------------------------------
		private void CompaniesComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (importSel.Company != CompaniesComboBox.SelectedItem.ToString())
			{
				importSel.Company = CompaniesComboBox.SelectedItem.ToString();
				importSel.Clear();
			}
		}

		//---------------------------------------------------------------------
		private void SelectCompanyRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			CompaniesComboBox.Enabled	= ((RadioButton)sender).Checked;
			PathTextBox.Enabled			= !((RadioButton)sender).Checked;
			BrowseButton.Enabled		= !((RadioButton)sender).Checked;
			importSel.Clear();
		}

		//---------------------------------------------------------------------
		private void BrowseButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.Description = DataManagerStrings.SelectFolder;
			fbd.ShowNewFolderButton = false;
			fbd.SelectedPath = (PathTextBox.Text.Length > 0) ? PathTextBox.Text : customDir;

			DialogResult fileDlgResult = fbd.ShowDialog();
			if (fileDlgResult == DialogResult.OK)
				PathTextBox.Text = fbd.SelectedPath;
		}

		//---------------------------------------------------------------------
		private void PathTextBox_Leave(object sender, EventArgs e)
		{
			if (string.Compare(PathTextBox.Text, importSel.PathFolderXml, true, CultureInfo.InvariantCulture) != 0)
				importSel.Clear();
		}
		# endregion
	}
}