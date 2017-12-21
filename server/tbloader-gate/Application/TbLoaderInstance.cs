using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microarea.TbLoaderGate
{
    public class TBLoaderInstance
    {
        public string name;
        public int httpPort = 11000;
        public int socketPort = 4502;
        public int processId = -1;
        public string server = "localhost";

        public string BaseUrl { get { return string.Concat("http://", server, ":", httpPort); } }
        public string WSBaseUrl { get { return string.Concat("ws://", server, ":", socketPort, "/TBWebSocketsController/"); } }

        //-----------------------------------------------------------------------------------------
        public TBLoaderInstance(string server, int port)
        {
            this.httpPort = port;
            this.server = server;
            this.name = Guid.NewGuid().ToString();
        }

        //-----------------------------------------------------------------------------------------
        internal async Task ExecuteAsync()
        {
            TBLoaderService svc = new TBLoaderService(server, httpPort);
            TBLoaderResponse response = await svc.ExecuteRemoteProcessAsync(name);
            httpPort = response.Port;
            processId = response.ProcessId;
        }

       
        //-----------------------------------------------------------------------------------------
        internal async Task<WebSocket> OpenWebSocketAsync(string name)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = string.Concat(BaseUrl, "/tb/document/getWebSocketsPort/");
                    HttpRequestMessage msg = new HttpRequestMessage();
                    msg.RequestUri = new Uri(url);
                    HttpResponseMessage resp = await client.SendAsync(msg);
                    string ret = await resp.Content.ReadAsStringAsync();
                    JObject jRes = JObject.Parse(ret);
                    socketPort = jRes["port"].Value<int>();
                }

                ClientWebSocket ws = new ClientWebSocket();
                CancellationToken token = CancellationToken.None;
                await ws.ConnectAsync(new Uri(WSBaseUrl), token);
                byte[] buff = Encoding.UTF8.GetBytes("SetWebSocketName:" + name);
                ArraySegment<byte> seg = new ArraySegment<byte>(buff);
                await ws.SendAsync(seg, WebSocketMessageType.Text, true, token);

                JObject jCmd = new JObject();
                jCmd["cmd"] = "getOpenDocuments";
                buff = Encoding.UTF8.GetBytes(jCmd.ToString());
                seg = new ArraySegment<Byte>(buff, 0, buff.Length);
                await ws.SendAsync(seg, WebSocketMessageType.Text, true, CancellationToken.None);

                return ws;
            }
            catch
            {
                return null;
            }
        }
    }
}