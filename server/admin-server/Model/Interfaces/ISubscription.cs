namespace Microarea.AdminServer.Model.Interfaces
{
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
