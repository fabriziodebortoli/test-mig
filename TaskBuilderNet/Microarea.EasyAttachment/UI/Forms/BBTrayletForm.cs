using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.NotificationManager;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WinControls;
using SailorsPromises;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Microarea.EasyAttachment.UI.Forms
{
	public partial class BBTrayletForm : MenuTabForm
	{
		private DMSOrchestrator dmsOrchestrator = null;
		private NotificationManager notificationManager = null;
		private BBNotificationModule bbNotificationModule = null;

		private Object _lock = new Object();
		private int RefreshRequestCount = 0;

		/// <summary>
		/// i due punti in alto a sx di origine delle immagini di caricamento
		/// </summary>
		private Point PointTop = new Point(0, 0);

		private Point PointBottom = new Point(0, 0);

		private bool FormsDGVEditedByUser = false;
		private int FormsDGVLastScrollingIndex = 0;

		private bool MyRequestsDGVEditedByUser = false;
		private int MyRequestsDGVLastScrollingIndex = 0;

		/// <summary>
		/// La parte che segue (prima del costruttore) è stata fatta come ottimizzazione. Ovvero serve per richiedere
		/// come prima cosa solo le form non ancora processate a BrainBusiness (si spera siano poche)
		/// E, solo specificandolo, permette di richiedere una sola volta tutte le form, incluse quelle già
		/// processate. "Una sola volta" nel senso che la ricerca per filtri o per tipo (processate, non,...) lavora in locale,
		/// In seguito è possibile ri-effettuare la richiesta delle form a brain business (incluse o meno le processate) utilizzando
		/// il pulsante Refresh.
		/// </summary>
		private bool ProcessedFormIncludedInDGV = false;

		//il valore 999 ha il significato = non sono mai passato di qua
		private int LastSelectedIndex = 999;

		//private DataTable dt = new DataTable("Requestes");
		private BindingSource bs = new BindingSource();

		private BindingList<WFAttachmentInfo> WFAttachmentInfoBindingList = new BindingList<WFAttachmentInfo>();

		public BBTrayletForm(DMSOrchestrator orchestrator)
		{
			InitializeComponent();

			dmsOrchestrator = orchestrator;
			notificationManager = dmsOrchestrator.NotificationManager;
			bbNotificationModule = notificationManager.GetModule<BBNotificationModule>();

			InitControls();
		}

		/// <summary>
		/// una volta caricata la form, mi registro agli eventi esterni.
		/// Questo, ad esempio, per essere sicuro di non ricevere un evento di aggiornamento delle dgv, prima di averle create
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			//evento sfruttato per il posizionamento delle immagini di caricamento
			this.SizeChanged += BBTrayletForm_SizeChanged;
			MasterSplitContainer.SplitterMoved += BBTrayletForm_SizeChanged;

			FormsDGV.KeyDown += FormsDGV_KeyDown;
			FormsDGV.DoubleClick += FormsDGV_DoubleClick;
			FormsDGV.MouseClick += FormsDGV_MouseClick;
			FormsDGV.GotFocus += FormsDGV_GotFocus;
			FormsDGV.SelectionChanged += FormsDGV_SelectionChanged;
			FormsDGV.Scroll += FormsDGV_Scroll;

			MyRequestsDGV.KeyDown += MyRequestsDGV_KeyDown;
			MyRequestsDGV.DoubleClick += MyRequestsDGV_DoubleClick;
			MyRequestsDGV.MouseClick += MyRequestsDGV_MouseClick;
			MyRequestsDGV.GotFocus += MyRequestsDGV_GotFocus;
			MyRequestsDGV.SelectionChanged += MyRequestsDGV_SelectionChanged;
			MyRequestsDGV.Scroll += MyRequestsDGV_Scroll;

			//quando il modulo di brain business mi segnala una notifica, aggiorno le DGV
			bbNotificationModule.RefreshDGVS += bbNotificationModule_RefreshDGVS;
			this.HandleDestroyed += (sender, args) =>
			{ bbNotificationModule.RefreshDGVS -= bbNotificationModule_RefreshDGVS; };
		}

		/// <summary>
		/// aliinea lo stile del traylet a quello usato nel nuovo menu
		/// </summary>
		private void SetDefaultThemeColors()
		{
			MainColorsTheme colors = NotificationManagerUtility.GetMainColorsTheme();

			// buttons
			DelFormBtn.FlatAppearance.MouseDownBackColor = colors.Hover;
			DelFormBtn.FlatAppearance.MouseOverBackColor = colors.Hover;

			ShowFormBtn.FlatAppearance.MouseDownBackColor = colors.Hover;
			ShowFormBtn.FlatAppearance.MouseOverBackColor = colors.Hover;

			ShowDocumentBtnTop.FlatAppearance.MouseDownBackColor = colors.Hover;
			ShowDocumentBtnTop.FlatAppearance.MouseOverBackColor = colors.Hover;

			RefreshBtnTop.FlatAppearance.MouseDownBackColor = colors.Hover;
			RefreshBtnTop.FlatAppearance.MouseOverBackColor = colors.Hover;

			ShowDocumentBtnBottom.FlatAppearance.MouseDownBackColor = colors.Hover;
			ShowDocumentBtnBottom.FlatAppearance.MouseOverBackColor = colors.Hover;

			RefreshBtnBottom.FlatAppearance.MouseDownBackColor = colors.Hover;
			RefreshBtnBottom.FlatAppearance.MouseOverBackColor = colors.Hover;

			//if theme is dark or light set particolar color differently
			if(colors.IsBackgroundDark())
			{
				//button's border color
				DelFormBtn.FlatAppearance.BorderColor = colors.Primary;
				ShowFormBtn.FlatAppearance.BorderColor = colors.Primary;
				ShowDocumentBtnTop.FlatAppearance.BorderColor = colors.Primary;
				RefreshBtnTop.FlatAppearance.BorderColor = colors.Primary;
				ShowDocumentBtnBottom.FlatAppearance.BorderColor = colors.Primary;
				RefreshBtnBottom.FlatAppearance.BorderColor = colors.Primary;
				//labels
				Toplbl.ForeColor = colors.Primary;
				BottomLbl.ForeColor = colors.Primary;

			}
			else 
			{
				//button's border color
				DelFormBtn.FlatAppearance.BorderColor = colors.Text;
				ShowFormBtn.FlatAppearance.BorderColor = colors.Text;
				ShowDocumentBtnTop.FlatAppearance.BorderColor = colors.Text;
				RefreshBtnTop.FlatAppearance.BorderColor = colors.Text;
				ShowDocumentBtnBottom.FlatAppearance.BorderColor = colors.Text;
				RefreshBtnBottom.FlatAppearance.BorderColor = colors.Text;

				//labels
				Toplbl.ForeColor = colors.Text;
				BottomLbl.ForeColor = colors.Text;
			}
			// DGV colors
			FormsDGV.BackgroundColor = colors.Background;
			MyRequestsDGV.BackgroundColor = colors.Background;

			FormsDGV.ColumnHeadersDefaultCellStyle.BackColor = colors.Primary;
			FormsDGV.ColumnHeadersDefaultCellStyle.ForeColor = colors.Text;

			MyRequestsDGV.ColumnHeadersDefaultCellStyle.BackColor = colors.Primary;
			MyRequestsDGV.ColumnHeadersDefaultCellStyle.ForeColor = colors.Text;

			FormsDGV.DefaultCellStyle.SelectionBackColor = colors.Hover;
			//FormsDGV.DefaultCellStyle.SelectionForeColor = colors.Text;
			FormsDGV.DefaultCellStyle.SelectionForeColor = Color.Black;

			MyRequestsDGV.DefaultCellStyle.SelectionBackColor = colors.Hover;
			//MyRequestsDGV.DefaultCellStyle.SelectionForeColor = colors.Text;
			MyRequestsDGV.DefaultCellStyle.SelectionForeColor = Color.Black;

			FormsDGV.RowsDefaultCellStyle.SelectionBackColor = colors.Hover;
			//FormsDGV.RowsDefaultCellStyle.SelectionForeColor = colors.Text;
			FormsDGV.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

			MyRequestsDGV.RowsDefaultCellStyle.SelectionBackColor = colors.Hover;
			//MyRequestsDGV.RowsDefaultCellStyle.SelectionForeColor = colors.Text;
			MyRequestsDGV.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

			// split container
			MasterSplitContainer.BackColor = colors.Primary;

			MasterSplitContainer.Panel1.BackColor = colors.Background;
			MasterSplitContainer.Panel2.BackColor = colors.Background;

			// panels
			TopPanel1.BackColor = colors.Background;
			TopPanel2.BackColor = colors.Background;

			BottomPanel1.BackColor = colors.Background;
			BottomPanel2.BackColor = colors.Background;

			// backgrounds color
			this.BackColor = colors.Background;
			FormsDGV.BackgroundColor = colors.Background;
			MyRequestsDGV.BackgroundColor = colors.Background;
		}

		/// <summary>
		/// Quando sto per mostrare la form, comincio a caricare i dati da mostrare nelle due dgv
		/// </summary>
		/// <param name="e"></param>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			BBTrayletForm_SizeChanged(this, new EventArgs());

			TopComboBox.SelectedIndex = 0;
			BottomComboBox.SelectedIndex = 3;

			//Load_FormsDataGridView();			-> già presente nella index_changed della combo box dei filtri
			LoadAndFilter_MyRequestsDGV_Async(string.Empty);
		}

		/// <summary>
		/// Ogni volta che muovo la scroll bar, mi salvo l'ultima posizione e il fatto che l'ho toccata,
		/// in questo modo, quando aggiorno le DGV, posso riportarle alla posizione precedente, altrimenti di default le porto in fondo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormsDGV_Scroll(object sender, ScrollEventArgs e)
		{
			FormsDGVEditedByUser = true;
			FormsDGVLastScrollingIndex = e.NewValue;
		}

		/// <summary>
		/// Ogni volta che muovo la scroll bar, mi salvo l'ultima posizione e il fatto che l'ho toccata,
		/// in questo modo, quando aggiorno le DGV, posso riportarle alla posizione precedente, altrimenti di default le porto in fondo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MyRequestsDGV_Scroll(object sender, ScrollEventArgs e)
		{
			MyRequestsDGVEditedByUser = true;
			MyRequestsDGVLastScrollingIndex = e.NewValue;
		}

		/// <summary>
		/// Se è stata selezionata una riga, attivo il pulsante per mostrare il documento associato all'allegato
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MyRequestsDGV_SelectionChanged(object sender, EventArgs e)
		{
			int selectedRowCount =
				MyRequestsDGV.Rows.GetRowCount(DataGridViewElementStates.Selected);

			if(selectedRowCount == 1)
				ShowDocumentBtnBottom.Enabled = true;
			else
				ShowDocumentBtnBottom.Enabled = false;
		}

		/// <summary>
		/// Evento utilizzato per gestire l'attivazione/disattivazione dei pulsanti in base al numero di righe selezionate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormsDGV_SelectionChanged(object sender, EventArgs e)
		{
			int selectedRowCount =
				FormsDGV.Rows.GetRowCount(DataGridViewElementStates.Selected);
			//se nessuna riga è stata selezionata disattivo tutti i pulsanti funzione
			if(selectedRowCount < 1)
			{
				ShowDocumentBtnTop.Enabled = false;
				ShowFormBtn.Enabled = false;
				DelFormBtn.Enabled = false;
			}
			else
				//se una sola riga è stata selezionata attivo tutti i pulsanti funzione
				if(selectedRowCount == 1)
				{
					ShowDocumentBtnTop.Enabled = true;
					ShowFormBtn.Enabled = true;
					DelFormBtn.Enabled = true;
				}
				else
					//se più righe sono state selezionate, attivo solo il pulsante per la cancellazione delle formInstance
					if(selectedRowCount > 1)
					{
						ShowDocumentBtnTop.Enabled = false;
						ShowFormBtn.Enabled = false;
						DelFormBtn.Enabled = true;
					}
		}

		/// <summary>
		/// Metodo chiamato dai thread esterni che vogliono aggiornare l'interfaccia in seguito ad una notifica di brain.
		/// Attenzione alla concorrenza!!!
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bbNotificationModule_RefreshDGVS(object sender, EventArgs e)
		{
			int prevCount;
			int postCount;

			//acquisisco il lock
			lock(_lock)
			{
				//incremento il numero delle richieste di refresh
				RefreshRequestCount++;
				//salvo il valore (ovvero la posizione del thread corrente nella coda)
				prevCount = RefreshRequestCount;
			}
			//se non sono il primo a fare una richiesta, esco
			if(prevCount > 1)
				return;
			//altrimenti sono il primo e posso aggiornare l'interfaccia
			else
			{
				try
				{
					//aggiorno la dgv delle form
					RefreshTopDGV_Sync();
					//aggiorno la dgv delle richieste
					RefreshBottomDGV_Sync();
				}
				finally
				{
					//riacquisisco il lock
					lock(_lock)
					{
						//rileggo il valore della variabile per sapere se ci sono state richieste di aggiornamento nel frattempo
						postCount = RefreshRequestCount;
						//resetto il conteggio
						Interlocked.Exchange(ref RefreshRequestCount, 0);
					}
					//verifico per debug che prevCount sia 1
					Debug.Assert(prevCount == 1, "PrevCount is not 1");
					//se ci sono state richieste mentre aggiornavo(prevcount dovrebbe essere sempre 1)
					if(postCount > prevCount)
					{
						//riaggiorno
						bbNotificationModule_RefreshDGVS(sender, e);
					}
				}
			}
		}

		/// <summary>
		/// Questo netodo serve per catturare l'evento "click col tasto destro" su una riga della DGV form ricevute
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormsDGV_MouseClick(object sender, MouseEventArgs e)
		{
			FormsDGV_GotFocus(null, null);

			int currentMouseOverRow = FormsDGV.HitTest(e.X, e.Y).RowIndex;

			if(e.Button == MouseButtons.Right && currentMouseOverRow >= 0)
			{
				FormsDGV.Rows[currentMouseOverRow].Selected = true;

				ContextMenu menu = new ContextMenu();

				//mostro i bottoni di show document e form solo se ho selezionato una sola riga
				if(FormsDGV.SelectedRows.Count == 1)
				{
					//show document button
					var showDocBtn = new MenuItem(Strings.BBMenuVoiceShowDocument);
					showDocBtn.Click += (s, args) =>
					{
						ShowSelectedDocumentTop();
					};
					menu.MenuItems.Add(showDocBtn);

					//show document button
					var showFormBtn = new MenuItem(Strings.BBMenuVoiceShowForm);
					showFormBtn.Click += (s, args) =>
					{
						ShowSelectedForm();
					};
					menu.MenuItems.Add(showFormBtn);
				}
				//altrimenti è possibile comunque cancellare più form contemporaneamente

				//delete formInstance button
				var deleteFormInstanceBtn = new MenuItem(Strings.BBMenuVoiceDeleteForm);
				deleteFormInstanceBtn.Click += (s, args) =>
				{
					DeleteSelectedFormInstance();
				};
				menu.MenuItems.Add(deleteFormInstanceBtn);

				menu.Show(FormsDGV, new Point(e.X, e.Y));
			}
		}

		/// <summary>
		/// Questo metodo serve per catturare l'evento "click col tasto destro" su una riga della DGV delle richieste
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MyRequestsDGV_MouseClick(object sender, MouseEventArgs e)
		{
			MyRequestsDGV_GotFocus(null, null);

			int currentMouseOverRow = MyRequestsDGV.HitTest(e.X, e.Y).RowIndex;

			if(e.Button == MouseButtons.Right && currentMouseOverRow >= 0)
			{
				MyRequestsDGV.Rows[currentMouseOverRow].Selected = true;

				ContextMenu menu = new ContextMenu();

				//show document button
				var showDocBtn = new MenuItem(Strings.BBMenuVoiceShowDocument);
				showDocBtn.Click += (s, args) =>
				{
					ShowSelectedDocumentBottom();
				};
				menu.MenuItems.Add(showDocBtn);

				//show document button
				var selectedWfAttachmentInfo = MyRequestsDGV.SelectedRows[0].DataBoundItem as WFAttachmentInfo;
				//se il documento è ancora da approvare posso stressare l'approvatore
				if(selectedWfAttachmentInfo.Status == 0)
				{
					var stressApproverBtn = new MenuItem(Strings.BBStressApprover);
					stressApproverBtn.Click += (s, args) =>
					{
						if(selectedWfAttachmentInfo != null)
							notificationManager.SendIGenericNotify(new GenericNotify
							{
								Description = string.Format(Strings.BBStressApproverMsgNotify, dmsOrchestrator.GetWorkerName(notificationManager.WorkerId), selectedWfAttachmentInfo.Description),
								ToWorkerId = selectedWfAttachmentInfo.ApproverId,
								ToCompanyId = notificationManager.CompanyId,
								Title = Strings.BBRequestForApproval,
								FromCompanyId = notificationManager.CompanyId,
								FromWorkerId = notificationManager.WorkerId
							}, true);
						/*notificationManager.SendMessage(
							string.Format(Strings.BBStressApproverMsgNotify, dmsOrchestrator.GetWorkerName(notificationManager.WorkerId), selectedWfAttachmentInfo.Description),
							selectedWfAttachmentInfo.ApproverId, notificationManager.CompanyId);*/
					};
					menu.MenuItems.Add(stressApproverBtn);
				}

				menu.Show(MyRequestsDGV, new Point(e.X, e.Y));
			}
		}

		/// <summary>
		/// Quando metto il focus sulla tabella delle form,
		/// cancello la selezione dalla tabelle delle richieste inviate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormsDGV_GotFocus(object sender, EventArgs e)
		{
			MyRequestsDGV.ClearSelection();
		}

		/// <summary>
		/// Quando metto il focus sulla tabella delle richieste di approvazione,
		/// cancello la selezione dalla tabella delle form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MyRequestsDGV_GotFocus(object sender, EventArgs e)
		{
			FormsDGV.ClearSelection();
		}

		/// <summary>
		/// Inizializza alcuni controlli, da richiamare una volta sola all'interno del costruttore
		/// </summary>
		private void InitControls()
		{
			NotificationManagerUtility.SetFlatStyleFlat(this);

			//setting theme colors for controls
			SetDefaultThemeColors();

			FormsDGV.CurrentCell = null;
			MyRequestsDGV.CurrentCell = null;

			// aggiungo la colonna immagine nella DGV delle richieste inviate
			DataGridViewImageColumn ic = new DataGridViewImageColumn();
			ic.HeaderText = Strings.BBColumnStatus;
			ic.Image = null;
			ic.Name = "cImg";
			ic.Width = 100;
			MyRequestsDGV.Columns.Add(ic);
			MyRequestsDGV.Columns[0].Visible = false;

			TopComboBox.Items.AddRange(new string[]
										{
											/*0*/Strings.BBComboBoxUnprocessed,
											/*1*/Strings.BBComboBoxProcessed,
											/*2*/Strings.BBComboBoxAll
										});

			BottomComboBox.Items.AddRange(new string[]
										{
											/*0*/Strings.BBComboBoxWaitingForApprovation,
											/*1*/Strings.BBComboBoxApproved,
											/*2*/Strings.BBComboBoxRejected,
											/*3*/Strings.BBComboBoxAll
										});

			// questa chiamata scatena anche TopComboBox_SelectedIndexChanged
			// quindi di fatto implica una LoadForms
			//BottomComboBox.SelectedIndex = 0;
			//try
			//{
			//	ITheme theme = DefaultTheme.GetTheme();
			//	FormsDGV.DefaultCellStyle.SelectionBackColor = theme.GetThemeElementColor("ControlsHighlightBkgColor");
			//	MyRequestsDGV.DefaultCellStyle.SelectionBackColor = theme.GetThemeElementColor("ControlsHighlightBkgColor");
			//}
			//catch { }

			//DelFormBtn.Text = Strings.Delete;
			//ShowFormBtn.Text = Strings.BBMenuVoiceShowForm;
			//ShowDocumentBtnTop.Text = Strings.BBMenuVoiceShowDocument;
			//RefreshBtnTop.Text = Strings.BBRefresh;

			//ShowDocumentBtnBottom.Text = Strings.BBMenuVoiceShowDocument;
			//RefreshBtnBottom.Text = Strings.BBRefresh;

			//Toplbl.Text = Strings.BBReceivedForms;
			//BottomLbl.Text = Strings.BBSentRequests;
			//TopFilterLbl.Text = Strings.BBFilterLbl;
			//BottomFilterLbl.Text = Strings.BBFilterLbl;

		}

		/// <summary>
		/// Metodo chiamato ad ogni cambio di dimensione della form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BBTrayletForm_SizeChanged(object sender, EventArgs e)
		{
			//todoandrea: possibile sostituire i calcoli seguenti inserendo la posizione centrale del pannello superiore
			PointTop.X = (BottomPanel1.Width - LoadingBoxTop.Width) / 2;
			PointTop.Y = (BottomPanel1.Height - LoadingBoxTop.Height) / 2;
			LoadingBoxTop.Location = PointTop;

			//todoandrea: possibile sostituire i calcoli seguenti inserendo la posizione centrale del pannello inferiore
			PointBottom.X = (BottomPanel2.Width - LoadingBoxBottom.Width) / 2;
			PointBottom.Y = (BottomPanel2.Height - LoadingBoxBottom.Height) / 2;
			LoadingBoxBottom.Location = PointBottom;
		}

		/// <summary>
		/// metodo richiamato per mostrare l'immagine di caricamento,
		/// mentre si carica la tabella delle form, si occupa di far
		/// diventare trasparente la DGV piuttosto che il contrario
		/// </summary>
		/// <param name="isLoading"></param>
		private void IsLoadingTop(bool isLoading)
		{
			if(ControlInvokeRequired(this, () => IsLoadingTop(isLoading)))
				return;
			RefreshBtnTop.Enabled = !isLoading;
			//LoadingBoxTop.Location = PointTop;
			FormsDGV.Visible = !isLoading;
			LoadingBoxTop.Visible = isLoading;
			FormsDGV.ClearSelection();
		}

		/// <summary>
		/// metodo richiamato per mostrare l'immagine di caricamento,
		/// mentre si carica la tabella delle richieste di approvazione,
		/// si occupa di far diventare trasparente la DGV piuttosto che il contrario
		/// </summary>
		/// <param name="isLoading"></param>
		private void IsLoadingBottom(bool isLoading)
		{
			if(ControlInvokeRequired(this, () => IsLoadingBottom(isLoading)))
				return;
			RefreshBtnBottom.Enabled = !isLoading;
			//LoadingBoxBottom.Location = PointBottom;
			MyRequestsDGV.Visible = !isLoading;
			LoadingBoxBottom.Visible = isLoading;
			MyRequestsDGV.ClearSelection();
		}

		/// <summary>
		/// Evento generato quando cambia il valore della comboBox.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TopComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			//controllo se è cambiato
			if(TopComboBox.SelectedIndex == LastSelectedIndex)
				return;

			if(!ProcessedFormIncludedInDGV)
			{
				LoadAndFilter_FormsDGV_Async(TopComboBox.SelectedIndex != 0, TopDGVFilterTxt.Text);
			}
			else
			{
				FormsDGVFilter(TopDGVFilterTxt.Text);
			}
			LastSelectedIndex = TopComboBox.SelectedIndex;
		}

		/// <summary>
		/// Richiede la lista delle (descrizioni delle) form a Brain Business e riempe la tabella
		/// </summary>
		private void LoadAndFilter_FormsDGV_Async(bool includeProcessed, string lowerFilter)
		{
			MyBBFormInstanceExtended[] allFormInfos = null;
			FormsDGV.Rows.Clear();
			IsLoadingTop(true);
			int selectedIndex = TopComboBox.SelectedIndex;
			int visibleRows = 0;

			A.Sailor()
			.When(() =>
			{
				allFormInfos = bbNotificationModule.BBGetAllFormInstancesExtended(includeProcessed);

				if(allFormInfos == null)
					throw new Exception();

				AddRowsToFormsDGV(allFormInfos);
				//Thread.Sleep(10000);
				if(includeProcessed)
					FormsDGVFilter(lowerFilter);
				visibleRows = FormsDGV.Rows.GetRowCount(DataGridViewElementStates.Visible);
			})
			.OnError(exc =>
			{
				NotificationManagerUtility.SetErrorMessage(
					Strings.BBLoadingFormsDgvError,
					exc.Message, "BBTrayletForm.Load_FormsDataGridView()");//, DiagnosticType.Error);
			})
			.Finally(() =>
			{
				IsLoadingTop(false);

				FormsDGV.AutoResizeColumns();

				if(visibleRows >= FormsDGVLastScrollingIndex && FormsDGVEditedByUser == true)
					//mi riposiziono nella posizione precedente
					FormsDGV.FirstDisplayedScrollingRowIndex = FormsDGVLastScrollingIndex;
				else
					//sposto la scrollbar in fondo
					if(visibleRows > 0 && FormsDGVEditedByUser == false)
						FormsDGV.FirstDisplayedScrollingRowIndex = visibleRows - 1;

				ProcessedFormIncludedInDGV = includeProcessed;
			});
		}

		/// <summary>
		/// Richiede la lista delle (descrizioni delle) form a Brain Business e riempe la tabella
		/// </summary>
		private void LoadAndFilter_FormsDGV_Sync(bool includeProcessed, string lowerFilter)
		{
			MyBBFormInstanceExtended[] allFormInfos = null;
			FormsDGV.Rows.Clear();
			IsLoadingTop(true);
			int selectedIndex = TopComboBox.SelectedIndex;
			int visibleRows = 0;

			try
			{
				allFormInfos = bbNotificationModule.BBGetAllFormInstancesExtended(includeProcessed);

				if(allFormInfos == null)
					throw new Exception();

				AddRowsToFormsDGV(allFormInfos);
				if(includeProcessed)
					FormsDGVFilter(lowerFilter);
				visibleRows = FormsDGV.Rows.GetRowCount(DataGridViewElementStates.Visible);
			}
			catch(Exception exc)
			{
				NotificationManagerUtility.SetErrorMessage(
					Strings.BBLoadingFormsDgvError,
					exc.Message, "BBTrayletForm.Load_FormsDataGridView()");//, DiagnosticType.Error);
			}
			finally
			{
				IsLoadingTop(false);

				FormsDGV.AutoResizeColumns();

				if(visibleRows >= FormsDGVLastScrollingIndex && FormsDGVEditedByUser == true)
					//mi riposiziono nella posizione precedente
					FormsDGV.FirstDisplayedScrollingRowIndex = FormsDGVLastScrollingIndex;
				else
					//sposto la scrollbar in fondo
					if(visibleRows > 0 && FormsDGVEditedByUser == false)
						FormsDGV.FirstDisplayedScrollingRowIndex = visibleRows - 1;

				ProcessedFormIncludedInDGV = includeProcessed;
			}
		}

		/// <summary>
		/// Metodo per inserire le descrizioni delle form ricevute da brain nella tabella di sopra
		/// </summary>
		/// <param name="allFormInfos"></param>
		private void AddRowsToFormsDGV(MyBBFormInstanceExtended[] allFormInfos)
		{
			if(ControlInvokeRequired(this, () => AddRowsToFormsDGV(allFormInfos)))
				return;

			foreach(var formInfos in allFormInfos ?? new MyBBFormInstanceExtended[0])
			{
				FormsDGV.Rows.Add(
					formInfos.DateSubmitted.ToString(),
					formInfos.Title,
					formInfos.Description,
					formInfos.Requester,
					formInfos.DateProcessed == DateTime.MinValue ? Strings.BBApprovalStatus0Waiting : formInfos.DateProcessed.ToString(),
					formInfos.Processed,
					formInfos.FormInstanceId,
					formInfos.AttachmentId);
			}
		}

		/// <summary>
		/// Filtra le form in base alla selezione dell'utente
		/// </summary>
		private void FormsDGVFilter(string lowerFilter)
		{
			if(ControlInvokeRequired(this, () => FormsDGVFilter(lowerFilter)))
				return;
			string filter = lowerFilter.ToUpper();
			try
			{
				foreach(DataGridViewRow row in FormsDGV.Rows)
				{
					switch(TopComboBox.SelectedIndex)
					{
						case 0:
							//Da processare
							row.Visible = !(bool)(row.Cells[ProcessedColumn.Index].Value);
							break;

						case 1:
							//Prcessate
							row.Visible = (bool)(row.Cells[ProcessedColumn.Index].Value);
							break;

						default:
							//Tutte
							row.Visible = true;
							break;
					}
					if(row.Visible == true)
					{
						var cells = new List<string>();

						foreach(var cell in row.Cells)
						{
							if(cell is DataGridViewTextBoxCell)
							{
								var obj = ((DataGridViewTextBoxCell)cell).Value;
								if(obj == null)
									continue;
								cells.Add(obj.ToString() ?? string.Empty);
							}
						}

						if(filter.In(cells.ToArray()))
							row.Visible = true;
						else
							row.Visible = false;
					}
				}
				FormsDGV.AutoResizeColumns();
			}
			catch(Exception e)
			{
				NotificationManagerUtility.SetErrorMessage(
						Strings.BBFilteringFormsDgvError,
						e.Message, "\nBBTrayletForm.FormsDataGridViewFilter()");
				return;
			}
		}

		/// <summary>
		/// Verifico di star eseguendo il codice nel giusto thread
		/// Usage:
		/// private void A()
		/// {
		///		if(ControlInvokeRequired(this, () => A()))
		///			return;
		///		*codice che modifica la gui*
		///	}
		/// </summary>
		/// <param name="c">lo user control che voglio modificare</param>
		/// <param name="a">L'azione che devo andare ad eseguire</param>
		/// <returns>se è necessario fare la invoke
		/// </returns>
		public bool ControlInvokeRequired(Control c, Action a)
		{
			if(c.InvokeRequired)
				c.Invoke(new MethodInvoker(delegate { a(); }));
			else
				return false;
			return true;
		}

		/// <summary>
		/// ricarica la lista delle form mantenendo il filtro selezionato dall'utente
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RefreshBtnTop_Click(object sender, EventArgs e)
		{
			RefreshTopDGV_Async();
		}

		/// <summary>
		/// aggiorno la DGV delle form
		/// </summary>
		private void RefreshTopDGV_Async()
		{
			if(ControlInvokeRequired(this, () => RefreshTopDGV_Async()))
				return;

			//TopDGVFilterTxt.Text = string.Empty;
			LoadAndFilter_FormsDGV_Async(TopComboBox.SelectedIndex != 0, TopDGVFilterTxt.Text);
		}

		/// <summary>
		/// aggiorno la DGV delle form
		/// </summary>
		private void RefreshTopDGV_Sync()
		{
			if(ControlInvokeRequired(this, () => RefreshTopDGV_Sync()))
				return;

			//TopDGVFilterTxt.Text = string.Empty;
			LoadAndFilter_FormsDGV_Sync(TopComboBox.SelectedIndex != 0, TopDGVFilterTxt.Text);
		}

		/// <summary>
		/// Carico la tabella delle richieste di approvazione inviate.
		/// Ora uso linq e uso la query come dataSource della datagridview
		/// </summary>
		private void LoadAndFilter_MyRequestsDGV_Async(string filter)
		{
			IsLoadingBottom(true);
			IEnumerable<WFAttachmentInfo> requestCollection = null;

			//IEnumerable<WFAttachmentInfo> filteredCollection = null;

			MyRequestsDGV.Rows.Clear();
			int visibleRows = 0;

			A.Sailor()
			.When(() =>
			{
				var dc = dmsOrchestrator.DataContext;
				requestCollection = (from att in dc.DMS_Attachments
									 join wfAtt in dc.DMS_WFAttachments
									 on att.AttachmentID equals wfAtt.AttachmentID
									 where wfAtt.WorkerID == dmsOrchestrator.WorkerId
									 //todoandrea: decidere se filtrare sul db o in locale
									 //nota bene: sul db non conosco il workername?? <- forse se faccio un join con la tabella dei workers ci arrivo
									 //&& (
									 //   att.Description.Contains(filter) ||
									 //   dmsOrchestrator.GetWorkerName(wfAtt.ApproverID).Contains(filter) ||
									 //   wfAtt.RequestDate.ToString().Contains(filter) ||
									 //   (wfAtt.ApprovalDate.ToString() ?? "In Attesa").Contains(filter)
									 //)
									 orderby wfAtt.RequestDate
									 select new WFAttachmentInfo
									 {
										 Status = wfAtt.ApprovalStatus,
										 Description = att.Description,
										 RequestDateString = wfAtt.RequestDate.ToString(),
										 ApprovalDateString = (wfAtt.ApprovalDate != null ? wfAtt.ApprovalDate.Value.ToString() : Strings.BBApprovalStatus0Waiting),
										 //ApprovalDateString = wfAtt.ApprovalDate.ToString() ?? Strings.BBApprovalStatus0Waiting,
										 ApproverName = dmsOrchestrator.GetWorkerName(wfAtt.ApproverID),
										 AttachmentId = wfAtt.AttachmentID,
										 ApproverId = wfAtt.ApproverID ?? 0
									 }).ToList();

				//todoandrea: decidere se filtrare sul db o in locale
				//filteredCollection = requestCollection.Where(Att => Att.Contains(filter));

				//Thread.Sleep(10000);
			})
			.Then((obj) =>
			{
				//MyRequestsDataGridView.DataSource = filteredCollection;
				//bs = new BindingSource(filteredCollection, null);

				WFAttachmentInfoBindingList.Clear();

				foreach(var item in requestCollection)
					WFAttachmentInfoBindingList.Add(item);

				bs.DataSource = WFAttachmentInfoBindingList;

				MyRequestsDGV.DataSource = bs;

				//filtro in locale
				MyRequestDGVFilter(filter);

				visibleRows = MyRequestsDGV.Rows.GetRowCount(DataGridViewElementStates.Visible);
			})
			.OnError(exc =>
			{
				NotificationManagerUtility.SetErrorMessage(
					Strings.BBLoadingRequestsDgvError,
					exc.Message, "BBTrayletForm.Load_MyRequestsDataGridView()");//, DiagnosticType.Error);
			})
			.Finally(() =>
			{
				IsLoadingBottom(false);
				ReplaceStatusStringWithImageInMyRequestDGV();
				MyRequestsDGV.AutoResizeColumns();

				if(visibleRows >= MyRequestsDGVLastScrollingIndex && MyRequestsDGVEditedByUser == true)
					//mi riposiziono nella posizione precedente
					MyRequestsDGV.FirstDisplayedScrollingRowIndex = MyRequestsDGVLastScrollingIndex;
				else
					//sposto la scrollbar in fondo
					if(visibleRows > 0 && MyRequestsDGVEditedByUser == false)
						MyRequestsDGV.FirstDisplayedScrollingRowIndex = visibleRows - 1;
			});
		}

		/// <summary>
		/// Carico la tabella delle richieste di approvazione inviate.
		/// Ora uso linq e uso la query come dataSource della datagridview
		/// </summary>
		private void LoadAndFilter_MyRequestsDGV_Sync(string filter)
		{
			IsLoadingBottom(true);
			IEnumerable<WFAttachmentInfo> requestCollection = null;

			//IEnumerable<WFAttachmentInfo> filteredCollection = null;

			MyRequestsDGV.Rows.Clear();
			int visibleRows = 0;
			try
			{
				var dc = dmsOrchestrator.DataContext;
				requestCollection = (from att in dc.DMS_Attachments
									 join wfAtt in dc.DMS_WFAttachments
									 on att.AttachmentID equals wfAtt.AttachmentID
									 where wfAtt.WorkerID == dmsOrchestrator.WorkerId
									 orderby wfAtt.RequestDate
									 select new WFAttachmentInfo
									 {
										 Status = wfAtt.ApprovalStatus,
										 Description = att.Description,
										 RequestDateString = wfAtt.RequestDate.ToString(),
										 ApprovalDateString = wfAtt.ApprovalDate.ToString() ?? Strings.BBApprovalStatus0Waiting,
										 ApproverName = dmsOrchestrator.GetWorkerName(wfAtt.ApproverID),
										 AttachmentId = wfAtt.AttachmentID,
										 ApproverId = wfAtt.ApproverID ?? 0
									 }).ToList();
				WFAttachmentInfoBindingList.Clear();

				foreach(var item in requestCollection)
					WFAttachmentInfoBindingList.Add(item);

				bs.DataSource = WFAttachmentInfoBindingList;

				MyRequestsDGV.DataSource = bs;

				//filtro in locale
				MyRequestDGVFilter(filter);

				visibleRows = MyRequestsDGV.Rows.GetRowCount(DataGridViewElementStates.Visible);
			}
			catch(Exception exc)
			{
				NotificationManagerUtility.SetErrorMessage(
					Strings.BBLoadingRequestsDgvError,
					exc.Message, "BBTrayletForm.Load_MyRequestsDataGridView()");//, DiagnosticType.Error);
			}
			finally
			{
				IsLoadingBottom(false);
				ReplaceStatusStringWithImageInMyRequestDGV();
				MyRequestsDGV.AutoResizeColumns();

				if(visibleRows >= MyRequestsDGVLastScrollingIndex && MyRequestsDGVEditedByUser == true)
					//mi riposiziono nella posizione precedente
					MyRequestsDGV.FirstDisplayedScrollingRowIndex = MyRequestsDGVLastScrollingIndex;
				else
					//sposto la scrollbar in fondo
					if(visibleRows > 0 && MyRequestsDGVEditedByUser == false)
						MyRequestsDGV.FirstDisplayedScrollingRowIndex = visibleRows - 1;
			}
		}

		/// <summary>
		/// Metodo che sostituisce la colonna dello status della form delle richieste con un'immagine significativa
		/// </summary>
		private void ReplaceStatusStringWithImageInMyRequestDGV()
		{
			MyRequestsDGV.Columns[0].Visible = true;
			MyRequestsDGV.Columns[1].Visible = false;

			foreach(DataGridViewRow row in MyRequestsDGV.Rows)
			{
				DataGridViewImageCell cell = row.Cells[0] as DataGridViewImageCell;
				int status = (int)row.Cells[1].Value;
				switch(status)
				{
					case 0:
						cell.Value = (System.Drawing.Image)Properties.Resources.Yellow;
						break;

					case 1:
						cell.Value = (System.Drawing.Image)Properties.Resources.Green;
						break;

					case 2:
						cell.Value = (System.Drawing.Image)Properties.Resources.Red;
						break;

					default:
						cell.Value = (System.Drawing.Image)Properties.Resources.Remove16x16;
						break;
				}
			}
		}

		/// <summary>
		/// Cattura la pressione del tasto f12 se premuto quando la tabella in alto ha il focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormsDGV_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.F12)
			{
				e.Handled = true;
				ShowSelectedForm();
			}
		}

		/// <summary>
		/// Cattura l'evento del doppio click su una riga
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormsDGV_DoubleClick(object sender, EventArgs e)
		{
			ShowSelectedForm();
		}

		/// <summary>
		///  Cattura la pressione del tasto f12 se premuto quando la tabella in basso ha il focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MyRequestsDGV_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.F12)
			{
				e.Handled = true;
				ShowSelectedDocumentBottom();
			}
		}

		/// <summary>
		/// Cattura l'evento del doppio click su una riga
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MyRequestsDGV_DoubleClick(object sender, EventArgs e)
		{
			ShowSelectedDocumentBottom();
		}

		/// <summary>
		/// Evento generato alla pressione del tasto di visualizzazione delle form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowFormBtn_Click(object sender, EventArgs e)
		{
			ShowSelectedForm();
		}

		/// <summary>
		/// Chiama il metodo del notification Manager per visualizzare la form, a partire
		/// dalla corrispondente riga della tabella selezionata
		/// </summary>
		private void ShowSelectedForm()
		{
			if(FormsDGV.SelectedRows.Count != 1)
				return;
			try
			{
				var formInstanceId = (int)FormsDGV.SelectedRows[0].Cells[FormInstanceIdColumn.Index].Value;
				bbNotificationModule.BBShowSpecificForm(formInstanceId);
			}
			catch(Exception e)
			{
				NotificationManagerUtility.SetErrorMessage(
						Strings.BBSelectingFormDgvError,
						e.Message, "BBTrayletForm.ShowSelectedForm()");
				return;
			}
		}

		/// <summary>
		/// Chiama il metodo del EaSynchWrapper per cancellare una FormInstance
		/// </summary>
		private void DeleteSelectedFormInstance()
		{
			if(FormsDGV.SelectedRows.Count < 1)
				return;
			try
			{
				var FilteredSelectedRows = new List<DataGridViewRow>();

				//seleziono solo le righe visibili in modo che la selezione dell'utente corrisponda a quella programmativa
				foreach(DataGridViewRow row in FormsDGV.SelectedRows)
					if(row.Visible)
						FilteredSelectedRows.Add(row);

				string messageBoxText = string.Format(Strings.BBDeleteFormConfirm, FilteredSelectedRows.Count);
				string caption = Strings.BBDeleteFormCaption;
				MessageBoxButtons button = MessageBoxButtons.YesNo;
				MessageBoxIcon icon = MessageBoxIcon.Warning;
				DialogResult result = MessageBox.Show(messageBoxText, caption, button, icon);

				int notProcessedCount = 0;
				if(result == DialogResult.Yes)
				{
					foreach(DataGridViewRow row in FilteredSelectedRows)
					{
						var formInstanceId = (int)row.Cells[FormInstanceIdColumn.Index].Value;
						var processed = (bool)row.Cells[ProcessedColumn.Index].Value;
						if(processed)
							dmsOrchestrator.EASync.DeleteFormInstance(notificationManager.CompanyId, notificationManager.WorkerId, formInstanceId);
						else
							notProcessedCount++;
					}
				}
				if(notProcessedCount > 0)
					NotificationManagerUtility.SetErrorMessage(
						string.Format(Strings.BBDeleteFormError, notProcessedCount),
						string.Empty, "BBTrayletForm.DeleteSelectedFormInstance()");
				return;
			}
			catch(Exception e)
			{
				NotificationManagerUtility.SetErrorMessage(
						Strings.BBSelectingFormDgvError,
						e.Message, "BBTrayletForm.DeleteSelectedFormInstance()");
				return;
			}
		}

		/// <summary>
		/// Chiama il metodo del notification Manager per visualizzare la form, a partire
		/// dalla corrispondente riga della tabella selezionata
		/// </summary>
		private void ShowSelectedDocumentTop()
		{
			if(FormsDGV.SelectedRows.Count != 1)
				return;
			var attachmentId = (int)FormsDGV.SelectedRows[0].Cells[AttachmentIdColumn.Index].Value;
			OpenErpDocument(dmsOrchestrator, attachmentId);
		}

		/// <summary>
		/// evento generato alla pressione del pulsante di visualizzazione documento
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowDocumentBtnTop_Click(object sender, EventArgs e)
		{
			ShowSelectedDocumentTop();
		}

		/// <summary>
		/// Evento generato alla pressione del pulsante di refresh in basso
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RefreshBtnBottom_Click(object sender, EventArgs e)
		{
			RefreshBottomDGV_Async();
		}

		/// <summary>
		/// Aggiorna la DGV delle richieste
		/// </summary>
		private void RefreshBottomDGV_Async()
		{
			if(ControlInvokeRequired(this, () => RefreshBottomDGV_Async()))
				return;
			LoadAndFilter_MyRequestsDGV_Async(BottomDGVFilterTxt.Text);
		}

		/// <summary>
		/// Aggiorna la DGV delle richieste
		/// </summary>
		private void RefreshBottomDGV_Sync()
		{
			if(ControlInvokeRequired(this, () => RefreshBottomDGV_Sync()))
				return;
			LoadAndFilter_MyRequestsDGV_Sync(BottomDGVFilterTxt.Text);
		}

		/// <summary>
		/// Chiama il metodo per visualizzare la form, a partire
		/// dalla corrispondente riga della tabella selezionata
		/// </summary>
		private void ShowSelectedDocumentBottom()
		{
			if(MyRequestsDGV.Rows.GetRowCount(DataGridViewElementStates.Selected) != 1)
				return;

			var selectedWfAttachmentInfo = MyRequestsDGV.SelectedRows[0].DataBoundItem as WFAttachmentInfo;
			if(selectedWfAttachmentInfo != null)
				OpenErpDocument(dmsOrchestrator, selectedWfAttachmentInfo.AttachmentId);

			//il valore sta nella colonna wfatt in posizione 6
			//var attachmentId = (int)MyRequestsDGV.SelectedRows[0].Cells[6].Value;
			//OpenErpDocument(dmsOrchestrator, attachmentId);
		}

		/// <summary>
		/// Evento generato alla pressione del pulsante di visualizzazione documento
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowDocumentBtnBottom_Click(object sender, EventArgs e)
		{
			ShowSelectedDocumentBottom();
		}

		/// <summary>
		/// Evento generato ogni volta che l'utente modifica il valore di test nella textBox dei filtri della DGV delle richieste
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BottomDGVFilterTxt_TextChanged(object sender, EventArgs e)
		{
			MyRequestDGVFilter(BottomDGVFilterTxt.Text);
		}

		/// <summary>
		/// Metodo utilizzato per filtrare la DGV delle richieste in base a:
		/// - valore della combobox
		/// - stringa passata come parametro -> utilizzare direttamente il valore della textBox dei filtri? todoandrea
		/// </summary>
		/// <param name="filter">stringa di testo utilizzata per filtrare</param>
		private void MyRequestDGVFilter(string filter)
		{
			IsLoadingBottom(true);

			try
			{
				var filtered = new BindingList<WFAttachmentInfo>
					(WFAttachmentInfoBindingList.Where(obj =>
						(obj.Status == BottomComboBox.SelectedIndex ||
						BottomComboBox.SelectedIndex == 3) &&
						obj.Contains(filter)
						)//.OrderBy(x=>x.RequestDateString)
						.ToList());

				MyRequestsDGV.DataSource = filtered;
				MyRequestsDGV.Update();

				ReplaceStatusStringWithImageInMyRequestDGV();
			}
			catch(Exception exc)
			{
				NotificationManagerUtility.SetErrorMessage(
							Strings.BBFilteringRequestsDgvError,
							exc.Message, "BBTrayletForm.BottomDGVFilterTxt_TextChanged()");//, DiagnosticType.Error);
			}
			finally
			{
				IsLoadingBottom(false);
			}
		}

		/// <summary>
		/// Evento generato ogni volta che l'utente digita qualcosa nella textbox dei filtri per la DGV delle form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TopDGVFilterTxt_TextChanged(object sender, EventArgs e)
		{
			FormsDGVFilter(TopDGVFilterTxt.Text);
		}

		/// <summary>
		/// Evento generato quando l'utente sceglie una voce dalla combo box relativa alla DGV delle richieste
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BottomComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			MyRequestDGVFilter(BottomDGVFilterTxt.Text);
		}

		/// <summary>
		/// Utilizzato nel bbTraylet e in un futuro anche nella notificationToolbar
		/// </summary>
		/// <param name="attachmentId"></param>
		public void OpenErpDocument(DMSOrchestrator dmsOrchestrator, int attachmentId)
		{
			if(dmsOrchestrator != null)
				try
				{
					AttachmentInfo att = dmsOrchestrator.SearchManager.GetAttachmentInfoFromAttachmentId(attachmentId);
					Utils.OpenERPDocument(att.ERPDocNamespace, att.ERPPrimaryKeyValue);
				}
				catch(Exception e)
				{
					NotificationManagerUtility.SetErrorMessage(Strings.BBOpenDocumentError, e.StackTrace, Strings.BBOpenDocumentErrorCaption);
				}
		}

		/// <summary>
		/// Pressione sul tasto di cancellazione form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DelFormBtn_Click(object sender, EventArgs e)
		{
			DeleteSelectedFormInstance();
		}
	}

	/// <summary>
	/// classe definita solo per esplicitare il nome delle colonne nella tabella delle richieste di approvazione
	/// ---> si può anche specificare in seguito al bind con il dataSource ma così è più pulito ed ordinato
	/// -------> però la classe anonima inline era ancora più pulita...
	/// ------------>todoandrea: vedere cosa fare
	/// </summary>
	public class WFAttachmentInfo
	{
		[DisplayNameLocalized(typeof(Strings), "BBColumnStatus")]
		public int Status { get; set; }

		[DisplayNameLocalized(typeof(Strings), "BBColumnDescription")]
		public string Description { get; set; }

		[DisplayNameLocalized(typeof(Strings), "BBColumnRequestDate")]
		public string RequestDateString { get; set; }

		[DisplayNameLocalized(typeof(Strings), "BBColumnApprovalDate")]
		public string ApprovalDateString { get; set; }

		[DisplayNameLocalized(typeof(Strings), "BBColumnApproverName")]
		public string ApproverName { get; set; }

		//hidden attributes
		[System.ComponentModel.Browsable(false)]
		public int AttachmentId { get; set; }

		[System.ComponentModel.Browsable(false)]
		public int ApproverId { get; set; }

		public bool Contains(string lowerFilter)
		{
			return lowerFilter.In(Description.ToUpper(),
									RequestDateString.ToUpper(),
									ApprovalDateString.ToUpper(),
									ApproverName.ToUpper());
		}
	}
}