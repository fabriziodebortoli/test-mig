using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using System.Collections.Generic;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Subscription : ISubscription
	{
		string subscriptionKey = string.Empty;
		string description = string.Empty;
        ActivationToken activationtoken = new ActivationToken(string.Empty);
		string language = string.Empty;
		string regionalSettings = string.Empty;
		int minDBSizeToWarn;
        bool underMaintenance;
        bool existsOnDB;

		//---------------------------------------------------------------------
		public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
        public ActivationToken ActivationToken { get { return this.activationtoken; } set { this.activationtoken = value; } }
		public string Language { get { return this.language; } set { this.language = value; } }
		public string RegionalSettings { get { return this.regionalSettings; } set { this.regionalSettings = value; } }
		public int MinDBSizeToWarn { get { return this.minDBSizeToWarn; } set { this.minDBSizeToWarn = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }
        public bool UnderMaintenance { get => underMaintenance; set => underMaintenance = value; }

        // data provider
        ISubscriptionDataProvider dataProvider;

		//---------------------------------------------------------------------
		public Subscription()
		{
		}

		//---------------------------------------------------------------------
		public Subscription(string subscriptionKey) : this()
		{
			this.subscriptionKey = subscriptionKey;
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = (ISubscriptionDataProvider)dataProvider;
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
		public List<ISubscription> GetSubscriptionsByAccount(string accountName, string instanceKey)
		{
			return this.dataProvider.GetSubscriptionsByAccount(accountName, instanceKey);
		}

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			return this.dataProvider.Query(qi);
		}
	}
}
