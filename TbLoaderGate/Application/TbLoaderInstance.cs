using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TBLoaderGate
{
    public class TBLoaderInstance
    {
        public int httpPort = 11000;
        public string server = "localhost";

        public string BaseUrl { get { return string.Concat("http://", server, ":", httpPort); } }


        internal async Task ExecuteAsync(string clientId)
        {
            TBLoaderService svc = new TBLoaderService();
            httpPort = await svc.ExecuteRemoteProcessAsync(clientId);
        }
        internal async void RequireWebSocketConnection(string name)
        {
           using (var client = new HttpClient())
            {
                string url = string.Concat(BaseUrl, "/tb/menu/openWebSocket/?name=", name);
                HttpRequestMessage msg = new HttpRequestMessage();
                msg.RequestUri = new Uri(url);
                HttpResponseMessage resp = await client.SendAsync(msg);
                string ret = await resp.Content.ReadAsStringAsync();
            }
        }
    }
} 