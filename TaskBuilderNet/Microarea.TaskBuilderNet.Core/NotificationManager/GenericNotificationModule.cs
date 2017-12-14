using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Runtime.Serialization;

namespace Microarea.TaskBuilderNet.Core.NotificationManager
{
	[Serializable]
	[KnownType(typeof(NotificationType))]
	public class GenericNotifyExtended : GenericNotify
	{
		[NonSerialized]
		private GenericNotificationModule GenericNotificationModule;

		public GenericNotifyExtended(GenericNotificationModule genericNotificationModule)
		{
			GenericNotificationModule = genericNotificationModule;
			NotificationType = NotificationType.Generic;
		}

		public override void OnClickAction()
		{
			GenericNotificationModule.ShowGenericNotify(this, false);
		}

	}


	public class GenericNotificationModule:BaseNotificationModule
	{
		public GenericNotificationModule(NotificationServiceWrapper notificationServiceWrapper, bool isViewer)
			: base(notificationServiceWrapper, isViewer)
		{
			//notificationServiceWrapper.IGenericNotify += notificationServiceWrapper_IGenericNotify;
		}

		private void notificationServiceWrapper_IGenericNotify(object sender, IGenericNotifyEventArgs e)
		{
			//innanzi tutto sparo la notifica generica del modulo base al notification manager
			RaiseBaseModuleEventHandler();
			//se sono il viewer, mostro la form all'utente
			if(this.isViewer)
				ShowGenericNotify(e.GenericNotify, true);
		}

		public override IList<Interfaces.IGenericNotify> GetNotifications()
		{
			//var notifications = NotificationServiceWrapper.GetAllIGenericNotify(NotificationServiceWrapper.WorkerId, NotificationServiceWrapper.CompanyId, true);
			//return notifications == null ? new List<IGenericNotify>() : notifications.ToList<IGenericNotify>();
			var notifications = NotificationServiceWrapper.GetAllIGenericNotify(NotificationServiceWrapper.WorkerId, NotificationServiceWrapper.CompanyId, false).Select(notify => new GenericNotifyExtended(this) 
			{ 
				Date=notify.Date,
 				Description=notify.Description,
				FromCompanyId= notify.FromCompanyId,
				FromWorkerId=notify.FromWorkerId,
				FromUserName=notify.FromUserName,
				ReadDate=notify.ReadDate,
				Title=notify.Title,
				ToCompanyId=notify.ToCompanyId,
				ToWorkerId=notify.ToWorkerId,
				NotificationId=notify.NotificationId,
				StoredOnDb = notify.StoredOnDb
			});
			return notifications == null ? new List<IGenericNotify>() : notifications.ToList<IGenericNotify>();
		}


