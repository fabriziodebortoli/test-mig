//using System;
//using System.Collections;
//using System.Data;

//using System.Data.SqlClient;
//using System.Diagnostics;
//using System.Globalization;
//using System.Text.RegularExpressions;
//using System.Xml;

//using Npgsql;
//using NpgsqlTypes;
//using System.Data.Common;

//#pragma warning disable 0618
//// disabilito temporaneamente warning CS0618: 'System.Data.OracleClient.OracleConnection' is obsolete: 
//// 'OracleConnection has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260'

//// Insieme di classi in grado di richiamare, a seconda del DBMS (SqlServer / Oracle)
//// le classi native .NET per effettuare operazioni sul database.

//namespace TaskBuilderNetCore.Data
//{
//    #region Public enums
//    //---------------------------------------------------------------------------
//    public enum DatabaseCheckError
//    {
//        NoDatabase,
//        NoTables,
//        NoActivatedDatabase,
//        InvalidModule,
//        Error,
//        NoError,
//        DBSizeError,
//        Sql2012NotAllowedForCompany,
//        Sql2012NotAllowedForDMS
//    }

//    //---------------------------------------------------------------------------
//    public enum DBObjectTypes
//    {
//        ALL,
//        TABLE,
//        VIEW,
//        ROUTINE,
//        SYNONYM
//    };

//    /// <summary>
//    /// SQLServerEdition enum: per individuare l'edizione di un server SQL
//    /// </summary>
//    //---------------------------------------------------------------------------
//    public enum SQLServerEdition
//    {
//        SqlServer2000,
//        SqlServer2005,
//        SqlServer2008,
//        SqlServer2012,
//        SqlServer2014,
//        SqlServer2016,
//        MSDE2000,
//        SqlExpress2005,
//        SqlExpress2008,
//        SqlExpress2012,
//        SqlExpress2014,
//        SqlExpress2016,
//        SqlAzureV12,
//        Undefined
//    }
//    #endregion

//    #region Class TBException
//    /// <summary>
//    /// Classe per la gestione delle Exception
//    /// </summary>
//    //============================================================================
//    public class TBException : Exception
//    {
//        #region Constructors
//        //---------------------------------------------------------------------------
//        public TBException(string message, Exception inner) : base(message, inner) { }
//        //---------------------------------------------------------------------------
//        public TBException(string message) : this(message, null) { }
//        //---------------------------------------------------------------------------
//        public TBException() : this(string.Empty, null) { }
//        #endregion

//        #region Properties TBException
//        //-----------------------------------------------------------------------
//        public string Number
//        {
//            get
//            {
//                if (InnerException == null)
//                    return "0";

//                if (InnerException is SqlException)
//                    return ((SqlException)InnerException).Number.ToString();

//                //if (InnerException is OracleException)
//                //    return ((OracleException)InnerException).Code;

//                if (InnerException is NpgsqlException)
//                    return ((PostgresException)InnerException).Code;

//                return "0";
//            }
//        }

//        //-----------------------------------------------------------------------
//        public SqlErrorCollection Errors
//        {
//            get
//            {
//                if (InnerException != null && InnerException is SqlException)
//                    return ((SqlException)InnerException).Errors;

//                return null;
//            }
//        }

//        //-----------------------------------------------------------------------
//        public string Procedure
//        {
//            get
//            {
//                if (InnerException == null)
//                    return string.Empty;

//                if (InnerException is SqlException)
//                    return ((SqlException)InnerException).Procedure;

//                //if (InnerException is OracleException)
//                //    return ((OracleException)InnerException).Source;

//                if (InnerException is NpgsqlException)
//                    return ((NpgsqlException)InnerException).Source;

//                return string.Empty;
//            }
//        }

//        //-----------------------------------------------------------------------
//        public string ExtendedMessage
//        {
//            get
//            {
//                if (InnerException == null || string.IsNullOrWhiteSpace(InnerException.Message))
//                    return Message;
//                return Message + "\n(" + InnerException.Message + ")";
//            }
//        }

//        //-----------------------------------------------------------------------
//        public string Server
//        {
//            get
//            {
//                if (InnerException != null && InnerException is SqlException)
//                    return ((SqlException)InnerException).Server;

//                return string.Empty;
//            }
//        }
//        #endregion
//    }
//    #endregion

//    #region Class TBType
//    /// <summary>
//    /// Utilizzata per mappare i tipi SqlDbType - OracleType
//    /// </summary>
//    //============================================================================
//    public class TBType
//    {
//        private object dbType = null;

//        //------------------------------------------------------------------
//        public TBType(object value)
//        {
//            dbType = value;
//        }

//        #region Properties
//        //------------------------------------------------------------------
//        internal SqlDbType SqlDbType
//        {
//            get
//            {
//                if (dbType is SqlDbType)
//                    return (SqlDbType)dbType;
//                throw (new TBException("TBType.SqlDbType: " + DatabaseLayerStrings.InvalidCast));
//            }
//            set
//            {
//                if (dbType is SqlDbType)
//                    dbType = (SqlDbType)value;
//                throw (new TBException("TBType.SqlDbType: " + DatabaseLayerStrings.InvalidCast));
//            }
//        }

//        //------------------------------------------------------------------
//        //internal OracleType OracleType
//        //{
//        //    get
//        //    {
//        //        if (dbType is OracleType)
//        //            return (OracleType)dbType;
//        //        throw (new TBException("TBType.OracleType: " + DatabaseLayerStrings.InvalidCast));
//        //    }
//        //    set
//        //    {
//        //        if (dbType is OracleType)
//        //            dbType = (OracleType)value;
//        //        throw (new TBException("TBType.OracleType: " + DatabaseLayerStrings.InvalidCast));
//        //    }
//        //}



//        //------------------------------------------------------------------
//        internal NpgsqlDbType PostgreType
//        {
//            get
//            {
//                if (dbType is NpgsqlDbType)
//                    return (NpgsqlDbType)dbType;
//                throw (new TBException("TBType.PostgreType: " + DatabaseLayerStrings.InvalidCast));
//            }
//            set
//            {
//                if (dbType is NpgsqlDbType)
//                    dbType = (NpgsqlDbType)value;
//                throw (new TBException("TBType.PostgreType: " + DatabaseLayerStrings.InvalidCast));
//            }
//        }
//        #endregion

//        #region OperatorCast
//        //------------------------------------------------------------------
//        public static implicit operator TBType(SqlDbType sqlType)
//        {
//            return new TBType(sqlType);
//        }

//        public static implicit operator TBType(NpgsqlDbType npgType)
//        {
//            return new TBType(npgType);
//        }

//        //------------------------------------------------------------------
//        //public static implicit operator TBType(OracleType oraType)
//        //{
//        //    return new TBType(oraType);
//        //}
//        #endregion

//        #region Methods
//        //------------------------------------------------------------------
//        public bool IsSqlDbType()
//        {
//            return dbType is SqlDbType;
//        }

//        //------------------------------------------------------------------
//        public bool IsPostgreType()
//        {
//            return dbType is NpgsqlDbType;
//        }

//        //------------------------------------------------------------------
//        //public bool IsOracleType()
//        //{
//        //    return dbType is OracleType;
//        //}

//        #endregion
//    }
//    #endregion

//    #region Class TBDatabaseType
//    //============================================================================
//    public class TBDatabaseType
//    {
//        #region Static methods
//        //---------------------------------------------------------------------------		
//        public static DBMSType GetDBMSType(string provider)
//        {
//            if (string.Compare(provider, NameSolverDatabaseStrings.SQLODBCProvider, StringComparison.OrdinalIgnoreCase) == 0)
//                return DBMSType.SQLSERVER;

//            //if (string.Compare(provider, NameSolverDatabaseStrings.OraOLEDBProvider, StringComparison.OrdinalIgnoreCase) == 0 ||
//            //    string.Compare(provider, NameSolverDatabaseStrings.MSDAORAProvider, StringComparison.OrdinalIgnoreCase) == 0)
//            //    return DBMSType.ORACLE;

//            if (string.Compare(provider, NameSolverDatabaseStrings.PostgreOdbcProvider, StringComparison.OrdinalIgnoreCase) == 0)
//                return DBMSType.POSTGRE;

//            return DBMSType.UNKNOWN;
//        }

//        //---------------------------------------------------------------------------		
//        public static string GetProvider(DBMSType dbmsType)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                return NameSolverDatabaseStrings.SQLOLEDBProvider;

//            if (dbmsType == DBMSType.ORACLE)
//                return NameSolverDatabaseStrings.OraOLEDBProvider;

//            if (dbmsType == DBMSType.POSTGRE)
//                return NameSolverDatabaseStrings.PostgreOdbcProvider;

//            return string.Empty;
//        }

//        //---------------------------------------------------------------------------		
//        public static string GetConnectionString
//            (
//            string server,
//            string user,
//            string password,
//            string catalog,
//            DBMSType dbmsType,
//            bool winNtAuthent,
//            int port = 0
//            )
//        {
//            string connectionString = string.Empty;

//            switch (dbmsType)
//            {
//                case DBMSType.ORACLE:
//                    {
//                        if (server.Length == 0 || (!winNtAuthent && (user.Length == 0 || password.Length == 0)))
//                            return string.Empty;

//                        connectionString = (winNtAuthent)
//                            ? string.Format(NameSolverDatabaseStrings.OracleWinNtConnection, server)
//                            : string.Format(NameSolverDatabaseStrings.OracleConnection, server, user, password);
//                        break;
//                    }
//                case DBMSType.SQLSERVER:
//                    {
//                        if (server.Length == 0 || catalog.Length == 0 || (!winNtAuthent && user.Length == 0))
//                            return string.Empty;

//                        connectionString = (winNtAuthent)
//                            ? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, server, catalog)
//                            : string.Format(NameSolverDatabaseStrings.SQLConnection, server, catalog, user, password);
//                        break;
//                    }

//                case DBMSType.POSTGRE:
//                    {
//                        if (server.Length == 0 || catalog.Length == 0 || (!winNtAuthent && user.Length == 0))
//                            return string.Empty;
//                        if (port == 0) port = DatabaseLayerConsts.postgreDefaultPort;
//                        if (password.Length==0 || password==null)
//                            password = DatabaseLayerConsts.postgreDefaultPassword;

//                        connectionString = (winNtAuthent)
//                            ? string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, server.ToLower(), port, catalog.ToLower(), DatabaseLayerConsts.postgreDefaultSchema)
//                            : string.Format(NameSolverDatabaseStrings.PostgreConnection, server, port, catalog, user.ToLower(), password, DatabaseLayerConsts.postgreDefaultSchema);
//                        break;
//                    }

//                default:
//                    throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//            }

//            return connectionString;
//        }

//        //---------------------------------------------------------------------------		
//        public static string GetConnectionString
//            (
//            string server,
//            string user,
//            string password,
//            string catalog,
//            string providerName,
//            bool winNtAuthent,
//            bool useProvider,
//            int port = 0
//            )
//        {
//            DBMSType dbmsType = GetDBMSType(providerName);

//            string connectionString = GetConnectionString(server, user, password, catalog, dbmsType, winNtAuthent, port);

//            if (useProvider && connectionString.Length > 0)
//            {
//                if (string.Compare(providerName, NameSolverDatabaseStrings.SQLODBCProvider, true) == 0)
//                    connectionString = string.Format(NameSolverDatabaseStrings.DriverConnAttribute, providerName) + connectionString;
//                else
//                    connectionString = string.Format(NameSolverDatabaseStrings.ProviderConnAttribute, providerName) + connectionString;

//            }

//            return connectionString;
//        }

//        //---------------------------------------------------------------------------		
//        public static string GetDBDataType(TBType dbType, DBMSType dbmsType)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                return (dbType.SqlDbType).ToString();

//            //if (dbmsType == DBMSType.ORACLE)
//            //{
//            //    if ((dbType.OracleType) == OracleType.VarChar)
//            //        return "VarChar2";
//            //    if ((dbType.OracleType) == OracleType.NVarChar)
//            //        return "NVarChar2";
//            //    if ((dbType.OracleType) == OracleType.DateTime)
//            //        return "Date";
//            //    if ((dbType.OracleType) == OracleType.LongRaw)
//            //        return "Long Raw";

//            //    return (dbType.OracleType).ToString();
//            //}

//            if (dbmsType == DBMSType.POSTGRE)
//            {
//                return (dbType.PostgreType).ToString();
//            }

//            throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//        }

//        //------------------------------------------------------------------
//        public static TBType GetTBType(DBMSType dbmsType, int providerType)
//        {
//            TBType myTBType = null;

//            if (dbmsType == DBMSType.SQLSERVER)
//            {
//                switch (providerType)
//                {
//                    case 2: // Bit
//                        myTBType = new TBType(SqlDbType.Bit);
//                        break;
//                    case 3: // Char
//                        myTBType = new TBType(SqlDbType.Char);
//                        break;
//                    case 4: // DateTime
//                        myTBType = new TBType(SqlDbType.DateTime);
//                        break;
//                    case 5: // Decimal
//                        myTBType = new TBType(SqlDbType.Decimal);
//                        break;
//                    case 6: // Float
//                        myTBType = new TBType(SqlDbType.Float);
//                        break;
//                    case 8: // Int
//                        myTBType = new TBType(SqlDbType.Int);
//                        break;
//                    case 10: // NChar
//                        myTBType = new TBType(SqlDbType.NChar);
//                        break;
//                    case 11: // NText
//                        myTBType = new TBType(SqlDbType.NText);
//                        break;
//                    case 12: // NVarChar
//                        myTBType = new TBType(SqlDbType.NVarChar);
//                        break;
//                    case 14: // UniqueIdentifier
//                        myTBType = new TBType(SqlDbType.UniqueIdentifier);
//                        break;
//                    case 16: // SmallInt
//                        myTBType = new TBType(SqlDbType.SmallInt);
//                        break;
//                    case 18: // Text
//                        myTBType = new TBType(SqlDbType.Text);
//                        break;
//                    case 22: // VarChar
//                    default:
//                        myTBType = new TBType(SqlDbType.VarChar);
//                        break;
//                }

//                return myTBType;
//            }

//            //if (dbmsType == DBMSType.ORACLE)
//            //{
//            //    switch (providerType)
//            //    {
//            //        case 3: // Char
//            //            myTBType = new TBType(OracleType.Char);
//            //            break;
//            //        case 4: // Clob
//            //            myTBType = new TBType(OracleType.Clob);
//            //            break;
//            //        case 6: // DateTime
//            //            myTBType = new TBType(OracleType.DateTime);
//            //            break;
//            //        case 11: // NChar
//            //            myTBType = new TBType(OracleType.NChar);
//            //            break;
//            //        case 12: // NClob
//            //            myTBType = new TBType(OracleType.NClob);
//            //            break;
//            //        case 13: // Number
//            //            myTBType = new TBType(OracleType.Number);
//            //            break;
//            //        case 14: // NVarChar
//            //            myTBType = new TBType(OracleType.NVarChar);
//            //            break;
//            //        case 27: // Int16
//            //            myTBType = new TBType(OracleType.Int16);
//            //            break;
//            //        case 28: // Int32
//            //            myTBType = new TBType(OracleType.Int32);
//            //            break;
//            //        case 29: // Float
//            //            myTBType = new TBType(OracleType.Float);
//            //            break;
//            //        case 22: // VarChar
//            //        default:
//            //            myTBType = new TBType(OracleType.VarChar);
//            //            break;
//            //    }

//            //    return myTBType;
//            //}

//            if (dbmsType == DBMSType.POSTGRE)
//            {
//                switch (providerType)
//                {
//                    case 2: // Bit
//                        myTBType = new TBType(NpgsqlDbType.Bit);
//                        break;
//                    case 3: // Char
//                        myTBType = new TBType(NpgsqlDbType.Char);
//                        break;
//                    case 4: // DateTime
//                        myTBType = new TBType(NpgsqlDbType.Date);
//                        break;
//                    case 5: // Decimal
//                        myTBType = new TBType(NpgsqlDbType.Double);
//                        break;
//                    case 6: // Float
//                        myTBType = new TBType(NpgsqlDbType.Real);
//                        break;
//                    case 8: // Int
//                        myTBType = new TBType(NpgsqlDbType.Integer);
//                        break;                                 
//                    case 14: // UniqueIdentifier
//                        myTBType = new TBType(NpgsqlDbType.Uuid);
//                        break;
//                    case 16: // SmallInt
//                        myTBType = new TBType(NpgsqlDbType.Smallint);
//                        break;
//                    case 18: // Text
//                        myTBType = new TBType(NpgsqlDbType.Text);
//                        break;
//                    case 22: // VarChar
//                    default:
//                        myTBType = new TBType(NpgsqlDbType.Varchar);
//                        break;
//                }

//                return myTBType;
//            }
//            return myTBType;
//        }      
       
//        #region Metodi per ottenere un OracleType
//        ///<summary>
//        ///	Per ottenere un OracleType da un SqlDbType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        //internal static OracleType GetOracleType(SqlDbType sqlType)
//        //{
//        //    switch (sqlType)
//        //    {
//        //        case SqlDbType.VarChar: return OracleType.VarChar;
//        //        case SqlDbType.Char: return OracleType.Char;
//        //        case SqlDbType.Bit: return OracleType.Char;
//        //        case SqlDbType.DateTime: return OracleType.DateTime;
//        //        case SqlDbType.Float: return OracleType.Float;
//        //        case SqlDbType.Int: return OracleType.Int32;
//        //        case SqlDbType.SmallInt: return OracleType.Int16;
//        //        case SqlDbType.Text: return OracleType.Clob;
//        //        case SqlDbType.UniqueIdentifier: return OracleType.Char;
//        //        case SqlDbType.NChar: return OracleType.NChar;
//        //        case SqlDbType.NVarChar: return OracleType.NVarChar;
//        //        case SqlDbType.NText: return OracleType.NClob;
//        //    }
//        //    return OracleType.VarChar;
//        //}

//        ///<summary>
//        ///	Per ottenere un OracleType da un NpgsqlDbType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        //internal static OracleType GetOracleType(NpgsqlDbType npgType)
//        //{
//        //    switch (npgType)
//        //    {
//        //        case NpgsqlDbType.Varchar: return OracleType.VarChar;
//        //        case NpgsqlDbType.Char: return OracleType.Char;
//        //        case NpgsqlDbType.Bit: return OracleType.Char;
//        //        case NpgsqlDbType.Timestamp: return OracleType.DateTime;
//        //        case NpgsqlDbType.Double: return OracleType.Float;
//        //        case NpgsqlDbType.Integer: return OracleType.Int32;
//        //        case NpgsqlDbType.Smallint: return OracleType.Int16;
//        //        case NpgsqlDbType.Text: return OracleType.Clob;
//        //        case NpgsqlDbType.Uuid: return OracleType.Char;
//        //    }
//        //    return OracleType.VarChar;
//        //}


//        ///<summary>
//        /// Per ottenere un OracleType da un generico TBType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        //internal static OracleType GetOracleType(TBType dbType)
//        //{
//        //    if (dbType.IsOracleType())
//        //        return dbType.OracleType;

//        //    if (dbType.IsSqlDbType())
//        //        return GetOracleType(dbType.SqlDbType);

//        //    if (dbType.IsPostgreType())
//        //        return GetOracleType(dbType.PostgreType);

//        //    return OracleType.VarChar;
//        //}
//        #endregion

//        #region Metodi per ottenere un SqlDbType
//        ///<summary>
//        ///	Per ottenere un SqlDbType da un OracleType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        //internal static SqlDbType GetSqlDbType(OracleType oracleType)
//        //{
//        //    switch (oracleType)
//        //    {
//        //        case OracleType.VarChar: return SqlDbType.VarChar;
//        //        case OracleType.Char: return SqlDbType.Char;
//        //        //case OracleType.Bit: return SqlDbType.Char;
//        //        case OracleType.DateTime: return SqlDbType.DateTime;
//        //        case OracleType.Float: return SqlDbType.Float;
//        //        case OracleType.Int32: return SqlDbType.Int;
//        //        case OracleType.Int16: return SqlDbType.SmallInt;
//        //        case OracleType.Clob: return SqlDbType.Text;
//        //        //case OracleType.UniqueIdentifier: return SqlDbType.Char;
//        //        case OracleType.NChar: return SqlDbType.NChar;
//        //        case OracleType.NVarChar: return SqlDbType.NVarChar;
//        //        case OracleType.NClob: return SqlDbType.NText;
//        //    }

//        //    return SqlDbType.VarChar;
//        //}


//        //<summary>
//        ///	Per ottenere un SqlDbType da un NpgsqlDbType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        internal static SqlDbType GetSqlDbType(NpgsqlDbType npgType)
//        {
//            switch (npgType)
//            {
//                case NpgsqlDbType.Varchar: return SqlDbType.VarChar;
//                case NpgsqlDbType.Char: return SqlDbType.Char;
//                case NpgsqlDbType.Timestamp: return SqlDbType.DateTime;
//                case NpgsqlDbType.Double: return SqlDbType.Float;
//                case NpgsqlDbType.Integer: return SqlDbType.Int;
//                case NpgsqlDbType.Smallint: return SqlDbType.SmallInt;
//                case NpgsqlDbType.Text: return SqlDbType.Text;
//                    //case OracleType.UniqueIdentifier: return SqlDbType.Char;
//            }

//            return SqlDbType.VarChar;
//        }


//        ///<summary>
//        ///	Per ottenere un SqlDbType da un generico TBType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        internal static SqlDbType GetSqlDbType(TBType dbType)
//        {
//            //if (dbType.IsOracleType())
//            //    return GetSqlDbType(dbType.OracleType);

//            if (dbType.IsPostgreType())
//                return GetSqlDbType(dbType.PostgreType);

//            if (dbType.IsSqlDbType())
//                return dbType.SqlDbType;

//            return SqlDbType.VarChar;
//        }
//        #endregion


//        #region Metodi per ottenere un NpgsqlDbType
//        ///<summary>
//        ///	Per ottenere un PostgreDbType da un OracleType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        //internal static NpgsqlDbType GetPostgreDbType(OracleType oracleType)
//        //{
//        //    switch (oracleType)
//        //    {
//        //        case OracleType.VarChar: return NpgsqlDbType.Varchar;
//        //        case OracleType.Char: return NpgsqlDbType.Char;

//        //        case OracleType.DateTime: return NpgsqlDbType.Timestamp;
//        //        case OracleType.Float: return NpgsqlDbType.Double;
//        //        case OracleType.Int32: return NpgsqlDbType.Integer;
//        //        case OracleType.Int16: return NpgsqlDbType.Smallint;
//        //        case OracleType.Clob: return NpgsqlDbType.Text;

//        //    }

//        //    return NpgsqlDbType.Varchar;
//        //}

//        ///<summary>
//        ///	Per ottenere un PostgreDbType da un SqlDbType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        internal static NpgsqlDbType GetPostgreDbType(SqlDbType sqlType)
//        {
//            switch (sqlType)
//            {
//                case SqlDbType.VarChar: return NpgsqlDbType.Varchar;
//                case SqlDbType.Char: return NpgsqlDbType.Char;
//                case SqlDbType.Bit: return NpgsqlDbType.Bit;
//                case SqlDbType.DateTime: return NpgsqlDbType.Timestamp;
//                case SqlDbType.Float: return NpgsqlDbType.Double;
//                case SqlDbType.Int: return NpgsqlDbType.Integer;
//                case SqlDbType.SmallInt: return NpgsqlDbType.Smallint;
//                case SqlDbType.Text: return NpgsqlDbType.Text;
//                case SqlDbType.UniqueIdentifier: return NpgsqlDbType.Uuid;
//            }
//            return NpgsqlDbType.Varchar;
//        }

//        ///<summary>
//        ///	Per ottenere un NpgsqlDbType da un generico TBType
//        ///</summary>
//        //---------------------------------------------------------------------------
//        internal static NpgsqlDbType GetPostgreDbType(TBType dbType)
//        {
//            //if (dbType.IsOracleType())
//            //    return GetPostgreDbType(dbType.OracleType);

//            if (dbType.IsSqlDbType())
//                return GetPostgreDbType(dbType.SqlDbType);

//            if (dbType.IsPostgreType())
//                return dbType.PostgreType;

//            return NpgsqlDbType.Varchar;
//        }
//        #endregion


//        //---------------------------------------------------------------------------		
//        public static bool IsCharType(TBType dbType, DBMSType dbmsType)
//        {
//            switch (dbmsType)
//            {
//                case DBMSType.SQLSERVER:
//                    {
//                        SqlDbType sqlType = dbType.SqlDbType;
//                        return (sqlType == SqlDbType.NChar || sqlType == SqlDbType.NVarChar ||
//                                sqlType == SqlDbType.VarBinary || sqlType == SqlDbType.VarChar ||
//                                sqlType == SqlDbType.Char || sqlType == SqlDbType.Text);
//                    }
//                //case DBMSType.ORACLE:
//                //    {
//                //        OracleType oracleType = dbType.OracleType;
//                //        return (oracleType == OracleType.NChar || oracleType == OracleType.NVarChar ||
//                //                oracleType == OracleType.LongVarChar || oracleType == OracleType.VarChar ||
//                //                oracleType == OracleType.Char || oracleType == OracleType.Clob);
//                //    }

//                case DBMSType.POSTGRE:
//                    {
//                        NpgsqlDbType npgType = dbType.PostgreType;
//                        return (npgType == NpgsqlDbType.Char || npgType == NpgsqlDbType.Varchar ||
//                               npgType == NpgsqlDbType.Text);
//                    }
//                default:
//                    return false;
//            }
//        }

//        //---------------------------------------------------------------------------		
//        public static bool HasLength(TBType dbType, DBMSType dbmsType)
//        {
//            if (!TBDatabaseType.IsCharType(dbType, dbmsType))
//                return false;

//            switch (dbmsType)
//            {
//                case DBMSType.SQLSERVER:
//                    return (dbType.SqlDbType != SqlDbType.Text);
//                //case DBMSType.ORACLE:
//                //    return (dbType.OracleType != OracleType.Clob);
//                case DBMSType.POSTGRE:
//                    return (dbType.PostgreType != NpgsqlDbType.Text);
//                default:
//                    return false;
//            }
//        }

//        //---------------------------------------------------------------------------		
//        public static bool HasPrecision(TBType dbType, DBMSType dbmsType)
//        {
//            switch (dbmsType)
//            {
//                case DBMSType.SQLSERVER:
//                    return (dbType.SqlDbType == SqlDbType.Decimal);
//                //case DBMSType.ORACLE:
//                //    return (dbType.OracleType == OracleType.Number || dbType.OracleType == OracleType.Float);
//                case DBMSType.POSTGRE:
//                    return (dbType.PostgreType == NpgsqlDbType.Numeric || dbType.PostgreType == NpgsqlDbType.Double);
//                default:
//                    return false;
//            }
//        }

//        //---------------------------------------------------------------------------		
//        public static bool HasScale(TBType dbType, DBMSType dbmsType)
//        {
//            switch (dbmsType)
//            {
//                case DBMSType.SQLSERVER:
//                    return (dbType.SqlDbType == SqlDbType.Decimal);
//                //case DBMSType.ORACLE:
//                //    return (dbType.OracleType == OracleType.Number);
//                case DBMSType.POSTGRE:
//                    return (dbType.PostgreType == NpgsqlDbType.Numeric);
//                default:
//                    return false;
//            }
//        }

//        //---------------------------------------------------------------------------		
//        public static bool IsNumericType(TBType dbType, DBMSType dbmsType)
//        {
//            switch (dbmsType)
//            {
//                case DBMSType.SQLSERVER:
//                    return (dbType.SqlDbType == SqlDbType.SmallInt ||
//                            dbType.SqlDbType == SqlDbType.Int ||
//                            dbType.SqlDbType == SqlDbType.Float ||
//                            dbType.SqlDbType == SqlDbType.Decimal);
//                //case DBMSType.ORACLE:
//                //    return (dbType.OracleType == OracleType.Number || dbType.OracleType == OracleType.Float);
//                case DBMSType.POSTGRE:
//                    return (dbType.PostgreType == NpgsqlDbType.Smallint ||
//                            dbType.PostgreType == NpgsqlDbType.Integer ||
//                            dbType.PostgreType == NpgsqlDbType.Double ||
//                            dbType.PostgreType == NpgsqlDbType.Numeric);
//                default:
//                    return false;
//            }
//        }

