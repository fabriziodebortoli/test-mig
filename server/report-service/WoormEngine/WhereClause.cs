using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Microarea.Common.Generic;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;
using Microarea.Common;

namespace Microarea.RSWeb.WoormEngine
{
    /////////////////////////////////////////////////////////////////////////
    //
    // <Native>				::=	NATIVE <sql_expression> | ALL | BREAK
    // <SqlLike>			::= <SqlLikeExpression> | ALL | BREAK
    // <base_where_clause>	::= <Native> | <SqlLike>
    // <WhereClauseExpr>		::=	<base_where_clause> | < CondWhereClause>
    // <CondWhereClause>	::=	IF <bool_expression>
    //								THEN <where_clause> | ALL | BREAK
    //								[ ELSE <where_clause> | ALL | BREAK ]
    //
    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// ParamItem
    /// </summary>
    //=========================================================================
    public class ParamItem
	{
		private string name;
		private DataItem item;
		private bool isValueContentOf = false;  //se e' un valueContentOf devo sostiturlo io nel commandText, perche se lo passo come parametro fa il "quote" della stringa
		
		//-----------------------------------------------------------------------------
		public object Data { get { return (item == null) ? null : ObjectHelper.CastToDBData(item.Data); } }
		//-----------------------------------------------------------------------------
		public string Name { get { return name; } }
		//-----------------------------------------------------------------------------
		public bool IsValueContentOf { get { return isValueContentOf; } }

		//-----------------------------------------------------------------------------
		public ParamItem (string name, DataItem item)
		{
			this.name = name;
			this.item = item;
		}
		//-----------------------------------------------------------------------------
		public ParamItem(string name, DataItem item, bool isValueContentOf)
		{
			this.name = name;
			this.item = item;
			this.isValueContentOf = isValueContentOf;
		}
	}

	/// <summary>
	/// WhereClauseExpr
	/// </summary>
	//=========================================================================
	//class TB_EXPORT WClauseExpr : public Expression
	public class WhereClauseExpr : Expression
	{
		static int paramNo = 1;

        private bool native = false;

		protected bool	firstBind = true;

        public enum EClauseType { WHERE, HAVING, JOIN_ON };
        private EClauseType clauseType = EClauseType.WHERE;
        public EClauseType ClauseType { get => clauseType; set => clauseType = value; }

        public enum EJoinType { INNER, LEFT_OUTER, RIGHT_OUTER, FULL_OUTER, CROSS };
        public EJoinType joinType = EJoinType.CROSS;
        public EJoinType JoinType { get => joinType; set { joinType = value; clauseType = EClauseType.JOIN_ON;  } }

        protected SelectedTable joinedTable = null;
        public SelectedTable JoinedTable { get => joinedTable; set { joinedTable = value;  } }

        protected DataTableRule	dataTableRule = null;
		protected Token			emptyWhere = Token.NOTOKEN;

		protected string	        sql = "";
		protected List<ParamItem>	parameters = new List<ParamItem>();

        public List<string> ForbiddenIdents = new List<string>();

		//-----------------------------------------------------------------------------
		public bool Native						{ get { return native; } }
		virtual public string Sql				{ get { return sql; } }
		virtual public List<ParamItem>	Parameters	{ get { return parameters; }}

        public override  bool IsWClause { get { return true; } }

        // è necessario disabilitare il check perchè si inseriscono nella espressione
        // delle variabili al posto costanti ma il tipo non lo conosco e quindi
        // creo un dato qualsiasi tanto mi serve solo il nome della variabile.
        //-----------------------------------------------------------------------------
        public WhereClauseExpr
			(
				TbSession				session,
				SymbolTable					symTable,
				DataTableRule				dataTableRule
			)
			:
			base(session, symTable)
		{	
			this.dataTableRule = dataTableRule;
		}

