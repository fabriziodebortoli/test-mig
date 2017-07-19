using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class SubscriptionDatabase : ISubscriptionDatabase
    {
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
		string language = string.Empty;
		string regionalSettings = string.Empty;
		string provider = string.Empty;
		bool test = false;
		bool existsOnDB = false;

		//---------------------------------------------------------------------
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
		public string Language { get { return this.language; } set { this.language = value; } }
		public string RegionalSettings { get { return this.regionalSettings; } set { this.regionalSettings = value; } }
		public string Provider { get { return this.provider; } set { this.provider = value; } }
		public bool Test { get { return this.test; } set { this.test = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public SubscriptionDatabase()
		{
		}

		//---------------------------------------------------------------------
		public SubscriptionDatabase(string SubscriptionDBName)
		{
			this.name = SubscriptionDBName;
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		//---------------------------------------------------------------------
		public OperationResult Save()
		{
			return this.dataProvider.Save(this);
		}

		//---------------------------------------------------------------------
		public IAdminModel Load()
		{
			return this.dataProvider.Load(this);
		}

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			return this.dataProvider.Query(qi);
		}
	}
}
