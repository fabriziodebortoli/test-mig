using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;

namespace Microarea.AdminServer.Controllers.Helpers.Tokens
{
	//================================================================================
	public class BootstrapToken
	{
		public string AccountName;
		public bool ProvisioningAdmin;
		public bool CloudAdmin;
		public string PreferredLanguage;
		public string ApplicationLanguage;

		// use arrays instead of list because you can't use JsonConvert.DeserializeObject with interface
		public ISecurityToken[] UserTokens;
		public ISubscription[] Subscriptions;
		public IInstance[] Instances;
		public IServerURL[] Urls;

		//--------------------------------------------------------------------------------
		public BootstrapToken()
		{
			this.AccountName = String.Empty;
			this.ProvisioningAdmin = false;
			this.CloudAdmin = false;
			this.PreferredLanguage = String.Empty;
			this.ApplicationLanguage = String.Empty;
			this.UserTokens = new SecurityToken[] { };
			this.Instances = new Instance[] { };
			this.Subscriptions = new Subscription[] { };
			this.Urls = new ServerURL[] { }; 
		}
	}
}
