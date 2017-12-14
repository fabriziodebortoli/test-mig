using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Microarea.Library.LogManager;
using Microarea.Library.LogManager.Loggers;
using Microarea.Library.LogManager.Loggables;
using Microarea.Library.TBTalkieCommon;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TBTalkieManager
{
    /// <summary>
	/// TBTalkieManager: server gestore delle comunicazioni via chat.
    /// </summary>
    //=========================================================================
    [WebService(Namespace = "http://www.microarea.it/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class TBTalkieManager : System.Web.Services.WebService, ILoggable
    {
		private ILogger loggerService;
         
		public event EventHandler<LoggableEventArgs> Event;

		//---------------------------------------------------------------------
		public TBTalkieManager()
		{
			loggerService = new BaseLogger();
			try
			{
				this.loggerService.AddLogWriter(
					new SystemEventLogLogger("Application", "TBTalkie")
					);
			} 
			catch
			{ }
			loggerService.ListenTo(this);
		}

		//---------------------------------------------------------------------
		protected void OnLoggableEvent(LoggableEventArgs le)
		{
			if (this.Event != null)
				Event(this, le);
		}

        //---------------------------------------------------------------------
        private bool ValidateToken(PacketData aPacketData)
        {
			if (aPacketData == null)
				return false;

            if (Global.IsUserInUsersList(aPacketData.Sender.UserName))
                return true;

			bool isTokenValid = false;

			try
			{
				isTokenValid = Global.AuthProv.IsTokenValid(aPacketData.AuthToken);
			}
			catch (Exception exc/*TODO Vedere quali eccezioni prendere*/)
			{
				ErrorEventArgs eea = new ErrorEventArgs(
						0,
						ErrorGravity.Fatal,
						"Error validating user token against the Authentication Provider" +
						Environment.NewLine + Environment.NewLine +
						exc.ToString()
					);
				OnLoggableEvent(eea);

				isTokenValid = false;
			}

            if (!isTokenValid)
                return false;
                
            Global.AddUserToUsersList(
				aPacketData.Sender.UserName,
                aPacketData.Sender
                );

            return true;
        }
        
        //---------------------------------------------------------------------
        [WebMethod]
        public string ProcessMessage(string aInMsg)
        {
			if (aInMsg == null || aInMsg.Length == 0)
				return string.Empty;

            ISerializationFactory	serFactory			= new SerializationFactory();
            IPacketDeserializer		packetDeserializer	=
				serFactory.GetPacketDeserializer(aInMsg);
            IPacketSerializer		packetSerializer	=
				serFactory.GetPacketSerializer(SerializationFormat.Json);

			PacketData packetData = null;
			try
			{
				packetData = new PacketData(aInMsg, packetDeserializer);
			}
			catch (Exception exc)
			{
				ErrorEventArgs eea = new ErrorEventArgs(
						0,
						ErrorGravity.Medium,
						"Unable to read packet" +
						Environment.NewLine +
						"aInMsg: " + aInMsg + Environment.NewLine +
						"exception: " + exc.ToString()
					);
				OnLoggableEvent(eea);

				return PacketHelper.CreateErrorPacket(
					Message.ServerUserInfo,// Trick! Da valutare che ricevitore inserire se non si riesce a leggere il pacchetto.
					"Unable to read packet",
					packetSerializer
					);
			}

			string serializedPacket = String.Empty;
			if (ValidateToken(packetData))
            {
				PacketHelper.ReadPacket(aInMsg, packetData);

                serializedPacket = PacketHelper.CreateMessagesWithUsersListPacket(
					packetData.Sender,
					packetSerializer
					);
            }
            else
            {
				serializedPacket = PacketHelper.CreateFailPacket(
					packetData.Sender,
					"Invalid authentication token",
					packetSerializer
					);
            }
            
            return serializedPacket;
        }
    }
}
