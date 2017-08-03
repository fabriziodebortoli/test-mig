namespace Microarea.AdminServer.Model.Interfaces
{
	public enum URLType { API, APP, TBLOADER }

	//================================================================================
	public interface IServerURL
	{
		string InstanceKey { get; set; }
		URLType URLType { get; set; }
		string URL { get; set; }
	}
}
