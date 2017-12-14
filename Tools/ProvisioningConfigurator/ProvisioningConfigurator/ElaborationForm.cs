using System;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

using Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator.Properties;

namespace Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator
{
	///<summary>
	/// Form per visualizzare l'elaborazione delle operazioni previste dal ProvisioningConfigurator
	///</summary>
	//================================================================================
	public partial class ElaborationForm : Form
	{
		private ProvisioningEngine provisioningEngine = null;

		private bool completeElaboration = false;
		private string authenticationTokenForLMInit = string.Empty;
		private bool executionIsRunning = false;
		private bool addCompanyMode = false;

		//--------------------------------------------------------------------------------
		public ElaborationForm(ProvisioningEngine pe, bool addCompanyMode)
		{
            provisioningEngine = pe;
			this.addCompanyMode = addCompanyMode;

			InitializeComponent();

			// inizializzo la listview inserendo i vari item necessari
			InitializeListView();
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
        ///<summary>
        /// Sulla Load della form faccio partire subito l'elaborazione
        ///</summary>
        //--------------------------------------------------------------------------------
        private void ElaborationForm_Load(object sender, EventArgs e)
        {
            InitButtons();
        }

		//--------------------------------------------------------------------------------
		private void InitializeListView()
		{
			ListOperations.Items.Clear();
			ListOperations.Columns.Clear();

			ListOperations.Columns.Add(string.Empty, 20, HorizontalAlignment.Left);
			ListOperations.Columns.Add(string.Empty, 400, HorizontalAlignment.Left);

			ListOperations.BeginUpdate();
			// idx 0
			ListViewItem lvi = new ListViewItem();
			lvi.SubItems.Add(addCompanyMode ? string.Empty : Resources.SystemDBCreation);
			ListOperations.Items.Add(lvi);
			// idx 1
			lvi = new ListViewItem();
			lvi.SubItems.Add(addCompanyMode ? string.Empty : Resources.SystemDBStructureCreation);
			ListOperations.Items.Add(lvi);
			// idx 2
			lvi = new ListViewItem();
			lvi.SubItems.Add(Resources.UsersAndCompanyCreation);
			ListOperations.Items.Add(lvi);
			// idx 3
			lvi = new ListViewItem();
			lvi.SubItems.Add(Resources.CompanyDBCreation);
            ListOperations.Items.Add(lvi);
			// idx 4
			lvi = new ListViewItem();
			lvi.SubItems.Add(Resources.DMSDBCreation);
			ListOperations.Items.Add(lvi);
			// idx 5
			lvi = new ListViewItem();
			lvi.SubItems.Add(Resources.CompanyDBStructureCreation);
            ListOperations.Items.Add(lvi);
			// idx 6
			lvi = new ListViewItem();
			lvi.SubItems.Add(Resources.MandatoryColumnsCreation);
			ListOperations.Items.Add(lvi);
			// idx 7
			//lvi = new ListViewItem();
			//lvi.SubItems.Add(Resources.ImportDefaultData);
			//ListOperations.Items.Add(lvi);

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
			if (!addCompanyMode)
				SetImageIndex(-1, MainEventType.CreateSystemDB);
			
			// aggancio gli eventi
			provisioningEngine.GenerationEvent += new EventHandler<DBEngineEventArgs>(DatabaseEngine_GenerationEvent);
			provisioningEngine.GenerationMainEvent += new EventHandler<DBEngineEventArgs>(DatabaseEngine_GenerationMainEvent);
			
			bool success = provisioningEngine.ConfigureProvisioningEnvironment(); // lancio l'esecuzione

			// ri-abilito alcuni control
			SetControlEnabled(false);

			if (success)
				CloseThisWindow(); // terminata l'elaborazione con successo chiudo la form
			else
			{
				// altrimento mostro il Diagnostic
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

					case MainEventType.CreateDMSDB:
						ListOperations.Items[4].ImageIndex = index;
						break;

					case MainEventType.CreateCompanyDBStructure:
						ListOperations.Items[5].ImageIndex = index;
						break;

					case MainEventType.MandatoryColumnsCreation:
						ListOperations.Items[6].ImageIndex = index;
						break;

					//case MainEventType.ImportSampleData:
						//ListOperations.Items[7].ImageIndex = index;
						//break;
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
			
			SetTextInLblDetail(DatabaseLayerStrings.ElaborationSuccessfullyCompleted);
			BtnClose.Visible = true;
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
            if (InvokeRequired)
            {
                this.Invoke((Action)delegate { ShowErrorsDiagnostic(); });
                return;
            }

            DiagnosticViewer.ShowDiagnostic(provisioningEngine.DatabaseEngine.DbEngineDiagnostic);
		}

		//--------------------------------------------------------------------------------
		private void ElaborationForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (executionIsRunning)
			{
				DiagnosticViewer.ShowWarning(Resources.UnableToCloseForm, Resources.Close);
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
