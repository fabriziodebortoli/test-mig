using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	///<summary>
	/// QuickStart form (miglioria 4154)
	/// Form visualizzata al primo lancio di Mago.Net (dopo la prima attivazione),
	/// per creare un database di sistema e un'azienda con i dati di esempio alla quale
	/// ci si puo' connettere (senza passare dalla Console)
	///</summary>
	//================================================================================
	public partial class QuickStart : Form
	{
		private LoginManager loginManager = null;

		private PathFinder pathFinder = null;
		private DatabaseEngine dbEngine = null;
		private Diagnostic quickStartDiagnostic = new Diagnostic("QuickStart");

		private bool isStandardEdition = false;
		private string edition = string.Empty;
		private DBNetworkType dbNetworkType = DBNetworkType.Undefined;

		TBConnection connection = null;
		private string masterConnectionString = string.Empty;

		private bool completeElaboration = false;
		private string authenticationTokenForLMInit = string.Empty;

		//--------------------------------------------------------------------------------
		public QuickStart(LoginManager loginManager)
		{
			this.loginManager = loginManager;

			edition = loginManager.GetEdition();
			dbNetworkType = loginManager.GetDBNetworkType();
			isStandardEdition = string.Compare(edition, NameSolverStrings.StandardEdition, StringComparison.InvariantCultureIgnoreCase) == 0;

			// istanzio al volo un PathFinder 
			pathFinder = new PathFinder(NameSolverStrings.AllCompanies, NameSolverStrings.AllUsers);
			// e gli assegno l'edizione corrente (per il caricamento dei dati di esempio)
			pathFinder.Edition = edition;

			// se il ServerConnection.Config esiste e la stringa di connessione al db di sistema non e' vuota
			// la memorizzo (mi serve poi per la Init di LoginManager)
			if (InstallationData.ServerConnectionInfo != null &&
				!string.IsNullOrEmpty(InstallationData.ServerConnectionInfo.SysDBConnectionString))
				authenticationTokenForLMInit = InstallationData.ServerConnectionInfo.SysDBConnectionString;

			InitializeComponent();

			// inizializzo la listview inserendo i vari item necessari
			InitializeListView();

			// nome del server di default inizializzato con il nome macchina
			string currentServerName = Dns.GetHostName().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			if (!DesignMode)
			{
				// aggiungo come come default il nome della macchina sulla quale sta girando Mago
				ComboSQLServers.InitDefaultServer(currentServerName);
				ComboSQLServers.SelectedSQLServer = currentServerName;
			}
		}

		# region InitializeListView
		//--------------------------------------------------------------------------------
		private void InitializeListView()
		{
			ListOperations.Items.Clear();
			ListOperations.Columns.Clear();

			ListOperations.Columns.Add(string.Empty, 20, HorizontalAlignment.Left);
			ListOperations.Columns.Add(string.Empty, 400, HorizontalAlignment.Left);

			ListOperations.BeginUpdate();

			ListViewItem lvi = new ListViewItem();
			lvi.SubItems.Add(DatabaseWinControlsStrings.SystemDBCreation);
			ListOperations.Items.Add(lvi);
			lvi = new ListViewItem();
			lvi.SubItems.Add(DatabaseWinControlsStrings.SystemDBStructureCreation);
			ListOperations.Items.Add(lvi);
			lvi = new ListViewItem();
			lvi.SubItems.Add(DatabaseWinControlsStrings.UserAndCompanyCreation);
			ListOperations.Items.Add(lvi);
			lvi = new ListViewItem();
			lvi.SubItems.Add(DatabaseWinControlsStrings.CompanyDBCreation);
			ListOperations.Items.Add(lvi);
			lvi = new ListViewItem();
			ListOperations.Items.Add(lvi);
			lvi.SubItems.Add(DatabaseWinControlsStrings.CompanyDBStructureCreation);
			lvi = new ListViewItem();
			lvi.SubItems.Add(DatabaseWinControlsStrings.ImportSampleData);
			ListOperations.Items.Add(lvi);
			lvi = new ListViewItem();
			lvi.SubItems.Add(DatabaseWinControlsStrings.MandatoryColumnsCreation);
			ListOperations.Items.Add(lvi);

			ListOperations.EndUpdate();
		}
		# endregion
		
		///<summary>
		/// Intercetto il click del pulsante Esegui modalita' base:
		/// - provo a connettermi al server
		/// - controllo lo stato dei databases
		/// - se e' tutto a posto abilito la seconda tab
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnRunBaseMode_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.ComboSQLServers.SelectedSQLServer))
				quickStartDiagnostic.SetError(DatabaseWinControlsStrings.NoServerSelected);

			if (string.IsNullOrEmpty(TxtLogin.Text))
				quickStartDiagnostic.SetError(DatabaseWinControlsStrings.NoLoginSelected);

			if (quickStartDiagnostic.Error)
			{
				DiagnosticViewer.ShowDiagnosticAndClear(quickStartDiagnostic);
				return;
			}

			// first: try connect to server and show error messages and check edition of sqlserver (only for standard edition)
			if (!TryToConnect())
			{
				DiagnosticViewer.ShowDiagnosticAndClear(quickStartDiagnostic);
				return;
			}

			// second: check if databases already exist on server and show message!
			if (DBsAlreadyExist())
			{
				DiagnosticViewer.ShowDiagnosticAndClear(quickStartDiagnostic);
				
				DialogResult dr = MessageBox.Show
					(
					this,
					DatabaseWinControlsStrings.ExecuteAdministrationConsoleNow,
					DatabaseWinControlsStrings.RunAdministrationConsole, 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question
					);
				if (dr == DialogResult.Yes)
				{
					RunMicroareaConsole();
					// imposto Cancel cosi' non viene visualizzato alcun msg di errore
					this.DialogResult = DialogResult.Cancel;
					this.Close();
				}
				return;
			}

			// seleziona la tab di Elaborazione
			TabControlPhantom.SelectedTab = TabPageElaboration;
		}

		///<summary>
		/// Se clicco sul pulsante AdvancedMode eseguo la MicroareaConsole
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnRunAdvancedMode_Click(object sender, EventArgs e)
		{
			DialogResult dr = MessageBox.Show
				(
				this,
				DatabaseWinControlsStrings.LaunchAdministrationConsole,
				DatabaseWinControlsStrings.RunAdministrationConsole,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
				);

			if (dr == DialogResult.Yes)
			{
				RunMicroareaConsole();
				// imposto Cancel cosi' non viene visualizzato alcun msg di errore
				this.DialogResult = DialogResult.Cancel;
				this.Close();
			}
		}

		///<summary>
		/// Eseguo in un altro processo l'Administration Console, se sto runnando lato server
		/// Altrimenti visualizzo un opportuno msg e non procedo.
		///</summary>
		//--------------------------------------------------------------------------------
		private void RunMicroareaConsole()
		{
			if (pathFinder.IsRunningInsideInstallation)
				Process.Start(pathFinder.GetMicroareaConsoleApplicationPath());
			else
				MessageBox.Show(DatabaseWinControlsStrings.RunAdministrationConsoleOnServer);
		}

		///<summary>
		/// Intercetto il click del pulsante Start, disabilito alcuni controls e lancio
		/// il thread separato
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnStartElab_Click(object sender, EventArgs e)
		{
			// se l'elaborazione e' completata allora chiudo la finestra
			// altrimenti procedo con l'avvio dell'elaborazione
			if (completeElaboration)
			{
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			else
			{
				// disabilito alcuni control
				SetControlEnabled(true);
				// eseguo l'elaborazione su un processo separato
				StartElaboration();
			}
		}

		///<summary>
		/// Lancio l'elaborazione su un thread separato
		///</summary>
		//---------------------------------------------------------------------
		private void StartElaboration()
		{
			Thread myThread = new Thread(new ThreadStart(InternalStartElaborationProcess));

			// serve per evitare l'errore "DragDrop registration did not succeed" riscontrato richiamando
			// la ShowDiagnostic statica per visualizzare i messaggi di un altro thread
			myThread.SetApartmentState(ApartmentState.STA);

			// quando si istanzia un nuovo Thread bisogna assegnargli le CurrentCulture, altrimenti le
			// traduzioni in lingue differenti da quelle del sistema operativo non funzionano!!!
			myThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			myThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			myThread.Start();
		}

		///<summary>
		/// Esecuzione vera e propria:
		/// - istanzio un PathFinder (non un BasePathFinder perche' mi serve anche l'Edizione)
		/// - istanzio un DatabaseEngine valorizzando le credenziali per accesso al server e lancio l'esecuzione
		///</summary>
		//---------------------------------------------------------------------
		private void InternalStartElaborationProcess()
		{
			SetBtnShowDiagnosticVisible(false);
			SetImageIndex(-1, MainEventType.CreateSystemDB);

			// istanzio la classe che si occupa di eseguire in cascata tutte le operazioni necessarie
			dbEngine = new DatabaseEngine(pathFinder, dbNetworkType, loginManager.GetCountry(), InstallationData.BrandLoader);

			dbEngine.GenerationEvent += new EventHandler<DBEngineEventArgs>(DatabaseEngine_GenerationEvent);
			dbEngine.GenerationMainEvent += new EventHandler<DBEngineEventArgs>(DatabaseEngine_GenerationMainEvent);
			// inizializzo le credenziali di SQL Server 
			dbEngine.Server = this.ComboSQLServers.SelectedSQLServer;
			dbEngine.User = TxtLogin.Text;
			dbEngine.Password = TxtPassword.Text;
			
			bool success = dbEngine.Execute(); // lancio l'esecuzione

			// ri-abilito alcuni control
			SetControlEnabled(false);

			if (success)
				CloseThisWindow(); // terminata l'elaborazione con successo chiudo la form
			else
			{
				SetBtnShowDiagnosticVisible(true);
				SetTextInLblDetail(DatabaseLayerStrings.ElaborationEndedWithErrors);
				ShowErrorsDiagnostic();
			}
		}

		///<summary>
		/// Evento di chiusura delle form
		/// Prima di chiuderla devo effettuare la Init di LoginManager, in modo che ricarichi le informazioni
		/// appena salvate.
		///</summary>
		//--------------------------------------------------------------------------------
		public void CloseThisWindow()
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { CloseThisWindow(); });
				return;
			}

			// reinizializzo LoginManager con le nuove informazioni
			// Alla Init del LoginManager passo invece la old string, xchè lui è sempre un colpo indietro
			loginManager.Init(false, authenticationTokenForLMInit);

			BtnBack.Visible = false;
			BtnStartElab.Visible = true;
			BtnStartElab.Text = DatabaseWinControlsStrings.Close;
			LblUserInfo.Visible = true;
			LblUserInfo.Text = string.Format(DatabaseWinControlsStrings.QuickStartUsers, TxtLogin.Text);
			InfoPictureBox.Visible = true;
			completeElaboration = true;
		}

		///<summary>
		/// Evento di click sul pulsante Mostra errori
		/// Apre un diagnostic con i soli errori
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnShowDiagnostic_Click(object sender, EventArgs e)
		{
			ShowErrorsDiagnostic();
		}

		///<summary>
		/// Evento di click sul pulsante Back, torna nella tab iniziale
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnBack_Click(object sender, EventArgs e)
		{
			quickStartDiagnostic.Clear();
			BtnShowDiagnostic.Visible = false;
			InitializeListView();

			// seleziona la tab di Info
			TabControlPhantom.SelectedTab = TabPageInfo;
		}

		///<summary>
		/// ShowErrorsDiagnostic
		/// Mostra un diagnostico con i soli messaggi di errore
		///</summary>
		//--------------------------------------------------------------------------------
		private void ShowErrorsDiagnostic()
		{
			if (dbEngine.DbEngineDiagnostic.Error)
			{
				quickStartDiagnostic.Set(dbEngine.DbEngineDiagnostic);
				dbEngine.DbEngineDiagnostic.Clear();
			}

			if (quickStartDiagnostic.Error)
				DiagnosticViewer.ShowDiagnostic(quickStartDiagnostic, DiagnosticType.Error);
		}

		//--------------------------------------------------------------------------------
		private void QuickStart_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (completeElaboration)
				this.DialogResult = DialogResult.OK;
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
				this.ComboSQLServers.SelectedSQLServer, 
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
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseWinControls");
				extendedInfo.Add(DatabaseLayerStrings.Function, "QuickStart.TryToConnect");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				quickStartDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorConnectionNotValid, extendedInfo);
				CloseConnection();
				return false;
			}
            // 6176 NON PIù dal 04/02/2016 si blocca solo dimensione  non db
			/*// CONTROLLO SMALLNETWORK: se è la licenza è SmallNetwork il db deve essere MSDE o SqlExpress
			if (dbNetworkType == DBNetworkType.Small)
			{
				if (TBCheckDatabase.GetDatabaseVersion(connection) != DatabaseVersion.MSDE)
				{
					CloseConnection();
					quickStartDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongSQLDatabaseVersion);
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
		/// Controllo nella tabella sysdatabases se esistono i database con i nomi cablati per la creazione
		///</summary>
		//---------------------------------------------------------------------
		private bool DBsAlreadyExist()
		{
			bool existSysDB = false;
			bool existCompanyDB = false;

			try
			{
				// la connessione qui dovrebbe essere aperta da prima, altrimenti la ri-apro io
				if (connection.State != System.Data.ConnectionState.Open)
				{
					connection.ConnectionString = masterConnectionString;
					connection.Open();
				}

				string countDatabase = "SELECT COUNT(*) FROM sysdatabases WHERE name = '{0}'";

				TBCommand command = new TBCommand(connection);
				command.CommandText = string.Format
					(
					countDatabase,
                    isStandardEdition ? DatabaseLayerConsts.StandardSystemDb : DatabaseLayerConsts.MagoNetSystemDBName(loginManager.GetBrandedKey("DBPrefix") as string)
					);
				existSysDB = (int)command.ExecuteScalar() > 0;

				command.CommandText = string.Format(countDatabase, DatabaseLayerConsts.MagoNetCompanyDBName(loginManager.GetBrandedKey("DBPrefix")as string));
				existCompanyDB = (int)command.ExecuteScalar() > 0;
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseWinControls");
				extendedInfo.Add(DatabaseLayerStrings.Function, "QuickStart.DBsAlreadyExist");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				quickStartDiagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorConnectionNotValid, extendedInfo);
				return false;
			}
			finally
			{
				// chiudo cmq la connessione (che era rimasta aperta dal metodo TryToConnect)
				CloseConnection();
			}

			if (existSysDB)
			{
				quickStartDiagnostic.Set(DiagnosticType.Error, string.Format(
					DatabaseWinControlsStrings.DatabaseAlreadyExists,
					isStandardEdition ? DatabaseLayerConsts.StandardSystemDb : DatabaseLayerConsts.MagoNetSystemDBName(loginManager.GetBrandedKey("DBPrefix")as string),
					ComboSQLServers.SelectedSQLServer));
			}
			if (existCompanyDB)
			{
				quickStartDiagnostic.Set(DiagnosticType.Error, string.Format(
					DatabaseWinControlsStrings.DatabaseAlreadyExists,
					DatabaseLayerConsts.MagoNetCompanyDBName(loginManager.GetBrandedKey("DBPrefix")as string), 
					ComboSQLServers.SelectedSQLServer));
			}

			return (existSysDB || existCompanyDB);
		}
		# endregion

		# region Gestione visualizzazione grafica delle informazioni
		///<summary>
		/// Evento sparato dal processo in DatabaseEngine per associare un'immagine
		/// al ListViewItem che sta per essere processato
		///</summary>
		//---------------------------------------------------------------------
		private void DatabaseEngine_GenerationMainEvent(object sender, DBEngineEventArgs e)
		{
			int imageIndex = -1;
			switch (e.EventType)
			{
				case EventType.Success:
					imageIndex = 0;
					break;

				case EventType.Error:
					imageIndex = 1;
					break;

				case EventType.Info:
				default:
					imageIndex = 2;
					break;
			}

			SetImageIndex(imageIndex, e.MainEventType);
		}

		///<summary>
		/// Evento sparato dal processo in DatabaseEngine per visualizzare un testo relativo
		/// all'elaborazione in corso
		///</summary>
		//---------------------------------------------------------------------
		private void DatabaseEngine_GenerationEvent(object sender, DBEngineEventArgs e)
		{
			SetTextInLblDetail(e.EventMessage);
		}

		//--------------------------------------------------------------------------------
		private void SetBtnShowDiagnosticVisible(bool isVisible)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetBtnShowDiagnosticVisible(isVisible); });
				return;
			}
			BtnShowDiagnostic.Visible = isVisible;
		}

		//--------------------------------------------------------------------------------
		public void SetImageIndex(int index, MainEventType mEventType)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetImageIndex(index, mEventType); });
				return;
			}

			ListOperations.BeginUpdate();

			if (index == -1)
			{
				foreach (ListViewItem lvi in ListOperations.Items)
					lvi.ImageIndex = -1;
			}
			else
			{
				switch (mEventType)
				{
					case MainEventType.CreateSystemDB:
						ListOperations.Items[0].ImageIndex = index;
						break;

					case MainEventType.CreateSystemDBStructure:
						ListOperations.Items[1].ImageIndex = index;
						break;

					case MainEventType.CreateUserAndCompany:
						ListOperations.Items[2].ImageIndex = index;
						break;

					case MainEventType.CreateCompanyDB:
						ListOperations.Items[3].ImageIndex = index;
						break;

					case MainEventType.CreateCompanyDBStructure:
						ListOperations.Items[4].ImageIndex = index;
						break;

					case MainEventType.ImportSampleData:
						ListOperations.Items[5].ImageIndex = index;
						break;

					case MainEventType.MandatoryColumnsCreation:
						ListOperations.Items[6].ImageIndex = index;
						break;
				}
			}
			ListOperations.EndUpdate();
		}

		//--------------------------------------------------------------------------------
		public void SetTextInLblDetail(string message)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetTextInLblDetail(message); });
				return;
			}
		
			LblDetail.Text = message;
		}

		//--------------------------------------------------------------------------------
		public void SetControlEnabled(bool enable)
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetControlEnabled(enable); });
				return;
			}

			this.UseWaitCursor = enable;
			this.PictBoxProgress.Visible = enable;
			BtnStartElab.Enabled = !enable;
			BtnStartElab.Visible = !enable;
			BtnBack.Visible = !enable;
		}
		# endregion
	}

	///<summary>
	/// Barbatrucco per fare in modo di avere un TabControl invisibile
	/// Ovvero in DesignMode si vedono le tab in modo da poter inserire i controlli in esse
	/// A runtime le "linguette" svaniscono, pertanto per spostarsi da una tab all'altra bisogna 
	/// farlo programmativamente con il comando TabControlPhantom.SelectedTab = tabDaSelezionare
	///</summary>
	//================================================================================
	public class InvisibleTabControl : TabControl
	{
		//--------------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			// Hide tabs by trapping the TCM_ADJUSTRECT message
			if (m.Msg == 0x1328 && !DesignMode)
				m.Result = (IntPtr)1;
			else
				base.WndProc(ref m);
		}
	}
}
