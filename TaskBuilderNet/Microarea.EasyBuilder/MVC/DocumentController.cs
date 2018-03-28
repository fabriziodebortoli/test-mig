using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms.Design;
using ICSharpCode.NRefactory.CSharp;
using Microarea.EasyBuilder.Properties;
using Microarea.EasyBuilder.Scripting;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Localization;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.EasyBuilder.ComponentModel;
using Newtonsoft.Json;

namespace Microarea.EasyBuilder.MVC
{
	/// <summary>
	/// Serializes an object graph to a series of CodeDOM statements for the DocumentController.
	/// </summary>
	/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentController"/>
	//================================================================================
	public class ControllerSerializer : EasyBuilderControlSerializer
	{
		/// <summary>
		/// Returns a value indicating whether the given EasyBuilderComponent can be
		/// serialized or not.
		/// </summary>
		/// <remarks>An instance of the DocumentController class is always serializable
		/// by the ControllerSerializer</remarks>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.EasyBuilder.EasyBuilderComponent"/>
		/// <returns>True if the EasyBuilderComponent is serializable, otherwise false.</returns>
		/// <param name="ebComponent">The EasyBuilderComponent to serialize.</param>
		//-----------------------------------------------------------------------------
		public override bool IsSerializable(EasyBuilderComponent ebComponent)
		{
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public override TypeDeclaration SerializeClass(SyntaxTree cu, IComponent o)
		{
			DocumentController controllerInstance = o as DocumentController;

			TypeDeclaration controllerClass = new TypeDeclaration();
			controllerClass.Modifiers = Modifiers.Partial | Modifiers.Public;
			controllerClass.Name = EasyBuilderSerializer.DocumentControllerClassName;

			controllerClass.BaseTypes.Add(new SimpleType(typeof(DocumentController).FullName));

			//private IntPtr formHandle = null;
			AttributeSection attr = AstFacilities.GetAttributeSection(typeof(PreserveFieldAttribute).FullName);
			FieldDeclaration formHandleVarDecl = AstFacilities.GetFieldsDeclaration(typeof(IntPtr).FullName, EasyBuilderSerializer.FormHandleVariableName);
			formHandleVarDecl.Modifiers = Modifiers.Private;
			formHandleVarDecl.Attributes.Add(attr);
			controllerClass.Members.Add(formHandleVarDecl);

			//private IntPtr documentHandle = null;
			attr = AstFacilities.GetAttributeSection(typeof(PreserveFieldAttribute).FullName);

			FieldDeclaration documentHandleVarDecl = AstFacilities.GetFieldsDeclaration(typeof(IntPtr).FullName, EasyBuilderSerializer.DocumentHandleVariableName);
			documentHandleVarDecl.Modifiers = Modifiers.Private;
			documentHandleVarDecl.Attributes.Add(attr);
			controllerClass.Members.Add(documentHandleVarDecl);

			//Crea la proprietà "Document"
			string docClassName = controllerInstance.Document.SerializedType;
			attr = AstFacilities.GetAttributeSection(typeof(PreserveFieldAttribute).FullName);

			PropertyDeclaration documentProperty = new PropertyDeclaration();
			documentProperty.Modifiers = Modifiers.Public | Modifiers.New;
			documentProperty.Name = EasyBuilderSerializer.DocumentPropertyName;
			documentProperty.ReturnType = new SimpleType(String.Format(docClassName));
			documentProperty.Getter = new Accessor();
			documentProperty.Getter.Body = new BlockStatement();
			ReturnStatement ret = new ReturnStatement
				(
					new CastExpression
						(
							new SimpleType(docClassName),
							new MemberReferenceExpression
								(
									new BaseReferenceExpression(),
									"document"
								)
						)
				);

			documentProperty.Getter.Body.Statements.Add(ret);
			documentProperty.Attributes.Add(attr);
			controllerClass.Members.Add(documentProperty);

            //[ThreadStatic]
			//public static new DocumentController controller;
			//serve per semplificare la sintassi degli event handlers
			attr = AstFacilities.GetAttributeSection(typeof(PreserveFieldAttribute).FullName);
            AttributeSection attrThreadStatic = AstFacilities.GetAttributeSection(typeof(ThreadStaticAttribute).FullName); 
            FieldDeclaration controllerVarDecl = AstFacilities.GetFieldsDeclaration(controllerClass.Name, EasyBuilderControlSerializer.ControllerVariableName);
			controllerVarDecl.Modifiers = Modifiers.Static | Modifiers.Public | Modifiers.New;
			controllerVarDecl.Attributes.Add(attr);
            controllerVarDecl.Attributes.Add(attrThreadStatic);
            controllerClass.Members.Add(controllerVarDecl);

            //private DocumentController oldController;
            FieldDeclaration oldControllerVarDecl = AstFacilities.GetFieldsDeclaration(controllerClass.Name, EasyBuilderControlSerializer.OldControllerVariableName);
            oldControllerVarDecl.Modifiers = Modifiers.Private;
            controllerClass.Members.Add(oldControllerVarDecl);


            //Crea la proprietà Get per la view
            attr = AstFacilities.GetAttributeSection(typeof(PreserveFieldAttribute).FullName);
			PropertyDeclaration viewProperty = new PropertyDeclaration();
			viewProperty.Modifiers = Modifiers.Public | Modifiers.New;
			viewProperty.Name = ControllerSources.ViewPropertyName;
			viewProperty.ReturnType = new SimpleType(EasyBuilderControlSerializer.ViewClassName);
			viewProperty.Getter = new Accessor();
			viewProperty.Getter.Body = new BlockStatement();
		
			ret = new ReturnStatement
				(
					new CastExpression
						(
						   new SimpleType(EasyBuilderControlSerializer.ViewClassName),
						   new MemberReferenceExpression
							   (
									new BaseReferenceExpression(),
									ControllerSources.ViewPropertyName
								)
						)
				);

			viewProperty.Getter.Body.Statements.Add(ret);
			viewProperty.Attributes.Add(attr);

			controllerClass.Members.Add(viewProperty);

			ConstructorDeclaration constr = new ConstructorDeclaration();
			constr.Modifiers = Modifiers.Public;
			constr.Name = controllerClass.Name;
			controllerClass.Members.Add(constr);

			constr.Parameters.Add(new ParameterDeclaration(new SimpleType(typeof(NameSpace).FullName), EasyBuilderSerializer.NameSpaceVariableName, ICSharpCode.NRefactory.CSharp.ParameterModifier.None));
			constr.Parameters.Add(new ParameterDeclaration(new SimpleType(typeof(IntPtr).FullName), EasyBuilderSerializer.FormHandleVariableName, ICSharpCode.NRefactory.CSharp.ParameterModifier.None));
			constr.Parameters.Add(new ParameterDeclaration(new SimpleType(typeof(IntPtr).FullName), EasyBuilderSerializer.DocumentHandleVariableName, ICSharpCode.NRefactory.CSharp.ParameterModifier.None));
			constr.Body = new BlockStatement();
			constr.Initializer = AstFacilities.GetConstructorInitializer(new IdentifierExpression(EasyBuilderSerializer.NameSpaceVariableName));

            //if (controller != null)
            //{
            //  oldController = controller;
            //}
            var ifStat = new IfElseStatement(//if 
                new BinaryOperatorExpression(
                        new IdentifierExpression(EasyBuilderControlSerializer.ControllerVariableName),//controller
                        BinaryOperatorType.InEquality,//!=
                        new PrimitiveExpression(null)//null
                        ),
                AstFacilities.GetAssignmentStatement(//oldController = controller;
                    new IdentifierExpression(EasyBuilderControlSerializer.OldControllerVariableName),
                    new IdentifierExpression(EasyBuilderControlSerializer.ControllerVariableName)
                    )
                );
            constr.Body.Statements.Add(ifStat);

            //controller = this;
            Statement controllerAssignment = AstFacilities.GetAssignmentStatement(
				new IdentifierExpression(EasyBuilderControlSerializer.ControllerVariableName),
				new ThisReferenceExpression());
			constr.Body.Statements.Add(controllerAssignment);

			//this.formHandle = formHandle;
			Statement formHandleAssignment = AstFacilities.GetAssignmentStatement(
				new MemberReferenceExpression(new ThisReferenceExpression(), EasyBuilderSerializer.FormHandleVariableName),
				new IdentifierExpression(EasyBuilderSerializer.FormHandleVariableName));
			constr.Body.Statements.Add(formHandleAssignment);

			//this.documentHandle = documentHandle;
			Statement documentHandleAssignment = AstFacilities.GetAssignmentStatement(
				new MemberReferenceExpression(new ThisReferenceExpression(), EasyBuilderSerializer.DocumentHandleVariableName),
				new IdentifierExpression(EasyBuilderSerializer.DocumentHandleVariableName));
			constr.Body.Statements.Add(documentHandleAssignment);

		
			//document_ProspectiveSuppliers = new MDProspectiveSuppliers(documentHandle, this);
			IdentifierExpression varDocExpression = new IdentifierExpression(controllerInstance.Document.SerializedName);
			constr.Body.Statements.Add
				(
					new ExpressionStatement
					(
						new AssignmentExpression
						(
							varDocExpression,
							AssignmentOperatorType.Assign,
							AstFacilities.GetObjectCreationExpression
							(
								new SimpleType(controllerInstance.Document.SerializedType),
								new IdentifierExpression(EasyBuilderSerializer.DocumentHandleVariableName)
							)
						)
					)
				);

			//view_custSupp = new ViewCustSupp();
			IdentifierExpression variableDeclExpression = new IdentifierExpression(controllerInstance.View.SerializedName);
			ObjectCreateExpression creationExpression =
							AstFacilities.GetObjectCreationExpression(
							EasyBuilderControlSerializer.ViewClassName,
								new IdentifierExpression(EasyBuilderSerializer.FormHandleVariableName)
							);

			constr.Body.Statements.Add(AstFacilities.GetAssignmentStatement
				(
					variableDeclExpression,
					creationExpression
				));

			//this.document = document_ProspectiveSuppliers;
			Statement documentAssignment = AstFacilities.GetAssignmentStatement(
				new MemberReferenceExpression(new ThisReferenceExpression(), "document"),
				new IdentifierExpression(controllerInstance.Document.SerializedName));
			constr.Body.Statements.Add(documentAssignment);


			//this.view = view_ProspSupp;
			Statement viewAssignment = AstFacilities.GetAssignmentStatement(
				new MemberReferenceExpression(new ThisReferenceExpression(), "view"),
				new IdentifierExpression(controllerInstance.View.SerializedName));
			constr.Body.Statements.Add(viewAssignment);

             //partial void CreateCodeExtender();
            MethodDeclaration createCodeExtenderMethod = new MethodDeclaration();
            createCodeExtenderMethod.Modifiers = Modifiers.Partial;
            createCodeExtenderMethod.Name = "CreateCodeExtender";
            createCodeExtenderMethod.ReturnType = new PrimitiveType("void");
			controllerClass.Members.Add(createCodeExtenderMethod);
            // CreateCodeExtender();
            Statement createCodeInvokeExpr = AstFacilities.GetInvocationStatement
            	(
            		new ThisReferenceExpression(),
                    "CreateCodeExtender"
                );
            constr.Body.Statements.Add(createCodeInvokeExpr);
            TBSite site = controllerInstance.Site as TBSite;
            if (site != null && site.Editor != null)
                site.Editor.ComponentDeclarator.UpdateAttributes(controllerClass);

            return controllerClass;
		}

        /// <summary>
        /// 
        /// </summary>
        //-----------------------------------------------------------------------------
        public TypeDeclaration SerializeClassForBusinessObject(SyntaxTree cu, IComponent o)
        {
            DocumentController controllerInstance = o as DocumentController;

			TypeDeclaration controllerClass = new TypeDeclaration();
			controllerClass.Modifiers = Modifiers.Partial | Modifiers.Public;
            controllerClass.Name = EasyBuilderSerializer.DocumentControllerClassName;
            controllerClass.BaseTypes.Add(new SimpleType(typeof(DocumentController).FullName));

            AttributeSection attr = AstFacilities.GetAttributeSection(typeof(PreserveFieldAttribute).FullName);
            //Crea la proprietà "Document"
            string docClassName = controllerInstance.Document.SerializedType;

			PropertyDeclaration documentProperty = new PropertyDeclaration();
			documentProperty.Name = EasyBuilderSerializer.DocumentPropertyName;
			documentProperty.Modifiers = Modifiers.Public | Modifiers.New;
            documentProperty.ReturnType = new SimpleType(String.Format(docClassName));

			documentProperty.Getter = new Accessor(); 
			documentProperty.Getter.Body = new BlockStatement();

			ReturnStatement ret = new ReturnStatement
                (
                    new CastExpression
                        (
                            new SimpleType(docClassName),
                            new MemberReferenceExpression
                                (
                                    new BaseReferenceExpression(),
                                    "document"
                                )
                        )
                );

			documentProperty.Getter.Body.Statements.Add(ret);
            documentProperty.Attributes.Add(attr);
            controllerClass.Members.Add(documentProperty);

            //private DocumentController controller;
            //serve per semplificare la sintassi degli event handlers
            FieldDeclaration controllerVarDecl = AstFacilities.GetFieldsDeclaration(controllerClass.Name, EasyBuilderControlSerializer.ControllerVariableName);
            controllerVarDecl.Modifiers = Modifiers.Static | Modifiers.Public;
            controllerVarDecl.Attributes.Add(attr);
            controllerClass.Members.Add(controllerVarDecl);


			ConstructorDeclaration constr = new ConstructorDeclaration();
			constr.Name = controllerClass.Name;
			constr.Modifiers = Modifiers.Public;
            controllerClass.Members.Add(constr);

            constr.Parameters.Add(new ParameterDeclaration(new SimpleType(typeof(NameSpace).FullName), EasyBuilderSerializer.NameSpaceVariableName));
            constr.Parameters.Add(new ParameterDeclaration(new SimpleType(typeof(IntPtr).FullName), EasyBuilderSerializer.FormHandleVariableName));
            constr.Parameters.Add(new ParameterDeclaration(new SimpleType(typeof(IntPtr).FullName), EasyBuilderSerializer.DocumentHandleVariableName));
            constr.Body = new BlockStatement();
            constr.Initializer = AstFacilities.GetConstructorInitializer(new IdentifierExpression(EasyBuilderSerializer.NameSpaceVariableName));

            //this.controller = this;
            Statement controllerAssignment = AstFacilities.GetAssignmentStatement(
                new IdentifierExpression(EasyBuilderControlSerializer.ControllerVariableName),
                new ThisReferenceExpression());
            constr.Body.Statements.Add(controllerAssignment);

            //this.formHandle = formHandle;
            Statement formHandleAssignment = AstFacilities.GetAssignmentStatement(
                new MemberReferenceExpression(new ThisReferenceExpression(), EasyBuilderSerializer.FormHandleVariableName),
                new IdentifierExpression(EasyBuilderSerializer.FormHandleVariableName));
            constr.Body.Statements.Add(formHandleAssignment);

            //this.documentHandle = documentHandle;
            Statement documentHandleAssignment = AstFacilities.GetAssignmentStatement(
                new MemberReferenceExpression(new ThisReferenceExpression(), EasyBuilderSerializer.DocumentHandleVariableName),
                new IdentifierExpression(EasyBuilderSerializer.DocumentHandleVariableName));
            constr.Body.Statements.Add(documentHandleAssignment);


            //document_ProspectiveSuppliers = new MDProspectiveSuppliers(documentHandle, this);
            IdentifierExpression varDocExpression = new IdentifierExpression(controllerInstance.Document.SerializedName);
            constr.Body.Statements.Add
                (
                    new ExpressionStatement
                    (
                        new AssignmentExpression
                        (
                            varDocExpression,
                            AssignmentOperatorType.Assign,
                            AstFacilities.GetObjectCreationExpression
                            (
                                new SimpleType(controllerInstance.Document.SerializedType),
                                new IdentifierExpression(EasyBuilderSerializer.DocumentHandleVariableName)
                            )
                        )
                    )
                );

            //view_custSupp = new ViewCustSupp();
            IdentifierExpression variableDeclExpression = new IdentifierExpression(controllerInstance.View.SerializedName);
            ObjectCreateExpression creationExpression =
                            AstFacilities.GetObjectCreationExpression(
                            EasyBuilderControlSerializer.ViewClassName,
                                new IdentifierExpression(EasyBuilderSerializer.FormHandleVariableName)
                            );

            constr.Body.Statements.Add(AstFacilities.GetAssignmentStatement
                (
                    variableDeclExpression,
                    creationExpression
                ));

            //this.document = document_ProspectiveSuppliers;
            Statement documentAssignment = AstFacilities.GetAssignmentStatement(
                new MemberReferenceExpression(new ThisReferenceExpression(), "document"),
                new IdentifierExpression(controllerInstance.Document.SerializedName));
            constr.Body.Statements.Add(documentAssignment);


            //this.view = view_ProspSupp;
            Statement viewAssignment = AstFacilities.GetAssignmentStatement(
                new MemberReferenceExpression(new ThisReferenceExpression(), "view"),
                new IdentifierExpression(controllerInstance.View.SerializedName));
            constr.Body.Statements.Add(viewAssignment);

            //partial void CreateCodeExtender();
            MethodDeclaration createCodeExtenderMethod = new MethodDeclaration();
            createCodeExtenderMethod.Modifiers = Modifiers.Partial;
            createCodeExtenderMethod.Name = "CreateCodeExtender";
            createCodeExtenderMethod.ReturnType = new PrimitiveType("void");
			controllerClass.Members.Add(createCodeExtenderMethod);
            // CreateCodeExtender();
            Statement createCodeInvokeExpr = AstFacilities.GetInvocationStatement
                (
                    new ThisReferenceExpression(),
                    "CreateCodeExtender"
                );
            constr.Body.Statements.Add(createCodeInvokeExpr);
            return controllerClass;
        }
        /// <summary>
        /// Serializes the specified object into a CodeDOM object.
        /// </summary>
        /// <seealso cref="System.ComponentModel.Design.Serialization.IDesignerSerializationManager"/>
        /// <param name="current">The object to serialize</param>
        /// <param name="manager">The manager for the serialization process</param>
            //-----------------------------------------------------------------------------
        public override object Serialize(IDesignerSerializationManager manager, object current)
		{
			List<Statement> newCollection = new List<Statement>();

			// events
			IList<Statement> events = SerializeEvents(manager, (EasyBuilderComponent)current, ControllerVariableName);
			if (events != null && events.Count > 0)
				newCollection.AddRange(events);

			return newCollection;
		}
	};

