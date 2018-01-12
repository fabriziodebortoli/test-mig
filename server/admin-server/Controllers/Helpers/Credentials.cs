using System;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	public class Credentials
	{
		public string AccountName;
		public string Password;
	}

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
			return !string.IsNullOrWhiteSpace(Provider) && !string.IsNullOrWhiteSpace(Server) && !string.IsNullOrWhiteSpace(Database) && !string.IsNullOrWhiteSpace(Login);
		}
	}

	//================================================================================
	public class AccountModification
    {
        public string AccountName;
		public string InstanceKey;
        public int Ticks;
        //-----------------------------------------------------------------------------	
        public AccountModification(string accountName, string instanceKey, int ticks)
        {
            this.AccountName = accountName;
			this.InstanceKey = instanceKey;
            this.Ticks = ticks;
        }
    }
}
