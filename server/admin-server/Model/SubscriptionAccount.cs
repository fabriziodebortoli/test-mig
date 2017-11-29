using System;
using System.Data;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System.Collections.Generic;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class SubscriptionAccount : ISubscriptionAccount, IModelObject
	{
        string accountName;
		string subscriptionKey;
        int ticks = TicksHelper.GetTicks();

        //---------------------------------------------------------------------
        public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
        public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
        public int Ticks { get => ticks; set => ticks = value; }

        //---------------------------------------------------------------------
        public SubscriptionAccount()
		{
			accountName = string.Empty;
			subscriptionKey = string.Empty;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(BurgerData burgerData)
		{
			OperationResult opRes = new OperationResult();

			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			burgerDataParameters.Add(new BurgerDataParameter("@AccountName", this.accountName));
			burgerDataParameters.Add(new BurgerDataParameter("@SubscriptionKey", this.subscriptionKey));
            burgerDataParameters.Add(new BurgerDataParameter("@Ticks", this.ticks));
			BurgerDataParameter accountKeyColumnParameter = new BurgerDataParameter("@AccountName", this.accountName);
			BurgerDataParameter subscriptionKeyColumnParameter = new BurgerDataParameter("@SubscriptionKey", this.subscriptionKey);
			BurgerDataParameter[] keyColumns = new BurgerDataParameter[] {
				accountKeyColumnParameter,
				subscriptionKeyColumnParameter
			};

			opRes.Result = burgerData.Save(ModelTables.SubscriptionAccounts, keyColumns, burgerDataParameters);
			opRes.Content = this;
			return opRes;
		}

		//---------------------------------------------------------------------
		public IModelObject Fetch(IDataReader reader)
		{
			SubscriptionAccount subAccount = new SubscriptionAccount();
			subAccount.accountName = reader["AccountName"] as string;
			subAccount.subscriptionKey = reader["SubscriptionKey"] as string;
            subAccount.Ticks = (int)reader["Ticks"] ;

            return subAccount;
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
