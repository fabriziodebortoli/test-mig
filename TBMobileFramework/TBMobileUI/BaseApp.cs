using System;
using Xamarin.Forms;
using TBMobile;
using System.Diagnostics;
using TBMobileUI;

namespace TBMobileUI
{
	public abstract class BaseApp : Application
	{
		LoginContext context = new LoginContext ();
		BaseController controller;

		public BaseController Controller {
			get { return controller; }
		}
		public LoginContext Context {
			get { return context; }
		}
		public BaseApp ()
		{	
			Application.Current.Resources = new ResourceDictionary ();
			ConnectionSettings.Server = "test.microarea.eu";
			ConnectionSettings.Installation = "developmentTBWEB";
			ConnectionSettings.User = "pippo";
			ConnectionSettings.Company = "MagoNet";
			ConnectionSettings.Password = "pippo";
			controller = CreateController ();
			Style BaseButtonStyle = new Style (typeof(Button)) {
				Setters = {
					new Setter { Property = Button.BorderWidthProperty, Value = 1 },
					new Setter { Property = Button.BorderColorProperty, Value = Color.Black },
					new Setter { Property = Button.TextColorProperty, Value = Color.Black },
					new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromRgb (188, 228, 245) } 
				}
			};
			Style BasePageStyle = new Style (typeof(Page)) {
				Setters = {
					new Setter { Property = Page.BackgroundColorProperty, Value = Color.FromRgb (230, 237, 227) }
				}
			};
			Style BaseLabelStyle = new Style (typeof(Label)) {
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Color.Black }
				}
			};
			Style BaseLabelBodyEditStyle = new Style (typeof(Label)) {
				Setters = {
					new Setter { Property = Label.FontSizeProperty, Value = 18 },
					new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold },
					new Setter { Property = Label.TextColorProperty, Value = Color.FromRgb (99, 141, 176) }
				}
			};
			Style BaseEntryStyle = new Style (typeof(Entry)) {
				Setters = {
					new Setter { Property = Entry.TextColorProperty, Value = Color.Black },
					new Setter { Property = Entry.BackgroundColorProperty, Value = Color.FromRgb (242, 242, 242) }
				}
			};
			Style BaseBodyEditStackLayoutStyle = new Style (typeof(StackLayout)) {
				Setters = {
					new Setter { Property = StackLayout.BackgroundColorProperty, Value = Color.FromRgb (231, 234, 230) }
				}
			};
			Style BaseFrameStyle = new Style (typeof(Frame)) {
				Setters = {
					new Setter { Property = Frame.OutlineColorProperty, Value = Color.Black }
				}
			};
			Application.Current.Resources.Add ("ButtonStyle", BaseButtonStyle);
			Application.Current.Resources.Add ("PageStyle", BasePageStyle);
			Application.Current.Resources.Add ("LabelStyle", BaseLabelStyle);
			Application.Current.Resources.Add ("EntryStyle", BaseEntryStyle);
			Application.Current.Resources.Add ("BodyEditStackLayoutStyle", BaseBodyEditStackLayoutStyle);
			Application.Current.Resources.Add ("BodyEditLabelStyle", BaseLabelBodyEditStyle);
			Application.Current.Resources.Add ("FrameStyle", BaseFrameStyle);
		}

		protected abstract BaseController CreateController(); 

		protected override void OnStart()
		{
		}

		protected override void OnSleep()
		{
		}

		protected override void OnResume()
		{
		}
	}
}

