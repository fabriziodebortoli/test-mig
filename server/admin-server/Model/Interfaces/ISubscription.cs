using Microarea.AdminServer.Library;

namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	interface ISubscription : IAdminModel
	{
		int SubscriptionId { get; }
		string Name { get; }
        ActivationToken ActivationToken { get; set; }
        string PurchaseId { get; set; }
		int InstanceId { get; set; }
	}
}
