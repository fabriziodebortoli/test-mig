using System;
using System.Collections.Generic;
using Xamarin.Forms;
using TBMobile;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;

namespace TBMobileUI
{
	public partial class UIBodyEdit : ContentView
	{
		private List<MColumnInfo> columns = new List<MColumnInfo>();

		private List<CommandInfo> commands = new List<CommandInfo> ();
		public event EventHandler<SelectedItemChangedEventArgs> OnItemSelected;

		protected Button AddRowButton {
			get { return AddRow; }
		}

		public List<MColumnInfo> Columns
		{
			get { return columns; }
		}

		public List<CommandInfo> Commands 
		{
			get { return commands; }
		}

		public IEnumerable ItemsSource { get { return bodyContent.ItemsSource; } set { bodyContent.ItemsSource = value; } }

		public UIBodyEdit()
		{
			InitializeComponent ();
			bodyContent.Columns = columns;
			bodyContent.Commands = commands;
			AddRowButton.IsVisible = false;
			bodyContent.ItemSelected += (object sender, SelectedItemChangedEventArgs e) => {
				if (OnItemSelected != null) 
					OnItemSelected (this, e);
				bodyContent.SelectedItem = null;
			};
		}

		public CommandInfo AddCommand(string text, EventHandler behavior, object owner, string enabledProperty = null)
		{
			CommandInfo ci = new CommandInfo {
				Text = text,
				Behavior = behavior,
				Owner = owner,
				EnabledProperty = enabledProperty
			};
			commands.Add (ci);
			return ci;
		}

		public MColumnInfo AddColumn<T>(string name, string title, MDataObj dataObj) where T : View
		{
			MColumnInfo ci = new MColumnInfo
			{
				ViewType = typeof(T),
				Title = title,
				Name = name,
				DataObj = dataObj
			};
			columns.Add(ci);

			Label v = new Label ();
			v.Text = ci.Title;
			v.Style = (Style)Application.Current.Resources ["BodyEditLabelStyle"];

			header.Children.Add(v);
			return ci;
		}
	}

	public class UIBodyEditContent : ListView
	{
		public List<MColumnInfo> Columns { get; set; }
		public List<CommandInfo> Commands { get; set; }

		public UIBodyEditContent()
		{
			ItemTemplate = new DataTemplate(typeof(BodyEditCell));
		}
	}

	public class MColumnInfo
	{
		public Type ViewType { get; set; }
		public MDataObj DataObj { get; set; }
		public string Title { get; set; }
		public string Name { get; set; }

	}

	public class CommandInfo
	{
		public string Text { get; set; }
		public EventHandler Behavior { get; set; }
		public object Owner { get; set; }
		public string EnabledProperty { get; set; }
	}

	public class BodyEditCell : ViewCell
	{
		StackLayout container;

		protected override void OnParentSet()
		{
			base.OnParentSet();
			UIBodyEditContent parent = (UIBodyEditContent)Parent;

			foreach (MColumnInfo ci in parent.Columns) {
				View v = (View)Activator.CreateInstance(ci.ViewType);
				container.Children.Add(v);
				BindingManager.BindField(v, ci.DataObj, false);
			}

			foreach (CommandInfo command in parent.Commands) {
				Button b = new Button ();
				b.Text = command.Text;
				b.Clicked += command.Behavior;
				if (!String.IsNullOrEmpty (command.EnabledProperty)) {
					Binding binding = new Binding (command.EnabledProperty);
					binding.Source = ((MDBTObject)command.Owner).Document; 
					binding.Mode = BindingMode.OneWay;
					b.SetBinding (Button.IsVisibleProperty, binding);
				}
				b.CommandParameter = new CommandArgs () { DataSource = BindingContext, Owner = command.Owner };
				b.HorizontalOptions = LayoutOptions.EndAndExpand;
				b.Style = (Style)Application.Current.Resources ["ButtonStyle"];
				container.Children.Add (b);
			}
		}

		public BodyEditCell()
		{
			container = new StackLayout()
			{
				Padding = new Thickness(5, 5, 0, 5),
				Orientation = StackOrientation.Horizontal,
				Spacing = 15,
			};
			this.View = container;
		}
	}

	public class DocumentEventArgs : EventArgs
	{
		public string key { get; set; }
	}

	public class CommandArgs 
	{
		public object DataSource;
		public object Owner;
	}

	public class UIDBTBodyEdit : UIBodyEdit
	{
		MDBTSlaveBuffered dbt;
		public UIDBTBodyEdit ()
		{
//			AddRowButton.IsVisible = true;
			AddRowButton.Clicked += delegate(object sender, EventArgs e) {
				Device.BeginInvokeOnMainThread (delegate {
					MSqlRecord record = dbt.AddRecord ();
					EditRecord(record, true);
				});
			};
			OnItemSelected += delegate(object sender, SelectedItemChangedEventArgs e) {
				if (e.SelectedItem == null) return;
				EditRecord((MSqlRecord)e.SelectedItem);
			};
		}
		private void EditRecord(MSqlRecord record, bool isNew = false)
		{
			EditRow es = new EditRow (record, isNew);
			Page p = Application.Current.MainPage;
			p.Navigation.PushModalAsync (es, true);
		}

		public void SetDataSource(MDBTSlaveBuffered dbt)
		{
			this.dbt = dbt;
			ItemsSource = dbt.Rows;
			Binding binding = new Binding ("IsEditing");
			binding.Source = dbt.Document; 
			binding.Mode = BindingMode.OneWay;
			AddRowButton.SetBinding (Button.IsVisibleProperty, binding);
		}
	}
}

