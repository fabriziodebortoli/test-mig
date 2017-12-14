using System;
using System.Collections.Generic;
using Xamarin.Forms;
using TBMobile;
using System.Diagnostics;
using TBMobileUI;


namespace TBMobileUI
{	
	public partial class LoginPage : ContentPage
	{	
		LoginContext context = ((BaseApp)BaseApp.Current).Context;
		public LoginPage ()
		{
			InitializeComponent ();
			user.Text = ConnectionSettings.User;
			company.Text = ConnectionSettings.Company;
			password.Text = ConnectionSettings.Password;
		}
		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			loginButton.IsEnabled = true;
			context.Error += OnLoginError;
			context.LoggedIn += OnLoggedIn;
		}
		protected override void OnDisappearing ()
		{
			context.Error -= OnLoginError;
			context.LoggedIn -= OnLoggedIn;
			base.OnDisappearing ();
		}
		private void OnLoginClicked(object sender, EventArgs args)
		{
			loginButton.IsEnabled = false;
			if (string.IsNullOrEmpty(user.Text) || string.IsNullOrEmpty(password.Text))
			{
				loginButton.IsEnabled = true;
				DisplayAlert ("Error", "Error username or password", "Ok");
				return;
			}
			indicator.IsRunning = true;
			context.Login(user.Text, company.Text, password.Text, true);
		}
		private void OnLoggedIn(object sender, EventArgs args)
		{

			ConnectionSettings.User = user.Text;
			ConnectionSettings.Company = company.Text;
			ConnectionSettings.Password = password.Text;
			Device.BeginInvokeOnMainThread(delegate {
				indicator.IsRunning = false;
				Navigation.PushModalAsync (new BaseMenu(), true); //apro la nuova pagina
				loginButton.IsEnabled = true;
			});
		}
		private void OnLoginError(object sender, ErrorEventArgs args)
		{
			Device.BeginInvokeOnMainThread(delegate {  
				indicator.IsRunning = false;
				loginButton.IsEnabled = true;
				if (args.Code != 0)
					DisplayAlert("Error!", "Code error: " + args.Code, "Ok!");
				else if (!string.IsNullOrEmpty(args.ErrorMessage)) {
					DisplayAlert("Error!", args.ErrorMessage, "Ok!");
					Debug.WriteLine("Errore: " + args.ErrorMessage);
				}
			});
		}

		private void OnSettingsClicked(object sender, EventArgs args)
		{
			Device.BeginInvokeOnMainThread (delegate {
				Navigation.PushModalAsync (new Settings (), true);
			});
		}
	}
}

