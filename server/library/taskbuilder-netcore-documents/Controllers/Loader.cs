using System;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Model;
using System.Runtime.Loader;
using System.Reflection;
using System.Collections.Generic;
using TaskBuilderNetCore.Interfaces;
using System.Linq;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Documents.Model.TbModel;

namespace TaskBuilderNetCore.Documents.Controllers
{

    //====================================================================================    
    [Name("Loader"), Description("It manages document loading process.")]
    public class Loader : Controller, ILoader
    {
        AssemblyLoader assemblyLoader;
        //-----------------------------------------------------------------------------------------------------
        public Loader()
        {
            Init();
        }

        //-----------------------------------------------------------------------------------------------------
        private void Init()
        {
            PathFinderChanged += Loader_PathFinderChanged;
            assemblyLoader = new AssemblyLoader(AppContext.BaseDirectory);
        }

        //-----------------------------------------------------------------------------------------------------
        private void Loader_PathFinderChanged(object sender, EventArgs e)
        {
            // TODOBRUNA si schianta il pathfinder
            /*  if (PathFinder != null)
              {
                  assemblyLoader.AssembliesPath = PathFinder.GetMagoNetApplicationPath();
                  }
              */
        }

        //-----------------------------------------------------------------------------------------------------
        public IComponent GetComponent
            (
                INameSpace nameSpace,
                ICallerContext callerContext,
                ILicenceConnector licenceConnector,
                IDocument document = null
            ) => GetComponent(nameSpace, callerContext, licenceConnector, document, document == null ? typeof(Component) : typeof(DocumentComponent));

        //-----------------------------------------------------------------------------------------------------
        private IComponent GetComponent
        (
                INameSpace nameSpace, 
                ICallerContext callerContext, 
                ILicenceConnector licenceConnector, 
                IDocument document = null, 
                Type baseType = null, 
                bool isDynamic = false,
                string typeName = null
            )
        {
            // controllo di licencing : componente non attivato
            if (!licenceConnector.IsActivated(nameSpace))
                return null;

            // gestione del caricamento 
            Type componentType = GetBestClass(nameSpace, baseType, isDynamic, typeName);
            if (componentType == null)
                throw new ControllerException(this, LoaderMessages.ClassNotFound, nameSpace.FullNameSpace);

            IComponent component = Activator.CreateInstance(componentType) as IComponent;

            // gestione del componente 
            if (component == null || !component.CanBeLoaded(callerContext))
            {
                component = null;
                return null;
            }

            // inizializzo il componente di documento se e' il caso
            if (component is DocumentComponent doc)
                doc.Document = document;

            component?.Initialize(callerContext);
            return component;
        }

        //-----------------------------------------------------------------------------------------------------
        private Type GetBestClass(INameSpace nameSpace, Type baseType, bool isDynamic = false, string typeName = null)
        {
            // istanziazione dinamica
            if (isDynamic)
                return string.IsNullOrEmpty(typeName) ? baseType : Type.GetType(typeName);
            
            // ricerca della classe negli assemblies
            return GetTypeFromAssemblies(nameSpace, baseType);
        }

        //-----------------------------------------------------------------------------------------------------
        public IDocument GetDocument(ICallerContext callerContext, ILicenceConnector licenceConnector)
        {
            // controllo di licencing
            if (!licenceConnector.IsActivated(callerContext.NameSpace))
                return null;

            // metadati
            IDocumentInfo info = PathFinder.GetDocumentInfo(callerContext.NameSpace);
            if (info == null)
                throw new ControllerException(this, LoaderMessages.DocumentNotDeclared, callerContext.NameSpace.FullNameSpace);

            Type documentType = GetBestClass(info.NameSpace, (info.IsBatch ? typeof(ActivityDocument) : typeof(Document)), info.IsDynamic, info.Classhierarchy);

            if (documentType == null)
                throw new ControllerException(this, LoaderMessages.ClassNotFound, callerContext.NameSpace.FullNameSpace);

            Document document = Activator.CreateInstance(documentType) as Document;

            if (
                    document != null &&
                    document.CanBeLoaded(callerContext) &&
                    document.Initialize(callerContext) &&
                    ComposeDocument(document, licenceConnector)
               )
                return document;

            document?.Dispose();
            document = null;

            throw new ControllerException(this, LoaderMessages.DocumentNotLoaded, callerContext.NameSpace.FullNameSpace);
        }

