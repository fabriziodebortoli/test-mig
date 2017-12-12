using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microarea.TaskBuilderNet.Core.Lexan;

namespace Microarea.Library.SqlScriptUtility
{
	//=======================================================================================================
	public class NoCommentLexanSqlParser : LexanSqlParser
	{
		//-----------------------------------------------------------------------------
		public NoCommentLexanSqlParser()
			: base(null)
		{
			parser = new Parser(Parser.SourceType.FromString);
		}

		//-----------------------------------------------------------------------------
		public NoCommentLexanSqlParser(SqlTableList aTablesList)
			: base(aTablesList)
		{
			parser = new Parser(Parser.SourceType.FromString);
		}

		//-----------------------------------------------------------------------------
		public override bool Parse(string fileName)
		{
			string noCommentString = RemoveComments(fileName);
			return base.Parse(noCommentString);
		}

		//-----------------------------------------------------------------------------
		public override bool ParseAlterTableAddedColumns(string fileName, out Hashtable addedColumnsForTables, out IList<TableUpdate> updateColumns)
		{
			string noCommentString = RemoveComments(fileName);
			return base.ParseAlterTableAddedColumns(noCommentString, out addedColumnsForTables, out updateColumns);
		}

		///<summary>
		/// Tramite una Regular-Expression rimuove tutti i commenti multilinea /* ... */
		/// e quelli su linea singola ( -- )
		///</summary>
		//-----------------------------------------------------------------------------
		public string RemoveComments(string fileName)
		{
			if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
				return string.Empty;

			FileInfo fi = new FileInfo(fileName);
			Stream fs;

			fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.None);
			StreamReader reader = new StreamReader(fs);
			string scriptText = reader.ReadToEnd();
			fs.Close();

			string noCommentString = Regex.Replace
			   (
			   scriptText,
			   "(/\\*([^*]|[\r\n]|(\\*+([^*/]|[\r\n])))*\\*+/)|(--.*[\n]*)",
			   string.Empty, RegexOptions.Multiline
			   );

