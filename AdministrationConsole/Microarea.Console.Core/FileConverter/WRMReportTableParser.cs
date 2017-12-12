using System;
using System.Collections;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	public class LinkLine
	{
		public int line = 0;
		public int pos = 0;

		public LinkLine(int l, int p) { line = l; pos = p; }
	}

	//================================================================================
	public class WRMReportTableParser: ObjectParser
	{
		private ArrayList tables;
		private ArrayList aliases;

		private string tableToSplitted;
		private bool  isTableSplitted;
		private ArrayList tableSplittedReferenced;
		private ArrayList tableSplitted;

		private Hashtable mapTableSplitted = new Hashtable();

		protected Hashtable mapId = new Hashtable();	//variabili del report

		//--------------------------------------------------------------------------------
		protected virtual void InsertInvalid()
		{
		}

		//---------------------------------------------------------------------------------------------------
		private bool ExistTable(string table)
		{
			if (tables != null)
				for (int i=0; i < tables.Count; i++)
					if (string.Compare(tables[i] as string, table, true) == 0) return true;
			return false;
		}

		//---------------------------------------------------------------------------------------------------
		private string TableFromAlias(string alias)
		{	
			if (aliases != null)
				for (int i=0; i < aliases.Count; i++)
					if (string.Compare(aliases[i] as string, alias, true) == 0) 
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
			isTableSplitted = false; 
			tableToSplitted = string.Empty;
			tableSplittedReferenced = new ArrayList();
			tableSplitted  = new ArrayList();
		}

		//---------------------------------------------------------------------------------------------------
		public WRMReportTableParser(string fileName) : base(fileName)
		{
		}
		
		//--------------------------------------------------------------------------------
		public WRMReportTableParser(string fileName, string destinationFileName) : base(fileName, destinationFileName)
		{
		}

		//---------------------------------------------------------------------------------------------------
		protected override void ProcessBuffer()
		{
		}		

		//---------------------------------------------------------------------------------------------------
		virtual protected void SkipRule()
		{
			do { parser.SkipToken();	}
			while (!parser.Parsed(Token.SEP));
		}

		//--------------------------------------------------------------------------------
		protected void ParseLink(bool isForm, bool isRadar)
		{
			string newName = string.Empty, name = string.Empty;

			parser.SkipToken() ;	// LinkForm/LinkReport
			if (parser.LookAhead() == Token.ALIAS)
			{
				ParseLinkAlias();
			}
			else 
			{
				if (!parser.ParseString(out name))	// Form Name or Report path
					throw new FileConverterException(FileConverterStrings.ReadingLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);

				newName = isForm ? GlobalContext.LinkFormTranslator.TranslateLink(name) : GlobalContext.LinkReportTranslator.TranslateLink(name);
				if (newName != name && newName != string.Empty)
				{
					string s = isForm ? 
						"\" On "
						:
						"\"\r\n/*ATTENZIONE verificare la correttezza dei campi di Link: nel report chiamato saranno valorizzati in automatico i campi il cui nome corrisponde a quello indicato dopo i criteri di filtraggio*/\r\nOn ";
					ReplaceWord ( 
						parser.CurrentPos + 1, 
						name + "\"", 
						newName + s, 
						true, 
						isForm ? DiagnosticType.Information : DiagnosticType.Warning);
				}
				else
				{
					string message = string.Format
						(
						"{0}\"/*ATTENZIONE sostituire il riferimento del Link con l'appropriato namespace*/\r\n{1}",
						name, 
							(
								isForm ? 
								" On " : 
								"\r\n/*ATTENZIONE verificare la correttezza dei campi di Link: nel report chiamato saranno valorizzati in automatico i campi il cui nome corrisponde a quello indicato dopo i criteri di filtraggio*/\r\nOn "
							)
						);
					ReplaceWord(parser.CurrentPos + 1, name + "\"", message, true, DiagnosticType.Warning);
				}
			}
			
			ParseLinkAlias();

			if (parser.Parsed(Token.WHEN))
			{
				ParseFilterClause();

				if (parser.LookAhead() == Token.AND || parser.LookAhead() == Token.OR)
				{
					parser.SkipToken();

					ParseFilterClause();
				}
			}

			if (parser.LookAhead(Token.BEGIN))
			{
				ParseItems(name, isForm, isRadar);
			}
			else
			{
				ReplaceWord(parser.CurrentPos, string.Empty, "Begin\r\n" );

				ParseItem(name, isForm, isRadar);

				if (
					string.Compare(parser.CurrentLexeme, "Select", true) != 0 
					&&
					string.Compare(parser.CurrentLexeme, "Report", true) != 0
					)
					//Alias di colonna
					ReplaceWord(parser.CurrentPos, parser.CurrentLexeme, parser.CurrentLexeme + "\r\nEnd\r\n"  , true );
				else
					//alias di tabella
					ReplaceWord(parser.CurrentPos, parser.CurrentLexeme, "\r\nEnd\r\n"  + parser.CurrentLexeme  , true );
			}
		}
		
		//---------------------------------------------------------------------------------------------------
		protected bool ParseItem(string formName, bool isForm, bool isRadar)
		{
			if (!isRadar) 
				ParseDataType();

			string column;
			if (parser.ParseID(out column))
			{
				if (isForm)
				{
					string tableName = GlobalContext.LinkFormTranslator.GetFormMasterTableName(formName);
					
					if (tableName != null && tableName != string.Empty)
					{
						string newCol = TranslateColumn(tableName, column, false);
						if (newCol != string.Empty)
						{
							if (newCol != column)
								ReplaceWord(parser.CurrentPos, column, newCol, true);
							else
							{
								if (!ExistColumn(tableName, column))
								{
//									string msg = string.Format(Strings.NoColumnIntoTable, column, tableName);
//									ReplaceWord(parser.CurrentPos, string.Empty, string.Format(Strings.WarningWithParam, msg), true);
									string msg = string.Format("La colonna {0} non risulta nella descrizione della tabella {1}", column, tableName);
									ReplaceWord(parser.CurrentPos, string.Empty, string.Format("/*ATTENZIONE {0}*/", msg), true);

									GlobalContext.LogManager.Message(msg, string.Empty, DiagnosticType.Warning, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine));
								}
							}
						}
						else
						{
//							string msg = string.Format(Strings.UnsupportedColumnIntoTable, column, tableName);
//							ReplaceWord(parser.CurrentPos, string.Empty, string.Format(Strings.WarningWithParam, msg), true);
							string msg = string.Format("La colonna {0} della tabella {1} non è più supportata", column, tableName);
							ReplaceWord(parser.CurrentPos, string.Empty, string.Format("/*ATTENZIONE {0}*/", msg), true);

							GlobalContext.LogManager.Message(msg, string.Empty, DiagnosticType.Warning, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine));
						}
					}
				}
				if (parser.LookAhead(Token.ALIAS))
					ParseLinkAlias();
				else
					ParseConstValue();
			}
			return true;
		}

		//---------------------------------------------------------------------------------------------------
		protected bool ParseItems(string formName, bool isForm, bool isRadar)
		{
			parser.ParseBegin();
			while (!parser.Parsed(Token.END))
			{
				if (!ParseItem(formName, isForm, isRadar))
					return false;
			}
			return true;
		}

		//---------------------------------------------------------------------------------------------------
		protected void ParseDataType()
		{
			if (parser.Parsed(Token.ENUM))
			{
				parser.ParseTag(Token.SQUAREOPEN);
				string s;
				parser.ParseString(out s);
				parser.ParseTag(Token.SQUARECLOSE);
			}
			else
				parser.SkipToken(); //simple DataType
		}
		//---------------------------------------------------------------------------------------------------
		protected void ParseConstValue()
		{
			string s;
			if (parser.Parsed(Token.BRACEOPEN))
			{
				if (parser.LookAhead() == Token.TEXTSTRING)
				{
					parser.ParseString(out s);
					parser.ParseTag(Token.COLON);
					parser.ParseString(out s);
					parser.ParseTag(Token.BRACECLOSE);
				}
				else
				{
					parser.SkipToken(); //{d"01/01/02"}
					parser.ParseString(out s);
					parser.ParseTag(Token.BRACECLOSE);
				}
			}
			else if (parser.LookAhead() == Token.TEXTSTRING)
				parser.ParseString(out s);
			else
			{
				parser.Parsed(Token.MINUS);
				parser.SkipToken(); //number
			}
		}
		//---------------------------------------------------------------------------------------------------
		protected void ParseFilterClause()
		{
			ParseDataType();

			ParseLinkAlias();
			
			parser.SkipToken(); //binary operator

			if (parser.LookAhead(Token.ALIAS))
			{
				ParseLinkAlias();
			}
			else
			{ 
				ParseConstValue(); 
			}
		}

		//--------------------------------------------------------------------------------
		protected void ParseLinkAlias()
		{	
			if (!parser.Parsed(Token.ALIAS)) 
				return;

			int dummy;
			if (!parser.ParseInt(out dummy))
				throw new FileConverterException(FileConverterStrings.ReadingLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			int p1 = parser.CurrentPos;

			if (parser.LookAhead() == Token.COMMA)
			{
				parser.ParseTag(Token.COMMA);
				int p2 = parser.CurrentPos;
//	1.2			string s = CurrentLine.Substring( p1 + GetCurrentLineOffset    , (p2-p1));
				string s = CurrentLine.Substring( p1 + GetCurrentLineOffset(p1), (p2-p1));
				ReplaceWord ( 
					parser.CurrentPos, 
					s, 
					" ", 
					false );


				ReplaceWord ( 
					parser.CurrentPos, 
					",", 
					" ", 
					true );

				if (!parser.ParseInt(out dummy))
					throw new FileConverterException(FileConverterStrings.ReadingLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			}
		}


		//---------------------------------------------------------------------------------------------------
		protected void ParseRules()
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
			if (!parser.ParseTag(Token.FROM)) return;

			ClearTables();

			ParseTables();
			
			bool isAzParam = string.Compare(tableToSplitted, "AzParam", true) == 0;

			if (!parser.ParseTag(Token.SELECT)) return;

			if (parser.Parsed(Token.NOT) && !parser.ParseTag(Token.NULL)) 
				return;
			else 
				parser.Parsed(Token.NULL);

			ParseColumns();
			
			if (parser.Parsed(Token.WHERE))
			{
				if (isAzParam)
				{
					//TODO: commento la where clause ... non so come ripartirla fa le query splittate, per AzParam è inutile
					ReplaceWord(parser.CurrentPos, 
						"Where", 
						"/*ATTENZIONE la clausola è inutile: la tabella è monoriga\r\n Where", 
						true);
					if (!parser.SkipToToken(Token.SEP, true, false)) return;
					ReplaceWord(parser.CurrentPos, ";", "*/;", true);
				}
				else
					ParseWhereClause();
			}

			if (!isAzParam)
			{
				if 	(parser.Parsed(Token.GROUP) && parser.ParseTag(Token.BY))
					ParseOrderOrGroupBy();
			
				if 	(parser.Parsed(Token.ORDER) && parser.ParseTag(Token.BY))
					ParseOrderOrGroupBy();
		
				parser.ParseSep();
			}

			if (isTableSplitted && tableSplittedReferenced.Count > 0)
			{
				string refT = (string)tableSplittedReferenced[0];
				for (int i=1; i < tableSplittedReferenced.Count; i++)
					refT += "," + (string)tableSplittedReferenced[i];

				if (!mapTableSplitted.ContainsKey(tableToSplitted.ToUpper()))
					mapTableSplitted.Add(tableToSplitted.ToUpper(), refT);

				if (!isAzParam)
						ReplaceWord
						(
							parser.CurrentPos, 
							";",  
							string.Format( ";\r\n/*ATTENZIONE le tabelle necessarie è/sono: {0} */", refT), 
							true
						);
			}
			ClearTables();
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseTables()
		{
			do
			{
				string dataTableName = string.Empty;
				string aliasTableName = string.Empty;

				if (!parser.ParseID(out dataTableName)) return;

				string newName = TranslateTable(dataTableName, false);
				if (newName != null && newName != string.Empty)
				{
					string allTables;
					if (TableTranslator.Splitted(dataTableName, out allTables))
					{
						string[] splitTables = allTables.Split(',');
						isTableSplitted = true;
						for(int i=0; i < splitTables.GetLength(0); i++)
							tableSplitted.Add(splitTables[i].ToUpper());

						string message = string.Format(FileConverterStrings.TableSplitted, dataTableName, allTables);
						GlobalContext.LogManager.Message
							(
							message.Replace("\r\n", " "),
							string.Empty, DiagnosticType.Warning, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
							);
						tableToSplitted = dataTableName; //la convertitrò alla seconda passata
						//						ReplaceWord(parser.CurrentPos + dataTableName.Length, string.Empty,  string.Format(FileConverterStrings.TableSplittedWarning, allTables), true);
						ReplaceWord(parser.CurrentPos + dataTableName.Length, 
							string.Empty,  
							string.Format("\r\n/*ATTENZIONE la tabella è stata sudivisa nelle {0} */", allTables),
							true);
					}
					else
						ReplaceWord(parser.CurrentPos + dataTableName.Length, dataTableName, newName);
				}
				else
				{
					bool isParGenMail = string.Compare(dataTableName, "ParGenMail", true) == 0;
					if (isParGenMail)
						ReplaceWord(
							parser.CurrentPos, 
							dataTableName, 
							"MN_Mail/*ATTENZIONE. è cambiata la chiave della tabella,\r\nla colonna 'ParGenMail.TipoDocumento' di tipo enumerativo è diventata la 'MN_Mail.Namespace' di tipo stringa,\r\nla colonna 'ParGenMail.PathDocumento' è stata rinominata in 'ParGenMail.Path'*/ ", 
							true, DiagnosticType.Error);
					else
						ReplaceWord(
							parser.CurrentPos, 
							dataTableName, 
							dataTableName + string.Format(" /*ATTENZIONE La tabella {0} non è più supportata*/", dataTableName),
							true, DiagnosticType.Error);

					InsertInvalid();
				}

				if (parser.Parsed(Token.ALIAS) && !parser.ParseID(out aliasTableName)) return;
				
				AddTable(dataTableName, aliasTableName);

			} while (parser.Parsed(Token.COMMA));
		}

		//---------------------------------------------------------------------------------------------------
		protected void TranslateSplittedTables()
		{
			string dataTableName;
			if (!parser.ParseID(out dataTableName)) return;

			string splitted = mapTableSplitted[dataTableName.ToUpper()] as string;
			if (splitted == null) return;

			string [] tables = splitted.Split(',');
			int tablesCount = tables.GetLength(0);
			if (tablesCount == 1)
			{
				ReplaceWord(parser.CurrentPos, dataTableName, splitted, true);
				return;
			}
			ReplaceWord(parser.CurrentPos, dataTableName, tables[0], true);

			if (!parser.ParseTag(Token.SELECT)) 
				return;
			if (parser.Parsed(Token.NOT) && !parser.ParseTag(Token.NULL))
				return;
			parser.Parsed(Token.NULL);

			string [] otherCols = new string[tablesCount];
			string physicalName = string.Empty;
			string publicName = string.Empty;

			do
			{
				if (!parser.ParseID(out physicalName))return;

				string [] sa = physicalName.Split('.');
				if (sa.GetLength(0) != 2) return;

				int t = 0;
				for (; t < tablesCount; t++)
					if (string.Compare(sa[0], tables[t], true) == 0)
						break;
				if (t == tablesCount) return;

				ReplaceWord(parser.CurrentPos, physicalName, string.Empty, true);
				
				if (!parser.ParseTag(Token.INTO)) return;
				ReplaceWord(parser.CurrentPos, "Into", string.Empty, true);

				if (!parser.ParseID(out publicName)) return;
				
				ReplaceWord(parser.CurrentPos, publicName, string.Empty, true);

				otherCols[t] = (otherCols[t] != null ? otherCols[t]+ "," : "") +
								"\r\n" + physicalName + " Into " + publicName;
				
				if (parser.Parsed(Token.COMMA))
					ReplaceWord(parser.CurrentPos, ",", string.Empty, true);
			}
			while (!parser.LookAhead(Token.SEP) && !parser.LookAhead(Token.WHERE));
			
			if (parser.Parsed(Token.WHERE)) 
			{
				//TODO: commento la where clause ... non so come ripartirla fa le query splittate, per AzParam è inutile ed è già stata commentata
				InsertInvalid();

				ReplaceWord(parser.CurrentPos, 
					"Where", 
					 "/*ATTENZIONE: occorre ripartire manualmente la clausola Where fra le rule suddivise \r\n Where", 
					true);
				if (!parser.SkipToToken(Token.SEP, true, false)) return;
				ReplaceWord(parser.CurrentPos, ";", "*/;", true, DiagnosticType.Error);
			}
			else 
				if (!parser.Parsed(Token.SEP)) return;
			
			string otherRule = otherCols[0] + ";\r\n";
			string formatRule = "\r\nFrom {0} Select {1};\r\n";
			for (int i = 1; i < tablesCount; i++)
			{
				otherRule += string.Format(formatRule, tables[i], otherCols[i]);
			}
			ReplaceWord(parser.CurrentPos, ";", otherRule, true);

		}

		//---------------------------------------------------------------------------------------------------
		private void ParseColumns()
		{
			string physicalName = string.Empty, publicName = string.Empty;
			bool commaParsed = false;

			do
			{
				if (!parser.ParseID(out physicalName)) return;

				bool remarked = false;
				commaParsed = false;

				DoTranslateColumn(physicalName, true, out remarked, out commaParsed, false);

				if (!remarked)
					if (!parser.ParseTag(Token.INTO) || !parser.ParseID(out publicName)) 
						return;
			}
			while (commaParsed || parser.Parsed(Token.COMMA));
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

				if	(!ParseVariableOrConst(ref openRoundBrackets))
					break;
			}
		}

		//-----------------------------------------------------------------------------
		protected void ParseNativeSelect(ref int openRoundBrackets)
		{
			int newOpenRoundBrackets = openRoundBrackets;
			ArrayList columns = new ArrayList();
			ArrayList tablesPos = new ArrayList();
			ArrayList aliases = new ArrayList();
			Token t;
			bool bExitWhile = false;
			do
			{
				t = parser.LookAhead();
				switch(t)
				{
					case Token.ROUNDOPEN:
					{
						parser.SkipToken();

						newOpenRoundBrackets++;
						
						if (parser.LookAhead() == Token.SELECT)
						{
							parser.SkipToken();

							int countT = tables.Count;
							for (int j=0; j < tablesPos.Count; j++)
							{
								string table = ((ParserPosition)tablesPos[j]).Name;
								tables.Add(table);
							}
							ParseNativeSelect(ref newOpenRoundBrackets);
							if (tables.Count > countT)
								tables.RemoveRange(countT, tables.Count - countT);
						}
						break;
					}
					case Token.ROUNDCLOSE:
					{
						parser.SkipToken();

						newOpenRoundBrackets--;

						if (newOpenRoundBrackets < openRoundBrackets)
						{
							bExitWhile = true;
							openRoundBrackets--;
						}
						break;
					}
					case Token.ID:
					{
						string column;
						parser.ParseID(out column); 
						columns.Add(new ParserPosition(parser.CurrentPos + column.Length, parser.CurrentLine, column));
						break;
					}

					case Token.FROM:
					{
						parser.SkipToken();
						do 
						{
							string table;
							parser.ParseID(out table); 
							tablesPos.Add(new ParserPosition(parser.CurrentPos + table.Length, parser.CurrentLine, table));

							if (parser.Parsed(Token.AS))
							{
								string alias;
								parser.ParseID(out alias);
								aliases.Add(alias);
							}
							else aliases.Add("");
							if (!parser.Parsed(Token.COMMA))
								break;
						}
						while (true);

						break;
					}
					case Token.JOIN:
					{
						parser.SkipToken();

						string table;
						parser.ParseID(out table); 
						tablesPos.Add(new ParserPosition(parser.CurrentPos + table.Length, parser.CurrentLine, table));

						if (parser.Parsed(Token.AS))
						{
							string alias;
							parser.ParseID(out alias);
							aliases.Add(alias.ToLower());
						}
						else aliases.Add("");

						break;
					}
//					case Token.GROUP:
//					case Token.ORDER:
//					{
//						parser.SkipToken();
//						if (!parser.ParseTag(Token.BY))
//							break;
//						ParseOrderOrGroupBy();
//						break;
//					}
					case Token.SEP:
					case Token.EOF:
					{
						bExitWhile = true;
						break;
					}
					default: 
					{
						parser.SkipToken();
						break;
					}
				}
			if (bExitWhile)
				break;
		} while(true);

		if (tablesPos.Count == 0 || columns.Count == 0) 
			return;

			int nC = 0;
			int nT = 0;
		for (int n = 0; n < tablesPos.Count + columns.Count; n++)
		{
			ParserPosition infoT;
			ParserPosition infoC;

			bool bTradTable;

			if (nT < tablesPos.Count && nC < columns.Count)
			{
				infoT = (ParserPosition) tablesPos[nT];
				infoC = (ParserPosition) columns[nC];

				bTradTable = infoT.Y <= infoC.Y ? infoT.X <= infoC.X : false;
			}
			else if (nT < tablesPos.Count)
			{
				bTradTable = true;
			}
			else
			{
				bTradTable = false;
			}

			if (bTradTable)
			{
				infoT = (ParserPosition) tablesPos[nT];
				string table = infoT.Name;
				ReplaceWord(infoT.Y, infoT.X, table, TranslateTable(table, true));
				nT++;
				continue;
			}

			infoC = (ParserPosition) columns[nC];
			nC++;
			string column = infoC.Name;
			if (column.IndexOf('.') == -1)
			{
				int j = 0;
				for (; j < tablesPos.Count; j++)
				{
					string table = ((ParserPosition)tablesPos[j]).Name;
					if (ExistColumn(table, column))
					{
						string newCol = TranslateColumn(table, column, false);	
						if (newCol != string.Empty)
						{
							if (mapId.Contains(column.ToLower()))
							{
								string msg = string.Format("\r\n/*ATTENZIONE l'identificatore {0} potrebbe essere sia un campo del report che la colonna non qualificata della tabella {1}\r\nIn questo punto è stato considerato come colonna non qualificata */\r\n", column, table);
								ReplaceWord(infoC.Y, infoC.X, column, msg + TranslateTable(table, true) + "." + newCol + "\r\n");
								GlobalContext.LogManager.Message(msg.Replace("\r\n", " "), DiagnosticType.Warning);
							}
							else
								ReplaceWord(infoC.Y, infoC.X, column, TranslateTable(table, true) + "." + newCol);
						}
						else
						{
							string msg = string.Format("La colonna {0} della tabella {1} non è più supportata", column, table);
							ReplaceWord(infoC.Y, infoC.X, string.Empty, string.Format("/*ATTENZIONE {0}*/\r\n", msg));
							
							GlobalContext.LogManager.Message(msg.Replace("\r\n", " "), DiagnosticType.Error);
							InsertInvalid();
						}
						break;
					}
				}
				if (j == tablesPos.Count)
				{
					for (j = this.tables.Count - 1; j >= 0; j--)
					{
						string table = (string) this.tables[j];

						if (ExistColumn(table, column))
						{
							string newCol = TranslateColumn(table, column, false);	
							if (newCol != string.Empty)
							{
								if (mapId.Contains(column.ToLower()))
								{
									newCol = TranslateTable(table, true) + "." + newCol;
									string msg = string.Format("\r\n/*ATTENZIONE l'identificatore {0} potrebbe essere sia un campo del report che la colonna non qualificata della tabella {1}\r\nIn questo punto è stato considerato come campo del report e non come la colonna {2} */\r\n", column, table, newCol);
									ReplaceWord(infoC.Y, infoC.X, column, msg + newCol + "\r\n");
									GlobalContext.LogManager.Message(msg.Replace("\r\n", " "), DiagnosticType.Warning);
								}
								else
									ReplaceWord(infoC.Y, infoC.X, column, TranslateTable(table, true) + "." + newCol);
							}
							else
							{
								string msg = string.Format("La colonna {0} della tabella {1} non è più supportata", column, table);
								ReplaceWord(infoC.Y, infoC.X, string.Empty, string.Format("/*ATTENZIONE {0}*/\r\n", msg));
								
								GlobalContext.LogManager.Message(msg.Replace("\r\n", " "), DiagnosticType.Error);
								InsertInvalid();
							}
							break;
						}
					}
				}
			}
			else
			{
				ReplaceWord(infoC.Y, infoC.X, column, TranslateQualifiedColumn(column));
			}
		}
	}
		//-----------------------------------------------------------------------------
		protected bool ParseVariableOrConst(ref int openRoundBrackets)
		{
			switch (parser.LookAhead())
			{
				case Token.SELECT:
				{
					parser.SkipToken();
					ParseNativeSelect(ref openRoundBrackets);
					break;
				}

				case Token.ID:
				{
					string columnName;
					
					if (!parser.ParseID(out columnName)) 
						return false;
					if (
						string.Compare("exists", columnName, true) == 0 ||
						string.Compare("union", columnName, true) == 0 ||
						string.Compare("inner", columnName, true) == 0
						)
						break;

					DoTranslateQualifiedColumn(columnName, false, true);
					break;
				}
			
			
				case Token.TEXTSTRING:
				{
					string	aString;
					if (!parser.ParseString(out aString)) return false;
					break;
				}

				default :
					parser.SkipToken();
					break;
			}
			
			return true;
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
			while (!parser.LookAhead(StopTokens))
			{
				if (parser.LookAhead() == Token.ID)
				{
					parser.ParseID(out databaseObject);
					
					if (databaseObject.IndexOf('.') != -1)
					{
						DoTranslateQualifiedColumn(databaseObject, false, false);
					}
					else if (
						!mapId.Contains(databaseObject.ToLower()) &&
						ExistColumn(tables[0] as string, databaseObject)
						)
					{
						bool remarked = false;
						bool commaParsed = false;
						DoTranslateColumn(databaseObject, true, out remarked, out commaParsed, false);
					}
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
			
			while (!parser.LookAhead(StopTokens)) parser.SkipToken(); //TODOPERASSO potrei evere dei nomi di colonna nella condizione?
		
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
				if (parser.LookAhead(Token.BYTE))
				{
					//order by 1, 4
					parser.SkipToken();
				}
				else if (parser.ParseID(out name)) 
				{
					bool remarked = false;
					bool commaParsed = false;
					DoTranslateColumn(name, true, out remarked, out commaParsed, false);
				}
				else 
					return;

				Token token = parser.LookAhead();					
				if (token == Token.DESCENDING || token == Token.ASC) 
					parser.SkipToken();					
			} 
			while (parser.Parsed(Token.COMMA));
		}

		//---------------------------------------------------------------------------------------------------
		private void DoTranslateColumn(string column, bool addTable, out bool remarked, out bool commaParsed, bool checkExistWoormField)
		{
			remarked = false; commaParsed = false;
			if (column.IndexOf('.') == -1)
			{
				if (tables.Count == 0) return;
				for (int i=0; i < tables.Count; i++)
				{
					string table = tables[i] as string;	
					if (ExistColumn(table, column))
					{
						string newCol = TranslateColumn(table, column, false);	
						if (newCol != string.Empty)
						{
							if (isTableSplitted)
							{
								//se la colonna non referenziata è stata trovata da tradurre ma la tabella si è splittata
								//tengo traccia di quale tabella figlia dello split è stata referenziata
								string splitTable = TableTranslator.OwnerColumn(table, column);
								if (!tableSplittedReferenced.Contains(splitTable.ToUpper()))
									tableSplittedReferenced.Add(splitTable.ToUpper());

								newCol = splitTable + "." + newCol;

								if (checkExistWoormField && mapId.Contains(column.ToLower()))
								{
									string msg = string.Format("\r\n/*ATTENZIONE l'identificatore {0} potrebbe essere sia un campo del report che la colonna non qualificata della tabella {1}\r\nIn questo punto è stato considerato come campo del report e non come la colonna {2} */\r\n", column, splitTable, newCol);
									ReplaceWord(parser.CurrentPos, column, msg + column + "\r\n", true);
									GlobalContext.LogManager.Message(msg.Replace("\r\n", " "), DiagnosticType.Warning);
								}
								else
									ReplaceWord(parser.CurrentPos, column, newCol, true);
							}
							else
							{
								if (addTable)
								{
									newCol = TranslateTable(table, true) + "." + newCol;

									if (checkExistWoormField && mapId.Contains(column.ToLower()))
									{
										string msg = string.Format("\r\n/*ATTENZIONE l'identificatore {0} potrebbe essere sia un campo del report che la colonna non qualificata della tabella {1}\r\nIn questo punto è stato considerato come campo del report e non come la colonna {2} */\r\n", column, table, newCol);
										ReplaceWord(parser.CurrentPos, column, msg + column + "\r\n", true);
										GlobalContext.LogManager.Message(msg.Replace("\r\n", " "), DiagnosticType.Warning);
									}
									else
										ReplaceWord(parser.CurrentPos, column, newCol, true);
								}
								else
									ReplaceWord(parser.CurrentPos, column, newCol, true);
							}
						}
						else
						{
							string msg = string.Format("La colonna {0} della tabella {1} non è più supportata", column, table);
							int cpos = parser.CurrentPos;
							if (parser.Parsed(Token.INTO))
							{
								ReplaceWord(cpos, column, string.Format("/*ATTENZIONE {0}\r\n", msg), true);
								string id;
								if(!parser.ParseID(out id))return;
								if (parser.Parsed(Token.COMMA))
								{
									commaParsed = true;
									ReplaceWord(parser.CurrentPos, ",", string.Format(",*/", msg), true);
								}
								else
									ReplaceWord(parser.CurrentPos, string.Empty, string.Format("*/", msg), true);
								remarked = true;
							}
							else
								ReplaceWord(cpos, column, string.Format("/*ATTENZIONE {0}*/\r\n", msg), true);

							GlobalContext.LogManager.Message(msg.Replace("\r\n", " "), string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine));
							InsertInvalid();
						}
					}
					else
					{
						GlobalContext.LogManager.Message(String.Format("Incontrata colonna custom {0}.{1}", table, column), string.Empty, DiagnosticType.Warning, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine));
					}
				}
			}
			else
			{
				DoTranslateQualifiedColumn(column, false, false);
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void DoTranslateQualifiedColumn(string name, bool checkAlias, bool checkExistWoormField)
		{
			string[] tokens = name.Split('.');
			if (tokens.Length != 2) 
			{
				bool remarked = false;
				bool commaParsed = false;
				DoTranslateColumn(name, true, out remarked, out commaParsed, checkExistWoormField);
				return;
			}
			string table = tokens[0];	
			string column = tokens[1];
			if (!ExistTable(table))	return;

			string newTable = TranslateTable(table, true);
			if (newTable != null && newTable != string.Empty)
				ReplaceWord(parser.CurrentPos, table, newTable, true); 

			string newCol = TranslateColumn(table, column, false);
			if (newCol != string.Empty)
			{
				if (isTableSplitted)
				{
					//se la colonna non referenziata è stata trovata da tradurre ma la tabella si è splittata
					//tengo traccia di quale tabella figlia dello split è stata referenziata
					string splitTable = TableTranslator.OwnerColumn(table, column);
					if (!tableSplittedReferenced.Contains(splitTable.ToUpper()))
						tableSplittedReferenced.Add(splitTable.ToUpper());
				}
				ReplaceWord(parser.CurrentPos + name.Length, column, newCol);
			}
			else
			{
				//string msg = string.Format(FileConverterStrings.UnsupportedColumnIntoTable, column, table);
				//ReplaceWord(parser.CurrentPos, string.Empty, string.Format(FileConverterStrings.WarningWithParam, msg));
				string msg = string.Format("La colonna {0} della tabella {1} non è più supportata", column, table);
				ReplaceWord(parser.CurrentPos, string.Empty, string.Format("/*ATTENZIONE {0}*/\r\n", msg));

				GlobalContext.LogManager.Message(msg.Replace("\r\n", " "), string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine));
				InsertInvalid();
			}
		}
	}
}
