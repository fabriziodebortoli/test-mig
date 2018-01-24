using System;
using System.Diagnostics;

namespace TaskBuilderNetCore.Data
{
    public enum DBMSType
    {
        UNKNOWN,
        SQLSERVER,
        ORACLE,
        POSTGRE
    }

    public enum DatabaseVersion
    {
        SqlServer2000,
        MSDE,
        Oracle,
        Postgre,
        Undefined,
        All,
        Ndb
    }


    public enum DBNetworkType
	{
		Undefined,
		Large,
		Small
	}

    public class DBSchemaStrings
    {
        public const string Name = "Name";
        public const string Schema = "Schema";
        public const string Definition = "Definition";
        public const string Type = "Type";
        public const string RoutineType = "RoutineType";
        public const string TableName = "TableName";
        public const string UserName = "UserName";
        public const string OSAuthent = "OSAuthent";
        public const string TBDBMark = "TB_DBMARK";
        public const string HasDefault = "HasDefault";
        public const string Default = "Default";
        public const string ColumnName = "ColumnName";
        public const string PKTableName = "PKTableName";
        public const string PKColumn = "PKColumn";
        public const string FKColumn = "FKColumn";
        public const string Position = "Position";
        public const string PrimaryKey = "PrimaryKey";
        public const string Clustered = "Clustered";
        public const string Unique = "Unique";
    }

    public sealed class NameSolverDatabaseStrings
    {
 
        private NameSolverDatabaseStrings()
        { }
        public const string SQLOLEDBProvider = "SQLOLEDB";
        public const string OraOLEDBProvider = "OraOLEDB.Oracle";
        public const string MSDAORAProvider = "MSDAORA";
        public const string SQLODBCProvider = "SQL Server Native Client 10.0";
        public const string ODBCProvider = "MSDASQL"; // generic ODBC provider

        public const string PostgreOdbcProvider = "PostgreSQL ODBC Driver(UNICODE)"; // Postgre provider

        public const string ProviderConnAttribute = "Provider={0}; ";
        public const string DriverConnAttribute = "Driver= {{{0}}}; ";

        public const string SQLWinNtConnection = "Data Source={0};Initial Catalog='{1}';Integrated Security='SSPI';Connect Timeout=30;Pooling=false;";
        public const string SQLConnection = "Data Source={0};Initial Catalog='{1}';User ID='{2}';Password='{3}';Connect Timeout=30;Pooling=false;";
        public const string SQLAzureConnection = "Server=tcp:{0};Database='{1}';User ID='{2}';Password='{3}';Connect Timeout=30;";

        public const string SQLWinNtConnectionRedux = "Data Source={0};Initial Catalog='{1}';Integrated Security='SSPI';";
        public const string SQLConnectionRedux = "Data Source={0};Initial Catalog='{1}';User ID='{2}';Password='{3}';";

        public const string OracleWinNtConnection = "Data Source={0};Integrated Security=yes;";
        public const string OracleConnection = "Data Source={0};User ID='{1}';Password='{2}';";
        public const string OracleWinNtConnectionWithProvider = "Provider={0}; Data Source={1}; OSAuthent=1";

        public const string PostgreWinNtConnection = "Server={0};Port={1} ;Database={2};SearchPath={3};Integrated Security=true;Pooling=False"; // SearchPath da cambiare probabilmente
        public const string PostgreConnection = "Server={0};Port={1};Database={2};User Id={3};Password={4};SearchPath={5};Pooling=False";

        public const string ODBCConnectionString = "Server={0};Database={1}; Uid={2}; Pwd={3};";
        public const string ODBCWinAuthConnectionString = "Server={0}; Database={1}; Trusted_Connection=yes;";

        public const string ODBCConnectionStringForPostgre = "Server={0};Port={1};Database={2};A6=set search_path to {3}; Uid={4};Pwd={5};";
        public const string ReleaseNumDirectory = "Release_";

        public const string SQLLatinCollation = "Latin1_General_CI_AS";
    }


    public class DatabaseLayerConsts
    {
        public const string NamespacePlugInsImg = "Microarea.Console.Core.PlugIns.PlugInsTreeViewImages";

        // numero massimo di dimensione in KB per i database in edizione Standard e Pro-Lite
        public const int MaxDBSize = 2097152; // equivalente a 2GB

