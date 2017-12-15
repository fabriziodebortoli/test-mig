using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Text;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Newtonsoft.Json;

namespace Microarea.TbLoaderGate.Application
{
    public class TBLoaderStub
    {
        private IHostingEnvironment hostingEnvironment;
        private WebSocket webSocket;
        private string mockFolder = "mock";
        public string MockFolder { get { return Path.Combine(hostingEnvironment.ContentRootPath, mockFolder); } }

        public TBLoaderStub(IHostingEnvironment hostingEnvironment, WebSocket webSocket)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.webSocket = webSocket;
        }

        internal async Task ProcessRequest(string url, HttpRequest request, HttpResponse response)
        {
            response.StatusCode = 200;
            
            string text = "{}";
            switch (url)
            {
                case "/tb/document/initTBLogin/":
                    text = "{\"success\" : true}";
                    break;
                case "/tb/document/getThemes/":
                    text = "{\"Themes\" : {\"Theme\" : []}}";
                    break;
                case "/tb/document/getApplicationDate/":
                    string appDate = DateTime.Now.ToString("yyyy-mm-ddT00:00:00");
                    text = "{\"dateInfo\" : {\"applicationDate\" : \"" + appDate + "\",\"culture\":\"it-IT\",\"formatDate\" : \"MM/dd/yyyy\"}}";
                    break;
                case "/tb/document/getAllAppsAndModules/":
                case "/tb/document/getCurrentContext/":
                    break;
                default:
                    break;
            }
            await response.WriteAsync(text);
        }


        internal async Task ProcessWSRequest(string cmd, JObject jObj)
        {
            switch (cmd)
            {
                case "SetClientWebSocketName":
                    break;
                case "runDocument":
                    string path = Path.Combine(MockFolder, jObj["ns"]?.ToString(), "runDocument.json");
                    bool served = false;
                    if (File.Exists(path))
                    {
                        JArray jCommands = ReadJsonCommands(path);
                        if (jCommands != null)
                        {
                            foreach (JObject jCmd in jCommands)
                                await SendMessage(jCmd.ToString());
                        }
                    }
                    if (!served)
                    {
                        await SendMessage("{\"args\" : {\"component\" : {\"app\" : \"Framework\",\"id\" : \"000000\",\"mod\" : \"TbGes\",\"name\" : \"IDD_Unsupported\"}},\"cmd\" : \"WindowOpen\"  }");
                    }
                    break;
                default:
                    break;
            }

           
        }
        async Task SendMessage(string message)
        {
            var encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        JArray ReadJsonCommands(string file)
        {
            JObject jRoot = ReadJsonFile(file);
            return jRoot["items"] as JArray;
        }
        JObject ReadJsonFile(string file)
        {
            using (StreamReader sr = new StreamReader(file))
                return (JObject)JObject.ReadFrom(new JsonTextReader(sr));
        }
    }
}
