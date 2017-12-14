using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using System.Windows.Forms;

/*  parte di report di woorm interessata a questa componente

Report 
	Begin NoName
		Tables
			Shippers [30] Alias 4 ;
			Products [30] Alias 9 ;
		End

		Variables
			String       Campo_1 [15] Alias 1  Hidden  Input Init = ""  ;
			String       Campo_2 [15] Alias 2  Hidden  Input Init = ""  ;
			Long         ShipperID [12] Alias 3  Column  of Shippers ;
			Long         TotShipperID = Csum (ShipperID ) Alias 5  ColTotal ;
			Long         SupplierID [12] Alias 6  Column  of Products ;
			Long         CategoryID [12] Alias 7  Column  of Products ;
			Integer      ReorderLevel [6] Alias 8  Column  of Products ;
			Long         TotSupplierID = Csum (SupplierID ) Alias 10  ColTotal ;
			Long         TotCategoryID = Csum (CategoryID ) Alias 11  ColTotal ;
			Integer      TotReorderLevel = Csum (ReorderLevel ) Alias 12  ColTotal ;
		End

		Rules
			From Shippers 
			Select 
				ShipperID            Into ShipperID ;

			From Products 
			Select 
				SupplierID           Into SupplierID ,
				CategoryID           Into CategoryID ,
				ReorderLevel         Into ReorderLevel ;
		End


		Events
			FormFeed : Do
				Before 
					Begin
						Display TotShipperID ;
						Reset TotShipperID ;
						Display TotSupplierID ;
						Reset TotSupplierID ;
						Display TotCategoryID ;
						Reset TotCategoryID ;
						Display TotReorderLevel ;
						Reset TotReorderLevel ;
					End

			Report : Do
				Always 
					Begin
						Eval TotShipperID ;
						Eval TotSupplierID ;
						Eval TotCategoryID ;
						Eval TotReorderLevel ;
					End
				After 
					Begin
						Display TotShipperID ;
						Display TotSupplierID ;
						Display TotCategoryID ;
						Display TotReorderLevel ;
					End
		End

		Dialog NoName  
			Begin   
				Campo_1 Prompt = "" ;
				Campo_2 Prompt = "" ;
			End
	End
*/

namespace Microarea.TaskBuilderNet.Woorm.WoormEngine
{
	public enum EngineType { Standard, OfficeXML, OfficePDF }

	/// <summary>
	/// Report.
	///		Riespetto al Woorm C++ non gestisce le seguenti sintassi:
	///		
	///			1) Clausola Order By finale.
	///			2) Const parameter nella chiamata di funzione esterna
	///			3) Clausola Having nelle WhereClause delle Select
	///			4) Eliminato il codice di gestione del file temporaneo (tanto non funzionava)
	///			
	/// </summary>
	//============================================================================
	[Serializable]
	[KnownType(typeof(RepSymTable))]
	public class Report : IDisposable, ISerializable
	{
		const string SYMTABLE = "symtable";


		private bool disposed = false;	// Track whether Dispose has been called.

		// mappa l'enumerativo definito negli enums.ini
		enum ReportStatus:ushort 
		{ 
			SUCCESS				= 0x0000, 
			NO_DATA_FOUND		= 0x0001,
			ABNORMAL_END		= 0x0002, 
			USER_ABORT			= 0x0003, 
			PRINTING_PREVIEW	= 0x0004,
            SCRIPT_ABORT        = 5,
            SCRIPT_QUIT         = 6,
			TAG					= 0xFFFE
		}

		private bool				compiled = false;
		private Parser				lex = null;

		protected string			reportName = "";
		private string				helpFileName = "";
		private ReportEngine		engine = null;
		private RepSymTable			symTable = null;
		private string				uniqueID = "";
		private string				sessionID = "";
		private DataEnum			reportStatus;
		private RuleReturn			exitStatus = RuleReturn.Success;
		private TbReportSession	    reportSession;
		private EngineType			engineType = EngineType.Standard;
		private string				maxString = ObjectHelper.GetMaxString(string.Empty);
        private ParametersList      initParameters;

		// properties
		public bool ForLocalizer { get; set; }//mi dice se l'engine è usato dal localizer per parsare i file; in questo caso, alcune funzionalità sono disabilitate
		public string			MaxString			{ get { return maxString; }}
		public RepSymTable		SymTable			{ get { return symTable; }}
		public ReportEngine		Engine				{ get { return engine; } set { engine = value; }}
		public string			HelpFileName		{ get { return helpFileName; } set { helpFileName = value; }}
		public TbReportSession	ReportSession		{ get { return reportSession; }}
		public EngineType		EngineType			{ get { return engineType; } set { engineType = value; }}

