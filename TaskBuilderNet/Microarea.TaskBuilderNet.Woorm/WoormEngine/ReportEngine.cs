using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Woorm.ExpressionManager;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace Microarea.TaskBuilderNet.Woorm.WoormEngine
{

	///<summary>
	///Classe contenitore dei nomi e degli ID delle variabili speciali di report
	///ATTENZIONE: TENERE ALLINEATO CON "TaskBuilder\Framework\TbWoormEngine\RepTable.h"
	/// </summary>
	//==============================================================================
	public class SpecialReportField
	{
		//ID riservati da Woorm
		public const ushort REPORT_STATUS_ID			=	0x7FFe;
		public const ushort REPORT_OWNER_ID				=	0x7FFd;
		public const ushort REPORT_LAYOUT_ID			=	0x7FFc;
		public const ushort REPORT_PAGE_NUMBER_ID		=	0x7FFb;
        public const ushort REPORT_LINKED_ID            =   0x7FFa;
        public const ushort REPORT_ISPRINTING_ID        =   0x7FF9;
		public const ushort DEFAULT_ATTRIBUTE_ID		=	0x7FF8;
 		public const ushort REPORT_LASTPAGE_ID			=	0x7FF7;
        public const ushort REPORT_EABARCODE_ID	        =   0x7FF6;
        public const ushort REPORT_ISARCHIVING_ID       =   0x7FF5;
        public const ushort REPORT_FUNCTION_RETURN_VALUE =	0x7FF4;
        public const ushort REPORT_HIDE_ALL_ASK_DIALOGS  =  0x7FF3;
        //the latest used id
        public const ushort REPORT_LOWER_SPECIAL_ID     =   0x7FF3;

		//nomi delle variabili riservate
		public const string REPORT_DEFAULT_LAYOUT_NAME                      =   "default";
		public const string REPORT_SPECIAL_FIELD_NAME_STATUS				=	"ReportStatus";
		public const string REPORT_SPECIAL_FIELD_NAME_OWNER					=	"OwnerID";
		public const string REPORT_SPECIAL_FIELD_NAME_LAYOUT				=	"ReportLayout";
		public const string REPORT_SPECIAL_FIELD_NAME_CURRENT_PAGE_NUMBER	=	"ReportCurrentPageNumber";
        public const string REPORT_SPECIAL_FIELD_NAME_LASTPAGE              =   "ReportLastPageNumber";
		public const string REPORT_SPECIAL_FIELD_NAME_ISPRINTING			=	"ReportIsPrinting";
        public const string REPORT_SPECIAL_FIELD_NAME_ISARCHIVING            =  "ReportIsArchiving";
        public const string REPORT_SPECIAL_FIELD_NAME_USEDEFAULTATTRIBUTE   =   "UseDefaultAttribute";
        //public const string REPORT_SPECIAL_FIELD_NAME_EABARCODE             =   "ReportEABarCode";
        //public const string REPORT_SPECIAL_FIELD_NAME_LINKED_ID				    =   "LinkedDocumentID";
        //public const string REPORT_SPECIAL_FIELD_NAME_FUNCTION_RETURN_VALUE	    =   "_ReturnValue";
        public const string REPORT_SPECIAL_FIELD_NAME_HIDE_ALL_ASK_DIALOGS       =   "_HideAllAskDialogs";

	}

	//==============================================================================
	//
	// FieldStatusForTemporaryFile
	// Poiche` e` ammessa l'estrazione di dati NULLI (cioe` non risolti dalla Select),
	// nel caso di costruzione del temporaneo e` necessario aggiungere un campo fittizio
	// costituito da uno stream di byte (tanti quanti sono i campi del data record del
	// temporaneo) in cui l'i-esimo byte contiene '1' o '0' a seconda che l'i-esimo campo
	// del data record sia VALIDO o NULLO.
	// Questo permette in fase di retrieving di rileggere i dati convalidandoli in maniera
	// corretta ed inoltre raggruppandoli e/o sortandoli in maniera coerente.
	//
	//============================================================================
	public enum RuleReturn { Abort, Backtrack, Success}
	public enum DataLevel { Rules, GroupBy, Events }

    /// <summary>
	/// TableNames contiene i nomi con cui è conosciuta la tabella
	/// </summary>
	//============================================================================       
	public class TableNames
	{
		private string		tableName;
		private string		aliasName;
		private DataTable	schema = null;

		public	Diagnostic	Diagnostic = new Diagnostic("TableNames");


		//-----------------------------------------------------------------------------
		public TableNames(string tableName, string tableAlias, string connection, string provider) : base()
		{ 
			Debug.Assert(tableName.Length > 0);

			this.tableName = tableName;
			this.aliasName = (tableAlias.Length == 0) ? tableName : tableAlias; 

			CreateSchema(connection, provider);
		}

		// legge lo schema della tabella per poter istanziare gli oggetti giusti nelle espressioni
		//-----------------------------------------------------------------------------
		public void CreateSchema(string connectionString, string provider)
		{ 
			if (connectionString == null || connectionString == string.Empty)
				return;

			try
			{
				using (TBConnection connection = new TBConnection(connectionString, TBDatabaseType.GetDBMSType(provider)))
				{
					connection.Open();

					TBDatabaseSchema dbSchema = new TBDatabaseSchema(connection);
					schema = dbSchema.GetTableSchema(tableName, false);

					connection.Close();
				}
			}
			catch (TBException e)
			{
				Debug.Fail(string.Format(WoormEngineStrings.CreateScheraError, e.Message));
				Diagnostic.Set(DiagnosticType.Error, e.Message);
				schema = null;
			}
		}

		//-----------------------------------------------------------------------------
		public string TableName		{ get { return tableName; }}
		public string AliasName		{ get { return aliasName; }}

		//-----------------------------------------------------------------------------
		public string GetColumnType(string columnName)
		{
			// se non ha la connessione al database o se la colonna non esiste assume
			// che la colonna sia di tipo stringa.
			if (schema != null)
				foreach (DataRow row in schema.Rows)
				{
					if (string.Compare((string)row["ColumnName"], columnName, true, CultureInfo.InvariantCulture) == 0)
					{
						// nel caso contenga il Namespace completo ritorno il solo basename del tipo
						// Es: System.Int32 torna Int32
						string type = row["DataType"].ToString();
						return type.Replace("System.", "");
					}
				}

			return null;
		}
	}

	//============================================================================
	abstract public class RuleEngine
	{
		private string		publicName = "";
		private DataLevel	dataLevel = DataLevel.Rules;
		private Report		report;
		private Diagnostic	diagnostic = new Diagnostic("RuleEngine");

		private List<RuleObj> sortedRules = new List<RuleObj>();	// L'esecuzione si basa su queste
		private List<RuleObj> unsortedRules = new List<RuleObj>();	// Costruito durante il parsing

	
		private int		startPos = 0;
		private int		stopPos = 0;
		private int		lastElementPos = -1;
		private int		lastSubTreePos = 0;
		private int		totSubTreeNodes = 0;
		private int		unsortedRulesNumber = 0;
		private int		nullRulesNum = 0;
		private int		totalGroupBySegment = 0;
		private bool	userBreak = false;
		protected bool	groupByDefined = false;

        public double ColumnWidthPercentage = 0.9;
        public bool OptimizedLineBreak = true;

		#region properties
		//---------------------------------------------------------------------------
		public int			NullRulesNum	{ get { return nullRulesNum;} set { nullRulesNum = value; }}	
		public RepSymTable	RepSymTable		{ get { return report.SymTable; }}
		public List<RuleObj> SortedRules { get { return sortedRules; } }

		//---------------------------------------------------------------------------
		public string	PublicName { get { return publicName; } set { publicName = value; }}
		public int		InternalId { get { return 0; }}

		//---------------------------------------------------------------------------
		public Report		Report		{ get { return report; }}
		public bool			UserBreak	{ get { return userBreak; } set { userBreak = value; }}
		public DataLevel	DataLevel 	{ get { return dataLevel; } set { dataLevel = value; }}
		public TbReportSession		Session		{ get { return Report.ReportSession; }}
		public Diagnostic	Diagnostic	{ get { return diagnostic; }}
		#endregion

		abstract protected bool	ProcessTuple	();

		//-----------------------------------------------------------------------------
		public RuleEngine (Report ownerReport)
		{
			report	= ownerReport;

            object setting1 = ReadSetting.GetSettings(Session.PathFinder, "Framework.TbWoormViewer.Woorm", "WoormGeneralOptions", "OptimizedLineBreak", true);
            this.OptimizedLineBreak = ObjectHelper.CastBool(setting1);

            object setting2 = ReadSetting.GetSettings(Session.PathFinder, "Framework.TbWoormViewer.Woorm", "WoormGeneralOptions", "ColumnWidthPercentage", 90);
			double columnPerc;
			if (double.TryParse(setting2 as string, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out columnPerc))
			{
				this.ColumnWidthPercentage = columnPerc / 100.0;
			}
		}

		//-----------------------------------------------------------------------------
		public void SetLevel(DataLevel level)
		{  
			dataLevel = level;
		}

		// blocca anche l'eventuale parser globale del report
		//-----------------------------------------------------------------------------
		public bool SetError(string errorText)
		{
			diagnostic.Set(DiagnosticType.Error, errorText);
			return false;
		}

		//-----------------------------------------------------------------------------
		public bool SetError(Diagnostic diagnostic, string errorText)
		{
			if (diagnostic.Error)
				this.diagnostic.Set(DiagnosticType.Error, diagnostic);
			
			return SetError(errorText);
		}

		//-----------------------------------------------------------------------------
		public bool SetError(Diagnostic diagnostic)
		{
			if (diagnostic.Error)
				this.diagnostic.Set(DiagnosticType.Error, diagnostic);

			return false;
		}

		//-----------------------------------------------------------------------------
		public void ResetError()
		{
			diagnostic = new Diagnostic("RuleEngine");
		}

		//-----------------------------------------------------------------------------
		public void StopEngine(string text)
		{
			userBreak = true;
            if (text.Length > 0)
			    diagnostic.Set(DiagnosticType.Warning, text);
		}

		//-----------------------------------------------------------------------------
		public void AddRule(RuleObj rule)
		{
			unsortedRules.Add(rule);
		}

		//-----------------------------------------------------------------------------
		public bool BuildTree(Parser lex, GroupBy groupByExp)
		{
			ResetError();

			unsortedRulesNumber = unsortedRules.Count;
			if (unsortedRulesNumber == 0)
				return true;

			// costruisce relazioni padre-figli
			BuildLinks();

			// controlla se ci sono rule che generano ricorsione
			if (!CheckRecursion())
			{
				lex.SetError(WoormEngineStrings.RecursiveRule);
				return false;
			}

			// controlla se la GroupBy è valida (vedi commento in GroupBy)
			if (groupByDefined)
			{
				ArrayList joinedRules = new ArrayList();
				if	(groupByExp.IsTemporaryRequired(ref joinedRules))
					return SetError(WoormEngineStrings.BadGroupBy);

				FindSubTree(ref joinedRules);
			}

			// builds a sub-tree moving the rules from the unsorted array to the sorted array.
			lastSubTreePos = BuildSubTree();

			return true;
		}

		/// <summary>
		///	Costruisce le relazioni padre e figlio tra tutte le regole
		///  esempio: 
		///		 b = 0 
		///		 a = b + 1
		///		
		///		allora a dipende da b
		/// </summary>
		//-----------------------------------------------------------------------------
		void BuildLinks ()
		{
			// it's used to break the rule moving if an order-by criterion exists
			//
			totSubTreeNodes = 0;

			int p,s;

			// inizializzazione (utile se si dovesse riabilitare la gestione su file temporaneo)
			int nr = unsortedRulesNumber;
			for (p = 0; nr > 0; p++)
				if (unsortedRules[p] != null)
				{
					nr--;
					RuleObj pRule = (RuleObj) unsortedRules[p];
					pRule.Sons = new ArrayList();
					pRule.Parents = new ArrayList();
					
					pRule.ParentsNum	= 0;
					pRule.RuleId		= p;
					pRule.Mark			= false;
					pRule.GroupByPos	= -1;
				}

			int np = unsortedRulesNumber;
			for (p = 0; np > 0; p++)
				if (unsortedRules[p] != null)
				{
					np--;
					RuleObj pParRule = (RuleObj) unsortedRules[p];

					int ns = unsortedRulesNumber;
					for (s = 0; ns > 0; s++)
						if (unsortedRules[s] != null)
						{
							ns--;
							if (p != s)
							{
								RuleObj pSonRule = (RuleObj) unsortedRules[s];
								if (pParRule.IsParentOf(pSonRule))
								{
									pParRule.Sons.Add(pSonRule);
									pSonRule.Parents.Add(pParRule);
									pSonRule.ParentsNum++;
								}
							}
						}
				}
		}

		//-----------------------------------------------------------------------------
		bool CheckRecursion ()
		{
			int nr = unsortedRulesNumber;
			for (int p = 0; nr > 0; p++)
				if (unsortedRules[p] != null)
				{
					nr--;
					if (!CheckDescendents((RuleObj)unsortedRules[p]))
						return false;
				}

			return true;
		}

		//-----------------------------------------------------------------------------
		bool CheckDescendents (RuleObj parentRule)
		{
			// tests if parentRule was already encountered
			if (parentRule.Mark) return false;

			// this is a local use of Mark data member to test as above the node tracking
			if (parentRule.Sons.Count > 0)
			{
				parentRule.Mark = true;

				foreach (RuleObj ro in parentRule.Sons)
					if (!CheckDescendents(ro))
						return false;

				// reset the initial value
				parentRule.Mark = false;
			}

			return true;
		}

		//-----------------------------------------------------------------------------
		bool FindSubTree (ref ArrayList joinedRules)
		{
			totalGroupBySegment = joinedRules.Count;

			if (totalGroupBySegment == 0) return true;		// empty order by

			for (int pos = 0; pos < totalGroupBySegment; pos++)
			{
				RuleObj aSegRule = (RuleObj)joinedRules[pos];

				if (aSegRule.GroupByPos == -1)
				{
					aSegRule.GroupByPos = pos;
					MakeSubTreeNodes(aSegRule);
				}
				else throw (new Exception(WoormEngineStrings.UnsupportedRulesError));
			}
			return true;
		}

		//-----------------------------------------------------------------------------
		void MakeSubTreeNodes (RuleObj sonRule)
		{
			if (!sonRule.Mark)
			{
				// signal to MoveRule function that this rule is a sub-tree-node
				//
				sonRule.Mark = true;
				totSubTreeNodes++;
			}

			foreach (RuleObj parentRule in sonRule.Parents)
				MakeSubTreeNodes(parentRule);
		}

		// move the rules to an array that will contain the rules sorted according to
		// the tree traversing
		//-----------------------------------------------------------------------------
		int BuildSubTree ()
		{
			int nr;

			bool	continueScans	= true;
			bool	ruleMoved	= true;
			int		currPos	= 0;

			for(;;)
			{
				while(((nr = unsortedRulesNumber) > 0) && continueScans && ruleMoved)
				{
					ruleMoved = false;
					for (int p = 0; nr > 0 && continueScans && !ruleMoved; p++)
						if (unsortedRules[p] != null)
						{
							nr--;
							MoveRule
								(
								(RuleObj) unsortedRules[p],
								ref continueScans,
								ref ruleMoved,
								ref currPos
								);
						}
				}

				break;
			}

			return lastElementPos;
		}

		// muove ciascuna rule in accordo ad eventuali criteri di group-by/order-by
		//-----------------------------------------------------------------------------
		void MoveRule (RuleObj parentRule, ref bool continueScans, ref bool ruleMoved, ref int currPos)
		{
			if (parentRule.ParentsNum != 0) return;
			if (groupByDefined)
			{
				if (currPos < totalGroupBySegment)
					if (parentRule.GroupByPos != currPos)
						return;
					else
						currPos++;

				if (!groupByDefined && totSubTreeNodes > 0)
					if (!parentRule.Mark)
						return;
					else
						if ((--totSubTreeNodes == 0) && false /*&& m_EngineStatus[TEMP]*/) continueScans = false;
			}

			// At this point we must remove the actual parentRule from unsorted array to insert it into
			// sorted array, but to do that we leave a hole in the unsorted array, first because it
			// is owner of stored objects and, second, if we need further pass on this array the index
			// position stored in the others rules it's relative to the actual array state.

			unsortedRules[parentRule.RuleId] = null;
			unsortedRulesNumber--;

			lastElementPos++;
			parentRule.RuleId = lastElementPos;
			sortedRules.Add(parentRule);
			parentRule.Sorted = true;

			ruleMoved = true;

			foreach (RuleObj sonRule in parentRule.Sons)
			{
				sonRule.ParentsNum--;
				if (continueScans)
					MoveRule(sonRule, ref continueScans, ref ruleMoved, ref currPos);
			}
		}

		//-----------------------------------------------------------------------------
		protected RuleReturn ApplyRules ()
		{
			ResetError();
			nullRulesNum = 0;
			startPos = 0;
			stopPos = lastElementPos;

			SetLevel(DataLevel.Rules);
			RuleReturn result = ApplyRuleFrom(startPos);
            
            // chiamo la dispose sulle rule per rilasciare le risorse (Es. chiudere le connessioni aperte)
            foreach (RuleObj theRule in sortedRules)
                theRule.Close();

			return result;
		}

		//-----------------------------------------------------------------------------
		public RuleReturn ApplyRuleFrom(int fromRule)
		{
			if (UserBreak) return RuleReturn.Abort;

			ResetError();

			// solo quando arrivo all'ultima rule applico lo spostamento dei dati
			// cioè in cascata tutte le azioni successive alla fecth eseguita dalle rule
			if (fromRule > stopPos)
			{
				if (!ProcessTuple())
				{
					SetError(WoormEngineStrings.BadCopyData);
					return RuleReturn.Abort;
				}

				SetLevel(DataLevel.Rules);

				return RuleReturn.Success;
			}

			// cerco la prossima rule da eseguire e se non la trovo mi arrabbio
			RuleObj theRule = (RuleObj) sortedRules[fromRule];
			if (theRule == null)
			{
				SetError(WoormEngineStrings.NoRule);
				return RuleReturn.Abort;
			}

			return theRule.Apply();
		}
	}

	/// <summary>
	/// ReportEngine
	/// </summary>
	//============================================================================
	public class ReportEngine : RuleEngine
	{
		public enum ReportStatus { Init, Before, FirstRow, Body, LastRow }

		// Events di segnalazione avanzamento attività
		public event EventHandler RetrieveTic;

		public event EventHandler BeginGroupBy;
		public event EventHandler GroupByTic;
		public event EventHandler EndGroupBy;

		private int			itemStart;
		private bool		firstTuple;
		private GroupBy		groupByExp = null;
		private Expression	havingExp = null;

		private RdeWriter		outChannel = null;
		private EventActions	onFormFeedActions;
		private ReportEventActions	reportActions;
		private List<TriggeredEventActions> triggeredEvents = new List<TriggeredEventActions>();
		private FormFeedAction	autoFormFeed;
		private ReportStatus	status = ReportStatus.Init;

		private List<AskDialog> askingRules = new List<AskDialog>();

		private int				currentAskDialogNo = -1;

		public bool		DisplayAction;
		public bool		ExplicitDisplayNotAllowed;
		public bool		OnFormFeedAction;
		public bool		OnTableAction;

		// contiene se esiste la dialog che il motore di renderizzazione deve presentare
		//----------------------------------------------------------------------------
		public AskDialog CurrentAskDialog 
		{ 
			get 
			{ 
				return (currentAskDialogNo < 0)
					? null 
					: (AskDialog)askingRules[currentAskDialogNo]; 
			}
		}

		//---------------------------------------------------------------------------
		public	EventActions	OnFFActions	{ get { return onFormFeedActions; }}
		public	ReportStatus	Status		{ get { return status; } set { status = value; }}
		public  List<AskDialog> AskingRules { get { return askingRules; } }
		public	RdeWriter		OutChannel	{ get { return outChannel;} set { outChannel = value; }}

		//---------------------------------------------------------------------------
		public ReportEngine (Report ownerReport, EngineType engineType) : base(ownerReport)
		{
			//se sono in modalita xml richesto da Magic Link, il canale di output e' l'xmlwriter, altrimenti l'rdewriter che scrive i dati
			//paginati
			this.outChannel	= (engineType == EngineType.OfficeXML) ? new XmlWriter(ownerReport) : new RdeWriter(ownerReport);
			
			onFormFeedActions	= null;
			reportActions		= null;
			autoFormFeed		= null;
		}

		//---------------------------------------------------------------------------
		private void CopyRuleDataToGroupData()
		{
			foreach (Field field in RepSymTable.Fields)
				field.UpdateGroupByData();
		}

		//---------------------------------------------------------------------------
		private void CopyGroupDataToEventData()
		{
			foreach (Field field in RepSymTable.Fields)
				field.UpdateEventData();
		}

		// serve a filtrare la tuple correntemente elaborata se è valida la HAVING
		//---------------------------------------------------------------------------
		private bool HavingFilter(out bool skipTuple)
		{
			skipTuple = false;

			if (havingExp != null && !havingExp.IsEmpty)
			{
				Value v = havingExp.Eval();
				if (havingExp.Error)
					return SetError(havingExp.Diagnostic);

				skipTuple = !((bool)v.Data);
			}

			if (RetrieveTic != null) RetrieveTic(this, new EventArgs());
			return true;
		}

		//---------------------------------------------------------------------------
		protected bool FireEvents()
		{
			bool skipTuple = false;
			if (!HavingFilter(out skipTuple))
				return SetError(WoormEngineStrings.EvalHaving);

			if (skipTuple)
				return true;

			SetLevel(DataLevel.Events);

			foreach (TriggeredEventActions te in triggeredEvents)
				if (!te.Check())
					return false;

			if (Status != ReportEngine.ReportStatus.FirstRow)
				if (autoFormFeed == null)
				{
					foreach (TriggeredEventActions te in triggeredEvents)
						if (!te.DoBeforeActions())
							return false;
				}
				else	
					if (!autoFormFeed.Exec())
						return false;

			status = ReportEngine.ReportStatus.Body;

			CopyGroupDataToEventData();

			foreach (TriggeredEventActions te in triggeredEvents)
				if (!te.DoAfterActions())
					return false;

			if (reportActions != null && !reportActions.AlwaysActions.Exec())
				return false;

			if (UserBreak)
				return false;
			
			// reset current data level
			SetLevel(DataLevel.GroupBy);
			return true;
		}

		//---------------------------------------------------------------------------
		private bool ExecuteGroupBy()
		{
			if (firstTuple)
			{
				firstTuple = false;

				if (!groupByExp.Init(this))
					return false;

				CopyRuleDataToGroupData();
				if (!groupByExp.EvalFunction(true))
					return SetError(groupByExp.Diagnostic);
			}
			else if (groupByExp.IsChanged())
			{
				if (!FireEvents())
					return false;

				CopyRuleDataToGroupData();
				if (!groupByExp.EvalFunction(true))
					return SetError(groupByExp.Diagnostic);
			}
			else if (Diagnostic.Error || !groupByExp.EvalFunction())
					return SetError(groupByExp.Diagnostic);

			if (GroupByTic != null) 
				GroupByTic(this, new EventArgs());

			return true;
		}

		//---------------------------------------------------------------------------
		protected override bool ProcessTuple()
		{
			SetLevel(DataLevel.GroupBy);

			if (groupByDefined)
			{
				if (!ExecuteGroupBy())
					return false;
			}
			else
			{
				CopyRuleDataToGroupData();
				if (!FireEvents())
					return false;
			}

			// a finito di processare tutta una tupla (rowset equivalente di tutte le rules)
			// e quindi inizializza tutti field che appartengono ad una rule per prepararli alla
			// prossima elaborazione
			// evita a GroupBy ed Event di tener conto dei valori contenuti in RuleData
			foreach (Field rf in RepSymTable.Fields)
				if (rf.OwnRule)	rf.RuleDataFetched = false;

			return !UserBreak;
		}

		// per Tupla si intende il RowSet equivalente a tutte le select di tutte le rules
		// scandite secondo l'ordine delle sortedrules
		//---------------------------------------------------------------------------
		public RuleReturn ExecuteRules ()
		{
			if (groupByDefined && BeginGroupBy != null) BeginGroupBy(this, new EventArgs());

			firstTuple = true;
			RuleReturn status = ApplyRules();

			// serve a gestire l'ultima tupla che ho ancora in canna e mi serve
			// solo se devo fare un groupby finale
			if (status == RuleReturn.Success && groupByDefined)
			{
				SetLevel(DataLevel.GroupBy);

				status = FireEvents() ? RuleReturn.Success : RuleReturn.Abort;
				if (EndGroupBy != null) EndGroupBy(this, new EventArgs());
			}
			SetLevel(DataLevel.Rules);

			if (groupByDefined && EndGroupBy != null) EndGroupBy(this, new EventArgs());
			return status;
		}

		//---------------------------------------------------------------------------
		private bool ParseQueryRule(Parser lex)
		{
			if (lex.Parsed(Token.RULES))
				do
				{
					switch (lex.LookAhead())
					{
						case Token.FROM :
						{
							DataTableRule dataTableRule = new DataTableRule(this);
							AddRule(dataTableRule);
							if (!dataTableRule.Parse(lex)) return false;
							break;
						}
			
						case Token.IF   :
						{
							ConditionalRule condRule = new ConditionalRule(this);
							AddRule(condRule);
							if (!condRule.Parse(lex)) return false;
							break;
						}
			
						case Token.ID   :
						{
							ExpressionRule expRule = new ExpressionRule(this);
							AddRule(expRule);
							if (!expRule.Parse(lex)) return false;
							break;
						}

						case Token.QUERY   :
						{
							QueryRule qr = new QueryRule(this);
							AddRule(qr);
							if (!qr.Parse(lex)) return false;
							break;
						}
						case Token.WHILE   :
						{
							WhileRule wr = new WhileRule(this);
							AddRule(wr);
							if (!wr.Parse(lex)) return false;
							break;
						}
			
						default : 
						{
							lex.SetError(WoormEngineStrings.RuleNotFound);
							return false;
						}
					}
				}
				while (!lex.Parsed(Token.END) && !lex.Error);

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		private bool ParseGroupBy(Parser lex)
		{
			if (groupByDefined)
			{
				lex.SetError(WoormEngineStrings.GroupByExist);
				return false;
			}

			if (!lex.ParseTag(Token.BY))
				return false;

			groupByExp = new GroupBy(Session, RepSymTable.Fields);
			
			if (!groupByExp.Parse(lex))
				return false;

			groupByDefined = true;
		    
			return true;
		}

		//---------------------------------------------------------------------------
		private bool ParseHaving(Parser lex)
		{
			if (havingExp != null)
			{
				lex.SetError(WoormEngineStrings.HavingExist);
				return false;
			}
			    
			havingExp = new Expression(Session, RepSymTable.Fields);
			
			if (!(havingExp.Compile(lex, CheckResultType.Match, "Boolean")))
				return false;

			return lex.ParseSep();
		}


		//---------------------------------------------------------------------------
		public bool ParseRules(Parser lex)
		{
			if	(
				!ParseQueryRule(lex)                                ||
				(lex.Parsed(Token.GROUP) && !ParseGroupBy(lex))     ||
				(lex.Parsed(Token.HAVING) && !ParseHaving(lex))		||
				lex.Error
				)
				return false;

			return BuildTree(lex, groupByExp);
		}

		//---------------------------------------------------------------------------
		public bool UnparseRules(Unparser unparser)
		{
			if (SortedRules.Count > 0)
			{
				unparser.WriteTag(Token.RULES);
				unparser.IncTab();

				foreach (RuleObj item in SortedRules)
					item.Unparse(unparser);

				unparser.DecTab();
				unparser.WriteEnd(true);
				
				unparser.DecTab();

				unparser.WriteLine();
			}

			if (groupByExp != null)
				groupByExp.Unparse(unparser);

			unparser.IncTab();

			if (havingExp != null && !havingExp.IsEmpty) 
			{
				unparser.WriteTag(Token.HAVING, false);
				unparser.WriteExpr(havingExp.ToString(), false);
				unparser.WriteSep(true);
			}
			unparser.DecTab();

			return true;
		}

		//---------------------------------------------------------------------------
		public AskDialog GetAskDialog(string name) 
		{
			foreach (AskDialog askDialog in askingRules)
				if (string.Compare(name, askDialog.FormName, true, CultureInfo.InvariantCulture) == 0)
					return askDialog;
			
			return null;
		}

		// inizializza indistintamente tutti i dati dei field nella tabella dei simboli
		// fare attenzione perche usando una Hastable la posizione è in ordine inversa 
		// a come vengono aggiunti e si recuperano i posti vuoti in caso di cancellazione
		// o aggiunta di elementi. Utilizzo allora la tecnica di inizializzarli dal fondo
		// utilizzando un vettore di appoggio. In alternativa potrei cercare di costruire
		// un grafo di dipendenza delle espressioni di inizializzazione ed eseguirle
		// sulla base del risultato
		//---------------------------------------------------------------------------
        public bool ExecuteInitialize(ParametersList initParameters)
		{
			Diagnostic.Clear();
			SetLevel(DataLevel.Events);
			Parameter par = null;
			object parObj = null;
			foreach (Field field in RepSymTable.Fields)
			{
				if (initParameters != null && (par = initParameters[field.PublicName]) != null)
				{
					try
					{
						object o = SoapTypes.From(par.ValueString, field.DataType);
						field.SetAllData(o, true);
						
					}
					catch (Exception ex)
					{
						SetError(string.Format(WoormEngineStrings.EvalInitExpression, field.PublicName));
						return SetError(ex.Message);
					}
				}
				else if (Report.WoormInfo != null && (parObj = Report.WoormInfo.GetInputParamValue(field.PublicName)) != null)
				{
					field.SetAllData(parObj, true);
				}
				else
				{
					if (!field.Init())
						return SetError(string.Format(WoormEngineStrings.EvalInitExpression, field.PublicName));
				}
					
			}
			if (initParameters != null)
				foreach (AskDialog askDialog in askingRules)
					askDialog.UserChanged.AddRange(initParameters.ParamNames);
			if (Report.WoormInfo != null)
				foreach (AskDialog askDialog in askingRules)
					askDialog.UserChanged.AddRange(Report.WoormInfo.GetInputParamNames());
			return true;
		}
		
		// valorizza i parametri delle Ask con i dati provenienti dal dom passato dal chiamante
		// nel caso di errore soap o di conversione di formato nella conversione di tipo non 
		// assegno il valore ma vado avanti utilizzando il valore di default del field
		//---------------------------------------------------------------------------
		public bool ExecuteLoadParamters()
		{
			string namespaceURI = Report.XmlDomParameters.DocumentElement.NamespaceURI;
			foreach (Field field in RepSymTable.Fields)
				if (field.Input)
				{
					try
					{
						XmlNodeList elemList = Report.XmlDomParameters.GetElementsByTagName(field.PublicName, namespaceURI);
						if (elemList == null) continue;

						for (int i = 0; i < elemList.Count; i++)
						{   
							field.AskData = SoapTypes.From(elemList[i].InnerXml, field.DataType);
							if (field.InputLimit == Token.UPPER_LIMIT)
							{
								if (field.DataType.ContainsNoCase("String"))
								{
									field.AskData = ObjectHelper.GetMaxString(field.AskData as String, Report.MaxString, field.Len);
								}
							}
						}
					}
					catch (FormatException) 
					{}
					catch (SoapClientException) 
					{}
				}

			return true;
		}


		// eseguo in modalita Modeless tutte le AskDialog che non sono collegata
		// ad eventi di report OnAsk (chiamata di dialog da eventi di report)
		//---------------------------------------------------------------------------
        ArrayList initializedFields = new ArrayList();

		public bool ExecuteAsk()
		{
			// La ExecuteLastAskPostProcess deve essere eseguita dopo aver mostrato la dialog
			// e quindi la eseguo prima della successiva dialog. Se non c'è dialog
			// successiva la eseguo lo stesso e poi passo ad eseguire il report
			if (!ExecuteLastAskPostProcess())
				return false;

 			for (int i  = (currentAskDialogNo + 1); i < askingRules.Count; i++)
			{
				AskDialog askDialog = (AskDialog)askingRules[i];
				
				// le dialog ON ASK non devono essere eseguite adesso ma su chiamata da codice
				if (askDialog.OnAsk) 
					continue;

				if (askDialog.BeforeActions != null && !askDialog.BeforeActions.Exec())
					return false;

                Variable varHideAllAskDialogs = this.RepSymTable.Fields.FindById(SpecialReportField.REPORT_HIDE_ALL_ASK_DIALOGS);
                if (varHideAllAskDialogs != null && varHideAllAskDialogs.Data != null && ((bool)varHideAllAskDialogs.Data))
                {
                    askDialog.EvalAllInitExpression(initializedFields, true);
                    askDialog.AssignUppeLowerLimit(initializedFields, true);
                    continue;
                }
				
                // se esiste una espressione di when allora potrei anche non dover
				// eseguire la dialog nel caso sia false. Se ho un errore di valutazione
				// allora interrompo tutto.
				if (askDialog.WhenExpr != null)
				{
					Value v = askDialog.WhenExpr.Eval();
					if (askDialog.WhenExpr.Error)
						return SetError(askDialog.WhenExpr.Diagnostic);

                    if (!(bool)v.Data)
                    {
                        askDialog.EvalAllInitExpression(initializedFields, true);
                        askDialog.AssignUppeLowerLimit(initializedFields, true);
                        continue;
                    }
				}

				// Prima di passare alla richiesta dei dati è necessario 
				// rivalutare eventuali askEntry che hanno espressioni
				// di valutazione dipendenti da altri campi di altre dialog
                askDialog.EvalAllInitExpression(initializedFields, false);

				currentAskDialogNo = i;
				return true;
			}

			// permette alla macchina a stati di riprendere l'esecuzione del prossimo step
			currentAskDialogNo = -1;
			return true;
		}

		// Esegue le clausole di On e di After della dialog appena processata
		//---------------------------------------------------------------------------
		private bool ExecuteLastAskPostProcess()
		{
            if (currentAskDialogNo < 0)
            {
                initializedFields.Clear();
                return true;
            }
			AskDialog askDialog = (AskDialog)askingRules[currentAskDialogNo];
				
            askDialog.AssignUppeLowerLimit(null, false);
		
			// evito di fare adesso il post processing delle dialog ON ASK
			if (askDialog.OnAsk) return true;

			// esegue le istruzione del blocco AFTER
			if (askDialog.AfterActions != null && !askDialog.AfterActions.Exec())
				return false;

			// se esiste una espressione di ON allora  fermo il motore e
			// presento il messaggio indicato dal report
			if (askDialog.OnExpr != null)
			{
				Value v = askDialog.OnExpr.Eval();
				if (askDialog.OnExpr.Error)
					return SetError(askDialog.OnExpr.Diagnostic);

				if ((bool)v.Data)
				{
					Value message = askDialog.OnMessage.Eval();
					if (askDialog.OnMessage.Error)
						return SetError(askDialog.OnMessage.Diagnostic);

					StopEngine((string) message.Data);
					return false;
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public void ExecuteAfterAsk()
		{
            //foreach (AskDialog askDialog in askingRules)
            //    askDialog.AssignUppeLowerLimit();
			
			this.RepSymTable.SaveAskDialogFieldsState();
		}

		// tutto ok perche ho valorizzato tutti i campi delle ask (se ci sono).
		// devo completare il preprocessing dell'ultima dialog presentata e dopo
		// parte il report e per prima cosa faccio tutte le azioni prima del report
		//---------------------------------------------------------------------------
		public bool ExecuteBeforeActions()
		{
            ReportEngine.ReportStatus rs = this.Status;
            this.Status = ReportEngine.ReportStatus.Before;

 			bool b = (reportActions == null || reportActions.BeforeActions.Exec());

            this.Status = rs;

            return b;
		}

		//---------------------------------------------------------------------------
		public RuleReturn ExecuteRulesAndEvents()
		{
			Field f = RepSymTable.Fields.Find(SpecialReportField.REPORT_SPECIAL_FIELD_NAME_CURRENT_PAGE_NUMBER);
			if (f != null)
			{
				f.SetAllData(OutChannel.PageNumber, true);
			}

			// la prima riga server per inibire la BeforeAction di ogni evento all'inizio
			// del report. Ovviamente posso solo avere AfterAction
			status = ReportEngine.ReportStatus.FirstRow;

			// parto con la esecuzione di tutte le rules (che ovviamente scatenano eventi)
			RuleReturn ruleReturn = ExecuteRules();
			if (ruleReturn != RuleReturn.Success)
				return ruleReturn;
		
			// dopo aver eseguito l'estrazione dell'ultima iterazione delle rules posso 
			// eseguire solo le BeforeAction per tutti gli eventi che possono verificarsi.
			// Quindi mi serve sapere che sono sull'ultima riga
			status = ReportEngine.ReportStatus.LastRow;

			// alzo il flag agli eventi che devono essere scatenati sulla base delle condizioni attuali
			foreach (TriggeredEventActions te in triggeredEvents)
				if (!te.Check(false))
					return RuleReturn.Abort;

			SetLevel(DataLevel.Events);

			// eseguo gli eventi che ho abilitato. Devo farlo separatamente al check per evitare che
			// side effect tra valutazioni successive alterassero la sequenza degli eventi.
			foreach (TriggeredEventActions te in triggeredEvents)
				if (!te.DoBeforeActions())
					return RuleReturn.Abort;

			return RuleReturn.Success;
		}

		//---------------------------------------------------------------------------
		public bool ExecuteAfterActions()
		{
			// Le azioni che sono di After per l'intero report (da eseguire alla fine)
			// fanno logicamente parte del BODY
			status = ReportEngine.ReportStatus.Body;	

			// Eseguo le azioni finali come parte del BODY
			if (reportActions !=  null && !reportActions.AfterActions.Exec())
				return false;

			return true;
		}

		// per lo stato svedi sopra
		//---------------------------------------------------------------------------
		public bool ExecuteFinalizeActions()
		{
			status = ReportEngine.ReportStatus.Body;	
			if (reportActions !=  null && !reportActions.FinalizeActions.Exec())
				return false;

			return true;
		}

		//---------------------------------------------------------------------------
		private bool ParseAskingRules(Parser lex)
		{
			if (!lex.Parsed(Token.DIALOGS))
				return true;
			
			lex.ParseBegin();
			while (lex.Parsed(Token.DIALOG))
			{
				AskDialog askDialog = new AskDialog(Report);
				if (!askDialog.Parse(lex))
					return false;

  
				if (GetAskDialog(askDialog.FormName) != null)
				{
					string strAdvisedName = "NoName";
					int i = 0;
					while(GetAskDialog(strAdvisedName) != null)
						strAdvisedName = String.Format("NoName{0}" , ++i);
					
					askDialog.FormName = strAdvisedName; 				
				}
				
				askingRules.Add(askDialog);
			}
			lex.ParseEnd();

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		private bool ParseStdActions(Parser lex)
		{
			if (lex.Parsed(Token.FORMFEED))
			{                             
				if (!lex.ParseTag(Token.COLON) || !lex.ParseTag(Token.DO)) return false;
			
				DisplayAction = false;
				OnFormFeedAction = true;

				onFormFeedActions = new EventActions(this);
				if (!onFormFeedActions.ParseEventActions(lex))
					return false;

				OnFormFeedAction  = false;
			}

			if  (lex.Parsed(Token.REPORT))
			{
				if (!lex.ParseTag(Token.COLON) || !lex.ParseTag(Token.DO)) return false;
			
				DisplayAction = false;

				reportActions   = new ReportEventActions(this);
				if (!reportActions.Parse(lex))
					return false;
			}

			while (lex.Parsed(Token.TABLE))
			{
				DisplayAction	= false;
				OnTableAction	= true;
		        
				// match display table name
				// if no name Parsed must exist only and only one display table
				DisplayTable displayTable = ParseDisplayTable(lex);
			    
				if (displayTable == null) return false;
				if (displayTable.TableActions != null)
				{
					lex.SetError(string.Format(WoormEngineStrings.TableActionExist, displayTable.PublicName));
					return false;
				}

				if (!lex.ParseTag(Token.COLON) || !lex.ParseTag(Token.DO)) return false;
			
				displayTable.TableActions = new EventActions(this);
				if (!displayTable.TableActions.ParseEventActions(lex)) return false;

				OnTableAction	= false;	
			}

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		private bool ParseTriggeredEvents(Parser lex)
		{
			DisplayAction = false;
			string name = string.Empty;

			while (lex.LookAhead(Token.ID))
			{
				if (!lex.ParseID(out name))
					return false;

				if (!lex.ParseTag(Token.COLON))
					return false;

				TriggeredEventActions pTrigEvent = new TriggeredEventActions(this);
				pTrigEvent.Name = name;
				triggeredEvents.Add(pTrigEvent);

				if (!pTrigEvent.Parse(lex)) return false;
			}

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		private bool ParseEvents(Parser lex)
		{
			if (lex.Parsed(Token.EVENTS))
			{
				if 	(
					ParseStdActions(lex) &&
					ParseTriggeredEvents(lex)
					)
					return lex.ParseEnd();  // EVENTS END

				return false;
			}
			
			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		private bool MakeAutoDisplayActions(Parser lex)
		{
			// insert eventually display field actions
			DisplayFieldsAction alwaysDFA = null;
			DisplayFieldsAction onFormFeedDFA = null;
			DisplayFieldsAction onEndOfReportDFA = null;

			foreach (Field field in RepSymTable.Fields)
			{
				if	(
						field.Displayed	||
						field.Hidden		||
						field.IsSubTotal
					) 
					continue;	
                              
				// ColTotals have column attribute!!
				if (!field.IsColumn || field.IsColTotal)
				{
					// build Report event if it don't exist yet
					if (reportActions == null)
						reportActions = new ReportEventActions(this);

					// build Form Feed event if it don't exist yet
					if (onFormFeedActions == null)
						onFormFeedActions = new EventActions(this);

					// add actions as below:
					//
					//	FormFeed : Do
					//		Before Begin
					//			Display field_not_col_1, field_not_col_2, .. field_not_col_N;
					//			... USER ACTIONS ...
					//		End
					//
					//	Report : Do
					//		After Begin
					//			... USER ACTIONS ...
					//			Display field_not_col_1, field_not_col_2, .. field_not_col_N;
					//		End
					//
					if (onFormFeedDFA == null && onEndOfReportDFA == null)
					{
						// insert as first action the automatic field display actions
						//
						onFormFeedDFA = new DisplayFieldsAction(this, null);
						onFormFeedActions.BeforeActions.InsertActionAt(0, onFormFeedDFA);

						// insert as last action the automatic field display actions
						//
                        onEndOfReportDFA = new DisplayFieldsAction(this, null);
						onEndOfReportDFA.IsUnparsable = false;
						reportActions.AfterActions.AddAction(onEndOfReportDFA);
					}

					onFormFeedDFA.AddField(field);
					onEndOfReportDFA.AddField(field);
				}
				else
				{
					// display actions in Report_Event.Always_Section explicity inserted!
					if (ExplicitDisplayNotAllowed)
					{
						lex.SetError(string.Format(WoormEngineStrings.ExplicitDisplayNotAllowed));
						return false;
					}

					// build Report event if it don't exist yet
					if (reportActions == null)
						reportActions = new ReportEventActions(this);
			
					// add actions as below:
					//
					//	Report : Do
					//		Always Display field_1, field_2, .. field_N;
					//
					if (alwaysDFA == null)
					{
                        alwaysDFA = new DisplayFieldsAction(this, null);
						// insert as first action the automatic field display actions
						reportActions.AlwaysActions.InsertActionAt(0, alwaysDFA);
					}
			
					alwaysDFA.AddField(field);
				}

				field.Displayed = true;
			}

			int numTable = RepSymTable.DisplayTablesNum;

			// add a NEXTLINE action for evry Display Table that has a field automatically displayed
			// and eventually insert a table overflow FORMFEED action
			//
			if	(
				RepSymTable.DisplayTables.Count == 0 &&
				(reportActions == null || reportActions.AlwaysActions.EmptyCommands) &&
				triggeredEvents.Count == 0
				)
			{
				autoFormFeed = new FormFeedAction(this, null);
				return true;
			}

			if (numTable > 0)
			{
				foreach (DisplayTable displayTable in RepSymTable.DisplayTables)
				{
					if (alwaysDFA != null && alwaysDFA.ExistColumnOf(displayTable))
					{
						DisplayTableAction pDisplayTableAction;
						pDisplayTableAction = new DisplayTableAction(this, null, RdeWriter.Command.NextLine, displayTable);
						pDisplayTableAction.IsUnparsable = false;
						reportActions.AlwaysActions.AddAction(pDisplayTableAction);
					}

					if (displayTable.TableActions == null)
						displayTable.TableActions = new EventActions(this);

					// at least a FormFeedAction on table overflow (BEFORE actions) must exists
					if (displayTable.TableActions.BeforeActions.EmptyCommands)
					{
						OnTableAction = true;

						FormFeedAction ffa = new FormFeedAction(this, null);
						ffa.IsUnparsable = false;
						displayTable.TableActions.BeforeActions.AddAction(ffa);
						OnTableAction = false;
					}
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private bool ParseProcedures(Parser lex)
		{
			OnFormFeedAction	= false;
			OnTableAction		= false;

			if (!lex.Parsed(Token.PROCEDURES))
				return true;			

			if (!lex.ParseBegin())
				return false;

			while (lex.LookAhead(Token.PROCEDURE))
			{
				Procedure procedure = new Procedure(this);
				if (!procedure.Parse(lex)) 
					return false;
			}

			if (!lex.ParseEnd())
				return false;

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		private bool ParseQueries(Parser lex)
		{
			OnFormFeedAction	= false;	//TODO servono ?
			OnTableAction		= false;

			if (!lex.Parsed(Token.QUERIES))
				return true;			

			if (!lex.ParseBegin())
				return false;

			string	name = "";
			while (lex.Parsed(Token.QUERY))
			{
				if (!lex.ParseID(out name)) 
                    return false;

				QueryObject queryObject = RepSymTable.QueryObjects.Find(name);
				if (queryObject == null)
				{
					queryObject = new QueryObject(name, RepSymTable.Fields, Session, null);
					RepSymTable.QueryObjects.Add(queryObject);
				}
				else 
				{
					lex.SetError(string.Format(WoormEngineStrings.ProcedureExist, name)); //TODO messaggio
					return false;
				}

				if (!queryObject.Parse(ref lex)) 
					return false;
			}

			if (!lex.ParseEnd())
				return false;

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		private bool ParseFieldInfo(Parser lex)
		{
			Field rf = Field.ParseFieldInfo(lex, RepSymTable, this);
            if (rf == null)
                return false;

            RepSymTable.Fields.Add(rf);

			return rf.Parse(lex, this) && lex.ParseSep();
		}

		// Aggiunge subito una variabile predefinita di nome OwnerID che contiene l'identificatore
		// (solitamente il puntatore) all'oggetto (tipicamente un Documento) che chiama il report
		//---------------------------------------------------------------------------
		private void AddOwnerIDField()
		{
			Field rf = new Field("Int32", SpecialReportField.REPORT_SPECIAL_FIELD_NAME_OWNER, this);
			rf.Hidden = true;
			rf.SetReadOnly();
			rf.Id = SpecialReportField.REPORT_OWNER_ID;

			RepSymTable.Fields.Add(rf);
		}

		// Aggiunge subito una variabile predefinita di nome ReportStatus che memorizza
		// gli stati del report e che può essere quindi utilizzata all'interno di un
		// report stesso da parte del programmatore
		//---------------------------------------------------------------------------
		private void AddReportStatusField()
		{
            string name = SpecialReportField.REPORT_SPECIAL_FIELD_NAME_STATUS;
			ushort tag = Session.Enums.TagValue(name);				
			ushort item = 0;

			Field rf = new Field("DataEnum", SpecialReportField.REPORT_SPECIAL_FIELD_NAME_STATUS, tag, item, this);
			rf.Hidden = true;
			rf.SetReadOnly();
			rf.Id = SpecialReportField.REPORT_STATUS_ID;
	
			RepSymTable.Fields.Add(rf);
		}

		// Aggiunge subito una variabile predefinita di nome ReportCurrentPageNumber che contiene 
		// il numero corrente di pagina del report
		//---------------------------------------------------------------------------
		private void AddReportReportCurrentPageNumberField()
		{
			Field rf = new Field("Int32", SpecialReportField.REPORT_SPECIAL_FIELD_NAME_CURRENT_PAGE_NUMBER, this);
			rf.Hidden = false;
			rf.SetReadOnly();
			rf.SetAllData(1, true);
			rf.Id = SpecialReportField.REPORT_PAGE_NUMBER_ID;
			rf.IsSpecialFieldInitialized = true;		//per evitare che la reinit dopo esecuzione ask lo resetti (in c++ c'e' un'espressione dummy x evitarlo)

			RepSymTable.Fields.Add(rf);
		}

		// Aggiunge subito una variabile predefinita di nome ReportLayout che contiene il nome del layout corrente 
		//---------------------------------------------------------------------------
		private void AddReportLayoutField()
		{
			Field rf = new Field("String", SpecialReportField.REPORT_SPECIAL_FIELD_NAME_LAYOUT, this);
			rf.Hidden = false;
			rf.SetReadOnly();
			rf.SetAllData(Layout.DefaultName, true);
			rf.Id = SpecialReportField.REPORT_LAYOUT_ID;
			rf.IsSpecialFieldInitialized = true;		//per evitare che la reinit dopo esecuzione ask lo resetti (in c++ c'e' un'espressione dummy x evitarlo)

			RepSymTable.Fields.Add(rf);
		}

        //---------------------------------------------------------------------------
        private void AddHideAllAskDialogField()
        {
            Field rf = new Field("Bool", SpecialReportField.REPORT_SPECIAL_FIELD_NAME_HIDE_ALL_ASK_DIALOGS, this);
            rf.Hidden = true;
            rf.SetReadOnly();
            rf.SetAllData(false, true);
            rf.Id = SpecialReportField.REPORT_HIDE_ALL_ASK_DIALOGS;
            rf.IsSpecialFieldInitialized = true;		//per evitare che la reinit dopo esecuzione ask lo resetti (in c++ c'e' un'espressione dummy x evitarlo)

            RepSymTable.Fields.Add(rf);
        }

		//---------------------------------------------------------------------------
        public void AddSpecialFields()
        {
			AddReportStatusField();
			AddOwnerIDField();
			AddReportReportCurrentPageNumberField();
			AddReportLayoutField();
            AddHideAllAskDialogField();
        }

		//---------------------------------------------------------------------------
		public bool ParseFields(Parser lex)
		{
			if (!lex.Error && lex.ParseTag(Token.VAR))
			{
                AddSpecialFields();

				while(!lex.Parsed(Token.END) && !lex.Error)
				{
					if (!ParseFieldInfo(lex)) 
						return false;
				}
			}

			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public DisplayTable ParseDisplayTable(Parser lex)
		{
			if (lex.LookAhead(Token.ID))
			{
				string strTableName;
				DisplayTable displayTable;

				if (lex.ParseID(out strTableName))
				{
					displayTable = RepSymTable.DisplayTables.Find(strTableName);
					if (displayTable != null)
						return displayTable;

					lex.SetError(string.Format(WoormEngineStrings.UnknownDisplayTable, strTableName));
				}
				return null;
			}

			if (RepSymTable.DisplayTables.Count == 1)
				return (DisplayTable) RepSymTable.DisplayTables[0];

			lex.SetError(WoormEngineStrings.MissingTableName);
			return null;
		}

		//---------------------------------------------------------------------------
		public bool ParseDisplayTablesInfo(Parser lex)
		{
			string name;

			while (lex.LookAhead(Token.ID))
			{
				if (!lex.ParseID(out name)) break;

				DisplayTable displayTable = new DisplayTable(name);
				if (!displayTable.Parse(lex, this.RepSymTable.Fields)) return false;

				if (RepSymTable.DisplayTables.Find(name, displayTable.LayoutTable) != null)
				{
					lex.SetError(string.Format(WoormEngineStrings.DisplayTableExist,name));
					return false;
				}
				/* aggiungo se non ci sono errori*/
				RepSymTable.DisplayTables.Add(displayTable);
			}
			lex.ParseEnd();
			
			return !lex.Error;
		}

		//---------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			ExplicitDisplayNotAllowed = false;
			DisplayAction = false;
			OnFormFeedAction = false;
			OnTableAction = false;
			itemStart = RepSymTable.Procedures.Count;

			string name = "";
			if (!lex.ParseID(out name))
				return false;

			PublicName = name;

			if (lex.Parsed(Token.TABLES) && !ParseDisplayTablesInfo(lex))
				return false;

			if (!ParseFields(lex))
				return false;

			if (!ParseRules(lex))
				return false;

			if (
					!ParseEvents(lex) ||
					!ParseProcedures(lex) ||
					!ParseQueries(lex) ||
					!ParseAskingRules(lex) ||
					!lex.ParseEnd()
				)
				return false;

			return MakeAutoDisplayActions(lex);
		}

		//---------------------------------------------------------------------------
		internal void UnparseEvents(Unparser unparser)
		{
			if (IsEmpty)
				return;

			unparser.IncTab();

			unparser.WriteLine();
			unparser.WriteTag(Token.EVENTS, true);

			unparser.IncTab();
			
			if (onFormFeedActions != null)
				onFormFeedActions.UnparseFormFeedEvent(unparser);

			if (reportActions != null)
				reportActions.Unparse(unparser);

			foreach (DisplayTable item in RepSymTable.DisplayTables)
				item.UnparseTableEventAction(unparser);
			
			foreach (TriggeredEventActions item in triggeredEvents)
				item.Unparse(unparser);

			unparser.DecTab();
			unparser.WriteEnd(true);
			
			unparser.DecTab();
		}

		//---------------------------------------------------------------------------
		public bool IsEmpty
		{
			get
			{
				return
					RepSymTable.DisplayTables.IsEmpty &&
					(triggeredEvents.Count == 0) &&
					(reportActions != null && reportActions.IsEmpty) &&
					(onFormFeedActions != null && onFormFeedActions.IsEmpty);
			}
		}

		//---------------------------------------------------------------------------
		internal void UnparseFields(Unparser unparser)
		{
			if (RepSymTable.IsEmpty)
				return;

			bool noDispTableName = RepSymTable.DisplayTables.Count <= 1;

			unparser.WriteTag(Token.VAR, true);
			unparser.IncTab();

			foreach (Field item in RepSymTable.Fields)
				item.Unparse(unparser, noDispTableName);

			unparser.DecTab();
			unparser.WriteEnd();
			unparser.WriteLine();
		}

		//---------------------------------------------------------------------------
		internal void UnparseDisplayTable(Unparser unparser)
		{
			unparser.WriteTag(Token.TABLES, true);
			unparser.IncTab();
			
			foreach (DisplayTable item in this.RepSymTable.DisplayTables)
				item.Unparse(unparser);

			unparser.DecTab();
			unparser.WriteEnd();
			
			unparser.WriteLine();
		}

		//---------------------------------------------------------------------------
		internal void UnparseAskingRules(Unparser unparser)
		{
			unparser.WriteTag(Token.DIALOGS);

			unparser.IncTab();
			unparser.WriteBegin();

			foreach (AskDialog item in askingRules)
				item.Unparse(unparser);

			unparser.WriteEnd();
			unparser.DecTab();
		}
    }
}

