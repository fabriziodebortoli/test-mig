using Microarea.TaskBuilderNet.Core.NotificationService;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Microarea.TaskBuilderNet.Core.NotificationManager
{
	/// <summary>
	/// Notifica di BrainBusiness
	/// Contiene solo le informazioni da visualizzare nel Viewer e il metodo da associare al click
	/// </summary>
	[Serializable]
	[KnownType(typeof(NotificationType))]
	public class BBNotify : IGenericNotify, ISerializable
	{
		//[NonSerialized]
		//private MyBBFormInstanceExtended ExtendedFormInstance;
		[NonSerialized]
		private BBNotificationModule BBNotificationModule;

		public int FormInstanceId { get; set; }

		public int ToCompanyId { get; set; }

		public int ToWorkerId { get; set; }

		public string FromUserName { get { return "Brain Business"; } }

		public BBNotify(int formInstanceId, BBNotificationModule bbNotificationModule)
		{
			FormInstanceId = formInstanceId;
			BBNotificationModule = bbNotificationModule;
		}

		/// <summary>
		/// in futuro verrà visualizzato in Bold e a caratteri più grossi
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// in futuro verrà visualizzato in italico e a caratteri più piccoli
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// il tipo della Notifica, utile?? -> da vedere
		/// </summary>
		public NotificationType NotificationType
		{
			get { return NotificationType.BrainBusiness; }
		}

		/// <summary>
		/// data in cui è stata emessa la notifica (non ricevuta, ma i due concetti vengono
		/// equiparati per comodità)
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// data di lettura notifica
		/// </summary>
		public DateTime ReadDate { get; set; }

		/// <summary>
		/// azione da eseguire al click sulla notifica del viewer
		/// </summary>
		public void OnClickAction()
		{
			if(BBNotificationModule != null)
				BBNotificationModule.BBShowSpecificForm(FormInstanceId, false);
		}

		public BBNotify()
		{
		}

		public BBNotify(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException("info", "value cannot be null");

			ToCompanyId		= (int)info.GetValue("CompanyId", typeof(int));
			ToWorkerId		= (int)info.GetValue("WorkerId", typeof(int));
			Title			= (string)info.GetValue("Title", typeof(string));
			Description		= (string)info.GetValue("Description", typeof(string));
			Date			= (DateTime)info.GetValue("Date", typeof(DateTime));
			FormInstanceId	= (int)info.GetValue("FormInstanceId", typeof(int));
		}

		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException("info", "value cannot be null");

			info.AddValue("CompanyId", ToCompanyId);
			info.AddValue("WorkerId", ToWorkerId);
			info.AddValue("Title", Title);
			info.AddValue("Description", Description);
			info.AddValue("NotificationType", NotificationType);
			info.AddValue("Date", Date);
			info.AddValue("FormInstanceId", FormInstanceId);
		}
	}

	/// <summary>
	/// Il modulo delle notifiche per BrainBusiness
	/// </summary>
	public class BBNotificationModule : BaseNotificationModule
	{
		public event EventHandler TestPerformedSuccessfullyEvent;

		public event EventHandler RefreshDGVS;

		public event EventHandler RefreshWFControlMaster;

		public override IList<IGenericNotify> GetNotifications()
		{
			var BBFormInstancesExtended = BBGetAllFormInstancesExtended(false);

			var BBNotifications = (BBFormInstancesExtended == null) ?
									new List<BBNotify>() :
									BBFormInstancesExtended.Select(form => new BBNotify(form.FormInstanceId, this)
										{
											Title = form.Title,
											ToCompanyId = NotificationServiceWrapper.CompanyId,
											ToWorkerId = NotificationServiceWrapper.WorkerId,
											Date = form.DateSubmitted,
											Description = form.Description,
										});

			return BBNotifications.ToList<IGenericNotify>();
		}

		public BBNotificationModule(NotificationServiceWrapper notificationServiceWrapper, bool isViewer)
			: base(notificationServiceWrapper, isViewer)
		{
			//mi registro agli eventi sparati dal ServiceWrapper (client xSocket)
			//notificationServiceWrapper.BBFormNotify += notificationServiceWrapper_BBFormNotify;
			//notificationServiceWrapper.BBMileStoneNotify += notificationServiceWrapper_BBMileStoneNotify;
		}

		/// <summary>
		/// Evento generato quando ricevo le mileStone:
		/// faccio apparire una form con il contenuto della milestone per 10 secondi, scattati i quali, si chiude
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void notificationServiceWrapper_BBMileStoneNotify(object sender, MileStoneNotifyEventArgs e)
		{
			//processo di test in BBSettings
			if(e.Title == "\"test\"")
			{
				TestPerformedSuccessfullyEvent.Raise(this, new EventArgs());
			}
			////se sono il viewer, mostro il messaggio all'utente
			else if(this.isViewer)
			{
				ShowMessage(e.Title);
			}
			//aggiorno il pannello di ea e le tabelle nel traylet
			else
			{
				RefreshWFControlMaster.Raise(this, EventArgs.Empty);
				RefreshDGVS.RaiseOnDifferentThread(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Evento generato quando ricevo una form:
		/// faccio apparire una form con il contenuto della form ricevuta (sotto forma di xml dal ws)
		/// posso compilarla e rispedirla indietro, oppure chiuderla e visualizzarla in seguito
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void notificationServiceWrapper_BBFormNotify(object sender, FormNotifyEventArgs e)
		{
			//innanzi tutto sparo la notifica generica del modulo base al notification manager
			RaiseBaseModuleEventHandler();
			//se sono il viewer, mostro la form all'utente
			if(this.isViewer)
			{
				BBShowSpecificForm(e.FormInstanceId, true);
			}
			//altrimenti aggiorno il pannello di ea e le tabelle nel traylet
			else
			{
				RefreshWFControlMaster.Raise(this, EventArgs.Empty);
				RefreshDGVS.RaiseOnDifferentThread(this, EventArgs.Empty);
			}
		}

		#region NotifcationManagerMethods

		//----------------------presi dal NotificationManager------------------------

		/// <summary>
		/// Recupera tutte le form non processate dall'utente
		/// </summary>
		/// <returns>un array di informazioni base sulle form ancora da processare</returns>
		public MyBBFormInstanceBase[] BBGetAllFormInstancesBase(bool includeProcessed)
		{
			return NotificationServiceWrapper.GetAllFormInstances(includeProcessed);
		}

		/// <summary>
		/// Recupera tutte le form non processate dall'utente
		/// </summary>
		/// <returns>un array di informazioni estese(+ attachmentId e requester) sulle form ancora da processare</returns>
		public MyBBFormInstanceExtended[] BBGetAllFormInstancesExtended(bool includeProcessed)
		{
			var formInstancesBase = BBGetAllFormInstancesBase(includeProcessed);
			return formInstancesBase == null ? null : formInstancesBase.Select(baseForm => BBMyBBFormInstanceExtendedFromBase(baseForm)).ToArray();
		}

		/// <summary>
		/// Mostra la specifica form identificata dal formInstanceId
		/// </summary>
		/// <param name="formInstanceId"></param>
		public void BBShowSpecificForm(int formInstanceId, bool AutoClose = false)
		{
			var myBBForm = NotificationServiceWrapper.GetForm(formInstanceId);
			if(myBBForm == null)
			{
				NotificationManagerUtility.SetErrorMessage(NotificationManagerStrings.ImpossibleRetrieveForm, string.Empty, NotificationManagerStrings.Error);
				return;
			}
			BBShowForm(myBBForm, AutoClose);
		}

		/// <summary>
		/// Metodo per mostrare la form
		/// </summary>
		/// <param name="xmlSchema">lo schema xml della form ritornato dal notification Service</param>
		/// <param name="formInstanceId">l'id univoco della form</param>
		/// <param name="AutoClose">booleano che viene utilizzato per discernere se fare la.Show() o la
		/// .ShowDialog() della form, dipendemente se viene eseguito dal thread del client o da uno
		/// generato in riposta della notifica del web Service</param>
		//---------------------------------------------------------------------
		internal void BBShowForm(MyBBFormSchema myBBFormSchema, bool AutoClose)
		{
			#region old style

			//var form = BBGetForm(myBBFormSchema, AutoClose);
			//if(AutoClose)
			//	form.ShowDialog();
			//else
			//	form.Show();

			#endregion old style

			Form activeForm = Form.ActiveForm;

			if(activeForm != null && activeForm.InvokeRequired)
			{
				activeForm.Invoke(new Action(() => BBShowForm(myBBFormSchema, AutoClose)));
				return;
			}

			var form = BBGetForm(myBBFormSchema, AutoClose);

			//se sono su mago e sto lavorando, mostro la form e riporto il focus sulla active form di prima
			if(activeForm != null)
			{
				form.Show();
				activeForm.Focus();
			}
			//altrimenti faccio direttamente una show dialog
			else
				form.ShowDialog();
		}

		/// <summary>
		/// Costruisce la form di Brain Business a partire dalla sua descrizione
		/// </summary>
		/// <param name="myBBFormSchema"></param>
		/// <param name="AutoClose"></param>
		/// <returns></returns>
		public Form BBGetForm(MyBBFormSchema myBBFormSchema, bool AutoClose)
		{
			if(myBBFormSchema == null)
				NotificationManagerUtility.SetErrorMessage(NotificationManagerStrings.ErrorWithParams, string.Empty, NotificationManagerStrings.Error);

			var xmlSchema = myBBFormSchema.xmlSchema;
			var myBBFormInstance = myBBFormSchema.myBBFormInstance;

			//!!!todo, spostare la chiamata del metodo parse all'esterno???
			var fieldList = BBParseXmlSchema(xmlSchema);

			//--------------------------------------------------------content Container panel
			//content containerPanel properties
			var contentContainerPanel = new FlowLayoutPanel();
			contentContainerPanel.FlowDirection = FlowDirection.TopDown;
			contentContainerPanel.AutoSize = true;
			contentContainerPanel.AutoSizeMode = AutoSizeMode.GrowOnly;

			//--------------------------------------------------------complete form
			//create the form
			var form = CreateCompleteNotificationForm(contentContainerPanel, myBBFormInstance.Title, AutoClose ? 10 : 0);

			//--------------------------------------------------------error provider
			//add errorProvider to validate mandatory textbox
			var errorProvider = new ErrorProvider(form);

			//--------------------------------------------------------content panel

			//--con flow Layout
			//contentPanel properties
			//var formPanel = new FlowLayoutPanel();
			//formPanel.FlowDirection = FlowDirection.TopDown;
			//formPanel.AutoSize = true;
			//formPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			//formPanel.Font = contentPanel.Font;

			////adding controls
			//BBAddSchemaControlsInPanel(ref fieldList, ref formPanel, errorProvider);
			//if(myBBFormInstance.Processed || myBBFormInstance.IsNotificationOnly)
			//	NotificationManagerUtility.LockControlValues(formPanel);

			//--con table layout
			var contentPanel = new TableLayoutPanel();
			contentPanel.Font = contentContainerPanel.Font;
			contentPanel.ColumnCount = 1;

			contentPanel.AutoSize = true;
			contentPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			contentPanel.AutoScroll = true;

			var screenTest = Screen.FromPoint(contentPanel.Location);
			contentPanel.MaximumSize = new Size(450, screenTest.WorkingArea.Height - 100);

			BBAddSchemaControlsInPanel(ref fieldList, ref contentPanel, errorProvider);
			if(myBBFormInstance.Processed || myBBFormInstance.IsNotificationOnly)
				NotificationManagerUtility.LockControlValues(contentPanel);

			//se la dimensione
			if(contentPanel.PreferredSize.Height >= /*screenTest.WorkingArea.Height - 100*/ contentPanel.MaximumSize.Height)
			{
				int vsbw = SystemInformation.VerticalScrollBarWidth;
				contentPanel.Padding = new Padding(0, 0, vsbw, 0);
			}
			//--------------------------------------------------------content buttons panel
			//content buttons panel properties
			var contentButtonsPanel = new FlowLayoutPanel();
			contentButtonsPanel.FlowDirection = FlowDirection.RightToLeft;
			contentButtonsPanel.AutoSize = true;
			contentButtonsPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			contentButtonsPanel.Dock = DockStyle.Bottom;

			//send button
			if(!myBBFormInstance.Processed)
			{
				//create sendFormButton
				Button SendFormBtn = new Button();
				SendFormBtn.Text = myBBFormInstance.IsNotificationOnly ?
									NotificationManagerStrings.MarkAsRead :
									NotificationManagerStrings.SendApproval;
				using(Graphics cg = form.CreateGraphics())
				{
					SizeF size = cg.MeasureString(SendFormBtn.Text, SendFormBtn.Font);
					SendFormBtn.Width = (int)size.Width + 20;
				}
				SendFormBtn.Click += (sender, args) =>
				{
					//controllo che tutti i campi obbligatori siano stati compilati
					if(!NotificationManagerUtility.AllControlsValidated(form, contentPanel.Controls, errorProvider))
						return;
					var returnedschema = BBUpdateSchemaWithUserChanges(xmlSchema, fieldList);
					bool sendWithSuccess = NotificationServiceWrapper.SetFormSchema(new TaskBuilderNet.Core.NotificationService.MyBBFormSchema
											{
												xmlSchema = returnedschema,
												myBBFormInstance = myBBFormInstance
											});
					if(sendWithSuccess)
					{
						form.Close();
						RefreshDGVS.RaiseOnDifferentThread(this, EventArgs.Empty);
						RaiseBaseModuleEventHandler();
					}
					//todo: else mostrare messaggio di errore
				};
				contentButtonsPanel.Controls.Add(SendFormBtn);
				form.AcceptButton = SendFormBtn;
			}

			//Close button
			Button CloseBtn = new Button();
			CloseBtn.Text = NotificationManagerStrings.Close;
			CloseBtn.Click += (sender, args) =>
			{
				form.Close();
			};
			//adding the button
			contentButtonsPanel.Controls.Add(CloseBtn);
			form.CancelButton = CloseBtn;

			//adding the content and button panels in the container
			contentContainerPanel.Controls.Add(contentPanel);
			contentContainerPanel.Controls.Add(contentButtonsPanel);

			return form;
		}

		#region oldBBgetFormMethod

		///// <summary>
		///// Costruisce la form di Brain Business a partire dalla sua descrizione
		///// </summary>
		///// <param name="myBBFormSchema"></param>
		///// <param name="AutoClose"></param>
		///// <returns></returns>
		//public Form BBGetForm(MyBBFormSchema myBBFormSchema, bool AutoClose)
		//{
		//	if(myBBFormSchema == null)
		//		NotificationManagerUtility.SetMessage("Errore nei parametri ricevuti", string.Empty, "Errore!");

		//	var xmlSchema = myBBFormSchema.xmlSchema;
		//	var myBBFormInstance = myBBFormSchema.myBBFormInstance;

		//	Timer timer = null;

		//	//!!!todo, spostare la chiamata del metodo parse all'esterno???
		//	var fieldList = BBParseXmlSchema(xmlSchema);

		//	//	questa è la struttura di una form brain business:
		//	//	form					-> form
		//	//		master panel		-> flow layout panel
		//	//			draggable panel -> draggable panel
		//	//			form panel		-> flow layout panel
		//	//				...			-> i campi della form
		//	//			timer panel		-> flow layout panel
		//	//			button panel	-> flow layout panel
		//	//				...			-> accept e cancel buttons

		//	//--------------------------------------------------------form
		//	//form properties
		//	var form = new Form();
		//	form.AutoSize = true;
		//	form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		//	form.Text = myBBFormInstance.Title;
		//	form.TopMost = true;
		//	form.BackColor = Color.FromArgb(43, 120, 228);//blu come il menu di mago
		//	form.Padding = new Padding(2);
		//	form.Font = new Font("Segoe UI", 9);

		//	//--------------------------------------------------------error provider
		//	//add errorProvider to validate mandatory textbox
		//	var errorProvider = new ErrorProvider(form);

		//	//--------------------------------------------------------master panel
		//	//master panel properties
		//	var masterPanel = new FlowLayoutPanel();
		//	masterPanel.Margin = Padding.Empty;
		//	masterPanel.FlowDirection = FlowDirection.TopDown;
		//	masterPanel.AutoSize = true;
		//	masterPanel.Width = 400;
		//	masterPanel.BackColor = Color.White;
		//	masterPanel.Dock = DockStyle.Fill;

		//	//--------------------------------------------------------draggable panel
		//	//draggable panel properties
		//	var draggablePanel = new DraggablePanel(form);
		//	draggablePanel.Margin = Padding.Empty;
		//	draggablePanel.Dock = DockStyle.Top;
		//	draggablePanel.AutoSizeMode = AutoSizeMode.GrowOnly;

		//	//--------------------------------------------------------form panel
		//	//formPanel properties
		//	var formPanel = new FlowLayoutPanel();
		//	formPanel.FlowDirection = FlowDirection.TopDown;
		//	formPanel.AutoSize = true;
		//	formPanel.Width = masterPanel.Width;
		//	formPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
		//	BBAddSchemaControlsInPanel(ref fieldList, ref formPanel, errorProvider);
		//	if(myBBFormInstance.Processed || myBBFormInstance.IsNotificationOnly)
		//		NotificationManagerUtility.LockControlValues(formPanel);

		//	//--------------------------------------------------------button panel
		//	//button panels properties
		//	var buttonsPanel = new FlowLayoutPanel();
		//	buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
		//	buttonsPanel.AutoSize = true;
		//	buttonsPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
		//	buttonsPanel.Dock = DockStyle.Bottom;

		//	//--------------------------------------------------------timer panel
		//	//timer panel properties
		//	var timerPanel = new FlowLayoutPanel();
		//	timerPanel.FlowDirection = FlowDirection.LeftToRight;
		//	timerPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
		//	timerPanel.AutoSize = true;

		//	if(AutoClose)
		//	{
		//		formPanel.Visible = false;
		//		buttonsPanel.Visible = false;
		//		//add a timer to autoclose the form
		//		var timerLabel = new System.Windows.Forms.Label();
		//		timerLabel.AutoSize = false;
		//		timer = new Timer { Interval = 1000 };
		//		var startTime = DateTime.Now;
		//		//var secondsToWait = TimeOutOfAutoCloseNotifications;
		//		timer.Tick += (sender, args) =>
		//		{
		//			int elapsedSeconds = (int)(DateTime.Now - startTime).TotalSeconds;
		//			int remainingSeconds = TimeOutOfAutoCloseNotifications - elapsedSeconds;
		//			if(remainingSeconds <= 0)
		//			{
		//				timer.Stop();
		//				form.Close();
		//			}
		//			timerLabel.Text =
		//			String.Format("{0} seconds remaining...", remainingSeconds);
		//		};

		//		Button StopTheTimerBtn = new Button();
		//		StopTheTimerBtn.Text = "Edit the Form on Fly";
		//		using(Graphics cg = form.CreateGraphics())
		//		{
		//			SizeF size = cg.MeasureString(StopTheTimerBtn.Text, StopTheTimerBtn.Font);
		//			StopTheTimerBtn.Width = (int)size.Width + 20;

		//			size = cg.MeasureString("{0} seconds remaining...", timerLabel.Font);
		//			timerLabel.Width = (int)size.Width + 20;
		//		}
		//		StopTheTimerBtn.Click += (sender, args) =>
		//		{
		//			timer.Stop();
		//			//timerLabel.Text = "Timer Stopped";
		//			timerPanel.Visible = false;
		//			formPanel.Visible = true;
		//			buttonsPanel.Visible = true;
		//			form.Invalidate();
		//			form.StartPosition = FormStartPosition.Manual;
		//			var screen = Screen.FromPoint(form.Location);
		//			form.Location = new Point(screen.WorkingArea.Right - form.Width, screen.WorkingArea.Bottom - form.Height);
		//		};
		//		timer.Start();

		//		timerPanel.Controls.Add(StopTheTimerBtn);
		//		timerPanel.Controls.Add(timerLabel);

		//	}

		//	if(!myBBFormInstance.Processed)
		//	{
		//		//create sendFormButton
		//		Button SendFormBtn = new Button();
		//		SendFormBtn.Text = myBBFormInstance.IsNotificationOnly ?
		//							"Segna come letto" :
		//							"Invia approvazione";
		//		using(Graphics cg = form.CreateGraphics())
		//		{
		//			SizeF size = cg.MeasureString(SendFormBtn.Text, SendFormBtn.Font);
		//			SendFormBtn.Width = (int)size.Width + 20;
		//		}
		//		SendFormBtn.Click += (sender, args) =>
		//		{
		//			//controllo che tutti i campi obbligatori siano stati compilati
		//			if(!NotificationManagerUtility.AllControlsValidated(form, formPanel.Controls, errorProvider))
		//				return;

		//			var returnedschema = BBUpdateSchemaWithUserChanges(xmlSchema, fieldList);
		//			NotificationServiceWrapper.SetFormSchema(new TaskBuilderNet.Core.NotificationService.MyBBFormSchema
		//			{
		//				xmlSchema = returnedschema,
		//				myBBFormInstance = myBBFormInstance
		//			});
		//			if(timer != null)
		//				timer.Dispose();
		//			form.Close();
		//			RefreshDGVS.RaiseOnDifferentThread(this, EventArgs.Empty);
		//			RaiseBaseModuleEventHandler();
		//		};
		//		buttonsPanel.Controls.Add(SendFormBtn);
		//		form.AcceptButton = SendFormBtn;
		//	}
		//	//create closeButton
		//	Button CloseBtn = new Button();
		//	CloseBtn.Text = "Close";
		//	CloseBtn.Click += (sender, args) =>
		//	{
		//		if(timer != null)
		//			timer.Dispose();
		//		form.Close();
		//	};
		//	buttonsPanel.Controls.Add(CloseBtn);
		//	form.CancelButton = CloseBtn;

		//	//-----------------------------------------------add all panels in form
		//	form.Controls.Add(masterPanel);
		//	masterPanel.Controls.Add(draggablePanel);
		//	masterPanel.Controls.Add(formPanel);
		//	masterPanel.Controls.Add(timerPanel);
		//	masterPanel.Controls.Add(buttonsPanel);
		//	//-----------------------------------------------------------------

		//	// Define the border style of the form to a dialog box.
		//	//form.FormBorderStyle = FormBorderStyle.FixedDialog;
		//	form.FormBorderStyle = FormBorderStyle.None;
		//	// Set the MaximizeBox to false to remove the maximize box.
		//	form.MaximizeBox = false;
		//	// Set the MinimizeBox to false to remove the minimize box.
		//	form.MinimizeBox = false;
		//	////-----------------------------------------------------------------
		//	//form.Shown += (sender, args) =>
		//	//{
		//	//	form.Size = masterPanel.Size;
		//	//	if(Parent == null)
		//	//	{
		//	//		//Posiziono la form nell'angolo in basso a destra dello schermo principale
		//	//		//Rectangle r = Screen.PrimaryScreen.WorkingArea;
		//	//		//form.StartPosition = FormStartPosition.Manual;
		//	//		//form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - form.Width,
		//	//		//							Screen.PrimaryScreen.WorkingArea.Height - form.Height);

		//	//		//posiziono la form al centro del parent
		//	//		//form.StartPosition = FormStartPosition.CenterParent;

		//	//		try
		//	//		{
		//	//			Point locationOnForm = form.FindForm().PointToClient(
		//	//								form.Parent.PointToScreen(form.Location));
		//	//			form.StartPosition = FormStartPosition.Manual;
		//	//			form.Location = locationOnForm;
		//	//		}
		//	//		catch
		//	//		{
		//	//			form.StartPosition = FormStartPosition.CenterParent;
		//	//		}
		//	//	}
		//	//	else
		//	//	{
		//	//		//Posiziono la form nell'angolo in basso a destra del parent--------------
		//	//		Point location = Parent.PointToScreen(Point.Empty);
		//	//		var parentBottomX = location.X + Parent.ClientSize.Width;
		//	//		var parentBottomY = location.Y + Parent.ClientSize.Height;
		//	//		form.StartPosition = FormStartPosition.Manual;
		//	//		form.Location = new Point(parentBottomX - form.Width, parentBottomY - form.Height);
		//	//	}
		//	//};
		//	form.Load += (sender, args) =>
		//	{
		//		form.StartPosition = FormStartPosition.Manual;
		//		var screen = Screen.FromPoint(form.Location);
		//		form.Location = new Point(screen.WorkingArea.Right - form.Width, screen.WorkingArea.Bottom - form.Height);
		//	};

		//	NotificationManagerUtility.SetFlatStyleFlat(form);
		//	return form;
		//}

		#endregion oldBBgetFormMethod

		/// <summary>
		/// Metodo per aggiornare la lista di campi generati a partire dallo xml, con il valore
		/// presente all'interno della form
		/// </summary>
		/// <param name="fieldList">la lista dei campi (frutto della parsata)</param>
		/// <param name="panel">il flowlayout panel in cui ho inserito i campi, all'interno della form</param>
		private static void BBAddSchemaControlsInPanel(ref List<NotificationField> fieldList, ref TableLayoutPanel panel, ErrorProvider errorProvider = null)
		{
			panel.Width = 400;

			Size maximumLabelSize = new Size(panel.Width, 100);
			Size minimumLabelSize = new Size(panel.Width, 0);

			foreach(var field in fieldList)
			{
				if(field is NotificationLabel)
				{
					//label mode
					var newLabel = new System.Windows.Forms.Label();
					newLabel.Text = field.Label;
					//multiline fitted label
					newLabel.AutoSize = true;
					newLabel.MinimumSize = minimumLabelSize;
					newLabel.MaximumSize = maximumLabelSize;

					//newLabel.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
					panel.Controls.Add(newLabel);
				}

				if(field is NotificationText)
				{
					var newLabel = new System.Windows.Forms.Label();
					var newTextBox = new System.Windows.Forms.TextBox();

					newLabel.Font = panel.Font;
					newTextBox.Font = panel.Font;

					newTextBox.Tag = field;
					newLabel.Text = field.Label;
					newTextBox.Text = ((NotificationText)field).Value;
					newTextBox.Enabled = !((NotificationText)field).IsReadOnly;
					//multiline fitted label
					newLabel.AutoSize = true;
					newLabel.MinimumSize = minimumLabelSize;
					newLabel.MaximumSize = maximumLabelSize;

					newTextBox.AcceptsTab = false;
					newTextBox.AcceptsReturn = false;

					if(((NotificationText)field).Lines > 1)
					{
						newTextBox.Multiline = true;
						newTextBox.ScrollBars = ScrollBars.Both;
						newTextBox.Height *= ((NotificationText)field).Lines;
					}

					newTextBox.Width = panel.Width;
					newLabel.Width = panel.Width;
					//newTextBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
					//newLabel.Anchor = (AnchorStyles.Left | AnchorStyles.Right);

					//multiline fitted label
					newLabel.AutoSize = true;
					newLabel.MinimumSize = minimumLabelSize;
					newLabel.MaximumSize = maximumLabelSize;

					newTextBox.TextChanged += (sender, args) =>
					{
						((NotificationText)newTextBox.Tag).Value = newTextBox.Text;
					};

					//if is a mandatory textbox use errorProvider to show message
					if(((NotificationText)field).IsMandatory)
					{
						newTextBox.Width -= errorProvider.Icon.Width;
						newLabel.Text += "*";
						newTextBox.Validated += (sender, cancelEventArgs) =>
						{
							if(String.IsNullOrEmpty(newTextBox.Text))
								errorProvider.SetError(newTextBox, NotificationManagerStrings.RequiredField);
							else
								errorProvider.SetError(newTextBox, string.Empty);
						};
					}
					panel.Controls.Add(newLabel);
					panel.Controls.Add(newTextBox);
				}

				if(field is NotificationDropDown<string>)
				{
					var newLabel = new System.Windows.Forms.Label();
					var listBox = new System.Windows.Forms.ListBox();

					listBox.DrawMode = DrawMode.OwnerDrawFixed;
					listBox.DrawItem += listBox_DrawItem;

					/*listBox.DrawItem += (sender, e) =>
					{
						if(e.Index < 0)
							return;
						//if the item state is selected them change the back color
						if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
							e = new DrawItemEventArgs(e.Graphics,
													  e.Font,
													  e.Bounds,
													  e.Index,
													  e.State ^ DrawItemState.Selected,
													  e.ForeColor,
													  Color.Yellow);//Choose the color

						// Draw the background of the ListBox control for each item.
						e.DrawBackground();
						// Draw the current item text
						e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
						// If the ListBox has focus, draw a focus rectangle around the selected item.
						e.DrawFocusRectangle();
					};*/

					newLabel.Font = panel.Font;
					listBox.Font = panel.Font;

					listBox.Tag = field;
					newLabel.Text = field.Label;
					listBox.Items.AddRange((((NotificationDropDown<string>)field).ItemList.Select(x => x.Description).ToArray()));
					listBox.SelectedItem = ((NotificationDropDown<string>)field).DefaultDescription;

					listBox.Width = panel.Width;
					//newLabel.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
					//listBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);

					//multiline fitted label
					newLabel.AutoSize = true;
					newLabel.MinimumSize = minimumLabelSize;
					newLabel.MaximumSize = maximumLabelSize;

					//adjust height
					listBox.IntegralHeight = false;
					listBox.Height = listBox.ItemHeight * listBox.Items.Count + listBox.ItemHeight;

					listBox.SelectedValueChanged += (sender, args) =>
					{
						((NotificationDropDown<string>)listBox.Tag).UpdateDefaultValue(listBox.SelectedItem.ToString());
					};

					panel.Controls.Add(newLabel);
					panel.Controls.Add(listBox);
				}

				if(field is NotificationDateEdit)
				{
					var newLabel = new System.Windows.Forms.Label();
					var newDateEdit = new System.Windows.Forms.DateTimePicker();

					newLabel.Font = panel.Font;
					newDateEdit.Font = panel.Font;

					newDateEdit.Tag = field;

					newLabel.Text = field.Label;
					newDateEdit.Value = ((NotificationDateEdit)field).DateTime;

					//multiline fitted label
					newLabel.AutoSize = true;
					newLabel.MinimumSize = minimumLabelSize;
					newLabel.MaximumSize = maximumLabelSize;

					//newLabel.Anchor = (AnchorStyles.Left | AnchorStyles.Right);

					newDateEdit.ValueChanged += (sender, args) =>
					{
						((NotificationDateEdit)newDateEdit.Tag).DateTime = newDateEdit.Value;
					};
					panel.Controls.Add(newLabel);
					panel.Controls.Add(newDateEdit);
				}
			}
		}

		private static void listBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			if(e.Index < 0)
				return;

			MainColorsTheme Colors = NotificationManagerUtility.GetMainColorsTheme();

			//Color BluChiaro=	Color.FromArgb(205,215,237);//BackGround	Color -- Should get theme Color
			//Color BluScuro=		Color.FromArgb(82, 123,158);//Fore			Color
			SolidBrush BluScuroBrush = new SolidBrush(Colors.Text);

			//if the item state is selected them change the back color 
			if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
				e = new DrawItemEventArgs(e.Graphics,
										  e.Font,
										  e.Bounds,
										  e.Index,
										  e.State ^ DrawItemState.Selected,
					//e.ForeColor,
										  Colors.Text,
										  Colors.Hover);

			// Draw the background of the ListBox control for each item.
			e.DrawBackground();

			// Draw the current item text
			if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
				e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, BluScuroBrush, e.Bounds, StringFormat.GenericDefault);
			else
				e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
			// If the ListBox has focus, draw a focus rectangle around the selected item.
			e.DrawFocusRectangle();

			//*************************************************************************************************************************

			//if(e.Index < 0)
			//	return;

			//MainColorsTheme Colors = NotificationManagerUtility.GetMainColorsTheme();
			//SolidBrush BluScuroBrush = new SolidBrush(Colors.Text);

			////if the item state is selected them change the back color
			//if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			//{
			//	e = new DrawItemEventArgs(e.Graphics,
			//							  e.Font,
			//							  e.Bounds,
			//							  e.Index,
			//							  e.State ^ DrawItemState.Selected,
			//							  Colors.Text,
			//							  Colors.Hover);

			//	e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, BluScuroBrush, e.Bounds, StringFormat.GenericDefault);
			//}
			//else 
			//{
			//	e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
			//}
			//// Draw the background of the ListBox control for each item.
			//e.DrawBackground();

			//// If the ListBox has focus, draw a focus rectangle around the selected item.
			//e.DrawFocusRectangle();

			//*************************************************************************************************************************

			//ListBox list = (ListBox)sender;

			//Color foreColor = NotificationManagerUtility.GetMainColorsTheme().Text;

			//Color backColor = NotificationManagerUtility.GetMainColorsTheme().Background;

			//// Get the Bounding rectangle for a selected item

			//if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			//{
			//	Rectangle SelRect = new Rectangle(e.Bounds.X, e.Bounds.Y,
			//	e.Bounds.Width - 1, e.Bounds.Height - 1);

			//	using(Brush backBrush = new SolidBrush(backColor))
			//	{
			//		// Paint the item background in the wanted color
			//		e.Graphics.FillRectangle(backBrush, SelRect);
			//	}
			//	using(Pen p = new Pen(Color.Empty))
			//	{
			//		if(e.State == DrawItemState.Selected)
			//		{
			//			// Set the pen color
			//			p.Color = Color.Black;
			//		}
			//		else
			//		{
			//			// Set the pen color
			//			p.Color = list.BackColor;
			//		}
			//		// Draw the selection rectangle in either black or the lisbox backcolor to hide it
			//		e.Graphics.DrawRectangle(p, SelRect);
			//	}
			//	e.DrawFocusRectangle();

			//	using(Brush brush = new SolidBrush(foreColor))
			//	{
			//		using(Font font = new Font(list.Font, FontStyle.Regular))
			//		{
			//			string text = list.Items[e.Index].ToString();

			//			e.Graphics.DrawString(text, font, brush, e.Bounds.X, e.Bounds.Y
			//			+ 1);
			//		}
			//	}
			//}
		}

		/// <summary>
		/// Metodo utilizzato per parsare lo schema xml ricevuto dal webService
		/// </summary>
		/// <param name="xmlSchema">lo schema xml della form</param>
		/// <returns>una lista di campi che poi vado ad inserire dinamicamente nella form dinamica</returns>
		private static List<NotificationField> BBParseXmlSchema(string xmlSchema)
		{
			var fieldList = new List<NotificationField>();
			try
			{
				XDocument doc = XDocument.Parse(xmlSchema);
				XNamespace ns = "http://schemas.datacontract.org/2004/07/PAT.CRM.WSC4.WorkflowObjects.Forms.Metadata";
				XNamespace nsi = "http://www.w3.org/2001/XMLSchema-instance";
				XNamespace nsz = "http://schemas.microsoft.com/2003/10/Serialization/";

				//todo: vedere se è possibile scorrere l'xDocument una sola volta mettendo in OR i tipi

				var labelItems = (from item in doc.Descendants(ns + "Field")
								  where (item.Attribute(nsi + "type").Value.Equals("Label"))
								  select new NotificationLabel(
											 item.Attribute(nsz + "Id").Value,
											 item.Descendants(ns + "Label").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "FieldName").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "Description").Select(x => x.Value).FirstOrDefault()
											 )).ToList<NotificationLabel>();

				var textItems = (from item in doc.Descendants(ns + "Field")
								 where (item.Attribute(nsi + "type").Value.Equals("Text"))
								 select new NotificationText(
										   item.Attribute(nsz + "Id").Value,
										   item.Descendants(ns + "Label").Select(x => x.Value).FirstOrDefault(),
										   item.Descendants(ns + "FieldName").Select(x => x.Value).FirstOrDefault(),
										   item.Descendants(ns + "Description").Select(x => x.Value).FirstOrDefault(),
										   item.Descendants(ns + "IsMandatory").Select(x => x.Value).FirstOrDefault(),
										   item.Descendants(ns + "IsReadOnly").Select(x => x.Value).FirstOrDefault(),
										   item.Descendants(ns + "Value").Select(x => x.Value).FirstOrDefault(),
										   item.Descendants(ns + "Lines").Select(x => x.Value).FirstOrDefault()
										   )).ToList<NotificationText>();

				var dropDownItems = (from item in doc.Descendants(ns + "Field")
									 where (item.Attribute(nsi + "type").Value.Equals("DropDown"))
									 select new NotificationDropDown<string>(
											 item.Attribute(nsz + "Id").Value,
											 item.Descendants(ns + "Label").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "FieldName").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "Description").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "IsMandatory").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "IsReadOnly").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "Value").Select(x => x.Value).FirstOrDefault(), //string (as the T of the class)
											 (
													 from listItem in item.Descendants(ns + "ListItem")
													 select new NotificationDropDownItem<string>
													 {
														 Description = listItem.Descendants(ns + "Description").Select(x => x.Value).FirstOrDefault(),
														 Value = listItem.Descendants(ns + "Value").Select(x => x.Value).FirstOrDefault()
													 }
											  ).ToList<NotificationDropDownItem<string>>())
										 ).ToList<NotificationDropDown<string>>();

				var dateEditItems = (from item in doc.Descendants(ns + "Field")
									 where (item.Attribute(nsi + "type").Value.Equals("DateEdit"))
									 select new NotificationDateEdit(
											 item.Attribute(nsz + "Id").Value,
											 item.Descendants(ns + "Label").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "FieldName").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "Description").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "IsMandatory").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "IsReadOnly").Select(x => x.Value).FirstOrDefault(),
											 item.Descendants(ns + "Value").Select(x => x.Value).FirstOrDefault() //string (as the T of the class)
										 )).ToList<NotificationDateEdit>();

				fieldList.AddRange(labelItems);
				fieldList.AddRange(textItems);
				fieldList.AddRange(dropDownItems);
				fieldList.AddRange(dateEditItems);
			}
			catch(Exception)
			{
				fieldList.Add(new NotificationLabel("0", NotificationManagerStrings.ErrorParsing, "", NotificationManagerStrings.Description));
			}

			return fieldList.OrderBy(x => Int32.Parse(x.WfId.Substring(1))).ToList();
		}

		/// <summary>
		/// Metodo per aggiornare lo schema ricevuto dal web service, con i campi compilati nella form dal client
		/// in modo da poterlo rispedire indietro e inviarlo al flusso di lavoro
		/// </summary>
		/// <param name="xmlSchema">lo schema xml ricevuto dal notification Service</param>
		/// <param name="fieldList">la lista dei campi aggiornati dalla form</param>
		/// <returns>lo schema xml da inviare al notificaiton service per "rispondere alla form"</returns>
		private static string BBUpdateSchemaWithUserChanges(string xmlSchema, List<NotificationField> fieldList)
		{
			XDocument doc = XDocument.Parse(xmlSchema);
			XNamespace ns = "http://schemas.datacontract.org/2004/07/PAT.CRM.WSC4.WorkflowObjects.Forms.Metadata";
			XNamespace nsi = "http://www.w3.org/2001/XMLSchema-instance";
			XNamespace nsz = "http://schemas.microsoft.com/2003/10/Serialization/";
			foreach(var field in fieldList)
			{
				if(field is NotificationText)
				{
					var xElement = (from item in doc.Descendants(ns + "Field")
									where (item.Attribute(nsz + "Id").Value.Equals(field.WfId))
									select item.Descendants(ns + "Value").Single()).Single();
					if(((NotificationText)field).Value != "")
					{
						xElement.RemoveAttributes();
						xElement.Add(
							new XAttribute(XNamespace.Xmlns + "d6p1", "http://www.w3.org/2001/XMLSchema"),
							new XAttribute(nsi + "type", "d6p1:string"));
						xElement.SetValue(((NotificationText)field).Value);
					}
				}
				if(field is NotificationDropDown<string>)
				{
					var xElement = (from item in doc.Descendants(ns + "Field")
									where (item.Attribute(nsz + "Id").Value.Equals(field.WfId))
									select item.Descendants(ns + "Value").First()).Single();
					xElement.SetValue(((NotificationDropDown<string>)field).DefaultValue);
				}
				if(field is NotificationDateEdit)
				{
					var xElement = (from item in doc.Descendants(ns + "Field")
									where (item.Attribute(nsz + "Id").Value.Equals(field.WfId))
									select item.Descendants(ns + "Value").First()).Single();
					xElement.RemoveAttributes();
					xElement.Add(
						new XAttribute(XNamespace.Xmlns + "d6p1", "http://www.w3.org/2001/XMLSchema"),
						new XAttribute(nsi + "type", "d6p1:dateTime"));
					xElement.SetValue(((NotificationDateEdit)field).DateTime.ToString("s"));
				}
			}
			return doc.ToString();
		}

		/// <summary>
		/// -> base form + attachmentId, Requester
		/// </summary>
		/// <param name="baseFormInfo"></param>
		/// <returns></returns>
		private MyBBFormInstanceExtended BBMyBBFormInstanceExtendedFromBase(MyBBFormInstanceBase baseFormInfo)
		{
			string requester = string.Empty;
			string description = string.Empty;
			int attachmentId = 0;
			int temp;
			try
			{
				var myBBForm = NotificationServiceWrapper.GetForm(baseFormInfo.FormInstanceId);
				if(myBBForm != null)
				{
					XDocument doc = XDocument.Parse(myBBForm.xmlSchema);
					XNamespace ns = "http://schemas.datacontract.org/2004/07/PAT.CRM.WSC4.WorkflowObjects.Forms.Metadata";
					XNamespace nsi = "http://www.w3.org/2001/XMLSchema-instance";

					requester = (from item in doc.Descendants(ns + "Field")
								 where (item.Attribute(nsi + "type").Value.Equals("Text") &&
									item.Descendants(ns + "FieldName").Select(x => x.Value).FirstOrDefault() == "RequesterTxt")
								 select item.Descendants(ns + "Value").Select(x => x.Value).FirstOrDefault()).FirstOrDefault();

					description = (from item in doc.Descendants(ns + "Field")
								   where (item.Attribute(nsi + "type").Value.Equals("Text") &&
									  item.Descendants(ns + "FieldName").Select(x => x.Value).FirstOrDefault() == "DescriptionTxt")
								   select item.Descendants(ns + "Value").Select(x => x.Value).FirstOrDefault()).FirstOrDefault();

					bool parsedInt = Int32.TryParse(
							(from item in doc.Descendants(ns + "Field")
							 where (item.Attribute(nsi + "type").Value.Equals("Text") &&
								item.Descendants(ns + "FieldName").Select(x => x.Value).FirstOrDefault() == "AttachmentIdTxt")
							 select item.Descendants(ns + "Value").Select(x => x.Value).FirstOrDefault()).FirstOrDefault(), out temp);

					if(parsedInt)
						attachmentId = temp;
				}
			}
			catch(Exception) { }//se ci sono stati errori. utilizzo i valori di default

			return new MyBBFormInstanceExtended
			{
				AttachmentId = attachmentId,
				DateProcessed = baseFormInfo.DateProcessed,
				DateSubmitted = baseFormInfo.DateSubmitted,
				FormInstanceId = baseFormInfo.FormInstanceId,
				Processed = baseFormInfo.Processed,
				Requester = requester,
				Description = description,
				Title = baseFormInfo.Title,
				UserName = baseFormInfo.UserName
			};
		}

		/// <summary>
		/// Restituisce l'url a cui contattare il servizio di Brain Business o string.Empty in caso di string nulla O.o
		/// </summary>
		/// <returns></returns>
		public string BBGetBrainBusinessServiceUrl()
		{
			return NotificationServiceWrapper.GetBrainBusinessServiceUrl();
		}

		/// <summary>
		/// Aggiorna l'indirizzo a cui contattare il servizio di Brain Business
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public bool UpdateBrainBusinessServiceUrl(string url)
		{
			return NotificationServiceWrapper.UpdateBrainBusinessServiceUrl(url);
		}

		#endregion NotifcationManagerMethods

		//private class CustomListBox : ListBox
		//{
		//	protected override void DrawItemEventHandler(object sender, DrawItemEventArgs e)
		//	{
		//		if(e.Index < 0)
		//			return;
		//		//if the item state is selected them change the back color
		//		if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
		//			e = new DrawItemEventArgs(e.Graphics,
		//									  e.Font,
		//									  e.Bounds,
		//									  e.Index,
		//									  e.State ^ DrawItemState.Selected,
		//									  e.ForeColor,
		//									  Color.Yellow);//Choose the color

		//		// Draw the background of the ListBox control for each item.
		//		e.DrawBackground();
		//		// Draw the current item text
		//		e.Graphics.DrawString(this.Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
		//		// If the ListBox has focus, draw a focus rectangle around the selected item.
		//		e.DrawFocusRectangle();
		//	}
		//}
	}
}