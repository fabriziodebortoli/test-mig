using Microarea.AdminServer.Interfaces;
using Microarea.AdminServer.Services.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Services.AdminDataService
{
    public class AdminDataService
    {
        IAdminDataServiceProvider iAdminDataProvider;

        public AdminDataService(IAdminDataServiceProvider provider)
        {
            this.iAdminDataProvider = provider;
        }

        public IUserAccount GetUserAccount(string userName, string password)
        {
            return this.iAdminDataProvider.ReadLogin(userName, password);
        }
    }
}