//        //---------------------------------------------------------------------------		
//        public static bool IsDateTimeType(TBType dbType, DBMSType dbmsType)
//        {
//            switch (dbmsType)
//            {
//                case DBMSType.SQLSERVER:
//                    return (dbType.SqlDbType == SqlDbType.SmallDateTime ||
//                            dbType.SqlDbType == SqlDbType.DateTime);
//                //case DBMSType.ORACLE:
//                //    return (dbType.OracleType == OracleType.DateTime ||
//                //            dbType.OracleType == OracleType.Timestamp ||
//                //            dbType.OracleType == OracleType.TimestampLocal ||
//                //            dbType.OracleType == OracleType.TimestampWithTZ);
//                case DBMSType.POSTGRE:
//                    return (dbType.PostgreType == NpgsqlDbType.Date ||
//                            dbType.PostgreType == NpgsqlDbType.Timestamp ||
//                            dbType.PostgreType == NpgsqlDbType.TimestampTZ);
//                default:
//                    return false;
//            }
//        }

//        //---------------------------------------------------------------------------		
//        public static bool IsGUIDType(TBType dbType, DBMSType dbmsType, int columnSize)
//        {
//            switch (dbmsType)
//            {
//                case DBMSType.SQLSERVER:
//                    return (dbType.SqlDbType == SqlDbType.UniqueIdentifier);
//                //case DBMSType.ORACLE:
//                //    return (dbType.OracleType == OracleType.VarChar && columnSize == 38);
//                case DBMSType.POSTGRE:
//                    return (dbType.PostgreType == NpgsqlDbType.Uuid);
//                default:
//                    return false;
//            }
//        }

//        //-----------------------------------------------------------------------------
//        //public static string DBNativeConvert(object o, bool useUnicode, DBMSType dbType)
//        //{
//        //    string strOracleDateTs = "TO_DATE('{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}', 'YYYY-MM-DD HH24:MI:SS'";
//        //    string strSqlServerDateTs = "{{ts '{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}'}}";
//        //    string strPostgreDateTs = "to_timestamp('{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}', 'YYYY-MM-DD HH24:MI:SS'";

//        //    // usato in Expression di woorm
//        //    switch (o.GetType().Name)
//        //    {
//        //        case "Boolean":
//        //            {
//        //                return ObjectHelper.CastToDBBool((bool)o);
//        //            }
//        //        case "Byte":
//        //        case "Int16":
//        //        case "Int32":
//        //        case "Int64":
//        //        case "Decimal":
//        //        case "Single":
//        //        case "Double":
//        //            {
//        //                return o.ToString();
//        //            }
//        //        case "DataEnum":
//        //            {
//        //                return ObjectHelper.CastToDBData(o).ToString();
//        //            }
//        //        case "String":
//        //            {
//        //                string str = o.ToString().Replace("'", "''");

//        //                if (dbType == DBMSType.ORACLE && str.Length == 0)
//        //                    str.Insert(0, " ");  //problema stringa vuota di oracle

//        //                return useUnicode ? String.Format("N'{0}'", str) : String.Format("'{0}'", str);
//        //            }
//        //        case "Guid": // @@Anastasia: non so 
//        //            {
//        //                return String.Format("'{0}'", o.ToString());
//        //            }
//        //        case "DateTime":
//        //            {
//        //                //Timestamp format: {ts '2001-01-15 00:00:00'}
//        //                DateTime dt = (DateTime)o;
//        //                string sDateConvert = string.Empty;
//        //                switch (dbType)
//        //                {
//        //                    case DBMSType.SQLSERVER: sDateConvert = strSqlServerDateTs; break;
//        //                    case DBMSType.ORACLE: sDateConvert = strOracleDateTs; break;
//        //                    case DBMSType.POSTGRE: sDateConvert = strPostgreDateTs; break;
//        //                }
//        //                return String.Format(sDateConvert, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
//        //            }
//        //        default:
//        //            Debug.Fail(CoreTypeStrings.IllegalDataType);
//        //            break;
//        //    }
//        //    return String.Empty;
//        //}

//        /// <summary>
//        /// ReplaceParamSymbol
//        /// Data una stringa (o di comando o un parametro) contenente uno o + '@', utilizzata da SQL Server
//        /// come simbolo per identificare i parametri, viene effettuata la sostituzione di:
//        /// - '@' con ':' che è il simbolo dei parametri in Oracle
//        /// </summary>
//        /// <param name="toReplace">stringa in cui effettuare la sostituzione dei caratteri</param>
//        /// <param name="dbmsType">DBMSType da applicare</param>
//        /// <returns>stringa corretta</returns>
//        //---------------------------------------------------------------------------
//        public static string ReplaceParamSymbol(string stringToReplace, DBMSType dbmsType)
//        {
//            string validString = string.Empty;

//            switch (dbmsType)
//            {
//                case DBMSType.ORACLE:
//                    validString = stringToReplace.Replace('@', ':');
//                    break;
//                case DBMSType.POSTGRE:
//                case DBMSType.SQLSERVER:
//                default:
//                    validString = stringToReplace;
//                    break;
//            }

//            return validString;
//        }
//        #endregion
//    }
//    #endregion

//    #region Class TBCheckDatabase
//    //============================================================================
//    public class TBCheckDatabase
//    {
//        #region GetDatabaseVersion methods
//        /// <summary>
//        /// GetDatabaseVersion (istanzia al volo una TBConnection)
//        /// </summary>
//        /// <param name="companyDBConnectionString">stringa di connessione al db</param>
//        /// <param name="providerName">nome del provider</param>
//        /// <returns>enum DatabaseVersion</returns>
//        //-----------------------------------------------------------------------
//        public static DatabaseVersion GetDatabaseVersion(string companyDBConnectionString, string providerName)
//        {
//            DatabaseVersion ret = DatabaseVersion.Undefined;

//            if (string.IsNullOrWhiteSpace(companyDBConnectionString))
//                return ret;

//            TBConnection compDBConn = null;

//            try
//            {
//                compDBConn = new TBConnection(companyDBConnectionString, TBDatabaseType.GetDBMSType(providerName));
//                compDBConn.Open();

//                ret = GetDatabaseVersion(compDBConn);
//            }
//            catch
//            {
//                return DatabaseVersion.Undefined;
//            }
//            finally
//            {
//                if (compDBConn != null && compDBConn.State != ConnectionState.Closed)
//                {
//                    compDBConn.Close();
//                    compDBConn.Dispose();
//                }
//            }

//            return ret;
//        }

//        /// <summary>
//        /// GetDatabaseVersion
//        /// </summary>
//        /// <param name="compDBConn">connessione aperta sul db</param>
//        /// <returns>enum DatabaseVersion</returns>
//        //-----------------------------------------------------------------------
//        public static DatabaseVersion GetDatabaseVersion(TBConnection compDBConn)
//        {
//            // per Oracle non eseguo alcuna query
//            //if (compDBConn.IsOracleConnection())
//            //    return DatabaseVersion.Oracle;

//            // per Postgre non eseguo alcuna query
//            if (compDBConn.IsPostgreConnection())
//                return DatabaseVersion.Postgre;

//            // per SqlServer eseguo la query 'SELECT @@version', visto che devo discriminare tra 4 versioni diverse
//            if (compDBConn.IsSqlConnection())
//            {
//                SQLServerEdition currEdition = GetSQLServerEdition(compDBConn.SqlConnect);

//                if (currEdition == SQLServerEdition.MSDE2000 ||
//                    currEdition == SQLServerEdition.SqlExpress2005 ||
//                    currEdition == SQLServerEdition.SqlExpress2008 ||
//                    currEdition == SQLServerEdition.SqlExpress2012 ||
//                    currEdition == SQLServerEdition.SqlExpress2014 ||
//                    currEdition == SQLServerEdition.SqlExpress2016)
//                    return DatabaseVersion.MSDE;

//                if (currEdition == SQLServerEdition.SqlServer2000 ||
//                    currEdition == SQLServerEdition.SqlServer2005 ||
//                    currEdition == SQLServerEdition.SqlServer2008 ||
//                    currEdition == SQLServerEdition.SqlServer2012 ||
//                    currEdition == SQLServerEdition.SqlServer2014 ||
//                    currEdition == SQLServerEdition.SqlServer2016)
//                    return DatabaseVersion.SqlServer2000;
//            }

//            return DatabaseVersion.Undefined;
//        }
//        #endregion

//        #region GetSQLServerEdition
//        /// <summary>
//        /// GetSQLServerEdition - solo per SQL!!!
//        /// Data una connessione aperta su un database di sql ritorna l'edizione del server stesso
//        /// - SQL 2000				(Edition: Personal Edition - ProductVersion: 8.00.x)
//        /// - MSDE 2000				(Edition: Desktop Engine - ProductVersion: 8.00.x)
//        /// - SQL 2005				(Edition: Standard Edition - ProductVersion: 9.00.x)
//        /// - SQL 2005 Express Ed.	(Edition: Express Edition - ProductVersion: 9.00.x)
//        /// - SQL 2008				(Edition: Standard Edition - ProductVersion: 10.x) version R2: 10.5
//        /// - SQL 2008 Express Ed.	(Edition: Express Edition (with Advanced Services) - ProductVersion: 10.x) version R2: 10.5
//        /// - SQL 2012				(Edition: Standard Edition - ProductVersion: 11.)
//        /// - SQL 2012 Express Ed.	(Edition: Express Edition - ProductVersion: 11.)
//        /// - SQL 2014				(Edition: Standard Edition - ProductVersion: 12.)
//        /// - SQL 2014 Express Ed.	(Edition: Express Edition - ProductVersion: 12.)
//        /// - SQL 2016				(Edition: Standard Edition - ProductVersion: 13.)
//        /// - SQL 2016 Express Ed.	(Edition: Express Edition - ProductVersion: 13.)
//        /// <param name="dbConnection">connessione aperta sul db</param>
//        /// <returns>enum SQLServerEdition</returns>
//        /// </summary>
//        //-----------------------------------------------------------------------
//        public static SQLServerEdition GetSQLServerEdition(SqlConnection dbConnection)
//        {
//            if (dbConnection == null || dbConnection.State != ConnectionState.Open)
//                return SQLServerEdition.Undefined;

//            SqlCommand command = null;
//            SqlDataReader reader = null;

//            string version = string.Empty, edition = string.Empty;
//            string query = "SELECT SERVERPROPERTY(N'Edition') AS Edition, SERVERPROPERTY(N'ProductVersion') AS Version";

//            try
//            {
//                command = new SqlCommand(query, dbConnection);
//                reader = command.ExecuteReader();

//                if (reader.Read())
//                {
//                    edition = (string)reader["Edition"];
//                    version = (string)reader["Version"];
//                }
//            }
//            catch
//            {
//                return SQLServerEdition.Undefined;
//            }
//            finally
//            {
//                if (reader != null && !reader.IsClosed)
//                    reader.Dispose();
//                command.Dispose();
//            }

//            // si tratta della versione SQL Server 2000
//            if (version.StartsWith("8.00"))
//            {
//                if (edition.IndexOf("Desktop Engine", StringComparison.OrdinalIgnoreCase) >= 0)
//                    return SQLServerEdition.MSDE2000;

//                return SQLServerEdition.SqlServer2000;
//            }

//            // si tratta della versione SQL Server 2005
//            if (version.StartsWith("9.00"))
//            {
//                if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
//                    return SQLServerEdition.SqlExpress2005;

//                return SQLServerEdition.SqlServer2005;
//            }

//            // si tratta della versione SQL Server 2008 (o R2)
//            if (version.StartsWith("10."))
//            {
//                // eseguo la IndexOf perche' potrebbe essere Express Edition with Advanced Services
//                if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
//                    return SQLServerEdition.SqlExpress2008;

//                return SQLServerEdition.SqlServer2008;
//            }

//            // si tratta della versione SQL Server 2012
//            if (version.StartsWith("11."))
//            {
//                // eseguo la IndexOf perche' potrebbe essere Express Edition with Advanced Services
//                if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
//                    return SQLServerEdition.SqlExpress2012;

//                return SQLServerEdition.SqlServer2012;
//            }

//            // si tratta della versione SQL Server 2014
//            if (version.StartsWith("12."))
//            {
//                if (edition.IndexOf("SQL Azure", StringComparison.OrdinalIgnoreCase) >= 0)
//                    return SQLServerEdition.SqlAzureV12;

//                // eseguo la IndexOf perche' potrebbe essere Express Edition with Advanced Services
//                if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
//                    return SQLServerEdition.SqlExpress2014;

//                return SQLServerEdition.SqlServer2014;
//            }

//            // si tratta della versione SQL Server 2016
//            if (version.StartsWith("13."))
//            {
//                // eseguo la IndexOf perche' potrebbe essere Express Edition with Advanced Services
//                if (edition.IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) >= 0)
//                    return SQLServerEdition.SqlExpress2016;

//                return SQLServerEdition.SqlServer2016;
//            }

//            return SQLServerEdition.Undefined;
//        }

//        //-----------------------------------------------------------------------
//        /// <summary>
//        /// torna true se la connessione passata è ad un sqlserver 2012 o successivi
//        /// </summary>
//        public static bool IsSql2012Edition(TBConnection tbConnection)
//        {
//            if (!tbConnection.IsSqlConnection())
//                return false;

//            return IsSql2012Edition(tbConnection.SqlConnect);
//        }

//        //-----------------------------------------------------------------------
//        /// <summary>
//        /// torna true se la connessione passata è ad un sqlserver 2012 o successivi
//        /// </summary>
//        public static bool IsSql2012Edition(SqlConnection sqlConnection)
//        {
//            if (sqlConnection == null) return false;

//            SQLServerEdition ed = GetSQLServerEdition(sqlConnection);
//            return (ed == SQLServerEdition.SqlServer2012 || ed == SQLServerEdition.SqlServer2014 || ed == SQLServerEdition.SqlServer2016);
//        }
//        #endregion

//        #region GetDatabaseCollation (SQL Server)
//        /// <summary>
//        /// GetDatabaseCollation
//        /// Esegue le query per ottenere l'impostazione corrente della proprietà Collation del database specificato
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns>COLLATION di database</returns>
//        //-----------------------------------------------------------------------
//        public static string GetDatabaseCollation(TBConnection tbConn)
//        {
//            string dbCollate = string.Empty;

//            // in Oracle non esiste il concetto di collation, pertanto ritorniamo stringa vuota
//            //if (tbConn.IsOracleConnection())
//            //    return dbCollate;

//            //Per adesso qui non metto niente
//            if (tbConn.IsPostgreConnection())
//                return dbCollate;

//            // SQL Server: esegue la query DATABASEPROPERTYEX
//            if (tbConn.IsSqlConnection())
//            {
//                SqlCommand command = null;
//                SqlDataReader reader = null;

//                string query =
//                    string.Format("SELECT DATABASEPROPERTYEX (N'{0}', N'Collation') AS DBCOLLATE", tbConn.Database);

//                try
//                {
//                    command = new SqlCommand(query, tbConn.SqlConnect);
//                    reader = command.ExecuteReader();

//                    if (reader.Read())
//                        dbCollate = reader["DBCOLLATE"].ToString();
//                }
//                catch (SqlException)
//                {
//                }
//                finally
//                {
//                    if (reader != null && !reader.IsClosed)
//                        reader.Dispose();
//                    command.Dispose();
//                }
//            }

//            return dbCollate;
//        }
//        #endregion

//        #region GetServerCollation (SQL Server)
//        /// <summary>
//        /// GetServerCollation
//        /// Esegue la query che restituisce informazioni relative alla proprietà Collation dell'istanza del server.
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns>COLLATION associata al server</returns>
//        //-----------------------------------------------------------------------
//        public static string GetServerCollation(TBConnection tbConn)
//        {
//            string serverCollate = string.Empty;

//            // in Oracle non esiste il concetto di collation, pertanto ritorniamo stringa vuota
//            //if (tbConn.IsOracleConnection())
//            //    return serverCollate;

//            if (tbConn.IsPostgreConnection())
//                return serverCollate;

//            // SQL Server: esegue la query SELECT SERVERPROPERTYEX
//            if (tbConn.IsSqlConnection())
//            {
//                SqlCommand command = null;
//                SqlDataReader reader = null;

//                try
//                {
//                    command = new SqlCommand("SELECT SERVERPROPERTY (N'Collation') AS SERVERCOLLATE", tbConn.SqlConnect);
//                    reader = command.ExecuteReader();

//                    if (reader.Read())
//                        serverCollate = reader["SERVERCOLLATE"].ToString();
//                }
//                catch (SqlException)
//                {
//                }
//                finally
//                {
//                    if (reader != null && !reader.IsClosed)
//                        reader.Dispose();
//                    command.Dispose();
//                }
//            }

//            return serverCollate;
//        }
//        #endregion

//        #region GetColumnCollation (SQL Server)
//        /// <summary>
//        /// GetColumnCollation
//        /// Esegue una query sulle view di sistema di SQL per restituire la Collation imputata alla
//        /// colonna Status della tabella TB_DBMark
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns>COLLATION di colonna</returns>
//        //-----------------------------------------------------------------------
//        public static string GetColumnCollation(TBConnection tbConn)
//        {
//            return GetColumnCollation(tbConn, "TB_DBMark", "Status");
//        }

//        /// <summary>
//        /// GetColumnCollation
//        /// Esegue una query sulle view di sistema di SQL per restituire la Collation imputata alla
//        /// colonna passata come parametro
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <param name="tableName">nome della tabella a cui appartiene la colonna di cui si vuole sapere la collation</param>
//        /// <param name="columnName">nome della colonna di cui si vuole sapere la collation</param>
//        /// <returns>COLLATION di colonna</returns>
//        //-----------------------------------------------------------------------
//        public static string GetColumnCollation(TBConnection tbConn, string tableName, string columnName)
//        {
//            string columnCollate = string.Empty;

//            // in Oracle non esiste il concetto di collation, pertanto ritorniamo stringa vuota
//            //if (tbConn.IsOracleConnection())
//            //    return columnCollate;

//            if (tbConn.IsPostgreConnection())
//                return columnCollate;

//            // prima controllo l'esistenza della tabella passata come parametro
//            TBDatabaseSchema schema = new TBDatabaseSchema(tbConn);
//            if (!schema.ExistTable(tableName))
//                return columnCollate;

//            // SQLServer: legge dalla view di sistema INFORMATION_SCHEMA.COLUMNS
//            if (tbConn.IsSqlConnection())
//            {
//                SqlCommand command = null;
//                SqlDataReader reader = null;

//                string query = string.Format
//                    (
//                        @"SELECT COLLATION_NAME FROM [{0}].INFORMATION_SCHEMA.COLUMNS
//						WHERE TABLE_NAME = N'{1}' AND COLUMN_NAME = N'{2}'",
//                        tbConn.Database,
//                        tableName,
//                        columnName
//                    );

//                try
//                {
//                    command = new SqlCommand(query, tbConn.SqlConnect);
//                    reader = command.ExecuteReader();

//                    if (reader.Read())
//                        columnCollate = reader["COLLATION_NAME"].ToString();
//                }
//                catch (SqlException)
//                {
//                }
//                finally
//                {
//                    if (reader != null && !reader.IsClosed)
//                        reader.Dispose();
//                    command.Dispose();
//                }
//            }

//            return columnCollate;
//        }
//        #endregion

//        #region GetValidCollationPropertyForDB (SQL Server)
//        /// <summary>
//        /// GetValidCollationPropertyForDB
//        /// Metodo che in scaletta cerca di ritornare un valido valore di COLLATION:
//        /// 1. legge la Collation della colonna AddOnModule della tabella TB_DBMark (se esiste)
//        /// 2. legge quella del database, se vuota...
//        /// 3. legge quella del server.
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns>COLLATION valida</returns>
//        //-----------------------------------------------------------------------
//        public static string GetValidCollationPropertyForDB(TBConnection tbConn)
//        {
//            string collation = string.Empty;

//            //if (tbConn.IsOracleConnection())
//            //    return collation;

//            if (tbConn.IsPostgreConnection())
//                return collation;

//            collation = GetColumnCollation(tbConn);

//            if (collation.Length <= 0)
//                collation = GetDatabaseCollation(tbConn);

//            if (collation.Length <= 0)
//                collation = GetServerCollation(tbConn);

//            return collation;
//        }
//        #endregion

//        #region GetSQLServerIsInMixedMode (SQL Server)
//        /// <summary>
//        /// GetSQLServerIsInMixedMode
//        /// Esegue la query che restituisce informazioni relative alla proprietà IsIntegratedSecurityOnly dell'istanza del server.
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns>true: server in Mixed Mode; false: server solo in Win Auth</returns>
//        //-----------------------------------------------------------------------
//        public static bool GetSQLServerIsInMixedMode(TBConnection tbConn)
//        {
//            bool isMixedMode = false;

//            // in Oracle non esiste il concetto di collation, pertanto ritorniamo stringa vuota
//            //if (tbConn.IsOracleConnection())
//            //    return isMixedMode;
//            if (tbConn.IsPostgreConnection())
//                return isMixedMode;

//            if (tbConn.IsSqlConnection() && tbConn.State == ConnectionState.Open)
//            {
//                SqlCommand command = null;
//                try
//                {
//                    command = new SqlCommand("SELECT SERVERPROPERTY(N'IsIntegratedSecurityOnly') AS IsIntegratedSecurity", tbConn.SqlConnect);
//                    using (SqlDataReader reader = command.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        {
//                            string isMixed = reader["IsIntegratedSecurity"].ToString();
//                            isMixedMode = string.Compare(isMixed, "0", StringComparison.OrdinalIgnoreCase) == 0;
//                        }
//                    }
//                }
//                catch (SqlException)
//                {
//                }
//                finally
//                {
//                    if (command != null)
//                        command.Dispose();
//                }
//            }

//            return isMixedMode;
//        }
//        #endregion

//        #region IsSupportedColumnCollationForOracle
//        /// <summary>
//        /// IsSupportedColumnCollationForOracle
//        /// Serve per capire se il database Oracle è stato creato con la differenziazione dei tipi (VARCHAR2 e NVARCHAR2)
//        /// Vengono controllati i tipi delle colonne Application e Status nella tabella TB_DBMark.
//        /// Se sono uguali allora risultano compatibili, altrimenti no.
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns></returns>
//        //-----------------------------------------------------------------------
//        //public static bool IsSupportedColumnCollationForOracle(TBConnection tbConn)
//        //{
//        //    if (!tbConn.IsOracleConnection())
//        //        return false;

//        //    // istanzio TBDatabaseSchema sulla connessione
//        //    TBDatabaseSchema mySchema = new TBDatabaseSchema(tbConn);
//        //    if (!mySchema.ExistTable(DBSchemaStrings.TBDBMark))
//        //        return false;

//        //    // analizzo lo schema della tabella e verifico il tipo delle colonne Application e Status
//        //    DataTable cols = mySchema.GetTableSchema(DBSchemaStrings.TBDBMark, false);

//        //    string colStatusType = string.Empty;
//        //    string colApplicationType = string.Empty;
//        //    TBType providerType = null;

//        //    foreach (var col in cols)
//        //    {
//        //        if (string.Compare(col["ColumnName"].ToString(), "Application", true, CultureInfo.InvariantCulture) == 0)
//        //        {
//        //            providerType = new TBType(((OracleType)col["ProviderType"]));
//        //            colApplicationType = (TBDatabaseType.GetDBDataType(providerType, DBMSType.ORACLE)); //col["DataTypeName"].ToString(); 
//        //            continue;
//        //        }
//        //        if (string.Compare(col["ColumnName"].ToString(), "Status", true, CultureInfo.InvariantCulture) == 0)
//        //        {
//        //            providerType = new TBType(((OracleType)col["ProviderType"]));
//        //            colStatusType = (TBDatabaseType.GetDBDataType(providerType, DBMSType.ORACLE));  //col["DataTypeName"].ToString();  
//        //            continue;
//        //        }
//        //    }

//        //    return
//        //        string.Compare(colApplicationType, colStatusType, true, CultureInfo.InvariantCulture) != 0;
//        //}
//        #endregion

//        #region IsSupportedLanguageForFullTextSearch
//        /// <summary>
//        /// IsSupportedLanguageForFullTextSearch - controlla che l'lcid passato come parametro sia presente
//        /// nella tabella delle languages previste per il Full Text Search
//        /// </summary>
//        /// <param name="connection">connessione aperta sul db</param>
//        /// <param name="lcid">lcid del language da ricercare</param>
//        /// <returns>enum DatabaseCheckError</returns>
//        //-----------------------------------------------------------------------
//        public static bool IsSupportedLanguageForFullTextSearch(TBConnection connection, int lcid)
//        {
//            if (!connection.IsSqlConnection() || (lcid <= 0))
//                return false;

//            string selectQuery = string.Format("SELECT COUNT(*) FROM sys.fulltext_languages WHERE lcid = '{0}'", lcid.ToString());

//            TBCommand command = null;

//            try
//            {
//                command = new TBCommand(selectQuery, connection);

//                if (command.ExecuteTBScalar() <= 0)
//                    return false;
//            }
//            catch (TBException)
//            {
//                return false;
//            }
//            finally
//            {
//                command.Dispose();
//            }

//            return true;
//        }
//        #endregion

//        #region GetDBPercentageUsedSize, GetDBSizeInKByte, IsDBSizeOverMaxLimit e overload
//        /// <summary>
//        /// GetDBPercentageUsedSize
//        /// Ritorna la percentuale di spazio utilizzato di un database con il limite dei 2GB
//        /// (da usare per le edizione Standard e Pro-Lite, con la DBNetworkType = Small)
//        /// </summary>
//        /// <param name="connectionStr">stringa di connessione al db</param>
//        /// <param name="dbmsType">DBMSType</param>
//        /// <returns>percentuale di spazio utilizzato</returns>
//        //----------------------------------------------------------------------
//        public static float GetDBPercentageUsedSize(string connectionStr, DBMSType dbmsType)
//        {
//            long maxDBSize = GetDBSizeInKByte(connectionStr, dbmsType);

//            return ((maxDBSize * 100) / DatabaseLayerConsts.MaxDBSize);
//        }

//        /// <summary>
//        /// IsDBSizeOverMaxLimit
//        /// Data una stringa si connette al volo al database ed effettua la somma delle dimensioni
//        /// dei file di dati del database. Il valore di ritorno indica se tale somma eccede o meno i 2GB
//        /// </summary>
//        /// <param name="connectionStr">stringa di connessione al db</param>
//        /// <param name="dbmsType">dbmstype per la connessione</param>
//        /// <returns>true: se la somma dei file di dati del db eccede i 2GB</returns>
//        //----------------------------------------------------------------------
//        public static bool IsDBSizeOverMaxLimit(string connectionStr, DBMSType dbmsType)
//        {
//            long maxDBSize = GetDBSizeInKByte(connectionStr, dbmsType);

//            if (maxDBSize > DatabaseLayerConsts.MaxDBSize || maxDBSize < 0)
//                return true;

//            return false;
//        }

//        /// <summary>
//        /// IsDBSizeOverMaxLimit
//        /// Data una stringa si connette al volo al database ed effettua la somma delle dimensioni
//        /// dei file di dati del database. Il valore di ritorno indica se tale somma eccede o meno i 2GB
//        /// </summary>
//        /// <param name="tbConnection">connessione aperta sul db</param>
//        /// <returns>true: se la somma dei file di dati del db eccede i 2GB</returns>
//        //----------------------------------------------------------------------
//        public static bool IsDBSizeOverMaxLimit(TBConnection tbConnection)
//        {
//            long maxDBSize = GetDBSizeInKByte(tbConnection);

