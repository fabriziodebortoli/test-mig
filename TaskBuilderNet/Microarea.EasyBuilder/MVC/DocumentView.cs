using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using ICSharpCode.NRefactory.CSharp;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.MVC
{
	/// <summary>
	/// Serializes an object graph to a series of CodeDOM statements for the DocumentView.
	/// </summary>
	//================================================================================
	public class ViewSerializer : EasyBuilderControlSerializer
	{
		/// <summary>
		/// La serialize della view, contrariamente a tutti gli altri serializzatori, non genera la "new" , ma solo la add...
		/// la new viene generata nel costruttore del controller
		/// </summary>
		//-----------------------------------------------------------------------------
		public override object Serialize(IDesignerSerializationManager manager, object current)
		{
			EasyBuilderComponent ebControl = current as EasyBuilderComponent;
			List<Statement> newCollection = new List<Statement>();
			IdentifierExpression variableDeclExpression = new IdentifierExpression(ebControl.SerializedName);

			//this.Add(document_ProspectiveSuppliers);
			Statement addStat = AstFacilities.GetInvocationStatement(
				new ThisReferenceExpression(),
				EasyBuilderSerializer.AddMethodName,
				variableDeclExpression,
				new PrimitiveExpression(ebControl.IsChanged)
				);

			newCollection.Add(addStat);

            // serializzazione della proprietà DPI
            MView view = current as MView;
            if (view.LastEditDPI != 96)
            {
                string propertyName = ReflectionUtils.GetPropertyName(() => view.LastEditDPI);
                PropertyInfo info = typeof(MView).GetProperty(propertyName);

                if (info != null)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(info.DeclaringType)[info.Name];
                    if (descriptor != null)
                        SerializeProperty(manager, newCollection, view, descriptor);
                }
            }

            return newCollection;
		}

		/// <summary>
		/// Returns a value indicating whether the given EasyBuilderComponent can be
		/// serialized or not.
		/// </summary>
		/// <remarks>An instance of the DocumentView class is always serializable
		/// by the ViewSerializer</remarks>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.EasyBuilder.EasyBuilderComponent"/>
		//-----------------------------------------------------------------------------
		public override bool IsSerializable(EasyBuilderComponent ebComponent)
		{
			return true;
		}

		/// <summary>
		/// Serializes the specified object into a CodeDOM object.
		/// </summary>
		/// <seealso cref="System.ComponentModel.Design.Serialization.IDesignerSerializationManager"/>
		//-----------------------------------------------------------------------------
		public override TypeDeclaration SerializeClass(SyntaxTree cu, IComponent control)
		{
			//creo la classe se non esiste
			EasyBuilderComponent ebComponent = control as EasyBuilderComponent;

			TypeDeclaration classStructure = new TypeDeclaration();
			classStructure.Modifiers = Modifiers.Public;
			classStructure.Name = ebComponent.SerializedType;
			classStructure.BaseTypes.Add(new SimpleType(typeof(DocumentView).FullName));
			ConstructorDeclaration constr = new ConstructorDeclaration();
			constr.Name = classStructure.Name;
			constr.Modifiers = Modifiers.Public;
			classStructure.Members.Add(constr);

			constr.Parameters.Add(
					new ParameterDeclaration(
						new SimpleType(typeof(IntPtr).FullName),
						EasyBuilderControlSerializer.WrappedObjectVariableName
						)
					);

			constr.Body = new BlockStatement();
			constr.Initializer = AstFacilities.GetConstructorInitializer(
				new IdentifierExpression(EasyBuilderControlSerializer.WrappedObjectVariableName)
				);

			AttributeSection attr = AstFacilities.GetAttributeSection(typeof(PreserveFieldAttribute).FullName);
			TypeDeclaration controllerClass = GetControllerTypeDeclaration(cu);
			PropertyDeclaration documentProperty = EasyBuilderSerializer.FindMember<PropertyDeclaration>(controllerClass, EasyBuilderSerializer.DocumentPropertyName);

			SerializeDocumentAccessor(classStructure, documentProperty.ReturnType);
			return classStructure;
		}

		//--------------------------------------------------------------------------------
		private static void SerializeDocumentAccessor(
			TypeDeclaration aClass,
			AstType documentTypeReference
			)
		{
			AttributeSection attr = AstFacilities.GetAttributeSection(typeof(PreserveFieldAttribute).FullName);

			PropertyDeclaration prop = new PropertyDeclaration();
			prop.Modifiers = Modifiers.Public | Modifiers.New;
			prop.Name = DocumentPropertyName;
			prop.ReturnType = new SimpleType(documentTypeReference.AstTypeToString());
			prop.Attributes.Add(attr);
			prop.Getter = new Accessor();
			prop.Getter.Body = new BlockStatement();
			prop.Getter.Body.Statements.Add
				(
					new ReturnStatement
						(
							new CastExpression
								(
									new SimpleType(documentTypeReference.AstTypeToString()),
									new IdentifierExpression(String.Format("{0}.{1}", EasyBuilderControlSerializer.StaticControllerVariableName, DocumentPropertyName))
								)
						)
				);

			aClass.Members.Add(prop);
		}
	}

	/// <summary>
	/// This the root object of the object model describing the user interface of the
	/// customized document.
	/// This is the base class for all views customization classes EasyBuilder generates
	/// to customize any document user interface.
	/// </summary>
	//=========================================================================
	[DesignerSerializer(typeof(ViewSerializer), typeof(CodeDomSerializer))]
	[ExcludeFromIntellisense]
	public class DocumentView : MView, IDocumentView
	{
		/// <summary>
		/// Occurs after the DocumentView is loaded.
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler Load;
		
		private bool keepTabsAlive = false;
	
		/// <summary>
		/// Gets or sets a value indicating whether tabs are to be kept alive during normal use
		/// of the document or not.
		/// </summary>
		/// <remarks>Internal use only, it is not intended to be used by the programmer.</remarks>
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public bool KeepTabsAlive { get { return keepTabsAlive; } set { keepTabsAlive = value; } }

		/// <summary>dd
		/// </summary>
		/// <remarks>Internal use only, it is not intended to be used by the programmer.</remarks>
		[Browsable(false)]
		public override IDocumentDataManager Document { get { return ((DocumentController)ParentComponent).Document; } }
		
		/// <summary>
		/// Internal use
		/// </summary>
		[Browsable(false)]
		//-----------------------------------------------------------------------------
        public override EditingMode DesignerMovable { get{ return EditingMode.None; } }

        /// <summary>
        ///  Initializes a new instance of the DocumentView with the given ptrView.
        /// </summary>
        /// <remarks>
        /// The ptrView represents the handle of the TaskBuilder.Net C++ window wrapped
        /// by this instance of DocumentView.
        /// </remarks>
        //-----------------------------------------------------------------------------
        [ExcludeFromIntellisense]
		public DocumentView(IntPtr ptrView)
			: base(ptrView)
		{
		}

		/// <summary>
		/// Creates instances of all children components.
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override void CreateComponents()
		{

		}

		/// <summary>
		/// Gets the name given to the programmative variable generated to serialize
		/// this instance.
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override string SerializedName
		{
			get
			{
				return "view_" + EasyBuilderSerializer.Escape(base.Name);
			}
		}

		/// <summary>
		/// Raises the Load event.
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public void RaiseOnLoad()
		{
			if (Load != null)
				Load(this, EventArgs.Empty);
		}

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		internal void ApplyResources(bool recursive)
		{
			//localizza i suoi controlli
			ApplyResources();

			if (!recursive)
				return;

			//TODOBRUNA
			//cicla su tutti i tabbers
			foreach (IComponent component in Components)
			{
				MTabber tbTabber = component as MTabber;
				if (tbTabber == null)
					continue;

				if (tbTabber.CurrentTab != null)
					tbTabber.CurrentTab.ApplyResources();
			}
		}

		/// <summary>
		/// Calls the CreateComponents method.
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override void CallCreateComponents() 
		{
            raise_SuspendLayout(this, new EasyBuilderEventArgs());

			//azzoro la scroll position, le coordinate dei controlli sono relative ad uno scrolling 0,0
			ScrollToPosition(System.Drawing.Point.Empty);
			
			// creo i controlli della view
			base.CallCreateComponents();

			// devo sistemare il keep alive delle tab
			foreach (IComponent component in Components)
			{
				IEasyBuilderContainer ebContainer = component as IEasyBuilderContainer;
				if (ebContainer == null)
					continue;

                MTabber mTabber = ebContainer as MTabber;
                if (mTabber != null)
                {
                    if (keepTabsAlive)
                        mTabber.KeepTabsAlive = true;

                    mTabber.ResumeDefaultLayout();
                }
            }

            raise_ResumeLayout(this, new EasyBuilderEventArgs());
			RequestRelayout();
		}

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		internal void EnableKeepAlive()
		{
			//cicla su tutti i tabbers
			foreach (IComponent component in Components)
			{
				MTabber tbTabber = component as MTabber;
				if (tbTabber == null)
					continue;

				tbTabber.KeepTabsAlive = true;
			}
		}
	}
}
