
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json.Linq;

namespace Microarea.TbLoaderGate
{
	class WebSocketCouple
	{
		public WebSocket clientSocket;
		public WebSocket serverSocket;

		internal WebSocket GetOther(WebSocket webSocket)
		{
			return (clientSocket == webSocket) ? serverSocket : clientSocket;
		}

		internal void Remove(WebSocket webSocket)
		{
			if (clientSocket == webSocket) clientSocket = null;
			else if (serverSocket == webSocket) serverSocket = null;
		}
	}
	public class SocketDispatcher
	{
		public static async Task Listen(HttpContext http, Func<Task> next)
		{
			if (!await HandleAsync(http))
			{
				await next();
			}
		}

		const string setClientWebSocketName = "SetClientWebSocketName";
		const string setServerWebSocketName = "SetServerWebSocketName";
		static Dictionary<string, WebSocketCouple> socketMap = new Dictionary<string, WebSocketCouple>();

		public static async Task<bool> HandleAsync(HttpContext http)
		{
			if (!http.WebSockets.IsWebSocketRequest || !http.Request.Path.StartsWithSegments("/tbloader"))
				return false;

			var webSocket = await http.WebSockets.AcceptWebSocketAsync();
			string coupleName = "";
			try
			{
				while (webSocket.State == WebSocketState.Open)
				{
					var token = CancellationToken.None;
					int segSize = 4096;
					int offset = 0;
					int totalBytes = 0;
					Byte[] totalBuffer = new Byte[segSize];
					var received = await webSocket.ReceiveAsync(new ArraySegment<Byte>(totalBuffer, offset, segSize), token);
					totalBytes += received.Count;
					while (!received.EndOfMessage)
					{
						offset = totalBuffer.Length;
						Array.Resize(ref totalBuffer, offset + segSize);
						received = await webSocket.ReceiveAsync(new ArraySegment<Byte>(totalBuffer, offset, segSize), token);
						totalBytes += received.Count;
					}
					switch (received.MessageType)
					{
						case WebSocketMessageType.Text:
							{
								var message = Encoding.UTF8.GetString(totalBuffer,
														0,
														totalBuffer.Length);

								WebSocketCouple couple = null;
								//se il messaggio imposta il nome del socket, metto da parte l'istanza per accoppiarla con la controparte
								JObject jObj = null;
								try
								{
									jObj = JObject.Parse(message);
								}
								catch (Exception ex)
								{
									//TODO gestione errore
									continue;
								}
								string cmd = jObj["cmd"].ToString();
								if (cmd == setClientWebSocketName)
								{
									JObject jArgs = (JObject)jObj["args"];
									coupleName = jArgs["webSocketName"].ToString();
									couple = GetWebCouple(coupleName);
									couple.clientSocket = webSocket;
									string tbName = jArgs["tbLoaderName"].ToString();
									bool dummy;
									TBLoaderInstance tb = TBLoaderEngine.GetTbLoader(tbName, false, out dummy);
									if (tb != null)
										tb.RequireWebSocketConnection(coupleName, http.Request.Host);
								}
								else if (cmd == setServerWebSocketName)
								{
									JObject jArgs = (JObject)jObj["args"];
									coupleName = jArgs["webSocketName"].ToString();
									couple = GetWebCouple(coupleName);
									couple.serverSocket = webSocket;
								}
								else
								{
									//altrimenti cerco il socket della controparte (by name) e gli mando una copia di quanto ricevuto
									couple = GetWebCouple(coupleName);
								}
								if (couple != null)
								{
									WebSocket otherSocket = couple.GetOther(webSocket);
									if (otherSocket != null)
									{
										await otherSocket.SendAsync(new ArraySegment<Byte>(totalBuffer, 0, totalBytes), WebSocketMessageType.Text, true, token);
									}
								}
								break;
							}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			DisconnectOtherSocket(coupleName, webSocket);
			return true;

		}

		private static void DisconnectOtherSocket(string coupleName, WebSocket webSocket)
		{
			WebSocketCouple couple = GetWebCouple(coupleName);
			WebSocket otherSocket = couple.GetOther(webSocket);
			couple.Remove(webSocket);
			if (otherSocket != null)
				otherSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Twin socket disconnected", CancellationToken.None);
			else
				RemoveWebCopule(coupleName);
		}

		private static void RemoveWebCopule(string coupleName)
		{
			lock (typeof(SocketDispatcher))
			{
				socketMap.Remove(coupleName);
			}
		}

		private static WebSocketCouple GetWebCouple(string coupleName)
		{
			lock (typeof(SocketDispatcher))
			{
				WebSocketCouple couple = null;
				if (!socketMap.TryGetValue(coupleName, out couple))
				{
					couple = new WebSocketCouple();
					socketMap[coupleName] = couple;
				}
				return couple;
			}
		}
	}
}