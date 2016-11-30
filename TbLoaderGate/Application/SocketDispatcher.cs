
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
        WebSocket first;
        WebSocket second;
    }
    public class SocketDispatcher
    {
        Dictionary<string, WebSocketCouple> socketMap = new Dictionary<string, WebSocketCouple>();
        internal async Task HandleAsync(ISession session, WebSocketManager webSockets)
        {
            var webSocket = await webSockets.AcceptWebSocketAsync();
            
            while (webSocket.State == WebSocketState.Open)
            {
                var token = CancellationToken.None;
                var buffer = new ArraySegment<Byte>(new Byte[4096]);
                var received = await webSocket.ReceiveAsync(buffer, token);


                switch (received.MessageType)
                {
                    case WebSocketMessageType.Text:
                        var request = Encoding.UTF8.GetString(buffer.Array,
                                                buffer.Offset,
                                                buffer.Count);

                        if (request.StartsWith("SetWebSocketName:"))
                        {

                        }
                        else
                        {
                            WebSocket otherSocket = null;
                            //await otherSocket.SendAsync(buffer, WebSocketMessageType.Text, true, token);
                        }
                        break;
                }
            }
        }
    }
}