namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ICompanyAccount
    {
        int AccountId { get; }
        int CompanyId { get; }
        bool Admin { get; }
    }
}