			return noCommentString;
		}
	}

	//=======================================================================================================
	public class LexanSqlParser
	{
		//---------------------------------------------------------------------------------------------------
		enum SQLToken
		{
			TABLE				= Token.USR00,
			CREATE				= Token.USR01,
			DBO					= Token.USR02,
			CONSTRAINT			= Token.USR03,
			PRIMARY				= Token.USR04,
			FOREIGN				= Token.USR05,
			KEY					= Token.USR06,
			REFERENCES			= Token.USR07,
			OBJECT_ID			= Token.USR08,
			ALTER				= Token.USR09,
			GO					= Token.USR10,
			ADD					= Token.USR11,
			END					= Token.USR12,
			DELETE				= Token.USR13,
			FROM				= Token.USR14,
			INSERT				= Token.USR15,
			INTO				= Token.USR16,
			DOT					= Token.USR17,
			INDEX				= Token.USR18,
			UPDATE				= Token.USR19,
			ON					= Token.USR20,
			DROP				= Token.USR21,
			EXEC				= Token.USR22,
			VIEW				= Token.USR23,
			OR					= Token.USR24,
			REPLACE				= Token.USR25,
			AS					= Token.USR26,
			SYSCOLUMNS			= Token.USR27,
			SYSOBJECTS			= Token.USR28,
			DEFAULT				= Token.USR29,
			CHAR_DATATYPE		= Token.USR30,
			NCHAR_DATATYPE		= Token.USR31,
			VARCHAR_DATATYPE	= Token.USR32,
			NVARCHAR_DATATYPE	= Token.USR33,
			SMALLINT_DATATYPE	= Token.USR34,
			INT_DATATYPE		= Token.USR35,
			FLOAT_DATATYPE		= Token.USR36,
			TEXT_DATATYPE		= Token.USR37,
			DATETIME_DATATYPE	= Token.USR38,
			UNIQUEID_DATATYPE	= Token.USR39,
			COLLATE				= Token.USR40,
			IDENTITY			= Token.USR41,
			NTEXT_DATATYPE		= Token.USR42,
			UNIQUE				= Token.USR43,
			CLUSTERED			= Token.USR44,
			NONCLUSTERED		= Token.USR45,
            DECLARE             = Token.USR46,
			NOCHECK				= Token.USR47,
			CASCADE				= Token.USR48
		}

		//---------------------------------------------------------------------------------------------------
		protected Parser parser = new Parser(Parser.SourceType.FromFile);
		protected SqlTableList tables = null;
        protected StringCollection viewNames = new StringCollection();
        protected StringCollection procedureNames = new StringCollection();
		protected bool Oracle = false;
		protected string currentTableName = string.Empty;

		//-----------------------------------------------------------------------------
		public LexanSqlParser(SqlTableList aTablesList)
		{
			tables = aTablesList;
		}

		//-----------------------------------------------------------------------------
		public LexanSqlParser() 
			: this(null)
		{
		}

        public StringCollection ViewNames { get { return viewNames; } }
        public StringCollection ProcedureNames { get { return procedureNames; } }

		///<summary>
		/// Parse della CREATE TABLE  / CREATE INDEX
		///</summary>
		//-----------------------------------------------------------------------------
		public virtual bool Parse(string fileName)
		{
			InitParser();
			if (!parser.Open(fileName))
				return false;

			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case (Token)SQLToken.CREATE:
					{
						parser.SkipToken();

						if (parser.LookAhead((Token)SQLToken.TABLE))
						{
							parser.SkipToken();
							ParseTable();
							break;
						}

                        if (parser.LookAhead((Token)SQLToken.VIEW))
                        {
                            parser.SkipToken();
                            ParseView();
                            break;
                        } 
                        if (parser.LookAhead(Token.PROCEDURE))
                        {
                            parser.SkipToken();
                            ParseProcedure();
                            break;
                        }

						// per la gestione degli indici: la sintassi ammessa è:
						// CREATE [ UNIQUE ] [ CLUSTERED | NONCLUSTERED ] INDEX 
						if (parser.LookAhead((Token)SQLToken.INDEX) ||
							parser.LookAhead((Token)SQLToken.UNIQUE) ||
							parser.LookAhead((Token)SQLToken.CLUSTERED) ||
							parser.LookAhead((Token)SQLToken.NONCLUSTERED))
						{
							GetIndexInfo();
							break;
						}
						break;
					}

					case (Token)SQLToken.INSERT:
					{
						System.Diagnostics.Debug.WriteLine("INSERT...");
						string tableName = null;
						ParseInsert(out tableName);
						break;
					}
					case (Token)SQLToken.DECLARE:
					{
						System.Diagnostics.Debug.WriteLine("DECLARE...");
						string declareID;
						ParseDeclare(out declareID);
						break;
					}
					case Token.BEGIN:
					default:
						parser.SkipToken();
						break;
				}
			}

			parser.Close();
			parser = null;

			return true;
		}

		//-----------------------------------------------------------------------------
		private bool ParseDeclare(out string declareID)
		{	
			declareID = null;
			parser.SkipToken();
			parser.ParseID(out declareID);
			return true;
		}

		//-----------------------------------------------------------------------------
		private bool ParseInsert(out string tableName)
		{
			tableName = null;
			parser.SkipToken();
			if(!parser.LookAhead((Token)SQLToken.INTO))
				return false;
			parser.SkipToken();
			ParseTableName(ref tableName);
			return true;
		}

		///<summary>
		/// Parse della ALTER TABLE  / UPDATE
		///</summary>
		//-----------------------------------------------------------------------------
		public virtual bool ParseAlterTableAddedColumns(string fileName, out Hashtable addedColumnsForTables, out IList<TableUpdate> updateColumns)
		{
			addedColumnsForTables = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
			updateColumns = new List<TableUpdate>();

			InitParser();
			if (!parser.Open(fileName))
				return false;

			while (!parser.Eof)
			{
				switch (parser.LookAhead())
				{
					case (Token)SQLToken.ALTER:
					{
						parser.SkipToken();
						
						if (parser.LookAhead((Token)SQLToken.TABLE))
						{
							parser.SkipToken();
							string tableName = String.Empty;

							if (ParseTableName(ref tableName))
							{
								TableColumnList addedColumns = new TableColumnList();
								
								if (addedColumnsForTables.Contains(tableName))
									addedColumns = addedColumnsForTables[tableName] as TableColumnList;
								else
									addedColumnsForTables.Add(tableName, addedColumns);

								ParseAlterTableAddedColumns(ref addedColumns);
							}
						}

						break;
					}

					case (Token)SQLToken.UPDATE:
					{
						parser.SkipToken(); 
						string tableName = String.Empty;

						if (ParseTableName(ref tableName) && !string.IsNullOrEmpty(tableName))
							//Se non viene trovato il nome della tabella vuol dire che la parola chiave 
							//UPDATE è stata letta in un contesto diverso da un update TABLE
						{
							TableUpdate updatedColumn = new TableUpdate(tableName);
							ParseSetOfUpdate(ref updatedColumn);
							updateColumns.Add(updatedColumn);
						}
						break;
					}

					default:
						parser.SkipToken();
						break;
				}
			}

			parser.Close();
			parser = null;

			return true;
		}

		///<summary>
		/// Parse della SET di uno statement di UPDATE
		///</summary>
		//-----------------------------------------------------------------------------
		public void ParseSetOfUpdate(ref TableUpdate updatedColumn)
		{
			string columnName = String.Empty;
			string whereColumnName = String.Empty;
			string whereTableName = String.Empty;

			while (!parser.Eof)
			{
				switch (parser.LookAhead())
				{
					case Token.SET:
					{
						parser.SkipToken();
						ParseColumnName(ref columnName);
						updatedColumn.SetColumnName = columnName;
						break;
					}

					case Token.ASSIGN: //(=)
					{
						parser.SkipToken();
						
						string setValue = string.Empty;
						bool isQuotedString = false;

						if (!ParseRoundBracketsContent(out setValue, out isQuotedString))
						{
							Token[] tokensToFind = { Token.WHERE, (Token)SQLToken.GO };
							ParseUntilSpecificTokens(tokensToFind, out setValue, out isQuotedString);
						}

						updatedColumn.SetValueAsString = isQuotedString;
						updatedColumn.SetValueForSql = setValue;
						int val;
						//se può essere valido anche per oracle lo metto in automatico, 
						//se fosse una espressione SQL non posso metterla, devo avvertire
						if (isQuotedString || Int32.TryParse(setValue, out val))
							updatedColumn.SetValueForOracle = setValue;
						//else todo diagnostica
						break;
					}

					case Token.WHERE:
					{
						parser.SkipToken();

						ParseColumnName(ref whereColumnName, ref whereTableName);

						updatedColumn.WhereTableName = whereTableName;
						updatedColumn.WhereColumnName = whereColumnName;

						ParseWhereOfUpdate(ref updatedColumn);
						break;
					}

					default:
					return;
				}
			}
		}

		///<summary>
		/// Parse della WHERE di uno statement di UPDATE
		///</summary>
		//-----------------------------------------------------------------------------
		public void ParseWhereOfUpdate(ref TableUpdate updatedColumn)
		{
			while (!parser.Eof)
			{
				switch (parser.LookAhead())
				{
					case Token.IS:
					{
						bool not = false;

						parser.SkipToken();

						if (parser.LookAhead(Token.NOT))
						{
							not = true;
							parser.SkipToken();
						}

						if (parser.LookAhead(Token.NULL))
						{
							parser.SkipToken();
							updatedColumn.WhereValueForSql = string.Format("IS {0}NULL", (not ? "NOT " : string.Empty));
							updatedColumn.WhereValueForOracle = string.Format("IS {0}NULL", (not ? "NOT " : string.Empty));
						}
						return;
					}

					case Token.ASSIGN:
					case Token.DIFF:
					case Token.LT:
					case Token.GT:
					case Token.LE:
					case Token.GE:             
					{
						string condition = parser.CurrentLexeme;

						parser.SkipToken();

						string whereSetValue = string.Empty;
						bool isQuotedString = false;

						if (!ParseRoundBracketsContent(out whereSetValue, out isQuotedString))
						{
							Token[] tokensToFind = { (Token)SQLToken.GO };
							ParseUntilSpecificTokens(tokensToFind, out whereSetValue, out isQuotedString);
						}
						if (isQuotedString) whereSetValue = String.Concat("'", whereSetValue, "'");
						int dummyVal;
						
						updatedColumn.WhereValueForSql = string.Concat(condition, " ", whereSetValue);
						if (isQuotedString || Int32.TryParse(whereSetValue, out dummyVal))
							updatedColumn.WhereValueForOracle = string.Concat(condition, " ", whereSetValue);
						//else todo diagnostica perchè no nriesco a tradurre per oracle
						return;
					}

					case (Token)SQLToken.GO:
						return;

					default:
					{
						parser.SkipToken();
						break;
					}
				}
			}
		}

		///<summary>
		/// Parsa tutto quello che si trova all'interno di ()
		///</summary>
		//-----------------------------------------------------------------------------
		public bool ParseRoundBracketsContent(out string content, out bool isQuotedString)
		{
			content = string.Empty;
			isQuotedString = false;

			int parCounter = 0;
			string textstring;
			long currentFilePos = 0;

			if (!parser.LookAhead(Token.ROUNDOPEN))
				return false;

			if (currentFilePos == 0)
				currentFilePos = parser.CurrentFilePos;

			parCounter++;
			parser.ParseOpen();

			while (parCounter != 0)
			{
				if (parser.LookAhead(Token.ROUNDOPEN))
					parCounter++;
				if (parser.LookAhead(Token.ROUNDCLOSE))
					parCounter--;

				if (parser.LookAhead(Token.TEXTSTRING))
				{
					parser.ParseString(out textstring);

					currentFilePos += parser.CurrentLexeme.Length;
					if (currentFilePos < parser.CurrentFilePos)
					{
						content += " ";
						currentFilePos = parser.CurrentFilePos;
					}

					isQuotedString = isQuotedString && true;
					content += String.Concat("'", textstring, "'");
					textstring = string.Empty;
					continue;
				}
				else
					isQuotedString = false;

				if (parCounter == 0)
					break;

				currentFilePos += parser.CurrentLexeme.Length;
				if (currentFilePos < parser.CurrentFilePos)
				{
					content += " ";
					currentFilePos = parser.CurrentFilePos;
				}

				content += parser.CurrentLexeme;
				parser.SkipToken();
			}

			parser.ParseClose();

			return true;
		}

		///<summary>
		/// Parsa tutto quello che si trova fino a trovare specifici token passati come parametro
		///</summary>
		//-----------------------------------------------------------------------------
		public bool ParseUntilSpecificTokens(Token[] tokensToFind, out string content, out bool isQuotedString)
		{
			content = string.Empty;
			isQuotedString = false;

			string textstring = string.Empty;

			if (tokensToFind == null || tokensToFind.Length == 0)
				return false;

			long currentFilePos = parser.CurrentFilePos;

			while (!parser.Eof && !parser.LookAhead(tokensToFind))
			{
				if (parser.LookAhead(Token.TEXTSTRING))
				{
					parser.ParseString(out textstring);
					isQuotedString = true;
					// se entra qui si presuppone che sto parsando una porzione di script che non ha ()
					// pertanto parso solo la stringa tra apici e esco dal loop (e non faccio +=)
					content = textstring;
					break;
				}
				else
				{
					currentFilePos += parser.CurrentLexeme.Length;

					if (currentFilePos < parser.CurrentFilePos)
					{
						content += " ";
						currentFilePos = parser.CurrentFilePos;
					}

					content += parser.CurrentLexeme;
					parser.SkipToken();
				}
			}

			return true;
		}
		///<summary>
		/// Parse delle ADD nelle ALTER TABLE
		///</summary>
        //-----------------------------------------------------------------------------
        public void ParseAlterTableAddedColumns(ref TableColumnList addedColumns)
        {
            while (!parser.Eof)
            {
                switch (parser.LookAhead())
                {
                    case (Token)SQLToken.ADD:
					case Token.COMMA:
					{
                        parser.SkipToken();

                        if (parser.LookAhead((Token)SQLToken.CONSTRAINT))
                           break;
                        //Dovrebbe poter parsare la ADD di CONSTRAINT
                        //GetConstraintInfo();
                        else
                        {
                            TableColumn parsedColumn = null;
                            if (ParseColumnInfo(out parsedColumn))
                                addedColumns.Add(parsedColumn);
                        }

						break;
                    }

					case (Token)SQLToken.CONSTRAINT:
					case (Token)SQLToken.NOCHECK:
					case (Token)SQLToken.ALTER:
					case (Token)SQLToken.UPDATE:
					case (Token)SQLToken.CREATE:
						return;

                    default:
                        parser.SkipToken();
                        break;
                }
            }
        }

		///<summary>
		/// Parse delle ADD nelle ALTER TABLE
		///</summary>
		//-----------------------------------------------------------------------------
		public bool ParseAlterTableAddedColumns(string fileName, string aTableName, out TableColumnList addedColumns)
		{
			addedColumns = new TableColumnList();

			InitParser();
			if (!parser.Open(fileName))
				return false;

			while (!parser.Eof)
			{
				switch (parser.LookAhead())
				{
					case (Token)SQLToken.ALTER:
					{
						parser.SkipToken();
						
						if (parser.LookAhead((Token)SQLToken.TABLE))
						{
							parser.SkipToken();

							string tableName = String.Empty;
							if (ParseTableName(ref tableName))
							{
								if (String.Compare(tableName, aTableName, true) == 0)
								{
									currentTableName = tableName;
									while (!parser.Eof)
									{
										switch (parser.LookAhead())
										{
											case (Token)SQLToken.ADD:
											{
												parser.SkipToken();

												TableColumn parsedColumn = null;
												if (ParseColumnInfo(out parsedColumn))
													addedColumns.Add(parsedColumn);
												break;
											}
											default:
												parser.SkipToken();
												break;
										}
									}
								}
							}
						}
						break;
					}
					default:
						parser.SkipToken();
						break;
				}
			}

			parser.Close();
			parser = null;

			return true;
		}

		///<summary>
		/// Parse di un INDEX
		///</summary>
		//-----------------------------------------------------------------------------
		private void GetIndexInfo()
		{
			if (Oracle)
				return;

			SqlTable currentTable = tables.GetTableByName(currentTableName);
			if (currentTable == null)
				return;

			string indexName = string.Empty;
			string indexTableName = string.Empty;
			string indexColumnName = string.Empty;
			bool uniqueIndex = false;
			bool nonClusteredIndex = true;

			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case Token.SQUAREOPEN:
					{
						// parsa il nome dell'indice
						if (string.IsNullOrEmpty(indexName))
						{
							parser.ParseSquareOpen();
							ParseIDIncludeKeywords(out indexName);
							parser.ParseSquareClose();
							break;
						}

						// parsa dopo la ON la tabella su cui viene costruito l'indice
						if (string.IsNullOrEmpty(indexTableName))
						{
							if (ParseTableName(ref indexTableName))
								currentTable.AddIndex(indexName, indexTableName, uniqueIndex, nonClusteredIndex);
						}
						else // parsa le colonne che compongono l'indice
						{
							parser.ParseSquareOpen();
							ParseIDIncludeKeywords(out indexColumnName);
							parser.ParseSquareClose();
							currentTable.AddIndexColumn(indexName, indexColumnName);
						}
						break;
					}

					case (Token)SQLToken.UNIQUE:
					{
						uniqueIndex = true;
						parser.SkipToken();
						break;
					}

					case (Token)SQLToken.CLUSTERED:
					{
						nonClusteredIndex = false;
						parser.SkipToken();
						break;
					}

					case (Token)SQLToken.NONCLUSTERED:
					{
						nonClusteredIndex = true;
						parser.SkipToken();
						break;
					}

					case (Token)SQLToken.CREATE:
					case Token.CLOSE:
						return;
					
					default:
						parser.SkipToken();
						break;
				}
			}
		}

		///<summary>
		/// Parsa il nome di una tabella
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public bool ParseTableName(ref string aTableName)
		{
			aTableName = String.Empty;

			// mi aspetto i seguenti tokens: 
			// [dbo].[<nome_tabella>] oppure [<nome_tabella>]	(per sql server) 
			// "<nome_tabella>"									(per oracle)
			Oracle = parser.LookAhead(Token.TEXTSTRING);

			bool result = Oracle ? GetOracleTableName(out aTableName) : GetSqlTableName(out aTableName);

			if (!result)
				aTableName = String.Empty;
			
			return result;
		}

		///<summary>
		/// Parsa il nome di una colonna
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public bool ParseColumnName(ref string aColumnName)
		{
			string aTableName = string.Empty; 
			return ParseColumnName(ref aColumnName, ref aTableName);
		}

		///<summary>
		/// Parsa il nome di una colonna
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public bool ParseColumnName(ref string aColumnName, ref string aTableName)
		{
			aColumnName = String.Empty;
			aTableName =  String.Empty;

			string dummy = string.Empty;

			if (parser.LookAhead(Token.TEXTSTRING))
				return false;

			// mi aspetto i seguenti tokens: [dbo].[<nome_tabella>].[<nome_colonna>] 
			// oppure [<nome_tabella>].[<nome_colonna>] oppure [<nome_colonna>]
			for (int i = 0; i < 3; i++)
			{
				if (parser.LookAhead(Token.SQUAREOPEN))
					parser.SkipToken();
				ParseIDIncludeKeywords(out dummy);
				if (!string.IsNullOrEmpty(aColumnName))
					aTableName = aColumnName;
				aColumnName = dummy;

				if (parser.LookAhead(Token.SQUARECLOSE))
					parser.SkipToken();
				if (!parser.LookAhead((Token)SQLToken.DOT))
					break;
				parser.SkipToken();
			}

			return true;
		}

		///<summary>
		/// Parsa una tabella
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public bool ParseTable()
		{
			string tableName = string.Empty;
			
			if (!ParseTableName(ref tableName))
				return false;

			tables.Add(tableName);
			currentTableName = tableName;

			if (!Oracle)
				ParseTableElement();
			
			return true;
		}

        ///<summary>
        /// Parsa il nome di una view
        ///</summary>
        //---------------------------------------------------------------------------------------------------
        public bool ParseView()
        {
            string viewName = string.Empty;

            if (!ParseTableName(ref viewName))
                return false;

            viewNames.Add(viewName);
           
            return true;
        }

        ///<summary>
        /// Parsa il nome di una storedprocedure
        ///</summary>
        //---------------------------------------------------------------------------------------------------
        public bool ParseProcedure()
        {
            string procName = string.Empty;

            if (!ParseTableName(ref procName))
                return false;

            procedureNames.Add(procName);

            return true;
        }

		///<summary>
		/// Parsa gli elementi di una tabella
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public void ParseTableElement()
		{
			if (Oracle)
				return;

			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case Token.SQUAREOPEN:
					case Token.ID:
						string colName = string.Empty;
						ParseColumns();
						break;

					case (Token)SQLToken.CONSTRAINT:
						GetConstraintInfo();
						break;

                    case Token.BEGIN:
                    case Token.END:
                    case (Token)SQLToken.GO:
                    case (Token)SQLToken.CREATE:
					case (Token)SQLToken.DECLARE:
					case (Token)SQLToken.INSERT:
						return;

					default:
						parser.SkipToken();
						break;
				}
			}
		}

		///<summary>
		/// GetConstraintInfo
		/// Parse generico di constraints (PK, FK, indici)
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public void GetConstraintInfo()
		{
			if (Oracle)
				return;

			string constraintName = string.Empty;

			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case (Token)SQLToken.CONSTRAINT:
					{
						parser.SkipToken();
						parser.ParseSquareOpen();
						ParseIDIncludeKeywords(out constraintName);
						parser.ParseSquareClose();
						break;
					}
					case (Token)SQLToken.PRIMARY:
						GetPrimaryConstraintInfo(constraintName);
						break;
					case (Token)SQLToken.FOREIGN:
						GetForeignConstraintInfo(constraintName);
						break;
					case (Token)SQLToken.UNIQUE:
						GetIndexConstraintInfo(constraintName);
						break;
                    case Token.BEGIN:
                    case Token.END:
                    case (Token)SQLToken.GO:
                    case (Token)SQLToken.CREATE:
					case (Token)SQLToken.DECLARE:
					case (Token)SQLToken.INSERT:
                        return;
					default:
						parser.SkipToken();
						break;
				}
			}
		}

		///<summary>
		/// GetIndexConstraintInfo
		/// Parse degli indici
		///</summary>
		//-----------------------------------------------------------------------------
		private void GetIndexConstraintInfo(string constraintName)
		{
			if (Oracle)
				return;

			SqlTable currentTable = tables.GetTableByName(currentTableName);
			if (currentTable == null)
				return;
			
			string indexColumnName = string.Empty;
			bool nonClustered = true;
			
			while (!parser.Eof)
			{
				switch (parser.LookAhead())
				{
                    case Token.SQUAREOPEN:
                    {
                        // L'add dell'indice può esssere effettuato solo a questo punto, perchè potrebbe essere indicato come
                        // clustered o nonclustered o solo UNIQUE, 
                        //quindi arriva qua e non è ancora stato aggiunto, 
                        // lo aggiungo sempre prima di ogni colonna, 
                        //la funzione di add si occupa di verifica l'esistenza o meno dell'indice
                        parser.ParseSquareOpen();
						ParseIDIncludeKeywords(out indexColumnName);
                        parser.ParseSquareClose();
                        currentTable.AddIndex(constraintName, currentTableName, true, nonClustered);
                        currentTable.AddIndexColumn(constraintName, indexColumnName);
                        indexColumnName = string.Empty;
                        break;
                    }
					
					case (Token)SQLToken.CLUSTERED:
						nonClustered = false;
						parser.SkipToken();
						break;
					
					case (Token)SQLToken.NONCLUSTERED:
						nonClustered = true;
						parser.SkipToken();
                        break;
					
                    case Token.BEGIN:
                    case Token.END:
                    case (Token)SQLToken.GO:
                    case (Token)SQLToken.CREATE:
					case (Token)SQLToken.CONSTRAINT:
					case Token.CLOSE:
					case (Token)SQLToken.DECLARE:
					case (Token)SQLToken.INSERT:

						return;
		
					default:
						parser.SkipToken();
						break;
				}
			}
		}

		///<summary>
		/// GetPrimaryConstraintInfo
		/// Parse delle PK
		///</summary>
		//-----------------------------------------------------------------------------
		public void GetPrimaryConstraintInfo(string constraintName)
		{
			if (Oracle)
				return;
			SqlTable currentTable = tables.GetTableByName(currentTableName);
			if (currentTable == null)
				return;

			currentTable.AddConstraint(constraintName, true);

			string constraintColumnName = string.Empty;
			bool clustered = true;
			bool valid = false;
			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case (Token)SQLToken.KEY:
					{//Verifichiamo che sia Primary Key, 
					//per evitare di scambiare la parola Primary con altro
						valid = true;
						parser.SkipToken();
						break;
					}

					case Token.SQUAREOPEN:
					{
						parser.ParseSquareOpen();
						ParseIDIncludeKeywords(out constraintColumnName);
						parser.ParseSquareClose();
						currentTable.AddConstraintColumn(constraintName, constraintColumnName);
						constraintColumnName = string.Empty;
						break;
					}
					case (Token)SQLToken.CLUSTERED:
						clustered = true;
						parser.SkipToken();
						break;
					
					case (Token)SQLToken.NONCLUSTERED:
						clustered = false;						
						parser.SkipToken();
						break;

                    case Token.BEGIN:
                    case Token.END:
                    case (Token)SQLToken.GO:
                    case (Token)SQLToken.CREATE:
					case (Token)SQLToken.CONSTRAINT:
					case Token.CLOSE:
					case (Token)SQLToken.DECLARE:
					case (Token)SQLToken.INSERT:

						if (valid)
						currentTable.GetPrimaryKeyConstraint().Clustered = clustered;
						return;

					default:
						parser.SkipToken();
						break;
				}
			}
			if (valid)
			currentTable.GetPrimaryKeyConstraint().Clustered = clustered;
		}

		///<summary>
		/// GetForeignConstraintInfo
		/// Parse delle FK
		///</summary>
		//-----------------------------------------------------------------------------
		public void GetForeignConstraintInfo(string constraintName)
		{
			if (Oracle)
				return;

			SqlTable currentTable = tables.GetTableByName(currentTableName);
			if (currentTable == null)
				return;

			currentTable.AddConstraint(constraintName, false);

			string constraintColumnName = string.Empty;
			string constraintRefTableName = string.Empty;

			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case (Token)SQLToken.REFERENCES:
					{
						// mi aspetto i seguenti tokens: 
						// [dbo].[<nome_tabella>] oppure [<nome_tabella>]
						while (!parser.ParseSquareOpen());

						if (ParseTableName(ref constraintRefTableName))
							currentTable.AddConstraintTableReference(constraintName, constraintRefTableName);
						break;
					}

					case Token.SQUAREOPEN:
					{
						if (string.IsNullOrEmpty(constraintRefTableName))
						{
							parser.ParseSquareOpen();
							ParseIDIncludeKeywords(out constraintColumnName);
							parser.ParseSquareClose();
							currentTable.AddConstraintColumn(constraintName, constraintColumnName);
						}
						else
						{
							parser.ParseSquareOpen();
							ParseIDIncludeKeywords(out constraintColumnName);
							parser.ParseSquareClose();
							currentTable.AddConstraintColumnReference(constraintName, constraintColumnName);
						}
						break;
					}

					case (Token)SQLToken.ON:
					{
						
						parser.SkipToken();
						if (parser.LookAhead((Token)SQLToken.DELETE))
						{
							parser.SkipToken();
							if (parser.LookAhead((Token)SQLToken.CASCADE))		
							{
								TableConstraint tc = currentTable.Constraints.GetConstraintByName(constraintName);
									if (tc != null)
										tc.OnDeleteCascade = true;
							}
						}

						else if (parser.LookAhead((Token)SQLToken.UPDATE))
						{
							parser.SkipToken();
							if (parser.LookAhead((Token)SQLToken.CASCADE))
							{
								TableConstraint tc = currentTable.Constraints.GetConstraintByName(constraintName);
								if (tc != null)
									tc.OnUpdateCascade = true;
							}
						}
						else
							parser.SkipToken();

						break;
					}

                    case Token.BEGIN:
                    case Token.END:
                    case (Token)SQLToken.GO:
                    case (Token)SQLToken.CREATE:
                    case (Token)SQLToken.CONSTRAINT:
                    case Token.CLOSE:
					case (Token)SQLToken.PRIMARY:
					case (Token)SQLToken.DECLARE:
					case (Token)SQLToken.INSERT:
						return;

					default:
						parser.SkipToken();
						break;
				}
			}
		}

		///<summary>
		/// ParseColumns
		/// Parsa le colonne nella CREATE TABLE
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public bool ParseColumns()
		{
			SqlTable currentTable = tables.GetTableByName(currentTableName);
			if (currentTable == null)
				return false;
			
			string columnName = string.Empty;
			string columnType = string.Empty;
			int columnLength = 0;
			bool isColumnNullable = true;
			bool isCollateSensitive = true;
            string columnDefaultValue = null;
			string columnDefaultExpression = string.Empty;
			string defaultConstraintName = string.Empty;
			bool isAutoIncrement = false;
			int seed = -1;
			int increment = -1;

			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case (Token)SQLToken.CREATE:
					{
						// per leggere file SQL contenenti più CREATE TABLE
						// invece di skippare tra un GO e l'altro controlliamo se incontriamo una CREATE
						return true;
					}

					case Token.NOT:
					{
						parser.SkipToken();
						if (parser.LookAhead(Token.NULL))
							isColumnNullable = false;
						break;
					}

					case Token.NULL:
					{
						parser.SkipToken();
						break;
					}

					case Token.ID:
					case Token.SQUAREOPEN:
					{
						if (!string.IsNullOrEmpty(columnName))
						{
							columnType = GetSqlType();

							if (
								(string.Compare(columnType, SqlParser.NVarcharFieldDataType, true) == 0) ||
								(string.Compare(columnType, SqlParser.VarcharFieldDataType, true) == 0) ||
								(string.Compare(columnType, SqlParser.CharFieldDataType, true) == 0) ||
								(string.Compare(columnType, SqlParser.NCharFieldDataType, true) == 0))
							{
								parser.LookAhead();
								if (parser.ParseOpen())
								{
									parser.ParseInt(out columnLength);
									parser.ParseClose();
								}
							}
						}
						else
						{
							ParseColumnName(ref columnName);

							// gestione del caso in cui dopo il nome della colonna compare il tipo senza []
							// se il token successivo è [ non procedo (verrà gestito dall'if qui sopra)
							if (parser.LookAhead(Token.SQUAREOPEN))
								break;

							// altrimenti parso il tipo senza []
							Token tokenToParse = parser.LookAhead();

							if (!parser.ParseTag(tokenToParse))
								break;

							columnType = parser.CurrentLexeme;

							if (
								(string.Compare(columnType, SqlParser.NVarcharFieldDataType, true) == 0) ||
								(string.Compare(columnType, SqlParser.VarcharFieldDataType, true) == 0) ||
								(string.Compare(columnType, SqlParser.CharFieldDataType, true) == 0) ||
								(string.Compare(columnType, SqlParser.NCharFieldDataType, true) == 0))
							{
								parser.LookAhead();
								if (parser.ParseOpen())
								{
									parser.ParseInt(out columnLength);
									parser.ParseClose();
								}
							}

						}
						break;
					}

					case (Token)SQLToken.CONSTRAINT:
					{
						if (columnName.Length == 0)
							return true;

						//ultima colonna dello script prima della definizione del constraint 
                        if (!String.IsNullOrEmpty(defaultConstraintName))
						{
							currentTable.AddColumn
							(
							columnName,
							columnType,
							columnLength,
							isColumnNullable,
							columnDefaultValue,
							defaultConstraintName,
							isCollateSensitive,
							isAutoIncrement,
							seed,
							increment,
							columnDefaultExpression);

							columnName = string.Empty;
							columnType = string.Empty;
							isColumnNullable = true;
							columnLength = 0;
                            columnDefaultValue = null;
							columnDefaultExpression = string.Empty;
							defaultConstraintName = string.Empty;
							isCollateSensitive = true;
							isAutoIncrement = false;
							seed = -1;
							increment = -1;
						}
						else
						{
							parser.SkipToken();
							if (parser.LookAhead(Token.SQUAREOPEN))
								parser.SkipToken();
							ParseIDIncludeKeywords(out defaultConstraintName);
							if (parser.LookAhead(Token.SQUARECLOSE))
								parser.SkipToken();
							// contemplato il caso in cui dopo l'ultima colonna manchi la virgola prima
							// della definizione dei CONSTRAINT di PK, FK o UNIQUE (da non confondere con un
							// constraint di DEFAULT!!!)
						}
						break;
					}

					// gestione virgola mancante prima della definizione dei constraints (PK, FK, UNIQUE)
					case (Token)SQLToken.PRIMARY:
					case (Token)SQLToken.FOREIGN:
					case (Token)SQLToken.UNIQUE:
					{
						if (string.IsNullOrEmpty(defaultConstraintName))
						{
							parser.SkipToken();
							break;
						}

						// aggiungo prima la colonna per il problema delle virgole tra i vari constraint
						currentTable.AddColumn
						(
						columnName,
						columnType,
						columnLength,
						isColumnNullable,
						columnDefaultValue,
						string.Empty,
						isCollateSensitive,
						isAutoIncrement,
						seed,
						increment,
						columnDefaultExpression
						);

						if (parser.LookAhead() == (Token)SQLToken.PRIMARY)
							GetPrimaryConstraintInfo(defaultConstraintName);
						else if (parser.LookAhead() == (Token)SQLToken.FOREIGN)
							GetForeignConstraintInfo(defaultConstraintName);
						else if (parser.LookAhead() == (Token)SQLToken.UNIQUE)
							GetIndexConstraintInfo(defaultConstraintName);
						
						columnName = string.Empty;
						columnType = string.Empty;
						isColumnNullable = true;
						columnLength = 0;
                        columnDefaultValue = null;
						columnDefaultExpression = string.Empty;
						defaultConstraintName = string.Empty;
						isCollateSensitive = true;
						isAutoIncrement = false;
						seed = -1;
						increment = -1;

						return true;
					}

					case (Token)SQLToken.DEFAULT:
					{
						//il valore di default DEVE essere compreso tra parentesi tonde, 
						//può contenere qualsiasi cosa (stringhe, numeri, funzioni con parametri...)
						//Il parse se trova una stringa come primo Token la prende e la mette nel valore di default.
						//Se invece trova altri tipi di token, concatena tutto quello che trova fino all'ultima tonda 
						//chiusa e lo valuta, considerando anche il tipo dato della colonna in modo da identificare 
						//i valori di default espressi tramite funzioni o espressi esplicitamente
						//Per es: una colonna di tipo data può contenere nel suo default ('20080101' oppure (GetDate()))
						//Se vengono trovate delle funzioni, o cmq dei valori non riconducibili al tipo di dato dichiarato,
						//viene valorizzato un attributo che contiene il valore del default dichiarato così 
						//com'è nello script sql. Questo perchè i valori di default vengono elaborati 
						//castandoli al tipo della colonna dichiarato e quindi si perderebbe la possibilità 
						// di inserire funzioni, che così vengono preservate
						parser.SkipToken();
						int parCounter = 0;
						string textstring;
						
						if (parser.LookAhead(Token.ROUNDOPEN))
						{
							bool isQuotedString = false;
							parCounter++;
							parser.ParseOpen();
							while (parCounter != 0)
							{
								if (parser.LookAhead(Token.ROUNDOPEN)) 
									parCounter++;
								if (parser.LookAhead(Token.ROUNDCLOSE)) 
									parCounter--;
								if (parser.LookAhead(Token.TEXTSTRING))
								{
									parser.ParseString(out textstring);
									if (parCounter == 1)
									{
										isQuotedString = true;
										columnDefaultValue = textstring;
										textstring = string.Empty;
										break;
									};
									columnDefaultValue += String.Concat("'", textstring, "'");
									textstring = string.Empty;
									continue;
								}
								if (parCounter == 0) break;
								columnDefaultValue += parser.CurrentLexeme;
								parser.SkipToken();
							}
							parser.ParseClose();
							if (!isQuotedString)
								EvaluateDefaultValue(ref columnDefaultValue, out columnDefaultExpression, columnType);
						}
						break;
					}

					case Token.ROUNDCLOSE:
					{
						//nela caso la create della tabella finisce senza creazione di CONSTRAINT 
						//dopo e l'ultima riga di definizione delle colonne non termina con COMMA 
						//viene parsata la parentesi tonda finale, tutte le altre tonde chiuse 
						//vengono parsate nei rispettivi case
						parser.SkipToken();
						if (!String.IsNullOrEmpty(columnName))
						{
							currentTable.AddColumn
							(
							columnName,
							columnType,
							columnLength,
							isColumnNullable,
							columnDefaultValue,
							defaultConstraintName,
							isCollateSensitive,
							isAutoIncrement,
							seed,
							increment,
							columnDefaultExpression);

							columnName = string.Empty;
							columnType = string.Empty;
							isColumnNullable = true;
							columnLength = 0;
                            columnDefaultValue = null;
							columnDefaultExpression = string.Empty;
							defaultConstraintName = string.Empty;
							isCollateSensitive = true;
							isAutoIncrement = false;
							seed = -1;
							increment = -1;
						}
						break;
					}

					case Token.COMMA:
					{
						parser.SkipToken();
						currentTable.AddColumn
							(
							columnName,
							columnType,
							columnLength,
							isColumnNullable,
							columnDefaultValue,
							defaultConstraintName,
							isCollateSensitive,
							isAutoIncrement,
							seed,
							increment,
							columnDefaultExpression);

						columnName = string.Empty;
						columnType = string.Empty;
						isColumnNullable = true;
						columnLength = 0;
						columnDefaultValue = null;
						columnDefaultExpression = string.Empty;
						defaultConstraintName = string.Empty;
						isCollateSensitive = true;
						isAutoIncrement = false;
						seed = -1;
						increment = -1;
						break;
					}

					case (Token)SQLToken.COLLATE:
					{
						string collate = null;
						parser.SkipToken();
						ParseIDIncludeKeywords(out collate);
						isCollateSensitive = String.Compare("Latin1_General_CI_AS", collate, true, CultureInfo.InvariantCulture) != 0;
						break;
					}

					case (Token)SQLToken.IDENTITY:
					{
						isAutoIncrement = true;

						parser.SkipToken();
						if (parser.LookAhead(Token.ROUNDOPEN))
						{
							if (parser.ParseOpen())
							{
								parser.ParseInt(out seed);
								if (parser.ParseComma())
									parser.ParseInt(out increment);
								parser.ParseClose();
							}
							else//eccezione? diagnostica?
								seed = increment = 1; // se fallisce il parse di (seed, increment) li inizializziamo ai valori di default
						}
						else
							seed = increment = 1; // se i due parametri non sono presenti sono inizializzati a 1
						break;
					}

					case (Token)SQLToken.GO:
                    case Token.BEGIN:
					case Token.END:
					case (Token)SQLToken.DECLARE:
					case (Token)SQLToken.INSERT:
						return true;

					default:
					{
						parser.SkipToken();
						break;
					}
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------------------------------
		public void EvaluateDefaultValue(ref string columnDefaultValue, out string columnDefaultExpression, string columnType)
		{
			columnDefaultExpression = null;

			if (String.IsNullOrEmpty(columnDefaultValue) || String.IsNullOrEmpty(columnType))
				return;

			switch (columnType.Trim().ToLower(CultureInfo.InvariantCulture))
			{
				case "char":
				case "nchar":
				case "nvarchar":
				case "varchar":
				case "ntext":
				case "text":
					columnDefaultExpression = columnDefaultValue;
					columnDefaultValue = null;
					break;
					
				case "smallint":
					short mySmallint;
					if (!Int16.TryParse(columnDefaultValue, out mySmallint))
					{
						columnDefaultExpression = columnDefaultValue;
                        columnDefaultValue = null;
					}
					break;

				case "int":
					int myInt;
					if (!Int32.TryParse(columnDefaultValue, out myInt))
					{
						columnDefaultExpression = columnDefaultValue;
                        columnDefaultValue = null;
					}
					break;

				case "float":
					double myDouble;
					if (!double.TryParse(columnDefaultValue, out myDouble))
					{
						columnDefaultExpression = columnDefaultValue;
                        columnDefaultValue = null;
					}
					break;

				case "datetime":
					DateTime myDateTime;
					if (!DateTime.TryParse(columnDefaultValue, out myDateTime))
					{
						columnDefaultExpression = columnDefaultValue;
                        columnDefaultValue = null;
					}
					break;

				case "uniqueidentifier":
					try
					{
						// nel caso in cui nel file sql sia stato indicato il valore di default 0x00 
						// lo skippo, nell'xml non verrà scritto il default_value e, in fase di scrittura dei file sql,
						// l'unparse già sa cosa mettere sia per SQL ("0x00") che per Oracle ('{00000000-0000-0000-0000-000000000000'})
						if (string.Compare(columnDefaultValue, "0x00", StringComparison.InvariantCultureIgnoreCase) == 0)
							break;

						Guid myGuid = new Guid(columnDefaultValue);
					}
					catch 
					{
						columnDefaultExpression = columnDefaultValue;
                        columnDefaultValue = null;
					}
					break;

				case "bit":
					if (columnDefaultValue != "0" && columnDefaultValue != "1")
					{
						columnDefaultExpression = columnDefaultValue;
                        columnDefaultValue = null;
					}
					break;
				default:
					break;
			}
		}

		///<summary>
		/// ParseColumnInfo
		/// Chiamata per le ALTER TABLE da arricchire con le modifiche apportate alla ParseColumn per gestire tutti i case
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public bool ParseColumnInfo(out TableColumn parsedColumn)
		{
			parsedColumn = null;

            string columnName = string.Empty;
            string columnType = string.Empty;
            int columnLength = 0;
            bool isColumnNullable = true;
            bool isCollateSensitive = true;
            string columnDefaultValue = null;
            string columnDefaultExpression = string.Empty;
            string defaultConstraintName = string.Empty;
            bool isAutoIncrement = false;
            int seed = -1;
            int increment = -1;

			while (!parser.Eof)
			{
				Token nextToken = parser.LookAhead();
				
				switch(nextToken)
				{
					case Token.NOT:
					{
						parser.SkipToken();
						if (parser.LookAhead(Token.NULL))
						{
							isColumnNullable = false;
							parser.SkipToken();
						}
						break;
					}

					case Token.NULL:
					{
						parser.SkipToken();
						isColumnNullable = true;
						break;
					}

                    case Token.SQUAREOPEN:
                    {
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            columnType = GetSqlType();

                            if (
                                (string.Compare(columnType, SqlParser.NVarcharFieldDataType, true) == 0) ||
                                (string.Compare(columnType, SqlParser.VarcharFieldDataType, true) == 0) ||
                                (string.Compare(columnType, SqlParser.CharFieldDataType, true) == 0) ||
                                (string.Compare(columnType, SqlParser.NCharFieldDataType, true) == 0))
                            {
                                parser.LookAhead();
                                if (parser.ParseOpen())
                                {
                                    parser.ParseInt(out columnLength);
                                    parser.ParseClose();
                                }
                            }
                        }
                        else
                        {
							ParseColumnName(ref columnName);

                            // gestione del caso in cui dopo il nome della colonna compare il tipo senza []
                            // se il token successivo è [ non procedo (verrà gestito dall'if qui sopra)
                            if (parser.LookAhead(Token.SQUAREOPEN))
                                break;

                            // altrimenti parso il tipo senza []
                            Token tokenToParse = parser.LookAhead();

                            if (!parser.ParseTag(tokenToParse))
                                break;

                            columnType = parser.CurrentLexeme;

                            if (
                                (string.Compare(columnType, SqlParser.NVarcharFieldDataType, true) == 0) ||
                                (string.Compare(columnType, SqlParser.VarcharFieldDataType, true) == 0) ||
                                (string.Compare(columnType, SqlParser.CharFieldDataType, true) == 0) ||
                                (string.Compare(columnType, SqlParser.NCharFieldDataType, true) == 0))
                            {
                                parser.LookAhead();
                                if (parser.ParseOpen())
                                {
                                    parser.ParseInt(out columnLength);
                                    parser.ParseClose();
                                }
                            }
                        }
                        break;
                    }
					
					case (Token)SQLToken.CONSTRAINT:
					{
						if (columnName.Length == 0)
							return false;

						if (!String.IsNullOrEmpty(defaultConstraintName))
						{
							parsedColumn = 
								new TableColumn(columnName, columnType, columnLength, isColumnNullable, columnDefaultValue, defaultConstraintName);

							parsedColumn.DefaultExpressionValue = columnDefaultExpression;
							parsedColumn.IsAutoIncrement = isAutoIncrement;
							parsedColumn.Seed = seed;
							parsedColumn.Increment = increment;
							parsedColumn.IsCollateSensitive = isCollateSensitive;

							columnName = string.Empty;
							columnType = string.Empty;
							isColumnNullable = true;
							columnLength = 0;
							columnDefaultValue = null;
							columnDefaultExpression = string.Empty;
							defaultConstraintName = string.Empty;
							isCollateSensitive = true;
							isAutoIncrement = false;
							seed = -1;
							increment = -1;

							return true;
						}

						parser.SkipToken();
						if (parser.LookAhead(Token.SQUAREOPEN))
							parser.SkipToken();
						ParseIDIncludeKeywords(out defaultConstraintName);
						if (parser.LookAhead(Token.SQUARECLOSE))
							parser.SkipToken();

						break;
					}

                    case (Token)SQLToken.COLLATE:
                    {
                        string collate = null;
                        parser.SkipToken();
						ParseIDIncludeKeywords(out collate);
                        isCollateSensitive = String.Compare("Latin1_General_CI_AS", collate, true, CultureInfo.InvariantCulture) != 0;
                        break;
                    }

                    case (Token)SQLToken.IDENTITY:
                    {
                        isAutoIncrement = true;

                        parser.SkipToken();
                        if (parser.LookAhead(Token.ROUNDOPEN))
                        {
                            if (parser.ParseOpen())
                            {
                                parser.ParseInt(out seed);
                                if (parser.ParseComma())
                                    parser.ParseInt(out increment);
                                parser.ParseClose();
                            }
                            else//eccezione? diagnostica?
                                seed = increment = 1; // se fallisce il parse di (seed, increment) li inizializziamo ai valori di default
                        }
                        else
                            seed = increment = 1; // se i due parametri non sono presenti sono inizializzati a 1
                        break;
                    }

                    case (Token)SQLToken.DEFAULT:
                    {
                        //il valore di default DEVE essere compreso tra parentesi tonde, 
                        //può contenere qualsiasi cosa 
                        //(stringhe, numeri, funzioni con parametri...)
                        //Il parse se trova una stringa come primo Token 
                        //la prende e la mette nel valore di default.
                        //Se invece trova altri tipi di token, 
                        //concatena tutto quello che trova fino all'ultima tonda chiusa
                        //e lo valuta, considerando anche il tipo dato della colonna
                        //in modo da identificare i valori di default 
                        //espressi tramite funzioni o espressi esplicitamente
                        //esempio, una colonna di tipo data 
                        //può contenere nel suo default ('20080101' oppure (GetDate()))
                        //Se vengono trovate delle funzioni, 
                        //o comunque dei valori non riconducibili al tipo di dato dichiarato,
                        //viene valorizzato un attributo che contiene 
                        //il valore del default dichiarato così com'è nelllo script sql
                        //Questo perchè i valori di default vengono elaborati 
                        //castandoli al tipo della colonna dichiarato 
                        //e quindi si perderebbe la possibilità di inserire funzioni, 
                        //che così vengono preservate
                        parser.SkipToken();
                        int parCounter = 0;
                        string textstring;
                        if (parser.LookAhead(Token.ROUNDOPEN))
                        {
                            bool isQuotedString = false;
                            parCounter++;
                            parser.ParseOpen();
                            while (parCounter != 0)
                            {
                                if (parser.LookAhead(Token.ROUNDOPEN))
                                    parCounter++;
                                if (parser.LookAhead(Token.ROUNDCLOSE))
                                    parCounter--;
                                if (parser.LookAhead(Token.TEXTSTRING))
                                {
                                    parser.ParseString(out textstring);
                                    if (parCounter == 1)
                                    {
                                        isQuotedString = true;
                                        columnDefaultValue = textstring;
                                        textstring = string.Empty;
                                        break;
                                    };
                                    columnDefaultValue += String.Concat("'", textstring, "'");
                                    textstring = string.Empty;
                                    continue;
                                }
                                if (parCounter == 0) break;
                                columnDefaultValue += parser.CurrentLexeme;
                                parser.SkipToken();
                            }
                            parser.ParseClose();

                            if (!isQuotedString)
                                EvaluateDefaultValue(ref columnDefaultValue, out columnDefaultExpression, columnType);
                        }
                        break;
                    }

					default:
					{
						if (columnName.Length == 0)
							return false;

						parsedColumn = 
							new TableColumn(columnName, columnType, columnLength, isColumnNullable, columnDefaultValue, defaultConstraintName);

						parsedColumn.DefaultExpressionValue = columnDefaultExpression;
						parsedColumn.IsAutoIncrement = isAutoIncrement;
						parsedColumn.Seed = seed;
						parsedColumn.Increment = increment;
						parsedColumn.IsCollateSensitive = isCollateSensitive;

						columnName = string.Empty;
						columnType = string.Empty;
						isColumnNullable = true;
						columnLength = 0;
						columnDefaultValue = null;
						columnDefaultExpression = string.Empty;
						defaultConstraintName = string.Empty;
						isCollateSensitive = true;
						isAutoIncrement = false;
						seed = -1;
						increment = -1;

						return true;
					}
				}
			}

			return false;
		}

		//---------------------------------------------------------------------------------------------------
		private string GetSqlType()
		{
			if (parser == null)
				return String.Empty;

			if (!parser.ParseSquareOpen())
				return String.Empty;

			Token tokenToParse = parser.LookAhead();

			if (!parser.ParseTag(tokenToParse))
				return String.Empty;

			string typeString = parser.CurrentLexeme;

			parser.ParseSquareClose();

			return typeString;
		}

		///<summary>
		/// ParseIDIncludeKeywords
		/// Visto che il Lexan "esclude" a priori tutta la serie di parole riservate per woorm, 
		/// in questi casi la ParseID ritorna false.
		/// In questo modo ritorniamo forzatamente il currentLexeme, se rientra tra i token riservati
		///</summary>
		//---------------------------------------------------------------------------------------------------
		private bool ParseIDIncludeKeywords(out string parsedString)
		{
			parsedString = string.Empty;

			if (!parser.ParseID(out parsedString)) // si mangia già il chr, non serve poi lo skip
			{
				// se è fallita la ParseID potrebbe darsi che si è incontrata una parola chiave di woorm
				// pertanto se il CurrentLexeme contiene qualcosa, 
				// provo a cercarlo nelle keywords del lexan e se corrisponde ad un token specifico 
				// lo ritorno forzatamente
				if (!string.IsNullOrEmpty(parser.CurrentLexeme))
				{
					Token currToken = Language.GetKeywordsToken(parser.CurrentLexeme);
					if (currToken != Token.NOTOKEN)
						parsedString = parser.CurrentLexeme;
				}
				else
					return false;
			}

			return true;
		}

		///<summary>
		/// GetSqlTableName
		/// Ritorna il nome della tabella aspettando i seguenti tokens:
		/// [dbo].[<nome_tabella>] oppure [<nome_tabella>]
		///</summary>
		//---------------------------------------------------------------------------------------------------
		public bool GetSqlTableName(out string tableName)
		{
			// mi aspetto i seguenti tokens: [dbo].[<nome_tabella>] oppure [<nome_tabella>]
			tableName = string.Empty;
			for (int i = 0; i < 2; i++)
			{
				if (parser.LookAhead(Token.SQUAREOPEN))
					parser.SkipToken();
				parser.ParseID(out tableName); // si mangia già il chr, non serve poi lo skip				
				if (parser.LookAhead(Token.SQUARECLOSE))
					parser.SkipToken();
				if (!parser.LookAhead((Token)SQLToken.DOT))
					return true;
				parser.SkipToken();
			}

			return true;
		}

		//---------------------------------------------------------------------------------------------------
		public bool GetOracleTableName(out string tableName)
		{
			// mi aspetto i seguenti tokens: 
			// "<nome_tabella>"	(per oracle)
			tableName = string.Empty;
			
			return parser.ParseString(out tableName);
		}

		//---------------------------------------------------------------------------------------------------
		private void InitParser()
		{
			parser.UserKeywords.Add("TABLE",						SQLToken.TABLE);
			parser.UserKeywords.Add("CREATE",						SQLToken.CREATE);
			parser.UserKeywords.Add("dbo",							SQLToken.DBO);
			parser.UserKeywords.Add("CONSTRAINT",					SQLToken.CONSTRAINT);
			parser.UserKeywords.Add("PRIMARY",						SQLToken.PRIMARY);
			parser.UserKeywords.Add("FOREIGN",						SQLToken.FOREIGN);
			parser.UserKeywords.Add("KEY",							SQLToken.KEY);
			parser.UserKeywords.Add("REFERENCES",					SQLToken.REFERENCES);
			parser.UserKeywords.Add("object_id",					SQLToken.OBJECT_ID);
			parser.UserKeywords.Add("ALTER",						SQLToken.ALTER);
			parser.UserKeywords.Add("GO",							SQLToken.GO);
			parser.UserKeywords.Add("ADD",							SQLToken.ADD);
			parser.UserKeywords.Add("END",							SQLToken.END);
			parser.UserKeywords.Add("DELETE",						SQLToken.DELETE);
			parser.UserKeywords.Add("FROM",							SQLToken.FROM);
			parser.UserKeywords.Add("INSERT",						SQLToken.INSERT);
			parser.UserKeywords.Add("UPDATE",						SQLToken.UPDATE);
			parser.UserKeywords.Add("INTO",							SQLToken.INTO);
			parser.UserKeywords.Add(".",							SQLToken.DOT);
			parser.UserKeywords.Add("INDEX",						SQLToken.INDEX);
			parser.UserKeywords.Add("ON",							SQLToken.ON);
			parser.UserKeywords.Add("DROP",							SQLToken.DROP);
			parser.UserKeywords.Add("DESCRIPTION",					Token.ID);
			parser.UserKeywords.Add("PATH",							Token.ID);
			parser.UserKeywords.Add("Exec",							SQLToken.EXEC);
			parser.UserKeywords.Add("VIEW",							SQLToken.VIEW);
			parser.UserKeywords.Add("OR",							SQLToken.OR);
			parser.UserKeywords.Add("REPLACE",						SQLToken.REPLACE);
			parser.UserKeywords.Add("AS",							SQLToken.AS);
			parser.UserKeywords.Add("DEFAULT",						SQLToken.DEFAULT);
			parser.UserKeywords.Add("dbo.syscolumns.name",			SQLToken.SYSCOLUMNS);
			parser.UserKeywords.Add("COLLATE",						SQLToken.COLLATE);
			parser.UserKeywords.Add("IDENTITY",						SQLToken.IDENTITY);
			parser.UserKeywords.Add("UNIQUE",						SQLToken.UNIQUE);
			parser.UserKeywords.Add("CLUSTERED",					SQLToken.CLUSTERED);
			parser.UserKeywords.Add("NONCLUSTERED",					SQLToken.NONCLUSTERED);
            parser.UserKeywords.Add("DECLARE",                      SQLToken.DECLARE);
			parser.UserKeywords.Add("NOCHECK",						SQLToken.NOCHECK);
			parser.UserKeywords.Add("CASCADE",						SQLToken.CASCADE);
			parser.UserKeywords.Add(SqlParser.CharFieldDataType,	SQLToken.CHAR_DATATYPE);
			parser.UserKeywords.Add(SqlParser.NCharFieldDataType,	SQLToken.NCHAR_DATATYPE);
			parser.UserKeywords.Add(SqlParser.VarcharFieldDataType, SQLToken.VARCHAR_DATATYPE);
			parser.UserKeywords.Add(SqlParser.NVarcharFieldDataType,SQLToken.NVARCHAR_DATATYPE);
			parser.UserKeywords.Add(SqlParser.SmallIntFieldDataType,SQLToken.SMALLINT_DATATYPE);
			parser.UserKeywords.Add(SqlParser.IntFieldDataType,		SQLToken.INT_DATATYPE);
			parser.UserKeywords.Add(SqlParser.FloatFieldDataType,	SQLToken.FLOAT_DATATYPE);
			parser.UserKeywords.Add(SqlParser.TextFieldDataType,	SQLToken.TEXT_DATATYPE);
			parser.UserKeywords.Add(SqlParser.NTextFieldDataType,	SQLToken.NTEXT_DATATYPE);
			parser.UserKeywords.Add(SqlParser.DateTimeFieldDataType,SQLToken.DATETIME_DATATYPE);
			parser.UserKeywords.Add(SqlParser.UniqueIdFieldDataType,SQLToken.UNIQUEID_DATATYPE);
		}
	}
}