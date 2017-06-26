namespace Microarea.AdminServer.Model.Interfaces
{
	public enum URLType { API, APP, TBLOADER }

	//================================================================================
	interface IServerURL : IAdminModel
	{
		string InstanceKey { get; }
		URLType URLType { get; }
		string AppName { get; set; }
		string URL { get; set; }
	}
}
