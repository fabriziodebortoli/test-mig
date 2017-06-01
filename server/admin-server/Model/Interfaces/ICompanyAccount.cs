namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface ICompanyAccount : IAdminModel
	{
        int AccountId { get; }
        int CompanyId { get; }
        bool Admin { get; }
    }
}
