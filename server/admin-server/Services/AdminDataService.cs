using Microarea.AdminServer.Interfaces;
using provisioning_server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Services.AdminDataService
{
    public class AdminDataService
    {
        IAdminDataServiceProvider iAdminDataProvider;

        public AdminDataService() { }

        public AdminDataService(IAdminDataServiceProvider provider)
        {
            this.iAdminDataProvider = provider;
        }

        IUserAccount GetUserAccount(string userName, string password)
        {
            return this.iAdminDataProvider.ReadLogin(userName, password);
        }
    }
}
