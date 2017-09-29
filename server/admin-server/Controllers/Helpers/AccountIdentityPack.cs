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
        Subscription[] subscriptions;
		Instance[] instances;
        string detailedMessage;

        public bool Result;
        public int MessageCode;
        public Account Account;
        public Subscription[] Subscriptions { get { return this.subscriptions; } set { this.subscriptions = value; } }
		public Instance[] Instances { get { return this.instances; } set { this.instances = value; } }
		public string DetailedMessage { get { return detailedMessage; } set { detailedMessage = value; } }
        public string Message { get { return GwamMessageStrings.GetString(MessageCode); } }

        //-----------------------------------------------------------------------------	
        public AccountIdentityPack()
        {
            this.Result = false;
            this.MessageCode = 1;//undefinded
            this.Account = new Account();
			this.detailedMessage = string.Empty;
            this.subscriptions = new Subscription[]{};
			this.instances = new Instance[]{};
        }
    }
}
