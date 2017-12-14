using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Newtonsoft.Json;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NotificationManager
{
	public class MessagesManager
	{

		//---------------------------------------------------------------------
		public static string GetNewMessages(string authenticationToken)
		{
			//Chiedo a loginManager se ci sono nuovi messaggi per questo utente
			IAdvertisement[] newMessages = null;
			LoginManager loginManager = new LoginManager();
			try
			{
				if (loginManager != null)
					newMessages = loginManager.GetMessages(authenticationToken);
			}
			catch
			{
				//Non do errore per non bloccare l'esecuzione di MenuManager
			}
			newMessages = FilterExpiredMessages(newMessages);

			if (newMessages.Count() <= 0)
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);

			jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
			jsonWriter.WriteStartObject();

			jsonWriter.WritePropertyName("Notifications");
			
			jsonWriter.WriteStartArray();

			foreach (Advertisement current in newMessages)
			{
				jsonWriter.WriteStartObject();

				jsonWriter.WritePropertyName("messageBody");
				jsonWriter.WriteValue(current.Body.Html);

				jsonWriter.WritePropertyName("messageLink");
				jsonWriter.WriteValue(current.Body.Link);

				jsonWriter.WritePropertyName("messageText");
				jsonWriter.WriteValue(current.Body.Text);

				jsonWriter.WritePropertyName("messageExpiryDate");
				jsonWriter.WriteValue(current.ExpiryDate);

				jsonWriter.WritePropertyName("messageCreationDate");
				jsonWriter.WriteValue(current.CreationDate);

				jsonWriter.WriteEndObject();
			
			}
			//--------modificato da andrea per test----------------------------------------------------------------------------------

			//try
			//{
			//	var notificationManager = new NotificationManager(1, 1, false);

			//	var allNotifications = notificationManager.GetAllNotifications();

			//	foreach(var notify in allNotifications ?? new IGenericNotify[0])
			//	{
			//		jsonWriter.WriteStartObject();
			//		jsonWriter.WritePropertyName("messageBody");
			//		jsonWriter.WriteValue("<a href= \"file:///C:/Users/dossi/Pictures/Icons/\">" + notify.Title + "</a>");

			//		jsonWriter.WritePropertyName("messageLink");
			//		jsonWriter.WriteValue("");

			//		jsonWriter.WritePropertyName("messageText");
			//		jsonWriter.WriteValue(notify.Description);

			//		jsonWriter.WritePropertyName("messageExpiryDate");
			//		jsonWriter.WriteValue(notify.Date.AddDays(1).ToString());

			//		jsonWriter.WritePropertyName("messageCreationDate");
			//		jsonWriter.WriteValue(notify.Date.ToString());

			//		jsonWriter.WriteEndObject();
			//	}
			//}
			//catch(Exception e )
			//{
			//	NotificationManagerUtility.SetMessage(e.Message, e.StackTrace, "Errore durante la richiesta delle notifiche al Notification Manager");
			//}

			//var genericNotify= new GenericNotify{Date= DateTime.Now, Description= "Description", Title= "Title", FromUserName="newMessages", NotificationType= NotificationType.Generic};
			
			//jsonWriter.WriteStartObject();
			//jsonWriter.WritePropertyName("messageBody");
			//jsonWriter.WriteValue("<a href= \"file:///C:/Users/dossi/Pictures/Icons/\">" + genericNotify.Title + "</a>");

			//jsonWriter.WritePropertyName("messageLink");
			//jsonWriter.WriteValue("");

			//jsonWriter.WritePropertyName("messageText");
			//jsonWriter.WriteValue(genericNotify.Description);

			//jsonWriter.WritePropertyName("messageExpiryDate");
			//jsonWriter.WriteValue(genericNotify.Date.AddDays(1).ToString());

			//jsonWriter.WritePropertyName("messageCreationDate");
			//jsonWriter.WriteValue(genericNotify.Date.ToString());

			//jsonWriter.WriteEndObject();

			//-----------------------------------------------------------------------------------------------------------------------

			jsonWriter.WriteEndArray();


			jsonWriter.WriteEndObject();

			string output = sw.ToString();

			jsonWriter.Close();
			sw.Close();

			return output;
		}


		//--------------------------------------------------------------------------------------------------------------------------------
		private static Advertisement[] FilterExpiredMessages(IAdvertisement[] messages)
		{
			List<Advertisement> validMessages = new List<Advertisement>();
			if (messages == null || messages.Length == 0)
				return validMessages.ToArray();

			foreach (Advertisement adv in messages)
				if (adv != null && !adv.Expired)//scarto i messaggi scaduti
					validMessages.Add(adv);
			return validMessages.ToArray();
		}
	}
}