        public const string SetIdentityInsertON = "SET IDENTITY_INSERT {0} ON\r\n";
        public const string SetIdentityInsertOFF = " SET IDENTITY_INSERT {0} OFF\r\n";

        public const string LoginSa = "sa";
        public const string MasterDatabase = "master";

        //@@Anastasia il nome del masterdatabase in postgre
        public const string postgreMasterDatabase = "postgres";
        //@@Anastasia Postgre login del superuser. TODO: da cambiare!! 
        public const string postgreSuperUser = "sa";
        ///@@Anastasia Postgre default password. Viene assegnato o usato quando la pasword e' vuota.
        /// Utente senza password non puo' mai accedere al db Postgre
        public const string postgreDefaultPassword = "111";

        ///@@Anastasia Postgre default schema.
        public const string postgreDefaultSchema = "test";

        ///@@Anastasia Postgre default port.
        public const int postgreDefaultPort = 5432;


        // login generica utilizzata per il EasyAttachment su azienda Oracle
        public const string DmsOraUser = "DmsOraUser";
        public const string DmsOraUserPw = "resUarOsmD.";

        public const string RoleSysAdmin = "sysadmin";
        public const string RoleDataReader = "db_datareader";
        public const string RoleDataWriter = "db_datawriter";
        public const string RoleDbOwner = "db_owner";

        public const string ERPSignature = "ERP";
        public const string DMSSignature = "DMS";
        public const string TBModuleName = "TB";
        public const string TbOleDbModuleName = "TbOledb";
        public const string ERPCoreModuleName = "Core";
        public const string OK = "OK";

        //// radice della login di SQL da associare all'utente (utilizzata nel QuickStart)
        public const string SqlLoginPrefix = "UserAuto";

        // DataSynchronizer
        public const string DataSynchroFunctionality = "DataSynchroFunctionality"; //funzionalita' generica da testare se almeno un connettore e' attivato

        // signature per i provider di sincronizzazione con il CRM (devono essere uguali ai nomi delle functionality specificati nei sales modules)
        public const string InfiniteCRM = "InfiniteCRM";
        public const string CRMInfinity = "CRMInfinity";
        public const string DMSInfinity = "DMSInfinity";
        //

        // costanti per controllare l'IsActivated
        public const string MicroareaConsole = "MicroareaConsole";
        public const string SecurityAdmin = "SecurityAdmin";

        public const string AuditingAdmin = "AuditingAdmin";
        public const string TbRowSecurity = "TbRowSecurity";
        public const string RowSecurityToolKit = "RowSecurityToolKit";
        public const string MigrationXPNet = "MigrationXPNet";
        public const string MigrationNetNet = "MigrationNetNet";
        public const string RegressionTestNet = "RegressionTestNet";
        public const string XEngine = "XEngine";
        public const string MailConnector = "MailConnector";
        public const string WebFramework = "WebFramework";
        public const string EasyLook = "EasyLook";
        public const string EasyAttachment = "EasyAttachment";

        // EasyAttachment
        public const string FullTextSearchSuffix = "_FTS";

        // RowSecurityLayer
        public const string RowSecurityIDForSQL = "RowSecurityID";
        public const string RowSecurityIDForOracle = "ROWSECURITYID";
        public const string IsProtectedForSQL = "IsProtected";
        public const string IsProtectedForOracle = "ISPROTECTED";

        // elenco provider supportati e memorizzati in MSD_Providers
        public const string SqlOleProviderDescription = "Microsoft SQL Server Provider";
        public const string SqlODBCProviderDescription = "Microsoft SQL Server ODBC Provider";
        public const string OracleProviderDescription = "Oracle OLE DB Provider";
        public const string PostgreProviderDescription = "PostgreSQL ODBC Provider";

        // nome cablato per il database di sistema con la Standard Edition
        public const string StandardSystemDb = "StandardSysDb";

        // nomi cablati per il quick start di MagoNet
        private const string magoNetSystemDBName = "{0}SystemDB";
        private const string magoNetCompanyDBName = "{0}CompanyDB";
        private const string magoNetCompanyName = "{0}Company";
        private const string magoNetDMSName = "{0}CompanyDMSDB";

