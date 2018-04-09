using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Microarea.EasyBuilder.CodeEditorProviders;
using Microarea.EasyBuilder.Localization;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.EasyBuilder
{
    //=========================================================================
    /// <summary>
    /// Manages references to build
    /// </summary>
    public abstract class Sources : IDisposable
    {

        // Dictionary<string, ICompilationUnit> compilationUnitCache = new Dictionary<string, ICompilationUnit>();

        private NewCustomizationInfos newCustomizationInfos;

        private ApplicationType applicationType;
        private LocalizationSources localizationSources = new LocalizationSources();
        private SourcesCustomizationInfoManager customizationInfosManager = new SourcesCustomizationInfoManager();

        /// <remarks/>
        private string applicationName;
        /// <remarks/>
        private string moduleName;
        /// <remarks />
        public string ApplicationName { get { return applicationName; } }
        /// <remarks />
        public string ModuleName { get { return moduleName; } }

        private IProjectContent projectContent;

        /// <remarks />
        public IProjectContent ProjectContent { get { return projectContent; } set { projectContent = value; } }

        /// <summary>
        /// Internal use: stuff for localization.
        /// </summary>
        //--------------------------------------------------------------------------------
        public Dictionaries Dictionaries
        {
            get { return localizationSources.Dictionaries; }
        }
        /// <summary>
        /// Gets a value copy of the namespace for the current customization
        /// </summary>
        /// <seealso cref="Microarea.TaskBuilderNet.Core.Generic.NameSpace"/>
        //--------------------------------------------------------------------------------
        public NameSpace Namespace
        {
            get { return newCustomizationInfos.Namespace; }
            set { newCustomizationInfos.Namespace = value; }
        }

        /// <summary>
        /// Internal use: stuff for localization.
        /// </summary>
        //--------------------------------------------------------------------------------
        public LocalizationSources Localization
        {
            get { return localizationSources; }
        }

        /// <remarks />
        protected abstract SyntaxTree InitCodeNamespaceMethods(INameSpace customizationNamespace);
        /// <remarks />
        protected abstract bool IsSubclassOfController(Type t);

        /// <remarks />
        protected abstract void DecorateMethod(MethodDeclaration eventHandlerMethod, IList<Statement> initialCode);

        /// <summary>
        /// Occurs when source code generate by EasyBuilder changed.
        /// </summary>
        public event EventHandler<CodeChangedEventArgs> CodeChanged;

        /// <summary>
        /// Occurs when references to assembly are updated.
        /// </summary>
        public event EventHandler<ReferenceUpdatedEventArgs> ReferenceUpdated;

        /// <summary>
        /// Occurs when the application ends to regenerate controller sources
        /// </summary>
        public event EventHandler<EventArgs> SourcesUpdated;

        /// <summary>
        /// Occurs when a programmative method is going to be modified by the code editor.
        /// </summary>
        public event EventHandler<CodeMethodEditorEventArgs> CodeMethodEdit;

        /// <summary>
        /// Invia un messaggio che avvisa i sottoscrittori che il codice ha subito un cambiamento
        /// </summary>
        /// <param name="args"></param>
        //--------------------------------------------------------------------------------
        internal void OnCodeMethodEdit(CodeMethodEditorEventArgs args)
        {
            if (CodeMethodEdit != null)
                CodeMethodEdit(this, args);
        }

        ///<remarks />
        //-------------------------------------------------------------------------------
        protected virtual void OnSourcesUpdated()
        {
            if (SourcesUpdated != null)
                SourcesUpdated(this, new EventArgs());
        }

        /// <summary>
        /// Fired when references are updated
        /// </summary>
        /// <param name="args"></param>
        //-------------------------------------------------------------------------------
        protected virtual void OnReferenceUpdated(ReferenceUpdatedEventArgs args)
        {
            if (ReferenceUpdated != null)
                ReferenceUpdated(this, args);
        }

        /// <summary>
        /// Fired when code changes
        /// </summary>
        //-------------------------------------------------------------------------------
        internal virtual void OnCodeChanged()
        {
            OnCodeChanged(new CodeChangedEventArgs(ChangeType.CodeChanged));
        }

        /// <summary>
        /// Fired when code changes
        /// </summary>
        //-------------------------------------------------------------------------------
        protected virtual void OnCodeChanged(CodeChangedEventArgs e)
        {
            if (CodeChanged != null)
                CodeChanged(this, e);
        }

        /// <remarks />
        //--------------------------------------------------------------------------------
        public ApplicationType ApplicationType
        {
            get { return applicationType; }
            protected set { applicationType = value; }
        }

        /// <summary>
        /// NewCustomizationInfos
        /// </summary>
        //--------------------------------------------------------------------------------
        internal NewCustomizationInfos CustomizationInfos
        {
            get { return newCustomizationInfos; }
            set { newCustomizationInfos = value; }
        }

        //--------------------------------------------------------------------------------
        internal virtual string GetUserMethodsFileCode()
        {
            return newCustomizationInfos.UserMethodsCode;
        }

        //-------------------------------------------------------------------------------
        /// <summary>
        /// Creates a new instance of the reference manager
        /// </summary>
        protected Sources(
            string applicationName,
            string moduleName,
            ApplicationType applicationType
            )
        {
            this.applicationName = applicationName;
            this.applicationType = applicationType;
            this.moduleName = moduleName;

            localizationSources.CodeChanged += new EventHandler<CodeChangedEventArgs>(
                   (object sender, CodeChangedEventArgs e)
                   =>
                   {
                       OnCodeChanged();
                   }
               );
            localizationSources.AddLocalizableLanguage("");//dizionario invariant di default

            BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderAppChanged += new EventHandler<EventArgs>(CurrentEasyBuilderAppChanged);
        }

        //-------------------------------------------------------------------------------
        private void CurrentEasyBuilderAppChanged(object sender, EventArgs e)
        {
            applicationName = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName;
            moduleName = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName;
        }

        // Se il controller proviene da una dll, deserializza la classe codenamespace da embedded resource, 
        // altrimenti la genera ispezionando la classe controller
        ///<remarks />
        //-------------------------------------------------------------------------------
        public void Init(INameSpace customizationNamespace, Assembly asm)
        {
            INameSpace safeEscapedNamespace = new NameSpace(EasyBuilderSerializer.Escape(customizationNamespace.FullNameSpace));
            projectContent = ProjectContentFacilities.GetDefaultProjectContent(safeEscapedNamespace.Leaf);

            if (asm != null && asm != Assembly.GetExecutingAssembly())
            {
                newCustomizationInfos = customizationInfosManager.LoadCustomizationInfos(customizationNamespace, asm);
            }
            else
            {
                newCustomizationInfos = new NewCustomizationInfos(InitCodeNamespaceMethods(safeEscapedNamespace));
                newCustomizationInfos.InitCodeNamespace(customizationNamespace);
                newCustomizationInfos.UserMethodsCode = AstFacilities.GetVisitedText(newCustomizationInfos.UserMethodsCompilationUnit);
            }

            newCustomizationInfos.RefreshUsings();
            RefreshReferencedAssemblies(false);

            Functions.LowPriorityThread(() =>
            {
                IntellisenseExcludeList i = IntellisenseExcludeList.Instance;
            });

            Functions.LowPriorityThread(() =>
            {
                //pre-carica gli assembly del presentation framework, in modo da non avere rallentamenti alla prima apertura dell'intellisense nel codeeditor
                PreLoadAdditionalDll();
            });
        }

        //-------------------------------------------------------------------------------
        private static void PreLoadAdditionalDll()
        {
            try
            {
                Assembly.Load("PresentationFramework-SystemXml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                Assembly.Load("PresentationFramework-SystemData, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                Assembly.Load("PresentationFramework-SystemXmlLinq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                Assembly.Load("System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                Assembly.Load("UIAutomationTypes, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Assert(false, e.Message);
            }
        }

        //-------------------------------------------------------------------------------
        internal void RemoveReferencedAssembly(string dllName, bool signalReferenceUpdated = false)
        {
            string path = PathFinderWrapper.GetEasyStudioReferenceAssembliesPath();

            foreach (IAssemblyReference content in ProjectContent.AssemblyReferences)
            {
                DefaultUnresolvedAssembly reference = content as DefaultUnresolvedAssembly;
                if (reference == null)
                    continue;

                if (
                    reference.AssemblyName == dllName &&
                    reference.Location.StartsWith(path, StringComparison.InvariantCultureIgnoreCase)
                    )
                {
                    projectContent = projectContent.RemoveAssemblyReferences(reference);
                    foreach (IUnresolvedTypeDefinition t in reference.GetAllTypeDefinitions())
                    {
                        if (!t.IsPublic)
                            continue;

                        newCustomizationInfos.RemoveUsing(t.Namespace);
                    }
                }
            }

            if (signalReferenceUpdated)
                OnCodeChanged();
        }

        //-------------------------------------------------------------------------------
        internal void RefreshReferencedAssemblies(bool removeExisting)
        {
            string path = PathFinderWrapper.GetEasyStudioReferenceAssembliesPath();
            if (removeExisting)
            {
                //prima li tolgo tutti e poi li rimetto così gestisco le cancellazioni
                foreach (IAssemblyReference content in ProjectContent.AssemblyReferences)
                {
                    DefaultUnresolvedAssembly reference = content as DefaultUnresolvedAssembly;
                    if (reference == null)
                        continue;

                    if (reference.Location.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                        RemoveReferencedAssembly(reference.AssemblyName);
                }
            }

            newCustomizationInfos.RefreshUsings();

            if (!Directory.Exists(path))
                return;

            foreach (string asmFile in Directory.GetFiles(path, "*.dll"))
            {
                //inserisco nel project content l'assembly che ho appena aggiunto
                AddAssemblyToProjectContent(asmFile);
            }

        }

        //-------------------------------------------------------------------------------
        /// <remarks/>
        public void AddAssemblyToProjectContent(string file)
        {
            if (String.IsNullOrEmpty(file))
                return;

            var loader = new CecilLoader();
            loader.DocumentationProvider = ProjectContentFacilities.GetXmlDocumentation(file);
            var unresolvedAssembly = loader.LoadAssemblyFile(file);
            List<IAssemblyReference> assemblies = projectContent.AssemblyReferences.ToList();
            if (assemblies.Contains(unresolvedAssembly))
                return;

            projectContent = projectContent.AddAssemblyReferences(unresolvedAssembly);

            List<string> namespaces = new List<string>();
            foreach (IUnresolvedTypeDefinition t in unresolvedAssembly.GetAllTypeDefinitions())
                if (t.IsPublic && !t.Namespace.IsNullOrEmpty())
                    namespaces.Add(t.Namespace);

            newCustomizationInfos.AddUsings(namespaces.Distinct().ToList());
        }


        /// <summary>
        /// Dispose
        /// </summary>
        //-------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        //-------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderAppChanged -= new EventHandler<EventArgs>(CurrentEasyBuilderAppChanged);

            EventHandlers.RemoveEventHandlers(ref CodeChanged);
            EventHandlers.RemoveEventHandlers(ref ReferenceUpdated);
            EventHandlers.RemoveEventHandlers(ref SourcesUpdated);
        }

        //-------------------------------------------------------------------------------
        /// <summary>
        /// Initialize additional namespace
        /// </summary>
        public SyntaxTree InitAdditionalNamespace(string @namespace)
        {
            SyntaxTree cu = CustomizationInfos.InitAdditionalCompilationUnit(@namespace);
            Localization.AddResourceManager(CustomizationInfos, cu);

            return cu;
        }

        /// <summary>
        /// Removes an additional namespace
        /// </summary>
        /// <param name="nameSpace">The namespace to be used for the current customization</param>
        //-------------------------------------------------------------------------------
        public void RemoveAdditionalNamespace(string nameSpace)
        {
            IUnresolvedFile file = CustomizationInfos.RemoveAdditionalNamespace(nameSpace);
            if (file != null)
                projectContent = projectContent.RemoveFiles(file.FileName);
            //TODO eliminare file cs di BO
        }

        //--------------------------------------------------------------------------------
        /// <remarks/>
        public void SerializeBusinessObject(BusinessObject businessObject)
        {
            SyntaxTree cu = InitAdditionalNamespace(businessObject.Namespace.FullNameSpace);
            // Sources.SerializeBusinessObjectController(cu, new MVC.DocumentController(businessObject));
            SourcesSerializer.GenerateClass(cu, businessObject);

            UpdateProjectContent(cu);


            OnCodeChanged();
        }

        //--------------------------------------------------------------------------------
        /// <remarks/>
        public void UpdateProjectContent(SyntaxTree sintaxTree)
        {
            projectContent = projectContent.AddOrUpdateFiles(sintaxTree.ToTypeSystem());
        }

        //--------------------------------------------------------------------------------
        /// <remarks/>
        public virtual void SerializeHotlink(MHotLink hotlink)
        {
            SourcesSerializer.GenerateClass(CustomizationInfos.EbDesignerCompilationUnit, hotlink);

            OnCodeChanged();
        }

        //--------------------------------------------------------------------------------
        /// <remarks/>
        public virtual void SerializeDataManager(MDataManager dataManager)
        {
            SourcesSerializer.GenerateClass(CustomizationInfos.EbDesignerCompilationUnit, dataManager);

            OnCodeChanged();
        }


        //--------------------------------------------------------------------------------
        internal MethodDeclaration GetUpdatedUserMethod(MethodDeclaration oldMethod)
        {
            if (oldMethod == null)
                return null;

            foreach (var method in GetUserMethods())
            {
                if (oldMethod.IsEqualTo(method))
                {
                    return method;
                }
            }

            return null;
        }

        /// <remarks />
        /// <summary>
        /// Returns all the user method added to the module.
        /// </summary>
        /// <returns></returns>
        //--------------------------------------------------------------------------------
        public virtual List<MethodDeclaration> GetUserMethods()
        {
            //Attualmente il modulo non ha metodi scritti dall'utente
            return new List<MethodDeclaration>();
        }

        //-------------------------------------------------------------------------------
        /// <remarks />
        protected List<MethodDeclaration> GetUserMethods(SyntaxTree cu)
        {
            NamespaceDeclaration ns = EasyBuilderSerializer.GetNamespaceDeclaration(cu);

            List<MethodDeclaration> methods = new List<MethodDeclaration>();

            TypeDeclaration oldControllerType = EasyBuilderSerializer.GetControllerTypeDeclaration(cu);
            MethodDeclaration method = null;
            if (oldControllerType == null)
                return methods;

            foreach (EntityDeclaration member in oldControllerType.Members)
            {
                method = member as MethodDeclaration;
                if (method == null)
                    continue;

                methods.Add(method);
            }
            return methods;
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Add an attribute to the given type
        /// </summary>
        /// <param name="aClass"></param>
        /// <param name="attr"></param>
        //-----------------------------------------------------------------------------
        public void AddCustomAttribute(TypeDeclaration aClass, AttributeSection attr)
        {
            aClass.Attributes.Add(attr);
            OnCodeChanged();
        }

        /// <summary>
        /// Retrieves an event handler given its name and the event described by the given event descriptor.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        //--------------------------------------------------------------------------------
        public MethodDeclaration FindCodeMemberMethod(string methodName, EventDescriptor e)
        {
            foreach (MethodDeclaration method in GetUserMethods())
            {
                if (
                    String.Compare(methodName, method.Name, StringComparison.InvariantCulture) == 0 &&
                    MatchParameters(method, e)
                    )
                    return method;
            }

            return null;
        }

        //-------------------------------------------------------------------------------
        /// <summary>
        /// Ritorna la dichiarazione di classe a partire dal suo nome
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal TypeDeclaration FindClass(string name)
        {
            return EasyBuilderSerializer.FindClass(CustomizationInfos.EbDesignerCompilationUnit, name);
        }

        /// <summary>
        /// Return true if method is present among user methods, otherwise false
        /// </summary>
        //--------------------------------------------------------------------------------
        public bool ContainsCodeMemberMethod(MethodDeclaration method)
        {
            if (method == null)
                return false;

            foreach (MethodDeclaration m in GetUserMethods())
            {
                if (m.IsEqualTo(method))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// True se i parametri del metodo fornito sono identici a quelli specificati 
        /// nell'eventdescriptor
        /// </summary>
        //--------------------------------------------------------------------------------
        private static bool MatchParameters(MethodDeclaration method, EventDescriptor e)
        {
            Type[] parameterTypes = ReflectionUtils.GetParametersTypes(e);

            if (method.Parameters.Count != parameterTypes.Length)
                return false;

            bool areEquals = true;

            List<ParameterDeclaration> parameters = method.Parameters.ToList();
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i].Name != parameters[i].Type.AstTypeToString())
                {
                    areEquals = false;
                    break;
                }
            }
            return areEquals;
        }

        /// <summary>
        /// Returns a collection of method names whose signature is compatible with the
        /// event described by the given event descriptor.
        /// </summary>
        //--------------------------------------------------------------------------------
        public ICollection FindCompatibleMethodsName(EventDescriptor e)
        {
            List<string> methods = new List<string>();

            foreach (MethodDeclaration method in GetUserMethods())
            {
                if (MatchParameters(method, e))
                    methods.Add(method.Name);
            }

            return methods;
        }

        /// <summary>
        /// Removes an event handler registration.
        /// </summary>
        /// <param name="toBeRemovedMethod">The code member method representing the event handler to be unsubscribed</param>
        /// <param name="component">The component raising the event</param>
        //--------------------------------------------------------------------------------
        public bool RemoveEventHandlerRegistration(
            MethodDeclaration toBeRemovedMethod,
            EasyBuilderComponent component
            )
        {
            IList<TaskBuilderNet.Core.EasyBuilder.EventInfo> eventInfos = EasyBuilderComponent.GetEventInfosFromCache(toBeRemovedMethod.Name);
            if (eventInfos != null && eventInfos.Count > 0)
            {
                bool ok = true;
                foreach (var ei in eventInfos)
                {
                    ok &= RemoveEventHandlerRegistration(ei, toBeRemovedMethod, component);
                }

                return ok;
            }

            return false;
        }

        /// <summary>
        /// Removes an event handler registration.
        /// </summary>
        /// <param name="toBeReadded">The code member method representing the event handler to be unsubscribed</param>
        /// <param name="modelRoot">The controller</param>
        //--------------------------------------------------------------------------------
        public bool RestoreEventHandlerRegistration(IModelRoot modelRoot, MethodDeclaration toBeReadded)
        {
            IList<TaskBuilderNet.Core.EasyBuilder.EventInfo> eventInfos = EasyBuilderComponent.GetEventInfosFromCache(toBeReadded.Name);
            if (eventInfos == null || eventInfos.Count <= 0)
                return false;

            EasyBuilderComponent component = null;
            bool ok = true;
            foreach (var ei in eventInfos)
            {
                //casistiche: rimuovo evento da controller
                if (ei.ComponentFullPath.IsNullOrEmpty())
                    component = modelRoot as EasyBuilderComponent;  //rimuovo evento da controller
                else
                    component = ReflectionUtils.GetComponentFromPath(modelRoot as IEasyBuilderContainer, ei.ComponentFullPath) as EasyBuilderComponent;
                //oppure da un generico component.
                //in questo caso, sfrutto l'interfaccia IChangedEventsSource che chiede al component quale oggetto recepisce la registrazione dell'evento
                //serve perchè i value changed di dataobj veniva associati al sqlrecord item
                var changeEventsSource = component as IChangedEventsSource;
                component = changeEventsSource?.EventSourceComponent as EasyBuilderComponent;

                ok &= AddEventHandlerRegistration(ei, toBeReadded, component);
            }

            return ok;
        }


        /// <summary>
        /// Removes an event handler registration.
        /// </summary>
        /// <param name="toBeRemovedMethod">The code member method representing the event handler to be unsubscribed</param>
        /// <param name="modelRoot">The controller</param>
        //--------------------------------------------------------------------------------
        public bool RemoveEventHandlerRegistration(IModelRoot modelRoot, MethodDeclaration toBeRemovedMethod)
        {
            IList<TaskBuilderNet.Core.EasyBuilder.EventInfo> eventInfos = EasyBuilderComponent.GetEventInfosFromCache(toBeRemovedMethod.Name);
            if (eventInfos == null || eventInfos.Count <= 0)
                return false;

            EasyBuilderComponent component = null;
            bool ok = true;
            foreach (var ei in eventInfos)
            {
                //casistiche: rimuovo evento da controller
                if (ei.ComponentFullPath.IsNullOrEmpty())
                    component = modelRoot as EasyBuilderComponent;  //rimuovo evento da controller
                else
                    component = ReflectionUtils.GetComponentFromPath(modelRoot as IEasyBuilderContainer, ei.ComponentFullPath) as EasyBuilderComponent;
                //oppure da un generico component.
                //in questo caso, sfrutto l'interfaccia IChangedEventsSource che chiede al component quale oggetto recepisce la registrazione dell'evento
                //serve perchè i value changed di dataobj veniva associati al sqlrecord item
                var changeEventsSource = component as IChangedEventsSource;
                component = changeEventsSource?.EventSourceComponent as EasyBuilderComponent;

                ok &= RemoveEventHandlerRegistration(ei, toBeRemovedMethod, component);
            }

            return ok;
        }

        /// <summary>
        /// Removes an event handler registration.
        /// </summary>
        /// <param name="eventInfo">A structure describing the event to unsubscribe</param>
        /// <param name="toBeAdded">The code member method representing the event handler to be subscribed</param>
        /// <param name="component">The component raising the event</param>
        //--------------------------------------------------------------------------------
        public bool AddEventHandlerRegistration(
            Microarea.TaskBuilderNet.Core.EasyBuilder.EventInfo eventInfo,
            MethodDeclaration toBeAdded,
            EasyBuilderComponent component
            )
        {
            bool codeChanged = false;
            //Se il nome del tipo che ha la registrazione del metodo non è nella collezione UserData del MethodDeclaration
            //allora per default agisco sul controller,
            //altrimenti agisco sul tipo specificato
            var relatedComponent = component.EventSourceComponent as EasyBuilderComponent;
            if (relatedComponent != null)
            {
                string tag = TaskBuilderNet.Core.EasyBuilder.EventInfo.CalculateFullEventName(relatedComponent, eventInfo.SourceEvent);
                if (eventInfo != null)
                {
                    relatedComponent.AddChangedEvent(eventInfo);

                    codeChanged = true;
                }
            }

            if (codeChanged)
                OnCodeChanged(new CodeChangedEventArgs(ChangeType.CodeChanged, toBeAdded));

            return codeChanged;
        }


        /// <summary>
        /// Removes an event handler registration.
        /// </summary>
        /// <param name="eventInfo">A structure describing the event to unsubscribe</param>
        /// <param name="toBeRemovedMethod">The code member method representing the event handler to be unsubscribed</param>
        /// <param name="component">The component raising the event</param>
        //--------------------------------------------------------------------------------
        public bool RemoveEventHandlerRegistration(
            Microarea.TaskBuilderNet.Core.EasyBuilder.EventInfo eventInfo,
            MethodDeclaration toBeRemovedMethod,
            EasyBuilderComponent component
            )
        {
            bool codeChanged = false;
            //Se il nome del tipo che ha la registrazione del metodo non è nella collezione UserData del MethodDeclaration
            //allora per default agisco sul controller,
            //altrimenti agisco sul tipo specificato
            var relatedComponent = component.EventSourceComponent as EasyBuilderComponent;
            if (relatedComponent != null)
            {
                string tag = TaskBuilderNet.Core.EasyBuilder.EventInfo.CalculateFullEventName(relatedComponent, eventInfo.SourceEvent);
                if (eventInfo != null)
                {
                    relatedComponent.RemoveChangedEvent(eventInfo);

                    codeChanged = true;
                }
            }

            if (codeChanged)
                OnCodeChanged(new CodeChangedEventArgs(ChangeType.CodeChanged, toBeRemovedMethod));

            return codeChanged;
        }

        /// <summary>
        /// Returns a value indicating if there is an event handler registration.
        /// </summary>
        /// <param name="methodName">The event handler name to look for the registration.</param>
        /// <param name="eventName">The event name subscribed.</param>
        /// <param name="ebComponent">The component raising the event.</param>
        /// <returns></returns>
        //--------------------------------------------------------------------------------
        public bool IsThereEventHandlerRegistrationStatement(
            string methodName,
            string eventName,
            EasyBuilderComponent ebComponent//component che effettivamente avrebbe appiccicato l'evento ma di cui non posso scrivrere il  += perche` il suo papa` e` lazy.
            )
        {
            bool found = false;

            string[] tokens = null;
            string controlName = null;
            string sourceEventName = null;
            string controlNameToBeSearched = ebComponent.SerializedName;
            if (
                (controlNameToBeSearched == null || controlNameToBeSearched.Trim().Length == 0) &&
                ebComponent != null &&
                ebComponent.Site != null
                )
                controlNameToBeSearched = ebComponent.Site.Name;

            //logica un po' perversa...gli eventi nel caso di di dataobj, sono associati al dataobj  stesso, ma il component che arriva è il MSqlRecordItem.
            //Abbiamo aggiunto EventSourceComponent che torna this per tutti, tranne che per il MSqlRecordItem, che invece torna il dataobj
            EasyBuilderComponent eb = ebComponent.EventSourceComponent as EasyBuilderComponent;
	        if (eb == null)
				return false;

	        foreach (Microarea.TaskBuilderNet.Core.EasyBuilder.EventInfo eventInfo in eb.ChangedEvents)
	        {
		        tokens = eventInfo.SourceEvent.Split('.');
		        controlName = tokens[tokens.Length - 2];
		        sourceEventName = tokens[tokens.Length - 1];
		        if (
			        controlName.CompareNoCase(controlNameToBeSearched) &&
			        sourceEventName.CompareNoCase(eventName) &&
			        methodName.CompareNoCase(eventInfo.EventHandlerName)
		        )
		        {
			        found = true;
			        break;
		        }
	        }

	        return found;
        }

        /// <summary>
        /// Gets a string containing al the source code for the customization
        /// </summary>
        //--------------------------------------------------------------------------------
        internal SourceFile[] GetAllCode()
        {
            List<SourceFile> sourceFiles = new List<SourceFile>();

            //Creo il file di metadati
            SourceFile metadataInfoSourceFile = new SourceFile(NewCustomizationInfos.MetaSourceFileExtension, AstFacilities.GetVisitedText(CustomizationInfos.MetadataInfoCompilationUnit), this.Namespace);
            sourceFiles.Add(metadataInfoSourceFile);

            //Creo il file contenente il codice serializzato del controller (niente metodi personalizzati)
            GenerateOneSourceFilePerTypeDeclaration(sourceFiles, CustomizationInfos.EbDesignerCompilationUnit);

            //Creo il file contenente il codice degli user method del controller 
            SourceFile userMethodsSourceFile = new SourceFile(NewCustomizationInfos.UserMethodsSourceFileExtension, GetUserMethodsFileCode(), this.Namespace);
            sourceFiles.Add(userMethodsSourceFile);

            NamespaceDeclaration additionalNs = null;
            SourceFile additionalSourceFile = null;
            StringBuilder sb = null;
            foreach (SyntaxTree additionalCu in CustomizationInfos.AdditionalCompilationUnits)
            {
                sb = new StringBuilder();
                additionalNs = EasyBuilderSerializer.GetNamespaceDeclaration(additionalCu);

                sb.Append(AstFacilities.GetVisitedText(additionalCu));

                additionalSourceFile = new SourceFile(additionalNs.Name, sb.ToString(), this.Namespace);
                sourceFiles.Add(additionalSourceFile);
            }

            return sourceFiles.ToArray();
        }

        //--------------------------------------------------------------------------------
        private void GenerateOneSourceFilePerTypeDeclaration(
            List<SourceFile> sourceFiles,
            SyntaxTree compilationUnit
            )
        {
            StringBuilder usingsBuilder = new StringBuilder();
            UsingDeclaration ud = null;
            foreach (AstNode current in compilationUnit.Members)
            {
                ud = current as UsingDeclaration;
                if (ud == null)
                    continue;
                usingsBuilder.Append(AstFacilities.GetVisitedText(ud));
            }

            NamespaceDeclaration ns = null;
            TypeDeclaration td = null;
            NamespaceDeclaration tempNs = null;
            foreach (AstNode current in compilationUnit.Members)
            {
                ns = current as NamespaceDeclaration;
                if (ns == null)
                    continue;
                foreach (var nsChild in ns.Members)
                {
                    td = nsChild as TypeDeclaration;
	                if (td == null)
						continue;

	                td = td.Clone() as TypeDeclaration;
	                tempNs = new NamespaceDeclaration(ns.Name);
	                tempNs.Members.Add(td);
	                StringBuilder sb = new StringBuilder();
	                sb.Append(usingsBuilder);
	                sb.Append(AstFacilities.GetVisitedText(tempNs));
	                sourceFiles.Add(new SourceFile(td.Name, sb.ToString(), this.Namespace));
                }
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Generate sources files
        /// </summary>
        public string GetUserMethodsFilePath()
        {
            return GetCustomizedFilesPath(
                this.Namespace,
                String.Concat(NewCustomizationInfos.UserMethodsSourceFileExtension, NewCustomizationInfos.CSSourceFileExtension));
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Generate sources files
        /// </summary>
        public virtual IEnumerable<string> GenerateSources(string filePath)
        {
            string pathToDesigner = GetCustomizedPartialPath(this.Namespace);
            DirectoryInfo ebSrcDirInfo = new DirectoryInfo(pathToDesigner);
            DirectoryInfo srcDirInfo = new DirectoryInfo(Path.GetDirectoryName(pathToDesigner));

            if (ebSrcDirInfo.Exists)
            {
                var workingDirInfo = new DirectoryInfo(ebSrcDirInfo.FullName);
                var destDirInfo = new DirectoryInfo(Path.Combine(srcDirInfo.FullName, String.Concat(ebSrcDirInfo, "_bak")));
                if (destDirInfo.Exists)
                {
                    destDirInfo.Delete(true);
                }
                workingDirInfo.MoveTo(destDirInfo.FullName);
            }

            ebSrcDirInfo.Create();

            foreach (var sourceFile in GetAllCode())
            {
                string sourceFilePath = String.Concat(Path.Combine(sourceFile.FileName), NewCustomizationInfos.CSSourceFileExtension);
                using (StreamWriter sw = new StreamWriter(sourceFilePath))
                {
                    sw.Write(sourceFile.Content);
                    yield return sourceFilePath;
                }
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public void InitializeProjectContent(string customizationNamespaceLeaf)
        {
            SourceFile[] files = GetAllCode();
            projectContent = null;
            projectContent = ProjectContentFacilities.GetDefaultProjectContent(customizationNamespaceLeaf);

            foreach (var item in files)
            {
                CSharpParser parser = new CSharpParser();
                SyntaxTree syntaxTree = parser.Parse(new StringReader(item.Content), item.Name);
                if (syntaxTree.Errors.Count > 0)
                    continue;

                if (item.Name.Contains(NewCustomizationInfos.UserMethodsSourceFileExtension))
                {
                    CustomizationInfos.UpdateUserMethodsCompilationUnit(syntaxTree);
                }

                try
                {
                    UpdateProjectContent(syntaxTree);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            RefreshReferencedAssemblies(false);
        }


        //-------------------------------------------------------------------------------
        /// <remarks />
        public virtual string[] GetSourcesFileNames(string filePath)
        {
            string moduleCodeFile, metaCodeFile, moduleMethodsCodeFile;

            moduleCodeFile = Path.GetFileName(Path.ChangeExtension(filePath, NewCustomizationInfos.CSSourceFileExtension));
            metaCodeFile = Path.GetFileName(Path.ChangeExtension(filePath, NewCustomizationInfos.MetaSourceFileExtension + NewCustomizationInfos.CSSourceFileExtension));
            moduleMethodsCodeFile = Path.GetFileName(Path.ChangeExtension(filePath, NewCustomizationInfos.UserMethodsSourceFileExtension + NewCustomizationInfos.CSSourceFileExtension));

            List<string> sourcesFileNames = new List<string>(new string[] { moduleCodeFile, metaCodeFile, moduleMethodsCodeFile });

            foreach (var additionalCu in CustomizationInfos.AdditionalCompilationUnits)
            {
                sourcesFileNames.Add(
                        String.Concat(BasePathFinder.ChangeExtension(filePath, string.Empty), EasyBuilderSerializer.GetNamespaceDeclaration(additionalCu).Name, NewCustomizationInfos.CSSourceFileExtension)
                    );
            }

            return sourcesFileNames.ToArray();
        }

        //--------------------------------------------------------------------------------
        /// <remarks />
        protected MethodInfo GetMethodInfo(
            string methodName,
            Type controllerType,
            Assembly assembly
            )
        {
            MethodInfo methodInfo = null;
            if (assembly == null)
                methodInfo = controllerType.GetMethod(methodName);
            else
                foreach (Type t in assembly.GetTypes())
                {
                    if (!IsSubclassOfController(t))
                        continue;

                    methodInfo = t.GetMethod(methodName);
                }

            return methodInfo;
        }

        /// <remarks/>
        //--------------------------------------------------------------------------------
        public List<EasyBuilderComponent> GetComponentsToUpdate(
            string methodName,
            IContainer controller,
            Assembly assembly
            )
        {
            List<EasyBuilderComponent> componentsToUpdate = new List<EasyBuilderComponent>();
            componentsToUpdate.AddRange(EasyBuilderComponent.ClearReferences(controller, methodName));

            MethodInfo meth = GetMethodInfo(methodName, controller.GetType(), assembly);
            if (meth == null)
                return componentsToUpdate;

            var referencedVariables = GetNetReferencedVariables(meth);

            foreach (var rv in referencedVariables)
            {
                foreach (EasyBuilderComponent cmp in EasyBuilderComponent.GetComponentsBySerializedName(controller, rv.Name))
                {
                    if (string.Compare(rv.TypeName, cmp.SerializedType, StringComparison.InvariantCulture) == 0)
                    {
                        cmp.AddReferencedBy(methodName);
                        if (!componentsToUpdate.Contains(cmp))
                            componentsToUpdate.Add(cmp);
                    }
                }
            }
            return componentsToUpdate;
        }

        private class ReferencedVariable
        {
            public string Name { get; set; }
            public string TypeName { get; set; }
        }

        //--------------------------------------------------------------------------------
        private List<ReferencedVariable> GetNetReferencedVariables(MethodInfo method)
        {
            List<ReferencedVariable> tokens = new List<ReferencedVariable>();
            try
            {
                MsilReader reader = new MsilReader(method);
                while (reader.Read())
                {
                    MsilInstruction ins = reader.Current;
                    if (ins.Data == null)
                        continue;

                    FieldInfo field = ins.Data as FieldInfo;
                    if (field != null)
                    {
                        var rv = new ReferencedVariable() { Name = field.Name, TypeName = field.FieldType.Name };
                        if (!tokens.Contains(rv))
                            tokens.Add(rv);

                        continue;
                    }

                    MethodInfo meth = ins.Data as MethodInfo;
                    if (meth != null && meth.DeclaringType?.Assembly == method.DeclaringType?.Assembly)
                    {
                        string methName = meth.Name;
                        if (meth.IsSpecialName && (methName.StartsWith("get_") || methName.StartsWith("set_")))
                            methName = methName.Substring(4);

                        var rv = new ReferencedVariable() { Name = methName, TypeName = meth.ReturnType.Name };
                        if (!tokens.Contains(rv))
                            tokens.Add(rv);
                    }
                }
            }
            catch
            {
            }
            return tokens;
        }

        //--------------------------------------------------------------------------------
        internal static BlockStatement GetUserMethodTryCatchContent(MethodDeclaration method)
        {
            TryCatchStatement tryCatchStatement = null;
            foreach (var stat in method.Body.Statements)
            {
                tryCatchStatement = stat as TryCatchStatement;
                if (tryCatchStatement != null)
                    return tryCatchStatement.TryBlock;
            }

            return null;
        }

        //--------------------------------------------------------------------------------
        private MethodDeclaration GetMethodByName(string methodName)
        {
            MethodDeclaration desiredMethod = null;
            foreach (MethodDeclaration item in GetUserMethods())
            {
                if (item.Name.CompareNoCase(methodName))
                    desiredMethod = item;
            }
            return desiredMethod;
        }

        /// <summary>
        /// Aggiunge il metodo al controller se non è già presente.
        /// Non fa nulla altrimenti.
        /// </summary>
        /// <returns>Il metodo aggiunto (se creato) o ritrovato (se già esistente).</returns>
        /// <remarks>
        /// Aggiunge alla collezione UserData del metodo trovato/aggiunto anche il nome
        /// dell'evento e il nome del tipo a cui aggiungere la registrazione dell'eventhandler.
        /// </remarks>
        //--------------------------------------------------------------------------------
        internal MethodDeclaration AddEventHandlerMethodToType(
            TypeDeclaration codeTypeDeclaration,
            string methodName,
            string sourceEvent,
            IList<Statement> initialCode,
            Type eventArgsType,
            ref bool codeChanged
            )
        {
            MethodDeclaration eventHandler = GetEventHandler(methodName, eventArgsType, codeTypeDeclaration.Members);
            //Se esiste gia' il metodo non faccio niente
            if (eventHandler == null)
            {
                eventHandler = GenerateEventHandler(methodName, eventArgsType, initialCode);
                codeTypeDeclaration.Members.Add(eventHandler);
                codeChanged = true;

                OnCodeChanged(new CodeChangedEventArgs(ChangeType.MethodAdded, eventHandler));
            }
            return eventHandler;
        }

        //--------------------------------------------------------------------------------
        /// <remarks/>
        protected MethodDeclaration GenerateEventHandler(string methodName, Type eventArgsType, IList<Statement> initialCode)
        {
            /*
			 //BEGIN_nome_metodo
			 public void #methodName#_OnValueChanged()
			 {
			    //BEGIN_TRY
			    try
			    {
					//BEGIN_USER_CODE nome_metodo
					//Add your user code here
					//END_USER_CODE
			    }
				catch (System.Exception exc)
				{
					// exception handling code.
				}
			 }
			 //END_nome_metodo
			 */
            MethodDeclaration eventHandlerMethod = new MethodDeclaration();
            eventHandlerMethod.Body = new BlockStatement();
            eventHandlerMethod.Name = methodName;
            eventHandlerMethod.ReturnType = new PrimitiveType("void");
            eventHandlerMethod.Modifiers = Modifiers.Public;

            ParameterDeclaration senderPar = new ParameterDeclaration(new PrimitiveType(typeof(System.Object).Name), EasyBuilderSerializer.SenderVariableName);
            ParameterDeclaration argsPar = new ParameterDeclaration(new SimpleType(eventArgsType.Name), EasyBuilderSerializer.EventArgsVariableName);
            eventHandlerMethod.Parameters.Add(senderPar);
            eventHandlerMethod.Parameters.Add(argsPar);

            DecorateMethod(eventHandlerMethod, initialCode);

            return eventHandlerMethod;
        }

        //--------------------------------------------------------------------------------
        private MethodDeclaration GetEventHandler(
            string methodName,
            Type eventArgsType,
            IEnumerable<EntityDeclaration> members
            )
        {
            foreach (EntityDeclaration member in members)
            {
                MethodDeclaration method = member as MethodDeclaration;
                if (method == null)
                    continue;

                List<ParameterDeclaration> parameters = method.Parameters.ToList();

                //devono corrispondere:
                if (method.Name != methodName ||            //il nome
                    method.Parameters.Count != 2 ||         //il numero di parametri
                    method.ReturnType.AstTypeToString() != "void" || //il valore di ritorno deve essere void
                    parameters[0].Type.AstTypeToString() != "Object" || //il primo parametro deve essere object
                    parameters[1].Type.AstTypeToString() != eventArgsType.Name)     //il secondo l'eventargs corretto  //era fullname
                    continue;

                return method;
            }

            return null;
        }

        //--------------------------------------------------------------------------------
        internal static string[] GetSourcesFilePathForDocument(string sourcesPath)
        {
            return BasePathFinder.GetCSharpFilesIn(sourcesPath);
        }

        //--------------------------------------------------------------------------------
        internal static string GetSourcesPath(string ebCustomizationDllPath)
        {
            return BasePathFinder.GetDesignerSourcesPathFromDll(ebCustomizationDllPath);

        }

        //--------------------------------------------------------------------------------
        internal static bool IsABusinessObjectSourceFile(SyntaxTree syntaxTree)
        {
            if (syntaxTree == null || syntaxTree.Children == null || syntaxTree.Children.Count() == 0)
            {
                return false;
            }
            //Si tratta di un file di BusinessObject se nella compilation unit e` presente una classe che eredita da BusinessObject
            NamespaceDeclaration nsd = null;
            TypeDeclaration td = null;
            bool found = false;
            foreach (var child in syntaxTree.Children)
            {
                nsd = child as NamespaceDeclaration;
                if (nsd == null)
                {
                    continue;
                }

                foreach (var @class in nsd.Children)
                {
                    td = @class as TypeDeclaration;
                    if (td == null)
                    {
                        continue;
                    }
                    var queryForTypes = td.BaseTypes.Where(
                        t
                        =>
                        {
                            var simpleType = t as SimpleType;
                            if (simpleType == null)
                            {
                                return false;
                            }
                            return String.Compare(simpleType.Identifier, NewCustomizationInfos.BusinessObjectToken, StringComparison.InvariantCultureIgnoreCase) == 0;
                        }
                        );
                    if (queryForTypes.FirstOrDefault() != null)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }
            return found;
        }

        //-----------------------------------------------------------------------------
        internal static string GetSerializedNamespace(INameSpace nameSpace)
        {
            return string.Format(
                "{0}.{1}",
                NewCustomizationInfos.BusinessObjectsToken,
                GetComponentSafeSerializedNamespace(nameSpace).GetNameSpaceWithoutType()
                );
        }

        //-----------------------------------------------------------------------------
        internal static NameSpace GetComponentSafeSerializedNamespace(INameSpace ns)
        {
            return new NameSpace(GetSafeSerializedNamespace(ns));
        }

        //-----------------------------------------------------------------------------
        internal static string GetSafeSerializedNamespace(INameSpace ns)
        {
            return GetSafeSerializedNamespace(ns.FullNameSpace);
        }
        //-----------------------------------------------------------------------------
        internal static string GetSafeSerializedNamespace(string ns)
        {
            return ns.Replace("-", "_");
        }

        //-----------------------------------------------------------------------------
        internal static string GetCustomizedPartialPath(NameSpace customizationNamespace)
        {
            string customizationPath = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationType == ApplicationType.Customization
                                ? BasePathFinder.BasePathFinderInstance.GetCustomizationPath(customizationNamespace, CUtility.GetUser(), BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp)
                                : BasePathFinder.BasePathFinderInstance.GetStandardDocumentPath(customizationNamespace);
            DirectoryInfo workingFolder = new DirectoryInfo(customizationPath);

            string controllerFileCode = Path.ChangeExtension(Path.Combine(workingFolder.FullName, customizationNamespace.Leaf), NameSolverStrings.DllExtension);
            return GetSourcesPath(controllerFileCode);
        }

        //-----------------------------------------------------------------------------
        internal static string GetCustomizedFilesPath(NameSpace customizationNamespace, string fileNsName)
        {
            string partialPath = GetCustomizedPartialPath(customizationNamespace);
            return Path.Combine(partialPath, fileNsName);
        }


    }
}
