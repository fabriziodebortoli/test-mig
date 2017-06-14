namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ICompanyAccount : IAdminModel
	{
        string AccountName { get; }
        int CompanyId { get; }
        bool Admin { get; }
    }
}
