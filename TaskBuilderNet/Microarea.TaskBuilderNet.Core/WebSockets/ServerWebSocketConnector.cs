using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Text;
using System.Net.WebSockets;
using Fleck;
using Newtonsoft.Json.Linq;

namespace Microarea.TaskBuilderNet.Core.WebSockets
{
	public delegate void SocketMessageHandler(string socketName, string message);
        
	public class ServerWebSocketConnector
	{
        public static event SocketMessageHandler SocketMessage;
        class TBWebSocket
		{
			public Fleck.IWebSocketConnection Socket { get; set; }
			public String Name { get; set; }

			public void OnClose()
			{
				clients.Remove(this);
			}
			public void OnOpen()
			{
				clients.Add(this);
			}
			public void OnMessage(string message)
			{
				const string varName = "SetWebSocketName:";
                if (message.StartsWith(varName))
                {
                    Name = message.Substring(varName.Length);
                }
                else
                {
                    if (SocketMessage != null)
                        SocketMessage(Name, message);
                }
			}

            public void SendCommand(string message)
            {
                Socket.Send(message);
            }
        }
		const string ControllerPathToken = "/TBWebSocketsController/";
		static ArrayList clients = ArrayList.Synchronized(new ArrayList());
		static string ControllerUrl;
		public static int Port;
		private static WebSocketServer websocketServer;

		/// <summary>
		/// Starts the XSocket engine
		/// </summary>
		public static void Start()
		{
			lock (typeof(ServerWebSocketConnector))
			{
				//needs to obtain a free port without conflicting with other concurrent processes (i.e. terminal server)
				//so I use a mutex
				string mutexName = "Global\\WebSocket" + BasePathFinder.BasePathFinderInstance.Installation;
				bool mutexWasCreated;
				Mutex menuMutex = new Mutex(true, mutexName, out mutexWasCreated);
				try
				{
					// If this thread does not own the named mutex, it requests it by calling WaitOne.
					if (!mutexWasCreated)
						menuMutex.WaitOne(Timeout.Infinite, false);
					// StartupLog
					//mutex has to have no security, so it can be shared by all sessions of terminal services
					Functions.SetNoSecurityOnMutex(menuMutex);

					Port = TbLoaderClientInterface.GetNewTbLoaderPort(4502);
					ControllerUrl = string.Concat("ws://127.0.0.1:", Port, ControllerPathToken);
					websocketServer = new Fleck.WebSocketServer(ControllerUrl);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
				finally
				{
					if (menuMutex != null)
						menuMutex.ReleaseMutex();
				}
			}

            try
            {
                websocketServer.Start(new Action<IWebSocketConnection>(OnAcceptClient));
            }
            catch (SocketException exc)
            {
                Debug.Fail(exc.Message);
            }
        }
		private static void OnAcceptClient(IWebSocketConnection socket)
		{
			TBWebSocket tbWebSocket = new TBWebSocket { Socket = socket };
			socket.OnOpen = new Action(tbWebSocket.OnOpen);
			socket.OnClose = new Action(tbWebSocket.OnClose);
			socket.OnMessage = new Action<string>(tbWebSocket.OnMessage);
		}
		/// <summary>
		/// Stop Xsocket engine.
		/// </summary>
		public static void Stop()
		{
			lock (typeof(ServerWebSocketConnector))
			{
				if (websocketServer != null)
				{
					websocketServer.Dispose();
					websocketServer = null;
				}
			}
		}

		/// <summary>
		/// Push events to proper subscribers acting as an Xsocket client.
		/// </summary>
		/// <param name="sClientId">The ID of the chent the event refers to.</param>
		/// <param name="sContent">The event content.</param>
		public static void PushToClients(String sClientId, String sMessage)
		{

			foreach (TBWebSocket tbws in clients.ToArray())
			{
				try
				{
					if (sClientId.IsNullOrEmpty() || sClientId == tbws.Name)
					{
						if (tbws.Socket.IsAvailable)
						{
							tbws.SendCommand(sMessage);
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		}

       
    }

}
