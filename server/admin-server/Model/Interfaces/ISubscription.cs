using Microarea.AdminServer.Library;
using System.Collections.Generic;

namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	interface ISubscription : IAdminModel
	{
		string SubscriptionKey { get; }
		string Description { get; }
		ActivationToken ActivationToken { get; set; }
		string PreferredLanguage { get; set; }
		string ApplicationLanguage { get; set; }
		int MinDBSizeToWarn { get; set; }
		string InstanceKey { get; set; }

		List<Subscription> GetSubscriptionsByAccount(string accountName, string instanceKey);
	}
}