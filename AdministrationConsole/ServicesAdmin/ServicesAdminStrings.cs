using System.Reflection;

namespace Microarea.Console.Plugin.ServicesAdmin
{
    //=========================================================================
	public class ConstString
	{
		public static readonly string servicePlugIn	= Assembly.GetExecutingAssembly().GetName().Name;

		public const string NamespacePlugInsImg		= "Microarea.Console.Core.PlugIns.PlugInsTreeViewImages";
		public const string settingsAdminPlugIn		= "SettingsAdminPlugIn";
		public const string settingsAdmin			= "SettingsAdmin.SettingsAdmin";
		public const string settingsAdminlbl		= "SettingsAdmin";
		public const string logContainer			= "LogContainer";
		public const string webServicesContainer    = "WebServiceContainer";
		public const string applicationsContainer	= "ApplicationsContainer";
		public const string application				= "Application";
		public const string sessionContainer		= "SessionContainer";
		public const string session					= "Session";
		public const string setParameters			= "SetParameters";
		public const string boolType				= "bool";
		public const string sysAdminPlugInRoot		= "SysAdminPlugInRoot";
		public const string sysAdminPlugIn			= "SysAdminPlugIn";
		public const string articleNode				= "ArticleNode";
		public const string setArticols				= "SetArticles";
		public const string usersGroup				= "UsersGroup";
		public const string locksGroup				= "LocksGroup";
		public const string userList				= "UsersList";
		public const string integer					= "integer";
		
		//Constant x il Parse degli Utenti Logati
		public const string user					= "User";
		public const string name					= "name";
		public const string company					= "company";
		public const string companiesTable          = "MSD_Companies";
		public const string loginsTable				= "MSD_Logins";
		public const string companyUsersTable       = "MSD_CompanyLogins";
		public const string operationTable          = "Operations";
		public const string traceTable				= "MSD_Trace";
		public const string companyId				= "CompanyId";
		public const string login                   = "login";
		public const string loginId					= "loginId";
		public const string operationDescription    = "operationDescription";
		public const string operationId             = "operationId";
		public const string operationDate           = "Data";
		public const string applicationName         = "ProcessName";
		public const string operationType           = "Type";
		public const string WinUser                 = "WinUser";
		public const string Location                = "Location";
	}
}