namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	interface IInstance : IAdminModel
	{
        string InstanceKey { get; }
        string Description { get; }
		bool Disabled { get; set; }
    }
}
