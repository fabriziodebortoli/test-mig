using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class CompanyGrants : ICompanyGrants
    {
        public string accountId;
        public string companyId;
        public bool isAdmin;

        public string AccountId { get { return this.accountId; } }
        public string CompanyId { get { return this.companyId; } }
        public bool IsAdmin { get { return this.isAdmin; } }
    }
}
