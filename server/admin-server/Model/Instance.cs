using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System;
using System.Collections.Generic;
using System.Data;

namespace Microarea.AdminServer.Model
{
	//================================================================================
	public class Instance : IInstance, IModelObject
    {
        string instanceKey;
        string description = string.Empty;
        string origin = string.Empty;
        string tags = string.Empty;
        bool disabled = false;
		bool existsOnDB = false;
        bool underMaintenance;
        DateTime pendingDate;

        //---------------------------------------------------------------------
        public string InstanceKey { get { return this.instanceKey; } set { this.instanceKey = value; } }
        public string Description { get { return this.description; } set { this.description = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }
        public string Origin { get => origin; set => origin = value; }
        public string Tags { get => tags; set => tags = value; }
        public bool UnderMaintenance { get => underMaintenance; set => underMaintenance = value; }
        public DateTime PendingDate { get => pendingDate; set => pendingDate = value; }

        //---------------------------------------------------------------------
        public Instance()
		{
			this.description = String.Empty;
		}

		//---------------------------------------------------------------------
		public Instance(string instanceKey) : this()
		{
			this.instanceKey = instanceKey;
		}

        //---------------------------------------------------------------------
        public OperationResult Save(BurgerData burgerData)
        {
            OperationResult opRes = new OperationResult();

            List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
            burgerDataParameters.Add(new BurgerDataParameter("@InstanceKey", this.instanceKey));
            burgerDataParameters.Add(new BurgerDataParameter("@Description", this.description));
            burgerDataParameters.Add(new BurgerDataParameter("@Disabled", this.disabled));
            burgerDataParameters.Add(new BurgerDataParameter("@Origin", this.origin));
            burgerDataParameters.Add(new BurgerDataParameter("@Tags", this.tags));
            burgerDataParameters.Add(new BurgerDataParameter("@UnderMaintenance", this.underMaintenance));
            burgerDataParameters.Add(new BurgerDataParameter("@PendingDate", this.pendingDate));
            BurgerDataParameter keyColumnParameter = new BurgerDataParameter("@InstanceKey", this.instanceKey);

            opRes.Result = burgerData.Save(ModelTables.Instances, keyColumnParameter, burgerDataParameters);
            return opRes;
        }

        //---------------------------------------------------------------------
        public IModelObject Fetch(IDataReader reader)
        {
            Instance instance = new Instance();
            instance.instanceKey = reader["InstanceKey"] as string;
            instance.description = reader["Description"] as string;
            instance.disabled = (bool)reader["Disabled"];
            instance.origin = reader["Origin"] as string;
            instance.tags = reader["Tags"] as string;
            instance.underMaintenance = (bool)reader["UnderMaintenance"];
            instance.pendingDate = (DateTime)reader["PendingDate"];
            return instance;
        }

        //---------------------------------------------------------------------
        public string GetKey()
        {
            return String.Concat(" ( InstanceKey = '", this.InstanceKey, "' ) ");
        }

  //      //---------------------------------------------------------------------
  //      public List<IServerURL> LoadURLs()
		//{
		//	return ((IInstanceDataProvider)this.dataProvider).LoadURLs(this.instanceKey);
		//}

		////---------------------------------------------------------------------
		//public List<IInstance> GetInstances()
		//{
		//	return ((IInstanceDataProvider)this.dataProvider).GetInstances();
		//}

		////---------------------------------------------------------------------
		//public List<IInstance> GetInstancesBySubscription(string subscriptionKey)
		//{
		//	return ((IInstanceDataProvider)this.dataProvider).GetInstancesBySubscription(subscriptionKey);
		//}

		////---------------------------------------------------------------------
		//public List<IInstance> GetInstancesByAccount(string accountName)
		//{
		//	return ((IInstanceDataProvider)this.dataProvider).GetInstancesByAccount(accountName);
		//}

    }
}
