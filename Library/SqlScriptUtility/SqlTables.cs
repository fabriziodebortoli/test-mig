using System;
using System.Collections;

namespace Microarea.Library.SqlScriptUtility
{
	# region TableColumn (mapping colonna di tabella)
	//=================================================================================
	public class TableColumn
	{
		private string name = string.Empty;
		private string dataType = string.Empty;
		private int dataLength = 0;
		private string defaultValue = null;
		private string defaultExpressionValue = string.Empty;
		private bool isNullable = true;
		private string defaultConstraintName = string.Empty;
		private bool isNew = false;
		private bool isChanging = false;
		private bool isCollateSensitive = true;
		private bool isAutoIncrement = false;
		private int seed = -1;
		private int increment = -1;

		// Old Values
		private string oldName = string.Empty; //Ancora non gestito
		private int oldLength = 0;
		private string oldDefaultValue = string.Empty;
		//private string oldDefaultExpressionValue = string.Empty; //TODO???

		//-----------------------------------------------------------------------------
		public string Name { get { return name; } set { name = value; } }
		//-----------------------------------------------------------------------------
		public string DefaultConstraintName { get { return defaultConstraintName; } set { defaultConstraintName = value; } }
		//-----------------------------------------------------------------------------
		public string DataType { get { return dataType; } set { dataType = value; } }
		//-----------------------------------------------------------------------------
		public int DataLength { get { return dataLength; } set { dataLength = value; } }
		//-----------------------------------------------------------------------------
		public string DefaultValue { get { return defaultValue; } set { defaultValue = value; } }
		//-----------------------------------------------------------------------------
		public string DefaultExpressionValue { get { return defaultExpressionValue; } set { defaultExpressionValue = value; } }
		//-----------------------------------------------------------------------------
		public bool IsNullable { get { return isNullable; } set { isNullable = value; } }
		//-----------------------------------------------------------------------------
		public bool IsNew { get { return isNew; } }
		//-----------------------------------------------------------------------------
		public bool IsChanging { get { return isChanging; } }
		//-----------------------------------------------------------------------------
		public bool IsCollateSensitive { get { return isCollateSensitive; } set { isCollateSensitive = value; } }
		//-----------------------------------------------------------------------------
		public bool IsAutoIncrement { get { return isAutoIncrement; } set { isAutoIncrement = value; } }
		//-----------------------------------------------------------------------------
		public int Seed { get { return seed; } set { seed = value; } }
		//-----------------------------------------------------------------------------
		public int Increment { get { return increment; } set { increment = value; } }

		//-----------------------------------------------------------------------------
		public TableColumn(string aColumnName, string aColumnType, int aColumnLength, bool nullable, string valoreDefault, string nomeConstraintDefault)
		{
			name = aColumnName;
			dataType = aColumnType;
			dataLength = aColumnLength;
			defaultValue = valoreDefault;
			isNullable = nullable;
			defaultConstraintName = nomeConstraintDefault;
		}

		//-----------------------------------------------------------------------------
		public TableColumn()
		{
			isNew = true;
		}

		//-----------------------------------------------------------------------------
		public void SetDefaultConstraintName(string aTableName, CUIdList uid)
		{
			if (defaultConstraintName != string.Empty)
				return;

			string nTabella = aTableName;
			string nColonna = name;
			
			try
			{
				if (nTabella.Length > 13)
					nTabella = nTabella.Substring(3, 10);
				else
					nTabella = nTabella.Substring(3, nTabella.Length - 3);
			}
			catch
			{
			}

			if (nColonna.Length > 10)
				nColonna = nColonna.Substring(0, 10);

			string nConstraint = string.Format("DF_{0}_{1}", nTabella, nColonna);

			defaultConstraintName = uid.Add(nConstraint);
		}

		//-----------------------------------------------------------------------------
		public void Rename(string newName)
		{
			name = newName;
		}

		//-----------------------------------------------------------------------------
		public void StartChanges()
		{
			isChanging = true;

			if (oldName.Length == 0)
				oldName = name;
			
			if (oldLength == 0)
				oldLength = dataLength;
			
			if (oldDefaultValue.Length == 0)
				oldDefaultValue = defaultValue;
		}

