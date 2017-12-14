using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WinControls.Combo;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Default
{
	//=========================================================================
	public partial class ChooseOperationPage : InteriorWizardPage
	{
		private Common.Images		myImages		= null;		
		private DefaultSelections	defaultSel		= null;
		private ConfigurationInfo	localConfigInfo	= null;
		
		private bool	reloadApplication	= false;
		private string	originalTitle		= string.Empty;
		private string	fileToLoad			= string.Empty;

		//---------------------------------------------------------------------
		public ChooseOperationPage()
		{
			InitializeComponent();			

			myImages = new Common.Images();
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Common.Images.GetDefaultBmpSmallIndex()];
		}

		# region Inizializzazione combos con le countries e con le configurazioni (Basic, ...)
		//---------------------------------------------------------------------
		private void InitializeCountriesCombo()
		{
			IsoStateComboBox.LoadISOCountries(defaultSel.ContextInfo.IsoState);
			IsoStateComboBox.SetISOCountry(defaultSel.ContextInfo.IsoState);
		}

		//---------------------------------------------------------------------
		private void SetConfigurationComboBox()
		{
			ConfigurationComboBox.Items.Clear();

			ConfigurationComboBox.DropDownStyle = (ExportRadioButton.Checked)
				? ComboBoxStyle.DropDown
				: ComboBoxStyle.DropDownList;							

			if (IsoStateComboBox.SelectedCountryValue.Length == 0)
				return;

			// carico nella combo delle configurazioni quelle disponibili per la country selezionata nell'apposita combo
			StringCollection defaultConf = new StringCollection();
			defaultSel.GetDefaultConfigurationList(ref defaultConf, IsoStateComboBox.SelectedCountryValue);

			foreach (string config in defaultConf)
				ConfigurationComboBox.Items.Add(config);
						
			if (ConfigurationComboBox.Items.Count > 0)
			{
				int nPos = ConfigurationComboBox.FindString(defaultSel.SelectedConfiguration);	
				ConfigurationComboBox.SelectedIndex	= (nPos >= 0) ? nPos : 0;
			}
		}
		# endregion

		# region CheckData and ReadSelectionsFromFile
		/// <summary>
		/// Check sui valori inseriti nelle combo-box delle configurazioni e del country code
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckData()
		{
			if (IsoStateComboBox.SelectedCountryValue.Length == 0)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.MsgLanguageNotValid, DataManagerStrings.LblAttention, MessageBoxIcon.Error);
				return false;
			}	

			if (IsoStateComboBox.Text.Length == 0 || !Functions.IsValidName(IsoStateComboBox.Text))
			{
				DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.MsgConfigNotValid, DataManagerStrings.LblAttention, MessageBoxIcon.Error);
				return false;
			}
			
			if (ExportRadioButton.Checked)
			{
				if (LoadFromFileCheckBox.Checked && PathConfigFileTextBox.Text.Length == 0)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.SpecifyAnXMLFile, DataManagerStrings.LblAttention, MessageBoxIcon.Error);
					return false;
				}

				if (LoadFromFileCheckBox.Checked && 
					PathConfigFileTextBox.Text.Length > 0 &&
					!File.Exists(PathConfigFileTextBox.Text))
				{
					DiagnosticViewer.ShowCustomizeIconMessage(DataManagerEngineStrings.ErrFileNotExist, DataManagerStrings.LblAttention, MessageBoxIcon.Error);
					return false;
				}
			}

			return true;			
		}

		/// <summary>
		/// se sto esportando i dati e il file di configurazione è stato scelto
		/// 1. effettuo il parsing del file in oggetto
		/// 2. se il parsing non è andato a buon fine dò un messaggio e non procedo
		/// 3. se il parsing è andato a buon fine leggo il valore del tag <DBProvider> e lo confronto con quello 
		///    dell'installazione. se non sono uguali dò un messaggio di incongruenza e non procedo
		/// </summary>
		//---------------------------------------------------------------------
		private bool ReadSelectionsFromFile()
		{
			localConfigInfo = new ConfigurationInfo();

			// PARSE del file
			if (localConfigInfo.Parse(fileToLoad))
			{
				if (localConfigInfo.DBProvider != defaultSel.ContextInfo.DbType.ToString())
				{
					DiagnosticViewer.ShowCustomizeIconMessage
						(
						string.Format(DataManagerStrings.DBProviderNotValid, fileToLoad),
						DataManagerStrings.LblAttention, 
						MessageBoxIcon.Error
						);
					return false;
				}
			}
			else
			{
				DiagnosticViewer.ShowDiagnostic(localConfigInfo.ConfigurationInfoDiagnostic);
				return false;
			}

			if (ConfigurationComboBox.Items.Count > 0)
			{
				int nPos = ConfigurationComboBox.FindString(localConfigInfo.Configuration);	
				if (nPos >= 0)
					ConfigurationComboBox.SelectedIndex	= nPos;
				else
				{
					int pos = ConfigurationComboBox.Items.Add(localConfigInfo.Configuration);
					ConfigurationComboBox.SelectedIndex = pos;
				}
			}

			// TODO: PRIMA BISOGNA FARE IL CONTROLLO SUL COUNTRY CODE, NEL CASO SIA DIVERSO ...

			return true;
		}
		# endregion

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;			

			defaultSel = ((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections();
			
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
		# endregion
		
		# region Get e Set delle selezioni effettuate dall'utente
		/// <summary>
		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			ExportRadioButton.Checked = (defaultSel.Mode == DefaultSelections.ModeType.EXPORT);
			ImportRadioButton.Checked = (defaultSel.Mode == DefaultSelections.ModeType.IMPORT && defaultSel.ImportSel != null)
										? defaultSel.ImportSel.NoOptional 
										: !ExportRadioButton.Checked;
			
			ImportOptionalRadioButton.Checked = (defaultSel.Mode == DefaultSelections.ModeType.IMPORT && defaultSel.ImportSel != null)
												 ? !defaultSel.ImportSel.NoOptional 
												 : false;

			if (defaultSel.Mode == DefaultSelections.ModeType.EXPORT && defaultSel.ExportSel != null) 
			{
				LoadFromFileCheckBox.Checked	= defaultSel.ExportSel.LoadFromConfigurationFile;
				PathConfigFileTextBox.Text		= defaultSel.ExportSel.ConfigurationFilePathToLoad;

				if (defaultSel.ExportSel.LoadFromConfigurationFile)
					fileToLoad = defaultSel.ExportSel.ConfigurationFilePathToLoad;
			}
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo all'export selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (ExportRadioButton.Checked)
			{
				defaultSel.Mode = DefaultSelections.ModeType.EXPORT;
				// se il nome del file è cambiato allora forzo il ricaricamento delle export selections
				defaultSel.ForceSelections = (defaultSel.ExportSel.ConfigurationFilePathToLoad != fileToLoad);
				defaultSel.ExportSel.ConfigurationFilePathToLoad = fileToLoad;
				defaultSel.ExportSel.ConfigInfo = localConfigInfo;
				// se carico il file allora pre-imposto anche il salvataggio
				defaultSel.ExportSel.SaveInConfigurationFile = defaultSel.ExportSel.LoadFromConfigurationFile = LoadFromFileCheckBox.Checked;
				defaultSel.ExportSel.ConfigurationFilePathToSave = defaultSel.ExportSel.ConfigurationFilePathToLoad = PathConfigFileTextBox.Text;
			}
			else
			{
				defaultSel.Mode = DefaultSelections.ModeType.IMPORT;			
				defaultSel.ImportSel.NoOptional = !ImportOptionalRadioButton.Checked;
			}

			// se la configurazione selezionata è diversa da quella precedentemente inserita, imposto
			// la variabile reloadApplication a true, in modo da permettere di ri-caricare tutti i file
			// con la nuova configurazione prescelta
			reloadApplication = 
				string.Compare(defaultSel.SelectedConfiguration, ConfigurationComboBox.Text, true, CultureInfo.InvariantCulture) != 0;

			defaultSel.SelectedIsoState = ((LanguageItem)IsoStateComboBox.SelectedItem).LanguageValue;
			defaultSel.SelectedConfiguration = ConfigurationComboBox.Text;
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			if (!CheckData())
				return WizardForm.NoPageChange; 				

			GetControlsValue();

			// se l'utente ha scelto l'importazione dei dati di default e al database aziendale
			// sono collegati altri utenti non consento di procedere nell'elaborazione 
			if (
				defaultSel.Mode == DefaultSelections.ModeType.IMPORT && 
				!((Common.DataManagerWizard)this.WizardManager).CheckFreeCompanyDBStatus()
				)
			{
				DiagnosticViewer.ShowInformation(DataManagerStrings.ErrCompanyDBIsNotFree, DataManagerStrings.LblAttention);
				return WizardForm.NoPageChange;
			}
			
			// se sto esportando i dati e il file di configurazione scelto è valido
			// inizializzo le selezioni del wizard con quelle caricate dal file
			if (defaultSel.Mode == DefaultSelections.ModeType.EXPORT &&
				defaultSel.ExportSel.LoadFromConfigurationFile/* && defaultSel.ForceSelections*/)
				defaultSel.LoadSelectionsFromConfigurationFile();

			defaultSel.LoadModuleTableInfo(reloadApplication);
			
			// forzo la clear dei file xml se devo fare il reload dell'applicazione
			if (reloadApplication || defaultSel.ForceSelections) 
				defaultSel.ImportSel.ClearItems = true;

			return (defaultSel.Mode == DefaultSelections.ModeType.EXPORT) 
					? "TablesParamPage" 
					: "FilesSelectionPage";
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();
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
			this.WizardManager.HelpFromWizardPage
				(
				this, 
				DataManagerConsts.NamespaceDBAdminPlugIn, 
				DataManagerConsts.NamespaceDataManagerDefault + "ChooseOperationPage"
				);
			return true;
		}
		#endregion

		# region Eventi intercettati sui controls della pagina
		//---------------------------------------------------------------------
		private void ChooseOperationPage_Load(object sender, System.EventArgs e)
		{
			InitializeCountriesCombo();

			((Form)this.WizardForm).Text = originalTitle = this.WizardManager.FormTitle;

			// inizializzo i controls
			SetControlsValue();		
		}

		//---------------------------------------------------------------------
		private void SetConfigurationComboBoxStyle(ComboBoxStyle style)
		{
			ConfigurationComboBox.DropDownStyle = style;
			if (ConfigurationComboBox.Items.Count > 0)
			{
				int nPos = ConfigurationComboBox.FindString(defaultSel.SelectedConfiguration);
				ConfigurationComboBox.SelectedIndex	= (nPos >= 0) ? nPos : 0;
			}
		}

		//---------------------------------------------------------------------
		private void ExportRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				ImportOptionalRadioButton.Checked	= false;
				ImportRadioButton.Checked			= false;
				LoadFromFileCheckBox.Enabled		= true;
				fileToLoad							= string.Empty;
				PathConfigFileTextBox.Enabled		= LoadFromFileCheckBox.Checked;
				LoadConfigFileButton.Enabled		= LoadFromFileCheckBox.Checked;
			}
			
			SetConfigurationComboBoxStyle((((RadioButton)sender).Checked) ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList);
		}

		//---------------------------------------------------------------------
		private void ImportRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				ImportOptionalRadioButton.Checked	= false;
				ExportRadioButton.Checked			= false;
                ((Form)this.WizardForm).Text = originalTitle;
				fileToLoad							= string.Empty;
				LoadFromFileCheckBox.Enabled		= false;
				PathConfigFileTextBox.Enabled		= false;
				LoadConfigFileButton.Enabled		= false;

				// devo settare prima il mode per avere le ImportSel valorizzate
				defaultSel.Mode = DefaultSelections.ModeType.IMPORT;			
				defaultSel.ImportSel.ClearItems = true;
			}
			
			SetConfigurationComboBoxStyle((ExportRadioButton.Checked) ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList);			
		}

		//---------------------------------------------------------------------
		private void ImportOptionalRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				ExportRadioButton.Checked		= false;
				ImportRadioButton.Checked		= false;
                ((Form)this.WizardForm).Text = originalTitle;
				fileToLoad						= string.Empty;
				LoadFromFileCheckBox.Enabled	= false;
				PathConfigFileTextBox.Enabled	= false;
				LoadConfigFileButton.Enabled	= false;

				// devo settare prima il mode per avere le ImportSel valorizzate
				defaultSel.Mode = DefaultSelections.ModeType.IMPORT;			
				defaultSel.ImportSel.ClearItems = ((RadioButton)sender).Checked;
			}	
			
			SetConfigurationComboBoxStyle((ExportRadioButton.Checked) ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList);
		}

		//---------------------------------------------------------------------
		private void IsoStateComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SetConfigurationComboBox();
		}

		//---------------------------------------------------------------------
		private void LoadConfigFileButton_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog fileDlg = new OpenFileDialog();
			fileDlg.DefaultExt = "*.xml";
			fileDlg.CheckPathExists = true;
			fileDlg.CheckFileExists = true;
			fileDlg.Filter = "XML files (*.xml)|*.xml";

			DialogResult fileDlgResult = fileDlg.ShowDialog();
			if (fileDlgResult == DialogResult.OK)
			{
				FileInfo fi = new FileInfo(fileDlg.FileName);
				if (string.Compare(fi.Extension, NameSolverStrings.XmlExtension, true, CultureInfo.InvariantCulture) != 0)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.SpecifyAnXMLFile, DataManagerStrings.LblAttention, MessageBoxIcon.Exclamation);
					return;
				}

				fileToLoad = fi.FullName;

				if (!ReadSelectionsFromFile())
				{
					fileToLoad = string.Empty;
					return;
				}

				PathConfigFileTextBox.Text = fileToLoad;
	
				if (string.Compare(this.WizardManager.FormTitle, originalTitle, true, CultureInfo.InvariantCulture) == 0)
                    ((Form)this.WizardForm).Text = string.Concat(originalTitle, " - ", "[", fi.Name, "]");
			}
		}

		//---------------------------------------------------------------------
		private void LoadFromFileCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if (!((CheckBox)sender).Checked)
			{
                ((Form)this.WizardForm).Text = originalTitle;
				fileToLoad = string.Empty;
				SetConfigurationComboBox();
			}
			
			PathConfigFileTextBox.Enabled	= ((CheckBox)sender).Checked;
			LoadConfigFileButton.Enabled	= ((CheckBox)sender).Checked;
		}

		//---------------------------------------------------------------------
		private void PathConfigFileTextBox_Leave(object sender, System.EventArgs e)
		{
			if (((TextBox)sender).Text.Length == 0)
				return; 

			FileInfo fi = new FileInfo(((TextBox)sender).Text);
			if (!fi.Exists)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(DataManagerEngineStrings.ErrFileNotExist, DataManagerStrings.LblAttention, MessageBoxIcon.Error);
				return;
			}
			if (string.Compare(fi.Extension, NameSolverStrings.XmlExtension, true, CultureInfo.InvariantCulture) != 0)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.SpecifyAnXMLFile, DataManagerStrings.LblAttention, MessageBoxIcon.Exclamation);
				return;
			}

			fileToLoad = fi.FullName;

			if (!ReadSelectionsFromFile())
			{
				fileToLoad = string.Empty;
				return;
			}

			if (string.Compare(this.WizardManager.FormTitle, originalTitle, true, CultureInfo.InvariantCulture) == 0)
                ((Form)this.WizardForm).Text = string.Concat(originalTitle, " - ", "[", fi.Name, "]");
		}
		# endregion		
	}
}