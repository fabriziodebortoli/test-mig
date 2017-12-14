using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

using Microarea.TaskBuilderNet.Woorm.WoormWebControl;

using RSjson;

namespace Microarea.TaskBuilderNet.Woorm.WoormController
{
	//=========================================================================
	[Serializable]
	[KnownType(typeof(Layout))]
	public class ReportData : ISerializable
	{
		public string message;
		public bool error;
		public bool ready;
		public Layout reportObjects;
		public short paperLength;
		public short paperWidth;

		const string MESSAGE = "message";
		const string ERROR = "error";
		const string READY = "ready";
		const string REPORT_OBJECTS = "reportObjects";
		const string PAPER_LENGTH = "paperLength";
		const string PAPER_WIDTH = "paperWidth";

		//--------------------------------------------------------------------------
		public ReportData()
		{
		}

		//--------------------------------------------------------------------------
		public ReportData(SerializationInfo info, StreamingContext context)
		{
			info.GetString(MESSAGE);
			error = info.GetBoolean(ERROR);
			ready = info.GetBoolean(READY);
			object[] arReportObjects = info.GetValue<object[]>(REPORT_OBJECTS);
			if (arReportObjects != null)
			{
				reportObjects = new Layout();
				foreach (object obj in arReportObjects)
					reportObjects.Add((BaseObj)obj);
			}

			paperLength = info.GetInt16(PAPER_LENGTH);
			paperWidth = info.GetInt16(PAPER_WIDTH);
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(MESSAGE, message);
			info.AddValue(ERROR, error);
			info.AddValue(READY, ready);
			info.AddValue(REPORT_OBJECTS, reportObjects);
			info.AddValue(PAPER_LENGTH, paperLength);
			info.AddValue(PAPER_WIDTH, paperWidth);
		}
	}

	//=========================================================================
	[Serializable]
	[KnownType(typeof(Report))]
	[KnownType(typeof(List<string>))]
	public class ReportEngineData : ISerializable
	{
		public bool ready;
		public Report report;
		public List<string> rules;

		const string READY = "ready";
		const string REPORT = "report";
		const string RULES = "rules";

		//--------------------------------------------------------------------------
		public ReportEngineData()
		{
			rules = new List<string>();
		}

		//--------------------------------------------------------------------------
		public ReportEngineData(SerializationInfo info, StreamingContext context)
		{
			//TODO SILVANO
			ready = info.GetBoolean(READY);
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(READY, ready);
			info.AddValue(REPORT, report);
			info.AddValue(RULES, rules);
		}
	}

	//=========================================================================
	[Serializable]
	[KnownType(typeof(CatalogInfo))]
	[KnownType(typeof(ArrayList))]
	[KnownType(typeof(CatalogTableEntry))]
	public class DBObjects : ISerializable
	{
		public bool ready;
		public CatalogInfo catalog;
		const string READY = "ready";
		const string CATALOG = "catalog";

		//--------------------------------------------------------------------------
		public DBObjects()
		{
		}

		//--------------------------------------------------------------------------
		public DBObjects(SerializationInfo info, StreamingContext context)
		{
			ready = info.GetBoolean(READY);
			catalog = info.GetValue<CatalogInfo>(CATALOG);
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(READY, ready);
			info.AddValue(CATALOG, catalog);
		}
	}

	//=========================================================================
	[Serializable]
	[KnownType(typeof(ArrayList))]
	[KnownType(typeof(CatalogColumn))]
	public class DBTableColumns : ISerializable
	{
		public bool ready;
		public ArrayList columns;
		const string READY = "ready";
		const string COLUMNS = "columns";

		//--------------------------------------------------------------------------
		public DBTableColumns()
		{
		}

		//--------------------------------------------------------------------------
		public DBTableColumns(SerializationInfo info, StreamingContext context)
		{
			ready = info.GetBoolean(READY);
			columns = new ArrayList();
			object[] arCols = info.GetValue<object[]>(COLUMNS);
			if (arCols != null)
			{
				columns.Add(arCols);
			}
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(READY, ready);
			info.AddValue(COLUMNS, columns);
		}
	}

	//=========================================================================
	[Serializable]
	[KnownType(typeof(AskDialog))]
	[KnownType(typeof(List<AskGroup>))]
	public class AskDialogData : ISerializable
	{
		public string message;
		public bool error;
		public bool ready;
		public AskDialog askDialog;
		
		const string MESSAGE = "message";
		const string ERROR = "error";
		const string READY = "ready";
		const string ASKDIALOG = "askDialog";

		//--------------------------------------------------------------------------
		public AskDialogData()
		{
		}

		//--------------------------------------------------------------------------
		public AskDialogData(SerializationInfo info, StreamingContext context)
		{
			info.GetString(MESSAGE);
			error = info.GetBoolean(ERROR);
			ready = info.GetBoolean(READY);
			askDialog = info.GetValue<AskDialog>(ASKDIALOG);
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(MESSAGE, message);
			info.AddValue(ERROR, error);
			info.AddValue(READY, ready);
			info.AddValue(ASKDIALOG, askDialog);
		}
	}