        public static string MagoNetSystemDBName(string prefix) { return FormatDbName(prefix, magoNetSystemDBName); }
        public static string MagoNetCompanyDBName(string prefix) { return FormatDbName(prefix, magoNetCompanyDBName); }
        public static string MagoNetDMSDBName(string prefix) { return FormatDbName(prefix, magoNetDMSName); }
        public static string MagoNetCompanyName(string prefix) { return FormatDbName(prefix, magoNetCompanyName); }

        private static string FormatDbName(string prefix, string db)
        { if (string.IsNullOrWhiteSpace(prefix)) prefix = "New"; return string.Format(db, prefix); }
        //

        public const string TB_DBMark = "TB_DBMark";
        public const string MSD_DBMark = "MSD_DBMark";
        public const string CreateInfoFile = "CreateInfo.xml";
        public const string UpgradeInfoFile = "UpgradeInfo.xml";

        // colonne obbligatorie per SQL
        public const string TBCreatedColNameForSql = "TBCreated";
        public const string TBModifiedColNameForSql = "TBModified";
        public const string TBCreatedIDColNameForSql = "TBCreatedID";
        public const string TBModifiedIDColNameForSql = "TBModifiedID";
        public const string TBGuidColNameForSql = "TBGuid";


        // colonne obbligatorie per Oracle
        public const string TBCreatedColNameForOracle = "TBCREATED";
        public const string TBModifiedColNameForOracle = "TBMODIFIED";
        public const string TBCreatedIDColNameForOracle = "TBCREATEDID";
        public const string TBModifiedIDColNameForOracle = "TBMODIFIEDID";
        public const string TBGuidColNameForOracle = "TBGUID";


        // colonne obbligatorie per Postgre
        public const string TBCreatedColNameForPostgre = "tbcreated";
        public const string TBModifiedColNameForPostgre = "tbmodified";
        public const string TBCreatedIDColNameForPostgre = "tbcreatedid";
        public const string TBModifiedIDColNameForPostgre = "tbmodifiedid";
        public const string TBGuidColNameForPostgre = "tbguid";

        //------- query per individuare quali tabelle sono prive di una delle colonne obbligatorie

        //------- query per individuare quali tabelle sono prive di una delle colonne obbligatorie
        public const string PostgreTablesWithNoMandatoryFields =
                @"select TABLE_NAME from INFORMATION_SCHEMA.TABLES as X 
				where not exists (select Y.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS as Y 
				where Y.TABLE_NAME = X.TABLE_NAME and Y.COLUMN_NAME IN ('tbmodifiedid') ) and X.TABLE_TYPE = 'BASE TABLE'  AND table_schema='{0}'
				union
				select TABLE_NAME from INFORMATION_SCHEMA.TABLES as X 
				where not exists (select Y.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS as Y 
				where Y.TABLE_NAME = X.TABLE_NAME and Y.COLUMN_NAME IN ('tbcreatedid') ) and X.TABLE_TYPE = 'BASE TABLE' AND table_schema='" + DatabaseLayerConsts.postgreDefaultSchema + @"'
				union
				select TABLE_NAME from INFORMATION_SCHEMA.TABLES as X 
				where not exists (select Y.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS as Y 
				where Y.TABLE_NAME = X.TABLE_NAME and Y.COLUMN_NAME IN ('tbcreated') ) and X.TABLE_TYPE = 'BASE TABLE' AND table_schema='" + DatabaseLayerConsts.postgreDefaultSchema + @"'
				union
				select TABLE_NAME from INFORMATION_SCHEMA.TABLES as X 
				where not exists (select Y.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS as Y 
				where Y.TABLE_NAME = X.TABLE_NAME and Y.COLUMN_NAME IN ('tbmodified') ) and X.TABLE_TYPE = 'BASE TABLE' AND table_schema='" + DatabaseLayerConsts.postgreDefaultSchema + @"'
				order by TABLE_NAME";

