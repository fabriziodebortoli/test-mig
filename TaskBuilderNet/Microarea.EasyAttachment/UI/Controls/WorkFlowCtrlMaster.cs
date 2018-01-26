using System;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using SailorsPromises;
using Microarea.TaskBuilderNet.Core.NotificationManager;
using System.Drawing;

namespace Microarea.EasyAttachment.UI.Controls
{
	internal partial class WorkFlowCtrlMaster: UserControl
	{
		//mi serve per inviare l'evento al web service di easyAttachment che inserisce una riga nel db e fa scattare il processo su brain
		private DMSOrchestrator dmsOrchestrator = null;
		private AttachmentInfo ai= null;
		private BBNotificationModule bbNotificationModule = null;

		public WorkFlowCtrlMaster()
		{
			InitializeComponent();
			WFPanel1.Visible = true;
			WFPanel2.Visible = false;

			this.HandleDestroyed += WorkFlowCtrlMaster_HandleDestroyed;
		}

		void WorkFlowCtrlMaster_HandleDestroyed(object sender, EventArgs e)
		{
			bbNotificationModule.RefreshWFControlMaster -= bbNotificationModule_RefreshWFControlMaster;
		}

		/// <summary>
		/// Verifica se è già stata fatta una richiesta con quest'allegato verificando ai==null oppure utilizzando la data di inserimento
		/// vedere se si potrebbe utilizzare lo status (se non l'aho usato avrò avuto i miei motivi, ma ora no ricordo)
		/// </summary>
		/// <returns></returns>
		private Boolean Requested()
		{
			return ((ai == null) ? false : ai.WFRequestDate != DateTime.MinValue);
		}

		/// <summary>
		/// Verifica se un allegato è già stato processato, basandosi sul suo status
		/// </summary>
		/// <returns>true if status= approved || status = notApproved</returns>
		private Boolean Processed()
		{
			return (ai.WFApprovalStatus == 1 || ai.WFApprovalStatus == 2);
		}

		/// <summary>
		/// Inizializza tutti i controls del pannello di del workFlow, quindi quale deve essere abilitato, visibile,...
		/// </summary>
		/// <param name="DmsOrchestrator">il dmsOrchestrator, mi serve per recuperare le informazioni sull'allegato</param>
		/// <param name="Ai"></param>
		public void InitControls(DMSOrchestrator DmsOrchestrator, AttachmentInfo Ai)
		{
			if(DmsOrchestrator == null || Ai == null)
				return;

			if(dmsOrchestrator==null)
			{
				this.dmsOrchestrator = DmsOrchestrator;
			}

			var notificationmanager = DmsOrchestrator.NotificationManager;

			if(bbNotificationModule == null && notificationmanager!=null)
			{
				bbNotificationModule = notificationmanager.GetModule<BBNotificationModule>();
				bbNotificationModule.RefreshWFControlMaster -= bbNotificationModule_RefreshWFControlMaster;
				bbNotificationModule.RefreshWFControlMaster += bbNotificationModule_RefreshWFControlMaster;
			}

			this.ai = Ai;

			FillComponent();
		}

		void bbNotificationModule_RefreshWFControlMaster(object sender, EventArgs e)
		{
			this.FillComponent();
		}

		/// <summary>
		/// Verifica se serve fare una Invoke per modificare l'interfaccia grafica
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

		/// <summary>
		/// Va a riempire i controls del pannello con i valori aggiornati
		/// </summary>
		protected void FillComponent()
		{
			if(ControlInvokeRequired(this, () => FillComponent()))
				return;

			dmsOrchestrator.ArchiveManager.LoadWfInfo(ref ai);

			RequestCommentsTxt.Text = ai.WFRequestComment;
			RequestDateTxt.Text = string.Empty;//todo verificare se serve
			MsgLabel.Text = string.Empty;
			
			if(Requested())
			{
				WFPanel2.Visible = true;
				SendRequestButton.Enabled = false;
				RequestCommentsTxt.Enabled = false;
				StatusTxt.Text = ApprovalStatusToString(ai.WFApprovalStatus);
				SetStatusBar(ai.WFApprovalStatus);
				RequestDateTxt.Text = ai.WFRequestDate.ToString();
				if(Processed())
				{
					WFPanel3.Visible = true;
					ApproverNameTxt.Text = dmsOrchestrator.GetWorkerName(ai.WFApproverId.ToString());
					ApprovalDateTxt.Text = ai.WFApprovalDate.ToString();
					ApprovalCommentsTxt.Text = ai.WFApprovalComment;
				}
				else
					WFPanel3.Visible = false;
			}
			else
			{
				WFPanel2.Visible = false;
				SendRequestButton.Enabled = true;
				RequestCommentsTxt.Enabled = true;
			}
		}