		//-----------------------------------------------------------------------------
		protected override Value ResolveSymbol(Variable var)
		{
			Debug.Assert(SymbolTable != null);

			bool physicalCol = dataTableRule.IsQualifiedName(var.Name);

			bool publicField = SymbolTable.Contains(var.Name);

            bool bForbidden = ForbiddenIdents.Contains(var.Name, StringComparer.OrdinalIgnoreCase);

            if (physicalCol && publicField)
            {
                if (bForbidden)
                    publicField = false;
               // throw (new Exception(WoormEngineStrings.NameAmbiguity + var.Name));
            }
            else if (publicField && bForbidden)
            {
                throw (new Exception(WoormEngineStrings.IllegalFieldName + ": " + var.Name));
            }

			int nDot = var.Name.IndexOf('.');
			string tableName = (nDot > 0) ? var.Name.Substring(0, nDot) : "";
			string columnName = (nDot > 0) ? var.Name.Substring(nDot + 1) : var.Name;

			//possibile colonna NON qualificata: 
			if (!physicalCol && !publicField && nDot < 0)
			{
				tableName = dataTableRule.FromTables.GetTableNameFromColumnName(columnName); 
				if (tableName != string.Empty)
				{
					physicalCol = true;	//altrimenti si schianta la punto *1 perche var.Data è null (anche il dataType)
					var.Name = tableName + "." + var.Name;
				}
				else
				{
					SetError(string.Format(WoormEngineStrings.FieldNotColumn, columnName));
					return null;
				}
			}

			//possibile colonna qualificata con un alias: 
			if (!physicalCol && !publicField && nDot > 0)
			{
				string tableAlias = tableName;
				tableName = dataTableRule.FromTables.GetTableNameFromAlias(tableAlias);
				if (tableName == null)
				{
					SetError(string.Format(WoormEngineStrings.UnknownTableAlias, tableAlias));
					return null;
				}
				physicalCol = true;
			}

			if (publicField)		
				return base.ResolveSymbol(var);
			else if (!physicalCol)
			{
				return new Value(var.Data);	// *1
			}

			string columnType = dataTableRule.FromTables.GetColumnType(tableName, columnName);
			if (columnType == null)
			{
				SetError(string.Format(WoormEngineStrings.IllegalDataType, "") + ':' + tableName + '.' + columnName);
				return null;
			}

			object obj = ObjectHelper.CreateObject(columnType);
			return new Value(obj);
		}

		//-----------------------------------------------------------------------------
		public virtual bool Unparse(Unparser unparser, bool emitSqltag = false)
		{
			if (Native)
				unparser.WriteTag(Token.NATIVE, false);

			unparser.WriteExpr(this.ToString(), false);

			return true;
		}

		//-----------------------------------------------------------------------------
		public override bool Compile(Parser lex, CheckResultType check, string type)
		{
			expressionStack.Clear();

			if (lex.Matched(Token.BREAK))
			{
				emptyWhere = Token.BREAK;
				return true;
			}

			if (lex.Matched(Token.ALL))
			{
				emptyWhere = Token.ALL;
				return true;
			}

			emptyWhere = Token.NOTOKEN;

			if (lex.Matched(Token.NATIVE) || Native)
				return ParseNative(lex);

            // si imposta altri token di chiusura dell'espressione (perchè T_WHERE ?)
            if (ClauseType == EClauseType.WHERE)
                StopTokens = new StopTokens(new Token[] { Token.ELSE, Token.GROUP, Token.HAVING, Token.ORDER, Token.WHEN });
            else if (ClauseType == EClauseType.HAVING)
                StopTokens = new StopTokens(new Token[] { Token.ELSE, Token.ORDER, Token.WHEN });
            else if (ClauseType == EClauseType.JOIN_ON)
                StopTokens = new StopTokens(new Token[] { Token.ELSE,
                                            Token.INNER, Token.CROSS, Token.FULL, Token.LEFT, Token.RIGHT, Token.SELECT
                                             });
            else
            {
                lex.SetError(ExpressionManagerStrings.SyntaxError);
                return false;
            }

            // per il lex dell'espressione e` necessario conoscere il tipo di ritorno
            // per poter effettuare gli opportuni check di congruenza, una Sql Clause
            // per definizione dovrebbe tornare true o false
            if (!(base.Compile(lex, CheckResultType.Match, "Boolean")))
				return false;

			// Se siamo arrivati in questo metodo vuol dire che e` stato trovato il token
			// Token.WHERE e quindi l'espressione non puo` essere vuota
			if (IsEmpty)
			{
				lex.SetError(ExpressionManagerStrings.SyntaxError);
				return false;
			}

			return true;
		}

