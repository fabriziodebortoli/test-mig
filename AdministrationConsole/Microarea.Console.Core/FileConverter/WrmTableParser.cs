using System;
using System.Collections;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	//================================================================================
	public class WRMTableParser: ObjectParser
	{
		private ArrayList tables;
		private ArrayList aliases;

		//---------------------------------------------------------------------------------------------------
		private bool ExistTable(string table)
		{
			return tables != null && tables.Contains(table);
		}

		//---------------------------------------------------------------------------------------------------
		private string TableFromAlias(string alias)
		{	
			if (aliases == null) return null;
		
			if (aliases.Contains(alias))
				return tables[aliases.IndexOf(alias)] as string;
			
			return null;
		}

		//---------------------------------------------------------------------------------------------------
		private void AddTable(string table, string alias)
		{
			if (tables == null || aliases == null) return;

			tables.Add(table);
			aliases.Add(alias);
		}

		//---------------------------------------------------------------------------------------------------
		private void ClearTables()
		{
			tables = new ArrayList();
			aliases = new ArrayList();
		}

		//---------------------------------------------------------------------------------------------------
		public WRMTableParser(string fileName) : base(fileName)
		{
			this.checkLogErrorsToValidateParsing = false;
		}
		
		//--------------------------------------------------------------------------------
		public WRMTableParser(string fileName, string destinationFileName) : base(fileName, destinationFileName)
		{
			this.checkLogErrorsToValidateParsing = false;
		}

		//---------------------------------------------------------------------------------------------------
		protected override void ProcessBuffer()
		{
			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				switch (t)
				{
					case Token.RULES :
					{
						ParseRules();
						break;
					}
			
					case Token.LINKFORM   :
					{
						ParseLinkForm();
						break;
					}

					default			:
					{
						ParseToken(t);
						break;
					}
				}
			}
			
		}		

		//--------------------------------------------------------------------------------
		protected virtual void ParseToken(Token t)
		{
			parser.SkipToken();
		}

		//---------------------------------------------------------------------------------------------------
		private void SkipRule()
		{
			do { parser.SkipToken();	}
			while (!parser.Parsed(Token.SEP));
		}

		//-----------------------------------------------------------------------------
		private void ParseAlias()
		{
			int alias = 0;

			if (!parser.ParseTag(Token.ALIAS) || !parser.ParseInt(out alias))
				ThrowException(FileConverterStrings.ReadingAlias);
		}

		//---------------------------------------------------------------------------------------------------
		protected virtual void ParseLinkForm()
		{
			parser.SkipToken() ;	// LinkForm
						
			string nameSpace = string.Empty;
			if (parser.LookAhead() == Token.ALIAS)
				ParseAlias();
			else if (!parser.ParseString(out nameSpace))
				ThrowException(FileConverterStrings.ReadingLinkNamespace);
			//if (!parser.SkipToToken(Token.BEGIN, true)) ThrowException(FileConverterStrings.EndOfFileLinkReading);	
			while (!parser.Parsed(Token.BEGIN))
			{
				if (parser.Eof)
					ThrowException(FileConverterStrings.EndOfFileLinkReading);
				parser.SkipToken();
			}

			string masterTable = GetDbtMasterTableName("Document." + LookUpDocumentNamespace(ref nameSpace));
			if (masterTable == null || masterTable.Length == 0)
				ThrowException(FileConverterStrings.CannotFindMasterTable);

			string oldMasterTable = TableTranslator.ReverseTranslateTable(masterTable);
			if (oldMasterTable == null || oldMasterTable.Length == 0)
				ThrowException(FileConverterStrings.CannotFindMasterTable);
			
			while (!parser.Parsed(Token.END))
			{
				if (parser.Eof)
					ThrowException(FileConverterStrings.EndOfFileLinkReading);
				ParseConnection(oldMasterTable);
			}				
		}

		//---------------------------------------------------------------------------------------------------
		override public string LookUpDocumentNamespace(ref string ns)
		{	//override in ReportMigrationNet per lookup sui namespace inglesi (serve anche il ref)
			if (string.Compare("document.", 0 , ns, 0, 9, true) == 0)
				ns = ns.Substring(9);
			return ns;
		}

		//---------------------------------------------------------------------------------------------------
		protected void ParseConnection(string tableName)
		{
			string type;
			if (!ParseDataType(out type))
				ThrowException(FileConverterStrings.ReadingLinkDataType);
			
			string column;
			if (!parser.ParseID(out column))
				ThrowException(FileConverterStrings.ReadingLinkFieldName);
			
			string newName = TranslateColumn(tableName, column, false);
			if (newName == string.Empty)
			{
				GlobalContext.LogManager.Message(string.Format("La colonna {0} della tabella {1} non e' piu' supportata", tableName, column), DiagnosticType.Error);
			}
			else
				ReplaceWord(parser.CurrentPos, column, newName, true);

			if (parser.LookAhead(Token.ALIAS))
			{
				ParseAlias();
			}
			else
			{
				if (!ParseConstValue (type))
					ThrowException(FileConverterStrings.ReadingConstantLink);
			}
		}

		//------------------------------------------------------------------------------
		bool ParseConstValue (string type)
		{
			switch (type)
			{
				case "Int32":
				{
					int n = 0;
					if (!parser.ParseSignedInt(out n))
						return false;

					break;
				}
				case "Int64":
				{
					long l = 0;
					if (!parser.ParseSignedLong(out l))
						return false;

					break;
				}
				case "Boolean":
				{
					bool b = false;
					if (!parser.ParseBool(out b))
						return false;

					break;
				}
				case "String":
				{
					string s;
					if (!parser.ParseString(out s))
						return false;

					s = s.Replace("%", ""); // elimina il % di un eventuale like
					break;
				}
				case "Single":
				case "Double":
				{
					double f = 0;
					if (!parser.ParseSignedDouble(out f))
						return false;

					break;
				}
				case "DateTime":
				case "DataEnum":
				{   
					if (!ParseComplexData())
						return false;

					break;
				} 
				default: 
					return false;
			}
		
			return true;
		}

		//--------------------------------------------------------------------------------
		private bool ParseComplexData()
		{
			bool ok = true;
			if (!parser.ParseTag(Token.BRACEOPEN))
				return false;

			switch (parser.ComplexData())
			{
				case Parser.ComplexDataType.DataEnum:
				{
					string sTag;
					string sItem;
					int tag;
					int item;
					if (parser.LookAhead(Token.TEXTSTRING))
						ok = parser.ParseDataEnum(out sTag, out sItem);
					else
						ok = parser.ParseDataEnum(out tag, out item);
					break;
				}

				case Parser.ComplexDataType.Time:
				case Parser.ComplexDataType.Date:
				case Parser.ComplexDataType.DateTime:
				{
					DateTime dt; 
					ok = parser.ParseDateTime(out dt);
					break;
				}

					// l'ElapsedTime in TB C++ è rappresentato da un long
				case Parser.ComplexDataType.TimeSpan:
				{
					TimeSpan ts; 
					ok = parser.ParseTimeSpan(out ts);
					long millisecs = (long) ts.TotalMilliseconds;
					break;
				}

				default : 
					return false;
			}

			return ok && parser.ParseTag(Token.BRACECLOSE);
		}


		//---------------------------------------------------------------------------
		private bool ParseDataType(out string type)
		{
			type = "";
			Token tokenType = parser.LookAhead();
			try
			{
				type = ObjectHelper.FromTBType(Language.GetTokenString(tokenType));
			}
			catch (ObjectHelperException)
			{
				return false;
			}
			parser.SkipToken();
			
			if (type != "DataEnum")
				return true;

			// sintassi per dichiarare  gli enumerativi: ENUM["TAGNAME"] NomeVariabile;
			// alternativa dalla release 7: ENUM [ nn ] NomeVariabile;
			if (!parser.ParseTag(Token.SQUAREOPEN))
				return false;

			string tagName;
			if (parser.LookAhead(Token.TEXTSTRING))
			{
				if (!parser.ParseString(out tagName))
					return false;
			}
			else
			{
				int enumTagValue;
				if (!parser.ParseInt(out enumTagValue))
					return false;
			}
			return parser.ParseTag(Token.SQUARECLOSE);
		}

		//---------------------------------------------------------------------------------------------------
		protected virtual void ParseRules()
		{
			while (!parser.Eof && !parser.Parsed(Token.END))
			{
				switch (parser.LookAhead())
				{
					case Token.FROM :
					{
						ParseTableRule();
						break;
					}
			
					case Token.IF   :
					{
						SkipRule();
						break;
					}
			
					case Token.ID   :
					{
						SkipRule();
						break;
					}

					default:
					{
						parser.SkipToken();
						break;
					}
				}
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseTableRule()
		{
			if (!parser.ParseTag(Token.FROM)) 
				ThrowLastParserErrorException();

			ClearTables();
			ParseTables();
			
			if (!parser.ParseTag(Token.SELECT)) 
				ThrowLastParserErrorException();

			if (
				(parser.Parsed(Token.NOT) || parser.Parsed(Token.IS) )
				&& !parser.ParseTag(Token.NULL)
				) 
				ThrowLastParserErrorException();
			
			parser.Parsed(Token.NULL);

			ParseColumns();
			
			/*	Nuova grammatica:
				[WHERE [NATIVE] expr]
				[GROUP BY native-groupby]
				[ORDER BY native-orderby]
			*/

			if (parser.Parsed(Token.WHERE))
				ParseWhereClause();

			if 	(parser.Parsed(Token.GROUP) && parser.ParseTag(Token.BY))
				ParseOrderOrGroupBy();
			
			if 	(parser.Parsed(Token.ORDER) && parser.ParseTag(Token.BY))
				ParseOrderOrGroupBy();
		
			if (!parser.ParseSep()) 
				ThrowLastParserErrorException();

			ClearTables();
			
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseTables()
		{
			do
			{
				string dataTableName = string.Empty;
				string aliasTableName = string.Empty;

				if (!parser.ParseID(out dataTableName))
					ThrowException(FileConverterStrings.CannotReadTableName);

				string newName = TranslateTable(dataTableName, false);
				if (newName == string.Empty)
				{
					GlobalContext.LogManager.Message(string.Format("La tabella {0} non e' piu' supportata", dataTableName), DiagnosticType.Error);
				}
				else
					ReplaceWord(parser.CurrentPos + dataTableName.Length, dataTableName, newName);

				if (parser.Parsed(Token.ALIAS) && !parser.ParseID(out aliasTableName)) 
					ThrowLastParserErrorException();
				
				AddTable(dataTableName, aliasTableName);

			} while (parser.Parsed(Token.COMMA));
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseColumns()
		{
			string physicalName = string.Empty, publicName = string.Empty;

			do
			{
				if (!parser.ParseID(out physicalName))
					ThrowException(string.Format(FileConverterStrings.ErrorReadColumnNameWithParam, parser.CurrentLexeme));
				
				TranslateColumn(physicalName);

				if (!parser.ParseTag(Token.INTO) || !parser.ParseID(out publicName))
					ThrowLastParserErrorException();
			}
			while (parser.Parsed(Token.COMMA));
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseWhereClause()
		{
			if (parser.Parsed(Token.NATIVE))
			{
				ParseNativeWhereClause();
				return;
			}
				
			if (!parser.LookAhead(Token.IF))
			{
				ParseStandardWhereClause();
				return;
			}

			ParseCondWhere();
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseNativeWhereClause()
		{
			int openRoundBrackets = 0;
					
			while (true)
			{
				Token tk = parser.LookAhead();
				if	(tk == Token.EOF || tk == Token.SEP || tk == Token.ELSE)
					break;

				if	(tk == Token.ROUNDOPEN)
				{
					openRoundBrackets++;
					parser.SkipToken();
					continue;
				}
				else if	(tk == Token.ROUNDCLOSE)	
				{
					openRoundBrackets--;
					parser.SkipToken();
					if	(openRoundBrackets == 0 && (parser.LookAhead() == Token.ORDER || parser.LookAhead() == Token.GROUP))
						break;
					continue;
				}

				if	(openRoundBrackets == 0 && (tk == Token.ORDER || tk == Token.GROUP))
					break;

				ParseVariableOrConst();
			}
		}

		//IPOTIZZO CHE LA SELECT SIA TERMINATA DA UNA WHERE O UNA )
		//-----------------------------------------------------------------------------
		protected void ParseNativeSelect()
		{
			int nRounds = 1;
			ArrayList columns = new ArrayList(), tables = new ArrayList();
			Token t;
			do
			{
				t = parser.LookAhead();
				if (t == Token.ID)
				{
					string column;
					parser.ParseID(out column); 
					columns.Add(new ParserPosition(parser.CurrentPos + column.Length, parser.CurrentLine, column));
				}
				else
				{
					parser.SkipToken();
				}
			}
			while (!parser.Parsed(Token.FROM));

			do
			{
				t = parser.LookAhead();
				if (t == Token.ID)
				{
					string table;
					parser.ParseID(out table); 
					tables.Add(new ParserPosition(parser.CurrentPos + table.Length, parser.CurrentLine, table));
				}
				else 
				{
					if (t == Token.SEP) 
						break;
					if (t == Token.ROUNDOPEN) 
						nRounds++;
					if (t == Token.ROUNDCLOSE && --nRounds == 0) 
						break;
					parser.SkipToken();
				}
			}
			while (!parser.Parsed(Token.WHERE) || nRounds > 1);

			if (tables.Count == 0 ) /*|| columns.Count == 0 //Exists (Select * from ...) */
				ThrowException(FileConverterStrings.NativeSelectStatementException);
			
			ParserPosition info;
			
			for (int i = 0; i < columns.Count; i++)
			{
				info = (ParserPosition) columns[i];
				string column = info.Name;
				if (column.IndexOf('.') == -1)
				{
					string table = ((ParserPosition)tables[0]).Name;
					ReplaceWord(info.Y, info.X, column, TranslateTable(table, true) + "." + TranslateColumn(table, column, true));
				}
				else
				{
					ReplaceWord(info.Y, info.X, column, TranslateQualifiedColumn(column));
				}
			}

			for (int i = 0; i < tables.Count; i++)
			{
				info = (ParserPosition) tables[i];
				string table = info.Name;
				ReplaceWord(info.Y, info.X, table, TranslateTable(table, true));
			}
		}

		//-----------------------------------------------------------------------------
		protected void ParseVariableOrConst()
		{
			switch (parser.LookAhead())
			{
				case Token.SELECT:
				{
					parser.SkipToken();
					ParseNativeSelect();
					break;
				}

				case Token.ID:
				{
					string columnName;
					
					if (!parser.ParseID(out columnName))
						ThrowException(string.Format(FileConverterStrings.ErrorReadColumnNameWithParam, parser.CurrentLexeme));
				
					if (columnName.IndexOf(".") != -1)
						TranslateQualifiedColumn(columnName, false);
					break;
				}
			
			
				case Token.TEXTSTRING:
				{
					string	aString;
					if (!parser.ParseString(out aString)) 
						ThrowLastParserErrorException();
					break;
				}

				default :
					parser.SkipToken();
					break;
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseStandardWhereClause()
		{
			if (parser.Parsed(Token.BREAK)) return;
			
			if (parser.Parsed(Token.ALL)) return;

			if (parser.Parsed(Token.NATIVE) )
			{
				ParseNativeWhereClause();
				return;
			}
			
			Token[] StopTokens = new Token[] 
			{ 
				Token.ORDER, 
				Token.ELSE, 
				Token.GROUP, 
				Token.WHERE, //WHERE?
				Token.EOF,	//proprio di ogni expression
				Token.SEP 	//proprio di ogni expression
			};
				
			string databaseObject;
			while (!HasToStop(StopTokens))
			{
				if (parser.LookAhead() == Token.ID)
				{
					parser.ParseID(out databaseObject);
					
					// tutti i nomi devono essere qualificati
					if (databaseObject.IndexOf('.') != -1)
						TranslateQualifiedColumn(databaseObject, false);
				}
				else 
					parser.SkipToken();
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseCondWhere()
		{
			if (!parser.ParseTag(Token.IF)) return;
			
			Token[] StopTokens = new Token[] 
			{ 
				Token.THEN, 
				Token.EOF,	//proprio di ogni expression
				Token.SEP 	//proprio di ogni expression
			};
			
			while (!HasToStop(StopTokens)) parser.SkipToken(); //TODOPERASSO potrei evere dei nomi di colonna nella condizione?
		
			if (!parser.ParseTag(Token.THEN)) return;
			
			if (parser.LookAhead(Token.IF))
			{	
				ParseCondWhere();
			}
			else
			{
				ParseWhereClause();
			}
			
			if (!parser.Parsed(Token.ELSE)) return;
			
			if (parser.LookAhead(Token.IF))
			{
				ParseCondWhere();
				return;
			}

			ParseWhereClause();
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseOrderOrGroupBy()
		{
			string name = string.Empty;
			do
			{
				if (!parser.ParseID(out name)) return;

				TranslateColumn(name);

				Token token = parser.LookAhead();					
				if (token == Token.DESCENDING || token == Token.ASC) parser.SkipToken();					
					
			} 
			while (parser.Parsed(Token.COMMA));

		}

		//---------------------------------------------------------------------------------------------------
		private void TranslateColumn(string name)
		{
			if (name.IndexOf('.') == -1)
			{
				if (tables.Count == 0) return;
				ReplaceWord(parser.CurrentPos, name, TranslateColumn(tables[0] as string, name, true), true);
			}
			else
			{
				TranslateQualifiedColumn(name, false);
			}
		}

		//---------------------------------------------------------------------------------------------------
		private new void TranslateQualifiedColumn(string name, bool checkAlias)
		{
			string[] tokens = name.Split('.');
			if (tokens.Length != 2) ThrowException(string.Format(FileConverterStrings.TranslateQualifiedColumnException, name));
					
			string table = tokens[0];					
			if (!checkAlias || ExistTable(table)) //se esiste, è il nome di una tabella
			{
				ReplaceWord(parser.CurrentPos, table, TranslateTable(table, true), true); 
			}
			else // altrimenti è un alias (non devo tradurre)
			{
				table = TableFromAlias(table);
			}
				
			if (table == null) return;

			string column = tokens[1];
			ReplaceWord(parser.CurrentPos + name.Length, column, TranslateColumn(table, column, true));
		}
	}
}
