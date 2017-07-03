using Microarea.AdminServer.Model;
using System.Linq;

namespace Microarea.AdminServer.Controllers.Helpers
{

    public class AccountIdentityPack
    {
        Subscription[] subscriptions;

        string detailedMessage = string.Empty;
        public bool Result;
        public int MessageCode;
        public Account Account;
        public Subscription[] Subscriptions { get { return this.subscriptions; } }
        public string DetailedMessage { get { return detailedMessage; } set { detailedMessage = value; } }

        public string Message { get { return GwamMessageStrings.GetString(MessageCode); } }
        //-----------------------------------------------------------------------------	
        public AccountIdentityPack()
        {
            this.Result = false;
            this.MessageCode = 1;//undefinded
            this.Account = new Account();
            this.subscriptions = new Subscription[] { };
        }

        //-----------------------------------------------------------------------------	
        public void AddSubscription(Subscription subscription)
        {
            this.subscriptions.Append<Subscription>(subscription);
        }
    }
}
