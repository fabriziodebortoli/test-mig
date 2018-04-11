using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.NameSolver;
using Microarea.Common.Generic;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;

using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.Objects;
using Microarea.Common.ExpressionManager;

namespace Microarea.RSWeb.WoormViewer
{
    public enum PageType { First, Last, Prev, Next, Current, Unknown }
	public enum CellType { Cell, SubTotal, Total, LowerInput, UpperInput, Array}

	/// <summary>
	/// Descrizione di riepilogo per reader.
	/// </summary>
	public class SpecialField
	{
		private WoormDocument document;

		public const string SPECIAL_PAGE				="Page";
		public const string SPECIAL_TOT_PAGE			="Page Number";
		public const string SPECIAL_APPDATE				="Application Date";
		public const string SPECIAL_TODAY				="Date (dd-mm-yy)";
		public const string SPECIAL_TODAY2				="Date (dd month yy)";
		public const string SPECIAL_YEAR				="Year";
		public const string SPECIAL_MONTH				="Month";
		public const string SPECIAL_MONTH2				="Month Name";
		public const string SPECIAL_DAY					="Day";
		public const string SPECIAL_HH_MM				="Time";
		public const string SPECIAL_HH					="Hours";
		public const string SPECIAL_MM					="Minutes";
		public const string SPECIAL_SEC					="Seconds";
		public const string SPECIAL_USER				="Window User";
		public const string SPECIAL_LOGINUSER			="Logged User";
		public const string SPECIAL_COMPUTER			="Computer Name";
		public const string SPECIAL_APP_TITLE			="Application Name";
		public const string SPECIAL_APP_REL				="Version";
		public const string SPECIAL_TB_REL				="TaskBuilder Version";
		public const string SPECIAL_DSN					="Company Name";
		public const string SPECIAL_LICENSEE			="Licensee";
		public const string SPECIAL_REPORTNAME			="Report Name";
		public const string SPECIAL_REPORT_TITLE		="Report Title";
		public const string SPECIAL_REPORT_SUBJECT		="Report Subject";
		public const string SPECIAL_REPORT_AUTHOR		="Report Author";
		public const string SPECIAL_REPORT_COMPANY		="Report Owner";
		public const string SPECIAL_REPORT_COMMENTS		="Report Remarks";

        public const string SPECIAL_DB_COMPANY_NAME     = "Company.Name";
        public const string SPECIAL_PRODUCER_NAME       = "Producer Name";
        public const string SPECIAL_PRODUCT_DATE        = "Product Date";

		public const string OLD_SPECIAL_PAGE				="Pagina";
		public const string OLD_SPECIAL_TOT_PAGE			="Totale pagine";
		public const string OLD_SPECIAL_APPDATE				="Data delle operazioni";
		public const string OLD_SPECIAL_TODAY				="Data (gg-mm-aa)";
		public const string OLD_SPECIAL_TODAY2				="Data (gg mese aa)";
		public const string OLD_SPECIAL_YEAR				="Anno";
		public const string OLD_SPECIAL_MONTH				="Mese";
		public const string OLD_SPECIAL_MONTH2				="Nome del mese";
		public const string OLD_SPECIAL_DAY					="Giorno";
		public const string OLD_SPECIAL_HH_MM				="Ora e minuto";
		public const string OLD_SPECIAL_HH					="Ora";
		public const string OLD_SPECIAL_MM					="Minuto";
		public const string OLD_SPECIAL_SEC					="Secondo";
		public const string OLD_SPECIAL_USER				="Utente";
		public const string OLD_SPECIAL_LOGINUSER			="Utente Applicativo";
		public const string OLD_SPECIAL_COMPUTER			="Computer";
		public const string OLD_SPECIAL_APP_TITLE			="Applicazione";
		public const string OLD_SPECIAL_APP_REL				="Versione";
		public const string OLD_SPECIAL_TB_REL				="TaskBuilder";
		public const string OLD_SPECIAL_DSN					="Azienda";
		public const string OLD_SPECIAL_LICENSEE			="Licenziatario";
		public const string OLD_SPECIAL_REPORTNAME			="Nome del report";
		public const string OLD_SPECIAL_REPORT_TITLE		="Titolo del report";
		public const string OLD_SPECIAL_REPORT_SUBJECT		="Oggetto del report";
		public const string OLD_SPECIAL_REPORT_AUTHOR		="Autore del report";
		public const string OLD_SPECIAL_REPORT_COMPANY		="Società proprietaria del report";
		public const string OLD_SPECIAL_REPORT_COMMENTS		="Commenti al report";
		//---------------------------------------------------------------------------
		public SpecialField(WoormDocument wd)
		{
			document = wd;
		}

		//---------------------------------------------------------------------------
		public string FormatToken(string source)
		{
            string shortDatePattern =  Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
            string longDatePattern =  Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern;   

            switch (source)
            {
                //TODO RSWEB verificare che siano tutti - vedi SpecialField di woorm
                case SPECIAL_LOGINUSER:
                case OLD_SPECIAL_LOGINUSER:
                    return document.ReportSession.UserInfo.User;

                case SPECIAL_APP_TITLE:
                case OLD_SPECIAL_APP_TITLE:
                    return "ReportingStudio-AspNetCore";

                case SPECIAL_APP_REL:
                case OLD_SPECIAL_APP_REL:
                    return document.ReportSession.PathFinder.InstallationVer.Version;
                case SPECIAL_TB_REL:
                case OLD_SPECIAL_TB_REL:
                    return ".Net";

                case SPECIAL_DSN:
                case OLD_SPECIAL_DSN:
                    return document.ReportSession.UserInfo.Company;

                case SPECIAL_LICENSEE:
                case OLD_SPECIAL_LICENSEE:
                    return document.ReportSession.UserInfo.UserInfoLicensee;

                case SPECIAL_PAGE:
                case OLD_SPECIAL_PAGE:
                    return document.CurrentPage.ToString();
                case SPECIAL_TOT_PAGE:
                case OLD_SPECIAL_TOT_PAGE:
                    return document.RdeReader.TotalPages.ToString();

                case SPECIAL_APPDATE:
                case OLD_SPECIAL_APPDATE:
                    DateTime d = document.ReportSession.ApplicationDate;
                    //return document.FormatStyles.Format(d.GetType().Name, d, document.Namespace);
                    return d.ToString(shortDatePattern);

                case SPECIAL_TODAY:
                case OLD_SPECIAL_TODAY:
                    return DateTime.Today.ToString(shortDatePattern);
                case SPECIAL_TODAY2:
                case OLD_SPECIAL_TODAY2:
                    return DateTime.Today.ToString(longDatePattern);
                case SPECIAL_YEAR:
                case OLD_SPECIAL_YEAR:
                    return DateTime.Today.ToString("yyyy");
                case SPECIAL_MONTH:
                case OLD_SPECIAL_MONTH:
                    return DateTime.Today.ToString("MM");
                case SPECIAL_MONTH2:
                case OLD_SPECIAL_MONTH2:
                    return DateTime.Today.ToString("MMMM");
                case SPECIAL_DAY:
                case OLD_SPECIAL_DAY:
                    return DateTime.Today.ToString("dd");
                case SPECIAL_HH_MM:
                case OLD_SPECIAL_HH_MM:
                    return DateTime.Now.ToString("HH:mm");
                case SPECIAL_HH:
                case OLD_SPECIAL_HH:
                    return DateTime.Now.ToString("HH");
                case SPECIAL_MM:
                case OLD_SPECIAL_MM:
                    return DateTime.Now.ToString("mm");
                case SPECIAL_SEC:
                case OLD_SPECIAL_SEC:
                    return DateTime.Now.ToString("ss");

                case SPECIAL_USER:
                case OLD_SPECIAL_USER:
                    //TODO RSWEB return SystemInformation.UserName;
                    return string.Empty;

                case SPECIAL_COMPUTER:
                case OLD_SPECIAL_COMPUTER:
                    //TODO RSWEB return SystemInformation.ComputerName;
                    return string.Empty;

                case SPECIAL_REPORTNAME:
                case OLD_SPECIAL_REPORTNAME:
                    return document.Filename;
                case SPECIAL_REPORT_TITLE:
                case OLD_SPECIAL_REPORT_TITLE:
                    return document.Properties.Title;
                case SPECIAL_REPORT_SUBJECT:
                case OLD_SPECIAL_REPORT_SUBJECT:
                    return document.Properties.Subject;
                case SPECIAL_REPORT_AUTHOR:
                case OLD_SPECIAL_REPORT_AUTHOR:
                    return document.Properties.Author;
                case SPECIAL_REPORT_COMPANY:
                case OLD_SPECIAL_REPORT_COMPANY:
                    return document.Properties.Company;
                case SPECIAL_REPORT_COMMENTS:
                case OLD_SPECIAL_REPORT_COMMENTS:
                    return document.Properties.Comments;

                case SPECIAL_DB_COMPANY_NAME:
                {
                    return document.DBCompanyName;
                }
                case SPECIAL_PRODUCER_NAME:
                    return "Microarea SpA - Zucchetti"; //TODO RSWEB document.ReportSession.UserInfo.Brand.GetCompanyName();

                case SPECIAL_PRODUCT_DATE:
                    {
                        //TODO RSWEB DateTime d = document.ReportSession.PathFinder.ProductDate;
                        //string formatStyleName = ObjectHelper.DefaultFormatStyleName(d);
                        return document.ReportSession.PathFinder.InstallationVer.InstallationDate; //.ToString(shortDatePattern);
                    }
            }

            string s = source.Trim();
            if (s.IndexOfNoCase("EVAL ") == 0)
            {
                WoormViewerExpression expr = new WoormViewerExpression(document);
                if (!expr.Compile(s.Mid(5), CheckResultType.Match, "String"))
                {
                    return "";
                }

                Value v = expr.Eval();
                if (v.Valid && v.Data != null)
                    return v.Data.ToString();
            }

            return "";
		}

