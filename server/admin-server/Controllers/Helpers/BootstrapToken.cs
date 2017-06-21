using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using System;
using System.Collections.Generic;

namespace Microarea.AdminServer.Controllers.Helpers
{
	public class BootstrapTokenContainer
	{
		BootstrapToken bootstrapToken;
		string message;
		bool result;
		int resultCode;
		DateTime expirationDate;

		public BootstrapToken JWTToken { get => bootstrapToken; set => bootstrapToken = value; }
		public DateTime ExpirationDate { get => expirationDate; set => expirationDate = value; }
		public bool Result { get => result; set => result = value; }
		public string Message { get => message; set => message = value; }
		public int ResultCode { get => resultCode; set => resultCode = value; }

		public BootstrapTokenContainer()
		{
			this.bootstrapToken = new BootstrapToken();
			this.expirationDate = new DateTime();
			this.result = false;
			this.message = String.Empty;
			this.resultCode = -1;
		}
	}

    public class BootstrapToken
    {
        public string AccountName;
        public bool ProvisioningAdmin;
        public string PreferredLanguage;
        public string ApplicationLanguage;
        public UserTokens UserTokens;
        public Subscription[] Subscriptions;
        public List<string> Urls;
        public BootstrapToken() {
			this.UserTokens = new UserTokens();
		}
    }
    
}
