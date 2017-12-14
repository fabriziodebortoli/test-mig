using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.NotificationService;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.NotificationManager
{
	public abstract class BaseNotificationModule
	{
		protected bool isViewer;
		internal static readonly int TimeOutOfAutoCloseNotifications = 1000;
		internal MainColorsTheme Colors;

		public NotificationServiceWrapper NotificationServiceWrapper { get; set; }

		public event EventHandler BaseModuleEventHandler;

		public BaseNotificationModule(NotificationServiceWrapper notificationServiceWrapper, bool isViewer)
		{
			this.NotificationServiceWrapper = notificationServiceWrapper;
			this.isViewer = isViewer;
			Colors = NotificationManagerUtility.GetMainColorsTheme();
		}
		/// <summary>
		/// metodo che deve essere implementato per ciascun modulo e che restituisce le notifiche specifiche per lui
		/// </summary>
		/// <returns></returns>
		public abstract IList<IGenericNotify> GetNotifications();

		/// <summary>
		/// Metodo richiamabile da tutti i moduli che estendo questo base, per lanciare eventi al notification manager
		/// </summary>
		public void RaiseBaseModuleEventHandler()
		{
			BaseModuleEventHandler.Raise(this, new EventArgs());
		}

		/// <summary>
		/// Metodo richiamabile da tutti i moduli che estendo questo base, per lanciare eventi al notification manager su un thread differente
		/// </summary>
		public void RaiseBaseModuleEventHandleronDifferentThread()
		{
			BaseModuleEventHandler.RaiseOnDifferentThread(this, new EventArgs());
		}

		/// <summary>
		/// restituisce la form di notifica da visualizzare,a cui però andaranno aggiunti i vari pannelli
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		private static Form CreateForm(string title = "New notify")
		{
			if(string.IsNullOrWhiteSpace(title) || title == "New notify")
				title = NotificationManagerStrings.NewNotify;
			//form properties
			var form = new PopupForm();
			form.AutoSize = true;
			form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			form.Width = 400;
			form.Text = title;
			form.TopMost = true;
			//form.BackColor = Color.FromArgb(43, 120, 228);
			try
			{
				//ITheme theme = DefaultTheme.GetTheme();
				//form.BackColor = theme.GetThemeElementColor("MainMenuSecondaryTabBkgColor");
				form.BackColor = NotificationManagerUtility.GetMainColorsTheme().Primary;
			}
			catch(Exception)
			{
				form.BackColor = Color.Empty;
			}

			form.Padding = new Padding(2);
			form.Font = new Font("Segoe UI", 9);
			// Define the border style of the form to a dialog box.
			form.FormBorderStyle = FormBorderStyle.None;
			// Set the MaximizeBox to false to remove the maximize box.
			form.MaximizeBox = false;
			// Set the MinimizeBox to false to remove the minimize box.
			form.MinimizeBox = false;
			return form;
		}

		/// <summary>
		/// pannello contenitore
		/// </summary>
		/// <returns></returns>
		private static FlowLayoutPanel CreateMasterPanel()
		{
			var masterPanel = new FlowLayoutPanel();
			masterPanel.Margin = Padding.Empty;
			masterPanel.FlowDirection = FlowDirection.TopDown;
			masterPanel.AutoSize = true;
			masterPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			masterPanel.Width = 400;
			masterPanel.BackColor = Color.White;
			masterPanel.Dock = DockStyle.Fill;
			return masterPanel;
		}

		/// <summary>
		/// pannellino draggable, o meglio: pannellino che va inserito in una form, 
		/// passandogliela come parametro, trascinando il pannello si muove anche la form
		/// </summary>
		/// <param name="parentForm"></param>
		/// <returns></returns>
		private static DraggablePanel CreateDraggablePanel(Form parentForm)
		{
			//fix null parentForm
			if(parentForm == null)
				parentForm = new Form();
			//draggable panel properties
			var draggablePanel = new DraggablePanel(parentForm);
			draggablePanel.Margin = Padding.Empty;
			draggablePanel.Dock = DockStyle.Top;
			draggablePanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			return draggablePanel;
		}

		/// <summary>
		/// restituisce il pannello con un timer di auto chiusura e i due pulsanti per espandere il pannello di contenuto e di chiusura
		/// </summary>
		/// <param name="parentForm"></param>
		/// <param name="contentPanel"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		private static FlowLayoutPanel CreateButtonsPanelWithTimer(Form parentForm, Panel contentPanel, int timeout)
		{
			//struttura pensata: 
			//	TimeOutContainerPanel
			//		-timerPanel = contains only the label changing at every tick
			//		-timeOutButtonsPanel = contains close and expand buttons

			//container
			var TimeOutContainerPanel = new FlowLayoutPanel();
			TimeOutContainerPanel.FlowDirection = FlowDirection.TopDown;
			TimeOutContainerPanel.AutoSize = true;
			TimeOutContainerPanel.AutoSizeMode = AutoSizeMode.GrowOnly;

			//label panel
			var timerPanel = new FlowLayoutPanel();
			timerPanel.FlowDirection = FlowDirection.LeftToRight;
			timerPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			timerPanel.AutoSize = true;
			timerPanel.Dock = DockStyle.Top;

			//buttons panel
			var timeOutButtonsPanel = new FlowLayoutPanel();
			timeOutButtonsPanel.FlowDirection = FlowDirection.RightToLeft;
			timeOutButtonsPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			timeOutButtonsPanel.AutoSize = true;
			timeOutButtonsPanel.Dock = DockStyle.Bottom;
			
			//add the panels to the container
			TimeOutContainerPanel.Controls.Add(timerPanel);
			TimeOutContainerPanel.Controls.Add(timeOutButtonsPanel);

			//hide the content panel
			contentPanel.Visible = false;

			//create and start the timer
			var timer = new Timer { Interval = 1000 };
			var startTime = DateTime.Now;
			//the label
			var timerLabel = new System.Windows.Forms.Label();
			timerLabel.AutoSize = false;
			timerPanel.Controls.Add(timerLabel);
			//tick event
			timer.Tick += (sender, args) =>
			{
				int elapsedSeconds = (int)(DateTime.Now - startTime).TotalSeconds;
				int remainingSeconds = timeout - elapsedSeconds;
				if(remainingSeconds <= 0)
				{
					timer.Stop();
					parentForm.Close();
				}
				timerLabel.Text =
					String.Format(NotificationManagerStrings.RemainingsSeconds, remainingSeconds);
			};

			//Close button
			Button CloseBtn = new Button();
			CloseBtn.Text = NotificationManagerStrings.Close;
			//click event
			CloseBtn.Click += (sender, args) =>
			{
				timer.Stop();
				timer.Dispose();
				parentForm.Close();
			};

			//Expand button
			Button StopTheTimerBtn = new Button();
			StopTheTimerBtn.Text = NotificationManagerStrings.Expand;
			//click event
			StopTheTimerBtn.Click += (sender, args) =>
			{
				timer.Stop();
				timer.Dispose();
				TimeOutContainerPanel.Visible = false;
				contentPanel.Visible = true;
				parentForm.Invalidate();
				parentForm.StartPosition = FormStartPosition.Manual;
				var screen = Screen.FromPoint(parentForm.Location);
				parentForm.Location = new Point(screen.WorkingArea.Right - parentForm.Width, screen.WorkingArea.Bottom - parentForm.Height);
			};
			
			//resize buttons based on text lenght
			using(Graphics cg = parentForm.CreateGraphics())
			{
				SizeF size = cg.MeasureString(NotificationManagerStrings.RemainingsSeconds, timerLabel.Font);
				timerLabel.Width = (int)size.Width + 20;
				
				size = cg.MeasureString(StopTheTimerBtn.Text, StopTheTimerBtn.Font);
				StopTheTimerBtn.Width = (int)size.Width + 20;

				size = cg.MeasureString(CloseBtn.Text, CloseBtn.Font);
				CloseBtn.Width = (int)size.Width + 20;
			}
			//adding buttons in the panel
			timeOutButtonsPanel.Controls.Add(StopTheTimerBtn);
			timeOutButtonsPanel.Controls.Add(CloseBtn);
			
			//setting button as accept and close buttons
			parentForm.AcceptButton = StopTheTimerBtn;
			parentForm.CancelButton = CloseBtn;
			//start the timer in easyway
			timer.Start();

			return TimeOutContainerPanel;
		}

		/// <summary>
		/// restituisce la form di notifica pronta da visualizzare, 
		/// gli va passato un pannello, che verrà visualizzato solo cliccando sul tasto di espansione
		/// </summary>
		/// <param name="contentPanel">pannello con il contenuto della notifica</param>
		/// <param name="title">titolo della form</param>
		/// <param name="timeOut">se 0, non viene impostato il timeout e viene visualizzato immediatamente il pannello col contenuto, 
		/// altrimenti il contenuto viene visualizzato solo su richiesta dell'utente</param>
		/// <returns></returns>
		public static Form CreateCompleteNotificationForm(Panel contentPanel, string title= "New Form", int timeOut = 0)
		{
			if(title == "New Form")
				title = NotificationManagerStrings.NewForm;

			//fix null Panel
			if(contentPanel == null)
				contentPanel = new FlowLayoutPanel();

			//create the form
			var form = CreateForm(title);

			//create and add the masterPanel
			var masterPanel = CreateMasterPanel();
			form.Controls.Add(masterPanel);

			//create and add DraggablePanel
			var draggablePanel = CreateDraggablePanel(form);
			masterPanel.Controls.Add(draggablePanel);

			//add the content panel
			contentPanel.Font = form.Font;
			masterPanel.Controls.Add(contentPanel);

			//if timeOut is setted, create and add the timeOut Buttons Panel
			if(timeOut!=0)
			{
				var timeOutButtonsPanel = CreateButtonsPanelWithTimer(form, contentPanel, timeOut);
				masterPanel.Controls.Add(timeOutButtonsPanel); 
			}

			// setting the position 
			form.Load += (sender, args) =>
			{
				form.StartPosition = FormStartPosition.Manual;
				var screen = Form.ActiveForm == null ? Screen.FromPoint(form.Location) : Screen.FromPoint(Form.ActiveForm.Location);
				form.Location = new Point(screen.WorkingArea.Right - form.Width, screen.WorkingArea.Bottom - form.Height);
				//set the flatstyle
				NotificationManagerUtility.SetFlatStyleFlat(form);
			};

			return form;
		}

		/// <summary>
		/// Versione semplificata per mostrare una notifica con un messaggio, utile ad esempio per debug
		/// </summary>
		/// <param name="message"></param>
		/// <param name="timeOut"></param>
		/// <returns></returns>
		internal static Form CreateMessageNotificationForm(string message, string title= "New message", int timeOut=0)
		{
			if(title == "New message")
				title = NotificationManagerStrings.NewMessage;

			//--------------------------------------------------------content Container panel
			//content containerPanel properties
			var contentContainerPanel = new FlowLayoutPanel();
			contentContainerPanel.FlowDirection = FlowDirection.TopDown;
			contentContainerPanel.AutoSize = true;
			contentContainerPanel.AutoSizeMode = AutoSizeMode.GrowOnly;

			//--------------------------------------------------------content panel
			//content panel properties
			var contentPanel = new FlowLayoutPanel();
			contentPanel.FlowDirection = FlowDirection.TopDown;
			contentPanel.AutoSize = true;
			contentPanel.Width = 400;
			//the message in the content panel
			var messageLabel = new System.Windows.Forms.Label();
			messageLabel.AutoSize = true;
			messageLabel.Text = message;
			contentPanel.Controls.Add(messageLabel);

			//--------------------------------------------------------content buttons panel
			//content buttons panel properties
			var contentButtonsPanel = new FlowLayoutPanel();
			contentButtonsPanel.FlowDirection = FlowDirection.RightToLeft;
			contentButtonsPanel.AutoSize = true;
			contentButtonsPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			contentButtonsPanel.Dock = DockStyle.Bottom;
			//Close button
			Button CloseBtn = new Button();
			CloseBtn.Text = NotificationManagerStrings.Close;
			//adding the button
			contentButtonsPanel.Controls.Add(CloseBtn);

			//adding the content and button panels in the container
			contentContainerPanel.Controls.Add(contentPanel);
			contentContainerPanel.Controls.Add(contentButtonsPanel);
			
			//create the form
			var form = CreateCompleteNotificationForm(contentContainerPanel, title, timeOut);

			//click event
			CloseBtn.Click += (sender, args) =>
			{
				form.Close();
			};

			return form;
		}

		/// <summary>
		/// Visualizza una form contenente il messaggio della milestone ricevuta
		/// dopo 10 secondi si chiude
		/// </summary>
		/// <param name="message">il contenuto della milestone</param>
		public static void ShowMessage(string message, string title = "New message")
		{
			if(title == "New message")
				title = NotificationManagerStrings.NewMessage;

			Form activeForm = Form.ActiveForm;

			if(activeForm != null && activeForm.InvokeRequired)
			{
				activeForm.Invoke(new Action(() => ShowMessage(message)));
				return;
			}

			var form = CreateMessageNotificationForm(message, title, 10);
			
			//form.ShowDialog(activeForm);

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

		

		#region oldShowMessageMethod
		///// <summary>
		///// Visualizza una form contenente il messaggio della milestone ricevuta
		///// dopo 10 secondi si chiude
		///// </summary>
		///// <param name="message">il contenuto della milestone</param>
		//public static void ShowMessage(string message)
		//{
		//	if(message == null)
		//		NotificationManagerUtility.SetMessage("Errore nei parametri ricevuti", string.Empty, "Errore!");

		//	Form activeForm = Form.ActiveForm;

		//	if(activeForm != null && activeForm.InvokeRequired)
		//	{
		//		activeForm.Invoke(new Action(() => ShowMessage(message)));
		//		return;
		//	}
		//	//--------------------------------------------------------form
		//	//form properties
		//	var form = new Form();
		//	form.AutoSize = true;
		//	form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		//	form.Text = "Nuova notifica";
		//	form.TopMost = true;
		//	form.BackColor = Color.FromArgb(43, 120, 228);
		//	form.Padding = new Padding(2);
		//	form.Font = new Font("Segoe UI", 9);
		//	// Define the border style of the form to a dialog box.
		//	form.FormBorderStyle = FormBorderStyle.None;
		//	// Set the MaximizeBox to false to remove the maximize box.
		//	form.MaximizeBox = false;
		//	// Set the MinimizeBox to false to remove the minimize box.
		//	form.MinimizeBox = false;
			
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
			
		//	//--------------------------------------------------------content panel
		//	//content panel properties
		//	var contentPanel = new FlowLayoutPanel();
		//	contentPanel.FlowDirection = FlowDirection.TopDown;
		//	contentPanel.AutoSize = true;
		//	contentPanel.Width = masterPanel.Width;
		//	masterPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			
		//	//the message in the content panel
		//	var messageLabel = new System.Windows.Forms.Label();
		//	messageLabel.AutoSize = true;
		//	messageLabel.Text = message;
		//	contentPanel.Controls.Add(messageLabel);
			
		//	//--------------------------------------------------------timer panel
		//	//timer panel properties
		//	var timerPanel = new FlowLayoutPanel();
		//	timerPanel.FlowDirection = FlowDirection.LeftToRight;
		//	timerPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
		//	timerPanel.AutoSize = true;

		//	//adding timer to autoclose the form
		//	var timerLabel = new System.Windows.Forms.Label();
		//	timerLabel.AutoSize = false;
		//	var timer = new Timer { Interval = 1000 };
		//	var startTime = DateTime.Now;
		//	//var secondsToWait = 10;
		//	timer.Tick += (sender, args) =>
		//	{
		//		int elapsedSeconds = (int)(DateTime.Now - startTime).TotalSeconds;
		//		int remainingSeconds = TimeOutOfAutoCloseNotifications - elapsedSeconds;
		//		if(remainingSeconds <= 0)
		//		{
		//			timer.Stop();
		//			form.Close();
		//		}
		//		timerLabel.Text =
		//			String.Format("{0} seconds remaining...", remainingSeconds);
		//	};
			
		//	//add the button to stop the timer
		//	Button StopTheTimerBtn = new Button();
		//	StopTheTimerBtn.Text = "Stop the timer";
		//	using(Graphics cg = form.CreateGraphics())
		//	{
		//		SizeF size = cg.MeasureString(StopTheTimerBtn.Text, StopTheTimerBtn.Font);
		//		StopTheTimerBtn.Width = (int)size.Width + 20;

		//		size = cg.MeasureString("{0} seconds remaining...", timerLabel.Font);
		//		timerLabel.Width = (int)size.Width + 20;
		//	}
		//	StopTheTimerBtn.Click += (sender, args) =>
		//	{
		//		timer.Stop();
		//		timerLabel.Text = "Timer Stopped";
		//		StopTheTimerBtn.Enabled = false;
		//	};

		//	timer.Start();

		//	timerPanel.Controls.Add(StopTheTimerBtn);
		//	timerPanel.Controls.Add(timerLabel);
			
		//	//--------------------------------------------------------button panel
		//	//button panels properties
		//	var buttonsPanel = new FlowLayoutPanel();
		//	buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
		//	buttonsPanel.AutoSize = true;
		//	buttonsPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
		//	buttonsPanel.Dock = DockStyle.Bottom;
		//	//create OkButton
		//	Button OkButton = new Button();
		//	OkButton.Text = "Ok";
		//	using(Graphics cg = form.CreateGraphics())
		//	{
		//		SizeF size = cg.MeasureString(OkButton.Text, OkButton.Font);
		//		OkButton.Width = (int)size.Width + 20;
		//	}
		//	OkButton.Click += (sender, args) =>
		//	{
		//		timer.Dispose();
		//		form.Close();
		//	};
		//	buttonsPanel.Controls.Add(OkButton);
		//	form.AcceptButton = form.CancelButton = OkButton;

		//	//-----------------------------------------------add all panels in form
		//	form.Controls.Add(masterPanel);
		//	masterPanel.Controls.Add(draggablePanel);
		//	masterPanel.Controls.Add(contentPanel);
		//	masterPanel.Controls.Add(timerPanel);
		//	masterPanel.Controls.Add(buttonsPanel);
		//	//-----------------------------------------------------------------
			
		//	//form.Load += (sender, args) =>
		//	//{
		//	//	form.Size = masterPanel.Size;

		//	//	if(parent == null)
		//	//	{
		//	//		//Posiziono la form nell'angolo in basso a destra dello schermo principale
		//	//		//form.StartPosition = FormStartPosition.Manual;
		//	//		//form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - form.Width,
		//	//		//							Screen.PrimaryScreen.WorkingArea.Height - form.Height);

		//	//		//posiziono la form al centro del parent
		//	//		//form.StartPosition = FormStartPosition.CenterParent;

		//	//		Point locationOnForm = form.FindForm().PointToClient(
		//	//				form.Parent.PointToScreen(form.Location));
		//	//		form.StartPosition = FormStartPosition.Manual;
		//	//		form.Location = locationOnForm;
		//	//	}
		//	//	else
		//	//	{
		//	//		//Posiziono la form nell'angolo in basso a destra del parent--------------
		//	//		Point location = parent.PointToScreen(Point.Empty);
		//	//		var parentBottomX = location.X + parent.ClientSize.Width;
		//	//		var parentBottomY = location.Y + parent.ClientSize.Height;
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
		//	form.ShowDialog(activeForm);
		//}
		#endregion
	}
}
