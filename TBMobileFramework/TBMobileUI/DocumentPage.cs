using System;
using System.Collections.Generic;
using Xamarin.Forms;
using TBMobile;
using TBMobileUI;

namespace TBMobileUI
{
	public partial class DocumentPage : ContentPage
	{
		protected MDocument _document;

		public bool insertNew { get; set; }

		public MDocument Document { 
			get
			{ return _document; } 
			set
			{ 
				if (_document != value) {
					if (_document != null) {
						_document.MessagesAvailable -= DisplayError;
						_document.DataLoaded -= OnDataLoaded;
					}
					_document = value;
					Document.DataLoaded += (sender, e) => {
						Device.BeginInvokeOnMainThread(() => indicator.IsRunning = false);
					};
					if (_document != null) {
						_document.MessagesAvailable += DisplayError;
						_document.DataLoaded += OnDataLoaded;
					}
				} 
			} 
		}

//		protected override void OnAppearing ()
//		{
//			base.OnAppearing ();
//			ManageButtons ();
//		}

		private void OnDataLoaded(object sender, EventArgs args)
		{
			Device.BeginInvokeOnMainThread (() => ManageButtons ());
		}

		private void DisplayError(object sender, EventArgs args)
		{
			Device.BeginInvokeOnMainThread (delegate {
				DisplayAlert ("Error", ((DiagnosticEventArgs)args).Messages.ToArray()[0].Message, "Ok");
			});
		}

		public void ManageButtons ()
		{
			Back.IsVisible = Document.CanDoBack ();
			Save.IsVisible = Document.CanDoSaveRecord();
			Edit.IsVisible = Document.CanDoEditRecord();
			Delete.IsVisible = Document.CanDoDeleteRecord();
			Cancel.IsVisible = Document.CanDoEscape();
		}

		public void ManageGesture (bool enable) {
			if (enable) {
				Frame.SwipeLeft += MoveNext;
				Frame.SwipeRight += MovePrev;
				Frame.SwipeTop += MoveFirst;
				Frame.SwipeDown += MoveLast;
			} else {
				Frame.SwipeLeft -= MoveNext;
				Frame.SwipeRight -= MovePrev;
				Frame.SwipeTop -= MoveFirst;
				Frame.SwipeDown -= MoveLast;
			}
		}

		private void MoveNext(object sender, EventArgs args)
		{
			if (Document.CanDoNextRecord ()) {
				indicator.IsRunning = true;
				Document.MoveNext ();
			}
		}
		private void MovePrev(object sender, EventArgs args)
		{
			if (Document.CanDoPrevRecord ()) {
				indicator.IsRunning = true;
				Document.MovePrev ();
			}
		}
		private void MoveLast(object sender, EventArgs args)
		{
			if (Document.CanDoLastRecord ()) {
				indicator.IsRunning = true;
				Document.MoveLast ();
			}
		}
		private void MoveFirst(object sender, EventArgs args)
		{
			if (Document.CanDoFirstRecord ()) {
				indicator.IsRunning = true;
				Document.MoveFirst ();
			}
		}

		public DocumentPage ()
		{
			InitializeComponent ();
			ManageGesture (true);
		}
		 
		public StackLayout Container
		{
			get {return container;} 
		}

		public GestureFrame Frame
		{
			get { return frame; }
		}

		private void OnBackClicked(object sender, EventArgs args) {
			if (Document.CanCloseDocument ()) {
				Document.Close ();
				Device.BeginInvokeOnMainThread (() => {
					Navigation.PopModalAsync (true);
				});
			}
		}

		private void OnSaveClicked(object sender, EventArgs args) {
			if (Document.CanDoSaveRecord ()) {
				Document.SaveCurrentRecord ();
				ManageGesture (true);
				ManageButtons ();
			}
		}

		private void OnEditClicked(object sender, EventArgs args) {
			if (Document.CanDoEditRecord ()) {
				Document.EditCurrentRecord ();
				ManageGesture (false);
				ManageButtons ();
			}
		}

		private async void OnDeleteClicked(object sender, EventArgs args) {
			if (Document.CanDoDeleteRecord ()) {
				var action = await DisplayActionSheet ("Confirm Elimination?", "Cancel", null, "Ok");
				if (action == "Ok")
					Document.DeleteCurrentRecord ();
			}
		}

		private void OnCancelClicked(object sender, EventArgs args) {
			if (insertNew) {
				Navigation.PopModalAsync (true);
			}
			Document.FormMode = MDocument.FormModeType.Browse;
			ManageGesture (true);
			ManageButtons ();
		}
	}
}