		//---------------------------------------------------------------------------
		static public string KeywordOld2New(string old)
		{       
			switch (old)
			{
				case OLD_SPECIAL_LOGINUSER			: 
					return SPECIAL_LOGINUSER;
				case OLD_SPECIAL_APP_TITLE			: 
					return SPECIAL_APP_TITLE;
				case OLD_SPECIAL_APP_REL			: 
					return SPECIAL_APP_REL;
				case OLD_SPECIAL_TB_REL				: 
					return SPECIAL_TB_REL;
				case OLD_SPECIAL_DSN				: 
					return SPECIAL_DSN; 
				case OLD_SPECIAL_LICENSEE			: 
					return SPECIAL_LICENSEE; 
				case OLD_SPECIAL_PAGE				: 
					return SPECIAL_PAGE;
				case OLD_SPECIAL_TOT_PAGE			: 
					return SPECIAL_TOT_PAGE;
				case OLD_SPECIAL_APPDATE			: 
					return SPECIAL_APPDATE;
				case OLD_SPECIAL_TODAY				: 
					return SPECIAL_TODAY;
				case OLD_SPECIAL_TODAY2				: 
					return SPECIAL_TODAY2;
				case OLD_SPECIAL_YEAR				: 
					return SPECIAL_YEAR;
				case OLD_SPECIAL_MONTH				: 
					return SPECIAL_MONTH;
				case OLD_SPECIAL_MONTH2				: 
					return SPECIAL_MONTH2;
				case OLD_SPECIAL_DAY				: 
					return SPECIAL_DAY;
				case OLD_SPECIAL_HH_MM				: 
					return SPECIAL_HH_MM;
				case OLD_SPECIAL_HH					: 
					return SPECIAL_HH;
				case OLD_SPECIAL_MM					: 
					return SPECIAL_MM;
				case OLD_SPECIAL_SEC				: 
					return SPECIAL_SEC;
				case OLD_SPECIAL_USER				: 
					return SPECIAL_USER;
				case OLD_SPECIAL_COMPUTER			: 
					return SPECIAL_COMPUTER;
				case OLD_SPECIAL_REPORTNAME			: 
					return SPECIAL_REPORTNAME;
				case OLD_SPECIAL_REPORT_TITLE		: 
					return SPECIAL_REPORT_TITLE;
				case OLD_SPECIAL_REPORT_SUBJECT		: 
					return SPECIAL_REPORT_SUBJECT;
				case OLD_SPECIAL_REPORT_AUTHOR		: 
					return SPECIAL_REPORT_AUTHOR;
				case OLD_SPECIAL_REPORT_COMPANY		: 
					return SPECIAL_REPORT_COMPANY;
				case OLD_SPECIAL_REPORT_COMMENTS	: 
					return SPECIAL_REPORT_COMMENTS;
			}
			return old;
		}

		// Converte le keywords italiane con quelle inglesi
		//---------------------------------------------------------------------------
		static public string ConvertKeywords(string text)
		{
			string newText = text;
			int begin = 0;
			while (true)
			{
				// cerca il primo token
				int start = newText.IndexOf("{", begin);
				if (start < 0) break;

				// cerca il secondo
				int stop = newText.IndexOf("}", start);
				if (stop < 0) break;

				string token = newText.Substring(start + 1, stop - start - 1);
				string newToken = KeywordOld2New(token);
				if (token != newToken)
				newText = newText.Replace("{" +  token + "}", "{" +  newToken + "}");
				begin = start + newToken.Length + 2;
			}
			return newText;
		}

		// Cerca in una stringa delle parole tra "{" e le espande secondo la funzione
		// FormatToken. Volendo puo` essere generalizzata come funzione non di classe
		// es: oggi {today} viene formattata in dd/mm/yy
		//---------------------------------------------------------------------------
		public string Expand(string source)
		{               
			if (source == null || source.Length == 0)
				return source;

			while (true)
			{
				// cerca il primo token
				int start = source.IndexOf("{");
				if (start < 0) break;

				// cerca il secondo
				int stop = source.IndexOf("}", start);
				if (stop < 0) break;

				string token = source.Substring(start + 1, stop - start - 1);
				source = source.Replace("{" +  token + "}", FormatToken(token));
			}

			return source;
		}
	}


	/// ================================================================================
	public class ReleaseChecker : IDisposable
	{
		private bool			disposed = false;	// Track whether Dispose has been called.
		private WoormParser		lex;
		private WoormDocument	woorm;
		private bool			localLex = false;
		private int				reportRelease;
		private int				reportModifyRelease = 0;
		private bool			checkOudDate = false;

		private const int ActualRelease = 7;
		private const int LowestCompatibleRelease = 6;
	
		public enum OutDateOperator { ND, LT, LE, EQ, GT, GE };

		//------------------------------------------------------------------------------
		private bool			_noWeb = false;
		public bool			NoWeb					{ get { return false; } set { _noWeb = value; } }
		public int			ReportRelease			{ get { return reportRelease; }}
		public int			ReportModifyRelease		{ get { return reportModifyRelease;  }}
		public IDiagnostic	Diagnostic				{ get { return lex.Diagnostic; }}

		//------------------------------------------------------------------------------
		public ReleaseChecker(WoormDocument woorm)
		{
			this.lex = new WoormParser(Parser.SourceType.FromFile);
			this.localLex = true;
			this.woorm = woorm;
			this.checkOudDate = true;
		}

		//------------------------------------------------------------------------------
		public ReleaseChecker(ref WoormParser lex, WoormDocument woorm)
		{
			this.lex = lex;
			this.localLex = false;
			this.woorm = woorm;
			this.checkOudDate = false;
		}
		
		//------------------------------------------------------------------------------
		private OutDateOperator StringToOperator(string outDateOperator)
		{
			if (outDateOperator != null)
				switch (outDateOperator.ToLower())
				{
					case "lt" : return OutDateOperator.LT;
					case "le" : return OutDateOperator.LE;
					case "eq" : return OutDateOperator.EQ;
					case "gt" : return OutDateOperator.GT;
					case "ge" : return OutDateOperator.GE;
				}

			return OutDateOperator.ND;
		}

