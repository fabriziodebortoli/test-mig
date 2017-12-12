using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Woorm.ExpressionManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Woorm.WoormEngine
{
	public enum TagType { IN, OUT, REF, COL, EXPAND, INCLUDE, EVAL };

	public class QueryObject : object, IDisposable
	{

		#region Protected Data Member

		private string	queryNameString		= string.Empty ;
		private string	queryTemplateString	= string.Empty ; //originale, completa dei tag
		private string	sqlString			= string.Empty ; //senza i tag, con i segnaposto dei parametri
		private string	errorInfoString		= string.Empty ;
		
		private long	queryHandle		= -1;

		private SymbolTable	symbolTable	= null;

		private bool			disposed		= false;//Track whether Dispose has been called.
		private TBConnection	tbConnection	= null; //m_pSqlSession
		private TBCommand		tbCommand		= null; //m_pSqlTable
		private IDataReader		iDataReader		= null; 

		private ArrayList			tagLinkArrayList	= null;
		private TbReportSession	session		= null;
		private QueryObject			parentQuery	= null;
		private int bindNumber = 0;

        public bool IsQueryRule = false;

        bool isOracle = false;
        bool isUnicode = false;
        DBMSType dbType = DBMSType.SQLSERVER;

        #endregion

        #region Property

        protected IDataReader DataReader { get { return parentQuery != null ? parentQuery.DataReader : iDataReader; }}

		public string	QueryTemplate	{ get { return queryTemplateString; }	set { queryTemplateString	= value; }}
		public string	Name			{ get { return queryNameString; }		set { queryNameString		= value; }}
		public long		Handle			{ get { return queryHandle; } }
		public string	SqlString		{ get { return sqlString; }	}

		public TBConnection	TbConnection { get { return tbConnection; }			set { tbConnection = value; }}

		#endregion

		#region Costructor + IDisposable

		public QueryObject(string name, SymbolTable aSymbolTable, TbReportSession aSession, QueryObject parent)
		{
			queryNameString		= name;
			symbolTable			= aSymbolTable;
			session				= aSession;
			tagLinkArrayList	= new ArrayList();
			parentQuery			= parent;
		}

		//------------------------------------------------------------------------------
		~QueryObject()
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
			{
				Clear();
			}
			disposed = true;         
		}

		//------------------------------------------------------------------------------
		public void Clear ()
		{
			if (iDataReader != null)
			{
				if (!iDataReader.IsClosed) iDataReader.Close();
				iDataReader.Dispose();
				iDataReader = null;
			}
			if (tbCommand != null)
			{
				tbCommand.Dispose();
				tbCommand = null;
			}
			if (tbConnection != null)
			{
                if (tbConnection.State != System.Data.ConnectionState.Closed) tbConnection.Close();
				tbConnection.Dispose();
				tbConnection = null;
			}
			queryHandle = -1;
			bindNumber = 0;
		}

		#endregion

		//-------------------------------------------------------------------------------
		public int AddLink (string name, TagType direction, object data, int len, Expression whenExpr, QueryObject expandClause)
		{
			return tagLinkArrayList.Add(new TagLink(name, direction, data, len, whenExpr, expandClause));
		}

		//------------------------------------------------------------------------------
		public bool HasMember (string name) 
		{
			for (int i = 0; i < tagLinkArrayList.Count; i++)
			{
				TagLink tagLink = (TagLink)(tagLinkArrayList[i]);

				if (string.Compare(tagLink.name, name, true, CultureInfo.InvariantCulture) == 0)
					return true;
                if (tagLink.expandClause != null && tagLink.expandClause.HasMember(name))
                    return true;
                if (tagLink.elseClause != null && tagLink.elseClause.HasMember(name))
                    return true;
            }
			return false;
		}

		//------------------------------------------------------------------------------
		public bool Parse (ref Parser parser)
		{
			tagLinkArrayList.Clear();

            if (this.session.UserInfo.LoginManager != null)
            {   //Il TbLocalizer NON ha il LoginManager
                string provider = this.session.UserInfo.LoginManager.ProviderName;
                dbType = TBDatabaseType.GetDBMSType(provider);
                isOracle = dbType == DBMSType.ORACLE;
                isUnicode = this.session.UserInfo.LoginManager.UseUnicode;
            }

            queryTemplateString = string.Empty;
			sqlString			= string.Empty;

			if (parser.Parsed(Token.QUERY)) 
			{
				if (!parser.ParseID(out queryNameString)) 
					return false;
			}

			if (!parser.ParseTag(Token.BEGIN)) 
				return false;
			
			if (!parser.ParseTag(Token.BRACEOPEN)) 
				return false;

			string	sPrecAuditString	= string.Empty;
			bool	bPrecAuditingState	= parser.DoAudit;
			
			if (bPrecAuditingState) 
				sPrecAuditString = parser.GetAuditString();
			else
				parser.DoAudit = true;

			if (!ParseInternal( ref parser))
				return false;

			if (bPrecAuditingState)
			{
				if (!parser.ParseTag(Token.BRACECLOSE))
					return false;

                //concateno i tre segmenti di audit aggiungendo un blank come separatore sse necessario
                //audit = sPrecAuditString + queryTemplateString + parser.GetAuditString()
                string sa = sPrecAuditString;
                if (sa.IsNullOrEmpty() || queryTemplateString.IsNullOrEmpty())
                    sa += queryTemplateString;
                else 
                    sa += ' ' + queryTemplateString;
 
                sa = sa.IsNullOrEmpty() ? parser.GetAuditString() : sa + ' ' + parser.GetAuditString();
               
				parser.SetAuditString (sa);
			}
			else
			{
				parser.DoAudit = false;
				if (!parser.ParseTag(Token.BRACECLOSE)) 
					return false;
			}

			if (!parser.ParseTag(Token.END)) 
				return false;

			parser.ClearErrors();
			
			Debug.Assert(!parser.Error);
			return !parser.Error;
		}
	
		//------------------------------------------------------------------------------
		private bool ParseInternal (ref Parser parser)
		{
			string auditString;

			while (!parser.LookAhead(Token.BRACECLOSE) && !parser.LookAhead(Token.EOF)) 
			{
				if (parser.LookAhead(Token.BRACEOPEN))
				{
					auditString = parser.GetAuditString();
					queryTemplateString += auditString;
					sqlString += auditString;

					if (!ParseTag(ref parser))
						return false;

					queryTemplateString += parser.GetAuditString();
					continue;
				}
				parser.SkipToken();
			} 

			auditString	= parser.GetAuditString();
			queryTemplateString += auditString;
			sqlString += auditString;
			
			Debug.WriteLine(string.Format("\nQuery Name: {0}:\nQuery: {1}\nSql: {2}\n", queryNameString, queryTemplateString, sqlString));
//			if (parser.LookAhead(Token.EOF))
//				return  false;
			return true;
		}

        //------------------------------------------------------------------------------
        private QueryObject ParseSubQuery(Parser parser)
        {
            QueryObject queryObject = new QueryObject(string.Empty, symbolTable, session, parentQuery != null ? parentQuery : this);

            if (!parser.ParseTag(Token.BRACEOPEN)) //apertura query di EXPAND
                return null;

            queryTemplateString += parser.GetAuditString();

            if (!queryObject.ParseInternal(ref parser))
                return null;

            queryTemplateString += queryObject.QueryTemplate;

            if (!parser.ParseTag(Token.BRACECLOSE)) //chiusura query di EXPAND
                return null;

            return queryObject;
        }

		//---------------------------------------------------------------------
		private bool ParseTag (ref Parser parser)
		{
			if (!parser.ParseTag(Token.BRACEOPEN)) 
				return false;

			string name	= string.Empty;
			object pObj	= null;
			Field field	= null;
  
			Token token = parser.LookAhead();
			switch (token)
			{
				case Token.COL:
				{
                    parser.SkipToken();
					if (parser.LookAhead() != Token.ID) 
						return false;

					if (!parser.ParseID( out name)) 
						return false;

					if (!symbolTable.Contains(name)) 
						return false;

					field = symbolTable.Find(name) as Field;
					Debug.Assert(field != null);

					if (field == null)
						return false;

					pObj = field.Data;
					if (pObj == null) 
						return false;

					AddLink(name, TagType.COL, pObj, field.Len, null, null);

					if (!parser.ParseTag(Token.BRACECLOSE)) 
						return false;

					return true;
				}
				case Token.IN:
				case Token.OUT:
				case Token.REF:
				{
                    parser.SkipToken();
					if (parser.LookAhead() != Token.ID) 
						return false;

					if (!parser.ParseID(out name)) 
						return false;

					if (!symbolTable.Contains(name)) 
						return false;
				
					field = symbolTable.Find(name) as Field;
					Debug.Assert(field != null);
					if (field == null)
						return false;

					pObj = field.Data;
					if (pObj == null) 
						return false;

					if (token == Token.IN)
						AddLink(name, TagType.IN, pObj, field.Len, null, null);
					else if (token == Token.OUT)
						AddLink(name, TagType.OUT, pObj, field.Len, null, null);
					else if (token == Token.REF)
						AddLink(name, TagType.REF, pObj, field.Len, null, null);
					else
					{
						Debug.Assert(false);
						return false;
					}

					TagLink tag = tagLinkArrayList[tagLinkArrayList.Count - 1] as TagLink;

                    if (parser.Parsed(Token.AS))
                        parser.ParseID(out tag.sqlStringName);
                    else
                        tag.sqlStringName = string.Format("@PAR_{0}_{1}", ++bindNumber, name);

					sqlString += string.Format(" {0} ", tag.sqlStringName);
				
					if (!parser.ParseTag(Token.BRACECLOSE)) 
						return false;

					return true;
				}
                case Token.PLUS:
	            {
                    parser.SkipToken();
		            if (!parser.ParseTag(Token.BRACECLOSE)) 
		            {
			            return false;
		            }

                    sqlString += isOracle ? " || " : " + ";
		
		            return true;
	            }
                case Token.FALSE:
                case Token.TRUE:
                {
                    parser.SkipToken();
                    if (!parser.ParseTag(Token.BRACECLOSE))
                    {
                        return false;
                    }
                    bool isTrue = token == Token.TRUE;
                     
                    string val = isTrue ? " '1' " : " '0' ";

                    sqlString += isOracle ? " CAST(" + val + " AS " + (isUnicode ? "NCHAR" : "CHAR") + ") " : val;

                    return true;
                }
                case Token.TEXTSTRING:
                {
                    string s;
                    if (!parser.ParseString(out s))
                    {
                         return false;
                    }
                    if (!parser.ParseTag(Token.BRACECLOSE))
                    {
                        return false;
                    }

                    int len = s.Length;
 
                    string nat = TBDatabaseType.DBNativeConvert(s, isUnicode, dbType);
                    if (nat.IsNullOrEmpty())
                    {
                        if (isOracle && len == 0)
                        {
                            len++;
                            s = " ";
                        }
                        nat = "\'" + s + "\'";
                        if (isUnicode)
                            nat = 'N' + nat;
                    }
                    else if (len == 0)
                        len++;

                    sqlString += ' ' + (isOracle ? " CAST(" + nat + " AS " + (isUnicode ? "NVARCHAR2(" : "VARCHAR2(") + len.ToString() +  ")) " : nat)  + ' ';
                    return true;
                }

				case Token.WHEN:
                    parser.SkipToken();
                    //bWhen = true;
					break;
				
                case Token.EVAL:
                {
                    parser.SkipToken();

		            Expression expr = new Expression (session, this.symbolTable);
                   	expr.StopTokens = new StopTokens(new Token[]{ Token.BRACECLOSE });
                    expr.StopTokens.skipInnerBraceBrackets = true;
 			        if (!expr.Compile(parser, CheckResultType.Compatible, "Variant"))
		            {
			            //SetError(_TB("Error on parsing conditional expression of query tag"));
			            return false;
		            }
                    if (!parser.ParseTag(Token.BRACECLOSE))
                        return false;

                    AddLink(name, TagType.EVAL, pObj, 0, expr, null);
                    TagLink tag = tagLinkArrayList[tagLinkArrayList.Count - 1] as TagLink;
                    tag.sqlStringName = string.Format("@PAR_{0}_{1}", ++bindNumber, name);

                    sqlString += string.Format(" {0} ", tag.sqlStringName);
                    return true;
                }
                default:
                {
                    object res = null;
                    if (!ComplexDataParser.Parse(parser, session.Enums, out res, false)) 
                        return false;

                    if (res != null)
                    {
                        string nat = TBDatabaseType.DBNativeConvert(res, isUnicode, dbType);
                        if (nat == null)
                            return false;

                        sqlString += ' ' + nat + ' ';
                        return true;
                    }
                    return false;
                }
			}

			Expression whenExpression = new Expression(session, symbolTable);
			whenExpression.StopTokens = new StopTokens(new Token[]{ Token.INCLUDE ,Token.EXPAND});
			if (!whenExpression.Compile(parser, CheckResultType.Match, "Boolean"))
				return false;

			if (parser.Parsed(Token.INCLUDE))
			{
				if (parser.LookAhead() != Token.ID) 
					return false;

				if (!parser.ParseID(out name)) 
					return false;

				if (!symbolTable.Contains(name)) 
					return false;
			
				field = symbolTable.Find(name) as Field;
				Debug.Assert(field != null);
				if (field == null)
					return false;

				pObj = field.Data;
				if (pObj == null || pObj.GetType() != typeof(string)) 
					return false;

				int		tagLinksNumber	= tagLinkArrayList.Count;
				string	marker		= string.Format("{{INCLUDE{0}}}", tagLinksNumber);

				AddLink(name, TagType.INCLUDE, null, 0, whenExpression, null);
				sqlString += string.Format(" {0} ", marker); //è un segnaposto che sarà sostituito in fase di Build
				((TagLink)tagLinkArrayList[tagLinksNumber]).sqlStringName = marker;

				if (!parser.ParseTag(Token.BRACECLOSE)) 
					return false;

				return true;	
			}

			if (!parser.ParseTag(Token.EXPAND)) 
				return false;

            QueryObject queryObject = ParseSubQuery(parser);
            if (queryObject == null)
                return false;

            QueryObject qryElse = null;
            if (parser.Parsed(Token.ELSE))
            {
                qryElse = ParseSubQuery(parser);
                if (qryElse == null)
                    return false;
            }

			if (!parser.ParseTag(Token.BRACECLOSE)) //chiusura tag WHEN - EXPAND - ELSE
				return false;

			int index = tagLinkArrayList.Count;
			name = string.Format("{{EXPAND{0}}}", index);
			AddLink(name, TagType.EXPAND, null, 0, whenExpression, queryObject);
			((TagLink)tagLinkArrayList[index]).sqlStringName = name;
            ((TagLink)tagLinkArrayList[index]).elseClause = qryElse;
			sqlString += string.Format (" {0} ", name);	//è un segnaposto che sarà sostituito in fase di Build

			return true;
		}


		//------------------------------------------------------------------------------
		private bool Build (bool isProcedure = false)
		{
			Clear();

			string strSql = sqlString;

			if (!ExpandTemplate( ref strSql)) 
				return false;

            strSql = strSql.StripBlankNearSquareBrackets();
            strSql = strSql.Replace('\r', ' ');
            strSql = strSql.Replace('\n', ' ');
            strSql = strSql.Trim();

			if (TbConnection == null)
			{
				TbConnection = new TBConnection(this.session.CompanyDbConnection, TBDatabaseType.GetDBMSType(this.session.Provider));
				TbConnection.Open();
			}
			tbCommand = new TBCommand(TbConnection);
			tbCommand.CommandTimeout = 0;
			tbCommand.CommandText = strSql;

            int bindPos = 0;
            if (!BindParameter(tbCommand, ref bindPos, isProcedure)) 
				return false;
			return true;
		}

		//------------------------------------------------------------------------------
		private bool ExpandTemplate (ref string strSql)
		{
			for (int i = 0; i < tagLinkArrayList.Count; i++)
			{
				TagLink tagLink = (TagLink)tagLinkArrayList[i];

                if (tagLink.direction == TagType.EVAL)
                {
                    Value valRes = tagLink.whenExpr.Eval(); 
                    object res = null; 
                    if (valRes != null && (res = valRes.Data) != null)
                    {                         
                        string nat = TBDatabaseType.DBNativeConvert(res, isUnicode, dbType);
                        if (nat == null)
                            return false;
                        sqlString.Replace(tagLink.sqlStringName, nat);
                    }
                    continue;
                }

                if (
					tagLink.direction != TagType.EXPAND &&
					tagLink.direction != TagType.INCLUDE
					) continue;

				Debug.Assert(tagLink.whenExpr != null);

				Value expressionValue = tagLink.whenExpr.Eval();
				if (tagLink.whenExpr.Error)	
				{
					return false;
				}
				tagLink.isWhen = (bool) expressionValue.Data;
				
				//per gli INCLUDE: occorre istanziare un oggetto subquery e parsarlo al volo a partire del valore corrente della variabile di woorm sorgente
				if (tagLink.direction == TagType.INCLUDE)
				{
				    if (!tagLink.isWhen)
				    {
					    strSql = strSql.Replace(tagLink.sqlStringName, "");
					    continue;
				    }

					Field field = symbolTable.Find(tagLink.name) as Field;
					Debug.Assert(field != null);
					if (field == null) 
						return false;

					string inc =  field.Data as string;
					Debug.Assert(inc != null);
					if (inc == null) 
						return false;

					Parser parser = new Parser(Parser.SourceType.FromString);
					parser.DoAudit = true;
					if (!parser.Open(inc))
						return false;
					tagLink.expandClause = new QueryObject(string.Empty, symbolTable, session, parentQuery != null ? parentQuery : this);

					if (!tagLink.expandClause.ParseInternal(ref parser)) 
					{
						return false;
					}
				}

                QueryObject q = tagLink.isWhen ? tagLink.expandClause : tagLink.elseClause;
                if (q == null)
                {
                    strSql = strSql.Replace(tagLink.sqlStringName, "");
                    continue;
                }

				String sqlSubString = q.sqlString;
				q.ExpandTemplate(ref sqlSubString);

				strSql = strSql.Replace(tagLink.sqlStringName, sqlSubString);
			}
			return true;
		}

		//------------------------------------------------------------------------------
		private bool BindParameter (TBCommand tbCommand, ref int bindPos, bool isProcedure = false)
		{
			//int nDataLevel = 0; //TODO symbolTable.GetDataLevel();

			for (int i = 0; i < tagLinkArrayList.Count; i++)
			{
				TagLink tagLink = (TagLink) tagLinkArrayList[i];
                tagLink.bindPos = -1;

				if (
					tagLink.direction == TagType.EXPAND ||
					tagLink.direction == TagType.INCLUDE
					)
				{
					if (tagLink.isWhen)
						tagLink.expandClause.BindParameter(tbCommand, ref bindPos);
                    else if (tagLink.elseClause != null)
                        tagLink.elseClause.BindParameter(tbCommand, ref bindPos);
                    continue;
				}

				if (
					tagLink.direction != TagType.IN &&
					tagLink.direction != TagType.OUT &&
					tagLink.direction != TagType.REF
					) continue;
		
				ParameterDirection directionType = ParameterDirection.Input;
			
				switch (tagLink.direction)
				{
					case TagType.OUT:
						directionType = ParameterDirection.Output;
						break;
					case TagType.REF:
						directionType = ParameterDirection.InputOutput;
						break;
					case TagType.IN:
						directionType = ParameterDirection.Input;
						break;
					default:
						Debug.Assert(false);
						return false;
				}
				
	            tagLink.bindPos = bindPos++;

                int paramNumber = tbCommand.Parameters.Add(tagLink.sqlStringName, tagLink.data);

				TBParameter tbParameter = tbCommand.Parameters.GetParameterAt(paramNumber);
				tbParameter.Direction	= directionType;

				if (tagLink.direction == TagType.OUT) 
					continue;

				Field field = symbolTable.Find(tagLink.name) as Field;
                tbParameter.Value = ObjectHelper.CastToDBData(field.Data);
			}
			return true;	
		}
	
		//------------------------------------------------------------------------------
		public bool Open ()
		{
			try
			{
				if (!Build())
					return false;

				Debug.WriteLine(string.Format("Query {0} Sql:\n{1}\nend query\n", queryNameString, sqlString));
	
				iDataReader = tbCommand.ExecuteReader();

				return true;
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
				//TODO far manifestare il messaggio
				//throw(new TBException(e.Message, e));
				Close();
			}
			return false;
		}

		//------------------------------------------------------------------------------
		public bool Close ()
		{
			try
			{
				Clear();
			}
			catch
			{ 
			}
			return true;
		}

		//------------------------------------------------------------------------------
		private bool ClearColumns ()
		{
			Debug.WriteLine(string.Format("Clear columns\n", queryNameString));

			for (int i = 0; i < tagLinkArrayList.Count; i++)
			{
				TagLink tagLink = (TagLink)tagLinkArrayList[i];

				if (
					tagLink.direction == TagType.EXPAND ||
					tagLink.direction == TagType.INCLUDE
					)
				{
					if (tagLink.isWhen)
						tagLink.expandClause.ClearColumns();
                    else if (tagLink.elseClause != null)
                        tagLink.elseClause.ClearColumns();
                    continue;
				}

				if (
					tagLink.direction != TagType.COL &&
					tagLink.direction != TagType.OUT &&
					tagLink.direction != TagType.REF
					) continue;
	
				Field field = (Field) symbolTable.Find(tagLink.name);
                field.ClearAllData();

                field.RuleDataFetched = true;
				field.ValidRuleData = true;
			}
			return true;
		}

		//------------------------------------------------------------------------------
		private bool ValorizeColumns ()
		{
			Debug.WriteLine(string.Format("Query {0} Fetch row\n", queryNameString));

			//TODO int nDataLevel = 0; //REPORT_ENGINE;//symbolTable->GetDataLevel(); // RULE_ENGINE QUERY_ENGINE REPORT_ENGINE

			int columnIndex= 0;
			for (int i = 0; i < tagLinkArrayList.Count; i++)
			{
				TagLink tagLink = (TagLink)tagLinkArrayList[i];

				if (
					tagLink.direction == TagType.EXPAND ||
					tagLink.direction == TagType.INCLUDE
					)
				{
					if (tagLink.isWhen)
						tagLink.expandClause.ValorizeColumns();
                    else if (tagLink.elseClause != null)
                        tagLink.elseClause.ValorizeColumns();
                    continue;
				}

				if (
					tagLink.direction != TagType.COL &&
					tagLink.direction != TagType.OUT &&
					tagLink.direction != TagType.REF
					) continue;
	
				Field field = (Field) symbolTable.Find(tagLink.name);
                object o;
				if (tagLink.direction == TagType.COL)
				{
                    o = DataReader[columnIndex++];

					//Debug.WriteLine(string.Format("Column {0}: {1}\n", tagLink.name, field.Data.ToString()));
				}
				else
				{
                    o = tbCommand.Parameters[tagLink.sqlStringName];

					//Debug.WriteLine(string.Format("Parameter {0}: {1}\n", tagLink.name, field.Data.ToString()));
				}

                if (this.IsQueryRule)
                {
                    field.SetData(DataLevel.Events,  ObjectHelper.CastFromDBData(o, field.GetData(DataLevel.Events) ));
                }
                else
                    field.Data = ObjectHelper.CastFromDBData(o, field.Data);

                Debug.WriteLine(string.Format("Field {0}: {1}\n", tagLink.name, field.Data.ToString()));

                field.RuleDataFetched = true;
				field.ValidRuleData = true;
			}
			return true;
		}

		//------------------------------------------------------------------------------
		public bool Read ()
		{
            if (!IsOpen() && !Open())
                return false;
                
			if (iDataReader == null || iDataReader.IsClosed)
				return false;

			try
			{
                if (iDataReader.Read())
                {
                    ValorizeColumns();
                    return true;
                }
                else
				{
					ClearColumns();  //fix 19770
                    Close();
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
				//TODO far manifestare il messaggio
				//throw(new TBException(e.Message, e));
				Close();
			}
			return false;
		}

		//------------------------------------------------------------------------------
		public bool Execute ()
		{
			try
			{
				if (!Build())
					return false;

				Debug.WriteLine(string.Format("Query {0} Sql:\n{1}\nend query\n", queryNameString, sqlString));

				int nr = tbCommand.ExecuteNonQuery(); //It returns the number of rows affected.

				ValorizeColumns();
				return true;
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
				//TODO far manifestare il messaggio 
				//throw(new TBException(e.Message, e));
			}
			finally
			{
				Close();
			}
			return false;
		}

        //------------------------------------------------------------------------------
        public bool Call()
        {
            try
            {
                if (!Build(true))
                    return false;

                Debug.WriteLine(string.Format("Query {0} Sql:\n{1}\nend query\n", queryNameString, sqlString));
                
                //devo convertire la sintassi OLEDB in quella minimale di ADO.NET
                string sql = tbCommand.CommandText.ToUpper();

                int posEqual = sql.IndexOf('=');
                if (posEqual >= 0)
                {
                    sql = sql.Mid(posEqual + 1);

                    Debug.Assert(tbCommand.Parameters.Count > 0);
                    if (tbCommand.Parameters.Count > 0)
                    {
                        Debug.Assert(tbCommand.Parameters.GetParameterAt(0).Direction == ParameterDirection.Output);
                        tbCommand.Parameters.GetParameterAt(0).Direction = ParameterDirection.ReturnValue;
                    }
                }

                int posCall = sql.IndexOf("CALL");
                if (posCall >= 0)
                    sql = sql.Mid(posCall + 4 + 1);

                int posRoundOpen = sql.IndexOf('(');
                if (posRoundOpen >= 0)
                    sql = sql.Left(posRoundOpen - 1);

                sql = sql.Trim();

                Debug.WriteLine(string.Format("Query {0} normalize to call store procedure: {1}\n", queryNameString, sql));

                tbCommand.CommandText = sql;
                //----

                tbCommand.CommandType = CommandType.StoredProcedure;

           /*
                 System.Data.SqlClient.SqlCommandBuilder.DeriveParameters(tbCommand);
                 foreach (SqlParameter p in tbCommand.Parameters)
                {
                    Console.WriteLine(p.ParameterName);
                }
           */
                int nr = tbCommand.ExecuteNonQuery(); //It returns the number of rows affected.

                ValorizeColumns();
                return true;
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
                //TODO far manifestare il messaggio 
                //throw(new TBException(e.Message, e));
            }
            finally
            {
                Close();
            }
            return false;
        }
		
		//------------------------------------------------------------------------------
		public bool IsOpen () 
		{
			if (tbCommand == null || iDataReader == null)
				return false;
	
			return !iDataReader.IsClosed;
		}

		//------------------------------------------------------------------------------
		public bool Unparse(Unparser unparser, bool skipHeader = false)
		{
			if (!skipHeader)
			{
				unparser.WriteTag(Token.QUERY, false);
				unparser.WriteBlank();
				unparser.WriteID(queryNameString); 
			}

			unparser.WriteBegin();
			unparser.IncTab();
			unparser.WriteTag(Token.BRACEOPEN, false);

			string qt = queryTemplateString; //E' ottenuta da una AuditString è ha quindi perso l'indentazione iniziale.
			qt = qt.Replace("\r\n\t ", "");
			string sIndent = new string('\t', /*unparser.TabCounter + */ 1); 
			qt = qt.Replace("\r\n", "\r\n" + sIndent);
			qt = qt.Replace("\t", "", qt.Length - "\t".Length);
			unparser.Write(qt);
			
			unparser.WriteTag(Token.BRACECLOSE);

			unparser.DecTab();
			unparser.WriteEnd();
			unparser.WriteLine();
			return true;
		}
	}

	//==================================================================================
	public class TagLink : object
	{
		public string name			= string.Empty;
		public string sqlStringName	= string.Empty;

		public TagType	direction;
		public Expression	whenExpr		= null;
		public int			len			    = 0;
		public object		data			= null;
		public bool			isWhen			= false;
		public QueryObject	expandClause	= null;
        public QueryObject  elseClause      = null;
        public int          bindPos         = -1;

		public TagLink(string aName, TagType aDirection, object aData, int aLen, Expression aWhenExpr, QueryObject aExpandClause)
		{
			name			= aName;
			direction		= aDirection;
			len				= aLen;
			whenExpr		= aWhenExpr;
			expandClause	= aExpandClause;
			
			if (aData != null)
				data = aData;	//TODO pData->Clone();
		}
	}
}





