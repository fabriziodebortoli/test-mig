namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	interface IInstance
    {
        int InstanceId { get; }
        string Name { get; }
		string Customer { get; }
		bool Disabled { get; }
    }

	//================================================================================
	interface ISubscription
	{
		int SubscriptionId { get; }
		string Name { get; }
		string ActivationKey { get; }
		string PurchaseId { get; }
		int InstanceId { get; }
	}
}
