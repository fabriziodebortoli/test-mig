using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;

namespace Microarea.AdminServer.Controllers.Helpers.Tokens
{
	//================================================================================
	public class BootstrapToken
	{
		public string AccountName;
		public string Language;
		public string RegionalSettings;

		// the nested object needs a property to deserialize it
		public AppSecurityInfo AppSecurity { get; set; }
        // use arrays instead of list because you can't use JsonConvert.DeserializeObject with interface
        public ISecurityToken[] UserTokens;
		public ISubscription[] Subscriptions;
		public IInstance[] Instances;
		public IServerURL[] Urls;
        public IAccountRoles[] Roles;

        //--------------------------------------------------------------------------------
        public BootstrapToken()
		{
			this.AccountName = String.Empty;
			this.Language = String.Empty;
			this.RegionalSettings = String.Empty;
			this.AppSecurity = new AppSecurityInfo();
			this.UserTokens = new SecurityToken[] { };
			this.Instances = new Instance[] { };
			this.Subscriptions = new Subscription[] { };
			this.Urls = new ServerURL[] { };
            this.Roles = new AccountRoles[] { };
        }
    
    }
}