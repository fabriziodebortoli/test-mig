using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Windows.Forms;

using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// Login.
	/// Maschera di input dati per la connessione al database di sistema
	/// </summary>
	//=========================================================================
	public partial class Login : System.Windows.Forms.Form, ILoginFormHasError
	{
		#region Delegati ed Eventi
		//---------------------------------------------------------------------
		public delegate void SuccessLogOn(object sender, DynamicEventsArgs e);
		public event SuccessLogOn OnSuccessLogOn;

		public delegate void UnSuccessLogOn(object sender, System.EventArgs e);
		public event UnSuccessLogOn OnUnSuccessLogOn;

		public delegate bool ModifiedServerConnectionConfig(object sender, bool isCreated, string connectionString);
		public event ModifiedServerConnectionConfig OnModifiedServerConnectionConfig;

		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;

		public delegate void LogPlugIns(object sender);
		public event LogPlugIns OnLogPlugIns;
		#endregion

		//---------------------------------------------------------------------
		private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
		private Diagnostic diagnostic = new Diagnostic("SysAdmin.Login");

		private string databaseName = string.Empty;
		private string oldConnectionSettings = string.Empty;
		private bool hasErrors = false;
		private bool serverConnectionNotExist = false;
		private bool isStandardEdition = false;
		private bool enableAutoLoginFromCmdLine = false;
		internal bool existAutoLoginParameter = false;

		private StatusType currentStatus = StatusType.None;
		private BrandLoader currentBrandLoader = null;
		private LicenceInfo currentLicenceInfo = null;

		private Dictionary<string, string> sqlServerLoginMode = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

		// Properties
		//---------------------------------------------------------------------
		public bool HasErrors { get { return hasErrors; } set { hasErrors = value; } }
		public Diagnostic Diagnostic { get { return diagnostic; } set { diagnostic = value; } }

		/// <summary>
		/// Costruttore
		/// Legge il file ServerConnection.config dove sono (o dovrebbero essere contenute)
		/// le informazioni relative all'ultima connessione effettuata. Tali informazioni 
		/// vengono presentate come valori di default della form
		/// </summary>
		//---------------------------------------------------------------------
		public Login(BrandLoader aBrandLoader, LicenceInfo licenceInfo, StatusType consoleStatus, bool enableAutoLoginFromCmdLine)
		{
			InitializeComponent();

			currentStatus = consoleStatus;
			currentBrandLoader = aBrandLoader;
			currentLicenceInfo = licenceInfo;

			isStandardEdition = (string.Compare(currentLicenceInfo.Edition, NameSolverStrings.StandardEdition, StringComparison.InvariantCultureIgnoreCase) == 0);
			this.enableAutoLoginFromCmdLine = enableAutoLoginFromCmdLine;

			if (currentBrandLoader != null)
			{
				string brandedCompany = currentBrandLoader.GetCompanyName();
				if (brandedCompany != null && brandedCompany.Length > 0)
					this.Text = this.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
			}

			if (currentLicenceInfo.IsAzureSQLDatabase)
			{
				AdminConsoleLabel.ForeColor = System.Drawing.Color.Magenta;
				AdminConsoleLabel.Text = "Azure " + AdminConsoleLabel.Text;
			}
		}

		/// <summary>
		/// SettingToolTips
		/// </summary>
		//---------------------------------------------------------------------
		private void SettingToolTips()
		{
			toolTip.SetToolTip(NGSqlServersCombo, Strings.ServerSystemDbToolTip);
			toolTip.SetToolTip(txtName, Strings.UserSystemDbToolTip);
			toolTip.SetToolTip(txtPassword, Strings.UserPwdSystemDbToolTip);
			toolTip.SetToolTip(DatabasesComboBox, Strings.DatabaseSystemDbToolTip);
			toolTip.SetToolTip(txtNewDataBaseName, Strings.DatabaseSystemDbToolTip);
			toolTip.SetToolTip(rbNewDb, Strings.NewSystemDbToolTip);
			toolTip.SetToolTip(rbSelectExistedDb, Strings.ExistedSystemDbToolTip);
		}

		#region LoadSettingsFromServerConnectionFile - Valorizza la form con i dati letti dal ServerConnection.config (ultima connessione)
		/// <summary>
		/// LoadSettingsFromServerConnectionFile
		/// Leggo dal file serverConnection i settaggi dell'ultima connessione effettuata dal SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadSettingsFromServerConnectionFile()
		{
			// nome del server di default inizializzato con il nome macchina
			string currentServerName = Dns.GetHostName().ToUpper(System.Globalization.CultureInfo.InvariantCulture);

			if (InstallationData.ServerConnectionInfo == null)
			{
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotReadConfigFile);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}

				//metto come default il nome del server sul quale sta girando la console
				NGSqlServersCombo.InitDefaultServer(currentServerName);
				NGSqlServersCombo.SelectedSQLServer = currentServerName;
				return;
			}

			if (!File.Exists(BasePathFinder.BasePathFinderInstance.ServerConnectionFile))
			{
				serverConnectionNotExist = true;

				// metto come default il nome del server sul quale sta girando la console
				NGSqlServersCombo.InitDefaultServer(currentServerName);
				NGSqlServersCombo.SelectedSQLServer = currentServerName;
			}
			else
			{
				oldConnectionSettings = InstallationData.ServerConnectionInfo.SysDBConnectionString;

				if (!string.IsNullOrEmpty(oldConnectionSettings))
				{
					System.Data.SqlClient.SqlConnectionStringBuilder scsb =
						new System.Data.SqlClient.SqlConnectionStringBuilder(InstallationData.ServerConnectionInfo.SysDBConnectionString);

					currentServerName = scsb.DataSource;
					if (currentServerName.StartsWith("tcp:")) // se il nome server inizia con tcp: (sintassi per Azure) lo levo
						currentServerName = currentServerName.Substring(4);

					databaseName = scsb.InitialCatalog;

					txtName.Text = scsb.IntegratedSecurity
						? Path.Combine(SystemInformation.UserDomainName, SystemInformation.UserName)
						: scsb.UserID;

					// nel caso di autologin assegno anche la password
					txtPassword.Text = existAutoLoginParameter ? scsb.Password : string.Empty;

					rbNewDb.Enabled = true;

					NGSqlServersCombo.InitDefaultServer(currentServerName);
				}

				NGSqlServersCombo.SelectedSQLServer = currentServerName;

				DatabasesComboBox.Add(databaseName);
			}

			if (string.IsNullOrWhiteSpace(txtName.Text))
				txtName.Text = currentBrandLoader.GetBrandedStringBySourceString("DefaultAdmin");
		}
		#endregion

		#region cmdCancel_Click - Premuto bottone di Cancel
		/// <summary>
		/// Lancio al SysAdmin l'evento di avvenuto LogOn e poi chiudo la form
		/// </summary>
		//---------------------------------------------------------------------
		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			if (OnUnSuccessLogOn != null)
				OnUnSuccessLogOn(sender, e);
			this.Close();
		}
		#endregion

		#region cmdOK_Click - Premuto bottone di OK
		/// <summary>
		/// cmdOK_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void cmdOK_Click(object sender, System.EventArgs e)
		{
			PerformLogin(sender);
		}

		//---------------------------------------------------------------------
		private void PerformLogin(object sender)
		{
			if (!CheckValidator())
			{
				cmdOK.Enabled = true;
				return;
			}

			HasErrors = false;
			((Button)sender).Enabled = false;

			AfterLogOnEventArgs logOnArgs = new AfterLogOnEventArgs();
			logOnArgs.DbServer = NGSqlServersCombo.ServerName;
			logOnArgs.DbServerIstance = NGSqlServersCombo.InstanceName;

			if (rbNewDb.Checked)
			{
				logOnArgs.IsNewDataBase = true;
				logOnArgs.DbDataSource = txtNewDataBaseName.Text;
			}
			else
			{
				logOnArgs.IsNewDataBase = false;
				logOnArgs.DbDataSource = DatabasesComboBox.DataSourceName;
			}

			logOnArgs.IsWindowsIntegratedSecurity = false;
			logOnArgs.DbDefaultUser = txtName.Text;
			logOnArgs.DbDefaultPassword = txtPassword.Text;
			logOnArgs.DbOwner = null;

			DynamicEventsArgs eventArg = new DynamicEventsArgs();
			eventArg.Add(logOnArgs);

			// se il controllo del CheckValidator e'andato a buon fine ri-compongo la stringa di connessione 
			// impostando il nome del database di sistema scelto dall'utente
			string connStrToSysDB =
				(logOnArgs.IsWindowsIntegratedSecurity)
				? string.Format
					(
					NameSolverDatabaseStrings.SQLWinNtConnection,
					(logOnArgs.DbServerIstance.Length > 0)
					? Path.Combine(logOnArgs.DbServer, logOnArgs.DbServerIstance) : logOnArgs.DbServer,
					logOnArgs.DbDataSource
					)
				: string.Format
					(
					NameSolverDatabaseStrings.SQLConnection,
					(logOnArgs.DbServerIstance.Length > 0)
					? Path.Combine(logOnArgs.DbServer, logOnArgs.DbServerIstance) : logOnArgs.DbServer,
					logOnArgs.DbDataSource,
					logOnArgs.DbDefaultUser,
					logOnArgs.DbDefaultPassword
					);

			if (currentLicenceInfo.IsAzureSQLDatabase)
				connStrToSysDB = string.Format(NameSolverDatabaseStrings.SQLAzureConnection,
					(logOnArgs.DbServerIstance.Length > 0) ? Path.Combine(logOnArgs.DbServer, logOnArgs.DbServerIstance) : logOnArgs.DbServer,
					logOnArgs.DbDataSource,
					logOnArgs.DbDefaultUser,
					logOnArgs.DbDefaultPassword);

			// sparo l'evento di OnSuccessLogOn (intercettato dal SysAdmin)
			OnSuccessLogOn?.Invoke(this, eventArg);

			if (HasErrors)
			{
				if (Diagnostic.Error | Diagnostic.Warning)
					DiagnosticViewer.ShowDiagnostic(Diagnostic);
				cmdOK.Enabled = true;
				return;
			}

			// se la stringa di connessione memorizzata nel ServerConnection.config è diversa da quella appena
			// inserita nella form di Login, devo contattare il LoginManager perchè devo rifare la sua Init 
			// Demando alla Console tale attività. A seconda di cosa l'utente risponde durante il processo 
			// (se ci sono Client connessi devo disconnetterli) posso continuare e loggarmi, oppure no
			if (string.Compare(oldConnectionSettings, connStrToSysDB, StringComparison.InvariantCultureIgnoreCase) != 0 &&
					InstallationData.ServerConnectionInfo != null)
			{
				// se e' cambiata la stringa di connessione vado ad aggiornare l'eventuale nuova password criptata
				// nella tabella MSD_CompanyLogins
				AdjustDBPasswordAdmin(logOnArgs, connStrToSysDB);

				bool continueAndSaveConfig = false;
				// L'evento viene ricevuto dal SysAdmin che provvede ad interrogare la Console e ad
				// effettuare tutte le operazioni necessarie per re-inizializzare il LoginManager e/o
				// disconnettere i client eventualmente connessi
				if (OnModifiedServerConnectionConfig != null)
					continueAndSaveConfig = OnModifiedServerConnectionConfig(sender, serverConnectionNotExist, connStrToSysDB);

				// se è andato tutto bene mi loggo, chiudo la form di login e carico i PlugIns
				if (continueAndSaveConfig)
					OnLogPlugIns?.Invoke(sender);
				else
					cmdOK.Enabled = true;
			}
			else
			{
				// se la stringa di connessione memorizzata nel ServerConnection.config è uguale a quella inserita 
				// nella form procedo con la Login e carico i PlugIns
				if (!HasErrors)
					OnLogPlugIns?.Invoke(sender);
				else
					cmdOK.Enabled = true;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		///<summary>
		/// Automatismo per allineare la password dell'utente amministratore cambiato direttamente dagli
		/// strumenti di amministrazione di SQL Server.
		/// Se ho cambiato la password dell'utente amministratore che si connette all'Administration Console
		/// faccio un update nella tabella MSD_CompanyLogins per il solo server SQL specificato, e memorizzo la
		/// nuova password criptata.
		/// Stessa cosa anche sulla tabella MSD_SlaveLogins, sempre a parita' di server.
		///</summary>
		//---------------------------------------------------------------------
		private void AdjustDBPasswordAdmin(AfterLogOnEventArgs logOnArgs, string connStrToSysDB)
		{
			// compongo la stringa di UPDATE in questo brutto modo (senza usare i parametri)
			// perche' per fare in modo che nella clausola di WHERE (dove confronto il nome del server)
			// sia possibile indicare anche un nomeserver\nomeistanza.
			// usando i parametri, la stringa nomeserver\nomeistanza viene "escapata" con nomeserver\\nomeistanza
			// peccato che viene passata alla WHERE senza togliere il carattere di escape, pertanto la query fallisce sempre
			// ho analizzato lo strano comportamento con tutta la mia stanza, ma ad oggi non siamo riusciti a capire come fare.
			string updateCompanyLogins = string.Empty, updateSlaveLogins = string.Empty;

			if (!string.IsNullOrWhiteSpace(logOnArgs.DbServerIstance))
			{
				updateCompanyLogins = string.Format(@"UPDATE MSD_CompanyLogins SET MSD_CompanyLogins.DBPassword = '{0}'
					FROM MSD_Companies INNER JOIN MSD_CompanyLogins ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId 
					INNER JOIN MSD_Logins ON MSD_Companies.CompanyDBOwner = MSD_Logins.LoginId AND 
                    MSD_CompanyLogins.LoginId = MSD_Logins.LoginId WHERE (MSD_Logins.Login = '{1}') 
					AND (MSD_Companies.CompanyDBServer = '{2}\{3}')",
					Crypto.Encrypt(logOnArgs.DbDefaultPassword),
					logOnArgs.DbDefaultUser,
					logOnArgs.DbServer,
					logOnArgs.DbServerIstance);

				updateSlaveLogins = string.Format(@"UPDATE MSD_SlaveLogins SET MSD_SlaveLogins.SlaveDBPassword = '{0}'
					FROM MSD_Logins INNER JOIN
					MSD_SlaveLogins ON MSD_Logins.LoginId = MSD_SlaveLogins.LoginId INNER JOIN
					MSD_CompanyDBSlaves ON MSD_Logins.LoginId = MSD_CompanyDBSlaves.SlaveDBOwner AND 
					MSD_SlaveLogins.SlaveId = MSD_CompanyDBSlaves.SlaveId
					WHERE (MSD_Logins.Login = '{1}') AND (MSD_CompanyDBSlaves.ServerName = '{2}\{3}')",
					Crypto.Encrypt(logOnArgs.DbDefaultPassword),
					logOnArgs.DbDefaultUser,
					logOnArgs.DbServer,
					logOnArgs.DbServerIstance);
			}
			else
			{ 
				updateCompanyLogins = string.Format(@"UPDATE MSD_CompanyLogins SET MSD_CompanyLogins.DBPassword = '{0}'
					FROM MSD_Companies INNER JOIN MSD_CompanyLogins ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId 
					INNER JOIN MSD_Logins ON MSD_Companies.CompanyDBOwner = MSD_Logins.LoginId AND 
                    MSD_CompanyLogins.LoginId = MSD_Logins.LoginId WHERE (MSD_Logins.Login = '{1}') 
					AND (MSD_Companies.CompanyDBServer = '{2}')",
					Crypto.Encrypt(logOnArgs.DbDefaultPassword),
					logOnArgs.DbDefaultUser,
					logOnArgs.DbServer);

				updateSlaveLogins = string.Format(@"UPDATE MSD_SlaveLogins SET MSD_SlaveLogins.SlaveDBPassword = '{0}'
					FROM MSD_Logins INNER JOIN
					MSD_SlaveLogins ON MSD_Logins.LoginId = MSD_SlaveLogins.LoginId INNER JOIN
					MSD_CompanyDBSlaves ON MSD_Logins.LoginId = MSD_CompanyDBSlaves.SlaveDBOwner AND 
					MSD_SlaveLogins.SlaveId = MSD_CompanyDBSlaves.SlaveId
					WHERE (MSD_Logins.Login = '{1}') AND (MSD_CompanyDBSlaves.ServerName = '{2}')",
					Crypto.Encrypt(logOnArgs.DbDefaultPassword),
					logOnArgs.DbDefaultUser,
					logOnArgs.DbServer);
			}

			TBConnection tbConn = null;
			TBCommand tbCommand = null;

			try
			{
				// istanzio al volo una connessione al database di sistema
				tbConn = new TBConnection(connStrToSysDB, DBMSType.SQLSERVER);
				tbConn.Open();

				// eseguo la query di update della password nella MSD_CompanyLogins
				tbCommand = new TBCommand(updateCompanyLogins, tbConn);
				tbCommand.ExecuteNonQuery();

				// eseguo la query di update della password nella MSD_SlaveLogins
				tbCommand.CommandText = updateSlaveLogins;
				tbCommand.ExecuteNonQuery();

				tbCommand.Dispose();
			}
			catch (TBException)
			{
			}
			finally
			{
				if (tbConn != null && tbConn.State == System.Data.ConnectionState.Open)
				{
					tbConn.Close();
					tbConn.Dispose();
				}
			}
		}
		#endregion

		#region CheckValidator - Verifica la validità dei dati inseriti dall'utente
		/// <summary>
		/// CheckValidator
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckValidator()
		{
			bool result = true;

			if (rbSelectExistedDb.Checked && DatabasesComboBox.DataSourceName.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedDatabase);
				result = false;
			}
			if (rbNewDb.Checked && txtNewDataBaseName.Text.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.SystemDatabaseEmpty);
				result = false;
			}
			if (NGSqlServersCombo.SelectedSQLServer.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedSQLServer);
				result = false;
			}
			if (txtName.Text.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Error, string.Format(Strings.NoEmptyValue, lblName.Text));
				result = false;
			}

			if (!result)
				DiagnosticViewer.ShowDiagnostic(Diagnostic);

			return result;
		}
		#endregion

		#region Eventi sui Controlli nella Form

		/// <summary>
		/// Sto caricando la lista dei dbs presenti sul server
		/// </summary>
		//---------------------------------------------------------------------
		private void rbSelectExistedDb_CheckedChanged(object sender, System.EventArgs e)
		{
			// se è standard edition e il serverDataBase <> dal fixed db lo mando in new
			if (isStandardEdition)
			{
				DatabasesComboBox.ServerName = NGSqlServersCombo.SelectedSQLServer;
				DatabasesComboBox.IsWindowsAuthentication = false;
				DatabasesComboBox.UserName = txtName.Text;
				DatabasesComboBox.UserPassword = txtPassword.Text;
				DatabasesComboBox.Enabled = false;
				DatabasesComboBox.ClearListOfDb();
				DatabasesComboBox.Add(DatabaseLayerConsts.StandardSystemDb);
				txtNewDataBaseName.Enabled = false;
				txtNewDataBaseName.Text = string.Empty;
				DatabasesComboBox.DataSourceName = DatabaseLayerConsts.StandardSystemDb;
			}
			else
			{
				DatabasesComboBox.Enabled = true;
				DatabasesComboBox.ClearListOfDb();
				DatabasesComboBox.DataSourceName = string.Empty;
				DatabasesComboBox.ServerName = string.Empty;
				DatabasesComboBox.UserName = string.Empty;
				DatabasesComboBox.UserPassword = string.Empty;
				txtNewDataBaseName.Text = string.Empty;
				txtNewDataBaseName.Enabled = false;
				DatabasesComboBox.Focus();
			}
		}

		/// <summary>
		/// Ho scelto di creare un nuovo databasese
		/// </summary>
		//---------------------------------------------------------------------
		private void rbNewDb_CheckedChanged(object sender, System.EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				if (isStandardEdition)
				{
					DatabasesComboBox.ClearListOfDb();
					DatabasesComboBox.DataSourceName = string.Empty;
					txtNewDataBaseName.Enabled = false;
					txtNewDataBaseName.Text = DatabaseLayerConsts.StandardSystemDb;
				}
				else
				{
					rbSelectExistedDb.Checked = false;
					DatabasesComboBox.Enabled = false;
					txtNewDataBaseName.Text = string.Empty;
					txtNewDataBaseName.Enabled = true;
					DatabasesComboBox.ClearListOfDb();
					DatabasesComboBox.DataSourceName = string.Empty;
					txtNewDataBaseName.Focus();
				}
			}
		}

		/// <summary>
		/// Sto inserendo il nome del nuovo db
		/// </summary>
		//---------------------------------------------------------------------
		private void txtNewDataBaseName_TextChanged(object sender, System.EventArgs e)
		{
			DatabasesComboBox.DataSourceName = ((TextBox)sender).Text;
		}

		/// <summary>
		/// Browsing dei Databases
		/// </summary>
		//---------------------------------------------------------------------
		private void DatabasesComboBox_OnDropDownDatabases(object sender)
		{
			DatabasesComboBox.ServerName = NGSqlServersCombo.SelectedSQLServer;

			DatabasesComboBox.IsWindowsAuthentication = false;
			DatabasesComboBox.UserName = txtName.Text;
			DatabasesComboBox.UserPassword = txtPassword.Text;
			DatabasesComboBox.ProviderType = DBMSType.SQLSERVER;
			DatabasesComboBox.ClearListOfDb();
			DatabasesComboBox.LoadAllDataBases();

			if (DatabasesComboBox.Diagnostic.Error || DatabasesComboBox.Diagnostic.Warning)
			{
				Diagnostic.Set(DatabasesComboBox.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, Diagnostic);
				DatabasesComboBox.Diagnostic.Clear();
			}
		}

		/// <summary>
		/// Sto editando il nome utente
		/// </summary>
		//---------------------------------------------------------------------
		private void txtName_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (rbNewDb.Checked)
			{
				databaseName = txtNewDataBaseName.Text;
				DatabasesComboBox.ServerName = databaseName;
			}
		}
		#endregion

		/// <summary>
		/// Invio l'eventuale diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void Login_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null && (diagnostic.Error || diagnostic.Warning || diagnostic.Information))
				OnSendDiagnostic(sender, diagnostic);
		}

		#region Login_Load - Caricamento della form
		/// <summary>
		/// Login_Load
		/// </summary>
		//---------------------------------------------------------------------
		private void Login_Load(object sender, System.EventArgs e)
		{
			LoadLoginForm();
		}

		//---------------------------------------------------------------------
		private void LoadLoginForm()
		{
			txtNewDataBaseName.Enabled = false;
			LoadSettingsFromServerConnectionFile();
			DatabasesComboBox.ClearListOfDb();

			if (isStandardEdition)
			{
				DatabasesComboBox.Enabled = false;
				if (string.Compare(databaseName, DatabaseLayerConsts.StandardSystemDb, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					DatabasesComboBox.Add(databaseName);
					DatabasesComboBox.DataSourceName = databaseName;
				}
				else
				{
					DatabasesComboBox.ClearListOfDb();
					DatabasesComboBox.DataSourceName = string.Empty;
					this.rbSelectExistedDb.Checked = false;
					this.rbNewDb.Checked = true;
				}
			}
			else
			{
				DatabasesComboBox.Add(databaseName);
				DatabasesComboBox.DataSourceName = databaseName;
			}

			if (Diagnostic.Error || Diagnostic.Warning)
			{
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(this, Diagnostic);
				Diagnostic.Clear();
			}

			SettingToolTips();

			//sto inizializzando il db di sistema - metto il check sul radio button  nuovo
			if (currentStatus == StatusType.StartUp)
			{
				this.rbSelectExistedDb.Checked = false;
				rbNewDb.Checked = true;
				rbNewDb_CheckedChanged(this.rbNewDb, EventArgs.Empty); // forzo il checked
			}
		}

		//---------------------------------------------------------------------
		public new DialogResult ShowDialog(IWin32Window owner)
		{
			// se posso effettuare l'autologin procedo a controllare gli arguments
			if (enableAutoLoginFromCmdLine && (existAutoLoginParameter = GetAutoLoginFromCommandLine()))
			{
				LoadLoginForm();
				if (!string.IsNullOrWhiteSpace(oldConnectionSettings))
				{
					PerformLogin(cmdOK);
					return this.DialogResult;
				}
			}
			return base.ShowDialog(owner);
		}

		/// <summary>
		/// Legge il parametro della linea di comando in modo da fare una login automatica
		/// </summary>
		/// <returns>true se nella linea di comando e' presente l'argomento necessario</returns>
		//---------------------------------------------------------------------
		private bool GetAutoLoginFromCommandLine()
		{
			CommandLineParam[] cmds = CommandLineParam.FromCommandLine();
			foreach (CommandLineParam cmd in cmds)
			{
				if (string.Compare(cmd.Name, "autologin", StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;
			}

			return false;
		}
		#endregion

		///<summary>
		/// Evento inviato dalla combobox dei SQL Server, dopo che ho effettuato la Leave del control
		///</summary>
		//---------------------------------------------------------------------
		private void NGSqlServersCombo_OnSetSelectedServerSQL(string serverName)
		{
			DatabasesComboBox.ServerName = string.Empty;
			DatabasesComboBox.UserName = string.Empty;
			DatabasesComboBox.UserPassword = string.Empty;
			txtName.Enabled = true;
			txtPassword.Enabled = true;

			if (isStandardEdition)
			{
				if (rbNewDb.Checked)
				{
					this.txtNewDataBaseName.Text = DatabaseLayerConsts.StandardSystemDb;
					DatabasesComboBox.DataSourceName = string.Empty;
				}
				else
				{
					this.DatabasesComboBox.Add(DatabaseLayerConsts.StandardSystemDb);
					DatabasesComboBox.DataSourceName = DatabaseLayerConsts.StandardSystemDb;
				}
			}
		}

		///<summary>
		/// Evento inviato dalla combobox dei SQL Server, dopo che ho cambiato il nome del server rispetto a quello
		/// letto dal ServerConnection.config
		///</summary>
		//---------------------------------------------------------------------
		private void NGSqlServersCombo_OnChangeServerName()
		{
			txtName.Text = string.Empty;
			txtPassword.Text = string.Empty;
			DatabasesComboBox.ClearListOfDb();
			DatabasesComboBox.DataSourceName = string.Empty;
		}
	}

	/// <summary>
	/// AfterLogOnEventArgs
	/// Argomenti per l'evento OnAfterLogOn
	/// </summary>
	//=========================================================================
	public class AfterLogOnEventArgs
	{
		#region Variabili
		private string dbServer = string.Empty;
		private string dbServerIstance = string.Empty;
		private string dbDataSource = string.Empty;
		private string dbOwner = string.Empty;
		private string dbDefaultUser = string.Empty;
		private string dbDefaultPassword = string.Empty;
		private bool isNewDataBase = false;
		private bool isWindowsIntegratedSecurity = false;

		// utilizzata per la gestione della standard edition 
		// (bloccata la visibilità nella Console e relativi plugin a 2 aziende).
		private StringCollection companiesIdAdmitted = new StringCollection();
		#endregion

		#region Proprietà
		//---------------------------------------------------------------------
		public string DbServer { get { return dbServer; } set { dbServer = value; } }
		public string DbServerIstance { get { return dbServerIstance; } set { dbServerIstance = value; } }
		public string DbDataSource { get { return dbDataSource; } set { dbDataSource = value; } }
		public string DbOwner { get { return dbOwner; } set { dbOwner = value; } }
		public string DbDefaultUser { get { return dbDefaultUser; } set { dbDefaultUser = value; } }
		public string DbDefaultPassword { get { return dbDefaultPassword; } set { dbDefaultPassword = value; } }
		public bool IsNewDataBase { get { return isNewDataBase; } set { isNewDataBase = value; } }
		public bool IsWindowsIntegratedSecurity { get { return isWindowsIntegratedSecurity; } set { isWindowsIntegratedSecurity = value; } }
		public StringCollection CompaniesIdAdmitted { get { return companiesIdAdmitted; } set { companiesIdAdmitted = value; } }
		#endregion

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public AfterLogOnEventArgs()
		{
		}
	}
}
