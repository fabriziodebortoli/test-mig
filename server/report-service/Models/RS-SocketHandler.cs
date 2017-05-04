﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;

using Microarea.Common.Generic;
using Microarea.Common.Applications;

using Microarea.RSWeb.Render;

namespace Microarea.RSWeb.Models
{
    /// <summary>
    /// Handle socket connections and messages reception
    /// </summary>
    public class RSSocketHandler
    {
        private JsonReportEngine CreateEngine(NamespaceMessage nsMsg, WebSocket webSocket)
        {
            if (nsMsg == null)
                return null;

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(nsMsg.authtoken);
            if (loginInfo == null)
                return null;
 
            UserInfo ui = new UserInfo(loginInfo, nsMsg.authtoken);

            TbReportSession session = new TbReportSession(ui, nsMsg.nameSpace, nsMsg.parameters);
            session.WebSocket = webSocket;

            JsonReportEngine engine = new JsonReportEngine(session);

            engine.Execute();

            return engine;
        }

        /// <summary>
        /// sends messages to the client connected via socket
        /// </summary>
        /// <param name="message"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public async Task SendMessage(string message, WebSocket webSocket)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<Byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);

            }
            else
            { 
                // disconnect
            }
        }

        /// <summary>
        /// dead loop 
        /// waiting for new socket connections
        /// </summary>
        /// <param name="http"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task Listen(HttpContext http, Func<Task> next)
        {
            if (http.WebSockets.IsWebSocketRequest && http.Request.Path.StartsWithSegments("/rs"))
            {
                /// accept connection               
                var webSocket = await http.WebSockets.AcceptWebSocketAsync();

                /// sends OK message
                await SendMessage(MessageBuilder.GetJSONMessage(MessageBuilder.CommandType.OK, string.Empty), webSocket);

                /// waits for the namespace
                var nsBuffer = new ArraySegment<Byte>(new Byte[4096]);
                var recNsBuffer = await webSocket.ReceiveAsync(nsBuffer, CancellationToken.None);

                string msgNs = Encoding.UTF8.GetString(nsBuffer.Array, nsBuffer.Offset, nsBuffer.Count).Replace("\0", "");

                /// creates states machine associated with pipe  
                JsonReportEngine jengine = CreateEngine(JsonConvert.DeserializeObject<NamespaceMessage>(msgNs), webSocket);

                if (jengine == null)
                {    /// handle errors
                    /// if guid is not found on server the web socket will be closed and disposed
                   await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, string.Format(ErrorHandler.NsNotValid, JsonConvert.DeserializeObject<Message>(msgNs).message), CancellationToken.None);
                   webSocket.Dispose();
                }
                
                /// active, waiting for new messages
                /// dead loop
                while (webSocket.State == WebSocketState.Open && jengine!=null)
                {
                    var buffer = new ArraySegment<Byte>(new Byte[4096]);
                    var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    switch (received.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            {
                                webSocket.Dispose();
                                break;
                            }
                        case WebSocketMessageType.Text:
                            {
                                var recMeg = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count).Replace("\0", "");
                                //parse
                                Message msg = JsonConvert.DeserializeObject<Message>(recMeg);

                                msg = jengine.GetResponseFor(msg);

                                await SendMessage(MessageBuilder.GetJSONMessage(msg),webSocket);
                                //send
                                break;
                            }
                        default: break;  
                    }
                }
            }
            else
            {
                await next();
            }
        }
    }
}
