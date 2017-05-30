namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ISubscriptionAccount : IAdminModel
	{
        int AccountId { get; }
        int SubscriptionId { get; }
    }
}