		//-----------------------------------------------------------------------------
		public bool IsNameToChange()
		{
			return (String.Compare(oldName, name) != 0);
		}

		//-----------------------------------------------------------------------------
		public bool IsLengthToChange()
		{
			return (oldLength != dataLength);
		}
	
		//-----------------------------------------------------------------------------
		public bool IsDefaultValueToChange()
		{
			return (String.Compare(oldDefaultValue, defaultValue) != 0);
		}

		//-----------------------------------------------------------------------------
		public bool IsToChange()
		{
			return IsNameToChange() || IsLengthToChange() || IsDefaultValueToChange();
		}
	}
	# endregion

	# region TableColumnList (array di TableColumn)
	//=================================================================================
	public class TableColumnList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public TableColumnList ()
		{
		}

		//-----------------------------------------------------------------------------
		public void Add
			(
			string aColumnName, 
			string aColumnType, 
			int aColumnLength, 
			bool nullable, 
			string valoreDefault, 
			string nomeConstraintDefault, 
			bool isCollateSensitive,
			bool isAutoIncrement,
			int seed,
			int increment,
			string defaultExpressionValue)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return;

			if (!Exist(aColumnName))
			{
				TableColumn tc = new TableColumn(aColumnName, aColumnType, aColumnLength, nullable, valoreDefault, nomeConstraintDefault);
				tc.IsCollateSensitive = isCollateSensitive;
				tc.IsAutoIncrement = isAutoIncrement;
				tc.Seed = seed;
				tc.Increment = increment;
				tc.DefaultExpressionValue = defaultExpressionValue;
				base.Add(tc);
			}
		}

		//-----------------------------------------------------------------------------
		public void Delete(string aColumnName)
		{
			TableColumn columnToDelete = this.GetColumnByName(aColumnName);
			if (columnToDelete != null)
				base.Remove(columnToDelete);
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string aColumnName)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return false;

			foreach (TableColumn aColumn in this)
			{
				if (String.Compare(aColumn.Name, aColumnName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public TableColumn GetColumnByName(string aColumnName)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return null;
			
			foreach (TableColumn aColumn in this)
			{
				if (String.Compare(aColumn.Name, aColumnName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return aColumn;
			}
			return null;
		}
	}
	# endregion

	# region TableConstraint (mapping constraint di tabella [PK/FK])
	//=================================================================================
	public class TableConstraint
	{
		private string name = string.Empty;
		private string extendedName = string.Empty;

		private bool isPrimary = true;
		private bool isOracleScriptApproved = false;
		private bool clustered = true;

		public string referenceTableName = string.Empty;
		public string referenceTableExtendedName = string.Empty;

		public TableColumnList columns = new TableColumnList();
		
		private ArrayList referenceColumns = new ArrayList();
		private ArrayList referenceExtendedColumns = new ArrayList();

		//per FK
		private bool onDeleteCascade = false;
		private bool onUpdateCascade = false;

		//-----------------------------------------------------------------------------
		public string Name { get { return name; } set { name = value; }}
		//-----------------------------------------------------------------------------
		public string ExtendedName { get { return extendedName; } }
		//-----------------------------------------------------------------------------
		public bool IsPrimaryKeyConstraint { get { return isPrimary; } }
		//-----------------------------------------------------------------------------
		public bool Clustered { get { return clustered; } set { clustered = value; } }
		//-----------------------------------------------------------------------------
		public bool IsOracleScriptApproved { get { return isOracleScriptApproved; } set { isOracleScriptApproved = value; } }
		//-----------------------------------------------------------------------------
		public string ReferenceTableName { get { return referenceTableName; } }
		//-----------------------------------------------------------------------------
		public TableColumnList Columns { get { return columns; } }
		//-----------------------------------------------------------------------------
		public ArrayList ReferenceColumns { get { return referenceColumns; } }
		//-----------------------------------------------------------------------------
		public bool OnDeleteCascade { get { return onDeleteCascade; } set { onDeleteCascade = value; } }
		//-----------------------------------------------------------------------------
		public bool OnUpdateCascade { get { return onUpdateCascade; } set { onUpdateCascade = value; } }

		//-----------------------------------------------------------------------------
		public TableConstraint(string aConstraintName, bool isPrimaryConstraint)
		{
			extendedName = aConstraintName;
/*			if (aConstraintName.Length > 30)
				name = aConstraintName.Substring(0, 30);
			else
*/				name = aConstraintName;
			isPrimary = isPrimaryConstraint;
		}

		//-----------------------------------------------------------------------------
		public bool IsColumnInvolved(string aColumnName)
		{
			if (aColumnName == null || aColumnName.Length == 0 || columns == null || columns.Count == 0)
				return false;

			return columns.Exist(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public void AddColumn(TableColumn aColumn)
		{
			if (aColumn == null)
				return;

			columns.Add(aColumn);
		}

		//-----------------------------------------------------------------------------
		public void MoveUpColumn(TableColumn aColumn)
		{
			if (aColumn == null)
				return;

			int idx = columns.IndexOf(aColumn);
			if (idx <= 0 || idx >= columns.Count)
				return;

			columns.Remove(aColumn);
			columns.Insert(idx - 1, aColumn);
		}

		//-----------------------------------------------------------------------------
		public void MoveDownColumn(TableColumn aColumn)
		{
			if (aColumn == null)
				return;

			int idx = columns.IndexOf(aColumn);
			if (idx < 0 || idx >= (columns.Count - 1))
				return;

			columns.Remove(aColumn);
			columns.Insert(idx + 1, aColumn);
		}

		//-----------------------------------------------------------------------------
		public void RemoveColumn(TableColumn aColumn)
		{
			if (aColumn == null)
				return;
			
			if (columns != null && columns.Contains(aColumn))
				columns.Remove(aColumn);
		}

		//-----------------------------------------------------------------------------
		public void DeleteColumn(string aColumnName)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return;
			
			if (columns != null)
				columns.Delete(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public void AddTableReference(string aTableName)
		{
			referenceTableExtendedName = aTableName;
/*			if (aTableName.Length > 30)
				referenceTableName = aTableName.Substring(0, 30);
			else
*/				referenceTableName = aTableName;
		}

		//-----------------------------------------------------------------------------
		public void AddColumnReference(string aColumnName)
		{
			if (String.IsNullOrEmpty(aColumnName))
				return;
			referenceExtendedColumns.Add(aColumnName);
/*			if (aColumnName.Length > 30)
				referenceColumns.Add(aColumnName.Substring(0, 30));
			else
*/				referenceColumns.Add(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public string[] CheckNames()
		{
			ArrayList errors = new ArrayList();
			
			if (extendedName != name && !isOracleScriptApproved)
				errors.Add(string.Format(Strings.TruncatedConstraintNameMessageFormat, extendedName, name.ToUpper()));

			return (errors.Count > 0) ? (string[])errors.ToArray(typeof(string)) : null;
		}
	}
	# endregion

	# region TableConstraintList (array di TableConstraint)
	//=================================================================================
	public class TableConstraintList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public TableConstraintList ()
		{
		}

		//-----------------------------------------------------------------------------
		public void Add(string aConstraintName, bool isPrimaryConstraint)
		{
			if (!Exist(aConstraintName))
				base.Add(new TableConstraint(aConstraintName, isPrimaryConstraint));
		}

		//-----------------------------------------------------------------------------
		public TableConstraint GetConstraintByName(string aConstraintName)
		{
			if (aConstraintName == null || aConstraintName.Length == 0)
				return null;

			foreach (TableConstraint aConstraint in this)
			{
				if (aConstraint.ExtendedName == aConstraintName)
					return aConstraint;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string aConstraintName)
		{
			if (aConstraintName == null || aConstraintName.Length == 0)
				return false;

			foreach (TableConstraint aConstraint in this)
			{
				if (String.Compare(aConstraint.ExtendedName, aConstraintName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;
			}
			return false;
		}
		
		//-----------------------------------------------------------------------------
		public TableConstraint GetPrimaryKeyConstraint()
		{
			foreach (TableConstraint aConstraint in this)
			{
				if (aConstraint.IsPrimaryKeyConstraint)
					return aConstraint;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public string[] CheckNames()
		{
			ArrayList errors = new ArrayList();

			foreach(TableConstraint aConstraint in this)
			{
				string[] constraintErrors = aConstraint.CheckNames();
				if (constraintErrors != null && constraintErrors.Length > 0)
					errors.AddRange(constraintErrors);
			}
	
			return (errors.Count > 0) ? (string[])errors.ToArray(typeof(string)) : null;
		}

		//-----------------------------------------------------------------------------
		public void DeleteColumn(string aColumnName)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return;
			
			foreach(TableConstraint aConstraint in this)
				aConstraint.DeleteColumn(aColumnName);
		}
	}
	# endregion

	# region TableUpdate (mapping UPDATE di colonna)
	//=================================================================================
	public class TableUpdate
	{
		private string tableName = string.Empty;
		private string setColumnName = string.Empty;

		private bool setValueAsString = true;
        private string setValueForSql = string.Empty;
        private string setValueForOracle = string.Empty;

		private string whereTableName = string.Empty;
		private string whereColumnName = string.Empty;
        private string whereValueForSql = string.Empty;
        private string whereValueForOracle = string.Empty;

		//-----------------------------------------------------------------------------
		public string TableName { get { return tableName; } set { tableName = value; } }

		//-----------------------------------------------------------------------------
		public string SetColumnName { get { return setColumnName; } set { setColumnName = value; } }

		//-----------------------------------------------------------------------------
		public bool SetValueAsString { get { return setValueAsString; } set { setValueAsString = value; } }

		//-----------------------------------------------------------------------------
        public string SetValueForSql { get { return setValueAsString ? String.Concat("'", setValueForSql, "'") : setValueForSql; } set { setValueForSql = value; } }

        //-----------------------------------------------------------------------------
        public string SetValueForOracle { get { return setValueAsString ? String.Concat("'", setValueForOracle, "'") : setValueForOracle; } set { setValueForOracle = value; } }

		//-----------------------------------------------------------------------------
		public string WhereTableName 
		{
			get { return string.IsNullOrEmpty(whereTableName) ? tableName : whereTableName; } 
			set { whereTableName = string.IsNullOrEmpty(value) ? tableName : value; } 
		}
		//-----------------------------------------------------------------------------
		public string WhereColumnName { get { return whereColumnName; } set { whereColumnName = value; } }

		//-----------------------------------------------------------------------------
        public string WhereValueForSql { get { return whereValueForSql; } set { whereValueForSql = value; } }

        //-----------------------------------------------------------------------------
        public string WhereValueForOracle { get { return whereValueForOracle; } set { whereValueForOracle = value; } }

        //-----------------------------------------------------------------------------
        public bool ExistsWhereClause { get { return !string.IsNullOrEmpty(whereValueForSql) || !string.IsNullOrEmpty(whereValueForOracle); } }

		//-----------------------------------------------------------------------------
		public TableUpdate(string aTableName)
		{
			tableName = aTableName;
		}
	}
	# endregion

	# region TableIndex (mapping indice di tabella)
	//=================================================================================
	public class TableIndex
	{
		private string name = string.Empty;
		private string table = string.Empty;
		private bool unique = false;
		private bool nonClustered = true;
		
		private TableColumnList columns = new TableColumnList();

		private string extendedName = string.Empty;
		private string extendedTable = string.Empty;

		//-----------------------------------------------------------------------------
		public string Name { get { return name; } }
		//-----------------------------------------------------------------------------
		public string ExtendedName { get { return extendedName; } }
		//-----------------------------------------------------------------------------
		public string Table { get { return table; } }
		//-----------------------------------------------------------------------------
		public TableColumnList Columns { get { return columns; } }
		//---------------------------------------------------------------------------
		public bool Unique { get { return unique; } set { unique = value; } }
		//---------------------------------------------------------------------------
		public bool NonClustered { get { return nonClustered; } set { nonClustered = value; } }

		//-----------------------------------------------------------------------------
		public TableIndex(string nomeIndex, string aTableName): this(nomeIndex, aTableName, false, true)
		{
		}
		//-----------------------------------------------------------------------------
		public TableIndex(string nomeIndex, string aTableName, bool unique, bool nonClustered)
		{
			extendedName = nomeIndex;
/*			if (nomeIndex.Length > 30)
				name = nomeIndex.Substring(0, 30);
			else
*/				name = nomeIndex;
			extendedTable = aTableName;
/*			if (aTableName.Length > 30)
				table = aTableName.Substring(0, 30);
			else
*/				table = aTableName;
			this.unique = unique;
			this.nonClustered = nonClustered;
		}

		//-----------------------------------------------------------------------------
		public void AddColumn(TableColumn aColumn)
		{
			if (aColumn == null)
				return;

			columns.Add(aColumn);
		}

		//-----------------------------------------------------------------------------
		public void DeleteColumn(string aColumnName)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return;

			columns.Delete(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public bool IsColumnIndex(string aColumnName)
		{
			return columns.Exist(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public string[] CheckNames()
		{
			ArrayList errors = new ArrayList();
			
			if (extendedName != name)
				errors.Add(string.Format(Strings.TruncatedIndexNameMessageFormat, extendedName, name.ToUpper()));

			return (errors.Count > 0) ? (string[])errors.ToArray(typeof(string)) : null;
		}
	}
	# endregion

	# region TableIndexList (array di TableIndex)
	//=================================================================================
	public class TableIndexList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public TableIndexList ()
		{
		}

		//-----------------------------------------------------------------------------
		public void Add(string aIndexName, string aTableName)
		{
			if (!Exist(aIndexName))
				base.Add(new TableIndex(aIndexName, aTableName, false, true));
		}
		
		//-----------------------------------------------------------------------------
		public void Add(string aIndexName, string aTableName, bool unique, bool nonClustered)
		{
			if (!Exist(aIndexName))
				base.Add(new TableIndex(aIndexName, aTableName, unique, nonClustered));
		}
		//-----------------------------------------------------------------------------
		public void Remove(string aIndexName, string aTableName)
		{
			TableIndex indexToRemove = GetIndexByName(aIndexName);
			if (indexToRemove == null)
				return;

			base.Remove(indexToRemove);
		}

		//-----------------------------------------------------------------------------
		public TableIndex GetIndexByName(string aIndexName)
		{
			if (aIndexName == null || aIndexName.Length == 0)
				return null;

			foreach (TableIndex aIndex in this)
			{
				if (String.Compare(aIndex.ExtendedName, aIndexName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return aIndex;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string aIndexName)
		{
			if (aIndexName == null || aIndexName.Length == 0)
				return false;

			foreach (TableIndex aIndex in this)
			{
				if (String.Compare(aIndex.ExtendedName, aIndexName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public bool IsColumnIndex(string aColumnName)
		{
			foreach (TableIndex tInd in this)
			{
				if (tInd.IsColumnIndex(aColumnName))
					return true;
			}
			
			return false;
		}

		//-----------------------------------------------------------------------------
		public string[] CheckNames()
		{
			ArrayList errors = new ArrayList();

			foreach(TableIndex aIndex in this)
			{
				string[] indexErrors = aIndex.CheckNames();
				if (indexErrors != null && indexErrors.Length > 0)
					errors.AddRange(indexErrors);
			}
	
			return (errors.Count > 0) ? (string[])errors.ToArray(typeof(string)) : null;
		}

		//-----------------------------------------------------------------------------
		public void DeleteColumn(string aColumnName)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return;

			foreach(TableIndex aIndex in this)
				aIndex.DeleteColumn(aColumnName);
		}
	}
	# endregion

	# region SqlTable (mapping tabella)
	//=================================================================================
	public class SqlTable
	{
		private string name = String.Empty;
		private TableColumnList columns = new TableColumnList();
		private TableConstraintList constraints = new TableConstraintList();
		private TableIndexList indexes = new TableIndexList();

		private string extendedName = string.Empty;

		//-----------------------------------------------------------------------------
		public TableColumnList Columns { get { return columns; } }
		//-----------------------------------------------------------------------------
		public TableConstraintList Constraints { get { return constraints; } }
		//-----------------------------------------------------------------------------
		public TableIndexList Indexes { get { return indexes; } }
		//-----------------------------------------------------------------------------
		public string ExtendedName { get { return extendedName; } }
		//-----------------------------------------------------------------------------
		public string OracleTableName  { get { return (name != null && name.Length > 0) ? name.ToUpper() : String.Empty; } }

		//-----------------------------------------------------------------------------
		public SqlTable(string aTableName)
		{
			extendedName = aTableName;
			
/*			if (aTableName.Length > 30)
				name = aTableName.Substring(0, 30);
			else
*/				name = aTableName;
		}

		//-----------------------------------------------------------------------------
		public void AddColumn(string aColumnName)
		{
			AddColumn(aColumnName, "", 0, true, "", "", true, false, -1, -1, null);
		}

		//-----------------------------------------------------------------------------
		public void AddColumn
			(
			string aColumnName, 
			string aColumnType, 
			int aColumnLength, 
			bool nullable, 
			string valoreDefault, 
			string nomeConstraintDefault
			)
		{
			columns.Add(aColumnName, aColumnType, aColumnLength, nullable, valoreDefault, nomeConstraintDefault, true, false, -1, -1, null);
		}

		//-----------------------------------------------------------------------------
		public void AddColumn
			(
			string aColumnName, 
			string aColumnType, 
			int aColumnLength, 
			bool nullable, 
			string valoreDefault, 
			string nomeConstraintDefault, 
			bool isCollateSensitive,
			bool isAutoIncrement,
			int seed,
			int increment,
			string columnDefaultExpression
			)
		{
			columns.Add(aColumnName, aColumnType, aColumnLength, nullable, valoreDefault, nomeConstraintDefault, isCollateSensitive, isAutoIncrement, seed, increment, columnDefaultExpression);
		}

		//-----------------------------------------------------------------------------
		public void DeleteColumn(string aColumnName)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return;

			columns.Delete(aColumnName);
			constraints.DeleteColumn(aColumnName);
			indexes.DeleteColumn(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public TableColumn GetColumn(string aColumnName)
		{
			return columns.GetColumnByName(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public void AddConstraint(string aConstraintName, bool isPrimaryConstraint)
		{
			constraints.Add(aConstraintName, isPrimaryConstraint);
		}

		//-----------------------------------------------------------------------------
		public void AddConstraintColumn(string aConstraintName, string aColumnName)
		{
			TableConstraint constraintToModify = constraints.GetConstraintByName(aConstraintName);
			if (constraintToModify != null)
				constraintToModify.AddColumn(GetColumn(aColumnName));
		}

		//-----------------------------------------------------------------------------
		public void RemoveConstraintColumn(string aConstraintName, string aColumnName)
		{
			TableConstraint constraintToModify = constraints.GetConstraintByName(aConstraintName);
			if (constraintToModify != null)
				constraintToModify.RemoveColumn(GetColumn(aColumnName));
		}

		//-----------------------------------------------------------------------------
		public void MoveUpConstraintColumn(string aConstraintName, string aColumnName)
		{
			TableConstraint constraintToModify = constraints.GetConstraintByName(aConstraintName);
			if (constraintToModify != null)
				constraintToModify.MoveUpColumn(GetColumn(aColumnName));
		}

		//-----------------------------------------------------------------------------
		public void MoveDownConstraintColumn(string aConstraintName, string aColumnName)
		{
			TableConstraint constraintToModify = constraints.GetConstraintByName(aConstraintName);
			if (constraintToModify != null)
				constraintToModify.MoveDownColumn(GetColumn(aColumnName));
		}

		//-----------------------------------------------------------------------------
		public void AddConstraintTableReference(string aConstraintName, string aTableName)
		{
			TableConstraint constraintToModify = constraints.GetConstraintByName(aConstraintName);
			if (constraintToModify != null)
				constraintToModify.AddTableReference(aTableName);
		}

		//-----------------------------------------------------------------------------
		public void AddConstraintColumnReference(string aConstraintName, string aColumnName)
		{
			TableConstraint constraintToModify = constraints.GetConstraintByName(aConstraintName);
			if (constraintToModify != null)
				constraintToModify.AddColumnReference(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public TableConstraint GetPrimaryKeyConstraint()
		{
			return constraints.GetPrimaryKeyConstraint();
		}

		//-----------------------------------------------------------------------------
		public void AddIndex(string nomeIndex, string aTableName)
		{
			indexes.Add(nomeIndex, aTableName);
		}
		//-----------------------------------------------------------------------------
		public void AddIndex(string nomeIndex, string aTableName, bool unique, bool nonClustered)
		{
			indexes.Add(nomeIndex, aTableName, unique, nonClustered);
		}

		//-----------------------------------------------------------------------------
		public void RemoveIndex(string aIndexName)
		{
			indexes.Remove(aIndexName);
		}

		//-----------------------------------------------------------------------------
		public void AddIndexColumn(string aIndexName, string aColumnName)
		{
			TableIndex indexToModify = indexes.GetIndexByName(aIndexName);
			if (indexToModify != null)
				indexToModify.AddColumn(GetColumn(aColumnName));
		}

		//-----------------------------------------------------------------------------
		public void RemoveIndexColumn(string aIndexName, string aColumnName)
		{
			TableIndex indexToModify = indexes.GetIndexByName(aIndexName);
			if (indexToModify != null)
				indexToModify.DeleteColumn(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public TableIndex GetTableIndex(string aIndexName)
		{
			return indexes.GetIndexByName(aIndexName);
		}

		//-----------------------------------------------------------------------------
		public bool IsColumnIndex(string aColumnName)
		{
			return indexes.IsColumnIndex(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public string[] CheckNames()
		{
			ArrayList errors = new ArrayList();

			if (extendedName != name)
				errors.Add(string.Format(Strings.TruncatedTableNameMessageFormat, extendedName, name.ToUpper()));

			string[] constraintsErrors = constraints.CheckNames();
			if (constraintsErrors != null && constraintsErrors.Length > 0)
				errors.AddRange(constraintsErrors);

			string[] indexesErrors = indexes.CheckNames();
			if (indexesErrors != null && indexesErrors.Length > 0)
				errors.AddRange(indexesErrors);

			if (errors.Count == 0)
				return null;

			errors.Insert(0, string.Format(Strings.TableNameCheckingErrorsMessageFormat, extendedName));

			return (string[])errors.ToArray(typeof(string));
		}
		
		//-----------------------------------------------------------------------------
		public void SetDefaultConstraintName(CUIdList uid)
		{
			foreach(TableColumn c in columns)
			{
				c.SetDefaultConstraintName(name, uid);
			}
		}
	}
	# endregion

	#region SqlTableList (array di SqlTable)
	//=================================================================================
	public class SqlTableList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public SqlTableList ()
		{
		}

		//-----------------------------------------------------------------------------
		public void Add(string aTableName)
		{
			if (aTableName == null || aTableName.Length == 0)
				return;

			if (!TableExists(aTableName))
				base.Add(new SqlTable(aTableName));
		}

		//-----------------------------------------------------------------------------
		public SqlTable GetTableByName(string aTableName)
		{
			if (aTableName == null || aTableName.Length == 0)
				return null;

			foreach (SqlTable aTable in this)
			{
				if (String.Compare(aTable.ExtendedName, aTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return aTable;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public SqlTable GetTableByOracleName(string aTableName)
		{
			if (aTableName == null || aTableName.Length == 0)
				return null;

			foreach (SqlTable aTable in this)
			{
				if (String.Compare(aTable.OracleTableName, aTableName) == 0)
					return aTable;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool TableExists(string aTableName)
		{
			foreach (SqlTable aTable in this)
			{
				if (String.Compare(aTable.OracleTableName, aTableName) == 0)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public string[] CheckNames()
		{
			ArrayList errors = new ArrayList();

			foreach(SqlTable aTable in this)
			{
				string[] tableErrors = aTable.CheckNames();
				if (tableErrors != null && tableErrors.Length > 0)
					errors.AddRange(tableErrors);
			}
			
			return (errors.Count > 0) ? (string[])errors.ToArray(typeof(string)) : null;
		}
	}
	# endregion

	# region TBAfterScript (gestione script di after)
	//=================================================================================
	public class TBAfterScript
	{
		private int step = -1;
		private string sqlValue = string.Empty;
		private string oracleValue = string.Empty;

		//-----------------------------------------------------------------------------
		public int Step { get { return step; } }
		public string SqlValue { get { return sqlValue; } }
		public string OracleValue { get { return oracleValue; } }

		//-----------------------------------------------------------------------------
		public TBAfterScript(int step, string sqlValue, string oracleValue)
		{
			this.step = step;
			this.sqlValue = sqlValue;
			this.oracleValue = oracleValue;
		}

		//-----------------------------------------------------------------------------
		public string GetValueForDBMSType(bool forSQLServer)
		{ 
			return (forSQLServer) ? sqlValue : oracleValue;
		}
	}
	# endregion
}