//            if (maxDBSize > DatabaseLayerConsts.MaxDBSize || maxDBSize < 0)
//                return true;

//            return false;
//        }


//        //----------------------------------------------------------------------
//        /// <summary>
//        /// Verifica se la dimensione del db è vicina al massimo indicato nel serverconnection.config
//        /// se la size è compresa (estremi inclusi) tra la 'limit' (es: 1.95GB) e la max (2GB),  o se è minore di 0, per me è warning
//        /// </summary>
//        /// <param name="tbConnection"></param>
//        /// <returns></returns>
//        //public static bool IsDBSizeNearMaxLimit(TBConnection tbConnection, out string freePercentage)
//        //{
//        //    long DBSize = GetDBSizeInKByte(tbConnection);
//        //    double freePercentageD = Math.Round((((double)(DatabaseLayerConsts.MaxDBSize - DBSize) * 100) / DatabaseLayerConsts.MaxDBSize), 1);

//        //    freePercentage = freePercentageD.ToString();
//        //    return ((DBSize >= InstallationData.ServerConnectionInfo.MinDBSizeToWarn && DBSize <= DatabaseLayerConsts.MaxDBSize) || DBSize < 0);
//        //}


//        /// <summary>
//        /// GetDBSizeInKByte
//        /// Data una stringa si connette al volo al database ed effettua la somma delle dimensioni
//        /// dei file di dati del database. Il valore di ritorno ritorna tale somma.
//        /// </summary>
//        /// <param name="connectionStr">stringa di connessione per il db</param>
//        /// <param name="dbmsType">dbmstype per la connessione</param>
//        /// <returns>long size</returns>
//        //----------------------------------------------------------------------
//        public static long GetDBSizeInKByte(string connectionStr, DBMSType dbmsType)
//        {
//            long fullSize = 0;

//            // non effettuo controlli sulla dimensione di un db di tipo Oracle e postgre (non ha limite)
//            if (dbmsType == DBMSType.ORACLE || dbmsType == DBMSType.POSTGRE)
//                return fullSize;


//            TBConnection myConnection = new TBConnection(dbmsType);

//            try
//            {
//                myConnection.ConnectionString = connectionStr;
//                myConnection.Open();
//                fullSize = GetDBSizeInKByte(myConnection);
//            }
//            catch
//            {
//                return 0;
//            }
//            finally
//            {
//                if (myConnection != null && myConnection.State != ConnectionState.Closed)
//                {
//                    myConnection.Close();
//                    myConnection.Dispose();
//                }
//            }

//            return fullSize;
//        }

//        /// <summary>
//        /// GetDBSizeInKByte
//        /// Data una connessione aperta al database, effettua la somma delle dimensioni
//        /// dei file di dati del database. Il valore di ritorno ritorna tale somma.
//        /// </summary>
//        /// <param name="connection">connessione aperta sul db</param>
//        /// <returns>long size</returns>
//        //----------------------------------------------------------------------
//        public static long GetDBSizeInKByte(TBConnection connection)
//        {
//            long fullSize = 0;

//            // non effettuo controlli sulla dimensione di un db di Oracle s Postgre  (illimitato)
//            //if (connection.IsOracleConnection() || connection.IsPostgreConnection())
//            //    return fullSize;

//            TBCommand myCommand = new TBCommand(connection);
//            IDataReader reader = null;

//            try
//            {
//                myCommand.CommandText = "sp_helpfile";
//                myCommand.CommandType = CommandType.StoredProcedure;
//                reader = myCommand.ExecuteReader();

//                while (reader.Read())
//                {
//                    object filegroup = reader["filegroup"];
//                    if (filegroup == DBNull.Value)
//                        continue;

//                    string size = ((string)reader["size"]).Trim().Replace("KB", "");

//                    if (string.Compare(size, "Unlimited", true) == 0)
//                        return -1;

//                    fullSize += Int32.Parse(size);
//                }
//            }
//            catch (Exception exc)
//            {
//                Debug.Fail(exc.Message);
//                return -1;
//            }
//            finally
//            {
//                if (reader != null && !reader.IsClosed)
//                    reader.Close();
//            }

//            return fullSize;
//        }
//        #endregion

//        #region IsFullTextSearchInstalled (SQL Server only)
//        /// <summary>
//        /// IsFullTextSearchInstalled
//        /// Esegue la query per sapere se il componente per la FullTextSearch e' installato sul server SQL
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns>true se e' installato, altrimenti false</returns>
//        //-----------------------------------------------------------------------
//        public static bool IsFullTextSearchInstalled(TBConnection tbConn)
//        {
//            bool isInstalled = false;

//            // se non si tratta di una connessione a SQL non procedo
//            if (!tbConn.IsSqlConnection())
//                return isInstalled;

//            SqlCommand command = null;
//            SqlDataReader reader = null;

//            try
//            {
//                command = new SqlCommand("SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') as IsFullTextInstalled", tbConn.SqlConnect);
//                reader = command.ExecuteReader();

//                if (reader.Read())
//                    isInstalled = (string.Compare(reader["IsFullTextInstalled"].ToString(), "1", StringComparison.OrdinalIgnoreCase) == 0);
//            }
//            catch (SqlException)
//            {
//            }
//            finally
//            {
//                if (reader != null && !reader.IsClosed)
//                    reader.Dispose();
//                command.Dispose();
//            }

//            return isInstalled;
//        }
//        #endregion

//        #region IsFullTextSearchEnabled (SQL Server only)
//        /// <summary>
//        /// IsFullTextSearchEnabled
//        /// Esegue la query per sapere se il componente per la FullTextSearch e' abilitato sul database della connessione
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns>true se e' abilitato, altrimenti false</returns>
//        //-----------------------------------------------------------------------
//        public static bool IsFullTextSearchEnabled(TBConnection tbConn)
//        {
//            bool isEnabled = false;

//            // se non si tratta di una connessione a SQL non procedo
//            if (!tbConn.IsSqlConnection())
//                return isEnabled;

//            SQLServerEdition currEdition = GetSQLServerEdition(tbConn.SqlConnect);

//            // Per le versioni di SQL Server uguali o superiori al 2008 vale il link seguente e la nota :
//            // http://msdn.microsoft.com/it-it/library/ms186823.aspx : Il valore di questa proprietà non ha alcun effetto.
//            // I database utente sono sempre abilitati per la ricerca full-text. 
//            // Quindi se l'edizione di SQL e' diversa dal 2005 ritorno true.
//            if (currEdition != SQLServerEdition.SqlExpress2005 && currEdition != SQLServerEdition.SqlServer2005)
//                return true;

//            SqlCommand command = null;
//            SqlDataReader reader = null;

//            try
//            {
//                string query = string.Format("SELECT DATABASEPROPERTYEX('{0}', 'IsFullTextEnabled') as IsFullTextEnabled", tbConn.Database);

//                command = new SqlCommand(query, tbConn.SqlConnect);
//                reader = command.ExecuteReader();

//                if (reader.Read())
//                    isEnabled = (string.Compare(reader["IsFullTextEnabled"].ToString(), "1", StringComparison.OrdinalIgnoreCase) == 0);
//            }
//            catch (SqlException)
//            {
//            }
//            finally
//            {
//                if (reader != null && !reader.IsClosed)
//                    reader.Dispose();
//                command.Dispose();
//            }

//            return isEnabled;
//        }
//        #endregion

//        #region CheckFullTextSearchEnabled (SQL Server only)
//        /// <summary>
//        /// CheckFullTextSearchEnabled
//        /// Esegue la query per sapere se il componente per la FullTextSearch e' abilitato sul database della connessione
//        /// Nel caso fosse disabilitato prova ad abilitarlo
//        /// </summary>
//        /// <param name="tbConn">connessione aperta sul db</param>
//        /// <returns>true se e' abilitato, altrimenti false</returns>
//        //-----------------------------------------------------------------------
//        public static bool CheckFullTextSearchEnabled(TBConnection tbConn)
//        {
//            bool isEnabled = false;

//            // se non si tratta di una connessione a SQL non procedo
//            if (!tbConn.IsSqlConnection())
//                return isEnabled;

//            // se il componente risulta abilitato ritorno subito true
//            if (IsFullTextSearchEnabled(tbConn))
//                return true;

//            try
//            {
//                // provo ad abilitare il fulltext
//                SqlCommand command = new SqlCommand("EXEC sp_fulltext_database 'enable'", tbConn.SqlConnect);
//                command.ExecuteNonQuery();
//                command.Dispose();

//                // ri-controllo di essere riuscita effettivamente ad abilitarlo
//                isEnabled = IsFullTextSearchEnabled(tbConn);
//            }
//            catch (SqlException)
//            {
//            }

//            return isEnabled;
//        }
//        #endregion

//        #region CheckDatabaseVersion
//        /// <summary>
//        /// CheckDatabaseVersion (controlla se il database a cui siamo connessi è compatibile con la versione
//        /// di database prevista dall'installazione
//        /// </summary>
//        /// <param name="conn">connessione aperta sul db</param>
//        /// <param name="activationDB">tipo db previsto dall'installazione</param>
//        /// <returns>true: versione db valida, altrimenti false</returns>
//        //----------------------------------------------------------------------
//        public static DatabaseCheckError CheckDatabaseVersion(TBConnection conn, DBNetworkType dbNetworkType)
//        {
//            if (conn == null)
//                return DatabaseCheckError.NoDatabase;

//            string currentDB = string.Empty;

//            try
//            {
//                currentDB = GetDatabaseVersion(conn).ToString();
//            }
//            catch
//            {
//                return DatabaseCheckError.NoDatabase;
//            }

//            if (dbNetworkType == DBNetworkType.Large)
//                return DatabaseCheckError.NoError;

//            return DatabaseCheckError.NoError;
//        }
//        #endregion

//        #region CheckDBDevelopment
//        /// <summary>
//        /// CheckDBDevelopment - controlla che siano delle righe nella TB_DBMark
//        /// </summary>
//        /// <param name="conn">connessione aperta sul db</param>
//        /// <returns>enum DatabaseCheckError</returns>
//        //-----------------------------------------------------------------------
//        private static DatabaseCheckError CheckDBDevelopment(TBConnection conn)
//        {
//            TBCommand command = null;

//            try
//            {
//                command = new TBCommand("SELECT COUNT(*) FROM TB_DBMARK", conn);

//                if (command.ExecuteTBScalar() < 0)
//                    return DatabaseCheckError.NoTables;
//            }
//            catch (TBException)
//            {
//                return DatabaseCheckError.NoTables;
//            }
//            finally
//            {
//                command.Dispose();
//            }

//            return DatabaseCheckError.NoError;
//        }
//        #endregion

//        #region CheckDBRelease
//        /// <summary>
//        /// CheckDBRelease - controlla la corrispondenza tra i numeri di release presenti nella TB_DBMark
//        /// e numeri di release specificati nei DatabaseObjects.xml
//        /// </summary>
//        /// <param name="conn">connessione aperta sul db</param>
//        /// <param name="PathFinder">pathfinder</param>
//        /// <returns>enum DatabaseCheckError</returns>
//        //-----------------------------------------------------------------------
//        //private static DatabaseCheckError CheckDBRelease(TBConnection conn, PathFinder PathFinder)
//        //{
//        //    StringCollection applicationsList = new StringCollection();
//        //    // array di supporto per avere l'elenco totale delle AddOnApplications
//        //    // (finchè non cambia il pathfinder e vengono unificati gli ApplicationType)
//        //    StringCollection supportList = new StringCollection();
//        //    // prima guardo le AddOn di TaskBuilder
//        //    PathFinder.GetApplicationsList(ApplicationType.TaskBuilder, out supportList);
//        //    applicationsList = supportList;
//        //    // poi guardo le AddOn di TaskBuilderApplications
//        //    PathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);
//        //    for (int i = 0; i < supportList.Count; i++)
//        //        applicationsList.Add(supportList[i]);
//        //    // poi guardo i verticali realizzati con EasyStudio
//        //    // poi guardo le customizzazioni
//        //    PathFinder.GetApplicationsList(ApplicationType.Customization, out supportList);
//        //    for (int i = 0; i < supportList.Count; i++)
//        //        applicationsList.Add(supportList[i]);

//        //    string query =
//        //        "SELECT DBRelease, Status FROM TB_DBMark WHERE Application = @application AND AddOnModule = @module";

//        //    if (conn.IsOracleConnection())
//        //        query = "SELECT DBRELEASE, STATUS FROM TB_DBMARK WHERE APPLICATION = @application AND ADDONMODULE = @module";

//        //    TBCommand command = null;
//        //    IDataReader reader = null;

//        //    try
//        //    {
//        //        command = new TBCommand(query, conn);
//        //        command.Parameters.Add("@application", ((TBType)SqlDbType.NVarChar), 20);
//        //        command.Parameters.Add("@module", ((TBType)SqlDbType.NVarChar), 40);
//        //        command.Prepare();

//        //        ApplicationInfo appInfo = null;
//        //        bool status = false;

//        //        foreach (string appName in applicationsList)
//        //        {
//        //            appInfo = (ApplicationInfo)PathFinder.GetApplicationInfoByName(appName);
//        //            if (appInfo.Modules == null)
//        //                continue;

//        //            foreach (ModuleInfo modInfo in appInfo.Modules)
//        //            {
//        //                // se la signature e' vuota oppure il modulo e' per il DMS skippo
//        //                if (string.IsNullOrWhiteSpace(modInfo.DatabaseObjectsInfo.Signature) || modInfo.DatabaseObjectsInfo.Dms)
//        //                    continue;

//        //                status = false;

//        //                ((IDbDataParameter)command.Parameters["@application"]).Value = appInfo.ApplicationConfigInfo.DbSignature;
//        //                ((IDbDataParameter)command.Parameters["@module"]).Value = modInfo.DatabaseObjectsInfo.Signature;
//        //                using (reader = command.ExecuteReader())
//        //                {

//        //                    if (!conn.DataReaderHasRows(reader))
//        //                    {
//        //                        reader.Close();
//        //                        return DatabaseCheckError.InvalidModule;
//        //                    }

//        //                    if (reader.Read())
//        //                    {

//        //                        if (conn.IsSqlConnection())
//        //                        {
//        //                            modInfo.CurrentDBRelease = Convert.ToInt32(reader["DBRelease"]);
//        //                            status = Convert.ToBoolean(int.Parse(reader["Status"].ToString()));
//        //                        }
//        //                        if (conn.IsOracleConnection())
//        //                        {
//        //                            modInfo.CurrentDBRelease = Convert.ToInt32(reader["DBRELEASE"]);
//        //                            status = Convert.ToBoolean(int.Parse(reader["STATUS"].ToString()));
//        //                        }
//        //                    }
//        //                }

//        //                if (InvalidRelease(modInfo) || !status)
//        //                    return DatabaseCheckError.InvalidModule;
//        //            }
//        //        }
//        //    }
//        //    catch (TBException)
//        //    {
//        //        return DatabaseCheckError.NoTables;
//        //    }
//        //    finally
//        //    {
//        //        if (reader != null && !reader.IsClosed)
//        //        {
//        //            reader.Close();
//        //            reader.Dispose();
//        //        }
//        //        command.Dispose();
//        //    }

//        //    return DatabaseCheckError.NoError;
//        //}

//        ////-----------------------------------------------------------------------
//        //private static bool InvalidRelease(ModuleInfo modInfo)
//        //{
//        //    //se sono una customizzazione, potrei avere il database disallineato ma devo poter entrare comunque
//        //    return (modInfo.CurrentDBRelease == 0 || modInfo.CurrentDBRelease != modInfo.DatabaseObjectsInfo.Release);
//        //}
//        //#endregion

//        #region CheckDatabase
//        //-----------------------------------------------------------------------
//    //    public static DatabaseCheckError CheckDatabase
//    //        (TBConnection conn, DBNetworkType dbNetworkType, PathFinder PathFinder, bool isDevelopment)
//    //    {
//    //        if (
//    //            PathFinder == null ||
//    //            conn == null ||
//    //            conn.State != ConnectionState.Open ||
//    //            dbNetworkType == DBNetworkType.Undefined
//    //            )
//    //            return DatabaseCheckError.NoActivatedDatabase;

//    //        DatabaseCheckError eCheck = CheckDBRelease(conn, PathFinder);
//    //        //prima si distingueva fra sviluppo e non, ma generava confusione neglii utenti Microarea,
//    //        //quindi è stato allineato al comportamento di release
//    //        /*(isDevelopment) 
//				//? CheckDBDevelopment(conn) 
//				//: CheckDBRelease(conn, PathFinder);*/

//    //        if (eCheck != DatabaseCheckError.NoError)
//    //            return eCheck;

//    //        return CheckDatabaseVersion(conn, dbNetworkType);
//    //    }
//        #endregion
//    }
//    #endregion

//    #region Class TBConnection
//    /// <summary>
//    /// Classe per la gestione della connessione al database
//    /// </summary>
//    //============================================================================
//    public class TBConnection : IDbConnection
//    {
//        private IDbConnection dbConnect = null;
//        private string schemaOwner = string.Empty;
//        private string completedConnectionString = string.Empty;

//        #region Properties IDbConnection
//        /// <summary>
//        /// Gets or sets the string used to open a database.
//        /// </summary>
//        //---------------------------------------------------------------------
//        public string ConnectionString
//        {
//            get { return dbConnect.ConnectionString; }
//            set { dbConnect.ConnectionString = value; }
//        }

//        /// <summary>
//        /// Gets the current state of the connection
//        /// </summary>
//        //---------------------------------------------------------------------
//        public ConnectionState State { get { return dbConnect.State; } }

//        //---------------------------------------------------------------------
//        public int ConnectionTimeout
//        {
//            get
//            {
//                if (dbConnect is SqlConnection)
//                    return dbConnect.ConnectionTimeout;

//                return 0;
//            }
//        }

//        //---------------------------------------------------------------------
//        public string Database
//        {
//            get
//            {
//                if (dbConnect is SqlConnection)
//                    return ((SqlConnection)dbConnect).Database;

//                //if (dbConnect is OracleConnection && dbConnect.ConnectionString.Length > 0)
//                //{
//                //    int nPos = dbConnect.ConnectionString.IndexOf("User ID");
//                //    if (nPos >= 0)
//                //    {
//                //        int nStart = dbConnect.ConnectionString.IndexOf("'", nPos);
//                //        int nEnd = 0;
//                //        if (nStart >= 0)
//                //            nEnd = dbConnect.ConnectionString.IndexOf("'", nStart + 1);

//                //        if (nEnd >= 0)
//                //            return dbConnect.ConnectionString.Substring(nStart + 1, (nEnd - nStart - 1)).ToUpper(CultureInfo.InvariantCulture);
//                //    }
//                //}

//                if (dbConnect is NpgsqlConnection)
//                    return ((NpgsqlConnection)dbConnect).Database;

//                return string.Empty;
//            }
//        }
//        #endregion

//        #region Properties TBConnection
//        //---------------------------------------------------------------------
//        public string SchemaOwner { get { return schemaOwner; } }

//        /// <summary>
//        /// Get of IDbConnection member 
//        /// </summary>
//        //---------------------------------------------------------------------
//        public IDbConnection DbConnect { get { return dbConnect; } }

//        /// <summary>
//        /// Return cast of IDbConnection member to a SqlConnection
//        /// </summary>
//        //---------------------------------------------------------------------
//        internal SqlConnection SqlConnect
//        {
//            get
//            {
//                if (dbConnect is SqlConnection)
//                    return (SqlConnection)dbConnect;
//                throw (new TBException("TBConnection.SqlConnect: " + DatabaseLayerStrings.InvalidCast));
//            }
//        }

//        /// <summary>
//        /// Cast of IDbConnection member to a OracleConnection
//        /// </summary>
//        //---------------------------------------------------------------------
//        //internal OracleConnection OracleConnect
//        //{
//        //    get
//        //    {
//        //        if (dbConnect is OracleConnection)
//        //            return (OracleConnection)dbConnect;
//        //        throw (new TBException("TBConnection.OracleConnect: " + DatabaseLayerStrings.InvalidCast));
//        //    }
//        //}

//        /// <summary>
//        /// Cast of IDbConnection member to a OracleConnection
//        /// </summary>
//        //---------------------------------------------------------------------
//        internal NpgsqlConnection PostgreConnect
//        {
//            get
//            {
//                if (dbConnect is NpgsqlConnection)
//                    return (NpgsqlConnection)dbConnect;
//                throw (new TBException("TBConnection.OracleConnect: " + DatabaseLayerStrings.InvalidCast));
//            }
//        }

//        /// <summary>
//        /// Restituisce il nome del server/nome servizio utilizzato dalla connessione
//        /// </summary>
//        //---------------------------------------------------------------------
//        public string DataSource
//        {
//            get
//            {
//                if (dbConnect is SqlConnection)
//                    return ((SqlConnection)dbConnect).DataSource;

//                //if (dbConnect is OracleConnection)
//                //    return ((OracleConnection)dbConnect).DataSource;

//                if (dbConnect is NpgsqlConnection)
//                    return ((NpgsqlConnection)dbConnect).DataSource;

//                throw (new TBException("TBConnection.DataSource: " + DatabaseLayerStrings.InvalidCast));
//            }
//        }
//        #endregion

//        #region Constructors
//        //---------------------------------------------------------------------------
//        public TBConnection(DBMSType dbmsType)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                dbConnect = new SqlConnection();
//            //else if (dbmsType == DBMSType.ORACLE)
//            //    dbConnect = new OracleConnection();
//            else if (dbmsType == DBMSType.POSTGRE)
//                dbConnect = new NpgsqlConnection();
//            else
//                throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//        }

//        /// <summary>
//        /// TBConnection constructor
//        /// </summary>
//        /// <param name="idbConnect">connessione </param>
//        //---------------------------------------------------------------------------
//        public TBConnection(IDbConnection idbConnect)
//        {
//            dbConnect = idbConnect;
//        }

//        /// <summary>
//        /// costruttore
//        /// </summary>
//        /// <param name="connection">stringa di connessione</param>
//        //---------------------------------------------------------------------------
//        public TBConnection(string connectionString, DBMSType dbmsType)
//        {
//            // La proprietà ConnectionString di IDbConnection "taglia" l'nformazione di pwd
//            // (a meno che la stringa usata per connettersi contenga anche "persist security info=True")
//            // per sicurezza salvo la stringa completa in un data member privato
//            completedConnectionString = connectionString;

//            // verifico il tipo di provider controllando le differenze nella stringa di connessione passata
//            // SqlServer ha l'attibuto di InitialCatalog oppure Database in caso di Azure
//            if (dbmsType == DBMSType.SQLSERVER)
//            {
//                if (connectionString.IndexOf("Initial Catalog", 0) < 0 && connectionString.IndexOf("Database", 0) < 0)
//                    throw (new TBException("Error in connection string"));
//                else
//                    dbConnect = new SqlConnection(connectionString);
//            }
//            //else if (dbmsType == DBMSType.ORACLE)
//            //{
//            //    dbConnect = new OracleConnection(connectionString);
//            //    schemaOwner = this.Database;
//            //}
//            else if (dbmsType == DBMSType.POSTGRE)
//            {
//                if (connectionString.IndexOf("Database", 0) < 0)
//                    throw (new TBException("Error in connection string"));
//                else
//                    dbConnect = new NpgsqlConnection(connectionString);
//            }

//            else
//                throw (new TBException("TBConnection constructor: " + string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//        }

//        /// <summary>
//        /// Costruttore
//        /// </summary>
//        /// <param name="connection"> stringa di connessione</param>
//        /// <param name="dbmsType">tipi di dbms da utilizzare per la connessione</param>
//        /// <param name="owner">eventuale owner: utilizzato per la gestione dei sinonimi in oralce</param> 
//        //---------------------------------------------------------------------------
//        public TBConnection(string connectionString, DBMSType dbmsType, string owner)
//        {
//            // La proprietà ConnectionString di IDbConnection "taglia" l'nformazione di pwd
//            // (a meno che la stringa usata per connettersi contenga anche "persist security info=True")
//            // per sicurezza salvo la stringa completa in un data member privato
//            completedConnectionString = connectionString;

//            // verifico il tipo di provider controllando le differenze nella stringa di connessione passata
//            // SqlServer ha l'attibuto di InitialCatalog attributo non utilizzato da Oracle
//            if (dbmsType == DBMSType.SQLSERVER)
//            {
//                if (connectionString.IndexOf("Initial Catalog", 0) < 0)
//                    throw (new TBException("Error in connection string"));
//                else
//                    dbConnect = new SqlConnection(connectionString);
//            }
//            //else if (dbmsType == DBMSType.ORACLE)
//            //    dbConnect = new OracleConnection(connectionString);

//            else if (dbmsType == DBMSType.POSTGRE)
//            {
//                if (connectionString.IndexOf("Database", 0) < 0)
//                    throw (new TBException("Error in connection string"));
//                else
//                    dbConnect = new NpgsqlConnection(connectionString);
//            }
//            else

//                throw (new TBException("TBConnection constructor: " + string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));

//            schemaOwner = owner.ToUpper();
//        }
//        #endregion

//        #region OperatorCast
//        //------------------------------------------------------------------
//        //public static implicit operator TBConnection(SqlConnection connect)
//        //{
//        //    return new TBConnection(connect);
//        //}

//        //------------------------------------------------------------------
//        //public static implicit operator TBConnection(OracleConnection connect)
//        //{
//        //    return new TBConnection(connect);
//        //}
//        #endregion

//        #region Dispose
//        /// <summary>
//        /// Releases all resources used by the Component.
//        /// </summary>
//        //---------------------------------------------------------------------------
//        public void Dispose()
//        {
//            dbConnect.Dispose();
//        }
//        #endregion

//        #region Required methods of IDbConnection
//        /// <summary>
//        /// Opens a connection to a database with the property settings specified by the ConnectionString.
//        /// </summary>
//        //---------------------------------------------------------------------------
//        public void Open()
//        {
//            try
//            {
//                dbConnect.Open();
//            }
//            //catch (OracleException oraEx)
//            //{
//            //    // nel caso di una specifica eccezione di Oracle faccio il throw
//            //    throw (new TBException(oraEx.Message, oraEx.InnerException));
//            //}
//            catch (DbException exc)
//            {
//                if (this.IsSqlConnection())
//                    TryToAdjustConnectionString(exc);
//            }
//            catch (Exception ex)
//            {
//                throw (new TBException(ex.Message, ex.InnerException));
//            }
//        }

//        // solo per SQl Server: prova a riaprire la connessione senza usare il pool (qualora fosse utilizzato)
//        //---------------------------------------------------------------------------
//        private void TryToAdjustConnectionString(Exception exc)
//        {
//            string pattern = @"Pooling\s*=\s*true";
//            //se non ero in pooling, allora l'errore e' un altro, rimbalzo l'eccezione originaria
//            if (!Regex.IsMatch(dbConnect.ConnectionString, pattern, RegexOptions.IgnoreCase))
//                throw (new TBException(exc.Message, exc.InnerException));

//            dbConnect.ConnectionString = Regex.Replace(dbConnect.ConnectionString, pattern, "Pooling=false", RegexOptions.IgnoreCase);

//            Open();
//        }

//        /// <summary>
//        /// Closes the connection to the database. This is the preferred method of closing any open connection.
//        /// </summary>
//        //---------------------------------------------------------------------------
//        public void Close()
//        {
//            try
//            {
//                if (dbConnect.State == ConnectionState.Open || dbConnect.State == ConnectionState.Broken)
//                    dbConnect.Close();
//            }
//            catch
//            {
//                //RESUME NEXT ...
//            }
//        }

//        //---------------------------------------------------------------------------
//        public IDbTransaction BeginTransaction()
//        {
//            return dbConnect.BeginTransaction();
//        }

