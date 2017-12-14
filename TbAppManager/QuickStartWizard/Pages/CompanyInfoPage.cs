using System;
using System.Net;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	///<summary>
	/// Pagina di richiesta informazioni per la creazione dei database di sistema ed aziendali
	///</summary>
	//================================================================================
	public partial class CompanyInfoPage : InteriorWizardPage
	{
		private QuickStartSelections qsSelections = null;
		private Diagnostic qsDiagnostic = null;

		private TBConnection connection = null;
		private string masterConnectionString = string.Empty;

		//--------------------------------------------------------------------------------
		public CompanyInfoPage()
		{
			InitializeComponent();
		}

		# region OnSetActive e OnKillActive
		///<summary>
		/// Il metodo OnSetActive viene richiamato OGNI volta che si entra nella pagina!
		///</summary>
		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			this.m_headerPicture.Image = QuickStartStrings.QuickStartSmall;

			qsSelections = ((QuickStartWizard)this.WizardManager).QSSelections;
			qsDiagnostic = ((QuickStartWizard)this.WizardManager).QSDiagnostic;

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

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
		public override string OnWizardNext()
		{
			if (!CheckData())
				return WizardForm.NoPageChange;

			GetControlsValue();

			return base.OnWizardNext();
		}

		//---------------------------------------------------------------------
		public override string OnWizardBack()
		{
			GetControlsValue();

			return base.OnWizardBack();
		}
		# endregion

		/// <summary>
		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			if (!DesignMode)
				ComboSQLServers.InitDefaultServer(qsSelections.Server);

			TxtLogin.Text = qsSelections.Login;
			TxtPassword.Text = qsSelections.Password;

			TxtSystemDB.Text = (((QuickStartWizard)this.WizardManager).LoginManager.GetDBNetworkType() == DBNetworkType.Large) ? qsSelections.SystemDBName : DatabaseLayerConsts.StandardSystemDb;
			TxtCompanyDB.Text = qsSelections.CompanyDBName;
			TxtCompany.Text = qsSelections.CompanyName;

			RadioDefaultData.Checked = qsSelections.LoadDefaultData;

			// disabilito il cambio del nome del db di sistema solo se non sono nella StandardEdition
			TxtSystemDB.Enabled = string.Compare(((QuickStartWizard)this.WizardManager).LoginManager.GetEdition(), NameSolverStrings.StandardEdition, StringComparison.InvariantCultureIgnoreCase) != 0;
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo alle selezioni
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			qsSelections.Server = ComboSQLServers.SelectedSQLServer;
			qsSelections.Login = TxtLogin.Text;
			qsSelections.Password = TxtPassword.Text;
			qsSelections.SystemDBName = TxtSystemDB.Text;
			qsSelections.CompanyDBName = TxtCompanyDB.Text;
			qsSelections.CompanyName = TxtCompany.Text;
			qsSelections.LoadDefaultData = RadioDefaultData.Checked;
		}

		//--------------------------------------------------------------------------------
		private void ChBoxShowOptions_CheckedChanged(object sender, EventArgs e)
		{
			GBoxOptions.Visible = ((CheckBox)sender).Checked;
			LblSystemDB.Visible = ((CheckBox)sender).Checked;
			LblCompanyDB.Visible = ((CheckBox)sender).Checked;
			LblCompany.Visible = ((CheckBox)sender).Checked;
			TxtSystemDB.Visible = ((CheckBox)sender).Checked;
			TxtCompanyDB.Visible = ((CheckBox)sender).Checked;
			TxtCompany.Visible = ((CheckBox)sender).Checked;
		}

		///<summary>
		/// Il metodo Load viene richiamato SOLO la prima volta!
		///</summary>
		//--------------------------------------------------------------------------------
		private void CompanyInfoPage_Load(object sender, EventArgs e)
		{
			// nome del server di default inizializzato con il nome macchina
			string currentServerName = Dns.GetHostName().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			if (!DesignMode)
			{
				// aggiungo come come default il nome della macchina sulla quale sta girando Mago
				ComboSQLServers.InitDefaultServer(currentServerName);
				ComboSQLServers.SelectedSQLServer = currentServerName;
			}
		}

		///<summary>
		/// Controlli preventivi prima di andare nella pagina di Summary
		/// 1. tutti i campi devono essere presenti
		/// 2. la connessione al database master con le credenziali specificate deve funzionare
		/// 3. non devono esistere sul server i nomi dei database specificati
		///</summary>
		//---------------------------------------------------------------------
		private bool CheckData()
		{
			bool errorFound = false;

			if (string.IsNullOrWhiteSpace(ComboSQLServers.SelectedSQLServer))
			{
				qsDiagnostic.SetError(QuickStartStrings.NoServerSelected);
				errorFound = true;
			}

			if (string.IsNullOrWhiteSpace(TxtLogin.Text))
			{
				qsDiagnostic.SetError(QuickStartStrings.NoLoginSelected);
				errorFound = true;
			}

			if (ChBoxShowOptions.Checked)
			{
				if (string.IsNullOrWhiteSpace(TxtCompany.Text) || string.IsNullOrWhiteSpace(TxtCompanyDB.Text) || string.IsNullOrWhiteSpace(TxtSystemDB.Text))
				{
					qsDiagnostic.SetError(QuickStartStrings.MissingAdvancedOptions);
					errorFound = true;
				}
			}

			// controllo intermedio prima del test della connessione
			if (errorFound)
			{
				DiagnosticViewer.ShowDiagnosticAndClear(qsDiagnostic);
				return false;
			}

			// first: try connect to server and show error messages and check edition of sqlserver (only for standard edition)
			if (!TryToConnect())
			{
				DiagnosticViewer.ShowDiagnosticAndClear(qsDiagnostic);
				return false;
			}

			// second: check if databases already exist on server and show message!
			if (DBsAlreadyExist())
			{
				DiagnosticViewer.ShowDiagnosticAndClear(qsDiagnostic);
				return false;
			}

			return true;
		}

		# region Metodi di check connessione e esistenza databases
		///<summary>
		/// Con le credenziali inserite dall'utente provo a connettermi, cosi' 
		/// da individuare eventuali errori alla login
		///</summary>
		//---------------------------------------------------------------------
		private bool TryToConnect()
		{
			masterConnectionString = string.Format
				(
				NameSolverDatabaseStrings.SQLConnection,
				ComboSQLServers.SelectedSQLServer,
				DatabaseLayerConsts.MasterDatabase,
				TxtLogin.Text,
				TxtPassword.Text
				);

			try
			{
				connection = new TBConnection(DBMSType.SQLSERVER);
				connection.ConnectionString = masterConnectionString;
				connection.Open();
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.MenuManager.QuickStartWizard");
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyInfoPage.TryToConnect");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				qsDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorConnectionNotValid, extendedInfo);
				CloseConnection();
				return false;
			}
            // 6176 NON PIù DAL 04/02/2016 si blocca solo la dimensione e non si obbliga ad avere  il db msde
			/*// CONTROLLO SMALLNETWORK: se è la licenza è SmallNetwork il db deve essere MSDE o SqlExpress
			if (((QuickStartWizard)this.WizardManager).LoginManager.GetDBNetworkType() == DBNetworkType.Small)
			{
				if (TBCheckDatabase.GetDatabaseVersion(connection) != DatabaseVersion.MSDE)
				{
					CloseConnection();
					qsDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongSQLDatabaseVersion);
					return false;
				}
			}*/
			// non chiudo la connessione perche' mi serve nel metodo DBsAlreadyExist richiamato dopo
			return true;
		}

		///<summary>
		/// Metodo centralizzato per effettuare la chiusura della connessione
		///</summary>
		//--------------------------------------------------------------------------------
		private void CloseConnection()
		{
			if (connection != null && connection.State != System.Data.ConnectionState.Open)
			{
				connection.Close();
				connection.Dispose();
			}
		}

		///<summary>
		/// Controllo nella tabella sysdatabases se esistono i database con i nomi impostati per la creazione
		///</summary>
		//---------------------------------------------------------------------
		private bool DBsAlreadyExist()
		{
			bool existSysDB = false;
			bool existCompanyDB = false;

			try
			{
				// la connessione qui dovrebbe essere aperta da prima, altrimenti la ri-apro
				if (connection.State != System.Data.ConnectionState.Open)
				{
					connection.ConnectionString = masterConnectionString;
					connection.Open();
				}

				string countDatabase = "SELECT COUNT(*) FROM sysdatabases WHERE name = '{0}'";

				TBCommand command = new TBCommand(connection);
				command.CommandText = string.Format(countDatabase, TxtSystemDB.Text);
				existSysDB = (int)command.ExecuteScalar() > 0;

				command.CommandText = string.Format(countDatabase, TxtCompanyDB.Text);
				existCompanyDB = (int)command.ExecuteScalar() > 0;
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.MenuManager.QuickStartWizard");
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyInfoPage.DBsAlreadyExist");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				qsDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorConnectionNotValid, extendedInfo);
				return false;
			}
			finally
			{
				// chiudo cmq la connessione (che era rimasta aperta dal metodo TryToConnect)
				CloseConnection();
			}

			if (existSysDB)
				qsDiagnostic.Set(DiagnosticType.Error, string.Format(QuickStartStrings.DatabaseAlreadyExists, TxtSystemDB.Text, ComboSQLServers.SelectedSQLServer));
			if (existCompanyDB)
				qsDiagnostic.Set(DiagnosticType.Error, string.Format(QuickStartStrings.DatabaseAlreadyExists, TxtCompanyDB.Text, ComboSQLServers.SelectedSQLServer));

			return (existSysDB || existCompanyDB);
		}
		# endregion
	}
}
