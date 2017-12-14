using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator.Properties;

namespace Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator
{
	/// <summary>
	/// Form ad uso dal sistema di Provisioning 
	/// per la configurazione dei database e delle aziende
	/// </summary>
	//---------------------------------------------------------------------
	public partial class ProvisioningFormLITE : Form
	{
		private const string italianIsoState = "IT";
        private string instanceName= string.Empty;
        private bool preconfigurationMode = false;
		private bool loadDataFromFile = true;
		private string preconfigurationCommandLine = string.Empty;
		private bool addCompanyMode = false;

		private ProvisioningEngine provisioningEngine = null;
		private ProvisioningData provisioningData = null;

		//---------------------------------------------------------------------
		public ProvisioningData ProvisioningData
		{
			get { return provisioningData; }
			set
			{
				provisioningData = (value == null) ? new ProvisioningData() : value;

				// riempio i controls con i dati della struttura in memoria
				FillProvisioningDataInControls();
			}
		}

		/// <summary>
		/// Property che ritorna una linea di comando (/nomeparametro "valore")
		/// da utilizzare dall'esterno per il servizio di provisioning automatico
		/// </summary>
		//---------------------------------------------------------------------
		public string PreconfigurationCommandLine { get { return preconfigurationCommandLine; } }

		/// <summary>
		/// costruttore
		/// </summary>
		/// <param name="preconfigurationMode">true per aprire la form solo come configurazione senza elaborazione</param>
		/// <param name="loaddatafromfile">per inibire il caricamento dei dati da file</param>
		//---------------------------------------------------------------------
		public ProvisioningFormLITE(string instanceName = "", bool preconfigurationMode = false, bool loadDataFromFile = true )
		{
			InitializeComponent();

            this.instanceName = String.IsNullOrWhiteSpace(instanceName) ? InstallationData.InstallationName : instanceName;
                
            BtnSkip.Visible = preconfigurationMode;
            if (!preconfigurationMode) BtnOK.Location =  BtnSkip.Location;
            
			this.preconfigurationMode = preconfigurationMode;
			this.loadDataFromFile = loadDataFromFile;

			if (!DesignMode)
			{
                //// select DISTINCT country from  PAI_EndUsersMaster 
                List<string> countriesList = new List<string>()
                    {"TR", "GH", "HU", "PA", "DE", "CL", "CS", "GB", "RS", "CZ", "IT", "NL", "GR", "SM", "RO", "SI", "MT",
                    "LB", "BG", "BR", "ZA", "CH", "HR", "PL", "ID", "US", "AE", "TN", "RU", "SK", "JP", "ES", "CN", "DZ" };

                CmbCountry.Items.AddRange(countriesList.OrderBy(c => c).ToArray()); //ordinamento alfabetico
                CmbCountry.SelectedItem = italianIsoState;

				//CmbEdition.Items.Add(NameSolverStrings.ProfessionalEdition);
				//CmbEdition.SelectedIndex = 0;

				if (!loadDataFromFile)
				{
					string currentServerName = Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture);
					// aggiungo come come default il nome del server di default inizializzato con il nome macchina
					ComboSQLServers.InitDefaultServer(currentServerName);
					ComboSQLServers.SelectedSQLServer = currentServerName;
				}

                textBox1.Text = this.instanceName;
            }
}

		/// <summary>
		/// carico dal file ProvisioningData.xml le informazioni e me le metto in memoria
		/// solo se NON sono in preconfigurazione oppure se ho specificato in modo esplicito di non
		/// caricare le info
		/// </summary>
		//---------------------------------------------------------------------
		private void ProvisioningForm_Load(object sender, EventArgs e)
		{
            if (!this.preconfigurationMode && this.loadDataFromFile)
            {
                ProvisioningData = ProvisioningData.Load();
                if (ProvisioningData.CheckEmptyValues())
                    addCompanyMode = true; // se i valori non sono vuoti significa che e' almeno la seconda volta che eseguo la procedura
            }
			UpdateControls();
		}

        //---------------------------------------------------------------------
        private void UpdateControls()
        {
            ComboSQLServers.Enabled = !addCompanyMode;
            TxtSystemDB.Enabled = !addCompanyMode;
            TxtAdminName.Enabled = !addCompanyMode;
            TxtAdminPwd.Enabled = !addCompanyMode;
            TxtUserNameSQL.Enabled = !addCompanyMode;
            TxtUserPwdSQL.Enabled = !addCompanyMode;
            CmbCountry.Enabled = !addCompanyMode;
			AdminPicture.Enabled = !addCompanyMode;
			UserPicture.Enabled = !addCompanyMode;
			// CmbEdition.Enabled = !addCompanyMode;

			if (addCompanyMode)
			{
				TxtCompanyDB.Clear();
				TxtDmsDb.Clear();
				TxtCompany.Clear();
			}

			LblTitle.Text = addCompanyMode ? Resources.ConfigureExistingConfiguration : Resources.ConfigureNewEnvironment;

			BtnResetData.Visible = addCompanyMode;
		}