//        //---------------------------------------------------------------------------
//        public IDbTransaction BeginTransaction(System.Data.IsolationLevel level)
//        {
//            //if (IsOracleConnection() && level == IsolationLevel.ReadUncommitted)
//            //    return dbConnect.BeginTransaction(IsolationLevel.ReadCommitted);

//            //if (IsPostgreConnection() && level == IsolationLevel.ReadUncommitted)
//            //    return dbConnect.BeginTransaction(IsolationLevel.ReadUncommitted);
//            return dbConnect.BeginTransaction(level);
//        }

//        //---------------------------------------------------------------------------
//        public void ChangeDatabase(string dbName)
//        {
//            dbConnect.ChangeDatabase(dbName);
//        }

//        //---------------------------------------------------------------------------
//        public IDbCommand CreateCommand()
//        {
//            return dbConnect.CreateCommand();
//        }
//        #endregion

//        #region Methods of TBConnection
//        /// <summary>
//        /// TRUE if the connection is a SqlConnection
//        /// </summary>
//        //---------------------------------------------------------------------
//        public bool IsSqlConnection()
//        {
//            return (dbConnect is SqlConnection);
//        }

//        /// <summary>
//        /// TRUE if the connection is a OracleConnection
//        /// </summary>
//        //---------------------------------------------------------------------
//        //public bool IsOracleConnection()
//        //{
//        //    return (dbConnect is OracleConnection);
//        //}

//        /// <summary>
//        /// TRUE if the connection is a NpgsqlConnection
//        /// </summary>
//        //---------------------------------------------------------------------
//        public bool IsPostgreConnection()
//        {
//            return (dbConnect is NpgsqlConnection);
//        }


//        /// <summary>
//        /// Permette di sapere se il datareader passato come argomento ha estratto o meno delle righe 
//        /// (Nota: il metodo HasRows non è dell'interfaccia IDataReader ma viene implementato solo dalle classi 
//        /// da essa derivate)
//        /// </summary>
//        /// <param name="reader">IDataReader da controllare</param>
//        /// <returns>true se sono presenti delle righe false altriementi</returns>
//        //---------------------------------------------------------------------------
//        public bool DataReaderHasRows(IDataReader reader)
//        {
//            if (reader == null || reader.IsClosed)
//                return false;

//            if (IsSqlConnection())
//                return ((SqlDataReader)reader).HasRows;

//            //if (IsOracleConnection())
//            //    return ((OracleDataReader)reader).HasRows;

//            if (IsPostgreConnection())
//                return ((NpgsqlDataReader)reader).HasRows;

//            throw (new TBException("TBConnection.DataReaderHasRows: " + DatabaseLayerStrings.InvalidCast));
//        }

       
//        //---------------------------------------------------------------------------
//        public TBConnection GetCloneTBConnection()
//        {
//            // uso la completedConnectionString che a differenza della property ConnectionString ha al suo 
//            // interno le informazioni relative alla pwd
//            if (completedConnectionString.Length == 0)
//                return null;

//            string provider = string.Empty;
//            DBMSType dbmsType;
//            if (IsSqlConnection())
//                dbmsType = DBMSType.SQLSERVER;
//            //else if (IsOracleConnection())
//            //    dbmsType = DBMSType.ORACLE;
//            else if (IsPostgreConnection())
//                dbmsType = DBMSType.POSTGRE;
//            else
//                return null;

//            return new TBConnection(completedConnectionString, dbmsType);
//        }
//        #endregion
//    }
//    #endregion

//    #region Class TBCommand
//    /// <summary>
//    /// Classe per la gestione dei command
//    /// </summary>
//    //============================================================================
//    public class TBCommand : IDbCommand
//    {
//        private IDbCommand dbCommand = null;
//        private TBConnection tbConnection = null;

//        #region Properties required for IDbCommand
//        //---------------------------------------------------------------------
//        public string CommandText
//        {
//            get { return dbCommand.CommandText; }
//            set
//            {
//                if (tbConnection != null)
//                {
//                    //if (tbConnection.IsOracleConnection())
//                    //    dbCommand.CommandText = TBDatabaseType.ReplaceParamSymbol(value, DBMSType.ORACLE);
//                    //else
//                        dbCommand.CommandText = value;
//                }
//            }
//        }

//        //---------------------------------------------------------------------
//        IDbConnection IDbCommand.Connection { get { return dbCommand.Connection; } set { dbCommand.Connection = value; } }

//        //---------------------------------------------------------------------
//        public IDbTransaction Transaction { get { return dbCommand.Transaction; } set { dbCommand.Transaction = value; } }

//        //---------------------------------------------------------------------
//        public int CommandTimeout { get { return dbCommand.CommandTimeout; } set { dbCommand.CommandTimeout = value; } }

//        //---------------------------------------------------------------------
//        public CommandType CommandType { get { return dbCommand.CommandType; } set { dbCommand.CommandType = value; } }

//        //---------------------------------------------------------------------
//        IDataParameterCollection IDbCommand.Parameters { get { return dbCommand.Parameters; } }

//        //---------------------------------------------------------------------
//        public UpdateRowSource UpdatedRowSource { get { return dbCommand.UpdatedRowSource; } set { dbCommand.UpdatedRowSource = value; } }
//        #endregion

//        #region Properties of TBCommand
//        /// <summary>
//        /// Get of IDbCommand member 
//        /// </summary>
//        //---------------------------------------------------------------------
//        public IDbCommand DbCommand { get { return dbCommand; } }

//        //---------------------------------------------------------------------
//        public TBConnection Connection
//        {
//            get
//            {
//                return tbConnection;
//            }
//            set
//            {
//                dbCommand.Connection = ((TBConnection)value).DbConnect;
//                tbConnection = (TBConnection)value;
//            }
//        }

//        //---------------------------------------------------------------------
//        public TBParameterCollection Parameters
//        {
//            get
//            {
//                if (dbCommand is SqlCommand)
//                    return new TBParameterCollection(((SqlCommand)dbCommand).Parameters);

//                //if (dbCommand is OracleCommand)
//                //    return new TBParameterCollection(((OracleCommand)dbCommand).Parameters);

//                if (dbCommand is NpgsqlCommand)
//                    return new TBParameterCollection(((NpgsqlCommand)dbCommand).Parameters);

//                throw (new TBException("TBCommand.Parameters" + DatabaseLayerStrings.InvalidCast));
//            }
//        }

//        /// <summary>
//        /// Return cast of IDbCommand member to a SqlCommand
//        /// </summary>
//        //---------------------------------------------------------------------
//        internal SqlCommand SqlCmd
//        {
//            get
//            {
//                if (dbCommand is SqlCommand)
//                    return (SqlCommand)dbCommand;
//                throw (new TBException("TBCommand.SqlCmd" + DatabaseLayerStrings.InvalidCast));
//            }
//        }

//        /// <summary>
//        /// Return cast of IDbCommand member to a OracleCommand
//        /// </summary>
//        //---------------------------------------------------------------------
//        //internal OracleCommand OracleCmd
//        //{
//        //    get
//        //    {
//        //        if (dbCommand is OracleCommand)
//        //            return (OracleCommand)dbCommand;
//        //        throw (new TBException("TBCommand.OracleCmd" + DatabaseLayerStrings.InvalidCast));
//        //    }
//        //}

//        /// <summary>
//        /// Return cast of IDbCommand member to a NpgsqlCommand
//        /// </summary>
//        //---------------------------------------------------------------------
//        internal NpgsqlCommand NpgsqlCmd
//        {
//            get
//            {
//                if (dbCommand is NpgsqlCommand)
//                    return (NpgsqlCommand)dbCommand;
//                throw (new TBException("TBCommand.SqlCmd" + DatabaseLayerStrings.InvalidCast));
//            }
//        }
//        #endregion

//        #region Constructor
//        //---------------------------------------------------------------------------
//        public TBCommand(TBConnection connection)
//        {
//            tbConnection = connection;

//            if (connection.IsSqlConnection())
//            {
//                dbCommand = new SqlCommand();
//                dbCommand.Connection = connection.SqlConnect;
//            }
//            //else if (connection.IsOracleConnection())
//            //{
//            //    dbCommand = new OracleCommand();
//            //    dbCommand.Connection = connection.OracleConnect;
//            //}
//            else if (connection.IsPostgreConnection())
//            {
//                dbCommand = new NpgsqlCommand();
//                dbCommand.Connection = connection.PostgreConnect;
//            }
//            else
//                throw (new TBException("TBCommand.Constructor : " + DatabaseLayerStrings.UnknownConnection));
//        }

//        /// <summary>
//        /// constructor
//        /// </summary>
//        /// <param name="query">stringa del comando da eseguire</param>
//        /// <param name="tbConnection">connessione aperta per eseguire il command</param>
//        //---------------------------------------------------------------------------
//        public TBCommand(string query, TBConnection connection)
//        {
//            if (query == null)
//                throw (new TBException("TBCommand.Constructor: null query parameter"));

//            tbConnection = connection;

//            if (connection.IsSqlConnection())
//                dbCommand = new SqlCommand(query, connection.SqlConnect);
//            //else if (connection.IsOracleConnection())
//            //    dbCommand = new OracleCommand(TBDatabaseType.ReplaceParamSymbol(query, DBMSType.ORACLE), connection.OracleConnect);
//            else if (connection.IsPostgreConnection())
//                dbCommand = new NpgsqlCommand(query, connection.PostgreConnect);
//            else
//                throw (new TBException("TBCommand.Constructor : " + DatabaseLayerStrings.UnknownConnection));
//        }

//        //------------------------------------------------------------------
//        public TBCommand(IDbCommand command)
//        {
//            dbCommand = command;
//        }
//        #endregion

//        #region OperatorCast
//        //------------------------------------------------------------------
//        //public static implicit operator TBCommand(SqlCommand command)
//        //{
//        //    return new TBCommand(command);
//        //}

//        //------------------------------------------------------------------
//        //public static implicit operator TBCommand (OracleCommand command)
//        //{
//        //    return new TBCommand(command);
//        //}
//        #endregion

//        #region Required methods for IDbCommand
//        //---------------------------------------------------------------------------
//        public void Cancel()
//        {
//            dbCommand.Cancel();
//        }

//        //---------------------------------------------------------------------------
//        public IDbDataParameter CreateParameter()
//        {
//            try
//            {
//                return dbCommand.CreateParameter();
//            }
//            catch (Exception e)
//            {
//                throw (new TBException(e.Message, e));
//            }
//        }

//        /// <summary>
//        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected
//        /// </summary>
//        /// <returns>nr. rows affected</returns>
//        //---------------------------------------------------------------------------
//        public int ExecuteNonQuery()
//        {
//            try
//            {
//                return dbCommand.ExecuteNonQuery();
//            }
//            catch (Exception e)
//            {
//                throw (new TBException(e.Message, e));
//            }
//        }

//        /// <summary>
//        /// Sends the CommandText to the Connection and builds a DataReader.
//        /// </summary>
//        /// <returns>IDataReader</returns>
//        //---------------------------------------------------------------------------
//        public IDataReader ExecuteReader()
//        {
//            try
//            {
//                return dbCommand.ExecuteReader();
//            }
//            catch (Exception e)
//            {
//                throw (new TBException(e.Message, e));
//            }
//        }

//        //---------------------------------------------------------------------------
//        public IDataReader ExecuteReader(CommandBehavior behavior)
//        {
//            try
//            {
//                return dbCommand.ExecuteReader(behavior);
//            }
//            catch (Exception e)
//            {
//                throw (new TBException(e.Message, e));
//            }
//        }

//        /// <summary>
//        /// Executes the query, and returns the first column of the first row in the 
//        /// result set returned by the query. Extra columns or rows are ignored.
//        /// </summary>
//        /// <returns>generic object</returns>
//        //---------------------------------------------------------------------------
//        public object ExecuteScalar()
//        {
//            try
//            {
//                return dbCommand.ExecuteScalar();
//            }
//            catch (Exception e)
//            {
//                throw (new TBException(e.Message, e));
//            }
//        }

//        ///<summary>
//        /// ExecuteTBScalar
//        /// Call ExecuteScalar method and effect a casting to int in conformity with database provider
//        ///</summary>
//        //---------------------------------------------------------------------------
//        public int ExecuteTBScalar()
//        {
//            try
//            {
//                object rowsAffected = ExecuteScalar();

//                //if (dbCommand is OracleCommand)
//                //    return (int)(decimal)rowsAffected;

//                return Int32.Parse(rowsAffected.ToString()); // default for SqlCommand
//            }
//            catch (Exception e)
//            {
//                throw (new TBException(e.Message, e));
//            }
//        }

//        /// <summary>
//        /// Sends the CommandText to the Connection and builds an XmlReader object.
//        /// SUPPORTATO SOLO DAL PROVIDER SQLSERVER!!!
//        /// </summary>
//        /// <returns>XmlReader</returns>
//        //---------------------------------------------------------------------------
//        public XmlReader ExecuteXmlReader()
//        {
//            if (/*dbCommand is OracleCommand ||*/ dbCommand is NpgsqlCommand)
//                throw (new TBException("ExecuteXmlReader not supported for an Oracle/Postgre connection"));

//            try
//            {
//                return ((SqlCommand)dbCommand).ExecuteXmlReader();
//            }
//            catch (XmlException e)
//            {
//                throw (new TBException(e.Message, e));
//            }
//        }
//        #endregion

//        #region Methods of TBCommand
//        //---------------------------------------------------------------------------
//        public void Prepare()
//        {
//            try
//            {
//                dbCommand.Prepare();
//            }
//            catch (Exception e)
//            {
//                throw (new TBException(e.Message, e));
//            }
//        }

//        /// <summary>
//        /// dato un parametro IDataParameter lo restituisce come TBParameter. 
//        /// </summary>
//        /// <param name="param"></param>
//        /// <returns></returns>
//        //------------------------------------------------------------------------------------
//        public TBParameter GetTBParameter(IDataParameter param)
//        {
//            if (dbCommand is SqlCommand)
//                return (TBParameter)((SqlParameter)param);

//            //if (dbCommand is OracleCommand)
//            //    return (TBParameter)((OracleParameter)param);

//            if (dbCommand is NpgsqlCommand)
//                return (TBParameter)((NpgsqlParameter)param);

//            throw (new TBException("TBCommand.GetTBParameter" + DatabaseLayerStrings.InvalidCast));
//        }
//        #endregion

//        #region Dispose
//        /// <summary>
//        /// Releases the resources used by the Component.
//        /// </summary>
//        //---------------------------------------------------------------------------
//        public void Dispose()
//        {
//            dbCommand.Dispose();
//        }
//        #endregion
//    }
//    #endregion

//    #region Class TBParameter
//    /// <summary>
//    /// Classe per la gestione dei Parameter
//    /// </summary>
//    //============================================================================
//    public class TBParameter : IDataParameter
//    {
//        private IDataParameter dbParameter = null;

//        #region Required properties for IDataParameter
//        //---------------------------------------------------------------------------
//        public DbType DbType { get { return dbParameter.DbType; } set { dbParameter.DbType = value; } }

//        //---------------------------------------------------------------------------
//        public ParameterDirection Direction { get { return dbParameter.Direction; } set { dbParameter.Direction = value; } }

//        //---------------------------------------------------------------------------
//        public Boolean IsNullable { get { return dbParameter.IsNullable; } }

//        //---------------------------------------------------------------------------
//        public String ParameterName { get { return dbParameter.ParameterName; } set { dbParameter.ParameterName = value; } }

//        //---------------------------------------------------------------------------
//        public String SourceColumn { get { return dbParameter.SourceColumn; } set { dbParameter.SourceColumn = value; } }

//        //---------------------------------------------------------------------------
//        public DataRowVersion SourceVersion { get { return dbParameter.SourceVersion; } set { dbParameter.SourceVersion = value; } }

//        //---------------------------------------------------------------------------
//        public object Value { get { return dbParameter.Value; } set { dbParameter.Value = value; } }
//        #endregion

//        #region Properties of TBParameter
//        //---------------------------------------------------------------------------
//        public IDataParameter DbParameter { get { return dbParameter; } }

//        //---------------------------------------------------------------------------
//        internal SqlParameter SqlParam
//        {
//            get
//            {
//                if (dbParameter is SqlParameter)
//                    return (SqlParameter)dbParameter;

//                //throw (new TBException("TBParameter.SqlParam: " + DatabaseLayerStrings.InvalidCast));
//                return null;
//            }
//        }

//        //---------------------------------------------------------------------------
//        //internal OracleParameter OracleParam
//        //{
//        //    get
//        //    {
//        //        if (dbParameter is OracleParameter)
//        //            return (OracleParameter)dbParameter;

//        //        //throw (new TBException("TBParameter.OracleParam: " + DatabaseLayerStrings.InvalidCast));
//        //        return null;
//        //    }
//        //}

//        //---------------------------------------------------------------------------
//        internal NpgsqlParameter PostgreParam
//        {
//            get
//            {
//                if (dbParameter is NpgsqlParameter)
//                    return (NpgsqlParameter)dbParameter;

//                //throw (new TBException("TBParameter.PostgreParam: " + DatabaseLayerStrings.InvalidCast));
//                return null;
//            }
//        }

//        //---------------------------------------------------------------------------
//        internal SqlDbType SqlDbType
//        {
//            get
//            {
//                if (dbParameter is SqlParameter)
//                    return ((SqlParameter)dbParameter).SqlDbType;
//                //if (dbParameter is OracleParameter)
//                //    return TBDatabaseType.GetSqlDbType(((OracleParameter)dbParameter).OracleType);
//                if (dbParameter is NpgsqlParameter)
//                    return TBDatabaseType.GetSqlDbType(((NpgsqlParameter)dbParameter).NpgsqlDbType);

//                throw (new TBException("TBParameter.SqlDbType: " + DatabaseLayerStrings.InvalidCast));
//            }
//            set
//            {
//                if (dbParameter is SqlParameter)
//                    ((SqlParameter)dbParameter).SqlDbType = value;
//                //if (dbParameter is OracleParameter)
//                //    ((OracleParameter)dbParameter).OracleType = TBDatabaseType.GetOracleType(value);
//                if (dbParameter is NpgsqlParameter)
//                    ((NpgsqlParameter)dbParameter).NpgsqlDbType = TBDatabaseType.GetPostgreDbType(value);

//                throw (new TBException("TBParameter.SqlDbType: " + DatabaseLayerStrings.InvalidCast));
//            }
//        }
//        #endregion

//        #region Constructors
//        //---------------------------------------------------------------------------
//        public TBParameter(IDataParameter dbParam)
//        {
//            dbParameter = dbParam;
//        }

//        //---------------------------------------------------------------------------
//        public TBParameter(DBMSType dbmsType)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                dbParameter = new SqlParameter();
//            //else if (dbmsType == DBMSType.ORACLE)
//            //    dbParameter = new OracleParameter();
//            else if (dbmsType == DBMSType.POSTGRE)
//                dbParameter = new NpgsqlParameter();
//            else
//                throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));

//        }

//        //---------------------------------------------------------------------------
//        public TBParameter(string parameterName, object paramValue, DBMSType dbmsType)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                dbParameter = new SqlParameter(parameterName, paramValue);
//            //else if (dbmsType == DBMSType.ORACLE)
//            //{
//            //    // Oracle va differenza tra stringa vuota e stringa a null per convenzione inizializziamo con il blank
//            //    if (paramValue.GetType().Name == "String")
//            //    {
//            //        string s = (paramValue.ToString().Length == 0) ? " " : paramValue.ToString();
//            //        dbParameter = new OracleParameter(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType), s);
//            //    }
//            //    else
//            //        dbParameter = new OracleParameter(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType), paramValue);
//            //}
//            else if (dbmsType == DBMSType.POSTGRE)
//                dbParameter = new NpgsqlParameter(parameterName, paramValue);

//            else
//                throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//        }

//        //---------------------------------------------------------------------------
//        public TBParameter(string parameterName, TBType dbType, DBMSType dbmsType)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                dbParameter = new SqlParameter(parameterName, dbType.SqlDbType);
//            //else if (dbmsType == DBMSType.ORACLE)
//            //    dbParameter = new OracleParameter(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType), TBDatabaseType.GetOracleType(dbType));
//            else if (dbmsType == DBMSType.POSTGRE)
//                dbParameter = new NpgsqlParameter(parameterName, TBDatabaseType.GetPostgreDbType(dbType)); //@@ Anastasia da verificare  il dbType
//            else
//                throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//        }

//        //---------------------------------------------------------------------------
//        public TBParameter(string parameterName, TBType dbType, int size, DBMSType dbmsType)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                dbParameter = new SqlParameter(parameterName, dbType.SqlDbType, size);
//            //else if (dbmsType == DBMSType.ORACLE)
//            //    dbParameter = new OracleParameter(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType), TBDatabaseType.GetOracleType(dbType), size);
//            else if (dbmsType == DBMSType.POSTGRE)
//                dbParameter = new NpgsqlParameter(parameterName, TBDatabaseType.GetPostgreDbType(dbType), size);
//            else
//                throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//        }

//        //---------------------------------------------------------------------------
//        public TBParameter(string parameterName, TBType dbType, int size, string sourceColumn, DBMSType dbmsType)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                dbParameter = new SqlParameter(parameterName, dbType.SqlDbType, size, sourceColumn);
//            //else if (dbmsType == DBMSType.ORACLE)
//            //    dbParameter = new OracleParameter(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType), TBDatabaseType.GetOracleType(dbType), size, sourceColumn);
//            else if (dbmsType == DBMSType.POSTGRE)
//                dbParameter = new NpgsqlParameter(parameterName, TBDatabaseType.GetPostgreDbType(dbType), size, sourceColumn);
//            else
//                throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//        }

//        //---------------------------------------------------------------------------
//        public TBParameter
//            (
//            string parameterName,
//            TBType dbType,
//            int size,
//            ParameterDirection direction,
//            string sourceColumn,
//            DataRowVersion sourceVersion,
//            object value,
//            DBMSType dbmsType
//            )
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//            { 
//                dbParameter = new SqlParameter(parameterName, dbType.SqlDbType, size, sourceColumn);
//                dbParameter.Value = value;
//                dbParameter.SourceVersion = sourceVersion;
//                dbParameter.Direction = direction;

//            }
//            //else if (dbmsType == DBMSType.ORACLE)
//            //    dbParameter = new OracleParameter(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType), TBDatabaseType.GetOracleType(dbType), size, direction, isNullable, precision, scale, sourceColumn, sourceVersion, value);
//            else if (dbmsType == DBMSType.POSTGRE)
//            {
//                dbParameter = new NpgsqlParameter(parameterName, dbType.PostgreType, size, sourceColumn);
//                dbParameter.Value = value;
//                dbParameter.SourceVersion = sourceVersion;
//                dbParameter.Direction = direction;
//            }
                
//            else
//                throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));
//        }
//        #endregion

//        #region OperatorCast
//        //------------------------------------------------------------------
//        public static implicit operator TBParameter(SqlParameter sqlParam)
//        {
//            return new TBParameter(sqlParam);
//        }

//        //------------------------------------------------------------------
//        //public static implicit operator TBParameter(OracleParameter oracleParam)
//        //{
//        //    return new TBParameter(oracleParam);
//        //}

//        //------------------------------------------------------------------
//        public static implicit operator TBParameter(NpgsqlParameter postgreParam)
//        {
//            return new TBParameter(postgreParam);
//        }
//        #endregion

//        #region SetParameterValue (usata dall'ImportManager)
//        ///<summary>
//        /// SetParameterValue
//        /// Utilizzata nell'ImportManager per settare i parametri per poi effettuare le insert into nelle tabelle
//        ///</summary>
//        //---------------------------------------------------------------------------
//        public object SetParameterValue(string value)
//        {
//            try
//            {
//                switch (this.SqlDbType)
//                {
//                    case (SqlDbType.BigInt):
//                        this.Value = XmlConvert.ToInt64(value);
//                        break;

//                    case (SqlDbType.Binary):
//                    case (SqlDbType.Image):
//                    case (SqlDbType.TinyInt):
//                    case (SqlDbType.VarBinary):
//                        this.Value = XmlConvert.ToByte(value);
//                        break;

//                    case (SqlDbType.Bit):
//                        this.Value = XmlConvert.ToBoolean(value);
//                        break;

//                    case (SqlDbType.DateTime):
//                    case (SqlDbType.SmallDateTime):
//                    case (SqlDbType.Timestamp):
//                        this.Value = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
//                        break;

//                    case (SqlDbType.Decimal):
//                    case (SqlDbType.Money):
//                    case (SqlDbType.SmallMoney):
//                        this.Value = XmlConvert.ToDecimal(value);
//                        break;

//                    case (SqlDbType.Float):
//                        // ONLY FOR ORACLE: during import operation to avoid to add numeric noise to float value
//                        // use the DbType like VarNumeric
//                        //if (this.OracleParam != null)
//                        //    this.DbType = DbType.VarNumeric; //???
//                        if (this.PostgreParam != null)
//                            this.DbType = DbType.Double; //???
//                        this.Value = XmlConvert.ToDouble(value);
//                        break;

//                    case (SqlDbType.Int):
//                        this.Value = XmlConvert.ToInt32(value);
//                        break;

//                    case (SqlDbType.SmallInt):
//                        this.Value = XmlConvert.ToInt16(value);
//                        break;

//                    case (SqlDbType.UniqueIdentifier):
//                        this.Value = XmlConvert.ToGuid(value);
//                        break;

//                    case (SqlDbType.Char):
//                    case (SqlDbType.NChar):
//                    case (SqlDbType.VarChar):
//                    case (SqlDbType.NVarChar):
//                        if (this.PostgreParam != null && this.PostgreParam.NpgsqlDbType == NpgsqlDbType.Uuid)
//                        {
//                            this.DbType = DbType.Guid; //???
//                            this.Value = XmlConvert.ToGuid(value);
//                            break;
//                        }

//                        string readValue = System.Net.WebUtility.HtmlDecode(value);
//                        // se si tratta di Oracle aggiungo un blank
//                        //if (this.OracleParam != null && string.IsNullOrEmpty(readValue))
//                        //    readValue = " ";
//                        this.Value = readValue;
//                        break;

//                    case (SqlDbType.Text):
//                    case (SqlDbType.NText):
//                        // per i Text e NText non discrimino per Oracle (come sopra per le stringhe) e lascio il valore
//                        // letto dal file xml senza mettere il blank
//                        this.Value = System.Net.WebUtility.HtmlDecode(value);
//                        break;

//                    default:
//                        this.Value = value;
//                        break;
//                }
//            }
//            catch (Exception e)
//            {
//                Debug.Fail(e.Message);
//                Debug.WriteLine(e.ToString());
//            }

//            return value;
//        }
//        #endregion
//    }
//    #endregion

//    #region Class TBParameterCollection
//    //==============================================================================
//    public class TBParameterCollection : IDataParameterCollection
//    {
//        private IDataParameterCollection dbParameterCollection = null;
//        private DBMSType dbmsType = DBMSType.UNKNOWN;

//        //---------------------------------------------------------------------------
//        public TBParameterCollection(IDataParameterCollection dbParams)
//        {
//            if (dbParams is SqlParameterCollection)
//                dbmsType = DBMSType.SQLSERVER;
//            //else if (dbParams is OracleParameterCollection)
//            //    dbmsType = DBMSType.ORACLE;
//            else if (dbParams is NpgsqlParameterCollection)
//                dbmsType = DBMSType.POSTGRE;
//            else
//                throw (new TBException(string.Format(DatabaseLayerStrings.UnknownDBMS, dbmsType.ToString())));

//            dbParameterCollection = dbParams;
//        }

//        #region OperatorCast
//        //------------------------------------------------------------------
//        //public static implicit operator TBParameterCollection(SqlParameterCollection sqlParams)
//        //{
//        //    return new TBParameterCollection(sqlParams);
//        //}

