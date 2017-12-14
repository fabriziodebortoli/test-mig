using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Microarea.Library.TBTalkieCommon;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TBTalkieManager
{
	//=========================================================================
	sealed class PacketHelper
	{
		private static object staticLockTicket = new object();

		//---------------------------------------------------------------------
		private PacketHelper()
		{ }

		//---------------------------------------------------------------------
		public static string CreateErrorPacket(
			UserInfo receiver,
			string errorMessage,
			IPacketSerializer packetSerializer
			)
		{
			lock (staticLockTicket)
			{
				if (receiver == null)
					throw new ArgumentNullException("receiver");

				if (packetSerializer == null)
					throw new ArgumentNullException("packetSerializer");

				ICollection<string> parameters = new List<string>();
				ICollection<UserInfo> receivers = new List<UserInfo>();

				if (errorMessage != null && errorMessage.Length > 0)
					parameters.Add(errorMessage);

				receivers.Add(receiver);

				return new PacketData(
					Message.ServerUserInfo,
					receivers,
					TypeMsg.Error,
					parameters,
					Message.ServerUserInfo.AuthToken
					).Serialize(packetSerializer);
			}
		}

		//---------------------------------------------------------------------
		public static string CreateFailPacket(
			UserInfo receiver,
			string failMessage,
			IPacketSerializer packetSerializer
			)
		{
			lock (staticLockTicket)
			{
				if (receiver == null)
					throw new ArgumentNullException("receiver");

				if (packetSerializer == null)
					throw new ArgumentNullException("packetSerializer");

				ICollection<string> parameters = new List<string>();
				ICollection<UserInfo> receivers = new List<UserInfo>();

				if (failMessage != null && failMessage.Length > 0)
					parameters.Add(failMessage);

				receivers.Add(receiver);

				return new PacketData(
					Message.ServerUserInfo,
					receivers,
					TypeMsg.Fail,
					parameters,
					Message.ServerUserInfo.AuthToken
					).Serialize(packetSerializer);
			}
		}

		//---------------------------------------------------------------------
		public static string CreateUsersListPacket(
			UserInfo receiver,
			IPacketSerializer packetSerializer
			)
		{
			lock (staticLockTicket)
			{
				if (receiver == null)
					throw new ArgumentNullException("receiver");

				if (packetSerializer == null)
					throw new ArgumentNullException("packetSerializer");

				ICollection<string> parameters = new List<string>();
				ICollection<UserInfo> receivers = new List<UserInfo>();

			    parameters.Add(Global.UsersListToString());
				receivers.Add(receiver);

				return new PacketData(
					Message.ServerUserInfo,
					receivers,
					TypeMsg.UsersList,
					parameters,
					Message.ServerUserInfo.AuthToken
					).Serialize(packetSerializer);
			}
		}

		//---------------------------------------------------------------------
		public static string CreateMessagesWithUsersListPacket(
			UserInfo receiver,
			IPacketSerializer packetSerializer
			)
		{
			lock (staticLockTicket)
			{
				if (receiver == null)
					throw new ArgumentNullException("receiver");

				if (packetSerializer == null)
					throw new ArgumentNullException("packetSerializer");

				string messagesSerializedPacket = string.Empty;
				string usersListSerializedPacket = CreateUsersListPacket(receiver, packetSerializer);

				if (Global.MessageQueue.ContainsKey(receiver.UserName))
					messagesSerializedPacket = Global.ConsumeMessageFromQueue(receiver.UserName);

				return String.Concat(messagesSerializedPacket, usersListSerializedPacket);
			}
		}

		//---------------------------------------------------------------------
		public static void ReadPacket(
			string serializedPacketData,
			PacketData packetData
			)
		{
			lock (staticLockTicket)
			{
				if (serializedPacketData == null)
					throw new ArgumentNullException("serializedPacketData", "'serializedPacketData' cannot be null");

				if (packetData == null)
					throw new ArgumentNullException("packetData", "'packetData' cannot be null");


				switch (packetData.Type)
				{
					case TypeMsg.Alive:
					{
						Global.AddUserToUsersList(packetData.Sender.UserName, packetData.Sender);
						break;
					}
					case TypeMsg.State:
					{
						IEnumerator<string> enumerator = packetData.Parameters.GetEnumerator();
						enumerator.MoveNext();
						string userState = enumerator.Current;
						(Global.UsersList[packetData.Sender.UserName] as UserInfo).UserState = (UserState)Enum.Parse(typeof(UserState), userState);
						break;
					}
					case TypeMsg.Msg:
					case TypeMsg.QuitConversation:
					{
						ICollection<UserInfo> receivers = packetData.Receivers;
						foreach (UserInfo receiver in receivers)
							Global.PumpMessageToQueue(receiver.UserName, serializedPacketData);

						break;
					}
				}
			}
		}
	}
}
