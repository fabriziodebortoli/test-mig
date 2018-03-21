using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;
using TaskBuilderNetCore.Data;
using static TaskBuilderNetCore.Data.Provider;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.Hotlink;
using Microarea.Common;
using System.Data.SqlClient;

namespace Microarea.RSWeb.WoormEngine
{

    /// <summary>
    /// LetStatement
    /// </summary>
    //============================================================================
    public class LetStatement : WoormEngineExpression
	{
		private	Field field = null;

		//...................................................................properties
		public Field	Field		{ get { return field; }}
		public string	FieldName	{ get { return field.PublicName; }}

		//----------------------------------------------------------------------------
        public LetStatement(ReportEngine engine, TbReportSession session, FieldSymbolTable symTable)
            : base(engine, session, symTable)
		{
		}

		//----------------------------------------------------------------------------
		public Field Parse(Parser lex)
		{
			string fieldNameTo;
			if (lex.ParseID(out fieldNameTo))
			{
				field = SymbolTable.Find(fieldNameTo) as Field;
			
				if (field == null) lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, fieldNameTo));
				else if (field.ReadOnly) lex.SetError(string.Format(WoormEngineStrings.ReadOnlyField, fieldNameTo));
				else if (field.OwnerRule != null) lex.SetError(string.Format(WoormEngineStrings.FieldAlreadyUsed, fieldNameTo));
				else if (lex.ParseTag(Token.ASSIGN) && Compile(lex, CheckResultType.Compatible, Field.DataType)) 
					return field;
			}

