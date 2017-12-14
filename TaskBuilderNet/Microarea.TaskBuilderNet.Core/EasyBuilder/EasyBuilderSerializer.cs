using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using ICSharpCode.NRefactory.CSharp;
using Ast = ICSharpCode.NRefactory.CSharp;
using System.CodeDom;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.CSharp.Resolver;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=============================================================================
	public class EasyBuilderSerializer : CodeDomSerializer
	{
		public const string WrappedObjectParamName = "wrappedObject";
		public const string AddMethodName = "Add";
		public const string ChangedPropertiesPropertyName = "ChangedProperties";
		public const string ChangedEventsPropertyName = "ChangedEvents";
		public const string AddChangedEventMethodName = "AddChangedEvent";
		public const string AddReferencedByMethodName = "AddReferencedBy";
		public const string AddChangedPropertyMethodName = "AddChangedProperty";
		public const string AddBehaviourMethodName = "AddBehaviour";
		public const string ReferencedByPropertyName = "ReferencedBy";
		public const string ControllerVariableName = "controller";
        public const string OldControllerVariableName = "oldController";
        public const string StaticControllerVariableName = "DocumentController.controller";
        public const string DocumentControllerClassName = "DocumentController";
		public const string ModuleControllerFactoryGet = "Factory";
		public const string ModuleControllerClassName = "ModuleController";
		public const string BaseControllerClassName = "Microarea.EasyBuilder.MVC.DocumentController";
		public const string CreateComponentsMethodName = "CreateComponents";
		public const string ApplyResourcesMethodName = "ApplyResources";
		public const string ClearComponentsMethodName = "ClearComponents";
		public const string TableNameParameterName = "tableName";
		public const string DataManagerParameterName = "dataManagerName";
		public const string ContextParamName = "context";
		public const string UnattendedParamName = "unattended";
		public const string TbHandlePropertyName = "TbHandle";
		public const string DocumentPropertyName = "Document";
        public const string DataBindingPropertyName = "DataBinding";
		public const string SenderVariableName = "sender";
		public const string EventArgsVariableName = "e";
		public const string UserMethod = "USER_METHOD_{0}";
		public const string BeginUserCode = "BEGIN_USER_CODE_{0}";
		public const string EndUserCode = "END_USER_CODE_{0}";
		public const string BeginTryCode = "BEGIN_TRY";
		public const string EndTryCode = "END_TRY";
		public const string ExceptionVariableName = "exc";
		public const string FormHandleVariableName = "formHandle";
		public const string DocumentHandleVariableName = "documentHandle";
		public const string NameSpaceVariableName = "nameSpace";

        //--------------------------------------------------------------------------------
        public static TypeDeclaration FindClass(NamespaceDeclaration ns, string className)
        {
            TypeDeclaration declaration = null;
            foreach (AstNode type in ns.Members)
            {
                declaration = type as TypeDeclaration;
                if (declaration == null)
                    continue;

                if (declaration.ClassType == ClassType.Class && declaration.Name == className)
                    return declaration;
            }

            return null;
        }

        //--------------------------------------------------------------------------------
        public static TypeDeclaration FindClass(SyntaxTree cu, string className)
        {
            NamespaceDeclaration nsDeclaration = null;
            TypeDeclaration declaration = null;
            foreach (AstNode node in cu.Members)
            {
                nsDeclaration = node as NamespaceDeclaration;
                if (nsDeclaration == null)
                    continue;

                declaration = FindClass(nsDeclaration, className);
                if (declaration != null)
                    return declaration;
            }

            return null;
        }

        //--------------------------------------------------------------------------------
        public static TypeDeclaration GetControllerTypeDeclaration(SyntaxTree cu)
		{
			TypeDeclaration controllerTypeDecl = FindClass(cu, DocumentControllerClassName);

			if (controllerTypeDecl != null)
				return controllerTypeDecl;

			return FindClass(cu, ModuleControllerClassName);
		}
		
		//----------------------------------------------------------------------------	
		public static String GetFactoryMethodNameFromClassName(String className)
		{
			return "Create" + className;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Ritorna il membro della classe container a partire dal suo nome
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="container"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T FindMember<T>(TypeDeclaration container, string name) where T : class
		{
			EntityDeclaration parametrizedNode = null;
			FieldDeclaration fieldDeclaration = null;
			foreach (INode method in container.Children)
			{
				parametrizedNode = method as EntityDeclaration;
				if (parametrizedNode != null && parametrizedNode is T && parametrizedNode.Name == name)
				{
					return parametrizedNode as T;
				}

				fieldDeclaration = method as FieldDeclaration;
				if (fieldDeclaration != null)
				{
					VariableInitializer varIn = GetVariableInitializer(fieldDeclaration, name);
					if (varIn != null)
						return varIn as T;
				}
			}
			return null;
		}

		//--------------------------------------------------------------------------------
		public static VariableInitializer GetVariableInitializer(FieldDeclaration field, string name)
		{
			foreach (VariableInitializer variableDeclaration in field.Variables)
			{
				if (variableDeclaration.Name == name)
				{
					return variableDeclaration;
				}
			}

			return null;
		}

		//--------------------------------------------------------------------------------
		public static string Escape(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return text;

			return text.Replace("-", "_").Replace(" ", "");
		}

		//--------------------------------------------------------------------------------
		public static void RemoveClass(SyntaxTree syntaxTree, EasyBuilderComponent ebComponent)
		{
			RemoveClass(syntaxTree, ebComponent.SerializedType);
			IContainer container = ebComponent as IContainer;
			if (container == null)
				return;

			foreach (IComponent item in container.Components)
			{
				EasyBuilderComponent eComp = item as EasyBuilderComponent;
				if (eComp == null)
					continue;
				RemoveClass(syntaxTree, eComp);
			}
		}

		//--------------------------------------------------------------------------------
		public static void RemoveClass(SyntaxTree syntaxTree, string className)
		{
			TypeDeclaration aClass = EasyBuilderSerializer.FindClass(syntaxTree, className);
			if (aClass == null)
				return;

			//RemoveFieldRegion(cu, aClass);

			NamespaceDeclaration ns = null;
			foreach (var item in syntaxTree.Children)
			{
				ns = item as NamespaceDeclaration;
				if (ns == null)
					continue;

				aClass.Remove();
				break;
			}
		}

		//-----------------------------------------------------------------------------
		public static bool HasAttribute(FieldDeclaration node, Type attributeType)
		{
			if (node.Attributes.Count <= 0)
				return false;

			foreach (AttributeSection section in node.Attributes)
			{
				if (section.Attributes.Count <= 0)
					continue;

				foreach (Ast.Attribute attribute in section.Attributes)
				{
					if (attribute.Type == new SimpleType(attributeType.FullName))
						return true;
				}
			}

			return false;
		}

		//-----------------------------------------------------------------------------
		public static bool HasPreserveFieldAttribute(FieldDeclaration field)
		{
			return HasAttribute(field, typeof(PreserveFieldAttribute));
		}

		//-----------------------------------------------------------------------------
		//public static void RemoveFieldRegion(SyntaxTree cu, TypeDeclaration aClass)
		//{
		//	List<EntityDeclaration> members =  aClass.Members.ToList();
		//	for (int i = members.Count - 1; i >= 0; i--)
		//	{
		//		AstNode member = members[i] as AstNode;
		//		if (member == null)
		//			continue;

		//		FieldDeclaration field = member as FieldDeclaration;
		//		if (HasPreserveFieldAttribute(field))
		//			continue;

		//		PropertyDeclaration prop = member as PropertyDeclaration;
		//		SimpleType typeName = null;
		//		if (field != null)
		//			typeName = field.ReturnType as SimpleType;
		//		if (prop != null)
		//			typeName = prop.ReturnType as SimpleType;

		//		if (typeName == null)
		//			continue;

		//		RemoveClass(cu, typeName.Identifier);
		//		members.Remove(members[i]);  //member.Remove(); è una lista clonata, quindi la vecchia versione probabilmente non funziona
		//	}
		//}

		//--------------------------------------------------------------------------------
		public virtual PropertyDeclaration GenerateProperty(
													AstType propertyType,
													string propertyName,
													Expression returnGetExpression,
													bool isAnOverride = false
												)
		{
			List<Statement> statements = new List<Statement>();
			statements.Add(new ReturnStatement(returnGetExpression));

			return GenerateProperty(propertyType, propertyName, statements, isAnOverride);
		}


		//--------------------------------------------------------------------------------
		public virtual PropertyDeclaration GenerateProperty(
													string propertyType,
													string propertyName,
													Expression returnGetExpression,
													bool isAnOverride = false
												)
		{
			List<Statement> statements = new List<Statement>();
			statements.Add(new ReturnStatement(returnGetExpression));

			return GenerateProperty(propertyType, propertyName, statements, isAnOverride);
		}

		//--------------------------------------------------------------------------------
		public virtual PropertyDeclaration GenerateProperty(
													AstType propertyType,
													string propertyName,
													IList<Statement> getStatements,
													bool isAnOverride = false
												)
		{
			PropertyDeclaration prop = new PropertyDeclaration();
			prop.Modifiers = Modifiers.Public;
			if (isAnOverride)
				prop.Modifiers |= Modifiers.Override;

			prop.Name = Escape(propertyName);

			prop.ReturnType = propertyType;
			prop.Getter = new Accessor();
			prop.Getter.Body = new BlockStatement();
			foreach (Statement stat in getStatements)
			{
				prop.Getter.Body.Add(stat);
			}

			return prop;
		}


		//--------------------------------------------------------------------------------
		public virtual PropertyDeclaration GenerateProperty(
													string propertyType,
													string propertyName,
													IList<Statement> getStatements,
													bool isAnOverride = false
												)
		{
			PropertyDeclaration prop = new PropertyDeclaration();
			prop.Modifiers = Modifiers.Public;
			if (isAnOverride)
				prop.Modifiers |=  Modifiers.Override;

			prop.Name = Escape(propertyName);

			prop.ReturnType = new SimpleType(propertyType);
			prop.Getter = new Accessor();
			prop.Getter.Body = new BlockStatement();
			foreach (Statement stat in getStatements)
			{
				prop.Getter.Body.Add(stat);
			}
			
			return prop;
		}

		//--------------------------------------------------------------------------------
		public IList<Statement> SerializeProperties(
			IDesignerSerializationManager manager,
			EasyBuilderComponent ebComponent,
			String varName
			)
		{
			List<Statement> collection = new List<Statement>();

			// properties
			System.Attribute[] attribs = new System.Attribute[2];
			attribs[0] = new BrowsableAttribute(true);
			attribs[1] = new ReadOnlyAttribute(false);
            
            PropertyDescriptorCollection properties = ebComponent.GetProperties(attribs);

			if (properties != null)
				foreach (EasyBuilderPropertyDescriptor descriptor in properties)
				{
					if (descriptor.SerializationVisibility == DesignerSerializationVisibility.Hidden)
						continue;

                    if (ebComponent.ContainsChangedProperty(descriptor.Name))
                    {

                        if (!descriptor.IsLocalizable /*|| descriptor.Name != "Location"*/)
                            SerializeEBProperty(manager, collection, ebComponent, descriptor, ebComponent.SerializedName);

                        //varName.AddChangedProperty("prop");
                        collection.Add
                        (
                            AstFacilities.GetInvocationStatement(
                                varName,
                                AddChangedPropertyMethodName,
                                new PrimitiveExpression(descriptor.Name)
                                )
                        );
                    }
                }

			IList<Statement> extColl = SerializeExtensions(manager, ebComponent, varName);
            if (extColl != null)
                collection.AddRange(extColl);

			if (collection.Count == 0)
				return collection;
			
            return collection.Count > 0 ? collection : null;
		}

        //-----------------------------------------------------------------------------
        protected IList<Statement> SerializeDataBinding(EasyBuilderComponent ebComponent, string varName)
        {
            IDataBindingConsumer consumer = ebComponent as IDataBindingConsumer;

            // criteri per cui non posso e/o non devo serializzare
            if (consumer == null || consumer.DataBinding == null || consumer.DataBinding.IsEmpty || ebComponent.Site == null || ebComponent.HasCodeBehind)
                return null;

			List<Statement> collection = new List<Statement>();

            // per ora lo uso per distinguere il costruttore da usare ma non è bello
            IDataManager parent = consumer.DataBinding.Parent;
			Statement dataBindingCode = new ExpressionStatement(new AssignmentExpression
            (
                new MemberReferenceExpression(new IdentifierExpression(varName), DataBindingPropertyName),
				AssignmentOperatorType.Assign,
                parent == null ?
                    new ObjectCreateExpression
                    (
                        new SimpleType(consumer.DataBinding.GetType().Name),
                        new List<Expression>() { new IdentifierExpression(ReflectionUtils.GetComponentFullPath(consumer.DataBinding.Data as IComponent)) }
                     )
                     :
                    new ObjectCreateExpression
                    (
                        new SimpleType(consumer.DataBinding.GetType().Name),
						new List<Expression>() { 
							new IdentifierExpression(ReflectionUtils.GetComponentFullPath(consumer.DataBinding.Data as IComponent)),
							new IdentifierExpression(ReflectionUtils.GetComponentFullPath(parent as IComponent))
						}
                    )
            ));

		    collection.Add (dataBindingCode);
            if (!ebComponent.ContainsChangedProperty(EasyBuilderSerializer.DataBindingPropertyName))
                collection.Add
                (
                    new ExpressionStatement(new InvocationExpression
                    (
                        new MemberReferenceExpression(new IdentifierExpression(varName), EasyBuilderSerializer.AddChangedPropertyMethodName),
                        new List<Expression>() { new PrimitiveExpression(EasyBuilderSerializer.DataBindingPropertyName) }
                    ))
                );
            return collection;
        }

        //-----------------------------------------------------------------------------
        private IList<Statement> SerializeExtensions(IDesignerSerializationManager manager, EasyBuilderComponent ebComponent, String varName)
        {
            IEasyBuilderComponentExtendable extendable = ebComponent as IEasyBuilderComponentExtendable;

            // criteri per cui non posso e/o non devo serializzare
            if (extendable == null || extendable.Extensions == null || extendable.Extensions.Count == 0)
                return null;

			List<Statement> collection = new List<Statement>();
            foreach (EasyBuilderComponentExtender extender in extendable.Extensions)
            {
                CodeDomSerializer ser = (CodeDomSerializer) manager.GetSerializer(typeof(EasyBuilderComponentExtender), typeof(CodeDomSerializer));
				
				IList<Statement> extenderColl = ser.Serialize(manager, extender) as IList<Statement>;
                if (extenderColl != null && extenderColl.Count > 0)
                    collection.AddRange(extenderColl);
            }
            
            return collection;
        }

        //-----------------------------------------------------------------------------
        private IList<Statement> SerializeExtensionsEvents(
            IDesignerSerializationManager manager,
            EasyBuilderComponent ebComponent,
            string varName
            )
        {
            // Extendable Components serialize child events
            IEasyBuilderComponentExtendable extendable = ebComponent as IEasyBuilderComponentExtendable;
            if (extendable == null || extendable.Extensions.Count == 0)
                return null;

			List<Statement> collection = new List<Statement>();
            foreach (EasyBuilderComponentExtender extender in extendable.Extensions)
            {
                EasyBuilderSerializer ser = (EasyBuilderSerializer)manager.GetSerializer(typeof(EasyBuilderComponentExtender), typeof(CodeDomSerializer));
				List<Statement> extenderColl = ser.SerializeEvents(
					manager,
					extender,
					String.Concat(varName, "_", extender.SerializedName)
					) as List<Statement>;

                if (extenderColl != null && extenderColl.Count > 0)
                    collection.AddRange(extenderColl);
            }

            return collection;
        }

		//-----------------------------------------------------------------------------
		private void SerializeProperties(
			IDesignerSerializationManager manager,
			IList<Statement> singleCollection,
			EasyBuilderComponent easyBuilderComponent,
			System.Attribute[] attribs
			)
		{
			CodeStatementCollection codeStatementCollection = new CodeStatementCollection();
			
			base.SerializeProperties(manager, codeStatementCollection, easyBuilderComponent, attribs);

			Statement statement = null;
			foreach (var node in AstFacilities.ConvertToCompilationUnit(codeStatementCollection).Children)
			{
				statement = node as Statement;
				if (statement != null)
					singleCollection.Add(statement);
			}
		}


		//-----------------------------------------------------------------------------
		private void SerializeEBProperty(
			IDesignerSerializationManager manager,
			IList<Statement> collection,
			EasyBuilderComponent ebComponent,
			EasyBuilderPropertyDescriptor descriptor,
			string varName)
		{
			ObjectReferenceAttribute orAttribute = (ObjectReferenceAttribute)descriptor.Attributes[typeof(ObjectReferenceAttribute)];
			if (orAttribute == null)
			{
				if (!descriptor.IsLocalizable && descriptor.PropertyType == typeof(System.String))
				{
					//varName.Visible = true;
					ExpressionStatement cnt = new ExpressionStatement(new AssignmentExpression(
						new MemberReferenceExpression(new IdentifierExpression(varName), descriptor.Name),
						AssignmentOperatorType.Assign,
						new PrimitiveExpression(descriptor.GetValue(ebComponent)?.ToString())
						));

					collection.Add(cnt);
				}
				else
				{
					SerializeProperty(manager, collection, ebComponent, descriptor); 
				}
				return;
			}

			object propValue = descriptor.GetValue(ebComponent);
			IComponent cmp = propValue as IComponent;
			if (cmp == null)
				return;

			//varName.Visible = controller.Document.DBTCompany....;
			collection.Add(
				new ExpressionStatement(new AssignmentExpression(
					new MemberReferenceExpression(new IdentifierExpression(varName), descriptor.Name),
					AssignmentOperatorType.Assign,
					new IdentifierExpression(ReflectionUtils.GetComponentFullPath(cmp))
					))
				);
		}

		//-----------------------------------------------------------------------------
		protected virtual void SerializeProperty(
			IDesignerSerializationManager manager,
			IList<Statement> collection,
			EasyBuilderComponent ebComponent,
			PropertyDescriptor descriptor
			)
		{
			System.CodeDom.CodeStatementCollection codeStatementCollection =
				new System.CodeDom.CodeStatementCollection();

			base.SerializeProperty(manager, codeStatementCollection, ebComponent, descriptor);

			Statement statement = null;
			foreach (var node in AstFacilities.ConvertToCompilationUnit(codeStatementCollection).Children)
			{
				statement = node as Statement;
				if (statement != null)
					collection.Add(statement);
			}
		}

		//-----------------------------------------------------------------------------
		protected virtual string GetOwnerController()
		{
			return EasyBuilderSerializer.DocumentControllerClassName;
		}

		//-----------------------------------------------------------------------------
        protected virtual IList<Statement> SerializeEvents(
            IDesignerSerializationManager manager,
            EasyBuilderComponent ebComponent,
            string varName
            )
		{
			if (ebComponent.UsingWrappingSerialization)
				return null;

			List<Statement> collection = new List<Statement>();
            ebComponent = ebComponent.EventSourceComponent as EasyBuilderComponent;
			IEnumerator<EventInfo> enumerator = ebComponent.ChangedEvents.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Microarea.TaskBuilderNet.Core.EasyBuilder.EventInfo changedEvent = enumerator.Current;

				if (!changedEvent.Owner.CompareNoCase(GetOwnerController()))
					continue;

                Expression handlerExpression = null;
                if (changedEvent.UseGenericEventHandler)
                {
                    // new EventHandler<EventArgs>(<controller_or_this>.MParsedEdit0_TextChanged);
                    handlerExpression = new ObjectCreateExpression(
                        new SimpleType("System.EventHandler", new List<AstType>() { new SimpleType(changedEvent.EventArgsTypeName) }),
                            new List<Expression> { new MemberReferenceExpression(GetEventHandlerOwner(), changedEvent.EventHandlerName) }
                        );
                }
                else
                {
                    // new PropertyChangedEventHandler(<controller_or_this>.MParsedEdit0_PropertyChanged);
                    handlerExpression = new ObjectCreateExpression(
                        new SimpleType(changedEvent.EventHandlerTypeName),
                        new List<Expression> { new MemberReferenceExpression(GetEventHandlerOwner(), changedEvent.EventHandlerName) }
                        );
                }

				//this.MParsedEdit0.TextChanged += <eventDelegate definito poco sopra>;
				AssignmentExpression eventStatement = GenerateCodeAttachEventStatement(varName, changedEvent, handlerExpression);

				collection.Add(eventStatement);

				collection.Add
					(

					AstFacilities.GetInvocationStatement
								(
									new IdentifierExpression(varName),
									AddChangedEventMethodName,
									AstFacilities.GetObjectCreationExpression
									(
										typeof(Microarea.TaskBuilderNet.Core.EasyBuilder.EventInfo).Name,
										new PrimitiveExpression(changedEvent.SourceEvent),
										new PrimitiveExpression(changedEvent.EventHandlerName),
										new PrimitiveExpression(changedEvent.EventHandlerTypeName),
										new PrimitiveExpression(changedEvent.ComponentFullPath),
                                        new PrimitiveExpression(changedEvent.UseGenericEventHandler)
                                    )
								)
					);
			}

			IList<Statement> extColl = SerializeExtensionsEvents(manager, ebComponent, varName);
            if (extColl != null)
                collection.AddRange(extColl);
        
			return collection;
		}

		//-----------------------------------------------------------------------------
		protected virtual Expression GetEventHandlerOwner()
		{
			return new IdentifierExpression(StaticControllerVariableName);
		}

		//-----------------------------------------------------------------------------
		protected virtual AssignmentExpression GenerateCodeAttachEventStatement(
			string varName,
			EventInfo changedEvent,
			Expression handlerExpression
			)
		{
			return new AssignmentExpression(
				new IdentifierExpression(changedEvent.SourceEvent),
				AssignmentOperatorType.Add,
				handlerExpression
			);


		}

		//-----------------------------------------------------------------------------
		protected IList<Statement> SerializeReferences(EasyBuilderComponent ebComponent, string varName)
		{
			List<Statement> collection = new List<Statement>();
			foreach (string reference in ebComponent.ReferencedBy)
			{
				//MParsedEdit0.AddReferencedBy("tab_SiteParameters_ctrl_GenericButton3_Click");
				collection.Add
					(
						AstFacilities.GetInvocationStatement(
							new IdentifierExpression(varName),
							AddReferencedByMethodName,
							new PrimitiveExpression(reference)
							)
						);
			}
			return collection;
		}

		//-----------------------------------------------------------------------------
		protected virtual IList<MethodDeclaration> SerializeWebMethods(EasyBuilderComponent ebComponent)
		{
			if (ebComponent.InternalClasses == null)
				return null;

			List<MethodDeclaration> collection = new List<MethodDeclaration>();
			foreach (BaseApplicationInfo appInfo in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
			{
				foreach (BaseModuleInfo modInfo in appInfo.Modules)
				{
					if (modInfo == null || modInfo.WebMethods == null || modInfo.WebMethods.Count == 0)
						continue;

                    foreach (FunctionPrototype method in modInfo.WebMethods)
					{
						if (!IsMethodToSerialize(ebComponent, method))
							continue;

						MethodDeclaration methodCode = SerializeFunctionInfo(ebComponent, method);
						if (methodCode != null)
							collection.Add(methodCode);
					}
				}
			}
			return collection;
		}

		//-----------------------------------------------------------------------------
        private bool IsMethodToSerialize(EasyBuilderComponent ebComponent, FunctionPrototype method)
		{
			if (
					!method.InEasyBuilder ||
					method.Parameters == null || method.Parameters.Count == 0 ||
					ebComponent.InternalClasses == null || ebComponent.InternalClasses.Count == 0 ||
					method.NameSpace.Function.EndsWith("_Run") ||
					method.NameSpace.Function.EndsWith("_Dispose")
				)
				return false;

			Parameter handleParam = method.Parameters["handle"];
			if (handleParam == null)
				return false;

			// il web method appartiene ad una delle mie classi 
			string[] baseClasses = handleParam.BaseType.Split(',');
			bool isMyMethod = false;
			if (baseClasses.Length > 0)
				isMyMethod = ebComponent.InternalClasses.Contains(baseClasses[0]);

			return isMyMethod;
		}

		//-----------------------------------------------------------------------------
		private MethodDeclaration SerializeFunctionInfo(EasyBuilderComponent ebComponent, FunctionPrototype method)
		{
			try
			{
				if (
					method.Parameters == null || method.Parameters.Count == 0 ||
					ebComponent.InternalClasses == null || ebComponent.InternalClasses.Count == 0
					)
					return null;

				Parameter handleParam = method.Parameters["handle"];
				if (handleParam == null)
					return null;

				string funcName;
				string funcClass;
				SplitName(method.NameSpace.Function, out funcName, out funcClass);
				MethodDeclaration methodCode = new MethodDeclaration();
				methodCode.Name = funcName;
				methodCode.Modifiers = Modifiers.Public;
				methodCode.ReturnType = new SimpleType(AdjustParameterType(method.ReturnType).Name);
				methodCode.Body = new BlockStatement();
				Expression[] paramExpressions = new Expression[method.Parameters.Count];
				int nParam = 0;
				foreach (Parameter parameter in method.Parameters)
				{
					if (handleParam == parameter)
						paramExpressions[nParam] = new MemberReferenceExpression(new ThisReferenceExpression(), TbHandlePropertyName);
					else
					{
						Ast.FieldDirection direction = Ast.FieldDirection.None;
						switch (parameter.Mode)
						{
							case Interfaces.ParameterModeType.InOut:
								direction = Ast.FieldDirection.Ref; break;
							case Interfaces.ParameterModeType.Out:
								direction = Ast.FieldDirection.Out; break;
							default:
								break;
						}
						paramExpressions[nParam] = GetParamExpression(parameter, direction);

						methodCode.Parameters.Add
								(
									new ParameterDeclaration
									(
										 new SimpleType(AdjustParameterType(parameter.Type).Name),
										 parameter.Name
									)
								);
					}
					nParam++;
				}

				string methodPath = method.NameSpace.Module;
				if (method.NameSpace.Library != method.NameSpace.Module)
					methodPath += method.NameSpace.Library;

				InvocationExpression callExpression = new InvocationExpression
					(
						new MemberReferenceExpression(
							new IdentifierExpression(methodPath),
							method.NameSpace.Function
							)
					);
				if (paramExpressions.Length > 0)
					callExpression.Arguments.AddRange(paramExpressions);

				if (methodCode.ReturnType  == new PrimitiveType("void"))
					methodCode.Body.Add(callExpression);
				else
					methodCode.Body.Add(new ReturnStatement(callExpression));

				return methodCode;
			}
			catch
			{
				return null;
			}
		}

		//-----------------------------------------------------------------------------
		private DirectionExpression GetParamExpression(Parameter parameter, Ast.FieldDirection direction)
		{
			Expression expr = new IdentifierExpression(parameter.Name);
			if (parameter.Type == "DateTime")
			{
				expr = new InvocationExpression(
					new MemberReferenceExpression(expr, "ToString"),
					new List<Expression>() {new PrimitiveExpression("yyyy-MM-ddTHH:mm:ss") }
					);
			}

			return new DirectionExpression(direction, expr);
		}

		//-----------------------------------------------------------------------------
		private Type AdjustParameterType(string type)
		{
			return Type.GetType("System." + type);
		}

		//-----------------------------------------------------------------------------
		private void SplitName(string fullName, out string funcName, out string funcClass)
		{
			funcName = string.Empty;
			funcClass = string.Empty;
			string[] tokens = fullName.Split('_');
			if (tokens.Length >= 1)
				funcName = tokens[tokens.Length - 1];
			if (tokens.Length >= 2)
				funcClass = tokens[tokens.Length - 2];
		}

		//-----------------------------------------------------------------------------
		public virtual bool IsSerializable(EasyBuilderComponent ebComponent)
		{
			if (
				ebComponent.ChangedPropertiesCount != 0 ||
				ebComponent.ChangedEventsCount != 0 ||
				ebComponent.ReferencesCount != 0 || 
				ebComponent.IsChanged 
				)
				return true;

			if (!(ebComponent is IContainer))
				return false;

			DesignerSerializationManager mng = new DesignerSerializationManager();
			foreach (EasyBuilderComponent child in ((IContainer)ebComponent).Components)
			{
				CodeDomSerializer ser = (CodeDomSerializer)mng.GetSerializer(child.GetType(), typeof(CodeDomSerializer));
				if (ser is EasyBuilderSerializer && ((EasyBuilderSerializer)ser).IsSerializable(child))
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		private static bool IsFieldAlreadyDeclaredInClass(TypeDeclaration aClass, string varName, string varType)
		{
			if (aClass == null)
				return false;

			foreach (EntityDeclaration current in aClass.Members)
			{
				FieldDeclaration decl = current as FieldDeclaration;
				if (decl == null)
					continue;
				

				if (decl.ReturnType.AstTypeToString() != varType)
					continue;

				foreach (VariableInitializer varDeclaration in decl.Variables)
				{
					if (varDeclaration.Name.CompareNoCase(varName))
						return true;
				}
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public virtual void GenerateFields(IContainer container, TypeDeclaration classStructure)
		{
			foreach (IComponent icmp in container.Components)
			{
				EasyBuilderComponent cmp = icmp as EasyBuilderComponent;
				if (cmp == null)
					continue;
				GenerateField(classStructure, cmp);
			}
		}

		//-----------------------------------------------------------------------------
		public static void GenerateField(TypeDeclaration classStructure, EasyBuilderComponent ebComponent)
		{
			string varName = ebComponent.SerializedName;
			string varType = ebComponent.SerializedType;

			if (IsFieldAlreadyDeclaredInClass(classStructure, varName, varType))
				return;

			classStructure.Members.Add(AstFacilities.GetFieldsDeclaration(varType, varName));
		}

		//-----------------------------------------------------------------------------
		public virtual IList<Statement> GetAdditionalCreateComponentsStatements(IContainer container, IList<string> memberDeclaration)
		{
			return null;
		}

        //--------------------------------------------------------------------------------
        public virtual TypeDeclaration SerializeClass(SyntaxTree cu, IComponent o)
		{
			return null;
		}

		//--------------------------------------------------------------------------------
		protected void SetExpression(IDesignerSerializationManager manager, object value, IdentifierExpression expression)
		{
			this.SetExpression(manager, value, expression, false);
		}

		/// <summary>
		/// Converte l' expression da code dom a copilation unit
		/// </summary>
		//--------------------------------------------------------------------------------
		protected void SetExpression(IDesignerSerializationManager manager, object value, IdentifierExpression expression, bool isPreset)
		{
			CodeVariableReferenceExpression var = new CodeVariableReferenceExpression(expression.Identifier);
			base.SetExpression(manager, value, var, isPreset);
		}

		//--------------------------------------------------------------------------------
		public static NamespaceDeclaration GetNamespaceDeclaration(SyntaxTree compilationUnit)
		{
			NamespaceDeclaration aNamespaceDeclaration = null;
			foreach (AstNode child in compilationUnit.Members)
			{
				aNamespaceDeclaration = child as NamespaceDeclaration;
				if (aNamespaceDeclaration != null)
				{
					return aNamespaceDeclaration;
				}
			}

			return null;
		}
	}
}
