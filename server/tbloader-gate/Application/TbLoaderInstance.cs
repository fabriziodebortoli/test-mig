using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microarea.TbLoaderGate
{
    public class TBLoaderInstance
    {
		public string name;
		public int httpPort = 11000;
		public int processId = -1;
        public string server = "localhost";

        public string BaseUrl { get { return string.Concat("http://", server, ":", httpPort); } }

		//-----------------------------------------------------------------------------------------
		public TBLoaderInstance()
		{
			this.name = Guid.NewGuid().ToString();
		}

		//-----------------------------------------------------------------------------------------
		internal async Task ExecuteAsync()
        {
            TBLoaderService svc = new TBLoaderService();
			TBLoaderResponse response =  await svc.ExecuteRemoteProcessAsync(name);
			httpPort = response.Port;
			processId = response.ProcessId;
		}

		//-----------------------------------------------------------------------------------------
		internal async void RequireWebSocketConnection(string name, HttpRequest request)
        {
            using (var client = new HttpClient())
            {
				IPAddress[] addresses = await Dns.GetHostAddressesAsync(request.Host.Host);
				IPAddress goodAddress = null;
				foreach (IPAddress ip in addresses)
					if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
					{
						goodAddress = ip;
						break;
					}
				if (goodAddress == null)
					return;
				int port = request.Host.Port.HasValue ? request.Host.Port.Value : 80;
				string server = goodAddress.ToString();
                string wsUrl = string.Concat("ws://", server, ":", port, request.PathBase + "/tbloader/");  
                //string wsUrl = "ws://localhost:80/development/m4server/tbloader";
                string url = string.Concat(BaseUrl, "/tb/document/openWebSocket/?name=", name, "&url=", WebUtility.HtmlEncode(wsUrl));
                HttpRequestMessage msg = new HttpRequestMessage();
                msg.RequestUri = new Uri(url);
                HttpResponseMessage resp = await client.SendAsync(msg);
                string ret = await resp.Content.ReadAsStringAsync();
			}
		}
    }
}