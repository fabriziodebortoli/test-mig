using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//================================================================================
	public class ExpandiblePropertyItem : IDisposable
	{
		//--------------------------------------------------------------------------------
		private ExpandablePropertiesTypeDescriptionProvider provider = null;

		//--------------------------------------------------------------------------------
		protected ExpandiblePropertyItem()
		{
			provider = new ExpandablePropertiesTypeDescriptionProvider(GetType());
			TypeDescriptor.AddProvider(provider, this);
		}

		//--------------------------------------------------------------------------------
		~ExpandiblePropertyItem()
		{
			Dispose(false);
		}

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//--------------------------------------------------------------------------------
		protected virtual void Dispose(bool bDisposing)
		{
			if (provider != null)
				TypeDescriptor.RemoveProvider(provider, this);
			provider = null;
		}
	}
	
	//================================================================================
	public class DefaultPropertyDescriptor : PropertyDescriptor
	{
		//--------------------------------------------------------------------------------
		private PropertyInfo property;

		//--------------------------------------------------------------------------------
		public PropertyInfo Property { get { return property; } }

		//--------------------------------------------------------------------------------
		public override bool IsReadOnly { get { return this.Property.GetSetMethod() == null; } }

		//--------------------------------------------------------------------------------
		public override Type ComponentType { get { return this.property.DeclaringType; } }

		//--------------------------------------------------------------------------------
		public override Type PropertyType { get { return this.property.PropertyType; } }

		//--------------------------------------------------------------------------------
		public DefaultPropertyDescriptor(PropertyInfo property)
			: base(property.Name, (Attribute[]) property.GetCustomAttributes(typeof(Attribute), true))
		{
			this.property = property;
		}

		//--------------------------------------------------------------------------------
		public override void ResetValue(object component) 
		{ 
		}
		
		//--------------------------------------------------------------------------------
		public override bool CanResetValue(object component) 
		{ 
			return false; 
		}

		//--------------------------------------------------------------------------------
		public override bool ShouldSerializeValue(object component)
		{ 
			return true; 
		}

		//--------------------------------------------------------------------------------
		public override object GetValue(object component)
		{
			return property.GetValue(component, null);
		}

		//--------------------------------------------------------------------------------
		public override void SetValue(object component, object value)
		{
			property.SetValue(component, value, null);
			OnValueChanged(component, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return property.GetHashCode(); 
		}

		//--------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			DefaultPropertyDescriptor other = obj as DefaultPropertyDescriptor;
			return other != null && other.property.Equals(property);
		}
	}

	//================================================================================
	internal class ExpandablePropertiesTypeDescriptionProvider : TypeDescriptionProvider
	{
		//--------------------------------------------------------------------------------
		private TypeDescriptionProvider baseProvider;
		private PropertyDescriptorCollection propertyCache;
		private FilterCache filterCache;

		//--------------------------------------------------------------------------------
		public ExpandablePropertiesTypeDescriptionProvider(Type t)
		{
			baseProvider = TypeDescriptor.GetProvider(t);
		}

		//--------------------------------------------------------------------------------
		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
		{
			return new ExpandablePropertiesTypeDescriptor(this, baseProvider.GetTypeDescriptor(objectType, instance), objectType);
		}

		//--------------------------------------------------------------------------------
		[Conditional("DEBUG")]
		private static void DumpAttributes(PropertyInfo p, Attribute[] a)
		{
			string accessMods = string.Format("\nACCESS: {0}", p.PropertyType.IsPublic ? "public" : "private");
			Debug.WriteLine("DUMPING ATTRIBUTES for: " + p.Name + accessMods);
			foreach (Attribute aa in a)
				Debug.WriteLine(aa);
		}


		//================================================================================
		private class FilterCache
		{
			//--------------------------------------------------------------------------------
			public Attribute[] Attributes;
			public PropertyDescriptorCollection FilteredProperties;

			//--------------------------------------------------------------------------------
			public bool IsValid(Attribute[] other)
			{
				if (other == null || Attributes == null) return false;
				if (Attributes.Length != other.Length) return false;
				for (int i = 0; i < other.Length; i++)
				{
					if (!Attributes[i].Match(other[i])) return false;
				}
				return true;
			}
		}
	
		//================================================================================
		private class ExpandablePropertiesTypeDescriptor : CustomTypeDescriptor
		{
			//--------------------------------------------------------------------------------
			private Type objectType;
			private ExpandablePropertiesTypeDescriptionProvider provider;

			//--------------------------------------------------------------------------------
			public ExpandablePropertiesTypeDescriptor(ExpandablePropertiesTypeDescriptionProvider provider, ICustomTypeDescriptor descriptor, Type objectType)
				: base(descriptor)
			{
				if (provider == null) throw new ArgumentNullException("provider");
				if (descriptor == null) throw new ArgumentNullException("descriptor");
				if (objectType == null) throw new ArgumentNullException("objectType");
				
				this.objectType = objectType;
				this.provider = provider;
			}

			//--------------------------------------------------------------------------------
			public override PropertyDescriptorCollection GetProperties()
			{
				return GetProperties(null);
			}

			//--------------------------------------------------------------------------------
			public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
			{
				// Retrieve cached properties and filtered properties
				bool filtering = attributes != null && attributes.Length > 0;
				FilterCache cache = provider.filterCache;
				PropertyDescriptorCollection props = provider.propertyCache;

				// Use a cached version if we can
				if (filtering && cache != null && cache.IsValid(attributes))
					return cache.FilteredProperties;
				else if (!filtering && props != null)
					return props;

				// Otherwise, create the property collection
				props = new PropertyDescriptorCollection(null);
				foreach (PropertyInfo p in objectType.GetProperties())
				{
					//	FieldInfo[] pflds = p.PropertyType.GetFields();
					PropertyInfo[] pprops = p.PropertyType.GetProperties();
					PropertyDescriptor desc = null;

					//// if the property in not an array and has public fields or properties - use ExpandablePropertyDescriptor
					if (!p.PropertyType.HasElementType && ((pprops.Length > 0)))
						desc = new ExpandabPropertyDescriptor(p);
					else
						desc = new DefaultPropertyDescriptor(p);

					if (!filtering || desc.Attributes.Contains(attributes))
						props.Add(desc);
				}

				// Store the updated properties
				if (filtering)
				{
					cache = new FilterCache();
					cache.FilteredProperties = props;
					cache.Attributes = attributes;
					provider.filterCache = cache;
				}
				else provider.propertyCache = props;

				// Return the computed properties
				return props;
			}

			//--------------------------------------------------------------------------------
			public override EventDescriptorCollection GetEvents()
			{
				return this.GetEvents(null);
			}

			//--------------------------------------------------------------------------------
			public override EventDescriptorCollection GetEvents(Attribute[] attributes)
			{
				return base.GetEvents(attributes);
			}
		}
	}

	//================================================================================
	public class ExpandabPropertyDescriptor : DefaultPropertyDescriptor
	{
		//--------------------------------------------------------------------------------
		private TypeConverter converter = null;

		//--------------------------------------------------------------------------------
		public ExpandabPropertyDescriptor(PropertyInfo prop)
			: base(prop)
		{
		}

		//--------------------------------------------------------------------------------
		public override TypeConverter Converter
		{
			get
			{
				if (converter == null)
					if (base.Converter.GetType() != typeof(TypeConverter))
					{
						// if there is a custom-converter already - return it
						converter = base.Converter;
					}
					else // force an expandable-converter for this property
						converter = new ExpandableObjectConverter();

				return converter;
			}
		}
	}
}
