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
        public long Ticks;
        public AccountModification(string accountName, long ticks)
        {
            this.AccountName = accountName;
            this.Ticks = ticks;
        }

    }

}