		// siccome deve simulare una vera compilazione di espressione devo anche inizializzare
		// il tipo di ritorno (resultType) come se avessi realmente compilato l'espressione
		//-----------------------------------------------------------------------------
		protected bool ParseNative(Parser lex)
		{
			resultType = "Boolean";
			emptyWhere = Token.NOTOKEN;
			native = true;
			expressionStack.Clear(); 
		
			int openRoundBrackets = 0;
            int openCASE = 0;

            /*TODO riccardo: da expression
			 * string preExprAudit = string.Empty;
			 * bool currDoAudit = parser.EnableAudit;
            if (currDoAudit)
                preExprAudit = parser.GetAuditString();
			 * */
            lex.EnableAudit = true; this.auditExpr = string.Empty;

			while (!lex.Error)
			{
				Token tk = lex.LookAhead();

                if (tk == Token.CASE)
                    openCASE++;

                if (openCASE > 0 && tk == Token.END)
                    openCASE--;

                if (tk == Token.EOF || tk == Token.SEP || tk == Token.ELSE)
					break;

				if	(tk == Token.ROUNDOPEN)			openRoundBrackets++;
				else if	(tk == Token.ROUNDCLOSE)	openRoundBrackets--;

                if (openRoundBrackets == 0)
                {
                    if (openCASE == 0 && tk == Token.WHEN)
                        break;

                    if (ClauseType == EClauseType.WHERE)
                    {
                        if (tk == Token.GROUP || tk == Token.HAVING || tk == Token.ORDER)
                            break;
                    }
                    else if (ClauseType == EClauseType.JOIN_ON)
                    {
                        if (tk == Token.SELECT || tk == Token.INNER || tk == Token.LEFT || tk == Token.RIGHT || tk == Token.CROSS || tk == Token.FULL)
                            break;
                    }
                    else if (ClauseType == EClauseType.HAVING)
                    {
                        if (tk == Token.ORDER)
                            break;
                    }
                }

                if (!ParseVariableOrConstForNativeWhere(lex))
					break;
			}

			string subExpression = lex.GetAuditString();
			this.auditExpr += ' ' + subExpression;

			lex.EnableAudit = false;

			if (lex.Error  || openRoundBrackets != 0)
				return false;

			if (this.auditExpr.Length > 0)
				expressionStack.Push(new Value((object)subExpression));
			
			/*
			if (currDoAudit)
                parser.SetAuditString (preExprAudit.IsNullOrEmpty() ? this.auditExpr : preExprAudit + ' ' + this.auditExpr);
            else
                parser.EnableAudit = false;
			 */
			return true;
		}

		// mette nello stack tutto quello che viene skippato con la SkipToken
		//-----------------------------------------------------------------------------
		private void PushSkippedToken(string skipped)
		{
			// mette nello stack tutto quello che viene skippato con la SkipToken
			if (skipped.Length > 0)
			{
				string s = skipped;
				expressionStack.Push(new Value((object)s));
			}
		}

