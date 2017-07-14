﻿using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class SubscriptionAccount : ISubscriptionAccount
	{
        string accountName;
		string subscriptionKey;
		bool existsOnDB = false;

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
        public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }

		//---------------------------------------------------------------------
		public SubscriptionAccount()
		{
		}

		//---------------------------------------------------------------------
		public IAdminModel Load()
		{
			return this.dataProvider.Load(this);
		}

		//---------------------------------------------------------------------
		public OperationResult Save()
		{
			return this.dataProvider.Save(this);
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			return this.dataProvider.Query(qi);
		}
	}
}
