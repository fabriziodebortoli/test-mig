using System.Reflection;

namespace Microarea.Console.Plugin.AuditingAdmin
{
	/// <summary>
	/// AuditConstStrings
	/// Costanti stringa utilizzate nel PlugIn AuditingAdmin
	/// </summary>
	//=========================================================================
	public class AuditConstStrings
	{
		public static readonly string AuditingAdminPlugIn = Assembly.GetExecutingAssembly().GetName().Name;
		public const string NamespaceAuditingAdminImage	= "Microarea.Console.Plugin.AuditingAdmin.Images";

		public const string CompanyContainer			= "ContainerCompany";
		public const string AuditingAdministratorType	= "AuditingAdministrator";
		public const string Company						= "Azienda";
		public const string Application					= "Application";
		public const string Module						= "Module";
		public const string Dummy						= "Dummy"; 
		public const string NoExistTable				= "NoExistTable";
		public const string TracedTable					= "TracedTable";
		public const string NoTracedTable				= "NoTracedTable";
		public const string PauseTraceTable				= "PauseTraceTable";
		public const string All							= "ALL";
		public const string ApplicationsTree			= "ApplicationsTree";
		
		//utilizzati per DataTable
		public const string AuditTable					= "AuditTable";
		public const string AppTable					= "AppTable";
		public const string Users						= "Users";
		
		//per la combo del cambiamento di stato
		public const string Operation					= "Operation";
		public const string OperType					= "OperType";
		public const string OperDescri					= "OperDescri";
	}

	//=========================================================================
	public class AuditTableConsts
	{
		// tabella AUDIT_Tables		
		public const string AuditTablesTableName	= "AUDIT_Tables";
		public const string TableNameCol			= "TableName";
		public const string StartDateCol			= "StartTrace";
		public const string StopCol					= "Suspended";

		//tabella AUDIT_Namespaces
		public const string AuditNamespacesTableName= "AUDIT_Namespaces";
		public const string IDCol					= "ID";
		public const string NamespaceCol			= "Namespace";

		// per la tabella di tracciatura
		public const string AuditPrefixOld			= "AUDIT_";	//utilizzato nella prima versione
		public const string AuditPrefix				= "AU_"; //diminuito a causa lunghezza tabelle in oracle (max 30 caratteri)
		public const string AuIdCol					= "AU_ID";
		public const string DateCol					= "AU_OperationData";
		public const string LoginName				= "AU_LoginName";
		public const string NamespaceIDCol			= "AU_NameSpaceID";
		public const string OperationCol			= "AU_OperationType";

		//campo virtuale contenente la decodifica dell'operazione x una corretta visualizzazione all'utente	
		public const string OperationDecode			= "OperationDecode";

		//per la tabella delle login del database di sistema
		public const string LoginID					= "LoginId";
		public const string Login					= "Login";

		// creazione tabelle per SQL
		public const string SqlCreateTableText		= @"CREATE TABLE [{0}] 
														([AU_ID] [int] NOT NULL,
														[AU_OperationData] [datetime] NOT NULL DEFAULT (getdate()),
														[AU_OperationType] [smallint] NOT NULL, 	
														[AU_LoginName] [varchar](50) NOT NULL, 
														[AU_NameSpaceID] [int],";

		public const string SqlCreateTableTextUnicode = @"CREATE TABLE [{0}] 
														([AU_ID] [int] NOT NULL,
														[AU_OperationData] [datetime] NOT NULL DEFAULT (getdate()),
														[AU_OperationType] [smallint] NOT NULL, 	
														[AU_LoginName] [nvarchar](50) NOT NULL, 
														[AU_NameSpaceID] [int],";

		public const string SqlPKConstraintText	= ", CONSTRAINT [PK_{0}] PRIMARY KEY NONCLUSTERED ([AU_ID], [AU_OperationData], [AU_OperationType]";

		// creazione tabelle per Oracle
		public const string OracleCreateTableText	= "CREATE TABLE \"{0}\"(\r\n\"AU_ID\" NUMBER(10) DEFAULT(0) NOT NULL,\r\n\"AU_OPERATIONDATA\" DATE DEFAULT (sysdate) NOT NULL,\r\n\"AU_OPERATIONTYPE\" NUMBER(6) NOT NULL,\r\n\"AU_LOGINNAME\" VARCHAR2(50) NOT NULL,\r\n\"AU_NAMESPACEID\" NUMBER(10),\r\n";
		public const string OracleCreateTableTextUnicode = "CREATE TABLE \"{0}\"(\r\n\"AU_ID\" NUMBER(10) DEFAULT(0) NOT NULL,\r\n\"AU_OPERATIONDATA\" DATE DEFAULT (sysdate) NOT NULL,\r\n\"AU_OPERATIONTYPE\" NUMBER(6) NOT NULL,\r\n\"AU_LOGINNAME\" NVARCHAR2(50) NOT NULL,\r\n\"AU_NAMESPACEID\" NUMBER(10),\r\n";

		public const string OraclePKConstraintText	= ", CONSTRAINT \"PK_{0}\" PRIMARY KEY (\"AU_ID\", \"AU_OPERATIONDATA\", \"AU_OPERATIONTYPE\"";

		// drop tabella
		public const string SqlDropTableText		= "DROP TABLE [{0}]";
		public const string OracleDropTableText		= "DROP TABLE \"{0}\"";

		// add colonna
		public const string SqlAddColumnText		= "ALTER TABLE [{0}] ADD {1} ";
		public const string OraclelAddColumnText	= "ALTER TABLE \"{0}\" ADD ( {1} )";

		// drop colonna
		public const string SqlDropColumnText		= "ALTER TABLE [{0}] DROP COLUMN {1} ";
		public const string OracleDropColumnText	= "ALTER TABLE \"{0}\" DROP ( {1} )";
	}
}
