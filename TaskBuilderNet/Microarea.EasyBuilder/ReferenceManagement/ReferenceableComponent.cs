using System.ComponentModel;
using System.Reflection;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Localization;
using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using Ast = ICSharpCode.NRefactory.CSharp;

namespace Microarea.EasyBuilder
{
	//================================================================================
	internal sealed class ReferenceableComponent : EasyBuilderComponent
	{
		private ContentDescriptionAttribute componentDescription;
		private string errorMessage = string.Empty;
		private string dragDropClassName = string.Empty;

		//-----------------------------------------------------------------------------
		internal ContentDescriptionAttribute ComponentDescription { get { return componentDescription; } }

		//-----------------------------------------------------------------------------
		internal string FullMainClass { get { return string.Format("{0}.{1}", Sources.GetSerializedNamespace(Component), MainClass); } }

		//-----------------------------------------------------------------------------
		internal string DragDropClassName { get { return dragDropClassName; } set { dragDropClassName = value; } }

		//-----------------------------------------------------------------------------
		[LocalizedCategoryAttribute("GeneralCategory", typeof(EBCategories)), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ContentType { get { return componentDescription.ContentType; } }

		//-----------------------------------------------------------------------------
		[LocalizedCategoryAttribute("GeneralCategory", typeof(EBCategories)), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true)]
		public override string Name { get { return Component.Leaf; } }

		//-----------------------------------------------------------------------------
		[LocalizedCategoryAttribute("GeneralCategory", typeof(EBCategories)), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Description { get { return (componentDescription.ContentType == typeof(BusinessObject).Name) ? CUtility.GetDocumentTitle(Component) : string.Empty; } }

		//-----------------------------------------------------------------------------
		[LocalizedCategoryAttribute("Data", typeof(EBCategories)), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ReadOnly(true)]
		public NameSpace Component { get { return new NameSpace(componentDescription.Namespace); } set { componentDescription.Namespace = value.FullNameSpace; } }

		//-----------------------------------------------------------------------------
		[LocalizedCategoryAttribute("Data", typeof(EBCategories)), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string MainClass { get { return componentDescription.MainClass; } }

		//-----------------------------------------------------------------------------
		[LocalizedCategoryAttribute("StatusCategory", typeof(EBCategories)), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsValid { get { return errorMessage.IsNullOrEmpty(); } }

		//-----------------------------------------------------------------------------
		[LocalizedCategoryAttribute("StatusCategory", typeof(EBCategories)), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ErrorMessage { get { return errorMessage; } set { errorMessage = value; } }

		//--------------------------------------------------------------------------------
		internal ReferenceableComponent(ContentDescriptionAttribute attribute, NameSpace customizationNamespace)
		{
			this.componentDescription = attribute;
			// per poterli comparare devo per forza essere sicura che abbiano lo stesso formato
			NameSpace safeComponentNs = Sources.GetComponentSafeSerializedNamespace(Component);
			NameSpace safeCustomizationNs = Sources.GetComponentSafeSerializedNamespace(customizationNamespace);

			string nameWithSameRule = string.Format("{0}.{1}", safeComponentNs.GetNameSpaceWithoutType(), safeComponentNs.Leaf);
			dragDropClassName = nameWithSameRule == safeCustomizationNs.GetNameSpaceWithoutType() ? FullMainClass : MainClass;
		}

		//-----------------------------------------------------------------------------
		internal bool IsEqual(ReferenceableComponent refComponent)
		{
			return refComponent.ComponentDescription.Equals(ComponentDescription);
		}

		//-----------------------------------------------------------------------------
		internal void SerializeUsingDeclaration(Sources sources)
		{
			List<Expression> args = new List<Expression>();
			args.Add(new PrimitiveExpression(ContentType));
			args.Add(new PrimitiveExpression(Component.FullNameSpace));
			args.Add(new PrimitiveExpression(MainClass));

			Ast.Attribute attr = new Ast.Attribute();
			attr.Type = new SimpleType(typeof(ReferenceDeclarationAttribute).Name);
			attr.Arguments.AddRange(args);

			AttributeSection attrSec = new AttributeSection();
			attrSec.Attributes.Add(attr);

			TypeDeclaration aClass = EasyBuilderSerializer.GetControllerTypeDeclaration(sources.CustomizationInfos.EbDesignerCompilationUnit);

			if (aClass != null && GetCustomAttribute(aClass) == null)
				sources.AddCustomAttribute(aClass, attrSec);
		}

		//-----------------------------------------------------------------------------
		internal void RemoveUsingDeclaration(SyntaxTree cu)
		{
			TypeDeclaration aClass = EasyBuilderSerializer.GetControllerTypeDeclaration(cu);
			if (aClass == null)
				return;

			AttributeSection toRemove = GetCustomAttribute(aClass);

			if (toRemove != null)
				aClass.Attributes.Remove(toRemove);
		}

		//-----------------------------------------------------------------------------
		private AttributeSection GetCustomAttribute(TypeDeclaration aClass)
		{
			foreach (AttributeSection attributeSection in aClass.Attributes)
			{
				foreach (var attribute in attributeSection.Attributes)
				{
					if (attribute.Type.AstTypeToString() != typeof(ReferenceDeclarationAttribute).Name)
						continue;

					ReferenceDeclarationAttribute refAttr = ReferenceDeclarationAttribute.Create(attribute);
					if (refAttr.Equals(ComponentDescription))
						return attributeSection;
				}
			}
			return null;
		}


		//-----------------------------------------------------------------------------
		internal bool IsReferencedBy(MethodInfo methodInfo)
		{
			try
			{
				MsilReader reader = new MsilReader(methodInfo);
				while (reader.Read())
				{
					MsilInstruction ins = reader.Current;
					if (ins.Data == null)
						continue;

					MemberInfo info = ins.Data as MemberInfo;
					if (info != null && info.DeclaringType != null && info.DeclaringType.FullName == FullMainClass)
						return true;
				}
			}
			catch
			{
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			return string.Format("{0} ({1})", Description, MainClass);
		}
	}
}