	[AttributeUsage(AttributeTargets.Event)]
	internal class BatchFilterAttribute : TBEventFilterAttribute
	{ 
	}
	[AttributeUsage(AttributeTargets.Event)]
	internal class NonBatchFilterAttribute : TBEventFilterAttribute
	{
	}

    /// <summary>
    /// The DocumentController is the primary object for the customization
    /// architecture: it is the root of the object model representing the document
    /// that you are customizing.
    /// Through the DocumentController you have access to the part of the model
    /// representing all the UI (i.e.: the DocumentView) and the part of the
    /// model representing data (i.e.: the MDocument), gaining the control
    /// over all the document stuff.
    /// All the business logic realized by the customizer is inserted in the
    /// body of the DocumentController class under the form of class methods.
    /// This is the base class for all controllers customization classes EasyBuilder generates
    /// to customize any document.
    /// </summary>
    /// <seealso cref="Microarea.TaskBuilderNet.Core.EasyBuilder.EasyBuilderComponent"/>
    /// <seealso cref="Microarea.TaskBuilderNet.Interfaces.EasyBuilder.IEasyBuilderContainer"/>
    /// <seealso cref="Microarea.EasyBuilder.MVC.DocumentView"/>
    /// <seealso cref="Microarea.Framework.TBApplicationWrapper.MDocument"/>
    //=============================================================================
    [PropertyTabAttribute(typeof(EventsTab), PropertyTabScope.Component)]
	[ExcludeFromIntellisense]
	[DesignerSerializer(typeof(ControllerSerializer), typeof(CodeDomSerializer))]
	public class DocumentController : MEasyBuilderContainer, IModelRoot, IDocumentController
	{
   
