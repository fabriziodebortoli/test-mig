using System;
using System.Collections.Generic;

using Xamarin.Forms;
using TBMobileUI;
using System.Diagnostics;
using TBMobile;

namespace TBMobileUI
{
	public partial class DocumentListView : ContentPage
	{
		public string DocumentName { get; set; }

		private BaseController controller = ((BaseApp)BaseApp.Current).Controller;

		public DocumentListView ()
		{
			InitializeComponent ();
			Body.OnItemSelected += OnItemSelected;
		}

		public Grid Container {
			get { return container; }
		}

		public UIBodyEdit Body {
			get { return body; }
		}

		private void OnBackClicked(object sender, EventArgs args) {
			Navigation.PopModalAsync (true);
		}
		 
		private void OnNewClicked(object sender, EventArgs args) {
			Device.BeginInvokeOnMainThread (delegate {
				Navigation.PushModalAsync(controller.GetSelectedDocumentView(DocumentName, null), true);
			});
		} 

		private void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
			if (e.SelectedItem == null) return;
			Device.BeginInvokeOnMainThread(delegate {
				Navigation.PushModalAsync(controller.GetSelectedDocumentView(DocumentName, (MSqlRecord)e.SelectedItem), true);
			});
		}
	}
}

