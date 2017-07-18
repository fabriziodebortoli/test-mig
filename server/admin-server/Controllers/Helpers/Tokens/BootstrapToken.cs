using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;

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
		public List<SecurityToken> UserTokens;
		public List<ISubscription> Subscriptions;
		public List<IInstance> Instances;
		public List<IServerURL> Urls;

		//--------------------------------------------------------------------------------
		public BootstrapToken()
		{
			this.AccountName = String.Empty;
			this.ProvisioningAdmin = false;
			this.CloudAdmin = false;
			this.PreferredLanguage = String.Empty;
			this.ApplicationLanguage = String.Empty;
			this.UserTokens = new List<SecurityToken>();
			this.Instances = new List<IInstance>();
			this.Subscriptions = new List<ISubscription>();
			this.Urls = new List<IServerURL>();
		}
	}
}