		// identifica nella sql nativa solo le costanti e la variabili di symbol table
		//-----------------------------------------------------------------------------
		protected bool ParseVariableOrConstForNativeWhere(Parser lex)
		{
			switch (lex.LookAhead())
			{
				case Token.ID:
				{
					string subExpression = lex.GetAuditString();
					this.auditExpr += ' ' + subExpression;

					string columnName;
					if (!lex.ParseID(out columnName)) 
                        return false;

					if (SymbolTable.Contains(columnName))
					{
                        if (!ForbiddenIdents.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                        {
                            PushSkippedToken(subExpression);

							this.auditExpr += ' ' + lex.GetAuditString();

                            // metto una variabile fittizzia che mi permette di fare il giusto bind
                            expressionStack.Push(new Variable(columnName));
                            break;
                        }
					}

					// reset della stringa di audit
					this.auditExpr += ' ' + lex.GetAuditString();

					// altrimenti e` un token del linguaggio SQL
					subExpression += columnName + " ";
					string s2 = subExpression;
					expressionStack.Push(new Value((object) s2));
					
					break;
				}
			
				case Token.BRACEOPEN :
				{
					string subExpression = lex.GetAuditString(); 
                    lex.SkipToken();

					this.auditExpr += ' ' + subExpression;
					PushSkippedToken(subExpression);

					object data = null;

                    if (lex.Matched(Token.EVAL))
                    {
                        Expression expr = new Expression(TbSession, this.symbolTable);
                        expr.StopTokens = new StopTokens(new Token[] { Token.BRACECLOSE });
                        expr.StopTokens.skipInnerBraceBrackets = true;
                        if (!expr.Compile(lex, CheckResultType.Compatible, "Variant"))
                        {
                            //SetError(_TB("Error on parsing conditional expression of query tag"));
                            return false;
                        }
                        if (!lex.ParseTag(Token.BRACECLOSE))
                            return false;

                        Value valRes = expr.Eval();

                        if (valRes == null || valRes.Data == null)
                        {
                            return false;
                        }

                        data = valRes.Data;
                    }
                    else
                    {
                        if (!ComplexDataParser.Parse(lex, TbSession.Enums, out data, false))
                            return false;
                    }

					this.auditExpr += ' ' + lex.GetAuditString();

					string paramName = Token.NATIVE + paramNo.ToString(); 
                    paramNo++;

					expressionStack.Push(new Variable(paramName, 0, null, 0, data));

					break;
				}
			
				case Token.TEXTSTRING:
				{
					string subExpression = lex.GetAuditString(); 
					this.auditExpr += ' ' + subExpression;

					PushSkippedToken(subExpression);

					string	aString;
					if (!lex.ParseString(out aString)) return false;

					this.auditExpr += ' ' + lex.GetAuditString();
					string paramName = Token.NATIVE + paramNo.ToString(); paramNo++;
					expressionStack.Push(new Variable(paramName, 0, null, 0, (object)aString));

					break;
				}

				case Token.TRUE:
				case Token.FALSE:
				{
					string subExpression = lex.GetAuditString();
					this.auditExpr += ' ' + subExpression;  

					PushSkippedToken(subExpression);
					
					Token token = lex.LookAhead(); lex.SkipToken();
					bool cond = token == Token.TRUE;

					this.auditExpr += ' ' + lex.GetAuditString();

					string paramName = Token.NATIVE + paramNo.ToString(); paramNo++;
					expressionStack.Push(new Variable(paramName, 0, null, 0, (object)cond));

					break;
				}

		        case Token.CONTENTOF:
		        {
					string subExpression = lex.GetAuditString();
					this.auditExpr += ' ' + subExpression;

					PushSkippedToken(subExpression);

                    ExpressionParser ep = new ExpressionParser(this.TbSession, this.symbolTable, null);

			        if (!ep.ParseFunctionContentOf(lex, expressionStack, true)) 
				        return false;

					this.auditExpr += ' ' + lex.GetAuditString();
			        break;
		        }

				default :
					lex.SkipToken();
					break;
			}
			
			return true;
		}

		//-----------------------------------------------------------------------------
        bool ConvertToNative(string source, out string where)
        {
            return ConvertToNative
                (
                    SymbolTable, 
                    TbSession,
                    ForbiddenIdents,
                    source, out where
               );
        }

 		//-----------------------------------------------------------------------------
        static public bool ConvertToNative
            (
                SymbolTable symTable, 
                TbSession ReportSession,
                List<String> ForbiddenIdents,
                string source, out string where
            )
        {
            where = string.Empty;
 
            Parser lex = new Parser (Parser.SourceType.FromString);
            lex.Open(source);
	        lex.EnableAudit = true;

	        while (!lex.Error)
	        {
		        Token tk = lex.LookAhead();
		        if	(tk == Token.EOF)
			        break;

		        switch (tk)
		        {
			        case Token.ID:
			        {
				        where += lex.GetAuditString();

				        string name;
				        if (!lex.ParseID(out name)) 
					        return false;

                        Variable var = symTable.Find(name);
				        if (var != null && var.Data != null)
				        {
                            if (ForbiddenIdents != null && ForbiddenIdents.Contains(name, StringComparer.OrdinalIgnoreCase))
                                break;

                                string nat = ""; //TBDatabaseType.DBNativeConvert    TODO RSweb
                                           //    (
                                           //        var.Data,
                                           //        ReportSession.UserInfo.UseUnicode,
                                           //        TBDatabaseType.GetDBMSType(ReportSession.Provider)
                                           //    );

                            where += nat + ' ';

					        lex.GetAuditString();
					        break;
				        }
				        // altrimenti e` un token del linguaggio SQL
				        break;
			        }
	
			        case Token.BRACEOPEN :
			        {
				        where += lex.GetAuditString();
                        lex.SkipToken();

                        object data = null;

                        if (lex.Matched(Token.EVAL))
                        {
                            Expression expr = new Expression(ReportSession, symTable);
                            expr.StopTokens = new StopTokens(new Token[] { Token.BRACECLOSE });
                            expr.StopTokens.skipInnerBraceBrackets = true;
                            if (!expr.Compile(lex, CheckResultType.Compatible, "Variant"))
                            {
                                //SetError(_TB("Error on parsing conditional expression of query tag"));
                                return false;
                            }
                            if (!lex.ParseTag(Token.BRACECLOSE))
                                return false;

                            Value valRes = expr.Eval();

                            if (valRes == null || valRes.Data == null)
                            {
                               return false; 
                            }

                            data = valRes.Data;
                        }
                        else
                        {
                            if (!ComplexDataParser.Parse(lex, ReportSession.Enums, out data, false))
                                return false;
                        }

                            string nat = "";/* TBDatabaseType.DBNativeConvert
                            (
                                data,
                                ReportSession.UserInfo.UseUnicode,
                                TBDatabaseType.GetDBMSType(ReportSession.Provider)
                            );   TODO rsweb */

                        where += nat + ' ';
 
				        lex.GetAuditString();
				        break;
			        }
	
			        case Token.TEXTSTRING:
			        {
				        where += lex.GetAuditString();

				        string s;
				        if (!lex.ParseString (out s)) 
					        return false;

                            string nat = "";/* TBDatabaseType.DBNativeConvert
                            (
                                s,
                                ReportSession.UserInfo.UseUnicode,
                                TBDatabaseType.GetDBMSType(ReportSession.Provider)
                            );               TODO rsweb */ 

                        where += nat + ' ';

				        lex.GetAuditString();
				        break;
			        }
	
			        case Token.TRUE:
			        case Token.FALSE:
			        {
				        where += lex.GetAuditString();   
			
				        lex.SkipToken();

                        bool cond = (tk == Token.TRUE);

                            string nat = "";/* TBDatabaseType.DBNativeConvert

                            (
                                cond,
                                ReportSession.UserInfo.UseUnicode,
                                TBDatabaseType.GetDBMSType(ReportSession.Provider)
                            );              TODO rsweb */

                        where += nat + ' ';

				        lex.GetAuditString();
				        break;
			        }

 			        default :
				        lex.SkipToken();
				        break;
		        }
	        }
	
	        where += lex.GetAuditString();   
            return true;
        }

        //-----------------------------------------------------------------------------
        string GetPrefixParamName(string name = "Param") 
        {
	        switch (ClauseType)
            {
	        case EClauseType.WHERE:
                name += "_w_";
                break;
	        case EClauseType.JOIN_ON:
            {
                int tableIndex = 1;
                if (this.dataTableRule != null && this.JoinedTable != null)
                {
                    tableIndex = this.dataTableRule.FromTables.IndexOf(this.JoinedTable) + 1;
                }

                name += "_j_" + tableIndex.ToString() + '_';
                break;
            }
	        case EClauseType.HAVING:
                name += "_h_";
                break;
            }
            
	        return '@' + name + (parameters.Count + 1).ToString();
        }

        //-----------------------------------------------------------------------------
        // Le costanti e le variabili utente devono essere sostituite con
        // "@Value" o "@NomeVariabile" come vuole la sintassi per il binding dei parametri.
        // Anche le costanti sono bindate come parametri. in questo modo poiche cosi` facendo non ci
        // si deve porre il problema della convenzione di "quoting" dello specifico DataBase
        // lasciando all'interfaccia tra DataObj e parametro di ODBC il compito di quotare
        // correttamente. 
        //
        // NOTA : si usano i Token "Value" e "Native" per essere sicuri di non collidere con nomi
        // di variabili usate nella Sql, anche perche sono parole chiave e non possono essere utilizzate
        // come nomi di variabili.
        //
        // Devo anche spostare il tutto in un'altro stack perchè posso cambiare alcuni item 
        // da Value a Variable e non posso farlo all'interno del foreach.
        //-----------------------------------------------------------------------------
        protected bool ModifyVariableOrConst(ref Stack<Item> modifiedStack)
		{
			Stack<Item> tempStack = new Stack<Item>();
			foreach (Item item in modifiedStack)
			{
				if (item is OperatorItem)
				{
					if	(
							!ModifyVariableOrConst(ref ((OperatorItem)item).FirstStack) ||
							!ModifyVariableOrConst(ref ((OperatorItem)item).SecondStack)
						)
						return false;

					tempStack.Push(item);
					continue;
				}

				if (item is Variable)
				{
					Variable itemVar = item as Variable;

					// si verifica che sia un campo della tabella corrente o di symbol table
					bool physicalCol = dataTableRule.IsQualifiedName(itemVar.Name);

					Variable v = SymbolTable.Find(itemVar.Name);

                    if (v != null && physicalCol)
                    {
                        if (ForbiddenIdents.Contains(itemVar.Name, StringComparer.OrdinalIgnoreCase))
                            v = null;
                    }

					if (v != null)
					{
                        //Woorm tollera l'ambiguità
                        //if (physicalCol)
                        //    throw (new Exception(WoormEngineStrings.NameAmbiguity + itemVar.Name));
                        itemVar.Name = GetPrefixParamName(itemVar.Name);
                        //vedi an.17363
                        parameters.Add(new ParamItem(itemVar.Name, v));
					}
					else
					{
						if (!physicalCol)
						{
							SetError(ExpressionManagerStrings.UnknownField);
							return false;
						}

						// se e` un campo di tabella viene "qualificato"
						itemVar.Name = dataTableRule.GetQualifiedName(itemVar.Name);
					}
					tempStack.Push(itemVar);
					continue;
				}

				if (item is Value)
				{
					Value valore = (Value) item;

					// Al posto del Value (costante) trovato si sostituisce con parametro
                    string paramName = this.GetPrefixParamName();

                    if (item is ValueContentOf)
                    {
                        paramName = '{' + paramName + '}' ;

                        string s = valore.Data as string;
                        if (s != null)
                        {
                            string w;
                            if (ConvertToNative(s, out w))
                                valore.Data = w;
                        }
                    }

					Variable itemVar = new Variable(paramName, 0, null, 0,valore.Data);

					parameters.Add(new ParamItem(paramName, valore, item is ValueContentOf));

					tempStack.Push(itemVar);
					continue;
				}

				//An.17362:Se e' una funzione non devo fare nulla, ma devo comunque inserirla 
				//nello stack temporaneo per non perderla (analogo al metodo ModifyVariableOrConst di TbOleDb\WClause.cpp)
				if (item is FunctionItem)
					tempStack.Push(item);
			}

			Stack<Item> reversed = new Stack<Item>();
			Utility.ReverseStack(tempStack, reversed);
			modifiedStack = reversed;
			return true;
		}

		// devo costruire la Sql e la collezione di parametri solo la prima volta
		//-----------------------------------------------------------------------------
		protected bool BindParams()
		{
			Debug.Assert(!Native);
			if (firstBind)
			{
				firstBind = false;
				
				// Ci si deve appoggiare ad uno stack temporaneo poiche` per ottenere
				// la espressione in formato infisso si usera` la classe ExpUnparse il
				// cui metodo Unparse svuota ricorsivamente lo stack di lavoro
				//
				Stack<Item> modifiedStack = new Stack<Item>();
				ExpressionParser.ExpandStack(expressionStack, ref modifiedStack);
				
				ModifyVariableOrConst(ref modifiedStack);

				// Finalmente si Unparsa l'espressione per ottenerne una sua versione in 
				// formato stringa
				//
				ExpressionUnparser expUnparse = new ExpressionUnparser();
				
				if (!expUnparse.Unparse(out sql, modifiedStack))
				{
					SetError(ExpressionManagerStrings.SyntaxError);
					return false;
				}
			}
			
			return true;
		}

		// devo costruire la Sql e la collezione di parametri solo la prima volta
		//-----------------------------------------------------------------------------
		protected bool BindParamsNative()
		{
			Debug.Assert(Native);
			if (firstBind)
			{
				firstBind = false;
				parameters = new List<ParamItem>();

				// Le costanti e le variabili utente devono essere sostituite con
				// "@Value" o "@NomeVariabile" come vuole la sintassi per il binding dei parametri.
				// Anche le costanti sono bindate in questo modo poiche cosi` facendo non ci
				// si deve porre il problema della convenzione di "quoting" dello specifico DataBase
				// lasciando all'interfaccia il compito di quotare correttamente
				//
				Stack<Item> reversed = new Stack<Item>();
				Utility.ReverseStack(expressionStack, reversed);
				foreach (Item item in reversed)
				{
					if (item is Variable)
					{
						Variable v = ((Variable) item);

						string paramName = GetPrefixParamName(v.Name);

						sql += paramName + ' ';

						// recupero il valore attuale e lo aggiungo ai parametri bindati
						// Sono sicuro che si tratta solo di variabili in symbol table o costanti speciali
						// e non nomi di tabelle perchè nel parsing ho costruito i parametri solo per loro
						Variable actualValue = ResolveSymbolByReference(v, true);

						// allora si tratta di una costante speciale che il parser della native sql
						// ha sostituito con una variabile temporanea contenente il valore della costante
						// e con un nome generato automaticamente
						parameters.Add(new ParamItem(paramName, (actualValue == null ? v : actualValue)));
						continue;
					}

					if (item is Value)
					{
						Value v = (Value) item;
						sql += v.Data.ToString() + ' ';
						continue;
					}

                    if (item is FunctionContentOfItem)
                    {
                        FunctionContentOfItem fco = (FunctionContentOfItem)item;
                        Value v = (Value) fco.Expand();
                        string s = v.Data as string;
                        if (s != null)
                        {
                            string w;
                            if (ConvertToNative(s, out w))
                                v.Data = w;
                        }
 						sql += v.Data.ToString() + ' ';
						continue;
					}
				}
			}

            sql = sql.Replace('\r', ' ').Replace('\n', ' ').Replace("==", "=").Replace("!=", "<>");
 
			return true;
		}

		//----------------------------------------------------------------------------------
		virtual public bool BuildSql()
		{
			if (emptyWhere == Token.NOTOKEN)
			{
				if (IsEmpty)	
                    return true;

				if (Native)		
                    return BindParamsNative();
				
				return BindParams();
			}
			
			if (emptyWhere == Token.BREAK)	
                return false;
			if (emptyWhere == Token.ALL)	
                return true;

			return true;
		}

		//-----------------------------------------------------------------------------
		public override bool HasMember(string name)
		{
			return base.HasMember(name);
		}

		//-----------------------------------------------------------------------------
		public override bool IsEmpty
		{
			get
			{
				if (emptyWhere != Token.NOTOKEN)
					return false;

				return base.IsEmpty && this.auditExpr.IsNullOrWhiteSpace();	//per l'unparser
			}
		}

        //-----------------------------------------------------------------------------
        override public Value ApplySpecializedFunction(FunctionItem function, Stack<Item> paramStack)
		{
			switch (function.Name)
			{
				case "ContentOf":
				{
					FunctionContentOfItem item = paramStack.Pop() as FunctionContentOfItem;
					if (item != null)
						return (Value)item.Expand();
					break;
				}
			}
			return null;
		}
	}

