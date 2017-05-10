namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IDatabaseInfo
    {
        string Database { get; }
        string Server { get; }
        string DBOwner { get; }
        string Password { get; }
    }

    //================================================================================
    interface ICompany
    {
        int CompanyId { get; }
		string Name { get; }
		string Description { get; }
		int SubscriptionId { get; }
        IDatabaseInfo CompanyDatabaseInfo { get; }
		bool UseDMS { get; }
		IDatabaseInfo DMSDatabaseInfo { get; }
        bool Disabled { get; }
        string DatabaseCulture { get; }
        bool IsUnicode { get; }
		string PreferredLanguage { get; }
		string ApplicationLanguage { get; }
		string Provider { get; }
	}
}