		/// <summary>
		/// 
		/// </summary>
		protected MDocument			document;

		///
		protected DocumentView		view;
		private NameSpace			customizationNameSpace;
		private EasyBuilderScriptingManager scriptManager;
        private bool canBeLoaded = true;
        JsonEvents jsonEvents;
        /// <summary>
        /// static declared events in JSON
        /// </summary>
        public JsonEvents JSONEvents { get => jsonEvents; set => jsonEvents = value; }

        /// <summary>
        /// Public static property used only for businessobjects purpose
        /// </summary>
        public static DocumentController controller;

		//-----------------------------------------------------------------------------
		private TBScriptManager ScriptManager
		{
			get { if (scriptManager == null)scriptManager = new EasyBuilderScriptingManager(this); return scriptManager; }
		}
    

		/// <summary>
		/// Gets a value indicating whether this instance has document events registered.
		/// </summary>
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public bool HasEventManagerEvents { get { return Event != null; } }

		/// <summary>
		/// Occurs when a document event is raised.
		/// </summary>
		/// <remarks>
		/// A document event is raised by the programmer from the
		/// TaskBuilder.NET C++ document. You can use these events to know some
		/// specific information about the original document life cycle, you can react
		/// to these events as well by registering an event handler.
		/// </remarks>
		/// <seealso cref="Microarea.EasyBuilder.ManagedClientDocEvent"/>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("EventsCategory", typeof(EBCategories))]
		[Description("A document event is raised by the programmer from the TaskBuilder.NET C++ document. " +
					"You can use these events to know some specific information about the original document life cycle, " + 
					" you can react to these events as well by registering an event handler.")]
		public event EventHandler<ControllerEventManagerArgs> Event;
		
		/// <summary>
		/// Occurs when the document report button is pressed
		/// </summary>
		/// <remarks>
		/// You can use this event to pass information to the report
		/// </remarks>
		[LocalizedCategory("EventsCategory", typeof(EBCategories))]
		[Description("Occurs when the document report button is pressed")]
		public event EventHandler<WoormEventArgs> Report;

