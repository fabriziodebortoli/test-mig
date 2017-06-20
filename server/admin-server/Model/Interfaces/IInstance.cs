namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	interface IInstance : IAdminModel
	{
        string InstanceKey { get; }
        string Description { get; }
		string Customer { get; set; }
		bool Disabled { get; set; }
    }
}