//        ////------------------------------------------------------------------
//        //public static implicit operator TBParameterCollection(OracleParameterCollection oracleParams)
//        //{
//        //    return new TBParameterCollection(oracleParams);
//        //}
//        #endregion

//        #region Properties of IDataParameterCollection
//        //---------------------------------------------------------------------------
//        public object this[string parameterName]
//        {
//            get
//            {
//                return dbParameterCollection[TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType)];
//            }
//            set
//            {
//                dbParameterCollection[TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType)] = value;
//            }
//        }
//        #endregion

//        #region Properties of ICollection
//        //------------------------------------------------------------------
//        public int Count { get { return dbParameterCollection.Count; } }
//        //------------------------------------------------------------------
//        public bool IsSynchronized { get { return dbParameterCollection.IsSynchronized; } }
//        //------------------------------------------------------------------
//        public object SyncRoot { get { return dbParameterCollection.SyncRoot; } }
//        //---------------------------------------------------------------------------
//        public object this[int index] { get { return dbParameterCollection[index]; } set { dbParameterCollection[index] = value; } }
//        #endregion

//        #region Properties of IList
//        //------------------------------------------------------------------
//        public bool IsFixedSize { get { return dbParameterCollection.IsFixedSize; } }
//        //------------------------------------------------------------------
//        public bool IsReadOnly { get { return dbParameterCollection.IsReadOnly; } }
//        #endregion

//        #region Properties of TBParameterCollection
//        //------------------------------------------------------------------
//        internal SqlParameterCollection SqlParameterCollection
//        {
//            get
//            {
//                if (dbmsType == DBMSType.SQLSERVER)
//                    return (SqlParameterCollection)dbParameterCollection;
//                throw (new TBException("TBParameterCollection.SqlParameterCollection: " + DatabaseLayerStrings.InvalidCast));
//            }
//        }

//        //------------------------------------------------------------------
//        //internal OracleParameterCollection OracleParameterCollection
//        //{
//        //    get
//        //    {
//        //        if (dbmsType == DBMSType.ORACLE)
//        //            return (OracleParameterCollection)dbParameterCollection;
//        //        throw (new TBException("TBParameterCollection.OracleParameterCollection: " + DatabaseLayerStrings.InvalidCast));
//        //    }
//        //}


//        //------------------------------------------------------------------
//        internal NpgsqlParameterCollection PostgreParameterCollection
//        {
//            get
//            {
//                if (dbmsType == DBMSType.POSTGRE)
//                    return (NpgsqlParameterCollection)dbParameterCollection;
//                throw (new TBException("TBParameterCollection.PostgreParameterCollection: " + DatabaseLayerStrings.InvalidCast));
//            }
//        }

//        #endregion

//        #region Methods required by IDataParameterCollectin
//        //---------------------------------------------------------------------------
//        public bool Contains(string parameterName)
//        {
//            return dbParameterCollection.Contains(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType));
//        }

//        //---------------------------------------------------------------------------
//        public int IndexOf(string parameterName)
//        {
//            return dbParameterCollection.IndexOf(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType));
//        }

//        //---------------------------------------------------------------------------
//        public void RemoveAt(string parameterName)
//        {
//            dbParameterCollection.RemoveAt(TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType));
//        }

//        //---------------------------------------------------------------------------
//        public int Add(TBParameter value)
//        {
//            if (value.ParameterName != null)
//                return dbParameterCollection.Add(value.DbParameter);
//            else
//                throw new TBException(DatabaseLayerStrings.NamedParameter);
//        }

//        //---------------------------------------------------------------------------
//        public int Add(string parameterName, object value)
//        {
//            return Add(new TBParameter(parameterName, value, dbmsType));
//        }

//        //---------------------------------------------------------------------------
//        public int Add(string parameterName, TBType dbType)
//        {
//            return Add(new TBParameter(parameterName, dbType, dbmsType));
//        }

//        //---------------------------------------------------------------------------
//        public int Add(string parameterName, TBType dbType, int size)
//        {
//            return Add(new TBParameter(parameterName, dbType, size, dbmsType));
//        }

//        //---------------------------------------------------------------------------
//        public int Add(string parameterName, TBType dbType, int size, string sourceColumn)
//        {
//            return Add(new TBParameter(parameterName, dbType, size, sourceColumn, dbmsType));
//        }
//        #endregion

//        #region Methods required by ICollection
//        //------------------------------------------------------------------
//        public void CopyTo(System.Array array, int index)
//        {
//            dbParameterCollection.CopyTo(array, index);
//        }
//        #endregion

//        #region Methods required by IEnumerable
//        //------------------------------------------------------------------
//        public IEnumerator GetEnumerator()
//        {
//            return dbParameterCollection.GetEnumerator();
//        }
//        #endregion

//        #region Methods required by IList
//        //------------------------------------------------------------------
//        public void Clear()
//        {
//            dbParameterCollection.Clear();
//        }

//        //------------------------------------------------------------------
//        public bool Contains(object value)
//        {
//            return dbParameterCollection.Contains(value);
//        }

//        //------------------------------------------------------------------
//        public int IndexOf(object value)
//        {
//            return dbParameterCollection.IndexOf(value);
//        }

//        //------------------------------------------------------------------
//        public void Insert(int index, object value)
//        {
//            dbParameterCollection.Insert(index, value);
//        }

//        //------------------------------------------------------------------
//        public void Remove(object value)
//        {
//            dbParameterCollection.Remove(value);
//        }

//        //------------------------------------------------------------------
//        public void RemoveAt(int index)
//        {
//            dbParameterCollection.RemoveAt(index);
//        }

//        //------------------------------------------------------------------
//        public int Add(object value)
//        {
//            return Add((TBParameter)value);
//        }
//        #endregion

//        #region Methods of TBParameterCollection
//        //------------------------------------------------------------------
//        public TBParameter GetParameterAt(int index)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                return (TBParameter)SqlParameterCollection[index];
//            //if (dbmsType == DBMSType.ORACLE)
//            //    return (TBParameter)OracleParameterCollection[index];
//            if (dbmsType == DBMSType.POSTGRE)
//                return (TBParameter)PostgreParameterCollection[index];

//            throw (new TBException("TBParameterCollection.GetParameterAt: " + DatabaseLayerStrings.InvalidCast));
//        }

//        //------------------------------------------------------------------
//        public TBParameter GetParameterAt(string parameterName)
//        {
//            if (dbmsType == DBMSType.SQLSERVER)
//                return (TBParameter)SqlParameterCollection[parameterName];
//            //if (dbmsType == DBMSType.ORACLE)
//            //    return (TBParameter)OracleParameterCollection[TBDatabaseType.ReplaceParamSymbol(parameterName, dbmsType)];
//            if (dbmsType == DBMSType.POSTGRE)
//                return (TBParameter)PostgreParameterCollection[parameterName];

//            throw (new TBException("TBParameterCollection.GetParameterAt: " + DatabaseLayerStrings.InvalidCast));
//        }
//        #endregion
//    }
//    #endregion

//    #region Class TBDataAdapter
//    /// <summary>
//    /// Classe per la gestione dei DataAdapter
//    /// </summary>
//    //============================================================================
//    //  public class TBDataAdapter : DbDataAdapter, IDbDataAdapter
//    //  {
//    //      private IDbDataAdapter dbDataAdapter = null;

//    //      private TBCommand selectCommand = null;
//    //      private TBCommand insertCommand = null;
//    //      private TBCommand updateCommand = null;
//    //      private TBCommand deleteCommand = null;

//    //      /*
//    //* Inherit from Component through DbDataAdapter. The event mechanism is designed to work with the 
//    //* Component.Events property. These variables are the keys used to find the events in the components list of events.
//    //*/
//    //      static private readonly object EventRowUpdated = new object();
//    //      static private readonly object EventRowUpdating = new object();

//    //      #region Required properties for DbDataAdapter
//    //      //---------------------------------------------------------------------
//    //      IDbCommand IDbDataAdapter.SelectCommand
//    //      {
//    //          get { return dbDataAdapter.SelectCommand; }
//    //          set
//    //          {
//    //              selectCommand = (TBCommand)value;
//    //              dbDataAdapter.SelectCommand = (selectCommand != null) ? selectCommand.DbCommand : null;
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      IDbCommand IDbDataAdapter.InsertCommand
//    //      {
//    //          get { return dbDataAdapter.InsertCommand; }
//    //          set
//    //          {
//    //              insertCommand = (TBCommand)value;
//    //              dbDataAdapter.InsertCommand = (insertCommand != null) ? insertCommand.DbCommand : null;
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      IDbCommand IDbDataAdapter.UpdateCommand
//    //      {
//    //          get { return dbDataAdapter.UpdateCommand; }
//    //          set
//    //          {
//    //              updateCommand = (TBCommand)value;
//    //              dbDataAdapter.UpdateCommand = (updateCommand != null) ? updateCommand.DbCommand : null;
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      IDbCommand IDbDataAdapter.DeleteCommand
//    //      {
//    //          get { return dbDataAdapter.DeleteCommand; }
//    //          set
//    //          {
//    //              deleteCommand = (TBCommand)value;
//    //              dbDataAdapter.DeleteCommand = (deleteCommand != null) ? deleteCommand.DbCommand : null;
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      public new TBCommand SelectCommand
//    //      {
//    //          get { return selectCommand; }
//    //          set
//    //          {
//    //              selectCommand = value;
//    //              dbDataAdapter.SelectCommand = (selectCommand != null) ? selectCommand.DbCommand : null;
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      public new TBCommand InsertCommand
//    //      {
//    //          get { return insertCommand; }
//    //          set
//    //          {
//    //              insertCommand = value;
//    //              dbDataAdapter.InsertCommand = (insertCommand != null) ? insertCommand.DbCommand : null;
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      new public TBCommand UpdateCommand
//    //      {
//    //          get { return updateCommand; }
//    //          set
//    //          {
//    //              updateCommand = value;
//    //              dbDataAdapter.UpdateCommand = (updateCommand != null) ? updateCommand.DbCommand : null;
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      new public TBCommand DeleteCommand
//    //      {
//    //          get { return deleteCommand; }
//    //          set
//    //          {
//    //              deleteCommand = value;
//    //              dbDataAdapter.DeleteCommand = (deleteCommand != null) ? deleteCommand.DbCommand : null;
//    //          }
//    //      }
//    //      #endregion

//    //      #region Properties of TBDataAdapter
//    //      //---------------------------------------------------------------------
//    //      internal SqlDataAdapter SqlDataAdapter
//    //      {
//    //          get
//    //          {
//    //              if (dbDataAdapter is SqlDataAdapter)
//    //                  return (SqlDataAdapter)dbDataAdapter;
//    //              throw (new TBException("TBDataAdapter.SqlDataAdapter: " + DatabaseLayerStrings.InvalidCast));
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      internal OracleDataAdapter OracleDataAdapter
//    //      {
//    //          get
//    //          {
//    //              if (dbDataAdapter is OracleDataAdapter)
//    //                  return (OracleDataAdapter)dbDataAdapter;
//    //              throw (new TBException("TBDataAdapter.OracleDataAdapter: " + DatabaseLayerStrings.InvalidCast));
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      internal NpgsqlDataAdapter PostgreDataAdapter
//    //      {
//    //          get
//    //          {
//    //              if (dbDataAdapter is NpgsqlDataAdapter)
//    //                  return (NpgsqlDataAdapter)dbDataAdapter;
//    //              throw (new TBException("TBDataAdapter.PostgreDataAdapter: " + DatabaseLayerStrings.InvalidCast));
//    //          }
//    //      }

//    //      #endregion

//    //      #region TBDataAdapter Constructors
//    //      /// <summary>
//    //      /// constructor
//    //      /// Initializes a new instance of the TBDataAdapter class
//    //      /// </summary>
//    //      //---------------------------------------------------------------------
//    //      public TBDataAdapter(TBConnection connect)
//    //      {
//    //          if (connect.IsSqlConnection())
//    //              dbDataAdapter = new SqlDataAdapter();
//    //          else if (connect.IsOracleConnection())
//    //              dbDataAdapter = new OracleDataAdapter();
//    //          else if (connect.IsPostgreConnection())
//    //              dbDataAdapter = new NpgsqlDataAdapter();
//    //          else
//    //              throw (new TBException(DatabaseLayerStrings.UnknownConnection));
//    //      }

//    //      /// <summary>
//    //      /// constructor
//    //      /// Initializes a new instance of the TBDataAdapter class with a SelectCommand and a TBConnection object.
//    //      /// </summary>
//    //      //---------------------------------------------------------------------
//    //      public TBDataAdapter(string selectCommandText, TBConnection connect)
//    //      {
//    //          if (connect.IsSqlConnection())
//    //              dbDataAdapter = new SqlDataAdapter();
//    //          else if (connect.IsOracleConnection())
//    //              dbDataAdapter = new OracleDataAdapter();
//    //          else if (connect.IsPostgreConnection())
//    //              dbDataAdapter = new NpgsqlDataAdapter();
//    //          else
//    //              throw (new TBException(DatabaseLayerStrings.UnknownConnection));

//    //          selectCommand = new TBCommand(selectCommandText, connect);
//    //          insertCommand = new TBCommand(selectCommandText, connect);
//    //          updateCommand = new TBCommand(selectCommandText, connect);
//    //          deleteCommand = new TBCommand(selectCommandText, connect);

//    //          dbDataAdapter.SelectCommand = selectCommand.DbCommand;
//    //      }
//    //      #endregion

//    //      #region Implement abstract methods inherited from DbDataAdapter
//    //      //---------------------------------------------------------------------
//    //      override protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, System.Data.StatementType statementType, DataTableMapping tableMapping)
//    //      {
//    //          return new TBRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
//    //      }

//    //      //---------------------------------------------------------------------
//    //      override protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, System.Data.StatementType statementType, DataTableMapping tableMapping)
//    //      {
//    //          return new TBRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
//    //      }

//    //      //---------------------------------------------------------------------
//    //      override protected void OnRowUpdating(RowUpdatingEventArgs value)
//    //      {
//    //          TBRowUpdatingEventHandler handler = (TBRowUpdatingEventHandler)Events[EventRowUpdating];
//    //          if ((null != handler) && (value is TBRowUpdatingEventArgs))
//    //              handler(this, (TBRowUpdatingEventArgs)value);
//    //      }

//    //      //---------------------------------------------------------------------
//    //      override protected void OnRowUpdated(RowUpdatedEventArgs value)
//    //      {
//    //          TBRowUpdatedEventHandler handler = (TBRowUpdatedEventHandler)Events[EventRowUpdated];
//    //          if ((null != handler) && (value is TBRowUpdatedEventArgs))
//    //              handler(this, (TBRowUpdatedEventArgs)value);
//    //      }

//    //      //---------------------------------------------------------------------
//    //      public event TBRowUpdatingEventHandler RowUpdating
//    //      {
//    //          add { Events.AddHandler(EventRowUpdating, value); }
//    //          remove { Events.RemoveHandler(EventRowUpdating, value); }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      public event TBRowUpdatedEventHandler RowUpdated
//    //      {
//    //          add { Events.AddHandler(EventRowUpdated, value); }
//    //          remove { Events.RemoveHandler(EventRowUpdated, value); }
//    //      }
//    //      #endregion

//    //      #region Methods of TBDataAdapter
//    //      /// <summary>
//    //      /// Adds or refreshes rows in a DataTable to match those in the data source using the DataTable name
//    //      /// </summary>
//    //      /// <param name="data">A DataTable to fill with records</param>
//    //      /// <returns>The number of rows successfully added to or refreshed in the DataTable</returns>
//    //      //---------------------------------------------------------------------
//    //      public override int Fill(DataSet data)
//    //      {
//    //          try
//    //          {
//    //              return dbDataAdapter.Fill(data);
//    //          }
//    //          catch (SystemException e)
//    //          {
//    //              throw (new TBException(e.Message, e));
//    //          }
//    //          catch (Exception e)
//    //          {
//    //              throw (new TBException(e.Message, e));
//    //          }
//    //          throw (new TBException("TBDataAdapter.Fill: " + DatabaseLayerStrings.InvalidCast));
//    //      }

//    //      //---------------------------------------------------------------------
//    //      public new int Fill(DataSet dataSet, int startRecord, int maxRecords, string srcTable)
//    //      {
//    //          try
//    //          {
//    //              if (dbDataAdapter is SqlDataAdapter)
//    //                  return ((SqlDataAdapter)dbDataAdapter).Fill(dataSet, startRecord, maxRecords, srcTable);
//    //              if (dbDataAdapter is OracleDataAdapter)
//    //                  return ((OracleDataAdapter)dbDataAdapter).Fill(dataSet, startRecord, maxRecords, srcTable);
//    //              if (dbDataAdapter is NpgsqlDataAdapter)
//    //                  return ((NpgsqlDataAdapter)dbDataAdapter).Fill(dataSet, startRecord, maxRecords, srcTable);

//    //          }
//    //          catch (SystemException e)
//    //          {
//    //              throw (new TBException(e.Message, e));
//    //          }
//    //          catch (Exception e)
//    //          {
//    //              throw (new TBException(e.Message, e));
//    //          }
//    //          throw (new TBException("TBDataAdapter.Fill: " + DatabaseLayerStrings.InvalidCast));
//    //      }

//    //      //---------------------------------------------------------------------
//    //      public new int Fill(DataTable dataTable)
//    //      {
//    //          try
//    //          {
//    //              if (dbDataAdapter is SqlDataAdapter)
//    //                  return ((SqlDataAdapter)dbDataAdapter).Fill(dataTable);
//    //              if (dbDataAdapter is OracleDataAdapter)
//    //                  return ((OracleDataAdapter)dbDataAdapter).Fill(dataTable);
//    //              if (dbDataAdapter is NpgsqlDataAdapter)
//    //                  return ((NpgsqlDataAdapter)dbDataAdapter).Fill(dataTable);

//    //          }
//    //          catch (SystemException e)
//    //          {
//    //              throw (new TBException(e.Message, e));
//    //          }
//    //          catch (Exception e)
//    //          {
//    //              throw (new TBException(e.Message, e));
//    //          }
//    //          throw (new TBException("TBDataAdapter.Fill: " + DatabaseLayerStrings.InvalidCast));
//    //      }

//    //      //---------------------------------------------------------------------
//    //      public override int Update(DataSet dataSet)
//    //      {
//    //          try
//    //          {
//    //              return dbDataAdapter.Update(dataSet);
//    //          }
//    //          catch (SystemException e)
//    //          {
//    //              throw (new TBException(e.Message, e));
//    //          }
//    //      }

//    //      //---------------------------------------------------------------------
//    //      public new int Update(DataTable dataTable)
//    //      {
//    //          try
//    //          {
//    //              if (dbDataAdapter is SqlDataAdapter)
//    //                  return ((SqlDataAdapter)dbDataAdapter).Update(dataTable);
//    //              if (dbDataAdapter is OracleDataAdapter)
//    //                  return ((OracleDataAdapter)dbDataAdapter).Update(dataTable);
//    //              if (dbDataAdapter is NpgsqlDataAdapter)
//    //                  return ((NpgsqlDataAdapter)dbDataAdapter).Update(dataTable);

//    //          }
//    //          catch (SystemException e)
//    //          {
//    //              throw (new TBException(e.Message, e));
//    //          }
//    //          throw (new TBException("TBDataAdapter.Update: " + DatabaseLayerStrings.InvalidCast));
//    //      }
//    //      #endregion
//    //  }
//    #endregion

//    //public delegate void TBRowUpdatingEventHandler(object sender, TBRowUpdatingEventArgs e);
//    //public delegate void TBRowUpdatedEventHandler(object sender, TBRowUpdatedEventArgs e);

//    #region Class TBRowUpdatingEventArgs
//    //============================================================================
//    //public class TBRowUpdatingEventArgs : RowUpdatingEventArgs
//    //{
//    //    //---------------------------------------------------------------------
//    //    public TBRowUpdatingEventArgs(DataRow row, IDbCommand command, System.Data.StatementType statementType, DataTableMapping tableMapping)
//    //        : base(row, command, statementType, tableMapping)
//    //    {
//    //    }

//    //    // Hide the inherited implementation of the command property.
//    //    //---------------------------------------------------------------------
//    //    new public IDbCommand Command
//    //    {
//    //        get { return base.Command; }
//    //        set { base.Command = value; }
//    //    }
//    //}
//    //#endregion

//    //#region Class TBRowUpdatedEventArgs
//    ////============================================================================
//    //public class TBRowUpdatedEventArgs : RowUpdatedEventArgs
//    //{
//    //    //---------------------------------------------------------------------
//    //    public TBRowUpdatedEventArgs(DataRow row, IDbCommand command, System.Data.StatementType statementType, DataTableMapping tableMapping)
//    //        : base(row, command, statementType, tableMapping)
//    //    {
//    //    }

//    //    // Hide the inherited implementation of the command property.
//    //    //---------------------------------------------------------------------
//    //    new public IDbCommand Command
//    //    {
//    //        get { return base.Command; }
//    //    }
//    //}
//    #endregion

//    // di seguito le classi di ausilio per l'interrogazione allo schema del database
//    //============================================================================
//    //public class SchemaDataTable : DataTable
//    //{
//    //    public SchemaDataTable()
//    //    {
//    //        this.Columns.Add(DBSchemaStrings.Name, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.Schema, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.Definition, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.Type, typeof(Enum));
//    //        this.Columns.Add(DBSchemaStrings.RoutineType, typeof(String));
//    //    }
//    //}

//    //============================================================================
//    //public class UserDataTable : DataTable
//    //{
//    //    public UserDataTable()
//    //    {
//    //        this.Columns.Add(DBSchemaStrings.UserName, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.OSAuthent, typeof(Boolean));
//    //    }
//    //}

//    //============================================================================
//    //public class DefaultDataTable : DataTable
//    //{
//    //    public DefaultDataTable()
//    //    {
//    //        this.Columns.Add(DBSchemaStrings.ColumnName, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.HasDefault, typeof(Boolean));
//    //        this.Columns.Add(DBSchemaStrings.Default, typeof(String));
//    //    }
//    //}

//    //============================================================================
//    //public class FKDataTable : DataTable
//    //{
//    //    public FKDataTable()
//    //    {
//    //        this.Columns.Add(DBSchemaStrings.Name, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.PKTableName, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.Schema, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.FKColumn, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.PKColumn, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.Position, typeof(Int32));
//    //    }
//    //}

//    //============================================================================
//    //public class IdxDataTable : DataTable
//    //{
//    //    public IdxDataTable()
//    //    {
//    //        this.Columns.Add(DBSchemaStrings.Name, typeof(String));
//    //        this.Columns.Add(DBSchemaStrings.PrimaryKey, typeof(Boolean));
//    //        this.Columns.Add(DBSchemaStrings.Unique, typeof(Boolean));
//    //        this.Columns.Add(DBSchemaStrings.Clustered, typeof(Boolean));
//    //        this.Columns.Add(DBSchemaStrings.ColumnName, typeof(String));
//    //    }
//    //}

//    //============================================================================
//    //public class TBDatabaseSchema
//    //{
//    //    private TBConnection tbConnection = null;

//    //    #region Constructors
//    //    //-----------------------------------------------------------------------
//    //    public TBDatabaseSchema(TBConnection tbConn)
//    //    {
//    //        tbConnection = tbConn;
//    //    }

//    //    //-----------------------------------------------------------------------
//    //    public TBDatabaseSchema()
//    //    {
//    //    }
//    //    #endregion

//    //    #region Gestione singola tabella
//    //    ///<summary>
//    //    /// Generico metodo ExistTable per verificare l'esistenza di un oggetto su database
//    //    /// (sia esso tabella, vista o stored procedure)
//    //    /// A seconda del tipo di connessione vengono interrogate direttamente 
//    //    /// le tabelle di sistema del specifico server di database.
//    //    ///</summary>
//    //    //-----------------------------------------------------------------------
//    //    public bool ExistTable(string tableName)
//    //    {
//    //        try
//    //        {
//    //            if (tbConnection.IsSqlConnection())
//    //                return ExistSqlTable(tableName);
//    //            //else if (tbConnection.IsOracleConnection())
//    //            //    return ExistOracleTable(tableName);
//    //            else if (tbConnection.IsPostgreConnection())
//    //                return ExistPostgreTable(tableName);
//    //            else
//    //                return false;
//    //        }
//    //        catch (TBException)
//    //        {
//    //            throw;
//    //        }
//    //    }

//    //    /// <summary>
//    //    /// SQL SERVER
//    //    /// verificare l'esistenza di un oggetto su un database
//    //    /// </summary>	
//    //    //-----------------------------------------------------------------------
//    //    private bool ExistSqlTable(string tableName)
//    //    {
//    //        SqlCommand sqlCommand = null;

//    //        try
//    //        {
//    //            sqlCommand = new SqlCommand
//    //                (
//    //                string.Format("SELECT COUNT(*) FROM sysobjects WHERE id = object_id(N'{0}')", tableName),
//    //                tbConnection.SqlConnect
//    //                );
//    //            return (int)sqlCommand.ExecuteScalar() > 0;
//    //        }
//    //        catch (SqlException e)
//    //        {
//    //            throw (new TBException(e.Message, e));
//    //        }
//    //    }

//    /// <summary>
//    /// ORACLE
//    /// verificare l'esistenza di un oggetto su un database
//    /// </summary>	
//    //-----------------------------------------------------------------------
//    //private bool ExistOracleTable(string tableName)
//    //{
//    //    OracleCommand oraCommand = null;

//    //    try
//    //    {
//    //        string query = string.Format
//    //            (
//    //                "SELECT COUNT(*) FROM ALL_TABLES WHERE TABLE_NAME = '{0}' and OWNER = '{1}'",
//    //                tableName.ToUpper(CultureInfo.InvariantCulture),
//    //                tbConnection.SchemaOwner
//    //            );
//    //        oraCommand = new OracleCommand(query, tbConnection.OracleConnect);
//    //        return (decimal)oraCommand.ExecuteScalar() > 0;
//    //    }
//    //    catch (OracleException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //}


//    /// <summary>
//    /// Postgre
//    /// verificare l'esistenza di un oggetto su un database
//    /// </summary>	
//    //-----------------------------------------------------------------------
//    //private bool ExistPostgreTable(string tableName)
//    //{
//    //    NpgsqlCommand postgreCommand = null;
//    //    tableName = tableName.ToLower();
//    //    try
//    //    {
//    //        postgreCommand = new NpgsqlCommand
//    //            (
//    //            string.Format("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{0}' AND table_name='{1}' ", DatabaseLayerConsts.postgreDefaultSchema, tableName),
//    //            tbConnection.PostgreConnect
//    //            );
//    //        return Int32.Parse(postgreCommand.ExecuteScalar().ToString()) > 0;
//    //    }
//    //    catch (NpgsqlException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //}



//    /// <summary>
//    /// SQL SERVER
//    /// Lettura collation name delle colonne 
//    /// </summary>	
//    //-----------------------------------------------------------------------
//    //public void LoadSqlCollationName(string tableName, ref DataTable dataTable)
//    //{
//    //    // SQLServer: legge dalla view di sistema INFORMATION_SCHEMA.COLUMNS
//    //    string query = string.Format(@"SELECT COLUMN_NAME, COLLATION_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{0}'", tableName);

//    //    SqlCommand command = null;
//    //    SqlDataReader reader = null;

//    //    try
//    //    {
//    //        command = new SqlCommand(query, tbConnection.SqlConnect);
//    //        reader = command.ExecuteReader();
//    //        while (reader.Read())
//    //        {
//    //            foreach (DataRow row in dataTable.Rows)
//    //                if (string.Compare(row["BaseColumnName"].ToString(), reader["COLUMN_NAME"].ToString(),
//    //                    StringComparison.InvariantCultureIgnoreCase) == 0)
//    //                {
//    //                    row["CollationName"] = reader["COLLATION_NAME"].ToString();
//    //                    break;
//    //                }
//    //        }
//    //    }
//    //    catch (SqlException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null && !reader.IsClosed)
//    //            reader.Close();
//    //        if (command != null)
//    //            command.Dispose();
//    //    }
//    //}

