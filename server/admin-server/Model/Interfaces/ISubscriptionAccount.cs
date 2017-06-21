namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ISubscriptionAccount : IAdminModel
	{
        string AccountName { get; }
        string SubscriptionKey { get; }
    }
}
