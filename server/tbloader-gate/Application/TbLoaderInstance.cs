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
        public string Name;
        public bool Connected = false;
        public string ConnectionMessage = "";
        public int SocketPort = 4502;
        public int ProcessId = -1;

        public int TbLoaderServicePort = 11000;
        public int HttpPort = 10000;
        public string Server = "localhost";

        public string BaseUrl { get { return string.Concat("http://", Server, ":", HttpPort); } }
        public string WSBaseUrl { get { return string.Concat("ws://", Server, ":", SocketPort, "/TBWebSocketsController/"); } }

        //-----------------------------------------------------------------------------------------
        public TBLoaderInstance(string server, int port, string name)
        {
            this.TbLoaderServicePort = port;
            this.Server = server;
            this.Name = string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name;
        }

        //-----------------------------------------------------------------------------------------
        internal async Task ExecuteAsync()
        {
            TBLoaderService svc = new TBLoaderService(Server, TbLoaderServicePort);
            TBLoaderResponse response = await svc.ExecuteRemoteProcessAsync(Name);
            HttpPort = response.Port;
            ProcessId = response.ProcessId;
            Connected = response.Result;
            ConnectionMessage = response.Message;
        }

        //-----------------------------------------------------------------------------------------
        internal bool IsProcessRunning()
        {
            TBLoaderService svc = new TBLoaderService(Server, TbLoaderServicePort);
            TBLoaderResponse response = svc.IsProcessRunning(ProcessId).Result;
            return response.Result;
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
                    SocketPort = jRes["port"].Value<int>();
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