		public string			UniqueID			{ get { return uniqueID; }}
		public string			SessionID			{ get { return sessionID; }}
		public Parser			Lex					{ get { return lex;}}
		public List<AskDialog>	AskingRules			{ get { return engine.AskingRules; } }
		public string			ReportName			{ get { return reportName; }}
		public AskDialog		CurrentAskDialog	{ get { return engine.CurrentAskDialog; }}
        public ILocalizer		Localizer           { get { return ReportSession.Localizer; } /*set { localizer = value; }*/ }
		public RuleReturn		ExitStatus			{ get { return exitStatus; }}
		public IWoormInfo		WoormInfo		    { get; set; }
        public ParametersList   InitParameters      { get { return initParameters; } }

		// usate per la persistenza XML per office
		//---------------------------------------------------------------------------
		public		StringCollection	XmlResultReports;
		public		XmlDocument			XmlDomParameters;

		public		string				ReportTitle = string.Empty;
		public		IWoormDocumentObj woormDoumentObj;

		//---------------------------------------------------------------------------
		public IDiagnostic	Diagnostic			
		{ get 
			{ 
				if (lex.Diagnostic.Error)
					return lex.Diagnostic;

				return engine.Diagnostic; 
			}
		}

		//Per modalita stampa pdf da webservice
		//---------------------------------------------------------------------------
		public Report
			(
				string reportName,
				TbReportSession reportSession,
				XmlDocument xmlDomParameters,
				string sessionID,
				string uniqueID
			)
		{
			this.lex		= new Parser(Parser.SourceType.FromFile);
			this.reportName	= reportName;
			this.reportSession	= reportSession;

			this.engineType = EngineType.OfficePDF;
			this.sessionID	= sessionID;
			this.uniqueID	= uniqueID;

			this.XmlDomParameters	= xmlDomParameters;
			this.XmlResultReports	= null;

			this.maxString = Microarea.TaskBuilderNet.Core.NameSolver.ReadSetting.GetMaxString(ReportSession.PathFinder,ReportSession.UserInfo.LoginManager.PreferredLanguage);
			ObjectHelper.DataDblEpsilon = Microarea.TaskBuilderNet.Core.NameSolver.ReadSetting.GetDataDblDecimal(ReportSession.PathFinder);

			symTable		= new RepSymTable();
			engine			= new ReportEngine(this,engineType);
		
			ReadParameters();
		}

		//---------------------------------------------------------------------------
		public Report
			(
				string				reportName,
				TbReportSession	    reportSession,
				XmlDocument			xmlDomParameters,
				StringCollection	xmlResultReports
			)
		{
			this.lex		= new Parser(Parser.SourceType.FromFile);
			this.reportName	= reportName;
			this.sessionID	= "";
			this.uniqueID	= "";
			this.reportSession	= reportSession;
			this.engineType = EngineType.OfficeXML;

			this.XmlDomParameters	= xmlDomParameters;
			this.XmlResultReports	= xmlResultReports;

			this.maxString = Microarea.TaskBuilderNet.Core.NameSolver.ReadSetting.GetMaxString(ReportSession.PathFinder, ReportSession.UserInfo.LoginManager.PreferredLanguage);
			ObjectHelper.DataDblEpsilon = Microarea.TaskBuilderNet.Core.NameSolver.ReadSetting.GetDataDblDecimal(ReportSession.PathFinder);

			symTable		= new RepSymTable();
			engine			= new ReportEngine(this, engineType);
            //localizer		= new WoormLocalizer(reportName, Session.PathFinder);
			 
			ReadParameters();
		}

		//---------------------------------------------------------------------------
		public Report
			(
			string				reportName,
			TbReportSession	session,
			string				sessionID,
			string				uniqueID,
			IWoormDocumentObj   woormDoumentObj = null
			)
		{
			this.lex		= new Parser(Parser.SourceType.FromFile);
			this.reportName	= reportName;
			this.sessionID	= sessionID;
			this.uniqueID	= uniqueID;
			this.reportSession	= session;
			this.engineType = EngineType.Standard;

			this.XmlDomParameters	= null;
			this.XmlResultReports	= null;

			this.woormDoumentObj = woormDoumentObj;

			if (ReportSession!= null && ReportSession.UserInfo.LoginManager != null)
				this.maxString = ReadSetting.GetMaxString(ReportSession.PathFinder, ReportSession.UserInfo.LoginManager.PreferredLanguage);

			symTable		= new RepSymTable();
			engine			= new ReportEngine(this, engineType);
			//localizer		= new WoormLocalizer(reportName, Session.PathFinder);

			ReadParameters();
		}

