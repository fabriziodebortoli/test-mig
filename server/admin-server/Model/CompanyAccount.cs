using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class CompanyAccount : ICompanyAccount
	{
        public int accountId;
        public int companyId;
        public bool admin;

		//---------------------------------------------------------------------
		public int AccountId { get { return this.accountId; } set { this.accountId = value; } }
        public int CompanyId { get { return this.companyId; } set { this.companyId = value; } }
        public bool Admin { get { return this.admin; } set { this.admin = value; } }
    }
}
