using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    //=========================================================================
    /// <summary>
    ///unifica le due form di login alla console 
    /// nata perchè necessaria per non fare if login\loginlite ad ogni 
    /// cast che serve quando si accede a questa property
    /// </summary>
    public interface ILoginFormHasError
	{
        bool HasErrors { get; set; }
    }

    /// <summary>
    /// Login.
    /// Maschera di input dati per la connessione al database di sistema
    /// </summary>
    //=========================================================================
    public partial class LoginLite : System.Windows.Forms.Form, ILoginFormHasError
    {
        #region Delegati ed Eventi
        //---------------------------------------------------------------------
        public delegate void SuccessLogOnLite(object sender, DynamicEventsArgs e);
        public event SuccessLogOnLite OnSuccessLogOnLite;

        public delegate void UnSuccessLogOnLite(object sender, System.EventArgs e);
        public event UnSuccessLogOnLite OnUnSuccessLogOnLite;

        public delegate bool ModifiedServerConnectionConfigLite(object sender, bool isCreated, string connectionString);
        public event ModifiedServerConnectionConfigLite OnModifiedServerConnectionConfigLite;


        public delegate void SendDiagnosticLite(object sender, Diagnostic diagnostic);
        public event SendDiagnosticLite OnSendDiagnosticLite;

        public delegate void LogPlugInsLite(object sender);
        public event LogPlugInsLite OnLogPlugInsLite;
		#endregion

		//---------------------------------------------------------------------
		private IContainer components;

		internal DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
        internal Diagnostic diagnostic = new Diagnostic("SysAdmin.Login");

        internal string databaseName = string.Empty;
        internal string oldConnectionSettings = string.Empty;
        internal bool hasErrors = false;
        internal bool serverConnectionNotExist = false;
        internal bool isStandardEdition = false;
        internal bool enableAutoLoginFromCmdLine = false;
		internal bool existAutoLoginParameter = false;

        internal StatusType currentStatus = StatusType.None;
        internal BrandLoader currentBrandLoader = null;

        internal Dictionary<string, string> sqlServerLoginMode = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

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
        public LoginLite(BrandLoader aBrandLoader, string currentEdition, StatusType consoleStatus, bool enableAutoLoginFromCmdLine)
        {
            InitializeComponent();

            currentStatus = consoleStatus;
            currentBrandLoader = aBrandLoader;
            isStandardEdition = (string.Compare(currentEdition, NameSolverStrings.StandardEdition, StringComparison.InvariantCultureIgnoreCase) == 0);
            this.enableAutoLoginFromCmdLine = enableAutoLoginFromCmdLine;

            if (currentBrandLoader != null)
            {
                string brandedCompany = currentBrandLoader.GetCompanyName();
                if (brandedCompany != null && brandedCompany.Length > 0)
                    this.Text = this.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
            }
        }

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
                if (OnSendDiagnosticLite != null)
                {
                    OnSendDiagnosticLite(this, diagnostic);
                    diagnostic.Clear();
                }

                //metto come default il nome del server sul quale sta girando la console
                TxtServerName.Text = currentServerName;
                return;
            }

            if (!File.Exists(BasePathFinder.BasePathFinderInstance.ServerConnectionFile))
            {
                serverConnectionNotExist = true;

                // metto come default il nome del server sul quale sta girando la console
                TxtServerName.Text = currentServerName;
            }
            else
            {
                oldConnectionSettings = InstallationData.ServerConnectionInfo.SysDBConnectionString;

                if (!string.IsNullOrEmpty(oldConnectionSettings))
                {
                    System.Data.SqlClient.SqlConnectionStringBuilder scsb =
                        new System.Data.SqlClient.SqlConnectionStringBuilder(InstallationData.ServerConnectionInfo.SysDBConnectionString);

                    currentServerName = scsb.DataSource;
                    databaseName = scsb.InitialCatalog;

                    TxtLogin.Text = scsb.IntegratedSecurity
                        ? Path.Combine(SystemInformation.UserDomainName, SystemInformation.UserName)
                        : scsb.UserID;

					// nel caso di autologin assegno anche la password
                    TxtPassword.Text = existAutoLoginParameter ? scsb.Password : string.Empty;
                }

                TxtServerName.Text = currentServerName;
            }

            if (string.IsNullOrWhiteSpace(TxtLogin.Text))
                TxtLogin.Text = currentBrandLoader.GetBrandedStringBySourceString("DefaultAdmin");
        }

        /// <summary>
        /// Lancio al SysAdmin l'evento di avvenuto LogOn e poi chiudo la form
        /// </summary>
        //---------------------------------------------------------------------
        protected void cmdCancel_Click(object sender, System.EventArgs e)
        {
            if (OnUnSuccessLogOnLite != null)
                OnUnSuccessLogOnLite(sender, e);
            this.Close();
        }

        /// <summary>
        /// cmdOK_Click
        /// </summary>
        //---------------------------------------------------------------------
        protected void cmdOK_Click(object sender, System.EventArgs e)
        {
            PerformLogin(sender);
        }

        //---------------------------------------------------------------------
        private void PerformLogin(object sender)
        {
			if (!CheckValidator())
			{
				((Button)sender).Enabled = true;
				return;
			}

			HasErrors = false;
            ((Button)sender).Enabled = false;

            AfterLogOnEventArgs logOnArgs = new AfterLogOnEventArgs();
            logOnArgs.DbServer = TxtServerName.Text;
            logOnArgs.IsNewDataBase = false;
            logOnArgs.DbDataSource = TxtDatabase.Text;
            logOnArgs.IsWindowsIntegratedSecurity = false;
            logOnArgs.DbDefaultUser = TxtLogin.Text;
            logOnArgs.DbDefaultPassword = TxtPassword.Text;
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

            if (OnSuccessLogOnLite != null)
                OnSuccessLogOnLite(this, eventArg);

            if (HasErrors)
            {
                if (Diagnostic.Error | Diagnostic.Warning)
                    DiagnosticViewer.ShowDiagnostic(Diagnostic);
                BtnOk.Enabled = true;
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
                if (OnModifiedServerConnectionConfigLite != null)
                    continueAndSaveConfig = OnModifiedServerConnectionConfigLite(sender, serverConnectionNotExist, connStrToSysDB);

                // se è andato tutto bene mi loggo, chiudo la form di login e carico i PlugIns
                if (continueAndSaveConfig)
                {
                    if (OnLogPlugInsLite != null)
                        OnLogPlugInsLite(sender);
                }
                else
                    BtnOk.Enabled = true;
            }
            else
            {
                // se la stringa di connessione memorizzata nel ServerConnection.config è uguale a quella inserita 
                // nella form procedo con la Login e carico i PlugIns
                if (!HasErrors)
                {
                    if (OnLogPlugInsLite != null)
                        OnLogPlugInsLite(sender);
                }
                else
                    BtnOk.Enabled = true;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

		#region CheckValidator - Verifica la validità dei dati inseriti dall'utente
		/// <summary>
		/// CheckValidator
		/// </summary>
		//---------------------------------------------------------------------
		private bool CheckValidator()
		{
			bool result = true;

			if (string.IsNullOrWhiteSpace(TxtDatabase.Text))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedDatabase);
				result = false;
			}
			
			if (string.IsNullOrWhiteSpace(TxtServerName.Text))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NotSelectedSQLServer);
				result = false;
			}
			if (string.IsNullOrWhiteSpace(TxtLogin.Text))
			{
				diagnostic.Set(DiagnosticType.Error, Strings.NoLoginSpecified);
				result = false;
			}

			if (!result)
				DiagnosticViewer.ShowDiagnostic(Diagnostic);

			return result;
		}
		#endregion

        ///<summary>
        /// Automatismo per allineare la password dell'utente amministratore cambiato direttamente dagli
        /// strumenti di amministrazione di SQL Server.
        /// Se ho cambiato la password dell'utente amministratore che si connette all'Administration Console
        /// faccio un update nella tabella MSD_CompanyLogins per il solo server SQL specificato, e memorizzo la
        /// nuova password criptata.
        /// Stessa cosa anche sulla tabella MSD_SlaveLogins, sempre a parita' di server.
        ///</summary>
        //---------------------------------------------------------------------
        protected void AdjustDBPasswordAdmin(AfterLogOnEventArgs logOnArgs, string connStrToSysDB)
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
   
        /// <summary>
        /// Invio l'eventuale diagnostica al SysAdmin
        /// </summary>
        //---------------------------------------------------------------------
        protected void Login_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (OnSendDiagnosticLite != null && (diagnostic.Error || diagnostic.Warning || diagnostic.Information))
                OnSendDiagnosticLite(sender, diagnostic);
        }

        /// <summary>
        /// Login_Load
        /// </summary>
        //---------------------------------------------------------------------
        protected void Login_Load(object sender, System.EventArgs e)
        {
            if (!DesignMode)
                LoadLoginForm();
        }

        //---------------------------------------------------------------------
        private void LoadLoginForm()
        {
            LoadSettingsFromServerConnectionFile();

            if (isStandardEdition)
            {
                TxtDatabase.Enabled = false;
                if (string.Compare(databaseName, DatabaseLayerConsts.StandardSystemDb, StringComparison.InvariantCultureIgnoreCase) == 0)
                    TxtDatabase.Text = databaseName;
                else
                    TxtDatabase.Text = string.Empty;
            }
            else
                TxtDatabase.Text = databaseName;

            if (Diagnostic.Error || Diagnostic.Warning)
            {
                DiagnosticViewer.ShowDiagnostic(Diagnostic);
                if (OnSendDiagnosticLite != null)
                    OnSendDiagnosticLite(this, Diagnostic);
                Diagnostic.Clear();
            }
        }

        //---------------------------------------------------------------------
        public new DialogResult ShowDialog(IWin32Window owner)
        {
            // se posso effettuare l'autologin procedo a controllare gli arguments
            if (enableAutoLoginFromCmdLine && (existAutoLoginParameter = GetAutoLoginFromCommandLine()))
            {
				LoadLoginForm();
				if (!string.IsNullOrEmpty(oldConnectionSettings))
				{
					PerformLogin(BtnOk);
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
    }
}