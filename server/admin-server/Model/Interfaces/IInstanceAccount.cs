namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface IInstanceAccount : IAdminModel
	{
        string AccountName { get; }
        string InstanceKey { get; }
    }
}
