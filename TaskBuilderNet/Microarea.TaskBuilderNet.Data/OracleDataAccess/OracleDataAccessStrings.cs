

namespace Microarea.TaskBuilderNet.Data.OracleDataAccess
{
	//=========================================================================
	public class OracleDBSchemaStrings
	{
		public const string CreateUser = "CREATE USER \"{0}\" PROFILE \"DEFAULT\" IDENTIFIED BY \"{1}\" DEFAULT TABLESPACE \"USERS\" ACCOUNT UNLOCK";
		public const string CreateNTUser = "CREATE USER \"{0}\" PROFILE \"DEFAULT\" IDENTIFIED EXTERNALLY DEFAULT TABLESPACE \"USERS\" TEMPORARY TABLESPACE \"TEMP\" ACCOUNT UNLOCK";

		public const string AlterUserQuotaOnTableSpace = "ALTER USER \"{0}\" QUOTA 100M ON \"USERS\"";
		public const string GrantTablespaceToUser = "GRANT UNLIMITED TABLESPACE TO \"{0}\"";

		public const string GrantConnect = "GRANT \"CONNECT\" TO \"{0}\"";
		public const string GrantResource = "GRANT \"RESOURCE\" TO \"{0}\"";
		public const string GrantCreateView = "GRANT CREATE VIEW TO \"{0}\"";
		public const string GrantCreateAnySynonym = "GRANT CREATE ANY SYNONYM TO \"{0}\"";
		public const string GrantCreateMaterializedView = "GRANT CREATE MATERIALIZED VIEW TO \"{0}\"";
		
		public const string DropUser = "DROP USER \"{0}\" CASCADE";
	}
}
