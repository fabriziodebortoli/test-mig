using System;
using System.Data;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System.Collections.Generic;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class SubscriptionDatabase : ISubscriptionDatabase
    {
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
		string language = string.Empty;
		string regionalSettings = string.Empty;
		string provider = string.Empty;
		bool test = false;

		//---------------------------------------------------------------------
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
		public string Language { get { return this.language; } set { this.language = value; } }
		public string RegionalSettings { get { return this.regionalSettings; } set { this.regionalSettings = value; } }
		public string Provider { get { return this.provider; } set { this.provider = value; } }
		public bool Test { get { return this.test; } set { this.test = value; } }


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
           
            burgerDataParameters.Add(new BurgerDataParameter("@Description", this.Description));
            burgerDataParameters.Add(new BurgerDataParameter("@DBServer", this.DBServer));
            burgerDataParameters.Add(new BurgerDataParameter("@DBName", this.DBName));
            burgerDataParameters.Add(new BurgerDataParameter("@DBOwner", this.DBOwner));
            burgerDataParameters.Add(new BurgerDataParameter("@DBPassword", this.DBPassword));
            burgerDataParameters.Add(new BurgerDataParameter("@DatabaseCulture", this.DatabaseCulture));
            burgerDataParameters.Add(new BurgerDataParameter("@Disabled", this.Disabled));
            burgerDataParameters.Add(new BurgerDataParameter("@IsUnicode", this.IsUnicode));
            burgerDataParameters.Add(new BurgerDataParameter("@Provider", this.Provider));
            burgerDataParameters.Add(new BurgerDataParameter("@UseDMS", this.UseDMS));
            burgerDataParameters.Add(new BurgerDataParameter("@DMSDBServer", this.DMSDBServer));
            burgerDataParameters.Add(new BurgerDataParameter("@DMSDBName", this.DMSDBName));
            burgerDataParameters.Add(new BurgerDataParameter("@DMSDBOwner", this.DMSDBOwner));
            burgerDataParameters.Add(new BurgerDataParameter("@DMSDBPassword", this.DMSDBPassword));
            burgerDataParameters.Add(new BurgerDataParameter("@Test", this.Test));
            burgerDataParameters.Add(new BurgerDataParameter("@Language", this.Language));
            burgerDataParameters.Add(new BurgerDataParameter("@RegionalSettings", this.RegionalSettings));

            BurgerDataParameter keyColumnParameter1 = new BurgerDataParameter("@SubscriptionKey", this.SubscriptionKey);
            BurgerDataParameter keyColumnParameter2 = new BurgerDataParameter("@Name", this.Name);

            BurgerDataParameter[] keyParameters = new BurgerDataParameter[] {
               keyColumnParameter1,keyColumnParameter2
            };

            opRes.Result = burgerData.Save(ModelTables.SubscriptionDatabases, keyParameters, burgerDataParameters);
            opRes.Content = this;
            return opRes;
        }
      

        //--------------------------------------------------------------------------------
        public IModelObject Fetch(IDataReader dataReader)
        {
            SubscriptionDatabase subscriptionDatabase = new SubscriptionDatabase();

            subscriptionDatabase.SubscriptionKey = dataReader["SubscriptionKey"] as string;
            subscriptionDatabase.Name = dataReader["Name"] as string;
            subscriptionDatabase.Description = dataReader["Description"] as string;
            subscriptionDatabase.DBServer = dataReader["DBServer"] as string;
            subscriptionDatabase.DBName = dataReader["DBName"] as string;
            subscriptionDatabase.DBOwner = dataReader["DBOwner"] as string;
            subscriptionDatabase.DBPassword = dataReader["DBPassword"] as string;
            subscriptionDatabase.DatabaseCulture = dataReader["DatabaseCulture"] as string;
            subscriptionDatabase.Disabled = (bool)dataReader["Disabled"];
            subscriptionDatabase.IsUnicode = (bool)dataReader["IsUnicode"];
            subscriptionDatabase.Language = dataReader["RegionalSettings"] as string;
            subscriptionDatabase.RegionalSettings = dataReader["Language"] as string;
            subscriptionDatabase.Provider = dataReader["Provider"] as string;
            subscriptionDatabase.UseDMS = (bool)dataReader["UseDMS"];
            subscriptionDatabase.DMSDBServer = dataReader["DMSDBServer"] as string;
            subscriptionDatabase.DMSDBName = dataReader["DMSDBName"] as string;
            subscriptionDatabase.DMSDBOwner = dataReader["DMSDBOwner"] as string;
            subscriptionDatabase.DMSDBPassword = dataReader["DMSDBPassword"] as string;
            subscriptionDatabase.Test = (bool)dataReader["Test"];
       
            return subscriptionDatabase;
        }

        //---------------------------------------------------------------------
        public string GetKey()
        {
            throw new NotImplementedException();
        }
    }
}
