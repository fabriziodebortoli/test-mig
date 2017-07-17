namespace Microarea.AdminServer.Model.Interfaces
{
	public enum URLType { API, APP, TBLOADER }

	//================================================================================
	public interface IServerURL : IAdminModel
	{
		string InstanceKey { get; set; }
		URLType URLType { get; set; }
		string AppName { get; set; }
		string URL { get; set; }
	}
}
