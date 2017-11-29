using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Subscription : ISubscription, IModelObject
	{
		string subscriptionKey = string.Empty;
		string description = string.Empty;
		string activationToken = string.Empty;
		string language = string.Empty;
		string regionalSettings = string.Empty;
		int minDBSizeToWarn;
        bool underMaintenance;
        int ticks = TicksHelper.GetTicks();
        string vATNr;

        //---------------------------------------------------------------------
        public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
        public string ActivationToken { get { return this.activationToken; } set { this.activationToken = value; } }
		public string Language { get { return this.language; } set { this.language = value; } }
		public string RegionalSettings { get { return this.regionalSettings; } set { this.regionalSettings = value; } }
		public int MinDBSizeToWarn { get { return this.minDBSizeToWarn; } set { this.minDBSizeToWarn = value; } }
        public bool UnderMaintenance { get => underMaintenance; set => underMaintenance = value; }
        public int Ticks { get => ticks; set => ticks = value; }
        public string VATNr { get => vATNr; set => vATNr = value; }

        //---------------------------------------------------------------------
        public Subscription() {}

		//---------------------------------------------------------------------
		public Subscription(string subscriptionKey) : this()
		{
			this.subscriptionKey = subscriptionKey;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(BurgerData burgerData)
		{
			OperationResult opRes = new OperationResult();

			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			burgerDataParameters.Add(new BurgerDataParameter("@SubscriptionKey", this.subscriptionKey));
			burgerDataParameters.Add(new BurgerDataParameter("@Description", this.description));

			// subscriptions object coming from GWAM have activationToken set to null
			// because it's not relevant here.
			// But, anyway the sql parameter cannot be null

			if (String.IsNullOrEmpty(this.activationToken))
			{
				this.activationToken = String.Empty;
			}

			burgerDataParameters.Add(new BurgerDataParameter("@ActivationToken", this.activationToken));
            burgerDataParameters.Add(new BurgerDataParameter("@VATNr", this.vATNr));
            burgerDataParameters.Add(new BurgerDataParameter("@Language", this.language));
			burgerDataParameters.Add(new BurgerDataParameter("@RegionalSettings", this.regionalSettings));
			burgerDataParameters.Add(new BurgerDataParameter("@MinDBSizeToWarn", this.minDBSizeToWarn));
			burgerDataParameters.Add(new BurgerDataParameter("@UnderMaintenance", this.underMaintenance));
            burgerDataParameters.Add(new BurgerDataParameter("@Ticks", this.ticks));

            BurgerDataParameter keyColumnParameter = new BurgerDataParameter("@SubscriptionKey", this.subscriptionKey);

			opRes.Result = burgerData.Save(ModelTables.Subscriptions, keyColumnParameter, burgerDataParameters);
			opRes.Content = this;
			return opRes;
		}

		//---------------------------------------------------------------------
		public IModelObject Fetch(IDataReader reader)
		{
			Subscription subscription = new Subscription();
			subscription.subscriptionKey = reader["SubscriptionKey"] as string;
			subscription.description = reader["Description"] as string;
			subscription.activationToken = reader["ActivationToken"] as string;
            subscription.vATNr = reader["VATNr"] as string;
            subscription.language = reader["Language"] as string;
			subscription.regionalSettings = reader["RegionalSettings"] as string;
			subscription.minDBSizeToWarn = (int)reader["MinDBSizeToWarn"];
			subscription.underMaintenance = (bool)reader["UnderMaintenance"];
            subscription.Ticks = (int)reader["Ticks"];
            return subscription;
		}

		//---------------------------------------------------------------------
		public string GetKey()
		{
			throw new NotImplementedException();
		}

		//----------------------------------------------------------------------
		public OperationResult Delete(BurgerData burgerData)
		{
			throw new NotImplementedException();
		}
	}
}
