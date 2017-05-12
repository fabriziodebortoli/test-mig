using System;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Instance : IInstance
    {
        int instanceId;
        string name = string.Empty;
		string customer = string.Empty;
		bool disabled = false;

		//---------------------------------------------------------------------
		public int InstanceId { get { return this.instanceId; } set { this.instanceId = value; } }
        public string Name { get { return this.name; } set { this.name = value; } }
		public string Customer { get { return this.customer; } set { this.customer = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
	}

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
	}
}