	//=========================================================================
	public class ReportController
	{
		private string stateMachineSessionTag;
		public const string StateMachineSessionTagIdentifier = "__StateMachineSessionTag";
		private string filename = "";
		private NameValueCollection requestParams;
		Guid hotLinkformKey;
		IWoormInfo woormInfo = null;
		internal RSEngine StateMachine = null;

		//--------------------------------------------------------------------------
		public ReportController() { }

		public IWoormInfo			WoormInfo				{ get { return woormInfo; } set { woormInfo = value; } }
		public NameValueCollection	RequestParams			{ get { return requestParams; } private set { requestParams = value; } }
		public string				StateMachineSessionTag	{ get { return stateMachineSessionTag; } }
		public Guid					HotLinkFormKey			{ get { return hotLinkformKey; } }
		public string				Filename				{ get { return filename; } set { filename = value; } }
		public Report				Report					{ get { return StateMachine.Report; } set { StateMachine.Report = value; } }
		public WoormDocument		Woorm					{ get { return StateMachine.Woorm; } set { StateMachine.Woorm = value; } }
		public string				ReportTitle				{ get { return StateMachine.ReportTitle; } }
		public string				LocalizedReportTitle	{ get { return Woorm.Localizer.Translate(ReportTitle); } }
		public TbReportSession		ReportSession			{ get { return StateMachine != null ? StateMachine.ReportSession : null; } }

		//--------------------------------------------------------------------------
		public static ReportController FromSession(NameValueCollection requestParams)
		{
			ReportController controller = FromSession(requestParams[StateMachineSessionTagIdentifier]);
			controller.RequestParams = requestParams;
			return controller;
		}

		//--------------------------------------------------------------------------
		public static ReportController FromSession(string tag)
		{
			if (string.IsNullOrEmpty(tag))
				tag = Guid.NewGuid().ToString();

			ReportController controller = TBWebContext.Current.FromSession(tag) as ReportController;
			if (controller == null)
			{
				controller = new ReportController();
				controller.stateMachineSessionTag = tag;
				TBWebContext.Current.ToSession(tag, controller);
			}
			return controller;
		}

		//--------------------------------------------------------------------------
		public void InitStateMachine(bool isPostBack)
		{
			// retrieves authentication data and sets the culture for current language
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
				ui.SetCulture();

			// Inits state machine containing WoormDocument and Report
			if (!isPostBack)
			{
				// if a state machine already exists due to previous run reports 
				// release memory in order to minimize resource consumption
				if (StateMachine != null)
					StateMachine.Dispose();

				// instances the reportSession of work (parameters has to be set)
				TbReportSession localReportSession = new TbReportSession(ui);
				bool sessionOk = localReportSession.LoadSessionInfo();

				ReadParameters(localReportSession);

				localReportSession.Localizer = new Microarea.TaskBuilderNet.Core.StringLoader.WoormLocalizer(Filename, localReportSession.PathFinder);

				// istanzio una nuova macchina per la elaborazione del report e la memorizzo in reportSession
				StateMachine = new RSEngine(localReportSession, Filename, TBWebContext.Current.SessionID, stateMachineSessionTag);
				if (StateMachine.Report != null)
					StateMachine.Report.WoormInfo = WoormInfo;
				// se ci sono stati errore nel caricamento fermo tutto (solo dopo aver istanziato la RSEngine)
				if (!sessionOk)
					StateMachine.CurrentState = State.LoadSessionError;

				// devo essere autenticato
				if (ui == null)
					StateMachine.CurrentState = State.AuthenticationError;

				// deve essere indicata anche la connection su cui si estraggono i dati
				if (ui != null && (ui.CompanyDbConnection == null || ui.CompanyDbConnection.Length == 0))
					StateMachine.CurrentState = State.ConnectionError;
			}

			if (isPostBack && StateMachine != null && StateMachine.ReportSession != null && StateMachine.ReportSession.UICulture != null)
			{
				System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(StateMachine.ReportSession.UICulture);
				StateMachine.ReportSession.Localizer.Build(StateMachine.ReportSession.ReportPath, StateMachine.ReportSession.PathFinder);
			}
		}

         const string _DocumentParametersControlName = "__DocumentInitParameters";

