using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Company : ICompany
    {
        int companyId = -1;
        string name = string.Empty;
		string description = string.Empty;
		string subscriptionKey;
		string companyDBServer = string.Empty;
		string companyDBName = string.Empty;
		string companyDBOwner = string.Empty;
		string companyDBPassword = string.Empty;
		bool useDMS = false;
		string dmsDBServer = string.Empty;
		string dmsDBName = string.Empty;
		string dmsDBOwner = string.Empty;
		string dmsDBPassword = string.Empty;
		bool disabled = false;
        string databaseCulture = string.Empty;
		bool isUnicode = false;
		string preferredLanguage = string.Empty;
		string applicationLanguage = string.Empty;
		string provider = string.Empty;
		bool existsOnDB = false;

		//---------------------------------------------------------------------
		public int CompanyId { get { return this.companyId; } set { this.companyId = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
		public string SubscriptionKey { get { return this.subscriptionKey; } set { this.subscriptionKey = value; } }
		public string CompanyDBServer { get { return this.companyDBServer; } set { this.companyDBServer = value; } }
		public string CompanyDBName { get { return this.companyDBName; } set { this.companyDBName = value; } }
		public string CompanyDBOwner { get { return this.companyDBOwner; } set { this.companyDBOwner = value; } }
		public string CompanyDBPassword { get { return this.companyDBPassword; } set { this.companyDBPassword = value; } }
		public bool UseDMS { get { return this.useDMS; } set { this.useDMS = value; } }
		public string DMSDBServer { get { return this.dmsDBServer; } set { this.dmsDBServer = value; } }
		public string DMSDBName { get { return this.dmsDBName; } set { this.dmsDBName = value; } }
		public string DMSDBOwner { get { return this.dmsDBOwner; } set { this.dmsDBOwner = value; } }
		public string DMSDBPassword { get { return this.dmsDBPassword; } set { this.dmsDBPassword = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public string DatabaseCulture { get { return this.databaseCulture; } set { this.databaseCulture = value; } }
		public bool IsUnicode { get { return this.isUnicode; } set { this.isUnicode = value; } }
		public string PreferredLanguage { get { return this.preferredLanguage; } set { this.preferredLanguage = value; } }
		public string ApplicationLanguage { get { return this.applicationLanguage; } set { this.applicationLanguage = value; } }
		public string Provider { get { return this.provider; } set { this.provider = value; } }
		public bool ExistsOnDB { get { return this.existsOnDB; } set { this.existsOnDB = value; } }

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public Company()
		{
		}

		//---------------------------------------------------------------------
		public Company(string companyName)
		{
			this.name = companyName;
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
