using System;
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
using Microarea.Common;

namespace Microarea.RSWeb.Models
{
    /// <summary>
    /// Handle socket connections and messages reception
    /// </summary>
    public class RSSocketHandler
    {
        private static JsonReportEngine CreateEngine(NamespaceMessage nsMsg, WebSocket webSocket, string tbIstanceID = "")
        {
            if (nsMsg == null || nsMsg.authtoken == null)
                return null;   //TODO  gracefully expiration token message to client 

            LoginInfoMessage loginInfo = LoginInfoMessage.GetLoginInformation(nsMsg.authtoken);
            if (loginInfo == null)
                return null;
 
            UserInfo ui = new UserInfo(loginInfo, nsMsg.authtoken);

            // if ComponentId is received from client, it means this report is called from a tbloader document
            // ComponentId is the handle of woormdoc proxy tbloader side
            TbReportSession session = new TbReportSession(ui, nsMsg);
            session.WebSocket = webSocket;

            if (!string.IsNullOrWhiteSpace(tbIstanceID))
            {
                session.TbInstanceID = tbIstanceID;
                session.LoggedToTb = true;
            }
            Thread.CurrentThread.CurrentUICulture = ui.UserUICulture;
            Thread.CurrentThread.CurrentCulture = ui.UserCulture;

            JsonReportEngine engine = new JsonReportEngine(session);

            if (session.ReportSnapshot == null)
                engine.Execute();
            else 
            {
                var fireAndForget = SendMessage(webSocket, engine.RunJsonSnapshot());
            }
            return engine;
        }

        /// <summary>
        /// sends messages to the client connected via socket
        /// </summary>
        /// <param name="message"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public static async Task SendMessage(WebSocket webSocket, string message)
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
        public static async Task SendMessage(WebSocket webSocket, Message msg)
        {
            string message = MessageBuilder.GetJSONMessage(msg);

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<Byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);

            }
            else
            {
                // disconnect
            }
        }

        public static async void SendMessage(WebSocket webSocket, MessageBuilder.CommandType type, string body)
        {
            Message msg = new Message();
            msg.commandType = type;
            msg.message = body;

            string message = MessageBuilder.GetJSONMessage(msg);

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
        public async static Task Listen(HttpContext http, Func<Task> next)
        {
            if (http.WebSockets.IsWebSocketRequest && http.Request.Path.StartsWithSegments("/rs"))
            {
                /// accept connection               
                var webSocket = await http.WebSockets.AcceptWebSocketAsync();

                /// sends OK message
                await SendMessage(webSocket, MessageBuilder.GetJSONMessage(MessageBuilder.CommandType.NAMESPACE, string.Empty));

                /// waits for the namespace
                var nsBuffer = new ArraySegment<Byte>(new Byte[4096]);
                var recNsBuffer = await webSocket.ReceiveAsync(nsBuffer, CancellationToken.None);

                string msgNs = Encoding.UTF8.GetString(nsBuffer.Array, nsBuffer.Offset, nsBuffer.Count).Replace("\0", "");

                /// creates states machine associated with pipe  
                NamespaceMessage nm = JsonConvert.DeserializeObject<NamespaceMessage>(msgNs);
                //check the request to find a tbloader associated. If exists, use the same istance (e.g. a report called from a tbloader document)
                string tbIstanceID = nm.tbLoaderName;

                JsonReportEngine jengine = CreateEngine(nm, webSocket, tbIstanceID);

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
                                
                                Message msg = JsonConvert.DeserializeObject<Message>(recMeg);

                                msg = jengine.GetResponseFor(msg);

                                if (msg.commandType != MessageBuilder.CommandType.NONE)
                                    await SendMessage(webSocket, msg);

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
