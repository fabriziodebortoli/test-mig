namespace Microarea.ProvisioningDatabase.Infrastructure.Model.Interfaces
{
     //================================================================================
    public interface ISubscriptionDatabase
	{
		string InstanceKey { get; set; }
		string SubscriptionKey { get; set; }
		string Name { get; set; }
        string Description { get; set; }
		string DBServer { get; set; }
		string DBName { get; set; }
		string DBOwner { get; set; }
		string DBPassword { get; set; }
		bool UseDMS { get; }
		string DMSDBServer { get; set; }
		string DMSDBName { get; set; }
		string DMSDBOwner { get; set; }
		string DMSDBPassword { get; set; }
        bool Disabled { get; }
        string DatabaseCulture { get; }
        bool IsUnicode { get; }
		string Provider { get; }
		bool Test { get; }
		bool UnderMaintenance { get; set; }
	}
}
