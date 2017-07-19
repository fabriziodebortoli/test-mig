using Microarea.AdminServer.Library;
using System.Collections.Generic;

namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	public interface ISubscription : IAdminModel
	{
		string SubscriptionKey { get; set; }
		string Description { get; set; }
		ActivationToken ActivationToken { get; set; }
		string PreferredLanguage { get; set; }
		string ApplicationLanguage { get; set; }
		int MinDBSizeToWarn { get; set; }
        bool UnderMaintenance { get; set; }
	}
}