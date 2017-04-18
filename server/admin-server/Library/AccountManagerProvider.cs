using Microarea.AdminServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Library
{
    public class AccountManagerProvider : IAccountManagerProvider
    {
        public bool CreateLogin(IAccount userAccount)
        {
            throw new NotImplementedException();
        }

        public bool ValidateLogin(string userName, string password)
        {
            return true;
        }
    }
}
