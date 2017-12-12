using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.NotificationManager
{

	[Serializable]
	[KnownType(typeof(NotificationType))]
	public class ChatNotify : GenericNotify
	{
		[NonSerialized]
		private ChatNotificationModule ChatNotificationModule;

		public ChatNotify(ChatNotificationModule chatNotificationModule)
		{
			ChatNotificationModule = chatNotificationModule;
			NotificationType = NotificationType.Chat;
		}

		
		public override void OnClickAction()
		{
			ChatNotificationModule.ShowChatNotify(this, false);
		}
	}

	public class ChatNotificationModule : BaseNotificationModule
	{

		public ChatNotificationModule(NotificationServiceWrapper notificationServiceWrapper, bool isViewer)
			: base(notificationServiceWrapper, isViewer)
		{
			//notificationServiceWrapper.ChatNotify += notificationServiceWrapper_ChatNotify;
		}

		private void notificationServiceWrapper_ChatNotify(object sender, ChatNotifyEventArgs e)
		{
			//innanzi tutto sparo la notifica generica del modulo base al notification manager
			RaiseBaseModuleEventHandler();
			//se sono il viewer, mostro la form all'utente
			if(this.isViewer)
				ShowChatNotify(e.GenericNotify, true);
		}

		public override IList<Interfaces.IGenericNotify> GetNotifications()
		{
			return new List<IGenericNotify>();
		}

		public void ShowChatNotify(GenericNotify notify, bool autoClose = true)
		{ 

		}
	}
}
