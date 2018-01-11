using System;

using System.Collections.Generic;
using System.Reflection;
using ICSharpCode.NRefactory.CSharp;
using Microarea.EasyBuilder.MVC;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using System.ComponentModel;
using System.IO;

namespace Microarea.EasyBuilder
{
	//================================================================================
    /// <summary>
    /// business object component declarator
    /// </summary>
	public sealed class ComponentDeclarator 
	{
		private Sources sources;
		private List<ReferenceableComponent> referenceableComponents;
		private Type currentControllerType;
		private IDictionary<Type, Func<TypeDeclaration, NameSpace>> referenceableTypes;
		private Action<Sources, List<ReferenceableComponent>> loadReferencedComponents;

		//-------------------------------------------------------------------------------
		private List<ReferenceableComponent> ReferenceableComponents
		{
			get
			{
				if (referenceableComponents == null)
				{
					referenceableComponents = new List<ReferenceableComponent>();
					loadReferencedComponents(sources, referenceableComponents);

					AdjustAllReferencedBy();
				}
				return referenceableComponents;
			}
		}

		//-------------------------------------------------------------------------------
		internal Type CurrentControllerType { set { currentControllerType = value; } }

		//-------------------------------------------------------------------------------
		internal ComponentDeclarator(
			Sources sources,
			IDictionary<Type, Func<TypeDeclaration, NameSpace>> referenceableTypes,
			Action<Sources, List<ReferenceableComponent>> loadReferencedComponents
			)
		{
			this.sources = sources;
			this.referenceableTypes = referenceableTypes;

			this.loadReferencedComponents = loadReferencedComponents;
		}
	
		//-----------------------------------------------------------------------------
		internal ReferenceableComponent GetReferenceableComponent(INameSpace componentNameSpace)
		{
			foreach (ReferenceableComponent refComponent in ReferenceableComponents)
				if (refComponent.Component.FullNameSpace == componentNameSpace.FullNameSpace)
					return refComponent;

			return null;
		}

