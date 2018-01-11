using System;
using System.Data;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System.Collections.Generic;
using Microarea.AdminServer.Libraries;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class SubscriptionExternalSource : ISubscriptionExternalSource
	{
		string instanceKey = string.Empty;
		string subscriptionKey = string.Empty;
        string source = string.Empty;
		string description = string.Empty;
		string provider = string.Empty;
		string server = string.Empty;
		string dbName = string.Empty;
		string userId = string.Empty;
		string password = string.Empty;
		bool disabled = false;
		bool underMaintenance = false;
		string additionalInfo = string.Empty;

		//---------------------------------------------------------------------
		public string InstanceKey { get => instanceKey; set => instanceKey = value; }
		public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public string Source { get { return this.source; } set { this.source = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
		public string Provider { get { return this.provider; } set { this.provider = value; } }
		public string Server { get { return this.server; } set { this.server = value; } }
		public string DBName { get { return this.dbName; } set { this.dbName= value; } }
		public string UserId { get { return this.userId; } set { this.userId = value; } }
		public string Password { get { return this.password; } set { this.password = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool UnderMaintenance { get => underMaintenance; set => underMaintenance = value; }
		public string AdditionalInfo { get { return this.additionalInfo; } set { this.additionalInfo = value; } }

		//---------------------------------------------------------------------
		public SubscriptionExternalSource()
		{
		}

        //---------------------------------------------------------------------
        public OperationResult Save(BurgerData burgerData)
        {
            OperationResult opRes = new OperationResult();

            List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();

			burgerDataParameters.Add(new BurgerDataParameter("@InstanceKey", this.InstanceKey));
			burgerDataParameters.Add(new BurgerDataParameter("@SubscriptionKey", this.SubscriptionKey));
			burgerDataParameters.Add(new BurgerDataParameter("@Source", this.Source));
            burgerDataParameters.Add(new BurgerDataParameter("@Description", this.Description));
			burgerDataParameters.Add(new BurgerDataParameter("@Provider", this.Provider));
			burgerDataParameters.Add(new BurgerDataParameter("@Server", this.Server));
            burgerDataParameters.Add(new BurgerDataParameter("@DBName", this.DBName));
            burgerDataParameters.Add(new BurgerDataParameter("@UserId", this.UserId));
            burgerDataParameters.Add(new BurgerDataParameter("@Password", SecurityManager.EncryptString(this.Password)));
            burgerDataParameters.Add(new BurgerDataParameter("@Disabled", this.Disabled));
			burgerDataParameters.Add(new BurgerDataParameter("@UnderMaintenance", this.UnderMaintenance));
			burgerDataParameters.Add(new BurgerDataParameter("@AdditionalInfo", this.AdditionalInfo));

			BurgerDataParameter keyColumnParameter1 = new BurgerDataParameter("@InstanceKey", this.InstanceKey);
			BurgerDataParameter keyColumnParameter2 = new BurgerDataParameter("@SubscriptionKey", this.SubscriptionKey);
            BurgerDataParameter keyColumnParameter3 = new BurgerDataParameter("@Source", this.Source);

            BurgerDataParameter[] keyParameters = new BurgerDataParameter[] { keyColumnParameter1, keyColumnParameter2, keyColumnParameter3 };

            opRes.Result = burgerData.Save(ModelTables.SubscriptionExternalSources, keyParameters, burgerDataParameters);
            opRes.Content = this;
            return opRes;
        }

        //--------------------------------------------------------------------------------
        public IModelObject Fetch(IDataReader dataReader)
        {
			SubscriptionExternalSource subExternalSource = new SubscriptionExternalSource
			{
				InstanceKey = dataReader["InstanceKey"] as string,
				SubscriptionKey = dataReader["SubscriptionKey"] as string,
				Source = dataReader["Source"] as string,
				Provider = dataReader["Provider"] as string,
				Description = dataReader["Description"] as string,
				Server = dataReader["Server"] as string,
				DBName = dataReader["DBName"] as string,
				UserId = dataReader["UserId"] as string,
				Password = SecurityManager.DecryptString(dataReader["Password"] as string),
				Disabled = (bool)dataReader["Disabled"],
				UnderMaintenance = (bool)dataReader["UnderMaintenance"],
				AdditionalInfo = dataReader["AdditionalInfo"] as string,
			};

			return subExternalSource;
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
