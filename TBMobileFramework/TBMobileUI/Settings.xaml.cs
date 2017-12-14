using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace TBMobileUI
{
	public partial class Settings : ContentPage
	{
		public Settings ()
		{
			InitializeComponent ();
		}
			
		private Style BlackButtonStyle() {
			Style ButtonStyle = new Style (typeof(Button)) {
				Setters = {
					new Setter { Property = Button.BorderWidthProperty, Value = 1 },
					new Setter { Property = Button.BorderColorProperty, Value = Color.White },
					new Setter { Property = Button.TextColorProperty, Value = Color.White },
					new Setter { Property = Button.BackgroundColorProperty, Value = Color.Transparent } 
				}
			};
			return ButtonStyle;
		}

		private Style BlackPageStyle() {
			Style PageStyle = new Style (typeof(Page)) {
				Setters = {
					new Setter { Property = Page.BackgroundColorProperty, Value = Color.Black }
				}
			};
			return PageStyle;
		}

		private Style BlackLabelStyle() {
			Style BlackLabelStyle = new Style (typeof(Label)) {
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Color.White }
				}
			};
			return BlackLabelStyle;
		}

		private Style BlackLabelBodyEditStyle() {
			Style BlackLabelBodyEditStyle = new Style (typeof(Label)) {
				Setters = {
					new Setter { Property = Label.FontSizeProperty, Value = 18 },
					new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold },
					new Setter { Property = Label.TextColorProperty, Value = Color.White }
				}
			};
			return BlackLabelBodyEditStyle;
		}

		private Style BlackEntryStyle() {
			Style BlackEntryStyle = new Style (typeof(Entry)) {
				Setters = {
					new Setter { Property = Entry.TextColorProperty, Value = Color.White }
				}
			};
			return BlackEntryStyle;
		}

		private Style BlackBodyEditStackLayoutStyle() {
			Style BlackBodyEditStackLayoutStyle = new Style (typeof(StackLayout)) {
				Setters = {
					new Setter { Property = StackLayout.BackgroundColorProperty, Value = Color.Black }
				}
			};
			return BlackBodyEditStackLayoutStyle;
		}

		private Style BlackFrameStyle() {
			Style BlackFrameStyle = new Style (typeof(Frame)) {
				Setters = {
					new Setter { Property = Frame.OutlineColorProperty, Value = Color.White }
				}
			};
			return BlackFrameStyle;
		}

		private Style WhiteButtonStyle() {
			Style ButtonStyle = new Style (typeof(Button)) {
				Setters = {
					new Setter { Property = Button.BorderWidthProperty, Value = 1 },
					new Setter { Property = Button.BorderColorProperty, Value = Color.Black },
					new Setter { Property = Button.TextColorProperty, Value = Color.Black },
					new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromRgb(188, 228, 245) } 
				}
			};
			return ButtonStyle;
		}

		private Style WhitePageStyle() {
			Style PageStyle = new Style (typeof(Page)) {
				Setters = {
					new Setter { Property = Page.BackgroundColorProperty, Value = Color.FromRgb(230, 237, 227) }
				}
			};
			return PageStyle;
		}

		private Style WhiteLabelStyle() {
			Style WhiteLabelStyle = new Style (typeof(Label)) {
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Color.Black }
				}
			};
			return WhiteLabelStyle;
		}

		private Style WhiteLabelBodyEditStyle() {
			Style WhiteLabelBodyEditStyle = new Style (typeof(Label)) {
				Setters = {
					new Setter { Property = Label.FontSizeProperty, Value = 18 },
					new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold },
					new Setter { Property = Label.TextColorProperty, Value = Color.FromRgb (99, 141, 176) }
				}
			};
			return WhiteLabelBodyEditStyle;
		}

		private Style WhiteEntryStyle() {
			Style WhiteEntryStyle = new Style (typeof(Entry)) {
				Setters = {
					new Setter { Property = Entry.TextColorProperty, Value = Color.Black },
					new Setter { Property = Entry.BackgroundColorProperty, Value = Color.FromRgb(242, 242, 242) }
				}
			};
			return WhiteEntryStyle;
		}

		private Style WhiteBodyEditStackLayoutStyle() {
			Style WhiteBodyEditStackLayoutStyle = new Style (typeof(StackLayout)) {
				Setters = {
					new Setter { Property = StackLayout.BackgroundColorProperty, Value = Color.FromRgb (231, 234, 230) }
				}
			};
			return WhiteBodyEditStackLayoutStyle;
		}

		private Style WhiteFrameStyle() {
			Style WhiteFrameStyle = new Style (typeof(Frame)) {
				Setters = {
					new Setter { Property = Frame.OutlineColorProperty, Value = Color.Black }
				}
			};
			return WhiteFrameStyle;
		}

		private void OnBlackClicked (object sender, EventArgs args) 
		{
			Application.Current.Resources ["PageStyle"] = BlackPageStyle ();
			Application.Current.Resources ["ButtonStyle"] = BlackButtonStyle ();
			Application.Current.Resources ["LabelStyle"] = BlackLabelStyle ();
			Application.Current.Resources ["EntryStyle"] = BlackEntryStyle ();
			Application.Current.Resources ["BodyEditStackLayoutStyle"] = BlackBodyEditStackLayoutStyle ();
			Application.Current.Resources ["LabelBodyEditStyle"] = BlackLabelBodyEditStyle ();
			Application.Current.Resources ["FrameStyle"] = BlackFrameStyle ();
			Application.Current.MainPage = new LoginPage();
		}

		private void OnWhiteClicked (object sender, EventArgs args) 
		{
			Application.Current.Resources ["ButtonStyle"] = WhiteButtonStyle ();
			Application.Current.Resources ["PageStyle"] = WhitePageStyle ();
			Application.Current.Resources ["LabelStyle"] = WhiteLabelStyle ();
			Application.Current.Resources ["EntryStyle"] = WhiteEntryStyle ();
			Application.Current.Resources ["BodyEditStackLayoutStyle"] = WhiteBodyEditStackLayoutStyle ();
			Application.Current.Resources ["LabelBodyEditStyle"] = WhiteLabelBodyEditStyle ();
			Application.Current.Resources ["FrameStyle"] = WhiteFrameStyle ();
			Application.Current.MainPage = new LoginPage();
		}

		private void OnBackClicked (object sender, EventArgs args)
		{
			Device.BeginInvokeOnMainThread (delegate {
				Navigation.PopModalAsync (true);
			});
		}
	}
}