//    /// <summary>
//    /// ORACLE
//    /// Lettura constraint di PK di una tabella
//    /// </summary>	
//    //-----------------------------------------------------------------------
//    //public void LoadOraclePrimaryKeyInfo(string tableName, ref DataTable dataTable)
//    //{
//    //    string query =
//    //        string.Format(@"select DISTINCT y.COLUMN_NAME from ALL_CONSTRAINTS x, ALL_CONS_COLUMNS y 
//    //where CONSTRAINT_TYPE = 'P' and x.TABLE_NAME = '{0}' and x.OWNER = '{1}' and
//    //x.TABLE_NAME = y.TABLE_NAME ",
//    //                        tableName.ToUpper(CultureInfo.InvariantCulture),
//    //                        tbConnection.SchemaOwner);

//    //    OracleCommand command = new OracleCommand(query, tbConnection.OracleConnect);
//    //    OracleDataReader reader = null;

//    //    try
//    //    {
//    //        reader = command.ExecuteReader();
//    //        while (reader.Read())
//    //        {
//    //            foreach (DataRow row in dataTable.Rows)
//    //                if (string.Compare(row["ColumnName"].ToString(), reader["COLUMN_NAME"].ToString(),
//    //                    StringComparison.InvariantCultureIgnoreCase) == 0)
//    //                {
//    //                    row["IsPrimaryKey"] = true;
//    //                    break;
//    //                }
//    //        }
//    //    }
//    //    catch (OracleException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null && !reader.IsClosed)
//    //            reader.Close();
//    //        command.Dispose();
//    //    }
//    //}

//    ///<summary>
//    /// GetTableSchema
//    /// Per caricare le informazioni di schema di un oggetto di database
//    /// A seconda del tipo di connessione vengono interrogate direttamente 
//    /// le tabelle di sistema del specifico server di database.
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //public DataTable GetTableSchema(string tableName, bool withKeyInfo)
//    //{
//    //    TBCommand tbCommand = null;
//    //    IDataReader reader = null;
//    //    DataTable dataTable = null;

//    //    string query = string.Empty;

//    //    try
//    //    {
//    //        if (tbConnection.IsSqlConnection())
//    //            query = "SELECT TOP 1 * FROM [{0}]";
//    //        else if (tbConnection.IsOracleConnection())
//    //        {
//    //            // la TOP in Oracle e' cosi: "SELECT * FROM (SELECT * FROM \"{0}\") WHERE ROWNUM <= 1" 
//    //            // ma visto che fa cmq una select all in una subquery lasciamo stare cosi
//    //            query = "select * from \"{0}\"";
//    //            tableName = tableName.ToUpper(CultureInfo.InvariantCulture); // table name uppercase!!!
//    //        }
//    //        else if (tbConnection.IsPostgreConnection())
//    //        {
//    //            query = "select * from {0} LIMIT 1";
//    //            tableName = tableName.ToLower();
//    //        }

//    //        tbCommand = new TBCommand(string.Format(query, tableName), tbConnection);

//    //        reader = tbCommand.ExecuteReader((withKeyInfo) ? CommandBehavior.KeyInfo : CommandBehavior.SchemaOnly);
//    //        dataTable = reader.GetSchemaTable();
//    //        reader.Close();

//    //        DataColumn column = new DataColumn("CollationName", Type.GetType("System.String"));
//    //        column.ReadOnly = false;
//    //        dataTable.Columns.Add(column);

//    //        if (tbConnection.IsSqlConnection())
//    //            LoadSqlCollationName(tableName, ref dataTable);

//    //        return dataTable;
//    //    }
//    //    catch (TBException)
//    //    {
//    //        throw;
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null && !reader.IsClosed)
//    //            reader.Close();
//    //        tbCommand.Dispose();
//    //    }
//    //}
//    #endregion

//    #region Metodi statici
//    ///<summary>
//    /// AddRows (utilizzato solo dalle classi per interfacciarsi con il db Oracle)
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //public static void AddRows(OracleCommand command, ref SchemaDataTable dataTable, string ownerName)
//    //{
//    //    OracleDataReader reader = null;

//    //    try
//    //    {
//    //        reader = command.ExecuteReader();
//    //        if (reader == null)
//    //            return;

//    //        DataRow row = null;
//    //        string procedureName = string.Empty;

//    //        while (reader.Read())
//    //        {
//    //            if ((reader["TYPE"].ToString()) == "PROCEDURE")
//    //            {
//    //                if (procedureName != reader["NAME"].ToString())
//    //                {
//    //                    row = dataTable.NewRow();
//    //                    row[0] = reader["NAME"].ToString();
//    //                    row[1] = ownerName.ToUpper(CultureInfo.InvariantCulture);
//    //                    row[2] = reader["TEXT"].ToString();
//    //                    row[3] = DBObjectTypes.ROUTINE;
//    //                    row[4] = "PROCEDURE";
//    //                    dataTable.Rows.Add(row);
//    //                    procedureName = reader["NAME"].ToString();
//    //                }
//    //                else
//    //                    row[2] = row[2].ToString() + reader["TEXT"].ToString();
//    //            }
//    //            else
//    //            {
//    //                row = dataTable.NewRow();
//    //                row[0] = reader["NAME"].ToString();
//    //                row[1] = ownerName.ToUpper(CultureInfo.InvariantCulture);
//    //                row[2] = reader["TEXT"].ToString();
//    //                row[3] = ((reader["TYPE"].ToString()) == "T") ? DBObjectTypes.TABLE : DBObjectTypes.VIEW;
//    //                row[4] = string.Empty;
//    //                dataTable.Rows.Add(row);
//    //            }
//    //        }
//    //    }
//    //    catch (OracleException)
//    //    {
//    //        if (reader != null)
//    //            reader.Close();
//    //        throw;
//    //    }
//    //}
//    #endregion

//    #region Caricamento info struttura di views e stored procedures 
//    ///<summary>
//    /// LoadViewOrProcedureInfo (solo per SQL)
//    /// Metodo utilizzato per caricare dalle tabelle di sistema le informazioni di oggetti di database
//    /// di tipo VIEW e PROCEDURE, tra cui:
//    /// - definizione completa (ovvero il testo dello script)
//    /// - nomi colonne o alias per le views, oppure nome parametro per le procedures
//    /// - tipo colonna
//    /// - length colonna
//    /// - se si tratta di un parametro di out (solo per le procedures)
//    /// - collation della colonna
//    /// N.B. Tale metodo è stato implementato in supporto alla gestione del parse degli script SQL e per
//    /// la generazione di file xml comprensivi della struttura del database. Non viene effettuato un parse
//    /// da file xchè troppo complesso e quindi si demanda la ricerca di tali informazioni alle tabelle di
//    /// sistema del database.
//    /// METODO RICHIAMATO DAL TBWIZARD!!!
//    ///</summary>
//    //-------------------------------------------------------------------------
//    //public DataTable LoadViewOrProcedureInfo(string objectName, DBObjectTypes objectType, out string objectDefinition)
//    //{
//    //    objectDefinition = string.Empty;

//    //    if (!tbConnection.IsSqlConnection())
//    //        throw (new TBException("LoadViewOrProcedureInfo is supported only for SQL connection"));

//    //    if (objectType == DBObjectTypes.ALL || objectType == DBObjectTypes.TABLE || objectType == DBObjectTypes.SYNONYM)
//    //        throw (new TBException("LoadViewOrProcedureInfo supported only VIEW or PROCEDURE types"));

//    //    DataTable columnsDataTable = new DataTable();
//    //    columnsDataTable.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));
//    //    columnsDataTable.Columns.Add(new DataColumn("Type", Type.GetType("System.String")));
//    //    columnsDataTable.Columns.Add(new DataColumn("Length", Type.GetType("System.Int32")));
//    //    columnsDataTable.Columns.Add(new DataColumn("OutParam", Type.GetType("System.Int32")));
//    //    columnsDataTable.Columns.Add(new DataColumn("Collation", Type.GetType("System.String")));

//    //    // query x estrarre il testo completo dell'oggetto
//    //    string querySysComments = "SELECT text FROM syscomments WHERE id = OBJECT_ID('{0}') and encrypted = 0";

//    //    // query x individuare il nome delle colonne, tipo, lunghezza, collation e se parametro di out
//    //    string queryInfo = @"SELECT SC.name as Name, 
//    //ST.name as Type, SC.length as Length, SC.isoutparam as OutParam,
//    //SC.collation as Collation
//    //FROM sysobjects AS SO
//    //INNER JOIN syscolumns AS SC ON SC.id = SO.id
//    //INNER JOIN systypes AS ST ON SC.xtype = ST.xtype
//    //WHERE SO.name = '{0}'
//    //order by SC.colorder";

//    //    TBCommand tbCommand = null;
//    //    IDataReader myReader = null;

//    //    try
//    //    {
//    //        tbCommand = new TBCommand(string.Format(querySysComments, objectName), tbConnection);
//    //        myReader = tbCommand.ExecuteReader();

//    //        if (myReader != null)
//    //        {
//    //            while (myReader.Read())
//    //                objectDefinition += myReader["text"].ToString();

//    //            myReader.Close();
//    //            myReader.Dispose();
//    //        }

//    //        tbCommand.CommandText = string.Format(queryInfo, objectName);
//    //        myReader = tbCommand.ExecuteReader();

//    //        if (myReader != null)
//    //        {
//    //            while (myReader.Read())
//    //            {
//    //                DataRow rowToAdd = columnsDataTable.NewRow();
//    //                rowToAdd["Name"] = myReader[0].ToString();
//    //                rowToAdd["Type"] = myReader[1].ToString();
//    //                rowToAdd["Length"] = myReader[2].ToString();
//    //                rowToAdd["OutParam"] = myReader[3].ToString();
//    //                rowToAdd["Collation"] = myReader[4].ToString();
//    //                columnsDataTable.Rows.Add(rowToAdd);
//    //            }

//    //            myReader.Close();
//    //            myReader.Dispose();
//    //        }

//    //        tbCommand.Dispose();

//    //        return columnsDataTable;
//    //    }
//    //    catch (TBException)
//    //    {
//    //        if (myReader != null && !myReader.IsClosed)
//    //        {
//    //            myReader.Close();
//    //            myReader.Dispose();
//    //        }

//    //        tbCommand.Dispose();
//    //        throw;
//    //    }
//    //}
//    #endregion

//    #region ADO.NET vs OLEDB
//    //-------------------------------------------------------------------------
//    //public DataTable GetParameters(string objectName)
//    //{
//    //    DataTable columnsDataTable = null;
//    //    if (tbConnection.IsSqlConnection())
//    //        columnsDataTable = GetSqlParametersInfo(objectName);

//    //    //else if (tbConnection.IsOracleConnection())
//    //    //    columnsDataTable = GetOracleParametersInfo(objectName);

//    //    else if (tbConnection.IsPostgreConnection())
//    //        columnsDataTable = GetPostgreParametersInfo(objectName);

//    //    return columnsDataTable;
//    //}

//    //-------------------------------------------------------------------------
//    //public DataTable GetOracleParametersInfo(string objectName)
//    //{
//    //    DataTable columnsDataTable = null;
//    //    return columnsDataTable;
//    //}

//    //-------------------------------------------------------------------------
//    //public DataTable GetPostgreParametersInfo(string objectName)
//    //{
//    //    DataTable columnsDataTable = null;
//    //    return columnsDataTable;
//    //}

//    //-------------------------------------------------------------------------
//    //public DataTable GetSqlParametersInfo(string objectName)
//    //{
//    //    TBCommand tbCommand = null;
//    //    IDataReader myReader = null;

//    //    DataTable columnsDataTable = null; //new DataTable();
//    //    columnsDataTable.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));
//    //    columnsDataTable.Columns.Add(new DataColumn("OrdinalPosition", Type.GetType("System.Short")));
//    //    columnsDataTable.Columns.Add(new DataColumn("ParamMode", Type.GetType("System.String")));
//    //    columnsDataTable.Columns.Add(new DataColumn("IsResult", Type.GetType("System.Boolean")));
//    //    columnsDataTable.Columns.Add(new DataColumn("DataType", Type.GetType("System.String")));
//    //    columnsDataTable.Columns.Add(new DataColumn("MaxLength", Type.GetType("System.Int32")));
//    //    columnsDataTable.Columns.Add(new DataColumn("MaxOctetLength", Type.GetType("System.Int32")));
//    //    columnsDataTable.Columns.Add(new DataColumn("CollationName", Type.GetType("System.String")));
//    //    columnsDataTable.Columns.Add(new DataColumn("NumericPrecision", Type.GetType("System.Int32")));
//    //    columnsDataTable.Columns.Add(new DataColumn("NumericScale", Type.GetType("System.Int32")));

//    //    // query x individuare il nome delle colonne, tipo, lunghezza, collation e se parametro di out
//    //    string queryInfo = @"select PARAMETER_NAME, ORDINAL_POSITION, PARAMETER_MODE, IS_RESULT, 
//    //DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, CHARACTER_OCTET_LENGTH, COLLATION_NAME, 
//    //NUMERIC_PRECISION, NUMERIC_SCALE from INFORMATION_SCHEMA.PARAMETERS
//    //WHERE SPECIFIC_NAME = '{0}'
//    //order by ORDINAL_POSITION";
//    //    try
//    //    {
//    //        tbCommand = new TBCommand(string.Format(queryInfo, objectName), tbConnection);
//    //        myReader = tbCommand.ExecuteReader();

//    //        if (myReader != null)
//    //        {
//    //            while (myReader.Read())
//    //            {
//    //                DataRow rowToAdd = columnsDataTable.NewRow();
//    //                rowToAdd["Name"] = myReader[0].ToString();
//    //                rowToAdd["OrdinalPosition"] = Convert.ToInt16(myReader[1]);
//    //                rowToAdd["ParamMode"] = myReader[2].ToString();
//    //                rowToAdd["IsResult"] = myReader[3].ToString();
//    //                rowToAdd["DataType"] = myReader[4].ToString();
//    //                rowToAdd["MaxLength"] = Convert.ToInt32(myReader[5]);
//    //                rowToAdd["MaxOctetLength"] = (Int32)myReader[6];
//    //                rowToAdd["CollationName"] = myReader[7].ToString();
//    //                rowToAdd["NumericPrecision"] = myReader[8].ToString();
//    //                rowToAdd["NumericScale"] = myReader[9].ToString();
//    //                columnsDataTable.Rows.Add(rowToAdd);
//    //            }

//    //            myReader.Close();
//    //            myReader.Dispose();
//    //        }

//    //        tbCommand.Dispose();

//    //return columnsDataTable;
//    //    }
//    //    catch (TBException)
//    //    {
//    //        if (myReader != null)
//    //        {
//    //            myReader.Close();
//    //            myReader.Dispose();
//    //        }

//    //        tbCommand.Dispose();
//    //        throw;
//    //    }
//    //}

//    #endregion

//    #region Informazioni sugli oggetti dello schema
//    ///<summary>
//    /// Generico metodo GetAllSchemaObjects per caricare l'elenco degli oggetti di un database
//    /// a seconda del tipo specificato (tabelle, viste, stored procedures o tutti)
//    /// A seconda del tipo di connessione vengono interrogate direttamente 
//    /// le tabelle di sistema del specifico server di database.
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //public SchemaDataTable GetAllSchemaObjects(DBObjectTypes dbObj)
//    //{
//    //    try
//    //    {
//    //        if (tbConnection.IsSqlConnection())
//    //            return GetSqlAllSchemaObjects(dbObj);
//    //        else if (tbConnection.IsOracleConnection())
//    //            return GetOracleAllSchemaObjects(dbObj);
//    //        else if (tbConnection.IsPostgreConnection())
//    //            return GetPostgreAllSchemaObjects(dbObj);

//    //        return null;
//    //    }
//    //    catch (TBException)
//    //    {
//    //        throw;
//    //    }
//    //}

//    ///<summary>
//    /// SQL SERVER
//    /// Per caricare l'elenco degli oggetti del database
//    /// a seconda del tipo specificato (tabelle, viste, stored procedures o tutti)
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //private SchemaDataTable GetSqlAllSchemaObjects(DBObjectTypes dbObj)
//    //{
//    //    SqlDataReader reader = null;
//    //    SqlCommand command = null;
//    //    SchemaDataTable dataTable = new SchemaDataTable();

//    //    string query = string.Empty;

//    //    switch (dbObj)
//    //    {
//    //        case DBObjectTypes.TABLE:
//    //            query = "select TABLE_NAME as name, '' as routineType, '' as definition from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";
//    //            break;

//    //        case DBObjectTypes.VIEW:
//    //            query = "select TABLE_NAME as name, '' as routineType, VIEW_DEFINITION as definition from INFORMATION_SCHEMA.VIEWS";
//    //            break;

//    //        case DBObjectTypes.ROUTINE:
//    //            query = "select ROUTINE_NAME as name, ROUTINE_TYPE as routineType, ROUTINE_DEFINITION as definition from INFORMATION_SCHEMA.ROUTINES";
//    //            break;
//    //    }

//    //    try
//    //    {
//    //        command = new SqlCommand(query, tbConnection.SqlConnect);
//    //        reader = command.ExecuteReader();

//    //        DataRow row = null;
//    //        while (reader.Read())
//    //        {
//    //            if (reader["name"].ToString() == "dtproperties" ||
//    //                reader["name"].ToString() == "sysalternates" ||
//    //                reader["name"].ToString() == "sysconstraints" ||
//    //                reader["name"].ToString() == "syssegments" ||
//    //                reader["name"].ToString().IndexOf("dt_") == 0)
//    //                continue;

//    //            row = dataTable.NewRow();
//    //            row[0] = reader["name"].ToString();
//    //            row[1] = tbConnection.Database;
//    //            row[2] = reader["definition"].ToString();
//    //            row[3] = dbObj;
//    //            row[4] = reader["routineType"].ToString();
//    //            dataTable.Rows.Add(row);
//    //        }
//    //    }
//    //    catch (SqlException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null)
//    //            reader.Close();
//    //        command.Dispose();
//    //    }

//    //    return dataTable;
//    //}

//    ///<summary>
//    /// Postgre
//    /// Per caricare l'elenco degli oggetti del database
//    /// a seconda del tipo specificato (tabelle, viste, stored procedures o tutti)
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //private SchemaDataTable GetPostgreAllSchemaObjects(DBObjectTypes dbObj)
//    //{
//    //    NpgsqlDataReader reader = null;
//    //    NpgsqlCommand command = null;
//    //    SchemaDataTable dataTable = new SchemaDataTable();

//    //    string query = string.Empty;

//    //    switch (dbObj)
//    //    {
//    //        case DBObjectTypes.TABLE:
//    //            query = string.Format("select table_name as name, '' as routineType, '' as definition from information_schema.tables  where table_schema = '{0}' and table_type = 'BASE TABLE'", DatabaseLayerConsts.postgreDefaultSchema);
//    //            break;

//    //        case DBObjectTypes.VIEW:
//    //            query = string.Format("select table_name as name, '' as routineType, view_definition as definition  from information_schema.views  where table_schema = '{0}'", DatabaseLayerConsts.postgreDefaultSchema);
//    //            break;

//    //        case DBObjectTypes.ROUTINE:
//    //            query = string.Format("select routine_name as name, routine_type as routineType, routine_definition as definition  from information_schema.routines  where routine_schema = '{0}'", DatabaseLayerConsts.postgreDefaultSchema);
//    //            break;
//    //    }

//    //    try
//    //    {
//    //        command = new NpgsqlCommand(query, tbConnection.PostgreConnect);
//    //        reader = command.ExecuteReader();

//    //        DataRow row = null;
//    //        while (reader.Read())
//    //        {
//    //            row = dataTable.NewRow();
//    //            row[0] = reader["name"];
//    //            row[1] = tbConnection.Database;
//    //            row[2] = reader["definition"];
//    //            row[3] = dbObj;
//    //            row[4] = reader["routinetype"];
//    //            dataTable.Rows.Add(row);
//    //        }
//    //    }
//    //    catch (NpgsqlException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null)
//    //            reader.Close();
//    //        command.Dispose();
//    //    }

//    //    return dataTable;
//    //}


//    ///<summary>
//    /// ORACLE
//    /// Per caricare l'elenco degli oggetti del database
//    /// a seconda del tipo specificato (tabelle, viste, stored procedures o tutti)
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //private SchemaDataTable GetOracleAllSchemaObjects(DBObjectTypes dbObj)
//    //{
//    //    OracleCommand command = null;
//    //    SchemaDataTable dataTable = new SchemaDataTable();

//    //    string query = string.Empty;
//    //    string query1 = string.Empty;
//    //    string query2 = string.Empty;

//    //    switch (dbObj)
//    //    {
//    //        case DBObjectTypes.TABLE:
//    //            query = "select TABLE_NAME NAME, '' TEXT, 'T' TYPE from USER_TABLES";
//    //            break;

//    //        case DBObjectTypes.VIEW:
//    //            query = "select VIEW_NAME NAME, TEXT, 'V' TYPE from USER_VIEWS";
//    //            break;

//    //        case DBObjectTypes.ROUTINE:
//    //            query = "select NAME, TEXT, TYPE from USER_SOURCE  where TYPE = 'PROCEDURE' order by NAME, LINE";
//    //            break;

//    //        case DBObjectTypes.ALL: //non posso fare la UNION a causa del campo TEXT di USER_VIEWS e di USER_SOURCE
//    //                                // che è di tipo LONG e il tipo LONG non si può utilizzare nelle espressioni e nelle UNION
//    //            query = "select TABLE_NAME NAME, '' TEXT, 'T' TYPE from USER_TABLES";
//    //            query1 = @"select VIEW_NAME NAME, TEXT, 'V' TYPE  FROM USER_VIEWS";
//    //            query2 = @"select NAME, TEXT, TYPE from USER_SOURCE where TYPE = 'PROCEDURE' order by NAME, LINE";
//    //            break;
//    //    }
//    //    try
//    //    {
//    //        command = new OracleCommand(query, tbConnection.OracleConnect);
//    //        AddRows(command, ref dataTable, tbConnection.Database);

//    //        if (dbObj == DBObjectTypes.ALL)
//    //        {
//    //            command.CommandText = query1;
//    //            AddRows(command, ref dataTable, tbConnection.Database);
//    //            command.CommandText = query2;
//    //            AddRows(command, ref dataTable, tbConnection.Database);
//    //        }
//    //    }
//    //    catch (OracleException e)
//    //    {
//    //        command.Dispose();
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    catch (TBException)
//    //    {
//    //        command.Dispose();
//    //        throw;
//    //    }

//    //    command.Dispose();
//    //    return dataTable;
//    //}

//    /// <summary>
//    /// Dato il proprietario dello schema e il tipo di oggetto restituisce gli eventuali synonym presenti
//    /// per l'utente attualmente connesso (only for ORACLE)
//    /// </summary>
//    //-----------------------------------------------------------------------
//    //public SchemaDataTable GetAllSynonymSchemaObjects(DBObjectTypes dbObj)
//    //{
//    //    if (tbConnection.SchemaOwner.Length == 0)
//    //        throw (new TBException("Request owner schema but it is empty"));

//    //    if (!tbConnection.IsOracleConnection())
//    //        throw (new TBException("An Oracle connection is required"));

//    //    OracleDataReader reader = null;
//    //    OracleCommand command = null;
//    //    SchemaDataTable dataTable = new SchemaDataTable();

//    //    string sysViewName = string.Empty;
//    //    string colName = string.Empty;

//    //    switch (dbObj)
//    //    {
//    //        case DBObjectTypes.TABLE:
//    //            sysViewName = "ALL_TABLES";
//    //            colName = "TABLE_NAME";
//    //            break;

//    //        case DBObjectTypes.VIEW:
//    //            sysViewName = "ALL_VIEWS";
//    //            colName = "VIEW_NAME";
//    //            break;

//    //        case DBObjectTypes.ROUTINE:
//    //            sysViewName = "ALL_SOURCE";
//    //            colName = "NAME";
//    //            break;
//    //    }

//    //    try
//    //    {
//    //        string query = "select SYNONYM_NAME from USER_SYNONYMS";

//    //        if (dbObj != DBObjectTypes.ALL)
//    //            query += string.Format(@" where exists (select TABLE_NAME from {0} 
//    //			where USER_SYNONYMS.TABLE_NAME = {0}.{1} and {0}.OWNER = '{2}')",
//    //                                    sysViewName, colName, tbConnection.SchemaOwner);
//    //        command = new OracleCommand(query, tbConnection.OracleConnect);
//    //        reader = command.ExecuteReader();

//    //        DataRow row = null;
//    //        while (reader.Read())
//    //        {
//    //            row = dataTable.NewRow();
//    //            row[0] = reader["SYNONYM_NAME"].ToString();
//    //            row[1] = tbConnection.Database;
//    //            row[2] = "";
//    //            row[3] = (dbObj == DBObjectTypes.ALL) ? DBObjectTypes.SYNONYM : dbObj;
//    //            dataTable.Rows.Add(row);
//    //        }
//    //    }
//    //    catch (OracleException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null)
//    //            reader.Close();
//    //        command.Dispose();
//    //    }

//    //    return dataTable;
//    //}
//    #endregion

//    #region Gestione default
//    ///<summary>
//    /// Generico metodo LoadDefaults per caricare l'elenco dei constraint di default di una tabella
//    /// A seconda del tipo di connessione vengono interrogate direttamente 
//    /// le tabelle di sistema del specifico server di database.
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //public DefaultDataTable LoadDefaults(string tableName)
//    //{
//    //    try
//    //    {
//    //        if (tbConnection.IsSqlConnection())
//    //            return LoadSqlDefaults(tableName);
//    //        else if (tbConnection.IsOracleConnection())
//    //            return LoadOracleDefaults(tableName);
//    //        else if (tbConnection.IsPostgreConnection())
//    //            return LoadPostgreDefaults(tableName);


//    //        return null;
//    //    }
//    //    catch (TBException)
//    //    {
//    //        throw;
//    //    }
//    //}

//    ///<summary>
//    /// SQL SERVER
//    /// Generico metodo LoadDefaults per caricare l'elenco dei constraint di default di una tabella
//    ///</summary>
//    //-----------------------------------------------------------------------
//    // private DefaultDataTable LoadSqlDefaults(string tableName)
//    // {
//    //     SqlDataReader reader = null;
//    //     SqlCommand command = null;
//    //     DefaultDataTable dataTable = new DefaultDataTable();

//    //     string query = @"SELECT COLUMN_DEFAULT, COLUMN_NAME 
//    //FROM INFORMATION_SCHEMA.COLUMNS 
//    //WHERE TABLE_NAME = @tablename ORDER BY COLUMN_NAME";
//    //     try
//    //     {
//    //         command = new SqlCommand(query, tbConnection.SqlConnect);
//    //         command.Parameters.AddWithValue("@tablename", tableName);
//    //         reader = command.ExecuteReader();

//    //         DataRow row = null;
//    //         while (reader.Read())
//    //         {
//    //             row = dataTable.NewRow();
//    //             row[0] = reader["COLUMN_NAME"].ToString();
//    //             row[1] = reader["COLUMN_DEFAULT"] != System.DBNull.Value;
//    //             row[2] = reader["COLUMN_DEFAULT"].ToString();
//    //             dataTable.Rows.Add(row);
//    //         }

//    //         reader.Close();
//    //     }
//    //     catch (SqlException e)
//    //     {
//    //         throw (new TBException(e.Message, e));
//    //     }
//    //     finally
//    //     {
//    //         if (reader != null)
//    //             reader.Close();
//    //         command.Dispose();
//    //     }

//    //     return dataTable;
//    // }