        // legge i parametri dalla stringa di comando se ci sono, posso anche
        // non essere autenticato se vengo usato dal localizer
        //--------------------------------------------------------------------------
        private void ReadParameters(TbReportSession session)
		{
			string filenameParam = requestParams[Helper.FileNameParam];
			if (filenameParam != null)
			{
				Filename = filenameParam;

				// Servono per le funzioni interne implementate da Expression.
				// Nel caso di run da snapshot non ho valorizzato il Namespace ma tanto non
				// eseguo nessuna valutazione di espressione.
				session.ReportPath = Filename;
				session.ReportNamespace = "";
				if (session.UserInfo != null && string.Compare(Path.GetExtension(Filename), NameSolverStrings.XmlExtension, true, CultureInfo.InvariantCulture) != 0)
				{
					INameSpace nsNamespace = session.UserInfo.PathFinder.GetNamespaceFromPath(Filename);
					if (nsNamespace != null)
						session.ReportNamespace = nsNamespace.FullNameSpace;
				}
				return;
			}

			// potrei avere in sessione il namespace del report.
			// Messo da selezione del TreeView di EasyLook che si era salvato il namespace del report selezionato
			// Il tutto è necessario per evitare il blocco dei popup usando la redirect.
			string namespaceParam = requestParams[Helper.NameSpaceParam];
			if (namespaceParam == null || namespaceParam == string.Empty)
			{
				string s = TBWebContext.Current.FromSession(SessionKey.ReportPath) as string;
				if (!String.IsNullOrEmpty(s))
					namespaceParam = s;
			}

			if (namespaceParam != null && namespaceParam != string.Empty)
			{
				if (session.UserInfo != null)
				{
					NameSpace nameSpace = new NameSpace(namespaceParam, NameSpaceObjectType.Report);
					Filename = session.UserInfo.PathFinder.GetFilename(nameSpace, session.UserInfo.LoginManager.PreferredLanguage);
				}

				// servono per le funzioni interne implementate da Expression
				session.ReportPath = Filename;
				session.ReportNamespace = namespaceParam;
			}

			string parametersParam = requestParams[_DocumentParametersControlName];
			if (parametersParam != null &&
				parametersParam != string.Empty &&
				namespaceParam != null &&
				namespaceParam != string.Empty)
			{
				session.ReportParameters = Helper.UnformatParametersFromRequest(parametersParam);
				TBWebContext.Current.ToSession(Helper.GetConnectionKey(namespaceParam, parametersParam), true); //if params come from linkreport, this is used to maintain the color of visited link
			}
		}

		//---------------------------------------------------------------------------
		internal void Print()
		{
			StateMachine.CurrentState = State.Print;
			StateMachine.Step();
		}

		//---------------------------------------------------------------------------
		internal bool ExecuteNoDataFromRadar()
		{
			if (StateMachine == null)
				return false;

			StateMachine.HtmlPage = HtmlPageType.Form;
			StateMachine.Step();
			return true;
		}

		//---------------------------------------------------------------------------
		internal bool ExecuteDataFromRadar(string fieldData)
		{
			if (StateMachine == null)
				return false;

			StateMachine.HtmlPage = HtmlPageType.Form;

			StateMachine.ActiveAskDialog.ActiveAskEntry.Field.Data = fieldData;
			StateMachine.ActiveAskDialog.UserChanged.Add(StateMachine.ActiveAskDialog.ActiveAskEntry.Field.Name);

			StateMachine.Step();
			return true;
		}

		//---------------------------------------------------------------------------
		internal HtmlPageType RenderingStep()
		{
			HtmlPageType renderingType = StateMachine.HtmlPage;
			switch (renderingType)
			{
				case HtmlPageType.Print:
					{
						//restore viewer page, so next round trip from the original 
						//page (without "Print" information in querystring) shows the report correctly
						StateMachine.HtmlPage = HtmlPageType.Viewer;
						break;
					}

				case HtmlPageType.HotLink:
					{
						const string formKeyIdentifier = "__HtmlPageType.HotLink.Key";
						if (StateMachine.PreviousHtmlPage == HtmlPageType.HotLink)
						{
							hotLinkformKey = (Guid)StateMachine.StateBag[formKeyIdentifier];
						}
						else
						{
							hotLinkformKey = Guid.NewGuid();
							StateMachine.StateBag[formKeyIdentifier] = hotLinkformKey;
						}
						break;
					}
				default:
					break;
			}

			StateMachine.PreviousHtmlPage = StateMachine.HtmlPage;
			return renderingType;
		}

		//---------------------------------------------------------------------------
		internal bool IsPageReady()
		{
			 return StateMachine != null && StateMachine.Woorm != null && StateMachine.Woorm.RdeReader != null && StateMachine.Woorm.RdeReader.IsPageReady();
		}

		//---------------------------------------------------------------------------
		internal void NextPage()
		{
			StateMachine.Woorm.LoadPage(PageType.Next);
		}

		//---------------------------------------------------------------------------
		internal void PrevPage()
		{
			StateMachine.Woorm.LoadPage(PageType.Prev);
		}

		//---------------------------------------------------------------------------
		internal void FirstPage()
		{
			StateMachine.Woorm.LoadPage(PageType.First);
		}

		//---------------------------------------------------------------------------
		internal void LastPage()
		{
			StateMachine.Woorm.LoadPage(PageType.Last);
		}
	}
}
