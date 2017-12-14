using System;
using System.Threading;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.MenuManager.QuickStartWizard
{
	///<summary>
	/// Form per visualizzare l'elaborazione delle operazioni previste dal QuickStart
	///</summary>
	//================================================================================
	public partial class QSElaborationForm : Form
	{
		private QuickStartSelections qsSelections = null;
		private Diagnostic qsDiagnostic = null;
		private LoginManager loginManager = null;

		private PathFinder pathFinder = null;
		private DatabaseEngine dbEngine = null;

		private bool completeElaboration = false;
		private string authenticationTokenForLMInit = string.Empty;
		private bool executionIsRunning = false;

		//--------------------------------------------------------------------------------
		public QSElaborationForm(QuickStartSelections selections, Diagnostic diagnostic, LoginManager lm)
		{
			qsSelections = selections;
			qsDiagnostic = diagnostic;
			loginManager = lm;

			// istanzio al volo un PathFinder e gli assegno l'edizione corrente (per il caricamento dei dati di default/esempio)
			pathFinder = new PathFinder(NameSolverStrings.AllCompanies, NameSolverStrings.AllUsers);
			pathFinder.Edition = loginManager.GetEdition();

			// se il ServerConnection.Config esiste e la stringa di connessione al db di sistema non e' vuota
			// la memorizzo (mi serve poi per la Init di LoginManager)
			if (
				InstallationData.ServerConnectionInfo != null &&
				!string.IsNullOrWhiteSpace(InstallationData.ServerConnectionInfo.SysDBConnectionString)
				)
				authenticationTokenForLMInit = InstallationData.ServerConnectionInfo.SysDBConnectionString;

			InitializeComponent();

			// inizializzo la listview inserendo i vari item necessari
			InitializeListView();
		}

		///<summary>
		/// Sulla Load della form faccio partire subito l'elaborazione
		///</summary>
		//--------------------------------------------------------------------------------
		private void QSElaborationForm_Load(object sender, EventArgs e)
		{
			InitButtons();
		}

		//--------------------------------------------------------------------------------
		private void InitButtons()
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

		//--------------------------------------------------------------------------------
		private void InitializeListView()
		{
			ListOperations.Items.Clear();
			ListOperations.Columns.Clear();

			ListOperations.Columns.Add(string.Empty, 20, HorizontalAlignment.Left);
			ListOperations.Columns.Add(string.Empty, 400, HorizontalAlignment.Left);

			ListOperations.BeginUpdate();

			ListViewItem lvi = new ListViewItem();
			lvi.SubItems.Add(QuickStartStrings.SystemDBCreation);
			ListOperations.Items.Add(lvi);

			lvi = new ListViewItem();
			lvi.SubItems.Add(QuickStartStrings.SystemDBStructureCreation);
			ListOperations.Items.Add(lvi);

			lvi = new ListViewItem();
			lvi.SubItems.Add(QuickStartStrings.UserAndCompanyCreation);
			ListOperations.Items.Add(lvi);

			lvi = new ListViewItem();
			lvi.SubItems.Add(QuickStartStrings.CompanyDBCreation);
			ListOperations.Items.Add(lvi);

			lvi = new ListViewItem();
			ListOperations.Items.Add(lvi);
			lvi.SubItems.Add(QuickStartStrings.CompanyDBStructureCreation);

			lvi = new ListViewItem();
			lvi.SubItems.Add(QuickStartStrings.MandatoryColumnsCreation);
			ListOperations.Items.Add(lvi);

			lvi = new ListViewItem();
			lvi.SubItems.Add(qsSelections.LoadDefaultData ? QuickStartStrings.ImportDefaultData : QuickStartStrings.ImportSampleData);
			ListOperations.Items.Add(lvi);

			ListOperations.EndUpdate();
		}
				
		//--------------------------------------------------------------------------------
		private void StartElaboration()
		{
			// lancio l'elaborazione su un thread separato
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

		//---------------------------------------------------------------------
		private void InternalStartElaborationProcess()
		{
			executionIsRunning = true;

			SetBtnShowDiagnosticVisible(false);
			SetImageIndex(-1, MainEventType.CreateSystemDB);

			// istanzio la classe che si occupa di eseguire in cascata tutte le operazioni necessarie
			dbEngine = new DatabaseEngine(pathFinder, loginManager.GetDBNetworkType(), loginManager.GetCountry(), InstallationData.BrandLoader);

			// aggancio gli eventi
			dbEngine.GenerationEvent += new EventHandler<DBEngineEventArgs>(DatabaseEngine_GenerationEvent);
			dbEngine.GenerationMainEvent += new EventHandler<DBEngineEventArgs>(DatabaseEngine_GenerationMainEvent);

			// inizializzo le credenziali di SQL Server 
			dbEngine.Server = qsSelections.Server;
			dbEngine.User = qsSelections.Login;
			dbEngine.Password = qsSelections.Password;
	
			// inizializzo i nomi dei database e dell'azienda
			dbEngine.SystemDbName = qsSelections.SystemDBName;
			dbEngine.CompanyDbName = qsSelections.CompanyDBName;
			dbEngine.CompanyName = qsSelections.CompanyName;

			// imposto il set di dati da caricare a seconda delle selezioni
			dbEngine.LoadSampleData = !qsSelections.LoadDefaultData;
			
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

			executionIsRunning = false;
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

					case MainEventType.MandatoryColumnsCreation:
						ListOperations.Items[5].ImageIndex = index;
						break;

					case MainEventType.ImportSampleData:
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
		public void CloseThisWindow()
		{
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { CloseThisWindow(); });
				return;
			}

			SetTextInLblDetail(QuickStartStrings.InitLM);

			// reinizializzo LoginManager con le nuove informazioni
			// Alla Init del LoginManager passo invece la old string, xchè lui è sempre un colpo indietro
			int retCode = loginManager.Init(false, authenticationTokenForLMInit);
			
			SetTextInLblDetail(retCode == 0 ? DatabaseLayerStrings.ElaborationSuccessfullyCompleted : string.Format(WebServicesWrapperStrings.WebExceptionError, retCode.ToString()));

			BtnClose.Visible = true;
			LblUserInfo.Visible = true;
            LblUserInfo.Text = string.Format(QuickStartStrings.QuickStartUsers, qsSelections.Login, loginManager.GetBrandedProductTitle());
			InfoPictureBox.Visible = true;
			completeElaboration = true;

			this.Text = DatabaseLayerStrings.ElaborationCompleted;
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
				qsDiagnostic.Set(dbEngine.DbEngineDiagnostic);
				dbEngine.DbEngineDiagnostic.Clear();
			}

			if (qsDiagnostic.Error)
				DiagnosticViewer.ShowDiagnostic(qsDiagnostic, DiagnosticType.Error);
		}

		//--------------------------------------------------------------------------------
		private void QSElaborationForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (executionIsRunning)
			{
				DiagnosticViewer.ShowWarning(QuickStartStrings.UnableToCloseForm, QuickStartStrings.Close);
				e.Cancel = true;
				return;
			}

			// se l'utente chiude la finestra con la X ed ho completato l'elaborazione
			// forzo il valore di DialogResult a OK, cosi la finestra di Mago viene aperta
			if (completeElaboration)
				this.DialogResult = DialogResult.OK;
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
		/// Evento di click sul pulsante Chiudi
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnClose_Click(object sender, EventArgs e)
		{
			InitButtons();
		}
	}
}
