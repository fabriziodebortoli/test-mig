using System;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Model;
using Microarea.Common.NameSolver;
using System.Collections.ObjectModel;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{
    // TODO Gestione chiamate asincrone, thread di documento e sincronizzazione dei thread
    //====================================================================================    
    public class Orchestrator : IOrchestrator, IDocumentServices
    {
        PathFinder pathFinder;
         // TODO sincronizzazione thread safe degli array
        Controllers controllers;
        // anche se potenzialmente potrebbe stare tutto in un array 
        // preferisco tenerli separati logicamente per velocità di 
        // gestione e scorrimento
        ObservableCollection<IDocument> documents;
        ObservableCollection<IComponent> components;

        //-----------------------------------------------------------------------------------------------------
        public Controllers Controllers { get => controllers; }

        //-----------------------------------------------------------------------------------------------------
        public ObservableCollection<IDocument> Documents { get => documents; }

        //-----------------------------------------------------------------------------------------------------
        public Orchestrator(PathFinder pathFinder)
        {
            controllers = new Controllers();
            documents = new ObservableCollection<IDocument>();
            components = new ObservableCollection<IComponent>();
            this.pathFinder = pathFinder;
         }

        //-----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads default controller list
        /// </summary>
        /// <param name="pathFinder"></param>
        public void Configure()
        {
            LoadFromFile();
            IntegrateMandatories();
            InitializeControllers();
        }

        //-----------------------------------------------------------------------------------------------------
       private void  InitializeControllers ()
        {
            foreach (Controller controller in Controllers)
            {
                controller.PathFinder = pathFinder;
            }
            Loader.DocumentServices = this;
        }

        //-----------------------------------------------------------------------------------------------------
        private void IntegrateMandatories()
        {
            if (this.Loader == null) AddController(new Loader());
            if (this.LicenceConnector == null) AddController(new LicenceConnector());
        }

        //-----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Load controller list from configuration settings file
        /// </summary>
        private void LoadFromFile()
        {
            ControllersDeclaration declaration = ControllersDeclaration.LoadFromDefaultFile();

            if (declaration == null || declaration.Controllers == null)
                return;

            foreach (ControllerDeclaration controllerDeclaration in declaration.Controllers)
            {
                Type type = Type.GetType(controllerDeclaration.Type);
                if (type == null)
                    continue;

                IController controller = Activator.CreateInstance(type) as IController;
                if (controller != null)
                    AddController(controller);
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public void AddController(IController controller)
        {
            controllers.Add(controller);
        }

        //-----------------------------------------------------------------------------------------------------
        public void RemoveController(IController controller)
        {
            controllers.Remove(controller);
        }

        //-----------------------------------------------------------------------------------------------------
        public ILicenceConnector LicenceConnector { get=> controllers.GetController<ILicenceConnector>(); }

        //-----------------------------------------------------------------------------------------------------
        public IRecycler Recycler { get => controllers.GetController<IRecycler>(); }

        //-----------------------------------------------------------------------------------------------------
        public ILoader Loader { get => controllers.GetController<ILoader>(); }

        //-----------------------------------------------------------------------------------------------------
        public IStateSerializer StateSerializer  { get => controllers.GetController<IStateSerializer>(); }

        //-----------------------------------------------------------------------------------------------------
        public IUIController UIController  { get => controllers.GetController<IUIController>(); }

        //-----------------------------------------------------------------------------------------------------
        public ILogger Logger  { get => controllers.GetController<ILogger>(); }

        //-----------------------------------------------------------------------------------------------------
        public ObservableCollection<IComponent> Components { get => components; }

        #region IDocumentServices interface
        //-----------------------------------------------------------------------------------------------------
        public bool IsActivated(INameSpace nameSpace) => LicenceConnector.IsActivated(nameSpace);

        //-----------------------------------------------------------------------------------------------------
        public bool IsActivated(string activation) => LicenceConnector.IsActivated(activation);

        //-----------------------------------------------------------------------------------------------------
        public IDocument GetDocument(ICallerContext callerContext)
        {
            foreach (Document doc in Documents)
            {
                if (Recycler != null && Recycler.IsAssignable(doc, callerContext))
                    return doc;
            }

            IDocument document = null;
            try
            {
                document = Loader.GetDocument(callerContext);
            }
            catch (ControllerException loaderEx)
            {
                callerContext.Diagnostic.SetError(loaderEx.FullMessage);
            }
            catch (Exception ex)
            {
                callerContext.Diagnostic.SetError(ex.Message);
            }

            if (document != null)
            {
                if (StateSerializer != null) 
                    StateSerializer.Add(document);
                Documents.Add(document);
            }

            return document;
        }

        //-----------------------------------------------------------------------------------------------------
        public IComponent GetComponent(ICallerContext callerContext)
        {
            if (Loader == null)
                return null;

            foreach (IComponent comp in Components)
            {
                if (Recycler != null && Recycler.IsAssignable(comp, callerContext))
                    return comp;
            }

            IComponent component = null;
            try
            {
                component = Loader.GetComponent(callerContext.NameSpace, callerContext);
            }
            catch (ControllerException loaderEx)
            {
                callerContext.Diagnostic.SetError(loaderEx.FullMessage);
            }
            catch (Exception ex)
            {
                callerContext.Diagnostic.SetError(ex.Message);
            }

            if (component != null)
                Components.Add(component);
            
            return component;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool ExecuteActivity(ICallerContext callerContext)
        {
            string message = string.Empty;
            IDocument document = GetDocument(callerContext);

            if (document == null)
            {
                message = string.Format(OrchestratorMessages.DocumentNotFound.CompleteMessage, callerContext.NameSpace.GetNameSpaceWithoutType());
                callerContext.Diagnostic.SetError(message);
                return false;                
            }

            IBatchActivity activity = document as IBatchActivity;
            if (activity != null)
                return activity.ExecuteActivity();

            message = string.Format(OrchestratorMessages.IsNotActivityDocument.CompleteMessage, callerContext.NameSpace.GetNameSpaceWithoutType());
            callerContext.Diagnostic.SetError(message);
            return false;
        }

        #endregion
        //-----------------------------------------------------------------------------------------------------
        public void CloseDocument(ICallerContext callerContext)
        {
            IDocument document = GetDocument(callerContext);
            if (document == null)
                return;

            if (StateSerializer != null)
                StateSerializer.Remove(document);

            CloseDocument(document);
        }

        //-----------------------------------------------------------------------------------------------------
        public void CloseComponent(ICallerContext callerContext)
        {
            IComponent component = GetComponent(callerContext);
            if (component == null)
                return;

             CloseComponent(component);
        }

        //-----------------------------------------------------------------------------------------------------
        private void CloseDocument(IDocument document)
        {
            IComponent component = document as IComponent;

            if (CanBeRecycled(component))
                return;

            if (Recycler == null || Recycler.IsRemovable(component))
                documents.Remove(document);
        }

        //-----------------------------------------------------------------------------------------------------
        private void CloseComponent(IComponent component)
        {
             if (CanBeRecycled(component))
                return;

            if (Recycler == null || Recycler.IsRemovable(component))
                components.Remove(component);
        }

        //-----------------------------------------------------------------------------------------------------
        private bool CanBeRecycled(IComponent component)
        {
            if (Recycler != null && Recycler.IsRecyclable(component))
            {
                Recycler.Recycle(component);
                return true;
            }
            return false;
        }
   }
}
