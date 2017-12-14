using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
    [AttributeUsage(AttributeTargets.Event)]
    public class TBEventFilterAttribute : Attribute
    {
    }

	//================================================================================
	public class EasyBuilderPropertyDescriptor : PropertyDescriptor
	{
		private PropertyDescriptor propertyDescriptor;
		private bool isReadOnlyForContext;
        private string customName;
		private string customDisplayName;
		private bool bitIPropChangedQueried;
		private EventDescriptor realIPropChangedEvent;

        //--------------------------------------------------------------------------------
        public override string Name
        {
            get
            {
                return customName == null ?  base.Name : customName;
            }
        }
		//--------------------------------------------------------------------------------
        public override string DisplayName
		{
			get
			{
				return customDisplayName == null ?  base.DisplayName : customDisplayName;
			}
		}
		public override string Description
		{
			get
			{
				string s = base.Description;
				if (!string.IsNullOrEmpty(s))
					return s;
				return DescriptionProvider.GetPropertyDescription(ComponentType, Name);
			}
		}
        //--------------------------------------------------------------------------------
        public bool IsEasyBuilderComponentExtenderProperty
        {
            get
            {
                return propertyDescriptor.PropertyType.GetInterface(typeof(IEasyBuilderComponentExtender).FullName) != null;
            }
        }

		//--------------------------------------------------------------------------------
		private EventDescriptor IPropChangedEventValue
		{
			get
			{
				if (!this.bitIPropChangedQueried)
				{
					this.bitIPropChangedQueried = true;
					if (typeof(INotifyPropertyChanged).IsAssignableFrom(this.ComponentType))
					{
						this.realIPropChangedEvent = TypeDescriptor.GetEvents(typeof(INotifyPropertyChanged))["PropertyChanged"];
					}
				}
				return this.realIPropChangedEvent;
			}
		}

		//--------------------------------------------------------------------------------
		public EasyBuilderPropertyDescriptor(PropertyDescriptor descriptor)
			: base(descriptor)
		{
			this.propertyDescriptor = descriptor;
			customName = descriptor.Name;
			customDisplayName = descriptor.DisplayName;
		}

        //--------------------------------------------------------------------------------
        internal EasyBuilderPropertyDescriptor(string name, PropertyDescriptor descriptor)
            : 
            base(descriptor)
        {
            this.customName = name;
			this.customDisplayName = name;
            this.propertyDescriptor = descriptor;
        }
		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}
		public override bool CanResetValue(object component)
		{
			return propertyDescriptor.CanResetValue(component);
		}

		//--------------------------------------------------------------------------------
		public PropertyDescriptor OrignalDescriptor
		{
			get { return propertyDescriptor; }
		}

		//--------------------------------------------------------------------------------
		public override Type ComponentType
		{
			get { return propertyDescriptor.ComponentType; }
		}

		//--------------------------------------------------------------------------------
		public override object GetValue(object component)
		{
			return propertyDescriptor.GetValue(component);
		}

		//--------------------------------------------------------------------------------
		public bool IsReadOnlyForContext
		{
			get { return isReadOnlyForContext; }
			set { isReadOnlyForContext = value; }
		}

		//--------------------------------------------------------------------------------
		public override bool IsReadOnly
		{
			get { return propertyDescriptor.IsReadOnly || isReadOnlyForContext; }
		}

		//--------------------------------------------------------------------------------
		public override Type PropertyType
		{
			get { return propertyDescriptor.PropertyType; }
		}

		//--------------------------------------------------------------------------------
		public override void ResetValue(object component)
		{
			propertyDescriptor.ResetValue(component);
		}

		//--------------------------------------------------------------------------------
		public override void SetValue(object component, object value)
		{
			//Se il nome esiste già, disabilito la possibilità di cambiare il nome
			object newValue = ControlNameAlreadyExist(component, value);
			EasyBuilderComponent comp = component as EasyBuilderComponent;
			
			//metto un diagnostico locale per intercettare eventuali errori
			if (comp != null)
				comp.DiagnosticSession.StartSession(comp.Name);
			
			//imposto il valore (qui potrebbero essere segnalati errori)
			propertyDescriptor.SetValue(component, newValue);
			
			if (comp != null)
			{
				//ripristino il diagnostic originario e se ci sono errori in quello temporaneo lancio l'eccezione
				Diagnostic tempDiagnostic = comp.DiagnosticSession.CurrentDiagnostic;
				comp.DiagnosticSession.EndSession();
				if (tempDiagnostic.Error)
					throw new ApplicationException(tempDiagnostic.ToString());
			}
		}

		/// <summary>
		/// Nel caso la property cambiata sia "Name" viene verificato se il nuovo nome
		/// non sia già stato usato da altri control nella stessa view.
		/// Ritorna il nome vecchio se il nuovo è già usato
		/// </summary>
		/// <param name="component"></param>
		/// <param name="value"></param>
		//--------------------------------------------------------------------------------
		private object ControlNameAlreadyExist(object component, object value)
		{
			EasyBuilderComponent comp = component as EasyBuilderComponent;
			if (comp == null || comp.Site == null)
				return value;

			//e che abbia un container non null di tipo IWindowWrapperContainer
			IContainer container = comp.Site.Container as IContainer;
			if (container == null)
				return value;

			//Barbatrucco: per ottener il nome della property "ControlName" senza utilizzare una 
			//stringa cablata utilizzo la GetPropertyName. Necessita di un'istanza di una classe
			//che contenga la property che ci interessa e poi viene passata alla GetPropertyName
			string propertyName = ReflectionUtils.GetPropertyName(() => comp.Name);
			if (string.Compare(propertyDescriptor.Name, propertyName, true) != 0)
				return value;

			string oldValue = comp.Name;
			if (!comp.IsValidName(value.ToString()))
				throw new ApplicationException(EasyBuilderStrings.InvalidName);

			//verifico che l'oggetto passato sia un component...
			IEasyBuilderContainer wrapperContainer = container as IEasyBuilderContainer;
			if (wrapperContainer.HasComponent(value.ToString()))
				throw new ApplicationException(EasyBuilderStrings.UnableToRenameControl);
			
			return value;
		}

		//--------------------------------------------------------------------------------
		public override bool ShouldSerializeValue(object component)
		{
			return propertyDescriptor.ShouldSerializeValue(component);
		}

		//--------------------------------------------------------------------------------
		public override void AddValueChanged(object component, EventHandler handler)
		{
			if (component == null)
			{
				throw new ArgumentNullException("component");
			}
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			
			if (base.GetValueChangedHandler(component) == null)
			{
				EventDescriptor iPropChangedEventValue = this.IPropChangedEventValue;
				if (iPropChangedEventValue != null)
				{
					iPropChangedEventValue.AddEventHandler(component, new PropertyChangedEventHandler(this.OnINotifyPropertyChanged));
				}
			}
			base.AddValueChanged(component, handler);
		}

		//--------------------------------------------------------------------------------
		public override void RemoveValueChanged(object component, EventHandler handler)
		{
			if (component == null)
			{
				throw new ArgumentNullException("component");
			}
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}

			base.RemoveValueChanged(component, handler);
			if (base.GetValueChangedHandler(component) == null)
			{
				EventDescriptor iPropChangedEventValue = this.IPropChangedEventValue;
				if (iPropChangedEventValue != null)
				{
					iPropChangedEventValue.RemoveEventHandler(component, new PropertyChangedEventHandler(this.OnINotifyPropertyChanged));
				}
			}
		}

		//--------------------------------------------------------------------------------
		internal void OnINotifyPropertyChanged(object component, PropertyChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.PropertyName) || (string.Compare(e.PropertyName, this.Name, true, CultureInfo.InvariantCulture) == 0))
			{
				this.OnValueChanged(component, e);
			}
		}
	}

	public class DataObjPropertyDescriptor<TRecord> : PropertyDescriptor
	{
		PropertyInfo pi;
		private Type propType;
		public DataObjPropertyDescriptor(PropertyInfo pi, Type propType)
			: base(pi.Name, null)
		{
			this.pi = pi;
			this.propType = propType;
		}

		public override bool IsReadOnly { get { return false; } }
		public override void ResetValue(object component) { }
		public override bool CanResetValue(object component) { return false; }
		public override bool ShouldSerializeValue(object component)
		{
			return true;
		}

		public override Type ComponentType
		{
			get { return typeof(TRecord); }
		}
		public override Type PropertyType { get { return propType; } }

		public override object GetValue(object component)
		{
			IRecord rec = (IRecord)component;
			IDataObj dataObj = pi.GetValue(rec, null) as IDataObj;
			return dataObj == null ? null : dataObj.Value;
		}

		public override void SetValue(object component, object value)
		{
			IRecord rec = (IRecord)component;
			IDataObj dataObj = pi.GetValue(rec, null) as IDataObj;

			if (dataObj != null)
				dataObj.Value = value;

		}
	}

	public class RecordFieldPropertyDescriptor<TRecord> : PropertyDescriptor 
	{
		private Type propType;
		
		public RecordFieldPropertyDescriptor(string fieldName, Type propType)
			: base(fieldName, null)
		{
			this.propType = propType;
		}

		public override bool IsReadOnly { get { return false; } }
		public override void ResetValue(object component) { }
		public override bool CanResetValue(object component) { return false; }
		public override bool ShouldSerializeValue(object component)
		{
			return true;
		}

		public override Type ComponentType
		{
			get { return typeof(TRecord); }
		}
		public override Type PropertyType { get { return propType; } }

		public override object GetValue(object component)
		{
			IRecord rec = (IRecord)component;
			IRecordField field = rec.GetField(Name);
			return field == null ? null : field.DataObj.Value;
		}

		public override void SetValue(object component, object value)
		{
			IRecord rec = (IRecord)component;
			IRecordField field = rec.GetField(Name);
			if (field != null)
				field.DataObj.Value = value;

		}
	}

	public class MyCustomTypeDescriptor<T> : CustomTypeDescriptor
	{
		public MyCustomTypeDescriptor(ICustomTypeDescriptor parent)
			: base(parent)
		{
		}

		public override PropertyDescriptorCollection GetProperties()
		{
			List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
			IRecord record = null;
			if (typeof(IRecord).IsAssignableFrom(typeof(T)))
			{
				record = (IRecord)Activator.CreateInstance<T>();
				//aggiungo le proprietà fittizie del SqlRecord corrispondenti ai suoi campi
				for (int i = 0; i < record.Fields.Count; i++)
				{
					IRecordField field = (IRecordField)record.Fields[i];
					properties.Add(new RecordFieldPropertyDescriptor<T>(field.Name, field.DataObj.Value.GetType()));
				}
			}
			foreach (PropertyInfo pi in typeof(T).GetProperties())
			{
				if (typeof(IDataObj).IsAssignableFrom(pi.PropertyType))
				{
					PropertyInfo piValue = pi.PropertyType.GetProperty("Value", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
					properties.Add(new DataObjPropertyDescriptor<T>(pi, piValue.PropertyType));
				}
			}
			return new PropertyDescriptorCollection(properties.ToArray());
		}
	}
	public class EBTypeDescriptionProvider<T> : TypeDescriptionProvider
	{
		private ICustomTypeDescriptor td;

		public EBTypeDescriptionProvider()
			: this(TypeDescriptor.GetProvider(typeof(T)))
		{
		}
		public EBTypeDescriptionProvider(TypeDescriptionProvider parent)
			: base(parent)
		{
		}
		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
		{
			if (td == null)
			{
				td = base.GetTypeDescriptor(objectType, instance);
				td = new MyCustomTypeDescriptor<T>(td);
			}
			return td;
		}
	}
}
