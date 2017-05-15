using Microarea.AdminServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Model.Interfaces
{
    public interface IAdminModel
    {
        IDataProvider DataProvider { get; set; }
        bool Save(string connectionString);

    }
}
