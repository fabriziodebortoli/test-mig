namespace Microarea.AdminServer.Model.Interfaces
{
     //================================================================================
    public interface ISubscriptionExternalSource : IModelObject
	{
		string InstanceKey { get; set; }
		string SubscriptionKey { get; set; }
		string Source { get; set; }
        string Description { get; set; }
		string Provider { get; set; }
		string Server { get; set; }
		string Database { get; set; }
		string User { get; set; }
		string Password { get; set; }
        bool Disabled { get; set; }
		bool UnderMaintenance { get; set; }
		string AdditionalInfo { get; set; }
	}
}
