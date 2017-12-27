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
using System.Diagnostics;

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
                case "getOpenDocuments":
                    {
                        foreach (DocumentStub doc in Cache.Documents)
                        {
                            foreach (JObject jCmd in doc.runDocument)
                            {
                                await SendMessage(jCmd.ToString());
                            }
                        }
                        break;
                    }
                case "checkMessageDialog":
                    break;
                case "runDocument":
                    {
                        bool served = false;
                        string ns = jObj["ns"]?.ToString();
                        if (ns != null)
                        {
                            string folder = Path.Combine(MockFolder, ns);
                            string path = Path.Combine(folder, "runDocument.json");
                            if (File.Exists(path))
                            {
                                DocumentStub doc = new DocumentStub() { Id = Cache.NewId(), Ns = ns };
                                Cache.AddDocument(doc);
                                doc.runDocument = ReadJsonCommands(path, doc.Id);
                                doc.newData = ReadJsonCommands(Path.Combine(MockFolder, doc.Ns, "new.json"), doc.Id);
                                doc.windowStrings = ReadJsonCommands(Path.Combine(MockFolder, doc.Ns, "getWindowStrings.json"), doc.Id);
                                foreach (string dataFile in Directory.GetFiles(folder, "*.doc.json"))
                                    doc.Data.Add(ReadJsonCommands(dataFile, doc.Id));

                                foreach (JObject jCmd in doc.runDocument)
                                {
                                    await SendMessage(jCmd.ToString());
                                }

                                served = true;
                            }
                        }
                        if (!served)
                        {
                            DocumentStub doc = new DocumentStub() { Id = Cache.NewId(), Ns = ns };
                            Cache.AddDocument(doc);
                            await SendMessage(string.Concat("{\"args\" : {\"component\" : {\"app\" : \"Framework\",\"id\" : \"", doc.Id, "\",\"mod\" : \"TbGes\",\"name\" : \"IDD_Unsupported\"}},\"cmd\" : \"WindowOpen\"  }"));
                        }

                        break;
                    }
                case "getWindowStrings":
                    {
                        DocumentStub doc = Cache.GetDocStub(jObj["cmpId"]?.ToString());
                        if (doc == null)
                            break;
                        foreach (JObject jCmd in doc.windowStrings)
                        {
                            await SendMessage(jCmd.ToString());
                        }
                    }
                    break;

                case "getDocumentData":
                    {
                        DocumentStub doc = Cache.GetDocStub(jObj["cmpId"]?.ToString());
                        if (doc == null)
                            break;
                        string path = Path.Combine(MockFolder, doc.Ns, "getDocumentData.json");
                        if (File.Exists(path))
                        {
                            JArray jCommands = doc.CurrentData;
                            if (jCommands == null)
                                break;
                            JArray other = ReadJsonCommands(path, doc.Id);
                            if (other == null)
                                break;
                            jCommands.Append(other);
                            foreach (JObject jCmd in jCommands)
                            {
                                await SendMessage(jCmd.ToString());
                            }
                        }
                        break;
                    }
                case "doCommand":
                    {
                        DocumentStub doc = Cache.GetDocStub(jObj["cmpId"]?.ToString());
                        if (doc == null)
                            break;
                        string cmdId = jObj["id"]?.ToString();
                        switch (cmdId)
                        {
                            case "ID_FILE_CLOSE":
                            case "ID_EXTDOC_EXIT":
                                {
                                    Cache.RemoveDocument(doc);
                                    await SendMessage(string.Concat("{\"args\" : {\"id\" : \"", doc.Id, "\"},\"cmd\" : \"WindowClose\"  }"));
                                    break;
                                }

                            case "ID_EXTDOC_NEW":
                                {
                                    if (doc.State == DocumentStub.DocState.Browse)
                                    {
                                        doc.State = DocumentStub.DocState.New;
                                        await doc.SendCurrentData(this);
                                    }
                                    break;
                                }
                            case "ID_EXTDOC_FIRST":
                                {
                                    if (doc.State == DocumentStub.DocState.Browse && doc.currDoc != 0)
                                    {
                                        doc.currDoc = 0;
                                        await doc.SendCurrentData(this);
                                    }
                                    break;
                                }
                            case "ID_EXTDOC_LAST":
                                {
                                    if (doc.State == DocumentStub.DocState.Browse && doc.currDoc != doc.Data.Count - 1)
                                    {
                                        doc.currDoc = doc.Data.Count - 1;
                                        await doc.SendCurrentData(this);
                                    }
                                    break;
                                }
                            case "ID_EXTDOC_PREV":
                                {
                                    if (doc.State == DocumentStub.DocState.Browse && doc.currDoc > 0)
                                    {
                                        doc.currDoc--;
                                        await doc.SendCurrentData(this);
                                    }
                                    break;
                                }
                            case "ID_EXTDOC_NEXT":
                                {
                                    if (doc.State == DocumentStub.DocState.Browse && doc.currDoc < doc.Data.Count)
                                    {
                                        doc.currDoc++;
                                        await doc.SendCurrentData(this);
                                    }
                                    break;
                                }
                            case "ID_EXTDOC_ESCAPE":
                                {
                                    if (doc.State != DocumentStub.DocState.Browse)
                                    {
                                        doc.State = DocumentStub.DocState.Browse;
                                        await doc.SendCurrentData(this);
                                    }
                                    break;
                                }
                            case "ID_EXTDOC_EDIT":
                                {
                                    if (doc.State == DocumentStub.DocState.Browse)
                                    {
                                        doc.State = DocumentStub.DocState.Edit;
                                        await doc.SendCurrentData(this);
                                    }
                                    break;
                                }
                            //comodi per edit & continue
                            case "ID_EXTDOC_SAVE":
                                {
                                    if (doc.State != DocumentStub.DocState.Browse)
                                    {
                                        doc.ChangeCurrentData(jObj);
                                        doc.State = DocumentStub.DocState.Browse;
                                        await doc.SendCurrentData(this);
                                    }
                                    break;
                                }
                            case "2":
                            case "3":
                            case "4":
                            case "5":
                            case "6":
                            case "7":
                            case "8":
                            case "9":
                            default: break;
                        }
                        break;
                    }
                default:
                    break;
            }


        }


        internal async Task SendMessage(string message)
        {
            var encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        JArray ReadJsonCommands(string file, string id)
        {
            JObject jRoot = ReadJsonFile(file);

            foreach (JValue jId in jRoot.SelectTokens("$..id"))
                jId.Value = id;
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
        public enum DocState { Browse, Edit, New }
        public DocState State = DocState.Browse;
        public int currDoc = 0;
        public JArray runDocument;
        public JArray newData;
        public List<JArray> Data = new List<JArray>();
        internal JArray windowStrings;

        public string Id { get; set; }
        public string Ns { get; set; }
        public JArray CurrentData
        {
            get
            {
                if (currDoc < 0 || currDoc >= Data.Count)
                    return null;
                JArray data = Data[currDoc];
                JArray currData = new JArray();
                switch (State)
                {
                    case DocState.Browse:
                        currData.Add(data[0].DeepClone());
                        currData.Add(data[1].DeepClone());
                        break;
                    case DocState.Edit:
                        currData.Add(data[2].DeepClone());
                        currData.Add(data[3].DeepClone());
                        break;
                    case DocState.New:
                        currData.Add(newData[0].DeepClone());
                        currData.Add(newData[1].DeepClone());
                        break;
                    default:
                        break;
                }
                return currData;
            }
        }

        internal async Task SendCurrentData(TBLoaderStub stub)
        {
            JArray d = CurrentData;
            if (d != null)
            {
                foreach (JObject jCmd in d)
                {
                    await stub.SendMessage(jCmd.ToString());
                }
            }
        }

        internal void ChangeCurrentData(JObject jObj)
        {
            JArray mod = (JArray)jObj.SelectToken("model");

            JArray data = Data[currDoc];
            JObject jData = (JObject)data[0].SelectToken("args.models[0].data");
            ApplyPatch(jData, mod);
            jData = (JObject)data[2].SelectToken("args.models[0].data");
            ApplyPatch(jData, mod);
        }

        private void ApplyPatch(JObject jData, JArray mod)
        {
            foreach (JObject jOp in mod)
            {
                string op = jOp["op"].ToString();
                string path = jOp["path"].ToString();
                JToken jVal = jOp["value"];

                string jPath = path.Replace("/", ".");
                JToken jProp = jData.SelectToken(jPath);
                if (jProp != null)
                {
                    if (op == "replace")
                    {
                        jProp.Replace(jVal);
                    }
                    else if (op == "remove")
                    {
                        jProp.Remove();
                    }
                    else if (op == "add")
                    {
                        Debug.WriteLine("Operation not supported: " + op);
                    }
                    else
                    {
                        Debug.WriteLine("Operation not supported: " + op);
                    }
                }

            }
        }
    }

    static class Cache
    {
        private static List<DocumentStub> documents = new List<DocumentStub>();
        private static int latestId = 0;

        internal static DocumentStub[] Documents
        {
            get
            {
                lock (typeof(Cache))
                {
                    return documents.ToArray();
                }
            }
        }
        internal static void AddDocument(DocumentStub doc)
        {
            lock (typeof(Cache))
            {
                documents.Add(doc);
            }
        }

        internal static void RemoveDocument(DocumentStub doc)
        {
            lock (typeof(Cache))
            {
                documents.Remove(doc);
            }
        }

        internal static DocumentStub GetDocStub(string id)
        {
            lock (typeof(Cache))
            {
                foreach (DocumentStub doc in documents)
                    if (doc.Id == id)
                        return doc;
            }
            return null;

        }
        internal static string NewId()
        {
            return Interlocked.Increment(ref latestId).ToString("000000");
        }
    }
}