        //-----------------------------------------------------------------------------------------------------
        private bool ComposeDocument(IDocument document, ILicenceConnector licenceConnector)
        {
            IDocumentInfo info = PathFinder.GetDocumentInfo(document.NameSpace);
            if (info == null)
                throw new ControllerException(this, LoaderMessages.DocumentNotDeclared, document.NameSpace.FullNameSpace);

            document.Title = info.Title;

            // aggiungo i componenti dichiarati staticamente
            AddComponents(document, licenceConnector, info);

            // consento al documento di caricare i componenti anche programmativamente
            if (!document.LoadComponents())
                return false;

            // definizione di pipe line dei componenti
            ExecuteComponentsPipeline(document);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        private void AddComponents(IDocument document, ILicenceConnector licenceConnector, IDocumentInfo info)
        {
            // se ho dei componenti nella dichiarazione li istanzio in automatico
            if (info.Components != null)
                AddComponents(document, licenceConnector, info.Components);

            // ora vado a caricare i client doc e i relativi componenti
            List<ClientDocumentInfo> clientDocs = PathFinder.GetClientDocumentsFor(document);
            if (clientDocs != null)
            { 
                foreach (ClientDocumentInfo cInfo in clientDocs)
                {
                    // per primo guardo se devo creare la classe stessa di clientdoc
                    DocumentComponent component = GetComponent(cInfo.NameSpace, document.CallerContext, licenceConnector, document, typeof(ClientDoc), cInfo.IsDynamic, cInfo.OjectType) as DocumentComponent;
                    if (component != null)
                        document.Components.Add(component);
                    
                    // poi aggiungo gli eventuali components dichiarati in esso
                    if (cInfo.Components != null)
                        AddComponents(document, licenceConnector, cInfo.Components);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------
        private void AddComponents(IDocument document, ILicenceConnector licenceConnector, List<IDocumentInfoComponent> components)
        {
            // se ho dei componenti li istanzio in automatico
            foreach (IDocumentInfoComponent declaration in components)
            {
                if (declaration.NameSpace == null || string.IsNullOrEmpty(declaration.NameSpace.FullNameSpace))
                    continue;

                // controllo attivazione
                if (!string.IsNullOrEmpty(declaration.Activation) && !licenceConnector.IsActivated(declaration.Activation))
                    continue;

                DocumentComponent documentComponent = GetComponent(declaration.NameSpace, document.CallerContext, licenceConnector, document) as DocumentComponent;
                if (documentComponent != null)
                    document.Components.Add(documentComponent);
            }
        }

        //-----------------------------------------------------------------------------------------------------
        private void ExecuteComponentsPipeline(IDocument document)
        {
            // TODOBRUNA Inserire pipe line dei componenti
        }

        // si limita ad identificare l'assembly e a caricarlo
        // se cambiamo strategia di caricamento va qui
        //-----------------------------------------------------------------------------------------------------
        private Assembly LoadAssemblyFromNamespace(INameSpace nameSpace)
        {
            string assemblyName = System.IO.Path.Combine(assemblyLoader.AssebliesPath, nameSpace.Application + "-" + nameSpace.Module + "-" + nameSpace.Library + NameSolverStrings.DllExtension);
            Assembly assembly = null;
            try
            {
                assembly = assemblyLoader.LoadFromAssemblyPath(assemblyName);
            }
            catch (Exception e)
            {
                throw new ControllerException(this, LoaderMessages.LoadAssemblyError, new object[] { nameSpace.FullNameSpace, e.Message }, e);
            }
            return assembly;
        }


        //-----------------------------------------------------------------------------------------------------
        private Type GetTypeFromAssemblies(INameSpace nameSpace, Type baseType)
        {
            Assembly loadedAssembly = LoadAssemblyFromNamespace(nameSpace);

            if (loadedAssembly == null)
                throw new ControllerException(this, LoaderMessages.LoadAssemblyError, nameSpace.FullNameSpace);


            TypeInfo info = baseType.GetTypeInfo();
            foreach (Type t in loadedAssembly.GetTypes())
            {
                if (info.IsAssignableFrom(t))
                {
                    // se trovo l'attributo di namespace uso quello
                    NameSpaceAttribute nameAttr = t.GetTypeInfo().GetCustomAttribute<NameSpaceAttribute>();
                    if (nameAttr != null && string.Compare(nameAttr.Namespace, nameSpace.FullNameSpace, true) == 0)
                        return t;
                    
                    // altrimenti ammetto anche che il nome classe sia lo stesso
                    if (t.FullName.CompareTo(nameSpace.GetNameSpaceWithoutType()) == 0)
                        return t;
                }
            }
            throw new ControllerException(this, LoaderMessages.ClassNotFound, nameSpace.FullNameSpace);
        }
    }

    //====================================================================================    
    internal class AssemblyLoader : AssemblyLoadContext
    {
        List<Assembly> loadedAssemblies;
        string assebliesPath;
        public string AssebliesPath { get => assebliesPath; set => assebliesPath = value; }

        //-----------------------------------------------------------------------------------------------------
        public AssemblyLoader(string assebliesPath)
        {
            loadedAssemblies = new List<Assembly>();
            this.AssebliesPath = assebliesPath;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override Assembly Load(AssemblyName assemblyName)
        {
            Assembly assembly = loadedAssemblies.Where(m => m.FullName == assemblyName.Name).SingleOrDefault();
            if (assembly != null)
                return assembly;

            string alternativePath = System.IO.Path.Combine(AssebliesPath, String.Concat(assemblyName.Name, NameSolverStrings.DllExtension));
            try
            {
                assembly  = Assembly.Load(assemblyName);
                if (assembly == null)
                    assembly = Assembly.LoadFrom(alternativePath);

             }
            catch (System.IO.FileNotFoundException e)
            {
                assembly = Assembly.LoadFrom(alternativePath);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (assembly != null)
                    loadedAssemblies.Add(assembly);
            }

            return assembly;
        }
    }
}