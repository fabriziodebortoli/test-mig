using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NpgsqlTypes;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.CoreTypes;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	#region Classe per la gestione degli indici (anche primarykey) di una singola tabella
	//============================================================================
	public class IndexInfo
	{
		# region Variables and Properties
		public  bool				Selected	= false;
		private string				name		= string.Empty;
		private bool				primaryKey	= false;
		private bool				clustered	= false;
		private bool				unique		= false;
		private StringCollection	columns		= new StringCollection();

		public string				Name		{ get { return name; } }
		public bool					PrimaryKey	{ get { return primaryKey; } }
		public bool					Clustered	{ get { return clustered; } }
		public bool					Unique		{ get { return unique; } }
		public StringCollection		Columns		{ get { return columns; } }
		# endregion

		# region Constructor
		//----------------------------------------------------------------------
		public IndexInfo(string name, bool primaryKey, bool clustered, bool unique)
		{
			this.name		= name;
			this.primaryKey = primaryKey;
			this.clustered	= clustered;
			this.unique		= unique;
		}
		# endregion

		# region ToSql
		///<summary>
		/// Creazione primary o index con la sintassi SqlServer
		///</summary>
		//----------------------------------------------------------------------
		private string ToSqlForSql(string tableName)
		{
			string singleIndex = string.Empty;

			// se è un indice di chiave primaria
			if (primaryKey)
			{
				singleIndex = string.Format
					(
					"ALTER TABLE [{0}] ADD CONSTRAINT [{1}] PRIMARY KEY {2}(",
					tableName, 
					Name, 
					(clustered) ? "CLUSTERED" : "NONCLUSTERED"
					);
		
				/*[Banca],
				[EUnaBancaAzienda]
				)  ON [PRIMARY] 
				GO*/
				for (int i = 0 ; i < Columns.Count ; i++)
				{
					singleIndex += string.Format("[{0}]", Columns[i]);
					if (i < Columns.Count - 1)
						singleIndex += ",";
				}
				singleIndex += ") ON [PRIMARY] \r\n";
			}
			else
			{
				singleIndex = string.Format
					(
					"CREATE {0} {1} INDEX [{2}] ON [{3}] (", 
					(unique) ? "UNIQUE" : "", 
					(clustered) ? "CLUSTERED" : "", 
					Name, 
					tableName
					);

				for (int i = 0 ; i < Columns.Count ; i++)
				{
					singleIndex += string.Format("[{0}]", Columns[i]);
					if (i < Columns.Count - 1)
						singleIndex += ",";
				}

				singleIndex += ") ON [PRIMARY] \r\n\r\n";
			}

			return singleIndex;
		}


        ///<summary>
        /// Creazione primary o index con la sintassi Postgre
        ///</summary>
        //----------------------------------------------------------------------
        private string ToSqlForPostgre(string tableName)
        {
            string singleIndex = string.Empty;

            // se è un indice di chiave primaria
            if (primaryKey)
            {
                singleIndex = string.Format
                    (
                    "ALTER TABLE IF EXISTS {0} ADD CONSTRAINT {1} PRIMARY KEY (",
                    tableName,
                    Name
                    );

                /*[Banca],
                [EUnaBancaAzienda]
                )  ON [PRIMARY] 
                GO*/
                for (int i = 0; i < Columns.Count; i++)
                {
                    singleIndex += string.Format("{0}", Columns[i]);
                    if (i < Columns.Count - 1)
                        singleIndex += ",";
                }
                singleIndex += "); ";
            }
            else
            {
                singleIndex = string.Format
                    (
                    "CREATE {0} INDEX {1}] ON {2} (",
                    (unique) ? "UNIQUE" : "",
                    //(clustered) ? "CLUSTERED" : "",
                    Name,
                    tableName
                    );

                for (int i = 0; i < Columns.Count; i++)
                {
                    singleIndex += string.Format("{0}", Columns[i]);
                    if (i < Columns.Count - 1)
                        singleIndex += ",";
                }

                singleIndex += "); ";

                if (clustered) singleIndex += string.Format("CLUSTER {0} USING {1}; ", tableName, Name);
            }

            return singleIndex;
        }

		///<summary>
		/// Creazione primary o index con la sintassi Oracle
		///</summary>
		//----------------------------------------------------------------------
		private string ToSqlForOracle(string tableName)
		{
			string singleIndex = string.Empty;
			
			// se è un indice di chiave primaria
			if (primaryKey)
			{
				singleIndex = 
					string.Format("ALTER TABLE \"{0}\" ADD CONSTRAINT \"{1}\" PRIMARY KEY  (", tableName, Name);
		
				/*[Banca],
				[EUnaBancaAzienda]
				)  ON [PRIMARY] 
				GO*/
				for (int i = 0 ; i < Columns.Count ; i++)
				{
					singleIndex += string.Format("\"{0}\"", Columns[i]);
					if (i < Columns.Count - 1)
						singleIndex += ",";
				}
				singleIndex += ")\r\n";
			}
			else
			{
				singleIndex = string.Format("CREATE  INDEX \"{0}\" ON \"{1}\" (", Name, tableName);

				for (int i = 0 ; i < Columns.Count ; i++)
				{
					singleIndex += string.Format("\"{0}\"", Columns[i]);
					if (i < Columns.Count - 1)
						singleIndex += ",";
				}

				singleIndex += ")\r\n\r\n";
			}
			singleIndex += "\r\nGO\r\n\r\n";

			return singleIndex;
		}

		//----------------------------------------------------------------------
		public string ToSql(string tableName, DBMSType dbmsType)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				return ToSqlForSql(tableName);

			if (dbmsType == DBMSType.ORACLE)
				return ToSqlForOracle(tableName);

            if (dbmsType == DBMSType.POSTGRE)
                return ToSqlForPostgre(tableName);

			throw (new TBException("IndexInfo.ToSql: " + string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
		}
		# endregion
	}

	//============================================================================
	public class IndexArray
	{
		# region Variables and Properties
		private ArrayList	indexes			= new ArrayList();
		public	IndexInfo	PrimaryKeyIndex	= null;

		public	ArrayList	Indexes	{ get {	return indexes;	} }
		#endregion

		#region Constructors
		//----------------------------------------------------------------------
		public IndexArray()
		{
		}

		//----------------------------------------------------------------------
		public IndexArray(DataTable indexTable)
		{
			foreach (DataRow indexRow in indexTable.Rows)
			{
				string indexName = (string)indexRow[DBSchemaStrings.Name];
				
				bool primaryKey = false;
				primaryKey = (bool)(indexRow[DBSchemaStrings.PrimaryKey]);

				bool unique = false;
				unique = (bool)indexRow[DBSchemaStrings.Unique]; 		

				bool clustered = false;
				clustered = (bool)(indexRow[DBSchemaStrings.Clustered]);

				string columnName = (string)indexRow[DBSchemaStrings.ColumnName];

				AddIndex(indexName, primaryKey, clustered, unique, columnName);
			}			
		}
		#endregion

		#region Funzioni di aggiunta e ricerca indici
		//----------------------------------------------------------------------
		public void AddIndex(string indexName, bool primaryKey, bool clustered, bool unique, string columnName)
		{
			if (primaryKey)
			{
				if (PrimaryKeyIndex == null)
					PrimaryKeyIndex = new IndexInfo(indexName, true, clustered, unique);

				PrimaryKeyIndex.Columns.Add(columnName);
			}
			else
			{
				IndexInfo index = GetIndex(indexName);
				if (index == null)
				{
					index = new IndexInfo(indexName, false, clustered, unique);
					indexes.Add(index);
				}

				index.Columns.Add(columnName);	
			}
		}

		//----------------------------------------------------------------------
		public IndexInfo GetIndex(string indexName)
		{
			foreach (IndexInfo indexInfo in indexes)
			{
				if (string.Compare(indexName, indexInfo.Name, true, CultureInfo.InvariantCulture) == 0)
					return indexInfo;
			}

			return null;
		}
		#endregion

		#region Funzioni di unparsing in sql
		/*
		ALTER TABLE [MN_Banche] ADD 
			CONSTRAINT [PK_MN_Banche] PRIMARY KEY  NONCLUSTERED 
			(
			[Banca],
			[EUnaBancaAzienda]
			)  ON [PRIMARY] 
		GO

		CREATE  INDEX [MN_Banche2] ON [MN_Banche]([EUnaBancaAzienda], [Descrizione]) ON [PRIMARY]
		GO
		*/
		//----------------------------------------------------------------------
		public string ToSql(string tableName, DBMSType dbmsType)
		{
			string pkScript		= string.Empty;
			string indexesScript= string.Empty;
			
			if (PrimaryKeyIndex != null && PrimaryKeyIndex.Selected)
				pkScript = PrimaryKeyIndex.ToSql(tableName, dbmsType);
		
			foreach (IndexInfo indexInfo in indexes)
			{
				if (indexInfo.Selected)
					indexesScript += indexInfo.ToSql(tableName, dbmsType);
			}
	
			return pkScript + indexesScript;
		}
		#endregion
	}
	#endregion

	#region Classi per la gestione delle ForeignKeys: contengono tutte le informazioni (utilizzati dal Migrationkit)

	# region ForeignColumnInfo
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
		private string		name		= string.Empty;
		private string		pkTableName	= string.Empty;
		private ArrayList	columns		= new ArrayList();
		
		public string		Name		{ get { return name;		} }
		public string		PKTableName	{ get { return pkTableName; } }
		public ArrayList	Columns		{ get { return columns;		} }
		
		//----------------------------------------------------------------------
		public ForeignKeyInfo(string fkName, string tableName)
		{
			if (fkName == null || fkName.Length == 0 || tableName == null || tableName.Length == 0)
				throw new TBException("ForeignKeyInfo.Constructors");

			name		= fkName;
			pkTableName	= tableName;
		}

		//----------------------------------------------------------------------
		public void AddColumns(string fkColumn, string pkColumn)
		{
			columns.Add(new ForeignColumnInfo(fkColumn, pkColumn));
		}

		# region ToSql
		///<summary>
		/// Creazione e drop constraint di FK con la sintassi SqlServer
		///</summary>
		//----------------------------------------------------------------------
		private void ToSqlForSql(string tableName, ref string createSQL, ref string dropSQL)
		{
			createSQL += string.Format("ALTER TABLE [{0}] ADD\r\nCONSTRAINT [{1}] FOREIGN KEY\r\n(", tableName, Name);
				
			dropSQL += string.Format("if exists (select * from dbo.sysobjects where id = object_id(N'[{0}]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)\r\n", Name);
			dropSQL += string.Format("ALTER TABLE [{0}] DROP CONSTRAINT [{1}]\r\n", tableName, Name);
		
			for (int i = 0; i < Columns.Count ; i++)
			{
				createSQL += string.Format("[{0}]", ((ForeignColumnInfo)Columns[i]).FKColumn);
				if (i != Columns.Count -1)
					createSQL += ",";
			}

			createSQL += string.Format(")\r\nREFERENCES [{0}] (", pkTableName);
				
			for (int i = 0; i < Columns.Count ; i++)
			{
				createSQL += string.Format("[{0}]", ((ForeignColumnInfo)Columns[i]).PKColumn);
				if (i != Columns.Count -1)
					createSQL += ",";
			}

			createSQL +=")";
		}

		///<summary>
		/// Creazione e drop constraint di FK con la sintassi Oracle
		///</summary>
		//----------------------------------------------------------------------
		private void ToSqlForOracle(string tableName, ref string createSQL, ref string dropSQL)
		{
			createSQL += string.Format("ALTER TABLE \"{0}\" ADD\r\nCONSTRAINT \"{1}\" FOREIGN KEY\r\n(", tableName, Name);
				
			dropSQL += string.Format("ALTER TABLE \"{0}\" DROP CONSTRAINT \"{1}\"\r\n", tableName, Name);
		
			for (int i = 0; i < Columns.Count ; i++)
			{
				createSQL += string.Format("\"{0}\"", ((ForeignColumnInfo)Columns[i]).FKColumn);
				if (i != Columns.Count -1)
					createSQL += ",";
			}

			createSQL += string.Format(")\r\nREFERENCES \"{0}\" (", pkTableName);
				
			for (int i = 0; i < Columns.Count ; i++)
			{
				createSQL += string.Format("\"{0}\"", ((ForeignColumnInfo)Columns[i]).PKColumn);
				if (i != Columns.Count -1)
					createSQL += ",";
			}

			createSQL +=")";
		}

        ///<summary>
        /// Creazione e drop constraint di FK con la sintassi Postgre
        ///</summary>
        //----------------------------------------------------------------------
        private void ToSqlForPostgre(string tableName, ref string createSQL, ref string dropSQL)
        {
            createSQL += string.Format("ALTER TABLE IF EXISTS {0} ADD CONSTRAINT {1} FOREIGN KEY (", tableName, Name);

            dropSQL += string.Format("ALTER TABLE IF EXISTS {0} DROP CONSTRAINT IF EXISTS {1} ; ", tableName, Name);

            for (int i = 0; i < Columns.Count; i++)
            {
                createSQL += string.Format("{0}", ((ForeignColumnInfo)Columns[i]).FKColumn);
                if (i != Columns.Count - 1)
                    createSQL += ",";
            }

            createSQL += string.Format(")REFERENCES {0} (", pkTableName);

            for (int i = 0; i < Columns.Count; i++)
            {
                createSQL += string.Format("{0}", ((ForeignColumnInfo)Columns[i]).PKColumn);
                if (i != Columns.Count - 1)
                    createSQL += ",";
            }

            createSQL += "); ";
        }



		//----------------------------------------------------------------------
		public void ToSql(string tableName, DBMSType dbmsType, ref string createSQL, ref string dropSQL)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				ToSqlForSql(tableName, ref createSQL, ref dropSQL);
			else if (dbmsType == DBMSType.ORACLE)
				ToSqlForOracle(tableName, ref createSQL, ref dropSQL);
            else if (dbmsType == DBMSType.POSTGRE)
                ToSqlForPostgre(tableName, ref createSQL, ref dropSQL);
			else
				throw (new TBException("ForeignKeyInfo.ToSql: " + string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
		}
		# endregion
	}
	# endregion

	# region ForeignKeyArray
	//====================================================================================================
	public class ForeignKeyArray : ArrayList
	{
		#region Constructors
		//----------------------------------------------------------------------
		public ForeignKeyArray()
		{
		}

		//----------------------------------------------------------------------
		public ForeignKeyArray(DataTable fkTable)
		{
			ForeignKeyInfo fki = null;
			if (fkTable != null)
			{
				foreach (DataRow fkRow in fkTable.Rows)
				{
					string fkName		= (string) fkRow[DBSchemaStrings.Name];
					string pkTableName	= (string) fkRow[DBSchemaStrings.PKTableName];
					string fkColumn		= (string) fkRow[DBSchemaStrings.FKColumn];
					string pkColumn		= (string) fkRow[DBSchemaStrings.PKColumn];

					fki = Add(fkName, pkTableName);
					if (fki != null)
						fki.AddColumns(fkColumn, pkColumn);
				}
			}
		}
		#endregion

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
			{
				if (string.Compare(fkName, fkInfo.Name, true, CultureInfo.InvariantCulture) == 0)
					return fkInfo;
			}

			return null;
		}

		#region ToSql
		/*
		if exists (select * from dbo.sysobjects where id = object_id(N'[PK_MN_BancheCCEffetti_MN_Banche]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
		ALTER TABLE [MN_BancheCCEffetti] DROP CONSTRAINT PK_MN_BancheCCEffetti_MN_Banche
		GO
		
		ALTER TABLE [MN_BancheCCBancari] ADD 
		CONSTRAINT [PK_MN_BancheCCBancari_MN_Banche] FOREIGN KEY 
		(
			[Banca],
			[EUnaBancaAzienda]
		) REFERENCES [MN_Banche] (
			[Banca],
			[EUnaBancaAzienda]
		)
		GO
		*/
		//----------------------------------------------------------------------
		public void ToSql(string tableName, DBMSType dbmsType, out string createSQL, out string dropSQL)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				ToSqlForSql(tableName, out createSQL, out dropSQL);
			else if (dbmsType == DBMSType.ORACLE)
				ToSqlForOracle(tableName, out createSQL, out dropSQL);
            else if (dbmsType == DBMSType.POSTGRE)
                ToSqlForPostgre(tableName, out createSQL, out dropSQL);
			else
				throw (new TBException("ForeignKeyInfo.ToSql: " + string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
		}	

		//----------------------------------------------------------------------
		public void ToSqlForSql(string tableName, out string createSQL, out string dropSQL)
		{
			createSQL = dropSQL = string.Empty;
			
			if (this.Count == 0)
				return;
				
			createSQL = dropSQL = "BEGIN\r\n";
			foreach (ForeignKeyInfo fki in this)
				fki.ToSql(tableName, DBMSType.SQLSERVER, ref createSQL, ref dropSQL);
		
			createSQL	+= "END\r\nGO\r\n\r\n";
			dropSQL		+= "END\r\nGO\r\n\r\n";
		}		

		//----------------------------------------------------------------------
		public void ToSqlForOracle(string tableName, out string createSQL, out string dropSQL)
		{
			createSQL = dropSQL = string.Empty;
			
			if (this.Count == 0)
				return;
				
			foreach (ForeignKeyInfo fki in this)
				fki.ToSql(tableName, DBMSType.ORACLE, ref createSQL, ref dropSQL);
		}

        //----------------------------------------------------------------------
        public void ToSqlForPostgre(string tableName, out string createSQL, out string dropSQL)
        {
            createSQL = dropSQL = string.Empty;

            if (this.Count == 0)
                return;


            foreach (ForeignKeyInfo fki in this)
                fki.ToSql(tableName, DBMSType.POSTGRE, ref createSQL, ref dropSQL);
        }
		#endregion
	}
	#endregion

	#endregion

	#region Class RefFKConstraint: contiene solo il nome delle foreignkey e delle tabelle che riferiscono la tabella in questione
	//============================================================================
	public class RefFKConstraint
	{
		private int		idFKTable;
		public	string	FKTableName; // nome della tabella con la FK constraints
		public  string	Name;		 // nome del constraints

		public int		IdFKTable	{ get { return idFKTable; } }		
		
		//TODO da eliminare
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
		private short ordinalPosition;
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
		public short OrdinalPosition { get { return ordinalPosition; } }
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

		//---------------------------------------------------------------------- 
		public CatalogParameter(DataRow paramRow, DBMSType dbmsType)
		{
			if (paramRow == null)
				throw new TBException(DatabaseLayerStrings.ErrNoColumnInfo);

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
		}
		#endregion
	}
	#endregion 

	#region Classe CatalogColumn : mappa una colonna di database
	//==================================================================
	[Serializable]
	public class CatalogColumn : ISerializable
	{
		const string		NAME = "name";
		const string		TYPE = "type";
		const string		KEY = "key";

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
		private Int16	numericPrecision = -1;
		private Int16	numericScale = -1;
		private bool	isVirtual	= false;	
		
		private	bool	hasDefault		= false;
		private	string	columnDefault	= string.Empty;
		private bool	isAutoIncrement	= false;
		private Int64	autoIncrementSeed = 0;
		private Int64	autoIncrementStep = 0;
		private string  collationName = string.Empty;
		private bool	isDataText = false;

		private DBMSType dbmsType = DBMSType.UNKNOWN;
		public  bool	Selected  = false;
		#endregion

		#region Properties
		public string Name				{ get { return name; }}
		public string SystemType		{ get { return systemType; } }	//Maps to the .NET Framework type of the column
		public TBType ProviderType		{ get { return providerType; }} //The indicator of the column's provider data type	
		public string DataTypeName		{ get { return dataTypeName; } }
		public int ColumnSize			{ get { return columnSize; } set { columnSize = value; }}	
		public bool	IsKey				{ get { return isKey; }	set { isKey = value; }}
		public bool IsUnique			{ get { return isUnique; } }			
		public bool	IsNullable			{ get { return isNullable;		}}
		public Int16 NumericPrecision	{ get { return numericPrecision; }}
		public Int16 NumericScale		{ get { return numericScale; }}
		public bool IsVirtual			{ get { return isVirtual; } set { isVirtual = value; } }
		public bool HasDefault			{ get { return hasDefault; } set { hasDefault = value; }}		
		public string ColumnDefault		{ get { return columnDefault; } set { columnDefault = value; }}
		public string CollationName		{ get { return collationName; } }	
		public bool	 IsAutoIncrement	{ get { return isAutoIncrement;	} }
		public Int64 IsAutoIncrementSeed { get { return autoIncrementSeed; } }
		public Int64 IsAutoIncrementStep { get { return autoIncrementStep; } }
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
		public CatalogColumn(DataRow columnRow, DBMSType dbmsType)
		{
			if (columnRow == null)
				throw new TBException(DatabaseLayerStrings.ErrNoColumnInfo);
			
			this.dbmsType = dbmsType;

			try
			{
				object o = columnRow["BaseColumnName"];
				if (o != DBNull.Value)
					name = (string)(columnRow["BaseColumnName"]);
				else
				{
					o = (string)(columnRow["ColumnName"]);
					if (o != DBNull.Value)
						name = (string)(columnRow["ColumnName"]);
					else
						throw new TBException(DatabaseLayerStrings.ErrNoColumnInfo);
				}

				isNullable = (bool)columnRow["AllowDBNull"];

				o = columnRow["NumericPrecision"];
				if (o != DBNull.Value)
					numericPrecision = Convert.ToInt16(o);

				o = columnRow["NumericScale"];
				if (o != DBNull.Value)
					numericScale = Convert.ToInt16(o);

				o = columnRow["ColumnSize"];
				if (o != DBNull.Value)
					columnSize = (int)o;

				o = columnRow["IsKey"];
				isKey = (o != DBNull.Value) ? (bool)o : false;

				// proprietà AutoIncrement per le colonne di tipo IDENTITY (e relativi seed e step)
				// le colonne hanno la definizione: int IDENTITY[(seed, increment)]
                if (dbmsType == DBMSType.SQLSERVER || dbmsType == DBMSType.POSTGRE)
				{
					isAutoIncrement = (bool)columnRow["IsAutoIncrement"];
					if (isAutoIncrement)
					{
						autoIncrementSeed = columnRow.Table.Columns["IsAutoIncrement"].AutoIncrementSeed;
						autoIncrementStep = columnRow.Table.Columns["IsAutoIncrement"].AutoIncrementStep;
					}
				}
				
				// nel caso contenga il Namespace completo ritorno il solo basename del tipo
				// Es: System.Int32 torna Int32
				string tmpType = columnRow["DataType"].ToString();
				systemType = tmpType.Replace("System.", "");

				if (dbmsType == DBMSType.ORACLE)
				{
					// i float vengono letti come se fossero dei numeric e la precisione 126 risulta maggione della max precisione == 38
					if (numericPrecision == 126)
						providerType = TBDatabaseType.GetTBType(dbmsType, 29);
					else
					{
						providerType = TBDatabaseType.GetTBType(dbmsType, (int)columnRow["ProviderType"]);
						if (providerType.OracleType == OracleType.Clob || providerType.OracleType == OracleType.NClob)
							isDataText = true;
					}
				}
				else 
					if (dbmsType == DBMSType.POSTGRE)
						providerType = TBDatabaseType.GetTBTypePostgre(dbmsType, columnRow["ProviderType"].ToString());
					else
					{
						// SQLSERVER
						providerType = TBDatabaseType.GetTBType(dbmsType, (int)columnRow["ProviderType"]);
						if (providerType.SqlDbType == SqlDbType.Text || providerType.SqlDbType == SqlDbType.NText)
							isDataText = true;
					}

                dataTypeName = TBDatabaseType.GetDBDataType(providerType, dbmsType); 
				collationName = columnRow["CollationName"].ToString();
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

		// Questa  routine serve essenzialmente a Woorm per proporre all'utente tutte
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


			if (dbmsType == DBMSType.ORACLE)
			{
				switch (providerType.OracleType)
				{
					case OracleType.Int16:
						dataTypes.Add(DataType.Integer);
						break;
					case OracleType.Int32:
						dataTypes.Add(DataType.Long);
						dataTypes.Add(DataType.Enum);
						dataTypes.Add(DataType.Integer);
						break;
					case OracleType.Float:
						dataTypes.Add(DataType.Double);
						dataTypes.Add(DataType.Quantity);
						dataTypes.Add(DataType.Money);
						dataTypes.Add(DataType.Percent);
						break;
					case OracleType.Timestamp:
						dataTypes.Add(DataType.DateTime);
						break;
					case OracleType.Number:
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

					case OracleType.VarChar:
					case OracleType.NVarChar:
						dataTypes.Add(DataType.String);
						dataTypes.Add(DataType.Bool);
						dataTypes.Add(DataType.Guid);
						break;

					case OracleType.Clob:
					case OracleType.NClob:
						dataTypes.Add(DataType.Text);
						break;

					case OracleType.Blob:
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

				if (HasPrecision()&& numericPrecision > 0)
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
		public string CreateSqlColumnScript(bool withDefault, bool withAutoincrement)
		{
			string dbType = GetDBDataType();
			string column = string.Format("[{0}] [{1}]", name, dbType);

			if (HasLength())
				column += string.Format(" ({0})", columnSize.ToString());

			if (isAutoIncrement && withAutoincrement)
				column += string.Format(" IDENTITY ({0}, {1})", autoIncrementSeed, autoIncrementStep);

			if (isNullable)
				column += " NULL";
			else
				column += " NOT NULL";

			if (hasDefault && withDefault)
				column += string.Format(" DEFAULT {0}", columnDefault);
			
			return column;
		}


        //----------------------------------------------------------------------
        public string CreatePostgreColumnScript(bool withDefault, bool withAutoincrement)
        {
            string dbType = GetDBDataType();
            string column = string.Format("{0} {1}", name, dbType);

            if (HasLength())
                column += string.Format(" ({0})", columnSize.ToString());

            if (isAutoIncrement && withAutoincrement)
                column += string.Format(" serial");

            if (isNullable)
                column += " NULL";
            else
                column += " NOT NULL";

            if (hasDefault && withDefault)
                column += string.Format(" DEFAULT {0}", columnDefault);

            return column;
        }

		//----------------------------------------------------------------------
		public string CreateOracleColumnScript(bool withDefault)
		{
			string column = string.Format("\"{0}\" {1}", name, GetCompleteDBType());

			if (hasDefault && withDefault)
				column += string.Format(" DEFAULT {0}", columnDefault);
		
			if (isNullable)
				column += " NULL";
			else
				column += " NOT NULL";
				
			return column;
		}

		/// <summary>
		/// Genera lo script di una colonna, da inserire in un contesto di CREATE o ALTER TABLE
		/// (lo script è differente a seconda del tipo di DBMS connesso)
		/// </summary>
		//----------------------------------------------------------------------
		public string ToSql(bool withDefault, bool withAutoincrement = true)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				return CreateSqlColumnScript(withDefault, withAutoincrement);

			if (dbmsType == DBMSType.ORACLE)
				return CreateOracleColumnScript(withDefault);

            if (dbmsType == DBMSType.POSTGRE)
                return CreatePostgreColumnScript(withDefault, withAutoincrement);

			throw (new TBException("CatalogColumn.CreateColumnScript: " + string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
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
		public ArrayList ParametersInfo { get { return paramsInfo; } }

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
			else if (tbConnect.IsOracleConnection()) 
				dbmsType = DBMSType.ORACLE;
            else if (tbConnect.IsPostgreConnection())
                dbmsType = DBMSType.POSTGRE; 
			else
				throw(new TBException("CatalogTableEntry.LoadColumnsInfo: Invalid connection"));

			DataTable columnsTable = null;	
			columnsInfo = new ArrayList();
			bool autoIncrementalColFound = false;
			bool dataTextColumnFound = false;
			
			try
			{
				TBDatabaseSchema dbSchema = new TBDatabaseSchema(tbConnect);
				columnsTable = dbSchema.GetTableSchema(TableName, withKeyInfo);	
				
				foreach (DataRow columnRow in columnsTable.Rows)
				{
					CatalogColumn cc = new CatalogColumn(columnRow, dbmsType);
					columnsInfo.Add(cc);

                    if (dbmsType == DBMSType.SQLSERVER || dbmsType == DBMSType.POSTGRE)
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
			{
				if (string.Compare(column.Name, columnName, true, CultureInfo.InvariantCulture) == 0)
					return column;
			}
			return null;
		}

		/// <summary>
		/// dato il nome di una colonna viene restituito la sua posizione 
		/// </summary>
		//---------------------------------------------------------------------------
		public int GetColumnPosition(string columnName)
		{
			if (columnsInfo == null)
				return -1;
			
			CatalogColumn column = null;
			for (int i = 0; i < columnsInfo.Count; i++ )
			{
				column = (CatalogColumn)columnsInfo[i]; 
				if (string.Compare(column.Name, columnName, true, CultureInfo.InvariantCulture) == 0)
					return i;
			}
			return -1;
		}
		#endregion

		#region Gestione Parametri
		/// <summary>
		/// dato il nome di un parametro vengono restituite le sue informazioni
		/// </summary>
		//---------------------------------------------------------------------------
		public CatalogParameter GetParameterInfo(string paramName)
		{
			if (paramsInfo == null)
				return null;

			foreach (CatalogParameter param in paramsInfo)
			{
				if (string.Compare(param.Name, paramName, true, CultureInfo.InvariantCulture) == 0)
					return param;
			}
			return null;
		}

		/// <summary>
		/// dato il nome di un parametro viene restituito la sua posizione 
		/// </summary>
		//---------------------------------------------------------------------------
		public int GetParameterPosition(string paramName)
		{
			if (paramsInfo == null)
				return -1;

			CatalogParameter param = null;
			for (int i = 0; i < paramsInfo.Count; i++)
			{
				param = (CatalogParameter)columnsInfo[i];
				if (string.Compare(param.Name, paramName, true, CultureInfo.InvariantCulture) == 0)
					return i;
			}
			return -1;
		}
		#endregion

		#region Gestione Indici
		/// carico le informazioni relativi agli indici comprensivo di chiave primaria e per ciascun indice
		/// le informazioni relative alle colonne
		//---------------------------------------------------------------------------
		public void LoadIndexes(OleDbConnection oleDbConnection)
		{
			TBDatabaseSchemaOleDb dataSchema = new TBDatabaseSchemaOleDb(oleDbConnection);
			DataTable dataTable = null;

			try
			{
				dataTable = dataSchema.LoadIndexes(TableName);
			}
			catch (TBException)
			{
				throw;
			}

			if (dataTable != null)
			{
				indexesInfo = new IndexArray(dataTable);

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
		}

		// legge dalle tabelle di sistema le informazioni relative agli indici di Oracle
		// devo andare attraverso una connessione OLEDB
		//---------------------------------------------------------------------------
		public void LoadOracleIndexes(TBConnection tbConnection)
		{
			TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnection);
			DataTable dataTable = null;

			try
			{
				dataTable = dataSchema.LoadOracleIndexesInfo(TableName);
				indexesInfo = new IndexArray(dataTable);

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



        // legge dalle tabelle di sistema le informazioni relative agli indici di Postgre
        //
        //---------------------------------------------------------------------------
        public void LoadPostgreIndexes(TBConnection tbConnection)
        {
            TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnection);
            DataTable dataTable = null;

            try
            {
                dataTable = dataSchema.LoadPostgreIndexesInfo(TableName);
                indexesInfo = new IndexArray(dataTable);

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
	

		#region Gestione Default
		//---------------------------------------------------------------------------
		public void LoadDefaults(TBConnection tbConnect)
		{
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
		}
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
				FKDataTable dataTable = dbSchema.LoadFKConstraints(TableName);
				
				if (dataTable != null)
				{
					fKConstraintsInfo = new ForeignKeyArray();
					foreach (DataRow row in dataTable.Rows)
						fKConstraintsInfo.Add(row[DBSchemaStrings.Name].ToString(), row[DBSchemaStrings.PKTableName].ToString());
				}
			}
			catch (TBException e)
			{				
				Debug.Fail(e.Message);
			}
		}
	
		/// <summary>
		/// carica le informazioni relative alla foreign key constraints che riferiscono
		/// alla tabella (la lettura viene effettuata direttamente sulle tabelle di sistema)
		/// </summary>
		//---------------------------------------------------------------------------
		public void LoadRefFKConstraints(TBConnection tbConnect)
		{
			if (refFKConstraints != null)
				return;
		
			TBDatabaseSchema dbSchema = new TBDatabaseSchema(tbConnect);
			
			try
			{
				FKDataTable dataTable = dbSchema.LoadRefFKConstraints(TableName);
				
				if (dataTable != null)
				{
					refFKConstraints = new ArrayList();
					foreach (DataRow row in dataTable.Rows)
						refFKConstraints.Add(new RefFKConstraint(row[DBSchemaStrings.Name].ToString(), row[DBSchemaStrings.PKTableName].ToString()));
				}
			}
			catch (TBException e)
			{				
				Debug.Fail(e.Message);
			}
		}

		// legge dalle tabelle di sistema le informazioni relative alle foreignkey su SQLServer
		// devo andare attraverso una connessione OLEDB
		//---------------------------------------------------------------------------
		public void LoadFkConstraintsInfo(OleDbConnection oleDbConnection)
		{
			TBDatabaseSchemaOleDb dataSchema = new TBDatabaseSchemaOleDb(oleDbConnection);
			DataTable dataTable = null;

			try
			{
				dataTable = dataSchema.LoadFkConstraintsInfo(TableName);
				if (dataTable != null)
					fKConstraintsInfo = new ForeignKeyArray(dataTable);
			}
			catch (TBException)
			{
				throw;
			}
		}

		// legge dalle tabelle di sistema le informazioni relative alle foreignkey su Oracle
		// devo andare attraverso una connessione OLEDB
		//---------------------------------------------------------------------------
		public void LoadOracleFKConstraintsInfo(TBConnection tbConnection)
		{
			TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnection);
			DataTable dataTable = null;

			try
			{
				dataTable = dataSchema.LoadOracleFKConstraintsAllInfo(TableName);
				if (dataTable != null)
					fKConstraintsInfo = new ForeignKeyArray(dataTable);
			}
			catch (TBException)
			{
				throw;
			}
		}


        public void LoadPostgreFKConstraintsInfo(TBConnection tbConnection)
        {
            TBDatabaseSchema dataSchema = new TBDatabaseSchema(tbConnection);
            DataTable dataTable = null;

            try
            {
                dataTable = dataSchema.LoadPostgreFkConstraintsInfo(TableName);
                if (dataTable != null)
                    fKConstraintsInfo = new ForeignKeyArray(dataTable);
            }
            catch (TBException)
            {
                throw;
            }
        }

		#endregion

		#region Gestione ParamsInfo
		//---------------------------------------------------------------------------
		public void LoadParamsInfo(TBConnection tbConnect)
		{
			if (paramsInfo != null)
				return;

			if (tbConnect.IsSqlConnection())
				dbmsType = DBMSType.SQLSERVER;
			else if (tbConnect.IsOracleConnection())
				dbmsType = DBMSType.ORACLE;
            else if (tbConnect.IsPostgreConnection())
                dbmsType = DBMSType.POSTGRE;
			else
				throw (new TBException("CatalogTableEntry.LoadColumnsInfo: Invalid connection"));

			DataTable paramsTable = null;
			paramsInfo = new ArrayList();

			try
			{
				TBDatabaseSchema dbSchema = new TBDatabaseSchema(tbConnect);
				paramsTable = dbSchema.GetParameters(TableName);

				foreach (DataRow columnRow in paramsTable.Rows)
				{
					paramsInfo.Add(new CatalogParameter(columnRow, dbmsType));
				}
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
		public void LoadParamsInfo(TBConnection tbConnect)
		{
			catalogTableInfo.LoadParamsInfo(tbConnect);
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

		//--------------------------------------------------------------------------
		public void LoadAllInformationSchema(TBConnection connection, OleDbConnection oleDbConnect, bool withKey)
		{
			try
			{// le informazioni di chiave primaria le inserisco le so dalla lettura degli indici
				catalogTableInfo.LoadColumnsInfo(connection, withKey);

				// devo differenziare le due connessioni poichè nel caso di SqlServer leggo le informazioni
				// attraverso il metodo GetOleDbSchemaTable di una OleDbConnection mentre nel caso di Oracle
				// le informazioni sono lette direttamente nelle view di sistema a causa della chiamata OleDb
				// troppo lenta
				if (connection.IsOracleConnection())
				{
					catalogTableInfo.LoadOracleIndexes(connection);
					catalogTableInfo.LoadOracleFKConstraintsInfo(connection);
				}
                else if (connection.IsPostgreConnection())
                {
                    catalogTableInfo.LoadPostgreIndexes(connection);
                    catalogTableInfo.LoadPostgreFKConstraintsInfo(connection);
                }
				else
				{
					catalogTableInfo.LoadIndexes(oleDbConnect);
					catalogTableInfo.LoadFkConstraintsInfo(oleDbConnect);
				}

				catalogTableInfo.LoadDefaults(connection);
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
		public Int16 GetColumnPrecision(string columnName)
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
		# region Variables and Properties

		public string WhereClause = string.Empty;
		public int RecordCount = 0; // numero di record della tabella
		public string Namespace = string.Empty; // full namespace della tabella (RowSecurityLayer)

		private StringCollection selectedColumnsList = new StringCollection();

		//---------------------------------------------------------------------------
		public bool HasAutoIncrementalCol { get { return catalogTableInfo.HasAutoIncrementalCol; } }
		public bool HasDataTextColumn { get { return catalogTableInfo.HasDataTextColumn; } }

		public StringCollection	SelectedColumnsList { get { return selectedColumnsList; } }
		public DBMSType DBMSType { get { return dbmsType; }}
		# endregion

		//---------------------------------------------------------------------------
		public CatalogTableEntry(string name, DBMSType dbmsType)
			:
			base(name, dbmsType)
		{}

		/// <summary>
		/// imposta tutte le colonne della tabella come selezionate
		/// </summary>
		//---------------------------------------------------------------------------
		public void SetAllColsAsSelected(bool isKeyInfoSelected)
		{
			if (catalogTableInfo.ColumnsInfo == null)
				return;

			foreach (CatalogColumn col in catalogTableInfo.ColumnsInfo)
				col.Selected = true;

			if (isKeyInfoSelected && catalogTableInfo.IndexesInfo != null && catalogTableInfo.IndexesInfo.PrimaryKeyIndex != null)
				catalogTableInfo.IndexesInfo.PrimaryKeyIndex.Selected = true;
		}

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
			
		#region Funzioni per la creazione degli script SQL
		//---------------------------------------------------------------------------
		public virtual bool GetCreateScript
			(
			out string	tableSql, 
			out string	foreignKeysSql, 
			out string	dropForeignKeySql, 
				bool	existTargetDBTable
			)
		{

			tableSql = foreignKeysSql = dropForeignKeySql = string.Empty;
			
			string dropTableScript = string.Empty;

			if (dbmsType == DBMSType.SQLSERVER)
				dropTableScript = string.Format("if exists (select * from dbo.sysobjects where id = object_id(N'[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [{0}]\r\nGO\r\n", TableName);
            else if (dbmsType == DBMSType.POSTGRE)
                dropTableScript = string.Format("DROP TABLE IF EXISTS {0}; ", TableName);
			else
				if (existTargetDBTable) // questo vale per Oracle
					dropTableScript = string.Format("DROP TABLE {0}\r\nGO\r\n", TableName);
		
			tableSql += dropTableScript;
			
			string colsScript = CreateColumnsScript(false, existTargetDBTable);
			if (colsScript.Length == 0)
			{
				tableSql = foreignKeysSql = dropForeignKeySql = string.Empty;
				return false;
			}

			//CREATE TABLE 
			if (dbmsType == DBMSType.ORACLE)
				tableSql += string.Format("CREATE TABLE \"{0}\" \r\n(\r\n{1})\r\nGO\r\n\r\n", TableName.ToUpper(CultureInfo.InvariantCulture), colsScript);
			else if (dbmsType == DBMSType.SQLSERVER)
				tableSql += string.Format("BEGIN\r\nCREATE TABLE [{0}] \r\n(\r\n{1}) ON [PRIMARY]\r\n\r\n", TableName, colsScript);
            else if (dbmsType == DBMSType.POSTGRE)
                tableSql += string.Format("BEGIN\r\nCREATE TABLE {0} ({1}); ", TableName, colsScript);
					
			//INDICI (considero anche la primarykey)
			if (catalogTableInfo.IndexesInfo != null)
				tableSql += catalogTableInfo.IndexesInfo.ToSql(TableName, dbmsType);
			
			//END
			if (dbmsType == DBMSType.SQLSERVER)
				tableSql += "END\r\nGO\r\n\r\n";

            if (dbmsType == DBMSType.POSTGRE)
                tableSql += ";";
		
			//Creazione e rimozione foreign keys
			if (catalogTableInfo.FKConstraintsInfo != null)
				catalogTableInfo.FKConstraintsInfo.ToSql(TableName, dbmsType, out foreignKeysSql, out dropForeignKeySql);
		
			return true;
		}

		/// <summary>
		/// Script di INSERT INTO specifico per SQLServer e compatibili
		/// </summary>
		//----------------------------------------------------------------------
		public string GetInsertIntoScriptForSql(string sourceDB, string destinationDB)
		{
			string script = string.Empty;

			if (catalogTableInfo.ColumnsInfo == null)
				return script;

			if (catalogTableInfo.HasAutoIncrementalCol)
				script = string.Format(DatabaseLayerConsts.SetIdentityInsertON, TableName);

            script += string.Format("INSERT INTO [{0}].dbo.[{1}]\r\n(", destinationDB, TableName);
			
			string columnsScript = string.Empty;
			foreach (CatalogColumn columnInfo in catalogTableInfo.ColumnsInfo)
			{
				if (!columnInfo.Selected)
					continue;

				columnsScript += string.Format("[{0}], ", columnInfo.Name);
			}

			if (columnsScript.Length == 0)
				return string.Empty;

			columnsScript = columnsScript.Substring(0, columnsScript.Length - 2);

			script += columnsScript;
			script += ")\r\nSELECT ";
			script += columnsScript;
			script += string.Format("\r\nFROM [{0}].dbo.[{1}]\r\n", sourceDB, TableName);

			if (catalogTableInfo.HasAutoIncrementalCol)
				script += string.Format(DatabaseLayerConsts.SetIdentityInsertOFF, TableName);
		
			return script;
		}

        /// <summary>
        /// Script di INSERT INTO specifico per Postgre
        /// </summary>
        //----------------------------------------------------------------------
        public string GetInsertIntoScriptForPostgre(string sourceDB, string destinationDB)
        {
            string script = string.Empty;

            if (catalogTableInfo.ColumnsInfo == null)
                return script;

            if (catalogTableInfo.HasAutoIncrementalCol)
                script = string.Format(DatabaseLayerConsts.SetIdentityInsertON, TableName);

            script += string.Format("INSERT INTO {0}.{1}.{2}\r\n(", destinationDB, DatabaseLayerConsts.postgreDefaultSchema, TableName);

            string columnsScript = string.Empty;
            foreach (CatalogColumn columnInfo in catalogTableInfo.ColumnsInfo)
            {
                if (!columnInfo.Selected)
                    continue;

                columnsScript += string.Format("{0}, ", columnInfo.Name);
            }

            if (columnsScript.Length == 0)
                return string.Empty;

            columnsScript = columnsScript.Substring(0, columnsScript.Length - 2);

            script += columnsScript;
            script += ") SELECT ";
            script += columnsScript;
            script += string.Format("FROM {0}.{1}.{2}; ", sourceDB, DatabaseLayerConsts.postgreDefaultSchema, TableName);

            if (catalogTableInfo.HasAutoIncrementalCol)
                script += string.Format(DatabaseLayerConsts.SetIdentityInsertOFF, TableName);

            return script;
        }



		/// <summary>
		/// Script di INSERT INTO specifico per ORACLE
		/// </summary>
		//----------------------------------------------------------------------
		private string GetInsertIntoScriptForOracle(string sourceDB, string destinationDB)
		{
			if (catalogTableInfo.ColumnsInfo == null)
				return string.Empty;
	
			string script = string.Format("INSERT INTO \"{0}\".\"{1}\" (", destinationDB, TableName);
			
			string columsScript = string.Empty;
			foreach (CatalogColumn columnInfo in catalogTableInfo.ColumnsInfo)
			{
				if (!columnInfo.Selected)
					continue;

				columsScript += string.Format("\"{0}\", ", columnInfo.Name);
			}

			if (columsScript.Length == 0)
				return string.Empty;

			columsScript = columsScript.Substring(0, columsScript.Length - 2);

			script += columsScript;
			script += ") SELECT ";
			script += columsScript;
			script += string.Format("FROM \"{0}\".\"{1}\"", sourceDB, TableName);
		
			return script;
		}

		/*
		INSERT INTO [MagoNet].[MN_Agenti]
		([Agente], [Nome], [Dipendente], [Fornitore], [Enasarco], [EUnCapoArea], [CapoArea], [PercMaturazioneDataFattura], [TipoMaturazione], [FissoMese], [Disattivo], [Monomandatario], [DataInizioCollaborazione], [DataFineCollaborazione], [EUnaSocieta], [ProvvCorpoDocumento], [DataCambioMandato], [EUnaSocietaDiCapitali], [BloccaModificaProvv], [Politica], [ProvvigioneBase], [ProvvigioneBaseCapoArea])
		SELECT	 [Agente], [Nome], [Dipendente], [Fornitore], [Enasarco], [CapoArea], [CpAreaPrim], [PrcMatADF], [MatDefault], [FissoMese], [Disattivo], [Monomand], [DataIniz], [DataFine], [Societa], [ProvCorpo], [CambioMand], [SocDiCapitali], [BloccaModificaProvv], [CodicePolitica], [ProvvigioneBase], [ProvvigioneBaseCapoArea] 
		FROM [MagoXp].[Agenti]
		*/
		//----------------------------------------------------------------------
		public string GetInsertIntoScript(string sourceDB, string destinationDB)
		{
			if (catalogTableInfo.ColumnsInfo == null || sourceDB == null || sourceDB.Length == 0 || 
				destinationDB == null || destinationDB.Length == 0)
			{
				Debug.Assert(false);
				return string.Empty;
			}

			if (dbmsType == DBMSType.SQLSERVER)
				return GetInsertIntoScriptForSql(sourceDB, destinationDB);
			else if (dbmsType == DBMSType.ORACLE)
				return GetInsertIntoScriptForOracle(sourceDB, destinationDB);
            else if (dbmsType == DBMSType.POSTGRE)
                return GetInsertIntoScriptForPostgre(sourceDB, destinationDB);
			else
				throw (new TBException("IndexInfo.ToSql: " + string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
		}

		/// <summary>
		/// restituisce la stringa contenente lo script di creazione delle colonne della tabella. 
		/// Se onlyKeyColumns == true effettuo lo script solo per le colonne di chiave primaria
		/// </summary>
		//----------------------------------------------------------------------
		public string CreateColumnsScript(bool onlyKeyColumns, bool withAutoIncrement = true)
		{
			return CreateColumnsScript(onlyKeyColumns, false, withAutoIncrement);
		}

		/// <summary>
		/// restituisce la stringa contenente lo script di creazione delle colonne della tabella. 
		/// Se onlyKeyColumns == true effettuo lo script solo per le colonne di chiave primaria
		/// Se withMandatoryCols = true aggiungo anche le colonne obbligatorie TBCreated e TBModified
		/// </summary>
		//----------------------------------------------------------------------
		public string CreateColumnsScript(bool onlyKeyColumns, bool withMandatoryCols, bool withAutoIncrement = true)
		{
			if (catalogTableInfo.ColumnsInfo == null)
				return string.Empty;

			string columnScript = string.Empty;

			for (int n = 0; n < catalogTableInfo.ColumnsInfo.Count; n++)
			{
				CatalogColumn columnInfo = (CatalogColumn)catalogTableInfo.ColumnsInfo[n];
				if ((onlyKeyColumns && columnInfo.IsKey) || columnInfo.Selected)
				{
					if (columnScript.Length > 0)
						columnScript += ",";

					//script per la singola colonna
					columnScript += columnInfo.ToSql(true, withAutoIncrement) + "\r\n";
				}
			}

			// aggiungo in coda le colonne TBCreated e TBModified
			if (withMandatoryCols)
			{
                if (dbmsType == DBMSType.SQLSERVER)
                {
                    columnScript += string.Format(DatabaseLayerConsts.AddMandatoryColsForSQL, this.TableName);
                    columnScript += string.Format(DatabaseLayerConsts.AddWorkersMandatoryColsForSQL, this.TableName);
                }
                else if (dbmsType == DBMSType.ORACLE)
                {
                    columnScript += string.Format(DatabaseLayerConsts.AddMandatoryColsForOracle, this.TableName);
                    columnScript += string.Format(DatabaseLayerConsts.AddWorkersMandatoryColsForOracle, this.TableName);
                }
                else if (dbmsType == DBMSType.POSTGRE)
                {
                    columnScript += string.Format(DatabaseLayerConsts.AddMandatoryColsForPostgre, this.TableName);
                    columnScript += string.Format(DatabaseLayerConsts.AddWorkersMandatoryColsForPostgre, this.TableName);
                }
			}

			if (columnScript.Length > 0)
				return columnScript += "\r\n";
			
			return columnScript;
		}

		//----------------------------------------------------------------------
		protected string CreatePKConstraints()
		{
			return
				(catalogTableInfo.IndexesInfo != null && catalogTableInfo.IndexesInfo.PrimaryKeyIndex != null)
				? catalogTableInfo.IndexesInfo.PrimaryKeyIndex.ToSql(TableName, dbmsType) 
				: string.Empty;
		}	
		#endregion

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

                foreach (ForeignKeyInfo fkInfo in catalogTableInfo.FKConstraintsInfo)
                {

                    if (fkCommand.Connection.IsOracleConnection())
                        fkCommand.CommandText = string.Format
                            (
                            "ALTER TABLE {0} DISABLE CONSTRAINT {1}",
                            TableName,
                            fkInfo.Name
                            );
                    else if (fkCommand.Connection.IsSqlConnection())
                        fkCommand.CommandText = string.Format
                            (
                            "ALTER TABLE {0} NOCHECK CONSTRAINT {1}",
                            TableName,
                            fkInfo.Name
                            );

					fkCommand.ExecuteNonQuery();
				}
			}
			catch (TBException)
			{
				throw;
			}
		}

		// Cancello le FK associati alla tabella (utilizzato dal MigrationManager)
		//---------------------------------------------------------------------------
		public void DropForeignKey(TBCommand fkCommand)
		{
			if (catalogTableInfo.FKConstraintsInfo == null)
				return;

			string error = string.Empty;
			string createSQL = string.Empty;
			string dropSQL = string.Empty;

			foreach (ForeignKeyInfo fkInfo in catalogTableInfo.FKConstraintsInfo)
			{
				createSQL = string.Empty;
				dropSQL = string.Empty;

				fkInfo.ToSql(TableName, dbmsType, ref createSQL, ref dropSQL);
				fkCommand.CommandText = dropSQL;

				try
				{
					fkCommand.ExecuteNonQuery();
				}
				catch (TBException exc)
				{
					error += exc.Message + "\r\n";
				}
			}

			if (error.Length > 0)
				throw new TBException(error);
		}

		// Cancello le FK associati alla tabella (utilizzato dal MigrationManager)
		//---------------------------------------------------------------------------
		public void CreateForeignKey(TBCommand fkCommand, bool deleteRows)
		{
			if (catalogTableInfo.FKConstraintsInfo == null)
				return;

			string error = string.Empty;
			string createSQL = string.Empty;
			string dropSQL = string.Empty;

			foreach (ForeignKeyInfo fkInfo in catalogTableInfo.FKConstraintsInfo)
			{
				createSQL = string.Empty;
				dropSQL = string.Empty;

				fkInfo.ToSql(TableName, dbmsType, ref createSQL, ref dropSQL);
				fkCommand.CommandText = createSQL;

				try
				{
					fkCommand.ExecuteNonQuery();
				}
				catch (TBException exc)
				{
					if ((fkCommand.Connection.IsSqlConnection() && exc.Number == 547) ||
						(fkCommand.Connection.IsOracleConnection() && exc.Number == 2298))
					{
						if (deleteRows)
							DeleteNoIntegrityRows(fkCommand.Connection, fkInfo, ref error);
						else
							GetNoIntegrityRows(fkCommand.Connection, fkInfo, ref error);
					}
					else
						error += exc.Message + "\r\n";
				}
			}

			if (error.Length > 0)
				throw new TBException(error);
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
					case DBMSType.ORACLE:
						selectCols += string.Format("\"{0}\".\"{1}\"", TableName, pkSegName);
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
						joinText += string.Format
							(
							"[{0}].[{1}] = [{2}].[{3}]",
							TableName,
							colInfo.FKColumn,
							fkInfo.PKTableName,
							colInfo.PKColumn
							);
						break;
					case DBMSType.ORACLE:
						joinText += string.Format
							(
							"\"{0}\".\"{1}\" = \"{2}\".\"{3}\"",
							TableName,
							colInfo.FKColumn,
							fkInfo.PKTableName,
							colInfo.PKColumn
							);
						break;
                    case DBMSType.POSTGRE:
                        joinText += string.Format
                            (
                            "{0}.{1} = {2}.{3}",
                            TableName,
                            colInfo.FKColumn,
                            fkInfo.PKTableName,
                            colInfo.PKColumn
                            );
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
						case DBMSType.ORACLE:
							selectCols += string.Format("\"{0}\".\"{1}\"", TableName, colInfo.FKColumn);
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
					error += string.Format(DatabaseLayerStrings.NoIntegrityRows, TableName, fkInfo.Name);
					error += "\r\n";
					error += string.Format(DatabaseLayerStrings.RowsAffected, rowsAffected);
					error += "\r\n";
					error += string.Format(DatabaseLayerStrings.ListNoIntegrityRows, cmdText);
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
						joinText += string.Format("[{0}].[{1}] = [{2}].[{3}]",
												TableName, colInfo.FKColumn, fkInfo.PKTableName, colInfo.PKColumn);
						break;
					case DBMSType.ORACLE:
						joinText += string.Format("\"{0}\".\"{1}\" = \"{2}\".\"{3}\"",
												TableName, colInfo.FKColumn, fkInfo.PKTableName, colInfo.PKColumn);
						break;
                    case DBMSType.POSTGRE:
                        joinText += string.Format("{0}.{1} = {2}.{3}",
                                                TableName, colInfo.FKColumn, fkInfo.PKTableName, colInfo.PKColumn);
                        break;
				}
			}

			string cmdText = string.Format
				(
				"DELETE {0} WHERE NOT EXISTS (SELECT {1} FROM {2} WHERE {3})",
				TableName,
				((ForeignColumnInfo)fkInfo.Columns[0]).PKColumn,
				fkInfo.PKTableName,
				joinText
				);

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

			string sqlSql = (withCheck)
			? "ALTER TABLE [{0}] WITH CHECK CHECK CONSTRAINT [{1}]"
			: "ALTER TABLE [{0}] CHECK CONSTRAINT [{1}]";

			string sqlOracle = (withCheck)
			? "ALTER TABLE {0} ENABLE CONSTRAINT {1}"
			: "ALTER TABLE {0} ENABLE CONSTRAINT {1}";

			foreach (ForeignKeyInfo fkInfo in catalogTableInfo.FKConstraintsInfo)
			{
				fkCommand.CommandText = string.Format
					(
					(fkCommand.Connection.IsOracleConnection()) ? sqlOracle : sqlSql,
					TableName, fkInfo.Name
					);

				try
				{
					fkCommand.ExecuteNonQuery();
				}
				catch (TBException exc)
				{
					if (
						(fkCommand.Connection.IsSqlConnection() && exc.Number == 547) ||
						(fkCommand.Connection.IsOracleConnection() && exc.Number == 2298)
						)
					{
						if (deleteRows)
							DeleteNoIntegrityRows(fkCommand.Connection, fkInfo, ref error);
						else
							GetNoIntegrityRows(fkCommand.Connection, fkInfo, ref error);
					}

					//attenzione ORACLE quando viene effettuato l'enable controlla sempre i dati inseriti nella tabella
					if (!fkCommand.Connection.IsOracleConnection() || withCheck)
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
				if (string.Compare(cc.DataTypeName, dataTypeName, StringComparison.InvariantCultureIgnoreCase) == 0)
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
			{
				if (string.Compare(cc.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)
					return cc;
			}

			return null;
		}

		#endregion
	}
	#endregion	

	#region Class CatalogViewEntry
	//============================================================================
	public class CatalogViewEntry : CatalogEntry
	{
		public	string	ViewDefinition;	
		
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
		private string dbCollation; //collation del database
		private bool valid = true;
	
		//---------------------------------------------------------------------------
		public	ArrayList	TblDBList		{ get { return tblDBList; } }
		public	ArrayList	VwDBList		{ get { return vwDBList; }  }	
		public	ArrayList	RoutineDBList	{ get { return routineDBList; }  }
		public  bool		Valid			{ get { return valid; } }	
		public	string		SchemaName		{ get { return schemaName; } }
		# endregion

		# region Events and delegates per interrompere l'elaborazione (dal MigrationKit)
		public delegate void CancelLoadingCustomObj();
		public event CancelLoadingCustomObj OnCancelLoadingCustomObj;

		public delegate void CatalogEntryLoaded();
		public event CatalogEntryLoaded OnCatalogEntryLoaded;		
		# endregion

		# region Constructor
		//---------------------------------------------------------------------------
		public CatalogInfo()
		{
			tblDBList	= new ArrayList();
			vwDBList	= new ArrayList();
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
				else if (connection.IsOracleConnection())
					dbmsType = DBMSType.ORACLE;
                else if (connection.IsPostgreConnection())
                    dbmsType = DBMSType.POSTGRE;
				else
					throw (new TBException("CatalogTableEntry.LoadColumnsInfo: Invalid connection"));

				AddObjectsToCatalog(dbSchema.GetAllSchemaObjects(DBObjectTypes.TABLE), dbmsType);
					
				if (!onlyTables)
				{
					AddObjectsToCatalog(dbSchema.GetAllSchemaObjects(DBObjectTypes.VIEW), dbmsType);
					AddObjectsToCatalog(dbSchema.GetAllSchemaObjects(DBObjectTypes.ROUTINE), dbmsType);
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
		//---------------------------------------------------------------------------
		public void LoadAllInformationSchema(TBConnection connection, bool withKey)
		{
			if (OnCancelLoadingCustomObj != null)
			{
				OnCancelLoadingCustomObj();
				return;
			}

			try
			{
				Load(connection, false);
			}
			catch (TBException)
			{
				throw;
			}

			OleDbConnection oleDbConnect = connection.GetOleDbConnection();
			
			try
			{
				oleDbConnect.Open();
			}
			catch(OleDbException e)
			{
				valid = false;
				throw(new TBException(e.Message));
			}

			CatalogTableEntry table = null;

			try
			{
				foreach(CatalogTableEntry catEntry in tblDBList)
				{
					if (connection.IsOracleConnection() && catEntry.TableName.StartsWith("BIN$"))
						continue;

					table = catEntry; // solo per il debug
					catEntry.LoadAllInformationSchema(connection, oleDbConnect, withKey);
				
					if (OnCatalogEntryLoaded != null)
						OnCatalogEntryLoaded();

					if (OnCancelLoadingCustomObj != null)
					{
						OnCancelLoadingCustomObj();
						break;
					}
				}
			}
			catch(TBException e)
			{
				oleDbConnect.Close();
				Debug.Fail(e.Message);
				Debug.WriteLine("ERRORE ANALIZZANDO LA TABELLA " + table != null ? table.TableName : "????");				
				throw;
			}	
			oleDbConnect.Close();
		}

		//---------------------------------------------------------------------------
		public void LoadTableBlockInfo(string connectionString, DBMSType dmsType, List<int> tablesIdx)
		{
			if (tablesIdx.Count <= 0)
				return;

			TBConnection connection = new TBConnection(connectionString, dmsType);
			OleDbConnection oleDbConnect = connection.GetOleDbConnection();
			
			try
			{
				oleDbConnect.Open();
			}
			catch (OleDbException e)
			{
				valid = false;
				throw (new TBException(e.Message));
			}

			try
			{
				connection.Open();
			}
			catch (TBException e)
			{
				oleDbConnect.Close();
				valid = false;
				throw (e);
			}

			CatalogTableEntry catEntry = null;
			try
			{
				for(int i = 0; i < tablesIdx.Count; i++)
				{	
					catEntry = (CatalogTableEntry)tblDBList[tablesIdx[i]];
					if (catEntry != null)
					{
						catEntry.LoadAllInformationSchema(connection, oleDbConnect, true);
						
						if (OnCatalogEntryLoaded != null)
							OnCatalogEntryLoaded();					
					}
				}
			}
			catch (TBException e)
			{
				oleDbConnect.Close();
				connection.Close();
				valid = false;
				Debug.Fail(e.Message);
				Debug.WriteLine("ERRORE ANALIZZANDO LA TABELLA " + catEntry != null ? catEntry.TableName : "????");
				throw;
			}
			
			oleDbConnect.Close();
			connection.Close();
		}

		//---------------------------------------------------------------------------
		private void LoadTablesInfo(string connectionString, DBMSType dmsType)
		{
			int entriesCount = (tblDBList.Count > 10) ? tblDBList.Count / 10 : tblDBList.Count;
			int taskCount = (tblDBList.Count > 10) ? 10 : 1;
			
			Task[] loadTasks = new Task[taskCount];
			var tknSource = new CancellationTokenSource();
			var tkn = tknSource.Token;

			int start = 0;
			int end = entriesCount;
			for (int t = 0; t < taskCount; t++)
			{
				if (t == taskCount)
					end = tblDBList.Count;

				List<int> tableIdx = new List<int>();
				for (int i = start; i < end; i++)
					tableIdx.Add(i);

				loadTasks[t] = Task.Factory.StartNew(iter => LoadTableBlockInfo(connectionString, dmsType, tableIdx), t, TaskCreationOptions.AttachedToParent);

				start += entriesCount;
				end += entriesCount;
			}

			try
			{
				Task.WaitAll(loadTasks);
			}
			catch (AggregateException e)
			{
				valid = false;
				string msg = string.Empty;
				foreach (var v in e.InnerExceptions)
					msg += v.Message;
				TBException ex = new TBException(msg);
				throw (ex);
			}	
		}

		//---------------------------------------------------------------------------
		private void LoadViewsInfo(string connectionString, DBMSType dmsType)
		{
			if (vwDBList.Count <= 0)
				return;

			TBConnection connection = new TBConnection(connectionString, dmsType);
			try
			{
				connection.Open();
			}
			catch (TBException e)
			{
				valid = false;
				throw (e);
			}

			CatalogViewEntry view = null;
			try
			{
				foreach (CatalogViewEntry viewEntry in vwDBList)
				{
					if (viewEntry != null)
					{
						view = viewEntry;
						viewEntry.LoadColumnsInfo(connection, false);

						if (OnCatalogEntryLoaded != null)
							OnCatalogEntryLoaded();
					}
				}
			}
			catch (TBException e)
			{
				connection.Close();
				valid = false;
				Debug.Fail(e.Message);
				Debug.WriteLine("ERRORE ANALIZZANDO LA VIEW " + view != null ? view.TableName : "????");
				throw;
			}
			
			connection.Close();
		}

		//---------------------------------------------------------------------------
		private void LoadProceduresInfo(string connectionString, DBMSType dmsType)
		{
			if (routineDBList.Count <= 0)
				return;

			TBConnection connection = new TBConnection(connectionString, dmsType);
			try
			{
				connection.Open();
			}
			catch (TBException e)
			{
				valid = false;
				throw (e);
			}

			CatalogRoutineEntry proc = null;
			try
			{
				foreach (CatalogRoutineEntry procEntry in routineDBList)
				{
					if (procEntry != null)
					{
						proc = procEntry;
						procEntry.LoadColumnsInfo(connection, false);
						procEntry.LoadParamsInfo(connection);

						if (OnCatalogEntryLoaded != null)
							OnCatalogEntryLoaded();
					}
				}
			}
			catch (TBException e)
			{
				connection.Close();
				valid = false;
				Debug.Fail(e.Message);
				Debug.WriteLine("ERRORE ANALIZZANDO LA VIEW " + proc != null ? proc.TableName : "????");
				throw;
			}

			connection.Close();
		}

		//---------------------------------------------------------------------------
		public void LoadAllInformation(string connectionString, DBMSType dmsType)
		{			
			TBConnection connection = new TBConnection(connectionString, dmsType);
			
			try
			{
				connection.Open();
				Load(connection, false);
				dbCollation = TBCheckDatabase.GetDatabaseCollation(connection);
				if (string.IsNullOrEmpty(dbCollation))
					dbCollation = TBCheckDatabase.GetServerCollation(connection);
				connection.Close();
				if (tblDBList.Count <= 0)
					return;
			}
			catch (TBException e)
			{
				if (connection.State == ConnectionState.Open)
					connection.Close();
				valid = false;
				throw (e);
			}
			
			//to increase the perform I run tree parallel task to load tables, views and stored procedures information
			Task[] loadTasks = new Task[3]
			{	
				Task.Factory.StartNew( () => LoadTablesInfo(connectionString, dmsType)),
				Task.Factory.StartNew( () => LoadViewsInfo(connectionString, dmsType)),
				Task.Factory.StartNew( () => LoadProceduresInfo(connectionString, dmsType))
			};

			try
			{
				Task.WaitAll(loadTasks);
			}		
			catch (AggregateException e)
			{
				valid = false;
				string msg = string.Empty;
				foreach (var v in e.InnerExceptions)
					msg += v.Message;			
				TBException ex = new TBException(msg);
				throw (ex);				
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
		private void AddObjectsToCatalog(SchemaDataTable dataTable, DBMSType dbmsType)
		{
			foreach (DataRow row in dataTable.Rows)
				AddCatalogEntry(row, dbmsType);
		}

		//---------------------------------------------------------------------------
		private void AddCatalogEntry(DataRow row, DBMSType dbmsType)
		{
			DBObjectTypes type = (DBObjectTypes)row[DBSchemaStrings.Type];
			
			switch (type)
			{
				case DBObjectTypes.TABLE:
					TblDBList.Add(new CatalogTableEntry((string)row[DBSchemaStrings.Name], dbmsType));
					break;

				case DBObjectTypes.VIEW:
					VwDBList.Add(new CatalogViewEntry((string)row[DBSchemaStrings.Name], (string)row[DBSchemaStrings.Definition], dbmsType));
					break;

				case DBObjectTypes.ROUTINE:
					RoutineDBList.Add(new CatalogRoutineEntry((string)row[DBSchemaStrings.Name], (string)row[DBSchemaStrings.Definition], (string)row[DBSchemaStrings.RoutineType], dbmsType));
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
					{
						if (string.Compare(entry.TableName, name, true, CultureInfo.InvariantCulture) == 0)
							return true;
					}
					break;
				}

				case DBObjectTypes.VIEW:
				{
					foreach (CatalogEntry entry in VwDBList)
					{
						if (string.Compare(entry.TableName, name, true, CultureInfo.InvariantCulture) == 0)
							return true;
					}
					break;
				}

				case DBObjectTypes.ROUTINE:
				{
					foreach (CatalogEntry entry in RoutineDBList)
					{
						if (string.Compare(entry.TableName, name, true, CultureInfo.InvariantCulture) == 0)
							return true;
					}
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
				if (string.Compare(entry.TableName, tableName, true, CultureInfo.InvariantCulture) == 0)
					return entry;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public CatalogViewEntry GetViewEntry(string viewName)
		{
			foreach (CatalogViewEntry entry in VwDBList)
			{
				if (string.Compare(entry.TableName, viewName, true, CultureInfo.InvariantCulture) == 0)
					return entry;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public CatalogRoutineEntry GetSProcedureEntry(string spName)
		{
			foreach (CatalogRoutineEntry entry in routineDBList)
			{
				if (string.Compare(entry.TableName, spName, true, CultureInfo.InvariantCulture) == 0)
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
			return (string.Compare(cc.GetDBDataType(), "NVarChar", StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		//---------------------------------------------------------------------------
		public bool IsCollationCultureSensitive(CatalogColumn catalogColumn)
		{
			string collation = catalogColumn.CollationName;
			if (string.IsNullOrEmpty(collation))
				collation = dbCollation;

			if (string.IsNullOrEmpty(collation))
				return false;

			return string.Compare(collation, "Latin1_General_CI_AS", StringComparison.InvariantCultureIgnoreCase) != 0;
			
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

		/// <summary>
		/// Disabilita il check di tutti i constraint di FK 
		/// </summary>
		//---------------------------------------------------------------------------
		public void DisableCheckOnForeignKey(TBConnection connect)
		{
			TBCommand command = null;
			
			try
			{
				command = new TBCommand(connect);
				foreach (CatalogTableEntry entry in TblDBList)
					entry.DisableCheckOnForeignKey(command);
			}
			catch(TBException)
			{
				command.Dispose();
				throw;
			}
			command.Dispose();
		}

		//---------------------------------------------------------------------------
		public void DropForeignKey(TBConnection connect)
		{
			TBCommand command = new TBCommand(connect);
			command.CommandTimeout = 0;
			string error = string.Empty;

			foreach (CatalogTableEntry entry in TblDBList)
			{
				try
				{
					entry.DropForeignKey(command);
				}
				catch(TBException e)
				{
					error += e.Message + "\r\n";
				}
			}

			command.Dispose();
			if (error.Length > 0)
				throw new TBException(error);		
		}

		//---------------------------------------------------------------------------
		public void CreateForeignKey(TBConnection connect, bool deleteRow)
		{
			TBCommand command = new TBCommand(connect);
			command.CommandTimeout = 0;
			string error = string.Empty;
			
			foreach (CatalogTableEntry entry in TblDBList)
			{
				try
				{
					entry.CreateForeignKey(command, deleteRow);
				}
				catch(TBException e)
				{
					error += e.Message + "\r\n";
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