		/// <summary>
		/// popolo la struttura in memoria con i valori scritti dall'utente nella form
		/// </summary>
		//---------------------------------------------------------------------
		private void FillProvisioningData()
		{
			provisioningData = new ProvisioningData();
			
			provisioningData.Server = ComboSQLServers.SelectedSQLServer;
			
			provisioningData.SystemDbName = TxtSystemDB.Text;
			provisioningData.CompanyDbName = TxtCompanyDB.Text;
			provisioningData.DMSDbName = TxtDmsDb.Text;
			provisioningData.CompanyName = TxtCompany.Text;

			provisioningData.User = TxtAdminName.Text;
			provisioningData.Password = TxtAdminPwd.Text;
			provisioningData.AdminLoginName = TxtAdminName.Text;
			provisioningData.AdminLoginPassword = TxtAdminPwd.Text;
			provisioningData.UserLoginName = TxtUserNameSQL.Text;
			provisioningData.UserLoginPassword = TxtUserPwdSQL.Text;
			
			provisioningData.IsoCountry = CmbCountry.Text;
			provisioningData.Edition = NameSolverStrings.ProfessionalEdition;

			// devo assegnare anche questa property per pilotare i controlli 
			provisioningData.AddCompanyMode = addCompanyMode;
		}

		/// <summary>
		/// assegno ai controls i dati della struttura in memoria
		/// </summary>
		//---------------------------------------------------------------------
		private void FillProvisioningDataInControls()
		{
			ComboSQLServers.InitDefaultServer(provisioningData.Server);
			ComboSQLServers.SelectedSQLServer = provisioningData.Server;
			
			TxtSystemDB.Text = provisioningData.SystemDbName;
			TxtCompanyDB.Text = provisioningData.CompanyDbName;
			TxtDmsDb.Text = provisioningData.DMSDbName;
			TxtCompany.Text = provisioningData.CompanyName;
												
			TxtAdminName.Text = provisioningData.AdminLoginName;
			TxtAdminPwd.Text = provisioningData.AdminLoginPassword;
			TxtUserNameSQL.Text = provisioningData.UserLoginName;
			TxtUserPwdSQL.Text = provisioningData.UserLoginPassword;
			
			CmbCountry.Text = string.IsNullOrWhiteSpace(provisioningData.IsoCountry) ? italianIsoState : provisioningData.IsoCountry;
			//CmbEdition.SelectedItem = string.IsNullOrWhiteSpace(provisioningData.Edition) ? NameSolverStrings.ProfessionalEdition : provisioningData.Edition;
        }

        //---------------------------------------------------------------------
        private void BtnOK_Click(object sender, EventArgs e)
		{
			FillProvisioningData();

			// se sono in preconfiguration controllo solo che i valori non siano vuoti
			// e poi creo la stringa di parametri da command line da passare esternamente
			if (this.preconfigurationMode)
			{
				if (!provisioningData.CheckEmptyValues())
					DiagnosticViewer.ShowDiagnosticAndClear(ProvisioningData.ProvisioningDataDiagnostic);
				else
				{
					preconfigurationCommandLine = provisioningData.CommandLine();
                    this.DialogResult = DialogResult.OK;
					Close();
				}

				return; // devo fare return anche se sono passata dalla Close!!
			}

            //altrimenti se non sono in preconfiguration allora proseguo con l'elaborazione.
			if (!ValidateUserData())
				return;

			provisioningEngine = new ProvisioningEngine(provisioningData);

            ElaborationFormLITE ef = new ElaborationFormLITE(provisioningEngine, provisioningData.AddCompanyMode);
			if (ef.ShowDialog(this) == DialogResult.OK)
				Close();
		}

		//---------------------------------------------------------------------
		private bool ValidateUserData()
		{
			//verifica tutti campi compilati
			if (!provisioningData.Validate(true))
			{
				DiagnosticViewer.ShowDiagnosticAndClear(ProvisioningData.ProvisioningDataDiagnostic);
				return false;
			}
			
			return true;
		}

	
		
		//---------------------------------------------------------------------
		private void AdminPicture_Click(object sender, EventArgs e)
		{
			// mostro in chiaro la password
			TxtAdminPwd.UseSystemPasswordChar = !TxtAdminPwd.UseSystemPasswordChar;
		}

		//---------------------------------------------------------------------
		private void UserPicture_Click(object sender, EventArgs e)
		{
			// mostro in chiaro la password
			TxtUserPwdSQL.UseSystemPasswordChar = !TxtUserPwdSQL.UseSystemPasswordChar;
		}

		//---------------------------------------------------------------------
		private void CmbCountry_KeyPress(object sender, KeyPressEventArgs e)
		{
			// forzo l'iso stato tutto maiuscolo
			if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
				e.KeyChar = Convert.ToChar(e.KeyChar.ToString().ToUpper());
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, EventArgs e)
        {
			// perche' il cancel indica solo che non si vuole continuare, non necessariamente che ci siano errori
			DialogResult = (preconfigurationMode) ? DialogResult.Cancel : DialogResult.OK;
            Close();
        }

        //---------------------------------------------------------------------
        private void BtnSkip_Click(object sender, EventArgs e)
        {
            //per proseguire nell'installazione senza inserirre i dati che verrano poi richiesti successivamente.
            DialogResult = DialogResult.OK;
            preconfigurationCommandLine = string.Empty;
            Close();
        }

		//---------------------------------------------------------------------
		private void BtnResetData_Click(object sender, EventArgs e)
		{
			DialogResult dr = MessageBox.Show(Resources.MsgClearData, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dr == DialogResult.No)
				return;

			addCompanyMode = false;
			UpdateControls();

			provisioningData = new ProvisioningData();
		}
    }
}