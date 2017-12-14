using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microarea.TaskBuilderNet.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.DiagnosticManager
{
	#region ExtendedInfoItem
	//=========================================================================
	public class ExtendedInfoItem : IExtendedInfoItem
	{
		string name;
		object info;

		//---------------------------------------------------------------------
		public string Name { get { return name; } }
		public object Info { get { return info; } }

		//---------------------------------------------------------------------
		public ExtendedInfoItem(string name, object info)
		{
			this.name = name;
			this.info = info;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			if (info == null)
				return string.Empty;

			return info.ToString();
		}
	}
	#endregion

	#region ExtendedInfo
	//=========================================================================
	public class ExtendedInfo : ArrayList, IExtendedInfo
	{
		#region Constructors
		//---------------------------------------------------------------------
		public ExtendedInfo()
		{ }

		//---------------------------------------------------------------------
		public ExtendedInfo(ExtendedInfoItem[] items)
		{
			if (items == null)
				return;

			foreach (ExtendedInfoItem item in items)
				Add(item);
		}

		//---------------------------------------------------------------------
		public ExtendedInfo(ExtendedInfoItem item)
		{
			base.Add(item);
		}

		//---------------------------------------------------------------------
		public ExtendedInfo(string name, object info)
		{
			base.Add(new ExtendedInfoItem(name, info));
		}
		#endregion

		//---------------------------------------------------------------------
		public void Add(string name, object info)
		{
			base.Add(new ExtendedInfoItem(name, info));
		}

		//---------------------------------------------------------------------
		public new IExtendedInfoItem this[int index]
		{
			get
			{
				IExtendedInfoItem o = base[index] as IExtendedInfoItem;
				if (o != null)
					return o;

				Debug.Fail("ExtendedInfo[] : Invalid element type");
				return null;
			}
			set
			{
				base[index] = value;
			}
		}

		//---------------------------------------------------------------------
		public virtual object this[string name]
		{
			get
			{
				object o = null;
				foreach (IExtendedInfoItem item in this)
					if (string.Compare(item.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						o = item.Info;
						break;
					}

				return (o == null) ? string.Empty : o;
			}
		}

		//---------------------------------------------------------------------
		public static string Separator(LineSeparator separator)
		{
			string sep = string.Empty;
			switch (separator)
			{
				case LineSeparator.Blank: sep = " "; break;
				case LineSeparator.Cr: sep = "\r"; break;
				case LineSeparator.CrLf: sep = "\r\n"; break;
				case LineSeparator.Lf: sep = "\n"; break;
				case LineSeparator.None: sep = ""; break;
				case LineSeparator.Tab: sep = "\t"; break;
				case LineSeparator.Br: sep = "<br />"; break;
				default: sep = "\n"; break;
			}

			return sep;
		}

		//---------------------------------------------------------------------
		public virtual string Format(LineSeparator separator)
		{
			string sep = Separator(separator);
			string infos = string.Empty;

			foreach (IExtendedInfoItem item in this)
			{
				if (item == null)
					continue;

				if (!string.IsNullOrEmpty(item.Name))
					infos += string.Format(CultureInfo.CurrentCulture,
											"{0}: {1}",
											item.Name,
											(item.Info != null) ? item.Info.ToString() : string.Empty);
				else
					infos += string.Format(CultureInfo.CurrentCulture,
											"{0}",
											(item.Info != null) ? item.Info.ToString() : string.Empty);
				infos += sep;
			}

			return infos;
		}
	}
	#endregion

	#region DiagnosticItem
	//=========================================================================
	public class DiagnosticItem : IDiagnosticItem
	{
		#region Private members
		private DiagnosticType type = DiagnosticType.None;
		private IExtendedInfo extendedInfo = null;
		private DateTime occurred;
		private StringCollection explain = null;
		private bool showExtendedInfos = false;
		private int logEventID = 0;
		private short logCategory = 0;
		private string installation = string.Empty;
		#endregion

		#region Public properties
		public DiagnosticType Type { get { return type; } }
		public IExtendedInfo ExtendedInfo { get { return extendedInfo; } }
		public DateTime Occurred { get { return occurred; } set { occurred = value; } }
		public StringCollection Explain { get { return explain; } }
		public string FullExplain { get { return ToString(LineSeparator.Lf); } }
		public bool ShowExtendedInfos { get { return showExtendedInfos; } set { showExtendedInfos = value; } }
		public bool IsError { get { return ((type & DiagnosticType.Error) == DiagnosticType.Error); } }
		public bool IsWarning { get { return ((type & DiagnosticType.Warning) == DiagnosticType.Warning); } }
		public bool IsLogInfo { get { return ((type & DiagnosticType.LogInfo) == DiagnosticType.LogInfo); } }
		public bool IsInformation { get { return ((type & DiagnosticType.Information) == DiagnosticType.Information); } }
		public string Installation { get { return installation; } set { installation = value; } }
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public DiagnosticItem
		(
			DiagnosticType type,
			IExtendedInfo extendedInfo,
			StringCollection explain,
			int logEventID,
			short logCategory
		)
		{
			this.type = type;
			this.extendedInfo = extendedInfo;
			this.occurred = DateTime.UtcNow;
			this.explain = explain;
			this.logEventID = logEventID;
			this.logCategory = logCategory;
		}

		//---------------------------------------------------------------------
		public DiagnosticItem
		(
			DiagnosticType type,
			DateTime dateTime,
			IExtendedInfo extendedInfo,
			StringCollection explain,
			int logEventID,
			short logCategory
		)
		{
			this.type = type;
			this.extendedInfo = extendedInfo;
			this.occurred = dateTime;
			this.explain = explain;
			this.logEventID = logEventID;
			this.logCategory = logCategory;
		}

		//---------------------------------------------------------------------
		public DiagnosticItem
		(
			DiagnosticType type,
			IExtendedInfo extendedInfo,
			string explain,
			int logEventID,
			short logCategory
		) : this(type, extendedInfo, new StringCollection(), logEventID, logCategory)
		{
			this.explain.Add(explain);
		}

		//---------------------------------------------------------------------
		public DiagnosticItem
		(
			DiagnosticType type,
			DateTime dateTime,
			IExtendedInfo extendedInfo,
			string explain,
			int logEventID,
			short logCategory
		) : this(type, extendedInfo, new StringCollection(), logEventID, logCategory)
		{
			this.explain.Add(explain);
			this.Occurred = dateTime;
		}
		#endregion

		//---------------------------------------------------------------------
		public string ToString(LineSeparator separator)
		{
			string result = string.Empty;
			bool first = true;
			string sep = Microarea.TaskBuilderNet.Core.DiagnosticManager.ExtendedInfo.Separator(separator);

			foreach (string s in explain)
			{
				if (!first)
					result += sep;
				else
					first = false;

				result += s;
			}

			if (showExtendedInfos && ExtendedInfo != null)
				result += ExtendedInfo.Format(separator);

			return result;
		}

		//---------------------------------------------------------------------
		public bool WriteEventLogEntry(EventLog aEventLog)
		{
			if (aEventLog == null)
				return false;

            try
            {
                string entryMessage = null;
                //  " A string written to the event log cannot exceed 32766 characters.
                if (FullExplain.Length > 30000)//30000 perchè a tentativi era quello che gli andava bene senza andare in eccezione
                    entryMessage = FullExplain.Substring(0, 30000) + "[TRUNCATED]";
                else entryMessage = FullExplain;


                entryMessage = (string.IsNullOrEmpty(installation)) ? entryMessage : installation + ": " + entryMessage;

                if (IsError)
                    aEventLog.WriteEntry(entryMessage, EventLogEntryType.Error, logEventID, logCategory);
                if (IsWarning)
                    aEventLog.WriteEntry(entryMessage, EventLogEntryType.Warning, logEventID, logCategory);
                if (IsInformation)
                    aEventLog.WriteEntry(entryMessage, EventLogEntryType.Information, logEventID, logCategory);

                return true;
            }
            catch (Exception exception)
			{
				Debug.Fail("Error in DiagnosticItem.WriteEventLogEntry: " + exception.Message);
				return false;
			}
		}

		///<summary>
		/// Scrive una riga in un file di log incrementale
		///</summary>
		//---------------------------------------------------------------------
		public void WriteLogOnFile(string logFileFullPath)
		{
			if (string.IsNullOrWhiteSpace(logFileFullPath))
				return;

			try
			{
				// se non esiste il file viene creato nel path specificato
				using (StreamWriter logfile = new StreamWriter(logFileFullPath, true))
				{
					logfile.Write(Occurred.ToString("MM/dd/yyyy hh:mm:ss tt") + ": ");
					logfile.WriteLine(FullExplain);
					logfile.Close();
				}
			}
			catch (Exception e)
			{
				Debug.Fail("Error in DiagnosticItem.WriteLogOnFile: " + e.Message);
			}
		}
	}
	#endregion

	#region DiagnosticItems
	//=========================================================================
	public class DiagnosticItems : ArrayList, IDiagnosticItems
	{
		private string ownerName;

		public string OwnerName { get { return ownerName; } }

		#region Constructors
		//---------------------------------------------------------------------
		public DiagnosticItems(string ownerName)
		{
			this.ownerName = ownerName;
		}

		//---------------------------------------------------------------------
		public DiagnosticItems() : this(string.Empty)
		{
		}
		#endregion

		//---------------------------------------------------------------------
		public new IDiagnosticItem this[int index]
		{
			get { return base[index] as IDiagnosticItem; }
			set { base[index] = value; }
		}

		//---------------------------------------------------------------------
		public int CountOfType(DiagnosticType diagnosticType)
		{
			if (diagnosticType == DiagnosticType.All)
				return Count;

			int total = 0;

			foreach (IDiagnosticItem item in this)
			{
				if ((diagnosticType == DiagnosticType.None && item.Type == DiagnosticType.None) ||
					(item.Type & diagnosticType) != DiagnosticType.None)
					total++;
			}

			return total;
		}

		//---------------------------------------------------------------------
		public bool AreItemsOfTypePresent(DiagnosticType diagnosticType)
		{
			foreach (IDiagnosticItem item in this)
			{
				if ((diagnosticType == DiagnosticType.None && item.Type == DiagnosticType.None) ||
					(item.Type & diagnosticType) != DiagnosticType.None)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems GetItems(DiagnosticType diagnosticType)
		{
			if (diagnosticType == DiagnosticType.All)
				return this;

			DiagnosticItems items = new DiagnosticItems(ownerName);
			foreach (DiagnosticItem item in this)
			{
				if ((diagnosticType == DiagnosticType.None && item.Type == DiagnosticType.None) ||
					(item.Type & diagnosticType) != DiagnosticType.None)
					items.Add(item);
			}

			return (items.Count > 0) ? items : null;
		}

		//---------------------------------------------------------------------
		public void Clear(DiagnosticType diagnosticType)
		{
			if (diagnosticType == DiagnosticType.All)
				Clear();

			for (int i = Count - 1; i >= 0; i--)
				if ((diagnosticType == DiagnosticType.None && this[i].Type == DiagnosticType.None) ||
					(this[i].Type & diagnosticType) != DiagnosticType.None)
					RemoveAt(i);
		}
	}
	#endregion

	#region DiagnosticElements
	//=========================================================================
	public class DiagnosticElements : IDiagnosticElements
	{
		private ArrayList diagnosticItemsList;
		private string ownerName;

		//---------------------------------------------------------------------
		internal ArrayList DiagnosticItemsList { get { return diagnosticItemsList; } }

		//---------------------------------------------------------------------
		public int Count
		{
			get
			{
				if (diagnosticItemsList == null)
					return 0;

				int total = 0;
				foreach (DiagnosticItems items in diagnosticItemsList)
					total += items.Count;

				return total;
			}
		}

		//---------------------------------------------------------------------
		public bool Error { get { return AreItemsOfTypePresent(DiagnosticType.Error); } }
		public bool Warning { get { return AreItemsOfTypePresent(DiagnosticType.Warning); } }
		public bool Information { get { return AreItemsOfTypePresent(DiagnosticType.Information); } }
		public bool LogInfos { get { return AreItemsOfTypePresent(DiagnosticType.LogInfo); } }

		//---------------------------------------------------------------------
		public int ErrorsCount { get { return CountOfType(DiagnosticType.Error); } }
		public int WarningsCount { get { return CountOfType(DiagnosticType.Warning); } }
		public int InformationsCount { get { return CountOfType(DiagnosticType.Information); } }
		public int LogInfosCount { get { return CountOfType(DiagnosticType.LogInfo); } }

		//---------------------------------------------------------------------
		public DiagnosticElements(string ownerName)
		{
			this.ownerName = ownerName;
		}

		#region Public methods
		//---------------------------------------------------------------------
		public int CountOfType(DiagnosticType diagnosticType)
		{
			if (diagnosticItemsList == null)
				return 0;

			int total = 0;
			foreach (IDiagnosticItems items in diagnosticItemsList)
				total += items.CountOfType(diagnosticType);

			return total;
		}

		//---------------------------------------------------------------------
		public bool AreItemsOfTypePresent(DiagnosticType diagnosticType)
		{
			if (diagnosticItemsList == null)
				return false;

			foreach (IDiagnosticItems items in diagnosticItemsList)
				if (items.AreItemsOfTypePresent(diagnosticType))
					return true;

			return false;
		}

		//---------------------------------------------------------------------
		public void InitDiagnosticItemsList()
		{
			if (diagnosticItemsList == null)
				diagnosticItemsList = new ArrayList();

			if (diagnosticItemsList.Count == 0)
				diagnosticItemsList.Add(new DiagnosticItems(ownerName));
		}

		//---------------------------------------------------------------------
		public void Clear(DiagnosticType diagnosticType)
		{
			if (diagnosticItemsList == null)
				return;

			if (diagnosticType == DiagnosticType.All)
			{
				diagnosticItemsList.Clear();
				diagnosticItemsList = null;
				return;
			}

			int itemsCount = diagnosticItemsList.Count - 1;
			for (int i = itemsCount; i >= 0; i--)
			{
				DiagnosticItems diagnosticItems = (DiagnosticItems)diagnosticItemsList[i];
				diagnosticItems.Clear(diagnosticType);
				if (diagnosticItems.Count == 0)
					diagnosticItemsList.RemoveAt(i);
			}

			if (diagnosticItemsList.Count == 0)
				diagnosticItemsList = null;
		}

		//---------------------------------------------------------------------
		public void Clear()
		{
			Clear(DiagnosticType.All);
		}

		//---------------------------------------------------------------------
		public void Set(IDiagnosticElements diagnosticElement)
		{
			Set(DiagnosticType.All, diagnosticElement);
		}

		// eredita gli errori dal Diagnostic che viene passato e si mette in stato di errore
		//---------------------------------------------------------------------
		public void Set(DiagnosticType diagnosticType, IDiagnosticElements diagnosticElement)
		{
			InitDiagnosticItemsList();

			ArrayList list = (diagnosticElement as DiagnosticElements).diagnosticItemsList;
			if (list == null)
				return;

			foreach (DiagnosticItems diagnosticItems in list)
			{
				IDiagnosticItems itemsToAdd = diagnosticItems.GetItems(diagnosticType);

				if (itemsToAdd != null && itemsToAdd.Count > 0)
					diagnosticItemsList.Add(itemsToAdd);
			}
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, StringCollection explains)
		{
			return Set(diagnosticType, DateTime.UtcNow, explains, 0, 0);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, StringCollection explains)
		{
			return Set(diagnosticType, dateTime, explains, 0, 0);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, StringCollection explains, int logEventID, short logCategory)
		{
			return Set(diagnosticType, DateTime.UtcNow, explains, null, logEventID, logCategory);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, StringCollection explains, int logEventID, short logCategory)
		{
			return Set(diagnosticType, dateTime, explains, null, logEventID, logCategory);
		}

		// il primo elemento è quello specifico gli altri sono quelli inherited
		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, StringCollection explains, IExtendedInfo extendedInfo, int logEventID, short logCategory)
		{
			return Set(diagnosticType, DateTime.UtcNow, explains, extendedInfo, logEventID, logCategory);
		}

		// il primo elemento è quello specifico gli altri sono quelli inherited
		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, StringCollection explains, IExtendedInfo extendedInfo, int logEventID, short logCategory)
		{
			IDiagnosticItem diagnosticItemToAdd = new DiagnosticItem(diagnosticType, dateTime, extendedInfo, explains, logEventID, logCategory);

			InitDiagnosticItemsList();

			((DiagnosticItems)diagnosticItemsList[0]).Add(diagnosticItemToAdd);

			return diagnosticItemToAdd;
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems MyMessages(DiagnosticType diagnosticType)
		{
			if (diagnosticItemsList == null || diagnosticItemsList.Count == 0)
				return null;

			DiagnosticItems result = new DiagnosticItems(((IDiagnosticItems)diagnosticItemsList[0]).OwnerName);
			DiagnosticItems itemsOfType = ((DiagnosticItems)diagnosticItemsList[0]).GetItems(diagnosticType) as DiagnosticItems;

			if (itemsOfType != null && itemsOfType.Count > 0)
				result.AddRange(itemsOfType);

			return (result.Count > 0) ? result : null;
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems MyMessages()
		{
			return MyMessages(DiagnosticType.All);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems Messages(DiagnosticType diagnosticType, string[] selection)
		{
			DiagnosticItems result = new DiagnosticItems();

			if (diagnosticItemsList == null || diagnosticItemsList.Count == 0)
				return result;

			foreach (DiagnosticItems diagnosticItems in diagnosticItemsList)
			{
				DiagnosticItems itemsOfType = diagnosticItems.GetItems(diagnosticType) as DiagnosticItems;

				if (itemsOfType == null)
					continue;
				// se non ho dato selezione vuol dire che li voglio tutti 
				if (selection == null)
				{
					result.AddRange(itemsOfType);
					continue;
				}

				// esiste un elenco di ownerName da selezionare
				foreach (string ownerName in selection)
					if (string.Compare(ownerName, diagnosticItems.OwnerName, StringComparison.InvariantCultureIgnoreCase) == 0)
						result.AddRange(itemsOfType);
			}

			return result;
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems AllMessages(DiagnosticType diagnosticType)
		{
			return Messages(diagnosticType, null);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems AllMessages()
		{
			return AllMessages(DiagnosticType.All);
		}
		#endregion
	}
	#endregion

	#region Diagnostic
	//=========================================================================
	public class Diagnostic : IDiagnostic
	{
		public event EventHandler AddedDiagnostic;

		private string ownerName;
		private DiagnosticElements elements;
		private bool autoWriteLog = true;
		private string installation = string.Empty;

		// path del file txt incrementale dove salvare le info da loggare
		private bool writeLogOnFile = false;
		private string logFileFullPath = string.Empty;

		public const string EventLogName = "MA Server";

		#region Properties

		public string OwnerName { get { return ownerName; } }

		//---------------------------------------------------------------------
		public IDiagnosticItem[] AllItems
		{
			get
			{
				List<IDiagnosticItem> allItems = new List<IDiagnosticItem>();

				ArrayList list = (Elements as DiagnosticElements).DiagnosticItemsList;
				if (list == null)
					return allItems.ToArray();

				foreach (DiagnosticItems items in list)
					foreach (DiagnosticItem item in items)
						allItems.Add(item);

				return allItems.ToArray();
			}
		}

		//---------------------------------------------------------------------
		public IDiagnosticSimpleItem[] AllSimpleItems
		{
			get
			{
				List<IDiagnosticSimpleItem> items = new List<IDiagnosticSimpleItem>();
				foreach (IDiagnosticItem i in AllItems)
				{
					IDiagnosticSimpleItem si = new DiagnosticSimpleItem();
					si.Message = i.FullExplain;
					si.Type = i.Type;
					items.Add(si);
				}
				return items.ToArray();
			}
		}

		//---------------------------------------------------------------------
		public IDiagnosticElements Elements { get { return elements; } }
		public bool Error { get { return elements.Error; } }
		public bool Warning { get { return elements.Warning; } }
		public bool Information { get { return elements.Information; } }
		public bool LogInfos { get { return elements.LogInfos; } }

		//---------------------------------------------------------------------
		public int TotalWarnings { get { return elements.WarningsCount; } }
		public int TotalErrors { get { return elements.ErrorsCount; } }
		public int TotalInformations { get { return elements.InformationsCount; } }
		public int TotalLogInfos { get { return elements.LogInfosCount; } }

		//---------------------------------------------------------------------
		public bool AutoWriteLog { get { return autoWriteLog; } }
		public string Installation { get { return installation; } set { installation = value; } }

		//---------------------------------------------------------------------
		public string LogFileFullPath { get { return logFileFullPath; } set { logFileFullPath = value; } }
		public bool WriteLogOnFile { get { return writeLogOnFile; } set { writeLogOnFile = value; } }
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public Diagnostic(string ownerName, bool autoWriteLog)
		{
			this.ownerName = ownerName;

			elements = new DiagnosticElements(ownerName);

			this.autoWriteLog = autoWriteLog;
		}

		//---------------------------------------------------------------------
		public Diagnostic(string ownerName) : this(ownerName, true)
		{
		}
		#endregion

		#region Public methods
		// eredita dal diagnostico passato
		//---------------------------------------------------------------------
		public void Set(DiagnosticType diagnosticType, IDiagnostic diagnostic)
		{
			elements.Set(diagnosticType, diagnostic.Elements);
		}

		//---------------------------------------------------------------------
		public void Set(IDiagnostic diagnostic)
		{
			if (diagnostic != null && diagnostic.Elements.Count > 0)
				elements.Set(diagnostic.Elements);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, StringCollection explain, IExtendedInfo extendedInfo, int logEventID, short logCategory)
		{
			if (diagnosticType == DiagnosticType.None)
				return null;

			IDiagnosticItem diagnosticElementAdded = elements.Set(diagnosticType, dateTime, explain, extendedInfo, logEventID, logCategory);

			if (autoWriteLog && (diagnosticType & DiagnosticType.LogInfo) == DiagnosticType.LogInfo)
			{
				diagnosticElementAdded.Installation = installation;
				diagnosticElementAdded.WriteEventLogEntry(GetDiagnosticEventLog());
			}

			if (writeLogOnFile && (diagnosticType & DiagnosticType.LogOnFile) == DiagnosticType.LogOnFile)
			{
				diagnosticElementAdded.ShowExtendedInfos = true;
				((DiagnosticItem)diagnosticElementAdded).WriteLogOnFile(LogFileFullPath);
			}

			OnAddedDiagnostic();

			return diagnosticElementAdded;
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, StringCollection explain, IExtendedInfo extendedInfo, int logEventID, short logCategory)
		{
			return Set(diagnosticType, DateTime.UtcNow, explain, extendedInfo, logEventID, logCategory);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, StringCollection explain, int logEventID, short logCategory)
		{
			return Set(diagnosticType, DateTime.UtcNow, explain, null, logEventID, logCategory);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, StringCollection explain, int logEventID, short logCategory)
		{
			return Set(diagnosticType, dateTime, explain, null, logEventID, logCategory);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, StringCollection explain)
		{
			return Set(diagnosticType, explain, 0, 0);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, StringCollection explain)
		{
			return Set(diagnosticType, dateTime, explain, 0, 0);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain)
		{
			return Set(diagnosticType, dateTime, explain, 0, 0);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, string explain)
		{
			return Set(diagnosticType, explain, 0, 0);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, string explain, string extendedMessage)
		{
			return Set(diagnosticType, DateTime.UtcNow, explain, new ExtendedInfo(string.Empty, extendedMessage));
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain, string extendedMessage)
		{
			return Set(diagnosticType, dateTime, explain, new ExtendedInfo(string.Empty, extendedMessage));
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, string explain, int logEventID, short logCategory)
		{
			return Set(diagnosticType, DateTime.UtcNow, explain, null, logEventID, logCategory);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain, int logEventID, short logCategory)
		{
			return Set(diagnosticType, dateTime, explain, null, logEventID, logCategory);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain, IExtendedInfo extendedInfo)
		{
			return Set(diagnosticType, dateTime, explain, extendedInfo, 0, 0);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, string explain, IExtendedInfo extendedInfo)
		{
			return Set(diagnosticType, DateTime.UtcNow, explain, extendedInfo, 0, 0);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, string explain, IExtendedInfo extendedInfo, int logEventID, short logCategory)
		{
			StringCollection explains = new StringCollection();
			explains.Add(explain);

			return Set(diagnosticType, DateTime.UtcNow, explains, extendedInfo, logEventID, logCategory);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem Set(DiagnosticType diagnosticType, DateTime dateTime, string explain, IExtendedInfo extendedInfo, int logEventID, short logCategory)
		{
			StringCollection explains = new StringCollection();
			explains.Add(explain);

			return Set(diagnosticType, dateTime, explains, extendedInfo, logEventID, logCategory);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetError(StringCollection explain)
		{
			return Set(DiagnosticType.Error, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetError(DateTime dateTime, StringCollection explain)
		{
			return Set(DiagnosticType.Error, dateTime, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetError(DateTime dateTime, string explain)
		{
			return Set(DiagnosticType.Error, dateTime, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetError(string explain)
		{
			return Set(DiagnosticType.Error, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetWarning(StringCollection explain)
		{
			return Set(DiagnosticType.Warning, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetWarning(DateTime dateTime, StringCollection explain)
		{
			return Set(DiagnosticType.Warning, dateTime, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetWarning(DateTime dateTime, string explain)
		{
			return Set(DiagnosticType.Warning, dateTime, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetWarning(string explain)
		{
			return Set(DiagnosticType.Warning, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetInformation(StringCollection explain)
		{
			return Set(DiagnosticType.Information, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetInformation(DateTime dateTime, StringCollection explain)
		{
			return Set(DiagnosticType.Information, dateTime, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetInformation(DateTime dateTime, string explain)
		{
			return Set(DiagnosticType.Information, dateTime, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetInformation(string explain)
		{
			return Set(DiagnosticType.Information, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetLogInfo(StringCollection explain)
		{
			return Set(DiagnosticType.LogInfo, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetLogInfo(DateTime dateTime, StringCollection explain)
		{
			return Set(DiagnosticType.LogInfo, dateTime, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetLogInfo(DateTime dateTime, string explain)
		{
			return Set(DiagnosticType.LogInfo, dateTime, explain);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItem SetLogInfo(string explain)
		{
			return Set(DiagnosticType.LogInfo, explain);
		}

		//---------------------------------------------------------------------
		public void Clear()
		{
			Clear(DiagnosticType.All);
		}

		//---------------------------------------------------------------------
		public void Clear(DiagnosticType diagnosticType)
		{
			elements.Clear(diagnosticType);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems MyMessages(DiagnosticType diagnosticType)
		{
			return elements.MyMessages(diagnosticType);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems Messages(DiagnosticType diagnosticType, string[] selection)
		{
			return elements.Messages(diagnosticType, selection);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems AllMessages(DiagnosticType diagnosticType)
		{
			return elements.AllMessages(diagnosticType);
		}

		//---------------------------------------------------------------------
		public IDiagnosticItems AllMessages()
		{
			return elements.AllMessages();
		}

		//---------------------------------------------------------------------
		public void WriteLogInfos()
		{
			WriteLogInfos(null);
		}

		//---------------------------------------------------------------------
		public bool WriteLogInfos(string[] selection)
		{
			IDiagnosticItems logInfoItems = Messages(DiagnosticType.LogInfo, selection);

			if (logInfoItems == null || logInfoItems.Count == 0)
				return false;

			EventLog eventLog = GetDiagnosticEventLog();

			if (eventLog == null)
				return false;

			foreach (IDiagnosticItem logInfoItem in logInfoItems)
				logInfoItem.WriteEventLogEntry(eventLog);

			return true;
		}

		/// <summary>
		/// Crea una stringa formattata con tutti i messaggi riportati.
		/// </summary>
		//---------------------------------------------------------------------
		public override string ToString()
		{
			string message = string.Empty;
			IDiagnosticItems items = AllMessages();

			if (items == null)
				return string.Empty;

			foreach (IDiagnosticItem item in items)
			{
				if (!string.IsNullOrEmpty(item.FullExplain))
					message += item.FullExplain + "\r\n";
			}

			return message;
		}

		//---------------------------------------------------------------------
		public string GetErrorsStrings()
		{
			string message = string.Empty;
			IDiagnosticItems items = AllMessages(DiagnosticType.Error);

			if (items == null)
				return string.Empty;

            foreach (IDiagnosticItem item in items)
            {
                if (string.IsNullOrWhiteSpace(item.FullExplain))
                    continue;

                message += item.FullExplain + "\r\n";
                if (item.ExtendedInfo != null)
                    foreach (IExtendedInfoItem ei in item.ExtendedInfo)
                    {
                        message += ei.ToString() + "\r\n";
                    }
            }

			return message;
		}

		/// <summary>
		/// Logga i messaggi di un componente di libreria che attualmente ignora
		/// l'output della messaggistica.
		/// TODO RICCARDO: 
		/// </summary>
		//---------------------------------------------------------------------
		public bool WriteChildDiagnostic(IDiagnostic child, bool bClearChild)
		{
			IDiagnosticItems logInfoItems = child.AllMessages();

			if (logInfoItems == null || logInfoItems.Count == 0)
				return false;

			EventLog eventLog = GetDiagnosticEventLog();

			if (eventLog == null)
				return false;

			foreach (IDiagnosticItem logInfoItem in logInfoItems)
				logInfoItem.WriteEventLogEntry(eventLog);

			if (bClearChild)
				child.Clear();

			return true;
		}
		#endregion

		//---------------------------------------------------------------------
		protected virtual void OnAddedDiagnostic()
		{
			if (AddedDiagnostic != null)
				AddedDiagnostic(this, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		internal EventLog GetDiagnosticEventLog()
		{
			if (ownerName == null || ownerName == String.Empty)
			{
				Debug.Fail("Error in Diagnostic.GetDiagnosticEventLog: unspecified log source name.");
				return null;
			}

			try
			{
				if (EventLog.SourceExists(ownerName))
				{
					// Se esiste già un event log che sta utilizzando il source name 
					// restituisco null... è una situazione anomala, ma potrebbe accadere 
					// che contemporaneamente un componente tenti di di scrivere su due log 
					// distinti: ad es. lo Scheduler Agent è attivo e fa logging sul
					// suo event Log ( che si chiama Microarea SchedulerAgent) mentre si 
					// sta lavorando con la Console che usa uno stesso componente dello 
					// Scheduler Agent ma che stavolta fa logging sul log della Console.
					string existingLogName = EventLog.LogNameFromSourceName(ownerName, ".");
					if (String.Compare(Diagnostic.EventLogName, existingLogName, StringComparison.InvariantCultureIgnoreCase) != 0)
						return null;
				}
				else
					EventLog.CreateEventSource(ownerName, Diagnostic.EventLogName);

				EventLog eventLog = new EventLog();
				eventLog.Log = Diagnostic.EventLogName;
				eventLog.Source = ownerName;

				return eventLog;
			}
			catch (Exception)
			{
				//Debug.Fail("Error in DiagnosticItem.WriteEventLogEntry: " + exception.Message);
				return null;
			}
		}

		//---------------------------------------------------------------------
		public string ToJson(bool writeResult)
		{
			if (AllItems.Length == 0 && !writeResult)
				return string.Empty;
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				using (JsonWriter jsonWriter = new JsonTextWriter(sw))
				{
					jsonWriter.WriteStartObject();
					jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
					ToJson(jsonWriter, writeResult);
					jsonWriter.WriteEndObject();
					return sb.ToString();
				}
			}
		}
		//---------------------------------------------------------------------
		public void ToJson(JsonWriter jsonWriter, bool writeResult)
		{
			if (writeResult)
			{
				jsonWriter.WritePropertyName("success");
				jsonWriter.WriteValue(!this.Error);
			}
			if (AllItems.Length == 0)
				return;
			jsonWriter.WritePropertyName("messages");
			jsonWriter.WriteStartArray();
			foreach (var item in AllItems)
			{
				jsonWriter.WriteStartObject();
				jsonWriter.WritePropertyName("text");
				jsonWriter.WriteValue(item.FullExplain);
				jsonWriter.WritePropertyName("type");
				jsonWriter.WriteValue((int)item.Type);
				jsonWriter.WriteEndObject();
			}

			jsonWriter.WriteEndArray();
		}
	}
	#endregion

	#region DiagnosticSession
	/// <summary>
	/// Contiene uno stack di diagnostic per gestire i livelli nidificati di messaggi
	/// </summary>
	//=========================================================================
	public class DiagnosticSession
	{
		Diagnostic currentDiagnostic = null;
		Stack<Diagnostic> diagnosticLevels = new Stack<Diagnostic>();

		public Diagnostic CurrentDiagnostic
		{
			get { return currentDiagnostic; }
		}

		public Diagnostic StartSession(string name)
		{
			currentDiagnostic = new Diagnostic(name);
			diagnosticLevels.Push(currentDiagnostic);
			return currentDiagnostic;
		}
		public Diagnostic EndSession()
		{
			currentDiagnostic = diagnosticLevels.Pop();
			return currentDiagnostic;
		}
	}
	#endregion
}