//    ///<summary>
//    /// ORACLE
//    /// Generico metodo LoadDefaults per caricare l'elenco dei constraint di default di una tabella
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //  private DefaultDataTable LoadOracleDefaults(string tableName)
//    //  {
//    //      OracleDataReader reader = null;
//    //      OracleCommand command = null;
//    //      DefaultDataTable dataTable = new DefaultDataTable();

//    //      try
//    //      {
//    //          command = new OracleCommand
//    //              (
//    //                  string.Format
//    //                  (
//    //                  @"Select COLUMN_NAME, DATA_DEFAULT from ALL_TAB_COLUMNS 
//    //where TABLE_NAME = '{0}'and OWNER = '{1}' ORDER BY COLUMN_NAME",
//    //                  tableName.ToUpper(CultureInfo.InvariantCulture),
//    //                  tbConnection.SchemaOwner
//    //                  ),
//    //                  tbConnection.OracleConnect
//    //              );

//    //          reader = command.ExecuteReader();

//    //          DataRow row = null;
//    //          while (reader.Read())
//    //          {
//    //              row = dataTable.NewRow();
//    //              row[0] = reader["COLUMN_NAME"].ToString();
//    //              row[1] = reader["DATA_DEFAULT"] != System.DBNull.Value;
//    //              row[2] = reader["DATA_DEFAULT"].ToString();
//    //              dataTable.Rows.Add(row);
//    //          }

//    //          reader.Close();
//    //      }
//    //      catch (OracleException e)
//    //      {
//    //          throw (new TBException(e.Message, e));
//    //      }
//    //      finally
//    //      {
//    //          if (reader != null)
//    //              reader.Close();
//    //          command.Dispose();
//    //      }

//    //      return dataTable;
//    //  }

//    ///<summary>
//    /// Postgre
//    /// Generico metodo LoadDefaults per caricare l'elenco dei constraint di default di una tabella
//    ///</summary>
//    //-----------------------------------------------------------------------
//    //private DefaultDataTable LoadPostgreDefaults(string tableName)
//    //{
//    //    NpgsqlDataReader reader = null;
//    //    NpgsqlCommand command = null;
//    //    DefaultDataTable dataTable = new DefaultDataTable();

//    //    string query = string.Format("SELECT COLUMN_DEFAULT, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME =@tablename and table_schema='{0}' ORDER BY COLUMN_NAME", DatabaseLayerConsts.postgreDefaultSchema);
//    //    try
//    //    {

//    //        command = new NpgsqlCommand(query, tbConnection.PostgreConnect);
//    //        command.Parameters.AddWithValue("@tablename", tableName.ToLower());
//    //        reader = command.ExecuteReader();

//    //        DataRow row = null;
//    //        while (reader.Read())
//    //        {
//    //            row = dataTable.NewRow();
//    //            row[0] = reader["column_name"].ToString();
//    //            row[1] = reader["column_default"] != System.DBNull.Value;
//    //            row[2] = reader["column_default"].ToString();
//    //            dataTable.Rows.Add(row);
//    //        }

//    //        reader.Close();
//    //    }
//    //    catch (NpgsqlException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null)
//    //            reader.Close();
//    //        command.Dispose();
//    //    }

//    //    return dataTable;
//    //}

//    #endregion

//    #region Gestione ForeignKeys
//    /// <summary>
//    /// Generico metodo LoadFKConstraints per caricare le informazioni relative 
//    /// ai foreign key constraints relativi alla tabella.
//    /// A seconda del tipo di connessione vengono interrogate direttamente 
//    /// le tabelle di sistema del specifico server di database.
//    /// </summary>	
//    //---------------------------------------------------------------------------
//    //public FKDataTable LoadFKConstraints(string tableName)
//    //{
//    //    try
//    //    {
//    //        if (tbConnection.IsSqlConnection())
//    //            return LoadSqlFKConstraints(tableName);
//    //        else if (tbConnection.IsOracleConnection())
//    //            return LoadOracleFKConstraints(tableName);
//    //        else if (tbConnection.IsPostgreConnection())
//    //            return LoadPostgreFKConstraints(tableName);

//    //        return null;
//    //    }
//    //    catch (TBException)
//    //    {
//    //        throw;
//    //    }
//    //}

//    /// <summary>
//    /// SQL SERVER
//    /// carica le informazioni relative alla foreign key constraints relative alla tabella 
//    /// </summary>	
//    //---------------------------------------------------------------------------
//    //    private FKDataTable LoadSqlFKConstraints(string tableName)
//    //    {
//    //        SqlDataReader reader = null;
//    //        SqlCommand command = null;
//    //        FKDataTable dataTable = new FKDataTable();

//    //        string query =
//    //            @"SELECT DISTINCT X.name AS FKName, Y.name AS TableName 
//    //FROM sysobjects X, sysobjects Y, sysforeignkeys F 
//    //WHERE X.id = F.constid AND F.rkeyid = Y.id AND F.fkeyid = 
//    //(SELECT id FROM sysobjects WHERE name = @tablename AND xtype = @type)";

//    //        try
//    //        {
//    //            command = new SqlCommand(query, tbConnection.SqlConnect);
//    //            command.Parameters.AddWithValue("@tablename", tableName);
//    //            command.Parameters.AddWithValue("@type", "U");
//    //            reader = command.ExecuteReader();

//    //            DataRow row = null;
//    //            while (reader.Read())
//    //            {
//    //                row = dataTable.NewRow();
//    //                row[0] = reader["FKName"].ToString();
//    //                row[1] = reader["TableName"].ToString(); ;
//    //                row[2] = tbConnection.Database;
//    //                dataTable.Rows.Add(row);
//    //            }
//    //        }
//    //        catch (SqlException e)
//    //        {
//    //            throw (new TBException(e.Message, e));
//    //        }
//    //        finally
//    //        {
//    //            if (reader != null && !reader.IsClosed)
//    //                reader.Close();
//    //            command.Dispose();
//    //        }

//    //        return dataTable;
//    //    }

//    ///<summary>
//    /// ORACLE
//    /// metodo utilizzato per ORACLE al posto del metodo basato su connessione OLEDB: LoadFkConstraintsInfo
//    /// questo a causa dell'enorme lentezza in caso di connessione ORACLE
//    ///</summary>
//    //---------------------------------------------------------------------------
//    //  public FKDataTable LoadOracleFKConstraintsAllInfo(string tableName)
//    //  {
//    //      OracleCommand fkCommand = null;
//    //      OracleDataReader fkReader = null;

//    //      OracleCommand fkColCommand = null;
//    //      OracleCommand pkColCommand = null;
//    //      OracleDataReader colReader = null;

//    //      FKDataTable dataTable = new FKDataTable();
//    //      int count = 0;

//    //      try
//    //      {
//    //          //estraggo i constraint di foreignkey legati alla tabella
//    //          fkCommand = new OracleCommand
//    //              (
//    //              string.Format
//    //              (
//    //                  @"SELECT CONSTRAINT_NAME, R_CONSTRAINT_NAME FROM ALL_CONSTRAINTS 
//    //WHERE TABLE_NAME = '{0}' AND OWNER = '{1}' AND CONSTRAINT_TYPE = 'R'",
//    //                  tableName.ToUpper(CultureInfo.InvariantCulture),
//    //                  tbConnection.SchemaOwner
//    //                  ),
//    //              tbConnection.OracleConnect
//    //              );

//    //          fkReader = fkCommand.ExecuteReader();

//    //          if (!fkReader.HasRows)
//    //          {
//    //              fkReader.Close();
//    //              fkCommand.Dispose();
//    //              return null;
//    //          }

//    //          // estraggo le colonne che determinano la foreign key della relazione
//    //          fkColCommand = new OracleCommand
//    //              (
//    //              string.Format
//    //              (
//    //                  @"SELECT DISTINCT COLUMN_NAME, POSITION FROM ALL_CONS_COLUMNS 
//    //WHERE TABLE_NAME = '{0}' AND OWNER = '{1}' AND CONSTRAINT_NAME = :FkConstr 
//    //ORDER BY POSITION",
//    //                  tableName.ToUpper(CultureInfo.InvariantCulture),
//    //                  tbConnection.SchemaOwner
//    //              ),
//    //              tbConnection.OracleConnect
//    //              );

//    //          fkColCommand.Parameters.Add(":FkConstr", OracleType.VarChar);
//    //          fkColCommand.Prepare();

//    //          // estraggo le colonne che determinano la primary key della relazione
//    //          pkColCommand = new OracleCommand
//    //              (
//    //              string.Format
//    //              (
//    //                  @"SELECT DISTINCT CONSTRAINT_NAME, TABLE_NAME, COLUMN_NAME, POSITION FROM ALL_CONS_COLUMNS 
//    //WHERE OWNER = '{0}' AND CONSTRAINT_NAME = :PKConstr ORDER BY POSITION",
//    //                  tbConnection.SchemaOwner
//    //                  ),
//    //              tbConnection.OracleConnect
//    //              );

//    //          pkColCommand.Parameters.Add(":PKConstr", OracleType.VarChar);
//    //          pkColCommand.Prepare();

//    //          while (fkReader.Read())
//    //          {
//    //              fkColCommand.Parameters[":FkConstr"].Value = fkReader["CONSTRAINT_NAME"].ToString(); ;
//    //              colReader = fkColCommand.ExecuteReader();
//    //              if (!colReader.HasRows)
//    //              {
//    //                  fkReader.Close();
//    //                  fkCommand.Dispose();
//    //                  colReader.Close();
//    //                  fkColCommand.Dispose();
//    //                  return null;
//    //              }

//    //              DataRow row = null;
//    //              while (colReader.Read())
//    //              {
//    //                  row = dataTable.NewRow();
//    //                  row[DBSchemaStrings.Name] = fkReader["CONSTRAINT_NAME"].ToString();
//    //                  row[DBSchemaStrings.PKTableName] = fkReader["R_CONSTRAINT_NAME"].ToString(); ; //da sostituire con il nome della tabella refereziata
//    //                  row[DBSchemaStrings.Schema] = tbConnection.SchemaOwner;
//    //                  row[DBSchemaStrings.FKColumn] = colReader["COLUMN_NAME"].ToString();
//    //                  row[DBSchemaStrings.PKColumn] = string.Empty;
//    //                  row[DBSchemaStrings.Position] = (int)((decimal)colReader["POSITION"]);
//    //                  dataTable.Rows.Add(row);
//    //              }

//    //              colReader.Close();
//    //              pkColCommand.Parameters[":PKConstr"].Value = fkReader["R_CONSTRAINT_NAME"].ToString(); ;
//    //              colReader = pkColCommand.ExecuteReader();

//    //              if (!colReader.HasRows)
//    //              {
//    //                  fkReader.Close();
//    //                  fkCommand.Dispose();
//    //                  colReader.Close();
//    //                  fkColCommand.Dispose();
//    //                  pkColCommand.Dispose();
//    //                  return null;
//    //              }

//    //              while (colReader.Read())
//    //              {
//    //                  if (count >= dataTable.Rows.Count)
//    //                  {
//    //                      fkReader.Close();
//    //                      fkCommand.Dispose();
//    //                      colReader.Close();
//    //                      fkColCommand.Dispose();
//    //                      pkColCommand.Dispose();
//    //                      return null;
//    //                  }

//    //                  row = dataTable.Rows[count];
//    //                  if (row[DBSchemaStrings.PKTableName].ToString() == colReader["CONSTRAINT_NAME"].ToString() &&
//    //                      (int)row[DBSchemaStrings.Position] == (int)((decimal)colReader["POSITION"]))
//    //                  {
//    //                      row[DBSchemaStrings.PKTableName] = colReader["TABLE_NAME"].ToString();
//    //                      row[DBSchemaStrings.PKColumn] = colReader["COLUMN_NAME"].ToString();
//    //                  }
//    //                  count++;
//    //              }
//    //          }
//    //      }
//    //      catch (OracleException e)
//    //      {
//    //          throw (new TBException(e.Message, e));
//    //      }

//    //      return dataTable;
//    //  }

//    /// <summary>
//    /// ORACLE
//    /// carica le informazioni relative alla foreign key constraints relative alla tabella 
//    /// </summary>	
//    //---------------------------------------------------------------------------
//    //  private FKDataTable LoadOracleFKConstraints(string tableName)
//    //  {
//    //      OracleDataReader reader = null;
//    //      OracleCommand command = null;
//    //      FKDataTable dataTable = new FKDataTable();

//    //      try
//    //      {
//    //          command = new OracleCommand
//    //              (
//    //              string.Format
//    //              (
//    //                  @"SELECT r.CONSTRAINT_NAME FKName, f.TABLE_NAME TableName 
//    //FROM ALL_CONSTRAINTS r, ALL_CONSTRAINTS f 
//    //WHERE r.CONSTRAINT_TYPE  = 'R' AND f.CONSTRAINT_NAME = r.R_CONSTRAINT_NAME AND
//    //r.TABLE_NAME  = '{0}' AND r.OWNER = '{1}'",
//    //                  tableName.ToUpper(CultureInfo.InvariantCulture),
//    //                  tbConnection.SchemaOwner
//    //                  ),
//    //              tbConnection.OracleConnect
//    //              );

//    //          reader = command.ExecuteReader();

//    //          DataRow row = null;
//    //          while (reader.Read())
//    //          {
//    //              row = dataTable.NewRow();
//    //              row[0] = reader["FKName"].ToString();
//    //              row[1] = reader["TableName"].ToString();
//    //              row[2] = tbConnection.SchemaOwner;
//    //              dataTable.Rows.Add(row);
//    //          }
//    //      }
//    //      catch (OracleException e)
//    //      {
//    //          throw (new TBException(e.Message, e));
//    //      }
//    //      finally
//    //      {
//    //          if (reader != null && !reader.IsClosed)
//    //              reader.Close();
//    //          command.Dispose();
//    //      }

//    //      return dataTable;
//    //  }


//    /**
//    * Postgre load fk info
//    * */
//    //---------------------------------------------------------------------------
//    //public DataTable LoadPostgreFkConstraintsInfo(string tableName)
//    //{
//    //    NpgsqlDataReader reader = null;
//    //    NpgsqlCommand command = null;
//    //    FKDataTable dtFKs = new FKDataTable();

//    //    string query = @"SELECT
//    //                            tc.constraint_name as fk_name,  kcu.column_name as fk_column_name, 
//    //                            ccu.table_name AS pk_table_name,
//    //                            ccu.column_name AS pk_column_name 
//    //                        FROM 
//    //                            information_schema.table_constraints AS tc 
//    //                            JOIN information_schema.key_column_usage AS kcu
//    //                              ON tc.constraint_name = kcu.constraint_name
//    //                            JOIN information_schema.constraint_column_usage AS ccu
//    //                              ON ccu.constraint_name = tc.constraint_name
//    //                        WHERE constraint_type = 'FOREIGN KEY' AND tc.table_name=@tablename;

//    //                        ";

//    //    try
//    //    {
//    //        command = new NpgsqlCommand(query, tbConnection.PostgreConnect);
//    //        command.Parameters.AddWithValue("@tablename", tableName.ToLower());
//    //        reader = command.ExecuteReader();

//    //        DataRow row = null;
//    //        while (reader.Read())
//    //        {
//    //            row = dtFKs.NewRow();
//    //            row[DBSchemaStrings.Name] = reader["fk_name"].ToString();
//    //            row[DBSchemaStrings.PKTableName] = reader["pk_table_name"].ToString();
//    //            row[DBSchemaStrings.FKColumn] = reader["fk_column_name"].ToString();
//    //            row[DBSchemaStrings.PKColumn] = reader["pk_column_name"].ToString();
//    //            dtFKs.Rows.Add(row);
//    //        }


//    //        //// cambio il nome alle colonne con gli stessi nomi utlizzati per ORACLE
//    //        //dtFKs.Columns["FK_NAME"].ColumnName = DBSchemaStrings.Name;
//    //        //dtFKs.Columns["PK_TABLE_NAME"].ColumnName = DBSchemaStrings.PKTableName;
//    //        //dtFKs.Columns["FK_COLUMN_NAME"].ColumnName = DBSchemaStrings.FKColumn;
//    //        //dtFKs.Columns["PK_COLUMN_NAME"].ColumnName = DBSchemaStrings.PKColumn;


//    //        return dtFKs;
//    //    }
//    //    catch (NpgsqlException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null && !reader.IsClosed)
//    //            reader.Close();
//    //        command.Dispose();
//    //    }
//    //}

//    /// <summary>
//    /// Postgre
//    /// carica le informazioni relative alla foreign key constraints relative alla tabella 
//    /// </summary>	
//    //---------------------------------------------------------------------------
//    //private FKDataTable LoadPostgreFKConstraints(string tableName)
//    //{
//    //    NpgsqlDataReader reader = null;
//    //    NpgsqlCommand command = null;
//    //    FKDataTable dataTable = new FKDataTable();

//    //    string query = string.Format(@"SELECT 
//    //                            tc.constraint_name AS FKName, 
//    //                            ccu.table_name AS TableName

//    //                    FROM 
//    //                            information_schema.table_constraints AS tc 

//    //                    JOIN 
//    //                            information_schema.constraint_column_usage AS ccu
//    //                    ON 
//    //                            ccu.constraint_name = tc.constraint_name
//    //                    WHERE 
//    //                            constraint_type = 'FOREIGN KEY' AND 
//    //                            tc.table_name=@tablename AND 
//    //                            tc.constraint_schema='{0}'", DatabaseLayerConsts.postgreDefaultSchema);


//    //    try
//    //    {
//    //        command = new NpgsqlCommand(query, tbConnection.PostgreConnect);
//    //        command.Parameters.AddWithValue("@tablename", tableName.ToLower());
//    //        reader = command.ExecuteReader();

//    //        DataRow row = null;
//    //        while (reader.Read())
//    //        {
//    //            row = dataTable.NewRow();
//    //            row[0] = reader["fkname"].ToString();
//    //            row[1] = reader["tablename"].ToString(); ;
//    //            row[2] = tbConnection.Database;
//    //            dataTable.Rows.Add(row);
//    //        }
//    //    }
//    //    catch (NpgsqlException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null && !reader.IsClosed)
//    //            reader.Close();
//    //        command.Dispose();
//    //    }

//    //    return dataTable;
//    //}


//    /// <summary>
//    /// Generico metodo LoadRefFKConstraints per caricare le informazioni relative 
//    /// ai foreign key constraints che riferiscono alla tabella.
//    /// A seconda del tipo di connessione vengono interrogate direttamente 
//    /// le tabelle di sistema del specifico server di database.
//    /// </summary>	
//    //---------------------------------------------------------------------------
//    //public FKDataTable LoadRefFKConstraints(string refTableName)
//    //{
//    //    try
//    //    {
//    //        if (tbConnection.IsSqlConnection())
//    //            return LoadRefSqlFKConstraints(refTableName);
//    //        else if (tbConnection.IsOracleConnection())
//    //            return LoadRefOracleFKConstraints(refTableName);
//    //        else if (tbConnection.IsPostgreConnection())
//    //            return LoadRefPostgreFKConstraints(refTableName);

//    //        return null;
//    //    }
//    //    catch (TBException)
//    //    {
//    //        throw;
//    //    }
//    //}

//    ///<summary>
//    /// SQL SERVER
//    ///	carica le informazioni relative alla foreign key constraints che riferiscono la tabella
//    ///</summary>
//    //---------------------------------------------------------------------------
//    //    private FKDataTable LoadRefSqlFKConstraints(string refTableName)
//    //    {
//    //        SqlDataReader reader = null;
//    //        SqlCommand command = null;
//    //        FKDataTable dataTable = new FKDataTable();

//    //        string query =
//    //            @"SELECT DISTINCT X.name AS FKName, Y.name AS TableName 
//    //FROM sysobjects X, sysobjects Y, sysforeignkeys F 
//    //WHERE X.id = F.constid AND F.fkeyid = Y.id AND F.rkeyid = 
//    //(SELECT id FROM sysobjects WHERE name = @tablename AND xtype = @type)";

//    //        try
//    //        {
//    //            command = new SqlCommand(query, tbConnection.SqlConnect);
//    //            command.Parameters.AddWithValue("@tablename", refTableName);
//    //            command.Parameters.AddWithValue("@type", "U");

//    //            reader = command.ExecuteReader();

//    //            DataRow row = null;
//    //            while (reader.Read())
//    //            {
//    //                row = dataTable.NewRow();
//    //                row[0] = reader["FKName"].ToString();
//    //                row[1] = reader["TableName"].ToString();
//    //                row[2] = tbConnection.Database;
//    //                dataTable.Rows.Add(row);
//    //            }
//    //        }
//    //        catch (SqlException e)
//    //        {
//    //            throw (new TBException(e.Message, e));
//    //        }
//    //        finally
//    //        {
//    //            if (reader != null && !reader.IsClosed)
//    //                reader.Close();
//    //            command.Dispose();
//    //        }
//    //        return dataTable;
//    //    }

//    ///<summary>
//    /// ORACLE
//    ///	carica le informazioni relative alla foreign key constraints che riferiscono la tabella
//    ///</summary>
//    //---------------------------------------------------------------------------
//    // private FKDataTable LoadRefOracleFKConstraints(string refTableName)
//    // {
//    //     OracleDataReader reader = null;
//    //     OracleCommand command = null;
//    //     FKDataTable dataTable = new FKDataTable();

//    //     try
//    //     {
//    //         command = new OracleCommand
//    //             (
//    //                 string.Format
//    //                 (
//    //                     @"select X.CONSTRAINT_NAME FKName, X.TABLE_NAME TableName from ALL_CONSTRAINTS X 
//    //where CONSTRAINT_TYPE = 'R' and OWNER ='{0}' and 
//    //EXISTS (Select TABLE_NAME from ALL_CONSTRAINTS Y 
//    //where Y.CONSTRAINT_TYPE = 'P' and Y.OWNER = X.OWNER and 
//    //X.R_CONSTRAINT_NAME = Y.CONSTRAINT_NAME and Y.TABLE_NAME = '{1}')",
//    //                     tbConnection.SchemaOwner,
//    //                     refTableName.ToUpper(CultureInfo.InvariantCulture)
//    //                 ),
//    //                 tbConnection.OracleConnect
//    //             );

//    //         reader = command.ExecuteReader();

//    //         DataRow row = null;
//    //         while (reader.Read())
//    //         {
//    //             row = dataTable.NewRow();
//    //             row[0] = reader["FKName"].ToString();
//    //             row[1] = reader["TableName"].ToString();
//    //             row[2] = tbConnection.SchemaOwner;
//    //             dataTable.Rows.Add(row);
//    //         }
//    //     }
//    //     catch (OracleException e)
//    //     {
//    //         throw (new TBException(e.Message, e));
//    //     }
//    //     finally
//    //     {
//    //         if (reader != null && !reader.IsClosed)
//    //             reader.Close();
//    //         command.Dispose();
//    //     }
//    //     return dataTable;
//    // }


//    //<summary>
//    /// SQL SERVER
//    ///	carica le informazioni relative alla foreign key constraints che riferiscono la tabella
//    ///</summary>
//    //---------------------------------------------------------------------------
//    //private FKDataTable LoadRefPostgreFKConstraints(string tableName)
//    //{
//    //    NpgsqlDataReader reader = null;
//    //    NpgsqlCommand command = null;
//    //    FKDataTable dataTable = new FKDataTable();

//    //    string query = string.Format(@"SELECT 
//    //                            tc.constraint_name AS FKName, 
//    //                            ccu.table_name AS TableName

//    //                    FROM 
//    //                            information_schema.table_constraints AS tc 

//    //                    JOIN 
//    //                            information_schema.constraint_column_usage AS ccu
//    //                    ON 
//    //                            ccu.constraint_name = tc.constraint_name
//    //                    WHERE 
//    //                            constraint_type = 'FOREIGN KEY' AND 
//    //                            ccu.table_name='{1}' AND 
//    //                            tc.constraint_schema='{0}'", DatabaseLayerConsts.postgreDefaultSchema, tableName.ToLower());


//    //    try
//    //    {
//    //        command = new NpgsqlCommand(query, tbConnection.PostgreConnect);
//    //        // command.Parameters.AddWithValue("@tablename", tableName.ToLower());
//    //        reader = command.ExecuteReader();

//    //        DataRow row = null;
//    //        while (reader.Read())
//    //        {
//    //            row = dataTable.NewRow();
//    //            row[0] = reader["fkname"].ToString();
//    //            row[1] = reader["tablename"].ToString(); ;
//    //            row[2] = tbConnection.Database;
//    //            dataTable.Rows.Add(row);
//    //        }
//    //    }
//    //    catch (NpgsqlException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (reader != null && !reader.IsClosed)
//    //            reader.Close();
//    //        command.Dispose();
//    //    }

//    //    return dataTable;
//    //}
//    #endregion

//    #region metodi necessari per il caricamento Indici in caso di connessione Oracle
//    ///<summary>
//    /// metodo utilizzato per ORACLE al posto del metodo basato su connessione OLEDB: LoadIndexes
//    /// questo a causa dell'enorme lentezza in caso di connessione ORACLE
//    ///</summary>
//    //---------------------------------------------------------------------------
//    //   public IdxDataTable LoadOracleIndexesInfo(string tableName)
//    //   {
//    //       OracleCommand idxCommand = null;
//    //       OracleDataReader idxReader = null;

//    //       IdxDataTable dataTable = new IdxDataTable();

//    //       string idxText = string.Format
//    //           (
//    //               @"SELECT c.INDEX_NAME, i.UNIQUENESS, c.COLUMN_NAME, c.COLUMN_POSITION,
//    //(SELECT COUNT(*) FROM ALL_CONSTRAINTS p WHERE p.CONSTRAINT_NAME = c.INDEX_NAME AND 
//    //p.CONSTRAINT_TYPE = 'P' AND p.OWNER = i.OWNER) AS pkNumb
//    //FROM ALL_INDEXES i, ALL_IND_COLUMNS c 
//    //WHERE i.OWNER = '{0}' AND i.TABLE_NAME = '{1}' 
//    //AND c.INDEX_OWNER = i.OWNER AND c.INDEX_NAME = i.INDEX_NAME 
//    //ORDER BY c.INDEX_NAME, c.COLUMN_POSITION",
//    //               tbConnection.SchemaOwner,
//    //               tableName.ToUpper(CultureInfo.InvariantCulture)
//    //           );

//    //       try
//    //       {
//    //           idxCommand = new OracleCommand(idxText, tbConnection.OracleConnect);
//    //           idxReader = idxCommand.ExecuteReader();

//    //           if (!idxReader.HasRows)
//    //           {
//    //               idxReader.Close();
//    //               idxCommand.Dispose();
//    //               return dataTable;
//    //           }

//    //           DataRow row = null;
//    //           while (idxReader.Read())
//    //           {
//    //               row = dataTable.NewRow();
//    //               row[DBSchemaStrings.Name] = idxReader["INDEX_NAME"].ToString();
//    //               row[DBSchemaStrings.PrimaryKey] = ((decimal)idxReader["PKNUMB"] > 0) ? true : false;
//    //               row[DBSchemaStrings.Unique] = (idxReader["UNIQUENESS"].ToString()) == "UNIQUE" ? true : false;
//    //               row[DBSchemaStrings.Clustered] = false;
//    //               row[DBSchemaStrings.ColumnName] = idxReader["COLUMN_NAME"].ToString();
//    //               dataTable.Rows.Add(row);
//    //           }
//    //       }
//    //       catch (OracleException e)
//    //       {
//    //           throw (new TBException(e.Message, e));
//    //       }
//    //       finally
//    //       {
//    //           if (idxReader != null && !idxReader.IsClosed)
//    //               idxReader.Close();
//    //           idxCommand.Dispose();
//    //       }

//    //       return dataTable;
//    //   }
//    #endregion

