namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ISubscriptionAccount : IAdminModel
	{
        string AccountName { get; }
        int SubscriptionId { get; }
    }
}
