using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	//================================================================================
	public class ConstraintInfo
	{
		public enum ConstraintType { NONE, PRIMARY_KEY, FROEIGN_KEY }

		public ConstraintType	Type;
		public string			Table;
		public string			ReferenceTable; //only for FROEIGN_KEY types
	}

	//================================================================================
	public class SQLTableParser: ObjectParser
	{
		private string		currentTable;
		private string		firstObjectName;
		private SQLToken	firstOperationToken = 0;
		private Hashtable	constraintList;
		private Hashtable	indexList;
		
		private static ArrayList	sqlConstraintNames = new ArrayList();
		private static ArrayList	oracleConstraintNames = new ArrayList();

		protected bool		oracle = false;
		private bool		backupFile = true;
		private bool		changeFileName = false;
		private string		lastSystemTableName = null;
		private bool		startOfLine = false;

		//--------------------------------------------------------------------------------
		protected bool Oracle { get { return oracle; }}
		//--------------------------------------------------------------------------------
		public bool BackupFile { set { backupFile = value; } get { return backupFile; }}
		//--------------------------------------------------------------------------------
		public bool ChangeFileName { set { changeFileName = value; } get { return changeFileName; }}
		//--------------------------------------------------------------------------------
		protected string FirstObjectName 
		{
			set
			{
				if (firstObjectName == null) 
				{
					firstObjectName = value;
					if (ChangeFileName)
						destinationFileName = DynamicDestinationFileName;
				}
			}
			get
			{
				return firstObjectName;
			}
		}

		//--------------------------------------------------------------------------------
		protected SQLToken FirstOperationToken 
		{
			set
			{
				if (firstOperationToken == 0) 
				{
					firstOperationToken = value;
					if (ChangeFileName)
						destinationFileName = DynamicDestinationFileName;
				}
			}
			get
			{
				return firstOperationToken;
			}
		}

		//--------------------------------------------------------------------------------
		private string DynamicDestinationFileName
		{
			get
			{
				if (FirstObjectName == null || FirstOperationToken == 0)
					return fileName;
				
				if (FirstOperationToken == SQLToken.ALTER)
					return Path.Combine(Path.GetDirectoryName(fileName), "Alter_" + firstObjectName + ".sql");

				if (FirstOperationToken == SQLToken.CREATE || FirstOperationToken == SQLToken.DROP)
					return Path.Combine(Path.GetDirectoryName(fileName), firstObjectName + ".sql");

				return fileName;
			}
		}

		protected enum SQLToken 
		{
			TABLE		= Token.USR00,
			CREATE		= Token.USR01,
			DBO			= Token.USR02,
			CONSTRAINT	= Token.USR03,
			PRIMARY		= Token.USR04,
			FOREIGN		= Token.USR05,
			KEY			= Token.USR06,
			REFERENCES	= Token.USR07,
			OBJECT_ID	= Token.USR08,
			ALTER		= Token.USR09,
			GO			= Token.USR10,
			ADD			= Token.USR11,
			END			= Token.USR12,
			DELETE		= Token.USR13,
			FROM		= Token.USR14,
			INSERT		= Token.USR15,
			INTO		= Token.USR16,
			DOT			= Token.USR17,
			INDEX		= Token.USR18,
			UPDATE		= Token.USR19,
			ON			= Token.USR20,
			DROP		= Token.USR21,
			EXEC		= Token.USR22,
			VIEW		= Token.USR23,
			OR			= Token.USR24,
			REPLACE		= Token.USR25,
			AS			= Token.USR26,
			SYSCOLUMNS	= Token.USR27,
			SYSOBJECTS	= Token.USR28,
			
			WHERE		= Token.USR29,
			GROUP		= Token.USR30,
			HAVING		= Token.USR31,
			ORDER		= Token.USR32,

			INNER		= Token.USR33,
			LEFT		= Token.USR34,
			RIGHT		= Token.USR35,
			FULL		= Token.USR36,
			OUTER		= Token.USR37,
			JOIN		= Token.USR38

		}

		//---------------------------------------------------------------------------------------------------
		public SQLTableParser(string fileName) : base(fileName)
		{
			this.checkLogErrorsToValidateParsing = false;
			this.oracle = fileName.ToLower().Replace("/", "\\").IndexOf("\\oracle\\") != -1;
		}

		//---------------------------------------------------------------------------------------------------
		public SQLTableParser(string fileName, bool changeFileName, bool backupFile ) : base(fileName)
		{
			this.changeFileName = changeFileName;
			this.backupFile = backupFile;
			this.checkLogErrorsToValidateParsing = false;
			this.oracle = fileName.ToLower().Replace("/", "\\").IndexOf("\\oracle\\") != -1;

		}

		//---------------------------------------------------------------------------------------------------
		protected override void InitParser(Parser parser)
		{
			base.InitParser(parser);
			
			parser.UserKeywords.Add("TABLE",		SQLToken.TABLE);
			parser.UserKeywords.Add("CREATE",		SQLToken.CREATE);
			parser.UserKeywords.Add("dbo",			SQLToken.DBO);
			parser.UserKeywords.Add("CONSTRAINT",	SQLToken.CONSTRAINT);

			parser.UserKeywords.Add("PRIMARY",		SQLToken.PRIMARY);
			parser.UserKeywords.Add("FOREIGN",		SQLToken.FOREIGN);
			parser.UserKeywords.Add("KEY",			SQLToken.KEY);
			parser.UserKeywords.Add("REFERENCES",	SQLToken.REFERENCES);
			parser.UserKeywords.Add("object_id",	SQLToken.OBJECT_ID);
			parser.UserKeywords.Add("ALTER",		SQLToken.ALTER);
			parser.UserKeywords.Add("GO",			SQLToken.GO);
			parser.UserKeywords.Add("ADD",			SQLToken.ADD);
			parser.UserKeywords.Add("END",			SQLToken.END);
			parser.UserKeywords.Add("DELETE",		SQLToken.DELETE);
			parser.UserKeywords.Add("FROM",			SQLToken.FROM);
			parser.UserKeywords.Add("INSERT",		SQLToken.INSERT);
			parser.UserKeywords.Add("UPDATE",		SQLToken.UPDATE);
			parser.UserKeywords.Add("INTO",			SQLToken.INTO);
			parser.UserKeywords.Add(".",			SQLToken.DOT);
			parser.UserKeywords.Add("INDEX",		SQLToken.INDEX);
			parser.UserKeywords.Add("ON",			SQLToken.ON);
			parser.UserKeywords.Add("DROP",			SQLToken.DROP);
			parser.UserKeywords.Add("DESCRIPTION",	Token.ID);
			parser.UserKeywords.Add("PATH",			Token.ID);
			parser.UserKeywords.Add("Exec",			SQLToken.EXEC);
			parser.UserKeywords.Add("VIEW",			SQLToken.VIEW);
			parser.UserKeywords.Add("OR",			SQLToken.OR);
			parser.UserKeywords.Add("REPLACE",		SQLToken.REPLACE);
			parser.UserKeywords.Add("AS",			SQLToken.AS);
			parser.UserKeywords.Add("dbo.syscolumns.name", SQLToken.SYSCOLUMNS);
			parser.UserKeywords.Add("dbo.sysobjects.name", SQLToken.SYSOBJECTS);

			parser.UserKeywords.Add("WHERE", SQLToken.WHERE);
			parser.UserKeywords.Add("GROUP", SQLToken.GROUP);
			parser.UserKeywords.Add("HAVING", SQLToken.HAVING);
			parser.UserKeywords.Add("ORDER", SQLToken.ORDER);
			parser.UserKeywords.Add("INNER", SQLToken.INNER);
			parser.UserKeywords.Add("LEFT", SQLToken.LEFT);
			parser.UserKeywords.Add("RIGHT", SQLToken.RIGHT);
			parser.UserKeywords.Add("FULL", SQLToken.FULL);
			parser.UserKeywords.Add("OUTER", SQLToken.OUTER);
			parser.UserKeywords.Add("JOIN", SQLToken.JOIN);

			// overrides base parser keywords (that I wanto to consider as ID)
			parser.UserKeywords.Add("Subject", Token.ID);
			parser.UserKeywords.Add("Development", Token.ID);

		}

		//--------------------------------------------------------------------------------
		private static ArrayList GetConstraintNames(bool oracle)
		{
			return oracle ? oracleConstraintNames : sqlConstraintNames;
		}
		
		//---------------------------------------------------------------------------------------------------
		public override bool Parse()
		{
			PrepareInfo();

			bool result = base.Parse();
			
			if (result && ChangeFileName)
			{
				UpdateInfoXml();
				SafeRemoveFile(BackupFile);
			}

			return result;
		}

		//--------------------------------------------------------------------------------
		private void UpdateInfoXml()
		{
			if (!modified || string.Compare(fileName, destinationFileName, true) == 0) return;

			bool create, infoModified = false, error = true;
			
			string xmlPath = GetInfoXmlFile(out create);
			if (!File.Exists(xmlPath))
				return;
			
			XmlDocument d = new XmlDocument();
			d.Load(xmlPath);
			
			string scriptName = Path.GetFileName(fileName);
			string newScriptName = Path.GetFileName(destinationFileName);
			string xPath = string.Empty;
			try
			{
				if (create)
				{
					xPath = "//Step[@script]";
				}
				else
				{
					// uses folder name in irder to determine the release number
					string releasePath = Path.GetDirectoryName(fileName);
				
					//folder name contains release number in the form: release_<number>
					int index = releasePath.LastIndexOf("_");
					int relNumber = Convert.ToInt32(releasePath.Substring(index + 1));
					xPath = string.Format("//DBRel[@numrel='{0}']/node()/Step[@script]", relNumber);
				}

				XmlNodeList list = d.SelectNodes(xPath);
				foreach (XmlElement el in list)
				{
					if (string.Compare(el.GetAttribute("script"), scriptName, true) == 0)
					{
						el.SetAttribute("script", newScriptName);
						infoModified = true;
					}
					else if (string.Compare(el.GetAttribute("script"), newScriptName, true) == 0)
					{
						error = false;	//if new script name is already in this file, this means thast it already
						//has been translated (so, if 'infoModified' is false, this isn't an error)
					}
				}
			}
			catch(Exception ex)
			{
				throw new ApplicationException
					(
					string.Format(FileConverterStrings.UpdateInfoXmlException, xmlPath, scriptName),
					ex
					);
			}

			if (infoModified)
			{
				TryToCheckOut(xmlPath);

				d.Save(xmlPath);
				GlobalContext.LogManager.Message
					(
					string.Format(FileConverterStrings.UpdatedInfoXml, xmlPath, scriptName, newScriptName),
					DiagnosticType.Information
					);
			}
			else if(error)
				throw new ApplicationException
					(
					string.Format(FileConverterStrings.UpdateInfoXmlFailed, xmlPath, scriptName)
					);
		}

		//--------------------------------------------------------------------------------
		private string GetInfoXmlFile(out bool create)
		{
			int index = fileName.ToLower().Replace("/", "\\").LastIndexOf("\\databasescript\\create\\");
			
			string path = string.Empty;
			if (index != -1)
			{
				path = fileName.Substring(0, index + "\\databasescript\\create\\".Length);
				path = Path.Combine(path, "createinfo.xml");
				create = true;
			}
			else
			{
				create = false;
				index = fileName.ToLower().Replace("/", "\\").LastIndexOf("\\databasescript\\upgrade\\");
				if (index != -1)
				{
					path = fileName.Substring(0, index + "\\databasescript\\upgrade\\".Length);
					path = Path.Combine(path, "upgradeinfo.xml");
				}
			}

			return path;
		}

		//---------------------------------------------------------------------------------------------------
		private bool PrepareInfo()
		{
			constraintList = new Hashtable();
			indexList = new Hashtable();

			parser = new Parser(Parser.SourceType.FromFile);
			InitParser(parser);
			parser.Open(fileName);

			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				if (startOfLine && parser.CurrentLexeme.StartsWith("--"))
				{
					parser.SkipLine();
					continue;
				}

				startOfLine = false;
				switch(t)
				{
					case (Token)SQLToken.CREATE:
						parser.SkipToken();
						if (parser.LookAhead() == (Token)SQLToken.TABLE)
						{
							parser.SkipToken();
							GetTableName(out currentTable);
						}
						else if (parser.LookAhead() == (Token)SQLToken.INDEX)
						{
							parser.SkipToken();
							GetIndexInfo();
						}
						break;
					case (Token)SQLToken.CONSTRAINT:
						parser.SkipToken();
						GetConstraintInfo();
						break;
					default:
						parser.SkipToken();
						break;
				}
			}

			parser.Close();
			parser = null;
			return constraintList.Count > 0;
			
		}

		//---------------------------------------------------------------------------------------------------
		private bool GetIndexInfo()
		{
			string indexName;
			if (!GetItemName(out indexName)) return false;

			parser.SkipToken(); //salta ON

			string tableName;
			GetTableName(out tableName);

			indexList.Add(indexName, tableName);
			
			return true;
		}

		//---------------------------------------------------------------------------------------------------
		private bool GetConstraintInfo()
		{
			ConstraintInfo info = new ConstraintInfo();
			
			info.Table = currentTable;
			string constraintName = string.Empty;
			if (!GetItemName(out constraintName)) return false;

			while(!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case (Token) SQLToken.PRIMARY:
						parser.SkipToken();
						if (!parser.ParseTag((Token)SQLToken.KEY)) return false;
						info.Type = ConstraintInfo.ConstraintType.PRIMARY_KEY;
						constraintList.Add(constraintName, info);
						return true;;
					case (Token) SQLToken.FOREIGN:
						parser.SkipToken();
						if (!parser.ParseTag((Token)SQLToken.KEY)) return false;
						info.Type = ConstraintInfo.ConstraintType.FROEIGN_KEY;
						break;
					case (Token) SQLToken.REFERENCES:
						parser.SkipToken();
						if (info.Type != ConstraintInfo.ConstraintType.FROEIGN_KEY) return false;
						GetTableName(out info.ReferenceTable);
						constraintList.Add(constraintName, info);
						return true;
					default: 
						parser.SkipToken();
						break;
				}
			}

			return false;

		}

		//---------------------------------------------------------------------------------------------------
		public bool GetItemName(out string itemName)
		{
			itemName = string.Empty;
			if (Oracle)
				return parser.ParseString(out itemName);
			else
				return parser.ParseSquareOpen() && 
					parser.ParseID(out itemName) &&
					parser.ParseSquareClose();
		}

		//---------------------------------------------------------------------------------------------------
		public bool GetToken(out string token)
		{
			token = string.Empty;
			if (Oracle)
				return parser.ParseString(out token);
			else
				return parser.ParseID(out token);
		}

		//---------------------------------------------------------------------------------------------------
		public void GetTableName(out string tableName)
		{
			tableName = string.Empty;
			// mi aspetto i seguenti tokens: 
			// [dbo].[<nome_tabella>]			(per sql server) 
			// "<nome_tabella>"					(per oracle)

			bool result = Oracle ? GetOracleTableName(out tableName) : GetSqlTableName(out tableName);
			if (!result)
				ThrowException(FileConverterStrings.CannotReadTableName);
			
			FirstObjectName = TranslateTable(tableName, true);
		}

		
		//---------------------------------------------------------------------------------------------------
		public bool GetOracleTableName(out string tableName)
		{
			// mi aspetto i seguenti tokens: 
			// "<nome_tabella>"					(per oracle)
			tableName = string.Empty;
			
			return parser.ParseString(out tableName);
		}

		
		//---------------------------------------------------------------------------------------------------
		public bool GetSqlTableName(out string tableName)
		{
			// mi aspetto i seguenti tokens: 
			// [dbo].[<nome_tabella>]			(per sql server) 
			tableName = string.Empty;
			
			return
				parser.ParseSquareOpen() &&
				parser.ParseTag((Token)SQLToken.DBO) &&
				parser.ParseSquareClose() && 
				parser.ParseTag((Token)SQLToken.DOT) &&
				parser.ParseSquareOpen() && 
				parser.ParseID(out tableName) &&
				parser.ParseSquareClose();
		}

		//---------------------------------------------------------------------------------------------------
		protected override void ProcessBuffer()
		{
			string objectName = string.Empty;

			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				if (startOfLine && parser.CurrentLexeme.StartsWith("--"))
				{
					parser.SkipLine();
					continue;
				}

				startOfLine = false;
				switch(t)
				{
					case (Token)SQLToken.EXEC:
						parser.SkipToken();
						GlobalContext.LogManager.Message(FileConverterStrings.ExecUnsupported, DiagnosticType.Warning);
						break;
					case (Token)SQLToken.SYSOBJECTS:
						ParseSystemTable();
						break;
					case (Token)SQLToken.SYSCOLUMNS:
						ParseSystemColumn();
						break;
					case (Token)SQLToken.OBJECT_ID:
						parser.SkipToken();
						ParseTableTameFromObjectID();
						break;
					case (Token)SQLToken.DROP:
						FirstOperationToken = SQLToken.DROP;
						parser.SkipToken();
						if (parser.LookAhead() == (Token)SQLToken.VIEW)
						{
							parser.SkipToken();
							ParseDropView();
						}
						break;
						
					case (Token)SQLToken.CREATE:
						FirstOperationToken = SQLToken.CREATE;
						parser.SkipToken();
						if (parser.LookAhead() == (Token)SQLToken.TABLE)
						{
							parser.SkipToken();
							ParseCreateTable();
						}
						else if (parser.LookAhead() == (Token)SQLToken.INDEX)
						{
							parser.SkipToken();
							ParseCreateIndex();
						}

						if (parser.Parsed((Token)SQLToken.OR) && 
							parser.Parsed((Token)SQLToken.REPLACE))
						{
							//	Oracle = true;
						}

						if (parser.LookAhead() == (Token)SQLToken.VIEW)
						{
							parser.SkipToken();
							ParseCreateView();
						}
						break;	

					case (Token)SQLToken.ALTER:
						FirstOperationToken = SQLToken.ALTER;
						parser.SkipToken();
						if (parser.ParseTag((Token)SQLToken.TABLE))
						{
							ParseAlterTable();
						}
						break;	
					case (Token)SQLToken.DELETE:
						FirstOperationToken = SQLToken.DELETE;
						parser.SkipToken();
						if (parser.ParseTag((Token)SQLToken.FROM))
						{
							ParseDelete();
						}
						break;	
					case (Token)SQLToken.INSERT:
						FirstOperationToken = SQLToken.INSERT;
						parser.SkipToken();
						if (parser.ParseTag((Token)SQLToken.INTO))
						{
							ParseInsert();
						}
						break;	
					case (Token)SQLToken.UPDATE:
						FirstOperationToken = SQLToken.UPDATE;
						parser.SkipToken();
						ParseUpdate();
						break;	
					default:
						parser.SkipToken();
						break;
				}	
			}
			parser.Close();
			
			// Oracle convention needs upper case names 
			if (modified && Oracle)
			{
				for (long line = 1; line <= lines.Keys.Count; line++)
				{
					ParserInfo info = (ParserInfo)lines[line];
					if (info.Line != null) 
					{
						info.Line = info.Line.ToUpper();
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void ParseSystemTable()
		{
			string tableName = ParseSystemObject();
			if (tableName == null) return;

			lastSystemTableName = tableName;

			ReplaceWord(parser.CurrentPos + 1, tableName, TranslateTable(tableName, true), true);

		}
		
		//--------------------------------------------------------------------------------
		private void ParseSystemColumn()
		{
			string columnName = ParseSystemObject();
			if (columnName == null) return;	

			ReplaceWord(parser.CurrentPos + 1, columnName, TranslateColumn(lastSystemTableName, columnName, true), true);

		}
		
		//--------------------------------------------------------------------------------
		private string ParseSystemObject()
		{
			parser.SkipToken();

			string name = null;
			if (parser.Parsed(Token.ASSIGN))
			{
				if (!parser.ParseString(out name))
					ThrowException(FileConverterStrings.ParseSystemObjectException);
			}
			return name;		
		}

		//---------------------------------------------------------------------------------------------------
		public void ParseCreateIndex()
		{
			string indexName;
			
			if (!GetItemName(out indexName))
				ThrowException(FileConverterStrings.ParseCreateIndexReadingException);

			string tableName = indexList[indexName] as string;
			if (tableName == null) return;

			string newIndexName = indexName.Replace(tableName, TranslateTable(tableName, true));

			ReplaceWord(parser.CurrentPos, indexName, newIndexName);

			if (!parser.ParseTag((Token)SQLToken.ON))
				ThrowException(FileConverterStrings.ReadingIndexStructureException);

			GetTableName(out tableName);

			ReplaceWord(parser.CurrentPos, tableName, TranslateTable(tableName, true));

			ParseColumns(tableName);
			
		}
		
		//---------------------------------------------------------------------------------------------------
		public void ParseCreateTable()
		{
			string tableName;
			
			ParseTable(out tableName);

			if (!parser.ParseOpen()) return;

			Token t;
			while ((t = parser.LookAhead()) != Token.ROUNDCLOSE  && !parser.Eof)
			{
				switch(t)
				{
					case (Token) SQLToken.CONSTRAINT: 
						parser.SkipToken();
						ParseConstraint(tableName); break;
					case Token.SQUAREOPEN: // per sql server
						parser.SkipToken();
						ParseSqlColumn(tableName); 
						break;
					case Token.TEXTSTRING:	// per Oracle
						ParseOracleColumn(tableName); 
						break;
					case (Token) SQLToken.ON:
						SkipOnPrimary();
						break;
					default:
						parser.SkipToken();
						break;

				}
			}

		}
		
		//---------------------------------------------------------------------------------------------------
		public void ParseCreateView()
		{
			bool fromSection = false;
			string viewName = string.Empty;
			bool atStartOwWord = true;
			
			ArrayList untranslatedSingleTokens = new ArrayList();
			ArrayList untranslatedQualifiedTokens = new ArrayList();
			ArrayList tables = new ArrayList();
			Hashtable aliases = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

			if (parser.LookAhead() == Token.SQUAREOPEN)
			{
				atStartOwWord = false;
				GetTableName(out viewName);
			}
			else 
			{
				if (!GetToken(out viewName))
					ThrowException(FileConverterStrings.ReadingViewNameException);
				FirstObjectName = TranslateTable(viewName, true);
			}

			ReplaceWord(parser.CurrentPos, viewName, TranslateTable(viewName, true), atStartOwWord);

			string tokenName, newTokenName;
			if (parser.Parsed(Token.ROUNDOPEN))
			{
				while (!parser.Eof && !parser.Parsed(Token.ROUNDCLOSE))
				{
					if (!GetToken(out tokenName))
						ThrowException(FileConverterStrings.ReadingViewColumnNameException);
					
					newTokenName = TranslateColumn(viewName, tokenName, true);
					ReplaceWord(parser.CurrentPos, tokenName, newTokenName, true);
					parser.Parsed(Token.COMMA);
				}
			}

			try
			{
				while (!parser.Eof)
				{
					switch (parser.LookAhead())
					{
						case (Token) SQLToken.FROM:
						{
							parser.SkipToken();
							fromSection = true;
							break;
						}

						case Token.SELECT:
						{
							parser.SkipToken();
							fromSection = false;
							break;
						}
						case (Token) SQLToken.AS:
						{
							parser.SkipToken();
							if (fromSection) 
							{
								continue;
							}
							if (GetToken(out tokenName))
							{
								newTokenName = TranslateColumn(viewName, tokenName, true);
								base.ReplaceWord(parser.CurrentPos, tokenName, newTokenName, true);
							}
							break;

						}
						case Token.ID:
						{
							parser.ParseID(out tokenName);
							if (tokenName.IndexOf(".") == -1)
							{
								newTokenName = TranslateTable(tokenName, true);
								if (tokenName == newTokenName)
									untranslatedSingleTokens.Add(new ParserPosition(parser.CurrentPos + tokenName.Length, parser.CurrentLine, tokenName));
								else
								{
									base.ReplaceWord(parser.CurrentPos, tokenName, newTokenName, true);
									tables.Add(tokenName);
									parser.Parsed((Token) SQLToken.AS);
									if (parser.LookAhead(Token.ID))
									{
										string alias;
										parser.ParseID(out alias);
										aliases[alias] = tokenName; 
									}
									
								}
							}
							else 
							{
								newTokenName = TranslateQualifiedColumn(tokenName, true);
								if (tokenName == newTokenName)
									untranslatedQualifiedTokens.Add(new ParserPosition(parser.CurrentPos + tokenName.Length, parser.CurrentLine, tokenName));
								else
									base.ReplaceWord(parser.CurrentPos, tokenName, newTokenName, true);
							}
						
							break;
						}

						case (Token) SQLToken.GO:
						{
						
							return;
						}
						default: 
							parser.SkipToken();
							break;
					}
				}
			}
			finally
			{
				
				for (int i = 0; i < untranslatedSingleTokens.Count; i++)
				{
					ParserPosition potentialColumn = untranslatedSingleTokens[i] as ParserPosition;
							
					//first I look in table names
					foreach (string table in tables)
					{
						string newColumnName = TranslateColumn(table, potentialColumn.Name, true);
						if (newColumnName != potentialColumn.Name)
						{
							ReplaceWord(potentialColumn.Y, potentialColumn.X, potentialColumn.Name, newColumnName);
							break;
						}
								
					}
	
				}

				for (int i = 0; i < untranslatedQualifiedTokens.Count; i++)
				{
					ParserPosition potentialColumn = untranslatedQualifiedTokens[i] as ParserPosition;
					string[] tokens = potentialColumn.Name.Split('.');
					
					//then in aliases
					string table = aliases[tokens[0]] as string;
					if (table != null)
					{
						string newColumnName = tokens[0] + '.' + TranslateColumn(table, tokens[1], true);
						if (newColumnName != potentialColumn.Name)
						{
							ReplaceWord(potentialColumn.Y, potentialColumn.X, potentialColumn.Name, newColumnName);
						}
					}
				}
			}
		}

		//---------------------------------------------------------------------------------------------------
		public void ParseDropView()
		{
			string tableName = string.Empty;
			GetTableName(out tableName);
			
			ReplaceWord(parser.CurrentPos, tableName, TranslateTable(tableName, true));

		}

		//--------------------------------------------------------------------------------
		private void SkipOnPrimary()
		{
			if (!parser.LookAhead((Token)SQLToken.ON)) return;

			if (!parser.ParseTag((Token)SQLToken.ON)	||
				!parser.ParseSquareOpen()				||
				!parser.ParseTag((Token)SQLToken.PRIMARY)||	//normalmente trovo PRIMARY, ma non è detto
				!parser.ParseSquareClose())
				ThrowException(FileConverterStrings.ReadingPrimaryStatement);
		}
		
		//---------------------------------------------------------------------------------------------------
		public void ParseAlterTable()
		{
			string tableName;
			
			ParseTable(out tableName);

			if (!parser.LookAhead((Token) SQLToken.ADD) && 
				!parser.LookAhead((Token) SQLToken.ALTER) &&
				!parser.LookAhead((Token) SQLToken.DROP))
				ThrowException(string.Format(FileConverterStrings.ReadingAlterTableScript, tableName));

			parser.SkipToken();

			Token t;
			while	( 
				(t = parser.LookAhead()) != (Token)SQLToken.GO  &&
				t != (Token)SQLToken.CONSTRAINT &&  
				t != (Token)SQLToken.END &&  
				!parser.Eof
				)
			{
				switch(t)
				{
					case Token.SQUAREOPEN: // per sql server
						parser.SkipToken();
						ParseSqlColumn(tableName); 
						break;
					case Token.TEXTSTRING:	// per Oracle
						ParseOracleColumn(tableName); 
						break;
					default:
						parser.SkipToken();
						break;
				}
			}
			

		}

		//---------------------------------------------------------------------------------------------------
		public void ParseTable(out string tableName)
		{
			tableName = string.Empty;

			GetTableName(out tableName);

			ReplaceWord(parser.CurrentPos, tableName, TranslateTable(tableName, true));
			
		}

		//---------------------------------------------------------------------------------------------------
		protected new void ReplaceWord(int currentPos, string origin, string destination)
		{
			if (Oracle) currentPos++; //skips first " character
			base.ReplaceWord(currentPos, origin, destination, Oracle);
		}

		//---------------------------------------------------------------------------------------------------
		protected new void ReplaceWord(int currentPos, string origin, string destination, bool currPosAtStartOfWord)
		{
			if (Oracle) currentPos++; //skips first " character
			base.ReplaceWord(currentPos, origin, destination, currPosAtStartOfWord);
		}

		//--------------------------------------------------------------------------------
		private string GetConstraintName(string prefix, string firstName, string secondName, bool addNumber)
		{
			string name;
			if (firstName.StartsWith("MA_"))
				firstName = firstName.Substring(3);

			if (secondName != null && secondName.StartsWith("MA_"))
				secondName = secondName.Substring(3);

			if (secondName == null)
				name = prefix + "_" + Truncate(firstName, 27);
			else
				name = prefix + "_" + Truncate(firstName, 10)+ "_" + Truncate(secondName, 10);

			if (addNumber)
			{
				int n = 0;
				string numberedName; 
				do
				{
					numberedName = name + n.ToString("_00");
					n++;
				}
				while (GetConstraintNames(Oracle).Contains(numberedName.ToUpper()));

				name = numberedName;
			}
	
			GetConstraintNames(Oracle).Add(name.ToUpper());
			return name;
		}


		//--------------------------------------------------------------------------------
		private string Truncate(string s, int chars)
		{
			if (s.Length <= chars) return s;
			return s.Substring(0, chars);
		}

		//--------------------------------------------------------------------------------
		public void ParseColumns(string tableName)
		{
			// arrivo all'apertura della prima tonda
			while (!parser.Eof && !parser.ParseOpen());
			if (parser.LookAhead() == Token.SQUAREOPEN)
				ParseSqlColumns(tableName);
			else if (parser.LookAhead() == Token.TEXTSTRING)
				ParseOracleColumns(tableName);
			else
				ThrowException(string.Format(FileConverterStrings.ParseColumnsException, tableName));
			
		}	
		
		//---------------------------------------------------------------------------------------------------
		public void ParseSqlColumns(string tableName)
		{
			if (Oracle) ThrowException(FileConverterStrings.InvalidSyntaxOracleScript);
			
			while (!parser.Eof && parser.LookAhead() == Token.SQUAREOPEN)
			{
				parser.SkipToken();
				ParseSqlColumn(tableName);
			}
		}		


		//---------------------------------------------------------------------------------------------------
		public void ParseOracleColumns(string tableName)
		{
			if (!Oracle) ThrowException(FileConverterStrings.InvalidSyntaxSqlScript);
			
			while (!parser.Eof && parser.LookAhead() == Token.TEXTSTRING)
			{
				ParseOracleColumn(tableName);
			}
		}		

		//--------------------------------------------------------------------------------
		public void ParseSqlColumn(string tableName)
		{
			if (Oracle) ThrowException(FileConverterStrings.InvalidSyntaxOracleScript);
			
			string columnName;
			if (!parser.ParseID(out columnName) || !parser.ParseSquareClose())
				ThrowException(string.Format(FileConverterStrings.ParseColumnException, tableName + ":" + parser.CurrentLexeme));
			
			ParseColumn(tableName, columnName);
		}

		//--------------------------------------------------------------------------------
		public void ParseOracleColumn(string tableName)
		{
			if (!Oracle) ThrowException("Sintassi non valida per script SqlServer");
			
			string columnName;
			if (!parser.ParseString(out columnName))
				ThrowException(string.Format(FileConverterStrings.ParseColumnException, tableName + ":" + parser.CurrentLexeme));
			
			ParseColumn(tableName, columnName);
		}

		//---------------------------------------------------------------------------------------------------
		public void ParseColumn(string tableName, string columnName)
		{
			string newTableName = TranslateTable(tableName, true);
			string newColumnName = TranslateColumn(tableName, columnName, true);
			ReplaceWord(parser.CurrentPos, columnName, newColumnName);
			int roundToClose = 1;
			
			bool goOn = true;
			while(goOn && !parser.Eof)
			{
				switch (parser.LookAhead())
				{
					case (Token)SQLToken.GO:			goOn = false;							break;
					case (Token)SQLToken.END:			goOn = false;							break;
					case (Token)SQLToken.CONSTRAINT:	goOn = true; ParseColumnConstraint(newTableName, newColumnName); break;
					case Token.COMMA:					goOn = (roundToClose) > 1;				goto default;
					case Token.ROUNDOPEN:				roundToClose++;							goto default;
					case Token.ROUNDCLOSE:				goOn = (--roundToClose) > 0;			goto default;
					default:							parser.SkipToken();						break;		
				};
			}
		
		}
		
		//--------------------------------------------------------------------------------
		private void ParseColumnConstraint(string tableName, string columnName)
		{
			parser.SkipToken(); //constraint
			if (Oracle) return; //oracle does't have a column constraint name
			
			string oldName;
			bool squareOpen = parser.Parsed(Token.SQUAREOPEN);
			
			if (!parser.ParseID(out oldName))
				ThrowException(string.Format(FileConverterStrings.ParseColumnConstraintException, columnName));
			
			string newName = GetConstraintName("DF", tableName, columnName, true);
			ReplaceWord(parser.CurrentPos, oldName, newName, true);
			
			if (squareOpen && !parser.ParseSquareClose())
				ThrowException(string.Format(FileConverterStrings.ParseColumnConstraintException, columnName));
			
		}

		//--------------------------------------------------------------------------------
		public void ParseConstraint(string tableName)
		{
			string constraintName = string.Empty;
			if (!GetItemName(out constraintName)) return;
			
			ConstraintInfo info = constraintList[constraintName] as ConstraintInfo;
			
			if (info == null || info.Type == ConstraintInfo.ConstraintType.NONE) return;

			bool isForeign = info.Type == ConstraintInfo.ConstraintType.FROEIGN_KEY;

			string newName = TranslateTable(info.Table, true);
			
			if (isForeign)
				newName = GetConstraintName("FK", newName, TranslateTable(info.ReferenceTable, true), true);
			else
				newName = GetConstraintName("PK", newName, null, false);

			ReplaceWord(parser.CurrentPos, constraintName, newName);
			
			ParseColumns(tableName);

			if (!isForeign) return;

			// arrivo al tag REFERENCES
			while (!parser.Eof && !parser.ParseTag((Token)SQLToken.REFERENCES));

			string referencedTable;
			ParseTable(out referencedTable);

			ParseColumns(referencedTable);
		}

		//---------------------------------------------------------------------------------------------------
		public void ParseTableTameFromObjectID()
		{
			string tableName = string.Empty;

			if (!parser.ParseOpen()) return;

			// potrebbe esserci una N da saltare
			if (parser.LookAhead() == Token.ID)
				parser.SkipToken();
			
			string tableString;
			if (!parser.ParseString(out tableString) || !parser.ParseClose()) return;
			
			//mi arriva '[dbo].[<nome_tabella>]'
			//devo isolare <nome_tabella>
			tableName = tableString.Substring(7, tableString.Length-8);
			ReplaceWord(parser.CurrentPos - 2, tableName, TranslateTable(tableName, true));
		}

		//---------------------------------------------------------------------------------------------------
		public void ParseDelete()
		{
			string dummy;
			
			ParseTable(out dummy);
		}
	
		//---------------------------------------------------------------------------------------------------
		public void ParseInsert()
		{
			string tableName;
			
			ParseTable(out tableName);

			ParseColumns(tableName);
		}

		//---------------------------------------------------------------------------------------------------
		public void ParseUpdate()
		{
			string updateTableName;
			
			ParseTable(out updateTableName);			
			Token[] stopTokens = new Token[]
				{
					(Token)SQLToken.OBJECT_ID,
					(Token)SQLToken.CREATE,
					(Token)SQLToken.ALTER,
					(Token)SQLToken.DELETE,
					(Token)SQLToken.INSERT,
					(Token)SQLToken.UPDATE,
					(Token)SQLToken.GO,
					Token.IF
				};

			ParserPosition tablePos = null, columnPos = null;
			while (!HasToStop(stopTokens))
			{
				ParserPosition p = LookAhead(columnPos != null);
				if (p == null)
				{
					if (tablePos != null)
						ReplaceWord(tablePos.Y, tablePos.X, tablePos.Name, TranslateTable(tablePos.Name, true));
					if (columnPos != null)
					{
						string tableName = tablePos == null ? updateTableName : tablePos.Name;
						ReplaceWord(columnPos.Y, columnPos.X, columnPos.Name, TranslateColumn(tableName, columnPos.Name, true));				
					}
					tablePos = columnPos = null;
					parser.SkipToken();
					continue;
				}
				if (columnPos != null)
					tablePos = columnPos;
				columnPos = p;					
			}
				
		}

		//--------------------------------------------------------------------------------
		private ParserPosition LookAhead(bool existCurrentToken)
		{
			if (existCurrentToken && !parser.Parsed((Token)SQLToken.DOT)) return null;

			string name;
			if (Oracle)
			{
				if (parser.LookAhead() == Token.TEXTSTRING)
				{
					parser.ParseString(out name);
					return new ParserPosition(parser.CurrentPos + name.Length + 1, parser.CurrentLine, name);
				}
			}
			else
			{
				if (parser.LookAhead() == Token.SQUAREOPEN)
				{
					parser.SkipToken();
					if (parser.Parsed((Token)SQLToken.DBO))
						name = "dbo";
					else if (!parser.ParseID(out name))
						ThrowLastParserErrorException();

					ParserPosition ret = new ParserPosition(parser.CurrentPos + name.Length, parser.CurrentLine, name);
					if (!parser.ParseSquareClose())
						ThrowLastParserErrorException();

					return ret;
				}
			}

			return null;

		}

		//--------------------------------------------------------------------------------
		protected override void CurrentLineChanged()
		{
			base.CurrentLineChanged ();
			
			startOfLine = true;
		}
	}

	//================================================================================
	public class PostMigrationSqlParser : SQLTableParser
	{
		//--------------------------------------------------------------------------------
		public PostMigrationSqlParser(string fileName, bool changeFileName, bool backupFile) : base(fileName, changeFileName, backupFile) {}


		//--------------------------------------------------------------------------------
		protected override void ProcessBuffer()
		{
			try
			{
				FirstOperationToken = SQLToken.CREATE; //dummy assignment to force rename of file

				while (!parser.Eof)
				{
					if (FoundDatabaseName())
					{
						if (!parser.ParseTag((Token)SQLToken.DOT))
							ThrowException(string.Format(FileConverterStrings.UnexpectedToken, ".", parser.CurrentLexeme));	
					

						if (!Oracle)
						{
							if ( 
								!parser.ParseSquareOpen() || 
								! parser.ParseTag((Token)SQLToken.DBO) ||
								! parser.ParseSquareClose()
								)
								ThrowException(string.Format(FileConverterStrings.UnexpectedToken, "[dbo]", parser.CurrentLexeme));
						
							if (!parser.ParseTag((Token)SQLToken.DOT))
								ThrowException(string.Format(FileConverterStrings.UnexpectedToken, ".", parser.CurrentLexeme));	
					
						}

						string table;
						if (!GetItemName(out table))
							ThrowException(FileConverterStrings.CannotReadTableName);

						if (!Path.GetFileNameWithoutExtension(fileName).ToLower().StartsWith("postmigration"))
							FirstObjectName = TranslateTable(table, true);
						ReplaceWord(parser.CurrentPos, table, TranslateTable(table, true));

						if (parser.Parsed((Token)SQLToken.DOT))
						{
							string column;
							if (!GetItemName(out column))
								ThrowException(FileConverterStrings.ErrorReadColumnName);
							ReplaceWord(parser.CurrentPos, column, TranslateColumn(table, column, true));
						}
				
					}
					else
						parser.SkipToken();
				}

			}
			catch (Exception ex)
			{
				ThrowException(ex.Message);
			}
			base.ProcessBuffer();
		}

		//--------------------------------------------------------------------------------
		private bool FoundDatabaseName()
		{
			string name;
			
			switch (parser.LookAhead())
			{
				case Token.SQUAREOPEN:
				{
					if (parser.ParseSquareOpen() && 
						parser.ParseID(out name) && 
						parser.ParseSquareClose() 
						) return string.Compare(name, "MagoNet", true) == 0;
					break;
				}
				case Token.TEXTSTRING:
				{
					if (parser.ParseString(out name) )
					{
						if (string.Compare(name, "MagoNet", true) == 0) 
							return true;
						
						if (name.ToLower().StartsWith("[magonet]"))
						{
							string pattern = @"(?<prefix>\[MagoNet\]\.\[dbo\].\[)(?<table>\w+)\](\.\[(?<columnn>\w+)\])?"; 
							string newName = Regex.Replace(name, pattern, new MatchEvaluator(Translate), RegexOptions.IgnoreCase);
							if (newName != name)
								((ObjectParser)this).ReplaceWord(parser.CurrentPos + 1, name, newName, true);
						}
						return false;
					}
					break;
				}
				
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		private string Translate (Match m)
		{
			string retVal = m.Groups["prefix"].Value + TranslateTable(m.Groups["table"].Value, true) + "]";

			if (m.Groups["column"].Value.Length != 0)
				retVal += ".[" + TranslateColumn(m.Groups["table"].Value, m.Groups["column"].Value, true) + "]";

			return retVal;
		}
	}
}