		//-----------------------------------------------------------------------------
		private Type GetControllerType(Assembly controllerAssembly = null)
		{
			if (controllerAssembly == null)
				return currentControllerType;
			
			foreach (Type t in controllerAssembly.GetTypes())
			{
				if (t.IsSubclassOf(typeof(DocumentController)))
					return t;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		internal void ClearReferencedBy(string methodName, Assembly controllerAssembly = null)
		{
			foreach (ReferenceableComponent refComponent in referenceableComponents)
				refComponent.RemoveReferencedBy(methodName);
		}

        //-----------------------------------------------------------------------------
        private void AdjustAllReferencedBy()
        {
            foreach (MethodDeclaration m in sources.GetUserMethods())
                AdjustReferencedBy(m.Name);
        }

        //-----------------------------------------------------------------------------
        private bool MatchWholeWord (int nPos, string methodText, string nameToMatch)
        {
            int nMethodLength = methodText.Length;
            // prima controllo il carattere precedente
            int nCharBeforePos = nPos == 0 ? -1 : nPos - 1;
            if (nCharBeforePos >= 0)
            {
                char charBefore = methodText[nCharBeforePos];
                // lettere e numeri non sono un inizio parola
                if (char.IsLetterOrDigit(charBefore))
                    return false;
            }

            int nCharAfterPos = nPos + nameToMatch.Length;
            // guardo se ho superato il fondo
            if (nCharAfterPos <= nMethodLength)
            {
                char charAfter = methodText[nCharAfterPos];
                // lettere e numeri non sono un inizio parola
                if (char.IsLetterOrDigit(charAfter))
                    return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------------
        // algoritmo per calcolare l'utilizzo di un oggetto nel testo. E' approssimativo e 
        // per evitare di uccidere troppo le performance con troppe ricerche su stringhe lo faccio io.
        private bool IsComponentReferenced(string methodText, EasyBuilderComponent ebComponent)
        {
            // prima cerco il nome del componente
            int nPos = -1;
            do
            {
                nPos = methodText.IndexOf(ebComponent.SerializedName, nPos + 1);
                if (nPos > 0 && MatchWholeWord(nPos, methodText, ebComponent.SerializedName))
                {
                    //Se ho trovato il nome del componente allora cerco anche il parent name: mi serve per disambiguare
                    //i casi di istanze di classi diverse ma che hanno lo stesso nome, capita per esempio se in un documento
                    //abbiamo un DataManagers sulla tabella Items e anche un DBT sulla tabella Items, entrambi avranno il loro
                    //record di nome MA_Items ma il primo sara` del tipo DMItems_TMAItems mentre il secondo sara` di tipo TDBTItems_TMAItems
                    EasyBuilderComponent parentComponent = null;
                    var parentNameFromMethodText = GetParentNameFromMethodText(nPos, methodText, ebComponent.SerializedName);
                    if ((parentComponent = ebComponent.ParentComponent) != null && !string.IsNullOrWhiteSpace(parentNameFromMethodText))
                    {
                        if (string.Compare(parentComponent.SerializedName, parentNameFromMethodText, StringComparison.InvariantCulture) == 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        //per conservare il comportamento precedente
                        return true;
                    }
                }

            }
            while (nPos != -1);

            //Continuo cercando se la sua classe e` referenziata solo se si tratta di un tipo referenziabile.
            //Cio` si applica a tutti gli oggetti per cui EasyStudio genera classi tipizzate (sql record,
            //body edit, dbt, tile ecc) mentre non si applica agli oggetti per cui non sono generate classi
            //tipizzate (data obj per esempio)
            if (ebComponent.IsReferenceableType)
            {
                nPos = -1;
                do
                {
                    nPos = methodText.IndexOf(ebComponent.SerializedType, nPos + 1);
                    if (nPos > 0 && MatchWholeWord(nPos, methodText, ebComponent.SerializedType))
                        return true;
                }
                while (nPos != -1);
            }

            return false;
        }

        //-----------------------------------------------------------------------------
        private static string GetParentNameFromMethodText(int nPos, string methodText, string serializedName)
        {
            if (nPos <= 0)
            {
                return string.Empty;
            }
            var tempStr = methodText.Substring(0, nPos);
            if (tempStr.EndsWith("."))
            {
                tempStr = tempStr.Substring(0, tempStr.Length - 1);
            }

            int preceedingDotPos = tempStr.LastIndexOf('.');
            if (preceedingDotPos < 0)
            {
                return tempStr;
            }

            return tempStr.Substring(preceedingDotPos + 1);
        }

        //-----------------------------------------------------------------------------
        internal void AdjustReferencedBy(string methodName, string methodText, IContainer container)
        {
            foreach (IComponent component in container.Components)
            {
                EasyBuilderComponent ebComponent = component as EasyBuilderComponent;

                if (ebComponent == null) 
                    continue;

                if (IsComponentReferenced(methodText, ebComponent))
                    ebComponent.AddReferencedBy(methodName);

                 IContainer asContainer = component as IContainer;
                if (asContainer != null)
                    AdjustReferencedBy(methodName, methodText, asContainer);
            }
        }

        //-----------------------------------------------------------------------------
        internal void AdjustAllReferencedByOn(DocumentController controller)
        {
			foreach (MethodDeclaration m in sources.GetUserMethods())
			{
				string res = AstFacilities.GetVisitedText(m);
				AdjustReferencedBy(m.Name, res, controller);
			}
		}


		//-----------------------------------------------------------------------------
		internal void AdjustReferencedBy(string methodName, Assembly controllerAssembly = null)
		{
			Type controllerType = GetControllerType(controllerAssembly);
			if (controllerType == null)
				return;

			// prima ripulisco l'array
			ClearReferencedBy(methodName, controllerAssembly);

			MethodInfo methodInfo = controllerType.GetMethod(methodName);
			foreach (ReferenceableComponent refComponent in referenceableComponents)
			{
				if (refComponent.IsReferencedBy(methodInfo))
				    refComponent.AddReferencedBy(methodName);
			}
		}

		//-----------------------------------------------------------------------------
		internal void Remove(ReferenceableComponent refComponent, bool removeDeclaration)
		{
			if (refComponent == null)
				return;

			if (removeDeclaration)
			{
				sources.RemoveAdditionalNamespace(Sources.GetSerializedNamespace(refComponent.Component));
			}

			refComponent.RemoveUsingDeclaration(sources.CustomizationInfos.EbDesignerCompilationUnit);
			ReferenceableComponents.Remove(refComponent);
		}

        //-----------------------------------------------------------------------------
		internal bool IsDeclared(string contentType, NameSpace componentNameSpace)
		{
			// se è nella customizzazione ho il name della classe nella reference relativa
			ReferenceableComponent refComponent = GetReferenceableComponent(componentNameSpace);
			return refComponent != null &&
				sources
				.CustomizationInfos
				.FindClassInAdditionalNamespaces(refComponent.MainClass) != null;
		}

		//-----------------------------------------------------------------------------
		internal bool IsReferenced(string contentType, NameSpace componentNameSpace)
		{
			ReferenceableComponent refComponent = GetReferenceableComponent(componentNameSpace);

			return refComponent != null && contentType == refComponent.ContentType;
		}
	
		//-----------------------------------------------------------------------------
		private bool DeclareComponent(ComponentDeclarationRequest request)
		{
			NameSpace ns = request.Namespace as NameSpace;
			
			ReferenceableComponent refComponent = GetReferenceableComponent(ns);
			// per ora aggiorno sempre tutto così pulisco il source code
			if (refComponent != null)
				Remove(refComponent, false);

			if (request.Type == typeof(BusinessObject))
				refComponent = SerializeBusinessObject(ns);
			else if (request.Type == typeof(MHotLink))
				refComponent = SerializeMHotLink(ns, request.ComponentInstanceToDeclare as MHotLink);
			else if (request.Type == typeof(MDataManager))
				refComponent = SerializeMDataManager(ns, request.ComponentInstanceToDeclare as MDataManager);

			//È nato un componente che può essere referenziato,
			//lo aggiungo alla collezione dei componenti che possono essere referenziati.
			if (refComponent != null)
				AddReferenceableComponent(refComponent);

			return refComponent != null;
		}

		//-----------------------------------------------------------------------------
		private ReferenceableComponent SerializeMDataManager(NameSpace ns, MDataManager dataManager)
		{
			sources.SerializeDataManager(dataManager);

			ContentDescriptionAttribute declAttribute = new ReferenceDeclarationAttribute(
			    dataManager.GetType().Name,
			    ns.FullNameSpace,
			    dataManager.SerializedType
			    );

			ReferenceableComponent refComponent = new ReferenceableComponent(declAttribute, sources.Namespace);
			if (refComponent != null && !refComponent.IsValid)
			    throw new ApplicationException(refComponent.ErrorMessage);

			return refComponent;
		}

		//-----------------------------------------------------------------------------
		private ReferenceableComponent SerializeMHotLink(NameSpace ns, MHotLink hotlink)
		{
			sources.SerializeHotlink(hotlink);

			//ModuleSources moduleSources = sources as ModuleSources;
			//if (moduleSources != null)
			//    moduleSources.SerializeHKLFactoryMethod(hotlink);

			ContentDescriptionAttribute declAttribute = new ReferenceDeclarationAttribute(
                hotlink.GetType().Name,
				ns.FullNameSpace,
				hotlink.SerializedType
				);

			ReferenceableComponent refComponent = new ReferenceableComponent(declAttribute, sources.Namespace);
			if (refComponent != null && !refComponent.IsValid)
				throw new ApplicationException(refComponent.ErrorMessage);

			return refComponent;
		}

		//-----------------------------------------------------------------------------
		private ReferenceableComponent SerializeBusinessObject(NameSpace ns)
		{
			using (BusinessObject businessObject = BusinessObject.CreateForSerialization<BusinessObject>(ns))
			{
				businessObject.CallCreateComponents();

				List<Type> requestedTypes = new List<Type>();
				requestedTypes.Add(typeof(MHotLink));

				List<EasyBuilderComponent> usedComponents = new List<EasyBuilderComponent>();
				List<EasyBuilderComponent> notCodeBehindComponents = new List<EasyBuilderComponent>();

				// gestione dei components e del hasCodeBehind
				businessObject.GetEasyBuilderComponents(requestedTypes, usedComponents);

				foreach (EasyBuilderComponent component in usedComponents)
				{
					if (!component.HasCodeBehind)
					{
						notCodeBehindComponents.Add(component);
						component.HasCodeBehind = true;
					}
					component.UsingWrappingSerialization = true;
					businessObject.Add(component);
				}

				sources.SerializeBusinessObject(businessObject);

				// hasCodebehind originale
				foreach (EasyBuilderComponent component in usedComponents)
				{
					if (notCodeBehindComponents.Contains(component))
						component.HasCodeBehind = false;
					component.UsingWrappingSerialization = false;
				}

				ContentDescriptionAttribute declAttribute = new ReferenceDeclarationAttribute(
					typeof(BusinessObject).Name,
					ns.FullNameSpace,
					businessObject.SerializedType
					);

				ReferenceableComponent refComponent = new ReferenceableComponent(declAttribute, sources.Namespace);
				if (refComponent != null && !refComponent.IsValid)
					throw new ApplicationException(refComponent.ErrorMessage);

				return refComponent;
			}
		}

		//-----------------------------------------------------------------------------
		internal bool ExecuteRequest(ComponentDeclarationRequest request)
		{
			switch (request.RequestAction)
			{
				case ComponentDeclarationRequest.Action.Remove:
					Remove(GetReferenceableComponent(request.Namespace), false);
					return true;
				case ComponentDeclarationRequest.Action.Add:
					return DeclareComponent(request);
				case ComponentDeclarationRequest.Action.Update:
					Remove(GetReferenceableComponent(request.Namespace), true);
					return DeclareComponent(request);
				case ComponentDeclarationRequest.Action.UpdateWithReferences:
					Remove(GetReferenceableComponent(request.Namespace), true);
					if (DeclareComponent(request))
					{
						AdjustAllReferencedBy();
						return true;
					}
					break;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		private void AddReferenceableComponent(ReferenceableComponent refComponent)
		{
			if (refComponent == null)
			{
				throw new ArgumentNullException("refComponent");
			}

			bool addRef = true;
			for (int i = ReferenceableComponents.Count - 1; i >= 0; i--)
			{
                if (refComponent.FullMainClass.CompareNoCase(ReferenceableComponents[i].FullMainClass))
				{
					ReferenceableComponents[i] = refComponent;
					addRef = false;
				}
			}

			if (addRef)
			    ReferenceableComponents.Add(refComponent);
		}

        //------------------------------------------------------------------------------
        internal void UpdateAttributes(TypeDeclaration controllerClass)
        {
            for (int i = 0; i <= ReferenceableComponents.Count - 1; i++)
            {
                ReferenceableComponent refComponent = ReferenceableComponents[i];
                if (refComponent == null)
                    continue;

                refComponent.SerializeUsingDeclaration(sources, controllerClass);
            }
        }

        //-----------------------------------------------------------------------------
        internal List<ReferenceableComponent> GetReferenceableComponents(Type contentType)
		{
			List<ReferenceableComponent> declarations = new List<ReferenceableComponent>();
			foreach (ReferenceableComponent declaration in ReferenceableComponents)
			{
				if (declaration.ComponentDescription.ContentType == contentType.Name)
					declarations.Add(declaration);
			}

			return declarations;
		}

	}

	//=================================================================================
	static class CodeTypeDeclarationExtension
	{
		//-----------------------------------------------------------------------------
		public static bool InheritFrom(
			this TypeDeclaration concreteTypeDeclaration,
			Type baseType
			)
		{
			foreach (AstType type in concreteTypeDeclaration.BaseTypes)
			{
				if (type.AstTypeToString() == baseType.FullName)
				{
					return true;
				}
			}
			return false;
		}
	}
}
