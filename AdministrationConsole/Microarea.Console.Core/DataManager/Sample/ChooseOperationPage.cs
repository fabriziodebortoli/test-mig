using System.Collections.Specialized;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.UI.WinControls.Combo;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Sample
{
	//=========================================================================
	public partial class ChooseOperationPage : InteriorWizardPage
	{
		private bool				configurationIsChanged;
		private SampleSelections	sampleSel	= null;
		private Common.Images		myImages	= null;

		//---------------------------------------------------------------------
		public ChooseOperationPage()
		{
			InitializeComponent();	

			myImages = new Common.Images();
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Common.Images.GetSampleBmpSmallIndex()];
		}
		
		# region Inizializzazione combos con le countries e con le configurazioni (Basic...)
		//---------------------------------------------------------------------
		private void InitializeCountriesCombo()
		{
			IsoStateComboBox.LoadISOCountries(sampleSel.ContextInfo.IsoState);
			IsoStateComboBox.SetISOCountry(sampleSel.ContextInfo.IsoState);
		}

		//---------------------------------------------------------------------
		private void SetConfigurationComboBoxStyle(ComboBoxStyle style)
		{
			ConfigurationComboBox.DropDownStyle = style;
			if (ConfigurationComboBox.Items.Count > 0)
			{
				int nPos = ConfigurationComboBox.FindString(sampleSel.SelectedConfiguration);
				ConfigurationComboBox.SelectedIndex = (nPos >= 0) ? nPos : 0;
			}
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

			// carico nella combo delle configurazioni quelle disponibili per la lingua selezionata nell'apposita combo
			StringCollection sampleConf = new StringCollection();
			sampleSel.GetSampleConfigurationList(ref sampleConf, IsoStateComboBox.SelectedCountryValue);
			
			foreach (string config in sampleConf)
				ConfigurationComboBox.Items.Add(config);		

			if (ConfigurationComboBox.Items.Count > 0)
			{
				int nPos = ConfigurationComboBox.FindString(sampleSel.SelectedConfiguration);
				ConfigurationComboBox.SelectedIndex = (nPos >= 0) ? nPos : 0;
			}
		}
		
		//---------------------------------------------------------------------
		private bool CheckData()
		{
			if (IsoStateComboBox.SelectedCountryValue.Length == 0)
			{
				DiagnosticViewer.ShowError(string.Empty, DataManagerStrings.MsgLanguageNotValid, string.Empty, string.Empty, string.Empty);
				return false;
			}	

			if (ConfigurationComboBox.Text.Length == 0 || !Functions.IsValidName(ConfigurationComboBox.Text))
			{
				DiagnosticViewer.ShowError(string.Empty, DataManagerStrings.MsgConfigNotValid, string.Empty, string.Empty, string.Empty);
				return false;
			}
			return true;			
		}
		# endregion

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;			

			sampleSel = ((Common.DataManagerWizard)this.WizardManager).GetSampleSelections();
			
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
		/// per inizializzare i valori dei controls sulla base dei default e delle selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			ExportRadioButton.Checked = (sampleSel.Mode == DefaultSelections.ModeType.EXPORT);
			ImportRadioButton.Checked = !ExportRadioButton.Checked;			
			
			configurationIsChanged = false;
		}			

		/// <summary>
		/// considero i valori presenti nei control e li associo all'export selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			sampleSel.Mode = (ExportRadioButton.Checked) ? DefaultSelections.ModeType.EXPORT : DefaultSelections.ModeType.IMPORT;
			sampleSel.SelectedIsoState = ((LanguageItem)IsoStateComboBox.SelectedItem).LanguageValue;
			sampleSel.SelectedConfiguration = ConfigurationComboBox.Text;
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			if (!CheckData())
				return WizardForm.NoPageChange; 				

			GetControlsValue();

			if (sampleSel.Mode == DefaultSelections.ModeType.IMPORT)
			{
				// se l'utente ha scelto l'importazione dei dati di default e al database aziendale
				// sono collegati altri utenti non consento di procedere nell'elaborazione 
				if (((Common.DataManagerWizard)this.WizardManager).CheckFreeCompanyDBStatus())
				{
					// se qualcuno ha toccato la combo-box con le configurazioni 
					// pulisco l'array dei file da importare
					if (configurationIsChanged)
						sampleSel.ImportSel.ImportList.Clear();

					sampleSel.LoadSampleData();
					return "BaseColumnsPage";
				}
				else
				{
					DiagnosticViewer.ShowInformation(DataManagerStrings.ErrCompanyDBIsNotFree, DataManagerStrings.LblAttention);
					return "ChooseOperationPage";
				}
			}
			
			sampleSel.LoadModuleTableInfo(true);
			return "TablesParamPage";			
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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerSample + "ChooseOperationPage");
			return true;
		}
		#endregion

		# region Eventi sui vari controls della pagina
		//---------------------------------------------------------------------
		private void ExportRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			SetConfigurationComboBoxStyle((((RadioButton)sender).Checked) ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList);	
		}

		//---------------------------------------------------------------------
		private void ImportRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			SetConfigurationComboBoxStyle((!((RadioButton)sender).Checked) ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList);
		}

		//---------------------------------------------------------------------
		private void ChooseOperationPage_Load(object sender, System.EventArgs e)
		{
			InitializeCountriesCombo();			

			// inizializzo i controls
			SetControlsValue();
		}

		//---------------------------------------------------------------------
		private void ConfigurationComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// se qualcuno ha cambiato il valore nella combo delle configurazioni setto il
			// booleano a true e poi lo testo per fare la clear della importlist (se sono in importazione)
			configurationIsChanged = true;
		}

		//---------------------------------------------------------------------
		private void IsoStateComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SetConfigurationComboBox();
		}
		# endregion
	}
}