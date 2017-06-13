using Microarea.AdminServer.Model;
using System.Linq;

namespace Microarea.AdminServer.Controllers.Helpers
{
    public class AccountIdentityPack
    {
		Subscription[] subscriptions;

		public bool Result;
		public string Message;
		public Account Account;
		public Subscription[] Subscriptions { get { return this.subscriptions; } }

		public AccountIdentityPack()
		{
			this.Result = false;
			this.Message = "Empty object";
			this.Account = new Account();
			this.subscriptions = new Subscription[] { };
		}

		public void AddSubscription(Subscription subscription)
		{
			this.subscriptions.Append<Subscription>(subscription);
		}
    }
}
