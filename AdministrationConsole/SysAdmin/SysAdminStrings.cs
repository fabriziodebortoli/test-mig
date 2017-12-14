
namespace Microarea.Console.Plugin.SysAdmin
{
	/// <summary>
	/// ConstString
	/// Costanti stringa utilizzate nel plugIns SysAdmin
	/// </summary>
	//=========================================================================
	public class ConstString
	{
		public const string SysAdminPlugInRoot	  		= "SysAdminPlugInRoot";
		public const string containerProviders    		= "ContenitoreProvider";
		public const string containerUsers		  		= "ContenitoreUtenti";
		public const string containerCompanies	  		= "ContenitoreAziende";
		public const string containerCompanyRoles 		= "ContenitoreRuoliAzienda";
		public const string containerCompanyUsersRoles	= "ContenitoreUtentiRuolo";
		public const string containerCompanyUsers 		= "ContenitoreUtentiAzienda";
		public const string containerLoginsUsers  		= "ContenitoreLoginsAzienda";
		public const string itemCompany           		= "Azienda";
		public const string itemRole              		= "Ruolo";
		public const string itemUser              		= "Utente";
		public const string itemCompanyUser       		= "UtenteAzienda";
		public const string itemRoleCompanyUser   		= "UtenteRuoloAzienda";
		public const string itemProvider          		= "Provider";
		public const string configParameters	  		= "EditConfigFile";
		
		public const string providerNT			  		= "WinNT://";

		public const string serverLabel					= "Data Source=";
		public const string serverLoginLabel			= "Integrated Security=";
		public const string serverUserLabel				= "User ID=";
		public const string passwordElement				= "Password=";
		public const string serverDataBaseLabel			= "Initial Catalog=";
		public const string NTSecurity          		= "Integrated Security";
		public const string connectTimeOut				= "Connect Timeout=30";
		public const string integratedSecurityElement	= "Integrated Security=SSPI";
		public const string separatorElement			= ";";
		public const string nameSpaceSysAdmin   		= "Microarea.Console.Plugin.SysAdmin";

		public const string passwordEmpty       		= "Empty";
		public const string AllUsers            		= "AllUsers";
		public const string Standard            		= "Standard";
		public const string LoginNotExist       		= "LoginNotExist";

		public const string minPasswordLength			= "0";
	}
}