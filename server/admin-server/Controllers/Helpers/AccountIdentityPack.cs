using Microarea.AdminServer.Model;
using System.Linq;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	/// <summary>
	/// Used as a transport to get account data from GWAM
	/// </summary>
	public class AccountIdentityPack
    {
		public Account Account;

		Subscription[] subscriptions;
		Instance[] instances;
		AccountRoles[] roles;

        public bool Result;
        public int MessageCode;
		string detailedMessage;

		public string DetailedMessage { get { return detailedMessage; } set { detailedMessage = value; } }
		public string Message { get { return GwamMessageStrings.GetString(MessageCode); } }

		public Instance[] Instances { get { return this.instances; } set { this.instances = value; } }
		public Subscription[] Subscriptions { get { return this.subscriptions; } set { this.subscriptions = value; } }
		public AccountRoles[] Roles { get { return this.roles; } set { this.roles = value; } }

        //-----------------------------------------------------------------------------	
        public AccountIdentityPack()
        {
            this.Result = false;
            this.MessageCode = 1;//undefined
			this.detailedMessage = string.Empty;

			this.Account = new Account();
			this.subscriptions = new Subscription[] { };
			this.instances = new Instance[] { };
			this.roles = new AccountRoles[] { };
        }
    }
}
