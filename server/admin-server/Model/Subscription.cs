using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Subscription : ISubscription
	{
		string subscriptionKey;
		string description = string.Empty;
        ActivationToken activationtoken;
		string preferredLanguage = string.Empty;
		string applicationLanguage = string.Empty;
		string instanceKey;
		int minDBSizeToWarn;
		bool existsOnDB;

		//---------------------------------------------------------------------
		public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
        public ActivationToken ActivationToken { get { return this.activationtoken; } set { this.activationtoken = value; } }
		public string InstanceKey { get { return this.instanceKey; } set { this.instanceKey = value; } }
		public string PreferredLanguage { get { return this.preferredLanguage; } set { this.preferredLanguage = value; } }
		public string ApplicationLanguage { get { return this.applicationLanguage; } set { this.applicationLanguage = value; } }
		public int MinDBSizeToWarn { get { return this.minDBSizeToWarn; } set { this.minDBSizeToWarn = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public Subscription()
		{
			subscriptionKey = string.Empty;
			description = string.Empty;
			activationtoken = new ActivationToken(string.Empty);
			instanceKey = string.Empty;
		}

		//---------------------------------------------------------------------
		public Subscription(string subscriptionKey) : this()
		{
			this.subscriptionKey = subscriptionKey;
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
	}
}
