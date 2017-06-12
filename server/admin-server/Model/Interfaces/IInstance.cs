namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	interface IInstance : IAdminModel
	{
        int InstanceId { get; }
        string Name { get; }
		string Customer { get; set; }
		bool Disabled { get; set; }
      
    }
}
