namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ICompanyGrants
    {
        string AccountId { get; }
        string Company { get; }
        bool isAdmin { get; }
    }
}
