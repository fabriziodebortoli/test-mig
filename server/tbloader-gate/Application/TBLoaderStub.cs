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

        private List<DocumentStub> documents = new List<DocumentStub>();
        private int latestId = 0;
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
                case "checkMessageDialog":
                    break;
                case "runDocument":
                    {
                        bool served = false;
                        string ns = jObj["ns"]?.ToString();
                        if (ns != null)
                        {
                            string path = Path.Combine(MockFolder, ns, "runDocument.json");
                            if (File.Exists(path))
                            {
                                JArray jCommands = ReadJsonCommands(path);
                                if (jCommands != null)
                                {
                                    DocumentStub doc = new DocumentStub() { Id = NewId(), Ns = ns };
                                    documents.Add(doc);
                                    foreach (JObject jCmd in jCommands)
                                    {
                                        JValue jId = (JValue)jCmd.SelectToken("$..id");
                                        jId.Value = doc.Id;
                                        await SendMessage(jCmd.ToString());
                                    }

                                    served = true;
                                }
                            }
                        }
                        if (!served)
                        {
                            DocumentStub doc = new DocumentStub() { Id = NewId(), Ns = ns };
                            documents.Add(doc);
                            await SendMessage(string.Concat("{\"args\" : {\"component\" : {\"app\" : \"Framework\",\"id\" : \"", doc.Id, "\",\"mod\" : \"TbGes\",\"name\" : \"IDD_Unsupported\"}},\"cmd\" : \"WindowOpen\"  }"));
                        }

                        break;
                    }
                case "getWindowStrings":
                    {
                        DocumentStub doc = GetDocStub(jObj["cmpId"]?.ToString());
                        if (doc == null)
                            break;
                        string path = Path.Combine(MockFolder, doc.Ns, "getWindowStrings.json");
                        if (File.Exists(path))
                        {
                            JArray jCommands = ReadJsonCommands(path);
                            if (jCommands != null)
                            {
                                foreach (JObject jCmd in jCommands)
                                {
                                    foreach (JValue jId in jCmd.SelectTokens("$..id"))
                                        jId.Value = doc.Id;
                                    await SendMessage(jCmd.ToString());
                                }
                            }
                        }
                        break;

                    }
                case "getDocumentData":
                    {
                        DocumentStub doc = GetDocStub(jObj["cmpId"]?.ToString());
                        if (doc == null)
                            break;
                        string path = Path.Combine(MockFolder, doc.Ns, "getDocumentData.json");
                        if (File.Exists(path))
                        {
                            JArray jCommands = ReadJsonCommands(path);
                            if (jCommands != null)
                            {
                                foreach (JObject jCmd in jCommands)
                                {
                                    foreach (JValue jId in jCmd.SelectTokens("$..id"))
                                        jId.Value = doc.Id;
                                    await SendMessage(jCmd.ToString());
                                }
                            }
                        }
                        break;
                    }
                case "doCommand":
                    {
                        DocumentStub doc = GetDocStub(jObj["cmpId"]?.ToString());
                        if (doc == null)
                            break;
                        string cmdId = jObj["id"]?.ToString();
                        switch (cmdId)
                        {
                            case "ID_FILE_CLOSE":
                                {
                                    documents.Remove(doc);
                                    await SendMessage(string.Concat("{\"args\" : {\"id\" : \"", doc.Id, "\"},\"cmd\" : \"WindowClose\"  }"));
                                    break;
                                }
                            default: break;
                        }
                        break;
                    }
                default:
                    break;
            }


        }

        private DocumentStub GetDocStub(string id)
        {
            foreach (DocumentStub doc in documents)
                if (doc.Id == id)
                    return doc;
            return null;
        }

        private string NewId()
        {
            return (++latestId).ToString("000000");
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

    class DocumentStub
    {
        public string Id { get; set; }
        public string Ns { get; set; }
    }
}
