using System;
using System.Collections.Generic;
using TBMobileUI;
using Xamarin.Forms;
using TBMobile;

namespace TBMobileUI
{
	public partial class EditRow : ContentPage
	{
		MSqlRecord _model;
		private bool _isNew;

		public MSqlRecord Model {
			get { return _model; }
			set {
				_model = value;
				AddEntries ();
			}
		}

		public EditRow (MSqlRecord model, bool isNew = false)
		{
			InitializeComponent ();
			_isNew = isNew;
			Model = model;
		}

		private void AddEntries() 
		{
			foreach (MSqlRecordItem field in Model.Fields) {
				StackLayout stack = new StackLayout ();
				Label l = new Label ();
				l.Text = field.PropertyName;
				l.Style = (Style)Application.Current.Resources ["LabelStyle"];
				Entry e = new Entry ();
				if (_isNew) {
					e.WidthRequest = 250;
				}
				BindingManager.BindField (e, field.DataObj);
				e.Style = (Style)Application.Current.Resources ["EntryStyle"];
				stack.Children.Add (l);
				stack.Children.Add (e);
				stack.Orientation = StackOrientation.Horizontal;
				stack.Padding = new Thickness (2, 5, 2, 5);
				container.Children.Add (stack);
			}
		}

		private void OnBackClicked(object sender, EventArgs args)
		{
			Device.BeginInvokeOnMainThread (delegate {
				Navigation.PopModalAsync (true);
			});
		}
	}
}

