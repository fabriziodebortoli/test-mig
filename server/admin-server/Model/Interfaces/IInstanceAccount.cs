namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface IInstanceAccount : IAdminModel
	{
        int AccountId { get; }
        int InstanceId { get; }
    }
}