		/// <summary>
		/// aggiorna la status bar per mostrare lo status di un documento in maniera più immediata
		/// </summary>
		/// <param name="approvalStatus"></param>
		private void SetStatusBar(int approvalStatus)
		{
			if(approvalStatus < 0 || approvalStatus > 3)
				return;
			switch (approvalStatus)
			{
				case 0:
					//not approved
					StatusProgressBar.ForeColor = Color.Orange;
					StatusProgressBar.Value = 50;
					StatusProgressBarTooltip.SetToolTip(StatusProgressBar, ApprovalStatusToString(approvalStatus));
					break;
				case 1:
					//approved
					StatusProgressBar.ForeColor = Color.GreenYellow;
					StatusProgressBar.Value = 100;
					StatusProgressBarTooltip.SetToolTip(StatusProgressBar, ApprovalStatusToString(approvalStatus));
					break;
				case 2:
					//rejected
					StatusProgressBar.ForeColor = Color.FromArgb(255, 0, 0);
					StatusProgressBar.Value = 100;

					StatusProgressBarTooltip.SetToolTip(StatusProgressBar, ApprovalStatusToString(approvalStatus));
					break;
				default:
					//undefined
					StatusProgressBar.Value = 0;
					StatusProgressBarTooltip.SetToolTip(StatusProgressBar, ApprovalStatusToString(approvalStatus));
					break;
			}
		}

		/// <summary>
		/// Invia una richiesta di approvazione a BrainBusiness
		/// Potrebbe utilizzare il guid restituito per vedere se la chiamata è andata a buon fine
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SendRequestButton_Click(object sender, EventArgs e)
		{
			SendRequestButton.Enabled = false;
			bool success=false;
			A.Sailor()
			.When(() =>
			{
				success = dmsOrchestrator.EASync.NewAttachmentById(ai.AttachmentId, dmsOrchestrator.CompanyID, dmsOrchestrator.WorkerId, RequestCommentsTxt.Text);
			})
			.OnError((ex) =>
			{
				SetMessage(Strings.BBSendApprovalRequestError, NotificationMessageType.Error);
			})
			.Finally(() =>
			{
				if(success)
				{
					FillComponent();
					SetMessage(Strings.BBSendApprovalRequestOk, NotificationMessageType.Ok);
				}
				else { 
					SendRequestButton.Enabled = true;
					SetMessage(Strings.BBSendApprovalRequestError, NotificationMessageType.Error);
				}
			});
		}

		/// <summary>
		/// Mostra il messaggio nella notificationToolbar nel colore appropriato
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type">Error, Ok o Info</param>
		public void SetMessage(string message, NotificationMessageType type)
		{
			if(ControlInvokeRequired(this, () => SetMessage(message, type)))
				return;

			MsgLabel.Visible = true;

			switch(type)
			{
				case NotificationMessageType.Error:
					MsgLabel.ForeColor = Color.Red;
					break;
				case NotificationMessageType.Ok:
					MsgLabel.ForeColor = Color.Green;
					break;
				case NotificationMessageType.Info:
					MsgLabel.ForeColor = Color.DarkGray;
					break;
				case NotificationMessageType.MileStone:
					MsgLabel.ForeColor = Color.Blue;
					break;
			}

			MsgLabel.Text = message;
			this.Refresh();
		}

		/// <summary>
		/// Converte lo status da intero a stringa user-friendly
		/// </summary>
		/// <param name="status">un intero nel range 0-3</param>
		/// <returns>lo status di un allegato</returns>
		public static string ApprovalStatusToString(int status)
		{
			string approvalStatus = string.Empty;
			switch(status)
			{
				case 0:
					approvalStatus = Strings.BBApprovalStatus0Waiting;
					break;
				case 1:
					approvalStatus = Strings.BBApprovalStatus1Approved;
					break;
				case 2:
					approvalStatus = Strings.BBApprovalStatus2Rejected;
					break;
				default:
					approvalStatus = Strings.BBApprovalStatus3Undefined;
					break;
			}
			return approvalStatus;
		}

	}
}