			return null;
		}
	}

	/// <summary>
	/// RuleObj
	/// </summary>
	//============================================================================
	abstract public class RuleObj
	{
        public RuleEngine	    engine;
		public List<RuleObj>    Sons = new List<RuleObj>();		// valido solo dopo una BuildLinks
		public List<RuleObj>	Parents = new List<RuleObj>();	// valido solo dopo una BuildLinks
		public int		    	ParentsNum = 0;				// usato solo per il sort delle rules
		public int			    RuleId = 0;
		public int			    GroupByPos = -1;
		public bool			    Mark = false;
		public bool			    Sorted = false;

		protected bool		    failed = false;
		protected bool		    nullSolution = false;
	
        public enum SelectMode { ALL, NOT_NULL, NULL }
		//-----------------------------------------------------------------------------
		public RuleEngine	Engine	{ get { return engine; } set { engine = value; }}
		public TbReportSession Session	{ get { return Engine.Session; }}

		//-----------------------------------------------------------------------------
		abstract public bool		IsParentOf	(RuleObj ro);
		abstract public bool		Contains	(string aFldName);
		abstract public RuleReturn	Apply		();

		public virtual void Close() { /*Il default non fa nulla*/}

		public virtual bool Unparse(Unparser unparser) { return true; }

		//-----------------------------------------------------------------------------
		public RuleObj(RuleEngine engine)
		{
			this.engine = engine;
		}

		//-----------------------------------------------------------------------------
		protected bool HasSon (RuleObj ruleObj)
		{
			foreach (RuleObj son in Sons)
				if (son.RuleId == ruleObj.RuleId) 
					return true;

			return false;
		}

		//-----------------------------------------------------------------------------
		protected bool IsSonOfNullRules ()
		{
			foreach (RuleObj parent in Parents)
				if (parent.nullSolution)
					return true;

			return false;
		}

		//-----------------------------------------------------------------------------
		protected bool IsParentOfFailedRules ()
		{
			foreach (RuleObj son in Sons)
				if (son.failed)
					return true;

			return false;
		}

		//-----------------------------------------------------------------------------
		protected bool IsRoot ()
		{
			return Parents.Count == 0;
		}

        ///<summary>
        ///Rimuove gli spazi terminali dalla stringa
        /// </summary>
        //-----------------------------------------------------------------------------
        protected static string StripBlank(string data)
        {
            return data.TrimEnd(' ');
        }
    }

    /// <summary>
    /// ExpressionRule
    /// </summary>
    //============================================================================
    public class ExpressionRule : RuleObj
	{
		private LetStatement	letExpression;
		private WoormEngineExpression		whereExpression;

		//-----------------------------------------------------------------------------
		public ExpressionRule(RuleEngine engine) : base(engine)
		{
            letExpression = new LetStatement(engine.Report.Engine, Session, engine.RepSymTable.Fields);
            whereExpression = new WoormEngineExpression(engine.Report.Engine, Session, engine.RepSymTable.Fields);
		}

		//-----------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			letExpression.StopTokens = new StopTokens(new Token[] {Token.WHERE});

			Field aField = letExpression.Parse(lex);
			if (aField != null)
			{
				aField.OwnerRule = this;

				if (lex.Matched(Token.WHERE) && !(whereExpression.Compile(lex, CheckResultType.Match, "Boolean")))
					return false;

				if (!lex.Error) lex.ParseSep();
			}

			return !lex.Error;
		}

		//-----------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			if (!this.letExpression.IsEmpty)
			{
				unparser.WriteLine();
				unparser.WriteID(letExpression.FieldName, false);
				unparser.WriteTag(Token.ASSIGN, false);
				unparser.WriteExpr(letExpression.ToString(), false);
			}

			if (!whereExpression.IsEmpty)
			{
				unparser.WriteTag(Token.WHERE, false);
				unparser.WriteExpr(whereExpression.ToString(), false);
			}

			unparser.WriteSep(true);
			unparser.WriteLine();
			return true;
		}

		//-----------------------------------------------------------------------------
		public override bool IsParentOf (RuleObj ruleObj)
		{
			// non controlla le espressioni perchè la elazione parentale è già stata costruita dalla BuildLinks
			if (ruleObj.Sorted)
				return HasSon(ruleObj);

			return ruleObj.Contains(letExpression.FieldName);
		}

		//-----------------------------------------------------------------------------
		public override bool Contains (string fieldName)
		{
			if (letExpression.HasMember(fieldName)) return true;
			if (whereExpression.HasMember(fieldName)) return true;

			return false;
		}

		//-----------------------------------------------------------------------------
		public override RuleReturn Apply ()
		{
			Field aField  = (Field) letExpression.Field;

			// try to eval the expression also if the ancestors are evaluated null because the dependency
			// could be due to the sql-clause expression
			if ((Engine.NullRulesNum == 0) || !IsSonOfNullRules() || !whereExpression.IsEmpty)
			{
				if (!letExpression.IsEmpty)
				{
					Value val = letExpression.Eval();
					if (letExpression.Error)
					{
						Engine.SetError(letExpression.Diagnostic, string.Format(WoormEngineStrings.RuleExpression, aField.PublicName));
						return RuleReturn.Abort;
					}
					aField.AssignRuleData(val.Data, val.Valid);
				}

				if (!whereExpression.IsEmpty)
				{
					Value val = whereExpression.Eval();

					if (whereExpression.Error)
					{
						Engine.SetError(whereExpression.Diagnostic, string.Format(WoormEngineStrings.WhereError, aField.PublicName));
						return RuleReturn.Abort;
					}

					bool goodVal = (bool)val.Data;
					if (!goodVal)
					{
						failed = true;
						return RuleReturn.Backtrack;
					}
				}

				if (!letExpression.Error)
				{
					failed = false;
					if (nullSolution)
					{
						nullSolution = false;
						Engine.NullRulesNum--;
					}

					aField.RuleDataFetched = true;
					return Engine.ApplyRuleFrom(RuleId + 1);
				}
			}

			failed = false;
			if (!nullSolution)
			{
				nullSolution = true;
				Engine.NullRulesNum++;
			}

			aField.SetNullRuleData();
			return Engine.ApplyRuleFrom(RuleId + 1);
		}
	}

	/// <summary>
	/// ConditionalRule
	///		if (cond) then a= 10 else a= 20
	/// Attenzione deve per forza essere lo stesso l-value sia per la clausola "then"
	/// che per quella "else". Quindi nel nostro esempio solo "a".
	/// 	if (cond) then a= 10 else b= 20 
	/// 	non è valida
	/// </summary>
	//============================================================================
	public class ConditionalRule : RuleObj
	{
		private string						publicName;
		private WoormEngineExpression		testExpression;
		private LetStatement				thenExpression;
		private LetStatement				elseExpression;
		private WoormEngineExpression		whereExpression;
		
		//-----------------------------------------------------------------------------
		public ConditionalRule (RuleEngine engine) : base(engine)
		{
			testExpression	= new WoormEngineExpression	(engine.Report.Engine, Session, engine.RepSymTable.Fields);
            thenExpression  = new LetStatement      (engine.Report.Engine, Session, engine.RepSymTable.Fields);
            elseExpression  = new LetStatement      (engine.Report.Engine, Session, engine.RepSymTable.Fields);
            whereExpression = new WoormEngineExpression   (engine.Report.Engine, Session, engine.RepSymTable.Fields);
		}

		//-----------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			bool parseSep = true;

			if (lex.ParseTag(Token.IF))
			{
				testExpression.StopTokens = new StopTokens(new Token[] { Token.THEN });

				if (!(testExpression.Compile(lex, CheckResultType.Match, "Boolean")))
					return false;

				if (lex.ParseTag(Token.THEN))
				{
					thenExpression.StopTokens = new StopTokens(new Token[] {Token.ELSE, Token.WHERE});
					Field rfThen = thenExpression.Parse(lex);
					this.publicName = rfThen.PublicName;
					if (rfThen == null) 
						return false;

					parseSep = !lex.Matched(Token.SEP);

					if (lex.Matched(Token.ELSE))
					{
						elseExpression.StopTokens = new StopTokens(new Token[] {Token.WHERE});
						Field rfElse =  elseExpression.Parse(lex);
						if (rfElse == null) return false;

						if (rfThen != rfElse)
						{
							string strName = rfThen.PublicName;
							strName += ", ";
							strName += rfElse.PublicName;
							lex.SetError(string.Format(WoormEngineStrings.NotSameField, strName));
							return false;
						}

						parseSep = !lex.Matched(Token.SEP);
					}

					rfThen.OwnerRule = this;

					if (!lex.Error && lex.Matched(Token.WHERE))
					{
						if (!(whereExpression.Compile(lex, CheckResultType.Match, "Boolean")))
							return false;

						parseSep = true;
					}

					if (!lex.Error && parseSep)
						lex.ParseSep();
				}
			}

			return !lex.Error;
		}

		//-----------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			if (this.testExpression.IsEmpty)
			{
				unparser.WriteID(publicName, false);
				unparser.WriteTag(Token.ASSIGN, false);
				unparser.WriteExpr(thenExpression.ToString(), false);
			}
			else
			{
				unparser.WriteTag(Token.IF, false);
				unparser.WriteExpr(testExpression.ToString());

				unparser.IncTab();
				unparser.WriteTag(Token.THEN, false);
				unparser.WriteID(publicName, false);
				unparser.WriteTag(Token.ASSIGN, false);
				unparser.WriteExpr(thenExpression.ToString(), true);

				if (!elseExpression.IsEmpty)
				{
					unparser.WriteTag(Token.ELSE, false);
					unparser.WriteID(publicName, false);
					unparser.WriteTag(Token.ASSIGN, false);
					unparser.WriteExpr(elseExpression.ToString(), false);
				}

				unparser.DecTab();
			}
			
			if (!whereExpression.IsEmpty) 
			{
			    unparser.WriteLine();
			    unparser.WriteTag(Token.WHERE, false);
				unparser.WriteExpr(whereExpression.ToString(), false);
			}

			unparser.WriteSep(true);
			return true;
		}


		//-----------------------------------------------------------------------------
		public override bool IsParentOf (RuleObj ruleObj)
		{
			if (ruleObj.Sorted)
				return HasSon(ruleObj);
				
			return ruleObj.Contains(thenExpression.FieldName);
		}

		//-----------------------------------------------------------------------------
		public override bool Contains (string fieldName)
		{
			if (testExpression.HasMember(fieldName)) return true;
			if (thenExpression.HasMember(fieldName)) return true;
			if (elseExpression.HasMember(fieldName)) return true;
			if (whereExpression.HasMember(fieldName)) return true;

			return false;
		}

		//-----------------------------------------------------------------------------
		public override RuleReturn Apply ()
		{
			Field aField =  thenExpression.Field;
			Value vt = testExpression.Eval(); 
			if (testExpression.Error)
			{
				Engine.SetError(testExpression.Diagnostic, string.Format(WoormEngineStrings.EvalCondRuleExpression, aField.PublicName));
				return RuleReturn.Abort;
			}

			bool cond = (bool) vt.Data; 
			if (cond)
			{
				if (!thenExpression.IsEmpty)
				{
					Value v = thenExpression.Eval();
					if (thenExpression.Error)
					{
						Engine.SetError(thenExpression.Diagnostic, string.Format(WoormEngineStrings.EvalRuleThen, aField.PublicName));
						return RuleReturn.Abort;
					}
					aField.AssignRuleData(v.Data, v.Valid);
				}
			}
			else
			{
				if (!elseExpression.IsEmpty)
				{
					Value v = elseExpression.Eval();
					if (elseExpression.Error)
					{
						Engine.SetError(elseExpression.Diagnostic, string.Format(WoormEngineStrings.EvalRuleElse, aField.PublicName));
						return RuleReturn.Abort;
					}
					aField.AssignRuleData(v.Data, v.Valid);
				}
			}

			if (!whereExpression.IsEmpty)
			{
				Value v = whereExpression.Eval();
				if (whereExpression.Error && v != null)
				{
					Engine.SetError(whereExpression.Diagnostic, string.Format(WoormEngineStrings.EvalWhere, aField.PublicName));
					return RuleReturn.Abort;
				}

				bool goodVal = (bool) v.Data;
				if (!goodVal)
				{
					failed = true;
					return RuleReturn.Backtrack;
				}
			}

			failed = false;
			if (!testExpression.Error && (cond ? !thenExpression.Error : !elseExpression.Error))
			{
				if (nullSolution)
				{
					nullSolution = false;
					Engine.NullRulesNum--;
				}
				aField.RuleDataFetched = true;
			}
			else
			{
				if (!nullSolution)
				{
					nullSolution = true;
					Engine.NullRulesNum++;
				}
				aField.SetNullRuleData();
			}

			return Engine.ApplyRuleFrom(RuleId + 1);
		}
	}

	/// <summary>
	/// FromTablesList
	/// </summary>
	//============================================================================
	public class FromTablesList : List<SelectedTable>
	{
		//-----------------------------------------------------------------------------
		public FromTablesList() : base() {}

		//-----------------------------------------------------------------------------
		public SelectedTable GetTableNames(string tableName)
		{
			foreach (SelectedTable tn in this)
				if (string.Compare(tableName, tn.TableName, StringComparison.OrdinalIgnoreCase) == 0)
					return tn;

			return null;
		}

		//-----------------------------------------------------------------------------
		public string GetColumnType(string tableName, string columnName)
		{
			SelectedTable tn = GetTableNames(tableName);
			if (tn == null) 
                return null;

			return tn.GetColumnType(columnName);
		}

		//-----------------------------------------------------------------------------
		public string GetTableNameFromAlias(string tableAlias)
		{
			foreach (SelectedTable tn in this)
				if (string.Compare(tableAlias, tn.AliasName, StringComparison.OrdinalIgnoreCase) == 0)	
					return tn.TableName;

			return null;
		}
		//-----------------------------------------------------------------------------
		public string GetTableNameFromColumnName(string columnName)
		{
			foreach (SelectedTable tn in this)
				if (tn.GetColumnType(columnName) != string.Empty)	
					return tn.TableName;

			return string.Empty;
		}
	}

	/// <summary>
	/// DataTableRule
	/// </summary>
	//============================================================================
	public class DataTableRule : RuleObj, IDisposable
	{
		private bool disposed = false;	// Track whether Dispose has been called.
		public event EventHandler SearchTic;
		
		private List<Field>		selectFields = new List<Field>();

		public	FromTablesList	FromTables = new FromTablesList();

		private IfWhereClause	whereClause = null;
		private IfWhereClause	havingClause = null;

		private string			orderBy = "";
		private	string			groupBy = "";

		private SelectMode		constraint = SelectMode.ALL;	//Woorm type selection
		private bool			distinct = false;				//Sql Distinct clause
        private int             top = 0;				        //Sql TOP nn clause

        private string			orderByDynamic = string.Empty;
		private string			groupByDynamic = string.Empty;
        private string          extraFilter = string.Empty;

        private WoormEngineExpression whenExpr = null;

		private DBConnection	tbConnection	= null; 
		private DBCommand tbCommand		= null;
		private IDataReader		iDataReader		= null;
		// properties
		//-----------------------------------------------------------------------------
		public FieldSymbolTable SymbolTable { get { return Engine.RepSymTable.Fields;}}

		//-----------------------------------------------------------------------------
		public DataTableRule(RuleEngine engine) : base(engine)
		{
           whenExpr = new WoormEngineExpression(engine.Report.Engine, Session, engine.RepSymTable.Fields);
			
           Init();
		}

		//------------------------------------------------------------------------------
		~DataTableRule()
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
			if (!disposed)
                Close();
			disposed = true;         
		}

        //-----------------------------------------------------------------------------
        public override void Close()
        {
            base.Close();

            if (iDataReader != null)
            {
                iDataReader.Close();
                iDataReader = null;
            }
            if (tbCommand != null)
            {
                tbCommand.Dispose();
                tbCommand = null;
            }
            if (tbConnection != null)
            {
                tbConnection.Close();
                tbConnection = null;
            }
        }
		//-----------------------------------------------------------------------------
		protected void Init()
		{
			whereClause = new IfWhereClause(Session, Engine.RepSymTable.Fields, this);
			havingClause = new IfWhereClause(Session, Engine.RepSymTable.Fields, this);
			havingClause.ClauseType = WhereClauseExpr.EClauseType.HAVING;
		}

		// per fisico si intende un dato appartenente and una tabella ossia : table.variabile
		//-----------------------------------------------------------------------------
		public bool IsQualifiedName(string columnName)
		{
			int nIdx = columnName.IndexOf('.');

			Debug.Assert(nIdx != 0);
			if (nIdx < 0)
				return false;

			string tableName = columnName.Substring(0, nIdx);
			foreach (SelectedTable info in FromTables)
				if	(string.Compare(info.TableName, tableName, StringComparison.OrdinalIgnoreCase) == 0)
					return true;

			return false;
		}

		// costruisce un nome qualificato prendendo il nome della prima tabella presente
		// nella clausola from, se è già qualificato lo lascia uguale
		//-----------------------------------------------------------------------------
		public string GetQualifiedName(string strFromName)
		{
			// deve esserci almeno una tabella
			Debug.Assert(FromTables.Count > 0);

			// .nome non è considerato valido
			int dotPos = strFromName.IndexOf('.');
			Debug.Assert(dotPos != 0);

			SelectedTable first = (SelectedTable)FromTables[0];
			return (dotPos > 0) ? strFromName : first.TableName + '.' + strFromName;
		}

		//-----------------------------------------------------------------------------
		public void ResetDataMapping()
		{
			int nf = selectFields.Count - 1;

			for (int i = 0; i < nf; i++)
			{
				ResetDataItem((Field)selectFields[i], i);
			}
		}

		// Esegue il parsing del predicato ORDER BY 
		// E' utilizzata anche per la GROUP BY (elenco di colonne della tabella)
		//----------------------------------------------------------------------------
		public bool ParseOrderBy(Parser lex, ref string result)
		{
			string tempName = " ";
			string name = string.Empty;

			do
			{
				if (lex.LookAhead(Token.CONTENTOF))
				{
					string sPrecAuditString = string.Empty;
					bool bPrecAuditingState = lex.DoAudit;
					if (bPrecAuditingState) 
						sPrecAuditString = lex.GetAuditString();
					else
						lex.DoAudit = true;

					if (!lex.ParseTag(Token.CONTENTOF) || !lex.ParseTag(Token.ROUNDOPEN))
						return false;

					if (!lex.SkipToToken(new Token [] { Token.ROUNDCLOSE }, true, true, true))
						return false;

					string sContentOf = lex.GetAuditString();
					tempName += sContentOf;

					if (bPrecAuditingState)
					{
                        lex.SetAuditString(sPrecAuditString.IsNullOrEmpty() ? sContentOf : sPrecAuditString + ' ' + sContentOf);
					}
					else
					{
						lex.DoAudit = false;
					}
				}
				else
				{
					if (!lex.ParseID(out name))  
						return false;

					if (!IsQualifiedName(name) && !SymbolTable.Contains(name))
					{
						lex.SetError(string.Format(ExpressionManagerStrings.UnknownField, name));
						return false;
					}

					// l'espressione viene memorizzata cosi` come l'utente l'ha scritta (piu` o meno)
					tempName += GetQualifiedName(name);
				}

				Token nTok = lex.LookAhead();
                if (nTok != Token.EOF && nTok != Token.COMMA && nTok != Token.SEP && nTok != Token.ORDER && nTok != Token.HAVING && nTok != Token.WHEN)
					if (nTok == Token.DESCENDING || nTok == Token.ASC)
					{
						tempName += " " + Language.GetTokenString(nTok);
						lex.SkipToken();
					}
					else
					{
						lex.SetError(ExpressionManagerStrings.SyntaxError);
						return false;
					}

				if (lex.LookAhead() == Token.COMMA)
					tempName += Language.GetTokenString(Token.COMMA);
					
			} while (lex.Matched(Token.COMMA));

			if (lex.Error)	return false;

			result = tempName;
			return true;	
		}

		//-----------------------------------------------------------------------------
		bool ExpandContentOfClause (ref string  strSql, int nPos)
		{
			Parser parser = new Parser(Parser.SourceType.FromString);
			if (!parser.Open(strSql))
				return false;

			bool bOk = parser.SkipToToken(Token.CONTENTOF);

			if (!parser.ParseTag(Token.CONTENTOF) || !parser.ParseTag(Token.ROUNDOPEN)) 
				return false;

			parser.DoAudit = true;
			WoormEngineExpression expr = new WoormEngineExpression(this.Engine.Report.Engine, this.Session, this.SymbolTable);
			expr.StopTokens = new StopTokens(new Token [] {Token.ROUNDCLOSE});
            expr.StopTokens.skipInnerRoundBrackets = true;
			if (!expr.Compile(parser, CheckResultType.Match, "String"))
				return false;
			string auditExpr = parser.GetAuditString();
			parser.DoAudit = false;

			int nEnd = parser.CurrentPos;

			if (!parser.ParseTag(Token.ROUNDCLOSE)) 
				return false;

			Value v = expr.Eval();
			if (expr.Error)
				return false;
							
            string s1 = strSql.Substring(0, nPos);
            string s2 = strSql.Substring(nEnd + 1);

			strSql = (string)(v.Data);


			strSql = s1 + strSql + s2;

			return true;
		}

		//-----------------------------------------------------------------------------
		bool ExpandContentOfClause (ref string  strSql)
		{
			strSql = strSql.Replace('\n', ' ');
			strSql = strSql.Replace('\r', ' ');

			string f = Token.CONTENTOF.ToString().ToLower();

            while (true)
			{
				string s = strSql.ToLower();
				int nPos = s.IndexOf(f);
				if (nPos < 0) break;

				if (!ExpandContentOfClause(ref strSql, nPos))
					return false;
			}

            string str = strSql;
            return WhereClauseExpr.ConvertToNative(this.SymbolTable, this.Session, null, str, out strSql);
		}

		//----------------------------------------------------------------------------
		CultureInfo GetCollateCulture(FromTablesList fromTables, Field f)
		{
			string table, column;
			int index = f.QualifiedPhysicalName.IndexOf(".");
			if (index == -1)
			{
				table = ((SelectedTable)fromTables[0]).TableName;
				column = f.QualifiedPhysicalName;
			}
			else
			{
				table = f.QualifiedPhysicalName.Substring(0, index);
				column = f.QualifiedPhysicalName.Substring(index + 1);
			}

			try
			{
				if (tbConnection == null)
				{
                    if (string.IsNullOrEmpty(Engine.Report.ReportSession.CompanyDbConnection))
                        return CultureInfo.InvariantCulture;
                    /*TODO RSWEB new DBConnection (tipo database e collate) */
					tbConnection = new DBConnection(DBType.SQLSERVER, Engine.Report.ReportSession.CompanyDbConnection/*, TBDatabaseType.GetDBMSType(Engine.Report.ReportSession.Provider TODO rsweb*/);
					tbConnection.Open();
				}
                string collation = "";/*TODO RSWEB Microarea.TaskBuilderNet.Data.DatabaseLayer.TBCheckDatabase.GetColumnCollation(tbConnection, table, column); */
                return (collation.Length == 0 || collation == TaskBuilderNetCore.Data.NameSolverDatabaseStrings.SQLLatinCollation)
					? CultureInfo.InvariantCulture 
					: Session.UserInfo.CompanyCulture;
			}
	
			catch (DBException e)
			{
				Engine.SetError(e.Message);
				return CultureInfo.InvariantCulture;
			}
		}

		//----------------------------------------------------------------------------
		public bool Parse(Parser parser)
		{
			string publicName = "";
			string physicalName = "";
			int numTables = 0;

            /*	Pseudo grammatica (TODO: da riscrivere):
 *	            FROM <table> { [ , <table> } ] |
                               [ CROSS JOIN <table> ] |
                               [ [INNER JOIN | LEFT OUTER JOIN | RIGHT OUTER JOIN | FULL OUTER JOIN] <table> ON <on-clause> ]
                             }
				SELECT
                        [NULL | NOT NULL] [DISTINCT] [TOP <top-rows>]
				{ [ <column> | \{ <sql-native-expr> \} ]  INTO <field> , }
			*/

            if (!parser.ParseTag(Token.FROM))
				return false;

            bool bContinue = true;
            bool natural = true;
			do
			{
                WhereClauseExpr.EJoinType joinType = WhereClauseExpr.EJoinType.CROSS;
                if (FromTables.Count > 0 && !natural)
                {
                    if (parser.Matched(Token.INNER) && parser.Match(Token.JOIN))
                    {
                        joinType = WhereClauseExpr.EJoinType.INNER;
                    }
                    else if (parser.Matched(Token.CROSS) && parser.Match(Token.JOIN))
                    {
                        joinType = WhereClauseExpr.EJoinType.CROSS;
                    }
                    else if (parser.Matched(Token.LEFT) && parser.Match(Token.OUTER) && parser.Match(Token.JOIN))
                    {
                        joinType = WhereClauseExpr.EJoinType.LEFT_OUTER;
                    }
                    else if (parser.Matched(Token.RIGHT) && parser.Match(Token.OUTER) && parser.Match(Token.JOIN))
                    {
                        joinType = WhereClauseExpr.EJoinType.RIGHT_OUTER;
                    }
                    else if (parser.Matched(Token.FULL) && parser.Match(Token.OUTER) && parser.Match(Token.JOIN))
                    {
                        joinType = WhereClauseExpr.EJoinType.FULL_OUTER;
                    }

                    if (parser.Error)
                        return false;
                }

                string dataTableName = "";
				string aliasTableName = "";

				if (!parser.ParseID(out dataTableName))
					return false;

				if (parser.Matched(Token.ALIAS) && !parser.ParseID(out aliasTableName))
					return false;

                numTables++;

				SelectedTable selectedTable = new SelectedTable(dataTableName, aliasTableName, Engine.Report.ReportSession);
				if (selectedTable.Diagnostic.Error)
				{
					parser.SetError(selectedTable.Diagnostic);
					return false;
				}
				FromTables.Add(selectedTable);

                if (FromTables.Count > 1 && joinType != WhereClauseExpr.EJoinType.CROSS)
                {
                    if (!parser.Match(Token.ON))
                        return false;

                    IfWhereClause onClause = new IfWhereClause(Engine.Report.ReportSession, Engine.RepSymTable.Fields, this, selectedTable, joinType);
 
                    if (!onClause.Compile(parser, CheckResultType.Match, "Boolean"))
                        return false;
                    if (parser.Error)
                        return false;

                    selectedTable.JoinOnClause = onClause;
                }

                //----------------- 
                bContinue = false;
                if (natural && parser.Matched(Token.COMMA))
                {
                    bContinue = true;
                }
                else
                {
                    if (natural && FromTables.Count > 1)
                        break;

                    natural = false;
                    bContinue = parser.LookAhead(Token.INNER) || parser.LookAhead(Token.LEFT) || parser.LookAhead(Token.RIGHT) || parser.LookAhead(Token.FULL) || parser.LookAhead(Token.CROSS);
                }
            }
            while (bContinue);

            if (!parser.ParseTag(Token.SELECT))
				return false;

			constraint = SelectMode.ALL;

			if (parser.Matched(Token.NOT) && parser.ParseTag(Token.NULL))
				constraint = SelectMode.NOT_NULL;
			else
				if (!parser.Error && parser.Matched(Token.NULL))
				constraint = SelectMode.NULL;
			else
				if (parser.Error) 
                    return false;

            List<string> forbiddenIdents = new List<string>();

            distinct = parser.Matched(Token.DISTINCT);
            if (parser.Matched(Token.TOP))
            {
                if (!parser.ParseInt(out top))
                    return false;
                if (top < 0) top = 0;
            }

			try
			{
				do
				{
					bool nativeColumnExpr = false;
					if (parser.Matched(Token.BRACEOPEN))
					{
						nativeColumnExpr = true;

						parser.DoAudit = true;

						if (!parser.SkipToToken(Token.BRACECLOSE)) 
							return false;

						physicalName = parser.GetAuditString();
						parser.DoAudit = false;

                        physicalName = physicalName.Replace("[ ", "[").Replace(" ]", "]");

						if (!parser.ParseTag(Token.BRACECLOSE) || parser.Error )
							return false;
					}
					else if (!parser.ParseID(out physicalName))
						return false;

					if (!parser.ParseTag(Token.INTO) || !parser.ParseID(out publicName)) 
						return false;

					Field aField = Engine.RepSymTable.Fields.Find(publicName);
					if (aField == null)
					{
						parser.SetError(string.Format(ExpressionManagerStrings.UnknownField, publicName));
						return false;
					}

					if (aField.ReadOnly)
					{
						parser.SetError(string.Format(WoormEngineStrings.ReadOnlyField, publicName));
						return false;
					}
					
					if (aField.OwnerRule != null)
						parser.SetError(string.Format(WoormEngineStrings.FieldAlreadyUsed, publicName));

					if (nativeColumnExpr)
						aField.NativeColumnExpr = true;

					aField.PhysicalName = physicalName;

					if (aField.DataType.CompareNoCase("String")) 
						aField.CollateCulture = GetCollateCulture(FromTables, aField);

					aField.OwnerRule = this;

					selectFields.Add(aField);

                    forbiddenIdents.Add(publicName);
				}
				while (parser.Matched(Token.COMMA));
			}
			finally
			{
				if (tbConnection != null)
				{
					if (tbConnection.State != ConnectionState.Closed)
						tbConnection.Close();
					tbConnection = null;
				}
			}

			if (parser.Error)	
                return false;

			/*	Grammatica:
				[WHERE NATIVE native-expr | WHERE woorm-expr]
				[GROUP BY native-groupby]
				[HAVING NATIVE native-expr | HAVING woorm-expr]
				[ORDER BY native-orderby]
             *  [WHEN woorm-bool-expr]
             *  ;
			*/
            whereClause.ForbiddenIdents = forbiddenIdents;
			if (parser.Matched(Token.WHERE) && !(whereClause.Compile(parser, CheckResultType.Match, "Boolean")))
				return false;
            
			if (parser.Error)	
				return false;

			if 	(parser.Matched(Token.GROUP))
			{
				if	(
						!parser.ParseTag(Token.BY)	||
						!ParseOrderBy(parser, ref groupBy) 
					)
					return false;
				if (parser.Error)	
                    return false;

                if (groupBy.IndexOf('{') >= 0)
                    groupByDynamic = groupBy;
                else
                {
                    string s = groupBy.ToLower();
                    string f = Token.CONTENTOF.ToString().ToLower();
                    if (s.IndexOf(f) >= 0)
                    {
                        groupByDynamic = groupBy;
                    }
                }
				if (parser.Matched(Token.HAVING)) 
				{
                    havingClause.ForbiddenIdents = forbiddenIdents;
					if (!havingClause.Compile(parser, CheckResultType.Match, "Boolean"))
						return false;
				}
			}
			if (parser.Error) 
                return false;

			if 	(parser.Matched(Token.ORDER))
			{
				if	(
						!parser.ParseTag(Token.BY)	||
						!ParseOrderBy(parser, ref orderBy) 
					)
					return false;
				if (parser.Error) 
                    return false;

                if (orderBy.IndexOf('{') >= 0)
                    orderByDynamic = orderBy;
                else
                {
                    string s = orderBy.ToLower();
                    string f = Token.CONTENTOF.ToString().ToLower();
                    if (s.IndexOf(f) >= 0)
                    {
                        orderByDynamic = orderBy;
                    }
                }
			}

            if (parser.Matched(Token.WHEN))
            {
                whenExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                if (!whenExpr.Compile(parser, CheckResultType.Match, "Boolean"))
                    return false;
            }

            /* TODO RSWEB - gestione ExtraFiltering dagli SqlRecords (RowSecurity/Ge)
			//nel caso dell'uso all'interno del Localizer, non ho una connessione attiva
			//e non mi servono queste informazioni aggiuntive, quindi salto tutto
			if (!engine.Report.ForLocalizer)
			{
				// aggiunge la clausola WHERE
				ITbLoaderClient tbLoader = engine.Report.ReportSession.GetTBClientInterface();

				string[] tables = new string[FromTables.Count];
				int i = 0;
				foreach (SelectedTable tn in FromTables)
					tables.SetValue(tn.TableName, i++);

				extraFilter = tbLoader.GetExtraFiltering(tables, string.Empty);
			}
            */
            return parser.ParseSep();
		}

		//----------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.FROM, false);

			for (int i = 0; i < FromTables.Count; i++)
			{
				if (i > 0)
					unparser.WriteTag(Token.COMMA, false);

				SelectedTable table = (FromTables[i] as SelectedTable);
				if (table == null)
					continue;

				unparser.WriteID(table.TableName, false);

				//Scrivo l'alias solo se è diverso dal nome originale
				if (!table.AliasName.IsNullOrEmpty() && !table.TableName.CompareNoCase(table.AliasName))
				{
					unparser.WriteTag(Token.ALIAS, false);
					unparser.WriteID(table.AliasName, false);
				}
			}	

			unparser.WriteLine();
			unparser.WriteTag(Token.SELECT, false);

			switch (constraint)
			{
				case SelectMode.ALL:
					break;

				case SelectMode.NOT_NULL:
					unparser.WriteBlank();
					unparser.WriteTag(Token.NOT, false);
					unparser.WriteTag(Token.NULL, false);
					break;

				case SelectMode.NULL:
					unparser.WriteBlank();
					unparser.WriteTag(Token.NULL, false);
					break;
			}

			if (distinct)
			{
				unparser.WriteBlank();
				unparser.WriteTag(Token.DISTINCT, false);
			}
            if (top > 0)
            {
                unparser.WriteBlank();
                unparser.WriteTag(Token.TOP, false);
                unparser.Write(top, false);
            }

			unparser.WriteLine();
			unparser.IncTab();
			
			foreach (Field item in selectFields)
			{
				string physicalName = item.NativeColumnExpr
					? string.Format("{{0}}", item.PhysicalName)
					: item.PhysicalName;

				physicalName = physicalName.PadRight(20);
				unparser.Write(physicalName); 
				
				unparser.WriteTag(Token.INTO, false);
				unparser.WriteID(item.PublicName, false);

				//se non è l'ultimo aggiungo la virgola
				if (item != selectFields[selectFields.Count - 1])
					unparser.WriteComma(true);
			}

			if (whereClause != null && !whereClause.IsEmpty)
			{
				unparser.WriteLine();
				whereClause.Unparse(unparser, true);
			}

			unparser.DecTab();

			if (!string.IsNullOrEmpty(groupBy))
			{
				unparser.WriteLine();
				unparser.DecTab();

				unparser.WriteTag(Token.GROUP, false);
				unparser.WriteTag(Token.BY, false);
				unparser.Write(groupBy);

				if (havingClause != null && !havingClause.IsEmpty)
				{
					unparser.WriteLine();
					havingClause.Unparse(unparser, true);
				}
			}
			
			if (!string.IsNullOrEmpty(orderBy))
			{
				unparser.WriteTag(Token.ORDER, false);
				unparser.WriteTag(Token.BY, false);
				unparser.Write(orderBy); 
			}
			
			if (!whenExpr.IsEmpty)
			{
				unparser.WriteTag(Token.WHEN, false);
				unparser.WriteExpr(whenExpr.ToString()); 
			}

			unparser.WriteSep(true);
			return true;
		}

		//-----------------------------------------------------------------------------
		public override bool IsParentOf(RuleObj ruleObj)
		{
			if (ruleObj.Sorted)
				return HasSon(ruleObj);

			foreach (Field field in selectFields)
				if (ruleObj.Contains(field.PublicName))
					return true;

			return false;
		}

		//-----------------------------------------------------------------------------
		public override bool Contains(string fieldName)
		{
            if (whereClause != null &&  whereClause.HasMember(fieldName)) 
                return true;
            if (havingClause != null && havingClause.HasMember(fieldName))
                return true;

            foreach (SelectedTable t in this.FromTables)
            {
                if (t.JoinOnClause != null && t.JoinOnClause.HasMember(fieldName))
                    return true;
            }

            return false;
		}

        //-----------------------------------------------------------------------------
        public string GetFromTableList()
        {
            string fromList = "";
            bool first = true;
            foreach (SelectedTable t in FromTables)
            {
                if (first)
                { 
                    fromList += t.TableName; 
                    first = false; 
                }
                else
                {
                    if (t.JoinOnClause != null)
                    {
                        if (t.JoinOnClause.JoinType != WhereClauseExpr.EJoinType.CROSS && t.JoinOnClause.Sql.IsNullOrEmpty())
                        {
                            Debug.Assert(false);
                            fromList += " Cross Join " + t.TableName;
                        }
                        else switch (t.JoinOnClause.JoinType)
                        {
                            case WhereClauseExpr.EJoinType.INNER:
                                {
                                    fromList += " Inner Join " + t.TableName + " On " + t.JoinOnClause.Sql;
                                    break;
                                }
                            case WhereClauseExpr.EJoinType.LEFT_OUTER:
                                {
                                    fromList += " Left Outer Join " + t.TableName + " On " + t.JoinOnClause.Sql;
                                    break;
                                }
                            case WhereClauseExpr.EJoinType.RIGHT_OUTER:
                                {
                                    fromList += " Right Outer Join " + t.TableName + " On " + t.JoinOnClause.Sql;
                                    break;
                                }
                            case WhereClauseExpr.EJoinType.FULL_OUTER:
                                {
                                    fromList += " Full Outer Join " + t.TableName + " On " + t.JoinOnClause.Sql;
                                    break;
                                }
                            case WhereClauseExpr.EJoinType.CROSS:
                            default:
                                {
                                    fromList += " Cross Join " + t.TableName;
                                    break;
                                }
                        }
                    }
                    else
                        fromList += ',' + t.TableName;
                }
             }
            return fromList;
        }

        //-----------------------------------------------------------------------------
        private bool ExecuteCommand()
		{
			string select = "SELECT ";

			if (distinct)
				select += " DISTINCT ";

            if (top > 0)
            {
                bool isOracle = this.Session.UserInfo.DatabaseType == TaskBuilderNetCore.Data.DBMSType.ORACLE;
                if (!isOracle)
                    select += " TOP " + top.ToString() + ' ';
            }

			// costruische la select delle colonne;
			int current = 0;
			foreach (Field rf in selectFields)
			{
				if (rf.NativeColumnExpr)
				{
					//se c'e' contentof espando
					String s = rf.QualifiedPhysicalName;

					ExpandContentOfClause(ref s);

					select += s;
				}
				else
				    select += rf.QualifiedPhysicalName;

				current++;
				if (current < selectFields.Count) select += ",";
			}

			// aggiunge la clausola FROM con l'elenco dei nomi di tabella da cui estrarre i dati
			select += " FROM " + GetFromTableList();

            string _groupBy = groupBy;
            string _orderBy = orderBy;

			if (groupByDynamic != string.Empty)
			{
				_groupBy = groupByDynamic;
				if (!ExpandContentOfClause(ref _groupBy))
					return false;
			}
			if (orderByDynamic != string.Empty)
			{
				_orderBy = orderByDynamic;
				if (!ExpandContentOfClause(ref _orderBy))
					return false;
			}

            string filter = whereClause.Sql;
            if (!filter.Contains(extraFilter))
            {
                if (!filter.IsNullOrEmpty())
                    filter = "(" + filter + ") AND " + extraFilter;
                else
                    filter = extraFilter;
            }

			if (filter.Length > 0)
				select += " WHERE " + filter;

			// aggiunge la clausola GROUP BY
			if (groupBy.Length > 0)
			{
				select += " GROUP BY " + _groupBy;

				if (havingClause.Sql.Length > 0)
					select += " HAVING " + havingClause.Sql;
			}

			// aggiunge la clausola ORDER BY
			if (orderBy.Length > 0)
				select += " ORDER BY " + _orderBy;

            select = select.StripBlankNearSquareBrackets();

			// effettua la connessione e la costruzione della query. Deve usare la esecuzione senza prepare
			// perche la Sql potrebbe cambiare a causa di IF presenti nella estensione sintattica di woorm
			// che devono essere valutati ad ogni giro di Rule
			try
			{
                if (tbConnection == null)
                {    //ANASTASIA PROVA POSTGRE
                     tbConnection = new DBConnection(DBType.SQLSERVER, Engine.Report.ReportSession.CompanyDbConnection/*, TBDatabaseType.GetDBMSType(Engine.Report.ReportSession.Provider) TODO rsweb*/);
                    
                    //Npgsql
                    //tbConnection = new DBConnection(DBType.POSTGRE, string.Format(TaskBuilderNetCore.Interfaces.NameSolverDatabaseStrings.PostgreConnection, "localhost", 5432, "postgres", "postgres",
                                                                                     //"sa", "")/*, TBDatabaseType.GetDBMSType(Engine.Report.ReportSession.Provider) TODO rsweb*/);

                      //MYSQL
                   // tbConnection = new DBConnection(DBType.MYSQL, string.Format(TaskBuilderNetCore.Interfaces.NameSolverDatabaseStrings.MySqlConnection, "localhost", "root", "sasa", "northwind"
                                                                                   //  )/*, TBDatabaseType.GetDBMSType(Engine.Report.ReportSession.Provider) TODO rsweb*/);

                }
                if (tbConnection.State != ConnectionState.Open)
                    tbConnection.Open();

                if (iDataReader != null)
                {
                    iDataReader.Close();
                    iDataReader = null;
                }
                if (tbCommand != null)
                {
                    tbCommand.Dispose();
                    tbCommand = null;
                }

                tbCommand = new DBCommand(select, tbConnection);
                tbCommand.CommandTimeout = 0;

                foreach (SelectedTable t in FromTables)
                {
                    if (t.JoinOnClause != null)
                    {
                        BindParameters(t.JoinOnClause.Parameters);
                    }
                }
                BindParameters(whereClause.Parameters);
                BindParameters(havingClause.Parameters);
				
				// creo il DataReader
				iDataReader = tbCommand.ExecuteReader();
			}
			catch (DBException e)
			{
                if (iDataReader != null)
                {
                    iDataReader.Close();
                    iDataReader = null;
                }
                if (tbCommand != null)
                {
                    tbCommand.Dispose();
                    tbCommand = null;
                }
                tbConnection.Close();

				Engine.SetError(e.Message);
				return false;
			}
            
            return true;
		}

        //-----------------------------------------------------------------------------
        void BindParameters(List<ParamItem> parameters)
        {
            // faccio il bind dei parametri
            foreach (ParamItem param in parameters)
            {
                if (param.IsValueContentOf)
                {
                    if (tbCommand.CommandText.Contains(param.Name))
                        tbCommand.CommandText = tbCommand.CommandText.Replace(param.Name, param.Data.ToString());
                }
                else
                {
                    //TODO RSWEB add command parameter
                    IDbDataParameter p = tbCommand.CreateParameter();
                    p.ParameterName = param.Name;
                    p.Value = param.Data;

                    tbCommand.Parameters.Add(p);
                }
            }
        }

        //-----------------------------------------------------------------------------
        private RuleReturn Execute()
		{
			if (Engine.NullRulesNum != 0 && IsSonOfNullRules())
				return RuleReturn.Backtrack;

            foreach (SelectedTable t in FromTables)
            {
                if (t.JoinOnClause != null)
                {
                    if (!t.JoinOnClause.BuildSql())
                    {
                        if (t.JoinOnClause.Error)
                            return RuleReturn.Backtrack;

                        Engine.SetError(WoormEngineStrings.DatatableRuleJoinOn);
                        return RuleReturn.Abort;
                    }
                }
            }

			if (!whereClause.BuildSql())
			{
				if (whereClause.Error)
					return RuleReturn.Backtrack;

				Engine.SetError(WoormEngineStrings.WhereError);
				return RuleReturn.Abort;
			}
			if (!havingClause.BuildSql())
			{
				if (havingClause.Error)
					return RuleReturn.Backtrack;

				Engine.SetError(WoormEngineStrings.DatatableRuleHaving);
				return RuleReturn.Abort;
			}

            // Crea la select ed il data reader già eseguito
            if (!ExecuteCommand())
			{
				Engine.SetError(WoormEngineStrings.QueryFailed);
				return RuleReturn.Abort;
			}

			return RuleReturn.Success;
		}

		//-----------------------------------------------------------------------------
		private RuleReturn FetchData(ref bool foundData)
		{
            if (iDataReader == null)
            {
                Engine.SetError("The recorset is null");
                return RuleReturn.Abort;
            }

			try
			{
                int rowsFetched = 0;
                bool bStrip = true; // Session != null && Session.StripTrailingSpaces;
				while (iDataReader.Read())
				{
                    rowsFetched++;

					if (SearchTic != null) 
                        SearchTic(this, new EventArgs());

					if (constraint == SelectMode.NULL)
					{
						foundData = true;
						break;
					}

					// valorizzo i dati che provengono dalla select
					int i = 0;
					foreach (Field rf in selectFields)
					{
                        //object o = rf.NativeColumnExpr || rf.PhysicalName.IndexOf('.') != -1 ? iDataReader[i] : iDataReader[rf.PhysicalName];
                        //ATTENZIONE: se rf.NativeColumnExpr allora rf.PhysicalName contiene l'espressione dinamica non ancora espansa
                        object o = iDataReader[i]; 

						object data = Common.CoreTypes.ObjectHelper.CastFromDBData(o, rf.Data);

						//An. 17609
						if (data is string && bStrip)
                            data = StripBlank(data as string);

                        rf.Data = data;

						rf.RuleDataFetched = true;
						rf.ValidRuleData = true;

						i++;
					}
			
					failed = false;
					if (nullSolution)
					{
						nullSolution = false;
						Engine.NullRulesNum--;
					}
			
					// this statement musts be in this place after fields updating
					switch (Engine.ApplyRuleFrom(RuleId + 1))
					{
						case RuleReturn.Abort		:
							return RuleReturn.Abort;
					
						case RuleReturn.Backtrack	:
						{
							if (!IsParentOfFailedRules()) 
								return RuleReturn.Backtrack;
												
							break;
						}
											
						case RuleReturn.Success	:
						{
							foundData = true;
							break;
						}
					}

                    if (top > 0 && top == rowsFetched)
                        break;
				}

                if (iDataReader != null)
                {
                    iDataReader.Close();
                    iDataReader = null;
                }
                if (tbCommand != null)
                {
                    tbCommand.Dispose();
                    tbCommand = null;
                }
            }
            catch (DBException e)
			{
                if (iDataReader != null)
                {
                    iDataReader.Close();
                    iDataReader = null;
                }
                if (tbCommand != null)
                {
                    tbCommand.Dispose();
                    tbCommand = null;
                }


                Engine.SetError(e.Message);
				return RuleReturn.Abort;
			}
			finally
			{
                //tbConnection.Close();
               //tbConnection= null;
			}

			return RuleReturn.Success;
		}
					
		//-----------------------------------------------------------------------------
		public override RuleReturn Apply()
		{
            if (!whenExpr.IsEmpty)
            {
                Value cond = whenExpr.Eval();
                if (whenExpr.Error)
                {
                    engine.SetError("When expression on From-Select rule is wrong");
                    return RuleReturn.Abort;
                }

                if (!(bool)(cond.Data))
                {
                    engine.NullRulesNum--;
                    return engine.ApplyRuleFrom(RuleId + 1);
                }
            }
            //----

			bool foundData = false;

			RuleReturn rr = Execute();
			switch (rr)
			{
				case RuleReturn.Abort		:
					return RuleReturn.Abort;

				case RuleReturn.Success	:
				{
					RuleReturn retVal = FetchData(ref foundData);

					if (retVal != RuleReturn.Success)
						return retVal;

					if (foundData && (constraint != SelectMode.NULL))
					{
						failed = false;
						return RuleReturn.Success;
					}
					
					break;
				}
			}
			
			if	(
					(!foundData && (constraint == SelectMode.NOT_NULL)) ||
					(foundData && (constraint == SelectMode.NULL))
				)
			{
				failed = true;
				return RuleReturn.Backtrack;
			}

			// non ho trovato dati ma sono in SelectMode ALL o NULL
			failed = false;
			if (!nullSolution)
			{
				nullSolution = true;
				Engine.NullRulesNum++;
			}

			// considero null la rule perchè non ha estratto dati, e quindi
			// mi permette di gestire le successive rule con NULL o NOT NULL
			foreach (Field rf in selectFields)
				rf.SetNullRuleData();

			return Engine.ApplyRuleFrom(RuleId + 1);
		}

		//-----------------------------------------------------------------------------
		public void ResetDataItem(Field pri, int i)
		{
			Field rf = (Field) selectFields[i];
			rf.Data = pri.Data;
		}
	}

	/// <summary>
	/// QueryRule
	/// </summary>
	//============================================================================
	class QueryRule :  RuleObj
	{
		public string			Name = string.Empty;
		public  QueryObject		Query = null;
		private WoormEngineExpression	whenExpr = null;

        private SelectMode constraint = SelectMode.ALL; //Woorm type selection
 
        //-----------------------------------------------------------------------------
        public QueryRule(RuleEngine engine) : base(engine)
		{
			whenExpr = new WoormEngineExpression	(engine.Report.Engine, Session, engine.RepSymTable.Fields);
		}

		//-----------------------------------------------------------------------------
		//FindKeyName ?
		public override bool Contains (string fieldName)
		{
			if (whenExpr.HasMember(fieldName)) 
                return true;
			if (Query != null && Query.HasMember(fieldName)) 
                return true;
			return false;
		}

		//-----------------------------------------------------------------------------
		public override bool IsParentOf (RuleObj ruleObj)
		{
			if (ruleObj.Sorted)
				return HasSon(ruleObj);
				
			return false;//TODO parametri di input ?
		}

		//-----------------------------------------------------------------------------
		public bool	Parse (Parser parser)
		{
			if (parser.ParseTag(Token.QUERY))
			{
                parser.ParseID( out Name);

                constraint = SelectMode.ALL;

                if (parser.Matched(Token.NOT) && parser.ParseTag(Token.NULL))
                    constraint = SelectMode.NOT_NULL;
                else
                    if (!parser.Error && parser.Matched(Token.NULL))
                    constraint = SelectMode.NULL;
                else
                    if (parser.Error)
                    return false;

                if (parser.Matched(Token.WHEN))
				{
					whenExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
					if (!whenExpr.Compile(parser, CheckResultType.Match, "Boolean"))
						return false;
				}
                parser.ParseTag(Token.SEP);
			}
			return !parser.Error;
		}

		//-----------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.QUERY, false);
			unparser.WriteID(this.Name, false);
			
			if (!whenExpr.IsEmpty)
			{
				unparser.WriteLine();
				unparser.WriteTag(Token.WHEN, false);
				unparser.WriteExpr(whenExpr.ToString());
			}
			unparser.WriteTag(Token.SEP, true);

			return true;
		}

        //---------------------------------------------------------------------
        private RuleReturn ExecQuery()
        {
            if (engine.NullRulesNum != 0 && IsSonOfNullRules())
                return RuleReturn.Backtrack;

            //-----
            if (Query == null)
            {
                QueryObject qry = engine.RepSymTable.QueryObjects.Find(Name);
                if (qry == null)
                {
                    engine.SetError(string.Format("Named query rule {0} is missing", Name));
                    return RuleReturn.Abort;
                }
                Query = qry;
                qry.IsQueryRule = true;
            }

            //-----
            if (!Query.Open())
                return RuleReturn.Abort;

            return RuleReturn.Success;
        }

        //---------------------------------------------------------------------
        private RuleReturn ExecSubQuery(ref bool bRecFoundForConstraint)
        {
            RuleReturn rr = RuleReturn.Success;
            if (Query.IsEmpty())
                return rr;

            while (Query.Read())
            {
                if (constraint == SelectMode.NULL)
                {
                    bRecFoundForConstraint = true;
                    break;
                }

                failed = false;

                if (nullSolution)
                {
                    nullSolution = false;
                    engine.NullRulesNum--;
                }

                // this statement musts be in this place after fields updating
                rr = engine.ApplyRuleFrom(RuleId + 1);

                switch (rr)
                {
                    case RuleReturn.Abort:
                        return RuleReturn.Abort;

                    case RuleReturn.Backtrack:
                        {
                            if (!IsParentOfFailedRules())
                                return RuleReturn.Backtrack;
                            break;
                        }

                    case RuleReturn.Success:
                        {
                            bRecFoundForConstraint = true;
                            break;
                        }
                }
            }
            return RuleReturn.Success;
        }


        //-----------------------------------------------------------------------------
        public override RuleReturn Apply()
        {
            if (!whenExpr.IsEmpty)
            {
                Value cond = whenExpr.Eval();
                if (whenExpr.Error)
                {
                    engine.SetError(string.Format("When-clause of named query rule {0} is invalid", Name));
                    return RuleReturn.Abort;
                }

                if (!(bool)(cond.Data))
                {
                    engine.NullRulesNum--;
                    return engine.ApplyRuleFrom(RuleId + 1);
                }
            }
            //----

            RuleReturn rr = ExecQuery();

            bool bRecFoundForConstraint = false;
            switch (rr)
            {
                case RuleReturn.Abort:
                    return RuleReturn.Abort;

                case RuleReturn.Success:
                    {
                        RuleReturn rr1 = ExecSubQuery(ref bRecFoundForConstraint);

                        if (rr1 != RuleReturn.Success)
                            return rr1;

                        if (bRecFoundForConstraint && (constraint != SelectMode.NULL))
                        {
                            failed = false;
                            return RuleReturn.Success;
                        }

                        break;
                    }
            }

            if (
                    (!bRecFoundForConstraint && (constraint == SelectMode.NOT_NULL)) ||
                    (bRecFoundForConstraint && (constraint == SelectMode.NULL))
                )
            {
                failed = true;
                engine.SetError(string.Format("No data extracted from named query rule {0}", Name));

                return RuleReturn.Backtrack;
            }

            failed = false;

            if (!nullSolution)
            {
                nullSolution = true;
                engine.NullRulesNum++;
            }

            // make null all own fields
            for (int i = 0; i < GetSelFieldsNum(); i++)
                GetSelField(i).SetNullRuleData();

            return engine.ApplyRuleFrom(RuleId + 1);
        }
    
        //---------------------------------------------------------------------
        public int GetSelFieldsNum()
        {
            if (Query == null) 
                return 0;

            return Query.GetColumnCount();
        }

        public string GetSelFieldName(int i)
        {
            if (Query == null)
                return null;
            return Query.GetColumnName(i); ;
        }
        
        public Field GetSelField(int i)
        {
            string name  = GetSelFieldName(i);
            if (name == null)
                return null;

            Field f = this.Engine.RepSymTable.Fields.Find(name);
            return f;
        }
 	}

    /// <summary>
    /// WhileRule
    /// </summary>
    //============================================================================
    class WhileRule : RuleObj
	{
		private WoormEngineExpression		condExpr = null;

		//-----------------------------------------------------------------------------
		public WhileRule(RuleEngine engine) : base(engine)
		{
			condExpr = new WoormEngineExpression	(engine.Report.Engine, Session, engine.RepSymTable.Fields);
		}

		//-----------------------------------------------------------------------------
		//FindKeyName ?
		public override bool Contains (string fieldName)
		{
			if (condExpr.HasMember(fieldName)) return true;
			return false;
		}

		//-----------------------------------------------------------------------------
		public override bool IsParentOf (RuleObj ruleObj)
		{
			if (ruleObj.Sorted)
				return HasSon(ruleObj);
				
			return false;
		}

		//-----------------------------------------------------------------------------
		public bool	Parse (Parser lex)
		{
			if (lex.ParseTag(Token.WHILE))
			{
				condExpr.StopTokens = new StopTokens(new Token[] { Token.DO });
				if (!condExpr.Compile(lex, CheckResultType.Match, "Boolean"))
					return false;
				lex.ParseTag(Token.DO);
			}
			return !lex.Error;
		}

		//-----------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			if (!condExpr.IsEmpty) 
			{
				unparser.WriteTag(Token.WHILE, false);
				unparser.WriteExpr(condExpr.ToString());
				unparser.WriteTag(Token.DO, true);
			}
			return true;
		}

		//-----------------------------------------------------------------------------
		public override RuleReturn	Apply	()
		{
			failed = false;
			nullSolution = false;
			engine.NullRulesNum--;

			for(;;)
			{
				Value cond = condExpr.Eval();
				if (condExpr.Error)
				{
					engine.SetError(string.Format("Rule While is invalid"));
					return RuleReturn.Abort;
				}

				if (!(bool)(cond.Data)) 
					break;

				RuleReturn rr = engine.ApplyRuleFrom(RuleId + 1);	
			}
			return RuleReturn.Success;
		}
	}
}