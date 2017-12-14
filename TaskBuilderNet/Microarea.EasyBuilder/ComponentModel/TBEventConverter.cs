using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;

namespace Microarea.EasyBuilder.ComponentModel
{
	//=========================================================================
	internal class TBEventConverter : TypeConverter
	{
		private EventDescriptor eventDesc;

		//--------------------------------------------------------------------------------
		internal TBEventConverter(EventDescriptor eventDesc)
		{
			this.eventDesc = eventDesc;
		}

		//--------------------------------------------------------------------------------
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		//--------------------------------------------------------------------------------
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		//--------------------------------------------------------------------------------
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value != null)
			{
				string temp = value as string;
				if (temp == null)
					return base.ConvertFrom(context, culture, value);
			}
			return value;
		}

		//--------------------------------------------------------------------------------
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			if (value != null)
				return value;

			return String.Empty;
		}

		//--------------------------------------------------------------------------------
		public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			string[] values = null;
			if (context != null)
			{
				IEventBindingService service = (IEventBindingService)context.GetService(typeof(IEventBindingService));
				if (service != null)
				{
					ICollection compatibleMethods = service.GetCompatibleMethods(this.eventDesc);
					values = new string[compatibleMethods.Count];
					int num = 0;
					foreach (string str in compatibleMethods)
						values[num++] = str;
				}
			}
			return new TypeConverter.StandardValuesCollection(values);
		}

		//--------------------------------------------------------------------------------
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		//--------------------------------------------------------------------------------
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
