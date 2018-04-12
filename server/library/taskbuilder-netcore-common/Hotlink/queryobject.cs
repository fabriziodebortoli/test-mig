using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;

using TaskBuilderNetCore.Data;

using Microarea.Common.DBData;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.ExpressionManager;
using Microarea.Common.Lexan;
//using static Microarea.Common.Lexan.Parser;

namespace Microarea.Common.Hotlink
{
    public enum TagType { IN, OUT, REF, COL, EXPAND, INCLUDE, EVAL };

    public class ColumnType
    {
        public string columnName { get; set; }
        public string dataType { get; set; }
    }

    public class QueryObject : object, IDisposable
	{
		#region Data Member

		private string	queryNameString		= string.Empty ;
		private string	queryTemplateString	= string.Empty ; //originale, completa dei tag
		private string	sqlString			= string.Empty ; //senza i tag, con i segnaposto dei parametri
		private string	errorInfoString		= string.Empty ;
		
		private long	queryHandle		= -1;

		private SymbolTable	symbolTable	= null;

		//private bool			disposed		= false;//Track whether Dispose has been called.
		private DBConnection	tbConnection	= null; //m_pSqlSession
		private DBCommand       tbCommand		= null; //m_pSqlTable
		private IDataReader		iDataReader		= null; 

		private List<TagLink>   tagLinkArrayList	= null;
		private TbSession	    session		= null;
		private QueryObject		parentQuery	= null;
		private int             bindNumber = 0;

        public bool             ValorizeAll = true;
        public  bool            IsQueryRule = false;

        private List<string>    selectFields = null;
        public List<string>     AllColumns = null;

        private bool            isOracle = false;
        private bool            isUnicode = false;
        private DBMSType        dbType = DBMSType.SQLSERVER;

        private int             pageNumber = 0;
        private int             rowsForPage = 0;

        private string customWhere = string.Empty;
        private string customSort = string.Empty;

        #endregion

        #region Property

        protected IDataReader DataReader { get { return parentQuery != null ? parentQuery.DataReader : iDataReader; }}

		public string	QueryTemplate	{ get { return queryTemplateString; }	set { queryTemplateString	= value; }}
		public string	Name			{ get { return queryNameString; }		set { queryNameString		= value; }}
		public long		Handle			{ get { return queryHandle; } }
		public string	SqlString		{ get { return sqlString; }	}

		public DBConnection TbConnection { get { return tbConnection; }			set { tbConnection = value; } }

        UserInfo UserInfo { get { return (this.session != null && this.session.UserInfo != null) ? this.session.UserInfo : null; } }
        //ILoginManager LoginManager { get { return (this.session != null && this.session.UserInfo != null && this.session.UserInfo.LoginManager != null) ? this.session.UserInfo.LoginManager : null; } }

        #endregion

        #region Costructor + IDisposable

        public QueryObject(string name, SymbolTable aSymbolTable, TbSession aSession, QueryObject parent)
		{
			queryNameString		= name;
			symbolTable			= aSymbolTable;
			session				= aSession;
			tagLinkArrayList	= new List<TagLink>();
			parentQuery			= parent;
		}

		//------------------------------------------------------------------------------
		~QueryObject()
		{
			Dispose();
		}

