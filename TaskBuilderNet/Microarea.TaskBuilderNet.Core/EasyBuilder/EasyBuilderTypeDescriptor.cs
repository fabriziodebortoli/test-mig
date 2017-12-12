using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//================================================================================
	[Serializable]
	public class EasyBuilderTypeDescriptor : ICustomTypeDescriptor
	{
		protected bool disposed = false;

		[NonSerialized]
        private EasyBuilderPropertiesManager propertiesManager;

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore, ExcludeFromIntellisense]
        public EasyBuilderPropertiesManager PropertiesManager { get { if (propertiesManager == null) propertiesManager = new EasyBuilderPropertiesManager(this); return propertiesManager; } }

		//--------------------------------------------------------------------------------
		public EasyBuilderTypeDescriptor()
		{
		}

		//-----------------------------------------------------------------------------
		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//-----------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (this.disposed)
				return;

			disposed = true;
		}

		//--------------------------------------------------------------------------------
		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(GetType()); 
		}

		//-----------------------------------------------------------------------------
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore]
		public bool IsDisposed { get { return disposed; } }

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public string GetClassName()
		{
			return GetType().ToString(); 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual string GetComponentName()
		{
			return null; 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(GetType()); 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public EventDescriptor GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(GetType()); 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public PropertyDescriptor GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(GetType()); 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public object GetEditor()
		{
			return TypeDescriptor.GetEditor(this, GetType()); 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public object GetEditor(Type t)
		{ 
			return TypeDescriptor.GetEditor(GetType(), t); 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public object GetPropertyOwner(PropertyDescriptor t) 
		{
            EasyBuilderPropertyDescriptor ebDescriptor = t as EasyBuilderPropertyDescriptor;
            if (ebDescriptor != null && ebDescriptor.IsEasyBuilderComponentExtenderProperty)
            {
                IEasyBuilderComponentExtendable extendable = this as IEasyBuilderComponentExtendable;
                if (extendable != null)
                    return extendable.Extensions[t.Name] as object;
            }
        			
            return this; 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual PropertyDescriptorCollection GetProperties()
		{
			return GetProperties(null); 
		}

        [ExcludeFromIntellisense]
        //-----------------------------------------------------------------------------
        public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return PropertiesManager.GetProperties(attributes);
        }

        [ExcludeFromIntellisense]
        //-----------------------------------------------------------------------------
        public virtual EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return PropertiesManager.GetEvents(attributes);
        }

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual EventDescriptorCollection GetEvents()
		{
            return GetEvents(null); 
		}

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual EventDescriptor GetEventDescriptor(string name)
		{
			foreach (EventDescriptor descriptor in TypeDescriptor.GetEvents(GetType()))
				if (descriptor.Name == name)
					return descriptor;

			return null;
		}
	};

}
