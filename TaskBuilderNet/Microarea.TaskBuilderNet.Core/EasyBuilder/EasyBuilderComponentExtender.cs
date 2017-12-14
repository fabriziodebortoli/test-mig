using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Localization;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.View;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using ICSharpCode.NRefactory.CSharp;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
    //=============================================================================
    /// <summary>
    /// Serializer of EasyBuilderComponentExtender object
    /// </summary>
    public class EasyBuilderComponentExtenderSerializer : EasyBuilderSerializer
    {
        //----------------------------------------------------------------------------	
        public override object Serialize(IDesignerSerializationManager manager, object current)
        {
            EasyBuilderComponentExtender extender = current as EasyBuilderComponentExtender;
            if (extender == null || extender.ParentComponent == null || !IsSerializable(extender as EasyBuilderComponent))
                return null;

			List<Statement> collection = new List<Statement>();
            string variableName = string.Concat(extender.ParentComponent.SerializedName, "_", extender.SerializedName);

            IEasyBuilderComponentExtendable extendableParent = extender.ParentComponent as IEasyBuilderComponentExtendable;

			VariableDeclarationStatement varStatement = new VariableDeclarationStatement(

				new SimpleType(extender.GetType().Name),
				variableName,
				new CastExpression
					(
						new SimpleType(extender.GetType().Name),
						new IndexerExpression(
								new IdentifierExpression(
									string.Concat(extender.ParentComponent.SerializedName, ".", ReflectionUtils.GetPropertyName(() => extendableParent.Extensions))
									),
								new List<Expression>() { new PrimitiveExpression(extender.SerializedName) }
							 )
					 )
					 );
  		  
            collection.Add(varStatement);

            SetExpression(manager, extender, new IdentifierExpression(variableName), true);

            // ora inizio la vera serializzazione
			IList<Statement> dbCollection = SerializeDataBinding(extender, variableName);
            if (dbCollection != null)
                collection.AddRange(dbCollection);

            // sono costretta a cambiare il name temporaneamente perchè la SerializeProerties
            // utilizza i SerializedName x molte sintassi
            string realName = extender.SerializedName;
            extender.Name = variableName;
            IList<Statement> coll = SerializeProperties(manager, extender, variableName);
            extender.Name = realName;
 
            if (coll != null)
                collection.AddRange(coll);

            return collection;
        }

		//-----------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		protected override AssignmentExpression GenerateCodeAttachEventStatement(
			string varName,
			EventInfo changedEvent,
			Expression handlerExpression
			)
		{
			return new AssignmentExpression(
				new MemberReferenceExpression(new IdentifierExpression(varName), changedEvent.EventName), 
				AssignmentOperatorType.Add,
				handlerExpression
				);
		}
    }

    [DesignerSerializer(typeof(EasyBuilderComponentExtenderSerializer), typeof(CodeDomSerializer))]
    //=============================================================================
    public class EasyBuilderComponentExtender : EasyBuilderComponent, IEasyBuilderComponentExtender
    {
        private string name;
        private IEasyBuilderComponentExtendable extendedObject = null;
  		/// <summary>
		/// Gets the name of the window wrapper container
		/// </summary>
		[ExcludeFromIntellisense, Browsable(false), LocalizedCategory("GeneralCategory", typeof(EBCategories))]
		public override string Name { get { return name; } set { name = value; } }

		/// <summary>
		/// Gets the name used for serializing the object 
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public override string SerializedName { get { return EasyBuilderSerializer.Escape(Name); } }

		/// <summary>
		/// Gets extended object
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEasyBuilderComponentExtendable ExtendedObject { get { return extendedObject; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string AccessorPropertyName { get { return string.Empty; } }

        //----------------------------------------------------------------------------
        public EasyBuilderComponentExtender(IEasyBuilderComponentExtendable extendedObject, EasyBuilderComponent parentComponent, string name)
        {
            this.name = name;
            this.extendedObject = extendedObject;
            this.HasCodeBehind = parentComponent.HasCodeBehind;
            CanShowInObjectModel = false;
            ParentComponent = parentComponent;

            AdjustSite();
        }
		
        //----------------------------------------------------------------------------
        protected void NotifyParentChanged ()
        {
        	AddChangedProperty(name);
	        if (ParentComponent != null)
		        ParentComponent.AddChangedProperty(name);
        }

        //----------------------------------------------------------------------------
        public virtual void AdjustSite ()
        {
	        if (ParentComponent != null && ParentComponent.Site != null)
	        {
		        Site = ((ITBSite) ParentComponent.Site).CloneChild(this, name);
		        Site.Name = name;
	        }
        }

        //----------------------------------------------------------------------------
        public virtual bool CanExtendObject(IEasyBuilderComponentExtendable e)
        {
            return false;
        }
    
    }

    //=============================================================================
	public class EasyBuilderComponentExtenders : List<IEasyBuilderComponentExtender>, IEasyBuilderComponentExtenders, IDisposable
	{
		IEasyBuilderComponentExtenderService service = null;
		//----------------------------------------------------------------------------
		public IEasyBuilderComponentExtender this[string name] { get { return GetExtension(name); } }
		public virtual IEasyBuilderComponentExtenderService Service { get { return service; } }

		//----------------------------------------------------------------------------
		public EasyBuilderComponentExtenders(IEasyBuilderComponentExtendable extendedObject)
		{
			service = new EasyBuilderComponentExtenderService(extendedObject);
		}

		//----------------------------------------------------------------------------
		private IEasyBuilderComponentExtender GetExtension(string name)
		{
			foreach (IEasyBuilderComponentExtender extender in this)
				if (name == extender.Name)
					return extender;

			return Service.CreateExtension(name);
		}

		//----------------------------------------------------------------------------
		public void Dispose()
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				this[i].Dispose();
			}

			this.Clear();
		}
	}

    //=============================================================================
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class EasyBuilderComponentExtenderAttribute : System.Attribute
    {
        private Type extenderType;
        private string extenderPropertyName;

        //-----------------------------------------------------------------------------
        public Type ExtenderType { get { return extenderType; } }
        public string ExtenderPropertyName { get { return extenderPropertyName; } }
        //-----------------------------------------------------------------------------
        public EasyBuilderComponentExtenderAttribute(Type extenderType, string extenderPropertyName)
        {
            this.extenderType = extenderType;
            this.extenderPropertyName = extenderPropertyName;
        }
    }


	//=============================================================================
	/// <summary>
	/// Classe che espone una proprietà di oggetto nidificato come se appartenesse all'oggetto ospitante
	/// </summary>
	class ExtenderPropertyDescriptor : EasyBuilderPropertyDescriptor
	{
		private PropertyDescriptor originalDescriptor;
		object extenderObject;
		//----------------------------------------------------------------------------
		public ExtenderPropertyDescriptor(PropertyDescriptor originalDescriptor, object extenderObject)
			: base(originalDescriptor)
		{

			this.originalDescriptor = originalDescriptor;
			this.extenderObject = extenderObject;
		}

		//----------------------------------------------------------------------------
		public override Type ComponentType
		{
			get { return extenderObject.GetType(); }
		}
		
        //----------------------------------------------------------------------------
		public override void SetValue(object component, object value)
		{
			originalDescriptor.SetValue(extenderObject, value);

		}
		
        //----------------------------------------------------------------------------
		public override object GetValue(object component)
		{
			return originalDescriptor.GetValue(extenderObject);
		}
		
        //----------------------------------------------------------------------------
		public override bool ShouldSerializeValue(object component)
		{
			return originalDescriptor.ShouldSerializeValue(component);
		}
		
        //----------------------------------------------------------------------------
		public override void ResetValue(object component)
		{
			originalDescriptor.ResetValue(extenderObject);
		}

		//----------------------------------------------------------------------------
		public override Type PropertyType
		{
			get { return originalDescriptor.PropertyType; }
		}
		
        //----------------------------------------------------------------------------
		public override bool IsReadOnly
		{
			get { return originalDescriptor.IsReadOnly; }
		}
		
        //----------------------------------------------------------------------------
		public override bool CanResetValue(object component)
		{
			return originalDescriptor.CanResetValue(component);
		}
	}


    //=============================================================================
    internal class EasyBuilderComponentExtenderService : IEasyBuilderComponentExtenderService
    {
        private IEasyBuilderComponentExtendable extendedObject = null;
        private IEasyBuilderComponentExtendable parent = null;
   
        public virtual IEasyBuilderComponentExtendable Parent { set { parent = value; Refresh(); } } 

        //----------------------------------------------------------------------------
		internal EasyBuilderComponentExtenderService(IEasyBuilderComponentExtendable extendedObject)
		{
			this.extendedObject = extendedObject;
			this.parent = extendedObject;
		}

        // factory di extensionsioni
        //----------------------------------------------------------------------------
        public virtual IEasyBuilderComponentExtender CreateExtension(string name)
        {
            // l'oggetto non supporta l'estensione
            Type extenderType = GetExtenderTypeByName(name);
            if (extenderType == null)
                return null;

            IEasyBuilderComponentExtender extender = Activator.CreateInstance(extenderType, extendedObject, parent, name) as IEasyBuilderComponentExtender;
            if (extender == null)
                return null;

            if (extender.CanExtendObject(extendedObject))
            {
                extendedObject.Extensions.Add(extender);
				//parent.Add(extender); TODOPERASSO: serve? non è già in extendedObject.Extensions?
            }
            else
            {
                extender.Dispose();
                extender = null;
            }

	        return extender;       
        }

        //----------------------------------------------------------------------------
        private Type GetExtenderTypeByName(string name)
        {
            if (extendedObject == null)
                return null;

            foreach (EasyBuilderComponentExtenderAttribute attr in GetImplementedExtenders(extendedObject))
                if (attr != null && attr.ExtenderPropertyName == name)
                    return attr.ExtenderType;
            
            return null;
        }

        //-----------------------------------------------------------------------------
        public virtual void Refresh()
        {
            EasyBuilderComponent parentComponent = parent as EasyBuilderComponent;
            foreach (EasyBuilderComponentExtenderAttribute extAttr in GetImplementedExtenders(parent))
            {
                IEasyBuilderComponentExtender extender = extendedObject.Extensions[extAttr.ExtenderPropertyName];

                if (extender != null)
                {
                    // l'extender potrebbe dover essere rimosso dalle proprietà
                    if (!extender.CanExtendObject(parent))
                    {
                        extendedObject.Extensions.Remove(extender);
                        continue;
                    }

                    EasyBuilderComponent extComponent = extender as EasyBuilderComponent;
                    if (extComponent != null && parentComponent != null)
                    {
                        extComponent.ParentComponent = parentComponent;
                        extComponent.HasCodeBehind = parentComponent.HasCodeBehind;
                    }

                    extender.AdjustSite();
                }
            }
        }

        //----------------------------------------------------------------------------
        virtual public void AdjustSites()
        {
            foreach (IEasyBuilderComponentExtender extender in extendedObject.Extensions)
            {
                if (extender != null && extender.Site == null)
                    extender.AdjustSite();
            }
        }
        
        //----------------------------------------------------------------------------
        private IList<EasyBuilderComponentExtenderAttribute> GetImplementedExtenders(IEasyBuilderComponentExtendable extendable)
        {
            IList<EasyBuilderComponentExtenderAttribute> coll = new List<EasyBuilderComponentExtenderAttribute>();
            System.Attribute[] attribs = System.Attribute.GetCustomAttributes(extendable.GetType());
			foreach (System.Attribute attr in attribs)
            {
                EasyBuilderComponentExtenderAttribute extAttrib = attr as EasyBuilderComponentExtenderAttribute;
                if (extAttrib != null)
                    coll.Add(extAttrib);
            }

            return coll;
        }
    }

    //=========================================================================
    public class EasyBuilderExtenderEventDescriptor : EasyBuilderEventDescriptor
    {
        private string extenderPropertyName;

        //----------------------------------------------------------------------------
        public EasyBuilderExtenderEventDescriptor(EventDescriptor childEvnt, string extenderPropertyName)
            : base(childEvnt)
        {
            this.extenderPropertyName = extenderPropertyName;
        }

        //----------------------------------------------------------------------------
        public override string GenerateEventName(IComponent component)
        {
            string path = ReflectionUtils.GetComponentFullPath(component);
            path = path.Replace(".", "_");
            return String.Concat(path, "_", Name);
        }

        //-----------------------------------------------------------------------------
        override public IComponent GetComponent(IComponent component) 
        { 
            IEasyBuilderComponentExtendable extendable = component as IEasyBuilderComponentExtendable;
            if (extendable != null)
                return extendable.Extensions[extenderPropertyName] as IComponent;

            return base.GetComponent(component);
        }
    }
}


