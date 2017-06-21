namespace Microarea.AdminServer.Model.Interfaces
{
     //================================================================================
    interface ICompany : IAdminModel
	{
        int CompanyId { get; }
		string Name { get; }
		string Description { get; set; }
		string SubscriptionKey { get; set; }
		string CompanyDBServer { get; set; }
		string CompanyDBName { get; set; }
		string CompanyDBOwner { get; set; }
		string CompanyDBPassword { get; set; }
		bool UseDMS { get; }
		string DMSDBServer { get; set; }
		string DMSDBName { get; set; }
		string DMSDBOwner { get; set; }
		string DMSDBPassword { get; set; }
        bool Disabled { get; }
        string DatabaseCulture { get; }
        bool IsUnicode { get; }
		string PreferredLanguage { get; }
		string ApplicationLanguage { get; }
		string Provider { get; }
	}
}
