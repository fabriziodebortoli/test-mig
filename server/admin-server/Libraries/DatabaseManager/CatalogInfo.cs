using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microarea.Common.CoreTypes;
using NpgsqlTypes;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.AdminServer.Libraries.DatabaseManager
{
	#region Classe per la gestione degli indici (anche primarykey) di una singola tabella
	//============================================================================
	public class IndexInfo
	{
		private string			name		= string.Empty;
		private bool			primaryKey	= false;
		private List<string>	columns		= new List<string>();

		public string		Name		{ get { return name; } }
		public List<string> Columns		{ get { return columns; } }

		//----------------------------------------------------------------------
		public IndexInfo(string name, bool primaryKey)
		{
			this.name		= name;
			this.primaryKey = primaryKey;
		}
	}

	//============================================================================
	public class IndexArray
	{
		private List<IndexInfo> indexes	= new List<IndexInfo>();
		public	IndexInfo	PrimaryKeyIndex	= null;

		public	List<IndexInfo>  Indexes	{ get {	return indexes;	} }

		//----------------------------------------------------------------------
		public IndexArray()
		{
		}

		//----------------------------------------------------------------------
		public IndexArray(DataTable indexTable)
		{
			//@@TODOMICHI DataTable
			/*foreach (DataRow indexRow in indexTable.Rows)
			{
				string indexName = (string)indexRow[DBSchemaStrings.Name];
				
				bool primaryKey = false;
				primaryKey = (bool)(indexRow[DBSchemaStrings.PrimaryKey]);

				string columnName = (string)indexRow[DBSchemaStrings.ColumnName];

				AddIndex(indexName, primaryKey, columnName);
			}*/			
		}

		//----------------------------------------------------------------------
		public void AddIndex(string indexName, bool primaryKey, string columnName)
		{
			if (primaryKey)
			{
				if (PrimaryKeyIndex == null)
					PrimaryKeyIndex = new IndexInfo(indexName, primaryKey);

				PrimaryKeyIndex.Columns.Add(columnName);
			}
			else
			{
				IndexInfo index = GetIndex(indexName);
				if (index == null)
				{
					index = new IndexInfo(indexName, primaryKey);
					indexes.Add(index);
				}

				index.Columns.Add(columnName);	
			}
		}

		//----------------------------------------------------------------------
		public IndexInfo GetIndex(string indexName)
		{
			foreach (IndexInfo indexInfo in indexes)
				if (string.Compare(indexName, indexInfo.Name, StringComparison.OrdinalIgnoreCase) == 0)
					return indexInfo;

			return null;
		}
	}
	#endregion

	#region Classi per la gestione delle ForeignKeys: contengono tutte le informazioni (utilizzati dal Migrationkit)

	#region ForeignColumnInfo
	//=======================================================================================================
	public class ForeignColumnInfo
	{
		private string pkColumn = string.Empty;
		private string fkColumn = string.Empty;

		public string PKColumn { get { return pkColumn; } }
		public string FKColumn { get { return fkColumn; } }

		//----------------------------------------------------------------------
		public ForeignColumnInfo(string fkColumn, string pkColumn)
		{
			this.pkColumn = pkColumn;
			this.fkColumn = fkColumn;
		}
	}
	# endregion

	# region ForeignKeyInfo
	//=======================================================================================================
	public class ForeignKeyInfo 
	{
		private string name			= string.Empty;
		private string pkTableName	= string.Empty;
		private List<ForeignColumnInfo> columns = new List<ForeignColumnInfo>();
		
		public string		Name		{ get { return name;		} }
		public string		PKTableName	{ get { return pkTableName; } }
		public List<ForeignColumnInfo> Columns	{ get { return columns;		} }
		
		//----------------------------------------------------------------------
		public ForeignKeyInfo(string fkName, string tableName)
		{
			if (string.IsNullOrWhiteSpace(fkName) || string.IsNullOrWhiteSpace(tableName))
				throw new TBException("ForeignKeyInfo.Constructors");

			name		= fkName;
			pkTableName	= tableName;
		}

		//----------------------------------------------------------------------
		public void AddColumns(string fkColumn, string pkColumn)
		{
			columns.Add(new ForeignColumnInfo(fkColumn, pkColumn));
		}
	}
	# endregion

	# region ForeignKeyArray
	//====================================================================================================
	public class ForeignKeyArray : ArrayList
	{
		//----------------------------------------------------------------------
		public ForeignKeyArray()
		{
		}

		//----------------------------------------------------------------------
		public void Add(string fkName, string pkTableName, string fkColumn, string pkColumn)
		{
			ForeignKeyInfo fki = Add(fkName, pkTableName);
			if (fki != null)
				fki.AddColumns(fkColumn, pkColumn);
		}

		//----------------------------------------------------------------------
		public ForeignKeyInfo Add(string fkName, string pkTableName)
		{
			ForeignKeyInfo fki = GetForeignKey(fkName);

			if (fki == null)
			{
				fki = new ForeignKeyInfo(fkName, pkTableName);
				Add(fki);
			}
			return fki;
		}

		//----------------------------------------------------------------------
		public ForeignKeyInfo GetForeignKey(string fkName)
		{
			foreach (ForeignKeyInfo fkInfo in this)
				if (string.Compare(fkName, fkInfo.Name, StringComparison.OrdinalIgnoreCase) == 0)
					return fkInfo;

			return null;
		}
	}
	#endregion

	#endregion

	#region Class RefFKConstraint: contiene solo il nome delle foreignkey e delle tabelle che riferiscono la tabella in questione
	/// <summary>
	/// Utilizzata dall'ImportManager per disabilitare e ri-abilitare i constraint di FK
	/// </summary>
	//============================================================================
	public class RefFKConstraint
	{
		private int		idFKTable;
		public	string	FKTableName; // nome della tabella con la FK constraint
		public  string	Name;		 // nome del constraint

		public int		IdFKTable	{ get { return idFKTable; } }		
		
		//---------------------------------------------------------------------------
		public RefFKConstraint(string name, int idTable)
		{
			Name = name;
			idFKTable = idTable;
		}
		
		//---------------------------------------------------------------------------
		public RefFKConstraint(string name, string tableName)
		{
			Name = name;
			FKTableName = tableName;
		}
	}
	#endregion

	#region Class CatalogParameter: informazioni sul singolo parametro di una Stored Procedure
	//============================================================================
	public class CatalogParameter
	{
		#region Variables
		private string name = string.Empty;
		//private short ordinalPosition;
		private ParameterDirection direction = ParameterDirection.Input; //IN, OUT, INOUT
		private TBType providerType = null;
		private long maxLength = 0;
		private long maxOctetLength = 0;
		private string collationName = string.Empty;
		private Int16 numericPrecision = -1;
		private Int16 numericScale = -1;

		private DBMSType dbmsType = DBMSType.UNKNOWN;
		#endregion

		#region Properties
		public string Name { get { return name; } }
		//public short OrdinalPosition { get { return ordinalPosition; } }
		public ParameterDirection Direction { get { return direction; } }
		public TBType ProviderType { get { return providerType; } }
		public long MaxLength { get { return maxLength; } }
		public long MaxOctetLength { get { return maxOctetLength; } }
		public Int16 NumericPrecision { get { return numericPrecision; } }
		public Int16 NumericScale { get { return numericScale; } }
		#endregion

		#region Constructors
		//---------------------------------------------------------------------- 
		public CatalogParameter(string name, DBMSType type)
		{
			this.name = name;
			dbmsType = type;
		}

		//@@TODOMICHI DataTable
		//---------------------------------------------------------------------- 
		/*public CatalogParameter(DataRow paramRow, DBMSType dbmsType)
		{
			if (paramRow == null)
				throw new TBException(DatabaseManagerStrings.ErrNoColumnInfo);

			this.dbmsType = dbmsType;

			try
			{
				object o = paramRow["Name"];
				name = (string)(paramRow["Name"]);

				ordinalPosition = (short)paramRow["OrdinalPosition"];

				string paramMode = (string)paramRow["ParamMode"];
				if (string.Compare(paramMode, "IN", true) == 0)
					direction = ParameterDirection.Input;
				else
					if (string.Compare(paramMode, "OUT", true) == 0)
						direction = ParameterDirection.Output;
					else
						if (string.Compare(paramMode, "INOUT", true) == 0)
							direction = ParameterDirection.InputOutput;
						else
						if (string.Compare((string)paramRow["IsResult"], "YES", true) == 0)
							direction = ParameterDirection.ReturnValue;

				numericPrecision = Convert.ToInt16(paramRow["NumericPrecision"]);
				numericScale = Convert.ToInt16(paramRow["NumericScale"]);

				//i float vengono letti come se fossero dei numeric e la precisione 126 risulta maggione della max precisione == 38
				if (dbmsType == DBMSType.ORACLE && numericPrecision == 126)
					providerType = TBDatabaseType.GetTBType(dbmsType, 29);
				else
					providerType = TBDatabaseType.GetTBType(dbmsType, (int)paramRow["ProviderType"]);

				maxLength = Convert.ToInt32(paramRow["MaxLength"]);
				maxOctetLength = Convert.ToInt32(paramRow["MaxOctetLength"]);

				collationName = paramRow["CollationName"].ToString();				
			}
			catch (InvalidCastException e)
			{
				throw new TBException(e.Message);
			}
			catch (Exception e)
			{
				throw new TBException(e.Message);
			}
		}*/
		#endregion
	}
	#endregion 

	#region Classe CatalogColumn : mappa una colonna di database
	//==================================================================
	[Serializable]
	public class CatalogColumn : ISerializable
	{
		const string NAME = "name";
		const string TYPE = "type";
		const string KEY = "key";

		const int intPrecision = 6;
		const int longPrecision = 10;
		const int doublePrecision = 15;
		
		#region Variables
		private	string	name = string.Empty;
		private	string	systemType = string.Empty;
		private TBType	providerType = null;
		private string  dataTypeName = string.Empty;
		private int		columnSize = 0;
		private bool	isKey = false;
		private bool	isUnique = false;
		private bool	isNullable = false;
		private int		numericPrecision = -1;
		private int		numericScale = -1;
		private bool	isVirtual	= false;	
		
		private	bool	hasDefault		= false;
		private	string	columnDefault	= string.Empty;
		private bool	isAutoIncrement	= false;
		private string  collationName = string.Empty;
		private bool	isDataText = false;

		private DBMSType dbmsType = DBMSType.UNKNOWN;
		public  bool	Selected  = false;
		#endregion

		#region Properties
		public string Name				{ get { return name; } }
		public string SystemType		{ get { return systemType; } }	//Maps to the .NET Framework type of the column
		public TBType ProviderType		{ get { return providerType; } } //The indicator of the column's provider data type	
		public string DataTypeName		{ get { return dataTypeName; } }
		public int ColumnSize			{ get { return columnSize; } set { columnSize = value; } }
		public bool	IsKey				{ get { return isKey; }	set { isKey = value; } }
		public bool IsUnique			{ get { return isUnique; } }			
		public bool	IsNullable			{ get { return isNullable; } }
		public int NumericPrecision		{ get { return numericPrecision; } }
		public int NumericScale			{ get { return numericScale; } }
		public bool IsVirtual			{ get { return isVirtual; } set { isVirtual = value; } }
		public bool HasDefault			{ get { return hasDefault; } set { hasDefault = value; } }		
		public string ColumnDefault		{ get { return columnDefault; } set { columnDefault = value; } }
		public string CollationName		{ get { return collationName; } }	
		public bool IsAutoIncrement		{ get { return isAutoIncrement;	} }
		public bool IsDataText			{ get { return isDataText; } }
		#endregion

		#region Constructors
		//--------------------------------------------------------------------------
		public CatalogColumn(SerializationInfo info, StreamingContext context)
		{
			name = info.GetString(NAME);
			systemType = info.GetString(TYPE);
			isKey = info.GetBoolean(KEY);
		}

		//----------------------------------------------------------------------
		public CatalogColumn(string name, DBMSType type)
		{
			this.name = name;
			dbmsType = type;
		}

		//---------------------------------------------------------------------------		
		public CatalogColumn(string columnName, DataType dataType, int len, DBMSType dbmsType)
		{
			this.dbmsType = dbmsType;
			name = columnName;
			isVirtual = true;
			SetTypeAttribute(dataType, len);
		}

		//---------------------------------------------------------------------------		
		public CatalogColumn(TBColumn column, DBMSType dbmsType)
		{
			if (column == null)
				throw new TBException(DatabaseManagerStrings.ErrNoColumnInfo);
			
			this.dbmsType = dbmsType;

			try
			{
				name			= column.Name;

				isNullable		= column.IsNullable;
				numericPrecision= column.NumericPrecision;
				numericScale	= column.NumericScale;
				columnSize		= column.ColumnSize;

				isKey			= column.IsKey;
				isAutoIncrement	= column.IsAutoIncrement;

				// nel caso contenga il Namespace completo ritorno il solo basename del tipo
				// Es: System.Int32 torna Int32
				if (dbmsType == DBMSType.SQLSERVER)
				{
					providerType = TBDatabaseType.GetTBType(dbmsType, column.DataType);
					if (providerType.SqlDbType == SqlDbType.Text || providerType.SqlDbType == SqlDbType.NText)
						isDataText = true;
				}
				//else if (dbmsType == DBMSType.POSTGRE)
					//providerType = TBDatabaseType.GetTBTypePostgre(dbmsType, columnRow["ProviderType"].ToString());

				dataTypeName = TBDatabaseType.GetDBDataType(providerType, dbmsType);

				hasDefault		= column.HasDefault;
				columnDefault	= column.ColumnDefault;
				collationName	= column.CollationName;
			}
			catch (InvalidCastException e)
			{
				throw new TBException(e.Message);
			}
		}
		#endregion

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(NAME, name);
			info.AddValue(TYPE, systemType);
			info.AddValue(KEY, isKey);
		}

		//used to set local data attribute
		//---------------------------------------------------------------------------		
		public void SetTypeAttribute(DataType dataType, int len)
		{
			columnSize = 0;
			numericPrecision = 0;
			systemType = dataType.GetType().ToString();
			providerType = TBDatabaseType.GetTBType(dataType, dbmsType);

			if (dataType == DataType.String)
			{
				columnSize = len;
				return;
			}

			if (dataType == DataType.Integer)
			{
				columnSize = 2;
				numericPrecision = intPrecision;
				return;
			}

			if (dataType == DataType.Long || dataType == DataType.Enum)
			{
				columnSize = 4;
				numericPrecision = longPrecision;
				return;
			}

			if (dataType == DataType.Double || dataType == DataType.Money ||
				dataType == DataType.Quantity || dataType == DataType.Percent)
			{
				columnSize = 8;
				numericPrecision =  doublePrecision;
				return;
			}

			if (dataType == DataType.Date)
			{
				columnSize = 4;
				numericPrecision = 19;
				return;
			}

			if (dataType == DataType.Bool)
			{
				columnSize = 1;
				return;
			}

			if (dataType == DataType.Guid)
			{
				columnSize = 16;
				return;
			}
		}

		// Questa routine serve essenzialmente a Woorm per proporre all'utente tutte
		// le conversioni in DataObj possibili a partire dal tipo SQL della colonna.
		// Il valore di default e' il primo elemento del vettore (che deve essere 
		// allocato fuori, ma puo' non essere inizializzato).
		//---------------------------------------------------------------------------
		public List<DataType> GetDataObjTypes()
		{
			List<DataType> dataTypes = new List<DataType>();

			if (dbmsType == DBMSType.SQLSERVER)
			{
				switch (providerType.SqlDbType)
				{
					case SqlDbType.SmallInt:
						dataTypes.Add(DataType.Integer);
						break;
					case SqlDbType.Int:
						dataTypes.Add(DataType.Long);
						dataTypes.Add(DataType.Enum);
						dataTypes.Add(DataType.Integer);
						break;
					case SqlDbType.Float:
						dataTypes.Add(DataType.Double);
						dataTypes.Add(DataType.Quantity);
						dataTypes.Add(DataType.Money);
						dataTypes.Add(DataType.Percent);
						break;
					case SqlDbType.Timestamp:
						dataTypes.Add(DataType.DateTime);
						break;
					case SqlDbType.Decimal:
						if (numericScale > 0)
						{
							dataTypes.Add(DataType.Double);
							dataTypes.Add(DataType.Quantity);
							dataTypes.Add(DataType.Money);
							dataTypes.Add(DataType.Percent);
							break;
						}
						if (numericPrecision > intPrecision)
						{
							dataTypes.Add(DataType.Long);
							dataTypes.Add(DataType.Enum);
							dataTypes.Add(DataType.Double);
							dataTypes.Add(DataType.Quantity);
							dataTypes.Add(DataType.Money);
							dataTypes.Add(DataType.Percent);
							break;
						}

						dataTypes.Add(DataType.Integer);
						dataTypes.Add(DataType.Long);
						dataTypes.Add(DataType.Enum);
						dataTypes.Add(DataType.Double);
						dataTypes.Add(DataType.Quantity);
						dataTypes.Add(DataType.Money);
						dataTypes.Add(DataType.Percent);
						break;

					case SqlDbType.VarChar:
					case SqlDbType.NVarChar:
						dataTypes.Add(DataType.String);
						dataTypes.Add(DataType.Bool);
						break;

					case SqlDbType.Text:
					case SqlDbType.NText:
						dataTypes.Add(DataType.Text);
						break;

					case SqlDbType.UniqueIdentifier:
						dataTypes.Add(DataType.Guid);
						break;

					case SqlDbType.VarBinary:
						dataTypes.Add(DataType.Blob);
						break;					
				}
			}

            if (dbmsType == DBMSType.POSTGRE)
            {
                switch (providerType.PostgreType)
                {
                    case NpgsqlDbType.Smallint:
                        dataTypes.Add(DataType.Integer);
                        break;
                    case NpgsqlDbType.Integer:
                        dataTypes.Add(DataType.Long);
                        dataTypes.Add(DataType.Enum);
                        dataTypes.Add(DataType.Integer);
                        break;
                    case NpgsqlDbType.Double:
                        dataTypes.Add(DataType.Double);
                        dataTypes.Add(DataType.Quantity);
                        dataTypes.Add(DataType.Money);
                        dataTypes.Add(DataType.Percent);
                        break;
                    case NpgsqlDbType.Timestamp:
                        dataTypes.Add(DataType.DateTime);
                        break;
                    case NpgsqlDbType.Numeric:
                        if (numericScale > 0)
                        {
                            dataTypes.Add(DataType.Double);
                            dataTypes.Add(DataType.Quantity);
                            dataTypes.Add(DataType.Money);
                            dataTypes.Add(DataType.Percent);
                            break;
                        }
                        if (numericPrecision > intPrecision)
                        {
                            dataTypes.Add(DataType.Long);
                            dataTypes.Add(DataType.Enum);
                            dataTypes.Add(DataType.Double);
                            dataTypes.Add(DataType.Quantity);
                            dataTypes.Add(DataType.Money);
                            dataTypes.Add(DataType.Percent);
                            break;
                        }
                        dataTypes.Add(DataType.Integer);
                        dataTypes.Add(DataType.Long);
                        dataTypes.Add(DataType.Enum);
                        dataTypes.Add(DataType.Double);
                        dataTypes.Add(DataType.Quantity);
                        dataTypes.Add(DataType.Money);
                        dataTypes.Add(DataType.Percent);
                        break;

                    case NpgsqlDbType.Varchar:
                        dataTypes.Add(DataType.String);
                        dataTypes.Add(DataType.Bool);
                        break;

                    case NpgsqlDbType.Text:
                        dataTypes.Add(DataType.Text);
                        break;

                    case NpgsqlDbType.Uuid:
                        dataTypes.Add(DataType.Guid);
                        break;

                    case NpgsqlDbType.Bytea:
                        dataTypes.Add(DataType.Blob);
                        break;
                }
            }

			return dataTypes;
		}

		//---------------------------------------------------------------------------		
		public bool HasLength()
		{
			return TBDatabaseType.HasLength(providerType, dbmsType);
		}

		/// <summary>
		/// restituisce TRUE se il providerType è di tipo alfabetico
		/// </summary>
		//---------------------------------------------------------------------------		
		public bool IsCharType()
		{
			return TBDatabaseType.IsCharType(providerType, dbmsType);
		}

		/// <summary>
		/// restituisce TRUE se il providerType è di tipo numerico
		/// </summary>
		//---------------------------------------------------------------------------		
		public bool IsNumberType()
		{
			return TBDatabaseType.IsNumericType(providerType, dbmsType);
		}
		
		/// <summary>
		/// restituisce TRUE se il providerType è di tipo DateTime
		/// </summary>
		//---------------------------------------------------------------------------		
		public bool IsDateTimeType()
		{
			return TBDatabaseType.IsDateTimeType(providerType, dbmsType);
		}

		/// <summary>
		/// restituisce TRUE se il providerType è di tipo GUID
		/// </summary>
		//---------------------------------------------------------------------------		
		public bool IsGUIDType()
		{
			return TBDatabaseType.IsGUIDType(providerType, dbmsType, columnSize);
		}

		/// <summary>
		/// restituisce TRUE se il providerType ammette la precisione FALSE altrimenti
		/// </summary>
		//---------------------------------------------------------------------------		
		public bool HasPrecision()
		{
			return TBDatabaseType.HasPrecision(providerType, dbmsType);
		}	
		
		/// <summary>
		/// restituisce TRUE se il providerType ammette la scala FALSE altrimenti
		/// </summary>
		//---------------------------------------------------------------------------		
		public bool HasScale()
		{
			return TBDatabaseType.HasScale(providerType, dbmsType);
		}	

		//---------------------------------------------------------------------------		
		public string GetDBDataType()
		{
			return dataTypeName; //TBDatabaseType.GetDBDataType(providerType, dbmsType);
		}

		/// <summary>
		/// restituisce il tipo comprensivo dell'eventuale lunghezza (es: varchar(2), number(6)..)
		/// </summary>
		//---------------------------------------------------------------------------
		public string GetCompleteDBType()
		{
			string colType = string.Empty;
			try
			{
				colType = dataTypeName; // TBDatabaseType.GetDBDataType(providerType, dbmsType);

				if (HasLength() && columnSize > 0)
					return colType += string.Format("({0})", columnSize);

				if (HasPrecision() && numericPrecision > 0)
				{
					return colType += (HasScale() && numericScale > 0)
						? string.Format("({0},{1})", numericPrecision, numericScale)
						: string.Format("({0})", numericPrecision);		
				}
			}
			catch (TBException e)
			{
				Debug.Fail(e.Message);
			}
			
			return colType;		
		}

		//----------------------------------------------------------------------
		public string CreateSqlColumnScript(bool withDefault)
		{
			string dbType = GetDBDataType();
			string column = string.Format("[{0}] [{1}]", name, dbType);

			if (HasLength())
				column += string.Format(" ({0})", columnSize.ToString());

			column += (isNullable) ? " NULL" : " NOT NULL";

			if (hasDefault && withDefault)
				column += string.Format(" DEFAULT {0}", columnDefault);
			
			return column;
		}

        //----------------------------------------------------------------------
        public string CreatePostgreColumnScript(bool withDefault)
        {
            string dbType = GetDBDataType();
            string column = string.Format("{0} {1}", name, dbType);

            if (HasLength())
                column += string.Format(" ({0})", columnSize.ToString());

            if (isNullable)
                column += " NULL";
            else
                column += " NOT NULL";

            if (hasDefault && withDefault)
                column += string.Format(" DEFAULT {0}", columnDefault);

            return column;
        }

		/// <summary>
		/// Genera lo script di una colonna, da inserire in un contesto di CREATE o ALTER TABLE
		/// (lo script è differente a seconda del tipo di DBMS connesso)
		/// </summary>
		//----------------------------------------------------------------------
		public string ToSql(bool withDefault)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				return CreateSqlColumnScript(withDefault);
			else if (dbmsType == DBMSType.POSTGRE)
                return CreatePostgreColumnScript(withDefault);

			throw (new TBException("CatalogColumn.CreateColumnScript: " + string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
		}
	}
	#endregion

	#region Class CatalogTableInfo
	//=================================================================================================
	[Serializable]
	public class CatalogTableInfo : ISerializable
	{
		const string TABLENAME = "name";
		
		public string TableName;
		protected DBMSType dbmsType = DBMSType.UNKNOWN;
		public bool HasAutoIncrementalCol = false;
		public bool HasDataTextColumn = false;

		protected ArrayList columnsInfo = null; //informazioni sulle colonne (tabelle, view, stored procedure)
		protected ArrayList paramsInfo = null; //informazioni sui parametri delle stored procedure

		protected IndexArray indexesInfo = null; //informazioni sugl indici presenti nella tabella
		protected ForeignKeyArray fKConstraintsInfo = null; //tutte le informazioni dei constraints di FK
		protected ArrayList refFKConstraints = null; 		//per i constraint di FK riferiti alla tabella

		public ArrayList ColumnsInfo { get { return columnsInfo; } set { columnsInfo = value; } }
		public IndexArray IndexesInfo { get { return indexesInfo; } set { indexesInfo = value; } }
		
		public ForeignKeyArray FKConstraintsInfo { get { return fKConstraintsInfo; } }
		public ArrayList RefFKConstraints { get { return refFKConstraints; } }

		//--------------------------------------------------------------------------
		public CatalogTableInfo(string name)
		{
			TableName = name;
		}
				
		//---------------------------------------------------------------------------
		public CatalogTableInfo(SerializationInfo info, StreamingContext context)
		{
			TableName = info.GetString(TABLENAME);
		}
		
		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(TABLENAME, TableName);
		}
				
		#region Gestione ColumnsInfo
		/// <summary>
		/// caricamento delle ColumnInfo di una tabella specificata, con o senza l'informazioni di chiave primaria
		/// </summary>
		//---------------------------------------------------------------------------
		public void LoadColumnsInfo(TBConnection tbConnect, bool withKeyInfo)
		{
			if (columnsInfo != null)
				return; 

			if (tbConnect.IsSqlConnection()) 
				dbmsType = DBMSType.SQLSERVER; 
            else if (tbConnect.IsPostgreConnection())
                dbmsType = DBMSType.POSTGRE; 
			else
				throw(new TBException("CatalogTableEntry.LoadColumnsInfo: Invalid connection"));

			columnsInfo = new ArrayList();
			bool autoIncrementalColFound = false;
			bool dataTextColumnFound = false;
			
			try
			{
				TBDatabaseSchema dbSchema = new TBDatabaseSchema(tbConnect);
				TBTable tbTable = dbSchema.GetTableSchema(TableName, withKeyInfo);

				foreach (TBColumn column in tbTable.Columns)
				{
					CatalogColumn cc = new CatalogColumn(column, dbmsType);
					columnsInfo.Add(cc);

					if (cc.IsAutoIncrement)
						autoIncrementalColFound = true;

					if (cc.IsDataText)
						dataTextColumnFound = true;
				}

                if (dbmsType == DBMSType.SQLSERVER || dbmsType == DBMSType.POSTGRE)
					if (autoIncrementalColFound)
						this.HasAutoIncrementalCol = true;

				if (dataTextColumnFound)
					this.HasDataTextColumn = true;
			}	
			catch (TBException)
			{
				throw;
			}
		}
		
		/// <summary>
		/// ricarica le ColumnInfo della tabella, con o senza l'informazioni di chiave primaria
		/// </summary>
		//---------------------------------------------------------------------------
		public void RefreshColumns(TBConnection tbConnect, bool withKeyInfo)
		{
			columnsInfo = null;
			LoadColumnsInfo(tbConnect, withKeyInfo);
		}

		/// <summary>
		/// data una colonna vengono restituite le sue informazioni
		/// </summary>
		//---------------------------------------------------------------------------
		public CatalogColumn GetColumnInfo(string columnName)
		{
			if (columnsInfo == null)
				return null;

			foreach (CatalogColumn column in columnsInfo)
				if (string.Compare(column.Name, columnName, StringComparison.OrdinalIgnoreCase) == 0)
					return column;

			return null;
		}
		#endregion

		#region Gestione Indici
		/// carico le informazioni relativi agli indici comprensivo di chiave primaria e per ciascun indice
		/// le informazioni relative alle colonne
		//---------------------------------------------------------------------------
		public void LoadSqlIndexes(TBConnection tbConnection)
		{
			TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnection);

			try
			{
				indexesInfo = dataSchema.LoadSqlIndexes(TableName);
			}
			catch (TBException)
			{
				throw;
			}

			//aggiorno le informazioni relative alle chiavi primarie\indici univoci
			if (indexesInfo != null && indexesInfo.PrimaryKeyIndex != null && indexesInfo.PrimaryKeyIndex.Columns != null)
			{
				foreach (string colName in indexesInfo.PrimaryKeyIndex.Columns)
				{
					CatalogColumn colInfo = GetColumnInfo(colName);
					if (colInfo != null)
						colInfo.IsKey = true;
				}
			}
		}

		/// <summary>
		/// legge dalle tabelle di sistema le informazioni relative agli indici di Postgre 
		/// </summary>
		//---------------------------------------------------------------------------
        public void LoadPostgreIndexes(TBConnection tbConnection)
        {
            TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnection);

            try
            {
				indexesInfo = dataSchema.LoadPostgreIndexesInfo(TableName);

                //aggiorno le informazioni relative alle chiavi primarie\indici univoci
                if (indexesInfo.PrimaryKeyIndex != null && indexesInfo.PrimaryKeyIndex.Columns != null)
                {
                    CatalogColumn colInfo = null;
                    foreach (string colName in indexesInfo.PrimaryKeyIndex.Columns)
                    {
                        colInfo = GetColumnInfo(colName);
                        if (colInfo != null)
                            colInfo.IsKey = true;
                    }
                }
            }
            catch (TBException)
            {
                throw;
            }
        }
		#endregion 

		#region Gestione Default
		// forse non serve piu'
		//---------------------------------------------------------------------------
		/*public void LoadDefaults(TBConnection tbConnect)
		{
			//@@TODOMICHI DataTable
			TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnect);
			DefaultDataTable dataTable = null;

			try
			{
				dataTable = dataSchema.LoadDefaults(TableName);
			}
			catch (TBException)
			{
				throw;
			}

			foreach (DataRow row in dataTable.Rows)
			{
				CatalogColumn column = GetColumnInfo(row[DBSchemaStrings.ColumnName].ToString());
				if (column != null)
				{
					column.HasDefault = (bool)row[DBSchemaStrings.HasDefault];
					column.ColumnDefault = row[DBSchemaStrings.Default].ToString();
				}
			}
		}*/
		#endregion
			
		#region Gestione PrimaryKey
		/// <summary>
		/// ritorna un array di primary keys
		/// </summary>
		//---------------------------------------------------------------------------
		public void GetPrimaryKeys(ref StringCollection keys)
		{
			if (columnsInfo == null)
				return;

			foreach (CatalogColumn columnInfo in columnsInfo)
			{
				if (columnInfo.IsKey)
					keys.Add(columnInfo.Name);
			}
		}
		#endregion

		#region Gestione ForeignKey

		// legge dalle tabelle di sistema le informazioni relative alle foreignkey su SQLServer
		//---------------------------------------------------------------------------
		public void LoadSqlFkConstraintsInfo(TBConnection tbConnection)
		{
			TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnection);

			try
			{
				fKConstraintsInfo = dataSchema.LoadSqlFkConstraintsInfo(TableName);
			}
			catch (TBException)
			{
				throw;
			}
		}

		/// <summary>
		/// carica le informazioni relative alla foreign key constraints relative alla tabella (la lettura viene effettuata
		/// direttamente sulle tabelle di sistema)
		/// </summary>	
		//---------------------------------------------------------------------------
		public void LoadFKConstraintsName(TBConnection tbConnect)
		{
			if (fKConstraintsInfo != null)
				return;			

			TBDatabaseSchema dbSchema = new TBDatabaseSchema(tbConnect);
			
			try
			{
				fKConstraintsInfo = dbSchema.LoadFKConstraints(TableName);
			}
			catch (TBException e)
			{				
				Debug.Fail(e.Message);
			}
		}
	
		/// <summary>
		/// carica le informazioni relative alla foreign key constraints che riferiscono
		/// alla tabella (la lettura viene effettuata direttamente sulle tabelle di sistema)
		/// utilizzato dall'ImportManager
		/// </summary>
		//---------------------------------------------------------------------------
		public void LoadRefFKConstraints(TBConnection tbConnect)
		{
			if (refFKConstraints != null)
				return;
		
			TBDatabaseSchema dbSchema = new TBDatabaseSchema(tbConnect);
			
			try
			{
				ForeignKeyArray fkArray = dbSchema.LoadRefFKConstraints(TableName);

				if (fkArray != null)
				{
					refFKConstraints = new ArrayList();
					foreach (ForeignKeyInfo fkInfo in fkArray)
						refFKConstraints.Add(new RefFKConstraint(fkInfo.Name, fkInfo.PKTableName));
				}
			}
			catch (TBException e)
			{				
				Debug.Fail(e.Message);
			}
		}

		//---------------------------------------------------------------------------
		public void LoadPostgreFKConstraintsInfo(TBConnection tbConnection)
        {
            TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnection);

            try
            {
				fKConstraintsInfo = dataSchema.LoadPostgreFkConstraintsInfo(TableName);
            }
            catch (TBException)
            {
                throw;
            }
        }

		#endregion
	}
	# endregion

	#region Class CatalogEntry
	//=================================================================================================
	[Serializable]
	public class CatalogEntry : ISerializable
	{
		const string TABLENAME = "name";		

		protected DBMSType dbmsType = DBMSType.UNKNOWN;	
	
		//eventuale applicazione e modulo di appartenenza
		public string Application = string.Empty; 
		public string Module = string.Empty;

		//utilizzati durante la generazione dei dati di default
		public bool Optional = false;
		public bool Append = false;
		public bool Selected = false;		

		public bool Valid = true;
		protected CatalogTableInfo catalogTableInfo;

		//---------------------------------------------------------------------------
		public string TableName { get ;  set ; }
		public ArrayList ColumnsInfo { get { return catalogTableInfo.ColumnsInfo; } set { catalogTableInfo.ColumnsInfo = value; } }
		public IndexArray IndexesInfo { get { return catalogTableInfo.IndexesInfo; } set { catalogTableInfo.IndexesInfo = value; } }
		public ForeignKeyArray FKConstraintsInfo { get { return catalogTableInfo.FKConstraintsInfo; } }
		public ArrayList RefFKConstraints { get { return catalogTableInfo.RefFKConstraints; } }
		public CatalogTableInfo CatalogTableInfo { get { return catalogTableInfo; } }
	
		//---------------------------------------------------------------------------
		public CatalogEntry(string name, DBMSType dbmsType)
		{
			TableName = name;
			this.dbmsType = dbmsType;
	
			catalogTableInfo = new CatalogTableInfo(name);
		}

		//---------------------------------------------------------------------------
		public CatalogEntry(SerializationInfo info, StreamingContext context)
		{
			TableName = info.GetString(TABLENAME);
		
			catalogTableInfo = new CatalogTableInfo(info, context);
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(TABLENAME, TableName);
		}

		#region Funzioni di load
		//--------------------------------------------------------------------------
		public void LoadColumnsInfo(TBConnection tbConnect, bool withKeyInfo)
		{
			catalogTableInfo.LoadColumnsInfo(tbConnect, withKeyInfo);
		}
		
		//--------------------------------------------------------------------------
		public void LoadFKConstraintsName(TBConnection tbConnect)
		{
			catalogTableInfo.LoadFKConstraintsName(tbConnect);	
		}
		
		//--------------------------------------------------------------------------
		public void LoadRefFKConstraints(TBConnection tbConnect)
		{
			catalogTableInfo.LoadRefFKConstraints(tbConnect);
		}
		
		//--------------------------------------------------------------------------
		public void GetPrimaryKeys(ref StringCollection keys)
		{
			catalogTableInfo.GetPrimaryKeys(ref keys);
		}

		/// <summary>
		/// Richiamata dal metodo LoadAllInformationSchema di CatalogInfo
		/// VIENE UTILIZZATA SOLO DAL PLUGIN DELLA ROWSECURITY
		/// </summary>
		//--------------------------------------------------------------------------
		public void LoadAllInformationSchema(TBConnection connection, bool withKey)
		{
			try
			{
				// le informazioni di chiave primaria le inserisco le so dalla lettura degli indici
				catalogTableInfo.LoadColumnsInfo(connection, withKey);

                if (connection.IsPostgreConnection())
                {
                    catalogTableInfo.LoadPostgreIndexes(connection);
                    catalogTableInfo.LoadPostgreFKConstraintsInfo(connection);
                }
				else
				{
					catalogTableInfo.LoadSqlIndexes(connection);
					catalogTableInfo.LoadSqlFkConstraintsInfo(connection);
				}

				// questo non serve piu' perche' i default li carico gia' nella LoadColumnsInfo (controllare!)
				//catalogTableInfo.LoadDefaults(connection);
			}
			catch (TBException)
			{
				throw;
			}					
		}
		#endregion

		#region Funzioni sulla gestione delle colonne
		//---------------------------------------------------------------------------
		public void RefreshColumns(TBConnection tbConnect, bool withKeyInfo)
		{
			catalogTableInfo.RefreshColumns(tbConnect, withKeyInfo);
		}

		/// <summary>
		/// data una colonna viene ritornate le sue informazioni
		/// </summary>
		//---------------------------------------------------------------------------
		public CatalogColumn GetColumnInfo(string columnName)
		{
			return catalogTableInfo.GetColumnInfo(columnName);
		}

		/// <summary>
		/// data una colonna ritorna il suo system type
		/// </summary>
		//---------------------------------------------------------------------------
		public string GetColumnType(string columnName)
		{
			CatalogColumn columnInfo = GetColumnInfo(columnName);
			if (columnInfo == null)
				return null;

			return columnInfo.SystemType;
		}

		/// <summary>
		/// data una colonna ritorna la sua lunghezza
		/// </summary>
		//---------------------------------------------------------------------------
		public int GetColumnLength(string columnName)
		{
			CatalogColumn columnInfo = GetColumnInfo(columnName);
			if (columnInfo == null)
				return 0;

			return columnInfo.ColumnSize;
		}

		/// <summary>
		/// data una colonna ritorna la sua precisione
		/// </summary>
		//---------------------------------------------------------------------------
		public int GetColumnPrecision(string columnName)
		{
			CatalogColumn columnInfo = GetColumnInfo(columnName);
			if (columnInfo == null)
				return 0;

			return columnInfo.NumericPrecision;
		}

		/// <summary>
		/// restituisce il tipo di database comprensivo dell'eventuale lunghezza 
		/// (es: char(n), varchar(n), varchar2(n) number(6)......)
		/// </summary>
		//---------------------------------------------------------------------------
		public string GetCompleteDBType(string columnName)
		{
			CatalogColumn columnInfo = GetColumnInfo(columnName);

			try
			{
				if (columnInfo != null)
					return columnInfo.GetCompleteDBType();
			}
			catch (TBException e)
			{
				Debug.Fail(e.Message);
			}

			return string.Empty;
		}
		#endregion
	}
	#endregion
	
	#region Class CatalogTableEntry
	/// <summary>
	/// Classe derivata da CatalogEntry ed arricchita delle informazioni necessarie 
	/// Tale classe e' utilizzata dal DataManager, dal processo di Migrazione, dal RowSecurityLayer
	/// </summary>
	//============================================================================
	[Serializable]
	public class CatalogTableEntry : CatalogEntry
	{
		public string WhereClause = string.Empty;
		public int RecordCount = 0; // numero di record della tabella
		public string Namespace = string.Empty; // full namespace della tabella (RowSecurityLayer)

		private StringCollection selectedColumnsList = new StringCollection();

		//---------------------------------------------------------------------------
		public bool HasAutoIncrementalCol { get { return catalogTableInfo.HasAutoIncrementalCol; } }
		public bool HasDataTextColumn { get { return catalogTableInfo.HasDataTextColumn; } }

		public StringCollection	SelectedColumnsList { get { return selectedColumnsList; } }
		public DBMSType DBMSType { get { return dbmsType; }}

		//---------------------------------------------------------------------------
		public CatalogTableEntry(string name, DBMSType dbmsType)
			:
			base(name, dbmsType)
		{}

		/// <summary>
		/// Add delle colonne nell'array delle colonne selezionate (con check dell'esistenza)
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddSelectedColumn(string str)
		{
			// se l'array contiene già quella stringa non la inserisco una seconda volta
			if (!selectedColumnsList.Contains(str))
				selectedColumnsList.Add(str);
		}
			
		#region Funzioni utilizzate dal Migration Kit
		// Disabilita i check delle FK presenti nell'array
		//---------------------------------------------------------------------------
		public void DisableCheckOnForeignKey(TBCommand fkCommand)
		{
            if (catalogTableInfo.FKConstraintsInfo == null && !fkCommand.Connection.IsPostgreConnection())
				return;

			try
			{
				if (fkCommand.Connection.IsPostgreConnection())
				{

					fkCommand.CommandText = "SET SESSION session_replication_role = replica;";
					fkCommand.ExecuteNonQuery();
					fkCommand.CommandText = "show session_replication_role";
					string k = fkCommand.ExecuteScalar().ToString();
					return;
				}

				if (fkCommand.Connection.IsSqlConnection())
				{
					foreach (ForeignKeyInfo fkInfo in catalogTableInfo.FKConstraintsInfo)
					{
						fkCommand.CommandText = string.Format("ALTER TABLE {0} NOCHECK CONSTRAINT {1}", TableName, fkInfo.Name);
						fkCommand.ExecuteNonQuery();
					}
				}
			}
			catch (TBException)
			{
				throw;
			}
		}

		/// <summary>
		/// Estrae tutte le righe orfane della tabella che non rispettano il constraint di ForeignKey passato come argomento
		/// select TableName.pkSeg1, TableName.pkSeg2..., TableName.fkSeg1, TableName.fkSeg2 from TableName 
		/// where not exist (Select PKTableName.pkSeg1 from PKTableName where TableName.fkSeg1 = PKTableName.pkSeg1, TableName.fkSeg2 = PKTableName.pkSeg2
		///</summary>
		/// <param name="connection"></param>
		/// <param name="fkInfo">constraint di ForeignKey da controllare</param>
		/// <param name="error"></param>
		//---------------------------------------------------------------------------
		private void GetNoIntegrityRows(TBConnection connection, ForeignKeyInfo fkInfo, ref string error)
		{
			if (fkInfo.Columns == null || fkInfo.Columns.Count <= 0)
				return;

			if (catalogTableInfo.IndexesInfo == null || catalogTableInfo.IndexesInfo.PrimaryKeyIndex == null ||
				catalogTableInfo.IndexesInfo.PrimaryKeyIndex.Columns == null || catalogTableInfo.IndexesInfo.PrimaryKeyIndex.Columns.Count <= 0)
				return;

			//devo considerare solo i segmenti di primarykey e di foreignkey (è inutile che estraggo tutte le righe)
			string selectCols = string.Empty;
			string colToShow = string.Empty;

			foreach (string pkSegName in catalogTableInfo.IndexesInfo.PrimaryKeyIndex.Columns)
			{
				if (selectCols.Length > 0)
					selectCols += ", ";

				switch (dbmsType)
				{
					case DBMSType.SQLSERVER:
						selectCols += string.Format("[{0}].[{1}]", TableName, pkSegName);
						break;
                    case DBMSType.POSTGRE:
                        selectCols += string.Format("{0}.{1}", TableName, pkSegName);
                        break;
				}

				colToShow += string.Format(" {0} ", pkSegName);
			}

			string joinText = string.Empty;
			foreach (ForeignColumnInfo colInfo in fkInfo.Columns)
			{
				if (joinText.Length > 0)
					joinText += " AND ";

				switch (dbmsType)
				{
					case DBMSType.SQLSERVER:
						joinText += string.Format("[{0}].[{1}] = [{2}].[{3}]", TableName, colInfo.FKColumn, fkInfo.PKTableName, colInfo.PKColumn);
						break;
                    case DBMSType.POSTGRE:
                        joinText += string.Format("{0}.{1} = {2}.{3}", TableName, colInfo.FKColumn, fkInfo.PKTableName, colInfo.PKColumn);
                        break;
				}

				// inserisco il segmento di FK nella select solo se non è primarykey
				if (!catalogTableInfo.IndexesInfo.PrimaryKeyIndex.Columns.Contains(colInfo.FKColumn))
				{
					selectCols += ", ";

					switch (dbmsType)
					{
						case DBMSType.SQLSERVER:
							selectCols += string.Format("[{0}].[{1}]", TableName, colInfo.FKColumn);
							break;
                        case DBMSType.POSTGRE:
                            selectCols += string.Format("{0}.{1}", TableName, colInfo.FKColumn);
                            break;
					}

					colToShow += string.Format(" {0} ", colInfo.FKColumn);
				}
			}

			string cmdText = string.Format("SELECT {0} FROM {1} WHERE NOT EXISTS (SELECT {2} FROM {3} WHERE {4}) ",
				selectCols, TableName, ((ForeignColumnInfo)fkInfo.Columns[0]).PKColumn, fkInfo.PKTableName, joinText);

			string cmdCount = string.Format("SELECT COUNT(*) FROM {0} WHERE NOT EXISTS (SELECT {1} FROM {2} WHERE {3}) ",
				TableName, ((ForeignColumnInfo)fkInfo.Columns[0]).PKColumn, fkInfo.PKTableName, joinText);

			TBCommand command = new TBCommand(cmdCount, connection);
			int rowsAffected = 0;

			try
			{
				rowsAffected = command.ExecuteTBScalar();

				// invece di elencare le righe mettiamo solo la query da effettuare e le righe interessate,
				// per evitare i rallentamenti se sono presenti nella tabella molte righe orfane
				if (rowsAffected > 0)
				{
					error += "\r\n";
					error += string.Format(DatabaseManagerStrings.NoIntegrityRows, TableName, fkInfo.Name);
					error += "\r\n";
					error += string.Format(DatabaseManagerStrings.RowsAffected, rowsAffected);
					error += "\r\n";
					error += string.Format(DatabaseManagerStrings.ListNoIntegrityRows, cmdText);
				}

				command.Dispose();
			}
			catch (TBException exc)
			{
				command.Dispose();
				error += exc.Message + "\r\n";
			}
		}

		/// <summary>
		/// Cancella tutte le righe orfane della tabella che non rispettano il constraint di ForeignKey passato come argomento
		///  delete from TableName where not exist (Select PKTableName.pkSeg1 from PKTableName where
		///	 TableName.fkSeg1 = PKTableName.pkSeg1, TableName.fkSeg2 = PKTableName.pkSeg2
		/// </summary>
		//---------------------------------------------------------------------------
		private void DeleteNoIntegrityRows(TBConnection connection, ForeignKeyInfo fkInfo, ref string error)
		{
			if (fkInfo.Columns == null || fkInfo.Columns.Count <= 0)
				return;

			//devo cancellare tutti le righe orfane per cui non esiste la corrispondente riga master
			string joinText = string.Empty;

			foreach (ForeignColumnInfo colInfo in fkInfo.Columns)
			{
				if (joinText.Length > 0)
					joinText += " and ";

				switch (dbmsType)
				{
					case DBMSType.SQLSERVER:
						joinText += string.Format("[{0}].[{1}] = [{2}].[{3}]", TableName, colInfo.FKColumn, fkInfo.PKTableName, colInfo.PKColumn);
						break;
                    case DBMSType.POSTGRE:
                        joinText += string.Format("{0}.{1} = {2}.{3}", TableName, colInfo.FKColumn, fkInfo.PKTableName, colInfo.PKColumn);
                        break;
				}
			}

			string cmdText = string.Format("DELETE {0} WHERE NOT EXISTS (SELECT {1} FROM {2} WHERE {3})", TableName, ((ForeignColumnInfo)fkInfo.Columns[0]).PKColumn, fkInfo.PKTableName, joinText);
			TBCommand command = new TBCommand(cmdText, connection);

			try
			{
				command.ExecuteNonQuery();
			}
			catch (TBException exc)
			{
				error += exc.Message + "\r\n";
			}
		}

		/// <summary>
		/// permette di abilitare il check degli eventuali CONSTRAINT di FOREIGN KEY definiti sulla tabella
		/// </summary>
		/// <param name="fkCommand"></param>
		/// <param name="withCheck">se true viene verificata l'integrità sui dati presenti sulla tabella</param>
		/// <param name="deleteRows">se true le eventuali righe orfane sono cancellate</param>
		//---------------------------------------------------------------------------
		public void EnableCheckOnForeignKey(TBCommand fkCommand, bool withCheck, bool deleteRows)
		{
            if (fkCommand.Connection.IsPostgreConnection())
            {
                try
                {
                    fkCommand.CommandText = "SET SESSION session_replication_role = DEFAULT;";
                    fkCommand.ExecuteNonQuery();
                    fkCommand.CommandText = "show session_replication_role";
                    string k = fkCommand.ExecuteScalar().ToString();
                    return;
                }
                catch (TBException)
                {
                    throw new TBException();
                }
            }

            if (catalogTableInfo.FKConstraintsInfo == null && !fkCommand.Connection.IsPostgreConnection())
                return;

			string error = string.Empty;
			string constraint = string.Empty;

			string sqlSql = (withCheck) ? "ALTER TABLE [{0}] WITH CHECK CHECK CONSTRAINT [{1}]" : "ALTER TABLE [{0}] CHECK CONSTRAINT [{1}]";

			foreach (ForeignKeyInfo fkInfo in catalogTableInfo.FKConstraintsInfo)
			{
				fkCommand.CommandText = string.Format(sqlSql, TableName, fkInfo.Name);

				try
				{
					fkCommand.ExecuteNonQuery();
				}
				catch (TBException exc)
				{
					if (fkCommand.Connection.IsSqlConnection() && exc.Number == 547)
					{
						if (deleteRows)
							DeleteNoIntegrityRows(fkCommand.Connection, fkInfo, ref error);
						else
							GetNoIntegrityRows(fkCommand.Connection, fkInfo, ref error);
					}

					error += exc.Message + "\r\n";
				}
			}

			if (error.Length > 0)
				throw new TBException(error);
		}

		// chiamati dal datamanager su singola tabella
		//---------------------------------------------------------------------------
		public void DisableCheckOnForeignKey(TBConnection connect)
		{
			TBCommand command = new TBCommand(connect);

			try
			{
                if (!connect.IsPostgreConnection())
                    catalogTableInfo.LoadFKConstraintsName(connect);
				DisableCheckOnForeignKey(command);
			}
			catch (TBException)
			{
				command.Dispose();
				throw;
			}
		}

		/// <summary>
		/// permette di abilitare il check degli eventuali CONSTRAINT di FOREIGN KEY definiti sulla tabella
		/// </summary>
		/// <param name="connect"></param>
		/// <param name="withCheck">se true viene verificata l'integrità sui dati presenti sulla tabella</param>
		/// <param name="deleteRows">se true le eventuali righe orfane sono cancellate</param>
		//---------------------------------------------------------------------------
		public void EnableCheckOnForeignKey(TBConnection connect, bool withCheck, bool deleteRows)
		{
			TBCommand command = new TBCommand(connect);

			try
			{
                if (!connect.IsPostgreConnection())
                    catalogTableInfo.LoadFKConstraintsName(connect);
				EnableCheckOnForeignKey(command, withCheck, deleteRows);
				command.Dispose();
			}
			catch (TBException exc)
			{
				command.Dispose();
				throw exc;
			}
		}
		#endregion		

		# region Funzioni per il RowSecurityLayer
		///<summary>
		/// Riempie una sottolista di CatalogColumn di un dataType specifico
		///</summary>
		//---------------------------------------------------------------------------
		public List<CatalogColumn> LoadColumnsByDataType(string dataTypeName)
		{
			List<CatalogColumn> colsFilteredByDataType = new List<CatalogColumn>();

			foreach (CatalogColumn cc in this.ColumnsInfo)
				if (string.Compare(cc.DataTypeName, dataTypeName, StringComparison.OrdinalIgnoreCase) == 0)
					colsFilteredByDataType.Add(cc);

			return colsFilteredByDataType;
		}

		///<summary>
		/// Dalla sottolista di CatalogColumn di un dataType specifico viene ritornata la sola
		/// colonna con lo specifico nome
		///</summary>
		//---------------------------------------------------------------------------
		public CatalogColumn GetColumnByNameAndDataType(string name, string dataTypeName)
		{
			List<CatalogColumn> colsFilteredByDataType = LoadColumnsByDataType(dataTypeName);

			if (colsFilteredByDataType.Count == 0)
				return null;

			foreach (CatalogColumn cc in colsFilteredByDataType)
				if (string.Compare(cc.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
					return cc;

			return null;
		}
		#endregion
	}
	#endregion	

	#region Class CatalogViewEntry
	//============================================================================
	public class CatalogViewEntry : CatalogEntry
	{
		public string	ViewDefinition;	
		
		//------------------------------------------------------------------
		public CatalogViewEntry(string name, string definition, DBMSType dbmsType)
			: base(name, dbmsType)
		{
            if (dbmsType == DBMSType.POSTGRE)
                ViewDefinition = (definition.Length > 0) ? string.Format("{0}; ", definition) : string.Empty;
            else
				ViewDefinition = (definition.Length > 0) ? string.Format("{0} GO", definition) : string.Empty;
		}
	}
	#endregion

	#region Class CatalogSProcedureEntry
	public enum RoutineType { Function, Procedure };
	//============================================================================
	public class CatalogRoutineEntry : CatalogEntry
	{
		public string	Definition;
		public RoutineType Type = RoutineType.Procedure;

		//---------------------------------------------------------------------------
		public CatalogRoutineEntry(string name, string routDef, RoutineType routType, DBMSType dbmsType)
			: base(name, dbmsType)
		{
			Definition = routDef;
			Type = routType;
		}

		//---------------------------------------------------------------------------
		public CatalogRoutineEntry(string name, string routDef, string routType, DBMSType dbmsType)
			: base(name, dbmsType)
		{
			Definition = routDef;
			Type = (string.Compare(routType, "PROCEDURE", true) == 0) ? RoutineType.Procedure : RoutineType.Function;
		}
	}
	#endregion
	
	#region Class CatalogInfo
	/// <summary>
	/// classe con struttura contenente le info di catalog, con gli array (di oggetti di
	/// tipo CatalogEntry) relativi agli elenchi delle tabelle, view e stored procedure.
	/// </summary>
	//============================================================================
	[Serializable]
	public class CatalogInfo : ISerializable
	{
		const string TABLES = "tables";

		# region Variables and Properties
		private ArrayList tblDBList;
		private ArrayList vwDBList;	
		private ArrayList routineDBList;	

		private string schemaName; //nome dello schema di database 
		private string dbCollation = string.Empty; //collation del database
		private bool valid = true;
	
		//---------------------------------------------------------------------------
		public	ArrayList	TblDBList		{ get { return tblDBList; } }
		public	ArrayList	VwDBList		{ get { return vwDBList; }  }	
		public	ArrayList	RoutineDBList	{ get { return routineDBList; }  }
		public  bool		Valid			{ get { return valid; } }	
		public	string		SchemaName		{ get { return schemaName; } }
		# endregion

		# region Constructor
		//---------------------------------------------------------------------------
		public CatalogInfo()
		{
			tblDBList		= new ArrayList();
			vwDBList		= new ArrayList();
			routineDBList	= new ArrayList();
		}
		# endregion

		# region Serialization 
		//usata per serializzare il catalog ad uso e consumo del woormEditor web
		//--------------------------------------------------------------------------
		public CatalogInfo(SerializationInfo info, StreamingContext context)
		{
			tblDBList = new ArrayList();
			object[] arTbls = (object[])info.GetValue(TABLES, typeof(object[]));
			if (arTbls != null)
				tblDBList.Add(arTbls);
		}

		//--------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(TABLES, tblDBList);
		}
		# endregion

		# region Load
		//---------------------------------------------------------------------------
		public void Load(TBConnection connection, bool onlyTables)
		{
			schemaName = connection.Database;
			TBDatabaseSchema dbSchema = new TBDatabaseSchema(connection);
			DBMSType dbmsType = DBMSType.SQLSERVER;

			try
			{
				if (connection.IsSqlConnection())
					dbmsType = DBMSType.SQLSERVER;
                else if (connection.IsPostgreConnection())
                    dbmsType = DBMSType.POSTGRE;
				else
					throw (new TBException("CatalogTableEntry.LoadColumnsInfo: Invalid connection"));

				AddObjectsToCatalog(DBObjectTypes.TABLE, dbSchema.GetAllSchemaObjects(DBObjectTypes.TABLE), dbmsType);
					
				if (!onlyTables)
				{
					AddObjectsToCatalog(DBObjectTypes.VIEW, dbSchema.GetAllSchemaObjects(DBObjectTypes.VIEW), dbmsType);
					AddObjectsToCatalog(DBObjectTypes.ROUTINE, dbSchema.GetAllSchemaObjects(DBObjectTypes.ROUTINE), dbmsType);
				}
			}
			catch(TBException e)
			{
				Debug.Fail(e.Message);
				valid = false;
				throw;
			}
		}

		/// <summary>
		/// carica tutte le informazioni contenute nello schema del database la cui connessione è passata come argomento:
		/// - tabelle, view, sp
		/// - colonne con informazioni di Default constraints
		/// - indici con tutte le informazioni dei segmenti
		/// - foreignkey con tutte le informazioni dei segmenti
		/// </summary>
		/// VIENE UTILIZZATA SOLO DAL PLUGIN DELLA ROWSECURITY
		//---------------------------------------------------------------------------
		public void LoadAllInformationSchema(TBConnection connection, bool withKey)
		{
			try
			{
				Load(connection, false);
			}
			catch (TBException)
			{
				throw;
			}

			string table = string.Empty;

			try
			{
				foreach(CatalogTableEntry catEntry in tblDBList)
				{
					table = catEntry.TableName; // solo per il debug
					catEntry.LoadAllInformationSchema(connection, withKey);
				}
			}
			catch(TBException e)
			{
				Debug.Fail(e.Message);
				Debug.WriteLine(string.Format("CatalogInfo::LoadAllInformationSchema (errore analizzando la tabella {0})", table));
				throw;
			}	
		}

		///<summary>
		/// Per azzerare dall'esterno gli array contenenti gli oggetti 
		/// (puo' essere utile per non ricaricare informazioni duplicate)
		///</summary>
		//---------------------------------------------------------------------------
		public void Clear()
		{
			tblDBList.Clear();
			vwDBList.Clear();
			routineDBList.Clear();
		}
		# endregion

		//---------------------------------------------------------------------------
		private void AddObjectsToCatalog(DBObjectTypes dbObjectType, List<Dictionary<string, object>> dataTable, DBMSType dbmsType)
		{
			foreach (Dictionary<string, object> row in dataTable)
				AddCatalogEntry(dbObjectType, row, dbmsType);
		}

		//---------------------------------------------------------------------------
		private void AddCatalogEntry(DBObjectTypes dbObjectType, Dictionary<string, object> row, DBMSType dbmsType)
		{
			object name = string.Empty, definition = string.Empty, routineType = string.Empty;
			string objectName = string.Empty;

			if (row.TryGetValue("name", out name))
			{
				if (name == null)
					return;
				objectName = name as string;
				if (string.IsNullOrWhiteSpace(objectName))
					return;
			}

			switch (dbObjectType)
			{
				case DBObjectTypes.TABLE:
					TblDBList.Add(new CatalogTableEntry(objectName, dbmsType));
					break;

				case DBObjectTypes.VIEW:
					row.TryGetValue("definition", out definition);
					VwDBList.Add(new CatalogViewEntry(objectName, (string)definition, dbmsType));
					break;

				case DBObjectTypes.ROUTINE:
					row.TryGetValue("definition", out definition);
					row.TryGetValue("routineType", out routineType);
					RoutineDBList.Add(new CatalogRoutineEntry(objectName, (string)definition, (string)routineType, dbmsType));
					break;
			}
		}

		/// <summary>
		/// dato il parametro dbtype restituisce il tipo SqlDbType corrispondente
		/// </summary>
		/// <param name="dbType"> stringa che rappresenta il tipo nativo di Sql Server</param>
		//---------------------------------------------------------------------------
		public SqlDbType GetSqlType(string dbType)
		{
			switch (dbType)
			{
				case ("bigint"):
					return SqlDbType.BigInt;					
				case ("binary"):
					return SqlDbType.Binary;
				case ("bit"):
					return SqlDbType.Bit;
				case ("char"):
					return SqlDbType.Char;					
				case ("datetime"):
					return SqlDbType.DateTime;
				case ("decimal"):
					return SqlDbType.Decimal;
				case ("float"):
					return SqlDbType.Float;					
				case ("image"):
					return SqlDbType.Image;
				case ("int"):
					return SqlDbType.Int;
				case ("money"):
					return SqlDbType.Money;					
				case ("nchar"):
					return SqlDbType.NChar;
				case ("ntext"):
					return SqlDbType.NText;
				case ("nvarchar"):
					return SqlDbType.NVarChar;					
				case ("real"):
					return SqlDbType.Real;
				case ("smalldatetime"):
					return SqlDbType.SmallDateTime;
				case ("smallint"):
					return SqlDbType.SmallInt;					
				case ("smallmoney"):
					return SqlDbType.SmallMoney;
				case ("text"):
					return SqlDbType.Text;
				case ("timestamp"):
					return SqlDbType.Timestamp;					
				case ("tinyint"):
					return SqlDbType.TinyInt;
				case ("uniqueidentifier"):
					return SqlDbType.UniqueIdentifier;
				case ("varbinary"):
					return SqlDbType.VarBinary;					
				case ("varchar"):
					return SqlDbType.VarChar;
				case ("sql_variant"):
					return SqlDbType.Variant;
			}

			return SqlDbType.VarChar;
		}
		
		/// <summary>
		/// funzione che effettua il controllo se esiste l'entry passata come parametro
		/// esiste nell'array di appartenenza (specificato dal type)
		/// </summary>
		/// <param name="tablename">nome fisico dell'entry da cercare</param>
		/// <param name="type">tipo entry per discriminare l'array dove ricercare</param>
		/// <returns>se l'entry esiste o meno</returns>
		//---------------------------------------------------------------------------
		public bool GetExistingTableInfo(string name, DBObjectTypes type)
		{
			switch (type)
			{
				case DBObjectTypes.TABLE:
				{
					foreach (CatalogEntry entry in TblDBList)
						if (string.Compare(entry.TableName, name, StringComparison.OrdinalIgnoreCase) == 0)
							return true;
					break;
				}

				case DBObjectTypes.VIEW:
				{
					foreach (CatalogEntry entry in VwDBList)
						if (string.Compare(entry.TableName, name, StringComparison.OrdinalIgnoreCase) == 0)
							return true;
					break;
				}

				case DBObjectTypes.ROUTINE:
				{
					foreach (CatalogEntry entry in RoutineDBList)
						if (string.Compare(entry.TableName, name, StringComparison.OrdinalIgnoreCase) == 0)
							return true;
					break;
				}
			}
			return false;
		}

		/// <summary>
		/// funzione che ritorna le CatalogEntry di una tabella specifica 
		/// (passata come parametro)
		/// </summary>
		//---------------------------------------------------------------------------
		public CatalogTableEntry GetTableEntry(string tableName)
		{
			foreach (CatalogTableEntry entry in TblDBList)
			{
				if (string.Compare(entry.TableName, tableName, StringComparison.OrdinalIgnoreCase) == 0)
					return entry;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public CatalogViewEntry GetViewEntry(string viewName)
		{
			foreach (CatalogViewEntry entry in VwDBList)
			{
				if (string.Compare(entry.TableName, viewName, StringComparison.OrdinalIgnoreCase) == 0)
					return entry;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public CatalogRoutineEntry GetSProcedureEntry(string spName)
		{
			foreach (CatalogRoutineEntry entry in routineDBList)
			{
				if (string.Compare(entry.TableName, spName, StringComparison.OrdinalIgnoreCase) == 0)
					return entry;
			}
			return null;
		}

		/// <summary>
		/// funzione che ritorna le info delle colonne appartenente ad 
		/// una specifica tabella (passata come parametro)
		/// </summary>
		//---------------------------------------------------------------------------
		public ArrayList GetColumnsInfo(string tableName, TBConnection tbConnection)
		{
			CatalogTableEntry entry = GetTableEntry(tableName);
			if (entry == null)
				return null;

			try
			{
				if (entry.ColumnsInfo == null)
					entry.LoadColumnsInfo(tbConnection, true);
			}
			catch (TBException)
			{
				throw;
			}

			return entry.ColumnsInfo;
		}

		/// <summary>
		/// funzione che ritorna l'array delle primary key di una singola tabella
		/// (passata come parametro)
		/// </summary>
		//---------------------------------------------------------------------------
		public void GetPrimaryKeys(string tableName, ref StringCollection keys)
		{
			CatalogTableEntry entry = GetTableEntry(tableName);
			if (entry != null)	
				entry.GetPrimaryKeys(ref keys);
		}

		/// <summary>
		/// funzione che ritorna le info di una singola colonna appartenente ad una specifica
		/// tabella (passate come parametri)
		/// </summary>
		//---------------------------------------------------------------------------
		public CatalogColumn GetColumnInfo(string tableName, string colName)
		{
			CatalogTableEntry entry = GetTableEntry(tableName);
			if (entry == null)
				return null;
			
			return entry.GetColumnInfo(colName);
		}

		/// <summary>
		/// le tabelle diventano tutte deselezionate e vengono pulite gli array delle
		///  colonne selezionate (vedi wizard di esportazione)
		/// </summary>
		//---------------------------------------------------------------------------
		public void ClearSelectedTableEntry()
		{
			// sul changed di questo radiobutton devo 
			foreach (CatalogTableEntry entry in TblDBList)
			{
				entry.Selected = false;
				entry.SelectedColumnsList.Clear();
			}			
		}	

		/// <summary>
		/// Dati il nome di una tabella e colonna ritorna se il tipo della colonna e' nvarchar o meno
		/// </summary>
		/// <param name="connection">connessione aperta</param>
		/// <param name="table">nome tabella</param>
		/// <param name="column">nome colonna (di tipo stringa)</param>
		/// <returns>true: se e' di tipo Unicode (nvarchar) - false: se di tipo varchar</returns>
		//---------------------------------------------------------------------------
		public bool IsUnicodeValueInColumn(TBConnection connection, string table, string column)
		{
			CatalogTableEntry cte = GetTableEntry(table);
			cte.LoadColumnsInfo(connection, false);

			CatalogColumn cc = cte.GetColumnInfo(column);
			return (string.Compare(cc.GetDBDataType(), "NVarChar", StringComparison.OrdinalIgnoreCase) == 0);
		}

		//---------------------------------------------------------------------------
		public bool IsCollationCultureSensitive(CatalogColumn catalogColumn)
		{
			string collation = catalogColumn.CollationName;
			if (string.IsNullOrEmpty(collation))
				collation = dbCollation;

			if (string.IsNullOrEmpty(collation))
				return false;

			return string.Compare(collation, "Latin1_General_CI_AS", StringComparison.OrdinalIgnoreCase) != 0;
			
		}
		
		#region Azioni sulle FK (abilita/disabilita CHECK constraint + add/drop FK)
		/// <summary>
		/// permette di abilitare il check degli eventuali CONSTRAINT di FOREIGN KEY definiti sulle tabelle
		/// </summary>
		/// <param name="connect"></param>
		/// <param name="withCheck">se true viene verificata l'integrità sui dati presenti sulla tabella</param>
		/// <param name="deleteRows">se true le eventuali righe orfane sono cancellate</param>
		//---------------------------------------------------------------------------
		public void EnableCheckOnForeignKey(TBConnection connect, bool withCheck, bool deleteRows)
		{
			string		error	= string.Empty;
			TBCommand	command	= new TBCommand(connect);

			foreach (CatalogTableEntry entry in TblDBList)
			{
				try
				{
					entry.EnableCheckOnForeignKey(command, withCheck, deleteRows);
				}
				catch(TBException exc)
				{
					error += exc.Message;
					command.Dispose();
				}
			}

			command.Dispose();

			if (error.Length > 0)
				throw new TBException(error);
		}
		#endregion
	}
	#endregion
}