
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TBLoaderGate
{
    public class MyWebSocketManager
    {
        internal static async Task Handle(WebSocketManager webSockets)
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
                        var type = WebSocketMessageType.Text;
                        var data = Encoding.UTF8.GetBytes("Echo from server :" + request);
                        buffer = new ArraySegment<Byte>(data);
                        await webSocket.SendAsync(buffer, type, true, token);
                        break;
                }
            }
        }
    }
}