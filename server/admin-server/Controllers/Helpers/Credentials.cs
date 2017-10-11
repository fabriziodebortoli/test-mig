﻿using System;

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
