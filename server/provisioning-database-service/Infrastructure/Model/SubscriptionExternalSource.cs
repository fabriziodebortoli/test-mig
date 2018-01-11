using Microarea.ProvisioningDatabase.Infrastructure.Model.Interfaces;

namespace Microarea.ProvisioningDatabase.Infrastructure.Model
{
    //================================================================================
    public class SubscriptionExternalSource : ISubscriptionExternalSource
	{
		string instanceKey = string.Empty;
		string subscriptionKey = string.Empty;
        string source = string.Empty;
		string description = string.Empty;
		string provider = string.Empty;
		string server = string.Empty;
		string dbName = string.Empty;
		string userId = string.Empty;
		string password = string.Empty;
		bool disabled = false;
		bool underMaintenance = false;
		string additionalInfo = string.Empty;

		//---------------------------------------------------------------------
		public string InstanceKey { get => instanceKey; set => instanceKey = value; }
		public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public string Source { get { return this.source; } set { this.source = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
		public string Provider { get { return this.provider; } set { this.provider = value; } }
		public string Server { get { return this.server; } set { this.server = value; } }
		public string DBName { get { return this.dbName; } set { this.dbName= value; } }
		public string UserId { get { return this.userId; } set { this.userId = value; } }
		public string Password { get { return this.password; } set { this.password = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool UnderMaintenance { get => underMaintenance; set => underMaintenance = value; }
		public string AdditionalInfo { get { return this.additionalInfo; } set { this.additionalInfo = value; } }

		//---------------------------------------------------------------------
		public SubscriptionExternalSource()
		{
		}
	}
}
