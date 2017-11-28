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
    public class SubscriptionDatabase : ISubscriptionDatabase
    {
		string instanceKey = string.Empty;
		string subscriptionKey = string.Empty;
        string name = string.Empty;
		string description = string.Empty;
		string dbServer = string.Empty;
		string dbName = string.Empty;
		string dbOwner = string.Empty;
		string dbPassword = string.Empty;
		bool useDMS = false;
		string dmsDBServer = string.Empty;
		string dmsDBName = string.Empty;
		string dmsDBOwner = string.Empty;
		string dmsDBPassword = string.Empty;
		bool disabled = false;
        string databaseCulture = string.Empty;
		bool isUnicode = false;
		string provider = string.Empty;
		bool test = false;
		bool underMaintenance;

		//---------------------------------------------------------------------
		public string InstanceKey { get => instanceKey; set => instanceKey = value; }
		public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
		public string DBServer { get { return this.dbServer; } set { this.dbServer = value; } }
		public string DBName { get { return this.dbName; } set { this.dbName = value; } }
		public string DBOwner { get { return this.dbOwner; } set { this.dbOwner = value; } }
		public string DBPassword { get { return this.dbPassword; } set { this.dbPassword = value; } }
		public bool UseDMS { get { return this.useDMS; } set { this.useDMS = value; } }
		public string DMSDBServer { get { return this.dmsDBServer; } set { this.dmsDBServer = value; } }
		public string DMSDBName { get { return this.dmsDBName; } set { this.dmsDBName = value; } }
		public string DMSDBOwner { get { return this.dmsDBOwner; } set { this.dmsDBOwner = value; } }
		public string DMSDBPassword { get { return this.dmsDBPassword; } set { this.dmsDBPassword = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public string DatabaseCulture { get { return this.databaseCulture; } set { this.databaseCulture = value; } }
		public bool IsUnicode { get { return this.isUnicode; } set { this.isUnicode = value; } }
		public string Provider { get { return this.provider; } set { this.provider = value; } }
		public bool Test { get { return this.test; } set { this.test = value; } }
		public bool UnderMaintenance { get => underMaintenance; set => underMaintenance = value; }

		//---------------------------------------------------------------------
		public SubscriptionDatabase()
		{
		}

		//---------------------------------------------------------------------
		public SubscriptionDatabase(string SubscriptionDBName)
		{
			this.name = SubscriptionDBName;
		}

        //---------------------------------------------------------------------
        public OperationResult Save(BurgerData burgerData)
        {
            OperationResult opRes = new OperationResult();

            List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();

			burgerDataParameters.Add(new BurgerDataParameter("@SubscriptionKey", this.SubscriptionKey));
			burgerDataParameters.Add(new BurgerDataParameter("@Name", this.Name));
            burgerDataParameters.Add(new BurgerDataParameter("@InstanceKey", this.InstanceKey));
            burgerDataParameters.Add(new BurgerDataParameter("@Description", this.Description));
            burgerDataParameters.Add(new BurgerDataParameter("@DBServer", this.DBServer));
            burgerDataParameters.Add(new BurgerDataParameter("@DBName", this.DBName));
            burgerDataParameters.Add(new BurgerDataParameter("@DBOwner", this.DBOwner));
            burgerDataParameters.Add(new BurgerDataParameter("@DBPassword", SecurityManager.EncryptString(this.DBPassword)));
            burgerDataParameters.Add(new BurgerDataParameter("@DatabaseCulture", this.DatabaseCulture));
            burgerDataParameters.Add(new BurgerDataParameter("@Disabled", this.Disabled));
            burgerDataParameters.Add(new BurgerDataParameter("@IsUnicode", this.IsUnicode));
            burgerDataParameters.Add(new BurgerDataParameter("@Provider", this.Provider));
            burgerDataParameters.Add(new BurgerDataParameter("@UseDMS", this.UseDMS));
            burgerDataParameters.Add(new BurgerDataParameter("@DMSDBServer", this.DMSDBServer));
            burgerDataParameters.Add(new BurgerDataParameter("@DMSDBName", this.DMSDBName));
            burgerDataParameters.Add(new BurgerDataParameter("@DMSDBOwner", this.DMSDBOwner));
            burgerDataParameters.Add(new BurgerDataParameter("@DMSDBPassword", SecurityManager.EncryptString( this.DMSDBPassword)));
            burgerDataParameters.Add(new BurgerDataParameter("@Test", this.Test));
			burgerDataParameters.Add(new BurgerDataParameter("@UnderMaintenance", this.UnderMaintenance));

			BurgerDataParameter keyColumnParameter1 = new BurgerDataParameter("@SubscriptionKey", this.SubscriptionKey);
            BurgerDataParameter keyColumnParameter2 = new BurgerDataParameter("@Name", this.Name);
            BurgerDataParameter keyColumnParameter3 = new BurgerDataParameter("@InstanceKey", this.InstanceKey);

            BurgerDataParameter[] keyParameters = new BurgerDataParameter[] { keyColumnParameter1, keyColumnParameter2, keyColumnParameter3 };

            opRes.Result = burgerData.Save(ModelTables.SubscriptionDatabases, keyParameters, burgerDataParameters);
            opRes.Content = this;
            return opRes;
        }

        //--------------------------------------------------------------------------------
        public IModelObject Fetch(IDataReader dataReader)
        {
			SubscriptionDatabase subscriptionDatabase = new SubscriptionDatabase
			{
				SubscriptionKey = dataReader["SubscriptionKey"] as string,
				Name = dataReader["Name"] as string,
				InstanceKey = dataReader["InstanceKey"] as string,
				Description = dataReader["Description"] as string,
				DBServer = dataReader["DBServer"] as string,
				DBName = dataReader["DBName"] as string,
				DBOwner = dataReader["DBOwner"] as string,
				DBPassword = SecurityManager.DecryptString(dataReader["DBPassword"] as string),
				DatabaseCulture = dataReader["DatabaseCulture"] as string,
				Disabled = (bool)dataReader["Disabled"],
				IsUnicode = (bool)dataReader["IsUnicode"],
				Provider = dataReader["Provider"] as string,
				UseDMS = (bool)dataReader["UseDMS"],
				DMSDBServer = dataReader["DMSDBServer"] as string,
				DMSDBName = dataReader["DMSDBName"] as string,
				DMSDBOwner = dataReader["DMSDBOwner"] as string,
				DMSDBPassword = SecurityManager.DecryptString( dataReader["DMSDBPassword"] as string),
				Test = (bool)dataReader["Test"],
				UnderMaintenance = (bool)dataReader["UnderMaintenance"]
			};

			return subscriptionDatabase;
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
