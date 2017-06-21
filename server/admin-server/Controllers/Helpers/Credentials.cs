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
    public class AccountModification
    {
        public string AccountName;
        public string Ticks;
        public AccountModification(string accountName, string ticks)
        {
            this.AccountName = accountName;
        
            this.Ticks = ticks;
        }

    }

}
