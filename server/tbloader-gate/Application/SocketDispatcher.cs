
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Microarea.TbLoaderGate.Application;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

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
            if (clientSocket == webSocket)
            {
                clientSocket.Dispose();
                clientSocket = null;
            }

            else if (serverSocket == webSocket)
            {
                serverSocket.Dispose();
                serverSocket = null;
            }
        }
    }

    public class SocketDispatcher
    {
        private readonly IHostingEnvironment hostingEnvironment;
        TBLoaderConnectionParameters options;

        public SocketDispatcher(IHostingEnvironment hostingEnvironment, TBLoaderConnectionParameters options)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.options = options;
        }

        public async Task Listen(HttpContext http, Func<Task> next)
        {
            if (http.WebSockets.IsWebSocketRequest && http.Request.Path.StartsWithSegments("/tbloader"))
            {
                await HandleAsync(http);
            }
            else
            {
                await next();
            }
        }

        const string setClientWebSocketName = "SetClientWebSocketName";
        static Dictionary<string, WebSocketCouple> socketMap = new Dictionary<string, WebSocketCouple>();

        public static async Task<String> ReadString(WebSocket ws)
        {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[4096]);

            WebSocketReceiveResult result = null;
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage && ws.State == WebSocketState.Open);
                if (result.MessageType != WebSocketMessageType.Text)
                    return "";

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
        }

        public async Task HandleAsync(HttpContext http)
        {
            var webSocket = await http.WebSockets.AcceptWebSocketAsync();
            await ManageSocketTraffic(webSocket, new RecordedWSMessage(), true);
        }

        private async Task ManageSocketTraffic(WebSocket webSocket, RecordedWSMessage wsMessage, bool client2server, string coupleName = "")
        {
            try
            {
                TBLoaderStub stub = null;
                while (webSocket.State == WebSocketState.Open)
                {
                    string message = await ReadString(webSocket);
                    if (message != string.Empty)
                    {

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
                            Debug.WriteLine("Exception on parsing WebSocketMessageType.Text");
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine(message);
                            continue;
                        }

                        var cmd = jObj["cmd"];
                        if (cmd == null)
                        {
                            //TODO gestione errore
                            Debug.WriteLine("Missing cmd");
                            continue;
                        }

                        if (options.TestMode)
                        {
                            if (stub == null)
                                stub = new TBLoaderStub(hostingEnvironment, webSocket);
                            await stub.ProcessWSRequest(cmd.Value<string>(), jObj);
                        }
                        else
                        {
                            if (cmd.Value<string>() == setClientWebSocketName)
                            {
                                JObject jArgs = jObj["args"] as JObject;
                                if (jArgs == null)
                                {
                                    //TODO gestione errore
                                    Debug.WriteLine("Missing args");
                                    continue;
                                }

                                var jName = jArgs["webSocketName"];
                                if (jName == null)
                                {
                                    //TODO gestione errore
                                    Debug.WriteLine("Missing webSocketName");
                                    continue;
                                }
                                coupleName = jName.ToString();

                                couple = GetWebCouple(coupleName);
                                couple.clientSocket = webSocket;

                                jName = jArgs["tbLoaderName"];
                                if (jName == null)
                                {
                                    //TODO gestione errore
                                    Debug.WriteLine("Missing tbLoaderName");
                                    continue;
                                }
                                string tbName = jName.ToString();
                                TBLoaderInstance tb = TBLoaderEngine.GetTbLoader(options.TbLoaderServiceHost, options.TbLoaderServicePort, tbName, false, out bool dummy);
                                if (tb != null)
                                {
                                    couple = GetWebCouple(coupleName);
                                    couple.serverSocket = await tb.OpenWebSocketAsync(coupleName);
#pragma warning disable 4014 //questo deve essere asincrono, non è necessario aspettarlo, gestisce il traffico del socket tb
                                    ManageSocketTraffic(couple.serverSocket, wsMessage, false, coupleName);
#pragma warning restore 4014
                                }
                                continue;
                            }
                            else
                            {
                                //altrimenti cerco il socket della controparte (by name) e gli mando una copia di quanto ricevuto
                                couple = GetWebCouple(coupleName);
                                if (options.RecordingMode)
                                {
                                    RecordRequest(cmd.Value<string>(), jObj, wsMessage, client2server);
                                }
                            }

                            if (couple != null)
                            {
                                WebSocket otherSocket = couple.GetOther(webSocket);
                                if (otherSocket != null)
                                {
                                    var encoded = Encoding.UTF8.GetBytes(message);
                                    var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                                    await otherSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                            }
                        }
                    }
                    //			break;
                    //		}
                    //}
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            DisconnectOtherSocket(coupleName, webSocket);
        }

        private void RecordRequest(string msg, JObject jObj, RecordedWSMessage parentReq, bool client2server)
        {
            try
            {
                lock (this)
                {
                    RecordedWSMessage req = null;
                    if (client2server)
                    {
                        req = parentReq;
                        if (!string.IsNullOrEmpty(req.Cmd))
                        {
                            req.ResponseMessages.Clear();
                        }
                        req.FileName = msg + ".tbjson";
                    }
                    else
                    {
                        req = new RecordedWSMessage();
                        parentReq.ResponseMessages.Add(req);
                    }
                    req.Cmd = msg;
                    req.Body = jObj.ToString();

                    parentReq.Save(hostingEnvironment, req.FileName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
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