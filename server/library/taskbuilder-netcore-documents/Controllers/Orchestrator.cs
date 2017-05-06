using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Documents.Interfaces;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;

namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    public class Orchestrator : IOrchestrator
    {
        // TODO sincronizzazione thread safe degli array
        Model.Controllers controllers;
        List<IDocument> documents;

        public Model.Controllers Controllers
        {
            get
            {
                return controllers;
            }
        }

        public List<IDocument> Documents
        {
            get
            {
                return documents;
            }
        }

        public Orchestrator()
        {
            controllers = new Model.Controllers();
            documents = new List<IDocument>();
        }


        public void Initialize()
        {
            AddController(new WebConnector());
            AddController(new JsonSerializer());
            AddController(new Recycler());
            AddController(new Loader(BasePathFinder.BasePathFinderInstance));
            AddController(new LicenceConnector());
        }
        public void AddController(IController controller)
        {
            controllers.Add(controller);
        }

        public ILicenceConnector LicenceConnector
        {
            get { return controllers.GetController<ILicenceConnector>(); }

        }
        public IRecycler Recycler
        {
            get { return controllers.GetController<IRecycler>(); }
        }

        public ILoader Loader
        {
            get { return controllers.GetController<ILoader>(); }
        }

        public IJsonSerializer JsonSerializer
        {
            get
            {
                return controllers.GetController<IJsonSerializer>();
            }
        }

        public IWebConnector WebConnector
        {
            get
            {
                return controllers.GetController<IWebConnector>();
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public IDocument GetDocument(CallerContext callerContext)
        {
            foreach (IDocument doc in Documents)
            {
                if (doc.NameSpace == callerContext.NameSpace && Recycler.IsAvailable(doc))
                    return doc;
            }

            if (Loader == null)
                return null;

            // controllo di licencing
            if (!LicenceConnector.IsActivated(callerContext.NameSpace))
                return null;

            // gestione del caricamento 
            Type documentType = Loader.GetDocument(callerContext.NameSpace);
            if (documentType == null)
                return null;

            Document document = Activator.CreateInstance(documentType) as Document;

            document.Initialize(this, callerContext);
            return document;
        }

        public void Close(IDocument document)
        {
            if (Recycler.IsRecyclable(document))
            {
                Recycler.Recycle(document);
                return;
            }

            if (Recycler.IsRemovable(document))
                documents.Remove(document);
        }
    }
}
