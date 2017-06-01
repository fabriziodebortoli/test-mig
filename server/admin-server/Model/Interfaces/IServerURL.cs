namespace Microarea.AdminServer.Model.Interfaces
{
	public enum URLType { API, APP, TBLOADER }

	//================================================================================
	interface IServerURL : IAdminModel
	{
		int InstanceId { get; }
		URLType URLType { get; }
		string URL { get; set; }
	}
}
