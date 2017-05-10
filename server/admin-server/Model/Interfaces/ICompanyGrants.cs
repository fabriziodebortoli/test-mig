namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ICompanyAccounts
    {
        int AccountId { get; }
        int CompanyId { get; }
        bool Admin { get; }
    }
}