	/// <summary>
	/// IfWhereClause
	/// </summary>
	//=========================================================================
	//class name c++: WClause : public WClauseExpr
	public class IfWhereClause : WhereClauseExpr
	{
		protected Expression	    conditionExpression = null;
		protected WhereClauseExpr	thenWhereClause = null; //could be instance of class IfWhereClause too
        protected WhereClauseExpr	elseWhereClause = null; //could be instance of class IfWhereClause too

 		protected bool			    currentCondition = true;

        //-----------------------------------------------------------------------------
        new public EClauseType ClauseType
        {
            get => base.ClauseType;
            set
            {
                base.ClauseType = value;
                if (thenWhereClause != null) thenWhereClause.ClauseType = value;
                if (elseWhereClause != null) elseWhereClause.ClauseType = value;
            }
        }

        new public EJoinType JoinType
        {
            get => base.JoinType;
            set
            {
                base.JoinType = value;
                if (thenWhereClause != null) thenWhereClause.JoinType = value;
                if (elseWhereClause != null) elseWhereClause.JoinType = value;
            }
        }

        new public SelectedTable JoinedTable 
        { 
            get => joinedTable; 
            set 
            { 
                base.JoinedTable = value;
                if (thenWhereClause != null) thenWhereClause.JoinedTable = value;
                if (elseWhereClause != null) elseWhereClause.JoinedTable = value;
            }
        }