		//------------------------------------------------------------------------------
		~Report()
		{
			Dispose(false);
		}

		//------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
			}
			disposed = true;         
		}


		//--------------------------------------------------------------------------
		public Report(SerializationInfo info, StreamingContext context)
		{
			//TODO SILVANO
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(SYMTABLE, symTable);
		}

		// recupera la variabile predefinita ReportStatus
		//---------------------------------------------------------------------------
		public void InitReport()
		{
            Field rf = symTable.Fields.Find(SpecialReportField.REPORT_SPECIAL_FIELD_NAME_STATUS);
			Debug.Assert(rf != null);

			reportStatus = (DataEnum)rf.Data;
			reportStatus.Assign((ushort)ReportStatus.SUCCESS);
		}

		//unparsa gli eventuali parametri di report da stringa xml
		//---------------------------------------------------------------------------
		private void ReadParameters()
		{
			if (reportSession.ReportParameters == null || reportSession.ReportParameters.Length == 0)
				return;

			XmlDocument paramDoc = new XmlDocument();
			paramDoc.LoadXml(reportSession.ReportParameters);
            FunctionPrototype info = new FunctionPrototype();
            FunctionPrototype.ParseParameters(paramDoc.DocumentElement, info);
			initParameters = info.Parameters;
		}

		//---------------------------------------------------------------------------
		public void InitializeChannel()
		{
			// pulisce ii file XMl eventualmente lasciati dall'ultimo run
			PathFunctions.DeleteTempData(SessionID, UniqueID);
		}

		/// <summary>
		/// Metodo che permette il salvataggio su file system del file xml che definisce il nome delle variabili, gli alias ecc
		/// (corrispondente alla viewSymbolTable dell'engine c++)
		/// </summary>
		//---------------------------------------------------------------------------
		public void SaveInfo()
		{
			// salva le informazione globali a tutte le pagine in un file xml
			engine.OutChannel.SaveInfo();
		}

		//---------------------------------------------------------------------------
		public void FinalizeChannel()
		{
			// Chiude l'ultima pagina
			engine.OutChannel.SavePage();
			
			//Anticipato per permettere visualizzazione prima pagina prima della chiusura!!
			//verificare ci vada tutto dentro
			//engine.OutChannel.SaveInfo();

			engine.OutChannel.SaveTotPageFile();
		}

		//---------------------------------------------------------------------------
		public bool			ExecuteInitialize()		{ return engine.ExecuteInitialize(initParameters); }
		public bool			ExecuteAsk()			{ return engine.ExecuteAsk(); }
		public bool			ExecuteLoadParamters()	{ return engine.ExecuteLoadParamters(); }
		public void			ExecuteAfterAsk()		{ engine.ExecuteAfterAsk(); }
		public bool			ExecuteBeforeActions()	{ return engine.ExecuteBeforeActions(); }
		public RuleReturn	ExecuteRulesAndEvents()	{ exitStatus = engine.ExecuteRulesAndEvents(); return exitStatus;}
		public bool			ExecuteAfterActions()	{ return engine.ExecuteAfterActions(); }
		public bool			ExecuteFinalizeActions(){ return engine.ExecuteFinalizeActions(); }

		//---------------------------------------------------------------------------
		public void ExecuteErrorStep()
		{          
			reportStatus.Assign(engine.UserBreak ? (ushort)ReportStatus.USER_ABORT : (ushort)ReportStatus.ABNORMAL_END);
			ExecSpecialProc("OnAbend");
		}

		//---------------------------------------------------------------------------
		private bool SkipGraphSection(Parser lex)
		{                        
			while (true)
			{
				if (!lex.SkipToToken(new Token[] { Token.REPORT, Token.PROPERTIES}, false, false))
					return false;

				if (lex.LookAhead(Token.REPORT))
					return !lex.Error;

				// no syntax check here: simply if you find the Title property, just store it!
				// (syntax check will be performed by WoormDocument object)
				if (lex.Parsed(Token.PROPERTIES))
				{
					lex.ParseBegin();
					if (lex.SkipToToken(new Token[] { Token.END, Token.TITLE}, false, false))
					{
						if (lex.Parsed(Token.TITLE))
							lex.ParseString(out ReportTitle);
					}
				}

                if (lex.LookAhead(Token.EOF))
                    return true;
			}
		}

		//---------------------------------------------------------------------------
		public bool Compile()
		{
			if (compiled)
				return !Lex.Diagnostic.Error;

			if (!Lex.Open(reportName))
				return false;

			bool ok = 
				SkipGraphSection(Lex) && 
				ParseAndBuild(Lex);
	
			Lex.Close();
			if (ok) compiled = true;
			return ok;
		}

		//---------------------------------------------------------------------------
		private bool ParseAndBuild(Parser lex)
		{
			if (lex.Parsed(Token.REPORT))
			{
				while (lex.LookAhead() != Token.PROPERTIES && lex.LookAhead() != Token.EOF)
				{
					if (lex.Parsed(Token.HELP))
					{
						if (!lex.ParseString(out helpFileName))
							return false;
						if (helpFileName.Length == 0)
						{
							lex.SetError(WoormEngineStrings.InvalidHelp);
							return false;
						}
					}

					lex.ParseBegin();
					if (!engine.Parse(lex)) return false;
				}
			}
		    
            if (SymTable.Fields.Count == 0)
                engine.AddSpecialFields();

			return ! (lex.Error);
		}

		//---------------------------------------------------------------------------
		private bool ExecSpecialProc(string procedureName)
		{
			if (symTable == null) return false;

			Procedure procedure = symTable.Procedures.Find(procedureName);

			// se non c'è  non mi arrabbio e faccio finta di averla eseguita
			if (procedure == null)
				return true;

			return procedure.Exec();
		}


		//---------------------------------------------------------------------------
		public bool OnBeginPrinting(bool bPreview)
		{
			if (symTable == null) return false;

			// Assegna lo stato effettivo
			reportStatus.Assign
			(
				bPreview  
					? (ushort)ReportStatus.PRINTING_PREVIEW
					: (ushort)ReportStatus.SUCCESS
			);

			return ExecSpecialProc("OnBeginPrinting");
		}

		//---------------------------------------------------------------------------
		public ArrayList GetLocalizableStrings()
		{
			ArrayList localizableStrings = new ArrayList();
			localizableStrings.AddRange(SymTable.Fields.LocalizableStrings);
			return localizableStrings;
		}	

		//---------------------------------------------------------------------------
		public bool OnPreparePrinting()	{ return ExecSpecialProc("OnPreparePrinting");	}
		public bool OnEndPrinting()		{ return ExecSpecialProc("OnEndPrinting"); }
		public bool OnAbortPrinting()	{ return ExecSpecialProc("OnAbortPrinting"); }
		public bool OnCloseReport()		{ return ExecSpecialProc("OnCloseReport");	}

		//---------------------------------------------------------------------------
		internal bool Unparse(Unparser unparser)
		{
			if (engine.RepSymTable.IsEmpty) 
				return true;

			unparser.WriteLine();
			unparser.WriteTag(Token.REPORT, true);
			unparser.IncTab();

			unparser.WriteBegin(false);

			if (string.IsNullOrEmpty(engine.PublicName))
				engine.PublicName = "NoName";

			unparser.WriteID(engine.PublicName, true);
			unparser.IncTab();
			
			engine.UnparseDisplayTable(unparser);
			engine.UnparseFields(unparser); 
			engine.UnparseRules(unparser);
			engine.UnparseEvents(unparser);

            symTable.Procedures.Unparse(unparser);		
			symTable.QueryObjects.Unparse(unparser); 

			engine.UnparseAskingRules(unparser);

			unparser.DecTab();
			unparser.WriteEnd(true);
			unparser.DecTab();

			return true;
		}

	}

	//============================================================================
	public interface IWoormDocumentObj
	{
		int CalculateFieldWidth(int objectId,string strText);
        bool GetFieldWidthFactors(int objectId, ref FieldWidthFactors fwf, bool isSubTotal = false);
	}

 	//============================================================================
	public class FieldWidthFactors : IDisposable
	{
		private Form wnd; //SplitString
		public Graphics g;
		public Font font;
		public int m_nWidth;
        Bitmap myBitmap = null;

        public FieldWidthFactors()
		{
            wnd = new Form();
            g = wnd.CreateGraphics();
            myBitmap = new Bitmap(1980, 1080);
            g = Graphics.FromImage(myBitmap);
        }

		public void Dispose() { font.Dispose(); g.Dispose(); wnd.Dispose(); }

		public int GetStringWidth(string s)
		{
			return (int)g.MeasureString(s, font).Width;
		}
	}
}
