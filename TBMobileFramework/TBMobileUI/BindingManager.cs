using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBMobile;
using Xamarin.Forms;

namespace TBMobileUI
{
	public static class BindingManager 
	{
		private static void BindFieldToVisualElement(VisualElement el, MDataObj field, bool setSource = true)
		{
			Binding b = new Binding(field.Owner.PropertyName + ".IsEnabled");
			if (setSource)
				b.Source = field.Owner.Record; 
			b.Mode = BindingMode.OneWay;
			el.SetBinding(VisualElement.IsEnabledProperty, b);
		}
		internal static void BindField(Entry el, MDataObj field, bool setSource = true)
		{
			Binding b = new Binding(field.Owner.PropertyName + ".Value");
			if (setSource)
				b.Source = field.Owner.Record;
			b.Mode = BindingMode.TwoWay;
			el.SetBinding(Entry.TextProperty, b);
			BindFieldToVisualElement(el, field);
		}
		internal static void BindField(Editor el, MDataObj field, bool setSource)
		{
			Binding b = new Binding(field.Owner.PropertyName + ".Value");
			if (setSource)
				b.Source = field.Owner.Record;
			b.Mode = BindingMode.TwoWay;
			el.SetBinding(Editor.TextProperty, b);
			BindFieldToVisualElement(el, field);
		}
		internal static void BindField(Label el, MDataObj field, bool setSource)
		{
			Binding b = new Binding(field.Owner.PropertyName + ".Value");
			if (setSource)
				b.Source = field.Owner.Record;
			b.Mode = BindingMode.TwoWay;
			el.SetBinding(Label.TextProperty, b);
		}

		internal static void BindField(VisualElement el, MDataObj field, bool setSource)
		{
			if (el is Entry) {
				el.Style = (Style)Application.Current.Resources ["EntryStyle"];
				if (field.GetType() == typeof(MDataInt)) {
					((Entry)el).Keyboard = Keyboard.Numeric;
				}
				BindField ((Entry)el, field, setSource);
			} else if (el is Editor)
				BindField ((Editor)el, field, setSource);
			else if (el is Label) {
				el.Style = (Style)Application.Current.Resources ["LabelStyle"];
				BindField ((Label)el, field, setSource);
			}
		}
		public static void BindField(VisualElement el, MDataObj field)
		{
			BindField(el, field, true);
		}
	}
}