        public const string SQLTablesWithNoMandatoryFields =
                @"select TABLE_NAME from INFORMATION_SCHEMA.TABLES as X 
				where not exists (select Y.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS as Y 
				where Y.TABLE_NAME = X.TABLE_NAME and Y.COLUMN_NAME IN ('TBModifiedID') ) and X.TABLE_TYPE = 'BASE TABLE' 
				union
				select TABLE_NAME from INFORMATION_SCHEMA.TABLES as X 
				where not exists (select Y.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS as Y 
				where Y.TABLE_NAME = X.TABLE_NAME and Y.COLUMN_NAME IN ('TBCreatedID') ) and X.TABLE_TYPE = 'BASE TABLE' 
				union
				select TABLE_NAME from INFORMATION_SCHEMA.TABLES as X 
				where not exists (select Y.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS as Y 
				where Y.TABLE_NAME = X.TABLE_NAME and Y.COLUMN_NAME IN ('TBCreated') ) and X.TABLE_TYPE = 'BASE TABLE' 
				union
				select TABLE_NAME from INFORMATION_SCHEMA.TABLES as X 
				where not exists (select Y.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS as Y 
				where Y.TABLE_NAME = X.TABLE_NAME and Y.COLUMN_NAME IN ('TBModified') ) and X.TABLE_TYPE = 'BASE TABLE'
				order by TABLE_NAME";

        public const string ORACLETablesWithNoMandatoryFields =
                @"Select TABLE_NAME from USER_TABLES X
				where not exists (select COLUMN_NAME from USER_TAB_COLUMNS Y where X.TABLE_NAME = Y.TABLE_NAME and Y.COLUMN_NAME = 'TBCREATED') 
				union 
				Select TABLE_NAME from USER_TABLES X 
				where not exists (select COLUMN_NAME from USER_TAB_COLUMNS Y where X.TABLE_NAME = Y.TABLE_NAME and Y.COLUMN_NAME = 'TBCREATEDID') 
				union 
				Select TABLE_NAME from USER_TABLES X 
				where not exists (select COLUMN_NAME from USER_TAB_COLUMNS Y where X.TABLE_NAME = Y.TABLE_NAME and Y.COLUMN_NAME = 'TBMODIFIED') 
				union 
				Select TABLE_NAME from USER_TABLES X 
				where not exists (select COLUMN_NAME from USER_TAB_COLUMNS Y where X.TABLE_NAME = Y.TABLE_NAME and Y.COLUMN_NAME = 'TBMODIFIEDID')";
        //-------

        //------- comandi per aggiungere le colonne obbligatorie TBCreated e TBModified in coda all'esecuzione degli script

        public const string PostgreAddMandatoryColums = "ALTER TABLE {0} ADD COLUMN {1} timestamp NOT NULL CONSTRAINT DF_{0}_{1}_000 DEFAULT(now());";

        public const string SQLAddMandatoryColums = @"if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
														dbo.sysobjects.name = '{0}' and dbo.sysobjects.id = dbo.syscolumns.id 
														and dbo.syscolumns.name = '{1}')
														BEGIN
															ALTER TABLE [dbo].[{0}]	ADD [{1}] [datetime] NOT NULL CONSTRAINT DF_{0}_{1}_000 DEFAULT(GetDate())
														END
														GO
														UPDATE [dbo].[{0}] SET [dbo].[{0}].[{1}] = GetDate() WHERE [dbo].[{0}].[{1}] IS NULL";

        public const string OracleAddMandatoryColums = "ALTER TABLE {0} ADD \"{1}\" DATE DEFAULT(sysdate) GO UPDATE \"{0}\" SET \"{0}\".\"{1}\" = sysdate WHERE \"{0}\".\"{1}\" IS NULL";

        public const string AddMandatoryColsForPostgre = ",TBCreated timestamp NOT NULL CONSTRAINT DF_{0}_TBCreated_000 DEFAULT(now()), TBModified timestamp NOT NULL CONSTRAINT DF_{0}_TBModified_000 DEFAULT(now())";

        public const string AddMandatoryColsForSQL = @",[TBCreated] [datetime] NOT NULL CONSTRAINT DF_{0}_TBCreated_000 DEFAULT(GetDate()), 
														[TBModified] [datetime] NOT NULL CONSTRAINT DF_{0}_TBModified_000 DEFAULT(GetDate())";

        public const string AddMandatoryColsForOracle = ",\"TBCREATED\" DATE DEFAULT(sysdate), \"TBMODIFIED\" DATE DEFAULT(sysdate)";

        //------- comandi per aggiungere le colonne obbligatorie TBCreatedID, TBModifiedID (miglioria 4197)

