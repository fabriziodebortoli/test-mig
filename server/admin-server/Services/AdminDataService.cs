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

        AdminDataService(IAdminDataServiceProvider provider)
        {
            this.iAdminDataProvider = provider;
        }
    }
}
