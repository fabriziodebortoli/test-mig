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
        //-----------------------------------------------------------------------------	
        public AccountModification(string accountName, long ticks)
        {
            this.AccountName = accountName;
            this.Ticks = ticks;
        }

    }

    //================================================================================
    public class ChangePasswordInfo
    {
        public string AccountName;
        public string Password;
        public string NewPassword;
        //-----------------------------------------------------------------------------	
        public ChangePasswordInfo(string accountName, string newpwd, string oldpwd)
        {
            NewPassword = newpwd;
            AccountName = accountName;
            Password = oldpwd;
        }

    }
}
