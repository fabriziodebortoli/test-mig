using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using System;
using System.Collections.Generic;

namespace Microarea.AdminServer.Controllers.Helpers
{
    public class BootstrapToken
    {
        public bool Result;
        public string AccountName;
        public bool ProvisioningAdmin;
        public string PreferredLanguage;
        public string ApplicationLanguage;
        public UserTokens UserTokens;
        public string Message;
        public Subscription[] Subscriptions;
        public List<string> Urls;
        public BootstrapToken() { }
    }
    
}
