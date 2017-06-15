using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class CompanyAccount : ICompanyAccount
	{
        public string accountName;
		public int companyId;
        public bool admin;

		// data provider
		IDataProvider dataProvider;

		//---------------------------------------------------------------------
		public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
        public int CompanyId { get { return this.companyId; } set { this.companyId = value; } }
        public bool Admin { get { return this.admin; } set { this.admin = value; } }

		//---------------------------------------------------------------------
		public CompanyAccount()
		{

		}

		//---------------------------------------------------------------------
		public void Load()
		{
			this.dataProvider.Load(this);
		}

		//---------------------------------------------------------------------
		public bool Save()
		{
			return this.dataProvider.Save(this);
		}

		//---------------------------------------------------------------------
		public void SetDataProvider(IDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}
	}
}
