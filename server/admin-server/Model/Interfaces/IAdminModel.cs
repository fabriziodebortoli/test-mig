using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Model.Interfaces
{
    public interface IAdminModel
    {
        bool Save(string connectionString);
    }
}
