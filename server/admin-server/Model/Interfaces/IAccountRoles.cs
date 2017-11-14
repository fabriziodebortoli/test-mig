namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	public interface IAccountRoles
    {
		string RoleName { get; set; }
		string AccountName { get; set; }
        string EntityKey { get; set; }
		string Level { get; set; }
        int Ticks { get; set; }
    }
}
