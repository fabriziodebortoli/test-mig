using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AccountManager.Interfaces
{
    public interface IAccountManagerProvider
    {
        bool ValidateLogin(string userName, string password);
    }
}