		//----------------------------------------------------------------------------------------------
		private bool IsOutDate(OutDateOperator outDateOperator, int release)
		{
			if (outDateOperator == OutDateOperator.ND)
				return true;

			return 
				(outDateOperator == OutDateOperator.LT && reportModifyRelease <	 release)	||
				(outDateOperator == OutDateOperator.LE && reportModifyRelease <= release)	||
				(outDateOperator == OutDateOperator.EQ && reportModifyRelease == release)	||
				(outDateOperator == OutDateOperator.GT && reportModifyRelease >	 release)	||
				(outDateOperator == OutDateOperator.GE && reportModifyRelease >= release);
		}

		//------------------------------------------------------------------------------
		/// <summary>
		/// Parsa l'intero file in modo che li localize possa tradurre tutto. Restituisce
		/// un booleano che indica se posso girare sul web
		/// </summary>
		/// <returns></returns>
		private bool ParseRelease()
		{
			if (lex.LookAhead() == Token.NO_WEB)
			{
				NoWeb = true;
				lex.SkipToken();
			}
			if (lex.Matched(Token.INVALID))
			{
				lex.SetError(WoormViewerStrings.InvalidReport);
				return false;
			}
				
			if	(
				!lex.ParseTag (Token.RELEASE) ||
				!lex.ParseInt (out reportRelease)
				)
				return false;

			if (lex.Matched(Token.COMMA) && ! lex.ParseInt (out reportModifyRelease))
				return false;

			if (reportRelease < LowestCompatibleRelease || reportRelease > ActualRelease)
			{
				lex.SetError(WoormViewerStrings.BadRelease);
				return false;
			}

			//Warning in caso la release sia inferiore, in quanto non c'è compatibilità regressa completa
			if (reportRelease >= LowestCompatibleRelease && reportRelease < ActualRelease)
				lex.SetWarning(WoormViewerStrings.NoLastRelease);

			// non devo controllare la subRelease perchè il motore non gira ma prendo i dati da RDE (snapshot)
			// e quindi occorre solo controllare la release che indica la compatibilità di sintassi. 
			// Inoltre controllo solo quando parto dalla macchina a stati e non nei successivi reparse.
			// e quindi risparmio un accesso ad un file XML. Una ottimizzazione sarebbe quella di comportarci
			// come per le funzioni esterne che sono caricate in application on demand, ma mi sembra eccessivo
			if (woorm is RDEWoormDocument || !checkOudDate) 
				return true;

			if (!CheckOutDateReport(reportRelease, reportModifyRelease))
			{
				lex.SetError(WoormViewerStrings.OutDateRelease);
				return false;
			}

			return true;
		}

		//------------------------------------------------------------------------------
		private bool CheckOutDateReport(int reportRelease, int reportModifyRelease)
		{
			if ((woorm.Filename == null) || woorm.Filename == string.Empty)
				return true;

			INameSpace ns = woorm.ReportSession.PathFinder.GetNamespaceFromPath(woorm.Filename);
			string reportNamespace = ns.FullNameSpace;

			ModuleInfo mi = (ModuleInfo)woorm.ReportSession.PathFinder.GetModuleInfo(ns);
			if (mi == null)	return true;

			string path = mi.GetOutDateObjectsPath();
			if (!PathFinder.PathFinderInstance.ExistFile(path))
				return true;

			XmlDocument dom = new XmlDocument();
            dom = PathFinder.PathFinderInstance.LoadXmlDocument(dom, path);

			// cerca con XPath solo le funzioni con un dato nome per poi selezionare quella con i parametri giusti
			XmlNode root = dom.DocumentElement;
			string xpath = string.Format
				(
				"/{0}/{1}/{2}[@{3}]",
				OutDateObjectsXML.Element.OutDateObjects,
				OutDateObjectsXML.Element.Reports,
				OutDateObjectsXML.Element.Report,
				OutDateObjectsXML.Attribute.Namespace
				);

			// se non esiste la sezione allora il ReferenceObject è Undefined
			XmlNodeList outdates = root.SelectNodes(xpath);
			if (outdates == null) return true;

			foreach (XmlElement outdate in outdates)
			{
				// controllo che il namespace sia quello giusto in modalità CaseInsensitive
				string namespaceAttribute = outdate.GetAttribute(OutDateObjectsXML.Attribute.Namespace);
				if ((namespaceAttribute == null) || (string.Compare(namespaceAttribute, reportNamespace,StringComparison.OrdinalIgnoreCase) != 0))
					continue;

				// cerco solo tra le funzioni ammesse per woorm
				string release = outdate.GetAttribute(OutDateObjectsXML.Attribute.Release);
				if (release == null) return true;

				// cerco solo tra le funzioni ammesse per woorm
				OutDateOperator outDateOperator = StringToOperator(outdate.GetAttribute(OutDateObjectsXML.Attribute.Operator));
				if (IsOutDate(outDateOperator, XmlConvert.ToInt32(release)))
					return false;

				break;
			}

			// il report è compatibile
			return true;
		}

		//------------------------------------------------------------------------------
		public bool CheckNoWebAndRelease()
		{
			if (localLex)
				if (!lex.Open(woorm.Filename))
					return false;

			bool ok = ParseRelease();

			if (localLex)
				lex.Close();

			return ok;
		}

		//------------------------------------------------------------------------------
		~ReleaseChecker()
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
				// add sensitive resource garbage collection
			}
				
