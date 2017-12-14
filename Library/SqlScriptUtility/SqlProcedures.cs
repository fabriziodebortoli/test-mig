using System;
using System.Collections.Generic;

namespace Microarea.Library.SqlScriptUtility
{
	# region ProcedureParameter (mapping colonna di view)
	//=================================================================================
	public class ProcedureParameter
	{
		private string name = string.Empty;
		private string dataType = string.Empty;
		private uint dataLength = 0;
		private uint tbenum = 0; // valore dell'enumerativo letto dal c++

		private bool isCollateSensitive = true;
		private bool isOut = false;
		
		//-----------------------------------------------------------------------------
		public string Name { get { return name; } set { name = value; } }
		//-----------------------------------------------------------------------------
		public string DataType { get { return dataType; } set { dataType = value; } }
		//-----------------------------------------------------------------------------
		public uint DataLength { get { return dataLength; } set { dataLength = value; } }
		//---------------------------------------------------------------------------
		public uint TbEnum { get { return tbenum; } set { tbenum = value; } }
		//-----------------------------------------------------------------------------
		public bool IsOut { get { return isOut; } set { isOut = value; } }
		//-----------------------------------------------------------------------------
		public bool IsCollateSensitive { get { return isCollateSensitive; } set { isCollateSensitive = value; } }

		//-----------------------------------------------------------------------------
		public ProcedureParameter(string aParameterName, string aParameterType, uint aParameterLength, bool aIsOut)
		{
			name = aParameterName;
			dataType = aParameterType;
			dataLength = aParameterLength;
			isOut = aIsOut;
		}

		//-----------------------------------------------------------------------------
		public ProcedureParameter(string aParameterName, string aParameterType, uint aParameterLength, bool aIsOut, bool aIsCollateSensitive, uint aTbEnum)
		{
			name = aParameterName;
			dataType = aParameterType;
			dataLength = aParameterLength;
			isOut = aIsOut;
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

	# region ProcedureParameterList (array di ProcedureParameter)
	//=================================================================================
	public class ProcedureParameterList : List<ProcedureParameter>
	{
		//-----------------------------------------------------------------------------
		public ProcedureParameterList()
		{
		}

		//-----------------------------------------------------------------------------
		public void Add
			(
			string aParameterName,
			string aParameterType,
			uint aParameterLength,
			bool aIsOut,
			bool aIsCollateSensitive,
			uint tbEnum
			)
		{
			if (string.IsNullOrEmpty(aParameterName))
				return;

			if (!Exists(aParameterName))
			{
				ProcedureParameter tc = new ProcedureParameter(aParameterName, aParameterType, aParameterLength, aIsOut, aIsCollateSensitive, tbEnum);
				base.Add(tc);
			}
		}

		//-----------------------------------------------------------------------------
		public void Delete(string aParameterName)
		{
			ProcedureParameter parameterToDelete = this.GetParameterByName(aParameterName);
			if (parameterToDelete != null)
				base.Remove(parameterToDelete);
		}

		//-----------------------------------------------------------------------------
		public bool Exists(string aParameterName)
		{
			if (string.IsNullOrEmpty(aParameterName))
				return false;

			foreach (ProcedureParameter aParameter in this)
			{
				if (String.Compare(aParameter.Name, aParameterName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public ProcedureParameter GetParameterByName(string aParameterName)
		{
			if (string.IsNullOrEmpty(aParameterName))
				return null;

			foreach (ProcedureParameter aParameter in this)
			{
				if (String.Compare(aParameter.Name, aParameterName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return aParameter;
			}
			return null;
		}
	}
	# endregion

	# region SqlProcedure (mapping stored procedure)
	//=================================================================================
	public class SqlProcedure
	{
		private string name = String.Empty;
		private ProcedureParameterList parameters = new ProcedureParameterList(); // lista di parametri
		private string sqlDefinition = string.Empty; // testo di creazione della procedure (sintassi SQL)
		private string oracleDefinition = string.Empty; // testo di creazione della procedure (sintassi ORACLE)
        private string tbNameSpace = String.Empty;
        private int creationDbReleaseNumber = 0;

        //-----------------------------------------------------------------------------
        public string TbNameSpace { get { return tbNameSpace; } set { tbNameSpace = value; } }
        //-----------------------------------------------------------------------------
        public int CreationDbReleaseNumber { get { return creationDbReleaseNumber; } set { creationDbReleaseNumber = value; } }
		//-----------------------------------------------------------------------------
		public string Name { get { return name; } }
		//-----------------------------------------------------------------------------
		public ProcedureParameterList Parameters { get { return parameters; } }
		//-----------------------------------------------------------------------------
		public string SqlDefinition { get { return sqlDefinition; } set { sqlDefinition = value; } }
		//-----------------------------------------------------------------------------
		public string OracleDefinition { get { return oracleDefinition; } set { oracleDefinition = value; } }
		//-----------------------------------------------------------------------------
		public string OracleProcedureName { get { return (name != null && name.Length > 0) ? name.ToUpper() : String.Empty; } }

		//-----------------------------------------------------------------------------
		public SqlProcedure(string aProcedureName)
		{
			/*if (aProcedureName.Length > 30)
				name = aProcedureName.Substring(0, 30);
			else*/
			name = aProcedureName;
		}

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return this.Name;
        }

		//-----------------------------------------------------------------------------
		public void AddParameter
			(
			string aParameterName,
			string aParameterType,
			uint aParameterLength,
			bool aIsOut,
			bool aIsCollateSensitive,
			uint tbEnum
			)
		{
			parameters.Add(aParameterName, aParameterType, aParameterLength, aIsOut, aIsCollateSensitive, tbEnum);
		}

		//-----------------------------------------------------------------------------
		public void DeleteParameter(string aParameterName)
		{
			if (string.IsNullOrEmpty(aParameterName))
				return;

			parameters.Delete(aParameterName);
		}

		//-----------------------------------------------------------------------------
		public ProcedureParameter GetParameter(string aParameterName)
		{
			return parameters.GetParameterByName(aParameterName);
		}
	}
	# endregion

	#region SqlProcedureList (array di SqlProcedure)
	//=================================================================================
	public class SqlProcedureList : List<SqlProcedure>
	{
		//-----------------------------------------------------------------------------
		public SqlProcedureList()
		{
		}

		//-----------------------------------------------------------------------------
		public void Add(string aProcedureName)
		{
			if (string.IsNullOrEmpty(aProcedureName))
				return;

			if (!ProcedureExists(aProcedureName))
				base.Add(new SqlProcedure(aProcedureName));
		}

		//-----------------------------------------------------------------------------
		public SqlProcedure GetProcedureByName(string aProcedureName)
		{
			if (string.IsNullOrEmpty(aProcedureName))
				return null;

			foreach (SqlProcedure aProcedure in this)
			{
				if (String.Compare(aProcedure.Name, aProcedureName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return aProcedure;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public SqlProcedure GetProcedureByOracleName(string aProcedureName)
		{
			if (string.IsNullOrEmpty(aProcedureName))
				return null;

			foreach (SqlProcedure aProcedure in this)
			{
				if (String.Compare(aProcedure.OracleProcedureName, aProcedureName) == 0)
					return aProcedure;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool ProcedureExists(string aProcedureName)
		{
			foreach (SqlProcedure aProcedure in this)
			{
				if (String.Compare(aProcedure.OracleProcedureName, aProcedureName) == 0)
					return true;
			}
			return false;
		}
	}
	# endregion
}
