using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Microarea.Library.SqlScriptUtility
{
	#region Classi che gestiscono il parsing del file .sql

	public class CUId
	{
		private string name = string.Empty;
		private int counter = 0;

		//-----------------------------------------------------------------------------
		public CUId(string aName)
		{
			name = aName;
			counter = 0;
		}

		//-----------------------------------------------------------------------------
		public int Increment()
		{
			return ++counter;
		}

		//-----------------------------------------------------------------------------
		public string Name { get { return name; } }
	}


	public class CUIdList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public CUIdList ()
		{
		}

		public string Add(string aName)
		{
			if (!Exist(aName))
			{
				base.Add(new CUId(aName));
				return string.Format("{0}_00", aName);
			}
			else
				foreach (CUId tUid in this)
				{
					if (String.Compare(aName, tUid.Name, true) == 0)
					{
						int i = tUid.Increment();
						string cnt = string.Empty;
						if (i < 10)
							cnt = string.Format("0{0}", i.ToString());
						else cnt = i.ToString();
						return string.Format("{0}_{1}", aName, cnt);
					}
				}
			return string.Empty;
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string aName)
		{
			foreach (CUId tUid in this)
			{
				if (String.Compare(aName, tUid.Name, true) == 0)
					return true;
			}
			return false;
		}
	}

    //=========================================================================
	public class SqlParser
	{
		private CUIdList uid = new CUIdList();
		protected SqlTableList tables = new SqlTableList();
        protected StringCollection viewNames = new StringCollection();
        protected StringCollection procedureNames = new StringCollection();
		protected ArrayList errorsList = new ArrayList();

		#region SqlParser public constants

		public const string CharFieldDataType		= "char";
		public const string NCharFieldDataType		= "nchar";
		public const string VarcharFieldDataType	= "varchar";
		public const string NVarcharFieldDataType	= "nvarchar";
		public const string SmallIntFieldDataType	= "smallint";
		public const string IntFieldDataType		= "int";
		public const string FloatFieldDataType		= "float";
		public const string TextFieldDataType		= "text";
		public const string NTextFieldDataType		= "ntext";
		public const string DateTimeFieldDataType	= "datetime";
		public const string UniqueIdFieldDataType	= "uniqueidentifier";

		#endregion // SqlParser public constants

		public delegate void WriteTextEventHandler(string name, string txt, FileMode mode);
		public virtual event WriteTextEventHandler OnWriteText;

		public delegate string ReadTextEventHandler(string name, bool getLatest);
		public virtual event ReadTextEventHandler OnReadText;

        public StringCollection ViewNames { get { return viewNames; } }
        public StringCollection ProcedureNames { get { return procedureNames; } }
		
        #region SqlParser private methods
		
		//-----------------------------------------------------------------------------
		private string SqlFileRead(string sqlFile)
		{
			string sqlFileText = string.Empty;
			try
			{
				if (OnReadText != null)
					sqlFileText = OnReadText(sqlFile, true);
				else
				{
					FileStream fr = new FileStream(sqlFile, FileMode.Open, FileAccess.Read);
					
					for (int i = 1; i<=(int)fr.Length; i++)
						sqlFileText += (char)fr.ReadByte();
					fr.Close();
				}

				sqlFileText = sqlFileText.Replace("[dbo].", " ");
				sqlFileText = sqlFileText.Replace("\t", " ");
				sqlFileText = sqlFileText.Replace("[", " ");
				sqlFileText = sqlFileText.Replace("]", " ");
				sqlFileText = sqlFileText.Replace("(", " (");
				sqlFileText = sqlFileText.Replace(")", ") ");
				sqlFileText = sqlFileText.Replace(",", " , ");

				return sqlFileText;
			}
			catch (Exception ex)
			{
				errorsList.Add(ex.Message);
				return string.Empty;
			}
		}

		//-----------------------------------------------------------------------------
		private string OracleFileRead(string sqlFile)
		{
			string sqlFileText = string.Empty;
			try
			{
				if (OnReadText != null)
					sqlFileText = OnReadText(sqlFile, true);
				else
				{
					FileStream fr = new FileStream(sqlFile, FileMode.Open, FileAccess.Read);
					
					for (int i = 1; i<=(int)fr.Length; i++)
						sqlFileText += (char)fr.ReadByte();
					fr.Close();
				}

				sqlFileText = sqlFileText.Replace("\t", " ");
				sqlFileText = sqlFileText.Replace("\"", " ");
				sqlFileText = sqlFileText.Replace("(", " (");
				sqlFileText = sqlFileText.Replace(")", ") ");
				sqlFileText = sqlFileText.Replace(",", " , ");

				return sqlFileText;
			}
			catch (Exception ex)
			{
				errorsList.Add(ex.Message);
				return string.Empty;
			}
		}

		//-----------------------------------------------------------------------------
		private string MakeCreateTableOracle(string tableName)
		{
			return string.Format("CREATE TABLE \"{0}\" ( \r\n", tableName.ToUpper());
		}

		//-----------------------------------------------------------------------------
		private string MakeConstraintOracle(string aConstraintName, bool isPrimary)
		{
			if (isPrimary)
			{
				return string.Format("   CONSTRAINT \"{0}\" PRIMARY KEY\r\n", aConstraintName.ToUpper());
			}
			else
			{
				return string.Format("   CONSTRAINT \"{0}\" FOREIGN KEY\r\n", aConstraintName.ToUpper());
			}
		}

		//-----------------------------------------------------------------------------
		private string MakeIndexesOracle(TableIndex aIndex)
		{
			string returnString = string.Empty;
			
			returnString = string.Format("CREATE INDEX \"{0}\" ON \"{1}\" (", aIndex.Name.ToUpper(), aIndex.Table.ToUpper());
			foreach (TableColumn indiceColonna in aIndex.Columns)
			{
				if (aIndex.Columns.IndexOf(indiceColonna) == aIndex.Columns.Count - 1)
					returnString += string.Format("\"{0}\")\r\n", indiceColonna.Name.ToUpper());
				else
					returnString += string.Format("\"{0}\", ", indiceColonna.Name.ToUpper());
			}
			returnString += string.Format("GO\r\n\r\n");

			return returnString;
		}

		//-----------------------------------------------------------------------------
		private bool OracleFileWrite(string oracleFile, string oracleFileText)
		{
			if (OnWriteText != null)
			{
				OnWriteText(oracleFile, oracleFileText, FileMode.Create);
				return true;
			}

			char[] oracleFileTextArray = oracleFileText.ToCharArray();

			try
			{
				if (File.Exists(oracleFile))
				{
					File.Delete(oracleFile);
				}

				int lenPath = oracleFile.LastIndexOf("\\");
				string oracleDiractory = oracleFile.Substring(0, lenPath);
				if (!Directory.Exists(oracleDiractory))
				{
					Directory.CreateDirectory(oracleDiractory);
				}

				FileStream fw = new FileStream(oracleFile, FileMode.Append, FileAccess.Write);
				for (int i = 1; i<=oracleFileText.Length; i++)
					fw.WriteByte((byte) oracleFileTextArray[i-1]);
				fw.Close();
				return true;
			}
			catch (Exception ex)
			{
				errorsList.Add(ex.Message);
				return false;
			}
		}

		//-----------------------------------------------------------------------------
		private string MakeCreateTableSql(string aTableName)
		{
			string res = string.Empty;
			res = string.Format("if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n",aTableName);
			res+= string.Format(" BEGIN\r\n");
			res+= string.Format("CREATE TABLE [dbo].[{0}] (\r\n", aTableName);
			return res;
		}
		
		//-----------------------------------------------------------------------------
		private string MakeConstraintSql(string aConstraintName, bool isPrimary)
		{
			if (isPrimary)
			{
				return string.Format("   CONSTRAINT [{0}] PRIMARY KEY NONCLUSTERED \r\n", aConstraintName);
			}
			else
			{
				return string.Format("   CONSTRAINT [{0}] FOREIGN KEY\r\n", aConstraintName);
			}
		}

		//-----------------------------------------------------------------------------
		private string MakeIndexesSql(TableIndex indice)
		{
			string returnString = string.Empty;
			
			returnString = string.Format("CREATE INDEX [{0}] ON [dbo].[{1}] (", indice.Name, indice.Table);
			foreach (TableColumn indiceColonna in indice.Columns)
			{
				if (indice.Columns.IndexOf(indiceColonna) == indice.Columns.Count - 1)
					returnString += string.Format("[{0}]) ON [PRIMARY]\r\n", indiceColonna.Name);
				else
					returnString += string.Format("[{0}], ", indiceColonna.Name);
			}

			return returnString;
		}

		//-----------------------------------------------------------------------------
		private bool SqlFileWrite(string sqlFile, string sqlFileText)
		{
			if (OnWriteText != null)
			{
				OnWriteText(sqlFile, sqlFileText, FileMode.Create);
				return true;
			}

			char[] sqlFileTextArray = sqlFileText.ToCharArray();

			try
			{
				if (File.Exists(sqlFile))
				{
					File.Delete(sqlFile);
				}

				int lenPath = sqlFile.LastIndexOf("\\");
				string sqlDiractory = sqlFile.Substring(0, lenPath);
				if (!Directory.Exists(sqlDiractory))
				{
					Directory.CreateDirectory(sqlDiractory);
				}

				FileStream fw = new FileStream(sqlFile, FileMode.Append, FileAccess.Write);
				for (int i = 1; i<=sqlFileText.Length; i++)
					fw.WriteByte((byte) sqlFileTextArray[i-1]);
				fw.Close();
				return true;
			}
			catch (Exception ex)
			{
				errorsList.Add(ex.Message);
				return false;
			}
		}

		#endregion // SqlParser private methods
		
		#region SqlParser protected methods
		
		//-----------------------------------------------------------------------------
		protected string MakeInsertColumnOracle(string aColumnName, string colummType, int columnLen, bool nullable, string columnDef, string columnDefConstName)
		{
			string null_default = string.Empty;
			if (nullable)
			{
				if (columnDef != string.Empty)
					null_default = InterpretaDefault(colummType.ToLower(), columnDef, columnDefConstName.ToUpper());
			}
			else
				null_default = "NOT NULL";

			switch (colummType.ToLower())
			{
				case SmallIntFieldDataType:
				{
					return string.Format("    \"{0}\" NUMBER(6) {1}", aColumnName.ToUpper(), null_default);
				}
				case IntFieldDataType:
				{
					return string.Format("    \"{0}\" NUMBER(10) {1}", aColumnName.ToUpper(), null_default);
				}
				case FloatFieldDataType:
				{
					return string.Format("    \"{0}\" FLOAT(126) {1}", aColumnName.ToUpper(), null_default);
				}
				case CharFieldDataType:
				{
					return string.Format("    \"{0}\" CHAR({1}) {2}", aColumnName.ToUpper(), columnLen.ToString(), null_default);
				}
				case NCharFieldDataType:
				{
					return string.Format("    \"{0}\" NCHAR({1}) {2}", aColumnName.ToUpper(), columnLen.ToString(), null_default);
				}
				case VarcharFieldDataType:
				{
					return string.Format("    \"{0}\" VARCHAR2({1}) {2}", aColumnName.ToUpper(), columnLen.ToString(), null_default);
				}
				case NVarcharFieldDataType:
				{
					return string.Format("    \"{0}\" NVARCHAR2({1}) {2}", aColumnName.ToUpper(), columnLen.ToString(), null_default);
				}
				case UniqueIdFieldDataType:
				{
					return string.Format("    \"{0}\" CHAR(38) {1}", aColumnName.ToUpper(), null_default);
				}
				case TextFieldDataType:
				{
					return string.Format("    \"{0}\" CLOB {1}", aColumnName.ToUpper(), null_default);
				}
				case NTextFieldDataType:
				{
					return string.Format("    \"{0}\" NCLOB {1}", aColumnName.ToUpper(), null_default);
				}
				case DateTimeFieldDataType:
				{
					return string.Format("    \"{0}\" DATE {1}", aColumnName.ToUpper(), null_default);
				}
			}
			return string.Empty;
		}

		//-----------------------------------------------------------------------------
		protected string InterpretaDefault(string tipo, string def, string conName)
		{
			switch (tipo)
			{
				case UniqueIdFieldDataType:
				{
					return "DEFAULT('{00000000-0000-0000-0000-000000000000}')";
				}
				case DateTimeFieldDataType:
				{
					try
					{
						string gg = def.Substring(7, 2);
						string mm = def.Substring(5, 2);
						string aaaa = def.Substring(1, 4);
						return string.Format("DEFAULT (TO_DATE('{0}-{1}-{2}','DD-MM-YYYY'))", gg, mm, aaaa);
					}
					catch (Exception e)
					{
						errorsList.Add(e.Message);
						return string.Empty;
					}
				}
				default:
				{
					return string.Format("DEFAULT({0})", def);
				}
			}
		}

		//-----------------------------------------------------------------------------
		protected string MakeInsertColumnSql(string aColumnName, string colummType, int columnLen, bool nullable, string columnDef, string columnDefConstName)
		{
			string null_default = string.Empty;
			if (nullable)
			{
				if (columnDef != string.Empty)
					null_default = string.Format("NULL CONSTRAINT {0} DEFAULT({1})", columnDefConstName, columnDef);
			}
			else
				null_default = "NOT NULL";

			switch (colummType.ToLower())
			{
				case SmallIntFieldDataType:
				{
					return string.Format("    [{0}] [smallint] {1}", aColumnName, null_default);
				}
				case IntFieldDataType:
				{
					return string.Format("    [{0}] [int] {1}", aColumnName, null_default);
				}
				case FloatFieldDataType:
				{
					return string.Format("    [{0}] [float] {1}", aColumnName, null_default);
				}
				case CharFieldDataType:
				{
					return string.Format("    [{0}] [char] ({1}) {2}", aColumnName, columnLen.ToString(), null_default);
				}
				case NCharFieldDataType:
				{
					return string.Format("    [{0}] [nchar] ({1}) {2}", aColumnName, columnLen.ToString(), null_default);
				}
				case VarcharFieldDataType:
				{
					return string.Format("    [{0}] [varchar] ({1}) {2}", aColumnName, columnLen, null_default);
				}
				case NVarcharFieldDataType:
				{
					return string.Format("    [{0}] [nvarchar] ({1}) {2}", aColumnName, columnLen, null_default);
				}
				case UniqueIdFieldDataType:
				{
					return string.Format("    [{0}] [uniqueidentifier] {1}", aColumnName, null_default);
				}
				case TextFieldDataType:
				{
					return string.Format("    [{0}] [text] {1}", aColumnName, null_default);
				}
				case NTextFieldDataType:
				{
					return string.Format("    [{0}] [ntext] {1}", aColumnName, null_default);
				}
				case DateTimeFieldDataType:
				{
					return string.Format("    [{0}] [datetime] {1}", aColumnName, null_default);
				}
			}
			return string.Empty;
		}

		#region SqlParser protected virtual methods

		#region Parse

		//-----------------------------------------------------------------------------
		public virtual bool ParseOracle(string fileName)
		{
			string sqlFileText = OracleFileRead(fileName);
			if (sqlFileText == null || sqlFileText.Length == 0 || IsView(sqlFileText))
				return false;

			int parserState = 1;

			string tableName = string.Empty;
			string columnName = string.Empty;
			string constraintName = string.Empty;
			bool isPrimary = true;
			bool isCommentToFind = false;

			SqlTable currentTable = null;
			TableConstraintList oracleConstraints = null;

			StringReader sqlScriptStringReader = new StringReader(sqlFileText);
			string sqlScriptLine = string.Empty;

			while (true) 
			{
				sqlScriptLine = sqlScriptStringReader.ReadLine();
				if (sqlScriptLine == null) 
				{
					break;
				}

				sqlScriptLine = sqlScriptLine.Trim();
				
				while(sqlScriptLine != string.Empty)
				{
					if (sqlScriptLine.Length >= 2)
					{
						if (sqlScriptLine.Substring(0,2) == "--")
						{
							sqlScriptLine = string.Empty;
							continue;
						}

						isCommentToFind = isCommentToFind || sqlScriptLine.StartsWith("/*");

						if (isCommentToFind)
						{
							int endCommentIndex = sqlScriptLine.IndexOf("*/", 2);
							if (endCommentIndex >= 0)
							{
								sqlScriptLine = sqlScriptLine.Remove(0, endCommentIndex + 2);
								isCommentToFind = false;
							}
							else
								sqlScriptLine = string.Empty;
						}
					}

					if (sqlScriptLine.Length == 0)
						continue;

					switch(parserState)
					{
						case 1: //Cerco TABLE
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "TABLE", true) == 0)
							{
								parserState = 2;
							}

							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 2: //Cerco il nome della tabella
						{
							tableName = sqlScriptLine.Split(' ')[0];
							sqlScriptLine = sqlScriptLine.Substring(tableName.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							
							currentTable = tables.GetTableByOracleName(tableName);
							if (currentTable == null)
							{
								errorsList.Add(string.Format(Strings.MissingTableDefinitionErrorMessage, tableName));
								parserState = 1;
							}
							else
								parserState = 3;
							
							break;
						}
						case 3: //Cerco (
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "(", true) == 0)
							{
								parserState = 4;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 4: //Cerco la parola chiave CONSTRAINT o GO
						{
							columnName = sqlScriptLine.Split(' ')[0];
							
							sqlScriptLine = sqlScriptLine.Substring(columnName.Length);
							sqlScriptLine = sqlScriptLine.Trim();

							if (string.Compare(columnName, "CONSTRAINT", true) == 0)
							{
								columnName = string.Empty;
								parserState = 9;
							}
							else
							{
								if (string.Compare(columnName, "GO", true) == 0)
								{
									columnName = string.Empty;
									parserState = 1;
								}
							}
							
							break;
						}
						case 9: //Cerco il nome del CONSTRAINT (di chiave)
						{
							constraintName = sqlScriptLine.Split(' ')[0];
							sqlScriptLine = sqlScriptLine.Substring(constraintName.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							parserState = 10;
							break;
						}
						case 10: //Cerco il tipo del CONSTRAINT
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "PRIMARY", true) == 0)
							{
								oracleConstraints = new TableConstraintList();
								isPrimary = true;
							}
							else
								isPrimary = false;
							oracleConstraints.Add(constraintName, isPrimary);
							
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							parserState = 11;
							break;
						}
						case 11: //Testo i constraints e reinizializzo tutto
						{
							if (oracleConstraints.Count > currentTable.Constraints.Count)
							{
								errorsList.Add(Strings.DifferentConstraintsNumberErrorMessage);
								currentTable = null;
								oracleConstraints.Clear();
								oracleConstraints = null;
							}
							else
							{
								for (int idx = 0; idx < oracleConstraints.Count; idx ++)
								{
									TableConstraint truncatedConstraint = (TableConstraint) currentTable.Constraints[idx];
									TableConstraint parsedConstraint = (TableConstraint) oracleConstraints[idx];
									if (truncatedConstraint.Name.ToUpper() != parsedConstraint.ExtendedName.ToUpper())
										truncatedConstraint.Name = parsedConstraint.ExtendedName;
									
									truncatedConstraint.IsOracleScriptApproved = true;
								}
							}
							parserState = 1;
							break;
						}
					}
				}
			}
			return true;
		}

		#endregion // Parse

		#region Unparse

		//-----------------------------------------------------------------------------
		protected virtual void OnFileWrite(string name, string txt, FileMode mode)
		{
			if (OnWriteText != null)
				OnWriteText(name, txt, mode);
		}

		//-----------------------------------------------------------------------------
		public virtual bool UnParseAll(string sqlFileName, string oracleFileName)
		{
			return UnParseSql(sqlFileName) && UnParseOracle(oracleFileName);
		}

		//-----------------------------------------------------------------------------
		protected virtual void UnParseSql(out string unparsedText)
		{
			unparsedText = string.Empty;
			foreach (SqlTable aTable in tables)
			{
				unparsedText += MakeCreateTableSql(aTable.ExtendedName);
				foreach (TableColumn aColumn in aTable.Columns)
				{
					unparsedText += MakeInsertColumnSql(aColumn.Name, aColumn.DataType, aColumn.DataLength, aColumn.IsNullable, aColumn.DefaultValue, aColumn.DefaultConstraintName) + ",\r\n";
				}
				foreach (TableConstraint constraint in aTable.Constraints)
				{
					unparsedText += MakeConstraintSql(constraint.ExtendedName, constraint.IsPrimaryKeyConstraint);
					unparsedText += string.Format("    (\r\n");
					foreach (TableColumn aColumn in constraint.Columns)
					{
						if (constraint.Columns.IndexOf(aColumn) == constraint.Columns.Count - 1)
							unparsedText += string.Format("        [{0}]\r\n", aColumn.Name);
						else
							unparsedText += string.Format("        [{0}],\r\n", aColumn.Name);
					}
					if (constraint.IsPrimaryKeyConstraint)
						if (aTable.Constraints.IndexOf(constraint) < aTable.Constraints.Count - 1)
							unparsedText += string.Format("    ) ON [PRIMARY],\r\n");
						else
							unparsedText += string.Format("    ) ON [PRIMARY]\r\n");
					else
					{
						unparsedText += string.Format("    ) REFERENCES [dbo].[{0}] (\r\n", constraint.ReferenceTableName);
						foreach (string rifColonna in constraint.ReferenceColumns)
						{
							if (constraint.ReferenceColumns.IndexOf(rifColonna) == constraint.ReferenceColumns.Count - 1)
								unparsedText += string.Format("        [{0}]\r\n", rifColonna);
							else
								unparsedText += string.Format("        [{0}],\r\n", rifColonna);
						}
						if (aTable.Constraints.IndexOf(constraint) < aTable.Constraints.Count - 1)
							unparsedText += string.Format("    ),\r\n");
						else
							unparsedText += string.Format("    )\r\n");
					}
				}
				unparsedText += string.Format(") ON [PRIMARY]\r\n\r\n");
				foreach (TableIndex aIndex in aTable.Indexes)
				{
					unparsedText += MakeIndexesSql(aIndex);
				}

				
				unparsedText += string.Format("\r\nEND\r\n");
				unparsedText += string.Format("GO\r\n\r\n");
			}
		}

		//-----------------------------------------------------------------------------
		protected virtual bool UnParseOracle(out string unparsedText)
		{
			unparsedText = string.Empty;

			if (tables == null || tables.Count == 0)
				return true;

			foreach (SqlTable aTable in tables)
			{
				unparsedText += MakeCreateTableOracle(aTable.ExtendedName);
				
				if (aTable.Columns != null && aTable.Columns.Count > 0)
				{
					foreach (TableColumn aColumn in aTable.Columns)
						unparsedText += MakeInsertColumnOracle(aColumn.Name, aColumn.DataType, aColumn.DataLength, aColumn.IsNullable, aColumn.DefaultValue, aColumn.DefaultConstraintName) + ",\r\n";
				}

				if (aTable.Constraints != null && aTable.Constraints.Count > 0)
				{
					foreach (TableConstraint constraint in aTable.Constraints)
					{
						if (constraint.Columns.Count == 0)
							continue;

						unparsedText += MakeConstraintOracle(constraint.Name, constraint.IsPrimaryKeyConstraint);
						unparsedText += string.Format("    (\r\n");
						foreach (TableColumn aColumn in constraint.Columns)
						{
							if (constraint.Columns.IndexOf(aColumn) == constraint.Columns.Count - 1)
								unparsedText += string.Format("        \"{0}\"\r\n", aColumn.Name.ToUpper());
							else
								unparsedText += string.Format("        \"{0}\",\r\n", aColumn.Name.ToUpper());
						}
						if (constraint.IsPrimaryKeyConstraint)
							if (aTable.Constraints.IndexOf(constraint) < aTable.Constraints.Count - 1)
								unparsedText += string.Format("    ),\r\n");
							else
								unparsedText += string.Format("    )\r\n");
						else
						{
							unparsedText += string.Format("    ) REFERENCES \"{0}\" (\r\n", constraint.ReferenceTableName.ToUpper());
							foreach (string rifColonna in constraint.ReferenceColumns)
							{
								if (constraint.ReferenceColumns.IndexOf(rifColonna) == constraint.ReferenceColumns.Count - 1)
									unparsedText += string.Format("        \"{0}\"\r\n", rifColonna.ToUpper());
								else
									unparsedText += string.Format("        \"{0}\",\r\n", rifColonna.ToUpper());
							}
							if (aTable.Constraints.IndexOf(constraint) < aTable.Constraints.Count - 1)
								unparsedText += string.Format("    ),\r\n");
							else
								unparsedText += string.Format("    )\r\n");
						}
					}
				}

				unparsedText += string.Format(")\r\n");
				unparsedText += string.Format("GO\r\n\r\n");

				if (aTable.Indexes != null && aTable.Indexes.Count > 0)
				{
					foreach (TableIndex indice in aTable.Indexes)
						unparsedText += MakeIndexesOracle(indice);
				}
			}
			
			string[] errorsEncountered = tables.CheckNames();
			if (errorsEncountered == null || errorsEncountered.Length == 0)
				return true;

			errorsList.AddRange(errorsEncountered);

			return false;
		}

		#endregion // Unparse

		#endregion // SqlParser protected virtual methods

		#endregion // SqlParser protected methods

		#region SqlParser public properties
		
		//-----------------------------------------------------------------------------
		public SqlTableList Tables { get { return tables; } }
		
		//-----------------------------------------------------------------------------
		public bool HasErrors { get { return (errorsList.Count > 0); } }
		
		//-----------------------------------------------------------------------------
		public string[] Errors { get { return (string[])errorsList.ToArray(typeof(string)); } }

		#endregion

		#region SqlParser public methods

		//-----------------------------------------------------------------------------
		public SqlTable GetTableByName(string aTableName)
		{
			if (tables == null || tables.Count == 0)
				return null;

			return tables.GetTableByName(aTableName);
		}

		//-----------------------------------------------------------------------------
		public void AddTable(string aTableName)
		{
			if (tables == null)
				return;

			tables.Add(aTableName);
		}

		//-----------------------------------------------------------------------------
		public bool IsView(string fileText)
		{
			string tmp = fileText.Replace(" ", string.Empty).ToUpper();;
			return (tmp.IndexOf("CREATEVIEW") > 0);
		}

		//-----------------------------------------------------------------------------
		public void SetDefaultConstraintName()
		{
			if (tables == null || tables.Count == 0)
				return;

			foreach (SqlTable aTable in tables)
				aTable.SetDefaultConstraintName(uid);
		}

		//-----------------------------------------------------------------------------
		public ArrayList CheckConstraints()
		{
			if (tables == null || tables.Count == 0)
				return null;

			ArrayList constraintNames = new ArrayList();

			foreach (SqlTable aTable in tables)
			{
				if (aTable.Constraints == null || aTable.Constraints.Count == 0)
					continue;

				foreach (TableConstraint aConstraint in aTable.Constraints)
					constraintNames.Add(aConstraint.Name);
			}

			if (constraintNames.Count == 0)
				return null;

			constraintNames.Sort();

			string previousConstraintName = string.Empty;
			ArrayList err = new ArrayList();

			foreach (string aConstraintName in constraintNames)
			{
				if (String.Compare(aConstraintName, previousConstraintName, true) == 0)
					err.Add(aConstraintName);
				else
					previousConstraintName = aConstraintName;
			}
			
			return err;
		}

		//-----------------------------------------------------------------------------
		public virtual bool UnParseSql(string fileName)
		{
			string unparsedText = string.Empty;
			
			UnParseSql(out unparsedText);

			return SqlFileWrite(fileName, unparsedText);
		}

		//-----------------------------------------------------------------------------
		public virtual bool UnParseOracle(string fileName)
		{
			string unparsedText = string.Empty;
			
			return UnParseOracle(out unparsedText) && OracleFileWrite(fileName, unparsedText);
		}

		//-----------------------------------------------------------------------------
		public virtual bool ParseWithLexan(string fileName)
		{
			LexanSqlParser lsp = new LexanSqlParser(tables);
            return lsp.Parse(fileName);
		}

		///<summary>
		/// Estensione del LexanSqlParser che effettua prima della Parse la rimozione dei 
		/// commenti su una o più righe
		///</summary>
		//-----------------------------------------------------------------------------
		public virtual bool ParseWithNoCommentLexanSqlParser(string fileName)
		{
			NoCommentLexanSqlParser lsp = new NoCommentLexanSqlParser(tables);
            if (lsp.Parse(fileName))
            {
                procedureNames = lsp.ProcedureNames;
                viewNames = lsp.ViewNames;
                return true;
            }
            return false;
		}

		//-----------------------------------------------------------------------------
		public virtual bool Parse(string fileName)
		{
			string sqlFileText = SqlFileRead(fileName);
			if (sqlFileText == null || sqlFileText.Length == 0 || IsView(sqlFileText))
				return false;

			int parserState = 0;

			string tableName = string.Empty;
			SqlTable currentTable = null;
			string columnName = string.Empty;
			string columnType = string.Empty;
			int columnLength = 0;
			bool isColumnNullable = true;
			string defaultValue = string.Empty;
			string constraintName = string.Empty;
			string constraintColumnName = string.Empty;
			bool isPrimaryConstraint = true;
			string defaultConstraintName = string.Empty;
			string indexName = string.Empty;
			bool isCommentToFind = false;

			StringReader sqlScriptStringReader = new StringReader(sqlFileText);
			string sqlScriptLine = string.Empty;

			while (true) 
			{
				sqlScriptLine = sqlScriptStringReader.ReadLine();
				if (sqlScriptLine == null) 
				{
					break;
				}

				sqlScriptLine = sqlScriptLine.Trim();
				
				while(sqlScriptLine != string.Empty)
				{
					if (sqlScriptLine.Length >= 2)
					{
						if (sqlScriptLine.Substring(0,2) == "--")
						{
							sqlScriptLine = string.Empty;
							continue;
						}

						isCommentToFind = isCommentToFind || sqlScriptLine.StartsWith("/*");

						if (isCommentToFind)
						{
							int endCommentIndex = sqlScriptLine.IndexOf("*/", 2);
							if (endCommentIndex >= 0)
							{
								sqlScriptLine = sqlScriptLine.Remove(0, endCommentIndex + 2);
								isCommentToFind = false;
							}
							else
								sqlScriptLine = string.Empty;
						}
					}

					if (sqlScriptLine == string.Empty)
						continue;

					switch(parserState)
					{
						case 0: //Devo ancora trovare il BEGIN
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "BEGIN", true) == 0)
							{
								sqlScriptLine = string.Empty;
								parserState = 1;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 1: //Cerco TABLE
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "TABLE", true) == 0)
							{
								parserState = 2;
							}

							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 2: //Cerco il nome della tabella
						{
							tableName = sqlScriptLine.Split(' ')[0];
							sqlScriptLine = sqlScriptLine.Substring(tableName.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							
							tables.Add(tableName);

							currentTable = tables.GetTableByName(tableName);
							
							parserState = 3;
							break;
						}
						case 3: //Cerco (
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "(", true) == 0)
							{
								parserState = 4;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 4: //Cerco il nome della colonna o il CONSTRAINT o )
						{
							columnName = sqlScriptLine.Split(' ')[0];
							
							sqlScriptLine = sqlScriptLine.Substring(columnName.Length);
							sqlScriptLine = sqlScriptLine.Trim();

							if (string.Compare(columnName, "CONSTRAINT", true) == 0)
							{
								columnName = string.Empty;
								parserState = 9;
							}
							else
							{
								if (string.Compare(columnName, ")", true) == 0)
								{
									columnName = string.Empty;
									parserState = 18;
								}
								else
									parserState = 5;
							}
							
							break;
						}
						case 5: //Cerco il tipo della colonna
						{
							columnType = sqlScriptLine.Split(' ')[0];
							sqlScriptLine = sqlScriptLine.Substring(columnType.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							if (
								(string.Compare(columnType, NVarcharFieldDataType, true) == 0) || 
								(string.Compare(columnType, VarcharFieldDataType, true) == 0) || 
								(string.Compare(columnType, CharFieldDataType, true) == 0))
								parserState = 6;
							else
								parserState = 7;
							break;
						}
						case 6: //Cerco la lunghezza della colonna
						{
							string tmp = sqlScriptLine.Split(' ')[0];
							tmp = tmp.Replace("(", string.Empty);
							tmp = tmp.Replace(")", string.Empty);
							tmp = tmp.Trim();

							columnLength = int.Parse(tmp);
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							parserState = 7;
							break;
						}
						case 7: //Cerco NULL o NOT NULL
						{
							string tmp = sqlScriptLine.Split(' ')[0];

							if (string.Compare(tmp, ",", true) == 0)
							{
								defaultValue = string.Empty;
								defaultConstraintName = string.Empty;
								if (currentTable != null)
									currentTable.AddColumn(columnName, columnType, columnLength, isColumnNullable, defaultValue, defaultConstraintName);
								parserState = 4;
							}
							
							if (string.Compare(tmp, "NULL", true) == 0)
							{
								isColumnNullable = true;
							}

							if (string.Compare(tmp, "NOT", true) == 0)
							{
								isColumnNullable = false;
								sqlScriptLine = sqlScriptLine.Substring(tmp.Length + sqlScriptLine.Split(' ')[1].Length + 1);
								sqlScriptLine = sqlScriptLine.Trim();
								break;
							}

							if (string.Compare(tmp, "CONSTRAINT", true) == 0)
							{
								sqlScriptLine = sqlScriptLine.Replace("(", "");
								sqlScriptLine = sqlScriptLine.Replace(")", "");
								parserState = 26;
							}

							sqlScriptLine = sqlScriptLine.Substring(tmp.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							
							break;
						}
						case 8: //Cerco il valore di default
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], ",", true) != 0)
							{
								defaultValue = string.Format("{0} {1}", defaultValue, sqlScriptLine.Split(' ')[0]).Trim();

								if (sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length).Trim() == string.Empty) // non è stata messa la ,
								{
									tables.GetTableByName(tableName).AddColumn(columnName, columnType, columnLength, isColumnNullable, defaultValue, defaultConstraintName);
									defaultConstraintName = string.Empty;
									defaultValue = string.Empty;
									parserState = 4;
								}
							}
							else
							{
								tables.GetTableByName(tableName).AddColumn(columnName, columnType, columnLength, isColumnNullable, defaultValue, defaultConstraintName);
								defaultConstraintName = string.Empty;
								defaultValue = string.Empty;
								parserState = 4;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();

							break;
						}
						case 9: //Cerco il nome del CONSTRAINT
						{
							constraintName = sqlScriptLine.Split(' ')[0];
							sqlScriptLine = sqlScriptLine.Substring(constraintName.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							parserState = 10;
							break;
						}
						case 10: //Cerco il tipo del CONSTRAINT
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "PRIMARY", true) == 0)
								isPrimaryConstraint = true;
							else
								isPrimaryConstraint = false;
							tables.GetTableByName(tableName).AddConstraint(constraintName, isPrimaryConstraint);
							
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							parserState = 11;
							break;
						}
						case 11: //Cerco (
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "(", true) == 0)
							{
								parserState = 12;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 12: //Cerco il nome della colonna
						{
							constraintColumnName = sqlScriptLine.Split(' ')[0];

							switch (constraintColumnName)
							{
								case "," :
								{
									break;
								}
								case ")" :
								{
									parserState = isPrimaryConstraint ? 17 : 13;
									break;
								}
								default :
								{
									if (currentTable != null)
										currentTable.AddConstraintColumn(constraintName, constraintColumnName);
									break;
								}
							}
							
							sqlScriptLine = sqlScriptLine.Substring(constraintColumnName.Length);
							sqlScriptLine = sqlScriptLine.Trim();

							break;
						}
						case 13: //Cerco REFERENCES
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "REFERENCES", true) == 0)
							{
								parserState = 14;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 14: //Cerco il nome della tabella di riferimento
						{
							string referredTableName = sqlScriptLine.Split(' ')[0];
							sqlScriptLine = sqlScriptLine.Substring(referredTableName.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							if (currentTable != null)
								currentTable.AddConstraintTableReference(constraintName, referredTableName);
							parserState = 15;
							break;
						}
						case 15: //Cerco (
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "(", true) == 0)
							{
								parserState = 16;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 16: //Cerco il nome della colonna
						{
							string referredTableColumnName = sqlScriptLine.Split(' ')[0];

							switch (referredTableColumnName)
							{
								case "," :
								{
									break;
								}
								case ")" :
								{
									parserState = 4;

									break;
								}
								default :
								{
									tables.GetTableByName(tableName).AddConstraintColumnReference(constraintName, referredTableColumnName);
									break;
								}
							}
							
							sqlScriptLine = sqlScriptLine.Substring(referredTableColumnName.Length);
							sqlScriptLine = sqlScriptLine.Trim();

							break;
						}
						case 17: //Cerco , o )
						{
							switch (sqlScriptLine.Split(' ')[0])
							{
								case "," :
								{
									parserState = 4;
									break;
								}
								case ")" :
								{
									parserState = 18;
									break;
								}
							}
							
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();

							break;
						}
						case 18: //Cerco CREATE o END
						{
							switch (sqlScriptLine.Split(' ')[0])
							{
								case "CREATE" :
								{
									parserState = 19;
									break;
								}
								case "END" :
								{
									parserState = 0;
									break;
								}
							}
							
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();

							break;
						}
						case 19: //Cerco INDEX
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "INDEX", true) == 0)
							{
								parserState = 20;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 20: //Cerco il nome dell'Indice
						{
							indexName = sqlScriptLine.Split(' ')[0];

							sqlScriptLine = sqlScriptLine.Substring(indexName.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							parserState = 21;
							break;
						}
						case 21: //Cerco ON
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "ON", true) == 0)
							{
								parserState = 22;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 22: //Cerco il nome della tabella
						{
							string tableIndexName = sqlScriptLine.Split(' ')[0];
							tables.GetTableByName(tableName).AddIndex(indexName, tableIndexName);

							sqlScriptLine = sqlScriptLine.Substring(tableIndexName.Length);
							sqlScriptLine = sqlScriptLine.Trim();
							parserState = 23;
							break;
						}
						case 23: //Cerco (
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "(", true) == 0)
							{
								parserState = 24;
							}
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();
							break;
						}
						case 24: //Cerco il nome della colonna
						{
							string indexColumnName = sqlScriptLine.Split(' ')[0];

							if (currentTable != null)
								currentTable.AddIndexColumn(indexName, indexColumnName);

							sqlScriptLine = sqlScriptLine.Substring(indexColumnName.Length);
							sqlScriptLine = sqlScriptLine.Trim();

							parserState = 25;

							break;
						}
						case 25: //Cerco , o )
						{
							switch (sqlScriptLine.Split(' ')[0])
							{
								case "," :
								{
									parserState = 24;
									break;
								}
								case ")" :
								{
									parserState = 18;
									break;
								}
							}
							
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();

							break;
						}
						case 26: //Cerco il nome del constraint
						{
							defaultConstraintName = sqlScriptLine.Split(' ')[0].Trim();
							
							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();

							parserState = 27;

							break;
						}
						case 27: //Cerco la parola chiave DEFAULT
						{
							if (string.Compare(sqlScriptLine.Split(' ')[0], "DEFAULT", true) == 0)
							{
								sqlScriptLine = sqlScriptLine.Replace("(", "");
								sqlScriptLine = sqlScriptLine.Replace(")", "");
								parserState = 8;
							}

							sqlScriptLine = sqlScriptLine.Substring(sqlScriptLine.Split(' ')[0].Length);
							sqlScriptLine = sqlScriptLine.Trim();

							break;
						}
					}
				}
			}
			return true;
		}

	
		//-----------------------------------------------------------------------------
		public bool ParseAlterTableAddedColumns(string fileName, string aTableName, out TableColumnList addedColumns)
		{
			LexanSqlParser lsp = new LexanSqlParser();

			addedColumns = null;

			return lsp.ParseAlterTableAddedColumns(fileName, aTableName, out addedColumns);
		}

		//-----------------------------------------------------------------------------
		public bool ParseAlterTableAddedColumns(string fileName, out Hashtable addedColumnsForTable, out IList<TableUpdate> updateColumns)
		{
			NoCommentLexanSqlParser lsp = new NoCommentLexanSqlParser();

			addedColumnsForTable = null;

			return lsp.ParseAlterTableAddedColumns(fileName, out addedColumnsForTable, out updateColumns);
		}

		
		#endregion // SqlParser public methods

	}

	#endregion
}
