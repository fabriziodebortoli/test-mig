using Microarea.ProvisioningDatabase.Infrastructure.Model.Interfaces;

namespace Microarea.ProvisioningDatabase.Infrastructure.Model
{
	//================================================================================
	public class SubscriptionDatabase : ISubscriptionDatabase
	{
		string instanceKey = string.Empty;
		string subscriptionKey = string.Empty;
		string name = string.Empty;
		string description = string.Empty;
		string dbServer = string.Empty;
		string dbName = string.Empty;
		string dbOwner = string.Empty;
		string dbPassword = string.Empty;
		bool useDMS = false;
		string dmsDBServer = string.Empty;
		string dmsDBName = string.Empty;
		string dmsDBOwner = string.Empty;
		string dmsDBPassword = string.Empty;
		bool disabled = false;
		string databaseCulture = string.Empty;
		bool isUnicode = false;
		string provider = string.Empty;
		bool test = false;
		bool underMaintenance;

		//---------------------------------------------------------------------
		public string InstanceKey { get => instanceKey; set => instanceKey = value; }
		public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
		public string DBServer { get { return this.dbServer; } set { this.dbServer = value; } }
		public string DBName { get { return this.dbName; } set { this.dbName = value; } }
		public string DBOwner { get { return this.dbOwner; } set { this.dbOwner = value; } }
		public string DBPassword { get { return this.dbPassword; } set { this.dbPassword = value; } }
		public bool UseDMS { get { return this.useDMS; } set { this.useDMS = value; } }
		public string DMSDBServer { get { return this.dmsDBServer; } set { this.dmsDBServer = value; } }
		public string DMSDBName { get { return this.dmsDBName; } set { this.dmsDBName = value; } }
		public string DMSDBOwner { get { return this.dmsDBOwner; } set { this.dmsDBOwner = value; } }
		public string DMSDBPassword { get { return this.dmsDBPassword; } set { this.dmsDBPassword = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public string DatabaseCulture { get { return this.databaseCulture; } set { this.databaseCulture = value; } }
		public bool IsUnicode { get { return this.isUnicode; } set { this.isUnicode = value; } }
		public string Provider { get { return this.provider; } set { this.provider = value; } }
		public bool Test { get { return this.test; } set { this.test = value; } }
		public bool UnderMaintenance { get => underMaintenance; set => underMaintenance = value; }

		//---------------------------------------------------------------------
		public SubscriptionDatabase()
		{
		}

		//---------------------------------------------------------------------
		public SubscriptionDatabase(string SubscriptionDBName)
		{
			this.name = SubscriptionDBName;
		}
	}       
}
