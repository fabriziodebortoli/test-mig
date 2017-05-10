using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Services.Interfaces
{
    public interface IAdminDataServiceProvider
    {
        IAccount ReadLogin(string userName, string password);
    }
}
