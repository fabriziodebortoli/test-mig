using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder.ComponentModel
{
	//================================================================================
	class TBEventPropertyDescriptor : PropertyDescriptor
	{
		private EasyBuilderEventDescriptor eventDescriptor;
		private string methodName;

		//--------------------------------------------------------------------------------
		public string MethodName
		{
			get { return methodName; }
		}

		//--------------------------------------------------------------------------------
        public TBEventPropertyDescriptor(EasyBuilderEventDescriptor descr)
			: base(descr)
		{
			this.eventDescriptor = descr;
		}

		//--------------------------------------------------------------------------------
		public override Type ComponentType { get { return this.eventDescriptor.ComponentType; } }
		public override Type PropertyType { get { return this.eventDescriptor.EventType; } }
        public EasyBuilderEventDescriptor EventDescriptor { get { return eventDescriptor; } }
		public override bool IsReadOnly
		{
			get
			{
				return this.Attributes.Count > 0 &&
						this.Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);
			}
		}

		//--------------------------------------------------------------------------------
		public override bool CanResetValue(object component)
		{
			return (this.GetValue(component) != null);
		}

		//--------------------------------------------------------------------------------
		public override object GetValue(object component)
		{
			return methodName;
		}

		//--------------------------------------------------------------------------------
		public override void ResetValue(object component)
		{
			this.SetValue(component, null);
		}

		//--------------------------------------------------------------------------------
		public override void SetValue(object component, object value)
		{
			IComponentChangeService compChangeSvc = null;
            IComponent iComponent = EventDescriptor.GetComponent(component as IComponent);
			if (iComponent != null && iComponent.Site != null)
			{
				compChangeSvc = iComponent.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
			}
			if (compChangeSvc != null)
				compChangeSvc.OnComponentChanging(component, this);

			object oldValue = methodName;

			if (Object.ReferenceEquals(oldValue, value))
				return;

			if (String.Compare(oldValue as string, value as string, StringComparison.InvariantCulture) == 0)
				return;

			methodName = value as string;
			if (methodName != null && methodName.Trim().Length == 0)
				methodName = null;

			if (compChangeSvc != null)
				compChangeSvc.OnComponentChanged(component, this, oldValue, methodName);
		}

		//--------------------------------------------------------------------------------
		public override bool ShouldSerializeValue(object component)
		{
			return this.CanResetValue(component);
		}

		//--------------------------------------------------------------------------------
		public override TypeConverter Converter
		{
			get
			{
				return new TBEventConverter(this.eventDescriptor);
			}
		}
	}
}
