using Microarea.Common.NameSolver;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Controllers;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace Microarea.TbfWebGate.Application
{
    //===============================================================================================
    public class OrchestratorService : IDisposable, IOrchestratorService
    {
        IHostingEnvironment hostingEnvironment;
        //WebSocket webSocket;
        Orchestrator orchestrator;
        bool disposed;


        //---------------------------------------------------------------------
        public OrchestratorService(IHostingEnvironment hostingEnvironment/*, WebSocket webSocket*/)
        {
            this.hostingEnvironment = hostingEnvironment;
            //this.webSocket = webSocket;
            this.orchestrator = new Orchestrator(PathFinder.PathFinderInstance);
            this.orchestrator.Configure();
        }
        
        //---------------------------------------------------------------------
        public IEnumerable<string> GetAllComponents()
        {
            return null;//return orchestrator.Components.Select(c => c.NameSpace.ToString());
        }

        //---------------------------------------------------------------------
        public string GetComponent(CallerContext context)
        {
            string outputMessage;
            var component = orchestrator.GetComponent(context);
            if (component == null)
                outputMessage = "{\"Success\" : \"Component not found\"}";
            else
                outputMessage = "{\"Success\" : \"Component found\"}";
            return outputMessage;
        }

        //---------------------------------------------------------------------
        public string CloseComponent(CallerContext context)
        {
            orchestrator.CloseComponent(context);

            return "{\"Success\" : \"Component closed\"}";
        }

        //---------------------------------------------------------------------
        public IEnumerable<string> GetAllDocuments()
        {
            return orchestrator.Documents.Select(d => d.NameSpace.ToString());
        }

        //---------------------------------------------------------------------
        public string GetDocument(CallerContext context)
        {
            string outputMessage;
            var document = orchestrator.GetDocument(context);
            if (document == null)
                outputMessage = "{\"Success\" : \"Document not found\"}";
            else
                outputMessage = "{\"Success\" : \"Document found\"}";
            return outputMessage;
        }

        //---------------------------------------------------------------------
        public string CloseDocument(CallerContext context)
        {
            orchestrator.CloseDocument(context);

            return "{\"Success\" : \"Document closed\"}";
        }

        //---------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //if (this.webSocket != null)
                    //{
                    //    this.webSocket.Dispose();
                    //    this.webSocket = null;
                    //}
                }

                disposed = true;
            }
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
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

