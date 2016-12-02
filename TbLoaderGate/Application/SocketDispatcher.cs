
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TBLoaderGate
{
    class WebSocketCouple
    {
        public WebSocket clientSocket;
        public WebSocket serverSocket;

        internal WebSocket GetOther(WebSocket webSocket)
        {
            return (clientSocket == webSocket) ? serverSocket : clientSocket;
        }
    }
    public class SocketDispatcher
    {
        const string setClientWebSocketName = "SetClientWebSocketName:";
        const string setServerWebSocketName = "SetServerWebSocketName:";
        static Dictionary<string, WebSocketCouple> socketMap = new Dictionary<string, WebSocketCouple>();
        public static async Task HandleAsync(ISession session, WebSocket webSocket)
        {
            string coupleName = "";

            while (webSocket.State == WebSocketState.Open)
            {
                var token = CancellationToken.None;
                var buffer = new ArraySegment<Byte>(new Byte[4096]);
                var received = await webSocket.ReceiveAsync(buffer, token);

                switch (received.MessageType)
                {
                    case WebSocketMessageType.Text:
                        {
                            var message = Encoding.UTF8.GetString(buffer.Array,
                                                    buffer.Offset,
                                                    buffer.Count);
                            //se il messaggio imposta il nome del socket, metto da parte l'istanza per accoppiarla con la controparte
                            if (message.StartsWith(setClientWebSocketName))
                            {
                                coupleName = message.Substring(setClientWebSocketName.Length);
                                WebSocketCouple couple = GetWebCouple(coupleName);
                                couple.clientSocket = webSocket;
                                TBLoaderInstance tbLoader = TBLoaderEngine.GetTbLoader(session, true);
                                tbLoader.RequireWebSocketConnection(coupleName);
                            }
                            else if (message.StartsWith(setServerWebSocketName))
                            {
                                coupleName = message.Substring(setServerWebSocketName.Length);
                                WebSocketCouple couple = GetWebCouple(coupleName);
                                couple.serverSocket = webSocket;
                            }
                            else
                            {
                                //altrimenti cerco il socket della controparte (by name) e gli mando una copia di quanto ricevuto
                                WebSocketCouple couple = null;
                                if (socketMap.TryGetValue(coupleName, out couple))
                                {
                                    WebSocket otherSocket = couple.GetOther(webSocket);
                                    await otherSocket.SendAsync(buffer, WebSocketMessageType.Text, true, token);
                                }

                            }
                            break;
                        }
                }
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