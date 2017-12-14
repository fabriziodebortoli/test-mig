using System;
using System.Collections.Generic;

namespace Microarea.Library.SqlScriptUtility
{
	# region ViewColumn (mapping colonna di view)
	//=================================================================================
	public class ViewColumn
	{
		private string name = string.Empty;
		private string dataType = string.Empty;
		private uint dataLength = 0;
		private uint tbenum = 0; // valore dell'enumerativo letto dal c++

		private bool isNew = false;
		private bool isCollateSensitive = true;

		//-----------------------------------------------------------------------------
		public string Name { get { return name; } set { name = value; } }
		//-----------------------------------------------------------------------------
		public string DataType { get { return dataType; } set { dataType = value; } }
		//-----------------------------------------------------------------------------
		public uint DataLength { get { return dataLength; } set { dataLength = value; } }
		//---------------------------------------------------------------------------
        public uint TbEnum { get { return tbenum; } set { tbenum = value; } }
		//-----------------------------------------------------------------------------
		public bool IsNew { get { return isNew; } }
		//-----------------------------------------------------------------------------
		public bool IsCollateSensitive { get { return isCollateSensitive; } set { isCollateSensitive = value; } }

		//-----------------------------------------------------------------------------
		public ViewColumn(string aColumnName, string aColumnType, uint aColumnLength)
		{
			name = aColumnName;
			dataType = aColumnType;
			dataLength = aColumnLength;
		}

		//-----------------------------------------------------------------------------
        public ViewColumn(string aColumnName, string aColumnType, uint aColumnLength, bool aIsCollateSensitive, uint aTbEnum)
		{
			name = aColumnName;
			dataType = aColumnType;
			dataLength = aColumnLength;
			isCollateSensitive = aIsCollateSensitive;
			tbenum = aTbEnum;
		}

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return this.Name;
        }
    }
	
    # endregion

	# region ViewColumnList (array di ViewColumn)
	//=================================================================================
	public class ViewColumnList : List<ViewColumn>
	{
		//-----------------------------------------------------------------------------
		public ViewColumnList()
		{
		}

		//-----------------------------------------------------------------------------
		public void Add
			(
			string aColumnName,
			string aColumnType,
			uint aColumnLength,
			bool isCollateSensitive,
			uint tbEnum
			)
		{
			if (string.IsNullOrEmpty(aColumnName))
				return;

			if (!Exists(aColumnName))
			{
				ViewColumn tc = new ViewColumn(aColumnName, aColumnType, aColumnLength, isCollateSensitive, tbEnum);
				base.Add(tc);
			}
		}

		//-----------------------------------------------------------------------------
		public void Delete(string aColumnName)
		{
			ViewColumn columnToDelete = this.GetColumnByName(aColumnName);
			if (columnToDelete != null)
				base.Remove(columnToDelete);
		}

		//-----------------------------------------------------------------------------
		public bool Exists(string aColumnName)
		{
			if (string.IsNullOrEmpty(aColumnName))
				return false;

			foreach (ViewColumn aColumn in this)
			{
				if (String.Compare(aColumn.Name, aColumnName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public ViewColumn GetColumnByName(string aColumnName)
		{
			if (string.IsNullOrEmpty(aColumnName))
				return null;

			foreach (ViewColumn aColumn in this)
			{
				if (String.Compare(aColumn.Name, aColumnName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return aColumn;
			}
			return null;
		}
	}
	# endregion

	# region SqlView (mapping tabella)
	//=================================================================================
	public class SqlView
	{
		private string name = String.Empty;
		private ViewColumnList columns = new ViewColumnList();	// lista di colonne/alias usati nella view
		private string sqlDefinition = string.Empty;			// testo di creazione della view con la sintassi SQL
		private string oracleDefinition = string.Empty;			// testo di creazione della view con la sintassi ORACLE
        private string tbNameSpace = String.Empty;
        private int creationDbReleaseNumber = 0;

		//-----------------------------------------------------------------------------
		public string Name { get { return name; } }
        //-----------------------------------------------------------------------------
        public string TbNameSpace { get { return tbNameSpace; } set { tbNameSpace = value; } }
        //-----------------------------------------------------------------------------
        public int CreationDbReleaseNumber { get { return creationDbReleaseNumber; } set { creationDbReleaseNumber = value; } }
		//-----------------------------------------------------------------------------
		public ViewColumnList Columns { get { return columns; } } 
		//-----------------------------------------------------------------------------
		public string SqlDefinition { get { return sqlDefinition; } set { sqlDefinition = value; } } 
		//-----------------------------------------------------------------------------
		public string OracleDefinition { get { return oracleDefinition; } set { oracleDefinition = value; } } 
		//-----------------------------------------------------------------------------
		public string OracleViewName { get { return (name != null && name.Length > 0) ? name.ToUpper() : String.Empty; } }

		//-----------------------------------------------------------------------------
		public SqlView(string aViewName)
		{
			/*if (aViewName.Length > 30)
				name = aViewName.Substring(0, 30);
			else*/
			name = aViewName;
		}

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return this.Name;
        }

		//-----------------------------------------------------------------------------
		public void AddColumn
			(
			string aColumnName,
			string aColumnType,
			uint aColumnLength,
			bool isCollateSensitive,
			uint tbEnum
			)
		{
			columns.Add(aColumnName, aColumnType, aColumnLength, isCollateSensitive, tbEnum);
		}

		//-----------------------------------------------------------------------------
		public void DeleteColumn(string aColumnName)
		{
			if (string.IsNullOrEmpty(aColumnName))
				return;

			columns.Delete(aColumnName);
		}

		//-----------------------------------------------------------------------------
		public ViewColumn GetColumn(string aColumnName)
		{
			return columns.GetColumnByName(aColumnName);
		}
	}
	# endregion

	#region SqlViewList (array di SqlView)
	//=================================================================================
	public class SqlViewList : List<SqlView>
	{
		//-----------------------------------------------------------------------------
		public SqlViewList()
		{
		}

		//-----------------------------------------------------------------------------
		public void Add(string aViewName)
		{
			if (string.IsNullOrEmpty(aViewName))
				return;

			if (!ViewExists(aViewName))
				base.Add(new SqlView(aViewName));
		}

		//-----------------------------------------------------------------------------
		public SqlView GetViewByName(string aViewName)
		{
			if (string.IsNullOrEmpty(aViewName))
				return null;

			foreach (SqlView aView in this)
			{
				if (String.Compare(aView.Name, aViewName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return aView;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public SqlView GetViewByOracleName(string aViewName)
		{
			if (string.IsNullOrEmpty(aViewName))
				return null;

			foreach (SqlView aView in this)
			{
				if (String.Compare(aView.OracleViewName, aViewName) == 0)
					return aView;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool ViewExists(string aViewName)
		{
			foreach (SqlView aView in this)
			{
				if (String.Compare(aView.OracleViewName, aViewName) == 0)
					return true;
			}
			return false;
		}
	}
	# endregion
}
