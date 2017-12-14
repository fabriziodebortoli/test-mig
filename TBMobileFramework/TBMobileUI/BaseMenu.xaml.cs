using System;
using System.Collections.Generic;
using Xamarin.Forms;
using TBMobile;
using System.Diagnostics;
using TBMobileUI;

namespace TBMobileUI
{	
	public partial class BaseMenu : ContentPage
	{
		private BaseController controller = ((BaseApp)BaseApp.Current).Controller;
		/// <summary>
		/// Initializes a new instance of the <see cref="MobileApp.DocumentList"/> class.
		/// </summary>
		public BaseMenu ()
		{
			InitializeComponent ();

			documentList.ItemsSource = controller.DocumentEntries;
		}

		private void OnItemSelected (object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null) return;
			Xamarin.Forms.Device.BeginInvokeOnMainThread(delegate {
				Navigation.PushModalAsync(controller.GetListDocumentView(((DocumentEntry)e.SelectedItem).Name), true);
			});
			((ListView)sender).SelectedItem = null;
		}

		private async void OnLogoutClicked(object sender, EventArgs e)
		{
			LoginContext context = ((BaseApp)BaseApp.Current).Context;
			string action = await DisplayActionSheet ("Do you want Logout?", "Cancel", null, "Confirm");
			if (action == "Confirm") {
				context.Logoff ();
				await Navigation.PopModalAsync ();
			} 
		}
	}
}