using Microarea.EasyAttachment.BusinessLogic;
using Microarea.TaskBuilderNet.Core.NotificationManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WinControls;
using SailorsPromises;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyAttachment.UI.Forms
{
	public enum ProgressBarStatus { Error = 0, Ok = 1, UnKnown = 2 };

	public partial class BBSettingsForm : MenuTabForm
	{
		private DMSOrchestrator dmsOrchestrator = null;
		private NotificationManager notificationManager = null;
		private BBNotificationModule bbNotificationModule = null;

		private bool ReceivedMessageFromBB = false;
		private int TestNotifyTimeOut = 5000;

		MainColorsTheme colors = null;

		public BBSettingsForm(DMSOrchestrator orchestrator)
		{
			InitializeComponent();

			dmsOrchestrator = orchestrator;
			notificationManager = dmsOrchestrator.NotificationManager;
			bbNotificationModule = notificationManager.GetModule<BBNotificationModule>();

			InitControls();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			//mi registro all'evento di ricezione notifica per il test del processo di Brain Business
			bbNotificationModule.TestPerformedSuccessfullyEvent += bbNotificationModule_TestPerformedSuccessfullyEvent;

			this.HandleDestroyed += (sender, args) => 
			{ bbNotificationModule.TestPerformedSuccessfullyEvent -= bbNotificationModule_TestPerformedSuccessfullyEvent; };

			WorkersComboBox.Format += WorkersComboBox_Format;
		}

		void WorkersComboBox_Format(object sender, ListControlConvertEventArgs e)
		{
			//seleziono i campi dei worker da mostrare nella comboBox
			string lastname = ((MWorker)e.ListItem).Name;
			string firstname = ((MWorker)e.ListItem).LastName;
			e.Value = lastname + " " + firstname;
		}

		/// <summary>
		/// metodo richiamato quando vengo notificato dal server
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void bbNotificationModule_TestPerformedSuccessfullyEvent(object sender, EventArgs e)
		{
			ReceivedMessageFromBB = true;
		}

		private void InitControls()
		{
			NotificationManagerUtility.SetFlatStyleFlat(this);
			
			//setting theme colors for controls
			SetDefaultThemeColors();

			PatUrlTxt.Text = bbNotificationModule.BBGetBrainBusinessServiceUrl();

			//carico la lista dei worker nella comboBox della demo delle notifiche
			WorkersComboBox.Items.Clear();
			WorkersComboBox.DataSource = dmsOrchestrator.WorkersTable.Workers;
			WorkersComboBox.ValueMember="WorkerID";

			//UpdateUrlBtn.Text = Strings.BBUpdateUrl;
			//InsertUsersInBrainBtn.Text = Strings.BBAddWorkers;

			//PatUrlLbl.Text = Strings.BBUrlLbl;
			//TestLbl.Text = Strings.BBTestLbl;
			//InsertUsersInBrainLbl.Text = Strings.BBInsertWorkersInBrain;

			
		}

		private void SetDefaultThemeColors()
		{
			if( colors ==null)
				colors= NotificationManagerUtility.GetMainColorsTheme();

			this.BackColor = colors.Background;

			UpdateUrlBtn.BackColor = colors.Primary;
			UpdateUrlBtn.FlatAppearance.MouseOverBackColor = colors.Hover;
			UpdateUrlBtn.ForeColor = colors.Text;
			
			TestBtn.BackColor = colors.Primary;
			TestBtn.FlatAppearance.MouseOverBackColor = colors.Hover;
			TestBtn.ForeColor = colors.Text;

			InsertUsersInBrainBtn.BackColor = colors.Primary;
			InsertUsersInBrainBtn.FlatAppearance.MouseOverBackColor = colors.Hover;
			InsertUsersInBrainBtn.ForeColor = colors.Text;

			SendIGenericNotifyBtn.BackColor = colors.Primary;
			SendIGenericNotifyBtn.FlatAppearance.MouseOverBackColor = colors.Hover;
			SendIGenericNotifyBtn.ForeColor = colors.Text;

			CompletedTestDGV.DefaultCellStyle.SelectionBackColor = colors.Primary;
			CompletedTestDGV.DefaultCellStyle.SelectionForeColor = colors.Text;

			if(colors.IsBackgroundDark())
			{
				DemoNotificaGroupBox.ForeColor = colors.Primary;

				UpdateUrlBtn.MouseEnter += DarkBkgBtn_MouseEnter;
				UpdateUrlBtn.MouseLeave += DarkBkgBtn_MouseLeave;

				TestBtn.MouseEnter += DarkBkgBtn_MouseEnter;
				TestBtn.MouseLeave += DarkBkgBtn_MouseLeave;

				InsertUsersInBrainBtn.MouseEnter += DarkBkgBtn_MouseEnter;
				InsertUsersInBrainBtn.MouseLeave += DarkBkgBtn_MouseLeave;

				SendIGenericNotifyBtn.MouseEnter += DarkBkgBtn_MouseEnter;
				SendIGenericNotifyBtn.MouseLeave += DarkBkgBtn_MouseLeave;
			}
			else
			{
				DemoNotificaGroupBox.ForeColor = colors.Text;
			}
		}

		void DarkBkgBtn_MouseLeave(object sender, EventArgs e)
		{
			Button button = sender as Button;
			button.ForeColor = colors.Text;
		}

		void DarkBkgBtn_MouseEnter(object sender, EventArgs e)
		{
			Button button = sender as Button;
			button.ForeColor =Color.Black ;
		}

		private void InsertUsersInBrainBtn_Click(object sender, System.EventArgs e)
		{
			var sailor = A.Sailor();
			bool lastInsered = false;
			int failed = 0;
			int workersNumber = 0;
			int actualWorkerNumber = 0;
			InsertUsersProgressBar.Value = 0;
			InsertUsersProgressBar.Visible = true;

			sailor
			.When(() =>
			{
				var workers = dmsOrchestrator.WorkersTable.Workers;
				workersNumber = workers.Count;

				foreach(var worker in workers)
					if(worker.Disabled == false)
					{
						lastInsered = dmsOrchestrator.EASync.AddUser(dmsOrchestrator.CompanyID, worker.WorkerID, worker.Name, worker.LastName);
						failed += lastInsered == true ? 0 : 1;
						sailor.Notify(++actualWorkerNumber);
					}
			})
			.Notify((WorkerNumber) =>
			{
				int number = (int)WorkerNumber;
				double percentage = (double)number / workersNumber;
				UpdateProgressbar(InsertUsersProgressBar, Convert.ToInt32(percentage * 100), lastInsered ? ProgressBarStatus.Ok : ProgressBarStatus.Error);
			})
			.Then((obj) =>
			{
				if(failed == 0)
				{
					InsertUsersPictureBox.Image = Properties.Resources.Green;
					notificationManager.SendIGenericNotify(new GenericNotify 
					{ 
						Date = DateTime.Now, 
						FromUserName = "Settings", 
						Title = "Inserimento utenti di mago terminato con successo", 
						Description = "Tutti i worker di questa company ("+ workersNumber +") di Mago sono stati inseriti con successo nel database di BrainBusiness", 
						ToCompanyId = notificationManager.CompanyId, 
						ToWorkerId = notificationManager.WorkerId 
					}, false);
				}
				else
				{
					InsertUsersPictureBox.Image = Properties.Resources.Red;
					notificationManager.SendIGenericNotify(new GenericNotify
					{
						Date = DateTime.Now,
						FromUserName = "Settings",
						Title = "Inserimento utenti di mago terminato con fallimento",
						Description = "Errore durante l'inserimento di " + failed.ToString() + "/" + workersNumber.ToString() + " workers di mago nel database di BrainBusiness",
						ToCompanyId = notificationManager.CompanyId,
						ToWorkerId = notificationManager.WorkerId, 
						ReadDate=DateTime.Now
					}, true);
				}
			})
			.OnError((exc) =>
			{
				InsertUsersPictureBox.Image = Properties.Resources.Red;
				notificationManager.SendIGenericNotify(new GenericNotify
				{
					Date = DateTime.Now,
					FromUserName = "Settings",
					Title = "Inserimento utenti di mago terminato con fallimento",
					Description = "Errore durante l'inserimento dei workers di mago nel database di BrainBusiness \n" + exc.Message,
					ToCompanyId = notificationManager.CompanyId,
					ToWorkerId = notificationManager.WorkerId
				}, true);
			})
			.Finally(() =>
			{
				InsertUsersProgressBar.Visible = false;
			});
		}

		private void UpdateProgressbar(ProgressBar bar, int progressNumber, ProgressBarStatus status)
		{
			if(ControlInvokeRequired(this, () => UpdateProgressbar(bar, progressNumber, status)))
				return;
			bar.Value = progressNumber % 100;
			bar.Visible = bar.Value != 0;
			switch(status)
			{
				case ProgressBarStatus.Ok:
					bar.ForeColor = Color.GreenYellow;
					break;
				case ProgressBarStatus.Error:
					bar.ForeColor = Color.FromArgb(255, 0, 0);
					break;
				case ProgressBarStatus.UnKnown:
					bar.ForeColor = Color.Orange;
					break;
			}
		}

		private void TestBtn_Click(object sender, System.EventArgs e)
		{
			CompletedTestDGV.Rows.Clear();

			bool AtLeastOneFailed = false;
			ReceivedMessageFromBB = false;

			var sailor = A.Sailor();

			sailor
			.When(() =>
			{
				//passo 1 - connessione ad EA
				sailor.Notify(new TestClass
				{
					Description = Strings.BBTestConnectionEA,//"Connessione al Web Service di EasyAttachment",
					Result = dmsOrchestrator.EASync.IsAlive(),
					Percentage = 15
				});

				//passo 2 - connessione al NotificationManager
				sailor.Notify(new TestClass
				{
					Description = Strings.BBTestConnectionNM,//"Connessione al Notification Manager",
					Result = notificationManager.IsConnectedWithNotificationService(),
					Percentage = 30
				});

				//passo 3 - connessione ad XSocket
				if(!notificationManager.IsConnectedWithXSocket())
					notificationManager.SubscribeToTheNotificationServiceSync();
				sailor.Notify(new TestClass
				{
					Description = Strings.BBTestConnectionXSocket,//"Connessione ad XSocket",
					Result = notificationManager.IsConnectedWithXSocket(),
					Percentage = 45
				});

				//passo 4 - connessione a Brain
				sailor.Notify(new TestClass
				{
					Description = Strings.BBTestConnectionBB,//"Connessione al servizio di Brain Business",
					Result = dmsOrchestrator.EASync.BBIsAlive(),
					Percentage = 60
				});

				//passo 5 - lancio del processo di test EA - BB - EA
				sailor.Notify(new TestClass
				{
					Description = Strings.BBTestProcess,//"Lancio del processo di test su Brain Business che contatta EasyAttachmentSynch",
					Result = dmsOrchestrator.EASync.LaunchBBProcessTest(dmsOrchestrator.CompanyID, dmsOrchestrator.WorkerId),
					Percentage = 75
				});

				//passo 7 - ricezione notifiche
				sailor.Notify(new TestClass
				{
					Description = Strings.BBTestNotification,//"Ricezione notifiche dal processo di Brain Business",
					Result = WaitForBBNotify(TestNotifyTimeOut),
					Percentage = 90
				});

				////passo 6 - Disconnessione da XSocket
				//notificationManager.UnSubScribe();
				//sailor.Notify(new TestClass
				//{
				//	Description = "Disconnessione da XSocket",
				//	Result = true,
				//	Percentage = 100
				//});

			})
			.Notify((obj) =>
			{
				var testClass = (TestClass)obj;
				CompletedTestDGV.Rows.Add(testClass.Description, (testClass.Result ? Properties.Resources.Green : Properties.Resources.Red));
				UpdateProgressbar(TestProgressBar, testClass.Percentage, testClass.Result ? ProgressBarStatus.Ok : ProgressBarStatus.Error);
				if(testClass.Result == false)
					AtLeastOneFailed = true;
			})
			.Then((obj) =>
			{
				if(AtLeastOneFailed == false)
				{
					TestPictureBox.Image = Properties.Resources.Green;
				}
				else
				{
					TestPictureBox.Image = Properties.Resources.Red;
				}
			})
			.OnError((exc) =>
			{
				TestPictureBox.Image = Properties.Resources.Red;
			})
			.Finally(() =>
			{
				TestProgressBar.Visible = false;
				ReceivedMessageFromBB = false;
			});
		}

		/// <summary>
		/// Metodo per aspettare che terminino le operazioni per creare il canale di comunicazione con il controller sul NotificationService
		/// (Handshake)
		/// </summary>
		/// <param name="client"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		private bool WaitForBBNotify(int timeout = -1)
		{
			return SpinWait.SpinUntil(() => ReceivedMessageFromBB, timeout);
		}

		/// <summary>
		/// verifico di star eseguendo il codice nel giusto thread. Da fare refactoring e metterne solo una nel manager
		/// </summary>
		/// <param name="c"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public bool ControlInvokeRequired(Control c, System.Action a)
		{
			if(c.InvokeRequired)
				c.Invoke(new MethodInvoker(delegate { a(); }));
			else
				return false;
			return true;
		}

		private void SendIGenericNotifyBtn_Click(object sender, EventArgs e)
		{
			int toWorkerId = (int)WorkersComboBox.SelectedValue;
			
			var notify = new GenericNotify
			{
				ToCompanyId = notificationManager.CompanyId,
				ToWorkerId = toWorkerId,

				FromUserName = dmsOrchestrator.GetWorkerName(notificationManager.WorkerId),
				FromCompanyId = Convert.ToInt16(notificationManager.CompanyId),
				FromWorkerId = Convert.ToInt16(notificationManager.WorkerId),

				Title = TitoloTxtBox.Text,
				Date = System.DateTime.Now,
				Description = DescriptionTxtBox.Text,
				ReadDate=DateTime.MinValue
			};

			try
			{
				notificationManager.SendIGenericNotify(notify, SaveOnDbCheckBox.Checked);
			}
			catch(Exception exc)
			{
				NotificationManagerUtility.SetErrorMessage(Strings.BBSendNotificationError, exc.StackTrace, string.Empty);
			}
		}

		private void UpdateUrlBtn_Click(object sender, EventArgs e)
		{
			bool correctlyUpdatedUrl = bbNotificationModule.UpdateBrainBusinessServiceUrl(PatUrlTxt.Text);
			bool correctlyUpdateinEaSync = dmsOrchestrator.EASync.UpdateBBServiceUrl();
			//da tradurre
			NotificationManagerUtility.SetInfoMessage(
				(correctlyUpdatedUrl ? 
					"Indirizzo aggiornato con successo sul Notification Service" : 
					"Errore nell'aggiornamento dell'indirizzo sul Notification Service" ) + Environment.NewLine +
				(correctlyUpdateinEaSync ? 
					"Indirizzo aggiornato con successo sul web service di EasyAttachment" : 
					"Errore nell'aggiornamento dell'indirizzo sul web service di EasyAttachment"), "Aggiornamento stringa di connessione");
			PatUrlTxt.Text = bbNotificationModule.BBGetBrainBusinessServiceUrl();
		}
	}

	/// <summary>
	/// classe utilizzata unicamente per incapsulare le notifiche dei risultati dei test di connessione e di processo
	/// </summary>
	public class TestClass
	{
		public string Description { get; set; }
		public bool Result { get; set; }
		public int Percentage { get; set; }
	}

	public class ColoredProgressBar : ProgressBar
	{
		

		public ColoredProgressBar()
		{
			this.SetStyle(ControlStyles.UserPaint, true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Brush brush = new SolidBrush(this.ForeColor);
			Rectangle rec = e.ClipRectangle;

			rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
			if(ProgressBarRenderer.IsSupported)
				ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
			rec.Height = rec.Height - 4;
			e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
		}

		//protected override void OnPaint(PaintEventArgs e)
		//{
		//	LinearGradientBrush brush = null;
		//	Rectangle rec = new Rectangle(0, 0, this.Width, this.Height);
		//	double scaleFactor = (((double)Value - (double)Minimum) / ((double)Maximum - (double)Minimum));
		//	if(ProgressBarRenderer.IsSupported)
		//		ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);
		//	rec.Width = (int)((rec.Width * scaleFactor) - 4);
		//	rec.Height -= 4;
		//	brush = new LinearGradientBrush(rec, this.ForeColor, this.BackColor, LinearGradientMode.Vertical);
		//	e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
		//}
	}
}