		//------------------------------------------------------------------------------
		public void Dispose()
		{
            Clear();
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
        public void SetPaging(int page, int rows)
        {
            pageNumber = page;
            rowsForPage = rows;
        }

        public void SetCustomWhere(string where)
        {
            customWhere = where;
        }
        public void SetCustomSort(string sort)
        {
            customSort = sort;
        }

        //-------------------------------------------------------------------------------
        public int AddLink (string name, TagType direction, object data, int len, Expression whenExpr, QueryObject expandClause)
		{
            if (direction == TagType.COL)
            {
                AllColumns.Add(name);
            }

			tagLinkArrayList.Add(new TagLink(name, direction, data, len, whenExpr, expandClause));
            return tagLinkArrayList.Count - 1;
        }

		//------------------------------------------------------------------------------
		public bool HasMember (string name) 
		{
			for (int i = 0; i < tagLinkArrayList.Count; i++)
			{
				TagLink tagLink = tagLinkArrayList[i];

				if ((tagLink.direction == TagType.IN || tagLink.direction == TagType.REF) && tagLink.name.CompareNoCase(name))
                    return true;

                if (tagLink.direction == TagType.EXPAND || tagLink.direction == TagType.INCLUDE || tagLink.direction == TagType.EVAL)
                {
                    if (tagLink.whenExpr != null && tagLink.whenExpr.HasMember(name))
                        return true;
                    if (tagLink.expandClause != null && tagLink.expandClause.HasMember(name))
                        return true;
                    if (tagLink.elseClause != null && tagLink.elseClause.HasMember(name))
                        return true;
               }
            }
			return false;
		}

        public bool HasColumn(string name)
        {
            for (int i = 0; i < tagLinkArrayList.Count; i++)
            {
                TagLink tagLink = tagLinkArrayList[i];

                if ((tagLink.direction == TagType.COL /*|| tagLink.direction == TagType.OUT|| tagLink.direction == TagType.REF*/) && tagLink.name.CompareNoCase(name))
                    return true;

                if (tagLink.direction == TagType.EXPAND || tagLink.direction == TagType.INCLUDE)
                {
                    if (tagLink.expandClause != null && tagLink.expandClause.HasColumn(name))
                        return true;
                    if (tagLink.elseClause != null && tagLink.elseClause.HasColumn(name))
                        return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------------------------
        public bool Define(string sql, List<ColumnType> columns = null)
        {
            sql = "QUERY _q" + " BEGIN { " + sql + " } END";
            Parser parser = new Parser(Parser.SourceType.FromString);
            if (!parser.Open(sql))
                return false;

            bool ok = Parse(ref parser);
            if (!ok)
                return false;

            if (columns != null && sql.IndexOf("{COL") == -1)
            {
                foreach (ColumnType ct in columns)
                {
                    Variable field = null;
                    if (!symbolTable.Contains(ct.columnName))
                    {
                        string t = ct.dataType;
                        ushort tag = 0;
                        int idx = t.IndexOf('[');
                        if (idx > -1)
                        {
                            int r = t.LastIndexOf(']');
                            string stag = t.Mid(idx, r - idx);

                            ushort.TryParse(stag, out tag);
                            
                            t = t.Left(idx);
                        }

                        field = new Variable(ct.columnName, 0, t, tag, null);
                        symbolTable.Add(field);
                    }
                    else field = symbolTable.Find(ct.columnName) as Variable;
                    Debug.Assert(field != null);
                    if (field == null)
                        return false;

                    object o = field.Data;
                    if (o == null)
                        return false;

                    AddLink(ct.columnName, TagType.COL, o, field.Len, null, null);
                }
            }
            return ok;
         }

        //------------------------------------------------------------------------------
        public bool IsEmpty()
        {
            return iDataReader == null;
        }

        //------------------------------------------------------------------------------
        public bool Parse (ref Parser parser)
		{
			tagLinkArrayList.Clear(); 
            AllColumns = new List<string>();
            selectFields = new List<string>();

            /*  TODO RSWEB - dbType/Oracle/Unicode
                        if (this.session.UserInfo.LoginManager != null)
                        {   //Il TbLocalizer NON ha il LoginManager
                            string provider = this.session.UserInfo.LoginManager.ProviderName;
                            dbType = TBDatabaseType.GetDBMSType(provider);
                            isOracle = dbType == DBMSType.ORACLE;
                            isUnicode = this.session.UserInfo.LoginManager.UseUnicode;
                        }
            */
            isOracle = (UserInfo.DatabaseType == TaskBuilderNetCore.Data.DBMSType.ORACLE);
            isUnicode = UserInfo.UseUnicode;
            //----

            queryTemplateString = string.Empty;
			sqlString			= string.Empty;
            customWhere         = string.Empty;
            customSort          = string.Empty;

            if (parser.Matched(Token.QUERY)) 
			{
				if (!parser.ParseID(out queryNameString)) 
					return false;
			}

			if (!parser.ParseTag(Token.BEGIN)) 
				return false;
			
			if (!parser.ParseTag(Token.BRACEOPEN)) 
				return false;

			string	sPrecAuditString	= string.Empty;
			bool	bPrecAuditingState	= parser.EnableAudit;
			
			if (bPrecAuditingState) 
				sPrecAuditString = parser.GetAuditString();
			else
				parser.EnableAudit = true;

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
				parser.EnableAudit = false;
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

            //costanti unicode
            queryTemplateString = queryTemplateString.Replace(" N '", " N'");
            sqlString = sqlString.Replace(" N '", " N'");

            //Debug.WriteLine(string.Format("\nQuery Name: {0}:\nQuery: {1}\nSql: {2}\n", queryNameString, queryTemplateString, sqlString));
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
			Variable field	= null;
  
			Token token = parser.LookAhead();
			switch (token)
			{
				case Token.COL:
				{
                    parser.SkipToken();
					if (parser.LookAhead() != Token.ID) 
						return false;

					if (!parser.ParseID(out name)) 
						return false;

                    if (!symbolTable.Contains(name))
                    {
                       string aType = "String";
                       ushort tag = 0;
                       string woormType = "";
                       if (parser.Matched(Token.TYPE))
                       {
                            string baseType = "";

                            if (!DataTypeParser.Parse(parser, this.session.Enums, out aType, out woormType, out tag, out baseType))
                                return false;
                        }
                        field = new Variable(name, 0, aType, 0, null);

                        if (tag > 0 && woormType == "Enum")
                        {
                            field.WoormType = woormType;    // + '[' + tag.ToString() + ']';
                            field.EnumTag = tag;
                        }

                        symbolTable.Add(field);
                    }

					field = symbolTable.Find(name) as Variable;
					Debug.Assert(field != null);
					if (field == null)
						return false;

                   if (parser.Matched(Token.TITLE))
                   {
                        if (!parser.ParseString(out field.Title))
                            return false;
                   }

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
				
					field = symbolTable.Find(name) as Variable;
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

                    if (parser.Matched(Token.AS))
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

                    string nat = DBInfo.GetNativeConvert (s, isUnicode, dbType);
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
                        string nat = DBInfo.GetNativeConvert(res, this.UserInfo.UseUnicode, this.UserInfo.DatabaseType);    
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

			if (parser.Matched(Token.INCLUDE))
			{
				if (parser.LookAhead() != Token.ID) 
					return false;

				if (!parser.ParseID(out name)) 
					return false;

				if (!symbolTable.Contains(name)) 
					return false;
			
				field = symbolTable.Find(name) as Variable;
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
            if (parser.Matched(Token.ELSE))
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

            selectFields = new List<string>();
            SetCurrentQueryColumns();

            strSql = strSql.StripBlankNearSquareBrackets();
            strSql = strSql.Replace('\r', ' ');
            strSql = strSql.Replace('\n', ' ');
            strSql = strSql.Trim();

			if (TbConnection == null)
			{
				TbConnection = new DBConnection(Provider.DBType.SQLSERVER, this.session.CompanyDbConnection /*, TBDatabaseType.GetDBMSType(this.session.Provider) TODO RSWEB*/);
				TbConnection.Open();
			}

            //------------------------------------------------------
            if (this.pageNumber != 0 && customSort.IsNullOrEmpty() && customWhere.IsNullOrEmpty())
            {
                strSql = BuildPaging(strSql, TbConnection);
            }
            else if (!customWhere.IsNullOrEmpty())
            {
                string sortClause = string.Empty;
                int posSort = strSql.LastIndexOfWord("ORDER BY");
                if (posSort > 0)
                {
                    sortClause = customSort.IsNullOrEmpty() ? ' ' + strSql.Mid(posSort) : " ORDER BY " + customSort;
                    strSql = strSql.Left(posSort);
                }
                else if (!customSort.IsNullOrEmpty())
                    sortClause = " ORDER BY " + customSort;

                strSql = "SELECT * FROM (" + strSql + ") AS innerQuery WHERE " + customWhere.ReplaceQualifier() + sortClause.ReplaceQualifier();

                if (this.pageNumber != 0)
                    strSql = BuildPaging(strSql, TbConnection);
            }
            else if (!customSort.IsNullOrEmpty())
            {
                int posSort = strSql.LastIndexOfWord("ORDER BY");
                if (posSort > 0)
                {
                     strSql = strSql.Left(posSort);
                }

                strSql = "SELECT * FROM (" + strSql + ") AS innerQuery ORDER BY " + customSort.ReplaceQualifier();

                if (this.pageNumber != 0)
                    strSql = BuildPaging(strSql, TbConnection);
            }

            //---------------------------------------------
            tbCommand = new DBCommand(strSql, TbConnection);
            tbCommand.CommandTimeout = 0;

            int bindPos = 0;
            if (!BindParameter(tbCommand, ref bindPos, isProcedure)) 
				return false;
			return true;
		}

        string BuildPaging(string query, DBConnection dbConn)
        {
            //query = query.Trim().Remove(0, "select".Length);
            //query = "SELECT ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS id," + query + $" OFFSET {(this.pageNumber - 1) * this.rowsForPage} ROWS FETCH NEXT {this.rowsForPage} ROWS ONLY";

            int pos = query.LastIndexOfWord("ORDER BY");
            if (pos == -1)
                query += " ORDER BY 1 ";

            query +=  $" OFFSET {(this.pageNumber - 1) * this.rowsForPage} ROWS FETCH NEXT {this.rowsForPage} ROWS ONLY";
            return query;
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
                        string nat = DBInfo.GetNativeConvert(res, isUnicode, dbType); 
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

					Variable field = symbolTable.Find(tagLink.name) as Variable;
					Debug.Assert(field != null);
					if (field == null) 
						return false;

					string inc =  field.Data as string;
					Debug.Assert(inc != null);
					if (inc == null) 
						return false;

					Parser parser = new Parser(Parser.SourceType.FromString);
					parser.EnableAudit = true;
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
		private bool BindParameter (DBCommand tbCommand, ref int bindPos, bool isProcedure = false)
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

                //TODO RSWEB add command parameter
                IDbDataParameter p = tbCommand.CreateParameter();
                p.ParameterName = tagLink.sqlStringName;
                p.Value = tagLink.data;
                int paramNumber = tbCommand.Parameters.Add(p);
                //int paramNumber = tbCommand.Parameters.Add(tagLink.sqlStringName, tagLink.data);

				IDataParameter tbParameter = tbCommand.Parameters[paramNumber];
				tbParameter.Direction	= directionType;

				if (tagLink.direction == TagType.OUT) 
					continue;

				Variable field = symbolTable.Find(tagLink.name) as Variable;
                tbParameter.Value = CoreTypes.ObjectHelper.CastToDBData(field.Data);
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

				//Debug.WriteLine(string.Format("Query {0} Sql:\n{1}\nend query\n", queryNameString, sqlString));
	
				iDataReader = tbCommand.ExecuteReader();

				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
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
			//Debug.WriteLine(string.Format("Clear columns\n", queryNameString));

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
	
				Variable field = symbolTable.Find(tagLink.name) as Variable;

                //if (this.ValorizeAll)
                //    field.ClearAllData();
                //else
                    field.ClearAllData();

                field.RuleDataFetched = true;
				field.ValidRuleData = true;
			}
			return true;
		}

		//------------------------------------------------------------------------------
		private bool ValorizeColumns ()
		{
            //Debug.WriteLine(string.Format("Query {0} Fetch row\n", queryNameString));

            //TODO int nDataLevel = 0; //REPORT_ENGINE;//symbolTable->GetDataLevel(); // RULE_ENGINE QUERY_ENGINE REPORT_ENGINE
            bool bStrip = true; // this.session != null && session.StripTrailingSpaces;

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
	
				Variable field = symbolTable.Find(tagLink.name) as Variable;
                if (field == null)
                {
                    Debug.WriteLine("Unknown column: " + tagLink.name);
                    continue;
                }
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

                object data = Common.CoreTypes.ObjectHelper.CastFromDBData(o, field.Data);

                //An. 17609
                if (data is string && bStrip)
                    data = (data as string).TrimEnd(' ');

                field.Data = data;

                if (this.IsQueryRule)
                {
                    field.Data = data;

                    field.RuleDataFetched = true;
                    field.ValidRuleData = true;
                }
                else if (this.ValorizeAll)
                    field.SetAllData(data, true);
                else 
                    field.Data = data;  //set Field/Variable current level data

            }
			return true;
		}

        //------------------------------------------------------------------------------
        public void EnumColumns(List<Variable> columns)
        {
            for (int i = 0; i < tagLinkArrayList.Count; i++)
            {
                TagLink tagLink = (TagLink)tagLinkArrayList[i];

                if (
                    tagLink.direction == TagType.EXPAND ||
                    tagLink.direction == TagType.INCLUDE
                    )
                {
                    if (tagLink.isWhen)
                        tagLink.expandClause.EnumColumns(columns);
                    else if (tagLink.elseClause != null)
                        tagLink.elseClause.EnumColumns(columns);
                    continue;
                }

                if (tagLink.direction != TagType.COL) 
                    continue;

                Variable field = symbolTable.Find(tagLink.name) as Variable;
                columns.Add(field);
            }
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
				Debug.WriteLine(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
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

				//Debug.WriteLine(string.Format("Query {0} Sql:\n{1}\nend query\n", queryNameString, sqlString));

				int nr = tbCommand.ExecuteNonQuery(); //It returns the number of rows affected.

				ValorizeColumns();
				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
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

                //Debug.WriteLine(string.Format("Query {0} Sql:\n{1}\nend query\n", queryNameString, sqlString));
                
                //devo convertire la sintassi OLEDB in quella minimale di ADO.NET
                string sql = tbCommand.CommandText.ToUpper();

                int posEqual = sql.IndexOf('=');
                if (posEqual >= 0)
                {
                    sql = sql.Mid(posEqual + 1);

                    Debug.Assert(tbCommand.Parameters.Count > 0);
                    if (tbCommand.Parameters.Count > 0)
                    {
                        Debug.Assert(tbCommand.Parameters[0].Direction == ParameterDirection.Output);
                        tbCommand.Parameters[0].Direction = ParameterDirection.ReturnValue;
                    }
                }

                int posCall = sql.IndexOf("CALL");
                if (posCall >= 0)
                    sql = sql.Mid(posCall + 4 + 1);

                int posRoundOpen = sql.IndexOf('(');
                if (posRoundOpen >= 0)
                    sql = sql.Left(posRoundOpen - 1);

                sql = sql.Trim();

                //Debug.WriteLine(string.Format("Query {0} normalize to call store procedure: {1}\n", queryNameString, sql));

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
                Debug.WriteLine(e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.StackTrace);
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

        //---------------------------------------------------------------------
        QueryObject GetRoot() { return parentQuery != null ? parentQuery.GetRoot() : this; }

        //---------------------------------------------------------------------
        void SetCurrentQueryColumns()
        {
            for (int i = 0; i < tagLinkArrayList.Count; i++)
            {
                TagLink tagLink = tagLinkArrayList[i];

                if (tagLink.direction == TagType.COL)
                {
                    GetRoot().selectFields.Add(tagLink.name);
                    continue;
                }

                if (tagLink.direction == TagType.EXPAND || tagLink.direction == TagType.INCLUDE)
                {
                    if (tagLink.expandClause != null && tagLink.isWhen)
                        tagLink.expandClause.SetCurrentQueryColumns();

                    if (tagLink.elseClause != null && !tagLink.isWhen)
                        tagLink.elseClause.SetCurrentQueryColumns();
                }
            }
         }
        public int GetColumnCount()        
        {   
            return selectFields != null ? selectFields.Count : 0; 
        }

        public string GetColumnName(int i)   
        {
            if (selectFields == null)
                return null;
            if (i >= selectFields.Count)
                return null;

            return selectFields[i];
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