        //-----------------------------------------------------------------------------
        public IfWhereClause
            (
                TbSession       session,
                SymbolTable     symTable,
                DataTableRule   dataTableRule,
                SelectedTable   selectedTable     = null,
                EJoinType       joinType          = EJoinType.CROSS
            )
            :
            base(session, symTable, dataTableRule)
        {
            if (selectedTable != null)
            {
                JoinedTable = selectedTable;
                JoinedTable.JoinOnClause = this;

                JoinType = joinType;
           }
 
            Init();
        }

        //-----------------------------------------------------------------------------
        public void Init()
        {
            conditionExpression = null;
            thenWhereClause = null;
            elseWhereClause = null;
        }

        //-----------------------------------------------------------------------------
        override public string Sql		//SQL
		{ 
			get 
			{ 
				if (conditionExpression == null)
					return base.sql;

				//E' ammesso che non sia presente il ramo else nell'if
				//es. report JobTicketSheet in Manufacturing:
				//Sql: Where If ( w_ManufacturingPlus ) 
				//              Then MA_ItemsTechnicalData.Item == w_Bom And w_PrintTechnicalData == TRUE  
				if (currentCondition && thenWhereClause != null)
					return thenWhereClause.Sql;
				else if (elseWhereClause != null)
					return elseWhereClause.Sql;
				return "";
			}
		}