        public const string PostgreAddWorkersMandatoryColums = "ALTER TABLE IF EXISTS {0} ADD {1} integer NOT NULL CONSTRAINT DF_{0}_{1}_000 DEFAULT(0)";
        public const string SQLAddWorkersMandatoryColums = @"if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
														dbo.sysobjects.name = '{0}' and dbo.sysobjects.id = dbo.syscolumns.id 
														and dbo.syscolumns.name = '{1}')
														BEGIN
															ALTER TABLE [dbo].[{0}]	ADD [{1}] [int] NOT NULL CONSTRAINT DF_{0}_{1}_000 DEFAULT(0)
														END
														GO
														UPDATE [dbo].[{0}] SET [dbo].[{0}].[{1}] = 0 WHERE [dbo].[{0}].[{1}] IS NULL";
        public const string OracleAddWorkersMandatoryColums = "ALTER TABLE \"{0}\" ADD \"{1}\" NUMBER(10) DEFAULT(0) GO UPDATE \"{0}\" SET \"{0}\".\"{1}\" = 0 WHERE \"{0}\".\"{1}\" IS NULL";


        public const string AddWorkersMandatoryColsForPostgre = ",TBCreatedID integer NOT NULL CONSTRAINT DF_{0}_TBCreatedID_000 DEFAULT(0), TBModifiedID integer NOT NULL CONSTRAINT DF_{0}_TBModifiedID_000 DEFAULT(0)";
        public const string AddWorkersMandatoryColsForSQL = @",[TBCreatedID] [int] NOT NULL CONSTRAINT DF_{0}_TBCreatedID_000 DEFAULT(0), 
															[TBModifiedID] [int] NOT NULL CONSTRAINT DF_{0}_TBModifiedID_000 DEFAULT(0)";

        public const string AddWorkersMandatoryColsForOracle = ",\"TBCREATEDID\" NUMBER(10) DEFAULT(0), \"TBMODIFIEDID\" NUMBER(10) DEFAULT(0)";

        //------- comandi per aggiungere la colonne obbligatoria TBGuid solo per tabelle master (impr. #5936 BAUZI)
        public const string SQLAddGuidMandatoryColumn = @"if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
														dbo.sysobjects.name = '{0}' and dbo.sysobjects.id = dbo.syscolumns.id 
														and dbo.syscolumns.name = '{1}')
														BEGIN
															ALTER TABLE [dbo].[{0}]	ADD [{1}] [uniqueidentifier] NOT NULL CONSTRAINT DF_{0}_TBGuid_000 DEFAULT(NEWID())
														END
														GO
                                                        UPDATE [dbo].[{0}] SET [dbo].[{0}].[{1}] = NEWID() WHERE [dbo].[{0}].[{1}] IS NULL OR [dbo].[{0}].[{1}] = 0x00
                                                        GO";

        public const string SQLUpdateGuidMandatoryColumn = "UPDATE [dbo].[{0}] SET [dbo].[{0}].[{1}] = NEWID() WHERE [dbo].[{0}].[{1}] IS NULL OR [dbo].[{0}].[{1}] = 0x00 OR [dbo].[{0}].[{1}] = '00000000-0000-0000-0008-000000000000'";

        public const string OracleAddGuidMandatoryColumn = "ALTER TABLE \"{0}\" ADD \"{1}\" CHAR(38) DEFAULT('{{00000000-0000-0000-0000-000000000000}}')\r\nGO\r\nUPDATE \"{0}\" SET \"{1}\" = GET_GUID() WHERE \"{1}\" IS NULL OR \"{1}\" = '{{00000000-0000-0000-0000-000000000000}}' OR \"{1}\" = '00000000-0000-0000-0000-000000000000'\r\nGO";
        public const string OracleUpdateGuidMandatoryColumn = "UPDATE \"{0}\" SET \"{1}\" = GET_GUID() WHERE \"{1}\" IS NULL OR \"{1}\" = '{{00000000-0000-0000-0000-000000000000}}' OR \"{1}\" = '{{00000000-0000-0000-0008-000000000000}}' OR \"{1}\" = '00000000-0000-0000-0000-000000000000' OR \"{1}\" = '00000000-0000-0000-0008-000000000000'\r\nGO";

        public const string OracleCreateGetGuidFunction = @"CREATE OR REPLACE FUNCTION GET_GUID RETURN VARCHAR2 IS guid VARCHAR2(38); 
															BEGIN SELECT SYS_GUID() INTO guid FROM DUAL ; 
															guid := SUBSTR(guid,  1, 8) || '-' || SUBSTR(guid,  9, 4) || '-' || SUBSTR(guid, 13, 4) || '-' || SUBSTR(guid, 17, 4) || '-' || SUBSTR(guid, 21);
															RETURN guid; END GET_GUID; GO";
        public const string OracleDropGetGuidFunction = "DROP FUNCTION GET_GUID";

        public const string SQLSelectEmptyGuidColumn = "SELECT COUNT(*) FROM [dbo].[{0}] WHERE [dbo].[{0}].[{1}] IS NULL OR [dbo].[{0}].[{1}] = 0x00 OR [dbo].[{0}].[{1}] = '00000000-0000-0000-0008-000000000000'";
        public const string OracleSelectEmptyGuidColumn = "SELECT COUNT(*) FROM \"{0}\" WHERE \"{1}\" IS NULL OR \"{1}\" = '{{00000000-0000-0000-0000-000000000000}}' OR \"{1}\" = '{{00000000-0000-0000-0008-000000000000}}' OR \"{1}\" = '00000000-0000-0000-0000-000000000000' OR \"{1}\" = '00000000-0000-0000-0008-000000000000'";

        //@@TODO BAUZI: chiedere ad Anastasia per Postgre
        public const string PostgreAddGuidMandatoryColumn = "ALTER TABLE IF EXISTS {0} ADD {1} UUID NOT NULL CONSTRAINT DF_{0}_{1}_000 DEFAULT(0x00)";
        public const string AddGuidMandatoryColForPostgre = ",{0} UUID NOT NULL CONSTRAINT DF_{1}_{0}_000 DEFAULT(0x00)";
        public const string PostgreSelectEmptyGuidColumn = "SELECT COUNT(*) FROM \"{0}\" WHERE {1} = '{{00000000-0000-0000-0000-000000000000}}'";
        //-------

        // ------- CREAZIONE TABELLE AUDITING AU_
        // per la gestione di problematica legata al plugin Auditing ed alla necessità di aggiungere in testa
        // alle tabelle sottoposte a tracciatura la colonna "AU_ID" (vedi anomalia nr. 13679)
        public const string PostgreCreateTableText = @"CREATE TABLE IF NOT EXISTS {0} 
														(AU_ID serial NOT NULL,
														AU_OperationData timestamp NOT NULL DEFAULT (now()),
														AU_OperationType smallint NOT NULL, 	
														AU_LoginName varchar(50) NOT NULL, 
														AU_NameSpaceID int,
														";
        public const string SqlCreateTableText = @"CREATE TABLE [{0}] 
														([AU_ID] [int] IDENTITY (1, 1) NOT NULL,
														[AU_OperationData] [datetime] NOT NULL DEFAULT (getdate()),
														[AU_OperationType] [smallint] NOT NULL, 	
														[AU_LoginName] [varchar](50) NOT NULL, 
														[AU_NameSpaceID] [int],
														";
        public const string SqlCreateTableTextUnicode = @"CREATE TABLE [{0}] 
														([AU_ID] [int] IDENTITY (1, 1) NOT NULL,
														[AU_OperationData] [datetime] NOT NULL DEFAULT (getdate()),
														[AU_OperationType] [smallint] NOT NULL, 	
														[AU_LoginName] [nvarchar](50) NOT NULL, 
														[AU_NameSpaceID] [int],
														";

        // per la gestione di problematica legata al plugin Auditing ed alla necessità di modificare il tipo
        // della colonna "AU_ID" da identity a int (vedi anomalia nr. 16538)


        public const string CreateTableAuditRel3 = @"CREATE TABLE [{0}] 
														([AU_ID] [int] NOT NULL,
														[AU_OperationData] [datetime] NOT NULL DEFAULT (getdate()),
														[AU_OperationType] [smallint] NOT NULL, 	
														[AU_LoginName] [varchar](50) NOT NULL, 
														[AU_NameSpaceID] [int],
														";
        public const string SqlCreateTableAuditRel3 = @"CREATE TABLE [{0}] 
														([AU_ID] [int] NOT NULL,
														[AU_OperationData] [datetime] NOT NULL DEFAULT (getdate()),
														[AU_OperationType] [smallint] NOT NULL, 	
														[AU_LoginName] [varchar](50) NOT NULL, 
														[AU_NameSpaceID] [int],
														";
        public const string SqlCreateTableAuditRel3Unicode = @"CREATE TABLE [{0}] 
														([AU_ID] [int] NOT NULL,
														[AU_OperationData] [datetime] NOT NULL DEFAULT (getdate()),
														[AU_OperationType] [smallint] NOT NULL, 	
														[AU_LoginName] [nvarchar](50) NOT NULL, 
														[AU_NameSpaceID] [int],
														";

        // le tabelle di Oracle sono sempre le stesse
        public const string OracleCreateTableText = "CREATE TABLE \"{0}\"(\r\n\"AU_ID\" NUMBER(10) DEFAULT(0) NOT NULL,\r\n\"AU_OPERATIONDATA\" DATE DEFAULT (sysdate) NOT NULL,\r\n\"AU_OPERATIONTYPE\" NUMBER(6) NOT NULL,\r\n\"AU_LOGINNAME\" VARCHAR2(50) NOT NULL,\r\n\"AU_NAMESPACEID\" NUMBER(10),\r\n";
        public const string OracleCreateTableTextUnicode = "CREATE TABLE \"{0}\"(\r\n\"AU_ID\" NUMBER(10) DEFAULT(0) NOT NULL,\r\n\"AU_OPERATIONDATA\" DATE DEFAULT (sysdate) NOT NULL,\r\n\"AU_OPERATIONTYPE\" NUMBER(6) NOT NULL,\r\n\"AU_LOGINNAME\" NVARCHAR2(50) NOT NULL,\r\n\"AU_NAMESPACEID\" NUMBER(10),\r\n";
        // ------- END CREAZIONE TABELLE AUDITING AU_


        // ------- GESTIONE TRIGGERS
        public const string PostgreFindTriggersName = "select oid as \" \",  relname as tblname ,tgname as tgname from pg_class a JOIN  (select oid, tgrelid,tgname from pg_trigger where tgenabled !='D' AND tgisinternal=false) c ON tgrelid = a.oid  JOIN information_schema.tables b on a.relname=b.table_name where table_schema='" + DatabaseLayerConsts.postgreDefaultSchema + "'";

        public const string SQLFindTriggersName = @"SELECT sys.objects.name as TBLNAME, sys.triggers.name as TRNAME FROM sys.objects 
													INNER JOIN sys.triggers ON sys.objects.object_id = sys.triggers.parent_id AND sys.triggers.is_disabled = 0";
        public const string OracleFindTriggersName = "SELECT TRIGGER_NAME AS TRNAME FROM USER_TRIGGERS WHERE STATUS = 'ENABLED'";

        public const string PostgreEnableTriggerOfTable = "ALTER TABLE {0} ENABLE TRIGGER \"{1}\"";
        public const string PostgreDisableTriggerOfTable = "ALTER TABLE {0} DISABLE TRIGGER \"{1}\"";

        public const string SQLEnableTriggerOfTable = "ALTER TABLE [{0}] ENABLE TRIGGER [{1}]";
        public const string SQLDisableTriggerOfTable = "ALTER TABLE [{0}] DISABLE TRIGGER [{1}]";

        public const string OracleEnableTrigger = "ALTER TRIGGER \"{0}\" ENABLE";
        public const string OracleDisableTrigger = "ALTER TRIGGER \"{0}\" DISABLE";
        // ------- END GESTIONE TRIGGERS

        // ------- GESTIONE VISTE MATERIALIZZATE PER MIGLIORAMENTI PERFORMANCE DI ORACLE
        public const string CreateMViewPrivilege = "CREATE MATERIALIZED VIEW";

        public const string DropMView = "DROP MATERIALIZED VIEW \"{0}\"";

        public const string MV_ALL_SYNONYMS = "ALL_SYNONYMS";
        public const string MV_ALL_OBJECTS = "ALL_OBJECTS";
        public const string MV_ALL_CONSTRAINTS = "ALL_CONSTRAINTS";
        public const string MV_ALL_CONS_COLUMNS = "ALL_CONS_COLUMNS";
        public const string MV_ALL_TAB_COLUMNS = "ALL_TAB_COLUMNS";

        public const string CreateMV_ALL_SYNONYMS = "create materialized view ALL_SYNONYMS nologging cache refresh complete on demand as select * from sys.all_synonyms";
        public const string CreateMV_ALL_OBJECTS = "create materialized view ALL_OBJECTS nologging cache refresh complete on demand as select * from sys.all_objects";

        public const string CreateMV_ALL_CONSTRAINTS = @"create materialized view ALL_CONSTRAINTS nologging cache refresh complete on demand
							as select OWNER, CONSTRAINT_NAME, CONSTRAINT_TYPE, TABLE_NAME, R_OWNER, R_CONSTRAINT_NAME, DELETE_RULE,	STATUS, 
							DEFERRABLE, DEFERRED, VALIDATED, GENERATED, BAD, RELY, LAST_CHANGE, INDEX_OWNER, INDEX_NAME, INVALID, VIEW_RELATED from sys.all_constraints";

        public const string CreateMV_ALL_CONS_COLUMNS = "create materialized view ALL_CONS_COLUMNS nologging cache refresh complete on demand as select * from sys.all_cons_columns";

        public const string CreateMV_ALL_TAB_COLUMNS = @"create materialized view ALL_TAB_COLUMNS nologging cache refresh complete on demand 
							as select OWNER, TABLE_NAME, COLUMN_NAME, DATA_TYPE, DATA_TYPE_MOD, DATA_TYPE_OWNER, DATA_LENGTH, DATA_PRECISION, DATA_SCALE,
							NULLABLE, COLUMN_ID, DEFAULT_LENGTH, NUM_DISTINCT, LOW_VALUE, HIGH_VALUE, DENSITY, NUM_NULLS, NUM_BUCKETS, LAST_ANALYZED,
							SAMPLE_SIZE, CHARACTER_SET_NAME, CHAR_COL_DECL_LENGTH, GLOBAL_STATS, USER_STATS, AVG_COL_LEN, CHAR_LENGTH, CHAR_USED,
							V80_FMT_IMAGE, DATA_UPGRADED, HISTOGRAM from sys.all_tab_columns";

        public const string Idx_ALL_TAB_COLUMNS = "create index IDX_ALL_TAB_COLUMS_TABLE_NAME on ALL_TAB_COLUMNS(table_name) nologging";
        public const string Idx_ALL_CONS_COLUMNS = "create index IDX_ALL_CC_OWNER_NAME on ALL_CONS_COLUMNS(owner, constraint_name) nologging";
        public const string Idx_ALL_CONSTRAINTS = "create index IDX_ALL_CO_TYPE on ALL_CONSTRAINTS(constraint_type) nologging";
        // ------- END GESTIONE VISTE MATERIALIZZATE PER MIGLIORAMENTI PERFORMANCE DI ORACLE


        // ------- GESTIONE COLONNE OBBLIGATORIE PER ROWSECURITYLAYER
        public const string SQLAddRowSecurityIDColumn = @"if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
													dbo.sysobjects.name = '{0}' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = '{1}')
													BEGIN
														ALTER TABLE [dbo].[{0}]	ADD [{1}] [int] NULL CONSTRAINT DF_{0}_{1}_000 DEFAULT(-1)
													END
                                                    GO
													UPDATE [dbo].[{0}] SET [dbo].[{0}].[{1}] = -1 WHERE [dbo].[{0}].[{1}] IS NULL";
        public const string OracleAddRowSecurityIDColumn = "ALTER TABLE \"{0}\" ADD \"{1}\" NUMBER(10) DEFAULT(-1) GO UPDATE \"{0}\" SET \"{0}\".\"{1}\" = -1 WHERE \"{0}\".\"{1}\" IS NULL";

        public const string SQLAddIsProtectedColumn = @"if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
													dbo.sysobjects.name = '{0}' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = '{1}')
													BEGIN
														ALTER TABLE [dbo].[{0}]	ADD [{1}] [char](1) NOT NULL CONSTRAINT DF_{0}_{1}_000 DEFAULT('0')
													END
                                                    GO
                                                    UPDATE [dbo].[{0}] SET [dbo].[{0}].[{1}] = '0' WHERE [dbo].[{0}].[{1}] IS NULL";
        public const string OracleAddIsProtectedColumn = "ALTER TABLE \"{0}\" ADD \"{1}\" CHAR(1) DEFAULT(\'0\') GO UPDATE \"{0}\" SET \"{0}\".\"{1}\" = \'0\' WHERE \"{0}\".\"{1}\" IS NULL";
        // ------- END GESTIONE COLONNE OBBLIGATORIE PER ROWSECURITYLAYER
    }
  
}
