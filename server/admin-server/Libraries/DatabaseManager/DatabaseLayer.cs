using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using Microarea.Common;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Npgsql;
using NpgsqlTypes;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.AdminServer.Libraries.DatabaseManager
{
	#region Public enums
	//---------------------------------------------------------------------------
	public enum DatabaseCheckError
	{
		NoDatabase,
		NoTables,
		NoActivatedDatabase,
		InvalidModule,
		Error,
		NoError,
		DBSizeError,
        Sql2012NotAllowedForCompany,
		Sql2012NotAllowedForDMS
	}

	//---------------------------------------------------------------------------
	public enum DBObjectTypes
	{
		ALL,
		TABLE,
		VIEW,
		ROUTINE,
		SYNONYM
	};

	/// <summary>
	/// SQLServerEdition enum: per individuare l'edizione di un server SQL
	/// </summary>
	//---------------------------------------------------------------------------
	public enum SQLServerEdition
	{
		SqlServer2000,
		SqlServer2005,
		SqlServer2008,
		SqlServer2012,
        SqlServer2014,
		SqlServer2016,
		MSDE2000,
		SqlExpress2005,
		SqlExpress2008,
		SqlExpress2012,
        SqlExpress2014,
		SqlExpress2016,
		SqlAzureV12,
		Undefined
	}
	#endregion

	#region Class TBException
	/// <summary>
	/// Classe per la gestione delle Exception
	/// </summary>
	//============================================================================
	public class TBException : Exception
	{
		#region Constructors
		//---------------------------------------------------------------------------
		public TBException(string message, Exception inner) : base(message, inner) { }
		//---------------------------------------------------------------------------
		public TBException(string message) : this(message, null) { }
		//---------------------------------------------------------------------------
		public TBException() : this(string.Empty, null) { }
		#endregion

		#region Properties TBException
		//-----------------------------------------------------------------------
		public int Number
		{
			get
			{
				if (InnerException == null)
					return 0;

				if (InnerException is SqlException)
					return ((SqlException)InnerException).Number;

                if (InnerException is NpgsqlException)
                   return ((NpgsqlException)InnerException).HResult;

				return 0;
			}
		}

		//-----------------------------------------------------------------------
		public SqlErrorCollection Errors
		{
			get
			{
				if (InnerException != null && InnerException is SqlException)
					return ((SqlException)InnerException).Errors;

				return null;
			}
		}

		//-----------------------------------------------------------------------
		public string Procedure
		{
			get
			{
				if (InnerException == null)
					return string.Empty;

				if (InnerException is SqlException)
					return ((SqlException)InnerException).Procedure;

                if (InnerException is NpgsqlException)
                    return ((NpgsqlException)InnerException).Source;

				return string.Empty;
			}
		}

		//-----------------------------------------------------------------------
		public string ExtendedMessage
		{
			get
			{
				if (InnerException == null || string.IsNullOrWhiteSpace(InnerException.Message))
					return Message;
				return Message + "\n(" + InnerException.Message + ")";
			}
		}

		//-----------------------------------------------------------------------
		public string Server
		{
			get
			{
				if (InnerException != null && InnerException is SqlException)
					return ((SqlException)InnerException).Server;
				return string.Empty;
			}
		}
		#endregion
	}
	#endregion

	#region Class TBType
	/// <summary>
	/// Utilizzata per mappare i tipi SqlDbType - NpgsqlDbType
	/// </summary>
	//============================================================================
	public class TBType
	{
		private object dbType = null;

		//------------------------------------------------------------------
		public TBType(object value)
		{
			dbType = value;
		}

		#region Properties
		//------------------------------------------------------------------
		internal SqlDbType SqlDbType
		{
			get
			{
				if (dbType is SqlDbType)
					return (SqlDbType)dbType;
				throw (new TBException("TBType.SqlDbType: " + DatabaseManagerStrings.InvalidCast));
			}
			set
			{
				if (dbType is SqlDbType)
					dbType = value;
				throw (new TBException("TBType.SqlDbType: " + DatabaseManagerStrings.InvalidCast));
			}
		}

        //------------------------------------------------------------------
        internal NpgsqlDbType PostgreType
        {
            get
            {
                if (dbType is NpgsqlDbType)
                    return (NpgsqlDbType)dbType;
                throw (new TBException("TBType.PostgreType: " + DatabaseManagerStrings.InvalidCast));
            }
            set
            {
                if (dbType is NpgsqlDbType)
                    dbType = value;
                throw (new TBException("TBType.PostgreType: " + DatabaseManagerStrings.InvalidCast));
            }
        }
		#endregion

		#region OperatorCast
		//------------------------------------------------------------------
		public static implicit operator TBType(SqlDbType sqlType)
		{
			return new TBType(sqlType);
		}

		//------------------------------------------------------------------
		public static implicit operator TBType(NpgsqlDbType npgType)
        {
            return new TBType(npgType);
        }
		#endregion

		#region Methods
		//------------------------------------------------------------------
		public bool IsSqlDbType()
		{
			return dbType is SqlDbType;
		}

        //------------------------------------------------------------------
        public bool IsPostgreType()
        {
            return dbType is NpgsqlDbType;
        }
		#endregion
	}
	#endregion

	#region Class TBDatabaseType
	//============================================================================
	public class TBDatabaseType
	{
		#region Static methods
		//---------------------------------------------------------------------------		
		public static DBMSType GetDBMSType(string provider) 
		{
            if (string.Compare(provider, NameSolverDatabaseStrings.SQLOLEDBProvider, StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(provider, NameSolverDatabaseStrings.SQLODBCProvider, StringComparison.OrdinalIgnoreCase) == 0)
                return DBMSType.SQLSERVER;

            if (string.Compare(provider, NameSolverDatabaseStrings.PostgreOdbcProvider, StringComparison.OrdinalIgnoreCase) == 0)
                return DBMSType.POSTGRE;

            return DBMSType.UNKNOWN;
		}

		//---------------------------------------------------------------------------		
		public static string GetProvider(DBMSType dbmsType)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				return NameSolverDatabaseStrings.SQLOLEDBProvider;

            if (dbmsType == DBMSType.POSTGRE)
                return NameSolverDatabaseStrings.PostgreOdbcProvider;

			return string.Empty;
		}

		//---------------------------------------------------------------------------		
		public static string GetConnectionString
			(
			string server,
			string user,
			string password,
			string catalog,
			DBMSType dbmsType,
			bool winNtAuthent,
			int port=0
			)
		{
			string connectionString = string.Empty;

			switch (dbmsType)
			{
				case DBMSType.SQLSERVER:
					{
						if (server.Length == 0 || catalog.Length == 0 || (!winNtAuthent && user.Length == 0))
							return string.Empty;

						connectionString = (winNtAuthent)
							? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, server, catalog)
							: string.Format(NameSolverDatabaseStrings.SQLConnection, server, catalog, user, password);
						break;
					}

                case DBMSType.POSTGRE:
                    {
                        if (server.Length == 0 || catalog.Length == 0 || (!winNtAuthent && user.Length == 0))
                            return string.Empty;
                        if (port == 0)
							port = DatabaseLayerConsts.postgreDefaultPort;
                        if (password.Length == 0)
							password = DatabaseLayerConsts.postgreDefaultPassword;
                        connectionString = (winNtAuthent)
                            ? string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, server.ToLower(), port, catalog.ToLower(), DatabaseLayerConsts.postgreDefaultSchema)
                            : string.Format(NameSolverDatabaseStrings.PostgreConnection, server, port, catalog, user.ToLower(), password, DatabaseLayerConsts.postgreDefaultSchema);
                        break;
                    }

				default:
					throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
			}

			return connectionString;
		}

		//---------------------------------------------------------------------------		
		public static string GetConnectionString
			(
			string server,
			string user,
			string password,
			string catalog,
			string providerName,
			bool winNtAuthent,
			bool useProvider,
			int port=0
			)
		{
			DBMSType dbmsType = GetDBMSType(providerName);

			string connectionString = GetConnectionString(server, user, password, catalog, dbmsType, winNtAuthent, port);

            if (useProvider && connectionString.Length > 0)
            {
                if (string.Compare(providerName, NameSolverDatabaseStrings.SQLODBCProvider,true)==0)
                    connectionString = string.Format(NameSolverDatabaseStrings.DriverConnAttribute, providerName) + connectionString;
                else
                    connectionString = string.Format(NameSolverDatabaseStrings.ProviderConnAttribute, providerName) + connectionString;
            }

			return connectionString;
		}

		//---------------------------------------------------------------------------		
		public static string GetDBDataType(TBType dbType, DBMSType dbmsType)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				return (dbType.SqlDbType).ToString();

            if (dbmsType == DBMSType.POSTGRE)
                return (dbType.PostgreType).ToString();

			throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
		}

		//------------------------------------------------------------------
		public static TBType GetTBType(DBMSType dbmsType, int providerType)
		{
			TBType myTBType = null;

			if (dbmsType == DBMSType.SQLSERVER)
			{
				switch (providerType)
				{
					case 2: // Bit
						myTBType = new TBType(SqlDbType.Bit);
						break;
					case 3: // Char
						myTBType = new TBType(SqlDbType.Char);
						break;
					case 4: // DateTime
						myTBType = new TBType(SqlDbType.DateTime);
						break;
					case 5: // Decimal
						myTBType = new TBType(SqlDbType.Decimal);
						break;
					case 6: // Float
						myTBType = new TBType(SqlDbType.Float);
						break;
					case 8: // Int
						myTBType = new TBType(SqlDbType.Int);
						break;
					case 10: // NChar
						myTBType = new TBType(SqlDbType.NChar);
						break;
					case 11: // NText
						myTBType = new TBType(SqlDbType.NText);
						break;
					case 12: // NVarChar
						myTBType = new TBType(SqlDbType.NVarChar);
						break;
					case 14: // UniqueIdentifier
						myTBType = new TBType(SqlDbType.UniqueIdentifier);
						break;
					case 16: // SmallInt
						myTBType = new TBType(SqlDbType.SmallInt);
						break;
					case 18: // Text
						myTBType = new TBType(SqlDbType.Text);
						break;
					case 22: // VarChar
					default:
						myTBType = new TBType(SqlDbType.VarChar);
						break;
				}
			}

			return myTBType;
		}

		//------------------------------------------------------------------
		public static TBType GetTBType(DBMSType dbmsType, string providerType)
		{
			if (dbmsType != DBMSType.SQLSERVER)
				return null;

			TBType myTBType = null;


			switch (providerType)
			{
				case "bit": // Bit
					myTBType = new TBType(SqlDbType.Bit);
					break;
				case "char": // Char
					myTBType = new TBType(SqlDbType.Char);
					break;
				case "datetime": // DateTime
					myTBType = new TBType(SqlDbType.DateTime);
					break;
				case "decimal": // Decimal
					myTBType = new TBType(SqlDbType.Decimal);
					break;
				case "float": // Float
					myTBType = new TBType(SqlDbType.Float);
					break;
				case "int": // Int
					myTBType = new TBType(SqlDbType.Int);
					break;
				case "nchar": // NChar
					myTBType = new TBType(SqlDbType.NChar);
					break;
				case "ntext": // NText
					myTBType = new TBType(SqlDbType.NText);
					break;
				case "nvarchar": // NVarChar
					myTBType = new TBType(SqlDbType.NVarChar);
					break;
				case "uniqueidentifier": // UniqueIdentifier
					myTBType = new TBType(SqlDbType.UniqueIdentifier);
					break;
				case "smallint": // SmallInt
					myTBType = new TBType(SqlDbType.SmallInt);
					break;
				case "text": // Text
					myTBType = new TBType(SqlDbType.Text);
					break;
				case "xml":
					myTBType = new TBType(SqlDbType.Xml);
					break;
				case "varchar": // VarChar
				default:
					myTBType = new TBType(SqlDbType.VarChar);
					break;
			}

			return myTBType;
		}

		//@@Anastasia Solo per Poestgre. Il cast di DbType a int non funziona
		//----------------------------------------------------------------------------------
		public static TBType GetTBTypePostgre(DBMSType dbmsType, string providerType)
        {
            if (dbmsType != DBMSType.POSTGRE)
				return null;

            TBType myTBType = null;

            switch (providerType)
            {
                case "bit": // Bit
                    myTBType = new TBType(NpgsqlDbType.Bit);
                    break;
                case "char": // Char
                case "bpchar":
                    myTBType = new TBType(NpgsqlDbType.Char);
                    break;
                case "timestamp": // DateTime
                    myTBType = new TBType(NpgsqlDbType.Timestamp);
                    break;
                case "double": // Float
                case "float8":
                    myTBType = new TBType(NpgsqlDbType.Double);
                    break;
                case "integer": // Int
                case "int4":
                case "int":
                    myTBType = new TBType(NpgsqlDbType.Integer);
                    break;
                case "varchar": // Varchar
                    myTBType = new TBType(NpgsqlDbType.Varchar);
                    break;
                case "text": // Text
                    myTBType = new TBType(NpgsqlDbType.Text);
                    break;
                case "uuid": // uuid
                    myTBType = new TBType(NpgsqlDbType.Uuid);
                    break;
                case "smallint": // SmallInt
                case "int2":
                    myTBType = new TBType(NpgsqlDbType.Smallint);
                    break;
                case "Xml": // xml
                    myTBType = new TBType(NpgsqlDbType.Xml);
                    break;
                default:
                    myTBType = new TBType(NpgsqlDbType.Varchar);
                    break;
            }

            return myTBType;
        }

		//dato il ::DataType di TaskBuilder, restituisce il giusto tipo mappato nel database
		//---------------------------------------------------------------------------		
        public static TBType GetTBType(DataType dataType, DBMSType dbmsType)
        {
            if (dataType == DataType.String)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.NVarChar);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Varchar);
            }

            if (dataType == DataType.Integer)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.SmallInt);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Smallint);
            }

            if (dataType == DataType.Long || dataType == DataType.Enum)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.Int);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Integer);
            }

            if (dataType == DataType.Double || dataType == DataType.Money ||
                dataType == DataType.Quantity || dataType == DataType.Percent)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.Float);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Double);
            }

            if (dataType == DataType.Date)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.DateTime);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Timestamp);
            }

            if (dataType == DataType.Bool)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.NVarChar);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Varchar);
            }

            if (dataType == DataType.Guid)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.UniqueIdentifier);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Uuid);
            }

            if (dataType == DataType.Text)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.NText);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Varchar);
            }
			
            if (dataType == DataType.Blob)
            {
                if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.Binary);
                if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Bytea);
            }

            if (dbmsType == DBMSType.SQLSERVER) return new TBType(SqlDbType.VarChar);
            if (dbmsType == DBMSType.POSTGRE) return new TBType(NpgsqlDbType.Varchar);

            return new TBType(SqlDbType.NVarChar);
        }

		#region Metodi per ottenere un SqlDbType
        //<summary>
        ///	Per ottenere un SqlDbType da un NpgsqlDbType
        ///</summary>
        //---------------------------------------------------------------------------
        internal static SqlDbType GetSqlDbType(NpgsqlDbType npgType)
        {
            switch (npgType)
            {
                case NpgsqlDbType.Varchar: return SqlDbType.VarChar;
                case NpgsqlDbType.Char: return SqlDbType.Char;
                case NpgsqlDbType.Timestamp: return SqlDbType.DateTime;
                case NpgsqlDbType.Double: return SqlDbType.Float;
                case NpgsqlDbType.Integer: return SqlDbType.Int;
                case NpgsqlDbType.Smallint: return SqlDbType.SmallInt;
                case NpgsqlDbType.Text: return SqlDbType.Text;
            }

            return SqlDbType.VarChar;
        }

		///<summary>
		///	Per ottenere un SqlDbType da un generico TBType
		///</summary>
		//---------------------------------------------------------------------------
		internal static SqlDbType GetSqlDbType(TBType dbType)
		{
            if (dbType.IsPostgreType())
                return GetSqlDbType(dbType.PostgreType);

			if (dbType.IsSqlDbType())
				return dbType.SqlDbType;

			return SqlDbType.VarChar;
		}
		#endregion

        #region Metodi per ottenere un NpgsqlDbType
        ///<summary>
        ///	Per ottenere un PostgreDbType da un SqlDbType
        ///</summary>
        //---------------------------------------------------------------------------
        internal static NpgsqlDbType GetPostgreDbType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.VarChar: return NpgsqlDbType.Varchar;
                case SqlDbType.Char: return NpgsqlDbType.Char;
                case SqlDbType.Bit: return NpgsqlDbType.Bit;
                case SqlDbType.DateTime: return NpgsqlDbType.Timestamp;
                case SqlDbType.Float: return NpgsqlDbType.Double;
                case SqlDbType.Int: return NpgsqlDbType.Integer;
                case SqlDbType.SmallInt: return NpgsqlDbType.Smallint;
                case SqlDbType.Text: return NpgsqlDbType.Text;
                case SqlDbType.UniqueIdentifier: return NpgsqlDbType.Uuid;
            }
            return NpgsqlDbType.Varchar;
        }

        ///<summary>
        ///	Per ottenere un NpgsqlDbType da un generico TBType
        ///</summary>
        //---------------------------------------------------------------------------
        internal static NpgsqlDbType GetPostgreDbType(TBType dbType)
        {
            if (dbType.IsSqlDbType())
                return GetPostgreDbType(dbType.SqlDbType);

            if (dbType.IsPostgreType())
                return dbType.PostgreType;

            return NpgsqlDbType.Varchar;
        }
        #endregion

		//---------------------------------------------------------------------------		
		public static bool IsCharType(TBType dbType, DBMSType dbmsType)
		{
			switch (dbmsType)
			{
				case DBMSType.SQLSERVER:
					{
						SqlDbType sqlType = dbType.SqlDbType;
						return (sqlType == SqlDbType.NChar || sqlType == SqlDbType.NVarChar ||
								sqlType == SqlDbType.VarBinary || sqlType == SqlDbType.VarChar ||
								sqlType == SqlDbType.Char || sqlType == SqlDbType.Text);
					}
                case DBMSType.POSTGRE:
                    {
                        NpgsqlDbType npgType = dbType.PostgreType;
                        return (npgType == NpgsqlDbType.Char || npgType == NpgsqlDbType.Varchar ||
                               npgType == NpgsqlDbType.Text);
                    }
				default:
					return false;
			}
		}

		//---------------------------------------------------------------------------		
		public static bool HasLength(TBType dbType, DBMSType dbmsType)
		{
			if (!TBDatabaseType.IsCharType(dbType, dbmsType))
				return false;

			switch (dbmsType)
			{
				case DBMSType.SQLSERVER:
					return (dbType.SqlDbType != SqlDbType.Text);
                case DBMSType.POSTGRE:
                    return (dbType.PostgreType != NpgsqlDbType.Text);
				default:
					return false;
			}
		}

		//---------------------------------------------------------------------------		
		public static bool HasPrecision(TBType dbType, DBMSType dbmsType)
		{
			switch (dbmsType)
			{
				case DBMSType.SQLSERVER:
					return (dbType.SqlDbType == SqlDbType.Decimal);
                case DBMSType.POSTGRE:
                    return (dbType.PostgreType == NpgsqlDbType.Numeric || dbType.PostgreType == NpgsqlDbType.Double);
				default:
					return false;
			}
		}

		//---------------------------------------------------------------------------		
		public static bool HasScale(TBType dbType, DBMSType dbmsType)
		{
			switch (dbmsType)
			{
				case DBMSType.SQLSERVER:
					return (dbType.SqlDbType == SqlDbType.Decimal);
                case DBMSType.POSTGRE:
                    return (dbType.PostgreType == NpgsqlDbType.Numeric);
				default:
					return false;
			}
		}

		//---------------------------------------------------------------------------		
		public static bool IsNumericType(TBType dbType, DBMSType dbmsType)
		{
			switch (dbmsType)
			{
				case DBMSType.SQLSERVER:
					return (dbType.SqlDbType == SqlDbType.SmallInt ||
							dbType.SqlDbType == SqlDbType.Int ||
							dbType.SqlDbType == SqlDbType.Float ||
							dbType.SqlDbType == SqlDbType.Decimal);
                case DBMSType.POSTGRE:
                    return (dbType.PostgreType == NpgsqlDbType.Smallint ||
                            dbType.PostgreType == NpgsqlDbType.Integer ||
                            dbType.PostgreType == NpgsqlDbType.Double ||
                            dbType.PostgreType == NpgsqlDbType.Numeric);
				default:
					return false;
			}
		}

		//---------------------------------------------------------------------------		
		public static bool IsDateTimeType(TBType dbType, DBMSType dbmsType)
		{
			switch (dbmsType)
			{
				case DBMSType.SQLSERVER:
					return (dbType.SqlDbType == SqlDbType.SmallDateTime ||
							dbType.SqlDbType == SqlDbType.DateTime);
                case DBMSType.POSTGRE:
                    return (dbType.PostgreType == NpgsqlDbType.Date ||
                            dbType.PostgreType == NpgsqlDbType.Timestamp ||
                            dbType.PostgreType == NpgsqlDbType.TimestampTZ);
				default:
					return false;
			}
		}

		//---------------------------------------------------------------------------		
		public static bool IsGUIDType(TBType dbType, DBMSType dbmsType, int columnSize)
		{
			switch (dbmsType)
			{
				case DBMSType.SQLSERVER:
					return (dbType.SqlDbType == SqlDbType.UniqueIdentifier);
                case DBMSType.POSTGRE:
                    return (dbType.PostgreType == NpgsqlDbType.Uuid);
				default:
					return false;
			}
		}

		//-----------------------------------------------------------------------------
		public static string DBNativeConvert(object o, bool useUnicode, DBMSType dbType)
		{
			string strSqlServerDateTs = "{{ts '{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}'}}";
            string strPostgreDateTs = "to_timestamp('{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}', 'YYYY-MM-DD HH24:MI:SS'";

			// usato in Expression di woorm
			switch (o.GetType().Name)
			{
				case "Boolean":
					{
						return ObjectHelper.CastToDBBool((bool)o);
					}
				case "Byte":
				case "Int16":
				case "Int32":
				case "Int64":
				case "Decimal":
				case "Single":
				case "Double":
					{
						return o.ToString();
					}
				case "DataEnum":
					{
						return ObjectHelper.CastToDBData(o).ToString();
					}
				case "String":
					{
						string str = o.ToString().Replace("'", "''");
						return useUnicode ? String.Format("N'{0}'", str) : String.Format("'{0}'", str);
					}
                case "Guid": // @@Anastasia: non so 
					{
						return String.Format("'{0}'", o.ToString());
					}
				case "DateTime":
					{
						//Timestamp format: {ts '2001-01-15 00:00:00'}
						DateTime dt = (DateTime)o;
						string sDateConvert = string.Empty;
						switch (dbType)
						{
							case DBMSType.SQLSERVER: sDateConvert = strSqlServerDateTs; break;
                            case DBMSType.POSTGRE: sDateConvert = strPostgreDateTs; break;
						}
						return String.Format(sDateConvert, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
					}
				default:
					Debug.Fail(CoreTypeStrings.IllegalDataType);
					break;
			}
			return String.Empty;
		}

		/// <summary>
		/// ReplaceParamSymbol
		/// Data una stringa (o di comando o un parametro) contenente uno o + '@', utilizzata da SQL Server
		/// come simbolo per identificare i parametri, viene effettuata la sostituzione di:
		/// - '@' con ':' che è il simbolo dei parametri in Oracle
		/// </summary>
		/// <param name="toReplace">stringa in cui effettuare la sostituzione dei caratteri</param>
		/// <param name="dbmsType">DBMSType da applicare</param>
		/// <returns>stringa corretta</returns>
		//---------------------------------------------------------------------------
		public static string ReplaceParamSymbol(string stringToReplace, DBMSType dbmsType)
		{
			string validString = string.Empty;

			switch (dbmsType)
			{
                case DBMSType.POSTGRE:
				case DBMSType.SQLSERVER:
				default:
					validString = stringToReplace;
					break;
			}

			return validString;
		}		
		#endregion
	}
	#endregion

	#region Class TBCheckDatabase
	//============================================================================
	public class TBCheckDatabase
	{
		#region GetDatabaseVersion methods
		/// <summary>
		/// GetDatabaseVersion (istanzia al volo una TBConnection)
		/// </summary>
		/// <param name="companyDBConnectionString">stringa di connessione al db</param>
		/// <param name="providerName">nome del provider</param>
		/// <returns>enum DatabaseVersion</returns>
		//-----------------------------------------------------------------------
		public static DatabaseVersion GetDatabaseVersion(string companyDBConnectionString, string providerName)
		{
			DatabaseVersion ret = DatabaseVersion.Undefined;

			if (string.IsNullOrWhiteSpace(companyDBConnectionString))
				return ret;

			try
			{
				using (TBConnection compDBConn = new TBConnection(companyDBConnectionString, TBDatabaseType.GetDBMSType(providerName)))
				{
					compDBConn.Open();

					ret = GetDatabaseVersion(compDBConn);
				}
			}
			catch
			{
				return DatabaseVersion.Undefined;
			}

			return ret;
		}

		/// <summary>
		/// GetDatabaseVersion
		/// </summary>
		/// <param name="compDBConn">connessione aperta sul db</param>
		/// <returns>enum DatabaseVersion</returns>
		//-----------------------------------------------------------------------
		public static DatabaseVersion GetDatabaseVersion(TBConnection compDBConn)
		{
            // per Postgre non eseguo alcuna query
            if (compDBConn.IsPostgreConnection())
                return DatabaseVersion.Postgre;

			// per SqlServer eseguo la query 'SELECT @@version', visto che devo discriminare tra 4 versioni diverse
			if (compDBConn.IsSqlConnection())
			{
				SQLServerEdition currEdition = GetSQLServerEdition(compDBConn.SqlConnect);

				if (currEdition == SQLServerEdition.MSDE2000 ||
					currEdition == SQLServerEdition.SqlExpress2005 ||
					currEdition == SQLServerEdition.SqlExpress2008 ||
                    currEdition == SQLServerEdition.SqlExpress2012 ||
                    currEdition == SQLServerEdition.SqlExpress2014 ||
					currEdition == SQLServerEdition.SqlExpress2016)
					return DatabaseVersion.MSDE;

				if (currEdition == SQLServerEdition.SqlServer2000 ||
					currEdition == SQLServerEdition.SqlServer2005 ||
					currEdition == SQLServerEdition.SqlServer2008 ||
                    currEdition == SQLServerEdition.SqlServer2012 ||
                    currEdition == SQLServerEdition.SqlServer2014 ||
					currEdition == SQLServerEdition.SqlServer2016 ||
					currEdition == SQLServerEdition.SqlAzureV12)
					return DatabaseVersion.SqlServer2000;
			}

			return DatabaseVersion.Undefined;
		}
		#endregion

		#region GetSQLServerEdition
		/// <summary>
		/// GetSQLServerEdition - solo per SQL!!!
		/// Data una connessione aperta su un database di sql ritorna l'edizione del server stesso
		/// - SQL 2000				(Edition: Personal Edition - ProductVersion: 8.00.x)
		/// - MSDE 2000				(Edition: Desktop Engine - ProductVersion: 8.00.x)
		/// - SQL 2005				(Edition: Standard Edition - ProductVersion: 9.00.x)
		/// - SQL 2005 Express Ed.	(Edition: Express Edition - ProductVersion: 9.00.x)
		/// - SQL 2008				(Edition: Standard Edition - ProductVersion: 10.x) version R2: 10.5
		/// - SQL 2008 Express Ed.	(Edition: Express Edition (with Advanced Services) - ProductVersion: 10.x) version R2: 10.5
		/// - SQL 2012				(Edition: Standard Edition - ProductVersion: 11.)
		/// - SQL 2012 Express Ed.	(Edition: Express Edition - ProductVersion: 11.)
		/// - SQL 2014				(Edition: Standard Edition - ProductVersion: 12.)
		/// - SQL 2014 Express Ed.	(Edition: Express Edition - ProductVersion: 12.)
		/// - SQL 2016				(Edition: Standard Edition - ProductVersion: 13.)
		/// - SQL 2016 Express Ed.	(Edition: Express Edition - ProductVersion: 13.)
		/// - SQL Azure v.12		(Edition: SQL Azure - ProductVersion: 12.)
		/// <param name="dbConnection">connessione aperta sul db</param>
		/// <returns>enum SQLServerEdition</returns>
		/// </summary>
		//-----------------------------------------------------------------------
		public static SQLServerEdition GetSQLServerEdition(SqlConnection dbConnection)
		{
			if (dbConnection == null || dbConnection.State != ConnectionState.Open)
				return SQLServerEdition.Undefined;

			string version = string.Empty, edition = string.Empty;
			string query = "SELECT SERVERPROPERTY(N'Edition') AS Edition, SERVERPROPERTY(N'ProductVersion') AS Version";

			try
			{
				using (SqlCommand command = new SqlCommand(query, dbConnection))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							edition = (string)reader["Edition"];
							version = (string)reader["Version"];
						}
					}
				}
			}
			catch
			{
				return SQLServerEdition.Undefined;
			}

			// si tratta della versione SQL Server 2000
			if (version.StartsWith("8.00"))
			{
				if (edition.IndexOf("Desktop Engine", StringComparison.OrdinalIgnoreCase) >= 0)
					return SQLServerEdition.MSDE2000;

				return SQLServerEdition.SqlServer2000;
			}

			// si tratta della versione SQL Server 2005
			if (version.StartsWith("9.00"))
			{
				if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
					return SQLServerEdition.SqlExpress2005;

				return SQLServerEdition.SqlServer2005;
			}

			// si tratta della versione SQL Server 2008 (o R2)
			if (version.StartsWith("10."))
			{
				// eseguo la IndexOf perche' potrebbe essere Express Edition with Advanced Services
				if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
					return SQLServerEdition.SqlExpress2008;

				return SQLServerEdition.SqlServer2008;
			}

			// si tratta della versione SQL Server 2012
			if (version.StartsWith("11."))
			{
				// eseguo la IndexOf perche' potrebbe essere Express Edition with Advanced Services
				if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
					return SQLServerEdition.SqlExpress2012;

				return SQLServerEdition.SqlServer2012;
			}

            // si tratta della versione SQL Server 2014 oppure Azure v12
            if (version.StartsWith("12."))
            {
				if (edition.IndexOf("SQL Azure", StringComparison.OrdinalIgnoreCase) >= 0)
					return SQLServerEdition.SqlAzureV12;

				// eseguo la IndexOf perche' potrebbe essere Express Edition with Advanced Services
				if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
                    return SQLServerEdition.SqlExpress2014;

                return SQLServerEdition.SqlServer2014;
            }

			// si tratta della versione SQL Server 2016
			if (version.StartsWith("13."))
			{
				// eseguo la IndexOf perche' potrebbe essere Express Edition with Advanced Services
				if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
					return SQLServerEdition.SqlExpress2016;

				return SQLServerEdition.SqlServer2016;
			}

			return SQLServerEdition.Undefined;
		}

		/// <summary>
		/// torna true se la connessione passata è ad un sqlserver 2012 o successivi
		/// </summary>
		//-----------------------------------------------------------------------
		public static bool IsSql2012Edition(TBConnection tbConnection)
        {
            if (!tbConnection.IsSqlConnection())
				return false;

            return IsSql2012Edition(tbConnection.SqlConnect);
        }

		/// <summary>
		/// torna true se la connessione passata è ad un sqlserver 2012 o successivi
		/// </summary>
		//-----------------------------------------------------------------------
		public static bool IsSql2012Edition(SqlConnection sqlConnection)
        {
            if (sqlConnection == null) return false;

            SQLServerEdition ed = GetSQLServerEdition(sqlConnection);
			return (ed == SQLServerEdition.SqlServer2012 || ed == SQLServerEdition.SqlServer2014 || ed == SQLServerEdition.SqlServer2016);
		}

		/// <summary>
		/// Data una connessione ritorna se si tratta di un server di vecchie edizioni
		/// (MSDE2000 / SqlServer2000 / SqlExpress2005 / SqlServer2005)
		/// </summary>
		/// <returns>true se la versione e' <= 2005</returns>
		//-----------------------------------------------------------------------
		public static bool IsOldestSqlServerVersion(SqlConnection sqlConnection)
		{
			SQLServerEdition edition = GetSQLServerEdition(sqlConnection);

			if (edition == SQLServerEdition.SqlServer2000 || edition == SQLServerEdition.MSDE2000 ||
				edition == SQLServerEdition.SqlServer2005 || edition == SQLServerEdition.SqlExpress2005)
				return true;
			return false;
		}

		/// <summary>
		/// Data una connessione ritorna se si tratta di un server con una versione >= 2012
		/// </summary>
		/// <returns>true se la versione e' >= 2012</returns>
		//-----------------------------------------------------------------------
		public static bool IsSqlServerVersionStartingFrom2012(SqlConnection sqlConnection)
		{
			SQLServerEdition edition = GetSQLServerEdition(sqlConnection);

			if (edition == SQLServerEdition.SqlServer2000 || edition == SQLServerEdition.MSDE2000 ||
				edition == SQLServerEdition.SqlServer2005 || edition == SQLServerEdition.SqlExpress2005 ||
				edition == SQLServerEdition.SqlServer2008 || edition == SQLServerEdition.SqlExpress2008)
				return false;

			return true;
		}
		#endregion

		#region GetDatabaseCollation (SQL Server)
		/// <summary>
		/// GetDatabaseCollation
		/// Esegue le query per ottenere l'impostazione corrente della proprietà Collation del database specificato
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>COLLATION di database</returns>
		//-----------------------------------------------------------------------
		public static string GetDatabaseCollation(TBConnection tbConn)
		{
			string dbCollate = string.Empty;

            //Per adesso qui non metto niente
            if (tbConn.IsPostgreConnection())
                return dbCollate;

			// SQL Server: esegue la query DATABASEPROPERTYEX
			if (tbConn.IsSqlConnection())
			{
				string query =
					string.Format("SELECT DATABASEPROPERTYEX (N'{0}', N'Collation') AS DBCOLLATE", tbConn.Database);

				try
				{
					using (SqlCommand command = new SqlCommand(query, tbConn.SqlConnect))
					using (SqlDataReader reader = command.ExecuteReader())
						if (reader.Read())
							dbCollate = reader["DBCOLLATE"].ToString();
				}
				catch (SqlException)
				{
				}
			}

			return dbCollate;
		}
		#endregion

		#region GetServerCollation (SQL Server)
		/// <summary>
		/// GetServerCollation
		/// Esegue la query che restituisce informazioni relative alla proprietà Collation dell'istanza del server.
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>COLLATION associata al server</returns>
		//-----------------------------------------------------------------------
		public static string GetServerCollation(TBConnection tbConn)
		{
			string serverCollate = string.Empty;

            if (tbConn.IsPostgreConnection())
                return serverCollate;

			// SQL Server: esegue la query SELECT SERVERPROPERTYEX
			if (tbConn.IsSqlConnection())
			{
				try
				{
					using (SqlCommand command = new SqlCommand("SELECT SERVERPROPERTY (N'Collation') AS SERVERCOLLATE", tbConn.SqlConnect))
					using (SqlDataReader reader = command.ExecuteReader())
						if (reader.Read())
							serverCollate = reader["SERVERCOLLATE"].ToString();
				}
				catch (SqlException)
				{
				}
			}

			return serverCollate;
		}
		#endregion

		#region GetColumnCollation (SQL Server)
		/// <summary>
		/// GetColumnCollation
		/// Esegue una query sulle view di sistema di SQL per restituire la Collation imputata alla
		/// colonna Status della tabella TB_DBMark
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>COLLATION di colonna</returns>
		//-----------------------------------------------------------------------
		public static string GetColumnCollation(TBConnection tbConn)
		{
			return GetColumnCollation(tbConn, "TB_DBMark", "Status");
		}

		/// <summary>
		/// GetColumnCollation
		/// Esegue una query sulle view di sistema di SQL per restituire la Collation imputata alla
		/// colonna passata come parametro
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <param name="tableName">nome della tabella a cui appartiene la colonna di cui si vuole sapere la collation</param>
		/// <param name="columnName">nome della colonna di cui si vuole sapere la collation</param>
		/// <returns>COLLATION di colonna</returns>
		//-----------------------------------------------------------------------
		public static string GetColumnCollation(TBConnection tbConn, string tableName, string columnName)
		{
			string columnCollate = string.Empty;

            if (tbConn.IsPostgreConnection())
                return columnCollate;

			// prima controllo l'esistenza della tabella passata come parametro
			TBDatabaseSchema schema = new TBDatabaseSchema(tbConn);
			if (!schema.ExistTable(tableName))
				return columnCollate;

			// SQLServer: legge dalla view di sistema INFORMATION_SCHEMA.COLUMNS
			if (tbConn.IsSqlConnection())
			{
				string query = string.Format
					(
						@"SELECT COLLATION_NAME FROM [{0}].INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = N'{1}' AND COLUMN_NAME = N'{2}'",
						tbConn.Database,
						tableName,
						columnName
					);

				try
				{
					using (SqlCommand command = new SqlCommand(query, tbConn.SqlConnect))
					using (SqlDataReader reader = command.ExecuteReader())
						if (reader.Read())
							columnCollate = reader["COLLATION_NAME"].ToString();
				}
				catch (SqlException e)
				{
					throw (e);
				}
			}

			return columnCollate;
		}
		#endregion

		#region GetValidCollationPropertyForDB (SQL Server)
		/// <summary>
		/// GetValidCollationPropertyForDB
		/// Metodo che in scaletta cerca di ritornare un valido valore di COLLATION:
		/// 1. legge la Collation della colonna AddOnModule della tabella TB_DBMark (se esiste)
		/// 2. legge quella del database, se vuota...
		/// 3. legge quella del server.
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>COLLATION valida</returns>
		//-----------------------------------------------------------------------
		public static string GetValidCollationPropertyForDB(TBConnection tbConn)
		{
			string collation = string.Empty;

            if (tbConn.IsPostgreConnection())
                return collation;

			collation = GetColumnCollation(tbConn);

			if (collation.Length <= 0)
				collation = GetDatabaseCollation(tbConn);

			if (collation.Length <= 0)
				collation = GetServerCollation(tbConn);

			return collation;
		}
		#endregion

		#region GetSQLServerIsInMixedMode (SQL Server)
		/// <summary>
		/// GetSQLServerIsInMixedMode
		/// Esegue la query che restituisce informazioni relative alla proprietà IsIntegratedSecurityOnly dell'istanza del server.
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>true: server in Mixed Mode; false: server solo in Win Auth</returns>
		//-----------------------------------------------------------------------
		public static bool GetSQLServerIsInMixedMode(TBConnection tbConn)
		{
			bool isMixedMode = false;

			if (tbConn.IsPostgreConnection())
				return isMixedMode;

			if (tbConn.IsSqlConnection() && tbConn.State == ConnectionState.Open)
			{
				try
				{
					using (SqlCommand command = new SqlCommand("SELECT SERVERPROPERTY(N'IsIntegratedSecurityOnly') AS IsIntegratedSecurity", tbConn.SqlConnect))
					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							string isMixed = reader["IsIntegratedSecurity"].ToString();
							isMixedMode = string.Compare(isMixed, "0", StringComparison.OrdinalIgnoreCase) == 0;
						}
					}
				}
				catch (SqlException)
				{
				}
			}

			return isMixedMode;
		}
		#endregion

		#region GetColumnDataType (SQL Server)
		/// <summary>
		/// GetColumnDataType
		/// Esegue una query sulle view di sistema di SQL per restituire il DataType della
		/// colonna Status della tabella TB_DBMark
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>datatype colonna</returns>
		//-----------------------------------------------------------------------
		public static string GetColumnDataType(TBConnection tbConn)
		{
			return GetColumnDataType(tbConn, "TB_DBMark", "Status");
		}

		/// <summary>
		/// GetColumnDataType
		/// Esegue una query sulle view di sistema di SQL per restituire il DataType della
		/// colonna passata come parametro
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <param name="tableName">nome della tabella a cui appartiene la colonna</param>
		/// <param name="columnName">nome della colonna di cui si vuole sapere il datatype</param>
		/// <returns>datatype colonna</returns>
		//-----------------------------------------------------------------------
		public static string GetColumnDataType(TBConnection tbConn, string tableName, string columnName)
		{
			string dataType = string.Empty;

            if (tbConn.IsPostgreConnection())
                return dataType;

			// prima controllo l'esistenza della tabella passata come parametro
			TBDatabaseSchema schema = new TBDatabaseSchema(tbConn);
			if (!schema.ExistTable(tableName))
				return dataType;

			// SQLServer: legge dalla view di sistema INFORMATION_SCHEMA.COLUMNS
			if (tbConn.IsSqlConnection())
			{
				string query = string.Format
					(
						@"SELECT DATA_TYPE FROM [{0}].INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = N'{1}' AND COLUMN_NAME = N'{2}'",
						tbConn.Database,
						tableName,
						columnName
					);

				try
				{
					using (SqlCommand command = new SqlCommand(query, tbConn.SqlConnect))
					using (SqlDataReader reader = command.ExecuteReader())
						if (reader.Read())
							dataType = reader["DATA_TYPE"].ToString();
				}
				catch (SqlException e)
				{
					throw (e);
				}
			}

			return dataType;
		}
		#endregion

		#region IsUnicodeDataType (SQL Server)
		/// <summary>
		/// Esegue una query sulle view di sistema di SQL per restituire il DataType della
		/// colonna Status della tabella TB_DBMark
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>true se la colonna e' unicode</returns>
		//-----------------------------------------------------------------------
		public static bool IsUnicodeDataType(TBConnection tbConn)
		{
			string dataType = GetColumnDataType(tbConn);

			if (dataType.EndsWith("char", StringComparison.InvariantCultureIgnoreCase))
				return dataType.StartsWith("n", StringComparison.InvariantCultureIgnoreCase);

			return false;
		}
		#endregion

		#region IsSupportedLanguageForFullTextSearch
		/// <summary>
		/// IsSupportedLanguageForFullTextSearch - controlla che l'lcid passato come parametro sia presente
		/// nella tabella delle languages previste per il Full Text Search
		/// </summary>
		/// <param name="connection">connessione aperta sul db</param>
		/// <param name="lcid">lcid del language da ricercare</param>
		/// <returns>enum DatabaseCheckError</returns>
		//-----------------------------------------------------------------------
		public static bool IsSupportedLanguageForFullTextSearch(TBConnection connection, int lcid)
		{
			if (!connection.IsSqlConnection() || (lcid <= 0))
				return false;

			string selectQuery = string.Format("SELECT COUNT(*) FROM sys.fulltext_languages WHERE lcid = '{0}'", lcid.ToString());

			try
			{
				using (TBCommand command = new TBCommand(selectQuery, connection))
					if (command.ExecuteTBScalar() <= 0)
						return false;
			}
			catch (TBException)
			{
				return false;
			}

			return true;
		}
		#endregion

		#region GetDBPercentageUsedSize, GetDBSizeInKByte, IsDBSizeOverMaxLimit e overload
		/// <summary>
		/// GetDBPercentageUsedSize
		/// Ritorna la percentuale di spazio utilizzato di un database con il limite dei 2GB
		/// (da usare per le edizione Standard e Pro-Lite, con la DBNetworkType = Small)
		/// </summary>
		/// <param name="connectionStr">stringa di connessione al db</param>
		/// <param name="dbmsType">DBMSType</param>
		/// <returns>percentuale di spazio utilizzato</returns>
		//----------------------------------------------------------------------
		public static float GetDBPercentageUsedSize(string connectionStr, DBMSType dbmsType)
		{
			long maxDBSize = GetDBSizeInKByte(connectionStr, dbmsType);

			return ((maxDBSize * 100) / DatabaseLayerConsts.MaxDBSize);
		}

		/// <summary>
		/// IsDBSizeOverMaxLimit
		/// Data una stringa si connette al volo al database ed effettua la somma delle dimensioni
		/// dei file di dati del database. Il valore di ritorno indica se tale somma eccede o meno i 2GB
		/// </summary>
		/// <param name="connectionStr">stringa di connessione al db</param>
		/// <param name="dbmsType">dbmstype per la connessione</param>
		/// <returns>true: se la somma dei file di dati del db eccede i 2GB</returns>
		//----------------------------------------------------------------------
		public static bool IsDBSizeOverMaxLimit(string connectionStr, DBMSType dbmsType)
		{
			long maxDBSize = GetDBSizeInKByte(connectionStr, dbmsType);

			if (maxDBSize > DatabaseLayerConsts.MaxDBSize || maxDBSize < 0)
				return true;

			return false;
		}

		/// <summary>
		/// IsDBSizeOverMaxLimit
		/// Data una stringa si connette al volo al database ed effettua la somma delle dimensioni
		/// dei file di dati del database. Il valore di ritorno indica se tale somma eccede o meno i 2GB
		/// </summary>
		/// <param name="tbConnection">connessione aperta sul db</param>
		/// <returns>true: se la somma dei file di dati del db eccede i 2GB</returns>
		//----------------------------------------------------------------------
		public static bool IsDBSizeOverMaxLimit(TBConnection tbConnection)
		{
			long maxDBSize = GetDBSizeInKByte(tbConnection);

			if (maxDBSize > DatabaseLayerConsts.MaxDBSize || maxDBSize < 0)
				return true;

			return false;
		}

		/// <summary>
		/// Verifica se la dimensione del db è vicina al massimo indicato nel serverconnection.config
		/// se la size è compresa (estremi inclusi) tra la 'limit' (es: 1.95GB) e la max (2GB),  o se è minore di 0, per me è warning
		/// </summary>
		/// <param name="tbConnection"></param>
		/// <returns></returns>
		//----------------------------------------------------------------------
		public static bool IsDBSizeNearMaxLimit(TBConnection tbConnection, out string freePercentage)
		{
			long DBSize = GetDBSizeInKByte(tbConnection);
			double freePercentageD = Math.Round((((double)(DatabaseLayerConsts.MaxDBSize - DBSize) * 100) / DatabaseLayerConsts.MaxDBSize), 1);

			freePercentage = freePercentageD.ToString();
			return ((DBSize >= InstallationData.ServerConnectionInfo.MinDBSizeToWarn && DBSize <= DatabaseLayerConsts.MaxDBSize) || DBSize < 0);
		}

		/// <summary>
		/// GetDBSizeInKByte
		/// Data una stringa si connette al volo al database ed effettua la somma delle dimensioni
		/// dei file di dati del database. Il valore di ritorno ritorna tale somma.
		/// </summary>
		/// <param name="connectionStr">stringa di connessione per il db</param>
		/// <param name="dbmsType">dbmstype per la connessione</param>
		/// <returns>long size</returns>
		//----------------------------------------------------------------------
		public static long GetDBSizeInKByte(string connectionStr, DBMSType dbmsType)
		{
			long fullSize = 0;

            // non effettuo controlli sulla dimensione di un db di tipo Postgre (non ha limite)
            if (dbmsType == DBMSType.POSTGRE)
                return fullSize;

			try
			{
				using (TBConnection myConnection = new TBConnection(connectionStr, dbmsType))
				{
					myConnection.Open();
					fullSize = GetDBSizeInKByte(myConnection);
				}
			}
			catch
			{
				return 0;
			}

			return fullSize;
		}

		/// <summary>
		/// GetDBSizeInKByte
		/// Data una connessione aperta al database, effettua la somma delle dimensioni
		/// dei file di dati del database. Il valore di ritorno ritorna tale somma.
		/// </summary>
		/// <param name="connection">connessione aperta sul db</param>
		/// <returns>long size</returns>
		//----------------------------------------------------------------------
		public static long GetDBSizeInKByte(TBConnection connection)
		{
			long fullSize = 0;

            // non effettuo controlli sulla dimensione di un db di Postgre (illimitato)
            if (connection.IsPostgreConnection())
                return fullSize;

			try
			{
				using (TBCommand myCommand = new TBCommand(connection))
				{
					myCommand.CommandText = "sp_helpfile";
					myCommand.CommandType = CommandType.StoredProcedure;
					using (IDataReader reader = myCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							object filegroup = reader["filegroup"];
							if (filegroup == DBNull.Value)
								continue;

							string size = ((string)reader["size"]).Trim().Replace("KB", "");

							if (string.Compare(size, "Unlimited", StringComparison.OrdinalIgnoreCase) == 0)
								return -1;

							fullSize += Int32.Parse(size);
						}
					}
				}
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				return -1;
			}

			return fullSize;
		}
		#endregion

		#region IsFullTextSearchInstalled (SQL Server only)
		/// <summary>
		/// IsFullTextSearchInstalled
		/// Esegue la query per sapere se il componente per la FullTextSearch e' installato sul server SQL
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>true se e' installato, altrimenti false</returns>
		//-----------------------------------------------------------------------
		public static bool IsFullTextSearchInstalled(TBConnection tbConn)
		{
			bool isInstalled = false;

			// se non si tratta di una connessione a SQL non procedo
			if (!tbConn.IsSqlConnection())
				return isInstalled;

			try
			{
				using (SqlCommand command = new SqlCommand("SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') as IsFullTextInstalled", tbConn.SqlConnect))
					using (SqlDataReader reader = command.ExecuteReader())
						if (reader.Read())
							isInstalled = (string.Compare(reader["IsFullTextInstalled"].ToString(), "1", StringComparison.OrdinalIgnoreCase) == 0);
			}
			catch (SqlException)
			{
			}

			return isInstalled;
		}
		#endregion

		#region IsFullTextSearchEnabled (SQL Server only)
		/// <summary>
		/// IsFullTextSearchEnabled
		/// Esegue la query per sapere se il componente per la FullTextSearch e' abilitato sul database della connessione
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>true se e' abilitato, altrimenti false</returns>
		//-----------------------------------------------------------------------
		public static bool IsFullTextSearchEnabled(TBConnection tbConn)
		{
			bool isEnabled = false;

			// se non si tratta di una connessione a SQL non procedo
			if (!tbConn.IsSqlConnection())
				return isEnabled;

			SQLServerEdition currEdition = GetSQLServerEdition(tbConn.SqlConnect);
			
			// Per le versioni di SQL Server uguali o superiori al 2008 vale il link seguente e la nota :
			// http://msdn.microsoft.com/it-it/library/ms186823.aspx : Il valore di questa proprietà non ha alcun effetto.
			// I database utente sono sempre abilitati per la ricerca full-text. 
			// Quindi se l'edizione di SQL e' diversa dal 2005 ritorno true.
			if (currEdition != SQLServerEdition.SqlExpress2005 && currEdition != SQLServerEdition.SqlServer2005)
				return true;

			try
			{
				string query = string.Format("SELECT DATABASEPROPERTYEX('{0}', 'IsFullTextEnabled') as IsFullTextEnabled", tbConn.Database);

				using (SqlCommand command = new SqlCommand(query, tbConn.SqlConnect))
					using (SqlDataReader reader = command.ExecuteReader())
						if (reader.Read())
							isEnabled = (string.Compare(reader["IsFullTextEnabled"].ToString(), "1", StringComparison.OrdinalIgnoreCase) == 0);
			}
			catch (SqlException)
			{
			}

			return isEnabled;
		}
		#endregion

		#region CheckFullTextSearchEnabled (SQL Server only)
		/// <summary>
		/// CheckFullTextSearchEnabled
		/// Esegue la query per sapere se il componente per la FullTextSearch e' abilitato sul database della connessione
		/// Nel caso fosse disabilitato prova ad abilitarlo
		/// </summary>
		/// <param name="tbConn">connessione aperta sul db</param>
		/// <returns>true se e' abilitato, altrimenti false</returns>
		//-----------------------------------------------------------------------
		public static bool CheckFullTextSearchEnabled(TBConnection tbConn)
		{
			bool isEnabled = false;

			// se non si tratta di una connessione a SQL non procedo
			if (!tbConn.IsSqlConnection())
				return isEnabled;

			// se il componente risulta abilitato ritorno subito true
			if (IsFullTextSearchEnabled(tbConn))
				return true;

			try
			{
				// provo ad abilitare il fulltext
				using (SqlCommand command = new SqlCommand("EXEC sp_fulltext_database 'enable'", tbConn.SqlConnect))
					command.ExecuteNonQuery();

				// ri-controllo di essere riuscita effettivamente ad abilitarlo
				isEnabled = IsFullTextSearchEnabled(tbConn);
			}
			catch (SqlException)
			{
			}

			return isEnabled;
		}
		#endregion

		#region CheckDatabaseVersion
		/// <summary>
		/// CheckDatabaseVersion (controlla se il database a cui siamo connessi è compatibile con la versione
		/// di database prevista dall'installazione
		/// </summary>
		/// <param name="conn">connessione aperta sul db</param>
		/// <param name="activationDB">tipo db previsto dall'installazione</param>
		/// <returns>true: versione db valida, altrimenti false</returns>
		//----------------------------------------------------------------------
		public static DatabaseCheckError CheckDatabaseVersion(TBConnection conn, DBNetworkType dbNetworkType)
		{
			if (conn == null)
				return DatabaseCheckError.NoDatabase;

			string currentDB = string.Empty;

			try
			{
				currentDB = GetDatabaseVersion(conn).ToString();
			}
			catch
			{
				return DatabaseCheckError.NoDatabase;
			}

			if (dbNetworkType == DBNetworkType.Large)
				return DatabaseCheckError.NoError;

			//@@TODOMICHI InstallationData.CheckDBSize non esiste piu'!
			//if (InstallationData.CheckDBSize && IsDBSizeOverMaxLimit(conn))
				//return DatabaseCheckError.DBSizeError;

			return DatabaseCheckError.NoError;
		}
		#endregion

		#region CheckDBRelease
		/// <summary>
		/// CheckDBRelease - controlla la corrispondenza tra i numeri di release presenti nella TB_DBMark
		/// e numeri di release specificati nei DatabaseObjects.xml
		/// </summary>
		/// <param name="conn">connessione aperta sul db</param>
		/// <param name="basePathFinder">pathfinder</param>
		/// <returns>enum DatabaseCheckError</returns>
		//-----------------------------------------------------------------------
		private static DatabaseCheckError CheckDBRelease(TBConnection conn, IBasePathFinder basePathFinder)
		{
			StringCollection applicationsList = new StringCollection();
			// array di supporto per avere l'elenco totale delle AddOnApplications
			// (finchè non cambia il pathfinder e vengono unificati gli ApplicationType)
			StringCollection supportList = new StringCollection();
			// prima guardo le AddOn di TaskBuilder
			basePathFinder.GetApplicationsList(ApplicationType.TaskBuilder, out supportList);
			applicationsList = supportList;
			// poi guardo le AddOn di TaskBuilderApplications
			basePathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);
			for (int i = 0; i < supportList.Count; i++)
				applicationsList.Add(supportList[i]);
			// poi guardo le customizzazioni di EasyStudio
			basePathFinder.GetApplicationsList(ApplicationType.Customization, out supportList);
			for (int i = 0; i < supportList.Count; i++)
				applicationsList.Add(supportList[i]);

			string query =
				"SELECT DBRelease, Status FROM TB_DBMark WHERE Application = @application AND AddOnModule = @module";

			try
			{
				using (TBCommand command = new TBCommand(query, conn))
				{
					command.Parameters.Add("@application", ((TBType)SqlDbType.NVarChar), 20);
					command.Parameters.Add("@module", ((TBType)SqlDbType.NVarChar), 40);
					command.Prepare();

					BaseApplicationInfo appInfo = null;
					bool status = false;

					foreach (string appName in applicationsList)
					{
						appInfo = (BaseApplicationInfo)basePathFinder.GetApplicationInfoByName(appName);
						if (appInfo.Modules == null)
							continue;

						foreach (BaseModuleInfo modInfo in appInfo.Modules)
						{
							// se la signature e' vuota oppure il modulo e' per il DMS skippo
							if (string.IsNullOrWhiteSpace(modInfo.DatabaseObjectsInfo.Signature) || modInfo.DatabaseObjectsInfo.Dms)
								continue;

							status = false;

							((IDbDataParameter)command.Parameters["@application"]).Value = appInfo.ApplicationConfigInfo.DbSignature;
							((IDbDataParameter)command.Parameters["@module"]).Value = modInfo.DatabaseObjectsInfo.Signature;
							using (IDataReader reader = command.ExecuteReader())
							{
								if (!conn.DataReaderHasRows(reader))
								{
									reader.Close();
									return DatabaseCheckError.InvalidModule;
								}

								if (reader.Read())
								{
									if (conn.IsSqlConnection())
									{
										modInfo.CurrentDBRelease = Convert.ToInt32(reader["DBRelease"]);
										status = Convert.ToBoolean(int.Parse(reader["Status"].ToString()));
									}
								}
							}

							if (InvalidRelease(modInfo) || !status)
								return DatabaseCheckError.InvalidModule;
						}
					}
				}
			}
			catch (TBException)
			{
				return DatabaseCheckError.NoTables;
			}

			return DatabaseCheckError.NoError;
		}

		//-----------------------------------------------------------------------
		private static bool InvalidRelease(BaseModuleInfo modInfo)
		{
			//se sono una customizzazione, potrei avere il database disallineato ma devo poter entrare comunque
			return (modInfo.CurrentDBRelease == 0 || modInfo.CurrentDBRelease != modInfo.DatabaseObjectsInfo.Release);
		}
		#endregion

		#region CheckDatabase
		//-----------------------------------------------------------------------
		public static DatabaseCheckError CheckDatabase(TBConnection conn, DBNetworkType dbNetworkType, IBasePathFinder basePathFinder, bool isDevelopment)
		{
			if (
				basePathFinder == null ||
				conn == null ||
				conn.State != ConnectionState.Open ||
				dbNetworkType == DBNetworkType.Undefined
				)
				return DatabaseCheckError.NoActivatedDatabase;

			DatabaseCheckError eCheck = CheckDBRelease(conn, basePathFinder);
		
			if (eCheck != DatabaseCheckError.NoError)
				return eCheck;

			return CheckDatabaseVersion(conn, dbNetworkType);
		}
		#endregion
	}
	#endregion

	#region Class TBConnection
	/// <summary>
	/// Classe per la gestione della connessione al database
	/// </summary>
	//============================================================================
	public class TBConnection : IDbConnection
	{
		private IDbConnection dbConnect = null;
		private string schemaOwner = string.Empty;
		private string completedConnectionString = string.Empty;

		#region Properties IDbConnection
		/// <summary>
		/// Gets or sets the string used to open a database.
		/// </summary>
		//---------------------------------------------------------------------
		public string ConnectionString
		{
			get { return dbConnect.ConnectionString; }
			set { dbConnect.ConnectionString = value; }
		}

		/// <summary>
		/// Gets the current state of the connection
		/// </summary>
		//---------------------------------------------------------------------
		public ConnectionState State { get { return dbConnect.State; } }

		//---------------------------------------------------------------------
		public int ConnectionTimeout
		{
			get
			{
				if (dbConnect is SqlConnection)
					return dbConnect.ConnectionTimeout;

				return 0;
			}
		}

		//---------------------------------------------------------------------
		public string Database
		{
			get
			{
				if (dbConnect is SqlConnection)
					return ((SqlConnection)dbConnect).Database;

                if (dbConnect is NpgsqlConnection)
                    return ((NpgsqlConnection)dbConnect).Database;

				return string.Empty;
			}
		}
		#endregion

		#region Properties TBConnection
		//---------------------------------------------------------------------
		public string SchemaOwner { get { return schemaOwner; } }

		/// <summary>
		/// Get of IDbConnection member 
		/// </summary>
		//---------------------------------------------------------------------
		public IDbConnection DbConnect { get { return dbConnect; } }

		/// <summary>
		/// Return cast of IDbConnection member to a SqlConnection
		/// </summary>
		//---------------------------------------------------------------------
		internal SqlConnection SqlConnect
		{
			get
			{
				if (dbConnect is SqlConnection)
					return (SqlConnection)dbConnect;
				throw (new TBException("TBConnection.SqlConnect: " + DatabaseManagerStrings.InvalidCast));
			}
		}

		/// <summary>
		/// Cast of IDbConnection member to a NpgsqlConnection
		/// </summary>
		//---------------------------------------------------------------------
		internal NpgsqlConnection PostgreConnect
        {
            get
            {
                if (dbConnect is NpgsqlConnection)
                    return (NpgsqlConnection)dbConnect;
                throw (new TBException("TBConnection.PostgreConnect: " + DatabaseManagerStrings.InvalidCast));
            }
        }

		/// <summary>
		/// Restituisce il nome del server/nome servizio utilizzato dalla connessione
		/// </summary>
		//---------------------------------------------------------------------
		public string DataSource
		{
			get
			{
				if (dbConnect is SqlConnection)
					return ((SqlConnection)dbConnect).DataSource;

                if (dbConnect is NpgsqlConnection)
                    return ((NpgsqlConnection)dbConnect).DataSource;

				throw (new TBException("TBConnection.DataSource: " + DatabaseManagerStrings.InvalidCast));
			}
		}
		#endregion

		#region Constructors
		//---------------------------------------------------------------------------
		public TBConnection(DBMSType dbmsType)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				dbConnect = new SqlConnection();
		    else if (dbmsType == DBMSType.POSTGRE)
                dbConnect = new NpgsqlConnection();
			else
				throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
		}

		/// <summary>
		/// TBConnection constructor
		/// </summary>
		//---------------------------------------------------------------------------
		public TBConnection(IDbConnection idbConnect)
		{
			dbConnect = idbConnect;
		}

		/// <summary>
		/// costruttore
		/// </summary>
		//---------------------------------------------------------------------------
		public TBConnection(string connectionString, DBMSType dbmsType)
		{
            // La proprietà ConnectionString di IDbConnection "taglia" l'nformazione di pwd
            // (a meno che la stringa usata per connettersi contenga anche "persist security info=True")
            // per sicurezza salvo la stringa completa in un data member privato
            completedConnectionString = connectionString;

            // verifico il tipo di provider controllando le differenze nella stringa di connessione passata
            // SqlServer ha l'attibuto di InitialCatalog oppure Database in caso di Azure
            if (dbmsType == DBMSType.SQLSERVER)
            {
                if (connectionString.IndexOf("Initial Catalog", 0) < 0 && connectionString.IndexOf("Database", 0) < 0)
                    throw (new TBException("Error in connection string"));
                else
                    dbConnect = new SqlConnection(connectionString);
            }
            else if (dbmsType == DBMSType.POSTGRE)
            {
                if (connectionString.IndexOf("Database", 0) < 0)
                    throw (new TBException("Error in connection string"));
                else
                    dbConnect = new NpgsqlConnection(connectionString);
            }
            else
                throw (new TBException("TBConnection constructor: " + string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
        }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="connection"> stringa di connessione</param>
		/// <param name="dbmsType">tipi di dbms da utilizzare per la connessione</param>
		/// <param name="owner">eventuale owner: utilizzato per la gestione dei sinonimi in oralce</param> 
		//---------------------------------------------------------------------------
		public TBConnection(string connectionString, DBMSType dbmsType, string owner)
		{
            // La proprietà ConnectionString di IDbConnection "taglia" l'nformazione di pwd
            // (a meno che la stringa usata per connettersi contenga anche "persist security info=True")
            // per sicurezza salvo la stringa completa in un data member privato
            completedConnectionString = connectionString;

            // verifico il tipo di provider controllando le differenze nella stringa di connessione passata
            // SqlServer ha l'attibuto di InitialCatalog attributo non utilizzato da Oracle
            if (dbmsType == DBMSType.SQLSERVER)
            {
                if (connectionString.IndexOf("Initial Catalog", 0) < 0 && connectionString.IndexOf("Database", 0) < 0)
                    throw (new TBException("Error in connection string"));
                else
                    dbConnect = new SqlConnection(connectionString);
            }
            else if (dbmsType == DBMSType.POSTGRE)
            {
                if (connectionString.IndexOf("Database", 0) < 0)
                    throw (new TBException("Error in connection string"));
                else
                    dbConnect = new NpgsqlConnection(connectionString);
            }
            else
                throw (new TBException("TBConnection constructor: " + string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));

            schemaOwner = owner.ToUpper(CultureInfo.InvariantCulture);
		}
		#endregion

		#region Dispose
		/// <summary>
		/// Releases all resources used by the Component.
		/// </summary>
		//---------------------------------------------------------------------------
		public void Dispose()
		{
			dbConnect.Dispose();
		}
		#endregion

		#region Required methods of IDbConnection
		/// <summary>
		/// Opens a connection to a database with the property settings specified by the ConnectionString.
		/// </summary>
		//---------------------------------------------------------------------------
		public void Open()
		{
			try
			{
				dbConnect.Open();
			}
			catch (SqlException exc)
			{
				if (this.IsSqlConnection())
					TryToAdjustConnectionString(exc);
			}
			catch (Exception ex)
			{
				throw (new TBException(ex.Message, ex.InnerException));
			}
		}

		// solo per Sql Server: prova a riaprire la connessione senza usare il pool (qualora fosse utilizzato)
		//---------------------------------------------------------------------------
		private void TryToAdjustConnectionString(SqlException exc)
		{
			string pattern = @"Pooling\s*=\s*true";
			//se non ero in pooling, allora l'errore e' un altro, rimbalzo l'eccezione originaria
			if (!Regex.IsMatch(dbConnect.ConnectionString, pattern, RegexOptions.IgnoreCase))
				throw (new TBException(exc.Message, exc.InnerException));

			dbConnect.ConnectionString = Regex.Replace(dbConnect.ConnectionString, pattern, "Pooling=false", RegexOptions.IgnoreCase);

			Open();
		}

		/// <summary>
		/// Closes the connection to the database. This is the preferred method of closing any open connection.
		/// </summary>
		//---------------------------------------------------------------------------
		public void Close()
		{
			try
			{
				if (dbConnect.State == ConnectionState.Open || dbConnect.State == ConnectionState.Broken)
					dbConnect.Close();
			}
			catch
			{
				//RESUME NEXT ...
			}
		}

		//---------------------------------------------------------------------------
		public IDbTransaction BeginTransaction()
		{
			return dbConnect.BeginTransaction();
		}

		//---------------------------------------------------------------------------
		public IDbTransaction BeginTransaction(System.Data.IsolationLevel level)
		{
            //if (IsPostgreConnection() && level == IsolationLevel.ReadUncommitted)
            //    return dbConnect.BeginTransaction(IsolationLevel.ReadUncommitted);
            return dbConnect.BeginTransaction(level);
		}

		//---------------------------------------------------------------------------
		public void ChangeDatabase(string dbName)
		{
			dbConnect.ChangeDatabase(dbName);
		}

		//---------------------------------------------------------------------------
		public IDbCommand CreateCommand()
		{
			return dbConnect.CreateCommand();
		}
		#endregion

		#region Methods of TBConnection
		/// <summary>
		/// TRUE if the connection is a SqlConnection
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsSqlConnection()
		{
			return (dbConnect is SqlConnection);
		}

        /// <summary>
        /// TRUE if the connection is a NpgsqlConnection
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsPostgreConnection()
        {
            return (dbConnect is NpgsqlConnection);
        }

		/// <summary>
		/// Permette di sapere se il datareader passato come argomento ha estratto o meno delle righe 
		/// (Nota: il metodo HasRows non è dell'interfaccia IDataReader ma viene implementato solo dalle classi 
		/// da essa derivate)
		/// </summary>
		/// <param name="reader">IDataReader da controllare</param>
		/// <returns>true se sono presenti delle righe false altrimenti</returns>
		//---------------------------------------------------------------------------
		public bool DataReaderHasRows(IDataReader reader)
		{
			if (reader == null || reader.IsClosed)
				return false;

			if (IsSqlConnection())
				return ((SqlDataReader)reader).HasRows;

            if (IsPostgreConnection())
                return ((NpgsqlDataReader)reader).HasRows;

			throw (new TBException("TBConnection.DataReaderHasRows: " + DatabaseManagerStrings.InvalidCast));
		}

		//---------------------------------------------------------------------------
		public TBConnection GetCloneTBConnection()
		{
			// uso la completedConnectionString che a differenza della property ConnectionString ha al suo 
			// interno le informazioni relative alla pwd
			if (string.IsNullOrWhiteSpace(completedConnectionString))
				return null;

			string provider = string.Empty;
			DBMSType dbmsType;
			if (IsSqlConnection())
				dbmsType = DBMSType.SQLSERVER;
            else if (IsPostgreConnection())
                dbmsType = DBMSType.POSTGRE;
			else
				return null;

			return new TBConnection(completedConnectionString, dbmsType);
		}
		#endregion
	}
	#endregion

	#region Class TBCommand
	/// <summary>
	/// Classe per la gestione dei command
	/// </summary>
	//============================================================================
	public class TBCommand : IDbCommand
	{
		private IDbCommand dbCommand = null;
		private TBConnection tbConnection = null;

		#region Properties required for IDbCommand
		//---------------------------------------------------------------------
		public string CommandText { get { return dbCommand.CommandText; } set { dbCommand.CommandText = value; } }

		//---------------------------------------------------------------------
		IDbConnection IDbCommand.Connection { get { return dbCommand.Connection; } set { dbCommand.Connection = value; } }

		//---------------------------------------------------------------------
		public IDbTransaction Transaction { get { return dbCommand.Transaction; } set { dbCommand.Transaction = value; } }

		//---------------------------------------------------------------------
		public int CommandTimeout { get { return dbCommand.CommandTimeout; } set { dbCommand.CommandTimeout = value; } }

		//---------------------------------------------------------------------
		public CommandType CommandType { get { return dbCommand.CommandType; } set { dbCommand.CommandType = value; } }

		//---------------------------------------------------------------------
		IDataParameterCollection IDbCommand.Parameters { get { return dbCommand.Parameters; } }

		//---------------------------------------------------------------------
		public UpdateRowSource UpdatedRowSource { get { return dbCommand.UpdatedRowSource; } set { dbCommand.UpdatedRowSource = value; } }
		#endregion

		#region Properties of TBCommand
		/// <summary>
		/// Get of IDbCommand member 
		/// </summary>
		//---------------------------------------------------------------------
		public IDbCommand DbCommand { get { return dbCommand; } }

		//---------------------------------------------------------------------
		public TBConnection Connection
		{
			get
			{
				return tbConnection;
			}
			set
			{
				dbCommand.Connection = ((TBConnection)value).DbConnect;
				tbConnection = (TBConnection)value;
			}
		}

		//---------------------------------------------------------------------
		public TBParameterCollection Parameters
		{
			get
			{
				if (dbCommand is SqlCommand)
					return new TBParameterCollection(((SqlCommand)dbCommand).Parameters);
                if (dbCommand is NpgsqlCommand)
                    return new TBParameterCollection(((NpgsqlCommand)dbCommand).Parameters);

				throw (new TBException("TBCommand.Parameters" + DatabaseManagerStrings.InvalidCast));
			}
		}

		/// <summary>
		/// Return cast of IDbCommand member to a SqlCommand
		/// </summary>
		//---------------------------------------------------------------------
		internal SqlCommand SqlCmd
		{
			get
			{
				if (dbCommand is SqlCommand)
					return (SqlCommand)dbCommand;
				throw (new TBException("TBCommand.SqlCmd" + DatabaseManagerStrings.InvalidCast));
			}
		}

        /// <summary>
        /// Return cast of IDbCommand member to a NpgsqlCommand
        /// </summary>
        //---------------------------------------------------------------------
        internal NpgsqlCommand NpgsqlCmd
        {
            get
            {
                if (dbCommand is NpgsqlCommand)
                    return (NpgsqlCommand)dbCommand;
                throw (new TBException("TBCommand.NpgsqlCmd" + DatabaseManagerStrings.InvalidCast));
            }
        }
		#endregion

		#region Constructor
		//---------------------------------------------------------------------------
		public TBCommand(TBConnection connection)
		{
			tbConnection = connection;

			if (connection.IsSqlConnection())
			{
				dbCommand = new SqlCommand();
				dbCommand.Connection = connection.SqlConnect;
			}
            else if (connection.IsPostgreConnection())
            {
                dbCommand = new NpgsqlCommand();
                dbCommand.Connection = connection.PostgreConnect;
            }
            else
                throw (new TBException("TBCommand.Constructor : " + DatabaseManagerStrings.UnknownConnection));
		}

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="query">stringa del comando da eseguire</param>
		/// <param name="tbConnection">connessione aperta per eseguire il command</param>
		//---------------------------------------------------------------------------
		public TBCommand(string query, TBConnection connection)
		{
			if (string.IsNullOrWhiteSpace(query))
				throw (new TBException("TBCommand.Constructor: null query parameter"));

			tbConnection = connection;

			if (connection.IsSqlConnection())
				dbCommand = new SqlCommand(query, connection.SqlConnect);
			else if (connection.IsPostgreConnection())
                dbCommand = new NpgsqlCommand(query, connection.PostgreConnect);
			else
				throw (new TBException("TBCommand.Constructor : " + DatabaseManagerStrings.UnknownConnection));
		}

		//------------------------------------------------------------------
		public TBCommand(IDbCommand command)
		{
			dbCommand = command;
		}
		#endregion

		#region Required methods for IDbCommand
		//---------------------------------------------------------------------------
		public void Cancel()
		{
			dbCommand.Cancel();
		}

		//---------------------------------------------------------------------------
		public IDbDataParameter CreateParameter()
		{
			try
			{
				return dbCommand.CreateParameter();
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and returns the number of rows affected
		/// </summary>
		/// <returns>nr. rows affected</returns>
		//---------------------------------------------------------------------------
		public int ExecuteNonQuery()
		{
			try
			{
				return dbCommand.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}

		/// <summary>
		/// Sends the CommandText to the Connection and builds a DataReader.
		/// </summary>
		/// <returns>IDataReader</returns>
		//---------------------------------------------------------------------------
		public IDataReader ExecuteReader()
		{
			try
			{
				return dbCommand.ExecuteReader();
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}

		//---------------------------------------------------------------------------
		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			try
			{
				return dbCommand.ExecuteReader(behavior);
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the 
		/// result set returned by the query. Extra columns or rows are ignored.
		/// </summary>
		/// <returns>generic object</returns>
		//---------------------------------------------------------------------------
		public object ExecuteScalar()
		{
			try
			{
				return dbCommand.ExecuteScalar();
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}

		///<summary>
		/// ExecuteTBScalar
		/// Call ExecuteScalar method and effect a casting to int in conformity with database provider
		///</summary>
		//---------------------------------------------------------------------------
		public int ExecuteTBScalar()
		{
			try
			{
				object rowsAffected = ExecuteScalar();
                return Int32.Parse(rowsAffected.ToString()); // default for SqlCommand
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}

		/// <summary>
		/// Sends the CommandText to the Connection and builds an XmlReader object.
		/// SUPPORTATO SOLO DAL PROVIDER SQLSERVER!!!
		/// </summary>
		/// <returns>XmlReader</returns>
		//---------------------------------------------------------------------------
		public XmlReader ExecuteXmlReader()
		{
            if (dbCommand is NpgsqlCommand)
                throw (new TBException("ExecuteXmlReader not supported for a Postgre connection"));

			try
			{
				return ((SqlCommand)dbCommand).ExecuteXmlReader();
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}
		#endregion

		#region Methods of TBCommand
		//---------------------------------------------------------------------------
		public void Prepare()
		{
			try
			{
				dbCommand.Prepare();
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}

		/// <summary>
		/// dato un parametro IDataParameter lo restituisce come TBParameter. 
		/// </summary>
		/// <param name="param"></param>
		//------------------------------------------------------------------------------------
		public TBParameter GetTBParameter(IDataParameter param)
		{
			if (dbCommand is SqlCommand)
				return (TBParameter)((SqlParameter)param);

            if (dbCommand is NpgsqlCommand)
                return (TBParameter)((NpgsqlParameter)param);

			throw (new TBException("TBCommand.GetTBParameter" + DatabaseManagerStrings.InvalidCast));
		}
		#endregion

		#region Dispose
		/// <summary>
		/// Releases the resources used by the Component.
		/// </summary>
		//---------------------------------------------------------------------------
		public void Dispose()
		{
			dbCommand.Dispose();
		}
		#endregion
	}
	#endregion

	#region Class TBParameter
	/// <summary>
	/// Classe per la gestione dei Parameter
	/// </summary>
	//============================================================================
	public class TBParameter : IDataParameter
	{
		private IDataParameter dbParameter = null;

		#region Required properties for IDataParameter
		//---------------------------------------------------------------------------
		public DbType DbType { get { return dbParameter.DbType; } set { dbParameter.DbType = value; } }

		//---------------------------------------------------------------------------
		public ParameterDirection Direction { get { return dbParameter.Direction; } set { dbParameter.Direction = value; } }

		//---------------------------------------------------------------------------
		public bool IsNullable { get { return dbParameter.IsNullable; } }

		//---------------------------------------------------------------------------
		public string ParameterName { get { return dbParameter.ParameterName; } set { dbParameter.ParameterName = value; } }

		//---------------------------------------------------------------------------
		public string SourceColumn { get { return dbParameter.SourceColumn; } set { dbParameter.SourceColumn = value; } }

		//---------------------------------------------------------------------------
		public DataRowVersion SourceVersion { get { return dbParameter.SourceVersion; } set { dbParameter.SourceVersion = value; } }

		//---------------------------------------------------------------------------
		public object Value { get { return dbParameter.Value; } set { dbParameter.Value = value; } }
		#endregion

		#region Properties of TBParameter
		//---------------------------------------------------------------------------
		public IDataParameter DbParameter { get { return dbParameter; } }

		//---------------------------------------------------------------------------
		internal SqlParameter SqlParam
		{
			get
			{
				if (dbParameter is SqlParameter)
					return (SqlParameter)dbParameter;
				return null;
			}
		}

        //---------------------------------------------------------------------------
        internal NpgsqlParameter PostgreParam
        {
            get
            {
                if (dbParameter is NpgsqlParameter)
                    return (NpgsqlParameter)dbParameter;
                return null;
            }
        }

		//---------------------------------------------------------------------------
		internal SqlDbType SqlDbType
		{
			get
			{
				if (dbParameter is SqlParameter)
					return ((SqlParameter)dbParameter).SqlDbType;
                if (dbParameter is NpgsqlParameter)
                    return TBDatabaseType.GetSqlDbType(((NpgsqlParameter)dbParameter).NpgsqlDbType);

				throw (new TBException("TBParameter.SqlDbType: " + DatabaseManagerStrings.InvalidCast));
			}
			set
			{
				if (dbParameter is SqlParameter)
					((SqlParameter)dbParameter).SqlDbType = value;
                if (dbParameter is NpgsqlParameter)
                    ((NpgsqlParameter)dbParameter).NpgsqlDbType = TBDatabaseType.GetPostgreDbType(value);

				throw (new TBException("TBParameter.SqlDbType: " + DatabaseManagerStrings.InvalidCast));
			}
		}
		#endregion

		#region Constructors
		//---------------------------------------------------------------------------
		public TBParameter(IDataParameter dbParam)
		{
			dbParameter = dbParam;
		}

		//---------------------------------------------------------------------------
		public TBParameter(DBMSType dbmsType)
		{
            if (dbmsType == DBMSType.SQLSERVER)
                dbParameter = new SqlParameter();
            else if (dbmsType == DBMSType.POSTGRE)
                dbParameter = new NpgsqlParameter();
            else
                throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
		}

		//---------------------------------------------------------------------------
		public TBParameter(string parameterName, object paramValue, DBMSType dbmsType)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				dbParameter = new SqlParameter(parameterName, paramValue);
            else if (dbmsType == DBMSType.POSTGRE)
                dbParameter = new NpgsqlParameter(parameterName, paramValue);
			else
				throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
		}

		//---------------------------------------------------------------------------
		public TBParameter(string parameterName, TBType dbType, DBMSType dbmsType)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				dbParameter = new SqlParameter(parameterName, dbType.SqlDbType);
            else if (dbmsType == DBMSType.POSTGRE)
                dbParameter = new NpgsqlParameter(parameterName, TBDatabaseType.GetPostgreDbType(dbType)); //@@ Anastasia da verificare  il dbType
            else
				throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
		}

		//---------------------------------------------------------------------------
		public TBParameter(string parameterName, TBType dbType, int size, DBMSType dbmsType)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				dbParameter = new SqlParameter(parameterName, dbType.SqlDbType, size);
            else if (dbmsType == DBMSType.POSTGRE)
                dbParameter = new NpgsqlParameter(parameterName, TBDatabaseType.GetPostgreDbType(dbType), size);
            else
				throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
		}

		//---------------------------------------------------------------------------
		public TBParameter(string parameterName, TBType dbType, int size, string sourceColumn, DBMSType dbmsType)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				dbParameter = new SqlParameter(parameterName, dbType.SqlDbType, size, sourceColumn);
            else if (dbmsType == DBMSType.POSTGRE)
                dbParameter = new NpgsqlParameter(parameterName, TBDatabaseType.GetPostgreDbType(dbType), size, sourceColumn);
            else
				throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));
		}
		#endregion

		#region OperatorCast
		//------------------------------------------------------------------
		public static implicit operator TBParameter(SqlParameter sqlParam)
		{
			return new TBParameter(sqlParam);
		}

        //------------------------------------------------------------------
        public static implicit operator TBParameter(NpgsqlParameter postgreParam)
        {
            return new TBParameter(postgreParam);
        }
		#endregion

		#region SetParameterValue (usata dall'ImportManager)
		///<summary>
		/// SetParameterValue
		/// Utilizzata nell'ImportManager per settare i parametri per poi effettuare le insert into nelle tabelle
		///</summary>
		//---------------------------------------------------------------------------
		public object SetParameterValue(string value)
		{
			try
			{
				switch (this.SqlDbType)
				{
					case (SqlDbType.BigInt):
						this.Value = XmlConvert.ToInt64(value);
						break;

					case (SqlDbType.Binary):
					case (SqlDbType.Image):
					case (SqlDbType.TinyInt):
					case (SqlDbType.VarBinary):
						this.Value = XmlConvert.ToByte(value);
						break;

					case (SqlDbType.Bit):
						this.Value = XmlConvert.ToBoolean(value);
						break;

					case (SqlDbType.DateTime):
					case (SqlDbType.SmallDateTime):
					case (SqlDbType.Timestamp):
						this.Value = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
						break;

					case (SqlDbType.Decimal):
					case (SqlDbType.Money):
					case (SqlDbType.SmallMoney):
						this.Value = XmlConvert.ToDecimal(value);
						break;

					case (SqlDbType.Float):
						if (this.PostgreParam != null)
							this.DbType = DbType.Double; //???
						this.Value = XmlConvert.ToDouble(value);
						break;

					case (SqlDbType.Int):
						this.Value = XmlConvert.ToInt32(value);
						break;

					case (SqlDbType.SmallInt):
						this.Value = XmlConvert.ToInt16(value);
						break;

					case (SqlDbType.UniqueIdentifier):
						this.Value = XmlConvert.ToGuid(value);
						break;

					case (SqlDbType.Char):
					case (SqlDbType.NChar):
					case (SqlDbType.VarChar):
					case (SqlDbType.NVarChar):
						if (this.PostgreParam != null && this.PostgreParam.NpgsqlDbType == NpgsqlDbType.Uuid)
						{
							this.DbType = DbType.Guid; //???
							this.Value = XmlConvert.ToGuid(value);
							break;
						}
						//@@TODOMICHI: prima era cosi System.Web.HttpUtility.HtmlDecode(value)
						string readValue = System.Net.WebUtility.HtmlDecode(value);
						this.Value = readValue;
						break;

					case (SqlDbType.Text):
					case (SqlDbType.NText):
						//@@TODOMICHI: prima era cosi System.Web.HttpUtility.HtmlDecode(value)
						this.Value = System.Net.WebUtility.HtmlDecode(value);
						break;

					default:
						this.Value = value;
						break;
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				Debug.WriteLine(e.ToString());
			}

			return value;
		}
		#endregion
	}
	#endregion

	#region Class TBParameterCollection
	//==============================================================================
	public class TBParameterCollection : IDataParameterCollection
	{
		private IDataParameterCollection dbParameterCollection = null;
		private DBMSType dbmsType = DBMSType.UNKNOWN;

		//---------------------------------------------------------------------------
		public TBParameterCollection(IDataParameterCollection dbParams)
		{
			if (dbParams is SqlParameterCollection)
				dbmsType = DBMSType.SQLSERVER;
            else if (dbParams is NpgsqlParameterCollection)
                dbmsType = DBMSType.POSTGRE;  
			else	
                throw (new TBException(string.Format(DatabaseManagerStrings.UnknownDBMS, dbmsType.ToString())));

			dbParameterCollection = dbParams;
		}

		#region Properties of IDataParameterCollection
		//---------------------------------------------------------------------------
		public object this[string parameterName]
		{
			get
			{
				return dbParameterCollection[TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType)];
			}
			set
			{
				dbParameterCollection[TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType)] = value;
			}
		}
		#endregion

		#region Properties of ICollection
		//------------------------------------------------------------------
		public int Count { get { return dbParameterCollection.Count; } }
		//------------------------------------------------------------------
		public bool IsSynchronized { get { return dbParameterCollection.IsSynchronized; } }
		//------------------------------------------------------------------
		public object SyncRoot { get { return dbParameterCollection.SyncRoot; } }
		//---------------------------------------------------------------------------
		public object this[int index] { get { return dbParameterCollection[index]; } set { dbParameterCollection[index] = value; } }
		#endregion

		#region Properties of IList
		//------------------------------------------------------------------
		public bool IsFixedSize { get { return dbParameterCollection.IsFixedSize; } }
		//------------------------------------------------------------------
		public bool IsReadOnly { get { return dbParameterCollection.IsReadOnly; } }
		#endregion

		#region Properties of TBParameterCollection
		//------------------------------------------------------------------
		internal SqlParameterCollection SqlParameterCollection
		{
			get
			{
				if (dbmsType == DBMSType.SQLSERVER)
					return (SqlParameterCollection)dbParameterCollection;
				throw (new TBException("TBParameterCollection.SqlParameterCollection: " + DatabaseManagerStrings.InvalidCast));
			}
		}

        //------------------------------------------------------------------
        internal NpgsqlParameterCollection PostgreParameterCollection
        {
            get
            {
                if (dbmsType == DBMSType.POSTGRE)
                    return (NpgsqlParameterCollection)dbParameterCollection;
                throw (new TBException("TBParameterCollection.PostgreParameterCollection: " + DatabaseManagerStrings.InvalidCast));
            }
        }
		#endregion

		#region Methods required by IDataParameterCollectin
		//---------------------------------------------------------------------------
		public bool Contains(string parameterName)
		{
			return dbParameterCollection.Contains(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType));
		}

		//---------------------------------------------------------------------------
		public int IndexOf(string parameterName)
		{
			return dbParameterCollection.IndexOf(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType));
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(string parameterName)
		{
			dbParameterCollection.RemoveAt(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType));
		}

		//---------------------------------------------------------------------------
		public int Add(TBParameter value)
		{
			if (value.ParameterName != null)
				return dbParameterCollection.Add(value.DbParameter);
			else
				throw new TBException(DatabaseManagerStrings.NamedParameter);
		}

		//---------------------------------------------------------------------------
		public int Add(string parameterName, object value)
		{
			return Add(new TBParameter(parameterName, value, dbmsType));
		}

		//---------------------------------------------------------------------------
		public int Add(string parameterName, TBType dbType)
		{
			return Add(new TBParameter(parameterName, dbType, dbmsType));
		}

		//---------------------------------------------------------------------------
		public int Add(string parameterName, TBType dbType, int size)
		{
			return Add(new TBParameter(parameterName, dbType, size, dbmsType));
		}

		//---------------------------------------------------------------------------
		public int Add(string parameterName, TBType dbType, int size, string sourceColumn)
		{
			return Add(new TBParameter(parameterName, dbType, size, sourceColumn, dbmsType));
		}
		#endregion

		#region Methods required by ICollection
		//------------------------------------------------------------------
		public void CopyTo(Array array, int index)
		{
			dbParameterCollection.CopyTo(array, index);
		}
		#endregion

		#region Methods required by IEnumerable
		//------------------------------------------------------------------
		public IEnumerator GetEnumerator()
		{
			return dbParameterCollection.GetEnumerator();
		}
		#endregion

		#region Methods required by IList
		//------------------------------------------------------------------
		public void Clear()
		{
			dbParameterCollection.Clear();
		}

		//------------------------------------------------------------------
		public bool Contains(object value)
		{
			return dbParameterCollection.Contains(value);
		}

		//------------------------------------------------------------------
		public int IndexOf(object value)
		{
			return dbParameterCollection.IndexOf(value);
		}

		//------------------------------------------------------------------
		public void Insert(int index, object value)
		{
			dbParameterCollection.Insert(index, value);
		}

		//------------------------------------------------------------------
		public void Remove(object value)
		{
			dbParameterCollection.Remove(value);
		}

		//------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			dbParameterCollection.RemoveAt(index);
		}

		//------------------------------------------------------------------
		public int Add(object value)
		{
			return Add((TBParameter)value);
		}
		#endregion

		#region Methods of TBParameterCollection
		//------------------------------------------------------------------
		public TBParameter GetParameterAt(int index)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				return (TBParameter)SqlParameterCollection[index];
            if (dbmsType == DBMSType.POSTGRE)
                return (TBParameter)PostgreParameterCollection[index];

			throw (new TBException("TBParameterCollection.GetParameterAt: " + DatabaseManagerStrings.InvalidCast));
		}

		//------------------------------------------------------------------
		public TBParameter GetParameterAt(string parameterName)
		{
			if (dbmsType == DBMSType.SQLSERVER)
				return (TBParameter)SqlParameterCollection[parameterName];
            if (dbmsType == DBMSType.POSTGRE)
                return (TBParameter)PostgreParameterCollection[parameterName];

			throw (new TBException("TBParameterCollection.GetParameterAt: " + DatabaseManagerStrings.InvalidCast));
		}
		#endregion
	}
	#endregion
    
	//============================================================================
	public class TBDatabaseSchema
	{
		private TBConnection tbConnection = null;

		//-----------------------------------------------------------------------
		public TBDatabaseSchema(TBConnection tbConn)
		{
			tbConnection = tbConn;
		}

		#region Gestione singola tabella
		///<summary>
		/// Generico metodo ExistTable per verificare l'esistenza di un oggetto su database
		/// (sia esso tabella, vista o stored procedure)
		/// A seconda del tipo di connessione vengono interrogate direttamente 
		/// le tabelle di sistema del specifico server di database.
		///</summary>
		//-----------------------------------------------------------------------
		public bool ExistTable(string tableName)
		{
			try
			{
				if (tbConnection.IsSqlConnection())
					return ExistSqlTable(tableName);
                else if (tbConnection.IsPostgreConnection())
                    return ExistPostgreTable(tableName);
				else
					return false;
			}
			catch (TBException)
			{
				throw;
			}
		}

		/// <summary>
		/// SQL SERVER
		/// verificare l'esistenza di un oggetto su un database
		/// </summary>	
		//-----------------------------------------------------------------------
		private bool ExistSqlTable(string tableName)
		{
			try
			{
				using (SqlCommand sqlCommand = new SqlCommand
					(
					string.Format("SELECT COUNT(*) FROM sysobjects WHERE id = object_id(N'{0}')", tableName),
					tbConnection.SqlConnect
					))
					return (int)sqlCommand.ExecuteScalar() > 0;
			}
			catch (SqlException e)
			{
				throw (new TBException(e.Message, e));
			}
		}

        /// <summary>
        /// Postgre
        /// verificare l'esistenza di un oggetto su un database
        /// </summary>	
        //-----------------------------------------------------------------------
        private bool ExistPostgreTable(string tableName)
        {   
            try
            {
				using (NpgsqlCommand postgreCommand = new NpgsqlCommand
					(
					string.Format("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{0}' AND table_name='{1}' ",
					DatabaseLayerConsts.postgreDefaultSchema, tableName.ToLower()),
					tbConnection.PostgreConnect
					))
					return Int32.Parse(postgreCommand.ExecuteScalar().ToString()) > 0;
            }
            catch (NpgsqlException e)
            {
                throw (new TBException(e.Message, e));
            }
        }

		///<summary>
		/// GetTableSchema
		/// Per caricare le informazioni di schema di un oggetto di database
		/// A seconda del tipo di connessione vengono interrogate direttamente 
		/// le tabelle di sistema del specifico server di database.
		///</summary>
		//-----------------------------------------------------------------------
		public TBTable GetTableSchema(string tableName, bool withKeyInfo)
		{
			TBTable tbTable = new TBTable(tableName);

			string query = string.Empty;

			try
			{
				if (tbConnection.IsSqlConnection())
					query = @"select COLUMN_NAME as name, IS_NULLABLE as isNullable, 
							NUMERIC_PRECISION as numericPrecision, NUMERIC_SCALE as numericScale, 
							CHARACTER_MAXIMUM_LENGTH as columnSize, DATA_TYPE as dataType, 
							COLLATION_NAME as collationName, COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') as isAutoIncrement,
							COLUMN_DEFAULT as columnDefault
							from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{0}'";
				/*else if (tbConnection.IsPostgreConnection()) //@@TODO trovare una nuova query
				{
					query = "select * from {0} LIMIT 1";
					tableName = tableName.ToLower();
				}*/

				using (TBCommand tbCommand = new TBCommand(string.Format(query, tableName), tbConnection))
				using (IDataReader reader = tbCommand.ExecuteReader())
					while (reader.Read())
					{
						TBColumn tbColumn = new TBColumn(reader["name"].ToString());
						tbColumn.IsNullable			= (reader["isNullable"] != DBNull.Value && string.Compare(reader["isNullable"].ToString(), "YES", StringComparison.OrdinalIgnoreCase) == 0); //se sql e' in italiano YES diventa SI?
						tbColumn.NumericPrecision	= (reader["numericPrecision"] != DBNull.Value) ? Convert.ToInt32(reader["numericPrecision"]) : -1;
						tbColumn.NumericScale		= (reader["numericScale"] != DBNull.Value) ? (int)reader["numericScale"] : -1;
						tbColumn.ColumnSize			= (reader["columnSize"] != DBNull.Value) ? (int)reader["columnSize"] : 0;
						tbColumn.DataType			= reader["dataType"].ToString();
						tbColumn.CollationName		= (reader["collationName"] != DBNull.Value) ? reader["collationName"].ToString() : string.Empty;
						tbColumn.IsAutoIncrement	= (reader["isAutoIncrement"] != DBNull.Value && string.Compare(reader["isAutoIncrement"].ToString(), "1") == 0);
						tbColumn.HasDefault			= (reader["columnDefault"] != DBNull.Value);
						tbColumn.ColumnDefault		= tbColumn.HasDefault ? reader["columnDefault"].ToString() : string.Empty;
						tbTable.Columns.Add(tbColumn);
					}

				// se ho scelto di caricare anche le informazioni delle chiavi faccio un'ulteriore query per estrarre le colonne che appartengono alla PK
				if (withKeyInfo)
				{
					if (tbConnection.IsSqlConnection())
						query = @"select T.CONSTRAINT_NAME, CONSTRAINT_TYPE, K.COLUMN_NAME as column_name, K.ORDINAL_POSITION 
								from INFORMATION_SCHEMA.TABLE_CONSTRAINTS as T
								inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as K on T.CONSTRAINT_NAME = K.CONSTRAINT_NAME
								where T.TABLE_NAME = '{0}' AND CONSTRAINT_TYPE = 'PRIMARY KEY' order by K.ORDINAL_POSITION";

					using (TBCommand tbCommand = new TBCommand(string.Format(query, tableName), tbConnection))
					using (IDataReader reader = tbCommand.ExecuteReader())
						while (reader.Read())
						{
							string columnName = reader["column_name"].ToString();
							TBColumn tbColumn = tbTable.Columns.Find(col => string.Compare(col.Name, columnName, StringComparison.OrdinalIgnoreCase) == 0);
							if (tbColumn != null)
								tbColumn.IsKey = true;
						}
				}
				return tbTable;
			}
			catch (TBException e)
			{
				throw (e);
			}
			catch (Exception e)
			{
				throw (new TBException(e.Message, e));
			}
		}
		#endregion

		#region Caricamento info struttura di views e stored procedures 
		///<summary>
		/// LoadViewOrProcedureInfo (solo per SQL)
		/// Metodo utilizzato per caricare dalle tabelle di sistema le informazioni di oggetti di database
		/// di tipo VIEW e PROCEDURE, tra cui:
		/// - definizione completa (ovvero il testo dello script)
		/// - nomi colonne o alias per le views, oppure nome parametro per le procedures
		/// - tipo colonna
		/// - length colonna
		/// - se si tratta di un parametro di out (solo per le procedures)
		/// - collation della colonna
		/// N.B. Tale metodo è stato implementato in supporto alla gestione del parse degli script SQL e per
		/// la generazione di file xml comprensivi della struttura del database. Non viene effettuato un parse
		/// da file xchè troppo complesso e quindi si demanda la ricerca di tali informazioni alle tabelle di
		/// sistema del database.
		/// METODO RICHIAMATO DAL TBWIZARD!!!
		///</summary>
		/// @@TODOMICHI: PER ORA COMMENTO!
		//-------------------------------------------------------------------------
		/*public DataTable LoadViewOrProcedureInfo(string objectName, DBObjectTypes objectType, out string objectDefinition)
		{
			objectDefinition = string.Empty;

			if (!tbConnection.IsSqlConnection())
				throw (new TBException("LoadViewOrProcedureInfo is supported only for SQL connection"));

			if (objectType == DBObjectTypes.ALL || objectType == DBObjectTypes.TABLE || objectType == DBObjectTypes.SYNONYM)
				throw (new TBException("LoadViewOrProcedureInfo supported only VIEW or PROCEDURE types"));

			DataTable columnsDataTable = new DataTable();
			columnsDataTable.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));
			columnsDataTable.Columns.Add(new DataColumn("Type", Type.GetType("System.String")));
			columnsDataTable.Columns.Add(new DataColumn("Length", Type.GetType("System.Int32")));
			columnsDataTable.Columns.Add(new DataColumn("OutParam", Type.GetType("System.Int32")));
			columnsDataTable.Columns.Add(new DataColumn("Collation", Type.GetType("System.String")));

			// query x estrarre il testo completo dell'oggetto
			string querySysComments = "SELECT text FROM syscomments WHERE id = OBJECT_ID('{0}') and encrypted = 0";

			// query x individuare il nome delle colonne, tipo, lunghezza, collation e se parametro di out
			string queryInfo = @"SELECT SC.name as Name, 
								ST.name as Type, SC.length as Length, SC.isoutparam as OutParam,
								SC.collation as Collation
								FROM sysobjects AS SO
								INNER JOIN syscolumns AS SC ON SC.id = SO.id
								INNER JOIN systypes AS ST ON SC.xtype = ST.xtype
								WHERE SO.name = '{0}'
								order by SC.colorder";

			TBCommand tbCommand = null;
			IDataReader myReader = null;

			try
			{
				tbCommand = new TBCommand(string.Format(querySysComments, objectName), tbConnection);
				myReader = tbCommand.ExecuteReader();

				if (myReader != null)
				{
					while (myReader.Read())
						objectDefinition += myReader["text"].ToString();

					myReader.Close();
					myReader.Dispose();
				}

				tbCommand.CommandText = string.Format(queryInfo, objectName);
				myReader = tbCommand.ExecuteReader();

				if (myReader != null)
				{
					while (myReader.Read())
					{
						DataRow rowToAdd = columnsDataTable.NewRow();
						rowToAdd["Name"] = myReader[0].ToString();
						rowToAdd["Type"] = myReader[1].ToString();
						rowToAdd["Length"] = myReader[2].ToString();
						rowToAdd["OutParam"] = myReader[3].ToString();
						rowToAdd["Collation"] = myReader[4].ToString();
						columnsDataTable.Rows.Add(rowToAdd);
					}

					myReader.Close();
					myReader.Dispose();
				}

				tbCommand.Dispose();

				return columnsDataTable;
			}
			catch (TBException)
			{
				if (myReader != null && !myReader.IsClosed)
				{
					myReader.Close();
					myReader.Dispose();
				}

				tbCommand.Dispose();
				throw;
			}
		}*/
		#endregion

		#region Informazioni sugli oggetti dello schema
		///<summary>
		/// Generico metodo GetAllSchemaObjects per caricare l'elenco degli oggetti di un database
		/// a seconda del tipo specificato (tabelle, viste, stored procedures o tutti)
		/// A seconda del tipo di connessione vengono interrogate direttamente 
		/// le tabelle di sistema del specifico server di database.
		///</summary>
		//-----------------------------------------------------------------------
		public List<Dictionary<string, object>> GetAllSchemaObjects(DBObjectTypes dbObj)
		{
			try
			{
				if (tbConnection.IsSqlConnection())
					return GetSqlAllSchemaObjects(dbObj);
                else if (tbConnection.IsPostgreConnection())
                    return GetPostgreAllSchemaObjects(dbObj);

				return null;
			}
			catch (TBException)
			{
				throw;
			}
		}

		///<summary>
		/// SQL SERVER
		/// Per caricare l'elenco degli oggetti del database
		/// a seconda del tipo specificato (tabelle, viste, stored procedures o tutti)
		///</summary>
		//-----------------------------------------------------------------------
		private List<Dictionary<string, object>> GetSqlAllSchemaObjects(DBObjectTypes dbObj)
		{
			List<Dictionary<string, object>> resultsList = new List<Dictionary<string, object>>();

			string query = string.Empty;

			switch (dbObj)
			{
				case DBObjectTypes.TABLE:
					query = "select TABLE_NAME as name, '' as routineType, '' as definition from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";
					break;

				case DBObjectTypes.VIEW:
					query = "select TABLE_NAME as name, '' as routineType, VIEW_DEFINITION as definition from INFORMATION_SCHEMA.VIEWS";
					break;

				case DBObjectTypes.ROUTINE:
					query = "select ROUTINE_NAME as name, ROUTINE_TYPE as routineType, ROUTINE_DEFINITION as definition from INFORMATION_SCHEMA.ROUTINES";
					break;
			}

			try
			{
				using (SqlCommand command = new SqlCommand(query, tbConnection.SqlConnect))
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							if (reader["name"].ToString() == "dtproperties" ||
								reader["name"].ToString() == "sysalternates" ||
								reader["name"].ToString() == "sysconstraints" ||
								reader["name"].ToString() == "syssegments" ||
								reader["name"].ToString().IndexOf("dt_") == 0)
								continue;

							resultsList.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
						}
					}
			}
			catch (SqlException e)
			{
				throw (new TBException(e.Message, e));
			}
			
			return resultsList;
		}

		///<summary>
		/// Postgre
		/// Per caricare l'elenco degli oggetti del database
		/// a seconda del tipo specificato (tabelle, viste, stored procedures o tutti)
		///</summary>
		//-----------------------------------------------------------------------
		private List<Dictionary<string, object>> GetPostgreAllSchemaObjects(DBObjectTypes dbObj)
        {
			List<Dictionary<string, object>> resultsList = new List<Dictionary<string, object>>();

            string query = string.Empty;

            switch (dbObj)
            {
                case DBObjectTypes.TABLE:
                    query = string.Format("SELECT table_name AS name, '' AS routineType, '' AS definition FROM information_schema.tables WHERE table_schema = '{0}' AND table_type = 'BASE TABLE'", DatabaseLayerConsts.postgreDefaultSchema);
                    break;

                case DBObjectTypes.VIEW:
                    query = string.Format("SELECT table_name AS name, '' AS routineType, view_definition AS definition FROM information_schema.views WHERE table_schema = '{0}'", DatabaseLayerConsts.postgreDefaultSchema);
                    break;

                case DBObjectTypes.ROUTINE:
                    query = string.Format("SELECT routine_name AS name, routine_type AS routineType, routine_definition AS definition FROM information_schema.routines WHERE routine_schema = '{0}'", DatabaseLayerConsts.postgreDefaultSchema);
                    break;
            }

            try
            {
				using (NpgsqlCommand command = new NpgsqlCommand(query, tbConnection.PostgreConnect))
					using (NpgsqlDataReader reader = command.ExecuteReader())
						while (reader.Read())
							resultsList.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
            }
            catch (NpgsqlException e)
            {
                throw (new TBException(e.Message, e));
            }

            return resultsList;
        }
		#endregion

		#region Gestione default
		///<summary>
		/// Generico metodo LoadDefaults per caricare l'elenco dei constraint di default di una tabella
		/// A seconda del tipo di connessione vengono interrogate direttamente 
		/// le tabelle di sistema del specifico server di database.
		///</summary>
		//-----------------------------------------------------------------------
		/*public DefaultDataTable LoadDefaults(string tableName)
		{
			//@@TODOMICHI DataTable
			try
			{
				if (tbConnection.IsSqlConnection())
					return LoadSqlDefaults(tableName);
                else if (tbConnection.IsPostgreConnection())
                    return LoadPostgreDefaults(tableName);

				return null;
			}
			catch (TBException)
			{
				throw;
			}
		}*/

		///<summary>
		/// SQL SERVER
		/// Generico metodo LoadDefaults per caricare l'elenco dei constraint di default di una tabella
		///</summary>
		//-----------------------------------------------------------------------
		/*private DefaultDataTable LoadSqlDefaults(string tableName)
		{
			//@@TODOMICHI DataTable
			DefaultDataTable dataTable = new DefaultDataTable();

			string query = @"SELECT COLUMN_DEFAULT, COLUMN_NAME 
							FROM INFORMATION_SCHEMA.COLUMNS 
							WHERE TABLE_NAME = @tablename ORDER BY COLUMN_NAME";
			try
			{
				using (SqlCommand command = new SqlCommand(query, tbConnection.SqlConnect))
				{
					command.Parameters.AddWithValue("@tablename", tableName);
					using (SqlDataReader reader = command.ExecuteReader())
					{
						DataRow row = null;
						while (reader.Read())
						{
							row = dataTable.NewRow();
							row[0] = reader["COLUMN_NAME"].ToString();
							row[1] = reader["COLUMN_DEFAULT"] != System.DBNull.Value;
							row[2] = reader["COLUMN_DEFAULT"].ToString();
							dataTable.Rows.Add(row);
						}
					}
				}
			}
			catch (SqlException e)
			{
				throw (new TBException(e.Message, e));
			}

			return dataTable;
		}*/

		///<summary>
		/// Postgre
		/// Generico metodo LoadDefaults per caricare l'elenco dei constraint di default di una tabella
		///</summary>
		//-----------------------------------------------------------------------
		/*private DefaultDataTable LoadPostgreDefaults(string tableName)
        {
			//@@TODOMICHI DataTable
            DefaultDataTable dataTable = new DefaultDataTable();

            string query = string.Format
				("SELECT COLUMN_DEFAULT, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tablename and table_schema='{0}' ORDER BY COLUMN_NAME", 
				DatabaseLayerConsts.postgreDefaultSchema);

            try
            {
				using (NpgsqlCommand command = new NpgsqlCommand(query, tbConnection.PostgreConnect))
				{
					command.Parameters.AddWithValue("@tablename", tableName.ToLower());
					using (NpgsqlDataReader reader = command.ExecuteReader())
					{
						DataRow row = null;
						while (reader.Read())
						{
							row = dataTable.NewRow();
							row[0] = reader["column_name"].ToString();
							row[1] = reader["column_default"] != System.DBNull.Value;
							row[2] = reader["column_default"].ToString();
							dataTable.Rows.Add(row);
						}
					}
				}
            }
            catch (NpgsqlException e)
            {
                throw (new TBException(e.Message, e));
            }

            return dataTable;
        }*/
		#endregion

		#region Gestione ForeignKeys
		/// <summary>
		/// Generico metodo LoadFKConstraints per caricare le informazioni relative 
		/// ai foreign key constraints relativi alla tabella.
		/// A seconda del tipo di connessione vengono interrogate direttamente 
		/// le tabelle di sistema del specifico server di database.
		/// Richiamato dall'importazione dati per abilitare/disabilitare i constraint di FK
		/// </summary>	
		//---------------------------------------------------------------------------
		public ForeignKeyArray LoadFKConstraints(string tableName)
		{
			try
			{
				if (tbConnection.IsSqlConnection())
					return LoadSqlFKConstraints(tableName);
                else if (tbConnection.IsPostgreConnection())
                   return LoadPostgreFKConstraints(tableName);

				return null;
			}
			catch (TBException)
			{
				throw;
			}
		}

		/// <summary>
		/// SQL SERVER
		/// carica le informazioni relative al constraint di FK e la tabella referenziata
		/// </summary>	
		//---------------------------------------------------------------------------
		private ForeignKeyArray LoadSqlFKConstraints(string tableName)
		{
			ForeignKeyArray fkList = new ForeignKeyArray();

			string query = @"SELECT DISTINCT X.name AS FKName, Y.name AS PKTableName 
							FROM sysobjects X, sysobjects Y, sysforeignkeys F 
							WHERE X.id = F.constid AND F.rkeyid = Y.id AND F.fkeyid = 
							(SELECT id FROM sysobjects WHERE name = @tablename AND xtype = @type)";
			try
			{
				using (SqlCommand command = new SqlCommand(query, tbConnection.SqlConnect))
				{
					command.Parameters.AddWithValue("@tablename", tableName);
					command.Parameters.AddWithValue("@type", "U");
					using (SqlDataReader reader = command.ExecuteReader())
						while (reader.Read())
							fkList.Add(reader["FKName"].ToString(), reader["PKTableName"].ToString());
				}
			}
			catch (SqlException e)
			{
				throw (new TBException(e.Message, e));
			}

			return fkList;
		}

		/// <summary>
		/// SQL SERVER
		/// carica le informazioni relative al constraint di FK, la tabella referenziata e le rispettive colonne
		/// </summary>	
		//---------------------------------------------------------------------------
		public ForeignKeyArray LoadSqlFkConstraintsInfo(string tableName)
		{
			ForeignKeyArray fkList = new ForeignKeyArray();

			string query = @"SELECT C.CONSTRAINT_NAME AS FKName, PK.TABLE_NAME AS PKTableName, CU.COLUMN_NAME AS FKColumn, PT.COLUMN_NAME AS PKColumn
							FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
							INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
							INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
							INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
							INNER JOIN (SELECT i1.TABLE_NAME, i2.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
							INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY') PT
							ON PT.TABLE_NAME = PK.TABLE_NAME
							WHERE FK.TABLE_NAME = '{0}'";
			try
			{
				using (TBCommand command = new TBCommand(string.Format(query, tableName), tbConnection))
					using (IDataReader reader = command.ExecuteReader())
						while (reader.Read())
							fkList.Add(reader["FKName"].ToString(), reader["PKTableName"].ToString(), reader["FKColumn"].ToString(), reader["PKColumn"].ToString());
			}
			catch (TBException e)
			{
				throw(e);
			}

			return fkList;
		}
		///<summary>
		/// Postgre load fk info
		///</summary>
		//---------------------------------------------------------------------------
		public ForeignKeyArray LoadPostgreFkConstraintsInfo(string tableName)
        {
			ForeignKeyArray fkList = new ForeignKeyArray();

			string query = @"SELECT tc.constraint_name AS fk_name, kcu.column_name AS fk_column_name, ccu.table_name AS pk_table_name, ccu.column_name AS pk_column_name 
                             FROM information_schema.table_constraints AS tc 
                             JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
                             JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
                             WHERE constraint_type = 'FOREIGN KEY' AND tc.table_name = @tablename;";

            try
            {
				using (NpgsqlCommand command = new NpgsqlCommand(query, tbConnection.PostgreConnect))
				{
					command.Parameters.AddWithValue("@tablename", tableName.ToLower());
					using (NpgsqlDataReader reader = command.ExecuteReader())
						while (reader.Read())
							fkList.Add(reader["fk_name"].ToString(), reader["pk_table_name"].ToString(), reader["fk_column_name"].ToString(), reader["pk_column_name"].ToString());
				}
            }
            catch (NpgsqlException e)
            {
                throw (new TBException(e.Message, e));
            }

			return fkList;
		}

		/// <summary>
		/// Postgre
		/// carica le informazioni relative alla foreign key constraints relative alla tabella 
		/// </summary>	
		//---------------------------------------------------------------------------
		private ForeignKeyArray LoadPostgreFKConstraints(string tableName)
        {
			ForeignKeyArray fkList = new ForeignKeyArray();

            string query = string.Format(@"SELECT tc.constraint_name AS FKName, ccu.table_name AS TableName
                            FROM information_schema.table_constraints AS tc 
                            JOIN information_schema.constraint_column_usage AS ccu
                            ON ccu.constraint_name = tc.constraint_name
                            WHERE constraint_type = 'FOREIGN KEY' AND tc.table_name=@tablename AND tc.constraint_schema='{0}'", 
							DatabaseLayerConsts.postgreDefaultSchema);

            try
            {
				using (NpgsqlCommand command = new NpgsqlCommand(query, tbConnection.PostgreConnect))
				{
					command.Parameters.AddWithValue("@tablename", tableName.ToLower());
					using (NpgsqlDataReader reader = command.ExecuteReader())
						while (reader.Read())
							fkList.Add(reader["fkname"].ToString(), reader["tablename"].ToString());
				}
            }
            catch (NpgsqlException e)
            {
                throw (new TBException(e.Message, e));
            }

            return fkList;
        }

		/// <summary>
		/// Generico metodo LoadRefFKConstraints per caricare le informazioni relative 
		/// ai foreign key constraints che riferiscono alla tabella.
		/// A seconda del tipo di connessione vengono interrogate direttamente 
		/// le tabelle di sistema del specifico server di database.
		/// </summary>	
		//---------------------------------------------------------------------------
		public ForeignKeyArray LoadRefFKConstraints(string refTableName)
		{
			try
			{
				if (tbConnection.IsSqlConnection())
					return LoadRefSqlFKConstraints(refTableName);
                else if (tbConnection.IsPostgreConnection())
                    return LoadRefPostgreFKConstraints(refTableName);

				return null;
			}
			catch (TBException)
			{
				throw;
			}
		}

		///<summary>
		/// SQL SERVER
		///	carica le informazioni relative alla foreign key constraints che riferiscono la tabella
		///</summary>
		//---------------------------------------------------------------------------
		private ForeignKeyArray LoadRefSqlFKConstraints(string refTableName)
		{
			ForeignKeyArray fkList = new ForeignKeyArray();

			string query = @"SELECT DISTINCT X.name AS FKName, Y.name AS TableName 
							FROM sysobjects X, sysobjects Y, sysforeignkeys F 
							WHERE X.id = F.constid AND F.fkeyid = Y.id AND F.rkeyid = 
							(SELECT id FROM sysobjects WHERE name = @tablename AND xtype = @type)";

			try
			{
				using (SqlCommand command = new SqlCommand(query, tbConnection.SqlConnect))
				{
					command.Parameters.AddWithValue("@tablename", refTableName);
					command.Parameters.AddWithValue("@type", "U");
					
					using (SqlDataReader reader = command.ExecuteReader())
						while (reader.Read())
							fkList.Add(reader["FKName"].ToString(), reader["TableName"].ToString());
				}
			}
			catch (SqlException e)
			{
				throw (new TBException(e.Message, e));
			}

			return fkList;
		}

		//---------------------------------------------------------------------------
		private ForeignKeyArray LoadRefPostgreFKConstraints(string tableName)
		{
			ForeignKeyArray fkList = new ForeignKeyArray();

			string query = string.Format(@"SELECT tc.constraint_name AS FKName, ccu.table_name AS TableName
                            FROM information_schema.table_constraints AS tc 
                            JOIN information_schema.constraint_column_usage AS ccu
                            ON ccu.constraint_name = tc.constraint_name
                            WHERE constraint_type = 'FOREIGN KEY' AND ccu.table_name='{1}' AND tc.constraint_schema='{0}'",
							DatabaseLayerConsts.postgreDefaultSchema, tableName.ToLower());

			try
			{
				using (NpgsqlCommand command = new NpgsqlCommand(query, tbConnection.PostgreConnect))
				using (NpgsqlDataReader reader = command.ExecuteReader())
					while (reader.Read())
						fkList.Add(reader["fkname"].ToString(), reader["tablename"].ToString());
			}
			catch (NpgsqlException e)
			{
				throw (new TBException(e.Message, e));
			}

			return fkList;
		}
		#endregion

		#region Gestione PK e Indici
		//---------------------------------------------------------------------------
		public IndexArray LoadSqlIndexes(string tableName)
		{
			IndexArray idxArray = new IndexArray();

			string idxQuery = @"SELECT ind.name AS IndexName, col.name AS ColumnName, ind.is_primary_key AS IsPrimary
								FROM sys.indexes ind 
								INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
								INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
								INNER JOIN sys.tables t ON ind.object_id = t.object_id 
								WHERE t.name = '{0}'
								ORDER BY ind.name, ind.index_id, ic.index_column_id;";
			try
			{
				using (TBCommand myCommand = new TBCommand(string.Format(idxQuery, tableName), tbConnection))
				using (IDataReader reader = myCommand.ExecuteReader())
					while (reader.Read())
					{
						string idxName = reader["IndexName"].ToString();
						string colName = reader["ColumnName"].ToString();
						bool isPrimary = (string.Compare(reader["IsPrimary"].ToString(), "1") == 0);
						idxArray.AddIndex(idxName, isPrimary, colName);
					}
			}
			catch (SqlException e)
			{
				throw (new TBException(e.Message, e));
			}

			return idxArray;
		}

		//---------------------------------------------------------------------------
		public IndexArray LoadPostgreIndexesInfo(string tableName)
        {
			IndexArray idxArray = new IndexArray();

			string idxText = string.Format(@"SELECT i.relname as INDEX_NAME, idx.indisprimary as PKNUMB, idx.indisunique as UNIQUENESS, idx.indisclustered as CLUSTERED,
                              ARRAY(SELECT pg_get_indexdef(idx.indexrelid, k + 1, true) FROM generate_subscripts(idx.indkey, 1) as k ORDER BY k) as COLUMN_NAME
                              FROM   pg_index as idx
                              JOIN   pg_class as i
                              ON     i.oid = idx.indexrelid
                              JOIN   pg_namespace as ns
                              ON     ns.oid = i.relnamespace
                              AND    ns.nspname ='{0}'
                              WHERE  idx.indrelid::regclass = @tableName::regclass", DatabaseLayerConsts.postgreDefaultSchema);
            try
            {
				using (NpgsqlCommand idxCommand = new NpgsqlCommand(idxText, tbConnection.PostgreConnect))
				{
					idxCommand.Parameters.AddWithValue("@tableName", tableName);
					using (NpgsqlDataReader idxReader = idxCommand.ExecuteReader())
						while (idxReader.Read())
						{
							string idxName = idxReader["INDEX_NAME"].ToString();
							string colName = idxReader["COLUMN_NAME"].ToString(); //non funziona in portgre. da rivedere
							bool isPrimary = (string.Compare(idxReader["PKNUMB"].ToString(), "1") == 0); // cosa ritorna questa colonna
							idxArray.AddIndex(idxName, isPrimary, colName);
						}
				}
            }
            catch (NpgsqlException e)
            {
                throw (new TBException(e.Message, e));
            }
         
            return idxArray;
        }
		#endregion
	}

	//============================================================================
	public class TBTable
	{
		private string name;

		public List<TBColumn> Columns = new List<TBColumn>();
		public string Name { get { return name; } }

		//---------------------------------------------------------------------
		public TBTable(string name)
		{
			this.name = name;
		}
	}

	//============================================================================
	public class TBColumn
	{
		private string	name;
		public bool		IsNullable = true;
		public int		NumericPrecision = -1;
		public int		NumericScale = -1;
		public int		ColumnSize = 0;
		public string	DataType = string.Empty;
		public string	CollationName = string.Empty;
		public bool		IsAutoIncrement = false;
		public bool		IsKey = false;
		public bool		HasDefault = false;
		public string	ColumnDefault = string.Empty;

		public string Name { get { return name; } }
	
		//---------------------------------------------------------------------
		public TBColumn(string name)
		{
			this.name = name;
		}
	}
}
