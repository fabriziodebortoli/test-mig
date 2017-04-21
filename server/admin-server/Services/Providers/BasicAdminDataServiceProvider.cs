using Microarea.AdminServer.Services.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microarea.AdminServer.Interfaces;
using Microarea.AdminServer.Model;

namespace Microarea.AdminServer.Services.Providers
{
    public class BasicAdminDataServiceProvider : IAdminDataServiceProvider
    {
        public IUserAccount ReadLogin(string userName, string password)
        {
            // read the storage, find a login and return a IUserAccount object
            IUserAccount userAccount = new UserAccount();
            userAccount.Name = "Fra";
            return userAccount;
        }
    }
}
