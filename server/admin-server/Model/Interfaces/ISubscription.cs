namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	public interface ISubscription
	{
		string SubscriptionKey { get; set; }
		string Description { get; set; }
		string ActivationToken { get; set; }
		string Language { get; set; }
		string RegionalSettings { get; set; }
		int MinDBSizeToWarn { get; set; }
        bool UnderMaintenance { get; set; }
        int Ticks { get; set; }
        string VATNr { get; set; }
    }
}