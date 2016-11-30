
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TBLoaderGate
{
    class WebSocketCouple
    {
        private WebSocket first;
        private WebSocket second;

        internal bool Contains(WebSocket webSocket)
        {
            return first == webSocket || second == webSocket;
        }

        internal void Add(WebSocket webSocket)
        {
            if (first == null)
                first = webSocket;
            else if (second == null)
                second = webSocket;
            else
                throw new Exception("Web sockets already assigned");
        }

        internal WebSocket GetOther(WebSocket webSocket)
        {
             return (first == webSocket) ? second : first;
        }
    }
    public class SocketDispatcher
    {
        const string setWebSocketName = "SetWebSocketName:";
        Dictionary<string, WebSocketCouple> socketMap = new Dictionary<string, WebSocketCouple>();
        internal async Task HandleAsync(ISession session, WebSocketManager webSockets)
        {
            var webSocket = await webSockets.AcceptWebSocketAsync();
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
                            if (message.StartsWith(setWebSocketName))
                            {
                                coupleName = message.Substring(setWebSocketName.Length);
                                WebSocketCouple couple = null;
                                if (!socketMap.TryGetValue(coupleName, out couple))
                                {
                                    couple = new WebSocketCouple();
                                    socketMap[coupleName] = couple;
                                }
                                if (!couple.Contains(webSocket))
                                    couple.Add(webSocket);
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
    }