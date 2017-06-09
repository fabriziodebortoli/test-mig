using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Library
{
    public class UserTokens : ITokens
    {
        string subscriptionToken;
        string securityToken;
        string accountToken;
        public string SubscriptionToken { get; set; }
        public string SecurityToken { get; set; }
        public string AccountToken { get; set; }
    }
}