		/// <summary>
		/// Occurs when an exception is raised in any part of the customization.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ExceptionRaisedEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("EventsCategory", typeof(EBCategories))]
		[Description("Occurs when an exception is raised in any part of the customization.")]
		public event EventHandler<ExceptionRaisedEventArgs> ExceptionRaised;
	
		/// <summary>
		/// Occurs before a document starts a batch procedure
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnBeforeBatchExecute" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("BatchCategory", typeof(EBCategories))]
		[Description("Occurs before a document starts a batch procedure")]
		[BatchFilter]
		public event EventHandler<ControllerEventArgs> BatchExecuting;
		/*
		/// <summary>
		/// Occurs when a record in a batch procedure is being processed (when and if it's called depends on the specific procedure)
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnDuringBatchExecute" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.BatchEventArgs"/>
		[LocalizedCategory("BatchCategory", typeof(EBCategories))]
		[Description("Occurs when a record in a batch procedure is being processed (when and if it's called depends on the specific procedure)")]
		[BatchFilter]
		public event EventHandler<BatchEventArgs> BatchProcessingRecord;
		 */


		/// <summary>
		/// Occurs when a document executes a batch procedure
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnBatchExecute" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("BatchCategory", typeof(EBCategories))]
		[Description("Occurs when a document executes a batch procedure")]
		[BatchFilter]
		public event EventHandler<ControllerEventArgs> BatchExecute;

		/// <summary>
		/// Occurs when a document has finished a batch procedure
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnAfterBatchExecute" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("BatchCategory", typeof(EBCategories))]
		[Description("Occurs when a document has finished a batch procedure")]
		[BatchFilter]
		public event EventHandler<ControllerEventArgs> BatchExecuted;
	
		// metodi coinvolti nello lo stato del documento

		/// <summary>
		/// Occurs before a record deletion.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnBeforeDeleteRecord" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		[Description("Occurs before a record deletion: Represents the TaskBuilderNet C++ framework \"OnBeforeDeleteRecord\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> DeletingRecord;

		/// <summary>
		/// Occurs after a record deletion.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnOkDelete" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		[Description("Occurs after a record deletion: Represents the TaskBuilderNet C++ framework \"OnOkDelete\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> RecordDeleted;

		/// <summary>
		/// Occurs before a document enters the "new" state.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnBeforeNewRecord" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		[Description("Occurs before a document enters the \"new\" state: Represents the TaskBuilderNet C++ framework \"OnBeforeNewRecord\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> EnteringInNew;

		/// <summary>
		/// Occurs after a document entered the "new" status.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnOkNewRecord" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		[Description("Occurs after a document entered the \"new\" state: Represents the TaskBuilderNet C++ framework \"OnOkNewRecord\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> EnteredInNew;

		/// <summary>
		/// Occurs before a document enters the "edit" status.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnBeforeEditRecord" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		[Description("Occurs before a document enters the \"edit\" status: Represents the TaskBuilderNet C++ framework \"OnBeforeEditRecord\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> EnteringInEdit;

		/// <summary>
		/// Occurs after a document entered the "edit" status.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnOkEdit" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		[Description("Occurs after a document entered the \"edit\" status: Represents the TaskBuilderNet C++ framework \"OnOkEdit\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> EnteredInEdit;

		/// <summary>
		/// Occurs after a document entered the "browse" status.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnGoInBrowseMode" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		[Description("Occurs after a document entered the \"browse\" status: Represents the TaskBuilderNet C++ framework \"OnGoInBrowseMode\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> EnteredInBrowse;

		/// <summary>
		/// <list type="bullet">
		/// <listheader>
		/// <description>Occurs after all UI controls are enabled by the TaskBuilder.Net C++ document.
		/// Here you can enable/disable your own UI controls and standard document UI
		/// controls as well simply testing the Document.FormMode property to know
		/// which is the current document status between following possible values:</description>
		/// </listheader>
		/// <item>
		/// <term>None</term>
		/// <description>the default value</description>
		///  </item>
		/// <item><term>Browse</term>
		/// </item>
		/// <item><term>New</term>
		/// </item>
		/// <item><term>Edit</term>
		/// </item>
		/// <item><term>Find</term>
		/// </item>
		/// </list>
		/// </summary>
		/// <list type="bullet">
		/// <listheader>
		/// <description>Represents following TaskBuilderNet C++ framework events:
		/// </description>
		/// </listheader>
		/// <item><term>OnBeforeNewRecord</term></item>
		/// <item><term>OnDisableControlsForAlways</term></item>
		/// <item><term>OnEnableControlsForFind</term></item>
		/// <item><term>OnDisableControlsForAddNew</term></item>
		/// <item><term>EnableControlsForFind</term></item>
		/// <item><term>DisableControlsForAddNew</term></item>
		/// </list>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		/// <seealso cref="Microarea.Framework.TBApplicationWrapper.MDocument"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		[Description("Occurs after all UI controls are enabled by the TaskBuilder.Net C++ document: " +
					"Represents following TaskBuilderNet C++ framework events: OnBeforeNewRecord, OnDisableControlsForAlways, OnEnableControlsForFind" +
					"OnDisableControlsForAddNew, EnableControlsForFind and DisableControlsForAddNew")]
		public event EventHandler<ControllerEventArgs> ControlsEnabled;

		/// <summary>
		/// Occurs before standard primary transaction commit performed in order to persist a document. This event allows to update auxiliary data.
		/// If e.Cancel is setted, the primary transaction will be RolledBack.
		/// </summary>		
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnBeforeNewTransaction, OnBeforeEditTransaction, OnBeforeDeleteTransaction" method.
		/// The mode of the transaction can be tested using TransactionEventArgs Mode property
		/// </remarks>	
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentTransactionsCategory", typeof(EBCategories))]
		[Description("Occurs before standard primary transaction commit performed in order to persist a document: Represents the TaskBuilderNet C++ framework \"OnBeforeNewTransaction, OnBeforeEditTransaction, OnBeforeDeleteTransaction\" methods")]
		[NonBatchFilter]
		public event EventHandler<TransactionEventArgs> Transacting;

		/// <summary>
		/// Occurs after standard primary transaction but before commit operation is performed in order to persist a document. This event allows to update auxiliary data.
		/// If e.Cancel is setted, the primary transaction will be RolledBack.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnNewTransaction, OnEditTransaction, OnDeleteTransaction" method.
		/// The mode of the transaction can be tested using TransactionEventArgs Mode property
		/// </remarks>	
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentTransactionsCategory", typeof(EBCategories))]
		[Description("Occurs after standard primary transaction but before commit operation is performed in order to persist a document: Represents the TaskBuilderNet C++ framework \"OnNewTransaction, OnEditTransaction, OnDeleteTransaction\" method")]
		[NonBatchFilter]
		public event EventHandler<TransactionEventArgs> Transacted;

