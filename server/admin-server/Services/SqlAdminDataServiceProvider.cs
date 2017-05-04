using Microarea.AdminServer.Services.Interfaces;
using System;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services
{
    public class SqlAdminDataServiceProvider : IAdminDataServiceProvider
    {
        public IUserAccount ReadLogin(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
