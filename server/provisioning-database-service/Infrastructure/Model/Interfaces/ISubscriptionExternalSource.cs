namespace Microarea.ProvisioningDatabase.Infrastructure.Model.Interfaces
{
     //================================================================================
    public interface ISubscriptionExternalSource
	{
		string InstanceKey { get; set; }
		string SubscriptionKey { get; set; }
		string Source { get; set; }
        string Description { get; set; }
		string Provider { get; set; }
		string Server { get; set; }
		string DBName { get; set; }
		string UserId { get; set; }
		string Password { get; set; }
        bool Disabled { get; set; }
		bool UnderMaintenance { get; set; }
		string AdditionalInfo { get; set; }
	}
}
