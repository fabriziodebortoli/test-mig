using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using ICSharpCode.NRefactory.CSharp;
using Microarea.EasyBuilder.Localization;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	/// <remarks/>
	public class SourcesSerializer
	{
		/// <remarks/>
		public const string ClearComponentsMethodName = "ClearComponents";
		/// <remarks/>
		public const string ApplyResourcesMethodName = "ApplyResources";
		/// <remarks/>
		public const string ResourcesVariableName = "resources";

		static readonly Type mDataManagerType = typeof(MDataManager);
		static readonly Type mHotLinkType = typeof(MHotLink);

		//-----------------------------------------------------------------------------
		/// <remarks/>
		internal static void GenerateApplyResources(IComponent component, TypeDeclaration aClass)
		{
			MethodDeclaration applyResourcesMethod = EasyBuilderSerializer.FindMember<MethodDeclaration>(aClass, EasyBuilderSerializer.ApplyResourcesMethodName);
			if (applyResourcesMethod != null)
				applyResourcesMethod.Remove();
			
			applyResourcesMethod = new MethodDeclaration();

			applyResourcesMethod.Modifiers = Modifiers.Public | Modifiers.Override;
			applyResourcesMethod.Name = ApplyResourcesMethodName;
			applyResourcesMethod.ReturnType = new PrimitiveType("void");
			applyResourcesMethod.Body = new BlockStatement();
			applyResourcesMethod.Body.Statements.Add
				(
					new VariableDeclarationStatement
					(
						new SimpleType(typeof(CustomizationComponentResourceManager).FullName),
						ResourcesVariableName,
						new MemberReferenceExpression
						(
							new PrimitiveExpression(new SimpleType(LocalizationSources.ResourceManagerClassName)),
							LocalizationSources.ResourceManagerFieldName
						)
					)
				);

			IEasyBuilderContainer container = component as IEasyBuilderContainer;
			foreach (IComponent ebControl in container.Components)
                GenerateApplyResources(applyResourcesMethod, (EasyBuilderComponent)ebControl);

			aClass.Members.Add(applyResourcesMethod);
        }

		//-----------------------------------------------------------------------------
		internal static void GenerateApplyResources(MethodDeclaration applyResourcesMethod, EasyBuilderComponent ebControl)
		{
			//Se non ci sono property cambiate allora non serializzando niente non devo 
			//neanche applicarne le risorse
			if (ebControl.ChangedPropertiesCount == 0)
				return;

			//resources.ApplyResources(myObject, "myObject");
			if (applyResourcesMethod.Body.IsNull)
			{
				applyResourcesMethod.Body = new BlockStatement();
			}

			applyResourcesMethod.Body.Statements.Add
				(
					AstFacilities.GetInvocationStatement
					(
						new IdentifierExpression(ResourcesVariableName),
						ApplyResourcesMethodName,
						new IdentifierExpression(ebControl.SerializedName),
						new PrimitiveExpression(ebControl.SerializedName)
					)
				);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		internal static void GenerateCreateComponents(EasyBuilderSerializer parentSerializer, IComponent component, TypeDeclaration aClass, IList<string> memberDeclaration)
		{
			MethodDeclaration createComponentsMethod = EasyBuilderSerializer.FindMember<MethodDeclaration>(aClass, EasyBuilderSerializer.CreateComponentsMethodName);
			if (createComponentsMethod != null)
				createComponentsMethod.Remove();

			createComponentsMethod = SourcesSerializer.GetCreateComponentsMethodDeclaration();

			IEasyBuilderContainer currentContainer = component as IEasyBuilderContainer;
			if (currentContainer != null)
			{
				//rigenero i field della classe
				parentSerializer.GenerateFields(currentContainer, aClass);

				foreach (EasyBuilderComponent current in GetOrderComponents(component))
				{
					List<Statement> statement = SerializeCreateComponents(current);
					if (statement != null && statement.Count != 0)
					{
						memberDeclaration.Add(current.SerializedName);

						//Faccio il ciclo perchè invocando la Addchild mi imposta il parent correttamente
						foreach (var stm in statement)
						{
							Statement copy = stm.Clone();
							createComponentsMethod.Body.Statements.Add(copy);
						}
					}
				}

				IContainer container = component as IContainer;
				if (container != null)
				{
					IList<Statement> additionalColl = parentSerializer.GetAdditionalCreateComponentsStatements(container, memberDeclaration);
					if (additionalColl != null && additionalColl.Count != 0)
					{
						foreach (var addStm in additionalColl)
							createComponentsMethod.Body.Statements.Add(addStm);
					}
				}

                //Se si tratta di un Controller allora nel suo metodo CreateComponents devono essere
                //serializzate anche le righe di codice che sottoscrivono gli eventi del controller stesso.
                //Nel caso normale queste sottoscrizioni sarebbero nel suo parent ma il Controller non ha parent.
                IModelRoot modelRoot = component as IModelRoot;
                if (modelRoot != null)
                {
                    var mng = new DesignerSerializationManager();
                    using (var session = mng.CreateSession())
                    {
                        var modelRootStatements = parentSerializer.Serialize(mng, modelRoot) as IList<Statement>;
                        if (modelRootStatements != null && modelRootStatements.Count != 0)
                        {
                            foreach (var stm in modelRootStatements)
                                createComponentsMethod.Body.Statements.Add(stm);
                        }
                    }
                }
			}
			else
			{
				EasyBuilderComponent ebComp = component as EasyBuilderComponent;

				EasyBuilderSerializer.GenerateField(aClass, ebComp);
				//in questo caso, non è un IEasyBuilderContainer, mi limito a serializzare l'oggetto stesso

				List<Statement> statement = SerializeCreateComponents(component);
				if (statement != null && statement.Count != 0)
				{
					memberDeclaration.Add(ebComp.SerializedName);

					foreach (var stm in statement)
						createComponentsMethod.Body.Statements.Add(stm);
				}
			}

			aClass.Members.Add(createComponentsMethod);
		}


		//--------------------------------------------------------------------------------
		internal static IEnumerable GetOrderComponents(IComponent component)
		{
			IEasyBuilderContainer container = component as IEasyBuilderContainer;
			IEnumerable list = null;
			//clono la lista per ordinarla in base a ZIndex
			List<IComponent> components = new List<IComponent>();
			foreach (IComponent current in container.Components)
				components.Add(current);
            if (container is BaseWindowWrapper)
			    components.Sort(new ZIndexComparer());
			list = components;
			return list;
		}

		//--------------------------------------------------------------------------------
		private static List<Statement> SerializeCreateComponents(IComponent component)
		{
			DesignerSerializationManager mng = new DesignerSerializationManager();
			List<Statement> coll = new List<Statement>();
			using (IDisposable session = mng.CreateSession())
			{
				EasyBuilderSerializer componentSerializer = (EasyBuilderSerializer)mng.GetSerializer(component.GetType(), typeof(CodeDomSerializer));

				coll = componentSerializer.Serialize(mng, component) as List<Statement>;
			}

			return coll;
		}


		//--------------------------------------------------------------------------------
		internal static MethodDeclaration GetCreateComponentsMethodDeclaration()
		{
			MethodDeclaration createComponentsMethod = new MethodDeclaration();
			createComponentsMethod.Modifiers = Modifiers.Public | Modifiers.Override;
			createComponentsMethod.Name = EasyBuilderSerializer.CreateComponentsMethodName;
			createComponentsMethod.ReturnType = new PrimitiveType("void");

			createComponentsMethod.Body = new BlockStatement();
			return createComponentsMethod;
		}

		//-----------------------------------------------------------------------------
		private static EasyBuilderSerializer GetSerializer(DesignerSerializationManager mng, Type ebComponentType)
		{
			Type serializerType = typeof(CodeDomSerializer);
			if (mHotLinkType.IsAssignableFrom(ebComponentType) && IsModuleShared(ebComponentType))
			{
				serializerType = typeof(HotLinkSerializerForModuleController);
			}
			else if (mDataManagerType.IsAssignableFrom(ebComponentType) && IsModuleShared(ebComponentType))
			{
				serializerType = typeof(DataManagerSerializerForModuleController);
			}

			return mng.GetSerializer(ebComponentType, serializerType) as EasyBuilderSerializer;
		}

		//-----------------------------------------------------------------------------
		private static bool IsModuleShared(Type ebComponentType)
		{
			string path = AssembliesLoader.GetAssemblyPath(ebComponentType.Assembly);
			IEasyBuilderApp app = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp;
			string modulePath = BasePathFinder.BasePathFinderInstance.GetEBModuleDllPath(app.ApplicationName,app.ModuleName);
			return path.CompareNoCase(modulePath);
		}

		//--------------------------------------------------------------------------------
		internal static void GenerateClass(SyntaxTree unit, EasyBuilderComponent component, bool recursive = true)
		{
			// alcune classi (vd MSqlRecord) sono utilizzate da piu' istanze di oggetti ma solo quella
			// principale definisce la fisionomia del wrapping, pertanto serializzarle tutte non solo e'
			// inutile ma costringerebbe a tenere allineate sempre tutte le istanze in ogni momento allo
			// scopo di serializzare bene
			if (component.IsAddonInstanceOfSharedSerializedClass)
				return;

            // (vd. tilegroup e tabDialog)
            // se il componente non ha caricato i suoi sotto-componenti non lo tocco fino a 
            // che non sono in grado di serializzarlo bene, altrimenti la rigenerazione della
            // classe mi eliminerebbe tutto il codice pregresso, convinta che non ci siano componenti
            if (!component.AreComponentsLoaded)
                return;

            DesignerSerializationManager mng = new DesignerSerializationManager();
			using (IDisposable session = mng.CreateSession())
			{
				NamespaceDeclaration ns = EasyBuilderSerializer.GetNamespaceDeclaration(unit);

				CodeDomSerializer ser = (CodeDomSerializer)mng.GetSerializer(component.GetType(), typeof(CodeDomSerializer));

				EasyBuilderSerializer ebSerializer = ser as EasyBuilderSerializer;
				if (ebSerializer == null)
					return;
      
				//cerchiamo la classe, se trovata, ripuliamo i field, altrimenti la creiamo da zero
				EasyBuilderSerializer.RemoveClass(unit, component.SerializedType);

				var aClass = ebSerializer.SerializeClass(unit, component);
				if (aClass == null)
					return;

				ns.Members.Add(aClass);


				List<string> fields = new List<string>();
				//Aggiunge il metodo personalizzato per la creazione dei control customizzati
				GenerateCreateComponents(ebSerializer, component, aClass, fields);

				//Aggiunge il metodo personalizzato per la localizzazione dei control customizzati
				GenerateApplyResources(component, aClass);

				//Aggiunge il metodo personalizzato per mettere a null i componenti
				GenerateClearComponents(component, aClass, fields);

				if (recursive)
				{
					foreach (IComponent current in SourcesSerializer.GetOrderComponents(component))
						GenerateClass(unit, current as EasyBuilderComponent, recursive);
				}
			}
		}


		/// <remarks/>
		//-----------------------------------------------------------------------------
		internal static void GenerateClearComponents(IComponent component, TypeDeclaration aClass, IList<string> memberFields)
		{
			MethodDeclaration clearComponentsMethod = EasyBuilderSerializer.FindMember<MethodDeclaration>(aClass, EasyBuilderSerializer.ClearComponentsMethodName);
			if (clearComponentsMethod != null)
				clearComponentsMethod.Remove();
			
			clearComponentsMethod = new MethodDeclaration();
			clearComponentsMethod.Modifiers = Modifiers.Public | Modifiers.Override;
			clearComponentsMethod.Name = ClearComponentsMethodName;
			clearComponentsMethod.ReturnType = new PrimitiveType("void");

			// base.<method>
			clearComponentsMethod.Body = new BlockStatement();
			clearComponentsMethod.Body.Statements.Add(AstFacilities.GetInvocationStatement(new BaseReferenceExpression(), ClearComponentsMethodName));

            if (component!= null && typeof(IDocumentController).IsAssignableFrom(component.GetType()))
            {
                //controller = oldController;
                var stat = AstFacilities.GetAssignmentStatement(
                    new IdentifierExpression(EasyBuilderControlSerializer.ControllerVariableName),
                    new IdentifierExpression(EasyBuilderControlSerializer.OldControllerVariableName)
                    );
                clearComponentsMethod.Body.Statements.Add(stat);
                //oldController = null;
                stat = AstFacilities.GetAssignmentStatement(
                    new IdentifierExpression(EasyBuilderControlSerializer.OldControllerVariableName),
                    new PrimitiveExpression(null)
                    );
                clearComponentsMethod.Body.Statements.Add(stat);
            }

			if (memberFields != null)
			{
				foreach (string  member in memberFields)
				{
					PrimitiveExpression nullExpr = new PrimitiveExpression(null);
					clearComponentsMethod.Body.Statements.Add(AstFacilities.GetAssignmentStatement(new IdentifierExpression(member), nullExpr));
				}
			}
			aClass.Members.Add(clearComponentsMethod);
		}
	}
}
