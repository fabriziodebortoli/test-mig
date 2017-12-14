using Microarea.TaskBuilderNet.Interfaces;

namespace NotificationService
{
  //  public class NotificationController : XSocketController
  //  {
  //      /// <summary>
  //      /// l'identificatore univoco del worker
  //      /// </summary>
  //      public string BBUserName { get; set; }

  //      public NotificationController() 
  //      {
  //          this.OnOpen  += new System.EventHandler<XSockets.Core.Common.Socket.Event.Arguments.OnClientConnectArgs>(NotificationController_OnOpen);
  //          this.OnClose += new System.EventHandler<XSockets.Core.Common.Socket.Event.Arguments.OnClientDisconnectArgs>(NotificationController_OnClose);
  //      }
        
  //      void NotificationController_OnOpen(object sender, XSockets.Core.Common.Socket.Event.Arguments.OnClientConnectArgs e)
  //      {
            
  //      }

  //      void NotificationController_OnClose(object sender, XSockets.Core.Common.Socket.Event.Arguments.OnClientDisconnectArgs e)
  //      {
            
  //      }

  //      public void SendForm(string BBUserName, int formId) 
  //      {
  //          this.SendTo(client => client.BBUserName == BBUserName, formId, "FormNotify");
  //      }

  //      public void SendMileStone(string BBUserName, string mileStoneMessage)
  //      {
  //          this.SendTo(client => client.BBUserName == BBUserName, mileStoneMessage, "MileStoneNotify");
  //      }

		//public void SendMessage(string BBUserName, string message)
		//{
		//	this.SendTo(client => client.BBUserName == BBUserName, message, "Message");
		//}

		////public void SendIGenericNotify(string BBUserName, string jSonNotify)
		////{
		////	this.SendTo(client => client.BBUserName == BBUserName, jSonNotify, "IGenericNotify");
		////}

		//public void SendIGenericNotify(string BBUserName, GenericNotify notify)
		//{
		//	this.SendTo(client => client.BBUserName == BBUserName, notify, "IGenericNotify");
		//}
  //  }
}
