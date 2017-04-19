using Microarea.AdminServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace provisioning_server.Services.Interfaces
{
    public interface IAdminDataServiceProvider
    {
        IUserAccount ReadLogin(string userName, string password);
    }
}
