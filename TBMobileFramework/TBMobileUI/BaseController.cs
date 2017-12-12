using System;
using System.Collections.Generic;
using Xamarin.Forms;
using TBMobile;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;

namespace TBMobileUI
{
	public delegate Page GetListDocumentView();

	public delegate Page GetSelected(MSqlRecord record);

	public delegate void Foo(MSqlRecord record);

	public class BaseController
	{
		Dictionary<string, DocumentEntry> documents = new Dictionary<string, DocumentEntry> ();

		internal IEnumerable DocumentEntries {
			get {
				return documents.Values;
			}
		}

		public EventHandler DeleteRow = ((object sender, EventArgs e) =>  {
			Device.BeginInvokeOnMainThread (async () => {
				Page p = Application.Current.MainPage;
				string action;
				action = await p.DisplayActionSheet ("Do you want delete?", "Cancel", null, "Confirm");
				if (action == "Confirm"){
					Button b = (Button)sender;
					CommandArgs ca = (CommandArgs)b.CommandParameter;
					((MDBTSlaveBuffered)ca.Owner).DeleteRow((MSqlRecord)ca.DataSource);
					foreach (MSqlRecord item in ((MDBTSlaveBuffered)ca.Owner).Rows)
					{
						if (item.EqualsByKey((MSqlRecord)ca.DataSource)) {
							((MDBTSlaveBuffered)ca.Owner).Rows.Remove(item);
							break;
						}
					}
				}
			});
		});

		public EventHandler DeleteDocument = ((object sender, EventArgs e) => {
			Device.BeginInvokeOnMainThread (async () => {
				Page p = Application.Current.MainPage;
				string action;
				action = await p.DisplayActionSheet ("Do you want delete?", "Cancel", null, "Confirm");
				if (action == "Confirm") {
					Button b = (Button)sender;
					CommandArgs ca = (CommandArgs)b.CommandParameter;
					((MDocument)ca.Owner).Master.Record.Assign((MSqlRecord)ca.DataSource);
					((MDocument)ca.Owner).BrowseRecord();
					((MDocument)ca.Owner).DeleteCurrentRecord();
					foreach (MSqlRecord record in ((MDocument)ca.Owner).BrowserRecords)
					{
						if (record.EqualsByKey((MSqlRecord)ca.DataSource)) {
							((MDocument)ca.Owner).BrowserRecords.Remove(record);
							break;
						}
					}
				}
			});
		});

		protected void RegisterDocument(string name, string title, GetListDocumentView getListDocumentView, GetSelected getSelected)
		{
			DocumentEntry entry = new DocumentEntry{Name = name, Title=title, GetListDocumentView = getListDocumentView, GetSelected = getSelected};
			documents [name] = entry;
		}

		protected void AddColumnsToListDinamically (DocumentListView view, MDocument model)
		{
			if (view == null || model == null)
				return;

			foreach (MSqlRecordItem item in model.Master.Record.Fields) {
				if (item.IsSegmentKey || item.IsDescriptionField)
					view.Body.AddColumn<Label> (item.Name, item.Name, item.DataObj);
			}

		} 

		public Page GetListDocumentView(string name)
		{
			DocumentEntry entry;
			if (documents.TryGetValue (name, out entry))
				return entry.GetListDocumentView ();
			return null;
		}

		public Page GetSelectedDocumentView(string name, MSqlRecord record)
		{
			DocumentEntry entry;
			if (documents.TryGetValue (name, out entry))
				return entry.GetSelected (record);
			return null;
		}

		public BaseController ()
		{}
	}
	 
	internal class DocumentEntry
	{
		public override string ToString ()
		{
			return Title;
		}
		public GetSelected GetSelected {
			get;
			set;
		}
		public GetListDocumentView GetListDocumentView {
			get;
			set;
		}
		public string Name {
			get;
			set;
		}
		public string Title {
			get;
			set;
		}
	}
}