		/// <summary>
		/// Occurs after all data are loaded.
		/// Here you can load your own data.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnPrepareAuxData" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DataCategory", typeof(EBCategories))]
		[Description("Occurs after all data are loaded: Represents the TaskBuilderNet C++ framework \"OnPrepareAuxData\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> DataLoaded;

		/// <summary>
		/// Occurs after all data are initialized.
		/// Here you can initialize your own data.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnInitAuxData" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DataCategory", typeof(EBCategories))]
		[Description("Occurs after all data are initialized: Represents the TaskBuilderNet C++ framework \"OnInitAuxData\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> DataInitialized;

		//siccome questi sono i metodi di transazione secondaria dobbiamo definire cosa vogliamo cosentire di fare 

		/// <summary>
		/// Occurs after data correctness has been checked from the document.
		/// If all is ok, then a transaction to save all data can start.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnOkTransaction" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentTransactionsCategory", typeof(EBCategories))]
		[Description("Occurs after data correctness has been checked from the document: Represents the TaskBuilderNet C++ framework \"OnOkTransaction\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> DataForTransactionChecked;	

		/// <summary>
		/// Occurs before the document data correctness check.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnBeforeOkTransaction" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentTransactionsCategory", typeof(EBCategories))]
		[Description("Occurs before the document data correctness check: Represents the TaskBuilderNet C++ framework \"OnBeforeOkTransaction\" method")]
		[NonBatchFilter]
		public event EventHandler<ControllerEventArgs> CheckingDataForTransaction;

		/// <summary>
		/// Occurs after document primary transaction is committed and is called in order to perform extra transactions. This event allows to update auxiliary data after the document is saved using StartTransaction, Commit, Rollback commands.
		/// If e.Cancel is setted, the extra transactions will be RolledBack, but not the primary one.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnExtraNewTransaction, OnExtraEditTransaction, OnExtraDeleteTransaction" method.</remarks>
		/// The mode of the transaction can be tested using TransactionEventArgs Mode property
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentTransactionsCategory", typeof(EBCategories))]
		[Description("Occurs after document primary transaction is committed and is called in order to perform extra transactions: Represents the TaskBuilderNet C++ framework \"OnExtraNewTransaction, OnExtraEditTransaction, OnExtraDeleteTransaction\" methods")]
		[NonBatchFilter]
		public event EventHandler<TransactionEventArgs> ExtraTransacted;
				
		/// <summary>
		/// Occurs when document is locked in New,Edit,Delete operations. It allows to lock data involved in primary and extra transaction.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnLockDocumentForNew, OnLockDocumentForEdit, OnLockDocumentForDelete" method.</remarks>
		/// The mode of the transaction can be tested using LockEventArgs Mode property
		/// If property Result is set to LockResult.Locked, transaction will start; 
		/// if property Result is set to LockResult.NoDataToLock or LockResult.LockFailed, transaction operation will not start
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentTransactionsCategory", typeof(EBCategories))]
		[Description("Occurs when document is locked in New,Edit,Delete operations: Represents the TaskBuilderNet C++ framework \"OnLockDocumentForNew, OnLockDocumentForEdit, OnLockDocumentForDelete\" methods")]
		[NonBatchFilter]
		public event EventHandler<LockEventArgs> DocumentLocked;
		
		// ciclo di vita del documento

		/// <summary>
		/// Occurs before the document is loaded.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnInitDocument" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentLifeCycleCategory", typeof(EBCategories))]
		[Description("Occurs before the document is loaded: Represents the TaskBuilderNet C++ framework \"OnInitDocument\" method")]
		public event EventHandler<ControllerEventArgs> LoadingDocument;	

		/// <summary>
		/// Occurs after the document is loaded.
		/// Here you can load all objects that will live as long as the current document.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnAttachData" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentLifeCycleCategory", typeof(EBCategories))]
		[Description("Occurs after the document is loaded: Represents the TaskBuilderNet C++ framework \"OnAttachData\" method")]
		public event EventHandler<ControllerEventArgs> DataAttached;

        /// <summary>
        /// Occurs after the main data are attached.
         /// </summary>
        /// <remarks>Represents the TaskBuilderNet C++ framework "OnAttachData" method.</remarks>
        /// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
        //-----------------------------------------------------------------------------
        [LocalizedCategory("DocumentLifeCycleCategory", typeof(EBCategories))]
        [Description("Occurs after the document is loaded: Represents the TaskBuilderNet C++ framework \"OnAttachData\" method")]
        public event EventHandler<ControllerEventArgs> DocumentLoaded;

		/// <summary>
		/// Occurs before closing a document.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnDocumentCreated" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentLifeCycleCategory", typeof(EBCategories))]
		[Description("Occurs before closing a document: Represents the TaskBuilderNet C++ framework \"OnBeforeCloseDocument\" method")]
		public event EventHandler<ControllerEventArgs> ClosingDocument;

		/// <summary>
		/// Occurs after a document has been closed.
		/// </summary>
		/// <remarks>Represents the TaskBuilderNet C++ framework "OnCloseDocument" method.</remarks>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventArgs"/>
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentLifeCycleCategory", typeof(EBCategories))]
		[Description("Occurs after a document has been closed: Represents the TaskBuilderNet C++ framework \"OnCloseDocument\" method")]
		public event EventHandler<ControllerEventArgs> DocumentClosed;

#if DEBUG
		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		internal TestScripting TestScripting(int a, int b)
		{
			return new TestScripting(a, b);
		}
#endif
		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		protected internal virtual void OnExceptionRaised(Exception e)
		{
			if (ExceptionRaised != null)
				ExceptionRaised(this, new ExceptionRaisedEventArgs(e));
		}

		/// <summary>
		/// Gets the name given to the programmative variable generated to serialize
		/// this instance.
		/// </summary>
		//-----------------------------------------------------------------------------
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string SerializedName { get { return EasyBuilderSerializer.ControllerVariableName; } }

		/// <summary>
		/// Gets or sets the namespace for the customization.
		/// </summary>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.Generic.NameSpace"/>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.INameSpace"/>
		//-----------------------------------------------------------------------------
		virtual public INameSpace CustomizationNameSpace { get { return customizationNameSpace; } }
		
		/// <summary>
		/// Gets or sets the name for this DocumnentController instance.
		/// </summary>
		/// <remarks>For the DocumnentController the name is always String.Empty and
		/// cannot be set.</remarks>
		//-----------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public override string Name { get { return customizationNameSpace.Leaf; } set { } }

		/// <summary>
		/// Gets the MDocument attached to this DocumentController.
		/// </summary>
		/// <seealso cref="Microarea.Framework.TBApplicationWrapper.MDocument"/>
		/// <exception cref="System.InvalidOperationException">
		/// This instance of DocumentController doesn't have any MDocument associated.
		/// The DocumentController has not been initialized.
		/// </exception>
		//-----------------------------------------------------------------------------
		public virtual new MDocument Document
		{
			get
			{
				if (document == null)
					throw new InvalidOperationException(Resources.ControllerInitializeException);

				return document;
			}
		}

		/// <summary>
		/// Gets the DocumentView attached to this DocumentController.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentView"/>
		/// <exception cref="System.InvalidOperationException">
		/// This instance of DocumentController doesn't have any DocumentView associated.
		/// The DocumentController has not been initialized.
		/// </exception>
		//-----------------------------------------------------------------------------
		public DocumentView View
		{
			get
			{
				/*if (view == null)
					throw new InvalidOperationException(Resources.ControllerInitializeException);
                */
				return view;
			}
			set {  view = value; } 
		}
		//-----------------------------------------------------------------------------
		/// <summary>
		/// Internal use
		/// </summary>
		protected internal DocumentController()
		{

		}
        //-----------------------------------------------------------------------------
		static DocumentController()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
		}
		static List<string> notResolvedAssemblies = new List<string>();
		//-----------------------------------------------------------------------------
		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (args == null || args.RequestingAssembly == null)
				return null;

			if (notResolvedAssemblies.Contains(args.Name))
				return null;

			AssemblyName an = new AssemblyName(args.Name);
			if (an.Name.EndsWith("resources"))
				return null;

			string application, module;
			if (!OwnerEasyBuilderAppAttribute.GetModuleInfo(args.RequestingAssembly, out application, out module))
                return null;

			string moduleFolderPath = Path.GetTempPath();

			string dllPath = Path.Combine(moduleFolderPath, an.Name + ".dll");

			if (File.Exists(dllPath))
				return AssembliesLoader.Load(dllPath);

			moduleFolderPath = Path.GetDirectoryName(BasePathFinder.BasePathFinderInstance.GetEBModuleDllPath(application, module));

			dllPath = Path.Combine(moduleFolderPath, an.Name + ".dll");

			if (File.Exists(dllPath))
				return AssembliesLoader.Load(dllPath);

			notResolvedAssemblies.Add(args.Name);
			return null;
		}

		/// <summary>
		///  Initializes a new instance of DocumentController with the given namespace.
		/// </summary>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.Generic.NameSpace"/>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.INameSpace"/>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public DocumentController(NameSpace nameSpace)
			:this()
		{
			string name = GetControllerNameByReflection();
			customizationNameSpace = name.IsNullOrEmpty()
				? nameSpace
				: new NameSpace(string.Format("{0}.{1}", nameSpace.FullNameSpace, name));
		}

		/// <summary>
		/// constructor used by business objects 
		/// </summary>
		//-----------------------------------------------------------------------------
		public DocumentController(MDocument document)
			:this()
		{
			this.document = document;

			string controllerName = GetNameByDocumentNamespace(document);

			customizationNameSpace = string.Format
				(
				"{0}.{1}.{2}",
				GetControllerTypeByReflection(),
				this.document.Namespace.GetNameSpaceWithoutType(),
				controllerName
				);
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Returns false if the event has not to be shown in the property grid
		/// </summary>
		public override bool CanShowEvent(string eventName, TBEventFilterAttribute filter)
		{
			if (filter is BatchFilterAttribute)
				return Document.Batch;
			if (filter is NonBatchFilterAttribute)
				return !Document.Batch;
			return true;
		}

		/// <summary>
		/// Estrae dall' MDocument il nome vero e proprio da usare come nome del controller
		/// In questo caso non è il namespace del document (ERP.App.Mod....) ma il namespace programmativo della
		/// classe MDocument
		/// </summary>
		//-----------------------------------------------------------------------------
		private string GetNameByDocumentNamespace(MDocument document)
		{
			string[] tokens = document.GetType().Namespace.Split('.');
			if (tokens == null || tokens.Length <= 0)
				return string.Empty;

			string controllerName = tokens[tokens.Length - 1];
			if (controllerName.IsNullOrEmpty())
				return string.Empty;

			return controllerName;
		}

		/// <summary>
		/// Analizza la dll corrente della customizzazione e ritorna il tipo (Standardization, Customization)
		/// </summary>
		//-----------------------------------------------------------------------------
		private string GetControllerTypeByReflection()
		{
			int firstDot = this.GetType().Namespace.IndexOf(".");
			if (firstDot < 0)
				return string.Empty;

			string type = this.GetType().Namespace.Left(firstDot);
			if (type.IsNullOrEmpty())
				return string.Empty;

			return type;
		}

		/// <summary>
		/// Analizza la dll corrente della customizzazione, ne estrare la classe del DocumentController (tipizzata)
		/// e ne ritorna il nome 
		/// </summary>
		//-----------------------------------------------------------------------------
		private string GetControllerNameByReflection()
		{
			NameSpace tempNs = null;
			foreach (Type t in this.GetType().Assembly.GetTypes())
			{
				if (t.IsSubclassOf(typeof(DocumentController)))
				{
					tempNs = t.Namespace;
					break;
				}
			}

			if (tempNs == null || tempNs.FullNameSpace.IsNullOrEmpty())
			{
				//Debug.Fail("Invalid Controller Namespace");
				return string.Empty;
			}

			return tempNs.Leaf;
		}
		
		/// <summary>
		/// Opens a new connection to current database using current credentials
		/// </summary>
		//-----------------------------------------------------------------------------
		public IDbConnection OpenConnectionToCurrentCompany()
		{
			return CUtility.OpenConnectionToCurrentCompany();
		}

		/// <summary>
		/// Creates all children components.
		/// </summary>
		/// <remarks>
		/// The DocumentController has only two children: the MDocument and the DocumentView.
		/// </remarks>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override void CreateComponents()
		{
			
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public override void OnAfterCreateComponents()
		{
		}

		/// <summary>
		/// Apply all localized resources.
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override void ApplyResources()
		{
			this.ApplyResources(true);
		}

		/// <summary>
		/// Deletes all children components.
		/// </summary>
		/// <remarks>
		/// The DocumentController has only two children: the MDocument and the DocumentView.
		/// </remarks>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override void ClearComponents() { }

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public object ExecuteScript(string script, string returnType, params ScriptParameter[] args)
		{
			return ScriptManager.ExecuteScript(script, returnType, args);
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		internal void ReactToManagedClientDocEvent(ControllerEventManagerArgs args)
		{
			if (Event != null)
				Event(this, args);
		}
		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		internal void ReactToManagedClientDocEvent(WoormEventArgs args)
		{
			if (Report != null)
				Report(this, args);
		}
		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		internal void ReactToManagedClientDocEvent(BatchEventArgs args)
		{
		//	if (BatchProcessingRecord != null)
		//		BatchProcessingRecord(this, args);
		}
		/// <summary>
		/// Dispathces the ManagedClientDocEvent.
		/// </summary>
		/// <param name="clientDocEvent">The event argument describing the document event.</param>
		/// <param name="args">The event argument related with document event.</param>
		/// <seealso cref="Microarea.EasyBuilder.ManagedClientDocEvent"/>
		/// <returns>True to cancel the event prpoagation, otherwise false.</returns>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public bool ReactToManagedClientDocEvent(ManagedClientDocEvent clientDocEvent, ControllerEventArgs args = null)
		{
			switch (clientDocEvent)
			{
				case ManagedClientDocEvent.OnOkDelete:
					if (RecordDeleted != null)
						RecordDeleted(this, args);
					break;
				case ManagedClientDocEvent.OnOkEdit:
					if (EnteredInEdit != null)
						EnteredInEdit(this,args);
					break;
				case ManagedClientDocEvent.OnOkNewRecord:
					if (EnteredInNew != null)
						EnteredInNew(this,args);
					break;
				case ManagedClientDocEvent.OnBeforeDeleteRecord:
					if (DeletingRecord != null)
						DeletingRecord(this, args);
					break;
				case ManagedClientDocEvent.OnBeforeEditRecord:
					if (EnteringInEdit != null)
						EnteringInEdit(this, args);
					break;
				case ManagedClientDocEvent.OnBeforeNewRecord:
					if (EnteringInNew != null)
						EnteringInNew(this, args);
					break;
				case ManagedClientDocEvent.OnBeforeOkTransaction:
					if (CheckingDataForTransaction != null)
						CheckingDataForTransaction(this, args);
					break;
				case ManagedClientDocEvent.OnOkTransaction:
					if (DataForTransactionChecked != null)
						DataForTransactionChecked(this,args);
					break;
                case ManagedClientDocEvent.OnAttachData:
                    if (DataAttached != null)
                        DataAttached(this, args);
                    break;
				
				case ManagedClientDocEvent.OnDocumentCreated:
					if (DocumentLoaded != null)
						DocumentLoaded(this,args);
					break;
				case ManagedClientDocEvent.OnPrepareAuxData:
					{
						if (DataLoaded != null)
							DataLoaded(this, args);
						break;
					}
				case ManagedClientDocEvent.OnInitAuxData:
					if (DataInitialized != null)
						DataInitialized(this,args);
					break;
				case ManagedClientDocEvent.OnInitDocument:
					if (LoadingDocument != null)
						LoadingDocument(this,args);
					break;
				case ManagedClientDocEvent.OnBeforeCloseDocument:
					if (ClosingDocument != null)
						ClosingDocument(this,args);
					break;
				case ManagedClientDocEvent.OnCloseDocument:
					if (DocumentClosed != null)
						DocumentClosed(this,args);
					break;
				case ManagedClientDocEvent.OnGoInBrowseMode:
					if (EnteredInBrowse != null)
						EnteredInBrowse(this,args);
					break;
				case ManagedClientDocEvent.OnDisableControlsAlways:
				case ManagedClientDocEvent.OnDisableControlsForAddNew:
				case ManagedClientDocEvent.OnDisableControlsForEdit:
				case ManagedClientDocEvent.OnEnableControlsForFind:
				case ManagedClientDocEvent.OnDisableControlsForBatch:
					if (ControlsEnabled != null)
						ControlsEnabled(this,args);
					break;

				case ManagedClientDocEvent.OnExtraTransaction:
					if (ExtraTransacted != null)
						ExtraTransacted(this,(TransactionEventArgs)args);
					break;
				case ManagedClientDocEvent.OnBeforeTransaction:
					if (Transacting != null)
						Transacting(this,(TransactionEventArgs)args);
					break;
				case ManagedClientDocEvent.OnTransaction:
					if (Transacted != null)
						Transacted(this,(TransactionEventArgs)args);
					break;
				case ManagedClientDocEvent.OnLockDocument:
					if (DocumentLocked != null)
						DocumentLocked(this, (LockEventArgs) args);
					break;
				case ManagedClientDocEvent.OnBeforeBatchExecute:
					if (BatchExecuting != null)
						BatchExecuting(this, args);
					break;
				case ManagedClientDocEvent.OnAfterBatchExecute:
					if (BatchExecute != null)
						BatchExecute(this, args);//prima eseguo la batch (TODO: solo per server document controllers)
					if (BatchExecuted != null)
						BatchExecuted(this, args);//poi eseguo la after
					break;
				default:
					return true;
			}
			
			return args == null ? true : !args.Cancel;
		}


		/// <summary>
		/// Adds the given component as a child, using the given name.
		/// </summary>
		/// <param name="component">The component to be added as a child</param>
		/// <param name="name">The name to be given to the added component</param>
		/// <seealso cref="System.ComponentModel.IComponent"/>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override void Add(IComponent component, string name)
		{
			var v = component as DocumentView;
			var d = component as MDocument;
			if (v != null)
			{
				view = v;
				view.ParentComponent = this;
			}
			else if (d != null)
			{
				document = d;
				document.ParentComponent = this;
			}
			base.Add(component, name);
		}

		/// <summary>
		/// Adds the given component as a child.
		/// </summary>
		/// <param name="component">The component to be added as a child</param>
		/// <seealso cref="System.ComponentModel.IComponent"/>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override void Add(IComponent component)
		{
			Add(component, null);
		}

		/// <summary>
		/// Gets a copy of the collection of children components.
		/// </summary>
		/// <remarks>
		/// The DocumentController has only two children: the MDocument and the DocumentView.
		/// </remarks>
		/// <seealso cref="System.ComponentModel.ComponentCollection"/>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		[Browsable(false)]
		public override ComponentCollection Components
		{
			get { return new ComponentCollection(components.ToArray()); }
		}

        /// <summary>
        /// Remove the given component from the collection of children.
        /// </summary>
        /// <param name="component">The component to be removed from DocumentController components collection</param>
        /// <seealso cref="System.ComponentModel.IComponent"/>
        //-----------------------------------------------------------------------------
        [ExcludeFromIntellisense]
		public override void Remove(IComponent component)
		{
			components.Remove(component);
		}

		/// <summary>
		/// Releases all resources used by the DocumentController.
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		protected override void Dispose(bool bo)
		{
			if (disposed)
				return;
			if (view != null)
			{
				view.Dispose();
				view = null;
			}
			if (document != null)
			{
				document.Dispose();
				document = null;
			}
			base.Dispose(bo);
		}

		/// <summary>
		/// Returns an IComponent given its INameSpace
		/// </summary>
		/// <param name="nameSpace">The namespace identifying the IComponent to be retrieved</param>
		/// <seealso cref="System.ComponentModel.IComponent"/>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.INameSpace"/>
		/// <returns>The propertyFound IComponent, if any. Otherwise null</returns>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public IComponent GetObjectByNamespace(INameSpace nameSpace)
		{
			return GetObjectByNamespace(nameSpace, Components);
		}

		//--------------------------------------------------------------------------------
		private IComponent GetObjectByNamespace(INameSpace nameSpace, ComponentCollection componentCollection)
		{
			IWindowWrapper windowWrapper = null;
			IContainer container = null;
			IComponent componentFound = null;

			foreach (IComponent component in componentCollection)
			{
				container = component as IContainer;
				if (container == null)
					continue;
				componentFound = GetObjectByNamespace(nameSpace, container.Components);
				if (componentFound != null)
					return componentFound;

				windowWrapper = component as IWindowWrapper;
				if (windowWrapper == null)
					continue;

				if (nameSpace.FullNameSpace.CompareNoCase(windowWrapper.Namespace.FullNameSpace))
					return component;
			}
			
			return null;
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		private void ApplyResources(bool recursive)
		{
            View.ApplyResources(recursive);
		}

		#region ICustomTypeDescriptor Members

		/// <summary>
		/// Returns all the events for the current instance.
		/// </summary>
		/// <seealso cref="System.ComponentModel.EventDescriptorCollection"/>
		/// <returns>The collection of all events of the current instance.</returns>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public  override EventDescriptorCollection GetEvents()
		{
			return GetEvents(null); 
		}

		/// <summary>
		/// Returns all the properties for the current instance.
		/// </summary>
		/// <seealso cref="System.ComponentModel.PropertyDescriptorCollection"/>
		/// <returns>The collection of all properties of the current instance.</returns>
		/// <param name="attributes">Attributes to filter properties.</param>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public  override PropertyDescriptorCollection GetProperties(System.Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(GetType(), attributes);
		}

		#endregion

		/// <summary>
		/// Returns a String containing the name of the DocumentController, if any
		/// </summary>
		/// <returns>A String containing the name of the DocumentController, if any</returns>
		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			string controllerName = GetType().Namespace;
			controllerName = controllerName.Substring(controllerName.LastIndexOf('.') + 1);

			return controllerName;
		}

		/// <summary>
		/// Adds a message to be showed to the user using the standard
		/// TaskBuilderNet user interface.
		/// </summary>
		/// <param name="message">A message to be showed to the user</param>
		//-----------------------------------------------------------------------------
		public void AddMessage(string message)
		{
			Document.AddMessage(message, DiagnosticType.Error);
		}

		/// <summary>
		/// Adds a message to be showed to the user using the standard
		/// TaskBuilderNet user interface.
		/// </summary>
		/// <param name="message">A message to be showed to the user</param>
		/// <param name="type">
		/// The type of diagnostic to be given.
		/// </param>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.DiagnosticType"/>
		//-----------------------------------------------------------------------------
		public void AddMessage(string message, DiagnosticType type)
		{
			Document.AddMessage(message, type);
		}

		/// <summary>
		/// Shows e message on the opening banner.
		/// </summary>
		/// <param name="openingBanner">A message to be showed</param>
		//-----------------------------------------------------------------------------
		public void StartMessageSession(string openingBanner)
		{
			Document.StartMessageSession(openingBanner);
		}

		/// <summary>
		/// Shows e message on the closing banner.
		/// </summary>
		/// <param name="closingBanner">A message to be showed</param>
		//-----------------------------------------------------------------------------
		public void EndMessageSession(string closingBanner)
		{
			Document.EndMessageSession(closingBanner);
		}

		/// <summary>
		/// Returns an IComponent given its path in the object model tree.
		/// </summary>
		/// <param name="componentPath">The path of the IComponent to be propertyFound in the object model</param>
		/// <seealso cref="System.ComponentModel.IComponent"/>
		/// <returns>An IComponent from the object model tree, if any; null otherwise.</returns>
		/// <remarks>The DocumentController is the root of the object model tree.</remarks>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public IComponent GetComponentByPath(string componentPath)
		{
			// Se la stringa è vuota do per scontato che mi indichi "ritorna il corrente", cioè il controller.
			if (String.IsNullOrWhiteSpace(componentPath))
				return this;

			// Se la stringa comincia per "controller" allora lo levo.
			if (componentPath.StartsWith(EasyBuilderSerializer.ControllerVariableName))
				componentPath = componentPath.Replace(EasyBuilderSerializer.ControllerVariableName, String.Empty);

			// Se la stringa comincia per "." (dopo la rimozione precedente potrebbe accadere) allora lo levo.
			if (componentPath.StartsWith("."))
				componentPath = componentPath.Substring(1);

			// Se la stringa è vuota do (dopo le epurazioni precedenti) per scontato che mi indichi il controller.
			if (componentPath == String.Empty)
				return this;

			String[] tokens = componentPath.Split('.');

			IComponent foundComponent = null;
			IContainer container = null;
			ComponentCollection components = Components;
			foreach (string token in tokens)
			{
				foundComponent = null;
				foreach (IComponent component in components)
				{
					if (component.Site.Name == token)
					{
						foundComponent = component;
						container = component as IContainer;
						if (container != null)
							components = container.Components;
						break;
					}
				}
			}

			return foundComponent;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Returns the Component parent of the child fullpath
		/// </summary>
		/// <returns></returns>
		[ExcludeFromIntellisense]
		public IComponent GetParentComponentByChildPath(string componentPath)
		{
			string parent = componentPath.Substring(0, componentPath.LastIndexOf("."));
			return GetComponentByPath(parent);
		}

		//--------------------------------------------------------------------------------
		/// <remarks />
		[ExcludeFromIntellisense]
		public bool BelongsToObjectModel(IComponent component)
		{
			if (component == null)
				return false;

			if (component == this)
				return true;

			if (component.Site == null || component.Site.Container == null)
				return false;
			/*TODO MATTEO:
			 * inizio bruttura per il reperimento di un dataobj nel nostro object model.**********************************
			 * Ciò è dovuto a come è fatta la MSqlRecord::Add.
			 * MSqlRecord è un Container di MSqlRecordItem che a loro volta puntano degli MDataObj.
			 * l'MDataObj invece ha impostato come container MSqlRecord e non l'MSQLRecordItem (questa è la deroga al nostro modello).
			 * In pratica il data obj conosce il suo contenitore ma il suo contenitore non lo conosce perchè
			 * in realtà sta contenendo un MSqlRecordItem e non un MDataObj.
			 * La Add di un component in MSQLRecord non fa altro che sincronizzare i dataobj puntati dagli MSQRecordItem.
			 * Questa deroga fa si che nel normale ciclo IContaner/IComponent non resco mai ad arrivare al dataobj.
			 * Quindi, per dedurre se un data obj appartiene al model, valuto se il suo container
			 * (cioè l'MSQLRecord) appartiene al model.
			 * */
			IComponent toBeSearched = component;
			Type mDataObjType = typeof(MDataObj);
			Type componentType = component.GetType();

			if (
				componentType == mDataObjType ||
				componentType.IsSubclassOf(mDataObjType)
				)
				toBeSearched = component.Site.Container as IComponent;
			//Fine bruttura***********************************************************************************************

			return BelongsToContainer(this, toBeSearched);
		}

		//--------------------------------------------------------------------------------
		private bool BelongsToContainer(IContainer container, IComponent component)
		{
			bool found = false;
			IContainer tempContainer = null;
			foreach (IComponent comp in container.Components)
			{
				if (comp == component)
				{
					found = true;
					break;
				}
				tempContainer = comp as IContainer;
				if (tempContainer != null)
				{
					found = BelongsToContainer(tempContainer, component);
					if (found)
						break;
				}
                IEasyBuilderComponentExtendable extendable = comp as IEasyBuilderComponentExtendable;
                if (extendable != null)
                {
                    foreach (var extension in extendable.Extensions)
                    {
                        if (extension == component)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            }
			return found;
		}

        //--------------------------------------------------------------------------------
        internal string GetJsonFormsPath(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                return assemblyPath;
            string path = assemblyPath.ToLower();
            string[] tokens = path.Split(Path.DirectorySeparatorChar);
            string jsonFormsPath = string.Empty;
            bool stopNext = false;
            for (int i=0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (token.EndsWith(":"))
                    jsonFormsPath = string.Concat(token, Path.DirectorySeparatorChar);
                else
                    jsonFormsPath = Path.Combine(jsonFormsPath, token);

                // mi stoppo alla cartella di documento
                if (stopNext)
                    break;

                if (string.Compare(NameSolverStrings.ModuleObjects, token, true) == 0)
                    stopNext = true;
            }

            jsonFormsPath = Path.Combine(jsonFormsPath, NameSolverStrings.JsonForms);

            return jsonFormsPath;
        }

        //--------------------------------------------------------------------------------
        [ExcludeFromIntellisense]
        internal void Init(string assemblyPath)
        {
            if (PathFinderWrapper.IsRemoteInterface())
                LoadJsonEvents(assemblyPath);
        }

        //--------------------------------------------------------------------------------
        [ExcludeFromIntellisense]
        internal void LoadJsonEvents(string assemblyPath)
        {
            try
            {
                string fileName = Path.Combine(GetJsonFormsPath(assemblyPath), EventsJson.FileName);
                if (!PathFinderWrapper.ExistFile(fileName))
                    return;
        
                // TODOBRUNA da farsi ritornare lo stream dal path finder
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();

                    JSONEvents = JsonConvert.DeserializeObject<JsonEvents>(json);
                    r.Close();
                }
            }
            catch (Exception e)
            {
                throw(e);
            }
        }

		#region IEasyBuilderContainer Members

		/// <summary>
		/// Gets the name of the actual type to be serialized by EasyBuilder for the
		/// current customization.
		/// </summary>
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public override string SerializedType
		{
			get { return EasyBuilderSerializer.DocumentControllerClassName; }
		}
        /// <summary>
        /// Defines if controller can be loaded
        /// current customization.
        /// </summary>
        public bool CanBeLoaded
        {
            get
            {
                return canBeLoaded;
            }

            set
            {
                canBeLoaded = value;
            }
        }

        /// <summary>
        /// Returns a value indicating if the control belongs to the current object
        /// model given its name.
        /// </summary>
        /// <param name="controlName">the name of the component to be propertyFound.</param>
        /// <returns>True if the component with the given controlName is contained in
        /// the DocumentController components collection, otherwise false.</returns>
        //-----------------------------------------------------------------------------
        public override bool HasComponent(string controlName)
		{
			return EasyBuilderComponent.HasComponent(Components, controlName);
		}
		/// <summary>
		/// retrieves the component given the name
		/// </summary>
		/// <param name="controlName"></param>
		/// <returns></returns>
		public override IComponent GetComponent(string controlName)
		{
			return EasyBuilderComponent.GetComponent(Components, controlName);
		}

		#endregion
	}
}
