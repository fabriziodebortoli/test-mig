using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TBLoaderGate
{
    public class TBLoaderInstance
    {
        public int httpPort = 11000;
        public int wsPort = 4502;
        public string server = "localhost";

        public string BaseUrl { get { return string.Concat("http://", server, ":", httpPort); } }


        internal async Task ExecuteAsync()
        {
            TBLoaderService svc = new TBLoaderService();
            httpPort = await svc.ExecuteRemoteProcessAsync();
            wsPort = await GetWebSocketPortAsync();
        }

        private async Task<int> GetWebSocketPortAsync()
        {
            using (var client = new HttpClient())
            {
                string url = BaseUrl + "/tb/menu/getWebSocketsPort/";
                HttpRequestMessage msg = new HttpRequestMessage();
                msg.RequestUri = new Uri(url);
                HttpResponseMessage resp = await client.SendAsync(msg);
                string ret = await resp.Content.ReadAsStringAsync();
                return int.Parse(ret);
            }
        }
    }
}