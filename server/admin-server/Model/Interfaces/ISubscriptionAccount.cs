namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ISubscriptionAccount
	{
        string AccountName { get; set; }
        string SubscriptionKey { get; set; }
        int Ticks { get; set; }
    }
}
