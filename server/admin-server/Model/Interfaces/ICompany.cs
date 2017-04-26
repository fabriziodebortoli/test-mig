using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Model.Interfaces
{
    interface ICompanyDatabaseInfo
    {
        string Name { get; }
        string Server { get; }
        string DBOwner { get; }
        string Password { get; }

    }
    interface ICompany
    {
        string Id { get; }
        string InstanceId { get; }
        string Company { get; }
        string Description { get; }
        ICompanyDatabaseInfo CompanyDatabaseInfo { get; }
        ICompanyDatabaseInfo DMSDatabaseInfo { get; }
        bool Disabled { get; }
        int DatabaseCulture { get; }
        bool Unicode { get; }
        string Provider { get; }

    }
}
