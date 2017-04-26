namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ICompanyGrants
    {
        string AccountId { get; }
        string CompanyId { get; }
        bool IsAdmin { get; }
    }
}
