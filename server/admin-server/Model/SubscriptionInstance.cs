using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Model
{
    public class SubscriptionInstance : ISubscriptionInstance, IModelObject
	{
		private string instanceKey;
		private string subscriptionKey;
		private int ticks;

		public string InstanceKey { get => instanceKey; set => instanceKey = value; }
		public string SubscriptionKey { get => subscriptionKey; set => subscriptionKey = value; }
		public int Ticks { get => ticks; set => ticks = value; }

		//---------------------------------------------------------------------
		public OperationResult Save(BurgerData burgerData)
		{
			OperationResult opRes = new OperationResult();
			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			BurgerDataParameter subscriptionKeyParameter = new BurgerDataParameter("@SubscriptionKey", this.subscriptionKey);
			BurgerDataParameter instanceKeyParameter = new BurgerDataParameter("@instanceKey", this.instanceKey);
			burgerDataParameters.Add(subscriptionKeyParameter);
			burgerDataParameters.Add(instanceKeyParameter);
			burgerDataParameters.Add(new BurgerDataParameter("@Ticks", this.ticks));
			BurgerDataParameter[] keyParameters = new BurgerDataParameter[] {
				subscriptionKeyParameter,
				instanceKeyParameter
			};
			opRes.Result = burgerData.Save(ModelTables.SubscriptionInstances, keyParameters, burgerDataParameters);

			return opRes;
		}

		//---------------------------------------------------------------------
		public IModelObject Fetch(IDataReader reader)
		{
			SubscriptionInstance subIns = new SubscriptionInstance();
			subIns.instanceKey = reader["InstanceKey"] as string;
			subIns.subscriptionKey = reader["SubscriptionKey"] as string;
			subIns.ticks = (int)reader["Ticks"];
			return subIns;
		}

		//---------------------------------------------------------------------
		public string GetKey()
		{
			throw new System.NotImplementedException();
		}
	}
}
