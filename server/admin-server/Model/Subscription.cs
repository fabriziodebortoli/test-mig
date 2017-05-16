using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Subscription : ISubscription
	{
		int subscriptionId;
		string name = string.Empty;
		string activationKey = string.Empty;
		string purchaseId = string.Empty;
		int instanceId;

		//---------------------------------------------------------------------
		public int SubscriptionId { get { return this.subscriptionId; } set { this.subscriptionId = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string ActivationKey { get { return this.activationKey; } set { this.activationKey = value; } }
		public string PurchaseId { get { return this.purchaseId; } set { this.purchaseId = value; } }
		public int InstanceId { get { return this.instanceId; } set { this.instanceId = value; } }

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public Subscription()
		{
		}

		//---------------------------------------------------------------------
		public Subscription(string subscriptionName)
		{
			this.name = subscriptionName;
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		//---------------------------------------------------------------------
		public bool Save()
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