		/// <summary>
		/// Visualizza una form contenente le informazioni contenute all'interno di una notifica generica
		/// dopo 10 secondi si chiude
		/// </summary>
		/// <param name="message">il contenuto della milestone</param>
		public void ShowGenericNotify(GenericNotify notify, bool autoClose=true)
		{
			Form activeForm = Form.ActiveForm;

			if(activeForm != null && activeForm.InvokeRequired)
			{
				activeForm.Invoke(new Action(() => ShowGenericNotify(notify)));
				return;
			}

			//--------------------------------------------------------content Container panel
			//content containerPanel properties
			var contentContainerPanel = new FlowLayoutPanel();
			contentContainerPanel.FlowDirection = FlowDirection.TopDown;
			contentContainerPanel.AutoSize = true;
			contentContainerPanel.AutoSizeMode = AutoSizeMode.GrowOnly;

			//--------------------------------------------------------create scheleton form
			var form = CreateCompleteNotificationForm(contentContainerPanel, notify.Title, autoClose? 10 : 0);

			//--------------------------------------------------------content panel
			//content panel properties
			var contentPanel = new FlowLayoutPanel();
			contentPanel.Font = contentContainerPanel.Font;
			
			contentPanel.AutoSize = true;
			contentPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			contentPanel.AutoScroll = true;

			var screenTest = Screen.FromPoint(contentPanel.Location);
			contentPanel.MaximumSize = new Size(250, screenTest.WorkingArea.Height - 100);
			
			contentPanel.Width = 200;
			Size maximumLabelSize = new Size(contentPanel.Width, screenTest.WorkingArea.Height - 100);
			Size minimumLabelSize = new Size(contentPanel.Width, 0);
			//type
			var typeLabel = new Label();
			typeLabel.Font = contentContainerPanel.Font;
			typeLabel.Text = NotificationManagerStrings.Type + ": " + notify.NotificationType.ToString();
			typeLabel.AutoSize = true;
			typeLabel.MinimumSize = minimumLabelSize;
			typeLabel.MaximumSize = maximumLabelSize;
			contentPanel.Controls.Add(typeLabel);
			//sender
			var senderLabel = new Label();
			senderLabel.Font = contentContainerPanel.Font;
			senderLabel.Text = NotificationManagerStrings.Sender + ": " + notify.FromUserName + " (" + notify.FromCompanyId + ", " + notify.FromWorkerId + ")";
			senderLabel.AutoSize = true;
			senderLabel.MinimumSize = minimumLabelSize;
			senderLabel.MaximumSize = maximumLabelSize;
			contentPanel.Controls.Add(senderLabel);
			//date
			var dateLabel = new Label();
			dateLabel.Font = contentContainerPanel.Font;
			dateLabel.Text = NotificationManagerStrings.Date + ": " + notify.Date;
			dateLabel.AutoSize = true;
			dateLabel.MinimumSize = minimumLabelSize;
			dateLabel.MaximumSize = maximumLabelSize;
			contentPanel.Controls.Add(dateLabel);
			//description
			var descriptionLabel = new Label();
			descriptionLabel.Font = contentContainerPanel.Font;
			descriptionLabel.Text = NotificationManagerStrings.Description + ": " + notify.Description;
			descriptionLabel.AutoSize = true;
			descriptionLabel.MinimumSize = minimumLabelSize;
			descriptionLabel.MaximumSize = maximumLabelSize;
			contentPanel.Controls.Add(descriptionLabel);

			//se la dimensione 
			if(contentPanel.PreferredSize.Height >= /*screenTest.WorkingArea.Height - 100*/ contentPanel.MaximumSize.Height)
			{
				int vsbw = SystemInformation.VerticalScrollBarWidth;
				contentPanel.Padding = new Padding(0, 0, vsbw, 0);
			}


			//--------------------------------------------------------content buttons panel
			//content buttons panel properties
			var contentButtonsPanel = new FlowLayoutPanel();
			contentContainerPanel.Font = contentContainerPanel.Font;
			contentButtonsPanel.FlowDirection = FlowDirection.RightToLeft;
			contentButtonsPanel.AutoSize = true;
			contentButtonsPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			contentButtonsPanel.Dock = DockStyle.Bottom;

			if(!notify.IsRead() && notify.StoredOnDb)
			{
				//Close button
				Button SetAsReadBtn = new Button();
				SetAsReadBtn.Font = contentContainerPanel.Font;
				SetAsReadBtn.Text = NotificationManagerStrings.MarkAsRead;
				using(Graphics cg = form.CreateGraphics())
				{
					SizeF size = cg.MeasureString(SetAsReadBtn.Text, SetAsReadBtn.Font);
					SetAsReadBtn.Width = (int)size.Width + 20;
				}
				//click event
				SetAsReadBtn.Click += (sender, args) =>
				{
					bool setAsReadWithSuccess = NotificationServiceWrapper.SetNotificationAsRead(notify.NotificationId);
					if (setAsReadWithSuccess)
					{
						RaiseBaseModuleEventHandler();
						form.Close();
					}
				};
				//adding the button
				contentButtonsPanel.Controls.Add(SetAsReadBtn); 
			}

			//Close button
			Button CloseBtn = new Button();
			CloseBtn.Font = contentContainerPanel.Font;
			CloseBtn.Text = NotificationManagerStrings.Close;
			//click event
			CloseBtn.Click += (sender, args) =>
			{
				form.Close();
			};
			//adding the button
			contentButtonsPanel.Controls.Add(CloseBtn);

			//adding the content and button panels in the container
			contentContainerPanel.Controls.Add(contentPanel);
			contentContainerPanel.Controls.Add(contentButtonsPanel);

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
	}
}
