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

		// use arrays instead of list because you can't use JsonConvert.DeserializeObject with interface
		public ISecurityToken[] UserTokens;
		public ISubscription[] Subscriptions;
		public IInstance[] Instances;
		public IServerURL[] Urls;
        public IRoles[] Roles;// valutare se avere solo array di stringhe?

        //--------------------------------------------------------------------------------
        public BootstrapToken()
		{
			this.AccountName = String.Empty;
			this.Language = String.Empty;
			this.RegionalSettings = String.Empty;
			this.UserTokens = new SecurityToken[] { };
			this.Instances = new Instance[] { };
			this.Subscriptions = new Subscription[] { };
			this.Urls = new ServerURL[] { };
            this.Roles = new Roles[] { };
		}
    
    }
}