		//-----------------------------------------------------------------------------
		override public List<ParamItem> Parameters
		{
			get
			{
				if (conditionExpression == null)
					return base.parameters;

				//E' ammesso che non sia presente il ramo else nell'if
				//es. report JobTicketSheet in Manufacturing:
				//Sql If ( w_ManufacturingPlus ) 
				//Then MA_ItemsTechnicalData.Item == w_Bom And w_PrintTechnicalData == TRUE  
				//Order By  MA_ItemsTechnicalData.Item, MA_ItemsTechnicalData.Name ;
				if (currentCondition && thenWhereClause != null)
					return thenWhereClause.Parameters;
				else if (elseWhereClause != null)
					return elseWhereClause.Parameters;

				return new List<ParamItem>();
			}
		}

		//-----------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser, bool emitSqltag = false)
		{
            if (emitSqltag)
            {
                if (ClauseType == WhereClauseExpr.EClauseType.WHERE)
                    unparser.WriteTag(Token.WHERE, false);
                else if (ClauseType == WhereClauseExpr.EClauseType.HAVING)
                    unparser.WriteTag(Token.HAVING, false);
                else if (ClauseType == WhereClauseExpr.EClauseType.JOIN_ON)
                {
                    switch (this.JoinType)
                    {
                        case EJoinType.INNER:
                            unparser.WriteTag(Token.INNER, false);
                            unparser.WriteTag(Token.JOIN, false);
                            unparser.WriteID(this.joinedTable.TableName);
                            unparser.WriteTag(Token.ON, false);
                            break;
                        case EJoinType.LEFT_OUTER:
                            unparser.WriteTag(Token.LEFT, false);
                            unparser.WriteTag(Token.OUTER, false);
                            unparser.WriteTag(Token.JOIN, false);
                            unparser.WriteID(this.joinedTable.TableName);
                            unparser.WriteTag(Token.ON, false);
                            break;
                        case EJoinType.RIGHT_OUTER:
                            unparser.WriteTag(Token.RIGHT, false);
                            unparser.WriteTag(Token.OUTER, false);
                            unparser.WriteTag(Token.JOIN, false);
                            unparser.WriteID(this.joinedTable.TableName);
                            unparser.WriteTag(Token.ON, false);
                            break;
                        case EJoinType.FULL_OUTER:
                            unparser.WriteTag(Token.FULL, false);
                            unparser.WriteTag(Token.OUTER, false);
                            unparser.WriteTag(Token.JOIN, false);
                            unparser.WriteID(this.joinedTable.TableName);
                            unparser.WriteTag(Token.ON, false);
                            break;
                        case EJoinType.CROSS:
                            unparser.WriteTag(Token.CROSS, false);
                            unparser.WriteTag(Token.JOIN, false);
                            unparser.WriteID(this.joinedTable.TableName);
                            break;
                        default:
                            return false;
                    }
                }
            }

			if (conditionExpression != null)
			{
				unparser.WriteTag(Token.IF, false);
				unparser.WriteExpr(conditionExpression.ToString());
				unparser.WriteTag(Token.THEN, false);

				thenWhereClause.Unparse(unparser);

				if (elseWhereClause != null)
				{
					unparser.WriteTag(Token.ELSE, false);

					elseWhereClause.Unparse(unparser);
				}

				return true;
			}

			return base.Unparse(unparser);
		}

