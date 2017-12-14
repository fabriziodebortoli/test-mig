using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace TBMobile
{
	public class DocumentContext
	{
		DiagnosticLevel level = new DiagnosticLevel();

		internal bool ErrorFound()
		{
			throw new NotImplementedException();
		}

		internal bool WarningFound()
		{
			throw new NotImplementedException();
		}

		internal bool InfoFound()
		{
			throw new NotImplementedException();
		}

		internal bool MessageFound()
		{
			throw new NotImplementedException();
		}

		internal void EndMessageSession(string closingBanner)
		{
			throw new NotImplementedException();
		}

		internal void StartMessageSession(string openingBanner)
		{
			throw new NotImplementedException();
		}

		internal void AddMessage(string message, DiagnosticType type)
		{
			level.Items.Add(new DiagnosticItem { Message=message, Type = type });
		}

		internal List<DiagnosticItem> GetMessages()
		{
			return level.Items;
		}

		internal void ClearMessages()
		{
			level.Items.Clear();
		}
	}

	[Flags]
	public enum DiagnosticType
	{
		None = 0,
		Warning = 1,
		Error = 2,
		LogInfo = 4,
		Information = 8,
		FatalError = 16,
		Banner = 32,
		LogOnFile = 64,
		All = 127,
	}
	public class DiagnosticItem
	{
		public string Message { get; set; }
		public DiagnosticType Type { get; set; }
	}
	public class DiagnosticLevel
	{
		public List<DiagnosticItem> Items = new List<DiagnosticItem>();
		public List<DiagnosticLevel> InnerLevels = new List<DiagnosticLevel>();
	}
	public class DiagnosticEventArgs : EventArgs
	{
		public List<DiagnosticItem> Messages { get; set; }
	}
	public abstract class MDocument : INotifyPropertyChanged
	{
		public enum FormModeType
		{
			None = 0,
			Browse = 1,
			New = 2,
			Edit = 3,
			Find = 4,
			Design = 5,
		}

		MDBTMaster dbtMaster;
		LoginContext loginContext;
		public event PropertyChangedEventHandler PropertyChanged;

		bool modified = false;
		string title = "";

		public bool IsEditing {
			get {
				return FormMode == FormModeType.Edit || FormMode == FormModeType.New;
			}
		}
		string id = Guid.NewGuid().ToString();
		private FormModeType formMode = FormModeType.None;
		public FormModeType FormMode{
			get { return formMode;}
			set { if (formMode!= value)
				{
					formMode = value;
					EnableFields(); 
					if (PropertyChanged != null)
					{
						PropertyChanged (this, new PropertyChangedEventArgs("FormMode"));
						PropertyChanged (this, new PropertyChangedEventArgs("IsEditing"));
					}
					if (FormModeChange != null)	
						FormModeChange (this, EventArgs.Empty); 
				}
			}
		}
		
		private bool hasData;

		public string Id { get { return id; } }
		public bool SaveModified { get { return modified; } }
		internal HTTPConnector Connector { get { return loginContext.Connector; } }
		public ObservableCollection<MSqlRecord> BrowserRecords = new ObservableCollection<MSqlRecord>();

		public event EventHandler DocumentReady;
		public event EventHandler FormModeChange;
		public event EventHandler<ErrorEventArgs> Error;
		public event EventHandler<CancelEventArgs> DataLoaded;
		public event EventHandler<DiagnosticEventArgs> MessagesAvailable;
		DocumentContext documentContext = new DocumentContext();

		/*public event EventHandler<EventArgs> Aborted;
		public event EventHandler<EventArgs> BatchExecuted;
		public event EventHandler<CancelEventArgs> BatchExecuting;
		public event EventHandler<CancelEventArgs> BeforeValidating;
		public event EventHandler<EventArgs> ClosingDocument;
		public event EventHandler<EventArgs> ControlsEnabled;
		public event EventHandler<EventArgs> ControlsEnabledForAddOnFly;
		public event EventHandler<CancelEventArgs> DataAttached;
		public event EventHandler<CancelEventArgs> DataInitialized;
		public event EventHandler<EventArgs> DefiningChangeMap;
		public event EventHandler<EventArgs> DefiningValidation;
		public event EventHandler<CancelEventArgs> DeletingRecord;
		public event EventHandler<EventArgs> DocumentClosed;
		public event EventHandler<EventArgs> DocumentLoaded;
		public event EventHandler<EventArgs> DocumentPartsLoaded;
		public event EventHandler<EventArgs> DocumentSaved;
		public event EventHandler<EventArgs> EnteredInBrowse;
		public event EventHandler<CancelEventArgs> EnteredInEdit;
		public event EventHandler<CancelEventArgs> EnteredInNew;
		public event EventHandler<CancelEventArgs> EnteringInEdit;
		public event EventHandler<CancelEventArgs> EnteringInNew;
		public event EventHandler<EventArgs> ExtraTransacted;
		public event EventHandler<EventArgs> FormModeChanged;
		public event EventHandler<CancelEventArgs> LoadingDocument;
		public event EventHandler<EventArgs> LoadingDocumentParts;
		public event EventHandler<CancelEventArgs> RecordDeleted;
		public event EventHandler<EventArgs> SavingDocument;
		public event EventHandler<CancelEventArgs> Transacted;
		public event EventHandler<CancelEventArgs> Transacting;
		public event EventHandler<CancelEventArgs> Validating;*/

		protected MDocument()
		{

		}

		private void Open(LoginContext loginContext, string ns)
		{
			this.loginContext = loginContext;
			OnAttachData();
			FormMode = FormModeType.Browse;
			if (ns == null)
				loginContext.Connector.OpenDynamicDocument(ToJSON(), id, OnOpenDocumentResponse);
			else
				loginContext.Connector.OpenDocument(ns, id, OnOpenDocumentResponse);
		}

		private void OnOpenDocumentResponse(ResponseEventArgs args)
		{
			if (args.Success)
			{
				if (DocumentReady != null)
					DocumentReady(this, EventArgs.Empty);
			}
			else
			{
				if (Error != null)
					Error(this, new ErrorEventArgs(args));
			}
		}
		internal string ToJSON()
		{
			return ToJSONObject().ToString();
		}
		internal JObject ToJSONObject()
		{
			JObject doc = new JObject(
							new JProperty("title", Title),
							new JProperty("master", dbtMaster.ToJSONObject()));

			return doc;
		}
		public static T Create<T>(LoginContext context, string ns = null) where T : MDocument
		{
			T doc = Activator.CreateInstance<T>();
			doc.Open(context, ns);
			return doc;
		}


		protected void AttachMaster(MDBTMaster dbtMaster)
		{
			this.dbtMaster = dbtMaster;
			dbtMaster.Document = this;
			dbtMaster.DataAvailable += (sender, args) =>
			{
				if (DataLoaded != null)
					DataLoaded(this, new CancelEventArgs());
			};
			
		}

		public bool CanDoBack() { return FormMode == FormModeType.Browse; }
		public bool CanCloseDocument() { return !modified; }
		public bool CanDoDeleteRecord() { return FormMode == FormModeType.Browse && hasData; }
		public bool CanDoEditRecord() { return FormMode == FormModeType.Browse && hasData; }
		public bool CanDoEscape() { return FormMode == FormModeType.Edit || FormMode == FormModeType.New; }
		public bool CanDoExecQuery() { return true; }
		public bool CanDoFindRecord() { return  FormMode == FormModeType.Browse; }
		public bool CanDoFirstRecord() { return  FormMode == FormModeType.Browse; }
		public bool CanDoLastRecord() { return  FormMode == FormModeType.Browse; }
		public bool CanDoNewRecord() { return  FormMode == FormModeType.Browse; }
		public bool CanDoNextRecord() { return FormMode == FormModeType.Browse; }
		public bool CanDoPrevRecord() { return FormMode == FormModeType.Browse; }
		public bool CanDoQuery() { return FormMode == FormModeType.Browse; }
		public bool CanDoRadar() { return true; }
		public bool CanDoRefreshRowset() { return true; }
		public bool CanDoSaveRecord() { return FormMode == FormModeType.Edit || FormMode == FormModeType.New; }

		public string Title
		{
			get { return title; }
			set { title = value; }
		}
		public bool Batch { get { return false; } }
		public bool BatchAborted { get { return false; } }
		public bool BatchRunning { get { return false; } }
		public bool CanClose { get { return false; } }


		
		public bool InUnattendedMode { get; set; }
		public MDBTMaster Master { get { return dbtMaster; } }
		public string Namespace { get; set; }
		public bool OnlyOneRecord { get; set; }

		private void EnableFields()
		{
			bool readOnly = FormMode == FormModeType.Browse || FormMode == FormModeType.Design || FormMode == FormModeType.None;
			Master.SetReadOnlyFields(readOnly);

		}
		
		public void BrowseRecord()
		{
			string jsonKey = Master.Record.GetJSONData(true).ToString();
			loginContext.Connector.BrowseRecord(id, jsonKey, OnServerDataChanged);
		}
		
		public void Close()
		{
			loginContext.Connector.CloseDocument(id);
		}
		public void Commit() { }
		public void DeleteCurrentRecord()
		{
			loginContext.Connector.PostCommand(id, "delete", OnServerDataChanged);
		}
		public void EditCurrentRecord()
		{
			FormMode = FormModeType.Edit;

		}

		public void EnterNewRecord()
		{
			FormMode = FormModeType.New;

		}
		public void UndoChanges()
		{
		}
		public void ExecuteBatch() { }
		public List<DiagnosticItem> GetMessages() 
		{
			return documentContext.GetMessages(); 
		}
		public void GetServerMessages(bool clear) 
		{
			loginContext.Connector.GetMessages(id, clear, (args) => {
				if (!args.Success)
					return;
				JArray ar = (JArray)args.ResponseObject["messages"];
				foreach (JObject msg in ar)
					AddMessage(msg["text"].ToString(), (DiagnosticType)(int)msg["type"]);
			});
		}
		public MDBTObject GetDBT(string name) { return null; }
		public void GoInBrowseMode()
		{
			FormMode = FormModeType.Browse;
		
		}
		public void GoInFindMode() { }

		public void AddMessage(string message, DiagnosticType type) { documentContext.AddMessage(message, type); }
		public void SetError(string message) { AddMessage(message, DiagnosticType.Error); }
		public void SetWarning(string message) { AddMessage(message, DiagnosticType.Warning); }
		public void ShowMessages(bool clearMessages)
		{
			if (MessagesAvailable != null)
				MessagesAvailable(this, new DiagnosticEventArgs { Messages = documentContext.GetMessages() });
			if (clearMessages)
				documentContext.ClearMessages();
		}
		public void StartMessageSession(string openingBanner) { documentContext.StartMessageSession(openingBanner); }
		public void EndMessageSession(string closingBanner) { documentContext.EndMessageSession(closingBanner);  }

		public bool ErrorFound() { return documentContext.ErrorFound(); }
		public bool WarningFound() { return documentContext.WarningFound(); }
		public bool InfoFound() { return documentContext.InfoFound(); }
		public bool MessageFound() { return documentContext.MessageFound(); }

		public void Rollback() { }
		public void SaveCurrentRecord()
		{
			loginContext.Connector.SetData(id, Master.GetJSONData().ToString(), (args) => {
				GetServerMessages(true);
				if (!args.Success)
					return;
				FormMode = FormModeType.Browse;
				Master.RefreshData();

			}); 
		}
		public void SetBadData(MDataObj data, string message) { }


		public bool StartTransaction() { return true; }
		public void UnlockAll() { }
		public void UpdateDataView() { }
		public void ExecQuery() { }

		public void GetBrowserData()
		{
			loginContext.Connector.PostCommand(id, "getBrowserData", (args) => {
				if (args.Success)
				{
					JArray ar = (JArray)args.ResponseObject["records"];
					BrowserRecords.Clear();
					foreach (JObject jRec in ar)
					{
						MSqlRecord rec = Master.Record.Clone();
						rec.Assign(jRec);
						BrowserRecords.Add(rec);
					};

				}
			});
		}
		public void MoveFirst()
		{
			loginContext.Connector.PostCommand(id, "moveFirst", OnServerDataChanged);
		}
		public void MoveLast()
		{
			loginContext.Connector.PostCommand(id, "moveLast", OnServerDataChanged);
		}
		public void MoveNext()
		{
			loginContext.Connector.PostCommand(id, "moveNext", OnServerDataChanged);
		}
		public void MovePrev()
		{
			loginContext.Connector.PostCommand(id, "movePrevious", OnServerDataChanged);
		}
		private void OnServerDataChanged(ResponseEventArgs args)
		{
			if (args.Success)
			{
				dbtMaster.RefreshData();
				hasData = true;
			}
		}

		
		//public  void NewWrmRadar();
		public abstract bool OnAttachData();
		//public  void OtherQuery();
		public void Query() { }
		public void Radar() { }
		public void RefreshRowset() { }
	}

	public abstract class TDocument<TDBT> : MDocument where TDBT: MDBTMaster
	{
		public new TDBT Master { get{ return (TDBT)base.Master; }} 
	}
}
