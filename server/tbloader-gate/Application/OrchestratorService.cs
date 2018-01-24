﻿using Microarea.Common.NameSolver;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Controllers;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Documents.Model.TbModel;

namespace Microarea.TbLoaderGate.Application
{
    //===============================================================================================
    public class OrchestratorService
    {
        private IHostingEnvironment hostingEnvironment;
        private WebSocket webSocket;
        private Orchestrator orchestrator;

        static public string SubUrl { get => "/tbo/"; }


        //-----------------------------------------------------------------------------------------------------
        public OrchestratorService(IHostingEnvironment hostingEnvironment, WebSocket webSocket)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.webSocket = webSocket;
            this.orchestrator = new Orchestrator(PathFinder.PathFinderInstance);
            this.orchestrator.Configure();
        }

        //-----------------------------------------------------------------------------------------------------
        private CallerContext GetContextFromParameters(HttpRequest request)
        {
            CallerContext context = null;
            string callerContext = request.Query["context"];
            if (!string.IsNullOrEmpty(callerContext))
                context = JsonConvert.DeserializeObject<CallerContext>(callerContext);

            if (context == null)
                context = new CallerContext();

            string name = request.Query["name"];
            if (!string.IsNullOrEmpty(name))
                context.ObjectName = name;
            string company = request.Query["company"];

            if (!string.IsNullOrEmpty(company))
                context.Company = company;

            return context;
        }

        //-----------------------------------------------------------------------------------------------------
        internal async Task ProcessRequest(string url, HttpRequest request, HttpResponse response, string authHeader)
        {
            // tolgo l'url
            url = url.Substring(SubUrl.Length);

            CallerContext context = GetContextFromParameters(request);
            context.AuthToken = authHeader;

            string outputMessage = string.Empty;
            response.StatusCode = 200;

            switch (url.ToLower())
            {
                case "getcomponent":

                    IComponent component = orchestrator.GetComponent(context);
                    if (component == null)
                        outputMessage = "{\"Success\" : \"Component not found\"}";
                    else
                        outputMessage = "{\"Success\" : \"Component found\"}";
                    break;
                case "closecomponent":
                    orchestrator.CloseComponent(context);
                    break;
                case "getdocument":

                    IDocument document = orchestrator.GetDocument(context);
                    if (document == null)
                        outputMessage = "{\"Success\" : \"Document not found\"}";
                    else
                        outputMessage = "{\"Success\" : \"Document found\"}";
                    break;
                case "closedocument":
                    orchestrator.CloseDocument(context); break;
                case "test":
                    TestSamples samples = new TestSamples(orchestrator);
                    outputMessage = samples.RunAllTests();
                    samples = null;
                    break;
                case "saveorders":
                    TestSamples testSaveOrders = new TestSamples(orchestrator);
                    outputMessage = testSaveOrders.ExecuteSaveOrders();
                    samples = null;
                    break;
                default:
                    break;
            }
            await response.WriteAsync(outputMessage);
        }

        /// <summary>
        ///  Blocco di test dimostrtivo per adesso tutto sincrono
        /// </summary>
        //-----------------------------------------------------------------------------------------------------
        }

    //===============================================================================================
    internal class TestSamples
    {
        Orchestrator orchestrator;
        internal TestSamples(Orchestrator orchestrator) => this.orchestrator = orchestrator; 
        //-----------------------------------------------------------------------------------------------------
        internal string RunAllTests() =>    BrowseDataEntries() + ExecuteSaveOrders();


        // questo esempio usa il documento generico
        // e userà la stessa istanza di documento
        // perchè il caller context è lo stesso
        //-----------------------------------------------------------------------------------------------------
        private string BrowseDataEntries()
        {
            StringBuilder builder = new StringBuilder();
            /////////////////////////////////////////////////////////
            // documento interattivo
            /////////////////////////////////////////////////////////
            CallerContext myContext = new CallerContext();
            myContext.ObjectName = "Document.NEWERP.Masters.Documents.Customers";
            myContext.AuthToken = "1";
            myContext.Company = "Az1";

            // qui istanzia il documento
            IDocument document = orchestrator.GetDocument(myContext);
            if (document != null)
                builder.AppendLine(string.Concat("documento aperto: ", document.NameSpace));
            else
                builder.AppendLine(string.Concat("documento non trovato: ", myContext.NameSpace));
            
            // anche se richiamo due volte la GetDocument con lo stesso contesto mi torna la stessa istanza
            Document myDocument = orchestrator.GetDocument(myContext) as Document;
            if (myDocument.LoadData())
                builder.AppendLine(string.Concat("data browsed and ready to use : ", document.Title));

            /////////////////////////////////////////////////////////
            //      documento interattivo completamente dinamico
            /////////////////////////////////////////////////////////
            CallerContext myContext1 = new CallerContext();
            myContext1.ObjectName = "Document.CRM.Masters.Documents.MyDoc1";
            myContext1.AuthToken = "1";
            myContext1.Company = "Az1";

            Document dynamicDocument = orchestrator.GetDocument(myContext1) as Document;
            if (dynamicDocument != null && dynamicDocument.LoadData())
                builder.AppendLine(string.Concat("Data browsed and ready to use : ", dynamicDocument.Title));
            else
                builder.AppendLine(string.Concat("documento non trovato: ", myContext1.NameSpace));

             return builder.ToString();
        }

        // questo esempio usa l'interfaccia batch
        // per lanciare un documento batch
        //-----------------------------------------------------------------------------------------------------
        internal string ExecuteSaveOrders()
        {
            StringBuilder output = new StringBuilder();
            /////////////////////////////////////////////////////////
            //      batch unattended implementato con documento programmativo
            /////////////////////////////////////////////////////////
            CallerContext myContext = new CallerContext();
            myContext.ObjectName = "Document.NEWERP.Orders.Documents.SaveOrders";
            myContext.AuthToken = "1";
            myContext.Company = "Az1";
            myContext.Mode = ExecutionMode.Unattended;

            IBatchActivity myBatch = orchestrator.GetDocument(myContext) as IBatchActivity;
			IDocument doc = myBatch as IDocument;
			doc.ActivatorService = orchestrator;
			foreach (Component component in doc.Components)
				component.ActivatorService = orchestrator;

			if (myBatch != null)
            {
                if (myBatch.ExecuteActivity())
                    output.AppendLine(string.Concat("batchActivity eseguita: ", myBatch.NameSpace));
                else
                    output.AppendLine(string.Concat("batchActivity non eseguita: ", myBatch.NameSpace));
            }
            else
                output.AppendLine(string.Concat("batchActivity non trovata: ", myBatch.NameSpace));

            return output.ToString();
        }
    }
}

