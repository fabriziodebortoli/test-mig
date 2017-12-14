using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.MVC;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Ast = ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;

namespace Microarea.EasyBuilder
{
	//================================================================================
	/// <summary>
	/// Encapsulates all the management of the source code EasyBuilder generates to
	/// create the customization.
	/// </summary>
    public partial class ControllerSources : Sources
    {
       
		/// <summary>
        /// Initializes a new instance of the ControllerSources.
        /// </summary>
        //--------------------------------------------------------------------------------
		public ControllerSources(
			SourcesSerializer sourcesSerializer,
			string applicationName,
			string moduleName,
			ApplicationType applicationType)
			: base(sourcesSerializer, applicationName, moduleName, applicationType)
        {
			string path = BasePathFinder.BasePathFinderInstance.GetEBModuleDllPath(applicationName, moduleName);
			//TODOLUCA
			//Assembly asm = ModuleEditor.LoadModuleDll(applicationName, moduleName);

			//if (asm != null)
			//{
			//	//inserisco nel project content l'assembly che ho appena aggiunto
			//	GlobalReferences.AddReference(asm, path);
			//}
		}

		/// <summary>
		/// Initializes the instance.
		/// </summary>
		/// <param name="customizationNamespace">The namespace to be used for the current customization</param>
		/// <param name="view">The view to be attached to the controller</param>
		/// <param name="document">The document to be attached to the controller</param>
		/// <param name="controller">The instance of the current controller, if not null, otherwise a generic controller is created.</param>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.INameSpace"/>
		/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentView"/>
		/// <seealso cref="Microarea.Framework.TBApplicationWrapper.MDocument"/>
		/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentController"/>
		//-------------------------------------------------------------------------------
		// Se il controller proviene da una dll, deserializza la classe codenamespace da embedded resource, 
		// altrimenti la genera ispezionando la classe controller
		public void Init(
			INameSpace customizationNamespace,
			DocumentView view,
			MDocument document,
			DocumentController controller
			)
		{
			try
			{
				if (controller != null)
				{
					//Controlla se i sorgenti sono stati aggiornati dall'esterno, nel caso chiede se si vuole compilare e riapplicare le modifiche

					Type t = controller.GetType();
					Assembly asm = t.Assembly;
					if (t != typeof(DocumentController))
					{
						//recupero le stringhe localizzate
						Localization.ReadLocalizedStringsFromAssembly(customizationNamespace, asm);
					}
					//recupero il codenamespace
					base.Init(customizationNamespace, asm);
					CustomizationInfos.Namespace = customizationNamespace as NameSpace;

					//il controller adesso è partial, cerco la classe e cambio l'attributo
					TypeDeclaration controllerClass = EasyBuilderSerializer.FindClass(CustomizationInfos.EbDesignerCompilationUnit, EasyBuilderSerializer.DocumentControllerClassName);
					if (controllerClass != null)
						controllerClass.Modifiers |= Modifiers.Partial;

					view.SwitchVisibility(Settings.Default.ShowHiddelFields);   //visualizzo i campi nascosti per poterli modificare

					controller.CreateComponents();
					document.CallCreateComponents();   //quella del document viene fatta dalla onattachdata
					view.CallCreateComponents();
				}
			
				else
				{
					base.Init(customizationNamespace, null);
					Localization.AddResourceManager(CustomizationInfos);
					controller = new DocumentController((NameSpace)customizationNamespace);
					controller.Add(document);
					controller.Add(view);

					//view.CallCreateComponents();  //fatte implicitamente sulle add immediatamente precedenti
					//document.CallCreateComponents();
					SourcesSerializer.GenerateClass(CustomizationInfos.EbDesignerCompilationUnit, controller);
				}

				OnCodeChanged();
			}
			finally
			{
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks />
		protected override bool IsSubclassOfController(Type t)
		{
			return t.IsSubclassOf(typeof(DocumentController));
		}

        /// <remarks/>
        //--------------------------------------------------------------------------------
        public void AddReferencesToComponents(string methodName, DocumentController controller, Assembly tempAssembly)
        {
            //le deseleziono per avere un aggiornamento automatico dello stato delle references
            //se nel mio metodo referenzio un oggetto e questo e` selezionato, devo ad es.
            //rinfrescare la griglia delle proprieta per inibire eventuali modifiche (es. campi locali)
            ISelectionService selectionService = controller.Site.GetService(typeof(ISelectionService)) as ISelectionService;
            selectionService?.SetSelectedComponents(null);
        }

		//--------------------------------------------------------------------------------
		/// <remarks />
		protected override SyntaxTree InitCodeNamespaceMethods(INameSpace customizationNamespace)
		{
			SyntaxTree aCompilationUnit = new SyntaxTree();
			NamespaceDeclaration aCodeNamespace = new NamespaceDeclaration(
				customizationNamespace.FullNameSpace
				);

			aCompilationUnit.Members.Add(aCodeNamespace);

			TypeDeclaration ctd = new TypeDeclaration();
			ctd.Modifiers = Modifiers.Public | Modifiers.Partial;
			ctd.Name = EasyBuilderSerializer.DocumentControllerClassName;
			aCodeNamespace.Members.Add(ctd);

			return aCompilationUnit;
		}

        /// <summary>
        /// Returns all the user method added to the controller.
        /// </summary>
        /// <returns></returns>
        //--------------------------------------------------------------------------------
        public override List<MethodDeclaration> GetUserMethods()
        {
            List<MethodDeclaration> methods = new List<MethodDeclaration>();
            if (CustomizationInfos == null)
                return methods;

            //Questi sono i metodi del controller alla nuova maniera
            methods.AddRange(GetUserMethods(CustomizationInfos.UserMethodsCompilationUnit));

            return methods;
        }

		//---------------------------------------------------------------------
		private IContainer GetParentContainerToSerialize(IContainer parentContainer)
		{
			IDataManager dataManager = parentContainer as IDataManager;
			if (dataManager != null)
			{
				DocumentController controller = ((IComponent)parentContainer).Site.GetService(typeof(DocumentController)) as DocumentController;
				return controller?.Document;
			}

			// cerco la prima IEasyBuilderContainer da serializzare visto che il 
			// serializzatore non serializza le IContainer ma le IEasyBuilderContainer
			while (parentContainer != null && !(parentContainer is IEasyBuilderContainer))
				parentContainer = (parentContainer as IComponent).Site.Container;

			return parentContainer;
		}

		//---------------------------------------------------------------------
		internal void UpdateReferenceToModuleDll(string moduleDllPath)
		{
			//TODOLUCA
			//C'è una sola dll di modulo, se è presente, aggiungo il reference ad essa solo se non sto compilando proprio lei.
			//GlobalReferences.RemoveReference(Path.GetFileNameWithoutExtension(moduleDllPath));
			//if (File.Exists(moduleDllPath))
			//{
			//	AddReference(moduleDllPath, CustomizationInfos, false);//non segnalo che il codice è cambiato per non scatenare compilazioni: sono io che sto compilando.
			//}
		}
	}
}
