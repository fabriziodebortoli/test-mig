using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Company : ICompany
    {
        int companyId;
        string name = string.Empty;
		string description = string.Empty;
		int subscriptionId;
        IDatabaseInfo companyDatabaseInfo = null;
		bool useDMS = false;
        IDatabaseInfo dmsDatabaseInfo = null;
        bool disabled = false;
        string databaseCulture = string.Empty;
		bool isUnicode = false;
		string preferredLanguage = string.Empty;
		string applicationLanguage = string.Empty;
		string provider = string.Empty;

		//---------------------------------------------------------------------
		public int CompanyId { get { return this.companyId; } set { this.companyId = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
		public int SubscriptionId { get { return this.subscriptionId; } set { this.subscriptionId = value; } }
		public IDatabaseInfo CompanyDatabaseInfo { get { return this.companyDatabaseInfo; } set { this.companyDatabaseInfo = value; } }
		public bool UseDMS { get { return this.useDMS; } set { this.useDMS = value; } }
		public IDatabaseInfo DMSDatabaseInfo { get { return this.dmsDatabaseInfo; } set { this.dmsDatabaseInfo = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public string DatabaseCulture { get { return this.databaseCulture; } set { this.databaseCulture = value; } }
		public bool IsUnicode { get { return this.isUnicode; } set { this.isUnicode = value; } }
		public string PreferredLanguage { get { return this.preferredLanguage; } set { this.preferredLanguage = value; } }
		public string ApplicationLanguage { get { return this.applicationLanguage; } set { this.applicationLanguage = value; } }
		public string Provider { get { return this.provider; } set { this.provider = value; } }
    }
}