			disposed = true;         
		}
	}


	/// <summary>
	/// Summary description for Multi layouts.
	/// </summary>
	/// ================================================================================
	public class MultiLayout : Dictionary<string, Layout>
	{
		private WoormDocument woormdoc = null;
		public Layout Current = null;

		//---------------------------------------------------------------------------------
		public MultiLayout(WoormDocument woorm) : base(StringComparer.OrdinalIgnoreCase)
		{
			this.woormdoc = woorm;
			Current = new Layout(woormdoc);
			Add(Current.Name, Current);
		}

		//---------------------------------------------------------------------------------
		public Layout Add(string name)
		{
			Layout l = new Layout(woormdoc, name);
			Add(name, l);
			Current = l;
            return l;
        }

		//---------------------------------------------------------------------------------
		public bool SetCurrent(string name)
		{
            if (!ContainsKey(name))
                return false;
			Current = this[name];
			return true;
		}

        //---------------------------------------------------------------------------------
        internal void ApplyRepeater()
        {
            foreach (Layout layout in this.Values)
            {
                layout.ApplyRepeater();
            }
        }

        //---------------------------------------------------------------------------------
        internal void AddIDToDynamicStaticObjects()
        {
            foreach (Layout layout in this.Values)
            {
                layout.AddIDToDynamicStaticObjects();
            }
        }

        internal void AnchorFieldToColumn()
        {
            foreach (Layout layout in this.Values)
            {
                layout.AnchorFieldToColumn();
            }
        }
        internal void SortLayoutObjectsOnPosition()
        {
            foreach (Layout layout in this.Values)
            {
                layout.SortLayoutObjectsOnPosition();
            }
        }

    }

    /// <summary>
    /// Description of Report Template
    /// </summary>
    /// ================================================================================
    public class WoormTemplate
	{
		public string			NsTemplate = string.Empty;
		public WoormDocument	wrmTemplate = null;
        public static string 	CopyStaticObject_StyleName		= "_CopyObject_";
		public static string 	InheritStaticObject_StyleName	= "_InheritObject_";
		public static string 	CopiedStaticObject_StyleName	= "_CopiedObject_";

		public bool IsSavingTemplate { get; set; }

		public bool IsTemplate { get; set; }
	}

	/// <summary>
	/// Summary description for Woorm.
	/// </summary>
	/// ================================================================================
	public class WoormDocument : IDisposable, IWoormDocumentObj
	{
		public  TbReportSession     ReportSession;
		protected string		uniqueID = "";
		protected string		sessionID = "";
		
		private string pageFilename = string.Empty;
		private string pageFilenameWithoutExt = string.Empty;
		
		private MultiLayout		layouts = null;
		private PageInfo		pageInfo = new PageInfo();
		private Options			options = new Options();
		private Properties		properties = new Properties();
		private ConnectionLinks	connections	= null;
		protected FontStyles	fontStyles;
		protected FormatStyles	formatStyles;

		private OSLInfo			oslInfo = new OSLInfo();
		private string			graphicSection;
		private bool			noWeb = false;
		private string			description = "";
		private bool			templateError = false;
        public  bool            ForceVerticalAlignLabelRelative = true;

		protected string		filename;
		protected WoormParser	lex;
		protected bool			forLocalizer = false;

		public event EventHandler Disposed;

		private bool disposed = false;	// Track whether Dispose has been called.
		private bool loadAsTemplate = false;  //vuol dire che il woormDocument e' caricato come template
		private bool onlyGraphInfo = false;

		// correlati al caricamento di dati dal RdeReader;
		public	int					CurrentPage = 1;	
		public	StringCollection	Messages = new StringCollection();
		public	RdeReader			RdeReader;
		public	ReleaseChecker		ReleaseChecker;
		public	INameSpace			Namespace = new NameSpace("");
		public	WoormTemplate       Template = new WoormTemplate();

        public  List<string>        LayoutTemplateSendedToClient = new List<string>();

        // properties
        public Layout Objects							{ get { return layouts.Current; } }
		public bool ReplaceHiddenWhenExpr				{ get; set; }

		public WoormParser		Lex						{ get { return lex;}}
		public FontStyles		FontStyles				{ get { return fontStyles;}}
		public FormatStyles		FormatStyles			{ get { return formatStyles;} set { formatStyles = value; }}
		public bool				TemplateError			{ get { return templateError; } }
		public Properties		Properties				{ get { return properties; }}
		public string			Filename				{ get { return filename; }}
		public string			UniqueID				{ get { return ReportSession.uniqueID; }}
		public string			SessionID				{ get { return ReportSession.sessionID; }}
		public ILocalizer		Localizer				{ get { return ReportSession.Localizer; } }
		public IDiagnostic		Diagnostic				{ get { return lex.Diagnostic; }}
		public Options			Options					{ get { return options; }}
		public bool				NoWeb					{ get { return noWeb; }}
		public ConnectionLinks	Connections				{ get { return connections; }}

		// descrizione estesa del report (utile ad esempio al MenuManager);
		public string			Description				{ get { return description; }}
		public string			LocalizedDescription	{ get { return Localizer.Translate(description); }}		
		public bool				ForLocalizer			{ get { return forLocalizer; } set { forLocalizer= value; }}
		public SymbolTable		SymbolTable				{ get { return RdeReader.RdeSymbolTable; } }
		public PageInfo			PageInfo				{ get { return pageInfo; } }
 
		///<summary>
		///nome del file che viene scritto a inizio esecuzione e che contiene la viewsymboltable
		///del report (alias, nome variabili, tipo, attributi che dicono se e' colonna, nascosto, ecc..)
		///</summary>
		//---------------------------------------------------------------------------
		public virtual string InfoFilename
		{
			get
			{
				if (pageFilename == string.Empty)
					pageFilename = PathFunctions.WoormTempFilename(SessionID, UniqueID, Filename);
				
				return pageFilename;
			}
		}
		
		///<summary>
		///nome del file che viene scritto a esecuzione completata e che contiene il numero totale di 
		///pagine del report
		///</summary>
		//---------------------------------------------------------------------------
		public virtual string TotPageFilename
		{
			get
			{
				if (pageFilename == string.Empty)
					pageFilename = PathFunctions.WoormTempFilename(SessionID, UniqueID,Filename); 
				
				return PathFunctions.TotPageFilename(pageFilename);
			}
		}
				
		//---------------------------------------------------------------------------
		public string GraphicSection
		{
			get
			{
				if (graphicSection == null)
				{
					StringBuilder sb = new StringBuilder();
					Parser lex = new Parser(Parser.SourceType.FromFile);
					lex.EnableAudit = true;
					if (lex.Open(Filename))
					{
						while (!lex.Eof && lex.LookAhead() != Token.REPORT)
							lex.SkipToken();		
					}
					graphicSection = lex.GetAuditString(true);
				}

				return graphicSection;
			}
		}

 		//---------------------------------------------------------------------------
		public virtual string CurrentRdeFilename(int pageNo)
		{
			if (pageFilenameWithoutExt == string.Empty)
			{
				pageFilenameWithoutExt = PathFunctions.WoormTempFilePath(SessionID,UniqueID) +
								NameSolverStrings.Directoryseparetor +
								Path.GetFileNameWithoutExtension(Filename);
			}
			return pageFilenameWithoutExt + pageNo.ToString() + ".xml";
		}

		//------------------------------------------------------------------------------
		public WoormDocument(string filename, TbReportSession session)
			:this(filename,session,false/*default non e' template*/)
		{
		}

		//------------------------------------------------------------------------------
		public WoormDocument(string filename, TbReportSession session, bool loadAsTemplate)
		{
			this.loadAsTemplate = loadAsTemplate;
			Init(session);
	
			this.filename	= filename;
			this.Namespace  = ReportSession.PathFinder.GetNamespaceFromPath(filename);
	
			lex = new WoormParser(Parser.SourceType.FromFile);
			ReleaseChecker  = new ReleaseChecker(ref lex, this);

			//localizer = new WoormLocalizer(filename, ReportSession.PathFinder);
		}

		// mettere qui le inizializzazioni comuni a WoormDocument e RDEWoormDocument
		//------------------------------------------------------------------------------
		protected void Init(TbReportSession session)
		{
			this.ReportSession	= session;

			layouts			= new MultiLayout(this);
			connections		= new ConnectionLinks(this);
			fontStyles		= new FontStyles();
			formatStyles	= new FormatStyles(this.ReportSession.ApplicationFormatStyles);
			RdeReader		= new RdeReader(this);

            object setting1 = ReadSetting.GetSettings(session.PathFinder, "Framework.TbWoormViewer.Woorm", "WoormGeneralOptions", "ForceVerticalAlignLabelRelative", true);
            this.ForceVerticalAlignLabelRelative = ObjectHelper.CastBool(setting1);
		}

		//------------------------------------------------------------------------------
		protected WoormDocument()
		{
		}

		//------------------------------------------------------------------------------
		~WoormDocument()
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
				//TODO RSWEB per debug lascio i temporanei - PathFunctions.DeleteTempData(SessionID, UniqueID);
				if (Disposed != null)
					Disposed(this, EventArgs.Empty);
			}

			disposed = true;         
		}

		//------------------------------------------------------------------------------
		public void LoadPage(PageType page)
		{
			RdeReader.LoadPage(page);
		}

		//------------------------------------------------------------------------------
		public void LoadPage(int pageNumber)
		{
             RdeReader.LoadPage(pageNumber);
		}

		//------------------------------------------------------------------------------
		public void NewPage()
		{
			Messages.Clear();

            Objects.ClearData();
		}

		//------------------------------------------------------------------------------
		private bool ParseLayouts (WoormParser Lex)
		{
			if (!Lex.LookAhead(Token.PAGE_LAYOUT))
				return ParseObjects(lex, false);
			
			string layoutName = Layout.DefaultName;
			while (Lex.LookAhead(Token.PAGE_LAYOUT))
			{
				if (!Lex.ParseTag(Token.PAGE_LAYOUT) || !Lex.ParseString(out layoutName))
				{
					Lex.SetError(WoormViewerStrings.PageLayOutNotFound);
					return false;
				}

                bool invertOrientation = lex.Matched(Token.INVERT_ORIENTATION);

                Lex.ParseBegin();
			
				if (!layouts.SetCurrent(layoutName))
				{
					layouts.Add(layoutName).Invert = invertOrientation;
					//automatico layouts.SetCurrent(layoutName)
				}
				if (!ParseObjects(lex, true))
					return false;
			}
			layouts.SetCurrent(Layout.DefaultName);

            layouts.ApplyRepeater();
            layouts.AddIDToDynamicStaticObjects();
            layouts.AnchorFieldToColumn();
            layouts.SortLayoutObjectsOnPosition();  //RSWEB TODO
            return true;
		}

		//------------------------------------------------------------------------------
		private bool ParseObjects(WoormParser Lex, bool hasPageLayout)
		{
			BaseObj baseObject;
			Token token;
			while (true)
			{
				token = Lex.LookAhead();
				switch (token)
				{
					case Token.TEXT     : baseObject = new TextRect     (this); break;
					case Token.FIELD    : baseObject = new FieldRect    (this); break;

					case Token.TABLE    : // parse only dimensions
					{
						int row, col = 1;
						if  (Lex.ParseTable (out row, out col))
						{
							baseObject = new Table (this, row, col);
							break;
						}
						return false;
					}
                    case Token.REPEATER: 
                    {
                        baseObject = new Repeater(this); break;
                    }
                    case Token.CHART:
                    {
                        baseObject = new Chart(this); break;
                    }
                    case Token.GAUGE:
                        {
                            baseObject = new Gauge(this); break;
                        }

                    case Token.RNDRECT  : // mantiene la compatibilita' con il passato
					case Token.SQRRECT  : baseObject = new SqrRect      (this); break;
					case Token.METAFILE : baseObject = new GraphRect    (this); break;
					case Token.BITMAP   : baseObject = new GraphRect    (this); break;
					case Token.FILE     : baseObject = new FileRect     (this); break;

					case Token.END			: return hasPageLayout && Lex.ParseEnd(); 
                    case Token.LINKS		: return true;
					case Token.REPORT		: return true;
					case Token.PROPERTIES	: return true; 
					case Token.EOF			: return true;
					default: Lex.SetError(WoormViewerStrings.ObjectNotFound); return false;
				}

				if (!baseObject.Parse(Lex))
					return false;

                //skip EABarcode
                if (baseObject is TextRect ? ((TextRect)baseObject).InternalID == SpecialReportField.ID.EA_BARCODE : false)
                    continue;

				// trovato elemento, delega a lui la lettura dei suoi dati
				Objects.Add(baseObject);
			}
		}

		//------------------------------------------------------------------------------
		private bool ParseHeadData(WoormParser Lex) 
		{
			if (!ReleaseChecker.CheckNoWebAndRelease())
				return false;

			// exit from loop for any keyword different from
			// RECT, MAXIMIZED, MINIMIZED, READ_ONLY, ONLYGRAPHINFO, DESCRIPTION
			for (;;)
			{
				switch (Lex.LookAhead())
				{
					case Token.RECT:
					{
						Rectangle rect;
						if (!Lex.ParseRect(Token.RECT, out rect))
							return false;

						pageInfo.Rect = rect;
						break;
					}
	
					case Token.MAXIMIZED:
					case Token.MINIMIZED:
					case Token.ONLY_GRAPH:
						Lex.SkipToken();
						break;

					case Token.TEMPLATE:
						Lex.SkipToken();
                        if (Lex.LookAhead(Token.TEXTSTRING))
                        {
                            if (!Lex.ParseString(out Template.NsTemplate))
                                return false;
                        }
						break;

					default:
						return true;
				}
			}		
		}	

		//------------------------------------------------------------------------------
		private bool ParseOSLInfo (WoormParser Lex) 
		{
			if (!Lex.LookAhead(Token.UUID)) 
				return true;

			string guid;
			Lex.SkipToken();
			if	(!Lex.ParseString(out guid))
				return false;	
	
			oslInfo.Guid = guid;
			oslInfo.EType = OSLTypes.Report;

			// come nick prendo il nome del report
			oslInfo.NickName = filename;
			OSLInterface.ObjectGrant (oslInfo);

			return true;
		}

		//------------------------------------------------------------------------------
		private bool ParseConnections(WoormParser Lex)
		{
			return connections == null ? true : connections.Parse(Lex);
		}

		//------------------------------------------------------------------------------
		private bool ParseProperties(WoormParser Lex)
		{
			return properties.Parse(Lex);
		}

		//------------------------------------------------------------------------------
		private bool ParseViewInfo(WoormParser Lex)
		{
			return
				fontStyles.Parse(Lex, ReleaseChecker.ReportRelease, Namespace, loadAsTemplate) &&
				formatStyles.Parse(Lex, false, Namespace, Formatter.FormatSource.WOORM)&&
				ParseLayouts(Lex);
		}

		//------------------------------------------------------------------------------
		private bool ParsePageInfo(WoormParser Lex) 
		{
			return pageInfo.Parse(Lex);
		}

		//------------------------------------------------------------------------------
		private bool ParseOptions(WoormParser Lex) 
		{
			Lex.Matched(Token.ONLY_GRAPH);
			return options.Parse(Lex);
		}

		//------------------------------------------------------------------------------
		public virtual bool ParseDocument()
		{
			if (!Lex.Open(filename))
				return false;

			return ParseWoormDocument();

		}
		//------------------------------------------------------------------------------
		public virtual bool LoadDocument() 
		{
			RdeReader.LoadInfo();
			return true;
		}

        //------------------------------------------------------------------------------
        public void UpdateSymbolTable(FieldSymbolTable symTable)
        {
            foreach (Field field in symTable)
            {
                if (field.InternalId <= 0 || field.IsSubTotal) continue;
                if (field.Ask || field.Input)
                {
                    Variable v = this.SymbolTable.FindById(field.Id);
                    v.SetAllData(field.Data, true);
                }
            }
        }

        //------------------------------------------------------------------------------
        protected bool ParseWoormDocument()
		{
			bool ok =	ParseHeadData		(Lex)&&
						ParseOSLInfo		(Lex)&&
						ParseProperties		(Lex)&&
						ParsePageInfo		(Lex)&&
						ParseOptions		(Lex)&&
						ParseViewInfo		(Lex)&&
						ParseConnections	(Lex);

			if (!string.IsNullOrEmpty(Template.NsTemplate) && !ForLocalizer)
				templateError = LoadTemplate();
			
			Lex.Close();
			
			return ok;
		}

		//---------------------------------------------------------------------
		internal void Unparse(Unparser unparser)
		{
			//TODOLUCA
			unparser.SaveAsWithCurrentLanguage = false; // m_bSaveAsWithCurrentLanguage;
			//unparser.OriginFileName = m_strOriginFileName;
			//unparser.OriginDictionaryPath = m_strOriginDictionaryPath;

			UnparseHeadData(unparser, onlyGraphInfo);

			if (!Template.IsSavingTemplate)
				UnparseProperties(unparser);

			UnparsePageInfo(unparser);
				
			if (!Template.IsSavingTemplate)
			{
				// scrive la stampante preferenziale
				Options.Unparse(unparser);
				Options.SaveDefault(Namespace);
			}

			fontStyles.Unparse(unparser, Namespace, FontElement.FontSource.WOORM);

			if (!Template.IsSavingTemplate)
				formatStyles.Unparse(unparser, Namespace, Formatter.FormatSource.WOORM);
			
			// write all elements
			UnparseLayouts(unparser);

			if (!Template.IsSavingTemplate)
				connections.Unparse(unparser);
		}

		//------------------------------------------------------------------------------
		private void UnparseLayouts(Unparser unparser)
		{
			RemoveInheritObjects();

			bool thereIsTemplate = false;
			foreach (Layout currentLayout in layouts.Values)
			{
				if (currentLayout.Count <= currentLayout.CountAutoObjects &&
					currentLayout.Name.CompareNoCase(SpecialReportField.REPORT_DEFAULT_LAYOUT_NAME))
					continue;

				//if (m_pWoormIni->m_bSortObjects)		//TODOLUCA
				//    SortObjectOnPosition(pObjects);

			    unparser.WriteTag(Token.PAGE_LAYOUT, false);
			    unparser.WriteString(currentLayout.Name);

				unparser.IncTab();
			    unparser.WriteBegin();
				unparser.IncTab();

			    if (!Template.IsSavingTemplate)
			        RemoveTemplateStyles();

			    // write all elements CBaseObjArray* pObjects
				currentLayout.Unparse(unparser, Template.IsSavingTemplate, ref thereIsTemplate);

			    if (!Template.IsSavingTemplate)
			        ApplyTemplateStyles();

				unparser.DecTab();
				unparser.WriteEnd();
				unparser.DecTab();
			}
			if (thereIsTemplate && Template.IsSavingTemplate && Template.wrmTemplate != null)
			    UnparseInheritTemplateObjects(unparser);

			AddInheritObjects();
		}

		//-----------------------------------------------------------------------------
		private void UnparseInheritTemplateObjects(Unparser unparser)
		{
			//TODOLUCA
			int offset = 0; // = m_pEditorManager->GetNextId() + 100;

			foreach (Layout layout in Template.wrmTemplate.layouts.Values)
			{
				unparser.WriteTag(Token.PAGE_LAYOUT, false);
				unparser.WriteString("_TPL_" + layout.Name);
				unparser.WriteBegin();

				//write all elements
				foreach (BaseObj obj in layout)
				{
					if (obj is Table)
					{
						if (((Table)obj).TemplateOverridden)
							continue;

						Table pTable = new Table((Table)obj);
						pTable.PurgeTemplateColumns();

						pTable.RenameAlias(offset);
						pTable.Unparse(unparser);
						continue;
					}
					else if (obj is BaseRect)
					{
						BaseRect baseObj = obj as BaseRect;
						if (!baseObj.IsTemplate)
							continue;

						if (baseObj.TemplateOverridden)
							continue;

						baseObj.RenameAlias(offset);
					}

					obj.Unparse(unparser);
				}

				unparser.WriteEnd();
			}
			
		}

		//-----------------------------------------------------------------------------
		internal void ClearCustomStyles(BaseObjList objects)
		{
			Table table = null;
			foreach (BaseObj obj in objects)
			{
				if (obj is Table)
				{
					table = obj as Table;
					table.ClearAllStyles();
				}
				else if (obj is BaseRect)
				{
					BaseRect pF = obj as BaseRect;
					pF.ClearStyle();
				}
			}
		}

		//-----------------------------------------------------------------------------
		private void RemoveTemplateStyles()
		{
			Table table;
			for (int i = Objects.Count - 1; i >= 0; i--)
			{
				BaseObj obj = Objects[i];

				if (obj is Table)
				{
					table = obj as Table;
					table.RemoveAllStyles();
				}
				else if (obj is BaseRect)
				{
					BaseRect pF = obj as BaseRect;

					if (pF.ClassName.CompareNoCase(WoormTemplate.CopyStaticObject_StyleName)) 
						Objects.RemoveAt(i);
					else
						pF.RemoveStyle();
				}
			}
		}

		//-----------------------------------------------------------------------------
		void RemoveInheritObjects ()
		{
			//TODOLUCA
			//if (m_pMultipleSelObj) 
			//    OnDeselectAll();
			
			//m_pActiveRect->Clear();
			//m_pCurrentObj = NULL;
			foreach (Layout item in layouts.Values)
			{
				for (int i = item.Count - 1; i >= 0; i--)
				{
					BaseRect baseRect = item[i] as BaseRect;
					if (baseRect == null)
						continue;

					if (!baseRect.InheritByTemplate)
						continue;

					baseRect.DeleteEditorEntry();

					item.RemoveAt(i);
				}
			}
		}

		//------------------------------------------------------------------------------
		private void UnparseHeadData(Unparser unparser, bool onlyGraphInfo)
		{
			// calculate  frame dimension for saving them
			Rectangle rect = pageInfo.Rect;
			
			//TODOLUCA
			//CWoormFrame* pParentFrame = GetWoormFrame();
			//CRect rect;
			//pParentFrame->GetWindowRect(rect);

			// write header and m_nRelease number
			unparser.WriteHeader();

			if (noWeb)
				unparser.WriteTag(Token.NO_WEB, false);

			unparser.WriteTag(Token.RELEASE, false);
			unparser.Write(ReleaseChecker.ReportRelease, false);

			if (ReleaseChecker.ReportModifyRelease > 0)
			{
				unparser.WriteTag(Token.COMMA, false);
				unparser.Write(ReleaseChecker.ReportModifyRelease, false);
			}

			unparser.WriteBlank();

			// write window dimension //TODOLUCA
			//if (pParentFrame->IsIconic()) 
			//    unparser.UnparseTag(T_MINIMIZED, FALSE);
			//else if (pParentFrame->IsZoomed()) 
			//    unparser.UnparseTag(T_MAXIMIZED, FALSE);
			//else 
				unparser.WriteRect(Token.RECT, rect, false);

			// signal that only graph info will be saved
			if (onlyGraphInfo)
				unparser.WriteTag(Token.ONLY_GRAPH, false);

			if (!Template.IsSavingTemplate && !Template.NsTemplate.IsNullOrEmpty())
			{
				unparser.WriteTag(Token.TEMPLATE, false);
				unparser.WriteString(Template.NsTemplate, false);
			}
			else if (Template.IsTemplate)
				unparser.WriteTag(Token.TEMPLATE, false);

			unparser.WriteLine();
		}

		//------------------------------------------------------------------------------
		private void UnparsePageInfo(Unparser unparser)
		{
			if (!pageInfo.IsDefault) 
			{
				pageInfo.Unparse(unparser);
				unparser.WriteLine();
			}
		}

		//------------------------------------------------------------------------------
		private void UnparseProperties(Unparser unparser)
		{
			if (properties != null && !properties.IsEmpty) 
			{
				properties.Unparse(unparser);
				unparser.WriteLine();
			}
		}

		// il nome di un file non è da localizzare, ma costituisce un reference ad un oggetto da 
		// localizzare (il file) e viene memorizzato in localizableReferences
		//------------------------------------------------------------------------------
		public void GetLocalizableStrings
			(
				List<String> graphicLocalizableStrings,
			    List<String> functionLocalizableStrings, 
				List<String> localizableReferences
			)
		{
			functionLocalizableStrings.AddRange(SymbolTable.LocalizableStrings);

			//devo ciclare sui possibili layout del report, ed estrarre le stringhe dagli oggetti di ogni layout
			foreach (Layout layout in layouts.Values)
			{
				foreach (BaseObj obj in layout)
				{
					if (obj is FileRect) localizableReferences.Add(((FileRect)obj).Label.Text);
					else if (obj is FieldRect) graphicLocalizableStrings.Add(((FieldRect)obj).Label.Text);
					else if (obj is TextRect) graphicLocalizableStrings.Add(((TextRect)obj).Label.Text);
					else if (obj is Table)
					{
						graphicLocalizableStrings.Add(((Table)obj).Title.Text);
						foreach (Column columnn in ((Table)obj).Columns)
							graphicLocalizableStrings.Add(columnn.Title.Text);
					}
				}
			}

			if (!string.IsNullOrEmpty(Description))
				graphicLocalizableStrings.Add(Description);
			if (Properties != null)
			{
				if (!string.IsNullOrEmpty(Properties.Title))
					graphicLocalizableStrings.Add(Properties.Title);
				if (!string.IsNullOrEmpty(Properties.Subject))
					graphicLocalizableStrings.Add(Properties.Subject);
				if (!string.IsNullOrEmpty(Properties.Comments))
					graphicLocalizableStrings.Add(Properties.Comments);
			}
		}		

		// identifica il dato proveniente dall'RDE dei campi tabellari
		//------------------------------------------------------------------------------
		public string GetFormattedDataFromAlias(ushort	alias, int nRow)
		{
			foreach (BaseObj obj in Objects)
				if (obj is Table)		
				{
					foreach(Column columnn in ((Table)obj).Columns)
						if (columnn.InternalID == alias)
							return columnn.Cells[nRow].Value.FormattedData;
				}
			
			return string.Empty;
		}		

		// identifica il dato proveniente dall'RDE dei campi non tabellari
		//------------------------------------------------------------------------------
		public string GetFormattedDataFromAlias(ushort alias)
		{
            foreach (BaseObj obj in Objects)
            {
                if (obj is FieldRect)
                {
                    FieldRect fieldRect = (FieldRect) obj;
                    if (fieldRect.AnchorRepeaterID != 0)
                        continue;

                    if (fieldRect.InternalID == alias)
                        return fieldRect.Value.FormattedData;
                }
                else if (obj is Repeater)
                {
                    Repeater rep = obj as Repeater;
                    FieldRect fieldRect = rep.FindField(alias);
                    if (fieldRect != null)
                        return fieldRect.Value.FormattedData;
                }
            }
			return string.Empty;
		}		

		//------------------------------------------------------------------------------
        public object GetRDEData(ushort alias, int row, out bool found, out bool tail)
		{
            tail = false;
			found = false;
			foreach (BaseObj obj in Objects)
			{
				if (obj is FieldRect)
				{
                    if (row >= 0)
                        continue;

					FieldRect fieldRect = (FieldRect) obj;
					if (fieldRect.InternalID == alias)
					{
						if (fieldRect.Value.RDEData == null)
						{
							return null;
						}
						else
						{
                            found = fieldRect.Value.RDEData != null; 
                            tail = fieldRect.Value.CellTail;
							return fieldRect.Value.RDEData;
						}
					}
				}
				else if (obj is Table)		
				{
                    Table t = obj as Table;
 
                    foreach (Column columnn in t.Columns)
                    {
                        if (columnn.InternalID == alias)
                        {
                            t.ViewCurrentRow = row;

                            if (row == -1)
                            {
                                if (!columnn.ShowTotal)
                                {
                                    found = false;
                                    return null;
                                }

                                found = columnn.TotalCell.Value.RDEData != null; 
                                tail = columnn.TotalCell.Value.CellTail;
                                return columnn.TotalCell.Value.RDEData;
                            }
 
                            if (row >= columnn.Cells.Count || columnn.Cells[row].Value.RDEData == null)
                            {
                                return null;
                            }
                            else
                            {
                                found = columnn.Cells[row].Value.RDEData != null; 
                                tail = columnn.Cells[row].Value.CellTail;
                                return columnn.Cells[row].Value.RDEData;
                            }
                        }
                    }
				}
                else if (obj is Repeater && row >= 0)
                {
                    Repeater rep = obj as Repeater;
                    rep.ViewCurrentRow = row;

                    FieldRect f = rep.FindField(alias, row);
                    if (f != null)
                    {
                        rep.ViewCurrentRow = row;

                        if (f.Value.RDEData == null)
                        {
                            return null;
                        }
                        else
                        {
                            found = f.Value.RDEData != null; 
                            tail = f.Value.CellTail;
                            return f.Value.RDEData;
                        }
                    }
                }
			}
			return null;
		}

		//------------------------------------------------------------------------------
		public void SynchronizeSymbolTable(int row = -1, bool updateOnlyTailCell = false)
		{
            bool found, tail;
			foreach (Variable v in SymbolTable)
			{
                object dataObject = GetRDEData(v.Id, row, out found, out tail);

                if (updateOnlyTailCell && (!found || !tail))
                    continue;

				if (found)
				{
					v.Data = dataObject;
					v.Valid = true;
				}
				else if (row > -1 && v.IsColumn2)
				{
					v.Valid = false;
				}
			}
		}
		
		/// <summary>
		/// Carica il template caricandolo in memoria come woormdocument
		/// </summary>
		//---------------------------------------------------------------------
		private bool LoadTemplate()
		{
			//ottengo il path del file a partire dal namespace, cercando prima nella custom, poi nella standard
			string file = "";
			NameSpace ns = new NameSpace(Template.NsTemplate);
			file = PathFinder.PathFinderInstance.GetCustomReportFullNameFromNamespace(ns, ReportSession.UserInfo.Company, ReportSession.UserInfo.User);
			if (string.IsNullOrEmpty(file) || !PathFinder.PathFinderInstance.ExistFile(file))
				file = PathFinder.PathFinderInstance.GetStandardReportFullNameFromNamespace(ns);
			if (string.IsNullOrEmpty(file) || !PathFinder.PathFinderInstance.ExistFile(file))
			{
				Diagnostic.SetError(string.Format("{0} {1}", WoormViewerStrings.ErrorReadingTemplate, file));
				return false;
			}

			//istanzio il template
			Template.wrmTemplate = new WoormDocument(file, ReportSession, true);
			Template.wrmTemplate.ForLocalizer = true; // inibisce il type checking e la valutazione delle espressioni di "hidden when"
			
			bool loaded = Template.wrmTemplate.LoadDocument() && Template.wrmTemplate.ParseDocument();
			if (!loaded)
			{
				Diagnostic.SetError(WoormViewerStrings.ErrorLoadingTemplate);
				return false;
			}

			//trasferico eventuali fonts dal template al woormdocument, in modo che possano essere usati..
			ImportTemplateFonts();

            if (Options.BkgnBitmap.IsNullOrEmpty())
            {
                Options.BkgnBitmap = Template.wrmTemplate.Options.BkgnBitmap;
                Options.BitmapOrigin = Template.wrmTemplate.Options.BitmapOrigin;
            }

			//prelevo gli stili dal template e li applico agli oggetti del woormDocument
			ApplyTemplateStyles();
			return true;
		}

        /// <summary>
        /// Attenzione che in caso di
        /// </summary>
        //---------------------------------------------------------------------
		private void AddInheritObjects()
		{
			if (Template.wrmTemplate == null)
				return;

			foreach (Layout l in layouts.Values)
			{
				int idx = 0;
				foreach (Layout lt in Template.wrmTemplate.layouts.Values)
				{
					foreach (BaseObj obj in lt)
					{
						if (!(obj is BaseRect))
							continue;
						
						BaseRect br = obj as BaseRect;
						if (!br.IsTemplate)
							continue;

						if (br.ClassName.CompareNoCase(WoormTemplate.InheritStaticObject_StyleName))
						{
							br.InheritByTemplate = true;

							if (br is GraphRect)
								l.Insert(idx++, br);
							else if (br is SqrRect)
								l.Insert(idx++, br);
							else if (br is FileRect)
								l.Insert(idx++, br);
							else if (br is TextRect)
								l.Insert(idx++, br);
						}
					}
				}
			}
		}

		/// <summary>
		/// Cicla sugli oggetti di ogni layout del Woormdocument per applicargli lo style
		/// </summary>
		//---------------------------------------------------------------------
		private void ApplyTemplateStyles()
		{
			foreach (Layout l in layouts.Values)
				foreach (BaseObj obj in l)
					ApplyTemplateStyles(obj);
		}

		/// <summary>
		/// Applica all'oggetto il corrispettivo style del template (se presente nel template)
		/// </summary>
		//---------------------------------------------------------------------
		private void ApplyTemplateStyles(BaseObj obj)
		{
			SetTemplateStyle(obj);

            if (obj is Table)
            {
                Table table = obj as Table;
                if (table != null)
                {
                    foreach (Column col in table.Columns)
                        SetTemplateStyle(col);
                }
            }
            else if (obj is Repeater)
            {
                Repeater rep = obj as Repeater;
                if (rep != null)
                {
                    for (int r = 1; r < rep.Rows.Count; r++)
                    {
                        BaseObjList rl = rep.Rows[r];

				        foreach (BaseObj ro in rl)
					        SetTemplateStyle(ro);
                    }
                }
            }
        }

		/// <summary>
		/// Applica all'oggetto il corrispettivo style del template (se presente nel template)
		/// </summary>
		//------------------------------------------------------------------------------
		private bool SetTemplateStyle(Object obj)
		{
			if (Template.wrmTemplate == null)
				return false;
			
			MultiLayout layoutsTemplate = Template.wrmTemplate.layouts;
			foreach (Layout layoutTemplate in layoutsTemplate.Values)
			{
				for (int i = 0; i < layoutTemplate.Count; i++)
				{
					BaseObj templateObject = layoutTemplate[i];

					if (obj is BaseRect && templateObject is BaseRect)
					{
						BaseRect rect = obj as BaseRect;
						BaseRect templateRect = templateObject as BaseRect;

						if (templateRect.IsTemplate == false)
							continue;

						//TODO chiedere
						if (obj.GetType() != templateObject.GetType())
							continue; 

						if (!(string.Compare(rect.ClassName, templateRect.ClassName, true) == 0))
							continue;

						rect.SetStyle(templateRect);

						return true;
					}
					else if (obj is Table && templateObject is Table)
					{
						Table table = obj as Table;
						Table templateTable = templateObject as Table;

						if (templateTable.IsTemplate == false)
							continue;

						if (!(string.Compare(table.ClassName, templateTable.ClassName, true) == 0))
							continue;

						table.SetStyle(templateTable);

						return true;
					}
					else if (obj is Column && templateObject is Table)
					{
						Column column = obj as Column;
						Table templateTable = templateObject as Table;

						foreach (Column col in templateTable.Columns)
						{
							if (!col.IsTemplate || !(string.Compare(column.ClassName, col.ClassName, true) == 0))
								continue;

							column.SetStyle(col);
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Cambia il layout del report sostitunedo l'array degli oggetti grafici al woormDocument
		/// </summary>
		//------------------------------------------------------------------------------
		public void ApplyLayout(string layoutName)
		{
			//se sono gia sul layout che si vuole applicare, non faccio nulla
			if (string.Compare(layouts.Current.Name, layoutName) == 0)
				return;

			//se la stringa passata e' vuota viene applicato il layout di default
			layouts.SetCurrent
				(
				string.IsNullOrEmpty(layoutName)
					? Layout.DefaultName
					: layoutName
				);
		}

        private string dbCompanyName = string.Empty;
		//---------------------------------------------------------------------
		public string DBCompanyName 
		{ 
            get 
            {
                if (dbCompanyName == string.Empty)
                    dbCompanyName = GetDBCompanyName();
                
				return dbCompanyName; 
            } 
        }

		//---------------------------------------------------------------------
		private string GetDBCompanyName()
		{
			string companyName = string.Empty;
			try
			{
                //TODO rsweb
				//using (TBConnection conn = new TBConnection(ReportSession.CompanyDbConnection, TBDatabaseType.GetDBMSType(ReportSession.Provider)))
				//{
				//	conn.Open();
				//	using (TBCommand cmd = new TBCommand(conn))
				//	{
				//		cmd.CommandTimeout = 0;
				//		cmd.CommandText = "SELECT CompanyName FROM MA_Company";
				//		using (IDataReader reader = cmd.ExecuteReader())
				//		{
				//			if (reader.Read())
				//				companyName = reader[0] as string;
				//		}
				//	}
				//}
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
			}
			return companyName;
		}

		//====================================================================================================
		public int CalculateFieldWidth(int objectId, string strText)
		{
			for (int i = layouts.Current.Count - 1; i >= 0; i--)
			{
				Table obj = layouts.Current[i] as Table;
				if (obj != null)
				{
					if (obj.InternalID == objectId)
						break;

					int idx = obj.ColumnIndexFromID(objectId);
					if (idx != -1)
						return obj.Columns[idx].CalculateFieldWidth(strText);
				}
			}
			//colonna non trovata
			Debug.Assert(false, string.Format("Function 'CalculateFieldWidth' does not support the kind of object associated with alias {0}",objectId));
			return 0;
		}

        //---------------------------------------------------------------------
        public bool GetFieldWidthFactors(int objectId, ref FieldWidthFactors fwf, bool isSubTotal = false)
        {
            for (int i = layouts.Current.Count - 1; i >= 0; i--)
            {
                Table obj = layouts.Current[i] as Table;
                if (obj != null)
                {
                    if (obj.InternalID == objectId)
                        break;

                    int idx = obj.ColumnIndexFromID(objectId);
                    if (idx != -1)
                        return obj.Columns[idx].GetFieldWidthFactors(ref fwf, isSubTotal);
                }
            }
            //colonna non trovata
            Debug.Assert(false, string.Format("Function 'GetFieldWidthFactors' does not support the kind of object associated with alias {0}", objectId));
            return false;
        }

		//---------------------------------------------------------------------
		internal FontElement GetFontElementFromTemplate(string fontStyleName, INameSpace nameSpace)
		{
			if (Template == null || Template.wrmTemplate == null || Template.wrmTemplate.fontStyles.Count <= 0)
				return null;

			return Template.wrmTemplate.fontStyles.GetFontElement(fontStyleName, nameSpace);
		}

		//------------------------------------------------------------------------------
		internal FontElement GetFontElement(string fontStyleName)
		{
			FontElement fe = FontStyles.GetFontElement(fontStyleName, Namespace);
			if (fe == null)
				fe = GetFontElementFromTemplate(fontStyleName, Namespace);
			if (fe == null)
				fe = ReportSession.ApplicationFontStyles.GetFontElement(fontStyleName, Namespace);
			return fe;
		}

		/// <summary>
		/// Cicla sui fonts del template, se questi non sono presenti tra i fonts del woormdocument li aggiunge
		/// </summary>
		//---------------------------------------------------------------------
		private void ImportTemplateFonts()
		{
			if (Template.wrmTemplate == null)
				return;

			FontStyles templateFonts = Template.wrmTemplate.FontStyles;

			//ciclo sui font del template
			foreach (string keyTemplateFont in templateFonts.Keys)
			{
				//se c'e' gia un font con lo stesso nome nel woormDocument, prevale quello del documento
				if (FontStyles.ContainsKey(keyTemplateFont))
					continue;

				//altrimenti prelevo il font dal template e lo aggiungo ai fonts del documento
				fontStyles.Add(keyTemplateFont, templateFonts[keyTemplateFont]);
			}
		}

        //---------------------------------------------------------------------------
        public CultureInfo GetCollateCultureFromId(ushort id)
        {
            Variable v = this.SymbolTable.FindById(id);
            return v == null ? CultureInfo.InvariantCulture : v.CollateCulture;
        }

        ///<summary>
        ///Riformatta dinamicamente il dato 
        /// </summary>
        public string FormatFromSoapData(string formatStyleName, ushort ID, object data)
        {
            CultureInfo collateCulture = GetCollateCultureFromId(ID);
            //vedi RDEReader
            return this.FormatStyles.FormatFromSoapData(formatStyleName, data, this.Namespace, collateCulture);
        }

        //---------------------------------------------------------------------
        public string ToJson(bool template, string name = "page", bool bracket = true, string reportTitle="")
        {
            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            int pageNo = 1;
            if (this.RdeReader != null)
            {
                pageNo = this.RdeReader.CurrentPage;
            }
            else if (!template)
            {
                //TODO errore
                return string.Empty;
            }
            if (this.pageInfo == null)
            {
                //TODO errore
                return string.Empty;
            }

            bool fullTemplate = template;
            if (template)
            {
                if (LayoutTemplateSendedToClient.IndexOf(this.Objects.Name) > -1)
                {
                    fullTemplate = false;
                }
                else
                {
                    LayoutTemplateSendedToClient.Add(this.Objects.Name);
                }
            }
            
            string title = (!reportTitle.IsNullOrEmpty() ?  reportTitle : this.Namespace.ToString() );

             s += '{' +
                    title.ToJson("report_title", false, true) + ','+

                    (template ? "template" : "data").ToJson("type") + ','  +
                    this.Objects.Name.ToJson("layout_name") + ',' +
                    pageNo.ToJson("page_number") + 

                    (fullTemplate ? ',' + this.pageInfo.ToJson(this.Objects.Invert) : "") +
                   
                    ((!template || fullTemplate) ? 
                        ',' + this.Objects.ToJson(template, "layout") :
                        ',' + (new Layout(null, this.Objects.Name)).ToJson(true, "layout")) +
                 '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }
    }
}