//    ///</summary>
//    //---------------------------------------------------------------------------
//    //public IdxDataTable LoadPostgreIndexesInfo(string tableName)
//    //{
//    //    NpgsqlCommand idxCommand = null;
//    //    NpgsqlDataReader idxReader = null;

//    //    IdxDataTable dataTable = new IdxDataTable();

//    //    string idxText = string.Format(@"SELECT 
//    //                            i.relname as INDEX_NAME, 
//    //                            idx.indisprimary as PKNUMB,
//    //                            idx.indisunique as UNIQUENESS,
//    //                            idx.indisclustered as CLUSTERED,
   
//    //                          ARRAY(
//    //                          SELECT pg_get_indexdef(idx.indexrelid, k + 1, true)
//    //                          FROM generate_subscripts(idx.indkey, 1) as k
//    //                          ORDER BY k
//    //                          ) as COLUMN_NAME
//    //                          FROM   pg_index as idx
//    //                          JOIN   pg_class as i
//    //                          ON     i.oid = idx.indexrelid
//    //                          JOIN   pg_namespace as ns
//    //                          ON     ns.oid = i.relnamespace
//    //                          AND    ns.nspname ='{0}'
//    //                          WHERE  idx.indrelid::regclass = @tableName::regclass", DatabaseLayerConsts.postgreDefaultSchema);

//    //    try
//    //    {
//    //        idxCommand = new NpgsqlCommand(idxText, tbConnection.PostgreConnect);
//    //        idxCommand.Parameters.Add("@tableName", tableName);
//    //        idxCommand.Parameters.AddWithValue("@tableName", tableName);
//    //        idxReader = idxCommand.ExecuteReader();

//    //        if (!idxReader.HasRows)
//    //        {
//    //            idxReader.Close();
//    //            idxCommand.Dispose();
//    //            return dataTable;
//    //        }

//    //        DataRow row = null;
//    //        while (idxReader.Read())
//    //        {
//    //            row = dataTable.NewRow();
//    //            row[DBSchemaStrings.Name] = idxReader["INDEX_NAME"].ToString();
//    //            row[DBSchemaStrings.PrimaryKey] = idxReader["PKNUMB"].ToString();
//    //            row[DBSchemaStrings.Unique] = idxReader["UNIQUENESS"].ToString();
//    //            row[DBSchemaStrings.Clustered] = idxReader["CLUSTERED"].ToString();
//    //            row[DBSchemaStrings.ColumnName] = idxReader["COLUMN_NAME"];//non funziona in portgre. da rivedere
//    //            dataTable.Rows.Add(row);
//    //        }
//    //    }
//    //    catch (OracleException e)
//    //    {
//    //        throw (new TBException(e.Message, e));
//    //    }
//    //    finally
//    //    {
//    //        if (idxReader != null && !idxReader.IsClosed)
//    //            idxReader.Close();
//    //        idxCommand.Dispose();
//    //    }

//    //    return dataTable;
//    //}
//}

//#region Class TBOracleAdminFunction
///// <summary>
///// permette di richiamare funzionalità di amministrazione per la gestione dei sinonimi
///// La connessione passata è una connessione effettuata dal database administrator
///// </summary>
////============================================================================
////public class TBOracleAdminFunction
////    {
////        private OracleConnection oraConnection = null;
////        private StringCollection grantCollection = null;

////        //-------------------------------------------------------------------------
////        public TBOracleAdminFunction(OracleConnection connect)
////        {
////            oraConnection = connect;

////            grantCollection = new StringCollection();
////            grantCollection.Add("INSERT");
////            grantCollection.Add("UPDATE");
////            grantCollection.Add("DELETE");
////            grantCollection.Add("SELECT");
////        }

//        /// <summary>
//        /// Restituisce tutti gli utenti che non hanno sinonimo TB_DBMARK presenti nel database Oracle
//        /// </summary>
//        /// <returns></returns> 
//        //-----------------------------------------------------------------------
//     //   public UserDataTable GetFreeDBUsersForAttach()
//     //   {
//     //       OracleDataReader reader = null;
//     //       OracleCommand command = null;
//     //       UserDataTable dataTable = new UserDataTable();

//     //       try
//     //       {
//     //           string query = string.Format
//     //               (
//     //               @"select USERNAME, PASSWORD from DBA_USERS where ACCOUNT_STATUS = 'OPEN' and 
//					//INITIAL_RSRC_CONSUMER_GROUP != 'SYS_GROUP' and USERNAME != 'SYSMAN' and 
//					//not exists (select SYNONYM_NAME from ALL_SYNONYMS where OWNER = USERNAME and SYNONYM_NAME = '{0}') ",
//     //               DBSchemaStrings.TBDBMark
//     //               );

//     //           command = new OracleCommand(query, oraConnection);
//     //           reader = command.ExecuteReader();

//     //           DataRow row = null;
//     //           while (reader.Read())
//     //           {
//     //               if (string.Compare(reader["USERNAME"].ToString(), "SYSTEM", true, CultureInfo.InvariantCulture) == 0 ||
//     //                   string.Compare(reader["USERNAME"].ToString(), "SYS", true, CultureInfo.InvariantCulture) == 0)
//     //                   continue;

//     //               row = dataTable.NewRow();
//     //               row[0] = reader["USERNAME"].ToString();
//     //               row[1] = (string.Compare(reader["PASSWORD"].ToString(), "EXTERNAL") == 0);
//     //               dataTable.Rows.Add(row);
//     //           }
//     //       }
//     //       catch (OracleException e)
//     //       {
//     //           throw (new TBException(e.Message, e));
//     //       }
//     //       finally
//     //       {
//     //           if (reader != null)
//     //               reader.Close();
//     //           command.Dispose();
//     //       }

//     //       return dataTable;
//     //   }

//        /// <summary>
//        /// Restituisce tutti gli utenti che non hanno sinonimo TB_DBMARK presenti nel database Oracle
//        /// </summary>
//        /// <returns></returns> 
//        //-----------------------------------------------------------------------
//    //    public UserDataTable GetFreeDBUsersForAssociation()
//    //    {
//    //        OracleDataReader reader = null;
//    //        OracleCommand command = null;
//    //        UserDataTable dataTable = new UserDataTable();

//    //        try
//    //        {
//    //            string query = string.Format(
//    //            @"select USERNAME, PASSWORD from DBA_USERS where ACCOUNT_STATUS = 'OPEN' and 
//				//INITIAL_RSRC_CONSUMER_GROUP != 'SYS_GROUP' and USERNAME != 'SYSMAN' and 
//				//not exists (select SYNONYM_NAME from ALL_SYNONYMS where OWNER = USERNAME and SYNONYM_NAME = '{0}') and 
//				//(not exists (select TABLE_NAME from ALL_TABLES where OWNER = USERNAME and TABLE_NAME = '{0}'))", DBSchemaStrings.TBDBMark);

//    //            command = new OracleCommand(query, oraConnection);

//    //            reader = command.ExecuteReader();

//    //            DataRow row = null;
//    //            while (reader.Read())
//    //            {
//    //                if (string.Compare(reader["USERNAME"].ToString(), "SYSTEM", true, CultureInfo.InvariantCulture) == 0 ||
//    //                    string.Compare(reader["USERNAME"].ToString(), "SYS", true, CultureInfo.InvariantCulture) == 0)
//    //                    continue;

//    //                row = dataTable.NewRow();
//    //                row[0] = reader["USERNAME"].ToString();
//    //                row[1] = (string.Compare(reader["PASSWORD"].ToString(), "EXTERNAL") == 0);
//    //                dataTable.Rows.Add(row);
//    //            }
//    //        }
//    //        catch (OracleException e)
//    //        {
//    //            throw (new TBException(e.Message, e));
//    //        }
//    //        finally
//    //        {
//    //            if (reader != null)
//    //                reader.Close();
//    //            command.Dispose();
//    //        }

//    //        return dataTable;
//    //    }

//        /// <summary>
//        /// Restituisce tutti gli utenti che hanno almeno un synonym allo schema passato come parametro
//        /// più il dbowner (fare il filtraggio con TB_DBMark)
//        /// </summary>
//        /// <returns></returns>
//        //-----------------------------------------------------------------------
//      //  public UserDataTable GetAllUsersToSchema(string schemaName)
//      //  {
//      //      if (schemaName.Length == 0)
//      //          throw (new TBException("Request owner schema but it is empty"));

//      //      OracleDataReader reader = null;
//      //      OracleCommand command = null;
//      //      UserDataTable dataTable = new UserDataTable();

//      //      try
//      //      {
//      //          string query = string.Format
//      //              (
//      //                  @"Select USERNAME, PASSWORD from ALL_SYNONYMS, DBA_USERS 
//						//where TABLE_OWNER = '{0}' and OWNER = USERNAME and TABLE_NAME = '{1}' 
//						//union
//						//Select USERNAME, PASSWORD from  DBA_USERS where USERNAME = '{0}'",
//      //                  schemaName.ToUpper(CultureInfo.InvariantCulture),
//      //                  DBSchemaStrings.TBDBMark
//      //              );
//      //          command = new OracleCommand(query, oraConnection);
//      //          reader = command.ExecuteReader();

//      //          DataRow row = null;
//      //          while (reader.Read())
//      //          {
//      //              row = dataTable.NewRow();
//      //              row[0] = reader["USERNAME"].ToString();
//      //              row[1] = (string.Compare(reader["PASSWORD"].ToString(), "EXTERNAL") == 0);
//      //              dataTable.Rows.Add(row);
//      //          }
//      //      }
//      //      catch (OracleException e)
//      //      {
//      //          throw (new TBException(e.Message, e));
//      //      }
//      //      finally
//      //      {
//      //          if (reader != null)
//      //              reader.Close();
//      //          command.Dispose();
//      //      }

//      //      return dataTable;
//      //  }

//        /// <summary>
//        /// restituisce tutti i sinonimi di proprietà di userName
//        /// </summary>
//        /// <param name="ownerName"></param>
//        /// <returns></returns>
//        //-----------------------------------------------------------------------
//        //private SchemaDataTable GetAllSynonymsForUser(string userName, string ownerName)
//        //{
//        //    OracleDataReader reader = null;
//        //    OracleCommand command = null;
//        //    SchemaDataTable dataTable = new SchemaDataTable();

//        //    try
//        //    {
//        //        string query = string.Format
//        //            (
//        //                "select SYNONYM_NAME from ALL_SYNONYMS where OWNER  = '{0}' and TABLE_OWNER = '{1}'",
//        //                userName.ToUpper(CultureInfo.InvariantCulture),
//        //                ownerName.ToUpper(CultureInfo.InvariantCulture)
//        //            );

//        //        command = new OracleCommand(query, oraConnection);
//        //        reader = command.ExecuteReader();

//        //        DataRow row = null;
//        //        while (reader.Read())
//        //        {
//        //            row = dataTable.NewRow();
//        //            row[0] = reader["SYNONYM_NAME"].ToString();
//        //            row[1] = ownerName;
//        //            row[2] = DBObjectTypes.SYNONYM;
//        //            dataTable.Rows.Add(row);
//        //        }
//        //    }
//        //    catch (OracleException e)
//        //    {
//        //        throw (new TBException(e.Message, e));
//        //    }
//        //    finally
//        //    {
//        //        if (reader != null)
//        //            reader.Close();
//        //        command.Dispose();
//        //    }

//        //    return dataTable;
//        //}

//        /// <summary>
//        /// restituisce tutti gli oggetti di proprietà di ownerName
//        /// </summary>
//        /// <param name="ownerName"></param>
//        /// <returns></returns>
//        //-----------------------------------------------------------------------
//        //public SchemaDataTable GetAllObjectsForOwner(string ownerName)
//        //{
//        //    OracleCommand command = null;
//        //    SchemaDataTable dataTable = new SchemaDataTable();

//        //    //non posso fare la UNION a causa del campo TEXT di USER_VIEWS e di USER_SOURCE
//        //    // che è di tipo LONG e il tipo LONG non si può utilizzare nelle espressioni e nelle UNION
//        //    string query = string.Format("select TABLE_NAME NAME, '' TEXT, 'T' TYPE from ALL_TABLES where OWNER = '{0}'", ownerName.ToUpper(CultureInfo.InvariantCulture));
//        //    string query1 = string.Format("select VIEW_NAME NAME, TEXT, 'V' TYPE from ALL_VIEWS where OWNER = '{0}'", ownerName.ToUpper(CultureInfo.InvariantCulture));
//        //    string query2 = string.Format("select NAME, TEXT, TYPE from ALL_SOURCE where TYPE = 'PROCEDURE' and OWNER = '{0}' order by NAME, LINE", ownerName.ToUpper(CultureInfo.InvariantCulture));

//        //    try
//        //    {
//        //        command = new OracleCommand(query, oraConnection);
//        //        TBDatabaseSchema.AddRows(command, ref dataTable, ownerName);
//        //        command.CommandText = query1;
//        //        TBDatabaseSchema.AddRows(command, ref dataTable, ownerName);
//        //        command.CommandText = query2;
//        //        TBDatabaseSchema.AddRows(command, ref dataTable, ownerName);
//        //    }
//        //    catch (OracleException e)
//        //    {
//        //        throw (new TBException(e.Message, e));
//        //    }
//        //    finally
//        //    {
//        //        command.Dispose();
//        //    }

//        //    return dataTable;
//        //}

//        /// <summary>
//        /// crea il singolo sinonimo
//        /// </summary>
//        /// <param name="command"></param>
//        /// <param name="name">nome del sinonimo che è uguale al nome della table|view|sp su cui va creato</param>
//        /// <param name="owner">table|view|sp owner</param>
//        //-----------------------------------------------------------------------
//        //private void CreateSynonym(OracleCommand command, string tableName, string schemaUser, string schemaOwner)
//        //{
//        //    command.CommandText =
//        //        string.Format
//        //        (
//        //            "CREATE SYNONYM \"{0}\".\"{1}\" FOR \"{2}\".\"{1}\"",
//        //            schemaUser.ToUpper(CultureInfo.InvariantCulture),
//        //            tableName.ToUpper(CultureInfo.InvariantCulture),
//        //            schemaOwner.ToUpper(CultureInfo.InvariantCulture)
//        //        );

//        //    try
//        //    {
//        //        command.ExecuteNonQuery();
//        //    }
//        //    catch (OracleException)
//        //    {
//        //        throw;
//        //    }
//        //}

//        /// <summary>
//        /// l'utente owner dello schema deve dare i grant di INSERT, SELECT, UPDATE e DELETE sulle tabelle/view
//        /// all'utente che le utilizza mediante i sinonimi
//        /// </summary>
//        /// <param name="command"></param>
//        /// <param name="name"></param>
//        /// <param name="owner"></param>
//        //-----------------------------------------------------------------------
//        //private void GrantToTable(OracleCommand command, string tableName, string ownerName, string userName)
//        //{
//        //    foreach (string grant in grantCollection)
//        //    {
//        //        command.CommandText = string.Format
//        //            (
//        //                "GRANT {0} ON  \"{1}\".\"{2}\" TO \"{3}\"",
//        //                grant.ToString(),
//        //                ownerName.ToUpper(CultureInfo.InvariantCulture),
//        //                tableName.ToUpper(CultureInfo.InvariantCulture),
//        //                userName.ToUpper(CultureInfo.InvariantCulture)
//        //            );

//        //        try
//        //        {
//        //            command.ExecuteNonQuery();
//        //        }
//        //        catch (OracleException)
//        //        {
//        //            throw;
//        //        }
//        //    }
//        //}

//        /// <summary>
//        /// l'utente owner dello schema deve dare il grant di EXECUTE sulle stored procedure
//        /// all'utente che le utilizza mediante i sinonimi
//        /// </summary>
//        /// <param name="command"></param>
//        /// <param name="name"></param>
//        /// <param name="owner"></param>
//        //-----------------------------------------------------------------------
//        //private void GrantToProcedure(OracleCommand command, string tableName, string ownerName, string userName)
//        //{
//        //    command.CommandText = string.Format
//        //        (
//        //        "GRANT EXECUTE ON  \"{0}\".\"{1}\" TO \"{2}\"",
//        //        ownerName,
//        //        tableName.ToUpper(CultureInfo.InvariantCulture),
//        //        userName
//        //        );

//        //    try
//        //    {
//        //        command.ExecuteNonQuery();
//        //    }
//        //    catch (OracleException)
//        //    {
//        //        throw;
//        //    }
//        //}

//        /// <summary>
//        /// Connessione come sysdba, devo creare i sinonimi dell'utente userName sullo schema ownerName
//        /// </summary>
//        /// <param name="userName">utente utilizzatore</param>
//        /// <param name="pwd">password utente utilizzatore</param>
//        //-----------------------------------------------------------------------
//        //public void CreateMissingSynonyms(string ownerName, string userName)
//        //{
//        //    if (string.Compare(userName, ownerName, true, CultureInfo.InvariantCulture) == 0)
//        //        return;

//        //    SchemaDataTable ownerDataSchema = GetAllObjectsForOwner(ownerName);

//        //    //se lo schema è vuoto allora non faccio niente
//        //    if (ownerDataSchema.Rows.Count <= 0)
//        //        return;

//        //    try
//        //    {
//        //        CreateMissingSynonyms(ownerName, ownerDataSchema, userName);
//        //    }
//        //    catch (TBException)
//        //    {
//        //        throw;
//        //    }
//        //}

//        /// <summary>
//        /// Permette di creare i sinonimi sullo schema delll'utente userName riferiti agli oggetti dello schema
//        /// dell'utente ownerName
//        /// </summary>
//        //-------------------------------------------------------------------------
//        //public void CreateMissingSynonyms(string ownerName, SchemaDataTable ownerDataSchema, string userName)
//        //{
//        //    if (string.Compare(userName, ownerName, true, CultureInfo.InvariantCulture) == 0)
//        //        return;

//        //    SchemaDataTable userDataSchema = null;

//        //    //comando utilizzato per la creazione dei sinonimi(sulla connessione dell'utente dei sinonimi)
//        //    OracleCommand oracleCmd = new OracleCommand();

//        //    try
//        //    {
//        //        userDataSchema = GetAllSynonymsForUser(userName, ownerName);
//        //        oracleCmd.Connection = oraConnection;

//        //        bool bFound;
//        //        foreach (DataRow row in ownerDataSchema.Rows)
//        //        {
//        //            bFound = false;

//        //            foreach (DataRow rowFind in userDataSchema.Rows)
//        //                if (string.Compare(rowFind[DBSchemaStrings.Name].ToString(), row[DBSchemaStrings.Name].ToString(), true, CultureInfo.InvariantCulture) == 0)
//        //                {
//        //                    bFound = true;
//        //                    break;
//        //                }

//        //            if (!bFound)
//        //            {
//        //                CreateSynonym(oracleCmd, row[DBSchemaStrings.Name].ToString(), userName, ownerName);
//        //                if ((DBObjectTypes)row[DBSchemaStrings.Type] == DBObjectTypes.ROUTINE)
//        //                    GrantToProcedure(oracleCmd, row[DBSchemaStrings.Name].ToString(), ownerName, userName);
//        //                else
//        //                    GrantToTable(oracleCmd, row[DBSchemaStrings.Name].ToString(), ownerName, userName);
//        //            }
//        //        }
//        //    }
//        //    catch (OracleException e)
//        //    {
//        //        oracleCmd.Dispose();
//        //        throw new TBException("CreateMissingSynonyms", e);
//        //    }

//        //    catch (TBException)
//        //    {
//        //        oracleCmd.Dispose();
//        //        throw;
//        //    }
//        //    oracleCmd.Dispose();
//        //}

//        //-----------------------------------------------------------------------
//        //public void CreateSynonymOnTable(string ownerName, string tableName, DBObjectTypes type, string userName)
//        //{
//        //    if (string.Compare(userName, ownerName, true, CultureInfo.InvariantCulture) == 0)
//        //        return;

//        //    //comando utilizzato per la creazione dei grant(sulla connessione dell'utente owner)			
//        //    OracleCommand oracleCmd = new OracleCommand();

//        //    try
//        //    {
//        //        //assegno le due diverse connessioni ai comandi
//        //        oracleCmd.Connection = oraConnection;

//        //        CreateSynonym(oracleCmd, tableName, userName, ownerName);

//        //        if (type == DBObjectTypes.ROUTINE)
//        //            GrantToProcedure(oracleCmd, tableName, ownerName, userName);
//        //        else
//        //            GrantToTable(oracleCmd, tableName, ownerName, userName);
//        //    }
//        //    catch (OracleException e)
//        //    {
//        //        oracleCmd.Dispose();
//        //        throw (new TBException("CreateSynonymOnTable", e));
//        //    }
//        //    oracleCmd.Dispose();
//        //}

//        /// <summary>
//        /// cancella il singolo sinonimo
//        /// </summary>
//        /// <param name="userName"></param>
//        /// <param name="pwd"></param>
//        /// <param name="winNtAuthent"></param>
//        /// <returns></returns>
//        //-----------------------------------------------------------------------
//        //private void DropSynonym(OracleCommand command, string userName, string tableName)
//        //{
//        //    command.CommandText =
//        //        string.Format("DROP SYNONYM \"{0}\".\"{1}\"", userName, tableName.ToUpper(CultureInfo.InvariantCulture));

//        //    try
//        //    {
//        //        command.ExecuteNonQuery();
//        //    }
//        //    catch (OracleException)
//        //    {
//        //        throw;
//        //    }
//        //}

//        /// <summary>
//        /// cancella i sinonimi dell'utente userNamerelativi allo schema su cui è stata aperta la connessione
//        /// </summary>
//        /// <param name="userName"></param>
//        /// <param name="pwd"></param>
//        /// <param name="winNtAuthent"></param>
//        /// <returns></returns>
//        //-----------------------------------------------------------------------
//        //public bool DropSynonyms(string ownerName, string userName)
//        //{
//        //    OracleCommand command = new OracleCommand();

//        //    try
//        //    {
//        //        //leggo le informazioni dello schema dell'utente utilizzatore
//        //        SchemaDataTable usrSchema = GetAllSynonymsForUser(userName, ownerName);
//        //        command.Connection = oraConnection;

//        //        foreach (DataRow row in usrSchema.Rows)
//        //            DropSynonym(command, userName, row[DBSchemaStrings.Name].ToString());
//        //    }
//        //    catch (OracleException e)
//        //    {
//        //        command.Dispose();
//        //        throw (new TBException("DropSynonyms", e));
//        //    }

//        //    command.Dispose();
//        //    return true;
//        //}

//        /// <summary>
//        /// cancella il sinonimo alla tabella passata come argomento
//        /// </summary>
//        /// <param name="userName"></param>
//        /// <param name="pwd"></param>
//        /// <param name="winNtAuthent"></param>
//        /// <returns></returns>
//        //-----------------------------------------------------------------------
//        //public bool DropSynonymOnTable(string ownerName, string tableName, string userName)
//        //{
//        //    if (string.Compare(userName, ownerName, true, CultureInfo.InvariantCulture) == 0)
//        //        return true;

//        //    OracleCommand command = new OracleCommand();
//        //    try
//        //    {
//        //        command.Connection = oraConnection;
//        //        DropSynonym(command, userName, tableName);
//        //    }
//        //    catch (OracleException e)
//        //    {
//        //        command.Dispose();
//        //        throw (new TBException("DropSynonymOnTable", e));
//        //    }
//        //    command.Dispose();
//        //    return true;
//        //}

//        //-----------------------------------------------------------------------
//    //    public bool GrantPrivilegesToUser(string privilege, string user)
//    //    {
//    //        OracleCommand command = new OracleCommand();

//    //        try
//    //        {
//    //            command.Connection = oraConnection;
//    //            command.CommandText = string.Format("GRANT {0} TO \"{1}\"", privilege, user);
//    //            command.ExecuteNonQuery();
//    //        }
//    //        catch (OracleException)
//    //        {
//    //            return false;
//    //        }
//    //        finally
//    //        {
//    //            command.Dispose();
//    //        }

//    //        return true;
//    //    }
//    //}
//    #endregion

//    #region Class TBDatabaseSchemaOleDb: legge informazioni di schema utilizzando le classi OLEDB
//    //============================================================================
//    //public class TBDatabaseSchemaOleDb
//    //{
//    //    private OleDbConnection oledbConnect = null;

//    //    //---------------------------------------------------------------------------
//    //    public TBDatabaseSchemaOleDb(OleDbConnection connect)
//    //    {
//    //        oledbConnect = connect;
//    //    }

//    //    //---------------------------------------------------------------------------
//    //    public DataTable LoadIndexes(string tableName)
//    //    {
//    //        DataTable dtIndexes;

//    //        try
//    //        {
//    //            dtIndexes = oledbConnect.GetOleDbSchemaTable
//    //                (
//    //                OleDbSchemaGuid.Indexes,
//    //                new Object[] { null, null, null, null, tableName.ToUpper(CultureInfo.InvariantCulture) }
//    //                );
//    //        }
//    //        catch (OleDbException exception)
//    //        {
//    //            Debug.Fail(exception.Message);
//    //            return null;
//    //        }

//    //        if (dtIndexes.Columns["TABLE_CATALOG"] != null)
//    //            dtIndexes.Columns.Remove("TABLE_CATALOG");
//    //        if (dtIndexes.Columns["TABLE_SCHEMA"] != null)
//    //            dtIndexes.Columns.Remove("TABLE_SCHEMA");
//    //        if (dtIndexes.Columns["TABLE_NAME"] != null)
//    //            dtIndexes.Columns.Remove("TABLE_NAME");
//    //        if (dtIndexes.Columns["INDEX_CATALOG"] != null)
//    //            dtIndexes.Columns.Remove("INDEX_CATALOG");
//    //        if (dtIndexes.Columns["INDEX_SCHEMA"] != null)
//    //            dtIndexes.Columns.Remove("INDEX_SCHEMA");
//    //        if (dtIndexes.Columns["ORDINAL"] != null)
//    //            dtIndexes.Columns.Remove("ORDINAL");

//    //        // cambio il nome alle colonne con gli stessi nomi utlizzati per ORACLE nel metodo LoadOracleIndexes
//    //        dtIndexes.Columns["INDEX_NAME"].ColumnName = DBSchemaStrings.Name;
//    //        dtIndexes.Columns["PRIMARY_KEY"].ColumnName = DBSchemaStrings.PrimaryKey;
//    //        dtIndexes.Columns["UNIQUE"].ColumnName = DBSchemaStrings.Unique;
//    //        dtIndexes.Columns["CLUSTERED"].ColumnName = DBSchemaStrings.Clustered;
//    //        dtIndexes.Columns["COLUMN_NAME"].ColumnName = DBSchemaStrings.ColumnName;

//    //        return dtIndexes;
//    //    }

//        //---------------------------------------------------------------------------
//    //    public DataTable LoadFkConstraintsInfo(string tableName)
//    //    {
//    //        DataTable dtFKs;

//    //        try
//    //        {
//    //            dtFKs = oledbConnect.GetOleDbSchemaTable
//    //                (
//    //                OleDbSchemaGuid.Foreign_Keys,
//    //                new Object[] { null, null, null, null, null, tableName.ToUpper(CultureInfo.InvariantCulture) }
//    //                );
//    //        }
//    //        catch (OleDbException exception)
//    //        {
//    //            Debug.Fail(exception.Message);
//    //            return null;
//    //        }

//    //        // cambio il nome alle colonne con gli stessi nomi utlizzati per ORACLE
//    //        dtFKs.Columns["FK_NAME"].ColumnName = DBSchemaStrings.Name;
//    //        dtFKs.Columns["PK_TABLE_NAME"].ColumnName = DBSchemaStrings.PKTableName;
//    //        dtFKs.Columns["FK_COLUMN_NAME"].ColumnName = DBSchemaStrings.FKColumn;
//    //        dtFKs.Columns["PK_COLUMN_NAME"].ColumnName = DBSchemaStrings.PKColumn;

//    //        return dtFKs;
//    //    }
//    //}
//    //#endregion


//#pragma warning restore 0618
    //}