		//-----------------------------------------------------------------------------
		public override bool Compile(Parser lex, CheckResultType check, string type)
		{
			Init();

			if (lex.Matched(Token.NATIVE) || Native)
				return base.ParseNative(lex);
				
			if (!lex.LookAhead(Token.IF))
				return base.Compile(lex, check, type);

			return ParseCondWhere(lex, check, type);
		}

		//-----------------------------------------------------------------------------
		protected bool ParseCondWhere(Parser lex, CheckResultType check, string type)
		{
			if (!lex.ParseTag(Token.IF)) return false;
			
			conditionExpression = new Expression(TbSession, SymbolTable);
			
			conditionExpression.StopTokens = new StopTokens(new Token[] { Token.THEN });
			if (!(conditionExpression.Compile(lex, CheckResultType.Match, "Boolean"))) 
                return false;

			// siccome il vero ResultType è quello della clausola then o else, è necessario
			// inizializzare quello della classe da cui la Sql condizionale deriva. Con 
			// approssimazione possiamo utilizzare quello della condizione.
			resultType = conditionExpression.ResultType;
			
			if (!lex.ParseTag(Token.THEN)) 
                return false;
			
			if (lex.LookAhead(Token.IF))
			{	
				thenWhereClause = new IfWhereClause(TbSession, SymbolTable, dataTableRule);
                thenWhereClause.ForbiddenIdents = ForbiddenIdents;
				if (!((IfWhereClause)thenWhereClause).ParseCondWhere(lex, check, type)) 
                    return false;
			}
			else
			{
				thenWhereClause = new WhereClauseExpr(TbSession, SymbolTable, dataTableRule);
                thenWhereClause.ForbiddenIdents = ForbiddenIdents;
				if (!thenWhereClause.Compile(lex, check, type)) 
                    return false;
			}
			
			if (!lex.Matched(Token.ELSE)) 
                return true;
			
			if (lex.LookAhead(Token.IF))
			{
				elseWhereClause = new IfWhereClause(TbSession, SymbolTable, dataTableRule);
                elseWhereClause.ForbiddenIdents = ForbiddenIdents;
				return ((IfWhereClause)elseWhereClause).ParseCondWhere(lex, check, type);
			}
			else
			{
				elseWhereClause = new WhereClauseExpr(TbSession, SymbolTable, dataTableRule);
                elseWhereClause.ForbiddenIdents = ForbiddenIdents;
				if (!elseWhereClause.Compile(lex, check, type)) 
                    return false;
			}

			return true;
		}

		//-----------------------------------------------------------------------------
		public override bool BuildSql()
		{
			if (conditionExpression == null)
    			return base.BuildSql();

			Value v = conditionExpression.Eval();
			if (conditionExpression.Error)
			{
				SetError(conditionExpression.Diagnostic);
				return false;
			}
			
			currentCondition = (bool) v.Data;
			if (currentCondition)
			{
				if (!thenWhereClause.BuildSql())
				{
					SetError(thenWhereClause.Diagnostic);
					return false;
				}
					
				return true;
			}
					
			if (elseWhereClause != null)
			{
				if (!elseWhereClause.BuildSql())
				{
					SetError(elseWhereClause.Diagnostic);
					return false;
				}

				return true;
			}

			return true;
		}

		//-----------------------------------------------------------------------------
		public override bool HasMember(string name)
		{
			if (conditionExpression == null)
				return base.HasMember(name);

			return	conditionExpression.HasMember(name)		||
					thenWhereClause.HasMember(name)	||
					(elseWhereClause != null && elseWhereClause.HasMember(name));
		}

		//-----------------------------------------------------------------------------
		public override bool IsEmpty
		{
			get
			{
				if (conditionExpression == null)
					return base.IsEmpty;

				return	
					conditionExpression.IsEmpty		&&
					thenWhereClause.IsEmpty	&&
					(elseWhereClause == null || elseWhereClause.IsEmpty);
			}
		}
	}
}
