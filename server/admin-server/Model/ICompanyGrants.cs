using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace provisioning_server.Model
{
    interface ICompanyGrants
    {
        string AccountId { get; }
        string Company { get; }
        bool isAdmin { get; }
    }
}
