using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface ISubscriptionAccountRole : IAdminModel
    {
        string AccountName { get; set; }
        int RoleId { get; set; }
        string SubscriptionKey { get; set; }

    }
}
