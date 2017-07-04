using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
		public UserTokens UserTokens;
		public Subscription[] Subscriptions;
		public List<ServerURL> Urls;

		//--------------------------------------------------------------------------------
		public BootstrapToken()
		{
			this.AccountName = String.Empty;
			this.ProvisioningAdmin = false;
			this.PreferredLanguage = String.Empty;
			this.ApplicationLanguage = String.Empty;
			this.UserTokens = new UserTokens();
			this.Subscriptions = new Subscription[] { };
			this.Urls = new List<ServerURL>();
		}
	}
}
