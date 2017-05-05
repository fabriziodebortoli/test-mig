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
    public class Orchestrator : IOrchestrator
    {
        Model.Controllers controllers;
        List<Document> documents;

        public Model.Controllers Controllers
        {
            get
            {
                return controllers;
            }
        }

        public List<Document> Documents
        {
            get
            {
                return documents;
            }
        }

        public Orchestrator()
        {
            controllers = new Model.Controllers();
            documents = new List<Document>();
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

        ILoader  Loader
        {
            get { return controllers.GetController<ILoader>(); }
   
        }

        ILicenceConnector LicenceConnector
        {
            get { return controllers.GetController<ILicenceConnector>(); }

        }
        IRecycler Recycler
        {
            get { return controllers.GetController<IRecycler>(); }

        }

        public IDocument GetDocument(CallerContext callerContext)
        {
            foreach (IDocument document in Documents)
            {
                if (document.NameSpace == callerContext.NameSpace && Recycler.IsAvailable(document))
                    return document;
            }

            if (Loader == null)
                return null;

            // controllo di licencing
            if (!LicenceConnector.IsActivated(callerContext.NameSpace))
                return null;

            // gestione del caricamento 
            Type businessObjectType = Loader.GetDocument(callerContext.NameSpace);
            if (businessObjectType == null)
                return null;

            // istanza della classe sulla base del businessObjectregistry
            Document businessObject = Activator.CreateInstance(businessObjectType) as Document;
            businessObject.Initialize(this, callerContext);
            return businessObject;
        }
    }
}
