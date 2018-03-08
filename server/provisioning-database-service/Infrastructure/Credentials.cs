
namespace Microarea.ProvisioningDatabase.Infrastructure
{
	//================================================================================
	public class DatabaseCredentials
	{
		public string Provider;
		public string Server;
		public string Database;
		public string Login;
		public string Password;

		//-----------------------------------------------------------------------------	
		public bool Validate()
		{
			return !string.IsNullOrWhiteSpace(Provider) && !string.IsNullOrWhiteSpace(Server) && !string.IsNullOrWhiteSpace(Login);
		}
	}
}
