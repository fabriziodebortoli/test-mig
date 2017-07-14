namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ISubscriptionAccount : IAdminModel
	{
        string AccountName { get; set; }
        string SubscriptionKey { get; set; }
    }
}
