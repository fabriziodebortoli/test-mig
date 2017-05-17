namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	interface ISubscription : IAdminModel
	{
		int SubscriptionId { get; }
		string Name { get; }
		string ActivationKey { get; set; }
		string PurchaseId { get; set; }
		int InstanceId { get; set; }
	}
}
